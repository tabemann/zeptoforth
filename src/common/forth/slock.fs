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

compile-to-flash

begin-module slock

  task import
  multicore import

  begin-module slock-internal
  
    begin-structure slock-size
      
      \ The claiming task
      field: slock-task

    end-structure
    
  end-module> import

  export slock-size

  \ Initialize a simple lock
  : init-slock ( addr -- ) 0 swap ! ;

  \ Claim a simple lock
  : claim-slock ( slock -- )
    begin-critical slock-spinlock claim-spinlock begin
      dup slock-task @ ?dup if
	^ task-internal :: task-waited-for !
	slock-spinlock ^ task-internal :: spinlock-to-claim !
	-1 ^ task-internal :: task-ready-count +!
	^ task-internal :: task-ready-count @ 0< if
	  slock-spinlock ^ task-internal :: spinlock-to-claim !
	  [ ^ task-internal :: blocked-indefinite
	  ^ task-internal :: schedule-critical or
	  ^ task-internal :: schedule-with-spinlock or ] literal
	else
	  [ ^ task-internal :: readied
	  ^ task-internal :: schedule-critical or
	  ^ task-internal :: schedule-with-spinlock or ] literal
	then
	current-task ^ task-internal :: task-state h!
	slock-spinlock release-spinlock end-critical
	pause
	slock-spinlock claim-spinlock
	false
      else
	current-task swap slock-task !
	slock-spinlock release-spinlock end-critical
	true
      then
    until
  ;

\  \ Claim a simple lock
\  : claim-slock ( slock -- )
\    begin-critical claim-all-core-spinlock begin
\      dup slock-task @ ?dup if
\	^ task-internal :: task-waited-for !
\	-1 ^ task-internal :: task-ready-count +!
\	^ task-internal :: task-ready-count @ 0< if
\	  [ ^ task-internal :: blocked-indefinite
\	  ^ task-internal :: schedule-critical or ] literal
\	  current-task ^ task-internal :: task-state h!
\	  release-all-core-spinlock end-critical
\	  pause
\	  claim-all-core-spinlock
\	else
\	  [ ^ task-internal :: schedule-critical ] literal
\	  current-task ^ task-internal :: task-state h!
\	  release-all-core-spinlock end-critical
\	  pause
\	  claim-all-core-spinlock
\	then
\	false
\      else
\	current-task swap slock-task ! release-all-core-spinlock end-critical
\	true
\      then
\    until
\  ;

  \ Release a simple lock
  : release-slock ( slock -- )
    begin-critical slock-spinlock claim-spinlock claim-all-core-spinlock
    0 swap ! wake-other-tasks
    release-all-core-spinlock slock-spinlock release-spinlock end-critical
  ;

  \ Claim and release a simple lock while properly handling exceptions
  : with-slock ( xt slock -- )
    >r r@ claim-slock try r> release-slock ?raise
  ;

end-module

reboot