\ Copyright (c) 2023-2025 Travis Bemann
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

begin-module mandelbrot
  
  picocalc-term import
  pixmap8 import
  st7365p-8-common import
  float32 import
  
  32 constant max-iteration
  
  max-iteration 1+ cell align buffer: colors
  
  : init-colors ( -- )
    max-iteration 1+ 0 ?do
      i u>v max-iteration 1- u>v v/ 1e0 v+ vln
      2e0 vln v/ 255e0 v* v>u colors i + c!
    loop
  ;
  
  initializer init-colors
  
  : iteration>color { iteration -- color }
    iteration max-iteration < if
      0 colors iteration + c@ 0 rgb8
    else
      0 0 255 rgb8
    then
  ;
  
  : draw ( xa xb ya yb -- )
    [: { xa xb ya yb display }
      display clear-pixmap
      xb xa v- { x-mult }
      yb ya v- { y-mult }
      display dim@ { width height }
      height 0 ?do
        width 0 ?do
          i u>v width u>v v/ x-mult v* xa v+ { x0 }
          j u>v height u>v v/ y-mult v* ya v+ { y0 }
          0e0 0e0 { x y }
          0 { iteration }
          begin
            x dup v* y dup v* v+ 4e0 v<=
            iteration max-iteration < and
          while
            x dup v* y dup v* v- x0 v+ { xtemp }
            x y v* 2e0 v* y0 v+ to y
            xtemp to x
            1 +to iteration
          repeat
          iteration iteration>color
          i height j - display draw-pixel-const
        loop
        display update-display
      loop
    ;] with-term-display
  ;
  
  : test ( -- ) -2e0 0.47e0 -1.12e0 1.12e0 draw ;
  
end-module

    
