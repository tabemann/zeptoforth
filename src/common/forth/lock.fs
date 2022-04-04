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

\ Compile to flash
compile-to-flash

compress-flash

begin-module lock

  task import
  slock import

  begin-module lock-internal

    \ Lock header structure
    begin-structure lock-size

      \ Lock simple lock
      slock-size +field lock-slock
      
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

    commit-flash
    
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
	dup lock-wait-task @ task-priority@ rot max swap lock-wait-next @
      repeat
      drop
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

    \ Remove a lock wait record
    : remove-lock-wait ( wait lock -- )
      dup lock-first-wait @ 2 pick = if
	dup lock-last-wait @ 2 pick = if
	  0 over lock-last-wait !
	then
	swap lock-wait-next @ swap lock-first-wait !
      else
	dup lock-first-wait @ begin
	  dup if
	    dup lock-wait-next @ 3 pick = if
	      over lock-last-wait @ 3 pick = if
		0 over lock-wait-next ! swap lock-last-wait ! drop true
	      else
		rot lock-wait-next @ swap lock-wait-next ! drop true
	      then
	    else
	      lock-wait-next @ false
	    then
	  else
	    2drop drop true
	  then
	until
      then
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
      dup task-priority@ over task-saved-priority!
      swap over ['] current-lock-held for-task @ ?dup if
	over 3 roll ['] current-lock-held for-task !
	dup get-last-lock
	2 pick swap lock-next-held !
	swap lock-next-held !
      else
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

    commit-flash

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

    commit-flash
    
    \ Resolve task priority on holding a lock
    : update-hold-priority ( lock -- )
      dup lock-next-held @ if
	dup group-max-lock-wait-priority
	over lock-holder-task @ task-saved-priority@ max
	over lock-holder-task @ task-priority!
	lock-holder-task @ ['] current-lock for-task @ ?dup if
	  recurse
	then
      else
	drop
      then
    ;

    \ Resolve task priority on releasing a lock
    : update-release-priority ( lock -- )
      dup lock-next-held @ if
	dup get-last-lock over <> if
	  dup lock-next-held @ swap remove-lock
	  dup group-max-lock-wait-priority
	  over lock-holder-task @ task-saved-priority@ max
	  swap lock-holder-task @ task-priority!
	else
	  dup remove-lock lock-holder-task @ dup task-saved-priority@
	  swap task-priority!
	then
      else
	lock-holder-task @ dup task-saved-priority@
	swap task-priority!
      then
    ;

  end-module> import

  \ Initialize a lock
  : init-lock ( addr -- )
    dup lock-slock init-slock
    0 over lock-holder-task !
    0 over lock-first-wait !
    0 over lock-last-wait !
    0 swap lock-next-held !
  ;

  \ Double locking exception
  : x-double-lock ( -- ) ." double locked" cr ;

  \ Attempted to unlock a lock not owned by the current task
  : x-not-currently-owned ( -- ) ." lock not owned by current task" cr ;

  commit-flash
  
  \ Lock a lock
  : claim-lock ( lock -- )
    [:
      s" BEGIN CLAIM LOCK" trace
      dup lock-holder-task @ current-task = triggers x-double-lock
      current-task prepare-block
      dup lock-holder-task @ if
	dup current-task ['] current-lock for-task !
	init-lock-wait
	2dup swap add-lock-wait
	over update-hold-priority
	over lock-slock release-slock-block
	over lock-slock claim-slock
	[: current-task validate-timeout ;] try ?dup if
	  >r [:
	    dup rot remove-lock-wait lock-wait-orig-here @ ram-here!
	  ;] critical r> ?raise
	then
	lock-wait-orig-here @ ram-here!
	drop
      else
	dup current-task add-lock
	current-task over lock-holder-task !
	update-hold-priority
      then
      s" END CLAIM LOCK" trace
    ;] over lock-slock with-slock
  ;

  \ Unlock a lock
  : release-lock ( lock -- )
    [:
      s" BEGIN RELEASE LOCK" trace
      dup lock-holder-task @ current-task = averts x-not-currently-owned
      dup update-release-priority
      dup lock-first-wait @ ?dup if
	dup lock-wait-next @ 2 pick lock-first-wait !
	over lock-first-wait @ 0= if
	  0 2 pick lock-last-wait !
	then
	lock-wait-task @
	2dup add-lock
	2dup swap lock-holder-task !
	0 over ['] current-lock for-task !
	swap update-hold-priority
	ready
      else
	0 swap lock-holder-task !      
      then
      s" END RELEASE LOCK" trace
    ;] over lock-slock with-slock
  ;

  \ Update the priorities of tasks holding locks
  : update-lock-priority ( lock -- )
    ['] update-hold-priority over lock-slock with-slock
  ;

  commit-flash
  
  \ Execute a block of code with a lock
  : with-lock ( xt lock -- ) dup >r claim-lock try r> release-lock ?raise ;

  \ Export lock-size
  export lock-size

end-module

end-compress-flash
    
\ Reboot
reboot