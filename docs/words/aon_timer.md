# Always-On Timer words

The Always-On Timer is a timer on the RP2350 which can count time in milliseconds as long as a small amount of power is applied to the MCU. It can be driven by the XOSC, the LPOSC, or an external clock source tied to a GPIO at 1 kHz. It can also be synchronized by an external clock source tied to a GPIO at 1 Hz. By convention in zeptoforth it counts the number of milliseconds since 1 Jan 1970 00:00:00.000.

The Always-On Timer has the ability to trigger an alarm, which can wake up the RP2350 when it is in a low-power state.

Note that on power-up the Always-On Timer is initialized to enabled using the XOSC as a time source with a time of 0, i.e. 1 Jan 1970 00:00:00.000.

### `aon-timer`

The `aon-timer` module contains the following words:

##### `enable-timer`
( -- )

Enable the Always-On Timer.

##### `disable-timer`
( -- )

Disable the Always-On Timer.

##### `timer-enabled?`
( -- enabled? )

Get whether the Always-On Timer is enabled. On bootup this will return `true`.

##### `time!`
( D: time -- )

Set the Always-On Timer time to a double-cell value in milliseconds, typically from 1 Jan 1970 00:00:00.000.

##### `time@`
( -- D: time )

Get the time in milliseconds of the Always-On Timer as a double-cell value, typically from 1 Jan 1970 00:00:00.000.

##### `clear-time`
( -- )

Clear the Always-On Timer.

##### `enable-alarm`
( -- )

Enable the Always-On Timer alarm.

##### `disable-alarm`
( -- )

Disable the Always-On Timer alarm.

##### `alarm-enabled?`
( -- )

Get whether the Always-On Timer alarm is enabled. On power-up this is `false`.

##### `alarm!`
( D: time -- )

Set the Always-On Timer alarm time to a double-cell value in milliseconds, typically from 1 Jan 1970 00:00:00.000.

##### `alarm@`
( -- D: time )

Get the alarm time in milliseconds of the Always-On Timer as a double-cell value, typically from 1 Jan 1970 00:00:00.000.

##### `enable-alarm-wake-up`
( -- )

Enable the Always-On Timer alarm wake-up from low power mode.

##### `disable-alarm-wake-up`
( -- )

Disable the Always-On Timer alarm wake-up from low power mode.

##### `alarm-wake-up-enabled?`
( -- )

Get whether the Always-On Timer alarm wake-up from low power mode is enabled. On power-up this is `false`.

##### `lposc-timer`
( -- )

Switch to LPOSC as the source of the 1 kHz timer tick.

##### `xosc-timer`
( -- )

Switch to XOSC as the source of the 1 kHz timer tick.

##### `gpio-1khz-timer`
( -- )

Switch to a GPIO as the source of the 1 kHz timer tick.

##### `sync-gpio-1hz-timer`
( -- )

Synchronize with a GPIO as the source of the second counter.

##### `unsync-gpio-1hz-timer`
( -- )

Unsynchronize with a GPIO as the source of the second counter.
  
##### `lposc-timer?`
( -- flag )

Get whether the timer is running from the LPOSC.

##### `xosc-timer?`
( -- flag )

Get whether the timer is running from the XOSC.

##### `gpio-1khz-timer?`
( -- flag )

Get whether the timer is running from a 1 kHz GPIO source.

##### `sync-gpio-1hz-timer?`
( -- flag )

Get whether the timer is synchronized with a GPIO as the source of the second counter.

##### `source-sel!`
( gpio -- )

Set a GPIO as a source selection.

##### `source-sel-drive-lpck!`
( gpio -- )

Set a GPIO as a source selection to drive the 32 kHz low power clock.

##### `source-sel@`
( -- gpio )

Get the source selection GPIO.

##### `drive-lpck?`
( -- flag )

Get whether a GPIO is driving the low power clock.

##### `x-invalid-source-sel-gpio`
( -- )

GPIO is not a valid GPIO for source selection exception