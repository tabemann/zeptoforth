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

\ The red LED blink on action
variable red-blink-on-action

\ The orange LED blink on action
variable orange-blink-on-action

\ The green LED blink on action
variable green-blink-on-action

\ The blue LED blink on action
variable blue-blink-on-action

\ The red LED blink off action
variable red-blink-off-action

\ The orange LED blink off action
variable orange-blink-off-action

\ The green LED blink off action
variable green-blink-off-action

\ The blue LED blink off action
variable blue-blink-off-action

\ Delay
variable blink-delay

\ Create a blink word
: blink ( xt "name" -- )
  <builds current, does>
  @ execute blink-delay @ current-action reset-action-delay
;

\ Create the blink words
' led-red-on blink red-blink-on
' led-orange-on blink orange-blink-on
' led-green-on blink green-blink-on
' led-blue-on blink blue-blink-on
' led-red-off blink red-blink-off
' led-orange-off blink orange-blink-off
' led-green-off blink green-blink-off
' led-blue-off blink blue-blink-off

\ Run the schedule
: do-schedule ( -- )
  my-schedule @ run-schedule
;

\ Adjust blink timing
: reset-blinking ( -- )
  0 red-blink-on-action @ start-action-delay
  blink-delay @ 4/ orange-blink-on-action @ start-action-delay
  blink-delay @ 2/ green-blink-on-action @ start-action-delay
  blink-delay @ 4/ 3 * blue-blink-on-action @ start-action-delay
  blink-delay @ 4/ red-blink-off-action @ start-action-delay
  blink-delay @ 2/ orange-blink-off-action @ start-action-delay
  blink-delay @ 4/ 3 * green-blink-off-action @ start-action-delay
  blink-delay @ blue-blink-off-action @ start-action-delay
;

\ Init
: init ( -- )
  init
  4000 blink-delay !
  create-schedule my-schedule !
  ['] red-blink-on my-schedule @ add-action red-blink-on-action !
  ['] orange-blink-on my-schedule @ add-action orange-blink-on-action !
  ['] green-blink-on my-schedule @ add-action green-blink-on-action !
  ['] blue-blink-on my-schedule @ add-action blue-blink-on-action !
  ['] red-blink-off my-schedule @ add-action red-blink-off-action !
  ['] orange-blink-off my-schedule @ add-action orange-blink-off-action !
  ['] green-blink-off my-schedule @ add-action green-blink-off-action !
  ['] blue-blink-off my-schedule @ add-action blue-blink-off-action !
  reset-blinking
  red-blink-on-action @ enable-action
  orange-blink-on-action @ enable-action
  green-blink-on-action @ enable-action
  blue-blink-on-action @ enable-action
  red-blink-off-action @ enable-action
  orange-blink-off-action @ enable-action
  green-blink-off-action @ enable-action
  blue-blink-off-action @ enable-action
  ['] do-schedule 256 256 256 spawn schedule-task !
  schedule-task @ enable-task
  pause
;

\ Reboot
reboot
