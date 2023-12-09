# Simple Network Time Protocol Client

There is a SNTP (Simple Network Time Protocol) client implemented using zeptoIP. As an SNTP client it uses only one NTP server, as opposed to NTP clients which use multiple SNTP servers. It is not automatically included with the code loaded with `extra/rp2040/pico_w_net_all.fs` and it is up to the user whether to compile it to flash or to RAM.

Note that it gives time as a 32.32 unsigned fixed-point number of seconds from 1 Jan 1900 00:00:00 GMT. Consequently, if one tries to treat this number as a conventional S31.32 signed fixed-point number, it will (i.e. as of writing) to be treated as negative until the current NTP era ends.

Kiss-of-death (including reduce-rate) packets are supported. Also, the address resolved with DNS is regularly re-resolved so as to properly use DNS server pools where multiple DNS server as behind a single DNS name.

### `ntp`

The `ntp` module contains the following word:

##### `ntp-port`
( -- port )

This returns the default NTP port, i.e 123.

The `ntp` module contains the following class:

#### `<ntp>`

The `<ntp>` class inherits from the `<endpoint-handler>` class defined in the `endpoint-process` module. It has the following constructor:

##### `new`
( ip-interface sntp-client -- )

This constructor takes an instance of `<interface>` *ip-interface* and the `<ntp>` instance being constructed. *ip-interface* will be used by the NTP client for looking up the NTP server via DNS and communicating with it.

The `<ntp>` class has the following methods:

##### `init-ntp`
( dns-name dns-name-len port sntp-client -- )

This method starts the SNTP client with *dns-name* and *dns-name-len* being the hostname used to lookup the NTP server (note that using NTP servers by fixed IPv4 addresses is _not_ supported) using the port *port* (typically `ntp-port`, i.e. 123).

##### `current-time@`
( sntp-client -- D: time )

Get the current time as a 32.32 unsigned fixed-point number of seconds from 1 Jan 1900 00:00:00 GMT. Note that if `time-set?` does not return `true` this value will be invalid (and will be fixed to `0.`).

##### `time-set?`
( sntp-client -- time-set? )

Get whether a time has been established for the SNTP client. This will initially return `false` and will only return `true` after a (relatively short) delay provided the NTP server can be looked up and responds to SNTP requests.
