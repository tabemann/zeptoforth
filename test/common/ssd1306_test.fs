\ Copyright (c) 2022 Travis Bemann
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
  bitmap import
  ssd1306 import

  \ An I2C SSD1306 device
  <ssd1306> class-size buffer: my-ssd1306

  \ The number of columns
  128 constant my-cols

  \ The number of rows
  64 constant my-rows

  \ My framebuffer size
  my-cols my-rows * 8 / constant my-buf-size

  \ My framebuffer
  my-buf-size buffer: my-buf
  
  \ Initialize the test
  : init-test ( -- )
    14 15 my-buf my-cols my-rows SSD1306_I2C_ADDR 1
    <ssd1306> my-ssd1306 init-object
  ;

  \ Run the tests

  : set-test { const start-col end-col start-row end-row -- }
    const start-col end-col over - start-row end-row over - my-ssd1306
    set-rect-const
    my-ssd1306 update-display
  ;

  : or-test { const start-col end-col start-row end-row -- }
    const start-col end-col over - start-row end-row over - my-ssd1306
    or-rect-const
    my-ssd1306 update-display
  ;
  
  : and-test { const start-col end-col start-row end-row -- }
    const start-col end-col over - start-row end-row over - my-ssd1306
    and-rect-const
    my-ssd1306 update-display
  ;

  : bic-test { const start-col end-col start-row end-row -- }
    const start-col end-col over - start-row end-row over - my-ssd1306
    bic-rect-const
    my-ssd1306 update-display
  ;

  : xor-test { const start-col end-col start-row end-row -- }
    const start-col end-col over - start-row end-row over - my-ssd1306
    xor-rect-const
    my-ssd1306 update-display
  ;
  
end-module