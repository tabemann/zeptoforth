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

begin-module dragon
  
  turtle import
  
  variable dragon-size
  
  defer dcr
  defer dcl
  
  :noname { level -- }
    level 0> if
      -1 +to level
      45 right level dcr 90 left level dcl 45 right
    else
      dragon-size @ forward
    then
  ; is dcr

  :noname { level -- }
    level 0> if
      -1 +to level
      45 left level dcr 90 right level dcl 45 left
    else
      dragon-size @ forward
    then
  ; is dcl
  
  : draw-dragon ( size level -- )
    swap dragon-size !
    penup
    hideturtle
    page
    clear
    home
    pendown
    dcr
    showturtle
  ;
  
end-module