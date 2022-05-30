\ Copyright (c) 2020-2022 Travis Bemann
\ 
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

\ Compile this to flash
compile-to-flash

\ Begin compressing compiled code in flash
compress-flash

\ True constant
-1 constant true

\ False constant
0 constant false

\ Forth module constant
0 constant forth

\ Internal module constant
1 constant internal

\ Commit to flash
commit-flash

\ Set up wordlist order
forth internal 2 set-order
forth set-current

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

\ Folded flag
16 constant fold-flag

\ DMB instruction
: dmb ( -- ) [inlined] [ undefer-lit $F3BF h, $8F5F h, ] ;

\ DSB instruction
: dsb ( -- ) [inlined] [ undefer-lit $F3BF h, $8F4F h, ] ;

\ ISB instruction
: isb ( -- ) [inlined] [ undefer-lit $F3BF h, $8F6F h, ] ;

\ Add to a cell
\ : +! ( x addr -- ) swap over @ + swap ! [inlined] ;

\ Get the minimum of two numbers
\ : min ( n1 n2 -- n3 ) over - dup 0< and + ;

\ Get the maximum of two numbers
\ : max ( n1 n2 -- n3 ) 2dup > if drop else swap drop then ;

\ Rotate three cells in revese
: -rot ( x1 x2 x3 -- x3 x1 x2 ) rot rot [inlined] ;

\ Get the absolute value of a number
: abs ( n -- u ) dup 0< if negate then ;

\ Set internal
internal set-current

\ Fill memory with zeros up until a given address
: advance-here ( a -- )
  begin
    dup here >
  while
    0 c,
  repeat
  drop
;

\ Set forth
forth set-current

\ Align an address to a power of two
: align ( a power -- a ) swap 1- swap 1- or 1+ ;

\ Duplicate a cell if it is non-zero
: ?dup ( x -- x | 0 ) dup 0<> if dup then ;

\ Get the depth of the stack, not including the cell pushed onto it by this
\ word
: depth ( -- u ) sp@ stack-base @ swap - cell / 1- ;

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
  space ." [ "
  sp@ begin
    dup stack-base @ cell - <
  while
    dup @ .
    4+
  repeat
  drop
  ." ]"
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

\ Get whether a word is defined, and if it is, execute it
: execute-defined? ( "word" -- x )
  token visible-flag find-all ?dup if >body execute else 0 then
;

\ Set internal
internal set-current

\ Get the address of the flags for a word
: word-flags ( word -- h-addr ) [inlined] ;

\ Get the address of the wordlist id for a word
: wordlist-id ( word -- h-addr ) 2+ [inlined] ;

\ Get the next word for a word
: next-word ( word1 -- addr ) 4+ [inlined] ;

\ Get the name of a word (a counted string)
: word-name ( word -- b-addr ) 8 + [inlined] ;

\ Get the length of a common prefix to two strings
: common-prefix ( b-addr1 bytes1 b-addr2 bytes2 -- bytes3 )
  0 begin
    3 pick 0> 2 pick 0> and if
      4 pick c@ 3 pick c@ = if
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
	3 pick c@ 2 pick c@ = if
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
      over c@ [char] * = if
	+ 1- c@ [char] * =
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
    swap next-word @ swap
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
    swap next-word @ swap
  repeat
  nip rdrop
;

\ Find the common prefix length to a word
: find-prefix-len ( b-addr bytes dict -- prefix-bytes )
  0 begin over 0<> while
    over hidden? not if
      3 pick 3 pick 3 pick word-name count common-prefix max
    then
    swap next-word @ swap
  repeat
  nip nip nip
;

\ Commit code to flash
commit-flash

\ Set forth
forth set-current

\ Express the semantics of a word in the present compilation/interpretation
\ state
: apply ( ? word -- ? )
  state @ if
    dup word-flags h@ dup immediate-flag and if
      drop >body execute
    else
      dup inlined-flag and if
	drop >body inline,
      else
	fold-flag and if
	  >body fold,
	else
	  >body compile,
	then
      then
    then
  else
    dup word-flags h@ compiled-flag and triggers x-not-compiling
    >body execute
  then
;

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
internal set-current

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
    r> next-word @
  repeat
  drop 2drop
;

\ Search for a word by its xt
: search-by-xt ( dict xt -- name|0 flag )
  begin over 0<> while
    over >body over = if
      drop true exit
    then
    swap next-word @ swap
  repeat
  drop false
;  

\ Set forth
forth set-current

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

\ Commit code to flash
commit-flash

\ Fill memory with a byte
: fill ( b-addr u b -- )
  swap begin dup 0> while rot 2 pick over c! 1+ rot rot 1- repeat drop 2drop
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
  dup 0= triggers x-token-expected
  start-compile-no-push
  6 push,
  thumb-2 not if
    6 hreserve set-const-end
  else
    reserve-literal
  then
  14 bx,
  $003F h,
  visible
  finalize,
  thumb-2 if
    here 6 rot literal!
  then
;

\ Declare a deferred word
: defer ( "name" -- )
  token
  dup 0= triggers x-token-expected
  start-compile-no-push
  hreserve
  0 bx,
  $003F h,
  visible
  finalize-no-align,
  reserve 0 rot ldr-pc!
  flash-block-align,
;

internal set-current

\ Get a deferred word's xt address
: defer-xt@ ( deferred-xt -- ) dup h@ $FF and 2 lshift 2 + + 4 align ;

forth set-current

commit-flash

\ Set a deferred word
: defer! ( xt deferred-xt -- )
  compiling-to-flash? >r
  dup $20000000 < over $2FFFFFFF > or if
    compile-to-flash
  else
    compile-to-ram
  then
  swap 1 or swap defer-xt@ current!
  r> if
    compile-to-flash
  else
    compile-to-ram
  then
;

\ Get a deferred word
: defer@ ( deferred-xt -- xt ) defer-xt@ @ 1 bic ;

\ Set internal
internal set-current

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
  tos push,
  thumb-2 not if
    6 hreserve set-const-end
  else
    reserve-literal
  then
  reserve-literal build-target !
  0 bx,
  $003F h,
  visible
  finalize,
  thumb-2 if
    here 6 rot literal!
  then
;

\ Set forth
forth set-current

\ Commit changes to flash
commit-flash

\ Create a word that executes code specified by DOES>
: <builds ( "name" -- )
  token
  dup 0= triggers x-token-expected
  <builds-with-name
;

\ No word is being built exception
: x-no-word-being-built ( -- ) ." no word is being built" cr ;

\ Set internal
internal set-current

\ Align to flash block if compiling to flash
: block-align,
  compiling-to-flash? if flash-here flash-block-size align advance-here then
;

\ Set forth
forth set-current

\ Commit changes to flash
commit-flash

\ Specify code for a word created wth <BUILDS
: does> ( -- )
  thumb-2 if
    block-align,
  then
  build-target @ 0= triggers x-no-word-being-built
  r>
  0 build-target @ literal!
  thumb-2 not if consts, then
  0 build-target !
;

\ Begin declaring a structure
: begin-structure ( "name" -- offset )
  <builds here 0 4 allot does> @
;

\ Finish declaring a structure
: end-structure ( offset -- ) swap current! ;

\ Create an arbitrary-sized field
: +field ( offset size "name" -- offset )
  : over 65536 u< thumb-2 or if inlined then
    over lit, postpone + postpone ; +
;

\ Create a byte-sized field
: cfield: ( offset "name" -- offset )
  : dup 65536 u< thumb-2 or if inlined then
    dup lit, postpone + postpone ; 1+
;

\ Create a halfword-sized field
: hfield: ( offset "name" -- offset )
  : 2 align
    dup 65536 u< thumb-2 or if inlined then
    dup lit, postpone + postpone ; 2+
;

\ Create a cell-sized field
: field: ( offset "name" -- offset )
  : cell align
    dup 65536 u< thumb-2 or if inlined then
    dup lit, postpone + postpone ; cell+
;

\ Create a double cell-sized field
: 2field: ( offset "name" -- offset )
  : cell align
    dup 65536 u< thumb-2 or if inlined then
    dup lit, postpone + postpone ; 2 cells +
;

\ Get whether two strings are equal
: equal-strings? ( b-addr1 u1 b-addr2 u2 -- f )
  >r swap r@ = if
    begin
      r@ 0>
    while
      dup c@ 2 pick c@ = if
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
  undefer-lit
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
  undefer-lit
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
  undefer-lit
  postpone 2drop
  ?dup if
    here swap branch-back!
  then
;

\ Set internal
internal set-current

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
  internal set-current
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
  internal set-current
  s" *RAM*" constant-with-name
  set-current
  not if
    compile-to-ram
  then
;

\ Complete a USER variable word
: user> ( -- ) does> @ dict-base @ + ;

\ Set forth
forth set-current

\ Commit changes to flash
commit-flash

\ Allocate a byte user variable
: cuser ( "name" -- )
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

\ Commit to flash
commit-flash

\ Assign names to the built-in user variables; they must be in this order and
\ must not be preceded by any other user variables
end-compress-flash
user task-stack-base
user task-stack-end
user task-rstack-base
user task-rstack-end
user task-ram-here
user task-base
user task-handler
compress-flash

\ Set internal
internal set-current

\ Allocate a byte variable in RAM
: ram-cvariable ( "name" -- )
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
forth set-current

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
: cvariable ( "name" -- )
  compiling-to-flash? if
    ram-cvariable
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

\ Make a variable which is CPU-dependent
: cpu-variable ( "global-name" "cpu-name" -- )
  next-ram-space dup cpu-count cells + set-next-ram-space
  token start-compile-no-push visible
  cpu-count 1 > if
    2 lit, postpone lshift dup lit, postpone +
  else
    postpone drop dup lit,
  then
  undefer-lit 14 bx, finalize,
  cpu-count 1 > if
    token start-compile-no-push visible
    lit, postpone cpu-offset postpone +
    undefer-lit 14 bx, finalize,
  else
    constant
  then
;

\ Commit to flash
commit-flash

\ Set the internal wordlist
internal set-current

\ Is there a deferred context switch for a give CPU
cpu-count cells buffer: cpus-deferred-context-switch

\ Critical section state for a given CPU
cpu-count cells buffer: cpus-in-critical

\ Commit to flash
commit-flash

\ Is there a deferred context switch for the current CPU
: deferred-context-switch ( -- addr )
  cpus-deferred-context-switch cpu-offset +
;

\ Critical section state for the current CPU
: in-critical ( -- addr ) cpus-in-critical cpu-offset + ;

\ Commit to flash
commit-flash

\ Set the forth wordlist
forth set-current

\ Begin a critical section
: begin-critical ( -- )
  disable-int
  1 in-critical +!
  enable-int
;

\ End a critical section
: end-critical ( -- )
  disable-int
  in-critical @ 1- 0 max dup in-critical !
  0= if
    deferred-context-switch @
    false deferred-context-switch !
    if pause then
  then
  enable-int
;

\ End a critical section and pause
: end-critical-pause ( -- ) disable-int 0 in-critical ! pause enable-int ;

\ Commit to flash
commit-flash

\ Execute code within a critcal section, properly handling exceptions
: critical ( xt -- ) begin-critical try end-critical ?raise ;

\ Allot RAM temporarily and clean it up afterwards, even after an exception
: with-allot ( bytes xt -- ) ( xt: addr -- )
  ram-here rot ram-allot dup >r swap try r> ram-here! ?raise
;

\ Switch to compile to RAM mode, and afterwards restore the compilation state,
\ even if an exception occurs
: with-ram ( xt -- ) ( xt: -- )
  compiling-to-flash? dup if compile-to-ram then >r
  try r> if compile-to-flash then ?raise
;

\ Allot aligned RAM temporarily and clean it up afterwards, even after an
\ exception
: with-aligned-allot ( bytes xt -- ) ( xt: a-addr -- )
  ram-here >r 4 ram-align, ram-here rot ram-allot
  swap try r> ram-here! ?raise
;

\ Insufficient data exception
: x-insufficient-data ( -- ) ." insufficient data" cr ;

\ Commit to flash
commit-flash

\ Safely emit a character
: safe-emit ( b -- )
  begin-critical
  swap serial-emit
  end-critical
;

\ Safely type a string
: safe-type ( b-addr bytes -- )
  begin-critical
  -rot serial-type
  end-critical
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

\ Set internal
internal set-current

\ Specify current flash wordlist
: set-current-flash-wordlist ( wid -- )
  compiling-to-flash?
  swap
  compile-to-flash
  get-current
  swap
  internal set-current
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
    drop internal
  then
;

\ Set forth
forth set-current

\ Commit to flash
commit-flash

\ Create a flash wordlist
: flash-wordlist ( -- wid )
  get-current-flash-wordlist 1+ dup set-current-flash-wordlist
;

\ Set internal
internal set-current

\ The minimum RAM wordlist
32768 constant min-ram-wordlist

\ The current RAM wordlist
variable current-ram-wordlist

\ Set forth
forth set-current

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

\ Write to a field
: field! ( value mask lsb addr -- )
  dup >r @ 2 pick bic >r rot swap lshift swap and r> or r> !
;

\ Read a field
: field@ ( mask lsb addr -- value ) @ rot and swap rshift ;

\ Begin a do loop
: do ( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )
  [immediate]
  [compile-only]
  undefer-lit
  6 push,
  reserve-literal
  postpone (do)
  here
;

\ Begin a ?do loop
: ?do ( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )
  [immediate]
  [compile-only]
  undefer-lit
  6 push,
  reserve-literal
  postpone (?do)
  here
;

\ End a do loop
: loop ( R: leave current end -- leave current end | )
  [immediate]
  [compile-only]
  1+ lit, postpone (loop)
  here 1+ 6 rot literal!
;

\ End a do +loop
: +loop ( increment -- ) ( R: leave current end -- leave current end | )
  [immediate]
  [compile-only]
  1+ lit, postpone (+loop)
  here 1+ 6 rot literal!
;

\ Commit to flash
commit-flash

\ Set the internal module
internal set-current

\ Dump 16 bytes of ASCII
: dump-ascii-16 ( start-addr -- )
  [char] | emit
  16 0 do
    dup i + c@ dup $20 >= over $7F < and if emit else drop [char] . emit then
  loop
  [char] | emit drop
;

\ Set the forth module
forth set-current

\ Commit to flash
commit-flash

\ Dump memory as ASCII between two addresses
: dump-ascii ( start-addr end-addr -- )
  cr
  2dup < if
    swap do
      i h.8
      space space [char] | emit
      64 0 do
	j i + c@ dup $20 >= over $7F < and if emit else drop [char] . emit then
      loop
      [char] | emit cr
    64 +loop
  else
    2drop
  then
;

\ Dump memory as bytes and ASCII between two addresses
: dump ( start-addr end-addr -- )
  cr
  2dup < if
    swap do
      i h.8
      space
      4 0 do
	space j i + c@ h.2
      loop
      space
      8 4 do
	space j i + c@ h.2
      loop
      space
      12 8 do
	space j i + c@ h.2
      loop
      space
      16 12 do
	space j i + c@ h.2
      loop
      space space i dump-ascii-16 cr
    16 +loop
  else
    2drop
  then
;

\ Dump memory as 16-bit values and ASCII between two addresses
: dump-halfs ( start-addr end-addr -- )
  2 align swap 2 align swap
  cr
  2dup < if
    swap do
      i h.8
      space
      4 0 do
	space j i + h@ h.4
      2 +loop
      space
      8 4 do
	space j i + h@ h.4
      2 +loop
      space
      12 8 do
	space j i + h@ h.4
      2 +loop
      space
      16 12 do
	space j i + h@ h.4
      2 +loop
      space space i dump-ascii-16 cr
    16 +loop
  else
    2drop
  then
;

\ Dump memory as 32-bit cells and ASCII between two addresses
: dump-cells ( start-addr end-addr -- )
  4 align swap 4 align swap
  cr
  2dup < if
    swap do
      i h.8
      16 0 do
	space space j i + @ h.8
      4 +loop
      space space i dump-ascii-16 cr
    16 +loop
  else
    2drop
  then
;

\ Skip characters in the evaluation buffer until a predicate is met
: skip-until ( xt -- )
  >r
  begin
    source >parse @ > if
      >parse @ + c@ r@ execute 1 >parse +!
    else
      drop true
    then
  until
  rdrop
;

\ Begin lambda
: [: ( -- )
  [immediate]
  [compile-only]
  undefer-lit
  reserve-branch
  here
  $B500 h,
;

\ End lambda
: ;] ( -- )
  [immediate]
  [compile-only]
  undefer-lit
  $BD00 h,
  here rot branch-back!
  lit,
;

\ Print out multiple spaces
: spaces ( u -- ) begin dup 0> while space 1- repeat drop ;

\ Create an anonymous word for an exception
: x" ( <text>" -- )
  [immediate]
  [compile-only]
  undefer-lit
  reserve-branch
  here
  $B500 h,
  postpone space
  postpone ."
  $BD00 h,
  here rot branch-back!
  lit,
;

\ Set up the wordlist
wordlist constant esc-string
commit-flash
internal forth esc-string 3 set-order
esc-string set-current

\ Character constants
$07 constant alert
$08 constant backspace
$1B constant escape
$0C constant form-feed
$0D constant return
$0A constant line-feed
$09 constant horizontal-tab
$0B constant vertical-tab

\ Get an input byte, and return whether a byte was successfully gotten
: get-byte ( -- b success )
  >parse @ parse# < if
    parse-buffer >parse @ + c@
    1 >parse +!
    true
  else
    0 false
  then
;

\ Advance n bytes
: advance-bytes ( bytes -- )
  >parse @ + 0 max parse# min >parse !
;

\ Revert n bytes
: revert-bytes ( bytes -- ) negate advance-bytes ;

\ Commit to flash
commit-flash

\ Get the octal length
: octal-len ( -- bytes )
  3 begin dup 0> while
    get-byte if
      dup [char] 0 < swap [char] 9 > or if
	3 swap - dup 1+ revert-bytes exit
      else
	1-
      then
    else
      drop 3 swap - dup revert-bytes exit
    then
  repeat
  drop 3 dup revert-bytes
;

\ Get the hexadecimal length
: hex-len ( max-bytes -- bytes )
  dup >r begin dup 0> while
    get-byte if
      dup [char] 0 < over [char] 9 > or
      over [char] A < 2 pick [char] F > or and
      over [char] a < rot [char] f > or and if
	r> swap - dup 1+ revert-bytes exit
      else
	1-
      then
    else
      drop r> swap - dup revert-bytes exit
    then
  repeat
  drop r> dup revert-bytes
;

\ Commit to flash
commit-flash

\ Parse an octal escape
: escape-octal ( -- )
  octal-len
  dup >r base @ >r 8 base !
  >parse @ input + swap parse-unsigned if
    r> base !
    dup 256 u< if
      c, r> advance-bytes
    else
      drop rdrop
    then
  else
    r> base ! rdrop
  then
;

\ Skip an octal escape
: skip-escape-octal ( -- )
  octal-len
  dup >r base @ >r 8 base !
  >parse @ input + swap parse-unsigned if
    r> base !
    256 u< if
      r> advance-bytes
    else
      rdrop
    then
  else
    r> base ! rdrop
  then
;

\ Commit to flash
commit-flash

\ Parse a hexadecimal escape
: escape-hex ( -- )
  2 hex-len
  dup >r base @ >r 16 base !
  >parse @ input + swap parse-unsigned if
    r> base ! c, r> advance-bytes
  else
    r> base ! rdrop
  then
;

\ Skip a hexadecimal escape
: skip-escape-hex ( -- )
  2 hex-len
  dup >r base @ >r 16 base !
  >parse @ input + swap parse-unsigned if
    drop r> base ! r> advance-bytes
  else
    r> base ! rdrop
  then
;

\ Commit to flash
commit-flash

\ Parse an escape
: parse-escape ( -- )
  get-byte if
    dup [char] 0 < over [char] 9 > or if
      case
	[char] a of alert c, endof
	[char] A of alert c, endof
	[char] b of backspace c, endof
	[char] B of backspace c, endof
	[char] e of escape c, endof
	[char] E of escape c, endof
	[char] f of form-feed c, endof
	[char] F of form-feed c, endof
	[char] m of return c, line-feed c, endof
	[char] M of return c, line-feed c, endof
	[char] n of line-feed c, endof
	[char] N of line-feed c, endof
	[char] q of [char] " c, endof
	[char] Q of [char] " c, endof
	[char] r of return c, endof
	[char] R of return c, endof
	[char] t of horizontal-tab c, endof
	[char] T of horizontal-tab c, endof
	[char] v of vertical-tab c, endof
	[char] V of vertical-tab c, endof
	[char] x of escape-hex endof
	[char] X of escape-hex endof
	dup c,
      endcase
    else
      drop 1 revert-bytes escape-octal
    then
  else
    drop
  then
;

\ Skip an escape
: skip-escape ( -- )
  get-byte if
    dup [char] 0 < over [char] 9 > or if
      case
	[char] x of skip-escape-hex endof
	[char] X of skip-escape-hex endof
      endcase
    else
      drop 1 revert-bytes skip-escape-octal
    then
  else
    drop
  then
;

\ Commit to flash
commit-flash

\ Parse an escaped string
: parse-esc-string ( end-byte -- )
  >r begin
    get-byte if
      dup $5C = if
	drop parse-escape false
      else
	dup r@ = if
	  drop true
	else
	  c, false
	then
      then
    else
      drop true
    then
  until
  rdrop
;

\ Skip an escaped string
: skip-esc-string ( end-byte -- )
  >r begin
    get-byte if
      dup $5C = if
	drop skip-escape false
      else
	dup r@ = if
	  drop true
	else
	  1 advance-bytes false
	then
      then
    else
      drop true
    then
  until
  rdrop
;

\ Commit to flash
commit-flash

\ Actually compile an escaped counted string
: compile-esc-cstring ( end-byte -- )
  undefer-lit
  reserve-branch swap
  here dup 1+
  compiling-to-flash? if flash-here! else ram-here! then swap
  here swap parse-esc-string
  here swap -
  over ccurrent!
  here %1 and if 0 c, then
  swap here swap branch-back!
  lit,
  1 advance-bytes
;

\ Commit to flash
commit-flash

\ Change wordlists
forth set-current

\ Compile an escaped counted string
: c\" ( -- )
  [immediate]
  [compile-only]
  advance-once
  [char] " compile-esc-cstring
;

\ Compile an escaped string
: s\" ( -- )
  [immediate]
  [compile-only]
  advance-once
  [char] " compile-esc-cstring
  postpone count
;

\ Compile typing an escaped string
: .\" ( -- )
  [immediate]
  [compile-only]
  advance-once
  [char] " compile-esc-cstring
  postpone count
  postpone type
;

\ Immediately type an escaped string
: .\( ( -- )
  [immediate]
  advance-once
  compiling-to-flash? dup if
    compile-to-ram
  then
  here [char] ) parse-esc-string
  dup dup here swap - type
  ram-here! swap if compile-to-flash then
  1 advance-bytes
;

\ Set forth
forth set-current

\ Inner case of [else]
: [else]-case ( -- )
  case
    s" [if]" ofstrcase 1+ endof
    s" [else]" ofstrcase 1- dup if 1+ then endof
    s" [then]" ofstrcase 1- endof
    s" \" ofstrcase ['] newline? skip-until	endof
    s" (" ofstrcase [: [char] ) = ;] skip-until endof
    s\" s\"" ofstrcase [: [char] " = ;] skip-until endof
    s\" c\"" ofstrcase [: [char] " = ;] skip-until endof
    s\" .\"" ofstrcase [: [char] " = ;] skip-until endof
    s" .(" ofstrcase [: [char] ) = ;] skip-until endof
    s\" s\\\"" ofstrcase [char] " skip-esc-string endof
    s\" c\\\"" ofstrcase [char] " skip-esc-string endof
    s\" .\\\"" ofstrcase [char] " skip-esc-string endof
    s" .\(" ofstrcase [char] ) skip-esc-string endof
    s" char" ofstrcase state @ not if token 2drop then endof
    s" [char]" ofstrcase token 2drop endof
    s" '" ofstrcase state @ not if token 2drop then endof
    s" [']" ofstrcase token 2drop endof
    s" postpone" ofstrcase token 2drop endof
  endcasestr
;
    
\ Commit to flash
commit-flash

\ Implement the [else] in [if]/[else]/[then] for conditional
\ execution/compilation
: [else] ( -- )
  [immediate]
  1 begin
    begin token dup while
      [else]-case
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
internal set-current

\ Maximum pictured numeric output size
65 constant picture-size

\ Start of pictured numeric output
user picture-offset

\ Set forth
forth set-current

\ Commit to flash
commit-flash

\ Add a character to pictured numeric output
: hold ( b -- )
  -1 picture-offset +!
  pad picture-size + picture-offset @ + c!
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

\ Store a string in the RAM dictionary
: fix ( b-addr bytes -- b-addr bytes )
  [: here 2dup 2>r swap move 2r> swap dup allot ;] with-ram
;

\ Store a string as a counted string in the RAM dictionary
: cfix ( b-addr bytes -- b-addr )
  [: here dup >r 2dup c! 1+ swap dup 1+ allot move r> ;] with-ram
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

\ Type a signed double-cell number without a following space
: (d.) ( nd -- )
  ram-here -rot format-double dup >r dup ram-allot type r> negate ram-allot
;

\ Type an unsigned double-cell number without a following space
: (ud.) ( ud -- )
  ram-here -rot format-double-unsigned
  dup >r dup ram-allot type r> negate ram-allot
;

\ Commit to flash
commit-flash

\ Type a signed double-cell number with a following space
: d. ( nd -- )
  (d.) space
;

\ Type an unsigned double-cell number with a following space
: ud. ( ud -- )
  (ud.) space
;

\ Set internal
internal set-current

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
	r@ c! r> 1+ r> 1+ >r >r false
      else
	true
      then
    until
    drop rdrop r> r> + r> swap
  else
    drop [char] 0 r@ c! rdrop r> r> + 1+ r> swap
  then
;

\ Add a decimal point
: add-decimal ( b-addr bytes -- b-addr bytes )
  2dup + [char] , swap c! 1+
;

\ Set forth
forth set-current

\ Commit to flash
commit-flash

\ Format an s31.32 number
: format-fixed ( b-addr f -- b-addr bytes )
  base @ 2 >= base @ 16 <= and if
    2dup d0< if
      dnegate 0 <# #s -1 sign #> add-decimal format-fraction
    else
      0 <# #s #> add-decimal format-fraction
    then
    dup >r rot dup >r swap move r> r>
  else
    drop 0
  then
;

\ Commit to flash
commit-flash

\ Type a fixed-point number without a following space
: (f.) ( f -- )
  ram-here -rot format-fixed dup >r dup ram-allot type r> negate ram-allot
;

\ Commit to flash
commit-flash

\ Type a fixed-point number with a following space
: f. ( f -- )
  (f.) space
;

\ Wait hook variable
variable wait-hook

\ Wait for a predicate to become true
: wait ( xt -- )
  begin
    dup >r execute not r> swap
  while
    in-critical @ 0= if
      wait-hook @ ?execute
      pause
    then
  repeat
  drop
;

\ Wake hook variable
variable wake-hook

\ Wake all waiting tasks
: wake ( -- ) wake-hook @ ?execute ;

\ Set internal
internal set-current

commit-flash

\ Flush console hook variable
variable flush-console-hook

commit-flash

\ Flash space warning has been displayed
variable flash-dict-warned

\ Commit to flash
commit-flash

\ Warn if flash space is running low
: do-flash-validate-dict ( -- )
  flash-end flash-here - 1024 < flash-dict-warned @ not and if
    true flash-dict-warned !
    ." flash dictionary space is running low (<1K left)" cr
  then
;

\ Create a variable to store the end of the flash mini-dictionary
flash-mini-dict-size [if]

  variable flash-mini-dict-end

  \ Hash a counted string
  : hash-string ( b-addr bytes -- hash )
    2dup + rot ?do dup 7 lshift swap 25 rshift or i c@ to-upper-char xor loop
  ;

  \ Commit to flash
  commit-flash

  \ Hash an identifier by wordlist
  : hash-string-and-wid ( b-addr bytes wid -- hash )
    -rot hash-string xor
  ;
  
  \ Commit to flash
  commit-flash
  
  \ Hash a dictionary entry
  : hash-word ( word -- hash )
    dup word-name count rot wordlist-id h@ hash-string-and-wid
  ;
  
  \ Commit to flash
  commit-flash
  
  \ Initialize the flash mini-dictionary
  : init-flash-mini-dict ( -- )
    flash-latest
    begin dup 0<> flash-mini-dict-end @ flash-mini-dict u> and while
      -2 cells flash-mini-dict-end +!
      dup hash-word flash-mini-dict-end @ !
      dup flash-mini-dict-end @ cell+ !
      next-word @
    repeat
    drop
    flash-mini-dict-end @ flash-mini-dict u>= if
      flash-mini-dict-end @ flash-mini-dict
      [ flash-mini-dict-size flash-mini-dict + ] literal
      flash-mini-dict-end @ - move
      [ flash-mini-dict-size flash-mini-dict + ] literal
      flash-mini-dict-end @ - flash-mini-dict + flash-mini-dict-end !
    else
      -1 flash-mini-dict-end !
    then
  ;

  \ Add a word to the flash mini-dictionary
  : add-flash-mini-dict ( -- )
    compiling-to-flash? if
      flash-mini-dict-end @ dup -1 <> if
	[ flash-mini-dict flash-mini-dict-size + ] literal u< if
	  flash-latest hash-word flash-mini-dict-end @ !
	  flash-latest flash-mini-dict-end @ cell+ !
	  8 flash-mini-dict-end +!
	else
	  -1 flash-mini-dict-end !
	then
      else
	drop
      then
    then
  ;

  \ Find a word in the flash mini-dictionary
  : find-flash-mini-dict ( b-addr bytes mask wid -- addr|0 )
    swap >r 3dup hash-string-and-wid flash-mini-dict-end @ 
    begin dup flash-mini-dict u> while
      2 cells -
      2dup @ = if
	dup cell+ @ wordlist-id h@ 3 pick = if
	  dup cell+ @ word-flags h@ r@ and if
	    4 pick 4 pick 2 pick cell+ @ word-name count equal-case-strings? if
	      rdrop cell+ @ nip nip nip nip exit
	    then
	  then
	then
      then
    repeat
    rdrop 2drop 2drop drop 0
  ;

  \ Commit to flash
  commit-flash

  \ Find a word in a particular dictionary, while making use of the flash
  \ mini-dictionary
  : find-optimized-wid ( b-addr bytes mask wid -- addr|0 )
    compiling-to-flash? if
      find-flash-mini-dict
    else
      2over 2over ram-latest swap find-dict ?dup if
	nip nip nip nip
      else
	find-flash-mini-dict
      then
    then
  ;

  \ Commit to flash
  commit-flash

  \ Find a word using the flash mini-dictionary for optimization
  : find-optimized ( b-addr bytes mask -- addr|0 )
    flash-mini-dict-end @ -1 <> if
      order-count @ 1 lshift order + order ?do
	3dup i h@ find-optimized-wid ?dup if
	  nip nip nip unloop exit
	then
      2 +loop
      drop 2drop 0
    else
      do-find
    then
  ;

[then]

\ Commit to flash
commit-flash

\ Set forth
forth set-current

\ Provide a double cell as a buffer.
: provide-allot-2cell ( xd xt -- ) ( xt: addr bytes -- )
  2 cells [: swap 2>r r@ 2! r> 2 cells r> execute ;] with-aligned-allot
;

\ Provide a cell as a buffer.
: provide-allot-cell ( x xt -- ) ( xt: addr bytes -- )
  cell [: swap 2>r r@ ! r> cell r> execute ;] with-aligned-allot
;

\ Provide a halfword as a buffer.
: provide-allot-half ( h xt -- ) ( xt: addr bytes -- )
  cell [: swap 2>r r@ h! r> 2 r> execute ;] with-aligned-allot
;

\ Provide a byte as a buffer.
: provide-allot-byte ( c xt -- ) ( xt: addr bytes -- )
  cell [: swap 2>r r@ c! r> 1 r> execute ;] with-aligned-allot
;

\ Extract a double cell from a buffer.
: extract-allot-2cell ( xt -- xd ) ( xt: addr bytes -- bytes' )
  2 cells
  [: dup >r 2 cells rot execute 2 cells >= averts x-insufficient-data r> 2@ ;]
  with-aligned-allot
;

\ Extract a cell from a buffer.
: extract-allot-cell ( xt -- x ) ( xt: addr bytes -- bytes' )
  cell [: dup >r cell rot execute cell >= averts x-insufficient-data r> @ ;]
  with-aligned-allot
;

\ Extract a halfword from a buffer.
: extract-allot-half ( xt -- h ) ( xt: addr bytes -- bytes' )
  cell [: dup >r 2 rot execute 2 >= averts x-insufficient-data r> h@ ;]
  with-aligned-allot
;

\ Extract a byte from a buffer
: extract-allot-byte ( xt -- c ) ( xt: addr bytes -- bytes' )
  cell [: dup >r 1 rot execute 1 >= averts x-insufficient-data r> c@ ;]
  with-aligned-allot
;

\ Flush the console
: flush-console ( -- ) flush-console-hook @ ?execute ;

\ Initialize the RAM variables
: init ( -- )
  init
  task-stack-base @
  task-stack-end @
  task-rstack-base @
  task-rstack-end  @
  task-base @
  task-handler @
  next-ram-space dict-base !
  task-handler !
  task-base !
  task-rstack-end !
  task-rstack-base !
  task-stack-end !
  task-stack-base !
  dict-base @ next-user-space + ram-here!
  min-ram-wordlist current-ram-wordlist !
  cpu-count 0 ?do false cpus-deferred-context-switch i cells + ! loop
  cpu-count 0 ?do 0 cpus-in-critical i cells + ! loop
  0 wait-hook !
  0 wake-hook !
  0 flush-console-hook !
  false flash-dict-warned !
  ['] do-flash-validate-dict validate-dict-hook !
  [ flash-mini-dict-size ] [if]
    flash-mini-dict flash-mini-dict-size + flash-mini-dict-end !
    init-flash-mini-dict
    ['] add-flash-mini-dict finalize-hook !
    ['] find-optimized find-hook !
  [then]
;

\ Finish compressing the code
end-compress-flash

\ Set compilation back to RAM
compile-to-ram

\ Reboot
reboot