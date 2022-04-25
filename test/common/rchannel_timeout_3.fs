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
  rchan import

  \ Our rchannel
  rchan-size buffer: my-rchan
  
  \ Our tasks
  variable my-task-1
  variable my-task-2
  variable my-task-3
  variable my-task-4

  \ Run the first task
  : do-task-1 ( -- )
    no-timeout timeout !
    cr ." Receive 1"
    [: my-rchan recv-rchan ;] extract-allot-cell
    [: my-rchan reply-rchan ;] provide-allot-cell
    cr ." Receive 2"
    [: my-rchan recv-rchan ;] extract-allot-cell
    [: my-rchan reply-rchan ;] provide-allot-cell
    5000 timeout !
    cr ." Receive 3"
    [: my-rchan recv-rchan ;] extract-allot-cell
    [: my-rchan reply-rchan ;] provide-allot-cell
    cr ." Done"
  ;

  \ Run the second task
  : do-task-2 ( -- )
    no-timeout timeout !
    25 ms
    cr ." Sending 1"
    0 [: [: my-rchan send-rchan ;] extract-allot-cell ;] provide-allot-cell
    cr ." Sent 1"
  ;

  \ Run the third task
  : do-task-3 ( -- )
    no-timeout timeout !
    50 ms
    cr ." Sending 2"
    0 [: [: my-rchan send-rchan ;] extract-allot-cell ;] provide-allot-cell
    cr ." Sent 2"
  ;

  \ Run the fourth task
  : do-task-4 ( -- )
    no-timeout timeout !
    1000 ms
    cr ." Sending 3"
    0 [: [: my-rchan send-rchan ;] extract-allot-cell ;] provide-allot-cell
    cr ." Sent 3"
  ;

  \ Initialize our test
  : init-test ( -- )
    my-rchan init-rchan
    0 ['] do-task-1 420 128 512 spawn my-task-1 !
    0 ['] do-task-2 420 128 512 spawn my-task-2 !
    0 ['] do-task-3 420 128 512 spawn my-task-3 !
    0 ['] do-task-4 420 128 512 spawn my-task-4 !
    my-task-1 @ run
    my-task-2 @ run
    my-task-3 @ run
    my-task-4 @ run
  ;
  
end-module
