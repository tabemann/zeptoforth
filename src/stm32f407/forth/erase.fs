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

\ Begin compressing compiled code in flash
compress-flash

\ Test for next flash sector
: check-flash-sector ( addr start end -- addr )
  dup >r flash-here >= swap flash-here <= and if drop r> 1+ else rdrop then
;

\ Get the start of the next flash sector
: next-flash-sector ( -- addr )
  $100000
  $000000 $003FFF check-flash-sector
  $004000 $007FFF check-flash-sector
  $008000 $00BFFF check-flash-sector
  $00C000 $00FFFF check-flash-sector
  $010000 $01FFFF check-flash-sector
  $020000 $03FFFF check-flash-sector
  $040000 $05FFFF check-flash-sector
  $060000 $07FFFF check-flash-sector
  $080000 $08FFFF check-flash-sector
  $0A0000 $0BFFFF check-flash-sector
  $0C0000 $0DFFFF check-flash-sector
  $0E0000 $0FFFFF check-flash-sector
;

\ Test for next flash sector
: check-flash-sector-for-addr ( flash-addr addr start end -- addr )
  dup >r 3 pick >= swap 3 roll <= and if drop r> 1+ else rdrop then
;

\ Align an address for a following flash sector
: erase-align ( addr -- addr )
  >r $100000
  r@ swap $000000 $003FFF check-flash-sector-for-addr
  r@ swap $004000 $007FFF check-flash-sector-for-addr
  r@ swap $008000 $00BFFF check-flash-sector-for-addr
  r@ swap $00C000 $00FFFF check-flash-sector-for-addr
  r@ swap $010000 $01FFFF check-flash-sector-for-addr
  r@ swap $020000 $03FFFF check-flash-sector-for-addr
  r@ swap $040000 $05FFFF check-flash-sector-for-addr
  r@ swap $060000 $07FFFF check-flash-sector-for-addr
  r@ swap $080000 $08FFFF check-flash-sector-for-addr
  r@ swap $0A0000 $0BFFFF check-flash-sector-for-addr
  r@ swap $0C0000 $0DFFFF check-flash-sector-for-addr
  r> swap $0E0000 $0FFFFF check-flash-sector-for-addr
;

\ Pad flash to a sector boundary
: pad-flash-erase-block ( -- )
  next-flash-sector
  begin flash-here over < while
    0 bflash,
  repeat
  drop
;

\ Restore flash to a preexisting state
: restore-flash ( flash-here -- )
  erase-after rdrop
;

\ Commit flash
commit-flash

\ Create a MARKER to erase flash/return the flash dictionary to its prior state
: marker ( "name" -- )
  compiling-to-flash
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

\ Core of CORNERSTONE's DOES>
: cornerstone-does> ( -- )
  does>
  erase-align
  erase-after
;

\ Committing code in flash
commit-flash

\ Adapted from Terry Porter's code; not sure what license it was under
: cornerstone ( "name" -- )
  compiling-to-flash
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
compile-to-ram
