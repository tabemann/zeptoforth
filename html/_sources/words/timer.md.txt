# Hardware Timers

zeptoforth on the RP2040 (i.e. the Raspberry Pi Pico) has support for hardware timers and delays at up to a microsecond precision. Note that in practice microsecond precision is not necessarily possible, due to interrupts being disabled, interrupt priorities, busy-waits having a limited time resolution, and the chance of busy-waits being preempted if interrupts are not disabled during them.

The 64-bit microsecond timer can represent a sufficiently large range of values, i.e. it can represent thousands of years of time, such that it can be treated as effectively monotonic. Note that the user can change the value of the microsecond timer or pause it, but this is not recommended in most use cases.

The hardware timer on the RP2040 has one shared 64-bit microsecond counter and four "alarms" which are triggered by a set value to be compared with the lower 32 bits of the microsecond counter. Alarm handlers execute at interrupt time, so they must be interrupt-safe. Alarms must be enabled for each time they are used and are disabled when they are triggered. However, the interrupt state must be disabled before leaving the alarm interrupt handler or else they will be triggered repeatedly in an infinite loop. Also note that alarm handlers always execute on core 0 even when core 1 has been booted.

There are also busy-loop microsecond delays which wait until a given 64-bit microsecond time. Because the 64-bit microsecond counter can be reasonably treated as not wrapping, only a fixed 64-bit microsecond time is provided. On top of this, busy-loop microsecond delays which wait a given 64-bit microsecond interval are also available. This may be easier to use, but when waiting at repeated intervals maintaining a separate counter, incrementing it for each interval, and waiting until that time is likely to provide better behavior than waiting for intervals relative to the start of the wait.

### `timer`

The following words are in the `timer` module on the `rp2040` platform:

##### `us-counter-lsb`
( -- us )

Get the lower 32 bits in the current time in microseconds.

##### `us-counter`
( -- us-d )

Get the current time in microseconds as a 64-bit value.

##### `us-counter!`
( us-d -- )

Set the microsecond counter to a 64-bit value.

##### `pause-us`
( -- )

Pause the microsecond counter.

##### `unpause-us`
( -- )

Unpause the microsecond counter.

##### `delay-until-us`
( us-d -- )

Delay until at least a 64-bit microsecond value, even though it may take longer considering the practical delay resolution, interrupts, and whether the microsecond counter is set or paused.

##### `delay-us`
( us-d -- )

Delay a given number of microseconds, even though it may take longer considering the practical delay resolution, interrupts, and whether the microsecond counter is set or paused.

##### `set-alarm`
( us xt alarm -- )

Set an alarm at a time. *alarm* is an alarm index from 0 to 3. *xt* is the execution token for the alarm handler. *us* is the lower 32 bits of the microsecond counter to trigger the alarm at.

##### `clear-alarm`
( alarm -- )

Clear an alarm. *alarm* is an alarm index from 0 to 3.

##### `clear-alarm-int`
( alarm -- )

Clear an alarm interrupt. *alarm* is an alarm index from 0 to 3. This must be called with the approriate alarm index inside an alarm handler for a given alarm or else the alarm handler will be called in an infinite loop.

##### `x-out-of-range-alarm`

Out of range alarm index exception.
