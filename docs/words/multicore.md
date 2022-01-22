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

##### `spawn-on-core`
( xn ... x0 count xt dict-size stack-size rstack-size core -- task )

Allocate the space for a task (with *rstack-size* bytes of return stack space, *stack-size* bytes of data stack space, and *dict-size* bytes of dictionary space), place *count* cells on its data stack (*xn* through *x0*, with *x0* on the top of the data stack), and set it to execute *xt* on core *core*, booting the core if necessary. If the core is to be booted, the Systick is initiated on the booted core and the booted core is initialized for multitasking.

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

##### `claim-spinlock`
( spinlock-index -- )

Claim a spinlock; no validation is carried out.

##### `release-spinlock`
( spinlock-index -- )

Release a spinlock; no validation is carried out.

##### `claim-same-core-spinlock`
( -- )

Claim a multitasker spinlock for the current core.

##### `release-same-core-spinlock`
( -- )

Release a multitasker spinlock for the current core.

##### `claim-other-core-spinlock`
( core -- )

Claim a multitasker spinlock for the specified core.

##### `release-other-core-spinlock`
( core -- )

Release a multitasker spinlock for the specified core.

##### `claim-all-core-spinlock`
( -- )

Claim all the multitasker spinlocks.

##### `release-all-core-spinlock`
( -- )

Release all the multitasker spinlocks.

##### `with-spinlock`
( xt spinlock -- )

Claim a spinlock while executing *xt*, releasing it afterwards

##### `critical-with-spinlock`
( xt spinlock -- )

Claim a spinlock, enter a critical section, execute *xt*, release the spinlock, and then exit the critical section.

##### `critical-with-other-core-spinlock`
( xt core -- )

Claim a multitasker spinlock for the specified core, enter a critical section, execute *xt*, release the spinlock, and then exit the critical section.

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

##### `claim-spinlock`
( spinlock-index -- )

Drop *spinlock-index*.

##### `release-spinlock`
( spinlock-index -- )

Drop *spinlock-index*.

##### `claim-same-core-spinlock`
( -- )

This is a no-op.

##### `release-same-core-spinlock`
( -- )

This is a no-op.

##### `claim-other-core-spinlock`
( core -- )

Drop *core*.

##### `release-other-core-spinlock`
( core -- )

Drop *core*.

##### `claim-all-core-spinlock`
( -- )

This is a no-op.

##### `release-all-core-spinlock`
( -- )

This is a no-op.

##### `with-spinlock`
( xt spinlock -- )

Drop *spinlock* and execute *xt*.

##### `critical-with-spinlock`
( xt spinlock -- )

Drop *spinlock* and execute *xt* in a critical section.

##### `critical-with-other-core-spinlock`
( xt core -- )

Drop *core* and execute *xt* in a critical section.

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

