# Multicore Words

Multiprocessing via multicore execution in zeptoforth is handled separtely from multitasking; separate multitasking environments exist on each core after a second core has been booted. Once a second core has been booted, any attempt to directly control the multitasking environment of another core will result in undefined behavior.

Currently the only platform on which multicore execution is supported is the RP2040. On this platform cores communicate and synchronize via hardware spinlocks and hardware FIFO's (i.e. mailboxes). On platforms other than the RP2040 multicore words exist in the dictionary but are merely stubs which will always raise exceptions when called.

Note that there are currently some rough edges to multicore on the RP2040, specifically that once the second core is booted, no writes to flash may be made, even across reboots, until the MCU is power-cycled, and that `disable-int-io` in `int-io` must be called prior to calling `spawn-aux-main`.

Multicore support in zeptoforth is largely implemented outside the zeptoforth kernel and is split between `src/rp2040/forth/multicore.fs` (on platforms other than RP2040 stubbed out in `src/common/forth/multicore.fs`) and `src/common/forth/task.fs`, with a bit of supporting functionality also in `src/common/forth/systick.fs` and `src/rp2040/forth/erase.fs`.

### `forth`

Built into the kernel exists:

##### `cpu-count`
( -- count )

Get the core count.

##### `cpu-index`
( -- index )

Get the index of the core from which `cpu-index` was called.

##### `cpu-offset`
( -- offset )

Return 4 times `cpu-index`.

In `src/common/forth/basic.fs`, in `forth`, exists:

##### `cpu-variable`

( "global-name" "cpu-name" -- )

Compile a one-cell-per-core variable into flash that has two words referring to it, a *global-name* which takes a core index when called and outputs the address for that core, and a *cpu-name* which returns its address for the current core.

### `task`

In `src/common/forth/task.fs`, in `task`, exists:

##### `spawn-aux-main`
( xn ... x0 count xt dict-size stack-size rstack-size core -- )

Allocate the space for a main task (with *rstack-size* bytes of return stack space, *stack-size* bytes of data stack space, and *dict-size* bytes of dictionary space), place *count* cells on its data stack (*xn* through *x0*, with *x0* on the top of the data stack), and boot core *core* (which currently must be 1, otherwise `x-core-out-of-range` is raised) with it, executing *xt* on it. In the process the Systick is initiated on the booted core and the booted core is initialized for multitasking. Note that this can only be called from core 0 (i.e. the core executing on initial boot), otherwise `x-core-can-only-be-launched-from-core-0` is raised. Once booted the core in question cannot be booted again until the RP2040 as a whole is rebooted, or else `x-main-already-launched` is raised. Note that once a main task is created on a core it behaves like a normal task for multitasking purposes on that core; it can be killed, for instance.

##### `x-main-already-launched`
( -- )

Exception raised if one calls `spawn-aux-main` for a core which has already been booted.

##### `x-core-can-only-be-launched-from-core-0`
( -- )

Exception raised if one attempts to call `spawn-aux-main` from a core other than core 0.

### `multicore`

In both `src/rp2040/forth/multicore.fs` and `src/common/forth/multicore.fs`, in `multicore` on all platforms, exists:

##### `x-spinlock-out-of-range`
( -- )

Spinlock out of range exception.

##### `x-core-out-of-range`
( -- )

Core out of range exception, i.e. core index not in range 0 \<= core \< cpu-count.

##### `x-core-not-addressable`
( -- )

Core not addressable exception, i.e. invalid core for an operation carried out ont the current core.

In `src/rp2040/forth/multicore.fs`, in `multicore` on the `rp2040` platform, exists:

##### `sev`
( -- )

Signal an event to the other core.

##### `wfe`
( -- )

Wait for an event.

##### `SIO_IRQ_PROC0`
( -- irq )

SIO processor 0 IRQ index, i.e. 15.

##### `SIO_IRQ_PROC1`
( -- irq )

SIO processor 1 RIQ index, i.e. 16.

##### `FIFO_ST`
( -- addr )

FIFO status register; note that core 0 can see the read side of the 1 -> 0
FIFO and the write side of the 0 -> 1 FIFO; the converse is true of core 1

##### `FIFO_ST_ROE`
( -- bit )

Sticky flag indicating the RX FIFO was read when empty; write to clear.

##### `FIFO_ST_WOF`
( -- bit )

Sticky flag indicating the TX FIFO was written when full; write to clear.

##### `FIFO_ST_VLD`
( -- bit )

Set if this core's RX FIFO is not empty.

##### `FIFO_WR`
( -- bit )

Write access from this core's TX FIFO.

##### `FIFO_RD`
( -- bit )

Read access to this core's RX FIFO.

##### `SPINLOCK`
( index -- addr )

Returns the address of a spinlock with a given index. Note that reading from an spinlock claims it if a value other than 0 is read (which will be the index of the spinlock), with a value of 0 indicating that the spinlock is already held, and writing to a spinlock releases it.

##### `spinlock-count`
( -- count )

Returns the spinlock count, i.e. 32.

##### `task-spinlock`
( -- )

The index of the multitasking spinlock, i.e. 31.

##### `claim-spinlock`
( spinlock-index -- )

Claim a spinlock; if *spinlock-index* is outside the range 0 <= x < 32 `x-spinlock-out-of-range` is raised.

##### `release-spinlock`
( spinlock-index -- )

Release a spinlock; if *spinlock-index* is outside the range 0 <= x < 32 `x-spinlock-out-of-range` is raised.

##### `fifo-drain`
( core -- )

Drain a FIFO for inter-core communication; if *core* is outside the range 0 <= x < 2 `x-core-out-of-range` is raised; if *core* is equal to `cpu-index` `x-core-not-addressable` is raised.

##### `fifo-push-blocking`
( x core -- )

Do a blocking push onto a FIFO for inter-core communication; if *core* is outside the range 0 <= x < 2 `x-core-out-of-range` is raised; if *core* is equal to `cpu-index` `x-core-not-addressable` is raised.

##### `fifo-pop-blocking`
( core -- x )

Do a blocking pop from a FIFO for inter-core communication; if *core* is outside the range 0 <= x < 2 `x-core-out-of-range` is raised; if *core* is equal to `cpu-index` `x-core-not-addressable` is raised.

##### `fifo-push-confirm`
( x core -- confirmed? )

Do a blocking push onto a FIFO for inter-core communication; if *core* is outside the range 0 <= x < 2 `x-core-out-of-range` is raised; if *core* is equal to `cpu-index` `x-core-not-addressable` is raised. Afterwards do a blocking pop from the FIFO for receiving fromt he same core and return whether the value pushed is the same as the value popped.

##### `launch-aux-core`
( vector-table stack-ptr rstack-ptr entry-xt core -- )

Launch an auxiliary core, i.e. a core *core* other than core 0 and execute *entry-xt* on it with the return stack pointer *rstack-ptr*, the data stack pointer *stack-ptr*, and the vector table base *vector-table*. Note that it is not recommended that this be used by the user, rather the user should use `spawn-aux-main` in `src/common/forth/task.fs`.

In `src/common/forth/multicore.fs`, in `multicore` for all platforms other than rp2040, exists:

##### `spinlock-count`
( -- count )

Returns the spinlock count, i.e. 0.

##### `task-spinlock`
( -- )

Returns -1 as there is no multitasking spinlock.

##### `claim-spinlock`
( spinlock-index -- )

Placeholder for claiming a spinlock; this will always raise `x-spinlock-out-of-range`.

##### `release-spinlock`
( spinlock-index -- )

Placeholder for releasing a spinlock; this will always raise `x-spinlock-out-of-range`.

##### `fifo-drain`
( core -- )

Placeholder for draining a FIFO for inter-core communication; this will always raise `x-core-out-of-range`.

##### `fifo-push-blocking`
( x core -- )

Placeholder for doing a blocking push onto a FIFO for inter-core communication; this will always raise `x-core-out-of-range`.

##### `fifo-pop-blocking`
( core -- x )

Placeholder for doing a blocking pop from a FIFO for inter-core communication; this will always raise `x-core-out-of-range`.

##### `fifo-push-confirm`
( x core -- confirmed? )

Placeholder for attempting to send data on a FIFO and confirming that the same data is sent back; this will always raise `x-core-out-of-range`.

##### `launch-aux-core`
( xt stack-ptr rstack-ptr core -- )

Placeholder for attempting to launch an auxiliary core; this will always raise `x-core-out-of-range`.

