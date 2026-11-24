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

begin-module bitmap-flood-test

  oo import
  flood import
  picocalc-term import
  bitmap import
  bitmap-utils import
  st7365p-8-common import

  : draw-filled-triangle { color x0 y0 x1 y1 x2 y2 -- }
    x0 x1 min x2 min { x-min }
    y0 y1 min y2 min { y-min }
    x0 x1 max x2 max x-min - y0 y1 max y2 max y-min - { cols rows }
    color x0 y0 x1 y1 x2 y2 x-min y-min cols rows
    cols rows bitmap-buf-size [: { cols rows buf }
      cols rows buf cols rows <bitmap> [:
        { color x0 y0 x1 y1 x2 y2 x-min y-min cols rows bits }
        $FF x0 x-min - y0 y-min - x1 x-min - y1 y-min -
        op-or bits draw-pixel-line
        $FF x1 x-min - y1 y-min - x2 x-min - y2 y-min -
        op-or bits draw-pixel-line
        $FF x2 x-min - y2 y-min - x0 x-min - y0 y-min -
        op-or bits draw-pixel-line
        true x0 x1 + x2 + 3 / y0 y1 + y2 + 3 / cols rows max 2*
        bits draw-bitmap-flood
        color 0 0 x-min y-min cols rows bits [: { display }
          display pixmap8::draw-rect-const-mask
          display st7365p-8-common::update-display
        ;] with-term-display
      ;] with-object
    ;] with-aligned-allot
  ;
   : draw-hole-triangle { color x0 y0 x1 y1 x2 y2 -- }
    x0 x1 min x2 min { x-min }
    y0 y1 min y2 min { y-min }
    x0 x1 max x2 max x-min - y0 y1 max y2 max y-min - { cols rows }
    color x0 y0 x1 y1 x2 y2 x-min y-min cols rows
    cols rows bitmap-buf-size [: { cols rows buf }
      cols rows buf cols rows <bitmap> [:
        { color x0 y0 x1 y1 x2 y2 x-min y-min cols rows bits }
        $FF 0 0 cols rows op-or bits draw-rect-const
        $FF x0 x-min - y0 y-min - x1 x-min - y1 y-min -
        op-bic bits draw-pixel-line
        $FF x1 x-min - y1 y-min - x2 x-min - y2 y-min -
        op-bic bits draw-pixel-line
        $FF x2 x-min - y2 y-min - x0 x-min - y0 y-min -
        op-bic bits draw-pixel-line
        false x0 x1 + x2 + 3 / y0 y1 + y2 + 3 / cols rows max 2*
        bits draw-bitmap-flood
        color 0 0 x-min y-min cols rows bits [: { display }
          display pixmap8::draw-rect-const-mask
          display st7365p-8-common::update-display
        ;] with-term-display
      ;] with-object
    ;] with-aligned-allot
  ;
  
end-module