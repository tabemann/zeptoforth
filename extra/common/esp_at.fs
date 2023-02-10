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
\ SOFTWARE

begin-module esp-at

  oo import
  pin import
  spi import
  lock import
  closure import
  task import
  timer import

  \ ESP-AT device not owned
  : x-esp-at-not-owned ( -- ) ." ESP-AT device not owned" cr ;

  \ Are we logging input and output
  true constant log

  begin-module esp-at-internal

    2 constant SPI_MASTER_WRITE_DATA_TO_SLAVE_CMD
    3 constant SPI_MASTER_READ_DATA_FROM_SLAVE_CMD
    1 constant SPI_MASTER_WRITE_STATUS_TO_SLAVE_CMD
    4 constant SPI_MASTER_READ_STATUS_FROM_SLAVE_CMD
    
    \ The ESP-AT interface parent class
    <object> begin-class <esp-at-interface>

      \ Begin transaction
      method begin-transact ( self -- )

      \ End transaction
      method end-transact ( self -- )
      
      \ Get whether the ESP-AT device is ready
      method esp-at-ready? ( self -- )

      \ Send a byte
      method byte>esp-at ( byte self -- )
      
      \ Receive a byte
      method esp-at>byte ( self -- byte )
      
      \ Send a buffer
      method buffer>esp-at ( c-addr bytes self -- )
      
      \ Receive a buffer
      method esp-at>buffer ( c-addr bytes self -- )
      
      \ Send length message
      method trans-len>esp-at ( len self -- )

      \ Send data message
      method trans-data>esp-at ( data self -- )

      \ Receive length message
      method esp-at>trans-len ( self -- len )

      \ Receive data message
      method esp-at>trans-data ( data self -- )

  end-class

    \ This class is abstract so it has no method implementations
    <esp-at-interface> begin-implement
    end-implement
  
  end-module> import

  \ The ESP-AT SPI interface class
  <esp-at-interface> begin-class <esp-at-spi>

    continue-module esp-at-internal

      \ ESP-AT SPI device
      cell member esp-at-spi

      \ The Chip Select pin
      cell member esp-at-cs-pin
      
    end-module

  end-class

  \ The ESP-AT SPI interface class implemntation
  <esp-at-spi> begin-implement

    \ Constructor
    :noname { cs-pin spi self -- }
      spi spi-internal::validate-spi
      cs-pin pin-internal::validate-pin
      self <esp-at-interface>->new
      spi self esp-at-spi !
      cs-pin self esp-at-cs-pin !
      cs-pin output-pin
      high cs-pin pin!
    ; define new

    \ Begin transaction
    :noname { self -- }
      low self esp-at-cs-pin @ pin!
      log if cr ." <BEGIN>" then
    ; define begin-transact
    
    \ End transaction
    :noname { self -- }
      high self esp-at-cs-pin @ pin!
      log if cr ." <END>" then
    ; define end-transact

    \ Get whether the ESP-AT device is ready
    :noname { self -- }
      false
    ; define esp-at-ready?

    \ Send a byte
    :noname { byte self -- }
      byte self esp-at-spi @ >spi self esp-at-spi @ spi> drop
      log if byte emit then
    ; define byte>esp-at
    
    \ Receive a byte
    :noname { self -- byte }
      $00 self esp-at-spi @ >spi self esp-at-spi @ spi>
      log if dup $20 >= over $0D = or over $0A = or if dup emit then then
    ; define esp-at>byte
    
    \ Send a buffer
    :noname { c-addr bytes self -- }
      c-addr bytes self esp-at-spi @ buffer>spi
      log if c-addr bytes type then
    ; define buffer>esp-at
    
    \ Receive a buffer
    :noname { c-addr bytes self -- }
      c-addr bytes $00 self esp-at-spi @ spi>buffer
      log if c-addr bytes type then
    ; define esp-at>buffer

    \ Send length message
    :noname { W^ len self -- }
      self begin-transact
      SPI_MASTER_WRITE_STATUS_TO_SLAVE_CMD self byte>esp-at
      len 4 self buffer>esp-at
      self end-transact
    ; define trans-len>esp-at

    \ Send data message
    :noname { data self -- }
      self begin-transact
      SPI_MASTER_WRITE_DATA_TO_SLAVE_CMD self byte>esp-at
      0 self byte>esp-at
      data 64 self buffer>esp-at
      self end-transact
    ; define trans-data>esp-at

    \ Receive length message
    :noname { self -- len }
      self begin-transact
      SPI_MASTER_READ_STATUS_FROM_SLAVE_CMD self byte>esp-at
      0 { W^ len }
      len 4 self esp-at>buffer
      self end-transact
      len @
    ; define esp-at>trans-len

    \ Receive data message
    :noname { data self -- }
      self begin-transact
      SPI_MASTER_READ_DATA_FROM_SLAVE_CMD self byte>esp-at
      0 self byte>esp-at
      data 64 esp-at>buffer
      self end-transact
    ; define esp-at>trans-data
      
  end-implement

  \ The ESP-AT device class
  <object> begin-class <esp-at>

    continue-module esp-at-internal
      
      \ The ESP-AT device interface
      cell member esp-at-intf

      \ The ESP-AT lock
      lock-size member esp-at-lock

      \ ESP-AT device owner
      cell member esp-at-owner

      \ Message buffer size
      4096 constant esp-at-buffer-size

      \ Message buffer
      esp-at-buffer-size member esp-at-buffer

      \ Validate the ESP-AT device owner
      method validate-esp-at-owner ( self -- )

    end-module

    \ Execute code with an ESP-AT device
    method with-esp-at ( xt self -- )
    
    \ Clear an ESP-AT device's received messages
    method clear-esp-at ( self -- )
    
    \ Send a message to an ESP-AT device
    method msg>esp-at ( c-addr bytes self -- )

    \ Receive a message from an ESP-AT device
    method esp-at>msg ( c-addr bytes self -- bytes' )

    \ Receive a string from an ESP-AT device
    method esp-at>string ( self -- c-addr bytes )

  end-class

  \ Implement the ESP-AT device class
  <esp-at> begin-implement

    \ Constructor
    :noname { intf self -- }
      self <object>->new
      intf self esp-at-intf !
      0 self esp-at-owner !
      self esp-at-lock init-lock
    ; define new

    \ Execute code with an ESP-AT device
    :noname ( xt self -- ) ( xt: self -- )
      [:
        current-task over esp-at-owner ! dup >r
        [: swap execute ;] try
        0 r> esp-at-owner !
        ?raise
      ;] over esp-at-lock with-lock
    ; define with-esp-at
    
    \ Validate the ESP-AT device owner
    :noname { self -- }
      self esp-at-owner @ current-task = averts x-esp-at-not-owned
    ; define validate-esp-at-owner

    \ Clear an ESP-AT device's received messages
    :noname ( self -- )
      64 [: { self buffer }
        begin self esp-at-intf @ esp-at-ready? while
          self esp-at-intf @ esp-at>trans-len { len }
          begin len 0<> while
            50. delay-us
            buffer self esp-at-intf @ esp-at>trans-data
            180. delay-us
            len 64 - 0 max to len
          repeat
        repeat
      ;] with-allot
    ; define clear-esp-at
    
    \ Send a message to an ESP-AT device
    :noname ( c-addr bytes self -- )
      64 [: { c-addr bytes self buffer }
        self validate-esp-at-owner
        bytes self esp-at-intf @ trans-len>esp-at
        begin bytes 0<> while
          100. delay-us
          begin self esp-at-intf @ esp-at-ready? until
          bytes 64 < if buffer 64 0 fill then
          c-addr buffer 64 bytes min move
          buffer self esp-at-intf @ trans-data>esp-at
          100. delay-us
          bytes 64 - 0 max to bytes
          64 +to c-addr
        repeat
        begin self esp-at-intf @ esp-at-ready? until
        0 self esp-at-intf @ trans-len>esp-at
      ;] with-allot
    ; define msg>esp-at

    \ Receive a message from an ESP-AT device
    :noname ( c-addr bytes self -- bytes' )
      64 [: { c-addr bytes self buffer }
        self esp-at-intf @ esp-at>trans-len { len }
        bytes len min { bytes' }
        begin len 0<> while
          50. delay-us
          buffer self esp-at-intf @ esp-at>trans-data
          buffer c-addr bytes 64 min move
          180. delay-us
          bytes 64 - 0 max to bytes
          len 64 - 0 max to len
          64 +to c-addr
        repeat
        bytes'
      ;] with-allot
    ; define esp-at>msg

    \ Receive a string from an ESP-AT device
    :noname { self -- c-addr bytes }
      0 { offset }
      begin
        self esp-at-intf @ esp-at-ready? offset esp-at-buffer-size < and
      while
        self esp-at-buffer offset +
        esp-at-buffer-size offset - self esp-at>msg +to offset
      repeat
      self esp-at-buffer offset
    ; define esp-at>string

  end-implement
  
end-module
