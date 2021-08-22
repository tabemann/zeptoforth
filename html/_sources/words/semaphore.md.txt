# Semaphore Words

A semaphore is a task synchronization construct which keeps a counter indicating some resource and, when the counter is negative, a queue of waiting tasks. Tasks can *give* a semaphore, which increments the counter, and if there one or more waiting tasks, readies the task that has been blocked the longest in an FIFO fashion. Note that a limit can be specified where *giving* a semaphore will not increase the counter beyond the specified limit. Tasks can also *take* a semaphore, which decrements the counter, and if afterwards the counter is negative, blocks the task and places it at the end of the queue of waiting tasks. Additionally there are operations for *ungiving* a semaphore, in the case where one needs to decrement the counter without potentially blocking the current task, and for *broadcasting* a semaphore, which is equivalent to *giving* each waiting task.

Semaphores are not included in the default builds, the user must load `src/common/forth/semaphore.fs` or use a big build for them to be available. Note that logic is in place to ensure that it is not loaded multiple times. Note that it is compiled into flash when it is loaded.

The following words are in `sema-module`:

##### `sema-size`
( -- bytes )

The size of a semaphore in bytes.

##### `no-sema-limit`
( -- n )

Magic value indicating no semaphore limit.

##### `init-sema`
( limit counter addr -- )

Initialize a semaphore at the specified address *addr*, which must have size `sema-size`, with *counter* as the initial counter and *limit* as the counter limit; a *limit* of `no-sema-limit` will disable counter limits.

##### `take`
( semaphore -- )

Decrement the counter of a semaphore, blocking the current task if the counter after decrementing is negative; if a timeout is specified via the user variable `timeout` and the current task is not readied before the timeout is reached, the exception `x-timed-out` is raised (both `timeout` and `x-timed-out` are defined in `task-module`). This is not safe to call within an interrupt service routine or critical section.

##### `give`
( semaphore -- )

Increment the counter of a semaphore, up to the counter limit if one is set; if one or more tasks are blocked on the semaphore, the one that has been blocked the longest is readied. This is safe to call within an interrupt service routine or critical section.

##### `ungive`
( semaphore -- )

Decrement the counter of a semaphore without blocking the current task. This is safe to call within an interrupt service routine or critical section.

##### `broadcast`
( semaphore -- )

*Give* all of the tasks blocked on a semaphore. This is safe to call within an interrupt service routine or critical section.
