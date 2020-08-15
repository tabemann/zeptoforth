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
forth-wordlist internal-wordlist 2 set-order
internal-wordlist set-current

\ Begin compressing compiled code in flash
compress-flash

\ Pad flash to a 2048 byte boundary
: pad-flash-erase-block ( -- )
  begin flash-here $7FF and while
    0 bflash,
  repeat
;

\ Restore flash to a preexisting state
: restore-flash ( flash-here -- )
  erase-after rdrop
;

\ Set forth
forth-wordlist set-current

\ Commit flash
commit-flash

\ Create a MARKER to erase flash/return the flash dictionary to its prior state
: marker ( "name" -- )
  compiling-to-flash?
  token
  dup 0= if ['] token-expected ?raise then
  compile-to-flash
  pad-flash-erase-block
  flash-here
  rot rot
  start-compile
  lit,
  ['] restore-flash compile,
  visible
  end-compile,
  not if
    compile-to-ram
  then
;

\ Set internal
internal-wordlist set-current

\ Core of CORNERSTONE's DOES>
: cornerstone-does> ( -- )
  does>
  $800 align
  erase-after
;

\ Set forth
forth-wordlist set-current

\ Committing code in flash
commit-flash

\ Adapted from Terry Porter's code; not sure what license it was under
: cornerstone ( "name" -- )
  compiling-to-flash?
  compile-to-flash
  <builds
  pad-flash-erase-block
  cornerstone-does>
  not if
    compile-to-ram
  then
;

\ Ending compiling code in flash
end-compress-flash

\ Warm reboot
warm
