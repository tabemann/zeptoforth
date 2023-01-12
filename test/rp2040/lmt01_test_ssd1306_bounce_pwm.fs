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
  pwm import
  task import
  systick import
  oo import
  bitmap import
  ssd1306 import
  font import
  simple-font import
  systick import
  rng import

  \ A sensor structure
  begin-structure sensor-size
    \ The sensor GPIO
    field: sensor-gpio

    \ The sensor pin
    field: sensor-pin
    
    \ The temperature
    field: temp

    \ Whether the temperature has been read
    field: temp-read?
  end-structure
  
  \ The sensor count
  1 constant sensor-count

  \ The sensors
  sensor-count sensor-size * buffer: sensors
  
  \ The temperature counter
  variable temp-count
  
  \ The temperature task
  variable temp-task
  
  \ The PWM peripheral index
  1 constant pwm-index
  
  \ The PWM wrap level
  256 constant pwm-wrap
  
  \ The PWM GPIO pin
  19 constant pwm-gpio-pin
  
  \ The PIO handler
  : handle-pwm ( -- )
    pwm-wrap temp-count +! 
    pwm-index bit clear-pwm-int
    clear-pwm-pending
  ;

  \ Get the sensor for an index
  : sensor ( sensor -- addr ) sensor-size * sensors + ;
  
  \ The temperature tracker
  : temp-tracker ( -- )
    begin
      sensor-count 0 ?do
        0 temp-count !
        0 pwm-index pwm-counter!
        i sensor sensor-pin @ output-pin
        pwm-index bit enable-pwm
        high i sensor sensor-pin @ pin!
        104 ms
        low i sensor sensor-pin @ pin!
        i sensor sensor-pin @ input-pin
        pwm-index bit disable-pwm
        pwm-index pwm-counter@ temp-count @ +
        dup i sensor temp ! 0<> i sensor temp-read? !
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
    begin dup temp-read? @ not while pause repeat
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
    loop
  ;
  
  \ Initialize PWM
  : init-pwm ( -- )
    pwm-gpio-pin pull-up-pin
    pwm-gpio-pin fast-pin
    pwm-gpio-pin pwm-pin
    0 1 pwm-index pwm-clock-div!
    pwm-index falling-edge-pwm
    pwm-wrap pwm-index pwm-top!
    ['] handle-pwm pwm-vector!
    pwm-index bit enable-pwm-int
  ;
  
  \ Initialize the temperature sensor
  : init-temp ( -- )
    17 0 sensor!
    init-sensors
    init-pwm
    0 ['] temp-tracker 420 128 512 spawn temp-task !
    1 temp-task @ task-priority!
    temp-task @ run
  ;
  
  \ An I2C SSD1306 device
  <ssd1306> class-size buffer: my-ssd1306
  
  \ The number of columns
  128 constant my-width

  \ The number of rows
  64 constant my-height
  
  \ The width of a character
  7 constant my-char-width
  
  \ The height of a character
  7 constant my-char-height

  \ My framebuffer size
  my-width my-height bitmap-buf-size constant my-buf-size

  \ My framebuffer
  my-buf-size buffer: my-buf
  
  \ The readout string size
  12 constant readout-string-size
  
  \ The readout type
  begin-structure readout-size
    \ Buffer to contain the readout string
    readout-string-size +field readout-string
    
    \ The actual readout string length
    field: readout-string-len
    
    \ The readout x coordinate
    2field: readout-x
    
    \ THe reading y coordinate
    2field: readout-y
    
    \ The readout x delta
    2field: readout-x-delta
    
    \ The readout y delta
    2field: readout-y-delta
    
    \ The last time the readout was updated
    field: readout-last-systick
  end-structure
  
  \ Readout speed
  10,0 2constant readout-speed
  
  \ Readout count
  2 constant readout-count
  
  \ Readout draw/move interval
  1000 constant readout-interval
  
  \ Update interval
  10000 constant update-interval
  
  \ The Celsius readout
  readout-size buffer: readout-c
  
  \ The Fahrenheit readout
  readout-size buffer: readout-f
  
  \ The raw temperature readout
  readout-size buffer: readout-t
  
  \ Is the display initialized?
  false value display-inited?

  \ Display temperatures repeatedly task
  0 value display-temps-task

  \ Are we currently displaying temperatures?
  false value display-temps?
  
  \ Generate a random fixed-point value in a range
  : random-fixed { max-lo max-hi -- d }
    random 0 max-lo max-hi f*
  ;
  
  \ Initialize a readout
  : init-readout { init-chars readout -- }
    readout readout-string readout-string-size $20 fill
    init-chars readout readout-string-len !
    my-width init-chars my-char-width * - s>f random-fixed readout readout-x 2!
    my-height my-char-height - s>f random-fixed readout readout-y 2!
    pi 2,0 f* random-fixed { angle-lo angle-hi }
    angle-lo angle-hi cos readout-speed f* readout readout-x-delta 2!
    angle-lo angle-hi sin readout-speed f* readout readout-y-delta 2!
    systick-counter readout readout-last-systick !
  ;
  
  \ Initialize the display
  : init-display ( -- )
    display-inited? not if
      14 15 my-buf my-width my-height SSD1306_I2C_ADDR 1 <ssd1306> my-ssd1306 init-object
      init-simple-font
      7 readout-c init-readout
      7 readout-f init-readout
      7 readout-t init-readout
      true to display-inited?
    then
  ;
  
  \ Draw a readout
  : draw-readout { readout -- }
    readout readout-string
    readout readout-string-len @
    readout readout-x 2@ f>s
    readout readout-y 2@ f>s
    my-ssd1306 a-simple-font xor-string
  ;
  
  \ Move a readout
  : move-readout { readout -- }
    systick-counter { new-systick }
    new-systick readout readout-last-systick @ - s>f 10000,0 f/ { systick-diff-lo systick-diff-hi }
    readout readout-x-delta 2@ systick-diff-lo systick-diff-hi f*
    readout readout-x 2+!
    readout readout-y-delta 2@ systick-diff-lo systick-diff-hi f*
    readout readout-y 2+!
    new-systick readout readout-last-systick !
  ;
  
  \ Bounce a readout
  : bounce-readout { readout -- }
    readout readout-x 2@ d0<= if
      readout readout-x-delta 2@ dabs readout readout-x-delta 2!
    else
      readout readout-x 2@ readout readout-string-len @ my-char-width * s>f d+
      my-width s>f d>= if
        readout readout-x-delta 2@ dabs dnegate readout readout-x-delta 2!
      then
    then
    readout readout-y 2@ d0<= if
      readout readout-y-delta 2@ dabs readout readout-y-delta 2!
    else
      readout readout-y 2@ my-char-height s>f d+ my-height s>f d>= if
        readout readout-y-delta 2@ dabs dnegate readout readout-y-delta 2!
      then
    then
  ;
  
  \ Carry out an operation on both readoutus
  : both-readouts { xt -- }
    readout-c xt execute
    readout-f xt execute
    readout-t xt execute
  ;
  
  \ Display and run the readouts
  : cycle-readouts ( -- )
    ['] draw-readout both-readouts
    my-ssd1306 update-display
    ['] draw-readout both-readouts
    ['] move-readout both-readouts
    ['] bounce-readout both-readouts
  ;
  
  \ Set the text of a readout
  : set-readout-text { c-addr u readout -- }
    c-addr readout readout-string u move
    u readout readout-string-len !
  ;
  
  \ Append the text of a readout
  : append-readout-text { c-addr u readout -- }
    c-addr readout readout-string readout readout-string-len @ + u move
    u readout readout-string-len +!
  ;
  
  \ Update temperatures for a number of sensors
  : update-temp ( sensor -- )
    dup sensor-count u>= triggers x-sensor-out-of-range
    32 [:
      { sensor buf }
      buf sensor read-temp-c 2 format-fixed-truncate
      readout-c set-readout-text
      s"  C" readout-c append-readout-text
      buf sensor read-temp-f 2 format-fixed-truncate
      readout-f set-readout-text
      s"  F" readout-f append-readout-text
      buf sensor read-temp format-integer
      readout-t set-readout-text
      s"  t" readout-t append-readout-text
    ;] with-allot
  ;

  \ Display temperatures repeatedly
  : display-temps ( -- )
    systick-counter { start-systick }
    start-systick { update-start-systick }
    begin
      display-temps? if
        systick-counter update-start-systick - update-interval > if
          0 update-temp
          update-interval +to update-start-systick
        then
        cycle-readouts
      else
        my-ssd1306 clear-bitmap
        my-ssd1306 update-display
      then
      readout-interval start-systick current-task delay
      readout-interval +to start-systick
    again
  ;

  \ Start displaying temperatures repeatedly
  : start-display-temps ( -- )
    true to display-temps?
    display-temps-task 0= if
      init-display
      0 ['] display-temps 512 128 512 spawn to display-temps-task
      0 display-temps-task task-priority!
      display-temps-task run
    then
  ;
  
  \ Stop displaying temperatures repeatedly
  : stop-display-temps ( -- )
    false to display-temps?
  ;

end-module