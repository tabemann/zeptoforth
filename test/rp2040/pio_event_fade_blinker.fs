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
  schedule import
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

  \ The blinker maximum input shade
  variable blinker-max-input-shade

  \ The blinker input shade
  variable blinker-input-shade
  
  \ The blinker maximum shade
  variable blinker-max-shade
  
  \ The blinker shading
  variable blinker-shade

  \ The blinker maximum input step delay
  variable blinker-max-input-step-delay

  \ The blinker input step delay
  variable blinker-input-step-delay

  \ The maximum blinker step delay in 100 us increments
  variable blinker-max-step-delay

  \ The blinker shade step delay in 100 us increments
  variable blinker-step-delay

  \ The blinker step delay step delay
  variable blinker-step-delay-step-delay

  \ The blinker schedule
  schedule-size buffer: blinker-schedule

  \ The blinker shade action
  action-size buffer: blinker-shade-action

  \ The blinker shade rate action
  action-size buffer: blinker-rate-action

  \ The blinker shade conversion routine
  : convert-shade ( i -- shade ) 0 swap 2dup 2dup f* 0,01 f* d+ nip ;

  \ Step delay polynomial
  : step-delay-poly ( i -- step-delay ) 0 swap 2dup 2dup f* 0,1 f* d+ ;
  
  \ The blinker input step delay conversion routine
  : convert-step-delay ( i -- step-delay )
    step-delay-poly blinker-max-input-step-delay @ step-delay-poly f/
    0 blinker-max-step-delay @ f* nip
  ;
  
  \ PIO interrupt handler
  : handle-pio ( -- )
    blinker-state @ not if
      blinker-shade @ 0 PIO0 TXF !
    else
      blinker-max-shade @ blinker-shade @ - 0 PIO0 TXF !
    then
    blinker-state @ not blinker-state !
    0 INT_SM_TXNFULL PIO0 INTR !
    PIO0_IRQ0 NVIC_ICPR_CLRPEND!
  ;

  \ Set the sahding
  : blinker-shade! ( i -- )
    blinker-max-input-shade @ convert-shade blinker-max-shade !
    convert-shade blinker-shade !
  ;
  
  \ The blinker shading loop
  : blinker-shade-loop ( -- )
    begin
      begin blinker-max-input-shade @ blinker-input-shade @ > while
	1 blinker-input-shade +!
	blinker-input-shade @ blinker-shade!
	blinker-step-delay @ systick-counter current-action action-delay
	yield
      repeat
      yield
      begin 0 blinker-input-shade @ < while
	-1 blinker-input-shade +!
	blinker-input-shade @ blinker-shade!
	blinker-step-delay @ systick-counter current-action action-delay
	yield
      repeat
      yield
    again
  ;

  \ The blinker shading rate loop
  : blinker-rate-loop ( -- )
    begin
      begin blinker-max-input-step-delay @ blinker-input-step-delay @ > while
	1 blinker-input-step-delay +!
	blinker-input-step-delay @ convert-step-delay blinker-step-delay !
	blinker-step-delay-step-delay @ systick-counter
	current-action action-delay
	yield
      repeat
      yield
      begin 0 blinker-input-step-delay @ < while
	-1 blinker-input-step-delay +!
	blinker-input-step-delay @ convert-step-delay blinker-step-delay !
	blinker-step-delay-step-delay @ systick-counter
	current-action action-delay
	yield
      repeat
      yield
    again
  ;

  \ Init blinker
  : init-blinker ( -- )
    true blinker-state !
    500 blinker-max-input-shade !
    0 blinker-input-shade !
    1000 blinker-max-input-step-delay !
    0 blinker-input-step-delay !
    25 blinker-max-step-delay !
    0 blinker-step-delay !
    100 blinker-step-delay-step-delay !
    0 blinker-shade!
    %0000 CTRL_SM_ENABLE_MASK CTRL_SM_ENABLE_LSB PIO0 CTRL field!
    %0001 CTRL_SM_RESTART_MASK CTRL_SM_RESTART_LSB PIO0 CTRL field!
    6 25 GPIO_CTRL_FUNCSEL!
    25 bit GPIO_OE_SET !
    25 bit GPIO_OUT_CLR !
    781 SM_CLKDIV_INT_MASK SM_CLKDIV_INT_LSB 0 PIO0 SM_CLKDIV field!
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
    blinker-schedule init-schedule
    ['] blinker-shade-loop blinker-shade-action blinker-schedule init-action
    ['] blinker-rate-loop blinker-rate-action blinker-schedule init-action
    blinker-shade-action enable-action
    blinker-rate-action enable-action
    0 [: blinker-schedule run-schedule ;] 320 128 512 spawn run
  ;

end-module
