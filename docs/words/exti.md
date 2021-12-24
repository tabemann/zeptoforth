# EXTI Words

An EXTI API is provided for STM32 microcontrollers, with slight variations between individual microcontrollers (primarily in the number of supported GPIO's, along with internal differences in the registers used by the API). It provides a thin layer over the hardware registers that control EXTI, so as to make them more user-friendly while not limiting the programmer's capabilities.

### `exti`

These words are in `exti`.

##### `EXTI_0` through `EXTI_4`

These are the interrupt numbers for the EXTI line 0 through 4 interrupts.

##### `EXTI_9_5`

This is the interrupt number for the EXTI line 5 through 9 interupt.

##### `EXTI_15_10`

This is the interrupt number for the EXTI line 10 through 15 interrupt.

##### `PA` through `PK`

These are the indices of the ports for each of the GPIO's, from `PA` to `PI` on the STM32F407 and STM32L476 and `PA` to `PK` on the STM32F746.

##### `syscfg-clock-enable`
( -- )

Enable the SYSCFG peripheral clock.

##### `syscfg-lp-clock-enable`
( -- )

Enable the low-power SYSCFG peripheral clock

##### `syscfg-clock-disable`
( -- )

Disable the SYSCFG peripheral clock.

##### `syscfg-lp-clock-disable`
( -- )

Disable the low-power SYSCFG peripheral clock

##### `syscfg-clock-enable?`
( -- enable )

Get whether the SYSCFG peripheral clock is enabled

##### `syscfg-lp-clock-enable?`
( -- enable )

Get whether the low-power SYSCFG peripheral clock is enabled

##### `SYSCFG_EXTICRx!`
( port exti -- )

Set a given EXTI line (from 0 to 15) to map to a specified port.

##### `EXTI_IMR!`
( mask exti -- )

Set the interrupt mask for a given EXTI line (from 0 to 15).

##### `EXTI_EMR!`
( mask exti -- )

Set the event mask for a given EXTI line (from 0 to 15).

##### `EXTI_RTSR!`
( setting exti -- )

Set whether a given EXTI line (from 0 to 15) triggers on a rising edge.

##### `EXTI_FTSR!`
( setting exti -- )

Set whether a given EXTI line (from 0 to 15) triggers on a falling edge.

##### `EXTI_SWIER!`
( exti -- )

Trigger a software interrupt/event for a given EXTI line (from 0 to 15).

##### `EXTI_PR!`
( exti -- )

Clear a pending state for a given EXTI line (from 0 to 15).

##### `SYSCFG_EXTICRx@`
( exti -- port )

Get the mapping of a given EXTI line (from 0 to 15) to a port.

##### `EXTI_IMR@`
( exti -- mask )

Get the interrupt mask for a given EXTI line (from 0 to 15).

##### `EXTI_EMR@`
( exti -- mask )

Get the event mask for a given EXTI line (from 0 to 15).

##### `EXTI_RTSR@`
( exti -- setting )

Get whether a given EXTI line (from 0 to 15) triggers on a rising edge.

##### `EXTI_FTSR@`
( exti -- setting )

Get whether a given EXTI line (from 0 to 15) triggers on a falling edge.

##### `EXTI_SWIER@`
( exti -- state )

Get whether a software interrupt/event has been set for a given EXTI line (from 0 to 15).

##### `EXTI_PR@`
( exti -- state )

Get whether there is a pending state for a given EXTI line (from 0 to 15).

