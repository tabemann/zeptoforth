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

begin-module fixed32-test

  fixed32 import
  
  \ Run S15.16 fixed-point numeric tests, comparing against S31.32 math
  : test ( -- )
    cr ." 2,0 3,0 f* f. : "
    2,0 3,0 f* f.
    
    cr ." 2;0 3;0 f32* f32. : "
    2;0 3;0 f32* f32.
    
    cr ." 2,0 3,0 f/ f. : "
    2,0 3,0 f/ f.
    
    cr ." 2;0 3;0 f32/ f32. : "
    2;0 3;0 f32/ f32.
    
    cr ." 2,5 f>s . : "
    2,5 f>s .
    
    cr ." 2;5 f32>s . : "
    2;0 f32>s .
    
    cr ." 2 s>f f. : "
    2 s>f f.
    
    cr ." 2 s>f32 f32. : "
    2 s>f32 f32.
    
    cr ." 2,5 f64>f32 f32. : "
    2,5 f64>f32 f32.
    
    cr ." 2;5 f32>f64 f. : "
    2;5 f32>f64 f.
    
    cr ." 2,5 2,0 fmod f. : "
    2,5 2,0 fmod f.
    
    cr ." 2;5 2;0 f32mod f32. : "
    2;5 2;0 f32mod f32.
    
    cr ." pi f. : "
    pi f.
    
    cr ." f32pi f32. : "
    f32pi f32.
    
    cr ." 9,0 sqrt f. : "
    9,0 sqrt f.
    
    cr ." 9;0 f32sqrt f32. : "
    9;0 f32sqrt f32.
    
    cr ." 3,0 3 fi** f. : "
    3,0 3 fi** f.
    
    cr ." 3;0 3 f32i** f32. : "
    3;0 3 f32i** f32.
    
    cr ." 2,0 exp f. : "
    2,0 exp f.
    
    cr ." 2;0 f32exp f32. : "
    2;0 f32exp f32.
    
    cr ." 2,0 exp ln f. : "
    2,0 exp ln f.
    
    cr ." 2;0 f32exp f32ln f32. : "
    2;0 f32exp f32ln f32.
    
    cr ." 0,5 sin f. : "
    0,5 sin f.
    
    cr ." 0;5 f32sin f32. : "
    0;5 f32sin f32.
    
    cr ." 0,5 cos f. : "
    0,5 cos f.
    
    cr ." 0;5 f32cos f32. : "
    0;5 f32cos f32.
    
    cr ." 0,5 tan f. : "
    0,5 tan f.
    
    cr ." 0;5 f32tan f32. : "
    0;5 f32tan f32.
    
    cr ." 0,5 asin f. : "
    0,5 asin f.
    
    cr ." 0;5 f32asin f32. : "
    0;5 f32asin f32.
    
    cr ." 0,5 acos f. : "
    0,5 acos f.
    
    cr ." 0;5 f32acos f32. : "
    0;5 f32acos f32.
    
    cr ." 0,5 atan f. : "
    0,5 atan f.
    
    cr ." 0;5 f32atan f32. : "
    0;5 f32atan f32.
    
    cr ." 1,0 3,0 atan2 f. : "
    1,0 3,0 atan2 f.
    
    cr ." 1;0 3;0 f32atan2 f32. : "
    1;0 3;0 f32atan2 f32.
    
    cr ." 0,5 sinh f. : "
    0,5 sinh f.
    
    cr ." 0;5 f32sinh f32. : "
    0;5 f32sinh f32.
    
    cr ." 0,5 cosh f. : "
    0,5 cosh f.
    
    cr ." 0;5 f32cosh f32. : "
    0;5 f32cosh f32.
    
    cr ." 0,5 tanh f. : "
    0,5 tanh f.
    
    cr ." 0;5 f32tanh f32. : "
    0;5 f32tanh f32.
    
    cr ." 0,5 asinh f. : "
    0,5 asinh f.
    
    cr ." 0;5 f32asinh f32. : "
    0;5 f32asinh f32.
    
    cr ." 3,0 acosh f. : "
    3,0 acosh f.
    
    cr ." 3;0 f32acosh f32. : "
    3;0 f32acosh f32.
    
    cr ." 0,5 atanh f. : "
    0,5 atanh f.
    
    cr ." 0;5 f32atanh f32. : "
    0;5 f32atanh f32.
    
    cr .\" s\" 2,5\" parse-fixed . f. : "
    s" 2,5" parse-fixed . f.
    
    cr .\" s\" 2;5\" parse-f32 . f32. : "
    s" 2;5" parse-f32 . f32.
    
    cr ." 1,23456 2 f.n : "
    1,23456 2 f.n
    
    cr ." 1;23456 2 f32.n : "
    1;23456 2 f32.n
  ;
    
end-module
