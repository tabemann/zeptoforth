\ Copyright (c) 2023 Travis Bemann
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

\ Note that prior to running this code, one must upload the CYW43439 firmware
\ and CYW43439 driver to one's Raspberry Pi Pico W (this does not work on a
\ Raspberry Pi Pico) as follows:
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
\ If one only desires the CYW43439 driver and not zeptoIP, the following can be
\ substituted:
\ 
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp2040/cyw43/cyw43_all.fs
\ 
\ Once this setup is complete, load this code and then execute:
\ 
\ pico-w-blinky::run-blinky

begin-module pico-w-blinky

  oo import
  cyw43-control import

  <cyw43-control> class-size buffer: my-cyw43-control

  23 constant pwr-pin
  24 constant dio-pin
  25 constant cs-pin
  29 constant clk-pin
  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance

  false value inited?

  : run-blinky ( -- )
    inited? not if
      true to inited?
      default-mac-addr cyw43-clm::data cyw43-clm::size
      cyw43-fw::data cyw43-fw::size pwr-pin clk-pin dio-pin cs-pin
      pio-addr sm-index pio-instance
      <cyw43-control> my-cyw43-control init-object
      my-cyw43-control init-cyw43
    then
    begin key? not while
      true 0 my-cyw43-control cyw43-gpio!
      500 ms
      false 0 my-cyw43-control cyw43-gpio!
      500 ms
    repeat
  ;

end-module
