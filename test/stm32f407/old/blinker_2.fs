\ Copyright (c) 2020-2022 Travis Bemann
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

continue-module forth

  task import
  schedule import
  action-pool import
  led import

  \ The action count
  8 constant action-count
  
  \ The schedule
  schedule-size buffer: my-schedule

  \ The schedule task
  variable schedule-task

  \ The action pool
  action-count action-pool-size buffer: my-pool

  \ The actions
  variable red-blink-on-action
  variable orange-blink-on-action
  variable green-blink-on-action
  variable blue-blink-on-action
  variable red-blink-off-action
  variable orange-blink-off-action
  variable green-blink-off-action
  variable blue-blink-off-action
  
  \ Delay
  variable blink-delay

  \ Create a blink word
  : blink ( xt "name" -- )
    <builds , does>
    @ execute blink-delay @ current-action reset-action-delay
  ;

  \ Create the blink words
  ' led-red-on blink red-blink-on
  ' led-orange-on blink orange-blink-on
  ' led-green-on blink green-blink-on
  ' led-blue-on blink blue-blink-on
  ' led-red-off blink red-blink-off
  ' led-orange-off blink orange-blink-off
  ' led-green-off blink green-blink-off
  ' led-blue-off blink blue-blink-off

  \ Adjust blink timing
  : reset-blinking ( -- )
    0 red-blink-on-action @ start-action-delay
    blink-delay @ 4/ orange-blink-on-action @ start-action-delay
    blink-delay @ 2/ green-blink-on-action @ start-action-delay
    blink-delay @ 4/ 3 * blue-blink-on-action @ start-action-delay
    blink-delay @ 4/ red-blink-off-action @ start-action-delay
    blink-delay @ 2/ orange-blink-off-action @ start-action-delay
    blink-delay @ 4/ 3 * green-blink-off-action @ start-action-delay
    blink-delay @ blue-blink-off-action @ start-action-delay
  ;

  \ Init blinker
  : init-blinker-2 ( -- )
    4000 blink-delay !
    my-schedule init-schedule
    my-schedule action-count my-pool init-action-pool
    ['] red-blink-on my-pool init-from-action-pool red-blink-on-action !
    ['] orange-blink-on my-pool init-from-action-pool orange-blink-on-action !
    ['] green-blink-on my-pool init-from-action-pool green-blink-on-action !
    ['] blue-blink-on my-pool init-from-action-pool blue-blink-on-action !
    ['] red-blink-off my-pool init-from-action-pool red-blink-off-action !
    ['] orange-blink-off my-pool init-from-action-pool orange-blink-off-action !
    ['] green-blink-off my-pool init-from-action-pool green-blink-off-action !
    ['] blue-blink-off my-pool init-from-action-pool blue-blink-off-action !
    reset-blinking
    red-blink-on-action @ enable-action
    orange-blink-on-action @ enable-action
    green-blink-on-action @ enable-action
    blue-blink-on-action @ enable-action
    red-blink-off-action @ enable-action
    orange-blink-off-action @ enable-action
    green-blink-off-action @ enable-action
    blue-blink-off-action @ enable-action
    0 [: my-schedule run-schedule ;] 420 128 512 spawn schedule-task !
    schedule-task @ run
    pause
  ;

end-module
