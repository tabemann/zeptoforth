# Basic Words

These words are in `forth-wordlist`.

##### `drop`
( x -- )

Drop the top of the data stack

##### `dup`
( x1 -- x1 x1 )

Duplicate the top of the data stack

##### `swap`
( x1 x2 -- x2 x1 )

Swap the top two places on the data stack

##### `over`
( x1 x2 -- x1 x2 x1 )

Copy the second place on the data stack onto the top of the stack,
pushing the top of the data stack to the second place

##### `rot`
( x1 x2 x3 -- x2 x3 x1 )

Rotate the top three places on the data stack, so the third place
moves to the first place

##### `pick`
( xi ... x0 i -- xi ... x0 xi )

Pick a value at a specified depth on the stack

##### `roll`
( xi ... x0 i -- xi-1 ... x0 xi )

Rotate a value at a given deph to the top of the stackk

##### `nip`
( x1 x2 -- x2 )

Remove the cell under that on the top of the stack

##### `tuck`
( x1 x2 -- x2 x1 x2 )

Push the cell on top of the stack under the item beneath it

##### `lshift`
( x1 u -- x2 )

Logical shift left

##### `rshift`
( x1 u -- x2 )

Logical shift right

##### `arshift`
( x1 u -- x2 )

Arithmetic shift right

##### `and`
( x1 x2 -- x3 )

Binary and

##### `or`
( x1 x2 -- x3 )

Binary or

##### `xor`
( x1 x2 -- x3 )

Binary xor

##### `bic`
( x1 x2 -- x3 )

Bit clear

##### `not`
( x1 -- x2 )

Binary not

##### `negate`
( n1 -- n2 )

Negation
	
##### `+`
( x1 x2 -- x3 )

Addition of two integers

##### `-`
( x1 x2 -- x3 )

Substraction of two integers

##### `*`
( x1 x2 -- x3 )

Multiplication of two integers

##### `/`
( n1 n2 -- n3 )

Signed division of two two's complement integers

##### `u/`
( u1 u2 -- u3 )

Unsigned division of two integers

##### `mod`
( n1 n2 -- n3 )

Signed modulus of two two's complement integers

##### `umod`
( u1 u2 -- u3 )

Unsigned modulus of two unsigned integers

##### `1+`
( x1 -- x2 )

Add 1 to an integer

##### `2+`
( x1 -- x2 )

Add 2 to an integer

##### `4+`
( x1 -- x2 )

Add 4 to an integer
	
##### `1-`
( x1 -- x2 )

Subtract 1 from an integer

##### `2-`
( x1 -- x2 )

Subtract 2 from an integer

##### `4-`
( x1 -- x2 )

Subtract 4 from an integer

##### `2*`
( x1 -- x2 )

Multiply a value by 2
	
##### `4*`
( x1 -- x2 )

Multiply a value by 4

##### `2/`
( u1 -- u2 )

Divide a value by 2
	
##### `4/`
( u1 -- u2 )

Divide a value by 4

##### `=`
( x1 x2 -- flag )

Equals

##### `<>`
( x1 x2 -- flag )

Not equal

##### `<`
( n1 n2 -- flag )

Less than

##### `>`
( n1 n2 -- flag )

Greater than

##### `<=`
( n1 n2 -- flag )

Less than or equal

##### `0=`
( x -- flag )

Equals zero

##### `0<>`
( x -- flag )

Not equal to zero

##### `0<`
( n -- flag )

Less than zero

##### `0>`
( n -- flag )

Greater than zero

##### `0<=`
( n -- flag )

Less than or equal to zero

##### `0>=`
( n -- flag )

Greater than or equal to zero
	
##### `u<`
( u -- flag )

Unsigned less than

##### `u>`
( u -- flag )

Unsigned greater than

##### `u<=`
( u -- flag )

Unsigned less than or equal

##### `u>=`
( u -- flag )

Unsigned greater than or equal
	
##### `ram-here`
( -- addr )

Get the RAM HERE pointer

##### `pad`
( -- addr )

Get the PAD pointer

##### `ram-allot`
( u -- )

Allot space in RAM

##### `ram-here!`
( addr -- )

Set the RAM flash pointer

##### `flash-here`
( -- addr )

Get the flash HERE pointer

##### `flash-allot`
( u -- )

Allot space in flash

##### `flash-here!`
( addr -- )

Set the flash HERE pointer

##### `latest`
( -- word )

Get the base address of the latest word

##### `ram-latest`
( -- word )

Get the base address of the latest RAM word

##### `flash-latest`
( -- word )

Get the base address of the latest flash word

##### `latest!`
( word -- )

Set the base address of the latest word

##### `ram-latest!`
( word -- )

Set the base address of the latest RAM word

##### `flash-latest!`
( word -- )

Set the base address of the latest flash word

##### `here`
( -- addr )

Get either the HERE pointer or the flash HERE pointer, depending on
compilation mode

##### `allot`
( u -- )

Allot space in RAM or in flash, depending on the compilation mode

##### `emit`
( b -- )

Emit a character

##### `emit?`
( -- flag )

Test for whether the system is ready to receive a character

##### `space`
( -- )

Emit a space

##### `cr`
( -- )

Emit a newline

##### `type`
( b-addr u -- )

Type a string

##### `serial-type`
( b-addr u -- )

Type a string using the native serial driver

##### `count`
( b-addr1 -- b-addr2 u )

Convert a cstring to a string
	
##### `key`
( -- b )

Receive a character

##### `key?`
( -- flag )

Test for whether the system is ready to receive a character

##### `enable-int`
( -- )

Enable interrupts

##### `disable-int`
( -- )

Disable interrupts

##### `sleep`
( -- )

Enter sleep mode
	
##### `execute`
( xt -- )

Execute an xt
	
##### `?execute`
( xt|0 -- )

Execute an xt if it is non-zero

##### `pause`
( -- )

Execute a PAUSE word, if one is set
	
##### `exit`
( R: addr -- )

Exit a word

##### `init`
( -- )

An empty init routine, to call if no other init routines are
available, so as to enable any source file to call a preceding init

##### `[immediate]`
( -- )

Set the currently-defined word to be immediate

##### `[compile-only]`
( -- )

Set the currently-defined word to be compile-only

##### `[inlined]`
( -- )

Set the currently-defined word to be inlined

##### `immediate`
( -- )

Set the currently-defined word to be immediate

##### `compile-only`
( -- )

Set the currently-defined word to be compile-only

##### `inlined`
( -- )

Set the currently-defined word to be inlined

##### `visible`
( -- )

Set the currently-defined word to be visible

##### `[`
( -- )

Switch to interpretation mode

##### `]`
( -- )

Switch to compilation state

##### `compile-to-ram`
( -- )

Set compilation to RAM

##### `compile-to-flash`
( -- )

Set compilation to flash

##### `compiling-to-flash?`
( -- flag )

Get whether compilation is to flash

##### `begin-critical`
( -- )

Begin a critical section, within which context switches will not take place; note that critical sections do not nest.

##### `end-critical`
( -- )

End a critical section; if a context switch would have occurred within the critical section, it is initiated immediately upon executing this word.

##### `compress-flash`
( -- )

Get whether to compress code compiled to flash

##### `end-compress-flash`
( -- )

End flash compression

##### `commit-flash`
( -- )

Commit code to flash without finishing compressing it

##### `compressing-flash`
( -- flag )

Get whether flash is being compressed

##### `word-align`
( addr -- a-addr )

Word-align an address

##### `compile,`
( xt -- )

Compile an xt

##### `token-word`
( "name" -- name )

Get the word corresponding to a token

##### `'`
( "name" -- xt )

Tick

##### `[']`
( compile-time: "name" -- ) ( run-time: -- xt )

Compiled tick
	
##### `postpone`
( compile-time: "name" -- )

Postpone a word

##### `lit,`
( x -- )

Compile a literal

##### `literal`
( compile-time: x -- )

Compile a literal

##### `recurse`
( -- )

Recursively call a word
	
##### `unknown-word`
( -- )

Unknown word exception
	
##### `b!`
( b b-addr -- )

Store a byte

##### `h!`
( h h-addr -- )

Store a halfword

##### `!`
( x a-addr -- )

Store a word

##### `2!`
( d a-addr -- )

Store a doubleword

##### `b+!`
( x b-addr -- )

Read a byte from an address, add a value, and write it back

##### `h+!`
( x h-addr -- )

Read a halfword from an address, add a value, and write it back

##### `+!`
( x a-addr -- )

Read a word from an address, add a value, and write it back

##### `b@`
( b-addr -- b )

Get a byte

##### `h@`
( h-addr -- h )

Get a halfword

##### `@`
( a-addr -- x )

Get a word

##### `2@`
( a-addr -- d )

Get a doubleword

##### `bram,`
( b -- )

Store a byte at the RAM HERE location

##### `hram,`
( h -- )

Store a halfword at the RAM HERE location

##### `ram,`
( x -- )

Store a word at the RAM HERE location
	
##### `2ram,`
( d -- )

Store a doubleword at the RAM HERE location

##### `bflash,`
( b -- )

Store a byte at the flash HERE location

##### `hflash,`
( h -- )

Store a halfword at the flash HERE location

##### `flash,`
( x -- )

Store a word at the flash HERE location

##### `2flash,`
( d -- )

Store a doubleword at the flash HERE location

##### `bcurrent!`
( b b-addr -- )

Store a byte to RAM or to flash

##### `hcurrent!`
( h h-addr -- )

Store a halfword to RAM or to flash

##### `current!`
( x a-addr -- )

Store a word to RAM or to flash

##### `2current!`
( d a-addr -- )

Store a doubleword to RAM or to flash

##### `b,`
( b -- )

Store a byte to the RAM or flash HERE location

##### `h,`
( h -- )

Store a halfword to the RAM or flash HERE location

##### `,`
( x -- )

Store a word to the RAM or flash HERE location

##### `2,`
( d -- )

Store a doubleword to the RAM or flash HERE location

##### `bram-reserve`
( -- addr )

Reserve a byte at the RAM HERE location

##### `hram-reserve`
( -- addr )

Reserve a halfword at the RAM HERE location

##### `ram-reserve`
( -- addr )

Reserve a word at the RAM HERE location

##### `2ram-reserve`
( -- addr )

Reserve a doubleword at the RAM HERE location

##### `bflash-reserve`
( -- addr )

Reserve a byte at the flash HERE location

##### `hflash-reserve`
( -- addr )

Reserve a halfword at the flash HERE location

##### `flash-reserve`
( -- addr )

Reserve a word at the flash HERE location

##### `2flash-reserve`
( -- addr )

Reserve a doubleword at the flash HERE location

##### `breserve`
( -- addr )

Reserve a byte at the RAM or flash HERE location

##### `hreserve`
( -- addr )

Reserve a halfword at the RAM or flash HERE location

##### `reserve`
( -- addr )

Reserve a word at the RAM or flash HERE location

##### `2reserve`
( -- addr )

Reserve a doubleword at the RAM or flash HERE location

##### `align,`
( u -- )

Align to a power of two

##### `flash-align,`
( u -- )

Align to a power of two

##### `ram-align,`
( u -- )

Align to a power of two

##### `cstring,`
( b-addr u -- )

Compile a c-string

##### `>r`
( x1 -- ) ( R: -- x1 )

Push a value onto the return stack

##### `r>`
( R: x1 -- ) ( -- x1 )

Pop a value off the return stack

##### `r@`
( R: x1 -- x1 ) ( -- x1 )

Get a value off the return stack without popping it

##### `rdrop`
( R: x -- )

Drop a value from the return stack

##### `rp@`
( -- a-addr )

Get the return stack pointer

##### `rp!`
( a-addr -- )

Set the return stack pointer

##### `sp@`
( -- a-addr )

Get the data stack pointer

##### `sp!`
( a-addr -- )

Set the data stack pointer

##### `reboot`
( -- )

Reboot

##### `warm`
( -- )

Carry out a warm reboot

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

##### `cell+`
( n -- n )

Add a cell to a value

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

##### `depth`
( -- u )

Get the depth of the stack, not including the cell pushed onto it by this
word

##### `h.1`
( b -- )

Output a hexadecimal nibble

##### `h.2`
( b -- )

Output a hexadecimal 8 bit value, padded with zeros

##### `h.4`
( h -- )

Output a hexadecimal 16 bit value, padded with zeros

##### `h.8`
( x -- )

Output a hexadecimal 32 bit value, padded with zeros

##### `h.16`
( ud -- )

Output a hexadecimal 64 bit value, padded with zeros

##### `.s`
( -- )

Dump the contents of the data stack

##### `?raise`

( xt|0 -- | 0 )

Raise an exception with the exception type in the TOS register

##### `try`

( xt1 -- xt2|0 )

Try to see if an exception occurs

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

##### `>name`
( xt -- name )

Search the dictionary for a word by its xt

##### `safe-type`
( b-addr bytes -- )

Safely type a string

##### `safe-type-integer`
( n -- )

Safely type an integer

##### `safe-type-unsigned`
( u -- )

Safely type an unsigned integer

##### `fill`
( b-addr u b -- )

Fill memory with a byte

##### `create`
( "name" -- )

Create a word referring to memory after it

##### `<builds`
( "name" -- )

Create a word that executes code specified by DOES>

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

##### `buser`
( "name" -- )

Allocate a byte user variable

##### `huser`
( "name" -- )

Allocate a halfword variable in RAM

##### `user`
( "name" -- )

Allocate a user variable

##### `2user`
( "name" -- )

Allocate a doubleword user variable

##### `user-buffer:`
( bytes "name" -- )

Allocate a user buffer

##### `user-aligned-buffer:`
( bytes "name" -- )

Allocate a cell-aligned user buffer

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

##### `buffer:`
( # "name" -- )

Specify a buffer of a given size

##### `aligned-buffer:`
( # "name" -- )

Specify a cell-aligned buffer of a given size

##### `flash-wordlist`
( -- wid )

Create a flash wordlist

##### `ram-wordlist`
( -- wid )

Create a RAM wordlist

##### `wordlist`
( -- wid )

Create a new wordlist

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

##### `x"`
( <text>" -- )

Create an anonymous word for an exception

##### `defer`
( "name" -- )

Create a deferred word

##### `defer-ram!`
( xt xt-deferred -- )

Set a deferred word in RAM

##### `defer-flash!`
( xt xt-deferred -- )

Set a deferred word in flash

##### `defer!`
( xt xt-deferred -- )

Set a deferred word; note that a deferred word stored in flash can only have its implementation set once

##### `defer@`
( xt-deferred -- xt )

Get the referred xt from a deferred word

##### `ws?`
( b -- flag )

Test whether a character is whitespace.

##### `newline?`
( b -- flag )

Test whether a character is a newline.

##### `token-start`
( -- i )

Parse the input buffer for the start of a token

##### `token-end`
( i1 -- i2 )

Parse the input buffer for the end of a token

##### `token`
( "token" -- b-addr u )

Parse a token

##### `\\`
( "comment<NL>" -- )

Parse a line comment

##### `(`
( "comment)" -- )

Parse a paren coment
	
##### `to-upper-char`
( b -- b )

Convert a character to being uppercase

##### `equal-case-strings?`
( b-addr1 u1 b-addr2 u2 -- flag )

Compare whether two strings are equal

##### `3dup`
( x1 x2 x3 -- x1 x2 x3 x1 x2 x3 )

Duplicate three items on the stack
	
##### `find`
( b-addr u mask -- addr|0 )

Find a word in the dictionary according to the word order list

##### `find-all`
( b-addr u mask -- addr|0 )

Find a word in the dictionary in any wordlist in order of definition

##### `>body`
( name -- xt )

Get an xt from a word

##### `evaluate`
( b-addr u -- )

Evaluate a string
	
##### `abort`
( -- )

Abort

##### `quit`
( -- )

The outer loop of Forth

##### `stack-overflow`
( -- )

Stack overflow exception

##### `stack-underflow`
( -- )

Stack underflow exception

##### `rstack-overflow`
( -- )

Return stack overflow exception

##### `rstack-underflow`
( -- )

Return stack underflow exception

##### `xon`
( -- )

Send XON

##### `xoff`
( -- )

Send XOFF

##### `ack`
( -- )

Send ACK

##### `nak`
( -- )

Send NAK

##### `bel`
( -- )

Send BEL

##### `failed-parse`
( -- )

Failed parse exception	

##### `parse-integer`
( b-addr u --  n success )

Parse an integer

##### `parse-unsigned`
( b-addr u1 -- u2 success )

Parse an unsigned integer

##### `parse-base`
( b-addr1 u1 -- b-addr2 u2 base )

Actually parse an integer base

##### `parse-digit`
( b base -- u success  )

Parse a digit
	
##### `:`
( -- )

Start a colon definition

##### `:noname`
( -- xt )

Start an anonymous colon definition

##### `;`
( -- )

End a colon definition

##### `constant`
( x "name" -- )

Create a constant

##### `2constant`
( d "name" -- )

Create a 2-word constant

##### `token-expected`
( -- )

Token expected exception handler

##### `not-compiling`
( -- )

We are not currently compiling

##### `compile-to-ram-only`
( -- )

We are currently compiling to flash

##### `[else]`
( -- )

Implement the [else] in [if]/[else]/[then] for conditional execution/compilation

##### `[if]`
( f -- )

Start conditional execution/compilation

##### `[then]`
( -- )

Finish conditional execution/compilation

##### `hold`
( b -- )

Add a character to pictured numeric output

##### `<#`
( -- )

Start pictured numeric output

##### `#`
( ud1 -- ud2 )

Add a digit to the pictured numeric output

##### `#s`
( ud -- 0 0 )

Add one or more digits to the pictured numeric output

##### `sign`
( n -- )

If n (a single number) is negative, append '-' to the pictured numeric output

##### `#>`
( xd -- c-addr bytes )

Finish pictured numeric output

##### `format-double`
( b-addr nd -- b-addr bytes )

Format a signed double-cell number

##### `format-double-unsigned`
( b-addr ud -- b-addr bytes )

Format an unsigned double-cell number

##### `(d.)`
( nd -- )

Type a signed double-cell number without a leading space

##### `(ud.)`
( ud -- )

Type an unsigned double-cell number without a leading space

##### `d.`
( nd -- )

Type a signed double-cell number with a leading space

##### `ud.`
( ud -- )

Type an unsigned double-cell number with a leading space

##### `format-fixed`
( b-addr f -- b-addr bytes )

Format an s31.32 number

##### `(f.)`
( f -- )

Type a fixed-point number without a leading space

##### `f.`
( f -- )

Type a fixed-point number with a leading space

##### `wait`
( xt -- )

Wait for a predicate to become true

##### `ms`
( u -- )

Wait for u milliseconds. Note that when multitasking is in use this automatically calls PAUSE. Also note that times are normally measured in 10ths of milliseconds, unlike this word.

##### `forget-ram`
( -- )

Forget the contents of RAM except for RAM variables and buffers compiled from flash, user variables (which are compiled into flash), and kernel-level RAM variables. Note that this is actually implemented in `src/common/forth/task.fs` rather than `src/common/forth/basic.fs` because it relies upon code implemented for multitasking.

##### `task-free`
( task -- )

Display space free for a given task

##### `free`
( -- )

Display space free for the main task and for flash in general

##### `copyright`
( -- )

Display the copyright notices

##### `license`
( -- )

Display the license notice

##### `.\"`
( "text<quote>" -- )

Print a string immediately
	
##### `s\"`
( "text<quote>" -- ) ( compiled: -- b-addr u )

Compile a non-counted string

##### `c\"`
( "text<quote>" -- ) ( compiled: -- b-addr )

Compile a counted-string

##### `compile-cstring`
( b-addr u -- ) ( compiled: -- b-addr )

Compile a counted-string

##### `char`
( "char" -- b )

Parse a character and put it on the stack

##### `[char]`
( "char" -- ) ( compiled: -- b )

Parse a character and compile it

##### `(.)`
( n -- )

Type an integer without a preceding space

##### `(u.)`
( u -- )

Type an unsigned integer without a preceding space

##### `debugu.`
( u -- )

Type an unsigned hexadecimal integer safely without a preceding space

##### `.`
( n -- )

Type an integer with a preceding space

##### `u.`
( u -- )

Type an unsigned integer with a preceding space
	
##### `move`
( b-addr1 b-addr2 u -- )

Copy bytes from one buffer to another one (which may overlap)

##### `reverse`
( b-addr u -- )

Reverse bytes in place

##### `format-unsigned`
( b-addr u1 -- b-addr u2 )

Format an integer as a string

##### `format-integer`
( b-addr n -- b-addr u )

Format an integer as a string

##### `marker`
( "name" -- )

Create a marker word named *name* to erase flash/return the flash dictionary to its prior state; note that the marker word that is created is erased when it is executed

##### `cornerstone`
( "name" -- )

Create a cornerstone word named *name* to erase flash/return the flash dictionary to its state immediately after `cornerstone` was executed; unlike `marker` the word created does not erase itself when executed and may be executed multiple times