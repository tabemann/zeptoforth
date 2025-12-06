# Cycle Counting

On ARM Cortex-M4, ARM Cortex-M7, and ARM Cortex-M33 targets (i.e. all platforms supported other than RP2040 platforms) zeptoforth supports cycle counting in `full*` builds. Cycle counts are given as an unsigned 32-bit integer.

### `cycles`

The `cycles` module contains the following words:

##### `init-cycles`
( -- )

Start counting cycles from zero on the current core. Note that this does not apply to any other cores.

##### `cycle-counter`
( -- cycles )

Get the current cycle count for the current core as an unsigned 32-bit integer. `init-cycles` must have been called previously on the current core or else this will return zero.

##### `wait-cycles`
( cycles -- )

Wait a given number of cycles as a spinwait. Note that `init-cycles` must have been previously called on the current core or otherwise this word will not return.
