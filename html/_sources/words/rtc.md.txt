# Realtime Clock (RTC) Words on the Raspberry Pi Pico

zeptoforth has support for reading and setting the realtime clock (RTC) on the Raspberry Pi Pico, both for keeping track of the date and time and for triggering an alarm at a given combination of second, minute, hour, day of the week, day, month, and/or year values.

### `rtc`

The following words are in the `rtc` module:

##### `date-time-size`
( -- bytes )

This returns the size of a date/time structure in bytes.

##### `date-time-year`
( date-time -- addr )

This returns the address of a cell in a date/time containing a year, from 0 to 4095, or when not setting a year or not matching against a yaer, -1

##### `date-time-month`
( date-time -- c-addr )

This returns the address of a byte in a date/time containing a month, from 1 to 12, or when not setting a month or not matching against a month, 255

##### `date-time-day`
( date-time -- c-addr )

This returns the address of a byte in a date/time containing a day, from 1 to any of 28, 29, 30, or 31 depending on the month and year, or when not setting a day or not matching against a day, 255

##### `date-time-dotw`
( date-time -- c-addr )

This returns the address of a byte in a date/time containing a day of the week, from 0 representing Sunday to 6 representing Saturday, or when not setting a day of the week or not matching against a day of the week, 255.

##### `date-time-hour`
( date-time -- c-addr )

This returns the address of a byte in a date/time containing an hour, from 0 to 23, or when not setting an hour or not matching against an hour, 255.

##### `date-time-minute`
( date-time -- c-addr )

This returns the address of a byte in a date/time containing a minute, from 0 to 59, or when not setting a minute or not matching against a minute, 255.

##### `date-time-second`
( date-time -- c-addr )

This returns the address of a byte in a date/time containing a second, from 0 to 59, or when not setting a second or not matching against a second, 255.

##### `enable-rtc`
( -- )

Enable the realtime clock. (Note that it is enabled by default.)

##### `disable-rtc`
( -- )

Disable the realtime clock.

##### `date-time@`
( date-time -- )

Get the current date/time.

##### `date-time!`
( date-time -- )

Set the current date/time.

##### `set-rtc-alarm`
( date-time xt -- )

Set *xt* to execute as an RTC alarm interrupt handler at *date-time*, ignoring a year field of -1 and any of the other fields of 255. Note that when *xt* is executed it must reset the RTC alarm to a new date/time or clear the RTC alarm or else it will be triggered repeatedly.

##### `clear-rtc-alarm`
( -- )

Clear the RTC alarm.

##### `dotw-name`
( dotw -- c-addr u )

Get the name of a day of the week.

##### `month-name`
( month -- c-addr u )

Get the name of a month.

##### `max-date-time-format-size`
( -- bytes )

Get the maximum size in bytes of a date formated with `format-date-time`.

##### `format-date-time`
( c-addr date-time -- c-addr u )

Format *date-time* as a string starting at *c-addr*, and return the resulting string as the pair *c-addr* *u*.

##### `date-time.`
( date-time -- )

Print a date/time as formatted with `format-date-time`.
