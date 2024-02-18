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
    field: ray-x
    field: ray-y
    field: ray-z
    field: ray-u
    field: ray-v
    field: ray-w
    field: ray-q
    field: ray-e
    field: ray-f
    field: ray-g
    field: ray-p
    field: ray-d
    field: ray-t
  end-structure

  true value first
  
  \ Fire a ray
  : ray ( x y z u v w q -- color )
    ray-data-size [: { data }
      data ray-q ! data ray-w ! data ray-v ! data ray-u !
      data ray-z ! data ray-y ! data ray-x !
      data ray-x @ data ray-q @ - data ray-e !
      data ray-y @ data ray-q @ - data ray-f !
      data ray-z @ data ray-g !
      data ray-u @ data ray-e @ f32*
      data ray-v @ data ray-f @ f32* +
      data ray-w @ data ray-g @ f32* - data ray-p !
      data ray-p @ dup f32*
      data ray-e @ dup f32* -
      data ray-f @ dup f32* -
      data ray-g @ dup f32* - 1;0 + data ray-d !
      data ray-d @ 0;0 <= if
        data ray-x @ data ray-y @ data ray-z @
        data ray-u @ data ray-v @ data ray-w @ color
      else
        data ray-p @ negate data ray-d @ f32sqrt - data ray-t !
        data ray-t @ 0;0 <= if
          data ray-x @ data ray-y @ data ray-z @
          data ray-u @ data ray-v @ data ray-w @ color
        else
          data ray-t @ data ray-u @ f32* data ray-x +!
          data ray-t @ data ray-v @ f32* data ray-y +!
          data ray-t @ data ray-w @ f32* negate data ray-z +!
          data ray-x @ data ray-q @ - data ray-e !
          data ray-y @ data ray-q @ - data ray-f !
          data ray-z @ data ray-g !
          data ray-u @ data ray-e @ f32*
          data ray-v @ data ray-f @ f32* +
          data ray-w @ data ray-g @ f32* - 2;0 f32* data ray-p !
          data ray-p @ data ray-e @ f32* negate data ray-u +!
          data ray-p @ data ray-f @ f32* negate data ray-v +!
          data ray-p @ data ray-g @ f32* data ray-w +!
          data ray-q @ negate data ray-q !
          data ray-x @ data ray-y @ data ray-z @
          data ray-u @ data ray-v @ data ray-w @ data ray-q @ recurse
        then
      then
    ;] with-aligned-allot
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
    screen-height 0 ?do
      screen-width 0 ?do
        i s>f32 screen-width2/ - screen-width2/ f32/ { u }
        j s>f32 screen-height2/ - screen-width2/ f32/ { v }
        1;0 u u f32* v v f32* + 1;0 + f32sqrt f32/ { w }
        u w f32* to u
        v w f32* to v
        u 0;0 > if 1;0 else u 0;0 < if -1;0 else 1;0 then then { q }
        camera-x camera-y camera-z u v w q ray
        i screen-height j - my-display draw-pixel-const
      loop
    loop
    my-display update-display
  ;
  
end-module