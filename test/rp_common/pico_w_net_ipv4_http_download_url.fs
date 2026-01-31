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
\ Use instructions
\
\ 1. Load this code into RAM on a zeptoforth install where zeptoIP has already
\    been installed using a terminal which supports zeptoforth, e.g. zeptocom.js
\    or e4thcom in noforth mode.
\ 2. Execute: s" <WiFi SSID>" s" <WiFi password>" pico-w-net-http::init-test
\ 3. Execute: s" <URL>" pico-w-net-http::run-test
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
  net-ipv4 import
  endpoint-process import
  alarm import
  simple-cyw43-net-ipv4 import
  pico-w-cyw43-net-ipv4 import

  <pico-w-cyw43-net-ipv4> class-size buffer: my-cyw43-net
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

  <endpoint-handler> begin-class <tcp-echo-handler>
    
  end-class

  <tcp-echo-handler> begin-implement
  
    \ Handle a endpoint packet
    :noname { endpoint self -- }
      my-cyw43-net toggle-pico-w-led
      sent-header? not endpoint endpoint-tcp-state@ TCP_ESTABLISHED = and if
        cr ." SENDING HEADER" cr
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
      endpoint endpoint-rx-data@ type
      endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
\        cr ." CLOSING CONNECTION" cr
        closing? not if
          true to closing?
          500 0 endpoint [:
            drop my-interface @ close-tcp-endpoint
          ;] my-close-alarm set-alarm-delay-default
        then
      then
      endpoint endpoint-tcp-state@ TCP_LAST_ACK = if
\        cr ." WAITING FOR LAST ACK" cr
      then
      endpoint endpoint-tcp-state@ TCP_CLOSED = if
\        cr ." CONNECTION CLOSED" cr
      then
      endpoint my-interface @ endpoint-done
    ; define handle-endpoint
  
  end-implement
  
  <tcp-echo-handler> class-size buffer: my-tcp-echo-handler
  
  \ Initialize the test
  : init-test { D: ssid D: pass -- }
    1024 256 1024 0 init-default-alarm-task
    sm-index pio-instance <pico-w-cyw43-net-ipv4> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    <tcp-echo-handler> my-tcp-echo-handler init-object
    my-tcp-echo-handler
    my-cyw43-net net-endpoint-process@ add-endpoint-handler
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
  ;

  \ Run the test
  : run-test { addr len -- }
    len url-size min to len
    len url-len !
    addr url-buffer len move
    url-buffer url-len @ validate-url
    url-buffer url-len @ url-domain { domain-addr domain-len }
    url-buffer url-len @ url-port { port }
    false to sent-header?
    false to closing?
    cr ." DNS LOOKUP"
    domain-addr domain-len my-interface @ resolve-dns-ipv4-addr if { addr }
      my-cyw43-net toggle-pico-w-led
      cr ." RESOLVED: " addr ipv4.
      EPHEMERAL_PORT addr port
      my-interface @ allocate-tcp-connect-ipv4-endpoint if
        drop cr ." CONNECTED" cr
      else
        cr ." NOT CONNECTED" drop
      then
    else
      cr ." NOT RESOLVED" drop
    then
  ;

end-module