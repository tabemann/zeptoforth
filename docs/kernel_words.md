# Zeptoforth Kernel Words

#### drop
( x -- )

#### dup
( x1 -- x1 x1 )

#### swap
( x1 x2 -- x2 x1 )

#### over
( x1 x2 -- x1 x2 x1 )

#### rot
( x1 x2 x3 -- x2 x3 x1 )

#### pick
( xi ... x0 i -- xi ... x0 xi )

#### roll
( xi ... x0 i -- xi-1 ... x0 xi )

#### lshift
( x1 u -- x2 )

#### rshift
( x1 u -- x2 )

#### arshift
( x1 u -- x2 )

#### and
( x1 x2 -- x3 )

#### or
( x1 x2 -- x3 )

#### xor
( x1 x2 -- x3 )

#### not
( x1 -- x2 )

#### negate
( x1 -- x2 )

#### +
( x1 x2 -- x3 )

#### -
( x1 x2 -- x3 )

#### *
( x1 x2 -- x3 )

#### /
( n1 n2 -- n3 )

#### u/
( u1 u2 -- u3 )

#### mod
( n1 n2 -- n3 )

#### umod
( u1 u2 -- u3 )

#### =
( x1 x2 -- f )

#### <>
( x1 x2 -- f )

#### <
( n1 n2 -- f )

#### >
( n1 n2 -- f )

#### <=
( n1 n2 -- f )

#### >=
( n1 n2 -- f )

#### u<
( u1 u2 -- f )

#### u>
( u1 u2 -- f )

#### u<=
( u1 u2 -- f )

#### u>=
( u1 u2 -- f )

#### b!
( b b-addr -- )

#### h!
( h h-addr -- )

#### !
( x a-addr -- )

#### 2!
( x1 x2 a-addr -- )

#### bflash!
( b b-addr -- )

#### hflash!
( h h-addr -- )

#### flash!
( x a-addr -- )

#### 2flash!
( x1 x2 a-addr -- )

#### bcurrent!
( b b-addr -- )

#### hcurrent!
( h h-addr -- )

#### current!
( x a-addr -- )

#### 2current!
( x1 x2 a-addr -- )

#### b@
( b-addr -- b )

#### h@
( h-addr -- h )

#### @
( a-addr -- x )

#### 2@
( a-addr -- x1 x2 )

#### b,
( b -- )

#### h,
( h -- )

#### ,
( x -- )

#### 2,
( x1 x2 -- )

#### bflash,
( b -- )

#### hflash,
( h -- )

#### flash,
( x -- )

#### 2flash,
( x1 x2 -- )

#### bcurrent,
( b -- )

#### hcurrent,
( h -- )

#### current,
( x -- )

#### 2current,
( x1 x2 -- )

#### breserve
( b -- )

#### hreserve
( h -- )

#### reserve
( x -- )

#### 2reserve
( x1 x2 -- )

#### bflash-reserve
( b -- )

#### hflash-reserve
( h -- )

#### flash-reserve
( x -- )

#### 2flash-reserve
( x1 x2 -- )

#### bcurrent-reserve
( b -- )

#### hcurrent-reserve
( h -- )

#### current-reserve
( x -- )

#### 2current-reserve
( x1 x2 -- )

#### here
( -- b-addr )

#### pad
( -- b-addr )

#### allot
( u -- )

#### here!
( b-addr -- )

#### flash-here
( -- b-addr )

#### flash-allot
( u -- )

#### flash-here!
( b-addr -- )

#### latest
( -- word )

#### ram-latest
( -- word )

#### flash-latest
( -- word )

#### latest!
( word -- )

#### ram-latest!
( word -- )

#### flash-latest!
( word -- )

#### current-here
( -- b-addr )

#### current-allot
( u -- )

#### align,
( 2^power -- )

#### flash-align,
( 2^power -- )

#### current-align,
( 2^power -- )

#### current-cstring,
( b-addr u -- )

#### >r
( x1 -- ) ( R: -- x1 )

#### r>
( R: x1 -- ) ( -- x1 )

#### r@
( R: x1 -- x1 ) ( -- x1 )

#### rdrop
( R: x -- )

#### rp@
( -- a-addr )

#### rp!
( a-addr -- )

#### sp@
( -- a-addr )

#### sp!
( a-addr -- )

#### emit
( b -- )

#### emit?
( -- f )

#### space
( -- )

#### cr
( -- )

#### type
( b-addr u -- )

#### count
( b-addr1 -- b-addr2 u )

#### key
( -- b )

#### key?
( -- f )

#### execute
( ??? xt -- ??? )

#### ?execute
( ??? xt -- ??? )

#### try
( ??? xt1 -- ??? xt2 | ??? 0 )

#### ?raise
( xt -- )

#### pause
( -- )

#### exit
( R: b-addr -- )

#### ws?
( b -- f )

#### newline?
( b -- f )

#### token-start
( -- i )

#### token-end
( i1 -- i2 )

#### token
( "token" -- b-addr u )

#### \\
( "comment<NL>" -- )
#
(
( "comment)" -- )

#### to-upper-char
( b -- b )

#### equal-case-strings?
( b-addr1 u1 b-addr2 u2 -- f )

#### find-dict
( b-addr u mask dict -- word | 0 )

#### find
( b-addr u mask -- word | 0 )

#### >xt
( word -- xt )

#### evaluate
( ??? b-addr u -- ??? )

#### abort
( -- <empty stack> ) ( R: -- <empty stack> )

#### quit
( R: -- <empty stack> )

#### main
( -- )

#### outer
( -- )

#### refill
( -- )

#### xon
( -- )

#### xoff
( -- )

#### ack
( -- )

#### nak
( -- )

#### parse-integer
( b-addr u -- n f )

#### parse-unsigned
( b-addr u -- u f )

#### parse-digit
( b base -- digit f )

#### skip-to-token
( -- )

#### parse-to-char
( b -- b-addr u )

#### .(
( "text)" -- )

#### ."
( "text<quote>" -- )

#### s"
( "text<quote>" -- b-addr u )

#### c"
( "text<quote>" -- b-addr )

#### compile-cstring
( b-addr u -- ) ( compiled: b-addr )

#### char
( "char" -- ) ( compiled: b )

#### [char]
( compiling: "char" -- ) ( compiled: b )

#### (.)
( n -- )

#### (u.)
( u -- )

#### .
( n -- )

#### u.
( u -- )

#### move
( b-addr1 b-addr2 u -- )

#### <move
( b-addr1 b-addr2 u -- )

#### move>
( b-addr1 b-addr2 u -- )

#### reverse
( b-addr u -- )

#### format-unsigned
( u1 -- b-addr u2 )

#### format-integer
( n -- b-addr u ) 

#### :
( "name" -- )

#### :noname
( -- xt )

#### ;
( -- )

#### constant
( x "name" -- )

#### constant-with-name
( x b-addr u -- )

#### 2constant
( x1 x2 "name" -- )

#### 2constant-with-name
( x1 x2 b-addr u -- )

#### if
( f -- )

#### else
( -- )

#### then
( -- )

#### begin
( -- )

#### until
( f -- )

#### while
( f -- )

#### repeat
( -- )

#### again
( -- )

#### init
( -- )

#### [immediate]
( -- )

#### [compile-only]
( -- )

#### [inlined]
( -- )

#### immediate
( -- )

#### compile-only
( -- )

#### inlined
( -- )

#### visible
( -- )

#### [
( -- )

#### ]
( -- )

#### compile-to-ram
( -- )

#### compile-to-flash
( -- )

#### compiling-to-flash
( -- f )

#### compile,
( xt -- )

#### token-word
( "name" -- word )

#### '
( "name" -- xt )

#### [']
( compiling: "name" -- ) ( compiled: -- xt )

#### postpone
( "name" -- )

#### lit,
( x1 -- ) ( compiled: -- x1 )

#### literal
( compiling: x1 -- ) ( compiled: -- x1 )

#### recurse
( -- )

#### reboot
( -- )

#### state
( -- a-addr )

#### base
( -- a-addr )

#### pause-enabled
( -- a-addr )

#### stack-base
( -- a-addr )

#### stack-end
( -- a-addr )

#### rstack-base
( -- a-addr )

#### rstack-end
( -- a-addr )

#### handler
( -- a-addr )

#### >parse
( -- a-addr )

#### source
( -- b-addr u )

#### build-target
( -- a-addr )

#### sys-ram-dict-base
( -- b-addr )

#### >in
( -- a-addr )

#### input#
( -- a-addr )

#### input
( -- b-addr )

#### prompt-hook
( -- a-addr )

#### handle-number-hook
( -- a-addr )

#### failed-parse-hook
( -- a-addr )

#### emit-hook
( -- a-addr )

#### emit?-hook
( -- a-addr )

#### key-hook
( -- a-addr )

#### key?-hook
( -- a-addr )

#### refill-hook
( -- a-addr )

#### pause-hook
( -- a-addr )

#### fault-handler-hook
( -- a-addr )

#### null-handler-hook
( -- a-addr )

#### systick-handler-hook
( -- a-addr )
