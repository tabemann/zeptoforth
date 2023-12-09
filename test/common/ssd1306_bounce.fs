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

begin-module ssd1306-test

  oo import
  bitmap import
  ssd1306 import
  rng import
  
  \ An I2C SSD1306 device
  <ssd1306> class-size buffer: my-ssd1306
  
  \ A backinb bitmap
  <bitmap> class-size buffer: my-sprite
  
  \ The number of columns
  128 constant my-cols

  \ The number of rows
  64 constant my-rows
  
  \ The width of the sprite
  4 constant width
  
  \ The height of the sprite
  4 constant height

  \ My framebuffer size
  my-cols my-rows bitmap-buf-size constant my-buf-size

  \ My framebuffer
  my-buf-size buffer: my-buf
  
  \ My sprite buffer size
  width height bitmap-buf-size constant my-sprite-buf-size
  
  \ My sprite buffer
  my-sprite-buf-size buffer: my-sprite-buf
  
  \ Are we initialized
  false value inited?
  
  \ Initialize the sprite
  : init-sprite ( -- )
    my-sprite-buf width height <bitmap> my-sprite init-object
    my-sprite clear-bitmap
    $FF 1 0 2 1 op-set my-sprite draw-rect-const
    $FF 0 1 4 2 op-set my-sprite draw-rect-const
    $FF 1 3 2 1 op-set my-sprite draw-rect-const
  ;
  
  \ Initialize the test
  : init-test ( -- )
    14 15 my-buf my-cols my-rows SSD1306_I2C_ADDR 1
    <ssd1306> my-ssd1306 init-object
    init-sprite
  ;
  
  \ Bounce a square around the display
  : bounce-square ( -- )
    inited? not if init-test true to inited? then
    0 0 1 1 { column row delta-column delta-row }
    begin key? not while
      $FF column row width height op-xor my-ssd1306 draw-rect-const
      my-ssd1306 update-display
      column width + my-cols >= if
        -1 to delta-column
      else
        column 0 <= if
          1 to delta-column
        then
      then
      row height + my-rows >= if
        -1 to delta-row
      else
        row 0 <= if
          1 to delta-row
        then
      then
      delta-column +to column
      delta-row +to row
      100 ms
    repeat
    key drop
  ;
  
  \ Bounce a sprite around the display
  : bounce-sprite ( -- )
    inited? not if init-test true to inited? then
    0 0 1 1 { column row delta-column delta-row }
    begin key? not while
      0 0 column row width height op-or my-sprite my-ssd1306 draw-rect
      my-ssd1306 update-display
      column width + my-cols >= if
        -1 to delta-column
      else
        column 0 <= if
          1 to delta-column
        then
      then
      row height + my-rows >= if
        -1 to delta-row
      else
        row 0 <= if
          1 to delta-row
        then
      then
      delta-column +to column
      delta-row +to row
      100 ms
    repeat
    key drop
  ;
  
  \ Bounce a pixel around the display
  : bounce-pixel ( -- )
    inited? not if init-test true to inited? then
    my-cols 5 / 0 1 1 { column row delta-column delta-row }
    begin key? not while
      $FF column row op-xor my-ssd1306 draw-pixel-const
      my-ssd1306 update-display
      column my-cols >= if
        -1 to delta-column
      else
        column 0 <= if
          1 to delta-column
        then
      then
      row my-rows >= if
        -1 to delta-row
      else
        row 0 <= if
          1 to delta-row
        then
      then
      delta-column +to column
      delta-row +to row
      100 ms
    repeat
    key drop
  ;
  
  \ Bounce a pixel around the display, displaying full cycles
  : bounce-pixel-cycle ( -- )
    inited? not if init-test true to inited? then
    my-cols 5 / 1 { cycle delta-cycle }
    cycle 0 1 1 { column row delta-column delta-row }
    begin key? not while
      $FF column row op-xor my-ssd1306 draw-pixel-const
      column my-cols >= if
        -1 to delta-column
      else
        column 0 <= if
          1 to delta-column
        then
      then
      row my-rows >= if
        -1 to delta-row
      else
        row 0 <= if
          1 to delta-row
        then
      then
      delta-column +to column
      delta-row +to row
      column cycle = row 0= and if
        my-ssd1306 update-display
        cycle my-cols >= if
          -1 to delta-cycle
        else
          cycle 0 <= if
            1 to delta-cycle
          then
        then
        delta-cycle +to cycle
        cycle to column
        100 ms
      then
    repeat
    key drop
  ;
  
  \ XOR sprites at random coordinates
  : random-sprites ( -- )
    inited? not if init-test true to inited? then
    begin key? not while
      0 0 random my-cols width - umod random my-rows height - umod width height
      op-xor my-sprite my-ssd1306 draw-rect
      my-ssd1306 update-display
    repeat
    key drop
  ;
  
  \ XOR pixels at random coordinates
  : random-pixels ( -- )
    inited? not if init-test true to inited? then
    begin key? not while
      256 0 do
        $FF random my-cols umod random my-rows umod
        op-xor my-ssd1306 draw-pixel-const
      loop
      my-ssd1306 update-display
    repeat
    key drop
  ;
  
end-module