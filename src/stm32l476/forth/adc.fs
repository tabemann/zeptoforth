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

compile-to-flash

begin-module adc

  lock import
  gpio import
  pin import

  \ Invalid ADC exception
  : x-invalid-adc ( -- ) ." invalid ADC" cr ;
  
  \ Invalid ADC channel exception
  : x-invalid-adc-chan ( -- ) ." invalid ADC channel" cr ;
  
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
    $40021000 constant RCC_Base

    \ RCC registers
    RCC_Base $4C + constant RCC_AHB2ENR

    \ RCC register fields
    : RCC_AHB2ENR_ADCEN! ( flag -- )
      13 bit RCC_AHB2ENR rot if bis! else bic! then
    ;

    \ ADC base address
    : ADC_Base ( adc -- addr )
      case
	1 of $50040000 endof
	2 of $50040100 endof
	3 of $50040200 endof
      endcase
    ;

    \ ADC common base address
    $50040300 constant ADC_Common_Base

    \ ADC registers
    : ADC_ISR ( adc -- addr ) ADC_Base $00 + ;
    : ADC_CR ( adc -- addr ) ADC_Base $08 + ;
    : ADC_SQR1 ( adc -- addr ) ADC_Base $30 + ;
    : ADC_DR ( adc -- addr ) ADC_Base $40 + ;

    \ Common ADC registers
    ADC_Common_Base $00 + constant ADC_CSR
    ADC_Common_Base $08 + constant ADC_CCR

    \ ADC register fields
    : ADC_ISR_EOC@ ( adc -- flag ) 1 bit swap ADC_ISR bit@ ;
    : ADC_ISR_ADRDY@ ( adc -- flag ) 0 bit swap ADC_ISR bit@ ;
    : ADC_CR_ADCAL@ ( adc -- flag ) 31 bit swap ADC_CR bit@ ;
    : ADC_CR_ADCAL! ( flag adc -- )
      31 bit swap ADC_CR rot if bis! else bic! then
    ;
    : ADC_CR_ADCALDIF! ( flag adc -- )
      30 bit swap ADC_CR rot if bis! else bic! then
    ;
    : ADC_CR_DEEPPWD! ( flag adc -- )
      29 bit swap ADC_CR rot if bis! else bic! then
    ;
    : ADC_CR_ADVREGEN! ( flag adc -- )
      28 bit swap ADC_CR rot if bis! else bic! then
    ;
    : ADC_CR_ADSTART! ( flag adc -- )
      2 bit swap ADC_CR rot if bis! else bic! then
    ;
    : ADC_CR_ADDIS! ( flag adc -- )
      1 bit swap ADC_CR rot if bis! else bic! then
    ;
    : ADC_CR_ADEN! ( flag adc -- )
      0 bit swap ADC_CR rot if bis! else bic! then
    ;
    : ADC_SQR1_L! ( count adc -- )
      ADC_SQR1 swap $F and over @ $F bic or swap !
    ;
    : ADC_SQR1_SQ1! ( channel adc -- )
      ADC_SQR1 swap $1F and 6 lshift
      over @ [ $1F 6 lshift ] literal bic or swap !
    ;
    : ADC_CCR_CKMODE! ( ckmod -- )
      3 and 16 lshift ADC_CCR @ [ 3 16 lshift ] literal bic or ADC_CCR !
    ;
    : ADC_CCR_CH18SEL! ( flag -- ) 24 bit ADC_CCR rot if bis! else bic! then ;
    : ADC_CCR_CH17SEL! ( flag -- ) 23 bit ADC_CCR rot if bis! else bic! then ;
    : ADC_CCR_VREFEN! ( flag -- ) 22 bit ADC_CCR rot if bis! else bic! then ;

    \ Calibrate an ADC
    : calibrate-adc ( adc -- )
      false over ADC_CR_ADCALDIF!
      true over ADC_CR_ADCAL!
      begin dup ADC_CR_ADCAL@ not until
      true over ADC_CR_ADCALDIF!
      true over ADC_CR_ADCAL!
      begin dup ADC_CR_ADCAL@ not until
      drop
    ;

    \ Check to make sure ADC is ready
    : wait-adc-ready ( adc -- )
      begin dup ADC_ISR_ADRDY@ until
      drop
    ;
    
    \ Initialize the ADC's
    : init-adc ( -- )
      1 adc-lock init-lock
      2 adc-lock init-lock
      3 adc-lock init-lock
      true RCC_AHB2ENR_ADCEN!
      2 ADC_CCR_CKMODE!
      false 1 ADC_CR_DEEPPWD!
      false 2 ADC_CR_DEEPPWD!
      false 3 ADC_CR_DEEPPWD!
      true 1 ADC_CR_ADVREGEN!
      true 2 ADC_CR_ADVREGEN!
      true 3 ADC_CR_ADVREGEN!
      1 ms
      1 calibrate-adc
      2 calibrate-adc
      3 calibrate-adc
      true 1 ADC_CR_ADEN!
      true 2 ADC_CR_ADEN!
      true 3 ADC_CR_ADEN!
      true ADC_CCR_CH18SEL!
      true ADC_CCR_CH17SEL!
      false ADC_CCR_VREFEN!
    ;
    
  end-module> import

  \ Enable the VBAT internal channel connected to ADC1_INP18
  : enable-vbat ( -- ) true ADC_CCR_CH18SEL! ;

  \ Disable the VBAT internal channel connected to ADC1_INP18
  : disable-vbat ( -- ) false ADC_CCR_CH18SEL! ;

  \ Enable the temperature sensor internal channel connected to ADC1_INP17
  : enable-vsense ( -- ) true ADC_CCR_CH17SEL! ;

  \ Disable the temperature sensor internal channel connected to ADC1_INP17
  : disable-vsense ( -- ) false ADC_CCR_CH17SEL! ;

  \ Enable the VREFINT internal channel connected to ADC1_INP0
  : enable-vrefint ( -- ) true ADC_CCR_VREFEN! ;

  \ Disable the VREFINT internal channel connected to ADC1_INP0
  : disable-vrefint ( -- ) false ADC_CCR_VREFEN! ;
  
  \ Enable an ADC
  : enable-adc ( adc -- )
    dup validate-adc
    [:
      false over ADC_CR_DEEPPWD!
      true over ADC_CR_ADVREGEN!
      1 ms
      dup calibrate-adc
      true swap ADC_CR_ADEN!
    ;] over adc-lock with-lock
  ;

  \ Disable an ADC
  : disable-adc ( adc -- )
    dup validate-adc
    [:
      true over ADC_CR_ADDIS!
      false swap ADC_CR_DEEPPWD!
    ;] over adc-lock with-lock
  ;

  \ Set a pin to be an ADC pin
  : adc-pin ( adc pin -- )
    dup ^ pin-internal :: validate-pin
    swap validate-adc
    ANALOG_MODE swap ^ pin-internal :: extract-both MODER!
  ;

  \ Default ADC
  1 constant default-adc

  \ Internal temperature sensor ADC channel
  17 constant temp-adc-chan

  \ Internal reference voltage ADC channel
  0 constant vrefint-adc-chan

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
      dup wait-adc-ready
      1 over ADC_SQR1_L!
      tuck ADC_SQR1_SQ1!
      true over ADC_CR_ADSTART!
      begin dup ADC_ISR_EOC@ until
      ADC_DR @
    ;] over adc-lock with-lock
  ;
  
end-module> import

\ Initialize
: init ( -- ) init ^ adc-internal :: init-adc ;

reboot
