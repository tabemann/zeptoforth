# Action Pool words

Action pools provide a means allocating and reusing action scheduler actions out of a fixed sized pool. Actions that have not been added to a schedule or which have been removed from a schedule, whether manually or automatically (e.g. through failing to specify a continuation or through raising an uncaught exception) are considered free for allocation. Actions' initial execution tokens and data are assigned at the time that they are added to an action, unlike when manually initializing actions and adding them to schedules where the two are separate operations.

### `action-pool`

The following words are in `action-pool`:

##### `x-no-action-available`
( -- )

Exception raised if one attempts to allocate a action from a action pool and no actions are available.

##### `add-action-from-pool`
( schedule dat xt action-pool --  action )

Initalize a action from a action pool with the initial execution token *xt* and data *data*, if a action is available, or raise `x-no-action-available` if no actions are available. The action is then added to the schedule *schedule* and is returned.

##### `action-pool-free`
( action-pool -- count )

Get the number of free actions in a action pool.

##### `init-action-pool`
( count addr -- )

Initialize a action pool at *addr*, with *count* actions. These actions are free initially to be initialized and added to a schedule with `add-action-from-pool`.

##### `action-pool-size`
( count -- bytes )

Get the size taken up by a action pool with *count* actions; this size should be used for alloting the block of memory to be passed to `init-action-pool`
