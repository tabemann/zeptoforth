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

begin-module picocalc-hello
  
  picocalc-term import
  oo import
  pixmap8 import
  font import
  rng import

  use-st7789v? not [if]
    ili9488-8-common import
  [else]
    st7789v-8-common import
  [then]

  use-5x8-font? [if]
    simple-font-5x8 import
  [then]
  use-6x8-font? [if]
    simple-font-6x8 import
  [then]
  use-7x8-font? [if]
    simple-font import
  [then]
  
  : hello ( r g b x y -- )
    [: { r g b x y display }
      r g b rgb8 s" Hello, world!" x y display
      [ use-5x8-font? ] [if] a-simple-font-5x8 [then]
      [ use-6x8-font? ] [if] a-simple-font-6x8 [then]
      [ use-7x8-font? ] [if] a-simple-font [then]
      draw-string-to-pixmap8
      display update-display
    ;] with-term-display
  ;

  : random-hellos { n -- }
    term-pixels-dim@ { width height }
    n 0 ?do
      random $FF and random $FF and random $FF and
      random 0 width s>f f* nip random 0 height s>f f* nip
      hello
    loop
  ;
  
end-module
