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

begin-module bricks
  
  float32 import
  picocalc-term import
  tinymt32 import
  pixmap8 import
  pixmap8-utils import
  st7365p-8-common import
  
  begin-structure coord-size
    field: coord-x
    field: coord-y
  end-structure
  
  begin-structure delta-size
    field: delta-x
    field: delta-y
  end-structure
  
  begin-structure ball-size
    coord-size +field ball-coord
    delta-size +field ball-delta
    field: ball-radius
    field: ball-color
  end-structure
  
  begin-structure block-size
    coord-size +field block-coord
    field: block-width
    field: block-height
    field: block-color
    field: block-active
  end-structure
  
  begin-structure player-size
    coord-size +field player-coord
    delta-size +field player-delta
    field: player-width
    field: player-height
    field: player-color
  end-structure
  
  160e0 constant blocks-center-x
  80e0 constant blocks-center-y
  20e0 constant default-block-width
  7.5e0 constant default-block-height
  2e0 constant default-ball-radius
  160e0 constant init-ball-x
  160e0 constant init-ball-y
  120e0 constant init-ball-speed
  vpi 2e0 v/ constant init-ball-angle-range
  60e0 constant default-player-width
  7.5e0 constant default-player-height
  160e0 constant init-player-x
  300e0 constant init-player-y
  16 constant horiz-block-count
  8 constant vert-block-count
  horiz-block-count vert-block-count * constant block-count
  
  begin-structure world-size
    ball-size +field world-ball
    block-size block-count * +field world-blocks
    player-size +field world-player
    tinymt32-size +field world-prng
    field: world-last-tick
    field: world-tick
  end-structure
  
  : rnd ( world -- v )
    world-prng tinymt32-generate-uint32 0 f64>v
  ;
  
  : interval ( world -- v )
    dup world-tick @ swap world-last-tick @ - u>v 10000e0 v/
  ;
  
  : convert-angle ( n -- n' ) 255 * 60 / ;
  
  : angle-color { angle -- rgb8 }
    angle 60 < if
      255 angle convert-angle 0
    else
      angle 120 < if
        120 angle - convert-angle 255 0
      else
        angle 180 < if
          0 255 angle 120 - convert-angle
        else
          angle 240 < if
            0 240 angle - convert-angle 255
          else
            angle 300 < if
              angle 240 - convert-angle 0 255
            else
              255 0 360 angle - convert-angle
            then
          then
        then
      then
    then
    rgb8
  ;
  
  : rnd-color ( world -- rgb8 )
    rnd 360e0 v* v>n angle-color
  ;
  
  : init-block { x y world block -- }
    horiz-block-count 1- n>v default-block-width v* 2e0 v/
    blocks-center-x swap v-
    default-block-width x n>v v* v+
    block block-coord coord-x !
    vert-block-count 1- n>v default-block-height v* 2e0 v/
    blocks-center-y swap v-
    default-block-height y n>v v* v+
    block block-coord coord-y !
    default-block-width block block-width !
    default-block-height block block-height !
    world rnd-color block block-color !
    true block block-active !
  ;
  
  : init-blocks { world -- }
    vert-block-count 0 ?do
      horiz-block-count 0 ?do
        i j world j horiz-block-count * i + block-size *
        world world-blocks + init-block
      loop
    loop
  ;
  
  : init-ball { world ball -- }
    init-ball-x ball ball-coord coord-x !
    init-ball-y ball ball-coord coord-y !
    default-ball-radius ball ball-radius !
    vpi 2e0 v/ init-ball-angle-range 2e0 v/ v-
    init-ball-angle-range world rnd v* v+ { angle }
    angle vcos init-ball-speed v* ball ball-delta delta-x !
    angle vsin init-ball-speed v* ball ball-delta delta-y !
    world rnd-color ball ball-color !
  ;
  
  : init-player { world player -- }
    init-player-x player player-coord coord-x !
    init-player-y player player-coord coord-y !
    0e0 player player-delta delta-x !
    0e0 player player-delta delta-y !
    default-player-width player player-width !
    default-player-height player player-height !
    world rnd-color player player-color !
  ;
  
  : init-prng { prng -- }
    prng tinymt32-init
    rng::random prng tinymt32-prepare-example
  ;
  
  : init-world { world -- }
    systick::systick-counter dup world world-tick !
    world world-last-tick ! 
    world world-prng init-prng
    world init-blocks
    world dup world-ball init-ball
    world dup world-player init-player
  ;
  
  0 0 0 rgb8 constant erase-color
  
  : draw-rect { erase? color x y width height display -- }
    width 2e0 v/ { width2/ }
    height 2e0 v/ { height2/ }
    x width2/ v- v>n { x0 }
    y height2/ v- v>n { y0 }
    x width2/ v+ v>n { x1 }
    y height2/ v+ v>n { y1 }
    erase? if erase-color else color then
    x0 y0 width v>n height v>n display draw-rect-const
  ;
  
  : draw-block { erase? display block -- }
    erase? block block-color @
    block block-coord coord-x @
    block block-coord coord-y @
    block block-width @
    block block-height @
    display draw-rect
  ;
  
  : draw-player { erase? display player -- }
    erase? player player-color @
    player player-coord coord-x @
    player player-coord coord-y @
    player player-width @
    player player-height @
    display draw-rect
  ;
  
  : draw-circle { erase? color x y radius display -- }
    erase? if erase-color else color then
    x v>n y v>n radius v>n display draw-filled-circle
  ;
  
  : draw-ball { erase? display ball -- }
    erase? ball ball-color @
    ball ball-coord coord-x @
    ball ball-coord coord-y @
    ball ball-radius @
    display draw-circle
  ;
  
  : draw-world { display world -- }
    display clear-pixmap
    horiz-block-count vert-block-count * 0 ?do
      false display i block-size * world world-blocks +
      draw-block
    loop
    false display world world-player draw-player
    false display world world-ball draw-ball
  ;
  
  : collide-rect { time x y width height ball -- collide? }
    ball ball-coord coord-x @ { bx }
    ball ball-coord coord-y @ { by }
    ball ball-delta delta-x @ { bdx }
    ball ball-delta delta-y @ { bdy }
    ball ball-radius @ { radius }
    bx bdx time v* v+ { bx' }
    by bdy time v* v+ { by' }
    width 2e0 v/ { width2/ }
    height 2e0 v/ { height2/ }
    x width2/ v- radius v- { x0 }
    y height2/ v- radius v- { y0 }
    x width2/ v+ radius v+ { x1 }
    y height2/ v+ radius v+ { y1 }
    bx x0 v< bx' x0 v>= and
    bx x1 v> bx' x1 v<= and or
    bx' x0 v>= bx' x1 v<= and or
    by y0 v< by' y0 v>= and
    by y1 v> by' y1 v<= and or
    by' y0 v>= by' y1 v<= and or and dup if
      bx x0 v<= bx' x0 v>= and if
        bdx vabs vnegate ball ball-delta delta-x !
        x0 ball ball-coord coord-x !
      then
      bx x1 v>= bx' x1 v<= and if
        bdx vabs ball ball-delta delta-x !
        x1 ball ball-coord coord-x !
      then
      by y0 v<= by' y0 v>= and if
        bdy vabs vnegate ball ball-delta delta-y !
        y0 ball ball-coord coord-y !
      then
      by y1 v>= by' y1 v<= and if
        bdy vabs ball ball-delta delta-y !
        y1 ball ball-coord coord-y !
      then
    then
  ;
  
  : collide-block { time display ball block -- }
    block block-active @ if
      time
      block block-coord coord-x @
      block block-coord coord-y @
      block block-width @
      block block-height @
      ball collide-rect if
        false block block-active !
        true display block draw-block
      then
    then
  ;
  
  : collide-blocks { time display ball world -- }
    horiz-block-count vert-block-count * 0 ?do
      time display ball
      i block-size * world world-blocks + collide-block
    loop
  ;
  
  0.5e0 constant collide-player-fract
  
  : collide-player { time ball player -- }
    time
    player player-coord coord-x @
    player player-coord coord-y @
    player player-width @
    player player-height @
    ball collide-rect if
      player player-delta delta-x @ collide-player-fract v*
      ball ball-delta delta-x @ v+
      ball ball-delta delta-x !
    then
  ;
  
  : collide-wall { time ball -- bottom? }
    ball ball-coord coord-x @ { bx }
    ball ball-coord coord-y @ { by }
    ball ball-delta delta-x @ { bdx }
    ball ball-delta delta-y @ { bdy }
    ball ball-radius @ { radius }
    bx bdx time v* v+ { bx' }
    by bdy time v* v+ { by' }
    term-pixels-dim@ n>v swap n>v { height width }
    bx' width radius v- v>= if
      bdx vabs vnegate ball ball-delta delta-x !
    then
    bx' radius v<= if
      bdx vabs ball ball-delta delta-x !
    then
    by' radius v<= if
      bdy vabs ball ball-delta delta-y !
    then
    by' height v>=
  ;
  
  : +delta! { time delta coord -- }
    delta delta-x @ time v* coord coord-x @ v+ coord coord-x !
    delta delta-y @ time v* coord coord-y @ v+ coord coord-y !
  ;
  
  : move-ball { time ball -- }
    time ball ball-delta ball ball-coord +delta!
  ;
  
  0.0625e0 constant friction0
  1e0 64e0 v/ constant friction1
  
  : apply-friction { time player -- }
    friction0 time v** friction1 time v** v* { friction' }
    player player-delta delta-x @ friction' v*
    player player-delta delta-x !
  ;
  
  : move-player { time player -- }
    time player player-delta player player-coord +delta!
    player player-width @ 2e0 v/ { width2/ }
    player player-coord coord-x @ { x }
    term-pixels-dim@ drop n>v { width' }
    x width2/ v<= dup if
      width2/ player player-coord coord-x !
    then
    x width' width2/ v- v>= dup if
      width' width2/ v- player player-coord coord-x !
    then
    or if
      0e0 player player-delta delta-x !
    then
  ;
   
  : update { display world -- died? }
    world interval { time }
    true display world world-ball draw-ball
    true display world world-player draw-player
    time display world world-ball world collide-blocks
    time world world-ball world world-player collide-player
    time world world-ball collide-wall dup if
      world dup world-ball init-ball
      world dup world-player init-player
    else
      time world world-ball move-ball
      time world world-player apply-friction
      time world world-player move-player
    then
    false display world world-ball draw-ball
    false display world world-player draw-player
  ;
  
  240e0 constant accel
  640e0 constant max-speed
  
  : accel-left { time player -- }
    player player-delta delta-x @ accel ( time v* ) v-
    max-speed vnegate vmax
    player player-delta delta-x !
  ;

  : accel-right { time player -- }
    player player-delta delta-x @ accel ( time v* ) v+
    max-speed vmin
    player player-delta delta-x !
  ;
  
  : reset-tick { world }
    systick::systick-counter dup world world-tick !
    world world-last-tick ! 
  ;
  
  : run-game ( -- )
    page
    world-size [: { world }
      world init-world
      world [: { world display }
        display world draw-world
        display update-display
      ;] with-term-display
      begin key? until
      key [char] q = if exit then
      world reset-tick
      begin
        world [: { world display }
          display world update
          display update-display
        ;] with-term-display
        if
          begin key? until
          key [char] q = if exit then
          world reset-tick
        else
          key? if
            key case
              $1B of
                key? if
                  key case
                    [char] [ of
                      key? if
                        key case
                          [char] D of
                            world interval
                            world world-player accel-left
                          endof
                          [char] C of
                            world interval
                            world world-player accel-right
                          endof
                        endcase
                      then
                    endof
                  endcase
                then
              endof
              [char] q of
                exit
              endof
            endcase
          then
          world world-tick @ world world-last-tick !
          systick::systick-counter world world-tick !
        then
      again
    ;] with-aligned-allot
  ;
      
  
end-module