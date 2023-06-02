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

begin-module cyw43-spi

  oo import
  gpio import
  pio import
  pin import
  dma import
  dma-pool import
  cyw43-consts import

  begin-module cyw43-spi-internal
    
    \ SYSCFG base
    $40004000 constant SYSCFG_BASE

    \ Input sync bypass
    SYSCFG_BASE $0C + constant PROC_IN_SYNC_BYPASS
    
    \ The PIO program used for communicating
    0 constant SIDE_0
    31 constant SIDE_1
    create cyw43-pio-program
    1 SIDE_0 OUT_PINS out+,
    0 SIDE_1 COND_X1- jmp+,
    0 SIDE_0 SET_PINDIRS set+,
    MOV_SRC_Y SIDE_0 MOV_OP_NONE MOV_DEST_Y mov+,
    1 SIDE_1 IN_PINS in+,
    4 SIDE_0 COND_Y1- jmp+,
    0 SIDE_0 WAIT_PIN 1 wait+,
    0 SIDE_0 IRQ_SET irq+,

    \ Program for setting X
    create cyw43-x!-program
    32 OUT_X out,

    \ Program for setting Y
    create cyw43-y!-program
    32 OUT_Y out,

    \ Program for getting X
    create cyw43-x@-program
    32 IN_X in,

    \ Program for getting Y
    create cyw43-y@-program
    32 IN_Y in,

  end-module> import

  \ CYW43 SPI class
  <object> begin-class <cyw43-spi>

    continue-module cyw43-spi-internal
      
      \ SPI PIO block
      cell member cyw43-pio

      \ SPI PIO state machine
      cell member cyw43-sm

      \ SPI PIO DIO pin
      cell member cyw43-dio

      \ SPI PIO CLK pin
      cell member cyw43-clk

      \ SPI Chip Select pin
      cell member cyw43-cs
      
      \ SPI PIO start addr
      cell member cyw43-pio-addr

      \ SPI PIO DMA channel
      cell member cyw43-dma-channel

      \ Configure a transfer
      method cyw43-config-xfer ( write-cells read-cells self -- )
      
      \ Receive a status word
      method cyw43-status> ( self -- status )
      
      \ Transmit data via DMA
      method >cyw43-dma ( addr count self -- )
      
      \ Receive data via DMA
      method cyw43-dma> ( addr count self -- )
      
      \ Execute an instruction
      method cyw43-execute ( instr self -- )
      
      \ Write to FIFO
      method >cyw43-fifo ( x self -- )
      
      \ Read from FIFO
      method cyw43-fifo> ( self -- x )
      
      \ Set X register; note - state machine must not be enabled
      method cyw43-x! ( x self -- )
      
      \ Set Y register; note - state machine must not be enabled
      method cyw43-y! ( y self -- )
      
      \ Get X register; note - state machine must not be enabled
      method cyw43-x@ ( self -- x )
      
      \ Get Y register; note - state machine must not be enabled
      method cyw43-y@ ( self -- y )
      
      \ Set DIO PINDIR
      method cyw43-dio-pindir! ( pindir self -- )
      
    end-module

    \ Initialize the CYW43 SPI interface
    method init-cyw43-spi ( self -- )
    
    \ Send a message
    method >cyw43-msg ( addr count self -- status )

    \ Receive a message
    method cyw43-msg> ( cmd addr count self -- status )
    
  end-class

  \ Implement the CYW43 SPI class
  <cyw43-spi> begin-implement

    \ Constructor
    :noname { clk dio cs pio-addr sm pio self -- }

      \ Initialize the superclass
      self <object>->new

      \ Set up fields
      pio self cyw43-pio !
      sm self cyw43-sm !
      dio self cyw43-dio !
      clk self cyw43-clk !
      cs self cyw43-cs !
      pio-addr self cyw43-pio-addr !

      \ Allocate our DMA channel
      allocate-dma self cyw43-dma-channel !
      
    ; define new

    \ Destroy an SPI device
    :noname { self -- }

      \ Free our DMA channel
      self cyw43-dma-channel @ free-dma

      \ Destroy our superclass
      self <object>->destroy
      
    ; define destroy

    \ Initialize the CYW43 SPI interface
    :noname { self -- }

      ."  A" 10 ms \ DEBUG

      \ Get the values we need
      self cyw43-pio @ { pio }
      self cyw43-sm @ { sm }
      self cyw43-dio @ { dio }
      self cyw43-clk @ { clk }
      self cyw43-cs @ { cs }
      self cyw43-pio-addr @ { pio-addr }

      ."  B" 10 ms \ DEBUG

      \ Set up the DIO pin
      false dio PADS_BANK0_PUE!
      false dio PADS_BANK0_PDE!
      true dio PADS_BANK0_SCHMITT!
      dio bit PROC_IN_SYNC_BYPASS bis!
      DRIVE_12MA dio PADS_BANK0_DRIVE!
      true dio PADS_BANK0_SLEWFAST!

      ."  C" 10 ms \ DEBUG

      \ Set up the CLK pin
      DRIVE_12MA clk PADS_BANK0_DRIVE!
      true clk PADS_BANK0_SLEWFAST!

      ."  D" 10 ms \ DEBUG

      \ Set up the CS pin
      DRIVE_12MA cs PADS_BANK0_DRIVE!
      true cs PADS_BANK0_SLEWFAST!
      cs output-pin
      high cs pin!

      ."  E" 10 ms \ DEBUG

      \ Disable the PIO state machine
      sm bit pio sm-disable

      ."  F" 10 ms \ DEBUG

      \ Set up the PIO program
      cyw43-pio-program 8 pio-addr pio pio-instr-relocate-mem!
      pio-addr sm pio sm-addr!
      pio-addr pio-addr 8 + sm pio sm-wrap!

      ."  G" 10 ms \ DEBUG

      \ Configure the pins to be PIO output, input, set, and sidset pins
      dio 1 sm pio sm-out-pins!
      dio sm pio sm-in-pin-base!
      dio 1 sm pio sm-set-pins!
      clk 1 sm pio sm-sideset-pins!

      ."  H" 10 ms \ DEBUG

      \ Set the output shift direction and autopush
      left sm pio sm-out-shift-dir
      on sm pio sm-autopush!

      ."  I" 10 ms \ DEBUG

      \ Set the input shift direction and autopull
      left sm pio sm-in-shift-dir
      on sm pio sm-autopull!

      ."  J" 10 ms \ DEBUG

      \ Set the clock divisor, i.e. 62.5 MHz
      0 2 sm pio sm-clkdiv!

      ."  K" 10 ms \ DEBUG

      \ Initialize the CLK and DIO pins' initial direction (to output) and value
      out clk sm pio sm-pindir!
      out dio sm pio sm-pindir!
      off clk sm pio sm-pin!
      off dio sm pio sm-pin!

      ."  L" 10 ms \ DEBUG

      \ Enable the PIO state machine
      sm bit pio sm-enable      
      
      ."  M" 10 ms \ DEBUG

    ; define init-cyw43-spi

    \ Configure a transfer
    :noname { write-cells read-cells self -- }

      cr ." write-cells: " write-cells . ."  read-cells: " read-cells . \ DEBUG
      
      \ Disable the PIO state machine
      self cyw43-sm @ bit self cyw43-pio @ sm-disable

      \ Set the write and read bit counts, minus one
      write-cells 32 * 1- self cyw43-x!
      read-cells 32 * 1- self cyw43-y!

      \ Reset the DIO pin direction
      out self cyw43-dio-pindir!

      \ Reset the execution address
      self cyw43-pio-addr @ self cyw43-sm @ self cyw43-pio @ sm-addr!

      \ Enable the PIO state machine
      self cyw43-sm @ bit self cyw43-pio @ sm-enable
      
    ; define cyw43-config-xfer

    \ Send a message
    :noname { cmd addr count self -- status }

      cr ." > cmd: " cmd h.8 ."  addr: " addr h.8 ."  count: " count . \ DEBUG

      \ Configure a transfer of count words outgoing and one word incoming
      count 1+ 1 self cyw43-config-xfer

      \ Assert chip select
      low self cyw43-cs @ pin!

      \ Transfer the command word
      cmd self cyw43-sm @ self cyw43-pio @ sm-txf!
      
      \ Transmit out words
      addr count self >cyw43-dma
      
      \ Receive a status word
      self cyw43-status>

      \ Deassert chip select
      high self cyw43-cs @ pin!
      
    ; define >cyw43-msg

    \ Receive a message
    :noname { W^ cmd addr count self -- status }

      cr ." < cmd: " cmd h.8 ."  addr: " addr h.8 ."  count: " count . \ DEBUG
      
      \ Configure a transfer of one word outgoing and count plus one (for status)
      \ words incoming
      1 count 1 + self cyw43-config-xfer

      \ Assert chip select
      low self cyw43-cs @ pin!
      
      \ Transmit the command word
      cmd 1 self >cyw43-dma

      \ Receive the data words
      addr count cyw43-dma>

      \ Receive a status word
      self cyw43-status>

      \ Deassert chip select
      high self cyw43-cs @ pin!
      
    ; define cyw43-msg>

    \ Receive a status word
    :noname { self -- status }
      0 { W^ buffer }
      buffer 1 self cyw43-dma>
      buffer @
    ; define cyw43-status>

    \ Transmit data via DMA
    :noname { addr count self -- }

      \ Transfer data from the buffer, one word a time, to the PIO state
      \ machine's TXF register
      addr self cyw43-sm @ self cyw43-pio @ pio-registers::TXF count 4
      self cyw43-sm @ self cyw43-pio @ DREQ_PIO_TX
      self cyw43-dma-channel @ start-buffer>register-dma

      \ Spin until DMA completes
      self cyw43-dma-channel @ spin-wait-dma
      
    ; define >cyw43-dma

    \ Receive data via DMA
    :noname { addr count self -- }

      \ Transfer data from PIO state machine's RXF register, one word at a time,
      \ to the buffer
      self cyw43-sm @ self cyw43-pio @ pio-registers::RXF addr count 4
      self cyw43-sm @ self cyw43-pio @ DREQ_PIO_RX
      self cyw43-dma-channel @ start-register>buffer-dma

      \ Spin until DMA completes
      self cyw43-dma-channel @ spin-wait-dma
      
    ; define cyw43-dma>
    
    \ Execute an instruction
    :noname { instr self -- }
      instr self cyw43-sm @ self cyw43-pio @ sm-instr!
    ; define cyw43-execute

    \ Write to FIFO
    :noname { x self -- }
      x self cyw43-sm @ self cyw43-pio @ sm-txf!
    ; define >cyw43-fifo

    \ Read from FIFO
    :noname { self -- x }
      self cyw43-sm @ self cyw43-pio @ sm-rxf@
    ; define cyw43-fifo>

    \ Set X register; note - state machine must not be enabled
    :noname { x self -- }
      x self >cyw43-fifo
      cyw43-x!-program h@ self cyw43-execute
    ; define cyw43-x!

    \ Set Y register; note - state machine must not be enabled
    :noname { y self -- }
      y self >cyw43-fifo
      cyw43-y!-program h@ self cyw43-execute
    ; define cyw43-y!

    \ Get X register; note - state machine must not be enabled
    :noname { self -- x }
      cyw43-x@-program h@ self cyw43-execute
      self cyw43-fifo>
    ; define cyw43-x@

    \ Get Y register; note - state machine must not be enabled
    :noname { self -- y }
      cyw43-y@-program h@ self cyw43-execute
      self cyw43-fifo>
    ; define cyw43-y@
    
    \ Set DIO PINDIR
    :noname { pindir self -- }
      pindir self cyw43-dio @ self cyw43-sm @ self cyw43-pio @ sm-pindir!
    ; define cyw43-dio-pindir!

  end-implement

end-module