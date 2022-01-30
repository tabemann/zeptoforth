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
  lock import

  \ Our fchannel
  fchan-size buffer: my-fchan

  \ Our lock
  lock-size buffer: my-lock

  \ Our tasks
  variable producer-task
  variable consumer-1-task
  variable consumer-2-task
  variable consumer-3-task
  variable consumer-4-task

  \ Run the producer
  : do-producer ( -- )
    no-timeout timeout !
    0 begin
      [: cr ." Producer: " dup . ;] my-lock with-lock
      dup [: my-fchan send-fchan ;] provide-allot-cell 1+
    again
  ;

  \ Run a consumer
  : do-consumer ( n -- )
    no-timeout timeout !
    begin
      [: my-fchan recv-fchan ;] extract-allot-cell
      [: cr ." Consumer" over . ." : " . ;] my-lock with-lock
    again
  ;

  \ Initialize our test
  : init-test ( -- )
    my-fchan init-fchan
    my-lock init-lock
    0 ['] do-producer 320 128 512 spawn producer-task !
    1 1 ['] do-consumer 320 128 512 spawn consumer-1-task !
    2 1 ['] do-consumer 320 128 512 spawn consumer-2-task !
    3 1 ['] do-consumer 320 128 512 spawn consumer-3-task !
    4 1 ['] do-consumer 320 128 512 spawn consumer-4-task !
    producer-task @ run
    consumer-1-task @ run
    consumer-2-task @ run
    consumer-3-task @ run
    consumer-4-task @ run
  ;

end-module
