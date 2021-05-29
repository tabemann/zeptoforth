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
  import systick-module

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
  : handle-exti-2 ( -- ) 2 EXTI_PR! 1 temp-count +! ;

  \ The temperature tracker
  : temp-tracker ( -- )
    begin
      0 temp-count !
      true 2 EXTI_IMR!
      true 3 GPIOE BSRR!
      104 systick-divisor * systick-counter current-task set-task-delay pause
      false 3 GPIOE BSRR!
      false 2 EXTI_IMR!
      temp-count @ dup temp ! 0<> if
	true temp-read? !
	false temp-error? !
      else
	false temp-read? !
	true temp-error? !
      then
      104 systick-divisor * systick-counter current-task set-task-delay pause
    again
  ;

  \ Temperature error exception
  : x-temp-error space ." temperature error" cr ;
  
  \ Read the current temperature
  : read-temp ( -- u )
    [: temp-read? @ temp-error? @ or ;] wait
    temp-error? @ triggers x-temp-error
    temp @
  ;

  \ Read the current temperature in degrees Celsius
  : read-temp-c ( -- f )
    read-temp 0 swap 16,0 f/ 50,0 d-
  ;

  \ Read the current temperature in degrees Fahrenheit
  : read-temp-f ( -- f )
    read-temp-c 1,8 f* 32,0 d+
  ;
  
  \ Initialize the temperature sensor
  : init-temp ( -- )
    0 temp !
    0 temp-count !
    false temp-read? !
    false temp-error? !
    false 2 EXTI_IMR!
    false 2 EXTI_EMR!
    EXTI_2 NVIC_ICER_CLRENA!
    syscfg-clock-enable
    INPUT_MODE 2 GPIOE MODER!
    PULL_UP 2 GPIOE PUPDR!
    VERY_HIGH_SPEED 2 GPIOE OSPEEDR!
    OUTPUT_MODE 3 GPIOE MODER!
    false 3 GPIOE BSRR!
    PE 2 SYSCFG_EXTICRx!
    ['] handle-exti-2 exti-2-handler-hook !
    0 EXTI_2 NVIC_IPR_IP!
    EXTI_2 NVIC_ISER_SETENA!
    true 2 EXTI_FTSR!
    0 ['] temp-tracker 256 256 256 spawn temp-task !
    1 temp-task @ set-task-priority
    temp-task @ run
  ;

end-module