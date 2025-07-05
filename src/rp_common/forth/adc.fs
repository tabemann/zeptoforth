\ Copyright (c) 2022-2024 Travis Bemann
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
  pin import

  \ Invalid ADC exception
  : x-invalid-adc ( -- ) ." invalid ADC" cr ;

  \ Invalid ADC channel exception
  : x-invalid-adc-chan ( -- ) ." invalid ADC channel" cr ;
  
  \ Pin has no ADC channel exception
  : x-pin-has-no-adc-chan ( -- ) ." pin has no ADC channel" cr ;
  
  begin-module adc-internal

    \ Validate an ADC
    : validate-adc ( adc -- ) 0= averts x-invalid-adc ;
    
    \ Validate an ADC channel
    rp2040? [if]
      : validate-adc-chan ( adc-chan -- ) 5 u< averts x-invalid-adc-chan ;
    [then]
    rp2350? [if]
      : validate-adc-chan ( adc-chan -- ) 9 u< averts x-invalid-adc-chan ;
    [then]
    
    \ ADC lock
    lock-size buffer: adc-lock
    
    \ ADC base address
    rp2040? [if]
      $4004C000 constant ADC_BASE
    [then]
    rp2350? [if]
      $400A0000 constant ADC_BASE
    [then]
    
    \ ADC registers
    ADC_BASE $00 + constant ADC_CS
    ADC_BASE $04 + constant ADC_RESULT

    \ ADC register fields
    : ADC_CS_AINSEL! ( adc-chan -- )
      ADC_CS @ [ $7 12 lshift ] literal bic
      swap 12 lshift or ADC_CS !
    ;
    : ADC_CS_ERR_STICKY! ( -- ) 10 bit ADC_CS bis! ;
    : ADC_CS_ERR_STICKY@ ( -- flag ) 10 bit ADC_CS bit@ ;
    : ADC_CS_ERR@ ( -- flag ) 9 bit ADC_CS bit@ ;
    : ADC_CS_READY@ ( -- flag ) 8 bit ADC_CS bit@ ;
    : ADC_CS_START_ONCE! ( flag -- ) 2 bit ADC_CS rot if bis! else bic! then ;
    : ADC_CS_TS_EN! ( flag -- ) 1 bit ADC_CS rot if bis! else bic! then ;
    : ADC_CS_EN! ( flag -- ) 0 bit ADC_CS rot if bis! else bic! then ;

    \ Initialize the ADC's
    : init-adc ( -- )
      adc-lock init-lock
      true ADC_CS_EN!
      true ADC_CS_TS_EN!
    ;
    
  end-module> import

  \ Enable the ADC
  : enable-adc ( adc -- )
    validate-adc
    [:
      true ADC_CS_EN!
      true ADC_CS_TS_EN!
    ;] adc-lock with-lock
  ;
  
  \ Disable the ADC
  : disable-adc ( adc -- )
    validate-adc
    [:
      false ADC_CS_TS_EN!
      false ADC_CS_EN!
    ;] adc-lock with-lock
  ;

  \ Set a pin to be an ADC pin
  : adc-pin ( adc pin -- )
    dup pin-internal::validate-pin
    [ rp2040? ] [if]
      dup 26 >= over 29 <= and averts x-pin-has-no-adc-chan
    [then]
    [ rp2350? ] [if]
      dup 26 >= over 29 <= and over 40 >= over 47 <= and or
      averts x-pin-has-no-adc-chan
    [then]
    swap validate-adc
    [ rp2350? ] [if] true over gpio::PADS_BANK0_ISO! [then]
    true over gpio::PADS_BANK0_OD!
    false over gpio::PADS_BANK0_IE!
    false over gpio::PADS_BANK0_PUE!
    false over gpio::PADS_BANK0_PDE!
    [ rp2350? ] [if] false swap gpio::PADS_BANK0_ISO! [else] drop [then]
  ;

  \ Get the ADC channel for a pin
  : pin-adc-chan ( pin -- adc-chan )
    dup pin-internal::validate-pin
    [ rp2040? ] [if]
      dup 26 >= over 29 <= and averts x-pin-has-no-adc-chan
      26 -
    [then]
    [ rp2350? ] [if]
      dup 26 >= over 29 <= and over 40 >= 2 pick 47 <= and or
      averts x-pin-has-no-adc-chan
      dup 40 < if 26 - else 40 - then
    [then]
  ;

  \ Default ADC
  0 constant default-adc
  
  \ Internal temperature sensor ADC channel (on the RP2040, RP2350A and RP2354A)
  4 constant temp-adc-chan
  
  \ Minimum ADC value
  $000 constant adc-min
  
  \ Maximum ADC value
  $FFF constant adc-max

  \ Get an ADC value
  : adc@ ( adc-chan adc -- value )
    validate-adc
    dup validate-adc-chan
    [:
      begin ADC_CS_READY@ until
      ADC_CS_AINSEL!
      true ADC_CS_START_ONCE!
      begin ADC_CS_READY@ until
      ADC_RESULT @
    ;] adc-lock with-lock
  ;
  
end-module> import

\ Initialize
: init ( -- ) init adc-internal::init-adc ;

reboot
