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

\ Set up the wordlist
forth-wordlist 1 set-order
forth-wordlist set-current

\ Check whether this is already defined
defined? lock-wordlist not [if]

  \ Setup the wordlist
  wordlist constant lock-wordlist
  wordlist constant lock-internal-wordlist
  forth-wordlist task-wordlist lock-internal-wordlist lock-wordlist 4 set-order
  lock-wordlist set-current

  \ Lock header structure
  begin-structure lock-size

    \ Switch the current wordlist
    lock-internal-wordlist set-current

    \ Lock holder task
    field: lock-holder-task

    \ First lock wait
    field: lock-first-wait

    \ Last lock wait
    field: lock-last-wait

    \ Original holder priority
    field: lock-orig-holder-priority

    \ Current holder priority
    field: lock-current-holder-priority

  end-structure

  \ Lock wait structure
  begin-structure lock-wait-size

    \ Lock wait next record
    field: lock-wait-next

    \ Lock wait task
    field: lock-wait-task

    \ Original here position
    field: lock-wait-orig-here

  end-structure

  \ Set up a lock wait record
  : init-lock-wait ( -- wait )
    ram-here 4 ram-align, ram-here lock-wait-size ram-allot
    tuck lock-wait-orig-here !
    0 over lock-wait-next !
    current-task over lock-wait-task !
  ;

  \ Get maximum lock wait priority
  : max-lock-wait-priority ( lock -- priority )
    -32768 swap lock-first-wait @ begin dup while
      dup lock-wait-task @ get-task-priority rot max swap lock-wait-next @
    repeat
    drop
  ;

  \ Shift holder priority
  : shift-lock-holder-priority ( priority lock -- )
    dup lock-current-holder-priority @
    over lock-holder-task @ get-task-priority swap -
    over lock-orig-holder-priority +!
    2dup lock-holder-task @ set-task-priority
    lock-current-holder-priority !
  ;

  \ Adjust holder priority
  : adjust-lock-holder-priority ( lock -- )
    dup lock-first-wait @ 0<> over lock-holder-task @ 0<> and if
      dup max-lock-wait-priority dup
      2 pick lock-holder-task @ get-task-priority > if
	swap shift-lock-holder-priority
      else
	dup 2 pick lock-orig-holder-priority @ >=
	over 3 pick lock-holder-task @ get-task-priority < and if
	  swap shift-lock-holder-priority
	else
	  drop dup lock-orig-holder-priority @ swap shift-lock-holder-priority
	then
      then
    else
      drop
    then
  ;

  \ Add a lock wait record
  : add-lock-wait ( wait lock -- )
    dup lock-first-wait @ if
      2dup lock-last-wait @ lock-wait-next !
    else
      2dup lock-first-wait !
    then
    lock-last-wait !
  ;

  \ Switch the current wordlist
  lock-wordlist set-current

  \ Initialize a lock
  : init-lock ( addr -- )
    0 over lock-holder-task !
    0 over lock-first-wait !
    0 over lock-last-wait !
    0 over lock-orig-holder-priority !
    0 swap lock-current-holder-priority !
  ;

  \ Lock a lock
  : lock ( lock -- )
    begin-critical
    dup lock-holder-task @ if
      init-lock-wait
      2dup swap add-lock-wait
      swap adjust-lock-holder-priority
      current-task disable-task
      end-critical
      pause
      begin-critical
      lock-wait-orig-here @ ram-here!
    else
      current-task get-task-priority over lock-orig-holder-priority !
      dup lock-orig-holder-priority @ over lock-current-holder-priority !
      current-task over lock-holder-task !
      adjust-lock-holder-priority
    then
    end-critical
  ;

  \ Attempted to unlock a lock not owned by the current task
  : x-not-currently-owned ( -- ) space ." lock not owned by current task" cr ;

  \ Unlock a lock
  : unlock ( lock -- )
    begin-critical
    dup lock-holder-task @ current-task <> if
      end-critical ['] x-not-currently-owned ?raise
    then
    dup lock-holder-task @ get-task-priority
    over lock-current-holder-priority @ -
    over lock-orig-holder-priority @ +
    over lock-holder-task @ set-task-priority
    dup lock-first-wait @ if
      dup lock-first-wait @
      dup lock-wait-next @ 2 pick lock-first-wait !
      over lock-first-wait @ 0= if
	0 2 pick lock-last-wait !
      then
      lock-wait-task @
      dup enable-task
      dup get-task-priority 2 pick lock-orig-holder-priority !
      over lock-orig-holder-priority @ 2 pick lock-current-holder-priority !
      over lock-holder-task !
      adjust-lock-holder-priority
    else
      0 swap lock-holder-task !      
    then
    end-critical
  ;

[then]
    
