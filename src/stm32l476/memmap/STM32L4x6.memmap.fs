\ Copyright (c) 2016-2020 Terry Porter <terry@tjporter.com.au>
\ Copyright (c) 2020-2023 Travis Bemann
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

compress-flash

begin-module memmap

  execute-defined? use-DAC [if]
    $40007400 constant DAC ( Digital-to-analog converter ) 
    DAC $0 + constant DAC_CR ( read-write )  \ control register
    DAC $4 + constant DAC_SWTRIGR ( write-only )  \ software trigger register
    DAC $8 + constant DAC_DHR12R1 ( read-write )  \ channel1 12-bit right-aligned data holding  register
    DAC $C + constant DAC_DHR12L1 ( read-write )  \ channel1 12-bit left-aligned data holding  register
    DAC $10 + constant DAC_DHR8R1 ( read-write )  \ channel1 8-bit right-aligned data holding  register
    DAC $14 + constant DAC_DHR12R2 ( read-write )  \ channel2 12-bit right aligned data holding  register
    DAC $18 + constant DAC_DHR12L2 ( read-write )  \ channel2 12-bit left aligned data holding  register
    DAC $1C + constant DAC_DHR8R2 ( read-write )  \ channel2 8-bit right-aligned data holding  register
    DAC $20 + constant DAC_DHR12RD ( read-write )  \ Dual DAC 12-bit right-aligned data holding  register
    DAC $24 + constant DAC_DHR12LD ( read-write )  \ DUAL DAC 12-bit left aligned data holding  register
    DAC $28 + constant DAC_DHR8RD ( read-write )  \ DUAL DAC 8-bit right aligned data holding  register
    DAC $2C + constant DAC_DOR1 ( read-only )  \ channel1 data output register
    DAC $30 + constant DAC_DOR2 ( read-only )  \ channel2 data output register
    DAC $34 + constant DAC_SR (  )  \ status register
    DAC $38 + constant DAC_CCR ( read-write )  \ calibration control register
    DAC $3C + constant DAC_MCR ( read-write )  \ mode control register
    DAC $40 + constant DAC_SHSR1 ( read-write )  \ Sample and Hold sample time register  1
    DAC $44 + constant DAC_SHSR2 ( read-write )  \ Sample and Hold sample time register  2
    DAC $48 + constant DAC_SHHR ( read-write )  \ Sample and Hold hold time  register
    DAC $4C + constant DAC_SHRR ( read-write )  \ Sample and Hold refresh time  register
  [then]

  [if]
    $40020000 constant DMA1 ( Direct memory access controller ) 
    DMA1 $0 + constant DMA1_ISR ( read-only )  \ interrupt status register
    DMA1 $4 + constant DMA1_IFCR ( write-only )  \ interrupt flag clear register
    DMA1 $8 + constant DMA1_CCR1 ( read-write )  \ channel x configuration  register
    DMA1 $C + constant DMA1_CNDTR1 ( read-write )  \ channel x number of data  register
    DMA1 $10 + constant DMA1_CPAR1 ( read-write )  \ channel x peripheral address  register
    DMA1 $14 + constant DMA1_CMAR1 ( read-write )  \ channel x memory address  register
    DMA1 $1C + constant DMA1_CCR2 ( read-write )  \ channel x configuration  register
    DMA1 $20 + constant DMA1_CNDTR2 ( read-write )  \ channel x number of data  register
    DMA1 $24 + constant DMA1_CPAR2 ( read-write )  \ channel x peripheral address  register
    DMA1 $28 + constant DMA1_CMAR2 ( read-write )  \ channel x memory address  register
    DMA1 $30 + constant DMA1_CCR3 ( read-write )  \ channel x configuration  register
    DMA1 $34 + constant DMA1_CNDTR3 ( read-write )  \ channel x number of data  register
    DMA1 $38 + constant DMA1_CPAR3 ( read-write )  \ channel x peripheral address  register
    DMA1 $3C + constant DMA1_CMAR3 ( read-write )  \ channel x memory address  register
    DMA1 $44 + constant DMA1_CCR4 ( read-write )  \ channel x configuration  register
    DMA1 $48 + constant DMA1_CNDTR4 ( read-write )  \ channel x number of data  register
    DMA1 $4C + constant DMA1_CPAR4 ( read-write )  \ channel x peripheral address  register
    DMA1 $50 + constant DMA1_CMAR4 ( read-write )  \ channel x memory address  register
    DMA1 $58 + constant DMA1_CCR5 ( read-write )  \ channel x configuration  register
    DMA1 $5C + constant DMA1_CNDTR5 ( read-write )  \ channel x number of data  register
    DMA1 $60 + constant DMA1_CPAR5 ( read-write )  \ channel x peripheral address  register
    DMA1 $64 + constant DMA1_CMAR5 ( read-write )  \ channel x memory address  register
    DMA1 $6C + constant DMA1_CCR6 ( read-write )  \ channel x configuration  register
    DMA1 $70 + constant DMA1_CNDTR6 ( read-write )  \ channel x number of data  register
    DMA1 $74 + constant DMA1_CPAR6 ( read-write )  \ channel x peripheral address  register
    DMA1 $78 + constant DMA1_CMAR6 ( read-write )  \ channel x memory address  register
    DMA1 $80 + constant DMA1_CCR7 ( read-write )  \ channel x configuration  register
    DMA1 $84 + constant DMA1_CNDTR7 ( read-write )  \ channel x number of data  register
    DMA1 $88 + constant DMA1_CPAR7 ( read-write )  \ channel x peripheral address  register
    DMA1 $8C + constant DMA1_CMAR7 ( read-write )  \ channel x memory address  register
    DMA1 $A8 + constant DMA1_CSELR ( read-write )  \ channel selection register
  [then]

  execute-defined? use-DMA2 [if]
    $40020400 constant DMA2 ( Direct memory access controller ) 
    DMA2 $0 + constant DMA2_ISR ( read-only )  \ interrupt status register
    DMA2 $4 + constant DMA2_IFCR ( write-only )  \ interrupt flag clear register
    DMA2 $8 + constant DMA2_CCR1 ( read-write )  \ channel x configuration  register
    DMA2 $C + constant DMA2_CNDTR1 ( read-write )  \ channel x number of data  register
    DMA2 $10 + constant DMA2_CPAR1 ( read-write )  \ channel x peripheral address  register
    DMA2 $14 + constant DMA2_CMAR1 ( read-write )  \ channel x memory address  register
    DMA2 $1C + constant DMA2_CCR2 ( read-write )  \ channel x configuration  register
    DMA2 $20 + constant DMA2_CNDTR2 ( read-write )  \ channel x number of data  register
    DMA2 $24 + constant DMA2_CPAR2 ( read-write )  \ channel x peripheral address  register
    DMA2 $28 + constant DMA2_CMAR2 ( read-write )  \ channel x memory address  register
    DMA2 $30 + constant DMA2_CCR3 ( read-write )  \ channel x configuration  register
    DMA2 $34 + constant DMA2_CNDTR3 ( read-write )  \ channel x number of data  register
    DMA2 $38 + constant DMA2_CPAR3 ( read-write )  \ channel x peripheral address  register
    DMA2 $3C + constant DMA2_CMAR3 ( read-write )  \ channel x memory address  register
    DMA2 $44 + constant DMA2_CCR4 ( read-write )  \ channel x configuration  register
    DMA2 $48 + constant DMA2_CNDTR4 ( read-write )  \ channel x number of data  register
    DMA2 $4C + constant DMA2_CPAR4 ( read-write )  \ channel x peripheral address  register
    DMA2 $50 + constant DMA2_CMAR4 ( read-write )  \ channel x memory address  register
    DMA2 $58 + constant DMA2_CCR5 ( read-write )  \ channel x configuration  register
    DMA2 $5C + constant DMA2_CNDTR5 ( read-write )  \ channel x number of data  register
    DMA2 $60 + constant DMA2_CPAR5 ( read-write )  \ channel x peripheral address  register
    DMA2 $64 + constant DMA2_CMAR5 ( read-write )  \ channel x memory address  register
    DMA2 $6C + constant DMA2_CCR6 ( read-write )  \ channel x configuration  register
    DMA2 $70 + constant DMA2_CNDTR6 ( read-write )  \ channel x number of data  register
    DMA2 $74 + constant DMA2_CPAR6 ( read-write )  \ channel x peripheral address  register
    DMA2 $78 + constant DMA2_CMAR6 ( read-write )  \ channel x memory address  register
    DMA2 $80 + constant DMA2_CCR7 ( read-write )  \ channel x configuration  register
    DMA2 $84 + constant DMA2_CNDTR7 ( read-write )  \ channel x number of data  register
    DMA2 $88 + constant DMA2_CPAR7 ( read-write )  \ channel x peripheral address  register
    DMA2 $8C + constant DMA2_CMAR7 ( read-write )  \ channel x memory address  register
    DMA2 $A8 + constant DMA2_CSELR ( read-write )  \ channel selection register
  [then]

  [if]
    $40023000 constant CRC ( Cyclic redundancy check calculation  unit ) 
    CRC $0 + constant CRC_DR ( read-write )  \ Data register
    CRC $4 + constant CRC_IDR ( read-write )  \ Independent data register
    CRC $8 + constant CRC_CR (  )  \ Control register
    CRC $10 + constant CRC_INIT ( read-write )  \ Initial CRC value
    CRC $14 + constant CRC_POL ( read-write )  \ polynomial
  [then]

  execute-defined? use-IWDG [if]
    $40003000 constant IWDG ( Independent watchdog ) 
    IWDG $0 + constant IWDG_KR ( write-only )  \ Key register
    IWDG $4 + constant IWDG_PR ( read-write )  \ Prescaler register
    IWDG $8 + constant IWDG_RLR ( read-write )  \ Reload register
    IWDG $C + constant IWDG_SR ( read-only )  \ Status register
    IWDG $10 + constant IWDG_WINR ( read-write )  \ Window register
  [then]

  [if]
    $40002C00 constant WWDG ( System window watchdog ) 
    WWDG $0 + constant WWDG_CR ( read-write )  \ Control register
    WWDG $4 + constant WWDG_CFR ( read-write )  \ Configuration register
    WWDG $8 + constant WWDG_SR ( read-write )  \ Status register
  [then]

  execute-defined? use-I2C1 [if]
    $40005400 constant I2C1 ( Inter-integrated circuit ) 
    I2C1 $0 + constant I2C1_CR1 ( read-write )  \ Control register 1
    I2C1 $4 + constant I2C1_CR2 ( read-write )  \ Control register 2
    I2C1 $8 + constant I2C1_OAR1 ( read-write )  \ Own address register 1
    I2C1 $C + constant I2C1_OAR2 ( read-write )  \ Own address register 2
    I2C1 $10 + constant I2C1_TIMINGR ( read-write )  \ Timing register
    I2C1 $14 + constant I2C1_TIMEOUTR ( read-write )  \ Status register 1
    I2C1 $18 + constant I2C1_ISR (  )  \ Interrupt and Status register
    I2C1 $1C + constant I2C1_ICR ( write-only )  \ Interrupt clear register
    I2C1 $20 + constant I2C1_PECR ( read-only )  \ PEC register
    I2C1 $24 + constant I2C1_RXDR ( read-only )  \ Receive data register
    I2C1 $28 + constant I2C1_TXDR ( read-write )  \ Transmit data register
  [then]

  [if]
    $40005800 constant I2C2 ( Inter-integrated circuit ) 
    I2C2 $0 + constant I2C2_CR1 ( read-write )  \ Control register 1
    I2C2 $4 + constant I2C2_CR2 ( read-write )  \ Control register 2
    I2C2 $8 + constant I2C2_OAR1 ( read-write )  \ Own address register 1
    I2C2 $C + constant I2C2_OAR2 ( read-write )  \ Own address register 2
    I2C2 $10 + constant I2C2_TIMINGR ( read-write )  \ Timing register
    I2C2 $14 + constant I2C2_TIMEOUTR ( read-write )  \ Status register 1
    I2C2 $18 + constant I2C2_ISR (  )  \ Interrupt and Status register
    I2C2 $1C + constant I2C2_ICR ( write-only )  \ Interrupt clear register
    I2C2 $20 + constant I2C2_PECR ( read-only )  \ PEC register
    I2C2 $24 + constant I2C2_RXDR ( read-only )  \ Receive data register
    I2C2 $28 + constant I2C2_TXDR ( read-write )  \ Transmit data register
  [then]

  execute-defined? use-I2C3 [if]
    $40005C00 constant I2C3 ( Inter-integrated circuit ) 
    I2C3 $0 + constant I2C3_CR1 ( read-write )  \ Control register 1
    I2C3 $4 + constant I2C3_CR2 ( read-write )  \ Control register 2
    I2C3 $8 + constant I2C3_OAR1 ( read-write )  \ Own address register 1
    I2C3 $C + constant I2C3_OAR2 ( read-write )  \ Own address register 2
    I2C3 $10 + constant I2C3_TIMINGR ( read-write )  \ Timing register
    I2C3 $14 + constant I2C3_TIMEOUTR ( read-write )  \ Status register 1
    I2C3 $18 + constant I2C3_ISR (  )  \ Interrupt and Status register
    I2C3 $1C + constant I2C3_ICR ( write-only )  \ Interrupt clear register
    I2C3 $20 + constant I2C3_PECR ( read-only )  \ PEC register
    I2C3 $24 + constant I2C3_RXDR ( read-only )  \ Receive data register
    I2C3 $28 + constant I2C3_TXDR ( read-write )  \ Transmit data register
  [then]

  [if]
    $40021000 constant RCC ( Reset and clock control ) 
    RCC $0 + constant RCC_CR (  )  \ Clock control register
    RCC $4 + constant RCC_ICSCR (  )  \ Internal clock sources calibration  register
    RCC $8 + constant RCC_CFGR (  )  \ Clock configuration register
    RCC $C + constant RCC_PLLCFGR ( read-write )  \ PLL configuration register
    RCC $10 + constant RCC_PLLSAI1CFGR ( read-write )  \ PLLSAI1 configuration register
    RCC $14 + constant RCC_PLLSAI2CFGR ( read-write )  \ PLLSAI2 configuration register
    RCC $18 + constant RCC_CIER ( read-write )  \ Clock interrupt enable  register
    RCC $1C + constant RCC_CIFR ( read-only )  \ Clock interrupt flag register
    RCC $20 + constant RCC_CICR ( write-only )  \ Clock interrupt clear register
    RCC $28 + constant RCC_AHB1RSTR ( read-write )  \ AHB1 peripheral reset register
    RCC $2C + constant RCC_AHB2RSTR ( read-write )  \ AHB2 peripheral reset register
    RCC $30 + constant RCC_AHB3RSTR ( read-write )  \ AHB3 peripheral reset register
    RCC $38 + constant RCC_APB1RSTR1 ( read-write )  \ APB1 peripheral reset register  1
    RCC $3C + constant RCC_APB1RSTR2 ( read-write )  \ APB1 peripheral reset register  2
    RCC $40 + constant RCC_APB2RSTR ( read-write )  \ APB2 peripheral reset register
    RCC $48 + constant RCC_AHB1ENR ( read-write )  \ AHB1 peripheral clock enable  register
    RCC $4C + constant RCC_AHB2ENR ( read-write )  \ AHB2 peripheral clock enable  register
    RCC $50 + constant RCC_AHB3ENR ( read-write )  \ AHB3 peripheral clock enable  register
    RCC $58 + constant RCC_APB1ENR1 ( read-write )  \ APB1ENR1
    RCC $5C + constant RCC_APB1ENR2 ( read-write )  \ APB1 peripheral clock enable register  2
    RCC $60 + constant RCC_APB2ENR ( read-write )  \ APB2ENR
    RCC $68 + constant RCC_AHB1SMENR ( read-write )  \ AHB1 peripheral clocks enable in Sleep and  Stop modes register
    RCC $6C + constant RCC_AHB2SMENR ( read-write )  \ AHB2 peripheral clocks enable in Sleep and  Stop modes register
    RCC $70 + constant RCC_AHB3SMENR ( read-write )  \ AHB3 peripheral clocks enable in Sleep and  Stop modes register
    RCC $78 + constant RCC_APB1SMENR1 ( read-write )  \ APB1SMENR1
    RCC $7C + constant RCC_APB1SMENR2 ( read-write )  \ APB1 peripheral clocks enable in Sleep and  Stop modes register 2
    RCC $80 + constant RCC_APB2SMENR ( read-write )  \ APB2SMENR
    RCC $88 + constant RCC_CCIPR ( read-write )  \ CCIPR
    RCC $90 + constant RCC_BDCR (  )  \ BDCR
    RCC $94 + constant RCC_CSR (  )  \ CSR
  [then]

  execute-defined? use-PWR [if]
    $40007000 constant PWR ( Power control ) 
    PWR $0 + constant PWR_CR1 ( read-write )  \ Power control register 1
    PWR $4 + constant PWR_CR2 ( read-write )  \ Power control register 2
    PWR $8 + constant PWR_CR3 ( read-write )  \ Power control register 3
    PWR $C + constant PWR_CR4 ( read-write )  \ Power control register 4
    PWR $10 + constant PWR_SR1 ( read-only )  \ Power status register 1
    PWR $14 + constant PWR_SR2 ( read-only )  \ Power status register 2
    PWR $18 + constant PWR_SCR ( write-only )  \ Power status clear register
    PWR $20 + constant PWR_PUCRA ( read-write )  \ Power Port A pull-up control  register
    PWR $24 + constant PWR_PDCRA ( read-write )  \ Power Port A pull-down control  register
    PWR $28 + constant PWR_PUCRB ( read-write )  \ Power Port B pull-up control  register
    PWR $2C + constant PWR_PDCRB ( read-write )  \ Power Port B pull-down control  register
    PWR $30 + constant PWR_PUCRC ( read-write )  \ Power Port C pull-up control  register
    PWR $34 + constant PWR_PDCRC ( read-write )  \ Power Port C pull-down control  register
    PWR $38 + constant PWR_PUCRD ( read-write )  \ Power Port D pull-up control  register
    PWR $3C + constant PWR_PDCRD ( read-write )  \ Power Port D pull-down control  register
    PWR $40 + constant PWR_PUCRE ( read-write )  \ Power Port E pull-up control  register
    PWR $44 + constant PWR_PDCRE ( read-write )  \ Power Port E pull-down control  register
    PWR $48 + constant PWR_PUCRF ( read-write )  \ Power Port F pull-up control  register
    PWR $4C + constant PWR_PDCRF ( read-write )  \ Power Port F pull-down control  register
    PWR $50 + constant PWR_PUCRG ( read-write )  \ Power Port G pull-up control  register
    PWR $54 + constant PWR_PDCRG ( read-write )  \ Power Port G pull-down control  register
    PWR $58 + constant PWR_PUCRH ( read-write )  \ Power Port H pull-up control  register
    PWR $5C + constant PWR_PDCRH ( read-write )  \ Power Port H pull-down control  register
  [then]

  [if]
    $40010000 constant SYSCFG ( System configuration controller ) 
    SYSCFG $0 + constant SYSCFG_MEMRMP ( read-write )  \ memory remap register
    SYSCFG $4 + constant SYSCFG_CFGR1 ( read-write )  \ configuration register 1
    SYSCFG $8 + constant SYSCFG_EXTICR1 ( read-write )  \ external interrupt configuration register  1
    SYSCFG $C + constant SYSCFG_EXTICR2 ( read-write )  \ external interrupt configuration register  2
    SYSCFG $10 + constant SYSCFG_EXTICR3 ( read-write )  \ external interrupt configuration register  3
    SYSCFG $14 + constant SYSCFG_EXTICR4 ( read-write )  \ external interrupt configuration register  4
    SYSCFG $18 + constant SYSCFG_SCSR (  )  \ SCSR
    SYSCFG $1C + constant SYSCFG_CFGR2 (  )  \ CFGR2
    SYSCFG $20 + constant SYSCFG_SWPR ( write-only )  \ SWPR
    SYSCFG $24 + constant SYSCFG_SKR ( write-only )  \ SKR
  [then]

  execute-defined? use-RNG [if]
    $50060800 constant RNG ( Random number generator ) 
    RNG $0 + constant RNG_CR ( read-write )  \ control register
    RNG $4 + constant RNG_SR (  )  \ status register
    RNG $8 + constant RNG_DR ( read-only )  \ data register
  [then]

  [if]
    $50040000 constant ADC1 ( Analog-to-Digital Converter ) 
    ADC1 $0 + constant ADC1_ISR ( read-write )  \ interrupt and status register
    ADC1 $4 + constant ADC1_IER ( read-write )  \ interrupt enable register
    ADC1 $8 + constant ADC1_CR ( read-write )  \ control register
    ADC1 $C + constant ADC1_CFGR ( read-write )  \ configuration register
    ADC1 $10 + constant ADC1_CFGR2 ( read-write )  \ configuration register
    ADC1 $14 + constant ADC1_SMPR1 ( read-write )  \ sample time register 1
    ADC1 $18 + constant ADC1_SMPR2 ( read-write )  \ sample time register 2
    ADC1 $20 + constant ADC1_TR1 ( read-write )  \ watchdog threshold register 1
    ADC1 $24 + constant ADC1_TR2 ( read-write )  \ watchdog threshold register
    ADC1 $28 + constant ADC1_TR3 ( read-write )  \ watchdog threshold register 3
    ADC1 $30 + constant ADC1_SQR1 ( read-write )  \ regular sequence register 1
    ADC1 $34 + constant ADC1_SQR2 ( read-write )  \ regular sequence register 2
    ADC1 $38 + constant ADC1_SQR3 ( read-write )  \ regular sequence register 3
    ADC1 $3C + constant ADC1_SQR4 ( read-write )  \ regular sequence register 4
    ADC1 $40 + constant ADC1_DR ( read-only )  \ regular Data Register
    ADC1 $4C + constant ADC1_JSQR ( read-write )  \ injected sequence register
    ADC1 $60 + constant ADC1_OFR1 ( read-write )  \ offset register 1
    ADC1 $64 + constant ADC1_OFR2 ( read-write )  \ offset register 2
    ADC1 $68 + constant ADC1_OFR3 ( read-write )  \ offset register 3
    ADC1 $6C + constant ADC1_OFR4 ( read-write )  \ offset register 4
    ADC1 $80 + constant ADC1_JDR1 ( read-only )  \ injected data register 1
    ADC1 $84 + constant ADC1_JDR2 ( read-only )  \ injected data register 2
    ADC1 $88 + constant ADC1_JDR3 ( read-only )  \ injected data register 3
    ADC1 $8C + constant ADC1_JDR4 ( read-only )  \ injected data register 4
    ADC1 $A0 + constant ADC1_AWD2CR ( read-write )  \ Analog Watchdog 2 Configuration  Register
    ADC1 $A4 + constant ADC1_AWD3CR ( read-write )  \ Analog Watchdog 3 Configuration  Register
    ADC1 $B0 + constant ADC1_DIFSEL (  )  \ Differential Mode Selection Register  2
    ADC1 $B4 + constant ADC1_CALFACT ( read-write )  \ Calibration Factors
  [then]

  execute-defined? use-ADC2 [if]
    $50040100 constant ADC2 ( Analog-to-Digital Converter ) 
    ADC2 $0 + constant ADC2_ISR ( read-write )  \ interrupt and status register
    ADC2 $4 + constant ADC2_IER ( read-write )  \ interrupt enable register
    ADC2 $8 + constant ADC2_CR ( read-write )  \ control register
    ADC2 $C + constant ADC2_CFGR ( read-write )  \ configuration register
    ADC2 $10 + constant ADC2_CFGR2 ( read-write )  \ configuration register
    ADC2 $14 + constant ADC2_SMPR1 ( read-write )  \ sample time register 1
    ADC2 $18 + constant ADC2_SMPR2 ( read-write )  \ sample time register 2
    ADC2 $20 + constant ADC2_TR1 ( read-write )  \ watchdog threshold register 1
    ADC2 $24 + constant ADC2_TR2 ( read-write )  \ watchdog threshold register
    ADC2 $28 + constant ADC2_TR3 ( read-write )  \ watchdog threshold register 3
    ADC2 $30 + constant ADC2_SQR1 ( read-write )  \ regular sequence register 1
    ADC2 $34 + constant ADC2_SQR2 ( read-write )  \ regular sequence register 2
    ADC2 $38 + constant ADC2_SQR3 ( read-write )  \ regular sequence register 3
    ADC2 $3C + constant ADC2_SQR4 ( read-write )  \ regular sequence register 4
    ADC2 $40 + constant ADC2_DR ( read-only )  \ regular Data Register
    ADC2 $4C + constant ADC2_JSQR ( read-write )  \ injected sequence register
    ADC2 $60 + constant ADC2_OFR1 ( read-write )  \ offset register 1
    ADC2 $64 + constant ADC2_OFR2 ( read-write )  \ offset register 2
    ADC2 $68 + constant ADC2_OFR3 ( read-write )  \ offset register 3
    ADC2 $6C + constant ADC2_OFR4 ( read-write )  \ offset register 4
    ADC2 $80 + constant ADC2_JDR1 ( read-only )  \ injected data register 1
    ADC2 $84 + constant ADC2_JDR2 ( read-only )  \ injected data register 2
    ADC2 $88 + constant ADC2_JDR3 ( read-only )  \ injected data register 3
    ADC2 $8C + constant ADC2_JDR4 ( read-only )  \ injected data register 4
    ADC2 $A0 + constant ADC2_AWD2CR ( read-write )  \ Analog Watchdog 2 Configuration  Register
    ADC2 $A4 + constant ADC2_AWD3CR ( read-write )  \ Analog Watchdog 3 Configuration  Register
    ADC2 $B0 + constant ADC2_DIFSEL (  )  \ Differential Mode Selection Register  2
    ADC2 $B4 + constant ADC2_CALFACT ( read-write )  \ Calibration Factors
  [then]

  [if]
    $50040200 constant ADC3 ( Analog-to-Digital Converter ) 
    ADC3 $0 + constant ADC3_ISR ( read-write )  \ interrupt and status register
    ADC3 $4 + constant ADC3_IER ( read-write )  \ interrupt enable register
    ADC3 $8 + constant ADC3_CR ( read-write )  \ control register
    ADC3 $C + constant ADC3_CFGR ( read-write )  \ configuration register
    ADC3 $10 + constant ADC3_CFGR2 ( read-write )  \ configuration register
    ADC3 $14 + constant ADC3_SMPR1 ( read-write )  \ sample time register 1
    ADC3 $18 + constant ADC3_SMPR2 ( read-write )  \ sample time register 2
    ADC3 $20 + constant ADC3_TR1 ( read-write )  \ watchdog threshold register 1
    ADC3 $24 + constant ADC3_TR2 ( read-write )  \ watchdog threshold register
    ADC3 $28 + constant ADC3_TR3 ( read-write )  \ watchdog threshold register 3
    ADC3 $30 + constant ADC3_SQR1 ( read-write )  \ regular sequence register 1
    ADC3 $34 + constant ADC3_SQR2 ( read-write )  \ regular sequence register 2
    ADC3 $38 + constant ADC3_SQR3 ( read-write )  \ regular sequence register 3
    ADC3 $3C + constant ADC3_SQR4 ( read-write )  \ regular sequence register 4
    ADC3 $40 + constant ADC3_DR ( read-only )  \ regular Data Register
    ADC3 $4C + constant ADC3_JSQR ( read-write )  \ injected sequence register
    ADC3 $60 + constant ADC3_OFR1 ( read-write )  \ offset register 1
    ADC3 $64 + constant ADC3_OFR2 ( read-write )  \ offset register 2
    ADC3 $68 + constant ADC3_OFR3 ( read-write )  \ offset register 3
    ADC3 $6C + constant ADC3_OFR4 ( read-write )  \ offset register 4
    ADC3 $80 + constant ADC3_JDR1 ( read-only )  \ injected data register 1
    ADC3 $84 + constant ADC3_JDR2 ( read-only )  \ injected data register 2
    ADC3 $88 + constant ADC3_JDR3 ( read-only )  \ injected data register 3
    ADC3 $8C + constant ADC3_JDR4 ( read-only )  \ injected data register 4
    ADC3 $A0 + constant ADC3_AWD2CR ( read-write )  \ Analog Watchdog 2 Configuration  Register
    ADC3 $A4 + constant ADC3_AWD3CR ( read-write )  \ Analog Watchdog 3 Configuration  Register
    ADC3 $B0 + constant ADC3_DIFSEL (  )  \ Differential Mode Selection Register  2
    ADC3 $B4 + constant ADC3_CALFACT ( read-write )  \ Calibration Factors
  [then]

  execute-defined? use-GPIOA [if]
    $48000000 constant GPIOA ( General-purpose I/Os ) 
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

  [if]
    $48000400 constant GPIOB ( General-purpose I/Os ) 
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

  execute-defined? use-GPIOC [if]
    $48000800 constant GPIOC ( General-purpose I/Os ) 
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

  [if]
    $48000C00 constant GPIOD ( General-purpose I/Os ) 
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

  execute-defined? use-GPIOE [if]
    $48001000 constant GPIOE ( General-purpose I/Os ) 
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

  [if]
    $48001400 constant GPIOF ( General-purpose I/Os ) 
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

  execute-defined? use-GPIOG [if]
    $48001800 constant GPIOG ( General-purpose I/Os ) 
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

  [if]
    $48001C00 constant GPIOH ( General-purpose I/Os ) 
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

  execute-defined? use-TIM2 [if]
    $40000000 constant TIM2 ( General-purpose-timers ) 
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
    TIM2 $50 + constant TIM2_OR ( read-write )  \ TIM2 option register
  [then]

  [if]
    $40000400 constant TIM3 ( General-purpose-timers ) 
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
    TIM3 $50 + constant TIM3_OR ( read-write )  \ TIM2 option register
  [then]

  execute-defined? use-TIM4 [if]
    $40000800 constant TIM4 ( General-purpose-timers ) 
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
    TIM4 $50 + constant TIM4_OR ( read-write )  \ TIM2 option register
  [then]

  [if]
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
    TIM5 $50 + constant TIM5_OR ( read-write )  \ TIM2 option register
  [then]

  execute-defined? use-TIM1 [if]
    $40012C00 constant TIM1 ( Advanced-timers ) 
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
    TIM1 $30 + constant TIM1_RCR ( read-write )  \ repetition counter register
    TIM1 $34 + constant TIM1_CCR1 ( read-write )  \ capture/compare register 1
    TIM1 $38 + constant TIM1_CCR2 ( read-write )  \ capture/compare register 2
    TIM1 $3C + constant TIM1_CCR3 ( read-write )  \ capture/compare register 3
    TIM1 $40 + constant TIM1_CCR4 ( read-write )  \ capture/compare register 4
    TIM1 $44 + constant TIM1_BDTR ( read-write )  \ break and dead-time register
    TIM1 $48 + constant TIM1_DCR ( read-write )  \ DMA control register
    TIM1 $4C + constant TIM1_DMAR ( read-write )  \ DMA address for full transfer
    TIM1 $50 + constant TIM1_OR1 ( read-write )  \ DMA address for full transfer
    TIM1 $54 + constant TIM1_CCMR3_Output ( read-write )  \ capture/compare mode register 2 output  mode
    TIM1 $58 + constant TIM1_CCR5 ( read-write )  \ capture/compare register 4
    TIM1 $5C + constant TIM1_CCR6 ( read-write )  \ capture/compare register 4
    TIM1 $60 + constant TIM1_OR2 ( read-write )  \ DMA address for full transfer
    TIM1 $64 + constant TIM1_OR3 ( read-write )  \ DMA address for full transfer
  [then]

  [if]
    $40013400 constant TIM8 ( Advanced-timers ) 
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
    TIM8 $30 + constant TIM8_RCR ( read-write )  \ repetition counter register
    TIM8 $34 + constant TIM8_CCR1 ( read-write )  \ capture/compare register 1
    TIM8 $38 + constant TIM8_CCR2 ( read-write )  \ capture/compare register 2
    TIM8 $3C + constant TIM8_CCR3 ( read-write )  \ capture/compare register 3
    TIM8 $40 + constant TIM8_CCR4 ( read-write )  \ capture/compare register 4
    TIM8 $44 + constant TIM8_BDTR ( read-write )  \ break and dead-time register
    TIM8 $48 + constant TIM8_DCR ( read-write )  \ DMA control register
    TIM8 $4C + constant TIM8_DMAR ( read-write )  \ DMA address for full transfer
    TIM8 $50 + constant TIM8_OR1 ( read-write )  \ DMA address for full transfer
    TIM8 $54 + constant TIM8_CCMR3_Output ( read-write )  \ capture/compare mode register 2 output  mode
    TIM8 $58 + constant TIM8_CCR5 ( read-write )  \ capture/compare register 4
    TIM8 $5C + constant TIM8_CCR6 ( read-write )  \ capture/compare register 4
    TIM8 $60 + constant TIM8_OR2 ( read-write )  \ DMA address for full transfer
    TIM8 $64 + constant TIM8_OR3 ( read-write )  \ DMA address for full transfer
  [then]

  execute-defined? use-TIM6 [if]
    $40001000 constant TIM6 ( Basic-timers ) 
    TIM6 $0 + constant TIM6_CR1 ( read-write )  \ control register 1
    TIM6 $4 + constant TIM6_CR2 ( read-write )  \ control register 2
    TIM6 $C + constant TIM6_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM6 $10 + constant TIM6_SR ( read-write )  \ status register
    TIM6 $14 + constant TIM6_EGR ( write-only )  \ event generation register
    TIM6 $24 + constant TIM6_CNT ( read-write )  \ counter
    TIM6 $28 + constant TIM6_PSC ( read-write )  \ prescaler
    TIM6 $2C + constant TIM6_ARR ( read-write )  \ auto-reload register
  [then]

  [if]
    $40001400 constant TIM7 ( Basic-timers ) 
    TIM7 $0 + constant TIM7_CR1 ( read-write )  \ control register 1
    TIM7 $4 + constant TIM7_CR2 ( read-write )  \ control register 2
    TIM7 $C + constant TIM7_DIER ( read-write )  \ DMA/Interrupt enable register
    TIM7 $10 + constant TIM7_SR ( read-write )  \ status register
    TIM7 $14 + constant TIM7_EGR ( write-only )  \ event generation register
    TIM7 $24 + constant TIM7_CNT ( read-write )  \ counter
    TIM7 $28 + constant TIM7_PSC ( read-write )  \ prescaler
    TIM7 $2C + constant TIM7_ARR ( read-write )  \ auto-reload register
  [then]

  execute-defined? use-USART1 [if]
    $40013800 constant USART1 ( Universal synchronous asynchronous receiver  transmitter ) 
    USART1 $0 + constant USART1_CR1 ( read-write )  \ Control register 1
    USART1 $4 + constant USART1_CR2 ( read-write )  \ Control register 2
    USART1 $8 + constant USART1_CR3 ( read-write )  \ Control register 3
    USART1 $C + constant USART1_BRR ( read-write )  \ Baud rate register
    USART1 $10 + constant USART1_GTPR ( read-write )  \ Guard time and prescaler  register
    USART1 $14 + constant USART1_RTOR ( read-write )  \ Receiver timeout register
    USART1 $18 + constant USART1_RQR ( write-only )  \ Request register
    USART1 $1C + constant USART1_ISR ( read-only )  \ Interrupt & status  register
    USART1 $20 + constant USART1_ICR ( write-only )  \ Interrupt flag clear register
    USART1 $24 + constant USART1_RDR ( read-only )  \ Receive data register
    USART1 $28 + constant USART1_TDR ( read-write )  \ Transmit data register
  [then]

  [if]
    $40004400 constant USART2 ( Universal synchronous asynchronous receiver  transmitter ) 
    USART2 $0 + constant USART2_CR1 ( read-write )  \ Control register 1
    USART2 $4 + constant USART2_CR2 ( read-write )  \ Control register 2
    USART2 $8 + constant USART2_CR3 ( read-write )  \ Control register 3
    USART2 $C + constant USART2_BRR ( read-write )  \ Baud rate register
    USART2 $10 + constant USART2_GTPR ( read-write )  \ Guard time and prescaler  register
    USART2 $14 + constant USART2_RTOR ( read-write )  \ Receiver timeout register
    USART2 $18 + constant USART2_RQR ( write-only )  \ Request register
    USART2 $1C + constant USART2_ISR ( read-only )  \ Interrupt & status  register
    USART2 $20 + constant USART2_ICR ( write-only )  \ Interrupt flag clear register
    USART2 $24 + constant USART2_RDR ( read-only )  \ Receive data register
    USART2 $28 + constant USART2_TDR ( read-write )  \ Transmit data register
  [then]

  execute-defined? use-USART3 [if]
    $40004800 constant USART3 ( Universal synchronous asynchronous receiver  transmitter ) 
    USART3 $0 + constant USART3_CR1 ( read-write )  \ Control register 1
    USART3 $4 + constant USART3_CR2 ( read-write )  \ Control register 2
    USART3 $8 + constant USART3_CR3 ( read-write )  \ Control register 3
    USART3 $C + constant USART3_BRR ( read-write )  \ Baud rate register
    USART3 $10 + constant USART3_GTPR ( read-write )  \ Guard time and prescaler  register
    USART3 $14 + constant USART3_RTOR ( read-write )  \ Receiver timeout register
    USART3 $18 + constant USART3_RQR ( write-only )  \ Request register
    USART3 $1C + constant USART3_ISR ( read-only )  \ Interrupt & status  register
    USART3 $20 + constant USART3_ICR ( write-only )  \ Interrupt flag clear register
    USART3 $24 + constant USART3_RDR ( read-only )  \ Receive data register
    USART3 $28 + constant USART3_TDR ( read-write )  \ Transmit data register
  [then]

  [if]
    $40004C00 constant UART4 ( Universal synchronous asynchronous receiver  transmitter ) 
    UART4 $0 + constant UART4_CR1 ( read-write )  \ Control register 1
    UART4 $4 + constant UART4_CR2 ( read-write )  \ Control register 2
    UART4 $8 + constant UART4_CR3 ( read-write )  \ Control register 3
    UART4 $C + constant UART4_BRR ( read-write )  \ Baud rate register
    UART4 $10 + constant UART4_GTPR ( read-write )  \ Guard time and prescaler  register
    UART4 $14 + constant UART4_RTOR ( read-write )  \ Receiver timeout register
    UART4 $18 + constant UART4_RQR ( write-only )  \ Request register
    UART4 $1C + constant UART4_ISR ( read-only )  \ Interrupt & status  register
    UART4 $20 + constant UART4_ICR ( write-only )  \ Interrupt flag clear register
    UART4 $24 + constant UART4_RDR ( read-only )  \ Receive data register
    UART4 $28 + constant UART4_TDR ( read-write )  \ Transmit data register
  [then]

  execute-defined? use-UART5 [if]
    $40005000 constant UART5 ( Universal synchronous asynchronous receiver  transmitter ) 
    UART5 $0 + constant UART5_CR1 ( read-write )  \ Control register 1
    UART5 $4 + constant UART5_CR2 ( read-write )  \ Control register 2
    UART5 $8 + constant UART5_CR3 ( read-write )  \ Control register 3
    UART5 $C + constant UART5_BRR ( read-write )  \ Baud rate register
    UART5 $10 + constant UART5_GTPR ( read-write )  \ Guard time and prescaler  register
    UART5 $14 + constant UART5_RTOR ( read-write )  \ Receiver timeout register
    UART5 $18 + constant UART5_RQR ( write-only )  \ Request register
    UART5 $1C + constant UART5_ISR ( read-only )  \ Interrupt & status  register
    UART5 $20 + constant UART5_ICR ( write-only )  \ Interrupt flag clear register
    UART5 $24 + constant UART5_RDR ( read-only )  \ Receive data register
    UART5 $28 + constant UART5_TDR ( read-write )  \ Transmit data register
  [then]

  [if]
    $40013000 constant SPI1 ( Serial peripheral interface/Inter-IC  sound ) 
    SPI1 $0 + constant SPI1_CR1 ( read-write )  \ control register 1
    SPI1 $4 + constant SPI1_CR2 ( read-write )  \ control register 2
    SPI1 $8 + constant SPI1_SR (  )  \ status register
    SPI1 $C + constant SPI1_DR ( read-write )  \ data register
    SPI1 $10 + constant SPI1_CRCPR ( read-write )  \ CRC polynomial register
    SPI1 $14 + constant SPI1_RXCRCR ( read-only )  \ RX CRC register
    SPI1 $18 + constant SPI1_TXCRCR ( read-only )  \ TX CRC register
  [then]

  execute-defined? use-SPI2 [if]
    $40003800 constant SPI2 ( Serial peripheral interface/Inter-IC  sound ) 
    SPI2 $0 + constant SPI2_CR1 ( read-write )  \ control register 1
    SPI2 $4 + constant SPI2_CR2 ( read-write )  \ control register 2
    SPI2 $8 + constant SPI2_SR (  )  \ status register
    SPI2 $C + constant SPI2_DR ( read-write )  \ data register
    SPI2 $10 + constant SPI2_CRCPR ( read-write )  \ CRC polynomial register
    SPI2 $14 + constant SPI2_RXCRCR ( read-only )  \ RX CRC register
    SPI2 $18 + constant SPI2_TXCRCR ( read-only )  \ TX CRC register
  [then]

  [if]
    $40003C00 constant SPI3 ( Serial peripheral interface/Inter-IC  sound ) 
    SPI3 $0 + constant SPI3_CR1 ( read-write )  \ control register 1
    SPI3 $4 + constant SPI3_CR2 ( read-write )  \ control register 2
    SPI3 $8 + constant SPI3_SR (  )  \ status register
    SPI3 $C + constant SPI3_DR ( read-write )  \ data register
    SPI3 $10 + constant SPI3_CRCPR ( read-write )  \ CRC polynomial register
    SPI3 $14 + constant SPI3_RXCRCR ( read-only )  \ RX CRC register
    SPI3 $18 + constant SPI3_TXCRCR ( read-only )  \ TX CRC register
  [then]

  execute-defined? use-EXTI [if]
    $40010400 constant EXTI ( External interrupt/event  controller ) 
    EXTI $0 + constant EXTI_IMR1 ( read-write )  \ Interrupt mask register
    EXTI $4 + constant EXTI_EMR1 ( read-write )  \ Event mask register
    EXTI $8 + constant EXTI_RTSR1 ( read-write )  \ Rising Trigger selection  register
    EXTI $C + constant EXTI_FTSR1 ( read-write )  \ Falling Trigger selection  register
    EXTI $10 + constant EXTI_SWIER1 ( read-write )  \ Software interrupt event  register
    EXTI $14 + constant EXTI_PR1 ( read-write )  \ Pending register
    EXTI $20 + constant EXTI_IMR2 ( read-write )  \ Interrupt mask register
    EXTI $24 + constant EXTI_EMR2 ( read-write )  \ Event mask register
    EXTI $28 + constant EXTI_RTSR2 ( read-write )  \ Rising Trigger selection  register
    EXTI $2C + constant EXTI_FTSR2 ( read-write )  \ Falling Trigger selection  register
    EXTI $30 + constant EXTI_SWIER2 ( read-write )  \ Software interrupt event  register
    EXTI $34 + constant EXTI_PR2 ( read-write )  \ Pending register
  [then]

  [if]
    $40002800 constant RTC ( Real-time clock ) 
    RTC $0 + constant RTC_TR ( read-write )  \ time register
    RTC $4 + constant RTC_DR ( read-write )  \ date register
    RTC $8 + constant RTC_CR ( read-write )  \ control register
    RTC $C + constant RTC_ISR (  )  \ initialization and status  register
    RTC $10 + constant RTC_PRER ( read-write )  \ prescaler register
    RTC $14 + constant RTC_WUTR ( read-write )  \ wakeup timer register
    RTC $1C + constant RTC_ALRMAR ( read-write )  \ alarm A register
    RTC $20 + constant RTC_ALRMBR ( read-write )  \ alarm B register
    RTC $24 + constant RTC_WPR ( write-only )  \ write protection register
    RTC $28 + constant RTC_SSR ( read-only )  \ sub second register
    RTC $2C + constant RTC_SHIFTR ( write-only )  \ shift control register
    RTC $30 + constant RTC_TSTR ( read-only )  \ time stamp time register
    RTC $34 + constant RTC_TSDR ( read-only )  \ time stamp date register
    RTC $38 + constant RTC_TSSSR ( read-only )  \ timestamp sub second register
    RTC $3C + constant RTC_CALR ( read-write )  \ calibration register
    RTC $40 + constant RTC_TAMPCR ( read-write )  \ tamper configuration register
    RTC $44 + constant RTC_ALRMASSR ( read-write )  \ alarm A sub second register
    RTC $48 + constant RTC_ALRMBSSR ( read-write )  \ alarm B sub second register
    RTC $4C + constant RTC_OR ( read-write )  \ option register
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
    RTC $A0 + constant RTC_BKP20R ( read-write )  \ backup register
    RTC $A4 + constant RTC_BKP21R ( read-write )  \ backup register
    RTC $A8 + constant RTC_BKP22R ( read-write )  \ backup register
    RTC $AC + constant RTC_BKP23R ( read-write )  \ backup register
    RTC $B0 + constant RTC_BKP24R ( read-write )  \ backup register
    RTC $B4 + constant RTC_BKP25R ( read-write )  \ backup register
    RTC $B8 + constant RTC_BKP26R ( read-write )  \ backup register
    RTC $BC + constant RTC_BKP27R ( read-write )  \ backup register
    RTC $C0 + constant RTC_BKP28R ( read-write )  \ backup register
    RTC $C4 + constant RTC_BKP29R ( read-write )  \ backup register
    RTC $C8 + constant RTC_BKP30R ( read-write )  \ backup register
    RTC $CC + constant RTC_BKP31R ( read-write )  \ backup register
  [then]

  execute-defined? use-OTG_FS_GLOBAL [if]
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

  [if]
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

  execute-defined? use-OTG_FS_DEVICE [if]
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

  [if]
    $50000E00 constant OTG_FS_PWRCLK ( USB on the go full speed ) 
    OTG_FS_PWRCLK $0 + constant OTG_FS_PWRCLK_FS_PCGCCTL ( read-write )  \ OTG_FS power and clock gating control  register OTG_FS_PCGCCTL
  [then]

  execute-defined? use-NVIC [if]
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
    NVIC $450 + constant NVIC_IPR20 ( read-write )  \ Interrupt Priority Register
  [then]

end-module

end-compress-flash

compile-to-ram
