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
    GPIOD gpio-clock-enable
    OUTPUT_MODE 12 GPIOD MODER!
    OUTPUT_MODE 13 GPIOD MODER!
    OUTPUT_MODE 14 GPIOD MODER!
    OUTPUT_MODE 15 GPIOD MODER!
  ;

  \ Turn the orange LED on
  : led-orange-on ( -- )
    true 13 GPIOD BSRR!
  ;

  \ Turn the orange LED off
  : led-orange-off ( -- )
    false 13 GPIOD BSRR!
  ;

  \ Turn the green LED on
  : led-green-on  ( -- )
    true 12 GPIOD BSRR!
  ;

  \ Turn the green LED off
  : led-green-off  ( -- )
    false 12 GPIOD BSRR!
  ;

  \ Turn the red LED on
  : led-red-on  ( -- )
    true 14 GPIOD BSRR!
  ;

  \ Turn the red LED off
  : led-red-off  ( -- )
    false 14 GPIOD BSRR!
  ;

  \ Turn the blue LED on
  : led-blue-on  ( -- )
    true 15 GPIOD BSRR!
  ;

  \ Turn the blue LED off
  : led-blue-off  ( -- )
    false 15 GPIOD BSRR!
  ;

end-module

\ Init
: init ( -- )
  init
  led-init
;

unimport led-module

\ Reboot
reboot
