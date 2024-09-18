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

begin-module timer-test

  variable timer-alarm-0-count
  variable timer-alarm-1-count
  variable timer-alarm-2-count
  variable timer-alarm-3-count

  : us-counter-test ( -- )
    cr ." Before PAUSE-US"
    cr ." TIMER::US-COUNTER-LSB: " timer::us-counter-lsb .
    cr ." TIMER::US-COUNTER: " timer::us-counter d.
    cr ." After PAUSE-US and setting times to 1000"
    timer::pause-us
    1000. timer::us-counter!
    cr ." TIMER::US-COUNTER-LSB: " timer::us-counter-lsb .
    cr ." TIMER::US-COUNTER: " timer::us-counter d.
    cr ." After a short delay"
    1_000_000 0 do loop
    cr ." TIMER::US-COUNTER-LSB: " timer::us-counter-lsb .
    cr ." TIMER::US-COUNTER: " timer::us-counter d.
    cr ." After UNPAUSE-US and a short delay"
    timer::unpause-us
    1_000_000 0 do loop
    cr ." TIMER::US-COUNTER-LSB: " timer::us-counter-lsb .
    cr ." TIMER::US-COUNTER: " timer::us-counter d.
    cr ." After TIMER::US-COUNTER 1_000_000. D+ TIMER::DELAY-UNTIL-US"
    timer::us-counter 1_000_000. d+ timer::delay-until-us
    cr ." TIMER::US-COUNTER-LSB: " timer::us-counter-lsb .
    cr ." TIMER::US-COUNTER: " timer::us-counter d.
    cr ." After 1_000_000. TIMER::DELAY-UNTIL-US"
    1_000_000. timer::delay-us
    cr ." TIMER::US-COUNTER-LSB: " timer::us-counter-lsb .
    cr ." TIMER::US-COUNTER: " timer::us-counter d.
  ;

  : alarm-test ( -- )
    cr ." Setting alarms"
    0 timer-alarm-0-count !
    0 timer-alarm-1-count !
    0 timer-alarm-2-count !
    0 timer-alarm-3-count !
    timer::us-counter-lsb 1_000_000 + [:
      1 timer-alarm-0-count +!
      0 timer::clear-alarm-int
    ;] 0 timer::set-alarm
    timer::us-counter-lsb 1_100_000 + [:
      1 timer-alarm-1-count +!
      1 timer::clear-alarm-int
    ;] 1 timer::set-alarm
    timer::us-counter-lsb 1_200_000 + [:
      1 timer-alarm-2-count +!
      2 timer::clear-alarm-int
    ;] 2 timer::set-alarm
    timer::us-counter-lsb 1_300_000 + [:
      1 timer-alarm-3-count +!
      3 timer::clear-alarm-int
    ;] 3 timer::set-alarm
    2_700_000. timer::delay-us
    0 timer::clear-alarm
    1 timer::clear-alarm
    2 timer::clear-alarm
    3 timer::clear-alarm
    cr ." Cleared alarms, reading alarm counts"
    cr ." TIMER-ALARM-0-COUNT: " timer-alarm-0-count @ .
    cr ." TIMER-ALARM-1-COUNT: " timer-alarm-1-count @ .
    cr ." TIMER-ALARM-2-COUNT: " timer-alarm-2-count @ .
    cr ." TIMER-ALARM-3-COUNT: " timer-alarm-3-count @ . 
    2_700_000. timer::delay-us
    cr ." Reading alarm counts again"
    cr ." TIMER-ALARM-0-COUNT: " timer-alarm-0-count @ .
    cr ." TIMER-ALARM-1-COUNT: " timer-alarm-1-count @ .
    cr ." TIMER-ALARM-2-COUNT: " timer-alarm-2-count @ .
    cr ." TIMER-ALARM-3-COUNT: " timer-alarm-3-count @ . 
  ;
  
  : run-test ( -- )
    us-counter-test
    alarm-test
  ;

end-module
