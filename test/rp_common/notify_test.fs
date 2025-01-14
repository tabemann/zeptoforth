\ Copyright (c) 2025 Travis Bemann
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

begin-module notify-test

  task import
  timer import
  rng import
  
  \ Task notified
  variable notified-task
  
  \ Timer alarm 0 time in microseconds
  variable alarm-0-time

  \ Timer alarm 1 time in microseconds
  variable alarm-1-time

  \ Timer alarm 0 counter
  variable alarm-0-counter

  \ Timer alarm 1 counter
  variable alarm-1-counter

  \ Mailboxes
  2 cells buffer: mailboxes
  
  \ Timer alarm 0 interval
  100000 constant alarm-0-interval

  \ Timer alarm 1 interval
  100000 constant alarm-1-interval
  
  \ Handle a timer alarm 0 interrupt
  defer handle-alarm-0
  :noname
    0 clear-alarm-int
    1 alarm-0-counter +!
    alarm-0-counter @ 0 notified-task @ notify-set
    alarm-0-interval alarm-0-time +!
    alarm-0-time @ random 16 umod + ['] handle-alarm-0 0 set-alarm
  ; is handle-alarm-0

  \ Handle a timer alarm 1 interrupt
  defer handle-alarm-1
  :noname
    1 clear-alarm-int
    1 alarm-1-counter +!
    alarm-1-counter @ 1 notified-task @ notify-set
    alarm-1-interval alarm-1-time +!
    alarm-1-time @ random 16 umod + ['] handle-alarm-1 1 set-alarm
  ; is handle-alarm-1

  \ Check for notifications
  : check-notifications
    1 1 { alarm-0-value alarm-1-value }
    begin
      0 wait-notify dup alarm-0-value <> if
        cr ." alarm-0-value does not match; wait-notify: " dup .
        ." alarm-0-value: " alarm-0-value .
      else
        cr ." alarm-0-value matches; both: " dup .
      then
      1+ to alarm-0-value
      1 wait-notify dup alarm-1-value <> if
        cr ." alarm-1-value does not match; wait-notify: " dup .
        ." alarm-1-value: " alarm-1-value .
      else
        cr ." alarm-1-value matches; both: " dup .
      then
      1+ to alarm-1-value
    again
  ;
  
  \ Run the test
  : run-test ( -- )
    us-counter-lsb { current-time }
    0 alarm-0-counter !
    0 alarm-1-counter !
    current-time alarm-0-interval + alarm-0-time !
    current-time alarm-1-interval + alarm-1-time !
    0 ['] check-notifications 320 128 512 spawn notified-task !
    mailboxes 2 notified-task @ config-notify
    notified-task @ run
    alarm-0-time @ ['] handle-alarm-0 0 set-alarm
    alarm-1-time @ ['] handle-alarm-1 1 set-alarm
  ;
  
end-module
