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

begin-module turtle

  oo import
  pixmap16 import
  pixmap16-utils import
  st7735s import

  begin-module turtle-internal

    \ Screen dimensions
    160 constant screen-width
    80 constant screen-height

    \ Turtle dimensions
    8 constant turtle-width
    8 constant turtle-height

    \ Save dimensions
    16 constant save-width
    16 constant save-height

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
    screen-width screen-height pixmap16-buf-size aligned-buffer: my-buffer

    \ Save buffer
    save-width save-height pixmap16-buf-size aligned-buffer: my-save-buffer
    
    \ Display
    <st7735s> class-size buffer: my-display

    \ Save pixmap
    <pixmap16> class-size buffer: my-save

    \ Is the pen down
    true value pen-down?

    \ Is the turtle visible
    true value show-turtle?

    \ The turtle position
    screen-width s>f 2,0 f/ 2value turtle-x
    screen-height s>f 2,0 f/ 2value turtle-y

    \ The turtle angle
    pi 2,0 f/ 2value turtle-angle
    
    \ The pen color
    255 255 255 rgb16 value pen-color

    \ The turtle color
    0 255 0 rgb16 value turtle-color

    \ Pen size
    1 value pen-size
    
    \ Inited?
    false value inited?

    \ Update display?
    true value update-display?

    \ Initialize the turtle
    : init-turtle ( -- )
      lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
      my-buffer screen-width screen-height my-device <st7735s> my-display
      init-object
      my-save-buffer save-width save-height <pixmap16> my-save init-object
      my-display clear-pixmap
      my-display update-display
      my-save clear-pixmap
    ;

    \ Erase the turtle
    : erase-turtle ( -- )
      0 0 turtle-x f>s save-width 2 / -
      screen-height turtle-y f>s - save-height 2 / -
      save-width save-height my-save my-display draw-rect
    ;

    \ Draw a line
    : draw-line { weight color x0 y0 x1 y1 -- }
      screen-height 1- y0 - to y0
      screen-height 1- y1 - to y1
      weight 1 = if
        color x0 y0 x1 y1 my-display draw-pixel-line
      else
        weight 1 > if
          1 +to weight
          weight color x0 y0 x1 y1 weight dup dup bitmap::bitmap-buf-size [:
            swap dup bitmap::<bitmap> [: { weight color x0 y0 x1 y1 brush }
              brush bitmap::clear-bitmap
              $FF weight 2 / dup dup bitmap::op-set brush
              bitmap-utils::draw-filled-circle
              color 0 dup weight dup x0 y0 x1 y1 brush my-display
              draw-bitmap-line
            ;] with-object
          ;] with-aligned-allot
        then
      then
    ;

    \ Draw the turtle
    : draw-turtle ( -- )
      turtle-x f>s save-width 2 / -
      screen-height turtle-y f>s - save-height 2 / -
      0 0 save-width save-height my-display my-save draw-rect
      show-turtle? if
        turtle-angle cos { D: turtle-angle-cos }
        turtle-angle sin { D: turtle-angle-sin }
        turtle-angle-cos turtle-width 2 / s>f f* turtle-x d+ f>s { tip-x }
        turtle-angle-sin turtle-height 2 / s>f f* turtle-y d+ f>s { tip-y }
        turtle-angle-cos turtle-width -2 / s>f f* turtle-x d+ { D: base-x }
        turtle-angle-sin turtle-height -2 / s>f f* turtle-y d+ { D: base-y }
        turtle-angle [ pi 0,5 f* swap ] literal literal d- { D: right-angle }
        right-angle cos { D: right-angle-cos }
        right-angle sin { D: right-angle-sin }
        right-angle-cos turtle-width 2 / s>f f* base-x d+ f>s { right-x }
        right-angle-sin turtle-height 2 / s>f f* base-y d+ f>s { right-y }
        right-angle-cos turtle-width -2 / s>f f* base-x d+ f>s { left-x }
        right-angle-sin turtle-height -2 / s>f f* base-y d+ f>s { left-y }
        1 turtle-color left-x left-y right-x right-y draw-line
        1 turtle-color left-x left-y tip-x tip-y draw-line
        1 turtle-color right-x right-y tip-x tip-y draw-line
      then
    ;

  end-module> import

  \ Set the pen color
  : setpencolor ( r g b -- ) rgb16 to pen-color ;

  \ Set the turtle color
  : setturtlecolor ( r g b -- ) rgb16 to turtle-color ;

  \ Go forward
  : forward { pixels -- }
    inited? not if true to inited? init-turtle then
    erase-turtle
    turtle-angle cos pixels s>f f* turtle-x d+ { D: dest-x }
    turtle-angle sin pixels s>f f* turtle-y d+ { D: dest-y }
    pen-down? if
      pen-size pen-color turtle-x f>s turtle-y f>s dest-x f>s dest-y f>s
      draw-line
    then
    dest-x to turtle-x
    dest-y to turtle-y
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Go backward
  : backward ( pixels -- ) -1 * forward ;

  \ Go left; angle is in degrees
  : left { angle -- }
    inited? not if true to inited? init-turtle then
    erase-turtle
    angle s>f [ 2,0 pi f* 360,0 f/ swap ] literal literal f* +to turtle-angle
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Go right: angle is in degrees
  : right ( angle -- ) -1 * left ;
  
  \ Set the pen up
  : penup ( -- ) false to pen-down? ;

  \ Set the pen down
  : pendown ( -- ) true to pen-down? ;

  \ Set the x/y coordinate of the turtle
  : setxy { x y -- }
    inited? not if true to inited? init-turtle then
    erase-turtle
    x s>f to turtle-x
    y s>f to turtle-y
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Set the angle, from the y axis, of the turtle
  : setheading { angle -- }
    inited? not if true to inited? init-turtle then
    erase-turtle
    angle s>f [ 2,0 pi f* 360,0 f/ swap ] literal literal f* to turtle-angle
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Hide the turtle
  : hideturtle ( -- )
    inited? not if true to inited? init-turtle then
    erase-turtle
    false to show-turtle?
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Show the turtle
  : showturtle ( -- )
    inited? not if true to inited? init-turtle then
    erase-turtle
    true to show-turtle?
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Turn off updating the display
  : updateoff ( -- ) false to update-display? ;

  \ Turn on updating the display
  : updateon ( -- )
    true to update-display?
    my-display update-display
  ;

  \ Set pen size
  : setpensize ( pixels -- ) to pen-size ;

  \ Clear the display
  : clear ( -- )
    inited? not if true to inited? init-turtle then
    erase-turtle
    my-display clear-pixmap
    my-save clear-pixmap
    draw-turtle
    update-display? if my-display update-display then
  ;

end-module
