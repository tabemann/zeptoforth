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

begin-module bezier
  
  begin-module bezier-internal
    
    \ Implement De Casteljau's algorithm for an array of S31.32 fixed-point x,y
    \ pairs
    : de-casteljau ( D: t coefs-addr coefs-count -- D: x D: y )
      dup 2 * cells [: { D: t coefs-addr coefs-count beta }
        coefs-addr beta coefs-count 4 * cells move
        coefs-count 1 ?do
          coefs-count i - 0 ?do
            beta i [ 4 cells ] literal * + { beta' }
            beta' 2@ { D: x0 }
            beta' [ 2 cells ] literal + 2@ { D: y0 }
            beta' [ 4 cells ] literal + 2@ { D: x1 }
            beta' [ 6 cells ] literal + 2@ { D: y1 }
            x0 1,0 t d- f* x1 t f* d+ beta' 2!
            y0 1,0 t d- f* y1 t f* d+ beta' [ 2 cells ] literal + 2!
          loop
        loop
        beta 2@ beta [ 2 cells ] literal + 2@
      ;] with-aligned-allot
    ;
  
    \ Write frame
    : frame! ( D: t0 D: x0 D: y0 D: t1 D: x1 D: y1 -- )
      ram-here [ 12 cells ] literal - { addr }
      addr [ 10 cells ] literal + 2!
      addr [ 8 cells ] literal + 2!
      addr [ 6 cells ] literal + 2!
      addr [ 4 cells ] literal + 2!
      addr [ 2 cells ] literal + 2!
      addr 2!
    ;
    
    \ Read frame
    : frame@ ( -- D: t0 D: x0 D: y0 D: t1 D: x1 D: y1 )
      ram-here [ 12 cells ] literal - { addr }
      addr 2@
      addr [ 2 cells ] literal + 2@
      addr [ 4 cells ] literal + 2@
      addr [ 6 cells ] literal + 2@
      addr [ 8 cells ] literal + 2@
      addr [ 10 cells ] literal + 2@
    ;
    
    \ Push frame
    : >frame ( D: t0 D: x0 D: y0 D: t1 D: x1 D: y1 -- )
      [ 12 cells ] literal ram-allot frame!
    ;
    
    \ Drop frame
    : drop-frame ( -- ) [ -12 cells ] literal ram-allot ;
    
    \ Pop frame
    : frame> ( -- D: t0 D: x0 D: y0 D: t1 D: x1 D: y1 )
      frame@ drop-frame
    ;
    
  end-module> import
  
  \ Draw a bezier curve with a line function.
  \
  \ coefs-addr points to an array of S31.32 x,y pairs coefs-count long, which
  \ constitute the control points of the bezier curve.
  \
  \ max-depth is the maximum depth to which drawing the bezier curve will
  \ recurse.
  \
  \ xt is an execution token for drawing a single line segment; it has the
  \ following signature: ( D: x0 D: y0 D: x1 D: y1 -- )
  \
  \ Ensure that there is available space in the current task's RAM
  \ dictionary for max-depth * 48 bytes + coefs-count * 16 bytes.
  : draw-bezier ( coefs-addr coefs-count max-depth xt -- )
    over 2 < if 2drop drop exit then
    ram-here { saved-here }
    [: { addr count max-depth xt }
      2,0 max-depth negate fi** { D: epsilon }
      1 { depth }
      0,0 addr 2@ addr [ 2 cells ] literal + 2@
      1,0 addr count 1- 4 cells * + 2@ addr count 1- 4 cells * 2 cells + + 2@
      >frame
      begin depth 0> while
        depth max-depth = if
          frame> { D: t0 D: x0 D: y0 D: t1 D: x1 D: y1 }
          x0 y0 x1 y1 xt execute
          -1 +to depth
        else
          -1 +to depth
          frame> { D: t0 D: x0 D: y0 D: t1 D: x1 D: y1 }
          t1 t0 d- epsilon d> if
            t1 t0 d- 1 2arshift t0 d+ { D: t }
            t addr count de-casteljau { D: x D: y }
            x x0 d- 2dup f* y y0 d- 2dup f* d+ 4,0 d> if
              t0 x0 y0 t x y >frame 1 +to depth
            else
              x0 y0 x y xt execute
            then
            x1 x d- 2dup f* y1 y d- 2dup f* d+ 4,0 d> if
              t x y t1 x1 y1 >frame 1 +to depth
            else
              x y x1 y1 xt execute
            then
          else
            x0 y0 x1 y1 xt execute
          then
        then
      repeat
    ;] try
    saved-here ram-here!
    ?raise
  ;
  
end-module
