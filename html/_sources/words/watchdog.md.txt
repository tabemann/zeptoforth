# Watchdog

The RP2040 includes watchdog functionality that, when enabled, triggers a reboot after a set delay during which the watchdog timer is not updated. It also has the capability to force a reset. By default the watchdog timer is disabled, the watchog delay is 1 second, and the watchdog (once enabled) is updated automatically each time the multitasker runs outside of a critical section. This behavior chan be changed in that the watchdog delay can be set to any time between 0 seconds (i.e. trigger immediately, and report a 'force' reboot reason rather than a 'timer' reason) and 16.777 seconds, and the multitasker can be set to not automatically update the watchdog timer (and this require manual updating by the user).

### `watchdog`

##### `x-out-of-range-watchdog-delay`
( -- )

Out of range watchdog delay exception.

##### `update-watchdog`
( -- )

Update the watchdog timer with the currently set watchdog delay.

##### `force-watchdog-reboot`
( -- )

Force an immediate watchdog reboot even if the watchdog timer is not enabled.

##### `watchdog-delay-us!`
( us -- )

Set the watchdog delay to a specified positve number of microseconds, up to a delay of 8388607 microseconds. If an out of range delay is provided `x-out-of-range-watchdog-delay` will be raised. Note that if a value of 0 is provided and the watchdog is currently enabled the watchdog will trigger immediately and a 'force' reboot reason will be reported; this also will happen if the watchdog is disabled, and then the watchdog is enabled after this is called. Also note that if the watchdog is enabled and a value other than 0 is given the watchog timer will be updated with that value.

##### `enable-watchdog`
( -- )

Enable the watchdog timer. If the watchdog delay is currently set to 0 the watchdog will trigger immediately and a 'force' reboot reason will be reported.

##### `disable-watchdog`
( -- )

Disable the watchdog timer.

##### `reboot-reason-watchdog-force?`
( -- force? )

Get whether the most recent reboot was due to a watchdog force.

##### `reboot-reason-watchdog-timer?`
( -- timer? )

Get whether the most reason reboot was due to a watchdog timeout.

##### `enable-multitasker-update-watchdog`
( -- )

Enable the multitasker automatically updating the watchdog timer each time it runs outside of a critical section.

##### `disable-multitasker-update-watchdog`
( -- )

Disable the multitasker automatically updating the watchdog timer each time it runs outside of a critical section.

##### `enable-fault-watchdog-reboot`
( -- )

Enable triggering a watchdog force reboot on a hardware fault.

##### `disable-fault-watchdog-reboot`
( -- )

Disable triggering a watchdog force reboot on a hardware fault.
