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
  cyw43-structs import
  cyw43-events import
  cyw43-bus import
  cyw43-ioctl import
  cyw43-nvram import
  frame-interface import
  chan import
  lock import
  task import
  armv6m import
  sema import
  
  \ Core is not up exception
  : x-core-not-up ( -- ) cr ." core is not up" ;

  \ CYW43 log size
  256 constant cyw43-log-buf-size

  \ CYW43 log shared memory size
  1024 constant cyw43-log-shm-size

  \ Receive MTU count
  16 constant rx-mtu-count

  \ Transmit MTU count
  16 constant tx-mtu-count

  \ Event message count
  2 constant event-count

  \ CYW43 scratchpad buffer size
  4096 constant cyw43-scratch-size

  \ Switch byte order for a 16-bit value
  : rev16 ( -- ) [inlined] code[ r6 r6 rev16_,_ ]code ;

  \ Switch byte order for a 32-bit value
  : rev ( -- ) [inlined] code[ r6 r6 rev_,_ ]code ;

  \ Do an alignment-safe 32-bit load
  : unaligned@ { addr -- x }
    addr c@
    addr 1+ c@ 8 lshift or
    addr 2 + c@ 16 lshift or
    addr 3 + c@ 24 lshift or
  ;
  
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
    :noname { self -- }

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

      [ debug? ] [if] cr ." log addr: " addr h.8 [then] \ DEBUG
      
      addr self cyw43-log-addr !
    ; define init-cyw43-log
    
  end-implement

  \ The CYW43 frame interface
  <frame-interface> begin-class <cyw43-frame-interface>

    \ Receive lock
    lock-size member cyw43-rx-lock

    \ Transmit lock
    lock-size member cyw43-tx-lock

    \ Receive MTU channel
    mtu-size rx-mtu-count * cell align member cyw43-rx-buf

    \ Receive size channel
    rx-mtu-count cells member cyw43-rx-sizes

    \ Receive semaphore
    sema-size member cyw43-rx-sema

    \ Receive read index
    cell member cyw43-rx-get-index

    \ Receive write index
    cell member cyw43-rx-put-index

    \ Receive count
    cell member cyw43-rx-count

    \ Transmit MTU channel
    mtu-size tx-mtu-count * cell align member cyw43-tx-buf

    \ Transmit size channel
    tx-mtu-count cells member cyw43-tx-sizes

    \ Transmit semaphore
    sema-size member cyw43-tx-sema

    \ Transmit read index
    cell member cyw43-tx-get-index

    \ Transmit write index
    cell member cyw43-tx-put-index

    \ Transmit count
    cell member cyw43-tx-count

    \ The MAC address
    2 cells member cyw43-mac-addr
    
  end-class

  \ Implement the CYW43 frame interface
  <cyw43-frame-interface> begin-implement

    \ Constructor
    :noname { self -- }
      self <frame-interface>->new

      \ Initialize the receive lock
      self cyw43-rx-lock init-lock

      \ Initialize the transmit lock
      self cyw43-tx-lock init-lock

      \ Initialize the receive circular buffer
      no-sema-limit 0 self cyw43-rx-sema init-sema
      0 self cyw43-rx-get-index !
      0 self cyw43-rx-put-index !
      0 self cyw43-rx-count !

      \ Initialize the transmit circular buffer
      no-sema-limit 0 self cyw43-tx-sema init-sema
      0 self cyw43-tx-get-index !
      0 self cyw43-tx-put-index !
      0 self cyw43-tx-count !

      \ Initialize the MAC address
      0. self cyw43-mac-addr 2!
    ; define new
    
    \ Get the MTU size
    :noname { self -- bytes }
      mtu-size
    ; define mtu-size@

    \ Get the MAC address
    :noname ( self -- D: mac-addr )
      cyw43-mac-addr 2@
    ; define mac-addr@

    \ Set the MAC address
    :noname ( D: mac-addr self -- )
      cyw43-mac-addr 2!
    ; define mac-addr!
    
    \ Put a received frame
    :noname ( addr bytes self -- ) { self }
      [ debug? ] [if] cr ." ### BEGIN put-rx-frame" [then]
      self [: { addr bytes self }
        self cyw43-rx-count @ rx-mtu-count < if
          addr self cyw43-rx-buf self cyw43-rx-put-index @ mtu-size * + bytes
          move
          bytes self cyw43-rx-sizes self cyw43-rx-put-index @ cells + !
          self cyw43-rx-put-index @ 1+ rx-mtu-count umod
          self cyw43-rx-put-index !
          1 self cyw43-rx-count +! true
        else
          [ debug? ] [if] cr ." DROPPED RX PACKET: " bytes . [then]
          false
        then
      ;] self cyw43-rx-lock with-lock
      if self cyw43-rx-sema give then
      [ debug? ] [if] cr ." ### END put-rx-frame" [then]
    ; define put-rx-frame

    \ Get a received frame
    :noname ( addr bytes self -- bytes' ) { self }
      [ debug? ] [if] cr ." ### BEGIN get-rx-frame" [then]
      self cyw43-rx-sema take
      self [: { addr bytes self }
        self cyw43-rx-count @ 0> if
          self cyw43-rx-sizes self cyw43-rx-get-index @ cells + @ { actual }
          self cyw43-rx-buf self cyw43-rx-get-index @ mtu-size * + addr actual
          move
          self cyw43-rx-get-index @ 1+ rx-mtu-count umod
          self cyw43-rx-get-index !
          -1 self cyw43-rx-count +!
          actual
        else
          [ debug? ] [if] cr ." MISSING RX PACKET" [then]
          0
        then
      ;] self cyw43-rx-lock with-lock
      [ debug? ] [if] cr ." ### END get-rx-frame" [then]
    ; define get-rx-frame

    \ Poll a received frame
    :noname ( addr bytes self -- bytes' found? ) { self }
      self [: { addr bytes self }
        self cyw43-rx-count @ 0> if
          [ debug? ] [if] cr ." ### BEGIN poll-rx-frame" [then]
          self cyw43-rx-sema take
          self cyw43-rx-sizes self cyw43-rx-get-index @ cells + @ { actual }
          self cyw43-rx-buf self cyw43-rx-get-index @ mtu-size * + addr actual
          move
          self cyw43-rx-get-index @ 1+ rx-mtu-count umod
          self cyw43-rx-get-index !
          -1 self cyw43-rx-count +!
          actual true
          [ debug? ] [if] cr ." ### END poll-rx-frame" [then]
        else
          0 false
        then
      ;] self cyw43-rx-lock with-lock
    ; define poll-rx-frame

    \ Put a frame to transmit
    :noname ( addr bytes self -- ) { self }
      [ debug? ] [if] cr ." ### BEGIN put-tx-frame" [then]
      self [: { addr bytes self }
        self cyw43-tx-count @ tx-mtu-count < if
          addr self cyw43-tx-buf self cyw43-tx-put-index @ mtu-size * + bytes
          move
          bytes self cyw43-tx-sizes self cyw43-tx-put-index @ cells + !
          self cyw43-tx-put-index @ 1+ tx-mtu-count umod
          self cyw43-tx-put-index !
          1 self cyw43-tx-count +! true
        else
          [ debug? ] [if] cr ." DROPPED TX PACKET: " bytes . [then]
          false
        then
      ;] self cyw43-tx-lock with-lock
      if self cyw43-tx-sema give then
      [ debug? ] [if] cr ." ### END put-tx-frame" [then]
    ; define put-tx-frame

    \ Get a frame to transmit
    :noname ( addr bytes self -- bytes' ) { self }
      [ debug? ] [if] cr ." ### BEGIN get-tx-frame" [then]
      self cyw43-tx-sema take
      self [: { addr bytes self }
        self cyw43-tx-count @ 0> if
          self cyw43-tx-sizes self cyw43-tx-get-index @ cells + @ { actual }
          self cyw43-tx-buf self cyw43-tx-get-index @ mtu-size * + addr actual
          move
          self cyw43-tx-get-index @ 1+ tx-mtu-count umod
          self cyw43-tx-get-index !
          -1 self cyw43-tx-count +!
          actual
        else
          [ debug? ] [if] cr ." MISSING TX PACKET" [then]
          0
        then
      ;] self cyw43-tx-lock with-lock
      [ debug? ] [if] cr ." ### END get-tx-frame" [then]
    ; define get-tx-frame

    \ Poll a frame to transmit
    :noname ( addr bytes self -- bytes' found? ) { self }
      self [: { addr bytes self }
        self cyw43-tx-count @ 0> if
          [ debug? ] [if] cr ." ### BEGIN poll-tx-frame" [then]
          self cyw43-tx-sema take
          self cyw43-tx-sizes self cyw43-tx-get-index @ cells + @ { actual }
          self cyw43-tx-buf self cyw43-tx-get-index @ mtu-size * + addr actual
          move
          self cyw43-tx-get-index @ 1+ tx-mtu-count umod
          self cyw43-tx-get-index !
          -1 self cyw43-tx-count +!
          actual true
          [ debug? ] [if] cr ." ### END poll-tx-frame" [then]
        else
          0 false
        then
      ;] self cyw43-tx-lock with-lock
    ; define poll-tx-frame

  end-implement
  
  \ CYW43 runner class
  <object> begin-class <cyw43-runner>

    \ CYW43 bus
    <cyw43-bus> class-size member cyw43-bus

    \ CYW43 log
    <cyw43-log> class-size member cyw43-log

    \ CYW43 event lock
    lock-size member cyw43-event-lock

    \ CYW43 firmware address
    cell member cyw43-fw-addr

    \ CYW43 firmware size
    cell member cyw43-fw-bytes

    \ CYW43 shared memory log buffer
    cyw43-scratch-size member cyw43-scratch-buf

    \ CYW43 frame interface
    <cyw43-frame-interface> class-size member cyw43-frame-interface
    
    \ Event message channel
    event-message-size event-count chan-size cell align member cyw43-event-chan
    
    \ CYW43 ioctl state
    <cyw43-ioctl> class-size member cyw43-ioctl-state

    \ CYW43 event mask
    <cyw43-event-mask> class-size member cyw43-event-mask

    \ Event message being constructed
    event-message-size cell align member cyw43-event-message-scratch

    \ ioctl id
    2 member cyw43-ioctl-id

    \ sdpcm sequence
    1 member cyw43-sdpcm-seq

    \ sdpcm sequence maximum
    1 member cyw43-sdpcm-seq-max
    
    \ Initialize the CYW43
    method init-cyw43-runner ( self -- )

    \ Initialize the CYW43 firmware log shared memory
    method init-cyw43-log-shm ( self -- )

    \ Read the CYW43 firmware log
    method read-cyw43-log ( self -- )

    \ Run the CYW43
    method run-cyw43 ( self -- )

    \ Handle CYW43 IRQ's
    method handle-cyw43-irq ( buf self -- )

    \ Handle F2 events while status register is set
    method check-cyw43-status ( buf self -- )

    \ Handle CYW43 received packet
    method handle-cyw43-rx ( addr bytes self -- )

    \ Handle CYW43 control packet
    method handle-cyw43-control-pkt ( addr bytes self -- )

    \ Handle CYW43 event packet
    method handle-cyw43-event-pkt ( addr bytes self -- )

    \ Handle CYW43 data packet
    method handle-cyw43-data-pkt ( addr bytes self -- )

    \ Update credit
    method update-cyw43-credit ( addr self -- )

    \ Do we have credit
    method cyw43-has-credit? ( self -- credit? )

    \ Send an ioctl
    method handle-send-cyw43-ioctl ( buf-addr buf-size kind cmd iface self -- )

    \ Disable a core
    method disable-cyw43-core ( core self -- )

    \ Reset a core
    method reset-cyw43-core ( core self -- )

    \ Get whether a core is up
    method cyw43-core-up? ( core self -- up? )

    \ Enable an event
    method enable-cyw43-event ( event self -- )

    \ Enable multiple events
    method enable-cyw43-events ( event-addr event-count self -- )
    
    \ Disable an event
    method disable-cyw43-event ( event self -- )

    \ Disable multiple events
    method disable-cyw43-events ( event-addr event-count self -- )

    \ Disable all events
    method disable-all-cyw43-events ( self -- )

    \ Get the CYW43 frame interface
    method cyw43-frame-interface@ ( self -- interface )
    
    \ Enqueue event message
    method put-cyw43-event ( addr self -- )

    \ Dequeue event message
    method get-cyw43-event ( addr self -- )

    \ Poll for event message
    method poll-cyw43-event ( addr self -- found? )

    \ Clear event queue
    method clear-cyw43-events ( self -- )
    
    \ Block on an ioctl operation
    method block-cyw43-ioctl
    ( tx-addr tx-bytes rx-addr rx-bytes kind cmd iface self -- actual-bytes )

  end-class

  \ Implement the CYW43 runner class
  <cyw43-runner> begin-implement

    \ The constructor
    :noname { fw-addr fw-bytes pwr clk dio cs pio-addr sm pio self -- }

      \ Initialize the superclass
      self <object>->new

      \ Initialize the frame interface
      <cyw43-frame-interface> self cyw43-frame-interface init-object

      \ Initialize the event lock
      self cyw43-event-lock init-lock

      \ Initialize the vent channel
      event-message-size event-count self cyw43-event-chan init-chan
      
      \ Instantiate the bus
      pwr clk dio cs pio-addr sm pio <cyw43-bus> self cyw43-bus init-object
      
      \ Instantiate the log
      <cyw43-log> self cyw43-log init-object
      
      \ Instantiate the ioctl state
      <cyw43-ioctl> self cyw43-ioctl-state init-object
      
      \ Instantiate the event mask
      <cyw43-event-mask> self cyw43-event-mask init-object
      self cyw43-event-mask cyw43-events::disable-all-cyw43-events
      
      \ Initialize members
      0 self cyw43-ioctl-id h!
      0 self cyw43-sdpcm-seq c!
      0 self cyw43-sdpcm-seq-max c!
      fw-addr self cyw43-fw-addr !
      fw-bytes self cyw43-fw-bytes !
      
    ; define new

    \ The destructor
    :noname { self -- }

      \ Destroy the bus
      self cyw43-bus destroy

      \ Destroy the superclass
      self <object>->destroy

    ; define destroy

    \ Initialize the CYW43
    :noname { self -- }

      [ debug? ] [if] cr ." starting initialization... " [then]

      \ Initialize the bus
      self cyw43-bus init-cyw43-bus

      [ debug? ] [if] cr ." initialized bus" [then]

      \ Initialize ALP (Active Low Power) clock
      BACKPLANE_ALP_AVAIL_REQ FUNC_BACKPLANE REG_BACKPLANE_CHIP_CLOCK_CSR
      self cyw43-bus >cyw43-8
      
      [ debug? ] [if] cr ." waiting for clock..." [then]
      
      begin
        FUNC_BACKPLANE REG_BACKPLANE_CHIP_CLOCK_CSR self cyw43-bus cyw43-8>
      until

      [ debug? ] [if] cr ." clock ok" [then]

      $18000000 self cyw43-bus cyw43-bp-16>
      [ debug? ] [if] cr ." chip ID: " h.4 [then]

      \ Upload firmware
      [ debug? ] [if] cr ." loading fw" [then]
      WLAN self disable-cyw43-core
      SOCSRAM self reset-cyw43-core
      3 SOCSRAM_BASE_ADDRESS $10 + self cyw43-bus >cyw43-bp-32
      0 SOCSRAM_BASE_ADDRESS $44 + self cyw43-bus >cyw43-bp-32
      self cyw43-fw-addr @ self cyw43-fw-bytes @ 4 align
      ATCM_RAM_BASE_ADDRESS self cyw43-bus >cyw43-bp

      \ Upload NVRAM
      [ debug? ] [if] cr ." loading nvram" [then]
      nvram nvram-bytes
      ATCM_RAM_BASE_ADDRESS CHIP_RAM_SIZE 4 - nvram-bytes - +
      self cyw43-bus >cyw43-bp
      nvram-bytes 2 rshift not 16 lshift nvram-bytes 2 rshift or
      ATCM_RAM_BASE_ADDRESS CHIP_RAM_SIZE 4 - + self cyw43-bus >cyw43-bp-32

      \ Start core!
      [ debug? ] [if] cr ." starting up core..." [then]
      WLAN self reset-cyw43-core
      WLAN self cyw43-core-up? averts x-core-not-up

      [ debug? ] [if] cr ." polling clock... " [then]
      begin
        FUNC_BACKPLANE REG_BACKPLANE_CHIP_CLOCK_CSR
        self cyw43-bus cyw43-8> $80 and
      until

      [ debug? ] [if] cr ." enabling interrupts..." [then]

      IRQ_F2_PACKET_AVAILABLE FUNC_BUS REG_BUS_INTERRUPT_ENABLE
      self cyw43-bus >cyw43-16

      [ debug? ] [if] cr ." setting the watermark..." [then]

      32 FUNC_BACKPLANE REG_BACKPLANE_FUNCTION2_WATERMARK
      self cyw43-bus >cyw43-8

      \ Wait for WIFI startup
      [ debug? ] [if] cr ." waiting for wifi init..." [then]
      begin
        FUNC_BUS REG_BUS_STATUS self cyw43-bus cyw43-32> STATUS_F2_RX_READY and
      until

      \ Clear pulls
      0 FUNC_BACKPLANE REG_BACKPLANE_PULL_UP self cyw43-bus >cyw43-8
      FUNC_BACKPLANE REG_BACKPLANE_PULL_UP self cyw43-bus cyw43-8> drop

      self init-cyw43-log-shm
      
      [ debug? ] [if] cr ." wifi init done" [then]
      
    ; define init-cyw43-runner
    
    \ Initialize the CYW43 firmware log shared memory
    :noname { self -- }

      \ Get the log shared memory info address
      ATCM_RAM_BASE_ADDRESS CHIP_RAM_SIZE + 4 - SOCRAM_SRMEM_SIZE - { addr }
      addr self cyw43-bus cyw43-bp-32> { shared-addr }
      [ debug? ] [if] cr ." shared_addr " shared-addr h.8 [then]

      \ Read the log shared memory info
      shared-addr self shared-mem-data-size [: { shared-addr self data }
        data shared-mem-data-size shared-addr self cyw43-bus cyw43-bp>        
        data smd-console-addr @ 8 + self cyw43-log init-cyw43-log

        \ DEBUG
        [ debug? ] [if] data dup shared-mem-data-size + dump [then]
        \ DEBUG
        
      ;] with-aligned-allot
      
    ; define init-cyw43-log-shm

    \ Read the CYW43 firmware log
    :noname ( self -- )

      shared-mem-log-size [: { self log }

        \ Read log struct
        log shared-mem-log-size self cyw43-log cyw43-log-addr @
        self cyw43-bus cyw43-bp>

        \ DEBUG
        [ debug? ] [if]
          cr ." sml-idx: " log sml-idx @ h.8
          log dup shared-mem-log-size + dump
        [then]
        exit
        \ DEBUG

        \ If pointer hasn't moved, no need to do anything
        log sml-idx @ self cyw43-log cyw43-log-last-idx @ = if exit then

        \ Read entire buf for now. We could read only what we need, but then
        \ we run into annoying alignment issues
        self cyw43-scratch-buf cyw43-log-shm-size log sml-buf @
        self cyw43-bus cyw43-bp>

        begin log sml-idx @ self cyw43-log cyw43-log-last-idx @ <> while

          \ Read a byte from the buffer
          self cyw43-scratch-buf self cyw43-log cyw43-log-last-idx @ + c@ { b }
          
          b $0D = b $0A = or if
            
            \ Print a line from the log on newline
            self cyw43-log cyw43-log-buf-count @ 0<> if
              [ debug? ] [if] cr ." LOGS: " self cyw43-log cyw43-log-buf [then]
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

    \ Run the CYW43
    :noname ( self -- )
      1 [: { self }
        begin
          \ self read-cyw43-log \ commented for DEBUG
          self cyw43-has-credit? if
            self cyw43-ioctl-state cyw43-ioctl-pending? if

              \ Handle a pending ioctl
              self cyw43-ioctl-state cyw43-ioctl-tx-buf @
              self cyw43-ioctl-state cyw43-ioctl-tx-size @
              self cyw43-ioctl-state cyw43-ioctl-kind @
              self cyw43-ioctl-state cyw43-ioctl-cmd @
              self cyw43-ioctl-state cyw43-ioctl-iface @
              self handle-send-cyw43-ioctl
              self cyw43-scratch-buf self check-cyw43-status
              
            else
              self cyw43-scratch-buf mtu-size
              self cyw43-frame-interface poll-tx-frame if

                { packet-bytes }

                \ Handle a packet to transmit

                \ First calculate the size and sequence number
                sdpcm-header-size 2 + bdc-header-size + packet-bytes +
                { total-len }
                self cyw43-sdpcm-seq c@ { seq }
                seq 1+ self cyw43-sdpcm-seq c!

                \ First move the packet data in-buffer to avoid needing another
                \ buffer
                self cyw43-scratch-buf { buf }
                buf dup total-len + packet-bytes - packet-bytes move

                \ Fill the sdpcm header data
                total-len buf sdpcmh-len h!
                total-len not buf sdpcmh-len-inv h!
                seq buf sdpcmh-sequence c!
                CHANNEL_TYPE_DATA buf sdpcmh-channel-and-flags c!
                0 buf sdpcmh-next-length c!
                sdpcm-header-size 2 + buf sdpcmh-header-length c!
                0 buf sdpcmh-wireless-flow-control c!
                0 buf sdpcmh-bus-data-credit c!
                0 buf sdpcmh-reserved c!
                0 buf sdpcmh-reserved 1+ c!

                \ Then fill the bdc header data
                buf sdpcm-header-size + 2 + { bdc-buf }
                [ BDC_VERSION BDC_VERSION_SHIFT lshift ] literal
                bdc-buf bdch-flags c!
                0 bdc-buf bdch-priority c!
                0 bdc-buf bdch-flags2 c!
                0 bdc-buf bdch-data-offset c!

                \ Now we transmit the data
                buf total-len 4 align self cyw43-bus >cyw43-wlan

                \ And now we check the status
                buf self check-cyw43-status
                
              else
                drop self cyw43-scratch-buf self handle-cyw43-irq
              then
            then
          else
            [ debug? ] [if] cr ." TX stalled" [then]
            self cyw43-scratch-buf self handle-cyw43-irq
          then
        again
      ;] 1024 512 512 spawn run
    ; define run-cyw43

    \ Handle CYW43 IRQ's
    :noname { buf self -- }
      FUNC_BUS REG_BUS_INTERRUPT self cyw43-bus cyw43-16> { irq }
\      [ debug? ] [if] cr ." irq " irq h.4 [then]
      irq IRQ_F2_PACKET_AVAILABLE and if buf self check-cyw43-status then
      irq IRQ_DATA_UNAVAILABLE and if
        [ debug? ] [if] cr ." IRQ DATA_UNAVAILABLE, clearing..." [then]
        1 FUNC_BUS REG_BUS_INTERRUPT self cyw43-bus >cyw43-16
      then
    ; define handle-cyw43-irq

    \ Handle F2 events while status register is set
    :noname { buf self -- }
      begin
        self cyw43-bus cyw43-status@ { status }
        [ debug? ] [if] cr ." check status " status h.8 [then]
        status STATUS_F2_PKT_AVAILABLE and if
          status STATUS_F2_PKT_LEN_MASK and
          STATUS_F2_PKT_LEN_SHIFT rshift { len }
          buf len self cyw43-bus cyw43-wlan>
          buf len self handle-cyw43-rx
          false
        else
          true
        then
      until
    ; define check-cyw43-status

    \ Handle CYW43 received packet
    :noname { addr bytes self -- }

      [ debug? ] [if] cr ." initial packet len: " bytes . [then]

      \ Validate the SDPCM header
      bytes sdpcm-header-size < if
        [ debug? ] [if] cr ." packet too short, len=" bytes . [then]
        exit
      then
      addr sdpcmh-len h@ addr sdpcmh-len-inv h@ not $FFFF and <> if
        [ debug? ] [if] cr ." len inv mismatch" [then]
        exit
      then
      addr sdpcmh-len h@ bytes > if
        [ debug? ] [if] cr ." len from header is too large" [then]
      then
      
      addr sdpcmh-len h@ to bytes

      [ debug? ] [if] cr ." packet len: " bytes . [then]

      addr self update-cyw43-credit

      addr sdpcmh-channel-and-flags c@ { channel-and-flags }
      addr sdpcmh-header-length c@ sdpcm-header-size max { header-length }
      
      header-length +to addr
      header-length negate +to bytes

      bytes 0= if
        [ debug? ] [if] cr ." flow control packet" [then]
        exit
      then
      
      \ Handle some channel types
      channel-and-flags $0F and case
        CHANNEL_TYPE_CONTROL of addr bytes self handle-cyw43-control-pkt endof
        CHANNEL_TYPE_EVENT of addr bytes self handle-cyw43-event-pkt endof
        CHANNEL_TYPE_DATA of addr bytes self handle-cyw43-data-pkt endof
      endcase
      
    ; define handle-cyw43-rx
    
    \ Handle CYW43 control packet
    :noname { addr bytes self -- }
      bytes cdc-header-size < if exit then
      addr cdch-id h@ self cyw43-ioctl-id h@ = if
        addr cdch-status @ if
          [ debug? ] [if] cr ." IOCTL error " addr cdch-status @ h.8 [then]
        else
          addr cdc-header-size + bytes cdc-header-size -
          self cyw43-ioctl-state cyw43-ioctl-done
        then
      then
    ; define handle-cyw43-control-pkt

    \ Handle CYW43 event packet
    :noname { addr bytes self -- }

      addr dup bytes + dump \ DEBUG
      
      \ Validate an event packet
      bytes bdc-header-size < if
        [ debug? ] [if] cr ." BDC event, incomplete header" [then]
        exit
      then
      addr bdch-data-offset c@ cells bdc-header-size + { offset }
      offset +to addr
      offset negate +to bytes
      bytes event-packet-size < if
        [ debug? ] [if] cr ." BDC event, incomplete data" [then]
        exit
      then
      addr evtp-eth ethh-ether-type h@ rev16 ETH_P_LINK_CTL <> if
        [ debug? ] [if]
          cr ." unexpected ethernet type "
          addr evtp-eth ethh-ether-type h@ rev16 h.4
          ." , expected Broadcom ether type " ETH_P_LINK_CTL h.4
        [then]
        exit
      then
      addr evtp-hdr evth-oui 3 BROADCOM_OUI 3 equal-strings? not if
        [ debug? ] [if]
          cr ." unexpected ethernet OUI "
          addr evtp-hdr evth-oui dup 3 + swap ?do i c@ h.2 loop
          ." , expected Broadcom OUI "
          BROADCOM_OUI dup 3 + swap ?do i c@ h.2 loop
        [then]
        exit
      then
      addr evtp-hdr evth-subtype h@ rev16 BCMILCP_SUBTYPE_VENDOR_LONG <> if
        [ debug? ] [if]
          cr ." unexpected subtype " addr evtp-hdr evth-subtype h@ rev16 h.4
        [then]
        exit
      then
      addr evtp-hdr evth-user-subtype h@ rev16 BCMILCP_BCM_SUBTYPE_EVENT <> if
        [ debug? ] [if]
          cr ." unexpected user_subtype "
          addr evtp-hdr evth-user-subtype h@ rev16 h.4
        [then]
        exit
      then

      \ Handle an event if enabled
      addr evtp-msg emsg-event-type unaligned@ rev { event-type }
      addr evtp-msg emsg-status unaligned@ rev { event-status }
      [ debug? ] [if]
        cr ." event type: " event-type h.8 ."  status: " event-status h.8
      [then]
      event-type self cyw43-event-mask cyw43-event-enabled? if
        event-type self cyw43-event-message-scratch evt-event-type !
        event-status self cyw43-event-message-scratch evt-status !
        event-type EVENT_ESCAN_RESULT =
        event-status ESTATUS_PARTIAL = and if
          bytes [ event-packet-size scan-results-size + ] literal < if exit then
          [ event-packet-size scan-results-size + ] literal +to addr
          [ event-packet-size scan-results-size + ] literal negate +to bytes
          bytes bss-info-size < if exit then
          addr self cyw43-event-message-scratch evt-payload bss-info-size move
        else
          self cyw43-event-message-scratch evt-payload bss-info-size 0 fill
        then
        self cyw43-event-message-scratch self put-cyw43-event
      then
    ; define handle-cyw43-event-pkt

    \ Handle CYW43 data packet
    :noname { addr bytes self -- }
      bytes bdc-header-size < if exit then
      addr bdch-data-offset c@ cells bdc-header-size + { data-offset }
      addr data-offset + bytes data-offset -
      self cyw43-frame-interface put-rx-frame
    ; define handle-cyw43-data-pkt

    \ Update credit
    :noname { addr self -- }
      addr sdpcmh-channel-and-flags c@ $0F and 3 < if
        addr sdpcmh-bus-data-credit c@ { sdpcm-seq-max }
\        sdpcm-seq-max self cyw43-sdpcm-seq c@ - $FF and $40 > if
\          self cyw43-sdpcm-seq c@ 2 +
\        else
          sdpcm-seq-max
\        then
        self cyw43-sdpcm-seq-max c!
      then
    ; define update-cyw43-credit

    \ Do we have credit
    :noname { self -- credit? }
      self cyw43-sdpcm-seq c@ self cyw43-sdpcm-seq-max c@ <>
\      self cyw43-sdpcm-seq-max c@ self cyw43-sdpcm-seq c@ - $80 and 0= and
    ; define cyw43-has-credit?

    \ Send an ioctl
    :noname { buf-addr buf-size kind cmd iface self -- }

      [ sdpcm-header-size cdc-header-size + ] literal buf-size + { total-len }
      self cyw43-sdpcm-seq c@ { sdpcm-seq }
      sdpcm-seq 1+ self cyw43-sdpcm-seq c!
      1 self cyw43-ioctl-id h+!

      \ Construct a SDPCM header
      self cyw43-scratch-buf { sdpcm }
      total-len sdpcm sdpcmh-len h!
      total-len not sdpcm sdpcmh-len-inv h!
      sdpcm-seq sdpcm sdpcmh-sequence c!
      CHANNEL_TYPE_CONTROL sdpcm sdpcmh-channel-and-flags c!
      0 sdpcm sdpcmh-next-length c!
      sdpcm-header-size sdpcm sdpcmh-header-length c!
      0 sdpcm sdpcmh-wireless-flow-control c!
      0 sdpcm sdpcmh-bus-data-credit c!
      0 sdpcm sdpcmh-reserved c!
      0 sdpcm sdpcmh-reserved 1+ c!

      \ Contruct a CDC header
      sdpcm sdpcm-header-size + { cdc }
      cmd cdc cdch-cmd !
      buf-size $FFFF and cdc cdch-len !
      kind iface 12 lshift or cdc cdch-flags h!
      self cyw43-ioctl-id h@ cdc cdch-id h!
      0 cdc cdch-status !

      \ Populate the payload in the packet
      cdc cdc-header-size + { payload }
      buf-addr payload buf-size move
      buf-size 3 and if
        payload buf-size + buf-size 4 align buf-size - 0 fill
      then

      \ Send the packet
      self cyw43-scratch-buf total-len 4 align self cyw43-bus >cyw43-wlan

      [ debug? ] [if] cr ." SENT IOCTL LEN: " total-len . [then]
      
      \ Mark the packet as having been sent
      self cyw43-ioctl-state mark-cyw43-ioctl-sent
      
    ; define handle-send-cyw43-ioctl

    \ Disable a core
    :noname { core self -- }
      
      core CYW43_BASE_ADDR { base }

      \ Dummy read?
      base AI_RESETCTRL_OFFSET + self cyw43-bus cyw43-bp-8> drop

      \ Check it isn't already reset
      base AI_RESETCTRL_OFFSET + self cyw43-bus cyw43-bp-8>
      AI_RESETCTRL_BIT_RESET and if exit then

      0 base AI_IOCTRL_OFFSET + self cyw43-bus >cyw43-bp-8
      base AI_IOCTRL_OFFSET + self cyw43-bus cyw43-bp-8> drop

      1 ms

      AI_RESETCTRL_BIT_RESET
      base AI_RESETCTRL_OFFSET + self cyw43-bus >cyw43-bp-8
      base AI_RESETCTRL_OFFSET + self cyw43-bus cyw43-bp-8> drop
      
    ; define disable-cyw43-core

    \ Reset a core
    :noname { core self -- }
      
      core self disable-cyw43-core

      core CYW43_BASE_ADDR { base }

      [ AI_IOCTRL_BIT_FGC AI_IOCTRL_BIT_CLOCK_EN or ] literal
      base AI_IOCTRL_OFFSET + self cyw43-bus >cyw43-bp-8
      base AI_IOCTRL_OFFSET + self cyw43-bus cyw43-bp-8> drop

      0 base AI_RESETCTRL_OFFSET + self cyw43-bus >cyw43-bp-8

      1 ms

      AI_IOCTRL_BIT_CLOCK_EN base AI_IOCTRL_OFFSET + self cyw43-bus >cyw43-bp-8
      base AI_IOCTRL_OFFSET + self cyw43-bus cyw43-bp-8> drop

      1 ms
      
    ; define reset-cyw43-core

    \ Get whether a core is up
    :noname { core self -- up? }

      core CYW43_BASE_ADDR { base }

      base AI_IOCTRL_OFFSET + self cyw43-bus cyw43-bp-8> { io }
      [ AI_IOCTRL_BIT_FGC AI_IOCTRL_BIT_CLOCK_EN or ] literal io and
      AI_IOCTRL_BIT_CLOCK_EN <> if
        [ debug? ] [if]
          cr ." cyw43-core-up?: returning false due to bad ioctrl " io h.2
        [then]
        false exit
      then

      base AI_RESETCTRL_OFFSET + self cyw43-bus cyw43-bp-8> { r }
      AI_RESETCTRL_BIT_RESET r and if
        [ debug? ] [if]
          cr ." cyw43-core-up?: returning false due to bad resetctrl " r h.2
        [then]
        false exit
      then

      true
      
    ; define cyw43-core-up?

    \ Get the CYW43 frame interface
    :noname ( self -- interface )
      cyw43-frame-interface
    ; define cyw43-frame-interface@

    \ Enable an event
    :noname ( event self -- )
      cyw43-event-mask cyw43-events::enable-cyw43-event
    ; define enable-cyw43-event

    \ Enable multiple events
    :noname ( event-addr event-count self -- )
      cyw43-event-mask cyw43-events::enable-cyw43-events
    ; define enable-cyw43-events
    
    \ Disable an event
    :noname ( event self -- )
      cyw43-event-mask cyw43-events::disable-cyw43-event
    ; define disable-cyw43-event

    \ Disable multiple events
    :noname ( event-addr event-count self -- )
      cyw43-event-mask cyw43-events::disable-cyw43-events
    ; define disable-cyw43-events

    \ Disable all events
    :noname ( self -- )
      cyw43-event-mask cyw43-events::disable-all-cyw43-events
    ; define disable-all-cyw43-events

    \ Enqueue event message
    :noname { addr self -- }
      addr event-message-size self cyw43-event-chan send-chan
    ; define put-cyw43-event

    \ Dequeue event message
    :noname { addr self -- }
      addr event-message-size self cyw43-event-chan recv-chan drop
    ; define get-cyw43-event

    \ Poll for event message
    :noname ( addr self -- found? )
      [: { addr self }
        self cyw43-event-chan chan-empty? not if
          addr event-message-size self cyw43-event-chan recv-chan drop
          true
        else
          false
        then
      ;] over cyw43-event-lock with-lock
    ; define poll-cyw43-event

    \ Clear event queue
    :noname { self -- }
      begin self cyw43-event-chan chan-empty? not while
        self cyw43-event-chan skip-chan
      repeat
    ; define clear-cyw43-events

    \ Block on an ioctl operation
    :noname
      ( tx-addr tx-bytes rx-addr rx-bytes kind cmd iface self -- actual-bytes )
      cyw43-ioctl-state cyw43-ioctl::block-cyw43-ioctl
    ; define block-cyw43-ioctl

  end-implement
  
end-module
