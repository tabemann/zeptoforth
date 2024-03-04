\ Copyright (c) 2023 Travis Bemann
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

begin-module dma

  \ Out of range DMA index
  : x-out-of-range-dma-channel ( -- ) cr ." out of range DMA channel" ;

  \ Out of range transfer request
  : x-out-of-range-treq ( -- ) cr ." out of range transfer request" ;
  
  \ Out of range timer
  : x-out-of-range-timer ( -- ) cr ." out of range timer" ;
  
  \ Invalid transfer size
  : x-invalid-transfer-size ( -- ) cr ." invalid DMA transfer size" ;

  \ Out of range timer dividend or divisor
  : x-out-of-range-timer-value ( -- )
    cr ." out of range timer dividend or divisor"
  ;

  begin-module dma-internal

    \ DMA channel count
    12 constant dma-count

    \ Transfer request count
    $40 constant treq-count

    \ Timer count
    4 constant timer-count
    
    \ DMA base address
    $50000000 constant DMA_BASE

    \ Read address
    : CH_READ_ADDR ( channel -- addr ) 6 lshift [ DMA_BASE $00 + ] literal + ;

    \ Write address
    : CH_WRITE_ADDR ( channel -- addr ) 6 lshift [ DMA_BASE $04 + ] literal + ;

    \ Transfer count
    : CH_TRANS_COUNT ( channel -- addr ) 6 lshift [ DMA_BASE $08 + ] literal + ;

    \ Control and status
    : CH_CTRL_TRIG ( channel -- addr ) 6 lshift [ DMA_BASE $0C + ] literal + ;

    \ Timer
    : DMA_TIMER ( timer -- ) 2 lshift [ DMA_BASE $420 + ] literal + ;

    \ Abort register
    : CHAN_ABORT ( -- ) [ DMA_BASE $444 + ] literal ;
    
    \ Busy bit
    24 bit constant CH_CTRL_TRIG_BUSY

    \ Transfer request LSB
    15 constant CH_CTRL_TRIG_TREQ_SEL_LSB

    \ Chan to LSB
    11 constant CH_CTRL_TRIG_CHAIN_TO_LSB
    
    \ Increment write bit
    5 bit constant CH_CTRL_TRIG_INCR_WRITE

    \ Increment read bit
    4 bit constant CH_CTRL_TRIG_INCR_READ

    \ Data size LSB
    2 constant CH_CTRL_TRIG_DATA_SIZE_LSB
    
    \ Byte transfer
    $0 CH_CTRL_TRIG_DATA_SIZE_LSB lshift constant CH_CTRL_TRIG_SIZE_BYTE

    \ Halfword transfer
    $1 CH_CTRL_TRIG_DATA_SIZE_LSB lshift constant CH_CTRL_TRIG_SIZE_HALFWORD

    \ Word transfer
    $2 CH_CTRL_TRIG_DATA_SIZE_LSB lshift constant CH_CTRL_TRIG_SIZE_WORD
    
    \ DMA channel enable bit
    0 bit constant CH_CTRL_TRIG_EN
    
    \ Validate DMA channel index
    : validate-dma ( channel -- )
      dma-count u< averts x-out-of-range-dma-channel
    ;

    \ Validate transfer request
    : validate-treq ( treq -- )
      treq-count u< averts x-out-of-range-treq
    ;

    \ Validate timer
    : validate-timer ( timer -- )
      timer u< averts x-out-of-range-timer
    ;
    
    \ Get bits for transfer size
    : bits-of-transfer-size ( size -- bit )
      case
        1 of CH_CTRL_TRIG_SIZE_BYTE endof
        2 of CH_CTRL_TRIG_SIZE_HALFWORD endof
        4 of CH_CTRL_TRIG_SIZE_WORD endof
        ['] x-invalid-transfer-size ?raise
      endcase
    ;
    
  end-module> import

  \ System DREQ table

  \ PIO TX DREQ
  : DREQ_PIO_TX ( sm pio -- dreq ) 3 lshift + ;

  \ PIO RX DREQ
  : DREQ_PIO_RX ( sm pio -- dreq ) 3 lshift + 4 + ;

  \ SPI TX DREQ
  : DREQ_SPI_TX ( spi -- dreq ) 1 lshift 16 + ;

  \ SPI RX DREQ
  : DREQ_SPI_RX ( spi -- dreq ) 1 lshift 17 + ;

  \ UART TX DREQ
  : DREQ_UART_TX ( uart -- dreq ) 1 lshift 20 + ;

  \ UART RX DREQ
  : DREQ_UART_RX ( uart -- dreq ) 1 lshift 21 + ;

  \ PWM DREQ
  : DREQ_PWM_WRAP ( pwm -- dreq ) 24 + ;

  \ I2C TX DREQ
  : DREQ_I2C_TX ( i2c -- dreq ) 1 lshift 32 + ;

  \ I2C RX DREQ
  : DREQ_I2C_RX ( i2c -- dreq ) 1 lshift 33 + ;

  \ ADC DREQ
  : DREQ_ADC ( -- dreq ) 36 ;

  \ XIP STREAM DREQ
  : DREQ_XIP_STREAM ( -- dreq ) 37 ;

  \ XIP SSITX DREQ
  : DREQ_XIP_SSITX ( -- dreq ) 38 ;

  \ XIP SSIRX DREQ
  : DREQ_XIP_SSIRX ( -- dreq ) 39 ;

  \ Timer 0-3 as TREQ
  : TREQ_TIMER ( timer -- treq ) $3B + ;

  \ Unpaced transfer
  : TREQ_UNPACED ( -- treq ) $3F ;

  \ Start register to register transfer
  : start-register>register-dma { src dest count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    CH_CTRL_TRIG_EN or channel CH_CTRL_TRIG !
  ;

  \ Start register to buffer transfer
  : start-register>buffer-dma { src dest count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    [ CH_CTRL_TRIG_INCR_WRITE CH_CTRL_TRIG_EN or ] literal or
    channel CH_CTRL_TRIG !
  ;

  \ Start buffer to register transfer
  : start-buffer>register-dma { src dest count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    [ CH_CTRL_TRIG_INCR_READ CH_CTRL_TRIG_EN or ] literal or
    channel CH_CTRL_TRIG !
  ;

  \ Start buffer to buffer transfer
  : start-buffer>buffer-dma { src dest count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    [ CH_CTRL_TRIG_INCR_WRITE CH_CTRL_TRIG_INCR_READ or
    CH_CTRL_TRIG_EN or ] literal or
    channel CH_CTRL_TRIG !
  ;

  \ Prepare register to register transfer
  : prepare-register>register-dma { src dest count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    CH_CTRL_TRIG_EN or channel CH_CTRL_TRIG !
  ;

  \ Prepare register to buffer transfer
  : prepare-register>buffer-dma { src dest count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    CH_CTRL_TRIG_INCR_WRITE or
    channel CH_CTRL_TRIG !
  ;

  \ Prepare buffer to register transfer
  : prepare-buffer>register-dma { src dest count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    CH_CTRL_TRIG_INCR_READ or
    channel CH_CTRL_TRIG !
  ;

  \ Prepare buffer to buffer transfer
  : prepare-buffer>buffer-dma { src dest count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    [ CH_CTRL_TRIG_INCR_WRITE CH_CTRL_TRIG_INCR_READ or ] literal or
    channel CH_CTRL_TRIG !
  ;

  \ Start register to register transfer with chaining
  : start-register>register-dma-with-chain
    { chain-to src dest count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    CH_CTRL_TRIG_EN or channel CH_CTRL_TRIG !
  ;

  \ Start register to buffer transfer with chaining
  : start-register>buffer-dma-with-chain
    { chain-to src dest count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    [ CH_CTRL_TRIG_INCR_WRITE CH_CTRL_TRIG_EN or ] literal or
    channel CH_CTRL_TRIG !
  ;

  \ Start buffer to register transfer with chaining
  : start-buffer>register-dma-with-chain
    { chain-to src dest count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    [ CH_CTRL_TRIG_INCR_READ CH_CTRL_TRIG_EN or ] literal or
    channel CH_CTRL_TRIG !
  ;

  \ Start buffer to buffer transfer with chaining
  : start-buffer>buffer-dma-with-chain
    { chain-to src dest count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    [ CH_CTRL_TRIG_INCR_WRITE CH_CTRL_TRIG_INCR_READ or
    CH_CTRL_TRIG_EN or ] literal or
    channel CH_CTRL_TRIG !
  ;

  \ Prepare register to register transfer with chaining
  : prepare-register>register-dma-with-chain
    { chain-to src dest count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    CH_CTRL_TRIG_EN or channel CH_CTRL_TRIG !
  ;

  \ Prepare register to buffer transfer with chaining
  : prepare-register>buffer-dma-with-chain
    { chain-to src dest count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    CH_CTRL_TRIG_INCR_WRITE or
    channel CH_CTRL_TRIG !
  ;

  \ Prepare buffer to register transfer with chaining
  : prepare-buffer>register-dma-with-chain
    { chain-to src dest count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    CH_CTRL_TRIG_INCR_READ or
    channel CH_CTRL_TRIG !
  ;

  \ Prepare buffer to buffer transfer with chaining
  : prepare-buffer>buffer-dma-with-chain
    { chain-to src dest count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    [ CH_CTRL_TRIG_INCR_WRITE CH_CTRL_TRIG_INCR_READ or ] literal or
    channel CH_CTRL_TRIG !
  ;

  \ Set DMA timer
  : dma-timer! { dividend divisor timer -- }
    timer validate-timer
    dividend 65536 u< averts x-out-of-range-timer-value
    divisor 65536 u< averts x-out-of-range-timer-value
    dividend 16 lshift divisor or timer DMA_TIMER !
  ;
    
  \ Spin wait for DMA completion
  : spin-wait-dma { channel -- }
    begin CH_CTRL_TRIG_BUSY channel CH_CTRL_TRIG bit@ while repeat
  ;

  \ Non-busy wait for DMA completion
  : wait-dma { channel -- }
    begin CH_CTRL_TRIG_BUSY channel CH_CTRL_TRIG bit@ while pause repeat
  ;

  \ Halt DMA
  : halt-dma ( channel -- )
    CH_CTRL_TRIG_EN swap CH_CTRL_TRIG bic!
  ;

  \ Abort DMA
  : abort-dma ( channel -- )
    bit CHAN_ABORT bis!
    begin CHAN_ABORT @ 0= until
  ;

  \ Get DMA source address
  : dma-src-addr@ ( channel -- addr )
    CH_READ_ADDR @
  ;

  \ Get DMA destination address
  : dma-dest-addr@ ( channel -- addr )
    CH_WRITE_ADDR @
  ;
  
  \ Get outstanding bytes transferred
  : dma-remaining@ ( channel -- remaining )
    CH_TRANS_COUNT @
  ;
  
end-module

compile-to-ram
