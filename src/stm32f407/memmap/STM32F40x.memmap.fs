\ Copyright (c) 2020 Terry Porter <terry@tjporter.com.au>
\ Copyright (c) 2020 Travis Bemann
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

compile-to-flash

forth-wordlist 1 set-order
forth-wordlist set-current
defined? memmap-wordlist not [if]
wordlist constant memmap-wordlist
[then]
forth-wordlist memmap-wordlist 2 set-order
memmap-wordlist set-current

defined? use-RNG defined? RNG not and [if]
$50060800 constant RNG ( Random number generator ) 
RNG $0 + constant RNG_CR ( read-write )  \ control register
RNG $4 + constant RNG_SR (  )  \ status register
RNG $8 + constant RNG_DR ( read-only )  \ data register
[then]

defined? use-DCMI defined? DCMI not and [if]
$50050000 constant DCMI ( Digital camera interface ) 
DCMI $0 + constant DCMI_CR ( read-write )  \ control register 1
DCMI $4 + constant DCMI_SR ( read-only )  \ status register
DCMI $8 + constant DCMI_RIS ( read-only )  \ raw interrupt status register
DCMI $C + constant DCMI_IER ( read-write )  \ interrupt enable register
DCMI $10 + constant DCMI_MIS ( read-only )  \ masked interrupt status  register
DCMI $14 + constant DCMI_ICR ( write-only )  \ interrupt clear register
DCMI $18 + constant DCMI_ESCR ( read-write )  \ embedded synchronization code  register
DCMI $1C + constant DCMI_ESUR ( read-write )  \ embedded synchronization unmask  register
DCMI $20 + constant DCMI_CWSTRT ( read-write )  \ crop window start
DCMI $24 + constant DCMI_CWSIZE ( read-write )  \ crop window size
DCMI $28 + constant DCMI_DR ( read-only )  \ data register
[then]

defined? use-FSMC defined? FSMC not and [if]
$A0000000 constant FSMC ( Flexible static memory controller ) 
FSMC $0 + constant FSMC_BCR1 ( read-write )  \ SRAM/NOR-Flash chip-select control register  1
FSMC $4 + constant FSMC_BTR1 ( read-write )  \ SRAM/NOR-Flash chip-select timing register  1
FSMC $8 + constant FSMC_BCR2 ( read-write )  \ SRAM/NOR-Flash chip-select control register  2
FSMC $C + constant FSMC_BTR2 ( read-write )  \ SRAM/NOR-Flash chip-select timing register  2
FSMC $10 + constant FSMC_BCR3 ( read-write )  \ SRAM/NOR-Flash chip-select control register  3
FSMC $14 + constant FSMC_BTR3 ( read-write )  \ SRAM/NOR-Flash chip-select timing register  3
FSMC $18 + constant FSMC_BCR4 ( read-write )  \ SRAM/NOR-Flash chip-select control register  4
FSMC $1C + constant FSMC_BTR4 ( read-write )  \ SRAM/NOR-Flash chip-select timing register  4
FSMC $60 + constant FSMC_PCR2 ( read-write )  \ PC Card/NAND Flash control register  2
FSMC $64 + constant FSMC_SR2 (  )  \ FIFO status and interrupt register  2
FSMC $68 + constant FSMC_PMEM2 ( read-write )  \ Common memory space timing register  2
FSMC $6C + constant FSMC_PATT2 ( read-write )  \ Attribute memory space timing register  2
FSMC $74 + constant FSMC_ECCR2 ( read-only )  \ ECC result register 2
FSMC $80 + constant FSMC_PCR3 ( read-write )  \ PC Card/NAND Flash control register  3
FSMC $84 + constant FSMC_SR3 (  )  \ FIFO status and interrupt register  3
FSMC $88 + constant FSMC_PMEM3 ( read-write )  \ Common memory space timing register  3
FSMC $8C + constant FSMC_PATT3 ( read-write )  \ Attribute memory space timing register  3
FSMC $94 + constant FSMC_ECCR3 ( read-only )  \ ECC result register 3
FSMC $A0 + constant FSMC_PCR4 ( read-write )  \ PC Card/NAND Flash control register  4
FSMC $A4 + constant FSMC_SR4 (  )  \ FIFO status and interrupt register  4
FSMC $A8 + constant FSMC_PMEM4 ( read-write )  \ Common memory space timing register  4
FSMC $AC + constant FSMC_PATT4 ( read-write )  \ Attribute memory space timing register  4
FSMC $B0 + constant FSMC_PIO4 ( read-write )  \ I/O space timing register 4
FSMC $104 + constant FSMC_BWTR1 ( read-write )  \ SRAM/NOR-Flash write timing registers  1
FSMC $10C + constant FSMC_BWTR2 ( read-write )  \ SRAM/NOR-Flash write timing registers  2
FSMC $114 + constant FSMC_BWTR3 ( read-write )  \ SRAM/NOR-Flash write timing registers  3
FSMC $11C + constant FSMC_BWTR4 ( read-write )  \ SRAM/NOR-Flash write timing registers  4
[then]

defined? use-DBG defined? DBG not and [if]
$E0042000 constant DBG ( Debug support ) 
DBG $0 + constant DBG_DBGMCU_IDCODE ( read-only )  \ IDCODE
DBG $4 + constant DBG_DBGMCU_CR ( read-write )  \ Control Register
DBG $8 + constant DBG_DBGMCU_APB1_FZ ( read-write )  \ Debug MCU APB1 Freeze registe
DBG $C + constant DBG_DBGMCU_APB2_FZ ( read-write )  \ Debug MCU APB2 Freeze registe
[then]

defined? use-DMA2 defined? DMA2 not and [if]
$40026400 constant DMA2 ( DMA controller ) 
DMA2 $0 + constant DMA2_LISR ( read-only )  \ low interrupt status register
DMA2 $4 + constant DMA2_HISR ( read-only )  \ high interrupt status register
DMA2 $8 + constant DMA2_LIFCR ( read-write )  \ low interrupt flag clear  register
DMA2 $C + constant DMA2_HIFCR ( read-write )  \ high interrupt flag clear  register
DMA2 $10 + constant DMA2_S0CR ( read-write )  \ stream x configuration  register
DMA2 $14 + constant DMA2_S0NDTR ( read-write )  \ stream x number of data  register
DMA2 $18 + constant DMA2_S0PAR ( read-write )  \ stream x peripheral address  register
DMA2 $1C + constant DMA2_S0M0AR ( read-write )  \ stream x memory 0 address  register
DMA2 $20 + constant DMA2_S0M1AR ( read-write )  \ stream x memory 1 address  register
DMA2 $24 + constant DMA2_S0FCR (  )  \ stream x FIFO control register
DMA2 $28 + constant DMA2_S1CR ( read-write )  \ stream x configuration  register
DMA2 $2C + constant DMA2_S1NDTR ( read-write )  \ stream x number of data  register
DMA2 $30 + constant DMA2_S1PAR ( read-write )  \ stream x peripheral address  register
DMA2 $34 + constant DMA2_S1M0AR ( read-write )  \ stream x memory 0 address  register
DMA2 $38 + constant DMA2_S1M1AR ( read-write )  \ stream x memory 1 address  register
DMA2 $3C + constant DMA2_S1FCR (  )  \ stream x FIFO control register
DMA2 $40 + constant DMA2_S2CR ( read-write )  \ stream x configuration  register
DMA2 $44 + constant DMA2_S2NDTR ( read-write )  \ stream x number of data  register
DMA2 $48 + constant DMA2_S2PAR ( read-write )  \ stream x peripheral address  register
DMA2 $4C + constant DMA2_S2M0AR ( read-write )  \ stream x memory 0 address  register
DMA2 $50 + constant DMA2_S2M1AR ( read-write )  \ stream x memory 1 address  register
DMA2 $54 + constant DMA2_S2FCR (  )  \ stream x FIFO control register
DMA2 $58 + constant DMA2_S3CR ( read-write )  \ stream x configuration  register
DMA2 $5C + constant DMA2_S3NDTR ( read-write )  \ stream x number of data  register
DMA2 $60 + constant DMA2_S3PAR ( read-write )  \ stream x peripheral address  register
DMA2 $64 + constant DMA2_S3M0AR ( read-write )  \ stream x memory 0 address  register
DMA2 $68 + constant DMA2_S3M1AR ( read-write )  \ stream x memory 1 address  register
DMA2 $6C + constant DMA2_S3FCR (  )  \ stream x FIFO control register
DMA2 $70 + constant DMA2_S4CR ( read-write )  \ stream x configuration  register
DMA2 $74 + constant DMA2_S4NDTR ( read-write )  \ stream x number of data  register
DMA2 $78 + constant DMA2_S4PAR ( read-write )  \ stream x peripheral address  register
DMA2 $7C + constant DMA2_S4M0AR ( read-write )  \ stream x memory 0 address  register
DMA2 $80 + constant DMA2_S4M1AR ( read-write )  \ stream x memory 1 address  register
DMA2 $84 + constant DMA2_S4FCR (  )  \ stream x FIFO control register
DMA2 $88 + constant DMA2_S5CR ( read-write )  \ stream x configuration  register
DMA2 $8C + constant DMA2_S5NDTR ( read-write )  \ stream x number of data  register
DMA2 $90 + constant DMA2_S5PAR ( read-write )  \ stream x peripheral address  register
DMA2 $94 + constant DMA2_S5M0AR ( read-write )  \ stream x memory 0 address  register
DMA2 $98 + constant DMA2_S5M1AR ( read-write )  \ stream x memory 1 address  register
DMA2 $9C + constant DMA2_S5FCR (  )  \ stream x FIFO control register
DMA2 $A0 + constant DMA2_S6CR ( read-write )  \ stream x configuration  register
DMA2 $A4 + constant DMA2_S6NDTR ( read-write )  \ stream x number of data  register
DMA2 $A8 + constant DMA2_S6PAR ( read-write )  \ stream x peripheral address  register
DMA2 $AC + constant DMA2_S6M0AR ( read-write )  \ stream x memory 0 address  register
DMA2 $B0 + constant DMA2_S6M1AR ( read-write )  \ stream x memory 1 address  register
DMA2 $B4 + constant DMA2_S6FCR (  )  \ stream x FIFO control register
DMA2 $B8 + constant DMA2_S7CR ( read-write )  \ stream x configuration  register
DMA2 $BC + constant DMA2_S7NDTR ( read-write )  \ stream x number of data  register
DMA2 $C0 + constant DMA2_S7PAR ( read-write )  \ stream x peripheral address  register
DMA2 $C4 + constant DMA2_S7M0AR ( read-write )  \ stream x memory 0 address  register
DMA2 $C8 + constant DMA2_S7M1AR ( read-write )  \ stream x memory 1 address  register
DMA2 $CC + constant DMA2_S7FCR (  )  \ stream x FIFO control register
[then]

defined? use-DMA1 defined? DMA1 not and [if]
$40026000 constant DMA1 ( DMA controller ) 
DMA1 $0 + constant DMA1_LISR ( read-only )  \ low interrupt status register
DMA1 $4 + constant DMA1_HISR ( read-only )  \ high interrupt status register
DMA1 $8 + constant DMA1_LIFCR ( read-write )  \ low interrupt flag clear  register
DMA1 $C + constant DMA1_HIFCR ( read-write )  \ high interrupt flag clear  register
DMA1 $10 + constant DMA1_S0CR ( read-write )  \ stream x configuration  register
DMA1 $14 + constant DMA1_S0NDTR ( read-write )  \ stream x number of data  register
DMA1 $18 + constant DMA1_S0PAR ( read-write )  \ stream x peripheral address  register
DMA1 $1C + constant DMA1_S0M0AR ( read-write )  \ stream x memory 0 address  register
DMA1 $20 + constant DMA1_S0M1AR ( read-write )  \ stream x memory 1 address  register
DMA1 $24 + constant DMA1_S0FCR (  )  \ stream x FIFO control register
DMA1 $28 + constant DMA1_S1CR ( read-write )  \ stream x configuration  register
DMA1 $2C + constant DMA1_S1NDTR ( read-write )  \ stream x number of data  register
DMA1 $30 + constant DMA1_S1PAR ( read-write )  \ stream x peripheral address  register
DMA1 $34 + constant DMA1_S1M0AR ( read-write )  \ stream x memory 0 address  register
DMA1 $38 + constant DMA1_S1M1AR ( read-write )  \ stream x memory 1 address  register
DMA1 $3C + constant DMA1_S1FCR (  )  \ stream x FIFO control register
DMA1 $40 + constant DMA1_S2CR ( read-write )  \ stream x configuration  register
DMA1 $44 + constant DMA1_S2NDTR ( read-write )  \ stream x number of data  register
DMA1 $48 + constant DMA1_S2PAR ( read-write )  \ stream x peripheral address  register
DMA1 $4C + constant DMA1_S2M0AR ( read-write )  \ stream x memory 0 address  register
DMA1 $50 + constant DMA1_S2M1AR ( read-write )  \ stream x memory 1 address  register
DMA1 $54 + constant DMA1_S2FCR (  )  \ stream x FIFO control register
DMA1 $58 + constant DMA1_S3CR ( read-write )  \ stream x configuration  register
DMA1 $5C + constant DMA1_S3NDTR ( read-write )  \ stream x number of data  register
DMA1 $60 + constant DMA1_S3PAR ( read-write )  \ stream x peripheral address  register
DMA1 $64 + constant DMA1_S3M0AR ( read-write )  \ stream x memory 0 address  register
DMA1 $68 + constant DMA1_S3M1AR ( read-write )  \ stream x memory 1 address  register
DMA1 $6C + constant DMA1_S3FCR (  )  \ stream x FIFO control register
DMA1 $70 + constant DMA1_S4CR ( read-write )  \ stream x configuration  register
DMA1 $74 + constant DMA1_S4NDTR ( read-write )  \ stream x number of data  register
DMA1 $78 + constant DMA1_S4PAR ( read-write )  \ stream x peripheral address  register
DMA1 $7C + constant DMA1_S4M0AR ( read-write )  \ stream x memory 0 address  register
DMA1 $80 + constant DMA1_S4M1AR ( read-write )  \ stream x memory 1 address  register
DMA1 $84 + constant DMA1_S4FCR (  )  \ stream x FIFO control register
DMA1 $88 + constant DMA1_S5CR ( read-write )  \ stream x configuration  register
DMA1 $8C + constant DMA1_S5NDTR ( read-write )  \ stream x number of data  register
DMA1 $90 + constant DMA1_S5PAR ( read-write )  \ stream x peripheral address  register
DMA1 $94 + constant DMA1_S5M0AR ( read-write )  \ stream x memory 0 address  register
DMA1 $98 + constant DMA1_S5M1AR ( read-write )  \ stream x memory 1 address  register
DMA1 $9C + constant DMA1_S5FCR (  )  \ stream x FIFO control register
DMA1 $A0 + constant DMA1_S6CR ( read-write )  \ stream x configuration  register
DMA1 $A4 + constant DMA1_S6NDTR ( read-write )  \ stream x number of data  register
DMA1 $A8 + constant DMA1_S6PAR ( read-write )  \ stream x peripheral address  register
DMA1 $AC + constant DMA1_S6M0AR ( read-write )  \ stream x memory 0 address  register
DMA1 $B0 + constant DMA1_S6M1AR ( read-write )  \ stream x memory 1 address  register
DMA1 $B4 + constant DMA1_S6FCR (  )  \ stream x FIFO control register
DMA1 $B8 + constant DMA1_S7CR ( read-write )  \ stream x configuration  register
DMA1 $BC + constant DMA1_S7NDTR ( read-write )  \ stream x number of data  register
DMA1 $C0 + constant DMA1_S7PAR ( read-write )  \ stream x peripheral address  register
DMA1 $C4 + constant DMA1_S7M0AR ( read-write )  \ stream x memory 0 address  register
DMA1 $C8 + constant DMA1_S7M1AR ( read-write )  \ stream x memory 1 address  register
DMA1 $CC + constant DMA1_S7FCR (  )  \ stream x FIFO control register
[then]

defined? use-RCC defined? RCC not and [if]
$40023800 constant RCC ( Reset and clock control ) 
RCC $0 + constant RCC_CR (  )  \ clock control register
RCC $4 + constant RCC_PLLCFGR ( read-write )  \ PLL configuration register
RCC $8 + constant RCC_CFGR (  )  \ clock configuration register
RCC $C + constant RCC_CIR (  )  \ clock interrupt register
RCC $10 + constant RCC_AHB1RSTR ( read-write )  \ AHB1 peripheral reset register
RCC $14 + constant RCC_AHB2RSTR ( read-write )  \ AHB2 peripheral reset register
RCC $18 + constant RCC_AHB3RSTR ( read-write )  \ AHB3 peripheral reset register
RCC $20 + constant RCC_APB1RSTR ( read-write )  \ APB1 peripheral reset register
RCC $24 + constant RCC_APB2RSTR ( read-write )  \ APB2 peripheral reset register
RCC $30 + constant RCC_AHB1ENR ( read-write )  \ AHB1 peripheral clock register
RCC $34 + constant RCC_AHB2ENR ( read-write )  \ AHB2 peripheral clock enable  register
RCC $38 + constant RCC_AHB3ENR ( read-write )  \ AHB3 peripheral clock enable  register
RCC $40 + constant RCC_APB1ENR ( read-write )  \ APB1 peripheral clock enable  register
RCC $44 + constant RCC_APB2ENR ( read-write )  \ APB2 peripheral clock enable  register
RCC $50 + constant RCC_AHB1LPENR ( read-write )  \ AHB1 peripheral clock enable in low power  mode register
RCC $54 + constant RCC_AHB2LPENR ( read-write )  \ AHB2 peripheral clock enable in low power  mode register
RCC $58 + constant RCC_AHB3LPENR ( read-write )  \ AHB3 peripheral clock enable in low power  mode register
RCC $60 + constant RCC_APB1LPENR ( read-write )  \ APB1 peripheral clock enable in low power  mode register
RCC $64 + constant RCC_APB2LPENR ( read-write )  \ APB2 peripheral clock enabled in low power  mode register
RCC $70 + constant RCC_BDCR (  )  \ Backup domain control register
RCC $74 + constant RCC_CSR (  )  \ clock control & status  register
RCC $80 + constant RCC_SSCGR ( read-write )  \ spread spectrum clock generation  register
RCC $84 + constant RCC_PLLI2SCFGR ( read-write )  \ PLLI2S configuration register
[then]

defined? use-GPIOI defined? GPIOI not and [if]
$40022000 constant GPIOI ( General-purpose I/Os ) 
GPIOI $0 + constant GPIOI_MODER ( read-write )  \ GPIO port mode register
GPIOI $4 + constant GPIOI_OTYPER ( read-write )  \ GPIO port output type register
GPIOI $8 + constant GPIOI_OSPEEDR ( read-write )  \ GPIO port output speed  register
GPIOI $C + constant GPIOI_PUPDR ( read-write )  \ GPIO port pull-up/pull-down  register
GPIOI $10 + constant GPIOI_IDR ( read-only )  \ GPIO port input data register
GPIOI $14 + constant GPIOI_ODR ( read-write )  \ GPIO port output data register
GPIOI $18 + constant GPIOI_BSRR ( write-only )  \ GPIO port bit set/reset  register
GPIOI $1C + constant GPIOI_LCKR ( read-write )  \ GPIO port configuration lock  register
GPIOI $20 + constant GPIOI_AFRL ( read-write )  \ GPIO alternate function low  register
GPIOI $24 + constant GPIOI_AFRH ( read-write )  \ GPIO alternate function high  register
[then]

defined? use-GPIOH defined? GPIOH not and [if]
$40021C00 constant GPIOH ( General-purpose I/Os ) 
GPIOH $0 + constant GPIOH_MODER ( read-write )  \ GPIO port mode register
GPIOH $4 + constant GPIOH_OTYPER ( read-write )  \ GPIO port output type register
GPIOH $8 + constant GPIOH_OSPEEDR ( read-write )  \ GPIO port output speed  register
GPIOH $C + constant GPIOH_PUPDR ( read-write )  \ GPIO port pull-up/pull-down  register
GPIOH $10 + constant GPIOH_IDR ( read-only )  \ GPIO port input data register
GPIOH $14 + constant GPIOH_ODR ( read-write )  \ GPIO port output data register
GPIOH $18 + constant GPIOH_BSRR ( write-only )  \ GPIO port bit set/reset  register
GPIOH $1C + constant GPIOH_LCKR ( read-write )  \ GPIO port configuration lock  register
GPIOH $20 + constant GPIOH_AFRL ( read-write )  \ GPIO alternate function low  register
GPIOH $24 + constant GPIOH_AFRH ( read-write )  \ GPIO alternate function high  register
[then]

defined? use-GPIOG defined? GPIOG not and [if]
$40021800 constant GPIOG ( General-purpose I/Os ) 
GPIOG $0 + constant GPIOG_MODER ( read-write )  \ GPIO port mode register
GPIOG $4 + constant GPIOG_OTYPER ( read-write )  \ GPIO port output type register
GPIOG $8 + constant GPIOG_OSPEEDR ( read-write )  \ GPIO port output speed  register
GPIOG $C + constant GPIOG_PUPDR ( read-write )  \ GPIO port pull-up/pull-down  register
GPIOG $10 + constant GPIOG_IDR ( read-only )  \ GPIO port input data register
GPIOG $14 + constant GPIOG_ODR ( read-write )  \ GPIO port output data register
GPIOG $18 + constant GPIOG_BSRR ( write-only )  \ GPIO port bit set/reset  register
GPIOG $1C + constant GPIOG_LCKR ( read-write )  \ GPIO port configuration lock  register
GPIOG $20 + constant GPIOG_AFRL ( read-write )  \ GPIO alternate function low  register
GPIOG $24 + constant GPIOG_AFRH ( read-write )  \ GPIO alternate function high  register
[then]

defined? use-GPIOF defined? GPIOF not and [if]
$40021400 constant GPIOF ( General-purpose I/Os ) 
GPIOF $0 + constant GPIOF_MODER ( read-write )  \ GPIO port mode register
GPIOF $4 + constant GPIOF_OTYPER ( read-write )  \ GPIO port output type register
GPIOF $8 + constant GPIOF_OSPEEDR ( read-write )  \ GPIO port output speed  register
GPIOF $C + constant GPIOF_PUPDR ( read-write )  \ GPIO port pull-up/pull-down  register
GPIOF $10 + constant GPIOF_IDR ( read-only )  \ GPIO port input data register
GPIOF $14 + constant GPIOF_ODR ( read-write )  \ GPIO port output data register
GPIOF $18 + constant GPIOF_BSRR ( write-only )  \ GPIO port bit set/reset  register
GPIOF $1C + constant GPIOF_LCKR ( read-write )  \ GPIO port configuration lock  register
GPIOF $20 + constant GPIOF_AFRL ( read-write )  \ GPIO alternate function low  register
GPIOF $24 + constant GPIOF_AFRH ( read-write )  \ GPIO alternate function high  register
[then]

defined? use-GPIOE defined? GPIOE not and [if]
$40021000 constant GPIOE ( General-purpose I/Os ) 
GPIOE $0 + constant GPIOE_MODER ( read-write )  \ GPIO port mode register
GPIOE $4 + constant GPIOE_OTYPER ( read-write )  \ GPIO port output type register
GPIOE $8 + constant GPIOE_OSPEEDR ( read-write )  \ GPIO port output speed  register
GPIOE $C + constant GPIOE_PUPDR ( read-write )  \ GPIO port pull-up/pull-down  register
GPIOE $10 + constant GPIOE_IDR ( read-only )  \ GPIO port input data register
GPIOE $14 + constant GPIOE_ODR ( read-write )  \ GPIO port output data register
GPIOE $18 + constant GPIOE_BSRR ( write-only )  \ GPIO port bit set/reset  register
GPIOE $1C + constant GPIOE_LCKR ( read-write )  \ GPIO port configuration lock  register
GPIOE $20 + constant GPIOE_AFRL ( read-write )  \ GPIO alternate function low  register
GPIOE $24 + constant GPIOE_AFRH ( read-write )  \ GPIO alternate function high  register
[then]

defined? use-GPIOD defined? GPIOD not and [if]
$40020C00 constant GPIOD ( General-purpose I/Os ) 
GPIOD $0 + constant GPIOD_MODER ( read-write )  \ GPIO port mode register
GPIOD $4 + constant GPIOD_OTYPER ( read-write )  \ GPIO port output type register
GPIOD $8 + constant GPIOD_OSPEEDR ( read-write )  \ GPIO port output speed  register
GPIOD $C + constant GPIOD_PUPDR ( read-write )  \ GPIO port pull-up/pull-down  register
GPIOD $10 + constant GPIOD_IDR ( read-only )  \ GPIO port input data register
GPIOD $14 + constant GPIOD_ODR ( read-write )  \ GPIO port output data register
GPIOD $18 + constant GPIOD_BSRR ( write-only )  \ GPIO port bit set/reset  register
GPIOD $1C + constant GPIOD_LCKR ( read-write )  \ GPIO port configuration lock  register
GPIOD $20 + constant GPIOD_AFRL ( read-write )  \ GPIO alternate function low  register
GPIOD $24 + constant GPIOD_AFRH ( read-write )  \ GPIO alternate function high  register
[then]

defined? use-GPIOC defined? GPIOC not and [if]
$40020800 constant GPIOC ( General-purpose I/Os ) 
GPIOC $0 + constant GPIOC_MODER ( read-write )  \ GPIO port mode register
GPIOC $4 + constant GPIOC_OTYPER ( read-write )  \ GPIO port output type register
GPIOC $8 + constant GPIOC_OSPEEDR ( read-write )  \ GPIO port output speed  register
GPIOC $C + constant GPIOC_PUPDR ( read-write )  \ GPIO port pull-up/pull-down  register
GPIOC $10 + constant GPIOC_IDR ( read-only )  \ GPIO port input data register
GPIOC $14 + constant GPIOC_ODR ( read-write )  \ GPIO port output data register
GPIOC $18 + constant GPIOC_BSRR ( write-only )  \ GPIO port bit set/reset  register
GPIOC $1C + constant GPIOC_LCKR ( read-write )  \ GPIO port configuration lock  register
GPIOC $20 + constant GPIOC_AFRL ( read-write )  \ GPIO alternate function low  register
GPIOC $24 + constant GPIOC_AFRH ( read-write )  \ GPIO alternate function high  register
[then]

defined? use-GPIOB defined? GPIOB not and [if]
$40020400 constant GPIOB ( General-purpose I/Os ) 
GPIOB $0 + constant GPIOB_MODER ( read-write )  \ GPIO port mode register
GPIOB $4 + constant GPIOB_OTYPER ( read-write )  \ GPIO port output type register
GPIOB $8 + constant GPIOB_OSPEEDR ( read-write )  \ GPIO port output speed  register
GPIOB $C + constant GPIOB_PUPDR ( read-write )  \ GPIO port pull-up/pull-down  register
GPIOB $10 + constant GPIOB_IDR ( read-only )  \ GPIO port input data register
GPIOB $14 + constant GPIOB_ODR ( read-write )  \ GPIO port output data register
GPIOB $18 + constant GPIOB_BSRR ( write-only )  \ GPIO port bit set/reset  register
GPIOB $1C + constant GPIOB_LCKR ( read-write )  \ GPIO port configuration lock  register
GPIOB $20 + constant GPIOB_AFRL ( read-write )  \ GPIO alternate function low  register
GPIOB $24 + constant GPIOB_AFRH ( read-write )  \ GPIO alternate function high  register
[then]

defined? use-GPIOA defined? GPIOA not and [if]
$40020000 constant GPIOA ( General-purpose I/Os ) 
GPIOA $0 + constant GPIOA_MODER ( read-write )  \ GPIO port mode register
GPIOA $4 + constant GPIOA_OTYPER ( read-write )  \ GPIO port output type register
GPIOA $8 + constant GPIOA_OSPEEDR ( read-write )  \ GPIO port output speed  register
GPIOA $C + constant GPIOA_PUPDR ( read-write )  \ GPIO port pull-up/pull-down  register
GPIOA $10 + constant GPIOA_IDR ( read-only )  \ GPIO port input data register
GPIOA $14 + constant GPIOA_ODR ( read-write )  \ GPIO port output data register
GPIOA $18 + constant GPIOA_BSRR ( write-only )  \ GPIO port bit set/reset  register
GPIOA $1C + constant GPIOA_LCKR ( read-write )  \ GPIO port configuration lock  register
GPIOA $20 + constant GPIOA_AFRL ( read-write )  \ GPIO alternate function low  register
GPIOA $24 + constant GPIOA_AFRH ( read-write )  \ GPIO alternate function high  register
[then]

defined? use-SYSCFG defined? SYSCFG not and [if]
$40013800 constant SYSCFG ( System configuration controller ) 
SYSCFG $0 + constant SYSCFG_MEMRM ( read-write )  \ memory remap register
SYSCFG $4 + constant SYSCFG_PMC ( read-write )  \ peripheral mode configuration  register
SYSCFG $8 + constant SYSCFG_EXTICR1 ( read-write )  \ external interrupt configuration register  1
SYSCFG $C + constant SYSCFG_EXTICR2 ( read-write )  \ external interrupt configuration register  2
SYSCFG $10 + constant SYSCFG_EXTICR3 ( read-write )  \ external interrupt configuration register  3
SYSCFG $14 + constant SYSCFG_EXTICR4 ( read-write )  \ external interrupt configuration register  4
SYSCFG $20 + constant SYSCFG_CMPCR ( read-only )  \ Compensation cell control  register
[then]

defined? use-SPI1 defined? SPI1 not and [if]
$40013000 constant SPI1 ( Serial peripheral interface ) 
SPI1 $0 + constant SPI1_CR1 ( read-write )  \ control register 1
SPI1 $4 + constant SPI1_CR2 ( read-write )  \ control register 2
SPI1 $8 + constant SPI1_SR (  )  \ status register
SPI1 $C + constant SPI1_DR ( read-write )  \ data register
SPI1 $10 + constant SPI1_CRCPR ( read-write )  \ CRC polynomial register
SPI1 $14 + constant SPI1_RXCRCR ( read-only )  \ RX CRC register
SPI1 $18 + constant SPI1_TXCRCR ( read-only )  \ TX CRC register
SPI1 $1C + constant SPI1_I2SCFGR ( read-write )  \ I2S configuration register
SPI1 $20 + constant SPI1_I2SPR ( read-write )  \ I2S prescaler register
[then]

defined? use-SPI2 defined? SPI2 not and [if]
$40003800 constant SPI2 ( Serial peripheral interface ) 
SPI2 $0 + constant SPI2_CR1 ( read-write )  \ control register 1
SPI2 $4 + constant SPI2_CR2 ( read-write )  \ control register 2
SPI2 $8 + constant SPI2_SR (  )  \ status register
SPI2 $C + constant SPI2_DR ( read-write )  \ data register
SPI2 $10 + constant SPI2_CRCPR ( read-write )  \ CRC polynomial register
SPI2 $14 + constant SPI2_RXCRCR ( read-only )  \ RX CRC register
SPI2 $18 + constant SPI2_TXCRCR ( read-only )  \ TX CRC register
SPI2 $1C + constant SPI2_I2SCFGR ( read-write )  \ I2S configuration register
SPI2 $20 + constant SPI2_I2SPR ( read-write )  \ I2S prescaler register
[then]

defined? use-SPI3 defined? SPI3 not and [if]
$40003C00 constant SPI3 ( Serial peripheral interface ) 
SPI3 $0 + constant SPI3_CR1 ( read-write )  \ control register 1
SPI3 $4 + constant SPI3_CR2 ( read-write )  \ control register 2
SPI3 $8 + constant SPI3_SR (  )  \ status register
SPI3 $C + constant SPI3_DR ( read-write )  \ data register
SPI3 $10 + constant SPI3_CRCPR ( read-write )  \ CRC polynomial register
SPI3 $14 + constant SPI3_RXCRCR ( read-only )  \ RX CRC register
SPI3 $18 + constant SPI3_TXCRCR ( read-only )  \ TX CRC register
SPI3 $1C + constant SPI3_I2SCFGR ( read-write )  \ I2S configuration register
SPI3 $20 + constant SPI3_I2SPR ( read-write )  \ I2S prescaler register
[then]

defined? use-I2S2ext defined? I2S2ext not and [if]
$40003400 constant I2S2ext ( Serial peripheral interface ) 
I2S2ext $0 + constant I2S2ext_CR1 ( read-write )  \ control register 1
I2S2ext $4 + constant I2S2ext_CR2 ( read-write )  \ control register 2
I2S2ext $8 + constant I2S2ext_SR (  )  \ status register
I2S2ext $C + constant I2S2ext_DR ( read-write )  \ data register
I2S2ext $10 + constant I2S2ext_CRCPR ( read-write )  \ CRC polynomial register
I2S2ext $14 + constant I2S2ext_RXCRCR ( read-only )  \ RX CRC register
I2S2ext $18 + constant I2S2ext_TXCRCR ( read-only )  \ TX CRC register
I2S2ext $1C + constant I2S2ext_I2SCFGR ( read-write )  \ I2S configuration register
I2S2ext $20 + constant I2S2ext_I2SPR ( read-write )  \ I2S prescaler register
[then]

defined? use-I2S3ext defined? I2S3ext not and [if]
$40004000 constant I2S3ext ( Serial peripheral interface ) 
I2S3ext $0 + constant I2S3ext_CR1 ( read-write )  \ control register 1
I2S3ext $4 + constant I2S3ext_CR2 ( read-write )  \ control register 2
I2S3ext $8 + constant I2S3ext_SR (  )  \ status register
I2S3ext $C + constant I2S3ext_DR ( read-write )  \ data register
I2S3ext $10 + constant I2S3ext_CRCPR ( read-write )  \ CRC polynomial register
I2S3ext $14 + constant I2S3ext_RXCRCR ( read-only )  \ RX CRC register
I2S3ext $18 + constant I2S3ext_TXCRCR ( read-only )  \ TX CRC register
I2S3ext $1C + constant I2S3ext_I2SCFGR ( read-write )  \ I2S configuration register
I2S3ext $20 + constant I2S3ext_I2SPR ( read-write )  \ I2S prescaler register
[then]

defined? use-SDIO defined? SDIO not and [if]
$40012C00 constant SDIO ( Secure digital input/output  interface ) 
SDIO $0 + constant SDIO_POWER ( read-write )  \ power control register
SDIO $4 + constant SDIO_CLKCR ( read-write )  \ SDI clock control register
SDIO $8 + constant SDIO_ARG ( read-write )  \ argument register
SDIO $C + constant SDIO_CMD ( read-write )  \ command register
SDIO $10 + constant SDIO_RESPCMD ( read-only )  \ command response register
SDIO $14 + constant SDIO_RESP1 ( read-only )  \ response 1..4 register
SDIO $18 + constant SDIO_RESP2 ( read-only )  \ response 1..4 register
SDIO $1C + constant SDIO_RESP3 ( read-only )  \ response 1..4 register
SDIO $20 + constant SDIO_RESP4 ( read-only )  \ response 1..4 register
SDIO $24 + constant SDIO_DTIMER ( read-write )  \ data timer register
SDIO $28 + constant SDIO_DLEN ( read-write )  \ data length register
SDIO $2C + constant SDIO_DCTRL ( read-write )  \ data control register
SDIO $30 + constant SDIO_DCOUNT ( read-only )  \ data counter register
SDIO $34 + constant SDIO_STA ( read-only )  \ status register
SDIO $38 + constant SDIO_ICR ( read-write )  \ interrupt clear register
SDIO $3C + constant SDIO_MASK ( read-write )  \ mask register
SDIO $48 + constant SDIO_FIFOCNT ( read-only )  \ FIFO counter register
SDIO $80 + constant SDIO_FIFO ( read-write )  \ data FIFO register
[then]

defined? use-ADC1 defined? ADC1 not and [if]
$40012000 constant ADC1 ( Analog-to-digital converter ) 
ADC1 $0 + constant ADC1_SR ( read-write )  \ status register
ADC1 $4 + constant ADC1_CR1 ( read-write )  \ control register 1
ADC1 $8 + constant ADC1_CR2 ( read-write )  \ control register 2
ADC1 $C + constant ADC1_SMPR1 ( read-write )  \ sample time register 1
ADC1 $10 + constant ADC1_SMPR2 ( read-write )  \ sample time register 2
ADC1 $14 + constant ADC1_JOFR1 ( read-write )  \ injected channel data offset register  x
ADC1 $18 + constant ADC1_JOFR2 ( read-write )  \ injected channel data offset register  x
ADC1 $1C + constant ADC1_JOFR3 ( read-write )  \ injected channel data offset register  x
ADC1 $20 + constant ADC1_JOFR4 ( read-write )  \ injected channel data offset register  x
ADC1 $24 + constant ADC1_HTR ( read-write )  \ watchdog higher threshold  register
ADC1 $28 + constant ADC1_LTR ( read-write )  \ watchdog lower threshold  register
ADC1 $2C + constant ADC1_SQR1 ( read-write )  \ regular sequence register 1
ADC1 $30 + constant ADC1_SQR2 ( read-write )  \ regular sequence register 2
ADC1 $34 + constant ADC1_SQR3 ( read-write )  \ regular sequence register 3
ADC1 $38 + constant ADC1_JSQR ( read-write )  \ injected sequence register
ADC1 $3C + constant ADC1_JDR1 ( read-only )  \ injected data register x
ADC1 $40 + constant ADC1_JDR2 ( read-only )  \ injected data register x
ADC1 $44 + constant ADC1_JDR3 ( read-only )  \ injected data register x
ADC1 $48 + constant ADC1_JDR4 ( read-only )  \ injected data register x
ADC1 $4C + constant ADC1_DR ( read-only )  \ regular data register
[then]

defined? use-ADC2 defined? ADC2 not and [if]
$40012100 constant ADC2 ( Analog-to-digital converter ) 
ADC2 $0 + constant ADC2_SR ( read-write )  \ status register
ADC2 $4 + constant ADC2_CR1 ( read-write )  \ control register 1
ADC2 $8 + constant ADC2_CR2 ( read-write )  \ control register 2
ADC2 $C + constant ADC2_SMPR1 ( read-write )  \ sample time register 1
ADC2 $10 + constant ADC2_SMPR2 ( read-write )  \ sample time register 2
ADC2 $14 + constant ADC2_JOFR1 ( read-write )  \ injected channel data offset register  x
ADC2 $18 + constant ADC2_JOFR2 ( read-write )  \ injected channel data offset register  x
ADC2 $1C + constant ADC2_JOFR3 ( read-write )  \ injected channel data offset register  x
ADC2 $20 + constant ADC2_JOFR4 ( read-write )  \ injected channel data offset register  x
ADC2 $24 + constant ADC2_HTR ( read-write )  \ watchdog higher threshold  register
ADC2 $28 + constant ADC2_LTR ( read-write )  \ watchdog lower threshold  register
ADC2 $2C + constant ADC2_SQR1 ( read-write )  \ regular sequence register 1
ADC2 $30 + constant ADC2_SQR2 ( read-write )  \ regular sequence register 2
ADC2 $34 + constant ADC2_SQR3 ( read-write )  \ regular sequence register 3
ADC2 $38 + constant ADC2_JSQR ( read-write )  \ injected sequence register
ADC2 $3C + constant ADC2_JDR1 ( read-only )  \ injected data register x
ADC2 $40 + constant ADC2_JDR2 ( read-only )  \ injected data register x
ADC2 $44 + constant ADC2_JDR3 ( read-only )  \ injected data register x
ADC2 $48 + constant ADC2_JDR4 ( read-only )  \ injected data register x
ADC2 $4C + constant ADC2_DR ( read-only )  \ regular data register
[then]

defined? use-ADC3 defined? ADC3 not and [if]
$40012200 constant ADC3 ( Analog-to-digital converter ) 
ADC3 $0 + constant ADC3_SR ( read-write )  \ status register
ADC3 $4 + constant ADC3_CR1 ( read-write )  \ control register 1
ADC3 $8 + constant ADC3_CR2 ( read-write )  \ control register 2
ADC3 $C + constant ADC3_SMPR1 ( read-write )  \ sample time register 1
ADC3 $10 + constant ADC3_SMPR2 ( read-write )  \ sample time register 2
ADC3 $14 + constant ADC3_JOFR1 ( read-write )  \ injected channel data offset register  x
ADC3 $18 + constant ADC3_JOFR2 ( read-write )  \ injected channel data offset register  x
ADC3 $1C + constant ADC3_JOFR3 ( read-write )  \ injected channel data offset register  x
ADC3 $20 + constant ADC3_JOFR4 ( read-write )  \ injected channel data offset register  x
ADC3 $24 + constant ADC3_HTR ( read-write )  \ watchdog higher threshold  register
ADC3 $28 + constant ADC3_LTR ( read-write )  \ watchdog lower threshold  register
ADC3 $2C + constant ADC3_SQR1 ( read-write )  \ regular sequence register 1
ADC3 $30 + constant ADC3_SQR2 ( read-write )  \ regular sequence register 2
ADC3 $34 + constant ADC3_SQR3 ( read-write )  \ regular sequence register 3
ADC3 $38 + constant ADC3_JSQR ( read-write )  \ injected sequence register
ADC3 $3C + constant ADC3_JDR1 ( read-only )  \ injected data register x
ADC3 $40 + constant ADC3_JDR2 ( read-only )  \ injected data register x
ADC3 $44 + constant ADC3_JDR3 ( read-only )  \ injected data register x
ADC3 $48 + constant ADC3_JDR4 ( read-only )  \ injected data register x
ADC3 $4C + constant ADC3_DR ( read-only )  \ regular data register
[then]

defined? use-USART6 defined? USART6 not and [if]
$40011400 constant USART6 ( Universal synchronous asynchronous receiver  transmitter ) 
USART6 $0 + constant USART6_SR (  )  \ Status register
USART6 $4 + constant USART6_DR ( read-write )  \ Data register
USART6 $8 + constant USART6_BRR ( read-write )  \ Baud rate register
USART6 $C + constant USART6_CR1 ( read-write )  \ Control register 1
USART6 $10 + constant USART6_CR2 ( read-write )  \ Control register 2
USART6 $14 + constant USART6_CR3 ( read-write )  \ Control register 3
USART6 $18 + constant USART6_GTPR ( read-write )  \ Guard time and prescaler  register
[then]

defined? use-USART1 defined? USART1 not and [if]
$40011000 constant USART1 ( Universal synchronous asynchronous receiver  transmitter ) 
USART1 $0 + constant USART1_SR (  )  \ Status register
USART1 $4 + constant USART1_DR ( read-write )  \ Data register
USART1 $8 + constant USART1_BRR ( read-write )  \ Baud rate register
USART1 $C + constant USART1_CR1 ( read-write )  \ Control register 1
USART1 $10 + constant USART1_CR2 ( read-write )  \ Control register 2
USART1 $14 + constant USART1_CR3 ( read-write )  \ Control register 3
USART1 $18 + constant USART1_GTPR ( read-write )  \ Guard time and prescaler  register
[then]

defined? use-USART2 defined? USART2 not and [if]
$40004400 constant USART2 ( Universal synchronous asynchronous receiver  transmitter ) 
USART2 $0 + constant USART2_SR (  )  \ Status register
USART2 $4 + constant USART2_DR ( read-write )  \ Data register
USART2 $8 + constant USART2_BRR ( read-write )  \ Baud rate register
USART2 $C + constant USART2_CR1 ( read-write )  \ Control register 1
USART2 $10 + constant USART2_CR2 ( read-write )  \ Control register 2
USART2 $14 + constant USART2_CR3 ( read-write )  \ Control register 3
USART2 $18 + constant USART2_GTPR ( read-write )  \ Guard time and prescaler  register
[then]

defined? use-USART3 defined? USART3 not and [if]
$40004800 constant USART3 ( Universal synchronous asynchronous receiver  transmitter ) 
USART3 $0 + constant USART3_SR (  )  \ Status register
USART3 $4 + constant USART3_DR ( read-write )  \ Data register
USART3 $8 + constant USART3_BRR ( read-write )  \ Baud rate register
USART3 $C + constant USART3_CR1 ( read-write )  \ Control register 1
USART3 $10 + constant USART3_CR2 ( read-write )  \ Control register 2
USART3 $14 + constant USART3_CR3 ( read-write )  \ Control register 3
USART3 $18 + constant USART3_GTPR ( read-write )  \ Guard time and prescaler  register
[then]

defined? use-DAC defined? DAC not and [if]
$40007400 constant DAC ( Digital-to-analog converter ) 
DAC $0 + constant DAC_CR ( read-write )  \ control register
DAC $4 + constant DAC_SWTRIGR ( write-only )  \ software trigger register
DAC $8 + constant DAC_DHR12R1 ( read-write )  \ channel1 12-bit right-aligned data holding  register
DAC $C + constant DAC_DHR12L1 ( read-write )  \ channel1 12-bit left aligned data holding  register
DAC $10 + constant DAC_DHR8R1 ( read-write )  \ channel1 8-bit right aligned data holding  register
DAC $14 + constant DAC_DHR12R2 ( read-write )  \ channel2 12-bit right aligned data holding  register
DAC $18 + constant DAC_DHR12L2 ( read-write )  \ channel2 12-bit left aligned data holding  register
DAC $1C + constant DAC_DHR8R2 ( read-write )  \ channel2 8-bit right-aligned data holding  register
DAC $20 + constant DAC_DHR12RD ( read-write )  \ Dual DAC 12-bit right-aligned data holding  register
DAC $24 + constant DAC_DHR12LD ( read-write )  \ DUAL DAC 12-bit left aligned data holding  register
DAC $28 + constant DAC_DHR8RD ( read-write )  \ DUAL DAC 8-bit right aligned data holding  register
DAC $2C + constant DAC_DOR1 ( read-only )  \ channel1 data output register
DAC $30 + constant DAC_DOR2 ( read-only )  \ channel2 data output register
DAC $34 + constant DAC_SR ( read-write )  \ status register
[then]

defined? use-PWR defined? PWR not and [if]
$40007000 constant PWR ( Power control ) 
PWR $0 + constant PWR_CR ( read-write )  \ power control register
PWR $4 + constant PWR_CSR (  )  \ power control/status register
[then]

defined? use-I2C3 defined? I2C3 not and [if]
$40005C00 constant I2C3 ( Inter-integrated circuit ) 
I2C3 $0 + constant I2C3_CR1 ( read-write )  \ Control register 1
I2C3 $4 + constant I2C3_CR2 ( read-write )  \ Control register 2
I2C3 $8 + constant I2C3_OAR1 ( read-write )  \ Own address register 1
I2C3 $C + constant I2C3_OAR2 ( read-write )  \ Own address register 2
I2C3 $10 + constant I2C3_DR ( read-write )  \ Data register
I2C3 $14 + constant I2C3_SR1 (  )  \ Status register 1
I2C3 $18 + constant I2C3_SR2 ( read-only )  \ Status register 2
I2C3 $1C + constant I2C3_CCR ( read-write )  \ Clock control register
I2C3 $20 + constant I2C3_TRISE ( read-write )  \ TRISE register
[then]

defined? use-I2C2 defined? I2C2 not and [if]
$40005800 constant I2C2 ( Inter-integrated circuit ) 
I2C2 $0 + constant I2C2_CR1 ( read-write )  \ Control register 1
I2C2 $4 + constant I2C2_CR2 ( read-write )  \ Control register 2
I2C2 $8 + constant I2C2_OAR1 ( read-write )  \ Own address register 1
I2C2 $C + constant I2C2_OAR2 ( read-write )  \ Own address register 2
I2C2 $10 + constant I2C2_DR ( read-write )  \ Data register
I2C2 $14 + constant I2C2_SR1 (  )  \ Status register 1
I2C2 $18 + constant I2C2_SR2 ( read-only )  \ Status register 2
I2C2 $1C + constant I2C2_CCR ( read-write )  \ Clock control register
I2C2 $20 + constant I2C2_TRISE ( read-write )  \ TRISE register
[then]

defined? use-I2C1 defined? I2C1 not and [if]
$40005400 constant I2C1 ( Inter-integrated circuit ) 
I2C1 $0 + constant I2C1_CR1 ( read-write )  \ Control register 1
I2C1 $4 + constant I2C1_CR2 ( read-write )  \ Control register 2
I2C1 $8 + constant I2C1_OAR1 ( read-write )  \ Own address register 1
I2C1 $C + constant I2C1_OAR2 ( read-write )  \ Own address register 2
I2C1 $10 + constant I2C1_DR ( read-write )  \ Data register
I2C1 $14 + constant I2C1_SR1 (  )  \ Status register 1
I2C1 $18 + constant I2C1_SR2 ( read-only )  \ Status register 2
I2C1 $1C + constant I2C1_CCR ( read-write )  \ Clock control register
I2C1 $20 + constant I2C1_TRISE ( read-write )  \ TRISE register
[then]

defined? use-IWDG defined? IWDG not and [if]
$40003000 constant IWDG ( Independent watchdog ) 
IWDG $0 + constant IWDG_KR ( write-only )  \ Key register
IWDG $4 + constant IWDG_PR ( read-write )  \ Prescaler register
IWDG $8 + constant IWDG_RLR ( read-write )  \ Reload register
IWDG $C + constant IWDG_SR ( read-only )  \ Status register
[then]

defined? use-WWDG defined? WWDG not and [if]
$40002C00 constant WWDG ( Window watchdog ) 
WWDG $0 + constant WWDG_CR ( read-write )  \ Control register
WWDG $4 + constant WWDG_CFR ( read-write )  \ Configuration register
WWDG $8 + constant WWDG_SR ( read-write )  \ Status register
[then]

defined? use-RTC defined? RTC not and [if]
$40002800 constant RTC ( Real-time clock ) 
RTC $0 + constant RTC_TR ( read-write )  \ time register
RTC $4 + constant RTC_DR ( read-write )  \ date register
RTC $8 + constant RTC_CR ( read-write )  \ control register
RTC $C + constant RTC_ISR (  )  \ initialization and status  register
RTC $10 + constant RTC_PRER ( read-write )  \ prescaler register
RTC $14 + constant RTC_WUTR ( read-write )  \ wakeup timer register
RTC $18 + constant RTC_CALIBR ( read-write )  \ calibration register
RTC $1C + constant RTC_ALRMAR ( read-write )  \ alarm A register
RTC $20 + constant RTC_ALRMBR ( read-write )  \ alarm B register
RTC $24 + constant RTC_WPR ( write-only )  \ write protection register
RTC $28 + constant RTC_SSR ( read-only )  \ sub second register
RTC $2C + constant RTC_SHIFTR ( write-only )  \ shift control register
RTC $30 + constant RTC_TSTR ( read-only )  \ time stamp time register
RTC $34 + constant RTC_TSDR ( read-only )  \ time stamp date register
RTC $38 + constant RTC_TSSSR ( read-only )  \ timestamp sub second register
RTC $3C + constant RTC_CALR ( read-write )  \ calibration register
RTC $40 + constant RTC_TAFCR ( read-write )  \ tamper and alternate function configuration  register
RTC $44 + constant RTC_ALRMASSR ( read-write )  \ alarm A sub second register
RTC $48 + constant RTC_ALRMBSSR ( read-write )  \ alarm B sub second register
RTC $50 + constant RTC_BKP0R ( read-write )  \ backup register
RTC $54 + constant RTC_BKP1R ( read-write )  \ backup register
RTC $58 + constant RTC_BKP2R ( read-write )  \ backup register
RTC $5C + constant RTC_BKP3R ( read-write )  \ backup register
RTC $60 + constant RTC_BKP4R ( read-write )  \ backup register
RTC $64 + constant RTC_BKP5R ( read-write )  \ backup register
RTC $68 + constant RTC_BKP6R ( read-write )  \ backup register
RTC $6C + constant RTC_BKP7R ( read-write )  \ backup register
RTC $70 + constant RTC_BKP8R ( read-write )  \ backup register
RTC $74 + constant RTC_BKP9R ( read-write )  \ backup register
RTC $78 + constant RTC_BKP10R ( read-write )  \ backup register
RTC $7C + constant RTC_BKP11R ( read-write )  \ backup register
RTC $80 + constant RTC_BKP12R ( read-write )  \ backup register
RTC $84 + constant RTC_BKP13R ( read-write )  \ backup register
RTC $88 + constant RTC_BKP14R ( read-write )  \ backup register
RTC $8C + constant RTC_BKP15R ( read-write )  \ backup register
RTC $90 + constant RTC_BKP16R ( read-write )  \ backup register
RTC $94 + constant RTC_BKP17R ( read-write )  \ backup register
RTC $98 + constant RTC_BKP18R ( read-write )  \ backup register
RTC $9C + constant RTC_BKP19R ( read-write )  \ backup register
[then]

defined? use-UART4 defined? UART4 not and [if]
$40004C00 constant UART4 ( Universal synchronous asynchronous receiver  transmitter ) 
UART4 $0 + constant UART4_SR (  )  \ Status register
UART4 $4 + constant UART4_DR ( read-write )  \ Data register
UART4 $8 + constant UART4_BRR ( read-write )  \ Baud rate register
UART4 $C + constant UART4_CR1 ( read-write )  \ Control register 1
UART4 $10 + constant UART4_CR2 ( read-write )  \ Control register 2
UART4 $14 + constant UART4_CR3 ( read-write )  \ Control register 3
[then]

defined? use-UART5 defined? UART5 not and [if]
$40005000 constant UART5 ( Universal synchronous asynchronous receiver  transmitter ) 
UART5 $0 + constant UART5_SR (  )  \ Status register
UART5 $4 + constant UART5_DR ( read-write )  \ Data register
UART5 $8 + constant UART5_BRR ( read-write )  \ Baud rate register
UART5 $C + constant UART5_CR1 ( read-write )  \ Control register 1
UART5 $10 + constant UART5_CR2 ( read-write )  \ Control register 2
UART5 $14 + constant UART5_CR3 ( read-write )  \ Control register 3
[then]

defined? use-C_ADC defined? C_ADC not and [if]
$40012300 constant C_ADC ( Common ADC registers ) 
C_ADC $0 + constant C_ADC_CSR ( read-only )  \ ADC Common status register
C_ADC $4 + constant C_ADC_CCR ( read-write )  \ ADC common control register
C_ADC $8 + constant C_ADC_CDR ( read-only )  \ ADC common regular data register for dual  and triple modes
[then]

defined? use-TIM1 defined? TIM1 not and [if]
$40010000 constant TIM1 ( Advanced-timers ) 
TIM1 $0 + constant TIM1_CR1 ( read-write )  \ control register 1
TIM1 $4 + constant TIM1_CR2 ( read-write )  \ control register 2
TIM1 $8 + constant TIM1_SMCR ( read-write )  \ slave mode control register
TIM1 $C + constant TIM1_DIER ( read-write )  \ DMA/Interrupt enable register
TIM1 $10 + constant TIM1_SR ( read-write )  \ status register
TIM1 $14 + constant TIM1_EGR ( write-only )  \ event generation register
TIM1 $18 + constant TIM1_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM1 $18 + constant TIM1_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM1 $1C + constant TIM1_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output  mode
TIM1 $1C + constant TIM1_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input  mode
TIM1 $20 + constant TIM1_CCER ( read-write )  \ capture/compare enable  register
TIM1 $24 + constant TIM1_CNT ( read-write )  \ counter
TIM1 $28 + constant TIM1_PSC ( read-write )  \ prescaler
TIM1 $2C + constant TIM1_ARR ( read-write )  \ auto-reload register
TIM1 $34 + constant TIM1_CCR1 ( read-write )  \ capture/compare register 1
TIM1 $38 + constant TIM1_CCR2 ( read-write )  \ capture/compare register 2
TIM1 $3C + constant TIM1_CCR3 ( read-write )  \ capture/compare register 3
TIM1 $40 + constant TIM1_CCR4 ( read-write )  \ capture/compare register 4
TIM1 $48 + constant TIM1_DCR ( read-write )  \ DMA control register
TIM1 $4C + constant TIM1_DMAR ( read-write )  \ DMA address for full transfer
TIM1 $30 + constant TIM1_RCR ( read-write )  \ repetition counter register
TIM1 $44 + constant TIM1_BDTR ( read-write )  \ break and dead-time register
[then]

defined? use-TIM8 defined? TIM8 not and [if]
$40010400 constant TIM8 ( Advanced-timers ) 
TIM8 $0 + constant TIM8_CR1 ( read-write )  \ control register 1
TIM8 $4 + constant TIM8_CR2 ( read-write )  \ control register 2
TIM8 $8 + constant TIM8_SMCR ( read-write )  \ slave mode control register
TIM8 $C + constant TIM8_DIER ( read-write )  \ DMA/Interrupt enable register
TIM8 $10 + constant TIM8_SR ( read-write )  \ status register
TIM8 $14 + constant TIM8_EGR ( write-only )  \ event generation register
TIM8 $18 + constant TIM8_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM8 $18 + constant TIM8_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM8 $1C + constant TIM8_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output  mode
TIM8 $1C + constant TIM8_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input  mode
TIM8 $20 + constant TIM8_CCER ( read-write )  \ capture/compare enable  register
TIM8 $24 + constant TIM8_CNT ( read-write )  \ counter
TIM8 $28 + constant TIM8_PSC ( read-write )  \ prescaler
TIM8 $2C + constant TIM8_ARR ( read-write )  \ auto-reload register
TIM8 $34 + constant TIM8_CCR1 ( read-write )  \ capture/compare register 1
TIM8 $38 + constant TIM8_CCR2 ( read-write )  \ capture/compare register 2
TIM8 $3C + constant TIM8_CCR3 ( read-write )  \ capture/compare register 3
TIM8 $40 + constant TIM8_CCR4 ( read-write )  \ capture/compare register 4
TIM8 $48 + constant TIM8_DCR ( read-write )  \ DMA control register
TIM8 $4C + constant TIM8_DMAR ( read-write )  \ DMA address for full transfer
TIM8 $30 + constant TIM8_RCR ( read-write )  \ repetition counter register
TIM8 $44 + constant TIM8_BDTR ( read-write )  \ break and dead-time register
[then]

defined? use-TIM2 defined? TIM2 not and [if]
$40000000 constant TIM2 ( General purpose timers ) 
TIM2 $0 + constant TIM2_CR1 ( read-write )  \ control register 1
TIM2 $4 + constant TIM2_CR2 ( read-write )  \ control register 2
TIM2 $8 + constant TIM2_SMCR ( read-write )  \ slave mode control register
TIM2 $C + constant TIM2_DIER ( read-write )  \ DMA/Interrupt enable register
TIM2 $10 + constant TIM2_SR ( read-write )  \ status register
TIM2 $14 + constant TIM2_EGR ( write-only )  \ event generation register
TIM2 $18 + constant TIM2_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM2 $18 + constant TIM2_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM2 $1C + constant TIM2_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output  mode
TIM2 $1C + constant TIM2_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input  mode
TIM2 $20 + constant TIM2_CCER ( read-write )  \ capture/compare enable  register
TIM2 $24 + constant TIM2_CNT ( read-write )  \ counter
TIM2 $28 + constant TIM2_PSC ( read-write )  \ prescaler
TIM2 $2C + constant TIM2_ARR ( read-write )  \ auto-reload register
TIM2 $34 + constant TIM2_CCR1 ( read-write )  \ capture/compare register 1
TIM2 $38 + constant TIM2_CCR2 ( read-write )  \ capture/compare register 2
TIM2 $3C + constant TIM2_CCR3 ( read-write )  \ capture/compare register 3
TIM2 $40 + constant TIM2_CCR4 ( read-write )  \ capture/compare register 4
TIM2 $48 + constant TIM2_DCR ( read-write )  \ DMA control register
TIM2 $4C + constant TIM2_DMAR ( read-write )  \ DMA address for full transfer
TIM2 $50 + constant TIM2_OR ( read-write )  \ TIM5 option register
[then]

defined? use-TIM3 defined? TIM3 not and [if]
$40000400 constant TIM3 ( General purpose timers ) 
TIM3 $0 + constant TIM3_CR1 ( read-write )  \ control register 1
TIM3 $4 + constant TIM3_CR2 ( read-write )  \ control register 2
TIM3 $8 + constant TIM3_SMCR ( read-write )  \ slave mode control register
TIM3 $C + constant TIM3_DIER ( read-write )  \ DMA/Interrupt enable register
TIM3 $10 + constant TIM3_SR ( read-write )  \ status register
TIM3 $14 + constant TIM3_EGR ( write-only )  \ event generation register
TIM3 $18 + constant TIM3_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM3 $18 + constant TIM3_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM3 $1C + constant TIM3_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output  mode
TIM3 $1C + constant TIM3_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input  mode
TIM3 $20 + constant TIM3_CCER ( read-write )  \ capture/compare enable  register
TIM3 $24 + constant TIM3_CNT ( read-write )  \ counter
TIM3 $28 + constant TIM3_PSC ( read-write )  \ prescaler
TIM3 $2C + constant TIM3_ARR ( read-write )  \ auto-reload register
TIM3 $34 + constant TIM3_CCR1 ( read-write )  \ capture/compare register 1
TIM3 $38 + constant TIM3_CCR2 ( read-write )  \ capture/compare register 2
TIM3 $3C + constant TIM3_CCR3 ( read-write )  \ capture/compare register 3
TIM3 $40 + constant TIM3_CCR4 ( read-write )  \ capture/compare register 4
TIM3 $48 + constant TIM3_DCR ( read-write )  \ DMA control register
TIM3 $4C + constant TIM3_DMAR ( read-write )  \ DMA address for full transfer
[then]

defined? use-TIM4 defined? TIM4 not and [if]
$40000800 constant TIM4 ( General purpose timers ) 
TIM4 $0 + constant TIM4_CR1 ( read-write )  \ control register 1
TIM4 $4 + constant TIM4_CR2 ( read-write )  \ control register 2
TIM4 $8 + constant TIM4_SMCR ( read-write )  \ slave mode control register
TIM4 $C + constant TIM4_DIER ( read-write )  \ DMA/Interrupt enable register
TIM4 $10 + constant TIM4_SR ( read-write )  \ status register
TIM4 $14 + constant TIM4_EGR ( write-only )  \ event generation register
TIM4 $18 + constant TIM4_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM4 $18 + constant TIM4_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM4 $1C + constant TIM4_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output  mode
TIM4 $1C + constant TIM4_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input  mode
TIM4 $20 + constant TIM4_CCER ( read-write )  \ capture/compare enable  register
TIM4 $24 + constant TIM4_CNT ( read-write )  \ counter
TIM4 $28 + constant TIM4_PSC ( read-write )  \ prescaler
TIM4 $2C + constant TIM4_ARR ( read-write )  \ auto-reload register
TIM4 $34 + constant TIM4_CCR1 ( read-write )  \ capture/compare register 1
TIM4 $38 + constant TIM4_CCR2 ( read-write )  \ capture/compare register 2
TIM4 $3C + constant TIM4_CCR3 ( read-write )  \ capture/compare register 3
TIM4 $40 + constant TIM4_CCR4 ( read-write )  \ capture/compare register 4
TIM4 $48 + constant TIM4_DCR ( read-write )  \ DMA control register
TIM4 $4C + constant TIM4_DMAR ( read-write )  \ DMA address for full transfer
[then]

defined? use-TIM5 defined? TIM5 not and [if]
$40000C00 constant TIM5 ( General-purpose-timers ) 
TIM5 $0 + constant TIM5_CR1 ( read-write )  \ control register 1
TIM5 $4 + constant TIM5_CR2 ( read-write )  \ control register 2
TIM5 $8 + constant TIM5_SMCR ( read-write )  \ slave mode control register
TIM5 $C + constant TIM5_DIER ( read-write )  \ DMA/Interrupt enable register
TIM5 $10 + constant TIM5_SR ( read-write )  \ status register
TIM5 $14 + constant TIM5_EGR ( write-only )  \ event generation register
TIM5 $18 + constant TIM5_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM5 $18 + constant TIM5_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM5 $1C + constant TIM5_CCMR2_Output ( read-write )  \ capture/compare mode register 2 output  mode
TIM5 $1C + constant TIM5_CCMR2_Input ( read-write )  \ capture/compare mode register 2 input  mode
TIM5 $20 + constant TIM5_CCER ( read-write )  \ capture/compare enable  register
TIM5 $24 + constant TIM5_CNT ( read-write )  \ counter
TIM5 $28 + constant TIM5_PSC ( read-write )  \ prescaler
TIM5 $2C + constant TIM5_ARR ( read-write )  \ auto-reload register
TIM5 $34 + constant TIM5_CCR1 ( read-write )  \ capture/compare register 1
TIM5 $38 + constant TIM5_CCR2 ( read-write )  \ capture/compare register 2
TIM5 $3C + constant TIM5_CCR3 ( read-write )  \ capture/compare register 3
TIM5 $40 + constant TIM5_CCR4 ( read-write )  \ capture/compare register 4
TIM5 $48 + constant TIM5_DCR ( read-write )  \ DMA control register
TIM5 $4C + constant TIM5_DMAR ( read-write )  \ DMA address for full transfer
TIM5 $50 + constant TIM5_OR ( read-write )  \ TIM5 option register
[then]

defined? use-TIM9 defined? TIM9 not and [if]
$40014000 constant TIM9 ( General purpose timers ) 
TIM9 $0 + constant TIM9_CR1 ( read-write )  \ control register 1
TIM9 $4 + constant TIM9_CR2 ( read-write )  \ control register 2
TIM9 $8 + constant TIM9_SMCR ( read-write )  \ slave mode control register
TIM9 $C + constant TIM9_DIER ( read-write )  \ DMA/Interrupt enable register
TIM9 $10 + constant TIM9_SR ( read-write )  \ status register
TIM9 $14 + constant TIM9_EGR ( write-only )  \ event generation register
TIM9 $18 + constant TIM9_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM9 $18 + constant TIM9_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM9 $20 + constant TIM9_CCER ( read-write )  \ capture/compare enable  register
TIM9 $24 + constant TIM9_CNT ( read-write )  \ counter
TIM9 $28 + constant TIM9_PSC ( read-write )  \ prescaler
TIM9 $2C + constant TIM9_ARR ( read-write )  \ auto-reload register
TIM9 $34 + constant TIM9_CCR1 ( read-write )  \ capture/compare register 1
TIM9 $38 + constant TIM9_CCR2 ( read-write )  \ capture/compare register 2
[then]

defined? use-TIM12 defined? TIM12 not and [if]
$40001800 constant TIM12 ( General purpose timers ) 
TIM12 $0 + constant TIM12_CR1 ( read-write )  \ control register 1
TIM12 $4 + constant TIM12_CR2 ( read-write )  \ control register 2
TIM12 $8 + constant TIM12_SMCR ( read-write )  \ slave mode control register
TIM12 $C + constant TIM12_DIER ( read-write )  \ DMA/Interrupt enable register
TIM12 $10 + constant TIM12_SR ( read-write )  \ status register
TIM12 $14 + constant TIM12_EGR ( write-only )  \ event generation register
TIM12 $18 + constant TIM12_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM12 $18 + constant TIM12_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM12 $20 + constant TIM12_CCER ( read-write )  \ capture/compare enable  register
TIM12 $24 + constant TIM12_CNT ( read-write )  \ counter
TIM12 $28 + constant TIM12_PSC ( read-write )  \ prescaler
TIM12 $2C + constant TIM12_ARR ( read-write )  \ auto-reload register
TIM12 $34 + constant TIM12_CCR1 ( read-write )  \ capture/compare register 1
TIM12 $38 + constant TIM12_CCR2 ( read-write )  \ capture/compare register 2
[then]

defined? use-TIM10 defined? TIM10 not and [if]
$40014400 constant TIM10 ( General-purpose-timers ) 
TIM10 $0 + constant TIM10_CR1 ( read-write )  \ control register 1
TIM10 $C + constant TIM10_DIER ( read-write )  \ DMA/Interrupt enable register
TIM10 $10 + constant TIM10_SR ( read-write )  \ status register
TIM10 $14 + constant TIM10_EGR ( write-only )  \ event generation register
TIM10 $18 + constant TIM10_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM10 $18 + constant TIM10_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM10 $20 + constant TIM10_CCER ( read-write )  \ capture/compare enable  register
TIM10 $24 + constant TIM10_CNT ( read-write )  \ counter
TIM10 $28 + constant TIM10_PSC ( read-write )  \ prescaler
TIM10 $2C + constant TIM10_ARR ( read-write )  \ auto-reload register
TIM10 $34 + constant TIM10_CCR1 ( read-write )  \ capture/compare register 1
[then]

defined? use-TIM13 defined? TIM13 not and [if]
$40001C00 constant TIM13 ( General-purpose-timers ) 
TIM13 $0 + constant TIM13_CR1 ( read-write )  \ control register 1
TIM13 $C + constant TIM13_DIER ( read-write )  \ DMA/Interrupt enable register
TIM13 $10 + constant TIM13_SR ( read-write )  \ status register
TIM13 $14 + constant TIM13_EGR ( write-only )  \ event generation register
TIM13 $18 + constant TIM13_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM13 $18 + constant TIM13_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM13 $20 + constant TIM13_CCER ( read-write )  \ capture/compare enable  register
TIM13 $24 + constant TIM13_CNT ( read-write )  \ counter
TIM13 $28 + constant TIM13_PSC ( read-write )  \ prescaler
TIM13 $2C + constant TIM13_ARR ( read-write )  \ auto-reload register
TIM13 $34 + constant TIM13_CCR1 ( read-write )  \ capture/compare register 1
[then]

defined? use-TIM14 defined? TIM14 not and [if]
$40002000 constant TIM14 ( General-purpose-timers ) 
TIM14 $0 + constant TIM14_CR1 ( read-write )  \ control register 1
TIM14 $C + constant TIM14_DIER ( read-write )  \ DMA/Interrupt enable register
TIM14 $10 + constant TIM14_SR ( read-write )  \ status register
TIM14 $14 + constant TIM14_EGR ( write-only )  \ event generation register
TIM14 $18 + constant TIM14_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM14 $18 + constant TIM14_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM14 $20 + constant TIM14_CCER ( read-write )  \ capture/compare enable  register
TIM14 $24 + constant TIM14_CNT ( read-write )  \ counter
TIM14 $28 + constant TIM14_PSC ( read-write )  \ prescaler
TIM14 $2C + constant TIM14_ARR ( read-write )  \ auto-reload register
TIM14 $34 + constant TIM14_CCR1 ( read-write )  \ capture/compare register 1
[then]

defined? use-TIM11 defined? TIM11 not and [if]
$40014800 constant TIM11 ( General-purpose-timers ) 
TIM11 $0 + constant TIM11_CR1 ( read-write )  \ control register 1
TIM11 $C + constant TIM11_DIER ( read-write )  \ DMA/Interrupt enable register
TIM11 $10 + constant TIM11_SR ( read-write )  \ status register
TIM11 $14 + constant TIM11_EGR ( write-only )  \ event generation register
TIM11 $18 + constant TIM11_CCMR1_Output ( read-write )  \ capture/compare mode register 1 output  mode
TIM11 $18 + constant TIM11_CCMR1_Input ( read-write )  \ capture/compare mode register 1 input  mode
TIM11 $20 + constant TIM11_CCER ( read-write )  \ capture/compare enable  register
TIM11 $24 + constant TIM11_CNT ( read-write )  \ counter
TIM11 $28 + constant TIM11_PSC ( read-write )  \ prescaler
TIM11 $2C + constant TIM11_ARR ( read-write )  \ auto-reload register
TIM11 $34 + constant TIM11_CCR1 ( read-write )  \ capture/compare register 1
TIM11 $50 + constant TIM11_OR ( read-write )  \ option register
[then]

defined? use-TIM6 defined? TIM6 not and [if]
$40001000 constant TIM6 ( Basic timers ) 
TIM6 $0 + constant TIM6_CR1 ( read-write )  \ control register 1
TIM6 $4 + constant TIM6_CR2 ( read-write )  \ control register 2
TIM6 $C + constant TIM6_DIER ( read-write )  \ DMA/Interrupt enable register
TIM6 $10 + constant TIM6_SR ( read-write )  \ status register
TIM6 $14 + constant TIM6_EGR ( write-only )  \ event generation register
TIM6 $24 + constant TIM6_CNT ( read-write )  \ counter
TIM6 $28 + constant TIM6_PSC ( read-write )  \ prescaler
TIM6 $2C + constant TIM6_ARR ( read-write )  \ auto-reload register
[then]

defined? use-TIM7 defined? TIM7 not and [if]
$40001400 constant TIM7 ( Basic timers ) 
TIM7 $0 + constant TIM7_CR1 ( read-write )  \ control register 1
TIM7 $4 + constant TIM7_CR2 ( read-write )  \ control register 2
TIM7 $C + constant TIM7_DIER ( read-write )  \ DMA/Interrupt enable register
TIM7 $10 + constant TIM7_SR ( read-write )  \ status register
TIM7 $14 + constant TIM7_EGR ( write-only )  \ event generation register
TIM7 $24 + constant TIM7_CNT ( read-write )  \ counter
TIM7 $28 + constant TIM7_PSC ( read-write )  \ prescaler
TIM7 $2C + constant TIM7_ARR ( read-write )  \ auto-reload register
[then]

defined? use-Ethernet_MAC defined? Ethernet_MAC not and [if]
$40028000 constant Ethernet_MAC ( Ethernet: media access control  MAC ) 
Ethernet_MAC $0 + constant Ethernet_MAC_MACCR ( read-write )  \ Ethernet MAC configuration  register
Ethernet_MAC $4 + constant Ethernet_MAC_MACFFR ( read-write )  \ Ethernet MAC frame filter  register
Ethernet_MAC $8 + constant Ethernet_MAC_MACHTHR ( read-write )  \ Ethernet MAC hash table high  register
Ethernet_MAC $C + constant Ethernet_MAC_MACHTLR ( read-write )  \ Ethernet MAC hash table low  register
Ethernet_MAC $10 + constant Ethernet_MAC_MACMIIAR ( read-write )  \ Ethernet MAC MII address  register
Ethernet_MAC $14 + constant Ethernet_MAC_MACMIIDR ( read-write )  \ Ethernet MAC MII data register
Ethernet_MAC $18 + constant Ethernet_MAC_MACFCR ( read-write )  \ Ethernet MAC flow control  register
Ethernet_MAC $1C + constant Ethernet_MAC_MACVLANTR ( read-write )  \ Ethernet MAC VLAN tag register
Ethernet_MAC $2C + constant Ethernet_MAC_MACPMTCSR ( read-write )  \ Ethernet MAC PMT control and status  register
Ethernet_MAC $34 + constant Ethernet_MAC_MACDBGR ( read-only )  \ Ethernet MAC debug register
Ethernet_MAC $38 + constant Ethernet_MAC_MACSR (  )  \ Ethernet MAC interrupt status  register
Ethernet_MAC $3C + constant Ethernet_MAC_MACIMR ( read-write )  \ Ethernet MAC interrupt mask  register
Ethernet_MAC $40 + constant Ethernet_MAC_MACA0HR (  )  \ Ethernet MAC address 0 high  register
Ethernet_MAC $44 + constant Ethernet_MAC_MACA0LR ( read-write )  \ Ethernet MAC address 0 low  register
Ethernet_MAC $48 + constant Ethernet_MAC_MACA1HR ( read-write )  \ Ethernet MAC address 1 high  register
Ethernet_MAC $4C + constant Ethernet_MAC_MACA1LR ( read-write )  \ Ethernet MAC address1 low  register
Ethernet_MAC $50 + constant Ethernet_MAC_MACA2HR ( read-write )  \ Ethernet MAC address 2 high  register
Ethernet_MAC $54 + constant Ethernet_MAC_MACA2LR ( read-write )  \ Ethernet MAC address 2 low  register
Ethernet_MAC $58 + constant Ethernet_MAC_MACA3HR ( read-write )  \ Ethernet MAC address 3 high  register
Ethernet_MAC $5C + constant Ethernet_MAC_MACA3LR ( read-write )  \ Ethernet MAC address 3 low  register
[then]

defined? use-Ethernet_MMC defined? Ethernet_MMC not and [if]
$40028100 constant Ethernet_MMC ( Ethernet: MAC management counters ) 
Ethernet_MMC $0 + constant Ethernet_MMC_MMCCR ( read-write )  \ Ethernet MMC control register
Ethernet_MMC $4 + constant Ethernet_MMC_MMCRIR ( read-write )  \ Ethernet MMC receive interrupt  register
Ethernet_MMC $8 + constant Ethernet_MMC_MMCTIR ( read-only )  \ Ethernet MMC transmit interrupt  register
Ethernet_MMC $C + constant Ethernet_MMC_MMCRIMR ( read-write )  \ Ethernet MMC receive interrupt mask  register
Ethernet_MMC $10 + constant Ethernet_MMC_MMCTIMR ( read-write )  \ Ethernet MMC transmit interrupt mask  register
Ethernet_MMC $4C + constant Ethernet_MMC_MMCTGFSCCR ( read-only )  \ Ethernet MMC transmitted good frames after a  single collision counter
Ethernet_MMC $50 + constant Ethernet_MMC_MMCTGFMSCCR ( read-only )  \ Ethernet MMC transmitted good frames after  more than a single collision
Ethernet_MMC $68 + constant Ethernet_MMC_MMCTGFCR ( read-only )  \ Ethernet MMC transmitted good frames counter  register
Ethernet_MMC $94 + constant Ethernet_MMC_MMCRFCECR ( read-only )  \ Ethernet MMC received frames with CRC error  counter register
Ethernet_MMC $98 + constant Ethernet_MMC_MMCRFAECR ( read-only )  \ Ethernet MMC received frames with alignment  error counter register
Ethernet_MMC $C4 + constant Ethernet_MMC_MMCRGUFCR ( read-only )  \ MMC received good unicast frames counter  register
[then]

defined? use-Ethernet_PTP defined? Ethernet_PTP not and [if]
$40028700 constant Ethernet_PTP ( Ethernet: Precision time protocol ) 
Ethernet_PTP $0 + constant Ethernet_PTP_PTPTSCR ( read-write )  \ Ethernet PTP time stamp control  register
Ethernet_PTP $4 + constant Ethernet_PTP_PTPSSIR ( read-write )  \ Ethernet PTP subsecond increment  register
Ethernet_PTP $8 + constant Ethernet_PTP_PTPTSHR ( read-only )  \ Ethernet PTP time stamp high  register
Ethernet_PTP $C + constant Ethernet_PTP_PTPTSLR ( read-only )  \ Ethernet PTP time stamp low  register
Ethernet_PTP $10 + constant Ethernet_PTP_PTPTSHUR ( read-write )  \ Ethernet PTP time stamp high update  register
Ethernet_PTP $14 + constant Ethernet_PTP_PTPTSLUR ( read-write )  \ Ethernet PTP time stamp low update  register
Ethernet_PTP $18 + constant Ethernet_PTP_PTPTSAR ( read-write )  \ Ethernet PTP time stamp addend  register
Ethernet_PTP $1C + constant Ethernet_PTP_PTPTTHR ( read-write )  \ Ethernet PTP target time high  register
Ethernet_PTP $20 + constant Ethernet_PTP_PTPTTLR ( read-write )  \ Ethernet PTP target time low  register
Ethernet_PTP $28 + constant Ethernet_PTP_PTPTSSR ( read-only )  \ Ethernet PTP time stamp status  register
Ethernet_PTP $2C + constant Ethernet_PTP_PTPPPSCR ( read-only )  \ Ethernet PTP PPS control  register
[then]

defined? use-Ethernet_DMA defined? Ethernet_DMA not and [if]
$40029000 constant Ethernet_DMA ( Ethernet: DMA controller operation ) 
Ethernet_DMA $0 + constant Ethernet_DMA_DMABMR ( read-write )  \ Ethernet DMA bus mode register
Ethernet_DMA $4 + constant Ethernet_DMA_DMATPDR ( read-write )  \ Ethernet DMA transmit poll demand  register
Ethernet_DMA $8 + constant Ethernet_DMA_DMARPDR ( read-write )  \ EHERNET DMA receive poll demand  register
Ethernet_DMA $C + constant Ethernet_DMA_DMARDLAR ( read-write )  \ Ethernet DMA receive descriptor list address  register
Ethernet_DMA $10 + constant Ethernet_DMA_DMATDLAR ( read-write )  \ Ethernet DMA transmit descriptor list  address register
Ethernet_DMA $14 + constant Ethernet_DMA_DMASR (  )  \ Ethernet DMA status register
Ethernet_DMA $18 + constant Ethernet_DMA_DMAOMR ( read-write )  \ Ethernet DMA operation mode  register
Ethernet_DMA $1C + constant Ethernet_DMA_DMAIER ( read-write )  \ Ethernet DMA interrupt enable  register
Ethernet_DMA $20 + constant Ethernet_DMA_DMAMFBOCR ( read-write )  \ Ethernet DMA missed frame and buffer  overflow counter register
Ethernet_DMA $24 + constant Ethernet_DMA_DMARSWTR ( read-write )  \ Ethernet DMA receive status watchdog timer  register
Ethernet_DMA $48 + constant Ethernet_DMA_DMACHTDR ( read-only )  \ Ethernet DMA current host transmit  descriptor register
Ethernet_DMA $4C + constant Ethernet_DMA_DMACHRDR ( read-only )  \ Ethernet DMA current host receive descriptor  register
Ethernet_DMA $50 + constant Ethernet_DMA_DMACHTBAR ( read-only )  \ Ethernet DMA current host transmit buffer  address register
Ethernet_DMA $54 + constant Ethernet_DMA_DMACHRBAR ( read-only )  \ Ethernet DMA current host receive buffer  address register
[then]

defined? use-CRC defined? CRC not and [if]
$40023000 constant CRC ( Cryptographic processor ) 
CRC $0 + constant CRC_DR ( read-write )  \ Data register
CRC $4 + constant CRC_IDR ( read-write )  \ Independent Data register
CRC $8 + constant CRC_CR ( write-only )  \ Control register
[then]

defined? use-OTG_FS_GLOBAL defined? OTG_FS_GLOBAL not and [if]
$50000000 constant OTG_FS_GLOBAL ( USB on the go full speed ) 
OTG_FS_GLOBAL $0 + constant OTG_FS_GLOBAL_FS_GOTGCTL (  )  \ OTG_FS control and status register  OTG_FS_GOTGCTL
OTG_FS_GLOBAL $4 + constant OTG_FS_GLOBAL_FS_GOTGINT ( read-write )  \ OTG_FS interrupt register  OTG_FS_GOTGINT
OTG_FS_GLOBAL $8 + constant OTG_FS_GLOBAL_FS_GAHBCFG ( read-write )  \ OTG_FS AHB configuration register  OTG_FS_GAHBCFG
OTG_FS_GLOBAL $C + constant OTG_FS_GLOBAL_FS_GUSBCFG (  )  \ OTG_FS USB configuration register  OTG_FS_GUSBCFG
OTG_FS_GLOBAL $10 + constant OTG_FS_GLOBAL_FS_GRSTCTL (  )  \ OTG_FS reset register  OTG_FS_GRSTCTL
OTG_FS_GLOBAL $14 + constant OTG_FS_GLOBAL_FS_GINTSTS (  )  \ OTG_FS core interrupt register  OTG_FS_GINTSTS
OTG_FS_GLOBAL $18 + constant OTG_FS_GLOBAL_FS_GINTMSK (  )  \ OTG_FS interrupt mask register  OTG_FS_GINTMSK
OTG_FS_GLOBAL $1C + constant OTG_FS_GLOBAL_FS_GRXSTSR_Device ( read-only )  \ OTG_FS Receive status debug readDevice  mode
OTG_FS_GLOBAL $1C + constant OTG_FS_GLOBAL_FS_GRXSTSR_Host ( read-only )  \ OTG_FS Receive status debug readHost  mode
OTG_FS_GLOBAL $24 + constant OTG_FS_GLOBAL_FS_GRXFSIZ ( read-write )  \ OTG_FS Receive FIFO size register  OTG_FS_GRXFSIZ
OTG_FS_GLOBAL $28 + constant OTG_FS_GLOBAL_FS_GNPTXFSIZ_Device ( read-write )  \ OTG_FS non-periodic transmit FIFO size  register Device mode
OTG_FS_GLOBAL $28 + constant OTG_FS_GLOBAL_FS_GNPTXFSIZ_Host ( read-write )  \ OTG_FS non-periodic transmit FIFO size  register Host mode
OTG_FS_GLOBAL $2C + constant OTG_FS_GLOBAL_FS_GNPTXSTS ( read-only )  \ OTG_FS non-periodic transmit FIFO/queue  status register OTG_FS_GNPTXSTS
OTG_FS_GLOBAL $38 + constant OTG_FS_GLOBAL_FS_GCCFG ( read-write )  \ OTG_FS general core configuration register  OTG_FS_GCCFG
OTG_FS_GLOBAL $3C + constant OTG_FS_GLOBAL_FS_CID ( read-write )  \ core ID register
OTG_FS_GLOBAL $100 + constant OTG_FS_GLOBAL_FS_HPTXFSIZ ( read-write )  \ OTG_FS Host periodic transmit FIFO size  register OTG_FS_HPTXFSIZ
OTG_FS_GLOBAL $104 + constant OTG_FS_GLOBAL_FS_DIEPTXF1 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO size  register OTG_FS_DIEPTXF2
OTG_FS_GLOBAL $108 + constant OTG_FS_GLOBAL_FS_DIEPTXF2 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO size  register OTG_FS_DIEPTXF3
OTG_FS_GLOBAL $10C + constant OTG_FS_GLOBAL_FS_DIEPTXF3 ( read-write )  \ OTG_FS device IN endpoint transmit FIFO size  register OTG_FS_DIEPTXF4
[then]

defined? use-OTG_FS_HOST defined? OTG_FS_HOST not and [if]
$50000400 constant OTG_FS_HOST ( USB on the go full speed ) 
OTG_FS_HOST $0 + constant OTG_FS_HOST_FS_HCFG (  )  \ OTG_FS host configuration register  OTG_FS_HCFG
OTG_FS_HOST $4 + constant OTG_FS_HOST_HFIR ( read-write )  \ OTG_FS Host frame interval  register
OTG_FS_HOST $8 + constant OTG_FS_HOST_FS_HFNUM ( read-only )  \ OTG_FS host frame number/frame time  remaining register OTG_FS_HFNUM
OTG_FS_HOST $10 + constant OTG_FS_HOST_FS_HPTXSTS (  )  \ OTG_FS_Host periodic transmit FIFO/queue  status register OTG_FS_HPTXSTS
OTG_FS_HOST $14 + constant OTG_FS_HOST_HAINT ( read-only )  \ OTG_FS Host all channels interrupt  register
OTG_FS_HOST $18 + constant OTG_FS_HOST_HAINTMSK ( read-write )  \ OTG_FS host all channels interrupt mask  register
OTG_FS_HOST $40 + constant OTG_FS_HOST_FS_HPRT (  )  \ OTG_FS host port control and status register  OTG_FS_HPRT
OTG_FS_HOST $100 + constant OTG_FS_HOST_FS_HCCHAR0 ( read-write )  \ OTG_FS host channel-0 characteristics  register OTG_FS_HCCHAR0
OTG_FS_HOST $120 + constant OTG_FS_HOST_FS_HCCHAR1 ( read-write )  \ OTG_FS host channel-1 characteristics  register OTG_FS_HCCHAR1
OTG_FS_HOST $140 + constant OTG_FS_HOST_FS_HCCHAR2 ( read-write )  \ OTG_FS host channel-2 characteristics  register OTG_FS_HCCHAR2
OTG_FS_HOST $160 + constant OTG_FS_HOST_FS_HCCHAR3 ( read-write )  \ OTG_FS host channel-3 characteristics  register OTG_FS_HCCHAR3
OTG_FS_HOST $180 + constant OTG_FS_HOST_FS_HCCHAR4 ( read-write )  \ OTG_FS host channel-4 characteristics  register OTG_FS_HCCHAR4
OTG_FS_HOST $1A0 + constant OTG_FS_HOST_FS_HCCHAR5 ( read-write )  \ OTG_FS host channel-5 characteristics  register OTG_FS_HCCHAR5
OTG_FS_HOST $1C0 + constant OTG_FS_HOST_FS_HCCHAR6 ( read-write )  \ OTG_FS host channel-6 characteristics  register OTG_FS_HCCHAR6
OTG_FS_HOST $1E0 + constant OTG_FS_HOST_FS_HCCHAR7 ( read-write )  \ OTG_FS host channel-7 characteristics  register OTG_FS_HCCHAR7
OTG_FS_HOST $108 + constant OTG_FS_HOST_FS_HCINT0 ( read-write )  \ OTG_FS host channel-0 interrupt register  OTG_FS_HCINT0
OTG_FS_HOST $128 + constant OTG_FS_HOST_FS_HCINT1 ( read-write )  \ OTG_FS host channel-1 interrupt register  OTG_FS_HCINT1
OTG_FS_HOST $148 + constant OTG_FS_HOST_FS_HCINT2 ( read-write )  \ OTG_FS host channel-2 interrupt register  OTG_FS_HCINT2
OTG_FS_HOST $168 + constant OTG_FS_HOST_FS_HCINT3 ( read-write )  \ OTG_FS host channel-3 interrupt register  OTG_FS_HCINT3
OTG_FS_HOST $188 + constant OTG_FS_HOST_FS_HCINT4 ( read-write )  \ OTG_FS host channel-4 interrupt register  OTG_FS_HCINT4
OTG_FS_HOST $1A8 + constant OTG_FS_HOST_FS_HCINT5 ( read-write )  \ OTG_FS host channel-5 interrupt register  OTG_FS_HCINT5
OTG_FS_HOST $1C8 + constant OTG_FS_HOST_FS_HCINT6 ( read-write )  \ OTG_FS host channel-6 interrupt register  OTG_FS_HCINT6
OTG_FS_HOST $1E8 + constant OTG_FS_HOST_FS_HCINT7 ( read-write )  \ OTG_FS host channel-7 interrupt register  OTG_FS_HCINT7
OTG_FS_HOST $10C + constant OTG_FS_HOST_FS_HCINTMSK0 ( read-write )  \ OTG_FS host channel-0 mask register  OTG_FS_HCINTMSK0
OTG_FS_HOST $12C + constant OTG_FS_HOST_FS_HCINTMSK1 ( read-write )  \ OTG_FS host channel-1 mask register  OTG_FS_HCINTMSK1
OTG_FS_HOST $14C + constant OTG_FS_HOST_FS_HCINTMSK2 ( read-write )  \ OTG_FS host channel-2 mask register  OTG_FS_HCINTMSK2
OTG_FS_HOST $16C + constant OTG_FS_HOST_FS_HCINTMSK3 ( read-write )  \ OTG_FS host channel-3 mask register  OTG_FS_HCINTMSK3
OTG_FS_HOST $18C + constant OTG_FS_HOST_FS_HCINTMSK4 ( read-write )  \ OTG_FS host channel-4 mask register  OTG_FS_HCINTMSK4
OTG_FS_HOST $1AC + constant OTG_FS_HOST_FS_HCINTMSK5 ( read-write )  \ OTG_FS host channel-5 mask register  OTG_FS_HCINTMSK5
OTG_FS_HOST $1CC + constant OTG_FS_HOST_FS_HCINTMSK6 ( read-write )  \ OTG_FS host channel-6 mask register  OTG_FS_HCINTMSK6
OTG_FS_HOST $1EC + constant OTG_FS_HOST_FS_HCINTMSK7 ( read-write )  \ OTG_FS host channel-7 mask register  OTG_FS_HCINTMSK7
OTG_FS_HOST $110 + constant OTG_FS_HOST_FS_HCTSIZ0 ( read-write )  \ OTG_FS host channel-0 transfer size  register
OTG_FS_HOST $130 + constant OTG_FS_HOST_FS_HCTSIZ1 ( read-write )  \ OTG_FS host channel-1 transfer size  register
OTG_FS_HOST $150 + constant OTG_FS_HOST_FS_HCTSIZ2 ( read-write )  \ OTG_FS host channel-2 transfer size  register
OTG_FS_HOST $170 + constant OTG_FS_HOST_FS_HCTSIZ3 ( read-write )  \ OTG_FS host channel-3 transfer size  register
OTG_FS_HOST $190 + constant OTG_FS_HOST_FS_HCTSIZ4 ( read-write )  \ OTG_FS host channel-x transfer size  register
OTG_FS_HOST $1B0 + constant OTG_FS_HOST_FS_HCTSIZ5 ( read-write )  \ OTG_FS host channel-5 transfer size  register
OTG_FS_HOST $1D0 + constant OTG_FS_HOST_FS_HCTSIZ6 ( read-write )  \ OTG_FS host channel-6 transfer size  register
OTG_FS_HOST $1F0 + constant OTG_FS_HOST_FS_HCTSIZ7 ( read-write )  \ OTG_FS host channel-7 transfer size  register
[then]

defined? use-OTG_FS_DEVICE defined? OTG_FS_DEVICE not and [if]
$50000800 constant OTG_FS_DEVICE ( USB on the go full speed ) 
OTG_FS_DEVICE $0 + constant OTG_FS_DEVICE_FS_DCFG ( read-write )  \ OTG_FS device configuration register  OTG_FS_DCFG
OTG_FS_DEVICE $4 + constant OTG_FS_DEVICE_FS_DCTL (  )  \ OTG_FS device control register  OTG_FS_DCTL
OTG_FS_DEVICE $8 + constant OTG_FS_DEVICE_FS_DSTS ( read-only )  \ OTG_FS device status register  OTG_FS_DSTS
OTG_FS_DEVICE $10 + constant OTG_FS_DEVICE_FS_DIEPMSK ( read-write )  \ OTG_FS device IN endpoint common interrupt  mask register OTG_FS_DIEPMSK
OTG_FS_DEVICE $14 + constant OTG_FS_DEVICE_FS_DOEPMSK ( read-write )  \ OTG_FS device OUT endpoint common interrupt  mask register OTG_FS_DOEPMSK
OTG_FS_DEVICE $18 + constant OTG_FS_DEVICE_FS_DAINT ( read-only )  \ OTG_FS device all endpoints interrupt  register OTG_FS_DAINT
OTG_FS_DEVICE $1C + constant OTG_FS_DEVICE_FS_DAINTMSK ( read-write )  \ OTG_FS all endpoints interrupt mask register  OTG_FS_DAINTMSK
OTG_FS_DEVICE $28 + constant OTG_FS_DEVICE_DVBUSDIS ( read-write )  \ OTG_FS device VBUS discharge time  register
OTG_FS_DEVICE $2C + constant OTG_FS_DEVICE_DVBUSPULSE ( read-write )  \ OTG_FS device VBUS pulsing time  register
OTG_FS_DEVICE $34 + constant OTG_FS_DEVICE_DIEPEMPMSK ( read-write )  \ OTG_FS device IN endpoint FIFO empty  interrupt mask register
OTG_FS_DEVICE $100 + constant OTG_FS_DEVICE_FS_DIEPCTL0 (  )  \ OTG_FS device control IN endpoint 0 control  register OTG_FS_DIEPCTL0
OTG_FS_DEVICE $120 + constant OTG_FS_DEVICE_DIEPCTL1 (  )  \ OTG device endpoint-1 control  register
OTG_FS_DEVICE $140 + constant OTG_FS_DEVICE_DIEPCTL2 (  )  \ OTG device endpoint-2 control  register
OTG_FS_DEVICE $160 + constant OTG_FS_DEVICE_DIEPCTL3 (  )  \ OTG device endpoint-3 control  register
OTG_FS_DEVICE $300 + constant OTG_FS_DEVICE_DOEPCTL0 (  )  \ device endpoint-0 control  register
OTG_FS_DEVICE $320 + constant OTG_FS_DEVICE_DOEPCTL1 (  )  \ device endpoint-1 control  register
OTG_FS_DEVICE $340 + constant OTG_FS_DEVICE_DOEPCTL2 (  )  \ device endpoint-2 control  register
OTG_FS_DEVICE $360 + constant OTG_FS_DEVICE_DOEPCTL3 (  )  \ device endpoint-3 control  register
OTG_FS_DEVICE $108 + constant OTG_FS_DEVICE_DIEPINT0 (  )  \ device endpoint-x interrupt  register
OTG_FS_DEVICE $128 + constant OTG_FS_DEVICE_DIEPINT1 (  )  \ device endpoint-1 interrupt  register
OTG_FS_DEVICE $148 + constant OTG_FS_DEVICE_DIEPINT2 (  )  \ device endpoint-2 interrupt  register
OTG_FS_DEVICE $168 + constant OTG_FS_DEVICE_DIEPINT3 (  )  \ device endpoint-3 interrupt  register
OTG_FS_DEVICE $308 + constant OTG_FS_DEVICE_DOEPINT0 ( read-write )  \ device endpoint-0 interrupt  register
OTG_FS_DEVICE $328 + constant OTG_FS_DEVICE_DOEPINT1 ( read-write )  \ device endpoint-1 interrupt  register
OTG_FS_DEVICE $348 + constant OTG_FS_DEVICE_DOEPINT2 ( read-write )  \ device endpoint-2 interrupt  register
OTG_FS_DEVICE $368 + constant OTG_FS_DEVICE_DOEPINT3 ( read-write )  \ device endpoint-3 interrupt  register
OTG_FS_DEVICE $110 + constant OTG_FS_DEVICE_DIEPTSIZ0 ( read-write )  \ device endpoint-0 transfer size  register
OTG_FS_DEVICE $310 + constant OTG_FS_DEVICE_DOEPTSIZ0 ( read-write )  \ device OUT endpoint-0 transfer size  register
OTG_FS_DEVICE $130 + constant OTG_FS_DEVICE_DIEPTSIZ1 ( read-write )  \ device endpoint-1 transfer size  register
OTG_FS_DEVICE $150 + constant OTG_FS_DEVICE_DIEPTSIZ2 ( read-write )  \ device endpoint-2 transfer size  register
OTG_FS_DEVICE $170 + constant OTG_FS_DEVICE_DIEPTSIZ3 ( read-write )  \ device endpoint-3 transfer size  register
OTG_FS_DEVICE $118 + constant OTG_FS_DEVICE_DTXFSTS0 ( read-only )  \ OTG_FS device IN endpoint transmit FIFO  status register
OTG_FS_DEVICE $138 + constant OTG_FS_DEVICE_DTXFSTS1 ( read-only )  \ OTG_FS device IN endpoint transmit FIFO  status register
OTG_FS_DEVICE $158 + constant OTG_FS_DEVICE_DTXFSTS2 ( read-only )  \ OTG_FS device IN endpoint transmit FIFO  status register
OTG_FS_DEVICE $178 + constant OTG_FS_DEVICE_DTXFSTS3 ( read-only )  \ OTG_FS device IN endpoint transmit FIFO  status register
OTG_FS_DEVICE $330 + constant OTG_FS_DEVICE_DOEPTSIZ1 ( read-write )  \ device OUT endpoint-1 transfer size  register
OTG_FS_DEVICE $350 + constant OTG_FS_DEVICE_DOEPTSIZ2 ( read-write )  \ device OUT endpoint-2 transfer size  register
OTG_FS_DEVICE $370 + constant OTG_FS_DEVICE_DOEPTSIZ3 ( read-write )  \ device OUT endpoint-3 transfer size  register
[then]

defined? use-OTG_FS_PWRCLK defined? OTG_FS_PWRCLK not and [if]
$50000E00 constant OTG_FS_PWRCLK ( USB on the go full speed ) 
OTG_FS_PWRCLK $0 + constant OTG_FS_PWRCLK_FS_PCGCCTL ( read-write )  \ OTG_FS power and clock gating control  register
[then]

defined? use-CAN1 defined? CAN1 not and [if]
$40006400 constant CAN1 ( Controller area network ) 
CAN1 $0 + constant CAN1_MCR ( read-write )  \ master control register
CAN1 $4 + constant CAN1_MSR (  )  \ master status register
CAN1 $8 + constant CAN1_TSR (  )  \ transmit status register
CAN1 $C + constant CAN1_RF0R (  )  \ receive FIFO 0 register
CAN1 $10 + constant CAN1_RF1R (  )  \ receive FIFO 1 register
CAN1 $14 + constant CAN1_IER ( read-write )  \ interrupt enable register
CAN1 $18 + constant CAN1_ESR (  )  \ interrupt enable register
CAN1 $1C + constant CAN1_BTR ( read-write )  \ bit timing register
CAN1 $180 + constant CAN1_TI0R ( read-write )  \ TX mailbox identifier register
CAN1 $184 + constant CAN1_TDT0R ( read-write )  \ mailbox data length control and time stamp  register
CAN1 $188 + constant CAN1_TDL0R ( read-write )  \ mailbox data low register
CAN1 $18C + constant CAN1_TDH0R ( read-write )  \ mailbox data high register
CAN1 $190 + constant CAN1_TI1R ( read-write )  \ mailbox identifier register
CAN1 $194 + constant CAN1_TDT1R ( read-write )  \ mailbox data length control and time stamp  register
CAN1 $198 + constant CAN1_TDL1R ( read-write )  \ mailbox data low register
CAN1 $19C + constant CAN1_TDH1R ( read-write )  \ mailbox data high register
CAN1 $1A0 + constant CAN1_TI2R ( read-write )  \ mailbox identifier register
CAN1 $1A4 + constant CAN1_TDT2R ( read-write )  \ mailbox data length control and time stamp  register
CAN1 $1A8 + constant CAN1_TDL2R ( read-write )  \ mailbox data low register
CAN1 $1AC + constant CAN1_TDH2R ( read-write )  \ mailbox data high register
CAN1 $1B0 + constant CAN1_RI0R ( read-only )  \ receive FIFO mailbox identifier  register
CAN1 $1B4 + constant CAN1_RDT0R ( read-only )  \ mailbox data high register
CAN1 $1B8 + constant CAN1_RDL0R ( read-only )  \ mailbox data high register
CAN1 $1BC + constant CAN1_RDH0R ( read-only )  \ receive FIFO mailbox data high  register
CAN1 $1C0 + constant CAN1_RI1R ( read-only )  \ mailbox data high register
CAN1 $1C4 + constant CAN1_RDT1R ( read-only )  \ mailbox data high register
CAN1 $1C8 + constant CAN1_RDL1R ( read-only )  \ mailbox data high register
CAN1 $1CC + constant CAN1_RDH1R ( read-only )  \ mailbox data high register
CAN1 $200 + constant CAN1_FMR ( read-write )  \ filter master register
CAN1 $204 + constant CAN1_FM1R ( read-write )  \ filter mode register
CAN1 $20C + constant CAN1_FS1R ( read-write )  \ filter scale register
CAN1 $214 + constant CAN1_FFA1R ( read-write )  \ filter FIFO assignment  register
CAN1 $21C + constant CAN1_FA1R ( read-write )  \ filter activation register
CAN1 $240 + constant CAN1_F0R1 ( read-write )  \ Filter bank 0 register 1
CAN1 $244 + constant CAN1_F0R2 ( read-write )  \ Filter bank 0 register 2
CAN1 $248 + constant CAN1_F1R1 ( read-write )  \ Filter bank 1 register 1
CAN1 $24C + constant CAN1_F1R2 ( read-write )  \ Filter bank 1 register 2
CAN1 $250 + constant CAN1_F2R1 ( read-write )  \ Filter bank 2 register 1
CAN1 $254 + constant CAN1_F2R2 ( read-write )  \ Filter bank 2 register 2
CAN1 $258 + constant CAN1_F3R1 ( read-write )  \ Filter bank 3 register 1
CAN1 $25C + constant CAN1_F3R2 ( read-write )  \ Filter bank 3 register 2
CAN1 $260 + constant CAN1_F4R1 ( read-write )  \ Filter bank 4 register 1
CAN1 $264 + constant CAN1_F4R2 ( read-write )  \ Filter bank 4 register 2
CAN1 $268 + constant CAN1_F5R1 ( read-write )  \ Filter bank 5 register 1
CAN1 $26C + constant CAN1_F5R2 ( read-write )  \ Filter bank 5 register 2
CAN1 $270 + constant CAN1_F6R1 ( read-write )  \ Filter bank 6 register 1
CAN1 $274 + constant CAN1_F6R2 ( read-write )  \ Filter bank 6 register 2
CAN1 $278 + constant CAN1_F7R1 ( read-write )  \ Filter bank 7 register 1
CAN1 $27C + constant CAN1_F7R2 ( read-write )  \ Filter bank 7 register 2
CAN1 $280 + constant CAN1_F8R1 ( read-write )  \ Filter bank 8 register 1
CAN1 $284 + constant CAN1_F8R2 ( read-write )  \ Filter bank 8 register 2
CAN1 $288 + constant CAN1_F9R1 ( read-write )  \ Filter bank 9 register 1
CAN1 $28C + constant CAN1_F9R2 ( read-write )  \ Filter bank 9 register 2
CAN1 $290 + constant CAN1_F10R1 ( read-write )  \ Filter bank 10 register 1
CAN1 $294 + constant CAN1_F10R2 ( read-write )  \ Filter bank 10 register 2
CAN1 $298 + constant CAN1_F11R1 ( read-write )  \ Filter bank 11 register 1
CAN1 $29C + constant CAN1_F11R2 ( read-write )  \ Filter bank 11 register 2
CAN1 $2A0 + constant CAN1_F12R1 ( read-write )  \ Filter bank 4 register 1
CAN1 $2A4 + constant CAN1_F12R2 ( read-write )  \ Filter bank 12 register 2
CAN1 $2A8 + constant CAN1_F13R1 ( read-write )  \ Filter bank 13 register 1
CAN1 $2AC + constant CAN1_F13R2 ( read-write )  \ Filter bank 13 register 2
CAN1 $2B0 + constant CAN1_F14R1 ( read-write )  \ Filter bank 14 register 1
CAN1 $2B4 + constant CAN1_F14R2 ( read-write )  \ Filter bank 14 register 2
CAN1 $2B8 + constant CAN1_F15R1 ( read-write )  \ Filter bank 15 register 1
CAN1 $2BC + constant CAN1_F15R2 ( read-write )  \ Filter bank 15 register 2
CAN1 $2C0 + constant CAN1_F16R1 ( read-write )  \ Filter bank 16 register 1
CAN1 $2C4 + constant CAN1_F16R2 ( read-write )  \ Filter bank 16 register 2
CAN1 $2C8 + constant CAN1_F17R1 ( read-write )  \ Filter bank 17 register 1
CAN1 $2CC + constant CAN1_F17R2 ( read-write )  \ Filter bank 17 register 2
CAN1 $2D0 + constant CAN1_F18R1 ( read-write )  \ Filter bank 18 register 1
CAN1 $2D4 + constant CAN1_F18R2 ( read-write )  \ Filter bank 18 register 2
CAN1 $2D8 + constant CAN1_F19R1 ( read-write )  \ Filter bank 19 register 1
CAN1 $2DC + constant CAN1_F19R2 ( read-write )  \ Filter bank 19 register 2
CAN1 $2E0 + constant CAN1_F20R1 ( read-write )  \ Filter bank 20 register 1
CAN1 $2E4 + constant CAN1_F20R2 ( read-write )  \ Filter bank 20 register 2
CAN1 $2E8 + constant CAN1_F21R1 ( read-write )  \ Filter bank 21 register 1
CAN1 $2EC + constant CAN1_F21R2 ( read-write )  \ Filter bank 21 register 2
CAN1 $2F0 + constant CAN1_F22R1 ( read-write )  \ Filter bank 22 register 1
CAN1 $2F4 + constant CAN1_F22R2 ( read-write )  \ Filter bank 22 register 2
CAN1 $2F8 + constant CAN1_F23R1 ( read-write )  \ Filter bank 23 register 1
CAN1 $2FC + constant CAN1_F23R2 ( read-write )  \ Filter bank 23 register 2
CAN1 $300 + constant CAN1_F24R1 ( read-write )  \ Filter bank 24 register 1
CAN1 $304 + constant CAN1_F24R2 ( read-write )  \ Filter bank 24 register 2
CAN1 $308 + constant CAN1_F25R1 ( read-write )  \ Filter bank 25 register 1
CAN1 $30C + constant CAN1_F25R2 ( read-write )  \ Filter bank 25 register 2
CAN1 $310 + constant CAN1_F26R1 ( read-write )  \ Filter bank 26 register 1
CAN1 $314 + constant CAN1_F26R2 ( read-write )  \ Filter bank 26 register 2
CAN1 $318 + constant CAN1_F27R1 ( read-write )  \ Filter bank 27 register 1
CAN1 $31C + constant CAN1_F27R2 ( read-write )  \ Filter bank 27 register 2
[then]

defined? use-CAN2 defined? CAN2 not and [if]
$40006800 constant CAN2 ( Controller area network ) 
CAN2 $0 + constant CAN2_MCR ( read-write )  \ master control register
CAN2 $4 + constant CAN2_MSR (  )  \ master status register
CAN2 $8 + constant CAN2_TSR (  )  \ transmit status register
CAN2 $C + constant CAN2_RF0R (  )  \ receive FIFO 0 register
CAN2 $10 + constant CAN2_RF1R (  )  \ receive FIFO 1 register
CAN2 $14 + constant CAN2_IER ( read-write )  \ interrupt enable register
CAN2 $18 + constant CAN2_ESR (  )  \ interrupt enable register
CAN2 $1C + constant CAN2_BTR ( read-write )  \ bit timing register
CAN2 $180 + constant CAN2_TI0R ( read-write )  \ TX mailbox identifier register
CAN2 $184 + constant CAN2_TDT0R ( read-write )  \ mailbox data length control and time stamp  register
CAN2 $188 + constant CAN2_TDL0R ( read-write )  \ mailbox data low register
CAN2 $18C + constant CAN2_TDH0R ( read-write )  \ mailbox data high register
CAN2 $190 + constant CAN2_TI1R ( read-write )  \ mailbox identifier register
CAN2 $194 + constant CAN2_TDT1R ( read-write )  \ mailbox data length control and time stamp  register
CAN2 $198 + constant CAN2_TDL1R ( read-write )  \ mailbox data low register
CAN2 $19C + constant CAN2_TDH1R ( read-write )  \ mailbox data high register
CAN2 $1A0 + constant CAN2_TI2R ( read-write )  \ mailbox identifier register
CAN2 $1A4 + constant CAN2_TDT2R ( read-write )  \ mailbox data length control and time stamp  register
CAN2 $1A8 + constant CAN2_TDL2R ( read-write )  \ mailbox data low register
CAN2 $1AC + constant CAN2_TDH2R ( read-write )  \ mailbox data high register
CAN2 $1B0 + constant CAN2_RI0R ( read-only )  \ receive FIFO mailbox identifier  register
CAN2 $1B4 + constant CAN2_RDT0R ( read-only )  \ mailbox data high register
CAN2 $1B8 + constant CAN2_RDL0R ( read-only )  \ mailbox data high register
CAN2 $1BC + constant CAN2_RDH0R ( read-only )  \ receive FIFO mailbox data high  register
CAN2 $1C0 + constant CAN2_RI1R ( read-only )  \ mailbox data high register
CAN2 $1C4 + constant CAN2_RDT1R ( read-only )  \ mailbox data high register
CAN2 $1C8 + constant CAN2_RDL1R ( read-only )  \ mailbox data high register
CAN2 $1CC + constant CAN2_RDH1R ( read-only )  \ mailbox data high register
CAN2 $200 + constant CAN2_FMR ( read-write )  \ filter master register
CAN2 $204 + constant CAN2_FM1R ( read-write )  \ filter mode register
CAN2 $20C + constant CAN2_FS1R ( read-write )  \ filter scale register
CAN2 $214 + constant CAN2_FFA1R ( read-write )  \ filter FIFO assignment  register
CAN2 $21C + constant CAN2_FA1R ( read-write )  \ filter activation register
CAN2 $240 + constant CAN2_F0R1 ( read-write )  \ Filter bank 0 register 1
CAN2 $244 + constant CAN2_F0R2 ( read-write )  \ Filter bank 0 register 2
CAN2 $248 + constant CAN2_F1R1 ( read-write )  \ Filter bank 1 register 1
CAN2 $24C + constant CAN2_F1R2 ( read-write )  \ Filter bank 1 register 2
CAN2 $250 + constant CAN2_F2R1 ( read-write )  \ Filter bank 2 register 1
CAN2 $254 + constant CAN2_F2R2 ( read-write )  \ Filter bank 2 register 2
CAN2 $258 + constant CAN2_F3R1 ( read-write )  \ Filter bank 3 register 1
CAN2 $25C + constant CAN2_F3R2 ( read-write )  \ Filter bank 3 register 2
CAN2 $260 + constant CAN2_F4R1 ( read-write )  \ Filter bank 4 register 1
CAN2 $264 + constant CAN2_F4R2 ( read-write )  \ Filter bank 4 register 2
CAN2 $268 + constant CAN2_F5R1 ( read-write )  \ Filter bank 5 register 1
CAN2 $26C + constant CAN2_F5R2 ( read-write )  \ Filter bank 5 register 2
CAN2 $270 + constant CAN2_F6R1 ( read-write )  \ Filter bank 6 register 1
CAN2 $274 + constant CAN2_F6R2 ( read-write )  \ Filter bank 6 register 2
CAN2 $278 + constant CAN2_F7R1 ( read-write )  \ Filter bank 7 register 1
CAN2 $27C + constant CAN2_F7R2 ( read-write )  \ Filter bank 7 register 2
CAN2 $280 + constant CAN2_F8R1 ( read-write )  \ Filter bank 8 register 1
CAN2 $284 + constant CAN2_F8R2 ( read-write )  \ Filter bank 8 register 2
CAN2 $288 + constant CAN2_F9R1 ( read-write )  \ Filter bank 9 register 1
CAN2 $28C + constant CAN2_F9R2 ( read-write )  \ Filter bank 9 register 2
CAN2 $290 + constant CAN2_F10R1 ( read-write )  \ Filter bank 10 register 1
CAN2 $294 + constant CAN2_F10R2 ( read-write )  \ Filter bank 10 register 2
CAN2 $298 + constant CAN2_F11R1 ( read-write )  \ Filter bank 11 register 1
CAN2 $29C + constant CAN2_F11R2 ( read-write )  \ Filter bank 11 register 2
CAN2 $2A0 + constant CAN2_F12R1 ( read-write )  \ Filter bank 4 register 1
CAN2 $2A4 + constant CAN2_F12R2 ( read-write )  \ Filter bank 12 register 2
CAN2 $2A8 + constant CAN2_F13R1 ( read-write )  \ Filter bank 13 register 1
CAN2 $2AC + constant CAN2_F13R2 ( read-write )  \ Filter bank 13 register 2
CAN2 $2B0 + constant CAN2_F14R1 ( read-write )  \ Filter bank 14 register 1
CAN2 $2B4 + constant CAN2_F14R2 ( read-write )  \ Filter bank 14 register 2
CAN2 $2B8 + constant CAN2_F15R1 ( read-write )  \ Filter bank 15 register 1
CAN2 $2BC + constant CAN2_F15R2 ( read-write )  \ Filter bank 15 register 2
CAN2 $2C0 + constant CAN2_F16R1 ( read-write )  \ Filter bank 16 register 1
CAN2 $2C4 + constant CAN2_F16R2 ( read-write )  \ Filter bank 16 register 2
CAN2 $2C8 + constant CAN2_F17R1 ( read-write )  \ Filter bank 17 register 1
CAN2 $2CC + constant CAN2_F17R2 ( read-write )  \ Filter bank 17 register 2
CAN2 $2D0 + constant CAN2_F18R1 ( read-write )  \ Filter bank 18 register 1
CAN2 $2D4 + constant CAN2_F18R2 ( read-write )  \ Filter bank 18 register 2
CAN2 $2D8 + constant CAN2_F19R1 ( read-write )  \ Filter bank 19 register 1
CAN2 $2DC + constant CAN2_F19R2 ( read-write )  \ Filter bank 19 register 2
CAN2 $2E0 + constant CAN2_F20R1 ( read-write )  \ Filter bank 20 register 1
CAN2 $2E4 + constant CAN2_F20R2 ( read-write )  \ Filter bank 20 register 2
CAN2 $2E8 + constant CAN2_F21R1 ( read-write )  \ Filter bank 21 register 1
CAN2 $2EC + constant CAN2_F21R2 ( read-write )  \ Filter bank 21 register 2
CAN2 $2F0 + constant CAN2_F22R1 ( read-write )  \ Filter bank 22 register 1
CAN2 $2F4 + constant CAN2_F22R2 ( read-write )  \ Filter bank 22 register 2
CAN2 $2F8 + constant CAN2_F23R1 ( read-write )  \ Filter bank 23 register 1
CAN2 $2FC + constant CAN2_F23R2 ( read-write )  \ Filter bank 23 register 2
CAN2 $300 + constant CAN2_F24R1 ( read-write )  \ Filter bank 24 register 1
CAN2 $304 + constant CAN2_F24R2 ( read-write )  \ Filter bank 24 register 2
CAN2 $308 + constant CAN2_F25R1 ( read-write )  \ Filter bank 25 register 1
CAN2 $30C + constant CAN2_F25R2 ( read-write )  \ Filter bank 25 register 2
CAN2 $310 + constant CAN2_F26R1 ( read-write )  \ Filter bank 26 register 1
CAN2 $314 + constant CAN2_F26R2 ( read-write )  \ Filter bank 26 register 2
CAN2 $318 + constant CAN2_F27R1 ( read-write )  \ Filter bank 27 register 1
CAN2 $31C + constant CAN2_F27R2 ( read-write )  \ Filter bank 27 register 2
[then]

defined? use-FLASH defined? FLASH not and [if]
$40023C00 constant FLASH ( FLASH ) 
FLASH $0 + constant FLASH_ACR (  )  \ Flash access control register
FLASH $4 + constant FLASH_KEYR ( write-only )  \ Flash key register
FLASH $8 + constant FLASH_OPTKEYR ( write-only )  \ Flash option key register
FLASH $C + constant FLASH_SR (  )  \ Status register
FLASH $10 + constant FLASH_CR ( read-write )  \ Control register
FLASH $14 + constant FLASH_OPTCR ( read-write )  \ Flash option control register
[then]

defined? use-EXTI defined? EXTI not and [if]
$40013C00 constant EXTI ( External interrupt/event  controller ) 
EXTI $0 + constant EXTI_IMR ( read-write )  \ Interrupt mask register  EXTI_IMR
EXTI $4 + constant EXTI_EMR ( read-write )  \ Event mask register EXTI_EMR
EXTI $8 + constant EXTI_RTSR ( read-write )  \ Rising Trigger selection register  EXTI_RTSR
EXTI $C + constant EXTI_FTSR ( read-write )  \ Falling Trigger selection register  EXTI_FTSR
EXTI $10 + constant EXTI_SWIER ( read-write )  \ Software interrupt event register  EXTI_SWIER
EXTI $14 + constant EXTI_PR ( read-write )  \ Pending register EXTI_PR
[then]

defined? use-OTG_HS_GLOBAL defined? OTG_HS_GLOBAL not and [if]
$40040000 constant OTG_HS_GLOBAL ( USB on the go high speed ) 
OTG_HS_GLOBAL $0 + constant OTG_HS_GLOBAL_OTG_HS_GOTGCTL (  )  \ OTG_HS control and status  register
OTG_HS_GLOBAL $4 + constant OTG_HS_GLOBAL_OTG_HS_GOTGINT ( read-write )  \ OTG_HS interrupt register
OTG_HS_GLOBAL $8 + constant OTG_HS_GLOBAL_OTG_HS_GAHBCFG ( read-write )  \ OTG_HS AHB configuration  register
OTG_HS_GLOBAL $C + constant OTG_HS_GLOBAL_OTG_HS_GUSBCFG (  )  \ OTG_HS USB configuration  register
OTG_HS_GLOBAL $10 + constant OTG_HS_GLOBAL_OTG_HS_GRSTCTL (  )  \ OTG_HS reset register
OTG_HS_GLOBAL $14 + constant OTG_HS_GLOBAL_OTG_HS_GINTSTS (  )  \ OTG_HS core interrupt register
OTG_HS_GLOBAL $18 + constant OTG_HS_GLOBAL_OTG_HS_GINTMSK (  )  \ OTG_HS interrupt mask register
OTG_HS_GLOBAL $1C + constant OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Host ( read-only )  \ OTG_HS Receive status debug read register  host mode
OTG_HS_GLOBAL $20 + constant OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Host ( read-only )  \ OTG_HS status read and pop register host  mode
OTG_HS_GLOBAL $24 + constant OTG_HS_GLOBAL_OTG_HS_GRXFSIZ ( read-write )  \ OTG_HS Receive FIFO size  register
OTG_HS_GLOBAL $28 + constant OTG_HS_GLOBAL_OTG_HS_GNPTXFSIZ_Host ( read-write )  \ OTG_HS nonperiodic transmit FIFO size  register host mode
OTG_HS_GLOBAL $28 + constant OTG_HS_GLOBAL_OTG_HS_TX0FSIZ_Peripheral ( read-write )  \ Endpoint 0 transmit FIFO size peripheral  mode
OTG_HS_GLOBAL $2C + constant OTG_HS_GLOBAL_OTG_HS_GNPTXSTS ( read-only )  \ OTG_HS nonperiodic transmit FIFO/queue  status register
OTG_HS_GLOBAL $38 + constant OTG_HS_GLOBAL_OTG_HS_GCCFG ( read-write )  \ OTG_HS general core configuration  register
OTG_HS_GLOBAL $3C + constant OTG_HS_GLOBAL_OTG_HS_CID ( read-write )  \ OTG_HS core ID register
OTG_HS_GLOBAL $100 + constant OTG_HS_GLOBAL_OTG_HS_HPTXFSIZ ( read-write )  \ OTG_HS Host periodic transmit FIFO size  register
OTG_HS_GLOBAL $104 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF1 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size  register
OTG_HS_GLOBAL $108 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF2 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size  register
OTG_HS_GLOBAL $11C + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF3 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size  register
OTG_HS_GLOBAL $120 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF4 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size  register
OTG_HS_GLOBAL $124 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF5 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size  register
OTG_HS_GLOBAL $128 + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF6 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size  register
OTG_HS_GLOBAL $12C + constant OTG_HS_GLOBAL_OTG_HS_DIEPTXF7 ( read-write )  \ OTG_HS device IN endpoint transmit FIFO size  register
OTG_HS_GLOBAL $1C + constant OTG_HS_GLOBAL_OTG_HS_GRXSTSR_Peripheral ( read-only )  \ OTG_HS Receive status debug read register  peripheral mode mode
OTG_HS_GLOBAL $20 + constant OTG_HS_GLOBAL_OTG_HS_GRXSTSP_Peripheral ( read-only )  \ OTG_HS status read and pop register  peripheral mode
[then]

defined? use-OTG_HS_HOST defined? OTG_HS_HOST not and [if]
$40040400 constant OTG_HS_HOST ( USB on the go high speed ) 
OTG_HS_HOST $0 + constant OTG_HS_HOST_OTG_HS_HCFG (  )  \ OTG_HS host configuration  register
OTG_HS_HOST $4 + constant OTG_HS_HOST_OTG_HS_HFIR ( read-write )  \ OTG_HS Host frame interval  register
OTG_HS_HOST $8 + constant OTG_HS_HOST_OTG_HS_HFNUM ( read-only )  \ OTG_HS host frame number/frame time  remaining register
OTG_HS_HOST $10 + constant OTG_HS_HOST_OTG_HS_HPTXSTS (  )  \ OTG_HS_Host periodic transmit FIFO/queue  status register
OTG_HS_HOST $14 + constant OTG_HS_HOST_OTG_HS_HAINT ( read-only )  \ OTG_HS Host all channels interrupt  register
OTG_HS_HOST $18 + constant OTG_HS_HOST_OTG_HS_HAINTMSK ( read-write )  \ OTG_HS host all channels interrupt mask  register
OTG_HS_HOST $40 + constant OTG_HS_HOST_OTG_HS_HPRT (  )  \ OTG_HS host port control and status  register
OTG_HS_HOST $100 + constant OTG_HS_HOST_OTG_HS_HCCHAR0 ( read-write )  \ OTG_HS host channel-0 characteristics  register
OTG_HS_HOST $120 + constant OTG_HS_HOST_OTG_HS_HCCHAR1 ( read-write )  \ OTG_HS host channel-1 characteristics  register
OTG_HS_HOST $140 + constant OTG_HS_HOST_OTG_HS_HCCHAR2 ( read-write )  \ OTG_HS host channel-2 characteristics  register
OTG_HS_HOST $160 + constant OTG_HS_HOST_OTG_HS_HCCHAR3 ( read-write )  \ OTG_HS host channel-3 characteristics  register
OTG_HS_HOST $180 + constant OTG_HS_HOST_OTG_HS_HCCHAR4 ( read-write )  \ OTG_HS host channel-4 characteristics  register
OTG_HS_HOST $1A0 + constant OTG_HS_HOST_OTG_HS_HCCHAR5 ( read-write )  \ OTG_HS host channel-5 characteristics  register
OTG_HS_HOST $1C0 + constant OTG_HS_HOST_OTG_HS_HCCHAR6 ( read-write )  \ OTG_HS host channel-6 characteristics  register
OTG_HS_HOST $1E0 + constant OTG_HS_HOST_OTG_HS_HCCHAR7 ( read-write )  \ OTG_HS host channel-7 characteristics  register
OTG_HS_HOST $200 + constant OTG_HS_HOST_OTG_HS_HCCHAR8 ( read-write )  \ OTG_HS host channel-8 characteristics  register
OTG_HS_HOST $220 + constant OTG_HS_HOST_OTG_HS_HCCHAR9 ( read-write )  \ OTG_HS host channel-9 characteristics  register
OTG_HS_HOST $240 + constant OTG_HS_HOST_OTG_HS_HCCHAR10 ( read-write )  \ OTG_HS host channel-10 characteristics  register
OTG_HS_HOST $260 + constant OTG_HS_HOST_OTG_HS_HCCHAR11 ( read-write )  \ OTG_HS host channel-11 characteristics  register
OTG_HS_HOST $104 + constant OTG_HS_HOST_OTG_HS_HCSPLT0 ( read-write )  \ OTG_HS host channel-0 split control  register
OTG_HS_HOST $124 + constant OTG_HS_HOST_OTG_HS_HCSPLT1 ( read-write )  \ OTG_HS host channel-1 split control  register
OTG_HS_HOST $144 + constant OTG_HS_HOST_OTG_HS_HCSPLT2 ( read-write )  \ OTG_HS host channel-2 split control  register
OTG_HS_HOST $164 + constant OTG_HS_HOST_OTG_HS_HCSPLT3 ( read-write )  \ OTG_HS host channel-3 split control  register
OTG_HS_HOST $184 + constant OTG_HS_HOST_OTG_HS_HCSPLT4 ( read-write )  \ OTG_HS host channel-4 split control  register
OTG_HS_HOST $1A4 + constant OTG_HS_HOST_OTG_HS_HCSPLT5 ( read-write )  \ OTG_HS host channel-5 split control  register
OTG_HS_HOST $1C4 + constant OTG_HS_HOST_OTG_HS_HCSPLT6 ( read-write )  \ OTG_HS host channel-6 split control  register
OTG_HS_HOST $1E4 + constant OTG_HS_HOST_OTG_HS_HCSPLT7 ( read-write )  \ OTG_HS host channel-7 split control  register
OTG_HS_HOST $204 + constant OTG_HS_HOST_OTG_HS_HCSPLT8 ( read-write )  \ OTG_HS host channel-8 split control  register
OTG_HS_HOST $224 + constant OTG_HS_HOST_OTG_HS_HCSPLT9 ( read-write )  \ OTG_HS host channel-9 split control  register
OTG_HS_HOST $244 + constant OTG_HS_HOST_OTG_HS_HCSPLT10 ( read-write )  \ OTG_HS host channel-10 split control  register
OTG_HS_HOST $264 + constant OTG_HS_HOST_OTG_HS_HCSPLT11 ( read-write )  \ OTG_HS host channel-11 split control  register
OTG_HS_HOST $108 + constant OTG_HS_HOST_OTG_HS_HCINT0 ( read-write )  \ OTG_HS host channel-11 interrupt  register
OTG_HS_HOST $128 + constant OTG_HS_HOST_OTG_HS_HCINT1 ( read-write )  \ OTG_HS host channel-1 interrupt  register
OTG_HS_HOST $148 + constant OTG_HS_HOST_OTG_HS_HCINT2 ( read-write )  \ OTG_HS host channel-2 interrupt  register
OTG_HS_HOST $168 + constant OTG_HS_HOST_OTG_HS_HCINT3 ( read-write )  \ OTG_HS host channel-3 interrupt  register
OTG_HS_HOST $188 + constant OTG_HS_HOST_OTG_HS_HCINT4 ( read-write )  \ OTG_HS host channel-4 interrupt  register
OTG_HS_HOST $1A8 + constant OTG_HS_HOST_OTG_HS_HCINT5 ( read-write )  \ OTG_HS host channel-5 interrupt  register
OTG_HS_HOST $1C8 + constant OTG_HS_HOST_OTG_HS_HCINT6 ( read-write )  \ OTG_HS host channel-6 interrupt  register
OTG_HS_HOST $1E8 + constant OTG_HS_HOST_OTG_HS_HCINT7 ( read-write )  \ OTG_HS host channel-7 interrupt  register
OTG_HS_HOST $208 + constant OTG_HS_HOST_OTG_HS_HCINT8 ( read-write )  \ OTG_HS host channel-8 interrupt  register
OTG_HS_HOST $228 + constant OTG_HS_HOST_OTG_HS_HCINT9 ( read-write )  \ OTG_HS host channel-9 interrupt  register
OTG_HS_HOST $248 + constant OTG_HS_HOST_OTG_HS_HCINT10 ( read-write )  \ OTG_HS host channel-10 interrupt  register
OTG_HS_HOST $268 + constant OTG_HS_HOST_OTG_HS_HCINT11 ( read-write )  \ OTG_HS host channel-11 interrupt  register
OTG_HS_HOST $10C + constant OTG_HS_HOST_OTG_HS_HCINTMSK0 ( read-write )  \ OTG_HS host channel-11 interrupt mask  register
OTG_HS_HOST $12C + constant OTG_HS_HOST_OTG_HS_HCINTMSK1 ( read-write )  \ OTG_HS host channel-1 interrupt mask  register
OTG_HS_HOST $14C + constant OTG_HS_HOST_OTG_HS_HCINTMSK2 ( read-write )  \ OTG_HS host channel-2 interrupt mask  register
OTG_HS_HOST $16C + constant OTG_HS_HOST_OTG_HS_HCINTMSK3 ( read-write )  \ OTG_HS host channel-3 interrupt mask  register
OTG_HS_HOST $18C + constant OTG_HS_HOST_OTG_HS_HCINTMSK4 ( read-write )  \ OTG_HS host channel-4 interrupt mask  register
OTG_HS_HOST $1AC + constant OTG_HS_HOST_OTG_HS_HCINTMSK5 ( read-write )  \ OTG_HS host channel-5 interrupt mask  register
OTG_HS_HOST $1CC + constant OTG_HS_HOST_OTG_HS_HCINTMSK6 ( read-write )  \ OTG_HS host channel-6 interrupt mask  register
OTG_HS_HOST $1EC + constant OTG_HS_HOST_OTG_HS_HCINTMSK7 ( read-write )  \ OTG_HS host channel-7 interrupt mask  register
OTG_HS_HOST $20C + constant OTG_HS_HOST_OTG_HS_HCINTMSK8 ( read-write )  \ OTG_HS host channel-8 interrupt mask  register
OTG_HS_HOST $22C + constant OTG_HS_HOST_OTG_HS_HCINTMSK9 ( read-write )  \ OTG_HS host channel-9 interrupt mask  register
OTG_HS_HOST $24C + constant OTG_HS_HOST_OTG_HS_HCINTMSK10 ( read-write )  \ OTG_HS host channel-10 interrupt mask  register
OTG_HS_HOST $26C + constant OTG_HS_HOST_OTG_HS_HCINTMSK11 ( read-write )  \ OTG_HS host channel-11 interrupt mask  register
OTG_HS_HOST $110 + constant OTG_HS_HOST_OTG_HS_HCTSIZ0 ( read-write )  \ OTG_HS host channel-11 transfer size  register
OTG_HS_HOST $130 + constant OTG_HS_HOST_OTG_HS_HCTSIZ1 ( read-write )  \ OTG_HS host channel-1 transfer size  register
OTG_HS_HOST $150 + constant OTG_HS_HOST_OTG_HS_HCTSIZ2 ( read-write )  \ OTG_HS host channel-2 transfer size  register
OTG_HS_HOST $170 + constant OTG_HS_HOST_OTG_HS_HCTSIZ3 ( read-write )  \ OTG_HS host channel-3 transfer size  register
OTG_HS_HOST $190 + constant OTG_HS_HOST_OTG_HS_HCTSIZ4 ( read-write )  \ OTG_HS host channel-4 transfer size  register
OTG_HS_HOST $1B0 + constant OTG_HS_HOST_OTG_HS_HCTSIZ5 ( read-write )  \ OTG_HS host channel-5 transfer size  register
OTG_HS_HOST $1D0 + constant OTG_HS_HOST_OTG_HS_HCTSIZ6 ( read-write )  \ OTG_HS host channel-6 transfer size  register
OTG_HS_HOST $1F0 + constant OTG_HS_HOST_OTG_HS_HCTSIZ7 ( read-write )  \ OTG_HS host channel-7 transfer size  register
OTG_HS_HOST $210 + constant OTG_HS_HOST_OTG_HS_HCTSIZ8 ( read-write )  \ OTG_HS host channel-8 transfer size  register
OTG_HS_HOST $230 + constant OTG_HS_HOST_OTG_HS_HCTSIZ9 ( read-write )  \ OTG_HS host channel-9 transfer size  register
OTG_HS_HOST $250 + constant OTG_HS_HOST_OTG_HS_HCTSIZ10 ( read-write )  \ OTG_HS host channel-10 transfer size  register
OTG_HS_HOST $270 + constant OTG_HS_HOST_OTG_HS_HCTSIZ11 ( read-write )  \ OTG_HS host channel-11 transfer size  register
OTG_HS_HOST $114 + constant OTG_HS_HOST_OTG_HS_HCDMA0 ( read-write )  \ OTG_HS host channel-0 DMA address  register
OTG_HS_HOST $134 + constant OTG_HS_HOST_OTG_HS_HCDMA1 ( read-write )  \ OTG_HS host channel-1 DMA address  register
OTG_HS_HOST $154 + constant OTG_HS_HOST_OTG_HS_HCDMA2 ( read-write )  \ OTG_HS host channel-2 DMA address  register
OTG_HS_HOST $174 + constant OTG_HS_HOST_OTG_HS_HCDMA3 ( read-write )  \ OTG_HS host channel-3 DMA address  register
OTG_HS_HOST $194 + constant OTG_HS_HOST_OTG_HS_HCDMA4 ( read-write )  \ OTG_HS host channel-4 DMA address  register
OTG_HS_HOST $1B4 + constant OTG_HS_HOST_OTG_HS_HCDMA5 ( read-write )  \ OTG_HS host channel-5 DMA address  register
OTG_HS_HOST $1D4 + constant OTG_HS_HOST_OTG_HS_HCDMA6 ( read-write )  \ OTG_HS host channel-6 DMA address  register
OTG_HS_HOST $1F4 + constant OTG_HS_HOST_OTG_HS_HCDMA7 ( read-write )  \ OTG_HS host channel-7 DMA address  register
OTG_HS_HOST $214 + constant OTG_HS_HOST_OTG_HS_HCDMA8 ( read-write )  \ OTG_HS host channel-8 DMA address  register
OTG_HS_HOST $234 + constant OTG_HS_HOST_OTG_HS_HCDMA9 ( read-write )  \ OTG_HS host channel-9 DMA address  register
OTG_HS_HOST $254 + constant OTG_HS_HOST_OTG_HS_HCDMA10 ( read-write )  \ OTG_HS host channel-10 DMA address  register
OTG_HS_HOST $274 + constant OTG_HS_HOST_OTG_HS_HCDMA11 ( read-write )  \ OTG_HS host channel-11 DMA address  register
[then]

defined? use-OTG_HS_DEVICE defined? OTG_HS_DEVICE not and [if]
$40040800 constant OTG_HS_DEVICE ( USB on the go high speed ) 
OTG_HS_DEVICE $0 + constant OTG_HS_DEVICE_OTG_HS_DCFG ( read-write )  \ OTG_HS device configuration  register
OTG_HS_DEVICE $4 + constant OTG_HS_DEVICE_OTG_HS_DCTL (  )  \ OTG_HS device control register
OTG_HS_DEVICE $8 + constant OTG_HS_DEVICE_OTG_HS_DSTS ( read-only )  \ OTG_HS device status register
OTG_HS_DEVICE $10 + constant OTG_HS_DEVICE_OTG_HS_DIEPMSK ( read-write )  \ OTG_HS device IN endpoint common interrupt  mask register
OTG_HS_DEVICE $14 + constant OTG_HS_DEVICE_OTG_HS_DOEPMSK ( read-write )  \ OTG_HS device OUT endpoint common interrupt  mask register
OTG_HS_DEVICE $18 + constant OTG_HS_DEVICE_OTG_HS_DAINT ( read-only )  \ OTG_HS device all endpoints interrupt  register
OTG_HS_DEVICE $1C + constant OTG_HS_DEVICE_OTG_HS_DAINTMSK ( read-write )  \ OTG_HS all endpoints interrupt mask  register
OTG_HS_DEVICE $28 + constant OTG_HS_DEVICE_OTG_HS_DVBUSDIS ( read-write )  \ OTG_HS device VBUS discharge time  register
OTG_HS_DEVICE $2C + constant OTG_HS_DEVICE_OTG_HS_DVBUSPULSE ( read-write )  \ OTG_HS device VBUS pulsing time  register
OTG_HS_DEVICE $30 + constant OTG_HS_DEVICE_OTG_HS_DTHRCTL ( read-write )  \ OTG_HS Device threshold control  register
OTG_HS_DEVICE $34 + constant OTG_HS_DEVICE_OTG_HS_DIEPEMPMSK ( read-write )  \ OTG_HS device IN endpoint FIFO empty  interrupt mask register
OTG_HS_DEVICE $38 + constant OTG_HS_DEVICE_OTG_HS_DEACHINT ( read-write )  \ OTG_HS device each endpoint interrupt  register
OTG_HS_DEVICE $3C + constant OTG_HS_DEVICE_OTG_HS_DEACHINTMSK ( read-write )  \ OTG_HS device each endpoint interrupt  register mask
OTG_HS_DEVICE $40 + constant OTG_HS_DEVICE_OTG_HS_DIEPEACHMSK1 ( read-write )  \ OTG_HS device each in endpoint-1 interrupt  register
OTG_HS_DEVICE $80 + constant OTG_HS_DEVICE_OTG_HS_DOEPEACHMSK1 ( read-write )  \ OTG_HS device each OUT endpoint-1 interrupt  register
OTG_HS_DEVICE $100 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL0 (  )  \ OTG device endpoint-0 control  register
OTG_HS_DEVICE $120 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL1 (  )  \ OTG device endpoint-1 control  register
OTG_HS_DEVICE $140 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL2 (  )  \ OTG device endpoint-2 control  register
OTG_HS_DEVICE $160 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL3 (  )  \ OTG device endpoint-3 control  register
OTG_HS_DEVICE $180 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL4 (  )  \ OTG device endpoint-4 control  register
OTG_HS_DEVICE $1A0 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL5 (  )  \ OTG device endpoint-5 control  register
OTG_HS_DEVICE $1C0 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL6 (  )  \ OTG device endpoint-6 control  register
OTG_HS_DEVICE $1E0 + constant OTG_HS_DEVICE_OTG_HS_DIEPCTL7 (  )  \ OTG device endpoint-7 control  register
OTG_HS_DEVICE $108 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT0 (  )  \ OTG device endpoint-0 interrupt  register
OTG_HS_DEVICE $128 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT1 (  )  \ OTG device endpoint-1 interrupt  register
OTG_HS_DEVICE $148 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT2 (  )  \ OTG device endpoint-2 interrupt  register
OTG_HS_DEVICE $168 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT3 (  )  \ OTG device endpoint-3 interrupt  register
OTG_HS_DEVICE $188 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT4 (  )  \ OTG device endpoint-4 interrupt  register
OTG_HS_DEVICE $1A8 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT5 (  )  \ OTG device endpoint-5 interrupt  register
OTG_HS_DEVICE $1C8 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT6 (  )  \ OTG device endpoint-6 interrupt  register
OTG_HS_DEVICE $1E8 + constant OTG_HS_DEVICE_OTG_HS_DIEPINT7 (  )  \ OTG device endpoint-7 interrupt  register
OTG_HS_DEVICE $110 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ0 ( read-write )  \ OTG_HS device IN endpoint 0 transfer size  register
OTG_HS_DEVICE $114 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA1 ( read-write )  \ OTG_HS device endpoint-1 DMA address  register
OTG_HS_DEVICE $134 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA2 ( read-write )  \ OTG_HS device endpoint-2 DMA address  register
OTG_HS_DEVICE $154 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA3 ( read-write )  \ OTG_HS device endpoint-3 DMA address  register
OTG_HS_DEVICE $174 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA4 ( read-write )  \ OTG_HS device endpoint-4 DMA address  register
OTG_HS_DEVICE $194 + constant OTG_HS_DEVICE_OTG_HS_DIEPDMA5 ( read-write )  \ OTG_HS device endpoint-5 DMA address  register
OTG_HS_DEVICE $118 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS0 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO  status register
OTG_HS_DEVICE $138 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS1 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO  status register
OTG_HS_DEVICE $158 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS2 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO  status register
OTG_HS_DEVICE $178 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS3 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO  status register
OTG_HS_DEVICE $198 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS4 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO  status register
OTG_HS_DEVICE $1B8 + constant OTG_HS_DEVICE_OTG_HS_DTXFSTS5 ( read-only )  \ OTG_HS device IN endpoint transmit FIFO  status register
OTG_HS_DEVICE $130 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ1 ( read-write )  \ OTG_HS device endpoint transfer size  register
OTG_HS_DEVICE $150 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ2 ( read-write )  \ OTG_HS device endpoint transfer size  register
OTG_HS_DEVICE $170 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ3 ( read-write )  \ OTG_HS device endpoint transfer size  register
OTG_HS_DEVICE $190 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ4 ( read-write )  \ OTG_HS device endpoint transfer size  register
OTG_HS_DEVICE $1B0 + constant OTG_HS_DEVICE_OTG_HS_DIEPTSIZ5 ( read-write )  \ OTG_HS device endpoint transfer size  register
OTG_HS_DEVICE $300 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL0 (  )  \ OTG_HS device control OUT endpoint 0 control  register
OTG_HS_DEVICE $320 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL1 (  )  \ OTG device endpoint-1 control  register
OTG_HS_DEVICE $340 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL2 (  )  \ OTG device endpoint-2 control  register
OTG_HS_DEVICE $360 + constant OTG_HS_DEVICE_OTG_HS_DOEPCTL3 (  )  \ OTG device endpoint-3 control  register
OTG_HS_DEVICE $308 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT0 ( read-write )  \ OTG_HS device endpoint-0 interrupt  register
OTG_HS_DEVICE $328 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT1 ( read-write )  \ OTG_HS device endpoint-1 interrupt  register
OTG_HS_DEVICE $348 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT2 ( read-write )  \ OTG_HS device endpoint-2 interrupt  register
OTG_HS_DEVICE $368 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT3 ( read-write )  \ OTG_HS device endpoint-3 interrupt  register
OTG_HS_DEVICE $388 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT4 ( read-write )  \ OTG_HS device endpoint-4 interrupt  register
OTG_HS_DEVICE $3A8 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT5 ( read-write )  \ OTG_HS device endpoint-5 interrupt  register
OTG_HS_DEVICE $3C8 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT6 ( read-write )  \ OTG_HS device endpoint-6 interrupt  register
OTG_HS_DEVICE $3E8 + constant OTG_HS_DEVICE_OTG_HS_DOEPINT7 ( read-write )  \ OTG_HS device endpoint-7 interrupt  register
OTG_HS_DEVICE $310 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ0 ( read-write )  \ OTG_HS device endpoint-1 transfer size  register
OTG_HS_DEVICE $330 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ1 ( read-write )  \ OTG_HS device endpoint-2 transfer size  register
OTG_HS_DEVICE $350 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ2 ( read-write )  \ OTG_HS device endpoint-3 transfer size  register
OTG_HS_DEVICE $370 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ3 ( read-write )  \ OTG_HS device endpoint-4 transfer size  register
OTG_HS_DEVICE $390 + constant OTG_HS_DEVICE_OTG_HS_DOEPTSIZ4 ( read-write )  \ OTG_HS device endpoint-5 transfer size  register
[then]

defined? use-OTG_HS_PWRCLK defined? OTG_HS_PWRCLK not and [if]
$40040E00 constant OTG_HS_PWRCLK ( USB on the go high speed ) 
OTG_HS_PWRCLK $0 + constant OTG_HS_PWRCLK_OTG_HS_PCGCR ( read-write )  \ Power and clock gating control  register
[then]

defined? use-NVIC defined? NVIC not and [if]
$E000E000 constant NVIC ( Nested Vectored Interrupt  Controller ) 
NVIC $4 + constant NVIC_ICTR ( read-only )  \ Interrupt Controller Type  Register
NVIC $F00 + constant NVIC_STIR ( write-only )  \ Software Triggered Interrupt  Register
NVIC $100 + constant NVIC_ISER0 ( read-write )  \ Interrupt Set-Enable Register
NVIC $104 + constant NVIC_ISER1 ( read-write )  \ Interrupt Set-Enable Register
NVIC $108 + constant NVIC_ISER2 ( read-write )  \ Interrupt Set-Enable Register
NVIC $180 + constant NVIC_ICER0 ( read-write )  \ Interrupt Clear-Enable  Register
NVIC $184 + constant NVIC_ICER1 ( read-write )  \ Interrupt Clear-Enable  Register
NVIC $188 + constant NVIC_ICER2 ( read-write )  \ Interrupt Clear-Enable  Register
NVIC $200 + constant NVIC_ISPR0 ( read-write )  \ Interrupt Set-Pending Register
NVIC $204 + constant NVIC_ISPR1 ( read-write )  \ Interrupt Set-Pending Register
NVIC $208 + constant NVIC_ISPR2 ( read-write )  \ Interrupt Set-Pending Register
NVIC $280 + constant NVIC_ICPR0 ( read-write )  \ Interrupt Clear-Pending  Register
NVIC $284 + constant NVIC_ICPR1 ( read-write )  \ Interrupt Clear-Pending  Register
NVIC $288 + constant NVIC_ICPR2 ( read-write )  \ Interrupt Clear-Pending  Register
NVIC $300 + constant NVIC_IABR0 ( read-only )  \ Interrupt Active Bit Register
NVIC $304 + constant NVIC_IABR1 ( read-only )  \ Interrupt Active Bit Register
NVIC $308 + constant NVIC_IABR2 ( read-only )  \ Interrupt Active Bit Register
NVIC $400 + constant NVIC_IPR0 ( read-write )  \ Interrupt Priority Register
NVIC $404 + constant NVIC_IPR1 ( read-write )  \ Interrupt Priority Register
NVIC $408 + constant NVIC_IPR2 ( read-write )  \ Interrupt Priority Register
NVIC $40C + constant NVIC_IPR3 ( read-write )  \ Interrupt Priority Register
NVIC $410 + constant NVIC_IPR4 ( read-write )  \ Interrupt Priority Register
NVIC $414 + constant NVIC_IPR5 ( read-write )  \ Interrupt Priority Register
NVIC $418 + constant NVIC_IPR6 ( read-write )  \ Interrupt Priority Register
NVIC $41C + constant NVIC_IPR7 ( read-write )  \ Interrupt Priority Register
NVIC $420 + constant NVIC_IPR8 ( read-write )  \ Interrupt Priority Register
NVIC $424 + constant NVIC_IPR9 ( read-write )  \ Interrupt Priority Register
NVIC $428 + constant NVIC_IPR10 ( read-write )  \ Interrupt Priority Register
NVIC $42C + constant NVIC_IPR11 ( read-write )  \ Interrupt Priority Register
NVIC $430 + constant NVIC_IPR12 ( read-write )  \ Interrupt Priority Register
NVIC $434 + constant NVIC_IPR13 ( read-write )  \ Interrupt Priority Register
NVIC $438 + constant NVIC_IPR14 ( read-write )  \ Interrupt Priority Register
NVIC $43C + constant NVIC_IPR15 ( read-write )  \ Interrupt Priority Register
NVIC $440 + constant NVIC_IPR16 ( read-write )  \ Interrupt Priority Register
NVIC $444 + constant NVIC_IPR17 ( read-write )  \ Interrupt Priority Register
NVIC $448 + constant NVIC_IPR18 ( read-write )  \ Interrupt Priority Register
NVIC $44C + constant NVIC_IPR19 ( read-write )  \ Interrupt Priority Register
[then]

forth-wordlist 1 set-order
forth-wordlist set-current

compile-to-ram
