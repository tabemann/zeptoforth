\ Copyright (c) 2020-2026 Travis Bemann
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
  serial import
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

    \ Task guard value
    $DEADCAFE constant task-guard-value
    
    \ Waiting for a task to be ready
    cpu-variable cpu-waiting-for-task? waiting-for-task?
    
    \ In task change
    cpu-variable cpu-in-task-change in-task-change

    \ Main task
    cpu-variable cpu-main-task main-task
    
    \ Current task
    cpu-variable cpu-current-task current-task

    \ Previous active task
    cpu-variable cpu-prev-task prev-task

    \ First active task
    cpu-variable cpu-first-active-task first-active-task
    
    \ Last task
    cpu-variable cpu-last-active-task last-active-task

    \ First delayed task
    cpu-variable cpu-first-delayed-task first-delayed-task

    \ Last delayed task
    cpu-variable cpu-last-delayed-task last-delayed-task

    \ First blocked task
    cpu-variable cpu-first-blocked-task first-blocked-task

    \ Last blocked task
    cpu-variable cpu-last-blocked-task last-blocked-task

    \ Next delayed task tick
    cpu-variable cpu-next-delayed-tick next-delayed-tick

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

    \ Total timeslice
    cpu-variable cpu-total-timeslice total-timeslice

    \ Deadline limiting systick
    cpu-variable cpu-limit-task-deadlines-systick limit-task-deadlines-systick

    \ Task deadline limit constant (one hour)
    1000 systick-divisor * 60 * 60 * constant task-deadline-limit

    \ Task deadline limit interval (one minute)
    1000 systick-divisor * 60 * constant limit-task-deadlines-interval
    
    \ SVCall vector index
    11 constant svcall-vector

    \ PendSV vector index
    14 constant pendsv-vector
    
    \ Terminated task
    $8000 constant terminated
    
    \ The task structure
    begin-structure task-size
      \ Task start guard value
      dup constant .task-start-guard field: task-start-guard

      \ Return stack size
      dup constant .task-rstack-size hfield: task-rstack-size
      
      \ Data stack size
      dup constant .task-stack-size hfield: task-stack-size

      \ Dictionary size
      dup constant .task-dict-size field: task-dict-size

      \ Current return stack address
      dup constant .task-rstack-current field: task-rstack-current

      \ Saved data stack address
      dup constant .task-saved-stack-current field: task-saved-stack-current

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

      \ A task's interval (0< means default)
      dup constant .task-interval field: task-interval

      \ A task's deadline
      dup constant .task-deadline field: task-deadline

      \ Has a task's deadline been set manually
      dup constant .task-deadline-set? field: task-deadline-set?

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

      \ Task floating point context saved?
      dup constant .task-float32-ctx-saved? field: task-float32-ctx-saved?
      
      \ Task floating point context
      dup constant .task-float32-ctx 4 cells +field task-float32-ctx
      
      \ Task end guard value
      dup constant .task-end-guard field: task-end-guard
      
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

  \ Watchdog hook
  variable watchdog-hook

  \ Task initialization hook
  variable task-init-hook
  
  \ The latest lock currently held by a tack
  user current-lock-held

  \ The default timeout
  user timeout

  \ Task being waited for
  user task-waited-for
  
  \ No timeout
  $80000000 constant no-timeout
  
  \ Task has not terminated
  0 constant not-terminated

  \ Task has terminated normally
  1 constant terminated-normally

  \ Task has been killed
  2 constant terminated-killed

  \ Task has terminated due to a hardware exception
  3 constant terminated-crashed

  \ Task has terminated due to a stack overflow
  4 constant terminated-overflowed

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
    cortex-m7? cortex-m33? or [if]
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
    cortex-m7? cortex-m33? or [if]
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
    cortex-m7? cortex-m33? or [if]
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

  \ Get task active state
  : task-active@ ( task -- active )
    task-active h@ 16 lshift 16 arshift
  ;

  continue-module task-internal

    \ Get task dictionary end
    : task-dict-end ( task -- addr )
      dup main-task @ = if drop free-end @ then
    ;

    \ Get task current stack address
    : task-stack-current ( task -- addr )
      dup current-task @ = if drop sp@ else task-rstack-current @ 12 + @ then
    ;

    \ Get task current dictionary address
    : task-dict-current ( task -- addr )
      ['] task-ram-here for-task@
    ;

    \ Set task current stack address
    : task-stack-current! ( addr task -- )
      2dup task-saved-stack-current !
      dup current-task @ = if drop sp! else task-rstack-current @ 12 + ! then
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
    
    \ Adjust next delayed tick
    : adjust-next-delayed-tick ( core -- )
      dup cpu-first-delayed-task @ ?dup if
        dup task-systick-start @ swap task-systick-delay @ +
        swap cpu-next-delayed-tick !
      else
        systick-counter $7FFFFFFF + swap cpu-next-delayed-tick !
      then
    ;

    \ Core of finding the next task with a higher priority; 0 returned
    \ indicates no task exists with a higher priority.
    : find-higher-priority-core ( last-task task -- higher-task )
      systick-counter swap
      code[
      r2 1 dp ldm
      .task-deadline tos r3 ldr_,[_,#_]
      r2 r3 r3 subs_,_,_
      .task-priority tos r0 ldrh_,[_,#_]
      16 r0 r0 lsls_,_,#_
      16 r0 r0 asrs_,_,#_
      r3 r2 r0 3 push
      tos 1 dp ldm
      mark<
      0 tos cmp_,#_
      ne bc>
      pc r3 r2 r0 4 pop
      >mark
      0 r0 ldr_,[sp,#_]
      .task-priority tos r1 ldrh_,[_,#_]
      16 r1 r1 lsls_,_,#_
      16 r1 r1 asrs_,_,#_
      r0 r1 cmp_,_
      le bc>
      pc r3 r2 r0 4 pop
      >mark
      ne bc>
      .task-deadline tos r0 ldr_,[_,#_]
      4 r1 ldr_,[sp,#_]
      r1 r0 r0 subs_,_,_
      8 r1 ldr_,[sp,#_]
      r1 r0 cmp_,_
      gt bc>
      pc r3 r2 r0 4 pop
      >mark
      >mark
      .task-next tos tos ldr_,[_,#_]
      b<
      ]code
    ;

    \ Find the next task with a higher priority from the active tasks; 0
    \ returned indicates no task exists with a higher priority.
    : find-higher-priority-active ( task -- higher-task )
      dup task-core @ cpu-last-active-task @ ?dup if
        swap find-higher-priority-core
      else
        drop 0
      then
    ;

    \ Find the next task with a higher priority from the delayed task; 0
    \ returned indicates no task exists with a higher priority.
    : find-higher-priority-delayed ( task -- higher-task )
      dup task-core @ cpu-last-delayed-task @ ?dup if
        swap find-higher-priority-core
      else
        drop 0
      then
    ;

    \ Core of inserting a task before another task
    : insert-task-before-core ( last-task-addr task after-task -- )
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
      4 dp tos ldr_,[_,#_]
      0 dp r1 ldr_,[_,#_]
      0 tos r1 str_,[_,#_]
      tos 1 pop
      >mark
      .task-prev tos r1 str_,[_,#_]
      .task-next r1 tos str_,[_,#_]
      cortex-m7? cortex-m33? or [if]
        8 dp tos ldr_,[_,#_]
        12 dp adds_,#_
      [else]
        tos r0 r1 3 dp ldm
      [then]
      ]code
    ;

    \ Insert a task before an active task
    : insert-task-before-active ( task after-task -- )
      dup task-core @ cpu-last-active-task -rot insert-task-before-core
    ;

    \ Insert a task before a delayed task
    : insert-task-before-delayed ( task after-task -- )
      dup task-core @ cpu-last-delayed-task -rot insert-task-before-core
    ;

    \ The core of inserting a task into first position in a list
    : insert-task-first-core ( task last-task-addr first-task-addr -- )
      code[
      tos r0 movs_,_
      0 dp r1 ldr_,[_,#_]
      4 dp r2 ldr_,[_,#_]
      8 dp tos ldr_,[_,#_]
      12 dp adds_,#_
      .task-core r2 r3 ldr_,[_,#_]
      2 r3 r3 lsls_,_,#_
      r3 r0 r0 adds_,_,_
      r3 r1 r1 adds_,_,_
      0 r0 r3 ldr_,[_,#_]
      0 r3 cmp_,#_
      ne bc>
      .task-prev r2 r3 str_,[_,#_]
      .task-next r2 r3 str_,[_,#_]
      0 r0 r2 str_,[_,#_]
      0 r1 r2 str_,[_,#_]
      pc 1 pop
      >mark
      .task-next r3 r2 str_,[_,#_]
      .task-prev r2 r3 str_,[_,#_]
      0 r0 r2 str_,[_,#_]
      0 r0 movs_,#_
      .task-next r2 r0 str_,[_,#_]
      ]code
    ;
    
    \ Insert a task into first position in the active task list
    : insert-task-first-active ( task -- )
      [ 0 cpu-last-active-task ] literal
      [ 0 cpu-first-active-task ] literal
      insert-task-first-core
    ;

    \ Insert a task into first position in the delayed task list
    : insert-task-first-delayed ( task -- )
      [ 0 cpu-last-delayed-task ] literal
      [ 0 cpu-first-delayed-task ] literal
      insert-task-first-core
    ;

    \ Insert a task into first position in the blocked task list
    : insert-task-blocked ( task -- )
      [ 0 cpu-last-blocked-task ] literal
      [ 0 cpu-first-blocked-task ] literal
      insert-task-first-core
    ;

    \ Insert a task in the active task list
    : insert-task-active ( task -- )
      dup find-higher-priority-active ?dup if
	insert-task-before-active
      else
	insert-task-first-active
      then
    ;

    \ Insert a task in the delayed task list
    : insert-task-delayed ( task -- )
      dup find-higher-priority-delayed ?dup if
	insert-task-before-delayed
      else
        dup insert-task-first-delayed
        task-core @ adjust-next-delayed-tick
      then
    ;

    \ Insert a task
    : insert-task ( task -- )
      dup task-state h@
      dup readied = if drop insert-task-active exit then
      dup delayed = if drop insert-task-delayed exit then
      dup blocked-timeout = if drop insert-task-delayed exit then
      dup blocked-wait = if drop insert-task-active exit then
      block-timed-out = if insert-task-active exit then
      insert-task-blocked
    ;

    \ Insert a task last in the active task list
    : insert-task-last-active ( task -- )
      dup task-core @ cpu-last-active-task @ ?dup if
        insert-task-before-active
      else
        insert-task-first-active
      then
    ;

    \ Insert a task last in the delayed task list
    : insert-task-last-delayed ( task -- )
      dup task-core @ cpu-last-delayed-task @ ?dup if
        insert-task-before-delayed
      else
        dup insert-task-first-delayed
        task-core @ adjust-next-delayed-tick
      then
    ;

    \ Insert a task last
    : insert-task-last ( task -- )
      dup task-state h@
      dup readied = if drop insert-task-last-active exit then
      dup delayed = if drop insert-task-last-delayed exit then
      dup blocked-timeout = if drop insert-task-last-delayed exit then
      dup blocked-wait = if drop insert-task-last-active exit then
      block-timed-out = if insert-task-last-active exit then
      insert-task-blocked
    ;

    \ Core of removing a task in first position
    : remove-task-first-core ( task last-task-addr first-task-addr -- )
      code[
      tos r0 movs_,_
      cortex-m7? cortex-m33? or [if]
        0 dp r1 ldr_,[_,#_]
        4 dp r2 ldr_,[_,#_]
        8 dp tos ldr_,[_,#_]
        12 dp adds_,#_
      [else]
        tos r2 r1 3 dp ldm
      [then]
      .task-core r2 r3 ldr_,[_,#_]
      2 r3 r3 lsls_,_,#_
      r3 r0 r0 adds_,_,_
      r3 r1 r1 adds_,_,_
      .task-prev r2 r3 ldr_,[_,#_]
      0 r0 r3 str_,[_,#_]
      0 r3 cmp_,#_
      ne bc>
      0 r1 r3 str_,[_,#_]
      .task-prev r2 r3 str_,[_,#_]
      .task-next r2 r3 str_,[_,#_]
      pc 1 pop
      >mark
      0 r0 movs_,#_
      .task-prev r2 r0 str_,[_,#_]
      .task-next r2 r0 str_,[_,#_]
      .task-next r3 r0 str_,[_,#_]
      ]code
    ;

    \ Remove a first task from the active list
    : remove-task-first-active ( task -- )
      [ 0 cpu-last-active-task ] literal
      [ 0 cpu-first-active-task ] literal
      remove-task-first-core
    ;

    \ Remove a first task from the delayed task list
    : remove-task-first-delayed ( task -- )
      dup
      [ 0 cpu-last-delayed-task ] literal
      [ 0 cpu-first-delayed-task ] literal
      remove-task-first-core
      task-core @ adjust-next-delayed-tick
    ;

    \ Core of removing a task
    : remove-task-core ( task last-task-addr first-task-addr -- )
      code[
      r4 1 push
      tos r0 movs_,_
      0 dp r1 ldr_,[_,#_]
      4 dp r2 ldr_,[_,#_]
      8 dp tos ldr_,[_,#_]
      12 dp adds_,#_
      .task-core r2 r3 ldr_,[_,#_]
      2 r3 r3 lsls_,_,#_
      r3 r0 r0 adds_,_,_
      r3 r1 r1 adds_,_,_
      .task-prev r2 r3 ldr_,[_,#_]
      .task-next r2 r4 ldr_,[_,#_]
      0 r3 cmp_,#_
      ne bc>
      0 r1 r4 str_,[_,#_]
      b>
      2swap
      >mark
      .task-next r3 r4 str_,[_,#_]
      >mark
      0 r4 cmp_,#_
      ne bc>
      0 r0 r3 str_,[_,#_]
      b>
      2swap
      >mark
      .task-prev r4 r3 str_,[_,#_]
      >mark
      0 r0 movs_,#_
      .task-prev r2 r0 str_,[_,#_]
      .task-next r2 r0 str_,[_,#_]
      r4 1 pop
      ]code
    ;

    \ : remove-task-core { task last-task-addr first-task-addr -- }
    \   task task-core @ 2 lshift dup +to last-task-addr +to first-task-addr
    \   task task-prev @ { prev-task }
    \   task task-next @ { next-task }
    \   prev-task 0= if
    \     next-task last-task-addr !
    \   else
    \     next-task prev-task task-next !
    \   then
    \   next-task 0= if
    \     prev-task first-task-addr !
    \   else
    \     prev-task next-task task-prev !
    \   then
    \   0 task task-prev !
    \   0 task task-next !
    \ ;

    \ Remove a task from the active list
    : remove-task-active ( task -- )
      [ 0 cpu-last-active-task ] literal
      [ 0 cpu-first-active-task ] literal
      remove-task-core
    ;

    \ Remove a task from the delayed task list
    : remove-task-delayed ( task -- )
      dup
      [ 0 cpu-last-delayed-task ] literal
      [ 0 cpu-first-delayed-task ] literal
      remove-task-core
      task-core @ adjust-next-delayed-tick
    ;

    \ Remove a task from the blocked task list
    : remove-task-blocked ( task -- )
      [ 0 cpu-last-blocked-task ] literal
      [ 0 cpu-first-blocked-task ] literal
      remove-task-core
    ;

    \ Remove a task by itself
    : remove-only-task ( task -- )
      dup task-state h@
      swap task-core @
      over readied = if
        nip
        0 over cpu-first-active-task !
        0 swap cpu-last-active-task !
        drop exit
      then
      over delayed = if
        nip
        0 over cpu-first-delayed-task !
        0 swap cpu-last-delayed-task !
        drop exit
      then
      over blocked-timeout = if
        nip
        0 over cpu-first-delayed-task !
        0 swap cpu-last-delayed-task !
        drop exit
      then
      over blocked-wait = if
        nip
        0 over cpu-first-active-task !
        0 swap cpu-last-active-task !
        drop exit
      then
      swap block-timed-out = if
        0 over cpu-first-active-task !
        0 swap cpu-last-active-task !
        exit
      then
      0 over cpu-first-blocked-task !
      0 swap cpu-last-blocked-task !
    ;
    
    \ Remove a task
    : remove-task ( task -- )
      dup task-prev @ over task-next @ or if
        dup task-state h@
        dup readied = if remove-task-active exit then
        dup delayed = if remove-task-delayed exit then
        dup blocked-timeout = if remove-task-delayed exit then
        dup blocked-wait = if remove-task-delayed exit then
        dup block-timed-out = if remove-task-active exit then
        drop remove-task-blocked
      else
        remove-only-task
      then
    ;

    \ Update task state
    : update-task-state ( state task -- )
      dup dup task-core @ cpu-current-task @ <> over task-active@ 0> and if
        dup remove-task
        tuck task-state h!
        insert-task
      else
        task-state h!
      then
    ;

    \ Limit task deadlines
    : limit-task-deadlines-core ( last-task -- )
      systick-counter task-deadline-limit -
      code[
      tos r0 movs_,_
      0 dp r1 ldr_,[_,#_]
      4 dp tos ldr_,[_,#_]
      8 dp adds_,#_
      mark<
      0 r1 cmp_,#_
      ne bc>
      pc 1 pop
      >mark
      .task-deadline r1 r2 ldr_,[_,#_]
      r0 r2 r3 subs_,_,_
      ge bc>
      .task-deadline r1 r0 str_,[_,#_]
      >mark
      .task-next r1 r1 ldr_,[_,#_]
      b<
      ]code
    ;

    \ Limit task deadlines
    : limit-task-deadlines ( -- )
      limit-task-deadlines-systick @ systick-counter - 0<= if
        last-active-task @ limit-task-deadlines-core
        last-delayed-task @ limit-task-deadlines-core
        last-blocked-task @ limit-task-deadlines-core
        limit-task-deadlines-interval limit-task-deadlines-systick +!
      then
    ;
    
    \ Start a task change
    : start-validate-task-change ( task -- )
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
        dup task-active h@ 16 lshift 16 arshift 0> if
          dup task-timeslice @ negate over task-core @ cpu-total-timeslice +!
        then
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
        readied swap update-task-state true
      else
        drop false
      then
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Validate not timing out
  : validate-timeout ( task -- )
    check-timeout triggers x-timed-out
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

  \ Set task interval in ticks (0< means default)
  : task-interval! ( interval task -- )
    dup validate-not-terminated
    task-interval !
  ;

  \ Get task interval in ticks (0< means default)
  : task-interval@ ( task -- interval )
    dup validate-not-terminated
    task-interval @
  ;

  \ Set task deadline in ticks
  : task-deadline! ( deadline task -- )
    [:
      dup validate-not-terminated
      true over task-deadline-set? !
      task-deadline !
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Get task deadlline in ticks
  : task-deadline@ ( task -- deadline )
    dup validate-not-terminated
    task-deadline @
  ;

  \ Set task timeslice
  : task-timeslice! ( timeslice task -- )
    [:
      dup validate-not-terminated
      dup task-active@ 0> if
        dup task-timeslice @ negate over task-core @ cpu-total-timeslice !
      then
      dup current-task @ = if
      2dup task-timeslice @ - task-systick-counter @ +
        task-systick-counter !
      then
      swap 0 max over task-timeslice !
      dup task-active@ 0> if
        dup task-timeslice @ swap task-core @ cpu-total-timeslice !
      else
        drop
      then
    ;] over task-core @ critical-with-other-core-spinlock
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
      dup 1 = if
        over task-deadline-set? @ not if
          over task-core @ cpu-total-timeslice @ systick-counter +
          2 pick task-deadline !
        then
        over task-timeslice @ 2 pick task-core @ cpu-total-timeslice +!
        dup 1 > if over remove-task then
        dup 0 > if over insert-task then
      then
      swap task-active h!
    ;] over task-core @ critical-with-other-core-spinlock
  ;

  \ Stop a task's execution
  : stop ( task -- )
    [:
      dup start-validate-task-change
      dup task-active@ 1-
      dup 0= if
        over task-timeslice @ negate 2 pick task-core @ cpu-total-timeslice +!
      then
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

  \ Adjust deadline for delayed task
  : adjust-delayed-deadline ( task -- )
    dup task-systick-start @ over task-systick-delay @ + swap task-deadline !
  ;

  \ Delay a task
  : delay ( ticks-delay ticks-start task -- )
    [:
      dup validate-not-terminated
      tuck task-systick-start !
      tuck task-systick-delay !
      dup adjust-delayed-deadline
      delayed over update-task-state
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
        dup adjust-delayed-deadline
	blocked-timeout over update-task-state
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
      current-task @ adjust-delayed-deadline
      disable-int
      begin dup bit current-task @ task-notified-bitmap bit@ not while
	dup current-task @ task-current-notify !
        blocked-timeout current-task @ update-task-state
        enable-int
	cpu-index end-critical-with-other-core-spinlock pause-wo-reschedule
        cpu-index begin-critical-with-other-core-spinlock
        current-task @ check-timeout if
          -1 current-task @ task-current-notify !
          ['] x-timed-out ?raise
        then
        disable-int
      repeat
      current-task @ task-notify-area @ over cells + @
      swap bit current-task @ task-notified-bitmap bic!
      enable-int
      -1 current-task @ task-current-notify !
    ;] cpu-index critical-with-other-core-spinlock
  ;

  \ Wait until a timeout on the specified notification index and return the
  \ value for that notification index once notified, unless already notified,
  \ where then that value will be returned and the notified state will be
  \ cleared immediately; after fetching the value of the mailbox, atomically
  \ set the mailbox to the specified value. x-timed-out is raised if the
  \ timeout is reached.
  : wait-notify-set-timeout ( x' ticks-delay ticks-start notify-index -- x )
    [:
      dup current-task @ validate-notify
      swap current-task @ task-systick-start !
      swap current-task @ task-systick-delay !
      current-task @ adjust-delayed-deadline
      disable-int
      begin dup bit current-task @ task-notified-bitmap bit@ not while
	dup current-task @ task-current-notify !
        blocked-timeout current-task @ update-task-state
        enable-int
	cpu-index end-critical-with-other-core-spinlock pause-wo-reschedule
        cpu-index begin-critical-with-other-core-spinlock
        current-task @ check-timeout if
          -1 current-task @ task-current-notify !
          ['] x-timed-out ?raise
        then
        disable-int
      repeat
      current-task @ task-notify-area @ over cells + dup >r @
      rot r> !
      swap bit current-task @ task-notified-bitmap bic!
      enable-int
      -1 current-task @ task-current-notify !
    ;] cpu-index critical-with-other-core-spinlock
  ;

  \ Wait until a timeout on the specified notification index and return the
  \ value for that notification index once notified, unless already notified,
  \ where then that value will be returned and the notified state will be
  \ cleared immediately; after fetching the value of the mailbox, pass the
  \ current value of the mailbox to the passed-in execution token, which is
  \ executed with the current task locked and interrupts disabled, and then
  \ atomically set the mailbox to the returned value. x-timed-out is raised if
  \ the timeout is reached.
  : wait-notify-update-timeout ( xt ticks-delay ticks-start notify-index -- x )
    \ xt: ( x -- x' )
    [:
      dup current-task @ validate-notify
      swap current-task @ task-systick-start !
      swap current-task @ task-systick-delay !
      current-task @ adjust-delayed-deadline
      disable-int
      begin dup bit current-task @ task-notified-bitmap bit@ not while
	dup current-task @ task-current-notify !
        blocked-timeout current-task @ update-task-state
        enable-int
	cpu-index end-critical-with-other-core-spinlock pause-wo-reschedule
        cpu-index begin-critical-with-other-core-spinlock
        current-task @ check-timeout if
          -1 current-task @ task-current-notify !
          ['] x-timed-out ?raise
        then
        disable-int
      repeat
      current-task @ task-notify-area @ over cells + dup >r @
      dup -rot 2>r swap execute 2r> rot r> !
      swap bit current-task @ task-notified-bitmap bic!
      enable-int
      -1 current-task @ task-current-notify !
    ;] cpu-index critical-with-other-core-spinlock
  ;

  \ Mark a task as waiting
  : block-wait ( wake-count task -- )
    dup validate-not-terminated
    tuck task-wake-after !
    blocked-wait over update-task-state
    current-task @ = if pause-reschedule-last then
  ;
  
  \ Mark a task as blocked indefinitely
  : block-indefinite ( task -- )
    [:
      dup validate-not-terminated
      -1 over task-ready-count +!
      dup task-ready-count @ 0< if
	blocked-indefinite over update-task-state
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
        blocked-indefinite current-task @ update-task-state
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
      disable-int
      begin dup bit current-task @ task-notified-bitmap bit@ not while
        dup current-task @ task-current-notify !
        blocked-indefinite current-task @ update-task-state
        enable-int
	cpu-index end-critical-with-other-core-spinlock pause-wo-reschedule
        cpu-index begin-critical-with-other-core-spinlock
        disable-int
      repeat
      current-task @ task-notify-area @ over cells + @
      swap bit current-task @ task-notified-bitmap bic!
      -1 current-task @ task-current-notify !
      enable-int
    ;] cpu-index critical-with-other-core-spinlock
  ;

  \ Wait indefinitely on the specified notification index and return the value
  \ for that notification index once notified, unless already notified, where
  \ then that value will be returned and the notified state will be cleared
  \ immediately; after fetching the value of the mailbox, atomically set the
  \ mailbox to the specified value.
  : wait-notify-set-indefinite ( x' notify-index -- x )
    [:
      dup current-task @ validate-notify
      disable-int
      begin dup bit current-task @ task-notified-bitmap bit@ not while
        dup current-task @ task-current-notify !
        blocked-indefinite current-task @ update-task-state
        enable-int
	cpu-index end-critical-with-other-core-spinlock pause-wo-reschedule
        cpu-index begin-critical-with-other-core-spinlock
        disable-int
      repeat
      current-task @ task-notify-area @ over cells + dup >r @
      rot r> !
      swap bit current-task @ task-notified-bitmap bic!
      -1 current-task @ task-current-notify !
      enable-int
    ;] cpu-index critical-with-other-core-spinlock
  ;

  \ Wait indefinitely on the specified notification index and return the value
  \ for that notification index once notified, unless already notified, where
  \ then that value will be returned and the notified state will be cleared
  \ immediately; after fetching the value of the mailbox, pass the current
  \ value of the mailbox to the passed-in execution token, which is executed
  \ with the current task locked and interrupts disabled, and then atomically
  \ set the mailbox to the returned value.
  : wait-notify-update-indefinite ( xt notify-index -- x ) \ xt: ( x -- x' )
    [:
      dup current-task @ validate-notify
      disable-int
      begin dup bit current-task @ task-notified-bitmap bit@ not while
        dup current-task @ task-current-notify !
        blocked-indefinite current-task @ update-task-state
        enable-int
	cpu-index end-critical-with-other-core-spinlock pause-wo-reschedule
        cpu-index begin-critical-with-other-core-spinlock
        disable-int
      repeat
      current-task @ task-notify-area @ over cells + dup >r @
      dup -rot 2>r swap execute 2r> rot r> !
      swap bit current-task @ task-notified-bitmap bic!
      -1 current-task @ task-current-notify !
      enable-int
    ;] cpu-index critical-with-other-core-spinlock
  ;

  continue-module task-internal

    \ Core of readying a task
    : do-ready ( task -- )
      dup task-current-notify @ -1 = if
        0 over task-ready-count !
\        dup task-ready-count @ 1+ 0 min dup 2 pick task-ready-count !
\	0>= if
	  readied swap update-task-state
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
      disable-int
      2dup swap bit swap task-notified-bitmap bis!
      dup task-current-notify @ rot = if
	readied swap update-task-state true
      else
	drop false
      then
      enable-int
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
      disable-int
      2dup swap bit swap task-notified-bitmap bis!
      dup task-current-notify @ rot = if
	readied swap update-task-state true
      else
	drop false
      then
      enable-int
    ;] over task-core @ critical-with-other-core-spinlock
    if pause-wo-reschedule then
  ;

  \ Notify a task for a specified notification index, updating the notification
  \ value with an xt with the signature ( x0 -- x1 ) with the specified task
  \ locked and interrupts disabled.
  : notify-update ( xt notify-index task -- )
    [:
      dup validate-not-terminated
      2dup validate-notify
      rot >r ( index task )
      dup task-notify-area @ 2 pick cells + ( index task addr )
      r> rot >r rot >r swap dup >r
      disable-int
      @ swap execute r> ! ( )
      r> r> 2dup swap bit swap task-notified-bitmap bis!
      dup task-current-notify @ rot = if
        readied swap update-task-state true
      else
	drop false
      then
      enable-int
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

  \ Wait for a notification at a specified notification index with a specified
  \ initialized timeout and return the notification value; after fetching the
  \ value of the mailbox, atomically set the mailbox to the specified value.
  : wait-notify-set ( x' notify-index -- x )
    [:
      timeout @ no-timeout <> if
        current-task @ timeout-systick-delay @
        current-task @ timeout-systick-start @ rot
        true
      else
        false
      then
    ;] cpu-index critical-with-other-core-spinlock
    if wait-notify-set-timeout else wait-notify-set-indefinite then
  ;

  \ Wait for a notification at a specified notification index with a specified
  \ initialized timeout and return the notification value; after fetching the
  \ value of the mailbox, pass the current value of the mailbox to the passed-in
  \ execution token, which is executed with the current task locked and
  \ interrupts disabled, and then atomically set the mailbox to the returned
  \ value.
  : wait-notify-update ( xt notify-index -- x ) \ xt: ( x -- x' )
    [:
      timeout @ no-timeout <> if
        current-task @ timeout-systick-delay @
        current-task @ timeout-systick-start @ rot
        true
      else
        false
      then
    ;] cpu-index critical-with-other-core-spinlock
    if wait-notify-update-timeout else wait-notify-update-indefinite then
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
        i cpu-first-blocked-task @ begin ?dup while
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
          dup adjust-delayed-deadline
	  blocked-timeout over update-task-state
	else
	  nip nip
	then
      else
	-1 over task-ready-count +!
	dup task-ready-count @ 0< if
	  blocked-indefinite over update-task-state
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
      sp@ over task-saved-stack-current !
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
      -1 over task-interval !
      systick-counter over task-deadline !
      false over task-deadline-set? !
      false over task-float32-ctx-saved? !
      0 current-lock-held !
      0 over task-next !
      0 over task-prev !
      task-guard-value over task-start-guard !
      task-guard-value over task-end-guard !
      dup main-task !
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
        dup ['] task-rstack-end for-task@
        2dup swap ['] task-stack-base for-task!
        over task-saved-stack-current !
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
	c" aux-main" over task-name !
	default-timeslice over task-timeslice !
	default-min-timeslice over task-min-timeslice !
        default-timeslice over task-saved-systick-counter !
        -1 over task-interval !
        systick-counter over task-deadline !
        false over task-deadline-set? !
        false over task-float32-ctx-saved? !
	dup ['] task-rstack-base for-task@ over task-rstack-current !
	next-user-space over task-dict-base @ +
	over ['] task-ram-here for-task!
	0 over task-next !
        0 over task-prev !
        task-guard-value over task-start-guard !
        task-guard-value over task-end-guard !
        task-init-hook @ ?dup if over swap execute then
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
          dup adjust-delayed-deadline
          blocked-timeout swap update-task-state
        else
          2drop drop
        then
      else
        -1 over task-ready-count +!
        dup task-ready-count @ 0< if
          blocked-indefinite swap update-task-state
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
    dup ['] task-rstack-end for-task@ 2dup swap ['] task-stack-base for-task!
    over task-saved-stack-current !
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
    -1 over task-interval !
    systick-counter over task-deadline !
    false over task-deadline-set? !
    false over task-float32-ctx-saved? !
    dup ['] task-rstack-base for-task@
    over ['] task-stack-base for-task@ ['] task-entry
    init-context over task-rstack-current !
    next-user-space over task-dict-base @ + over ['] task-ram-here for-task!
    0 over task-next !
    0 over task-prev !
    task-guard-value over task-start-guard !
    task-guard-value over task-end-guard !
    task-init-hook @ ?dup if over swap execute then
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
      swap 4 align next-user-space 4 align + swap tuck task-dict-size !
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

    \ PendSV return value
    variable pendsv-return

    \ Save task state
    : save-task-state { prev-task -- }
      task-systick-counter @ prev-task task-saved-systick-counter !
      [ defined? float32 ] [if]
        float32::vsave prev-task task-float32-ctx 4!
        true prev-task task-float32-ctx-saved? !
      [then]
    ;

    \ Restore task state
    : restore-task-state { next-task -- }
      next-task task-dict-base @ dict-base !
      [ defined? float32 ] [if]
        next-task task-float32-ctx-saved? @ if
          next-task task-float32-ctx 4@ float32::vload
          false next-task task-float32-ctx-saved? !
        then
      [then]
    ;

    \ Handle task termination
    : handle-task-terminated ( task -- )
      >r r@ task-terminate-hook @ ?dup if
        claim-same-core-spinlock
        r@ task-active@ 0<= if r@ task-timeslice @ total-timeslice +! then
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
      begin wake-counter @ 1+ current-task @ block-wait again
    ;

    \ Initialize the extra task
    : init-extra-task ( -- )
      extra-task @ if
        0 ['] do-extra-task extra-task @ cpu-index init-task
      else
        0 ['] do-extra-task 320 128 512 spawn extra-task !
      then
      claim-same-core-spinlock
      c" extra" extra-task @ task-name !
      -1 extra-task @ task-priority h!
      1 extra-task @ task-active h!
      extra-task @ insert-task-active
      release-same-core-spinlock
    ;

    \ Adjust a task's deadline
    : adjust-deadline { task -- }
      task task-state h@
      dup readied =
      over blocked-wait = or
      swap block-timed-out = or if
        task task-deadline-set? @ not if
          task task-interval @ dup 0< if
            drop total-timeslice @ task task-timeslice @ -
            systick-counter + task task-deadline !
          else { interval }
            interval task task-deadline +!
            task task-deadline @ systick-counter - interval negate < if
              systick-counter interval - task task-deadline !
            then
          then
        else
          false task task-deadline-set? !
        then
      then
    ;

    \ Reschedule previous task
    : reschedule-task ( task -- )
      true in-task-change !
      reschedule-last? @ if
        claim-same-core-spinlock
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
        claim-same-core-spinlock
        dup task-active@ 0> if
          reschedule? @ task-systick-counter @ 0<= or if
            dup adjust-deadline
          then
          insert-task
        else
          dup terminated? if
            terminated-task !
          else
            drop
          then
        then
        release-same-core-spinlock
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
      mark<
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

    \ Adjust the current task's timeslice
    : adjust-timeslice ( -- )
      current-task @ { task }
      task task-saved-systick-counter @ 0<= if
        task task-saved-systick-counter @ task task-timeslice @ +
        task task-min-timeslice @ max
      else
        task task-saved-systick-counter @
      then
      task-systick-counter !
    ;

    \ Handle task-switching
    : switch-tasks ( -- )
      r> pendsv-return !

      current-task @ if
        current-task @ dup task-end-guard @ task-guard-value <>
        swap task-start-guard @ task-guard-value <> or if
          exception::handle-panic
        then
      then
        
      in-critical @ 0= in-task-change @ 0= and if

        handle-pending-ops

        do-pause? @ if

          watchdog-hook @ ?execute
          
          1 pause-count +!
          
          current-task @ dup prev-task !
          ?dup if dup save-task-state reschedule-task then

          begin
            true in-task-change !
            limit-task-deadlines
            claim-same-core-spinlock

            next-delayed-tick @ systick-counter - 0<= if
              first-delayed-task @ ?dup if
                dup remove-task-first-delayed
              else
                first-active-task @ ?dup if
                  dup remove-task-first-active
                  dup task-state h@ blocked-wait = if
                    wake-counter @ over task-wake-after @ - 0> if
                      readied over task-state h!
                    else
                      insert-task-last-active 0
                    then
                  then
                else
                  0
                then
              then
            else
              first-active-task @ ?dup if
                dup remove-task-first-active
                dup task-state h@ blocked-wait = if
                  wake-counter @ over task-wake-after @ - 0> if
                    readied over task-state h!
                  else
                    insert-task-last-active 0
                  then
                then
              else
                0
              then
            then

            dup 0= if
              first-active-task @ 0=
              first-delayed-task @ 0= and
              first-blocked-task @ 0= and if
                drop init-extra-task
                first-active-task @ ?dup if
                  dup remove-task-first-active
                else
                  0
                then
              then
            then
            dup if
              dup task-ready-count @ 0 max over task-ready-count !
            then
            release-same-core-spinlock
            dup if
              dup task-active@ 0> if
                dup task-state h@
                dup blocked-timeout = swap block-timed-out = or if
                  block-timed-out over task-state h!
                else
                  readied over task-state h!
                then
              else
                drop 0
              then
            then
            dup current-task !
            false in-task-change !
            dup if
              false waiting-for-task? !
            else
              true waiting-for-task? !
              handle-pending-ops sleep
            then
          until

          prev-task @ current-task @ <> if
            disable-int
            sp@ current-task @ task-saved-stack-current !
            current-task @ task-rstack-current @ context-switch
            prev-task @ if prev-task @ task-rstack-current ! else drop then
            current-task @ restore-task-state
            sp@ current-task @ task-saved-stack-current !
            enable-int
          then

          adjust-timeslice
          
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
      field: task-info-dict-size
      field: task-info-dict-used
      hfield: task-info-stack-size
      hfield: task-info-stack-used
      hfield: task-info-rstack-size
      hfield: task-info-rstack-used
      field: task-info-name-len
      28 +field task-info-name-bytes
      
    end-structure

    \ Count tasks in list
    : count-tasks-in-list ( first-task -- count )
      0 swap begin ?dup while swap 1+ swap task-prev @ repeat
    ;

    \ Get the task count
    : get-cpu-task-count ( cpu -- count )
      dup cpu-current-task @ if 1 else 0 then
      over cpu-first-active-task @ count-tasks-in-list +
      over cpu-first-delayed-task @ count-tasks-in-list +
      swap cpu-first-blocked-task @ count-tasks-in-list +
    ;

    \ Copy a task name into a task info structure
    : copy-task-name-for-info { info task -- }
      task task-name@ ?dup if
        count dup info task-info-name-len !
        info task-info-name-bytes swap 23 min move
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
      task task-rstack-size h@ info task-info-rstack-size h!
      task task-stack-size h@ info task-info-stack-size h!
      task task-dict-size @ info task-info-dict-size !
      task ['] task-rstack-base for-task@
      task task-rstack-current @ - info task-info-rstack-used h!
      task ['] task-stack-base for-task@
      task current-task @ <> if task task-saved-stack-current @ else sp@ then
      - info task-info-stack-used h!
      task task-dict-current
      task task-dict-base @ - info task-info-dict-used !
      info task copy-task-name-for-info
      info task copy-task-delay-for-info
    ;
    
    \ Dump task name
    : dump-task-name { info -- }
      info task-info-name-len @ ?dup if
        info task-info-name-bytes over type 24 swap - 0 max spaces
      else
        24 spaces
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

    \ Dump return stack size
    : dump-rstack-size ( info -- )
      here swap task-info-rstack-size h@ format-unsigned
      dup 11 swap - spaces type
    ;

    \ Dump return stack used
    : dump-rstack-used ( info -- )
      here swap task-info-rstack-used h@ format-unsigned
      dup 11 swap - spaces type
    ;
    
    \ Dump stack size
    : dump-stack-size ( info -- )
      here swap task-info-stack-size h@ format-unsigned
      dup 10 swap - spaces type
    ;
    
    \ Dump stack used
    : dump-stack-used ( info -- )
      here swap task-info-stack-used h@ format-unsigned
      dup 10 swap - spaces type
    ;

    \ Dump dictionary size
    : dump-dict-size ( info -- )
      here swap task-info-dict-size @ format-unsigned
      dup 10 swap - spaces type
    ;

    \ Dump dictionary used
    : dump-dict-used ( info -- )
      here swap task-info-dict-used @ format-unsigned
      dup 10 swap - spaces type
    ;

    \ Dump task ehader
    : dump-task-header ( -- )
      cr ." task     name                     priority state      "
      ." until       delay"
      cr ."          rstack-size rstack-used stack-size stack-used "
      ." dict-size  dict-used  "
    ;

  \ Extra space when calculating room needed for dumping tasks
  64 constant extra-space
  
  end-module

  \ Dump task information for all tasks
  : dump-tasks ( -- )
    dump-task-header
    cpu-count 0 ?do
      cpu-count 1 > if
        cr ." cpu " i (.) ." :"
      then
      i [:
        current-task @ task-dict-end
        current-task @ task-dict-current - { space }
        dup get-cpu-task-count dup task-info-size *
        dup extra-space + space u< if
          [: { cpu count info }
            info { current-info }
            cpu cpu-current-task @ ?dup if
              current-info swap copy-task-info
              task-info-size +to current-info
            then
            cpu cpu-first-active-task @ begin ?dup while
              current-info over copy-task-info
              task-prev @
              task-info-size +to current-info
            repeat
            cpu cpu-first-delayed-task @ begin ?dup while
              current-info over copy-task-info
              task-prev @
              task-info-size +to current-info
            repeat
            cpu cpu-first-blocked-task @ begin ?dup while
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
                cr ."          " info dump-rstack-size
                space info dump-rstack-used
                space info dump-stack-size
                space info dump-stack-used
                space info dump-dict-size
                space info dump-dict-used
                task-info-size +to info
              loop
            ;] swap outside-critical-with-other-core-spinlock
          ;] with-aligned-allot
        else
          2drop drop
          cr ." *** NOT ENOUGH DICTIONARY SPACE AVAILABLE TO DISPLAY TASKS ***"
        then
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
	$00 SHPR3_PRI_15!
	$00 SHPR2_PRI_11!
        $FF SHPR3_PRI_14!
        [ rp2040? ] [if]
          $00 SIO_IRQ_PROC1 NVIC_IPR_IP!
          SIO_IRQ_PROC1 NVIC_ISER_SETENA!
        [else]
          [ rp2350? ] [if]
            $00 SIO_IRQ_FIFO NVIC_IPR_IP!
            SIO_IRQ_FIFO NVIC_ISER_SETENA!
            init-core-1-ticks
          [then]
        [then]
        1 pause-enabled !
        [ defined? float32 ] [if]
          float32::float32-internal::enable-float
        [then]
        core-init-hook @ execute
        true core-1-launched !
        begin current-task @ task-active h@ 16 lshift 16 arshift 0<= while
          pause
        repeat
        try-and-display-error
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
          systick-counter limit-task-deadlines-interval +
          over cpu-limit-task-deadlines-systick !
          systick-counter over cpu-next-delayed-tick !
          >r task-allot >r
          2r@ cpu-current-task !
          2r@ cpu-main-task !
          2r@ cpu-prev-task !
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
      [:
        disable-int
        tuck task-raise !
        [: current-task task-raise @ 0 current-task task-raise ! ?raise ;]
        over force-call
        readied swap update-task-state
        enable-int
      ;] over task-core @ critical-with-other-core-spinlock
    else
      2drop
    then
  ;

  \ Signal all other tasks in a list to raise an exception
  : signal-all-tasks-in-list { xt exclude-task core task -- }
    begin task while
      task exclude-task <> task core cpu-extra-task @ <> and if
        xt task task-raise !
        [: current-task task-raise @ 0 current-task task-raise ! ?raise ;]
        task force-call
        task task-next @ { next-task }
        readied task update-task-state
        next-task to task
      then
    repeat
  ;

  \ Signal all other tasks to raise exceptions
  : signal-all-other ( xt task -- )
    over 0<> if
      [: { xt exclude-task }
        disable-int
        cpu-count 0 ?do
          xt exclude-task i dup cpu-current-task @ signal-all-tasks-in-list
          xt exclude-task i dup cpu-last-active-task @ signal-all-tasks-in-list
          xt exclude-task i dup cpu-last-blocked-task @ signal-all-tasks-in-list
          xt exclude-task i dup cpu-last-delayed-task @ signal-all-tasks-in-list
        loop
        enable-int
      ;] critical-with-all-core-spinlock
    then
  ;

  \ Saved attention hook
  variable saved-attention-hook

  \ Main interrupt exception
  : x-interrupt-main ." *** INTERRUPT MAIN ***" cr ;

  \ Other interrupt exception
  : x-interrupt-other ;
  
  \ Handle attention
  : do-attention ( c -- )
    dup [char] z = if
      drop false attention? ! ['] x-interrupt-main main-task signal
    else
      dup [char] c = if
        drop
        display-red ." *** INTERRUPT OTHER ***" cr display-normal
        ['] x-interrupt-other main-task signal-all-other
      else
        saved-attention-hook @ execute
      then
    then
  ;
  

  \ Initialize multitasking
  : init-tasker ( -- )
    disable-int
    0 task-init-hook !
    0 watchdog-hook !
    NVIC_ICPR_CLRPEND_All!
    0 pause-enabled !
    $00 SHPR3_PRI_15!
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
    [: init-systick-aux-core enable-int ;] core-init-hook !
    default-timeslice 0 cpu-total-timeslice !
    cpu-count 1 ?do 0 i cpu-total-timeslice ! loop
    cpu-count 0 ?do
      false i cpu-waiting-for-task? !
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
      then
      0 i cpu-first-active-task !
      0 i cpu-last-active-task !
      0 i cpu-first-delayed-task !
      0 i cpu-last-delayed-task !
      0 i cpu-first-blocked-task !
      0 i cpu-last-blocked-task !
      0 i cpu-pause-count !
      systick-counter limit-task-deadlines-interval +
      i cpu-limit-task-deadlines-systick !
      systick-counter i cpu-next-delayed-tick !
    loop
    ['] execute svcall-vector vector!
    ['] switch-tasks pendsv-vector vector!
    ['] task-systick-handler systick-vector vector!
    [: cpu-index 0= current-task main-task = and ;] in-main?-hook !
    [:
      display-red cr ." *** HARDWARE EXCEPTION, TASK TERMINATED ***"
      display-normal cr
      terminated-crashed current-task terminate
    ;] crash-hook !
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
