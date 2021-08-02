\ Copyright (c) 2020-2021 Travis Bemann
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

  import internal-module
  import task-module
  import fchan-module
  import lock-module

  \ Our channel
  1 cells fchan-size buffer: my-fchan

  \ Our output lock
  lock-size buffer: my-lock

  \ Our tasks
  variable consumer-task
  variable producer-a-task
  variable producer-b-task
  variable producer-c-task

  \ The inner loop of the consumer
  : consumer ( -- )
\    cr ." Consumer: " current-task h.8
    begin
      my-fchan recv-fchan-cell
      my-lock [: cr ." Received:" . ;] with-lock
      \    100 ms
      \    pause
    again
  ;

  \ The inner loop of a producer
  : producer ( -- )
\    cr ." Producer:" dup .
\    ." : " current-task h.8
    begin
      my-lock [: cr ." Sending: " dup . ;] with-lock
      dup my-fchan send-fchan-cell
      my-lock [: cr ." Done sending: " dup . ;] with-lock
    again
  ;

  \ Initiate the test
  : init-test ( -- )
    1 cells my-fchan init-fchan
    my-lock init-lock
    0 ['] consumer 512 256 256 spawn consumer-task !
    0 1 ['] producer 512 256 256 spawn producer-a-task !
    1 1 ['] producer 512 256 256 spawn producer-b-task !
    2 1 ['] producer 512 256 256 spawn producer-c-task !
    consumer-task @ run
    producer-a-task @ run
    producer-b-task @ run
    producer-c-task @ run
    pause
  ;

end-module