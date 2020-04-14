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
: cells ( n -- n ) 4* [inlined] ;

\ Add a cell to a value
: cell+ ( n -- n ) 4+ [inlined] ;

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
: +! ( x addr -- ) swap over @ + swap ! [inlined] ;

\ Get the minimum of two numbers
: min ( n1 n2 -- n3 ) over - dup 0< and + ;

\ Get the maximum of two numbers
: max ( n1 n2 -- n3 ) 2dup > if drop else swap drop then ;

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
: align ( a power -- a ) swap 1- swap 1- or 1+ ;

\ Duplicate a cell if it is non-zero
: ?dup ( x -- x | 0 ) dup 0<> if dup then ;

\ Get the depth of the stack, not including the cell pushed onto it by this
\ word
: depth ( -- u ) stack-base @ sp@ - cell / 1- ;

\ Dump the contents of the data stack
: .s ( -- )
  space ." ["
  sp@ begin
    dup stack-base @ <
  while
    dup @ .
    4+
  repeat
  drop
  space ." ]"
;

\ Assert that a value is true, otherwise raise a specified exception
: averts ( f "name" -- )
  [immediate]
  token-word
  >xt
  state @ if
    postpone 0=
    postpone if
    rot lit,
    postpone ?raise
    postpone then
  else
    swap 0= if
      ?raise
    else
      drop
    then
  then
;

\ Assert that a value is false, otherwise raise a specified exception
: triggers ( f "name" -- )
  [immediate]
  token-word
  >xt
  state @ if
    postpone 0<>
    postpone if
    rot lit,
    postpone ?raise
    postpone then
  else
    swap 0<> if
      ?raise
    else
      drop
    then
  then
;

\ Check whether an exception, typically returned by `try`, matches a specified
\ exception and if it does, replace it with zero, marking no exception
\ otherwise passing the specified argument through.
: suppress ( exc|0 "name" -- exc|0 )
  [immediate]
  token-word
  >xt
  state @ if
    postpone dup
    lit,
    postpone =
    postpone if
    postpone drop
    0 lit,
    postpone then
  else
    swap dup rot = if
      drop 0
    then
  then
;

\ Get the flags for a word
: word-flags ( word -- flags ) @ [inlined] ;

\ Get the previous word for a word
: prev-word ( word1 -- word2 ) 4+ @ [inlined] ;

\ Get the name of a word (a counted string)
: word-name ( word -- b-addr ) 8 + [inlined] ;

\ Get whether a word is hidden
: hidden? ( word -- f )
  dup word-flags visible-flag and if
    word-name count dup 2 > if
      over b@ [char] * = if
	+ 1- b@ [char] * =
      else
	2drop false
      then
    else
      2drop true
    then
  else
    drop false
  then
;

\ Actually print a string in one out of four columns, taking up more than one
\ column if necessary
: words-column ( b-addr bytes column1 -- column2 )
  over 0> if
    over 20 / + 1+
    dup 4 >= if
      drop 0
    then
    >r
    dup 0> if
      tuck type
      r@ 0<> if
	20 mod 20 swap - begin dup 0> while 1- space repeat drop
      else
	drop
      then
      r@ 0= if cr then
    else
      2drop
    then
    r>
  else
    rot rot 2drop
  then
;

\ Print a string in one out of four columns, taking up more than one column
\ if necessary
: words-column ( b-addr bytes column1 -- column2 )
  4 over - 20 * 2 pick < if
    cr drop 0 words-column
  else
    words-column
  then
;

\ Display all the words in a dictionary starting at a given column, and
\ returning the next column
: words-dict ( dict column1 -- column2 )
  begin
    over 0<>
  while
    over hidden? not if
      over word-name count rot words-column
    then
    swap prev-word swap
  repeat
  nip
;

\ Commit code to flash
commit-flash

\ Display all the words are four columns
: words ( -- )
  cr ram-latest 0 words-dict flash-latest swap words-dict drop cr
;

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
  rot rot serial-type
  pause-enabled !
;

\ Commit code to flash
commit-flash

\ Safely type an integer
: safe-type-integer ( n -- )
  here swap format-integer dup allot dup >r safe-type r> negate allot
;

\ Safely type an unsigned integer
: safe-type-unsigned ( n -- )
  here swap format-unsigned dup allot dup >r safe-type r> negate allot
;

\ Commit code to flash
commit-flash

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
  dup 0= triggers token-expected
  start-compile-no-push
  compiling-to-flash? if
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
  compiling-to-flash? if
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

\ Commit changes to flash
commit-flash

\ Create a word that executes code specified by DOES>
: <builds ( "name" -- )
  token
  dup 0= triggers token-expected
  <builds-with-name
;

\ No word is being built exception
: no-word-being-built ( -- ) space ." no word is being built" abort ;

\ Align to flash block if compiling to flash
: flash-align,
  compiling-to-flash? if flash-here flash-block-size align advance-here then
;

\ Commit changes to flash
commit-flash

\ Specify code for a word created wth <BUILDS
: does> ( -- )
  flash-align,
  build-target @ 0= if ['] no-word-being-built ?raise then
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
  <builds dup current, flash-align, 1+ does> @ +
;

\ Create a halfword-sized field
: hfield: ( offset "name" -- offset )
  <builds 2 align dup current, flash-align, 2+ does> @ +
;

\ Create a cell-sized field
: field: ( offset "name" -- offset )
  <builds cell align dup current, flash-align, cell + does> @ +
;

\ Create a double cell-sized field
: 2field: ( offset "name" -- offset )
  <builds 2 cells align dup current, flash-align, 2 cells + does> @ +
;

\ Get whether two strings are equal
: equal-strings? ( b-addr1 u1 b-addr2 u2 -- f )
  >r swap r@ = if
    begin
      r@ 0>
    while
      dup b@ 2 pick b@ = if
	1+ swap 1+ r> 1- >r
      else
	2drop rdrop false exit
      then
    repeat
    2drop rdrop true
  else
    2drop rdrop false
  then
;

\ Start a CASE statement
: case ( -- )
  [immediate]
  [compile-only]
  0
;

\ Start an OF clause
: of ( x -- )
  [immediate]
  [compile-only]
  postpone over
  postpone =
  postpone if
  postpone drop
;

\ End an OF clause
: endof ( -- )
  [immediate]
  [compile-only]
  rot ?dup if
    current-here swap branch-back!
  then
  reserve-branch
  rot rot postpone then
;

\ End a CASE statement
: endcase ( x -- )
  [immediate]
  [compile-only]
  postpone drop
  ?dup if
    here swap branch-back!
  then
;

\ Commit changes to flash
commit-flash

\ Start an OFSTR clause
: ofstr ( x -- )
  [immediate]
  [compile-only]
  3 lit,
  postpone pick
  3 lit,
  postpone pick
  postpone equal-strings?
  postpone if
  postpone 2drop
;

\ Start an OFSTRCASE clause
: ofstrcase ( x -- )
  [immediate]
  [compile-only]
  3 lit,
  postpone pick
  3 lit,
  postpone pick
  postpone equal-case-strings?
  postpone if
  postpone 2drop
;

\ End a CASE statement comparing against a string
: endcasestr ( x -- )
  [immediate]
  [compile-only]
  postpone 2drop
  ?dup if
    current-here swap branch-back!
  then
;

\ Output a hexadecimal nibble
: h.1 ( b -- )
  $F and dup 10 < if [char] 0 + else 10 - [char] A + then emit
;

\ Output a hexadecimal 8 bit value, padded with zeros
: h.2 ( b -- )
  $FF and dup 4 rshift h.1 h.1
;

\ Output a hexadecimal 16 bit value, padded with zeros
: h.4 ( h -- )
  $FFFF and dup 8 rshift h.2 h.2
;

\ Output a hexadecimal 32 bit value, padded with zeros
: h.8 ( x -- )
  dup 16 rshift h.4 h.4
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
  compiling-to-flash?
  swap
  compile-to-flash
  s" *RAM*" constant-with-name
  not if
    compile-to-ram
  then
;

\ Commit changes to flash
commit-flash

\ Allocate a byte variable in RAM
: ram-bvariable ( "name" -- )
  next-ram-space
  compiling-to-flash?
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  1+
  set-next-ram-space
;

\ Allocate a halfword variable in RAM
: ram-hvariable ( "name" -- )
  next-ram-space
  2 align
  compiling-to-flash?
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  2+
  set-next-ram-space
;

\ Allocate a variable in RAM
: ram-variable ( "name" -- )
  next-ram-space
  4 align
  compiling-to-flash?
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  4+
  set-next-ram-space
;

\ Allocate a doubleword variable in RAM
: ram-2variable ( "name" -- )
  next-ram-space
  4 align
  compiling-to-flash?
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
  compiling-to-flash?
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
  compiling-to-flash?
  over
  compile-to-flash
  constant
  not if
    compile-to-ram
  then
  +
  set-next-ram-space
;

\ Commit changes to flash
commit-flash

\ Specify a buffer of a given size
: buffer: ( # "name" -- )
  compiling-to-flash? if
    ram-buffer:
  else
    create allot
  then
;

\ Specify a buffer of a given size
: aligned-buffer: ( # "name" -- )
  compiling-to-flash? if
    ram-aligned-buffer:
  else
    create allot
  then
;

\ Create a one-byte variable
: bvariable ( "name" -- )
  compiling-to-flash? if
    ram-bvariable
  else
    create 1 allot
  then
;

\ Create a two-byte variable
: hvariable ( "name" -- )
  compiling-to-flash? if
    ram-hvariable
  else
    create 2 allot
  then
;

\ Create a four-byte variable
: variable ( "name" -- )
  compiling-to-flash? if
    ram-variable
  else
    create 4 allot
  then
;

\ Create an eight-byte variable
: 2variable ( "name" -- )
  compiling-to-flash? if
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
  current-here 1+ 6 rot literal!
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
  current-here 1+ 6 rot literal!
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
  postpone r>
  postpone dup
  postpone >r
  postpone swap
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

\ Unloop from a do loop (to exit, e.g.)
: unloop ( R: leave current end -- )
  [immediate]
  [compile-only]
  postpone rdrop
  postpone rdrop
  postpone rdrop
;

\ Commit to flash
commit-flash

\ Dump memory between two addresses
: dump ( start-addr end-addr -- )
  cr
  swap ?do
    i h.8
    16 0 ?do
      space j i + b@ h.2
    loop
    space [char] ' emit
    16 0 ?do
      j i + b@ dup $20 >= over $7F < and if emit else drop [char] . emit then
    loop
    [char] ' emit cr
  16 +loop
;

\ Skip characters in the evaluation buffer until a predicate is met
: skip-until ( xt -- )
  >r
  begin
    source >parse @ > if >parse @ + b@ r@ execute 1 >parse +! else drop true then
  until
  rdrop
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
  4+ lit,
;

\ Create a deferred word
: defer ( "name" -- )
  token
  dup 0= triggers token-expected
  start-compile-no-push
  visible
  compiling-to-flash? if
    flash-align,
  then
  reserve-literal drop
  0 bx,
  finalize,
;

\ Set a deferred word in RAM
: defer-ram! ( xt xt-deferred -- )
  swap 1+ swap
  0 swap
  compiling-to-flash? >r
  compile-to-ram
  literal!
  r> if
    compile-to-flash
  then
;

\ Set a deferred word in flash
: defer-flash! ( xt xt-deferred -- )
  flash-block-size align
  swap 1+ swap
  0 swap
  compiling-to-flash? >r
  compile-to-flash
  literal!
  r> not if
    compile-to-ram
  then
;

\ Set a deferred word; note that a deferred word stored in flash can only have
\ its implementation set once
: defer! ( xt xt-deferred -- )
  dup ram-base < if defer-flash! else defer-ram! then
;

\ Decode the immediate field from a MOVW or MOVT instruction
: decode-mov16 ( h-addr -- h )
  dup h@ dup $F and 1 lshift swap 10 rshift $1 and or 11 lshift
  swap 2+ h@ dup $FF and swap 4 rshift $700 and or or
;

\ Decode the immediate field from a pair of a MOVW instruction followed by a
\ MOVT instruction
: decode-literal ( h-addr -- x )
  dup decode-mov16 swap 4+ decode-mov16 16 lshift or
;

\ Get the referred xt from a deferred word in RAM
: defer-ram@ ( xt-deferred -- xt )
  decode-literal 1 -
;

\ Get the referred xt from a deferred word in flash
: defer-flash@ ( xt-deferred -- xt )
  flash-block-size align decode-literal 1 -
;

\ Get the referred xt from a deferred word
: defer@ ( xt-deferred -- xt )
  dup ram-base < if defer-flash@ else defer-ram@ then
;

\ s" constant
2 constant s"-length
create s"-data char s bcurrent, char " bcurrent,

\ ." constant
2 constant ."-length
create ."-data char c bcurrent, char " bcurrent,

\ c" constant
2 constant c"-length
create c"-data char c bcurrent, char " bcurrent,

\ Commit to flash
commit-flash

\ The constants themselves
s"-data s"-length 2constant s"-constant
."-data ."-length 2constant ."-constant
c"-data c"-length 2constant c"-constant

\ Commit to flash
commit-flash

\ Implement the [else] in [if]/[else]/[then] for conditional
\ execution/compilation
: [else] ( -- )
  [immediate]
  1 begin
    begin token dup while
      case
	s" [if]" ofstrcase 1+ endof
        s" [else]" ofstrcase 1- dup if 1+ then endof
        s" [then]" ofstrcase 1- endof
	s" \" ofstrcase ['] newline? skip-until	endof
	s" (" ofstrcase [: [char] ) = ;] skip-until endof
	s"-constant ofstrcase [: [char] " = ;] skip-until endof
	c"-constant ofstrcase [: [char] " = ;] skip-until endof
	."-constant ofstrcase [: [char] " = ;] skip-until endof
	s" .(" ofstrcase [: [char] ) = ;] skip-until endof
	s" char" ofstrcase state @ not if token 2drop then endof
	s" [char]" ofstrcase token 2drop endof
	s" '" ofstrcase state @ not if token 2drop then endof
	s" [']" ofstrcase token 2drop endof
	s" postpone" ofstrcase token 2drop endof
      endcasestr
      dup 0= if drop exit then
    repeat 2drop
    prompt-hook @ ?execute refill
  again
;

\ Start conditional execution/compilation
: [if] ( f -- ) 0= if postpone [else] then [immediate] ;

\ Finish conditional execution/compilation
: [then] ( -- ) [immediate] ;

\ Wait hook variable
variable wait-hook

\ Wait for a predicate to become true
: wait ( xt -- )
  begin
    dup execute not
  while
    wait-hook @ ?execute
    pause
  repeat
  drop
;

\ Initialize the RAM variables
: init ( -- )
  init
  next-ram-space here!
  0 wait-hook !
;

\ Finish compressing the code
end-compress-flash

\ Set compilation back to RAM
compile-to-ram
