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
\ net-http-demo::init-demo
\
\ You may then connect to the HTTP server started at:
\
\ http://<the reported IPv4 address>:80/

begin-module net-http-demo

  net-http import

  : index-html ( -- )
    http-method@ dup method-get = over method-head = or swap method-post = or if
      s" text/html " http-ok.
      http-method@ dup method-get = swap method-post = or if
        ." <html>"
        ." <head><title>Index</title></head>"
        ." <body>"
        ." <h1>Index</h1>"
        ." <ul>
        .\" <li><a href=\"/hello.txt\">hello.txt</a></li>"
        .\" <li><a href=\"/hello.html\">hello.html</a></li>"
        .\" <li><a href=\"/dump-headers.txt\">dump-headers.txt</a></li>"
        .\" <li><a href=\"/echo-uri/\">echo-uri/</a></li>"
        .\" <li><a href=\"/dump-tasks.txt\">dump-tasks.txt</a></li>"
        ." </ul>"
        ." </body>"
        ." </html>"
      then
    else
      http-method-not-allowed.
    then
  ;
  
  : hello-txt ( -- )
    http-method@ dup method-get = over method-head = or swap method-post = or if
      s" text/text " http-ok.
      http-method@ dup method-get = swap method-post = or if
        ." Hello, world!"
      then
    else
      http-method-not-allowed.
    then
  ;

  : hello-html ( -- )
    http-method@ dup method-get = over method-head = or swap method-post = or if
      s" text/html " http-ok.
      http-method@ dup method-get = swap method-post = or if
        ." <html><head><title>Hello, world!</title></head>"
        ." <body><h1>Hello, world!</h1></body>"
        ." </html>"
      then
    else
      http-method-not-allowed.
    then
  ;

  : dump-headers-txt ( -- )
    0 0 { cr-count nl-count }
    s" text/text" http-ok.
    begin http-closing? not cr-count 2 <> nl-count 2 <> and and while
      key { c }
      c $0D = if 1 +to cr-count then
      c $0A = if 1 +to nl-count then
      c $0D <> c $0A <> and if 0 to cr-count 0 to nl-count then
      c emit
    repeat
  ;

  : echo-uri ( -- )
    http-method@ dup method-get = over method-head = or swap method-post = or if
      s" text/text " http-ok.
      http-method@ dup method-get = swap method-post = or if
        http-uri@ type
      then
    else
      http-method-not-allowed.
    then
  ;
  
  : dump-tasks-txt ( -- )
    http-method@ dup method-get = over method-head = or swap method-post = or if
      s" text/text " http-ok.
      http-method@ dup method-get = swap method-post = or if
        task::dump-tasks
      then
    else
      http-method-not-allowed.
    then
  ;

  : init-demo ( -- )
    ['] index-html s" /" register-fixed-uri
    ['] index-html s" /index.html" register-fixed-uri
    ['] hello-txt s" /hello.txt" register-fixed-uri
    ['] hello-html s" /hello.html" register-fixed-uri
    ['] dump-headers-txt s" /dump-headers.txt" register-fixed-uri
    ['] echo-uri s" /echo-uri/" register-prefix-uri
    ['] dump-tasks-txt s" /dump-tasks.txt" register-fixed-uri
  ;
  
end-module
