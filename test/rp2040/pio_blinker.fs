\ Copyright (c) 2021-2023 Travis Bemann
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

  gpio import
  pio import

  \ The initial setup
  :pio pio-init
    1 SET_PINDIRS set,
    0 SET_PINS set,
  ;pio
  
  \ The PIO code
  :pio pio-code
    1 19 SET_PINS set+,
    MOV_SRC_X 19 MOV_OP_NONE MOV_DEST_X mov+,
    MOV_SRC_X 19 MOV_OP_NONE MOV_DEST_X mov+,
    MOV_SRC_X 19 MOV_OP_NONE MOV_DEST_X mov+,
    MOV_SRC_X 19 MOV_OP_NONE MOV_DEST_X mov+,
    0 19 SET_PINS set+,
    MOV_SRC_X 19 MOV_OP_NONE MOV_DEST_X mov+,
    MOV_SRC_X 19 MOV_OP_NONE MOV_DEST_X mov+,
    MOV_SRC_X 19 MOV_OP_NONE MOV_DEST_X mov+,
    MOV_SRC_X 19 MOV_OP_NONE MOV_DEST_X mov+,
  ;pio
  
  \ Init blinker
  : init-blinker ( -- )
    %0001 PIO0 sm-disable
    %0001 PIO0 sm-restart
    0 62500 0 PIO0 sm-clkdiv!
    25 1 PIO0 pins-pio-alternate
    25 1 0 PIO0 sm-set-pins!
    on 0 PIO0 sm-out-sticky!
    pio-init p-prog 0 PIO0 sm-instr!
    0 PIO0 pio-code 0 setup-prog
    %0001 PIO0 sm-enable
  ;

end-module
