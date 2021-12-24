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

continue-module forth

  task import
  fchan import
  lock import

  \ Our channel
  fchan-size buffer: my-fchan

  \ Our output lock
  lock-size buffer: my-lock

  \ Our tasks
  variable consumer-task
  variable producer-a-task
  variable producer-b-task
  variable producer-c-task

  \ The inner loop of the consumer
  : consumer ( -- )
    begin
      [: my-fchan recv-fchan ;] extract-allot-cell
      [: cr ." Received:" . ;] my-lock with-lock
    again
  ;

  \ The inner loop of a producer
  : producer ( -- )
    begin
      [: cr ." Sending: " dup . ;] my-lock with-lock
      dup [: my-fchan send-fchan ;] provide-allot-cell
\      [: cr ." Done sending: " dup . ;] my-lock with-lock
    again
  ;

  \ Initiate the test
  : init-test ( -- )
    my-fchan init-fchan
    my-lock init-lock
    0 ['] consumer 320 128 512 spawn consumer-task !
    0 1 ['] producer 320 128 512 spawn producer-a-task !
    1 1 ['] producer 320 128 512 spawn producer-b-task !
    2 1 ['] producer 320 128 512 spawn producer-c-task !
    consumer-task @ run
    producer-a-task @ run
    producer-b-task @ run
    producer-c-task @ run
    pause
  ;

end-module