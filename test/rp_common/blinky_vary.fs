\ Copyright (c) 2024 Travis Bemann
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

begin-module blinky-vary-test

  variable last-blink-us
  variable last-blink-angle-us
  variable blink-interval-offset
  2variable blink-angle
  125_000 constant blink-angle-interval
  500_000 constant blink-interval
  250_000,0 2constant blink-multiplier
  pi 64,0 f/ 2constant blink-angle-offset

  defer do-blink ( -- )
  :noname
    0 timer::clear-alarm-int
    led::green led::toggle-led
    blink-interval blink-interval-offset @ + { interval }
    last-blink-us @ interval + timer::us-counter-lsb -
    interval 2 / > if
      interval last-blink-us +!
    else
      interval timer::us-counter-lsb + last-blink-us !
    then
    last-blink-us @ ['] do-blink 0 timer::set-alarm
  ; is do-blink

  defer do-blink-angle ( -- )
  :noname
    1 timer::clear-alarm-int
    blink-angle 2@ blink-angle-offset d+
    2dup [ pi 2,0 f* swap ] literal literal d> if
      [ pi 2,0 f* swap ] literal literal d-
    then
    2dup blink-angle 2!
    sin blink-multiplier f* round-half-even blink-interval-offset !
    last-blink-angle-us @ blink-angle-interval + timer::us-counter-lsb -
    blink-angle-interval 2 / > if
      blink-angle-interval last-blink-angle-us +!
    else
      blink-angle-interval timer::us-counter-lsb + last-blink-angle-us !
    then
    last-blink-angle-us @ ['] do-blink-angle 1 timer::set-alarm
  ; is do-blink-angle

  : run-test ( -- )
    0,0 blink-angle 2!
    0 blink-interval-offset !
    timer::us-counter-lsb blink-interval + last-blink-us !
    last-blink-us @ ['] do-blink 0 timer::set-alarm
    timer::us-counter-lsb blink-angle-interval + last-blink-angle-us !
    last-blink-angle-us @ ['] do-blink-angle 1 timer::set-alarm
  ;

end-module
