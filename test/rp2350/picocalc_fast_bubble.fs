\ Copyright (c) 2025 Travis Bemann
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

begin-module bubble

  oo import
  float32 import
  picocalc-term import
  pixmap8 import
  st7365p-8-common import
  
  begin-module ftrig
    
    begin-module ftrig-internal
    
      360 constant divisions
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
    
  : draw-bubble ( -- )
    page
    10 ms
    [: { display }
      display clear-pixmap
      display update-display
    ;] with-term-display
    320 150 2 { w n step }
    2e0 vpi v* n n>v v/ { r }
    0e0 0e0 0e0 { x y t }
    begin key? not while
      w n step r x y t [: { w n step r x y t display }
        n n>v { n' }
        n' 2e0 v/ { n'2/ }
        w 2 / { w2/ }
        display clear-pixmap
        n 0 ?do
          n 0 ?do
            j n>v { j' }
            j' y v+ { jy+ }
            j' r v* x v+ { jr*x+ }
            jy+ vsin jr*x+ vsin v+ { u }
            jy+ vcos jr*x+ vcos v+ { v }
            u t v+ to x
            v to y
            j' n' v/ 255e0 v* vround-zero v>u
            i n>v n' v/ 255e0 v* vround-zero v>u
            168 rgb8
            u n'2/ v* vround-zero v>n w2/ +
            y n'2/ v* vround-zero v>n w2/ +
            display draw-pixel-const
          step +loop
        step +loop
        t 0.1e0 v+ to t
        display update-display
        x y t
      ;] with-term-display
      to t to y to x
    repeat
    key drop
  ;

end-module
