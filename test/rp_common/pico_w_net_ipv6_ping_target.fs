\ Copyright (c) 2023-2025 Travis Bemann
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
\ 2. Execute: s" <WiFi SSID>" s" <WiFi password>"
\             pico-w-net-test::start-server

begin-module pico-w-net-test

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
  lock import
  simple-cyw43-net-ipv6 import
  pico-w-cyw43-net-ipv6 import

  <pico-w-cyw43-net-ipv6> class-size buffer: my-cyw43-net
  variable my-cyw43-control
  variable my-interface

  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  
  \ Initialize the test
  : start-server { D: ssid D: pass -- }
    sm-index pio-instance <pico-w-cyw43-net-ipv6> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    cyw43-consts::PM_AGGRESSIVE my-cyw43-control @ cyw43-power-management!
    begin ssid pass my-cyw43-control @ join-cyw43-wpa2 nip until
    my-cyw43-net run-net-process
    cr ." Autoconfiguring link-local IPv6 address... "
    my-interface @ autoconfigure-link-local-ipv6-addr if
      ." Success"
    else
      ." Failure"
    then
    cr ." Discovering IPv6 address..."
    my-interface @ intf-ipv6-addr@ cr ." IPv6 address: " ipv6.
    my-interface @ intf-ipv6-prefix@ cr ." IPv6 prefix: " ipv6.
    my-interface @ intf-autonomous@ cr ." Autonomous: "
    if ." yes" else ." no" then
    my-interface @ intf-ipv6-prefix-len@ cr ." IPv6 prefix length: " .
    my-cyw43-net toggle-pico-w-led
  ;

end-module