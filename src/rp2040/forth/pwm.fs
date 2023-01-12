\ Copyright (c) 2023 Travis Bemann
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

begin-module pwm

  interrupt import
  pin import

  \ Out of range PWM index
  : x-out-of-range-pwm ." out of ramge PWM slice" cr ;

  \ Out of range clock divisor
  : x-out-of-range-clock-div ." out of range clock divisor" cr ;

  \ Out of range counter value
  : x-out-of-range-counter ." out of range counter value" cr ;

  \ Out of range compare value
  : x-out-of-range-compare ." out of range compare value" cr ;

  \ Out of range top value
  : x-out-of-range-top ." out of range top value" cr ;

  begin-module pwm-internal

    \ PWM base register
    $40050000 constant PWM_BASE

    \ PWM registers
    : pwm-regs ( index -- addr ) $14 * PWM_BASE + ;

    \ PWM control and status register
    : CH_CSR ( index -- addr ) pwm-regs $00 + ;

    \ PWM clock divider register
    : CH_DIV ( index -- addr ) pwm-regs $04 + ;

    \ PWM direct counter access register
    : CH_CTR ( index -- addr ) pwm-regs $08 + ;

    \ PWM counter compare values register
    : CH_CC ( index -- addr ) pwm-regs $0C + ;

    \ PWM counter wrap value register
    : CH_TOP ( index -- addr ) pwm-regs $10 + ;

    \ PWM multichannel enable register
    PWM_BASE $A0 + constant EN

    \ PWM raw interrupts register
    PWM_BASE $A4 + constant INTR

    \ PWM interrupt enable register
    PWM_BASE $A8 + constant INTE

    \ PWM interrupt force register
    PWM_BASE $AC + constant INTF

    \ PWM interrupt status after masking and forcing
    PWM_BASE $B0 + constant INTS

    \ Free-running counting at rate dictated by fractional divider
    0 constant DIVMODE_FREE_RUNNING

    \ Fractional divider operation is gated by the PWM B pin
    1 constant DIVMODE_GATED

    \ Counter advances with each rising edge of the PWM B pin
    2 constant DIVMODE_RISING_EDGE

    \ Counter advances with each falling edge of the PWM B pin
    3 constant DIVMODE_FALLING_EDGE

    \ Advance the phase of the counter by 1 count while it is running
    : CH_CSR_PH_ADV! ( index -- ) 7 bit swap CH_CSR bis! ;

    \ Get the state of advancing the phase of the counter by 1 count while it
    \ is running
    : CH_CSR_PH_ADV@ ( index -- state ) 7 bit swap CH_CSR bit@ ;

    \ Retard the phase of the counter by 1 count while it is running
    : CH_CSR_PH_RET! ( index -- ) 6 bit swap CH_CSR bis! ;

    \ Get the state of retarding the phase of the counter by 1 count while it
    \ is running
    : CH_CSR_PH_RET@ ( index -- state ) 6 bit swap CH_CSR bit@ ;

    \ Set the counter move
    : CH_CSR_DIVMODE! ( divmode index -- )
      CH_CSR { reg } reg @ $30 bic swap $3 and 4 lshift or reg !
    ;

    \ Set invert output B
    : CH_CSR_B_INV! ( state index -- )
      swap if 3 bit swap CH_CSR bis! else 3 bit swap CH_CSR bic! then
    ;

    \ Set invert output A
    : CH_CSR_A_INV! ( state index -- )
      swap if 2 bit swap CH_CSR bis! else 2 bit swap CH_CSR bic! then
    ;

    \ Set phase-correct modulation
    : CH_CSR_PH_CORRECT! ( state index -- )
      swap if 1 bit swap CH_CSR bis! else 1 bit swap CH_CSR bic! then
    ;

    \ Set clock frequency divisor; int is the integral portion from 1 to 255
    \ and fract is the fractional portion from 0 to 15
    : CH_DIV! { fract int index -- }
      int $FF and 4 lshift fract $F and or index CH_DIV !
    ;

    \ Set the B channel counter compare value, from 0 to 65535
    : CH_CC_B! { cc index -- }
      index CH_CC { reg } reg @ $FFFF0000 bic cc 16 lshift or reg !
    ;

    \ Set the A channel counter compare value, from 0 to 65535
    : CH_CC_A! { cc index -- }
      index CH_CC { reg } reg @ $0000FFFF bic cc $FFFF and or reg !
    ;
    
    \ PWM alternate function
    4 constant pwm-alternate

    \ PWM wrap IRQ
    4 constant pwm-irq

    \ PWM wrap vector
    pwm-irq 16 + constant pwm-vector

    \ Saved second core hook
    variable core-init-hook-saved

    \ Initialize PWM on the second core
    : init-pwm-core-1 ( -- )
      task::core-init-hook @ core-init-hook-saved !
      [:
        core-init-hook-saved @ execute
        disable-int
        0 pwm-irq NVIC_IPR_IP!
        pwm-irq NVIC_ISER_SETENA!
        enable-int
      ;] task::core-init-hook !
    ;

    \ Initialize PWM
    : init-pwm ( -- )
      disable-int
      [: $F INTR ! pwm-irq NVIC_ICPR_CLRPEND! ;] pwm-vector vector!
      0 pwm-irq NVIC_IPR_IP!
      pwm-irq NVIC_ISER_SETENA!
      enable-int
      init-pwm-core-1
    ;

    \ Validate a PWM slice
    : validate-pwm-index ( index -- ) 8 u< averts x-out-of-range-pwm ;

    \ Validate a PWM mask
    : validate-pwm-mask ( index -- ) $FF bic 0= averts x-out-of-range-pwm ;
    
  end-module> import

  \ Set a pin as a PWM pin
  : pwm-pin ( pin -- ) pwm-alternate swap alternate-pin ;
  
  \ Set PWM IRQ handler
  : pwm-vector! ( xt -- ) pwm-vector vector! ;

  \ Enable any set of slices, expressed as bits from 0 to 7
  : enable-pwm ( bits -- ) dup validate-pwm-mask EN bis! ;

  \ Disable any set of slices, expressed as bits from 0 to 7
  : disable-pwm ( bits -- ) dup validate-pwm-mask EN bic! ;

  \ Enable interrupts for any set of slices, expressed as bits from 0 to 7
  : enable-pwm-int ( bits -- ) dup validate-pwm-mask INTE bis! ;

  \ Disable interrupts for any set of slices, expressed as bits from 0 to 7
  : disable-pwm-int ( bits -- ) dup validate-pwm-mask INTE bic! ;

  \ Clear an interrupt for any set of slices, expressed as bits from 0 to 7
  : clear-pwm-int ( bits -- ) dup validate-pwm-mask INTR bis! ;

  \ Get the interrupt state for all slices, expressed as one bit per slice
  \ from 0 to 7
  : pwm-int@ ( -- bits ) INTS @ ;

  \ Clear pending PWM interrupt
  : clear-pwm-pending ( -- ) pwm-irq NVIC_ICPR_CLRPEND! ;
  
  \ Advance the phase of a running counter by 1 count
  : advance-pwm-phase { index -- }
    index validate-pwm-index
    index CH_CSR_PH_ADV! begin index CH_CSR_PH_ADV@ 0= until
  ;

  \ Retard the phase of a running counter by 1 count
  : retard-pwm-phase { index -- }
    index validate-pwm-index
    index CH_CSR_PH_RET! begin index CH_CSR_PH_RET@ = until
  ;

  \ Set a slice to be free-running
  : free-running-pwm ( index -- )
    dup validate-pwm-index DIVMODE_FREE_RUNNING swap CH_CSR_DIVMODE!
  ;

  \ Set a slice to be gated by the PWM B pin
  : gated-pwm ( index -- )
    dup validate-pwm-index DIVMODE_GATED swap CH_CSR_DIVMODE!
  ;

  \ Set a slice to advance with each rising eddge of the PWM B pin
  : rising-edge-pwm ( index -- )
    dup validate-pwm-index DIVMODE_RISING_EDGE swap CH_CSR_DIVMODE!
  ;

  \ Set a slice to advance with each falling edge of the PWM B pin
  : falling-edge-pwm ( index -- )
    dup validate-pwm-index DIVMODE_FALLING_EDGE swap CH_CSR_DIVMODE!
  ;

  \ Set invert PWM output B for a slice
  : pwm-invert-b! ( state index -- ) dup validate-pwm-index CH_CSR_B_INV! ;

  \ Set invert PWM output A for a slice
  : pwm-invert-a! ( state index -- ) dup validate-pwm-index CH_CSR_A_INV! ;

  \ Set phase-correct modulation for a slice
  : pwm-phase-correct! ( state index -- )
    dup validate-pwm-index CH_CSR_PH_CORRECT!
  ;

  \ Set clock frequency divisor; int is the integral portion from 1 to 255
  \ and fract is the fractional portion from 0 to 15
  : pwm-clock-div! ( fract int index -- )
    dup validate-pwm-index
    over 0 u> averts x-out-of-range-clock-div
    over 256 u< averts x-out-of-range-clock-div
    2 pick 16 u< averts x-out-of-range-clock-div
    CH_DIV!
  ;

  \ Set the PWM output B for a slice counter compare value
  : pwm-counter-compare-b! ( value index -- )
    dup validate-pwm-index
    over 65536 u< averts x-out-of-range-compare
    CH_CC_B!
  ;

  \ Set the PWM output A for a slice counter compare value
  : pwm-counter-compare-a! ( value index -- )
    dup validate-pwm-index
    over 65536 u< averts x-out-of-range-compare
    CH_CC_A!
  ;

  \ Set the slice counter wrap value
  : pwm-top! ( value index -- )
    dup validate-pwm-index
    over 65536 u< averts x-out-of-range-top
    CH_TOP !
  ;

  \ Set the slice counter value
  : pwm-counter! ( value index -- )
    dup validate-pwm-index
    over 65536 u< averts x-out-of-range-counter
    CH_CTR !
  ;

  \ Get the slice counter value
  : pwm-counter@ ( index -- value ) CH_CTR @ ;
  
end-module

\ Initialize
: init init pwm::pwm-internal::init-pwm ;

reboot
