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

begin-module cyw43-bus

  oo import
  pin import
  cyw43-consts import
  cyw43-structs import
  cyw43-spi import
  armv6m import

  \ The CYW43 failed to initialize exception
  : x-cyw43-failed-init ( -- ) cr ." CYW43 failed to initialize" ;

  \ Buffer size is not a multiple of one word
  : x-buffer-size-not-aligned ( -- ) cr ." buffer size not word-aligned" ;

  \ Convert halfword endianness of a word
  : rev16 ( x -- x' ) dup 16 lshift swap 16 rshift or ;
  
  \ Construct the command word
  : cyw43-cmd-word { write? incr? func addr len -- x }
    write? 0<> %1 and 31 lshift
    incr? 0<> %1 and 30 lshift or
    func %11 and 28 lshift or
    addr $1FFFF and 11 lshift or
    len $7FF and or
  ;

  \ Maximum WLAN transmit size
  sdpcm-header-size bdc-header-size + mtu-size + 6 + 4 align constant wlan-size
  
  \ CYW43 bus class
  <object> begin-class <cyw43-bus>

    \ The CYW43 SPI interface
    <cyw43-spi> class-size member cyw43-spi

    \ The CYW43 PWR pin
    cell member cyw43-pwr

    \ The CYW43 status
    cell member cyw43-status

    \ The CYW43 backplane window
    cell member cyw43-bp-window
    
    \ WLAN transmit buffer
    wlan-size member cyw43-wlan-tx-buf

    \ Initialize the CYW43 bus interface
    method init-cyw43-bus ( self -- )

    \ Get the CYW43 status
    method cyw43-status@ ( self -- status )

    \ Poll for a CYW43 event
    method poll-cyw43-event ( self -- event? )

    \ Wait for CYW43 event
    method wait-cyw43-event ( self -- )
    
    \ Read bytes from the WLAN
    method cyw43-wlan> ( buffer bytes self -- )

    \ Write bytes to the WLAN
    method >cyw43-wlan ( buffer bytes self -- )

    \ Set the backplane window
    method set-cyw43-bp-window ( addr self -- )

    \ Read data from the backplane
    method cyw43-bp> ( buffer bytes addr self -- )

    \ Write data to the backplane
    method >cyw43-bp ( buffer bytes addr self -- )
    
    \ Read 8 bits from the backplane
    method cyw43-bp-8> ( addr self -- value )

    \ Write 8 bits to the backplane
    method >cyw43-bp-8 ( val addr self -- )

    \ Read 16 bits from the backplane
    method cyw43-bp-16> ( addr self -- value )

    \ Write 16 bits to the backplane
    method >cyw43-bp-16 ( val addr self -- )

    \ Read 32 bits from the backplane
    method cyw43-bp-32> ( addr self -- value )

    \ Write 32 bits to the backplane
    method >cyw43-bp-32 ( val addr self -- )

    \ Read N bytes from the backplane
    method cyw43-bp-bytes> ( addr bytes self -- value )

    \ Write N bytes to the backplane
    method >cyw43-bp-bytes ( val addr bytes self -- )

    \ Read 8 bits from the CYW43
    method cyw43-8> ( func addr self -- value )

    \ Write 8 bits to the CYW43
    method >cyw43-8 ( val func addr self -- )

    \ Read 16 bits from the CYW43
    method cyw43-16> ( func addr self -- value )

    \ Write 16 bits to the CYW43
    method >cyw43-16 ( val func addr self -- )

    \ Read 32 bits from the CYW43
    method cyw43-32> ( func addr self -- value )

    \ Write 32 bits to the CYW43
    method >cyw43-32 ( val func addr self -- )

    \ Read N bytes from the CYW43
    method cyw43-bytes> ( func addr bytes self -- value )

    \ Write N bytes to the CYW43
    method >cyw43-bytes ( val func addr bytes self -- )

    \ Read a halfword-swapped 32-bit value from the CYW43 bus
    method cyw43-32-swapped> ( addr self -- value )

    \ Write a halfword-swapped 32-bit value to the CYW43 bus
    method >cyw43-32-swapped ( val addr self -- )

  end-class

  \ Implement the CYW43 bus class
  <cyw43-bus> begin-implement

    \ The constructor
    :noname { pwr clk dio cs pio-addr sm pio self -- }
      
      \ Initialize the superclass
      self <object>->new

      \ Set up fields
      pwr self cyw43-pwr !
      0 self cyw43-status !
      $AAAAAAAA self cyw43-bp-window !

      \ Instantiate the SPI interface
      clk dio cs pio-addr sm pio <cyw43-spi> self cyw43-spi init-object
      
    ; define new

    \ The destructor
    :noname { self -- }

      \ Destroy the SPI interface
      self cyw43-spi destroy
      
      \ Destroy the superclass
      self <object>->destroy
      
    ; define destroy

    \ Initialize the CYW43 bus interface
    :noname { self -- }

      cr ." initializing SPI..." \ DEBUG

      \ Initialize the CYW43 SPI interface
      self cyw43-spi init-cyw43-spi

      cr ." initializing PWR..." \ DEBUG

      \ Initialize the PWR pin
      self cyw43-pwr @ output-pin
      self cyw43-pwr @ pull-up-pin
      
      \ Reset the CYW43
      low self cyw43-pwr @ pin!
      20 ms
      high self cyw43-pwr @ pin!
      250 ms

      cr ." waiting for the CYW43 to come up..." \ DEBUG
      
      \ Wait for the CYW43
      begin REG_BUS_TEST_RO self cyw43-32-swapped> FEEDBEAD = until

      \ Validate that the CYW43 has come up
      TEST_PATTERN REG_BUS_TEST_RW self >cyw43-32-swapped
      REG_BUS_TEST_RW self cyw43-32-swapped>
      TEST_PATTERN = averts x-cyw43-failed-init

      \ Set up 32-bit word length, little-endian
      [ WORD_LENGTH_32 HIGH_SPEED or INTERRUPT_HIGH or WAKE_UP or
      STATUS_ENABLE or INTERRUPT_WITH_STATUS or ] literal
      REG_BUS_CTRL self >cyw43-32-swapped

      \ Validate that the CYW43 has come up some more
      FUNC_BUS REG_BUS_TEST_RO self cyw43-32>
      FEEDBEAD = averts x-cyw43-failed-init
      FUNC_BUS REG_BUS_TEST_RW self cyw43-32>
      TEST_PATTERN = averts x-cyw43-failed-init
      
    ; define init-cyw43-bus

    \ Get the CYW43 status
    :noname ( self -- status ) cyw43-status @ ; define cyw43-status@
    
    \ Poll for CYW43 event
    :noname { self -- event? }
      self cyw43-spi poll-cyw43-msg
    ; define poll-cyw43-event
    
    \ Wait for CYW43 event
    :noname { self -- }
      begin self poll-cyw43-event until
    ; define wait-cyw43-event

    \ Read bytes from the WLAN
    :noname ( buffer bytes self -- )
     over 3 and if
        over 4 align [: { buffer bytes self real-buffer }
          CYW43_READ INC_ADDR FUNC_WLAN 0 bytes cyw43-cmd-word { cmd }
          cmd real-buffer bytes 4 align 2 rshift self cyw43-spi cyw43-msg>
          real-buffer buffer bytes move
          self cyw43-status !
        ;] with-aligned-allot
      else { buffer bytes self }
        CYW43_READ INC_ADDR FUNC_WLAN 0 bytes cyw43-cmd-word { cmd }
        cmd buffer bytes 2 rshift self cyw43-spi cyw43-msg>
        self cyw43-status !
      then
    ; define cyw43-wlan>

    \ Write bytes to the WLAN
    :noname { buffer bytes self -- }
      CYW43_WRITE INC_ADDR FUNC_WLAN 0 bytes 4 align cyw43-cmd-word { cmd }
      bytes 3 and if
        buffer self cyw43-wlan-tx-buf bytes move
        self cyw43-wlan-tx-buf bytes + bytes 4 align bytes - 0 fill
        cyw43-wlan-tx-buf to buffer
      then
      cmd buffer bytes 4 align 2 rshift self cyw43-spi >cyw43-msg
      self cyw43-status !
    ; define >cyw43-wlan
    
    \ Read data from the backplane
    :noname ( buffer bytes addr self -- )
      2 pick 3 and 0= averts x-buffer-size-not-aligned
      BACKPLANE_MAX_TRANSFER_SIZE [: { buffer bytes addr self part }
        begin bytes while
          addr BACKPLANE_ADDRESS_MASK and { window-offs }
          BACKPLANE_WINDOW_SIZE window-offs -
          BACKPLANE_MAX_TRANSFER_SIZE min bytes min { len }
          addr self set-cyw43-bp-window
          CYW43_READ INC_ADDR FUNC_BACKPLANE window-offs len
          cyw43-cmd-word { cmd }
          cmd part len self cyw43-spi cyw43-msg>
          self cyw43-status !
          part buffer len move
          len +to buffer
          len negate +to bytes
          len +to addr
        repeat
      ;] with-aligned-allot
    ; define cyw43-bp>

    \ Write data to the backplane
    :noname { buffer bytes addr self -- }
      bytes 3 and 0= averts x-buffer-size-not-aligned
      begin bytes while
        addr BACKPLANE_ADDRESS_MASK and { window-offs }
        BACKPLANE_WINDOW_SIZE window-offs -
        BACKPLANE_MAX_TRANSFER_SIZE min bytes min { len }
        addr self set-cyw43-bp-window
        CYW43_WRITE INC_ADDR FUNC_BACKPLANE window-offs len
        cyw43-cmd-word { cmd }
        cmd buffer len self cyw43-spi >cyw43-msg
        self cyw43-status !
        len +to buffer
        len negate +to bytes
        len +to addr
      repeat
    ; define >cyw43-bp

    \ Read 8 bits from the backplane
    :noname ( addr self -- value ) 1 swap cyw43-bp-bytes> ; define cyw43-bp-8>

    \ Write 8 bits to the backplane
    :noname ( val addr self -- ) 1 swap >cyw43-bp-bytes ; define >cyw43-bp-8

    \ Read 16 bits from the backplane
    :noname ( addr self -- value ) 2 swap cyw43-bp-bytes> ; define cyw43-bp-16>

    \ Write 16 bits to the backplane
    :noname ( val addr self -- ) 2 swap >cyw43-bp-bytes ; define >cyw43-bp-16

    \ Read 32 bits from the backplane
    :noname ( addr self -- value ) 4 swap cyw43-bp-bytes> ; define cyw43-bp-32>

    \ Write 32 bits to the backplane
    :noname ( val addr self -- )  4 swap >cyw43-bp-bytes ; define >cyw43-bp-32

    \ Read N bytes from the backplane
    :noname { addr bytes self -- value }
      addr self set-cyw43-bp-window
      addr BACKPLANE_ADDRESS_MASK and to addr
      bytes 4 = if addr BACKPLANE_ADDRESS_32BIT_FLAG or to addr then
      FUNC_BACKPLANE addr bytes self cyw43-bytes>
    ; define cyw43-bp-bytes>

    \ Write N bytes to the backplane
    :noname { val addr bytes self -- }
      addr self set-cyw43-bp-window
      addr BACKPLANE_ADDRESS_MASK and to addr
      bytes 4 = if addr BACKPLANE_ADDRESS_32BIT_FLAG or to addr then
      val FUNC_BACKPLANE addr bytes self >cyw43-bytes
    ; define >cyw43-bp-bytes

    \ Set the backplane window
    :noname { addr self -- }
      addr BACKPLANE_ADDRESS_MASK bic { new-window }
      new-window 24 rshift self cyw43-bp-window @ 24 rshift <> if
        new-window 24 rshift
        FUNC_BACKPLANE REG_BACKPLANE_BACKPLANE_ADDRESS_HIGH self >cyw43-8
      then
      new-window 16 rshift $FF and
      self cyw43-bp-window @ 16 rshift $FF and <> if
        new-window 16 rshift $FF and
        FUNC_BACKPLANE REG_BACKPLANE_BACKPLANE_ADDRESS_MID self >cyw43-8
      then
      new-window 8 rshift $FF and
      self cyw43-bp-window @ 8 rshift $FF and <> if
        new-window 8 rshift $FF and
        FUNC_BACKPLANE REG_BACKPLANE_BACKPLANE_ADDRESS_LOW self >cyw43-8
      then
      new-window self cyw43-bp-window !
    ; define set-cyw43-bp-window
    
    \ Read 8 bits from the CYW43
    :noname ( func addr self -- value ) 1 swap cyw43-bytes> ; define cyw43-8>

    \ Write 8 bits to the CYW43
    :noname ( val func addr self -- ) 1 swap >cyw43-bytes ; define >cyw43-8

    \ Read 16 bits from the CYW43
    :noname ( func addr self -- value ) 2 swap cyw43-bytes> ; define cyw43-16>

    \ Write 16 bits to the CYW43
    :noname ( val func addr self -- ) 2 swap >cyw43-bytes ; define >cyw43-16

    \ Read 32 bits from the CYW43
    :noname ( func addr self -- value ) 4 swap cyw43-bytes> ; define cyw43-32>

    \ Write 32 bits to the CYW43
    :noname ( val func addr self -- ) 4 swap >cyw43-bytes ; define >cyw43-32

    \ Read N bytes from the CYW43
    :noname ( func addr bytes self -- value )
      2 cells [: { func addr bytes self buffer }
        CYW43_READ INC_ADDR func addr bytes cyw43-cmd-word { cmd }
        func FUNC_BACKPLANE = if 2 else 1 then { recv-len }
        cmd buffer recv-len self cyw43-spi cyw43-msg> self cyw43-status !
        buffer func FUNC_BACKPLANE = if cell+ then @
      ;] with-aligned-allot
    ; define cyw43-bytes>

    \ Write N bytes to the CYW43
    :noname { W^ val func addr bytes self -- }
      CYW43_WRITE INC_ADDR func addr bytes cyw43-cmd-word { cmd }
      cmd val bytes cell align cell / self cyw43-spi >cyw43-msg
      self cyw43-status !
    ; define >cyw43-bytes

    \ Read a halfword-swapped 32-bit value from the CYW43 bus
    :noname ( addr self -- value )
      2 cells [: { addr self buffer }
        CYW43_READ INC_ADDR FUNC_BUS addr 4 cyw43-cmd-word { cmd }
        cmd rev16 buffer 2 self cyw43-spi cyw43-msg> self cyw43-status !
        buffer @ rev16

        \ DEBUG
        dup cr ." cyw43-32-swapped> " h.8
        
      ;] with-aligned-allot
    ; define cyw43-32-swapped>

    \ Write a halfword-swapped 32-bit value to the CYW43 bus
    :noname { W^ val addr self -- }
      CYW43_WRITE INC_ADDR FUNC_BUS addr 4 cyw43-cmd-word rev16 { cmd }
      val @ rev16 val !
      cmd val cell self cyw43-spi >cyw43-msg self cyw43-status !
    ; define >cyw43-32-swapped

  end-implement
  
end-module