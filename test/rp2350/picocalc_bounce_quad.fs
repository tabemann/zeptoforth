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

begin-module bounce-quad
  
  picocalc-term import
  pixmap8 import
  pixmap8-utils import
  st7365p-8-common import
  tinymt32 import
  float32 import
  systick import

  4 constant corner-count
  120 constant poly-count
  10e0 constant delta-mag
  250 constant interval
  0 255 0 rgb8 constant edge-color
  
  begin-structure coord-size
    field: coord-x
    field: coord-y
  end-structure
  
  begin-structure delta-size
    field: delta-x
    field: delta-y
  end-structure
  
  begin-structure corner-coords-size
    coord-size corner-count * +field corner-coords
  end-structure
  
  begin-structure corner-deltas-size
    delta-size corner-count * +field corner-deltas
  end-structure
  
  begin-structure polys-size
    corner-coords-size poly-count 1+ * +field poly-corners
    field: first-index
    field: last-index
  end-structure
  
  begin-structure state-size
    tinymt32-size +field prng
    field: last-tick
    polys-size +field polys
    corner-deltas-size +field deltas
  end-structure
  
  : rnd ( state -- v )
    prng tinymt32-generate-uint32 0 f64>v
  ;
  
  : init-prng { state -- }
    rng::random state prng tinymt32-init
    state prng tinymt32-prepare-example
  ;
  
  : init-coord { coord state -- }
    term-pixels-dim@ { width height }
    width n>v state rnd v* coord coord-x !
    height n>v state rnd v* coord coord-y !
  ;
  
  : init-poly-corners { poly state -- }
    corner-count 0 do
      poly i coord-size * + state init-coord
    loop
  ;
  
  : init-polys { polys state -- }
    polys poly-corners state init-poly-corners
    0 polys first-index !
    1 polys last-index !
  ;
  
  : init-delta { delta state -- }
    state rnd vpi 2e0 v* v* { angle }
    angle vcos delta-mag v* delta delta-x !
    angle vsin delta-mag v* delta delta-y !
  ;
  
  : init-deltas { state -- }
    corner-count 0 do
      state deltas i delta-size * + state init-delta
    loop
  ;
  
  : init-state { state -- }
    state init-prng
    state polys state init-polys
    state deltas state init-deltas
    systick-counter state last-tick !
  ;
  
  : last-poly { polys -- poly }
    polys last-index @ 1- dup 0< if drop poly-count then
    corner-coords-size * polys poly-corners +
  ;
  
  : new-corner { delta coord' coord -- }
    term-pixels-dim@ { width height }
    coord' coord-x @ delta delta-x @ v+ v0< if
      delta delta-x @ vabs delta delta-x !
    else
      coord' coord-x @ delta delta-x @ v+ width 1- n>v v> if
        delta delta-x @ vabs vnegate delta delta-x !
      then
    then
    coord' coord-y @ delta delta-y @ v+ v0< if
      delta delta-y @ vabs delta delta-y !
    else
      coord' coord-y @ delta delta-y @ v+ height 1- n>v v> if
        delta delta-y @ vabs vnegate delta delta-y !
      then
    then
    coord' coord-x @ delta delta-x @ v+ coord coord-x !
    coord' coord-y @ delta delta-y @ v+ coord coord-y !
  ;
  
  : new-poly { deltas poly' poly -- }
    corner-count 0 do
      deltas corner-deltas delta-size i * +
      poly' corner-coords coord-size i * +
      poly corner-coords coord-size i * +
      new-corner
    loop
  ;
  
  : add-poly { deltas polys -- }
    polys last-poly { poly' }
    polys last-index @ { index }
    polys poly-corners corner-coords-size index * + { poly }
    index 1+ poly-count 1+ umod { index' }
    index' polys last-index !
    polys first-index @ { old-index }
    old-index index' = if
      old-index 1+ poly-count 1+ umod polys first-index !
    then
    deltas poly' poly new-poly
  ;
  
  : draw-poly { poly display -- }
    poly corner-coords coord-size corner-count 1- * +
    { last-corner }
    corner-count 0 do
      poly corner-coords coord-size i * + { corner }
      edge-color corner coord-x @ v>n corner coord-y @ v>n
      last-corner coord-x @ v>n last-corner coord-y @ v>n
      display draw-pixel-line
      corner to last-corner
    loop
  ;
  
  : draw-polys { polys display -- }
    polys first-index @ { index }
    begin index polys last-index @ <> while
      polys poly-corners corner-coords-size index * +
      display draw-poly
      index 1+ poly-count 1+ umod to index
    repeat
  ;
  
  : run-demo ( -- )
    state-size [: { state }
      page
      state init-state
      begin key? not while
        state polys [: { polys display }
          display clear-pixmap
          polys display draw-polys
          display update-display
        ;] with-term-display
        begin
          systick-counter state last-tick @ - interval >=
        while
          state deltas state polys add-poly
          interval state last-tick +!
        repeat
        state last-tick @ systick-counter - 10 / 0 max ms
      repeat
      key drop
    ;] with-aligned-allot
  ;
  
end-module
