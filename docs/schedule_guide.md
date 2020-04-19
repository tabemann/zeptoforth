# Scheduling Guide

Scheduling in zeptoforth is not part of the zeptoforth kernel, but is provided by `src/common/forth/schedule.fs`, which in turn relies upon `src/common/forth/systick.fs` and is normally combined with multitasking. It provides a lighter-weight means of asynchronous computing than multitasking, as each asynchronous action takes up far less RAM than a task does; however a scheduler comprising multiple asynchronous actions typically runs inside a task, so it can take advantage of automatic sleeping, and often that task is in addition to the main task, so the REPL can still be used while the scheduler operates.

The scheduler involves repeatedly executing words based on the specified timing delays which may be enabled and disabled, just like tasks. However unlike tasks, these actions do not have their own dictionaries, data stacks, or return stacks. Rather they share those belonging to the scheduler itself. Uncaught exceptions within a task will be handled, with its message being displayed.

#### current-action
( -- action )

The current action.

To create a new scheduler, execute:

#### create-schedule
( -- schedule )

Disposing of a scheduler involves disabling any task it is executing in, and then de-alloting the space alloted for it in the dictionary space.

To create a new action for a given scheduler, execute:

#### add-action
( xt schedule -- action )

where *xt* is the entry point of the action, *schedule* is the scheduler to create the action for, and *action* is the newly created action. Actions are disposed of for entire schedulers by the means that schedulers are disposed by.

New actions do not execute right away, rather to enable their execution, one executes:

#### enable-action
( action -- )

which increments the active counter for the action (which is initialized to zero); the action executes if this counter is greater than zero. To force a action to be enabled, one executes:

#### force-enable-action
( action -- )

which sets the active counter for the action to one if it is smaller than one.

In turn a action can be disabled with:

#### disable-action
( action -- )

which decrements the active counter for the action. To force a action to be disabled, one executes:

#### force-disable-action
( action -- )

which sets the active counter for the action to zero if is greater than zero.

The simplest case of delaying a action is simply to execute:

#### start-action-delay
( 1/10ms-delay action -- )

where *action* is the action to set the delay for, and *delay* is 10ths of milliseconds from the present.

To advance the time for the next delay from the last one for a action, execute:

#### advance-action-delay
( 1/10ms-delay action -- )

where *action* is the action to set the delay for, and *delay* is the new delay from the last delay for that action, in 10ths of milliseconds.

To advance the time for the next delay from the last one, or if it changed, set a new delay starting at the present, for a action, execute:

#### reset-action-delay
( 1/10ms-delay action -- )

where *action* is the action to set the delay for, and *delay* is the new delay from either the last delay for that action, or the present time, in 10ths of milliseconds.

To absolutely set the current delay for a action, execute:

#### set-action-delay
( 1/10ms-delay 1/10ms-start action -- )

where *action* is the action to set the delay for, *start* is the time the delay is from and *delay* is the delay from that time, in 10ths of milliseconds.

To absolutely get the current delay, execute:

#### get-action-delay
( action --  1/10ms-delay 1/10ms-start )

where *action* is the action to set the delay for, *start* is the time the delay is from and *delay* is the delay from that time, in 10ths of milliseconds.

To cancel the delay for the current action, execute:

#### cancel-action-delay
( action -- )

where *action* is the action to cancel the delay for. It is recommended to execute this for a action after the action has ceased to delay, so it does not delay again when `systick-counter` wraps around.