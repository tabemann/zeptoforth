# Stream Words

A stream is a monodirectional means of communicating data, as a fixed-size queue of bytes which may be accessed without any kind of message boundaries. Streams can be sent to and received from using both blocking operations, which when sending may wait to send entire buffers at once or may allow sending data in parts until all the data is sent, and when receiving/peeking/skipping may wait until a non-zero amount of data is available, and non-blocking operations, which when sending may raise `x-would-block` (declared in `task-module`) if blocking would occur or maysend only part of the buffer provided, and when receiving/peeking/skipping may return a zero byte count.

Streams can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put in alloted memory in the dictionary or into allocated memory in the heap. Note that the size of a stream for a given data size may be calculated with `stream-size`.

The following words are in `stream-module`:

##### `x-stream-closed
( -- )

Stream closed exception. Raised on attempting to send to a closed stream or when trying to receive on an empty closed stream.

##### `stream-full?`
( stream -- flag )

Get whether a stream is full.

##### `stream-empty?`
( stream -- flag )

Get whether a stream is empty.

##### `stream-size`
( data-bytes -- total-bytes )

Get the size in memory for a stream with a specified data size in bytes.

##### `init-stream`
( data-bytes addr -- )

Initialize a stream starting at he specified address with the specified data size in bytes *data-bytes*. *data-bytes* should be the same as the value passed to `stream-size` when alloting or allocating the memory whose starting address is passed in.

##### `close-stream`
( stream -- )

Close a stream. Raise `x-stream-closed` for all pending sending tasks or for all pending receiving tasks if the stream is empty.

##### `reopen-stream`
( stream -- )

Reopen a stream.

##### `stream-closed?`
( stream -- closed )

Get whether a stream is closed.

##### `send-stream`
( addr bytes stream -- )

Send data with a buffer to a stream. Block until there is enough space available in the stream to add the entire buffer of data. Note that the data is copied. This is not safe to call within an interrupt service routine or a critical section.

##### `send-stream-parts`
( addr bytes stream -- )

Send data with a buffer to a stream. Block until there is all the data is sent, adding the data incrementally as space becomes available in the stream. Note that if sending times out, the data may be left partially sent. Note that the data is copied. This is not safe to call within an interrupt service routine or a critcal section.

##### `recv-stream`
( addr bytes stream -- addr recv-bytes )

Receive data into a buffer from a stream. Block until data becomes available in the stream. Note that only as much data is available is copied, and the returned byte count may be lower than the byte count passed in. This is not safe to call within an interrupt service routine or a critical section.

##### `recv-stream-min`
( addr bytes min-bytes stream -- addr recv-bytes )

Receive at least a minimum number of bytes into a buffer from a stream. Block until the minimum number of bytes become available. Note that only as much data is available is copied, and the returned byte count may be lower than the buffer size passed in, even though it will always be equal to or greater than the minimum byte count. This is not safe to call within an interrupt service routine or a critical section.

##### `peek-stream`
( addr bytes stream -- addr peek-bytes )

Peek data into a buffer from a stream, without removing it from the stream. Block until data becomes available in the stream. Note that only as much data is available is copied, and the returned byte count may be lower than the byte count passed in. This is not safe to call within an interrupt service routine or a critical section.

##### `peek-stream-min`
( addr bytes min-bytes tream -- addr peek-bytes )

Peek at least a minimum number of bytes into a buffer from a stream, without removing them from the stream. Block until the minimum number of bytes become available. Note that only as much data is available is copied, and the returned byte count may be lower than the buffer size passed in, even though it will always be equal to or greater than the minimum byte count. This is not safe to call within an interrupt service routine or a critical section.

##### `skip-stream`
( bytes stream -- skip-bytes )

Skip data in a stream, removing it from the stream without copying it. Block until data becomes available in the stream. Note that less data may be skipped than the byte count provided, and the returned byte count is the number of bytes actually skipped. This is not safe to call within an interrupt service routine or a critical section.

##### `skip-stream-min`
( bytes min-bytes stream -- skip bytes )

Skip at least a minimum number of bytes in a stream, removing them from the stream without copying them. Block until the minimum number of bytes become available. Note that less data may be skipped than the byte count *bytes*, even though it will always be equal or greater than the minimum byte count *min-bytes*; the returned byte count is the number of bytes actually skipped. This is not safe to call within an interrupt service routine or a critical section.

##### `send-stream-no-block`
( addr bytes stream -- )

Send data with a buffer to a stream. If there is insufficient space available in the stream to copy the entire buffer, `x-would-block` is raised. This is safe to call within an interrupt service routine or a critical section.

##### `send-stream-partial-no-block`
( addr bytes stream -- send-bytes )

Send data with a buffer to a stream. Only as much data as there is space for inthe stream is sent; the remaining data is omitted. The actual number of bytes sent is returned. This is safe to call within an interrupt service routine or a critical section.

##### `recv-stream-no-block`
( addr bytes stream -- addr recv-bytes )

Receive data into a buffer from a stream. If no data is available, return a byte count of zero. Note that only as much data is available is copied, and the returned byte count may be lower than the byte count passed in. This is safe to call within an interrupt service routine or a critical section.

##### `recv-stream-min-no-block`
( addr bytes min-bytes stream -- addr recv-bytes )

Receive at least a minimum number of bytes from a stream. If the minimum number of bytes is not available, receive no bytes and return a byte count of zero. Note that only as much data is available is copied, and the returned byte count may be lower than the byte count *bytes* passed in. This is safe to call within an interrupt service routine or a critical section.

##### `peek-stream-no-block`
( addr bytes stream -- addr peek-bytes )

Peek data into a buffer from a stream, without removing it from the stream. if no data is available, return a byte count of zero. Note that only as much data is available is copied, and the returned byte count may be lower than the byte count passed in. This is safe to call within an interrupt service routine or a critical section.

##### `peek-stream-min-no-block`
( addr bytes min-bytes stream -- addr peek-bytes )

Peek at least a minimum number of bytes from a stream, without removing them from the stream. If the minimum number of bytes is not available, peek no bytes and return a byte count of zero. Note that only as much data is available is copied, and the returned byte count may be lower th an the byte count *bytes* passed in. This is safe to call within an interrupt service routine or a critical section.

##### `skip-stream-no-block`
( bytes stream -- skip-bytes )

Skip data in a stream, removing it from the stream without copying it. If no data is available, return a byte count of zero. Note that less data may be skipped than the byte count provided, and the returned byte count is the number of bytes actually skipped. This is safe to call within an interrupt service routine or a critical section.

##### `skip-stream-min-no-block`
( bytes min-bytes stream -- skip-bytes )

Skip at least a minimum number of bytes in a stream, removing them from the stream without copying them. If the minimum number of bytes is not available, skip no bytes and return a byte count of zero. Note that less data may be skipped by thane byte count *bytes* provided, and the returned byte count is the number of bytes actually skipped. This is safe to call within an interrupt service routine or a critical section.
