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

begin-module read-temp

  interrupt import
  gpio import
  pin import
  task import
  systick import

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
  1 constant sensor-count

  \ The sensors
  sensor-count sensor-size * buffer: sensors
  
  \ The temperature counter
  variable temp-count
  
  \ The temperature task
  variable temp-task

  \ The IO IRQ
  13 constant io-irq

  \ The IO IRQ vector
  io-irq 16 + constant io-vector

  \ The GPIO pin for the temperature data
  16 constant temp-pin

  \ The GPIO handler
  : handle-gpio ( -- )
    temp-pin PROC0_INTS_GPIO_EDGE_LOW@ if 1 temp-count +! then
    temp-pin INTR_GPIO_EDGE_LOW!
  ;

  \ Get the sensor for an index
  : sensor ( sensor -- addr ) sensor-size * sensors + ;
  
  \ The temperature tracker
  : temp-tracker ( -- )
    begin
      sensor-count 0 ?do
        0 temp-count !
        i sensor sensor-pin @ output-pin
        io-irq NVIC_ISER_SETENA!
        high i sensor sensor-pin @ pin!
        104 ms
        low i sensor sensor-pin @ pin!
        i sensor sensor-pin @ input-pin
        io-irq NVIC_ICER_CLRENA!
	temp-count @ dup i sensor temp ! 0<> if
	  true i sensor temp-read? !
	  false i sensor temp-error? !
	else
	  false i sensor temp-read? !
	  true i sensor temp-error? !
	then
	104 ms
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

  \ Set a sensor's pin
  : sensor! ( pin sensor -- ) sensor sensor-pin ! ;

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
    io-irq NVIC_ICER_CLRENA!
    temp-pin input-pin
    temp-pin pull-up-pin
    temp-pin fast-pin
    17 0 sensor!
    init-sensors
    ['] handle-gpio io-vector vector!
    true temp-pin PROC0_INTE_GPIO_EDGE_LOW!
    0 io-irq NVIC_IPR_IP!
    \ io-irq NVIC_ISER_SETENA!
    0 ['] temp-tracker 420 128 512 spawn temp-task !
    1 temp-task @ task-priority!
    temp-task @ run
  ;

  \ Display temperatures for a number of sensors
  : display-temp ( sensor -- )
    dup sensor-count u>= triggers x-sensor-out-of-range
    >r r@ read-temp-f r@ read-temp-c r@ read-temp
    cr ." Temperature " r> (.) ." : " . space ." pulses "
    f. ." °C " f. ." °F"
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
    1 ['] display-temps 420 128 512 spawn display-temps-task !
    0 display-temps-task @ task-priority!
    display-temps-task @ run
  ;

end-module