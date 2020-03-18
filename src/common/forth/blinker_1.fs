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

\ The schedule
variable my-schedule

\ The schedule task
variable schedule-task

\ The blinker action
variable blinker-action

\ The LED state variable
variable led-state

\ Do the blinker
: blinker ( -- )
  pause-count @ 10000 mod 2 < if
    led-state @ if
      led-red-off
    else
      led-red-on
    then
    led-state @ not led-state !
  then
;

\ Run the schedule
: do-schedule ( -- )
  my-schedule @ run-schedule
;

\ Init
: init ( -- )
  init
  false led-state !
  create-schedule my-schedule !
  ['] blinker my-schedule @ add-action blinker-action !
  blinker-action @ enable-action
  ['] do-schedule 256 256 256 spawn schedule-task !
  schedule-task @ enable-task
  pause
;

\ Reboot
reboot
