# Scheduler Words

##### current-action
( -- action )

The current action

##### create-schedule
( -- schedule )

Create a scheduler

##### add-action
( xt schedule -- action )

Add an action to a scheduler

##### enable-action
( action -- )

Enable an action

##### disable-action
( action -- )

Disable an action

##### force-enable-action
( action -- )

Force-enable an action

##### force-disable-action
( action -- )

Force-disable an action

##### start-action-delay
( 1/10m-delay action -- )

Start a delay from the present

##### set-action-delay
( 1/10ms-delay 1/10ms-start action -- )

Set a delay for an action

##### advance-action-delay
( 1/10ms-offset action -- )

Advance a delay for an action by a given amount of time

##### reset-action-delay
( 1/10ms-delay action -- )

Advance of start a delay from the present, depending on whether the delay
length has changed

##### get-action-delay
( action -- 1/10ms-delay 1/10ms-start )

Get a delay for an action

##### cancel-action-delay
( action -- )

Cancel a delay for an action

##### run-schedule
( schedule -- )

Run a schedule
