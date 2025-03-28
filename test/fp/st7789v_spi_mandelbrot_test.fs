\ Copyright (c) 2025 Travis Bemann
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

begin-module mandelbrot

  oo import
  pixmap8 import
  pixmap8-utils import
  st7789v-8-spi import
  float32 import

  \ Screen dimensions
  320 constant screen-width
  240 constant screen-height
  
  \ SPI device
  0 constant my-spi
  
  \ Pins
  18 constant lcd-sck
  19 constant lcd-din
  21 constant lcd-cs
  13 constant lcd-dc
  12 constant lcd-rst
  11 constant lcd-bl
  
  \ Buffer
  screen-width screen-height pixmap8-buf-size buffer: my-buffer
  
  \ Display
  <st7789v-8-spi> class-size buffer: my-display

  \ Initialize the display
  : init-display ( -- )
    lcd-din lcd-sck lcd-dc lcd-cs lcd-bl lcd-rst
    my-buffer false screen-width screen-height my-spi
    <st7789v-8-spi> my-display init-object
    my-display clear-pixmap
    my-display update-display
  ;
  initializer init-display
  
  : angle-color { degrees -- r g b }
    degrees 360 umod to degrees
    degrees 60 /mod { remainder quotient }
    remainder 255 * 60 / { shade }
    quotient case
      0 of 255 shade 0 endof
      1 of 255 shade - 255 0 endof
      2 of 0 255 shade endof
      3 of 0 255 shade - 255 endof
      4 of shade 0 255 endof
      5 of 255 0 255 shade - endof
    endcase
  ;
  
  80 constant max-iteration
  
  : get-color ( iteration -- color )
    negate 240 * max-iteration / 240 + angle-color rgb8
  ;  
  
  \ Draw a mandelbrot set
  : draw-mandelbrot { xa xb ya yb -- }
    xb xa v- { x-mult }
    yb ya v- { y-mult }
    screen-height 0 ?do
      screen-width 0 ?do
        i u>v screen-width u>v v/ x-mult v* xa v+ { x0 }
        screen-height j - u>v screen-height u>v v/ y-mult v* ya v+ { y0 }
        0e0 0e0 { x y }
        0 { iteration }
        begin
          x dup v* y dup v* v+ 4e0 v<= iteration max-iteration < and
        while
          x dup v* y dup v* v- x0 v+ { xtemp }
          x y v* 2e0 v* y0 v+ to y
          xtemp to x
          1 +to iteration
        repeat
        iteration get-color i j my-display draw-pixel-const
      loop
      my-display update-display
    loop
  ;
  
  : test ( -- ) -2.00e0 0.47e0 -1.12e0 1.12e0 draw-mandelbrot ;
  
end-module
