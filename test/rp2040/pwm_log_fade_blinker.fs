\ Copyright (c) 2023 Travis Bemann
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

begin-module shade-led

  pwm import
  timer import
  
  \ The blinker maximum input shade
  variable shade-max-input-shade

  \ The blinker maximum shade
  variable shade-max-shade
  
  \ The blinker shading
  variable shade-shade

  \ The blinker shade step delay in 100 us increments
  variable shade-step-delay

  \ The blinker multiplier
  2variable shade-multiply

  \ The blinker pre-multiplier
  2variable shade-premultiply

  \ The blinker shade conversion routine
  : convert-shade ( i -- shade )
    s>f shade-max-input-shade @ s>f f/ pi f* cos dnegate 1,0 d+ 2,0 f/
    shade-premultiply 2@ f* expm1 shade-premultiply 2@ expm1 f/
    shade-multiply 2@ f* f>s
  ;
  
  \ Shade increment
  variable shade-increment
  
  \ Shade level
  variable shade-level
  
  \ Maximum shade level
  variable max-shade-level
  
  \ Alarm interval
  variable alarm-interval
  
  \ PWM slice
  4 constant pwm-slice
  
  \ Alarm handler
  defer handle-alarm
  :noname ( -- )
    0 clear-alarm-int
    [:
      shade-level @ 0<= if
        shade-increment @ abs shade-increment !
      else
        shade-level @ shade-max-input-shade @ >= if
          shade-increment @ abs negate shade-increment !
        then
      then
      shade-increment @ shade-level +!
      shade-level @ convert-shade pwm-slice pwm-counter-compare-b!
    ;] try ?execute
    us-counter-lsb alarm-interval @ +  ['] handle-alarm 0 set-alarm
  ; ' handle-alarm defer!

  \ Shade an LED
  : run-shade-led ( -- )
    pwm-slice bit disable-pwm
    125 shade-max-input-shade !
    125,0 shade-multiply 2!
    15,0 shade-premultiply 2!
    2500 alarm-interval !
    0 shade-level !
    1 shade-increment !
    shade-max-input-shade @ convert-shade max-shade-level !
    25 pwm-pin
    0 pwm-slice pwm-counter!
    max-shade-level @ pwm-slice pwm-top!
    0 255 pwm-slice pwm-clock-div!
    true pwm-slice pwm-phase-correct!
    pwm-slice bit enable-pwm
    us-counter-lsb alarm-interval @ + ['] handle-alarm 0 set-alarm
  ;

end-module