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
  : x-invalid-pkt-size ( -- ) ." invalid USB packet size" cr ;

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

  \ USB endpoint in control
  : USB_EP_IN_CONTROL ( endpoint -- ) 3 lshift USB_Base + ;

  \ USB endpoint out control
  : USP_EP_OUT_CONTROL ( endpoint -- ) 3 lshift [ USB_Base cell+ ] literal + ;

  \ Endpoint control bits
  
  \ USB endpoint enable
  31 bit constant USB_EP_ENABLE

  \ USB double-buffered
  30 bit constant USB_EP_DOUBLE_BUFFER

  \ USB enable interrupt for every transferred buffer
  29 bit constant USB_EP_ENABLE_INTERRUPT_PER_BUFFER

  \ USB enable interrupt for every 2 transferred buffers (double-buffer only)
  28 bit constant USB_EP_ENABLE_INTERRUPT_PER_2_BUFFERS

  \ USB endpoint type bitshift
  26 constant USB_EP_ENDPOINT_TYPE_LSB

  \ USB endpoint interrupt on stall
  17 bit constant USB_EP_INTERRUPT_ON_STALL

  \ USB endpoint interrupt on NAK
  16 bit constant USB_EP_INTERRUPT_ON_NAK

  \ USB endpoint data buffer address bitshift
  0 constant USB_EP_ADDRESS_BASE_OFFSET_LSB

  \ Buffer control bits

  \ USB buffer 0 full bit
  15 bit constant USB_BUF_CTRL_FULL

  \ USB last buffer of transfer for buffer 0
  14 bit constant USB_BUF_CTRL_LAST

  \ USB buffer data pid 1
  13 bit constant USB_BUF_CTRL_DATA1_PID

  \ USB buffer data pid 0
  0 constant USB_BUF_CTRL_DATA0_PID

  \ USB reset buffer select to buffer 0
  12 bit constant USB_BUF_CTRL_RESET

  \ USB buffer stall
  11 bit constant USB_BUF_CTRL_STALL

  \ USB buffer is available for transfer
  10 bit constant USB_BUF_CTRL_AVAIL

  \ USB buffer 0 transfer length
  0 constant USB_BUF_CTRL_LENGTH0
  
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

  \ Endpoint types
  0 constant USB_EP_TYPE_CONTROL
  1 constant USB_EP_TYPE_ISO
  2 constant USB_EP_TYPE_BULK
  3 constant USB_EP_TYPE_INTERRUPT

  \ Configuration attributes
  7 bit constant USB_CONFIG_BUS_POWERED
  6 bit constant USB_CONFIG_SELF_POWERED
  
  \ Endpoint out address
  : USB_EP_OUT_ADDR ( u -- addr ) ;

  \ Endpoint in address
  : USB_EP_IN_ADDR ( u -- addr ) 128 + ;

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
  \ Descriptor type
  USB_DT_CONFIG c,
  \ Total length of all descriptors
  67 2c,
  \ Number of interfaces
  2 c,
  \ Configuration index
  1 c,
  \ Configuration string descriptor index (0 = none)
  0 c,
  \ Attribute bitmap
  USB_CONFIG_BUS_POWERED USB_CONFIG_SELF_POWERED or c,
  \ Maximum power in steps of 2 mA
  100 2 / c,

  \ Interface descriptor
  \ Descriptor length
  9 c,
  \ Descriptor type
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
  \ Descriptor type
  USB_DT_ENDPOINT c,
  \ Endpoint address
  2 USB_EP_IN_ADDR c,
  \ Attributes
  USB_EP_TYPE_INTERRUPT c,
  \ Maximum packet size
  8 2c,
  \ Polling interval in ms (slow?)
  255 c,

  \ Interface descriptor
  \ Descriptor length
  9 c,
  \ Descriptor type
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
  \ Descriptor type
  USB_DT_ENDPOINT c,
  \ Endpoint address
  1 USB_EP_OUT_ADDR c,
  \ Attributes
  USB_EP_TYPE_BULK c,
  \ Maximum packet size
  64 c,
  \ Polling interval (ignored)
  0 c,

  \ Endpoint descriptor
  \ Descriptor length
  7 c,
  \ Descriptor type
  USB_DT_ENDPOINT c,
  \ Endpoint address
  1 USB_EP_IN_ADDR c,
  \ Attributes
  USB_EP_TYPE_BULK c,
  \ Maximum packet size
  64 c,
  \ Polling interval
  0 c,

  here cell align, swap - constant config-data-size

  \ Global device address
  variable usb-device-addr

  \ Device address should be set
  variable set-usb-device-addr

  \ Device is configured
  variable usb-device-configd
  
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

    \ Endpoint maximum packet size
    field: endpoint-max-packet-size

    \ Endpoint transmit data
    field: endpoint-tx-data

    \ Endpoint transmit length
    field: endpoint-tx-len
    
  end-structure

  \ Setup packet structure
  begin-structure setup-pkt-size

    \ Setup packet request type
    cfield: setup-pkt-request-type

    \ Setup packet request
    cfield: setup-pkt-request

    \ Setup packet value
    hfield: setup-pkt-value

    \ Setup packet index
    hfield: setup-pkt-index

    \ Setup packet length
    hfield: setup-pkt-length
    
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

  \ Transfer data
  : usb-tx-next { endpoint -- }
    endpoint endpoint-tx-len @ endpoint endpoint-max-packet-size @
    min { bytes }
    bytes USB_BUF_CTRL_AVAIL or { buffer-control-val }
    endpoint endpoint-tx? if
      endpoint endpoint-tx-data bytes
      endpoint endpoint-buffer @ move
      bytes endpoint endpoint-tx-data +!
      buffer-control-val USB_BUF_CTRL_FULL or to buffer-control-val
    then
    bytes negate endpoint endpoint-tx-len +!
    endpoint endpoint-next-pid @ if
      USB_BUF_CTRL_DATA1_PID
    else
      USB_BUF_CTRL_DATA0_PID
    then
    buffer-control-val or to buffer-control-val
    endpoint endpoint-tx-len @ 0= if
      USB_BUF_CTRL_LAST buffer-control-val or to buffer-control-val
    then
    endpoint endpoint-next-pid @ 1 xor endpoint endpoint-next-pid !
    USB_EP_ENABLE_INTERRUPT_PER_BUFFER { ep-control-val }
    ep-control-val endpoint endpoint-control @ !
    buffer-control-val endpoint endpoint-buffer-control @ !
  ;

  \ Send a USB acknowledge out request
  : usb-ack-out-request ( -- )
    0 0 endpoint0-in usb-start-transfer
  ;

  \ Set a USB device address
  : usb-set-device-addr ( -- )
    setup-pkt setup-pkt-value h@ usb-device-address !
    true set-usb-device-addr !
    usb-ack-out-request
  ;

  \ Set a USB device configuration
  : usb-set-device-config ( -- )
    true usb-device-configd !
    usb-ack-out-request
  ;

  \ Transfer the configuration, interface, and endpoint descriptors
  : usb-handle-config-descr ( -- )
  ;

  \ Handle a USB setup packet
  : usb-handle-setup-pkt ( -- )
    \ Reset PID to 1
    1 setup-endpoint endpoint-next-pid !
    setup-pkt setup-pkt-request c@ { request }
    setup-pkt setup-pkt-request-type c@ case
      USB_DIR_OUT of
        setup-pkt setup-pkt-request c@ case
          USB_REQUEST_SET_ADDRESS of usb-set-device-addr endof
          USB_REQUEST_SET_CONFIGURATION of usb-set-device-config endof
          endof
          usb-acknowledge-out-request
        endcase
      endof
      USB_DIR_IN of
        setup-pkt setup-pkt-request c@ case
          USB_REQUEST_GET_DESCRIPTOR of
            setup-pkt setup-pkt-value h@ 8 rshift case
              USB_DT_DEVICE of usb-handle-device-descr endof
              USB_DT_CONFIG of usb-handle-config-descr endof
              USB_DT_STRING of usb-handle-string-descr endof
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
    0 usb-device-addr !
    false set-usb-device-addr !
    false usb-device-configd !
  ;
  
end-module
