# Channel Words

A channel is a monodirectional means of communicating data, as arbitrary-sized blocks of data, between two or more different tasks. Channels form a fixed-size queue onto which data is sent on one end and data is received on the other. Any task which attempts to send data on a full channel, or receive data on an empty channel, will be suspended until either data is received from the channel, or data is sent to the channel, respectively, unless non-blocking operations are used, where then `x-would-block` (declared in `task-module`) is raised instead of blocking.

Channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap. Note that the size of a channel for a given element size and element count may be calculated with `chan-size`.

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

Initialize a channel starting at the specified address with the element size and element count. The *element-bytes* and *element-count* should be the same as when they were passed to `chan-size` when alloting or allocating the memory whose starting address is passed in.

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
( addr bytes chan -- recv-bytes )

Receive message into a buffer from a channel. Block until another task sends a message if the channel is empty. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the channel, and padded with zeros if it is larger; the number of bytes copied is returned. This is not safe to call within an interrupt service routine or a critical section.

##### `peek-chan`
( addr bytes chan -- addr peek-bytes )

Peek the oldest message into a buffer from a channel, without popping it from the channel's queue. Block until another task sends a message if the channel is empty. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the channel, and padded with zeros if it is larger; the number of bytes copied is returned. This is not safe to call within an interrupt service routine or a critical section.

##### `skip-chan`
( chan -- )

Skip the most oldest message in a channel. Block until another task sends a message if the channel is empty. Note that no copying takes place, making this faster than receiving a message. This is not safe to call within an interrupt service routine or a critical section.

##### `send-chan-no-block`
( addr bytes chan -- )

Send message with a buffer as a payload over a channel. If the channel is full, `x-would-block` is raised. Note that the buffer is copied, and will be truncated if the buffer size of the channel is smaller than the data provided, and padded with zeros if it is larger. This is safe to call within an interrupt service routine or a critical section.

##### `recv-chan-no-block`
( addr bytes chan -- recv-bytes )

Receive message into a buffer from a channel. If the channel is empty, `x-would-block` is raised. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the channel, and padded with zeros if it is larger; the number of bytes copied is returned. This is safe to call within an interrupt service routine or a critical section.

##### `peek-chan-no-block`
( addr bytes chan -- addr peek-bytes )

Peek the oldest message into a buffer from a channel, without popping it from the channel's queue. If the channel is empty, `x-would-block` is raised. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the channel, and padded with zeros if it is larger; the number of bytes copied is returned. This is safe to call within an interrupt service routine or a critical section.

##### `skip-chan-no-block`
( chan -- )

Skip the most oldest message in a channel. If the channel is empty, `x-would-block` is raised. Note that no copying takes place, making this faster than receiving a message. This is safe to call within an interrupt service routine or a critical section.
