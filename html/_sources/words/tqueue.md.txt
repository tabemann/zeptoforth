# Task Queue Words

A task queue is a queue of waiting tasks, which then can be woken up, in the order of highest priority first and then the order of their joining the task queue, by another task. Note that tasks can wake up any given number of tasks *before* they join the task queue, upon which they will not actually deschedule themselves but rather increment the count of waiting tasks (which was negative in this case).

## `tqueue`

The following words are in `tqueue`:

##### `tqueue-size`
( -- bytes )

Get the size of a task queue in memory.

##### `no-tqueue-limit`
( -- n )

Magic value indicating no task queue counter limit.

##### `init-tqueue`
( slock addr -- )

Initialize a task queue with simple lock *slock* starting at the specified address; note that it must be `tqueue-size` bytes in size. It will have no counter limit, and its counter will be initialized to zero.

##### `init-tqueue-full`
( limit counter slock addr -- )

Initialize a task queue with simple lock *slock* starting at the specified address; note that it must be `tqueue-size` bytes in size. Its counter limit will be initialized to *limit* and its counter will be initialized to *counter*.

##### `wait-tqueue`
( tqueue -- )

Decrement the task queue's internal wait counter. If it is negative, block the current task and place it in the task queue after the last task with a greater or equal priority. Note that the simple lock for the task queue must be held by the task which calls this; this simple lock is released if this blocks, to be claimed again when the task is readied.

##### `wake-tqueue`
( tqueue -- )

Increment the task queue's internal wait counter. If a counter limit is set, the internal wait counter will not increase beyond it. If there is a task in the task queue, ready the task with the highest priority added first to the task queue. Note that the simple lock for the task queue must be held by the task which calls this.

##### `unwake-tqueue`
( tqueue -- )

Decrement the task queue's internal wait counter.

##### `wake-tqueue-all`
( tqueue -- )

Wake all the tasks waiting on a task queue.
