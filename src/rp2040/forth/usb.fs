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
  interrupt import
  task import

  begin-module usb-internal

    \ Invalid packet size exception
    : x-invalid-pkt-size ( -- ) ." invalid USB packet size" cr ;

    \ Convert byte endianness of a word
    : rev ( x -- x' ) [inlined] code[ tos tos rev_,_ ]code ;

    \ Convert halfword endianness of a word
    : rev16 ( x -- x' ) [inlined] code[ tos tos rev16_,_ ]code ;

    \ Write a halfword to the dictionary without concern for endianness
    : 2c, ( h -- ) dup c, 8 rshift c, ;

    \ USB IRQ index
    5 constant usbctrl-irq

    \ USB vector
    usbctrl-irq 16 + constant usbctrl-vector

    \ USB registers base address
    $50110000 constant USB_Base

    \ USB device address and endpoint control register
    USB_Base $00 + constant USB_ADDR_ENDP

    \ Main control register
    USB_Base $40 + constant USB_MAIN_CTRL

    \ Last SOF (Start of Frame) numer received from host register
    USB_Base $48 + constant USB_SOF_RD

    \ SIE control register
    USB_Base $4C + constant USB_SIE_CTRL

    \ SIE status register
    USB_Base $50 + constant USB_SIE_STATUS

    \ Interrupt endpoint control register
    USB_Base $54 + constant USB_INT_EP_CTRL

    \ Buffer status register
    USB_Base $58 + constant USB_BUFF_STATUS

    \ Stall armed register
    USB_Base $68 + constant USB_EP_STALL_ARM

    \ Where to connect the USB controller
    USB_Base $74 + constant USB_USB_MUXING

    \ Overrides for the power signals in the event that the VBUS signals are
    \ not hooked up to GPIO
    USB_Base $78 + constant USB_USB_PWR

    \ Interupt enable
    USB_Base $90 + constant USB_INTE

    \ Interrupt status after masking and forcing
    USB_Base $98 + constant USB_INTS

    \ Enable controller
    1 bit constant USB_MAIN_CTRL_CONTROLLER_EN

    \ Set bit in BUFF_STATUS for every buffer completed on EP0
    29 bit constant USB_SIE_CTRL_EP0_INT_1BUF

    \ Enable pull up resistor
    16 bit constant USB_SIE_CTRL_PULLUP_EN

    \ Setup packet received clear bit
    17 bit constant USB_SIE_STATUS_SETUP_REC

    \ Enable interrupt endpoint bit
    : USB_INT_EP_CTRL_INT_EP_ACTIVE ( endpoint -- ) [inlined] bit ;
    
    \ Buffer status endpoint in bit
    : USB_BUFF_STATUS_EP_IN ( endpoint -- ) [inlined] 1 lshift bit ;

    \ Buffer status endpoint out bit
    : USB_BUFF_STATUS_EP_OUT ( endpoint -- ) [inlined] 1 lshift 1+ bit ;

    \ USB muxing bits
    0 bit constant USB_USB_MUXING_TO_PHY
    3 bit constant USB_USB_MUXING_SOFTCON

    \ USB power signal override bits
    2 bit constant USB_USB_PWR_VBUS_DETECT
    3 bit constant USB_USB_PWR_VBUS_DETECT_OVERRIDE_EN

    \ USB interrupt bits
    4 bit constant USB_INTS_BUFF_STATUS
    12 bit constant USB_INTS_BUS_RESET
    16 bit constant USB_INTS_SETUP_REQ
    
    \ USB DPRAM base
    $50100000 constant USB_DPRAM_Base

    \ USB setup packet address
    USB_DPRAM_Base constant USB_SETUP_PACKET

    \ USB endpoint in control
    : USB_EP_IN_CONTROL ( endpoint -- ) 3 lshift USB_DPRAM_Base + ;

    \ USB endpoint out control
    : USP_EP_OUT_CONTROL ( endpoint -- )
      3 lshift [ USB_DPRAM_Base cell+ ] literal +
    ;

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
    0 constant USB_BUF_CTRL_LEN

    \ USB buffer 0 transfer length mask
    $3FF constant USB_BUF_CTRL_LEN_MASK
    
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

      \ Endpoint tranfer data
      field: endpoint-data-buffer

      \ Endpoint transfer length
      field: endpoint-data-len

      \ Endpoint total length
      field: endpoint-data-total-len
      
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
    
    \ USB handled interrupts
    variable usb-handled-interrupts

    \ USB pending operation
    pending-op-size buffer: usb-pending-op

    \ USB pending operation priority
    0 constant usb-pending-op-priority
    
    \ Get the offset in USB dual-ported RAM
    : usb-buffer-offset ( addr -- addr' ) USB_Base - ;

    \ Set up a USB endpoint
    : usb-setup-endpoint { endpoint -- }
      endpoint endpoint-control @ if
        [ EP_CTRL_ENABLE EP_CTRL_INTERRUPT_PER_BUFFER or ]
        endpoint endpoint-buffer @ usb-buffer-offset or
      then
    ;

    \ Carry out next transfer
    : usb-tx { endpoint -- }
      endpoint endpoint-data-len @ endpoint endpoint-max-packet-size @
      min { bytes }
      bytes USB_BUF_CTRL_AVAIL or { buffer-control-val }
      endpoint endpoint-tx? if
        endpoint endpoint-data-buffer @ bytes
        endpoint endpoint-buffer @ move
        bytes endpoint endpoint-data-buffer +!
        buffer-control-val USB_BUF_CTRL_FULL or to buffer-control-val
        bytes negate endpoint endpoint-data-len +!
      then
      endpoint endpoint-next-pid @ if
        USB_BUF_CTRL_DATA1_PID
      else
        USB_BUF_CTRL_DATA0_PID
      then
      buffer-control-val or to buffer-control-val
      endpoint endpoint-data-len @ 0= if
        USB_BUF_CTRL_LAST buffer-control-val or to buffer-control-val
      then
      endpoint endpoint-next-pid @ 1 xor endpoint endpoint-next-pid !
      USB_EP_ENABLE_INTERRUPT_PER_BUFFER { ep-control-val }
      ep-control-val endpoint endpoint-control @ !
      buffer-control-val endpoint endpoint-buffer-control @ !
    ;

    \ Receive data
    : usb-rx { endpoint -- }
      endpoint endpoint-buffer-control @ @ { buffer-control-val }
      buffer-control-val USB_BUF_CTRL_LEN_MASK and { bytes }
      endpoint endpoint-tx? not if
        endpoint endpoint-buffer @
        endpoint endpoint-data-buffer @
        bytes move
        bytes endpoint endpoint-data-buffer +!
        bytes endpoint endpoint-data-total-len +!
        bytes negate endpoint endpoint-data-len +!
      then
    ;

    \ Handle a USB endpoint
    : usb-handle { endpoint -- }
      endpoint usb-rx
      endpoint usb-tx
    ;
    
    \ Start a USB transfer
    : usb-start-transfer { addr bytes endpoint -- }
      addr endpoint endpoint-data-buffer !
      bytes endpoint endpoint-data-len !
      endpoint usb-tx
    ;

    \ Send a USB acknowledge out request
    : usb-ack-out-request ( -- )
      0 0 endpoint0-in usb-start-transfer
    ;

    \ Set a USB device address
    : usb-set-device-addr ( -- )
      USB_SETUP_PACKET setup-pkt-value h@ dup usb-device-addr ! USB_ADDR_ENDP !
      true set-usb-device-addr !
      usb-ack-out-request
    ;

    \ Set a USB device configuration
    : usb-set-device-config ( -- )
      true usb-device-configd !
      
      usb-ack-out-request
    ;

    \ Transfer the device descriptors
    : usb-handle-device-descr ( -- )
      device-data device-data-size endpoint0-in usb-start-transfer
    ;
    
    \ Transfer the configuration, interface, and endpoint descriptors
    : usb-handle-config-descr ( -- )
      config-data config-data-size endpoint0-in usb-start-transfer
    ;

    \ Handle a USB setup packet
    : usb-handle-setup-pkt ( -- )
      \ Reset PID to 1
      1 setup-endpoint endpoint-next-pid !
      USB_SETUP_PACKET setup-pkt-request c@ { request }
      USB_SETUP_PACKET setup-pkt-request-type c@ case
        USB_DIR_OUT of
          USB_SETUP_PACKET setup-pkt-request c@ case
            USB_REQUEST_SET_ADDRESS of usb-set-device-addr endof
            USB_REQUEST_SET_CONFIGURATION of usb-set-device-config endof
            usb-acknowledge-out-request
          endcase
        endof
        USB_DIR_IN of
          USB_SETUP_PACKET setup-pkt-request c@ case
            USB_REQUEST_GET_DESCRIPTOR of
              USB_SETUP_PACKET setup-pkt-value h@ 8 rshift case
                USB_DT_DEVICE of usb-handle-device-descr endof
                USB_DT_CONFIG of usb-handle-config-descr endof
                USB_DT_STRING of usb-handle-string-descr endof
              endcase
            endof
          endcase
        endof
      endcase
    ;

    \ Handle bus reset
    : usb-handle-bus-reset ( -- )
      0 USB_ADDR_ENDP !
      0 usb-device-addr !
      false set-usb-device-addr !
      false usb-device-configd !
    ;
    
    \ Actually handle USB
    : handle-usb-pending-op ( -- )
      usb-handled-interrupts @ USB_INTS_BUS_RESET and if
        usb-handle-bus-reset
        USB_INTS_BUS_RESET usb-handled-interrupts bic!
      then
      usb-handled-interrupts @ USB_INTS_SETUP_REQ and if
        usb-handle-setup-pkt
        USB_INTS_SETUP_REQ usb-handled-interrupts bic!
      then
      USB_BUFF_STATUS @ 1 USB_BUFF_STATUS_EP_OUT and if
        1 USB_BUFF_STATUS_EP_OUT USB_BUFF_STATUS !
        endpoint1-out usb-handle
      then
      USB_BUFF_STATUS @ 1 USB_BUFF_STATUS_EP_IN and if
        1 USB_BUFF_STATUS_EP_IN USB_BUFF_STATUS !
        endpoint1-in usb-handle
      then
      USB_BUFF_STATUS @ 2 USB_BUFF_STATUS_EP_IN and if
        2 USB_BUFF_STATUS_EP_IN USB_BUFF_STATUS !
        endpoint2-in usb-handle
      then
    ;

    \ Handle a USB interrupt
    : handle-usb-irq ( -- )
      USB_INTS @ { ints }
      ints usb-handled-interrupts @ or usb-handled-interrupts !
      ints USB_SIE_STATUS !
      ints if
        ['] handle-usb-pending-op usb-pending-op set-pending-op
      then
    ;
    
    \ Initialize USB
    : init-usb ( -- )
      \ Clear the DPRAM just because
      USB_DPRAM_Base dpram-size 0 fill
      0 USB_ADDR_ENDP !
      0 usb-device-addr !
      false set-usb-device-addr !
      false usb-device-configd !

      usb-pending-op-priority usb-pending-op register-pending-op

      0 usbctrl-irq NVIC_IPR_IP!
      ['] handle-usb-irq usbctrl-vector vector!
      usbctrl-irq NVIC_ISER_SETENA!

      [ USB_USB_MUXING_TO_PHY USB_USB_MUXING_SOFTCON or ] literal
      USB_USB_MUXING !
      [ USB_USB_PWR_VBUS_DETECT USB_USB_PWR_VBUS_DETECT_OVERRIDE or ] literal
      USB_USB_PWR !
      USB_MAIN_CTRL_CONTROLLER_EN USB_MAIN_CTRL !
      USB_SIE_CTRL_EP0_INT_1BUF USB_SIE_CTRL !
      [ USB_INTS_BUFF_STATUS USB_INTS_BUS_RESET or USB_INTS_SETUP_REQ or ]
      literal USB_INTE !

      \ Put more here
      
      USB_SIE_CTRL @ USB_SIE_CTRL_PULLUP_EN or USB_SIE_CTRL !
    ;

  end-module> import
  
end-module
