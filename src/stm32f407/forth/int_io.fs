\ Copyright (c) 2013? Matthias Koch
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

begin-import-module-once int-io-module

  import internal-module
  import interrupt-module

  begin-import-module int-io-internal-module

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
    $40004400 constant USART2_Base
    USART2_Base $00 + constant USART2_SR
    USART2_Base $04 + constant USART2_DR
    USART2_Base $0C + constant USART2_CR1

    \ USART2 vector index
    54 constant usart2-vector

    $40023800 constant RCC_Base
    RCC_Base $60 + constant RCC_APB1LPENR ( RCC_APB1LPENR )
    : RCC_APB1LPENR_USART2LPEN   %1 17 lshift RCC_APB1LPENR bis! ;  \ RCC_APB1LPENR_USART2LPEN    USART2 clocks enable during Sleep modes
    : RCC_APB1LPENR_USART2LPEN_Clear   %1 17 lshift RCC_APB1LPENR bic! ;  \ RCC_APB1LPENR_USART2LPEN    USART2 clocks enable during Sleep modes
    : USART2_CR1_TXEIE   %1 7 lshift USART2_CR1 bis! ;  \ USART2_CR1_TXEIE    interrupt enable
    : USART2_CR1_RXNEIE   %1 5 lshift USART2_CR1 bis! ;  \ USART2_CR1_RXNEIE    RXNE interrupt enable
    : USART2_CR1_TXEIE_Clear   %1 7 lshift USART2_CR1 bic! ;  \ USART2_CR1_TXEIE    interrupt disable
    : USART2_CR1_RXNEIE_Clear   %1 5 lshift USART2_CR1 bic! ;  \ USART2_CR1_RXNEIE    RXNE interrupt enable

    $20 constant RXNE
    $80 constant TXE

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
      dmb dsb isb
      disable-int
      begin
	rx-full? not if
	  USART2_SR @ RXNE and if
	    USART2_DR c@ write-rx false
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
	  USART2_SR @ TXE and if
	    read-tx USART2_DR c! false
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
      38 NVIC_ICPR_CLRPEND!
      enable-int
      wake
      dmb dsb isb
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

    : do-flush-console ( -- )
      [: tx-empty? not ;] wait
    ;

  end-module

  \ Handle IO for multitasking
  : task-io ( -- )
  ;

  \ Set up the serial console
  : serial-console ( -- )
    ['] do-key key-hook !
    ['] do-emit emit-hook !
    ['] do-key? key?-hook !
    ['] do-emit? emit?-hook !
    ['] do-flush-console flush-console-hook !
  ;
  
  \ Enable interrupt-driven IO
  : enable-int-io ( -- )
    disable-int
    0 38 NVIC_IPR_IP!
    ['] handle-io usart2-vector vector!
    serial-console
    RCC_APB1LPENR_USART2LPEN
    38 NVIC_ISER_SETENA!
    USART2_CR1_RXNEIE
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
    ['] handle-null usart2-vector vector!
    USART2_CR1_RXNEIE_Clear
    USART2_CR1_TXEIE_Clear
    38 NVIC_ICER_CLRENA!
    RCC_APB1LPENR_USART2LPEN_Clear
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
  
end-module

\ Warm
: warm ( -- )
  disable-int-io
  warm
;

\ Init
: init ( -- )
  init
  init-int-io
;

unimport int-io-module

\ Warm reboot
warm
