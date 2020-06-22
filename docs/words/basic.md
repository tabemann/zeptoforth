# Basic Non-Kernel Words

##### `true`
( -- f )

##### `false`
( -- f )

##### `binary`
( -- )

##### `octal`
( -- )

##### `decimal`
( -- )

##### `hex`
( -- )

##### `cell`
( -- u )

##### `cells`
( n -- n )

##### `tos`
( -- u )

##### `visible-flag`
( -- u )

##### `immediate-flag`
( -- u )

##### `compiled-flag`
( -- u )

##### `inlined-flag`
( -- u )

##### `nip`
( x1 x2 -- x2 )

Drop the cell under the top of the stack

##### `tuck`
( x1 x2 -- x2 x1 x2 )

Tuck a cell under the cell at he top of the stack

##### `+!`
( x addr -- )

Add to a cell

##### `min`
( n1 n2 -- n3 )

Get the minimum of two numbers

##### `max`
( n1 n2 -- n3 )

Get the maximum of two numbers

##### `-rot`
( x1 x2 x3 -- x3 x1 x2 )

Rotate three cells in reverse

##### `abs`
( n -- u )

Get the absolute value of a number

##### `advance-here`
( a -- )

Fill memory with zeros up until a given address

##### `align`
( a power -- a )

Align an address to a power of two

##### `?dup`
( x -- x | 0 )

Duplicate a cell if it is non-zero

##### `bbit@`
( mask b-addr -- f )

Test for bits in a byte being set

##### `hbit@`
( mask h-addr -- f )

Test for bits in a halfword being set

##### `bit@`
( mask addr -- f )

Test for bits in a cell being set

##### `depth`
( -- u )

Get the depth of the stack, not including the cell pushed onto it by this
word

##### `.s`
( -- )

Dump the contents of the data stack

##### `averts`
( f "name" -- )

Assert that a value is true, otherwise raise a specified exception

##### `triggers`
( f "name" -- )

Assert that a value is false, otherwise raise a specified exception

##### `suppress`
( exc|0 "name" -- exc|0 )

Check whether an exception, typically returned by `try`, matches a specified
exception and if it does, replace it with zero, marking no exception,
otherwise passing the specified argument through.

##### `word-flags`
( word -- flags )

Get the flags for a word

##### `prev-word`
( word1 -- word2 )

Get the previous word for a word

##### `word-name`
( word -- b-addr )

Get the name of a word (a counted word)

##### `hidden?`
( word -- f )

Get whether a word is hidden (note that this means whether a word is displayed by WORDS, not whether it will be found by `find` set to find visible words)

##### `words-dict`
( dict -- )

Display all the words in a dictionary

##### `words`
( -- )

Display all the words

##### `bbis!`
( bits addr -- )

Set bits on a byte

##### `bbic!`
( bits addr -- )

Clear bits on a byte

##### `hbis!`
( bits addr -- )

Set bits on a halfword

##### `hbic!`
( bits addr -- )

Clear bits on a halfword

##### `bis!`
( bits addr -- )

Set bits on a word

##### `bic!`
( bits addr -- )

Clear bits on a word

##### `safe-type`
( addr bytes -- )

Safely type a string

##### `create`
( "name" -- )

Create a word referring to memory after it

##### `<builds-with-name`
( addr bytes -- )

Create a word that executes code specified by DOES>

##### `<builds`
( "name" -- )

Create a word that executes code specified by DOES>

##### `flash-align,`
Align to flash block if compiling to flash

##### `does>`
( -- )

Specify code for a word created wth <BUILDS

##### `begin-structure`
( "name" -- offset )

Begin declaring a structure

##### `end`
( offset -- )

Finish declaring a structure

##### `+field`
( offset size "name" -- offset )

Create an arbitrary-sized field

##### `bfield:`
( offset "name" -- offset )

Create a byte-sized field

##### `hfield:`
( offset "name" -- offset )

Create a halfword-sized field

##### `field:`
( offset "name" -- offset )

Create a cell-sized field

##### `2field:`
( offset "name" -- offset )

Create a double cell-sized field

##### `equal-strings?`
( b-addr1 u1 b-addr2 u2 -- f )

Get whether two strings are equal

##### `case`
( -- )

Start a CASE statement

##### `of`
( x -- )

Start an OF clause

##### `endof`
( -- )

End an OF clause

##### `endcase`
( x -- )

End a CASE statement

##### `ofstr`
( x -- )

Start an OFSTR clause

##### `ofstrcase`
( x -- )

Start an OFSTRCASE clause

##### `endcasestr`
( x -- )

End a CASE statement comparing against a string

##### `next-ram-space`
( -- addr )

Look up next available RAM space

##### `set-next-ram-space`
( addr -- )

Specify next available RAM space

##### `buffer:`
( # "name" -- )

Specify a buffer of a given size

##### `aligned-buffer:`
( # "name" -- )

Specify a cell-aligned buffer of a given size

##### `bvariable`
( "name" -- )

Create a one-byte variable

##### `hvariable`
( "name" -- )

Create a two-byte variable

##### `variable`
( "name" -- )

Create a four-byte variable

##### `2variable`
( "name" -- )

Create an eight-byte variable

##### `user-buffer:`
( # "name" -- )

Specify a user buffer of a given size

##### `user-aligned-buffer:`
( # "name" -- )

Specify a cell-aligned user buffer of a given size

##### `buser`
( "name" -- )

Create a one-byte user variable

##### `huser`
( "name" -- )

Create a two-byte user variable

##### `user`
( "name" -- )

Create a four-byte user variable

##### `2user`
( "name" -- )

Create an eight-byte user variable

##### `do`
( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )

Begin a do loop

##### `?do`
( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )

Begin a ?do loop

##### `loop`
( R leave current end -- leave current end | )

End a do loop

##### `+loop`
( increment -- ) ( R: leave current end -- leave current end | )

End a do +loop

##### `i`
( R current end -- current end ) ( -- current )

Get the loop index

##### `j`
( R cur1 end1 leave cur2 end2 -- cur1 end1 leave cur2 end2 ) ( -- cur1 )

Get the loop index beneath the current loop

##### `leave`
( R leave current end -- )

Leave a do loop

##### `unloop`
( R: leave current end -- )

Unloop from a do loop (to exit, e.g.)

##### `dump`
( start-addr end-addr -- )

Dump memory between two addresses

##### `skip-until`
( xt -- )

Skip characters in the evaluation buffer until a predicate is met

##### `[:`
( -- )

Begin lambda

##### `;]`
( -- ) ( compiled: -- xt )

End lambda

##### `defer`
( "name" -- )

Create a deferred word

##### `defer!`
( xt xt-deferred -- )

Set a deferred word; note that a deferred word stored in flash can only have its implementation set once

##### `defer@`
( xt-deferred -- xt )

Get the referred xt from a deferred word

##### `decode-mov16`
( h-addr -- h )

Decode the immediate field from a MOVW or MOVT instruction

##### `decode-literal`
( h-addr -- x )

Decode the immediate field from a pair of a MOVW instruction followed by a MOVT instruction

##### `[else]`
( -- )

Implement the [else] in [if]/[else]/[then] for conditional execution/compilation

##### `[if]`
( f -- )

Start conditional execution/compilation

##### `[then]`
( -- )

Finish conditional execution/compilation

##### `wait`
( xt -- )

Wait for a predicate to become true

##### `forget-ram`
( -- )

Forget the contents of RAM except for RAM variables and buffers compiled from flash, user variables (which are compiled into flash), and kernel-level RAM variables. Note that this is actually implemented in `src/common/forth/task.fs` rather than `src/common/forth/basic.fs` because it relies upon code implemented for multitasking.