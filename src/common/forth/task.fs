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

\ Compile this to flash
compile-to-flash

begin-import-module-once task-module

  import internal-module
  import interrupt-module
  import systick-module
  import int-io-module

  begin-import-module task-internal-module

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

    \ Schedule into critical section bit
    $8000 constant schedule-critical

    \ Schedule into critical section mask
    $7FFF constant schedule-critical-mask
    
    \ In task change
    variable in-task-change

    \ Wake tasks flag
    variable wake-tasks
    
    \ Main task
    variable main-task
    
    \ Current task
    variable current-task

    \ Previous task
    variable prev-task

    \ First task
    variable first-task
    
    \ Last task
    variable last-task

    \ Declare a RAM variable for the end of free RAM memory
    variable free-end

    \ Pause count
    variable pause-count

    \ Currently in multitasker
    variable in-multitasker?

    \ The original SysTIck handler
    variable orig-systick-handler

    \ The multitasker SysTick counter
    variable task-systick-counter

    \ The default timeslice
    10 constant default-timeslice

    \ The default maximum timeslice
    20 constant default-max-timeslice
    
    \ Sleep is enabled
    variable sleep-enabled?
    
    \ The current task handler
    user task-handler

    \ Task systick start time
    user task-systick-start

    \ Task systick delay
    user task-systick-delay

    \ The current base for a task
    user task-base

    \ A task's timeslice
    user task-timeslice

    \ A task's maximum timeslice
    user task-max-timeslice

    \ The current timeout start time in ticks
    user timeout-systick-start
    
    \ The current timeout delay time in ticks
    user timeout-systick-delay
  
    \ The task's name as a counted string
    user task-name
  
    \ SVCall vector index
    11 constant svcall-vector

    \ PendSV vector index
    14 constant pendsv-vector
    
    \ Terminated task
    $8000 constant terminated
    
    \ The task structure
    begin-structure task
      \ Return stack size
      hfield: task-rstack-size
      
      \ Data stack size
      hfield: task-stack-size

      \ Dictionary size
      field: task-dict-size

      \ Current return stack adress
      field: task-rstack-current

      \ Current dictionary offset
      field: task-dict-offset

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

  end-module

  \ The currently waited-for lock
  user current-lock
  
  \ The latest lock currently held by a tack
  user current-lock-held

  \ The default timeout
  user timeout

  \ No timeout
  -1 constant no-timeout
  
  \ Attempted to use a terminated task
  : x-terminated ( -- ) space ." task has been terminated" cr ;

  \ Would block exception
  : x-would-block ( -- ) space ." operation would block" cr ;
  
  \ Attempted to task change while changing tasks during interrupt
  : x-in-task-change ( -- ) space ." in task change" cr ;

  \ Out of range task priority exception
  : x-out-of-range-priority ( -- ) space ." out of range priority" cr ;

  \ Timed out exception
  : x-timed-out ( -- ) space ." block timed out" cr ;

  \ Sleep
  : sleep ( -- ) sleep-enabled? @ if sleep then ;

  begin-module task-internal-module

    \ Get task stack base
    : task-stack-base ( task -- addr )
      dup task + swap task-stack-size h@ +
    ;

    \ Get task stack end
    : task-stack-end ( task -- addr )
      task +
    ;

    \ Get task return stack base
    : task-rstack-base ( task -- addr )
      dup task + over task-stack-size h@ + swap task-rstack-size h@ +
    ;

    \ Get task dictionary base
    : task-dict-base ( task -- addr ) dup task-dict-size @ - ;

    \ Get task dictionary end
    : task-dict-end ( task -- addr ) ;

    \ Get task current stack address
    : task-stack-current ( task -- addr )
      dup last-task @ = if drop sp@ else task-rstack-current @ 12 + @ then
    ;

    \ Get task current dictionary address
    : task-dict-current ( task -- addr )
      dup task-dict-base swap task-dict-offset @ +
    ;

    \ Set task current stack address
    : task-stack-current! ( addr task -- )
      dup last-task @ = if drop sp! else task-rstack-current @ 12 + ! then
    ;

    \ Get task return stack end
    : task-rstack-end ( task -- addr ) task-stack-base ;

    \ Set task current dictionary address
    : task-dict-current! ( addr task -- )
      dup task-dict-base rot swap - swap task-dict-offset !
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
  
  \ Access a given user variable for a given task
  : for-task ( task xt -- addr )
    execute dict-base @ - swap task-dict-base +
  ;

  \ Get task priority
  : get-task-priority ( task -- priority )
    task-priority h@ 16 lshift 16 arshift
  ;

  \ Validate task is not terminated
  : validate-not-terminated ( task -- )
    task-active h@ terminated = triggers x-terminated
  ;

  begin-module task-internal-module
    
    \ Find the next task with a higher priority; 0 returned indicates no task
    \ exists with a higher priority.
    : find-higher-priority ( task -- higher-task )
      get-task-priority >r
      last-task @ begin
	dup 0<> if
	  dup get-task-priority r@ < if task-next @ false else true then
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
	over last-task !
      then
      2dup task-prev !
      swap task-next !
    ;

    \ Insert a task into first position
    : insert-task-first ( task -- )
      first-task @ 0<> if
	dup first-task @ task-next !
	first-task @ over task-prev !
      else
	dup last-task !
	0 over task-prev !
      then
      dup first-task !
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
      dup task-next @ ?dup if
	over task-prev @ swap task-prev !
      else
	dup task-prev @ first-task !
      then
      dup task-prev @ ?dup if
	over task-next @ swap task-next !
      else
	dup task-next @ last-task !
      then
      0 over task-prev !
      0 swap task-next !
    ;

    \ Remove a task if it is scheduled
    : test-remove-task ( task -- )
      dup task-prev @ 0<> over task-next @ 0<> or if remove-task else drop then
    ;

    \ Set task change
    : start-task-change ( -- )
      disable-int
      begin in-task-change @ while
	ICSR_VECTACTIVE@ 0 = if
	  enable-int
	  pause
	  disable-int
	else
	  enable-int
	  ['] x-in-task-change ?raise
	then
      repeat
      true in-task-change !
      enable-int
    ;

    \ Start a task change
    : start-validate-task-change ( task -- )
      start-task-change
      ['] validate-not-terminated try ?dup if false in-task-change ! ?raise then
    ;
  
  end-module

  \ Get task active sate
  : get-task-active ( task -- active )
    task-active h@ 16 lshift 16 arshift
  ;

  \ Get saved task priority
  : get-task-saved-priority ( task -- priority )
    task-saved-priority h@
  ;

  \ Set task priority
  : set-task-priority ( priority task -- )
    over -32768 < triggers x-out-of-range-priority
    over 32767 > triggers x-out-of-range-priority
    task-priority h!
  ;

  \ Set task saved priority
  : set-task-saved-priority ( priority task -- )
    over -32768 < triggers x-out-of-range-priority
    over 32767 > triggers x-out-of-range-priority
    task-saved-priority h!
  ;

  \ Set task timeslice
  : set-task-timeslice ( timeslice task -- )
    disable-int
    dup validate-not-terminated
    dup current-task @ = if
      2dup ['] task-timeslice for-task @ - task-systick-counter @ + 0 max
      task-systick-counter !
    then
    swap 0 max swap ['] task-timeslice for-task !
    enable-int
  ;

  \ Get task timeslice
  : get-task-timeslice ( task -- timeslice )
    dup validate-not-terminated ['] task-timeslice for-task @
  ;

  \ Set task maximum timeslice
  : set-task-max-timeslice ( max-timeslice task -- )
    dup validate-not-terminated ['] task-max-timeslice for-task !
  ;

  \ Get task maximum timeslice
  : get-task-max-timeslice ( task -- max-timeslice )
    dup validate-not-terminated ['] task-max-timeslice for-task @
  ;

  \ Get a task's name as a counted string; an address of zero indicates no name
  \ is set.
  : get-task-name ( task -- addr )
    dup validate-not-terminated ['] task-name for-task @
  ;

  \ Set a task's name as a counted string; an address of zero indicates to set
  \ no name.
  : set-task-name ( addr -- task )
    dup validate-not-terminated ['] task-name for-task !
  ;

  \ Start a task's execution
  : run ( task -- )
    dup start-validate-task-change
    dup get-task-active 1+
    dup 1 = if over test-remove-task over insert-task then
    swap task-active h!
    false in-task-change !
  ;

  \ Stop a task's execution
  : stop ( task -- )
    dup start-validate-task-change
    dup get-task-active 1-
    swap task-active h!
    false in-task-change !
  ;

  \ Kill a task
  : kill ( task -- )
    dup start-validate-task-change
    terminated over task-active h!
    dup current-task @ = swap last-task @ = or if
      false in-task-change ! begin pause again
    else
      false in-task-change !
    then
  ;

  \ Get the last delay time
  : last-delay ( task -- ticks-delay ticks-start )
    [:
      dup validate-not-terminated
      dup ['] task-systick-delay for-task @
      swap ['] task-systick-start for-task @
    ;] critical
  ;

  \ Delay a task
  : delay ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      tuck ['] task-systick-start for-task !
      tuck ['] task-systick-delay for-task !
      delayed swap task-state h!
    ;] critical
    pause
  ;

  \ Delay a task and schedule as critcal once done
  : delay-critical ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      tuck ['] task-systick-start for-task !
      tuck ['] task-systick-delay for-task !
      [ delayed schedule-critical or ] literal swap task-state h!
    ;] critical
    pause
  ;

  \ Mark a task as blocked until a timeout
  : block-timeout ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      tuck ['] task-systick-start for-task !
      tuck ['] task-systick-delay for-task !
      blocked-timeout swap task-state h!
    ;] critical
    pause
  ;

  \ Mark a task as blocked until a timeout and schedule as critical once done
  : block-timeout-critical ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      tuck ['] task-systick-start for-task !
      tuck ['] task-systick-delay for-task !
      [ blocked-timeout schedule-critical or ] literal swap task-state h!
    ;] critical
    pause
  ;

  \ Mark a task as waiting
  : block-wait ( task -- )
    dup validate-not-terminated
    blocked-wait swap task-state h!
    pause
  ;
  
  \ Mark a task as waiting
  : block-wait-critical ( task -- )
    dup validate-not-terminated
    [ blocked-wait schedule-critical or ] literal swap task-state h!
    pause
  ;

  \ Mark a task as blocked indefinitely
  : block-indefinite ( task -- )
    dup validate-not-terminated
    blocked-indefinite swap task-state h!
    pause
  ;

  \ Mark a task as blocked indefinitely and schedule as critical when done
  : block-indefinite-critical ( task -- )
    dup validate-not-terminated
    [ blocked-indefinite schedule-critical or ] literal swap task-state h!
    pause
  ;

  \ Ready a task
  : ready ( task -- )
    dup validate-not-terminated
    readied swap task-state h!
    pause
  ;

  \ Block a task for the specified initialized timeout
  : block ( task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task @ no-timeout <> if
	dup ['] timeout-systick-delay for-task @
	over ['] timeout-systick-start for-task @
	rot block-timeout
      else
	block-indefinite
      then
    ;] critical
  ;

  \ Block a task for the specified initialized timeout and schedule as critical
  \ once done
  : block-critical ( task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task @ no-timeout <> if
	dup ['] timeout-systick-delay for-task @
	over ['] timeout-systick-start for-task @
	rot block-timeout-critical
      else
	block-indefinite-critical
      then
    ;] critical
  ;

  \ Prepare blocking for a task
  : prepare-block ( task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task @ no-timeout <> if
	systick-counter over ['] timeout-systick-start for-task !
	dup ['] timeout for-task @ swap ['] timeout-systick-delay for-task !
      else
	drop
      then
    ;] critical
  ;

  \ Get whether a task has timed out
  : timed-out? ( task -- timed-out )
    dup validate-not-terminated task-state h@ schedule-critical-mask and
    block-timed-out =
  ;

  \ Validate not timing out
  : validate-timeout ( task -- ) timed-out? triggers x-timed-out ;

  \ Get whether a task has terminated
  : terminated? ( task -- terminated ) task-active h@ terminated = ;

  begin-module task-internal-module
    
    \ Initialize the main task
    : init-main-task ( -- )
      free-end @ task -
      rstack-base @ rstack-end @ - over task-rstack-size h!
      stack-base @ stack-end @ - over task-stack-size h!
      free-end @ task - next-ram-space - over task-dict-size !
      rp@ over task-rstack-current !
      ram-here next-ram-space - over task-dict-offset !
      0 over task-priority h!
      0 over task-saved-priority !
      1 over task-active h!
      readied over task-state h!
      no-timeout timeout !
      0 timeout-systick-start !
      0 timeout-systick-delay !
      0 task-systick-start !
      -1 task-systick-delay !
      c" main" task-name !
      base @ task-base !
      default-timeslice task-timeslice !
      default-max-timeslice task-max-timeslice !
      0 current-lock !
      0 current-lock-held !
      0 over task-next !
      0 over task-prev !
      dup main-task !
      dup first-task !
      dup last-task !
      dup prev-task !
      current-task !
      free-end @ task - free-end !
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
      systick-counter over ['] task-systick-start for-task @ -
      swap ['] task-systick-delay for-task @ <
    ;

  end-module

  \ Initialize a task
  : init-task ( xn...x0 count xt task -- )
    0 over ['] task-handler for-task !
    0 over task-priority h!
    0 over task-saved-priority !
    0 over task-active h!
    base @ over ['] task-base for-task !
    0 over ['] current-lock for-task !
    0 over ['] current-lock-held for-task !
    readied over task-state h!
    no-timeout over ['] timeout for-task !
    0 over ['] timeout-systick-start for-task !
    0 over ['] timeout-systick-delay for-task !
    0 over ['] task-systick-start for-task !
    0 over ['] task-name for-task !
    -1 over ['] task-systick-delay for-task !
    default-timeslice over ['] task-timeslice for-task !
    default-max-timeslice over ['] task-max-timeslice for-task !
    dup task-rstack-base over task-stack-base ['] task-entry
    ['] init-context svc over task-rstack-current !
    next-user-space over task-dict-offset !
    0 over task-next !
    0 over task-prev !
    swap >r >r
    begin dup 0<> while
      dup roll r@ push-task-stack 1-
    repeat
    drop r> r> swap push-task-stack
  ;

  \ Spawn a non-main task
  : spawn ( xn...x0 count xt dict-size stack-size rstack-size -- task )
    2dup + task +
    free-end @ swap -
    swap 4 align swap tuck task-rstack-size h!
    swap 4 align swap tuck task-stack-size h!
    swap 4 align swap tuck task-dict-size !
    dup dup task-dict-size @ - free-end !
    dup >r init-task r>
  ;

  begin-module task-internal-module

    \ Wake tasks
    : do-wake ( -- ) true wake-tasks ! ;

    \ Get whether a task is waiting
    : waiting-task? ( task -- )
      dup task-state h@ schedule-critical-mask and
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
      ram-here over task-dict-current!
      handler @ over ['] task-handler for-task !
      base @ swap ['] task-base for-task !
    ;

    \ Restore task state
    : restore-task-state ( task -- )
      dup task-stack-base stack-base !
      dup task-stack-end stack-end !
      dup task-rstack-base rstack-base !
      dup task-rstack-end rstack-end !
      dup task-dict-current ram-here!
      task-dict-base dict-base !
      task-base @ base !
      task-handler @ handler !
    ;

    \ Reschedule previous task
    : reschedule-task ( task -- )
      true in-task-change !
      dup remove-task
      dup get-task-active 0> if insert-task else drop then
      false in-task-change !
    ;

    \ Actually wake tasks
    : actually-wake-tasks ( -- )
      first-task @ begin ?dup while
	dup task-state h@ blocked-wait = if
	  readied over task-state h!
	then
	task-prev @
      repeat
    ;

    \ Handle task-switching
    : switch-tasks ( -- )
      dmb dsb isb
      r> pendsv-return !

      in-critical @ 0= in-task-change @ 0= and if

	current-task @ dup prev-task !
	
	?dup if dup save-task-state reschedule-task then

	begin
	  true in-task-change !
	  wake-tasks @ if actually-wake-tasks false wake-tasks ! then
	  begin
	    find-next-task dup 0<> if
	      dup get-task-active 1 < if
		remove-task false
	      else
		dup task-state h@ schedule-critical and if
		  1 in-critical !
		then
		dup task-state h@ schedule-critical-mask and
		blocked-timeout = if
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
	  current-task !
	  false in-task-change !
	  current-task @ if true else sleep false then
	until

	prev-task @ current-task @ <> if
	  disable-int
	  current-task @ task-rstack-current @ context-switch
	  prev-task @ if prev-task @ task-rstack-current ! else drop then
	  current-task @ restore-task-state
	  enable-int
	then

	task-timeslice @
	task-max-timeslice @ task-timeslice @ max min task-systick-counter !
	
      else
	true deferred-context-switch !
      then

      pendsv-return @ >r

      false in-multitasker? !
      dmb dsb isb
    ;

    \ Multitasker systick handler
    : task-systick-handler ( -- )
      dmb dsb isb
      systick-handler
      -1 task-systick-counter +!
      task-systick-counter @ 0 <= if
	pause
      then
      dmb dsb isb
    ;

    \ If this is not a Cortex-M0(+) MCU

    m0-architecture not [if]
    
      \ Handle PAUSE
      : do-pause ( -- ) true in-multitasker? ! ICSR_PENDSVSET! dmb dsb isb ;

    [else]

      \ Handle Pause
      : do-pause ( -- ) true in-multitasker? ! ['] switch-tasks svc ;
      
    [then]
    
    \ Dump task name
    : dump-task-name ( task -- )
      get-task-name ?dup if count tuck type 16 swap - 0 max else 16 then spaces
    ;
    
    \ Dump task state
    : dump-task-state ( task -- )
      dup current-task @ = if
	s" running"
      else
	dup task-state h@ schedule-critical-mask and case
	  readied of            s" ready" endof
	  delayed of            s" delayed" endof
	  blocked-timeout of    s" timeout" endof
	  blocked-wait of       s" wait" endof
	  blocked-indefinite of s" indefinite" endof
	  block-timed-out of    s" timed-out" endof
	endcase
      then
      tuck type 11 swap - spaces
      task-state h@ schedule-critical and if ." yes     " else ." no      " then
    ;
    
    \ Dump task priority
    : dump-task-priority ( task -- )
      get-task-priority here swap format-integer dup 8 swap - spaces type
    ;
    
    \ Dump task until time
    : dump-task-until ( task -- )
      dup task-state h@ schedule-critical-mask and case
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

  \ Wait for n milliseconds with multitasking support
  : ms ( u -- )
    systick-divisor * systick-counter
    2dup current-task @ delay
    begin
      dup systick-counter swap - 2 pick u<
    while
      pause
    repeat
    drop drop
  ;

  begin-import-module task-internal-module

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
	space ." RAM dictionary space is running low (<1K left)" cr
      then
      saved-validate-dict @ ?execute
    ; 

  end-module

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
    false in-multitasker? !
    0 pause-enabled !
    false sleep-enabled? !
    false in-task-change !
    false wake-tasks !
    $7F SHPR3_PRI_15!
    $FF SHPR2_PRI_11!
    $FF SHPR3_PRI_14!
    0 task-systick-counter !
    stack-end @ free-end !
    init-main-task
    0 pause-count !
    ['] do-pause pause-hook !
    ['] do-wait wait-hook !
    ['] do-wake wake-hook !
    validate-dict-hook @ saved-validate-dict !
    false ram-dict-warned !
    ['] do-validate-dict validate-dict-hook !
    ['] execute svcall-vector vector!
    [ m0-architecture ] [if] ['] switch-tasks pendsv-vector vector! [then]
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
    dup task-stack-current over task-stack-end - 0 <# #s #> type cr
    ." rstack free:     "
    dup task-rstack-current @ swap task-rstack-end - 0 <# #s #> type
  ;
  
  \ Display space free for the main task and for flash in general
  : unused ( -- )
    cr main-task @
    ." flash dictionary free:     "
    flash-end flash-here - 0 <# #s #> type cr
    ." main task dictionary free: "
    dup task-dict-end over task-dict-current - 0 <# #s #> type cr
    ." main task stack free:      "
    dup task-stack-current over task-stack-end - 0 <# #s #> type cr
    ." main task rstack free:     "
    dup task-rstack-current @ swap task-rstack-end - 0 <# #s #> type
  ;

end-module

\ Display space free for a given task
: task-unused ( task -- ) task-unused ;

\ Display space free for the main task and for flash in general
: unused ( -- ) unused ;

\ Wait for n milliseconds with multitasking support
: ms ( u -- ) ms ;

\ Init
: init ( -- )
  init
  init-tasker
;

begin-module task-module
  
  import task-internal-module
  
  \ Make pause-count read-only
  : pause-count ( -- u ) pause-count @ ;

  \ Enable sleep
  : enable-sleep ( -- ) true sleep-enabled? ! ;

  \ Disable sleep
  : disable-sleep ( -- ) false sleep-enabled? ! ;

  \ Get whether sleep is enabled
  : sleep-enabled? ( -- flag ) sleep-enabled? @ ;

  \ Allot memory from the end of RAM
  : allot-end ( u -- addr ) negate free-end +! free-end @ ;

end-module

unimport task-module

\ Reboot to initialize multitasking
reboot