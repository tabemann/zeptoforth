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

begin-module plant

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
  
  begin-structure frame-size
    2field: frame-x
    2field: frame-y
    field: frame-heading
  end-structure
  
  : push ( -- )
    here { frame }
    frame-size allot
    fgetxy frame frame-y 2! frame frame-x 2!
    getheading frame frame-heading !
  ;
  
  : pop ( -- )
    penup
    here frame-size - { frame }
    frame frame-x 2@ frame frame-y 2@ fsetxy
    frame frame-heading @ setheading
    [ frame-size negate ] literal allot
    pendown
  ;
  
  defer plant-x-unit
  defer plant-f-unit
  
  :noname { level -- }
    key? if exit then
    level 0> if
      -1 +to level
      level plant-f-unit
      25 left
      push
      push
      level plant-x-unit
      pop
      25 right
      level plant-x-unit
      pop
      25 right
      level plant-f-unit
      push
      25 right
      level plant-f-unit
      level plant-x-unit
      pop
      25 left
      level plant-x-unit
    then
  ; is plant-x-unit

  :noname { level -- }
    key? if exit then
    level 0> if
      -1 +to level
      level plant-f-unit
      level plant-f-unit
    else
      color convert-color setpencolor color color-incr d+ 1,0 fmod to color
      length forward
    then
  ; is plant-f-unit
  
  : drop-keys ( -- ) begin key? while key drop repeat ;
  
  : draw-plant ( -- )
    page home -130 -160 setxy 25 right clear hideturtle
    2 to length
    0,0 to color
    6 plant-x-unit
    showturtle
    drop-keys
  ;
  
end-module
