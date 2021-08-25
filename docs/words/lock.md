# Lock Words

A lock enforces mutual exclusion, typically with regard to control of a resource, between multiple tasks. It also serves as a queue for multiple tasks waiting on the lock which is held by another task. Furthermore, it provides a mechanism to avoid priority inversion, by temporarily elevating each task holding a lock's priority to the maximum priority of all the tasks waiting for it; this applies even when chaining together multiple locks held by tasks holding some locks and waiting on others.

The following words are in `lock-module`:

##### `lock-size`
( -- bytes )

Get the size of a lock in memory.

##### `init-lock`
( addr -- )

Initialize a lock starting at the specified address; note that it must be `lock-size` bytes in size.

##### `lock`
( lock -- )

Attempt to acquire a lock; if the lock is already held, put the current task in a queue and disable it. In that case, update the priority of the holder of a lock, and any subsequent holders of any locks waited for by the holder of this lock, in order to avoid priority inversion. Note that this must not be called within a critical section.

##### `unlock`
( lock -- )

Attempt to release a lock; if the current task is not the holder of the lock, raise `x-not-currently-owned`. If releasing the lock is successful, restore the priority of the current task to what it would be had its priority not been updated in order to avoid priority inversion; afterwards, carry out all the functionality that `lock` would carry out had it been called by the task at the head of the queue if there is one. Note that this must not be called within a critical section.

##### `with-lock`
( xt lock -- )

Execute an xt with a given lock locked while it executes, unlocking it afterwards. Exceptions are handled properly with unlocking occurring of it occurs within the xt.

##### `x-not-currently-owned`

Exception raised if a task attempts to unlock a lock it is not the holder of.

##### `update-lock-priority`
( lock -- )

Update the priority of the holder of a lock, and the priority of the holder of a lock waited on by the holder of this lock, and so on, in order to avoid priority inversion. Note that this must not be called within a critical section.
