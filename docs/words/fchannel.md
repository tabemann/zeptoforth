# Rendezvous Channel Words

A rendezvous channel is a monodirectional means of communicating data between two different tasks. Rendezvous channels are not buffered queues; provided another task is listening to a rendezvous channel, sending on a rendezvous channel immediately transfers control to the receiving task and sends the sending task to the end of the scheduling ring. Only a single copy is done when transferring data from the sending task to the receiving task.

If no task is listening to a rendezvous channel, if no other tasks are attempting to send on the rendezvous channel the data for the rendezvous channel is recorded in the rendezvous channel, and the sending task is descheduled, to be rescheduled at the back of the scheduling ring when a task then receives on the rendezvous channel. If no task is listening to a rendezvous channel, and if other tasks are already attempting to send on the rendezvous channel, another task attempting to send on the rendezvous channel waits until no more tasks are ahead of it, and then records its data, deschedules itself, immediately transfers control if a listening task is ready to receive a message, and once a listening task has received the message is rescheduled.

Note that if multiple tasks attempt to send on a rendezvous channel, they are queued so that they send their data in the order in which they attempted to send their data. Likewise, if multiple tasks attempt to receive on a rendezvous channel, they are queued so that they receive data in the order in which they attempted to receive dat.

Rendezvous channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap.

The following words are in `fchan-module`:

##### `x-fchan-closed`
( -- )

The rendezvous channel closed exception.

##### `fchan-size`
( -- bytes )

Get the size of an rendezvous channel in bytes.

##### `init-fchan`
( addr -- )

Initialize a rendezvous channel starting at the specified address.

##### `close-fchan`
( fchan -- )

Close a rendezvous channel. Pending sends and receives will be aborted with the exception `x-fchan-closed`.

##### `reopen-fchan`
( fchan -- )

Reopen a rendezvous channel.

##### `fchan-closed?`
( fchan -- closed )

Get whether a rendezvous channel is closed.

##### `send-fchan`
( addr bytes fchan -- )

Send message with a buffer as a payload over a rendezvous channel. Block until another task receives the message; if a task is already waiting for a message, transfer control to it. Note that the buffer is copied, and will be truncated if the buffer size of the rendezvous channel is smaller than the data provided, and padded with zeros if it is larger. Note that this must not be called within a critical section.

##### `recv-fchan`
( addr bytes fchan -- addr recv-bytes )

Receive message with a buffer as a payload over a rendezvous channel. Block until another task sends a message. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the bufer size of the rendezvous channel; the passed in buffer and the number of bytes copied into it are returned. Note that this must not be called within a critical section.
