# Building and Using zeptoIP

zeptoIP is an IP stack for zeptoforth, which at the present is layered on top of a CYW43439 driver for the Raspberry Pi Pico W. This is currently in an alpha state, and is being made available for testing prior to becoming part of the zeptoforth master branch. Note that it cannot be used on top of any currently existing build of zeptoforth but rather requires the latest code in the `devel` branch.

zeptoIP, the CYW43439 driver, and the CYW43439 firmware on the Raspberry Pi Pico W require a `rp2040_big` platform build, because there is barely enough space with them on a standard `rp2040` platform build. This is at the expense of 512 KB of space for blocks, and leaves approximately 571 KB (at last check) of space available for code in flash (assuming 2 MB of flash). Note that for boards that do have more than 2 MB of flash available custom builds are needed to take advantage of this space.

## Uploading the CYW43439 firmware

The CYW43439 firmware is not in the zeptoforth Git repository for intellectual property reasons, i.e. to avoid unnecessarily mixing free with non-free code. Hence if one wishes to install the CYW43439 driver (and zeptoIP, most likely) on a Raspberry Pi Pico W, after flashing it with an `rp2040_big` platform `full` (for serial console) or `full_usb` (for USB console) build, one must manually upload the CYW43439 firmware prior to building the CYW43439 driver and zeptoIP. The recommended approach is to execute the following command:

```
utils/load_cyw43_fw.sh <your console tty device> <your 43439A0.bin path> <your 43439A0_clm.bin path>
```

`43439A0.bin` is mirrored [here](https://github.com/tabemann/cyw43-firmware/raw/master/cyw43439-firmware/43439A0.bin). `43439A0_clm.bin` is mirrored [here](https://github.com/tabemann/cyw43-firmware/raw/master/cyw43439-firmware/43439A0_clm.bin).

Note that this will be faster if one uses a USB CDC console device rather than a serial console device.

## Building the CYW43439 driver and zeptoIP

Once one has uploaded the CYW43439 firmware, both the CYW43439 driver and zeptoIP may be built together by executing:

```
utils/codeload3.sh -B 115200 -p <your console tty device> serial extra/rp2040/pico_w_net_all.fs
```

Note that once this is complete one must reboot the board prior to using zeptoIP or the CYW43439 driver. On the first reboot there likely will be a noticeable delay before the board responds, as it will be generating new entries in the flash dictionary index, which is stored in flash.

## Bringing up zeptoIP and the CYW43439 driver

Once one has built the CYW43439 driver and zeptoIP and rebooted, the simplest and easiest way to use zeptoIP and the CYW43439 driver is by making use of the `<pico-w-cyw43-net>` class in the `pico-w-cyw43-net` module. In the following example we show the initialization of an `<pico-w-cyw43-net>` instance:

```
oo import
pico-w-cyw43-net import
cyw43-control import
net import
endpoint-process import

0 constant pio-addr
0 constant sm-index
pio::PIO0 constant pio-instance

<pico-w-cyw43-net> class-size buffer: my-cyw43-net
0 value my-cyw43-control
0 value my-interface

pio-addr sm-index pio-instance <pico-w-cyw43-net> my-cyw43-net init-object
my-cyw43-net init-cyw43-net
my-cyw43-net cyw43-control@ to my-cyw43-control
my-cyw43-net net-interface@ to my-interface

```

Afterwards, we will probably want to connect to an access point (AP), typically an WPA2 AP:

```
: connect-wpa2-ap { D: ssid D: pass - }
  begin ssid pass my-cyw43-control join-cyw43-wpa2 nip until
  my-cyw43-control disable-all-cyw43-events
  my-cyw43-control clear-cyw43-events
  my-cyw43-net run-net-process
;

s" my-ssid-here" s" my-password-here" connect-wpa2-ap
```

Where "my-ssid-here" and "my-password-here" are the SSID and the password you wish to connect to, for a WPA2 AP.

If one instead wishes to connect to an open AP, execute the following instead:

```
: connect-open-ap { D: ssid - }
  begin ssid my-cyw43-control join-cyw43-open nip until
  my-cyw43-control disable-all-cyw43-events
  my-cyw43-control clear-cyw43-events
  my-cyw43-net run-net-process
;

s" my-ssid-here" connect-open-ap
```

Next, in most cases, you will want to acquire an IPv4 address, IPv4 netmask, gateway IPv4 address, and DNS server IPv4 address via DHCP. This can simply be carried out through the following:

```
cr ." Discovering IPv4 address..."
my-interface discover-ipv4-addr
my-interface intf-ipv4-addr@
cr ." IPv4 address: " net-misc::ipv4.
my-interface intf-ipv4-netmask@
cr ." IPv4 netmask: " net-misc::ipv4.
my-interface gateway-ipv4-addr@
cr ." Gateway IPv4 address: " net-misc::ipv4.
my-interface dns-server-ipv4-addr@
cr ." DNS server IPv4 address: " net-misc::ipv4.
my-cyw43-net toggle-pico-w-led
```

Printing out the addresses is not necessary, but is helpful if it is not known what IP address will be assigned. Likewise, toggling the LED is unnecessary, but is helpful if connectivity over the WiFi link is poor, and discovery of an IPv4 address is not necessarily immediate.

## Using zeptoIP

Once you have initialized zeptoIP you will probably want to actually use it. This encompasses resolving IP addresses with DNS, sending UDP packets, listening for and receiving IP packets, connecting to TCP endpoints, listening for incoming TCP connections, sending data over TCP, and receiving data from TCP.

### Resolving IP addresses with DNS

Continuing from the aforementioned code, to acquire a DNS address one may execute something like the following:

```
0 value my-resolved-ip

: resolve-my-ip { D: hostname -- ipv4-addr }
  my-interface resolve-dns-ipv4-addr not if
    [: ." hostname not found" cr ;] ?raise
  then
;

s" my.host.name.org" resolve-my-ip dup net-misc::ipv4. to my-resolved-ip
```

### Sending UDP packets

Once one has resolved an IPv4 address for a hostname one can send UDP packets to it. Take for instance the following:

```
4444 constant my-port
4445 constant their-port

: send-a-udp-packet { addr bytes src-port dest-ipv4-addr dest-port -- }
  addr bytes src-port dest-ipv4-addr dest-port bytes [: { addr bytes buf }
    addr buf bytes move true
  ;] my-interface send-ipv4-udp-packet not if
    [: ." unable to send packet (unable to find MAC address?)" cr ;] ?raise
  then
;

s" send me!" my-port my-resolved-ip their-port send-a-udp-packet
  
```

This sends a single UDP packet from our port, 4444, to the destination port, 4445, on the IPv4 address that we just resolved. Note that there is no guarantee that the packet will reach its destination, but in this test an exception is raised if the MAC address for our destination IPv4 address cannot be resolved.

### Receiving UDP packets

To receive UDP packets we use a mechanism that is rather different from now `recvfrom()` works with POSIX-compliant sytsems. We register a handler which handles endpoints that become ready, processes the data, and retires them once they are no longer needed. Take the following:

```
<endpoint-handler> begin-class <udp-handler> end-class

<udp-handler> begin-implement
  :noname { endpoint self -- }
    endpoint endpoint-local-port@ my-port = endpoint udp-endpoint? and if
      endpoint endpoint-ipv4-remote@ { src-addr src-port }
      endpoint endpoint-rx-data@ { data-addr data-bytes }
      cr ." UDP: " src-addr net-misc::ipv4. ." :" src-port .
      data-addr data-bytes over + dump
      my-cyw43-net toggle-pico-w-led
      endpoint my-interface endpoint-done
    then
  ; define handle-endpoint
end-implement

<udp-handler> class-size buffer: my-udp-handler
<udp-handler> my-udp-handler init-object

: register-udp-endpoint ( -- )
  my-udp-handler my-cyw43-net net-endpoint-process@ add-endpoint-handler
  my-port my-interface allocate-udp-listen-endpoint not if
    [: ." unable to allocate UDP listen endpoint (too many endpoints)" cr ;]
    ?raise
  then
;

register-udp-endpoint
```

Here we register an endpoint handler which, when a UDP packet is received on `my-port`, port 4444, the source IPv4 address and port of the packet along with the content of the packet are dumped to the console and the LED on the Raspberry Pi Pico W is toggled. Afterwards the endpoint is marked as done, permitting more data to be made available on it.

### Connecting to Hosts via TCP

To connect to a most via TCP we must allocate an outgoing endpoint for it. Also note that the local port for this endpoint must be unique; there is a mechanism for automatically allocating unique endpoints, which is to specify `net-consts::EPHEMERAL_PORT` for the local port. It should be noted that the number of local endpoints are limited; this is hardcoded as `net-consts::max-endpoints`, which can be changed as need be (but note that each endpoint takes up a non-negligible amount of memory).

To initiate an outgoing TCP connection, take the following:

```
0 value my-outgoing-endpoint

: register-tcp-endpoint ( dest-addr dest-port -- )
  EPHEMERAL_PORT dest-addr dest-port
  my-interface allocate-tcp-connect-ipv4-endpoint not if
    [: ." unable to allocate TCP outgoing endpoint (too many endpoints)" cr ;]
    ?raise
  then
  to my-outgoing-endpoint
;

my-resolved-ip their-port register-tcp-endpoint

```

Here we allocate an endpoint for the connection to the port `their-port` (4445) on the host at `my-resolved-ip` which we just had resolved. Note that the endpoint will not be immediately ready for sending data, as the connection will not hae been established yet.

### Sending Data to a Host via TCP

In the above example before we can send data, we must determine that the endpoint is read to send data. We do this by establishing an endpoint handler that simply determines that the connection is ready for sending data. Take the following:

```
sema import

sema-size buffer: my-send-semaphore
1 0 my-send-semaphore init-sema

: send-data ( -- )
  0 [:
    my-send-semaphore take
    s" This is a test, repeat, this is only a test"
    my-outgoing-endpoint my-interface send-tcp-endpoint
    my-outgoing-endpoint my-interface close-tcp-endpoint
  ;] 320 128 1024 task::spawn task::run
;

<endpoint-handler> begin-class <tcp-ready-handler> end-class

<tcp-ready-handler> begin-implement
  :noname { endpoint self -- }
    endpoint my-outgoing-endpoint = if
      endpoint endpoint-tcp-state@ { state }
      endpoint my-interface endpoint-done
      state net-consts::TCP_ESTABLISHED = if
        my-send-semaphore give
      then
    then
  ; define handle-endpoint
end-implement

<tcp-ready-handler> class-size buffer: my-tcp-ready-handler
<tcp-ready-handler> my-ready-tcp-handler init-object
my-tcp-ready-handler my-cyw43-net net-endpoint-process@ add-endpoint-handler
```

We create a binary semaphore for signalling to a task we create that the outgoing TCP endpoint that we just created is ready to send data, where then we send an example string of data. Then we create a handler which specifically handles the endpoint in question and checks for whether it is in `net-consts::TCP_ESTABLISHED` state, where then it signals the semaphore.

### Listening to and Receiving Data from Hosts via TCP

To listen for data via TCP we use a mechanism that is very similar to listening for data via UDP. We register a handler which handles endpoints that become ready, processes the data, and retires them once they are no longer needed. The primary differences are that connections are involved, and TCP connections have state (with the primary states in which data is to be received being `TCP_ESTABLISHED` and `TCP_CLOSE_WAIT`. Take the following:

```
<endpoint-handler> begin-class <tcp-handler> end-class

<tcp-handler> begin-implement
  :noname { endpoint self -- }
    endpoint endpoint-local-port@ my-port = endpoint udp-endpoint? not and if
      endpoint endpoint-tcp-state@ net-consts::TCP_ESTABLISHED = if
        endpoint endpoint-ipv4-remote@ { src-addr src-port }
        endpoint endpoint-rx-data@ { data-addr data-bytes }
        cr ." TCP: "src-addr net-misc::ipv4. ." :" src-port .
        data-addr data-bytes over + dump
        my-cyw43-net toggle-pico-w-led
      then
      endpoint endpoint-tcp-state@ net-consts::TCP_CLOSE_WAIT = if
        endpoint close-tcp-endpoint
      then
      endpoint my-interface endpoint-done
    then
  ; define handle-endpoint
end-implement

<tcp-handler> class-size buffer: my-tcp-handler
<tcp-handler> my-tcp-handler init-object

: register-tcp-listen-endpoint { port -- }
  my-tcp-handler my-cyw43-net net-endpoint-process@ add-endpoint-handler
  port my-interface allocate-tcp-listen-endpoint not if
    [: ." unable to allocate TCP listen endpoint (too many endpoints)" cr ;]
    ?raise
  then
;

my-port register-tcp-listen-endpoint
```

Here we register an endpoint handler which, when TCP data (when the connection is in a `TCP_ESTABLISHED` state) is registered from a connection on `my-port`, port 4444, the source IPv4 address and port of the packet along with the content of the packet are dumped to the console and the LED on the Raspberry Pi Pico W is toggled. If the connection is in `TCP_CLOSE_WAIT` state, indicating that the peer has initiated connection closure, the connection is closed. Afterwards the endpoint is marked as done, permitting more data to be made available on it.

Note that if one wishes to have multiple TCP connections on the same local port, multiple endpoints need to be registered; to listen to multiple connections on the same port one must register multiple listening endpoints on that port.
