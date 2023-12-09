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
  pixmap16 import
  font import
  simple-font import
  st7735s import
  rng import

  \ Columns
  160 constant my-cols

  \ Rows
  80 constant my-rows

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

  \ Display
  <st7735s> class-size buffer: my-display

  \ Initialize the test:
  : init-test
    init-simple-font
    lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
    my-buffer my-cols my-rows my-device <st7735s> my-display init-object
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

  \ Draw a random rectangle
  : draw-random-char ( -- )
    random-color random-char random-coord my-display a-simple-font
    draw-char-to-pixmap16
  ;

  \ Carry out the test
  : run-test ( -- )
    begin key? not while
      draw-random-char
      my-display update-display
\      100 ms
    repeat
    key drop
  ;

end-module
