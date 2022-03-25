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

\ Compile this to flash
compile-to-flash

begin-module task

  internal import
  interrupt import
  multicore import
  systick import
  int-io import

  begin-module task-internal

    \ Task readied state
    0 constant readied

    \ Task delayed state
    1 constant delayed

    \ Task blocked with timeout
    2 constant blocked-timeout

    \ Task blocked until waking
    3 constant blocked-wait

    \ Task blocked indefinitely
    4 constant blocked-indefinite

    \ Task block timed out
    5 constant block-timed-out

    \ Claim same core spinlock on schedule bit
    $1000 constant schedule-with-same-core-spinlock

    \ Claim arbitrary spinlock on schedule bit
    $2000 constant schedule-with-spinlock
    
    \ User schedule into critical section bit
    $4000 constant schedule-user-critical
    
    \ Schedule into critical section bit
    $8000 constant schedule-critical

    \ Task state mask
    $0FFF constant task-state-mask
    
    \ In task change
    cpu-variable cpu-in-task-change in-task-change

    \ Wake tasks flag
    cpu-variable cpu-wake-tasks wake-tasks
    
    \ Main task
    cpu-variable cpu-main-task main-task
    
    \ Current task
    cpu-variable cpu-current-task current-task

    \ Previous task
    cpu-variable cpu-prev-task prev-task

    \ First task
    cpu-variable cpu-first-task first-task
    
    \ Last task
    cpu-variable cpu-last-task last-task

    \ Declare a RAM variable for the end of free RAM memory
    variable free-end

    \ Pause count
    cpu-variable cpu-pause-count pause-count

    \ Currently in multitasker
    cpu-variable cpu-in-multitasker? in-multitasker?

    \ The multitasker SysTick counter
    cpu-variable cpu-task-systick-counter task-systick-counter

    \ The saved multitasker SysTick counter
    user task-saved-systick-counter
    
    \ The default timeslice
    10 constant default-timeslice

    \ The default minimum timeslice
    0 constant default-min-timeslice
    
    \ Sleep is enabled
    cpu-variable cpu-sleep-enabled? sleep-enabled?

    \ Tracing is enabled
    cpu-variable cpu-trace-enabled? trace-enabled?

    \ CPU is active
    cpu-count cells buffer: cpu-active?
    : cpu-active? ( index -- addr ) cells cpu-active? + ;

    \ The task ready count
    user task-ready-count
    
    \ Task systick start time
    user task-systick-start

    \ Task systick delay
    user task-systick-delay

    \ A task's timeslice
    user task-timeslice

    \ A task's minimum timeslice
    user task-min-timeslice

    \ The current timeout start time in ticks
    user timeout-systick-start
    
    \ The current timeout delay time in ticks
    user timeout-systick-delay
  
    \ The task's name as a counted string
    user task-name

    \ The notification being waited on
    user task-current-notify

    \ The notified bitmap
    user task-notified-bitmap

    \ The current notification count
    user task-notify-count

    \ The current notification area pointer
    user task-notify-area

    \ Spinlock to claim
    user spinlock-to-claim

    \ The current core of a task
    user task-core

    \ Task being waited for
    user task-waited-for

    \ SVCall vector index
    11 constant svcall-vector

    \ PendSV vector index
    14 constant pendsv-vector
    
    \ Terminated task
    $8000 constant terminated
    
    \ The task structure
    begin-structure task-size
      \ Return stack size
      hfield: task-rstack-size
      
      \ Data stack size
      hfield: task-stack-size

      \ Dictionary size
      field: task-dict-size

      \ Current return stack adress
      field: task-rstack-current

      \ Current dictionary base
      field: task-dict-base
      
      \ Task priority
      hfield: task-priority
      
      \ Task active state ( > 0 active, <= 0 inactive, $8000 terminated )
      hfield: task-active

      \ Whether a task is waiting
      hfield: task-state

      \ Task saved priority
      hfield: task-saved-priority

      \ Prev task
      field: task-prev
      
      \ Next task
      field: task-next
    end-structure

    \ Auxiliary main task dictionary size in bytes
    256 constant aux-main-task-dict-size

    \ Auxiliary main task data stack size in bytes
    128 constant aux-main-task-stack-size

    \ Auxiliary main task return stack size in bytes
    512 constant aux-main-task-rstack-size

  end-module> import

  \ The currently waited-for lock
  user current-lock
  
  \ The latest lock currently held by a tack
  user current-lock-held

  \ The default timeout
  user timeout

  \ No timeout
  -1 constant no-timeout
  
  \ Attempted to use a terminated task
  : x-terminated ( -- ) ." task has been terminated" cr ;

  \ Would block exception
  : x-would-block ( -- ) ." operation would block" cr ;
  
  \ Attempted to task change while changing tasks during interrupt
  : x-in-task-change ( -- ) ." in task change" cr ;

  \ Out of range task priority exception
  : x-out-of-range-priority ( -- ) ." out of range priority" cr ;

  \ Timed out exception
  : x-timed-out ( -- ) ." block timed out" cr ;

  \ Out of range notification index
  : x-out-of-range-notify ( -- ) ." out of range notification" cr ;

  \ Currently waiting on notification that would be removed
  : x-current-wait-notify ( -- ) ." currently await notification" cr ;

  \ Sleep
  : sleep ( -- ) sleep-enabled? @ if sleep then ;

  \ Access a given user variable for a given task
  : for-task ( task xt -- addr )
    execute dict-base @ - swap task-dict-base @ +
  ;

  \ Set a given user variable for a given task
  : for-task! ( x task xt -- )
    execute dict-base @ - swap task-dict-base @ + !
  ;

  \ Get a given user variable for a given task
  : for-task@ ( task xt -- x )
    execute dict-base @ - swap task-dict-base @ + @
  ;

  continue-module task-internal

    \ Get task dictionary end
    : task-dict-end ( task -- addr ) ;

    \ Get task current stack address
    : task-stack-current ( task -- addr )
      dup last-task @ = if drop sp@ else task-rstack-current @ 12 + @ then
    ;

    \ Get task current dictionary address
    : task-dict-current ( task -- addr )
      ['] task-ram-here for-task@
    ;

    \ Set task current stack address
    : task-stack-current! ( addr task -- )
      dup last-task @ = if drop sp! else task-rstack-current @ 12 + ! then
    ;

    \ Set task current dictionary address
    : task-dict-current! ( addr task -- )
      ['] task-ram-here for-task!
    ;

    \ Push data onto a task's stack
    : push-task-stack ( x task -- )
      dup task-rstack-current @ 8 + @
      over dup task-stack-current cell - tuck swap task-stack-current! !
      task-rstack-current @ 8 + !
    ;

    \ Push data onto a task's return stack
    : push-task-rstack ( x task -- )
      dup task-rstack-current @ cell - tuck swap task-rstack-current !
    ;

  end-module
  
  \ Get task priority
  : task-priority@ ( task -- priority )
    task-priority h@ 16 lshift 16 arshift
  ;

  \ Validate task is not terminated
  : validate-not-terminated ( task -- )
    task-active h@ terminated = triggers x-terminated
  ;

  continue-module task-internal

    \ Validate a notification index
    : validate-notify ( notify-index task -- )
      ['] task-notify-count for-task@ u< averts x-out-of-range-notify
    ;
    
    \ Find the next task with a higher priority; 0 returned indicates no task
    \ exists with a higher priority.
    : find-higher-priority ( task -- higher-task )
      task-priority@ >r
      dup ['] task-core for-task@ cpu-last-task @ begin
	dup 0<> if
	  dup task-priority@ r@ < if task-next @ false else true then
	else
	  true
	then
      until
      rdrop
    ;

    \ Insert a task before another task
    : insert-task-before ( task after-task -- )
      dup task-prev @ 0<> if
	2dup task-prev @ task-next !
	2dup task-prev @ swap task-prev !
      else
	0 2 pick task-prev !
	over dup ['] task-core for-task@ cpu-last-task !
      then
      2dup task-prev !
      swap task-next !
    ;

    \ Insert a task into first position
    : insert-task-first ( task -- )
      dup ['] task-core for-task@ >r
      r@ cpu-first-task @ 0<> if
	dup r@ cpu-first-task @ task-next !
	r@ cpu-first-task @ over task-prev !
      else
	dup r@ cpu-last-task !
	0 over task-prev !
      then
      dup r> cpu-first-task !
      0 swap task-next !
    ;

    \ Insert a task
    : insert-task ( task -- )
      dup find-higher-priority ?dup if
	insert-task-before
      else
	insert-task-first
      then
    ;

    \ Remove a task
    : remove-task ( task -- )
      dup ['] task-core for-task@ >r
      dup task-next @ ?dup if
	over task-prev @ swap task-prev !
      else
	dup task-prev @ r@ cpu-first-task !
      then
      dup task-prev @ ?dup if
	over task-next @ swap task-next !
      else
	dup task-next @ r@ cpu-last-task !
      then
      0 over task-prev !
      0 swap task-next !
      rdrop
    ;

    \ Remove a task if it is scheduled
    : test-remove-task ( task -- )
      dup task-prev @ 0<> over task-next @ 0<> or if remove-task else drop then
    ;

    \ Set task change
    \ : start-task-change ( core -- )
    \   disable-int
    \   begin dup cpu-in-task-change @ while
    \ 	ICSR_VECTACTIVE@ 0 = if
    \ 	  enable-int
    \ 	  pause
    \ 	  disable-int
    \ 	else
    \ 	  enable-int
    \ 	  ['] x-in-task-change ?raise
    \ 	then
    \   repeat
    \   true swap cpu-in-task-change !
    \   enable-int
    \ ;

    \ Start a task change
    : start-validate-task-change ( task -- )
\      dup ['] task-core for-task@ start-task-change
      ['] validate-not-terminated try ?dup if
	( false in-task-change ! ) ?raise
      then
    ;
  
  end-module

  \ Get whether a task has timed out
  : timed-out? ( task -- timed-out )
    dup validate-not-terminated task-state h@ task-state-mask and
    block-timed-out =
  ;

  \ Validate not timing out
  : validate-timeout ( task -- ) timed-out? triggers x-timed-out ;

  \ Get task active state
  : task-active@ ( task -- active )
    task-active h@ 16 lshift 16 arshift
  ;

  \ Get saved task priority
  : task-saved-priority@ ( task -- priority )
    task-saved-priority h@
  ;

  \ Get the core of a task
  : task-core@ ( task -- core ) ['] task-core for-task@ ;

  \ Set task priority
  : task-priority! ( priority task -- )
    over -32768 < triggers x-out-of-range-priority
    over 32767 > triggers x-out-of-range-priority
    task-priority h!
  ;

  \ Set task saved priority
  : task-saved-priority! ( priority task -- )
    over -32768 < triggers x-out-of-range-priority
    over 32767 > triggers x-out-of-range-priority
    task-saved-priority h!
  ;

  \ Set task timeslice
  : task-timeslice! ( timeslice task -- )
    disable-int
    dup validate-not-terminated
    dup current-task @ = if
      2dup ['] task-timeslice for-task@ - task-systick-counter @ +
      task-systick-counter !
    then
    swap 0 max swap ['] task-timeslice for-task!
    enable-int
  ;

  \ Get task timeslice
  : task-timeslice@ ( task -- timeslice )
    dup validate-not-terminated ['] task-timeslice for-task@
  ;

  \ Set task minimum timeslice
  : task-min-timeslice! ( min-timeslice task -- )
    dup validate-not-terminated ['] task-min-timeslice for-task!
  ;

  \ Get task minimum timeslice
  : task-min-timeslice@ ( task -- min-timeslice )
    dup validate-not-terminated ['] task-min-timeslice for-task@
  ;

  \ Get a task's name as a counted string; an address of zero indicates no name
  \ is set.
  : task-name@ ( task -- addr )
    dup validate-not-terminated ['] task-name for-task@
  ;

  \ Set a task's name as a counted string; an address of zero indicates to set
  \ no name.
  : task-name! ( addr -- task )
    dup validate-not-terminated ['] task-name for-task!
  ;

  \ Start a task's execution
  : run ( task -- )
    [:
      dup start-validate-task-change
      dup task-active@ 1+
      dup 1 = if over test-remove-task over insert-task then
      swap task-active h!
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Stop a task's execution
  : stop ( task -- )
    [:
      dup start-validate-task-change
      dup task-active@ 1-
      swap task-active h!
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Kill a task
  : kill ( task -- )
    [:
      dup start-validate-task-change
      terminated over task-active h!
      dup current-task @ = if
	task-core@ release-other-core-spinlock
	end-critical
	begin pause again
      else
	drop
      then
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Get the last delay time
  : last-delay ( task -- ticks-delay ticks-start )
    [:
      dup validate-not-terminated
      dup ['] task-systick-delay for-task@
      swap ['] task-systick-start for-task@
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Delay a task
  : delay ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      tuck ['] task-systick-start for-task!
      tuck ['] task-systick-delay for-task!
      delayed over task-state h!
    ;] over task-core@ critical-with-other-core-spinlock
    current-task @ = if pause then
  ;

  \ Delay a task and schedule as critcal once done
  : delay-critical ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      tuck ['] task-systick-start for-task!
      tuck ['] task-systick-delay for-task!
      [ delayed schedule-critical or schedule-user-critical or ] literal
      over task-state h!
    ;] over task-core@ critical-with-other-core-spinlock
    current-task @ = if pause then
  ;

  \ Mark a task as blocked until a timeout
  : block-timeout ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      -1 over ['] task-ready-count for-task +!
      dup ['] task-ready-count for-task@ 0< if
	tuck ['] task-systick-start for-task!
	tuck ['] task-systick-delay for-task!
	blocked-timeout over task-state h!
      else
	nip nip
      then
    ;] over task-core@ critical-with-other-core-spinlock
    current-task @ = if pause then
  ;

  \ Mark a task as blocked until a timeout and schedule as critical once done
  : block-timeout-critical ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      -1 over ['] task-ready-count for-task +!
      dup ['] task-ready-count for-task@ 0< if
	tuck ['] task-systick-start for-task!
	tuck ['] task-systick-delay for-task!
	[ blocked-timeout schedule-critical or schedule-user-critical or ]
	literal
	over task-state h!
      else
	nip nip
	[ readied schedule-critical or schedule-user-critical or ]
	literal
	over task-state h!
      then
    ;] over task-core@ critical-with-other-core-spinlock
    current-task @ = if pause then
  ;

  \ Mark a task as blocked until a timeout and schedule as critical along with
  \ claiming a spinlock once done
  : block-timeout-with-spinlock ( spinlock ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      -1 over ['] task-ready-count for-task +!
      dup ['] task-ready-count for-task@ 0< if
	tuck ['] task-systick-start for-task!
	tuck ['] task-systick-delay for-task!
	tuck ['] spinlock-to-claim for-task!
	[ blocked-timeout schedule-critical or schedule-user-critical or
	schedule-with-spinlock or ] literal
	over task-state h!
      else
	nip nip
	[ readied schedule-critical or schedule-user-critical or ]
	literal
	over task-state h!
      then
    ;] over task-core@ critical-with-other-core-spinlock
    current-task @ = if pause then
  ;

  \ Wait until a timeout on the specified notification index and return the
  \ value for that notification index once notified, unless already notified,
  \ where then that value will be returned and the notified state will be
  \ cleared immediately. x-timed-out is raised if the timeout is reached.
  : wait-notify-timeout ( ticks-delay ticks-start notify-index -- x )
    [:
      dup current-task @ validate-notify
      swap task-systick-start !
      swap task-systick-delay !
      begin dup bit task-notified-bitmap bit@ not while
	dup task-current-notify !
	[ blocked-timeout schedule-critical or ] literal
	current-task @ task-state h!
	release-same-core-spinlock end-critical pause
	claim-same-core-spinlock
	current-task @ validate-timeout
      repeat
      task-notify-area @ over cells + @
      swap bit task-notified-bitmap bic!
      -1 task-current-notify !
    ;] cpu-index critical-with-other-core-spinlock
  ;

  \ Wait until a timeout on the specified notification index and return the
  \ value for that notification index once notified, unless already notified,
  \ where then that value will be returned and the notified state will be
  \ cleared immediately. x-timed-outo is raised if the timeout is reached.
  \ This word will leave the system in a critical section once completed.
  : wait-notify-timeout-critical ( ticks-delay ticks-start notify-index -- x )
    [:
      begin-critical
      [:
	dup current-task @ validate-notify
	swap task-systick-start !
	swap task-systick-delay !
	begin dup bit task-notified-bitmap bit@ not while
	  dup task-current-notify !
	  [ blocked-timeout schedule-critical or schedule-user-critical or ]
	  literal
	  current-task @ task-state h!
	  release-same-core-spinlock end-critical pause
	  claim-same-core-spinlock
	  current-task @ validate-timeout
	repeat
	task-notify-area @ over cells + @
	swap bit task-notified-bitmap bic!
	-1 task-current-notify !
      ;] try ?dup if end-critical ?raise then
    ;] cpu-index critical-with-other-core-spinlock
  ;

  \ Mark a task as waiting
  : block-wait ( task -- )
    dup validate-not-terminated
    blocked-wait over task-state h!
    current-task @ = if pause then
  ;
  
  \ Mark a task as waiting
  : block-wait-critical ( task -- )
    dup validate-not-terminated
    [ blocked-wait schedule-critical or schedule-user-critical or ] literal
    over task-state h!
    current-task @ = if pause then
  ;

  \ Mark a task as blocked indefinitely
  : block-indefinite ( task -- )
    [:
      dup validate-not-terminated
      -1 over ['] task-ready-count for-task +!
      dup ['] task-ready-count for-task@ 0< if
	blocked-indefinite over task-state h!
      then
    ;] over task-core@ critical-with-other-core-spinlock
    current-task @ = if pause then
  ;

  \ Mark a task as blocked indefinitely and schedule as critical when done
  : block-indefinite-critical ( task -- )
    [:
      dup validate-not-terminated
      -1 over ['] task-ready-count for-task +!
      dup ['] task-ready-count for-task@ 0< if
	[ blocked-indefinite schedule-critical or schedule-user-critical or ]
	literal over task-state h!
      else
	[ readied schedule-critical or schedule-user-critical or ]
	literal over task-state h!
      then
    ;] over task-core@ critical-with-other-core-spinlock
    current-task @ = if pause then
  ;

  \ Exit a critical section, release a spinlock, block indefinitely, and then
  \ claim the spinlock and enter a critical section on readying
  : block-indefinite-self-release ( spinlock -- )
    disable-int
    -1 task-ready-count !
    blocked-indefinite current-task @ task-state h!
    release-same-core-spinlock-raw
    dup release-spinlock-raw
    0 in-critical !
    pause
    enable-int
    disable-int
    1 in-critical !
    claim-same-core-spinlock-raw
    claim-spinlock-raw
    enable-int

\     -1 task-ready-count +!
\     dup spinlock-to-claim !
\     task-ready-count @ 0< if
\       [ blocked-indefinite schedule-critical or schedule-user-critical or
\       schedule-with-spinlock or schedule-with-same-core-spinlock or ] literal
\     else
\       [ readied schedule-critical or schedule-user-critical or
\       schedule-with-spinlock or schedule-with-same-core-spinlock or ] literal
\     then
\     current-task @ task-state h!
\     release-spinlock release-same-core-spinlock end-critical-pause
  ;

  \ Mark a task as blocked indefinitely and schedule as critical and claim a
  \ spinlock when done
  : block-indefinite-with-spinlock ( spinlock task -- )
    [:
      dup validate-not-terminated
      -1 over ['] task-ready-count for-task +!
      tuck ['] spinlock-to-claim for-task!
      dup ['] task-ready-count for-task@ 0< if
	[ blocked-indefinite schedule-critical or schedule-user-critical or
	schedule-with-spinlock or ]
	literal over task-state h!
      else
	[ readied schedule-critical or schedule-user-critical or
	schedule-with-spinlock or ]
	literal over task-state h!
      then
    ;] over task-core@ critical-with-other-core-spinlock
    current-task @ = if pause then
  ;

  \ Wait indefinitely on the specified notification index and return the value
  \ for that notification index once notified, unless already notified, where
  \ then that value will be returned and the notified state will be cleared
  \ immediately.
  : wait-notify-indefinite ( notify-index -- x )
    [:
      dup current-task @ validate-notify
      begin dup bit task-notified-bitmap bit@ not while
	dup task-current-notify !
	[ blocked-indefinite schedule-critical or ] literal
	current-task @ task-state h!
	release-same-core-spinlock end-critical pause
	claim-same-core-spinlock
      repeat
      task-notify-area @ over cells + @
      swap bit task-notified-bitmap bic!
      -1 task-current-notify !
    ;] cpu-index critical-with-other-core-spinlock
  ;

  \ Wait indefinitely on the specified notification index and return the value
  \ for that notification index once notified, unless already notified, where
  \ then that value will be returned and the notified state will be cleared
  \ immediately. This word will leave the system in a critical section once
  \ completed.
  : wait-notify-indefinite-critical ( notify-index -- x )
    [:
      begin-critical
      [:
	dup current-task @ validate-notify
	begin dup bit task-notified-bitmap bit@ not while
	  dup task-current-notify !
	  [ blocked-indefinite schedule-critical or schedule-user-critical or ]
	  literal
	  current-task @ task-state h!
	  release-same-core-spinlock end-critical pause
	  claim-same-core-spinlock
	repeat
	task-notify-area @ over cells + @
	swap bit task-notified-bitmap bic!
	-1 task-current-notify !
      ;] try ?dup if end-critical ?raise then
    ;] cpu-index critical-with-other-core-spinlock
  ;

  continue-module task-internal

    \ Core of readying a task
    : do-ready ( task -- )
      dup ['] task-current-notify for-task@ -1 = if
	1 over ['] task-ready-count for-task +!
	dup ['] task-ready-count for-task@ 0>= if
	  dup task-state h@
	  [ schedule-critical schedule-user-critical or
	  schedule-with-spinlock or schedule-with-same-core-spinlock or ]
	  literal and readied or swap task-state h!
	else
	  drop
	then
      else
	drop
      then
    ;

  end-module
      
  \ Ready a task
  : ready ( task -- )
    [:
      dup validate-not-terminated do-ready
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Notify a task for a specified notification index
  : notify ( notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      2dup swap bit swap ['] task-notified-bitmap for-task bis!
      dup ['] task-current-notify for-task@ rot = if
	dup task-state h@ schedule-user-critical and
	[ schedule-critical readied or ] literal or swap task-state h! true
      else
	drop false
      then
    ;] over task-core@ critical-with-other-core-spinlock
    if pause then
  ;

  \ Notify a task for a specified notification index, setting the notification
  \ value to a set value
  : notify-set ( x notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      dup ['] task-notify-area for-task@ 2 pick cells + >r rot r> !
      2dup swap bit swap ['] task-notified-bitmap for-task bis!
      dup ['] task-current-notify for-task@ rot = if
	dup task-state h@ schedule-user-critical and
	[ schedule-critical readied or ] literal or swap task-state h! true
      else
	drop false
      then
    ;] over task-core@ critical-with-other-core-spinlock
    if pause then
  ;

  \ Notify a task for a specified notification index, updating the notification
  \ value with an xt with the signature ( x0 -- x1 )
  : notify-update ( xt notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      rot >r ( index task )
      dup ['] task-notify-area for-task@ 2 pick cells + ( index task addr )
      r> rot >r rot >r swap dup >r @ swap execute r> ! ( )
      r> r> 2dup swap bit swap ['] task-notified-bitmap for-task bis!
      dup ['] task-current-notify for-task@ rot = if
	dup task-state h@ schedule-user-critical and
	[ schedule-critical readied or ] literal or swap task-state h! true
      else
	drop false
      then
    ;] over task-core@ critical-with-other-core-spinlock
    if pause then
  ;

  \ Clear a notification for a task
  : clear-notify ( notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      swap bit swap ['] task-notified-bitmap for-task bic!
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Get the contents of a notification mailbox without waiting on it
  : mailbox@ ( notify-index task -- x )
    [:
      dup validate-not-terminated
      2dup validate-notify
      ['] task-notify-area for-task@ swap cells + @
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Set the contents of a notification mailbox without notifying a task
  : mailbox! ( x notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      ['] task-notify-area for-task@ swap cells + !
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Block a task for the specified initialized timeout
  : block ( task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task@ no-timeout <> if
	dup ['] timeout-systick-delay for-task@
	over ['] timeout-systick-start for-task@
	rot block-timeout
      else
	block-indefinite
      then
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Block a task for the specified initialized timeout and schedule as critical
  \ once done
  : block-critical ( task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task@ no-timeout <> if
	dup ['] timeout-systick-delay for-task@
	over ['] timeout-systick-start for-task@
	rot block-timeout-critical
      else
	block-indefinite-critical
      then
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Block a task for the specified initialized timeout and schedule as critical
  \ and claim a spinlock once done
  : block-with-spinlock ( spinlock task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task@ no-timeout <> if
	dup ['] timeout-systick-delay for-task@
	over ['] timeout-systick-start for-task@
	rot block-timeout-with-spinlock
      else
	block-indefinite-with-spinlock
      then
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Wait for a notification at a specified notification index with a specified
  \ initialized timeout and return the notification value
  : wait-notify ( notify-index -- x )
    claim-same-core-spinlock
    begin-critical
    timeout @ no-timeout <> if
      timeout-systick-delay @ timeout-systick-start @ rot
      end-critical release-same-core-spinlock wait-notify-timeout
    else
      end-critical release-same-core-spinlock wait-notify-indefinite
    then
  ;

  \ Wait for a notification at a specified notification index with a specified
  \ initialized timeout and return the notification value and schedule as
  \ critical once done
  : wait-notify-critical ( task -- )
    claim-same-core-spinlock
    begin-critical
    timeout @ no-timeout <> if
      timeout-systick-delay @ timeout-systick-start @ rot
      end-critical release-same-core-spinlock wait-notify-timeout-critical
    else
      end-critical release-same-core-spinlock wait-notify-indefinite-critical
    then
  ;

  \ Prepare blocking for a task
  : prepare-block ( task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task@ no-timeout <> if
	systick-counter over ['] timeout-systick-start for-task!
	dup ['] timeout for-task@ swap ['] timeout-systick-delay for-task!
      else
	drop
      then
    ;] over task-core@ critical-with-other-core-spinlock
  ;

  \ Get whether a task has terminated
  : terminated? ( task -- terminated ) task-active h@ terminated = ;

  \ Ready tasks waiting for current task
  \ argument
  : wake-other-tasks ( -- )
    cpu-count 0 do \ ." E"
      i cpu-first-task @ begin ?dup while \ ." F"
	dup ['] task-waited-for for-task@ current-task @ = if \ ." G"
	  0 over ['] task-waited-for for-task! dup do-ready \ ." H"
	then \ ." J"
	task-prev @ \ ." K"
      repeat \ ." L"
    loop \ ." M"
  ;
  
  continue-module task-internal
    
    \ Initialize the main task
    : init-main-task ( -- )
      claim-all-core-spinlock
      free-end @ task-size -
      dict-base @ over task-dict-base !
      rstack-base @ rstack-end @ - over task-rstack-size h!
      stack-base @ stack-end @ - over task-stack-size h!
      free-end @ task-size - next-ram-space - over task-dict-size !
      rp@ over task-rstack-current !
      0 over task-priority h!
      0 over task-saved-priority h!
      1 over task-active h!
      readied over task-state h!
      no-timeout timeout !
      0 task-ready-count !
      0 task-waited-for !
      0 timeout-systick-start !
      0 timeout-systick-delay !
      0 task-systick-start !
      -1 task-systick-delay !
      cpu-index task-core !
      c" main" task-name !
      -1 task-current-notify !
      0 task-notified-bitmap !
      0 task-notify-count !
      0 task-notify-area !
      default-timeslice task-timeslice !
      default-min-timeslice task-min-timeslice !
      default-timeslice task-saved-systick-counter !
      0 current-lock !
      0 current-lock-held !
      0 over task-next !
      0 over task-prev !
      dup main-task !
      dup first-task !
      dup last-task !
      dup prev-task !
      current-task !
      free-end @ task-size - free-end !
      release-all-core-spinlock
    ;

    \ Task entry point
    : task-entry ( -- )
      rdrop
      try
      ?dup if display-red execute display-normal then
      current-task @ kill
    ;

    \ Get whether a task is finished with a delay or timeout
    : delayed? ( task -- )
      systick-counter over ['] task-systick-start for-task@ -
      swap ['] task-systick-delay for-task@ <
    ;

    cpu-count 1 > [if]
      
      \ Populate an auxiliary task's stack
      : init-aux-task-stack ( xn...x0 count xt task -- sp )
	['] task-stack-base for-task@
	swap >r >r
	$FEDCBA98 r> cell - tuck ! >r
	begin dup 0<> while
	  dup roll r> cell - tuck ! >r 1-
	repeat
	drop
	r> r> swap cell - tuck !
      ;
      
      \ Initialize an auxiliary task
      : init-aux-task ( xn...x0 count xt task core -- data-stack return-stack )
	swap dup dup task-dict-size @ - over task-dict-base ! swap
	over ['] task-core for-task!
	dup task-rstack-size h@ over task-stack-size h@ + over + task-size +
	over ['] task-rstack-base for-task!
	dup task-stack-size h@ over + task-size +
	over ['] task-rstack-end for-task!
	dup ['] task-rstack-end for-task@ over ['] task-stack-base for-task!
	dup task-size + over ['] task-stack-end for-task!
	0 over ['] task-ready-count for-task!
	0 over ['] task-handler for-task!
	0 over ['] task-waited-for for-task!
	0 over task-priority h!
	0 over task-saved-priority h!
	0 over task-active h!
	base @ over ['] task-base for-task!
	0 over ['] current-lock for-task!
	0 over ['] current-lock-held for-task!
	readied over task-state h!
	no-timeout over ['] timeout for-task!
	0 over ['] timeout-systick-start for-task!
	0 over ['] timeout-systick-delay for-task!
	0 over ['] task-systick-start for-task!
	0 over ['] task-name for-task!
	-1 over ['] task-current-notify for-task!
	0 over ['] task-notified-bitmap for-task!
	0 over ['] task-notify-count for-task!
	0 over ['] task-notify-area for-task!
	-1 over ['] task-systick-delay for-task!
	c" aux-main" over ['] task-name for-task!
	default-timeslice over ['] task-timeslice for-task!
	default-min-timeslice over ['] task-min-timeslice for-task!
	default-timeslice over ['] task-saved-systick-counter for-task!
	dup ['] task-rstack-base for-task@ over task-rstack-current !
	next-user-space over task-dict-base @ +
	over ['] task-ram-here for-task!
	0 over task-next !
	0 over task-prev !
	dup >r init-aux-task-stack
	r> ['] task-rstack-base for-task@
      ;

    [then]

  end-module

  \ Initialize a task
  : init-task ( xn...x0 count xt task core -- )
    swap dup dup task-dict-size @ - over task-dict-base ! swap
    over ['] task-core for-task!
    dup task-rstack-size h@ over task-stack-size h@ + over + task-size +
    over ['] task-rstack-base for-task!
    dup task-stack-size h@ over + task-size +
    over ['] task-rstack-end for-task!
    dup ['] task-rstack-end for-task@ over ['] task-stack-base for-task!
    dup task-size + over ['] task-stack-end for-task!
    0 over ['] task-ready-count for-task!
    0 over ['] task-handler for-task!
    0 over ['] task-waited-for for-task!
    0 over task-priority h!
    0 over task-saved-priority h!
    0 over task-active h!
    base @ over ['] task-base for-task!
    0 over ['] current-lock for-task!
    0 over ['] current-lock-held for-task!
    readied over task-state h!
    no-timeout over ['] timeout for-task!
    0 over ['] timeout-systick-start for-task!
    0 over ['] timeout-systick-delay for-task!
    0 over ['] task-systick-start for-task!
    0 over ['] task-name for-task!
    -1 over ['] task-current-notify for-task!
    0 over ['] task-notified-bitmap for-task!
    0 over ['] task-notify-count for-task!
    0 over ['] task-notify-area for-task!
    -1 over ['] task-systick-delay for-task!
    default-timeslice over ['] task-timeslice for-task!
    default-min-timeslice over ['] task-min-timeslice for-task!
    default-timeslice over ['] task-saved-systick-counter for-task!
    dup ['] task-rstack-base for-task@
    over ['] task-stack-base for-task@ ['] task-entry
    ['] init-context svc over task-rstack-current !
    next-user-space over task-dict-base @ + over ['] task-ram-here for-task!
    0 over task-next !
    0 over task-prev !
    swap >r >r
    begin dup 0<> while
      dup roll r@ push-task-stack 1-
    repeat
    drop r> r> swap push-task-stack
  ;

  \ Allot space for a task
  : task-allot ( dict-size stack-size rstack-size -- task )
    claim-all-core-spinlock
    2dup 4 align swap 4 align + task-size +
    free-end @ swap -
    swap 4 align swap tuck task-rstack-size h!
    swap 4 align swap tuck task-stack-size h!
    swap 4 align swap tuck task-dict-size !
    dup dup task-dict-size @ - free-end !
    release-all-core-spinlock
  ;

  \ Configure notification for a task; notify-count may be from 0 to 32, and
  \ notify-area-addr is the address of an area of memory that contains that
  \ number of cells.
  : config-notify ( notify-area-addr notify-count task -- )
    [:
      over 32 u<= averts x-out-of-range-notify
      2dup ['] task-current-notify for-task@ > averts x-current-wait-notify
      2dup swap $FFFFFFFF 32 rot - rshift
      over ['] task-notified-bitmap for-task@
      and swap ['] task-notified-bitmap for-task!
      2dup ['] task-notify-count for-task!
      2 pick rot cells 0 fill
      ['] task-notify-area for-task!
    ;] critical
  ;

  continue-module task-internal

    \ Wake tasks
    : do-wake ( -- )
      cpu-count begin ?dup while
	1- true over cpu-wake-tasks !
      repeat
    ;

    \ Get whether a task is waiting
    : waiting-task? ( task -- )
      dup task-state h@ task-state-mask and
      dup blocked-wait = over blocked-indefinite = or
      over delayed = rot blocked-timeout = or
      rot delayed? and or
    ;

    \ Find next task
    : find-next-task ( -- task )
      first-task @ begin
	dup 0<> if
	  dup waiting-task? if
	    task-prev @ false
	  else
	    true
	  then
	else
	  true
	then
      until
    ;

    \ PendSV return value
    variable pendsv-return

    \ Save task state
    : save-task-state ( task -- )
      task-systick-counter @ swap ['] task-saved-systick-counter for-task!
    ;

    \ Restore task state
    : restore-task-state ( task -- )
      task-dict-base @ dict-base !
    ;

    \ Reschedule previous task
    : reschedule-task ( task -- )
      true in-task-change !
      claim-same-core-spinlock
      dup remove-task
      dup task-active@ 0> if insert-task else drop then
      release-same-core-spinlock
      false in-task-change !
    ;

    \ Actually wake tasks
    : actually-wake-tasks ( -- )
      first-task @ begin ?dup while
	dup task-state h@ task-state-mask and blocked-wait = if
	  readied over task-state h!
	then
	task-prev @
      repeat
    ;

    \ Handle task-switching
    : switch-tasks ( -- )
      r> pendsv-return !

      in-critical @ 0= in-task-change @ 0= and if

	1 pause-count +!

	current-task @ dup prev-task !
	?dup if dup save-task-state reschedule-task then
	
	begin
	  true in-task-change !
	  wake-tasks @ if
	    claim-same-core-spinlock
	    actually-wake-tasks false wake-tasks !
	    release-same-core-spinlock
	  then
	  begin
	    claim-same-core-spinlock
	    find-next-task
	    release-same-core-spinlock
	    dup 0<> if
	      dup task-active@ 1 < if
		claim-same-core-spinlock
		remove-task
		release-same-core-spinlock
		false
	      else
		dup task-state h@
		dup schedule-with-same-core-spinlock and if
		  claim-same-core-spinlock
		then
		dup schedule-critical and if
		  1 in-critical !
		then
		dup schedule-with-spinlock and if
		  over ['] spinlock-to-claim for-task@ claim-spinlock
		then
		task-state-mask and blocked-timeout = if
		  block-timed-out over task-state h!
		else
		  readied over task-state h!
		then
		true
	      then
	    else
	      true
	    then
	  until
	  dup current-task !
	  false in-task-change !
	  0<> if true else sleep false then
	until

	prev-task @ current-task @ <> if
	  disable-int
	  current-task @ task-rstack-current @ context-switch
	  prev-task @ if prev-task @ task-rstack-current ! else drop then
	  current-task @ restore-task-state
	  enable-int
	then

	task-saved-systick-counter @ 0<= if
	  task-saved-systick-counter @ task-timeslice @ +
	  task-min-timeslice @ max
	  task-systick-counter !
	else
	  task-saved-systick-counter @ task-systick-counter !
	then
	
      else
	true deferred-context-switch !
      then

      pendsv-return @ >r

      false in-multitasker? !
    ;

    \ Multitasker systick handler
    : task-systick-handler ( -- )
      systick-handler
      -1 task-systick-counter +!
      task-systick-counter @ 0 <= if
	pause
      then
    ;

    \ Handle PAUSE
    : do-pause ( -- ) true in-multitasker? ! ICSR_PENDSVSET! dmb dsb isb ;
    
    \ Dump task name
    : dump-task-name ( task -- )
      task-name@ ?dup if count tuck type 16 swap - 0 max else 16 then spaces
    ;
    
    \ Dump task state
    : dump-task-state ( task -- )
      dup current-task @ = if
	s" running"
      else
	dup ['] task-current-notify for-task@ -1 <> if
	  s" to-notify"
	else
	  dup task-state h@ task-state-mask and case
	    readied of            s" ready" endof
	    delayed of            s" delayed" endof
	    blocked-timeout of    s" timeout" endof
	    blocked-wait of       s" wait" endof
	    blocked-indefinite of s" indefinite" endof
	    block-timed-out of    s" timed-out" endof
	  endcase
	then
      then
      tuck type 11 swap - spaces
      task-state h@ schedule-user-critical and if
	." yes     "
      else
	." no      "
      then
    ;
    
    \ Dump task priority
    : dump-task-priority ( task -- )
      task-priority@ here swap format-integer dup 8 swap - spaces type
    ;
    
    \ Dump task until time
    : dump-task-until ( task -- )
      dup task-state h@ task-state-mask and case
	delayed of true endof
	blocked-timeout of true endof
	false swap
      endcase
      if
	last-delay + dup here swap format-unsigned
	dup 10 swap - spaces type space
	systick-counter - here swap format-integer dup 11 swap - spaces type
      else
	drop 22 spaces
      then
    ;
    
    \ Dump task ehader
    : dump-task-header ( -- )
      cr ." task     name             priority state      critical "
      ." until      delay"
    ;

  end-module
  
  \ Dump task information for all tasks
  : dump-tasks ( -- )
    dump-task-header
    [:
      first-task @ begin dup 0<> while
	dup
	cr dup h.8 space dup dump-task-name space dup dump-task-priority space
	dup dump-task-state space dump-task-until
	task-prev @
      repeat
      drop
    ;] critical
  ;

  \ Display tracing information
  : trace ( c-addr u -- )
    trace-enabled? @ if
      [:
	cr type space ." critical: " in-critical @ 1- . .s dump-tasks
      ;] critical
    else
      2drop
    then
  ;

  \ Wait for n milliseconds with multitasking support
  : ms1 ( u -- ) systick-divisor * systick-counter current-task @ delay ;

  continue-module task-internal

    \ Wait the current thread
    : do-wait ( -- )
      pause-enabled @ 0> if
	current-task @ block-wait
      then
    ;

    \ RAM dictionary space warning has been displayed
    variable ram-dict-warned

    \ Saved dictionary space validator
    variable saved-validate-dict

    \ Get the actual current dictionary here for a task
    : actual-here ( task -- addr )
      dup current-task @ = if drop ram-here else task-dict-current then
    ;

    \ Validate the dictionary space
    : do-validate-dict ( -- )
      main-task @ task-dict-end main-task @ actual-here - 1024 <
      ram-dict-warned @ not and if
	true ram-dict-warned !
	display-red
	." RAM dictionary space is running low (<1K left)" cr
	display-normal
      then
      saved-validate-dict @ ?execute
    ;

    cpu-count 1 > [if]
      
      \ Initialize an auxiliary core's main task
      : init-aux-main-task ( -- )
	0 fifo-drain
	wait-current-core
	current-task @ task-dict-base @ dict-base !
	$7F SHPR3_PRI_15!
	$FF SHPR2_PRI_11!
	$FF SHPR3_PRI_14!
	$00 SIO_IRQ_PROC1 NVIC_IPR_IP!
	SIO_IRQ_PROC1 NVIC_ISER_SETENA!
	1 pause-enabled !
	^ int-io :: enable-int-io
	init-systick-aux-core
	pause try ?dup if display-red execute display-normal then
	current-task @ kill
      ;
      
    [then]

    \ Core already has a main task spawned exception
    : x-main-already-launched ( -- ) ." core already has main task" cr ;
    
    \ Auxiliary cores can only be launched from core 0
    : x-core-can-only-be-launched-from-core-0 ( -- )
      ." core can only be launched from core 0" cr
    ;

    \ Launch an auxiliary core with a task
    : spawn-aux-main
      ( xn ... x0 count xt dict-size stack-size rstack-size core -- task )
      [ cpu-count 1 > ] [if]
	cpu-index 0= averts x-core-can-only-be-launched-from-core-0
	dup 1 = averts x-core-out-of-range
	claim-all-core-spinlock
	dup cpu-active? @ if
	  release-all-core-spinlock
	  ['] x-main-already-launched ?raise
	else
	  true over cpu-active? !
	then
	release-all-core-spinlock
	>r task-allot >r
	2r@ cpu-current-task !
	2r@ cpu-main-task !
	2r@ cpu-prev-task !
	2r@ cpu-first-task !
	2r@ cpu-last-task !
	2r@ init-aux-task
	['] init-aux-main-task -rot
	2r> swap >r launch-aux-core r>
	begin dup task-core@ cpu-current-task @ 0= until
      [else]
	['] x-core-out-of-range ?raise
      [then]
    ;

  end-module

  \ Spawn a non-main task
  : spawn ( xn...x0 count xt dict-size stack-size rstack-size -- task )
    task-allot dup >r cpu-index init-task r>
  ;

  \ Spawn a task on a particular core
  : spawn-on-core
    ( xn...x0 count xt dict-size stack-size rstack-size core -- task )
    [ cpu-count 1 > ] [if]
      dup cpu-count < over 0 >= and averts x-core-out-of-range
      dup cpu-active? @ if
	>r task-allot r> over >r init-task r>
      else
	spawn-aux-main
      then
    [else]
      0= averts x-core-out-of-range
      spawn
    [then]
  ;

  \ Make current-task read-only
  : current-task ( -- task ) current-task @ ;
  
  \ Make main-task read-only
  : main-task ( -- task ) main-task @ ;
  
  \ Call block if not in multitasker, and return whether in multitasker
  : exclude-multitasker ( xt -- flag )
    disable-int in-multitasker? @ dup not if
      >r execute r>
    else
      nip
    then
    enable-int
  ;

  \ Initialize multitasking
  : init-tasker ( -- )
    disable-int
    0 pause-enabled !
    $7F SHPR3_PRI_15!
    $FF SHPR2_PRI_11!
    $FF SHPR3_PRI_14!
    stack-end @ free-end !
    validate-dict-hook @ saved-validate-dict !
    false ram-dict-warned !
    ['] do-pause pause-hook !
    ['] do-wait wait-hook !
    ['] do-wake wake-hook !
    ['] do-validate-dict validate-dict-hook !
    cpu-count 0 ?do
      false i cpu-in-multitasker? !
      false i cpu-sleep-enabled? !
      false i cpu-trace-enabled? !
      false i cpu-in-task-change !
      false i cpu-wake-tasks !
      0 i cpu-task-systick-counter !
      i 0= if
	true i cpu-active? !
	init-main-task
      else
	false i cpu-active? !
	0 i cpu-main-task !
	0 i cpu-current-task !
	0 i cpu-prev-task !
	0 i cpu-first-task !
	0 i cpu-last-task !
      then
      0 i cpu-pause-count !
    loop
    ['] execute svcall-vector vector!
    ['] switch-tasks pendsv-vector vector!
    ['] task-systick-handler systick-vector vector!
    1 pause-enabled !
    enable-int
  ;

  \ Display space free for a given task
  : task-unused ( task -- )
    cr
    ." dictionary free: "
    dup task-dict-end over task-dict-current - 0 <# #s #> type cr
    ." stack free:      "
    dup task-stack-current over ['] task-stack-end for-task@ -
    0 <# #s #> type cr
    ." rstack free:     "
    dup task-rstack-current @ swap ['] task-rstack-end for-task@ -
    0 <# #s #> type
  ;
  
  \ Display space free for the main task and for flash in general
  : unused ( -- )
    cr main-task @
    ." flash dictionary free:     "
    flash-end flash-here - 0 <# #s #> type cr
    ." main task dictionary free: "
    dup task-dict-end over task-dict-current - 0 <# #s #> type cr
    ." main task stack free:      "
    dup task-stack-current over ['] task-stack-end for-task@ -
    0 <# #s #> type cr
    ." main task rstack free:     "
    dup task-rstack-current @ swap ['] task-rstack-end for-task@ -
    0 <# #s #> type
  ;

end-module> import

\ Display space free for a given task
: task-unused ( task -- ) task-unused ;

\ Display space free for the main task and for flash in general
: unused ( -- ) unused ;

\ Wait for n milliseconds with multitasking support
: ms ( u -- ) ms1 ;

\ Init
: init ( -- )
  init
  init-tasker
;

continue-module task
  
  task-internal import
  multicore import

  \ Make pause-count read-only
  : pause-count ( -- u ) pause-count @ ;

  \ Enable sleep
  : enable-sleep ( -- ) true sleep-enabled? ! ;

  \ Disable sleep
  : disable-sleep ( -- ) false sleep-enabled? ! ;

  \ Get whether sleep is enabled
  : sleep-enabled? ( -- flag ) sleep-enabled? @ ;

  \ Enable tracing
  : enable-trace ( -- ) true trace-enabled? ! ;

  \ Disable tracing
  : disable-trace ( -- ) false trace-enabled? ! ;

  \ Get whether tracing is enabled
  : trace-enable? ( -- flag ) trace-enabled? @ ;

  \ Allot memory from the end of RAM
  : allot-end ( u -- addr )
    claim-all-core-spinlock
    negate free-end +! free-end @
    release-all-core-spinlock
  ;

end-module

\ Reboot to initialize multitasking
reboot