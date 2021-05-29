# Quad SPI words

The Quad SPI interface is written for the STM32F746 DISCOVERY board. It maps Quad SPI flash to the addressing space at `qspi-base`. It also provides means for writing bytes, halfwords, words, and arbitrary buffers containing bytes to flash and erasing 4K and 32K subsectors, 64K sectors, and the entire flash. Note that even though internally they require turning off memory-mapped Quad SPI mode, it is automatically turned back on when they are complete, and they are hidden from all other tasks (however they significantly impact realtime performance due to being slow while requiring interrupts to be turned off).

The Quad SPI interface is not included in the default builds; the user must load `src/stm32f746/forth/qspi.fs` or use an STM32F746 big build for it to be available. Note that logic is in place to ensure that it is not loaded multiple times. When it is loaded it reboots the MCU to carry out initialization.

The following words are in `qspi-module`:

##### `qspi-base`
( -- addr )

Returns the base address of the Quad SPI memory mapped space.

##### `qspi-size`
( -- bytes )

Returns the size of the Quad SPI flash space in bytes.

##### `qspi-inited?`
( -- flag )

Returns whether Quad SPI is initialized - should always be true after system initialization.

##### `map-qspi-enabled?`
( -- flag )

Returns whether mapping Quad SPI to memory is enabled - should always be true after system initialization.

##### `x-invalid-qspi-addr`
( -- )

Invalid Quad SPI address exception.

##### `qspi!`
( x addr -- )

Writes a 32-bit value in little-endian order to Quad SPI flash at the address corresponding to the specified address in memory-mapped Quad SPI space; note that it has to be 32-bit aligned.

##### `hqspi!`
( h addr -- )

Writes a 16-bit value in little-endian order to Quad SPI flash at the address corresponding to the specified address in memory-mapped Quad SPI space; note that it has to be 16-bit aligned.

##### `bqspi!`
( b addr -- )

Writes a 8-bit value to Quad SPI flash at the address corresponding to the specified address in memory-mapped Quad SPI space; note that it has to be 16-bit aligned.

##### `mass-qspi!`
( data-addr bytes addr -- )

Writes an arbitrary number of bytes in the provided buffer to Quad SPIs starting at the specified address in memory-mapped Quad SPI space; note the data can be larger than a write page (and can therefore cross write page boundaries) despite the underlying hardware not allowing this due to this case being handled in sofotware.

##### `erase-qspi-4k-subsector`
( addr -- )

Erase a 4K flash subsector containing the specified address in memory-mapped Quad SPI space.

##### `erase-qspi-32k-subsector`
( addr -- )

Erase a 32K flash subsector containing the specified address in memory-mapped Quad SPI space.

##### `erase-qspi-sector`
( addr -- )

Erase a 64K flash sector containing the specified address in memory-mapped Quad SPI space.

##### `erase-qspi-bulk`
( -- )

Erase the entire contents of Quad SPI flash.
