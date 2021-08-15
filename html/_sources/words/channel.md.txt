# Channel Words

A channel is a monodirectional means of communicating data, as bytes or as cells, between two different tasks. Channels form a queue onto which data is sent on one end and data is received on the other. Channels have a fixed size, and any task which attempts to send data on a full channel, or receive data on an empty channel, will be suspended until either data is received from the channel, or data is sent to the channel, respectively.

Note that the internal usable size of a channel is the specified byte count minus one, so in order to be able to queue 16 bytes one must specify a byte count of 17. Internally the byte count is rounded up to the nearest cell but the extra bytes are not used.

Channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap.

Channels are not included in the default builds; the user must load `src/common/forth/channel.fs` or use a big build for them to be available. Note that logic is in place to ensure that it is not to be loaded multiple times. Note that it is compiled into flash when it is loaded.

The following words are in `chan-module`:

##### `x-chan-closed`
( -- )

Channel closed exception. Raised on attempting to send to a closed channel or when trying to receive on an empty closed channel.

##### `chan-full?`
( chan -- flag )

Get whether a channel is full.

##### `chan-empty?`
( chan -- flag )

Get whether a channel is empty.

##### `chan-size`
( element-bytes element-count -- total-bytes )

Get the size in memory for a channel with a specified element size in bytes and element count.

##### `init-chan`
( element-bytes element-count addr -- )

Initialize a channel starting at the specified address with the specified buffer size in bytes. The *element-bytes* and *element-count* should be the same as when they were passed to `chan-size` when alloting or allocating the memory whose starting address is passed in.

##### `close-chan`
( chan -- )

Close a channel. Raise `x-chan-closed` for all pending sending tasks or for all pending receiving tasks if the channel is empty.

##### `reopen-chan`
( chan -- )

Reopen a channel.

##### `chan-closed?`
( chan -- closed )

Get whether a channel is closed.

##### `send-chan`
( addr bytes chan -- )

Send message with a buffer as a payload over a channel. Block until another task receives the message if the channel is full. Note that the buffer is copied, and will be truncated if the buffer size of the channel is smaller than the data provided, and padded with zeros if it is larger. Note that this must not be called within a critical section.

##### `recv-chan`
( addr bytes chan -- addr recv-bytes )

Receive message with a buffer as a payload over a channel. Block until another task sends a message if the channel is empty. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the bufer size of the channel; the passed in buffer and the number of bytes copied into it are returned. Note that this must not be called within a critical section.

##### `send-chan-2cell`
( xd chan -- )

Send message with a double cell as a payload over a channel. Block until another task receives the message if the channel is full. Note that this must not be called within a critical section.

##### `recv-chan-2cell`
( chan -- xd )

Receive message with a double cell as a payload over a channel. Block until another sends a message if the channel is empty. Note that this must not be called within a critical section.

##### `send-chan-cell`
( x chan -- )

Send message with a cell as a payload over a channel. Block until another task receives a message if the channel is full. Note that this must not be called within a critical section.

##### `recv-chan-cell`
( chan -- x )

Receive message with a cell as a payload over a channel. Block until another task sends a message if the channel is empty. Note that this must not be called within a critical section.

##### `send-chan-half`
( h chan -- )

Send message with a halfword as a payload over a channel. Block until another task receives a message if the channel is full. Note that this must not be called within a critical section.

##### `recv-chan-half`
( chan -- h )

Receive message with a halfword as a payload over a channel. Block until another task sends a message if the channel is empty. Note that this must not be called within a critical section.

##### `send-chan-byte`
( b chan -- )

Send message with a byte as a payload over a channel. Block until another task receives a message if the channel is full. Note that this must not be called within a critical section.

##### `recv-chan-byte`
( chan -- b )

Receive message with a byte as a payload over a channel. Block until another task sends a message if the channel is empty. Note that this must not be called within a critical section.
