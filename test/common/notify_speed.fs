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

  systick import
  task import

  \ The two tasks
  variable target-task
  variable source-task

  \ The mailboxes for the two tasks
  1 cells buffer: target-mailboxes
  1 cells buffer: source-mailboxes
  
  \ The inner loop of the target
  : target ( -- )
    0 wait-notify-indefinite drop
    begin
      0 wait-notify-indefinite drop
      0 source-task @ notify
    again
  ;

  \ The target task
  0 ' target 480 128 512 spawn target-task !

  \ The send count
  variable send-count

  \ The starting systick
  variable start-systick

  \ The send count limit
  10000 constant send-count-limit

  \ The inner loop of a source
  : source ( -- )
    0 wait-notify-indefinite drop
    begin
      0 target-task @ notify
      0  wait-notify-indefinite drop
      1 send-count +!
      send-count @ send-count-limit > if
	0 send-count !
	systick-counter dup start-systick @ -
	cr ." Notification pairs per second: " 0 swap 0 send-count-limit f/
	10000,0 f/ 1,0 2swap f/ f.
	start-systick !
      then
    again
  ;

  \ The source task
  0 ' source 480 128 512 spawn source-task !

  \ Initiate the test
  : init-test ( -- )
    0 send-count !
    systick-counter start-systick !
    c" target" target-task @ task-name!
    c" source" source-task @ task-name!
    target-mailboxes 1 target-task @ config-notify
    source-mailboxes 1 source-task @ config-notify
    target-task @ run
    source-task @ run
    0 target-task @ notify
    0 source-task @ notify
    pause
  ;

end-module
