\ Copyright (c) 2023-2024 Travis Bemann
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
  pixmap16 import
  pixmap16-utils import
  st7735s import

  \ Displayed X size
  160 constant width

  \ Displayed Y size
  80 constant height

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
  width height pixmap16-buf-size buffer: my-buffer

  \ Display
  <st7735s> class-size buffer: my-display

  \ X scale offset
  -2,00 2constant x-offset

  \ Y scale offset
  -1,12 2constant y-offset

  \ X scale multiplier
  0,47 -2,00 d- 2constant x-multiplier

  \ Y scale multiplier
  1,12 -1,12 d- 2constant y-multiplier

  \ The maximum number of iterations
  16 constant max-iteration

  \ Color table
  max-iteration 1+ cell align aligned-buffer: colors

  \ Initialize the colors
  : init-colors ( -- )
    max-iteration 1+ 0 ?do
      i s>f max-iteration 1- s>f f/ 1,0 d+ ln 2,0 ln f/
      255,0 f* f>s colors i + c!
    loop
  ;
  
  \ Initialize the test
  : init-test ( -- )
    lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
    my-buffer width height my-device <st7735s> my-display init-object
    my-display clear-pixmap
    my-display update-display
    init-colors
  ;

  \ Are we initialized
  false value inited?

  \ Convert iteration to color
  : iteration>color { iteration -- color }
    iteration max-iteration < if
      0 colors iteration + c@ 0 rgb16
    else
      0 0 255 rgb16
    then
  ;
  
  \ Mandelbrot test
  : draw { D: xa D: xb D: ya D: yb -- }
    inited? not if init-test true to inited? then
    my-display clear-pixmap
    my-display update-display
    xb xa d- { D: x-mult }
    yb ya d- { D: y-mult }
    height 0 ?do
      width 0 ?do
        i s>f width s>f f/ x-mult f* xa d+ { D: x0 }
        j s>f height s>f f/ y-mult f* ya d+ { D: y0 }
        0,0 0,0 { D: x D: y }
        0 { iteration }
        begin
          x 2dup f* y 2dup f* d+ 4,0 d<= iteration max-iteration < and
        while
          x 2dup f* y 2dup f* d- x0 d+ { D: xtemp }
          x y f* 2,0 f* y0 d+ to y
          xtemp to x
          1 +to iteration
        repeat
        iteration iteration>color i height j - my-display draw-pixel-const
      loop
      my-display update-display
    loop
  ;

  \ Draw a mandelbot set
  : test ( -- ) -2,00 0,47 -1,12 1,12 draw ;
  
end-module
