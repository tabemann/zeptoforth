# Building and Using zeptoIP

zeptoIP is an IP stack for zeptoforth. It is at the present layered on top of a CYW43439 driver for WiFi on the Raspberry Pi Pico W and Raspberry Pi Pico 2 W along with an ENC28J60 driver for directly interfacing with 10 Mbit Ethernet. It has versions for both IPv4 and IPv6, which are largely separate but share some common code. These will be referred to as zeptoIPv4 and zeptoIPv6 respectively when it is needed to disambiguate the two. Also, while the CYW43439 driver is originally implemented specifically for the Raspberry Pi Pico W and Raspberry Pi Pico 2 W, it unofficially supports some other boards such as the Pimoroni Pico Plus 2 W.

The CYW43439 driver, and the CYW43439 firmware on the Raspberry Pi Pico W require a `rp2040_big` platform build, because there is barely enough space with them on a standard `rp2040` platform build. This is at the expense of 512 KB of space for blocks, and leaves approximately 571 KB (at last check) of space available for code in flash (assuming 2 MB of flash). Note that for boards that do have more than 2 MB of flash available custom builds are needed to take advantage of this space. This is not an issue on the Raspberry Pi Pico 2 W, where a standard `rp2350` platform build will do.

Note that zeptoIP, the CYW43439, and the ENC28J60 driver normally live on core 1 of the RP2040 and RP2350, so it is a good idea to leave these cores be and run one's user code on core 0.

## zeptoIP tools

There are a number of pre-written tools for use with zeptoIPv4 and zeptoIPv6. These all assume the CYW43439 is being used on a Raspberry Pi Pico W, Raspberry Pi Pico 2W, or Pimoroni Pico Plus 2W.

To use them one first loads `extra/rp_common/net_tools/pico_w_ipv4_base.fs` or `extra/rp_common/net_tools/pico_w_ipv6_base.fs` depending on whether one wants to use zeptoIPv4 or zeptoIPv6, respectively.

Then one executes the following command:

```
<your WiFi SSID> <your WiFi password> <your Multicast DNS hostname> pico-w-net::init-wifi
```

\<your WiFi SSID>, \<your WiFi password>, and \<your Multicast DNS hostname> are all Forth address/length pair strings, which are saved so they need not stay valid after executing this line.

Note that Multicast DNS will be enabled, and your Multicast DNS hostname will be set as specified; note that this hostname should end in `.local`. You will be able to connect to your board via this hostname from any system on your local network link that supports Multicast DNS.

After you enter this command, it may take a moment before you get a response indicating relevant IP addresses. If it appears to fail to respond or it takes a long time before it responds you may have weak connectivity with your WiFi router.

Once a WiFi connection is established you should get an output such as the following (with varying values replacing the text in angle brackets) for zeptoIPv4:

```
Discovering IPv4 address...
IPv4 address: <your IPv4 address>
IPv4 netmask: <your IPv4 netmask>
Gateway IPv4 address: <your gateway IPv4 address>
DNS server IPv4 address: <your DNS server IPv4 address> ok
```

For zeptoIPv6 you should instead get an output such as the following:

```
Autoconfiguring link-local IPv6 address... <Success or Failure>
Discovering IPv6 router...
Discovering IPv6 address... <Success or Failure>
Primary IPv6 address: <your primary IPv6 address>
Link-local IPv6 address: <your link-local IPv6 address>
SLAAC IPv6 address: <your SLAAC IPv6 address; note that this may not be valid>
DHCPv6 IPv6 address: <your DHCPv6 IPv6 address; note that this may not be valid>
IPv6 prefix: <your IPv6 prefix>
Autonomous: <yes or no>
IPv6 prefix length: <your IPv6 prefix length>
Gateway IPv6 address: <your gateway IPv6 address>
Discovering IPv6 DNS server...
DNS server IPv6 address: <your DNS server IPv6 address>
```

Then one can select one or more of the following programs to load for zeptoIPv4:

* `extra/rp_common/net_tools/pico_w_ntp_ipv4.fs`, an NTP client that makes use of `extra/{rp2040,rp2350}/net/net_ntp_ipv4.fs`, which must be loaded before it; this tool persistently makes use of one endpoint
* `extra/rp_common/net_tools/pico_w_http_file_ipv4.fs`, an HTTP client which saves a downloaded file to a file in a FAT32 filesystem
* `extra/rp_common/net_tools/pico_w_http_dump_ipv4.fs`, an HTTP client which dumps a downloaded file and its HTTP headers to the console after converting LF newlines to CRLF newlines
* `extra/rp_common/net_tools/pico_w_repl_ipv4.fs`, a TCP console server; note that it has no security at all, so do not expose it to the open Internet
* `extra/rp_common/net_tools/pico_w_udp_echo_ipv4.fs`, a UDP echo server; note that it has no way of verifying that incoming UDP packets are coming from where they say they are coming from, so do not expose it to the open Internet
* `extra/rp_common/net_tools/pico_w_led_http_ipv4.fs`, a simple HTTP server for controlling the LED on your board

There are the following similar programs to load for zeptoIPv6:

* `extra/rp_common/net_tools/pico_w_ntp_ipv6.fs`, an NTP client that makes use of `extra/{rp2040,rp2350}/net/net_ntp_ipv6.fs`, which must be loaded before it; this tool persistently makes use of one endpoint
* `extra/rp_common/net_tools/pico_w_http_file_ipv6.fs`, an HTTP client which saves a downloaded file to a file in a FAT32 filesystem
* `extra/rp_common/net_tools/pico_w_http_dump_ipv6.fs`, an HTTP client which dumps a downloaded file and its HTTP headers to the console after converting LF newlines to CRLF newlines
* `extra/rp_common/net_tools/pico_w_repl_ipv6.fs`, a TCP console server; note that it has no security at all, so do not expose it to the open Internet
* `extra/rp_common/net_tools/pico_w_udp_echo_ipv6.fs`, a UDP echo server; note that it has no way of verifying that incoming UDP packets are coming from where they say they are coming from, so do not expose it to the open Internet
* `extra/rp_common/net_tools/pico_w_led_http_ipv6.fs`, a simple HTTP server for controlling the LED on your board

## Uploading the CYW43439 firmware

The CYW43439 firmware is not in the zeptoforth Git repository for intellectual property reasons, i.e. to avoid unnecessarily mixing free with non-free code. Hence if one wishes to install the CYW43439 driver (and zeptoIP, most likely) on a Raspberry Pi Pico W, after flashing it with an `rp2040_big` platform `full` (for serial console) or `full_usb` (for USB console) build, or on a Raspberry Pi Pico 2 W, after flashing it with a `rp2350` platform `full` or `full_usb` build, one must manually upload the CYW43439 firmware prior to building the CYW43439 driver and zeptoIP. The recommended approach is to execute the following command:

```
utils/load_cyw43_fw.sh <your console tty device> <your 43439A0.bin path> <your 43439A0_clm.bin path>
```

`43439A0.bin` is mirrored [here](https://github.com/tabemann/cyw43-firmware/raw/master/cyw43439-firmware/43439A0.bin). `43439A0_clm.bin` is mirrored [here](https://github.com/tabemann/cyw43-firmware/raw/master/cyw43439-firmware/43439A0_clm.bin).

Note that this will be faster if one uses a USB CDC console device rather than a serial console device.

## Building the CYW43439 driver and zeptoIP

Once one has uploaded the CYW43439 firmware, both the CYW43439 driver and zeptoIPv4 may be built together by executing:

```
utils/codeload3.sh -B 115200 -p <your console tty device> serial extra/rp_common/pico_w_net_ipv4_all.fs
```

The same can be done for the CYW43439 driver and zeptoIPv6 with:

```
utils/codeload3.sh -B 115200 -p <your console tty device> serial extra/rp_common/pico_w_net_ipv6_all.fs
```

Note that once this is complete one must reboot the board prior to using zeptoIP or the CYW43439 driver. On the first reboot there likely will be a noticeable delay before the board responds, as it will be generating new entries in the flash dictionary index, which is stored in flash.

## Building the ENC28J60 driver and zeptoIP

The process for installing the ENC28J60 driver is similar to the CYW43439 driver except no firmware for the ENC28J60 needs to be uploaded, and one uses the following to install the ENC28J60 driver and zeptoIPv4:

```
utils/codeload3.sh -B 115200 -p <your console tty device> serial extra/rp_common/enc28j60_net_ipv4_all.fs
```

The same can be done for the ENC28J60 driver and zeptoIPv6 with:

```
utils/codeload3.sh -B 115200 -p <your console tty device> serial extra/rp_common/enc28j60_net_ipv6_all.fs
```

## Bringing up zeptoIP and the CYW43439 driver

Once one has built the CYW43439 driver and zeptoIP and rebooted, the simplest and easiest way to use zeptoIP and the CYW43439 driver is by making use of the `<pico-w-cyw43-net-ipv4>` class in the `pico-w-cyw43-net-ipv4` module, for zeptoIPv4, or the `<pico-w-cyw43-net-ipv6>` class in the `pico-w-cyw43-net-ipv6` module, for zeptoIPv6. In the following example we show the initialization of an `<pico-w-cyw43-net-ipv4>` instance:

```
oo import
simple-cyw43-net-ipv4 import
pico-w-cyw43-net-ipv4 import
cyw43-control import
net import
net-ipv4 import
endpoint-process import

0 constant sm-index
pio::PIO0 constant pio-instance

<pico-w-cyw43-net-ipv4> class-size buffer: my-cyw43-net
0 value my-cyw43-control
0 value my-interface

sm-index pio-instance <pico-w-cyw43-net-ipv4> my-cyw43-net init-object
s" XX" s" XX" -1 my-cyw43-net cyw43-net-ipv4-country! \ This is optional
my-cyw43-net init-cyw43-net-ipv4
my-cyw43-net cyw43-control@ to my-cyw43-control
my-cyw43-net net-interface@ to my-interface

```

The line above:

```
s" XX" s" XX" -1 my-cyw43-net cyw43-net-ipv4-country! \ This is optional
```

specifies the default country settings, but under some circumstances one might to specify a different setting. In particular, the default country code/abbreviation, `XX`, which is a catch-all international country code, excludes the use of 802.11n, and some routers are 802.11n-only. Take, for instance, the settings for the UK, which would be:


```
s" GB" s" GB" 0 my-cyw43-net cyw43-net-ipv4-country!
```

For a list of abbreviations and codes (which are normally identical) and revisions see `extra/rp_common/cyw43/COUNTRIES.md`.

Afterwards, we will probably want to connect to an access point (AP), typically an WPA2 AP:

```
: connect-wpa2-ap { D: ssid D: pass -- }
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
: connect-open-ap { D: ssid -- }
  begin ssid my-cyw43-control join-cyw43-open nip until
  my-cyw43-control disable-all-cyw43-events
  my-cyw43-control clear-cyw43-events
  my-cyw43-net run-net-process
;

s" my-ssid-here" connect-open-ap
```

For the above if one wants to use zeptoIPv6 rather than zeptoIPv4, just substitute all references to `ipv4` with `ipv6`.

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

To manually set a static IPv4 address, IPv4 netmask, gateway IPv4 address, and DNS server IPv4 address one can execute something like the following in the place of the code above:

```
192 168 1 1 net-misc::make-ipv4-addr my-interface intf-ipv4-addr!
255 255 255 0 net-misc::make-ipv4-addr my-interface intf-ipv4-netmask!
192 168 1 254 net-misc::make-ipv4-addr my-interface gateway-ipv4-addr!
8 8 8 8 net-misc::make-ipv4-addr my-interface dns-server-ipv4-addr!
```

Replace the values with those for your configuration.

Setting up zeptoIPv6 is slightly more complex. First one will need to configure one's link-local address. That can be accomplished with:

```
cr ." Autoconfiguring link-local IPv6 address... "
my-interface autoconfigure-link-local-ipv6-addr if
  ." Success"
else
  ." Failure"
then
```

Then, in most configurations, one will need to discover one's router. That can be accomplished with:

```
cr ." Discovering IPv6 router..."
my-interface discover-ipv6-router
```

After that, provided one has discovered one's router, one will typically want to discover one's global IPv6 address with SLAAC or DHCPv6. Note that this blocks indefinitely waiting for positive confirmation from a DHCPv6 server if the router has specified that the client is to obtain its address from DHCPv6 even if SLAAC is also enabled. This will always be successful except in the use case where a collision with an existing node is detected, where then a failure will be reported. This is accomplished with:

```
cr ." Discovering IPv6 address..."
my-interface discover-ipv6-addr if
  ." Success"
else
  ." Failure"
then
```

Last but not least, if one plans on using DNS one will want to ensure that a DNS server has been found. Note that this step does not in itself find a DNS server, as it may have been found during router discovery or already been found via DHCPv6, but if stateless DHCPv6 is used `discover-ipv6-addr` may return prior to the DNS server address being found. This is accomplished with:

```
cr ." Discovering IPv6 DNS server..."
my-interface discover-dns-ipv6-addr
```

To print out the configuration of your IPv6 interface, you can execute the following:

```
my-interface intf-ipv6-addr@ cr ." IPv6 address: " net-misc::ipv6.
my-interface intf-ipv6-prefix@ cr ." IPv6 prefix: " net-misc::ipv6.
my-interface intf-autonomous@ cr ." Autonomous: " if ." yes" else ." no" then
my-interface intf-ipv6-prefix-len@ cr ." IPv6 prefix length: " .
my-interface gateway-ipv6-addr@ cr ." Gateway IPv6 address: " net-misc::ipv6.
my-interface dns-server-ipv6-addr@ cr ." DNS server IPv6 address: " net-misc::ipv6.
```

Manually configuring an IPv6 interface is similar to manually configuring an IPv4 interface, except that IPv6 addresses are used instead of IPv4 addresses, and an IPv6 prefix and prefix length are used instead of an IPv4 netmask.

## Bringing up the ENC28J60 driver and zeptoIP

Bringing up the ENC28J60 driver and zeptoIP is similar to bringing up CYW43439 driver and zeptoIP, except one uses `simple-enc28j60-net-ipv4::<simple-enc28j60-net-ipv4>` for zeptoIPv4 or `simple-enc28j60-net-ipv6::<simple-enc28j60-net-ipv6>` for zeptoIPv6 as follows (for zeptoIPv4; replace references to `ipv4` with `ipv6` for zeptoIPv6):

```
simple-enc28j60-net-ipv4 import

<simple-enc28j60-net-ipv4> class-size buffer: my-enc28j60-net
0 value my-interface

int-pin spi-pin spi-device mac-address duplex <simple-enc28j60-net-ipv4> my-enc28j60-net init-project
my-enc28j60-net init-net
my-enc28j60-net net-interface@ to my-interface
```

where `int-pin` is the interrupt pin GPIO index, `spi-pin` is the first GPIO index of four pins for the SPI interface, `spi-device` is the SPI interface index, `mac-address` is a double-cell value for the MAC address for the Ethernet interface, and `duplex` is whether the Ethernet interface is full-duplex (consult with your switch to determine whether it is full or half-duplex).

Note that the ENC28J60 driver currently monopolizes the GPIO and DMA interrupt vectors, even though this may change in the future.

After this point the only differences between bringing up the ENC28J60 and the CYW43439 with zeptoIP is that there is no setting a country or connecting to an AP.

## Using zeptoIP

Once you have initialized zeptoIP you will probably want to actually use it. This encompasses resolving IP addresses with DNS, sending UDP packets, listening for and receiving IP packets, connecting to TCP endpoints, listening for incoming TCP connections, sending data over TCP, and receiving data from TCP.

### Resolving IP addresses with DNS

Continuing from the aforementioned code, to resolve a DNS A record (i.e. an IPv4 address from a DNS name) one may execute something like the following:

```
0 value my-resolved-ip

: resolve-my-ip { D: hostname -- ipv4-addr }
  my-interface resolve-dns-ipv4-addr not if
    [: ." hostname not found" cr ;] ?raise
  then
;

s" my.host.name.org" resolve-my-ip dup net-misc::ipv4. to my-resolved-ip
```

To resolve a DNS AAAA record (i.e an IPv6 address from a DNS name) one may execute something like the following:

```
4 cells buffer: my-resolved-ip

: resolve-my-ip
{ D: hostname -- ipv6-addr-0 ipv6-addr-1 ipv6-addr-2 ipv6-addr-3 }
  my-interface resolve-dns-ipv6-addr not if
    [: ." hostname not found" cr ;] ?raise
  then
;

s" my.host.name.org" resolve-my-ip my-resolved-ip net-misc::ipv6-unaligned!
my-resolved-ip net-misc::ipv6-unaligned@ net-misc::ipv6.
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

The process is similar for IPv6 addresses, as you can see below:

```
4444 constant my-port
4445 constant their-port

: send-a-udp-packet
  { addr bytes src-port dest-ipv6-0 dest-ipv6-1 dest-ipv6-2 dest-ipv6-3 dest-port -- }
  addr bytes src-port dest-ipv4-0 dest-ipv6-1 dest-ipv6-2 dest-ipv6-3 dest-port bytes [: { addr bytes buf }
    addr buf bytes move true
  ;] my-interface send-ipv6-udp-packet not if
    [: ." unable to send packet (unable to find MAC address?)" cr ;] ?raise
  then
;

s" send me!" my-port my-resolved-ip net-misc::ipv6-unaligned@ their-port send-a-udp-packet
  
```

### Receiving UDP packets

To receive UDP packets we use a mechanism that is rather different from now `recvfrom()` works with POSIX-compliant sytsems. We register a handler which handles endpoints that become ready, processes the data, and retires them once they are no longer needed. Take the following example in the case of IPv4:

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

The code is almost identical for IPv6 except that one uses `endpoint-ipv6-remote@` and the remote addresses are 4 cells rather than a single cell:

```
<endpoint-handler> begin-class <udp-handler> end-class

<udp-handler> begin-implement
  :noname { endpoint self -- }
    endpoint endpoint-local-port@ my-port = endpoint udp-endpoint? and if
      endpoint endpoint-ipv6-remote@
      { src-addr-0 src-addr-1 src-addr-2 src-addr-3 src-port }
      endpoint endpoint-rx-data@ { data-addr data-bytes }
      cr ." UDP: " src-addr-0 src-addr-1 src-addr-2 src-addr-3 net-misc::ipv6. ." :" src-port .
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

### Connecting to Hosts via TCP

To connect to a most via TCP we must allocate an outgoing endpoint for it. Also note that the local port for this endpoint must be unique; there is a mechanism for automatically allocating unique endpoints, which is to specify `net-consts::EPHEMERAL_PORT` for the local port. It should be noted that the number of local endpoints are limited; this is hardcoded as `net-consts::max-endpoints`, which can be changed as need be (but note that each endpoint takes up a non-negligible amount of memory).

To initiate an outgoing TCP connection over IPv4, take the following:

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

The code is very similar with IPv6 except that one uses `allocate-tcp-connect-ipv6-endpoint` and the addresses are 4 cells rather than one cell in size:

```
0 value my-outgoing-endpoint

: register-tcp-endpoint
  ( dest-addr-0 dest-addr-1 dest-addr-2 dest-addr-3 dest-port -- )
  EPHEMERAL_PORT dest-addr-0 dest-addr-1 dest-addr-2 dest-addr-3 dest-port
  my-interface allocate-tcp-connect-ipv6-endpoint not if
    [: ." unable to allocate TCP outgoing endpoint (too many endpoints)" cr ;]
    ?raise
  then
  to my-outgoing-endpoint
;

my-resolved-ip net-misc::ipv6-unaligned@ their-port register-tcp-endpoint

```

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
        cr ." TCP: " src-addr net-misc::ipv4. ." :" src-port .
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

If one wants to use IPv6, the code is very similar except that one uses `endpoint-ipv6-remote@` and the addresses are 4 cells rather than one cell in size:

```
<endpoint-handler> begin-class <tcp-handler> end-class

<tcp-handler> begin-implement
  :noname { endpoint self -- }
    endpoint endpoint-local-port@ my-port = endpoint udp-endpoint? not and if
      endpoint endpoint-tcp-state@ net-consts::TCP_ESTABLISHED = if
        endpoint endpoint-ipv6-remote@
        { src-addr-0 src-addr-1 src-addr-2 src-addr-3 src-port }
        endpoint endpoint-rx-data@ { data-addr data-bytes }
        cr ." TCP: " src-addr-0 src-addr-1 src-addr-2 src-addr-3 net-misc::ipv6. ." :" src-port .
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

### Enabling Multicast DNS

Multicast DNS can be enabled under both zeptoIPv4 and zeptoIPv6 by executing:

```
true my-interface mdns-enabled!
```

This will by itself enable resolving DNS names on the `.local` domain via Multicast DNS.

To go further, and to expose a DNS name on the `.local` domain for your device via Multicast DNS, also execute, say:

```
s" myhostname.local" my-interface mdns-hostname!
```

Replace `s" myhostname.local"` with a string containing your choice of hostnames. Note that the hostname is saved internally in a buffer so does not need to remain valid after `mdns-hostname!` returns.

Also note that passing an empty string into `mdns-hostname!` disables exposing aDNS name via Multicast DNS. This is equivalent to the situation on initialization.
