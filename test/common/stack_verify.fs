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

begin-module stack-verify

  task import
  action import

  begin-module stack-verify-internal

    \ Our marker value
    $DEADBEEF constant verify-marker
    
    \ Our stack verifier schedule
    schedule-size buffer: verify-schedule

    \ Our stack verifier task
    variable verify-task

    \ The inner meat of stack verification
    defer do-verify
    :noname
      true
      current-data ['] task-stack-end for-task@ @ verify-marker <> if
	display-red current-data h.8 ." : stack violation" display-normal cr
	drop false
      then
      current-data ['] task-rstack-end for-task@ @ verify-marker <> if
	display-red current-data h.8 ." : rstack violation" display-normal cr
	drop false
      then
      if ['] do-verify yield-action else current-data kill then
      pause
    ; ' do-verify defer!
    
  end-module> import

  \ Initialize the stack verifier
  : init-stack-verify ( -- )
    verify-schedule init-schedule
    verify-schedule 1 ['] run-schedule 256 128 512 spawn verify-task !
  ;

  \ Start the stack verifier
  : start-stack-verify ( -- ) verify-task @ run ;

  \ Stop the stack verifier
  : stop-stack-verify ( -- ) verify-task @ stop ;

  \ Add a stack verifier for a task
  : add-stack-verify ( task -- )
    dup ['] task-stack-end for-task@ verify-marker swap !
    dup ['] task-rstack-end for-task@ verify-marker swap !
    ram-here 4 align dup ram-here! >r
    action-size ram-allot
    ['] do-verify r@ init-action
    verify-schedule r> add-action
  ;
  
end-module