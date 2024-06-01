\ Copyright (c) 2023-2024 Travis Bemann
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

begin-module st7735s-color-test

  oo import
  pixmap8 import
  pixmap8-utils import
  st7735s-8 import
  rng import

  \ Columns
  160 constant my-cols

  \ Rows
  80 constant my-rows

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
  my-cols my-rows pixmap8-buf-size buffer: my-buffer

  \ Display
  <st7735s-8> class-size buffer: my-display

  \ Initialize the test:
  : init-test
    lcd-din lcd-clk lcd-dc lcd-cs lcd-bl lcd-rst
    my-buffer my-cols my-rows my-device <st7735s-8> my-display init-object
    my-display clear-pixmap
    my-display update-display
  ;

  initializer init-test
  
  \ Permutations
  create permutations
  1 c, 2 c, 0 c,
  1 c, 0 c, 2 c,
  0 c, 1 c, 2 c,
  2 c, 1 c, 0 c,
  2 c, 0 c, 1 c,
  0 c, 2 c, 1 c,
  here permutations - 3 / cell align, constant permutation-count 
  
  \ Get a color
  : get-color { x y permutation -- color }
    permutations permutation 3 * + { entry }
    entry c@ case 0 of 0 endof 1 of x endof 2 of y endof endcase
    entry 1 + c@ case 0 of 0 endof 1 of x endof 2 of y endof endcase
    entry 2 + c@ case 0 of 0 endof 1 of x endof 2 of y endof endcase
    rgb8
  ;
  
  \ Carry out the test
  : run-test ( -- )
    0 { permutation }
    begin key? not while
      256 0 ?do
        256 0 ?do
          j i permutation get-color
          j my-cols * 8 arshift
          i my-rows * 8 arshift
          my-display draw-pixel-const
        loop
      loop
      my-display update-display
      100 ms
      permutation 1+ permutation-count umod to permutation
    repeat
    key drop
  ;

end-module