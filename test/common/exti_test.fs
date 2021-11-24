\ Copyright (c) 2021 Travis Bemann
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

begin-module forth-module

  import interrupt-module
  import gpio-module
  import exti-module
  import task-module

  \ Total count
  variable total-count

  \ Rising edge count
  variable rising-edge-count

  \ Falling edge count
  variable falling-edge-count

  \ Logic high?
  variable logic-high?
  
  \ Edge tracker
  : edge-tracker ( -- )
    rising-edge-count @
    falling-edge-count @
    begin
      over rising-edge-count @ < if
	swap 1+ swap ." +"
      then
      dup falling-edge-count @ < if
	1+ ." -"
      then
    again
  ;

  \ Handle EXTI_2 interrupt
  : handle-exti-2 ( -- )
    1 total-count +!
    2 EXTI_PR@ if
      2 GPIOE IDR@ if
	logic-high? @ not if
	  1 rising-edge-count +!
	  true logic-high? !
	then
      else
	logic-high? @ if
	  1 falling-edge-count +!
	  false logic-high? !
	then
      then
      2 EXTI_PR!
    then
  ;

  \ Edge tracker task
  variable edge-tracker-task
  
  \ Test EXTI
  : init-test ( -- )
    false 2 EXTI_IMR!
    EXTI_2 NVIC_ICER_CLRENA!
    syscfg-clock-enable
    INPUT_MODE 2 GPIOE MODER!
    OUTPUT_MODE 3 GPIOE MODER!
    2 GPIOE IDR@ logic-high? !
    0 total-count !
    0 rising-edge-count !
    0 falling-edge-count !
    PE 2 SYSCFG_EXTICRx!
    0 ['] edge-tracker 320 128 512 spawn edge-tracker-task !
    edge-tracker-task @ run
    ['] handle-exti-2 exti-2-handler-hook !
    EXTI_2 NVIC_ISER_SETENA!
    true 2 EXTI_IMR!
    true 2 EXTI_EMR!
    true 2 EXTI_RTSR!
    true 2 EXTI_FTSR!
  ;
  
  \ Set PE3 high
  : pe3-high ( -- ) true 3 GPIOE BSRR! ;

  \ Set PE3 low
  : pe3-low ( -- ) false 3 GPIOE BSRR! ;
  
end-module