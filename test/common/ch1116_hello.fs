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

begin-module ch1116-hello

  oo import
  bitmap import
  ch1116 import
  font import
  simple-font import
  
  128 constant my-width
  64 constant my-height
  7 constant my-char-width
  8 constant my-char-height

  my-width my-height bitmap-buf-size constant my-buf-size
  my-buf-size 4 align buffer: my-buf
  <ch1116> class-size buffer: my-ch1116

  : init-test ( -- )
    init-simple-font
    14 15 my-buf my-width my-height CH1116_I2C_ADDR 1
    <ch1116> my-ch1116 init-object
    my-ch1116 clear-bitmap
    my-ch1116 update-display
  ;

  initializer init-test

  : hello ( -- )
    s" Hello, world!" { addr bytes }
    my-width 2 / bytes my-char-width * 2 / - { col }
    my-height 2 / my-char-height 2 / - { row }
    addr bytes col row op-or my-ch1116 a-simple-font draw-string
    my-ch1116 update-display
  ;
  
end-module
