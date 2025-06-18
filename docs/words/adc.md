# Analog-Digital Converter Words

Analog-digital converter (ADC) support is implemented for each of the supported platforms. A given platform may have a number of ADC peripherals. On the RP2040, RP2350, and STM32F411 there is only a single ADC peripheral, numbered 0 on the RP2040 and RP2350 and, using STMicroelectronics' numbering, 1 on the STM32F411. On the STM32F407, STM32F746, and STM32L476 there are three ADC peripherals, numbered from 1 through 3.

### `adc`

#### All Platforms

##### `adc@`
( channel adc -- value )

Read *channel* on *adc*. Note that this is a blocking operation, and will also prevent use of *adc* by any other task until this operation completes.

##### `enable-adc`
( adc -- )

Enable *adc*. Note that this is a blocking operation, and will also prevent use of *adc* by any other task until this operation completes.

##### `disable-adc`
( adc -- )

Disable *adc*. Note that this is a blocking operation, and will also prevent use of *adc* by any other task until this operation completes.

##### `adc-pin`
( adc pin -- )

Configure *pin* for use with *adc*.

##### `default-adc`
( -- adc )

The default ADC on a platform. On the RP2040 and RP2350 this is 0, on the other supported platforms it is 1.

##### `temp-adc-chan`
( -- channel )

The ADC channel, on the default ADC, for the microcontroller's temperature sensor

##### `adc-min`
( -- Value )

The minimum value returned by `adc@`

##### `adc-max`
( -- value )

The maximum value returned by `adc@`

##### `x-invalid-adc`
( -- )

The invalid ADC exception.

##### `x-invalid-adc-chan`
( -- )

The invalid ADC channel exception.

#### RP2040 and RP2350 only words

##### `pin-adc-chan`
( pin -- channel )

Get the ADC channel for *pin*.

#### STM32F407, STM32F411, STM32F746, and STM32L476 only words

##### `adc-sampling-time!`
( sampling-time channel adc -- )

Set *sampling-time* for *channel* on *adc*. Note that this value is rounded up to the next valid sampling time for the microcontroller in question, and on the STM32L476 an additional 0.5 is added to the value. The maximum sampling time for STM32F407, STM32F411, and STM32F746 microcontrollers is 480, and the maximum sampling time for STM32L476 microcontrollers is 640 (before the 0.5 is added). For more information see the microcontroller's reference manual. Also note that without sufficient sampling time, some channels, such as the temperature sensor, may provide erroneous data.

##### `vrefint-adc-chan`
( -- channel )

Channel on ADC 1 for the internal reference voltage.

##### `vbat-adc-chan`
( -- channel )

Channel on ADC 1 for Vbat.

##### `x-out-of-range-sampling-time`
( -- )

Out of range sampling time exception.

#### STM32F411, STM32F746, and STM32L476 only words

##### `enable-vbat`
( -- )

Enable the Vbat ADC channel.

##### `disable-vbat`
( -- )

Disable the Vbat ADC channel.

#### STM32F411 and STM32F746 only words

##### `enable-tsvref`
( -- )

Enable the temperature sensor and Vrefint ADC channels.

##### `disable-tsvref`
( -- )

Disable the temperature sensor and Vrefint ADC channels.

#### STM32L476 only words

##### `enable-vsense`
( -- )

Enable the temperature sensor ADC channel.

##### `disable-vesnse`
( -- )

Disable the temperature sensor ADC channel.

##### `enable-vrefint`
( -- )

Enable the Vrefint ADC channel.

##### `disable-vrefint`
( -- )

Disable the Vrefint ADC channel.
