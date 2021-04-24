\ Copyright (c) 2021 Travis Bemann
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

\ Compile this to flash
compile-to-flash

\ Set up the word order
0 1 2 set-order
1 set-current

\ Power management registers
$40007000 constant PWR_Base
PWR_Base $00 + constant PWR_CR1
PWR_Base $14 + constant PWR_SR2

\ Power management register bits
$600 constant PWR_CR1_VOS
$200 constant PWR_CR1_VOS_MODE1
$400 constant PWR_SR2_VOSF

\ Flash registers
$40022000 constant FLASH_Base
FLASH_Base $00 + constant FLASH_ACR

\ Flash register bits
$07 constant FLASH_ACR_LATENCY
2 0 lshift constant FLASH_ACR_LATENCY_Value
8 bit constant FLASH_ACR_PREFETCH

\ RCC registers
$40021000 constant RCC_Base
RCC_Base $00 + constant RCC_CR

\ RCC register bits
$08 constant RCC_CR_MSIRGSEL
$F0 constant RCC_CR_MSIRANGE
$B0 constant RCC_CR_MSIRANGE_48MHZ
$60 constant RCC_CR_MSIRANGE_4MHZ

\ USART2 registers
$40004400 constant USART2_Base
USART2_Base $00 + constant USART2_CR1
USART2_Base $0C + constant USART2_BRR

\ USART2 register bits
0 bit constant USART2_CR1_UE
2 bit constant USART2_CR1_RE
3 bit constant USART2_CR1_TE

\ RW SysTick Control and Status Register
$E000E010 constant SYST_CSR

\ SysTick tick interrupt flag mask
1 1 lshift constant SYST_CSR_TICKINT

\ SysTick tick enable flag mask
1 0 lshift constant SYST_CSR_ENABLE

\ Set power for 48 MHz
: set-pwr-for-48mhz ( -- )
  PWR_CR1 @ PWR_CR1_VOS bic PWR_CR1_VOS_MODE1 or PWR_CR1 !
  begin PWR_SR2 @ PWR_SR2_VOSF and 0= until
;

\ Set the flash latency and prefetch for 48 MHz MSI
: set-flash-latency-for-48mhz ( -- )
  FLASH_ACR @ FLASH_ACR_LATENCY bic FLASH_ACR_LATENCY_Value or
  FLASH_ACR_PREFETCH or FLASH_ACR !
;

\ Set the MSI to 48 MHz
: set-msi-48mhz ( -- )
  RCC_CR @ RCC_CR_MSIRANGE bic RCC_CR_MSIRANGE_48MHZ or RCC_CR !
  RCC_CR_MSIRGSEL RCC_CR bis!
;

\ Enable USART2
: enable-usart2 ( -- )
  [ USART2_CR1_UE USART2_CR1_RE or USART2_CR1_TE or ] literal USART2_CR1 bis!
;

\ Disable USART2
: disable-usart2 ( -- )
  [ USART2_CR1_UE USART2_CR1_RE or USART2_CR1_TE or ] literal USART2_CR1 bic!
;

\ Set the BRR for 48 MHz
: set-brr-48mhz
  48000000 115200 2 / + 115200 / USART2_BRR !
;

\ Disable SysTick
\ : disable-systick ( -- )
\   [ SYST_CSR_TICKINT SYST_CSR_ENABLE or ] literal SYST_CSR bic! dsb isb
\ ;

\ Use 48 MHz
: use-48mhz ( -- )
\   disable-systick
  disable-int
  disable-usart2
  set-pwr-for-48mhz
  set-flash-latency-for-48mhz
  set-msi-48mhz
  set-brr-48mhz
  enable-usart2
  enable-int
;

\ Time multiplier
48 constant time-multiplier

\ Time divisor
4 constant time-divisor

\ Divisor to get ms from systicks
10 constant systick-divisor
  
\ Change the current wordlist
0 set-current

\ Set the MSI to 4 MHz and then reboot
: warm ( -- )
  RCC_CR @ RCC_CR_MSIRANGE bic RCC_CR_MSIRANGE_4MHZ or RCC_CR !
  RCC_CR_MSIRGSEL RCC_CR bis!
  SYST_CSR_TICKINT SYST_CSR bic!
  SYST_CSR_ENABLE SYST_CSR bic!
  warm
;

\ Initialize
: init ( -- )
  init
  use-48mhz
;

\ Warm reboot
warm
