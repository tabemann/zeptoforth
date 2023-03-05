# Core Lock Words

Core locks provide a mutual exclusion mechanism between cores while allowing code executing on the same core to access the protected resource in a nested fashion. This is significant if one needs to protect access to a resource within a critical section on a single core, but one needs to ensure the ability to access that resource within an interrupt handler executing within the same resource. Unlike hardwære spinlocks such as that provided by the RP2040 core locks do not occupy any additional hardware resources, and do not require either some sort of allocation mechanism or a fixed set of pre-allocated hardware resources.

### `core-lock`

The following words are in `core-lock`:

##### `core-lock-size`
( -- bytes )

Get the size of a core lock in bytes.

##### `init-core-lock`
( addr -- )

Initialize a core lock starting at the specified address *addr*; note that it must be `core-lock-size` bytes in size.

##### `claim-core-lock`
( core-lock -- )

Claim a core lock *core-lock* for the current core. If it is already claimed for a core other than the current core, relinquish control of the current core and try again later repeatly until claiming *core-lock* is successful. If, when claimed for the current core it had already been claimed for the current core, increment a nested claim counter. Note that this is not safe to execute within an interrupt handler.

##### `claim-core-lock-spin`
( core-lock -- )

Claim a core lock *core-lock* for the current core. If it is already claimed for a core other than the current core, spin until it is no longer claimed by any core other the current one, and then claim it. If, when claimed for the current core it had already been claimed for the current core, increment a nested claim counter. Note that this is strictly-speaking safe to execute within an interrupt handler, but that is only advisable if *core-lock* is only claimed for very short periods of time, because it will cause the interrupt handler to wait until it cain claim *corelock*.

##### `claim-core-lock-timeout`
( core-lock -- )

Claim a core lock *core-lock* for the current core. If it is already claimed for a core other than the current core, relinquish control of the current core and try again later repeatly until claiming *core-lock* is successful or the current timeout is reached, where then `x-timed-out` is raised. If, when claimed for the current core it had already been claimed for the current core, increment a nested claim counter. Note that this is not safe to execute within an interrupt handler.

##### `release-core-lock`
( core-lock -- )

Release a core lock *core-lock*, decrementing its nested claim counter until it reaches zero. If all previous claims have been balanced, the *core-lock* may now be claimed by a task on another core.

##### `with-core-lock`
( xt core-lock -- )

Execute an execution token *xt* with a given core lock *core-lock* claimed while it executes, releasing it afterwards. If an exception is raised by *xt*, release *core-lock* and then re-raise the exception. Note that this uses the mechanism implemented in `claim-core-lock`, with all of its associated caveats.

##### `with-core-lock-spin`
( xt core-lock -- )

Execute an execution token *xt* with a given core lock *core-lock* claimed while it executes, releasing it afterwards. If an exception is raised by *xt*, release *core-lock* and then re-raise the exception. Note that this uses the mechanism implemented in `claim-core-lock-spin`, with all the associated caveats.
