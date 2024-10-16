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

float32 import

vpi 2e0 v* constant v2pi

: 2pi-fract ( n -- f ) n>v 16e0 v/ v2pi v* ;

: do-assorted-float32-test
  { D: forward-str forward-xt D: reverse-str reverse-xt -- }
  16 0 ?do
    i 2pi-fract
    cr dup v.
    forward-str type space forward-xt execute dup v.
    reverse-str type space reverse-xt execute v.
  loop
;

: do-vatan2-test
  16 0 ?do
    i 2pi-fract
    cr dup v.
    ." vsin " dup vsin dup v.
    ." vcos " swap vcos dup v.
    ." vatan2 " vatan2 v.
  loop
;

: do-pair-test { x y D: str xt -- }
  cr x v. y v. str type space x y xt execute v.
;

: do-single-test { x D: str xt -- }
  cr x v. str type space x xt execute v.
;

: do-fract-test { D: str xt -- }
  2e0 str xt do-single-test
  1.5e0 str xt do-single-test
  1.25e0 str xt do-single-test
  1e0 str xt do-single-test
  0.5e0 str xt do-single-test
  0.25e0 str xt do-single-test
  0e0 str xt do-single-test
  -2e0 str xt do-single-test
  -1.5e0 str xt do-single-test
  -1.25e0 str xt do-single-test
  -1e0 str xt do-single-test
  -0.5e0 str xt do-single-test
  -0.25e0 str xt do-single-test
;

: assorted-float32-test
  s" vexp" ['] vexp s" vln" ['] vln do-assorted-float32-test
  s" vsin" ['] vsin s" vasin" ['] vasin do-assorted-float32-test
  s" vcos" ['] vcos s" vacos" ['] vacos do-assorted-float32-test
  s" vtan" ['] vtan s" vatan" ['] vatan do-assorted-float32-test
  s" vsinh" ['] vsinh s" vasinh" ['] vasinh do-assorted-float32-test
  s" vcosh" ['] vcosh s" vacosh" ['] vacosh do-assorted-float32-test
  s" vtanh" ['] vtanh s" vatanh" ['] vatanh do-assorted-float32-test
  do-vatan2-test
  1e0 s" vnegate" ['] vnegate do-single-test
  0e0 s" vnegate" ['] vnegate do-single-test
  -1e0 s" vnegate" ['] vnegate do-single-test
  1e0 s" vabs" ['] vabs do-single-test
  0e0 s" vabs" ['] vabs do-single-test
  -1e0 s" vabs" ['] vabs do-single-test
  0e0 1e0 s" vmax" ['] vmax do-pair-test
  1e0 0e0 s" vmax" ['] vmax do-pair-test
  0e0 1e0 s" vmin" ['] vmin do-pair-test
  1e0 0e0 s" vmin" ['] vmin do-pair-test
  s" vnfract" ['] vnfract do-fract-test
  s" vfract" ['] vfract do-fract-test
;

float32 unimport