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

    \ USART3
    $40004800 constant USART3_Base
    USART3_Base $00 + constant USART3_CR1
    USART3_Base $1C + constant USART3_ISR
    USART3_Base $20 + constant USART3_ICR ( Interrupt flag clear register ) 
    USART3_Base $24 + constant USART3_RDR
    USART3_Base $28 + constant USART3_TDR

    \ USART3 IRQ number
    39 constant usart3-irq
    
    \ USART3 vector index
    usart3-irq 16 + constant usart3-vector

    \ Control-C
    $03 constant ctrl-c

    \ Control-T
    $14 constant ctrl-t

    $58024400 constant RCC_Base
    RCC_Base $64 + constant RCC_APB2LPENR ( RCC_APB2LPENR )
    : USART3_CR1_TXFNFIE   %1 7 lshift USART3_CR1 bis! ;  \ USART3_CR1_TXFNEIE    interrupt enable
    : USART3_CR1_RXFNEIE   %1 5 lshift USART3_CR1 bis! ;  \ USART3_CR1_RXNEIE    RXNE interrupt enable
    : USART3_CR1_TXFNFIE_Clear   %1 7 lshift USART3_CR1 bic! ;  \ USART3_CR1_TXFNEIE    interrupt disable
    : USART3_CR1_RXFNEIE_Clear   %1 5 lshift USART3_CR1 bic! ;  \ USART3_CR1_RXNEIE    RXNE interrupt enable
    : USART3_ICR_ORECF %1 3 lshift USART3_ICR bis! ; ( Overrun error clear flag )  

    %1 5 lshift constant RXFNE
    %1 7 lshift constant TXFNE
    %1 3 lshift constant ORE

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
      [:
	rx-full? not if
	  rx-write-index c@ rx-buffer + c!
	  rx-write-index c@ 1+ rx-buffer-size mod rx-write-index c!
	else
	  drop
	then
      ;] critical
    ;

    \ Read a byte from the rx buffer
    : read-rx ( -- c )
      [:
	rx-empty? not if
	  rx-read-index c@ rx-buffer + c@
	  rx-read-index c@ 1+ rx-buffer-size mod rx-read-index c!
	else
	  0
	then
      ;] critical
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
      [:
	tx-full? not if
	  tx-write-index c@ tx-buffer + c!
	  tx-write-index c@ 1+ tx-buffer-size mod tx-write-index c!
	else
	  drop
	then
      ;] critical
    ;

    \ Read a byte from the tx buffer
    : read-tx ( -- c )
      [:
	tx-empty? not if
	  tx-read-index c@ tx-buffer + c@
	  tx-read-index c@ 1+ tx-buffer-size mod tx-read-index c!
	else
	  0
	then
      ;] critical
    ;

    \ Handle IO
    : handle-io ( -- )
      begin
	rx-full? not if
	  USART3_ISR @ RXFNE and if
            USART3_RDR c@
            uart-special-enabled @ if
              dup ctrl-c = if
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
              write-rx false
            then
	  else
	    true
	  then
	else
	  true
	then
      until
      rx-full? if
	USART3_CR1_RXFNEIE_Clear
      then
      begin
	tx-empty? not if
	  USART3_ISR @ TXFNE and if
	    read-tx USART3_TDR c! false
	  else
	    true
	  then
	else
	  true
	then
      until
      tx-empty? if
	USART3_CR1_TXFNEIE_Clear
      then
      USART3_ISR @ ORE and if
	USART3_ICR_ORECF
      then
      usart3-irq NVIC_ICPR_CLRPEND!
      wake
    ;

    \ Interrupt-driven IO hooks

    : do-emit ( c -- )
      [: tx-full? not ;] wait
      write-tx
      USART3_CR1_TXFNEIE
    ; 

    : do-key ( -- c )
      USART3_CR1_RXFNEIE
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
      [: tx-empty? TXFNE USART3_ISR bit@ and ;] wait
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
    ['] do-emit? error-emit?-hook !
    ['] do-flush-console flush-console-hook !
    ['] do-flush-console error-flush-console-hook !
  ;
  
  \ Enable interrupt-driven IO
  : enable-int-io ( -- )
    disable-int
    0 usart3-irq NVIC_IPR_IP!
    ['] handle-io usart3-vector vector!
    serial-console
    RCC_APB2LPENR_USART3LPEN
    usart3-irq NVIC_ISER_SETENA!
    USART3_CR1_RXFNEIE
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
    ['] handle-null usart3-vector vector!
    USART3_CR1_RXFNEIE_Clear
    USART3_CR1_TXFNEIE_Clear
    usart3-irq NVIC_ICER_CLRENA!
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
