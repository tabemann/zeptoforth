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

\ The scheduler structure
begin-structure schedule
  \ Current action
  field: schedule-current
end-structure

\ The action structure
begin-structure action
  \ The action xt
  field: action-xt

  \ Next action
  field: action-next

  \ Whether the action is active (> 0 means active)
  field: action-active
end-structure

\ Create a scheduler
: create-schedule ( -- schedule )
  here schedule allot
  0 over schedule-current !
;

\ Add an action to a scheduler
: add-action ( xt schedule -- action )
  here action allot
  2 roll over action-xt !
  0 over action-active !
  over schedule-current @ 0 <> if
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
  dup action-active @ 1 + swap action-active !
;

\ Disable an action
: disable-action ( action -- )
  dup action-active @ 1 - swap action-active !
;

\ Force-enable an action
: force-enable-action ( action -- )
  1 swap action-active !
;

\ Force-disable an action
: force-disable-action ( action -- )
  -1 swap action-active !
;

\ Run a schedule
: run-schedule ( schedule -- )
  begin
    pause
    dup schedule-current @
    dup action-active @ 0 > if
      dup action-xt @ execute
      action-next @ over schedule-current !
    then
  again
;

\ Reboot
reboot