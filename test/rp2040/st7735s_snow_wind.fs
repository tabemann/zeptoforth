\ Copyright (c) 2022-2023 Travis Bemann
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
  pixmap16 import
  st7735s import
  font import
  simple-font import
  rng import
  systick import
  task import
  task-pool import
  
  begin-structure flake-size
    field: flake-active
    2field: flake-x
    2field: flake-y
    2field: flake-x-delta
    2field: flake-y-delta
  end-structure
  
  160 constant my-width
  80 constant my-height
  7 constant my-char-width
  8 constant my-char-height
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
  
  my-width my-height pixmap16-buf-size constant my-buf-size
  my-buf-size 4 align buffer: my-buf
  <st7735s> class-size buffer: my-st7735s
  flake-size my-flake-count * buffer: my-flakes
  1 task-pool-size buffer: my-task-pool
  variable free-flake-count
  2variable new-flake-x-delta
  2variable new-flake-y-delta
  variable continue-run?

  false value inited?
  
  \ SPI device
  1 constant my-device
  
  \ Pins
  11 constant lcd-din 
  10 constant lcd-clk
  8 constant lcd-dc
  12 constant lcd-rst
  9 constant lcd-cs
  25 constant lcd-bl

  \ Colors
  0 0 0 rgb16 constant back-color
  127 127 255 rgb16 constant fore-color
  
  : x-no-flakes-available ( -- ) ." no flakes available" cr ;

  : flake@ ( index -- addr ) flake-size * my-flakes + ;

  : flake-coord@ { flake -- x y }
    flake flake-x 2@ nip flake flake-y 2@ nip
  ;
  
  : init-snow ( -- )
    lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
    my-buf my-width my-height my-device
    <st7735s> my-st7735s init-object
    init-simple-font
    320 128 512 1 my-task-pool init-task-pool
    true to inited?
  ;

  : vary-wind ( -- f )
    random 0 my-max-wind-x-delta-vary my-min-wind-x-delta-vary d- f*
    my-min-wind-x-delta-vary d+ new-flake-x-delta 2@ d+
    my-min-wind-x-delta dmax my-max-wind-x-delta dmin
    new-flake-x-delta 2@ d-
  ;
  
  : clear-snow ( -- )
    my-flake-count free-flake-count !
    my-flake-count 0 do false i flake@ flake-active ! loop
    my-st7735s clear-pixmap
    my-st7735s update-display
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
        
  : draw-snow ( -- )
    [: { flake }
      fore-color [char] * flake flake-coord@ my-st7735s a-simple-font
      draw-char-to-pixmap16
    ;] for-all-flakes
    my-st7735s update-display
  ;

  : erase-snow ( -- )
    [: { flake }
      back-color [char] * flake flake-coord@ my-st7735s a-simple-font
      draw-char-to-pixmap16
    ;] for-all-flakes
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
    clear-snow
    systick-counter { start-systick }
    begin key? not while
      systick-counter { new-systick }
      new-systick start-systick - s>f my-delay s>f f/ { D: interval }
      new-systick to start-systick
      draw-snow
      my-accumulate if free-snow then
      erase-snow
      my-accumulate not if free-snow then
      interval snow-fall
      interval adjust-wind
      interval new-flake? if init-flake then
    repeat
    clear-snow
    key drop
  ;
  
  : run-snow-w/-wind ( -- )
    inited? not if init-snow then
    clear-snow
    true continue-run? !
    0 [:
      systick-counter { start-systick }
      begin continue-run? @ while
        systick-counter { new-systick }
        new-systick start-systick - s>f my-delay s>f f/ { D: interval }
        new-systick to start-systick
        draw-snow
        my-accumulate if free-snow then
        erase-snow
        my-accumulate not if free-snow then
        interval snow-fall
        interval adjust-wind
        interval new-flake? if init-flake then
      repeat
      clear-snow
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : stop-snow-w/-wind ( -- ) false continue-run? ! ;
  
end-module
