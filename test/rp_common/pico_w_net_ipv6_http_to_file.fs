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
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp_common/pico_w_net_ipv6_all.fs
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
\ 2. Execute: s" <WiFi SSID>" s" <WiFi password>" pico-w-net-http::init-wifi
\ 3. Execute: s" <URL>" s" <PATH>" pico-w-net-http::download-file
\
\ If all goes well, then the contents of the reply from a GET request to
\ www.google.com will be echoed on the console.

begin-module pico-w-net-http
  
  oo import
  cyw43-control import
  net-misc import
  frame-process import
  net-consts import
  net-config import
  net import
  net-ipv6 import
  endpoint-process import
  alarm import
  simple-cyw43-net-ipv6 import
  pico-w-cyw43-net-ipv6 import
  lock import
  fat32 import

  <pico-w-cyw43-net-ipv6> class-size buffer: my-cyw43-net
  variable my-cyw43-control
  variable my-interface

  -1 constant sm-index
  -1 constant pio-instance
  
  alarm-size buffer: my-close-alarm
  
  \ Have we sent the header
  false value sent-header?
  
  \ Are we closing?
  false value closing?
    
  \ Our MAC address
  default-mac-addr 2constant my-mac-addr
  
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
  net-config::max-endpoint-in-size 2 * constant http-buf-size

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

  \ Destination file
  <fat32-file> class-size buffer: dest-file
  
  \ Search for endline
  : search-endline { -- offset found? }
    http-buf-bytes 0 ?do
      i http-buf + c@ $0D = if
        i http-buf-bytes 2 - < if
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
  
  \ Consume HTTP buffer data
  : consume-http-buf-data { -- done? }
    recv-content? if
      content-len-found? if
        content-len dest-file tell-file - http-buf-bytes min 0 max
      else
        http-buf-bytes
      then { bytes }
      0 { offset }
      cr ." Downloaded " bytes . ." bytes"
      begin offset bytes < while
        http-buf offset + bytes offset - dest-file write-file +to offset
      repeat
      0 to http-buf-bytes
      content-len-found? if content-len dest-file tell-file <= else false then
    else
      search-endline if { len }
        recv-first-line? if
          cr http-buf len type
          http-buf len s" HTTP/1.0 200 OK" equal-strings?
          http-buf len s" HTTP/1.1 200 OK" equal-strings? or if
            false to recv-first-line?
            false
          else
            true
          then
        else
          len 0= if
            true to recv-content?
          else
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
      then
    then
  ;

  false value closed-file?

  lock-size buffer: finalize-lock
  
  \ Close the file if it is not already closed
  : finalize-file ( -- )
    [:
      closed-file? not if
        true to closed-file?
        dest-file truncate-file
        dest-file close-file
        dest-file destroy
      then
    ;] finalize-lock with-lock
  ;
  
  <endpoint-handler> begin-class <tcp-http-handler>
  end-class

  <tcp-http-handler> begin-implement
  
    \ Handle a endpoint packet
    :noname { endpoint self -- }
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
        finalize-file
      then
      endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
        cr ." CLOSING CONNECTION"
        closing? not if
          true to closing?
          500 0 endpoint [:
            drop my-interface @ close-tcp-endpoint
            finalize-file
          ;] my-close-alarm set-alarm-delay-default
        then
      then
      endpoint endpoint-tcp-state@ TCP_LAST_ACK = if
        cr ." WAITING FOR LAST ACK"
      then
      endpoint endpoint-tcp-state@ TCP_CLOSED = if
        cr ." CONNECTION CLOSED"
        finalize-file
      then
      endpoint my-interface @ endpoint-done
    ; define handle-endpoint
  
  end-implement
  
  <tcp-http-handler> class-size buffer: my-tcp-http-handler
  
  \ Initialize wifi
  : init-wifi { D: ssid D: pass -- }
    1024 256 1024 0 init-default-alarm-task
    sm-index pio-instance <pico-w-cyw43-net-ipv6> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    <tcp-http-handler> my-tcp-http-handler init-object
    my-tcp-http-handler
    my-cyw43-net net-endpoint-process@ add-endpoint-handler
    cyw43-consts::PM_AGGRESSIVE my-cyw43-control @ cyw43-power-management!
    begin ssid pass my-cyw43-control @ join-cyw43-wpa2 nip until
    my-cyw43-net run-net-process
    cr ." Autoconfiguring link-local IPv6 address... "
    my-interface @ autoconfigure-link-local-ipv6-addr if
      ." Success"
    else
      ." Failure"
    then
    cr ." Discovering IPv6 router..."
    my-interface @ discover-ipv6-router
    cr ." Discovering IPv6 address..."
    my-interface @ discover-ipv6-addr if
      ." Success"
    else
      ." Failure"
    then
    my-interface @ intf-ipv6-addr@ cr ." Primary IPv6 address: " ipv6.
    my-interface @ intf-link-local-ipv6-addr@
    cr ." Link-local IPv6 address: " ipv6.
    my-interface @ intf-slaac-ipv6-addr@ cr ." SLAAC IPv6 address: " ipv6.
    my-interface @ intf-dhcpv6-ipv6-addr@ cr ." DHCPv6 IPv6 address: " ipv6.
    my-interface @ intf-ipv6-prefix@ cr ." IPv6 prefix: " ipv6.
    my-interface @ intf-autonomous@ cr ." Autonomous: "
    if ." yes" else ." no" then
    my-interface @ intf-ipv6-prefix-len@ cr ." IPv6 prefix length: " .
    my-interface @ gateway-ipv6-addr@ cr ." Gateway IPv6 address: " ipv6.
    cr ." Discovering IPv6 DNS server..."
    my-interface @ discover-dns-ipv6-addr
    my-interface @ dns-server-ipv6-addr@ cr ." DNS server IPv6 address: " ipv6.
    my-cyw43-net toggle-pico-w-led
    true my-interface @ mdns-enabled!
    finalize-lock init-lock
  ;

  \ Download a file over HTTP
  : download-file { addr len path-addr path-len -- }
    len url-size min to len
    len url-len !
    addr url-buffer len move
    url-buffer url-len @ validate-url
    url-buffer url-len @ url-domain { domain-addr domain-len }
    url-buffer url-len @ url-port { port }
    path-addr path-len fat32-tools::current-fs@ root-path-exists? if
      dest-file path-addr path-len [:
        { file file-addr file-bytes dir }
        file-addr file-bytes file dir open-file
      ;] fat32-tools::current-fs@ with-root-path
    else
      dest-file path-addr path-len [:
        { file file-addr file-bytes dir }
        file-addr file-bytes file dir create-file
      ;] fat32-tools::current-fs@ with-root-path
    then
    false to sent-header?
    false to closing?
    0 to http-buf-bytes
    true to recv-first-line?
    false to recv-content?
    0 to content-len
    false to content-len-found?
    false to closed-file?
    cr ." DNS LOOKUP"
    domain-addr domain-len my-interface @ resolve-dns-ipv6-addr if
      { addr-0 addr-1 addr-2 addr-3 }
      my-cyw43-net toggle-pico-w-led
      cr ." RESOLVED: " addr-0 addr-1 addr-2 addr-3 ipv6.
      EPHEMERAL_PORT addr-0 addr-1 addr-2 addr-3 port
      my-interface @ allocate-tcp-connect-ipv6-endpoint if
        drop cr ." CONNECTED"
      else
        cr ." NOT CONNECTED" drop
      then
    else
      cr ." NOT RESOLVED" 2drop 2drop
    then
  ;

end-module