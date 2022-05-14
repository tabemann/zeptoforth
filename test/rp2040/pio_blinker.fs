\ Copyright (c) 2021-2022 Travis Bemann
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
  create pio-init
  1 SET_PINDIRS set,
  0 SET_PINS set,
  
  \ The PIO code
  create pio-code
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
  
  \ Init blinker
  : init-blinker ( -- )
    %0000 PIO0 CTRL_SM_ENABLE!
    6 25 GPIO_CTRL_FUNCSEL!
    25 bit GPIO_OE_SET !
    25 bit GPIO_OUT_CLR !
    62500 0 PIO0 SM_CLKDIV_INT!
    0 0 PIO0 SM_CLKDIV_FRAC!
    1 0 PIO0 SM_PINCTRL_SET_COUNT!
    25 0 PIO0 SM_PINCTRL_SET_BASE!
    0 0 PIO0 SM_EXECCTRL_WRAP_BOTTOM!
    9 0 PIO0 SM_EXECCTRL_WRAP_TOP!
    true 0 PIO0 SM_EXECCTRL_OUT_STICKY!
    pio-init 2 0 PIO0 sm-instr!
    pio-code 10 PIO0 instr-mem!
    0 0 PIO0 SM_ADDR !
    %0001 PIO0 CTRL_SM_ENABLE!
  ;

end-module
