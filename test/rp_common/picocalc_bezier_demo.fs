\ Copyright (c) 2023-2026 Travis Bemann
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

begin-module bezier-demo

  bezier import
  picocalc-term import
  pixmap8 import
  pixmap8-utils import
  st7365p-8-common import
  closure import
  
  : my-line { D: x0 D: y0 D: x1 D: y1 color dst -- }
    color x0 round-half-even y0 round-half-even
    x1 round-half-even y1 round-half-even dst draw-pixel-line
  ;
  
  : my-bezier3 ( r g b x0 y0 x1 y1 x2 y2 -- )
    12 cells [: { r g b x0 y0 x1 y1 x2 y2 coefs }
      x0 s>f coefs 2!
      y0 s>f coefs [ 2 cells ] literal + 2!
      x1 s>f coefs [ 4 cells ] literal + 2!
      y1 s>f coefs [ 6 cells ] literal + 2!
      x2 s>f coefs [ 8 cells ] literal + 2!
      y2 s>f coefs [ 10 cells ] literal + 2!
      coefs r g b rgb8 [: `my-line [: { coefs line-xt }
        coefs 3 12 line-xt draw-bezier
      ;] with-dclosure ;] with-term-display
      `update-display with-term-display
    ;] with-aligned-allot
  ;
  
  : my-bezier4 ( r g b x0 y0 x1 y1 x2 y2 x3 y3 -- )
    16 cells [: { r g b x0 y0 x1 y1 x2 y2 x3 y3 coefs }
      x0 s>f coefs 2!
      y0 s>f coefs [ 2 cells ] literal + 2!
      x1 s>f coefs [ 4 cells ] literal + 2!
      y1 s>f coefs [ 6 cells ] literal + 2!
      x2 s>f coefs [ 8 cells ] literal + 2!
      y2 s>f coefs [ 10 cells ] literal + 2!
      x3 s>f coefs [ 12 cells ] literal + 2!
      y3 s>f coefs [ 14 cells ] literal + 2!
      coefs r g b rgb8 [: `my-line [: { coefs line-xt }
        coefs 4 12 line-xt draw-bezier
      ;] with-dclosure ;] with-term-display
      `update-display with-term-display
    ;] with-aligned-allot
  ;
  
  : run-demo ( -- )
    refresh-term
    255 0 0 0 320 160 0 320 320 my-bezier3
    0 255 0 0 0 160 320 320 0 my-bezier3
    0 0 255 0 0 320 0 0 320 320 320 my-bezier4
    255 255 0 320 0 0 0 320 320 0 320 my-bezier4
    0 255 255 0 160 80 0 240 320 320 160 my-bezier4
    255 0 255 320 160 240 0 80 320 0 160 my-bezier4
  ;
    
end-module
