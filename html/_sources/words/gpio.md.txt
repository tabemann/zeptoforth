# GPIO Words

A GPIO API is provided for STM32 microcontrollers, with slight variations between individual microcontrollers (primarily in the number of supported GPIO's, along with internal differences in the registers used by the API). It provides a thin layer over the hardware registers that control GPIO's, so as to make them more user-friendly while not limiting the programmer's capabilities.

A GPIO API is also provided for the RP2040; note that this API differs considerably from the GPIO API for STM32 microcontrollers and has no compatiblity with it. It exposes registers for manipulating GPIO input and output and for controlling GPIO interrupts.

These words are in `gpio-module`.

## STM32 Words

### `gpio-module`

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

## RP2040 Words

### `gpio-module`

##### `CTRL_NORMAL`

Normal control state for GPIO control

##### `CTRL_INVERT`

Inverse control state for GPIO control

##### `CTRL_FORCE_LOW`

Force low / disable control state for GPIO control

##### `CTRL_FORCE_HIGH`

Force high / enable control state for GPIO control

##### `VOLTAGE_3.3V`

Voltage set to 3.3V

##### `VOLTAGE_1.8V`

Voltage set to 1.8V

##### `DRIVE_2MA`

Drive strength set to 2mA

##### `DRIVE_4MA`

Drive strength set to 4mA

##### `DRIVE_8MA`

Drive strength set to 8mA

##### `DRIVE_12MA`

Drive strength set to 12mA

##### `PAD_SWCLK`

Pad index for SWCLK

##### `PAD_SWD`

Pad index for SWD

##### `GPIO_IN`

GPIO input register; each bit corresponds to the GPIO with an index equal to its position

##### `GPIO_OUT`

GPIO output register; each bit corresponds to the GPIO with an index equal to its position

##### `GPIO_OUT_SET`

GPIO output atomic bit-set register; each bit corresponds to the GPIO with an index equal to its position

##### `GPIO_OUT_CLR`

GPIO output atomic bit-clear register; each bit corresponds to the GPIO with an index equal to its position

##### `GPIO_OUT_XOR`

GPIO output atomic bit-xor register; each bit corresponds to the GPIO with an index equal to its position

##### `GPIO_OE`

GPIO output enable register; each bit corresponds to the GPIO with an index equal to its position

##### `GPIO_OE_SET`

GPIO output enable atomic bit-set register; each bit corresponds to the GPIO with an index equal to its position

##### `GPIO_OE_CLR`

GPIO output enable atomic bit-clear register; each bit corresponds to the GPIO with an index equal to its position

##### `GPIO_OE_XOR`

GPIO output enable atomic bit-xor register; each bit corresponds to the GPIO with an index equal to its position

##### `GPIO_STATUS_IRQTOPROC@`
( index -- flag )

Get interrupt to processors, after override is applied

##### `GPIO_STATUS_IRQFROMPAD@`
( index -- flag )

Get interrupt from pad, before override is applied

##### `GPIO_STATUS_INTOPERI@`
( index -- flag )

Get input signal to peripheral, after override is applied

##### `GPIO_STATUS_INFROMPAD@`
( index -- flag )

Get input signal from pad, before override is applied

##### `GPIO_STATUS_OETOPAD@`
( index -- flag )

Get output enable to pad, after register override is applied

  \ applied
##### `GPIO_STATUS_OEFROMPERI@`
( index -- flag )

Get output enable from selected peripheral, before register override is

##### `GPIO_STATUS_OUTTOPAD@`
( index -- flag )

Get output signal to pad, after register override is applied

  \ applied
##### `GPIO_STATUS_OUTFROMPERI@`
( index -- flag )

Get output signal from selected peripheral, before register override is

##### `GPIO_CTRL_IRQOVER!`
( control index -- )

Set interrupt state for GPIO

##### `GPIO_CTRL_INOVER!`
( control index -- )

Set peripheral input state for GPIO

##### `GPIO_CTRL_OEOVER!`
( control index -- )

Set output enable state for GPIO

##### `GPIO_CTRL_OUTOVER!`
( control index -- )

Set output state for GPIO

##### `GPIO_CTRL_FUNCSEL!`
( function index -- )

Set the function select for GPIO

##### `GPIO_CTRL_IRQOVER@`
( index -- control )

Get interrupt state for GPIO

##### `GPIO_CTRL_INOVER@`
( index -- control )

Get peripheral input state for GPIO

##### `GPIO_CTRL_OEOVER@`
( index -- control )

Get output enable state for GPIO

##### `GPIO_CTRL_OUTOVER@`
( index -- control )

Get output state for GPIO

##### `GPIO_CTRL_FUNCSEL@`
( index -- function )

Set the function select for GPIO

##### `INTR_GPIO_EDGE_LOW!`
( index -- )

Clear a raw edge low interrupt

##### `INTR_GPIO_EDGE_HIGH!`
( index -- )

Clear a raw edge high interrupt
  
##### `INTR_GPIO_LEVEL_LOW@`
( index -- state )

Get a level low raw interrupt state

##### `INTR_GPIO_LEVEL_HIGH@`
( index -- state )

Get a level high raw interrupt state

##### `INTR_GPIO_EDGE_LOW@`
( index -- state )

Get an edge low raw interrupt state

##### `INTR_GPIO_EDGE_HIGH@`
( index -- state )

Get an edge high raw interrupt state
  
##### `PROC0_INTE_GPIO_LEVEL_LOW!`
( enable index -- )

Set a level low interrupt enable for processor 0

##### `PROC0_INTE_GPIO_LEVEL_HIGH!`
( enable index -- )

Set a level high interrupt enable for processor 0

##### `PROC0_INTE_GPIO_EDGE_LOW!`
( enable index -- )

Set an edge low interrupt enable for processor 0

##### `PROC0_INTE_GPIO_EDGE_HIGH!`
( enable index -- )

Set an edge high interrupt enable for processor 0
  
##### `PROC0_INTE_GPIO_LEVEL_LOW@`
( index -- enable )

Get a level low interrupt enable for processor 0

##### `PROC0_INTE_GPIO_LEVEL_HIGH@`
( index -- enable )

Get a level high interrupt enable for processor 0

##### `PROC0_INTE_GPIO_EDGE_LOW@`
( index -- enable )

Get an edge low interrupt enable for processor 0

##### `PROC0_INTE_GPIO_EDGE_HIGH@`
( index -- enable )

Get an edge high interrupt enable for processor 0

##### `PROC0_INTF_GPIO_LEVEL_LOW!`
( force index -- )

Set a level low interrupt force for processor 0

##### `PROC0_INTF_GPIO_LEVEL_HIGH!`
( force index -- )

Set a level high interrupt force for processor 0

##### `PROC0_INTF_GPIO_EDGE_LOW!`
( force index -- )

Set an edge low interrupt force for processor 0

##### `PROC0_INTF_GPIO_EDGE_HIGH!`
( force index -- )

Set an edge high interrupt force for processor 0
  
##### `PROC0_INTF_GPIO_LEVEL_LOW@`
( index -- force )

Get a level low interrupt force for processor 0

##### `PROC0_INTF_GPIO_LEVEL_HIGH@`
( index -- force )

Get a level high interrupt force for processor 0

##### `PROC0_INTF_GPIO_EDGE_LOW@`
( index -- force )

Get an edge low interrupt force for processor 0

##### `PROC0_INTF_GPIO_EDGE_HIGH@`
( index -- force )

Get an edge high interrupt force for processor 0

##### `PROC0_INTS_GPIO_LEVEL_LOW@`
( index -- enable )

Get a level low interrupt status for processor 0

##### `PROC0_INTS_GPIO_LEVEL_HIGH@`
( index -- enable )

Get a level high interrupt status for processor 0

##### `PROC0_INTS_GPIO_EDGE_LOW@`
( index -- enable )

Get an edge low interrupt status for processor 0

##### `PROC0_INTS_GPIO_EDGE_HIGH@`
( index -- enable )

Get an edge high interrupt status for processor 0

##### `PADS_BANK0_VOLTAGE_SELECT!`
( voltage -- )

Select voltage for pads

##### `PADS_BANK0_VOLTAGE_SELECT@`
( -- voltage )

Get voltage for pads

##### `PADS_BANK0_OD!`
( disable index -- )

Set output disable

##### `PADS_BANK0_IE!`
( enable index -- )

Set input enable

##### `PADS_BANK0_DRIVE!`
( strength index -- )

Set drive strength

##### `PADS_BANK0_PUE!`
( enable index -- )

Set pull up enable

##### `PADS_BANK0_PDE!`
( enable index -- )

Set pull down enable

##### `PADS_BANK0_SCHMITT!`
( enable index -- )

Set schmitt trigger

##### `PADS_BANK0_SLEWFAST!`
( fast index -- )

Set slew rate control  
