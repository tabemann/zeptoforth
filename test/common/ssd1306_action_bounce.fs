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

  \ The particle structure
  begin-structure particle-size
    
    \ The last systick at which the particle was updated
    field: last-systick
    
    \ The current coordinates of the particle
    2field: particle-x
    2field: particle-y
    
    \ The current delta of the particle
    2field: particle-x-delta
    2field: particle-y-delta
    
  end-structure

  \ The width and height of the display in pixels
  128 constant my-width
  64 constant my-height
  
  \ The width and height of the sprite in pixels
  4 constant my-sprite-width
  4 constant my-sprite-height
  
  \ The number of particles to create
  60 constant my-particle-count
  
  \ The speed of the particles
  20,0 2constant my-particle-speed
  
  \ The size in bytes of the SSD1306 framebuffer
  my-width my-height bitmap-buf-size constant my-buf-size
  
  \ The SSD1306 framebuffer
  my-buf-size 4 align buffer: my-buf
  
  \ The SSD1306 display
  <ssd1306> class-size buffer: my-ssd1306
  
  \ The size in bytes of the sprite buffer
  my-sprite-width my-sprite-height bitmap-buf-size constant my-sprite-buf-size
  
  \ The sprite buffer
  my-sprite-buf-size 4 align buffer: my-sprite-buf
  
  \ The sprite
  <bitmap> class-size buffer: my-sprite
  
  \ The task pool, for running the particle system
  1 task-pool-size buffer: my-task-pool
  
  \ The schedule for the particles and the display updater
  schedule-size buffer: my-schedule
  
  \ The action pool for the particles and the display updater
  my-particle-count 1+ action-pool-size buffer: my-action-pool
  
  \ The memory pool for the particles
  pool-size buffer: my-pool
  
  \ The size in bytes of the memory for the memory pool
  particle-size my-particle-count * constant my-pool-size
  
  \ The memory for the memory pool
  my-pool-size buffer: my-pool-data
  
  \ Whether the display, the sprite, and the particle system have been initialized
  false value inited?
  
  \ How many particles are ready for display
  variable particle-ready-count
  
  \ Initialize the display
  : init-display ( -- )
    14 15 my-buf my-width my-height SSD1306_I2C_ADDR 1
    <ssd1306> my-ssd1306 init-object
  ;
  
  \ Initialize the sprite
  : init-sprite ( -- )
    my-sprite-buf my-sprite-width my-sprite-height
    <bitmap> my-sprite init-object
    my-sprite clear-bitmap
    $FF 1 2 0 1 my-sprite set-rect-const
    $FF 0 4 1 2 my-sprite set-rect-const
    $FF 1 2 3 1 my-sprite set-rect-const
  ;
  
  \ Initialize the test overall
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
  
  \ Draw a particle to the display (or erase it, as it is using XOR)
  : draw-particle ( -- )
    0 current-data particle-x 2@ nip my-sprite-width
    0 current-data particle-y 2@ nip my-sprite-height
    my-sprite my-ssd1306 xor-rect
  ;
  
  \ Bounce a particle off the edges of the display
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
  
  \ Move a particle
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
  
  \ Carry out one cycle of a particle
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
  
  \ Initialize a particle
  : init-particle ( -- )
    my-pool allocate-pool { particle }
    systick-counter particle last-systick !
    random 0 2,0 f* { angle-lo angle-hi }
    random 0 0 my-width my-sprite-width - f* particle particle-x 2!
    random 0 0 my-height my-sprite-height - f* particle particle-y 2!
    angle-lo angle-hi cos my-particle-speed f* particle particle-x-delta 2!
    angle-lo angle-hi sin my-particle-speed f* particle particle-y-delta 2!
    my-schedule particle ['] particle-cycle
    my-action-pool add-action-from-pool drop
  ;
  
  \ Update the display if all the particles are ready
  defer display-update
  :noname
    particle-ready-count @ my-particle-count = if
      my-ssd1306 update-display
    then
    ['] display-update yield-action
  ; ' display-update defer!
  
  \ Initialize the display updater
  : init-updater ( -- )
    my-schedule 0 ['] display-update
    my-action-pool add-action-from-pool drop
  ;
  
  \ Begin bouncing particles
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
  
  \ Stop bouncing particles
  : stop-bounce ( -- )
    inited? if
      my-schedule stop-schedule
    then
  ;
  
end-module