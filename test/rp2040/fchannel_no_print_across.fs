\ Copyright (c) 2021-2022 Travis Bemann
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
  int-io import

  \ The channel
  fchan-size buffer: my-fchan

  \ The task
  variable consumer-task
  variable producer-task

  \ The consumer
  : consumer ( -- )
    current-task consumer-task !
    begin
      [: my-fchan recv-fchan ;] extract-allot-cell drop
    again
  ;

  \ The producer
  : producer ( -- )
    0 begin
      dup [: my-fchan send-fchan ;] provide-allot-cell 1+
    again
  ;
  
  \ Initialize the test
  : init-test ( -- )
    disable-int-io
    my-fchan init-fchan
    0 consumer-task !
    0 ['] consumer 320 128 512 1 spawn-aux-main
    0 ['] producer 320 128 512 spawn producer-task !
    begin consumer-task @ until
    c" consumer" consumer-task @ task-name!
    c" producer" producer-task @ task-name!
    producer-task @ run
  ;

end-module
