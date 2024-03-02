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

begin-module fixed32-conv-test

  fixed32 import

  \ Verify a result
  : verify ( n1 n0 -- )
    over = if ."  PASS" drop else ."  FAIL (" (.) ." )" then
  ;

  \ Run a subtest
  : subtest { f32-xt f64-xt D: name -- }
    -3,0 { D: n }
    begin n 3,0 d<= while
      n f64>f32 { m }
      n f64-xt execute { n' }
      cr m f32. name type space n' . ." ="
      m f32-xt execute n' verify
      0,25 +to n
    repeat
  ;
  
  \ Run the test
  : test ( -- )
    ['] f32ceil ['] ceil
    s" f32ceil" subtest
    ['] f32floor ['] floor
    s" f32floor" subtest
    ['] f32round-half-up ['] round-half-up
    s" f32round-half-up" subtest
    ['] f32round-half-down ['] round-half-down
    s" f32round-half-down" subtest
    ['] f32round-half-zero ['] round-half-zero
    s" f32round-half-zero" subtest
    ['] f32round-half-away-zero ['] round-half-away-zero
    s" f32round-half-away-zero" subtest
    ['] f32round-half-even ['] round-half-even
    s" f32round-half-even" subtest
    ['] f32round-half-odd ['] round-half-odd
    s" f32round-half-odd" subtest
    ['] f32round-zero ['] round-zero
    s" f32round-zero" subtest
    ['] f32round-away-zero ['] round-away-zero
    s" f32round-away-zero" subtest
  ;
  
end-module
