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

\ TOS register
6 constant tos

\ Duplicate two cells
: 2dup ( x1 x2 -- x1 x2 x1 x2 ) over over ;

\ Drop the cell under the top of the stack
: nip ( x1 x2 -- x2 ) swap drop ;

\ Tuck a cell under the cell at he top of the stack
: tuck ( x1 x2 -- x2 x1 x2 ) swap over ;

\ Fill memory with zeros up until a given address
: advance-here ( a -- )
  begin
    dup current-here >
  while
    0 bcurrent,
  repeat
;

\ Align an address to a power of two
: align ( a power -- a )
  2dup 1 - not and rot tuck <> if swap 1 - or 1 + else tuck then
;

\ In all cases:
\
\ 4 bytes for push,
\ up to 8 bytes for literal,
\ 2 bytes for bx lr
\ 2 bytes for movs r7, r7
\
\ For flash only:
\
\ 4 bytes for prev pointer
\ 4 bytes for $DEADBEEF

\ Create a word referring to memory after it
: create ( "name" -- )
  token
  dup 0 = if ['] token-expected ?raise then
  start-compile-no-push
  compiling-to-flash if
    current-here 28 + ( 28 bytes ) 4 align
  else
    current-here 16 + ( 16 bytes ) 4 align
  then
  tos push,
  dup tos literal,
  14 bx,
  $003F hcurrent,
  inlined
  finalize,
  advance-here
;

\ Specify a buffer of a given size
: buffer: ( # "name" -- ) create allot ;

\ Create a one-byte variable
: bvariable ( "name" -- ) create 1 allot ;

\ Create a two-byte variable
: hvariable ( "name" -- ) create 2 allot ;

\ Create a four-byte variable
: variable ( "name" -- ) create 4 allot ;

\ Create an eight-byte variable
: 2variable ( "name" -- ) create 8 allot ;

\ In all cases:
\
\ 4 bytes for push,
\ up to 8 bytes for literal,
\ 8 bytes for deferred literal
\ 2 bytes for bx lr
\ 2 bytes for movs r7, r7
\
\ For flash only:
\
\ 4 bytes for prev pointer
\ 4 bytes for $DEADBEEF

\ Create a word that executes code specified by DOES>
: <builds ( "name" -- )
  token
  dup 0 = if ['] token-expected ?raise then
  start-compile-no-push
  compiling-to-flash if
    current-here 32 + ( 32 bytes ) 4 align
  else
    current-here 24 + ( 24 bytes ) 4 align
  then
  tos push,
  dup tos literal,
  reserve-literal build-target !
  0 bx,
  $003F hcurrent,
  inlined
  finalize,
  advance-here
;

\ No word is being built exception
: no-word-being-built ( -- ) space ." no word is being built" abort ;

\ Specify code for a word created wth <BUILDS
: does> ( -- )
  build-target @ 0 = if ['] no-word-being-built ?raise then
  r> 0 build-target @ literal!
;
