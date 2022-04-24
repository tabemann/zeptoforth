# Simple channel Words

A simple channel is a monodirectional means of communicating data, as arbitrary-sized blocks of data, between two or more different tasks and/or interrupt service routines. Simple channels form a fixed-size queue onto which data is sent on one end and data is received on the other. Any task which attempts to send data on a full simple channel, or receive data on an empty simple channel, will be suspended until either data is received from the simple channel, or data is sent to the simple channel, respectively, unless non-blocking operations are used, where then `x-would-block` (declared in `task`) is raised instead of blocking. Note that non-blocking simple channel routines are specifically interrupt service routine-safe, unlike other channel constructs provided by zeptoforth

Simple channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap. Note that the size of a simple channel for a given element size and element count may be calculated with `schan-size`.

### `schan`

The following words are in `schan`:

##### `x-schan-closed`
( -- )

Simple channel closed exception. Raised on attempting to send to a closed simple channel or when trying to receive on an empty closed simple channel.

##### `schan-full?`
( schan -- flag )

Get whether a simple channel is full.

##### `schan-empty?`
( schan -- flag )

Get whether a simple channel is empty.

##### `schan-size`
( element-bytes element-count -- total-bytes )

Get the size in memory for a simple channel with a specified element size in bytes and element count.

##### `init-schan`
( element-bytes element-count addr -- )

Initialize a simple channel starting at the specified address with the element size and element count. The *element-bytes* and *element-count* should be the same as when they were passed to `schan-size` when alloting or allocating the memory whose starting address is passed in.

##### `close-schan`
( schan -- )

Close a simple channel. Raise `x-schan-closed` for all pending sending tasks or for all pending receiving tasks if the simple channel is empty.

##### `reopen-schan`
( schan -- )

Reopen a simple channel.

##### `schan-closed?`
( schan -- closed )

Get whether a simple channel is closed.

##### `send-schan`
( addr bytes schan -- )

Send message with a buffer as a payload over a simple channel. Block until another task receives the message if the simple channel is full. Note that the buffer is copied, and will be truncated if the buffer size of the simple channel is smaller than the data provided, and padded with zeros if it is larger. This is not safe to call within an interrupt service routine or a critical section.

##### `recv-schan`
( addr bytes schan -- recv-bytes )

Receive message into a buffer from a simple channel. Block until another task sends a message if the simple channel is empty. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the simple channel, and padded with zeros if it is larger; the number of bytes copied is returned. This is not safe to call within an interrupt service routine or a critical section.

##### `peek-schan`
( addr bytes schan -- addr peek-bytes )

Peek the oldest message into a buffer from a simple channel, without popping it from the simple channel's queue. Block until another task sends a message if the simple channel is empty. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the simple channel, and padded with zeros if it is larger; the number of bytes copied is returned. This is not safe to call within an interrupt service routine or a critical section.

##### `skip-schan`
( schan -- )

Skip the most oldest message in a simple channel. Block until another task sends a message if the simple channel is empty. Note that no copying takes place, making this faster than receiving a message. This is not safe to call within an interrupt service routine or a critical section.

##### `send-schan-no-block`
( addr bytes schan -- )

Send message with a buffer as a payload over a simple channel. If the simple channel is full, or if another task or interrupt service rouine is attempting to send on the same simple channel simultaneously, `x-would-block` is raised. Note that the buffer is copied, and will be truncated if the buffer size of the simple channel is smaller than the data provided, and padded with zeros if it is larger. This is safe to call within an interrupt service routine or a critical section.

##### `recv-schan-no-block`
( addr bytes schan -- recv-bytes )

Receive message into a buffer from a simple channel. If the simple channel is empty, or if another task or interrupt service rouine is attempting to receive on the same simple channel simultaneously, `x-would-block` is raised. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the simple channel, and padded with zeros if it is larger; the number of bytes copied is returned. This is safe to call within an interrupt service routine or a critical section.

##### `peek-schan-no-block`
( addr bytes schan -- addr peek-bytes )

Peek the oldest message into a buffer from a simple channel, without popping it from the simple channel's queue. If the simple channel is empty, or if another task or interrupt service rouine is attempting to receive on the same simple channel simultaneously, `x-would-block` is raised. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the simple channel, and padded with zeros if it is larger; the number of bytes copied is returned. This is safe to call within an interrupt service routine or a critical section.

##### `skip-schan-no-block`
( schan -- )

Skip the most oldest message in a simple channel. If the simple channel is empty, or if another task or interrupt service rouine is attempting to receive on the same simple channel simultaneously, `x-would-block` is raised. Note that no copying takes place, making this faster than receiving a message. This is safe to call within an interrupt service routine or a critical section.
