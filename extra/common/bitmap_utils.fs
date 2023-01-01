\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module bitmap-utils

  oo import
  bitmap import

  begin-module bitmap-utils-internal

    \ Draw a line on a bitmap with a pixel operation with slopes between 1 and -1
    : draw-pixel-line-low { const x0 y0 x1 y1 op dst -- }
      x1 x0 - y1 y0 - 1 y0 { dx dy yi y }
      dy 0< if
        -1 to yi
        dy negate to dy
      then
      2 dy * dx - { d }
      x1 1+ x0 ?do
        const i y dst op execute
        d 0> if
          yi +to y
          dy dx - 2 * +to d
        else
          2 dy * +to d
        then
      loop
    ;

    \ Draw a line on a bitmap with a pixel operation with steep slopes
    : draw-pixel-line-high { const x0 y0 x1 y1 op dst -- }
      x1 x0 - y1 y0 - 1 x0 { dx dy xi x }
      dx 0< if
        -1 to xi
        dx negate to dx
      then
      2 dx * dy - { d }
      y1 1+ y0 ?do
        const x i dst op execute
        d 0> if
          xi +to x
          dx dy - 2 * +to d
        else
          2 dx * +to d
        then
      loop
    ;

    \ Draw a line on a bitmap with a constant rectangle operation with slopes
    \ between 1 and -1
    : draw-rect-line-low { const width height x0 y0 x1 y1 op dst -- }
      width 2 / height 2 / { width2/ height2/ }
      x1 x0 - y1 y0 - 1 y0 { dx dy yi y }
      dy 0< if
        -1 to yi
        dy negate to dy
      then
      2 dy * dx - { d }
      x1 1+ x0 ?do
        const i width2/ - width y height2/ - height dst op execute
        d 0> if
          yi +to y
          dy dx - 2 * +to d
        else
          2 dy * +to d
        then
      loop
    ;

    \ Draw a line on a bitmap with a constant rectangle operation with steep
    \ slopes
    : draw-rect-line-high { const width height x0 y0 x1 y1 op dst -- }
      width 2 / height 2 / { width2/ height2/ }
      x1 x0 - y1 y0 - 1 x0 { dx dy xi x }
      dx 0< if
        -1 to xi
        dx negate to dx
      then
      2 dx * dy - { d }
      y1 1+ y0 ?do
        const x width2/ - width i height2/ - height dst op execute
        d 0> if
          xi +to x
          dx dy - 2 * +to d
        else
          2 dx * +to d
        then
      loop
    ;

    \ Draw a line on a bitmap with a bitmap operation with slopes between 1 and -1
    : draw-bitmap-line-low
      { src-x src-y src-width src-height x0 y0 x1 y1 op src dst -- }
      src-width 2 / src-height 2 / { src-width2/ src-height2/ }
      x1 x0 - y1 y0 - 1 y0 { dx dy yi y }
      dy 0< if
        -1 to yi
        dy negate to dy
      then
      2 dy * dx - { d }
      x1 1+ x0 ?do
        src-x i src-width2/ - src-width
        src-y y src-height2/ - src-height src dst op execute
        d 0> if
          yi +to y
          dy dx - 2 * +to d
        else
          2 dy * +to d
        then
      loop
    ;

    \ Draw a line on a bitmap with a bitmap operation with steep slopes
    : draw-bitmap-line-high
      { src-x src-y src-width src-height x0 y0 x1 y1 op src dst -- }
      src-width 2 / src-height 2 / { src-width2/ src-height2/ }
      x1 x0 - y1 y0 - 1 x0 { dx dy xi x }
      dx 0< if
        -1 to xi
        dx negate to dx
      then
      2 dx * dy - { d }
      y1 1+ y0 ?do
        src-x x src-width2/ - src-width
        src-y i src-height2/ - src-height src dst op execute
        d 0> if
          xi +to x
          dx dy - 2 * +to d
        else
          2 dx * +to d
        then
      loop
    ;

  end-module> import

  \ Draw a line on a bitmap with a pixel operation
  : draw-pixel-line { const x0 y0 x1 y1 op dst -- }
    y1 y0 - abs x1 x0 - abs < if
      x0 x1 > if
        const x1 y1 x0 y0 op dst draw-pixel-line-low
      else
        const x0 y0 x1 y1 op dst draw-pixel-line-low
      then
    else
      y0 y1 > if
        const x1 y1 x0 y0 op dst draw-pixel-line-high
      else
        const x0 y0 x1 y1 op dst draw-pixel-line-high
      then
    then
  ;
  
  \ Apply a bitmap to another bitmap with an operation along a line
  : draw-rect-line { const width height x0 y0 x1 y1 op dst -- }
    y1 y0 - abs x1 x0 - abs < if
      x0 x1 > if
        const width height x1 y1 x0 y0 op dst draw-rect-line-low
      else
        const width height x0 y0 x1 y1 op dst draw-rect-line-low
      then
    else
      y0 y1 > if
        const width height x1 y1 x0 y0 op dst draw-rect-line-high
      else
        const width height x0 y0 x1 y1 op dst draw-rect-line-high
      then
    then
  ;

  \ Apply a bitmap to another bitmap with an operation along a line
  : draw-bitmap-line
    { src-x src-y src-width src-height x0 y0 x1 y1 op src dst -- }
    y1 y0 - abs x1 x0 - abs < if
      x0 x1 > if
        src-x src-y src-width src-height x1 y1 x0 y0 op src dst
        draw-bitmap-line-low
      else
        src-x src-y src-width src-height x0 y0 x1 y1 op src dst
        draw-bitmap-line-low
      then
    else
      y0 y1 > if
        src-x src-y src-width src-height x1 y1 x0 y0 op src dst
        draw-bitmap-line-high
      else
        src-x src-y src-width src-height x0 y0 x1 y1 op src dst
        draw-bitmap-line-high
      then
    then
  ;
  
end-module
