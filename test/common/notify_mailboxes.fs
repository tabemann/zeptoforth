\ Copyright (c) 2021 Travis Bemann
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

begin-module forth-module

  import task-module

  \ The two tasks
  variable consumer-task
  variable producer-task

  \ The mailboxes for the two tasks
  2 cells buffer: consumer-mailboxes
  1 cells buffer: producer-mailboxes
  
  \ The inner loop of the consumer
  : consumer ( -- )
    0 wait-notify-indefinite drop
    begin
      0 wait-notify-indefinite .
      1 wait-notify-indefinite .
      0 producer-task @ notify
    again
  ;

  \ The consumer task
  0 ' consumer 320 128 512 spawn consumer-task !

  \ The inner loop of a producer
  : producer ( -- )
    0 wait-notify-indefinite drop
    0 begin
      dup 0 consumer-task @ notify-set 1+
      dup 1 consumer-task @ notify-set 1+
      0 wait-notify-indefinite drop
    again
  ;

  \ The producer task
  0 ' producer 320 128 512 spawn producer-task !

  \ Initiate the test
  : init-test ( -- )
    c" consumer" consumer-task @ set-task-name
    c" producer" producer-task @ set-task-name
    consumer-mailboxes 2 consumer-task @ config-notify
    producer-mailboxes 1 producer-task @ config-notify
    consumer-task @ run
    producer-task @ run
    0 consumer-task @ notify
    0 producer-task @ notify
    pause
  ;

end-module
