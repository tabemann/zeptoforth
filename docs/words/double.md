# Double-Cell Words

##### `2drop`
( d -- )

Double drop

##### `2swap`
( d1 d2 -- d2 d1 )

Double swap

##### `2over`
( d1 d2 -- d1 d2 d1 )

Double over

##### `2dup`
( d1 -- d1 d1 )

Double dup

##### `2nip`
( d1 d2 -- d2 )

Double nip

##### `2tuck`
( d1 d2 -- d2 d1 d2 )

Double tuck

##### `d=`
( d1 d2 -- f )

Test for the equality of two double cells

##### `d<>`
( d1 d2 -- f )

Test for the inequality of two double cells

##### `du<`
( ud1 ud2 -- f )

Unsigned double less than

##### `du>`
( ud1 ud2 -- f )

Unsigned double greater than

##### `du>=`
( ud1 ud2 -- f )

Unsigned double greater than or equal

##### `du<=`
( ud1 ud2 -- f )

Unsigned double less than or equal

##### `d<`
( nd1 nd2 -- f )

Signed double less than

##### `d>`
( nd1 nd2 -- f )

Signed double greater than

##### `d>=`
( nd1 nd2 -- f )

Signed double greater than or equal

##### `d<=`
( nd1 nd2 -- f )

Signed double less than or equal

##### `d0=`
( d -- f )

Double equal to zero

##### `d0<>`
( d -- f )

Double not equal to zero

##### `d0<`
( nd -- f )

Double less than zero

##### `d0>`
( nd -- f )

Double greater than zero

##### `d0>=`
( nd -- f )

Double greater than or equal to zero

##### `d0<=`
( nd -- f )

Double less than or equal to zero

##### `dnegate`
( nd1 -- nd2 )

Negate a double cell

##### `d+`
( d1 d2 -- d3 )

Add two double cells

##### `d-`
( d1 d2 -- d3 )

Subtract two double cells
	
##### `um+`
( u1 u2 -- u3 carry )

Add with carry

##### `um*`
( u1 u2 -- ud )

Multiply two unsigned 32-bit values to get an unsigned 64-bit value

##### `m*`
( n1 n2 -- d )

Multiply two signed 32-bit values to get a signed 64-bit value

##### `ud*`
( ud1 ud2 -- ud3 )

Unsigned multiply 64 * 64 = 64

##### `*/`
( n1 n2 n3 -- n4 )

Multiply signed n1 and n2 to get double cell intermediate value, then divide it by n3 to get a single cell result
	
##### `*/mod`
( n1 n2 n3 -- n4 n5 )

Multiply signed n1 and n2 to get double cell intermediate value, then divide it by n3 to get a single cell remainder n4 and quotient n5

##### `u*/`
( u1 u2 u3 -- u4 )

Multiply unsigned u1 and u2 to get double cell intermediate value, then divide it by u3 to get a single cell result
	
##### `u*/mod`
( u1 u2 u3 -- u4 u5 )

Multiply unsigned u1 and u2 to get double cell intermediate value, then divide it by u3 to get a single cell remainder u4 and quotient u5

##### `um/mod`
( ud u1 -- u2 u3 )

Divide unsigned ud by u1 and get a single cell remainder u2 and quotient u3

##### `m/mod`
( nd n1 -- n2 n3 )

Divide signed nd by n1 and get a single cell remainder n2 and quotient n3

##### `ud/mod`
( ud1 ud2 -- ud3 ud4 )

Divide unsigned ud1 by ud2 and get double cell remainder ud3 and quotient ud4

##### `d/mod`
( nd1 nd2 -- nd3 nd4 )

Divide signed nd1 by nd2 and get double cell remainder nd3 and quotient nd4

##### `ud/`
( ud1 ud2 -- ud3 )

Divide unsigned two double cells and get a double cell quotient

##### `d/`
( nd1 nd2 -- nd3 )

Divide signed two double cells and get a double cell quotient

##### `f*`
( d1 d2 -- d3 )

Multiply two s31.32 fixed-point numbers. Note that overflow is possible, where then the sign will be wrong.

##### `f/`
( d1 d2 -- d3 )

Divide two s31.32 fixed-point numbers. Note that overflow is possible, where then the sign will be wrong.

##### `udm*`
( d1 d2 -- dl dh )

Multiply two 64-bit double-cell numbers into a single 128-bit quadruple-cell number.
