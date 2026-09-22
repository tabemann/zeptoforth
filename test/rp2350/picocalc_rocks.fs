\ Copyright (c) 2025-2026 Travis Bemann
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
\ Semicolon: Shields
\ P: Pause / Unpause
\ S: Take a screenshot
\ Q: Give up in shame

begin-module rocks
  
  oo import
  picocalc-term import
  picocalc-sound import
  picocalc-keys import
  picocalc-screenshot import
  pixmap8 import
  pixmap8-utils import
  font import
  st7365p-8-common import
  float32 import
  tinymt32 import
  
  $B4 constant LEFT_ARROW
  $B5 constant UP_ARROW
  $B6 constant DOWN_ARROW
  $B7 constant RIGHT_ARROW

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
  120e0 constant alien-shot-speed
  12e0 constant alien-speed
  vpi 16e0 v/ constant alien-shot-error
  1e0 60e0 v/ constant alien-chance
  1e0 constant alien-shot-chance
  -2e0 constant recoil-speed
  8e0 vpi v* constant ship-heading-decel
  8e0 constant ship-body-radius
  8e0 constant alien-radius
  12e0 constant ship-shield-radius
  16e0 constant ship-dir-radius
  0 255 0 rgb8 constant ship-color
  0 255 255 rgb8 constant shield-color
  40000 constant wave-start-ticks
  -12.5e0 constant shield-rate
  0.33e0 constant shield-persist
  30e0 constant bonus-persist
  100e0 constant bonus-shield
  8e0 constant bonus-radius
  1e0 60e0 v/ constant bonus-chance
  1e0 constant retro-decel
  100e0 constant init-shield
  0.25e0 constant slow-shot-delay
  0.0625e0 constant fast-shot-delay
  
  
  : interval-chance { interval chance -- flag }
    vrandom interval v/ chance v<
  ;
  
  variable paused
  variable wave
  variable lives
  variable shield
  variable shield-active
  variable retro
  variable shot-delay
  variable shot-delay-time
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

  : render-fixed-label { color addr bytes x y display -- }
    color addr bytes x y display term-font@
    draw-string-to-pixmap8
  ;
  
  : render-info { color display -- }
    color s" Wave:  " wave @ info-x info-y display render-label
    color s" Lives: " lives @ 0 max
    info-x info-y term-font@ char-rows @ +
    display render-label
    color s" Shield: " shield @ v>n
    info-x info-y term-font@ char-rows @ 2 * +
    display render-label
    3 { line }
    retro @ if
      color s" Retro: On"
      info-x info-y term-font@ char-rows @ line * +
      display render-fixed-label
      1 +to line
    then
    shot-delay @ fast-shot-delay = if
      color s" Rapid fire: On"
      info-x info-y term-font@ char-rows @ line * +
      display render-fixed-label
    then
  ;
  
  : draw-info { display -- } info-color display render-info ;
  
  : erase-info { display -- } bk-color display render-info ;
  
  255 255 0 rgb8 constant paused-color
  
  : render-paused { color display -- }
    term-pixels-dim@ { screen-width screen-height }
    term-font@ dup char-cols @ { font-width } char-rows @ { font-height }
    s" *** PAUSED ***" { addr bytes }
    screen-width 2 / bytes font-width * 2 / - { x }
    screen-height 2 / font-height 2 / - { y }
    color addr bytes x y display render-fixed-label
  ;
  
  : draw-paused { display -- }
    paused @ if paused-color display render-paused then
  ;
  
  : erase-paused { display -- }
    paused @ if bk-color display render-paused then
  ;
   
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
    method decel-entity-delta ( decel-x decel-y entity -- )
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
    
    :noname { decel-x decel-y self -- }
      self entity-delta-x @ dup decel-x v* v- self entity-delta-x !
      self entity-delta-y @ dup decel-y v* v- self entity-delta-y !
    ; define decel-entity-delta
    
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
    method check-collide ( ship? x y asteroid -- collide? )
    
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
  
  : check-collide-asteroids { ship? x y -- collide? }
    false
    max-asteroid-count 0 ?do
      ship? x y i asteroid@ check-collide or
    loop
  ;
  
  : asteroid-count ( -- count )
    0
    max-asteroid-count 0 ?do
      i asteroid@ entity-active? if 1+ then
    loop
  ;
  
  : ship-collide-radius ( -- radius )
    shield-active @ v0> wave-start @ or if
      ship-shield-radius
    else
      ship-body-radius
    then
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
    
    :noname { ship? ship-x ship-y self -- hit? }
      self entity-active? if
        self entity-coord@ { x y }
        self shootable-radius@ { radius }
        ship-x x v- dup v* ship-y y v- dup v* v+
        radius ship? if ship-collide-radius else alien-radius then
        v+ dup v* v<= dup if
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
  
  <shootable> begin-class <bonus>
  
    cell member bonus-type
    cell member bonus-time
    
    method do-spawn-bonus ( type x y bonus -- )
    method render-bonus ( color display bonus -- )
    method check-bonus-collide ( ship? x y bonus -- )
    method do-bonus ( bonus -- )
    method do-expire ( bonus -- )
    
  end-class
  
  32 constant max-bonus-count
  max-bonus-count <bonus> class-size * buffer: bonuses
  40e0 constant max-bonus-speed
  
  : bonus@ ( index -- bonus )
    <bonus> class-size * bonuses +
  ;
  
  : spawn-bonus { type x y -- }
    max-bonus-count 0 ?do
      i bonus@ { bonus }
      bonus entity-active? not if
        type x y bonus do-spawn-bonus exit
      then
    loop
  ;
  
  : random-bonus-type ( -- xt )
    vrandom { rnd }
    rnd 0.25e0 v< if
      [: bonus-shield shield @ v+ shield ! ;]
    else
      rnd 0.5e0 v< if
        [: 1 lives +! ;]
      else
        rnd 0.75e0 v< if
          [: true retro ! ;]
        else
          [: fast-shot-delay shot-delay ! ;]
        then
      then
    then
  ;
  
  : start-bonus ( -- )
    random-bonus-type
    arena-width vrandom v* arena-height vrandom v*
    spawn-bonus
  ;
  
  : init-bonuses ( -- )
    max-bonus-count 0 ?do
      <bonus> i bonus@ init-object
    loop
  ;
  
  : deactivate-bonuses ( -- )
    max-bonus-count 0 ?do
      i bonus@ deactivate-entity
    loop
  ;
  
  : update-bonuses { interval -- }
    max-bonus-count 0 ?do
      interval i bonus@ update-entity
    loop
  ;
  
  : draw-bonuses { display -- }
    max-bonus-count 0 ?do
      display i bonus@ draw-entity
    loop
  ;
  
  : erase-bonuses { display -- }
    max-bonus-count 0 ?do
      display i bonus@ erase-entity
    loop
  ;
  
  : try-shoot-bonuses { x y -- hit? }
    false
    max-bonus-count 0 ?do
      x y i bonus@ try-shoot or
    loop
  ;
  
  : try-collide-bonuses { ship? x y -- }
    max-bonus-count 0 ?do
      ship? x y i bonus@ check-bonus-collide
    loop
  ;
  
  : bonus-count ( -- count )
    0
    max-bonus-count 0 ?do
      i bonus@ entity-active? if 1+ then
    loop
  ;
  
  255 0 255 rgb8 constant bonus-color
  
  <bonus> begin-implement
    
    :noname { self -- }
      self <shootable>->new
      [: ;] self bonus-type !
      0e0 self bonus-time !
    ; define new
    
    :noname { type x y self -- }
      type self bonus-type !
      bonus-persist self bonus-time !
      max-bonus-speed vrandom v* { speed }
      2e0 vpi v* vrandom v* { angle }
      speed angle vcos v* { delta-x }
      speed angle vsin v* { delta-y }
      x y delta-x delta-y self activate-entity
    ; define do-spawn-bonus
    
    :noname { self -- radius }
      bonus-radius
    ; define shootable-radius@
    
    :noname { self -- }
      beep
      self deactivate-entity
    ; define do-hit
    
    :noname { self -- }
      self bonus-type @ execute
      self deactivate-entity
    ; define do-bonus
    
    :noname { self -- }
      beep
      self deactivate-entity
    ; define do-expire
    
    :noname { ship? ship-x ship-y self -- }
      self entity-active? if
        self entity-coord@ { x y }
        self shootable-radius@ { radius }
        ship-x x v- dup v* ship-y y v- dup v* v+
        radius ship? if ship-collide-radius else alien-radius then
        v+ dup v* v<= if
          shield-active @ v0> ship? not or if
            self do-hit
          else
            self do-bonus
          then
        then
      then
    ; define check-bonus-collide
    
    :noname { color display self -- }
      self entity-coord@ { x y }
      color x y convert-coord
      bonus-radius v>n
      display draw-filled-circle
    ; define render-bonus
    
    :noname { interval self -- }
      interval self <entity>->do-update-entity
      self bonus-time @ interval v- 0e0 vmax self bonus-time !
      self bonus-time @ v0= if self do-expire then
    ; define do-update-entity

    :noname { display self -- }
      bonus-color display self render-bonus
    ; define do-draw-entity
    
    :noname { display self -- }
      bk-color display self render-bonus
    ; define do-erase-entity
    
  end-implement
  
  <entity> begin-class <shot>
  
    cell member shot-energy
    
    method do-spawn-shot ( x y delta-x delta-y shot -- )
    method check-shot-collide ( alien-x alien-y shot -- )
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
  
  : check-shots-collide { x y -- collide? }
    false
    max-shot-count 0 ?do
      x y i shot@ check-shot-collide or
    loop
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
      self entity-coord@ try-shoot-bonuses
      self entity-coord@ try-shoot-asteroids or if
        self deactivate-entity exit
      then
      self shot-energy @ interval v- dup { energy }
      self shot-energy !
      energy 0e0 v<= if self deactivate-entity then
    ; define do-update-entity
    
    :noname { alien-x alien-y self -- hit? }
      self entity-active? if
        self entity-coord@ { x y }
        alien-x x v- dup v* alien-y y v- dup v* v+
        alien-radius dup v* v<= dup if
          self deactivate-entity
        then
      else
        false
      then
    ; define check-shot-collide

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
  
  <entity> begin-class <alien-shot>
  
    cell member alien-shot-energy
    
    method do-spawn-alien-shot ( x y delta-x delta-y shot -- )
    method check-alien-shot-collide ( x y shot -- hit? )
    method render-alien-shot ( color display shot -- )
  
  end-class
  
  2e0 constant init-alien-shot-energy
  120 constant max-alien-shot-count
  max-alien-shot-count <alien-shot> class-size * buffer: alien-shots
  
  : alien-shot@ ( index -- shot )
    <alien-shot> class-size * alien-shots +
  ;
  
  : spawn-alien-shot { x y delta-x delta-y -- }
    max-alien-shot-count 0 ?do
      i alien-shot@ { shot }
      shot entity-active? not if
        x y delta-x delta-y shot do-spawn-alien-shot exit
      then
    loop
  ;
  
  : init-alien-shots ( -- )
    max-alien-shot-count 0 ?do <alien-shot> i alien-shot@ init-object loop
  ;
  
  : deactivate-alien-shots ( -- )
    max-alien-shot-count 0 ?do i alien-shot@ deactivate-entity loop
  ;
  
  : check-alien-shots-collide { x y -- hit? }
    false
    max-alien-shot-count 0 ?do
      x y i alien-shot@ check-alien-shot-collide or
    loop
  ;
  
  : update-alien-shots { interval -- }
    max-alien-shot-count 0 ?do interval i alien-shot@ update-entity loop
  ;
  
  : draw-alien-shots { display -- }
    max-alien-shot-count 0 ?do display i alien-shot@ draw-entity loop
  ;
  
  : erase-alien-shots { display -- }
    max-alien-shot-count 0 ?do display i alien-shot@ erase-entity loop
  ;
  
  0 0 255 rgb8 constant alien-shot-color
  4e0 constant alien-shot-size
  
  <alien-shot> begin-implement
    
    :noname { self -- }
      self <entity>->new
      0e0 self alien-shot-energy !
    ; define new
    
    :noname { x y delta-x delta-y self -- }
      x y delta-x delta-y self activate-entity
      init-alien-shot-energy self alien-shot-energy !
    ; define do-spawn-alien-shot
    
    :noname { interval self -- }
      interval self <entity>->do-update-entity
      self entity-coord@ try-shoot-bonuses
      self entity-coord@ try-shoot-asteroids or if
        self deactivate-entity exit
      then
      self alien-shot-energy @ interval v- dup { energy }
      self alien-shot-energy !
      energy 0e0 v<= if self deactivate-entity then
    ; define do-update-entity
    
    :noname { ship-x ship-y self -- hit? }
      self entity-active? if
        self entity-coord@ { x y }
        ship-x x v- dup v* ship-y y v- dup v* v+
        ship-collide-radius dup v* v<= dup if
          self deactivate-entity
        then
      else
        false
      then
    ; define check-alien-shot-collide

    :noname { color display self -- }
      self entity-coord@ { x y }
      self entity-delta@ { delta-x delta-y }
      delta-y vnegate delta-x vnegate vatan2 { angle }
      color x y convert-coord
      x angle vcos alien-shot-size v* v+
      y angle vsin alien-shot-size v* v+ convert-coord
      display draw-pixel-line
    ; define render-alien-shot
    
    :noname { display self -- }
      alien-shot-color display self render-shot
    ; define do-draw-entity
    
    :noname { display self -- }
      bk-color display self render-shot
    ; define do-erase-entity
    
  end-implement
  
  <entity> begin-class <alien>
  
    method do-spawn-alien ( alien -- )
    method do-check-collide-alien ( ship-x ship-y alien -- )
    method render-alien ( color display alien -- )
    
  end-class
  
  <alien> class-size buffer: alien
  
  : init-alien ( -- ) <alien> alien init-object ;
  
  : spawn-alien ( -- )
    alien entity-active? not if alien do-spawn-alien then
  ;
  
  : deactivate-alien ( -- ) alien deactivate-entity ;

  : check-collide-alien ( x y -- hit? ) alien do-check-collide-alien ;
  
  : update-alien ( interval -- ) alien update-entity ;
  
  : draw-alien ( display -- ) alien draw-entity ;
  
  : erase-alien ( display -- ) alien erase-entity ;
  
  <entity> begin-class <ship>
    
    cell member ship-heading
    cell member ship-delta-heading
    
    method do-spawn-ship ( ship -- )
    method turn-ship ( angle ship -- )
    method thrust-ship ( speed ship -- )
    method ship-shoot ( ship -- )
    method ship-shield ( ship -- )
    method render-ship ( color display ship -- )
    method render-shield ( color display ship -- )
    
  end-class
  
  <ship> class-size buffer: ship
  
  : init-ship ( -- ) <ship> ship init-object ;
  
  : spawn-ship ( -- )
    0e0 shield-active !
    0e0 shot-delay-time !
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
      shot-delay-time @ v0= if
        self ship-heading @ { heading }
        heading vcos { heading-cos }
        heading vsin { heading-sin }
        self entity-delta@ { delta-x delta-y }
        self entity-coord@
        delta-x heading-cos base-shot-speed v* v+
        delta-y heading-sin base-shot-speed v* v+
        spawn-shot
        heading-cos recoil-speed v* heading-sin recoil-speed v*
        self adjust-entity-delta
        shot-delay @ shot-delay-time !
      then
    ; define ship-shoot
    
    :noname { interval self -- }
      interval self <entity>->do-update-entity
      retro @ if retro-decel interval v* dup self decel-entity-delta then
      self ship-heading @ { heading }
      self ship-delta-heading @ { delta-heading }
      delta-heading v0<> if
        heading delta-heading interval v* v+
        self ship-heading !
        delta-heading vabs ship-heading-decel interval v* v-
        0e0 vmax delta-heading dup vabs v/ v*
        self ship-delta-heading !
      then
      true self entity-coord@ try-collide-bonuses
      true self entity-coord@ check-collide-asteroids
      self entity-coord@ check-alien-shots-collide or
      self entity-coord@ check-collide-alien or if
        wave-start @ not shield-active @ v0= and if
          beep
          init-shield shield !
          false retro !
          slow-shot-delay shot-delay !
          -1 lives +!
          lives @ 0>= if spawn-ship then
        then
      then
      wave-start @ not shield-active @ v0> and if
        shield-rate interval v* shield @ v+ 0e0 vmax shield !
      then
      shield-active @ v0> if
        shield-active @ interval v- 0e0 vmax shield-active !
      then
      shield @ 0e0 = if 0e0 shield-active ! then
    ; define do-update-entity
    
    :noname { self -- }
      shield @ v0> if shield-persist shield-active ! then
    ; define ship-shield
    
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
    
    :noname { color display self -- }
      self entity-coord@ { x y }
      color x y convert-coord ship-shield-radius v>n
      display draw-pixel-circle
    ; define render-shield
    
    :noname { display self -- }
      ship-color display self render-ship
      shield-active @ v0> wave-start @ or if
        shield-color display self render-shield
      then
    ; define do-draw-entity
    
    :noname { display self -- }
      bk-color display self render-ship
      bk-color display self render-shield
    ; define do-erase-entity
    
  end-implement
  
  0 0 255 rgb8 constant alien-color
  
  <alien> begin-implement
    
    :noname { self -- }
      self <entity>->new
    ; define new
    
    :noname { self -- }
      vrandom 0.5e0 v< { right }
      right if arena-width else 0e0 then
      arena-height vrandom v*
      right if -1e0 else 1e0 then alien-speed v* 0e0
      self activate-entity
    ; define do-spawn-alien
    
    :noname { interval self -- }
      interval self <entity>->do-update-entity
      interval alien-shot-chance interval-chance if
        ship entity-coord@ { sx sy }
        self entity-coord@ { ax ay }
        sy ay v- sx ax v- vatan2 alien-shot-error vrandom 0.5e0 v- v* v+
        { angle }
        ax ay angle vcos alien-shot-speed v* angle vsin alien-shot-speed v*
        spawn-alien-shot
      then
      false self entity-coord@ try-collide-bonuses
      false self entity-coord@ check-collide-asteroids drop
      self entity-coord@ check-shots-collide if
        self deactivate-entity exit
      then
      self entity-delta@ drop v0> self entity-coord@ drop arena-width v> and if
        self deactivate-entity exit
      then
      self entity-delta@ drop v0< self entity-coord@ drop v0< and if
        self deactivate-entity
      then
    ; define do-update-entity
    
    :noname { ship-x ship-y self -- hit? }
      self entity-active? if
        self entity-coord@ { x y }
        ship-x x v- dup v* ship-y y v- dup v* v+
        alien-radius ship-collide-radius v+ dup v* v<= dup if
          shield-active @ v0> if self deactivate-entity then
        then
      else
        false
      then
    ; define do-check-collide-alien

    :noname { color display self -- }
      self entity-coord@ { x y }
      color x y convert-coord alien-radius v>n
      display draw-filled-circle
    ; define render-alien
    
    :noname { display self -- }
      alien-color display self render-alien
    ; define do-draw-entity
    
    :noname { display self -- }
      bk-color display self render-alien
    ; define do-erase-entity
    
  end-implement
  
  : init-world ( -- )
    init-asteroids init-bonuses init-shots init-alien-shots init-ship
    init-alien
  ;
  
  initializer init-world
  
  : deactivate-world ( -- )
    deactivate-asteroids deactivate-bonuses deactivate-shots
    deactivate-alien-shots deactivate-ship deactivate-alien
  ;
    
  : update-world { interval -- }
    interval bonus-chance interval-chance if start-bonus then
    interval alien-chance interval-chance if spawn-alien then
    interval update-bonuses
    interval update-asteroids
    interval update-shots
    interval update-alien-shots
    interval update-ship
    interval update-alien
    shot-delay-time @ interval v- 0e0 vmax shot-delay-time !
  ;
  
  : draw-world { display -- }
    display draw-bonuses
    display draw-asteroids
    display draw-shots
    display draw-alien-shots
    display draw-alien
    display draw-ship
    display draw-info
    display draw-paused
  ;
  
  : erase-world { display -- }
    display erase-bonuses
    display erase-asteroids
    display erase-shots
    display erase-alien-shots
    display erase-alien
    display erase-ship
    display erase-info
    display erase-paused
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
  
  : handle-shield ( -- ) ship ship-shield ;
  
  : empty-keys ( -- ) begin key? while key drop repeat ;

  : handle-screenshot ( -- )
    [:
      screenshot-fs@ { fs }
      fs if
        screenshot-path@ fs ['] take-screenshot try-and-display-error 0<> if
          drop 2drop
        then
      then
    ;] console::with-serial-error-output
  ;
  
  : handle-pause ( -- ) true paused xor! ;
  
  : handle-key ( -- exit? )
    reset-keymap
    update-keymap
    [char] q keymap-pressed@ if true exit then
    paused @ not if
      UP_ARROW keymap-pressed@ if handle-thrust then
      RIGHT_ARROW keymap-pressed@ if handle-turn-right then
      LEFT_ARROW keymap-pressed@ if handle-turn-left then
      bl keymap-pressed@ if handle-shoot then
      [char] ; keymap-pressed@ if handle-shield then
    then
    [char] p keymap-pressed@ if handle-pause then
    false
  ;
  
  2 constant init-lives
  5 constant extra-life-wave
  
  : play-rocks ( -- )
    false paused !
    0 wave !
    init-lives lives !
    init-shield shield !
    0e0 shield-active !
    false retro !
    slow-shot-delay shot-delay !
    0e0 shot-delay-time !
    begin
      ansi-term::hide-cursor
      true raw-keys-enabled!
      clear-keymap
      [: dup clear-pixmap update-display ;] with-term-display
      start-world
      systick::systick-counter { last-systick }
      begin
        last-systick [: { last-systick display }
          display erase-world
          systick::systick-counter { current-systick }
          paused @ not if
            wave-start @ if
              current-systick wave-start-systick @ -
              wave-start-ticks < wave-start !
            then
            current-systick last-systick - u>v 10000e0 v/
            update-world
          else
            wave-start @ if
              current-systick last-systick - wave-start-systick +!
            then
          then
          handle-key { exit-key? }
          display draw-world
          display update-display
          current-systick exit-key?
        ;] with-term-display
        { exit-key? } to last-systick
        [char] s keymap-released@ if handle-screenshot then
        exit-key? lives @ 0< or if
          [: dup clear-pixmap update-display ;]
          with-term-display
          page
          ." *** GAME OVER ***" cr cr
          ." You survived " wave @ . ." waves" cr
          false raw-keys-enabled!
          clear-keymap
          1000 ms
          empty-keys
          ansi-term::show-cursor
          exit
        then
        asteroid-count 0=
      until
      1 wave +!
      wave @ extra-life-wave umod 0= if 1 lives +! then
    again
  ;
  
end-module
