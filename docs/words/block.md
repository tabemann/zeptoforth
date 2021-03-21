# Block words

The block interface is written for the STM32F746 DISCOVERY board. It provides a convenient interface for turning the Quad SPI flash functionality into a map of block id's to 1K blocks of memory.

The block interface is not included in the default builds; the user must load `src/stm32f746/forth/block.fs` or use an STM32F746 big build for it to be available. Note that logic is in place to ensure that it is not loaded multiple times. It also requires `src/stm32f746/forth/qspi.fs` to be loaded before it is loaded. When it is loaded it reboots the MCU to carry out initialization.

The following words are in `forth-wordlist`:

##### `load`
( id -- )

Evaluate each 64-byte line in a block with the given id from first to last; if the block does not exist `x-block-not-found` is raised.

##### `load-range`
( start-id end-id -- )

Evaluate each 64-byte line from first to last in each block, if it exists, in the range from `start-id` to `end-id`.

##### `list`
( id -- )

Display each 64-byte line in a block with the given id from first to last; if the block does not exist `x-block-not-found` is raised.

##### `list-range`
( start-id end-id -- )

Display each 64-byte line from first to last in each block, if it exists, in the range from `start-id` to `end-id`.

The following words are in `block-wordlist`:

##### `block-size`
( --  bytes )

The block size in bytes, currently 1024 bytes.

##### `x-invalid-block-id`
( -- )

Exception raised when an invalid block id (presently only $FFFFFFFF) is used.

##### `x-block-write-fail`

Exception when `block!` is unable to write a given block, due to no free space being available and not being unable to free up space due to no completely used sectors existing.

##### `x-block-not-found`

Exception when attempting to delete, load, or list a nonexistent block.

##### `find-block`
( id -- addr | 0 )

Find a block in the memory-mapped Quad SPI flash space by id, or return 0 if no such block can be found.

##### `block?`
( id -- flag )

Return whether a block by a given id exists.

##### `block!`
( addr id -- )

Attempt to write a the 1K buffer at the specified address to a block with a given id; this may fail and raise `x-block-write-fail`. Note that this is not friendly to realtime performance, especially when it has to reclaim existing used sectors

##### `delete-block`
( id -- )

Attempt to delete a block with the given id; if the block does not exist, `x-block-not-found` is raised.

##### `erase-all-blocks`
( -- )

Erase all blocks. Note that this is not friendly to realtime performance.