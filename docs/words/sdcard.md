# SDHC/SDXC Card Support

zeptoforth includes SDHC/SDXC card support using the SPI interface for the supported and a separate chip-select pin independent of the SPI interface. It is built upon zeptoforth's object system, and involves an abstract block device `<block-dev>` in the `block-dev` module from which the `<sd>` class in the `sd` module inherits. It caches up to eight blocks in RAM at any given time, and has support for both write-through and non-write-through modes of operation, with the latter providing better performance at the expense of potential loss of data integrity in the event of unexpected resets or power loss. Note that the SPI pins used need to be configured appropriately and the chip-select pin must be configured as an output pin.

### `block-dev`

The `block-dev` module contains the following words:

##### `x-block-out-of-range`
( -- )

Block out of range exception.

##### `<block-dev>`
( -- class )

The block device class. Note that this class is not meant to be directly instantiated, and if any methods on it are called other than `new` or `destroy`, `abstract-method` will be called.

The `<block-dev>` class has no constructor.

The `<block-dev>` class includes the following methods:

##### `block-size`
( dev -- bytes )

Get block size.
    
##### `block-count`
( dev -- blocks )

Get block count.

##### `block!`
( c-addr u block-index dev -- )

Write block.
    
##### `block-part!`
( c-addr u offset block-index dev -- )

Write part of a block.

##### `block@`
( c-addr u block-index dev -- )

Read block.
    
##### `block-part@`
( c-addr u offset block-index dev -- )

Read part of a block.

##### `flush-blocks`
( dev -- )

Flush blocks.
    
##### `clear-blocks`
( dev -- )

Clear cached blocks.
    
##### `write-through!`
( write-through dev -- )

Set write-through cache mode.
    
##### `write-through@`
( dev -- write-through )

Get write-through cache mode.

### `sd`

The `sd` module contains the following words:

##### `x-sd-timeout`
( -- )

SD Card timeout.

##### `x-sd-init-error`
( -- )

SD Card init error.
  
##### `x-sd-read-error`
( -- )

SD Card read error.

##### `x-sd-write-error`
( -- )

SD Card write error.

##### `x-sd-not-sdhc`
( -- )

SD Card is not SDHC/SDXC error.

##### `x-block-zero-protected`
( - )

Attempted to write to protected block zero.

##### `<sd>`
( -- class )

The SDHC/SDXC interface class.

The `<sd>` class has the following constructor:

##### `new`
( cs-pin spi-device sd-card -- )

This constructs a `<sd>` instance for SPI device *spi-device* and chip select pin *cs-pin*. Note that `init-sd` must be called to actually initialize communication with the SDHC/SDXC card connected via said SPI device and chip select line. Note that write-through is set to `false` by default and block zero protection is set to `true` by default.

The `<sd>` class contains the following methods in addition to those in `<block-dev>`:

##### `init-sd`
( sd-card -- )

Init SDHC/SDXC card device.

##### `write-sd-block-zero!`
( enabled sd-card -- )

Enable block zero writes.
