# zeptoIP Words

zeptoIP is an Internet Protocol stack for zeptoforth. Currently it supports the Raspberry Pi Pico W's CYW43439 WiFi chip. By design it can be extended to any WiFi or Ethernet interface which exposes receiving and sending Ethernet frames. It expressly does not support WiFi interfaces such as the ESP8285 on the Wio RP2040 which operate by using their own protocols such as the "AT" protocol. Note that currently zeptoIP only supports IPv4.

zeptoIP is centered around one or more _interfaces_, instances of `net::<interface>`each of which has its own IP address, netmask, gateway IP address, and DNS server IP address. With interfaces sources of incoming (and in the case of TCP data, outgoing) data, known as _endpoints_, instances of `net::<endpoint>` may be created, endpoints with ready data or state changes can be fetched, UDP packets may be sent, IP addresses of hostnames can be resolved via DNS, and IP addresses may be acquired via DHCP.

Endpoints encapsulate connection state in the case of TCP, and received packet state in the case of UDP. Reading received data from an endpoint need not require copying the data; the received data associated with an endpoint is valid until the endpoint is marked as _done_, at which the received data is cleared from the endpoint's buffer and the room it took up in the endpoint's buffer is made available for more received data. Note that data continues to be received even between the time that ready endpoint is fetched with `net::get-ready-endpoint` and it is marked as done with `net::endpoint-done`. Also note that with UDP endpoints the ready data at any one data comprises only a single received UDP packet's data.

The typical means by which endpoints are serviced is by creating an _endpoint processor_, an instance of `endpoint-process::<endpoint-process>`, and registering an _endpoint handler_, an instance of `endpoint-process::<endpoint-handler>` with it. The endpoint processor involves a task which fetches ready endpoints one at a time from an interface, and then applies it to each register endpoint handler. Once all the endpoint handlers have processed the endpoint, it should be marked as done to make it available for new data or state changes.

Sending data is done differently with regard to TCP and UDP. With TCP sending data requires a TCP endpoint, gotten through either receiving an incoming connection on a listening TCP endpoint, started with `net::allocate-tcp-listen-endpoint`, which will result in the endpoint being readied with the appropriate state change(s), or starting a TCP connection with `net::allocate-tcp-connect-ipv4-endpoint`, at which an TCP endpoint will be provided to the user. Once one has done this, all that one must do is call `send-tcp-endpoint`, which will send the provided data unless the connection is closed or reset, where then it will exit prematurely. With UDP sending data simply requires executing `net::send-ipv4-udp-packet`.

Closing an endpoint is accomplished through `net::close-tcp-endpoint`, for TCP endpoints, and `net::close-udp-endpoint`, for UDP endpoints. Closing UDP endpoints is immediate, while closing TCP endpoints involves waiting for the endpoint to be closed after a four-way handshake.

If the user wishes to listen to multiple connections on the same point they must allocate multiple endpoints to listen on; incoming connections will be alloted to each of these endpoints.

Note that there is a fixed number of available endpoints which is set at compile-time, specified by the constant `net-config::max-endpoints`. This is set to four by default.

### `net`

The `net` module contains the following words:

##### `x-oversized-frame`
( -- )

This exception is raised if a frame larger than the MTU would be raised.

The `net` module contains the following classes:

#### `<interface>`

The `<interface>` class has the following constructor:

##### `new`
( frame-interface self -- )

This constructs an `<interface>` instance for a given _frame interface_, an instance of `frame-interface::<frame-interface>`, which encapsulates a connection to the hardware network interface.

##### `intf-ipv4-addr@`
( self -- addr )

Get an interface's IPv4 address.

##### `intf-ipv4-addr!`
( addr self -- )

Manually set an interface's IPv4 address.

##### `intf-ipv4-netmask@`
( self -- netmask )

Get an interface's IPv4 netmask.

##### `intf-ipv4-netmask!`
( netmask self -- )

Manually set an interface's IPv4 netmask.

##### `gateway-ipv4-addr@`
( self -- addr )

Get an interface's gateway's IPv4 address.

##### `gateway-ipv4-addr!`
( addr self -- )

Manually set an interface's gateway's IPv4 address.

##### `dns-server-ipv4-addr@`
( self -- addr )

Get an interface's DNS server's IPv4 address.

##### `dns-server-ipv4-addr!`
( addr self -- )

Manually set an interface's gateway's IPv4 address.

##### `intf-ipv4-broadcast@`
( self -- addr )

Get an interface's IPv4 broadcast address.

##### `intf-mac-addr@`
( self -- D: mac-addr )

Get an interface's MAC address.

##### `intf-ttl@`
( self -- tll )

Get an interface's TTL.

##### `intf-ttl!`
( ttl self -- )

Set an interface's TTL.

##### `discover-ipv4-addr`
( self -- )

Discover an interface's IPv4 address, IPv4 netmask, gateway IPv4 address, and DNS server IPv4 address via DHCP.

##### `send-tcp-endpoint`
( addr bytes endpoint self -- )

Send *bytes* at *addr* to the peer of *endpoint*. This will return when sending data is complete or if the connection is closed or its connection is not yet established.

##### `send-ipv4-udp-packet`
( ? src-port dest-addr dest-port bytes xt self -- ? success? )

This will call *xt* with a signature of ( ? buffer -- ? send? ) with an address of a buffer guaranteed to be of size *bytes* to construct a UDP packet with a payload consisting of that buffer from *src-port* (which may be any port from `net-consts::MIN_EPHEMERAL_PORT` to `net-consts::MAX_EPHEMERAL_PORT` if `net-consts::EPHEMERAL_PORT` is provided) to *dest-addr* at *dest-port*; if true is returned the packet is sent, and true will be returned afterwards, else the packet is not sent and false is returned. Note that false may be returned if the MAC address corresponding to *dest-addr* cannot be resolved.

##### `resolve-ipv4-addr-mac-addr`
( dest-addr self -- D: mac-addr success? )

Attempt to resolve the MAC address of an IPv4 address; if successful, true and the MAC address are returned, else false and padding cells are returned.

##### `resolve-dns-ipv4-addr`
( c-addr bytes self -- ipv4-addr success? )

Attempt to resolve the IPv4 address of a hostname via DNS; if successful, true and the IPv4 address are returned, else false and a apdding cell is returned.

##### `get-ready-endpoint`
( self -- endpoint )

Do a blocking wait to get the next ready endpoint; note that the user may use `task::timeout` to apply a timeout to this.

##### `endpoint-done`
( endpoint self -- )

Retire the current pending data for an endpoint and allow the endpoint to be readied again; if there is data already available, the endpoint will be readied again immediately.

##### `allocate-udp-listen-endpoint`
( port self -- endpoint success? )

Attempt to allocate a UDP endpoint listening on *port* (which may be any port from `net-consts::MIN_EPHEMERAL_PORT` to `net-consts::MAX_EPHEMERAL_PORT` if `net-consts::EPHEMERAL_PORT` is provided) and return true along with the endpoint, unless no endpoints are free, where then false is returned along with a padding cell.

##### `allocate-tcp-listen-endpoint`
( port self -- endpoint success? )

Attempt to allocate a TCP endpoint listening on *port* (which may be any port from `net-consts::MIN_EPHEMERAL_PORT` to `net-consts::MAX_EPHEMERAL_PORT` if `net-consts::EPHEMERAL_PORT` is provided) and return true along with the endpoint, unless no endpoints are free, where then false is returned along with a padding cell.

##### `allocate-tcp-connect-ipv4-endpoint`
( src-port dest-addr dest-port self -- endpoint success? )

Attempt to allocate a TCP endpoint connected to the IPv4 address *dest-addr* at *dest-port* from *src-port* (which may be any port from `net-consts::MIN_EPHEMERAL_PORT` to `net-consts::MAX_EPHEMERAL_PORT` if `net-consts::EPHEMERAL_PORT` is provided) and return true along with the endpoint, unless no endpoints are free, wheree then false is returned along with a padding cell.

##### `close-udp-endpoint`
( endpoint self -- )

Close a UDP endpoint. This is immediate, and any queued UDP packets will be lost.

##### `close-tcp-endpoint`
( endpoint self -- )

Close a TCP endpoint. This waits until the connection is normally closed or the connection is reset.

#### `<endpoint>`

The `<endpoint>` class has the following methods:

##### `endpoint-tcp-state@`
( self -- tcp-state )

This word returns the current TCP state of an endpoint.

##### `endpoint-rx-data@`
( self -- addr bytes )

This word returns the address and size in bytes of the current pending data for an endpoint.

##### `endpoint-ipv4-remote@`
( self -- ipv4-addr port )

This word returns the IPv4 address and port of, for a TCP endpoint, the peer of a connection, and for a UDP endpoint, the packet for the current pending data.

##### `udp-endpoint?`
( self -- udp? )

Get whether an endpoint is a UDP endpoint; note that if this returns false it means the endpoint is a TCP endpoint (by default endpoints are TCP endpoints).

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
