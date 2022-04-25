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
  task-pool import
  fchan import

  \ Allot the channels
  fchan-size buffer: signal-0-fchan
  fchan-size buffer: signal-1-fchan
  fchan-size buffer: signal-2-fchan
  fchan-size buffer: signal-3-fchan

  \ The task pool
  3 task-pool-size buffer: my-task-pool

  \ Wait on an fchannel, display a number of numbers and send on the fchannel
  \ provided
  : action ( out-fchannel xn...x0 count in-channel -- )
    [: rot recv-fchan ;] extract-allot-cell drop
    begin dup 0> while 1- swap . repeat drop pause
    0 [: rot send-fchan ;] provide-allot-cell
    begin pause again
  ;

  \ Initialize the test
  : init-test ( -- )
    signal-0-fchan init-fchan
    signal-1-fchan init-fchan
    signal-2-fchan init-fchan
    signal-3-fchan init-fchan
    420 128 512 3 my-task-pool init-task-pool
    signal-1-fchan 2 1 0 3 signal-0-fchan 6
    ['] action my-task-pool spawn-from-task-pool run
    signal-2-fchan 12 11 10 3 signal-1-fchan 6
    ['] action my-task-pool spawn-from-task-pool run
    signal-3-fchan 22 21 20 3 signal-2-fchan 6
    ['] action my-task-pool spawn-from-task-pool run
    0 [: signal-0-fchan send-fchan ;] provide-allot-cell
    [: signal-3-fchan recv-fchan ;] extract-allot-cell drop
  ;

end-module
