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
forth-wordlist task-wordlist 2 set-order
forth-wordlist set-current

\ The blinker variation delay
variable vary-delay

\ The blinker variation minimum blinker delay
variable vary-min

\ The blinker variation maximum blinker delay
variable vary-max

\ The blinker variation blinker delay step
variable vary-step

\ The blinker variation routine
: vary ( -- )
  begin
    vary-min @ blinker-delay !
    begin
      blinker-delay @ vary-max @ <
    while
      vary-delay @ ms
      blinker-delay @ vary-step @ + blinker-delay !
    repeat
    vary-max @ blinker-delay !
    begin
      blinker-delay @ vary-min @ >
    while
      vary-delay @ ms
      blinker-delay @ vary-step @ - blinker-delay !
    repeat
  again
;

\ The blinker variation task
variable vary-task

\ Init
: init ( -- )
  init
  100 vary-delay !
  50 vary-min !
  500 vary-max !
  25 vary-step !
  ['] vary 256 256 256 spawn vary-task !
  vary-task @ enable-task
;

\ Reboot
reboot