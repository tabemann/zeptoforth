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
  pin import
  pio import
  dma import
  dma-pool import

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

  \ CYW43 SPI class
  <object> begin-class <cyw43-spi>

    \ SPI PIO block
    cell member cyw43-pio

    \ SPI PIO state machine
    cell member cyw43-sm

    \ SPI PIO DIO pin
    cell member cyw43-dio

    \ SPI PIO start addr
    cell member cyw43-pio-addr

    \ Configure a transfer
    method cyw43-config-xfer ( write-cells read-cells self -- )

    \ Send a message
    method >cyw43-msg ( addr count self -- status )

    \ Receive a message
    method cyw43-msg> ( cmd addr count self -- status )
    
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
    
  end-class

  \ Implement the CYW43 SPI class
  <cyw43-spi> begin-implement

    \ Constructor
    :noname { clk dio pio-addr sm pio self -- }
      self <object>->new
      pio self cyw43-pio !
      sm self cyw43-sm !
      dio self cyw43-dio !
      pio-addr self cyw43-pio-addr !
      false dio PADS_BANK0_PUE!
      false dio PADS_BANK0_PDE!
      true dio PADS_BANK0_SCHMITT!
      dio bit PROC_IN_SYNC_BYPASS bis!
      DRIVE_12MA dio PADS_BANK0_DRIVE!
      true dio PADS_BANK0_SLEWFAST!

      DRIVE_12MA clk PADS_BANK0_DRIVE!
      true clk PADS_BANK0_DRIVE!

      sm pio sm-disable

      cyw43-pio-program 8 pio-addr pio pio-instr-relocate-mem!
      pio-addr sm pio sm-addr!
      pio-addr pio-addr 8 + sm pio sm-wrap!

      dio 1 pins-pio-alternate
      clk 1 pins-pio-alternate
      
      dio 1 sm pio sm-out-pins!
      dio sm pio sm-in-pin-base!
      dio 1 sm pio sm-set-pins!
      clk 1 sm pio sm-sideset-pins!
      
      left sm pio sm-out-shift-dir
      on sm pio sm-autopush!

      left sm pio sm-in-shift-dir
      on sm pio sm-autopull!

      0 2 sm pio sm-clkdiv!

      out clk sm pio sm-pindir!
      out dio sm pio sm-pindir!
      off clk sm pio sm-pin!
      off dio sm pio sm-pin!

      sm pio sm-enable
      
    ; define new

    \ Configure a transfer
    :noname { write-cells read-cells self -- }
      self cyw43-sm @ bit self cyw43-pio @ sm-disable
      write-cells 32 * 1- self cyw43-x!
      read-cells 32 * 1- self cyw43-y!
      out self cyw43-dio-pindir!
      pio-addr self cyw43-sm @ self cyw43-pio @ sm-addr!
      self cyw43-sm @ bit self cyw43-pio @ sm-enable
    ; define cyw43-config-xfer

    \ Transmit data via DMA
    :noname { addr count self -- }
      allocate-dma { channel }
      addr self cyw43-sm @ self cyw43-pio @ pio-registers::TXF count 4
      self cyw43-sm @ self cyw43-pio @ DREQ_PIO_TX
      channel start-buffer>register-dma
      channel spin-dma-wait
      channel free-dma
    ; define >cyw43-dma

    \ Send a message
    :noname { addr count self -- status }
      count 1 self cyw43-config-xfer
      addr count self >cyw43-dma
      0 { W^ buffer }
      buffer 1 self cyw43-dma>
      buffer @
    ; define >cyw43-msg

    \ Receive a message
    :noname { W^ cmd addr count self -- status }
      1 count 2 + self cyw43-config-xfer
      cmd 1 self >cyw43-dma
      addr count cyw43-dma>
      0 { W^ buffer }
      buffer 1 self cyw43-dma>
      buffer @
    ; define cyw43-msg>

    \ Receive data via DMA
    :noname { addr count self -- }
      allocate-dma { channel }
      self cyw43-sm @ self cyw43-pio @ pio-registers::RXF addr count 4
      self cyw43-sm @ self cyw43-pio @ DREQ_PIO_RX
      channel start-register>buffer-dma
      channel spin-dma-wait
      channel free-dma
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