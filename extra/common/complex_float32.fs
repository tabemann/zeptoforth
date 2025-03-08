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

begin-module complex-float32

  float32 import

  \ Domain error
  : x-domain-error ( -- ) ." domain error" cr ;
  
  \ Create an imaginary number
  : cvimag ( f -- complex ) 0e0 ;

  \ Create a real number
  : cvreal ( f -- complex ) 0e0 swap ;
  
  \ Add two complex numbers
  : cv+ { ai ar bi br -- ci cr } ai bi v+ ar br v+ ;

  \ Subtract two complex numbers
  : cv- { ai ar bi br -- ci cr } ai bi v- ar br v- ;

  \ Multiply two complex numbers
  : cv* { ai ar bi br -- ci cr }
    ai br v* ar bi v* v+ ar br v* ai bi v* v-
  ;

  \ Divide two complex number
  : cv/ { ai ar bi br -- ci cr }
    br dup v* bi dup v* v+ { den }
    ai br v* ar bi v* v- den v/ ar br v* ai bi v* v+ den v/
  ;

  \ Negate a complex number
  : cvnegate { ai ar -- bi br } ai vnegate ar vnegate ;

  \ Get the natural exponent of a complex number
  : cvexp { ai ar -- bi br }
    ar vexp { ar-vexp } ar-vexp ai vsin v* ar-vexp ai vcos v*
  ;

  \ Get the absolute value of a complex number
  : cvabs { ai ar -- f } ar dup v* ai dup v* v+ vsqrt ;

  \ Get the argument function of a complex number
  : cvarg { ai ar -- f }
    ai v0<> ar v0<> or averts x-domain-error
    ai ar vatan2
  ;

  \ Get the square root of a complex number
  : cvsqrt { D: a -- D: b }
    a cvabs vsqrt cvreal a cvarg 2e0 v/ cvimag cvexp cv*
  ;
  
  \ Get the principal value of the natural logarithm of a complex value
  : cvln { D: a -- D: b } a cvarg a cvabs vln ;

  \ The power function
  : cv** { D: c D: z -- D: cz** } c cvln z cv* cvexp ;
  
  \ The generalized power function, taking a value n due to the multivariant
  \ nature of the natural logarithm
  : cvn** { D: c D: z n -- D: cz** }
    c cvln n [ 2e0 vpi v* ] literal v* cvimag cv+ z cv* cvexp
  ;
  
  \ Get the sine of a complex number
  : cvsin { D: a -- D: b }
    1e0 cvimag a cv* { D: a' }
    -1e0 cvreal a' cv* { D: a'' }
    a' cvexp a'' cvexp cv- 2e0 cvimag cv/
  ;

  \ Get the cosine of a complex number
  : cvcos { D: a -- D: b }
    1e0 cvimag a cv* { D: a' }
    -1e0 cvreal a' cv* { D: a'' }
    a' cvexp a'' cvexp cv+ 2e0 cvreal cv/
  ;

  \ Get the tangent of a complex number
  : cvtan { D: a -- D: b }
    1e0 cvimag a cv* { D: a' }
    -1e0 cvreal a' cv* { D: a'' }
    a' cvexp { D: a' }
    a'' cvexp { D: a'' }
    a' a'' cv- a' a'' cv+ cv/ -1e0 cvimag cv*
  ;

  \ Get the principal value of the arcsine of a complex value
  : cvasin { D: a -- D: b }
    1e0 cvreal a 2dup cv* cv- cvsqrt a 1e0 cvimag cv* cv+ cvln -1e0 cvimag cv*
  ;

  \ Get the principal value of the arccosine of a complex value
  : cvacos { D: a -- D: b }
    1e0 cvreal a 2dup cv* cv- cvsqrt 1e0 cvimag cv* a cv+ cvln -1e0 cvimag cv*
  ;

  \ Get the principal value of the arctangent of a complex value
  : cvatan { D: a -- D: b }
    1e0 cvimag a cv- 1e0 cvimag a cv+ cv/ cvln 2e0 cvimag cv/
  ;

  \ Get the principal value of the hyperbolic sine of a complex value
  : cvsinh { D: a -- D: b }
    a cvexp a -1e0 cvreal cv* cvexp cv- 2e0 cvreal cv/
  ;

  \ Get the principal value of the hyperbolic cosine of a complex value
  : cvcosh { D: a -- D: b }
    a cvexp a -1e0 cvreal cv* cvexp cv+ 2e0 cvreal cv/
  ;

  \ Get the principal value of the hyperbolic tangent of a complex value
  : cvtanh { D: a -- D: b }
    a cvexp { D: a' } a -1e0 cvreal cv* cvexp { D: a'' }
    a' a'' cv- a' a'' cv+ cv/
  ;

  \ Get the principal value of the hyperbolic arcsine of a complex value
  : cvasinh { D: a -- D: b }
    1e0 cvreal a 2dup cv* cv+ cvsqrt a cv+ cvln
  ;

  \ Get the principal value of the hyperbolic arccosine of a complex value
  : cvacosh { D: a -- D: b }
    a 1e0 cvreal cv+ cvsqrt a 1e0 cvreal cv- cvsqrt cv* a cv+ cvln
  ;

  \ Get the principal value of the hyperbolic arctangent of a complex value
  : cvatanh { D: a -- D: b }
    1e0 cvreal a cv+ 1e0 cvreal a cv- cv/ cvln 2e0 cvreal cv/
  ;

  \ Format a complex value as a string
  : format-complex-float32-exponent { addr bytes ai ar -- addr' bytes' }
    addr bytes ar format-float32-exponent { addr' bytes' }
    bytes bytes' - 0> if
      ai v0>= if [char] + addr' bytes' + c! 1 +to bytes' then
      addr bytes' + bytes bytes' - ai format-float32-exponent { addr'' bytes'' }
      bytes bytes' bytes'' + - 0> if
        [char] i addr'' bytes'' + c! 1 +to bytes''
      then
      addr bytes' bytes'' +
    else
      addr bytes'
    then
  ;

  \ Print out a complex value without a space
  : (cv.) ( D: a -- )
    31 [: -rot 31 -rot format-complex-float32-exponent type ;] with-allot
  ;

  \ Print out a complex value with a space
  : cv. ( D: a -- ) (cv.) space ;

  \ Parse a complex value as a string
  : parse-complex-float32 { addr bytes -- D: a success? }
    bytes 0<= if 0e0 0e0 false exit then
    addr bytes + 1- c@ dup [char] i = swap [char] I = or not if
      0e0 0e0 false exit
    then
    -1 +to bytes
    bytes { index }
    begin index 1 u> while
      -1 +to index
      addr index + c@ dup [char] + = swap [char] - = or if
        addr index + 1- c@ dup [char] e = swap [char] E = or not if
          addr index + bytes index - parse-float32 if
            addr index parse-float32 if
              true exit
            else
              2drop 0e0 0e0 false exit
            then
          else
            drop 0e0 0e0 false exit
          then
        then
      then
    repeat
    0e0 0e0 false
  ;

  begin-module complex-float32-internal

    \ Saved handle number hook
    variable saved-handle-number-hook
  
    \ Handle numeric literals
    : do-handle-number { addr bytes -- flag }
      addr bytes saved-handle-number-hook @ execute if
        true
      else
        addr bytes parse-complex-float32 if
          state @ if
            swap lit, lit, true
          else
            true
          then
        else
          false
        then
      then
    ;
    
    \ Initialize
    : init-handle-number ( -- )
      handle-number-hook @ saved-handle-number-hook !
      ['] do-handle-number handle-number-hook !
    ;

    initializer init-handle-number

  end-module
  
end-module
