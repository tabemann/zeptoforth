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

begin-module raytracing-fun

  oo import
  bitmap import
  bitmap-utils import
  pixmap16 import
  pixmap16-utils import
  st7735s import

  \ Screen dimensions
  160 constant screen-width
  80 constant screen-height

  \ Camera position
  0,0 2value camera-x
  -0,1 2value camera-y
  3,0 2value camera-z

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
  screen-width screen-height pixmap16-buf-size buffer: my-buffer

  \ Display
  <st7735s> class-size buffer: my-display

  \ Inited?
  false value inited

  \ Initialize the test:
  : init-test ( -- )
    lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
    my-buffer screen-width screen-height my-device <st7735s> my-display
    init-object
    my-display clear-pixmap
    my-display update-display
  ;

  \ Generate a color
  : color { D: x D: y D: z D: u D: v D: w -- color }
    v 0,0 d>= if
      v 1,0 2swap d- 0,0 dmax 1,0 dmin 255,0 f* f>s 0 0 rgb16
    else
      y 2,0 d+ v f/ { D: p }
      x u p f* d- f>s z w p f* d- f>s + 1 and s>f 2,0 f/ 0,3 d+
      v dnegate f* 0,2 d+ 0,0 dmax 1,0 dmin 255,0 f* f>s 0 swap 0 rgb16
    then
  ;
  
  \ Create a structure for storing all these variables
  begin-structure ray-data-size
    2field: ray-x
    2field: ray-y
    2field: ray-z
    2field: ray-u
    2field: ray-v
    2field: ray-w
    2field: ray-q
    2field: ray-e
    2field: ray-f
    2field: ray-g
    2field: ray-p
    2field: ray-d
    2field: ray-t
  end-structure

  \
  \ NOTE: This SQRT implementation is included because there is a bug
  \ in the existing SQRT implementation; it has been fixed in 'main'
  \ and 'devel' but is not in a release yet.
  \

  \ Calculate whether a square root is close enough
  : sqrt-close-enough ( f1 f2 -- flag )
    4dup d- 2rot dabs 2rot dabs dmax f/ dabs 2 0 d<
  ;
  
  \ Calculate a better square root guess
  : sqrt-better-guess ( f1 f2 -- f3 ) 2dup 2rot 2rot f/ d+ 0 2 f/ ;
  
  \ Calculate a square root
  : sqrt ( f1 -- f2 )
    64 >r
    2dup 0 2 f/
    begin
      r@ 0> if
        4dup f/ 2over sqrt-close-enough if
          rdrop 2nip true
        else
          r> 1- >r 4dup sqrt-better-guess 2nip false
        then
      else
        rdrop 2nip true
      then
    until
  ;
  

  \ Fire a ray
  : ray ( D: x D: y D: z D: u D: v D: w D: q -- D: color )
    ray-data-size [: { data }
      data ray-q 2! data ray-w 2! data ray-v 2! data ray-u 2!
      data ray-z 2! data ray-y 2! data ray-x 2!
      data ray-x 2@ data ray-q 2@ d- data ray-e 2!
      data ray-y 2@ data ray-q 2@ d- data ray-f 2!
      data ray-z 2@ data ray-g 2!
      data ray-u 2@ data ray-e 2@ f*
      data ray-v 2@ data ray-f 2@ f* d+
      data ray-w 2@ data ray-g 2@ f* d- data ray-p 2!
      data ray-p 2@ 2dup f*
      data ray-e 2@ 2dup f* d-
      data ray-f 2@ 2dup f* d-
      data ray-g 2@ 2dup f* d- 1,0 d+ data ray-d 2!
      data ray-d 2@ 0,0 d<= if
        data ray-x 2@ data ray-y 2@ data ray-z 2@
        data ray-u 2@ data ray-v 2@ data ray-w 2@ color
      else
        data ray-p 2@ dnegate data ray-d 2@ sqrt d- data ray-t 2!
        data ray-t 2@ 0,0 d<= if
          data ray-x 2@ data ray-y 2@ data ray-z 2@
          data ray-u 2@ data ray-v 2@ data ray-w 2@ color
        else
          data ray-t 2@ data ray-u 2@ f* data ray-x 2+!
          data ray-t 2@ data ray-v 2@ f* data ray-y 2+!
          data ray-t 2@ data ray-w 2@ f* dnegate data ray-z 2+!
          data ray-x 2@ data ray-q 2@ d- data ray-e 2!
          data ray-y 2@ data ray-q 2@ d- data ray-f 2!
          data ray-z 2@ data ray-g 2!
          data ray-u 2@ data ray-e 2@ f*
          data ray-v 2@ data ray-f 2@ f* d+
          data ray-w 2@ data ray-g 2@ f* d- 2,0 f* data ray-p 2!
          data ray-p 2@ data ray-e 2@ f* dnegate data ray-u 2+!
          data ray-p 2@ data ray-f 2@ f* dnegate data ray-v 2+!
          data ray-p 2@ data ray-g 2@ f* data ray-w 2+!
          data ray-q 2@ dnegate data ray-q 2!
          data ray-x 2@ data ray-y 2@ data ray-z 2@
          data ray-u 2@ data ray-v 2@ data ray-w 2@ data ray-q 2@ recurse
        then
      then
    ;] with-aligned-allot
  ;
  
  \ Raytrace!
  : raytrace ( -- )
    inited not if
      init-test
      true to inited
    then
    my-display clear-pixmap
    screen-width s>f 2,0 f/ { D: screen-width2/ }
    screen-height s>f 2,0 f/ { D: screen-height2/ }
    screen-height 0 ?do
      screen-width 0 ?do
        i s>f screen-width2/ d- screen-width2/ f/ { D: u }
        j s>f screen-height2/ d- screen-width2/ f/ { D: v }
        1,0 u u f* v v f* d+ 1,0 d+ sqrt f/ { D: w }
        u w f* to u
        v w f* to v
        u 0,0 d> if 1,0 else u 0,0 d< if -1,0 else 1,0 then then { D: q }
        camera-x camera-y camera-z u v w q ray
        i screen-height j - my-display draw-pixel-const
        my-display update-display
      loop
    loop
  ;
  
end-module