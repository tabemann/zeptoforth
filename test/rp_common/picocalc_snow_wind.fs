\ Copyright (c) 2022-2025 Travis Bemann
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

begin-module snow-wind

  oo import
  pixmap8 import
  font import
  picocalc-term-common import
  picocalc-term import
  rng import
  tinymt32 import
  systick import
  task import
  task-pool import
  
  use-st7789v? not [if]
    ili9488-8-common import
    ili9488-8-spi import
  [else]
    st7789v-8-common import
    st7789v-8-spi import
  [then]

  use-5x8-font? [if]
    simple-font-5x8 import
  [then]
  use-6x8-font? [if]
    simple-font-6x8 import
  [then]
  use-7x8-font? [if]
    simple-font import
  [then]

  begin-structure flake-size
    field: flake-active
    2field: flake-x
    2field: flake-y
    2field: flake-x-delta
    2field: flake-y-delta
  end-structure

  term-pixels-dim@ constant my-height constant my-width
  term-char-dim@ constant my-char-height constant my-char-width
  400 constant my-flake-count
  false constant my-accumulate
  2,0 2constant my-divider
  2,0 my-divider f/ 2constant my-flake-chance
  1000,0 my-divider f/ nip constant my-delay
  -0,25 my-divider f/ 2constant my-min-flake-x-delta-vary
  0,25 my-divider f/ 2constant my-max-flake-x-delta-vary
  -0,25 my-divider f/ 2constant my-min-flake-y-delta-vary
  0,25 my-divider f/ 2constant my-max-flake-y-delta-vary
  -0,75 my-divider f/ 2constant my-min-wind-x-delta
  0,75 my-divider f/ 2constant my-max-wind-x-delta
  -0,25 my-divider f/ 2constant my-min-wind-x-delta-vary
  0,25 my-divider f/ 2constant my-max-wind-x-delta-vary
  
  flake-size my-flake-count * buffer: my-flakes
  tinymt32-size buffer: my-prng
  1 task-pool-size buffer: my-task-pool
  variable free-flake-count
  2variable new-flake-x-delta
  2variable new-flake-y-delta
  variable continue-run?

  false value inited?

  : x-no-flakes-available ( -- ) ." no flakes available" cr ;

  : flake@ ( index -- addr ) flake-size * my-flakes + ;

  : flake-coord@ { flake -- x y }
    flake flake-x 2@ nip flake flake-y 2@ nip
  ;
  
  : init-snow ( -- )
    random my-prng tinymt32-init
    my-prng tinymt32-prepare-example
    1024 128 1024 1 my-task-pool init-task-pool
    true to inited?
  ;

  : random ( -- x )
    my-prng tinymt32-generate-uint32
  ;

  : vary-wind ( -- f )
    random 0 my-max-wind-x-delta-vary my-min-wind-x-delta-vary d- f*
    my-min-wind-x-delta-vary d+ new-flake-x-delta 2@ d+
    my-min-wind-x-delta dmax my-max-wind-x-delta dmin
    new-flake-x-delta 2@ d-
  ;
  
  : clear-snow { display -- }
    my-flake-count free-flake-count !
    my-flake-count 0 do false i flake@ flake-active ! loop
    display clear-pixmap
    display update-display
    0,0 new-flake-x-delta 2! vary-wind new-flake-x-delta 2+!
    1,0 my-divider f/ new-flake-y-delta 2!
  ;

  : free-flake? ( -- free? ) free-flake-count @ 0> ;

  : new-flake? { D: interval -- new? }
    free-flake? if random 0 my-flake-chance interval f* d<= else false then
  ;

  : find-free-flake ( -- index )
    free-flake-count @ 0> averts x-no-flakes-available
    -1 free-flake-count +!
    my-flake-count 0 do i flake@ flake-active @ not if unloop i exit then loop
  ;

  : init-flake ( -- )
    find-free-flake flake@ { flake }
    true flake flake-active !
    random my-width 2 * umod my-width 2 / - 0 swap flake flake-x 2!
    0,0 flake flake-y 2!
    random 0 my-max-flake-x-delta-vary my-min-flake-x-delta-vary d- f*
    my-min-flake-x-delta-vary d+ new-flake-x-delta 2@ d+ flake flake-x-delta 2!
    random 0 my-max-flake-y-delta-vary my-min-flake-y-delta-vary d- f*
    my-min-flake-y-delta-vary d+ new-flake-y-delta 2@ d+ flake flake-y-delta 2!
  ;

  : for-all-flakes { xt -- }
    my-flake-count 0 do
      i flake@ flake-active @ if
        i flake@ xt execute
      then
    loop
  ;
        
  : draw-snow ( display -- )
    [: { display flake }
      255 255 255 rgb8 [char] * flake flake-coord@ display
      [ use-5x8-font? ] [if] a-simple-font-5x8 [then]
      [ use-6x8-font? ] [if] a-simple-font-6x8 [then]
      [ use-7x8-font? ] [if] a-simple-font [then]
      draw-char-to-pixmap8
      display
    ;] for-all-flakes
    update-display
  ;

  : erase-snow ( display -- )
    [: { display flake }
      0 0 0 rgb8 [char] * flake flake-coord@ display
      [ use-5x8-font? ] [if] a-simple-font-5x8 [then]
      [ use-6x8-font? ] [if] a-simple-font-6x8 [then]
      [ use-7x8-font? ] [if] a-simple-font [then]
      draw-char-to-pixmap8
      display
    ;] for-all-flakes
    drop
  ;

  : free-snow ( -- )
    [: { flake }
      flake flake-y 2@ 0 my-height my-char-height 2 / - d>= if
        false flake flake-active !
        1 free-flake-count +!
      then
    ;] for-all-flakes
  ;

  : snow-fall ( D: interval -- )
    [: { flake }
      2dup flake flake-x-delta 2@ f* flake flake-x 2+!
      2dup flake flake-y-delta 2@ f* flake flake-y 2+!
    ;] for-all-flakes
    2drop
  ;

  : adjust-wind ( D: interval -- )
    vary-wind f* 2dup new-flake-x-delta 2+!
    [: { flake } 2dup flake flake-x-delta 2+! ;] for-all-flakes
    2drop
  ;

  : draw-snow-w/-wind ( -- )
    inited? not if init-snow then
    [: clear-snow ;] with-term-display
    systick-counter { start-systick }
    begin key? not while
      systick-counter { new-systick }
      new-systick start-systick - s>f my-delay s>f f/ { D: interval }
      new-systick to start-systick
      [: { display }
        display draw-snow
        my-accumulate if free-snow then
        display erase-snow
      ;] with-term-display
      my-accumulate not if free-snow then
      interval snow-fall
      interval adjust-wind
      interval new-flake? if init-flake then
    repeat
    [: clear-snow ;] with-term-display
    key drop
  ;
  
  : run-snow-w/-wind ( -- )
    inited? not if init-snow then
    [: clear-snow ;] with-term-display
    true continue-run? !
    0 [:
      systick-counter { start-systick }
      begin continue-run? @ while
        systick-counter { new-systick }
        new-systick start-systick - s>f my-delay s>f f/ { D: interval }
        new-systick to start-systick
        [: { display }
          display draw-snow
          my-accumulate if free-snow then
          display erase-snow
        ;] with-term-display
        my-accumulate not if free-snow then
        interval snow-fall
        interval adjust-wind
        interval new-flake? if init-flake then
      repeat
      [: clear-snow ;] with-term-display
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : stop-snow-w/-wind ( -- ) false continue-run? ! ;
  
end-module
