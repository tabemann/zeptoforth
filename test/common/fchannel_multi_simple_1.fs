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
  import fchan-module
  import lock-module

  \ Our channel
  fchan-size buffer: my-fchan

  \ Our output lock
  lock-size buffer: my-lock

  \ The loop of the consumer
  : consumer ( -- )
    begin
      1000 timeout ! my-fchan recv-fchan-cell
      [: space ." out:" . ;] my-lock with-lock
    again
  ;

  \ The loop of a producer
  : producer ( c -- )
    0 begin
      [: space ." in: " over emit ." :" dup . ;] my-lock with-lock
      1000 timeout ! dup my-fchan send-fchan-cell
      [: space ." done: " over emit ." :" dup . ;] my-lock with-lock
      1+
    again
  ;

  \ Initialize
  my-fchan init-fchan
  my-lock init-lock
  0 ' consumer 512 256 256 spawn constant my-consumer-task
  char A 1 ' producer 512 256 256 spawn constant my-producer-a-task
  char B 1 ' producer 512 256 256 spawn constant my-producer-b-task
  char C 1 ' producer 512 256 256 spawn constant my-producer-c-task
  char D 1 ' producer 512 256 256 spawn constant my-producer-d-task
  my-consumer-task run
  my-producer-a-task run
  my-producer-b-task run
  my-producer-c-task run
  my-producer-d-task run

end-module