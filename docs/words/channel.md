# Channel Words

A channel is a monodirectional means of communicating data, as bytes or as cells, between two different tasks. Channels form a queue onto which data is sent on one end and data is received on the other. Channels have a fixed size, and any task which attempts to send data on a full channel, or receive data on an empty channel, will be suspended until either data is received from the channel, or data is sent to the channel, respectively.

Note that the internal usable size of a channel is the specified byte count minus one, so in order to be able to queue 16 bytes one must specify a byte count of 17. Internally the byte count is rounded up to the nearest cell but the extra bytes are not used.

Channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap.

Channels are not included in the default builds; the user must load `src/common/forth/channel.fs` or use a big build for them to be available. Note that logic is in place to ensure that it is not to be loaded multiple times. Note that it is compiled into flash when it is loaded.

The following words are in `chan-module`:

##### `x-chan-closed`
( -- )

Channel closed exception. Raised on attempting to send to a closed channel or when trying to receive on an empty closed channel.

##### `x-would-block`
( -- )

Operation would block exception. Raised on attempting to carry out a non-blocking operation when blocking would normally be necessary for the equivalent blocking operation.

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

Send message with a buffer as a payload over a channel. Block until another task receives the message if the channel is full. Note that the buffer is copied, and will be truncated if the buffer size of the channel is smaller than the data provided, and padded with zeros if it is larger. This is not safe to call within an interrupt service routine or a critical section.

##### `recv-chan`
( addr bytes chan -- addr recv-bytes )

Receive message into a buffer from a channel. Block until another task sends a message if the channel is empty. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the channel, and padded with zeros if it is larger; the passed in buffer and the number of bytes copied into it are returned. This is not safe to call within an interrupt service routine or a critical section.

##### `peek-chan`
( addr bytes chan -- addr peek-bytes )

Peek the oldest message into a buffer from a channel, without popping it from the channel's queue. Block until another task sends a message if the channel is empty. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the channel, and padded with zeros if it is larger; the passed in buffer and the number of bytes copied into it are returned. This is not safe to call within an interrupt service routine or a critical section.

##### `send-chan-no-block`
( addr bytes chan -- )

Send message with a buffer as a payload over a channel. If the channel is full, `x-would-block` is raised. Note that the buffer is copied, and will be truncated if the buffer size of the channel is smaller than the data provided, and padded with zeros if it is larger. This is safe to call within an interrupt service routine or a critical section.

##### `recv-chan-no-block`
( addr bytes chan -- addr recv-bytes )

Receive message into a buffer from a channel. If the channel is empty, `x-would-block` is raised. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the channel, and padded with zeros if it is larger; the passed in buffer and the number of bytes copied into it are returned. This is safe to call within an interrupt service routine or a critical section.

##### `peek-chan-no-block`
( addr bytes chan -- addr peek-bytes )

Peek the oldest message into a buffer from a channel, without popping it from the channel's queue. If the channel is empty, `x-would-block` is raised.. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the channel, and padded with zeros if it is larger; the passed in buffer and the number of bytes copied into it are returned. This is safe to call within an interrupt service routine or a critical section.

##### `send-chan-2cell`
( xd chan -- )

Send message with a double cell as a payload over a channel. Block until another task receives the message if the channel is full. This is not safe to call within an interrupt service routine or a critical section.

##### `recv-chan-2cell`
( chan -- xd )

Receive message with a double cell as a payload from a channel. Block until another task sends a message if the channel is empty. This is not safe to call within an interrupt service routine or a critical section.

##### `peek-chan-2cell`
( chan -- xd )

Peek a message with a double cell as a payload from a channel without popping it from the channel's queue. Block until another task sends a message if the channel is empty. This is not safe to call within an interrupt service routine or a critical section.

##### `send-chan-cell`
( x chan -- )

Send message with a cell as a payload over a channel. Block until another task receives a message if the channel is full. This is not safe to call within an interrupt service routine or a critical section.

##### `recv-chan-cell`
( chan -- x )

Receive message with a cell as a payload from a channel. Block until another task sends a message if the channel is empty. This is not safe to call within an interrupt service routine or a critical section.

##### `peek-chan-cell`
( chan -- x )

Peek a message with a cell as a payload from a channel without popping it from the channel's queue. Block until another task sends a message if the channel is empty. This is not safe to call within an interrupt service routine or a critical section.

##### `send-chan-half`
( h chan -- )

Send message with a halfword as a payload over a channel. Block until another task receives a message if the channel is full. This is not safe to call within an interrupt service routine or a critical section.

##### `recv-chan-half`
( chan -- h )

Receive message with a halfword as a payload from a channel. Block until another task sends a message if the channel is empty. This is not safe to call within an interrupt service routine or a critical section.

##### `peek-chan-half`
( chan -- h )

Peek a message with a halfword as a payload from a channel without popping it from the channel's queue. Block until another task sends a message if the channel is empty. This is not safe to call within an interrupt service routine or a critical section.

##### `send-chan-byte`
( b chan -- )

Send message with a byte as a payload over a channel. Block until another task receives a message if the channel is full. This is not safe to call within an interrupt service routine or a critical section.

##### `recv-chan-byte`
( chan -- b )

Receive message with a byte as a payload from a channel. Block until another task sends a message if the channel is empty. This is not safe to call within an interrupt service routine or a critical section.

##### `peek-chan-byte`
( chan -- b )

Peek a message with a byte as a payload from a channel without popping it from the channel's queue. Block until another task sends a message if the channel is empty. This is not safe to call within an interrupt service routine or a critical section.

##### `send-chan-no-block-2cell`
( xd chan -- )

Send message with a double cell as a payload over a channel. If the channel is full, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `recv-chan-no-block-2cell`
( chan -- xd )

Receive message with a double cell as a payload from a channel. If the channel is empty, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `peek-chan-no-block-2cell`
( chan -- xd )

Peek a message with a double cell as a payload from a channel without popping it from the channel's queue. If the channel is empty, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `send-chan-no-block-cell`
( x chan -- )

Send message with a cell as a payload over a channel. If the channel is full, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `recv-chan-no-block-cell`
( chan -- x )

Receive message with a cell as a payload from a channel. If the channel is empty, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `peek-chan-no-block-cell`
( chan -- x )

Peek a message with a cell as a payload from a channel without popping it from the channel's queue. If the channel is empty, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `send-chan-no-block-half`
( h chan -- )

Send message with a halfword as a payload over a channel. If the channel is full, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `recv-chan-no-block-half`
( chan -- h )

Receive message with a halfword as a payload from a channel. If the channel is empty, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `peek-chan-no-block-half`
( chan -- h )

Peek a message with a halfword as a payload from a channel without popping it from the channel's queue. If the channel is empty, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `send-chan-no-block-byte`
( b chan -- )

Send message with a byte as a payload over a channel. If the channel is full, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `recv-chan-no-block-byte`
( chan -- b )

Receive message with a byte as a payload from a channel. If the channel is empty, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.

##### `peek-chan-no-block-byte`
( chan -- b )

Peek a message with a byte as a payload from a channel without popping it from the channel's queue. If the channel is empty, raise `x-would-block`. This is safe to call within an interrupt service routine or a critical section.
