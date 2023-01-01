\ Copyright (c) 2022-2023 Travis Bemann
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
  fchan import

  \ Our fchannel
  fchan-size buffer: my-fchan

  \ Generate task index from priority
  : task-index ( priority -- index ) cpu-index 4 * + ;

  \ Our producers
  : producer ( delay-ms send-count priority -- )
    >r r@ current-task task-priority! r@ 500 * ms
    begin
      dup begin ?dup while
	r@ task-index [: my-fchan send-fchan ;] provide-allot-cell 1-
      repeat
      over ms
    again
  ;

  \ Our consumers
  : consumer ( delay-ms recv-count priority -- )
    >r r@ current-task task-priority! r@ 1+ 500 * ms
    begin
      dup begin ?dup while
	[: my-fchan recv-fchan ;] extract-allot-cell
	r@ task-index (.) ." :" . 1-
      repeat
      over ms
    again
  ;

  \ Initialize our test
  : init-test ( -- )
    my-fchan init-fchan
    3 current-task task-priority!
    1000 3000 0 3 ['] producer 420 128 512 0 spawn-on-core run
    1500 2000 0 3 ['] consumer 420 128 512 0 spawn-on-core run
    1000 3000 0 3 ['] producer 420 128 512 1 spawn-on-core run
    1500 2000 0 3 ['] consumer 420 128 512 1 spawn-on-core run
    1000 3000 1 3 ['] producer 420 128 512 0 spawn-on-core run
    1500 3000 1 3 ['] consumer 420 128 512 0 spawn-on-core run
    1000 3000 1 3 ['] producer 420 128 512 1 spawn-on-core run
    1500 3000 1 3 ['] consumer 420 128 512 1 spawn-on-core run

\    1000 3000 1 3 ['] producer 420 128 512 0 spawn-on-core run
\    1500 2000 1 3 ['] consumer 420 128 512 0 spawn-on-core run
\    1000 3000 1 3 ['] producer 420 128 512 1 spawn-on-core run
\    1500 2000 1 3 ['] consumer 420 128 512 1 spawn-on-core run
\    1000 3000 2 3 ['] producer 420 128 512 0 spawn-on-core run
\    1500 3000 2 3 ['] consumer 420 128 512 0 spawn-on-core run
\    1000 3000 2 3 ['] producer 420 128 512 1 spawn-on-core run
\    1500 3000 2 3 ['] consumer 420 128 512 1 spawn-on-core run

    0 current-task task-priority!
  ;
  
end-module