\ Copyright (c) 2026 Travis Bemann
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
\ Once that is done load extra/rp_common/net_tools/http_server.fs, execute:
\
\ 80 pico-w-net::my-interface @ net-http::start-server
\
\ where 80 is the HTTP port; a different value can be used if one wants to use
\ a different port
\
\ Then load this file and execute:
\
\ net-http-led::init-led
\
\ You may then connect to the HTTP server started at:
\
\ http://<the reported IPv4 address>:80/led/

begin-module net-http-led

  net-http import
  pico-w-cyw43-net-ipv4 import

  \ Generate a button with a label
  : button. { D: label D: path -- }
    .\" <form method=\"post\" action=\""
    path type
    .\" \"><button type=\"submit\">"
    label type
    ." </button></form>" cr
  ;
  
  : menu ( -- )
    ." <html>" cr
    ." <head><title>LED Index</title></head>" cr
    ." <body>" cr
    ." <h1>LED Index</h1>" cr
    s" LED Off" s" /led/off" button.
    s" LED On" s" /led/on" button.
    s" Toggle LED" s" /led/toggle" button.
    ." </body>" cr
    ." </html>" cr
  ;
  
  : index-html ( -- )
    http-method@ dup method-get = over method-head = or swap method-post = or if
      s" text/html " http-ok.
      http-method@ dup method-get = swap method-post = or if
        menu
      then
    else
      http-method-not-allowed.
    then
  ;
  
  : action { xt -- }
    http-method@ dup method-head = swap method-post = or if
      http-method@ method-post = if xt execute then
      s" text/html " http-ok.
      http-method@ method-post = if menu then
    else
      http-method-not-allowed.
    then
  ;

  : init-led ( -- )
    ['] index-html s" /led" register-fixed-uri
    ['] index-html s" /led/" register-fixed-uri
    ['] index-html s" /led/index.html" register-fixed-uri
    [:
      [: false pico-w-net::my-cyw43-net pico-w-led! ;] action
    ;] s" /led/off" register-fixed-uri
    [:
      [: true pico-w-net::my-cyw43-net pico-w-led! ;] action
    ;] s" /led/on" register-fixed-uri
    [:
      [: pico-w-net::my-cyw43-net toggle-pico-w-led ;] action
    ;] s" /led/toggle" register-fixed-uri
  ;
  
end-module
