\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ Compile this to flash
compile-to-flash

\ Declare a RAM variable for the end of free RAM memory
variable free-end

\ Main task
variable main-task

\ Current task
variable current-task

\ Starting task for a pause
variable start-task

\ Pause count
variable pause-count

\ The task structure
begin-structure task
  \ Return stack size
  hfield: task-rstack-size

  \ Data stack size
  hfield: task-stack-size

  \ Dictionary size
  field: task-dict-size

  \ Current return stack offset
  hfield: task-rstack-offset

  \ Current data stack offset
  hfield: task-stack-offset

  \ Current dictionary offset
  field: task-dict-offset

  \ Handler
  field: task-handler
  
  \ Task active state ( > 0 active, <= 0 inactive )
  field: task-active

  \ Task wait state
  field: task-wait

  \ Task systick start time
  field: task-systick-start
  
  \ Task systick delay time
  field: task-systick-delay

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

\ Get task current return stack address
: task-rstack-current ( task -- addr )
  dup task-rstack-base swap task-rstack-offset h@ -
;

\ Get task current stack address
: task-stack-current ( task -- addr )
  dup task-stack-base swap task-stack-offset h@ -
;

\ Get task current dictionary address
: task-dict-current ( task -- addr )
  dup task-dict-base swap task-dict-offset @ +
;

\ Set task current return stack address
: task-rstack-current! ( addr task -- )
  dup task-rstack-base rot - swap task-rstack-offset h!
;

\ Set task current stack address
: task-stack-current! ( addr task -- )
  dup task-stack-base rot - swap task-stack-offset h!
;

\ Set task current dictionary address
: task-dict-current! ( addr task -- )
  dup task-dict-base rot swap - swap task-dict-offset !
;

\ Push data onto a task's stack
: push-task-stack ( x task -- )
  dup task-stack-current cell - tuck swap task-stack-current! !
;

\ Push data onto a task's return stack
: push-task-rstack ( x task -- )
  dup task-rstack-current cell - tuck swap task-rstack-current! !
;

\ Enable a task
: enable-task ( task -- ) dup task-active @ 1+ swap task-active ! ;

\ Disable a task
: disable-task ( task -- ) dup task-active @ 1- swap task-active ! ;

\ Force-enable a task
: force-enable-task ( task -- )
  dup task-active @ 1 < if
    1 swap task-active !
  else
    drop
  then
;

\ Force-disable a task
: force-disable-task ( task -- )
  dup task-active @ 0> if
    0 swap task-active !
  else
    drop
  then
;

\ Mark a task as waiting
: wait-task ( task -- )
  true swap task-wait !
;

\ Initialize the main task
: init-main-task ( -- )
  free-end @ task -
  rstack-base @ rstack-end @ - over task-rstack-size h!
  stack-base @ stack-end @ - over task-stack-size h!
  free-end @ task - next-ram-space - over task-dict-size !
  rstack-base @ rp@ - over task-rstack-offset h!
  stack-base @ sp@ - over task-stack-offset h!
  here next-ram-space - over task-dict-offset !
  1 over task-active !
  false over task-wait !
  0 over task-systick-start !
  -1 over task-systick-delay !
  dup dup task-next !
  dup main-task !
  dup start-task !
  current-task !
  free-end @ task - free-end !
;

\ Task entry point
: task-entry ( -- )
  r> drop
  try
  ?dup if execute then
  current-task @ force-disable-task
  begin
    pause
  again
;

\ Dump information on a task
: dump-task-info ( task -- )
  cr ." task-rstack-size:    " dup task-rstack-size h@ .
  cr ." task-stack-size:     " dup task-stack-size h@ .
  cr ." task-dict-size:      " dup task-dict-size @ .
  cr ." task-rstack-offset:  " dup task-rstack-offset h@ .
  cr ." task-stack-offset:   " dup task-stack-offset h@ .
  cr ." task-dict-offset:    " dup task-dict-offset @ .
  cr ." task-rstack-end:     " dup task-rstack-end .
  cr ." task-stack-end:      " dup task-stack-end .
  cr ." task-dict-end:       " dup task-dict-end .
  cr ." task-rstack-base:    " dup task-rstack-base .
  cr ." task-stack-base:     " dup task-stack-base .
  cr ." task-dict-base:      " dup task-dict-base .
  cr ." task-rstack-current: " dup task-rstack-current .
  cr ." task-stack-current:  " dup task-stack-current .
  cr ." task-dict-current:   " task-dict-current .
;
  
\ Spawn a non-main task
: spawn ( xt dict-size stack-size rstack-size -- task )
  2dup + task +
  free-end @ swap -
  tuck task-rstack-size h!
  tuck task-stack-size h!
  tuck task-dict-size !
  0 over task-handler !
  0 over task-active !
  false over task-wait !
  0 over task-systick-start !
  -1 over task-systick-delay !
  current-task @ task-next @ over task-next !
  dup current-task @ task-next !
  dup dup task-dict-size @ - free-end !
  0 over task-rstack-offset h!
  0 over task-stack-offset h!
  0 over task-dict-offset !
  ['] task-entry 1+ over push-task-rstack
  tuck push-task-stack
;

\ Go to the next task
: go-to-next-task ( task -- task )
  task-next @
  begin
    dup task-active @ 1 <
    over task-wait @ or
    over task-systick-delay @ -1 <>
    systick-counter 3 pick task-systick-start @ -
    3 pick task-systick-delay @ u< and or
    over start-task @ <> and
  while
    task-next @
  repeat
;

\ Reset waits
: reset-waiting-tasks ( -- )
  main-task @ begin
    false over task-wait !
    task-next @
    dup main-task @ =
  until
  drop
;

\ Handle all tasks are waiting
: handle-waiting-tasks ( task -- )
  dup start-task @ = if
    dup task-active @ 1 <
    over task-wait @ or
    over task-systick-delay @ -1 <>
    systick-counter 3 pick task-systick-start @ -
    3 roll task-systick-delay @ u< and or
    if
      sleep
      reset-waiting-tasks
    then
  else
    drop
  then
;

\ Handle PAUSE
: do-pause ( -- )
  pause-count @ 1+ pause-count !
  task-io
  current-task @
  rp@ over task-rstack-current!
  >r sp@ r> tuck task-stack-current!
  here over task-dict-current!
  handler @ over task-handler !
  dup start-task !
  go-to-next-task
  dup handle-waiting-tasks
  dup current-task !
  dup task-handler @ handler !
  dup task-stack-base stack-base !
  dup task-stack-end stack-end !
  dup task-rstack-base rstack-base !
  dup task-rstack-end rstack-end !
  dup task-rstack-current rp!
  dup task-dict-current here!
  task-stack-current sp!
;

\ Start a delay from the present
: start-task-delay ( 1/10m-delay task -- )
  dup systick-counter swap task-systick-start !
  task-systick-delay !
;

\ Set a delay for a task
: set-task-delay ( 1/10ms-delay 1/10ms-start task -- )
  tuck task-systick-start !
  task-systick-delay !
;

\ Advance a delay for a task by a given amount of time
: advance-task-delay ( 1/10ms-offset task -- )
  systick-counter over task-systick-start @ - over task-systick-delay @ < if
    task-systick-delay +!
  else
    dup task-systick-delay @ over task-systick-start +!
    task-systick-delay !
  then
;

\ Advance of start a delay from the present, depending on whether the delay
\ length has changed
: reset-task-delay ( 1/10ms-delay task -- )
  dup task-systick-delay @ 2 pick = if
    advance-task-delay
  else
    start-task-delay
  then
;

\ Get a delay for a task
: get-task-delay ( task -- 1/10ms-delay 1/10ms-start )
  dup task-systick-delay @
  over task-systick-start @
;

\ Cancel a delay for a task
: cancel-task-delay ( task -- )
  0 over task-systick-start !
  -1 swap task-systick-delay !
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

\ Wait the current thread
: do-wait ( -- )
  current-task @ wait-task
;
  
\ Init
: init ( -- )
  init
  stack-end @ free-end !
  init-main-task
  0 pause-count !
  ['] do-pause pause-hook !
  ['] do-wait wait-hook !
  1 pause-enabled !
;

\ Make current-task read-only
: current-task ( -- task ) current-task @ ;

\ Make main-task read-only
: main-task ( -- task ) main-task @ ;

\ Reboot to initialize multitasking
reboot
