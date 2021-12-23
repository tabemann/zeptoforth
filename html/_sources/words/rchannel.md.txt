# Reply Channel Words

A reply channel is a bidirectional means of communicating data between two different tasks, involving interlocking messages sent to tasks listening on them followed by replies sent synchronously back to original sending tasks by the receiving tasks. Reply channels are not buffered queues; provided another task is listening to a reply channel, sending on a reply channel immediately transfers control to the receiving task and sends the sending task to the end of the scheduling ring. Only a single copy is done when transferring data from the sending task to the receiving task.

If no task is listening to a reply channel, if no other tasks are attempting to send on the reply channel the data for the reply channel is recorded in the reply channel, and the sending task is descheduled, to be rescheduled at the back of the scheduling ring when a task then receives on the reply channel. If no task is listening to a reply channel, and if other tasks are already attempting to send on the reply channel, another task attempting to send on the reply channel waits until no more tasks are ahead of it, and then records its data, deschedules itself, immediately transfers control if a listening task is ready to receive a message, and once a listening task has received the message is rescheduled.

Note that if multiple tasks attempt to send on a reply channel, they are queued so that they send their data in the order in which they attempted to send their data. Likewise, if multiple tasks attempt to receive on a reply channel, they are queued so that they receive data in the order in which they attempted to receive dat.

Once a task receives a message on a reply channel, the reply channel is put into a state where it is pending a reply. No other messages will be sent over the reply channel until the task which received the message sends a reply back to the task which had originally sent on the reply channel. Like the original message sent from the sending task to the receiving task, no data is buffered in a reply but rather the reply data is copied directly from the buffer provided by the replying task to the reply buffer provided by the original sending task.

Reply channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap.

### `rchan-module`

The following words are in `rchan-module`:

##### `x-rchan-closed`
( -- )

The reply channel closed exception.

##### `x-rchan-wait-reply`
( -- )

This exceptions is raised if a task for which a reply channel is awaiting a reply follow a receive attempts to receive from that reply channel again. 

##### `x-rchan-not-wait-reply`
( -- )

This exception is raised if a task attempts to reply to a reply channel which is not awaiting a reply from that task.

##### `rchan-size`
( -- bytes )

Get the size of an reply channel in bytes.

##### `init-rchan`
( addr -- )

Initialize a reply channel starting at the specified address.

##### `close-rchan`
( rchan -- )

Close a reply channel. Pending sends and receives will be aborted with the exception `x-rchan-closed`.

##### `reopen-rchan`
( rchan -- )

Reopen a reply channel.

##### `rchan-closed?`
( rchan -- closed )

Get whether a reply channel is closed.

##### `send-rchan`
( send-addr send-bytes reply-addr reply-bytes rchan -- reply-bytes' )

Send message with a buffer *send-addr* with size *send-bytes* in bytes as a payload over a reply channel. Block until another task replies to the message; if a task is already waiting for a message, transfer control to it. Note that the buffer is copied, and will be truncated if the buffer size of the reply channel is smaller than the data provided, and padded with zeros if it is larger. In turn, the reply provided by the receiving task is copied into the provided reply buffer *reply-addr* with the size *reply-bytes* in bytes, and the actual size in bytes of the reply is returned. Note that this must not be called within a critical section.

##### `recv-rchan`
( addr bytes rchan -- recv-bytes )

Receive message with a buffer *addr* with size *bytes* in bytes as a payload over a reply channel. Block until another task sends a message. Note that the buffer is copied, and will be truncated if the provided buffer is smaller than the bufer size of the reply channel; the number of bytes copied is returned. At this point the reply channel is in a state where it is waiting for a reply; the sending task will only be readied once a reply is provided with `reply-rchan`. Note that this must not be called within a critical section. Note that the task for which a reply channel is awaiting a reply following a receive cannot receive from that reply channel; if this is attenpted `x-rchan-wait-reply` is raised.

##### `reply-rchan`
( addr bytes rchan -- )

Reply to a reply channel which the current task had received a message from with the data in buffer *addr* with size *bytes* in bytes. This data is copied into the reply buffer of the original sending task, truncated to the size of the sending task's reply buffer. Note that the task replying to a reply channel must be the same task as that which had originally received from the reply channel, or else `x-rchan-not-wait-reply` is raised.
