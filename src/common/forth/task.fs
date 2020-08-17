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

\ Set up the wordlist
forth-wordlist 1 set-order
forth-wordlist set-current
wordlist constant task-wordlist
wordlist constant task-internal-wordlist
forth-wordlist internal-wordlist systick-wordlist int-io-wordlist
task-wordlist task-internal-wordlist 6 set-order
task-internal-wordlist set-current

\ Main task
variable main-task

\ Current task
variable current-task

\ Declare a RAM variable for the end of free RAM memory
variable free-end

\ Starting task for a pause
variable start-task

\ Pause count
variable pause-count

\ The current task handler
user task-handler

\ Whether a task is waiting
user task-wait

\ Task systick start time
user task-systick-start

\ Task systick delay time
user task-systick-delay

\ The current base for a task
user task-base

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

  \ Task active state ( > 0 active, <= 0 inactive )
  field: task-active

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

\ Access a given user variable for a given task
: for-task ( task xt -- addr )
  execute dict-base @ - swap task-dict-base +
;

\ Link a task into the main loop
: link-task ( task -- )
  current-task @ task-next @ over task-next !
  current-task @ task-next !
;

\ Find the previous task
: prev-task ( task1 -- task2 )
  dup
  begin dup task-next @ 2 pick <> while task-next @ repeat
  tuck = if drop 0 then
;

\ Unlink a task from the main loop
: unlink-task ( task -- )
  dup prev-task ?dup if
    over current-task @ = if
      over task-next @ current-task !
    then
    swap task-next @ swap task-next !
  else
    0 current-task !
  then
;

\ Set non-internal
task-wordlist set-current

\ Enable a task
: enable-task ( task -- )
  dup task-active @ 1+
  dup 1 = if over link-task then
  swap task-active ! ;

\ Disable a task
: disable-task ( task -- )
  dup task-active @ 1-
  dup 0 = if over unlink-task then
  swap task-active ! ;

\ Force-enable a task
: force-enable-task ( task -- )
  dup task-active @ 1 < if
    dup link-task
    1 swap task-active !
  else
    drop
  then
;

\ Force-disable a task
: force-disable-task ( task -- )
  dup task-active @ 0> if
    dup unlink-task
    0 swap task-active !
  else
    drop
  then
;

\ Mark a task as waiting
: wait-task ( task -- )
  true swap ['] task-wait for-task !
;

\ Set internal
task-internal-wordlist set-current

\ Initialize the main task
: init-main-task ( -- )
  free-end @ task -
  rstack-base @ rstack-end @ - over task-rstack-size h!
  stack-base @ stack-end @ - over task-stack-size h!
  free-end @ task - next-ram-space - over task-dict-size !
  rstack-base @ rp@ - over task-rstack-offset h!
  stack-base @ sp@ - over task-stack-offset h!
  ram-here next-ram-space - over task-dict-offset !
  1 over task-active !
  false task-wait !
  0 task-systick-start !
  -1 task-systick-delay !
  base @ task-base !
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

\ Set non-internal
task-wordlist set-current

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
  0 over ['] task-handler for-task !
  0 over task-active !
  base @ over ['] task-base for-task !
  false over ['] task-wait for-task !
  0 over ['] task-systick-start for-task !
  -1 over ['] task-systick-delay for-task !
  dup dup task-dict-size @ - free-end !
  0 over task-rstack-offset h!
  0 over task-stack-offset h!
  next-user-space over task-dict-offset !
  0 over task-next !
  ['] task-entry 1+ over push-task-rstack
  tuck push-task-stack
;

\ Set internal
task-internal-wordlist set-current

\ Go to the next task
: go-to-next-task ( task -- task )
  task-next @
  begin
    dup ['] task-wait for-task @
    over ['] task-systick-delay for-task @ -1 <>
    systick-counter 3 pick ['] task-systick-start for-task @ -
    3 pick ['] task-systick-delay for-task @ u< and or
    over start-task @ <> and
  while
    task-next @
  repeat
;

\ Reset waits
: reset-waiting-tasks ( -- )
  main-task @ begin
    false over ['] task-wait for-task !
    task-next @
    dup main-task @ =
  until
  drop
;

\ Handle all tasks are waiting
: handle-waiting-tasks ( task -- )
  dup start-task @ = if
    dup ['] task-wait for-task @
    over ['] task-systick-delay for-task @ -1 <>
    systick-counter 3 pick ['] task-systick-start for-task @ -
    3 roll ['] task-systick-delay for-task @ u< and or
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
  1 pause-count +!
  begin
    task-io
    current-task @ 0<>
    dup not if
      sleep
    then
  until
  current-task @
  rp@ over task-rstack-current!
  >r sp@ r> tuck task-stack-current!
  ram-here over task-dict-current!
  handler @ task-handler !
  base @ task-base !
  dup start-task !
  go-to-next-task
  dup handle-waiting-tasks
  dup current-task !
  task-base @ base !
  task-handler @ handler !
  dup task-stack-base stack-base !
  dup task-stack-end stack-end !
  dup task-rstack-base rstack-base !
  dup task-rstack-end rstack-end !
  dup task-rstack-current rp!
  dup task-dict-current ram-here!
  dup task-dict-base dict-base !
  task-stack-current sp!
;

\ Set non-internal
task-wordlist set-current

\ Start a delay from the present
: start-task-delay ( 1/10m-delay task -- )
  dup systick-counter swap ['] task-systick-start for-task !
  ['] task-systick-delay for-task !
;

\ Set a delay for a task
: set-task-delay ( 1/10ms-delay 1/10ms-start task -- )
  tuck ['] task-systick-start for-task !
  ['] task-systick-delay for-task !
;

\ Advance a delay for a task by a given amount of time
: advance-task-delay ( 1/10ms-offset task -- )
  systick-counter over ['] task-systick-start for-task @ -
  over ['] task-systick-delay for-task @ < if
    ['] task-systick-delay for-task +!
  else
    dup ['] task-systick-delay for-task @
    over ['] task-systick-start for-task +!
    ['] task-systick-delay for-task !
  then
;

\ Advance of start a delay from the present, depending on whether the delay
\ length has changed
: reset-task-delay ( 1/10ms-delay task -- )
  dup ['] task-systick-delay for-task @ 2 pick = if
    advance-task-delay
  else
    start-task-delay
  then
;

\ Get a delay for a task
: get-task-delay ( task -- 1/10ms-delay 1/10ms-start )
  dup ['] task-systick-delay for-task @
  over ['] task-systick-start for-task @
;

\ Cancel a delay for a task
: cancel-task-delay ( task -- )
  0 over ['] task-systick-start for-task !
  -1 swap ['] task-systick-delay for-task !
;

\ Set forth
forth-wordlist set-current

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

\ Set internal
task-internal-wordlist set-current

\ Wait the current thread
: do-wait ( -- )
  current-task @ wait-task
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

\ Set non-internal
task-wordlist set-current

\ Make pause-count read-only
: pause-count ( -- u ) pause-count @ ;

\ Make current-task read-only
: current-task ( -- task ) current-task @ ;

\ Make main-task read-only
: main-task ( -- task ) main-task @ ;

\ Reset current wordlist
forth-wordlist set-current

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
  main-task task-stack-end free-end !
  init-main-task
  0 pause-count !
  ['] do-pause pause-hook !
  ['] do-wait wait-hook !
  false ram-dict-warned !
  ['] do-validate-dict validate-dict-hook !
  1 pause-enabled !
;

\ Display space free for a given task
: task-free ( task -- )
  cr
  ." dictionary free: "
  dup task-dict-end over task-dict-current - 0 <# #s #> type cr
  ." stack free:      "
  dup task-stack-current over task-stack-end - 0 <# #s #> type cr
  ." rstack free:     "
  dup task-rstack-current swap task-rstack-end - 0 <# #s #> type
;

\ Display space free for the main task and for flash in general
: free ( -- )
  cr main-task
  ." flash dictionary free:     "
  flash-end flash-here - 0 <# #s #> type cr
  ." main task dictionary free: "
  dup task-dict-end over task-dict-current - 0 <# #s #> type cr
  ." main task stack free:      "
  dup task-stack-current over task-stack-end - 0 <# #s #> type cr
  ." main task rstack free:     "
  dup task-rstack-current swap task-rstack-end - 0 <# #s #> type
;

\ Init
: init ( -- )
  init
  stack-end @ free-end !
  init-main-task
  0 pause-count !
  ['] do-pause pause-hook !
  ['] do-wait wait-hook !
  validate-dict-hook @ saved-validate-dict !
  false ram-dict-warned !
  ['] do-validate-dict validate-dict-hook !
  1 pause-enabled !
;

\ Reboot to initialize multitasking
warm
