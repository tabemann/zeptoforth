\ Copyright (c) 2024 Travis Bemann
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

begin-module raytracing-fun

  oo import
  bitmap import
  bitmap-utils import
  pixmap16 import
  pixmap16-utils import
  st7735s import
  fixed32 import

  \ Screen dimensions
  160 constant screen-width
  80 constant screen-height

  \ Camera position
  0;0 value camera-x
  -0;1 value camera-y
  3;0 value camera-z

  \ Cycle Z offset factor
  2;0 value cycle-z-offset-factor

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
  screen-width screen-height pixmap16-buf-size buffer: my-buffer

  \ Display
  <st7735s> class-size buffer: my-display

  \ Inited?
  false value inited

  \ Initialize the test:
  : init-test ( -- )
    lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
    my-buffer screen-width screen-height my-device <st7735s> my-display
    init-object
    my-display clear-pixmap
    my-display update-display
  ;

  \ The ray variables; note that this assumes that each ray will only go one
  \ direction.
  variable ray-x
  variable ray-y
  variable ray-z
  variable ray-u
  variable ray-v
  variable ray-w
  variable ray-q
  variable ray-e
  variable ray-f
  variable ray-g
  variable ray-p
  variable ray-d
  variable ray-t

  \ Generate a color
  : color { x y z u v w -- color }
    v 0;0 >= if
      0 v 1;0 swap - 0;0 max 1;0 min 255;0 f32* f32>s dup rgb16
    else
      y 2;0 + v f32/ { p }
      x u p f32* - f32>s z w p f32* - f32>s + 1 and if
        0;8 v negate f32* 0;2 + 0;0 max 1;0 min 255;0 f32* f32>s 0 0 rgb16
      else
        0 0;8 v negate f32* 0;2 + 0;0 max 1;0 min 255;0 f32* f32>s 0 rgb16
      then
    then
  ;
  
  \ Create a structure for storing all these variables
  begin-structure ray-data-size
  end-structure

  true value first
  
  \ Fire a ray
  : ray ( -- )
    ray-x @ ray-q @ - ray-e !
    ray-y @ ray-q @ - ray-f !
    ray-z @ ray-g !
    ray-u @ ray-e @ f32*
    ray-v @ ray-f @ f32* +
    ray-w @ ray-g @ f32* - ray-p !
    ray-p @ dup f32*
    ray-e @ dup f32* -
    ray-f @ dup f32* -
    ray-g @ dup f32* - 1;0 + ray-d !
    ray-d @ 0;0 <= if
      ray-x @ ray-y @ ray-z @
      ray-u @ ray-v @ ray-w @ color
    else
      ray-p @ negate ray-d @ f32sqrt - ray-t !
      ray-t @ 0;0 <= if
        ray-x @ ray-y @ ray-z @
        ray-u @ ray-v @ ray-w @ color
      else
        ray-t @ ray-u @ f32* ray-x +!
        ray-t @ ray-v @ f32* ray-y +!
        ray-t @ ray-w @ f32* negate ray-z +!
        ray-x @ ray-q @ - ray-e !
        ray-y @ ray-q @ - ray-f !
        ray-z @ ray-g !
        ray-u @ ray-e @ f32*
        ray-v @ ray-f @ f32* +
        ray-w @ ray-g @ f32* - 2;0 f32* ray-p !
        ray-p @ ray-e @ f32* negate ray-u +!
        ray-p @ ray-f @ f32* negate ray-v +!
        ray-p @ ray-g @ f32* ray-w +!
        ray-q @ negate ray-q !
        recurse
      then
    then
  ;
  
  \ Raytrace!
  : raytrace ( -- )
    inited not if
      init-test
      true to inited
    then
    my-display clear-pixmap
    screen-width s>f32 2;0 f32/ { screen-width2/ }
    screen-height s>f32 2;0 f32/ { screen-height2/ }
    0;0 { cycle }
    begin key? not while
      cycle f32sin cycle-z-offset-factor f32* { z-offset }
      screen-height 0 ?do
        screen-width 0 ?do
          i s>f32 screen-width2/ - screen-width2/ f32/ { u }
          j s>f32 screen-height2/ - screen-width2/ f32/ { v }
          1;0 u u f32* v v f32* + 1;0 + f32sqrt f32/ { w }
          u w f32* to u
          v w f32* to v
          u 0;0 > if 1;0 else u 0;0 < if -1;0 else 1;0 then then { q }
          camera-x ray-x !
          camera-y ray-y !
          camera-z z-offset + ray-z !
          u ray-u !
          v ray-v !
          w ray-w !
          q ray-q !
          ray
          i screen-height j - my-display draw-pixel-const
        loop
      loop
      my-display update-display
      cycle [ f32pi 16;0 f32/ ] literal +
      [ f32pi 2;0 f32* ] literal f32mod to cycle
    repeat
  ;
  
end-module