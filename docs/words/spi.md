# SPI Words

There are a number of words for controlling SPI peripherals available on each of the supported microcontrollers. These are as follows:

Note that *spi* values as mentioned below vary between the different supported platforms. The following applies:

* On the RP2040 the available *spi* values are 0 and 1.
* On the STM32F411 the available *spi* values are 1 through 5.
* On the STM32F407 the available *spi* values are 1 through 3.
* On the STM32L476 the available *spi* values are 1 through 3.
* On the STM32F746 the available *spi* values are 1 through 6.

### `spi`

These words are in the `spi` module:

#### All platforms

##### `enable-spi`
( spi -- )

Enable an SPI peripheral.

##### `disable-spi`
( spi -- )

Disable an SPI peripheral. Note that SPI peripherals are disabled by default.

##### `enable-spi-tx`
( spi -- )

Enable SPI peripheral transmit.

##### `disable-spi-tx`
( spi -- )

Disable SPI peripheral transmit.

##### `master-spi`
( spi -- )

Set an SPI peripheral to be a master. This must be done with the SPI peripheral disabled.

##### `slave-spi`
( spi -- )

Set an SPI peripheral to be a slave. This must be done with the SPI peripheral disabled.

##### `spi-baud!`
( baud spi -- )

Set the baud of a master SPI peripheral. This must be done with the SPI peripheral disabled.

##### `motorola-spi`
( sph spo spi -- )

Set the protocol of an SPI peripheral to Motorola SPI, with SPO/CPOL set to *spo* and SPH/CPHA set to *sph*. This must be done with the SPI peripheral disabled.

##### `ti-ss-spi`
( spi -- )

Set the protocol of an SPI peripheral to TI synchronous serial. This must be done with the SPI peripheral disabled.

##### `spi-data-size!`
( data-size spi -- )

Set the word size of an SPI peripheral to *data-size* bits; on the RP2040, STM32L476, and STM32F746 any value from 4 to 16 is permitted, whereas on the STM32F407 and STM32F411 only 8 and 16 are permitted. This must be done with the SPI peripheral disabled.

##### `>spi`
( c spi -- )

Emit a word to an SPI peripheral.

##### `spi>`
( spi -- c )

Read a word from an SPI peripheral.

##### `buffer>spi`
( addr bytes spi -- )

Write a buffer of data to an SPI peripheral as a master, discarding the data returned by the slave.

##### `spi>buffer`
( add bytes filler spi -- )

Read a buffer of data from an SPI peripheral as a master, transmitting a filler for each unit of data sent to the slave.

##### `>spi?`
( spi -- flag )

Get whether an SPI peripheral is ready to emit a word.

##### `spi>?`
( spi -- flag )

Get whether a word is ready to be read from an SPI peripheral.

##### `drain-spi`
( spi -- )

Empty an SPI peripheral's receive buffer.

##### `flush-spi`
( spi -- )

Flush an SPI peripheral's transmit buffer.

##### `spi-alternate`
( spi -- )

The alternate function for a given SPI peripheral. Note that using `spi-pin` is recommended as some SPI peripherals may have multiple alternate functions in practice depending on the particular pin.

##### `spi-pin`
( spi pin -- )

Set the alternate function for *pin* to configure it for *spi*.

##### `spi-rx-handler!`
( xt spi -- )

Set an SPI RX handler called when the SPI interrupt is called for RX data. Note that this may not be called if `spi>` is called before the SPI interrupt has the chance to be invoked. A value of 0 indicates no handler.

##### `x-invalid-spi`
( -- )

Invalid SPI peripheral exception.

##### `x-invalid-spi-clock`
( -- )

Invalid SPI clock setting exception.

##### `x-invalid-spi-data-size`
( -- )

Invalid SPI data size setting exception.

#### RP2040 only

##### `enable-spi-loopback`
( spi -- )

Enable SPI loopback mode for an SPI peripheral.

##### `disable-spi-loopback`
( spi -- )

Disable SPI loopback mdoe for an SPI peripheral.

##### `natl-microwire-spi`
( spi -- )

Set the protocol of the SPI peripheral to National Semiconductor Microwire.

#### STM32F407, STM32F411, STM32L476, and STM32F746 only

##### `msb-first-spi`
( spi -- )

Set an SPI peripheral to have MSB-first data. This must be done with the SPI peripheral disabled.

##### `lsb-first-spi`
( spi -- )

Set an SPI peripheral to have LSB-first data. This must be done with the SPI peripheral disabled.

##### `enable-spi-ssm`
( spi -- )

Enable SPI software slave management by master SPI peripherals. This must be done with the SPI peripheral disabled.

##### `disable-spi-ssm`
( spi -- )

Disable SPI software slave management by master SPI peripherals. This must be done with the SPI peripheral disabled.

##### `spi-ssm!`
( ssi spi -- )

Set the software slave management selection for master SPI peripherals.

##### `1-line-spi`
( spi -- )

Set an SPI peripheral to one-wire mode.

##### `2-line-spi`
( spi -- )

Set an SPI peripheral to two-wire mode (the default).

##### `1-line-spi-in`
( spi -- )

Set the single wire in one-wire mode to be an input.

##### `1-line-spi-out`
( spi -- )

Set the single wire in one-wire mode to be an output.

### RP2040-only words

##### `buffer>spi-raw-dma`
( addr bytes dma1 dma0 spi -- last-data )

Write a buffer of data to an SPI peripheral as a master, discarding the data returned by the slave. This word uses the DMA channels *dma0* and *dma1*, and returns the last data read from the SPI peripheral.

##### `spi>buffer-raw-dma`
( add bytes filler dma1 dma0 spi -- )

Read a buffer of data from an SPI peripheral as a master, transmitting a filler for unit of data sent to the slave. This word uses the DMA channels *dma0* and *dma1*.
