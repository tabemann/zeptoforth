\ Copyright (c) 2022 Travis Bemann
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

begin-module action-bounce

  oo import
  bitmap import
  ssd1306 import
  rng import
  systick import
  task import
  task-pool import
  action import
  action-pool import
  pool import

  begin-structure particle-size
    field: last-systick
    2field: particle-x
    2field: particle-y
    2field: particle-x-delta
    2field: particle-y-delta
  end-structure

  128 constant my-width
  64 constant my-height
  4 constant my-sprite-width
  4 constant my-sprite-height
  60 constant my-particle-count
  20,0 2constant my-particle-speed
  
  my-width my-height bitmap-buf-size constant my-buf-size
  my-buf-size 4 align buffer: my-buf
  <ssd1306> class-size buffer: my-ssd1306
  
  my-sprite-width my-sprite-height bitmap-buf-size constant my-sprite-buf-size
  my-sprite-buf-size 4 align buffer: my-sprite-buf
  <bitmap> class-size buffer: my-sprite
  
  1 task-pool-size buffer: my-task-pool
  schedule-size buffer: my-schedule
  my-particle-count 1+ action-pool-size buffer: my-action-pool
  pool-size buffer: my-pool
  particle-size my-particle-count * constant my-pool-size
  my-pool-size buffer: my-pool-data

  false value inited?
  variable particle-ready-count
  
  : init-display ( -- )
    14 15 my-buf my-width my-height SSD1306_I2C_ADDR 1
    <ssd1306> my-ssd1306 init-object
  ;
  
  : init-sprite ( -- )
    my-sprite-buf my-sprite-width my-sprite-height
    <bitmap> my-sprite init-object
    my-sprite clear-bitmap
    $FF 1 2 0 1 my-sprite set-rect-const
    $FF 0 4 1 2 my-sprite set-rect-const
    $FF 1 2 3 1 my-sprite set-rect-const
  ;
  
  : init-test ( -- )
    init-display
    init-sprite
    my-schedule init-schedule
    320 128 512 1 my-task-pool init-task-pool
    my-particle-count 1+ my-action-pool init-action-pool
    particle-size my-pool init-pool
    my-pool-data my-pool-size my-pool add-pool
    true to inited?
  ;
  
  : draw-particle ( -- )
    0 current-data particle-x 2@ nip my-sprite-width
    0 current-data particle-y 2@ nip my-sprite-height
    my-sprite my-ssd1306 xor-rect
  ;
  
  : bounce-particle ( -- )
    current-data particle-x 2@ 0,0 d<= if
      current-data particle-x-delta 2@ dabs
      current-data particle-x-delta 2!
    else
      current-data particle-x 2@ 0 my-width my-sprite-width - d>= if
        current-data particle-x-delta 2@ dabs dnegate
        current-data particle-x-delta 2!
      then
    then
    current-data particle-y 2@ 0,0 d<= if
      current-data particle-y-delta 2@ dabs
      current-data particle-y-delta 2!
    else
      current-data particle-y 2@ 0 my-height my-sprite-height - d>= if
        current-data particle-y-delta 2@ dabs dnegate
        current-data particle-y-delta 2!
      then
    then
  ;
  
  : move-particle ( -- )
    systick-counter { current-systick }
    current-systick current-data last-systick @ - { systick-diff }
    current-systick current-data last-systick !
    0 systick-diff 10000,0 f/ { fract-lo fract-hi }
    fract-lo fract-hi current-data particle-x-delta 2@ f*
    current-data particle-x 2+!
    fract-lo fract-hi current-data particle-y-delta 2@ f*
    current-data particle-y 2+!
    bounce-particle
  ;
  
  defer particle-cycle
  :noname
    particle-ready-count @ my-particle-count = if
      draw-particle
      move-particle
    else
      systick-counter current-data last-systick !
    then
    particle-ready-count @ 1+ my-particle-count min particle-ready-count !
    draw-particle
    ['] particle-cycle yield-action
  ; ' particle-cycle defer!
  
  : particle-first-draw ( -- )
    draw-particle
    1 particle-ready-count +!
    ['] particle-cycle yield-action
  ;
  
  : init-particle ( -- )
    my-pool allocate-pool { particle }
    systick-counter particle last-systick !
    random 0 2,0 f* { angle-lo angle-hi }
    random 0 0 my-width my-sprite-width - f* particle particle-x 2!
    random 0 0 my-height my-sprite-height - f* particle particle-y 2!
    angle-lo angle-hi cos my-particle-speed f* particle particle-x-delta 2!
    angle-lo angle-hi sin my-particle-speed f* particle particle-y-delta 2!
    my-schedule particle ['] particle-first-draw
    my-action-pool add-action-from-pool drop
  ;
  
  defer display-update
  :noname
    particle-ready-count @ my-particle-count = if
      my-ssd1306 update-display
    then
    ['] display-update yield-action
  ; ' display-update defer!
  
  : init-updater ( -- )
    my-schedule 0 ['] display-update
    my-action-pool add-action-from-pool drop
  ;
  
  : run-bounce ( -- )
    inited? not if
      init-test
      my-particle-count 0 ?do init-particle loop
      init-updater
    then
    0 [:
      0 particle-ready-count !
      my-schedule run-schedule
      0 particle-ready-count !
      my-ssd1306 clear-bitmap
      my-ssd1306 update-display
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : stop-bounce ( -- )
    inited? if
      my-schedule stop-schedule
    then
  ;
  
end-module