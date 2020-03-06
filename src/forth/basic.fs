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

\ True constant
-1 constant true

\ False constant
0 constant false

\ TOS register
6 constant tos

\ Duplicate two cells
: 2dup ( x1 x2 -- x1 x2 x1 x2 ) over over [inlined] ;

\ Drop two cells
: 2drop ( x1 x2 -- ) drop drop [inlined] ;

\ Drop the cell under the top of the stack
: nip ( x1 x2 -- x2 ) swap drop [inlined] ;

\ Tuck a cell under the cell at he top of the stack
: tuck ( x1 x2 -- x2 x1 x2 ) swap over [inlined] ;

\ Fill memory with zeros up until a given address
: advance-here ( a -- )
  begin
    dup current-here >
  while
    0 bcurrent, 1 -
  repeat
  drop
;

\ Align an address to a power of two
: align ( a power -- a ) swap 1 - swap 1 - or 1 + ;

\ Dump the contents of the data stack
: .s ( -- )
  space ." ["
  sp@ begin
    dup stack-base @ <
  while
    dup @ .
    4 +
  repeat
  drop
  space ." ]"
;

\ Pad flash to a 2048 byte boundary
: pad-flash-erase-block
  begin flash-here $7FF and while
    0 bflash,
  repeat
;

\ Restore flash to a preexisting state
: restore-flash ( flash-here flash-latest -- )
  latest ram-latest <> if
    dup latest!
  then
  flash-latest!
  flash-here $7FF not and
  begin
    dup .
    dup erase-page $800 - 2dup >
  until
  drop
  flash-here!
  r> drop
  .s
;

\ Create a MARKER to erase flash/return the flash dictionary to its prior state
: marker ( "name" -- )
  compiling-to-flash
  token
  dup 0 = if ['] token-expected ?raise then
  compile-to-flash
  pad-flash-erase-block
  flash-here
  rot rot
  start-compile
  6 push,
  6 literal,
  flash-latest
  6 push,
  6 literal,
  ['] restore-flash call,
  visible
  end-compile,
  not if
    compile-to-ram
  then
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
  visible
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
  visible
  inlined
  finalize,
  advance-here
;

\ No word is being built exception
: no-word-being-built ( -- ) space ." no word is being built" abort ;

\ Specify code for a word created wth <BUILDS
: does> ( -- )
  space ." a"
  build-target @ 0 = if ['] no-word-being-built ?raise then
  space ." b"
  r>
  space ." c"
  1 + ( due to the nature of bx )
  space ." d"
  0 build-target @ literal!
  space ." e"
  0 build-target !
  space ." f"
;

\ Set compilation back to RAM
compile-to-ram
