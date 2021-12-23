# Heap Words

Heaps in zeptoforth are created by the user and consist of discretes blocks that are allocated, freed, and resized as multiples; the size of allocations plus a cell taken up by a block count is rounded up to the next full number of blocks. There is by default no global heap, and heaps created by the user are specifically *not* concurrency-safe; to make them concurrency-safe one must wrap them in locks, and because of the overhead this would impose, this is not done so by default. Note that the time taken up by heap allocation or resizing is bounded by a maximum which is defined by the number of blocks in the heap; any heap allocation or resizing may take this full time. On the other hand, the time taken up by freeing an allocation is determined solely by the number of blocks comprising the allocation.

### `heap-module`

The following words are in `heap-module`:

##### `heap-size`
( block-size block-count -- heap-bytes )

Get the size in bytes of a heap with the given block size in bytes and block count.

##### `init-heap`
( block-size block-count addr -- )

Initialize a heap at *addr* with the given block size in bytes and block count; note that the size of the available memory at *addr* should be equal to or greater than the number of bytes returned by `heap-size` for *block-size* and *block-count*.

##### `allocate`
( size heap -- addr )

Allocate memory in a heap of *size* bytes and return its address; if the memory cannot be allocated due to insufficient contiguous memory being available, *x-allocate-failed* is raised.

##### `free`
( addr heap -- )

Free memory at *addr* in a heap.

##### `resize`
( size addr heap -- new-addr )

Resize memory in a heap at *addr* to a new size in bytes, returning its new address. If sufficient memory is available for resizing at *addr* the allocation is expanded without moving or copying it and *addr* is returned. Otherwise, the allocation at *addr* is freed, and its contents is copied to a new allocation, whose address is returned. Note that if insufficient memory is available in the heap for resizing the allocation, the existing allocation is preserved, and *x-allocate-failed* is raised.

##### `x-allocate-failed`
( -- )

This is an exception raised if allocation or resizing fails.
