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

begin-module snow-wind

  oo import
  bitmap import
  ssd1306 import
  font import
  simple-font import
  rng import
  systick import
  
  begin-structure flake-size
    field: flake-active
    2field: flake-x
    2field: flake-y
    2field: flake-x-delta
    2field: flake-y-delta
  end-structure
  
  128 constant my-width
  64 constant my-height
  7 constant my-char-width
  8 constant my-char-height
  200 constant my-flake-count
  false constant my-accumulate
  2,0 2constant my-divider
  2,0 my-divider f/ 2constant my-flake-chance
  50,0 my-divider f/ nip constant my-delay
  -0,25 my-divider f/ 2constant my-min-flake-x-delta-vary
  0,25 my-divider f/ 2constant my-max-flake-x-delta-vary
  -0,25 my-divider f/ 2constant my-min-flake-y-delta-vary
  0,25 my-divider f/ 2constant my-max-flake-y-delta-vary
  -0,75 my-divider f/ 2constant my-min-wind-x-delta
  0,75 my-divider f/ 2constant my-max-wind-x-delta
  -0,25 my-divider f/ 2constant my-min-wind-x-delta-vary
  0,25 my-divider f/ 2constant my-max-wind-x-delta-vary
  
  my-width my-height bitmap-buf-size constant my-buf-size
  my-buf-size 4 align buffer: my-buf
  <ssd1306> class-size buffer: my-ssd1306
  flake-size my-flake-count * buffer: my-flakes
  variable free-flake-count
  2variable new-flake-x-delta
  2variable new-flake-y-delta

  false value inited?

  : x-no-flakes-available ( -- ) ." no flakes available" cr ;

  : flake@ ( index -- addr ) flake-size * my-flakes + ;

  : flake-coord@ { flake -- x y }
    flake flake-x 2@ nip flake flake-y 2@ nip
  ;
  
  : init-snow ( -- )
    14 15 my-buf my-width my-height SSD1306_I2C_ADDR 1
    <ssd1306> my-ssd1306 init-object
    init-simple-font
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
    my-ssd1306 clear-bitmap
    my-ssd1306 update-display
    0,0 new-flake-x-delta 2! vary-wind new-flake-x-delta 2+!
    1,0 my-divider f/ new-flake-y-delta 2!
  ;

  : free-flake? ( -- free? ) free-flake-count @ 0> ;

  : new-flake? ( -- new? )
    free-flake? if random 0 my-flake-chance d<= else false then
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
      [char] * flake flake-coord@ my-ssd1306 a-simple-font
      or-char
    ;] for-all-flakes
    my-ssd1306 update-display
  ;

  : erase-snow ( -- )
    [: { flake }
      [char] * flake flake-coord@ my-ssd1306 a-simple-font
      bic-char
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

  : snow-fall ( -- )
    [: { flake }
      flake flake-x-delta 2@ flake flake-x 2+!
      flake flake-y-delta 2@ flake flake-y 2+!
    ;] for-all-flakes
  ;

  : adjust-wind ( -- )
    vary-wind 2dup new-flake-x-delta 2+!
    [: { flake } 2dup flake flake-x-delta 2+! ;] for-all-flakes
    2drop
  ;

  : draw-snow-w/-wind ( -- )
    inited? not if init-snow then
    clear-snow
    systick-counter { start-systick }
    begin key? not while
      draw-snow
      my-accumulate if free-snow then
      erase-snow
      my-accumulate not if free-snow then
      snow-fall
      adjust-wind
      new-flake? if init-flake then
      my-delay start-systick task::current-task task::delay
      my-delay +to start-systick
    repeat
    key drop
  ;
  
  : run-snow-w/-wind ( -- )
    inited? not if init-snow then
    clear-snow
    0 [:
      systick-counter { start-systick }
      begin
        draw-snow
        my-accumulate if free-snow then
        erase-snow
        my-accumulate not if free-snow then
        snow-fall
        adjust-wind
        new-flake? if init-flake then
        my-delay start-systick task::current-task task::delay
        my-delay +to start-systick
      again
    ;] 320 128 512 task::spawn task::run
  ;
  
end-module
