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

compile-to-flash

marker erase-me

\ Wait for a counter
: wait-counter ( xt freq -- )
  2dup swap execute swap umod
  begin
    pause
    2 pick execute
    2 pick umod
    over <=
  until
  begin
    pause
    2 pick execute
    2 pick umod
    over >=
  until
  drop drop drop
;

\ The blinker delay time
variable blinker-delay

\ The blinker
: blinker ( -- )
  led-red-on
  begin
    pause
    [: pause-count @ ;] blinker-delay @ wait-counter
    led-red-off
    led-green-on
    pause
    [: pause-count @ ;] blinker-delay @ wait-counter
    led-green-off
    led-red-on
  again
;


\ The blinker task
variable blinker-task

\ Init
: init ( -- )
  init
  10000 blinker-delay !
  ['] blinker 256 256 256 spawn blinker-task !
  blinker-task @ enable-task
;

\ Reboot to initialize
reboot
