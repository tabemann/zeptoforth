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

\ Forth wordlist constant
0 constant forth-wordlist

\ Internal wordlist constant
1 constant internal-wordlist

\ Commit to flash
commit-flash

\ Set up wordlist order
forth-wordlist internal-wordlist 2 set-order
forth-wordlist set-current

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

\ Add to a cell
\ : +! ( x addr -- ) swap over @ + swap ! [inlined] ;

\ Get the minimum of two numbers
: min ( n1 n2 -- n3 ) over - dup 0< and + ;

\ Get the maximum of two numbers
: max ( n1 n2 -- n3 ) 2dup > if drop else swap drop then ;

\ Rotate three cells in revese
: -rot ( x1 x2 x3 -- x3 x1 x2 ) rot rot [inlined] ;

\ Get the absolute value of a number
: abs ( n -- u ) dup 0< if negate then ;

\ Set internal
internal-wordlist set-current

\ Fill memory with zeros up until a given address
: advance-here ( a -- )
  begin
    dup here >
  while
    0 b,
  repeat
  drop
;

\ Set forth
forth-wordlist set-current

\ Align an address to a power of two
: align ( a power -- a ) swap 1- swap 1- or 1+ ;

\ Duplicate a cell if it is non-zero
: ?dup ( x -- x | 0 ) dup 0<> if dup then ;

\ Test for bits in a byte being set
: bbit@ ( mask b-addr -- f ) b@ and 0<> ;

\ Test for bits in a halfword being set
: hbit@ ( mask h-addr -- f ) h@ and 0<> ;

\ Test for bits in a cell being set
: bit@ ( mask addr -- f ) @ and 0<> ;

\ Set bits on a byte
: bbis! ( bits addr -- ) dup b@ rot or swap b! ;

\ Clear bits on a byte
: bbic! ( bits addr -- ) dup b@ rot bic swap b! ;

\ Set bits on a halfword
: hbis! ( bits addr -- ) dup h@ rot or swap h! ;

\ Clear bits on a halfword
: hbic! ( bits addr -- ) dup h@ rot bic swap h! ;

\ Set bits on a word
: bis! ( bits addr -- ) dup @ rot or swap ! ;

\ Clear bits on a word
: bic! ( bits addr -- ) dup @ rot bic swap ! ;

\ Get the depth of the stack, not including the cell pushed onto it by this
\ word
: depth ( -- u ) stack-base @ sp@ - cell / 1- ;

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

\ Output a hexadecimal 64 bit value, padded with zeros
: h.16 ( ud -- )
  h.8 h.8
;

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
  >body
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
  >body
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
  >body
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

\ Get whether a word is defined
: defined? ( "word" -- flag ) token visible-flag find-all 0<> ;

\ Set internal
internal-wordlist set-current

\ Get the address of the flags for a word
: word-flags ( word -- h-addr ) [inlined] ;

\ Get the address of the wordlist id for a word
: wordlist-id ( word -- h-addr ) 2+ [inlined] ;

\ Get the previous word for a word
: prev-word ( word1 -- addr ) 4+ [inlined] ;

\ Get the name of a word (a counted string)
: word-name ( word -- b-addr ) 8 + [inlined] ;

\ Get the length of a common prefix to two strings
: common-prefix ( b-addr1 bytes1 b-addr2 bytes2 -- bytes3 )
  0 begin
    3 pick 0> 2 pick 0> and if
      4 pick b@ 3 pick b@ = if
	4 roll 1+ 4 roll 1- 4 roll 1+ 4 roll 1- 4 roll 1+ false
      else
	nip nip nip nip true
      then
    else
      nip nip nip nip true
    then
  until
;

\ Get whether a string has a prefix (the prefix is the first string)
: prefix? ( b-addr1 bytes1 b-addr2 bytes2 -- flag )
  begin
    2 pick 0> if
      dup 0> if
	3 pick b@ 2 pick b@ = if
	  3 roll 1+ 3 roll 1- 3 roll 1+ 3 roll 1- false
	else
	  2drop 2drop false true
	then
      else
	2drop 2drop false true
      then
    else
      2drop 2drop true true
    then
  until
;  

\ Commit to flash
commit-flash

\ Get whether a word is hidden
: hidden? ( word -- f )
  dup word-flags h@ visible-flag and if
    word-name count dup 2 > if
      over b@ [char] * = if
	+ 1- b@ [char] * =
      else
	2drop false
      then
    else
      2drop false
    then
  else
    drop true
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
    -rot 2drop
  then
;

\ Print a string in one out of four columns, taking up more than one column
\ if necessary
: words-column-wrap ( b-addr bytes column1 -- column2 )
  4 over - 20 * 2 pick < if
    cr drop 0 words-column
  else
    words-column
  then
;

\ Commit to flash
commit-flash

\ Display all the words in a dictionary starting at a given column, and
\ returning the next column
: words-dict ( dict wid column1 -- column2 )
  swap >r
  begin
    over 0<>
  while
    over hidden? not 2 pick wordlist-id h@ r@ = and if
      over word-name count rot words-column-wrap
    then
    swap prev-word @ swap
  repeat
  nip rdrop
;

\ Display all the words in a dictionary starting at a given column and returning
\ the next column
: lookup-dict ( b-addr bytes dict wid column1 -- column2 )
  swap >r
  begin over 0<> while
    over hidden? not 2 pick wordlist-id h@ r@ = and if
      3 pick 3 pick 3 pick word-name count prefix? if
	over word-name count rot words-column-wrap
      then
    then
    swap prev-word @ swap
  repeat
  nip rdrop
;

\ Find the common prefix length to a word
: find-prefix-len ( b-addr bytes dict -- prefix-bytes )
  0 begin over 0<> while
    over hidden? not if
      3 pick 3 pick 3 pick word-name count common-prefix max
    then
    swap prev-word @ swap
  repeat
  nip nip nip
;

\ Commit code to flash
commit-flash

\ Set forth
forth-wordlist set-current

\ Lookup a word by its prefix
: lookup ( "name" -- )
  cr token
  2dup ram-latest find-prefix-len
  2 pick 2 pick flash-latest find-prefix-len
  rot drop max
  0 0 begin
    dup order-count @ <
  while
    >r 2 pick 2 pick ram-latest order r@ 2* + h@ 4 roll lookup-dict
    2 pick 2 pick flash-latest order r@ 2* + h@ 4 roll lookup-dict r> 1+
  repeat
  2drop 2drop cr
;

\ Display all the words as four columns
: words ( -- )
  cr
  0 0 begin
    dup order-count @ <
  while
    >r ram-latest order r@ 2* + h@ rot words-dict
    flash-latest order r@ 2* + h@ rot words-dict
    r> 1+
  repeat
  2drop cr
;

\ Set internal
internal-wordlist set-current

\ Search for all the words that go by a certain name in a given dictionary
: search-word-info ( b-addr bytes dict -- )
  begin dup 0<> while
    >r 2dup r@ word-name count equal-case-strings? if
      r@ ram-base >= r@ ram-end < and if
	." ram   "
      else
	." flash "
      then
      r@ h.8 space
      r@ >body h.8 space
      r@ word-flags h@ h.4 space space
      r@ wordlist-id h@ h.4 cr
    then
    r> prev-word @
  repeat
  drop 2drop
;

\ Search for a word by its xt
: search-by-xt ( dict xt -- name|0 flag )
  begin over 0<> while
    over >body over = if
      drop true exit
    then
    swap prev-word @ swap
  repeat
  drop false
;  

\ Set forth
forth-wordlist set-current

\ Dump all the words that go by a certain name
: word-info ( "name" -- )
  cr token  ." dict  name     xt       flags wordlist" cr
  2dup ram-latest search-word-info
  flash-latest search-word-info
;

\ Commit code to flash
commit-flash

\ Search the dictionary for a word by its xt
: >name ( xt -- name )
  ram-latest over search-by-xt if
    nip exit
  else
    drop flash-latest over search-by-xt drop nip
  then
;

\ Safely type a string
: safe-type ( b-addr bytes -- )
  pause-enabled @
  0 pause-enabled !
  -rot serial-type
  pause-enabled !
;

\ Commit code to flash
commit-flash

\ Safely type an integer
: safe-type-integer ( n -- )
  ram-here swap format-integer dup ram-allot
  dup >r safe-type r> negate ram-allot
;

\ Safely type an unsigned integer
: safe-type-unsigned ( u -- )
  ram-here swap format-unsigned dup ram-allot
  dup >r safe-type r> negate ram-allot
;

\ Commit code to flash
commit-flash

\ Fill memory with a byte
: fill ( b-addr u b -- )
  swap begin dup 0> while rot 2 pick over b! 1+ rot rot 1- repeat drop 2drop
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
  dup 0= triggers token-expected
  start-compile-no-push
  compiling-to-flash? if
    here 28 + ( 28 bytes ) flash-block-size align
  else
    here 16 + ( 16 bytes ) 4 align
  then
  tos push,
  dup tos literal,
  14 bx,
  $003F h,
  visible
  inlined
  finalize-no-align,
  advance-here
;

\ Set internal
internal-wordlist set-current

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
    here 32 + ( 32 bytes ) 4 align
  else
    here 24 + ( 24 bytes ) 4 align
  then
  tos push,
  dup tos literal,
  reserve-literal build-target !
  0 bx,
  $003F h,
  visible
  finalize-no-align,
  advance-here
;

\ Set forth
forth-wordlist set-current

\ Commit changes to flash
commit-flash

\ Create a word that executes code specified by DOES>
: <builds ( "name" -- )
  token
  dup 0= triggers token-expected
  <builds-with-name
;

\ No word is being built exception
: no-word-being-built ( -- ) space ." no word is being built" ;

\ Set internal
internal-wordlist set-current

\ Align to flash block if compiling to flash
: flash-align,
  compiling-to-flash? if flash-here flash-block-size align advance-here then
;

\ Set forth
forth-wordlist set-current

\ Commit changes to flash
commit-flash

\ Specify code for a word created wth <BUILDS
: does> ( -- )
  flash-align,
  build-target @ 0= triggers no-word-being-built
  r>
  0 build-target @ literal!
  0 build-target !
;

\ Begin declaring a structure
: begin-structure ( "name" -- offset )
  <builds here 0 4 allot flash-align, does> @
;

\ Finish declaring a structure
: end-structure ( offset -- ) swap current! ;

\ Create an arbitrary-sized field
: +field ( offset size "name" -- offset )
  <builds over , flash-align, + does> @ +
;

\ Create a byte-sized field
: bfield: ( offset "name" -- offset )
  <builds dup , flash-align, 1+ does> @ +
;

\ Create a halfword-sized field
: hfield: ( offset "name" -- offset )
  <builds 2 align dup , flash-align, 2+ does> @ +
;

\ Create a cell-sized field
: field: ( offset "name" -- offset )
  <builds cell align dup , flash-align, cell + does> @ +
;

\ Create a double cell-sized field
: 2field: ( offset "name" -- offset )
  <builds 2 cells align dup , flash-align, 2 cells + does> @ +
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
    here swap branch-back!
  then
  reserve-branch
  -rot postpone then
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
    here swap branch-back!
  then
;

\ Set internal
internal-wordlist set-current

\ Look up next available user space
: next-user-space ( -- offset )
  s" *USER*" visible-flag flash-latest find-all-dict dup if
    >body execute
  else
    drop 0
  then
;

\ Specify next available user space
: set-next-user-space ( offset -- )
  compiling-to-flash?
  swap
  compile-to-flash
  get-current
  swap
  internal-wordlist set-current
  s" *USER*" constant-with-name
  set-current
  not if
    compile-to-ram
  then
;

\ Look up next available RAM space
: next-ram-space ( -- addr )
  s" *RAM*" visible-flag flash-latest find-all-dict dup if
    >body execute
  else
    drop 0
  then
  sys-ram-dict-base +
;

\ Specify next available RAM space
: set-next-ram-space ( addr -- )
  sys-ram-dict-base -
  compiling-to-flash?
  swap
  compile-to-flash
  get-current
  swap
  internal-wordlist set-current
  s" *RAM*" constant-with-name
  set-current
  not if
    compile-to-ram
  then
;

\ Complete a USER variable word
: user> ( -- ) does> @ dict-base @ + ;

\ Set forth
forth-wordlist set-current

\ Commit changes to flash
commit-flash

\ Allocate a byte user variable
: buser ( "name" -- )
  next-user-space
  compiling-to-flash?
  over
  compile-to-flash
  <builds , user>
  not if
    compile-to-ram
  then
  1+
  set-next-user-space
;

\ Allocate a halfword variable in RAM
: huser ( "name" -- )
  next-user-space
  2 align
  compiling-to-flash?
  over
  compile-to-flash
  <builds , user>
  not if
    compile-to-ram
  then
  2+
  set-next-user-space
;

\ Allocate a user variable
: user ( "name" -- )
  next-user-space
  4 align
  compiling-to-flash?
  over
  compile-to-flash
  <builds , user>
  not if
    compile-to-ram
  then
  4+
  set-next-user-space
;

\ Allocate a doubleword user variable
: 2user ( "name" -- )
  next-user-space
  4 align
  compiling-to-flash?
  over
  compile-to-flash
  <builds , user>
  not if
    compile-to-ram
  then
  8 +
  set-next-user-space
;

\ Allocate a user buffer
: user-buffer: ( bytes "name" -- )
  next-user-space
  compiling-to-flash?
  over
  compile-to-flash
  <builds , user>
  not if
    compile-to-ram
  then
  +
  set-next-user-space
;

\ Allocate a cell-aligned user buffer
: user-aligned-buffer: ( bytes "name" -- )
  next-user-space
  4 align
  compiling-to-flash?
  over
  compile-to-flash
  <builds , user>
  not if
    compile-to-ram
  then
  +
  set-next-user-space
;

\ Set internal
internal-wordlist set-current

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

\ Set forth
forth-wordlist set-current

\ Commit changes to flash
commit-flash

\ Specify a buffer of a given size
: buffer: ( # "name" -- )
  compiling-to-flash? if
    ram-buffer:
  else
    create ram-allot
  then
;

\ Specify a buffer of a given size
: aligned-buffer: ( # "name" -- )
  compiling-to-flash? if
    ram-aligned-buffer:
  else
    create ram-allot
  then
;

\ Create a one-byte variable
: bvariable ( "name" -- )
  compiling-to-flash? if
    ram-bvariable
  else
    create 1 ram-allot
  then
;

\ Create a two-byte variable
: hvariable ( "name" -- )
  compiling-to-flash? if
    ram-hvariable
  else
    create 2 ram-allot
  then
;

\ Create a four-byte variable
: variable ( "name" -- )
  compiling-to-flash? if
    ram-variable
  else
    create 4 ram-allot
  then
;

\ Create an eight-byte variable
: 2variable ( "name" -- )
  compiling-to-flash? if
    ram-2variable
  else
    create 8 ram-allot
  then
;

\ Set internal
internal-wordlist set-current

\ Specify current flash wordlist
: set-current-flash-wordlist ( wid -- )
  compiling-to-flash?
  swap
  compile-to-flash
  get-current
  swap
  internal-wordlist set-current
  s" *WORDLIST*" constant-with-name
  set-current
  not if
    compile-to-ram
  then
;

\ Look up the current flash wordlist
: get-current-flash-wordlist ( -- wid )
  s" *WORDLIST*" visible-flag flash-latest find-all-dict dup if
    >body execute
  else
    drop internal-wordlist
  then
;

\ Set forth
forth-wordlist set-current

\ Commit to flash
commit-flash

\ Create a flash wordlist
: flash-wordlist ( -- wid )
  get-current-flash-wordlist 1+ dup set-current-flash-wordlist
;

\ Set internal
internal-wordlist set-current

\ The minimum RAM wordlist
32768 constant min-ram-wordlist

\ The current RAM wordlist
variable current-ram-wordlist

\ Set forth
forth-wordlist set-current

\ Commit to flash
commit-flash

\ Create a RAM wordlist
: ram-wordlist ( -- wid )
  current-ram-wordlist @ 1 current-ram-wordlist +!
;

\ Commit to flash
commit-flash

\ Create a new wordlist
: wordlist ( -- wid )
  compiling-to-flash? if
    flash-wordlist
  else
    ram-wordlist
  then
;

\ Begin a do loop
: do ( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )
  [immediate]
  [compile-only]
  undefer-lit
  6 push,
  reserve-literal
  postpone >r
  postpone >r
  postpone >r
  here
;

\ Begin a ?do loop
: ?do ( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )
  [immediate]
  [compile-only]
  undefer-lit
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
  here
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
  undefer-lit
  0 6 0 lsl-imm,
  6 pull,
  0 0 cmp-imm,
  0branch,
  postpone rdrop
  postpone rdrop
  postpone rdrop
  here 1+ 6 rot literal!
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
  undefer-lit
  0 6 0 lsl-imm,
  6 pull,
  0 0 cmp-imm,
  0branch,
  postpone rdrop
  postpone rdrop
  postpone rdrop
  here 1+ 6 rot literal!
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
  $B500 h,
;

\ End lambda
: ;] ( -- )
  [immediate]
  [compile-only]
  $BD00 h,
  here over branch-back!
  4+ lit,
;

\ Create an anonymous word for an exception
: x" ( <text>" -- )
  [immediate]
  [compile-only]
  reserve-branch
  $B500 h,
  postpone space
  postpone ."
  $BD00 h,
  here over branch-back!
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

\ Set internal
internal-wordlist set-current

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

\ Set forth
forth-wordlist set-current

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

\ Set internal
internal-wordlist set-current

\ s" constant
2 constant s"-length
create s"-data char s b, char " b,

\ ." constant
2 constant ."-length
create ."-data char c b, char " b,

\ c" constant
2 constant c"-length
create c"-data char c b, char " b,

\ Commit to flash
commit-flash

\ The constants themselves
s"-data s"-length 2constant s"-constant
."-data ."-length 2constant ."-constant
c"-data c"-length 2constant c"-constant

\ Set forth
forth-wordlist set-current

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

\ Set internal
internal-wordlist set-current

\ Maximum pictured numeric output size
65 constant picture-size

\ Start of pictured numeric output
variable picture-offset

\ Set forth
forth-wordlist set-current

\ Commit to flash
commit-flash

\ Add a character to pictured numeric output
: hold ( b -- )
  -1 picture-offset +!
  pad picture-size + picture-offset @ + b!
;

\ Commit to flash
commit-flash

\ Start pictured numeric output
: <# ( -- ) 0 picture-offset ! ;

\ Add a digit to the pictured numeric output
: # ( ud1 -- ud2 )
  base @ 0 ud/mod 2swap drop
  dup 10 >= if
    [ char A 10 - ] literal +
  else
    [char] 0 +
  then
  hold
;

\ Commit to flash
commit-flash

\ Add one or more digits to the pictured numeric output
: #s ( ud -- 0 0 )
  begin
    base @ 0 ud/mod 2dup d0= >r 2swap drop
    dup 10 >= if
      [ char A 10 - ] literal +
    else
      [char] 0 +
    then
    hold
  r> until
;

\ If n (a single number) is negative, append '-' to the pictured numeric output
: sign ( n -- ) 0< if [char] - hold then ;

\ Finish pictured numeric output
: #> ( xd -- c-addr bytes )
  2drop
  pad picture-size + picture-offset @ + picture-offset @ negate
;

\ Commit to flash
commit-flash

\ Format a signed double-cell number
: format-double ( b-addr nd -- b-addr bytes )
  2dup d0< if dnegate <# #s -1 sign #> else <# #s #> then
  dup >r rot dup >r swap move r> r>
;

\ Format an unsigned double-cell number
: format-double-unsigned ( b-addr ud -- b-addr bytes )
  <# #s #> dup >r rot dup >r swap move r> r>
;

\ Commit to flash
commit-flash

\ Type a signed double-cell number without a leading space
: (d.) ( nd -- )
  ram-here -rot format-double dup >r dup ram-allot type r> negate ram-allot
;

\ Type an unsigned double-cell number without a leading space
: (ud.) ( ud -- )
  ram-here -rot format-double-unsigned
  dup >r dup ram-allot type r> negate ram-allot
;

\ Commit to flash
commit-flash

\ Type a signed double-cell number with a leading space
: d. ( nd -- )
  space (d.)
;

\ Type an unsigned double-cell number with a leading space
: ud. ( ud -- )
  space (ud.)
;

\ Set internal
internal-wordlist set-current

\ Fraction size lookup table
create fraction-size-table
32 , 21 , 16 , 14 , 13 , 12 , 11 , 11 , 10 , 10 , 9 , 9 , 9 , 9 , 8 , 8 ,
8 , 8 , 8 , 8 , 8 , 8 , 7 , 7 , 7 , 7 , 7 , 7 ,

\ Commit to flash
commit-flash

\ Get current fraction size
: fraction-size ( -- u ) base @ 2 - cells fraction-size-table + @ ;

\ Commit to flash
commit-flash

\ Format digits to the right of the decimal point
: format-fraction ( u b-addr bytes -- b-addr bytes )
  2dup swap >r >r + 0 >r >r dup 0<> if
    begin
      r> r> dup swap >r swap >r fraction-size <> swap dup 0<> rot and if
	base @ um* dup 10 < if
	  [char] 0 +
	else
	  10 - [char] A +
	then
	r@ b! r> 1+ r> 1+ >r >r false
      else
	true
      then
    until
    drop rdrop r> r> + r> swap
  else
    drop [char] 0 r@ b! rdrop r> r> + 1+ r> swap
  then
;

\ Add a decimal point
: add-decimal ( b-addr bytes -- b-addr bytes )
  2dup + [char] , swap b! 1+
;

\ Set forth
forth-wordlist set-current

\ Commit to flash
commit-flash

\ Format an s31.32 number
: format-fixed ( b-addr f -- b-addr bytes )
  2dup d0< if
    dnegate 0 <# #s -1 sign #> add-decimal format-fraction
  else
    0 <# #s #> add-decimal format-fraction
  then
  dup >r rot dup >r swap move r> r>
;

\ Commit to flash
commit-flash

\ Type a fixed-point number without a leading space
: (f.) ( f -- )
  ram-here -rot format-fixed dup >r dup ram-allot type r> negate ram-allot
;

\ Commit to flash
commit-flash

\ Type a fixed-point number with a leading space
: f. ( f -- )
  space (f.)
;

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

\ Set internal
internal-wordlist set-current

\ Flash space warning has been displayed
variable flash-dict-warned

\ Commit to flash
commit-flash

\ Warn if flash space is running low
: do-flash-validate-dict ( -- )
  flash-end flash-here - 1024 < flash-dict-warned @ not and if
    true flash-dict-warned !
    space ." flash dictionary space is running low (<1K left)" cr
  then
;

\ Set forth
forth-wordlist set-current

\ Initialize the RAM variables
: init ( -- )
  init
  min-ram-wordlist current-ram-wordlist !
  next-ram-space dict-base !
  dict-base @ next-user-space + ram-here!
  0 wait-hook !
  false flash-dict-warned !
  ['] do-flash-validate-dict validate-dict-hook !
;

\ Finish compressing the code
end-compress-flash

\ Set compilation back to RAM
compile-to-ram

\ Warm reboot
warm