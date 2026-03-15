\ Copyright (c) 2026 Travis Bemann
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

begin-module graph

  picocalc-term import
  pixmap8 import
  st7365p-8-common import
  float32 import
  
  begin-module graph-internal
  
    begin-structure space-size
      field: x-min
      field: x-max
      field: y-min
      field: y-max
    end-structure
    
    : x-incr { space -- v }
      space x-max @ space x-min @ v-
      term-pixels-dim@ drop n>v v/
    ;
    
    : convert-y { y space -- y' }
      y space y-min @ v-
      space y-max @ space y-min @ v- v/
      term-pixels-dim@ nip n>v v* v>n negate
      term-pixels-dim@ nip +
    ;
    
    : convert-x { x space -- x' }
      x space x-min @ v-
      space x-max @ space x-min @ v- v/
      term-pixels-dim@ drop n>v v* v>n
    ;
    
    7 constant mark-size
    
    : draw-x-marks { color space display -- }
      0e0 space convert-y { y }
      space x-max @ space x-min @ v- vabs v>n
      term-pixels-dim@ drop 4 / <= if
        space x-max @ space x-min @ vmax v>n 1+
        space x-max @ space x-min @ vmin v>n 1- ?do
          i n>v space convert-x { x }
          color x y [ mark-size 2 / ] literal - 1 mark-size
          display draw-rect-const
        loop
      then
    ;
    
    : draw-y-marks { color space display -- }
      0e0 space convert-x { x }
      space y-max @ space y-min @ v- vabs v>n
      term-pixels-dim@ nip 4 / <= if
        space y-max @ space y-min @ vmax v>n 1+
        space y-max @ space y-min @ vmin v>n 1- ?do
          i n>v space convert-y { y }
          color x [ mark-size 2 / ] literal - y mark-size 1
          display draw-rect-const
        loop
      then
    ;
    
    : draw-x-axis { color space display -- }
      space y-min @ space y-max @ vmin v0<=
      space y-min @ space y-max @ vmax v0>= and if
        0e0 space convert-y { y }
        color 0 y term-pixels-dim@ drop 1
        display draw-rect-const
        color space display draw-x-marks
      then
    ;
    
    : draw-y-axis { color space display -- }
      space x-min @ space x-max @ vmin v0<=
      space x-min @ space x-max @ vmax v0>= and if
        0e0 space convert-x { x }
        color x 0 1 term-pixels-dim@ nip
        display draw-rect-const
        color space display draw-y-marks
      then
    ;
    
    : draw-axes { color space display -- }
      color space display draw-x-axis
      color space display draw-y-axis
    ;
    
    : draw-fx { f color space display -- }
      space x-incr { incr }
      0e0 false { last-y last-y? }
      term-pixels-dim@ drop 0 ?do
        i n>v incr v* space x-min @ v+ { x }
        x f try dup ['] x-domain-error = if
          2drop 0e0 false
        else
          ?raise true
        then
        { y y? }
        y? if
          y space convert-y { y' }
          color i y' display draw-pixel-const
          last-y? if
            last-y space convert-y { last-y' }
            last-y' y' < if
              y' last-y' - { diff }
              diff 2 / { diff2/ }
              color i 1- last-y' 1 diff2/
              display draw-rect-const
              color i last-y' diff2/ + 1 diff diff2/ -
              display draw-rect-const
            else
              y' last-y' < if
                last-y' y' - { diff }
                diff 2 / { diff2/ }
                color i 1- last-y' diff2/ - 1 diff2/
                display draw-rect-const
                color i y' 1 diff diff2/ -
                display draw-rect-const
              then
            then 
          then
        then
        y to last-y
        y? to last-y?
      loop
    ;
    
    255 255 255 rgb8 constant axes-color
    
    create colors
    255 0 0 rgb8 c,
    0 255 0 rgb8 c,
    0 0 255 rgb8 c,
    255 0 255 rgb8 c,
    255 255 0 rgb8 c,
    0 255 255 rgb8 c,
    here colors - cell align, constant color-count
    
  end-module> import
  
  : draw-graph ( fn ... f0 u x-min x-max y-min y-max -- )
    page
    [: { display }
      display clear-pixmap
      display update-display
    ;] with-term-display
    space-size [: { x-min' x-max' y-min' y-max' space }
      x-min' space x-min !
      x-max' space x-max !
      y-min' space y-min !
      y-max' space y-max !
      space [: { count space display }
        axes-color space display draw-axes
        count 0 ?do
          colors i color-count umod + c@ { color }
          color space display draw-fx
        loop
      ;] with-term-display
    ;] with-aligned-allot
  ;
  
end-module