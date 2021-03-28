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

begin-import-module-once led-wordlist

  \ Registers
  $40021000        constant RCC_BASE
  $4C RCC_BASE or  constant RCC_AHB2ENR
  $48000000        constant GPIOA
  : gpio-port  ( n -- a ) #10 lshift GPIOA or ;
  GPIOA   0 or     constant GPIOA_MODER
  GPIOA $10 or     constant GPIOA_IDR
  GPIOA  $C or     constant GPIOA_PUPD
  1 gpio-port      constant GPIOB
  4 gpio-port      constant GPIOE
  $18 GPIOB or     constant GPIOB_BSRR
  $18 GPIOE or     constant GPIOE_BSRR

  \ Initialize the LEDs
  : led-init  ( -- )                           \ initialize the leds
    1 1 lshift RCC_AHB2ENR bis!                \ enable clock on gpio port B
    1 4 lshift RCC_AHB2ENR bis!                \ enable clock on gpio port E
    3 4 lshift GPIOB bic!                      \ PB2 output
    1 4 lshift GPIOB bis!                      \ PB2 output
    3 16 lshift GPIOE bic!                     \ PE8 output
    1 16 lshift GPIOE bis!                     \ PE8 output
  ;

  \ Turn the red LED on
  : led-red-on  ( -- )
    1 2 lshift GPIOB_BSRR !
  ;

  \ Turn the red LED off
  : led-red-off  ( -- )
    1 2 16 + lshift GPIOB_BSRR !
  ;

  \ Turn the green LED on
  : led-green-on  ( -- )
    1 8 lshift GPIOE_BSRR !
  ;

  \ Turn the green LED offd
  : led-green-off  ( -- )
    1 8 16 + lshift GPIOE_BSRR !
  ;

end-module

\ Init
: init ( -- )
  init
  led-init
;

unimport led-wordlist

\ Warm reboot
warm
