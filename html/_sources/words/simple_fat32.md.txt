# Simple FAT32 Support

zeptoforth includes the `<simple-fat32-fs>` class in the `simple-fat32` module to simplify the usage of FAT32 filesystems on SDHC/SDXC cards in common use cases. Objects of this class are interchangeable with objects of the `<fat32-fs>` class in the `fat32` module, the only differences being in its constructor and in that writethrough is set/gotten on it directly rather than through a separate `<block-dev>` (typically `<sd>`) instance.

### `simple-fat32`

The `simple-fat32` module contains the following words:

##### `<simple-fat32-fs>`
( -- class )

This is the simple FAT32 filesystem class which inherits from the `<base-fat32-fs>` class in the `fat32` module.

This class has the following constructor:

##### `new`
( sck-pin tx-pin rx-pin cs-pin spi-device fs -- )

Construct an `<simple-fat32-fs>` instance with the specified SCK (clock) pin *sck-pin*, TX (transmit) pin *tx-pin*, RX (receive) pin *rx-pin*, and CS (chip select) pin *cs-pin* on the SPI device *spi-device*. All of these will be configured for the user and require no prior initialization. Once the constructor is complete a working FAT32 filesystem will be ready for use.

It also has the following methods in addition to the methods inherited from `<base-fat32-fs>`:

##### `writethrough!`
( writethrough fs -- )

Set the underlying SD card interface to be either writethrough, i.e. write to blocks are immediately written out to the SDHC/SDXC card, or not writethrough, i.e. blocks are not written out to the SDHC/SDXC card except when evicted to load other blocks or if explicitly flushed with `flush`.

##### `writethrough@`
( fs -- writethrough )

Get whether the underlying SD card interface is set to writethrough or not.
