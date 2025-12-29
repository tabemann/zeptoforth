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

begin-module ulam
  
  picocalc-term import
  pixmap8 import
  st7365p-8-common import
  
  : sieve-size ( u -- bytes ) 1- 2 / 8 align 8 / ;
  
  : sieve-set? { u sieve -- flag }
    u 2 < if false exit then
    u 2 = if true exit then
    u 1- 1 rshift dup 7 and bit swap 3 rshift sieve + cbit@
  ;
  
  : clear-sieve { u sieve -- }
    u 1 and 0= if exit then
    u 1- 1 rshift dup 7 and bit swap 3 rshift sieve + cbic!
  ;
  
  : sieve { u sieve -- }
    sieve u sieve-size $FF fill
    u 3 < if exit then
    u 3 ?do
      i sieve sieve-set? if
        i dup + u <= if
          u 1+ i dup + ?do
            i sieve clear-sieve
          j +loop
        then
      then
    2 +loop
  ;
  
  : sieve. { u sieve -- }
    2 .
    u 3 < if exit then
    u 1+ 3 ?do
      i sieve sieve-set? if i . then
    2 +loop
  ;
  
  : spiral-coord { n -- x y }
    n s>f sqrt 1,0 d- 2,0 f/ ceil { k }
    k 1 lshift 1+ { j }
    j j * { m }
    j 1- { j }
    n m j - >= if k m n - - k negate exit then
    j negate +to m
    n m j - >= if k negate dup m n - + exit then
    j negate +to m
    n m j - >= if k negate m n - + k exit then
    k dup negate m n - j - +
  ;
  
  : spiral-coord' { n -- x y }
    n spiral-coord { x y }
    term-pixels-dim@ { width height }
    width 1 rshift x + height 1 rshift y -
  ;
  
  0 255 0 rgb8 constant spiral-color
  
  : draw-spiral ( -- )
    term-pixels-dim@ * 1+ dup sieve-size [: { u my-sieve }
      u my-sieve sieve
      page
      [: { display }
        display clear-pixmap
        display update-display
      ;] with-term-display
      u my-sieve [: { u my-sieve display }
        spiral-color 2 spiral-coord' display draw-pixel-const
        u 3 >= if
          u 1+ 3 ?do
            i my-sieve sieve-set? if
              spiral-color i spiral-coord'
              display draw-pixel-const
            then
          2 +loop
        then
        display update-display
      ;] with-term-display
      begin key? until
      key drop
    ;] with-allot
  ;
  
end-module
