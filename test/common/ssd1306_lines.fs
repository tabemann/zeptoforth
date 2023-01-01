\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module bitmap-line-test
  
  oo import
  bitmap import
  ssd1306 import
  bitmap-utils import
  
  128 constant my-width
  64 constant my-height
  
  my-width my-height bitmap-buf-size constant my-buf-size
  my-buf-size 4 align buffer: my-buf
  <ssd1306> class-size buffer: my-ssd1306
  
  4 constant my-sprite-width
  4 constant my-sprite-height
  
  my-sprite-width my-sprite-height bitmap-buf-size constant my-sprite-buf-size
  my-sprite-buf-size 4 align buffer: my-sprite-buf
  <bitmap> class-size buffer: my-sprite
  
  : run-test ( -- )
    14 15 my-buf my-width my-height SSD1306_I2C_ADDR 1 <ssd1306> my-ssd1306 init-object
    my-sprite-buf my-sprite-width my-sprite-height <bitmap> my-sprite init-object
    $FF 1 2 0 1 my-sprite set-rect-const
    $FF 0 4 1 2 my-sprite set-rect-const
    $FF 1 2 3 1 my-sprite set-rect-const
    
    $FF 64 32 16 - 64 32 16 + ['] set-pixel-const my-ssd1306 draw-pixel-line
    $FF 64 16 - 32 64 16 + 32 ['] set-pixel-const my-ssd1306 draw-pixel-line
    
    $FF 4 4 2 2 2 61 ['] set-rect-const my-ssd1306 draw-rect-line
    $FF 4 4 2 61 125 61 ['] set-rect-const my-ssd1306 draw-rect-line
    $FF 4 4 126 61 126 2 ['] set-rect-const my-ssd1306 draw-rect-line
    $FF 4 4 126 2 2 2 ['] set-rect-const my-ssd1306 draw-rect-line
    
    0 0 4 4 64 16 - 32 16 - 64 16 + 32 16 + ['] or-rect my-sprite my-ssd1306 draw-bitmap-line
    0 0 4 4 64 16 - 32 16 + 64 16 + 32 16 - ['] or-rect my-sprite my-ssd1306 draw-bitmap-line
    
    my-ssd1306 update-display
  ;
  
end-module
