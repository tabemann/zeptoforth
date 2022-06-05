# Pin Words

Pins provide an abstraction layer over GPIO's to greatly simplify their API interface and to unify them as far as possible across the supported platforms

### `pin`

The following words are in the `pin` module:

#### Common Pin Words

##### `low`
( -- state )

The low pin state.

##### `high`
( -- state )

The high pin state.

##### `x-pin-out-of-range`
( -- )

The pin out of range exception.

##### `x-alternate-out-of-range`
( -- )

The alternate function out of range exception.

##### `input-pin`
( pin -- )

Set *pin* to an input state.

##### `output-pin`
( pin -- )

Set *pin* to an output state.

##### `alternate-pin`
( function pin -- )

Set *pin* to alternate function *function*.

##### `pull-up-pin`
( pin -- )

Set *pin* to be pull-up.

##### `pull-down-pin`
( pin -- )

Set *pin* to be pull-down.

##### `floating-pin`
( pin -- )

Set *pin* to be floating.

##### `slow-pin`
( pin -- )

Set *pin* to have a slow data rate.

##### `fast-pin`
( pin -- )

Set *pin* to have a fast data rate.

##### `pin!`
( state pin -- )

Set *pin* to output the high/low state *state*.

##### `pin@`
( pin -- state )

Get the input high/low state of *pin*.

##### `pin-out@`
( pin -- state )

Get the output high/low state of *pin*.

##### `toggle-pin`
( pin -- )

Toggle the high/low state of *pin*.

#### RP2040 Notes

On the RP2040, a pin is simply an integer from 0 to 29 corresponding to GPIO0 through GPIO29.

#### STM32F407, STM32L476, STM32F746, and STM32F411 Pin Words

On the STM32F407, STM32L476, STM32F746, or STM32F411, a pin is a value that combines a pin on a GPIO with a GPIO which is constructed on the STM32F407 or STM32L476 with `XA` through `XI`, on the STM32F746 with `XA` through `XK`, or on the STM32F411 with `XA` through `XE` or with `XH`.

##### `x-gpio-out-of-range`
( -- )

GPIO out of range exception.

#### STM32F407 and STM32L476 Pin Words

##### `XA` through `XI`
( index -- pin )

Take a pin index on a GPIO and return a pin on GPIOA through GPIOI.

#### STM32F746 Pin Words

##### `XA` through `XK`
( index -- pin )

Take a pin index on a GPIO and return a pin on GPIOA through GPIOK.

##### `XA` through `XE` and `XH`
( index -- pin )

Take a pin index on a GPIO and return a pin on GPIOA through GPIOE and GPIOH.
