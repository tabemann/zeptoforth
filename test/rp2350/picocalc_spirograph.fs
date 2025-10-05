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

begin-module spirograph

  oo import
  float32 import
  picocalc-term import
  pixmap8 import
  pixmap8-utils import
  st7365p-8-common import
  
  : draw-spirograph { rf rm r step -- }
    page
    [: { display }
      display clear-pixmap
      display update-display
    ;] with-term-display
    true { first? }
    0e0 0e0 { theta theta' }
    0 0 { lx ly }
    rf rm v- { r' }
    rf r v/ { k }
    begin key? not while
      theta vcos r' v* theta' vcos r v* v+ { rx }
      theta vsin r' v* theta' vsin r v* v+ { ry }
      term-pixels-dim@ { width height }
      width 2 / rx v>n + { x }
      height 2 / ry v>n - { y }
      first? if
        x to lx
        y to ly
        false to first?
      then
      255 255 255 rgb8 lx ly x y [: { display }
        display draw-pixel-line
        display update-display
      ;] with-term-display
      theta step v+ to theta
      theta k v* to theta'
      x to lx
      y to ly
    repeat
    key drop
  ;
  
end-module
