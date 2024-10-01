# Realtime Clock (RTC) Words

zeptoforth has support for reading and setting the realtime clock (RTC) on the RP2040 (e.g. the Raspberry Pi Pico), both for keeping track of the date and time and for triggering an alarm. It also provides a layer for interfacing with the Always-On TImer on the RP2350 (e.g. the Raspberry Pi Pico 2) for converting its time (in milliseconds since Thursday, 1 January 1970 00:00:00) to and from human-friendly dates/times.

Note that the RP2040's RTC's value is not preserved across resets, even if power is applied, so each time it is reset the RTC's value must be provided a value before it can be useful. It is initialized on bootup to Thursday, 1 January 1970 00:00:00, and is enabled on boot.

The RP2350's Always-On Timer's time value is preserved across resets provided power is applied. The time when power is first applied is Thursday, 1 January 1970 00:00:00.

The alarm functionality on the RP2040 allows triggering an alarm interrupt at a given combination of second, minute, hour, day of the week, day, month, and/or year values. Note that the alarm interrupt must either reset the alarm date/time to a new one or clear the alarm or otherwise it will be repeatedly triggered, which in most use cases is undesirable.

There is no alarm functionality on the RP2350 in the `rtc` module, even though an alarm exists in the `aon-timer` module.

Date time values are validated when provided, except that year values of -1 and values for other fields of $FF or, for the millisecond, $FFFF, are ignored during validation, as they are explicit non-values used for setting a subset of RTC fields or matching against a subset of RTC fields.

Millisecond values are only actively used on the RP2350 and are ignored on the RP2040 and other platforms except when converting date/times and comparing date/times. The RP2350 Always-On Timer has millisecond resolution while the RP2040 RTC only has second resolution exposed to the user.

Also provided is code for formatting date/times as strings and printing them to the console, for convenience's sake.

Note that a subset of these words are provided for platforms other than the RP2040 and RP2350, specifically all of these words aside from `enable-rtc`, `disable-rtc`, `date-time!`, `simple-date-time!`, `set-rtc-alarm`, and `clear-rtc-alarm`. Also, `enable-rtc`, `disable-rtc`, `set-rtc-alarm`, and `clear-rtc-alarm` are not available on the RP2350.

### `rtc`

The following words are in the `rtc` module:

##### `date-time-size`
( -- bytes )

This returns the size of a date/time structure in bytes.

##### `date-time-year`
( date-time -- addr )

This returns the address of a cell in a date/time containing a year, from 0 to 4095, or when not setting a year or not matching against a year, -1

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

##### `date-time-msec`
( date-time -- h-addr )

This returns the address of a halfword in a date/time containing a millisecond, from 0 to 999, or when not setting a millisecond or not matching against a millisecond, 65535.

##### `enable-rtc`
( -- )

Enable the realtime clock. (Note that it is enabled by default.) (This is `rp2040` only.)

##### `disable-rtc`
( -- )

Disable the realtime clock. (This is `rp2040` only.)

##### `date-time-equal?`
( date-time0 date-time1 -- equal? )

Check whether two date-times are equal (ignoring the day of the week).

##### `date-time@`
( date-time -- )

Get the current date/time. (This is `rp2040` and `rp2350` only.)

##### `date-time!`
( date-time -- )

Set the current date/time. (This is `rp2040` and `rp2350` only.)

##### `simple-date-time!`
( year month day hour minute second -- )

A convenience word to the current date/time at once, automatically calculating the day of the week. The arguments have the same meanings, respectively, as the `date-time-year`, `date-time-month`, `date-time-day`, `date-time-hour`, `date-time-minute`, and `date-time-second` of a date/time structure except that for all of these, not just the *year*, may have values of -1 to indicate unchanged values. Also, note that if *year* is -1, *month* is -1 or 255, or *day* is -1 or 255, the day of the week is not changed. Note that milliseconds are set to 0. (This is `rp2040` and `rp2350` only.)

##### `set-rtc-alarm`
( date-time xt -- )

Set *xt* to execute as an RTC alarm interrupt handler at *date-time*, ignoring a year field of -1 and any of the other fields of 255. Note that when *xt* is executed it must reset the RTC alarm to a new date/time or clear the RTC alarm or else it will be triggered repeatedly. (This is `rp2040` only.)

##### `clear-rtc-alarm`
( -- )

Clear the RTC alarm. (This is `rp2040` only.)

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

##### `get-dotw`
( date-time -- dotw )

Calculate the day of the week for a date/time, ignoring its day of the week field; 0 is Sunday and 6 is Saturday. Note that if the `date-time-year` field of *date-time* is -1, the `date-time-month` field of *date-time* is 255, or the `date-time-day` field of *date-time* is 255, 255 will be returned for the day of the week.

##### `update-dotw`
( date-time )

Update the day of the week for a date/time, ignoring its preexisting day of the week field. Note that if the `date-time-year` field of *date-time* is -1, the `date-time-month` field of *date-time* is 255, or the `date-time-day` field of *date-time* is 255, 255 will be set for the day of the week.

##### `convert-secs-since-1970`
( seconds date-time -- )

Set a date/time with seconds from 1970-01-01 00:00:00. Note that milliseconds are set to 0.

##### `convert-msecs-since-1970`
( D: milliseconds date-time -- )

Set a date/time with milliseconds from 1970-01-01 00:00:00.

##### `convert-to-msecs-since-1970`
( date-time -- D: milliseconds )

Convert a date/time to milliseconds from 1970-01-01 00:00:00.
