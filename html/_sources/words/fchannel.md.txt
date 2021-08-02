# Fast Channel Words

A fast channel is a monodirectional means of communicating data between two different tasks. Fast channels are not buffered queues; provided another task is listening to a fast channel, sending on a fast channel immediately transfers control to the receiving task and sends the sending task to the end of the scheduling ring.

If no task is listening to a fast channel, if no other tasks are attempting to send on the fast channel the data for the fast channel is recorded in the fast channel, and the sending task is descheduled, to be rescheduled at the back of the scheduling ring when a task then receives on the fast channel. If no task is listening to a fast channel, and if other tasks are already attempting to send on the fast channel, another task attempting to send on the fast channel waits until no more tasks are ahead of it, and then records its data, deschedules itself, immediately transfers control if a listening task is ready to receive a message, and once a listening task has received the message is rescheduled.

Note that if multiple tasks attempt to send on a fast channel, they are queued so that they send their data in the order in which they attempted to send their data. Likewise, if multiple tasks attempt to receive on a fast channel, they are queued so that they receive data in the order in which they attempted to receive dat.

Fast channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap.

Fast channels are not included in the default builds; the user must load `src/common/forth/fchannel.fs` or use a big build for them to be available, after loading `src/common/forth/tqueue.fs`. Note that logic is in place to ensure that they are not loaded multiple times. Note that it is compiled into flash when it is loaded.

The following words are in `fchan-module`:

##### `x-fchan-closed`
( -- )

The fast channel closed exception.

##### `fchan-size`
( data-bytes -- bytes )

Get the size of an fast channel with the given number of data bytes in memory.

##### `init-fchan`
( data-bytes addr -- )

Initialize a fast channel starting at the specified address with the given number of data bytes.

##### `close-fchan`
( fchan -- )

Close a fast channel. Pending sends and receives will be aborted with the exception `x-fchan-closed`.

##### `reopen-fchan`
( fchan -- )

Reopen a fast channel.

##### `fchan-closed?`
( fchan -- closed )

Get whether a fast channel is closed.

##### `send-fchan`
( addr bytes fchan -- )

Send message with a buffer as a payload over a fast channel. Block until another task receives the message; if a task is already waiting for a message, transfer control to it. Note that the buffer is copied, and will be truncated if the buffer size of the fast channel is smaller than the data provided, and padded with zeros if it is larger. Note that this must not be called within a critical section.

##### `recv-fchan`
( addr bytes fchan -- addr recv-bytes )

Receive message with a buffer as a payload over a fast channel. Block until another task sends a message. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the bufer size of the fast channel; the passed in buffer and the number of bytes copied into it are returned. Note that this must not be called within a critical section.

##### `send-fchan-2cell`
( xd fchan -- )

Send message with a double cell as a payload over a fast channel. Block until another task receives the message; if a task is already waiting for a message, transfer control to it. Note that this must not be called within a critical section.

##### `recv-fchan-2cell`
( fchan -- xd )

Receive message with a double cell as a payload over a fast channel. Block until another sends a message. Note that this must not be called within a critical section.

##### `send-fchan-cell`
( x fchan -- )

Send message with a cell as a payload over a fast channel. Block until another task receives the message; if a task is already waiting for a message, transfer control to it. Note that this must not be called within a critical section.

##### `recv-fchan-cell`
( fchan -- x )

Receive message with a cell as a payload over a fast channel. Block until another task sends a message. Note that this must not be called within a critical section.
