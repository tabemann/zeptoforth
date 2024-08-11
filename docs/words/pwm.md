# Pulse Width Modulation Words

The RP2040 (e.g. the Raspberry Pi Pico) and the RP2350 (e.g. the Raspberry Pi Pico 2) have support for Pulse Width Modulation (PWM for short), which includes a counter capability. There are eight PWM "slices" on the RP2040 and twelve PWM "slices" on the RP2350, each of which have pair of GPIO pins as outputs, pins A and B, and pin B can also be used as an input. Also, each of the PWM slices, aside from PWM slice 7, maps to four separate GPIO pins, two of which are A pins and two of which are B pins (PWM slice 7 only maps to one A pin and one B pin on the RP2040).

Each PWM slice is associated with a counter value and a top value at which the counter wraps back to zero. Also, the A and B outputs each have a compare value at which their output transitions from high to low. Note that PWM slices have a "phase correct" mode which modifies this functionality so that the phase of a PWM output does not change with changes to its compare value.

PWM slices have a clock divider which divides the system clock by an integral value, from 1 to 255, plus a fractional value, of 0 to 15 divided by 16.

PWM slices have four different primary modes. One is free-running mode, where the counter increases by one each cycle. Another is gating mode, where the counter increases by one each cycle the B input of the PWM slice is high. Another is risng-edge mode, where the counter increases by one for each rising edge detected on the B input of the PWM slice. And last but not least, there is falling-edge mode, where the counter increase by one for each falling edge detected on the B input of the PWM slice.

When a PWM slice's counter reaches its top value, an interrupt can be raised. All PWM slices share the same IRQ. They have separate flags indicating their interrupt status, which can be cleared independent of one another, and interrupts can be enabled and disabled independently of each of them.

Last but not least, PWM slices' A and B outputs can be inverted independent of one another.

### `pwm`

The following words are in the `pwm` module:

##### `x-out-of-range-pwm`
( -- )

Out of range PWM index exception.

##### `x-out-of-range-clock-div`
( -- )

Out of range clock divisor exception.

##### `x-out-of-range-counter`
( -- )

Out of range counter value exception.

##### `x-out-of-range-compare`
( -- )

Out of range compare value exception.

##### `x-out-of-range-top`
( -- )

Out of range top value exception.

##### `pwm-pin`
( pin -- )

Set a pin as a PWM pin.
  
##### `pwm-vector!`
( xt -- )

Set PWM IRQ handler. On the `rp2350` this is for PWM_IRQ_WRAP_0.

##### `pwm-wrap-0-vector!`
( xt -- )

Set PWM IRQ handler for PWM_IRQ_WRAP_0 (`rp2350` only).

##### `pwm-wrap-1-vector!`
( xt -- )

Set PWM IRQ handler for PWM_IRQ_WRAP_0 (`rp2350` only).

##### `enable-pwm`
( bits -- )

Enable any set of slices, expressed as bits from 0 to 7 on the RP2040, and 0 to 11 on the RP2350.

##### `disable-pwm`
( bits -- )

Disable any set of slices, expressed as bits from 0 to 7 on the RP2040, and 0 to 11 on the RP2350.

##### `enable-pwm-int`
( bits -- )

Enable interrupts for any set of slices, expressed as bits from 0 to 7 on the RP2040, and 0 to 11 on the RP2350.

##### `disable-pwm-int`
( bits -- )

Disable interrupts for any set of slices, expressed as bits from 0 to 7 on the RP2040, and 0 to 11 on the RP2350.

##### `clear-pwm-int`
( bits -- )

Clear an interrupt for any set of slices, expressed as bits from 0 to 7 on the RP2040, and 0 to 11 on the RP2350.

##### `pwm-int@`
( -- bits )

Get the interrupt state for all slices, expressed as one bit per slice from 0 to 7 on the RP2040, and 0 to 11 on the RP2350.

##### `clear-pwm-pending`
( -- )

Clear pending PWM interrupt.

##### `enable-pwm-wrap-0-int`
( bits -- )

Enable interrupts for PWM_IRQ_WRAP_0 for any set of slices, expressed as bits from 0 to 11 (`rp2350` only).

##### `disable-pwm-wrap-0-int`
( bits -- )

Disable interrupts for PWM_IRQ_WRAP_0 for any set of slices, expressed as bits from 0 to 11 (`rp2350` only).

##### `clear-pwm-wrap-0-int`
( bits -- )

Clear an interrupt for PWM_IRQ_WRAP_0 for any set of slices, expressed as bits from 0 to 11 (`rp2350` only).

##### `pwm-wrap-0-int@`
( -- bits )

Get the interrupt state for PWM_IRQ_WRAP_0 for all slices, expressed as one bit per slice from 0 to 11 (`rp2350` only).

##### `clear-pwm-wrap-0-pending`
( -- )

Clear pending PWM interrupt for PWM_IRQ_WRAP_0 (`rp2350` only).

##### `enable-pwm-wrap-1-int`
( bits -- )

Enable interrupts for PWM_IRQ_WRAP_1 for any set of slices, expressed as bits from 0 to 11 (`rp2350` only).

##### `disable-pwm-wrap-1-int`
( bits -- )

Disable interrupts for PWM_IRQ_WRAP_1 for any set of slices, expressed as bits from 0 to 11 (`rp2350` only).

##### `clear-pwm-wrap-1-int`
( bits -- )

Clear an interrupt for PWM_IRQ_WRAP_1 for any set of slices, expressed as bits from 0 to 11 (`rp2350` only).

##### `pwm-wrap-1-int@`
( -- bits )

Get the interrupt state for PWM_IRQ_WRAP_1 for all slices, expressed as one bit per slice from 0 to 11 (`rp2350` only).

##### `clear-pwm-wrap-1-pending`
( -- )

Clear pending PWM interrupt for PWM_IRQ_WRAP_1 (`rp2350` only).

##### `advance-pwm-phase`
( index -- )

Advance the phase of a running counter by 1 count.

##### `retard-pwm-phase`
( index -- )

Retard the phase of a running counter by 1 count.

##### `free-running-pwm`
( index -- )

Set a slice to be free-running.

##### `gated-pwm`
( index -- )

Set a slice to be gated by the PWM B pin.

##### `rising-edge-pwm`
( index -- )

Set a slice to advance with each rising eddge of the PWM B pin.

##### `falling-edge-pwm`
( index -- )

Set a slice to advance with each falling edge of the PWM B pin.

##### `pwm-invert-b!`
( state index -- )

Set invert PWM output B for a slice.

##### `pwm-invert-a!`
( state index -- )

Set invert PWM output A for a slice.

##### `pwm-phase-correct!`
( state index -- )

Set phase-correct modulation for a slice.

##### `pwm-clock-div!`
( fract int index -- )

Set clock frequency divisor; int is the integral portion from 1 to 255 and fract is the fractional portion from 0 to 15.

##### `pwm-counter-compare-b!`
( value index -- )

Set the PWM output B for a slice counter compare value.

##### `pwm-counter-compare-a!`
( value index -- )

Set the PWM output A for a slice counter compare value.

##### `pwm-top!`
( value index -- )

Set the slice counter wrap value.

##### `pwm-counter!`
( value index -- )

Set the slice counter value.

##### `pwm-counter@`
( index -- value )

Get the slice counter value.
