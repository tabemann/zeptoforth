# Complex Numbers Based on Single-Precision Floating-Point Values

There is optional support on platforms for which hardware single-precision floating point is supported for complex numbers. Complex numbers are implemented as pairs of cells containing single-precision floating-point values on the stack, with the real component being on the top of the stack and the imaginary componet being beneath it.

An array of basic mathematical functions are implemented for complex numbers based on single-precision floating-point values. They are in the `complex-float32` module.

There is also support for complex literals, which have the format of `x{+,-}yi` where `x` and `y` are floating-point values (but where the `e` is optional).

### `complex-float32`

The `complex-float32` module contains the following words:

##### `x-domain-error`
( -- )

Domain error.
  
##### `cvimag`
( f -- complex )

Create an imaginary number.

##### `cvreal`
( f -- complex )

Create a real number.
  
##### `cv+`
( D: complex0 D: complex1 -- D: complex2 )

Add two complex numbers.

##### `cv-`
( D: complex0 D: complex1 -- D: complex2 )

Subtract two complex numbers.

##### `cv*`
( D: complex0 D: complex1 -- D: complex2 )

Multiply two complex numbers.

##### `cv/`
( D: complex0 D: complex1 -- D: complex2 )

Divide two complex number.

##### `cvnegate`
( D: complex0 -- D: complex1 )

Negate a complex number.

##### `cvexp`
( D: complex0 -- D: complex1 )

Get the natural exponent of a complex number.

##### `cvabs`
( D: complex0 -- f )

Get the absolute value of a complex number.

##### `cvarg`
( D: complex0 -- f )

Get the argument function of a complex number.

##### `cvsqrt`
( D: complex0 -- D: complex1 )

Get the square root of a complex number.
  
##### `cvln`
( D: complex0 -- D: complex1 )

Get the principal value of the natural logarithm of a complex value.

##### `cv**`
( D: complex0 D: complex1 -- D: complex2 )

The power function.
  
##### `cvn**`
( D: complex0 D: complex1 n -- D: complex2 )

The generalized power function, taking a value n due to the multivariant
  \ nature of the natural logarithm.
  
##### `cvsin`
( D: complex0 -- D: complex1 )

Get the sine of a complex number.

##### `cvcos`
( D: complex0 -- D: complex1 )

Get the cosine of a complex number.

##### `cvtan`
( D: complex0 -- D: complex1 )

Get the tangent of a complex number.

##### `cvasin`
( D: complex0 -- D: complex1 )

Get the principal value of the arcsine of a complex value.

##### `cvacos`
( D: complex0 -- D: complex1 )

Get the principal value of the arccosine of a complex value.

##### `cvatan`
( D: complex0 -- D: complex1 )

Get the principal value of the arctangent of a complex value.

##### `cvsinh`
( D: complex0 -- D: complex1 )

Get the principal value of the hyperbolic sine of a complex value.

##### `cvcosh`
( D: complex0 -- D: complex1 )

Get the principal value of the hyperbolic cosine of a complex value.

##### `cvtanh`
( D: complex0 -- D: complex1 )

Get the principal value of the hyperbolic tangent of a complex value.

##### `cvasinh`
( D: complex0 -- D: complex1 )

Get the principal value of the hyperbolic arcsine of a complex value.

##### `cvacosh`
( D: complex0 -- D: complex1 )

Get the principal value of the hyperbolic arccosine of a complex value.

##### `cvatanh`
( D: complex0 -- D: complex1 )

Get the principal value of the hyperbolic arctangent of a complex value.

##### `format-complex-float32-exponent`
( addr bytes D: complex -- addr' bytes' )

Format a complex value as a string.

##### `(cv.)`
( D: complex -- )

Print out a complex value without a space.

##### `cv.`
( D: complex -- )

Print out a complex value with a space.

##### `parse-complex-float32`
( addr bytes -- D: complex success? )

Parse a complex value as a string.
