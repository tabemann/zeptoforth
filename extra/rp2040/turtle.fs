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

  \ Inited?
  false value inited?

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
  : draw-line { color x0 y0 x1 y1 -- }
    color x0 screen-height 1- y0 - x1 screen-height 1- y1 - my-display
    draw-pixel-line
  ;

  \ Draw the turtle
  : draw-turtle ( -- )
    turtle-x f>s save-width 2 / -
    screen-height turtle-y f>s - save-height 2 / -
    0 0 save-width save-height my-display my-save draw-rect
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
    show-turtle? if
      turtle-color left-x left-y right-x right-y draw-line
      turtle-color left-x left-y tip-x tip-y draw-line
      turtle-color right-x right-y tip-x tip-y draw-line
    then
  ;

  \ Set the pen color
  : setpencolor ( r g b -- ) rgb16 to pen-color ;

  \ Set the turtle color
  : setturtlecolor ( r g b -- ) rgb16 to turtle-color ;

  \ Go forward
  : forward { pixels -- } \ [char] a emit .s
    inited? not if true to inited? init-turtle then \ [char] b emit .s
    erase-turtle \ [char] c emit .s
    turtle-angle cos pixels s>f f* turtle-x d+ { D: dest-x } \ [char] d emit .s
    turtle-angle sin pixels s>f f* turtle-y d+ { D: dest-y } \ [char] e emit .s
    pen-down? if \ [char] f emit .s
      pen-color turtle-x f>s turtle-y f>s dest-x f>s dest-y f>s draw-line \ [char] g emit .s
    then \ [char] h emit .s
    dest-x to turtle-x \ [char] i emit .s
    dest-y to turtle-y \ [char] j emit .s
    draw-turtle \ [char] k emit .s
    my-display update-display \ [char] l emit .s
  ;

  \ Go backward
  : backward ( pixels -- ) -1 * forward ;

  \ Go left; angle is in degrees
  : left { angle -- } \ [char] m emit .s
    inited? not if true to inited? init-turtle then \ [char] n emit .s
    erase-turtle \ [char] o emit .s
    angle s>f [ 2,0 pi f* 360,0 f/ swap ] literal literal f* +to turtle-angle \ [char] p emit .s
    draw-turtle \ [char] q emit .s
    my-display update-display \ [char] r emit .s
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
    my-display update-display
  ;

  \ Set the angle, from the y axis, of the turtle
  : setheading { angle -- }
    inited? not if true to inited? init-turtle then
    erase-turtle
    angle s>f [ 2,0 pi f* 360,0 f/ swap ] literal literal f* to turtle-angle
    draw-turtle
    my-display update-display
  ;

  \ Hide the turtle
  : hideturtle ( -- )
    inited? not if true to inited? init-turtle then
    erase-turtle
    false to show-turtle?
    draw-turtle
    my-display update-display
  ;

  \ Show the turtle
  : showturtle ( -- )
    inited? not if true to inited? init-turtle then
    erase-turtle
    true to show-turtle?
    draw-turtle
    my-display update-display
  ;

  \ Set pen size
  : setpensize { pixels -- } ;

  \ Clear the display
  : clear ( -- )
    inited? not if true to inited? init-turtle then
    erase-turtle
    my-display clear-pixmap
    my-save clear-pixmap
    draw-turtle
    my-display update-display
  ;

end-module
