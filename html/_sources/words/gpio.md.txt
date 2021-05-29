# GPIO Words

A GPIO API is provided for STM32 microcontrollers, with slight variations between individual microcontrollers (primarily in the number of supported GPIO's, along with internal differences in the registers used by the API). It provides a thin layer over the hardware registers that control GPIO's, so as to make them more user-friendly while not limiting the programmer's capabilities.

The GPIO API is defined in `src/*/forth/gpio.fs`, where `*` is any platform. Note that it is included in all the prebuilt non-kernel-only images.

These words are in `gpio-module`.

##### `GPIOA` through `GPIOK`

These are the memory addresses of the spaces for each of the the GPIO's, from GPIOA to GPIOI on the STM32F407 and STM32L476 and from GPIOA to GPIOK on the STM32F746.

##### `MODER`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_MODER address.

##### `OTYPER`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_OTYPER address.

##### `OSPEEDR`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_OSPEEDR address.

##### `PUPDR`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_PUPDR address.

##### `IDR`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_IDR address.

##### `ODR`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_ODR address.

##### `BSRR`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_BSRR address.

##### `LCKR`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_LCKR address.

##### `AFRL`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_AFRL address.

##### `AFRH`
( gpio-addr -- addr )

This takes a base GPIOx address and outputs a GPIOx_AFRH address.

##### `INPUT_MODE`

This is the input port mode.

##### `OUTPUT_MODE`

This is the output port mode.

##### `ALTERNATE_MODE`

This is the alternate function port mode.

##### `ANALOG_MODE`

This is the analog port mode.

##### `PUSH_PULL`

This is the push-pull output type.

##### `OPEN_DRAIN`

This is the open drain output type.

##### `LOW_SPEED`

This is the low speed output speed.

##### `MEDIUM_SPEED`

This is the medium speed output speed.

##### `HIGH_SPEED`

This is the high speed output speed.

##### `VERY_HIGH_SPEED`

This is the very high speed output speed.

##### `NO_PULL_UP_PULL_DOWN`

This is the no pull up/pull down setting.

##### `PULL_UP`

This is the pull up setting.

##### `PULL_DOWN`

This is the pull down setting.

##### `gpio-clock-enable`
( gpio-addr -- )

Enable the clock for the specified GPIO.

##### `gpio-lp-clock-enable`
( gpio-addr -- )

Enable the low-power clock for the specified GPIO.

##### `gpio-clock-disable`
( gpio-addr -- )

Disable the clock for the specified GPIO.

##### `gpio-lp-clock-disable`
( gpio-addr -- )

Disable the low-power clock for the specified GPIO.

##### `gpio-clock-enable?`
( gpio-addr -- enable )

Get whether the clock for the specified GPIO is enabled.

##### `gpio-lp-clock-enable?`
( gpio-addr -- enable )

Get whether the low-power clock for the specified GPIO is enabled.

##### `MODER!`
( mode pin gpio-addr -- )

Set the specified mode for the specified pin (from 0 to 15) on the specified GPIO.

##### `OTYPER!`
( otype pin gpio-addr -- )

Set the specified output type for the specified pin (from 0 to 15) on the specified GPIO.

##### `OSPEEDR!`
( ospeed pin gpio-addr -- )

Set the specified output speed for the specified pin (from 0 to 15) on the specified GPIO.

##### `PUPDR!`
( pupd pin gpio-addr -- )

Set the specified pull up/pull down setting for the specified pin (from 0 to 15) on the specified GPIO.

##### `AFRL!`
( af pin gpio-addr -- )

Set the specified alternate function for the specified pin (from 0 to 7) on the specified GPIO.

##### `AFRH!`
( af pin gpio-addr -- )

Set the specified alternate function for the specified pin (from 8 to 15) on the specified GPIO.

##### `AFR!`
( af pin gpio-addr -- )

Set the specified alternate function for the specified pin (from 0 to 15) on the specified GPIO.

##### `BS!`
( pin gpio-addr -- )

Set the output for the specified pin (from 0 to 15) on the specified GPIO to on.

##### `BR!`
( pin gpio-addr -- )

Set the output for the specified pin (from 0 to 15) on the specified GPIO to off.

##### `BSRR!`
( output pin gpio-addr -- )

Set the output for the specified pin (from 0 to 15) on the specified GPIO to on or off.

##### `MODER@`
( pin gpio-addr -- mode )

Get the mode of the specified pin (from 0 to 15) on the specified GPIO.

##### `OTYPER@`
( pin gpio-addr -- otype )

Get the output type of the specified pin (from 0 to 15) on the specified GPIO.

##### `OSPEEDR@`
( pin gpio-addr -- ospeed )

Get the output speed of the specified pin (from 0 to 15) on the specified GPIO.

##### `PUPDR@`
( pin gpio-addr -- pupd )

Get the pull up/pull down setting of the specified pin (from 0 to 15) on the specified GPIO.

##### `AFRL@`
( pin gpio-addr -- af )

Get the alternate function for the specified pin (from 0 to 7) on the specified GPIO.

##### `AFRH@`
( pin gpio-addr -- af )

Get the alternate function for the specified pin (from 8 to 15) on the specified GPIO.

##### `AFR@`
( pin gpio-addr -- af )

Get the alternate function for the specified pin (from 0 to 15) on the specified GPIO.

##### `IDR@`
( pin gpio-addr -- input )

Get the input for the specified pin (from 0 to 15) on the specified GPIO.
