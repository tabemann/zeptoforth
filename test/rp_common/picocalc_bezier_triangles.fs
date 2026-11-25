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

begin-module bezier-triangles-test

  oo import
  closure import
  bezier import
  flood import
  picocalc-term import
  bitmap import
  bitmap-utils import
  st7365p-8-common import
  tinymt32 import

  12 cells buffer: coefs
  <bitmap> class-size buffer: bits
  
  : my-line ( D: x0 D: y0 D: x1 D: y1 -- )
    round-half-even >r round-half-even >r round-half-even >r round-half-even >r
    $FF r> r> r> r> op-or bits draw-pixel-line
  ;
  
  : my-bezier3 ( x0 y0 x1 y1 x2 y2 -- )
    s>f coefs [ 10 cells ] literal + 2!
    s>f coefs [ 8 cells ] literal + 2!
    s>f coefs [ 6 cells ] literal + 2!
    s>f coefs [ 4 cells ] literal + 2!
    s>f coefs [ 2 cells ] literal + 2!
    s>f coefs 2!
    coefs 3 8 `my-line draw-bezier
  ;
  
  : 6dup { x0 y0 x1 y1 x2 y2 -- x0 y0 x1 y1 x2 y2 x0 y0 x1 y1 x2 y2 }
    x0 y0 x1 y1 x2 y2 x0 y0 x1 y1 x2 y2
  ;
  
  : validate-pair { x0 y0 x1 y1 -- flag } x0 x1 <> y0 y1 <> or ;
  
  : validate-coords { x0 y0 x1 y1 x2 y2 -- flag }
    x0 y0 x1 y1 validate-pair
    x1 y1 x2 y2 validate-pair and
    x2 y2 x0 y0 validate-pair and
  ;
  
  : min-max { x0 y0 x1 y1 x2 y2 -- x-min y-min x-max y-max }
    x0 x1 x2 min min y0 y1 y2 min min
    x0 x1 x2 max max y0 y1 y2 max max
  ;

  : collinear? { x0 y0 x1 y1 x2 y2 -- collinear? }
    y1 y2 - x0 * y2 y0 - x1 * y0 y1 - x2 * + + abs dup *
    x0 x1 max x2 max x0 x1 min x2 min - dup *
    y0 y1 max y2 max y0 y1 min y2 min - dup * + 3 * 1 arshift <
  ;

  : draw-filled-bezier-triangle ( color x0 y0 x1 y1 x2 y2 -- )
    6dup validate-coords not if 2drop 2drop 2drop drop exit then
    6dup min-max 2 pick - swap 3 pick - swap
    2dup bitmap-buf-size [: { cols rows buf }
      buf cols rows <bitmap> bits init-object
      { color x0 y0 x1 y1 x2 y2 x-min y-min }
      x-min negate +to x0 y-min negate +to y0
      x-min negate +to x1 y-min negate +to y1
      x-min negate +to x2 y-min negate +to y2
      x0 x1 + 1 arshift { x01 } y0 y1 + 1 arshift { y01 }
      x1 x2 + 1 arshift { x12 } y1 y2 + 1 arshift { y12 }
      x2 x0 + 1 arshift { x20 } y2 y0 + 1 arshift { y20 }
      x20 y20 x0 y0 x01 y01 my-bezier3
      x01 y01 x1 y1 x12 y12 my-bezier3
      x12 y12 x2 y2 x20 y20 my-bezier3
      x0 y0 x1 y1 x2 y2 collinear? not if
        true x0 x1 + x2 + 3 / y0 y1 + y2 + 3 / cols rows max
        bits draw-bitmap-flood
      then
      color 0 0 x-min y-min cols rows bits [: { display }
        display pixmap8::draw-rect-const-mask
        display update-display
      ;] with-term-display
      bits destroy
    ;] with-aligned-allot
  ;

  : angle-color { D: angle -- rgb8 }
    angle 2,0 pi f* f/ 6,0 f* { D: h' }
    1,0 h' 2,0 fmod 1,0 d- dabs d- 255,0 f* f>s { x }
    0,0 h' d<= h' 1,0 d< and if
      255 x 0
    then
    1,0 h' d<= h' 2,0 d< and if
      x 255 0
    then
    2,0 h' d<= h' 3,0 d< and if
      0 255 x
    then
    3,0 h' d<= h' 4,0 d< and if
      0 x 255
    then
    4,0 h' d<= h' 5,0 d< and if
      x 0 255
    then
    5,0 h' d<= if
      255 0 x
    then
    pixmap8::rgb8
  ;
  
  : prnd { D: f prng -- D: f' }
    f prng tinymt32-generate-uint32 0 f* f dmin
  ;
  
  : prnd-coord { prng -- x y }
    term-pixels-dim@ { width height }
    width 1- s>f prng prnd round-zero
    height 1- s>f prng prnd round-zero
  ;
  
  : prnd-color { prng -- rgb8 }
    pi 2,0 f* prng prnd angle-color
  ;

  : run-demo ( -- )
    tinymt32-size [: { prng }
      rng::random prng tinymt32-init
      prng tinymt32-prepare-example
      ansi-term::hide-cursor
      page
      [: dup pixmap8::clear-pixmap update-display ;] with-term-display
      begin key? not while
        prng prnd-color
        prng prnd-coord prng prnd-coord prng prnd-coord
        draw-filled-bezier-triangle
      repeat
      begin key? while key drop repeat
      ansi-term::show-cursor
    ;] with-aligned-allot
  ;

end-module
