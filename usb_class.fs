
\ Copyright (c) 2024 Serialcomms (GitHub)
\
\ todo - restore licence text

compile-to-flash

cornerstone remove-usb-class

begin-module usb-class

task import

\ import-from usb-class 

   begin-structure line-state-notification-descriptor

      cfield: request-type
      cfield: request
      hfield: value
      hfield: index
      hfield: length
      hfield: data
    
    end-structure

    begin-structure cdc-line-coding-profile

      field: baud
      cfield: parity
      cfield: stop
      cfield: data

    end-structure

    cdc-line-coding-profile aligned-buffer: cdc-line-coding

    line-state-notification-descriptor aligned-buffer: line-state-notification

    : init-cdc-line-coding ( -- )

      115200  cdc-line-coding baud !
      0       cdc-line-coding parity c! 
      0       cdc-line-coding stop c! 
      8       cdc-line-coding data c!       
    
    ;

    : init-line-nofification ( -- )
    
      $A1 line-state-notification request c!
      $20 line-state-notification request-type c!
        0 line-state-notification value h!
        0 line-state-notification index h!
        2 line-state-notification length h!
        0 line-state-notification data h!

      \ $A1 c, $20 c, $00 c, $00 c, $00 c, $00 c, $02 c, $00 c, $00 c, $00 c,            \ Line State Notification
    
    ;

  : usb-class-get-line-coding ( -- )

  ." USB Setup Class, Get Line Coding " cr

   7 cdc-line-coding 

   postpone usb-start-control-transfer-to-host

  \ : ^ usb-core :: usb-start-control-transfer-to-host ;
   
  ;

  : usb-class-set-line-coding ( -- )

   ." USB Setup Class, Set Line Coding " cr

   \ EP0-to-Pico dpram-address @ EP0-to-Host dpram-address @ 7 + dump

    1 EP0-to-Pico callback-handler !

    EP0-to-Pico 7 usb-receive-data-packet 
    
  ;

  : usb-class-set-line-control ( -- ) 
  
   ." USB Setup Class, Set Line Control " cr

    usb-setup value h@ line-state-notification value h!

    usb-ack-out-request

  ;

  : usb-setup-type-class ( -- )

   usb-setup request c@ case

          CDC_CLASS_SET_LINE_CODING of usb-class-set-line-coding endof
          CDC_CLASS_GET_LINE_CODING of usb-class-get-line-coding endof
          CDC_CLASS_SET_LINE_CONTROL of usb-class-set-line-control endof
         \ usb-ack-out-request
        
        endcase
  ;

  end-Module