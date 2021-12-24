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

begin-module multicore

  internal import
  interrupt import

  \ Must be carried out from core 0 only
  : x-core-0-only ( -- ) space ." core 0 only" cr ;
  
  \ Spinlock out of range exception
  : x-spinlock-out-of-range ( -- ) space ." spinlock out of range" cr ;
  
  \ Core out of range exception
  : x-core-out-of-range ( -- ) space ." core out of range" cr ;

  \ Core not addressable exception
  : x-core-not-addressable ( -- ) space ." core not addressable" cr ;

  begin-module multicore-internal
      
    \ SIO base
    $D0000000 constant SIO_BASE
    
    \ Prepare the return stack for an auxiliary core
    : prepare-aux-rstack ( xt stack-ptr rstack-ptr -- new-rstack-ptr )
      over @ over 2 cells - !
      tuck swap cell + swap cell - !
      ['] reboot 1 or over 3 cells - !
      swap 1 or swap 4 cells - tuck !
    ;
  
    \ Trampoline
    : trampoline ( --  ) [ $BCC3 h, $468E h, $4687 h, ] ; \ POP {R0, R1, R6, R7}; MOV LR, R1: MOV PC, R0
    
    \ Power-on state machine base
    $40010000 constant PSM_BASE
    
    \ Force off register
    PSM_BASE $4 + constant PSM_FRCE_OFF
    
    \ Force power-off processor 1 bit
    16 bit constant PSM_FRCE_OFF_PROC1

    \ Check whether a core is addressable
    : validate-addressable-core ( core -- )
      dup cpu-count u< averts x-core-out-of-range
      cpu-index <> averts x-core-not-addressable
    ;
  
  end-module> import

  \ Signal an event to the other core
  : sev ( -- ) [inlined] [ undefer-lit %1011111101000000 h, ] ;
  
  \ Wait for an event
  : wfe ( -- ) [inlined] [ undefer-lit %1011111100100000 h, ] ;

  \ SIO processor 0 IRQ
  15 constant SIO_IRQ_PROC0

  \ SIO processor 1 RIQ
  16 constant SIO_IRQ_PROC1

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

  \ Spinlock count
  32 constant spinlock-count

  \ Multitasker spinlock index
  31 constant task-spinlock

  \ Hardware spinlocks; reading will return 0 if already locked, or
  \ 1 lock-number lshift on success; writing any value will release the spinlock
  : SPINLOCK ( index -- addr ) SIO_BASE $100 + swap cells + ;

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
  : fifo-push-blocking ( x core -- )
    validate-addressable-core
    begin FIFO_ST_RDY FIFO_ST bit@ until FIFO_WR ! sev
  ;
  
  \ Blocking FIFO pop
  : fifo-pop-blocking ( core -- x )
    validate-addressable-core
    begin FIFO_ST_VLD FIFO_ST bit@ until FIFO_RD @ sev
  ;

  \ Attempt to send data on a FIFO and confirm that the same data is sent back.
  : fifo-push-confirm ( x core -- confirmed? )
    dup validate-addressable-core
    2dup fifo-push-blocking fifo-pop-blocking =
  ;

  \ Launch an auxiliary core
  : launch-aux-core ( xt stack-ptr rstack-ptr core -- )
    1 = averts x-core-out-of-range
    prepare-aux-rstack
    SIO_IRQ_PROC0 NVIC_ICER_CLRENA!
\    PSM_FRCE_OFF_PROC1 PSM_FRCE_OFF bic!
    6 0 do
      i case
	0 of 1 fifo-drain 0 endof
	1 of 1 fifo-drain 0 endof
	2 of 1 endof
	3 of vector-table endof
	4 of dup endof
	5 of ['] trampoline 3 + endof \ Bypass the initial PUSH {LR}
      endcase
      1 fifo-push-confirm if
	1
      else
	i negate
      then
    +loop
    drop
  ;

  \ Reset an auxiliary core
  : reset-aux-core ( core -- )
    1 = averts x-core-out-of-range
    PSM_FRCE_OFF_PROC1 PSM_FRCE_OFF bis!
    begin PSM_FRCE_OFF_PROC1 PSM_FRCE_OFF bit@ until
    PSM_FRCE_OFF_PROC1 PSM_FRCE_OFF bic!
    1 fifo-pop-blocking drop
  ;

end-module> import

\ Set up reboot to reset the second core
: reboot ( -- )
  cpu-index 0 = averts x-core-0-only
  1 reset-aux-core
  reboot
;

\ Reboot
reboot
