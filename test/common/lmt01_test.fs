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

  \ A sensor structure
  begin-structure sensor-size
    \ The sensor GPIO
    field: sensor-gpio

    \ The sensor pin
    field: sensor-pin
    
    \ The temperature
    field: temp

    \ Whether the temperature has been re ad
    field: temp-read?

    \ Whether there is a temperature-reading error
    field: temp-error?
  end-structure
  
  \ The sensor count
  3 constant sensor-count

  \ The sensors
  sensor-count sensor-size * buffer: sensors
  
  \ The temperature counter
  variable temp-count
  
  \ The temperature task
  variable temp-task

  \ The EXTI 2 handler
  : handle-exti-2 ( -- ) 2 EXTI_PR! 1 temp-count +! ;

  \ Get the sensor for an index
  : sensor ( sensor -- addr ) sensor-size * sensors + ;
  
  \ The temperature tracker
  : temp-tracker ( -- )
    begin
      sensor-count 0 ?do
	0 temp-count !
	true 2 EXTI_IMR!
	OUTPUT_MODE i sensor sensor-pin @ i sensor sensor-gpio @ MODER!
	true i sensor sensor-pin @ i sensor sensor-gpio @ BSRR!
	104 ms
	false i sensor sensor-pin @ i sensor sensor-gpio @ BSRR!
	INPUT_MODE i sensor sensor-pin @ i sensor sensor-gpio @ MODER!
	false 2 EXTI_IMR!
	temp-count @ dup i sensor temp ! 0<> if
	  true i sensor temp-read? !
	  false i sensor temp-error? !
	else
	  false i sensor temp-read? !
	  true i sensor temp-error? !
	then
\	104 ms
      loop
    again
  ;

  \ Temperature error exception
  : x-temp-error space ." temperature error" cr ;

  \ Bad temperature sensor index exception
  : x-sensor-out-of-range space ." out of range sensor index" cr ;
  
  \ Read the current temperature
  : read-temp ( sensor -- u )
    dup sensor-count u>= triggers x-sensor-out-of-range
    sensor
    begin dup temp-read? @ over temp-error? @ or not while pause repeat
    dup temp-error? @ triggers x-temp-error
    temp @
  ;

  \ Read the current temperature in degrees Celsius
  : read-temp-c ( sensor -- f ) read-temp 0 swap 16,0 f/ 50,0 d- ;

  \ Read the current temperature in degrees Fahrenheit
  : read-temp-f ( sensor -- f ) read-temp-c 1,8 f* 32,0 d+ ;

  \ Set a sensor's GPIO and pin
  : sensor! ( pin gpio sensor -- ) sensor rot over sensor-pin ! sensor-gpio ! ;

  \ Initialize the sensors
  : init-sensors ( -- )
    sensor-count 0 ?do
      0 i sensor temp !
      false i sensor temp-read? !
      false i sensor temp-error? !
    loop
  ;
  
  \ Initialize the temperature sensor
  : init-temp ( -- )
    false 2 EXTI_IMR!
    false 2 EXTI_EMR!
    EXTI_2 NVIC_ICER_CLRENA!
    syscfg-clock-enable
    INPUT_MODE 2 GPIOE MODER!
    PULL_UP 2 GPIOE PUPDR!
    VERY_HIGH_SPEED 2 GPIOE OSPEEDR!
    3 GPIOE 0 sensor!
    4 GPIOE 1 sensor!
    5 GPIOE 2 sensor!
    init-sensors
    PE 2 SYSCFG_EXTICRx!
    ['] handle-exti-2 EXTI_2 16 + vector!
    0 EXTI_2 NVIC_IPR_IP!
    EXTI_2 NVIC_ISER_SETENA!
    true 2 EXTI_FTSR!
    0 ['] temp-tracker 256 128 512 spawn temp-task !
    1 temp-task @ set-task-priority
    temp-task @ run
  ;

  \ Display temperatures for a number of sensors
  : display-temp ( sensor -- )
    dup sensor-count u>= triggers x-sensor-out-of-range
    >r r@ read-temp-f r@ read-temp-c r@ read-temp
    cr ." Temperature" r> . ." :" . space ." pulses"
    f. space ." °C" f. space ." °F"
  ;

  \ Display temperatures repeatedly
  : display-temps ( count -- )
    dup sensor-count u> triggers x-sensor-out-of-range
    begin dup 0 ?do i display-temp loop 1000 ms again
  ;

  \ Display temperatures repeatedly task
  variable display-temps-task

  \ Start displaying temperatures repeatedly
  : start-display-temps ( count -- )
    dup sensor-count u> triggers x-sensor-out-of-range
    1 ['] display-temps 256 128 512 spawn display-temps-task !
    0 display-temps-task @ set-task-priority
    display-temps-task @ run
  ;

end-module