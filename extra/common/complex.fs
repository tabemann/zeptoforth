\ Copyright (c) 2024 Travis Bemann
\
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

begin-module complex

  float32 import

  \ Domain error
  : x-domain-error ." domain error" cr ;
  
  \ Create an imaginary number
  : imag ( f -- complex ) 0e0 ;

  \ Create a real number
  : real ( f -- complex ) 0e0 swap ;
  
  \ Add two complex numbers
  : c+ { ai ar bi br -- ci cr } ai bi v+ ar br v+ ;

  \ Subtract two complex numbers
  : c- { ai ar bi br -- ci cr } ai bi v- ar br v- ;

  \ Multiply two complex numbers
  : c* { ai ar bi br -- ci cr }
    ai br v* ar bi v* v+ ar br v* ai bi v* v-
  ;

  \ Divide two complex number
  : c/ { ai ar bi br -- ci cr }
    br dup v* bi dup v* v+ { den }
    ai br v* ar bi v* v- den v/ ar br v* ai bi v* v+ den v/
  ;

  \ Get the natural exponent of a complex number
  : cexp { ai ar -- bi br }
    ar vexp { ar-vexp } ar-vexp ai vsin v* ar-vexp ai vcos v*
  ;

  \ Get the absolute value of a complex number
  : cabs { ai ar -- f } ar dup v* ai dup v* v+ vsqrt ;

  \ Get the argument function of a complex number
  : carg { ai ar -- f }
    ai v0<> ar v0<> or averts x-domain-error
    ai ar vatan2
  ;
  
  \ Get the principal value of the natural logarithm of a complex value
  : cln { D: a -- D: b } a carg a cabs vln ;

  \ The power function
  : c** { D: c D: z -- D: cz** } c cln z c* cexp ;
  
  \ The generalized power function, taking a value n due to the multivariant
  \ nature of the natural logarithm
  : cn** { D: c D: z n -- D: cz** }
    c cln n [ 2e0 vpi v* ] literal v* imag c+ z c* cexp
  ;
  
  \ Get the sine of a complex number
  : csin { D: a -- D: b }
    1e0 imag a c* { D: a' }
    -1e0 real a' c* { D: a'' }
    a' cexp a'' cexp c- 2e0 imag c/
  ;

  \ Get the cosine of a complex number
  : ccos { D: a -- D: b }
    1e0 imag a c* { D: a' }
    -1e0 real a' c* { D: a'' }
    a' cexp a'' cexp c+ 2e0 real c/
  ;

  \ Get the tangent of a complex number
  : ctan { D: a -- D: b }
    1e0 imag a c* { D: a' }
    -1e0 real a' c* { D: a'' }
    a' cexp { D: a' }
    a'' cexp { D: a'' }
    a' a'' c- a' a'' c+ c/ -1e0 imag c*
  ;

  \ Get the principal value of the arcsine of a complex value
  : casin { D: a -- D: b }
    1e0 real a 2dup c* c- { D: a' }
    a' carg 2e0 v/ vexp a' cabs 2e0 v/ v* real a 1e0 imag c* c+ cln 1e0 imag c/
  ;

  \ Get the principal value of the arccosine of a complex value
  : cacos { D: a -- D: b }
    1e0 real a 2dup c* c- { D: a' }
    a' carg 2e0 v/ vexp a' cabs 2e0 v/ v* real 1e0 imag c* a c+ cln 1e0 imag c/
  ;

  \ Get the principal value of the arctangent of a complex value
  : catan { D: a -- D: b }
    1e0 imag a c- 1e0 imag a c+ c/ cln 2e0 imag c/
  ;

  \ Get the principal value of the hyperbolic sine of a complex value
  : csinh { D: a -- D: b }
    a cexp a -1e0 real c* cexp c- 2e0 real c/
  ;

  \ Get the principal value of the hyperbolic cosine of a complex value
  : ccosh { D: a -- D: b }
    a cexp a -1e0 real c* cexp c+ 2e0 real c/
  ;

  \ Get the principal value of the hyperbolic tangent of a complex value
  : ctanh { D: a -- D: b }
    a cexp { D: a' } a -1e0 real c* cexp { D: a'' } a' a'' c- a' a a'' c+ c/
  ;

  \ Get the principal value of the hyperbolic arcsine of a complex value
  : casinh { D: a -- D: b }
    1e0 real a 2dup c* c+ { D: a' }
    a' carg 2e0 v/ vexp a' cabs 2e0 v/ v* real a c+ cln
  ;

  \ Get the principal value of the hyperbolic arccosine of a complex value
  : cacosh { D: a -- D: b }
    a 2dup c* 1e0 real c- { D: a' }
    a' carg 2e0 v/ vexp a' cabs 2e0 v/ v* real a c+ cln
  ;

  \ Get the principal value of the hyperbolic arctangent of a complex value
  : catanh { D: a -- D: b }
    1e0 real a c+ 1e0 real a c- c/ cln 2e0 real c/
  ;
  
end-module
