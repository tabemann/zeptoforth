# zeptoIP Words

zeptoIP is an Internet Protocol stack for zeptoforth. Currently it supports the Raspberry Pi Pico W and Raspberry Pi Pico 2 W's CYW43439 WiFi chip. By design it can be extended to any WiFi or Ethernet interface which exposes receiving and sending Ethernet frames. It expressly does not support WiFi interfaces such as the ESP8285 on the Wio RP2040 which operate by using their own protocols such as the "AT" protocol. Note that currently zeptoIP only supports IPv4.

zeptoIP is centered around one or more _interfaces_, instances of `net::<interface>`each of which has its own IP address, netmask, gateway IP address, and DNS server IP address. With interfaces sources of incoming (and in the case of TCP data, outgoing) data, known as _endpoints_, instances of `net::<endpoint>` may be created, endpoints with ready data or state changes can be fetched, UDP packets may be sent, IP addresses of hostnames can be resolved via DNS, and IP addresses may be acquired via DHCP.

Endpoints encapsulate connection state in the case of TCP, and received packet state in the case of UDP. Reading received data from an endpoint need not require copying the data; the received data associated with an endpoint is valid until the endpoint is marked as _done_, at which the received data is cleared from the endpoint's buffer and the room it took up in the endpoint's buffer is made available for more received data. Note that data continues to be received even between the time that ready endpoint is fetched with `net::get-ready-endpoint` or is waited for with `net::wait-ready-endpoint` and it is marked as done with `net::endpoint-done`. Also note that with UDP endpoints the ready data at any one data comprises only a single received UDP packet's data.

The typical means by which endpoints are serviced is by creating an _endpoint processor_, an instance of `endpoint-process::<endpoint-process>`, and registering an _endpoint handler_, an instance of `endpoint-process::<endpoint-handler>` with it. The endpoint processor involves a task which fetches ready endpoints one at a time from an interface, and then applies it to each register endpoint handler. Once all the endpoint handlers have processed the endpoint, it should be marked as done to make it available for new data or state changes.

A more conventional approach to servicing endpoints, albeit one that requires multiple tasks if multiple endpoints are to be serviced, is through calling `net::wait-ready-endpoint`, which blocks until the endpoint is ready, and then promotes the data its receive buffer.

Sending data is done differently with regard to TCP and UDP. With TCP sending data requires a TCP endpoint, gotten through either receiving an incoming connection on a listening TCP endpoint, started with `net::allocate-tcp-listen-endpoint`, which will result in the endpoint being readied with the appropriate state change(s), or starting a TCP connection with `net::allocate-tcp-connect-ipv4-endpoint`, at which an TCP endpoint will be provided to the user. Once one has done this, all that one must do is call `send-tcp-endpoint`, which will send the provided data unless the connection is closed or reset, where then it will exit prematurely. With UDP sending data simply requires executing `net::send-ipv4-udp-packet`.

Closing an endpoint is accomplished through `net::close-tcp-endpoint`, for TCP endpoints, and `net::close-udp-endpoint`, for UDP endpoints. Closing UDP endpoints is immediate, while closing TCP endpoints involves waiting for the endpoint to be closed after a four-way handshake.

If the user wishes to listen to multiple connections on the same point they must allocate multiple endpoints to listen on; incoming connections will be alloted to each of these endpoints.

Note that there is a fixed number of available endpoints which is set at compile-time, specified by the constant `net-config::max-endpoints`. This is set to four by default.

### `net`

The `net` module contains the following words:

##### `x-oversized-frame`
( -- )

This exception is raised if a frame larger than the MTU would be raised.

##### `x-invalid-dns-name`
( -- )

This exception is raised if an invalid DNS name is provided.

The `net` module contains the following classes:

#### `<interface>`

The `<interface>` class has the following methods:

##### `intf-mac-addr@`
( interface -- D: mac-addr )

Get an interface's MAC address.

##### `send-tcp-endpoint`
( addr bytes endpoint interface -- )

Send *bytes* at *addr* to the peer of *endpoint*. This will return when sending data is complete or if the connection is closed or its connection is not yet established.

##### `evict-dns`
( c-addr bytes interface -- )

Evict a DNS name's cache entry, forcing it to be re-resolved.

##### `get-ready-endpoint`
( interface -- endpoint )

Do a blocking wait to get the next ready endpoint; note that the user may use `task::timeout` to apply a timeout to this.

##### `wait-ready-endpoint`
( endpoint interface -- )

Do a blocking wait until the given endpoint becomes ready; note that the user may use `task::timeout` to apply a timeout to this.

##### `endpoint-done`
( endpoint interface -- )

Retire the current pending data for an endpoint and allow the endpoint to be readied again; if there is data already available, the endpoint will be readied again immediately.

##### `allocate-udp-listen-endpoint`
( port interface -- endpoint success? )

Attempt to allocate a UDP endpoint listening on *port* (which may be any port from `net-consts::MIN_EPHEMERAL_PORT` to `net-consts::MAX_EPHEMERAL_PORT` if `net-consts::EPHEMERAL_PORT` is provided) and return true along with the endpoint, unless no endpoints are free, where then false is returned along with a padding cell.

##### `allocate-tcp-listen-endpoint`
( port interface -- endpoint success? )

Attempt to allocate a TCP endpoint listening on *port* (which may be any port from `net-consts::MIN_EPHEMERAL_PORT` to `net-consts::MAX_EPHEMERAL_PORT` if `net-consts::EPHEMERAL_PORT` is provided) and return true along with the endpoint, unless no endpoints are free, where then false is returned along with a padding cell.

##### `close-udp-endpoint`
( endpoint interface -- )

Close a UDP endpoint. This is immediate, and any queued UDP packets will be lost.

##### `close-tcp-endpoint`
( endpoint interface -- )

Close a TCP endpoint. This waits until the connection is normally closed or the connection is reset.

#### `<endpoint>`

The `<endpoint>` class has the following methods:

##### `endpoint-tcp-state@`
( endpoint -- tcp-state )

This word returns the current TCP state of an endpoint.

##### `endpoint-rx-data@`
( endpoint -- addr bytes )

This word returns the address and size in bytes of the current pending data for an endpoint.

##### `udp-endpoint?`
( endpoint -- udp? )

Get whether an endpoint is a UDP endpoint; note that if this returns false it means the endpoint is a TCP endpoint (by default endpoints are TCP endpoints).

##### `endpoint-local-port@`
( endpoint -- port )

Get the local port for *endpoint*. Note that if the local port for an endpoint had been specified as `net-consts::EPHEMERAL_PORT` this will be the actual local port.

#### `waiting-rx-data?`
( endpoint -- waiting? )

Get whether there is any data waiting on endpoint beyond the currently pending data.

### `net-ipv4`

The `net-ipv4` module contains the following classes:

#### `<ipv4-interface>`

The `<ipv4-interface>` class inherits from `net::<interface>` and has the following constructor:

##### `new`
( frame-interface interface -- )

This constructs an `<ipv4-interface>` instance for a given _frame interface_, an instance of `frame-interface::<frame-interface>`, which encapsulates a connection to the hardware network interface.

##### `intf-ipv4-addr@`
( interface -- addr )

Get an interface's IPv4 address.

##### `intf-ipv4-addr!`
( addr interface -- )

Manually set an interface's IPv4 address.

##### `intf-ipv4-netmask@`
( interface -- netmask )

Get an interface's IPv4 netmask.

##### `intf-ipv4-netmask!`
( netmask interface -- )

Manually set an interface's IPv4 netmask.

##### `gateway-ipv4-addr@`
( interface -- addr )

Get an interface's gateway's IPv4 address.

##### `gateway-ipv4-addr!`
( addr interface -- )

Manually set an interface's gateway's IPv4 address.

##### `dns-server-ipv4-addr@`
( interface -- addr )

Get an interface's DNS server's IPv4 address.

##### `dns-server-ipv4-addr!`
( addr interface -- )

Manually set an interface's gateway's IPv4 address.

##### `intf-ipv4-broadcast@`
( interface -- addr )

Get an interface's IPv4 broadcast address.

##### `intf-ttl@`
( interface -- tll )

Get an interface's TTL.

##### `intf-ttl!`
( ttl interface -- )

Set an interface's TTL.

##### `discover-ipv4-addr`
( interface -- )

Discover an interface's IPv4 address, IPv4 netmask, gateway IPv4 address, and DNS server IPv4 address via DHCP.

##### `send-ipv4-udp-packet`
( ? src-port dest-addr dest-port bytes xt interface -- ? success? )

This will call *xt* with a signature of ( ? buffer -- ? send? ) with an address of a buffer guaranteed to be of size *bytes* to construct a UDP packet with a payload consisting of that buffer from *src-port* (which may be any port from `net-consts::MIN_EPHEMERAL_PORT` to `net-consts::MAX_EPHEMERAL_PORT` if `net-consts::EPHEMERAL_PORT` is provided) to *dest-addr* at *dest-port*; if true is returned the packet is sent, and true will be returned afterwards, else the packet is not sent and false is returned. Note that false may be returned if the MAC address corresponding to *dest-addr* cannot be resolved.

##### `resolve-ipv4-addr-mac-addr`
( dest-addr interface -- D: mac-addr success? )

Attempt to resolve the MAC address of an IPv4 address; if successful, true and the MAC address are returned, else false and padding cells are returned.

##### `resolve-dns-ipv4-addr`
( c-addr bytes interface -- ipv4-addr success? )

Attempt to resolve the IPv4 address of a hostname via DNS; if successful, true and the IPv4 address are returned, else false and a apdding cell is returned.

##### `allocate-tcp-connect-ipv4-endpoint`
( src-port dest-addr dest-port interface -- endpoint success? )

Attempt to allocate a TCP endpoint connected to the IPv4 address *dest-addr* at *dest-port* from *src-port* (which may be any port from `net-consts::MIN_EPHEMERAL_PORT` to `net-consts::MAX_EPHEMERAL_PORT` if `net-consts::EPHEMERAL_PORT` is provided) and return true along with the endpoint, unless no endpoints are free, wheree then false is returned along with a padding cell.

#### `<ipv4-endpoint>`

The `<ipv4-endpoint>` class inherits from `net::<endpoint>` and has the following methods:

##### `endpoint-ipv4-remote@`
( endpoint -- ipv4-addr port )

This word returns the IPv4 address and port of, for a TCP endpoint, the peer of a connection, and for a UDP endpoint, the packet for the current pending data.

#### `<ipv4-handler>`

The `<ipv4-handler>` class handles receiving IP frames from a frame interface and passing them on to an instance of `<ipv4-interface>`.

The `<ipv4-handler>` class has the following constructor:

##### `new`
( interface handler -- )

Construct an IP frame handler for *interface*.

The `<ipv4-handler>` class has the following methods:

##### `handle-frame`
( addr bytes handler -- )

Handle a received frame.

##### `handle-refresh`
( handler -- )

Handle periodic housekeeping activities.

#### `<arp-handler>`

The `<arp-handler>` class handles receiving ARP frames from a frame interface and passing them onto an instance of `<interface>`.

The `<arp-handler>` class has the following constructor:

##### `new`
( interface handler -- )

Construct an ARP frame handler for *interface*.

The `<arp-handler>` class has the following methods:

##### `handle-frame`
( addr bytes handler -- )

Handle a received frame.

##### `handle-refresh`
( handler -- )

Handle periodic housekeeping activities.

### `net-consts`

The `net-consts` module contains the following words:

#### TCP states

There are the following TCP states:

##### `TCP_CLOSED`
##### `TCP_LISTEN`
##### `TCP_SYN_SENT`
##### `TCP_SYN_RECEIVED`
##### `TCP_ESTABLISHED`
##### `TCP_FIN_WAIT_1`
##### `TCP_FIN_WAIT_2`
##### `TCP_CLOSING`
##### `TCP_CLOSE_WAIT`
##### `TCP_LAST_ACK`
##### `TCP_TIME_WAIT`

#### Ports

##### `MIN_EPHEMERAL_PORT`

The minimum ephemeral port, 49152.

##### `MAX_EPHEMERAL_PORT`

The maximum ephemeral port, 65534 (port 65535 is use by zeptoIP for DNS).

##### `EPHEMERAL_PORT`

A token indicating that for a source port an ephemeral port is to be used. Ephemeral ports are chosen by choosing a random port between `MIN_EPHEMERAL_PORT` and `MAX_EPHEMERAL_PORT`, inclusive, on initialization, and then incrementing the ephemeral port each time an ephemeral port is needed, wrapping around within this range.

### `endpoint-process`

The `endpoint-process` module contains the following classes:

#### `<endpoint-handler>`

The `<endpoint-handler>` class is meant to be subclassed by the user, who shall provide an implementation of the `handle-endpoint` method.

The `<endpoint-handler>` class contains the following methods:

##### `handle-endpoint`
( endpoint process -- )

Handle a ready *endpoint*; note that *endpoint* should be marked as done before it is available for furthe processing. The user shall provide their own implementation of this method.

##### `handle-timeout`
( process -- )

Handle a timeout, based on the timeout returned by `handler-timeout@`.

##### `handler-timeout@`
( process -- timeout )

Get the current timeout for an endpoint handler in ticks from the present; note that `task::no-timeout` signifies there being no timeout.

#### `<endpoint-process>`

The `<endpoint-process>` class encapsulates a task carrying out the core loop of handling input and state changes on endpoints associated with a interface.

The `<endpoint-process>` class has the following constructor:

##### `new`
( interface process -- )

This constructor sets the interface for the endpoint processor to *interface*.

The `<endpoint-process>` class has the following methods:

##### `add-endpoint-handler`
( handler process -- )

Add a endpoint handler.

##### `run-endpoint-process`
( process -- )

This starts the task for processing endpoints on the chosen interface.

##### `endpoint-process-priority!`
( priority self -- )

Set the endpoint processor priority. The default endpoint processor priority is  0. This method can be called before `run-endpoint-process` to set the priority of the endpoint processor when it is started.

##### `endpoint-process-priority@`
( self -- priority )

Get the endpoint processor priority. The default endpoint processor priority is 0.

##### `endpoint-process-interval!`
( interval self -- )

Set the endpoint processor interval in ticks, which are normally 100 us increments. A negative interval means that the endpoint processor operates as a normal task, and -1 is the default value. This method can be called before `run-endpoint-process` to set the interval of the endpoint processor when it is started with.

##### `endpoint-process-interval@`
( self -- interval )

Get the endpoint processor interval in ticks, which are normally 100 us increments. A negative interval means that the endpoint processor operates as a normal task, and -1 is the default value.

##### `endpoint-process-deadline!`
( deadline self -- )

Set the endpoint processor deadline in ticks, which are normally 100 us increments, with the current time in ticks being `systick::systick-counter`. Note that this raises `x-endpoint-process-not-started` if called before `run-endpoint-process` has been called.

##### `endpoint-process-deadline@`
( self -- deadline )

Get the endpoint processor deadline in ticks, which are normally 100 us increments, with the current time in ticks being `systick::systick-counter`. Note that this raises `x-endpoint-process-not-started` if called before `run-endpoint-process` has been called.

### `frame-process`

The `frame-process` module contains the following classes:

#### `<frame-handler>`

The `<frame-handler>` class is meant to be subclassed by the user, who shall provide an implementation of the `handle-frame` and `handle-refresh` methods.

The `<frame-handler>` class contains the following method:

##### `handle-frame`
( addr bytes process -- )

Handle a frame of *bytes* at *addr*.

##### `handle-refresh`
( process -- )

Carry out periodic housekeeeping activities.

#### `<frame-process>`

The `<frame-process>` class encapsulates receiving frames from a frame interface and handling periodic housekeeping activties.

The `<frame-process>` class has the following constructor:

##### `new`
( frame-interface process -- )

This constructor sets the frame interface for the frame processor to *frame-interface*.

The `<frame-process>` class has the following methods:

##### `add-frame-handler`
( handler process -- )

Add a frame handler.

##### `run-frame-process`
( process -- )

This starts the task for processing frames and doing periodic housekeeping on the chosen frame interface.

### `frame-interface`

The `frame-interface` module encapsulates the interface between a network and zeptoIP, by providing a means of receiving and sending frames.

The `frame-interface` module contains the following class:

#### `<frame-interface>`

The `<frame-interface>` class is meant to be subclassed by implementors of interfaces to external networks.

The `<frame-interface>` class contains the following abstract methods:

##### `mtu-size@`
( frame-interface -- bytes )

Get the MTU size.

##### `mac-addr@`
( frame-interface -- D: mac-addr )

Get the MAC address.

##### `mac-addr!`
( D: mac-addr frame-interface -- )

Set the MAC address.

##### `put-rx-frame`
( addr bytes frame-interface -- )

Put a received frame.

##### `get-rx-frame`
( addr bytes frame-interface -- bytes' )

Get a received frame.

##### `poll-rx-frame`
( addr bytes frame-interface -- bytes' found? )

Poll a received frame.

##### `put-tx-frame`
( addr bytes frame-interface -- )

Put a frame to transmit.

##### `get-tx-frame`
( addr bytes frame-interface -- bytes' )

Get a frame to transmit.

##### `poll-tx-frame`
( addr bytes frame-interface -- bytes' found? )

Poll a frame to transmit.

### `simple-net-ipv4`

The `simple-net-ipv4` module contains the following word:

##### `x-endpoint-process-not-started`
( -- )

This exception is raised if one attempts to obtain the endpoint process from a `<simple-net-ipv4>` instance for which the endpoint process has not been started.

The `simple-net-ipv4` module contains the following class:

#### `<simple-net-ipv4>`

The `<simple-net-ipv4>` class is a base class for encapsulating a network interface driver and a zeptoIP network stack while simplifying their configuration.

It has the following constructor:

##### `new`
( driver -- )

This is a basic constructor that initializes the state of the `<simple-net-ipv4>` base class.

It has the following methods:

##### `init-net`
( driver -- )

This initializes a `<simple-net-ipv4>` instance.

##### `init-net-no-handler`
( driver -- )

This initalizes a `<simpl-net>` instance without starting an endpoint processing task.

##### `device-frame-interface@`
( driver -- frame-interface )

This gets the network interface driver's frame interface instance.

##### `net-interface@`
( driver -- interface )

This gets the zeptoIP interface instance.

##### `net-frame-process@`
( driver -- frame-processor )

This gets the zeptoIP frame processor instance.

##### `net-endpoint-process@`
( driver -- endpoint-processor )

This gets the zeptoIP endpoint processor instance. This raises `x-endpoint-process-not-started` if `init-cyw43-net` has not been called (e.g. if `init-cyw43-net-no-handler` has been called instead).

##### `run-net-process`
( driver -- )

This starts the zeptoIP frame and endpoint processors.

### `simple-cyw43-net-ipv4`

The `simple-cyw43-net-ipv4` module contains the following class:

#### `<simple-cyw43-net-ipv4>`

The `<simple-cyw43-net-ipv4>` class encapsulates a CYW43xxx driver and a zeptoIP network stack while simplifying their configuration.

It has the following constructor:

##### `new`
( pwr-pin dio-pin cs-pin clk-pin sm-index pio-instance driver -- )

This instantiates an instance with *pwr-pin*, *dio-pin*, *cs-pin*, and *pio-pin* being specified as the GPIO pins for communication with the CYW43xxx, and a PIO state machine *sm-index*, and a PIO instance (`pio::PIO0` or `pio::PIO1` or, on the RP2350, `pio::PIO2`) for the PIO program and state machine for implementing the half-duplex protocol for communicating with the CYW43xxx.

It has the following methods:

##### `init-cyw43-net`
( driver -- )

This method is an alias for `simple-net-ipv4::init-net`.

##### `init-cyw43-net-no-handler`
( driver -- )

This method is an alias for `simple-net-ipv4::init-net-no-handler`.

##### `cyw43-net-country!`
( abbrev-addr abbrev-bytes code-addr code-bytes country-rev self -- )

Initialize the country abbrevation, code, and revision; these default to `XX`, `XX`, and -1. Note that this must be called before `init-cyw43-net`, if it called at all.

##### `cyw43-gpio!`
( state gpio driver -- )

This sets a GPIO pin on the CYW43xxx.

##### `cyw43-control@`
( driver -- control )

This gets the CYW43xxx controller instance.

##### `net-interface@`
( driver -- interface )

This method is an alias for `simple-net-ipv4::net-interface@`.

##### `net-endpoint-process@`
( driver -- endpoint-processor )

This method is an alias for `simple-net-ipv4::net-endpoint-process@`.

##### `run-net-process`
( driver -- )

This method is an alias for `simple-net-ipv4::run-net-process`.

### `pico-w-cyw43-net-ipv4`

This `pico-w-cyw43-net-ipv4` class has the following class:

#### `<pico-w-cyw43-net-ipv4>`

The `<pico-w-cyw43-net-ipv4>` class inherits from the `<simple-cyw43-net-ipv4>` class, providing functionality specific to the Raspberry Pi Pico W.

It has the following constructor:

##### `new`
( sm-index pio-instance driver -- )

This instantiates an instance with a PIO state machine *sm-index*, and a PIO instance (`pio::PIO0` or `pio::PIO1` or, on the RP2350, `pio::PIO2`) for the PIO program and state machine for implementing the half-duplex protocol for communicating with the CYW43xxx.

It has the following methods:

##### `init-cyw43-net`
( driver -- )

This initializes a `<pico-w-cyw43-net-ipv4>` instance.

##### `init-cyw43-net-no-handler`
( driver -- )

This initalizes a `<pico-w-cyw43-net-ipv4>` instance without starting an endpoint processing task.

##### `pico-w-led!`
( state driver -- )

This sets the LED on the Raspberry Pi Pico W to *state*.

##### `pico-w-led@`
( driver -- state )

This gets the state of the LED on the Raspberry Pi Pico W.

##### `toggle-pico-w-led`
( driver -- )

This toggles the state of the LED on the Raspberry Pi Pico W.

### `net-misc`

The `net-misc` module contains a number of internal and convenience words, notably:

##### `make-ipv4-addr`
( addr0 addr1 addr2 addr3 -- addr )

Construct an IPv4 address, in the typical order, e.g.:

```
192 168 1 1 net-misc::make-ipv4-addr
```
##### `ipv4.`
( addr -- )

Print an IPv4 address formatted in the typical x.y.z.w format to the console.
