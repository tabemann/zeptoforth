\ Copyright (c) 2024-2025 Travis Bemann
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
    0,0 2value turtle-x
    0,0 2value turtle-y

    \ The turtle angle
    0 value turtle-angle
    
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

    \ Convert an angle
    : convert-angle ( angle -- D: real-angle )
      s>f 180,0 f/ pi f*
    ;
    
    \ Degree sine
    : dsin { angle -- D: sin }
      begin angle 0< while 360 +to angle repeat
      angle 360 mod 90 + to angle
      angle 0= if 0,0 exit then
      angle 45 = if
        [ pi 0,25 f* sin swap ] literal literal exit
      then
      angle 90 = if 1,0 exit then
      angle 135 = if
        [ pi 0,75 f* sin swap ] literal literal exit
      then
      angle 180 = if 0,0 exit then
      angle 225 = if
        [ pi 1,25 f* sin swap ] literal literal exit
      then
      angle 270 = if -1,0 exit then
      angle 315 = if
        [ pi 1,75 f* sin swap ] literal literal exit
      then
      angle convert-angle sin
    ;
    
    \ Degree cosine
    : dcos ( angle -- D: cos ) 90 + dsin ;

    \ Convert a coordinate
    : convert-coord ( x y -- x' y' )
      screen-height 2 / swap - swap screen-width 2 / + swap
    ;

    \ Erase the turtle
    : erase-turtle ( -- )
      turtle-x f>s turtle-y f>s convert-coord { x y }
      0 0 x save-width 2 / - y save-height 2 / -
      save-width save-height my-save my-display draw-rect
    ;

    \ Draw a line
    : draw-line { weight color x0 y0 x1 y1 -- }
      x0 y0 convert-coord to y0 to x0
      x1 y1 convert-coord to y1 to x1
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
      turtle-x f>s turtle-y f>s convert-coord { x y }
      x save-width 2 / - y save-height 2 / -
      0 0 save-width save-height my-display my-save draw-rect
      show-turtle? if
        turtle-angle dcos { D: angle-cos }
        turtle-angle dsin { D: angle-sin }
        angle-cos turtle-width 2 / s>f f* turtle-x d+ f>s { tip-x }
        angle-sin turtle-height 2 / s>f f* turtle-y d+ f>s { tip-y }
        angle-cos turtle-width -2 / s>f f* turtle-x d+ { D: base-x }
        angle-sin turtle-height -2 / s>f f* turtle-y d+ { D: base-y }
        turtle-angle 90 - dcos { D: right-angle-cos }
        turtle-angle 90 - dsin { D: right-angle-sin }
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
    turtle-angle dcos pixels s>f f* turtle-x d+ { D: dest-x }
    turtle-angle dsin pixels s>f f* turtle-y d+ { D: dest-y }
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
    angle +to turtle-angle
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Go right: angle is in degrees
  : right ( angle -- ) -1 * left ;
  
  \ Set the pen up
  : penup ( -- ) false to pen-down? ;

  \ Set the pen down
  : pendown ( -- ) true to pen-down? ;

  \ Get whether the pen is down
  : getpendown ( -- pendown? ) pen-down? ;
  
  \ Set the x/y coordinate of the turtle
  : setxy { x y -- }
    inited? not if true to inited? init-turtle then
    erase-turtle
    pen-down? if
      pen-size pen-color turtle-x f>s turtle-y f>s x y draw-line
    then
    x s>f to turtle-x
    y s>f to turtle-y
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Get the x/y coordinate of the turtle - note that this is only accurate to
  \ the pixel, so calling GETXY and then using the same values for SETXY may
  \ lose precision.
  : getxy ( -- x y )
    turtle-x f>s turtle-y f>s
  ;
  
  \ Set the angle, from pointing up, of the turtle
  : setheading { angle -- }
    inited? not if true to inited? init-turtle then
    erase-turtle
    angle to turtle-angle
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Get the heading of the turtle, from pointing up
  : getheading ( -- angle )
    turtle-angle
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

  \ Get whether the turtle is shown
  : getshowturtle ( --  showturtle? ) show-turtle? ;

  \ Turn off updating the display
  : updateoff ( -- ) false to update-display? ;

  \ Turn on updating the display
  : updateon ( -- )
    true to update-display?
    my-display update-display
  ;

  \ Get whether updates are on
  : getupdateon ( -- updateon? )  update-display? ;

  \ Set pen size
  : setpensize ( pixels -- ) to pen-size ;

  \ Get whether updates are on
  : getpensize ( -- pixels ) pen-size ;

  \ Clear the display
  : clear ( -- )
    inited? not if true to inited? init-turtle then
    erase-turtle
    my-display clear-pixmap
    my-save clear-pixmap
    draw-turtle
    update-display? if my-display update-display then
  ;

  \ Move the turtle to home position
  : home ( -- )
    inited? not if true to inited? init-turtle then
    getupdateon { saved-updateon }
    getpendown { saved-pendown }
    updateoff
    penup
    0 0 setxy
    0 setheading
    saved-pendown if pendown then
    saved-updateon if updateon then
  ;
  
end-module
