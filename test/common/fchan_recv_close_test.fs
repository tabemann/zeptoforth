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
  task-pool import

  \ Task count
  2 constant my-task-count

  \ The task pool
  my-task-count task-pool-size buffer: my-task-pool

  \ The fchannel
  fchan-size buffer: my-fchan

  \ The consumer
  : consumer ( -- )
    begin [: my-fchan recv-fchan ;] extract-allot-cell cr ." RECV: " . again
  ;

  \ The closer
  : closer ( -- )
    2000 ms my-fchan close-fchan
  ;
  
  \ The consumer task
  variable consumer-task

  \ The closer task
  variable closer-task
  
  \ Initialize the test
  : init-test ( -- )
    420 128 512 my-task-count my-task-pool init-task-pool
    my-fchan init-fchan
    0 ['] consumer my-task-pool spawn-from-task-pool consumer-task !
    0 ['] closer my-task-pool spawn-from-task-pool closer-task !
    consumer-task @ run
    closer-task @ run
    pause
  ;
    
end-module
