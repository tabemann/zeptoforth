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
  import chan-module
  import task-pool-module

  \ Task count
  3 constant my-task-count

  \ Channel size
  16 constant my-chan-size
  
  \ The task pool
  create my-task-pool my-task-count task-pool-size allot

  \ The channel
  create my-chan my-chan-size chan-size allot

  \ The producer
  : producer ( -- )
    0 begin
      dup $FF and dup cr ." SEND: " .
      my-chan send-chan-byte 1+ 500 ms
    again
  ;

  \ The consumer
  : consumer ( -- )
    4000 ms
    begin my-chan recv-chan-byte cr ." RECV: " . 500 ms again
  ;

  \ The closer
  : closer ( -- )
    8000 ms my-chan close-chan
  ;

  \ The producer task
  variable producer-task

  \ The consumer task
  variable consumer-task

  \ The closer task
  variable closer-task
  
  \ Initialize the test
  : init-test ( -- )
    512 256 256 my-task-count my-task-pool init-task-pool
    my-chan my-chan-size init-chan
    0 ['] producer my-task-pool spawn-from-task-pool producer-task !
    0 ['] consumer my-task-pool spawn-from-task-pool consumer-task !
    0 ['] closer my-task-pool spawn-from-task-pool closer-task !
    producer-task @ run
    consumer-task @ run
    closer-task @ run
    pause
  ;
    
end-module
