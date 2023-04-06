\ Copyright (c) 2023 Travis Bemann
\
\ Permission is hereby granted, free of charge, to any person obtaining a copy
\ of this software and associated documentation files (the "Software"), to deal
\ in the Software without restriction, including without limitation the rights
\ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
\ copies of the Software, and to permit persons to whom the Software is
\ furnished to do so, subject to the following conditions:
\ 
\ The above copyright notice and this permission notice shall be included in
\ all copies or substantial portions of the Software.
\ 
\ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

begin-module usb

  armv6m import

  \ Invalid packet size exception
  : x-invalid-packet-size ( -- ) ." invalid USB packet size" cr ;

  \ Convert byte endianness of a word
  : rev ( x -- x' ) [inlined] code[ tos tos rev_,_ ]code ;

  \ Convert halfword endianness of a word
  : rev16 ( x -- x' ) [inlined] code[ tos tos rev16_,_ ]code ;

  \ Write a halfword to the dictionary without concern for endianness
  : 2c, ( h -- ) dup c, 8 rshift c, ;


  \ USB registers base address
  $50110000 constant USB_Base

  \ USB setup packet address
  USB_Base constant USB_SETUP_PACKET
  
  \ USB dual-ported RAM size
  4096 constant dpram-size

  \ Descriptor types
  1 constant USB_DT_DEVICE
  2 constant USB_DT_CONFIG
  3 constant USB_DT_STRING
  4 constant USB_DT_INTERFACE
  5 constant USB_DT_ENDPOINT

  \ Requests
  0 constant USB_REQUEST_GET_STATUS
  1 constant USB_REQUEST_CLEAR_FEATURE
  3 constant USB_REQUEST_SET_FEATURE
  5 constant USB_REQUEST_SET_ADDRESS
  6 constant USB_REQUEST_GET_DESCRIPTOR
  7 constant USB_REQUEST_SET_DESCRIPTOR
  8 constant USB_REQUEST_GET_CONFIGURATION
  9 constant USB_REQUEST_SET_CONFIGURATION
  10 constant USB_REQUEST_GET_INTERFACE
  11 constant USB_REQUEST_SET_INTERFACE
  12 constant USB_REQUEST_SYNC_FRAME

  \ Device descriptor data
  create dev-data
  
  \ Device descriptor
  \ Descriptor length
  18 c,
  \ Descriptor type
  USB_DT_DEVICE c,
  \ USB specification number
  $0200 2c,
  \ Device class
  2 c,
  \ Device subclass
  0 c,
  \ Device protocol
  0 c,
  \ Maximum packet size
  64 c,
  \ Vendor ID
  $0483 2c,
  \ Product ID
  $5740 2c,
  \ Device release number
  $0200 2c,
  \ Manufacturer string descriptor index (0 = none)
  0 c,
  \ Product string descriptor index (0 = none)
  0 c,
  \ Serial number string descriptor index (0 = none)
  0 c,
  \ Number of possible configurations
  1 c,
  
  here cell align, swap - constant dev-data-size

  \ Configuration data
  create config-data

  \ Configuration descriptor
  \ Descriptor length
  9 c,
  \ Descriptor type (2 = configuration descriptor)
  USB_DT_CONFIG c,
  \ Total length of all descriptors
  67 2c,
  \ Number of interfaces
  2 c,
  \ Configuration index
  1 c,
  \ Configuration string descriptor index (0 = none)
  0 c,
  \ Attribute bitmap (bit 7 = bus powered, bit 6 = self powered)
  $C0 c,
  \ Maximum power in steps of 2 mA
  100 2 / c,

  \ Interface descriptor
  \ Descriptor length
  9 c,
  \ Descriptor type (4 = interface descriptor)
  USB_DT_INTERFACE c,
  \ Interface number
  0 c,
  \ Alternate setting
  0 c,
  \ Number of endpoints
  1 c,
  \ Class code
  2 c,
  \ Subclass code
  2 c,
  \ Protocol code
  0 c,
  \ Interface string descriptor index (0 = none)
  0 c,

  \ Header functional
  5 c, 36 c, 0 c, 16 c, 1 c,
  \ Call mangaement functional
  5 c, 36 c, 1 c, 0 c, 1 c,
  \ ACM functional
  4 c, 36 c, 2 c, 2 c,
  \ Union functional
  5 c, 36 c, 6 c, 0 c, 1 c,

  \ Endpoint descriptor
  \ Descriptor length
  7 c,
  \ Descriptor type (5 = endpoint descriptor)
  USB_DT_ENDPOINT c,
  \ Endpoint address (endpoint = 2, direction = in)
  130 c,
  \ Attributes (type = interrupt)
  3 c,
  \ Maximum packet size
  8 2c,
  \ Polling interval in ms (slow?)
  255 c,

  \ Interface descriptor
  \ Descriptor length
  9 c,
  \ Descriptor type (4 = interface descriptor)
  USB_DT_INTERFACE c,
  \ Interface number
  1 c,
  \ Alternate setting
  0 c,
  \ Number of endpoints
  2 c,
  \ Class code
  10 c,
  \ Subclass code
  0 c,
  \ Protocol code
  0 c,
  \ Interface string descriptor (0 = none)
  0 c,

  \ Endpoint descriptor
  \ Descriptor length
  7 c,
  \ Descriptor type (5 = endpoint descriptor)
  USB_DT_ENDPOINT c,
  \ Endpoint address (endpoint = 1, direction = out)
  1 c,
  \ Attributes (type = bulk)
  2 c,
  \ Maximum packet size
  64 c,
  \ Polling interval (ignored)
  0 c,

  \ Endpoint descriptor
  \ Descriptor length
  7 c,
  \ Descriptor type (5 = endpoint descriptor)
  USB_DT_ENDPOINT c,
  \ Endpoint address (endpoint = 1, direction = in)
  129 c,
  \ Attributes (type = bulk)
  2 c,
  \ Maximum packet size
  64 c,
  \ Polling interval
  0 c,

  here cell align, swap - constant config-data-size

  \ Endpoint control structure
  begin-structure endpoint-size
    
    \ Endpoint buffer address
    field: endpoint-buffer

    \ Endpoint control register address
    field: endpoint-control

    \ Endpoint buffer control register address
    field: endpoint-buffer-control

    \ Endpoint next PID
    field: endpoint-next-pid
    
  end-structure

  \ Setup packet structure
  begin-structure setup-packet-size

    \ Setup packet request type
    cfield: setup-packet-request-type

    \ Setup packet request
    cfield: setup-packet-request

    \ Setup packet value
    hfield: setup-packet-value

    \ Setup packet index
    hfield: setup-packet-index

    \ Setup packet length
    hfield: setup-packet-length
    
  end-structure

  \ Setup endpoint
  endpoint-size buffer: setup-endpoint

  \ Get the offset in USB dual-ported RAM
  : usb-buffer-offset ( addr -- addr' ) USB_Base - ;

  \ Set up a USB endpoint
  : usb-setup-endpoint { endpoint -- }
    endpoint endpoint-control @ if
      [ EP_CTRL_ENABLE EP_CTRL_INTERRUPT_PER_BUFFER or ]
      endpoint endpoint-buffer @ usb-buffer-offset or
    then
  ;

  \ Start a USB transfer
  : usb-start-transfer { c-addr bytes endpoint -- }
    bytes 64 u<= averts x-invalid-packet-size
    bytes USB_BUF_CTRL_AVAIL or { buffer-control-val }
    endpoint endpoint-tx? if
      c-addr bytes endpoint endpoint-buffer @ move
      buffer-control-val USB_BUF_CTRL_FULL or to buffer-control-val
    then
    endpoint endpoint-next-pid @ if
      USB_BUF_CTRL_DATA1_PID
    else
      USB_BUF_CTRL_DATA0_PID
    then
    buffer-control-val or to buffer-control-val
    endpoint endpoint-next-pid @ 1 xor endpoint endpoint-next-pid !
    buffer-control-val endpoint endpoint-buffer-control @ !
  ;

  \ Transfer the configuration, interface, and endpoint descriptors
  : usb-handle-config-descriptor ( -- )
  ;

  \ Handle a USB setup packet
  : usb-handle-setup-packet ( -- )
    \ Reset PID to 1
    1 setup-endpoint endpoint-next-pid !
    setup-packet setup-packet-request c@ { request }
    setup-packet setup-packet-request-type c@ case
      USB_DIR_OUT of
        setup-packet setup-packet-request c@ case
          USB_REQUEST_SET_ADDRESS of usb-set-device-address endof
          USB_REQUEST_SET_CONFIGURATION of usb-set-device-configuration endof
          endof
          usb-acknowledge-out-request
        endcase
      endof
      USB_DIR_IN of
        setup-packet setup-packet-request c@ case
          USB_REQUEST_GET_DESCRIPTOR of
            setup-packet setup-packet-value h@ 8 rshift case
              USB_DT_DEVICE of usb-handle-device-descriptor endof
              USB_DT_CONFIG of usb-handle-config-descriptor endof
              USB_DT_STRING of usb-handle-string-descriptor endof
            endcase
          endof
        endcase
      endof
    endcase
  ;
  
  \ Initialize USB
  : init-usb ( -- )
    \ Clear the DPSRAM just because
    USB_Base dpsram-size 0 fill

    
  ;
  
end-module
