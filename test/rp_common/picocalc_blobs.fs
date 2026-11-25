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

begin-module blobs-test

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

  0,0 2value interval
  0 value prng
    
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

  : draw-filled-bezier-triangle ( color display x0 y0 x1 y1 x2 y2 -- )
    6dup validate-coords not if 2drop 2drop 2drop 2drop exit then
    6dup min-max 2 pick - swap 3 pick - swap
    2dup bitmap-buf-size [: { cols rows buf }
      buf cols rows <bitmap> bits init-object
      { color display x0 y0 x1 y1 x2 y2 x-min y-min }
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
      color 0 0 x-min y-min cols rows bits
      display pixmap8::draw-rect-const-mask
      bits destroy
    ;] with-aligned-allot
  ;

  : angle-color ( D: angle -- rgb8 )
    2,0 pi f* f/ 6,0 f* { D: h' }
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
  
  : prnd { D: f -- D: f' }
    f prng tinymt32-generate-uint32 0 f* f dmin
  ;
  
  begin-structure corner-size
    2field: corner-x
    2field: corner-y
    2field: corner-dx
    2field: corner-dy
  end-structure
  
  3 constant corner-count
  
  begin-structure blob-size
    2field: blob-color
    corner-size corner-count * +field blob-corners
  end-structure
  
  : apply-delta { corner -- }
    term-pixels-dim@ { width height }
    corner corner-dx 2@ interval f* corner corner-x 2@ d+ 2dup { D: x' }
    0,0 dmax width 1- s>f dmin corner corner-x 2!
    corner corner-dy 2@ interval f* corner corner-y 2@ d+ 2dup { D: y' }
    0,0 dmax height 1- s>f dmin corner corner-y 2!
    x' 0,0 d<= if
      corner corner-dx 2@ dabs corner corner-dx 2!
    then
    x' width 1- s>f d>= if
      corner corner-dx 2@ dabs dnegate corner corner-dx 2!
    then
    y' 0,0 d<= if
      corner corner-dy 2@ dabs corner corner-dy 2!
    then
    y' height 1- s>f d>= if
      corner corner-dy 2@ dabs dnegate corner corner-dy 2!
    then
  ;
  
  : blob-corner { index blob -- corner }
    index corner-size * blob blob-corners +
  ;
  
  : apply-deltas { blob -- }
    corner-count 0 ?do
      i blob blob-corner apply-delta
    loop
  ;
  
  60,0 2constant optimal-dist
  0,1 2constant corner-force-factor
  
  : corner-force { corner1 corner0 -- D: ddx D: ddy }
    corner0 corner-x 2@ corner1 corner-x 2@ d- 2dup f*
    corner0 corner-y 2@ corner1 corner-y 2@ d- 2dup f* d+ sqrt { D: dist }
    dist dnegate optimal-dist d+ corner-force-factor f* { D: force }
    corner0 corner-x 2@ corner1 corner-x 2@ d- dist f/ force f*
    corner0 corner-y 2@ corner1 corner-y 2@ d- dist f/ force f*
  ;
  
  10,0 2constant prnd-force-max
  
  : prnd-force { -- D: ddx D: ddy }
    prnd-force-max 2,0 f* prnd prnd-force-max d-
    prnd-force-max 2,0 f* prnd prnd-force-max d-
  ;
  
  : forces+ ( D: ddx0 D: ddy0 D: ddx1 D: ddy1 -- D: ddx' D: ddy' )
    2>r 2swap 2>r d+ 2r> 2r> d+
  ;
  
  : force* ( D: ddx D: ddy D: factor -- D: dx' D: dy' )
    2>r 2r@ f* 2swap 2r> f* 2swap
  ;
  
  0,9 2constant dampen-factor
  
  : adjusted-dampen-factor ( -- D: factor )
    dampen-factor interval f**
  ;
  
  : dampen-delta ( D: dx D: dy -- D: dx' D: dy' )
    adjusted-dampen-factor 2dup 2>r f* 2swap 2r> f* 2swap
  ;
  
  : apply-forces { corner2 corner1 corner0 -- }
    corner2 corner0 corner-force
    corner1 corner0 corner-force forces+
    prnd-force forces+
    interval force*
    corner0 corner-dy 2@ d+ 2swap
    corner0 corner-dx 2@ d+ 2swap
    dampen-delta
    corner0 corner-dy 2!
    corner0 corner-dx 2!
  ;
  
  : update-deltas { blob -- }
    1 blob blob-corner 2 blob blob-corner 0 blob blob-corner apply-forces
    0 blob blob-corner 2 blob blob-corner 1 blob blob-corner apply-forces
    0 blob blob-corner 1 blob blob-corner 2 blob blob-corner apply-forces
  ;
  
  pi 4,0 f/ 2constant blob-color-rate
  
  : update-blob { blob -- }
    blob blob-color 2@ blob-color-rate interval f* d+ pi 2,0 f* fmod
    blob blob-color 2!
    blob update-deltas
    blob apply-deltas
  ;
  
  : init-blob { blob -- }
    pi 2,0 f* prnd blob blob-color 2!
    term-pixels-dim@ { width height }
    corner-count 0 ?do
      i blob blob-corner { corner }
      width 1- s>f prnd corner corner-x 2!
      height 1- s>f prnd corner corner-y 2!
      0,0 corner corner-dx 2!
      0,0 corner corner-dy 2!
    loop
  ;
  
  : draw-blob { display blob -- }
    blob blob-color 2@ angle-color
    display
    corner-count 0 ?do
      i blob blob-corner { corner }
      corner corner-x 2@ round-half-even
      corner corner-y 2@ round-half-even
    loop
    draw-filled-bezier-triangle
  ;
  
  0 0 0 pixmap8::rgb8 constant bk-color
  
  : erase-blob { display blob -- }
    bk-color
    display
    corner-count 0 ?do
      i blob blob-corner { corner }
      corner corner-x 2@ round-half-even
      corner corner-y 2@ round-half-even
    loop
    draw-filled-bezier-triangle
  ;
  
  : blob { index blobs -- blob }
    index blob-size * blobs +
  ;
  
  : cycle-blobs { display blobs blob-count -- }
    blob-count 0 ?do display i blobs blob erase-blob loop
    blob-count 0 ?do i blobs blob update-blob loop
    blob-count 0 ?do display i blobs blob draw-blob loop
  ;
  
  : init-blobs { blobs blob-count -- }
    blob-count 0 ?do i blobs blob init-blob loop
  ;
  
  : run-demo ( blob-count -- )
    tinymt32-size [:
      to prng
      dup blob-size * [: { blob-count blobs }
        rng::random prng tinymt32-init
        prng tinymt32-prepare-example
        ansi-term::hide-cursor
        page
        [: dup pixmap8::clear-pixmap update-display ;] with-term-display
        blobs blob-count init-blobs
        systick::systick-counter { last-tick }
        begin key? not while
          systick::systick-counter { next-tick }
          next-tick last-tick - s>f 10000,0 f/ to interval
          blobs blob-count [:
            { blobs blob-count display }
            display blobs blob-count cycle-blobs
            display update-display
          ;] with-term-display
          next-tick to last-tick
        repeat
        begin key? while key drop repeat
        ansi-term::show-cursor
      ;] with-aligned-allot
    ;] with-aligned-allot
  ;

end-module
