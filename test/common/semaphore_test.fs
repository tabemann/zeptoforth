\ Copyright (c) 2021-2023 Travis Bemann
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
  task-pool import
  lock import
  sema import

  \ Our task pool count
  9 constant my-task-count
  
  \ Our task pool
  my-task-count task-pool-size buffer: my-task-pool

  \ Our semaphore
  sema-size buffer: my-sema

  \ Our semaphore initial counter
  2 constant init-sema-counter

  \ Our lock to control IO
  lock-size buffer: my-lock
  
  \ Our action task main
  : do-action ( n -- )
    [: cr ." Taking semaphore: " dup . ;] my-lock with-lock
    my-sema take
    [: cr ." Took semaphore: " dup . ;] my-lock with-lock
    4000 ms
    [: cr ." Releasing semaphore: " dup . ;] my-lock with-lock
    my-sema give
  ;
  
  \ Our task creation task main
  : do-init-tasks ( -- )
    0 begin dup my-task-count 1- < while
      [: cr ." Creating task: " dup . ;] my-lock with-lock
      dup 1 ['] do-action my-task-pool spawn-from-task-pool run 1+ 1000 ms
    repeat
    drop
  ;
  
  \ Initialize our test
  : init-test ( -- )
    420 128 512 my-task-count my-task-pool init-task-pool
    my-lock init-lock
    no-sema-limit init-sema-counter my-sema init-sema
    0 ['] do-init-tasks my-task-pool spawn-from-task-pool run
  ;

end-module
