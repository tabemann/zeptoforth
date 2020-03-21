# Zeptoforth Kernel Words

### drop
( x -- )

### dup
( x1 -- x1 x1 )

### swap
( x1 x2 -- x2 x1 )

### over
( x1 x2 -- x1 x2 x1 )

### rot
( x1 x2 x3 -- x2 x3 x1 )

### pick
( xi ... x0 i -- xi ... x0 xi )

### roll
( xi ... x0 i -- xi-1 ... x0 xi )

### lshift
( x1 shift -- x2 )

### rshift
( x1 shift -- x2 )

### arshift
( x1 shift -- x2 )

### and
( x1 x2 -- x3 )

### or
( x1 x2 -- x3 )

### xor
( x1 x2 -- x3 )

### not
( x1 -- x2 )

### negate
( x1 -- x2 )

### +
( x1 x2 -- x3 )

### -
( x1 x2 -- x3 )

### *
( x1 x2 -- x3 )

### /
( n1 n2 -- n3 )

### u/
( u1 u2 -- u3 )

### mod
( n1 n2 -- n3 )

### umod
( u1 u2 -- u3 )

### =
( x1 x2 -- flag )

### <>
( x1 x2 -- flag )

### <
( n1 n2 -- flag )

### >
( n1 n2 -- flag )

### <=
( n1 n2 -- flag )

### >=
( n1 n2 -- flag )

### u<
( u1 u2 -- flag )

### u>
( u1 u2 -- flag )

### u<=
( u1 u2 -- flag )

### u>=
( u1 u2 -- flag )

### b!
( b addr -- )

### h!
( h h-addr -- )

### !
( x a-addr -- )

### 2!
( x1 x2 a-addr -- )

### bflash!
( b addr -- )

### hflash!
( h h-addr -- )

### flash!
( x a-addr -- )

### 2flash!
( x1 x2 a-addr -- )

### bcurrent!
( b addr -- )

### hcurrent!
( h h-addr -- )

### current!
( x a-addr -- )

### 2current!
( x1 x2 a-addr -- )

### b@
( addr -- b )

### h@
( h-addr -- h )

### @
( a-addr -- x )

### 2@
( a-addr -- x1 x2 )

### b,
( b -- )

### h,
( h -- )

### ,
( x -- )

### 2,
( x1 x2 -- )

### bflash,
( b -- )

### hflash,
( h -- )

### flash,
( x -- )

### 2flash,
( x1 x2 -- )

### bcurrent,
( b -- )

### hcurrent,
( h -- )

### current,
( x -- )

### 2current,
( x1 x2 -- )

### breserve
( b -- )

### hreserve
( h -- )

### reserve
( x -- )

### 2reserve
( x1 x2 -- )

### bflash-reserve
( b -- )

### hflash-reserve
( h -- )

### flash-reserve
( x -- )

### 2flash-reserve
( x1 x2 -- )

### bcurrent-reserve
( b -- )

### hcurrent-reserve
( h -- )

### current-reserve
( x -- )

### 2current-reserve
( x1 x2 -- )

### here
( -- addr )

### pad
( -- addr )

### allot
( bytes -- )

### here!
( addr -- )

### flash-here
( -- addr )

### flash-allot
( bytes -- )

### flash-here!
( addr -- )

### latest
( -- word )

### ram-latest
( -- word )

### flash-latest
( -- word )

### latest!
( word -- )

### ram-latest!
( word -- )

### flash-latest!
( word -- )

### current-here
( -- addr )

### current-allot
( bytes -- )

### align,
( 2^power -- )

### flash-align,
( 2^power -- )

### current-align,
( 2^power -- )

### current-cstring,
( addr bytes -- )

### >r
( x1 -- ) ( R: -- x1 )

### r>
( R: x1 -- ) ( -- x1 )

### r@
( R: x1 -- x1 ) ( -- x1 )

### rdrop
( R: x -- )

### rp@
( -- a-addr )

### rp!
( a-addr -- )

### sp@
( -- a-addr )

### sp!
( a-addr -- )

### emit
( b -- )

### emit?
( -- flag )

### space
( -- )

### cr
( -- )

### type
( addr bytes -- )

### count
( addr1 -- addr2 bytes )

### key
( -- b )

### key?
( -- flag )

### execute
( ??? xt -- ??? )

### ?execute
( ??? xt -- ??? )

### try
( ??? xt1 -- ??? xt2 | ??? 0 )

### ?raise
( xt -- )

### pause
( -- )

### exit
( R: addr -- )

### ws?
( b -- flag )

### newline?
( b -- flag )

### token-start
( -- i )

### token-end
( i1 -- i2 )

### token
( "token" -- addr bytes )

### \\
( "comment<NL>" -- )
#
(
( "comment)" -- )

### to-upper-char
( b -- b )

### equal-case-strings?
( addr1 bytes1 addr2 bytes2 -- flag )

### find-dict
( addr bytes mask dict -- word | 0 )

### find
( addr bytes mask -- word | 0 )

### >xt
( word -- xt )

### evaluate
( ??? addr bytes -- ??? )

### abort
( -- <empty stack> ) ( R: -- <empty stack> )

### quit
( R: -- <empty stack> )

### main
( -- )

### outer
( -- )

### refill
( -- )

### xon
( -- )

### xoff
( -- )

### ack
( -- )

### nak
( -- )

### parse-integer
( addr bytes -- n flag )

### parse-unsigned
( addr bytes -- u flag )

### parse-digit
( b base -- digit flag )

### :
( "name" -- )

### :noname
( -- xt )

### ;
( -- )

### constant
( x "name" -- )

### constant-with-name
( x addr bytes -- )

### 2constant
( x1 x2 "name" -- )

### 2constant-with-name
( x1 x2 addr bytes -- )

### if
( flag -- )

### else
( -- )

### then
( -- )

### begin
( -- )

### until
( flag -- )

### while
( flag -- )

### repeat
( -- )

### again
( -- )

### init
( -- )

### [immediate]
( -- )

### [compile-only]
( -- )

### [inlined]
( -- )

### immediate
( -- )

### compile-only
( -- )

### inlined
( -- )

### visible
( -- )

### [
( -- )

### ]
( -- )

### compile-to-ram
( -- )

### compile-to-flash
( -- )

### compiling-to-flash
( -- flag )

### compile,
( xt -- )

### token-word
( "name" -- word )

### '
( "name" -- xt )

### [']
( compiling: "name" -- ) ( compiled: -- xt )

### postpone
( "name" -- )

### lit,
( x1 -- ) ( compiled: -- x1 )

### literal
( compiling: x1 -- ) ( compiled: -- x1 )

### recurse
( -- )

### reboot
( -- )

### state
( -- a-addr )

### base
( -- a-addr )

### pause-enabled
( -- a-addr )

### stack-base
( -- a-addr )

### stack-end
( -- a-addr )

### rstack-base
( -- a-addr )

### rstack-end
( -- a-addr )

### handler
( -- a-addr )

### >parse
( -- a-addr )

### source
( -- addr bytes )

### build-target
( -- a-addr )

### sys-ram-dict-base
( -- addr )

### >in
( -- a-addr )

### input#
( -- a-addr )

### input
( -- addr )

### prompt-hook
( -- a-addr )

### handle-number-hook
( -- a-addr )

### failed-parse-hook
( -- a-addr )

### emit-hook
( -- a-addr )

### emit?-hook
( -- a-addr )

### key-hook
( -- a-addr )

### key?-hook
( -- a-addr )

### refill-hook
( -- a-addr )

### pause-hook
( -- a-addr )

### fault-handler-hook
( -- a-addr )

### null-handler-hook
( -- a-addr )

### systick-handler-hook
( -- a-addr )
