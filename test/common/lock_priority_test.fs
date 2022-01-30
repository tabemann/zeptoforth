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

  \ Our lock
  lock-size buffer: my-lock

  \ Our tasks
  variable low-task
  variable high-task

  \ Our higher-priority task loop
  : high ( -- )
    begin
      500000 0 ?do loop
      my-lock lock
      500000 0 ?do loop
      [: my-lock unlock ;] try if ." A " then
    again
  ;

  \ Our lower-priority task loop
  : low ( -- )
    begin
      my-lock lock
      10 0 ?do
	50000 0 ?do loop
	current-task task-priority@ . space
      loop
      [: my-lock unlock ;] try if ." B " then
    again
  ;

  \ Initialize the test
  : init-test ( -- )
    my-lock init-lock
    0 ['] low 320 128 512 spawn low-task !
    0 ['] high 320 128 512 spawn high-task !
    1 high-task @ task-priority!
    low-task @ run
    500000 0 ?do loop
    high-task @ run
  ;

end-module
