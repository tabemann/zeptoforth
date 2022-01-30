\ Copyright (c) 2021-2022 Travis Bemann
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
8 bit constant PWR_CR1_DBP

\ Flash registers
$40022000 constant FLASH_Base
FLASH_Base $00 + constant FLASH_ACR

\ Flash register bits
$07 constant FLASH_ACR_LATENCY
\ 2 0 lshift constant FLASH_ACR_LATENCY_Value
4 0 lshift constant FLASH_ACR_LATENCY_Value
8 bit constant FLASH_ACR_PREFETCH

\ RCC registers
$40021000 constant RCC_Base
RCC_Base $00 + constant RCC_CR
RCC_Base $08 + constant RCC_CFGR
RCC_Base $0C + constant RCC_PLLCFGR
RCC_Base $58 + constant RCC_APB1ENR1
RCC_Base $88 + constant RCC_CCIPR
RCC_Base $90 + constant RCC_BDCR

\ RCC register bits
$08 constant RCC_CR_MSIRGSEL
$F0 constant RCC_CR_MSIRANGE
$B0 constant RCC_CR_MSIRANGE_48MHZ
$60 constant RCC_CR_MSIRANGE_4MHZ
25 bit constant RCC_CR_PLLRDY
24 bit constant RCC_CR_PLLON
2 bit constant RCC_CR_MSIPLLEN
1 bit constant RCC_CR_MSIRDY
0 bit constant RCC_CR_MSION

7 28 lshift constant RCC_CFGR_MCOPRE
0 28 lshift constant RCC_CFGR_MCOPRE_Value
$F 24 lshift constant RCC_CFGR_MCOSEL
1 24 lshift constant RCC_CFGR_MCOSEL_SYSCLK \ SYSCLK
7 8 lshift constant RCC_CFGR_PPRE1
0 8 lshift constant RCC_CFGR_PPRE1_Value \ HCLK divided by 1
7 11 lshift constant RCC_CFGR_PPRE2
0 11 lshift constant RCC_CFGR_PPRE2_Value \ HCLK divided by 1
$F 4 lshift constant RCC_CFGR_HPRE
0 4 lshift constant RCC_CFGR_HPRE_Value \ SYSCLK divided by 1
$03 constant RCC_CFGR_SW
$0C constant RCC_CFGR_SWS
$00 constant RCC_CFGR_SW_MSI
$00 constant RCC_CFGR_SWS_MSI
$03 constant RCC_CFGR_SW_PLL
$0C constant RCC_CFGR_SWS_PLL

1 25 lshift constant RCC_PLLCFGR_PLLR_Value \ PLLR = 4
24 bit constant RCC_PLLCFGR_PLLREN
2 21 lshift constant RCC_PLLCFGR_PLLQ_Value \ PLLQ = 6
20 bit constant RCC_PLLCFGR_PLLQEN
\ 0 16 lshift constant RCC_PLLCFGR_PLLPEN_Value
72 8 lshift constant RCC_PLLCFGR_PLLN_Value \ PLLN = 72
0 4 lshift constant RCC_PLLCFGR_PLLM_Value \ PLLM = 1
1 0 lshift constant RCC_PLLCFGR_PLLSRC_Value \ MSI

28 bit constant RCC_APB1ENR1_PWREN

3 26 lshift constant RCC_CCIPR_CLK48SEL
2 26 lshift constant RCC_CCIPR_CLK48SEL_PLL

3 lshift 2 constant RCC_CCIPR_USART2SEL
1 lshift 2 constant RCC_CCIPR_USART2SEL_SYSCLK

25 bit constant RCC_BDCR_LSCOSEL
24 bit constant RCC_BDCR_LSCOEN
1 bit constant RCC_BDCR_LSERDY
0 bit constant RCC_BDCR_LSEON

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
: set-pwr-for-72mhz ( -- )
  PWR_CR1 @ PWR_CR1_VOS bic PWR_CR1_VOS_MODE1 or PWR_CR1 !
  begin PWR_SR2 @ PWR_SR2_VOSF and 0= until
;

\ Set the flash latency and prefetch for 48 MHz MSI
: set-flash-latency-for-72mhz ( -- )
  FLASH_ACR @ FLASH_ACR_LATENCY bic FLASH_ACR_LATENCY_Value or
  FLASH_ACR_PREFETCH or FLASH_ACR !
;

\ Enable the LSE
: enable-lse ( -- )
  RCC_APB1ENR1_PWREN RCC_APB1ENR1 bis!
  PWR_CR1_DBP PWR_CR1 bis!
  RCC_BDCR_LSEON RCC_BDCR bis!
  begin RCC_BDCR_LSERDY RCC_BDCR bit@ until
;

\ Set the MSI to 4 MHz
: set-msi-4mhz ( -- )
  RCC_CR_MSION RCC_CR bis!
  begin RCC_CR_MSIRDY RCC_CR bit@ until
  RCC_CR_MSIPLLEN RCC_CR bis!
  RCC_CR @ RCC_CR_MSIRANGE bic RCC_CR_MSIRANGE_4MHZ or RCC_CR !
  RCC_CR_MSIRGSEL RCC_CR bis!
  begin RCC_CR_MSIRDY RCC_CR bit@ until
  RCC_CFGR @ RCC_CFGR_SW bic RCC_CFGR_SW_MSI or RCC_CFGR !
  begin RCC_CFGR @ RCC_CFGR_SWS and RCC_CFGR_SWS_MSI = until
  RCC_CFGR @ RCC_CFGR_PPRE1 bic RCC_CFGR_PPRE1_Value or RCC_CFGR !
;

\ Set the PLL for RNG to 72 MHz
: set-pll-72mhz ( -- )
  RCC_CR_PLLON RCC_CR bic!
  begin RCC_CR_PLLRDY RCC_CR bit@ not until
  [ RCC_PLLCFGR_PLLR_Value
  RCC_PLLCFGR_PLLQ_Value or
  RCC_PLLCFGR_PLLN_Value or RCC_PLLCFGR_PLLM_Value or
  RCC_PLLCFGR_PLLSRC_Value or ] literal RCC_PLLCFGR !
  RCC_CR_PLLON RCC_CR bis!
  begin RCC_CR_PLLRDY RCC_CR bit@ until
  RCC_PLLCFGR_PLLREN RCC_PLLCFGR bis!
  RCC_PLLCFGR_PLLQEN RCC_PLLCFGR bis!
  RCC_CFGR @
  RCC_CFGR_MCOPRE bic RCC_CFGR_MCOPRE_Value or
  RCC_CFGR_MCOSEL bic RCC_CFGR_MCOSEL_SYSCLK or
  RCC_CFGR_PPRE1 bic RCC_CFGR_PPRE1_Value or
  RCC_CFGR_PPRE2 bic RCC_CFGR_PPRE2_Value or
  RCC_CFGR_HPRE bic RCC_CFGR_HPRE_Value or
  RCC_CFGR_SW bic RCC_CFGR_SW_PLL or RCC_CFGR !
  begin RCC_CFGR @ RCC_CFGR_SWS and RCC_CFGR_SWS_PLL = until
;

\ Set CLK48 to PLL
: set-clk48 ( -- )
  RCC_CCIPR @ RCC_CCIPR_CLK48SEL bic RCC_CCIPR_CLK48SEL_PLL or RCC_CCIPR !
;

\ Enable USART2
: enable-usart2 ( -- )
  [ USART2_CR1_UE USART2_CR1_RE or USART2_CR1_TE or ] literal USART2_CR1 bis!
;

\ Disable USART2
: disable-usart2 ( -- )
  [ USART2_CR1_UE USART2_CR1_RE or USART2_CR1_TE or ] literal USART2_CR1 bic!
;

\ Set the BRR for 72 MHz
: set-brr-72mhz
  [ 72000000 115200 2 / + 115200 / ] literal USART2_BRR !
;

\ Use 72 MHz
: use-72mhz ( -- )
  disable-int
  disable-usart2
  set-pwr-for-72mhz
  set-flash-latency-for-72mhz
  enable-lse
  set-msi-4mhz
  set-pll-72mhz
  set-clk48
  set-brr-72mhz
  enable-usart2
  enable-int
;

\ Fix an LDMIA interrupt issue
: fix-ldmia-issue ( -- ) 1 $E000E008 ! ;

\ Time multiplier
72 constant time-multiplier

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
  use-72mhz
;

\ Reboot
reboot
