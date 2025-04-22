\ Copyright (c) 2023-2025 Travis Bemann, Paul Koning
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

\ These are the runner tasks for the ENC28J60 Ethernet chip, adapted
\ by Paul Koning from the CYW43xxx version written by Travis Bemann.

begin-module enc28j60-runner

  oo import
  enc28j60-bus import
  enc28j60-bus-internal import  
  frame-interface import
  buffer-queue import
  lock import
  task import
  sema import
  net-misc import
  closure import
  pin import
  gpio import
  interrupt import

  \ Core is not up exception
  : x-core-not-up ( -- ) cr ." core is not up" ;

  \ Ring handling bug
  : x-rx-ring-err ( -- ) cr ." RX RING ERROR" ;
  : x-tx-ring-err ( -- ) cr ." TX RING ERROR" ;

  begin-module enc28j60-runner-internal

    false constant debug? \ DEBUG
    false debug? not and constant cdebug?

    cdebug? [if]
      : cdebug [immediate] postpone [char] postpone emit ;
    [else]
      : cdebug [immediate] char drop ;
    [then]
    
    \ Interrupt stuff
    rp2040? [if]
      11 constant DMA_IRQ_0
      13 constant IO_IRQ_BANK0
    [else]
      10 constant DMA_IRQ_0
      21 constant IO_IRQ_BANK0
    [then]
    \ Vector table positions (first 16 are for internal exceptions)
    DMA_IRQ_0 16 + constant DMA_VECTOR
    IO_IRQ_BANK0 16 + constant GPIO_VECTOR

    \ Notification flags (these are bits for a bitmap of pending things)
    1 constant notify-enc-int
    2 constant notify-dma-int
    4 constant notify-rx-space
    8 constant notify-tx-req
    16 constant notify-ctl-req

    \ Receive frame count
    24 constant rx-frame-count

    \ Transmit frame count
    16 constant tx-frame-count

  end-module> import
  
  \ The ENC28J60 frame interface
  <frame-interface> begin-class <enc28j60-frame-interface>

    continue-module enc28j60-runner-internal
      
      \ Receive buffer queue data
      MTU cell + rx-frame-count * cell align member enc28j60-rx-queue-data

      \ Transmit buffer queue data
      MTU cell + tx-frame-count * cell align member enc28j60-tx-queue-data

      \ Receive buffer queue
      <buffer-queue> class-size member enc28j60-rx-queue
      
      \ Transmit buffer queue
      <buffer-queue> class-size member enc28j60-tx-queue
      
      \ The runner that owns this frame interface and its runner task
      cell member enc28j60-owner
      cell member enc28j60-owner-runner-task

    end-module
    
  end-class

  \ Before we give the implementation of the frame interface, we first need
  \ to define the runner class since the frame interface references that
  \ in a few places.
  <object> begin-class <enc28j60-runner>

    continue-module enc28j60-runner-internal
      
      \ ENC28J60 bus
      <enc28j60-bus> class-size member my-enc28j60-bus

      \ ENC28J60 frame interface
      <enc28j60-frame-interface> class-size member enc28j60-frame-interface

      \ Closures used to dispatch to the interrupt handlers
      closure-size member enc28j60-dev-int-closure
      closure-size member enc28j60-dma-int-closure
      
      \ The runner task object
      cell member enc28j60-runner-task

      \ The notification mailbox
      cell member enc28j60-notify-mb

      \ The runner state (which says what to do next when the
      \ runner task is awakened)
      cell member enc28j60-state

      0 constant ENC_STATE_RXDMA
      1 constant ENC_STATE_TXDMA
      2 constant ENC_STATE_DEVPOLL

      \ Length of current receive DMA
      cell member current-rx-length

      \ Device transmit buffer state.  We keep two transmit buffers,
      \ toggling between the two.  This allows prefetching the next
      \ packet while a transmit on the wire is underway.  The "slot"
      \ is the index of the device buffer currently being transmitted,
      \ if a transmit is in progress, or the next one to be transmitted
      \ if transmit is not currently in progress.
      cell member enc28j60-xmit-slot

      \ Flag whether transmit is in progress.  This tracks the runner
      \ actions for transmit start and completion, so the device
      \ transmit active state would not quite serve.
      cell member enc28j60-xmit-active

      \ Byte count of the packet for each slot.  0 means empty.
      \ This is a pair of halfwords, one per slot.
      cell member enc28j60-xmit-len

      \ The control request semaphore
      sema-size member enc28j60-ctl-sema

      \ The control request buffer
      3 cells member enc28j60-ctl-req

      \ ENC28J60 shared memory buffer
      MTU cell align member enc28j60-scratch-buf

      \ Handlers for each of the things checked by the runner task loop.
      method handle-rxdma-done ( self -- )
      method handle-txdma-done ( self -- )
      method handle-rxdone ( self -- )
      method handle-txdone ( self -- )
      method handle-txstart ( self -- )
      method handle-txring ( self -- )
      method handle-ctl ( self -- )

      \ Helpers for the frame interface
      method enc28j60-mac-addr@ ( self -- D: mac-addr )
      method enc28j60-mac-addr! ( D: mac-addr self -- )

    end-module
    
    \ Initialize the ENC28J60
    method init-enc28j60-runner ( self -- )

    \ Run the ENC28J60
    method run-enc28j60 ( self -- )

    \ Enable a MAC address
    method enc28j60-enable-mac ( D: d-mac-addr self -- )

    \ Set duplex mode
    method enc28j60-set-duplex ( full-duplex self -- )

  end-class

  \ Implement the ENC28J60 frame interface
  <enc28j60-frame-interface> begin-implement

    \ Constructor
    :noname { owner self -- }
      self <frame-interface>->new

      owner self enc28j60-owner !
      0 self enc28j60-owner-runner-task !

      \ Initialize the receive buffer queue
      self enc28j60-rx-queue-data
      MTU cell + rx-frame-count * cell align
      <buffer-queue> self enc28j60-rx-queue init-object

      \ Initialize the transmit buffer queue
      self enc28j60-tx-queue-data
      MTU cell + tx-frame-count * cell align
      <buffer-queue> self enc28j60-tx-queue init-object
    ; define new
    
    \ Get the MTU size
    :noname ( self -- bytes )
      drop MTU
    ; define mtu-size@

    \ Get the MAC address
    :noname ( self -- D: mac-addr )
      enc28j60-owner @ enc28j60-mac-addr@
    ; define mac-addr@

    \ Set the MAC address
    :noname ( D: mac-addr self -- )
      enc28j60-owner @ enc28j60-mac-addr!
    ; define mac-addr!

    \ Add a MAC address to the multicast filter
    :noname ( D: mac-addr self -- )
      enc28j60-owner @ enc28j60-enable-mac
    ; define add-multicast-filter

    \ Remove a MAC address from the multicast filter
    \ Currently this is a NOP
    :noname ( D: mac-addr self -- )
      2drop drop
    ; define remove-multicast-filter

    \ Attempt to put a received frame
    : _poll-put-rx-frame ( addr bytes self -- success? )
      [ debug? ] [if] cr ." BEGIN poll-put-rx-frame" [then]
      enc28j60-rx-queue poll-put-buffer
      [ debug? ] [if] cr ." END poll-put-rx-frame " dup .
      [then]
    ; ' _poll-put-rx-frame define poll-put-rx-frame

    \ Put a received frame
    : _put-rx-frame ( addr bytes self -- )
      [ debug? ] [if] cr ." BEGIN put-rx-frame" [then]
      enc28j60-rx-queue put-buffer
      [ debug? ] [if] cr ." END put-rx-frame" [then]
    ; ' _put-rx-frame define put-rx-frame

    \ Get a received frame
    : _get-rx-frame { self -- addr bytes }
      \ [ debug? ] [if] cr ." BEGIN get-rx-frame" [then]
      self enc28j60-rx-queue get-buffer
      \ [ debug? ] [if] cr ." END get-rx-frame" [then]
      \ Tell the runner we freed up space in the receive queue
      [: notify-rx-space or ;] 0 self enc28j60-owner-runner-task @ notify-update
    ; ' _get-rx-frame define get-rx-frame

    \ Poll a received frame
    :noname { self -- addr bytes found? }
      [ debug? ] [if] cr ." BEGIN poll-rx-frame" [then]
      self enc28j60-rx-queue poll-buffer
      [ debug? ] [if] cr ." END poll-rx-frame" [then]
      \ Tell the runner we freed up space in the receive queue
      dup if
	[: notify-rx-space or ;] 0 self enc28j60-owner-runner-task @ notify-update
      then
    ; define poll-rx-frame

    \ Retire a received frame
    : _retire-rx-frame ( self -- )
      [ debug? ] [if] cr ." BEGIN retire-rx-frame" [then]
      enc28j60-rx-queue retire-buffer
      [ debug? ] [if] cr ." END retire-rx-frame" [then]
    ; ' _retire-rx-frame define retire-rx-frame

    \ Attempt to put a frame to transmit
    : _poll-put-tx-frame { addr bytes self -- success? }
      [ debug? ] [if] cr ." BEGIN poll-put-tx-frame" [then]
      addr bytes self enc28j60-tx-queue poll-put-buffer
      [ debug? ] [if] cr ." END poll-put-tx-frame" [then]
    ; ' _poll-put-tx-frame define poll-put-tx-frame
    
    \ Put a frame to transmit into the transmit frame ring
    : _put-tx-frame ( addr bytes self -- )
      { self }
      [ debug? ] [if] cr ." BEGIN put-tx-frame" [then]
      self enc28j60-tx-queue put-buffer
      [ debug? ] [if] cr ." END put-tx-frame" [then]
      \ Tell the runner there's transmit data
      [: notify-tx-req or ;] 0 self enc28j60-owner-runner-task @ notify-update
    ; ' _put-tx-frame define put-tx-frame

    \ Get a frame to transmit
    : _get-tx-frame { self -- addr bytes }
      [ debug? ] [if] cr ." BEGIN get-tx-frame" [then]
      self enc28j60-tx-queue get-buffer
      [ debug? ] [if] cr ." END get-tx-frame" [then]
    ; ' _get-tx-frame define get-tx-frame
    
    \ Poll a frame to transmit
    : _poll-tx-frame { self -- addr bytes found? }
      [ debug? ] [if] cr ." BEGIN poll-tx-frame" [then]
      self enc28j60-tx-queue poll-buffer
      [ debug? ] [if] cr ." END poll-tx-frame" [then]
    ; ' _poll-tx-frame define poll-tx-frame

    \ Retire a frame to transmit
    : _retire-tx-frame { self -- }
      [ debug? ] [if] cr ." BEGIN retire-tx-frame" [then]
      self enc28j60-tx-queue retire-buffer
      [ debug? ] [if] cr ." END retire-tx-frame" [then]
    ; ' _retire-tx-frame define retire-tx-frame

  end-implement

  \ Interrupt handler for ENC28J60 interrupt signal.
  \ Note: these handlers are not methods.
  : handle-enc28j60-dev-irq { self -- }
    IO_IRQ_BANK0 NVIC_ICER_CLRENA!
    cdebug I
    [: notify-enc-int or ;] 0 self enc28j60-runner-task @ notify-update
  ;

  \ Interrupt handler for DMA done interrupt
  : handle-enc28j60-dma-irq { self -- }
    DMA_IRQ_0 NVIC_ICER_CLRENA! 
    cdebug J
    [: notify-dma-int or ;] 0 self enc28j60-runner-task @ notify-update
  ;

  \ Implement the ENC28J60 runner class
  <enc28j60-runner> begin-implement

    \ The constructor
    :noname { int-pin spi-pin spi-num D: mac-address self -- }
      \ Initialize the superclass
      self <object>->new

      \ Initialize the control request data
      1 1 self enc28j60-ctl-sema init-sema
      0 self enc28j60-ctl-req !

      \ Initialize the frame interface
      self <enc28j60-frame-interface> self enc28j60-frame-interface init-object

      \ Instantiate the bus
      int-pin spi-pin spi-num mac-address <enc28j60-bus> self my-enc28j60-bus init-object

      \ Build the interrupt handler closures
      self dup enc28j60-dev-int-closure ['] handle-enc28j60-dev-irq bind
      self dup enc28j60-dma-int-closure ['] handle-enc28j60-dma-irq bind

      \ Set up transmit slot info
      0 self enc28j60-xmit-slot !
      0 self enc28j60-xmit-active !
      0 self enc28j60-xmit-len !

      \ Clear out receive state
      0 self current-rx-length !

    ; define new

    \ The destructor
    :noname { self -- }

      \ Destroy the bus
      self my-enc28j60-bus destroy

      \ Destroy the superclass
      self <object>->destroy

    ; define destroy

    \ Initialize the ENC28J60

    :noname { duplex self -- }

      [ debug? ] [if] cr ." starting initialization... " [then]

      \ Initialize the bus
      duplex self my-enc28j60-bus init-enc28j60-bus

      [ debug? ] [if] cr ." initialized bus" [then]

      \ Initialize the runner state
      ENC_STATE_DEVPOLL self enc28j60-state !

      [ debug? ] [if] cr ." enc28j60init done" [then]
    ; define init-enc28j60-runner

    \ Enable a MAC address.
    :noname { D: d-mac-addr self -- }
      self enc28j60-ctl-sema take
      d-mac-addr self enc28j60-ctl-req cell+ 2!
      1 self enc28j60-ctl-req !
      \ Tell the runner there's a control request
      [: notify-ctl-req or ;] 0 self enc28j60-runner-task @ notify-update
    ; define enc28j60-enable-mac

    \ Set duplex mode (true is full duplex)
    :noname { full-duplex self -- }
      self enc28j60-ctl-sema take
      full-duplex self enc28j60-ctl-req cell+ !
      2 self enc28j60-ctl-req !
      \ Tell the runner there's a control request
      [: notify-ctl-req or ;] 0 self enc28j60-runner-task @ notify-update
    ; define enc28j60-set-duplex
    
    \ Read and write the station MAC address
    :noname ( self -- D: mac-addr )
      my-enc28j60-bus mac-addr 2@
    ; define enc28j60-mac-addr@

    :noname ( D: mac-addr self -- )
      my-enc28j60-bus enc-mac!
    ; define enc28j60-mac-addr!

    \ This task handles receive and transmit actions.  It is invoked
    \ in several ways:
    \ from put-tx-frame to start transmitting
    \ from get-rx-frame to get more receives
    \ from the enc28j60 interrupt to process receive completion
    \ from the enc28j60 interrupt to process transmit completion
    \ from the dma interrupt to process rx or tx dma completion

    : _run-enc28j60 { self -- }
      self 1 [: { self }
	\ Dummy ask for transmit done to clear done status
	self my-enc28j60-bus enc28j60-tx-done? drop
	begin
	  \ debugging
	  [ debug? ] [if]
	    internal::validate
	  [else]
	    stack-base @ sp@ - 8 < triggers stack-underflow
	  [then]
	  \ Default to interrupts disabled
	  DMA_IRQ_0 NVIC_ICER_CLRENA!
	  IO_IRQ_BANK0 NVIC_ICER_CLRENA!
	  \ Is DMA done yet? 
	  self my-enc28j60-bus enc28j60-dma-done? if
	    [ debug? ] [if] cr ." runner begin state " self enc28j60-state @ . [then]
	    [ cdebug? ] [if] self enc28j60-state @  [char] a + emit [then]
	    \ First handle any DMA completions
	    self enc28j60-state @ case
	      ENC_STATE_RXDMA of self handle-rxdma-done endof
	      ENC_STATE_TXDMA of self handle-txdma-done endof
	    endcase
	    \ Any control action needed?
	    self handle-ctl
	    \ Now do the other (device related) actions
	    self handle-txdone
	    self handle-txstart
	    self handle-rxdone
	    self handle-txring
	    [ debug? ] [if] cr ." runner end state " self enc28j60-state @ . [then]
	    [ cdebug? ] [if] self enc28j60-state @ [char] A + emit [then]
	    \ Now that we've done all that, wait if needed and repeat
	    self enc28j60-state @ ENC_STATE_DEVPOLL = if
	      \ Didn't start DMA, is interrupt still asserted?
	      self my-enc28j60-bus int-pin @ pin@ if
		\ No, so wait
		[ debug? ] [if] cr ." runner wait for GPIO" [then]
		cdebug G
		IO_IRQ_BANK0 NVIC_ISER_SETENA!
		0 0 wait-notify-set-indefinite drop
	      else cdebug <
	      then
	    then
	  else
	    \ DMA still in progress, wait for it
	    [ debug? ] [if] cr ." runner wait for DMA" [then]
	    cdebug D
	    DMA_IRQ_0 NVIC_ISER_SETENA!
	    0 0 wait-notify-set-indefinite drop
	  then
	again
      ;] 2048 512 512 1 spawn-on-core { runner }
      c" enc28j60-runner" runner task-name!
      self enc28j60-notify-mb 1 runner config-notify
      runner self enc28j60-runner-task !
      runner self enc28j60-frame-interface enc28j60-owner-runner-task !
      \ Now hook up the ENC28J60 device and DMA interrupts
      self enc28j60-dev-int-closure GPIO_VECTOR vector!
      true self my-enc28j60-bus int-pin @ PROC1_INTE_GPIO_LEVEL_LOW!
      self enc28j60-dma-int-closure DMA_VECTOR vector!
      \ Start the runner task, then enable the receiver
      runner run
      true self my-enc28j60-bus enc28j60-enable-recv
    ; ' _run-enc28j60 define run-enc28j60

    \ Receive DMA done (to receive ring buffer).  Mark buffer complete
    \ and tell device also.
    :noname { self -- }
      [ debug? ] [if] cr ." rxDMA done begin" [then]
      cdebug R
      self my-enc28j60-bus recv-done
      \ Post completed buffer
      self enc28j60-scratch-buf self current-rx-length @
      self enc28j60-frame-interface poll-put-rx-frame drop
      ENC_STATE_DEVPOLL self enc28j60-state !
      [ debug? ] [if] cr ." rxDMA done end" [then]
      cdebug %
    ; define handle-rxdma-done

    \ Transmit DMA done.  Release buffer.
    :noname { self -- }
      [ debug? ] [if] cr ." txDMA done begin" [then]
      cdebug T
      self enc28j60-frame-interface retire-tx-frame
      ENC_STATE_DEVPOLL self enc28j60-state !
      [ debug? ] [if] cr ." txDMA done end" [then]
    ; define handle-txdma-done

    \ Handle transmit completion, if any
    :noname { self -- }
      self my-enc28j60-bus enc28j60-tx-done? if
	\ Mark no transmit in progress
	[ debug? ] [if] cr ." tx done begin slot " self enc28j60-xmit-slot @ . [then]
	cdebug t
	false self enc28j60-xmit-active !
	0 self enc28j60-xmit-slot @ 2* self enc28j60-xmit-len + h!
	\ Flip current/next transmit pointer to the other slot
	1 self enc28j60-xmit-slot xor!
	[ debug? ] [if] cr ." tx done end slot now " self enc28j60-xmit-slot @ . [then]
      then
    ; define handle-txdone

    \ Handle receive completion, if any
    :noname { self -- }
      self enc28j60-state @ ENC_STATE_DEVPOLL = if
	[ debug? ] [if] cr ." rx done begin" [then]
	\ Check for completed receive
	self my-enc28j60-bus recv-len ?dup if
	  [ debug? ] [if] cr ." rx done, len " dup . [then]
	  cdebug r
	  MTU min dup self current-rx-length !
	  \ Transfer the received data to the scratch buffer
	  \ TODO: transfer directly to the receive buffer
	  self enc28j60-scratch-buf swap self my-enc28j60-bus read-recv-mem
	  ENC_STATE_RXDMA self enc28j60-state !
	  [ debug? ] [if] cr ." rx done end" [then]
	else
	  \ No receives waiting, enable interrupt
	  [ debug? ] [if] cr ." no received packet" [then]
	  cdebug !
	  true self my-enc28j60-bus recv-int-ctl
	then
      then
    ; define handle-rxdone

    \ Start another transmit if we can.
    :noname { self -- }
      self enc28j60-state @ ENC_STATE_DEVPOLL = if
	self enc28j60-xmit-active @ 0= if
	  \ Transmit is idle, control request pending?
	  self enc28j60-ctl-req @ if
	    \ Yes, don't start another transmit
	    exit
	  then
	  [ debug? ] [if] cr ." tx idle" [then]
	  self enc28j60-xmit-slot @ dup 2* self enc28j60-xmit-len + h@ ?dup if
	    \ We have another packet waiting to be sent
	    [ debug? ] [if] cr ." tx start slot " 2dup swap . ." len " . [then]
	    cdebug x
	    self my-enc28j60-bus start-xmit averts x-tx-ring-err
	    true self enc28j60-xmit-active !
	    [ debug? ] [if] cr ." tx started" [then]
	    cdebug >
	  else
	    [ debug? ] [if] cr ." no waiting packet slot " dup . [then]
	    drop
	  then
	then
      then
    ; define handle-txstart

    \ Fill a transmit slot if we can.
    :noname { self -- }
      self enc28j60-state @ ENC_STATE_DEVPOLL = if
	self enc28j60-frame-interface poll-tx-frame if
	  [ debug? ] [if] cr ." txDMA buf " 2dup swap h.8 space ." len " . [then]
	  \ We have a transmit buffer, do we have a slot for it?
	  self enc28j60-xmit-slot @ dup 2* self enc28j60-xmit-len + h@ if
	    \ next-to-transmit slot is occupied, what about the other?
	    cdebug +
	    1 xor dup 2* self enc28j60-xmit-len + h@ if
	      \ No free slot, quit
	      [ debug? ] [if] cr ." no free tx slot" [then]
	      cdebug -
	      drop 2drop exit
	    then
	  then
	  \ We found a free slot, start the DMA
	  [ debug? ] [if] cr ." start txDMA slot " 3dup . swap
	    ." buf " h.8 space ." len " . [then]
	  cdebug X
	  2dup 2* self enc28j60-xmit-len + h!
	  self my-enc28j60-bus start-xmit-dma
	  ENC_STATE_TXDMA self enc28j60-state !
	else
	  2drop
	  [ debug? ] [if] cr ." txDMA no tx buf" [then]
	then
      then
    ; define handle-txring

    \ Handle a pending control request
    :noname { self -- }
      self enc28j60-ctl-req @ ?dup if
	\ Disable the receiver
	false self my-enc28j60-bus enc28j60-enable-recv
	\ Spin loop waiting for any transmit to finish
	begin
	  self my-enc28j60-bus enc28j60-tx-busy not
	until
	case
	  1 of
	    \ Enable MAC address (normally multicast address)
	    self enc28j60-ctl-req cell+ 2@ self my-enc28j60-bus enc28j60-bus-enable-mac
	  endof
	  2 of
	    \ Set duplex mode
	    self enc28j60-ctl-req cell+ @ self my-enc28j60-bus enc28j60-bus-set-duplex
	  endof
	endcase
	\ Done, enable receive
	true self my-enc28j60-bus enc28j60-enable-recv
	\ Mark request handled
	0 self enc28j60-ctl-req !
	self enc28j60-ctl-sema give
      then
    ; define handle-ctl
      
  end-implement
  
end-module
