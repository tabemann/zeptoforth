\ Copyright (c) 2022-2024 Travis Bemann
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

continue-module forth

  pio import
  pin import
  
  2 constant your-pin \ Change this to the desired GPIO pin

  \ Our initial PIO instructions
  create pio-init
  %00001 SET_PINDIRS set,
  %00000 SET_PINS set,

  \ Our PIO program
  :pio pio-program
  start>
  wrap>
  %00001 SET_PINS set,
  %00000 SET_PINS set,
  <wrap
  ;pio
  
  \ Initialize our test
  : init-test ( -- )
    %0001 PIO0 sm-disable
    %0001 PIO0 sm-restart
    0 1 0 PIO0 sm-clkdiv!
    your-pin 1 PIO0 pins-pio-alternate
    your-pin 1 0 PIO0 sm-set-pins!
    0 3 0 PIO0 sm-wrap!
    on 0 PIO0 sm-out-sticky!
    pio-init 2 0 PIO0 sm-instr!
    PIO0 pio-program p-size alloc-piomem { base-addr }
    0 PIO0 pio-program base-addr setup-prog
    base-addr 0 PIO0 sm-addr!
    your-pin fast-pin
    %0001 PIO0 sm-enable
  ;

end-module

