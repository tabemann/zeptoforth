\ Copyright (c) 2013? Matthias Koch
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

\ Compile to flash
compile-to-flash

\ RAM variable for a character to emit
bvariable char-emit

\ RAM variable for a character waiting to be emitted
bvariable char-emit?

\ RAM variable for key buffer read-index
bvariable key-read-index

\ RAM variable for key buffer write-index
bvariable key-write-index

\ RAM variable for key buffer count
bvariable key-count

\ Constant for number of keys to buffer
16 constant key-buffer-size

\ Key buffer
key-buffer-size ram-buffer: key-buffer

\ USART2
$40004400 constant USART2_BASE
USART2_BASE constant CONSOLEL_BASE
CONSOLE_BASE $00 + constant CONSOLE_SR
CONSOLE_BASE $04 + constant CONSOLE_DR
$20 constant RXNE
$80 constant TXE

\ Write a key to the key buffer
: write-key ( c -- )
  key-count b@ key-buffer-size < if
    key-write-index b@ key-buffer + b!
    key-write-index b@ 1 + key-buffer-size mod key-write-index b!
    key-count b@ 1 + key-count b!
  else
    drop
  then
;

\ Read a key from the key buffer
: read-key ( -- c )
  key-count b@ 0 > if
    key-read-index b@ key-buffer + b@
    key-read-index b@ 1 + key-buffer-size mod key-read-index b!
    key-count b@ 1 - key-count b!
  else
    0
  then
;

\ Handle IO
: handle-io ( -- )
  begin
    key-count b@ key-buffer-size < if
      CONSOLE_SR @
      dup RXNE and if
	CONSOLE_DR b@ write-key false swap
      else
	true swap
      then
      TXE and if
	char-emit? b@ if
	  char-emit b@ CONSOLE_DR b!
	  false char-emit? b!
	then
      then
    else
      true
    then
  until
;

\ Multitasking IO hooks

: do-emit ( c -- )
  pause-enabled @ 0 > if
    [: char-emit? b@ not ;] wait
    char-emit b!
    true char-emit? b!
    pause
    [: char-emit? b@ not ;] wait
  else
    serial-emit
  then
; 

: do-key ( -- c )
  pause-enabled @ 0 > if
    [: key-count b@ 0 > ;] wait
    read-key
  else
    serial-key
  then
;

: do-emit? ( -- flag )
  pause-enabled @ 0 > if char-emit? b@ 0 <> else serial-emit? then
;

: do-key? ( -- flag )
  pause-enabled @ 0 > if key-count b@ 0 > else serial-key? then
;

\ Init
: init ( -- )
  init
  0 key-read-index b!
  0 key-write-index b!
  0 key-count b!
  0 char-emit b!
  0 char-emit? b!
  ['] do-key key-hook !
  ['] do-emit emit-hook !
  ['] do-key? key?-hook !
  ['] do-emit? emit?-hook !
;

\ Reboot
reboot
