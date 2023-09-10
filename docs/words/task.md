# Multitasking Words

Multitasking in zeptoforth is not part of the zeptoforth kernel, but is provided by `src/common/forth/task.fs`, which in turn relies upon `src/<platform>/forth/int_io.fs` and `src/common/forth/systick.fs`. It is preemptive and priority-scheduled, but `PAUSE` may be called to explicitly relinquish control of the processor (which is highly recommended when possible); note that some words do this implicitly, such as `MS`.

Note that tasks in zeptoforth are a relatively heavy-weight asynchronous computing means. For lighter-weight asynchronous computing, consider creating a single task for running a scheduler within (so the main task can be devoted to the REPL), and then put all asynchronous actions within that.

There are a number of intertask communication and synchronization constructs available in full builds, including semaphores, locks, message-oriented queue channels, message-oriented rendezvous channels, byte-oriented streams, and last but not least, task notifications. All of these except for task notifications are discussed in other documentation pages.

Task notifications are the lightest-weight of all these mechanisms; simple synchronization between two tasks using task notifications take roughly 1/4 the time of using rendezvous channels, and task notifications have no extra memory overhead other than the mailboxes allocated by the user for each task, at one cell per mailbox whereas rendezvous channels take a minimum of 76 bytes of RAM. Mailboxes may only contain one cell each, and a task may have a maximum of 32 mailboxes for notifications. Notifications may set them to fixed values, update them with provided execution tokens, or leave them as-is. Note that improperly-written code using notifications may be subject to race conditions and data loss because no mechanism exists built-in to prevent a notification sent to a task in close succession after another notification from overwriting the contents of the mailbox in question. Also note that task notifications must be configured for a task `config-notify` before they may be used; the user must provide a pointer to a buffer containing a number of cells equal to the specified number of notification mailboxes.

Pending operations are scheduled operations that are executed within the multitasker when `pause` or `force-pending-ops` are executed, except within a critical section, where then they are defered. Pending operations are initialized and registered with `register-pending-op`, which gives them a fixed priority. Once registered, they can be set to execute with `set-pending-op`, which assigns an execution token to execute to them; this execution token will be cleared just prior to their execution. Pending operations are always of `pending-op-size` bytes. Note that it is safe to reuse a pending operation while it is executing, but if they are set again after they have already been set prior to their execution, their previously set execution token will be overwritten.

Multitasking is enabled by default once `src/common/forth/task.fs` has been loaded and the MCU has been rebooted; afterwards each time the MCU is booted a new task is created for the REPL, the main task, and multitasking is initiated.

The "attention" key combination Control-T `z` sends the exception `x-interrupt-main` to the main task. The "attention" key combination Control-T `t`, after the task monitor has been started with `start-monitor` in the `monitor` module, displays information on all the tasks running.

### `forth`

This word is in `forth`:

##### `pause`
( -- )

Pass off control to the next active task; if no tasks are active, put the MCU to sleep until an interrupt occurs (typically due to SysTick or USART activity).

### `task`

These words are in `task`:

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

##### `spawn-on-core`
( xn ... x0 count xt dict-size stack-size rstack-size core -- task )

Allocate the space for a task (with *rstack-size* bytes of return stack space, *stack-size* bytes of data stack space, and *dict-size* bytes of dictionary space), place *count* cells on its data stack (*xn* through *x0*, with *x0* on the top of the data stack), and set it to execute *xt* on core *core*, booting the core if necessary. If the core is to be booted, the Systick is initiated on the booted core and the booted core is initialized for multitasking.

The same as applies to `spawn` applies here.

##### `pause-wo-reschedule`
( -- )

Relinquish control of the current core without rescheduling the current task, i.e. reinserting it into the schedule. This ensures that once the task will not move in the schedule despite its giving up control of the current core.

##### `pause-reschedule-last`
( -- )

Relinquish control of the current core, rescheduling the current task to be last in the task queue regardless of its priority. This ensures that other tasks will be given the opportunity to execute on the current core even if they are of lower priority than the current task.

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

To raise an exception in a task, one executes:

##### `signal`
( xt task -- )

which readies the task and raises an exception within it, which may be caught by the task in question.

To force a task to call an execution token next time it is scheduled, one executes:

##### `force-call`
( xt task -- )

which sets control to be passed to the specified execution token. Note that said execution token must never return except by raising an exception or calling `abort`, `quit-reset`, or `quit` if it is the main task. The only other actions it may ultimately take are to enter into an infinite loop or to kill the task in question.

To terminate a task, one executes:

##### `kill`
( task -- )

which immediately halts that task's executing, including if it is the current task, and puts it in a terminated state. Note that the task's termination reason is set to `terminated-killed`.

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

##### `x-interrupt-main`
( -- )

This exception is sent to the main task by issuing Control-T `z` at the console.

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

##### `block-timeout`
( delay start task -- )

Block a task until *delay* ticks after the time represented by *start* ticks, and signal timed out if this time is reached without the task being readied first.

##### `block-wait`
( task -- )

Block a task until all waiting task are woken with `wake`.

##### `block-indefinite`
( task -- )

Block a task indefinitely until the task is readied.

##### `wait-notify-timeout`
( delay start notify-index -- x )

If the current task has not been notified for the specified notification, set the task in an awaiting notification state for that notification until *delay* ticks after the time represented by *start* ticks, and raise `x-timed-out` if this time is reached without the task being notified for that notification first. If successful, the notified state for the specified notification is cleared and the contents of the notification mailbox in question is returned.

##### `wait-notify-indefinite`
( notify-index -- x )

If the current task has not been notified for the specified notification, set the task in an awaiting notification state for that notification until the task is notified for that notification. If successful, the notified state for the specified notification is cleared and the contents of the notification mailbox in question is returned.

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

Notify a task on a specified notification index *notify-index*, updating the value of its notification mailbox at *notify-index* by applying the execution token *xt* with the signature ( x0 -- x1 ) to it, readying the task if it is currently waiting on that notification. Note that the code executed has full access to the contents of the stack below *xt* on it when `notify-update` was executed.

##### `clear-notify`
( notify-index task -- )

Clear a notification state for a specified notification index *notify-index* for a task *task*. A subsequent `wait-notify` etc. on this notification index will block until another notification occurs for this notification index even if a notifcation had occurred prior to calling `clear-notify`.

##### `block`
( task -- )

Block a task for which blocking has been prepared.

##### `mailbox@`
( notify-index task -- x )

Get the value for a mailbox at notification index *notify-index* for task *task* without blocking.

##### `mailbox!`
( x notify-index task -- )

Set the value for a mailbox at notification index *notify-index* for task *task* without setting the corresponding notification state and unblocking any wait on that notification index.

##### `wait-notify`
( notify-index -- x )

If the current task has not been notified for the specified notification, set the task in an awaiting notification state for that notification that may be blockign or indefinite depending on how blocking has been prepared. If successful, the notified state for the specified notification is cleared and the contents of the notification mailbox in question is returned.

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

Validate whether a task has timed out, raising `x-timed-out` if it has. Note that this clears the timeout state of the task.

##### `check-timeout`
( task -- timed-out? )

Get whether a task has timed out, clearing the timeout state of the task if it has.

##### `x-would-block`
( -- )

Operation would block exception. Raised on attempting to carry out a non-blocking operation when blocking would normally be necessary for the equivalent blocking operation.

##### `task-core@`
( task -- core )

Get the core that a task is running on or is set to run on.

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

##### `task-terminate-hook!`
( xt task -- )

Set a task's termination hook, to be invoked in the context of the task when the task terminates. The execution token invoked as the hook has the stack signature ( data reason -- ) where *data* is the task termination data associated with the task. Note that the task is re-initialized, aside from being with the same name and on the same core, before the task's termination hook is invoked, and will have a priority of 32768 (the maximum priority). Also note that a task termination hook of 0, the default, means that no task termination hook is to be invoked. *Reason* is the *immediate* task termination reason, which is overwritten with `not-terminated` due to the task being reinitialized when the task termination hook is called.

##### `task-terminate-hook@`
( task -- xt )

Get a task's termination hook.

##### `task-terminate-data!`
( data task -- )

Set a task's termination data, to be passed to the task's termination hook when it is invoked.

##### `task-terminate-data@`
( task -- data )

Get a task's termination data.

##### `task-terminate-immed-reason@`
( task -- reason )

Get a task's termination immediate reason. Note that this is set back to `not-terminated` if there is a task termination hook.

##### `task-terminate-reason@`
( task -- reason )

Get a task's termination reason.

Task termination reasons are either uncaught exceptions or one of the following:

##### `not-terminated`
( -- reason )

Task has not been terminated.

##### `terminated-normally`
( -- reason )

Task terminated normally.

##### `terminated-killed`
( -- reason )

Task was killed.

##### `terminated-crashed`
( -- reason )

Task was terminated due to a hardware exception.

##### `pending-op-size`
( -- bytes )

Size of a pending operation structure in bytes.

##### `register-pending-op`
( priority pending-op -- )

Initialize and register a pending operation *pending-op* with priority *priority* (greater values are higher priority).

##### `set-pending-op`
( xt pending-op -- )

Set a pending operation to execute *xt*; note that if it has already been set and not yet executed, *xt* will overwrite whichever execution token had already been set.

##### `force-pending-ops`
( -- )

Force pending operations to execute immediately.

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

### `monitor`

The following words are in the `monitor` module (note that this module is only present in full builds):

##### `start-monitor`
( -- )

This word starts the task monitor in the current core; if the task monitor has already been started, it has no effect. Once started, Control-T `t` dumps information on all running tasks.
