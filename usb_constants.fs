\ Copyright (c) 2023-2024 Travis Bemann
\ Copyright (c) 2024 Serialcomms (GitHub)
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

compile-to-flash

marker remove-usb-constants

begin-module usb-constants

  false constant debug?

  \ USB IRQ index
  rp2040? [if]
    5 constant usbctrl-irq
  [then]
  rp2350? [if]
    14 constant usbctrl-irq
  [then]

  \ USBCTRL reset bit
  rp2040? [if]
    24 bit constant RESETS_USBCTRL
  [then]
  rp2350? [if]
    28 bit constant RESETS_USBCTRL
  [then]

  \ Reset base
  rp2040? [if]
    $4000C000 constant RESETS_BASE
  [then]
  rp2350? [if]
    $40020000 constant RESETS_BASE
  [then]

  \ USB vector
  usbctrl-irq 16 + constant usbctrl-vector

  \ USB registers base address
  $50110000 constant USB_Base
  \ USB dual-ported RAM base
  $50100000 constant USB_DPRAM_Base
  \ USB dual-ported RAM size
  $00001000 constant dpram-size

  \ Set reset
  RESETS_BASE 2 12 lshift or constant RESETS_RESET_Set
  \ Clear reset
  RESETS_BASE 3 12 lshift or constant RESETS_RESET_Clr
  \ Reset done
  RESETS_BASE 8 + constant RESETS_RESET_DONE

  \ USB device address (USB device mode)
  USB_Base $00 + constant USB_DEVICE_ADDRESS
  \ Main control register
  USB_Base $40 + constant USB_MAIN_CONTROL
  \ SIE control register
  USB_Base $4C + constant USB_SIE_CONTROL
  \ SIE status register
  USB_Base $50 + constant USB_SIE_STATUS
  \ Interrupt endpoint control register
  USB_Base $54 + constant USB_INT_EP_CTRL
  \ Buffer status register
  USB_Base $58 + constant USB_BUFFER_STATUS
  \ USB endpoint abort register
  USB_Base $60 + constant USB_EP_ABORT
  \ USB endpoint abort done register
  USB_Base $64 + constant USB_EP_ABORT_DONE
  \ Stall armed register
  USB_Base $68 + constant USB_EP_STALL_ARM
  \ Where to connect the USB controller
  USB_Base $74 + constant USB_USB_MUXING
  \ Power signals overrides if VBUS signals not hooked up to GPIO
  USB_Base $78 + constant USB_USB_POWER
  \ Interupt enable
  USB_Base $90 + constant USB_INTE
  \ Interrupt status after masking and forcing
  USB_Base $98 + constant USB_INTS

  \ Enable controller
  0 bit constant USB_MAIN_CTRL_CONTROLLER_EN
  \ Enable pull up resistor  ( = Insert Device )
  16 bit constant USB_SIE_CTRL_PULLUP_EN
  \ Setup packet received clear bit
  17 bit constant USB_SIE_STATUS_SETUP_REC
  \ Bus reset received clear bit
  19 bit constant USB_SIE_STATUS_BUS_RESET
  \ CRC error bit
  24 bit constant USB_SIE_STATUS_CRC_ERROR
  \ Bit stuff error bit
  25 bit constant USB_SIE_STATUS_BIT_STUFF_ERROR
  \ Receive timeout bit
  27 bit constant USB_SIE_STATUS_RX_TIMEOUT
  \ Set bit in BUFF_STATUS for every buffer completed on EP0
  29 bit constant USB_SIE_CTRL_EP0_INT_1BUF
  \ Data sequence error bit
  31 bit constant USB_SIE_STATUS_DATA_SEQ_ERROR

  \ USB muxing bits
  0 bit constant USB_USB_MUXING_TO_PHY
  3 bit constant USB_USB_MUXING_SOFTCON

  \ USB power signal override bits
  2 bit constant USB_USB_PWR_VBUS_DETECT
  3 bit constant USB_USB_PWR_VBUS_DETECT_OVERRIDE_EN

  \ USB interrupt bits
  4 bit constant USB_INTS_BUFFER_STATUS
  12 bit constant USB_INTS_BUS_RESET
  \ Endpoint control bits
  16 bit constant USB_INTS_SETUP_REQ
  \ USB endpoint enable
  31 bit constant USB_EP_ENABLE
  \ USB enable interrupt for every transferred buffer
  29 bit constant USB_EP_ENABLE_INTERRUPT_PER_BUFFER
  \ from Pico SDK
  26     constant EP_CTRL_BUFFER_TYPE_LSB
  \ USB endpoint interrupt on stall
  17 bit constant USB_EP_INTERRUPT_ON_STALL
  \ USB endpoint interrupt on NAK
  16 bit constant USB_EP_INTERRUPT_ON_NAK
  26     constant USB_EP_ENDPOINT_TYPE_BITS

  \ USB endpoint data buffer address bitshift
  0 constant USB_EP_ADDRESS_BASE_OFFSET_LSB

  \ Buffer control bits
  \ USB buffer 0 full bit
  15 bit constant USB_BUF_CTRL_FULL
  \ USB last buffer of transfer for buffer 0
  14 bit constant USB_BUF_CTRL_LAST
  \ USB buffer data pid 1
  13 bit constant USB_BUF_CTRL_DATA1_PID
  \ USB reset buffer select to buffer 0
  12 bit constant USB_BUF_CTRL_SEL
  \ USB buffer stall
  11 bit constant USB_BUF_CTRL_STALL
  \ USB buffer is available for transfer
  10 bit constant USB_BUF_CTRL_AVAIL
  \ USB buffer data pid 0
  0 constant USB_BUF_CTRL_DATA0_PID
  \ USB buffer 0 transfer length  \ ???
  0 constant USB_BUF_CTRL_LEN

  \ USB buffer transfer length mask
  $3FF constant USB_BUF_CTRL_LEN_MASK
  \ USB buffer PID set/clear mask
  13 bit constant USB_BUF_CTRL_PID_MASK

  \ Buffer Status bits

  \ USB default control endpoint IN
  0 bit constant USB_BUFFER_STATUS_EP0_TO_HOST
  \ USB default control endpoint OUT
  1 bit constant USB_BUFFER_STATUS_EP0_TO_PICO
  \ Zeptoforth  console endpoint IN
  2 bit constant USB_BUFFER_STATUS_EP1_TO_HOST
  \ Zeptoforth  console endpoint OUT
  3 bit constant USB_BUFFER_STATUS_EP1_TO_PICO
  \ Spare endpoint
  4 bit constant USB_BUFFER_STATUS_EP2_TO_HOST
  \ Spare endpoint
  5 bit constant USB_BUFFER_STATUS_EP2_TO_PICO
  \ USB CDC Class Endpoint IN
  6 bit constant USB_BUFFER_STATUS_EP3_TO_HOST
  \ Spare endpoint
  7 bit constant USB_BUFFER_STATUS_EP3_TO_PICO

  \ Request types
  0  constant USB_REQUEST_TYPE_STANDARD
  32 constant USB_REQUEST_TYPE_CLASS
  
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

  \ Recipients
  0 constant USB_REQ_TYPE_RECIPIENT_DEVICE
  1 constant USB_REQ_TYPE_RECIPIENT_INTERFACE
  2 constant USB_REQ_TYPE_RECIPIENT_ENDPOINT

  \ Descriptor types
  1 constant USB_DT_DEVICE
  2 constant USB_DT_CONFIG
  3 constant USB_DT_STRING
  4 constant USB_DT_INTERFACE
  5 constant USB_DT_ENDPOINT

  \ CDC Class requests
  $20 constant CDC_CLASS_SET_LINE_CODING
  $21 constant CDC_CLASS_GET_LINE_CODING
  $22 constant CDC_CLASS_SET_LINE_CONTROL
  $23 constant CDC_CLASS_SET_LINE_BREAK

  \ Endpoint types
  0 constant USB_EP_TYPE_CONTROL
  1 constant USB_EP_TYPE_ISO
  2 constant USB_EP_TYPE_BULK
  3 constant USB_EP_TYPE_INTERRUPT

  \ Configuration attributes
  7 bit constant USB_CONFIG_BUS_POWERED
  6 bit constant USB_CONFIG_SELF_POWERED

  \ Control-C
  $03 constant ctrl-c
  \ Control-T
  $14 constant ctrl-t

  USB_DPRAM_Base       constant USB_SETUP_PACKET                  \ USB setup packet address

  \ Fixed register addresses
  
  \ USB endpoint 1 endpoint control In
  USB_DPRAM_Base $08  + constant EP1_ENDPOINT_CONTROL_TO_HOST
  \ USB endpoint 1 endpoint control Out
  USB_DPRAM_Base $0C  + constant EP1_ENDPOINT_CONTROL_TO_PICO
  \ USB endpoint 2 endpoint control In
  USB_DPRAM_Base $10  + constant EP2_ENDPOINT_CONTROL_TO_HOST
  \ USB endpoint 2 endpoint control Out
  USB_DPRAM_Base $14  + constant EP2_ENDPOINT_CONTROL_TO_PICO
  \ USB endpoint 3 endpoint control In
  USB_DPRAM_Base $18  + constant EP3_ENDPOINT_CONTROL_TO_HOST
  \ USB endpoint 3 endpoint control Out
  USB_DPRAM_Base $1C  + constant EP3_ENDPOINT_CONTROL_TO_PICO
  
  \ USB endpoint 0 buffer control In
  USB_DPRAM_Base $80  + constant EP0_BUFFER_CONTROL_TO_HOST
  \ USB endpoint 0 buffer control Out
  USB_DPRAM_Base $84  + constant EP0_BUFFER_CONTROL_TO_PICO
  \ USB endpoint 1 buffer control In
  USB_DPRAM_Base $88  + constant EP1_BUFFER_CONTROL_TO_HOST
  \ USB endpoint 1 buffer control Out
  USB_DPRAM_Base $8C  + constant EP1_BUFFER_CONTROL_TO_PICO
  \ USB endpoint 2 buffer control In
  USB_DPRAM_Base $90  + constant EP2_BUFFER_CONTROL_TO_HOST
  \ USB endpoint 2 buffer control Out
  USB_DPRAM_Base $94  + constant EP2_BUFFER_CONTROL_TO_PICO
  \ USB endpoint 3 buffer control In
  USB_DPRAM_Base $98  + constant EP3_BUFFER_CONTROL_TO_HOST
  \ USB endpoint 3 buffer control Out
  USB_DPRAM_Base $9C  + constant EP3_BUFFER_CONTROL_TO_PICO

  \ USB endpoint 0 DPRAM Address (shared)
  USB_DPRAM_Base $100 + constant EP0_DPRAM_ADDRESS_TO_HOST
  \ USB endpoint 0 DPRAM Address (shared)
  USB_DPRAM_Base $100 + constant EP0_DPRAM_ADDRESS_TO_PICO

  \ Configurable DPRAM addresses

  \ USB endpoint 1 DPRAM Address In
  USB_DPRAM_Base $180 + constant EP1_DPRAM_ADDRESS_TO_HOST
  \ USB endpoint 2 DPRAM Address In
  USB_DPRAM_Base $200 + constant EP2_DPRAM_ADDRESS_TO_HOST
  \ USB endpoint 3 DPRAM Address In
  USB_DPRAM_Base $280 + constant EP3_DPRAM_ADDRESS_TO_HOST

  \ USB endpoint 1 DPRAM Address Out
  USB_DPRAM_Base $900 + constant EP1_DPRAM_ADDRESS_TO_PICO
  \ USB endpoint 2 DPRAM Address Out
  USB_DPRAM_Base $980 + constant EP2_DPRAM_ADDRESS_TO_PICO
  \ USB endpoint 3 DPRAM Address Out
  USB_DPRAM_Base $A00 + constant EP3_DPRAM_ADDRESS_TO_PICO

  \ USB endpoint 0 in/out buffer
  USB_DPRAM_Base $100 + constant USB_EP0_BUFFER
  \ USB data buffer base
  USB_DPRAM_Base $180 + constant USB_BUFFER_Base

end-module