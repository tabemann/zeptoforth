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

  \ Generate task index from priority
  : task-index ( priority -- index ) cpu-index 2 * + ;

  \ Our producer
  : producer ( priority -- )
    dup current-task task-priority! dup 1+ 500 * ms
    task-index 1
    begin 2dup [: my-fchan send-fchan ;] provide-allot-2cell 1+ again
  ;

  \ Our consumer
  : consumer ( priority -- )
    dup current-task task-priority! 1+ 500 * ms
    4 cells [:
      dup 4 cells 0 fill
      begin
	1000 0 ?do
	  [: my-fchan recv-fchan ;] extract-allot-2cell
	  2 pick 2 pick cells + 2dup @ <= if ." x" then ! drop
	loop
	." ."
      again
    ;] with-aligned-allot
  ;

  \ Initialize our test
  : init-test ( -- )
    my-fchan init-fchan
    3 current-task task-priority!
    0 1 ['] producer 480 128 512 0 spawn-on-core run
    0 1 ['] consumer 480 128 512 0 spawn-on-core run
    0 1 ['] producer 480 128 512 1 spawn-on-core run
    0 1 ['] consumer 480 128 512 1 spawn-on-core run
    1 1 ['] producer 480 128 512 0 spawn-on-core run
    1 1 ['] consumer 480 128 512 0 spawn-on-core run
    1 1 ['] producer 480 128 512 1 spawn-on-core run
    1 1 ['] consumer 480 128 512 1 spawn-on-core run
    0 current-task task-priority!
  ;
    
  
end-module