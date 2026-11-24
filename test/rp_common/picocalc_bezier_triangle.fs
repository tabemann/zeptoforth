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

begin-module bezier-flood-test

  oo import
  closure import
  bezier import
  flood import
  picocalc-term import
  bitmap import
  bitmap-utils import
  st7365p-8-common import

  : my-line { D: x0 D: y0 D: x1 D: y1 dst -- }
    $FF x0 round-half-even y0 round-half-even
    x1 round-half-even y1 round-half-even op-or dst draw-pixel-line
  ;
  
  : my-bezier3 ( x0 y0 x1 y1 x2 y2 dst -- )
    12 cells [: { x0 y0 x1 y1 x2 y2 dst coefs }
      x0 s>f coefs 2!
      y0 s>f coefs [ 2 cells ] literal + 2!
      x1 s>f coefs [ 4 cells ] literal + 2!
      y1 s>f coefs [ 6 cells ] literal + 2!
      x2 s>f coefs [ 8 cells ] literal + 2!
      y2 s>f coefs [ 10 cells ] literal + 2!
      coefs dst `my-line [: { coefs line-xt }
        coefs 3 12 line-xt draw-bezier
      ;] with-closure
    ;] with-aligned-allot
  ;

  : draw-filled-bezier-triangle { color x0 y0 x1 y1 x2 y2 -- }
    x0 x1 min x2 min { x-min }
    y0 y1 min y2 min { y-min }
    x0 x1 max x2 max x-min - y0 y1 max y2 max y-min - { cols rows }
    color x0 y0 x1 y1 x2 y2 x-min y-min cols rows
    cols rows bitmap-buf-size [: { cols rows buf }
      cols rows buf cols rows <bitmap> [:
        { color x0 y0 x1 y1 x2 y2 x-min y-min cols rows bits }
        x-min negate +to x0 y-min negate +to y0
        x-min negate +to x1 y-min negate +to y1
        x-min negate +to x2 y-min negate +to y2
        x0 x1 + 1 arshift { x01 } y0 y1 + 1 arshift { y01 }
        x1 x2 + 1 arshift { x12 } y1 y2 + 1 arshift { y12 }
        x2 x0 + 1 arshift { x20 } y2 y0 + 1 arshift { y20 }
        x20 y20 x0 y0 x01 y01 bits my-bezier3
        x01 y01 x1 y1 x12 y12 bits my-bezier3
        x12 y12 x2 y2 x20 y20 bits my-bezier3
        true x0 x1 + x2 + 3 / y0 y1 + y2 + 3 / cols rows max 2*
        bits draw-bitmap-flood
        color 0 0 x-min y-min cols rows bits [: { display }
          display pixmap8::draw-rect-const-mask
          display st7365p-8-common::update-display
        ;] with-term-display
      ;] with-object
    ;] with-aligned-allot
  ;

end-module
