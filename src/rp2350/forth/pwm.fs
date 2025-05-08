\ Copyright (c) 2023-2024 Travis Bemann
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
  : x-out-of-range-pwm ." out of range PWM slice" cr ;

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
    $400A8000 constant PWM_BASE

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
    PWM_BASE $0F0 + constant EN

    \ PWM raw interrupts register
    PWM_BASE $0F4 + constant INTR

    \ PWM interrupt 0 enable register
    PWM_BASE $0F8 + constant IRQ0_INTE

    \ PWM interrupt 0 force register
    PWM_BASE $0FC + constant IRQ0_INTF

    \ PWM interrupt 0 status after masking and forcing
    PWM_BASE $100 + constant IRQ0_INTS
    
    \ PWM interrupt 1 enable register
    PWM_BASE $104 + constant IRQ1_INTE

    \ PWM interrupt 1 force register
    PWM_BASE $108 + constant IRQ1_INTF

    \ PWM interrupt 1 status after masking and forcing
    PWM_BASE $10C + constant IRQ1_INTS

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

    \ PWM wrap IRQ 0
    8 constant pwm-wrap-0-irq

    \ PWM wrap IRQ 1
    9 constant pwm-wrap-1-irq

    \ PWM wrap vector 0
    pwm-wrap-0-irq 16 + constant pwm-wrap-0-vector

    \ PWM wrap vector 1
    pwm-wrap-1-irq 16 + constant pwm-wrap-1-vector

    \ Initialize PWM
    : init-pwm ( -- )
      disable-int
      [: $F INTR ! pwm-wrap-0-irq NVIC_ICPR_CLRPEND! ;]
      pwm-wrap-0-vector vector!
      0 pwm-wrap-0-irq NVIC_IPR_IP!
      pwm-wrap-0-irq NVIC_ISER_SETENA!
      [: $F INTR ! pwm-wrap-1-irq NVIC_ICPR_CLRPEND! ;]
      pwm-wrap-1-vector vector!
      0 pwm-wrap-1-irq NVIC_IPR_IP!
      pwm-wrap-1-irq NVIC_ISER_SETENA!
      enable-int
    ;

    \ Validate a PWM slice
    : validate-pwm-index ( index -- ) 12 u< averts x-out-of-range-pwm ;

    \ Validate a PWM mask
    : validate-pwm-mask ( index -- ) $FFF bic 0= averts x-out-of-range-pwm ;
    
  end-module> import

  \ Set a pin as a PWM pin
  : pwm-pin ( pin -- ) pwm-alternate swap alternate-pin ;
  
  \ Set PWM wrap 0 IRQ handler
  : pwm-wrap-0-vector! ( xt -- ) pwm-wrap-0-vector vector! ;

  \ Set PWM wrap 1 IRQ handler
  : pwm-wrap-1-vector! ( xt -- ) pwm-wrap-1-vector vector! ;

  \ Set PWM IRQ handler (legacy)
  : pwm-vector! ( xt -- ) pwm-wrap-0-vector! ;

  \ Enable any set of slices, expressed as bits from 0 to 11
  : enable-pwm ( bits -- ) dup validate-pwm-mask EN bis! ;

  \ Disable any set of slices, expressed as bits from 0 to 11
  : disable-pwm ( bits -- ) dup validate-pwm-mask EN bic! ;

  \ Enable interrupts for PWM_IRQ_WRAP_0 for any set of slices, expressed as
  \ bits from 0 to 11
  : enable-pwm-wrap-0-int ( bits -- ) dup validate-pwm-mask IRQ0_INTE bis! ;

  \ Disable interrupts for PWM_IRQ_WRAP_0 for any set of slices, expressed as
  \ bits from 0 to 11
  : disable-pwm-wrap-0-int ( bits -- ) dup validate-pwm-mask IRQ0_INTE bic! ;

  \ Clear an interrupt for any set of slices, expressed as
  \ bits from 0 to 11
  : clear-pwm-wrap-int ( bits -- ) dup validate-pwm-mask INTR bis! ;

  \ Get the interrupt state for PWM_IRQ_WRAP_0 for all slices, expressed as one
  \ bit per slice from 0 to 11
  : pwm-wrap-0-int@ ( -- bits ) IRQ0_INTS @ ;

  \ Clear pending PWM interrupt for PWM_IRQ_WRAP_0
  : clear-pwm-wrap-0-pending ( -- ) pwm-wrap-0-irq NVIC_ICPR_CLRPEND! ;
  
  \ Enable interrupts for PWM_IRQ_WRAP_1 for any set of slices, expressed as
  \ bits from 0 to 11
  : enable-pwm-wrap-1-int ( bits -- ) dup validate-pwm-mask IRQ1_INTE bis! ;

  \ Disable interrupts for PWM_IRQ_WRAP_1 for any set of slices, expressed as
  \ bits from 0 to 11
  : disable-pwm-wrap-1-int ( bits -- ) dup validate-pwm-mask IRQ1_INTE bic! ;

  \ Get the interrupt state for PWM_IRQ_WRAP_1 for all slices, expressed as one
  \ bit per slice from 0 to 11
  : pwm-wrap-1-int@ ( -- bits ) IRQ1_INTS @ ;

  \ Clear pending PWM interrupt for PWM_IRQ_WRAP_1
  : clear-pwm-wrap-1-pending ( -- ) pwm-wrap-1-irq NVIC_ICPR_CLRPEND! ;

  \ Enable interrupts for any set of slices, expressed as bits from 0 to 11;
  \ note that this is a legacy word that applies to PWM_IRQ_WRAP_0
  : enable-pwm-int ( bits -- ) enable-pwm-wrap-0-int ;

  \ Disable interrupts for any set of slices, expressed as bits from 0 to 11;
  \ note that this is a legacy word that applies to PWM_IRQ_WRAP_0
  : disable-pwm-int ( bits -- ) disable-pwm-wrap-0-int ;

  \ Clear an interrupt for any set of slices, expressed as bits from 0 to 11;
  \ note that this is a legacy word that applies to PWM_IRQ_WRAP_0
  : clear-pwm-int ( bits -- ) clear-pwm-wrap-int ;

  \ Get the interrupt state for all slices, expressed as one bit per slice
  \ from 0 to 11; note that this is a legacy word that applies to PWM_IRQ_WRAP_0
  : pwm-int@ ( -- bits ) pwm-wrap-0-int@ ;

  \ Clear pending PWM interrupt; note that this is a legacy word that applies to
  \ PWM_IRQ_WRAP_0
  : clear-pwm-pending ( -- ) clear-pwm-wrap-0-pending ;
  
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
