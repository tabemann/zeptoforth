\ Copyright (c) 2025 Travis Bemann
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

begin-module fern
  
  picocalc-term import
  pixmap8 import
  st7365p-8-common import
  tinymt32 import
  float32 import
  
  : draw-fern ( -- )
    tinymt32-size [: { seed prng }
      seed prng tinymt32-init
      prng tinymt32-prepare-example
      [: { display }
        display clear-pixmap
        display update-display
      ;] with-term-display
      0e0 0e0 { x y }
      begin key? not while
        x y prng [: { x y prng display }
          prng tinymt32-generate-uint32 0
          f64>v { c }
          c 0.01e0 v< if
            0e0
            0.16e0 y v*
          else
            c 0.08e0 v< if
              0.2e0 x v* 0.26e0 y v* v-
              0.23e0 x v* 0.22e0 y v* v+ 1.6e0 v+
            else
              c 0.15e0 v< if
                -0.15e0 x v* 0.28e0 y v* v+
                0.26e0 x v* 0.24e0 y v* v+ 0.44e0 v+
              else
                0.85e0 x v* 0.04e0 y v* v+
                -0.04e0 x v* 0.85e0 y v* v+ 1.6e0 v+
              then
            then
          then
          to y to x
          0 255 0 rgb8
          x 3e0 v+ 6e0 v/ 320e0 v* v>n
          320e0 y 2e0 v+ 14e0 v/ 320e0 v* v- v>n
          display draw-pixel-const
          display update-display
          x y
        ;] with-term-display
        to y to x
      repeat
      key drop
    ;] with-aligned-allot
  ;
  
end-module
