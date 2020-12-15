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

\ Compile this to RAM
compile-to-ram

\ Set up the wordlist order
forth-wordlist 1 set-order
forth-wordlist set-current

\ Test whether the event scheduler has been compiled
defined? schedule-wordlist not [if]
  :noname space ." event scheduler not loaded" cr ; ?raise
[then]

\ Set up the wordlist order
forth-wordlist task-wordlist schedule-wordlist led-wordlist 4 set-order

\ Our task
variable schedule-task

\ Our schedule
schedule-size buffer: my-schedule

\ Our actions
action-size buffer: red-green-action
action-size buffer: orange-blue-action

\ Our delays
variable red-delay
variable orange-delay
variable green-delay
variable blue-delay

\ Red-green action word
: do-red-green ( -- )
  begin
    led-red-on led-green-off
    red-delay @ current-action reset-action-delay
    suspend
    led-green-on led-red-off
    green-delay @ current-action reset-action-delay
    suspend
  again
;

\ Orange-blue action word
: do-orange-blue ( -- )
  begin
    led-orange-on led-blue-off
    orange-delay @ current-action reset-action-delay
    suspend
    led-blue-on led-orange-off
    blue-delay @ current-action reset-action-delay
    suspend
  again
;

\ Initialize the test
: init-test ( -- )
  3333 red-delay !
  3333 orange-delay !
  3333 green-delay !
  3333 blue-delay !
  my-schedule init-schedule
  ['] do-red-green red-green-action my-schedule add-action
  ['] do-orange-blue orange-blue-action my-schedule add-action
  [: my-schedule run-schedule ;] 256 256 256 spawn schedule-task !
  1666 orange-blue-action start-action-delay
  red-green-action enable-action
  orange-blue-action enable-action
  schedule-task @ enable-task
  pause
;