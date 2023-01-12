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

begin-module pwm-test

  pwm import
  pin import

  \ PWM output index
  3 constant pwm-out-index
  
  \ PWM input index
  4 constant pwm-in-index

  \ PWM output GPIO pin
  6 constant pwm-out-pin
  
  \ PWM input GPIO pin
  9 constant pwm-in-pin
  
  \ PWM output wrap
  3 constant pwm-out-wrap
  
  \ PWM input wrap
  65535 constant pwm-in-wrap
  
  \ PWM output compare value
  2 constant pwm-out-compare
  
  \ Our counter
  2variable counter
  
  \ Our interrupt handler
  : handle-pwm ( -- )
    pwm-in-wrap s>d counter 2+!
    pwm-in-index bit clear-pwm-int
    clear-pwm-pending
  ;
  
  \ Are we initialized
  false value inited?
  
  \ Run our test
  : run-test ( -- )
    inited? not if
      pwm-out-pin pull-up-pin
      pwm-out-pin fast-pin
      pwm-out-pin pwm-pin
      pwm-in-pin pull-up-pin
      pwm-in-pin fast-pin
      pwm-in-pin pwm-pin
      ['] handle-pwm pwm-vector!
      true to inited?
    then
    pwm-out-index bit pwm-in-index bit or disable-pwm
    pwm-in-index bit disable-pwm-int
    0. counter 2!
    0 pwm-out-index pwm-counter!
    0 pwm-in-index pwm-counter!
    pwm-out-wrap pwm-out-index pwm-top!
    pwm-in-wrap pwm-in-index pwm-top!
    pwm-out-compare pwm-out-index pwm-counter-compare-a!
    pwm-out-index free-running-pwm
    pwm-in-index rising-edge-pwm
    0 16 pwm-out-index pwm-clock-div!
    0 1 pwm-in-index pwm-clock-div!
    pwm-in-index bit enable-pwm-int
    pwm-out-index bit pwm-in-index bit or enable-pwm
    begin key? not while
      cr counter 2@ d.
      500 ms
    repeat
    key drop
  ;

end-module
