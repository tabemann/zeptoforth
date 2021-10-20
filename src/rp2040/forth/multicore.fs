\ Copyright (c) 2021 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-module-once multicore-module

  import internal-module
  import interrupt-module

  begin-import-module multicore-internal-module
      
    \ SIO base
    $D0000000 constant SIO_BASE

  end-module

  \ SIO processor 0 IRQ
  15 constant SIO_IRQ_PROC0

  \ SIO processor 1 RIQ
  16 constant SIO_IRQ_PROC1

  \ Signal an event to the other core
  : sev ( -- ) [: undefer-lit %1011111101000000 h, ;] ;

  \ Wait for an event
  : wfe ( -- ) [: undefer-lit %1011111100100000 h, ;] ;

  \ CPUID register (contains 0 for core 0, 1 for core 1)
  SIO_BASE $000 + constant CPUID

  \ FIFO status register; note that core 0 can see the read side of the 1 -> 0
  \ FIFO and the write side of the 0 -> 1 FIFO; the converse is true of core 1
  SIO_BASE $050 + constant FIFO_ST

  \ Sticky flag indicating the RX FIFO was read when empty; write to clear
  3 bit constant FIFO_ST_ROE

  \ Sticky flag indicating the TX FIFO was written when full; write to clear
  2 bit constant FIFO_ST_WOF

  \ Set if this core's TX FIFO is not full
  1 bit constant FIFO_ST_RDY

  \ Set if this core's RX FIFO is not empty
  0 bit constant FIFO_ST_VLD

  \ Write access from this core's TX FIFO
  SIO_BASE $054 + constant FIFO_WR

  \ Read access to this core's RX FIFO
  SIO_BASE $058 + constant FIFO_RD

  \ Power-on state machine base
  $40010000 constant PSM_BASE

  \ Force off register
  PSM_BASE $4 + constant PSM_FRCE_OFF
  
  \ Force power-off processor 1 bit
  16 bit constant PSM_FRCE_OFF_PROC1

  \ Spinlock count
  32 constant spinlock-count

  \ Multitasker spinlock index
  31 constant task-spinlock

  \ Hardware spinlocks; reading will return 0 if already locked, or
  \ 1 lock-number lshift on success; writing any value will release the spinlock
  : SPINLOCK ( index -- addr ) SIO_BASE $100 swap cells + ;

  \ Spinlock out of range exception
  : x-spinlock-out-of-range ( -- ) space ." spinlock out of range" cr ;
  
  \ Core out of range exception
  : x-core-out-of-range ( -- ) space ." core out of range" cr ;

  \ Core not addressable exception
  : x-core-not-addressable ( -- ) space ." core not addressable" cr ;

  \ Check whether a core is addressable
  : validate-addressable-core ( core -- )
    dup cpu-count u< averts x-core-out-of-range
    cpu-index <> averts x-core-not-addressable
  ;
  
  \ Claim a spinlock
  : claim-spinlock ( index -- )
    dup spinlock-count u< averts x-spinlock-out-of-range
    SPINLOCK begin dup @ 0<> until drop
  ;

  \ Release a spinlock
  : release-spinlock ( index -- )
    dup spinlock-count u< averts x-spinlock-out-of-range
    SPINLOCK -1 swap !
  ;
  
  \ Drain a multicore FIFO
  : fifo-drain ( core -- )
    validate-addressable-core
    begin FIFO_ST_VLD FIFO_ST bit@ while FIFO_RD @ drop repeat sev
  ;
  
  \ Blocking FIFO push
  : fifo-push-blocking ( core -- )
    validate-addressable-core
    begin FIFO_ST_RDY FIFO_ST bit@ until FIFO_WR ! sev
  ;
  
  \ Blocking FIFO pop
  : fifo-pop-blocking ( core -- )
    validate-addressable-core
    begin FIFO_ST_VLD FIFO_ST bit@ until FIFO_RD @
  ;

  \ Blocking FIFO pop, sleeping the current core in the process
  : fifo-pop-blocking-sleep ( core -- )
    validate-addressable-core
    begin FIFO_ST_VLD FIFO_ST bit@ not while wfe repeat FIFO_RD @
  ;

  \ Keep on sending data on the FIFO until a positive response is received
  : fifo-push-confirm ( x core -- )
    validate-addressable-core
    begin dup fifo-push-blocking fifo-pop-blocking until
    drop
  ;
  
  \ Launch an auxiliary core
  : launch-aux-core ( vector-table stack-pointer core -- )
    1 = averts x-core-out-of-range
    SIO_IRQ_PROC0 NVIC_ISER_SETENA@ >r
    SIO_IRQ_PROC0 NVIC_ICER_CLRENA!
    fifo-drain 0 1 fifo-push-confirm
    fifo-drain 0 1 fifo-push-confirm
    rot 1 fifo-push-confirm
    swap 1 fifo-push-confirm
    ['] aux-core-entry 1 fifo-push-confirm
    >r if SIO_IRQ_PROC0 NVIC_ISER_SETENV! then
  ;

  \ Force core 1 off
  : force-core-1-off ( -- )
    PSM_FRCE_OFF_PROC1 PSM_FRCE_OFF bis!
    begin PSM_FRCE_OFF_PROC1 PSM_FRCE_OFF bit@ until
  ;

  \ Restore core 1
  : restore-core-1 ( -- )
    PSM_FRCE_OFF_PROC1 PSM_FRCE_OFF bic!
    fifo-pop-blocking drop
  ;

  \ Reset core 1
  : reset-core-1 ( -- )
    force-core-1-off
    restore-core-1
  ;

end-module