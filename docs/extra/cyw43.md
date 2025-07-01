# CYW43xxx Words

A driver for CYW43xxx WiFi chips is included with zeptoforth. The external API for controlling them is provided by the `<cyw43-control>` class in the `cyw43-control` module. This class exposes a frame interface and a means of retrieving WiFi interface events.

The constant `select-rx-frame-count` specifies the multiple of the Ethernet frame size used for the CYW43xxx receive frame buffer when it exists. This constant, when it exists, must be globally visible, must be in the flash dictionary, and must exist prior to compiling the source code for the CYW43xxx driver.

The constant `select-tx-frame-count` specifies the multiple of the Ethernet frame size used for the CYW43xxx transmit frame buffer when it exists. This constant, when it exists, must be globally visible, must be in the flash dictionary, and must exist prior to compiling the source code for the CYW43xxx driver.

### `cyw43-control`

The `cyw43-control` module contains the following constant:

##### `default-mac-addr`
( -- D: default-mac-addr )

This is a token when provided to the `<cyw43-control>` class when instatiated indicates that the default MAC address is to be used.

The `cyw43-control` module contains the following class:

#### `<cyw43-control>`

The `<cyw43-control>` class has the constructor:

##### `new`
( D: mac-addr clm-addr clm-bytes fw-addr fw-bytes pwr clk dio cs pio-addr sm pio -- )

This instantiates a `<cyw43-control>` instance to use the MAC address *mac-addr* (which if `default-mac-addr` indicates that the default MAC address is to be used), CLM firmware of *clm-bytes* at *clm-addr*, main firmware of *fw-bytes* at *fw-addr*, *pwr*, *clk*, *dio*, and *cs* GPIO pins for communication with the CYW43xxx, and PIO state machine index *sm*, and PIO instance *pio* (`pio::PIO0` or `pio::PIO1`).

Note that *pio-addr* is no longer used, but it is retained in the argument list for backward compatibility.  The CYW43xxx driver uses `alloc-piomem` to obtain space for its PIO program.  If you need to place other PIO programs in the same PIO instance, you need to use `alloc-piomem` for those as well to avoid PIO memory addressing conflicts.

The `<cyw43-control>` class has the following methods:
      
##### `init-cyw43`
( self -- )

Initialize the CYW43.

##### `cyw43-country!`
( abbrev-addr abbrev-bytes code-addr code-bytes country-rev self -- )

Initialize the country abbrevation, code, and revision; these default to `XX`, `XX`, and -1. Note that this must be called before `init-cyw43`, if it called at all.

##### `cyw43-power-management!`
( pm self -- )

Set power management to *pm*, defined in the module `cyw43-consts`.

##### `join-cyw43-open`
( ssid-addr ssid-bytes self -- status success? )

Join an open AP. *ssid-addr* and *ssid-bytes* comprise the SSID of the AP being connected to. On success *success?* of true and *status* of 0 will be returned.

##### `join-cyw43-wpa2`
( ssid-addr ssid-bytes pass-addr pass-bytes self -- status success? )

Join a WPA2 AP. *ssid-addr* and *ssid-bytes* comprise the SSID of the AP being connected to. *pass-addr* and *pass-bytes* comprise the password of the AP being connected to. On success *success?* of true and *status* of 0 will be returned.

##### `start-cyw43-open`
( ssid-addr ssid-bytes channel self -- )

Start open AP. *ssid-addr* and *ssid-bytes* comprise the SSID of the AP being started. *channel* comprises the channel of the AP being started.

##### `start-cyw43-wpa2`
( ssid-addr ssid-bytes pass-addr pass-bytes channel self -- )

Start WPA2 AP. *ssid-addr* and *ssid-bytes* comprise the SSID of the AP being started. *pass-addr* and *pass-bytes* comprise the password of the AP being started. *channel* comprises the channel of the AP being started.

##### `cyw43-gpio!`
( val index self -- )

Set a GPIO on the CYW43xxx.

##### `enable-cyw43-event`
( event self -- )

Enable an event. *event* is an `EVENT_*` value found in the `cyw43-events` module.

##### `enable-cyw43-events`
( event-addr event-count self -- )

Enable multiple events. *event-addr* is an array of *event-count* bytes, each being an `EVENT_*` value found in the `cyw43-events` module.
    
##### `disable-cyw43-event`
( event self -- )

Disable an event. *event* is an `EVENT_*` value found in the `cyw43-events` module.

##### `disable-cyw43-events`
( event-addr event-count self -- )

Disable multiple events. *event-addr* is an array of *event-count* bytes, each being an `EVENT_*` value found in the `cyw43-events` module.

##### `disable-all-cyw43-events`
( self -- )

Disable all events.
    
##### `cyw43-frame-interface@`
( self -- interface )

Get the CYW43 frame interface.
    
##### `get-cyw43-event`
( addr self -- )

Carry out a blocking dequeue of an event message. *addr* points to a block of memory for an event message of `cyw43-events::event-message-size` size.

##### `poll-cyw43-event`
( addr self -- found? )

Poll for an event message. *addr* points to a block of memory for an event message of `cyw43-events::event-message-size` size. Its contents are only valid if *found?* is true, indicating that an event was found.

##### `clear-cyw43-events`
( self -- )

Clear the event queue.
