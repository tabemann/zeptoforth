\ Copyright (c) 2020 Travis Bemann
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

\ Set up the wordlist order
forth-wordlist task-wordlist systick-wordlist fchan-wordlist 4 set-order
forth-wordlist set-current

\ Allot the channel
fchan-size buffer: my-fchan

\ The inner loop of the consumer
: consumer ( -- )
  begin
    my-fchan recv-fchan-cell drop
    pause
  again
;

\ The consumer task
' consumer 256 256 256 spawn constant consumer-task

\ The send count
variable send-count

\ The starting systick
variable start-systick

\ The send count limit
10000 constant send-count-limit

\ The inner loop of a producer
: producer ( -- )
  begin
    0 my-fchan send-fchan-cell
    1 send-count +!
    send-count @ send-count-limit > if
      0 send-count !
      systick-counter dup start-systick @ -
      0 pause-enabled ! cr ." Sends per second: " 0 swap 0 send-count-limit f/
      10000,0 f/ 1,0 2swap f/ f. 1 pause-enabled !
      start-systick !
    then
  again
;

\ The producer task
' producer 256 256 256 spawn constant producer-task

\ Initiate the test
: init-test ( -- )
  0 send-count !
  systick-counter start-systick !
  my-fchan init-fchan
  consumer-task enable-task
  producer-task enable-task
  pause
;