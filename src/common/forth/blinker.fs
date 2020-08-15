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

\ Set up the wordlist order
forth-wordlist task-wordlist led-wordlist 3 set-order
forth-wordlist set-current

\ The blinker delay time
variable blinker-delay

\ The blinker
: blinker ( -- )
  led-red-on
  begin
    pause
    blinker-delay @ ms
    led-red-off
    led-green-on
    pause
    blinker-delay @ ms
    led-green-off
    led-red-on
  again
;

\ The blinker task
variable blinker-task

\ Init
: init ( -- )
  init
  500 blinker-delay !
  ['] blinker 256 256 256 spawn blinker-task !
  blinker-task @ enable-task
;

\ Reboot to initialize
reboot
