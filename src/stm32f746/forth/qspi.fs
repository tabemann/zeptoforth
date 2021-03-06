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

\ Set up the wordlist
forth-wordlist 1 set-order
forth-wordlist set-current

\ Compile this to flash
compile-to-flash

\ Check whether this is already defined
defined? qspi-wordlist not [if]

  \ Set up the wordlist
  forth-wordlist 1 set-order
  forth-wordlist set-current
  wordlist constant qspi-wordlist
  wordlist constant qspi-internal-wordlist
  forth-wordlist internal-wordlist qspi-internal-wordlist qspi-wordlist
  4 set-order
  qspi-internal-wordlist set-current

  \ Is mapping QSPI enabled
  variable map-qspi-enabled?

  \ Is QSPI initialized
  variable qspi-inited?

  \ GPIO base
  $40020000 constant GPIO_Base

  \ GPIOB base
  GPIO_Base $400 + constant GPIOB_Base

  \ GPIOD base
  GPIO_Base $400 3 * + constant GPIOD_Base

  \ GPIOE base
  GPIO_Base $400 4 * + constant GPIOE_Base

  \ GPIOB mode register
  GPIOB_Base $00 + constant GPIOB_MODER

  \ GPIOB output speed register
  GPIOB_Base $08 + constant GPIOB_OSPEEDR

  \ GPIOB pull-up/pull-down register
  GPIOB_Base $0C + constant GPIOB_PUPDR

  \ GPIOB lower alternate function register
  GPIOB_Base $20 + constant GPIOB_AFRL

  \ GPIOB higher alternate function register
  \ GPIOB_Base $24 + constant GPIOB_AFRH

  \ GPIOD mode register
  GPIOD_Base $00 + constant GPIOD_MODER

  \ GPIOD output speed register
  GPIOD_Base $08 + constant GPIOD_OSPEEDR

  \ GPIOD lower alternate function register
  \ GPIOD_Base $20 + constant GPIOD_AFRL

  \ GPIOD higher alternate function register
  GPIOD_Base $24 + constant GPIOD_AFRH

  \ GPIOE mode register
  GPIOE_Base $00 + constant GPIOE_MODER

  \ GPIOE output speed register
  GPIOE_Base $08 + constant GPIOE_OSPEEDR

  \ GPIOE lower alternate function register
  GPIOE_Base $20 + constant GPIOE_AFRL

  \ GPIOE higher alternate function register
  \ GPIOE_Base $24 + constant GPIOE_AFRH

  \ Alternate function
  %10 constant ALTERNATE_FUNCTION

  \ Pull up
  %01 constant PULL_UP

  \ Very high speech
  %11 constant VERY_HIGH_SPEED

  \ Set GPIOB_MODER field
  : GPIOB_MODER! ( mode pin -- )
    GPIOB_MODER @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or GPIOB_MODER !
  ;

  \ Set GPIOB_OSPEEDR field
  : GPIOB_OSPEEDR! ( ospeed pin -- )
    GPIOB_OSPEEDR @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or GPIOB_OSPEEDR !
  ;

  \ Set GPIOB_PUPDR field
  : GPIOB_PUPDR! ( pupd pin -- )
    GPIOB_PUPDR @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or GPIOB_PUPDR !
  ;

  \ Set GPIOB_AFRL field
  : GPIOB_AFRL! ( af pin -- )
    GPIOB_AFRL @ over %1111 swap 2 lshift lshift bic
    rot %1111 and rot 2 lshift lshift or GPIOB_AFRL !
  ;

  \ Set GPIOB_AFRH field
  \ : GPIOB_AFRH! ( af pin -- )
  \   8 - GPIOB_AFRH @ over %1111 swap 2 lshift lshift bic
  \   rot %1111 and rot 2 lshift lshift or GPIOB_AFRH !
  \ ;

  \ Set GPIOD_MODER field
  : GPIOD_MODER! ( mode pin -- )
    GPIOD_MODER @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or GPIOD_MODER !
  ;

  \ Set GPIOD_OSPEEDR field
  : GPIOD_OSPEEDR! ( ospeed pin -- )
    GPIOD_OSPEEDR @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or GPIOD_OSPEEDR !
  ;

  \ Set GPIOD_AFRL field
  \ : GPIOD_AFRL! ( af pin -- )
  \   GPIOD_AFRL @ over %1111 swap 2 lshift lshift bic
  \   rot %1111 and rot 2 lshift lshift or GPIOD_AFRL !
  \ ;

  \ Set GPIOD_AFRH field
  : GPIOD_AFRH! ( af pin -- )
    8 - GPIOD_AFRH @ over %1111 swap 2 lshift lshift bic
    rot %1111 and rot 2 lshift lshift or GPIOD_AFRH !
  ;

  \ Set GPIOE_MODER field
  : GPIOE_MODER! ( mode pin -- )
    GPIOE_MODER @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or GPIOE_MODER !
  ;

  \ Set GPIOE_OSPEEDR field
  : GPIOE_OSPEEDR! ( ospeed pin -- )
    GPIOE_OSPEEDR @ over %11 swap 1 lshift lshift bic
    rot %11 and rot 1 lshift lshift or GPIOE_OSPEEDR !
  ;

  \ Set GPIOE_AFRL field
  : GPIOE_AFRL! ( af pin -- )
    GPIOE_AFRL @ over %1111 swap 2 lshift lshift bic
    rot %1111 and rot 2 lshift lshift or GPIOE_AFRL !
  ;

  \ Set GPIOE_AFRH field
  \ : GPIOE_AFRH! ( af pin -- )
  \   8 - GPIOE_AFRH @ over %1111 swap 2 lshift lshift bic
  \   rot %1111 and rot 2 lshift lshift or GPIOE_AFRH !
  \ ;

  \ Quad SPI base
  $A0001000 constant QUADSPI_Base

  \ Quad SPI map base
  $90000000 constant QUADSPI_Map_Base

  \ Quad SPI control register
  QUADSPI_Base $00 + constant QUADSPI_CR

  \ Quad SPI device configuration register
  QUADSPI_Base $04 + constant QUADSPI_DCR

  \ Quad SPI status register
  QUADSPI_Base $08 + constant QUADSPI_SR

  \ Quad SPI flag clear register
  QUADSPI_Base $0C + constant QUADSPI_FCR

  \ Quad SPI data length register
  QUADSPI_Base $10 + constant QUADSPI_DLR

  \ Quad SPI communication configuration register
  QUADSPI_Base $14 + constant QUADSPI_CCR

  \ Quad SPI address register
  QUADSPI_Base $18 + constant QUADSPI_AR

  \ Quad SPI alternate bytes register
  QUADSPI_Base $1C + constant QUADSPI_ABR

  \ Quad SPI data register
  QUADSPI_Base $20 + constant QUADSPI_DR

  \ Quad SPI polling status mask register
  QUADSPI_Base $24 + constant QUADSPI_PSMKR

  \ Quad SPI polling status match register
  QUADSPI_Base $28 + constant QUADSPI_PSMAR

  \ Quad SPI polling internal register
  QUADSPI_Base $2C + constant QUADSPI_PIR

  \ Quad SPI low-power timeout register
  QUADSPI_Base $30 + constant QUADSPI_LPTR

  \ RCC base
  $40023800 constant RCC_Base

  \ RCC AHB1 peripheral clock enable register
  RCC_Base $30 + constant RCC_AHB1ENR

  \ RCC AHB1 peripheral clock enable in low-power mode register
  RCC_Base $50 + constant RCC_AHB1LPENR

  \ RCC AHB3 peripheral clock enable register
  RCC_Base $38 + constant RCC_AHB3ENR

  \ RCC AHB3 peripheral clock enable in low-power mode register
  RCC_Base $58 + constant RCC_AHB3LPENR

  \ Indirect write mode
  %00 constant MODE_INDIRECT_WRITE

  \ Indirect read mode
  %01 constant MODE_INDIRECT_READ

  \ Automatic polling mode
  %10 constant MODE_AUTO_POLL

  \ Memory-mapped mode
  %11 constant MODE_MEMORY_MAPPED

  \ 8-bit size
  %00 constant SIZE_8_BIT

  \ 16-bit size
  %01 constant SIZE_16_BIT

  \ 24-bit size
  %10 constant SIZE_24_BIT

  \ 32-bit size
  %11 constant SIZE_32_BIT

  \ No mode
  %00 constant MODE_NONE

  \ Single-line mode
  %01 constant MODE_1_LINE

  \ Two-line mode
  %10 constant MODE_2_LINE

  \ Four-line mode
  %11 constant MODE_4_LINE

  \ Set Quad SPI clock prescaler ( Fclk = Fahb / (PRESCALER + 1) )
  : QUADSPI_CR_PRESCALER! ( prescaler -- )
    QUADSPI_CR @ $FF 24 lshift bic swap $FF and 24 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI polling OR match mode
  : QUADSPI_CR_PMM! ( pmm -- )
    QUADSPI_CR @ 23 bit bic swap 0<> %1 and 23 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI automatic poll mode stop on match
  : QUADSPI_CR_APMS! ( apms -- )
    QUADSPI_CR @ 22 bit bic swap 0<> %1 and 22 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI timeout interrupt enabled
  : QUADSPI_CR_TOIE! ( enable -- )
    QUADSPI_CR @ 20 bit bic swap 0<> %1 and 20 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI status match interrupt enabled
  : QUADSPI_CR_SMIE! ( enable -- )
    QUADSPI_CR @ 19 bit bic swap 0<> %1 and 19 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI FIFO threshold interrupt enabled
  : QUADSPI_CR_FTIE! ( enable -- )
    QUADSPI_CR @ 18 bit bic swap 0<> %1 and 18 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI transfer complete interrupt enabled
  : QUADSPI_CR_TCIE! ( enable -- )
    QUADSPI_CR @ 18 bit bic swap 0<> %1 and 17 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI transfer error interrupt enabled
  : QUADSPI_CR_TEIE! ( enable -- )
    QUADSPI_CR @ 16 bit bic swap 0<> %1 and 16 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI FIFO threshold level ( FTHRES + 1 free bytes available to write )
  : QUADSPI_CR_FTHRES! ( fthres -- )
    QUADSPI_CR @ %11111 8 lshift bic swap %11111 and 8 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI flash memory selection
  : QUADSPI_CR_FSEL! ( index -- )
    QUADSPI_CR @ 7 bit bic swap 0<> %1 and 7 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI dual flash mode enabled
  : QUADSPI_CR_DFM! ( enable -- )
    QUADSPI_CR @ 6 bit bic swap 0<> %1 and 6 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI sample shift ( true = 1/2 cycle shift )
  : QUADSPI_CR_SSHIFT! ( sshift -- )
    QUADSPI_CR @ 4 bit bic swap 0<> %1 and 4 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI timeout counter enabled
  : QUADSPI_CR_TCEN! ( enable -- )
    QUADSPI_CR @ 3 bit bic swap 0<> %1 and 3 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI DMA enabled
  : QUADSPI_CR_DMAEN! ( enable -- )
    QUADSPI_CR @ 2 bit bic swap 0<> %1 and 2 lshift or QUADSPI_CR !
  ;

  \ Abort the on-going Quad SPI command sequence
  : QUADSPI_CR_ABORT! ( -- ) 1 bit QUADSPI_CR bis! ;

  \ Get whether abort has been requested for Quad SPI
  : QUADSPI_CR_ABORT@ ( -- abort ) 1 bit QUADSPI_CR bit@ ;

  \ Set Quad SPI enabled
  : QUADSPI_CR_EN! ( enable -- )
    QUADSPI_CR @ 0 bit bic swap 0<> %1 and 0 lshift or QUADSPI_CR !
  ;

  \ Set Quad SPI flash memory size ( size = 2^(FSIZE + 1) )
  : QUADSPI_DCR_FSIZE! ( fsize -- )
    QUADSPI_DCR @ %11111 16 lshift bic swap %11111 and 16 lshift or QUADSPI_DCR !
  ;

  \ Set Quad SPI chip select high time ( CLK cycles = CSHT + 1 )
  : QUADSPI_DCR_CSHT! ( csht -- )
    QUADSPI_DCR @ %111 8 lshift bic swap %111 and 8 lshift or QUADSPI_DCR !
  ;

  \ Set Quad SPI mode 0 ( CLK must stay low while nCS is high )
  : QUADSPI_DCR_CKMODE_0 ( -- ) 0 bit QUADSPI_DCR bic! ;

  \ Set Quad SPI mode 3 ( CLK must stay high while nCS is high )
  : QUADSPI_DCR_CKMODE_3 ( -- ) 0 bit QUADSPI_DCR bis! ;

  \ Get Quad SPI FIFO level
  : QUADSPI_SR_FLEVEL@ ( -- flevel ) QUADSPI_SR @ 8 rshift %111111 and ;

  \ Get Quad SPI FIFO threshold flag
  : QUADSPI_SR_FTF@ ( -- ftf ) 2 bit QUADSPI_SR bit@ ;

  \ Get Quad SPI busy bit
  : QUADSPI_SR_BUSY@ ( -- flag ) 5 bit QUADSPI_SR bit@ ;

  \ Clear Quad SPI timeout flag
  : QUADSPI_FCR_CTOF! ( -- ) 4 bit QUADSPI_FCR ! ;

  \ Clear Quad SPI status match flag
  : QUADSPI_FCR_CSMF! ( -- ) 3 bit QUADSPI_FCR ! ;

  \ Clear Quad SPI transfer complete flag
  : QUADSPI_FCR_CTCF! ( -- ) 1 bit QUADSPI_FCR ! ;

  \ Clear Quad SPI transfer error flag
  : QUADSPI_FCR_CTEF! ( -- ) 0 bit QUADSPI_FCR ! ;

  \ Set Quad SPI DDR mode
  : QUADSPI_CCR_DDRM! ( ddr -- )
    QUADSPI_CCR @ 31 bit bic swap 0<> %1 and 31 lshift or QUADSPI_CCR !
  ;

  \ Set Quad SPI send instruction only once mode
  : QUADSPI_CCR_SIOO! ( sioo -- )
    QUADSPI_CCR @ 28 bit bic swap 0<> %1 and 28 lshift or QUADSPI_CCR !
  ;

  \ Set Quad SPI functional mode
  : QUADSPI_CCR_FMODE! ( fmode -- )
    QUADSPI_CCR @ %11 26 lshift bic swap %11 and 26 lshift or QUADSPI_CCR !
  ;

  \ Set Quad SPI data mode
  : QUADSPI_CCR_DMODE! ( dmode -- )
    QUADSPI_CCR @ %11 24 lshift bic swap %11 and 24 lshift or QUADSPI_CCR !
  ;

  \ Set Quad SPI number of dummy cycles
  : QUADSPI_CCR_DCYC! ( dcyc -- )
    QUADSPI_CCR @ %11111 18 lshift bic swap %11111 and 18 lshift or QUADSPI_CCR !
  ;

  \ Set Quad SPI alternate bytes size
  : QUADSPI_CCR_ABSIZE! ( absize -- )
    QUADSPI_CCR @ %11 16 lshift bic swap %11 and 16 lshift or QUADSPI_CCR !
  ;

  \ Set Quad SPI alternate bytes mode
  : QUADSPI_CCR_ABMODE! ( abmode -- )
    QUADSPI_CCR @ %11 14 lshift bic swap %11 and 14 lshift or QUADSPI_CCR !
  ;

  \ Set Quad SPI address size
  : QUADSPI_CCR_ADSIZE! ( adsize -- )
    QUADSPI_CCR @ %11 12 lshift bic swap %11 and 12 lshift or QUADSPI_CCR !
  ;

  \ Set Quad SPI address mode
  : QUADSPI_CCR_ADMODE! ( admode -- )
    QUADSPI_CCR @ %11 10 lshift bic swap %11 and 10 lshift or QUADSPI_CCR !
  ;

  \ Set Quad SPI instruction mode
  : QUADSPI_CCR_IMODE! ( imode -- )
    QUADSPI_CCR @ %11 8 lshift bic swap %11 and 8 lshift or QUADSPI_CCR !
  ;

  \ Send instruction to external Quad SPI device
  : QUADSPI_CCR_INSTRUCTION! ( instruction -- )
    QUADSPI_CCR @ $FF bic swap $FF and or QUADSPI_CCR !
  ;

  \ Enable GPIOB clock
  : RCC_AHB1ENR_GPIOBEN! ( enable -- )
    RCC_AHB1ENR @ 1 bit bic swap 0<> %1 and 1 lshift or RCC_AHB1ENR !
  ;

  \ Enable GPIOD clock
  : RCC_AHB1ENR_GPIODEN! ( enable -- )
    RCC_AHB1ENR @ 3 bit bic swap 0<> %1 and 3 lshift or RCC_AHB1ENR !
  ;

  \ Enable GPIOE clock
  : RCC_AHB1ENR_GPIOEEN! ( enable -- )
    RCC_AHB1ENR @ 4 bit bic swap 0<> %1 and 4 lshift or RCC_AHB1ENR !
  ;

  \ Enable GPIOB low power clock
  : RCC_AHB1LPENR_GPIOBLPEN! ( enable -- )
    RCC_AHB1LPENR @ 1 bit bic swap 0<> %1 and 1 lshift or RCC_AHB1LPENR !
  ;

  \ Enable GPIOD low power clock
  : RCC_AHB1LPENR_GPIODLPEN! ( enable -- )
    RCC_AHB1LPENR @ 3 bit bic swap 0<> %1 and 3 lshift or RCC_AHB1LPENR !
  ;

  \ Enable GPIOE low power clock
  : RCC_AHB1LPENR_GPIOELPEN! ( enable -- )
    RCC_AHB1LPENR @ 4 bit bic swap 0<> %1 and 4 lshift or RCC_AHB1LPENR !
  ;

  \ Enable Quad SPI clock
  : RCC_AHB3ENR_QSPIEN! ( enable -- )
    RCC_AHB3ENR @ 1 bit bic swap 0<> %1 and 1 lshift or RCC_AHB3ENR !
  ;

  \ Enable Quad SPI clock in low-power mode
  : RCC_AHB3LPENR_QSPILPEN! ( enable -- )
    RCC_AHB3LPENR @ 1 bit bic swap 0<> %1 and 1 lshift or RCC_AHB3LPENR !
  ;

  \ MT25QL128ABA Operations

  \ Enable Quad SPI mode
  $35 constant QUADSPI_OP_ENTER_QUAD_IO_MODE

  \ Reset Quad SPI mode
  $F5 constant QUADSPI_OP_RESET_QUAD_IO_MODE

  \ Enable writing
  $06 constant QUADSPI_OP_WRITE_ENABLE

  \ Disable writing
  $04 constant QUADSPI_OP_WRITE_DISABLE

  \ Read status register
  $05 constant QUADSPI_OP_READ_STATUS_REG

  \ Read volatile configuration register
  $85 constant QUADSPI_OP_READ_VOLATILE_CFG_REG

  \ Write volatile configuration register
  $81 constant QUADSPI_OP_WRITE_VOLATILE_CFG_REG

  \ Read enhanced volatile configuration register
  $65 constant QUADSPI_OP_READ_ENH_VOLATILE_CFG_REG

  \ Write enhanced volatile configuration register
  $61 constant QUADSPI_OP_WRITE_ENH_VOLATILE_CFG_REG

  \ Set Quad output fast read
  $EB constant QUADSPI_OP_QUAD_INPUT_OUTPUT_FAST_READ

  \ Set Quad input fast program
  $32 constant QUADSPI_OP_QUAD_INPUT_FAST_PROGRAM

  \ 4K subsector erase
  $20 constant QUADSPI_OP_4K_SUBSECTOR_ERASE

  \ 32K subsector erase
  $52 constant QUADSPI_OP_32K_SUBSECTOR_ERASE

  \ Sector erase
  $D8 constant QUADSPI_OP_SECTOR_ERASE

  \ Bulk erase
  $60 constant QUADSPI_OP_BULK_ERASE

  \ Reset enable
  $66 constant QUADSPI_OP_RESET_ENABLE

  \ Reset memory
  $99 constant QUADSPI_OP_RESET_MEMORY

  \ Status register write enable bit
  $02 constant QUADSPI_WRITE_ENABLE

  \ Status register write in progress bit
  $01 constant QUADSPI_WRITE_IN_PROGRESS

  \ Nonvolatile configuration Quad SPI enable bit
  $7F constant QUADSPI_QUADSPI_ENABLE

  \ Nonvolatile configuration Quad SPI enable wait mask
  $80 constant QUADSPI_QUADSPI_ENABLE_WAIT_MASK

  \ Nonvolatile configuration Quad SPI enable wait bit
  $00 constant QUADSPI_QUADSPI_ENABLE_WAIT

  \ Dummy cycle position
  4 constant QUADSPI_DUMMY_CYCLE_Pos

  \ XIP disable
  $08 constant QUADSPI_XIP_DISABLE

  \ Wrap continuous
  $03 constant QUADSPI_WRAP_CONTINUOUS

  \ Volatile configuration register setting
  10 QUADSPI_DUMMY_CYCLE_Pos lshift QUADSPI_XIP_DISABLE or
  QUADSPI_WRAP_CONTINUOUS or constant QUADSPI_VOLATILE_CFG_REG_SETTING

  \ Quad SPI flash size
  1024 1024 * 16 * constant QUADSPI_SIZE

  \ Quad SPI page size
  256 constant QUADSPI_PAGE_SIZE
  
  \ Wait for Quad SPI to not be busy
  : wait-qspi-busy ( -- ) begin QUADSPI_SR_BUSY@ not until ;

  \ Wait for Quad SPI to not be aborting
  : wait-qspi-abort ( -- ) begin QUADSPI_CR_ABORT@ not until ;

  \ Wait for a register value
  : wait-qspi-reg ( mask match op -- )
    false QUADSPI_CR_EN!
    swap QUADSPI_PSMAR !
    swap QUADSPI_PSMKR !
    $10 QUADSPI_PIR !
    true QUADSPI_CR_APMS!
    0 QUADSPI_CCR_INSTRUCTION!
    MODE_AUTO_POLL QUADSPI_CCR_FMODE!
    MODE_4_LINE QUADSPI_CCR_IMODE!
    MODE_4_LINE QUADSPI_CCR_DMODE!
    MODE_NONE QUADSPI_CCR_ADMODE!
    0 QUADSPI_DLR ! \ 1 byte
    0 QUADSPI_CCR_DCYC!
    true QUADSPI_CR_EN!
    QUADSPI_CCR_INSTRUCTION!
    wait-qspi-busy
    QUADSPI_FCR_CSMF!
    MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
    MODE_NONE QUADSPI_CCR_DMODE!
    false QUADSPI_CR_EN!
  ;

  \ Wait for operation to complete
  : wait-qspi-write-in-progress ( -- )
    QUADSPI_WRITE_IN_PROGRESS $00 QUADSPI_OP_READ_STATUS_REG wait-qspi-reg
  ;

  \ Send a 4 line instruction op
  : send-qspi-op ( op -- )
    false QUADSPI_CR_EN!
    0 QUADSPI_CCR_INSTRUCTION!
    MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
    MODE_NONE QUADSPI_CCR_DMODE!
    MODE_NONE QUADSPI_CCR_ADMODE!
    MODE_4_LINE QUADSPI_CCR_IMODE!
    0 QUADSPI_CCR_DCYC!
    true QUADSPI_CR_EN!
    QUADSPI_CCR_INSTRUCTION!
    wait-qspi-busy
    false QUADSPI_CR_EN!
  ;

  \ Send a 4 line instruction op with address
  : send-qspi-op-addr ( addr op -- )
    false QUADSPI_CR_EN!
    0 QUADSPI_CCR_INSTRUCTION!
    MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
    MODE_NONE QUADSPI_CCR_DMODE!
    MODE_4_LINE QUADSPI_CCR_ADMODE!
    MODE_4_LINE QUADSPI_CCR_IMODE!
    0 QUADSPI_CCR_DCYC!
    true QUADSPI_CR_EN!
    QUADSPI_CCR_INSTRUCTION!
    QUADSPI_AR !
    wait-qspi-busy
    false QUADSPI_CR_EN!
  ;

  \ Send a 4 line instruction op with address and mass data
  : send-qspi-op-addr-data-mass ( data-addr count addr op -- )
    2 pick 0> if
      false QUADSPI_CR_EN!
      MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
      MODE_4_LINE QUADSPI_CCR_IMODE!
      MODE_4_LINE QUADSPI_CCR_ADMODE!
      MODE_4_LINE QUADSPI_CCR_DMODE!
      0 QUADSPI_CCR_DCYC!
      2 pick 1 - QUADSPI_DLR !
      0 QUADSPI_CCR_INSTRUCTION!
      true QUADSPI_CR_EN!
      QUADSPI_CCR_INSTRUCTION!
      QUADSPI_AR !
      begin dup 4 >= while
	QUADSPI_SR_FTF@ if
	  4 - swap dup @ QUADSPI_DR ! cell+ swap
	then
      repeat
      begin dup 0 > while
	QUADSPI_SR_FTF@ if
	  1 - swap dup b@ QUADSPI_DR b! 1+ swap
	then
      repeat
      2drop
      wait-qspi-busy
      false QUADSPI_CR_EN!
      0 QUADSPI_DLR !
    then
  ;  

  \ Send a 4 line instruction op with address and 32-bit data
  : send-qspi-op-addr-data-32 ( data addr op -- )
    false QUADSPI_CR_EN!
    MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
    MODE_4_LINE QUADSPI_CCR_IMODE!
    MODE_4_LINE QUADSPI_CCR_ADMODE!
    MODE_4_LINE QUADSPI_CCR_DMODE!
    0 QUADSPI_CCR_DCYC!
    3 QUADSPI_DLR ! \ 4 bytes
    0 QUADSPI_CCR_INSTRUCTION!
    true QUADSPI_CR_EN!
    QUADSPI_CCR_INSTRUCTION!
    QUADSPI_AR !
    QUADSPI_DR !
    wait-qspi-busy
    false QUADSPI_CR_EN!
    0 QUADSPI_DLR !
  ;

  \ Send a 4 line instruction op with address and 16-bit data
  : send-qspi-op-addr-data-16 ( data addr op -- )
    false QUADSPI_CR_EN!
    MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
    MODE_4_LINE QUADSPI_CCR_IMODE!
    MODE_4_LINE QUADSPI_CCR_ADMODE!
    MODE_4_LINE QUADSPI_CCR_DMODE!
    0 QUADSPI_CCR_DCYC!
    1 QUADSPI_DLR ! \ 2 bytes
    0 QUADSPI_CCR_INSTRUCTION!
    true QUADSPI_CR_EN!
    QUADSPI_CCR_INSTRUCTION!
    QUADSPI_AR !
    QUADSPI_DR h!
    wait-qspi-busy
    false QUADSPI_CR_EN!
    0 QUADSPI_DLR !
  ;

  \ Send a 4 line instruction op with address and 8-bit data
  : send-qspi-op-addr-data-8 ( data addr op -- )
    false QUADSPI_CR_EN!
    MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
    MODE_4_LINE QUADSPI_CCR_IMODE!
    MODE_4_LINE QUADSPI_CCR_ADMODE!
    MODE_4_LINE QUADSPI_CCR_DMODE!
    0 QUADSPI_CCR_DCYC!
    0 QUADSPI_DLR ! \ 1 bytes
    0 QUADSPI_CCR_INSTRUCTION!
    true QUADSPI_CR_EN!
    QUADSPI_CCR_INSTRUCTION!
    QUADSPI_AR !
    QUADSPI_DR b!
    wait-qspi-busy
    false QUADSPI_CR_EN!
  ;

  \ Enable writes to the Quad SPI flash
  : enable-qspi-write ( -- )
    QUADSPI_OP_WRITE_ENABLE send-qspi-op
    QUADSPI_OP_READ_STATUS_REG QUADSPI_WRITE_ENABLE QUADSPI_WRITE_ENABLE
    wait-qspi-reg
  ;

  \ Wait for a register value with o ne wire
  : wait-qspi-reg-1-line ( mask match op -- )
    false QUADSPI_CR_EN!
    swap QUADSPI_PSMAR !
    swap QUADSPI_PSMKR !
    $10 QUADSPI_PIR !
    true QUADSPI_CR_APMS!
    0 QUADSPI_CCR_INSTRUCTION!
    MODE_AUTO_POLL QUADSPI_CCR_FMODE!
    MODE_1_LINE QUADSPI_CCR_IMODE!
    MODE_1_LINE QUADSPI_CCR_DMODE!
    MODE_NONE QUADSPI_CCR_ADMODE!
    0 QUADSPI_DLR ! \ 1 byte
    0 QUADSPI_CCR_DCYC!
    true QUADSPI_CR_EN!
    QUADSPI_CCR_INSTRUCTION!
    wait-qspi-busy
    QUADSPI_FCR_CSMF!
    MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
    MODE_NONE QUADSPI_CCR_DMODE!
    false QUADSPI_CR_EN!
  ;

  \ Send a 1 line instruction op
  : send-qspi-op-1-line ( op -- )
    false QUADSPI_CR_EN!
    0 QUADSPI_CCR_INSTRUCTION!
    MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
    MODE_NONE QUADSPI_CCR_DMODE!
    MODE_NONE QUADSPI_CCR_ADMODE!
    MODE_1_LINE QUADSPI_CCR_IMODE!
    0 QUADSPI_CCR_DCYC!
    true QUADSPI_CR_EN!
    QUADSPI_CCR_INSTRUCTION!
    wait-qspi-busy
    false QUADSPI_CR_EN!
  ;

  \ Send a 1 line instruction and data op
  : send-qspi-op-data-8-1-line ( data op -- )
    false QUADSPI_CR_EN!
    0 QUADSPI_CCR_INSTRUCTION!
    MODE_INDIRECT_WRITE QUADSPI_CCR_FMODE!
    MODE_1_LINE QUADSPI_CCR_DMODE!
    MODE_NONE QUADSPI_CCR_ADMODE!
    MODE_1_LINE QUADSPI_CCR_IMODE!
    0 QUADSPI_CCR_DCYC!
    0 QUADSPI_DLR ! \ 1 byte
    true QUADSPI_CR_EN!
    QUADSPI_CCR_INSTRUCTION!
    QUADSPI_DR b!
    wait-qspi-busy
    false QUADSPI_CR_EN!
  ;

  \ Enable writes to the QSPI SPI flash with one wire
  : enable-qspi-write-1-line ( -- )
    QUADSPI_OP_WRITE_ENABLE send-qspi-op-1-line
    QUADSPI_WRITE_ENABLE QUADSPI_WRITE_ENABLE QUADSPI_OP_READ_STATUS_REG
    wait-qspi-reg-1-line
  ;

  \ Register assertion failure exception
  : reg-assert-fail ( -- ) space ." register assertion failure" ;

  \ Test bits
  : bit-assert ( tested mask bitshift value -- )
    swap >r over and r@ lshift swap r> lshift rot and = averts reg-assert-fail
  ;

  \ Initialize GPIO settings
  : init-qspi-gpio ( -- )
    \ QUADSPI_CLK PB2 AF9 *
    \ QUADSPI_BK1_NCS PB6 AF10 *
    \ QUADSPI_BK1_IO0 PC9 AF9
    \ QUADSPI_BK1_IO1 PC10 AF9
    \ QUADSPI_BK2_NCS PC11 AF9
    \ QUADSPI_BK1_IO0 PD11 AF9 *
    \ QUADSPI_BK1_IO1 PD12 AF9 *
    \ QUADSPI_BK1_IO3 PD13 AF9 *
    \ QUADSPI_BK1_IO2 PE2 AF9 *
    \ QUADSPI_BK2_IO0 PE7 AF10
    \ QUADSPI_BK2_IO1 PE8 AF10
    \ QUADSPI_BK2_IO2 PE9 AF10
    \ QUADSPI_BK2_IO3 PE10 AF10
    \ QUADSPI_BK1_IO3 PF6 AF9
    \ QUADSPI_BK1_IO2 PF7 AF9
    \ QUADSPI_BK1_IO0 PF8 AF10
    \ QUADSPI_BK1_IO1 PF9 AF10
    \ QUADSPI_BK2_IO2 PG9 AF9
    \ QUADSPI_BK2_IO3 PG14 AF9
    \ QUADSPI_BK2_IO0 PH2 AF9
    \ QUADSPI_BK2_IO1 PH3 AF9
    true RCC_AHB1ENR_GPIOBEN!
    RCC_AHB1ENR @ %1 1 %1 bit-assert
    true RCC_AHB1ENR_GPIODEN!
    RCC_AHB1ENR @ %1 3 %1 bit-assert
    true RCC_AHB1ENR_GPIOEEN!
    RCC_AHB1ENR @ %1 4 %1 bit-assert
    true RCC_AHB1LPENR_GPIOBLPEN!
    RCC_AHB1LPENR @ %1 1 %1 bit-assert
    true RCC_AHB1LPENR_GPIODLPEN!
    RCC_AHB1LPENR @ %1 3 %1 bit-assert
    true RCC_AHB1LPENR_GPIOELPEN!
    RCC_AHB1LPENR @ %1 4 %1 bit-assert
    true RCC_AHB3ENR_QSPIEN!
    RCC_AHB3ENR @ %1 1 %1 bit-assert
    true RCC_AHB3LPENR_QSPILPEN!
    RCC_AHB3LPENR @ %1 1 %1 bit-assert
    ALTERNATE_FUNCTION 2 GPIOB_MODER!
    GPIOB_MODER @ %11 2 2 * ALTERNATE_FUNCTION bit-assert
    9 2 GPIOB_AFRL!
    GPIOB_AFRL @ %1111 4 2 * 9 bit-assert
    VERY_HIGH_SPEED 2 GPIOB_OSPEEDR!
    GPIOB_OSPEEDR @ %11 2 2 * VERY_HIGH_SPEED bit-assert
    ALTERNATE_FUNCTION 6 GPIOB_MODER!
    GPIOB_MODER @ %11 2 6 * ALTERNATE_FUNCTION bit-assert
    10 6 GPIOB_AFRL!
    GPIOB_AFRL @ %1111 4 6 * 10 bit-assert
    PULL_UP 6 GPIOB_PUPDR!
    GPIOB_PUPDR @ %11 2 6 * PULL_UP bit-assert
    VERY_HIGH_SPEED 6 GPIOB_OSPEEDR!
    GPIOB_OSPEEDR @ %11 2 6 * VERY_HIGH_SPEED bit-assert
    ALTERNATE_FUNCTION 11 GPIOD_MODER!
    GPIOD_MODER @ %11 2 11 * ALTERNATE_FUNCTION bit-assert
    9 11 GPIOD_AFRH!
    GPIOD_AFRH @ %1111 4 3 * 9 bit-assert
    VERY_HIGH_SPEED 11 GPIOD_OSPEEDR!
    GPIOD_OSPEEDR @ %11 2 11 * VERY_HIGH_SPEED bit-assert
    ALTERNATE_FUNCTION 12 GPIOD_MODER!
    GPIOD_MODER @ %11 2 12 * ALTERNATE_FUNCTION bit-assert
    9 12 GPIOD_AFRH!
    GPIOD_AFRH @ %1111 4 4 * 9 bit-assert
    VERY_HIGH_SPEED 12 GPIOD_OSPEEDR!
    GPIOD_OSPEEDR @ %11 2 12 * VERY_HIGH_SPEED bit-assert
    ALTERNATE_FUNCTION 13 GPIOD_MODER!
    GPIOD_MODER @ %11 2 13 * ALTERNATE_FUNCTION bit-assert
    9 13 GPIOD_AFRH!
    GPIOD_AFRH @ %1111 4 5 * 9 bit-assert
    VERY_HIGH_SPEED 13 GPIOD_OSPEEDR!
    GPIOD_OSPEEDR @ %11 2 13 * VERY_HIGH_SPEED bit-assert
    ALTERNATE_FUNCTION 2 GPIOE_MODER!
    GPIOE_MODER @ %11 2 2 * ALTERNATE_FUNCTION bit-assert
    9 2 GPIOE_AFRL!
    GPIOE_AFRL @ %1111 4 2 * 9 bit-assert
    VERY_HIGH_SPEED 2 GPIOE_OSPEEDR!
    GPIOE_OSPEEDR @ %11 2 2 * VERY_HIGH_SPEED bit-assert
  ;

  \ Initialize basic SPI settings
  : init-qspi-basic ( -- )
    23 QUADSPI_DCR_FSIZE!
    MODE_1_LINE QUADSPI_CCR_IMODE!
    MODE_NONE QUADSPI_CCR_ADMODE!
    MODE_NONE QUADSPI_CCR_ABMODE!
    MODE_NONE QUADSPI_CCR_DMODE!
    SIZE_24_BIT QUADSPI_CCR_ADSIZE!
    SIZE_8_BIT QUADSPI_CCR_ABSIZE!
    0 QUADSPI_DLR !
    true QUADSPI_CR_SSHIFT!
    2 QUADSPI_CR_PRESCALER! \ Divide AHB CLK by 3
    3 QUADSPI_CR_FTHRES! \ Set FIFO threshold to 4 or more free bytes
    0 QUADSPI_CR_FSEL!
    false QUADSPI_CR_DFM!
    false QUADSPI_CR_TOIE!
    false QUADSPI_CR_SMIE!
    false QUADSPI_CR_FTIE!
    false QUADSPI_CR_TCIE!
    false QUADSPI_CR_TEIE!
    false QUADSPI_CR_DMAEN!
    false QUADSPI_CR_TCEN!
  ;

  \ Initialize Quad SPI
  : init-qspi ( -- )
    qspi-inited? not if
      init-qspi-gpio
      init-qspi-basic
      QUADSPI_OP_RESET_QUAD_IO_MODE send-qspi-op
      QUADSPI_OP_RESET_ENABLE send-qspi-op-1-line
      QUADSPI_OP_RESET_MEMORY send-qspi-op-1-line
      enable-qspi-write-1-line
      QUADSPI_VOLATILE_CFG_REG_SETTING
      QUADSPI_OP_WRITE_VOLATILE_CFG_REG send-qspi-op-data-8-1-line
      $FF QUADSPI_VOLATILE_CFG_REG_SETTING
      QUADSPI_OP_READ_VOLATILE_CFG_REG wait-qspi-reg-1-line
      enable-qspi-write-1-line
      QUADSPI_QUADSPI_ENABLE QUADSPI_OP_WRITE_ENH_VOLATILE_CFG_REG
      send-qspi-op-data-8-1-line
      QUADSPI_QUADSPI_ENABLE_WAIT_MASK QUADSPI_QUADSPI_ENABLE_WAIT
      QUADSPI_OP_READ_ENH_VOLATILE_CFG_REG wait-qspi-reg-1-line
      QUADSPI_OP_ENTER_QUAD_IO_MODE send-qspi-op-1-line
      true qspi-inited? !
    then
  ;

  \ Enter memory-mapped mode
  : map-qspi ( -- )
    false QUADSPI_CR_EN!
    MODE_MEMORY_MAPPED QUADSPI_CCR_FMODE!
    MODE_4_LINE QUADSPI_CCR_ADMODE!
    MODE_4_LINE QUADSPI_CCR_DMODE!
    MODE_4_LINE QUADSPI_CCR_IMODE!
    0 QUADSPI_DLR !
    10 QUADSPI_CCR_DCYC!
    QUADSPI_OP_QUAD_INPUT_OUTPUT_FAST_READ QUADSPI_CCR_INSTRUCTION!
    true QUADSPI_CR_EN!
    true map-qspi-enabled? !
  ;

  \ Set current wordlist to the Forth wordlist
  forth-wordlist set-current

  : init ( -- )
    init
    false qspi-inited? !
    false map-qspi-enabled? !
    init-qspi
    map-qspi
  ;

  \ Set current wordlist to the QSPI wordlist
  qspi-wordlist set-current

  \ Quad SPI address validation exception
  : x-invalid-qspi-addr ( -- ) space ." invalid qspi address" ;

  \ Set the current wordlist to the QSPI internal wordlist
  qspi-internal-wordlist set-current

  \ Validate a QSPI address
  : validate-qspi-addr ( addr -- )
    dup QUADSPI_Map_Base u>= averts x-invalid-qspi-addr
    QUADSPI_Map_Base QUADSPI_SIZE + u< averts x-invalid-qspi-addr
  ;
  
  \ Set current wordlist to the QSPI wordlist
  qspi-wordlist set-current

  \ Write a word to Quad SPI flash
  : qspi! ( x addr -- )
    dup validate-qspi-addr
    QUADSPI_Map_Base -
    disable-int dsb isb
    enable-qspi-write
    QUADSPI_OP_QUAD_INPUT_FAST_PROGRAM send-qspi-op-addr-data-32
    wait-qspi-write-in-progress
    map-qspi-enabled? @ if map-qspi then
    enable-int
  ;

  \ Write a halfword to Quad SPI flash
  : hqspi! ( h addr -- )
    dup validate-qspi-addr
    QUADSPI_Map_Base -
    disable-int dsb isb
    enable-qspi-write
    QUADSPI_OP_QUAD_INPUT_FAST_PROGRAM send-qspi-op-addr-data-16
    wait-qspi-write-in-progress
    map-qspi-enabled? @ if map-qspi then
    enable-int
  ;

  \ Write a byte to Quad SPI flash
  : bqspi! ( b addr -- )
    dup validate-qspi-addr
    QUADSPI_Map_Base -
    disable-int dsb isb
    enable-qspi-write
    QUADSPI_OP_QUAD_INPUT_FAST_PROGRAM send-qspi-op-addr-data-8
    wait-qspi-write-in-progress
    map-qspi-enabled? @ if map-qspi then
    enable-int
  ;

  \ Write a buffer to Quad SPI flash
  : mass-qspi! ( data-addr bytes addr -- )
    dup validate-qspi-addr
    QUADSPI_Map_Base -
    disable-int dsb isb
    begin over 0 > while
      2 pick 2 pick 2 pick dup QUADSPI_PAGE_SIZE align swap -
      dup 0= if QUADSPI_PAGE_SIZE + then
      min dup >r 2 pick
      enable-qspi-write
      QUADSPI_OP_QUAD_INPUT_FAST_PROGRAM send-qspi-op-addr-data-mass
      wait-qspi-write-in-progress
      rot r@ + rot r@ - rot r> +
    repeat
    2drop drop
    map-qspi-enabled? @ if map-qspi then
    enable-int
  ;

  \ Erase a 4K subsector on the Quad SPI flash
  : erase-qspi-4k-subsector ( addr -- )
    dup validate-qspi-addr
    QUADSPI_Map_Base -
    disable-int dsb isb
    enable-qspi-write
    QUADSPI_OP_4K_SUBSECTOR_ERASE send-qspi-op-addr
    wait-qspi-write-in-progress
    map-qspi-enabled? @ if map-qspi then
    enable-int
  ;

  \ Erase a 32K subsector on the Quad SPI flash
  : erase-qspi-32k-subsector ( addr -- )
    dup validate-qspi-addr
    QUADSPI_Map_Base -
    disable-int dsb isb
    enable-qspi-write
    QUADSPI_OP_32K_SUBSECTOR_ERASE send-qspi-op-addr
    wait-qspi-write-in-progress
    map-qspi-enabled? @ if map-qspi then
    enable-int
  ;

  \ Erase a sector on the Quad SPI flash
  : erase-qspi-sector ( addr -- )
    dup validate-qspi-addr
    QUADSPI_Map_Base -
    disable-int dsb isb
    enable-qspi-write
    QUADSPI_OP_SECTOR_ERASE send-qspi-op-addr
    wait-qspi-write-in-progress
    map-qspi-enabled? @ if map-qspi then
    enable-int
  ;

  \ Bulk erase a Quad SPI flash
  : erase-qspi-bulk ( -- )
    disable-int dsb isb
    enable-qspi-write
    QUADSPI_OP_BULK_ERASE send-qspi-op
    wait-qspi-write-in-progress
    map-qspi-enabled? @ if map-qspi then
    enable-int
  ;

  \ Get whether QSPI is initialized
  : qspi-inited? ( -- flag ) qspi-inited? @ ;

  \ Get whether mapping QSPI is enabled
  : map-qspi-enabled? ( -- flag ) map-qspi-enabled? @ ;

  \ Get base Quad SPI address
  : qspi-base ( -- addr ) QUADSPI_Map_Base ;

  \ Get Quad SPI flash size
  : qspi-size ( -- bytes ) QUADSPI_SIZE ;

[then]

\ Reboot
reboot