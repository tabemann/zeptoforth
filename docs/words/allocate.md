# Allocator Words

The allocator in zeptoforth enables the user to create any number of heaps (but in the general use case allows creating a shared heap used implicitly by `allocate`, `resize`, `free`, `allocate!`, `resize!`, and `free!`). Heaps are allocated in the dictionary space, and the shared heap must be manually allocated with `init-shared-heap` before it may be used.

The allocator is not included in the default builds; the user must load `src/common/forth/allocate.fs` or use a big build for it to be available. Note that logic is in place to ensure that it is not loaded multiple times. Note that it is compiled into flash when it is loaded.

The following words are in `forth-wordlist`:

##### `allocate`
( bytes -- addr -1|0 )

Allocate memory on the current heap; returns -1 on success and 0 on failure

##### `free`
( addr -- -1|0 )

Free memory on the current heap; returns -1 on success and 0 on failure

##### `resize`
( addr new-bytes -- addr -1|0 )

Resize memory on the current heap; returns -1 on success and 0 on failure

##### `allocate!`
( bytes -- addr )

Allocate memory in the heap, raising an exception if allocation fails.

##### `resize!`
( addr new-bytes -- new-addr )

Resize memory in the heap, raising an exception if allocation fails.

##### `free!`
( addr -- )

Free memory in the heap, raising an exception if freeing fails.

##### `x-memory-management-failure`
( -- )

Memory management failure exception

The following words are in `allocate-wordlist`:

##### `init-heap-header`
( high-block-size -- heap )

Create an heap with a specified heap size and a specified largest size in the array of sized free lists

##### `expand-heap`
( block-size heap -- )

Expand the heap

##### `init-shared-heap`
( heap-size high-block-size -- )

Initialize the shared heap

##### `x-no-shared-heap`
( -- )

No shared heap exists

##### `allocate-with-heap`
( bytes heap -- addr -1|0 )

Allocate memory on a heap; returns -1 on success and 0 on failure

##### `free-with-heap`
( addr heap -- -1|0 )

Free memory on a heap; returns -1 on success and 0 on failure

##### `resize-with-heap`
( addr new-bytes heap -- addr -1|0 )

Resize memory on a heap; returns -1 on success and 0 on failure
