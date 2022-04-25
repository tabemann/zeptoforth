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
  lock import

  \ Our lock
  lock-size buffer: my-lock
  
  \ Our tasks
  variable my-task-1
  variable my-task-2

  \ Run the first task
  : do-task-1 ( -- )
    no-timeout timeout !
    [: cr ." Start wait 1" 50 ms cr ." End wait 1" ;] my-lock with-lock
    [: cr ." Start wait 3" 100 ms cr ." End wait 3" ;] my-lock with-lock
    [: cr ." Start wait 5" 300 ms cr ." End wait 5" ;] my-lock with-lock
    [: cr ." Start wait 7" 600 ms cr ." End wait 7" ;] my-lock with-lock
  ;

  \ Run the second task
  : do-task-2 ( -- )
    4500 timeout !
    25 ms
    [: cr ." Start wait 2" 100 ms cr ." End wait 2" ;] my-lock with-lock
    [: cr ." Start wait 4" 100 ms cr ." End wait 4" ;] my-lock with-lock
    [: cr ." Start wait 6" 100 ms cr ." End wait 6" ;] my-lock with-lock
    [: cr ." Start wait 8" 100 ms cr ." End wait 8" ;] my-lock with-lock
  ;

  \ Initialize our test
  : init-test ( -- )
    my-lock init-lock
    0 ['] do-task-1 420 128 512 spawn my-task-1 !
    0 ['] do-task-2 420 128 512 spawn my-task-2 !
    my-task-1 @ run
    my-task-2 @ run
  ;
  
end-module
