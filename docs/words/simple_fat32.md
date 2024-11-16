# Simple FAT32 Support

zeptoforth includes the `<simple-fat32-fs>` class in the `simple-fat32` module to simplify the usage of FAT32 filesystems on SDHC/SDXC cards (SD cards greater than or equal to 4 GB in size) in common use cases.

zeptoforth also includes the `<simple-blocks-fat32-fs>` class in the `simple-blocks-fat32` module to simplify the usage of FAT32 filesystems on on-board Quad SPI flash (on RP2040 boards, RP2350 boards, and the STM32F746 DISCOVERY board) in common use cases. Note that prior to initial use a FAT32 filesystem needs to be initialized in the blocks storage in Quad SPI flash.

The easiest way to accomplish this is by loading with zeptocom.js, `utils/codeload3.sh`, or e4thcom `extra/common/setup_blocks_fat32.fs`, which initializes a master boot record and a single partition containing a FAT32 file system if an initialized master boot record does not exist, and compiles code to flash (if it has not already been compiled) which initializes a `<simple-blocks-fat32-fs>` instance and sets it as the current filesystem on bootup.

This can also be accomplished by loading `test/common/init_fat32_test.fs` and executing `init-fat32-test::run-test`. (This example can be modified to initialize FAT32 filesystems on SDHC/SDXC cards as well.)

For storing FAT32 filesystems in PSRAM on RP2350 boards with PSRAM, there is the optional `<simple-psram-fat32-fs>` class in the `simple-psram-fat32` module. This takes a single constructor parameter, a PSRAM chip select GPIO index for initializing PSRAM with.

This can be added for the Pimoroni Pico Plus 2 by loading `extra/rp2350/setup_pico_plus_2_psram_fat32.fs` with `utils/codeload3.py`, zeptocom.js, or e4thcom. This file includes a fixed PSRAM chip select GPIO of 47; for other boards this can be adapted to use a different chip select GPIO need be. This file creates a module `psram-fat32` containing a `<simple-psram-fat32-fs>` instance that can be gotten by calling `psram-fs@`.

Note that PSRAM is always uninitialized on bootup, so a FAT32 filesystem is initialized in PSRAM on each bootup once this optional code is loaded. For this purpose, `extra/rp2350/setup_psram_fat32.fs` compiles `extra/common/init_fat32.fs` to flash.

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

This is the simple FAT32 filesystem class for accessing FAT32 filesystems in the on-board Quad SPI flash of RP2040 boards, RP2350 boards, and STM32F746 DISCOVERY boards which inherits from the `<base-fat32-fs>` class in the `fat32` module.

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

### `simple-psram-fat32`

This is an optional module in `extra/rp2350/simple_psram_fat32.fs`, to be manually loaded by the user. Note that usually one would actually load `extra/rp2350/setup_pico_plus_2_psram_fat32.fs` which will load this in turn.

The `simple-psram-fat32` module contains the following words:

##### `<simple-psram-fat32-fs>`
( -- class )

This is the simple FAT32 filesystem class for accessing FAT32 filesystems in the on-board Quad SPI PSRAM of RP2350 boards such as the Pimoroni Pico Plus 2 which have PSRAM which inherits from the `<base-fat32-fs>` class in the `fat32` module.

This class has the following constructor:

##### `new`
( psram-cs-pin fs -- )

Construct an `<simple-psram-fat32-fs>` instance using the PSRAM chip select GPIO *psram-cs-pin*. Once the constructor is complete a working FAT32 filesystem will be ready for use.

It also has the following methods in addition to the methods inherited from `<base-fat32-fs>`:

##### `writethrough!`
( writethrough fs -- )

This is a dummy word which is a no-op.

##### `writethrough@`
( fs -- writethrough )

This is a dummy word which is a no-op.
