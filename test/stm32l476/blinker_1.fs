\ Copyright (c) 2020 Travis Bemann
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

\ Set up the wordlist order
forth-wordlist task-wordlist schedule-wordlist led-wordlist 4 set-order
forth-wordlist set-current

\ The schedule
variable my-schedule

\ The schedule task
variable schedule-task

\ The red LED blinker action
variable red-blinker-action

\ The green LED blinker action
variable green-blinker-action

\ The red LED state variable
variable red-led-state

\ The green LED state variable
variable green-led-state

\ The red LED blinker delay variable
variable red-blinker-delay

\ The green LED blinker delay variable
variable green-blinker-delay

\ Blink the red LED
: red-blinker ( -- )
  red-led-state @ if
    led-red-off
  else
    led-red-on
  then
  red-led-state @ not red-led-state !
  red-blinker-delay @ current-action reset-action-delay
;

\ Blink the green LED
: green-blinker ( -- )
  green-led-state @ if
    led-green-off
  else
    led-green-on
  then
  green-led-state @ not green-led-state !
  green-blinker-delay @ current-action reset-action-delay
;

\ Run the schedule
: do-schedule ( -- )
  my-schedule @ run-schedule
;

\ Init
: init-blinker-1 ( -- )
  false red-led-state !
  false green-led-state !
  create-schedule my-schedule !
  3333 red-blinker-delay !
  ['] red-blinker my-schedule @ add-action red-blinker-action !
  red-blinker-delay @ red-blinker-action @ start-action-delay
  red-blinker-action @ enable-action
  5000 green-blinker-delay !
  ['] green-blinker my-schedule @ add-action green-blinker-action !
  green-blinker-delay @ green-blinker-action @ start-action-delay
  green-blinker-action @ enable-action
  ['] do-schedule 256 256 256 spawn schedule-task !
  schedule-task @ enable-task
  pause
;
