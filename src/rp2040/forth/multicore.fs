\ Copyright (c) 2021-2023 Travis Bemann
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
  armv6m import

  \ Must be carried out from core 0 only
  : x-core-0-only ( -- ) ." core 0 only" cr ;
  
  \ Spinlock out of range exception
  : x-spinlock-out-of-range ( -- ) ." spinlock out of range" cr ;
  
  \ Core out of range exception
  : x-core-out-of-range ( -- ) ." core out of range" cr ;

  \ Core not addressable exception
  : x-core-not-addressable ( -- ) ." core not addressable" cr ;

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

  \ Multitasker core 1 spinlock index
  31 constant task-core-1-spinlock

  \ Multitasker core 0 spinlock index
  30 constant task-core-0-spinlock
  
  \ Serial spinlock index
  29 constant serial-spinlock

  \ Test and set spinlock index
  28 constant test-set-spinlock

  \ RTC spinlock index
  27 constant rtc-spinlock
  
  continue-module multicore-internal

    \ Spinlock lock counts
    spinlock-count cpu-count * cells buffer: spinlock-lock-counts

    \ Get the address of a spinlock lock count
    : spinlock-lock-count ( index -- addr )
      cpu-index
      spinlock-lock-counts
      code[
      r1 r0 2 dp ldm
      1 r1 r1 lsls_,_,#_
      r1 r0 r0 adds_,_,_
      2 r0 r0 lsls_,_,#_
      r0 tos tos adds_,_,_
      ]code
    ;
      
\      1 lshift cpu-index + 2 lshift spinlock-lock-counts +
\      cpu-count * cpu-index + cells spinlock-lock-counts +
\    ;
    
  end-module
  
  \ Hardware spinlocks; reading will return 0 if already locked, or
  \ 1 lock-number lshift on success; writing any value will release the spinlock
  : SPINLOCK ( index -- addr )
    [ SIO_BASE $100 + ] literal
    code[
    r0 1 dp ldm
    2 r0 r0 lsls_,_,#_
    r0 tos tos adds_,_,_
    ]code
  ;
  
\    [ SIO_BASE $100 + ] literal swap 2 lshift + ;

  \ Just claim a spinlock
  : claim-spinlock-raw ( index -- )
    dup spinlock-lock-count
    code[
    0 tos r0 ldr_,[_,#_]
    1 r0 adds_,#_
    0 tos r0 str_,[_,#_]
    1 r0 cmp_,#_
    eq bc>
    tos r0 2 dp ldm
    pc 1 pop
    >mark
    tos 1 dp ldm
    ]code
    SPINLOCK
    code[
    mark>
    0 tos r0 ldr_,[_,#_]
    0 r0 cmp_,#_
    eq bc<
    tos 1 dp ldm
    ]code
  ;

  \   dup spinlock-lock-count dup @ 1+ dup rot !
  \   1 = if SPINLOCK begin dup @ until then drop
  \ ;

  \ Just release a spinlock ( index -- )
  : release-spinlock-raw ( index -- )
    dup spinlock-lock-count
    code[
    0 tos r0 ldr_,[_,#_]
    1 r0 subs_,#_
    0 tos r0 str_,[_,#_]
    0 r0 cmp_,#_
    eq bc>
    tos r0 2 dp ldm
    pc 1 pop
    >mark
    tos 1 dp ldm
    ]code
    SPINLOCK
    code[
    0 r0 movs_,#_
    r0 r0 mvns_,_
    0 tos r0 str_,[_,#_]
    tos 1 dp ldm
    ]code
  ;
  
\    dup spinlock-lock-count dup @ 1- dup rot !
\    0= if SPINLOCK -1 swap ! else drop then
\  ;

  \ Claim a spinlock
  : claim-spinlock ( index -- )
    dup spinlock-lock-count
    code[
    cpsid
    0 tos r0 ldr_,[_,#_]
    1 r0 adds_,#_
    0 tos r0 str_,[_,#_]
\    cpsie \ DEBUG
    1 r0 cmp_,#_
    eq bc>
    cpsie \ Trying this
    tos r0 2 dp ldm
    pc 1 pop
    >mark
    tos 1 dp ldm
    ]code
    SPINLOCK
    code[
    mark>
    0 tos r0 ldr_,[_,#_]
    0 r0 cmp_,#_
    eq bc<
    cpsie \ Trying this
    tos 1 dp ldm
    ]code
  ;
  
\    dup spinlock-count u< averts x-spinlock-out-of-range
\    disable-int
\    dup spinlock-lock-count dup @ 1+ dup rot !
\    enable-int
\    1 = if SPINLOCK begin dup @ until then drop
\  ;

  \ Release a spinlock
  : release-spinlock ( index -- )
    dup spinlock-lock-count
    code[
    cpsid
    0 tos r0 ldr_,[_,#_]
    1 r0 subs_,#_
    0 tos r0 str_,[_,#_]
    0 r0 cmp_,#_
    eq bc>
    tos r0 2 dp ldm
    cpsie
    pc 1 pop
    >mark
    tos 1 dp ldm
    ]code
    SPINLOCK
    code[
    0 r0 movs_,#_
    r0 r0 mvns_,_
    0 tos r0 str_,[_,#_]
    tos 1 dp ldm
    cpsie
    ]code
  ;

\    dup spinlock-count u< averts x-spinlock-out-of-range
\    disable-int
\    dup spinlock-lock-count dup @ 1- dup rot !
\    0= if SPINLOCK -1 swap ! else drop then
\    enable-int
\  ;

  \ Claim a spinlock and begin critical
  : claim-spinlock-begin-critical ( index -- )
    disable-int
    claim-spinlock-raw
    1 internal::in-critical +!
    enable-int
  ;

  \ Release a spinlock and end critical
  : release-spinlock-end-critical ( index -- )
    disable-int
    release-spinlock-raw
    internal::in-critical @ 1- 0 max dup internal::in-critical !
    0= if
      internal::deferred-context-switch @
      false internal::deferred-context-switch !
      if pause then
    then
    enable-int
  ;

  \ Claim a spinlock for the current core's multitasker
  : claim-same-core-spinlock ( -- )
    cpu-index task-core-0-spinlock + claim-spinlock
  ;

  \ Release a spinlock for the current core's multitasker
  : release-same-core-spinlock ( -- )
    cpu-index task-core-0-spinlock + release-spinlock
  ;

  \ Just claim a spinlock for all cores' multitasker
  : claim-all-core-spinlock-raw ( -- )
    task-core-0-spinlock claim-spinlock-raw
    task-core-1-spinlock claim-spinlock-raw
  ;

  \ Just release a spinlock for all-cores multitasker
  : release-all-core-spinlock-raw ( -- )
    task-core-0-spinlock release-spinlock-raw
    task-core-1-spinlock release-spinlock-raw
  ;

  \ Claim a spinlock for a different core's multitasker
  : claim-other-core-spinlock ( core -- )
    task-core-0-spinlock + claim-spinlock
  ;

  \ Release a spinlock for the other core's multitasker
  : release-other-core-spinlock ( core -- )
    task-core-0-spinlock + release-spinlock
  ;
  
  \ Claim all core's multitasker's spinlocks
  : claim-all-core-spinlock ( -- )
    task-core-0-spinlock claim-spinlock
    task-core-1-spinlock claim-spinlock
  ;

  \ Release all core's multitasker's spinlocks
  : release-all-core-spinlock ( -- )
    task-core-0-spinlock release-spinlock
    task-core-1-spinlock release-spinlock
  ;

  \ Claim all core's multitasker's spinlocks and begin critcal
  : claim-all-core-spinlock-begin-critical ( -- )
    disable-int
    task-core-0-spinlock claim-spinlock-raw
    task-core-1-spinlock claim-spinlock-raw
    1 internal::in-critical +!
    enable-int
  ;

  \ Release all core's multitasker's spinlocks and end critical
  : release-all-core-spinlock-end-critical ( -- )
    disable-int
    task-core-0-spinlock release-spinlock-raw
    task-core-1-spinlock release-spinlock-raw
    internal::in-critical @ 1- 0 max dup internal::in-critical !
    0= if
      internal::deferred-context-switch @
      false internal::deferred-context-switch !
      if pause then
    then
    enable-int
  ;

  \ Claim a spinlock, releasing it afterwards
  : with-spinlock ( xt spinlock -- )
    >r r@ claim-spinlock try r> release-spinlock ?raise
  ;

  \ Enter a critical section and claim a spinlock, releasing it afterwards
  : critical-with-spinlock ( xt spinlock -- )
    >r r@ claim-spinlock-begin-critical try
    r> release-spinlock-end-critical ?raise
  ;
  
  \ Enter a critical section and claim another core's multitasker's spinlock,
  \ releasing it afterwards
  : critical-with-other-core-spinlock ( xt core -- )
    >r r@ claim-spinlock-begin-critical try
    r> release-spinlock-end-critical ?raise
  ;

  \ Enter a critical section and claim both cores' spinlocks, releasing then
  \ afterwards
  : critical-with-all-core-spinlock ( xt -- )
    claim-all-core-spinlock-begin-critical try
    release-all-core-spinlock-end-critical ?raise
  ;

  \ Test and set
  : test-set ( value addr -- set? )
    [ test-set-spinlock SPINLOCK ] literal
    code[
    cpsid
    r1 r0 2 dp ldm
    mark>
    0 tos r2 ldr_,[_,#_]
    0 r2 cmp_,#_
    eq bc<
    0 r0 r2 ldr_,[_,#_]
    0 r2 cmp_,#_
    ne bc>
    0 r0 r1 str_,[_,#_]
    0 r2 movs_,#_
    r2 r2 mvns_,_
    0 tos r2 str_,[_,#_]
    r2 tos movs_,_
    cpsie
    pc 1 pop
    >mark
    0 r2 movs_,#_
    r2 r2 mvns_,_
    0 tos r2 str_,[_,#_]
    0 tos movs_,#_
    cpsie
    ]code
  ;

  \ Test and set without touching interrupts
  : test-set-raw ( value addr -- set? )
    [ test-set-spinlock SPINLOCK ] literal
    code[
    r1 r0 2 dp ldm
    mark>
    0 tos r2 ldr_,[_,#_]
    0 r2 cmp_,#_
    eq bc<
    0 r0 r2 ldr_,[_,#_]
    0 r2 cmp_,#_
    ne bc>
    0 r0 r1 str_,[_,#_]
    0 r2 movs_,#_
    r2 r2 mvns_,_
    0 tos r2 str_,[_,#_]
    r2 tos movs_,_
    pc 1 pop
    >mark
    0 r2 movs_,#_
    r2 r2 mvns_,_
    0 tos r2 str_,[_,#_]
    0 tos movs_,#_
    ]code
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
\    true core-1-launched !
    $00 SIO_IRQ_PROC0 NVIC_IPR_IP!
    SIO_IRQ_PROC0 NVIC_ISER_SETENA!
  ;

  \ Prepare for rebooting the second core
  : prepare-reboot ( -- )
    cpu-index 0= if
      hold-core
      spinlock-count 0 ?do -1 i SPINLOCK ! loop
    else
      disable-int
      $B007B007 0 fifo-push-blocking
      begin again
    then
  ;    

  \ Reset an auxiliary core
  : reset-aux-core ( core -- )
    1 = averts x-core-out-of-range
  ;

end-module> import

\ Initialize
: init ( -- )
  init
  spinlock-count cpu-count * 0 ?do
    0 multicore::multicore-internal::spinlock-lock-counts i cells + !
  loop
  [:
    $B007B007 = if [: prepare-reboot reboot ;] critical then
  ;] sio-hook !
;

\ Set up reboot to reset the second core
: reboot ( -- ) [: prepare-reboot reboot ;] critical ;
\ Reboot
reboot
