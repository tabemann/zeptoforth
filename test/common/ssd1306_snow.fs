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

begin-module snow

  oo import
  bitmap import
  ssd1306 import
  font import
  simple-font import
  rng import
  
  begin-structure flake-size
    field: flake-active
    field: flake-col
    field: flake-row
  end-structure
  
  128 constant my-width
  64 constant my-height
  7 constant my-char-width
  8 constant my-char-height
  100 constant my-flake-count
  0,35 2constant my-flake-chance

  my-width my-height bitmap-buf-size constant my-buf-size
  my-buf-size 4 align buffer: my-buf
  <ssd1306> class-size buffer: my-ssd1306
  flake-size my-flake-count * buffer: my-flakes
  variable free-flake-count

  false value inited?

  : x-no-flakes-available ( -- ) ." no flakes available" cr ;

  : flake@ ( index -- addr ) flake-size * my-flakes + ;

  : init-snow ( -- )
    14 15 my-buf my-width my-height SSD1306_I2C_ADDR 1
    <ssd1306> my-ssd1306 init-object
    init-simple-font
    true to inited?
  ;

  : clear-snow ( -- )
    my-flake-count free-flake-count !
    my-flake-count 0 do false i flake@ flake-active ! loop
    my-ssd1306 clear-bitmap
    my-ssd1306 update-display
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
    random my-width my-char-width - umod flake flake-col !
    0 flake flake-row !
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
      [char] * flake flake-col @ flake flake-row @ my-ssd1306 a-simple-font
      or-char
    ;] for-all-flakes
    my-ssd1306 update-display
  ;

  : erase-snow ( -- )
    [: { flake }
      [char] * flake flake-col @ flake flake-row @ my-ssd1306 a-simple-font
      bic-char
    ;] for-all-flakes
  ;

  : free-snow ( -- )
    [: { flake }
      flake flake-row @ my-height my-char-height 2 / - = if
        false flake flake-active !
        1 free-flake-count +!
      then
    ;] for-all-flakes
  ;

  : snow-fall ( -- ) [: 1 swap flake-row +! ;] for-all-flakes ;

  : draw-snow ( -- )
    inited? not if init-snow then
    clear-snow
    begin key? not while
      draw-snow
      free-snow
      erase-snow
      snow-fall
      new-flake? if init-flake then
      50 ms
    repeat
    key drop
  ;
  
end-module
