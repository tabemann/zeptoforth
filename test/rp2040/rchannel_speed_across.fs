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

  systick import
  task import
  rchan import

  \ Allot the channel
  rchan-size buffer: my-rchan

  \ The tasks
  variable consumer-task
  variable producer-task

  \ The inner loop of the consumer
  : consumer ( -- )
    begin
      [: my-rchan recv-rchan ;] extract-allot-cell drop
      0 [: my-rchan reply-rchan ;] provide-allot-cell
    again
  ;

  \ The send count
  variable send-count

  \ The starting systick
  variable start-systick

  \ The send count limit
  1000 constant send-count-limit

  \ The inner loop of a producer
  : producer ( -- )
    begin
      0 [: [: my-rchan send-rchan ;] extract-allot-cell ;] provide-allot-cell
      drop
      1 send-count +!
      send-count @ send-count-limit > if
	0 send-count !
	systick-counter dup start-systick @ -
	cr ." Sends per second: " 0 swap 0 send-count-limit f/
	10000,0 f/ 1,0 2swap f/ f.
	start-systick !
      then
    again
  ;

  \ Initiate the test
  : init-test ( -- )
    0 send-count !
    systick-counter start-systick !
    my-rchan init-rchan
    0 ['] consumer 420 128 512 1 spawn-on-core consumer-task !
    0 ['] producer 420 128 512 spawn producer-task !
    consumer-task @ run
    producer-task @ run
    pause
  ;

end-module
