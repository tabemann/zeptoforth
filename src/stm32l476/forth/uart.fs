\ Copyright (c) 2013? Matthias Koch
\ Copyright (c) 2020-2025 Travis Bemann
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
  serial import
  serial-internal import
  pin import
  closure import
  
  \ Invalid UART exception
  : x-invalid-uart ( -- ) ." invalid UART" cr ;

\  \ LPUART1
\  6 constant lpuart1
  
  begin-module uart-internal

    \ Alternate UART special enabled
    variable alt-uart-special-enabled
  
    \ Console UART user variable
    user base-console-uart

    \ Saved task-init-hook
    variable saved-task-init-hook
  
    \ Validate a USART
    : validate-uart ( uart -- )
      dup 1 >= swap 5 ( lpuart ) <= and averts x-invalid-uart
    ;
    
    \ Constant for number of bytes to buffer
    128 constant uart-rx-buffer-size
      
    \ Constant for number of bytes to buffer
    128 constant uart-tx-buffer-size
      
    begin-structure uart-size
    
      \ RX buffer read-index
      cfield: uart-rx-read-index
      
      \ RX buffer write-index
      cfield: uart-rx-write-index
      
      \ RX buffer
      uart-rx-buffer-size +field uart-rx-buffer
      
      \ TX buffer read-index
      cfield: uart-tx-read-index
      
      \ TX buffer write-index
      cfield: uart-tx-write-index
      
      \ TX buffer
      uart-tx-buffer-size +field uart-tx-buffer

    end-structure

    \ UART buffers
    uart-size 4 ( 5 ) * buffer: usarts

    \ USART base register table
    create USART_Base_table
    $40013800 ,
    $40004400 ,
    $40004800 ,
    $40004C00 ,
    $40005000 ,
\    $40008000 ,
    
    \ USART base
    : USART_Base ( usart -- addr ) 1- cells USART_Base_table + @ ;
    
    \ USART registers
    : USART_CR1 ( usart -- addr ) USART_Base $00 + ;
    : USART_BRR ( usart -- addr ) USART_Base $0C + ;
    : USART_ISR ( usart -- addr ) USART_Base $1C + ;
    : USART_ICR ( usart -- addr ) USART_Base $20 + ;
    : USART_RDR ( usart -- addr ) USART_Base $24 + ;
    : USART_TDR ( usart -- addr ) USART_Base $28 + ;

    \ USART IRQ number
    : uart-irq ( usart -- irq )
      case
	1 of 37 endof
	2 of 38 endof
	3 of 39 endof
	4 of 52 endof
	5 of 53 endof
\	lpuart of 70 endof
	['] x-invalid-uart ?raise
      endcase
    ;

    \ USART vector number
    : uart-vector ( usart -- vector ) uart-irq 16 + ;

    \ Control-C
    $03 constant ctrl-c

    \ Control-T
    $14 constant ctrl-t
    
    $40021000 constant RCC_Base
    RCC_Base $58 + constant RCC_APB1ENR1
\    RCC_Base $5C + constant RCC_APB1ENR2
    RCC_Base $60 + constant RCC_APB2ENR
    RCC_Base $78 + constant RCC_APB1SMENR1
\    RCC_Base $7C + constant RCC_APB1SMENR2
    RCC_Base $80 + constant RCC_APB2SMENR
    : RCC_APB1ENR1_USART2EN 17 bit RCC_APB1ENR1 bis! ;
    : RCC_APB1ENR1_USART2EN_Clear 17 bit RCC_APB1ENR1 bic! ;
    : RCC_APB1ENR1_USART3EN 18 bit RCC_APB1ENR1 bis! ;
    : RCC_APB1ENR1_USART3EN_Clear 18 bit RCC_APB1ENR1 bic! ;
    : RCC_APB1ENR1_UART4EN 19 bit RCC_APB1ENR1 bis! ;
    : RCC_APB1ENR1_UART4EN_Clear 19 bit RCC_APB1ENR1 bic! ;
    : RCC_APB1ENR1_UART5EN 20 bit RCC_APB1ENR1 bis! ;
    : RCC_APB1ENR1_UART5EN_Clear 20 bit RCC_APB1ENR1 bic! ;
    : RCC_APB2ENR_USART1EN 14 bit RCC_APB2ENR bis! ;
    : RCC_APB2ENR_USART1EN_Clear 14 RCC_APB2ENR bic! ;
\    : RCC_APB1ENR2_LPUART1EN 0 bit RCC_APB1ENR2 bis! ;
\    : RCC_APB1ENR2_LPUART1EN_Clear 0 bit RCC_APB1ENR2 bic! ;
    : RCC_APB1SMENR1_USART2SMEN 17 bit RCC_APB1SMENR1 bis! ;
    : RCC_APB1SMENR1_USART2SMEN_Clear 17 bit RCC_APB1SMENR1 bic! ;
    : RCC_APB1SMENR1_USART3SMEN 18 bit RCC_APB1SMENR1 bis! ;
    : RCC_APB1SMENR1_USART3SMEN_Clear 18 bit RCC_APB1SMENR1 bic! ;
    : RCC_APB1SMENR1_UART4SMEN 19 bit RCC_APB1SMENR1 bis! ;
    : RCC_APB1SMENR1_UART4SMEN_Clear 19 bit RCC_APB1SMENR1 bic! ;
    : RCC_APB1SMENR1_UART5SMEN 20 bit RCC_APB1SMENR1 bis! ;
    : RCC_APB1SMENR1_UART5SMEN_Clear 20 bit RCC_APB1SMENR1 bic! ;
    : RCC_APB2SMENR_USART1SMEN 14 bit RCC_APB2SMENR bis! ;
    : RCC_APB2SMENR_USART1SMEN_Clear 14 RCC_APB2SMENR bic! ;
\    : RCC_APB1SMENR2_LPUART1SMEN 0 bit RCC_APB1SMENR2 bis! ;
\    : RCC_APB1SMENR2_LPUART1SMEN_Clear 0 bit RCC_APB1SMENR2 bic! ;
    : USART_CR1_TXEIE   %1 7 lshift swap USART_CR1 bis! ;  \ USART_CR1_TXEIE    interrupt enable
    : USART_CR1_RXNEIE   %1 5 lshift swap USART_CR1 bis! ;  \ USART_CR1_RXNEIE    RXNE interrupt enable
    : USART_CR1_TXEIE_Clear   %1 7 lshift swap USART_CR1 bic! ;  \ USART_CR1_TXEIE    interrupt disable
    : USART_CR1_RXNEIE_Clear   %1 5 lshift swap USART_CR1 bic! ;  \ USART_CR1_RXNEIE    RXNE interrupt enable
    : USART_ICR_ORECF %1 3 lshift swap USART_ICR bis! ; ( Overrun error clear flag )

    $20 constant RXNE
    $80 constant TXE
    $08 constant ORE

    \ USART enable bits
    0 bit constant USART_CR1_UE
    3 bit constant USART_CR1_TE
    2 bit constant USART_CR1_RE

    \ Select USART structure
    : uart-select ( usart -- addr )
      dup 1 = if drop usarts else 2 - uart-size * usarts + then
    ;

    \ RX buffer read-index
    : uart-rx-read-index ( usart -- addr )
      uart-select uart-rx-read-index
    ;
      
    \ RX buffer write-index
    : uart-rx-write-index ( usart -- addr )
      uart-select uart-rx-write-index
    ;

    \ RX buffer
    : uart-rx-buffer ( usart -- addr )
      uart-select uart-rx-buffer
    ;
      
    \ TX buffer read-index
    : uart-tx-read-index ( usart -- addr )
      uart-select uart-tx-read-index
    ;
      
    \ TX buffer write-index
    : uart-tx-write-index ( usart -- addr )
      uart-select uart-tx-write-index
    ;

    \ TX buffer
    : uart-tx-buffer ( usart -- addr )
      uart-select uart-tx-buffer
    ;

    \ Get whether the rx buffer is full
    : uart-rx-full? ( usart -- f )
      dup uart-rx-write-index c@ swap uart-rx-read-index c@
      uart-rx-buffer-size 1- + uart-rx-buffer-size umod =
    ;

    \ Get whether the rx buffer is empty
    : uart-rx-empty? ( usart -- f )
      dup uart-rx-read-index c@ swap uart-rx-write-index c@ =
    ;

    \ Write a byte to the rx buffer
    : uart-write-rx ( c uart -- )
      [:
	dup uart-rx-full? not if
	  tuck dup uart-rx-write-index c@ swap uart-rx-buffer + c!
	  dup uart-rx-write-index c@ 1+ uart-rx-buffer-size mod
	  swap uart-rx-write-index c!
	else
	  2drop
	then
      ;] critical
    ;

    \ Read a byte from the rx buffer
    : uart-read-rx ( usart -- c )
      [:
	dup uart-rx-empty? not if
	  dup uart-rx-read-index c@ over uart-rx-buffer + c@
	  over uart-rx-read-index c@ 1+ uart-rx-buffer-size mod
	  rot uart-rx-read-index c!
	else
	  drop 0
	then
      ;] critical
    ;

    \ Get whether the tx buffer is full
    : uart-tx-full? ( usart -- f )
      dup uart-tx-write-index c@ swap uart-tx-read-index c@
      uart-tx-buffer-size 1- + uart-tx-buffer-size umod =
    ;

    \ Get whether the tx buffer is empty
    : uart-tx-empty? ( usart -- f )
      dup uart-tx-read-index c@ swap uart-tx-write-index c@ =
    ;

    \ Write a byte to the tx buffer
    : uart-write-tx ( c uart -- )
      [:
	dup uart-tx-full? not if
	  tuck dup uart-tx-write-index c@ swap uart-tx-buffer + c!
	  dup uart-tx-write-index c@ 1+ uart-tx-buffer-size mod
	  swap uart-tx-write-index c!
	else
	  2drop
	then
      ;] critical
    ;

    \ Read a byte from the tx buffer
    : uart-read-tx ( usart -- c )
      [:
	dup uart-tx-empty? not if
	  dup uart-tx-read-index c@ over uart-tx-buffer + c@
	  over uart-tx-read-index c@ 1+ uart-tx-buffer-size mod
	  rot uart-tx-read-index c!
	else
	  drop 0
	then
      ;] critical
    ;

    \ Handle IO for an USART
    : handle-uart-io ( usart -- )
      begin
	dup uart-rx-full? not if
	  dup USART_ISR @ RXNE and if
            dup USART_RDR c@
            over bit alt-uart-special-enabled bit@ if
              dup ctrl-c = if
                drop reboot false
              else
                attention? @ if
                  attention-hook @ execute false
                else
                  dup ctrl-t = if
                    drop attention-start-hook @ execute false
                  else
                    over uart-write-rx false
                  then
                then
              then
            else
              over uart-write-rx false
            then
	  else
	    true
	  then
	else
	  true
	then
      until
      dup uart-rx-full? if
	dup USART_CR1_RXNEIE_Clear
      then
      begin
	dup uart-tx-empty? not if
	  dup USART_ISR @ TXE and if
	    dup uart-read-tx over USART_TDR c! false
	  else
	    true
	  then
	else
	  true
	then
      until
      dup uart-tx-empty? if
	dup USART_CR1_TXEIE_Clear
      then
      dup USART_ISR @ ORE and if
	dup USART_ICR_ORECF
      then
      uart-irq NVIC_ICPR_CLRPEND!
      wake
    ;

    \ USART interrupt handlers
    : handle-uart1-io ( -- ) 1 handle-uart-io ;
    : handle-uart3-io ( -- ) 3 handle-uart-io ;
    : handle-uart4-io ( -- ) 4 handle-uart-io ;
    : handle-uart5-io ( -- ) 5 handle-uart-io ;
\    : handle-lpuart1-io ( -- ) lpuart handle-uart-io ;
    
    \ Enable USART2 interrupt-driven IO
    : enable-uart1-int-io ( usart -- )
      disable-int
      0 1 uart-irq NVIC_IPR_IP!
      ['] handle-uart1-io 1 uart-vector vector!
      RCC_APB2SMENR_USART1SMEN
      1 uart-irq NVIC_ISER_SETENA!
      1 USART_CR1_RXNEIE
      enable-int
    ;

    \ Enable USART3 interrupt-driven IO
    : enable-uart3-int-io ( usart -- )
      disable-int
      0 3 uart-irq NVIC_IPR_IP!
      ['] handle-uart3-io 3 uart-vector vector!
      RCC_APB1SMENR1_USART3SMEN
      3 uart-irq NVIC_ISER_SETENA!
      3 USART_CR1_RXNEIE
      enable-int
    ;

    \ Enable UART4 interrupt-driven IO
    : enable-uart4-int-io ( usart -- )
      disable-int
      0 4 uart-irq NVIC_IPR_IP!
      ['] handle-uart4-io 4 uart-vector vector!
      RCC_APB1SMENR1_UART4SMEN
      4 uart-irq NVIC_ISER_SETENA!
      4 USART_CR1_RXNEIE
      enable-int
    ;

    \ Enable UART5 interrupt-driven IO
    : enable-uart5-int-io ( usart -- )
      disable-int
      0 5 uart-irq NVIC_IPR_IP!
      ['] handle-uart5-io 5 uart-vector vector!
      RCC_APB1SMENR1_UART5SMEN
      5 uart-irq NVIC_ISER_SETENA!
      5 USART_CR1_RXNEIE
      enable-int
    ;

\    \ Enable LPUART1 interrupt-driven IO
\    : enable-lpuart1-int-io ( usart -- )
\      disable-int
\      0 lpuart uart-irq NVIC_IPR_IP!
\      ['] handle-lpuart1-io lpuart uart-vector vector!
\      RCC_APB2SMENR_USART6SMEN
\      lpuart uart-irq NVIC_ISER_SETENA!
\      lpuart USART_CR1_RXNEIE
\      enable-int
\    ;
    
    \ Disable USART2 interrupt-driven IO
    : disable-uart1-int-io ( -- )
      disable-int
      ['] handle-null 1 uart-vector vector!
      1 USART_CR1_RXNEIE_Clear
      1 USART_CR1_TXEIE_Clear
      1 uart-irq NVIC_ICER_CLRENA!
      RCC_APB2SMENR_USART1SMEN_Clear
      enable-int
    ;

    \ Disable USART3 interrupt-driven IO
    : disable-uart3-int-io ( -- )
      disable-int
      ['] handle-null 3 uart-vector vector!
      3 USART_CR1_RXNEIE_Clear
      3 USART_CR1_TXEIE_Clear
      3 uart-irq NVIC_ICER_CLRENA!
      RCC_APB1SMENR1_USART3SMEN_Clear
      enable-int
    ;

    \ Disable UART4 interrupt-driven IO
    : disable-uart4-int-io ( -- )
      disable-int
      ['] handle-null 4 uart-vector vector!
      4 USART_CR1_RXNEIE_Clear
      4 USART_CR1_TXEIE_Clear
      4 uart-irq NVIC_ICER_CLRENA!
      RCC_APB1SMENR1_UART4SMEN_Clear
      enable-int
    ;
    
    \ Disable UART5 interrupt-driven IO
    : disable-uart5-int-io ( -- )
      disable-int
      ['] handle-null 5 uart-vector vector!
      5 USART_CR1_RXNEIE_Clear
      5 USART_CR1_TXEIE_Clear
      5 uart-irq NVIC_ICER_CLRENA!
      RCC_APB1SMENR1_UART5SMEN_Clear
      enable-int
    ;

\    \ Disable LPUART1 interrupt-driven IO
\    : disable-lpuart1-int-io ( -- )
\      disable-int
\      ['] handle-null lpuart uart-vector vector!
\      lpuart USART_CR1_RXNEIE_Clear
\      lpuart USART_CR1_TXEIE_Clear
\      lpuart uart-irq NVIC_ICER_CLRENA!
\      RCC_APB1SMENR2_LPUART1SMEN_Clear
\      enable-int
\    ;

  end-module> import

  \ UART alternate function
  : uart-alternate ( uart -- alternate )
    dup validate-uart
    case
      1 of 7 endof
      2 of 7 endof
      3 of 7 endof
      4 of 8 endof
      5 of 8 endof
\      lpuart of 8 endof
    endcase
  ;

  \ Get whether a USART is enabled
  : uart-enabled? ( usart -- flag )
    dup validate-uart
    USART_CR1_UE swap USART_CR1 bit@
  ;

  \ Enable a USART
  : enable-uart ( usart -- )
    dup validate-uart
    [ USART_CR1_UE USART_CR1_TE or USART_CR1_RE or ] literal swap USART_CR1 bis!
  ;

  \ Disable a USART
  : disable-uart ( usart -- )
    dup validate-uart
    [ USART_CR1_UE USART_CR1_TE or USART_CR1_RE or ] literal swap USART_CR1 bic!
  ;

  \ Carry out an operation with a USART disabled
  : with-uart-disabled ( xt usart -- )
    dup uart-enabled? 2dup if disable-uart else drop then
    2>r try 2r> if enable-uart else drop then ?raise
  ;

  \ Set a USART's baud
  : uart-baud! ( baud usart -- )
    [:
      swap dup 2 /
      2 pick case
	1 of 72000000 endof
	2 of 72000000 endof
	3 of 72000000 endof
	4 of 72000000 endof
	5 of 72000000 endof
\ 	lpuart of 72000000 endof
      endcase
      + swap / swap USART_BRR !
    ;] over with-uart-disabled
  ;

  continue-module uart-internal

    \ Initialize interrupt-driven IO on USART2
    : init-uart1 ( -- )
      0 1 uart-rx-read-index c!
      0 1 uart-rx-write-index c!
      0 1 uart-tx-read-index c!
      0 1 uart-tx-write-index c!
      RCC_APB2ENR_USART1EN
      RCC_APB2SMENR_USART1SMEN
      115200 1 uart-baud!
      1 enable-uart
      enable-uart1-int-io
    ;
    
    \ Initialize interrupt-driven IO on USART3
    : init-uart3 ( -- )
      0 3 uart-rx-read-index c!
      0 3 uart-rx-write-index c!
      0 3 uart-tx-read-index c!
      0 3 uart-tx-write-index c!
      RCC_APB1ENR1_USART3EN
      RCC_APB1SMENR1_USART3SMEN
      115200 3 uart-baud!
      3 enable-uart
      enable-uart3-int-io
    ;
    
    \ Initialize interrupt-driven IO on UART4
    : init-uart4 ( -- )
      0 4 uart-rx-read-index c!
      0 4 uart-rx-write-index c!
      0 4 uart-tx-read-index c!
      0 4 uart-tx-write-index c!
      RCC_APB1ENR1_UART4EN
      RCC_APB1SMENR1_UART4SMEN
      115200 4 uart-baud!
      4 enable-uart
      enable-uart4-int-io
    ;
    
    \ Initialize interrupt-driven IO on UART5
    : init-uart5 ( -- )
      0 5 uart-rx-read-index c!
      0 5 uart-rx-write-index c!
      0 5 uart-tx-read-index c!
      0 5 uart-tx-write-index c!
      RCC_APB1ENR1_UART5EN
      RCC_APB1SMENR1_UART5SMEN
      115200 5 uart-baud!
      5 enable-uart
      enable-uart5-int-io
    ;

\    \ Initialize interrupt-driven IO on LPUART1
\    : init-lpuart1 ( -- )
\      0 lpuart uart-rx-read-index c!
\      0 lpuart uart-rx-write-index c!
\      0 lpuart uart-tx-read-index c!
\      0 lpuart uart-tx-write-index c!
\      RCC_APB1ENR2_LPUART1EN
\      RCC_APB1SMENR2_LPUART1SMEN
\      115200 lpuart uart-baud!
\      lpuart enable-uart
\      enable-lpuart1-int-io
\    ;

  end-module

  \ Emit a byte to a USART
  : >uart ( c usart -- )
    dup validate-uart
    dup 2 = if
      drop do-emit
    else
      [: dup uart-tx-full? not ;] wait
      tuck uart-write-tx
      USART_CR1_TXEIE
    then
  ; 

  \ Read a byte from a USART
  : uart> ( usart -- c )
    dup validate-uart
    dup 2 = if
      drop do-key
    else
      dup USART_CR1_RXNEIE
      [: dup uart-rx-empty? not ;] wait
      uart-read-rx
    then
  ;

  \ Get whether a USART is ready to emit a byte
  : >uart? ( usart -- flag )
    dup validate-uart
    dup 2 = if
      drop do-emit?
    else
      uart-tx-full? not
    then
  ;

  \ Get whether a byte is ready to be read from a USART
  : uart>? ( usart -- flag )
    dup validate-uart
    dup 2 = if
      drop do-key?
    else
      uart-rx-empty? not
    then
  ;

  \ Flush the transmit buffer for a USART
  : flush-uart ( usart -- )
    dup validate-uart
    dup 2 = if
      drop do-flush-console
    else
      [: dup uart-tx-empty? over TXE swap USART_ISR bit@ and ;] wait drop
    then
  ;
  
  \ Enable interrupt-driven IO on a UART
  : enable-uart-int-io ( uart -- )
    dup validate-uart
    case
      1 of enable-uart1-int-io endof
      2 of enable-serial-int-io endof
      3 of enable-uart3-int-io endof
      4 of enable-uart4-int-io endof
      5 of enable-uart5-int-io endof
\      lpuart of enable-lpuart1-int-io endof
    endcase
  ;

  \ Disable interrupt-drive IO on a UART
  : disable-uart-int-io ( uart -- )
    dup validate-uart
    case
      1 of disable-uart1-int-io endof
      2 of disable-serial-int-io endof
      3 of disable-uart3-int-io endof
      4 of disable-uart4-int-io endof
      5 of disable-uart5-int-io endof
\      lpuart of disable-lpuart1-int-io endof
    endcase
  ;
  
  \ Set a pin to be a UART pin
  : uart-pin ( uart pin -- ) swap uart-alternate swap alternate-pin ;
  
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
    dup 2 = if drop uart-special-enabled ! exit then
    bit alt-uart-special-enabled rot if bis! else bic! then
  ;

  \ Get the alternate UART special enabled bitmap
  : uart-special-enabled@ ( uart -- enabled )
    dup validate-uart
    dup 2 = if drop uart-special-enabled @ exit then
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
  uart-internal::init-uart3
  uart-internal::init-uart4
  uart-internal::init-uart5
\  uart-internal::init-lpuart1
;

\ Reboot
reboot
