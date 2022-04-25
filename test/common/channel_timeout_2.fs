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
  chan import

  \ Our channel
  1 cells 1 chan-size buffer: my-chan
  
  \ Our tasks
  variable my-task-1
  variable my-task-2
  variable my-task-3
  variable my-task-4

  \ Run the first task
  : do-task-1 ( -- )
    no-timeout timeout !
    cr ." Start wait 1" 1000 ms cr ." End wait 1"
    [: my-chan recv-chan ;] extract-allot-cell drop
    cr ." Done"
  ;

  \ Run the second task
  : do-task-2 ( -- )
    no-timeout timeout !
    25 ms
    0 [: my-chan send-chan ;] provide-allot-cell
    cr ." Sent 1"
  ;

  \ Run the third task
  : do-task-3 ( -- )
    5000 timeout !
    50 ms
    0 [: my-chan send-chan ;] provide-allot-cell
    cr ." Sent 2"
  ;

  \ Initialize our test
  : init-test ( -- )
    1 cells 1 my-chan init-chan
    0 [: my-chan send-chan ;] provide-allot-cell
    0 ['] do-task-1 420 128 512 spawn my-task-1 !
    0 ['] do-task-2 420 128 512 spawn my-task-2 !
    0 ['] do-task-3 420 128 512 spawn my-task-3 !
    my-task-1 @ run
    my-task-2 @ run
    my-task-3 @ run
  ;
  
end-module
