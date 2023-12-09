\ Copyright (c) 2023 Travis Bemann
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
\ SOFTWARE

begin-module lines-test

  oo import
  bitmap import
  bitmap-utils import
  ssd1306 import

  128 constant my-width
  64 constant my-height

  my-width my-height bitmap-buf-size constant my-buf-size
  my-buf-size 4 align buffer: my-buf
  <ssd1306> class-size buffer: my-ssd1306

  6 constant my-brush-width
  6 constant my-brush-height

  my-brush-width my-brush-height bitmap-buf-size constant my-brush-buf-size
  my-brush-buf-size 4 align buffer: my-brush-buf
  <bitmap> class-size buffer: my-brush

  : init-test ( -- )
    14 15 my-buf my-width my-height SSD1306_I2C_ADDR 1
    <ssd1306> my-ssd1306 init-object
    my-brush-buf my-brush-width my-brush-height <bitmap> my-brush init-object

    $FF 2 0 2 my-brush-height op-set my-brush draw-rect-const
    $FF 0 2 my-brush-width 2 op-set my-brush draw-rect-const
    $FF 1 1 4 4 op-set my-brush draw-rect-const

    $FF 0 0 my-width 1- my-height 1- op-set my-ssd1306 draw-pixel-line

    $FF 4 4 0 my-height 1- my-width 1- 0 op-set my-ssd1306 draw-rect-line

    0 0 my-brush-width my-brush-height
    my-width 3 / my-height 2 / my-width 3 / 2 * my-height 2 /
    op-or my-brush my-ssd1306 draw-bitmap-line

    $FF my-width 4 / my-height 4 / my-width 2 / my-height 2 /
    op-xor my-ssd1306 draw-rect-const

    0 0
    my-width 2 / my-brush-width 2 / -
    my-height 2 / my-brush-height 2 / -
    my-brush-width my-brush-height
    op-xor my-brush my-ssd1306 draw-rect

    my-ssd1306 update-display
  ;
  
end-module
