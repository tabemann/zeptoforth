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

\ The red LED blinker action
variable red-blinker-action

\ The green LED blinker action
variable green-blinker-action

\ The red LED state variable
variable red-led-state

\ The green LED state variable
variable green-led-state

\ The red LED blinker delay variable
variable red-blinker-delay

\ The green LED blinker delay variable
variable green-blinker-delay

\ Blink the red LED
: red-blinker ( -- )
  red-led-state @ if
    led-red-off
  else
    led-red-on
  then
  red-led-state @ not red-led-state !
  red-blinker-delay @ current-action @ reset-action-delay
;

\ Blink the green LED
: green-blinker ( -- )
  green-led-state @ if
    led-green-off
  else
    led-green-on
  then
  green-led-state @ not green-led-state !
  green-blinker-delay @ current-action @ reset-action-delay
;

\ Run the schedule
: do-schedule ( -- )
  my-schedule @ run-schedule
;

\ Init
: init ( -- )
  init
  false red-led-state !
  false green-led-state !
  create-schedule my-schedule !
  6666 red-blinker-delay !
  ['] red-blinker my-schedule @ add-action red-blinker-action !
  red-blinker-delay @ red-blinker-action @ start-action-delay
  red-blinker-action @ enable-action
  10000 green-blinker-delay !
  ['] green-blinker my-schedule @ add-action green-blinker-action !
  green-blinker-delay @ green-blinker-action @ start-action-delay
  green-blinker-action @ enable-action
  ['] do-schedule 256 256 256 spawn schedule-task !
  schedule-task @ enable-task
  pause
;

\ Reboot
reboot
