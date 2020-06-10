\ Copyright (c) 2013? Matthias Koch
\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ Compile to flash
compile-to-flash

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

\ Init
: init ( -- )
  init
  led-init
;

\ Warm reboot
warm
