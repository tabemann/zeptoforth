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

begin-module levyc

  turtle import
  
  : rgb { D: r D: g D: b -- r g b }
    r 255,0 f* round-zero g 255,0 f* round-zero b 255,0 f* round-zero
  ;
  
  : convert-color { D: f -- r g b }
    f 6,0 f* to f
    f 1,0 d< if 1,0 f 0,0 rgb exit then
    f 2,0 d< if 2,0 f d- 1,0 0,0 rgb exit then
    f 3,0 d< if 0,0 1,0 f 2,0 d- rgb exit then
    f 4,0 d< if 0,0 4,0 f d- 1,0 rgb exit then
    f 5,0 d< if f 4,0 d- 0,0 1,0 rgb exit then
    1,0 0,0 6,0 f d- rgb
  ;
  
  0,1 16,0 f/ 2constant color-incr
  
  0 value length
  0,0 2value color
    
  defer levyc-unit
  :noname { level -- }
    key? if exit then
    level 0> if
      -1 +to level
      45 right
      level levyc-unit
      90 left
      level levyc-unit
      45 right
    else
      color convert-color setpencolor color color-incr d+ 1,0 fmod to color
      length forward
    then
  ; is levyc-unit
  
  : drop-keys ( -- ) begin key? while key drop repeat ;
  
  : draw-levyc ( -- )
    page home 0 -60 setxy clear hideturtle
    2 to length
    0,0 to color
    12 levyc-unit
    showturtle
    drop-keys
  ;
  
end-module
