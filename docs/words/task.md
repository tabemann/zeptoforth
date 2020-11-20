# Multitasking Words

Multitasking in zeptoforth is not part of the zeptoforth kernel, but is provided by `src/common/forth/task.fs`, which in turn relies upon `src/<platform>/forth/int_io.fs` and `src/common/forth/systick.fs`. It is preemptivee, but `PAUSE` may be called to explicitly relinquish control of the processor (which is highly recommended when possible); note that some words do this implicitly, such as `MS`, `KEY`, or `EMIT` (which are in turn called by words such as `REFILL` or `TYPE`).

Note that task in zeptoforth are a relatively heavy-weight asynchronous computing means. For lighter-weight asynchronous computing, consider creating a single task for running a scheduler within (so the main task can be devoted to the REPL), and then put all asynchronous actions within that.

Multitasking is enabled by default once `sys/common/forth/task.fs` has been loaded and the MCU has been rebooted; afterwards each time the MCU is booted a new task is created for the REPL, the main task, and multitasking is initiated.

These words are in `task-wordlist`.

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
( xt dictionary-size stack-size rstack-size -- task )

where *xt* is the entry point to the task, *dictionary-size* is the size of the dictionary for the spawned task in bytes, *stack-size* is the size of the data stack for the spawned task in bytes, *rstack-size* is the size of the return stack for the spawned task in bytes, and *task* is the header for the newly spawned task. *dictionary-size*, *stack-size*, and *rstack-size* must be multiples of four I have had good luck with values of 256 for each of these; I do not know how well smaller values will work out, especially in the case of *rstack-size*, where too small of a size will almost certainly result in a crash. This in turn returns a pointer to the task header on the stack, which can then be stored in a variable or constant.

Note that it is not recommended to execute this while compiling to flash; rather, it should be put in the `init` routine and then the result should be stored to a variable in RAM.

Note that tasks may be enabled or disabled but once created exist until the MCU is rebooted.

New task default to a priority of zero; to change this use `set-task-priority`.

Uncaught exceptions within a task will be handled, with the message for them being displayed, but they will result in said task being disabled. Note that reenabling such a task will result in a crash.

New tasks do not execute right away, rather to enable their execution, one executes:

##### `enable-task`
( task -- )

which increments the active counter for the *task* (which is initialized to zero); the task executes if this counter is greater than zero.

To force a task to be enabled, one executes:

##### `force-enable-task`
( task -- )

which sets the active counter for the *task* to one if it is smaller than one.

In turn a task can be disabled with:

##### `disable-task`
( task -- )

which decrements the active counter for the *task*.

To force a task to be disabled, one executes:

##### `force-disable-task`
( task -- )

which sets the active counter for the *task* to zero if is greater than zero.

##### `set-task-priority`
( priority task -- )

Set the priority of a task, from -32768 to 32767, with higher numbers being greater task priorities.

##### `get-task-priority`
( task -- priority )

Get the priority of a task.

##### `x-out-of-range-priority`
( -- )

The exception raised when setting an out-of-range task priority

##### `get-task-active`
( task -- level )

Get the activation level of a task, with values 0 and lower indicating that a task is inactive, and values 1 and greater indicating that a task is active.

The simplest case of delaying a task is simply to execute:

##### `start-task-delay`
( 1/10ms-delay task -- )

where *task* is the task to set the delay for, and *delay* is 10ths of milliseconds from the present.

To advance the time for the next delay from the last one for a task, execute:

##### `advance-task-delay`
( 1/10ms-delay task -- )

where *task* is the task to set the delay for, and *delay* is the new delay from the last delay for that task, in 10ths of milliseconds.

To advance the time for the next delay from the last one, or if it changed, set a new delay starting at the present, for a task, execute:

##### `reset-task-delay`
( 1/10ms-delay task -- )

where *task* is the task to set the delay for, and *delay* is the new delay from either the last delay for that task, or the present time, in 10ths of milliseconds.

To absolutely set the current delay for a task, execute:

##### `set-task-delay`
( 1/10ms-delay 1/10ms-start task -- )

where *task* is the task to set the delay for, *start* is the time the delay is from and *delay* is the delay from that time, in 10ths of milliseconds.

To absolutely get the current delay, execute:

##### `get-task-delay`
( task --  1/10ms-delay 1/10ms-start )

where *task* is the task to set the delay for, *start* is the time the delay is from and *delay* is the delay from that time, in 10ths of milliseconds.

To cancel the delay for the current task, execute:

##### `cancel-task-delay`
( task -- )

where *task* is the task to cancel the delay for. It is recommended to execute this for a task after the task has ceased to delay, so it does not delay again when `systick-counter` wraps around.

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
