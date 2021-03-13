# Task Queue Words

A task queue is a queue of waiting tasks, which then can be woken up, in the order of their joining the task queue, by another task. Note that tasks can wake up any given number of tasks *before* they join the task queue, upon which they will not actually deschedule themselves but rather increment the count of waiting tasks (which was negative in this case).

Task queues are not included in the default builds; the user must load `src/common/forth/tqueue.fs` or a big build for them to be available. Note that logic is in place to ensure that it is not loaded multiple times. Note that it is compiled into flash when it is loaded.

The following words are in `tqueue-wordlist`:

##### `tqueue-size`
( -- bytes )

Get the size of a task queue in memory.

##### `init-tqueue`
( addr -- )
a
Initialize a task queue starting at the specified address; note that it must be `tqueue-size` bytes in size.

##### `wait-tqueue`
( tqueue -- )

Increment the task queue's internal wait counter. If it is positive, disable the current task and place it at the end of the task queue. Note that this must be called from within a critical section, which it will leave when it disables the current task, and then start a new critical section after the current task is reenabled.

##### `wake-tqueue`
( tqueue -- )

Decrement the task queue's internal wait counter. If there is a task in the task queue, enable the task added to it earliest and place it at the head of the schedule. Note that this must be called from within a critical section.
