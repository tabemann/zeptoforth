\ Copyright (c) 2026 Travis Bemann
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

begin-module hilbert

  turtle import

  create colors
  0 c, 0 c, 0 c,
  0 c, 0 c, 255 c,
  0 c, 64 c, 0 c,
  0 c, 64 c, 255 c,
  0 c, 128 c, 0 c,
  0 c, 128 c, 255 c,
  0 c, 255 c, 0 c,
  0 c, 255 c, 255 c,
  255 c, 0 c, 0 c,
  255 c, 0 c, 255 c,
  255 c, 64 c, 0 c,
  255 c, 64 c, 255 c,
  255 c, 128 c, 0 c,
  255 c, 128 c, 255 c,
  255 c, 255 c, 0 c,
  255 c, 255 c, 255 c,
  cell align,

  1,0 2value color

  : set-color { index -- }
    index 3 * to index
    colors index + c@ colors index 1+ + c@ colors index 2 + + c@ setpencolor
  ;

  defer hilbert-unit
  :noname { level angle length -- }
    key? if exit then
    level 0= if exit then
    0,1 color d+ to color
    color 14,0 fmod 1,0 d+ round-zero set-color
    angle right
    level 1- angle negate length hilbert-unit
    length forward
    angle left
    level 1- angle length hilbert-unit
    length forward
    level 1- angle length hilbert-unit
    angle left
    length forward
    level 1- angle negate length hilbert-unit
    angle right
  ; is hilbert-unit

  : setup-turtle ( x y -- ) page home clear penup setxy pendown ;
  
  : drop-keys ( -- ) begin key? while key drop repeat ;
  
  : draw-hilbert1 ( -- )
    -150 -50 setup-turtle
    5 90 6 hilbert-unit
    drop-keys
  ;
  
  : draw-hilbert2 ( -- )
    -70 -60 setup-turtle
    6 90 2 hilbert-unit
    drop-keys
  ;
  
end-module
