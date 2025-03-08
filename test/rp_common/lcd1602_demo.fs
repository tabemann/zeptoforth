\ Copyright (c) 2025 tmsgthb (GitHub)
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

\ This is a demo for the LCD1602 I2C module.

begin-module lcd1602-demo

  lcd1602 import

  \ Constants
  0 constant I2C_0
  4 constant PIN_SDA \ GPIO 4, PIN 6
  5 constant PIN_SCL \ GPIO 5, PIN 7

  \ Run the demo
  : run-demo ( -- )
    PIN_SCL PIN_SDA I2C_0 init-lcd-i2c
    init-lcd
    5 0 do
      0 1 s" hello" put-text
      1 2 s" zeptoforth" put-text
      3000 ms clear-display
      1 3 s" another msg" put-text
      3000 ms clear-display
    loop
  ;

end-module
