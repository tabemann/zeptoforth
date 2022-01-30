\ Copyright (c) 2013? Matthias Koch
\ Copyright (c) 2020-2022 Travis Bemann
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

\ Set up the wordlist
forth 1 set-order
forth set-current
wordlist constant int-io
wordlist constant int-io-internal
forth internal interrupt int-io
int-io-internal 5 set-order
int-io-internal set-current

\ RAM variable for rx buffer read-index
cvariable rx-read-index

\ RAM variable for rx buffer write-index
cvariable rx-write-index

\ Constant for number of bytes to buffer
128 constant rx-buffer-size

\ Rx buffer
rx-buffer-size buffer: rx-buffer

\ RAM variable for tx buffer read-index
cvariable tx-read-index

\ RAM variable for tx buffer write-index
cvariable tx-write-index

\ Constant for number of bytes to buffer
128 constant tx-buffer-size

\ Tx buffer
tx-buffer-size buffer: tx-buffer

\ USART2
$4000200 constant UART_Base
UART_Base $108 + constant UART_EVENT_RXDRDY
UART_Base $11C + constant UART_EVENT_TXDRDY
UART_Base $300 + constant UART_INTEN
UART_Base $518 + constant UART_RXD
UART_Base $51C + constant UART_TXD
: UART_INTEN_RXDRDY_Enable 1 2 lshift UART_INTEN bis! ;
: UART_INTEN_RXDRDY_Disable 1 2 lshift UART_INTEN bic! ;
: UART_INTEN_TXDRDY_Enable 1 7 lshift UART_INTEN bis! ;
: UART_INTEN_TXDRDY_Disable 1 7 lshift UART_INTEN bic! ;

\ Get whether the rx buffer is full
: rx-full? ( -- f )
  rx-write-index c@ rx-read-index c@
  rx-buffer-size 1- + rx-buffer-size umod =
;

\ Get whether the rx buffer is empty
: rx-empty? ( -- f )
  rx-read-index c@ rx-write-index c@ =
;

\ Write a byte to the rx buffer
: write-rx ( c -- )
  rx-full? not if
    rx-write-index c@ rx-buffer + c!
    rx-write-index c@ 1+ rx-buffer-size mod rx-write-index c!
  else
    drop
  then
;

\ Read a byte from the rx buffer
: read-rx ( -- c )
  rx-empty? not if
    rx-read-index c@ rx-buffer + c@
    rx-read-index c@ 1+ rx-buffer-size mod rx-read-index c!
  else
    0
  then
;

\ Get whether the tx buffer is full
: tx-full? ( -- f )
  tx-write-index c@ tx-read-index c@
  tx-buffer-size 1- + tx-buffer-size umod =
;

\ Get whether the tx buffer is empty
: tx-empty? ( -- f )
  tx-read-index c@ tx-write-index c@ =
;

\ Write a byte to the tx buffer
: write-tx ( c -- )
  tx-full? not if
    tx-write-index c@ tx-buffer + c!
    tx-write-index c@ 1+ tx-buffer-size mod tx-write-index c!
  else
    drop
  then
;

\ Read a byte from the tx buffer
: read-tx ( -- c )
  tx-empty? not if
    tx-read-index c@ tx-buffer + c@
    tx-read-index c@ 1+ tx-buffer-size mod tx-read-index c!
  else
    0
  then
;

\ Handle IO
: handle-io ( -- )
  disable-int
  begin
    rx-full? not if
      UART_EVENT_RXDRDY @ if
	0 UART_EVENT_RXDRDY !
	UART_RXD c@ write-rx false
      else
	true
      then
    else
      true
    then
  until
  rx-full? if
    UART_INTEN_RXDRDY_Disable
  then
  begin
    tx-empty? not if
      UART_EVENT_TXDRDY @ if
	0 UART_EVENT_TXDRDY !
	read-tx UART_TXD c! false
      else
	true
      then
    else
      true
    then
  until
  tx-empty? if
    UART_INTEN_TXDRDY_Disable
  then
  enable-int
;

\ Null interrupt handler
: null-handler ( -- )
  handle-io
;

\ Interrupt-driven IO hooks

: do-emit ( c -- )
  [: tx-full? not ;] wait
  write-tx
  UART_INTEN_TXDRDY_Enable
; 

: do-key ( -- c )
  UART_INTEN_RXDRDY_Enable
  [: rx-empty? not ;] wait
  read-rx
;

: do-emit? ( -- flag )
  tx-full? not
;

: do-key? ( -- flag )
  rx-empty? not
;

\ Set non-internal
int-io set-current

\ Handle IO for multitasking
: task-io ( -- ) ;

\ Enable interrupt-driven IO
: enable-int-io ( -- )
  ['] null-handler null-handler-hook !
  ['] do-key key-hook !
  ['] do-emit emit-hook !
  ['] do-key? key?-hook !
  ['] do-emit? emit?-hook !
  UART_INTEN_RXDRDY_Enable
;

\ Disable interrupt-driven IO
: disable-int-io ( -- )
  disable-int
  ['] serial-key key-hook !
  ['] serial-emit emit-hook !
  ['] serial-key? key?-hook !
  ['] serial-emit? emit?-hook !
  0 null-handler-hook !
  UART_INTEN_RXDRDY_Disable
  UART_INTEN_TXDRDY_Disable
  enable-int
;

\ Reset current wordlist
forth set-current

\ Init
: init ( -- )
  init
  0 rx-read-index c!
  0 rx-write-index c!
  0 tx-read-index c!
  0 tx-write-index c!
  enable-int-io
;

\ Reboot
reboot

