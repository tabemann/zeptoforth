\ Copyright (c) 2023-2024 Travis Bemann
\ Copyright (c) 2024 Serialcomms (GitHub)
\
\ todo - restore licence text

\ https://github-wiki-see.page/m/tabemann/zeptoforth/wiki/Getting-Started-with-zeptoforth
 
compile-to-flash

marker remove-usb-constants

begin-module usb-constants

    5 constant usbctrl-irq                                          \ USB IRQ index - RP2040
    24 bit constant RESETS_USBCTRL                                  \ RP2040
    $4000C000 constant RESETS_BASE                                  \ RP2040

    usbctrl-irq 16 + constant usbctrl-vector                        \ USB vector

    $50110000 constant USB_Base                                     \ USB registers base address
    $50100000 constant USB_DPRAM_Base                               \ USB dual-ported RAM base
    $00001000 constant dpram-size                                   \ USB dual-ported RAM size  

    RESETS_BASE 2 12 lshift or constant RESETS_RESET_Set            \ Set reset
    RESETS_BASE 3 12 lshift or constant RESETS_RESET_Clr            \ Clear reset
    RESETS_BASE 8 + constant RESETS_RESET_DONE                      \ Reset done

    USB_Base $00 + constant USB_DEVICE_ADDRESS                      \ USB device address (USB device mode)
    USB_Base $40 + constant USB_MAIN_CONTROL                        \ Main control register
    USB_Base $4C + constant USB_SIE_CONTROL                         \ SIE control register
    USB_Base $50 + constant USB_SIE_STATUS                          \ SIE status register
    USB_Base $54 + constant USB_INT_EP_CTRL                         \ Interrupt endpoint control register
    USB_Base $58 + constant USB_BUFFER_STATUS                       \ Buffer status register
    USB_Base $60 + constant USB_EP_ABORT                            \ USB endpoint abort register
    USB_Base $64 + constant USB_EP_ABORT_DONE                       \ USB endpoint abort done register
    USB_Base $68 + constant USB_EP_STALL_ARM                        \ Stall armed register
    USB_Base $74 + constant USB_USB_MUXING                          \ Where to connect the USB controller
    USB_Base $78 + constant USB_USB_POWER                           \ Power signals overrides if VBUS signals not hooked up to GPIO
    USB_Base $90 + constant USB_INTE                                \ Interupt enable
    USB_Base $98 + constant USB_INTS                                \ Interrupt status after masking and forcing

     0 bit constant USB_MAIN_CTRL_CONTROLLER_EN                     \ Enable controller
    16 bit constant USB_SIE_CTRL_PULLUP_EN                          \ Enable pull up resistor  ( = Insert Device )
    17 bit constant USB_SIE_STATUS_SETUP_REC                        \ Setup packet received clear bit
    19 bit constant USB_SIE_STATUS_BUS_RESET                        \ Bus reset received clear bit
    24 bit constant USB_SIE_STATUS_CRC_ERROR                        \ CRC error bit
    25 bit constant USB_SIE_STATUS_BIT_STUFF_ERROR                  \ Bit stuff error bit
    27 bit constant USB_SIE_STATUS_RX_TIMEOUT                       \ Receive timeout bit
    29 bit constant USB_SIE_CTRL_EP0_INT_1BUF                       \ Set bit in BUFF_STATUS for every buffer completed on EP0
    31 bit constant USB_SIE_STATUS_DATA_SEQ_ERROR                   \ Data sequence error bit

     0 bit constant USB_USB_MUXING_TO_PHY                           \ USB muxing bits
     3 bit constant USB_USB_MUXING_SOFTCON

     2 bit constant USB_USB_PWR_VBUS_DETECT                         \ USB power signal override bits
     3 bit constant USB_USB_PWR_VBUS_DETECT_OVERRIDE_EN

     4 bit constant USB_INTS_BUFFER_STATUS                          \ USB interrupt bits
    12 bit constant USB_INTS_BUS_RESET
    16 bit constant USB_INTS_SETUP_REQ

    \ Endpoint control bits
    31 bit constant USB_EP_ENABLE                                   \ USB endpoint enable
    29 bit constant USB_EP_ENABLE_INTERRUPT_PER_BUFFER              \ USB enable interrupt for every transferred buffer   
    26     constant EP_CTRL_BUFFER_TYPE_LSB                         \ from Pico SDK
    17 bit constant USB_EP_INTERRUPT_ON_STALL                       \ USB endpoint interrupt on stall
    16 bit constant USB_EP_INTERRUPT_ON_NAK                         \ USB endpoint interrupt on NAK
    26     constant USB_EP_ENDPOINT_TYPE_BITS

    0 constant USB_EP_ADDRESS_BASE_OFFSET_LSB                       \ USB endpoint data buffer address bitshift

    \ Buffer control bits
    15 bit constant USB_BUF_CTRL_FULL                               \ USB buffer 0 full bit
    14 bit constant USB_BUF_CTRL_LAST                               \ USB last buffer of transfer for buffer 0  
    13 bit constant USB_BUF_CTRL_DATA1_PID                          \ USB buffer data pid 1
    12 bit constant USB_BUF_CTRL_SEL                                \ USB reset buffer select to buffer 0 
    11 bit constant USB_BUF_CTRL_STALL                              \ USB buffer stall
    10 bit constant USB_BUF_CTRL_AVAIL                              \ USB buffer is available for transfer                    
    0 constant USB_BUF_CTRL_DATA0_PID                               \ USB buffer data pid 0
    0 constant USB_BUF_CTRL_LEN                                     \ USB buffer 0 transfer length  \ ???

    $3FF constant USB_BUF_CTRL_LEN_MASK                             \ USB buffer transfer length mask
    13 bit constant USB_BUF_CTRL_PID_MASK                           \ USB buffer PID set/clear mask    

    \ Buffer Status bits

    0 bit constant USB_BUFFER_STATUS_EP0_TO_HOST                    \ USB default control endpoint IN                    
    1 bit constant USB_BUFFER_STATUS_EP0_TO_PICO                    \ USB default control endpoint OUT   
    2 bit constant USB_BUFFER_STATUS_EP1_TO_HOST                    \ Zeptoforth  console endpoint IN                        
    3 bit constant USB_BUFFER_STATUS_EP1_TO_PICO                    \ Zeptoforth  console endpoint OUT  
    4 bit constant USB_BUFFER_STATUS_EP2_TO_HOST                    \ Spare endpoint                       
    5 bit constant USB_BUFFER_STATUS_EP2_TO_PICO                    \ Spare endpoint   
    6 bit constant USB_BUFFER_STATUS_EP3_TO_HOST                    \ USB CDC Class Endpoint IN                       
    7 bit constant USB_BUFFER_STATUS_EP3_TO_PICO                    \ Spare endpoint   

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

    $03 constant ctrl-c                         \ Control-C    
    $14 constant ctrl-t                         \ Control-T

    USB_DPRAM_Base       constant USB_SETUP_PACKET                  \ USB setup packet address

    \ Fixed register addresses
    
    USB_DPRAM_Base $08  + constant EP1_ENDPOINT_CONTROL_TO_HOST     \ USB endpoint 1 endpoint control In
    USB_DPRAM_Base $0C  + constant EP1_ENDPOINT_CONTROL_TO_PICO     \ USB endpoint 1 endpoint control Out  
    USB_DPRAM_Base $10  + constant EP2_ENDPOINT_CONTROL_TO_HOST     \ USB endpoint 2 endpoint control In
    USB_DPRAM_Base $14  + constant EP2_ENDPOINT_CONTROL_TO_PICO     \ USB endpoint 2 endpoint control Out
    USB_DPRAM_Base $18  + constant EP3_ENDPOINT_CONTROL_TO_HOST     \ USB endpoint 3 endpoint control In
    USB_DPRAM_Base $1C  + constant EP3_ENDPOINT_CONTROL_TO_PICO     \ USB endpoint 3 endpoint control Out
   
    USB_DPRAM_Base $80  + constant EP0_BUFFER_CONTROL_TO_HOST       \ USB endpoint 0 buffer control In
    USB_DPRAM_Base $84  + constant EP0_BUFFER_CONTROL_TO_PICO       \ USB endpoint 0 buffer control Out
    USB_DPRAM_Base $88  + constant EP1_BUFFER_CONTROL_TO_HOST       \ USB endpoint 1 buffer control In
    USB_DPRAM_Base $8C  + constant EP1_BUFFER_CONTROL_TO_PICO       \ USB endpoint 1 buffer control Out
    USB_DPRAM_Base $90  + constant EP2_BUFFER_CONTROL_TO_HOST       \ USB endpoint 2 buffer control In
    USB_DPRAM_Base $94  + constant EP2_BUFFER_CONTROL_TO_PICO       \ USB endpoint 2 buffer control Out
    USB_DPRAM_Base $98  + constant EP3_BUFFER_CONTROL_TO_HOST       \ USB endpoint 3 buffer control In
    USB_DPRAM_Base $9C  + constant EP3_BUFFER_CONTROL_TO_PICO       \ USB endpoint 3 buffer control Out

    USB_DPRAM_Base $100 + constant EP0_DPRAM_ADDRESS_TO_HOST        \ USB endpoint 0 DPRAM Address (shared)
    USB_DPRAM_Base $100 + constant EP0_DPRAM_ADDRESS_TO_PICO        \ USB endpoint 0 DPRAM Address (shared)

    \ Configurable DPRAM addresses

    USB_DPRAM_Base $180 + constant EP1_DPRAM_ADDRESS_TO_HOST        \ USB endpoint 1 DPRAM Address In
    USB_DPRAM_Base $200 + constant EP2_DPRAM_ADDRESS_TO_HOST        \ USB endpoint 2 DPRAM Address In 
    USB_DPRAM_Base $280 + constant EP3_DPRAM_ADDRESS_TO_HOST        \ USB endpoint 3 DPRAM Address In

    USB_DPRAM_Base $900 + constant EP1_DPRAM_ADDRESS_TO_PICO        \ USB endpoint 1 DPRAM Address Out
    USB_DPRAM_Base $980 + constant EP2_DPRAM_ADDRESS_TO_PICO        \ USB endpoint 2 DPRAM Address Out
    USB_DPRAM_Base $A00 + constant EP3_DPRAM_ADDRESS_TO_PICO        \ USB endpoint 3 DPRAM Address Out

    USB_DPRAM_Base $100 + constant USB_EP0_BUFFER               \ USB endpoint 0 in/out buffer
    USB_DPRAM_Base $180 + constant USB_BUFFER_Base              \ USB data buffer base

end-module