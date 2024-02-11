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

begin-module julia

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
  
  \ The maximum number of iterations
  32 constant max-iteration

  \ Color table
  max-iteration 1+ cell align aligned-buffer: colors

  \ Initialize the colors
  : init-colors ( -- )
    max-iteration 1+ 0 ?do
      i s>f max-iteration 1- s>f f/ 1,0 d+ ln 2,0 ln f/
      127,0 f* f>s colors i + c!
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
      0 0 127 rgb16
    then
  ;
  
  \ Julia test
  : draw { D: cx D: cy D: r -- }
    inited? not if init-test true to inited? then
    my-display clear-pixmap
    my-display update-display
    r 2,0 f* { D: 2r }
    r r f* { D: r2** }
    height 0 ?do
      width 0 ?do
        i s>f width s>f f/ 0,5 d- 2r f* { D: zx }
        j s>f height s>f f/ 0,5 d- 2r f* { D: zy }
        0 { iteration }
        begin
          zx 2dup f* zy 2dup f* d+ r2** d< iteration max-iteration < and
        while
          zx 2dup f* zy 2dup f* d- { D: xtemp }
          zx zy f* 2,0 f* cy d+ to zy
          xtemp cx d+ to zx
          1 +to iteration
        repeat
        iteration iteration>color i height j - my-display draw-pixel-const
      loop
      my-display update-display
    loop
  ;

  \ Draw a julia set
  : test ( -- ) -0,4 0,6 1,0 julia::draw ;
  
end-module
