\ Copyright (c) 2022 Travis Bemann
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

  \ Task count arrays
  16 cells buffer: task-counts

  \ Current task count array
  variable task-counts-current

  \ Generate task index from priority
  : task-index ( priority -- index ) cpu-index 2 * + ;

  \ Increment producer counter
  : producer-count+ ( priority -- )
    1 swap task-index cells task-counts-current @ + +!
  ;

  \ Increment consumer counter
  : consumer-count+ ( priority -- )
    1 swap task-index 4 + cells task-counts-current @ + +!
  ;

  \ Swap task count arrays
  : swap-counts ( -- )
    task-counts-current @ task-counts = if
      task-counts 8 cells +
    else
      task-counts
    then
    dup 8 cells 0 fill
    task-counts-current !
  ;

  \ Print counts
  : print-counts ( -- )
    task-counts-current @
    swap-counts
    cr ." 0> " dup 0 cells + @ .
    ." 1> " dup 1 cells + @ .
    ." 2> " dup 2 cells + @ .
    ." 3> " dup 3 cells + @ .
    ." >0 " dup 4 cells + @ .
    ." >1 " dup 5 cells + @ .
    ." >2 " dup 6 cells + @ .
    ." >3 " 7 cells + @ .
  ;

  \ Our outputter
  : output ( -- )
    3 current-task task-priority!
    begin 1000 ms print-counts again
  ;
  
  \ Our producers
  : producer ( delay-ms send-count priority -- )
    >r r@ current-task task-priority! r@ 500 * ms
    begin
      dup begin ?dup while
	r@ task-index [: my-fchan send-fchan ;] provide-allot-cell 1-
	r@ producer-count+
      repeat
      over ms
    again
  ;

  \ Our consumers
  : consumer ( delay-ms recv-count priority -- )
    >r r@ current-task task-priority! r@ 1+ 500 * ms
    begin
      dup begin ?dup while
	[: my-fchan recv-fchan ;] extract-allot-cell drop 1-
	r@ consumer-count+
      repeat
      over ms
    again
  ;

  \ Initialize our test
  : init-test ( -- )
    task-counts 16 cells 0 fill
    task-counts task-counts-current !
    my-fchan init-fchan
    4 current-task task-priority!
    0 ['] output 320 128 512 0 spawn-on-core run
    333 1000 0 3 ['] producer 320 128 512 0 spawn-on-core run
    500 1000 0 3 ['] consumer 320 128 512 0 spawn-on-core run
    333 1000 0 3 ['] producer 320 128 512 1 spawn-on-core run
    500 1000 0 3 ['] consumer 320 128 512 1 spawn-on-core run
    333 1000 1 3 ['] producer 320 128 512 0 spawn-on-core run
    500 1000 1 3 ['] consumer 320 128 512 0 spawn-on-core run
    333 1000 1 3 ['] producer 320 128 512 1 spawn-on-core run
    500 1000 1 3 ['] consumer 320 128 512 1 spawn-on-core run
    0 current-task task-priority!
  ;
  
end-module