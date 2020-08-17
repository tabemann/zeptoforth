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
wordlist constant schedule-wordlist
wordlist constant schedule-internal-wordlist
forth-wordlist systick-wordlist task-wordlist schedule-wordlist
schedule-internal-wordlist 5 set-order
schedule-internal-wordlist set-current

\ The current action
variable current-action

\ The scheduler structure
begin-structure schedule
  \ Current action
  field: schedule-current

  \ Last action executed
  field: schedule-last
end-structure

\ The action structure
begin-structure action
  \ The action xt
  field: action-xt

  \ Next action
  field: action-next

  \ Whether the action is active (> 0 means active)
  field: action-active

  \ Action systick start time
  field: action-systick-start
  
  \ Action systick delay time
  field: action-systick-delay
end-structure

\ Set non-internal
schedule-wordlist set-current

\ Create a scheduler
: create-schedule ( -- schedule )
  ram-here schedule ram-allot
  0 over schedule-current !
  0 over schedule-last !
;

\ Add an action to a scheduler
: add-action ( xt schedule -- action )
  ram-here action ram-allot
  2 roll over action-xt !
  0 over action-active !
  0 over action-systick-start !
  -1 over action-systick-delay !
  over schedule-last @ 0= if
    2dup swap schedule-last !
  then
  over schedule-current @ 0<> if
    over schedule-current @ action-next @
    over action-next !
    tuck swap schedule-current @ action-next !
  else
    dup dup action-next !
    tuck swap schedule-current !
  then
;

\ Enable an action
: enable-action ( action -- )
  dup action-active @ 1+ swap action-active !
;

\ Disable an action
: disable-action ( action -- )
  dup action-active @ 1- swap action-active !
;

\ Force-enable an action
: force-enable-action ( action -- )
  dup action-active @ 1 < if
    1 swap action-active !
  else
    drop
  then
;

\ Force-disable an action
: force-disable-action ( action -- )
  dup action-active @ 0> if
    0 swap action-active !
  else
    drop
  then
;

\ Start a delay from the present
: start-action-delay ( 1/10m-delay action -- )
  dup systick-counter swap action-systick-start !
  action-systick-delay !
;

\ Set a delay for an action
: set-action-delay ( 1/10ms-delay 1/10ms-start action -- )
  tuck action-systick-start !
  action-systick-delay !
;

\ Advance a delay for an action by a given amount of time
: advance-action-delay ( 1/10ms-offset action -- )
  systick-counter over action-systick-start @ - over
  action-systick-delay @ < if
    action-systick-delay +!
  else
    dup action-systick-delay @ over action-systick-start +!
    action-systick-delay !
  then
;

\ Advance of start a delay from the present, depending on whether the delay
\ length has changed
: reset-action-delay ( 1/10ms-delay action -- )
  dup action-systick-delay @ 2 pick = if
    advance-action-delay
  else
    start-action-delay
  then
;

\ Get a delay for an action
: get-action-delay ( action -- 1/10ms-delay 1/10ms-start )
  dup action-systick-delay @
  over action-systick-start @
;

\ Cancel a delay for an action
: cancel-action-delay ( action -- )
  0 over action-systick-start !
  -1 swap action-systick-delay !
;

\ Run a schedule
: run-schedule ( schedule -- )
  begin
    pause
    dup schedule-current @
    dup action-active @ 0>
    over action-systick-delay @ -1 =
    systick-counter 3 pick action-systick-start @ -
    3 pick action-systick-delay @ u>= or and
    if
      dup current-action !
      dup action-xt @ try ?dup if execute then
      2dup swap schedule-last !
    else
      2dup swap schedule-last @ = if
	wait-hook @ ?execute
      then
    then
    action-next @ over schedule-current !
  again
;

\ Make current-action read-only
: current-action ( -- action ) current-action @ ;

\ Reboot
warm