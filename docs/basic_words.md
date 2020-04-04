# Basic Non-Kernel Words

#### true
( -- f )

#### false
( -- f )

#### binary
( -- )

#### octal
( -- )

#### decimal
( -- )

#### hex
( -- )

#### cell
( u -- )

#### cells
( n -- n )

#### tos
( -- u )

#### visible-flag
( -- u )

#### immediate-flag
( -- u )

#### compiled-flag
( -- u )

#### inlined-flag
( -- u )

#### 2dup
( x1 x2 -- x1 x2 x1 x2 )

Duplicate two cells

#### 2drop
( x1 x2 -- )

Drop two cells

#### nip
( x1 x2 -- x2 )

Drop the cell under the top of the stack

#### tuck
( x1 x2 -- x2 x1 x2 )

Tuck a cell under the cell at he top of the stack

#### +!
( x addr -- )

Add to a cell

#### min
( n1 n2 -- n3 )

Get the minimum of two numbers

#### max
( n1 n2 -- n3 )

Get the maximum of two numbers

#### advance-here
( a -- )

Fill memory with zeros up until a given address

#### align
( a power -- a )

Align an address to a power of two

#### ?dup
( x -- x | 0 )

Duplicate a cell if it is non-zero

#### depth
( -- u )

Get the depth of the stack, not including the cell pushed onto it by this
word

#### .s
( -- )

Dump the contents of the data stack

#### averts
( f "name" -- )

Assert that a value is true, otherwise raise a specified exception

#### triggers
( f "name" -- )

Assert that a value is false, otherwise raise a specified exception

#### words-dict
( dict -- )

Display all the words in a dictionary

#### words
( -- )

Display all the words

#### bbis!
( bits addr -- )

Set bits on a byte

#### bbic!
( bits addr -- )

Clear bits on a byte

#### hbis!
( bits addr -- )

Set bits on a halfword

#### hbic!
( bits addr -- )

Clear bits on a halfword

#### bis!
( bits addr -- )

Set bits on a word

#### bic!
( bits addr -- )

Clear bits on a word

#### safe-type
( addr bytes -- )

Safely type a string

#### pad-flash-erase-block
Pad flash to a 2048 byte boundary

#### restore-flash
( flash-here -- )

Restore flash to a preexisting state

#### marker
( "name" -- )

Create a MARKER to erase flash/return the flash dictionary to its prior state

#### create
( "name" -- )

Create a word referring to memory after it

#### <builds-with-name
( addr bytes -- )

Create a word that executes code specified by DOES>

#### <builds
( "name" -- )

Create a word that executes code specified by DOES>

#### flash-align,
Align to flash block if compiling to flash

#### does>
( -- )

Specify code for a word created wth <BUILDS

#### begin-structure
( "name" -- offset )

Begin declaring a structure

#### end
( offset -- )

Finish declaring a structure

#### +field
( offset size "name" -- offset )

Create an arbitrary-sized field

#### bfield:
( offset "name" -- offset )

Create a byte-sized field

#### hfield:
( offset "name" -- offset )

Create a halfword-sized field

#### field:
( offset "name" -- offset )

Create a cell-sized field

#### 2field:
( offset "name" -- offset )

Create a double cell-sized field

#### [:
( -- )

Begin lambda

#### ;]
( -- ) ( compiled: -- xt )

End lambda

#### equal-strings?
( b-addr1 u1 b-addr2 u2 -- f )

Get whether two strings are equal

#### case
( -- )

Start a CASE statement

#### of
( x -- )

Start an OF clause

#### endof
( -- )

End an OF clause

#### endcase
( x -- )

End a CASE statement

#### ofstr
( x -- )

Start an OFSTR clause

#### ofstrcase
( x -- )

Start an OFSTRCASE clause

#### endcasestr
( x -- )

End a CASE statement comparing against a string

#### cornerstone
( "name" -- )

Adapted from Terry Porter's code; not sure what license it was under

#### next-ram-space
( -- addr )

Look up next available RAM space

#### set-next-ram-space
( addr -- )

Specify next available RAM space

#### buffer:
( # "name" -- )

Specify a buffer of a given size

#### aligned-buffer:
( # "name" -- )

Specify a buffer of a given size

#### bvariable
( "name" -- )

Create a one-byte variable

#### hvariable
( "name" -- )

Create a two-byte variable

#### variable
( "name" -- )

Create a four-byte variable

#### 2variable
( "name" -- )

Create an eight-byte variable

#### do
( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )

Begin a do loop

#### ?do
( end start -- ) ( R: -- leave start end ) ( compile: -- leave* loop )

Begin a ?do loop

#### loop
( R leave current end -- leave current end | )

End a do loop

#### +loop
( increment -- ) ( R: leave current end -- leave current end | )

End a do +loop

#### i
( R current end -- current end ) ( -- current )

Get the loop index

#### j
( R cur1 end1 leave cur2 end2 -- cur1 end1 leave cur2 end2 ) ( -- cur1 )

Get the loop index beneath the current loop

#### leave
( R leave current end -- )

Leave a do loop

#### unloop
( R: leave current end -- )

Unloop from a do loop (to exit, e.g.)

#### option
( f true-xt -- ) ( true-xt: ??? -- ??? )

Execute an xt based on whether a condition is true

#### choose
( f true-xt false-xt -- ) ( true-xt: ??? -- ??? ) ( false-xt: ??? -- ??? )

Execute one of two different xts based on whether a condition is true or false

#### loop-until
( ??? xt -- ??? ) ( xt: ??? -- ??? f )

Execute an until loop with an xt

#### while-loop
( ??? while-xt body-xt -- ??? ) ( while-xt: ??? -- ??? f )
( body-xt: ??? -- ??? )

Execute a while loop with a while-xt and a body-xt

#### count-loop
( ??? limit init xt -- ??? ) ( xt: ??? i -- ??? )

Execute a counted loop with an xt

#### count+loop
( ??? limit init xt -- ??? ) ( xt: ??? i -- ??? increment )

Execute a counted loop with an arbitrary increment with an xt

#### biter
( ??? addr count xt -- ??? ) ( xt: ??? b -- ??? )

Iterate executing an xt over a byte array

#### hiter
( ??? addr count xt -- ??? ) ( xt: ??? h -- ??? )

Iterate executing an xt over a halfword array

#### iter
( ??? addr count xt -- ??? ) ( xt: ??? x -- ??? )

Iterate executing an xt over a cell array

#### 2iter
( ??? addr count xt -- ??? ) ( xt: ??? d -- ??? )

Iterate executing an xt over a double-word array

#### iter-get
( ??? get-xt count iter-xt -- ??? ) ( get-xt: ??? i -- ??? x )
( iter-xt: ??? x -- ??? )

Iterate executing at xt over values from a getter

#### 2iter-get
( ??? get-xt count iter-xt -- ??? ) ( get-xt: ??? i -- ??? d ) ( iter-xt: ??? d -- ??? )

Iterate executing at xt over double-word values from a getter

#### bfind-index
( ??? b-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )

Find the index of a value in a byte array with a predicate

#### hfind-index
( ??? h-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )

Find the index of a value in a halfword array with a predicate

#### find-index
( ??? a-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )

Find the index of a value in a cell array with a predicate

#### 2find-index
( ??? a-addr count xt -- ??? i|-1 ) ( xt: ??? d -- ??? f )

Find the index of a value in a double-word array with a predicate

#### find-get-index
( ??? get-xt count pred-xt --- ??? i|-1 ) ( get-xt: ??? i -- ??? x )
( pred-xt: ??? x -- ??? f )

Find the index of a value from a getter with a predicate

#### 2find-get-index
( ??? get-xt count pred-xt --- ??? i|-1 ) ( get-xt: ??? i -- ??? d )
( pred-xt: ??? d -- ??? f )

Find the index of a double-word value from a getter with a predicate

#### bfind-value
( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )

Find a value in a byte array with a predicate


#### hfind-value
( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )

Find a value in a halfword array with a predicate


#### find-value
( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )

Find a value in a cell array with a predicate


#### 2find-value
( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? d -- ??? f )

Find a value in a double-word array with a predicate

#### find-get-value
( ???? get-xt count pred-xt --- ??? x|0 f ) ( get-xt: ??? i -- ??? x )
( pred-xt: ??? x -- ??? f )

Find a value from a getter with a predicate

#### 2find-get-value
( ???? get-xt count pred-xt --- ??? d|0 f ) ( get-xt: ??? i -- ??? d )
( pred-xt: ??? d -- ??? f )

Find a double-word value from a getter with a predicate

#### wait
( xt -- )

Wait for a predicate to become true
