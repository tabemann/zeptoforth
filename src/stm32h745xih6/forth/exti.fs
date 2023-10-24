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
    $58024400 constant RCC_Base
    RCC_Base $F4 + constant RCC_APB4ENR
    RCC_Base $11C + constant RCC_APB4LPENR

    \ RCC fields
    1 bit constant RCC_APB4ENR_SYSCFGEN
    1 bit constant RCC_APB4LPENR_SYSCFGLPEN

    \ SYSCFG base address
    $58000400 constant SYSCFG_Base

    \ SYSCFG EXTI configuration register 1
    SYSCFG_Base $08 + constant SYSCFG_EXTICR1

    \ SYSCFG EXTI configuration register x
    : SYSCFG_EXTICRx ( exti -- addr ) 4 u/ cells SYSCFG_EXTICR1 + ;

    \ EXTI base address
    $58000000 constant EXTI_Base

    \ EXTI rising trigger selection register
    EXTI_Base $00 + constant EXTI_RTSR1

    \ EXTI rising trigger selection register
    EXTI_Base $20 + constant EXTI_RTSR2

    \ EXTI rising trigger selection register
    EXTI_Base $40 + constant EXTI_RTSR3

    \ EXTI falling trigger selection register
    EXTI_Base $04 + constant EXTI_FTSR1

    \ EXTI falling trigger selection register
    EXTI_Base $24 + constant EXTI_FTSR2

    \ EXTI falling trigger selection register
    EXTI_Base $44 + constant EXTI_FTSR3

    \ EXTI software interrupt event register
    EXTI_Base $08 + constant EXTI_SWIER1

    \ EXTI software interrupt event register
    EXTI_Base $28 + constant EXTI_SWIER2

    \ EXTI software interrupt event register
    EXTI_Base $48 + constant EXTI_SWIER3

    \ EXTI core 0 interrupt mask register
    EXTI_Base $80 + constant EXTI_C1IMR1

    \ EXTI core 1 interrupt mask register
    EXTI_Base $C0 + constant EXTI_C2IMR1

    \ EXTI core 0 event mask register
    EXTI_Base $84 + constant EXTI_C1EMR1

    \ EXTI core 1 event mask register
    EXTI_Base $C4 + constant EXTI_C2EMR1

    \ EXTI core 0 pending register
    EXTI_Base $88 + constant EXTI_C1PR1

    \ EXTI core 1 pending register
    EXTI_Base $C8 + constant EXTI_C2PR1

    \ EXTI core 0 interrupt mask register
    EXTI_Base $90 + constant EXTI_C1IMR2

    \ EXTI core 1 interrupt mask register
    EXTI_Base $D0 + constant EXTI_C2IMR2

    \ EXTI core 0 event mask register
    EXTI_Base $94 + constant EXTI_C1EMR2

    \ EXTI core 1 event mask register
    EXTI_Base $D4 + constant EXTI_C2EMR2

    \ EXTI core 0 pending register
    EXTI_Base $98 + constant EXTI_C1PR2

    \ EXTI core 1 pending register
    EXTI_Base $D8 + constant EXTI_C2PR2

    \ EXTI core 0 interrupt mask register
    EXTI_Base $A0 + constant EXTI_C1IMR3

    \ EXTI core 1 interrupt mask register
    EXTI_Base $E0 + constant EXTI_C2IMR3

    \ EXTI core 0 event mask register
    EXTI_Base $A4 + constant EXTI_C1EMR3

    \ EXTI core 1 event mask register
    EXTI_Base $E4 + constant EXTI_C2EMR3

    \ EXTI core 0 pending register
    EXTI_Base $A8 + constant EXTI_C1PR3

    \ EXTI core 1 pending register
    EXTI_Base $E8 + constant EXTI_C2PR3

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
  9 constant PJ
  10 constant PK
  
  \ Enable the SYSCFG peripheral clock
  : syscfg-clock-enable ( -- )
    RCC_APB4ENR_SYSCFGEN RCC_APB4ENR bis!
  ;

  \ Enable the low-power SYSCFG peripheral clock
  : syscfg-lp-clock-enable ( -- )
    RCC_APB4LPENR_SYSCFGLPEN RCC_APB4LPENR bis!
  ;

  \ Disable the SYSCFG peripheral clock
  : syscfg-clock-disable ( -- )
    RCC_APB4ENR_SYSCFGEN RCC_APB4ENR bic!
  ;

  \ Disable the low-power SYSCFG peripheral clock
  : syscfg-lp-clock-disable ( -- )
    RCC_APB4LPENR_SYSCFGLPEN RCC_APB4LPENR bic!
  ;

  \ Get whether the SYSCFG peripheral clock is enabled
  : syscfg-clock-enable? ( -- enable )
    RCC_APB4ENR_SYSCFGEN RCC_APB4ENR bit@
  ;

  \ Get whether the low-power SYSCFG peripheral clock is enabled
  : syscfg-lp-clock-enable? ( -- enable )
    RCC_APB4LPENR_SYSCFGLPEN RCC_APB4LPENR bit@
  ;
  
  \ Set port for EXTI input
  : SYSCFG_EXTICRx! ( port exti -- )
    swap >r dup SYSCFG_EXTICRx
    dup @ %1111 3 pick 3 and 2 lshift lshift bic
    rot r> swap 3 and 2 lshift lshift or swap !
  ;

  \ Set the EXTI interrupt mask
  : EXTI_IMR! ( mask exti cpu-index -- )
    1 = if
      dup 32 u< if
        swap if bit EXTI_C2IMR1 bis! else bit EXTI_C2IMR1 bic! then
      else
        dup 64 u< if
          32 - swap if bit EXTI_C2IMR2 bis! else bit EXTI_C2IMR2 bic! then
        else
          64 - swap if bit EXTI_C2IMR3 bis! else bit EXTI_C2IMR3 bic! then
        then
      then
    else
      dup 32 u< if
        swap if bit EXTI_C1IMR1 bis! else bit EXTI_C1IMR1 bic! then
      else
        dup 64 u< if
          32 - swap if bit EXTI_C1IMR2 bis! else bit EXTI_C1IMR2 bic! then
        else
          64 - swap if bit EXTI_C1IMR3 bis! else bit EXTI_C1IMR3 bic! then
        then
      then
    then
  ;
  
  \ Set the EXTI event mask
  : EXTI_EMR! ( mask exti cpu-index -- )
    1 = if
      dup 32 u< if
        swap if bit EXTI_C2EMR1 bis! else bit EXTI_C2EMR1 bic! then
      else
        dup 64 u< if
          32 - swap if bit EXTI_C2EMR2 bis! else bit EXTI_C2EMR2 bic! then
        else
          64 - swap if bit EXTI_C2EMR3 bis! else bit EXTI_C2EMR3 bic! then
        then
      then
    else
      dup 32 u< if
        swap if bit EXTI_C1EMR1 bis! else bit EXTI_C1EMR1 bic! then
      else
        dup 64 u< if
          32 - swap if bit EXTI_C1EMR2 bis! else bit EXTI_C1EMR2 bic! then
        else
          64 - swap if bit EXTI_C1EMR3 bis! else bit EXTI_C1EMR3 bic! then
        then
      then
    then
  ;

  \ Set the EXTI rising trigger selection register
  : EXTI_RTSR! ( setting exti -- )
    dup 32 u< if
      swap if bit EXTI_RTSR1 bis! else bit EXTI_RTSR1 bic! then
    else
      dup 64 u< if
        32 - swap if bit EXTI_RTSR2 bis! else bit EXTI_RTSR2 bic! then
      else
        64 - swap if bit EXTI_RTSR3 bis! else bit EXTI_RTSR3 bic! then
      then
    then
  ;

  \ Set the EXTI falling trigger selection register
  : EXTI_FTSR! ( setting exti -- )
    dup 32 u< if
      swap if bit EXTI_FTSR1 bis! else bit EXTI_FTSR1 bic! then
    else
      dup 64 u< if
        32 - swap if bit EXTI_FTSR2 bis! else bit EXTI_FTSR2 bic! then
      else
        64 - swap if bit EXTI_FTSR3 bis! else bit EXTI_FTSR3 bic! then
      then
    then
  ;

  \ Set the EXTI software interrupt event register
  : EXTI_SWIER! ( exti -- )
    dup 32 u< if
      swap if bit EXTI_SWIER1 bis! else bit EXTI_SWIER1 bic! then
    else
      dup 64 u< if
        32 - swap if bit EXTI_SWIER2 bis! else bit EXTI_SWIER2 bic! then
      else
        64 - swap if bit EXTI_SWIER3 bis! else bit EXTI_SWIER3 bic! then
      then
    then
  ;

  \ Set the EXTI pending register (to clear it)
  : EXTI_PR! ( exti cpu-index -- )
    1 = if
      dup 32 u< if
        bit EXTI_C2IMR1 !
      else
        dup 64 u< if
          32 - bit EXTI_C2IMR2 !
        else
          64 - bit EXTI_C2IMR3 !
        then
      then
    else
      dup 32 u< if
        bit EXTI_C1IMR1 !
      else
        dup 64 u< if
          32 - bit EXTI_C1IMR2 !
        else
          64 - bit EXTI_C1IMR3 !
        then
      then
    then
  ;

  \ Get the EXTI input port
  : SYSCFG_EXTICRx@ ( exti -- port )
    dup SYSCFG_EXTICRx @ swap 4 umod 4 * rshift %1111 and
  ;

  \ Get the EXTI interrupt mask
  : EXTI_IMR@ ( exti cpu-index -- mask )
    1 = if
      dup 32 u< if
        bit EXTI_C2IMR1 bit@
      else
        dup 64 u< if
          32 - bit EXTI_C2IMR2 bit@
        else
          64 - bit EXTI_C2IMR3 bit@
        then
      then
    else
      dup 32 u< if
        bit EXTI_C1IMR1 bit@
      else
        dup 64 u< if
          32 - bit EXTI_C1IMR2 bit@
        else
          64 - bit EXTI_C1IMR3 bit@
        then
      then
    then
  ;

  \ Get the EXTI event mask
  : EXTI_EMR@ ( exti cpu-index -- mask )
    1 = if
      dup 32 u< if
        bit EXTI_C2EMR1 bit@
      else
        dup 64 u< if
          32 - bit EXTI_C2EMR2 bit@
        else
          64 - bit EXTI_C2EMR3 bit@
        then
      then
    else
      dup 32 u< if
        bit EXTI_C1EMR1 bit@
      else
        dup 64 u< if
          32 - bit EXTI_C1EMR2 bit@
        else
          64 - bit EXTI_C1EMR3 bit@
        then
      then
    then
  ;

  \ Get the EXTI rising trigger selection
  : EXTI_RTSR@ ( exti -- setting )
    dup 32 u< if
      bit EXTI_RTSR1 bit@
    else
      dup 64 u< if
        32 - bit EXTI_RTSR2 bit@
      else
        64 - bit EXTI_RTSR3 bit@
      then
    then
  ;

  \ Get the EXTI falling trigger selection
  : EXTI_FTSR@ ( exti -- setting )
    dup 32 u< if
      bit EXTI_FTSR1 bit@
    else
      dup 64 u< if
        32 - bit EXTI_FTSR2 bit@
      else
        64 - bit EXTI_FTSR3 bit@
      then
    then
  ;

  \ Get the EXTI software interrupt event state
  : EXTI_SWIER@ ( exti -- setting )
    dup 32 u< if
      bit EXTI_SWIER1 bit@
    else
      dup 64 u< if
        32 - bit EXTI_SWIER2 bit@
      else
        64 - bit EXTI_SWIER3 bit@
      then
    then
  ;

  \ Get the EXTI pending state
  : EXTI_PR@ ( exti cpu-index -- state )
    1 = if
      dup 32 u< if
        bit EXTI_C2PR1 bit@
      else
        dup 64 u< if
          32 - bit EXTI_C2PR2 bit@
        else
          64 - bit EXTI_C2PR3 bit@
        then
      then
    else
      dup 32 u< if
        bit EXTI_C1PR1 bit@
      else
        dup 64 u< if
          32 - bit EXTI_C1PR2 bit@
        else
          64 - bit EXTI_C1PR3 bit@
        then
      then
    then
  ;

end-module

\ Reboot
reboot
