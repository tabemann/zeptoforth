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
  picocalc-term import
  pixmap8 import
  pixmap8-utils import
  defined? st7365p-8-common [if]
    st7365p-8-common import
  [else]
    defined? st7789v-8-common [if]
      st7789v-8-common import
    [then]
  [then]

  begin-module turtle-internal
    
    \ Turtle dimensions
    8 constant turtle-width
    8 constant turtle-height

    \ Save dimensions
    16 constant save-width
    16 constant save-height

    \ Save buffer
    save-width save-height pixmap8-buf-size buffer: my-save-buffer
    
    \ Save pixmap
    <pixmap8> class-size buffer: my-save

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
    255 255 255 rgb8 value pen-color

    \ The turtle color
    0 255 0 rgb8 value turtle-color

    \ Pen size
    1 value pen-size
    
    \ Inited?
    false value inited?

    \ Update display?
    true value update-display?

    \ Initialize the turtle
    : init-turtle ( -- )
      my-save-buffer save-width save-height <pixmap8> my-save init-object
      my-save clear-pixmap
    ;
    initializer init-turtle

    \ Convert an angle
    : convert-angle ( angle -- D: real-angle )
      s>f 180,0 f/ pi f*
      [ pi 2,0 f/ swap ] literal literal d+
    ;

    \ Convert a coordinate
    : convert-coord ( x y -- x' y' )
      term-pixels-dim@ { screen-width screen-height }
      screen-height 2 / swap - swap screen-width 2 / + swap
    ;

    \ Erase the turtle
    : erase-turtle { display -- }
      turtle-x f>s turtle-y f>s convert-coord { x y }
      0 0 x save-width 2 / - y save-height 2 / -
      save-width save-height my-save display draw-rect
    ;

    \ Draw a line
    : draw-line { weight color x0 y0 x1 y1 display -- }
      x0 y0 convert-coord to y0 to x0
      x1 y1 convert-coord to y1 to x1
      weight 1 = if
        color x0 y0 x1 y1 display draw-pixel-line
      else
        weight 1 > if
          1 +to weight
          display weight color x0 y0 x1 y1
          weight dup dup bitmap::bitmap-buf-size [:
            swap dup bitmap::<bitmap> [:
              { display weight color x0 y0 x1 y1 brush }
              brush bitmap::clear-bitmap
              $FF weight 2 / dup dup bitmap::op-set brush
              bitmap-utils::draw-filled-circle
              color 0 dup weight dup x0 y0 x1 y1 brush display
              draw-bitmap-line
            ;] with-object
          ;] with-aligned-allot
        then
      then
    ;

    \ Draw the turtle
    : draw-turtle { display -- }
      turtle-x f>s turtle-y f>s convert-coord { x y }
      x save-width 2 / - y save-height 2 / -
      0 0 save-width save-height display my-save draw-rect
      show-turtle? if
        turtle-angle convert-angle { D: angle }
        angle cos { D: angle-cos }
        angle sin { D: angle-sin }
        angle-cos turtle-width 2 / s>f f* turtle-x d+ f>s { tip-x }
        angle-sin turtle-height 2 / s>f f* turtle-y d+ f>s { tip-y }
        angle-cos turtle-width -2 / s>f f* turtle-x d+ { D: base-x }
        angle-sin turtle-height -2 / s>f f* turtle-y d+ { D: base-y }
        angle [ pi 0,5 f* swap ] literal literal d- { D: right-angle }
        right-angle cos { D: right-angle-cos }
        right-angle sin { D: right-angle-sin }
        right-angle-cos turtle-width 2 / s>f f* base-x d+ f>s { right-x }
        right-angle-sin turtle-height 2 / s>f f* base-y d+ f>s { right-y }
        right-angle-cos turtle-width -2 / s>f f* base-x d+ f>s { left-x }
        right-angle-sin turtle-height -2 / s>f f* base-y d+ f>s { left-y }
        1 turtle-color left-x left-y right-x right-y display draw-line
        1 turtle-color left-x left-y tip-x tip-y display draw-line
        1 turtle-color right-x right-y tip-x tip-y display draw-line
      then
    ;

  end-module> import

  \ Set the pen color
  : setpencolor ( r g b -- ) rgb8 to pen-color ;

  \ Set the turtle color
  : setturtlecolor ( r g b -- ) rgb8 to turtle-color ;

  \ Go forward
  : forward ( pixels -- )
    [: { pixels display }
      display erase-turtle
      turtle-angle convert-angle cos pixels s>f f* turtle-x d+ { D: dest-x }
      turtle-angle convert-angle sin pixels s>f f* turtle-y d+ { D: dest-y }
      pen-down? if
        pen-size pen-color turtle-x f>s turtle-y f>s dest-x f>s dest-y f>s
        display draw-line
      then
      dest-x to turtle-x
      dest-y to turtle-y
      display draw-turtle
      update-display? if display update-display then
    ;] with-term-display
  ;

  \ Go backward
  : backward ( pixels -- ) -1 * forward ;

  \ Go left; angle is in degrees
  : left ( angle -- )
    [: { angle display }
      display erase-turtle
      angle +to turtle-angle
      display draw-turtle
      update-display? if display update-display then
    ;] with-term-display
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
  : setxy ( x y -- )
    [: { x y display }
      display erase-turtle
      pen-down? if
        pen-size pen-color turtle-x f>s turtle-y f>s x y display draw-line
      then
      x s>f to turtle-x
      y s>f to turtle-y
      display draw-turtle
      update-display? if display update-display then
    ;] with-term-display
  ;

  \ Get the x/y coordinate of the turtle - note that this is only accurate to
  \ the pixel, so calling GETXY and then using the same values for SETXY may
  \ lose precision.
  : getxy ( -- x y )
    turtle-x f>s turtle-y f>s
  ;
  
  \ Set the angle, from pointing up, of the turtle
  : setheading ( angle -- )
    [: { angle display }
      display erase-turtle
      angle to turtle-angle
      display draw-turtle
      update-display? if display update-display then
    ;] with-term-display
  ;

  \ Get the heading of the turtle, from pointing up
  : getheading ( -- angle )
    turtle-angle
  ;

  \ Hide the turtle
  : hideturtle ( -- )
    [: { display }
      display erase-turtle
      false to show-turtle?
      display draw-turtle
      update-display? if display update-display then
    ;] with-term-display
  ;

  \ Show the turtle
  : showturtle ( -- )
    [: { display }
      display erase-turtle
      true to show-turtle?
      display draw-turtle
      update-display? if display update-display then
    ;] with-term-display
  ;

  \ Get whether the turtle is shown
  : getshowturtle ( --  showturtle? ) show-turtle? ;

  \ Turn off updating the display
  : updateoff ( -- ) false to update-display? ;

  \ Turn on updating the display
  : updateon ( -- )
    [: { display }
      true to update-display?
      display update-display
    ;] with-term-display
  ;

  \ Get whether updates are on
  : getupdateon ( -- updateon? )  update-display? ;

  \ Set pen size
  : setpensize ( pixels -- ) to pen-size ;

  \ Get whether updates are on
  : getpensize ( -- pixels ) pen-size ;

  \ Clear the display
  : clear ( -- )
    [: { display }
      display erase-turtle
      display clear-pixmap
      my-save clear-pixmap
      display draw-turtle
      update-display? if display update-display then
    ;] with-term-display
  ;

  \ Move the turtle to home position
  : home ( -- )
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
