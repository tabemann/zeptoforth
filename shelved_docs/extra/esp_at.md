# ESP-AT and Wio RP2040 ESP-AT Words

zeptoforth has optional support for the SPI ESP-AT protocol for communicating with the ESP8285 WiFi radio on the Wio RP2040 board. This includes supporting outgoing TCP connections and starting a TCP server. It currently does not support UDP, but this may come in the future.

### `esp-at`

The ESP-AT module is defined in `extra/common/esp_at,fs` and defines the ESP-AT API for interfacing with ESP-AT devices such as the ESP8285 WiFi radio on the Wio RP2040 board.

#### Exceptions

##### `x-esp-at-not-owned`
( -- )

ESP-AT device not owned exception

##### `x-esp-at-timeout`
( -- )

ESP-AT timeout exception

##### `x-esp-at-not-ready`
( -- )

ESP-AT device is not ready exception
  
##### `x-esp-at-error`
( -- )

General ESP-AT error exception

##### `x-out-of-range-value`
( -- )

Out of range value exception

#### Connection types

##### `no-connect`

No connection

##### `tcp`

IPv4 TCP connection

##### `tcpv6`

IPv6 TCP connection

##### `udp`

IPv4 UDP non-connection

##### `udpv6`

IPv6 UDP non-connection

##### `ssl`

IPv4 TLS connection

##### `sslv6`

IPv6 TLS connection

#### Connection server/client types

##### `client`

ESP-AT device initiated connection

##### `server`

ESP-AT device received connection

#### Station interface statuses

##### `station-not-inited`

WiFi station is not initialized

##### `station-not-connected`

WiFi station is not connected

##### `station-connected`

WiFi station is connected

##### `station-active`

WiFi station is active

##### `station-disconnected`

WiFi station is disconnected

##### `station-attempted-wifi`

WiFi station attempted connection unsuccessfully

#### Domain name resolution preferences

##### `prefer-resolve-ipv4`

Prefer IPv4 addresses when resolving hostname addresses

##### `resolve-ipv4-only`

Resolve IPv4 addresses only

##### `resolve-ipv6-only`

Resolve IPv6 addresses only

#### WiFi modes

##### `null-mode`

No WiFi mode

##### `station-mode`

WiFi station mode

##### `softap-mode`

WiFi softAP mode

##### `softap-station-mode`

WiFi softAP/station mode

#### Auto-connect settings

##### `not-auto-connect`

Do not auto-connect

##### `auto-connect`

Auto-connect

#### Sleep modes

##### `disable-sleep-mode`

Disable sleep mode

##### `modem-sleep-dtim-mode`

Modem sleep DTIM mode

##### `light-sleep-mode`

Light sleep mode

##### `modem-sleep-listen-interval-mode`

Modem interval listen sleep mode

#### Close-all setting

##### `not-close-all`

Do not close existing connections on closing a server

##### `close-all`

Close existing connections on closing a server

#### `<esp-at>`

The `<esp-at>` class implements the core of the zeptoforth ESP-AT API.

It has the following constructor:

##### `new`
( interface self -- )

The `<esp-at>` constructor takes an instance of `<wio-esp-at-spi>` when it is initialized.

The `<esp-at>` class has the following methods:

##### `claim-esp-at`
( self -- )

Claim the ESP-AT device. All other attempts to claim the ESP-AT device will block until the ESP-AT device is released.

##### `release-esp-at`
( self -- )

Release the ESP-AT device. The next task blocked on the ESP-AT device, if there is any, will then claim it.

##### `with-esp-at`
( xt self -- ) ( xt: device -- )

Execute code with an ESP-AT device, claiming it beforehand and releasing it afterwards. The ESP-AT device will also be released if an exception occurs, which will be re-raised afterwards.

##### `with-esp-at-timeout`
( xt timeout self -- ) ( xt: -- )

Execute code with a set timeout. The timeout will be restored afterwards, including if an exception ocurs, which will be re-raised?

##### `catch-with-esp-at-timeout`
( xt timeout self -- timed-out? ) ( xt: -- )

Execute code with a set timeout and return whether that timeout has been reached. The timeout will be restored afterwards, including if an exception ocurs, which will be re-raised?

##### `esp-at-recv-xt!`
( xt self -- ) ( xt: c-addr bytes mux -- )

Set ESP-AT frame receive callback.

*mux* is the ID from 0 to 4 of the connection in question, or -1 if the ESP-AT device is in single connection mode.

##### `esp-at-timeout!`
( timeout self -- )

Set the ESP-AT device timeout in ticks.

##### `esp-at-timeout@`
( self -- timeout )

Get the ESP-AT device timeout in ticks.

##### `esp-at-delay!`
( delay self -- )

Set the ESP-AT delay in microseconds.

##### `esp-at-delay@`
( self -- delay )

Get the ESP-AT delay in microseconds.

##### `esp-at-long-delay!`
( delay self -- )

Set the ESP-AT long delay in microseconds.

##### `esp-at-long-delay@`
( self -- delay )

Get the ESP-AT long delay in microseconds.

##### `esp-at-multi!`
( multi? self -- )

Enable/disable multiple connections.

##### `esp-at-multi@`
( self -- multi? )

Get multiple connections enabled/disabled.

##### `esp-at-ipv6!`
( ipv6? self -- )

Enable/disable IPv6 mode.

##### `esp-at-ipv6@`
( self -- ipv6? )

Get IPv6 mode enabled/disabled.

##### `esp-at-sleep!`
( sleep-mode self -- )

Set the sleep mode.

##### `esp-at-sleep@`
( self -- sleep-mode )

Get the sleep mode.

##### `esp-at-status@`
( status self -- )

Get the connection state.

*status* is an instance of the `<esp-at-status>` class that will be populated by this method.

##### `esp-at-wifi-power!`
( power self -- )

Set the WiFi power.

##### `esp-at-wifi-power@`
( self -- power )

Get the WiFi power.
    
##### `esp-at-wifi-mode!`
( auto-connect-mode wifi-mode self -- )

Set the WiFi mode.

##### `esp-at-ap-ipv4-addr@`
( self -- c-addr bytes found? )

Get the local softAP IPv4 address.

##### `esp-at-ap-ipv6-ll-addr@`
( self -- c-addr bytes found? )

Get the local softAP link-local IPv6 address.

##### `esp-at-ap-ipv6-gl-addr@`
( self -- c-addr bytes found? )

Get the local softAP global IPv6 address.

##### `esp-at-ap-mac-addr@`
( self -- c-addr bytes found? )

Get the local softAP MAC address.

##### `esp-at-station-ipv4-addr@`
( self -- c-addr bytes found? )

Get the local station IPv4 address.

##### `esp-at-station-ipv6-ll-addr@`
( self -- c-addr bytes found? )

Get the local station link-local IPv6 address.

##### `esp-at-station-ipv6-gl-addr@`
( self -- c-addr bytes found? )

Get the local station global IPv6 address.

##### `esp-at-station-mac-addr@`
( self -- c-addr bytes found? )

Get the local station MAC address.

##### `test-esp-at`
( self -- )

Test an ESP-AT device. If the ESP-AT device does not respond `x-esp-at-not-ready` will be raised.

##### `esp-at-echo!`
( echo? self -- )

Set ESP-AT command echoing. This is relevant when logging to the serial console is enabled.

##### `init-esp-at`
( mode self -- )

Initialize an ESP-AT device. This must be executed before using an ESP-AT device. This also disconnects any prior connections, puts the ESP-AT device in single connection mode, and sets the WiFi mode to *mode*.
    
##### `reset-esp-at`
( self -- )

Reset an ESP-AT device.
    
##### `poll-esp-at`
( self -- )

Poll an ESP-AT device for received data. Any received data is passed to the xt that was specified with `esp-at-recv-xt!`.
   
##### `connect-esp-at-wifi`
( D: password D: ssid self -- )

Connect to a WiFi AP.

##### `disconnect-esp-at-wifi`
( self -- )

Disconnect from a WiFi AP.

##### `resolve-esp-at-ip`
( prefer D: domain -- c-addr bytes found )

Resolve a domain name.
    
##### `start-esp-at-single`
( keep-alive type remote-port D: remote-host self -- )

Start a single ESP-AT TCP connection.

*remote-host* may be an IPv4 or IPv6 address or a hostname.

*keep-alive* is 0 for no TCP keep alive functionality, 1-7200 for detection interval in seconds.

##### `start-esp-at-multi`
( keep-alive type remote-port D: remote-host mux self -- )

Start a multiple ESP-AT TCP connection.

*mux* is an ID from 0 to 4 to assign to the TCP connection.

*remote-host* may be an IPv4 or IPv6 address or a hostname.

*keep-alive* is 0 for no TCP keep alive functionality, 1-7200 for detection interval in seconds.

##### `start-esp-at-server`
( port self -- )

Start a TCP server.

Note that prior to executing this multiple connection mode must be enabled with `esp-at-multi!`.

##### `delete-esp-at-server`
( close-all self -- )

Delete a TCP server.

##### `single>esp-at`
( c-addr bytes self -- )

Send data for a single TCP connection to an ESP-AT device.

##### `multi>esp-at`
( c-addr bytes mux self -- )

*mux* is the ID from 0 to 4 of the TCP connection.

Send data for a multiple TCP connection to an ESP-AT device.
    
##### `close-esp-at-single`
( self -- )

Close a single TCP connection on an ESP-AT device. Note that with some ESP8285 firmware this may cause a crash.

##### `close-esp-at-multi`
( mux self -- )

Close a multiple TCP connection on an ESP-AT device. Note that with some ESP8285 firmware this may cause a crash.

*mux* is the ID from 0 to 4 of the TCP connection.

#### `<esp-at-status>`

The `<esp-at-status>` class encapsulates the ESP-AT device status as reported by `esp-at-status@`.

##### `esp-at-status-station!`
( station self -- )

Set the station status.

##### `esp-at-status-station@`
( self -- station )

Get the station status.
    
##### `esp-at-status-count!`
( count self -- )

Set the entry count.

##### `esp-at-status-count@`
( self -- count )

Get the entry count.

##### `esp-at-status-mux!`
( mux index self -- )

Set the entry link ID.

##### `esp-at-status-mux@`
( index self -- mux )

Get the entry link ID.

##### `esp-at-status-type!`
( type index self -- )

Set the entry link type.

##### `esp-at-status-type@`
( index self -- type )

Get the entry link type.

##### `esp-at-status-tetype!`
( tetype index self -- )

Set the entry link server/client type.

##### `esp-at-status-tetype@`
( index self -- tetype )

Get the entry link server/client type.

##### `esp-at-status-remote-ip!`
( ip-addr ip-len index self -- )

Set the entry link remote IP.

##### `esp-at-status-remote-ip@`
( index self -- ip-addr ip-len )

Get the entry link remove IP.

##### `esp-at-status-remote-port!`
( port index self -- )

Set the entry link remote port.

##### `esp-at-status-remote-port@`
( index self -- port )

Get the entry link remote port.

##### `esp-at-status-local-port!`
( port index self -- )

Set the entry link local port.

##### `esp-at-status-local-port@`
( index self -- port )

Get the entry link local port.

##### `esp-at-status.`
( self -- )

Print link status.

### `wio-esp-at`

The `wio-esp-at` module defined in `extra/rp2040/wio_esp_at.fs` defines the hardware configuration necessary to interface with the ESP8285 WiFi radio on the Wio RP2040 board.

#### `<wio-esp-at-spi>`

The `<wio-esp-at-spi>` class provides an interface to the ESP8285 on the Wio RP2040 over SPI peripheral 1. It exposes the following API:

##### `esp-at-log!`
( log? self -- )

This sets whether logging is enabled for communications over SPI. Note that logging always goes to the serial console, even if the console has been redirected.

##### `esp-at-log?`
( self -- log? )

This gets whether logging is enabled for communications over SPI.
