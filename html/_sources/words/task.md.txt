# Multitasking Words

Multitasking in zeptoforth is not part of the zeptoforth kernel, but is provided by `src/common/forth/task.fs`, which in turn relies upon `src/<platform>/forth/int_io.fs` and `src/common/forth/systick.fs`. It is preemptive and priority-scheduled, but `PAUSE` may be called to explicitly relinquish control of the processor (which is highly recommended when possible); note that some words do this implicitly, such as `MS`, `KEY`, or `EMIT` (which are in turn called by words such as `REFILL` or `TYPE`).

Note that tasks in zeptoforth are a relatively heavy-weight asynchronous computing means. For lighter-weight asynchronous computing, consider creating a single task for running a scheduler within (so the main task can be devoted to the REPL), and then put all asynchronous actions within that.

There are a number of intertask communication and synchronization constructs available in full builds, including semaphores, locks, message-oriented queue channels, message-oriented rendezvous channels, byte-oriented streams, and last but not least, task notifications. All of these except for task notifications are discussed in other documentation pages.

Task notifications are the lightest-weight of all these mechanisms; simple synchronization between two tasks using task notifications take roughly 1/4 the time of using rendezvous channels, and task notifications have no extra memory overhead other than the mailboxes allocated by the user for each task, at one cell per mailbox whereas rendezvous channels take a minimum of 76 bytes of RAM. Mailboxes may only contain one cell each, and a task may have a maximum of 32 mailboxes for notifications. Notifications may set them to fixed values, update them with provided execution tokens, or leave them as-is. Note that improperly-written code using notifications may be subject to race conditions and data loss because no mechanism exists built-in to prevent a notification sent to a task in close succession after another notification from overwriting the contents of the mailbox in question. Also note that task notifications must be configured for a task `config-notify` before they may be used; the user must provide a pointer to a buffer containing a number of cells equal to the specified number of notification mailboxes.

Multitasking is enabled by default once `src/common/forth/task.fs` has been loaded and the MCU has been rebooted; afterwards each time the MCU is booted a new task is created for the REPL, the main task, and multitasking is initiated.

### `forth-module`

This word is in `forth-module`:

##### `pause`
( -- )

Pass off control to the next active task; if no tasks are active, put the MCU to sleep until an interrupt occurs (typically due to SysTick or USART activity).

### `task-module`

These words are in `task-module`:

##### `current-task`
( -- task )

The current task.

##### `main-task`
( -- task )

The main task.

##### `pause-count`
( -- count )

Get the current pause count, an unsigned 32-bit value which rolls over.xs

To create a task in zeptoforth, one should execute the following:

##### `spawn`
( xn...x0 count xt dictionary-size stack-size rstack-size -- task )

where *xn* through *x0* are parameters to pass to the *xt* when executed, *count* is the number of such parameters, *xt* is the entry point to the task, *dictionary-size* is the size of the dictionary for the spawned task in bytes, *stack-size* is the size of the data stack for the spawned task in bytes, *rstack-size* is the size of the return stack for the spawned task in bytes, and *task* is the header for the newly spawned task. *dictionary-size*, *stack-size*, and *rstack-size* must be multiples of four I have had good luck with values of 256 for each of these; I do not know how well smaller values will work out, especially in the case of *rstack-size*, where too small of a size will almost certainly result in a crash. This in turn returns a pointer to the task header on the stack, which can then be stored in a variable or constant.

Note that it is not recommended to execute this while compiling to flash; rather, it should be put in the `init` routine and then the result should be stored to a variable in RAM.

Note that tasks may be enabled or disabled but once created exist until the MCU is rebooted.

New task default to a priority of zero; to change this use `task-priority!`.

Tasks default to having no support for notifications; notifications must be configured for tasks with `config-notify`.

Uncaught exceptions within a task will be handled, with the message for them being displayed, but they will result in said task being terminated.

##### `config-notify`
( notify-area-addr notify-count task -- )

Configure notification for a task, with *notify-count* being the number of supported notifications, from 0 to 32, and *notify-area-addr* being the address to an area of memory that may contain *notify-count* cells.

To reinitialize existing tasks, one executes:

##### `init-task`
( xn...x0 count xt task -- )

These tasks may be in any state, including being terminated. *xn* through *x0* are parameters to pass to the *xt* when executed.

New tasks do not execute right away, rather to enable their execution, one executes:

##### `run`
( task -- )

which increments the active counter for the *task* (which is initialized to zero); the task executes if this counter is greater than zero.

In turn a task can be disabled with:

##### `stop`
( task -- )

which decrements the active counter for the *task*.

To terminate a task, one executes:

##### `kill`
( task -- )

which immediately halts that task's executing, including if it is the current task, and puts it in a terminated state.

To get whether a task is terminated, one executes:

##### `terminated?`
( task -- terminated )

which returns the terminated state of a task.

If one attempts to execute a word against a terminated task, aside from `init-task` or `terminated?`, the following exception is raised:

##### `x-terminated`
( -- )

This exception is raised when executed displays the task terminated error message.

##### `x-out-of-range-notify`
( -- )

This exception is raised when an out of range notification mailbox index or count is specified.

##### `timeout`
( -- addr )

A user variable used for storing a timeout value in ticks for various operations.

##### `no-timeout`
( -- n )

A constant representing no timeout

##### `last-delay`
( task -- ticks-delay ticks-start )

Get the last delay setting, from calling `delay` or `block-timeout`, for a task.

##### `delay`
( delay start task -- )

Delay a task until *delay* ticks after the time represented by *start* ticks.

##### `delay-critical`
( delay start task -- )

Delay a task until *delay* ticks after the time represented by *start* ticks, and afterwards immediately start a critical section.

##### `block-timeout`
( delay start task -- )

Block a task until *delay* ticks after the time represented by *start* ticks, and signal timed out if this time is reached without the task being readied first.

##### `block-timeout-critical`
( delay start task -- )

Block a task until *delay* ticks after the time represented by *start* ticks, and signal timed out if this time is reached without the task being readied first; after blocking finishes, immediately start a critical section.

##### `block-wait`
( task -- )

Block a task until all waiting task are woken with `wake`.

##### `block-wait-critical`
( task -- )

Block a task until all waiting task are woken with `wake`, and then immediately start a critical section.

##### `block-indefinite`
( task -- )

Block a task indefinitely until the task is readied.

##### `block-indefinite-critical`
( task -- )

Block a task indefinitely until the task is readied, and then immediately start a critical section.

##### `wait-notify-timeout`
( delay start notify-index -- x )

If the current task has not been notified for the specified notification, set the task in an awaiting notification state for that notification until *delay* ticks after the time represented by *start* ticks, and raise `x-timed-out` if this time is reached without the task being notified for that notification first. If successful, the notified state for the specified notification is cleared and the contents of the notification mailbox in question is returned.

##### `wait-notify-timeout-critical`
( delay start notify-index -- x )

If the current task has not been notified for the specified notification, set the task in an awaiting notification state for that notification until *delay* ticks after the time represented by *start* ticks, and raise `x-timed-out` if this time is reached without the task being notified for that notification first; after blocking finishes, immediately start a critical section unless an exception has been raised. If successful, the notified state for the specified notification is cleared and the contents of the notification mailbox in question is returned.

##### `wait-notify-indefinite`
( notify-index -- x )

If the current task has not been notified for the specified notification, set the task in an awaiting notification state for that notification until the task is notified for that notification. If successful, the notified state for the specified notification is cleared and the contents of the notification mailbox in question is returned.

##### `wait-notify-indefinite-critical`
( notify-index -- x )

If the current task has not been notified for the specified notification, set the task in an awaiting notification state for that notification until the task is notified for that notification; after blocking finishes, immediately start a critical section unless an exception has been raised. If successful, the notified state for the specified notification is cleared and the contents of the notification mailbox in question is returned.

##### `ready`
( task -- )

Ready a blocked or delayed task.

##### `notify`
( notify-index task -- )

Notify a task on a specified notification index *notify-index* without changing the value of the notification mailbox at *notify-index*, readying the task if it is currently waiting on that notification.

##### `notify-set`
( x notify-index task -- )

Notify a task on a specified notification index *notify-index*, setting the value of its notification mailbox at *notify-index* to *x*, readying the task if it is currently waiting on that notification.

##### `notify-update`
( xt notify-index task -- )

Notify a task on a specified notification index *notify-index*, updating the value of its notification mailbox at *notify-index* by applying the execution token *xt* with the signature ( x0 -- x1 ) to it, readying the task if it is currently waiting on that notification.

##### `block`
( task -- )

Block a task for which blocking has been prepared.

##### `block-critical`
( task -- )

Block a task for which blocking has been prepared, and immediately start a new critical section once it finishes blocking.

##### `wait-notify`
( notify-index -- x )

If the current task has not been notified for the specified notification, set the task in an awaiting notification state for that notification that may be blockign or indefinite depending on how blocking has been prepared. If successful, the notified state for the specified notification is cleared and the contents of the notification mailbox in question is returned.

##### `wait-notify-critical`
( notify-index -- x )

If the current task has not been notified for the specified notification, set the task in an awaiting notification state for that notification that may be blockign or indefinite depending on how blocking has been prepared; after blocking finishes, immediately start a critical section unless an exception has been raised. If successful, the notified state for the specified notification is cleared and the contents of the notification mailbox in question is returned.

##### `wait-notify`
( task -- )

If a task has not been notified, set the task in an awaiting notification state for which blocking has been prepared. If the task has already been notified, clear its notification state.

##### `wait-notify-critical`
( task -- )

If a task has not been notified, set the task in an awaiting notification state for blocking has been prepared, and immediately start a new critical section once it finishes blocking. If the task has already been notified, clear its notification state.

##### `prepare-block`
( task -- )

Prepare blocking timeouts for a given task.

##### `timed-out?`
( task -- timed-out )

Get whether a task has timed out.

##### `x-timed-out`
( -- )

An exception raised when timeout validation fails.

##### `validate-timeout`
( task -- )

Validate whether a task has timed out, raising `x-timed-out` if it has.

##### `x-would-block`
( -- )

Operation would block exception. Raised on attempting to carry out a non-blocking operation when blocking would normally be necessary for the equivalent blocking operation.

##### `task-priority!`
( priority task -- )

Set the priority of a task, from -32768 to 32767, with higher numbers being greater task priorities.

##### `task-priority@`
( task -- priority )

Get the priority of a task.

##### `x-out-of-range-priority`
( -- )

The exception raised when setting an out-of-range task priority

##### `task-saved-priority!`
( priority task -- )

Set the saved priority of a task, from -32768 to 32767, with higher numbers being greater task priorities.

##### `task-saved-priority@`
( task -- priority )

Get the saved priority of a task.

##### `task-timeslice!`
( timeslice task -- )

Set the timeslice, in ticks (usually 100 us increments), of a task, indicating the minimum amount of time a task will run before being preempted. If a task does not use up all of its timeslice before it gives up control of the processor, it will start off with the remainder of its timeslice next time it has control of the processor, unless it exhausted its timeslice, where then the timeslice value is added onto the tick counter (which may be negative) to yield the new timeslice (but which may not be less than the task's minimum timeslice). Note that the default setting for this for a newly initialized task is 10.

##### `task-timeslice@`
( task -- timeslice )

Get the timeslice, in ticks (usually 100 us increments), of a task.

##### `task-min-timeslice!`
( timeslice task -- )

Set the minimum timeslice, in ticks (usually 100 us increments), that a task will be guaranteed to run when scheduled, regardless of how many ticks the task executed for the last time it was scheduled. For instance, to ensure that each time a task will run for at least 10 ticks each time it is scheduled, this should be set to 10. By default, this value is set to 0 for each task, such that a given task is not guaranteed to not be descheduled immediately on a SysTick if it had already used up both its timeslice and also the next timeslice (through, e.g., spending time in critical sections).

##### `get-task-max-timeslice`
( task -- timeslice )

Getet the minimum timeslice, in ticks (usually 100 us increments), that a task will be guaranteed to run when scheduled, regardless of how many ticks the task executed for the last time it was scheduled.

##### `task-active@`
( task -- level )

Get the activation level of a task, with values 0 and lower indicating that a task is inactive, and values 1 and greater indicating that a task is active.

The simplest case of delaying a task is simply to execute:

##### `task-name@`
( task -- addr )

Get the name of a task as a counted string; an address of zero indicates no name is set.

##### `task-name!`
( addr task -- )

Set the name of a task as a counted string; an address of zero indicates to set no name.

##### `dump-tasks`
( -- )

Dump information for each task that is in the schedule.

##### `enable-trace`
( -- )

Enable dumping trace information when trace points are reached, i.e. the associated name of the trace point, the data stack of the task that hit the trace point, the critical depth at that time, and the states of all the tasks at at that point in time.

##### `disable-trace`
( -- )

Disable dumping trace information.

##### `trace-enable?`
( -- flag )

Get whether dumping trace information is enabled.

##### `trace`
( c-addr bytes -- )

Specify a trace point with the given name which is displayed when the trace point is reached.
