\ Copyright (c) 2013? Matthias Koch
\ Copyright (c) 2020-2021 Travis Bemann
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

\ Compile to flash
compile-to-flash

begin-import-module-once led-module

  import gpio-module
  
  \ Initialize the LEDs
  : led-init  ( -- )
    GPIOB gpio-clock-enable
    GPIOE gpio-clock-enable
    OUTPUT_MODE 2 GPIOB MODER!
    OUTPUT_MODE 8 GPIOE MODER!
  ;

  \ Turn the red LED on
  : led-red-on  ( -- )
    true 2 GPIOB BSRR!
  ;

  \ Turn the red LED off
  : led-red-off  ( -- )
    false 2 GPIOB BSRR!
  ;

  \ Turn the green LED on
  : led-green-on  ( -- )
    true 8 GPIOE BSRR!
  ;

  \ Turn the green LED offd
  : led-green-off  ( -- )
    false 8 GPIOE BSRR!
  ;

end-module

\ Init
: init ( -- )
  init
  led-init
;

unimport led-module

\ Warm reboot
warm
