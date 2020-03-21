# Multitasker Words

#### main-task
( -- a-addr )
Main task

#### current-task
( -- a-addr )
Current task

#### pause-count
( -- a-addr )
Pause count

#### push-task-stack
( x task -- )

Push data onto a task's stack

#### push-task-rstack
( x task -- )

Push data onto a task's return stack

#### enable-task
( task -- )

Enable a task

#### force-enable-task
( task -- )

Force-enable a task

#### force-disable-task
( task -- )

Force-disable a task
  
#### spawn
( xt dict-size stack-size rstack-size -- task )

Spawn a non-main task

#### start-task-delay
( 1/10m-delay task -- )

Start a delay from the present

#### set-task-delay
( 1/10ms-delay 1/10ms-start task -- )

Set a delay for a task

#### advance-task-delay
( 1/10ms-offset task -- )

Advance a delay for a task by a given amount of time

#### reset-task-delay
( 1/10ms-delay task -- )

Advance of start a delay from the present, depending on whether the delay
length has changed

#### get-task-delay
( task -- 1/10ms-delay 1/10ms-start )

Get a delay for a task

#### cancel-task-delay
( task -- )

Cancel a delay for a task

#### ms
( u -- )

Wait for n milliseconds with multitasking support
