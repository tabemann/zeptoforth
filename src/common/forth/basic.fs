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

\ Base 2
: binary 2 base ! ;

\ Base 8
: octal 8 base ! ;

\ Base 10
: decimal 10 base ! ;

\ Base 16
: hex 16 base ! ;

\ Cell size
4 constant cell

\ Multiple cells size
: cells ( n -- n ) cell * [inlined] ;

\ TOS register
6 constant tos

\ Visible flag
1 constant visible-flag

\ Immediate flag
2 constant immediate-flag

\ Compile-only flag
4 constant compiled-flag

\ Inlined flag
8 constant inlined-flag

\ Duplicate two cells
: 2dup ( x1 x2 -- x1 x2 x1 x2 ) over over [inlined] ;

\ Drop two cells
: 2drop ( x1 x2 -- ) drop drop [inlined] ;

\ Drop the cell under the top of the stack
: nip ( x1 x2 -- x2 ) swap drop [inlined] ;

\ Tuck a cell under the cell at he top of the stack
: tuck ( x1 x2 -- x2 x1 x2 ) swap over [inlined] ;

\ Add to a cell
: +! ( x addr -- ) tuck @ + swap ! [inlined] ;

\ Get the minimum of two numbers
: min ( n1 n2 -- n3 ) over - dup 0 < and + ;

\ Get the maximum of two numbers
: max ( n1 n2 -- n3 ) 2dup > if drop else nip then ;

\ Fill memory with zeros up until a given address
: advance-here ( a -- )
  begin
    dup current-here >
  while
    0 bcurrent,
  repeat
  drop
;

\ Align an address to a power of two
: align ( a power -- a ) swap 1 - swap 1 - or 1 + ;

\ Duplicate a cell if it is non-zero
: ?dup ( x -- x | 0 ) dup 0 <> if dup then ;

\ Get the depth of the stack, not including the cell pushed onto it by this
\ word
: depth ( -- u ) stack-base @ sp@ - cell / 1 - ;

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

\ Display all the words in a dictionary
: words-dict ( dict -- )
  begin
    dup 0 <>
  while
    dup 8 + count space type
    4 + @
  repeat
  drop
;

\ Display all the words
: words ( -- ) ram-latest words-dict flash-latest words-dict ;

\ Set bits on a byte
: bbis! ( bits addr -- ) dup b@ rot or swap b! ;

\ Clear bits on a byte
: bbic! ( bits addr -- ) dup b@ rot not and swap b! ;

\ Set bits on a halfword
: hbis! ( bits addr -- ) dup h@ rot or swap h! ;

\ Clear bits on a halfword
: hbic! ( bits addr -- ) dup h@ rot not and swap h! ;

\ Set bits on a word
: bis! ( bits addr -- ) dup @ rot or swap ! ;

\ Clear bits on a word
: bic! ( bits addr -- ) dup @ rot not and swap ! ;

\ Safely type a string
: safe-type ( addr bytes -- )
  pause-enabled @
  0 pause-enabled !
  rot rot type
  pause-enabled !
;

\ Pad flash to a 2048 byte boundary
: pad-flash-erase-block
  begin flash-here $7FF and while
    0 bflash,
  repeat
;

\ Restore flash to a preexisting state
: restore-flash ( flash-here -- )
  erase-after r> drop
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
  lit,
  ['] restore-flash compile,
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
    current-here 28 + ( 28 bytes ) flash-block-size align
  else
    current-here 16 + ( 16 bytes ) 4 align
  then
  tos push,
  dup tos literal,
  14 bx,
  $003F hcurrent,
  visible
  inlined
  finalize-no-align,
  advance-here
;

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
: <builds-with-name ( addr bytes -- )
  start-compile
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
  finalize-no-align,
  advance-here
;

\ Create a word that executes code specified by DOES>
: <builds ( "name" -- )
  token
  dup 0 = if ['] token-expected ?raise then
  <builds-with-name
;

\ No word is being built exception
: no-word-being-built ( -- ) space ." no word is being built" abort ;

\ Align to flash block if compiling to flash
: flash-align,
  compiling-to-flash if flash-here flash-block-size align advance-here then
;

\ Specify code for a word created wth <BUILDS
: does> ( -- )
  flash-align,
  build-target @ 0 = if ['] no-word-being-built ?raise then
  r>
  0 build-target @ literal!
  0 build-target !
;

\ Begin declaring a structure
: begin-structure ( "name" -- offset )
  <builds current-here 0 4 current-allot flash-align, does> @
;

\ Finish declaring a structure
: end-structure ( offset -- ) swap current! ;

\ Create an arbitrary-sized field
: +field ( offset size "name" -- offset )
  <builds over current, flash-align, + does> @ +
;

\ Create a byte-sized field
: bfield: ( offset "name" -- offset )
  <builds dup current, flash-align, 1 + does> @ +
;

\ Create a halfword-sized field
: hfield: ( offset "name" -- offset )
  <builds 2 align dup current, flash-align, 2 + does> @ +
;

\ Create a cell-sized field
: field: ( offset "name" -- offset )
  <builds cell align dup current, flash-align, cell + does> @ +
;

\ Create a double cell-sized field
: 2field: ( offset "name" -- offset )
  <builds 2 cells align dup current, flash-align, 2 cells + does> @ +
;

\ Begin lambda
: [: ( -- )
  [immediate]
  [compile-only]
  reserve-branch
  $B500 hcurrent,
;

\ End lambda
: ;] ( -- )
  [immediate]
  [compile-only]
  $BD00 hcurrent,
  current-here over branch-back!
  4 + lit,
;

\ Core of CORNERSTONE's DOES>
: cornerstone-does> ( -- )
  does>
  $800 align
  erase-after
;

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

\ Look up next available RAM space
: next-ram-space ( -- addr )
  s" *RAM*" visible-flag flash-latest find-dict dup if
    >xt execute
  else
    0
  then
  sys-ram-dict-base +
;

\ Specify next available RAM space
: set-next-ram-space ( addr -- )
  sys-ram-dict-base -
  compiling-to-flash
  swap
  compile-to-flash
  s" *RAM*" constant-with-name
  not if
    compile-to-ram
  then
;

\ Allocate a byte variable in RAM
: ram-bvariable ( "name" -- )
  next-ram-space
  compiling-to-flash
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  1 +
  set-next-ram-space
;
\ Allocate a halfword variable in RAM
: ram-hvariable ( "name" -- )
  next-ram-space
  2 align
  compiling-to-flash
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  2 +
  set-next-ram-space
;

\ Allocate a variable in RAM
: ram-variable ( "name" -- )
  next-ram-space
  4 align
  compiling-to-flash
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  4 +
  set-next-ram-space
;

\ Allocate a doubleword variable in RAM
: ram-2variable ( "name" -- )
  next-ram-space
  4 align
  compiling-to-flash
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  8 +
  set-next-ram-space
;

\ Allocate a buffer in RAM
: ram-buffer: ( bytes "name" -- )
  next-ram-space
  compiling-to-flash
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  +
  set-next-ram-space
;

\ Allocate a cell-aligned buffer in RAM
: ram-aligned-buffer: ( bytes "name" -- )
  next-ram-space
  4 align
  compiling-to-flash
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  +
  set-next-ram-space
;

\ Specify a buffer of a given size
: buffer: ( # "name" -- )
  compiling-to-flash if
    ram-buffer:
  else
    create allot
  then
;

\ Specify a buffer of a given size
: aligned-buffer: ( # "name" -- )
  compiling-to-flash if
    ram-aligned-buffer:
  else
    create allot
  then
;

\ Create a one-byte variable
: bvariable ( "name" -- )
  compiling-to-flash if
    ram-bvariable
  else
    create 1 allot
  then
;

\ Create a two-byte variable
: hvariable ( "name" -- )
  compiling-to-flash if
    ram-hvariable
  else
    create 2 allot
  then
;

\ Create a four-byte variable
: variable ( "name" -- )
  compiling-to-flash if
    ram-variable
  else
    create 4 allot
  then
;

\ Create an eight-byte variable
: 2variable ( "name" -- )
  compiling-to-flash if
    ram-2variable
  else
    create 8 allot
  then
;

\ Begin a do loop
: do ( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )
  [immediate]
  [compile-only]
  6 push,
  reserve-literal
  postpone >r
  postpone >r
  postpone >r
  current-here
;

\ Begin a ?do loop
: ?do ( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )
  [immediate]
  [compile-only]
  6 push,
  reserve-literal
  postpone >r
  postpone 2dup
  postpone <>
  postpone if
  postpone >r
  postpone >r
  postpone else
  postpone 2drop
  postpone exit
  postpone then
  current-here
;

\ End a do loop
: loop ( R: leave current end -- leave current end | )
  [immediate]
  [compile-only]
  postpone r>
  postpone r>
  1 lit, postpone +
  postpone 2dup
  postpone =
  postpone swap
  postpone >r
  postpone swap
  postpone >r
  0 6 0 lsl-imm,
  6 pull,
  0 0 cmp-imm,
  0branch,
  postpone rdrop
  postpone rdrop
  postpone rdrop
  current-here 1 + 6 rot literal!
;

\ End a do +loop
: +loop ( increment -- ) ( R: leave current end -- leave current end | )
  [immediate]
  [compile-only]
  postpone r>
  postpone r>
  postpone rot
  postpone dup
  0 lit,
  postpone >=
  postpone if
  postpone +
  postpone 2dup
  postpone <=
  postpone else
  postpone +
  postpone 2dup
  postpone >
  postpone then
  postpone swap
  postpone >r
  postpone swap
  postpone >r
  0 6 0 lsl-imm,
  6 pull,
  0 0 cmp-imm,
  0branch,
  postpone rdrop
  postpone rdrop
  postpone rdrop
  current-here 1 + 6 rot literal!
;

\ Get the loop index
: i ( R: current end -- current end ) ( -- current )
  [immediate]
  [compile-only]
  postpone r>
  postpone r>
  postpone dup
  postpone >r
  postpone swap
  postpone >r
;

\ Get the loop index beneath the current loop
: j ( R: cur1 end1 leave cur2 end2 -- cur1 end1 leave cur2 end2 ) ( -- cur1 )
  [immediate]
  [compile-only]
  postpone r>
  postpone r>
  postpone r>
  postpone r>
  postpone dup
  postpone >r
  postpone swap
  postpone >r
  postpone swap
  postpone >r
  postpone swap
  postpone >r
;

\ Leave a do loop
: leave ( R: leave current end -- )
  [immediate]
  [compile-only]
  postpone rdrop
  postpone rdrop
  postpone exit
;

\ Wait for a predicate to become true
: wait ( xt -- )
  begin
    dup execute not
  while
    pause
  repeat
  drop
;

\ Initialize the RAM variables
: init ( -- )
  init
  next-ram-space here!
;

\ Set compilation back to RAM
compile-to-ram
