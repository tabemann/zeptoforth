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

begin-module turtle-color-skewed-square

  turtle import

  : angle-color { degrees -- r g b }
    degrees 360 umod to degrees
    degrees 60 /mod { remainder quotient }
    remainder 255 * 60 / { shade }
    quotient case
      0 of 255 shade 0 endof
      1 of 255 shade - 255 0 endof
      2 of 0 255 shade endof
      3 of 0 255 shade - 255 endof
      4 of shade 0 255 endof
      5 of 255 0 255 shade - endof
    endcase
  ;

  : color-skewed-square ( -- )
    0,0 { D: color }
    0 begin dup 200 < while
      color round-zero angle-color setpencolor
      3,6 +to color
      dup forward
      91 left
      2 +
    repeat
    drop
  ;

end-module

