# Simple FAT32 Support

zeptoforth includes the `<simple-fat32-fs>` class in the `simple-fat32` module to simplify the usage of FAT32 filesystems on SDHC/SDXC cards (SD cards greater than or equal to 4 GB in size) in common use cases.

zeptoforth also includes the `<simple-blocks-fat32-fs>` class in the `simple-blocks-fat32` module to simplify the usage of FAT32 filesystems on on-board Quad SPI flash (on RP2040 boards and the STM32F746 DISCOVERY board) in common use cases. Note that prior to initial use a FAT32 filesystem needs to be initialized in the blocks storage in Quad SPI flash, which can be accomplished by loading `test/common/init_fat32_test.fs` and executing `init-fat32-test::run-test`. (This example can be modified to initialize FAT32 filesystems on SDHC/SDXC cards as well.)

Objects of these classes are interchangeable with objects of the `<fat32-fs>` class in the `fat32` module, the only differences being in their constructors and in that writethrough is set/gotten on them directly rather than through a separate `<block-dev>` (`<sd>` or `<blocks>`) instance.

### `simple-fat32`

The `simple-fat32` module contains the following words:

##### `<simple-fat32-fs>`
( -- class )

This is the simple FAT32 filesystem class for accessing FAT32 filesystems on SDHC/SDXC cards which inherits from the `<base-fat32-fs>` class in the `fat32` module.

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

### `simple-blocks-fat32`

The `simple-blocks-fat32` module contains the following words:

##### `<simple-blocks-fat32-fs>`
( -- class )

This is the simple FAT32 filesystem class for accessing FAT32 filesystems in the on-board Quad SPI flash of RP2040 boards and STM32F746 DISCOVERY boards which inherits from the `<base-fat32-fs>` class in the `fat32` module.

This class has the following constructor:

##### `new`
( fs -- )

Construct an `<simple-blocks-fat32-fs>` instance Once the constructor is complete a working FAT32 filesystem will be ready for use.

It also has the following methods in addition to the methods inherited from `<base-fat32-fs>`:

##### `writethrough!`
( writethrough fs -- )

Set the underlying blocks interface to be either writethrough, i.e. write to blocks are immediately written out to the Quad SPI flash, or not writethrough, i.e. blocks are not written out to the Quad SPI flash except when evicted to load other blocks or if explicitly flushed with `flush`.

##### `writethrough@`
( fs -- writethrough )

Get whether the underlying blocks interface is set to writethrough or not.
