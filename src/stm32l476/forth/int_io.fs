\ Copyright (c) 2013? Matthias Koch
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

\ RAM variable for rx buffer read-index
bvariable rx-read-index

\ RAM variable for rx buffer write-index
bvariable rx-write-index

\ Constant for number of bytes to buffer
128 constant rx-buffer-size

\ Rx buffer
rx-buffer-size ram-buffer: rx-buffer

\ RAM variable for tx buffer read-index
bvariable tx-read-index

\ RAM variable for tx buffer write-index
bvariable tx-write-index

\ Constant for number of bytes to buffer
128 constant tx-buffer-size

\ Tx buffer
tx-buffer-size ram-buffer: tx-buffer

\ USART2
$40004400 constant USART2_Base
USART2_Base $00 + constant USART2_CR1
USART2_Base $1C + constant USART2_ISR
USART2_Base $24 + constant USART2_RDR
USART2_Base $28 + constant USART2_TDR

$40021000 constant RCC_Base
RCC_Base $78 + constant RCC_APB1SMENR1 ( APB1SMENR1 )
: RCC_APB1SMENR1_USART2SMEN   %1 17 lshift RCC_APB1SMENR1 bis! ;  \ RCC_APB1SMENR1_USART2SMEN    USART2 clocks enable during Sleep and  Stop modes
: RCC_APB1SMENR1_USART2SMEN_Clear   %1 17 lshift RCC_APB1SMENR1 bic! ;  \ RCC_APB1SMENR1_USART2SMEN    USART2 clocks enable during Sleep and  Stop modes
: USART2_CR1_TXEIE   %1 7 lshift USART2_CR1 bis! ;  \ USART2_CR1_TXEIE    interrupt enable
: USART2_CR1_RXNEIE   %1 5 lshift USART2_CR1 bis! ;  \ USART2_CR1_RXNEIE    RXNE interrupt enable
: USART2_CR1_TXEIE_Clear   %1 7 lshift USART2_CR1 bic! ;  \ USART2_CR1_TXEIE    interrupt disable
: USART2_CR1_RXNEIE_Clear   %1 5 lshift USART2_CR1 bic! ;  \ USART2_CR1_RXNEIE    RXNE interrupt enable
$E000E000 constant NVIC_Base ( Nested Vectored Interrupt  Controller )
NVIC_Base $104 + constant NVIC_ISER1 ( Interrupt Set-Enable Register )
: NVIC_ISER1_SETENA   ( %XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX -- ) 0 lshift NVIC_ISER1 bis! ;  \ NVIC_ISER1_SETENA    SETENA
NVIC_Base $184 + constant NVIC_ICER1 ( Interrupt Clear-Enable Register )
: NVIC_ICER1_CLRENA   ( %XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX -- ) 0 lshift NVIC_ICER1 bis! ;  \ NVIC_ICER1_CLRENA    CLRENA
NVIC_Base $284 + constant NVIC_ICPR1 ( Interrupt Clear-Pending  Register )
USART2_Base $20 + constant USART2_ICR ( Interrupt flag clear register ) 
: USART2_ICR_ORECF %1 3 lshift USART2_ICR bis! ; ( Overrun error clear flag )  

$20 constant RXNE
$80 constant TXE
$08 constant ORE

\ Get whether the rx buffer is full
: rx-full? ( -- f )
  rx-write-index b@ rx-read-index b@
  rx-buffer-size 1- + rx-buffer-size umod =
;

\ Get whether the rx buffer is empty
: rx-empty? ( -- f )
  rx-read-index b@ rx-write-index b@ =
;

\ Write a byte to the rx buffer
: write-rx ( c -- )
  rx-full? not if
    rx-write-index b@ rx-buffer + b!
    rx-write-index b@ 1+ rx-buffer-size mod rx-write-index b!
  else
    drop
  then
;

\ Read a byte from the rx buffer
: read-rx ( -- c )
  rx-empty? not if
    rx-read-index b@ rx-buffer + b@
    rx-read-index b@ 1+ rx-buffer-size mod rx-read-index b!
  else
    0
  then
;

\ Get whether the tx buffer is full
: tx-full? ( -- f )
  tx-write-index b@ tx-read-index b@
  tx-buffer-size 1- + tx-buffer-size umod =
;

\ Get whether the tx buffer is empty
: tx-empty? ( -- f )
  tx-read-index b@ tx-write-index b@ =
;

\ Write a byte to the tx buffer
: write-tx ( c -- )
  tx-full? not if
    tx-write-index b@ tx-buffer + b!
    tx-write-index b@ 1+ tx-buffer-size mod tx-write-index b!
  else
    drop
  then
;

\ Read a byte from the tx buffer
: read-tx ( -- c )
  tx-empty? not if
    tx-read-index b@ tx-buffer + b@
    tx-read-index b@ 1+ tx-buffer-size mod tx-read-index b!
  else
    0
  then
;

\ Handle IO
: handle-io ( -- )
  disable-int
  begin
    rx-full? not if
      USART2_ISR @ RXNE and if
	USART2_RDR b@ write-rx false
      else
	true
      then
    else
      true
    then
  until
  rx-full? if
    USART2_CR1_RXNEIE_Clear
  then
  begin
    tx-empty? not if
      USART2_ISR @ TXE and if
	read-tx USART2_TDR b! false
      else
	true
      then
    else
      true
    then
  until
  tx-empty? if
    USART2_CR1_TXEIE_Clear
  then
  USART2_ISR @ ORE and if
    USART2_ICR_ORECF
  then
  1 38 32 - lshift NVIC_ICPR1 bis!
  enable-int
;

\ Handle IO for multitasking
: task-io ( -- ) ;

\ Null interrupt handler
: null-handler ( -- )
  handle-io
;

\ Interrupt-driven IO hooks

: do-emit ( c -- )
  [: tx-full? not ;] wait
  write-tx
  USART2_CR1_TXEIE
; 

: do-key ( -- c )
  USART2_CR1_RXNEIE
  [: rx-empty? not ;] wait
  read-rx
;

: do-emit? ( -- flag )
  tx-full? not
;

: do-key? ( -- flag )
  rx-empty? not
;

\ Init
: init ( -- )
  init
  0 rx-read-index b!
  0 rx-write-index b!
  0 tx-read-index b!
  0 tx-write-index b!
  ['] null-handler null-handler-hook !
  ['] do-key key-hook !
  ['] do-emit emit-hook !
  ['] do-key? key?-hook !
  ['] do-emit? emit?-hook !
  RCC_APB1SMENR1_USART2SMEN
  1 38 32 - lshift NVIC_ISER1_SETENA
  USART2_CR1_RXNEIE
;

\ Disable interrupt-driven IO
: disable-int-io ( -- )
  disable-int
  ['] serial-key key-hook !
  ['] serial-emit emit-hook !
  ['] serial-key? key?-hook !
  ['] serial-emit? emit?-hook !
  0 null-handler-hook !
  USART2_CR1_RXNEIE_Clear
  USART2_CR1_TXEIE_Clear
  1 38 32 - lshift NVIC_ICER1_CLRENA
  RCC_APB1SMENR1_USART2SMEN_Clear
  enable-int
;

\ Reboot
reboot
