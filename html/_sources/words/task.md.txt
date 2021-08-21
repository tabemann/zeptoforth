# Multitasking Words

Multitasking in zeptoforth is not part of the zeptoforth kernel, but is provided by `src/common/forth/task.fs`, which in turn relies upon `src/<platform>/forth/int_io.fs` and `src/common/forth/systick.fs`. It is preemptivee, but `PAUSE` may be called to explicitly relinquish control of the processor (which is highly recommended when possible); note that some words do this implicitly, such as `MS`, `KEY`, or `EMIT` (which are in turn called by words such as `REFILL` or `TYPE`).

Note that task in zeptoforth are a relatively heavy-weight asynchronous computing means. For lighter-weight asynchronous computing, consider creating a single task for running a scheduler within (so the main task can be devoted to the REPL), and then put all asynchronous actions within that.

Multitasking is enabled by default once `src/common/forth/task.fs` has been loaded and the MCU has been rebooted; afterwards each time the MCU is booted a new task is created for the REPL, the main task, and multitasking is initiated.

These words are in `task-module`.

##### `pause`
( -- )

Pass off control to the next active task; if no tasks are active, put the MCU to sleep until an interrupt occurs (typically due to SysTick or USART activity).

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

New task default to a priority of zero; to change this use `set-task-priority`.

Uncaught exceptions within a task will be handled, with the message for them being displayed, but they will result in said task being terminated.

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

This exception when executed displays the task terminated error message.

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

##### `ready`
( task -- )

Ready a blocked or delayed task.

##### `block`
( task -- )

Block a task for which blocking has been prepared.

##### `block-critical`
( task -- )

Block a task for which blocking has been prepared, and immediately start a new critical section once it finishes blocking.

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

##### `set-task-priority`
( priority task -- )

Set the priority of a task, from -32768 to 32767, with higher numbers being greater task priorities.

##### `get-task-priority`
( task -- priority )

Get the priority of a task.

##### `x-out-of-range-priority`
( -- )

The exception raised when setting an out-of-range task priority

##### `set-task-saved-priority`
( priority task -- )

Set the saved priority of a task, from -32768 to 32767, with higher numbers being greater task priorities.

##### `get-task-saved-priority`
( task -- priority )

Get the saved priority of a task.

##### `set-task-timeslice`
( timeslice task -- )

Set the timeslice, in 100 us increments, of a task, indicating the minimum amount of time a task will run before being preempted. Note that the default setting for this for a newly initialized task is 10.

##### `get-task-timeslice`
( task -- timeslice )

Get the timeslice, in 100 us increments, of a task.

##### `set-task-max-timeslice`
( timeslice task -- )

Set the maximum timeslice, in 100 us increments of a task, indicating the maximum amount of time a task will run before being preempted; note that if the timeslice of a task is larger than this amount, the maximum timeslice of the task will be treated as being the same as the task's timeslice. Also note that the default setting for this for a newly initialized task is 20.

##### `get-task-max-timeslice`
( task -- timeslice )

Get the maximum timeslice, in 100 us increments, of a task.

##### `get-task-active`
( task -- level )

Get the activation level of a task, with values 0 and lower indicating that a task is inactive, and values 1 and greater indicating that a task is active.

The simplest case of delaying a task is simply to execute:

##### `task-checksum`
( task -- checksum )

Calculate a checksum for a given task.

##### `dump-task-info`
( task -- )

Print out information describing a task.

##### `dump-task-stack`
( task -- )

Print a hex dump of the contents of a task's stack, excluding the top of the stack.

##### `dump-task-rstack`
( task -- )

Print a hex dump of the contents of a task's return stack.

##### `dump-task-dict`
( task -- )

Print a hex dump of the contents of a task's dictionary

##### `dump-task-regs`
( task -- )

Print out the saved registers for a task; note that the task must not be the current task. Note that R6 contains the top of the stack, and R7 contains the current stack pointer.
