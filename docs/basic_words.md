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

#### .s
( -- )

Dump the contents of the data stack

#### bbis
( bits addr -- )

Set bits on a byte

#### bbic
( bits addr -- )

Clear bits on a byte

#### hbis
( bits addr -- )

Set bits on a halfword

#### hbic
( bits addr -- )

Clear bits on a halfword

#### bis
( bits addr -- )

Set bits on a word

#### bic
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

\ Create a byte-sized field

#### hfield:
( offset "name" -- offset )

\ Create a halfword-sized field

#### field:
( offset "name" -- offset )

\ Create a cell-sized field

#### 2field:
( offset "name" -- offset )

Create a double cell-sized field

#### [:
( -- )

Begin lambda

#### ;]
( -- )

End lambda

#### cornerstone-does>
( -- )

Core of CORNERSTONE's DOES>

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

Begin a ?dup loop

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

#### wait
( xt -- )

Wait for a predicate to become true
