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

begin-module bitmap-utils

  oo import
  bitmap import

  begin-module bitmap-utils-internal

    \ Draw a line on a bitmap with a pixel operation with slopes between 1
    \ and -1
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

    \ Draw a line on a bitmap with a bitmap operation with slopes between 1
    \ and -1
    : draw-bitmap-line-low
      { src-x src-y width height x0 y0 x1 y1 op src dst -- }
      width 2 / height 2 / { width2/ height2/ }
      x1 x0 - y1 y0 - 1 y0 { dx dy yi y }
      dy 0< if
        -1 to yi
        dy negate to dy
      then
      2 dy * dx - { d }
      x1 1+ x0 ?do
        src-x i width2/ - width src-y y height2/ - height src dst op execute
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
      { src-x src-y width height x0 y0 x1 y1 op src dst -- }
      width 2 / height 2 / { width2/ height2/ }
      x1 x0 - y1 y0 - 1 x0 { dx dy xi x }
      dx 0< if
        -1 to xi
        dx negate to dx
      then
      2 dx * dy - { d }
      y1 1+ y0 ?do
        src-x x width2/ - width src-y i height2/ - height src dst op execute
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
    { src-x src-y width height x0 y0 x1 y1 op src dst -- }
    y1 y0 - abs x1 x0 - abs < if
      x0 x1 > if
        src-x src-y width height x1 y1 x0 y0 op src dst draw-bitmap-line-low
      else
        src-x src-y width height x0 y0 x1 y1 op src dst draw-bitmap-line-low
      then
    else
      y0 y1 > if
        src-x src-y width height x1 y1 x0 y0 op src dst draw-bitmap-line-high
      else
        src-x src-y width height x0 y0 x1 y1 op src dst draw-bitmap-line-high
      then
    then
  ;

  \ Draw an empty circle on a bitmap with a pixel operation
  : draw-pixel-circle { const x y radius op dst -- }
    1 radius - 0 radius 2* negate 0 radius { f ddx ddy dx dy }
    const x y radius + dst op execute
    const x y radius - dst op execute
    const x radius + y dst op execute
    const x radius - y dst op execute
    begin dx dy < while
      f 0>= if
        dy 1- to dy
        ddy 2 + dup to ddy
        f + to f
      then
      dx 1+ to dx
      ddx 2 + dup to ddx
      f 1+ + to f
      const x dx + y dy + dst op execute
      const x dx - y dy + dst op execute
      const x dx + y dy - dst op execute
      const x dx - y dy - dst op execute
      const x dy + y dx + dst op execute
      const x dy - y dx + dst op execute
      const x dy + y dx - dst op execute
      const x dy - y dx - dst op execute
    repeat
  ;

  \ Draw an empty circle on a bitmap with a constant rectangle operation
  : draw-rect-circle { const width height x y radius op dst -- }
    width 2 / height 2 / { width2/ height2/ }
    1 radius - 0 radius 2* negate 0 radius { f ddx ddy dx dy }
    const x width2/ - width y radius + height2/ - height dst op execute
    const x width2/ - width y radius - height2/ - height dst op execute
    const x radius + width2/ - width y height2/ - height dst op execute
    const x radius - width2/ - width y height2/ - height dst op execute
    begin dx dy < while
      f 0>= if
        dy 1- to dy
        ddy 2 + dup to ddy
        f + to f
      then
      dx 1+ to dx
      ddx 2 + dup to ddx
      f 1+ + to f
      const x dx + width2/ - width y dy + height2/ - height dst op execute
      const x dx - width2/ - width y dy + height2/ - height dst op execute
      const x dx + width2/ - width y dy - height2/ - height dst op execute
      const x dx - width2/ - width y dy - height2/ - height dst op execute
      const x dy + width2/ - width y dx + height2/ - height dst op execute
      const x dy - width2/ - width y dx + height2/ - height dst op execute
      const x dy + width2/ - width y dx - height2/ - height dst op execute
      const x dy - width2/ - width y dx - height2/ - height dst op execute
    repeat
  ;
  
  \ Draw an empty circle on a bitmap with another bitmap as a brush
  : draw-bitmap-circle { src-x src-y width height x y radius op src dst -- }
    width 2 / height 2 / { width2/ height2/ }
    1 radius - 0 radius 2* negate 0 radius { f ddx ddy dx dy }
    src-x x width2/ - width src-y y radius + height2/ - height
    src dst op execute
    src-x x width2/ - width src-y y radius - height2/ - height
    src dst op execute
    src-x x radius + width2/ - width src-y y height2/ - height
    src dst op execute
    src-x x radius - width2/ - width src-y y height2/ - height
    src dst op execute
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
      src dst op execute
      src-x x dx - width2/ - width src-y y dy + height2/ - height
      src dst op execute
      src-x x dx + width2/ - width src-y y dy - height2/ - height
      src dst op execute
      src-x x dx - width2/ - width src-y y dy - height2/ - height
      src dst op execute
      src-x x dy + width2/ - width src-y y dx + height2/ - height
      src dst op execute
      src-x x dy - width2/ - width src-y y dx + height2/ - height
      src dst op execute
      src-x x dy + width2/ - width src-y y dx - height2/ - height
      src dst op execute
      src-x x dy - width2/ - width src-y y dx - height2/ - height
      src dst op execute
    repeat
  ;

  \ Draw an filled circle on a bitmap with a rectangle operation
  : draw-filled-circle { const x y radius op dst -- }
    1 radius - 0 radius 2* negate 0 radius { f ddx ddy dx dy }
    const x 1 y radius + 1 dst op execute
    const x 1 y radius - 1 dst op execute
    const x radius - radius 2* y 1 dst op execute
    begin dx dy < while
      f 0>= if
        dy 1- to dy
        ddy 2 + dup to ddy
        f + to f
      then
      dx 1+ to dx
      ddx 2 + dup to ddx
      f 1+ + to f
      const x dx - dx 2 * y dy + 1 dst op execute
      const x dx - dx 2 * y dy - 1 dst op execute
      const x dy - dy 2 * y dx + 1 dst op execute
      const x dy - dy 2 * y dx - 1 dst op execute
    repeat
  ;

end-module
