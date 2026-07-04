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
\ net-http-time::init-time
\
\ You may then connect to the HTTP server started at:
\
\ http://<the reported IPv4 address>:80/time

begin-module net-http-time

  net-http import
  rtc import

  : serve-time ( -- )
    date-time-size [: { date }
      date date-time@
      s" text/html" http-ok.
      ." <html>" cr
      ." <head>" cr
      ." <title>" date date-time. ." </title>" cr
      .\" <meta http-equiv=\"refresh\" content=\"1\">" cr
      ." </head>" cr
      ." <body>" cr
      ." <h1>" date date-time. ." </h1>" cr
      ." </body>" cr
      ." </html>" cr
    ;] with-aligned-allot
  ;
  
  : init-time ( -- )
    ['] serve-time s" /time" register-fixed-uri
  ;
  
end-module
