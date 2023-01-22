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
  rng import
  rtc import
  closure import
  fat32 import
  simple-fat32 import
  
  \ The PWM peripheral index
  1 constant pwm-index

  \ The PWM input pin
  19 constant pwm-input-pin
  
  \ The PWM wrap level
  65535 constant pwm-wrap

  \ The sensor output pin
  17 constant sensor-output-pin

  \ Temperature read timeout in 100 us systicks
  2500 constant temp-timeout

  \ Temperature error exception
  : x-temp-error space ." temperature error" cr ;

  \ The <pwm-slice> class
  <object> begin-class <pwm-slice>

    \ The PWM slice
    cell member pwm-slice
    
    \ Enable the PWM slice
    method enable-pwm-slice ( self -- )

    \ Disable the PWM slice
    method disable-pwm-slice ( self -- )
    
    \ Set the PWM counter
    method pwm-slice-counter! ( counter self -- )

    \ Get the PWM counter
    method pwm-slice-counter@ ( self -- counter )
    
  end-class

  \ Implement the <pwm-slice> class
  <pwm-slice> begin-implement

    \ Initialize a PWM slice
    :noname { init-pwm-slice-top init-pwm-slice-pin init-pwm-slice self -- }
      self <object>->new
      init-pwm-slice self pwm-slice !
      self disable-pwm-slice
      init-pwm-slice-pin pull-up-pin
      init-pwm-slice-pin fast-pin
      init-pwm-slice-pin pwm-pin
      0 1 self pwm-slice @ pwm-clock-div!
      self pwm-slice @ falling-edge-pwm
      init-pwm-slice-top self pwm-slice @ pwm-top!
      0 self pwm-slice-counter!
    ; define new
    
    \ Enable the PWM slice
    :noname { self -- }
      self pwm-slice @ bit enable-pwm
    ; define enable-pwm-slice

    \ Disable the PWM slice
    :noname { self -- }
      self pwm-slice @ bit disable-pwm
    ; define disable-pwm-slice

    \ Set the PWM counter
    :noname { counter self -- }
      counter self pwm-slice @ pwm-counter!
    ; define pwm-slice-counter!

    \ Get the PWM counter
    :noname { self -- }
      self pwm-slice @ pwm-counter@
    ; define pwm-slice-counter@

  end-implement
  
  \ The <sensor> class
  <object> begin-class <sensor>

    \ The PWM used by the sensor
    cell member sensor-pwm

    \ The sensor pin
    cell member sensor-pin

    \ The measured temperature
    cell member sensor-temp

    \ Whether the temperature has been read
    cell member sensor-read?

    \ The next sensor
    cell member sensor-next
    
    \ Read a sensor
    method read-sensor ( self -- )

    \ Get a sensor's temperature
    method sensor-temp@ ( self -- temp )

    \ Get a sensor's temperature as a fixed-point Celsius value
    method sensor-temp-c@ ( self -- celsius-f )

    \ Get a sensor's temperature as a fixed-point Fahrenheit value
    method sensor-temp-f@ ( self -- fahrenheit-f )
    
  end-class

  \ Implement the <sensor> class
  <sensor> begin-implement

    \ Initialize a sensor
    :noname { self-pwm self-pin self -- }
      self <object>->new
      self-pin self sensor-pin !
      self-pwm self sensor-pwm !
      0 self sensor-temp !
      false self sensor-read? !
      0 self sensor-next !
    ; define new

    \ Read a sensor
    :noname { self -- }
      0 self sensor-pwm @ pwm-slice-counter!
      self sensor-pin @ output-pin
      self sensor-pwm @ enable-pwm-slice
      high self sensor-pin @ pin!
      104 ms
      low self sensor-pin @ pin!
      self sensor-pin @ input-pin
      self sensor-pwm @ disable-pwm-slice
      self sensor-pwm @ pwm-slice-counter@ self sensor-temp !
      self sensor-temp @ 0<> self sensor-read? !
    ; define read-sensor

    \ Get a sensor's temperature
    :noname { self -- temp }
      systick-counter { start-systick }
      begin self sensor-read? @ not while
        systick-counter start-systick - temp-timeout < averts x-temp-error
        pause
      then
      self sensor-temp @
    ; define sensor-temp@

    \ Get a sensor's temperature as a fixed-point Celsius value
    :noname ( self -- celsius-f )
      sensor-temp@ s>f 16,0 f/ 50,0 d-
    ; define sensor-temp-c@

    \ Get a sensor's temperature as a fixed-point Fahrenheit value
    :noname ( self -- fahrenheit-f )
      sensor-temp-c@ 1,8 f* 32,0 d+
    ; define sensor-temp-f@

  end-implement

  \ Implement the <tracker> class
  <object> begin-class <tracker>
    
    \ The sensor count
    cell member tracker-count
    
    \ The first sensor
    cell member tracker-first

    \ The tracker enabled flag
    cell member tracker-enabled?

    \ The tracker started flag
    cell member tracker-started?

    \ Add a sensor to a tracker
    method add-tracker-sensor ( sensor self -- )
    
    \ Run a tracker
    method run-tracker ( self -- )

    \ Stop a tracker
    method stop-tracker ( self -- )
    
  end-class

  \ Implement the <tracker> class
  <tracker> begin-implement

    \ Initialize the tracker
    :noname { self -- }
      self <object>->new
      0 self tracker-count !
      0 self tracker-first !
      false self tracker-enabled? !
      false self tracker-started? !
    ; define new

    \ Add a sensor to a tracker
    :noname { sensor self -- }
      self tracker-first @ sensor sensor-next !
      sensor self tracker-first !
      1 self tracker-count +!
    ; define add-tracker-sensor

    \ Run a tracker
    :noname { self -- }
      self tracker-started? @ not if
        true self tracker-started? !
        self 1 [: { self }
          begin
            self tracker-enabled? @ if
              self tracker-first @ begin ?dup while
                dup read-sensor
                sensor-next @
              repeat
              self tracker-count @ 1 = if
                104 ms
              then
            else
              pause
            then
          again
        ;] 320 128 512 spawn { tracker-task }
        1 tracker-task task-priority!
        tracker-task run
      then
      true self tracker-enabled? !
    ; define run-tracker

    \ Stop a tracker
    :noname { self -- }
      false self tracker-enabled? !
    ; define stop-tracker
    
  end-implement
  
  \ Our PWM slice
  <pwm-slice> class-size buffer: pwm-slice

  \ Our sensor
  <sensor> class-size buffer: sensor

  \ Our tracker
  <tracker> class-size buffer: tracker
  
  \ The number of columns
  128 constant my-width

  \ The number of rows
  64 constant my-height
  
  \ The width of a character
  7 constant my-char-width
  
  \ The height of a character
  7 constant my-char-height

  \ The readout string size
  12 constant readout-string-size
  
  \ Readout speed
  10,0 2constant readout-speed
  
  \ The <readout> class
  <object> begin-class <readout>
    
    \ Buffer to contain the readout string
    readout-string-size cell align member readout-string

    \ The SSD1306 display
    cell member readout-display
    
    \ The actual readout string length
    cell member readout-string-len
    
    \ The readout x coordinate
    2 cells member readout-x
    
    \ THe reading y coordinate
    2 cells member readout-y
    
    \ The readout x delta
    2 cells member readout-x-delta
    
    \ The readout y delta
    2 cells member readout-y-delta
    
    \ The last time the readout was updated
    cell member readout-last-systick

    \ Update a readout's value
    method update-readout ( self -- )

    \ Draw a readout
    method draw-readout ( self -- )

    \ Move a readout
    method move-readout ( self -- )

    \ Set the readout text
    method readout-text! ( c-addr u self -- )

    \ Append the readout text
    method append-readout-text! ( c-addr u self -- )
    
  end-class

  \ Generate a random fixed-point value in a range
  : random-fixed { max-lo max-hi -- d }
    random 0 max-lo max-hi f*
  ;
  
  \ Implement the <readout> class
  <readout> begin-implement

    \ Initialize a readout
    :noname { display init-chars self -- }
      self <object>->new
      display self readout-display !
      self readout-string readout-string-size $20 fill
      init-chars self readout-string-len !
      my-width init-chars my-char-width * - s>f random-fixed self readout-x 2!
      my-height my-char-height - s>f random-fixed self readout-y 2!
      pi 2,0 f* random-fixed { angle-lo angle-hi }
      angle-lo angle-hi cos readout-speed f* self readout-x-delta 2!
      angle-lo angle-hi sin readout-speed f* self readout-y-delta 2!
      -1 self readout-last-systick !
    ; define new

    \ Update a readout's value
    :noname { self -- }
    ; define update-readout
  
    \ Draw a readout
    :noname { self -- }
      self readout-string
      self readout-string-len @
      self readout-x 2@ f>s
      self readout-y 2@ f>s
      self readout-display @ a-simple-font xor-string
    ; define draw-readout
  
    \ Move a readout
    :noname { self -- }
      systick-counter { new-systick }
      self readout-last-systick @ -1 = if
        new-systick self readout-last-systick !
      then
      new-systick self readout-last-systick @ - s>f 10000,0 f/
      { systick-diff-lo systick-diff-hi }
      self readout-x-delta 2@ systick-diff-lo systick-diff-hi f*
      self readout-x 2+!
      self readout-y-delta 2@ systick-diff-lo systick-diff-hi f*
      self readout-y 2+!
      new-systick self readout-last-systick !
      self readout-x 2@ d0<= if
        self readout-x-delta 2@ dabs self readout-x-delta 2!
      else
        self readout-x 2@ self readout-string-len @ my-char-width * s>f d+
        my-width s>f d>= if
          self readout-x-delta 2@ dabs dnegate self readout-x-delta 2!
        then
      then
      self readout-y 2@ d0<= if
        self readout-y-delta 2@ dabs self readout-y-delta 2!
      else
        self readout-y 2@ my-char-height s>f d+ my-height s>f d>= if
          self readout-y-delta 2@ dabs dnegate self readout-y-delta 2!
        then
      then
    ; define move-readout

    \ Set the text of a readout
    :noname { c-addr u self -- }
      c-addr self readout-string u move
      u self readout-string-len !
    ; define readout-text!

    \ Append the text of a readout
    :noname { c-addr u self -- }
      c-addr self readout-string self readout-string-len @ + u move
      u self readout-string-len +!
    ; define append-readout-text!
  
  end-implement

  \ The <sensor-readout> class
  <readout> begin-class <sensor-readout>

    \ The sensor
    cell member readout-sensor

  end-class

  \ Implement the <sensor-readout> class
  <sensor-readout> begin-implement

    \ Initialize the <sensor-readout> instance
    :noname { display sensor self -- }
      display 7 self <readout>->new
      sensor self readout-sensor !
    ; define new
    
  end-implement
  
  \ The <readout-t> class, for displaying the raw temperature
  <sensor-readout> begin-class <readout-t> end-class
  
  \ Implement the <readout-t> class
  <readout-t> begin-implement

    \ Update the value of the readout
    :noname ( self -- )
      32 [:
        { self buf }
        buf self readout-sensor @ sensor-temp@
        format-integer self readout-text!
        s"  t" self append-readout-text!
      ;] with-allot
    ; define update-readout

  end-implement
  
  \ The <readout-c> class, for displaying the temperature in Celsius
  <sensor-readout> begin-class <readout-c> end-class
  
  \ Implement the <readout-c> class
  <readout-c> begin-implement

    \ Update the value of the readout
    :noname ( self -- )
      32 [:
        { self buf }
        buf self readout-sensor @ sensor-temp-c@
        2 format-fixed-truncate self readout-text!
        s"  C" self append-readout-text!
      ;] with-allot
    ; define update-readout

  end-implement

  \ The <readout-f> class, for displaying the temperature in Fahrenheit
  <sensor-readout> begin-class <readout-f> end-class
  
  \ Implement the <readout-f> class
  <readout-f> begin-implement

    \ Update the value of the readout
    :noname ( self -- )
      32 [:
        { self buf }
        buf self readout-sensor @ sensor-temp-f@
        2 format-fixed-truncate self readout-text!
        s"  F" self append-readout-text!
      ;] with-allot
    ; define update-readout

  end-implement

  \ Readout draw/move interval
  1000 constant readout-interval
  
  \ Our <temp-display> class
  <object> begin-class <temp-display>

    \ Our <ssd1306> instance
    <ssd1306> class-size member temp-ssd1306

    \ Our framebuffer
    my-width my-height bitmap-buf-size 4 align member temp-buf
  
    \ Our Celsius readout
    <readout-c> class-size member temp-readout-c

    \ Our Fahrenheit readout
    <readout-f> class-size member temp-readout-f

    \ Our raw temperature readout
    <readout-t> class-size member temp-readout-t

    \ Our display temperatures flag
    cell member temp-display?

    \ Start displaying temperatures
    method start-display-temps ( self -- )

    \ Stop displaying temperatures
    method stop-display-temps ( self -- )

    \ Our display temperatures method
    method display-temps ( self -- )

    \ Our execute for all readouts method
    method all-readouts ( self -- )

    \ Our cycle through readouts
    method cycle-readouts ( self -- )
    
  end-class

  \ Implement our <temp-display> class
  <temp-display> begin-implement

    \ Initialize the display
    :noname { sensor self -- }
      self <object>->new
      14 15 self temp-buf my-width my-height SSD1306_I2C_ADDR 1
      <ssd1306> self temp-ssd1306 init-object
      init-simple-font
      self temp-ssd1306 sensor <readout-c> self temp-readout-c init-object
      self temp-ssd1306 sensor <readout-f> self temp-readout-f init-object
      self temp-ssd1306 sensor <readout-t> self temp-readout-t init-object
      false self temp-display? !
      self 1 ['] display-temps 320 128 512 spawn run
    ; define new
    
    \ Call a method on each readout
    :noname { xt self -- }
      self temp-readout-c xt execute
      self temp-readout-f xt execute
      self temp-readout-t xt execute
    ; define all-readouts
  
    \ Display and run the readouts
    :noname { self -- }
      ['] update-readout self all-readouts
      ['] draw-readout self all-readouts
      self temp-ssd1306 update-display
      ['] draw-readout self all-readouts
      ['] move-readout self all-readouts
    ; define cycle-readouts

    \ Display temperatures repeatedly
    :noname { self -- }
      systick-counter { start-systick }
      start-systick { update-start-systick }
      begin
        self temp-display? @ if
          self cycle-readouts
        else
          self temp-ssd1306 clear-bitmap
          self temp-ssd1306 update-display
          pause
        then
        readout-interval start-systick current-task delay
        readout-interval +to start-systick
      again
    ; define display-temps

    \ Start displaying temperatures
    :noname { self -- }
      true self temp-display? !
    ; define start-display-temps
  
    \ Stop displaying temperatures
    :noname { self -- }
      false self temp-display? !
    ; define stop-display-temps

  end-implement

  \ Our maximum temperature log message size
  64 constant temp-log-msg-size
  
  \ Our <temp-log> class
  <object> begin-class <temp-log>

    \ Our sensor
    cell member temp-log-sensor

    \ Our temperature log filesystem
    <simple-fat32-fs> class-size member temp-log-fs
  
    \ Our temp log file
    <fat32-file> class-size member temp-log-file

    \ Our temp long file is open flag
    cell member temp-log-open?
    
    \ Our log message buffer
    temp-log-msg-size member temp-log-msg-buf

    \ Our log current date/time
    date-time-size member temp-log-current-date-time

    \ Our log alarm date/time
    date-time-size member temp-log-alarm-date-time
    
    \ Our log task
    cell member temp-log-task

    \ Our log is active
    cell member temp-log-active?

    \ Our log task's mailbox
    cell member temp-log-mailbox

    \ Our alarm handler closure
    closure-size member temp-log-alarm-handler

    \ Set the file to log to
    method open-temp-log ( path-addr path-len self -- )

    \ Start the log
    method start-temp-log ( self -- )

    \ Stop the log
    method stop-temp-log ( self -- )

    \ Carry out a logging event
    method do-log ( self -- )

    \ Initialize the alarm
    method init-alarm ( self -- )
    
  end-class

  \ Implement our <temp-log> class
  <temp-log> begin-implement

    \ Initialize our <temp-log> instance
    :noname { sensor self -- }
      self <object>->new
      sensor self temp-log-sensor !
      0 self temp-log-task !
      false self temp-log-open? !
      false self temp-log-active? !
      2 3 4 5 0 <simple-fat32-fs> self temp-log-fs init-object
      self temp-log-fs fat32-tools::current-fs!
    ; define new

    \ Set the file to log to
    :noname { path-addr path-len self -- }
      self temp-log-file path-addr path-len
      [: 3 roll swap open-file ;] fat32-tools::current-fs@ with-root-path
      0 seek-end self temp-log-file seek-file
      true self temp-log-open? !
    ; define open-temp-log

    \ Start the log
    :noname { self -- }
      self temp-log-task @ 0= if
        self 1 [: { self }
          begin
            self temp-log-active? @ self temp-log-open? @ and if
              0 wait-notify drop
              self temp-log-active? @ self temp-log-open? @ and if
                self do-log
              then
            else
              pause
            then
          again
        ;] 320 128 512 spawn self temp-log-task !
        self temp-log-mailbox 1 self temp-log-task @ config-notify
        self temp-log-task @ run
        self init-alarm
      then
      true self temp-log-active? !
    ; define start-temp-log

    \ Stop the log
    :noname { self -- }
      false self temp-log-active? !
    ; define stop-temp-log
    
    \ Carry out a logging event
    :noname { self -- }
      self temp-log-sensor @ sensor-temp@ { temp-raw }
      self temp-log-sensor @ sensor-temp-c@ { temp-c-lo temp-c-hi }
      self temp-log-sensor @ sensor-temp-f@ { temp-f-lo temp-f-hi }
      self temp-log-current-date-time date-time@
      self temp-log-msg-buf self temp-log-current-date-time
      format-date-time + { new-addr }
      s"   " new-addr swap move 2 +to new-addr
      new-addr temp-raw format-integer + to new-addr
      s"  t  " new-addr swap move 4 +to new-addr
      new-addr temp-c-lo temp-c-hi 2 format-fixed-truncate + to new-addr
      s"  C  " new-addr swap move 4 +to new-addr
      new-addr temp-f-lo temp-f-hi 2 format-fixed-truncate + to new-addr
      s"  F" new-addr swap move 2 +to new-addr
      $0D new-addr c! 1 +to new-addr
      $0A new-addr c! 1 +to new-addr
      self temp-log-msg-buf new-addr over - type
      self temp-log-msg-buf new-addr over - self temp-log-file write-file drop
      fat32-tools::current-fs@ flush
    ; define do-log

    \ Initialize the alarm
    :noname { self -- }
      self temp-log-alarm-date-time date-time@
      -1 self temp-log-alarm-date-time date-time-year !
      $FF self temp-log-alarm-date-time date-time-month c!
      $FF self temp-log-alarm-date-time date-time-day c!
      $FF self temp-log-alarm-date-time date-time-dotw c!
      $FF self temp-log-alarm-date-time date-time-hour c!
      self temp-log-alarm-date-time date-time-minute c@ 1+ 60 umod
      self temp-log-alarm-date-time date-time-minute c!
      $FF self temp-log-alarm-date-time date-time-second c!
      self self temp-log-alarm-handler [: { self }
        0 self temp-log-task @ notify
        self temp-log-alarm-date-time date-time-minute c@ 1+ 60 umod
        self temp-log-alarm-date-time date-time-minute c!
        self temp-log-alarm-date-time self temp-log-alarm-handler set-rtc-alarm
      ;] bind
      self temp-log-alarm-date-time self temp-log-alarm-handler set-rtc-alarm
    ; define init-alarm
    
  end-implement
  
  \ Our temperature display
  <temp-display> class-size aligned-buffer: temp-display

  \ Our temperature log
  <temp-log> class-size aligned-buffer: temp-log
  
  \ Initialize the temperature sensor
  : init-temp ( -- )
    pwm-wrap pwm-input-pin pwm-index <pwm-slice> pwm-slice init-object
    pwm-slice sensor-output-pin <sensor> sensor init-object
    <tracker> tracker init-object
    sensor tracker add-tracker-sensor
    tracker run-tracker
    sensor <temp-display> temp-display init-object
    sensor <temp-log> temp-log init-object
  ;
  
end-module