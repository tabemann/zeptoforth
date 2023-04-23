# Software Alarm Words

Software alarms are time-based alarms that can be registered with an alarm task, which may be either automatically or explicitly created. Alarm tasks are tasks for which a list of alarms are associated which execute said alarms when the alarms' times expire. At bootup no alarm tasks are created, but if a default alarm task has not been created at the time that an alarm is first set using a default alarm task, a default alarm task is automatically created with a dictionary size of 320 bytes, a data stack size of 128 bytes, a return stack size of 512 bytes, and a core (relevant to operation on the RP2040) of 0. If a different default alarm task configuration is desired, `init-default-alarm-task` is to be used.

Alarms are created with a set delay after a start time, which can be set for convenience to the current time, after which they are triggered, calling their specified execution token, even if the trigger time was in the past when they were created. All times are measured in ticks, which are normally 100 microsecond intervals, with the current time corresponding to `systick::systick-counter`. Alarms are one-shot - they must always be manually set again after they are triggered. Alarms also have a set priority; an alarm task takes on the maximum of all the priorities of the currently active alarms associated with it. Alarms have an associated data value, to eliminate the memory cost and complexity of creating a closure for them in many cases. Their execution token has the signature ( data alarm -- ) for convenience for accessing their associated data and resetting themselves, for repeated alarms.

Alarms may be dynamically set and unset; note, however, that it is not safe to set an alarm that has already been set and which has not been triggered yet, as setting an alarm assumes that it is uninitialized and initializes it in the process. It is safe to set an alarm which has either been triggered or manually unset again. It is also safe to unset an alarm which has previously been set which has already been triggered or has previously been unset.

### `alarm`

The `alarm` module contains the following words:

##### `alarm-size`
( -- bytes )

This returns the size of an alarm structure in bytes.

##### `alarm-task-size`
( -- bytes )

This returns the size of an alarm task structure in bytes, not including the size of the associated task which is created when the alarm task structure is initialize.

##### `init-alarm-task`
( dict-size stack-size rstack-size core alarm-task -- )

Initialize an alarm task *alarm-task*, of size `alarm-task-size`, with a dictionary size of *dict-size* bytes, a data stack size of *stack-size* bytes, a return stack size of *rstack-size*, a core of *core* (on non-RP2040 platforms this must be 0). This will create a task for the alarm task with the name `alarm`.

##### `init-default-alarm-task`
( dict-size stack-size rstack-size core -- )

Initialize a default alarm task, with a dictionary size of *dict-size* bytes, a data stack size of *stack-size* bytes, a return stack size of *rstack-size*, and a core of *core* (on non-RP2040 platforms this must be 0). If a default alarm task has already been initialized, whether explicitly through this word or implicitly through calling `default-alarm-task@`, `set-alarm-default`, or `set-alarm-delay-default`, `x-default-alarm-task-already-inited` will be raised.

##### `default-alarm-task@`
( -- default-alarm-task )

Get the default alarm task, initializing it (as mentioned above) if it has not already been initialized..

##### `set-alarm`
( ticks-delay ticks-start priority data xt alarm alarm-task -- )

Set (and initialize) an alarm *alarm* for alarm-task *alarm-task* to execute *xt* with data *data* with priority *priority* at *ticks-delay* (or greater) ticks after *ticks-start*. It is assumed that *alarm* is not currently set, where if it is undefined results will occur.

##### `set-alarm-default`
( ticks-delay ticks-start priority data xt alarm -- )

Set (and initialize) an alarm *alarm* for the default alarm task, which will be initialized as mentioned above if not already initialized, to execute *xt* with data *data* with priority *priority* at *ticks-delay* (or greater) ticks after *ticks-start*. It is assumed that *alarm* is not currently set, where if it is undefined results will occur.

##### `set-alarm-delay`
( ticks-delay priority data xt alarm alarm-task -- )

Set (and initialize) an alarm *alarm* for alarm-task *alarm-task* to execute *xt* with data *data* with priority *priority* at *ticks-delay* (or greater) ticks after the current value of `systick::systick-counter`. It is assumed that *alarm* is not currently set, where if it is undefined results will occur.

##### `set-alarm-delay-default`
( ticks-delay priority data xt alarm -- )

Set (and initialize) an alarm *alarm* for the default alarm task, which will be initialized as mentioned above if not already initialized, to execute *xt* with data *data* with priority *priority* at *ticks-delay* (or greater) ticks after the current value of `systick::systick-counter`. It is assumed that *alarm* is not currently set, where if it is undefined results will occur.

##### `unset-alarm`
( alarm -- )

Unset an alarm that has been previously set (and thus initialized). This is safe even if said alarm has previously been triggered or otherwise unset.

##### `x-default-alarm-task-already-inited`
( -- )

Default alarm task already initialized exception.
