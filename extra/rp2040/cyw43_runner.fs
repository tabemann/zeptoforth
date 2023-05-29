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

begin-module cyw43-runner

  oo import
  cyw43-consts import
  cyw43-bus import
  cyw43-nvram import

  \ Core is not up exception
  : x-core-not-up ( -- ) cr ." core is not up" ;

  \ CYW43 log size
  256 constant cyw43-log-buf-size

  \ CYW43 log shared memory size
  1024 constant cyw43-log-shm-size
  
  \ CYW43 log class
  <object> begin-class <cyw43-log>

    \ Log address
    cell member cyw43-log-addr

    \ Last index
    cell member cyw43-log-last-idx

    \ Buffer
    cyw43-log-buf-size member cyw43-log-buf

    \ Buffer count
    cell member cyw43-log-buf-count

    \ Initialize the log
    method init-cyw43-log ( addr self -- )

  end-class

  \ Implement the CYW43 log class
  <cyw43-log> begin-implement

    \ Constructor
    :noname { self }

      \ Initialize the superclass
      self <object>->new

      \ Initialize the fields
      0 self cyw43-log-addr !
      0 self cyw43-log-last-idx !
      self cyw43-log-buf cyw43-log-buf-size 0 fill
      0 self cyw43-log-buf-count !
      
    ; define new
    
    \ Initialize the log
    :noname { addr self -- }
      addr self cyw43-log-addr !
    ; define init-cyw43-log
    
  end-implement
  
  \ CYW43 runner class
  <object> begin-class <cyw43-runner>

    \ CYW43 bus
    <cyw43-bus> class-size member cyw43-bus

    \ CYW43 log
    <cyw43-log> class-size member cyw43-log

    \ CYW43 shared memory log buffer
    cyw43-log-shm-size member cyw43-log-shm-buf
    
    \ Initialize the CYW43
    method init-cyw43-runner ( fw-addr fw-bytes self -- )

    \ Initialize the CYW43 firmware log shared memory
    method init-cyw43-log-shm ( self -- )

    \ Read the CYW43 firmware log
    method read-cyw43-log

  end-class

  \ Implement the CYW43 runner class
  <cyw43-runner> begin-implement

    \ The constructor
    :noname { pwr clk dio pio-addr sm pio self -- )

      \ Initialize the superclass
      self <object>-new

      \ Instantiate the bus
      pwr clk dio pio-addr sm pio <cyw43-bus> self cyw43-bus init-object

      \ Instantiate the log
      <cyw43-log> self cyw43-log init-object
      
    ; define new

    \ The destructor
    :noname { self -- }

      \ Destroy the bus
      self cyw43-bus destroy

      \ Destroy the superclass
      self <object>->destroy

    ; define destroy

    \ Initialize the CYW43
    :noname { fw-addr fw-bytes self -- }

      \ Initialize the bus
      self cyw43-bus init-cyw43-bus

      \ Initialize ALP (Active Low Power) clock
      BACKPLANE_ALP_AVAIL_REQ FUNC_BACKPLANE REG_BACKPLANE_CHIP_CLOCK_CR
      self cyw43-bus >cyw43-8
      cr ." waiting for clock..."
      begin
        FUNC_BACKPLANE REG_BACKPLANE_CHIP_CLOCK_CSR self cyw43-bus cyw43-8>
      until
      cr ." clock ok"

      $18000000 self cyw43-bus cyw43-bp-16>
      cr ." chip ID: " h.4

      \ Upload firmware
      cr ." loading fw"
      WLAN self disable-cyw43-core
      SOCSRAM self reset-cyw43-core
      3 SOCSRAM_BASE_ADDRESS $10 + self cyw43-bus >cyw43-bp-32
      0 SOCSRAM_BASE_ADDRESS $44 + self cyw43-bus >cyw43-bp-32
      fw-addr fw-bytes ATCM_RAM_BASE_ADDRESS self cyw43-bus >cyw43-bp

      \ Upload NVRAM
      cr ." loading nvram"
      nvram mvram-bytes ATCM_RAM_BASE_ADDRESS CHIP_RAM_SIZE 4 - nvram-bytes -
      self cyw43-bus >cyw43-bp
      nvram-bytes 2 rshift not 16 lshift nvram-bytes 2 rshift or
      ATCM_RAM_BASE_ADDRESS CHIP_RAM_SIZE 4 - self cyw43-bus >cyw43-bp-32

      \ Start core!
      cr ." starting up core..."
      WLAN self reset-cyw43-core
      WLAN self cyw43-core-up? averts x-core-not-up

      begin
        FUNC_BACKPLANE REG_BACKPLANE_CHIP_CLOCK_SCR
        self cyw43-bus cyw43-8> $80 and
      until

      IRQ_F2_PACKET_AVAILABLE FUNC_BUS REG_BUS_INTERRUPT_ENABLE
      self cyw43-bus >cyw43-16

      32 FUNC_BACKPLANE REG_BACKPLANE_FUNCTION2_WATERMARK
      self cyw43-bus >cyw43-8

      \ Wait for WIFI startup
      cr ." waiting for wifi init..."
      begin
        FUNC_BUS REG_BUS_STATUS self cyw43-bus cyw43-32> STATUS_F2_RX_READY and
      until

      \ Clear pulls
      0 FUNC_BACKPLANE REG_BACKPLANE_PULL_UP self cyw43-bus >cyw43-8
      FUNC_BACKPLANE REG_BACKPLANE_PULL_UP self cyw43-bus cyw43-8> drop

      self init-cyw43-log-shm
      
      cr ." wifi init done"
      
    ; define init-cyw43-runner
    
    \ Initialize the CYW43 firmware log shared memory
    :noname { self -- }

      \ Get the log shared memory info address
      ATCM_RAM_BASE_ADDRESS CHIP_RAM_SIZE + 4 - SOCRAM_SRMEM_SIZE - { addr }
      addr self cyw43-bus cyw430-bp-32> { shared-addr }
      cr ." shared_addr " shared-addr h.8

      \ Read the log shared memory info
      shared-addr self shared-mem-data-size [: { shared-addr self data }
        data shared-mem-data-size shared-addr self cyw43-bus cyw43-bp>
        data smd-console-addr @ 8 + self cyw43-log init-cyw43-log
      ;] with-aligned-allot
      
    ; define init-cyw43-log-shm

    \ Read the CYW43 firmware log
    :noname ( self -- )

      shared-mem-log-size [: { self log }

        \ Read log struct
        log shared-mem-log-size self cyw43-log cyw43-log-addr @
        self cyw43-bus cyw43-bp>

        \ If pointer hasn't moved, no need to do anything
        log sml-idx @ self cyw43-log cyw43-log-last-idx @ = if exit then

        \ Read entire buf for now. We could read only what we need, but then
        \ we run into annoying alignment issues
        self cyw43-log-shm-buf cyw43-log-shm-size log sml-buf @
        self cyw43-bus cyw43-bp>

        begin log sml-idx @ self cyw43-log cyw43-log-last-idx @ <> while

          \ Read a byte from the buffer
          self cyw43-log-shm-buf self cyw43-log cyw43-log-last-idx @ + c@ { b }
          
          b $0D = b $0A = if
            
            \ Print a line from the log on newline
            self cyw43-log cyw43-log-buf-count @ 0<> if
              cr ." LOGS: " self cyw43-log cyw43-log-buf
              self cyw43-log cyw43-log-buf-count @ type
              0 self cyw43-log cyw43-log-buf-count !
            then
            
          else

            \ Write a byte to our local buffer if there is room
            self cyw43-log cyw43-log-buf-count @ cyw43-log-buf-size < if
              b self cyw43-log cyw43-log-buf
              self cyw43-log cyw43-log-buf-count @ + c!
              1 self cyw43-log cyw43-log-buf-count +!
            then
            
          then

          \ Increment the last index
          1 self cyw43-log cyw43-log-last-idx +!
          self cyw43-log cyw43-log-last-idx @ cyw43-log-shm-size = if
            0 self cyw43-log cyw43-log-last-idx !
          then
          
        repeat
        
      ;] with-aligned-allot
      
    ; define read-cyw43-log

  end-class
  
end-module
