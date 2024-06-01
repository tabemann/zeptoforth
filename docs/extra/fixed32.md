# S15.16 Fixed Point Numbers

32-bit, or more specifically S15.16 (15 bits before the decimal point, 16 bits after the decimal point, one bit for the sign, two's complement) fixed-point numbers are optionally supported by zeptoforth. They are not in any builds, but are available to be loaded by the user, both into RAM and into flash.

### `fixed32`

The `fixed2` module contains the following words:

##### `f32*`
( x y -- z )

Multiply two S15.16 fixed-point numbers

##### `f32/`
( x y -- z )

Divide an S15.16 fixed-point number by another

##### `f32>s`
( x -- y )

Convert an S15.16 fixed-point number to a 16-bit integer

##### `s>f32`
( x -- y )

Convert a 16-bit integer to an S15.16 fixed-point number

##### `f64>f32`
( D: x -- y )

Convert an S31.32 fixed-point number to an S15.16 fixed-point number

##### `f32>f64`
( x -- D: y )

Convert an S15.16 fixed-point number to an S31.32 fixed-point number

##### `f32mod`
( x y -- z )

Calculate the modulus of two S15.16 fixed-point numbers

##### `f32ceil`
( f -- n )

Get the ceiling of an S15.16 fixed-point number as a single-cell number.

##### `f32floor`
( f -- n )

Get the floor of an S15.16 fixed-point number as a single-cell number.

##### `f32round-half-up`
( f -- n )

Round an S15.16 fixed-point number up to the nearest integer with half rounding up.

##### `f32round-half-down`
( f -- n )

Round an S15.16 fixed-point number down to the nearest integer with half rounding down.

##### `f32round-half-zero`
( f -- n )

Round an S15.16 fixed-point number to the nearest integer with half rounding towards zero.

##### `f32round-half-away-zero`
( f -- n )

Round an S15.16 fixed-point number to the nearest integer with half rounding away from zero.

##### `f32round-half-even`
( f -- n )

Round an S15.16 fixed-point number to the nearest integer with half rounding towards even.

##### `f32round-half-odd`
( f -- n )

Round an S15.16 fixed-point number to the nearest integer with half rounding towards even.

##### `f32round-zero`
( f -- n )

Round an S15.16 fixed-point number towards zero.

##### `f32round-away-zero`
( f -- n )

Round an S15.16 fixed-point number away from zero.

##### `f32pi`
( -- pi )

Pi as a S15.16 fixed-point number

##### `f32sqrt`
( x -- y )

Get the square root of an S15.16 fixed-point number
  
##### `f32i**`
( f32 exponent -- f32' )

Exponentiate an S15.16 fixed-point number by an integer

##### `f32expm1`
( f32 -- f32' )

Get the (e^x)-1 of an S15.16 fixed-point number
  
##### `f32exp`
( f32 -- f32' )

Get the e^x of an S15.16 fixed-point number

##### `f32lnp1`
( f32 -- f32' )

Get the ln(x+1) of an S15.16 fixed-point number
  
##### `f32ln`
( f32 -- f32' )

Get the ln(x) of an S15.16 fixed-point number

##### `f32sin`
( f32 -- f32' )

Get the sine of an S15.16 fixed-point number
  
##### `f32cos`
( f32 -- f32' )

Get the cosine of an S15.16 fixed-point number

##### `f32tan`
( f32 -- f32' )

Get the tangent of an S15.16 fixed-point number

##### `f32atan`
( f32 -- f32' )

Get the arctangent of an S15.16 fixed-point number
  
##### `f32atan2`
( f32x f32y -- f32angle )

Get the angle of an x and an y S15.16 fixed-point numbers
  
##### `f32asin`
( f32 -- f32' )

Get the arcsine of an S15.16 fixed-point number
  
##### `f32acos`
( f32 -- f32' )

Get the arccosine of an S15.16 fixed-point number
  
##### `f32**`
( f32b f32x -- f32b^f32x )

Exponentiate two S15.16 fixed-point numbers

##### `f32sinh`
( f32 -- f32' )

Get the hyperbolic sine of an S15.16 fixed-point number
  
##### `f32cosh`
( f32 -- f32' )

Get the hyperbolic cosine of an S15.16 fixed-point number

##### `f32tanh`
( f32 -- f32' )

Get the hyperbolic tangent of an S15.16 fixed-point number

##### `f32asinh`
( f32 -- f32' )

Get the hyperbolic arcsine of an S15.16 fixed-point number

##### `f32acosh`
( f32 -- f32' )

Get the hyperbolic arccosine of an S15.16 fixed-point number

##### `f32atanh`
( f32 -- f32' )

Get the hyperbolic arctangent of an S15.16 fixed-point number

##### `parse-f32`
( c-addr bytes -- f32 success? )

Parse a 32-bit fixed-point number

##### `format-f32`
( c-addr f32 -- c-addr bytes )

Format an S15.16 number

##### `format-f32-truncate`
( c-addr f32 places -- c-addr bytes )

Format a S15,16 number truncated to *places* to the right of the decimal point
  
##### `(f32.)`
( f32 -- )

Type an s15.16 fixed-point number without a following space

##### `(f32.n)`
( f32 places -- )

Type a s15.16 fixed-point number truncated to *places* to the right of the decimal point without a following space

##### `f32.`
( f32 -- )

Type an s15.16 fixed-point number with a following space

##### `f32.n`
( f32 places -- )

Type a s15.16 fixed-point number truncated to *places* to the right of the decimal point with a following space
