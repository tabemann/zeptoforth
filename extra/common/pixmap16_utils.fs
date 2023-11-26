\ Copyright (c) 2022-2023 Travis Bemann
\ Copyright (c) 2023 Mark Lacas
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

begin-module pixmap16-utils

  oo import
  pixmap16 import

  begin-module pixmap16-utils-internal

    \ Draw a line on a pixmap16 with slopes between 1 and -1
    : draw-pixel-line-low { color x0 y0 x1 y1 dst -- }
      x1 x0 - y1 y0 - 1 y0 { dx dy yi y }
      dy 0< if
        -1 to yi
        dy negate to dy
      then
      2 dy * dx - { d }
      x1 1+ x0 ?do
        color i y dst draw-pixel-const
        d 0> if
          yi +to y
          dy dx - 2 * +to d
        else
          2 dy * +to d
        then
      loop
    ;

    \ Draw a line on a pixmap16 with steep slopes
    : draw-pixel-line-high { color x0 y0 x1 y1 dst -- }
      x1 x0 - y1 y0 - 1 x0 { dx dy xi x }
      dx 0< if
        -1 to xi
        dx negate to dx
      then
      2 dx * dy - { d }
      y1 1+ y0 ?do
        color x i dst draw-pixel-const
        d 0> if
          xi +to x
          dx dy - 2 * +to d
        else
          2 dx * +to d
        then
      loop
    ;

    \ Draw a line on a pixmap16 with with slopes between 1 and -1
    : draw-rect-line-low { color width height x0 y0 x1 y1 dst -- }
      width 2 / height 2 / { width2/ height2/ }
      x1 x0 - y1 y0 - 1 y0 { dx dy yi y }
      dy 0< if
        -1 to yi
        dy negate to dy
      then
      2 dy * dx - { d }
      x1 1+ x0 ?do
        color i width2/ - width y height2/ - height dst draw-rect-const
        d 0> if
          yi +to y
          dy dx - 2 * +to d
        else
          2 dy * +to d
        then
      loop
    ;

    \ Draw a line on a pixmap16 with steep slopes
    : draw-rect-line-high { color width height x0 y0 x1 y1 dst -- }
      width 2 / height 2 / { width2/ height2/ }
      x1 x0 - y1 y0 - 1 x0 { dx dy xi x }
      dx 0< if
        -1 to xi
        dx negate to dx
      then
      2 dx * dy - { d }
      y1 1+ y0 ?do
        color x width2/ - width i height2/ - height dst draw-rect-const
        d 0> if
          xi +to x
          dx dy - 2 * +to d
        else
          2 dx * +to d
        then
      loop
    ;

    \ Draw a line on a pixmap16 with slopes between 1 and -1
    : draw-pixmap-line-low
      { src-x src-y width height x0 y0 x1 y1 src dst -- }
      width 2 / height 2 / { width2/ height2/ }
      x1 x0 - y1 y0 - 1 y0 { dx dy yi y }
      dy 0< if
        -1 to yi
        dy negate to dy
      then
      2 dy * dx - { d }
      x1 1+ x0 ?do
        src-x i width2/ - width src-y y height2/ - height src dst draw-rect
        d 0> if
          yi +to y
          dy dx - 2 * +to d
        else
          2 dy * +to d
        then
      loop
    ;

    \ Draw a line on a pixmap16 with steep slopes
    : draw-pixmap-line-high
      { src-x src-y width height x0 y0 x1 y1 src dst -- }
      width 2 / height 2 / { width2/ height2/ }
      x1 x0 - y1 y0 - 1 x0 { dx dy xi x }
      dx 0< if
        -1 to xi
        dx negate to dx
      then
      2 dx * dy - { d }
      y1 1+ y0 ?do
        src-x x width2/ - width src-y i height2/ - height src dst draw-rect
        d 0> if
          xi +to x
          dx dy - 2 * +to d
        else
          2 dx * +to d
        then
      loop
    ;

  end-module> import

  \ Draw a line on a pixmap16 with a pixel operation
  : draw-pixel-line { color x0 y0 x1 y1 dst -- }
    y1 y0 - abs x1 x0 - abs < if
      x0 x1 > if
        color x1 y1 x0 y0 dst draw-pixel-line-low
      else
        color x0 y0 x1 y1 dst draw-pixel-line-low
      then
    else
      y0 y1 > if
        color x1 y1 x0 y0 dst draw-pixel-line-high
      else
        color x0 y0 x1 y1 dst draw-pixel-line-high
      then
    then
  ;
  
  \ Apply a pixmap16 to another pixmap16 with an operation along a line
  : draw-rect-line { color width height x0 y0 x1 y1 dst -- }
    y1 y0 - abs x1 x0 - abs < if
      x0 x1 > if
        color width height x1 y1 x0 y0 dst draw-rect-line-low
      else
        color width height x0 y0 x1 y1 dst draw-rect-line-low
      then
    else
      y0 y1 > if
        color width height x1 y1 x0 y0 dst draw-rect-line-high
      else
        color width height x0 y0 x1 y1 dst draw-rect-line-high
      then
    then
  ;

  \ Apply a pixmap16 to another pixmap16 with an operation along a line
  : draw-pixmap-line
    { src-x src-y width height x0 y0 x1 y1 src dst -- }
    y1 y0 - abs x1 x0 - abs < if
      x0 x1 > if
        src-x src-y width height x1 y1 x0 y0 src dst draw-pixmap-line-low
      else
        src-x src-y width height x0 y0 x1 y1 src dst draw-pixmap-line-low
      then
    else
      y0 y1 > if
        src-x src-y width height x1 y1 x0 y0 src dst draw-pixmap-line-high
      else
        src-x src-y width height x0 y0 x1 y1 src dst draw-pixmap-line-high
      then
    then
  ;

  \ Draw an empty circle on a pixmap16 with a pixel operation
  : draw-pixel-circle { color x y radius dst -- }
    1 radius - 0 radius 2* negate 0 radius { f ddx ddy dx dy }
    color x y radius + dst draw-pixel-const
    color x y radius - dst draw-pixel-const
    color x radius + y dst draw-pixel-const
    color x radius - y dst draw-pixel-const
    begin dx dy < while
      f 0>= if
        dy 1- to dy
        ddy 2 + dup to ddy
        f + to f
      then
      dx 1+ to dx
      ddx 2 + dup to ddx
      f 1+ + to f
      color x dx + y dy + dst draw-pixel-const
      color x dx - y dy + dst draw-pixel-const
      color x dx + y dy - dst draw-pixel-const
      color x dx - y dy - dst draw-pixel-const
      color x dy + y dx + dst draw-pixel-const
      color x dy - y dx + dst draw-pixel-const
      color x dy + y dx - dst draw-pixel-const
      color x dy - y dx - dst draw-pixel-const
    repeat
  ;

  \ Draw an empty circle on a pixmap16 with a constant rectangle operation
  : draw-rect-circle { color width height x y radius dst -- }
    width 2 / height 2 / { width2/ height2/ }
    1 radius - 0 radius 2* negate 0 radius { f ddx ddy dx dy }
    color x width2/ - width y radius + height2/ - height dst draw-rect-const
    color x width2/ - width y radius - height2/ - height dst draw-rect-const
    color x radius + width2/ - width y height2/ - height dst draw-rect-const
    color x radius - width2/ - width y height2/ - height dst draw-rect-const
    begin dx dy < while
      f 0>= if
        dy 1- to dy
        ddy 2 + dup to ddy
        f + to f
      then
      dx 1+ to dx
      ddx 2 + dup to ddx
      f 1+ + to f
      color x dx + width2/ - width y dy + height2/ - height
      dst draw-rect-const
      color x dx - width2/ - width y dy + height2/ - height
      dst draw-rect-const
      color x dx + width2/ - width y dy - height2/ - height
      dst draw-rect-const
      color x dx - width2/ - width y dy - height2/ - height
      dst draw-rect-const
      color x dy + width2/ - width y dx + height2/ - height
      dst draw-rect-const
      color x dy - width2/ - width y dx + height2/ - height
      dst draw-rect-const
      color x dy + width2/ - width y dx - height2/ - height
      dst draw-rect-const
      color x dy - width2/ - width y dx - height2/ - height
      dst draw-rect-const
    repeat
  ;
  
  \ Draw an empty circle on a pixmap16 with another pixmap16 as a brush
  : draw-pixmap-circle { src-x src-y width height x y radius src dst -- }
    width 2 / height 2 / { width2/ height2/ }
    1 radius - 0 radius 2* negate 0 radius { f ddx ddy dx dy }
    src-x x width2/ - width src-y y radius + height2/ - height
    src dst draw-rect
    src-x x width2/ - width src-y y radius - height2/ - height
    src dst draw-rect
    src-x x radius + width2/ - width src-y y height2/ - height
    src dst draw-rect
    src-x x radius - width2/ - width src-y y height2/ - height
    src dst draw-rect
    begin dx dy < while
      f 0>= if
        dy 1- to dy
        ddy 2 + dup to ddy
        f + to f
      then
      dx 1+ to dx
      ddx 2 + dup to ddx
      f 1+ + to f
      src-x x dx + width2/ - width src-y y dy + height2/ - height
      src dst draw-rect
      src-x x dx - width2/ - width src-y y dy + height2/ - height
      src dst draw-rect
      src-x x dx + width2/ - width src-y y dy - height2/ - height
      src dst draw-rect
      src-x x dx - width2/ - width src-y y dy - height2/ - height
      src dst draw-rect
      src-x x dy + width2/ - width src-y y dx + height2/ - height
      src dst draw-rect
      src-x x dy - width2/ - width src-y y dx + height2/ - height
      src dst draw-rect
      src-x x dy + width2/ - width src-y y dx - height2/ - height
      src dst draw-rect
      src-x x dy - width2/ - width src-y y dx - height2/ - height
      src dst draw-rect
    repeat
  ;

  \ Draw an filled circle on a pixmap16 with a rectangle operation
  : draw-filled-circle { color x y radius dst -- }
    1 radius - 0 radius 2* negate 0 radius { f ddx ddy dx dy }
    color x 1 y radius + 1 dst draw-rect-const
    color x 1 y radius - 1 dst draw-rect-const
    color x radius - radius 2* y 1 dst draw-rect-const
    begin dx dy < while
      f 0>= if
        dy 1- to dy
        ddy 2 + dup to ddy
        f + to f
      then
      dx 1+ to dx
      ddx 2 + dup to ddx
      f 1+ + to f
      color x dx - dx 2 * y dy + 1 dst draw-rect-const
      color x dx - dx 2 * y dy - 1 dst draw-rect-const
      color x dy - dy 2 * y dx + 1 dst draw-rect-const
      color x dy - dy 2 * y dx - 1 dst draw-rect-const
    repeat
  ;

end-module
