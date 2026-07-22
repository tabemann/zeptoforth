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

\ This is based off code at:
\ https://github.com/oonap0oo/QB64-projects/blob/main/quiverbloom2.bas

begin-module quiverbloom

  float32 import
  picocalc-term import
  pixmap8 import
  st7365p-8-common import
  ansi-term import
  systick import
  
  begin-module ftrig
    
    begin-module ftrig-internal
    
      1024 constant divisions
      vpi 2e0 v/ divisions u>v v/ constant increment
      
      : fill-table ( -- )
        divisions 1+ 0 ?do i u>v increment v* vsin , loop
      ;
      
      create table fill-table
      
      : vsin'' ( theta -- )
        increment v/ vround-zero v>u cells table + @
      ;
      
      : vsin' ( theta -- )
        dup [ vpi 2e0 v/ ] literal v< if vsin'' exit then
        dup vpi v< if vpi swap v- vsin'' exit then
        vpi v-
        dup [ vpi 2e0 v/ ] literal v< if vsin'' vnegate exit then
        vpi swap v- vsin'' vnegate
      ;
      
    end-module> import
    
    : vsin ( f -- f' )
      dup [ vpi 2e0 v* ] literal v/ vround-zero
      [ vpi 2e0 v* ] literal v* v-
      dup v0< if [ vpi 2e0 v* ] literal v+ then
      vsin'
    ;
    
    : vcos ( f -- f' )
      [ vpi 2e0 v/ ] literal swap v- vsin
    ;
    
  end-module> import

  : rescale { x y -- x' y' }
    x [ 160e0 190e0 v/ ] literal v* 160e0 v+ v>n
    y [ 160e0 200e0 v/ ] literal v* 160e0 v+ v>n
  ;
  
  : vapproxisqrt { x -- isqrt }
    x 0.5e0 v* { x2 }
    $5F3759DF x 1 arshift - { y } \ yes, this is correct
    y 1.5e0 x2 y v* y v* v- v* to y \ 1st iteration of Newton-Raphson
    y 1.5e0 x2 y v* y v* v- v* \ 2nd iteration of Newton-Raphson
  ;

  : vapproxsqrt ( x -- sqrt ) dup vapproxisqrt v* ;

  : hypot { x y -- hypot } x x v* y y v* v+ vapproxsqrt ;
  
  : drop-keys ( -- ) begin key? while key drop repeat ;
  
  10000 30 / n>v constant frame-ticks
  
  : draw-quiverbloom ( -- )
    hide-cursor
    [:
      page
      [: dup clear-pixmap update-display ;] with-term-display
      systick-counter { last-tick }
      0e0 { t }
      begin
        t [: { t display }
          display clear-pixmap
          0e0 { x }
          begin x 12000e0 v<= while
            x 235e0 v/ { y }
            4e0 x 11e0 v/ 8e0 t v* v+ vsin v+ x 14e0 v/ vcos v* { k }
            y 9e0 v/ 19e0 v- { e }
            k e hypot y 9e0 v/ 3e0 t v* v+ vsin v+ { d }
            2e0 2e0 k v* vsin v*
            y 17e0 v/ vsin k v* 9e0 2e0 y 3e0 d v* v- vsin v* v+ v* v+ { q }
            d d v* 50e0 v/ { c }
            q 50e0 c vcos v* v- 85e0 v- { xp }
            d 39e0 v* q c vsin v* v- 620e0 v- { yp }
            xp t vcos v* yp t vsin v* v- { xr }
            xp t vsin v* yp t vcos v* v+ { yr }
            100e0 3e0 k v* vsin v* v>n { col }
            255 col 155 + 255 col - rgb8 xr yr rescale display
            draw-pixel-const
            2e0 x v+ to x
          repeat
          display update-display
        ;] with-term-display
        key? if exit then
        systick-counter { current-tick }
        current-tick last-tick - n>v frame-ticks v/
        [ vpi 400e0 v/ ] literal v* t v+
        [ 2e0 vpi v* ] literal vmod to t
        current-tick to last-tick
      again
    ;] try
    show-cursor
    ?raise
    drop-keys
  ;
  
end-module