\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ Compile to flash
compile-to-flash

\ Set up the wordlist
forth-wordlist 1 set-order
forth-wordlist set-current
wordlist constant interrupt-wordlist
forth-wordlist interrupt-wordlist 2 set-order
interrupt-wordlist set-current

\ SHPRx registers
$E000ED18 constant SHPR1
$E000ED1C constant SHPR2
$E000ED20 constant SHPR3

\ Set system fault handler priority field 4, for memory management fault
: SHPR1_PRI_4! ( u -- ) $FF and 0 lshift SHPR1 @ $F0 bic or SHPR1 ! ;

\ Set system fault handler priority field 5, for bus fault
: SHPR1_PRI_5! ( u -- ) $FF and 8 lshift SHPR1 @ $F000 bic or SHPR1 ! ;

\ Set system fault handler priority field 6, for usage fault
: SHPR1_PRI_6! ( u -- ) $FF and 16 lshift SHPR1 @ $F00000 bic or SHPR1 ! ;

\ Set system fault handler priority field 11, for SVCall
: SHPR2_PRI_11! ( u -- ) $FF and 24 lshift SHPR2 @ $F0000000 bic or SHPR2 ! ;

\ Set system fault handler priority field 14, for PendSV
: SHPR3_PRI_14! ( u -- ) $FF and 16 lshift SHPR3 @ $F00000 bic or SHPR3 ! ;

\ Set system fault handler priority field 15, for SysTick
: SHPR3_PRI_15! ( u -- ) $FF and 24 lshift SHPR3 @ $F0000000 bic or SHPR3 !  ;

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

\ ICSR register
$E000ED04 constant ICSR

\ Set PENDSVSET
: ICSR_PENDSVSET! ( -- ) 1 28 lshift ICSR bis! ;

\ Set PENDSVCLR
: ICSR_PENDSVCLR! ( -- ) 1 27 lshift ICSR bis! ;

\ Get PENDSVSET
: ICSR_PENDSVSET@ ( -- bit ) 1 28 lshift ICSR bit@ ;

\ Initiate an SVCall
: svc ( -- ) [ $DF00 h, ] [inlined] ;

\ NVIC base address
$E000E100 constant NVIC_Base

\ NVIC interrupt set-enable register base address
NVIC_Base constant NVIC_ISER_Base

\ Set NVIC interrupt set-enable
: NVIC_ISER_SETENA! ( u -- )
  dup 32 / cells NVIC_ISER_Base + swap 32 mod 1 swap lshift swap bis!
;

\ Get NVIC interrupt set-enable
: NVIC_ISER_SETENA@ ( u -- bit )
  dup 32 / cells NVIC_ISER_Base + @ swap 32 mod 1 swap lshift swap bit@
;

\ NVIC interrupt clear-enable register base address
NVIC_Base $80 + constant NVIC_ICER_Base

\ Set NVIC interrupt clear-enable
: NVIC_ICER_CLRENA! ( u -- )
  dup 32 / cells NVIC_ICER_Base + swap 32 mod 1 swap lshift swap bis!
;

\ Get NVIC interrupt clear-enable
: NVIC_ICER_CLRENA@ ( u -- bit )
  dup 32 / cells NVIC_ICER_Base + @ swap 32 mod 1 swap lshift swap bit@
;

\ NVIC interrupt set-pending register base address
NVIC_Base $100 + constant NVIC_ISPR_Base

\ Set NVIC interrupt set-pending
: NVIC_ISPR_SETPEND! ( u -- )
  dup 32 / cells NVIC_ISPR_Base + swap 32 mod 1 swap lshift swap bis!
;

\ Get NVIC interrupt set-pending
: NVIC_ISPR_SETPEND@ ( u -- bit )
  dup 32 / cells NVIC_ISPR_Base + @ swap 32 mod 1 swap lshift swap bit@
;

\ NVIC interrupt clear-pending register base address
NVIC_Base $180 + constant NVIC_ICPR_Base

\ Set NVIC interrupt clear-pending
: NVIC_ICPR_CLRPEND! ( u -- )
  dup 32 / cells NVIC_ICPR_Base + swap 32 mod 1 swap lshift swap bis!
;

\ Get NVIC interrupt clear-pending
: NVIC_ICPR_CLRPEND@ ( u -- bit )
  dup 32 / cells NVIC_ICPR_Base + @ swap 32 mod 1 swap lshift swap bit@
;

\ NVIC interrupt active bit register base address
NVIC_Base $200 + constant NVIC_IABR_Base

\ Get NVIC interrupt active bit
: NVIC_IABR_ACTIVE@ ( u -- bit )
  dup 32 / cells NVIC_IABR_Base + @ swap 32 mod 1 swap lshift swap bit@
;

\ NVIC interrupt priority register base register
NVIC_Base $300 + constant NVIC_IPR_Base

\ Set NVIC interrupt priority register field
: NVIC_IPR_IP! ( priority u -- )
  dup 4 / cells NVIC_IPR_Base + @ $FF 2 pick 4 mod 8 * lshift bic
  rot $FF and 2 pick 4 mod 8 + lshift or
  swap 4 / cells NVIC_IPR_Base + !
;

\ Get NVIC interrupt priority register field
: NVIC_IPR_IP@ ( u -- priority )
  dup 4 / cells NVIC_IPR_Base + @ swap 4 mod 8 * rshift $FF and
;

\ Warm reboot
warm
