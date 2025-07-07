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

begin-module sierpinski
  
  turtle import

  \ Draw a Sierpinski triangle
  defer draw-sierpinski
  :noname { len level -- }
    level 0= len 1 = or if
      3 0 do len forward 120 left loop
    else
      len 2 / to len
      level 1- to level
      len level draw-sierpinski
      penup len forward pendown
      len level draw-sierpinski
      penup len backward pendown
      60 left
      penup len forward pendown
      60 right
      len level draw-sierpinski
      60 left
      penup len backward pendown
      60 right
    then
  ; is draw-sierpinski

  \ Draw a Sierpinski triangle arranged on the screen for a PicoCalc
  : draw-sierpinski' { level -- }
    page
    penup clear 160 -160 setxy 30 setheading pendown
    320 level draw-sierpinski
  ;
  
end-module
