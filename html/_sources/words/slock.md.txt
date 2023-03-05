# Simple Lock Words

A simple lock enforces mutual exclusion, typically with regard to control of a resource, between multiple tasks. Unlike a normal lock, it does not establish a queue of multiple waiting tasks, but rather no particular order is established for which task claims the lock when multiple tasks are waiting on the lock to be released by another task. Simple locks are optimized for speed when claiming locks that are only held for very short periods of time, and particularly on multicore systems they should not be held for long because other tasks which attempt to claim them on other cores will spinwait until the lock is freed. Simple locks provide no means of avoiding priority inversion, which is another reason for avoiding holding them for long. Also, no means are provided to avoid double locking, so if a task double locks a simple lock it will hang forever until it is killed.

### `slock`

The following words are in `slock`:

##### `slock-size`
( -- bytes )

Get the size of a simple lock in bytes.

##### `init-slock`
( addr -- )

Initialize a simple lock starting at the specified address *addr*; note that it must be `slock-size` bytes in size.

##### `try-claim-slock`
( slock -- success? )

Attempt to claim a simple lock *slock* and return whether claiming it was successful.

##### `claim-slock`
( slock -- )

Attempt to claim a simple lock *slock*; if the simple lock is already held, continually attempt to claim the simple lock until it can be successfully claimed. In that case, if the simple lock is held by another task on the same core as the task attempting to claim it, give up the processor after each unsuccessful attempt to claim the simple lock; if it is held by another task on a different core, spinwait instead.

##### `release-slock`
( slock -- )

Release a simple lock *slock*. If other tasks are waiting to claim the simple lock, the first one to attempt to claim it next will claim it.

##### `with-slock`
( xt slock -- )

Execute an xt *xt* with a given simple lock *slock* locked while it executes, unlocking it afterwards. If an exception is raised by *xt*, unlock *lock* and then re-raise the exception.
