\ Copyright (c) 2013? Matthias Koch
\ Copyright (c) 2020-2025 Travis Bemann
\ Copyright (c) 2024 Paul Koning
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

begin-module uart

  internal import
  interrupt import
  multicore import
  serial import
  serial-internal import
  pin import
  closure import

  \ Invalid UART exception
  : x-invalid-uart ( -- ) ." invalid UART" cr ;
  
  \ Invalid CTS/RTS pin exception
  rp2350? [if]
    : x-invalid-cts/rts-pin ( -- ) ." invalid CTS/RTS pin" cr ;
  [then]

  begin-module uart-internal

    \ Alternate UART special enabled bitmap
    variable alt-uart-special-enabled

    \ Console UART user variable
    user base-console-uart

    \ Saved task-init-hook
    variable saved-task-init-hook
  
    \ Validate a UART
    : validate-uart ( uart -- ) 2 u< averts x-invalid-uart ;

    \ Validate a CTS/RTS pin
    rp2350? [if]
      : validate-cts/rts-pin ( pin -- )
        4 umod dup 2 >= swap 3 <= and averts x-invalid-cts/rts-pin
      ;
    [then]

    \ RAM variable for rx buffer read-index
    variable uart1-rx-read-index

    \ RAM variable for rx buffer write-index
    variable uart1-rx-write-index

    \ Constant for number of bytes to buffer
    256 constant uart1-rx-buffer-size

    \ Rx buffer
    uart1-rx-buffer-size buffer: uart1-rx-buffer

    \ RAM variable for tx buffer read-index
    variable uart1-tx-read-index

    \ RAM variable for tx buffer write-index
    variable uart1-tx-write-index

    \ Constant for number of bytes to buffer
    256 constant uart1-tx-buffer-size

    \ Tx buffer
    uart1-tx-buffer-size buffer: uart1-tx-buffer

    \ UART0
    UART0_Base $24 + constant UART0_UARTIBRD
    UART0_Base $28 + constant UART0_UARTFBRD
    UART0_Base $2C + constant UART0_UARTLCR_H
    UART0_Base $30 + constant UART0_UARTCR
    
    \ UART1
    rp2040? [if]
      $40038000 constant UART1_Base
    [then]
    rp2350? [if]
      $40078000 constant UART1_Base
    [then]
    UART1_Base $00 + constant UART1_UARTDR
    UART1_Base $18 + constant UART1_UARTFR
    UART1_Base $24 + constant UART1_UARTIBRD
    UART1_Base $28 + constant UART1_UARTFBRD
    UART1_Base $2C + constant UART1_UARTLCR_H
    UART1_Base $30 + constant UART1_UARTCR
    UART1_Base $34 + constant UART1_UARTIFLS
    UART1_Base $38 + constant UART1_UARTIMSC

    \ UART0 IRQ number
    rp2040? [if]
      20 constant uart0-irq
    [then]
    rp2350? [if]
      33 constant uart0-irq
    [then]
    
    \ UART1 IRQ number
    rp2040? [if]
      21 constant uart1-irq
    [then]
    rp2350? [if]
      34 constant uart1-irq
    [then]

    \ UART0 vector index
    uart0-irq 16 + constant uart0-vector

    \ UART1 vector index
    uart1-irq 16 + constant uart1-vector

    \ Control-C
    $03 constant ctrl-c

    \ Control-T
    $14 constant ctrl-t

    \ Some constants for initalization
    6 bit 5 bit or constant UART_8N1 \ Set the UART to be in 8N1 mode
    4 bit constant UART_FIFO \ Set the UART FIFO to be enabled
    9 bit 8 bit or 0 bit or constant UART_ENABLE \ Enable the UART
    
    : UART1_UARTDR_DATA! $FF and UART1_UARTDR c! ; \ Transmit data
    : UART1_UARTDR_DATA@ UART1_UARTDR c@ ; \ Receive data
    : UART1_UARTFR_TXFE@ 7 bit UART1_UARTFR bit@ ; \ Transmit FIFO empty
    : UART1_UARTFR_TXFF@ 5 bit UART1_UARTFR bit@ ; \ Transmit FIFO full
    : UART1_UARTFR_RXFE@ 4 bit UART1_UARTFR bit@ ; \ Receive FIFO empty
    : UART1_UARTFR_BUSY@ 3 bit UART1_UARTFR bit@ ; \ Busy
    : UART1_UARTIMSC_RTIM! 6 bit UART1_UARTIMSC bis! ; \ Receive timeout interrupt mask
    : UART1_UARTIMSC_TXIM! 5 bit UART1_UARTIMSC bis! ; \ Transmit interrupt mask
    : UART1_UARTIMSC_RXIM! 4 bit UART1_UARTIMSC bis! ; \ Receive interrupt mask
    : UART1_UARTIMSC_RTIM_Clear 6 bit UART1_UARTIMSC bic! ; \ Receive timeout interrupt mask
    : UART1_UARTIMSC_TXIM_Clear 5 bit UART1_UARTIMSC bic! ; \ Transmit interrupt mask
    : UART1_UARTIMSC_RXIM_Clear 4 bit UART1_UARTIMSC bic! ; \ Receive interrupt mask
    
    \ Receive interrupt FIFO level select
    : UART1_UARTIFLS_RXIFLSEL! ( rxiflsel -- )
      UART1_UARTIFLS @ $38 bic swap $7 and 3 lshift or UART1_UARTIFLS !
    ;

    \ Transmit interrupt FIFO level select
    : UART1_UARTIFLS_TXIFLSEL! ( txiflsel -- )
      UART1_UARTIFLS @ $07 bic swap $7 and or UART1_UARTIFLS !
    ;

    \ Get whether the rx buffer is full
    : uart1-rx-full? ( -- f )
      uart1-rx-write-index @ uart1-rx-read-index @
      rx-buffer-size 1- + $FF and =
    ;

    \ Get whether the rx buffer is empty
    : uart1-rx-empty? ( -- f )
      uart1-rx-read-index @ uart1-rx-write-index @ =
    ;

    \ Write a byte to the rx buffer
    : write-uart1-rx ( c -- )
      [:
	uart1-rx-full? not if
	  uart1-rx-write-index @ uart1-rx-buffer + c!
	  uart1-rx-write-index @ 1+ $FF and uart1-rx-write-index !
	else
	  drop
	then
      ;] serial-spinlock critical-with-spinlock
    ;

    \ Read a byte from the rx buffer
    : read-uart1-rx ( -- c )
      [:
	uart1-rx-empty? not if
	  uart1-rx-read-index @ uart1-rx-buffer + c@
	  uart1-rx-read-index @ 1+ $FF and uart1-rx-read-index !
	else
	  0
	then
      ;] serial-spinlock critical-with-spinlock
    ;

    \ Get whether the tx buffer is full
    : uart1-tx-full? ( -- f )
      uart1-tx-write-index @ uart1-tx-read-index @
      uart1-tx-buffer-size 1- + $FF and =
    ;

    \ Get whether the tx buffer is empty
    : uart1-tx-empty? ( -- f )
      uart1-tx-read-index @ uart1-tx-write-index @ =
    ;

    \ Write a byte to the tx buffer
    : write-uart1-tx ( c -- )
      [:
	uart1-tx-full? not if
	  uart1-tx-write-index @ uart1-tx-buffer + c!
	  uart1-tx-write-index @ 1+ $FF and uart1-tx-write-index !
	else
	  drop
	then
      ;] serial-spinlock critical-with-spinlock
    ;

    \ Read a byte from the tx buffer
    : read-uart1-tx ( -- c )
      [:
	uart1-tx-empty? not if
	  uart1-tx-read-index @ uart1-tx-buffer + c@
	  uart1-tx-read-index @ 1+ $FF and uart1-tx-read-index !
	else
	  0
	then
      ;] serial-spinlock critical-with-spinlock
    ;

    \ Handle IO
    : handle-uart1-io ( -- )
      begin
        uart1-rx-full? not if
	  UART1_UARTFR_RXFE@ not if
            UART1_UARTDR_DATA@
            [ 1 bit ] literal alt-uart-special-enabled bit@ if
              dup ctrl-c = if
                drop reboot false
              else
                attention? @ if
                  attention-hook @ execute false
                else
                  dup ctrl-t = if
                    drop attention-start-hook @ execute false
                  else
                    write-uart1-rx false
                  then
                then
              then
            else
              write-uart1-rx false
            then
	  else
	    true
	  then
	else
	  true
	then
      until
      uart1-rx-full? if
	UART1_UARTIMSC_RTIM_Clear
	UART1_UARTIMSC_RXIM_Clear
      then
      begin
	uart1-tx-empty? not if
	  UART1_UARTFR_TXFF@ not if
	    read-uart1-tx UART1_UARTDR_DATA! false
	  else
	    true
	  then
	else
	  true
	then
      until
      uart1-tx-empty? if
	UART1_UARTIMSC_TXIM_Clear
      then
      uart1-irq NVIC_ICPR_CLRPEND!
      wake
    ;
  
    \ Enable interrupt-driven IO on UART1
    : enable-uart1-int-io ( -- )
      disable-int
      0 UART1_UARTIFLS_RXIFLSEL! \ Interrupt on receive FIFO >= 1/8 full
      0 UART1_UARTIFLS_TXIFLSEL! \ Interrupt on transmit FIFO <= 1/8 full
      0 uart1-irq NVIC_IPR_IP!
      ['] handle-uart1-io uart1-vector vector!
      uart1-irq NVIC_ISER_SETENA!
      UART1_UARTIMSC_RTIM!
      UART1_UARTIMSC_RXIM!
      enable-int
    ;
    
    \ Disable interrupt-driven IO on UART1
    : disable-uart1-int-io ( -- )
      disable-int
      ['] handle-null uart1-vector vector!
      UART1_UARTIMSC_RTIM_Clear
      UART1_UARTIMSC_RXIM_Clear
      UART1_UARTIMSC_TXIM_Clear
      uart1-irq NVIC_ICER_CLRENA!
      enable-int
    ;
    
  end-module> import

  \ UART alternate function
  rp2040? [if]
    : uart-alternate ( uart -- alternate ) validate-uart 2 ;
  [then]

  \ Non-CTS/RTS UART alternate function
  rp2350? [if]
    : uart-non-cts/rts-alternate { pin uart -- alternate }
      uart validate-uart
      pin 4 umod dup 0 >= swap 1 <= and if 2 else 11 then
    ;
  [then]

  \ UART CTS/RTS alternate function
  rp2350? [if]
    : uart-cts/rts-alternate { pin uart -- alternate }
      uart validate-uart pin validate-cts/rts-pin 2
    ;
  [then]
  
  \ Carry out operations on arbitrary UART's

  \ Get whether a UART is enabled
  : uart-enabled? ( uart -- flag )
    dup validate-uart
    0 bit swap 0= if UART0_UARTCR else UART1_UARTCR then bit@
  ;
  
  \ Enable a UART
  : enable-uart ( uart -- )
    dup validate-uart
    UART_8N1 UART_FIFO or over 0= if UART0_UARTLCR_H else UART1_UARTLCR_H then !
    UART_ENABLE swap 0= if UART0_UARTCR else UART1_UARTCR then bis!
  ;

  \ Disable a UART
  : disable-uart ( uart -- )
    dup validate-uart
    UART_ENABLE swap 0= if UART0_UARTCR else UART1_UARTCR then bic!
  ;

  \ Get whether CTS is enabled on a UART
  rp2350? [if]
    : uart-cts-enabled? ( uart -- flag )
      dup validate-uart
      15 bit swap 0= if UART0_UARTCR else UART1_UARTCR then bit@
    ;
  [then]

  \ Enable CTS on a UART
  rp2350? [if]
    : enable-uart-cts ( uart -- )
      dup validate-uart
      15 bit swap 0= if UART0_UARTCR else UART1_UARTCR then bis!
    ;
  [then]

  \ Disable CTS on a UART
  rp2350? [if]
    : disable-uart-cts ( uart -- )
      dup validate-uart
      15 bit swap 0= if UART0_UARTCR else UART1_UARTCR then bic!
    ;
  [then]

  \ Get whether RTS is enabled on a UART
  rp2350? [if]
    : uart-rts-enabled? ( uart -- flag )
      dup validate-uart
      14 bit swap 0= if UART0_UARTCR else UART1_UARTCR then bit@
    ;
  [then]

  \ Enable RTS on a UART
  rp2350? [if]
    : enable-uart-rts ( uart -- )
      dup validate-uart
      14 bit swap 0= if UART0_UARTCR else UART1_UARTCR then bis!
    ;
  [then]

  \ Disable RTS on a UART
  rp2350? [if]
    : disable-uart-rts ( uart -- )
      dup validate-uart
      14 bit swap 0= if UART0_UARTCR else UART1_UARTCR then bic!
    ;
  [then]

  \ Carry out an operation with a UART disabled
  : with-uart-disabled ( xt uart -- )
    dup uart-enabled? 2dup if disable-uart else drop then
    2>r try 2r> if enable-uart else drop then ?raise
  ;

  \ Set a UART's baud
  : uart-baud! ( baud uart -- )
    [:
      >r 0 swap 16,0 f* sysclk @ s>f 2swap f/
      r@ 0= if UART0_UARTIBRD else UART1_UARTIBRD then !
      0 64,0 f* 0,5 d+ nip r> 0= if UART0_UARTFBRD else UART1_UARTFBRD then !
    ;] over with-uart-disabled
  ;

  continue-module uart-internal
    
    \ Initialize interrupt-driven IO on UART1
    : init-uart1 ( -- )
      0 uart1-rx-read-index !
      0 uart1-rx-write-index !
      0 uart1-tx-read-index !
      0 uart1-tx-write-index !
      115200 1 uart-baud!
      1 enable-uart
      enable-uart1-int-io
    ;

  end-module

  \ Emit a byte to a UART
  : >uart ( c uart -- )
    dup validate-uart
    0= if
      do-emit
    else
      UART1_UARTIMSC_TXIM!
      uart1-tx-empty? UART1_UARTFR_TXFF@ not and if
	UART1_UARTDR_DATA!
      else
	[: uart1-tx-full? not ;] wait
	write-uart1-tx
      then
    then
  ; 

  \ Read a byte from a UART
  : uart> ( uart -- c )
    dup validate-uart
    0= if
      do-key
    else
      UART1_UARTIMSC_RTIM!
      UART1_UARTIMSC_RXIM!
      [: uart1-rx-empty? not ;] wait
      read-uart1-rx
    then
  ;

  \ Get whether a UART is ready to emit a byte
  : >uart? ( uart -- flag )
    dup validate-uart
    0= if
      do-emit?
    else
      uart1-tx-full? not
    then
  ;

  \ Get whether a byte is ready to be read from a UART
  : uart>? ( uart -- flag )
    dup validate-uart
    0= if
      do-key?
    else
      uart1-rx-empty? not
    then
  ;

  \ Flush the transmit buffer for a UART
  : flush-uart ( uart -- )
    dup validate-uart
    0= if
      do-flush-console
    else
      [: uart1-tx-empty? UART1_UARTFR_TXFE@ and UART0_UARTFR_BUSY@ not and ;]
      wait
    then
  ;

  \ Enable interrupt-driven IO on a UART
  : enable-uart-int-io ( uart -- )
    dup validate-uart
    0= if enable-serial-int-io else enable-uart1-int-io then
  ;

  \ Disable interrupt-driven IO on a UART
  : disable-uart-int-io ( uart -- )
    dup validate-uart
    0= if disable-serial-int-io else disable-uart1-int-io then
  ;

  \ Set a pin to be a UART pin
  rp2040? [if]
    : uart-pin ( uart pin -- )
      swap uart-alternate swap alternate-pin
    ;
  [then]
  rp2350? [if]
    : uart-pin ( uart pin -- )
      tuck swap uart-non-cts/rts-alternate swap alternate-pin
    ;
  [then]

  \ Set a pin to be a CTS/RTS pin
  rp2350? [if]
    : uart-cts/rts-pin ( uart pin -- )
      tuck swap uart-cts/rts-alternate swap alternate-pin
    ;
  [then]

  continue-module uart-internal

    \ Data associated with UART input and output
    begin-structure console-uart-data-size
      
      \ A closure associated with KEY or EMIT
      closure-size +field console-io

      \ A closure associated with KEY? or EMIT?
      closure-size +field console-io?
      
      \ The uart associated with the input or output
      field: console-uart

    end-structure

    \ Data associated with UART output only
    begin-structure console-out-uart-data-size

      console-uart-data-size +field console-initial-part

      \ Flush console
      closure-size +field console-io-flush
      
    end-structure

    \ Initialize console UART data for input
    : init-console-uart-input { uart data -- }
      uart data console-uart !
      data data console-io [: { data }
        data console-uart @ uart>
      ;] bind
      data data console-io? [: { data }
        data console-uart @ uart>?
      ;] bind
    ;

    \ Initialize console UART data for output
    : init-console-uart-output { uart data -- }
      uart data console-uart !
      data data console-io [: { byte data }
        byte data console-uart @ >uart
      ;] bind
      data data console-io? [: { data }
        data console-uart @ >uart?
      ;] bind
      data data console-io-flush [: { data }
        data console-uart @ flush-uart
      ;] bind
    ;
    
  end-module

  \ Set the current input to a UART within an xt
  : with-uart-input ( uart xt -- )
    over validate-uart
    console-uart-data-size [: { data }
      swap data init-console-uart-input
      data console-io data console-io? rot console::with-input
    ;] with-aligned-allot
  ;

  \ Set the current output to a UART within an xt
  : with-uart-output ( uart xt -- )
    over validate-uart
    console-out-uart-data-size [: { data }
      swap data init-console-uart-output
      data console-io data console-io? rot data console-io-flush swap
      console::with-output
    ;] with-aligned-allot
  ;

  \ Set the current error output to a UART within an xt
  : with-uart-error-output ( uart xt -- )
    over validate-uart
    console-out-uart-data-size [: { data }
      swap data init-console-uart-output
      data console-io data console-io? rot data console-io-flush swap
      console::with-error-output
    ;] with-aligned-allot
  ;

  \ Set the console to a UART
  : uart-console ( uart -- )
    dup validate-uart
    base-console-uart !
    [: base-console-uart @ uart> ;] key-hook !
    [: base-console-uart @ uart>? ;] key?-hook !
    [: base-console-uart @ >uart ;] dup emit-hook ! error-emit-hook !
    [: base-console-uart @ >uart? ;] dup emit?-hook ! error-emit?-hook !
    [: base-console-uart @ flush-uart ;]
    dup flush-console-hook ! error-flush-console-hook !
  ;
  
  \ Set the alternate UART special enabled bitmap
  : uart-special-enabled! ( enabled uart -- )
    dup validate-uart
    dup 0= if drop uart-special-enabled ! exit then
    bit alt-uart-special-enabled rot if bis! else bic! then
  ;

  \ Get the alternate UART special enabled bitmap
  : uart-special-enabled@ ( uart -- enabled )
    dup validate-uart
    dup 0= if drop uart-special-enabled @ exit then
    bit alt-uart-special-enabled bit@
  ;
  
end-module> import

\ Init
: init ( -- )
  init
  0 uart-internal::alt-uart-special-enabled !
  -1 uart-internal::base-console-uart !
  task::task-init-hook @ uart-internal::saved-task-init-hook !
  [:
    uart-internal::base-console-uart @
    over ['] uart-internal::base-console-uart task::for-task!
    uart-internal::saved-task-init-hook @ ?dup if execute else drop then
  ;] task::task-init-hook !
  uart-internal::init-uart1
;

\ Reboot
reboot
