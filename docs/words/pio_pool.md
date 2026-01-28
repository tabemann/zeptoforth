# PIO/state machine pools

From release 1.16.0 on there is a PIO/state machine pool mechanism that provides a centralized means of allocating PIO blocks and state machines integrated with PIO memory management.

Multiple state machines on a single PIO block can be allocated at once with this mechanism. This is useful in cases where one needs multiple state machines to communicate with one another via IRQ's and thus limitations are imposed on which PIO's the state machines are on.

At the same time, this mechanism provides a means of simultaneously allocating PIO memory, ensuring that sufficient memory is available on a given PIO block and atomically allocating it if it is available.

Freeing state machines is provided for, but if the user wishes to free PIO memory they must do so manually with `pio::free-piomem`.

Last but not least, for the sake of convenience a means of installing a PIO program into all the allocated state machines at once is provided.

### `pio-pool` module

The `pio-pool` module contains the following words:

##### `x-invalid-sm-count`
( -- )

This exception is raised if the user attempts to allocate fewer than 1 or greater than 4 PIO state machines.

##### `x-unable-allocate-pio/sm`
( -- )

This exception is raised if a PIO/state machine word fails for any reason, e.g. insufficient free state machines on any single PIO block and/or no PIO block with sufficient free state machines has adequate PIO memory available.

##### `allocate-pio-sms`
( sm-count -- smn ... sm0 pio )

Allocate 1 to 4 state machines on a single PIO block and return the state machine indices and the PIO block.

##### `allocate-pio-sms-w-piomem`
( size sm-count -- smn ... sm0 base pio )

Allocate 1 to 4 state machines on a single PIO block, atomically allocating the specified PIO program size in that PIO block's memory (ensuring that the selected PIO block has sufficient space in the process), and returning the state machine indices, the allocated PIO memory base address, and the PIO block.

##### `allocate-pio-sms-w-prog`
( program sm-count -- smn ... sm0 base pio )

Allocate 1 to 4 state machines on a single PIO block, atomically allocating the specified PIO program's size in that PIO block's memory (ensuring that the selected PIO block has sufficient space in the process), installing the PIO program into the selected PIO block's memory, configuring the allocated state machines to execute the PIO program, and returning the state machine indices, the allocated PIO memory base address, and the PIO block.

##### `free-pio-sm`
( sm pio -- )

Free a single state machine for a specified PIO block.
