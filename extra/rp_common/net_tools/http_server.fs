\ Copyright (c) 2023-2026 Travis Bemann
\
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

\ Note that prior to running this code, one must have uploaded the CYW43439
\ firmware, CYW43439 driver, and zeptoIP to one's Raspberry Pi Pico W (this
\ does not work on a Raspberry Pi Pico) as follows:
\ 
\ Execute from the base directory of zeptoforth:
\ 
\ utils/load_cyw43_fw.sh <tty device> <43439A0.bin path> <43439A0_clm.bin path>
\ 
\ 43439A0.bin and 43439A0_clm.bin can be gotten from:
\ https://github.com/tabemann/cyw43-firmware/tree/master/cyw43439-firmware
\ 
\ Then execute from the base directory of zeptoforth:
\ 
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp_common/pico_w_net_ipv4_all.fs
\ 
\ Afterwards, if you had not already installed the CYW43439 firmware, driver,
\ and zeptoIP, make sure to reboot zeptoforth, either by executing:
\
\ reboot
\
\ or by entering control-C at a terminal or Reboot with zeptocom.js.
\
\ Then ensure that extra/rp_common/net_tools/pico_w_ipv4_base_no_handler.fs or
\ extra/rp_common/net_tools/pico_w_ipv6_base_no_handler.h4 is loaded into
\ RAM and configured per the directions therein.
\
\ Once that is done, execute:
\
\ 80 pico-w-net::my-interface @ net-http::start-server
\
\ where 80 is the HTTP port; a different value can be used if one wants to use
\ a different port
\ 
\ You may then connect to the HTTP server started at:
\
\ http://<the reported IPv4 address>:80/

begin-module net-http
    
  oo import
  closure import
  dynamic import
  console import
  net-misc import
  net-consts import
  net-config import
  net import
  systick import
  task import

  \ HTTP method types
  0 constant method-not-set
  1 constant method-get
  2 constant method-head
  3 constant method-post
  4 constant method-put
  5 constant method-delete
  6 constant method-connect
  7 constant method-options
  8 constant method-trace
  9 constant method-patch
  10 constant method-other

  begin-module net-http-internal

    \ Current HTTP server dynamically-scoped variable
    dyn current-http-server
    
    \ The HTTP input buffer size
    512 constant http-buffer-size

    \ THe HTTP protocol types
    0 constant protocol-unsupported
    1 constant protocol-http1.0
    2 constant protocol-http1.1

    \ URI buffer size
    256 constant uri-buffer-size

    \ Output buffer size
    512 constant out-buffer-size

    \ Close delay in ticks
    50000 constant close-delay
    
    \ Timed out connection close delay
    1000 constant timed-out-close-delay

    \ Our server count
    net-config::max-endpoints 1- 1 max constant http-server-count

    \ HTTP task dictionary size
    1024 constant http-task-dict-size

    \ HTTP task stack size
    512 constant http-task-stack-size

    \ HTTP task rstack size
    1024 constant http-task-rstack-size

  end-module> import
    
  \ URI matcher base class
  <object> begin-class <uri-matcher>

    continue-module net-http-internal
      
      \ Next URI matcher
      cell member uri-matcher-next
      
      \ URI matcher xt ( http-method addr bytes -- )
      cell member uri-matcher-xt
      
      \ Fetch next URI matcher
      method uri-matcher-next@ ( self -- next | 0 )
      
      \ Fetch URI matcher xt
      method uri-matcher-xt@ ( self -- xt )

    end-module
    
    \ Abstract method to match
    method match-uri? ( addr bytes self -- match? )

  end-class

  continue-module net-http-internal
  
    \ URI matcher base class implementation
    <uri-matcher> begin-implement

      \ Constructor
      :noname { xt next-matcher self -- }
        self <object>->new
        next-matcher self uri-matcher-next !
        xt self uri-matcher-xt !
      ; define new

      :noname ( self -- next | 0 ) uri-matcher-next @ ; define uri-matcher-next@

      :noname ( self -- xt ) uri-matcher-xt @ ; define uri-matcher-xt@

    end-implement

  end-module
  
  \ Fixed path excluding ? matcher
  <uri-matcher> begin-class <fixed-uri-matcher>

    continue-module net-http-internal
      
      \ URI to match
      2 cells member fixed-uri-matcher-uri
      
    end-module
      
  end-class

  \ Prefix path excluding ? matcher
  <uri-matcher> begin-class <prefix-uri-matcher>

    continue-module net-http-internal

      \ Prefix to match
      2 cells member prefix-uri-matcher-uri

    end-module

  end-class
  
  continue-module net-http-internal
  
    \ Parse a byte
    : parse-byte { addr bytes -- addr' bytes' c escaped? valid? }
      bytes 0= if addr bytes 0 false false exit then
      addr c@ [char] % = if
        bytes 3 < if addr bytes + 0 0 false false exit then
        base @ { saved-base }
        addr 1+ 2 [: hex parse-unsigned ;] try
        saved-base base !
        ?raise
        not if addr bytes + 0 0 false false exit then
        addr 3 + bytes 3 - rot true true
      else
        addr 1+ bytes 1- addr c@ dup [char] + = if drop bl then false true
      then
    ;

    \ Reserved characters
    create reserved-chars
    char ! c, char # c, char $ c, char % c, char & c, char ( c, char ) c,
    char * c, char + c, char , c, char / c, char : c, char ; c, char = c,
    char ? c, char @ c, char [ c, char ] c,
    here cell align, reserved-chars - constant reserved-chars-count

    \ Is a character reserved
    : reserved-char? { c -- reserved? }
      reserved-chars-count 0 ?do
        reserved-chars i + c@ c = if unloop true exit then
      loop
      false
    ;
    
    \ Fixed path excluding ? matcher
    <fixed-uri-matcher> begin-implement

      \ Constructor
      :noname { xt D: fixed-uri next-matcher self -- }
        xt next-matcher self <uri-matcher>->new
        fixed-uri self fixed-uri-matcher-uri 2!
      ; define new

      :noname { addr bytes self -- match? }
        self fixed-uri-matcher-uri 2@ { our-addr our-bytes }
        begin
          bytes 0= our-bytes 0= and if true exit then
          bytes 0> our-bytes 0= and if addr c@ [char] ? = exit then
          bytes 0= our-bytes 0> and if false exit then
          addr bytes parse-byte { c escaped? valid? } to bytes to addr
          our-addr our-bytes parse-byte { c' escaped?' valid?' }
          to our-bytes to our-addr
          valid? not valid?' not or if false exit then
          c c' <> if false exit then
          c reserved-char? escaped? escaped?' xor and if false exit then
        again
      ; define match-uri?

    end-implement

    \ Prefix path excluding ? matcher
    <prefix-uri-matcher> begin-implement

      \ Constructor
      :noname { xt D: prefix-uri next-matcher self -- }
        xt next-matcher self <uri-matcher>->new
        prefix-uri self prefix-uri-matcher-uri 2!
      ; define new

      :noname { addr bytes self -- match? }
        self prefix-uri-matcher-uri 2@ { our-addr our-bytes }
        begin
          bytes 0>= our-bytes 0= and if true exit then
          bytes 0= our-bytes 0> and if false exit then
          addr bytes parse-byte { c escaped? valid? } to bytes to addr
          our-addr our-bytes parse-byte { c' escaped?' valid?' }
          to our-bytes to our-addr
          valid? not valid?' not or if false exit then
          c c' <> if false exit then
          c reserved-char? escaped? escaped?' xor and if false exit then
        again
      ; define match-uri?

    end-implement
    
    \ The URI matchers
    0 value first-uri-matcher
          
    \ HTTP server class
    <object> begin-class <http-server>

      \ HTTP server interface
      cell member http-interface
      
      \ HTTP server endpoint
      cell member http-endpoint

      \ HTTP port
      cell member http-port
      
      \ Our closing state
      cell member closing?

      \ The HTTP buffer
      http-buffer-size cell align member http-buffer

      \ The HTTP buffer offset
      cell member http-buffer-offset

      \ The endpoint RX offset
      cell member endpoint-rx-offset
      
      \ Are we on the first HTTP header
      cell member first-http-header?

      \ HTTP method
      cell member http-method

      \ HTTP protocol
      cell member http-protocol

      \ HTTP corrupt
      cell member http-corrupt?
      
      \ URI buffer
      uri-buffer-size cell align member uri-buffer

      \ URI length
      cell member uri-len
      
      \ Output buffer
      out-buffer-size cell align member out-buffer

      \ Output buffer offset
      cell member out-buffer-offset
      
      \ HTTP start tick
      cell member http-start

      \ HTTP start tick set
      cell member http-start-set?

      \ Is this a new connection
      cell member new-http?
      
      \ EMIT closure
      closure-size cell align member http-emit

      \ KEY closure
      closure-size cell align member http-key

      \ KEY? closure
      closure-size cell align member http-key?

      \ Main server loop
      method http-main ( self -- )

      \ Core of receiving HTTP data
      method recv-data ( self -- )
      
      \ Carry out EMIT
      method do-http-emit ( c self -- )

      \ Carry out KEY
      method do-http-key ( self -- c )

      \ Carry out KEY?
      method do-http-key? ( self -- flag )
      
      \ Find HTTP header newline
      method find-http-newline ( self -- offset | -1 ) 

      \ Get the token end from a header
      method find-token-end ( offset length self -- offset' ) 

      \ Get the next token from a header
      method find-next-token ( offset length self -- offset' | -1 ) 

      \ Remove text after ?
      method remove-extra ( offset length self -- offset' ) 

      \ Parse HTTP method
      method parse-http-method ( D: header-method self -- ) 
      
      \ Parse HTTP protocol
      method parse-http-protocol ( D: protocol self -- ) 

      \ Initialize state
      method init-http-state ( self -- ) 
      
      \ Process the first HTTP header
      method http-first-header ( length self -- ) 

      \ Parse HTTP headers
      method parse-http-headers ( self -- done? ) 

      \ Send output
      method send-output ( self -- ) 

      \ Add text to the output buffer
      method add-output ( addr bytes self -- ) 

      \ Serve an HTTP protocol indicator
      method serve-protocol ( self -- ) 

      \ Serve header fields
      method serve-header-fields ( D: content-type self -- ) 

      \ Serve a corrupt HTTP response
      method serve-corrupt ( self -- ) 

      \ Serve an invalid destination
      method serve-invalid ( self -- ) 

      \ Serve an unsupported method
      method serve-method-not-allowed ( self -- )

      \ Serve an OK destination
      method serve-ok ( D: content-type self -- )
      
      \ Serve a page
      method serve-page ( self -- ) 

      \ Add data to the HTTP buffer
      method add-http-buffer ( in-buffer in-size self -- ) 

      \ Handle HTTP input
      method handle-input ( self -- )
      
      \ Close a connection
      method close-http ( self -- )

    end-class

    <http-server> begin-implement

      \ Construct HTTP server
      :noname { port intf self -- }
        self <object>->new
        intf self http-interface !
        port self http-port !
        false self closing? !
        false self http-corrupt? !
        true self first-http-header? !
        method-other self http-method !
        protocol-unsupported self http-protocol !
        -1 self uri-len !
        0 self http-buffer-offset !
        -1 self endpoint-rx-offset !
        0 self out-buffer-offset !
        0 self http-endpoint !
        0 self http-start !
        false self http-start-set? !
        true self new-http? !
        self self http-emit ['] do-http-emit bind
        self self http-key ['] do-http-key bind
        self self http-key? ['] do-http-key? bind
      ; define new

      \ Carry out EMIT
      :noname { W^ c self -- }
        c 1 self add-output
      ; define do-http-emit

      \ Carry out KEY
      :noname { self -- c }
        begin
          self http-buffer-offset @ 0> if
            self http-buffer c@
            self http-buffer 1+ self http-buffer
            self http-buffer-offset @ 1- move
            -1 self http-buffer-offset +!
            true
          else
            self closing? @ not if
              self recv-data false
            else
              0 true
            then
          then
        until
      ; define do-http-key

      \ Carry out KEY?
      :noname { self -- flag }
        self http-buffer-offset @ 0>
        self http-endpoint @ endpoint-rx-data@ nip
        self endpoint-rx-offset @ 0 max - 0> or
        self http-endpoint @ waiting-rx-data? or
      ; define do-http-key?
      
      \ Find HTTP header newline
      :noname { self -- offset | -1 }
        self http-buffer-offset @ 0 ?do
          self http-buffer i + c@ $0D = if
            i 1+ self http-buffer-offset @ < if
              self http-buffer i 1+ + c@ $0A = if
                i unloop exit
              then
            else
              -1 unloop exit
            then
          then
        loop
        -1
      ; define find-http-newline

      \ Get the token end from a header
      :noname { offset length self -- offset' }
        length offset ?do
          self http-buffer i + c@ bl = if i unloop exit then
        loop
        length
      ; define find-token-end

      \ Get the next token from a header
      :noname { offset length self -- offset' | -1 }
        offset length < if
          length offset ?do
            self http-buffer i + c@ bl <> if i unloop exit then
          loop
        then
        -1
      ; define find-next-token

      \ Remove text after ?
      :noname { offset length self -- offset' }
        length offset ?do
          self http-buffer i + c@ [char] ? = if i unloop exit then
        loop
        offset
      ; define remove-extra

      \ Parse HTTP method
      :noname ( D: header-method self -- )
        { self }
        [: { D: header-method }
          header-method s" GET" equal-case-strings? if
            method-get exit
          then
          header-method s" HEAD" equal-case-strings? if
            method-head exit
          then
          header-method s" POST" equal-case-strings? if
            method-post exit
          then
          header-method s" PUT" equal-case-strings? if
            method-put exit
          then
          header-method s" DELETE" equal-case-strings? if
            method-delete exit
          then
          header-method s" CONNECT" equal-case-strings? if
            method-connect exit
          then
          header-method s" OPTIONS" equal-case-strings? if
            method-options exit
          then
          header-method s" TRACE" equal-case-strings? if
            method-trace exit
          then
          header-method s" PATCH" equal-case-strings? if
            method-patch exit
          then
          method-other
        ;] execute
        self http-method !
      ; define parse-http-method
      
      \ Parse HTTP protocol
      :noname { D: protocol self -- }
        protocol s" HTTP/1.0" equal-case-strings? if
          protocol-http1.0
        else
          protocol s" HTTP/1.1" equal-case-strings? if
            protocol-http1.1
          else
            protocol 5 min s" HTTP/" equal-case-strings? not
            self http-corrupt? !
            protocol-unsupported
          then
        then
        self http-protocol !
      ; define parse-http-protocol

      \ Initialize state
      :noname { self -- }
        false self closing? !
        false self http-corrupt? !
        true self first-http-header? !
        method-other self http-method !
        protocol-unsupported self http-protocol !
        -1 self uri-len !
        0 self http-buffer-offset !
        -1 self endpoint-rx-offset !
        0 self out-buffer-offset !
        true { new-endpoint? }
        self http-endpoint @ ?dup if { endpoint }
          endpoint endpoint-tcp-state@ TCP_LISTEN <> to new-endpoint?
        then
        new-endpoint? if
          self http-port @ self http-interface @ allocate-tcp-listen-endpoint if
            self http-endpoint !
          else
            drop 0 self http-endpoint !
          then
        then
        0 self http-start !
        false self http-start-set? !
        true self new-http? !
      ; define init-http-state

      \ Main loop of handling HTTP connections
      :noname { self -- }
        self current-http-server dyn-no-scope!
        begin
          self http-endpoint @ ?dup if { endpoint }
            self http-start-set? @ if
              self http-start @ close-delay + systick-counter - 0 max timeout !
            else
              no-timeout timeout !
            then
            endpoint self http-interface @ ['] wait-ready-endpoint try
            dup 0= if
              drop
              endpoint endpoint-tcp-state@ { state }
              state TCP_ESTABLISHED = state TCP_CLOSE_WAIT = or if
                self new-http? @ self http-start-set? @ not and if
                  systick-counter self http-start !
                  true self http-start-set? !
                then
                state TCP_CLOSE_WAIT = if true self closing? ! then
                0 self endpoint-rx-offset !
                self recv-data
                self handle-input
              else
                endpoint self http-interface @ endpoint-done
                state TCP_CLOSED = if self close-http then
              then            
            else
              dup ['] x-timed-out = if
                2drop drop
                timed-out-close-delay timeout !
                endpoint self http-interface @ ['] close-tcp-endpoint try
                dup ['] x-timed-out = if 2drop 0 then
                ?raise
                self init-http-state
              else
                ?raise
              then
            then
          else
            self init-http-state
          then
        again
      ; define http-main
      
      \ Process the first HTTP header
      :noname { length self -- }
        false self first-http-header? !
        false self http-corrupt? !
        0 { offset }
        offset length self find-token-end { end-offset }
        self http-buffer offset + end-offset offset - self parse-http-method
        end-offset length self find-next-token to offset
        offset -1 <> if
          offset length self find-token-end to end-offset
          end-offset length self remove-extra { path-end-offset }
          self http-buffer offset + self uri-buffer path-end-offset offset -
          uri-buffer-size min move
          path-end-offset offset - uri-buffer-size min self uri-len !
          end-offset length self find-next-token to offset
          offset -1 <> if
            offset length self find-token-end to end-offset
            self http-buffer offset + end-offset offset - self parse-http-protocol
            end-offset length self find-next-token -1 <> self http-corrupt? !
          else
            true self http-corrupt? !
          then
        else
          -1 self uri-len !
        then
      ; define http-first-header

      \ Parse HTTP headers
      :noname { self -- done? }
        self find-http-newline dup -1 <> if { offset }
          self first-http-header? @ if
            offset self http-first-header
            offset 2 + negate self http-buffer-offset +!
            self http-buffer offset 2 + +
            self http-buffer self http-buffer-offset @ move
          then
          true
        else
          drop false
        then
      ; define parse-http-headers

      \ Send output
      :noname { self -- }
        self http-endpoint @ ?dup if { endpoint }
          self out-buffer self out-buffer-offset @
          endpoint self http-interface @ send-tcp-endpoint
        then
        0 self out-buffer-offset !
      ; define send-output

      \ Add text to the output buffer
      :noname { addr bytes self -- }
        begin bytes 0> while
          out-buffer-size self out-buffer-offset @ - bytes min { bytes-to-send }
          addr self out-buffer self out-buffer-offset @ + bytes-to-send move
          bytes-to-send self out-buffer-offset +!
          self out-buffer-offset @ out-buffer-size = if self send-output then
          bytes-to-send negate +to bytes
          bytes-to-send +to addr
        repeat
      ; define add-output

      \ Serve an HTTP protocol indicator
      :noname { self -- }
        self http-protocol @ case
          protocol-http1.0 of ." HTTP/1.0 " endof
          protocol-http1.1 of ." HTTP/1.1 " endof
          protocol-unsupported of ." HTTP/1.1 " endof
        endcase
      ; define serve-protocol

      \ Serve header fields
      :noname { D: content-type self -- }
        ." Content-Type: " content-type type cr
        ." Connection: close" cr cr
      ; define serve-header-fields

      \ Serve a corrupt HTTP response
      :noname { self -- }
        self serve-protocol
        ." 400 Bad Request" cr
        s" text/html" self serve-header-fields
        ." <html><head><title>400 Bad Request</title></head>"
        ." <body><h1>400 Bad Request</h1></body>"
        ." </html>"
      ; define serve-corrupt

      \ Serve an invalid destination
      :noname { self -- }
        self serve-protocol
        ." 404 Not Found" cr
        s" text/html" self serve-header-fields
        ." <html><head><title>404 Not Found</title></head>"
        ." <body><h1>404 Not Found</h1></body>"
        ." </html>"
      ; define serve-invalid

      \ Serve an unsupported method
      :noname { self -- }
        self serve-protocol
        ." 405 Method Not Allowed" cr
        s" text/html" self serve-header-fields
        ." <html><head><title>405 Method Not Allowed</title></head>"
        ." <body><h1>405 Method Not Allowed</h1></body>"
        ." </html>"
      ; define serve-method-not-allowed
      
      \ Serve an OK destination
      :noname { D: content-type self -- }
        self serve-protocol
        ." 200 OK" cr
        content-type self serve-header-fields
      ; define serve-ok

      \ Attempt to serve a page
      :noname { self -- }
        self self http-key self http-key? [:
          dup http-emit ['] true [: ;] [: { self }
            self uri-len @ -1 = if
              self serve-invalid
            then
            first-uri-matcher { current-uri-matcher }
            begin current-uri-matcher while
              self uri-buffer self uri-len @ current-uri-matcher match-uri? if
                current-uri-matcher uri-matcher-xt@ execute
                exit
              else
                current-uri-matcher uri-matcher-next@ to current-uri-matcher
              then
            repeat
            self serve-invalid
          ;] with-output
        ;] with-input
      ; define serve-page
      
      \ Add data to the HTTP buffer
      :noname { in-buffer in-size self -- }
        in-buffer self http-buffer self http-buffer-offset @ + in-size move
        in-size self http-buffer-offset +!
      ; define add-http-buffer

      \ Receive HTTP data
      :noname { self -- }
        self http-endpoint @ { endpoint }
        self endpoint-rx-offset @ -1 = if
          endpoint if
            endpoint self http-interface @ wait-ready-endpoint
            endpoint endpoint-tcp-state@ { state }
            state TCP_ESTABLISHED =
            state TCP_CLOSE_WAIT = or if
              state TCP_CLOSE_WAIT = if true self closing? ! then
              0 self endpoint-rx-offset !
              true
            else
              endpoint self http-interface @ endpoint-done
              state TCP_CLOSED = if self close-http then
              false
            then
          else
            false
          then
        else
          endpoint if
            endpoint endpoint-tcp-state@ { state }
            state TCP_CLOSE_WAIT = if true self closing? ! then
            state TCP_ESTABLISHED = state TCP_CLOSE_WAIT = or
          else
            -1 self endpoint-rx-offset !
            false
          then
        then
        if
          endpoint endpoint-rx-data@ { addr bytes }
          bytes 0> if false self new-http? ! false self http-start-set? ! then
          bytes self endpoint-rx-offset @ -
          http-buffer-size self http-buffer-offset @ - min { accept-bytes }
          addr self endpoint-rx-offset @ + accept-bytes self add-http-buffer
          accept-bytes self endpoint-rx-offset +!
          self endpoint-rx-offset @ bytes = if
            endpoint self http-interface @ endpoint-done
            -1 self endpoint-rx-offset !
          then
        then
      ; define recv-data
      
      \ Handle HTTP input
      :noname { self -- }
        self parse-http-headers if
          self http-corrupt? @ if
            self serve-corrupt
          else
            self serve-page
          then
          self send-output
          self close-http
        then
      ; define handle-input

      \ Close a connection
      :noname { self -- }
        self http-endpoint @ if
          self endpoint-rx-offset @ -1 <> if
            self http-endpoint @ self http-interface @ endpoint-done
          then
          self http-endpoint @ self http-interface @ close-tcp-endpoint
          self init-http-state
        then
      ; define close-http

    end-implement

    \ Our HTTP servers
    http-server-count <http-server> class-size * buffer: http-servers

    \ Initialize our HTTP servers
    : init-http-servers { port intf -- }
      http-server-count 0 ?do
        port intf
        <http-server> http-servers <http-server> class-size i * + init-object
      loop
      http-server-count 0 ?do
        http-servers <http-server> class-size i * + init-http-state
      loop
      http-server-count 0 ?do
        http-servers <http-server> class-size i * + 1 ['] http-main
        http-task-dict-size http-task-stack-size http-task-rstack-size
        spawn c" http" over task-name! run
      loop
    ;

    \ Store bytes
    : bytes, { addr bytes -- }
      here bytes 0 ?do addr i + c@ c, loop cell align,
    ;
    
  end-module> import

  \ Get whether a connection is closing
  : http-closing? ( -- closing? ) current-http-server dyn@ closing? @ ;

  \ Get the current method
  : http-method@ ( -- method ) current-http-server dyn@ http-method @ ;

  \ Get the current URI
  : http-uri@ ( -- addr bytes )
    current-http-server dyn@ { http-server }
    http-server uri-buffer http-server uri-len @
  ;

  \ Get the current endpoint
  : http-endpoint@ ( -- endpoint ) current-http-server dyn@ http-endpoint @ ;

  \ Get the current interface
  : http-interface@ ( -- interface ) current-http-server dyn@ http-interface @ ;

  \ Serve protocol
  : http-protocol. ( -- ) current-http-server dyn@ serve-protocol ;

  \ Serve header fields
  : http-header-fields. ( D: content-type -- )
    current-http-server dyn@ serve-header-fields
  ;

  \ Serve a corrupt HTTP response
  : http-corrupt. ( -- ) current-http-server dyn@ serve-corrupt ;

  \ Serve an invalid destination
  : http-invalid. ( -- ) current-http-server dyn@ serve-invalid ;

  \ Serve an unsupported method
  : http-method-not-allowed. ( -- )
    current-http-server dyn@ serve-method-not-allowed
  ;

  \ Serve an OK destination
  : http-ok. ( D: content-type -- ) current-http-server dyn@ serve-ok ;

  \ URL-decode a string
  : url-decode { addr bytes dest-addr dest-bytes -- dest-addr' bytes' valid? }
    0 { offset }
    begin
      offset dest-bytes < if
        addr bytes parse-byte if
          drop dest-addr offset + c! to bytes to addr
          1 +to offset
          bytes 0= if dest-addr offset true true else false then
        else
          2drop 2drop dest-addr offset false true
        then
      else
        dest-addr offset bytes 0= true
      then
    until
  ;
  
  \ Register a URI matcher
  : register-uri-matcher ( ... ) { klass addr -- }
    first-uri-matcher klass addr init-object
    addr to first-uri-matcher
  ;
    
  \ Register a fixed URI
  : register-fixed-uri { xt addr bytes -- }
    addr bytes bytes, { stored-addr }
    here { uri-matcher }
    <fixed-uri-matcher> class-size allot
    cell align,
    xt stored-addr bytes <fixed-uri-matcher> uri-matcher register-uri-matcher
  ;

  \ Register a prefix URI
  : register-prefix-uri { xt addr bytes -- }
    addr bytes bytes, { stored-addr }
    here { uri-matcher }
    <prefix-uri-matcher> class-size allot
    cell align,
    xt stored-addr bytes <prefix-uri-matcher> uri-matcher register-uri-matcher
  ;
  
  \ Initialize the test
  : start-server ( port interface -- ) init-http-servers ;

end-module