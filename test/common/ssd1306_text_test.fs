\ Copyright (c) 2022-2024 Travis Bemann
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

begin-module ssd1306-test

  oo import
  simple-font import
  text-display import
  ssd1306-text import
  
  128 constant my-width
  64 constant my-height
    
  7 constant my-char-width
  8 constant my-char-height
    
  my-width my-height my-char-width my-char-height text-buf-size buffer: my-text-buf
  my-width my-height my-char-width my-char-height invert-buf-size buffer: my-invert-buf
  <ssd1306-text> class-size buffer: my-display
  
  : init-test ( -- )
    init-simple-font
    14 15 my-text-buf my-invert-buf a-simple-font my-width my-height SSD1306_I2C_ADDR 1
    <ssd1306-text> my-display init-object
    s" Hello, World!" { c-addr u }
    my-height my-char-height / 0 ?do
      c-addr u 0 i my-display string!
      i 1 and if
        u 0 ?do true i j my-display invert! loop
      then
    loop
    my-display update-display
    begin key? not while
      my-height my-char-height / 0 ?do
        u 0 ?do j 1 and 0= i j my-display invert! loop
      loop
      my-display update-display
      my-height my-char-height / 0 ?do
        u 0 ?do j 1 and 0<> i j my-display invert! loop
      loop
      my-display update-display
    repeat
  ;    

end-module
