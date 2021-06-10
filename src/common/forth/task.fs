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
forth-module 1 set-order
forth-module set-current

\ Compile this to flash
compile-to-flash

begin-import-module-once task-module

  import internal-module
  import interrupt-module
  import systick-module
  import int-io-module

  begin-import-module task-internal-module
  
    \ Main task
    variable main-task
    
    \ Current task
    variable current-task
    
    \ Has current task changed
    variable current-task-changed?
    
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

    \ Whether a task is waiting
    user task-wait

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

    \ SVCall vector index
    11 constant svcall-vector

    \ PendSV vector index
    14 constant pendsv-vector
    
  end-module

  \ The currently waited-for lock
  user current-lock
  
  \ The latest lock currently held by a tack
  user current-lock-held

  \ Sleep
  : sleep ( -- ) sleep-enabled? @ if sleep then ;

  begin-module task-internal-module

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

      \ Task saved priority
      field: task-saved-priority

      \ Next task
      field: task-next
    end-structure

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

    \ Get task return stack end
    : task-rstack-end ( task -- addr ) task-stack-base ;

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

  begin-module task-internal-module

    \ Find the previous task
    : prev-task ( task1 -- task2 )
      dup begin dup task-next @ 2 pick <> while task-next @ repeat
      tuck = if
	drop 0
      then
    ;

    \ Display a task cycle
    : show-cycle ( task always? -- )
      0 pause-enabled !
      swap dup dup task-next @ <> rot or if
	dup h.8 dup ['] task-wait for-task @ if [char] * emit then
	dup task-next @ begin dup 2 pick <> while
	  space ." -> "
	  dup h.8
	  dup ['] task-wait for-task @ if [char] * emit then
	  task-next @
	repeat
	2drop
	cr
      else
	drop
      then
      1 pause-enabled !
    ;

    \ Link a task into the main loop
    : link-task ( task -- )
      current-task @ if
	current-task @ prev-task ?dup if
	  over swap task-next !
	else
	  dup current-task @ task-next !
	then
	current-task @ swap task-next !
      else
	0 task-systick-counter !
	dup dup task-next !
	current-task !
	true current-task-changed? !
      then
    ;

    \ Link a task into the head of the main loop
    : link-first-task ( task -- )
      current-task @ 0<> if
	dup current-task @ <> if
	  current-task @ task-next @ over task-next !
	  current-task @ task-next !
	else
	  drop
	then
      else
	0 task-systick-counter !
	dup current-task !
	dup dup task-next !
	true current-task-changed? !
      then
    ;

    \ Unlink a task from the main loop
    : unlink-task ( task -- )
      dup prev-task ?dup if
	over current-task @ = if
	  over task-next @ current-task !
	  true current-task-changed? !
	then
	swap task-next @ swap task-next !
      else
	drop 0 current-task !
	true current-task-changed? !
      then
    ;
    
  end-module

  \ Get task active sate
  : get-task-active ( task -- active )
    task-active h@ 16 lshift 16 arshift
  ;

  \ Get task priority
  : get-task-priority ( task -- priority )
    task-priority h@ 16 lshift 16 arshift
  ;

  \ Get saved task priority
  : get-task-saved-priority ( task -- priority )
    task-saved-priority @
  ;

  \ Out of range task priority exception
  : x-out-of-range-priority ( -- ) space ." out of range priority" cr ;

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
    task-saved-priority !
  ;

  \ Attempted to use a terminated task
  : x-terminated ( -- ) space ." task has been terminated" cr ;

  \ Validate task is not terminated
  : validate-not-terminated ( task -- )
    task-active h@ terminated = if enable-int ['] x-terminated ?raise then
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
    dup validate-not-terminated
    ['] task-timeslice for-task @
  ;

  \ Set task maximum timeslice
  : set-task-max-timeslice ( max-timeslice task -- )
    dup validate-not-terminated
    ['] task-max-timeslice for-task !
  ;

  \ Get task maximum timeslice
  : get-task-max-timeslice ( task -- max-timeslice )
    dup validate-not-terminated
    ['] task-max-timeslice for-task @
  ;
  
  \ Start a task's execution
  : run ( task -- )
    disable-int
    dup validate-not-terminated
    dup get-task-active 1+
    dup 1 = if over link-task then
    swap task-active h!
    enable-int
  ;

  \ Activate a task (i.e. enable it and move it to the head of the queue)
  : activate ( task -- )
    disable-int
    dup validate-not-terminated
    dup get-task-active 1+
    dup 1 = if
      over link-first-task
    else
      dup 1 > if
	over unlink-task over link-first-task
      then
    then
    swap task-active h!
    enable-int
  ;

  \ Stop a task's execution
  : stop ( task -- )
    disable-int
    dup validate-not-terminated
    dup get-task-active 1-
    dup 0= if over unlink-task then
    swap task-active h!
    enable-int
  ;

  \ Kill a task
  : kill ( task -- )
    disable-int
    dup validate-not-terminated
    dup get-task-active 0> if dup unlink-task then
    terminated over task-active h!
    dup current-task @ = swap last-task @ = or if
      enable-int begin pause again
    else
      enable-int
    then
  ;
  
  \ Mark a task as waiting
  : wait-task ( task -- )
    dup validate-not-terminated
    true swap ['] task-wait for-task !
  ;

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
      false task-wait !
      0 task-systick-start !
      -1 task-systick-delay !
      base @ task-base !
      default-timeslice task-timeslice !
      default-max-timeslice task-max-timeslice !
      0 current-lock !
      0 current-lock-held !
      dup dup task-next !
      dup main-task !
      dup last-task !
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
    false over ['] task-wait for-task !
    0 over ['] task-systick-start for-task !
    -1 over ['] task-systick-delay for-task !
    default-timeslice over ['] task-timeslice for-task !
    default-max-timeslice over ['] task-max-timeslice for-task !
    dup task-rstack-base over task-stack-base ['] task-entry
    ['] init-context svc over task-rstack-current !
    next-user-space over task-dict-offset !
    0 over task-next !
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
    
    \ Reset waits
    : reset-waiting-tasks ( task -- )
      dup >r
      begin
	false over ['] task-wait for-task !
	task-next @
	dup r@ =
      until
      drop rdrop
    ;

    \ Wake tasks
    : do-wake ( -- )
      current-task @ if
	current-task @ reset-waiting-tasks
      then
    ;

    \ Get whether a task is waiting
    : waiting-task? ( task -- )
      >r
      r@ ['] task-wait for-task @
      r@ ['] task-systick-delay for-task @ -1 <>
      systick-counter r@ ['] task-systick-start for-task @ -
      r@ ['] task-systick-delay for-task @ u< and or
      rdrop
    ;

    \ Handle waiting tasks
    : handle-waiting-tasks ( task -- )
      dup waiting-task? if
	enable-int
	sleep
	disable-int
	reset-waiting-tasks
      else
	drop
      then
    ;

    \ Get the highest task priority
    : get-highest-priority ( -- priority )
      -32768 current-task @ begin
	dup waiting-task? not if
	  dup get-task-priority rot max swap
	then
	task-next @ dup current-task @ =
      until
      drop
    ;

    \ Go to the next task
    : go-to-next-task ( task -- task )
      dup >r get-highest-priority swap
      begin
	task-next @ dup get-task-priority 2 pick = if
	  dup waiting-task? not over r@ = or
	else
	  dup r@ =
	then
      until
      nip dup r@ = if dup handle-waiting-tasks then
      rdrop
    ;

    \ PendSV return value
    variable pendsv-return

    \ The PendSV handler
    : pendsv-handler ( -- )
      r> pendsv-return !

\      disable-int
      
      in-critical @ 0= if
	  
	1 pause-count +!
	begin
	  task-io
	  current-task @ 0<>
	  dup not if
\	    enable-int
	    sleep
\	    disable-int
	    0 task-systick-counter !
	  then
	until

	last-task @ if
	  ram-here last-task @ task-dict-current!
	  handler @ last-task @ ['] task-handler for-task !
	  base @ last-task @ ['] task-base for-task !
	then

	current-task-changed? @ not if
	  current-task @ go-to-next-task current-task !
	then

	disable-int
	
	current-task @ last-task @ <> if
	  current-task @ task-rstack-current @ context-switch
	  last-task @ if last-task @ task-rstack-current ! else drop then
	then

	current-task @ dup last-task !
	false current-task-changed? !
	dup task-stack-base stack-base !
	dup task-stack-end stack-end !
	dup task-rstack-base rstack-base !
	dup task-rstack-end rstack-end !
	dup task-dict-current ram-here!
	task-dict-base dict-base !
	task-base @ base !
	task-handler @ handler !

	enable-int
	
	task-timeslice @
	task-max-timeslice @ task-timeslice @ max min task-systick-counter !
	
      else
	true deferred-context-switch !
      then

\      enable-int
      
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
    : do-pause ( -- ) true in-multitasker? ! ICSR_PENDSVSET! dsb isb ;

  end-module

  \ Start a delay from the present
  : start-task-delay ( 1/10m-delay task -- )
    dup validate-not-terminated
    begin-critical
    dup systick-counter swap ['] task-systick-start for-task !
    ['] task-systick-delay for-task !
    end-critical
  ;

  \ Set a delay for a task
  : set-task-delay ( 1/10ms-delay 1/10ms-start task -- )
    dup validate-not-terminated
    begin-critical
    tuck ['] task-systick-start for-task !
    ['] task-systick-delay for-task !
    end-critical
  ;

  \ Advance a delay for a task by a given amount of time
  : advance-task-delay ( 1/10ms-offset task -- )
    dup validate-not-terminated
    begin-critical
    systick-counter over ['] task-systick-start for-task @ -
    over ['] task-systick-delay for-task @ < if
      ['] task-systick-delay for-task +!
    else
      dup ['] task-systick-delay for-task @
      over ['] task-systick-start for-task +!
      ['] task-systick-delay for-task !
    then
    end-critical
  ;

  \ Advance of start a delay from the present, depending on whether the delay
  \ length has changed
  : reset-task-delay ( 1/10ms-delay task -- )
    dup validate-not-terminated
    begin-critical
    dup ['] task-systick-delay for-task @ 2 pick = if
      advance-task-delay
    else
      start-task-delay
    then
    end-critical
  ;

  \ Get a delay for a task
  : get-task-delay ( task -- 1/10ms-delay 1/10ms-start )
    dup validate-not-terminated
    begin-critical
    dup ['] task-systick-delay for-task @
    over ['] task-systick-start for-task @
    end-critical
  ;

  \ Cancel a delay for a task
  : cancel-task-delay ( task -- )
    dup validate-not-terminated
    begin-critical
    0 over ['] task-systick-start for-task !
    -1 swap ['] task-systick-delay for-task !
    end-critical
  ;

  \ Wait for n milliseconds with multitasking support
  : ms ( u -- )
    systick-divisor * systick-counter
    2dup current-task @ set-task-delay
    begin
      dup systick-counter swap - 2 pick u<
    while
      pause
    repeat
    drop drop
    current-task @ cancel-task-delay
  ;

  begin-import-module task-internal-module

    \ Wait the current thread
    : do-wait ( -- )
      pause-enabled @ 0> if
	current-task @ wait-task
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
    $7F SHPR3_PRI_15!
    $FF SHPR2_PRI_11!
    $FF SHPR3_PRI_14!
    0 task-systick-counter !
    stack-end @ free-end !
    init-main-task
    0 pause-count !
    false current-task-changed? !
    ['] do-pause pause-hook !
    ['] do-wait wait-hook !
    ['] do-wake wake-hook !
    validate-dict-hook @ saved-validate-dict !
    false ram-dict-warned !
    ['] do-validate-dict validate-dict-hook !
    ['] execute svcall-vector vector!
    ['] pendsv-handler pendsv-vector vector!
    ['] task-systick-handler systick-vector vector!
    1 pause-enabled !
    enable-int
  ;
  
  \ Forget RAM contents
  : forget-ram ( -- )
    0 ram-latest!
    latest $20000000 >= if 0 latest! then
    0 pause-enabled !
    min-ram-wordlist current-ram-wordlist !
    next-ram-space dict-base !
    dict-base @ next-user-space + ram-here!
    0 wait-hook !
    false flash-dict-warned !
    ['] do-flash-validate-dict saved-validate-dict !
    main-task @ task-stack-end free-end !
    init-main-task
    0 pause-count !
    ['] do-pause pause-hook !
    ['] do-wait wait-hook !
    false ram-dict-warned !
    ['] do-validate-dict validate-dict-hook !
    1 pause-enabled !
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

\ Forget RAM contents
: forget-ram ( -- ) forget-ram ;

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
warm