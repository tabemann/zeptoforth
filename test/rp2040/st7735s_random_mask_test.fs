\ Copyright (c) 2023 Travis Bemann
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

begin-module st7735s-random-test

  oo import
  bitmap import
  pixmap16 import
  font import
  simple-font import
  st7735s import
  rng import

  \ Columns
  160 constant my-cols

  \ Rows
  80 constant my-rows

  \ Backend columns
  8 constant my-back-cols

  \ Backend rows
  8 constant my-back-rows
  
  \ SPI device
  1 constant my-device

  \ Pins
  11 constant lcd-din 
  10 constant lcd-clk
  8 constant lcd-dc
  12 constant lcd-rst
  9 constant lcd-cs
  25 constant lcd-bl

  \ Buffer
  my-cols my-rows pixmap16-buf-size buffer: my-buffer

  \ Mask buffer
  my-back-cols my-back-rows bitmap-buf-size buffer: my-mask-buffer

  \ Masked buffer
  my-back-cols my-back-rows pixmap16-buf-size buffer: my-masked-buffer

  \ Display
  <st7735s> class-size buffer: my-display

  \ Mask
  <bitmap> class-size buffer: my-mask

  \ Masked pixmap
  <pixmap16> class-size buffer: my-masked

  \ Initialize the test:
  : init-test
    init-simple-font
    lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
    my-buffer my-cols my-rows my-device <st7735s> my-display init-object
    my-mask-buffer my-back-cols my-back-rows <bitmap> my-mask init-object
    my-masked-buffer my-back-cols my-back-rows <pixmap16> my-masked init-object
    my-display clear-pixmap
    my-display update-display
  ;

  initializer init-test

  \ Generate a random coordinate
  : random-coord ( -- col row )
    random 0 my-cols s>f f* f>s random 0 my-rows s>f f* f>s
  ;

  \ Generate a random rectangle
  : random-rect ( -- col cols row rows )
    random-coord { first-col first-row }
    random-coord { second-col second-row }
    first-col second-col min { start-col }
    first-col second-col max { end-col }
    first-row second-row min { start-row }
    first-row second-row max { end-row }
    start-col end-col over - start-row end-row over -
  ;

  \ Generate a random color
  : random-color ( -- color16 )
    random 255 and random 255 and random 255 and rgb16
  ;

  \ Generate a random character
  : random-char ( -- char )
    [ $75 $21 - ] literal s>f random 0 f* f>s $21 +
  ;
  
  \ Draw a random character
  : draw-random-char ( -- )
    my-mask clear-bitmap
    random-char 0 0 op-set my-mask a-simple-font draw-char
    random-color 0 0 my-back-cols my-back-rows my-masked draw-rect-const
    random-coord { dst-col dst-row }
    0 0 0 0 dst-col dst-row 8 8 my-mask my-masked my-display draw-rect-mask
  ;

  \ Carry out the test
  : run-test ( -- )
    begin key? not while
      draw-random-char
      my-display update-display
\      500 ms
    repeat
    key drop
  ;

end-module
