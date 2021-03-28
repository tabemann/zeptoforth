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

begin-import-module-once int-io-wordlist

  import internal-wordlist
  import interrupt-wordlist

  begin-import-module int-io-internal-wordlist

    \ RAM variable for rx buffer read-index
    bvariable rx-read-index

    \ RAM variable for rx buffer write-index
    bvariable rx-write-index

    \ Constant for number of bytes to buffer
    128 constant rx-buffer-size

    \ Rx buffer
    rx-buffer-size buffer: rx-buffer

    \ RAM variable for tx buffer read-index
    bvariable tx-read-index

    \ RAM variable for tx buffer write-index
    bvariable tx-write-index

    \ Constant for number of bytes to buffer
    128 constant tx-buffer-size

    \ Tx buffer
    tx-buffer-size buffer: tx-buffer

    \ USART2
    $40004400 constant USART2_Base
    USART2_Base $00 + constant USART2_SR
    USART2_Base $04 + constant USART2_DR
    USART2_Base $0C + constant USART2_CR1

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
	  USART2_SR @ RXNE and if
	    USART2_DR b@ write-rx false
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
	    read-tx USART2_DR b! false
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
    ;

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

    : do-flush-console ( -- )
      [: tx-empty? not ;] wait
    ;

  end-module

  \ Handle IO for multitasking
  : task-io ( -- )
  ;

  \ Enable interrupt-driven IO
  : enable-int-io ( -- )
    0 38 NVIC_IPR_IP!
    ['] null-handler null-handler-hook !
    ['] do-key key-hook !
    ['] do-emit emit-hook !
    ['] do-key? key?-hook !
    ['] do-emit? emit?-hook !
    ['] do-flush-console flush-console-hook !
    RCC_APB1LPENR_USART2LPEN
    38 NVIC_ISER_SETENA!
    USART2_CR1_RXNEIE
  ;

  \ Disable interrupt-driven IO
  : disable-int-io ( -- )
    begin-critical
    disable-int
    ['] serial-key key-hook !
    ['] serial-emit emit-hook !
    ['] serial-key? key?-hook !
    ['] serial-emit? emit?-hook !
    0 flush-console-hook !
    0 null-handler-hook !
    USART2_CR1_RXNEIE_Clear
    USART2_CR1_TXEIE_Clear
    38 NVIC_ICER_CLRENA!
    RCC_APB1LPENR_USART2LPEN_Clear
    enable-int
    end-critical
  ;

  \ Initialize interrupt-driven IO
  : init-int-io ( -- )
    0 rx-read-index b!
    0 rx-write-index b!
    0 tx-read-index b!
    0 tx-write-index b!
    enable-int-io
  ;
  
end-module

\ Init
: init ( -- )
  init
  init-int-io
;

unimport int-io-wordlist

\ Warm reboot
warm
