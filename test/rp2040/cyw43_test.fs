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

begin-module cyw43-test
  
  oo import
  cyw43-control import
  armv6m import
  
  23 constant pwr-pin
  24 constant dio-pin
  25 constant cs-pin
  29 constant clk-pin
  
  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  <cyw43-control> class-size buffer: my-cyw43-control
  
  1500 constant mtu-size
  mtu-size cell align aligned-buffer: frame
  
  : rev16 ( h -- h' ) code[ r6 r6 rev16_,_ ]code ;
  
  : init-test ( -- )
    dma-pool::init-dma-pool
    cyw43-clm::data cyw43-clm::size cyw43-fw::data cyw43-fw::size
    pwr-pin clk-pin dio-pin cs-pin pio-addr sm-index pio-instance
    <cyw43-control> my-cyw43-control init-object
    my-cyw43-control init-cyw43
  ;

  : mac. { addr -- }
    6 0 ?do addr i + c@ h.2 i 5 <> if ." :" then loop
  ;
  
  : ethernet-header. { addr -- }
    cr ." destination MAC: " addr cyw43-structs::ethh-destination-mac mac.
    cr ." source MAC: " addr cyw43-structs::ethh-source-mac mac.
    cr ." ethernet type " addr cyw43-structs::ethh-ether-type h@ rev16 h.4
  ;
  
  : receive-data
    begin key? not while
      frame cyw43-test::my-cyw43-control cyw43-control::get-cyw43-rx { bytes }
      cr ." bytes: " bytes .
      frame ethernet-header.
      frame frame bytes + dump
    repeat
  ;

  : run-test { D: ssid D: pass -- }
    init-test
    ssid pass my-cyw43-control join-cyw43-wpa2
    receive-data
  ;

end-module