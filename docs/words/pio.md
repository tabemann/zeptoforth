# Programmable Input/Output Words

Programmable input/output (PIO) on the RP2040 (i.e. the Raspberry Pi Pico) provides a means to input to or output from pins in a very high-speed fashion at some speed up to the system clock of 125 MHz. There are two PIO peripherals, `PIO0` and `PIO1`, each of which contain four *state machines*.

PIO's may have up to 32 PIO instructions in their memory, which are 16 bits in size each. PIO state machines may be set to wrap from a *top* instruction to a *bottom* instruction automatically, unless a `jmp` instruction is executed at the *top* address. Instructions may be loaded into a PIO's instruction memory with `pio-instr-mem!`. Instructions may also be fed into a state machine to be executed immediately with `sm-instr!`. The address to execute PIO instructions at may be set for a state machine with `sm-addr!`

Up to four PIO state machines may be enabled, disabled, or reset at a time with `sm-enable`, `sm-disable`, or `sm-restart` respectively. These take a bitset of four bits where the position of each bit corresponds to the index of the state machine to enable, disable, or restart.

Each PIO state machine has an RX FIFO and a TX FIFO of four 32-bit values each. Note that the RX FIFO and TX FIFO on a PIO state machine may be joined into a single unidirectional FIFO consisting of eight 32-bit values. The RX FIFO for a state machine may be pushed to from a state machine's ISR register, which is 32-bits in size. The TX FIFO for a state machine may be pulled from to a state machine's OSR register, which is also 32-bits in size.

PIO state machines may automatically *pull* from its TX FIFO after a threshold number of bits have been shifted out of its OSR register. They may also automatically *push* to its RX FIFO after a threshold number of bits have been shifted into its ISR register.

The clock divider for a state machine is set with `sm-clkdiv!`, which takes a fractional component (from 0 to 255) and an integral component (from 0 to 65536) to divide the system clock by for the clock rate of the state machine in question. Note that an integral component of 0 is a special value which must be accompanied by a fractional component of 0 which indicates to disable the clock divider.

PIO state machines may either have an optional delay associated with each PIO instruction, or may have *sideset* enabled, where they may set the state of up to five output pins each cycle simultaneous with whatever other operations they are carrying out. `sm-delay-enable` is used to enable delay mode and `sm-sideset-enable` is to enable sideset mode.

PIO assembler words compile PIO instructions to `here` as 16 bits per instruction. There are two different basic types of PIO instructions - instructions without an associated delay or sideset, and instructions with an associated delay or sideset. The latter kind of instruction is marked with an `+` in its assembling word.

### `pio`

The `pio` module contains the following words:

#### Constants

##### `on`
( -- state )

On state.

##### `off`
( -- state )

Off state.

##### `right`
( -- direction )

Right direction.

##### `left`
( -- direction )

Left direction.

##### `PIO0`
( -- pio )

PIO0.

##### `PIO1`
( -- pio )

PIO1.

##### `IRQ0`
( -- irq )

IRQ0 index.

##### `IRQ1`
( -- irq )

IRQ1 index.

##### `PIO0_IRQ0`
( -- irq )

PIO0 IRQ0 index.

##### `PIO0_IRQ1`
( -- irq )

PIO0 IRQ1 index.

##### `PIO1_IRQ0`
( -- irq )

PIO1 IRQ0 index.

##### `PIO1_IRQ1`
( -- irq )

PIO1 IRQ1 index.

#### PIO Words

##### `pins-pio-alternate`
( pin-base pin-count -- )

Configure GPIO pins starting from *pin-base* of a count up to *pin-count* to be in an alternate function state such that the PIO may make use to them. Note that the pins wrap around, e.g. `29 3 pins-pio-alternate` configures GPIO pins 29, 0, and 1. Note that this is unnecessary when configuring pins with `sm-sideset-pins!`, `sm-set-pins!`, or `sm-out-pins!` as these internally call this word.

However, this is necessary with `sm-in-pin-base!` because any number of pins may be used by the `in` instruction and there exists no register to set specifying a number of pins to use with this instruction

##### `pio-instr-mem!`
( addr count pio -- )

Write a number of halfwords to instruction memory.

##### `pio-interrupt-enable`
( interrupt-bits irq pio -- )

Enable interrupts.

##### `pio-interrupt-disable`
( interrupt-bits irq pio -- )

Disable interrupts.

##### `pio-interrupt-enable-force`
( interrupt-bits irq pio -- )

Enable forcing interrupts.

##### `pio-interrupt-disable-force`
( interrupt-bits irq pio -- )

Disable forcing interrupts.

##### `pio-interrupt-raw@`
( pio -- interrupt-bits )

Get raw interrupts.

##### `pio-interrupt@`
( irq pio -- interrupt-bits )

Get interrupt status.

##### `INT_SM`
( state-machine -- index )

Interrupt bits.

##### `INT_SM_TXNFULL`
( state-machine -- index )

TXN not full interupt bits.

##### `INT_SM_RXNEMPTY`
( state-machine -- index )

RXN not empty interrupt bits.

#### State Machine Words

##### `sm-enable`
( state-machine-bits pio -- )

Enable state machines.

##### `sm-disable`
( state-machine-bits pio -- )

Disable state machines.

##### `sm-restart`
( state-machine-bits pio -- )

Restart state machines.

##### `sm-clkdiv!`
( fractional integral state-machine pio -- )

Set the clock divisor for a state machine.

##### `sm-addr!`
( address state-machine pio -- )

Set the address for a state machine.

##### `sm-wrap!`
( bottom-address top-address state-machine pio -- )

Set the wrapping of a state machine.

##### `sm-out-sticky!`
( sticky state-machine pio -- )

Set the sticky state of a state machine.

##### `sm-delay-enable`
( state-machine pio -- )

Set delay enable (sideset disable) state for a state-machine.

##### `sm-sideset-enable`
( state-machine pio -- )

Set sideset enable (delay disable) state for a state machine.

##### `sm-pull-threshold!`
( threshold state-machine pio -- )

Set OSR threshold before autopull or conditional pull will take place; value may be from 1 to 32.

##### `sm-push-threshold!`
( threshold state-machine pio -- )

Set ISR threshold before autopush or conditional push will take place; value may be from 1 to 32.

##### `sm-txf!`
( x state-machine pio -- )

Write to the TX FIFO of a state machine.

##### `sm-rxf@`
( x state-machine pio -- )

Read from the RX FIFO of a state machine.

##### `pins-pio-alternate`
( pin-base pin-count -- )

Set pins to PIO mode.

##### `sm-sideset-pins!`
( pin-base pin-count state-matchine pio -- )

Set the sideset pins for a state machine.

##### `sm-set-pins!`
( pin-base pin-count state-machine pio -- )

Set the SET pins for a state machine.

##### `sm-out-pins!`
( pin-base pin-count state-machine pio -- )

Set the OUT pins for a state machine.

##### `sm-in-pin-base!`
( pin-base state-machine pio -- )

Set the IN pin base for a state machine.

##### `sm-rx-fifo-level@`
( state-machine pio -- level )

Get a state machine RX FIFO level.

##### `sm-tx-fifo-level@`
( state-machine pio -- level )

Get a state machine TX FIFO level.

##### `sm-join-rx-fifo!`
( join? state-machine pio -- )

Set joining the RX FIFO and TX FIFO of a state machine into a single eight by 32-bit RX FIFO.

##### `sm-join-tx-fifo!`
( join? state-machine pio -- )

Set joining the RX FIFO and TX FIFO of a state machine into a single eight by 32-bit TX FIFO.

##### `sm-out-shift-dir`
( direction state-machine pio -- )

Set the output shift register direction.

##### `sm-in-shift-dir`
( direction state-machine pio -- )

Set the input shift register direction.

##### `sm-autopull!`
( on/off state-machine pio -- )

Set autopull on/off.

##### `sm-autopush!`
( on/off state-machine pio -- )

Set autopush on/off.

##### `sm-instr!`
( addr count state-machine pio -- )

Manually write instructions to a state machine.

#### PIO Assembler Words

##### `jmp,`
( address condition -- )

PIO JMP instruction.

##### `wait,`
( index source polarity -- )

PIO WAIT instruction.

##### `in,`
( bit-count source -- )

PIO IN instruction.

##### `out,`
( bit-count destination -- )

PIO OUT instruction.

##### `push,`
( block if-full -- )

PIO PUSH instruction.

##### `pull,`
( block if-empty -- )

PIO PULL instruction.

##### `mov,`
( source op destination -- )

PIO MOV instruction.

##### `irq,`
( index set/wait -- )

PIO IRQ instruction.

##### `set,`
( data destination -- )

PIO SET instruction.

##### `jmp+,`
( address delay/side-set condition -- )

PIO JMP instruction with delay or side-set.

##### `wait+,`
( index delay/side-set source polarity -- )

PIO WAIT instruction with delay or side-set.

##### `in+,`
( bit-count delay/side-set source -- )

PIO IN instruction with delay or side-set.

##### `out+,`
( bit-count delay/side-set destination -- )

PIO OUT instruction with delay or side-set.

##### `push+,`
( delay/side-set block if-full -- )

PIO PUSH instruction with delay or side-set.

##### `pull+,`
( delay/side-set block if-empty -- )

PIO PULL instruction with delay or side-set.

##### `mov+,`
( source delay/side-set op destination -- )

PIO MOV instruction with delay or side-set.

##### `irq+,`
( index delay/side-set set/wait -- )

PIO IRQ instruction with delay or side-set.

##### `set+,`
( data delay/side-set destination -- )

PIO SET instruction with delay or side-set.

##### `COND_ALWAYS`
( -- condition )

Always jump.

##### `COND_X0=`
( -- condition )

Jump if scratch X is zero.

##### `COND_X1-`
( -- condition )

Jump if scratch X is non-zero, post-decrement.

##### `COND_Y0=`
( -- condition )

Jump if scratch Y is zero.

##### `COND_Y1-`
( -- condition )

Jump if scratch Y is non-zero, post-decrement.

##### `COND_XY<>`
( -- condition )

Jump if scratch X not equal scratch Y.

##### `COND_PIN`
( -- condition )

Jump on input pin.

##### `COND_IOSRE`
( -- condition )

Jump on output shift register not empty.

##### `WAIT_GPIO`
( -- wait )

Wait for GPIO.

##### `WAIT_PIN`
( -- wait )

Wait for a pin.

##### `WAIT_IRQ`
( -- wait )

Wait for an IRQ.

##### `IN_PINS`
( -- in-source )

Pins input.

##### `IN_X`
( -- in-source )

Scratch register X input.

##### `IN_Y`
( -- in-source )

Scratch register Y input.

##### `IN_NULL`
( -- in-source )

NULL input (all zeros).

##### `IN_ISR`
( -- in-source )

ISR input.

##### `IN_OSR`
( -- in-source )

OSR input.

##### `OUT_PINS`
( -- out-destination )

Pins output.

##### `OUT_X`
( -- out-destination )

Scratch register X output.

##### `OUT_Y`
( -- out-destination )

Scratch register Y output.

##### `OUT_NULL`
( -- out-destination )

NULL output (discard data).

##### `OUT_PINDIRS`
( -- out-destination )

PINDIRs output.

##### `OUT_PC`
( -- out )

PC output (unconditional jump to shifted address).

##### `OUT_ISR`
( -- out )

ISR output (also sets ISR shift counter to bit count).

##### `OUT_EXEC`
( -- out )

Execute OSR shift data as instruction.

##### `PUSH_NOT_FULL`
( -- push-if-full-option )

Push data even if threshold is not met.

##### `PUSH_IF_FULL`
( -- push-if-full-option )

Do nothing unless the total input shift count has reached its threshold.

##### `PUSH_NO_BLOCK`
( -- push-block-option )

Do not stall execution if RX FIFO is full, instead drop data from ISR.

##### `PUSH_BLOCK`
( -- push-block-option )

Stall execution if RX FIFO is full.

##### `PULL_NOT_EMPTY`
( -- pull-if-full-option )

Pull data even if threshold is not met.

##### `PULL_IF_EMPTY`
( -- pull-if-full-option )

Do nothing unless the total output shift count has reached its threshold.

##### `PULL_NO_BLOCK`
( -- pull-block-option )

Do not stall execution if TX FIFO is empty, instead copy from scratch X.

##### `PULL_BLOCK`
( -- pull-block-option )

Stall execution if TX FIFO is empty.

##### `MOV_DEST_PINS`
( -- mov-destination )

Move to PINS.

##### `MOV_DEST_X`
( -- mov-destination )

Move to scratch register X.

##### `MOV_DEST_Y`
( -- mov-destination )

Move to scratch register Y.

##### `MOV_DEST_EXEC`
( -- mov-destination )

Move to EXEC (execute data as instruction).

##### `MOV_DEST_PC`
( -- mov-destination )

Move to PC (treat data as address for unconditional branch).

##### `MOV_DEST_ISR`
( -- mov-destination )

Move to ISR (input shift counter is reset to 0, i.e. empty).

##### `MOV_DEST_OSR`
( -- mov-destination )

Move to OSR (output shift counter is reset to 0, i.e. full).

##### `MOV_OP_NONE`
( -- mov-op )

Move operation none.

##### `MOV_OP_INVERT`
( -- mov-op )

Move operation invert.

##### `MOV_OP_REVERSE`
( -- mov-op )

Move operation bit-reverse.

##### `MOV_SRC_PINS`
( -- mov-op )

Move from PINS.

##### `MOV_SRC_X`
( -- mov-source )

Move from scratch register X.

##### `MOV_SRC_Y`
( -- mov-source )

Move from scratch register Y.

##### `MOV_SRC_NULL`
( -- mov-source )

Move from NULL.

##### `MOV_SRC_STATUS`
( -- mov-source )

Move from STATUS.

##### `MOV_SRC_ISR`
( -- mov-source )

Move from ISR.

##### `MOV_SRC_OSR`
( -- mov-source )

Move from OSR.

##### `IRQ_SET`
( -- irq-op )

Raise an IRQ.

##### `IRQ_CLEAR`
( -- irq-op )

Clear an IRQ.

##### `IRQ_WAIT`
( -- irq-op )

Wait for an IRQ to be lowered.

##### `SET_PINS`
( -- set-destination )

Set PINS.

##### `SET_X`
( -- set-destination )

Set scratch register X (5 LSBs are set to data, all others are cleared).

##### `SET_Y`
( -- set-destination )

Set scratch register Y (5 LSBs are set to data, all others are cleared).

##### `SET_PINDIRS`
( -- set-destination )

Set PINDIRS.

##### `REL`
( irq -- irq-rel )

Add the state machine ID to the lower two bits of the IRQ index, by way of module-4 addition on the two LSB's.

#### Exceptions

##### `x-sm-out-of-range`
( -- )

State machine out of range exception.

##### `x-pio-out-of-range`
( -- )

PIO out of range exception.

##### `x-too-many-instructions`
( -- )

Too many instructions exception.

##### `x-address-out-of-range`
( -- )

Address out of range exception.

##### `x-too-many-pins`
( -- )

Too many pins exception.

##### `x-pin-out-of-range`
( -- )

Pin out of range exception.

##### `x-clkdiv-out-of-range`
( -- )

Clock divisor out of range exception.

##### `x-irq-out-of-range`
( -- )

IRQ out of range exception.

##### `x-interrupt-out-of-range`
( -- )

Interrupt out of range exception.

##### `x-threshold-out-of-range`
( -- )

Buffer threshold out of range exception.
