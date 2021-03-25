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

\ Set up the wordlist order
forth-wordlist task-wordlist 2 set-order
forth-wordlist set-current

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
  0 ['] highest 256 256 256 spawn highest-task !
  0 ['] middle 256 256 256 spawn middle-task !
  0 ['] lowest 256 256 256 spawn lowest-task !
  -1 lowest-task @ set-task-priority
  0 middle-task @ set-task-priority
  1 highest-task @ set-task-priority
  begin-critical
  lowest-task @ run
  middle-task @ run
  highest-task @ run
  end-critical
;
