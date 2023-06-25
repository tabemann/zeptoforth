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

begin-module net-test
  
  oo import
  cyw43-control import
  net-misc import
  net-process import
  net import
  net-diagnostic import
  
  23 constant pwr-pin
  24 constant dio-pin
  25 constant cs-pin
  29 constant clk-pin
  
  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  <cyw43-control> class-size buffer: my-cyw43-control
  <interface> class-size buffer: my-interface
  <net-process> class-size buffer: my-net-process
  <arp-diag-handler> class-size buffer: my-arp-diag-handler
  <ipv4-diag-handler> class-size buffer: my-ipv4-diag-handler
  <arp-handler> class-size buffer: my-arp-handler
  
  : init-test ( -- )
    dma-pool::init-dma-pool
    cyw43-clm::data cyw43-clm::size cyw43-fw::data cyw43-fw::size
    pwr-pin clk-pin dio-pin cs-pin pio-addr sm-index pio-instance
    <cyw43-control> my-cyw43-control init-object
    my-cyw43-control init-cyw43
    my-cyw43-control <interface> my-interface init-object
    192 168 1 44 make-ipv4-addr my-interface ipv4-addr!
    my-cyw43-control <net-process> my-net-process init-object
    <arp-diag-handler> my-arp-diag-handler init-object
    <ipv4-diag-handler> my-ipv4-diag-handler init-object
    my-interface <arp-handler> my-arp-handler init-object
    my-arp-diag-handler my-net-process add-net-handler
    my-ipv4-diag-handler my-net-process add-net-handler
    my-arp-handler my-net-process add-net-handler
  ;

  : run-test { D: ssid D: pass -- }
    init-test
    ssid pass my-cyw43-control join-cyw43-wpa2
    my-net-process run-process-net
  ;

end-module