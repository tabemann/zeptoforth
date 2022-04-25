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
  lock import

  \ Our locks
  lock-size buffer: lock-a
  lock-size buffer: lock-b

  \ Our tasks
  variable low-task
  variable middle-task
  variable high-task

  \ Our higher-priority task loop
  : high ( -- )
    begin
      5000 ms
      lock-a claim-lock
      5000 ms
      lock-a release-lock
    again
  ;

  \ Our middle-priority task loop
  : middle ( -- )
    begin
      2500 ms
      lock-a claim-lock
      2500 ms
      lock-b claim-lock
      2500 ms
      lock-b release-lock
      2500 ms
      lock-a release-lock
    again
  ;

  \ Our lower-priority task loop
  : low ( -- )
    begin
      lock-b claim-lock
      10 0 ?do
	1000 ms
	current-task task-priority@ .
      loop
      lock-b release-lock
    again
  ;

  \ Initialize the test
  : init-test ( -- )
    lock-a init-lock
    lock-b init-lock
    0 ['] low 420 128 512 spawn low-task !
    0 ['] middle 420 128 512 spawn middle-task !
    0 ['] high 420 128 512 spawn high-task !
    2 high-task @ task-priority!
    1 middle-task @ task-priority!
    0 low-task @ task-priority!
    begin-critical
    low-task @ run
    middle-task @ run
    high-task @ run
    end-critical
  ;
  
end-module
