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

  \ Registers
  $40023800 constant RCC_Base
  $30 RCC_Base or  constant RCC_AHB1ENR
  $40020000        constant GPIOA
  : gpio-port  ( n -- a ) #10 lshift GPIOA or ;
  GPIOA   0 or     constant GPIOA_MODER
  GPIOA $10 or     constant GPIOA_IDR
  GPIOA  $C or     constant GPIOA_PUPD
  3 gpio-port      constant GPIOD
  $18 GPIOD or     constant GPIOD_BSRR

  \ Initialize the LEDs
  : led-init  ( -- )
    1 3 lshift RCC_AHB1ENR bis!                \ enable clock on gpio port D
    $FF 24 lshift GPIOD bic!                   \ PD12-PD15 output
    $55 24 lshift GPIOD bis!                   \ PD12-PD15 output
  ;

  \ Turn the orange LED on
  : led-orange-on ( -- )
    1 13 lshift GPIOD_BSRR !
  ;

  \ Turn the orange LED off
  : led-orange-off ( -- )
    1 13 16 + lshift GPIOD_BSRR !
  ;

  \ Turn the green LED on
  : led-green-on  ( -- )
    1 12 lshift GPIOD_BSRR !
  ;

  \ Turn the green LED off
  : led-green-off  ( -- )
    1 12 16 + lshift GPIOD_BSRR !
  ;

  \ Turn the red LED on
  : led-red-on  ( -- )
    1 14 lshift GPIOD_BSRR !
  ;

  \ Turn the red LED off
  : led-red-off  ( -- )
    1 14 16 + lshift GPIOD_BSRR !
  ;

  \ Turn the blue LED on
  : led-blue-on  ( -- )
    1 15 lshift GPIOD_BSRR !
  ;

  \ Turn the blue LED off
  : led-blue-off  ( -- )
    1 15 16 + lshift GPIOD_BSRR !
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
