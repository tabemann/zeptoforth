\ Copyright (c) 2013? Matthias Koch
\ Copyright (c) 2020-2023 Travis Bemann
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

begin-module int-io

  internal import
  interrupt import
  multicore import

  begin-module int-io-internal

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

    \ UART0
    $40034000 constant UART0_Base
    UART0_Base $00 + constant UART0_UARTDR
    UART0_Base $18 + constant UART0_UARTFR
    UART0_Base $34 + constant UART0_UARTIFLS
    UART0_Base $38 + constant UART0_UARTIMSC

    \ UART0 IRQ number
    20 constant uart0-irq

    \ UART0 vector index
    uart0-irq 16 + constant uart0-vector

    \ Control-C
    $03 constant ctrl-c

    \ Control-T
    $14 constant ctrl-t
    
    : UART0_UARTDR_DATA! $FF and UART0_UARTDR c! ; \ Transmit data
    : UART0_UARTDR_DATA@ UART0_UARTDR c@ ; \ Receive data
    : UART0_UARTFR_TXFE@ 7 bit UART0_UARTFR bit@ ; \ Transmit FIFO empty
    : UART0_UARTFR_TXFF@ 5 bit UART0_UARTFR bit@ ; \ Transmit FIFO full
    : UART0_UARTFR_RXFE@ 4 bit UART0_UARTFR bit@ ; \ Receive FIFO empty
    : UART0_UARTIMSC_RTIM! 6 bit UART0_UARTIMSC bis! ; \ Receive timeout interrupt mask
    : UART0_UARTIMSC_TXIM! 5 bit UART0_UARTIMSC bis! ; \ Transmit interrupt mask
    : UART0_UARTIMSC_RXIM! 4 bit UART0_UARTIMSC bis! ; \ Receive interrupt mask
    : UART0_UARTIMSC_RTIM_Clear 6 bit UART0_UARTIMSC bic! ; \ Receive timeout interrupt mask
    : UART0_UARTIMSC_TXIM_Clear 5 bit UART0_UARTIMSC bic! ; \ Transmit interrupt mask
    : UART0_UARTIMSC_RXIM_Clear 4 bit UART0_UARTIMSC bic! ; \ Receive interrupt mask
    
    \ Receive interrupt FIFO level select
    : UART0_UARTIFLS_RXIFLSEL! ( rxiflsel -- )
      UART0_UARTIFLS @ $38 bic swap $7 and 3 lshift or UART0_UARTIFLS !
    ;

    \ Transmit interrupt FIFO level select
    : UART0_UARTIFLS_TXIFLSEL! ( txiflsel -- )
      UART0_UARTIFLS @ $07 bic swap $7 and or UART0_UARTIFLS !
    ;

    \ Get whether the rx buffer is full
    : rx-full? ( -- f )
      rx-write-index c@ rx-read-index c@
      rx-buffer-size 1- + $7F and =
    ;

    \ Get whether the rx buffer is empty
    : rx-empty? ( -- f )
      rx-read-index c@ rx-write-index c@ =
    ;

    \ Write a byte to the rx buffer
    : write-rx ( c -- )
      [:
	rx-full? not if
	  rx-write-index c@ rx-buffer + c!
	  rx-write-index c@ 1+ $7F and rx-write-index c!
	else
	  drop
	then
      ;] serial-spinlock critical-with-spinlock
    ;

    \ Read a byte from the rx buffer
    : read-rx ( -- c )
      [:
	rx-empty? not if
	  rx-read-index c@ rx-buffer + c@
	  rx-read-index c@ 1+ $7F and rx-read-index c!
	else
	  0
	then
      ;] serial-spinlock critical-with-spinlock
    ;

    \ Get whether the tx buffer is full
    : tx-full? ( -- f )
      tx-write-index c@ tx-read-index c@
      tx-buffer-size 1- + $7F and =
    ;

    \ Get whether the tx buffer is empty
    : tx-empty? ( -- f )
      tx-read-index c@ tx-write-index c@ =
    ;

    \ Write a byte to the tx buffer
    : write-tx ( c -- )
      [:
	tx-full? not if
	  tx-write-index c@ tx-buffer + c!
	  tx-write-index c@ 1+ $7F and tx-write-index c!
	else
	  drop
	then
      ;] serial-spinlock critical-with-spinlock
    ;

    \ Read a byte from the tx buffer
    : read-tx ( -- c )
      [:
	tx-empty? not if
	  tx-read-index c@ tx-buffer + c@
	  tx-read-index c@ 1+ $7F and tx-read-index c!
	else
	  0
	then
      ;] serial-spinlock critical-with-spinlock
    ;

    \ Handle IO
    : handle-io ( -- )
      begin
	rx-full? not if
	  UART0_UARTFR_RXFE@ not if
            UART0_UARTDR_DATA@ dup ctrl-c = if
              drop reboot false
            else
              attention? @ if
                attention-hook @ execute false
              else
                dup ctrl-t = if
                  drop attention-start-hook @ execute false
                else
                  write-rx false
                then
              then
            then
	  else
	    true
	  then
	else
	  true
	then
      until
      rx-full? if
	UART0_UARTIMSC_RTIM_Clear
	UART0_UARTIMSC_RXIM_Clear
      then
      begin
	tx-empty? not if
	  UART0_UARTFR_TXFF@ not if
	    read-tx UART0_UARTDR_DATA! false
	  else
	    true
	  then
	else
	  true
	then
      until
      tx-empty? if
	UART0_UARTIMSC_TXIM_Clear
      then
      uart0-irq NVIC_ICPR_CLRPEND!
      wake
      dmb dsb isb
    ;

    \ Interrupt-driven IO hooks

    : do-emit ( c -- )
      UART0_UARTIMSC_TXIM!
      tx-empty? UART0_UARTFR_TXFF@ not and if
	UART0_UARTDR_DATA!
      else
	[: tx-full? not ;] wait
	write-tx
      then
    ; 

    : do-key ( -- c )
      UART0_UARTIMSC_RTIM!
      UART0_UARTIMSC_RXIM!
      [: rx-empty? not ;] wait
      read-rx
    ;

    : do-emit? ( -- flag )
      tx-full? not
    ;

    : do-key? ( -- flag )
      rx-empty? not
    ;

    : do-flush-console ( -- )
      [: tx-empty? UART0_UARTFR_TXFE@ and ;] wait
    ;

  end-module> import

  \ Handle IO for multitasking
  : task-io ( -- )
  ;

  \ Set up the serial console
  : serial-console ( -- )
    ['] do-key key-hook !
    ['] do-emit emit-hook !
    ['] do-emit error-emit-hook !
    ['] do-key? key?-hook !
    ['] do-emit? emit?-hook !
    ['] do-emit? error-emit?-hook
    ['] do-flush-console flush-console-hook !
  ;
  
  \ Enable interrupt-driven IO
  : enable-int-io ( -- )
    disable-int
    0 UART0_UARTIFLS_RXIFLSEL! \ Interrupt on receive FIFO >= 1/8 full
    0 UART0_UARTIFLS_TXIFLSEL! \ Interrupt on transmit FIFO <= 1/8 full
    0 uart0-irq NVIC_IPR_IP!
    ['] handle-io uart0-vector vector!
    serial-console
    uart0-irq NVIC_ISER_SETENA!
    UART0_UARTIMSC_RTIM!
    UART0_UARTIMSC_RXIM!
    enable-int
  ;

  \ Disable interrupt-driven IO
  : disable-int-io ( -- )
    disable-int
    ['] serial-key key-hook !
    ['] serial-emit emit-hook !
    ['] serial-key? key?-hook !
    ['] serial-emit? emit?-hook !
    0 flush-console-hook !
    ['] handle-null uart0-vector vector!
    UART0_UARTIMSC_RTIM_Clear
    UART0_UARTIMSC_RXIM_Clear
    UART0_UARTIMSC_TXIM_Clear
    uart0-irq NVIC_ICER_CLRENA!
    enable-int
  ;

  \ Initialize interrupt-driven IO
  : init-int-io ( -- )
    0 rx-read-index c!
    0 rx-write-index c!
    0 tx-read-index c!
    0 tx-write-index c!
    enable-int-io
  ;
  
end-module> import

\ Init
: init ( -- )
  init
  init-int-io
;

\ Reboot
reboot
