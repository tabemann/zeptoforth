\ Copyright (c) 2022 Travis Bemann
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

\ compile-to-flash

begin-module spi

  interrupt import
  multicore import
  pin import
  
  \ Invalid SPI periperal exception
  : x-invalid-spi ( -- ) ." invalid SPI" cr ;

  \ Invalid SPI clock setting
  : x-invalid-spi-clock ( -- ) ." invalid SPI clock" cr ;

  \ Invalid SPI data size setting
  : x-invalid-spi-data-size ( -- ) ." invalid SPI data size" cr ;

  begin-module spi-internal

    \ RX buffer size in bytes (not halfwords)
    128 constant spi-rx-buffer-size
    
    \ TX buffer size in bytes (not halfwords)
    128 constant spi-tx-buffer-size

    \ SPI peripheral count
    2 constant spi-count
    
    \ Validate an SPI peripheral
    : validate-spi ( spi -- ) spi-count u< averts x-invalid-spi ;

    begin-structure spi-size

      \ RX buffer read-index
      cfield: spi-rx-read-index
      
      \ RX buffer write-index
      cfield: spi-rx-write-index
      
      \ RX buffer
      spi-rx-buffer-size +field spi-rx-buffer
      
      \ TX buffer read-index
      cfield: spi-tx-read-index
      
      \ TX buffer write-index
      cfield: spi-tx-write-index
      
      \ TX buffer
      spi-tx-buffer-size +field spi-tx-buffer

      \ Receiver handler
      field: spi-rx-handler

    end-structure

    spi-size spi-count * buffer: spi-buffers

    \ SPI base address
    : SPI_Base ( spi -- addr ) $4000 * $4003C000 + ;

    \ SPI registers
    : SPI_SSPCR0 ( spi -- addr ) SPI_Base $000 + ;
    : SPI_SSPCR1 ( spi -- addr ) SPI_Base $004 + ;
    : SPI_SSPDR ( spi -- addr ) SPI_Base $008 + ;
    : SPI_SSPSR ( spi -- addr ) SPI_Base $00C + ;
    : SPI_SSPCPSR ( spi -- addr ) SPI_Base $010 + ;
    : SPI_SSPIMSC ( spi -- addr ) SPI_Base $014 + ;
    : SPI_SSPRIS ( spi -- addr ) SPI_Base $018 + ;
    : SPI_SSPMIS ( spi -- addr ) SPI_Base $01C + ;
    : SPI_SSPICR ( spi -- addr ) SPI_Base $020 + ;
    : SPI_SSPDMACR ( spi -- addr ) SPI_Base $024 + ;

    \ SPI fields
    : SPI_SSPCR0_SCR! ( scr spi -- )
      SPI_SSPCR0 dup >r @ $FF00 bic swap $FF and 8 lshift or r> !
    ;
    : SPI_SSPCR0_SPH! ( sph spi -- )
      7 bit swap SPI_SSPCR0 rot if bis! else bic! then
    ;
    : SPI_SSPCR0_SPO! ( spo spi -- )
      6 bit swap SPI_SSPCR0 rot if bis! else bic! then
    ;
    : SPI_SSPCR0_FRF! ( frf spi -- )
      SPI_SSPCR0 dup >r @ $30 bic swap $3 and 4 lshift or r> !
    ;
    : SPI_SSPCR0_DSS! ( dss spi -- )
      SPI_SSPCR0 dup >r @ $F bic swap $F and or r> !
    ;
    : SPI_SSPCR1_SOD! ( sod spi -- )
      3 bit swap SPI_SSPCR1 rot if bis! else bic! then
    ;
    : SPI_SSPCR1_MS! ( ms spi -- )
      2 bit swap SPI_SSPCR1 rot if bis! else bic! then
    ;
    : SPI_SSPCR1_SSE! ( sse spi -- )
      1 bit swap SPI_SSPCR1 rot if bis! else bic! then
    ;
    : SPI_SSPCR1_LBM! ( lbm spi -- )
      0 bit swap SPI_SSPCR1 rot if bis! else bic! then
    ;
    : SPI_SSPSR_BSY@ ( spi -- bsy ) 4 bit swap SPI_SSPSR bit@ ;
    : SPI_SSPSR_RFF@ ( spi -- rff ) 3 bit swap SPI_SSPSR bit@ ;
    : SPI_SSPSR_RNE@ ( spi -- rne ) 2 bit swap SPI_SSPSR bit@ ;
    : SPI_SSPSR_TNF@ ( spi -- tnf ) 1 bit swap SPI_SSPSR bit@ ;
    : SPI_SSPSR_TFE@ ( spi -- tfe ) 0 bit swap SPI_SSPSR bit@ ;
    : SPI_SSPIMSC_TXIM! ( txim spi -- )
      3 bit swap SPI_SSPIMSC rot if bis! else bic! then
    ;
    : SPI_SSPIMSC_RXIM! ( rxim spi -- )
      2 bit swap SPI_SSPIMSC rot if bis! else bic! then
    ;
    : SPI_SSPIMSC_RTIM! ( rtim spi -- )
      1 bit swap SPI_SSPIMSC rot if bis! else bic! then
    ;
    : SPI_SSPIMSC_RORIM! ( rorim spi -- )
      0 bit swap SPI_SSPIMSC rot if bis! else bic! then
    ;
    : SPI_SSPMIS_TXMIS@ ( spi -- txmis ) 3 bit swap SPI_SSPMIS bit@ ;
    : SPI_SSPMIS_RXMIS@ ( spi -- rxmis ) 2 bit swap SPI_SSPMIS bit@ ;
    : SPI_SSPMIS_RTMIS@ ( spi -- rtmis ) 1 bit swap SPI_SSPMIS bit@ ;
    : SPI_SSPMIS_RORMIS@ ( spi -- rormis ) 0 bit swap SPI_SSPMIS bit@ ;
    : SPI_SSPICR_RTIC! ( rtic spi -- )
      1 bit swap SPI_SSPICR rot if bis! else bic! then
    ;
    : SPI_SSPICR_RORIC! ( roric spi -- )
      0 bit swap SPI_SSPICR rot if bis! else bic! then
    ;

    \ SPI IRQ
    : spi-irq ( spi -- irq ) 18 + ;

    \ SPI vector
    : spi-vector ( spi -- vector ) spi-irq 16 + ;
    
    \ Select SPI structure
    : spi-select ( spi -- addr ) spi-size * spi-buffers + ;

    \ RX buffer read-index
    : spi-rx-read-index ( spi -- addr ) spi-select spi-rx-read-index ;
      
    \ RX buffer write-index
    : spi-rx-write-index ( spi -- addr ) spi-select spi-rx-write-index ;

    \ RX buffer
    : spi-rx-buffer ( spi -- addr ) spi-select spi-rx-buffer ;
      
    \ TX buffer read-index
    : spi-tx-read-index ( spi -- addr ) spi-select spi-tx-read-index ;
      
    \ TX buffer write-index
    : spi-tx-write-index ( spi -- addr ) spi-select spi-tx-write-index ;

    \ TX buffer
    : spi-tx-buffer ( spi -- addr ) spi-select spi-tx-buffer ;

    \ SPI RX handler
    : spi-rx-handler ( spi -- addr ) spi-select spi-rx-handler ;

    \ Get whether the rx buffer is full
    : spi-rx-full? ( spi -- f )
      dup spi-rx-write-index c@ swap spi-rx-read-index c@
      spi-rx-buffer-size 1- + spi-rx-buffer-size umod =
    ;

    \ Get whether the rx buffer is empty
    : spi-rx-empty? ( spi -- f )
      dup spi-rx-read-index c@ swap spi-rx-write-index c@ =
    ;

    \ Write a halfword to the rx buffer
    : spi-write-rx ( h spi -- )
      dup spi-rx-full? not if
	tuck dup spi-rx-write-index c@ swap spi-rx-buffer + h!
	dup spi-rx-write-index c@ 2 + spi-rx-buffer-size mod
	swap spi-rx-write-index c!
      else
	2drop
      then
    ;

    \ Read a halfword from the rx buffer
    : spi-read-rx ( spi -- h )
      dup spi-rx-empty? not if
	dup spi-rx-read-index c@ over spi-rx-buffer + h@
	over spi-rx-read-index c@ 2 + spi-rx-buffer-size mod
	rot spi-rx-read-index c!
      else
	drop 0
      then
    ;

    \ Get whether the tx buffer is full
    : spi-tx-full? ( spi -- f )
      dup spi-tx-write-index c@ swap spi-tx-read-index c@
      spi-tx-buffer-size 1- + spi-tx-buffer-size umod =
    ;

    \ Get whether the tx buffer is empty
    : spi-tx-empty? ( spi -- f )
      dup spi-tx-read-index c@ swap spi-tx-write-index c@ =
    ;

    \ Write a halfword to the tx buffer
    : spi-write-tx ( h spi -- )
      dup spi-tx-full? not if
	tuck dup spi-tx-write-index c@ swap spi-tx-buffer + h!
	dup spi-tx-write-index c@ 2 + spi-tx-buffer-size mod
	swap spi-tx-write-index c!
      else
	2drop
      then
    ;

    \ Read a halfword from the tx buffer
    : spi-read-tx ( spi -- h )
      dup spi-tx-empty? not if
	dup spi-tx-read-index c@ over spi-tx-buffer + h@
	over spi-tx-read-index c@ 2 + spi-tx-buffer-size mod
	rot spi-tx-read-index c!
      else
	drop 0
      then
    ;

    \ Handle an SPI interrupt
    : handle-spi ( spi -- )
      [:
	begin
	  dup spi-tx-empty? not if
	    dup SPI_SSPSR_TNF@ if
	      dup spi-read-tx over SPI_SSPDR h! false
	    else
	      true
	    then
	  else
	    true
	  then
	until
	false
	begin
	  over spi-rx-full? not if
	    over SPI_SSPSR_RNE@ if
	      over SPI_SSPDR h@ 2 pick spi-write-rx drop true false
	    else
	      true
	    then
	  else
	    true
	  then
	until
	true 2 pick SPI_SSPICR_RTIC!
	true 2 pick SPI_SSPICR_RORIC!
      ;] serial-spinlock critical-with-spinlock
      if dup spi-rx-handler @ ?execute then
      dup spi-tx-empty? not swap SPI_SSPIMSC_TXIM!
    ;

    \ Handle an SPI interrupt for SPI0
    : handle-spi0 0 handle-spi ;

    \ Handle an SPI interrupt for SPI1
    : handle-spi1 1 handle-spi ;

    \ Initialize an SPI entity
    : init-spi ( spi -- )
      0 over spi-rx-read-index c!
      0 over spi-rx-write-index c!
      0 over spi-tx-read-index c!
      0 over spi-tx-write-index c!
      0 over spi-irq NVIC_IPR_IP!
      0 over spi-rx-handler !
      dup if ['] handle-spi1 else ['] handle-spi0 then over spi-vector vector!
      true swap SPI_SSPIMSC_RXIM!
    ;

  end-module> import

  \ Set the SPI serial bit rate
  : spi-bit-rate! ( cpsdvsr scr spi -- )
    dup validate-spi
    over 256 u< averts x-invalid-spi-clock
    2 pick 255 u< averts x-invalid-spi-clock
    2 pick 0 u> averts x-invalid-spi-clock
    2 pick 1 and 0= averts x-invalid-spi-clock
    tuck SPI_SSPCR0_SCR!
    SPI_SSPCPSR !
  ;

  \ Set SPI to master
  : master-spi ( spi -- ) dup validate-spi false swap SPI_SSPCR1_MS! ;

  \ Set SPI to salve
  : slave-spi ( spi -- ) dup validate-spi true swap SPI_SSPCR1_MS! ;

  \ Set SPI to Motorola SPI frame format with SPH and SPO settings
  : motorola-spi ( sph spo spi -- )
    dup validate-spi
    tuck SPI_SSPCR0_SPO!
    tuck SPI_SSPCR0_SPH!
    0 swap SPI_SSPCR0_FRF!
  ;

  \ Set SPI to TI synchronous serial frame format
  : ti-ss-spi ( spi -- ) dup validate-spi 1 swap SPI_SSPCR0_FRF! ;

  \ Set SPI to National Microwire frame format
  : natl-microwire-spi ( spi -- ) dup validate-spi 2 swap SPI_SSPCR0_FRF! ;

  \ Set SPI data size
  : spi-data-size! ( data-size spi -- )
    dup validate-spi swap
    dup 4 u>= averts x-invalid-spi-data-size
    dup 16 u<= averts x-invalid-spi-data-size
    1- swap SPI_SSPCR0_DSS!
  ;
    
  \ Enable SPI
  : enable-spi ( spi -- ) dup validate-spi true swap SPI_SSPCR1_SSE! ;

  \ Disable SPI
  : disable-spi ( spi -- ) dup validate-spi false swap SPI_SSPCR1_SSE! ;

  \ Enable SPI TX
  : enable-spi-tx ( spi -- ) dup validate-spi false swap SPI_SSPCR1_SOD! ;

  \ Disable SPI TX for slaves
  : disable-spi-tx ( spi -- ) dup validate-spi true swap SPI_SSPCR1_SOD! ;

  \ Enable loopback
  : enable-spi-loopback ( spi -- ) dup validate-spi true swap SPI_SSPCR1_LBM! ;

  \ Disable loopback
  : disable-spi-loopback ( spi -- )
    dup validate-spi false swap SPI_SSPCR1_LBM!
  ;

  \ Set the SPI RX handler; note that this handler executes in an interrupt
  \ handler
  : spi-rx-handler! ( xt spi -- ) dup validate-spi spi-rx-handler ! ;

  \ SPI alternate function
  : spi-alternate ( spi -- alternate ) validate-spi 1 ;

  \ Set a pin to be an SPI pin
  : spi-pin ( spi pin -- )
    swap spi-alternate swap alternate-pin
  ;

  \ Write a halfword to SPI
  : >spi ( h spi -- )
    dup validate-spi
    [:
      dup spi-tx-empty? if
	dup SPI_SSPSR_RFF@ if
	  dup spi-tx-full? not if
	    tuck spi-write-tx
	    true swap SPI_SSPIMSC_TXIM!
	  then
	else
	  SPI_SSPDR h!
	then
      else
	dup spi-tx-full? not if
	  tuck spi-write-tx
	  true swap SPI_SSPIMSC_TXIM!
	then
      then
    ;] serial-spinlock critical-with-spinlock
  ;

  \ Read a halfword from SPI
  : spi> ( spi -- h )
    dup validate-spi
    begin
      [:
	disable-int
	dup spi-rx-empty? if
	  dup SPI_SSPSR_RNE@ if
	    SPI_SSPDR h@ enable-int false true
	  else
	    enable-int false
	  then
	else
	  enable-int true true
	then
      ;] serial-spinlock critical-with-spinlock
      dup not if pause then
    until
    if spi-read-rx then
  ;

  \ Get whether an SPI peripheral can have data written
  : >spi? ( spi -- tx? ) dup validate-spi spi-tx-full? not ;

  \ Get whether there is data to receive in the SPI FIFO's
  : spi>? ( spi -- rx? )
    dup validate-spi dup spi-rx-empty? not swap SPI_SSPSR_RNE@ or
  ;

  \ Drain SPI
  : drain-spi ( spi -- )
    dup validate-spi
    begin dup spi>? while dup spi> drop repeat
    begin dup SPI_SSPSR_BSY@ while repeat
    begin dup spi>? while dup spi> drop repeat drop
  ;
  
end-module> import

\ Initialize
: init ( -- )
  init
  0 ^ spi-internal :: init-spi
  1 ^ spi-internal :: init-spi
;

\ reboot