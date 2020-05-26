# SysTick Guide

There is not much to SysTick, so this will be short. In zeptoforth SysTicks are at 10ths of milliseconds intervals, so:

##### `systick-counter`
( -- u )

returns the current SysTick count in 10ths of milliseconds. This value is an unsigned 32-bit value, and will wrap around.

To wait a given number of milliseconds (not 10ths of milliseconds), one executes:

##### `ms`
( u -- )

where *u* is the number of milliseconds to wait. Note that this word is replaced with an outwardly equivalent word by `sys/common/forth/task.fs`; unlike that word this word does not take advantage of the timing capabilities provided by `sys/common/forth/task.fs`, even though it does call `PAUSE`.

To disable SysTicks, execute:

##### `disable-systick`
( -- )

To enable SysTicks again, execute:

##### `enable-systick`
( -- )

SysTicks are enabled by default once `sys/common/forth/systick.fs` has been loaded and the MCU has been rebooted.
