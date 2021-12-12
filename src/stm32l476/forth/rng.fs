\ Copyright (c) 2021 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-import-module rng-module

  begin-import-module rng-internal-module

    \ RCC base
    $40021000 constant RCC_Base
    
    \ RCC AHB2 peripheral clock enable register
    RCC_Base $4C + constant RCC_AHB2ENR

    \ RCC AHB2 peripheral clock stop and sleep enable register
    RCC_Base $6C + constant RCC_AHB2SMENR

    \ Random number generator base address
    $50060800 constant RNG_Base

    \ Random number generator registers
    RNG_Base $00 + constant RNG_CR
    RNG_Base $04 + constant RNG_SR
    RNG_Base $08 + constant RNG_DR

    \ Random number generator accessors
    
    : RCC_AHB2ENR_RNGEN! ( flag -- )
      RCC_AHB2ENR @ 18 bit bic swap 0<> 1 and 18 lshift or RCC_AHB2ENR !
    ;
    
    : RCC_AHB2SMENR_RNGEN! ( flag -- )
      RCC_AHB2SMENR @ 18 bit bic swap 0<> 1 and 18 lshift or RCC_AHB2SMENR !
    ;

    : RNG_CR_CED! ( flag -- )
      RNG_CR @ 5 bit bic swap 0<> 1 and 5 lshift or RNG_CR !
    ;
    
    : RNG_CR_IE! ( flag -- )
      RNG_CR @ 3 bit bic swap 0<> 1 and 3 lshift or RNG_CR !
    ;
    
    : RNG_CR_RNGEN! ( flag -- )
      RNG_CR @ 2 bit bic swap 0<> 1 and 2 lshift or RNG_CR !
    ;

    : RNG_CR_CED@ ( -- flag ) 5 bit RNG_CR bit@ ;

    : RNG_CR_IE@ ( -- flag ) 3 bit RNG_CR bit@ ;
    
    : RNG_CR_RNGEN@ ( -- flag ) 2 bit RNG_CR bit@ ;
    
    : RNG_SR_SEIS! ( flag -- )
      RNG_SR @ 6 bit bic swap 0<> 1 and 6 lshift or RNG_SR !
    ;
    
    : RNG_SR_CEIS! ( flag -- )
      RNG_SR @ 5 bit bic swap 0<> 1 and 5 lshift or RNG_SR !
    ;
    
    : RNG_SR_SECS! ( flag -- )
      RNG_SR @ 2 bit bic swap 0<> 1 and 2 lshift or RNG_SR !
    ;
    
    : RNG_SR_CECS! ( flag -- )
      RNG_SR @ 1 bit bic swap 0<> 1 and 1 lshift or RNG_SR !
    ;
    
    : RNG_SR_DRDY! ( flag -- )
      RNG_SR @ 0 bit bic swap 0<> 1 and 0 lshift or RNG_SR !
    ;
    
    : RNG_SR_SEIS@ ( -- flag ) 6 bit RNG_SR bit@ ;

    : RNG_SR_CEIS@ ( -- flag ) 5 bit RNG_SR bit@ ;
    
    : RNG_SR_SECS@ ( -- flag ) 2 bit RNG_SR bit@ ;
    
    : RNG_SR_CECS@ ( -- flag ) 1 bit RNG_SR bit@ ;
    
    : RNG_SR_DRDY@ ( -- flag ) 0 bit RNG_SR bit@ ;
    
    \ Error detecting a random number
    : x-rng-error ( -- ) space ." RNG error" cr ;

    \ The core of reading a random number
    : random-raw ( -- random )
      begin
	RNG_SR_SECS@ RNG_SR_SEIS@ or if
	  false RNG_SR_SEIS!
	  false RNG_CR_RNGEN!
	  true RNG_CR_RNGEN!
	then
	RNG_SR_CECS@ RNG_SR_CEIS@ or triggers x-rng-error
	RNG_SR_DRDY@
      until
      RNG_DR @
    ;

  end-module

  \ Initialize the random number generator
  : init-rng ( -- )
    true RCC_AHB2ENR_RNGEN!
    true RCC_AHB2SMENR_RNGEN!
    true RNG_CR_RNGEN!
  ;

  \ Read a random number
  : random ( -- random )
    random-raw
  ;

end-module

\ Initialize
: init ( -- )
  init
  init-rng
;

\ Reboot
reboot
