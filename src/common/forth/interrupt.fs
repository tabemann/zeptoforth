\ Copyright (c) 2020-2021 Travis Bemann
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

\ Compile to flash
compile-to-flash

compress-flash

begin-module-once interrupt-module

  \ Invalid interrupt vector index exception
  : x-invalid-vector ( -- ) space ." invalid vector" cr ;

  \ ICSR register
  $E000ED04 constant ICSR

  \ SHPRx registers
  $E000ED18 constant SHPR1
  $E000ED1C constant SHPR2
  $E000ED20 constant SHPR3

  \ NVIC base address
  $E000E100 constant NVIC_Base

  commit-flash
  
  \ NVIC interrupt set-enable register base address
  NVIC_Base constant NVIC_ISER_Base

  \ NVIC interrupt clear-enable register base address
  NVIC_Base $80 + constant NVIC_ICER_Base

  \ NVIC interrupt set-pending register base address
  NVIC_Base $100 + constant NVIC_ISPR_Base

  \ NVIC interrupt clear-pending register base address
  NVIC_Base $180 + constant NVIC_ICPR_Base

  \ NVIC interrupt active bit register base address
  NVIC_Base $200 + constant NVIC_IABR_Base

  \ NVIC interrupt priority register base register
  NVIC_Base $300 + constant NVIC_IPR_Base

  commit-flash
  
  \ Set an interrupt vector
  : vector! ( xt vector-index -- )
    dup 0 > over vector-count < or averts x-invalid-vector
    swap 1+ swap cells vector-table + !
  ;

  \ Get an interrupt vector
  : vector@ ( vector-index -- xt )
    dup 0 > over vector-count < or averts x-invalid-vector
    cells vector-table + @ 1-
  ;

  \ Get the active interrupt
  : ICSR_VECTACTIVE@ ( -- interrupt ) ICSR @ $1FF and ;
  
  \ Set system fault handler priority field 4, for memory management fault
  : SHPR1_PRI_4! ( u -- ) $F0 and 0 lshift SHPR1 @ $FF bic or SHPR1 ! ;

  \ Set system fault handler priority field 5, for bus fault
  : SHPR1_PRI_5! ( u -- ) $F0 and 8 lshift SHPR1 @ $FF00 bic or SHPR1 ! ;

  \ Set system fault handler priority field 6, for usage fault
  : SHPR1_PRI_6! ( u -- ) $F0 and 16 lshift SHPR1 @ $FF0000 bic or SHPR1 ! ;

  \ Set system fault handler priority field 11, for SVCall
  : SHPR2_PRI_11! ( u -- ) $F0 and 24 lshift SHPR2 @ $FF000000 bic or SHPR2 ! ;

  \ Set system fault handler priority field 14, for PendSV
  : SHPR3_PRI_14! ( u -- ) $F0 and 16 lshift SHPR3 @ $FF0000 bic or SHPR3 ! ;

  \ Set system fault handler priority field 15, for SysTick
  : SHPR3_PRI_15! ( u -- ) $F0 and 24 lshift SHPR3 @ $FF000000 bic or SHPR3 !  ;

  \ Get system fault handler priority field 4, for memory management fault
  : SHPR1_PRI_4@ ( -- u ) SHPR1 @ 0 rshift $FF and ;

  \ Get system fault handler priority field 5, for bus fault
  : SHPR1_PRI_5@ ( -- u ) SHPR1 @ 8 rshift $FF and ;

  \ Get system fault handler priority field 6, for usage fault
  : SHPR1_PRI_6@ ( -- u ) SHPR1 @ 16 rshift $FF and ;

  \ Get system fault handler priority field 11, for SVCall
  : SHPR2_PRI_11@ ( -- u ) SHPR2 @ 24 rshift $FF and ;

  \ Get system fault handler priority field 14, for PendSV
  : SHPR3_PRI_14@ ( -- u ) SHPR3 @ 16 rshift $FF and ;

  \ Get system fault handler priority field 15, for SysTick
  : SHPR3_PRI_15@ ( -- u ) SHPR3 @ 24 rshift $FF and ;

  \ Set PENDSVSET
  : ICSR_PENDSVSET! ( -- ) [ 1 28 lshift ] literal ICSR bis! ;

  \ Set PENDSVCLR
  : ICSR_PENDSVCLR! ( -- ) [ 1 27 lshift ] literal ICSR bis! ;

  \ Get PENDSVSET
  : ICSR_PENDSVSET@ ( -- bit ) [ 1 28 lshift ] literal ICSR bit@ ;

  \ Initiate an SVCall
  : svc ( -- ) [ $DF00 h, ] [inlined] ;

  \ Set NVIC interrupt set-enable
  : NVIC_ISER_SETENA! ( u -- )
    dup 32 / cells NVIC_ISER_Base + swap 32 mod 1 swap lshift swap bis!
  ;

  \ Get NVIC interrupt set-enable
  : NVIC_ISER_SETENA@ ( u -- bit )
    dup 32 / cells NVIC_ISER_Base + @ swap 32 mod 1 swap lshift swap bit@
  ;

  \ Set NVIC interrupt clear-enable
  : NVIC_ICER_CLRENA! ( u -- )
    dup 32 / cells NVIC_ICER_Base + swap 32 mod 1 swap lshift swap bis!
  ;

  \ Get NVIC interrupt clear-enable
  : NVIC_ICER_CLRENA@ ( u -- bit )
    dup 32 / cells NVIC_ICER_Base + @ swap 32 mod 1 swap lshift swap bit@
  ;

  \ Set NVIC interrupt set-pending
  : NVIC_ISPR_SETPEND! ( u -- )
    dup 32 / cells NVIC_ISPR_Base + swap 32 mod 1 swap lshift swap bis!
  ;

  \ Get NVIC interrupt set-pending
  : NVIC_ISPR_SETPEND@ ( u -- bit )
    dup 32 / cells NVIC_ISPR_Base + @ swap 32 mod 1 swap lshift swap bit@
  ;

  \ Set NVIC interrupt clear-pending
  : NVIC_ICPR_CLRPEND! ( u -- )
    dup 32 / cells NVIC_ICPR_Base + swap 32 mod 1 swap lshift swap bis!
  ;

  \ Get NVIC interrupt clear-pending
  : NVIC_ICPR_CLRPEND@ ( u -- bit )
    dup 32 / cells NVIC_ICPR_Base + @ swap 32 mod 1 swap lshift swap bit@
  ;

  \ Get NVIC interrupt active bit
  : NVIC_IABR_ACTIVE@ ( u -- bit )
    dup 32 / cells NVIC_IABR_Base + @ swap 32 mod 1 swap lshift swap bit@
  ;

  \ Set NVIC interrupt priority register field
  : NVIC_IPR_IP! ( priority u -- ) NVIC_IPR_Base + c! ;

  \ Get NVIC interrupt priority register field
  : NVIC_IPR_IP@ ( u -- priority ) NVIC_IPR_Base + c@ ;

end-module

import interrupt-module

\ Add clearing of registers to warm
: warm ( -- )
  8 0 ?do $FF NVIC_ICER_Base i cells + ! loop warm
;

unimport interrupt-module

end-compress-flash

\ Reboot
reboot
