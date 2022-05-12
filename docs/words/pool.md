# Pool Words

Pools in zeptoforth are created by the user and consist of discretes blocks that are allocated and freed individual as wholes. There is by default no global pool, and pools created by the user are specifically *not* concurrency-safe; to make them concurrency-safe one must wrap them in locks, and because of the overhead this would impose, this is not done so by default. Allocating and freeing blocks in pools occurs in constant time and are fast, unlike allocation, resizing, and freeing in heaps

### `pool`

The following words are in `pool`:

##### `pool-size`
( -- bytes )

Get the size of a pool header in bytes.

##### `init-pool`
( block-size addr -- )

Initialize a pool at *addr* with the given block size of *block-size* bytes. Note that no space for storing blocks is available in a pool when it is first initialized; to add memory to a pool use `add-pool`.

##### `add-pool`
( addr bytes pool -- )

Add memory starting at *addr* of size *bytes* to *pool* as discrete blocks; only a multiple of the block size of the pool will be added to the pool, so if *bytes* is not a multiple of said block size not all of the space in the memory provided will be used.

##### `allocate-pool`
( pool -- addr )

Allocate a block in *pool* and return its address. If no blocks are available in the pool, `x-allocate-failed` is raised.

##### `free-pool`
( addr pool -- )

Free a block at *addr* in *pool*, making it available to future allocation.

##### `pool-block-size`
( pool -- bytes )

Get the block size of a pool.

##### `pool-free-count`
( pool -- count )

Get the number of free blocks in a pool.

##### `pool-total-count`
( pool -- count )

Get the total number of blocks in a pool.

##### `x-allocate-failed`
( -- )

The exception raised if block allocation fails.
