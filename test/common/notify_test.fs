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

  import systick-module
  import task-module

  \ The two tasks
  variable consumer-task
  variable producer-task

  \ The mailboxes for the two tasks
  1 cells buffer: consumer-mailboxes
  1 cells buffer: producer-mailboxes
  
  \ The inner loop of the consumer
  : consumer ( -- )
    0 wait-notify-indefinite drop
    begin
      0 wait-notify-indefinite .
      0 producer-task @ notify
    again
  ;

  \ The consumer task
  0 ' consumer 320 128 512 spawn consumer-task !

  \ The send count
  variable send-count

  \ The starting systick
  variable start-systick

  \ The send count limit
  10000 constant send-count-limit

  \ The inner loop of a producer
  : producer ( -- )
    0 wait-notify-indefinite drop
    0 begin
      dup 0 consumer-task @ notify-set 1+
      0 wait-notify-indefinite drop
    again
  ;

  \ The producer task
  0 ' producer 320 128 512 spawn producer-task !

  \ Initiate the test
  : init-test ( -- )
    0 send-count !
    systick-counter start-systick !
    consumer-mailboxes 1 consumer-task @ config-notify
    producer-mailboxes 1 producer-task @ config-notify
    consumer-task @ run
    producer-task @ run
    0 consumer-task @ notify
    0 producer-task @ notify
    pause
  ;

end-module
