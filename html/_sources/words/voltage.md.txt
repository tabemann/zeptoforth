# Voltage words

There is support on the RP2040 and RP2350 for controlling the output voltage of the voltage regulator. The range of voltages normally supported is 850 mV to 1300 mV on the RP2040 and 550 mV to 1300 mV on the RP2350, but there is also an option to set an 'unlimited' voltage up to 3300 mV on the RP2350 at the risk of bricking your RP2350.

Warning: setting a voltage above 1300 mV on your RP2350 may brick it!

### `voltage`

The `voltage` module contains the following words:

##### `x-unsupported-voltage`
( -- )

This exception is raised if one attempts to set an out-of-range voltage. Note that attempting to set an in-range-voltage that is not one of the specific voltages supported will result in it getting rounded down a supported voltage.

##### `voltage@`
( -- mvolts )

Get the current output voltage of the voltage regulator in millivolts.

##### `set-voltage`
( mvolts -- )

Set the current output voltage of the voltage regulator in millivolts, from 850 mV to 1300 mV on the RP2040 and 550 mV to 1300 mV on the RP2350.

Attempting to set a voltage outside of one of these ranges will result in `x-unsupported-voltage` being raised. Attempting to set a voltage that is in one of these ranges which is not a specifically supported increment will result in it getting rounded down.

##### `set-voltage-unlimited`
( mvolts -- )

Set the current output voltage of the voltage regulator in millivolts, from 850 mV to 1300 mV on the RP2040 and 550 mV to 3300 mV on the RP2350. Note that on the RP2040 this is merely an alias for `set-voltage` and has no differences from it, existing only for the sake of compatibility with the RP2350 codebase.

Attempting to set a voltage outside of one of these ranges will result in `x-unsupported-voltage` being raised. Attempting to set a voltage that is in one of these ranges which is not a specifically supported increment will result in it getting rounded down.

Warning: you may brick your RP2350 by using this word if you set a voltage above 1300 mV, so use it with caution!
