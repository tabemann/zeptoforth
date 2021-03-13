\ Copyright (c) 2020-2021 Travis Bemann
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

  \ Compile to flash
  compile-to-flash
  
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

    \ The next lock held by the holder task
    field: lock-next-held

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

  \ Get group maximum lock wait priority
  : group-max-lock-wait-priority ( lock -- priority )
    -32768 >r
    dup lock-next-held @ if
      dup begin
	r> over max-lock-wait-priority max >r
	dup lock-next-held @ 2 pick <> if
	  lock-next-held @ false
	else
	  true
	then
      until
      2drop
    else
      max-lock-wait-priority >r
    then
    r>
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

  \ Get the last lock held in a loop
  : get-last-lock ( lock -- )
    dup lock-next-held @ if
      dup begin
	dup lock-next-held @ 2 pick <> if
	  lock-next-held @ false
	else
	  true
	then
      until
      nip
    then
  ;

  \ Add a holder to a lock loop
  : add-lock ( lock task -- )
    swap over ['] current-lock-held for-task @ ?dup if
      over 3 roll ['] current-lock-held for-task !
      dup get-last-lock
      2 pick swap lock-next-held !
      swap lock-next-held !
    else
      over get-task-priority 2 pick set-task-saved-priority
      tuck swap ['] current-lock-held for-task !
      dup lock-next-held !
    then
  ;

  \ Remove a lock from a lock loop
  : remove-lock ( lock -- )
    dup lock-next-held @ if
      dup get-last-lock dup 2 pick <> if
	over lock-next-held @ swap lock-next-held !
	dup lock-next-held @ over lock-holder-task @
	['] current-lock-held for-task !
	0 swap lock-next-held !
      else
	drop 0 over lock-next-held !
	0 swap lock-holder-task @ ['] current-lock-held for-task !
      then
    then
  ;

  \ Resolve task priority on holding a lock
  : update-hold-priority ( lock -- )
    dup lock-next-held @ if
      dup group-max-lock-wait-priority
      over lock-holder-task @ get-task-saved-priority max
      over lock-holder-task @ set-task-priority
      lock-holder-task @ ['] current-lock for-task @ ?dup if
	recurse
      then
    then
  ;

  \ Resolve task priority on releasing a lock
  : update-release-priority ( lock -- )
    dup lock-next-held @ if
      dup get-last-lock over <> if
	dup lock-next-held @ swap remove-lock
	dup group-max-lock-wait-priority
	over lock-holder-task @ get-task-saved-priority max
	swap lock-holder-task @ set-task-priority
      else
	dup remove-lock lock-holder-task @ dup get-task-saved-priority
	swap set-task-priority
      then
    else
      lock-holder-task @ dup get-task-saved-priority
      swap set-task-priority
    then
  ;
  
  \ Switch the current wordlist
  lock-wordlist set-current

  \ Initialize a lock
  : init-lock ( addr -- )
    0 over lock-holder-task !
    0 over lock-first-wait !
    0 over lock-last-wait !
    0 swap lock-next-held !
  ;

  \ Lock a lock
  : lock ( lock -- )
    begin-critical
    dup lock-holder-task @ if
      dup current-task ['] current-lock for-task !
      init-lock-wait
      2dup swap add-lock-wait
      swap update-hold-priority
      current-task stop
      end-critical
      pause
      begin-critical
      lock-wait-orig-here @ ram-here!
    else
      dup current-task add-lock
      current-task over lock-holder-task !
      update-hold-priority
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
    dup update-release-priority
    dup lock-first-wait @ if
      dup lock-first-wait @
      dup lock-wait-next @ 2 pick lock-first-wait !
      over lock-first-wait @ 0= if
	0 2 pick lock-last-wait !
      then
      lock-wait-task @
      2dup add-lock
      2dup swap lock-holder-task !
      0 over ['] current-lock for-task !
      swap update-hold-priority
      run
    else
      0 swap lock-holder-task !      
    then
    end-critical
  ;

  \ Update the priorities of tasks holding locks
  : update-lock-priority ( lock -- )
    begin-critical
    update-hold-priority
    end-critical
  ;

[then]
    
\ Warm reboot
warm