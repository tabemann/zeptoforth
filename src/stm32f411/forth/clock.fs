\ Copyright (c) 2021-2025 Travis Bemann
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

\ Flash registers
$40023C00 constant FLASH_Base
FLASH_Base $00 + constant FLASH_ACR
8 bit constant FLASH_ACR_PRFTEN
9 bit constant FLASH_ACR_ICEN
10 bit constant FLASH_ACR_DCEN
$0F constant FLASH_ACR_LATENCY
3 0 lshift constant FLASH_ACR_LATENCY_Value

\ RCC registers
$40023800 constant RCC_Base
RCC_Base $00 + constant RCC_CR
RCC_Base $04 + constant RCC_PLLCFGR
RCC_Base $08 + constant RCC_CFGR

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
4 24 lshift constant RCC_PLLCFGR_PLLQ_Value
0 16 lshift constant RCC_PLLCFGR_PLLP_Value \ divide by 2
192 6 lshift constant RCC_PLLCFGR_PLLN_Value
25 0 lshift constant RCC_PLLCFGR_PLLM_Value
8 0 lshift constant RCC_PLLCFGR_PLLM_Nucleo64_Value
0 13 lshift constant RCC_CFGR_PPRE2_Value
4 10 lshift constant RCC_CFGR_PPRE1_Value
0 4 lshift constant RCC_CFGR_HPRE_Value

\ USART2 registers
$40004400 constant USART2_Base
USART2_Base $08 + constant USART2_BRR
USART2_Base $0C + constant USART2_CR1

\ USART2 register bits
13 bit constant USART2_CR1_UE
2 bit constant USART2_CR1_RE
3 bit constant USART2_CR1_TE

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

\ Set the flash latency and prefetch for 168 MHz
: set-flash-latency-for-100mhz
  FLASH_ACR @ FLASH_ACR_PRFTEN or FLASH_ACR_ICEN or FLASH_ACR_DCEN or
  FLASH_ACR_LATENCY bic FLASH_ACR_LATENCY_Value or FLASH_ACR !
;

\ Enable USART2
: enable-usart2 ( -- )
  [ USART2_CR1_UE USART2_CR1_RE or USART2_CR1_TE or ] literal USART2_CR1 bis!
;

\ Disable USART2
: disable-usart2 ( -- )
  [ USART2_CR1_UE USART2_CR1_RE or USART2_CR1_TE or ] literal USART2_CR1 bic!
;

\ Enable clock for 100 MHz
: set-clock-100mhz
  RCC_CR_PLLON RCC_CR bic!
  s" platform-nucleo64" find dup if
    >xt execute if RCC_PLLCFGR_PLLM_Nucleo64_Value 0 else -1 then
  else
    drop -1
  then
  if
    s" clock-pllm" find dup if
      >xt execute dup 2 < over 432 > or if drop RCC_PLLCFGR_PLLM_VALUE then
    else
      drop RCC_PLLCFGR_PLLM_Value
    then
  then
  [ RCC_PLLCFGR_PLLSRC
  RCC_PLLCFGR_PLLQ_Value or
  RCC_PLLCFGR_PLLP_Value or
  RCC_PLLCFGR_PLLN_Value or ] literal or RCC_PLLCFGR !    
  RCC_CR @ RCC_CR_PLLON or RCC_CR !
  begin RCC_CR @ RCC_CR_PLLRDY or until
  RCC_CFGR_PPRE2_Value RCC_CFGR_PPRE1_Value or RCC_CFGR_HPRE_Value or
  RCC_CFGR_SW_PLL or RCC_CFGR !
  begin RCC_CFGR @ RCC_CFGR_SWS and RCC_CFGR_SWS_PLL = until
;

\ Set the BRR for 100 MHz
: set-brr-100mhz
  [ 48000000 115200 2 / + 115200 / ] literal USART2_BRR !
\  [ 417 4 lshift 3 or ] literal USART2_BRR!
;

\ Fix an LDMIA interrupt issue
: fix-ldmia-issue ( -- ) 1 $E000E008 ! ;

\ Use 168 MHz
: use-100mhz ( -- )
  disable-int
  disable-usart2
  enable-hsi
  enable-hse
  set-flash-latency-for-100mhz
  set-clock-100mhz
  100_000_000 sysclk !
  set-brr-100mhz
  enable-usart2
  enable-int
;
    
\ Time multiplier
96 constant time-multiplier

\ Time divisor
8 constant time-divisor

\ Divisor to get ms from systicks
10 constant systick-divisor
    
\ Change the current wordlist
0 set-current

\ Not Cortex-M0 architecture
0 constant m0-architecture

\ Initialize
: init ( -- )
  init
  fix-ldmia-issue
  use-100mhz
;

\ Reboot
reboot
