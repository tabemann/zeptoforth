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

\ Power registers
$40007000 constant PWR_Base
PWR_Base $00 + constant PWR_CR1
PWR_Base $04 + constant PWR_CSR1

\ Power bits
3 14 lshift constant PWR_CR1_VOS
16 bit constant PWR_CR1_ODEN
17 bit constant PWR_CR1_ODSWEN
16 bit constant PWR_CSR1_ODRDY
17 bit constant PWR_CSR1_ODSWRDY
0 14 lshift constant PWR_CR1_VOS_Value

\ Flash registers
$40023C00 constant FLASH_Base
FLASH_Base $00 + constant FLASH_ACR
8 bit constant FLASH_ACR_PRFTEN
9 bit constant FLASH_ACR_ARTEN
15 0 lshift constant FLASH_ACR_LATENCY
7 0 lshift constant FLASH_ACR_LATENCY_Value

\ RCC registers
$40023800 constant RCC_Base
RCC_Base $00 + constant RCC_CR
RCC_Base $04 + constant RCC_PLLCFGR
RCC_Base $08 + constant RCC_CFGR
RCC_Base $30 + constant RCC_APB1ENR
RCC_Base $44 + constant RCC_APB2ENR

\ RCC register bits
25 bit constant RCC_CR_PLLRDY
24 bit constant RCC_CR_PLLON
17 bit constant RCC_CR_HSERDY
16 bit constant RCC_CR_HSEON
1 bit constant RCC_CR_HSIRDY
0 bit constant RCC_CR_HSION

$03 constant RCC_CFGR_SW
0 constant RCC_CFGR_SW_HSI
0 bit constant RCC_CFGR_SW_HSE
1 bit constant RCC_CFGR_SW_PLL
$0C constant RCC_CFGR_SWS
0 constant RCC_CFGR_SWS_HSI
2 bit constant RCC_CFGR_SWS_HSE
3 bit constant RCC_CFGR_SWS_PLL

22 bit constant RCC_PLLCFGR_PLLSRC
9 24 lshift constant RCC_PLLCFGR_PLLQ_Value
0 16 lshift constant RCC_PLLCFGR_PLLP_Value
432 6 lshift constant RCC_PLLCFGR_PLLN_Value
25 0 lshift constant RCC_PLLCFGR_PLLM_Value
7 13 lshift constant RCC_CFGR_PPRE2
7 10 lshift constant RCC_CFGR_PPRE1
15 4 lshift constant RCC_CFGR_HPRE
4 13 lshift constant RCC_CFGR_PPRE2_Value
5 10 lshift constant RCC_CFGR_PPRE1_Value
0 4 lshift constant RCC_CFGR_HPRE_Value
28 bit constant RCC_APB1ENR_PWR

\ USART1 registers
$40011000 constant USART1_Base
USART1_Base $00 + constant USART1_CR1
USART1_Base $0C + constant USART1_BRR

\ USART1 register bits
0 bit constant USART1_CR1_UE
2 bit constant USART1_CR1_RE
3 bit constant USART1_CR1_TE

\ Enable HSI
: enable-hsi ( -- )
  RCC_CR @ RCC_CR_HSION or RCC_CR !
  begin RCC_CR @ RCC_CR_HSIRDY and until
  RCC_CFGR @ RCC_CFGR_SW bic RCC_CFGR_SW_HSI or RCC_CFGR !
  begin RCC_CFGR @ RCC_CFGR_SWS and RCC_CFGR_SWS_HSI = until
;

\ Enable HSE
: enable-hse ( -- )
  RCC_CR @ RCC_CR_HSEON or RCC_CR !
  begin RCC_CR @ RCC_CR_HSERDY and until
  RCC_CFGR @ RCC_CFGR_SW bic RCC_CFGR_SW_HSE or RCC_CFGR !
  begin RCC_CFGR @ RCC_CFGR_SWS and RCC_CFGR_SWS_HSE = until
  RCC_CR @ RCC_CR_HSION bic RCC_CR !
;

\ Set up power for 216 MHz
: set-pwr-for-216mhz ( -- )
  RCC_APB1ENR @ RCC_APB1ENR_PWR or RCC_APB1ENR !
  PWR_CR1 @ PWR_CR1_VOS bic PWR_CR1_VOS_Value or PWR_CR1 !
  PWR_CR1 @ PWR_CR1_ODEN or PWR_CR1 !
  begin PWR_CSR1 @ PWR_CSR1_ODRDY or until
  PWR_CR1 @ PWR_CR1_ODSWEN or PWR_CR1 !
  begin PWR_CSR1 @ PWR_CSR1_ODSWRDY or until
;

\ Set the clock divisors for 216 MHz
: set-clock-div-for-216mhz ( -- )
  RCC_CFGR @ RCC_CFGR_HPRE bic RCC_CFGR_HPRE_Value or RCC_CFGR !
  RCC_CFGR @ RCC_CFGR_PPRE1 bic RCC_CFGR_PPRE1_Value or RCC_CFGR !
  RCC_CFGR @ RCC_CFGR_PPRE2 bic RCC_CFGR_PPRE2_Value or RCC_CFGR !
;

\ Set the PLL for 216 Mhz
: set-pll-216mhz ( -- )
  RCC_CR_PLLON RCC_CR bic!
  [ RCC_PLLCFGR_PLLSRC RCC_PLLCFGR_PLLM_Value or
  RCC_PLLCFGR_PLLN_Value or RCC_PLLCFGR_PLLP_Value or
  RCC_PLLCFGR_PLLQ_Value or ] literal RCC_PLLCFGR !
  RCC_CR_PLLON RCC_CR bis!
  begin RCC_CR @ RCC_CR_PLLRDY and until
;

\ Set the flash latency and prefetch for 216 MHz
: set-flash-latency-for-216mhz ( -- )
  FLASH_ACR @ FLASH_ACR_LATENCY bic FLASH_ACR_LATENCY_Value or FLASH_ACR !
  FLASH_ACR_ARTEN FLASH_ACR bis!
  FLASH_ACR_PRFTEN FLASH_ACR bis!
;

\ Set system clock to PLL
: set-sysclk-pll ( -- )
  RCC_CFGR @ RCC_CFGR_SW bic RCC_CFGR_SW_PLL or RCC_CFGR !
  begin RCC_CFGR @ RCC_CFGR_SWS and RCC_CFGR_SWS_PLL = until
;

\ Enable USART1
: enable-usart1 ( -- )
  [ USART1_CR1_UE USART1_CR1_RE or USART1_CR1_TE or ] literal USART1_CR1 bis!
;

\ Disable USART1
: disable-usart1 ( -- )
  [ USART1_CR1_UE USART1_CR1_RE or USART1_CR1_TE or ] literal USART1_CR1 bic!
;

\ Set the BRR for 216 MHz
: set-brr-108mhz
  [ 108000000 115200 2 / + 115200 / ] literal USART1_BRR !
;

\ Use 216 MHz
: use-216mhz ( -- )
  disable-int
  disable-usart1
  enable-hsi
  enable-hse
  set-pwr-for-216mhz
  set-clock-div-for-216mhz
  set-pll-216mhz
  set-flash-latency-for-216mhz
  set-sysclk-pll
  set-brr-108mhz
  enable-usart1
  enable-int
;

\ Time multiplier
13 constant time-multiplier

\ Time divisor
1 constant time-divisor

\ Divisor to get ms from systicks
10 constant systick-divisor

\ Change the current wordlist
0 set-current

\ Initialize
: init ( -- )
  init
  use-216mhz
;

\ Reboot
reboot
