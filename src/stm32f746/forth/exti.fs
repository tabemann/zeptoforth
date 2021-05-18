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

compile-to-flash

begin-module-once exti-module

  begin-import-module exti-internal-module

    \ RCC registers
    $40023800 constant RCC_Base
    RCC_Base $44 + constant RCC_APB2ENR
    RCC_Base $64 + constant RCC_APB2LPENR

    \ RCC fields
    14 bit constant RCC_APB2ENR_SYSCFGEN
    14 bit constant RCC_APB2LPENR_SYSCFGLPEN

    \ SYSCFG base address
    $40013800 constant SYSCFG_Base

    \ SYSCFG EXTI configuration register 1
    SYSCFG_Base $08 + constant SYSCFG_EXTICR1

    \ SYSCFG EXTI configuration register x
    : SYSCFG_EXTICRx ( exti -- addr ) 4 u/ cells SYSCFG_EXTICR1 + ;

    \ EXTI base address
    $40013C00 constant EXTI_Base

    \ EXTI interrupt mask register
    EXTI_Base $00 + constant EXTI_IMR

    \ EXTI event mask register
    EXTI_Base $04 + constant EXTI_EMR

    \ EXTI rising trigger selection register
    EXTI_Base $08 + constant EXTI_RTSR

    \ EXTI falling trigger selection register
    EXTI_Base $0C + constant EXTI_FTSR

    \ EXTI software interrupt event register
    EXTI_Base $10 + constant EXTI_SWIER

    \ EXTI pending register
    EXTI_Base $14 + constant EXTI_PR

  end-module
  
  \ EXTI interrupts
  6 constant EXTI_0
  7 constant EXTI_1
  8 constant EXTI_2
  9 constant EXTI_3
  10 constant EXTI_4
  23 constant EXTI_9_5
  40 constant EXTI_15_10

  \ Ports
  0 constant PA
  1 constant PB
  2 constant PC
  3 constant PD
  4 constant PE
  5 constant PF
  6 constant PG
  7 constant PH
  8 constant PI
  9 constant PJ
  10 constant PK
  
  \ Enable the SYSCFG peripheral clock
  : syscfg-clock-enable ( -- )
    RCC_APB2ENR_SYSCFGEN RCC_APB2ENR bis!
  ;

  \ Enable the low-power SYSCFG peripheral clock
  : syscfg-lp-clock-enable ( -- )
    RCC_APB2LPENR_SYSCFGLPEN RCC_APB2LPENR bis!
  ;

  \ Disable the SYSCFG peripheral clock
  : syscfg-clock-disable ( -- )
    RCC_APB2ENR_SYSCFGEN RCC_APB2ENR bic!
  ;

  \ Disable the low-power SYSCFG peripheral clock
  : syscfg-lp-clock-disable ( -- )
    RCC_APB2LPENR_SYSCFGLPEN RCC_APB2LPENR bic!
  ;

  \ Set port for EXTI input
  : SYSCFG_EXTICRx! ( port exti -- )
    swap >r dup SYSCFG_EXTICRx
    dup @ %1111 3 pick 3 and 2 lshift lshift bic
    rot r> swap 3 and 2 lshift lshift or swap !
  ;

  \ Set the EXTI interrupt mask
  : EXTI_IMR! ( mask exti -- )
    swap if bit EXTI_IMR bis! else bit EXTI_IMR bic! then
  ;

  \ Set the EXTI event mask
  : EXTI_EMR! ( mask exti -- )
    swap if bit EXTI_EMR bis! else bit EXTI_EMR bic! then
  ;

  \ Set the EXTI rising trigger selection register
  : EXTI_RTSR! ( setting exti -- )
    swap if bit EXTI_RTSR bis! else bit EXTI_RTSR bic! then
  ;

  \ Set the EXTI falling trigger selection register
  : EXTI_FTSR! ( setting exti -- )
    swap if bit EXTI_FTSR bis! else bit EXTI_FTSR bic! then
  ;

  \ Set the EXTI software interrupt event register
  : EXTI_SWIER! ( exti -- ) bit EXTI_SWIER bis! ;

  \ Set the EXTI pending register (to clear it)
  : EXTI_PR! ( exti -- ) bit EXTI_PR bis! ;

  \ Get the EXTI input port
  : SYSCFG_EXTICRx@ ( exti -- port )
    dup SYSCFG_EXTICRx @ swap 4 umod 4 * rshift %1111 and
  ;

  \ Get the EXTI interrupt mask
  : EXTI_IMR@ ( exti -- mask ) bit EXTI_IMR bit@ ;

  \ Get the EXTI event mask
  : EXTI_EMR@ ( exti -- mask ) bit EXTI_EMR bit@ ;

  \ Get the EXTI rising trigger selection
  : EXTI_RTSR@ ( exti -- setting ) bit EXTI_RTSR bit@ ;

  \ Get the EXTI falling trigger selection
  : EXTI_FTSR@ ( exti -- setting ) bit EXTI_FTSR bit@ ;

  \ Get the EXTI software interrupt event state
  : EXTI_SWIER@ ( exti -- state ) bit EXTI_SWIER bit@ ;

  \ Get the EXTI pending state
  : EXTI_PR@ ( exti -- state ) bit EXTI_PR bit@ ;

end-module

\ Warm reboot
warm
