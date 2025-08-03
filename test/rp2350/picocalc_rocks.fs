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

\ The controls are:
\ 
\ Up: Thrust
\ Right: Turn right
\ Left: Turn left
\ Space: Fire
\ Q: Give up in shame

begin-module rocks
  
  oo import
  picocalc-term import
  picocalc-sound import
  pixmap8 import
  pixmap8-utils import
  font import
  st7365p-8-common import
  float32 import
  tinymt32 import
  
  tinymt32-size buffer: prng
  
  : init-prng ( -- )
    rng::random prng tinymt32-init
    prng tinymt32-prepare-example
  ;
  
  initializer init-prng
  
  : urandom ( -- u ) prng tinymt32-generate-uint32 ;
  
  : vrandom ( -- v ) urandom 0 f64>v ;
  
  16e0 constant border-width
  16e0 constant border-height
  320e0 constant arena-width
  320e0 constant arena-height
  
  : convert-coord { x y -- x' y' }
    x v>n arena-height y v- v>n
  ;
  
  0 0 0 rgb8 constant bk-color
  255 255 0 rgb8 constant info-color
  
  16 constant info-x
  16 constant info-y
  
  180e0 constant base-shot-speed
  8e0 vpi v* constant ship-heading-decel
  8e0 constant ship-body-radius
  16e0 constant ship-dir-radius
  0 255 0 rgb8 constant ship-color
  40000 constant wave-start-ticks
  
  variable wave
  variable lives
  variable wave-start
  variable wave-start-systick
  
  : render-label ( color addr bytes n x y display -- )
    256 [: { color addr bytes n x y display buffer }
      addr buffer bytes move
      n s>d <# #s #> dup { bytes' } buffer bytes + swap move
      color buffer bytes bytes' + x y display term-font@
      draw-string-to-pixmap8
    ;] with-allot
  ;
  
  : render-info { color display -- }
    color s" Wave:  " wave @ info-x info-y display render-label
    color s" Lives: " lives @ 0 max
    info-x info-y term-font@ char-rows @ +
    display render-label
  ;
  
  : draw-info { display -- } info-color display render-info ;
  
  : erase-info { display -- } bk-color display render-info ;
   
  <object> begin-class <entity>
  
    cell member entity-active
    cell member entity-x
    cell member entity-y
    cell member entity-delta-x
    cell member entity-delta-y
    
    method entity-active? ( entity -- active? )
    method entity-coord@ ( entity -- x y )
    method entity-delta@ ( entity -- delta-x delta-y )
    method activate-entity ( x y delta-x delta-y entity -- )
    method deactivate-entity ( entity -- )
    method adjust-entity-delta ( adjust-x adjust-y entity -- )
    method update-entity ( interval entity -- )
    method draw-entity ( display entity -- )
    method erase-entity ( display entity -- )
    method do-update-entity ( interval entity -- )
    method do-draw-entity ( display entity -- )
    method do-erase-entity ( display entity -- )
    
  end-class
  
  <entity> begin-implement
  
    :noname { self -- }
      self <object>->new
      false self entity-active !
      0e0 self entity-x !
      0e0 self entity-y !
      0e0 self entity-delta-x !
      0e0 self entity-delta-y !
    ; define new
    
    :noname { self -- active? }
      self entity-active @
    ; define entity-active?
    
    :noname { self -- x y }
      self entity-x @ self entity-y @
    ; define entity-coord@
    
    :noname { self -- delta-x delta-y }
      self entity-delta-x @ self entity-delta-y @
    ; define entity-delta@
    
    :noname { x y delta-x delta-y self -- }
      true self entity-active !
      x self entity-x !
      y self entity-y !
      delta-x self entity-delta-x !
      delta-y self entity-delta-y !
    ; define activate-entity
    
    :noname { self -- }
      false self entity-active !
      0e0 self entity-x !
      0e0 self entity-y !
      0e0 self entity-delta-x !
      0e0 self entity-delta-y !
    ; define deactivate-entity
    
    :noname { adjust-x adjust-y self -- }
      self entity-delta-x @ adjust-x v+
      self entity-delta-x !
      self entity-delta-y @ adjust-y v+
      self entity-delta-y !
    ; define adjust-entity-delta
    
    :noname { interval self -- }
      self entity-active? if
        interval self do-update-entity
      then
    ; define update-entity
    
    :noname { display self -- }
      self entity-active? if display self do-draw-entity then
    ; define draw-entity
    
    :noname { display self -- }
      self entity-active? if display self do-erase-entity then
    ; define erase-entity
    
    :noname { interval self -- }
      self entity-x @
      self entity-delta-x @ interval v* v+ { x }
      self entity-y @
      self entity-delta-y @ interval v* v+ { y }
      x border-width vnegate v< if
        x arena-width v+ border-width v+ to x
      then
      x arena-width border-width v+ v> if
        x arena-width v- border-width v- to x
      then
      y border-height vnegate v< if
        y arena-height v+ border-height v+ to y
      then
      y arena-height border-height v+ v> if
        y arena-height v- border-height v- to y
      then
      x self entity-x !
      y self entity-y !
    ; define do-update-entity
    
    :noname { display self -- } ; define do-draw-entity
    
    :noname { display self -- } ; define do-erase-entity
    
  end-implement
  
  <entity> begin-class <shootable>
    
    method shootable-radius@ ( shootable -- radius )
    method try-shoot ( x y shootable -- hit? )
    method do-hit ( shootable -- )
  
  end-class
  
  <shootable> begin-implement
    
    :noname { self -- radius } 0e0 ; define shootable-radius@
    
    :noname { shot-x shot-y self -- hit? }
      self entity-active? if
        self entity-coord@ { x y }
        self shootable-radius@ { radius }
        shot-x x v- dup v* shot-y y v- dup v* v+
        radius dup v* v<= if self do-hit true else false then
      else
        false
      then
    ; define try-shoot
    
    :noname { self -- } ; define do-hit
    
  end-implement
  
  16e0 constant max-asteroid-radius
  3 constant max-divide-count
  max-asteroid-radius 4e0 v/ constant min-asteroid-radius
  20e0 constant max-asteroid-speed
  
  <shootable> begin-class <asteroid>
  
    cell member asteroid-radius
    
    method do-spawn-asteroid ( radius x y asteroid -- )
    method render-asteroid ( color display asteroid -- )
    method check-collide ( x y asteroid -- collide? )
    
  end-class
  
  120 constant max-asteroid-count
  max-asteroid-count <asteroid> class-size * buffer: asteroids
  
  : asteroid@ ( index -- asteroid )
    <asteroid> class-size * asteroids +
  ;
  
  : spawn-asteroid { radius x y -- }
    max-asteroid-count 0 ?do
      i asteroid@ { asteroid }
      asteroid entity-active? not if
        radius x y asteroid do-spawn-asteroid exit
      then
    loop
  ;
  
  : start-asteroid ( -- )
    max-asteroid-radius
    arena-width vrandom v* arena-height vrandom v*
    spawn-asteroid
  ;
  
  : init-asteroids ( -- )
    max-asteroid-count 0 ?do
      <asteroid> i asteroid@ init-object
    loop
  ;
  
  : deactivate-asteroids ( -- )
    max-asteroid-count 0 ?do
      i asteroid@ deactivate-entity
    loop
  ;
  
  : update-asteroids { interval -- }
    max-asteroid-count 0 ?do
      interval i asteroid@ update-entity
    loop
  ;
  
  : draw-asteroids { display -- }
    max-asteroid-count 0 ?do
      display i asteroid@ draw-entity
    loop
  ;
  
  : erase-asteroids { display -- }
    max-asteroid-count 0 ?do
      display i asteroid@ erase-entity
    loop
  ;
  
  : try-shoot-asteroids { x y -- hit? }
    false
    max-asteroid-count 0 ?do
      x y i asteroid@ try-shoot or
    loop
  ;
  
  : check-collide-asteroids { x y -- collide? }
    false
    max-asteroid-count 0 ?do
      x y i asteroid@ check-collide or
    loop
  ;
  
  : asteroid-count ( -- count )
    0
    max-asteroid-count 0 ?do
      i asteroid@ entity-active? if 1+ then
    loop
  ;
  
  255 255 255 rgb8 constant asteroid-color
  
  <asteroid> begin-implement
    
    :noname { self -- }
      self <shootable>->new
      0e0 self asteroid-radius !
    ; define new
    
    :noname { radius x y self -- }
      max-asteroid-speed vrandom v* { speed }
      2e0 vpi v* vrandom v* { angle }
      speed angle vcos v* { delta-x }
      speed angle vsin v* { delta-y }
      x y delta-x delta-y self activate-entity
      radius self asteroid-radius !
    ; define do-spawn-asteroid
    
    :noname { self -- radius }
      self asteroid-radius @
    ; define shootable-radius@
    
    :noname { self -- }
      max-divide-count 2 - u>v vrandom v* 2e0 v+
      vround-half-away-zero v>u { divide }
      self entity-coord@ { x y }
      self asteroid-radius @ divide u>v v/ { radius }
      self deactivate-entity
      radius min-asteroid-radius v>= if
        divide 0 ?do radius x y spawn-asteroid loop
      then
    ; define do-hit
    
    :noname { ship-x ship-y self -- hit? }
      self entity-active? if
        self entity-coord@ { x y }
        self shootable-radius@ { radius }
        ship-x x v- dup v* ship-y y v- dup v* v+
        radius ship-body-radius v+ dup v* v<= dup if
          self do-hit
        then
      else
        false
      then
    ; define check-collide
    
    :noname { color display self -- }
      self entity-coord@ { x y }
      color x y convert-coord
      self asteroid-radius @ v>n
      display draw-pixel-circle
    ; define render-asteroid
    
    :noname { display self -- }
      asteroid-color display self render-asteroid
    ; define do-draw-entity
    
    :noname { display self -- }
      bk-color display self render-asteroid
    ; define do-erase-entity
    
  end-implement
  
  <entity> begin-class <shot>
  
    cell member shot-energy
    
    method do-spawn-shot ( x y delta-x delta-y shot -- )
    method render-shot ( color display shot -- )
  
  end-class
  
  1e0 constant init-shot-energy
  120 constant max-shot-count
  max-shot-count <shot> class-size * buffer: shots
  
  : shot@ ( index -- shot )
    <shot> class-size * shots +
  ;
  
  : spawn-shot { x y delta-x delta-y -- }
    max-shot-count 0 ?do
      i shot@ { shot }
      shot entity-active? not if
        x y delta-x delta-y shot do-spawn-shot exit
      then
    loop
  ;
  
  : init-shots ( -- )
    max-shot-count 0 ?do <shot> i shot@ init-object loop
  ;
  
  : deactivate-shots ( -- )
    max-shot-count 0 ?do i shot@ deactivate-entity loop
  ;
  
  : update-shots { interval -- }
    max-shot-count 0 ?do interval i shot@ update-entity loop
  ;
  
  : draw-shots { display -- }
    max-shot-count 0 ?do display i shot@ draw-entity loop
  ;
  
  : erase-shots { display -- }
    max-shot-count 0 ?do display i shot@ erase-entity loop
  ;
  
  255 0 0 rgb8 constant shot-color
  4e0 constant shot-size
  
  <shot> begin-implement
    
    :noname { self -- }
      self <entity>->new
      0e0 self shot-energy !
    ; define new
    
    :noname { x y delta-x delta-y self -- }
      x y delta-x delta-y self activate-entity
      init-shot-energy self shot-energy !
    ; define do-spawn-shot
    
    :noname { interval self -- }
      interval self <entity>->do-update-entity
      self entity-coord@ try-shoot-asteroids if
        self deactivate-entity exit
      then
      self shot-energy @ interval v- dup { energy }
      self shot-energy !
      energy 0e0 v<= if self deactivate-entity then
    ; define do-update-entity
    
    :noname { color display self -- }
      self entity-coord@ { x y }
      self entity-delta@ { delta-x delta-y }
      delta-y vnegate delta-x vnegate vatan2 { angle }
      color x y convert-coord
      x angle vcos shot-size v* v+
      y angle vsin shot-size v* v+ convert-coord
      display draw-pixel-line
    ; define render-shot
    
    :noname { display self -- }
      shot-color display self render-shot
    ; define do-draw-entity
    
    :noname { display self -- }
      bk-color display self render-shot
    ; define do-erase-entity
    
  end-implement
  
  <entity> begin-class <ship>
    
    cell member ship-heading
    cell member ship-delta-heading
    
    method do-spawn-ship ( ship -- )
    method turn-ship ( angle ship -- )
    method thrust-ship ( speed ship -- )
    method ship-shoot ( ship -- )
    method render-ship ( color display ship -- )
    
  end-class
  
  <ship> class-size buffer: ship
  
  : init-ship ( -- ) <ship> ship init-object ;
  
  : spawn-ship ( -- )
    true wave-start !
    systick::systick-counter wave-start-systick !
    ship do-spawn-ship
  ;
  
  : deactivate-ship ( -- ) ship deactivate-entity ;
  
  : update-ship ( interval -- ) ship update-entity ;
  
  : draw-ship ( display -- ) ship draw-entity ;
  
  : erase-ship ( display -- ) ship erase-entity ;
  
  <ship> begin-implement
    
    :noname { self -- }
      self <entity>->new
      0e0 self ship-heading !
      0e0 self ship-delta-heading !
    ; define new
    
    :noname { self -- }
      arena-width 2e0 v/ arena-height 2e0 v/ 0e0 0e0
      self activate-entity
      vpi 2e0 v/ self ship-heading !
      0e0 self ship-delta-heading !
    ; define do-spawn-ship
    
    :noname { angle self -- }
      self ship-delta-heading @ angle v+
      self ship-delta-heading !
    ; define turn-ship
    
    :noname { thrust self -- }
      self ship-heading @ { heading }
      heading vcos thrust v* heading vsin thrust v*
      self adjust-entity-delta
    ; define thrust-ship
    
    :noname { self -- }
      self ship-heading @ { heading }
      self entity-delta@ { delta-x delta-y }
      self entity-coord@
      delta-x heading vcos base-shot-speed v* v+
      delta-y heading vsin base-shot-speed v* v+
      spawn-shot
    ; define ship-shoot
    
    :noname { interval self -- }
      interval self <entity>->do-update-entity
      self ship-heading @ { heading }
      self ship-delta-heading @ { delta-heading }
      delta-heading v0<> if
        heading delta-heading interval v* v+
        self ship-heading !
        delta-heading vabs ship-heading-decel interval v* v-
        0e0 vmax delta-heading dup vabs v/ v*
        self ship-delta-heading !
      then
      self entity-coord@ check-collide-asteroids if
        wave-start @ not if
          beep
          -1 lives +!
          lives @ 0>= if spawn-ship then
        then
      then
    ; define do-update-entity
    
    :noname { color display self -- }
      self entity-coord@ { x y }
      color x y convert-coord ship-body-radius v>n
      display draw-filled-circle
      self ship-heading @ { heading }
      x heading vcos ship-dir-radius v* v+ { x1 }
      y heading vsin ship-dir-radius v* v+ { y1 }
      color x y convert-coord x1 y1 convert-coord
      display draw-pixel-line
    ; define render-ship
    
    :noname { display self -- }
      ship-color display self render-ship
    ; define do-draw-entity
    
    :noname { display self -- }
      bk-color display self render-ship
    ; define do-erase-entity
    
  end-implement
  
  : init-world ( -- ) init-asteroids init-shots init-ship ;
  
  initializer init-world
  
  : deactivate-world ( -- )
    deactivate-asteroids deactivate-shots deactivate-ship
  ;
    
  : update-world { interval -- }
    interval update-asteroids
    interval update-shots
    interval update-ship
  ;
  
  : draw-world { display -- }
    display draw-asteroids
    display draw-shots
    display draw-ship
    display draw-info
  ;
  
  : erase-world { display -- }
    display erase-asteroids
    display erase-shots
    display erase-ship
    display erase-info
  ;
  
  3 constant init-asteroid-count
  
  : start-world ( -- )
    deactivate-world
    wave @ init-asteroid-count + 0 ?do start-asteroid loop
    spawn-ship
  ;
  
  vpi constant ship-turn-speed
  10e0 constant ship-thrust
  
  : handle-thrust ( -- ) ship-thrust ship thrust-ship ;
  
  : handle-turn-right ( -- )
    ship-turn-speed vnegate ship turn-ship
  ;
  
  : handle-turn-left ( -- )
    ship-turn-speed ship turn-ship
  ;
  
  : handle-shoot ( -- ) ship ship-shoot ;
  
  $1B constant escape
  
  : empty-keys ( -- ) begin key? while key drop repeat ;
  
  : handle-key ( -- exit? )
    key? if
      key case
        escape of
          key? if
            key [char] [ = if
              key? if
                key case
                  [char] A of handle-thrust endof
                  [char] C of handle-turn-right endof
                  [char] D of handle-turn-left endof
                  empty-keys
                endcase
              then
            else
              empty-keys
            then
          then
          false
        endof
        bl of handle-shoot false endof
        [char] q of true endof
        false swap
      endcase
    else
      false
    then
  ;
  
  2 constant init-lives
  5 constant extra-life-wave
  
  : play-rocks ( -- )
    0 wave !
    init-lives lives !
    begin
      [: dup clear-pixmap update-display ;] with-term-display
      start-world
      systick::systick-counter { last-systick }
      begin
        last-systick [: { last-systick display }
          display erase-world
          systick::systick-counter { current-systick }
          wave-start @ if
            current-systick wave-start-systick @ -
            wave-start-ticks < wave-start !
          then
          current-systick last-systick - u>v 10000e0 v/
          update-world
          handle-key { exit-key? }
          display draw-world
          display update-display
          current-systick exit-key?
        ;] with-term-display
        { exit-key? } to last-systick
        exit-key? lives @ 0< or if
          [: dup clear-pixmap update-display ;]
          with-term-display
          page
          ." *** GAME OVER ***" cr cr
          ." You survived " wave @ . ." waves" cr
          empty-keys
          exit
        then
        asteroid-count 0=
      until
      1 wave +!
      wave @ extra-life-wave umod 0= if 1 lives +! then
    again
  ;
  
end-module
