# Action Words

Actions provide a means of asynchronous processing involving message-passing that is lighter-weight than tasks as all computation takes place within a single task and there is none of the complexities of dealing with multiprocessing (except when handling other tasks adding actions to or removing actions from a scheduler that is currently running within a given task, which are minimal in impact). Individual actions do not have their own stacks but rather share the stacks of their parent task.

Message-passing between actions is synchronous; a given action is halted when it is waiting to receive a message or when it is waiting for the message it has sent to be received. Messages can be of any size, and are stored in a buffer provided by the sending task and copied into a buffer provided by the receiving task when received. Note that no order is guaranteed with regard to message-passing, but messages are guaranteed to be eventually received. Note that actions may only send messages to actions within the same schedule; otherwise they will be ignored. Also, if an action is waiting to send a message and that other message is removed from the schedule, the first action is unblocked.

Schedules may be run in any task, may have actions added to or removed from them at any time, and may be stopped safely at any time (the current action's execution is completed before the schedule stops executing). One note is that a schedule may not be run while it is already running, whether from within the same task or from within a different task.

### `action`

These words are in the module `action`.

##### `current-schedule`
( -- schedule )

Get the current schedule.

##### `current-action`
( -- action )

Get the current action.

##### `current-data`
( -- data )

Get the current action's data.

##### `schedule-size`
( -- bytes )

Get the size of a schedule in bytes.

##### `action-size`
( -- bytes )

Get the size of an action in bytes.

##### `init-schedule`
( addr -- )

Initialize a schedule at the address *addr*.

##### `init-action`
( data xt addr -- )

Initialize an action at the address *addr* with the initial execution token *xt* and the data *data*

##### `add-action`
( schedule action -- )

Add an action *action* to schedule *schedule*. This may be done at any time, including while the schedule is executing.

##### `remove-action`
( schedule action -- )

Remove an action *action* from schedule *schedule*. This may be done at any time, including while the schedule is executing.

##### `send-action`
( send-xt addr bytes dest-action -- )

Send a message in a buffer at *addr* of size *bytes* to action *dest-action* and set the send execution token to be called once the message is sent or message-sending fails to *send-xt*. *send-xt* has the signature ( -- ). Sending a message is synchronous; the current action will not execute until either the message is received or the message becomes unable to be sent. *dest-action* must be in the current schedule or otherwise this routine is a no-op. Sending a message must be done within an executing schedule; messages may not be sent outside of a schedule. Only one message may be sent before control returns to the scheduler; to send multiple messages, send them within subsequent *send-xt*s.

##### `send-action-fail`
( send-xt fail-xt addr bytes dest-action -- )

Send with failure-handling a message in a buffer at *addr* of size *bytes* to action *dest-action*, set the send execution token to be called once the message is sent to *send-xt*, and set the failure execution token to be called if message-sending fails to *fail-xt*. *send-xt* and *fail-xt* have the signature ( -- ). Sending a message is synchronous; the current action will not execute until either the message is received or the message becomes unable to be sent. *dest-action* must be in the current schedule or otherwise this routine is a no-op. Sending a message must be done within an executing schedule; messages may not be sent outside of a schedule. Only one message may be sent before control returns to the scheduler; to send multiple messages, send them within subsequent *send-xt*s.

##### `send-action-timeout`
( send-xt fail-xt addr bytes dest-action timeout-ticks -- )

Send with failure and timeoout-handling a message in a buffer at *addr* of size *bytes* to action *dest-action* with a timeout of *timeout-ticks*, set the send execution token to be called once the message is sent to *send-xt*, and set the failure/timeout execution token to be called if message-sending fails to *fail-xt*. *send-xt* and *fail-xt* have the signature ( -- ). Sending a message is synchronous; the current action will not execute until either the message is received, the message becomes unable to be sent, or the timeout is reached. *dest-action* must be in the current schedule or otherwise this routine is a no-op. Sending a message must be done within an executing schedule; messages may not be sent outside of a schedule. Only one message may be sent before control returns to the scheduler; to send multiple messages, send them within subsequent *send-xt*s.

##### `recv-action`
( recv-xt addr bytes -- )

Receive a message sent to the current action into a buffer at *addr* of size *bytes* and set the receive execution token to *recv-xt*. *recv-xt* has the signature ( addr bytes src-action -- ) where *addr* is the same *addr* of the buffer provided earlier, *bytes* is the actual size of the message received or the size of the buffer, whichever is smaller, and *src-action* is the source action which sent the message received. Receiving messages is synchronous; the current action will not execute until a message is received. Note that only messages sent from other actions in the same schedule may be received.

##### `recv-action-timeout`
( recv-xt timeout-xt addr bytes timeout-ticks -- )

Receive a message sent to the current action into a buffer at *addr* of size *bytes* and set the receive execution token to *recv-xt* and the timeout execution token to *timeout-xt*. *recv-xt* has the signature ( addr bytes src-action -- ) where *addr* is the same *addr* of the buffer provided earlier, *bytes* is the actual size of the message received or the size of the buffer, whichever is smaller, and *src-action* is the source action which sent the message received. Receiving messages is synchronous; the current action will not execute until either a message is received or *timeout-ticks* elapses, where then execution will resume at *timeout-xt* rather than at *recv-xt* as it would were a message received. Note that only messages sent from other actions in the same schedule may be received.

##### `delay-action`
( resume-xt systick-start systick-delay -- )

Delay the current action to a time starting from *systick-start* ticks with a delay of *systick-delay* ticks and set the resume execution token to *resume-xt*.

##### `yield-action`
( resume-xt -- )

Yield the current action and set the resume execution token to *resume-xt*.

##### `run-schedule`
( schedule -- )

Run the schedule *schedule* in the current task. Note that this may not be done if the schedule is already running in any task.

##### `stop-schedule`
( schedule -- )

Stop the schedule *schedule* safely, allowing it to finish executing its current action before stopping.

##### `x-already-in-schedule`
( -- )

Action is already in schedule exception.

##### `x-not-in-schedule`
( -- )

Action is not in a schedule exception.

##### `x-schedule-already-running`
( -- )

Schedule is already running exception.

##### `x-already-sending-msg`
( -- )

Action is already sending a message exception.

##### `x-already-recving-msg`
( -- )

Action is already receiving a message exception.

##### `x-resume-xt-already-set`
( -- )

Resume xt already set for action exception.
