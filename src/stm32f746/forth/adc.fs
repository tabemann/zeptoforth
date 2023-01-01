\ Copyright (c) 2022-2023 Travis Bemann
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

compile-to-flash

begin-module adc

  lock import
  gpio import
  pin import

  \ Invalid ADC exception
  : x-invalid-adc ( -- ) ." invalid ADC" cr ;
  
  \ Invalid ADC channel exception
  : x-invalid-adc-chan ( -- ) ." invalid ADC channel" cr ;
  
  \ Out of range sampling time exception
  : x-out-of-range-sampling-time ( -- ) ." out of range sampling time" cr ;
  
  begin-module adc-internal

    \ Validate an ADC
    : validate-adc ( adc -- ) dup 1 >= swap 3 <= and averts x-invalid-adc ;
    
    \ Validate an ADC channel
    : validate-adc-chan ( adc-chan -- ) 19 u< averts x-invalid-adc-chan ;
    
    \ ADC lock
    lock-size 3 * buffer: adc-lock

    \ Get the current adc lcok
    : adc-lock ( adc -- lock ) 1- lock-size * adc-lock + ;

    \ RCC base address
    $40023800 constant RCC_Base

    \ RCC registers
    RCC_Base $44 + constant RCC_APB2ENR

    \ RCC register fields
    : RCC_APB2ENR_ADC1EN! ( flag -- )
      8 bit RCC_APB2ENR rot if bis! else bic! then
    ;
    : RCC_APB2ENR_ADC2EN! ( flag -- )
      9 bit RCC_APB2ENR rot if bis! else bic! then
    ;
    : RCC_APB2ENR_ADC3EN! ( flag -- )
      10 bit RCC_APB2ENR rot if bis! else bic! then
    ;

    \ ADC base address
    : ADC_Base ( adc -- addr )
      case
	1 of $40012000 endof
	2 of $40012100 endof
	3 of $40012200 endof
      endcase
    ;

    \ ADC common base address
    $40012300 constant ADC_Common_Base

    \ ADC registers
    : ADC_SR ( adc -- addr ) ADC_Base $00 + ;
    : ADC_CR1 ( adc -- addr ) ADC_Base $04 + ;
    : ADC_CR2 ( adc -- addr ) ADC_Base $08 + ;
    : ADC_SMPR1 ( adc -- adr ) ADC_Base $0C + ;
    : ADC_SMPR2 ( adc -- adr ) ADC_Base $10 + ;
    : ADC_SQR1 ( adc -- addr ) ADC_Base $2C + ;
    : ADC_SQR2 ( adc -- addr ) ADC_Base $30 + ;
    : ADC_SQR3 ( adc -- addr ) ADC_Base $34 + ;
    : ADC_DR ( adc -- addr ) ADC_Base $4C + ;

    \ Common ADC registers
    ADC_Common_Base $00 + constant ADC_CSR
    ADC_Common_Base $04 + constant ADC_CCR

    \ ADC register fields
    : ADC_SR_EOC@ ( adc -- flag ) 1 bit swap ADC_SR bit@ ;
    : ADC_CR2_SWSTART! ( flag adc -- )
      30 bit swap ADC_CR2 rot if bis! else bic! then
    ;
    : ADC_CR2_EOCS! ( flag adc -- )
      10 bit swap ADC_CR2 rot if bis! else bic! then
    ;
    : ADC_CR2_ADON! ( flag adc -- )
      0 bit swap ADC_CR2 rot if bis! else bic! then
    ;
    : ADC_SMPRx_SMPx! ( sampling channel adc -- )
      over 10 u< if ADC_SMPR2 else ADC_SMPR1 swap 10 - swap then
      >r r@ @ swap 3 * tuck $7 swap lshift bic
      -rot lshift or r> !
    ;
    : ADC_SQR1_L! ( count adc -- )
      ADC_SQR1 swap $F and 20 lshift
      over @ [ $F 20 lshift ] literal bic or
      swap !
    ;
    : ADC_SQR3_SQ1! ( channel adc -- )
      ADC_SQR3 swap $1F and over @ $1F bic or swap !
    ;
    : ADC_CCR_VBATE! ( flag -- ) 22 bit ADC_CCR rot if bis! else bic! then ;
    : ADC_CCR_TSVREFE! ( flag -- ) 23 bit ADC_CCR rot if bis! else bic! then ;
    
    \ Initialize the ADC's
    : init-adc ( -- )
      1 adc-lock init-lock
      2 adc-lock init-lock
      3 adc-lock init-lock
      true RCC_APB2ENR_ADC1EN!
      true RCC_APB2ENR_ADC2EN!
      true RCC_APB2ENR_ADC3EN!
      true 1 ADC_CR2_ADON!
      true 2 ADC_CR2_ADON!
      true 3 ADC_CR2_ADON!
      false ADC_CCR_VBATE!
      true ADC_CCR_TSVREFE!
    ;
    
    \ Sampling times array
    create sampling-times 3 h, 15 h, 28 h, 56 h, 84 h, 112 h, 144 h, 480 h,

  end-module> import

  \ Enable the VBAT internal channel
  : enable-vbat ( -- ) true ADC_CCR_VBATE! ;

  \ Disable the VBAT internal channel
  : disable-vbat ( -- ) false ADC_CCR_VBATE! ;

  \ Enable the temperature sensor and VREFINT internal channels
  : enable-tsvref ( -- ) true ADC_CCR_TSVREFE! ;

  \ Disable the temperature sensor and VREFINT internal channels
  : disable-tsvref ( -- ) false ADC_CCR_TSVREFE! ;
  
  \ Set sampling time for a channel
  : adc-sampling-time! ( sampling channel adc -- )
    dup validate-adc
    over validate-adc-chan
    rot dup 480 u<= averts x-out-of-range-sampling-time
    [:
      >r 0 begin
	dup 2* sampling-times + h@ r@ u>= if
	  rdrop -rot ADC_SMPRx_SMPx! true
	else
	  1+ false
	then
      until
    ;] 2 pick adc-lock with-lock
  ;
  
  \ Enable an ADC
  : enable-adc ( adc -- )
    dup validate-adc
    [:
      dup case
	1 of true RCC_APB2ENR_ADC1EN! endof
	2 of true RCC_APB2ENR_ADC2EN! endof
	3 of true RCC_APB2ENR_ADC3EN! endof
      endcase
      true swap ADC_CR2_ADON!
    ;] over adc-lock with-lock
  ;

  \ Disable an ADC
  : disable-adc ( adc -- )
    dup validate-adc
    [:
      false over ADC_CR2_ADON!
      case
	1 of false RCC_APB2ENR_ADC1EN! endof
	2 of false RCC_APB2ENR_ADC2EN! endof
	3 of false RCC_APB2ENR_ADC3EN! endof
      endcase
    ;] over adc-lock with-lock
  ;

  \ Set a pin to be an ADC pin
  : adc-pin ( adc pin -- )
    dup pin-internal::validate-pin
    swap validate-adc
    ANALOG_MODE swap pin-internal::extract-both MODER!
  ;

  \ Default ADC
  1 constant default-adc

  \ Internal temperature sensor ADC channel
  18 constant temp-adc-chan

  \ Internal reference voltage ADC channel
  17 constant vrefint-adc-chan

  \ VBAT ADC channel
  18 constant vbat-adc-chan

  \ Minimum ADC value
  $000 constant adc-min
  
  \ Maximum ADC value
  $FFF constant adc-max

  \ Get an ADC value
  : adc@ ( channel adc -- value )
    dup validate-adc
    over validate-adc-chan
    [:
      true over ADC_CR2_EOCS!
      0 over ADC_SQR1_L!
      tuck ADC_SQR3_SQ1!
      true over ADC_CR2_SWSTART!
      begin dup ADC_SR_EOC@ until
      ADC_DR @
    ;] over adc-lock with-lock
  ;
  
end-module> import

\ Initialize
: init ( -- ) init adc-internal::init-adc ;

reboot
