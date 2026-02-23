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
\ Then ensure that extra/rp_common/net_tools/pico_w_ipv4_base.fs from the base
\ directory of zeptoforth is loaded into RAM and configured per the directions
\ therein.
\
\ Afterwards, to download a file via HTTP, execute:
\
\ s" <URL>" pico-w-net-http::download

begin-module pico-w-net-http-dump

  begin-module pico-w-net-http-dump-internal
    
    oo import
    net-misc import
    frame-process import
    net-consts import
    net-config import
    net import
    net-ipv4 import
    endpoint-process import
    alarm import
    simple-cyw43-net-ipv4 import
    pico-w-cyw43-net-ipv4 import
    pico-w-net import
    lock import
    fat32 import
    
    \ Have we sent the header
    false value sent-header?
    
    \ Our URL buffer size
    512 constant url-size
    
    \ Our URL length
    variable url-len
    
    \ Our URL
    url-size buffer: url-buffer
    
    \ Not a valid URL exception
    : x-invalid-url ( -- ) ." invalid or unsupported URL" cr ;
    
    \ Validate a URL as being HTTP (not HTTPS)
    : validate-url { addr count -- }
      s" http://" { prefix-addr prefix-count }
      count prefix-count > averts x-invalid-url
      addr prefix-count prefix-addr prefix-count equal-strings?
      averts x-invalid-url
      addr count + addr ?do i c@ bl <> averts x-invalid-url loop
    ;
    
    \ Get a URL's domain
    : url-domain { addr count -- addr' count' }
      addr count validate-url
      s" http://" nip { prefix-count }
      prefix-count +to addr
      prefix-count negate +to count
      count 0 ?do
        i net-misc::max-dns-name-len <= averts x-invalid-url
        i addr + c@ case
          [char] / of addr i unloop exit endof
          [char] : of addr i unloop exit endof
        endcase
      loop
      count net-misc::max-dns-name-len <= averts x-invalid-url
      addr count
    ;
    
    \ Default HTTP port
    80 constant default-http-port
    
    \ Parse a port
    : parse-port { addr count -- port }
      addr count parse-unsigned averts x-invalid-url
      dup 65536 < averts x-invalid-url
    ;
    
    \ Get a URL's port
    : url-port { addr count -- port }
      addr count validate-url
      s" http://" nip { prefix-count }
      prefix-count +to addr
      prefix-count negate +to count
      count 0 ?do
        i net-misc::max-dns-name-len <= averts x-invalid-url
        i addr + c@ case
          [char] / of default-http-port unloop exit endof
          [char] : of
            count i 1+ ?do
              i addr + c@ case
                [char] / of
                  addr j 1+ + i j 1+ - parse-port unloop unloop exit
                endof
              endcase
            loop
            addr i 1+ + count i 1+ - parse-port unloop exit
          endof
        endcase
      loop
      count net-misc::max-dns-name-len <= averts x-invalid-url
      default-http-port
    ;
    
    \ Get a URL's path
    : url-path { addr count -- addr' count' }
      addr count validate-url
      s" http://" nip { prefix-count }
      prefix-count +to addr
      prefix-count negate +to count
      count 0 ?do
        i net-misc::max-dns-name-len <= averts x-invalid-url
        i addr + c@ case
          [char] / of addr i + count i - unloop exit endof
        endcase
      loop
      count net-misc::max-dns-name-len <= averts x-invalid-url
      s" /"
    ;

    \ HTTP buffer size
    max-endpoint-in-size 2 * constant http-buf-size

    \ HTTP buffer
    http-buf-size buffer: http-buf

    \ HTTP buffer bytes
    0 value http-buf-bytes

    \ Add data to HTTP buffer
    : add-http-buf-data { addr bytes -- }
      http-buf-size http-buf-bytes - bytes min to bytes
      addr http-buf http-buf-bytes + bytes move
      bytes +to http-buf-bytes
    ;

    \ Got first line
    false value recv-first-line?
    
    \ Receiving content data
    false value recv-content?

    \ Content length
    0 value content-len

    \ Content length found
    false value content-len-found?
    
    \ Search for endline
    : search-endline { -- offset found? }
      http-buf-bytes 0 ?do
        i http-buf + c@ $0D = if
          i http-buf-bytes 1 - < if
            i 1+ http-buf + c@ $0A = if
              i true unloop exit
            then
          then
        then
      loop
      0 false
    ;

    \ Discard line
    : discard-http-buf-data { len -- }
      2 +to len
      http-buf len + http-buf http-buf-bytes len - move
      len negate +to http-buf-bytes
    ;

    \ Test for an HTTP header
    : test-header { addr bytes len -- addr' bytes' found? }
      len bytes >= if
        addr bytes http-buf bytes equal-strings? if
          http-buf bytes + len bytes -  true
        else
          0 0 false
        then
      else
        0 0 false
      then
    ;
    
    \ Parse HTTP header
    : parse-header ( -- found? done? )
      search-endline if { len }
        true
        recv-first-line? if
          cr http-buf len type
          http-buf len s" HTTP/1.0 200 OK" equal-strings?
          http-buf len s" HTTP/1.1 200 OK" equal-strings? or if
            false to recv-first-line?
            false
          else
            cr ." REJECTED"
            true
          then
        else
          len 0= if
            cr cr
            true to recv-content?
          else
            cr http-buf len type
            s" Content-Length: " len test-header if
              base @ { saved-base }
              [:
                10 base !
                parse-unsigned if
                  to content-len true to content-len-found?
                else
                  drop
                then
              ;] try
              saved-base base !
              ?raise
            else
              2drop
            then
          then
          false
        then
        len discard-http-buf-data
      else
        drop false false
      then
    ;

    \ Total offset
    0 value total-offset

    \ Last byte was a CR
    false value last-byte-cr?

    \ Dump-text
    : dump-text { addr bytes -- }
      bytes 0 ?do
        i addr + c@ { c }
        c $0A = if last-byte-cr? if $0A emit else cr then else c emit then
        c $0D = to last-byte-cr?
      loop
    ;
    
    \ Process HTTP data
    : process-data ( -- done? )
      content-len-found? if
        content-len total-offset - http-buf-bytes min 0 max
      else
        http-buf-bytes
      then { bytes }
      http-buf bytes dump-text
      bytes +to total-offset
      0 to http-buf-bytes
      content-len-found? if content-len total-offset <= else false then
    ;
    
    \ Consume HTTP buffer data
    : consume-http-buf-data ( -- done? )
      begin
        recv-content? not if
          parse-header if drop true exit then not
        else
          true
        then
      until
      recv-content? if process-data else false then
    ;
    
    lock-size buffer: connect-lock
    
    0 value current-endpoint
    max-endpoints cells buffer: all-endpoints
    
    : avail-endpoint? ( -- available? )
      max-endpoints 0 ?do
        all-endpoints i cells + @ 0= if true exit then
      loop
      false
    ;
    
    : register-endpoint { endpoint -- }
      max-endpoints 0 ?do
        all-endpoints i cells + @ 0= if
          endpoint all-endpoints i cells + ! exit
        then
      loop
    ;
    
    : unregister-endpoint { endpoint -- }
      max-endpoints 0 ?do
        all-endpoints i cells + @ endpoint = if
          0 all-endpoints i cells + ! exit
        then
      loop
    ;

    : test-endpoint { endpoint -- match }
      max-endpoints 0 ?do
        all-endpoints i cells + @ endpoint = if true exit then
      loop
      false
    ;
    
    <endpoint-handler> begin-class <tcp-http-handler>
    end-class

    <tcp-http-handler> begin-implement
      
      \ Handle a endpoint packet
      :noname { endpoint self -- }
        endpoint [: test-endpoint not ;] connect-lock with-lock
        if exit then
        my-cyw43-net toggle-pico-w-led
        sent-header? not endpoint endpoint-tcp-state@ TCP_ESTABLISHED = and if
          cr ." SENDING HEADER"
          true to sent-header?
          s\" GET " endpoint my-interface @ send-tcp-endpoint
          url-buffer url-len @ url-path
          endpoint my-interface @ send-tcp-endpoint
          s\"  HTTP/1.1\r\n" endpoint my-interface @ send-tcp-endpoint
          s\" Host: " endpoint my-interface @ send-tcp-endpoint
          url-buffer url-len @ url-domain
          endpoint my-interface @ send-tcp-endpoint
          s\" \r\n" endpoint my-interface @ send-tcp-endpoint
          s\" Accept: */*\r\n" endpoint my-interface @ send-tcp-endpoint
          s\" Connection: close\r\n\r\n" endpoint my-interface @ send-tcp-endpoint
        then
        endpoint endpoint-rx-data@ add-http-buf-data
        consume-http-buf-data if
          endpoint my-interface @ close-tcp-endpoint
        then
        endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
        then
        endpoint endpoint-tcp-state@ TCP_LAST_ACK = if
        then
        endpoint endpoint-tcp-state@ TCP_CLOSED = if
          endpoint unregister-endpoint
        then
        endpoint my-interface @ endpoint-done
      ; define handle-endpoint
      
    end-implement
    
    <tcp-http-handler> class-size buffer: my-tcp-http-handler

    \ HTTP initialization flag
    false value http-inited?

    \ HTTP initialization lock
    lock-size buffer: http-init-lock
    : init-http-lock ( -- ) http-init-lock init-lock ;
    initializer init-http-lock
    
    \ Initialize HTTP
    : init-http ( -- )
      all-endpoints net-config::max-endpoints cells 0 fill
      connect-lock init-lock
      <tcp-http-handler> my-tcp-http-handler init-object
      my-tcp-http-handler
      my-cyw43-net net-endpoint-process@ add-endpoint-handler
      true to http-inited?
    ;

    \ Download and dump a file over HTTP
    : download { addr len -- }
      [: http-inited? not if init-http then ;] http-init-lock with-lock
      avail-endpoint? not if
        cr ." NOT READY" exit
      then
      len url-size min to len
      len url-len !
      addr url-buffer len move
      url-buffer url-len @ validate-url
      url-buffer url-len @ url-domain { domain-addr domain-len }
      url-buffer url-len @ url-port { port }
      false to sent-header?
      0 to http-buf-bytes
      true to recv-first-line?
      false to recv-content?
      0 to content-len
      0 to total-offset
      false to last-byte-cr?
      false to content-len-found?
      cr ." DNS LOOKUP"
      domain-addr domain-len my-interface @ resolve-dns-ipv4-addr if { addr }
        my-cyw43-net toggle-pico-w-led
        cr ." RESOLVED: " addr ipv4.
        EPHEMERAL_PORT addr port
        [:
          my-interface @ allocate-tcp-connect-ipv4-endpoint if
            register-endpoint cr ." CONNECTED"
          else
            cr ." NOT CONNECTED" drop
          then
        ;] connect-lock with-lock
      else
        cr ." NOT RESOLVED" drop
      then
    ;

  end-module> import

  \ Wrapper for downloading and dumping a file over HTTP
  : download ( url-addr url-bytes ) download ;

end-module