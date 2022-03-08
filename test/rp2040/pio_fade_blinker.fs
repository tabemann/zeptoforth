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

  interrupt import
  gpio import
  pio import
  task import
  systick import

  \ The initial setup
  create pio-init
  1 SET_PINDIRS set,
  0 SET_PINS set,
  
  \ The PIO code
  create pio-code
  PULL_BLOCK PULL_NOT_EMPTY pull,
  32 OUT_X out,
  1 SET_PINS set,
  3 COND_X1- jmp,
  PULL_BLOCK PULL_NOT_EMPTY pull,
  32 OUT_X out,
  0 SET_PINS set,
  7 COND_X1- jmp,

  \ The blinker state
  variable blinker-state

  \ The blinker maximum delay
  variable blinker-max-delay
  
  \ The blinker shading
  variable blinker-shade

  \ The blinker shade step delay in 100 us increments
  variable blinker-step-delay

  \ The blinker shade step delta
  variable blinker-step-delta

  \ PIO interrupt handler
  : handle-pio ( -- )
    blinker-state @ not if
      blinker-shade @ 0 PIO0 TXF !
    else
      blinker-max-delay @ blinker-shade @ - 0 PIO0 TXF !
    then
    blinker-state @ not blinker-state !
    0 INT_SM_TXNFULL PIO0 INTR !
    PIO0_IRQ0 NVIC_ICPR_CLRPEND!
  ;

  \ The blinker shading task loop
  : blinker-shade-loop ( -- )
    begin
      begin blinker-shade @ blinker-max-delay @ < while
	blinker-step-delay @ systick-counter current-task delay
	blinker-shade @ blinker-step-delta @ + blinker-max-delay @ min
	blinker-shade !
      repeat
      begin blinker-shade @ 0> while
	blinker-step-delay @ systick-counter current-task delay
	blinker-shade @ blinker-step-delta @ - 0 max blinker-shade !
      repeat
    again
  ;

  \ Init blinker
  : init-blinker ( -- )
    true blinker-state !
    500 blinker-max-delay !
    0 blinker-shade !
    50 blinker-step-delay !
    1 blinker-step-delta !
    %0000 CTRL_SM_ENABLE_MASK CTRL_SM_ENABLE_LSB PIO0 CTRL field!
    %0001 CTRL_SM_RESTART_MASK CTRL_SM_RESTART_LSB PIO0 CTRL field!
    6 25 GPIO_CTRL_FUNCSEL!
    25 bit GPIO_OE_SET !
    25 bit GPIO_OUT_CLR !
    6250 SM_CLKDIV_INT_MASK SM_CLKDIV_INT_LSB 0 PIO0 SM_CLKDIV field!
    0 SM_CLKDIV_FRAC_MASK SM_CLKDIV_FRAC_LSB 0 PIO0 SM_CLKDIV field!
    1 SM_PINCTRL_SET_COUNT_MASK SM_PINCTRL_SET_COUNT_LSB
    0 PIO0 SM_PINCTRL field!
    25 SM_PINCTRL_SET_BASE_MASK SM_PINCTRL_SET_BASE_LSB
    0 PIO0 SM_PINCTRL field!
    0 SM_EXECCTRL_WRAP_BOTTOM_MASK SM_EXECCTRL_WRAP_BOTTOM_LSB
    0 PIO0 SM_EXECCTRL field!
    7 SM_EXECCTRL_WRAP_TOP_MASK SM_EXECCTRL_WRAP_TOP_LSB
    0 PIO0 SM_EXECCTRL field!
    SM_EXECCTRL_OUT_STICKY 0 PIO0 SM_EXECCTRL bis!
    2 0 ?do pio-init i 2 * + h@ 0 PIO0 SM_INSTR ! loop
    8 0 ?do pio-code i 2 * + h@ i PIO0 INSTR_MEM ! loop
    0 0 PIO0 SM_ADDR !
    blinker-shade @ 0 PIO0 TXF !
    ['] handle-pio PIO0_IRQ0 16 + vector!
    0 INT_SM_TXNFULL IRQ0 PIO0 INTE bis!
    PIO0_IRQ0 NVIC_ISER_SETENA!
    %0001 CTRL_SM_ENABLE_MASK CTRL_SM_ENABLE_LSB PIO0 CTRL field!
    0 ['] blinker-shade-loop 320 128 512 spawn run
  ;

end-module