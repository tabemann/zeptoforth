# Basic Words

### `forth`

These words are in `forth`.

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

##### `1-`
( x1 -- x2 )

Subtract 1 from an integer

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

##### `if`
Compile-time: ( -- if-block ) Runtime: ( flag -- )

Compile the start if an *if* conditional block. If *flag* is false at runtime, branch after a following *else*, or if there is no *else*, *then*. If not compiling, begin implicit compilation (i.e. automatically compiling a temporary anonymous word at the REPL).

##### `else`
Compile-time: ( if-block -- if-block) Runtime: ( -- )

Compile an *else* in an *if* conditional block. At runtime jump past the end of the corresponding *then*.

##### `then`
Compile-time: ( if-block -- ) Runtime: ( -- )

Compile the end of an *if* conditional block. If matching an *if* that started implict compilation, end implicit compilation, execute the anonymous word, and then forget it.

##### `begin`
Compile-time: ( -- begin-block ) Runtime: ( -- )

Compile the beginning of a *begin* block. If not compiling, begin implicit compilation (i.e. automatically compiling a temporary anonymous word at the REPL).

##### `end`
Compile-time: ( begin-block -- ) Runtime: ( -- )

Compile the end of a *begin* block. If matching a *begin* that started implicit compilation, end implicit compilation, execute the anonymous word, and then forget it.

##### `while`
Compile-time: ( begin-block -- while-block ) Runtime: ( flag -- )

Compile the end of a *begin* block and the beginning of a *while* block. If *flag* is false at runtime, jump past the end of the corresponding *repeat*.

##### `repeat`
Compile-time: ( while-block -- ) Runtime: ( -- )

Compile the end of a *while* block. At runtime jump to the previous corresponding *begin*. If matching a *while* corresponding to a *begin* that started implicit compilation, end implicit compilation, execute the anonymous word, and then forget it.

##### `until`
Compile-time: ( begin-block -- ) Runtime: ( flag -- )

Compile an *until* to end a *begin* block. At runtime, if *flag* is false, jump back to the corresponding *begin*. If matching a *begin* that started implicit compilation, end implicit compilation, execute the anonymous word, and then forget it.

##### `again`
Compile-time: ( begin-block -- ) Runtime: ( -- )

Compile an *again* to end a *begin* block. At runtime jump back to the corresponding *begin*. If matching a *begin* that started implicit compilation, end implicit compilation, execute the anonymous word, and then forget it.

##### `do`
Compile-time: ( -- do-block ) Runtime: ( end start -- )

Compile the beginning of a *do* block. At runtime take *end* and *start* to initialize loop variables. If not compiling, begin implicit compilation (i.e. automatically compiling a temporary anonymous word at the REPL).

##### `?do`
Compile-time: ( -- do-block ) Runtime: ( end start -- )

Compile the beginning of a *do* block. At runtime take *end* and *start* to initialize loop variables, with *i* being set to *start*, and if they are equal jump past the corresponding *loop* or *+loop*. If not compiling, begin implicit compilation (i.e. automatically compiling an a temporary anonymous word at the REPL).

##### `loop`
Compile-time: ( do-block -- ) Runtime: ( -- )

Compile the end of a *do* block. Increment *i* by 1, and if it does not equal the *end* loop variable jump back to just after the corresponding *do* or *?do*. If matching a *do* or *?do* that started implicit compilation, end implicit compilation, execute the anonymous word, and then forget it.

##### `+loop`
Compile-time: ( do-block -- ) Runtime: ( increment -- )

Compile the end of a *do* block. Increment *i* by *increment*; if *increment* is positive and *i* moves past or equals the *end* loop variable, or if *increment* is negative and *i* moves past (but not equals) the *end* loop variable, jump back to just after the corresponding *do* or *?do*. If matching a *do* or *?do* that started implicit compilation, end implicit compilation, execute the anonymous word, and then forget it.

##### `i`
Compile-time: ( do-block -- do-block ) Runtime: ( -- i )

Get the loop index for a *do* block.

##### `j`
Compile-time: ( do-block -- do-block ) Runtime: ( -- j )

Get the loop index for the *do* block immediately outside the current *do* block.

##### `leave`
Compile-time: ( do-block -- do-block ) Runtime ( -- )

Jump to the end of a *do* block.

Leave a do loop

##### `unloop`
Compile-time: ( do-block -- do-block ) Runtime ( -- )

In ANS Forth and Forth 2012 this removes the current loop's loop variables, but in zeptofrth this is a no-op.

##### `case`
Compile-time: ( -- case-block )

Compile the beginning of a *case* block. If not compiling, begin implicit compilation (i.e. automatically compiling a temporary anonymous word at the REPL).

##### `of`
Compile-time: ( case-block -- case-block of-block ) Runtime: ( x1 x0 -- )

Compile the beginning of an *of* block within a *case* block. If *x1* does not match *x0* jump past the corresponding *endof*.

##### `endof`
Compile-time: ( case-block of-block -- case-block ) Runtime: ( -- )

Compile the end of an *of* block within a *case* block. Jump to the end of the *case* block.

##### `endcase`
Compile-time: ( case-block -- ) Runtime: ( x -- )

Compile the end of a *case* block comparing against a cell. If matching a *case* that started implicit compilation, end implicit compilation, execute the anonymous word, and then forget it.

##### `ofstr`
Compile-time: ( case-block -- case-block of-block ) Runtime: ( c-addr1 u1 c-addr0 u0 -- )

Compile the beginning of an *of* block within a *case* block. If the string defined by *c-addr1* *u1* does not match the string defined by *c-addr0* *u0* jump past the corresponding *endof*.

##### `ofstrcase`
Compile-time: ( case-block -- case-block of-block ) Runtime: ( c-addr1 u1 c-addr0 u0 -- )

Compile the beginning of an *of* block within a *case* block. If the string defined by *c-addr1* *u1* does not match (in a case-insensitive fashion, for ASCII characters) the string defined by *c-addr0* *u0* jump past the corresponding *endof*.

##### `endcasestr`
Compile-time: ( case-block -- ) Runtime: ( c-addr u -- )

Compile the end of a *case* block comparing against a string. If matching a *case* that started implicit compilation, end implicit compilation, execute the anonymous word, and then forget it.

##### `goto`
( "word" -- )

Drop all local and loop variables and branch to the target word. Note that no new return address is pushed onto the return stack.

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
( c -- )

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

##### `bl`
( -- c )

A space character

##### `type`
( c-addr u -- )

Type a string

##### `serial-type`
( c-addr u -- )

Type a string using the native serial driver

##### `count`
( c-addr1 -- c-addr2 u )

Convert a cstring to a string
	
##### `key`
( -- c )

Receive a character

##### `key?`
( -- flag )

Test for whether the system is ready to receive a character

##### `flush-console`
( -- )

Flush the current console's transmit buffer. Note that if the current console is a UART, this has the same considerations as `flush-uart` in the `uart` module, i.e. this will flush the in-RAM transmit buffer and the UART's transmit fifo, but not any data in any bitwise shift register, so to truly guarantee every bit has been transmitted a delay of (1 / baud rate) * 10 (for 8 data bits, 1 start bit, and 1 stop bit) after executing this is necessary.

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

##### `initializer`
( "init-word" -- )

Specify an initializer word, to execute immediately if currently compiling to RAM, or to execute on bootup after any previous `init` routines but before `turnkey` or `welcome` if currently compiling to flash; in the latter case, a new `init` routine will be compiled which first calls any previous `init` routine and then calls the referenced word. This is the commended way for any new code from this point on to specify initializers, as it will enable initializing properly both when code is compiled to RAM or is compiled to flash.

Note that this word _must not_ reference `init` or else it will inappropriately cause the preceding `init` to be immediately be run again, if compiling to RAM, or to be run twice, if compiling to flash. This will put the system in an undefined state (i.e. it will probably crash as a result).

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

Begin a critical section, within which context switches will not take place on the current core; note that critical sections do nest.

##### `end-critical`
( -- )

End a critical section; if a context switch would have occurred within the critical section, it is initiated immediately upon executing this word.

##### `critical`
( xt -- )

Execute code within a critical section; if an exception was raised within *xt* it is re-raised afterwards.

##### `with-allot`
( bytes xt -- ) ( xt: addr -- )

Allot RAM temporarily and clean it up afterwards, even if an exception occurs.

##### `with-ram`
( xt -- ) ( xt: -- )

Switch to compile to RAM mode, and afterwards restore the compilation state, even if an exception occurs.

##### `with-aligned-allot`
( bytes xt -- ) ( xt: addr -- )

Allot cell-aligned RAM temporarily and clean it up afterwards, even if an exception occurs.

##### `with-here`
( addr xt -- ) ( xt: -- )

Set RAM here to an address and reset it once complete or if an exception is
raised.

##### `provide-allot-2cell`
( xd xt -- ) ( xt: addr bytes -- )

Allot double cell-sized cell-aligned RAM temporarily and write a double cell into it before executing the provided *xt* with the buffer's address and size in bytes.

##### `provide-allot-cell`
( x xt -- ) ( xt: addr bytes -- )

Allot cell-sized cell-aligned RAM temporarily and write a cell into it before executing the provided *xt* with the buffer's address and size in bytes.

##### `provide-allot-half`
( h xt -- ) ( xt: addr bytes -- )

Allot halfword-sized cell-aligned RAM temporarily and write a halfword into it before executing the provided *xt* with the buffer's address and size in bytes.

##### `provide-allot-byte`
( c xt -- ) ( xt: addr bytes -- )

Allot byte-sized cell-aligned RAM temporarily and write a byte into it before executing the provided *xt* with the buffer's address and size in bytes.

##### `x-insufficient-data`
( -- )

This is an exception raised by the `extract-allot-*` words if they do not receive a sufficient-sized buffer of data.

##### `extract-allot-2cell`
( xt -- xd ) ( xt: addr bytes -- bytes' )

Allot double cell-sized cell-aligned RAM temporarily and provide its address and size in bytes to the *xt* that is executed, then read a double cell out of the buffer, unless the buffer length returned is shorter than a double cell, where then `x-insufficient-data` is raised.

##### `extract-allot-cell`
( xt -- x ) ( xt: addr bytes -- bytes' )

Allot cell-sized cell-aligned RAM temporarily and provide its address and size in bytes to the *xt* that is executed, then read a cell out of the buffer, unless the buffer length returned is shorter than a cell, where then `x-insufficient-data` is raised.

##### `extract-allot-half`
( xt -- h ) ( xt: addr bytes -- bytes' )

Allot halfword-sized cell-aligned RAM temporarily and provide its address and size in bytes to the *xt* that is executed, then read a halfword out of the buffer, unless the buffer length returned is shorter than a halfword, where then `x-insufficient-data` is raised.

##### `extract-allot-byte`
( xt -- c ) ( xt: addr bytes -- addr' bytes' )

Allot byte-sized cell-aligned RAM temporarily and provide its address and size in bytes to the *xt* that is executed, then read a byte out of the buffer, unless the buffer length returned is shorter than a byte, where then `x-insufficient-data` is raised.

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
	
##### `c!`
( c c-addr -- )

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

##### `c+!`
( x c-addr -- )

Read a byte from an address, add a value, and write it back

##### `h+!`
( x h-addr -- )

Read a halfword from an address, add a value, and write it back

##### `+!`
( x a-addr -- )

Read a word from an address, add a value, and write it back

##### `c@`
( c-addr -- c )

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

##### `cram,`
( c -- )

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

##### `cflash,`
( c -- )

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

##### `ccurrent!`
( c c-addr -- )

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

##### `c,`
( c -- )

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

##### `cram-reserve`
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

##### `cflash-reserve`
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

##### `creserve`
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
( c-addr u -- )

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

##### `cbit@`
( mask c-addr -- f )

Test for bits in a byte being set

##### `hbit@`
( mask h-addr -- f )

Test for bits in a halfword being set

##### `bit@`
( mask addr -- f )

Test for bits in a cell being set

##### `cbis!`
( bits addr -- )

Set bits on a byte

##### `cbic!`
( bits addr -- )

Clear bits on a byte

##### `cxor!`
( bits addr -- )

Exclusive-or bits on a byte

##### `hbis!`
( bits addr -- )

Set bits on a halfword

##### `hbic!`
( bits addr -- )

Clear bits on a halfword

##### `hxor!`
( bits addr -- )

Exclusive-or bits on a halfword

##### `bis!`
( bits addr -- )

Set bits on a word

##### `bic!`
( bits addr -- )

Clear bits on a word

##### `xor!`
( bits addr -- )

Exclusive-or bits on a word

##### `depth`
( -- u )

Get the depth of the stack, not including the cell pushed onto it by this
word

##### `h.1`
( c -- )

Output a hexadecimal nibble

##### `h.2`
( c -- )

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
( c-addr bytes -- )

Safely type a string

##### `safe-type-integer`
( n -- )

Safely type an integer

##### `safe-type-unsigned`
( u -- )

Safely type an unsigned integer

##### `fill`
( c-addr u c -- )

Fill memory with a byte

##### `create`
( "name" -- )

Create a word referring to memory after it

##### `<builds`
( "name" -- )

Create a word that executes code specified by `does>`

##### `does>`
( -- )

Specify code for a word created wth `<builds`

##### `defer`
( "name" -- )

Create a deferred word with the given name; note that executing it before it is set will result in a crash.

##### `is`
( xt "deferred-word" -- )

Set the deferred word named "deferred-word" to execute the execution token *xt* when it is executed. If the deferred word is compiled to flash this may only be done once. Also note that a crash will result if the deferred word is in flash and *xt* is in RAM and one executes the deferred word after rebooting.

##### `defer!`
( xt deferred-xt -- )

Set the deferred word corresponding to the execution token *deferred-xt* to execute the execution token *xt* when it is executed. If the deferred word is compiled to flash this may only be done once. Also note that a crash will result if *deferred-xt* is in flash and *xt* is in RAM and one executes *deferred-xt* after rebooting.

##### `defer@`
( deferred-xt -- xt )

Get the execution token *xt* executed when the deferred word corresponding to the execution token *deferred-xt* is executed.

##### `begin-structure`
( "name" -- offset )

Begin declaring a structure

##### `end-structure`
( offset -- )

Finish declaring a structure

##### `+field`
( offset size "name" -- offset )

Create an arbitrary-sized field

##### `cfield:`
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
( c-addr1 u1 c-addr2 u2 -- f )

Get whether two strings are equal

##### `begin-jumptable`
( "name" -- )

Start defining a jump table, which is a word, when called, takes one argument and matches it against any number of `=>` clauses, where if it matches one it will branch to the referenced word, until it reaches the end of the jump table or a `default=>` clause which, when reached, it will unconditionally branch to the referenced word.

##### `end-jumptable`
( -- )

End defining a jump table.

##### `=>`
( x "target" -- )

Compile a jump table entry matching a value *x* which, if matched, will branch to *target*.

##### `default=>`
( "target" -- )

Compile a jump table which will unconditionally branch to *target*.

##### `cuser`
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

##### `cvariable`
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

##### `dump`
( start-addr end-addr -- )

Dump memory between two addresses

##### `skip-until`
( xt -- )

Skip characters in the evaluation buffer until a predicate is met

##### `[:`
( -- )

Begin lambda; note that when executed in interpretation mode this is equivalent to `:noname`.

##### `;]`
( -- ) ( compiled: -- xt )

End lambda; note that when executed such that it matches a `[:` that was executed in interpretation mode this is equivalent to `;`.

##### `ws?`
( c -- flag )

Test whether a character is whitespace.

##### `newline?`
( c -- flag )

Test whether a character is a newline.

##### `token-start`
( -- i )

Parse the input buffer for the start of a token

##### `token-end`
( i1 -- i2 )

Parse the input buffer for the end of a token

##### `token`
( "token" -- c-addr u )

Parse a token

##### `\`
( "comment\<NL>" -- )

Parse a line comment

##### `(`
( "comment)" -- )

Parse a paren coment
	
##### `to-upper-char`
( c -- c )

Convert a character to being uppercase

##### `equal-case-strings?`
( c-addr1 u1 c-addr2 u2 -- flag )

Compare whether two strings are equal

##### `3dup`
( x1 x2 x3 -- x1 x2 x3 x1 x2 x3 )

Duplicate three items on the stack
	
##### `find`
( c-addr u -- word|0 )

Find a word in the dictionary according to the word order list

##### `find-all`
( c-addr u -- word|0 )

Find a word in the dictionary in any wordlist in order of definition

##### `find-by-xt`
( xt -- word|0 )

Find a word in any dictionary by execution token or return zero for no word found; only words with headers will be found

##### `>xt`
( word -- xt )

Get an xt from a word

##### `apply`
( ? word -- ? )

Evaluate a word in the current interpretation/compilation context.

##### `evaluate`
( ? c-addr u -- ? )

Evaluate a string. Note that only one task may interpret code at a time.

##### `evaluate-with-input`
( ? data input-addr input-bytes refill-xt eof-xt -- ? )

Evaluate input, initialised to the buffer at *input-addr* and the buffer length of *input-bytes*, with a refill handler *refill-xt* with the signature ( -- ), an end-of-file test *eof-xt* with the signature ( -- eof? ), and an auxiliary dat value *data*.

##### `feed-input`
( input-addr input-bytes -- )

Set the current input for evaluation to the buffer at *input-addr* and the buffer length of *input-bytes*, and the current evaluation index is reset to the start of the specified buffer.

##### `abort`
( -- )

Abort

##### `quit`
( -- )

The outer loop of Forth

##### `x-stack-overflow`
( -- )

Stack overflow exception

##### `x-stack-underflow`
( -- )

Stack underflow exception

##### `x-rstack-overflow`
( -- )

Return stack overflow exception

##### `x-rstack-underflow`
( -- )

Return stack underflow exception

##### `x-invalid-base`
( -- )

Invalid base (base < 2 or > 36) exception for numeric formatting.

##### `bel`
( -- )

Send BEL

##### `failed-parse`
( -- )

Failed parse exception	

##### `parse-integer`
( c-addr bytes --  n success? )

Parse a signed single-cell integer

##### `parse-unsigned`
( c-addr bytes -- u success? )

Parse an unsigned single-cell integer

##### `parse-double`
( c-addr bytes -- nd success? )

Parse a signed double-cell integer

##### `parse-double-unsigned`
( c-addr bytes -- ud success? )

Parse an unsigned double-cell integer

##### `parse-fixed`
( c-addr bytes -- f success? )

Parse an S31.32 fixed-point number

##### `parse-base`
( c-addr bytes -- c-addr' bytes' base )

Actually parse an integer base

##### `parse-digit`
( c base -- u success? )

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
( c -- )

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
( c-addr nd -- c-addr bytes )

Format a signed double-cell number

##### `format-double-unsigned`
( c-addr ud -- c-addr bytes )

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
( c-addr f -- c-addr bytes )

Format an S31.32 fixed-point number

##### `format-fixed-truncate`
( c-addr f places -- c-addr bytes )

Format an S31.32 fixed-point number truncated to *places* to the right of the decimal point

##### `(f.)`
( f -- )

Type an S31.32 fixed-point number without a leading space

##### `(f.n)`
( f places -- )

Type an S31.32 fixed-point number truncated to *places* to the right of the decimal point without a leading space

##### `f.`
( f -- )

Type an S31.32 fixed-point number with a leading space

##### `f.n`
( f places -- )

Type an S31.32 fixed-point number truncated to *places* to the right of the decimal point with a leading space

##### `s>d`
( n -- d )

Convert a signed single-cell number to a signed double-cell number.

##### `d>s`
( d -- n )

Convert a double-cell number to a single-cell number.

##### `s>f`
( n -- f )

Convert a signed single-cell number to an S31.32 fixed-point number.

##### `f>s`
( f -- n )

Convert an S31.32 fixed point number to a signed single-cell number, flooring it in the process.

##### `wait`
( xt -- )

Wait for a predicate to become true

##### `ms`
( u -- )

Wait for u milliseconds. Note that when multitasking is in use this automatically calls PAUSE. Also note that times are normally measured in 10ths of milliseconds, unlike this word.

##### `forget-ram`
( -- )

Forget the contents of RAM except for RAM variables and buffers compiled from flash, user variables (which are compiled into flash), and kernel-level RAM variables. Note that this is actually implemented in `src/common/forth/task.fs` rather than `src/common/forth/basic.fs` because it relies upon code implemented for multitasking.

##### `task-unused`
( task -- )

Display space free for a given task

##### `unused`
( -- )

Display space free for the main task and for flash in general

##### `license`
( -- )

Display the license notice

##### `."`
( "text\<quote>" -- )

Compile a string to be printed.
	
##### `s"`
( "text\<quote>" -- ) ( compiled: -- c-addr u )

Compile a non-counted string

##### `c"`
( "text\<quote>" -- ) ( compiled: -- c-addr )

Compile a counted-string

##### `.(`
( "text\<close-paren>" -- )

Immediately print a string.

##### `.\"`
( "text\<quote>" -- )

Compile an escaped string to be printed.
	
##### `s\"`
( "text\<quote>" -- ) ( compiled: -- c-addr u )

Compile an escaped non-counted string

##### `c\"`
( "text\<quote>" -- ) ( compiled: -- c-addr )

Compile an escaped counted-string

##### `.\(`
( "text\<close-paren>" -- )

Immediately print an escaped string.

##### `compile-cstring`
( c-addr u -- ) ( compiled: -- c-addr )

Compile a counted-string

##### `char`
( "char" -- c )

Parse a character and put it on the stack

##### `[char]`
( "char" -- ) ( compiled: -- c )

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
( c-addr1 c-addr2 u -- )

Copy bytes from one buffer to another one (which may overlap)

##### `reverse`
( c-addr u -- )

Reverse bytes in place

##### `spaces`
( u -- )

Emit a number of spaces.

##### `format-unsigned`
( c-addr u1 -- c-addr u2 )

Format an integer as a string

##### `format-integer`
( c-addr n -- c-addr u )

Format an integer as a string

##### `fix`
( c-addr bytes -- c-addr bytes )

Store a string in the RAM dictionary.

##### `cfix`
( c-addr bytes -- c-addr )

Store a string as counted string in the RAM dictionary.

##### `marker`
( "name" -- )

Create a marker word named *name* to erase flash/return the flash dictionary to its prior state; note that the marker word that is created is erased when it is executed

##### `cornerstone`
( "name" -- )

Create a cornerstone word named *name* to erase flash/return the flash dictionary to its state immediately after `cornerstone` was executed; unlike `marker` the word created does not erase itself when executed and may be executed multiple times

##### `with-error-console`
( xt -- )

Execute *xt* with `emit-hook` and `emit?-hook` replaced with `error-emit-hook` and `error-emit?-hook`, resetting them afterwards even if an exception is raised.

##### `try-and-display-error`
( xt -- exception-xt )

Execute *xt* and if an exception is raised, execute the exception with the console set to bright red (unless colors are disabled), and with `emit-hook` and `emit?-hook` replaced with `error-emit-hook` and `error-emit?-hook`, resetting them afterwards even if an exception is raised. Note that if the exception raises an exception when executed, it is caught and ignored.

##### `vector-count`
( -- count )

Get the number of interrupt vectors.

##### `vector-table`
( -- addr )

Get the interrupt vector table address.

##### `display-red`
( -- )

Set the console to display red unless colors are disabled.

##### `display-normal`
( -- )

Set the console to display normal colors unless colors are disabled.

##### `xon`
( -- )

Emit an XON character if `xon-xoff-enabled` is set to non-zero.

##### `xoff`
( -- )

Emit an XOFF character if `xon-xoff-enabled` is seto to zero.

##### `ack`
( -- )

Emit an ACK character.

##### `nak`
( -- )

Emit a NAK character.

##### `emit-hook`
( -- addr )

The emit hook user variable

##### `emit?-hook`
( -- addr )

The emit? hook user variable

##### `key-hook`
( -- addr )

The key hook user variable

##### `key?-hook`
( -- addr )

The key? hook user variable

##### `error-emit-hook`
( -- addr )

The error emit hook user variable

##### `error-emit?-hook`
( -- addr )

The error emit? hook user variable

##### `flush-console-hook`
( -- addr )

The flush console hook user variable

##### `error-flush-console-hook`
( -- addr )

The error flush console hook user variable

##### `pause-enabled`
( -- addr )

The `pause` enabled variable for the current core. This defaults to `true`.

##### `ack-nak-enabled`
( -- addr )

The ACK/NAK flow control enabled variable. This defaults to `true`.

##### `bel-enabled`
( -- addr )

The BEL on error enabled variable. This defaults to `true`.

##### `color-enabled`
( -- addr )

The console color enabled variable. This defaults to `true`.

##### `xon-xoff-error`
( -- addr )

The XON/XOFF flow control enabled variable. This defaults to `false`.
