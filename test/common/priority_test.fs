\ Copyright (c) 2020-2022 Travis Bemann
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

  \ The highest-priority task's loop
  : highest ( -- ) begin 25 0 ?do 100000 0 ?do loop ." *" loop 1000 ms again ;

  \ The middle-priority task's loop
  : middle ( -- ) begin 100 0 ?do 10000 0 ?do loop ." x" loop 500 ms again ;

  \ The lowest-priority task's loop
  : lowest ( -- ) begin 10000 0 ?do loop ." ." again ;

  \ The tasks
  variable highest-task
  variable middle-task
  variable lowest-task

  \ Initialize the test
  : init-test ( -- )
    0 ['] highest 420 128 512 spawn highest-task !
    0 ['] middle 420 128 512 spawn middle-task !
    0 ['] lowest 420 128 512 spawn lowest-task !
    -1 lowest-task @ task-priority!
    0 middle-task @ task-priority!
    1 highest-task @ task-priority!
    begin-critical
    lowest-task @ run
    middle-task @ run
    highest-task @ run
    end-critical
  ;

end-module
