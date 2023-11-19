\ Copyright (c) 2020-2023 Travis Bemann
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
  armv6m import
  
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

    \ The maximum task priority
    $7FFF constant max-priority

    \ Task has not terminated
    0 constant not-terminated

    \ Task has terminated normally
    1 constant terminated-normally

    \ Task has been killed
    2 constant terminated-killed

    \ Task has terminated due to a hardware exception
    3 constant terminated-crashed
    
    \ In task change
    cpu-variable cpu-in-task-change in-task-change

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

    \ Do a pause
    cpu-variable cpu-do-pause? do-pause?
    
    \ Currently in multitasker
    cpu-variable cpu-in-multitasker? in-multitasker?

    \ The multitasker SysTick counter
    cpu-variable cpu-task-systick-counter task-systick-counter

    \ The default timeslice
    10 constant default-timeslice

    \ The default minimum timeslice
    0 constant default-min-timeslice
    
    \ Sleep is enabled
    cpu-variable cpu-sleep-enabled? sleep-enabled?

    \ CPU is active
    cpu-count cells buffer: cpu-active?
    : cpu-active? ( index -- addr ) cells cpu-active? + ;

    \ Whether to keep the current task at the head of the schedule
    cpu-variable cpu-reschedule? reschedule?

    \ Whether to force a task to be rescheduled last in the schedule
    cpu-variable cpu-reschedule-last? reschedule-last?
    
    \ The current terminated task
    cpu-variable cpu-terminated-task terminated-task

    \ Extra task (for cleaning up after terminated tasks)
    cpu-variable cpu-extra-task extra-task

    \ First pending operation
    cpu-variable cpu-first-pending-op first-pending-op

    \ SVCall vector index
    11 constant svcall-vector

    \ PendSV vector index
    14 constant pendsv-vector
    
    \ Terminated task
    $8000 constant terminated
    
    \ The task structure
    begin-structure task-size
      \ Return stack size
      dup constant .task-rstack-size hfield: task-rstack-size
      
      \ Data stack size
      dup constant .task-stack-size hfield: task-stack-size

      \ Dictionary size
      dup constant .task-dict-size field: task-dict-size

      \ Current return stack adress
      dup constant .task-rstack-current field: task-rstack-current

      \ Current dictionary base
      dup constant .task-dict-base field: task-dict-base
      
      \ Task priority
      dup constant .task-priority hfield: task-priority
      
      \ Task active state ( > 0 active, <= 0 inactive, $8000 terminated )
      dup constant .task-active hfield: task-active

      \ Whether a task is waiting
      dup constant .task-state hfield: task-state

      \ Task saved priority
      dup constant .task-saved-priority hfield: task-saved-priority

      \ Prev task
      dup constant .task-prev field: task-prev
      
      \ Next task
      dup constant .task-next field: task-next

      \ The saved multitasker SysTick counter
      dup constant .task-saved-systick-counter field: task-saved-systick-counter
    
      \ The task ready count
      dup constant .task-ready-count field: task-ready-count
      
      \ Task systick start time
      dup constant .task-systick-start field: task-systick-start
      
      \ Task systick delay
      dup constant .task-systick-delay field: task-systick-delay
      
      \ A task's timeslice
      dup constant .task-timeslice field: task-timeslice
      
      \ A task's minimum timeslice
      dup constant .task-min-timeslice field: task-min-timeslice

      \ The current timeout start time in ticks
      dup constant .timeout-systick-start field: timeout-systick-start
      
      \ The current timeout delay time in ticks
      dup constant .timeout-systick-delay field: timeout-systick-delay
  
      \ The task's name as a counted string
      dup constant .task-name field: task-name

      \ The notification being waited on
      dup constant .task-current-notify field: task-current-notify

      \ The notified bitmap
      dup constant .task-notified-bitmap field: task-notified-bitmap

      \ The current notification count
      dup constant .task-notify-count field: task-notify-count

      \ The current notification area pointer
      dup constant .task-notify-area field: task-notify-area

      \ Spinlock to claim
      dup constant .spinlock-to-claim field: spinlock-to-claim

      \ The current core of a task
      dup constant .task-core field: task-core

      \ The wake counter of a task
      dup constant .task-wake-after field: task-wake-after

      \ Routine to call upon scheduling if non-zero
      dup constant .task-force-call field: task-force-call

      \ Exception to send to task
      dup constant .task-raise field: task-raise

      \ Handler to invoke upon task termination
      dup constant .task-terminate-hook field: task-terminate-hook

      \ Data to pass to the task termination handler
      dup constant .task-terminate-data field: task-terminate-data

      \ Task termination immediate reason
      dup constant .task-terminate-immed-reason
      field: task-terminate-immed-reason
      
      \ Task termination reason
      dup constant .task-terminate-reason field: task-terminate-reason
      
    end-structure

    \ Pending operation structure
    begin-structure pending-op-size

      \ Pending operation to execute
      dup constant .pending-op-xt field: pending-op-xt

      \ Next pending operation
      dup constant .pending-op-next field: pending-op-next

      \ Pending operation priority
      dup constant .pending-op-priority field: pending-op-priority
      
    end-structure
    
  end-module> import

  \ Initialize on core boot hook
  variable core-init-hook
  
  \ The latest lock currently held by a tack
  user current-lock-held

  \ The default timeout
  user timeout

  \ Task being waited for
  user task-waited-for
  
  \ No timeout
  $80000000 constant no-timeout
  
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
    code[
    1 r0 movs_,#_
    r0 tos orrs_,_
    tos blx_
    ]code
    dict-base
    code[
    cortex-m7? [if]
      0 dp r0 ldr_,[_,#_]
      4 dp r1 ldr_,[_,#_]
      8 dp r2 ldr_,[_,#_]
      12 dp adds_,#_
    [else]
      r2 r1 r0 3 dp ldm
    [then]
    0 tos tos ldr_,[_,#_]
    tos r0 r0 subs_,_,_
    .task-dict-base r2 tos ldr_,[_,#_]
    r0 tos tos adds_,_,_
    ]code
\    execute dict-base @ - swap task-dict-base @ +
  ;

  \ Set a given user variable for a given task
  : for-task! ( x task xt -- )
    code[
    1 r0 movs_,#_
    r0 tos orrs_,_
    tos blx_
    ]code
    dict-base
    code[
    cortex-m7? [if]
      0 dp r0 ldr_,[_,#_]
      4 dp r1 ldr_,[_,#_]
      8 dp r2 ldr_,[_,#_]
      12 dp r3 ldr_,[_,#_]
      16 dp adds_,#_
    [else]
      r3 r2 r1 r0 4 dp ldm
    [then]
    0 tos tos ldr_,[_,#_]
    tos r0 r0 subs_,_,_
    .task-dict-base r2 tos ldr_,[_,#_]
    r0 tos r3 str_,[_,_]
    tos 1 dp ldm
    ]code
\    execute dict-base @ - swap task-dict-base @ + !
  ;

  \ Get a given user variable for a given task
  : for-task@ ( task xt -- x )
    code[
    1 r0 movs_,#_
    r0 tos orrs_,_
    tos blx_
    ]code
    dict-base
    code[
    cortex-m7? [if]
      0 dp r0 ldr_,[_,#_]
      4 dp r1 ldr_,[_,#_]
      8 dp r2 ldr_,[_,#_]
      12 dp adds_,#_
    [else]
      r2 r1 r0 3 dp ldm
    [then]
    0 tos tos ldr_,[_,#_]
    tos r0 r0 subs_,_,_
    .task-dict-base r2 tos ldr_,[_,#_]
    r0 tos tos ldr_,[_,_]
    ]code
\    execute dict-base @ - swap task-dict-base @ + @
  ;

  \ Pause a task without rescheduling it
  : pause-wo-reschedule ( -- )
    disable-int false reschedule? ! pause enable-int
  ;

  \ Pause a task so as to force it be rescheduled last
  : pause-reschedule-last ( -- )
    disable-int true reschedule-last? ! pause enable-int
  ;

  continue-module task-internal

    \ Get task dictionary end
    : task-dict-end ( task -- addr )
      dup main-task @ = if drop free-end @ then
    ;

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
      task-notify-count @ u< averts x-out-of-range-notify
    ;
    
    \ Find the next task with a higher priority; 0 returned indicates no task
    \ exists with a higher priority.
    : find-higher-priority ( task -- higher-task )
      code[
      .task-priority tos r0 ldrh_,[_,#_]
      16 r0 r0 lsls_,_,#_
      16 r0 r0 asrs_,_,#_
      r0 1 push
      .task-core tos tos ldr_,[_,#_]
      ]code
      cpu-last-task
      code[
      0 tos tos ldr_,[_,#_]
      mark>
      0 tos cmp_,#_
      ne bc>
      pc r0 2 pop
      >mark
      0 r0 ldr_,[sp,#_]
      .task-priority tos r1 ldrh_,[_,#_]
      16 r1 r1 lsls_,_,#_
      16 r1 r1 asrs_,_,#_
      r0 r1 cmp_,_
      lt bc>
      pc r0 2 pop
      >mark
      .task-next tos tos ldr_,[_,#_]
      b<
      ]code
    ;
      
    \   task-priority@ >r
    \   dup task-core @ cpu-last-task @ begin
    \     dup 0<> if
    \       dup task-priority@ r@ < if task-next @ false else true then
    \     else
    \       true
    \     then
    \   until
    \   rdrop
    \ ;

    \ Insert a task before another task
    : insert-task-before ( task after-task -- )
      code[
      .task-prev tos r0 ldr_,[_,#_]
      0 r0 cmp_,#_
      eq bc>
      0 dp r1 ldr_,[_,#_]
      .task-next r0 r1 str_,[_,#_]
      .task-prev r1 r0 str_,[_,#_]
      b>
      2swap >mark
      0 dp r1 ldr_,[_,#_]
      0 r2 movs_,#_
      .task-prev r1 r2 str_,[_,#_]
      tos 1 push
      .task-core r1 tos ldr_,[_,#_]
      ]code
      cpu-last-task
      code[
      0 dp r1 ldr_,[_,#_]
      0 tos r1 str_,[_,#_]
      tos 1 pop
      >mark
      .task-prev tos r1 str_,[_,#_]
      .task-next r1 tos str_,[_,#_]
      cortex-m7? [if]
        0 dp r0 ldr_,[_,#_]
        4 dp tos ldr_,[_,#_]
        8 dp adds_,#_
      [else]
        tos r0 2 dp ldm
      [then]
      ]code
    ;

    \   dup task-prev @ 0<> if
    \     2dup task-prev @ task-next !
    \     2dup task-prev @ swap task-prev !
    \   else
    \     0 2 pick task-prev !
    \     over dup task-core @ cpu-last-task !
    \   then
    \   2dup task-prev !
    \   swap task-next !
    \ ;

    \ Insert a task into first position
    : insert-task-first ( task -- )
      code[
      tos 1 push
      .task-core tos tos ldr_,[_,#_]
      tos 1 push
      ]code
      cpu-first-task
      code[
      0 tos tos ldr_,[_,#_]
      0 tos cmp_,#_
      eq bc>
      4 r0 ldr_,[sp,#_]
      .task-next tos r0 str_,[_,#_]
      .task-prev r0 tos str_,[_,#_]
      b>
      2swap >mark
      0 tos ldr_,[sp,#_]
      ]code
      cpu-last-task
      code[
      4 r0 ldr_,[sp,#_]
      0 tos r0 str_,[_,#_]
      >mark
      tos 1 pop
      ]code
      cpu-first-task
      code[
      r0 1 pop
      0 tos r0 str_,[_,#_]
      0 tos movs_,#_
      .task-next r0 tos str_,[_,#_]
      tos 1 dp ldm
      ]code
    ;
      
    \   dup task-core @ >r
    \   r@ cpu-first-task @ 0<> if
    \     dup r@ cpu-first-task @ task-next !
    \     r@ cpu-first-task @ over task-prev !
    \   else
    \     dup r@ cpu-last-task !
    \     0 over task-prev !
    \   then
    \   dup r> cpu-first-task !
    \   0 swap task-next !
    \ ;

    \ Insert a task
    : insert-task ( task -- )
      dup find-higher-priority ?dup if
	insert-task-before
      else
	insert-task-first
      then
    ;

    \ Insert a task last
    : insert-task-last ( task -- )
      last-task @ ?dup if
        insert-task-before
      else
        insert-task-first
      then
    ;

    \ Remove a task
    : remove-task ( task -- )
      [ 0 cpu-last-task ] literal
      [ 0 cpu-first-task ] literal
      code[
      r4 1 push
      tos r2 movs_,_
      cortex-m7? [if]
        0 dp r3 ldr_,[_,#_]
        4 dp tos ldr_,[_,#_]
        8 dp adds_,#_
      [else]
        tos r3 2 dp ldm
      [then]
      .task-prev tos r0 ldr_,[_,#_]
      .task-next tos r1 ldr_,[_,#_]
      0 r1 cmp_,#_
      eq bc>
      .task-prev r1 r0 str_,[_,#_]
      b>
      2swap >mark
      .task-core tos r4 ldr_,[_,#_]
      2 r4 r4 lsls_,_,#_
      r4 r2 r0 str_,[_,_]
      >mark
      0 r0 cmp_,#_
      eq bc>
      .task-next r0 r1 str_,[_,#_]
      b>
      2swap >mark
      .task-core tos r4 ldr_,[_,#_]
      2 r4 r4 lsls_,_,#_
      r4 r3 r1 str_,[_,_]
      >mark
      0 r0 movs_,#_
      .task-prev tos r0 str_,[_,#_]
      .task-next tos r0 str_,[_,#_]
      tos 1 dp ldm
      pc r4 2 pop
      ]code
    ;

    \   dup task-core @ >r
    \   dup task-next @ ?dup if
    \     over task-prev @ swap task-prev !
    \   else
    \     dup task-prev @ r@ cpu-first-task !
    \   then
    \   dup task-prev @ ?dup if
    \     over task-next @ swap task-next !
    \   else
    \     dup task-next @ r@ cpu-last-task !
    \   then
    \   0 over task-prev !
    \   0 swap task-next !
    \   rdrop
    \ ;

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
\      dup task-core @ start-task-change
      ['] validate-not-terminated try ?dup if
	( false in-task-change ! ) ?raise
      then
    ;

    \ Set a task's termination reason
    : task-terminate-reason! ( reason task -- )
      2dup task-terminate-immed-reason !
      dup task-terminate-reason @ terminated-crashed u< if
        dup task-terminate-reason @ terminated-killed =
        2 pick terminated-killed < and not if
          task-terminate-reason !
        else
          2drop
        then
      else
        2drop
      then
    ;
  
    \ Terminate a task for any reason
    : terminate ( reason task -- )
      [:
        dup task-active h@ terminated <> over current-task @ = or if
          tuck task-terminate-reason!
          terminated over task-active h!
          max-priority over task-priority h!
          dup current-task @ = if
            task-core @ release-other-core-spinlock
            end-critical
            begin pause again
          else
            drop
          then
        else
          2drop
        then
      ;] over task-core @ critical-with-other-core-spinlock
    ;

  end-module

  \ Get whether a task has timed out
  : timed-out? ( task -- timed-out )
    dup validate-not-terminated task-state h@ block-timed-out =
  ;

  \ Execute an xt with a timeout, restored to its previous value afterwards
  : with-timeout ( xt timeout -- )
    timeout @ { old-timeout }
    [: timeout ! execute ;] try
    old-timeout timeout !
    ?raise
  ;

  \ Execute an xt with a timeout after a start time in ticks
  : with-timeout-from-start ( xt timeout ticks-start -- )
    over no-timeout = if
      drop with-timeout
    else
      + systick-counter 2dup - 0> averts x-timed-out
      - 0 max with-timeout
    then
  ;

  \ Get whether a task has timed out and clear the timeout status
  : check-timeout ( task -- timed-out? )
    [:
      dup validate-not-terminated dup task-state h@ block-timed-out = if
        readied swap task-state h! true
      else
        drop false
      then
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Validate not timing out
  : validate-timeout ( task -- )
    check-timeout triggers x-timed-out
  ;

  \ Get task active state
  : task-active@ ( task -- active )
    task-active h@ 16 lshift 16 arshift
  ;

  \ Get saved task priority
  : task-saved-priority@ ( task -- priority )
    task-saved-priority h@ 16 lshift 16 arshift
  ;

  \ Get the core of a task
  : task-core@ ( task -- core ) task-core @ ;

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
      2dup task-timeslice @ - task-systick-counter @ +
      task-systick-counter !
    then
    swap 0 max swap task-timeslice !
    enable-int
  ;

  \ Get task timeslice
  : task-timeslice@ ( task -- timeslice )
    dup validate-not-terminated task-timeslice @
  ;

  \ Set task minimum timeslice
  : task-min-timeslice! ( min-timeslice task -- )
    dup validate-not-terminated task-min-timeslice !
  ;

  \ Get task minimum timeslice
  : task-min-timeslice@ ( task -- min-timeslice )
    dup validate-not-terminated task-min-timeslice @
  ;

  \ Get a task's name as a counted string; an address of zero indicates no name
  \ is set.
  : task-name@ ( task -- addr )
    dup validate-not-terminated task-name @
  ;

  \ Set a task's name as a counted string; an address of zero indicates to set
  \ no name.
  : task-name! ( addr task -- )
    dup validate-not-terminated task-name !
  ;

  \ Set a task's termination hook
  : task-terminate-hook! ( xt task -- )
    task-terminate-hook !
  ;

  \ Get a task's termination hook
  : task-terminate-hook@ ( task -- xt )
    task-terminate-hook @
  ;

  \ Set a task's termination hook data
  : task-terminate-data! ( x task -- )
    task-terminate-data !
  ;

  \ Get a task's termination data
  : task-terminate-data@ ( task -- x )
    task-terminate-data @
  ;

  \ Get a task's termination immediate reason
  : task-terminate-immed-reason@ ( task -- reason )
    task-terminate-immed-reason @
  ;

  \ Get a task's termination reason
  : task-terminate-reason@ ( task -- reason )
    task-terminate-reason @
  ;

  \ Start a task's execution
  : run ( task -- )
    [:
      dup start-validate-task-change
      dup task-active@ 1+
      dup 1 = if over test-remove-task over insert-task then
      swap task-active h!
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Stop a task's execution
  : stop ( task -- )
    [:
      dup start-validate-task-change
      dup task-active@ 1-
      swap task-active h!
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Kill a task
  : kill ( task -- )
    terminated-killed swap terminate
  ;

  \ Get the last delay time
  : last-delay ( task -- ticks-delay ticks-start )
    [:
      dup validate-not-terminated
      dup task-systick-delay @
      swap task-systick-start @
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Delay a task
  : delay ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      tuck task-systick-start !
      tuck task-systick-delay !
      delayed over task-state h!
    ;] over task-core @ critical-with-other-core-spinlock
    current-task @ = if pause-wo-reschedule then
  ;

  \ Mark a task as blocked until a timeout
  : block-timeout ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      -1 over task-ready-count +!
      dup task-ready-count @ 0< if
	tuck task-systick-start !
	tuck task-systick-delay !
	blocked-timeout over task-state h!
      else
	nip nip
      then
    ;] over task-core @ critical-with-other-core-spinlock
    current-task @ = if pause-wo-reschedule then
  ;

  \ Wait until a timeout on the specified notification index and return the
  \ value for that notification index once notified, unless already notified,
  \ where then that value will be returned and the notified state will be
  \ cleared immediately. x-timed-out is raised if the timeout is reached.
  : wait-notify-timeout ( ticks-delay ticks-start notify-index -- x )
    [:
      dup current-task @ validate-notify
      swap current-task @ task-systick-start !
      swap current-task @ task-systick-delay !
      begin dup bit current-task @ task-notified-bitmap bit@ not while
	dup current-task @ task-current-notify !
        blocked-timeout current-task @ task-state h!
	cpu-index end-critical-with-other-core-spinlock pause-wo-reschedule
        cpu-index begin-critical-with-other-core-spinlock
        current-task @ check-timeout if
          -1 current-task @ task-current-notify !
          ['] x-timed-out ?raise
        then
      repeat
      current-task @ task-notify-area @ over cells + @
      swap bit current-task @ task-notified-bitmap bic!
      -1 current-task @ task-current-notify !
    ;] cpu-index critical-with-other-core-spinlock
  ;

  \ Mark a task as waiting
  : block-wait ( wake-count task -- )
    dup validate-not-terminated
    tuck task-wake-after !
    blocked-wait over task-state h!
    current-task @ = if pause-wo-reschedule then
  ;
  
  \ Mark a task as blocked indefinitely
  : block-indefinite ( task -- )
    [:
      dup validate-not-terminated
      -1 over task-ready-count +!
      dup task-ready-count @ 0< if
	blocked-indefinite over task-state h!
      then
    ;] over task-core @ critical-with-other-core-spinlock
    current-task @ = if pause-wo-reschedule then
  ;

  continue-module task-internal

    \ Release the same core spinlock, release a spinlock, block indefinitely,
    \ and then claim the same core spinlock, and claim a spinlock.
    : block-indefinite-self-release ( spinlock -- )
      disable-int
      claim-all-core-spinlock-raw
      task-waited-for @ if
        -1 current-task @ task-ready-count !
        blocked-indefinite current-task @ task-state h!
        false reschedule? ! pause
      then
      release-all-core-spinlock-raw
      enable-int
    ;

  end-module

  \ Check for timing out for non-blocked tasks
  : compare-timeout ( task -- timed-out? )
    dup ['] timeout for-task@ no-timeout <> if
      systick-counter over timeout-systick-start @ -
      swap timeout-systick-delay @ >=
    else
      drop false
    then
  ;

  \ Wait indefinitely on the specified notification index and return the value
  \ for that notification index once notified, unless already notified, where
  \ then that value will be returned and the notified state will be cleared
  \ immediately.
  : wait-notify-indefinite ( notify-index -- x )
    [:
      dup current-task @ validate-notify
      begin dup bit current-task @ task-notified-bitmap bit@ not while
        dup current-task @ task-current-notify !
        blocked-indefinite current-task @ task-state h!
	cpu-index end-critical-with-other-core-spinlock pause-wo-reschedule
        cpu-index begin-critical-with-other-core-spinlock
      repeat
      current-task @ task-notify-area @ over cells + @
      swap bit current-task @ task-notified-bitmap bic!
      -1 current-task @ task-current-notify !
    ;] cpu-index critical-with-other-core-spinlock
  ;

  continue-module task-internal

    \ Core of readying a task
    : do-ready ( task -- )
      dup task-current-notify @ -1 = if
        0 over task-ready-count !
\        dup task-ready-count @ 1+ 0 min dup 2 pick task-ready-count !
\	0>= if
	  readied swap task-state h!
\	else
\	  drop
\	then
      else
	drop
      then
    ;

  end-module
      
  \ Ready a task
  : ready ( task -- )
    [:
      dup validate-not-terminated do-ready
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Notify a task for a specified notification index
  : notify ( notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      2dup swap bit swap task-notified-bitmap bis!
      dup task-current-notify @ rot = if
	readied swap task-state h! true
      else
	drop false
      then
    ;] over task-core @ critical-with-other-core-spinlock
    if pause-wo-reschedule then
  ;

  \ Notify a task for a specified notification index, setting the notification
  \ value to a set value
  : notify-set ( x notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      dup task-notify-area @ 2 pick cells + >r rot r> !
      2dup swap bit swap task-notified-bitmap bis!
      dup task-current-notify @ rot = if
	readied swap task-state h! true
      else
	drop false
      then
    ;] over task-core @ critical-with-other-core-spinlock
    if pause-wo-reschedule then
  ;

  \ Notify a task for a specified notification index, updating the notification
  \ value with an xt with the signature ( x0 -- x1 )
  : notify-update ( xt notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      rot >r ( index task )
      dup task-notify-area @ 2 pick cells + ( index task addr )
      r> rot >r rot >r swap dup >r @ swap execute r> ! ( )
      r> r> 2dup swap bit swap task-notified-bitmap bis!
      dup task-current-notify @ rot = if
        readied swap task-state h! true
      else
	drop false
      then
    ;] over task-core @ critical-with-other-core-spinlock
    if pause-wo-reschedule then
  ;

  \ Clear a notification for a task
  : clear-notify ( notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      swap bit swap task-notified-bitmap bic!
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Get the contents of a notification mailbox without waiting on it
  : mailbox@ ( notify-index task -- x )
    [:
      dup validate-not-terminated
      2dup validate-notify
      task-notify-area @ swap cells + @
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Set the contents of a notification mailbox without notifying a task
  : mailbox! ( x notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      task-notify-area @ swap cells + !
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Block a task for the specified initialized timeout
  : block ( task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task@ no-timeout <> if
	dup timeout-systick-delay @
	over timeout-systick-start @
	rot block-timeout
      else
	block-indefinite
      then
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Wait for a notification at a specified notification index with a specified
  \ initialized timeout and return the notification value
  : wait-notify ( notify-index -- x )
    [:
      timeout @ no-timeout <> if
        current-task @ timeout-systick-delay @
        current-task @ timeout-systick-start @ rot
        true
      else
        false
      then
    ;] cpu-index critical-with-other-core-spinlock
    if wait-notify-timeout else wait-notify-indefinite then
  ;

  \ Prepare blocking for a task
  : prepare-block ( task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task@ no-timeout <> if
	systick-counter over timeout-systick-start !
	dup ['] timeout for-task@ 0 max swap timeout-systick-delay !
      else
	drop
      then
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Get whether a task has terminated
  : terminated? ( task -- terminated ) task-active h@ terminated = ;

  continue-module task-internal
  
    \ Ready tasks waiting for current task
    \ argument
    : wake-other-tasks ( -- )
      cpu-count 0 do
        i cpu-first-task @ begin ?dup while
          dup ['] task-waited-for for-task@ current-task @ = if
            0 over ['] task-waited-for for-task!
            dup task-state h@ blocked-indefinite = if
              dup do-ready
            then
          then
	  task-prev @
	repeat
      loop
    ;
    
    \ Block the current task with spinlocks already held
    : block-raw ( task -- )
      dup validate-not-terminated
      dup ['] timeout for-task@ no-timeout <> if
	dup timeout-systick-delay @
	over timeout-systick-start @
	rot -1 over task-ready-count +!
	dup task-ready-count @ 0< if
	  tuck task-systick-start !
	  tuck task-systick-delay !
	  blocked-timeout over task-state h!
	else
	  nip nip
	then
      else
	-1 over task-ready-count +!
	dup task-ready-count @ 0< if
	  blocked-indefinite over task-state h!
	then
      then
      current-task @ = if false reschedule? ! pause then
    ;

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
      0 over task-ready-count !
      0 task-waited-for !
      0 over timeout-systick-start !
      0 over timeout-systick-delay !
      0 over task-systick-start !
      -1 over task-systick-delay !
      wake-counter @ over task-wake-after !
      0 over task-force-call !
      0 over task-raise !
      0 over task-terminate-hook !
      0 over task-terminate-data !
      not-terminated over task-terminate-immed-reason !
      not-terminated over task-terminate-reason !
      cpu-index over task-core !
      c" main" over task-name !
      -1 over task-current-notify !
      0 over task-notified-bitmap !
      0 over task-notify-count !
      0 over task-notify-area !
      default-timeslice over task-timeslice !
      default-min-timeslice over task-min-timeslice !
      default-timeslice over task-saved-systick-counter !
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
      try-and-display-error
      dup 0= if drop terminated-normally then current-task @ terminate
    ;

    \ Get whether a task is finished with a delay or timeout
    : delayed? ( task -- )
      systick-counter over task-systick-start @ -
      swap task-systick-delay @ <
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
	over task-core !
	dup task-rstack-size h@ over task-stack-size h@ + over + task-size +
	over ['] task-rstack-base for-task!
	dup task-stack-size h@ over + task-size +
	over ['] task-rstack-end for-task!
	dup ['] task-rstack-end for-task@ over ['] task-stack-base for-task!
	dup task-size + over ['] task-stack-end for-task!
	0 over task-ready-count !
	0 over ['] task-handler for-task!
	0 over ['] task-waited-for for-task!
	0 over task-priority h!
	0 over task-saved-priority h!
	0 over task-active h!
	base @ over ['] task-base for-task!
	0 over ['] current-lock-held for-task!
	readied over task-state h!
	no-timeout over ['] timeout for-task!
	0 over timeout-systick-start !
	0 over timeout-systick-delay !
	0 over task-systick-start !
	0 over task-name !
	-1 over task-current-notify !
	0 over task-notified-bitmap !
	0 over task-notify-count !
	0 over task-notify-area !
        -1 over task-systick-delay !
        wake-counter @ over task-wake-after !
        0 over task-force-call !
        0 over task-raise !
        0 over task-terminate-hook !
        0 over task-terminate-data !
        not-terminated over task-terminate-immed-reason !
        not-terminated over task-terminate-reason !
	c" aux-main" over task-name !
	default-timeslice over task-timeslice !
	default-min-timeslice over task-min-timeslice !
	default-timeslice over task-saved-systick-counter !
	dup ['] task-rstack-base for-task@ over task-rstack-current !
	next-user-space over task-dict-base @ +
	over ['] task-ram-here for-task!
	0 over task-next !
	0 over task-prev !
	dup >r init-aux-task-stack
	r> ['] task-rstack-base for-task@
      ;

    [then]

    \ Initialize a context
    : init-context ( ctx dp xt -- ctx )
      rot
      dup 7 and if
        2 cells - $01000200 over ! \ XPSR, alignment added
      else
        cell - $01000000 over ! \ XPSR, no alignment added
      then
      cell - swap over ! \ Return address
      cell - $FFFFFFFE over ! \ LR
      cell - $DEADBEEF over ! \ R12
      cell - $DEADBEEF over ! \ R3
      cell - $DEADBEEF over ! \ R2
      cell - $DEADBEEF over ! \ R1
      cell - $DEADBEEF over ! \ R0
      cell - $DEADBEEF over ! \ R10
      cell - $DEADBEEF over ! \ R9
      cell - $DEADBEEF over ! \ R8
      cell - swap over ! \ R7
      cell - $FEDCBA98 over ! \ R6
      cell - $DEADBEEF over ! \ R5
      cell - $DEADBEEF over ! \ R4
    ;

  end-module

  \ Block a task, setting an address to a value
  : block-set ( value addr task -- )
    [:
      dup validate-not-terminated
      dup ['] timeout for-task@ no-timeout <> if
        dup timeout-systick-delay @
        over timeout-systick-start @
        rot -1 over task-ready-count +!
        dup task-ready-count @ 0< if
          tuck task-systick-start !
          tuck task-systick-delay !
          blocked-timeout swap task-state h!
        else
          2drop drop
        then
      else
        -1 over task-ready-count +!
        dup task-ready-count @ 0< if
          blocked-indefinite swap task-state h!
        else
          drop
        then
      then
      ! false reschedule? ! pause
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Initialize a task
  : init-task ( xn...x0 count xt task core -- )
    swap dup dup task-dict-size @ - over task-dict-base ! swap
    over task-core !
    dup task-rstack-size h@ over task-stack-size h@ + over + task-size +
    over ['] task-rstack-base for-task!
    dup task-stack-size h@ over + task-size +
    over ['] task-rstack-end for-task!
    dup ['] task-rstack-end for-task@ over ['] task-stack-base for-task!
    dup task-size + over ['] task-stack-end for-task!
    0 over task-ready-count !
    0 over ['] task-handler for-task!
    0 over ['] task-waited-for for-task!
    0 over task-priority h!
    0 over task-saved-priority h!
    0 over task-active h!
    base @ over ['] task-base for-task!
    key-hook @ over ['] task-key-hook for-task!
    key?-hook @ over ['] task-key?-hook for-task!
    emit-hook @ over ['] task-emit-hook for-task!
    emit?-hook @ over ['] task-emit?-hook for-task!
    error-emit-hook @ over ['] error-emit-hook for-task!
    error-emit?-hook @ over ['] error-emit?-hook for-task!
    flush-console-hook @ over ['] flush-console-hook for-task!
    error-flush-console-hook @ over ['] error-flush-console-hook for-task!
    0 over ['] current-lock-held for-task!
    readied over task-state h!
    no-timeout over ['] timeout for-task!
    0 over timeout-systick-start !
    0 over timeout-systick-delay !
    0 over task-systick-start !
    0 over task-name !
    -1 over task-current-notify !
    0 over task-notified-bitmap !
    0 over task-notify-count !
    0 over task-notify-area !
    -1 over task-systick-delay !
    wake-counter @ over task-wake-after !
    0 over task-force-call !
    0 over task-raise !
    0 over task-terminate-hook !
    0 over task-terminate-data !
    not-terminated over task-terminate-immed-reason !
    not-terminated over task-terminate-reason !
    default-timeslice over task-timeslice !
    default-min-timeslice over task-min-timeslice !
    default-timeslice over task-saved-systick-counter !
    dup ['] task-rstack-base for-task@
    over ['] task-stack-base for-task@ ['] task-entry
    init-context over task-rstack-current !
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
    [:
      2dup 4 align swap 4 align + task-size +
      free-end @ swap -
      swap 4 align swap tuck task-rstack-size h!
      swap 4 align swap tuck task-stack-size h!
      swap 4 align swap tuck task-dict-size !
      dup dup task-dict-size @ - free-end !
    ;] critical-with-all-core-spinlock
  ;

  \ Configure notification for a task; notify-count may be from 0 to 32, and
  \ notify-area-addr is the address of an area of memory that contains that
  \ number of cells.
  : config-notify ( notify-area-addr notify-count task -- )
    [:
      over 32 u<= averts x-out-of-range-notify
      2dup task-current-notify @ > averts x-current-wait-notify
      2dup swap $FFFFFFFF 32 rot - rshift
      over task-notified-bitmap @
      and swap task-notified-bitmap !
      2dup task-notify-count !
      2 pick rot cells 0 fill
      task-notify-area !
    ;] critical
  ;

  \ Spawn a non-main task
  : spawn ( xn...x0 count xt dict-size stack-size rstack-size -- task )
    task-allot dup >r cpu-index init-task r>
  ;

  continue-module task-internal

    \ Get whether a task is waiting
    : waiting-task? ( task -- waiting? )
      code[ \ ( task tos: task-state-mask )
      .task-state tos r0 ldrh_,[_,#_]
      ( tos: task r0: *task-state )
      readied r0 cmp_,#_ ( tos: task r0: *task-state )
      ne bc>
      0 tos movs_,#_
      pc 1 pop
      >mark
      blocked-wait tos cmp_,#_ ( tos: task r0: *task-state )
      ne bc>
      ]code
      wake-counter
      code[ ( task tos: wake-counter )
      0 tos r1 ldr_,[_,#_] ( task r1: *wake-counter tos: wake-counter )
      0 tos movs_,#_ ( task r1: *wake-counter tos: 0 )
      r0 1 dp ldm ( r1: *wake-counter r0: task tos: 0 )
      .task-wake-after r0 r0 ldr_,[_,#_]
      ( r1: *wake-counter r0: *task-wake-after tos: 0 )
      r0 r1 cmp_,_
      ne bc>
      tos tos mvns_,_
      >mark
      pc 1 pop
      >mark ( tos: task r0: *task-state )
      blocked-indefinite r0 cmp_,#_
      ne bc>
      0 tos movs_,#_
      tos tos mvns_,_
      pc 1 pop
      >mark
      delayed r0 cmp_,#_ ( tos: task r0: *task-state )
      eq bc>
      blocked-timeout r0 cmp_,#_ ( tos: task r0: *task-state )
      eq bc>
      0 tos movs_,#_
      pc 1 pop
      >mark
      >mark
      ]code
      delayed?
    ;

    \ Find next task
    : find-next-task ( -- task )
      first-task
      code[
      0 tos tos ldr_,[_,#_]
      mark>
      0 tos cmp_,#_
      ne bc>
      pc 1 pop
      >mark
      tos 1 push
      ]code
      waiting-task?
      code[
      0 tos cmp_,#_
      ne bc>
      pc tos 2 pop
      >mark
      tos 1 pop
      .task-prev tos tos ldr_,[_,#_]
      b<
      ]code
    ;
    
    \ PendSV return value
    variable pendsv-return

    \ Save task state
    : save-task-state ( task -- )
      task-systick-counter @ swap task-saved-systick-counter !
    ;

    \ Restore task state
    : restore-task-state ( task -- )
      task-dict-base @ dict-base !
    ;

    \ Handle task termination
    : handle-task-terminated ( task -- )
      >r r@ task-terminate-hook @ ?dup if
        claim-same-core-spinlock
        r@ task-terminate-reason @ swap
        r@ task-name @ swap
        r@ task-terminate-data @ swap r@ task-terminate-immed-reason @ 2 rot
        r@ cpu-index init-task
        r@ task-name !
        r@ task-terminate-reason !
        max-priority r@ task-priority h!
        1 r@ task-active h!
        r@ insert-task
        release-same-core-spinlock
      then
      rdrop
    ;

    \ Body of extra task
    : do-extra-task ( -- )
      begin wake-counter 1+ current-task @ block-wait again
    ;

    \ Initialize the extra task
    : init-extra-task ( -- )
      extra-task @ if
        0 ['] do-extra-task extra-task @ cpu-index init-task
      else
        0 ['] do-extra-task 320 128 512 spawn extra-task !
      then
      claim-same-core-spinlock
      first-task @ 0= if
        c" extra" extra-task @ task-name !
        -1 extra-task @ task-priority h!
        1 extra-task @ task-active h!
        extra-task @ insert-task
      then
      release-same-core-spinlock
    ;

    \ Reschedule previous task
    : reschedule-task ( task -- )
      true in-task-change !
      reschedule-last? @ if
        claim-same-core-spinlock
        dup remove-task
        dup task-active@ 0> if
          insert-task-last
        else
          dup terminated? if
            terminated-task !
          else
            drop
          then
        then
        release-same-core-spinlock
      else
        reschedule? @ task-systick-counter @ 0<= or if
          claim-same-core-spinlock
          dup remove-task
          dup task-active@ 0> if
            insert-task
          else
            dup terminated? if
              terminated-task !
            else
              drop
            then
          then
          release-same-core-spinlock
        else
          drop
        then
      then
      false reschedule-last? !
      true reschedule? !
      false in-task-change !
    ;

    \ Handle pending operations
    : handle-pending-ops ( -- )
      first-pending-op @
      code[
      r6 r0 movs_,_
      r6 1 r7 ldm
      mark>
      0 r0 cmp_,#_
      ne bc>
      pc 1 pop
      >mark
      .pending-op-xt r0 r1 ldr_,[_,#_]
      0 r2 movs_,#_
      .pending-op-xt r0 r2 str_,[_,#_]
      .pending-op-next r0 r0 ldr_,[_,#_]
      0 r1 cmp_,#_
      2dup eq bc<
      1 r1 adds_,#_
      r0 1 push
      r1 blx_
      r0 1 pop
      b<
      ]code
    ;
    
    \ Handle task-switching
    : switch-tasks ( -- )
      r> pendsv-return !

      in-critical @ 0= in-task-change @ 0= and if

        handle-pending-ops

        do-pause? @ if
          
          1 pause-count +!

          current-task @ dup prev-task !
          ?dup if dup save-task-state reschedule-task then

          begin
            true in-task-change !
            begin
              first-task @ 0= if init-extra-task then
              claim-same-core-spinlock
              find-next-task
              dup 0<> if
                dup task-ready-count @ 0 max over task-ready-count !
              then
              release-same-core-spinlock
              dup 0<> if
                dup task-active@ 1 < if
                  claim-same-core-spinlock
                  remove-task
                  release-same-core-spinlock
                  false
                else
                  dup task-state h@
                  dup blocked-timeout = swap block-timed-out = or if
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
            0<> if true else handle-pending-ops sleep false then
          until

          prev-task @ current-task @ <> if
            disable-int
            current-task @ task-rstack-current @ context-switch
            prev-task @ if prev-task @ task-rstack-current ! else drop then
            current-task @ restore-task-state
            enable-int
          then

          current-task @ task-saved-systick-counter @ 0<= if
            current-task @ task-saved-systick-counter @
            current-task @ task-timeslice @ +
            current-task @ task-min-timeslice @ max
            task-systick-counter !
          else
            current-task @ task-saved-systick-counter @ task-systick-counter !
          then
          
          terminated-task @ if
            terminated-task @ handle-task-terminated
            0 terminated-task !
          then

          false do-pause? !
          
        then

      else
	true deferred-context-switch !
      then

      pendsv-return @ >r

      current-task @ task-force-call @ ?dup if
        claim-same-core-spinlock
        code[
        $18 4 + tos str_,[sp,#_]
        tos 1 dp ldm
        ]code
        0 current-task @ task-force-call !
        release-same-core-spinlock
      then
      
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
    : do-pause ( -- )
      true do-pause? ! true in-multitasker? ! ICSR_PENDSVSET! dmb dsb isb
    ;

    \ Task info structure
    begin-structure task-info-size

      field: task-info-task
      hfield: task-info-running
      hfield: task-info-state
      field: task-info-current-notify
      field: task-info-ticks-delay
      field: task-info-ticks-start
      field: task-info-priority
      field: task-info-name-len
      28 +field task-info-name-bytes
      
    end-structure

    \ Get the task count
    : get-cpu-task-count ( cpu -- count )
      0 swap cpu-first-task @ begin ?dup while
        swap 1+ swap task-prev @
      repeat
    ;

    \ Copy a task name into a task info structure
    : copy-task-name-for-info { info task -- }
      task task-name@ ?dup if
        count dup info task-info-name-len !
        info task-info-name-bytes swap 24 min move
      else
        0 info task-info-name-len !
      then
    ;

    \ Copy a task delay info a task info structure
    : copy-task-delay-for-info { info task -- }
      task task-state h@ case
	delayed of true endof
	blocked-timeout of true endof
	false swap
      endcase
      if
        task last-delay
      else
        0 0
      then
      info task-info-ticks-start !
      info task-info-ticks-delay !
    ;

    \ Copy task info into a task info structure
    : copy-task-info { info task -- }
      task info task-info-task !
      task current-task @ = info task-info-running h!
      task task-state h@ info task-info-state h!
      task task-current-notify @ info task-info-current-notify !
      task task-priority@ info task-info-priority !
      info task copy-task-name-for-info
      info task copy-task-delay-for-info
    ;
    
    \ Dump task name
    : dump-task-name { info -- }
      info task-info-name-len @ ?dup if
        info task-info-name-bytes over type 25 swap - 0 max spaces
      else
        25 spaces
      then
    ;
    
    \ Dump task state
    : dump-task-state { info -- }
      info task-info-running h@ if
	s" running"
      else
	info task-info-current-notify @ -1 <> if
	  s" to-notify"
	else
	  info task-info-state h@ case
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
    ;
    
    \ Dump task priority
    : dump-task-priority { info -- }
      info task-info-priority @ here swap format-integer
      dup 8 swap - spaces type
    ;
    
    \ Dump task until time
    : dump-task-until { info -- }
      info task-info-state h@ case
	delayed of true endof
	blocked-timeout of true endof
	false swap
      endcase
      if
        info task-info-ticks-delay @
        info task-info-ticks-start @
        + dup here swap format-unsigned
	dup 10 swap - spaces type space
	systick-counter - here swap format-integer dup 11 swap - spaces type
      else
	22 spaces
      then
    ;
    
    \ Dump task ehader
    : dump-task-header ( -- )
      cr ." task     name                      priority state      "
      ." until       delay"
    ;

  end-module
  
  \ Dump task information for all tasks
  : dump-tasks ( -- )
    dump-task-header
    cpu-count 0 ?do
      cpu-count 1 > if
        cr ." cpu " i (.) ." :"
      then
      i [:
        dup get-cpu-task-count dup task-info-size * [: { cpu count info }
          info { current-info }
          cpu cpu-first-task @ begin ?dup while
            current-info over copy-task-info
            task-prev @
            task-info-size +to current-info
          repeat
          count info cpu [: { count info }
            count 0 ?do
              cr info task-info-task @ h.8
              space info dump-task-name
              space info dump-task-priority
              space info dump-task-state
              space info dump-task-until
              task-info-size +to info
            loop
          ;] swap outside-critical-with-other-core-spinlock
        ;] with-aligned-allot
      ;] i critical-with-other-core-spinlock
    loop
  ;

  \ Wait for n milliseconds with multitasking support
  : ms1 ( u -- ) systick-divisor * systick-counter current-task @ delay ;

  continue-module task-internal

    \ Wait the current thread
    : do-wait ( wait-count -- )
      pause-enabled @ 0> if
        current-task @ block-wait
      else
        drop
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
        [:
          display-red
          ." RAM dictionary space is running low (<1K left)" cr
          display-normal
        ;] with-error-console
      then
      saved-validate-dict @ ?execute
    ;

    cpu-count 1 > [if]
      
      \ Initialize an auxiliary core's main task
      : init-aux-main-task ( -- )
	0 fifo-drain
	wait-current-core
        NVIC_ICPR_CLRPEND_All!
        current-task @ task-dict-base @ dict-base !
	$7F SHPR3_PRI_15!
	$FF SHPR2_PRI_11!
	$FF SHPR3_PRI_14!
	$00 SIO_IRQ_PROC1 NVIC_IPR_IP!
	SIO_IRQ_PROC1 NVIC_ISER_SETENA!
	1 pause-enabled !
        core-init-hook @ execute
        true core-1-launched !
        pause try-and-display-error
        dup 0= if drop terminated-normally then current-task @ terminate
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
        [:
          dup cpu-active? @ if
            ['] x-main-already-launched ?raise
          else
            true over cpu-active? !
          then
          >r task-allot >r
          2r@ cpu-current-task !
          2r@ cpu-main-task !
          2r@ cpu-prev-task !
          2r@ cpu-first-task !
          2r@ cpu-last-task !
          2r@ init-aux-task
          ['] init-aux-main-task -rot
          2r> swap >r disable-int launch-aux-core r>
          begin core-1-launched @ until
          enable-int
        ;] critical
      [else]
	['] x-core-out-of-range ?raise
      [then]
    ;

  end-module

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
  : main-task ( -- task ) 0 cpu-main-task @ ;
  
  \ Call block if not in multitasker, and return whether in multitasker
  : exclude-multitasker ( xt -- flag )
    disable-int in-multitasker? @ dup not if
      >r execute r>
    else
      nip
    then
    enable-int
  ;

  \ Force a task to call a routine
  : force-call ( xt task -- ) task-force-call ! ;

  \ Signal a task to raise an exception
  : signal ( xt task -- )
    over 0<> if
      disable-int
      tuck task-raise !
      [: current-task task-raise @ 0 current-task task-raise ! ?raise ;]
      over force-call
      readied swap task-state h!
      enable-int
    else
      2drop
    then
  ;

  \ Saved attention hook
  variable saved-attention-hook

  \ Main interrupt exception
  : x-interrupt-main ." *** INTERRUPT MAIN ***" cr ;
  
  \ Handle attention
  : do-attention ( c -- )
    dup [char] z = if
      drop false attention? ! ['] x-interrupt-main main-task signal
    else
      saved-attention-hook @ execute
    then
  ;
  
  \ Initialize multitasking
  : init-tasker ( -- )
    disable-int
    NVIC_ICPR_CLRPEND_All!
    0 pause-enabled !
    $7F SHPR3_PRI_15!
    $00 SHPR2_PRI_11!
    $FF SHPR3_PRI_14!
    stack-end @ free-end !
    validate-dict-hook @ saved-validate-dict !
    attention-hook @ saved-attention-hook !
    ['] do-attention attention-hook !
    false ram-dict-warned !
    ['] do-pause pause-hook !
    ['] do-wait wait-hook !
    ['] do-validate-dict validate-dict-hook !
    [: ( int-io::enable-int-io ) init-systick-aux-core ;] core-init-hook !
    cpu-count 0 ?do
      false i cpu-in-multitasker? !
      false i cpu-sleep-enabled? !
      false i cpu-in-task-change !
      false i cpu-do-pause? !
      true i cpu-reschedule? !
      false i cpu-reschedule-last? !
      0 i cpu-task-systick-counter !
      0 i cpu-terminated-task !
      0 i cpu-extra-task !
      0 i cpu-first-pending-op !
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
    [: cpu-index 0= current-task main-task = and ;] in-main?-hook !
    [: terminated-crashed current-task terminate ;] crash-hook !
    [: 0 cpu-main-task @ task-dict-current ;] main-here-hook !
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
    cr 0 cpu-main-task @
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

  \ Force pending operations to execute
  : force-pending-ops ( -- )
    true in-multitasker? ! ICSR_PENDSVSET! dmb dsb isb
  ;

  \ Register pending operation on the current CPU
  : register-pending-op ( priority pending-op -- )
    [: { priority pending-op }
      priority pending-op pending-op-priority !
      0 pending-op pending-op-xt !
      first-pending-op @ if
        priority first-pending-op @ pending-op-priority @ > if
          first-pending-op @ pending-op pending-op-next !
          pending-op first-pending-op !
        else
          first-pending-op @ begin
            dup pending-op-next @ ?dup if
              priority over pending-op-priority @ > if
                pending-op pending-op-next !
                pending-op swap pending-op-next !
                true
              else
                nip false
              then
            else
              0 pending-op pending-op-next !
              pending-op swap pending-op-next !
              true
            then
          until
        then
      else
        pending-op first-pending-op !
        0 pending-op pending-op-next !
      then
    ;] critical
  ;

  \ Set a pending operation
  : set-pending-op ( xt pending-op -- ) pending-op-xt ! ;
  
  \ Export pending operation size
  ' pending-op-size export pending-op-size
  
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

  \ Allot memory from the end of RAM
  : allot-end ( u -- addr )
    [:
      negate free-end +! free-end @
    ;] critical-with-all-core-spinlock
  ;

end-module

\ Reboot to initialize multitasking
reboot