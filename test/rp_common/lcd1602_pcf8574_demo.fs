\ Copyright (c) 2025 tmsgthb (GitHub)
\ Copyright (c) 2025 Travis Bemann
\ Copyright (c) 2026 Ken Mitton
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

begin-module lcd1602-pcf8574-demo

  oo import
  lcd1602-pcf8574 import

  \ An I2C LCD1602-PCF8574 device
  <lcd1602-pcf8574> class-size buffer: my-lcd1602-pcf8574

  \ Constants
  4 constant PIN_SDA \ GPIO 4, PIN 6
  5 constant PIN_SCL \ GPIO 5, PIN 7
  16 constant my-cols
  2 constant my-rows
  false constant oled?  \ LCD or OLED variant?

  \ Run the demo
  : run-demo ( -- )
    PIN_SDA PIN_SCL my-cols my-rows oled? LCD1602_PCF8574_I2C_ADDR 0
    <lcd1602-pcf8574> my-lcd1602-pcf8574 init-object
    500 ms
    true my-lcd1602-pcf8574 backlight!
    0 0 s" Hello, world!" my-lcd1602-pcf8574 put-text
    1 2 s" Zeptoforth" my-lcd1602-pcf8574 put-text
  ;

end-module
