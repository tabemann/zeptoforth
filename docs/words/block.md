# Block words

The block interface is written for the STM32F746 DISCOVERY board. It provides a convenient interface for turning the Quad SPI flash functionality into a map of block id's to 1K blocks of memory.

The block interface is not included in the default builds; the user must load `src/stm32f746/forth/block.fs` for it to be available. Note that logic is in place to ensure that it is not loaded multiple times. It also requires `src/stm32f746/forth/qspi.fs` to be loaded before it is loaded. When it is loaded it reboots the MCU to carry out initialization.

The following words are in `block-wordlist`:

##### `block-size`
( --  bytes )

The block size in bytes, currently 1024 bytes.

##### `x-invalid-block-id`
( -- )

Exception raised when an invalid block id (presently only $FFFFFFFF) is used.

##### `x-block-write-fail`

Exception when `block!` is unable to write a given block, due to no free space being available and not being unable to free up space due to no completely used sectors existing.

##### `find-block`
( id -- addr | 0 )

Find a block in the memory-mapped Quad SPI flash space by id, or return 0 if no such block can be found.

##### `block?`
( id -- flag )

Return whether a block by a given id exists.

##### `block!`
( addr id -- )

Attempt to write a the 1K buffer at the specified address to a block with a given id; this may fail and raise `x-block-write-fail`. Note that this is not friendly to realtime performance, especially when it has to reclaim existing used sectors

##### `erase-all-blocks`
( -- )

Erase all blocks. Note that this is not friendly to realtime performance.