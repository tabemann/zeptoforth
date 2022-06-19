# Analog-Digital Converter Words

Analog-digital converter (ADC) support is implemented for each of the supported platforms. A given platform may have a number of ADC peripherals. On the RP2040 and STM32F411 there is only a single ADC peripheral, numbered 0 on the RP2040 and, using STMicroelectronics' numbering, 1 on the STM32F411. On the STM32F407, STM32F746, and STM32L476 there are three ADC peripherals, numbered from 1 through 3.

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

The default ADC on a platform. On the RP2040 this is 0, on the other supported platforms it is 1.

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

#### RP2040 only words

##### `pin-adc-chan`
( pin -- channel )

Get the ADC channel for *pin*.

#### STM32F407, STM32F411, STM32F746, and STM32L476 only words

##### `vrefint-adc-chan`
( -- channel )

Channel on ADC 1 for the internal reference voltage.

##### `vbat-adc-chan`
( -- channel )

Channel on ADC 1 for Vbat.

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
