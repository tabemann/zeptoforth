# Temporary Buffer Words

Temporary buffers are circular buffers for allocating temporary data. Temporarily allocated space should not be relied upon to store data indefinitely, because they reallocate space allocated to old buffers when no more space for continguous blocks of memory are available.

Temporary buffers are not included in the default builds, the user must load `src/common/forth/temp.fs` or use a big build for them to be available. Note that logic is in place to ensure that it is not loaded multiple times. Note that it is compiled into flash when it is loaded.

The following words are in `temp-module`.

##### `x-data-too-big`
( -- )

An exception raised when the size of the data requested to be allocated is larger than the size of the temporary buffer's total space.

##### `temp-size`
( data-bytes -- bytes )

Get the size of a temporary buffer with a given data size.

##### `init-temp`
( data-bytes addr -- )

Initialize a temporary buffer with a given data size at the specified address. Note that the amount of space returned by `temp-size` for the given data size should be alloted or allocated for the temporary buffer.

##### `allocate-temp`
( bytes temp -- addr )

Allocate temporary space of a given data size for a given temporary buffer. If the data size is larger than the temporary buffer's total data size, `x-data-too-big` is raised.
