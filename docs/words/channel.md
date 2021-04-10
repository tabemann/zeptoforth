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
( bytes -- total-bytes )

Get the size in memory for a channel with a specified buffer size in bytes.

##### `init-chan`
( addr bytes -- )

Initialize a channel starting at the specified address with the specified buffer size in bytes. The size should be the same size passed to `chan-size` when alloting or allocating the memory whose starting address is passed in.

##### `close-chan`
( chan -- )

Close a channel. Raise `x-chan-closed` for all pending sending tasks or for all pending receiving tasks if the channel is empty.

##### `chan-closed?`
( chan -- closed )

Get whether a channel is closed.

##### `send-chan-byte`
( b chan -- )

Send a byte to a channel. Block if the channel is full.

##### `recv-chan-byte`
( chan -- b )

Receive a byte from a channel. Block if the channel is empty.

##### `send-chan`
( addr bytes chan -- )

Send bytes from a buffer to a channel. Block if the channel is full or becomes full midway through.

##### `recv-chan`
( addr bytes chan -- )

Receive bytes from a channel into a buffer. Block if the channel is empty or becomes empty midway through.

##### `send-chan-cell`
( x chan -- )

Send a cell to a channel. Block if the channel is full or becomes full midway through.

##### `recv-chan-cell`
( chan -- x )

Receive a cell from a channel. Block if the channel is empty or becomes empty midway through.