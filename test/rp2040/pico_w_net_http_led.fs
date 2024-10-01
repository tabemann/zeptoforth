\ Copyright (c) 2023-2024 Travis Bemann
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
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp2040/pico_w_net_all.fs
\ 
\ Afterwards, if you had not already installed the CYW43439 firmware, driver,
\ and zeptoIP, make sure to reboot zeptoforth, either by executing:
\
\ reboot
\
\ or by entering control-C at a terminal or Reboot with zeptocom.js.
\ 
\ Use instructions
\
\ 1. Load this code into RAM on a zeptoforth install where zeptoIP has already
\    been installed using a terminal which supports zeptoforth, e.g. zeptocom.js
\    or e4thcom in noforth mode.
\ 2. Execute: s" <WiFi SSID>" s" <WiFi password>"
\             pico-w-net-http-server::start-server
\
\ At this point the console will report the IPv4 address acquired via DHCP. You
\ may then connect to the HTTP server started at:
\
\ http://<the reported IPv4 address>:80/
\
\ You will be presented with a web page with three buttons on it, to turn on
\ the Raspberry Pi Pico W's LED, to turn it off, and to toggle it.

begin-module pico-w-net-http-server

  oo import
  cyw43-control import
  net-misc import
  frame-process import
  net-consts import
  net-config import
  net import
  endpoint-process import
  alarm import
  lock import
  simple-cyw43-net import
  pico-w-cyw43-net import

  <pico-w-cyw43-net> class-size buffer: my-cyw43-net
  variable my-cyw43-control
  variable my-interface

  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  \ Server port
  80 constant server-port
  
  \ The HTTP input buffer size
  1024 constant http-buffer-size

  \ HTTP method types
  0 constant method-not-set
  1 constant method-get
  2 constant method-post
  3 constant method-other
  
  \ The HTTP destination types
  0 constant dest-invalid
  1 constant dest-home
  2 constant dest-led-on
  3 constant dest-led-off
  4 constant dest-toggle-led

  \ THe HTTP protocol types
  0 constant protocol-unsupported
  1 constant protocol-http1.0
  2 constant protocol-http1.1

  \ Output buffer size
  1024 constant out-buffer-size

  \ Close delay in ticks
  50000 constant close-delay
  
  \ Timed out connection close delay
  1000 constant timed-out-close-delay
    
  \ HTTP server class
  <object> begin-class <http-server>
    
    \ HTTP server lock
    lock-size member http-lock
    
    \ HTTP server endpoint
    cell member http-endpoint
    
    \ Our close alarm
    alarm-size member my-close-alarm
    
    \ Our closing state
    cell member closing?
    
    \ The HTTP buffer
    http-buffer-size cell align member http-buffer

    \ The HTTP buffer offset
    cell member http-buffer-offset

    \ Are we on the first HTTP header
    cell member first-http-header?

    \ HTTP method
    cell member http-method

    \ HTTP destination
    cell member http-dest

    \ HTTP protocol
    cell member http-protocol

    \ HTTP corrupt
    cell member http-corrupt?

    \ Output buffer
    out-buffer-size cell align member out-buffer

    \ Output buffer offset
    cell member out-buffer-offset
    
    \ HTTP timeout
    cell member http-timeout

    \ HTTP timeout set
    cell member http-timeout-set?

    \ Match HTTP server
    method match-http-server ( endpoint self -- match? ) 

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
    
    \ Parse HTTP destination
    method parse-http-dest ( D: dest self -- ) 
    
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
    method serve-header-fields ( self -- ) 

    \ Serve a corrupt HTTP response
    method serve-corrupt ( self -- ) 

    \ Serve an invalid destination
    method serve-invalid ( self -- ) 

    \ Serve an unsupported method
    method serve-method-not-allowed ( self -- ) 
    
    \ Serve an OK destination
    method serve-ok ( self -- ) 

    \ Generate a button with a label
    method serve-button ( D: label D: path self -- ) 

    \ Serve HTML content
    method serve-home-content ( self -- ) 

    \ Serve the home page
    method serve-home ( self -- ) 

    \ Serve the LED on page
    method serve-led-on ( self -- ) 

    \ Serve the LED off page
    method serve-led-off ( self -- ) 

    \ Serve the toggle LED page
    method serve-toggle-led ( self -- ) 

    \ Add data to the HTTP buffer
    method add-http-buffer ( in-buffer in-size self -- ) 

    \ Handle HTTP input
    method handle-http-input ( in-buffer in-size self -- )
    
    \ Close a connection
    method close-http ( self -- )

    \ Force a connection to close on timeout
    method force-close-http ( self -- )

  end-class

  <http-server> begin-implement

    \ Construct HTTP server
    :noname { self -- }
      self <object>->new
      false self closing? !
      false self http-corrupt? !
      true self first-http-header? !
      method-other self http-method !
      dest-invalid self http-dest !
      protocol-unsupported self http-protocol !
      0 self http-buffer-offset !
      0 self out-buffer-offset !
      0 self http-endpoint !
      self http-lock init-lock
      0 self http-timeout !
      false self http-timeout-set? !
    ; define new

    \ Match HTTP server
    :noname ( endpoint self -- match? )
      http-endpoint @ =
    ; define match-http-server
    
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
    :noname { D: header-method self -- }
      header-method s" GET" equal-strings? if
        method-get
      else
        header-method s" POST" equal-strings? if
          method-post
        else
          method-other
        then
      then
      self http-method !
    ; define parse-http-method
    
    \ Parse HTTP destination
    :noname { D: dest self -- }
      dest s" /" equal-strings? if
        dest-home
      else
        dest s" /led-on" equal-strings? if
          dest-led-on
        else
          dest s" /led-off" equal-strings? if
            dest-led-off
          else
            dest s" /toggle-led" equal-strings? if
              dest-toggle-led
            else
              dest-invalid
            then
          then
        then
      then
      self http-dest !
    ; define parse-http-dest
    
    \ Parse HTTP protocol
    :noname { D: protocol self -- }
      protocol s" HTTP/1.0" equal-strings? if
        protocol-http1.0
      else
        protocol s" HTTP/1.1" equal-strings? if
          protocol-http1.1
        else
          protocol 5 min s" HTTP/" equal-strings? not self http-corrupt? !
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
      dest-invalid self http-dest !
      protocol-unsupported self http-protocol !
      0 self http-buffer-offset !
      0 self out-buffer-offset !
      true { new-endpoint? }
      self http-endpoint @ ?dup if { endpoint }
        endpoint endpoint-tcp-state@ TCP_LISTEN <> to new-endpoint?
      then
      new-endpoint? if
        server-port my-interface @ allocate-tcp-listen-endpoint if
          self http-endpoint !
        else
          drop 0 self http-endpoint !
        then
      then
      0 self http-timeout !
      false self http-timeout-set? !
    ; define init-http-state
    
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
        self http-buffer offset + path-end-offset offset - self parse-http-dest
        end-offset length self find-next-token to offset
        offset -1 <> if
          offset length self find-token-end to end-offset
          self http-buffer offset + end-offset offset - self parse-http-protocol
          end-offset length self find-next-token -1 <> self http-corrupt? !
        else
          true self http-corrupt? !
        then
      else
        dest-invalid self http-dest !
      then
    ; define http-first-header

    \ Parse HTTP headers
    :noname { self -- done? }
      begin
        self find-http-newline dup -1 <> if { offset }
          self first-http-header? @ if
            offset self http-first-header self http-corrupt? @ if
              true true
            else
              false
            then
          else
            offset 0= if true true else false then
          then
          offset 2 + negate self http-buffer-offset +!
          self http-buffer offset 2 + +
          self http-buffer self http-buffer-offset @ move
        else
          drop false true
        then
      until
    ; define parse-http-headers

    \ Send output
    :noname { self -- }
      self http-endpoint @ ?dup if { endpoint }
        self out-buffer self out-buffer-offset @
        endpoint my-interface @ send-tcp-endpoint
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
        protocol-http1.0 of s" HTTP/1.0 " endof
        protocol-http1.1 of s" HTTP/1.1 " endof
        protocol-unsupported of s" HTTP/1.1 " endof
      endcase
      self add-output
    ; define serve-protocol

    \ Serve header fields
    :noname { self -- }
      s\" Content-Type: text/html\r\n" self add-output
      s\" Connection: close\r\n\r\n" self add-output
    ; define serve-header-fields

    \ Serve a corrupt HTTP response
    :noname { self -- }
      self serve-protocol
      s\" 400 Bad Request\r\n" self add-output
      self serve-header-fields
      s" <html><head><title>400 Bad Request</title></head>"
      self add-output
      s" <body><h1>400 Bad Request</h1></body>" self add-output
      s" </html>" self add-output
    ; define serve-corrupt

    \ Serve an invalid destination
    :noname { self -- }
      self serve-protocol
      s\" 404 Not Found\r\n" self add-output
      self serve-header-fields
      s" <html><head><title>404 Not Found</title></head>"
      self add-output
      s" <body><h1>404 Not Found</h1></body>" self add-output
      s" </html>" self add-output
    ; define serve-invalid

    \ Serve an unsupported method
    :noname { self -- }
      self serve-protocol
      s\" 405 Method Not Allowed\r\n" self add-output
      self serve-header-fields
      s" <html><head><title>405 Method Not Allowed</title></head>"
      self add-output
      s" <body><h1>405 Method Not Allowed</h1></body>" self add-output
      s" </html>" self add-output
    ; define serve-method-not-allowed
    
    \ Serve an OK destination
    :noname { self -- }
      self serve-protocol
      s\" 200 OK\r\n" self add-output
      self serve-header-fields
    ; define serve-ok

    \ Generate a button with a label
    :noname { D: label D: path self -- }
      s\" <form method=\"post\" action=\"" self add-output
      path self add-output
      s\" \"><button type=\"submit\">" self add-output
      label self add-output
      s" </button></form>" self add-output
    ; define serve-button

    \ Serve HTML content
    :noname { self -- }
      s" <html><head><title>Control the LED</title></head>" self add-output
      s" <body>" self add-output
      s" LED On" s" led-on" self serve-button
      s" LED Off" s" led-off" self serve-button
      s" Toggle LED" s" toggle-led" self serve-button
      s" </body></html>" self add-output
    ; define serve-home-content

    \ Serve the home page
    :noname { self -- }
      self serve-ok
      self serve-home-content
    ; define serve-home

    \ Serve the LED on page
    :noname { self -- }
      self http-method @ method-post = if
        true my-cyw43-net pico-w-led!
        self serve-home
      else
        self serve-method-not-allowed
      then
    ; define serve-led-on

    \ Serve the LED off page
    :noname { self -- }
      self http-method @ method-post = if
        false my-cyw43-net pico-w-led!
        self serve-home
      else
        self serve-method-not-allowed
      then
    ; define serve-led-off

    \ Serve the toggle LED page
    :noname { self -- }
      self http-method @ method-post = if
        my-cyw43-net toggle-pico-w-led
        self serve-home
      else
        self serve-method-not-allowed
      then
    ; define serve-toggle-led

    \ Add data to the HTTP buffer
    :noname { in-buffer in-size self -- }
      \    in-buffer in-size type
      in-size http-buffer-size self http-buffer-offset @ - min { accept-size }
      in-buffer self http-buffer self http-buffer-offset @ + accept-size move
      accept-size self http-buffer-offset +!
    ; define add-http-buffer

    \ Handle HTTP input
    :noname { in-buffer in-size self -- closed? }
\      self schedule-close
      in-buffer in-size self add-http-buffer
      self parse-http-headers if
        self http-corrupt? @ if
          self serve-corrupt
        else
          self http-dest @ case
            dest-invalid of self serve-invalid endof
            dest-home of self serve-home endof
            dest-led-on of self serve-led-on endof
            dest-led-off of self serve-led-off endof
            dest-toggle-led of self serve-toggle-led  endof
          endcase
        then
        self send-output
        self close-http
      then
    ; define handle-http-input

    \ Close a connection
    :noname { self -- }
      self http-endpoint @ if
        self http-endpoint @ my-interface @ close-tcp-endpoint
        self init-http-state
      then
    ; define close-http
    
    \ Force a connection to close on timeout
    :noname { self -- }
      self http-endpoint @ if
        self [:
          [: http-endpoint @ my-interface @ close-tcp-endpoint ;]
          timed-out-close-delay task::with-timeout
        ;] try
        dup ['] task::x-timed-out = if 2drop 0 then
        ?raise
        self init-http-state
      then
    ; define force-close-http

  end-implement

  \ Our HTTP servers
  net-config::max-endpoints <http-server> class-size * buffer: http-servers

  \ Initialize our HTTP servers
  : init-http-servers ( -- )
    net-config::max-endpoints 0 ?do
      <http-server> http-servers <http-server> class-size i * + init-object
    loop
    net-config::max-endpoints 0 ?do
      http-servers <http-server> class-size i * + init-http-state
    loop
  ;

  \ Find our HTTP server
  : find-http-server { endpoint -- server found? }
    net-config::max-endpoints 0 ?do
      http-servers <http-server> class-size i * + { http-server }
      endpoint http-server match-http-server if http-server true exit then
    loop
    0 false
  ;
  
  <endpoint-handler> begin-class <tcp-echo-handler>
  end-class

  <tcp-echo-handler> begin-implement

    \ Get timeout
    :noname { self -- timeout }
      task::no-timeout { current-timeout }
      systick::systick-counter { current-systick }
      net-config::max-endpoints 0 ?do
        http-servers <http-server> class-size i * + { http-server }
        http-server http-timeout-set? @ if
          http-server http-endpoint @ ?dup if { endpoint }
            endpoint endpoint-tcp-state@ { state }
            state TCP_ESTABLISHED =
            state TCP_CLOSE_WAIT = or
            state TCP_CLOSED = or if
              http-server http-timeout @ current-systick - 0 max
              current-timeout task::no-timeout <> if
                current-timeout min to current-timeout
              else
                to current-timeout
              then
            then
          then
        then
      loop
      current-timeout
    ; define handler-timeout@
    
    \ Handle timeout
    :noname { self -- }
      systick::systick-counter { current-systick }
      net-config::max-endpoints 0 ?do
        http-servers <http-server> class-size i * + { http-server }
        http-server http-timeout-set? @ if
          http-server http-timeout @ current-systick - 0<= if
            http-server http-endpoint @ ?dup if { endpoint }
              endpoint endpoint-tcp-state@ { state }
              state TCP_ESTABLISHED =
              state TCP_CLOSE_WAIT = or
              state TCP_CLOSED = or if
                cr ." Force-closing connection... "
                http-server force-close-http
              then
            then
          then
        then
      loop
    ; define handle-timeout
    
    \ Handle an HTTP request
    :noname { endpoint self -- }
      endpoint find-http-server not if
        endpoint my-interface @ endpoint-done drop exit
      then { http-server }
      endpoint self http-server [: { endpoint self http-server }
        endpoint endpoint-tcp-state@ { state }
        http-server http-timeout-set? @ not
        state TCP_CLOSED = not and
        state TCP_LISTEN = not and if
          systick::systick-counter close-delay + http-server http-timeout !
          true http-server http-timeout-set? !
        then
        state TCP_ESTABLISHED =
        state TCP_CLOSE_WAIT = or if
          endpoint endpoint-rx-data@ http-server handle-http-input
        else
          state TCP_CLOSED = if
            http-server close-http
          then
        then
        endpoint my-interface @ endpoint-done
      ;] http-server http-lock with-lock
    ; define handle-endpoint
  
  end-implement
  
  <tcp-echo-handler> class-size buffer: my-tcp-echo-handler
  
  \ Initialize the test
  : start-server { D: ssid D: pass -- }
    1024 256 1024 0 init-default-alarm-task
    pio-addr sm-index pio-instance <pico-w-cyw43-net> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
\    <tcp-echo-handler> my-tcp-echo-handler init-object
\    my-tcp-echo-handler
\    my-cyw43-net net-endpoint-process@ add-endpoint-handler
    cyw43-consts::PM_AGGRESSIVE my-cyw43-control @ cyw43-power-management!
    begin ssid pass my-cyw43-control @ join-cyw43-wpa2 nip until
    my-cyw43-net run-net-process
    cr ." Discovering IPv4 address..."
    my-interface @ discover-ipv4-addr
    my-interface @ intf-ipv4-addr@ cr ." IPv4 address: " ipv4.
    my-interface @ intf-ipv4-netmask@ cr ." IPv4 netmask: " ipv4.
    my-interface @ gateway-ipv4-addr@ cr ." Gateway IPv4 address: " ipv4.
    my-interface @ dns-server-ipv4-addr@ cr ." DNS server IPv4 address: " ipv4.
    my-cyw43-net toggle-pico-w-led
    <tcp-echo-handler> my-tcp-echo-handler init-object
    my-tcp-echo-handler
    my-cyw43-net net-endpoint-process@ add-endpoint-handler
    init-http-servers
  ;

end-module