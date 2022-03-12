# Bidirectional Channel Words

A bidirectional channel is a bidirectional means of communicating data between two different tasks, involving interlocking messages sent to tasks listening on them followed by replies sent synchronously back to original sending tasks by the receiving tasks. Bidirectional channels are not buffered queues; provided another task is listening to a bidirectional channel, sending on a bidirectional channel immediately transfers control to the receiving task and sends the sending task to the end of the scheduling ring. Only a single copy is done when transferring data from the sending task to the receiving task.

Tasks attempting to send on bidirectional channels first check whether there is a queued task waiting to receive on the bidirectional channel; if there are tasks queued to receive, the first task queued is dequeued, the sending task's buffer is copied into the receiving task's buffer, and the receiving task is readied. If no tasks are queued to receive, the sending task is queued to send on the bidirectional channel and is blocked.

Tasks attempting to receive on bidirectional channels first check whether there is a queued task waiting to send on the bidirectional channel; if there are tasks queued to send, the first task queued is dequeued, the sending task's buffer is copied into the receiving task's buffer, and the sending task is readied. If no tasks are queued to send, the receiving task is queued to receive on the bidirectional channel and is blocked.

Both the send queues and receive queues are ordered first by task priority and second by the order in which tasks were added to them, with tasks of higher priority and tasks added earlier coming first.

Once a task receives a message on a bidirectional channel, the bidirectional channel is put into a state where it is pending a reply. No other messages will be sent over the bidirectional channel until the task which received the message sends a reply back to the task which had originally sent on the bidirectional channel. Like the original message sent from the sending task to the receiving task, no data is buffered in a reply but rather the reply data is copied directly from the buffer provided by the replying task to the reply buffer provided by the original sending task.

Bidirectional channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap.

### `rchan`

The following words are in `rchan`:

##### `x-rchan-closed`
( -- )

The bidirectional channel closed exception.

##### `x-reply-pending`
( -- )

This exception is raised if a bidirectional channel is replied to after it has already been replied to and before the original sending task awaiting for a reply has had the chance to handle the reply

##### `rchan-size`
( -- bytes )

Get the size of an bidirectional channel in bytes.

##### `init-rchan`
( addr -- )

Initialize a bidirectional channel starting at the specified address.

##### `close-rchan`
( rchan -- )

Close a bidirectional channel. Pending sends and receives will be aborted with the exception `x-rchan-closed`.

##### `reopen-rchan`
( rchan -- )

Reopen a bidirectional channel.

##### `rchan-closed?`
( rchan -- closed )

Get whether a bidirectional channel is closed.

##### `send-rchan`
( send-addr send-bytes reply-addr reply-bytes rchan -- reply-bytes' )

Send message with a buffer *send-addr* with size *send-bytes* in bytes as a payload over a bidirectional channel. Block until another task replies to the message; if a task is already waiting for a message, transfer control to it. Note that the buffer is copied, and will be truncated if the buffer size of the bidirectional channel is smaller than the data provided, and padded with zeros if it is larger. In turn, the reply provided by the receiving task is copied into the provided reply buffer *reply-addr* with the size *reply-bytes* in bytes, and the actual size in bytes of the reply is returned. Note that this must not be called within a critical section.

##### `recv-rchan`
( addr bytes rchan -- recv-bytes )

Receive message with a buffer *addr* with size *bytes* in bytes as a payload over a bidirectional channel. Block until another task sends a message. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the bufer size of the bidirectional channel; the number of bytes copied is returned. At this point the bidirectional channel is in a state where it is waiting for a reply; the sending task will only be readied once a reply is provided with `reply-rchan`. Note that this must not be called within a critical section. Note that the task for which a bidirectional channel is awaiting a reply following a receive cannot receive from that bidirectional channel; if this is attenpted `x-rchan-wait-reply` is raised.

##### `reply-rchan`
( addr bytes rchan -- )

Reply to a bidirectional channel which the current task had received a message from with the data in buffer *addr* with size *bytes* in bytes. This data is copied into the reply buffer of the original sending task, truncated to the size of the sending task's reply buffer. Note that if no task is waiting for a reply, the reply is discarded silently; this is necessary due to the possibility of sending tasks timing out prior to receiving their reply.
