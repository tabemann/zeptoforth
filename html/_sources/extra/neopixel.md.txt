# Neopixels on the RP2040 and RP2350

There is optional support for Neopixels and Neopixel strips on the RP2040 and RP2350 using a PIO block to drive the Neopixel protocol.

Configuring the Neopixel is as simple as alloting a Neopixel data structure for a given number of Neopixels and initializing it with a Neopixel data GPIO pin, a Neopixel count, a PIO state machine index, and a PIO. Once one has done that, `neopixel!` can be used to specify the colors of individual Neopixels (with RGB colors, with each element having values from 0 to 255), and once the colors have been specified one transmits them to the Neopixel strip with `update-neopixel`.

Note that in some setups, such as the SeeedStudio XIAO RP2040, a separate GPIO pin may need to be set high to enable a Neopixel (e.g. GPIO pin 11 on the SeeedStudio XIAO RP2040, in addition to the data GPIO pin 12). Use the `pin!` word in the `pin` module for this.

Neopixel support is in `extra/rp2040/neopixel.fs`, which can be compiled either to RAM or to flash as needed.

### `neopixel`

The `neopixel` module contains the following words:

##### `init-neopixel`
( state-machine pio count pin addr -- )

Initialize a Neopixel structure at *addr* for *count* Neopixels, data GPIO pin *pin*, PIO block *pio* (`PIO0` or `PIO1` from the `pio` module), and state machine *state-machine* (from 0 to 3). This does not take into account any power GPIO pin that may need to be configured. It reprograms the specified PIO block and assumes that it has full control over said block. Note that it is safe to use if one wishes to use multiple state machines that are all controlling Neopixels in one PIO block, because it will only overwrite the PIO program with the very same PIO program.

##### `clear-neopixel`
( neopixel -- )

Clear all the Neopixel states in Neopixel structure *neopixel*.

##### `update-neopixel`
( neopixel -- )

Transmit all the stored Neopixel color data for the Neopixels in Neopixel structure *neopixel*.

##### `neopixel!`
( red green blue index neopixel -- )

Specify the color of Neopixel *index* in the Neopixel structure *neopixel* as *red*, *green*, and *blue* color components from 0 to 255. Note that this does *not* transmit the color data to the Neopixel in question; rather one must call `update-neopixel` to do so.

##### `neopixel@`
( index neopixel -- red green blue )

Get the color of Neopixel *index* in the Neopixel structure *neopixel* as *red*, *green*, and *blue* color components from 0 to 255.

##### `neopixel-size`
( count -- bytes )

Get the size of a Neopixel structure in bytes with *count* Neopixels.

##### `x-out-of-range-neopixel`
( -- )

Out of range Neopixel index exception.

##### `x-out-of-range-color`
( -- )

Out of range color component exception.
