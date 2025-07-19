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

begin-module system

  picocalc-term import
  pixmap8 import
  pixmap8-utils import
  st7365p-8-common import
  float32 import
  tinymt32 import
  systick import
  oo import
  
  7 constant brush-size
  32 constant particle-count
  30e0 constant max-delta
  
  begin-structure particle-size
    field: particle-x
    field: particle-y
    field: particle-delta-x
    field: particle-delta-y
    field: particle-color
  end-structure
  
  begin-structure system-size
    tinymt32-size +field prng
    field: last-systick
    particle-size particle-count * +field particles
    brush-size dup bitmap::bitmap-buf-size
    +field brush-buf
    bitmap::<bitmap> class-size +field brush
  end-structure
  
  : urandom { u system -- u' }
    u s>f system prng tinymt32-generate-uint32 0 f*
    round-zero u min
  ;
  
  : vrandom { v system -- v' }
    v system prng tinymt32-generate-uint32 0 f64>v
    v* v vmin
  ;
  
  : init-particle { particle system -- }
    term-pixels-dim@ { width height }
    brush-size 2 / { edge-size }
    width brush-size - system urandom edge-size +
    u>v particle particle-x !
    height brush-size - system urandom edge-size +
    u>v particle particle-y !
    max-delta system vrandom { abs-delta }
    2e0 vpi v* system vrandom { angle }
    angle vcos abs-delta v*
    particle particle-delta-x !
    angle vsin abs-delta v*
    particle particle-delta-y !
    255 system urandom
    255 system urandom
    255 system urandom rgb8
    particle particle-color !
  ;
  
  : init-system { system -- }
    rng::random system prng tinymt32-init
    system prng tinymt32-prepare-example
    system brush-buf brush-size dup
    bitmap::<bitmap> system brush init-object
    system brush bitmap::clear-bitmap
    true brush-size 2 / dup dup
    bitmap::op-set system brush
    bitmap-utils::draw-filled-circle
    systick-counter system last-systick !
    particle-count 0 ?do
      i particle-size * system particles +
      system init-particle
    loop
  ;
  
  : update-particle { factor particle system -- }
    particle particle-x @
    particle particle-delta-x @ factor v* v+
    dup { x } particle particle-x ! 
    particle particle-y @
    particle particle-delta-y @ factor v* v+
    dup { y } particle particle-y !
    term-pixels-dim@ { width height }
    [ brush-size 2 / u>v ] literal { edge }
    x edge v< if
      particle particle-delta-x @ vabs
      particle particle-delta-x !
    else
      x width n>v edge v- v>= if
        particle particle-delta-x @ vabs vnegate
        particle particle-delta-x !
      then
    then
    y edge v< if
      particle particle-delta-y @ vabs
      particle particle-delta-y !
    else
      y height n>v edge v- v>= if
        particle particle-delta-y @ vabs vnegate
        particle particle-delta-y !
      then
    then
  ;
  
  : update-system { system -- }
    systick-counter { current-systick }
    current-systick system last-systick @ - u>v
    1000e0 v/ { factor }
    particle-count 0 ?do
      factor
      i particle-size * system particles +
      system update-particle
    loop
    current-systick system last-systick !
  ;
  
  : draw-particle
    { color display particle system -- }
    [ brush-size 2 / ] literal { half }
    color 0 0
    particle particle-x @ v>n half -
    particle particle-y @ v>n half -
    brush-size brush-size
    system brush display draw-rect-const-mask
  ;
  
  : erase-system { display system -- }
    particle-count 0 ?do
      [ 0 0 0 rgb8 ] literal
      display i particle-size * system particles +
      system draw-particle
    loop
  ;

  : draw-system { display system -- }
    particle-count 0 ?do
      display i particle-size * system particles +
      dup particle-color @ -rot system draw-particle
    loop
  ;
  
  : step-system ( system -- )
    [: { system display }
      display system erase-system
      system update-system
      display system draw-system
      display update-display
    ;] with-term-display
  ;
  
  : run-system ( -- )
    system-size [: { system }
      system init-system
      page
      [: dup clear-pixmap update-display ;]
      with-term-display
      begin key? not while system step-system repeat
      key drop
    ;] with-aligned-allot
  ;
  
end-module
