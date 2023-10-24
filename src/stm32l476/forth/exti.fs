\ Copyright (c) 2021-2023 Travis Bemann
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

begin-module exti

  begin-module exti-internal

    \ RCC registers
    $40021000 constant RCC_Base
    RCC_Base $60 + constant RCC_APB2ENR
    RCC_Base $80 + constant RCC_APB2SMENR

    \ RCC fields
    0 bit constant RCC_APB2ENR_SYSCFGEN
    0 bit constant RCC_APB2SMENR_SYSCFGSMEN
    
    \ SYSCFG base address
    $40010000 constant SYSCFG_Base

    \ SYSCFG EXTI configuration register 1
    SYSCFG_Base $08 + constant SYSCFG_EXTICR1

    \ SYSCFG EXTI configuration register x
    : SYSCFG_EXTICRx ( exti -- addr ) 4 u/ cells SYSCFG_EXTICR1 + ;

    \ EXTI base address
    $40010400 constant EXTI_Base

    \ EXTI interrupt mask register 1
    EXTI_Base $00 + constant EXTI_IMR1

    \ EXTI event mask register 1
    EXTI_Base $04 + constant EXTI_EMR1

    \ EXTI rising trigger selection register 1
    EXTI_Base $08 + constant EXTI_RTSR1

    \ EXTI falling trigger selection register 1
    EXTI_Base $0C + constant EXTI_FTSR1

    \ EXTI software interrupt event register 1
    EXTI_Base $10 + constant EXTI_SWIER1

    \ EXTI pending register 1
    EXTI_Base $14 + constant EXTI_PR1

    \ EXTI interrupt mask register 2
    EXTI_Base $00 + constant EXTI_IMR2

    \ EXTI event mask register 2
    EXTI_Base $04 + constant EXTI_EMR2

    \ EXTI rising trigger selection register 2
    EXTI_Base $08 + constant EXTI_RTSR2

    \ EXTI falling trigger selection register 2
    EXTI_Base $0C + constant EXTI_FTSR2

    \ EXTI software interrupt event register 2
    EXTI_Base $10 + constant EXTI_SWIER2

    \ EXTI pending register 2
    EXTI_Base $14 + constant EXTI_PR2

  end-module> import
  
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

  \ Enable the SYSCFG peripheral clock
  : syscfg-clock-enable ( -- )
    RCC_APB2ENR_SYSCFGEN RCC_APB2ENR bis!
  ;

  \ Enable the low-power SYSCFG peripheral clock
  : syscfg-lp-clock-enable ( -- )
    RCC_APB2SMENR_SYSCFGSMEN RCC_APB2SMENR bis!
  ;

  \ Disable the SYSCFG peripheral clock
  : syscfg-clock-disable ( -- )
    RCC_APB2ENR_SYSCFGEN RCC_APB2ENR bic!
  ;

  \ Disable the low-power SYSCFG peripheral clock
  : syscfg-lp-clock-disable ( -- )
    RCC_APB2SMENR_SYSCFGSMEN RCC_APB2SMENR bic!
  ;

  \ Get whether the SYSCFG peripheral clock is enabled
  : syscfg-clock-enable? ( -- enable )
    RCC_APB2ENR_SYSCFGEN RCC_APB2ENR bit@
  ;

  \ Get whether the low-power SYSCFG peripheral clock is enabled
  : syscfg-lp-clock-enable? ( -- enable )
    RCC_APB2SMENR_SYSCFGSMEN RCC_APB2SMENR bit@
  ;
  
    \ Set port for EXTI input
  : SYSCFG_EXTICRx! ( port exti -- )
    swap >r dup SYSCFG_EXTICRx
    dup @ %1111 3 pick 3 and 2 lshift lshift bic
    rot r> swap 3 and 2 lshift lshift or swap !
  ;

  \ Set the EXTI interrupt mask
  : EXTI_IMR! ( mask exti -- )
    dup 32 < if
      swap if bit EXTI_IMR1 bis! else bit EXTI_IMR1 bic! then
    else
      32 - swap if bit EXTI_IMR2 bis! else bit EXTI_IMR2 bic! then
    then
  ;

  \ Set the EXTI event mask
  : EXTI_EMR! ( mask exti -- )
    dup 32 < if
      swap if bit EXTI_EMR1 bis! else bit EXTI_EMR1 bic! then
    else
      32 - swap if bit EXTI_EMR2 bis! else bit EXTI_EMR2 bic! then
    then
  ;

  \ Set the EXTI rising trigger selection register
  : EXTI_RTSR! ( setting exti -- )
    dup 32 < if
      swap if bit EXTI_RTSR1 bis! else bit EXTI_RTSR1 bic! then
    else
      32 - swap if bit EXTI_RTSR2 bis! else bit EXTI_RTSR2 bic! then
    then
  ;

  \ Set the EXTI falling trigger selection register
  : EXTI_FTSR! ( setting exti -- )
    dup 32 < if
      swap if bit EXTI_FTSR1 bis! else bit EXTI_FTSR1 bic! then
    else
      32 - swap if bit EXTI_FTSR2 bis! else bit EXTI_FTSR2 bic! then
    then
  ;

  \ Set the EXTI software interrupt event register
  : EXTI_SWIER! ( exti -- )
    dup 32 < if bit EXTI_SWIER1 bis! else 32 - bit EXTI_SWIER2 bis! then
  ;

  \ Set the EXTI pending register (to clear it)
  : EXTI_PR! ( exti -- )
    dup 32 < if bit EXTI_PR1 ! else 32 - bit EXTI_PR2 ! then
  ;

  \ Get the EXTI input port
  : SYSCFG_EXTICRx@ ( exti -- port )
    dup SYSCFG_EXTICRx @ swap 4 umod 4 * rshift %1111 and
  ;

  \ Get the EXTI interrupt mask
  : EXTI_IMR@ ( exti -- mask )
    dup 32 < if bit EXTI_IMR1 bit@ else 32 - bit EXTI_IMR2 bit@ then
  ;

  \ Get the EXTI event mask
  : EXTI_EMR@ ( exti -- mask )
    dup 32 < if bit EXTI_EMR1 bit@ else 32 - bit EXTI_EMR2 bit@ then
  ;

  \ Get the EXTI rising trigger selection
  : EXTI_RTSR@ ( exti -- setting )
    dup 32 < if bit EXTI_RTSR1 bit@ else 32 - bit EXTI_RTSR2 bit@ then
  ;

  \ Get the EXTI falling trigger selection
  : EXTI_FTSR@ ( exti -- setting )
    dup 32 < if bit EXTI_FTSR1 bit@ else 32 - bit EXTI_FTSR2 bit@ then
  ;

  \ Get the EXTI software interrupt event state
  : EXTI_SWIER@ ( exti -- state )
    dup 32 < if bit EXTI_SWIER1 bit@ else 32 - bit EXTI_SWIER2 bit@ then
  ;

  \ Get the EXTI pending state
  : EXTI_PR@ ( exti -- state )
    dup 32 < if bit EXTI_PR1 bit@ else 32 - bit EXTI_PR2 bit@ then
  ;

end-module

\ Reboot
reboot
