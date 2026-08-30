\ Copyright (c) 2023-2025 Travis Bemann
\ Copyright (c) 2024-2025 Serialcomms (GitHub)
\ Copyright (c) 2025 Paul Koning
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
  
  \ Are USB console special characters enabled?
  variable usb-special-enabled?
  
end-module> import

begin-module usb-core

  armv6m import
  interrupt import
  usb-constants import
  usb-cdc-buffers import

  oo import
  block-dev import
  task import 

  0 value blks


  \ Start a descriptor
  : :desc ( desc-id -- desc-start ) here 1 allot swap c, ;

  \ End a descriptor
  : ;desc ( desc-start -- ) here over - swap ccurrent! ;
  
  \ Ditto but then align things
  : ;desc4 ( desc-start -- ) ;desc cell align, ;

  \ Device connected to USB host 
  variable usb-device-connected?

  \ Device configuration set by host
  variable usb-device-configured?
  variable usb-device-confignum

  \ Line state notification to host complete
  variable line-notification-complete?

  \ Has the USB CDC console been readied
  variable usb-readied?

  \ USB Start-Of-Frame callback handler
  variable sof-callback-handler

  \ Saved reboot hook
  variable saved-reboot-hook

  \ (Host - DTR) Data Terminal Ready
  variable DTR?

  \ (Host - RTS) Ready to Send
  variable RTS?

  \ (Pico - DSR) Data Set Ready
  variable DSR?

  \ (Pico - DCD) Data Carrier Detect
  variable DCD?

  \ (Pico - RI) Ring Indicate
  variable RING?

  \ Mass Storage variables

  variable LUNS
  variable CBW-PHASE
  variable RESPONSE-CMD
  variable PREVENT-REMOVAL

  variable EP4-mailbox
  variable EP4-task
  variable EP4-to-Pico-event
  variable EP4-to-Host-event

  variable LBA-CURRENT
  variable LBA-COUNT

  \ USB Standard Device Descriptor - 18 bytes
  create device-data
    USB_DT_DEVICE :desc
    \ USB_VERSION_BCD
    $00 c, $02 c,
    \ USB_MISC_IAD_DEVICE (IAD = Interface Association Device)
    $EF c, $02 c, $01 c,
    \ USB_EP0_MAX
    $40 c,

    \ https://github.com/pidcodes/pidcodes.github.com/pull/1024
    \ USB_VENDOR_ID
    $09 c, $12 c,
    \ USB_PRODUCT_ID
    $CC c, $CC c,
  
    \ USB_PRODUCT_BCD
    $00 c, $02 c,
    \ STRING_MANUFACTURER
    $01 c,
    \ STRING_PRODUCT
    $02 c,
    \ STRING_SERIAL
    $03 c,
    \ CONFIGURATIONS
    $01 c,
  ;desc4

  \ USB Standard Configuration Descriptor
  create config-data
  \ Configuration Descriptor (75 bytes)
\    USB_DT_CONFIG :desc 2 allot $02 c, $01 c, $00 c, $80 c, $FA c, ;desc
    USB_DT_CONFIG :desc 2 allot $03 c, $01 c, $00 c, $80 c, $FA c, ;desc
    \ Interface Association Descriptor (IAD)
    11 :desc $00 c, $02 c, $02 c, $02 c, $00 c, $00 c, ;desc
    \ CDC Class Interface Descriptor (CCI)
    4 :desc $00 c, $00 c, $01 c, $02 c, $02 c, $00 c, $00 c, ;desc
    \ CDC Functional Descriptor - Header (CDC Class revision 1.20)
    $24 :desc $00 c, $20 c, $01 c, ;desc
    \ CDC Functional Descriptor - Abstract Control Management
    $24 :desc $02 c, $06 c, ;desc
    \ CDC Functional Descriptor - Union
    $24 :desc $06 c, $00 c, $01 c, ;desc
    \ CDC Functional Descriptor - Call Management
    $24 :desc $01 c, $01 c, $01 c, ;desc
    \ Endpoint Descriptor: EP3 In - (max packet size = 16)
    5 :desc $83 c, $03 c, $10 c, $00 c, $FF c, ;desc
    \ CDC Data Class Interface Descriptor: CDC Data
    4 :desc $01 c, $00 c, $02 c, $0A c, $00 c, $00 c, $00 c, ;desc
    \ Endpoint Descriptor: EP1 In  - Bulk Transfer Type
    5 :desc $81 c, $02 c, $40 c, $00 c, $00 c, ;desc
    \ Endpoint Descriptor: EP1 Out - Bulk Transfer Type
    5 :desc  $01 c, $02 c, $40 c, $00 c, $00 c, ;desc
  \ -------------------------------------------------------
  \ Interface Descriptor MSC
  4 :desc $02 c, $00 c, $02 c, $08 c, $06 c, $50 c, $00 c, ;desc
  \ Endpoint Descriptor BULK-IN
  5 :desc $84 c, $02 c, $40 c, $00 c, $00 c, ;desc
  \ Endpoint Descriptor BULK-OUT
  5 :desc $04 c, $02 c, $40 c, $00 c, $00 c, ;desc
  
  here config-data - cell align, constant config-data-size
  config-data-size config-data 2 + hcurrent!

  \ USB Language String Descriptor
  create language-id-string-data
    \ Language String for LANGUAGE_ENGLISH_US
    USB_DT_STRING :desc $09 c, $04 c, ;desc4

  : usb-string-1 ( -- addr ) c" zeptoforth" ;
  : usb-string-2 ( -- addr ) c" zeptoforth CDC Console" ;
 
  \ USB ASCII Character generated serial number buffer 
  17 buffer: usb-string-3 \ (size byte + 16 ASCII Characters)
  
  \ USB ASCII to unicode string conversion buffer
  64 buffer: unicode-buffer

  create SCSI-inquiry-response-data
\ from tinyusb/src/class/msc/msc.h
  ( uint8_t peripheral_device_type     : 5;) 0 c,
  ( uint8_t peripheral_qualifier       : 3;)

  ( uint8_t                            : 7;) 
  ( uint8_t is_removable               : 1;) 7 bit c,

  ( uint8_t version;)            2 c,

  ( uint8_t response_data_format       : 4;) 2 c,
  ( uint8_t hierarchical_support       : 1;)
  ( uint8_t normal_aca                 : 1;)
  ( uint8_t                            : 2;)

  ( uint8_t additional_length;)       36 5 - c,

  ( uint8_t protect                    : 1;) 0 c,
  ( uint8_t                            : 2;)
  ( uint8_t third_party_copy           : 1;)
  ( uint8_t target_port_group_support  : 2;)
  ( uint8_t access_control_coordinator : 1;)
  ( uint8_t scc_support                : 1;)

  ( uint8_t addr16                     : 1;) 0 c,
  ( uint8_t                            : 3;)
  ( uint8_t multi_port                 : 1;)
  ( uint8_t                            : 1; // vendor specific)
  ( uint8_t enclosure_service          : 1;)
  ( uint8_t                            : 1;)

  ( uint8_t                            : 1; // vendor specific) 0 c,
  ( uint8_t cmd_que                    : 1;)
  ( uint8_t                            : 2;)
  ( uint8_t sync                       : 1;)
  ( uint8_t wbus16                     : 1;)
  ( uint8_t                            : 2;)
  
  ( uint8_t vendor_id[8]  ; ///< 8 bytes of ASCII data identifying the vendor of the product.)
  char Z c, char e c, char p c, char t c, char o c, char f c, bl c, bl c,   \ zeptof
  ( uint8_t product_id[16]; ///< 16 bytes of ASCII data defined by the vendor.)
  char b c, char l c, char k c, bl c, char M c, char a c, char s c, char s c, bl c, char S c, char t c, char o c, char r c, char a c, char g c, char e c, \ blk Mass Storage
  ( uint8_t product_rev[4]; ///< 4 bytes of ASCII data defined by the vendor.)
  char 1 c, char . c, char 0 c, char 0 c, 
  
  create SCSI-mode-sense-response-data
  3 c, 0 c,
  0 c,     \ $80 = write protected
  0 c, 
  \ 4 bytes total ( from tinyusb/src/class/msc/msc.h )

  create SCSI-request-sense-data
  $70 c,        \ error
  $00 c,
  $02 c,        \ not ready
  0 c, 0 c, 0 c, 0 c,
  $0a c,        \ 10 additional bytes
  0 c, 0 c, 0 c, 0 c,
  $3a c, $00 c,       \ medium not present
  0 c, 
  0 c, 0 c, 0 c, 
  \ 18 bytes total

  \ Read Format Capacity Response data
  12 buffer: SCSI-format-capacity-data 

  \ Read Capacity Response data
  8 buffer: SCSI-capacity-data

  \ USB CDC/ACM Class line state notification descriptor
  begin-structure line-state-notification-descriptor
    cfield: line-request-type
    cfield: line-request
    hfield: line-value
    hfield: line-index
    hfield: line-length
    hfield: dce-signals
  end-structure  \ 10 bytes to send

  \ USB Endpoint structure
  begin-structure usb-endpoint-profile
    \ Is endpoint transmit to host (USB Direction = In)
    field: endpoint-tx?
    \ Send ACK on buffer completion (control transfers)
    field: endpoint-ack?
    \ Is endpoint currently active tx/rx ? (As set by Forth - not USB hardware)
    field: endpoint-busy?
    \ Endpoint transfer type (Control, Bulk, Interrupt)
    field: endpoint-type
    \ Endpoint number, for debug etc.
    field: endpoint-number
    \ Endpoint maximum packet size
    field: max-packet-size
    \ DPRAM address in Pico hardware
    field: dpram-address
    \ Endpoint next PID (0 for PID0 or 8192 for PID1)
    field: next-pid
    \ Endpoint buffer control register address
    field: buffer-control
    \ Endpoint control register address
    field: endpoint-control
    \ Endpoint transfer bytes (sent or received on buffer completion)
    field: transfer-bytes
    \ Endpoint processed bytes (Multipacket)
    field: processed-bytes
    \ Endpoint bytes pending (Multipacket)
    field: pending-bytes
    \ Total transfer bytes (Multipacket)
    field: total-bytes
    \ Source data address (Multipacket transmit)
    field: source-address
    \ Callback handler for CDC set line coding
    field: callback-handler
  end-structure

  \ USB Setup Command structure
  begin-structure usb-setup-command
    \ Setup descriptor type (if used)
    cfield: setup-descriptor-type
    \ Setup descriptor index (if used)
    cfield: setup-descriptor-index
    \ Setup packet request type
    cfield: setup-request-type
    \ Setup direction ($80/true = Pico to Host)
    cfield: setup-direction?
    \ Setup Recipient (device, interface, endpoint)
    cfield: setup-recipient
    \ Setup packet request
    cfield: setup-request
    \ Setup packet length
    hfield: setup-length
    \ Setup packet value
    hfield: setup-value
    \ Setup packet index
    hfield: setup-index
  end-structure

  \ CDC Serial Port Settings - 7 bytes 
  begin-structure cdc-line-coding-profile
    field:  cdc-line-baud
    cfield: cdc-line-parity
    cfield: cdc-line-stop
    cfield: cdc-line-data
  end-structure  

 \ Mass Storage CBW structure
  begin-structure CBW-size
    field:  CBW-dCBWSignature
    field:  CBW-dCBWTag
    field:  CBW-dCBWDataTransferLength
    cfield: CBW-bmCBWFlags
    cfield: CBW-bCBWLUN
    cfield: CBW-bCBWCBLength
    16 +field CBW-dCBWCB
  end-structure

  begin-structure CSW-size
    field:  CSW-dCSWSignature
    field:  CSW-wCSWTag
    field:  CSW-dCSWDataResidue
    cfield: CSW-bCSWStatus
  end-structure

  CBW-size buffer: CBW-COPY
  CSW-size buffer: CSW
  512 buffer: lba-buf

  \ Setup, CDC Class and Endpoint buffers
  usb-setup-command buffer: usb-setup
  cdc-line-coding-profile buffer: cdc-line-coding
  line-state-notification-descriptor buffer: line-state-notification

  \ Default endpoint 0 to Host
  usb-endpoint-profile buffer: EP0-to-Host
  \ Default endpoint 0 to Pico
  usb-endpoint-profile buffer: EP0-to-Pico
  \ Function endpoint 1 to Host
  usb-endpoint-profile buffer: EP1-to-Host
  \ Function endpoint 1 to Pico
  usb-endpoint-profile buffer: EP1-to-Pico
  \ Function endpoint 3 to Host
  usb-endpoint-profile buffer: EP3-to-Host
  \ Mass Storage Endpoints
  usb-endpoint-profile buffer: EP4-to-Host
  usb-endpoint-profile buffer: EP4-to-Pico


  \ Initialise all port signals off 
  : init-port-signals ( -- )
    \ Data Terminal Ready - from Host to Pico
    false DTR? !
    \ Ready to Send       - from Host to Pico
    false RTS? !
    \ Data Set Ready      - from Pico to Host
    false DSR? !
    \ Data Carrier Detect - from Pico to Host
    false DCD? !
    \ Ring Indicate       - from Pico to Host
    false RING? !
  ;

  \ Initialise serial port settings to 115200-8-N-1 default
  : init-cdc-line-coding ( -- )
    \ 115200 baud
    115200 cdc-line-coding cdc-line-baud !
    \ no parity
    0 cdc-line-coding cdc-line-parity c!
    \ 1 stop bit
    0 cdc-line-coding cdc-line-stop c!
    \ 8 data bits
    8 cdc-line-coding cdc-line-data c!
  ;

  \ USB CDC Class descriptor sent to Host - 10 bytes
  : init-line-state-notification ( -- )
    $A1 line-state-notification line-request-type c!
    $20 line-state-notification line-request c!
    0 line-state-notification line-value h!
    0 line-state-notification line-index h!
    2 line-state-notification line-length h!
    0 line-state-notification dce-signals h!
    0 EP3-to-Host next-pid !
  ;

  \ Clear down setup packet buffer to zeros
  : usb-init-setup-packet ( -- )
    usb-setup usb-setup-command 0 fill
  ;

  \ Endpoint common initialisation (code de-duplication)
  : init-ep-common { ep-max ep-tx? ep-number endpoint }
    endpoint usb-endpoint-profile 0 fill
    ep-number endpoint endpoint-number !
    ep-max endpoint max-packet-size ! 
    ep-tx? endpoint endpoint-tx? !
  ;

  \ Initialise Control Endpoint 0 (fixed dpram, no EP control register)
  : init-usb-endpoint-0 { ep-max ep-tx? ep-buffer-control endpoint -- }
    ep-max ep-tx? 0 endpoint init-ep-common
    USB_EP_TYPE_CONTROL endpoint endpoint-type !
    ep-buffer-control endpoint buffer-control !
    EP0_DPRAM_SHARED endpoint dpram-address !
    0 endpoint buffer-control @ !
    \ there is no endpoint control register for EP0 
    \ interrupt enable for EP0 comes from SIE_CTRL
  ;

  \ Calculate and set endpoint x dpram address in endpoint profile
  : set-ep-x-dpram-address { ep-number endpoint dpram-start-address }
    128 ep-number 1 - * { dpram-offset }
    USB_DPRAM_Base dpram-start-address or dpram-offset +
    endpoint dpram-address !
  ;

  \ Initialise Endpoint x to Host (set dpram and control registers calculation)
  : init-ep-x-to-host { ep-number endpoint -- }
    ep-number endpoint $0180 set-ep-x-dpram-address \ 1st half of usb h/w dpram
    USB_EP_CONTROL_TO_HOST_BASE ep-number 3 lshift + endpoint endpoint-control !
    USB_BUFFER_CONTROL_TO_HOST_BASE ep-number 3 lshift +
    endpoint buffer-control !
  ;

  \ Initialise Endpoint x to Pico (set dpram and control registers calculation)
  : init-ep-x-to-pico { ep-number endpoint -- }
    ep-number endpoint $0900 set-ep-x-dpram-address \ 2nd half of usb h/w dpram
    USB_EP_CONTROL_TO_PICO_BASE ep-number 3 lshift + endpoint endpoint-control !
    USB_BUFFER_CONTROL_TO_PICO_BASE ep-number 3 lshift +
    endpoint buffer-control !
  ;

  \ Initialise Endpoint x to Host or to Pico 
  : init-usb-endpoint-x { ep-max ep-tx? ep-type ep-number endpoint -- }
    ep-max ep-tx? ep-number endpoint init-ep-common
    ep-type EP_CTRL_BUFFER_TYPE_LSB lshift endpoint endpoint-type !
    ep-number endpoint ep-tx? if init-ep-x-to-host else init-ep-x-to-pico then  
    0 endpoint buffer-control @ !
    endpoint endpoint-type @              endpoint endpoint-control @ bis!
    endpoint dpram-address @ $FFC0 and    endpoint endpoint-control @ bis!
    USB_EP_ENABLE_INTERRUPT_PER_BUFFER    endpoint endpoint-control @ bis! 
    USB_EP_ENABLE                         endpoint endpoint-control @ bis!   
  ;

  \ Initialize USB default/control endpoints 0
  : init-usb-default-endpoints ( -- )
    64 true  USB_BUFFER_CONTROL_TO_HOST_BASE EP0-to-Host init-usb-endpoint-0
    64 false USB_BUFFER_CONTROL_TO_PICO_BASE EP0-to-Pico init-usb-endpoint-0
  ;

  \ Initialize CDC/ACM Function Endpoints
  : init-usb-function-endpoints ( -- )
    64 true  USB_EP_TYPE_BULK       1 EP1-to-Host init-usb-endpoint-x
    64 false USB_EP_TYPE_BULK       1 EP1-to-Pico init-usb-endpoint-x
    16 true  USB_EP_TYPE_INTERRUPT  3 EP3-to-Host init-usb-endpoint-x
  \ Mass Storage Endpoints
    64 true  USB_EP_TYPE_BULK       4 EP4-to-Host init-usb-endpoint-x
    64 false USB_EP_TYPE_BULK       4 EP4-to-Pico init-usb-endpoint-x
  ;

  \ Toggle data PID DATA0/DATA1 on endpoint buffer completion
  : usb-toggle-data-pid { endpoint -- }
    endpoint next-pid @ if
      USB_BUF_CTRL_DATA0_PID
    else
      USB_BUF_CTRL_DATA1_PID
    then endpoint next-pid !
  ;

  \ Set buffer available to Host - final step in sending data packet 
  : usb-set-buffer-available { buffer-available endpoint -- }
    [ rp2040? ] [if] 
    code[ b> >mark b> >mark b> >mark b> >mark b> >mark b> >mark ]code
    [then]
    buffer-available endpoint buffer-control @ bis!
  ;

  \ Mark buffer as full and set to available
  : usb-dispatch-buffer { endpoint -- }
    USB_BUF_CTRL_FULL endpoint buffer-control @ bis!
    USB_BUF_CTRL_AVAIL endpoint usb-set-buffer-available
  ;

  \ Receive zero-length-packet (ZLP) from host - control and function endpoints
  : usb-receive-zero-length-packet  { endpoint -- }
    endpoint next-pid @ endpoint buffer-control @ !
    USB_BUF_CTRL_AVAIL endpoint usb-set-buffer-available
  ;

  \ Send zero-length-packet (ZLP) to host - control and function endpoints
  : usb-send-zero-length-packet { endpoint -- }
    endpoint next-pid @ USB_BUF_CTRL_FULL or endpoint buffer-control @ !
    USB_BUF_CTRL_AVAIL endpoint usb-set-buffer-available
  ;

  \ Send stall packet for unsupported requests
  : usb-send-stall-packet { endpoint -- }
    true endpoint endpoint-busy? !
    endpoint next-pid @ endpoint buffer-control @ !
    [ rp2040? ] [if]
      code[ b> >mark b> >mark b> >mark b> >mark b> >mark b> >mark ]code
    [then]
    USB_BUF_CTRL_STALL endpoint buffer-control @ bis!
  ;

  \ Start data packet transmission - set bytes and next-pid then dispatch
  : usb-send-data-packet { endpoint bytes -- }
    true endpoint endpoint-busy? !
    0 endpoint transfer-bytes !
    endpoint next-pid @ bytes or endpoint buffer-control @ !
    endpoint usb-dispatch-buffer
  ;

  \ Start data packet reception - complete when buffer IRQ received
  : usb-receive-data-packet { endpoint bytes -- }
    true endpoint endpoint-busy? !
    0 endpoint transfer-bytes !
    endpoint next-pid @ bytes or endpoint buffer-control @ !
    USB_BUF_CTRL_AVAIL endpoint buffer-control @ bis!
  ;

  \ Acknowledge USB control-out request (zero length packet)
  : usb-ack-control-out-request ( -- )
    EP0-to-Host usb-send-zero-length-packet
  ;

  \ Acknowledge USB control-in request (zero length packet)
  : usb-ack-control-in-request ( -- )
    EP0-to-Pico usb-receive-zero-length-packet
  ;

  \ Return USB device address as previously set by host, or 0 if not set
  : usb-get-device-address ( -- )
    USB_DEVICE_ADDRESS @ $7F and
  ;

  \ Set USB hardware device address as instructed by host
  : usb-set-device-address ( -- )
    usb-ack-control-out-request
    1000. timer::delay-us
    usb-setup setup-value h@ USB_DEVICE_ADDRESS !
  ;

  \ Build data packet from supplied source starting address
  : usb-build-data-packet { endpoint bytes source-data -- }
    endpoint max-packet-size @ bytes min { packet-bytes }
    source-data endpoint dpram-address @ packet-bytes move
  ;

  \ Start data transfer to host (single or multi-packet)
  : usb-start-transfer-to-host
    { host-endpoint total-data-bytes source-data-address -- }
    total-data-bytes host-endpoint total-bytes !
    source-data-address host-endpoint source-address !
    \ do not exceed max packet size
    host-endpoint max-packet-size @ total-data-bytes min { packet-bytes }
    host-endpoint packet-bytes source-data-address usb-build-data-packet
    host-endpoint packet-bytes usb-send-data-packet
  ;

  \ Start control transfer to host (single or multi-packet)
  : usb-start-control-transfer-to-host
    { total-data-bytes source-data-address -- }
    0 EP0-to-Host processed-bytes !
    total-data-bytes EP0-to-Host total-bytes !
    total-data-bytes EP0-to-Host pending-bytes !
    usb-get-device-address if
      true EP0-to-Host endpoint-ack? !
      EP0-to-Host total-data-bytes source-data-address
      usb-start-transfer-to-host
    else
      false EP0-to-Host endpoint-ack? !
      EP0-to-Host total-data-bytes source-data-address
      usb-start-transfer-to-host
      usb-ack-control-in-request
    then
  ;

  \ End control transfer to host (single or multi-packet)
  : usb-end-control-transfer-to-host ( -- )
    usb-ack-control-in-request
    false EP0-to-Host endpoint-ack? !
    false EP0-to-Host endpoint-busy? !
  ;

  \ Send a descriptor to the host, trimming the length sent if the
  \ host asked for less than the full descriptor size.
  : usb-send-descriptor ( desc -- )
    dup c@ usb-setup setup-length h@ min swap usb-start-control-transfer-to-host
  ;

  \ Send device descriptor to host
  : usb-send-device-descriptor ( -- )
    device-data usb-send-descriptor
  ;
  
  \ Send configuration descriptor to host
  : usb-send-config-descriptor ( -- )
    usb-setup setup-length h@ config-data-size min
    config-data usb-start-control-transfer-to-host
  ;

  \ Send language string descriptor to host
  : usb-send-language-id-string-to-host ( -- )
    language-id-string-data usb-send-descriptor
  ;

  \ Send usb string descriptor to host as unicode characters
  : usb-send-string-descriptor-to-host { string-descriptor -- }
    unicode-buffer 64 0 fill \ clear unicode buffer (64 bytes)
    string-descriptor count 31 min { string-length } drop
    string-length 2 * 2 + { unicode-bytes } 
    string-descriptor 1 + { string-start }
    unicode-bytes unicode-buffer 0 + c! \ descriptor length
    USB_DT_STRING unicode-buffer 1 + c! \ descriptor type 3 = string
    string-length 0 do    
     string-start i + c@ unicode-buffer 2 + i 2 * + c!
    loop
    unicode-buffer usb-send-descriptor
  ;
  
  \ Stall incoming request we cannot process
  : usb-stall-ep0-request-to-pico ( -- )
    EP0_STALL_TO_PICO EP0_STALL_ARM bis!
    EP0-to-Host usb-send-stall-packet
  ;

  \ Stall outgoing request we cannot process
  : usb-stall-ep0-respond-to-host ( -- )
    EP0_STALL_TO_HOST EP0_STALL_ARM bis!
    EP0-to-Host usb-send-stall-packet
  ;

  \ Send requested string descriptor to host
  : usb-send-string-descriptor ( -- )
    usb-setup setup-descriptor-index c@ case
      0 of usb-send-language-id-string-to-host endof
      1 of usb-string-1 usb-send-string-descriptor-to-host endof
      2 of usb-string-2 usb-send-string-descriptor-to-host endof
      3 of usb-string-3 usb-send-string-descriptor-to-host endof
      usb-stall-ep0-respond-to-host
    endcase
  ;

  \ Inform host of Pico's line signal state (send line state notification)
  : usb-send-line-state-notification ( -- )
    true EP3-to-Host endpoint-busy? !
    false line-notification-complete? !
    0 line-state-notification dce-signals !
    DSR?  @ if BITMAP_DSR line-state-notification dce-signals hbis! then
    DCD?  @ if BITMAP_DCD line-state-notification dce-signals hbis! then
    RING? @ if BITMAP_RING line-state-notification dce-signals hbis! then
    \ 10 bytes to send
    EP3-to-Host 10 line-state-notification usb-start-transfer-to-host
  ;

  \ Tell host client that Pico is online (useful to some clients)
  : usb-set-modem-online ( -- )
    usb-device-configured? @ usb-device-connected? @ and if
      \ data carrier detected
      true DCD? !
      \ data set (modem) ready
      true DSR? !
      \ Ring Indicate off
      false RING? !
      usb-send-line-state-notification
    then
  ;

  \ Tell host client that Pico is offline (useful to some clients)
  : usb-set-modem-offline ( -- )
    usb-device-configured? @ usb-device-connected? @ and if
      \ data carrier lost
      false DCD? !
      \ data set (modem) not ready
      false DSR? !
      \ Ring Indicate off
      false RING? !
      usb-send-line-state-notification
    then
  ;

  \ Set device configuration when instructed to by host
  : usb-set-device-configuration ( -- )
    usb-ack-control-out-request
    init-cdc-line-coding
    init-usb-function-endpoints
    init-line-state-notification
    EP1-to-Pico 64 usb-receive-data-packet

  EP4-to-Pico 64 usb-receive-data-packet

    \ device not ready to use until this point reached
    true usb-device-configured? !
    1 usb-device-confignum !
  ;

  \ Send requested descriptor type to host
  : usb-send-descriptor-to-host ( -- )
    usb-setup setup-descriptor-type c@ case
      USB_DT_DEVICE of usb-send-device-descriptor endof
      USB_DT_CONFIG of usb-send-config-descriptor endof
      USB_DT_STRING of usb-send-string-descriptor endof
      \ for Linux hosts if device used on high-speed port
      USB_DT_QUALIFIER of usb-send-device-descriptor endof
      usb-stall-ep0-respond-to-host
    endcase
  ;

  \ Send current configuration number to host
  : usb-send-config-to-host ( -- )
    1 usb-device-confignum usb-start-control-transfer-to-host
  ;

  \ Route standard setup type request to Host request handler word
  : usb-setup-type-standard-respond-to-host ( -- )
    usb-setup setup-request c@ case
      USB_REQUEST_GET_DESCRIPTOR of usb-send-descriptor-to-host endof
      USB_REQUEST_GET_CONFIGURATION of usb-send-config-to-host endof
      usb-stall-ep0-respond-to-host
    endcase
  ;

  \ Route standard setup type request to Pico request handler word
  : usb-setup-type-standard-request-to-pico ( -- )
    usb-setup setup-request c@ case
      USB_REQUEST_SET_ADDRESS of usb-set-device-address endof
      USB_REQUEST_SET_CONFIGURATION of usb-set-device-configuration endof
      usb-stall-ep0-request-to-pico
    endcase
  ;

  \ USB standard setup type direction routing
  : usb-setup-type-standard ( -- )
    usb-setup setup-direction? c@ if
      usb-setup-type-standard-respond-to-host
    else
      usb-setup-type-standard-request-to-pico
    then
  ;

  \ Tell host our current speed/data/parity/stop bit settings
  \ e.g. 115200-8-N-1
  : usb-class-get-line-coding ( -- )
    \ fixed 7 bytes to send
    7 cdc-line-coding usb-start-control-transfer-to-host
  ;

  \ Start callback if host wants us to set our speed/data/parity/stop bit
  \ settings
  : usb-class-set-line-coding ( -- )
    true EP0-to-Pico callback-handler !
    \ 7 bytes expected to receive
    EP0-to-Pico 7 usb-receive-data-packet
  ;

  \ DTE signals from host terminal client (zeptocomjs, Minicom, PuTTY, tio
  \ et al)
  : usb-class-set-line-control ( -- )
    usb-ack-control-out-request
    usb-setup setup-value h@ { DTE_SIGNALS }
    100000. timer::delay-us \ allow hosts and clients time to settle
    DTE_SIGNALS BITMAP_DTR and if true DTR? ! else false DTR? ! then
    DTE_SIGNALS BITMAP_RTS and if true RTS? ! else false RTS? ! then

    \ some hosts/clients may not ack line notification until they have set DTR
    \ & RTS
    \ set online here, not in console
    DTR? @ RTS? @ and if usb-set-modem-online then
  ;

  \ Process CDC break condition received from client
  : usb-class-set-line-break ( -- )
    usb-ack-control-out-request
    \ some clients may toggle break by sending with setup value = $FFFF
    \ followed by $0000
    \ CDC break is a usable signal state here as many clients are capable of
    \ sending it.
    usb-setup setup-value h@ $FFFF = if
    \ useful action can go here
    then 
  ;

  \ Route CDC class setup type request to Host request handler word
  : usb-setup-type-class-respond-to-host ( -- )
    usb-setup setup-request c@ case
      CDC_CLASS_GET_LINE_CODING of usb-class-get-line-coding endof
      $fe of
        0 EP4-to-Pico-event !         \ reinitialize state variables
        0 EP4-to-Host-event !         \ in case of disconnect
        -1 RESPONSE-CMD !
        0 CBW-PHASE !
        1 LUNS  usb-start-control-transfer-to-host
      endof
    endcase
  ;

   \ Route CDC class setup type request to Pico request handler word
  : usb-setup-type-class-request-to-pico ( -- )
    usb-setup setup-request c@ case
      CDC_CLASS_SET_LINE_BREAK of usb-class-set-line-break endof
      CDC_CLASS_SET_LINE_CODING of usb-class-set-line-coding endof
      CDC_CLASS_SET_LINE_CONTROL of usb-class-set-line-control endof
    endcase
  ;

  \ USB class setup type direction routing
  : usb-setup-type-class ( -- )
    usb-setup setup-direction? c@ if
      usb-setup-type-class-respond-to-host
    else
      usb-setup-type-class-request-to-pico
    then
  ;

  \ Prepare setup packet once here for use by subsequent handlers
  : usb-prepare-setup-packet ( -- )
    USB_SETUP_PACKET 0 + c@ $80 and usb-setup setup-direction? c!
    USB_SETUP_PACKET 0 + c@ $1f and usb-setup setup-recipient c!
    USB_SETUP_PACKET 0 + c@ $60 and usb-setup setup-request-type c!
    USB_SETUP_PACKET 1 + c@ usb-setup setup-request c!
    USB_SETUP_PACKET 2 + c@ usb-setup setup-descriptor-index c!
    USB_SETUP_PACKET 3 + c@ usb-setup setup-descriptor-type c!
    USB_SETUP_PACKET 2 + h@ usb-setup setup-value h!
    USB_SETUP_PACKET 4 + h@ usb-setup setup-index h!
    USB_SETUP_PACKET 6 + h@ usb-setup setup-length h!
  ;

  \ USB setup packet buffer preparation
  : usb-prepare-setup-direction ( -- )
    usb-setup setup-direction? c@ if
      USB_BUF_CTRL_AVAIL  EP0-to-Host buffer-control @ bis!
      USB_BUF_CTRL_AVAIL  EP0-to-Pico buffer-control @ bic!
    else
      USB_BUF_CTRL_AVAIL  EP0-to-Host buffer-control @ bic!
      USB_BUF_CTRL_AVAIL  EP0-to-Pico buffer-control @ bis!
    then
  ;

  \ USB setup packet handler, including setup request-type routing
  : usb-handle-setup-packet ( -- )
    USB_SIE_STATUS_SETUP_REC USB_SIE_STATUS !
    usb-prepare-setup-packet
    usb-prepare-setup-direction
    USB_BUF_CTRL_DATA1_PID EP0-to-Host next-pid !
    USB_BUF_CTRL_DATA1_PID EP0-to-Pico next-pid !

\ 16 base !
\ USB_SETUP_PACKET dup c@ . 1+ c@ . cr 
\ decimal

    usb-setup setup-request-type c@  case
      USB_REQUEST_TYPE_STANDARD of usb-setup-type-standard endof
      USB_REQUEST_TYPE_CLASS of usb-setup-type-class endof
      usb-stall-ep0-respond-to-host
    endcase
  ;

  \ USB bus reset handler 
  : usb-handle-bus-reset ( -- )
    USB_SIE_STATUS_BUS_RESET USB_SIE_STATUS !
    0 USB_DEVICE_ADDRESS !
    false usb-device-configured? !
    0 usb-device-confignum !
    false usb-readied? !
  ;

  \ Update endpoint structure transfer bytes for data packets sent/received 
  : usb-update-transfer-bytes { endpoint -- }
    endpoint buffer-control @ @ USB_BUF_CTRL_LENGTH_MASK and
    endpoint transfer-bytes !
  ;

  \ Update endpoint byte counts for multipacket transfers
  : usb-update-endpoint-byte-counts { endpoint -- }
    endpoint processed-bytes @ endpoint transfer-bytes @ +
    endpoint processed-bytes !
    endpoint total-bytes @ endpoint processed-bytes @ - endpoint pending-bytes !
  ;

  \ Calculate next source address for multipacket transfers
  : usb-get-continue-source-address { endpoint -- }
    endpoint source-address @ endpoint processed-bytes @ +
  ;

  \ Calculate next packet size for multipacket transfers
  : usb-get-next-packet-size-to-host { endpoint -- }
    \ check against negative values
    endpoint pending-bytes @ endpoint max-packet-size @ min 0 max
  ;

  \ USB Start-of-Frame handler - used by zeptoforth console
  : usb-handle-start-of-frame ( -- )
    \ this could probably be designed out eventually but
    \ is useful as a kind of USB console watchdog for now
    \ Read SOF register to clear IRQ
    USB_SOF_READ @ { frame-number }
    usb-device-configured? @ if sof-callback-handler @ ?execute then
  ;

  \ EP0 to Host buffer completion handler - including Multipacket transfers
  : ep0-handler-to-host ( -- )
    USB_BUFFER_STATUS_EP0_TO_HOST USB_BUFFER_STATUS !  \ Write 1 to clear
    EP0-to-Host usb-update-transfer-bytes
    EP0-to-Host usb-toggle-data-pid
    EP0-to-Host usb-update-endpoint-byte-counts
    EP0-to-Host usb-get-next-packet-size-to-host { next-packet-size }
    next-packet-size if
      EP0-to-Host usb-get-continue-source-address { continue-source-address }
      EP0-to-Host next-packet-size continue-source-address usb-build-data-packet
      EP0-to-Host next-packet-size usb-send-data-packet
    else
      EP0-to-Host endpoint-ack? @ if usb-end-control-transfer-to-host then
    then
  ;

  \ Update our line speed/data/parity/stop bit settings
  \ e.g. 115200-8-N-1 as requested by host client
  : ep0-handler-to-pico-callback ( -- )
    EP0-to-Pico dpram-address @ cdc-line-coding 7 move
    false EP0-to-Pico callback-handler !
    usb-ack-control-out-request
  ;

  \ EP0 to Pico buffer completion handler - including callback handler
  : ep0-handler-to-pico ( -- )
    USB_BUFFER_STATUS_EP0_TO_PICO USB_BUFFER_STATUS !
    EP0-to-Pico usb-update-transfer-bytes
    EP0-to-Pico usb-toggle-data-pid
    EP0-to-Pico callback-handler @ if
      ep0-handler-to-pico-callback then
  ;

  \ De-queue bytes from ring buffer to EP1 dpram and send packet to host 
  : ep1-start-ring-transfer-to-host ( -- )
    tx-used 64 min { tx-bytes }
    tx-bytes 0 do 
      read-tx EP1-to-Host dpram-address @ i + c!
    loop
    EP1-to-Host tx-bytes usb-send-data-packet
  ;

  \ USB buffer completion IRQ handler - CDC data from console to host
  : ep1-handler-to-host ( -- )
    EP1-to-Host usb-update-transfer-bytes
    EP1-to-Host usb-toggle-data-pid
    tx-empty? if
      \ USB 2.0, page 53, section 5.8.3
      \ bulk transfer complete (required by some, but not all clients)
      EP1-to-Host transfer-bytes @ 64 = if
        EP1-to-Host 0 usb-send-data-packet
      else
        false EP1-to-Host endpoint-busy? !
      then
    else
      ep1-start-ring-transfer-to-host
    then
  ;

  \ Add EP1 received data to rx ring buffer and process special characters (if
  \ enabled)
  : ep1-handler-to-pico ( -- )
    EP1-to-Pico usb-update-transfer-bytes
    EP1-to-Pico usb-toggle-data-pid
    EP1-to-Pico transfer-bytes @ 0 ?do
      EP1-to-Pico dpram-address @ i + c@ { rx-byte }
      usb-special-enabled? @ if
        rx-byte ctrl-c = if
          reboot
        else
          attention? @ if
            rx-byte [: attention-hook @ ?execute ;] try drop
          else
            rx-byte ctrl-t = if
              [: attention-start-hook @ ?execute ;] try drop
            else
              rx-byte write-rx
            then
          then
        then
      else
        rx-byte write-rx
      then
    loop
    
    rx-free 63 > if
      EP1-to-Pico 64 usb-receive-data-packet
    else
      false EP1-to-Pico endpoint-busy? !
    then
  ;

  \ USB buffer completion IRQ handler - CDC line state notification to Host
  : ep3-handler-to-host ( -- )
    EP3-to-Host usb-toggle-data-pid
    false EP3-to-Host endpoint-busy? !
    true line-notification-complete? !
    \ nothing else to do here for line state nofification
    \ N.B. no corresponding EP3-to-Pico endpoint required
  ;

  : flush-task  
  0 { st }
  begin
    st if                           \ if PREVENT-REMOVAL not active longer than 2 seconds
      PREVENT-REMOVAL @ 0= if       \ assuming filesystem unmounted on host PC
        2000 ms
        PREVENT-REMOVAL @ 0= if
          blks flush-blocks
          false to st
        then
      then
    else
      PREVENT-REMOVAL @ if
        true to st
      then
    then
    1000 ms
  again
  ;
  
  : u@  ( adr -- x ) \ unaligned read for RP2040
    [ rp2350? ] [if] [inlined] @ [else]
    dup c@ swap dup 1+ c@ 8 lshift swap dup 2 + c@ 16 lshift swap 3 + c@ 24 lshift 
    or or or
    [then]
  ;

  : uh! ( x adr -- ) \ unaligned 16-bit write
    [ rp2350? ] [if] [inlined] h! [else]
    over $ff and over c! swap $ff00 and 8 rshift swap 1+  c!
	[then]
  ;

  : reverse-byte-order ( u_in -- u_out )
    [inlined] code[ r6 r6 rev_,_ ]code
  ;

  : SCSI-prepare-format-capacity-data
      SCSI-format-capacity-data 12 0 fill
      8 SCSI-format-capacity-data 3 + c!                  \ list_length
      blks block-count reverse-byte-order SCSI-format-capacity-data 4 + ! \ block_num    should be aligned
      2 SCSI-format-capacity-data 8 + c!                  \ 2 - formated media
      blks block-size dup $ff and 8 lshift swap $ff00 and 8 rshift or SCSI-format-capacity-data 10 + uh!
  ;

  : SCSI-response ( -- )
        RESPONSE-CMD @ 0 >= if
          CBW-COPY CBW-dCBWTag CSW CBW-dCBWTag 4 move
          0 CSW CSW-dCSWDataResidue !
          0 { r-val }
          blks 0= if 1 to r-val else 0 to r-val then
          RESPONSE-CMD @ case
            SCSI-CMD-TEST-UNIT-READY of
                r-val CSW CSW-bCSWStatus c!
            endof
            SCSI-CMD-READ-CAPACITY-10 of
                r-val CSW CSW-bCSWStatus c!
            endof
            SCSI-CMD-READ-10 of
                r-val CSW CSW-bCSWStatus c!
            endof
            SCSI-CMD-WRITE-10 of
                r-val CSW CSW-bCSWStatus c!
            endof
            SCSI-CMD-READ-FORMAT-CAPACITY of
                r-val CSW CSW-bCSWStatus c!
            endof

            0 CSW CSW-bCSWStatus c!
          endcase
          CSW EP4-to-Host dpram-address @ 13 move
          -1 RESPONSE-CMD !
          EP4-to-Host 13 usb-send-data-packet
        then  
  ;

  : read-next-lba
    LBA-COUNT @ 0> if 
      lba-buf   blks block-size    LBA-CURRENT @   blks block@   \ read block to lba-buf
      1 LBA-CURRENT +!
      -1 LBA-COUNT +!
      0 EP4-to-Host processed-bytes !
      blks block-size EP4-to-Host total-bytes !
      lba-buf EP4-to-Host source-address !
      lba-buf     EP4-to-Host dpram-address @   EP4-to-Host max-packet-size @     move 
      EP4-to-Host EP4-to-Host max-packet-size @ usb-send-data-packet
    else
      0 CBW-PHASE !
      SCSI-response
    then
  ;

  : write-next-lba { init? -- }
    0 EP4-to-Pico processed-bytes !
    blks block-size EP4-to-Pico total-bytes !
    lba-buf   EP4-to-Pico  source-address !
  
    init? 0= if
      lba-buf   blks block-size   LBA-CURRENT @  blks block!
      1 LBA-CURRENT +!
      -1 LBA-COUNT +!
    then
    
    LBA-COUNT @ 0 = if
      0 CBW-PHASE !
      SCSI-response
    then
  ;

  : SCSI-command { adr len -- }
   

    adr c@ case
      SCSI-CMD-INQUIRY of                  \ INQUIRY
          SCSI-inquiry-response-data EP4-to-Host dpram-address @ 36 move
          SCSI-CMD-INQUIRY RESPONSE-CMD !
          EP4-to-Host 36 usb-send-data-packet
      endof

      SCSI-CMD-TEST-UNIT-READY of                  \ Test unit ready
          SCSI-CMD-TEST-UNIT-READY RESPONSE-CMD !
          SCSI-response
      endof

      SCSI-CMD-READ-CAPACITY-10 of                  \ Read Capacity
        blks block-count 1 - reverse-byte-order SCSI-capacity-data !
        blks block-size  reverse-byte-order SCSI-capacity-data 4 + !
        SCSI-capacity-data EP4-to-Host dpram-address @ 8 move
        SCSI-CMD-READ-CAPACITY-10 RESPONSE-CMD !
        EP4-to-Host 8 usb-send-data-packet
      endof
      
      SCSI-CMD-READ-10 of                  \ Read(10)
        CBW-COPY CBW-dCBWCB  2 + u@ reverse-byte-order                  \ Unaligned memory read !!! 
        CBW-COPY CBW-dCBWCB  7 + dup c@ swap 1+ c@ swap 8 lshift or 
        ( lba num -- )
        SCSI-CMD-READ-10 RESPONSE-CMD !
        1 CBW-PHASE !
        LBA-COUNT !
        LBA-CURRENT !
        ['] read-next-lba EP4-to-Host callback-handler !
        read-next-lba
      endof

      SCSI-CMD-MODE-SENSE of                  \ mode sense
        SCSI-mode-sense-response-data EP4-to-Host dpram-address @ 4 move
        SCSI-CMD-MODE-SENSE RESPONSE-CMD !
        EP4-to-Host 4 usb-send-data-packet
      endof

      SCSI-CMD-START-STOP-UNIT of                  \ handle STOP START UNIT
        SCSI-CMD-START-STOP-UNIT RESPONSE-CMD !
        SCSI-response
      endof

      SCSI-CMD-PREVENT-ALLOW-REMOVAL of                  \ prevent/allow media removal
        CBW-COPY CBW-dCBWCB 4 + c@ PREVENT-REMOVAL !
        SCSI-CMD-PREVENT-ALLOW-REMOVAL RESPONSE-CMD !
        SCSI-response
      endof

      SCSI-CMD-WRITE-10 of                  \ write(10)
        CBW-COPY CBW-dCBWCB  2 + u@ reverse-byte-order 
        CBW-COPY CBW-dCBWCB  7 + dup c@ swap 1+ c@ swap 8 lshift or 
        ( lba num -- )
        SCSI-CMD-WRITE-10 RESPONSE-CMD !
        2 CBW-PHASE !
        LBA-COUNT !
        LBA-CURRENT !
        ['] write-next-lba EP4-to-Pico callback-handler !
        true write-next-lba
      endof

      SCSI-CMD-READ-FORMAT-CAPACITY of                \ read format capacity
        SCSI-prepare-format-capacity-data
        SCSI-format-capacity-data EP4-to-Host dpram-address @ 12 move
        SCSI-CMD-READ-FORMAT-CAPACITY RESPONSE-CMD !
        EP4-to-Host 12 usb-send-data-packet
      endof

      SCSI-CMD-REQUEST-SENSE of                \ request sense
        SCSI-request-sense-data EP4-to-Host dpram-address @ 18 move
        SCSI-CMD-REQUEST-SENSE RESPONSE-CMD !
        EP4-to-Host 18 usb-send-data-packet
      endof

      SCSI-CMD-SYNCHRONIZE-CACHE-10 of                \ SYNCHRONIZE CACHE
        blks flush-blocks
        SCSI-CMD-SYNCHRONIZE-CACHE-10 RESPONSE-CMD !
        SCSI-response 
      endof


      s" SCSI Cmd: " type
      16 base !
      len 0 do adr i + c@ . loop ." not implemented" cr   \ debug missing commands
      decimal
    endcase
    
  ;

  : CBW-parse { CBW -- }
    CBW CBW-COPY 31 move
    CBW CBW-dCBWSignature 4  s" USBC" equal-strings? if
      CBW CBW-dCBWCB CBW CBW-bCBWCBLength c@  SCSI-command 
    then
  ;

  : ep4-handler-to-host ( -- )
    EP4-to-Host usb-update-transfer-bytes
    EP4-to-Host usb-toggle-data-pid
    false EP4-to-Host endpoint-busy? !
    CBW-PHASE @ case
      0 of
        SCSI-response
      endof
      1 of
        EP4-to-Host usb-update-endpoint-byte-counts
        EP4-to-Host usb-get-next-packet-size-to-host 0> if
          EP4-to-Host  usb-get-continue-source-address EP4-to-Host dpram-address @ EP4-to-Host usb-get-next-packet-size-to-host move
          EP4-to-Host EP4-to-Host usb-get-next-packet-size-to-host usb-send-data-packet
        else
          EP4-to-Host callback-handler @ execute
        then
      endof
    endcase
  ;
  
  : ep4-handler-to-pico ( -- )
      EP4-to-Pico usb-update-transfer-bytes
      EP4-to-Pico usb-toggle-data-pid
      CBW-PHASE @ case
        0 of EP4-to-Pico dpram-address @ CBW-parse endof
        2 of
          EP4-to-Pico dpram-address @ EP4-to-Pico source-address @ EP4-to-Pico transfer-bytes @ move
          EP4-to-Pico usb-update-endpoint-byte-counts
          EP4-to-Pico transfer-bytes @ EP4-to-Pico   source-address +!
          EP4-to-Pico usb-get-next-packet-size-to-host 0= if
            false EP4-to-Pico callback-handler @ execute
          then
        endof
      endcase
      EP4-to-Pico 64 usb-receive-data-packet
  ;
  

  : EP4-handler
    begin
      0 wait-notify drop
      begin
        EP4-to-Host-event @ if    \ EP4-to-Host interrupt
            0 EP4-to-Host-event !
            ep4-handler-to-host
        then
        EP4-to-Pico-event @ if  \ EP4-to-Pico interrupt
            0 EP4-to-Pico-event !
            ep4-handler-to-pico
        then
      EP4-to-Host-event @ 0= EP4-to-Pico-event @ 0= and until
    again
  ;
  
  : EP4-task-init
    0 EP4-mailbox !
    0 EP4-to-Pico-event !
    0 EP4-to-Host-event !
    0 ['] EP4-handler 1024 256 512 spawn EP4-task !
    c" EP4-handler" EP4-task @ task-name!
    EP4-mailbox 1 EP4-task @ config-notify
    EP4-task @ run
  ;

  \ USB buffer completion handler distribution - Control EP0 to/from Host
  : usb-buffer-status-control-endpoints ( -- )
    USB_BUFFER_STATUS @ USB_BUFFER_STATUS_EP0_TO_HOST and if
    ep0-handler-to-host
    then
    USB_BUFFER_STATUS @ USB_BUFFER_STATUS_EP0_TO_PICO and if
    ep0-handler-to-pico
    then
  ;

  \ USB buffer completion handler distribution
  \ EP1 and EP3 CDC function endpoints
  : usb-buffer-status-function-endpoints ( -- )
    \ clear all buffer status IRQ in advance for any read-blocking Linux
    \ clients (e.g. Minicom)
    USB_BUFFER_STATUS @ dup { buffer-status } USB_BUFFER_STATUS !
    buffer-status USB_BUFFER_STATUS_EP1_TO_HOST and if ep1-handler-to-host then
    buffer-status USB_BUFFER_STATUS_EP1_TO_PICO and if ep1-handler-to-pico then
    buffer-status USB_BUFFER_STATUS_EP3_TO_HOST and if ep3-handler-to-host then
    buffer-status USB_BUFFER_STATUS_EP4_TO_HOST and if 1 EP4-to-Host-event ! 0 EP4-task @ notify then
    buffer-status USB_BUFFER_STATUS_EP4_TO_PICO and if 1 EP4-to-Pico-event ! 0 EP4-task @ notify then

  ;

  \ USB buffer completion handler routing
  : usb-handle-buffer-status ( -- )
    \ EP0-to-Host and/or EP0-to-Pico
    USB_BUFFER_STATUS @
    dup USB_BUFFER_STATUS_EP0 and if usb-buffer-status-control-endpoints then
    [ USB_BUFFER_STATUS_EP0 not ] literal and if usb-buffer-status-function-endpoints then
  ;

  \ Device connected to active USB host (SIE = Serial Interface Engine)
  : usb-handle-device-connect ( -- )
    USB_SIE_STATUS USB_SIE_STATUS_DEVICE_CONNECTED and if
      true usb-device-connected? !
    else
      false usb-device-connected? !
    then
    \ write to clear 
    USB_SIE_STATUS_DEVICE_CONNECTED USB_SIE_STATUS bis!
  ;

  \ General USB hardware interrupt handler
  : usb-irq-handler ( -- )
    USB_INTS @ { ints }
    ints USB_INTS_START_OF_FRAME and if usb-handle-start-of-frame then
    ints USB_INTS_BUFFER_STATUS and if usb-handle-buffer-status then
    ints USB_INTS_SETUP_REQ and if usb-handle-setup-packet then
    ints USB_INTS_BUS_RESET and if usb-handle-bus-reset then
    ints USB_INTS_DEV_CONNECT and if usb-handle-device-connect then
  ;

  \ Prepare registers and simulate physical insertion of device to host
  : usb-insert-device ( -- )
    USB_INTS_BUS_RESET              USB_INTE bis!
    USB_INTS_SETUP_REQ              USB_INTE bis!
    USB_INTS_DEV_CONNECT            USB_INTE bis! 
    USB_INTS_BUFFER_STATUS          USB_INTE bis!
    USB_SIE_CTRL_EP0_INT_1BUF       USB_SIE_CONTROL bis!
    USB_SIE_CTRL_PULLUP_EN          USB_SIE_CONTROL bis!
  ;

  \ Clear registers and simulate physical removal of device from host
  : usb-remove-device ( -- )
    0 USB_INTE !
    0 USB_INTS !
    0 USB_SIE_CONTROL !
    0 USB_MAIN_CONTROL !
  ;

  \ Get DTR (Data Terminal Ready) signal setting from host client
  : usb-dtr? ( -- dtr? )
    DTR? @
  ;

  \ Get modem online status (set in response to host raising DTR)
  : usb-modem-online? ( -- online? )
    DSR? @ DCD? @ and line-notification-complete? @ and
  ;
  
  \ Reset USB hardware and wait until complete
  : reset-usb-hardware ( -- )
    RESETS_USBCTRL RESETS_RESET_Set !
    RESETS_USBCTRL RESETS_RESET_Clr !
    begin RESETS_RESET_DONE @ not RESETS_USBCTRL and while repeat
  ;

  \ Simulate a counted string for USB serial number enumeration
  : generate-usb-serial ( -- )
    base @ { orig-base } 
    [: $10 base !
    $10 usb-string-3 c! \ simulate 16-byte counted string
    unique-id <# $10 0 do # loop #> \ 16 hexadecimal characters
    usb-string-3 1 + swap move
    ;] try
    orig-base base !
    ?raise
  ;
 
  \ Initialize USB hardware and variables
  : init-usb ( -- )
    init-port-signals
    reset-usb-hardware
    generate-usb-serial 
    usb-init-setup-packet
    0 sof-callback-handler !
    USB_DPRAM_Base dpram-size 0 fill
    init-usb-default-endpoints
    true usb::usb-special-enabled? !
    false usb-device-connected? !
    false usb-device-configured? !
    false line-notification-complete? !
    false usb-readied? !
    0 LUNS !
    0 CBW-PHASE !
    0 RESPONSE-CMD !
    s" USBS" CSW CSW-dCSWSignature swap move
    0 PREVENT-REMOVAL !
    c" Flush-task"
    0 ['] flush-task 128 128 512 spawn dup run task-name!
    EP4-task-init

    ['] usb-irq-handler usbctrl-vector vector!

    USB_USB_MUXING_TO_PHY                 USB_USB_MUXING bis!
    USB_USB_MUXING_SOFTCON                USB_USB_MUXING bis!
    USB_USB_PWR_VBUS_DETECT               USB_USB_POWER bis!
    USB_USB_PWR_VBUS_DETECT_OVERRIDE_EN   USB_USB_POWER bis!
    \ Clear bit to remove isolation
    [ rp2350? ] [if] USB_MAIN_CTRL_PHY_ISOLATE USB_MAIN_CONTROL bic! [then]
    USB_MAIN_CTRL_CONTROLLER_EN           USB_MAIN_CONTROL bis!
    usbctrl-irq NVIC_ISER_SETENA!
   
    \ Attempt orderly reboot
    \ Allow client time to respond to modem going offline
    reboot-hook @ saved-reboot-hook !
    [:
      saved-reboot-hook @ ?execute
      false usb-device-connected? !
      false usb-device-configured? !
      false usb-readied? !
      init-port-signals
      usb-set-modem-offline
      20000. timer::delay-us
      usb-remove-device
      20000. timer::delay-us
    ;] reboot-hook !
  ;

end-module
