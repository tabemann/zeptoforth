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

  \ The start pause count
  variable start-pause-count

  \ The starting systick
  variable start-systick

  \ The send count limit
  10000 constant interval-pause-count-limit

  \ The inner loop of a counter
  : counter ( -- )
    begin
      pause
      pause-count start-pause-count @ - interval-pause-count-limit > if
	pause-count start-pause-count !
	systick-counter dup start-systick @ -
	cr ." Pauses per second: " 0 swap 0 interval-pause-count-limit f/
	10000,0 f/ 1,0 2swap f/ f.
	start-systick !
      then
    again
  ;

  \ The counter task
  0 ' counter 480 128 512 spawn constant counter-task

  \ Initiate the test
  : init-test ( -- )
    pause-count start-pause-count !
    systick-counter start-systick !
    counter-task run
    pause
  ;

end-module
