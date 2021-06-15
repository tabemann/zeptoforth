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

begin-module forth-module

  import task-module
  import fchan-module

  \ Allot the outgoing channel
  fchan-size buffer: my-out-fchan

  \ Allot the incoming channel
  fchan-size buffer: my-in-fchan

  \ The higher-priority task
  variable high-task

  \ The lower-priority task
  variable low-task

  \ The loop of the higher-priority task
  : high ( -- )
    begin
      ." < " 100000 0 ?do loop
      0 0 my-out-fchan send-fchan
      my-in-fchan recv-fchan 2drop
      ." > "
    again
  ;

  \ The loop of the lower-priority task
  : low ( -- )
    begin
      my-out-fchan recv-fchan 2drop
      ." [ " 10000 0 ?do loop ." ] "
      0 0 my-in-fchan send-fchan
    again
  ;

  \ Initialize the test
  : init-test ( -- )
    my-out-fchan init-fchan
    my-in-fchan init-fchan
    0 ['] high 512 256 256 spawn high-task !
    0 ['] low 512 256 256 spawn low-task !
    1 high-task @ set-task-priority
    0 low-task @ set-task-priority
    begin-critical
    high-task @ run
    low-task @ run
    end-critical
  ;

end-module
