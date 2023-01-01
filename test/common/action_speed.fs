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
\ SOFTWARE

continue-module forth

  task import
  systick import
  action import

  \ Our schedule
  schedule-size buffer: my-schedule

  \ Our actions
  action-size buffer: consumer-action
  action-size buffer: producer-action

  \ The consumer buffer
  cell buffer: consumer-buf
  
  \ Our deferred consumer routine
  defer consumer

  \ The consumer routine
  : do-consumer ( -- )
    [: 2drop drop consumer ;] consumer-buf cell recv-action
  ;

  \ Set the deferred consumer routine
  ' do-consumer ' consumer defer!

  \ The producer buffer
  variable send-count
  
  \ The starting systick
  variable start-systick

  \ The send count limit
  10000 constant send-count-limit

  \ Our deferred producer routine
  defer producer

  \ The producer routine
  : do-producer ( -- )
    [:
      1 send-count +!
      send-count @ send-count-limit > if
	0 send-count !
	systick-counter dup start-systick @ -
	cr ." Sends per second: " 0 swap 0 send-count-limit f/
	10000,0 f/ 1,0 2swap f/ f.
	start-systick !
      then
      producer
    ;] send-count cell consumer-action send-action
  ;

  \ Set the deferred producer routine
  ' do-producer ' producer defer!

  \ Initialize the test
  : init-test ( -- )
    0 send-count !
    systick-counter start-systick !
    my-schedule init-schedule
    0 ['] consumer consumer-action init-action
    0 ['] producer producer-action init-action
    my-schedule consumer-action add-action
    my-schedule producer-action add-action
    0 [: my-schedule run-schedule ;] 420 128 512 spawn run
  ;
  
end-module