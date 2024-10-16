# Single-Precision Floating-Point Support Words

On the RP2350, STM32F407, STM32L476, and STM32F746 hardware single-precision floating point is supported. (The STM32F411 does have such hardware functionality, but it is not supported to save what limited flash it has left after zeptoforth is loaded.)

The usual suspects for mathematical functions are supported, as are conversions to and from integers, fixed-point values, and strings. Also, single-precision floating-point literals in `({+,-})x.y{e,E}({+,-})z` are supported.

Note that no dedicated floating-point stack is used, unlike in some other Forths. Rather, single-precision floating-point values are cell-sized and live on the data and return stacks like any other vallues.

### `float32`

The `float32` module contains the following words:

##### `vload` ( x y z w -- )

Load floating point registers from stack.

##### `vsave` ( -- x y z w )

Save floating point registers on stack.

##### `vabs` ( f -- f' )

Get the absolute value of a single-precision floating-point value.

##### `vnegate` ( f -- f' )

Negate a single-precision floating-point value.

##### `vsqrt` ( f -- f' )

Get the square root of a single-precision floating-point value.

##### `v+` ( f1 f0 -- f' )

Add two single-precision floating-point values.

##### `v-` ( f1 f0 -- f' )

Subtract two single-precision floating-point values.

##### `v*` ( f1 f0 -- f' )

Multiply two single-precision floating-point values.

##### `v/` ( f1 f0 -- f' )

Divide two single-precision floating-point values.

##### `vmin` ( f1 f0 -- f' )

Get the minimum of two single-precision floating-point values.

##### `vmax` ( f1 f0 -- f' )

Get the maximum of two single-precision floating-point values.

##### `v<` ( f1 f0 -- flag )

Get whether a single-precision floating-point value is less.

##### `v<=` ( f1 f0 -- flag )

Get whether a single-precision floating-point value is less or equal.

##### `v=` ( f1 f0 -- flag )

Get whether a single-precision floating-point value is equal.

##### `v<>` ( f1 f0 -- flag )

Get whether a single-precision floating-point value is not equal.

##### `v>` ( f1 f0 -- flag )

Get whether a single-precision floating-point value is greater.

##### `v>=` ( f1 f0 -- flag )

Get whether a single-precision floating-point value is greater or equal.

##### `v0<` ( f -- flag )

Get whether a single-precision floating-point value is less than zero.

##### `v0<=` ( f -- flag )

Get whether a single-precision floating-point value is less than or equal to zero.

##### `v0=` ( f -- flag )

Get whether a single-precision floating-point value is equal to zero.

##### `v0<>` ( f -- flag )

Get whether a single-precision floating-point value is not equal to zero.

##### `v0>` ( f -- flag )

Get whether a single-precision floating-point value is greater than zero.

##### `v0>=` ( f -- flag )

Get whether a single-precision floating-point value is greater than or equal to zero.

##### `vnfract` ( f -- f' )

Get the non-fractional portion of a single-precision floating-point value.

##### `vfract` ( f -- f' )

Get the fractional portion of a single-precision floating-point value.

##### `vmod` ( f1 f0 -- f' )

Get the modulus of two single-precision floating-point values.

##### `f32>v` ( f32 -- f )

Convert an S15.16 fixed-point value to a single-precision floating-point value.

##### `v>f32` ( f -- f32 )

Convert a single-precision floating-point value to an S15.16 fixed-point value.

##### `v>n` ( f -- n )

Convert a single-precision floating-point value to a signed 32-bit integer rounding towards zero.

##### `v>u` ( f -- u )

Convert a single-precision floating-point value to an unsigned 32-bit integer rounding towards zero.

##### `n>v` ( n -- f )

Convert a signed 32-bit integer to a single-precision floating-point value.

##### `u>v` ( u -- f )

Convert an unsigned 32-bit integer to a single-precision floating-point value.

##### `f64>v` ( D: f64 -- f )

Convert an S31.32 fixed-point value to a single-precision floating-point value.

##### `x-nan`

NaN exception.

##### `v>f64` ( f -- f64 )

Convert a single-precision floating-point value to an S31.32 fixed-point value.

##### `format-float32-exponent` ( addr bytes f -- addr count )

Convert a single-precision floating-point value to a string in the (-)x.yez format.

##### `v1` ( -- f )

One.

##### `v-1` ( -- f )
  
Minus one.

##### `+infinity` ( -- f )

Positive infinity.

##### `-infinity` ( -- f )

Negative infinity.

##### `v>nan?` ( f -- nan? )

Get whether a floating-point value is a NaN.

##### `v>infinite?` ( f -- infinite? )

Get whether a floating-point value is infinite.

##### `(v.)` ( f -- )

Print out a single-precision floating-point value without a space.

##### `v.` ( f -- )

Print out a single-precision floating-point value with a space.

##### `parse-float32` ( addr bytes -- f success? )

Parse a single-precision floating-point value as a string.

##### `vceil` ( f -- f' )

Get the ceiling of a single-precision floating-point value.

##### `vfloor` ( f -- f' )

Get the floor of a single-precision floating-point value.

##### `vround-half-up` ( f -- f' )

Round a single-precision floating-point value to the nearest integer with half rounding up.

##### `vround-half-down` ( f -- f' )

Round a double-precision floating-point value to the nearest integer with half rounding down.

##### `vround-half-zero` ( f -- f' )

Round a single-precision floating-point value to the nearest integer with half rounding towards zero.

##### `vround-half-away-zero` ( f -- f' )

Round a single-precision floating-point value to the nearest integer with half rounding away from zero.

##### `vround-half-even` ( f -- f' )

Round a single-precision floating-point value to the nearest integer with half rounding towards even.

##### `vround-half-odd` ( f -- f' )

Round a single-precision floating-point value to the nearest integer with half rounding towards odd.

##### `vround-zero` ( f -- f' )

Round a single-precision floating-point value towards zero.

##### `vround-away-zero` ( f -- f' )

Round a single-precision floating-point value away from zero.

##### `vpi` ( -- f )

Pi as a single-precision floating-point value.

##### `vi**` ( f exponent -- f' )

Exponentiate a single-precision floating-point value by an integer.

##### `vexpm1` ( f -- f' )

Get the (e^x)-1 of a single-precision floating-point value.

##### `vexp` ( f -- f' )

Get the e^x of a single-precision floating-point value.

##### `x-domain-error` ( -- )

Domain error exception.

##### `vlnp1` ( f -- f' )

Get the ln(x+1) of a single-precision floating-point value.

##### `vln` ( f -- f' )

Get the ln(x) of a single-precision floating-point value.

##### `vsin` ( f -- f' )

Get the sine of a single-precision floating-point value.

##### `vcos` ( f -- f' )

Get the cosine of a single-precision floating-point value.

##### `vtan` ( f -- f' )

Get the tangent of a single-precision floating-point value.

##### `vatan` ( x1 -- f )

Get the arctangent of a single-precision floating-point value.

##### `vatan2` ( fy fx -- fangle )

Get the angle of an x and a y single-precision floating-point values.

##### `vasin` ( f -- f' )

Get the arcsine of a single-precision floating-point value.

##### `vacos` ( f -- f' )

Get the arccosine of a single-precision floating-point value.

##### `v**` ( fb fx -- fb^fx )

Exponentiate two single-precision floating-point values.

##### `vsinh` ( f -- f' )

Get the hyperbolic sine of an S15.16 fixed-point number.

##### `vcosh` ( f -- f' )

Get the hyperbolic cosine of an S15.16 fixed-point number.

##### `vtanh` ( f -- f' )

Get the hyperbolic tangent of an S15.16 fixed-point number.

##### `vasinh` ( f -- f' )

Get the hyperbolic arcsine of an S15.16 fixed-point number.

##### `vacosh` ( f -- f' )

Get the hyperbolic arccosine of an S15.16 fixed-point number.

##### `vatanh` ( f -- f' )

Get the hyperbolic arctangent of an S15.16 fixed-point number
