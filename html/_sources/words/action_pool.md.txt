# Action Pool words

Action pools provide a means of managing multiple schedule actions and dynamically creating and destroying them. Note that the total number of actions that may exist at any one time is fixed, but the number of executing actions may be of any number less than or equal to that at any point in time. Also note that action pools are associated with particular schedules when they are initialized.

The following words are in `action-pool-module`:

##### `x-no-action-available`
( -- )

Exception raised if one attempts to spawn a action from a action pool and no actions are available.

##### `init-from-action-pool`
( xt action-pool --  action )

Initalize a action from a action pool, if a action is available, or raise `x-no-action-available` if no actions are available. The action is initialized for the schedule associated with the action pool.

##### `action-pool-free`
( action-pool -- count )

Get the number of free actions in a action pool.

##### `init-action-pool`
( schedule count addr -- )

Initialize a action pool at *addr*, with *count* actions, for schedule *schedule*. These actions are not initialized initially, and are free to be initialized as needed.

##### `action-pool-size`
( count -- bytes )

Get the size taken up by a action pool with *count* actions; this size should be used for alloting the block of memory to be passed to `init-action-pool`
