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
  pixmap8 import
  pixmap8-utils import
  st7789v-8 import

  begin-module turtle-internal
    
    \ Screen dimensions
    320 constant screen-width
    240 constant screen-height
    
    \ Turtle dimensions
    8 constant turtle-width
    8 constant turtle-height

    \ Save dimensions
    16 constant save-width
    16 constant save-height

    \ Pio device
    pio::PIO0 constant my-pio
    
    \ State machine
    0 constant my-sm
    
    \ Pins
    14 constant lcd-d0
    13 constant lcd-rd-sck
    12 constant lcd-wr-sck
    11 constant lcd-dc
    10 constant lcd-cs
    2 constant lcd-bl
    
    \ Buffer
    screen-width screen-height pixmap8-buf-size buffer: my-buffer
    
    \ Save buffer
    save-width save-height pixmap8-buf-size buffer: my-save-buffer
    
    \ Display
    <st7789v-8> class-size buffer: my-display
    
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
      lcd-d0 lcd-wr-sck lcd-rd-sck lcd-dc lcd-cs lcd-bl
      my-buffer false screen-width screen-height my-sm my-pio
      <st7789v-8> my-display init-object
      my-save-buffer save-width save-height <pixmap8> my-save init-object
      my-display clear-pixmap
      my-display update-display
      my-save clear-pixmap
    ;

    \ Convert an angle
    : convert-angle ( angle -- D: real-angle )
      s>f 180,0 f/ pi f*
      [ pi 2,0 f/ swap ] literal literal d+
    ;

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
        1 turtle-color left-x left-y right-x right-y draw-line
        1 turtle-color left-x left-y tip-x tip-y draw-line
        1 turtle-color right-x right-y tip-x tip-y draw-line
      then
    ;

  end-module> import

  \ Set the pen color
  : setpencolor ( r g b -- ) rgb8 to pen-color ;

  \ Set the turtle color
  : setturtlecolor ( r g b -- ) rgb8 to turtle-color ;

  \ Go forward
  : forward { pixels -- }
    inited? not if true to inited? init-turtle then
    erase-turtle
    turtle-angle convert-angle cos pixels s>f f* turtle-x d+ { D: dest-x }
    turtle-angle convert-angle sin pixels s>f f* turtle-y d+ { D: dest-y }
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
