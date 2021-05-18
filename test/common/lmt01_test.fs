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

begin-module-once temp-module

  import interrupt-module
  import gpio-module
  import exti-module
  import task-module

  \ The temperature
  variable temp

  \ Whether the temperature has been read
  variable temp-read?

  \ Whether there is a temperature-reading error
  variable temp-error?
  
  \ The temperature counter
  variable temp-count

  \ The temperature task
  variable temp-task

  \ The EXTI 2 handler
  : handle-exti-2 ( -- )
    2 EXTI_PR@ if 1 temp-count +! 2 EXTI_PR! then
  ;

  \ The temperature tracker
  : temp-tracker ( -- )
    begin
      0 temp-count !
      true 3 GPIOE BSRR!
      104 ms
      false 3 GPIOE BSRR!
      temp-count @ dup temp ! 0<> if
	true temp-read? !
	false temp-error? !
      else
	false temp-read? !
	true temp-error? !
      then
      10 ms
    again
  ;

  \ Temperature error exception
  : x-temp-error space ." temperature error" cr ;
  
  \ Read the current temperature
  : read-temp ( -- )
    [: temp-read? @ temp-error? @ or ;] wait
    temp-error? @ triggers x-temp-error
    temp @
  ;
  
  \ Initialize the temperature sensor
  : init-temp ( -- )
    0 temp !
    0 temp-count !
    false temp-read? !
    false temp-error? !
    false 2 EXTI_IMR!
    EXTI_2 NVIC_ICER_CLRENA!
    syscfg-clock-enable
    INPUT_MODE 2 GPIOE MODER!
    OUTPUT_MODE 3 GPIOE MODER!
    false 3 GPIOE BSRR!
    PE 2 SYSCFG_EXTICRx!
    ['] handle-exti-2 exti-2-handler-hook !
    EXTI_2 NVIC_ISER_SETENA!
    true 2 EXTI_IMR!
    true 2 EXTI_EMR!
    false 2 EXTI_RTSR!
    true 2 EXTI_FTSR!
    0 ['] temp-tracker 256 256 256 spawn temp-task !
    1 temp-task @ set-task-priority
    temp-task @ run
  ;

end-module