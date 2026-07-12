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

begin-module gosper

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
  
  defer gosper-unit-a
  defer gosper-unit-b
  
  :noname ( level length D: color -- D: color' )
    2swap { level length }
    key? if exit then
    level 0> if
      -1 +to level
      level length 2swap gosper-unit-a
      60 right
      level length 2swap gosper-unit-b
      120 right
      level length 2swap gosper-unit-b
      60 left
      level length 2swap gosper-unit-a
      120 left
      level length 2swap gosper-unit-a
      level length 2swap gosper-unit-a
      60 left
      level length 2swap gosper-unit-b
      60 right
    else
      2dup convert-color setpencolor color-incr d+ 1,0 fmod
      length forward
    then
  ; is gosper-unit-a
  
  :noname ( level length D: color -- D: color' )
    2swap { level length }
    key? if exit then
    level 0> if
      -1 +to level
      60 left
      level length 2swap gosper-unit-a
      60 right
      level length 2swap gosper-unit-b
      level length 2swap gosper-unit-b
      120 right
      level length 2swap gosper-unit-b
      60 right
      level length 2swap gosper-unit-a
      120 left
      level length 2swap gosper-unit-a
      60 left
      level length 2swap gosper-unit-b
    else
      2dup convert-color setpencolor color-incr d+ 1,0 fmod
      length forward
    then
  ; is gosper-unit-b
  
  : drop-keys ( -- ) begin key? while key drop repeat ;
  
  : draw-gosper ( -- )
    page home -64 32 setxy clear hideturtle
    4 3 0,0 gosper-unit-a 2drop
    showturtle
    drop-keys
  ;
  
end-module
