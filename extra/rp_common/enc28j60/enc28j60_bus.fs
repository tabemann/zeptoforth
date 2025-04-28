\ Copyright (c) 2025 Paul Koning
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
\
\ Driver for Microchip ENC28J60 Ethernet chip - bus (SPI) class.  See
\ the data sheet (current rev as of this writing is rev E) and the
\ errata (rev C).

begin-module enc28j60-bus

  dma-pool import
  spi import
  spi-internal import
  pin import
  timer import
  oo import
  interrupt import
  dma import
  dma-internal import
  gpio import
  gpio-internal import
  armv6m import

  \ Reverse byte order in a 32-bit value
  : rev ( x -- x' ) [inlined] code[ r6 r6 rev_,_ ]code ;

  \ TEMP bugfix -- fixed in release 1.11.1
  rp2040? [if]
    6 constant CH_CTRL_TRIG_RING_SIZE_LSB
  [else]
    8 constant CH_CTRL_TRIG_RING_SIZE_LSB
    : DMA_TIMER ( timer -- ) 2 lshift [ DMA_BASE $440 + ] literal + ;
  : dma-timer! { dividend divisor timer -- }
    timer validate-timer
    dividend 65536 u< averts x-out-of-range-timer-value
    divisor 65536 u< averts x-out-of-range-timer-value
    dividend 16 lshift divisor or timer DMA_TIMER !
  ;
  [then]
  
  false constant debug?

  \ Exceptions
  : x-bad-ram-address ( -- ) ." invalid device RAM address" cr ;
  : x-bad-register ( -- ) ." invalid device register" cr ;
  : x-bad-mac-address ( -- ) ." invalid MAC address" cr ;

  \ Use default MAC address
  -1. 2constant default-mac-addr

  \ Frame size (including CRC)
  1518 constant MTU

  \ Register addresses are 5 bits, and there are 4 "banks" of
  \ registers (selected by the bank select bits in ECON1).  Here we
  \ encode the bank number in bits 6:5.  Note that EIE, EIR, ESTAT,
  \ ECON2, and ECON1 are mirrored in every bank, even though here
  \ they are defined as if they are bank 0 registers.  Registers
  \ categorized as "MAC" or "MII" take an extra cycle to read in the
  \ SPI interface.  These registers are flagged with bit 7 set in
  \ the register address assigned here.
  \
  \ Note that many of the registers are not defined here, because
  \ they are not used; we reduce the amount of dictionary space
  \ consumed by doing this.

  \ Bank 0
  $00 constant ERDPTL 
  $01 constant ERDPTH 
  $02 constant EWRPTL 
  $03 constant EWRPTH 
  $04 constant ETXSTL 
  $05 constant ETXSTH 
  $06 constant ETXNDL 
  $07 constant ETXNDH 
  $08 constant ERXSTL 
  $09 constant ERXSTH 
  $0a constant ERXNDL 
  $0b constant ERXNDH 
  $0c constant ERXRDPTL 
  $0d constant ERXRDPTH 
  $1b constant EIE 
  $1c constant EIR 
  $1d constant ESTAT 
  $1e constant ECON2 
  $1f constant ECON1 
  \ Bank 1
  $20 constant EHT0 
  $38 constant ERXFCON 
  $39 constant EPKTCNT 
  \ Bank 2
  $c0 constant MACON1 
  $c2 constant MACON3 
  $c3 constant MACON4 
  $c4 constant MABBIPG 
  $c6 constant MAIPGL 
  $c7 constant MAIPGH 
  $ca constant MAMXFLL 
  $cb constant MAMXFLH 
  $d2 constant MICMD 
  $d4 constant MIREGADR 
  $d6 constant MIWRL 
  $d7 constant MIWRH 
  $d8 constant MIRDL 
  $d9 constant MIRDH 
  \ Bank 3
  $e0 constant MAADR1 
  $e2 constant MAADR3 
  $e4 constant MAADR5 
  $ea constant MISTAT 

  \ The PHY registers are 16 bits wide, read via an indirect access
  \ mechanism.
  $00 constant PHCON1
  $10 constant PHCON2

  \ Values for bits or fields in selected registers
  \ ECON2
  $40 constant PKTDEC
  \ ECON1
  $08 constant TXRTS
  $04 constant RXEN
  \ MICMD
  $01 constant MIIRD
  \ MISTAT
  $01 constant BUSY

  begin-module enc28j60-bus-internal

    : bit 1 swap lshift ;
    : hbit 16 - bit ;

    \ DMA control flags
    rp2040? [if]
      21 bit constant IRQ_QUIET
    [else]
      23 bit constant IRQ_QUIET
    [then]

    \ Value written to GPIO control register to override output value
    rp2040? [if]
      $0200 constant CSHOLDVAL
    [else]
      $2000 constant CSHOLDVAL
    [then]

    \ Similar to field: and friends for struct, but defines constants
    : +field-c ( offset len "name" -- offset ) over constant + ;
    : field-c ( offset "name" -- offset ) 4 +field-c ;
    : hfield-c ( offset "name" -- offset ) 2 +field-c ;
    : cfield-c ( offset "name" -- offset ) 1 +field-c ;

    \ Analogous to , but for filling in a buffer
    : !++ ( offset n -- offset+4 ) [inlined] over ! cell+ ;

    \ Convert DMA command block count to byte count
    : commands ( cmds -- bytes ) [inlined] [ 4 cells ] literal * ;

    \ DMA registers not defined in "dma" module
    DMA_BASE $0400 + constant DMA_INTR
    DMA_BASE $0404 + constant DMA_INTE0
    
    \ OUI for generated MAC address
    $aa0001000000. 2constant def-oui

    \ SPI command codes
    $00 constant RCR
    $3a constant RBM
    $40 constant WCR
    $7a constant WBM
    $80 constant BFS
    $a0 constant BFC
    $ff constant SC

    \ Device memory layout
    7 constant xmit-overhead
    MTU xmit-overhead + 1+ constant xmit-slot-space
    8192 xmit-slot-space 2* - constant rxspace
    
    0
    rxspace 1- +field-c recv-ring  \ Errata 5: recv-ring should start at zero.
    cfield-c   recv-ring-end       \ last byte (not byte after)
    cfield-c   xmit1-ctl
    MTU        +field-c xmit1      xmit-overhead +
    cfield-c   xmit2-ctl
    MTU        +field-c xmit2      xmit-overhead +
    8192 <= averts x-bad-ram-address

    \ Received packet header
    begin-structure recv-header-layout
      hfield: next-packet
      hfield: recv-length
      hfield: status
    end-structure

    \ bits in "status" field of recv-header:
    23 hbit constant RECV_OK
    20 hbit constant RECV_CRC_ERROR

    \ Pad output status
    9 bit constant OUTTOPAD
    
    \ Command bytes and other constants, for DMA to read
    variable READMEM    \ RBM
    variable WRITEMEM   \ WBM
    variable TRUEVAL    \ true

    \ We control the CS pin explicitly because the SPI hardware in the
    \ RP2040 deasserts CS between every byte, which apparently is one
    \ of the "standards".  The ENC28J60 requires CS to remain asserted
    \ for the duration of the transaction.  Some of the operations we
    \ use here contain several transactions, so CS is deasserted in
    \ between.  The data sheet says that CS must be held for 10 ns
    \ after the end of an ETH register write, but 270 ns after the end
    \ of a MAC or MII write.  To obtain the longer hold, the sequence
    \ of words at CSHOLD is sent to the CS pin control register using
    \ a 100 ns pacing clock.  When only the short hold is required,
    \ only the last 3 words are used, and for the CS assert at the
    \ start of the transaction, only the last 2.
    4 cells buffer: CSHOLD    \ 0, 0, CSHOLDVAL, CSHOLDVAL
    CSHOLD cell    + constant CSHOLD1
    CSHOLD 2 cells + constant OVERRIDE-CSVAL

    \ Set the memory read address of a DMA request block
    : readaddr! ( addr chain entry -- ) commands + ! ;
    \ Set the memory write address of a DMA request block
    : writeaddr! ( addr chain entry -- ) commands cell+ + ! ;
    \ Set the transfer count of a DMA request block
    : count! ( len chain entry -- ) commands [ 2 cells ] literal + + ! ;
    \ Set the control field of a DMA request block
    : ctrl! ( len chain entry -- ) commands [ 3 cells ] literal + + ! ;

    \ Store a MAC address as a double integer into a buffer as a byte
    \ string in the standard order.
    : store-mac ( buf d-addr -- )
      6 0 do
	over 5 i - 4 pick + c!
	8 2rshift
      loop 2drop drop
    ;

    \ CRC32 polynomial (for hash matches)
    $04c11db7 constant crc32poly

    \ Hash the MAC address.  Note that the MAC address has the first
    \ address byte in the high order.  We run the address through the
    \ CRC starting with least-significant bit of the first byte.
    : mac-hash ( D: d-mac -- bit byte )
      16 2lshift rev swap rev
      -1
      48 0 do
	dup 31 rshift 3 pick xor 1 and if
	  2* crc32poly xor
	else
	  2*
	then
	>r 1 2rshift r>
      loop
      nip nip 23 rshift dup 7 and 1 swap lshift swap 3 rshift 7 and
    ;

  end-module> import

  \ ENC28J60 bus class
  <object> begin-class <enc28j60-bus>

    continue-module enc28j60-bus-internal
      
      \ SPI number 
      cell member enc-spi

      \ PIN for SPI CS (chip select), the second pin of the 4 pin SPI block
      cell member cspin

      \ Interrupt pin
      cell member int-pin

      \ MAC address
      2 cells member mac-addr

      \ Flag for whether SPI / DMA machinery have been started yet
      cell member spi-ready

      \ SPI transmit (host to SPI) channel
      cell member tx-dma

      \ SPI receive (SPI to host) channel
      cell member rx-dma

      \ Command DMA channel number
      cell member cmd-dma

      \ DMA timer number
      cell member dma-timer

      \ Scratch buffers (one to SPI, one from SPI)
      cell member txscratch
      cell member rxscratch

      debug? [if]
	\ transmit status buffer (7 bytes, rounded up)
	2 cells member txstatbuf

	\ address of tx status for current transmit
	cell member txstataddr
      [then]

      \ CSR address to write with command chain start, triggers operation
      cell member cmd-start

      \ Bank select command sequence command block
      9 commands member spi-bank-sel-cmd

      \ Control exchange command block
      5 commands member spi-control-cmd

      \ Set read pointers command sequence command block, keep with next
      8 commands member spi-read-addr-cmd
      \ Read memory command block
      7 commands member spi-readmem-cmd
      
      \ Set write pointers command sequence command block, keep with next
      8 commands member spi-write-addr-cmd
      \ Write memory command block
      6 commands member spi-writemem-cmd

      \ Read 1 byte from SPI to rxscratch DMA descriptor
      1 commands member spi>rxscratch-1-dma
      
      \ Read 2 bytes from SPI to rxscratch DMA descriptor
      1 commands member spi>rxscratch-2-dma
      
      \ Read N bytes from SPI to rxscratch byte 0 (N <= 4) DMA descriptor 
      1 commands member spi>rxscratch-n-dma
      
      \ Read N bytes from SPI to memory DMA descriptor
      1 commands member spi>mem-dma

      \ Bank select data.  This is 4 bytes: the first two are the
      \ command and data for "clear both bank select bits in ECON1"
      \ operation, the next two are "set specified bank select bits in
      \ ECON1".  The specified bits is where we track the current bank
      \ number.
      3 member set-bank-data
      \ Currently selected ENC28J60 register bank number
      1 member current-bank

      \ Jump target after set bank is finished
      4 member jump-target

      \ Done flag (set true at end of SPI command sequence)
      4 member done-flag
      
      \ Memory read address setup data
      4 member set-read-addr

      \ Memory write address setup data
      4 member set-write-addr
      
      \ Address of next packet header (control data)
      cell member next-pkt
      
      \ *** NOTE ***
      \ Members after this point are NOT aligned to cell boundaries.
      
      \ Buffer for reading the received packet descriptor
      recv-header-layout member recv-header

      \ Local address, as a byte string to copy into an Ethernet header
      6 member my-addr

      \ Start a SPI transaction
      method start-spi-action ( cmds self -- )
      
      \ Start a SPI transaction and wait for it to finish
      method do-spi-action ( cmds self -- )
      
      \ Spin wait for SPI transfer completion
      method spin-wait-spi ( self -- )

      \ Start an operation with optional bank select
      method start-bank-op ( cmds regnum self -- )

      \ Do an operation with optional bank select, wait for completion
      method do-bank-op ( cmds regnum self -- )

      \ Do a register operation on an ETH/MAC/MII register
      method do-reg-op ( opcode regnum len self -- )

    end-module
      
    \ Initialize the ENC28J60 bus class
    method init-enc28j60-bus ( self -- )

    \ Enable a MAC address in the hash table
    method enc28j60-bus-enable-mac ( D: d-mac-addr self -- )

    \ Set duplex mode
    method enc28j60-bus-set-duplex ( full-duplex self -- )

    \ Enable the packet receiver
    method enc28j60-enable-recv ( enable self -- )

    \ Test if device transmit active
    method enc28j60-tx-busy

    \ Read a register
    method reg@ ( regnum self -- c-val )

    \ Write a register
    method reg! ( c-val regnum self -- )

    \ Reset the device
    method reset ( self -- )
    
    \ Bit-set (OR) an ETH register
    method regbis! ( c-val regnum self -- )
    
    \ Bit-clear (NAND) an ETH register
    method regbic! ( c-val regnum self -- )
    
    \ Write an address into a register pair
    method addr! ( h-val l-regnum self -- )

    \ Write the MAC address into the device registers.  The MAC
    \ address is represented as a 48-bit (double word) integer, in big
    \ endian order
    method enc-mac! ( D: d-mac-addr self -- )
    
    \ Read a PHY register
    method phy@ ( regnum self -- h-val )

    \ Write a PHY register
    method phy! ( h-val regnum self -- )

    \ Read a block of data from the receive circular buffer (implicit
    \ device RAM address).  This waits for completion
    method read-recv-mem ( addr len self -- )

    \ Read a block of data from the specified device memory address.
    \ Note that if the start address is within the receive ring buffer
    \ address range, the read will wrap if it reaches the end of the
    \ receive ring buffer.  This starts the transfer but does not
    \ wait for completion.
    method read-mem ( dev-addr buf-addr len self -- )

    \ Write a block of data to the specified device memory address.
    \ This starts the transfer but does not wait for completion.
    method write-mem ( buf-addr dev-addr len self -- )

    \ Test for SPI transfer completion
    method enc28j60-dma-done? ( self -- flag )

    \ Start transmit, if not already in progress
    method start-xmit ( slot len self -- started )

    \ Start DMA to a transmit slot
    method start-xmit-dma ( buf len slot self -- )

    \ Test for transmit done
    method enc28j60-tx-done? ( self -- flag )

    \ Report length of next received packet, if any
    method recv-len ( self -- len | 0 )
    
    \ Mark current received packet done
    method recv-done ( self -- )

    \ Enable or disable receive interrupts
    method recv-int-ctl ( flag self -- )
    
  end-class

  \ Implement the ENC28J60 bus class
  <enc28j60-bus> begin-implement

    \ Constructor
    :noname { self }
      ( int-pin spi-pin spi-num D: mac-addr self -- )

      \ Initialize the superclass
      self <object>->new

      \ Set the locals
      false self spi-ready !       \ this needs to be first
      self enc-mac!
      self enc-spi !
      1+ self cspin !
      self int-pin !

      \ Initialize receive state
      0 self next-pkt !

      \ Initialize DMA state
      true self done-flag !

      \ Initialize the set-bank command data
      \ current-bank (MSB) is set to -1
      [ $ff000300 BFC ECON1 + or BFS ECON1 + 16 lshift or ] literal self set-bank-data !

      \ Initialize read and write memory address setup command data
      [ WCR ERDPTL + WCR ERDPTH + 16 lshift or ] literal self set-read-addr !
      [ WCR EWRPTL + WCR EWRPTH + 16 lshift or ] literal self set-write-addr !

      \ Allocate DMA channels and timer
      allocate-dma self tx-dma !
      allocate-dma self rx-dma !
      allocate-dma self cmd-dma !
      allocate-timer self dma-timer !

      \ Now initialize the various command chains and DMA descriptor
      \ blocks.  All the command chains have 4-word entries ordered
      \ read address, write address, length, control.

      \ This is the CSR address we write to start a command chain.
      \ It is the read-address CSR for the command DMA channel,
      \ at the alias address that also triggers the channel.
      self cmd-dma @ CH_READ_ADDR $03C or self cmd-start !      

      \ Receive 1 byte to rxscratch receive DMA block
      self spi>rxscratch-1-dma
      self enc-spi @ SPI_SSPDR !++
      self rxscratch !++
      1 !++
      \ Byte transfers, enable, paced by SPI RX, trigger command channel
      [ CH_CTRL_TRIG_SIZE_BYTE CH_CTRL_TRIG_EN or IRQ_QUIET or ] literal
      self enc-spi @ DREQ_SPI_RX CH_CTRL_TRIG_TREQ_SEL_LSB lshift or	
      self cmd-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or !++ drop

      \ Receive 2 bytes to rxscratch is the same except for length
      self spi>rxscratch-1-dma self spi>rxscratch-2-dma [ 1 commands ] literal move
      2 self spi>rxscratch-2-dma 0 count!

      \ Receive N bytes to rxscratch is the same (but at runtime the
      \ length is updated)
      self spi>rxscratch-1-dma self spi>rxscratch-n-dma [ 1 commands ] literal move

      \ Receive N bytes to memory is similar, but write autoinc is
      \ enabled.  At runtime, length and address are set.
      self spi>rxscratch-1-dma self spi>mem-dma [ 1 commands ] literal move
      CH_CTRL_TRIG_INCR_WRITE self spi>mem-dma [ 3 cells ] literal + bis!

      \ First the "control" command chain
      \ Command 0: assert chip select by setting the "force low" bits
      \ in GPIOn_CTRL.OUTOVER
      self spi-control-cmd
      OVERRIDE-CSVAL !++
      self cspin @ GPIO_CTRL $2000 or !++  \ Control reg, "set bits" action
      1 !++
      \ Word transfers, enable, paced by timer, trigger command channel
      [ CH_CTRL_TRIG_SIZE_WORD CH_CTRL_TRIG_EN or IRQ_QUIET or
      TREQ_UNPACED CH_CTRL_TRIG_TREQ_SEL_LSB lshift or ] literal
      self dma-timer @ TREQ_TIMER CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
      self cmd-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or !++
      \ Command 1: issue spi>rxscratch-n-dma to rx DMA
      self spi>rxscratch-n-dma !++
      self rx-dma @ CH_READ_ADDR !++
      4 !++
      \ Word transfers, enable, unpaced, inc both, trigger command channel
      [ CH_CTRL_TRIG_SIZE_WORD CH_CTRL_TRIG_EN or IRQ_QUIET or
      CH_CTRL_TRIG_INCR_READ or
      CH_CTRL_TRIG_INCR_WRITE or
      TREQ_UNPACED CH_CTRL_TRIG_TREQ_SEL_LSB lshift or ] literal
      self cmd-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or !++
      \ Command 2: transfer txscratch to SPI
      self txscratch !++
      self enc-spi @ SPI_SSPDR !++
      1 !++        \ set at runtime, but initial 1 value is used below
      \ Byte transfers, enable, paced by SPI, inc read, trigger none
      [ CH_CTRL_TRIG_SIZE_BYTE CH_CTRL_TRIG_EN or IRQ_QUIET or
      CH_CTRL_TRIG_INCR_READ or ] literal
      self enc-spi @ DREQ_SPI_TX CH_CTRL_TRIG_TREQ_SEL_LSB lshift or	
      self tx-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or !++
      \ Command 3: deassert chip select (clear OUTOVER).  Start out
      \ with the short hold which is used most places, so they can
      \ copy the spi-control-cmd sequence.  We'll change it at the
      \ end.
      CSHOLD1 !++
      self cspin @ GPIO_CTRL $3000 or !++  \ Control reg, "clear bits" action
      3 !++
      \ Word transfers, enable, paced by timer, int read, trigger command channel
      [ CH_CTRL_TRIG_SIZE_WORD CH_CTRL_TRIG_EN or CH_CTRL_TRIG_INCR_READ or IRQ_QUIET or ] literal
      self dma-timer @ TREQ_TIMER CH_CTRL_TRIG_TREQ_SEL_LSB lshift or
      self cmd-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or !++
      \ Command 4: write "Done" flag.  
      TRUEVAL !++
      self done-flag !++
      1 !++
      \ Word transfers, enable, unpaced, trigger none, interrupt not suppressed
      [ CH_CTRL_TRIG_SIZE_WORD CH_CTRL_TRIG_EN or
      TREQ_UNPACED CH_CTRL_TRIG_TREQ_SEL_LSB lshift or ] literal
      self tx-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or !++
      drop

      \ Bank select commands, similar to the general send-command
      \ chain but with most items fixed.  Start with the first four
      \ commands.
      self spi-control-cmd self spi-bank-sel-cmd [ 4 commands ] literal move
      self spi>rxscratch-2-dma self spi-bank-sel-cmd 1 readaddr!
      self set-bank-data self spi-bank-sel-cmd 2 readaddr!
      2 self spi-bank-sel-cmd 2 count!
      \ The second four commands are like the first except for the SPI
      \ write data source.
      self spi-bank-sel-cmd dup [ 4 commands ] literal + [ 4 commands ] literal move
      self set-bank-data 2 + self spi-bank-sel-cmd 6 readaddr!
      \ The final command is a command sequence jump: it triggers the
      \ command DMA engine with a different read address  This allows
      \ the bank select to precede any of several operation sequences
      \ without needing to be replicated before each of these.
      self spi-bank-sel-cmd [ 8 commands ] literal +
      self jump-target !++
      self cmd-start @ !++
      1 !++
      \ Word transfers, enable, unpaced, trigger none (the trigger is
      \ done by the read-address write, not here).
      [ CH_CTRL_TRIG_SIZE_WORD CH_CTRL_TRIG_EN or IRQ_QUIET or
      TREQ_UNPACED CH_CTRL_TRIG_TREQ_SEL_LSB lshift or ] literal
      self tx-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or !++ drop

      \ Read memory consists of two sections: set read address
      \ registers, and the actual read command. The first one is done
      \ only if needed since read updates the read pointers so
      \ sequential reads can be done without resetting the pointers.
      self spi-bank-sel-cmd self spi-read-addr-cmd [ 8 commands ] literal move
      self set-read-addr self spi-read-addr-cmd 2 readaddr!
      self set-read-addr 2 + self spi-read-addr-cmd 6 readaddr!

      \ Read memory command chain.  Commands 0..2 are taken from
      \ the control chain, with modifications
      self spi-control-cmd self spi-readmem-cmd [ 3 commands ] literal move
      self spi>rxscratch-1-dma self spi-readmem-cmd 1 readaddr!
      READMEM self spi-readmem-cmd 2 readaddr!
      \ Command 3..6 are taken from control chain 1..4 with
      \ modifications
      self spi-control-cmd [ 1 commands ] literal + 
      self spi-readmem-cmd [ 3 commands ] literal + [ 4 commands ] literal move
      self spi>mem-dma self spi-readmem-cmd 3 readaddr!
      \ Output DMA for read memory block transfer: no increment
      [ CH_CTRL_TRIG_SIZE_BYTE CH_CTRL_TRIG_EN or IRQ_QUIET or ] literal
      self enc-spi @ DREQ_SPI_TX CH_CTRL_TRIG_TREQ_SEL_LSB lshift or	
      self tx-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or
      self spi-readmem-cmd 4 ctrl!

      \ Write memory also consists of two parts, but here the write
      \ address registers setting is always done because those do not
      \ auto-update.
      self spi-bank-sel-cmd self spi-write-addr-cmd [ 8 commands ] literal move
      self set-write-addr self spi-write-addr-cmd 2 readaddr!
      self set-write-addr 2 + self spi-write-addr-cmd 6 readaddr!
      \ Write memory command chain.  Commands 0..1 are taken from
      \ the control chain
      self spi-control-cmd self spi-writemem-cmd [ 2 commands ] literal move
      \ Command 2: send "write memory" command byte to SPI
      self spi-writemem-cmd [ 2 commands ] literal +
      WRITEMEM !++
      self enc-spi @ SPI_SSPDR !++
      1 !++
      \ Byte transfers, enable, paced by SPI, trigger command channel
      [ CH_CTRL_TRIG_SIZE_BYTE CH_CTRL_TRIG_EN or IRQ_QUIET or ] literal
      self enc-spi @ DREQ_SPI_TX CH_CTRL_TRIG_TREQ_SEL_LSB lshift or	
      self cmd-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or !++ drop
      \ Command 3..5 are taken from control chain 2..4, unchanged
      self spi-control-cmd [ 2 commands ] literal +
      self spi-writemem-cmd [ 3 commands ] literal + [ 3 commands ] literal move

      \ Finally, change spi-control-cmd to use the full length CS deassert
      CSHOLD self spi-control-cmd 3 readaddr!
      4 self spi-control-cmd 3 count!
    ; define new

    \ Destructor
    :noname { self -- }

      \ Free our DMA channels and timer
      self rx-dma @ free-dma
      self tx-dma @ free-dma
      self cmd-dma @ free-dma
      self dma-timer @ free-timer

      \ Destroy our superclass
      self <object>->destroy
    ; define destroy

    \ Test for done, and dismiss the done interrupt if so.
    :noname { self -- flag }
      self done-flag @ dup if
	self tx-dma @ bit DMA_INTR !
      then
    ; define enc28j60-dma-done?

    \ Spin wait for SPI transfer completion flag set
    :noname { self -- }
      begin
	self done-flag @
      until
    ; define spin-wait-spi

    \ Start a SPI transaction.  The interrupt is dismissed before
    \ the operation is started, and will be asserted by the command
    \ block that set done-flag, which is executed by the tx-dma engine.
    \ The interrupt is also disabled; it should be enabled separately
    \ by "start" type operations where we want to use interrupts to
    \ learn about completion rather than polling for done.
    :noname { cmds self -- }
      self tx-dma @ bit dup DMA_INTR ! DMA_INTE0 bic!
      0 self done-flag !
      cmds self cmd-start @ !
    ; define start-spi-action

    \ Start a SPI transaction and wait for it to finish
    :noname ( cmds self -- )
      { self }
      self start-spi-action
      self spin-wait-spi
    ; define do-spi-action

    \ Do a command with optional set-bank preceding it
    :noname { cmds regnum self -- }
      cmds regnum $1f and EIE < if
	regnum 5 rshift 3 and { bank }
	bank self current-bank c@ <> if
	  bank self current-bank c!
	  \ we'll set bank first, so save the desired command address
	  \ as the jump target address
	  self jump-target !
	  self spi-bank-sel-cmd
	then
      then
      self start-spi-action
    ; define start-bank-op

    :noname ( cmds regnum self -- )
      { self }
      self start-bank-op
      self spin-wait-spi
    ; define do-bank-op

    \ Do a register operation on an ETH/MAC/MII register
    :noname { opcode regnum len self -- }
      \ Set the length in read and write commands
      len self spi>rxscratch-n-dma 0 count!
      len self spi-control-cmd 2 count!
      regnum $1f and opcode or self txscratch c!
      self spi-control-cmd regnum self do-bank-op
    ; define do-reg-op
    
    \ Read a value from an ETH/MAC/MII register.
    :noname { regnum self -- c-val }
      RCR regnum dup 7 rshift 2 + self do-reg-op
      self rxscratch c@
    ; define reg@

    \ Store a value in an ETH/MAC/MII register.
    :noname { c-val regnum self -- }
      c-val self txscratch 1+ c!
      WCR regnum 2 self do-reg-op
    ; define reg!

    \ Reset the ENC29J60
    :noname { self -- }
      SC self txscratch c!
      \ Set the length in read and write commands
      1 self spi>rxscratch-n-dma 0 count!
      1 self spi-control-cmd 2 count!
      self spi-control-cmd self do-spi-action
      -1 self current-bank c!
      1 ms    \ Errata 2: wait at least 1 ms after reset.
    ; define reset
    
    \ OR (bit-set) a value into an ETH register.
    :noname { c-val regnum self -- }
      c-val self txscratch 1+ c!
      BFS regnum 2 self do-reg-op
    ; define regbis!

    \ Bit-clear a value into an ETH register.
    :noname { c-val regnum self -- }
      c-val self txscratch 1+ c!
      BFC regnum 2 self do-reg-op
    ; define regbic!

    \ Write an address into a register pair.  The regnum argument is
    \ the low address register, which will be written first.
    :noname { h-val l-regnum self -- }
      l-regnum 1 and triggers x-bad-register
      h-val l-regnum self reg!
      h-val 8 rshift l-regnum 1+ self reg!
    ; define  addr!

    :noname { D: d-mac-addr self -- }
      d-mac-addr default-mac-addr d= if
	unique-id xor $00ffffff and def-oui -rot or swap
      else
	d-mac-addr dup $ffff0100 and triggers x-bad-mac-address
      then
      2dup self mac-addr 2!
      \ Convert to 6 bytes in Ethernet order
      self my-addr -rot store-mac
      self spi-ready @ if
	\ Now set it in the device.  MAADR5 contains the first byte of
	\ the address, though the documentation doesn't say that.
	self my-addr     h@ MAADR5 self addr!
	self my-addr 2 + h@ MAADR3 self addr!
	self my-addr 4 + h@ MAADR1 self addr!
      then
    ; define enc-mac!
    
    \ Read a value from a PHY register.
    :noname { regnum self -- h-val }
      regnum MIREGADR self reg!
      MIIRD MICMD self reg!
      \ 10. delay-us
      begin
	MISTAT self reg@ BUSY and
      while
	  1. delay-us
      repeat
      0 MICMD self reg!
      2. delay-us
      MIRDL self reg@ MIRDH self reg@ 8 lshift or
    ; define phy@

    \ Store a value in a PHY register.
    :noname { h-val regnum self }
      regnum MIREGADR self reg!
      h-val MIWRL self addr!
      \ 10. delay-us
      begin
	MISTAT self reg@ BUSY and
      while
	  1. delay-us
      repeat
    ; define phy!

    \ Read a block of data from the receive circular buffer (implicit
    \ device RAM address).
    \ Note this word does NOT wait for completion.  Interrupts are 
    \ enabled to signal completion.
    :noname { addr len self -- }
      \ Set the length in read and write commands
      len self spi>mem-dma 0 count!
      len self spi-readmem-cmd 4 count!
      \ Set the buffer address
      addr self spi>mem-dma 0 writeaddr!
      0 self txscratch !
      self spi-readmem-cmd self start-spi-action
      self tx-dma @ bit DMA_INTE0 bis!
    ; define read-recv-mem

    \ Read a block of data from the specified device memory address.
    \ Note that if the start address is within the receive ring buffer
    \ address range, the read will wrap if it reaches the end of the
    \ receive ring buffer.
    :noname { dev-addr buf-addr len self -- }
      dev-addr $ffffe000 and if
	cr dev-addr h.8 1 triggers x-bad-ram-address
      then
      \ Set the read address register data
      dev-addr self set-read-addr 1+ c!
      dev-addr 8 rshift self set-read-addr 3 + c!
      \ Set the length in read and write commands
      len self spi>mem-dma 0 count!
      len self spi-readmem-cmd 4 count!
      \ Set the buffer address
      buf-addr self spi>mem-dma 0 writeaddr!
      0 self txscratch !
      self spi-read-addr-cmd ERDPTL self do-bank-op
    ; define read-mem

    \ Write a block of data to the specified device memory address.
    \ Note this word does NOT wait for completion.  Interrupts are
    \ enabled to signal completion.
    :noname { buf-addr dev-addr len self -- }
      dev-addr $ffffe000 and triggers x-bad-ram-address
      \ Set the write address register data
      dev-addr self set-write-addr 1+ c!
      dev-addr 8 rshift self set-write-addr 3 + c!
      \ Set the length in read and write commands
      len 1+ self spi>rxscratch-n-dma 0 count!
      len self spi-writemem-cmd 3 count!
      \ Set buffer address
      buf-addr self spi-writemem-cmd 3 readaddr!
      self spi-write-addr-cmd EWRPTL self start-bank-op
      self tx-dma @ bit DMA_INTE0 bis!
    ; define write-mem

    \ Initialize the ENC28J60 bus class
    :noname { duplex self -- }
      \ Initialize the SPI controller
      self enc-spi @ { spi }
      spi disable-spi
      spi master-spi
      0 0 spi motorola-spi
      8 spi spi-data-size!
      \ Datasheet spec for ENC28J60 SPI clock is max of 20 MHz.
      \ Errata 1: don't go below 8 MHz.
      20000000 spi spi-baud!
      spi spi-irq NVIC_ICER_CLRENA!
      3 spi SPI_SSPDMACR !
      \ SPI uses 4 pins, MISO, CS, CLK, MOSI in that order.  "cspin"
      \ is the second of these.  CS is done wrong (at least for
      \ ENC28J60) -- CS is supposed to stay asserted (low) for the
      \ entire transfer, while the RP2040 version blips it high at
      \ each byte boundary.  We still let the SPI block drive CS but
      \ we use the "override" feature to ensure CS stays low during
      \ the transfer.  Letting SPI drive it too ensures start and end
      \ of transfer are handled at least as conservatively as the SPI
      \ block wants.
      self cspin @ 1- { pin }
      pin input-pin
      [ rp2350? ] [if]
	pin floating-pin   \ pull down is external, errata RP2350-E9
      [else]
	pin pull-down-pin
      [then]
      pin 1+ output-pin pin 1+ fast-pin
      pin 2 + output-pin pin 2 + fast-pin
      pin 3 + output-pin pin 3 + fast-pin

      \ deassert CS (active low)
      1 self cspin @ pin!
      \ Assign the pins to the SPI block
      spi pin spi-pin spi pin 1+ spi-pin
      spi pin 2 + spi-pin spi pin 3 + spi-pin
      self int-pin @ input-pin self int-pin @ pull-up-pin
      spi enable-spi

      \ Initialize the DMA TIMER to tick at 10 MHz (or a bit slower
      \ depending on rounding)
      1 10000000 dup 1- sysclk @ + swap / self dma-timer @ dma-timer!

      \ Initialize the command DMA channel registers except for the
      \ read address (start of the command chain memory block)
      self cmd-dma @ CH_READ_ADDR $030 or
      \ Words, auto inc both, wrap write 4 words, trigger none
      [ CH_CTRL_TRIG_SIZE_WORD CH_CTRL_TRIG_EN or IRQ_QUIET or
      CH_CTRL_TRIG_INCR_READ or
      CH_CTRL_TRIG_INCR_WRITE or
      CH_CTRL_TRIG_RING_SEL or                \ RING_SEL write
      4 CH_CTRL_TRIG_RING_SIZE_LSB lshift or  \ RING_SIZE 4: 1 << 4 == 16 byte wrap
      TREQ_UNPACED CH_CTRL_TRIG_TREQ_SEL_LSB lshift or ] literal
      self cmd-dma @ CH_CTRL_TRIG_CHAIN_TO_LSB lshift or !++
      self tx-dma @ CH_READ_ADDR !++
      4 !++
      drop

      true self spi-ready !

      \ The DMA machinery is now all set up, so SPI operations are now
      \ possible.  So start setting up the device itself.
      self reset
      recv-ring ERXSTL self addr!
      recv-ring ERDPTL self addr!          \ Initialize read-recv-mem pointer
      recv-ring-end ERXNDL self addr!
      recv-ring-end ERXRDPTL self addr!    \ Errata 14

      \ Set up the MAC
      self mac-addr 2@ self enc-mac!

      \ Configure full or half duplex as configured.  Note that the
      \ device doesn't negotiate, so the switch will assume
      \ half-duplex unless there is some way to manually configure the
      \ switch to full duplex mode on the port, or unless it is a
      \ switch that defaults to full duplex or only supports full
      \ duplex (as appears to be the case for the Netgear GS108)
      duplex self enc28j60-bus-set-duplex
      $12 maipgl self reg!
      $0c maipgh self reg!    \ set various Ethernet framing parameters
      MTU mamxfll self addr!  \ max frame length 1518
      \ Receive filters
      $a1 erxfcon self reg!   \ initially enable unicast and broadcast only
      $80 eie self reg!       \ initially disable all specific interrupts, set INTIE
    ; define init-enc28j60-bus
    
    \ Enable a MAC address.
    :noname { D: d-mac-addr self -- }
      d-mac-addr mac-hash eht0 + self regbis!
      4 erxfcon self regbis!
      \ d-mac-addr cr ." enabled mac addr " h.16
    ; define enc28j60-bus-enable-mac

    \ Set duplex mode.  This should be called when transmit is idle
    \ and the receiver has been disabled.
    :noname ( full-duplex self -- )
      { self }
      if
	$15 mabbipg self reg!
	$0d macon1 self reg!  \ enable pause frames in both directions and RX enable
	$31 macon3 self reg!  \ pad to 60 bytes, add CRC, enable CRC, full duplex
	$0100 phcon1 self phy!  \ set full duplex
	0 phcon2 self phy!  \ turn off HDLDIS
	\ cr ." set full duplex"
      else
	$12 mabbipg self reg!
	$01 macon1 self reg!  \ RX enable
	$30 macon3 self reg!  \ pad to 60 bytes, add CRC, enable CRC, half duplex
	0 phcon1 self phy!  \ set half duplex
	$0100 phcon2 self phy!  \ turn on HDLDIS
	\ cr ." set half duplex"
      then
    ; define enc28j60-bus-set-duplex

    \ Enable the packet receiver
    :noname ( enable self -- )
      swap if
	RXEN econ1 rot regbis!
      else
	RXEN econ1 rot regbic!
      then	
    ; define enc28j60-enable-recv

    \ See if transmit is busy
    :noname ( self -- flag )
      econ1 swap reg@ TXRTS and
    ; define enc28j60-tx-busy

    \ Check for transmit done.  Also clear the tx and txer interrupt request
    \ and interrupt enable flags if transmit done.
    :noname { self -- flag }
      eir self reg@ dup $02 and if
	[ debug? ] [if]
	  \ DEBUG: display error status
	  ." txerror eir " dup h.2 cr
	  self txstataddr @ self txstatbuf 7 self read-mem
	  ." tsv" 7 0 do
	    self txstatbuf i + c@ space h.2
	  loop cr
	[then]
	\ txer.  errata 12, do tx reset first
	$80 econ1 self regbis!
	$80 econ1 self regbic!
	\ txer, dismiss it
	$12 estat self regbic!
	$02 eir self regbic!
	TXRTS econ1 self regbic!
      then
      $0a and dup if
	$0a eir self regbic!
	$0a eie self regbic!
      then
    ; define enc28j60-tx-done?

    \ Start transmit of a previously copied packet, unless transmit
    \ is already underway.  Returns true if transmit was started,
    \ false if it was already in progress.
    :noname { slot len self -- started }
      econ1 self reg@ TXRTS and if
	false
      else
	xmit1-ctl xmit-slot-space slot * + dup etxstl self addr!
	len + dup etxndl self addr!
	[ debug? ] [if]
	  1+ self txstataddr !
	[else]
	  drop
	[then]
	TXRTS econ1 self regbis!
	\ Enable transmit completion interrupts (including txerr)
	$0a eie self regbis!
	true
      then
    ; define start-xmit

    \ Start a DMA into the specified slot.
    :noname { buf len slot self -- }
      buf slot xmit-slot-space * xmit1 + len self write-mem
    ; define start-xmit-dma
    
    \ Get the length of the next received packet, or 0 if there isn't one.
    \ Note that it is acceptable to call this multiple times, though that
    \ adds overhead so it should not happen too often.
    \ If receive error (RXERIF) is set, that error is dismissed.
    :noname { self -- len | 0 }
      epktcnt self reg@ if
	self next-pkt @ self recv-header 6 self read-mem
	self recv-header recv-length h@
      else
	\ No packets, report that
	0
      then
      \ Is there an error?
      eir self reg@ $01 and if
	[ debug? ] [if]
	  \ DEBUG: display error status
	  ." rxerror eir " eir self reg@ h.2 cr
	[then]
	$01 eir self regbic!
      then
    ; define recv-len

    \ Free the first received packet
    :noname { self -- }
      self recv-header h@ dup self next-pkt !
      ?dup if 1- else recv-ring-end then ERXRDPTL self addr!
      PKTDEC econ2 self regbis!
      \ Is there an error?
      eir self reg@ $01 and if
	[ debug? ] [if]
	  \ DEBUG: display error status
	  ." rxerror eir " eir self reg@ h.2 cr
	[then]
	$01 eir self regbic!
      then
    ; define recv-done

    \ Enable or disable receive interrupt and receive error interrupts
    :noname { flag self -- }
      $41 eie self flag if regbis! else regbic! then
    ; define recv-int-ctl

  end-implement

  : enc28j60-bus-initializer ( -- )
    \ Initialize a number of fixed memory locations.  These are done as
    \ RAM variables rather than (potentially flash resident) constants
    \ to avoid the latency of flash access.
    RBM READMEM !
    WBM WRITEMEM !
    true TRUEVAL !
    0 CSHOLD !
    0 CSHOLD cell + !
    CSHOLDVAL CSHOLD 2 cells + !
    CSHOLDVAL CSHOLD 3 cells + !
  ;

  initializer enc28j60-bus-initializer

end-module
