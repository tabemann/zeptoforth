\ Copyright (c) 2023-2025 Travis Bemann
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
    16 constant dma-count

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
    : DMA_TIMER ( timer -- ) 2 lshift [ DMA_BASE $440 + ] literal + ;

    \ Abort register
    : CHAN_ABORT ( -- ) [ DMA_BASE $464 + ] literal ;
    
    \ Busy bit
    26 bit constant CH_CTRL_TRIG_BUSY

    \ Transfer request LSB
    17 constant CH_CTRL_TRIG_TREQ_SEL_LSB

    \ Chan to LSB
    13 constant CH_CTRL_TRIG_CHAIN_TO_LSB

    \ Ring select (0 for read, 1 for write)
    12 bit constant CH_CTRL_TRIG_RING_SEL

    \ Ring lower bit count LSB
    8 constant CH_CTRL_TRIG_RING_SIZE_LSB

    \ Ring lower bit count mask
    $F CH_CTRL_TRIG_RING_SIZE_LSB lshift constant CH_CTRL_TRIG_RING_SIZE_MASK
    
    \ Decrement write bit, combine with CH_CTRL_TRIG_INCR_WRITE
    7 bit constant CH_CTRL_TRIG_INCR_WRITE_REV
    
    \ Increment write bit
    6 bit constant CH_CTRL_TRIG_INCR_WRITE

    \ Decrement read bit, combine with CH_CTRL_TRIG_INCR_READ
    5 bit constant CH_CTRL_TRIG_INCR_READ_REV

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
      timer-count u< averts x-out-of-range-timer
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
    
    \ Convert PIO's to indices
    : convert-pio ( pio -- pio-index )
      case
        pio::PIO0 of 0 then
        pio::PIO1 of 1 then
        pio::PIO2 of 2 then
        dup
      endcase
    ;
  end-module> import

  \ System DREQ table

  \ PIO TX DREQ
  : DREQ_PIO_TX ( sm pio -- dreq ) convert-pio 3 lshift + ;

  \ PIO RX DREQ
  : DREQ_PIO_RX ( sm pio -- dreq ) convert-pio 3 lshift + 4 + ;

  \ SPI TX DREQ
  : DREQ_SPI_TX ( spi -- dreq ) 1 lshift 24 + ;

  \ SPI RX DREQ
  : DREQ_SPI_RX ( spi -- dreq ) 1 lshift 25 + ;

  \ UART TX DREQ
  : DREQ_UART_TX ( uart -- dreq ) 1 lshift 28 + ;

  \ UART RX DREQ
  : DREQ_UART_RX ( uart -- dreq ) 1 lshift 29 + ;

  \ PWM DREQ
  : DREQ_PWM_WRAP ( pwm -- dreq ) 32 + ;

  \ I2C TX DREQ
  : DREQ_I2C_TX ( i2c -- dreq ) 1 lshift 44 + ;

  \ I2C RX DREQ
  : DREQ_I2C_RX ( i2c -- dreq ) 1 lshift 45 + ;

  \ ADC DREQ
  : DREQ_ADC ( -- dreq ) 48 ;

  \ XIP STREAM DREQ
  : DREQ_XIP_STREAM ( -- dreq ) 49 ;

  \ XIP QMITX DREQ
  : DREQ_XIP_QMITX ( -- dreq ) 50 ;

  \ XIP QMIRX DREQ
  : DREQ_XIP_QMIRX ( -- dreq ) 51 ;

  \ HSTX DREQ
  : DREQ_HSTX ( -- dreq ) 52 ;

  \ Coresight DREQ
  : DREQ_CORESIGHT ( -- dreq ) 53 ;

  \ SHA256 DREQ
  : DREQ_SHA256 ( -- dreq ) 54 ;

  \ Timer 0-3 as TREQ
  : TREQ_TIMER ( timer -- treq ) $3B + ;

  \ Unpaced transfer
  : TREQ_UNPACED ( -- treq ) $3F ;

  \ Set a transfer count to be normal
  : TRANS_COUNT_MODE_NORMAL ( count -- count' ) $0FFFFFFF and ;

  \ Set a transfer count to be trigger-self
  : TRANS_COUNT_MODE_TRIGGER_SELF ( count -- count' )
    $0FFFFFFF and $10000000 or
  ;

  \ Set a transfer count to be endless
  : TRANS_COUNT_MODE_ENDLESS ( count -- count' ) $F0000000 or ;

  \ Register read
  0 constant REGISTER_READ

  \ Register write
  0 constant REGISTER_WRITE

  \ Incrementing buffer read
  CH_CTRL_TRIG_INCR_READ constant INCR_BUFFER_READ

  \ Incrementing buffer write
  CH_CTRL_TRIG_INCR_WRITE constant INCR_BUFFER_WRITE

  \ Decrementing buffer read
  CH_CTRL_TRIG_INCR_READ CH_CTRL_TRIG_INCR_READ_REV or constant DECR_BUFFER_READ

  \ Decrementing buffer write
  CH_CTRL_TRIG_INCR_WRITE CH_CTRL_TRIG_INCR_READ_REV or
  constant DECR_BUFFER_WRITE

  \ Incrementing ring buffer read
  : INCR_RING_BUFFER_READ ( low-ring-bits -- mode )
    CH_CTRL_TRIG_RING_SIZE_LSB lshift CH_CTRL_TRIG_RING_SIZE_MASK and
    INCR_BUFFER_READ or
  ;

  \ Incrementing ring buffer write
  : INCR_RING_BUFFER_WRITE ( low-ring-bits -- mode )
    CH_CTRL_TRIG_RING_SIZE_LSB lshift CH_CTRL_TRIG_RING_SIZE_MASK and
    [ INCR_BUFFER_WRITE CH_CTRL_TRIG_RING_SEL or ] literal or
  ;

  \ Decrementing ring buffer read
  : DECR_RING_BUFFER_READ ( low-ring-bits -- mode )
    CH_CTRL_TRIG_RING_SIZE_LSB lshift CH_CTRL_TRIG_RING_SIZE_MASK and
    DECR_BUFFER_READ or
  ;

  \ Decrementing ring buffer write
  : DECR_RING_BUFFER_WRITE ( low-ring-bits -- mode )
    CH_CTRL_TRIG_RING_SIZE_LSB lshift CH_CTRL_TRIG_RING_SIZE_MASK and
    [ DECR_BUFFER_WRITE CH_CTRL_TRIG_RING_SEL or ] literal or
  ;

  \ Start transfer
  : start-dma { src dest src-mode dest-mode count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    src-mode or
    dest-mode or
    CH_CTRL_TRIG_EN or channel CH_CTRL_TRIG !
  ;

  \ Prepare register to register transfer
  : prepare-dma { src dest src-mode dest-mode count size treq channel -- }
    channel validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    channel CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    src-mode or
    dest-mode or
    channel CH_CTRL_TRIG !
  ;

  \ Start register to register transfer with chaining
  : start-dma-with-chain
    { chain-to src dest src-mode dest-mode count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    src-mode or
    dest-mode or
    CH_CTRL_TRIG_EN or channel CH_CTRL_TRIG !
  ;

  \ Prepare register to register transfer with chaining
  : prepare-dma-with-chain
    { chain-to src dest src-mode dest-mode count size treq channel -- }
    channel validate-dma
    chain-to validate-dma
    treq validate-treq
    size bits-of-transfer-size { size-bits }
    src channel CH_READ_ADDR !
    dest channel CH_WRITE_ADDR !
    count channel CH_TRANS_COUNT !
    size-bits treq CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
    chain-to CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
    src-mode or
    dest-mode or
    channel CH_CTRL_TRIG !
  ;

  \ Start register to register transfer
  : start-register>register-dma { src dest count size treq channel -- }
    src dest REGISTER_READ REGISTER_WRITE count size treq channel start-dma
  ;

  \ Start register to buffer transfer
  : start-register>buffer-dma { src dest count size treq channel -- }
    src dest REGISTER_READ INCR_BUFFER_WRITE count size treq channel start-dma
  ;

  \ Start buffer to register transfer
  : start-buffer>register-dma { src dest count size treq channel -- }
    src dest INCR_BUFFER_READ REGISTER_WRITE count size treq channel start-dma
  ;

  \ Start buffer to buffer transfer
  : start-buffer>buffer-dma { src dest count size treq channel -- }
    src dest INCR_BUFFER_READ INCR_BUFFER_WRITE count size treq channel
    start-dma
  ;

  \ Prepare register to register transfer
  : prepare-register>register-dma { src dest count size treq channel -- }
    src dest REGISTER_READ REGISTER_WRITE count size treq channel prepare-dma
  ;

  \ Prepare register to buffer transfer
  : prepare-register>buffer-dma { src dest count size treq channel -- }
    src dest REGISTER_READ INCR_BUFFER_WRITE count size treq channel prepare-dma
  ;

  \ Prepare buffer to register transfer
  : prepare-buffer>register-dma { src dest count size treq channel -- }
    src dest INCR_BUFFER_READ REGISTER_WRITE count size treq channel prepare-dma
  ;

  \ Prepare buffer to buffer transfer
  : prepare-buffer>buffer-dma { src dest count size treq channel -- }
    src dest INCR_BUFFER_READ INCR_BUFFER_WRITE count size treq channel
    prepare-dma
  ;

  \ Start register to register transfer with chaining
  : start-register>register-dma-with-chain
    { chain-to src dest count size treq channel -- }
    chain-to src dest REGISTER_READ REGISTER_WRITE count size treq channel
    start-dma-with-chain
  ;

  \ Start register to buffer transfer with chaining
  : start-register>buffer-dma-with-chain
    { chain-to src dest count size treq channel -- }
    chain-to src dest REGISTER_READ INCR_BUFFER_WRITE count size treq channel
    start-dma-with-chain
  ;

  \ Start buffer to register transfer with chaining
  : start-buffer>register-dma-with-chain
    { chain-to src dest count size treq channel -- }
    chain-to src dest INCR_BUFFER_READ REGISTER_WRITE count size treq channel
    start-dma-with-chain
  ;

  \ Start buffer to buffer transfer with chaining
  : start-buffer>buffer-dma-with-chain
    { chain-to src dest count size treq channel -- }
    chain-to src dest INCR_BUFFER_READ INCR_BUFFER_WRITE count size treq channel
    start-dma-with-chain
  ;

  \ Prepare register to register transfer with chaining
  : prepare-register>register-dma-with-chain
    { chain-to src dest count size treq channel -- }
    chain-to src dest REGISTER_READ REGISTER_WRITE count size treq channel
    prepare-dma-with-chain
  ;

  \ Prepare register to buffer transfer with chaining
  : prepare-register>buffer-dma-with-chain
    { chain-to src dest count size treq channel -- }
    chain-to src dest REGISTER_READ INCR_BUFFER_WRITE count size treq channel
    prepare-dma-with-chain
  ;

  \ Prepare buffer to register transfer with chaining
  : prepare-buffer>register-dma-with-chain
    { chain-to src dest count size treq channel -- }
    chain-to src dest INCR_BUFFER_READ REGISTER_WRITE count size treq channel
    prepare-dma-with-chain
  ;

  \ Prepare buffer to buffer transfer with chaining
  : prepare-buffer>buffer-dma-with-chain
    { chain-to src dest count size treq channel -- }
    chain-to src dest INCR_BUFFER_READ INCR_BUFFER_WRITE count size treq channel
    prepare-dma-with-chain
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
