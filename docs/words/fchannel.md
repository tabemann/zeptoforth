# Fast Channel Words

A fast channel is a monodirectional means of communicating data, as either the address and length of buffers (which is not copied) or as single cells, between two different tasks. Fast channels are not queues; provided another task is listening to a fast channel, sending on a fast channel immediately transfers control to the receiving task and sends the sending task to the end of the scheduling ring.

If no task is listening to a fast channel, if no other tasks are attempting to send on the fast channel the data for the fast channel is recorded in the fast channel, and the sending task is descheduled, to be rescheduled at the back of the scheduling ring when a task then receives on the fast channel. If no task is listening to a fast channel, and if other tasks are already attempting to send on the fast channel, another task attempting to send on the fast channel waits until no more tasks are ahead of it, and then records its data, deschedules itself, immediately transfers control if a listening task is ready to receive a message, and once a listening task has received the message is rescheduled.

Fast channels can be created anywhere in memory; they are not dependent upon any allocation mechanism. Therefore they can be put into alloted memory in the dictionary or into allocated memory in the heap.

Fast channels are not included in the default builds; the user must load `src/common/forth/fchannel.fs` for them to be available. Note that logic is in place to ensure that it is not to be loaded multiple times. Also note that they by default are compiled into RAM; the user must specify `compile-to-flash` in order to compile it to flash. Note that channels do not need global initialization in order to be used.

The following words are in `fchan-wordlist`:

##### `fchan-size`
( -- bytes )

Get the size of an fast channel in memory.

##### `init-fchan`
( addr -- )

Initialize a fast channel starting at the specified address. Note that it must have the size `fchan-size`.

##### `send-fchan`
( addr bytes fchan -- )

Send message with a buffer as a payload over a fast channel. Block until another task receives the message; if a task is already waiting for a message, transfer control to it. Note that the buffer is not copied.

##### `recv-fchan`
( fchan -- addr bytes )

Receive message with a buffer as a payload over a fast channel. Block until another task sends a message. Note that the buffer is not copied.

##### `send-fchan-cell`
( x fchan -- )

Send message with a cell as a payload over a fast channel. Block until another task receives the message; if a task is already waiting for a message, transfer control to it.

##### `recv-fchan-cell`
( fchan -- x )

Receive message with a cell as a payload over a fast channel. Block until another task sends a message.
