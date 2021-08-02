# Scheduling Words

Scheduling in zeptoforth is not part of the zeptoforth kernel, but is provided by `src/common/forth/schedule.fs`, which in turn relies upon `src/common/forth/systick.fs` and is normally combined with multitasking. It provides a lighter-weight means of asynchronous computing than multitasking, as each asynchronous action takes up far less RAM than a task does; however a scheduler comprising multiple asynchronous actions typically runs inside a task, so it can take advantage of automatic sleeping, and often that task is in addition to the main task, so the REPL can still be used while the scheduler operates.

The scheduler involves repeatedly executing words based on the specified timing delays which may be enabled and disabled, just like tasks. However unlike tasks, these actions do not have their own dictionaries, data stacks, or return stacks. Rather they share those belonging to the scheduler itself. Uncaught exceptions within a task will be handled, with its message being displayed.

These words are in `schedule-module`.

##### `current-action`
( -- action )

The current *action*.

To create a new scheduler, execute:

##### `create-schedule`
( -- scheduler )

Disposing of a scheduler involves disabling any task it is executing in, and then de-alloting the space alloted for it in the dictionary space.

To initialize an action for a given scheduler, execute:

##### `init-action`
( xt action scheduler -- )

where *xt* is the entry point of the action, *action* is the action to initialize, and *scheduler* is the scheduler to create the action for. Actions are disposed of for entire schedulers by the means that schedulers are disposed by.

New actions do not execute right away, rather to enable their execution, one executes:

##### `dispose-action`
( action -- )

which disposes of an action.

##### `action-disposed?`
( action -- disposed )

Returns whether *action* has been disposed.

##### `reset-action`
( xt action -- )

*action* is an action whose state is reset with the entry point *xt*.

##### `set-action-xt`
( xt action -- )

*action* is an action whose state is set to *xt*.

##### `enable-action`
( action -- )

which increments the active counter for the *action* (which is initialized to zero); the action executes if this counter is greater than zero.

To force a action to be enabled, one executes:

##### `force-enable-action`
( action -- )

which sets the active counter for the *action* to one if it is smaller than one.

In turn a action can be disabled with:

##### `disable-action`
( action -- )

which decrements the active counter for the *action*.

To force a action to be disabled, one executes:

##### `force-disable-action`
( action -- )

which sets the active counter for the *action* to zero if is greater than zero.

##### `action-delay`
( delay start action -- )

where *action* is the action to set the delay for, *start* ticks is the time the delay is from and *delay* ticks is the delay from that time.

To absolutely get the current delay, execute:

##### `action-delay-latest`
( action -- delay start )

where *action* is the action to set the delay for, *start* ticks is the time the delay is from and *delay* ticks is the delay from that time.

To cancel the delay for the current action, execute:

##### `cancel-action-delay`
( action -- )

where *action* is the action to cancel the delay for. It is recommended to execute this for a action after the action has ceased to delay, so it does not delay again when `systick-counter` wraps around.

##### `run-schedule`
( schedule -- )

Run *schedule* in the current task. This will not return except if an exception is raised.
