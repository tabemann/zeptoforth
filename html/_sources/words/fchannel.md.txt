# Rendezvous Channel Words

A rendezvous channel is a monodirectional means of communicating data between two different tasks. Rendezvous channels are not buffered queues; provided another task is listening to a rendezvous channel, sending on a rendezvous channel immediately transfers control to the receiving task and sends the sending task to the end of the scheduling ring. Only a single copy is done when transferring data from the sending task to the receiving task.

Tasks attempting to send on rendezvous channels first check whether there is a queued task waiting to receive on the rendezvous channel; if there are tasks queued to receive, the first task queued is dequeued, the sending task's buffer is copied into the receiving task's buffer, and the receiving task is readied. If no tasks are queued to receive, the sending task is queued to send on the rendezvous channel and is blocked, if `send-fchan` had been called, otherwise `task::x-would-block` is raised, if `send-fchan-no-block` had been called.

Tasks attempting to receive on rendezvous channels first check whether there is a queued task waiting to send on the rendezvous channel; if there are tasks queued to send, the first task queued is dequeued, the sending task's buffer is copied into the receiving task's buffer, and the sending task is readied. If no tasks are queued to send, the receiving task is queued to receive on the rendezvous channel and is blocked, if `recv-fchan` had been called, otherwise `task::x-would-block` is raised, if `recv-fchan-no-block` had been called.

Both the send queues and receive queues are ordered first by task priority and second by the order in which tasks were added to them, with tasks of higher priority and tasks added earlier coming first.

Rendezvous channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap.

### `fchan`

The following words are in `fchan`:

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

##### `send-fchan-no-block`
( addr bytes fchan -- )

Send message with a buffer as a payload over a rendezvous channel. If a task is already waiting for a message, transfer control to it, otherwise raise `task::x-would-block`. Note that the buffer is copied, and will be truncated if the buffer size of the rendezvous channel is smaller than the data provided, and padded with zeros if it is larger. Note that this must not be called within a critical section.

##### `recv-fchan`
( addr bytes fchan -- recv-bytes )

Receive message with a buffer as a payload over a rendezvous channel. Block until another task sends a message. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the rendezvous channel; the number of bytes copied is returned. Note that this must not be called within a critical section.

##### `recv-fchan-no-block`
( addr bytes fchan -- recv-bytes )

Receive message with a buffer as a payload over a rendezvous channel. If no task is waiting to send a message, raise `task::x-would-block`. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the buffer size of the rendezvous channel; the number of bytes copied is returned. Note that this must not be called within a critical section.
