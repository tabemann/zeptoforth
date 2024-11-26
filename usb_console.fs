\ Copyright (c) 2023-2024 Travis Bemann
\ Copyright (c) 2024 Serialcomms (GitHub)
\
\ todo - restore licence text

compile-to-flash

marker remove-usb-console

begin-module usb-console

  usb-constants import
  usb-core import
  console import

    variable rx-read-index              \ RAM variable for rx buffer read-index
    variable tx-read-index              \ RAM variable for tx buffer read-index
    
    variable rx-write-index             \ RAM variable for rx buffer write-index
    variable tx-write-index             \ RAM variable for tx buffer write-index

    256 constant rx-buffer-size         \ Constant for number of bytes to buffer     
    256 constant tx-buffer-size         \ Constant for number of bytes to buffer
    
    rx-buffer-size buffer: rx-buffer    \ RX buffer to Pico
    tx-buffer-size buffer: tx-buffer    \ TX buffer to Host

    tx-buffer-size buffer: tx-straight-buffer \ TX buffer that is not circular

    : rx-full? ( -- f )                 \ Get whether the rx buffer is full
      rx-read-index @ 1- $FF and rx-write-index @ =
    ;

    : rx-empty? ( -- f )                \ Get whether the rx buffer is empty
      rx-read-index @ rx-write-index @ =
    ;

    : rx-count ( -- u )                 \ Get number of bytes available in the rx buffer
      rx-read-index @ { read-index }
      rx-write-index @ { write-index }
      read-index write-index <= if
        write-index read-index -
      else
        rx-buffer-size read-index - write-index +
      then
    ;

    : write-rx ( c -- )                  \ Write a byte to the rx buffer
      rx-full? not if
        rx-write-index @ rx-buffer + c!
        rx-write-index @ 1+ $FF and rx-write-index !
      else
        drop
      then
    ;

    : read-rx ( -- c )                     \ Read a byte from the rx buffer
      rx-empty? not if
        rx-read-index @ rx-buffer + c@
        rx-read-index @ 1+ $FF and rx-read-index !
      else
        0
      then
    ;

    : tx-full? ( -- f )                     \ Get whether the tx buffer is full
      tx-read-index @ 1- $FF and tx-write-index @ =
    ;

    : tx-empty? ( -- f )                    \ Get whether the tx buffer is empty
      tx-read-index @ tx-write-index @ =
    ;

    : tx-count ( -- u )                      \ Get number of bytes available in the tx buffer
      tx-read-index @ { read-index }
      tx-write-index @ { write-index }
      read-index write-index <= if
        write-index read-index -
      else
        tx-buffer-size read-index - write-index +
      then
    ;

    : write-tx ( c -- )                      \ Write a byte to the tx buffer
      tx-full? not if
        tx-write-index @ tx-buffer + c!
        tx-write-index @ 1+ $FF and tx-write-index !
      else
        drop
      then
    ;

    : read-tx ( -- c )                      \ Read a byte from the tx buffer
      tx-empty? not if
        tx-read-index @ tx-buffer + c@
        tx-read-index @ 1+ $FF and tx-read-index !
      else
        0
      then
    ;

  \ Handle completion of sending a packet to the host
  : handle-ep1-to-host ( -- )
    tx-write-index @ { write-index }
    tx-read-index @ { read-index }
    write-index read-index
    [: ." read-index = $" h.8 ."  write-index = $" h.8 cr ;] debug
    write-index read-index <> if 
      tx-buffer read-index + { start-addr }
      write-index read-index > if
        write-index read-index - { bytes }
        bytes 64 min { real-bytes }
        real-bytes [: ." Sending Bytes = $" h.8 cr ;] debug
        EP1-to-Host real-bytes start-addr usb-build-data-packet
        EP1-to-Host real-bytes usb-send-data-packet
        real-bytes read-index + $FF and tx-read-index !
      else
        tx-buffer-size read-index - { first-bytes }
        first-bytes 64 < if
          start-addr tx-straight-buffer first-bytes move
          write-index first-bytes + 64 min first-bytes - { second-bytes }
          tx-buffer tx-straight-buffer first-bytes + second-bytes move
          first-bytes second-bytes + { total-bytes }
          total-bytes [: ." Sending Bytes = $" h.8 cr ;] debug
          EP1-to-Host total-bytes tx-straight-buffer usb-build-data-packet
          EP1-to-Host total-bytes usb-send-data-packet
          total-bytes read-index + $FF and tx-read-index !
        else
          64 [: ." Sending Bytes = $" h.8 cr ;] debug
          EP1-to-Host 64 start-addr usb-build-data-packet
          EP1-to-Host 64 usb-send-data-packet
          64 read-index + $FF and tx-read-index !
        then
      then
      [: ." Done Sending Bytes " cr ;] debug
    then
  ;

  \ Handle completion of receiving a packet from the host
  : handle-ep1-to-pico ( -- )
    [: ." Got EP1 Handler to Pico Callback! " cr ;] debug
    EP1-to-Pico buffer-control @ @ USB_BUF_CTRL_LEN_MASK and { bytes }
    bytes [: ." Receiving Bytes = $" h.8 cr ;] debug
    EP1-to-Pico dpram-address @ dup bytes + swap ?do i c@ write-rx loop
    rx-buffer-size 1- 64 - rx-count > if
      64 [: ." Receiving More Bytes = $" h.8 cr ;] debug
      EP1-to-Pico 64 usb-receive-data-packet
      [: ." Done Receiving More Bytes " cr ;] debug
    else
      [: ." Receive Buffer Full " cr ;] debug
    then
  ;
  
  : init-usb-console ( -- )
    0 rx-read-index !
    0 rx-write-index !
    0 tx-read-index !
    0 tx-write-index !
  ;

  initializer init-usb-console
  
    : usb-key? ( -- key? )       \ Get whether a byte is ready to be read

      ( usb-device-configd? @ ) rx-empty? not ( and )

    ;

    \ Get whether a byte is ready to be transmitted
    : usb-emit? ( -- emit? )
      ( usb-device-configd? @ ) tx-full? not ( and )
    ;

    \ Flush the USB console
    : usb-flush-console ( -- )
      begin tx-empty? not while pause repeat
    ;

    : usb-emit { W^ transmit-character }               \ Emit a byte towards the host

      begin tx-full? while pause repeat     \ wait (block) for queue capacity to host
      
        EP1-to-Host busy? @ if

            transmit-character c@ write-tx             \ add byte to already-running queue

\            ep1-to-host busy? @ not if              \ EP may have finished by the (short) time it takes to get here

            \ start queue runner to host - to do

\            then
        
        else

          1 [: ." Sending Bytes = $" h.8 cr ;] debug            

            ep1-to-host 1 transmit-character usb-build-data-packet  \ skip queue as not already running

          ep1-to-host 1 usb-send-data-packet

          [: ." Done Sending Bytes " cr ;] debug
      
        then

    ;

   : usb-key ( -- c)

    rx-empty? if

        false                                       \ no data waiting - return false

    else

        read-rx { receive-character }               \ save character for now


        rx-buffer-size 1- 64 - rx-count > EP1-to-Pico busy? @ not and if
\        ep1-to-pico queue-long? @ if                \ is queue previously marked as long ( < 64 bytes remaining ) ?

\        rx-count 63 > if                            \ did read-rx make 64 bytes now free ?

\            false ep1-to-pico queue-long? !         \ cancel queue-long

          64 [: ." Receiving More Bytes = $" h.8 cr ;] debug
          EP1-to-Pico 64 usb-receive-data-packet
          [: ." Done Receiving More Bytes " cr ;] debug
        else

          [: ." Receive Buffer Full " cr ;] debug
   
        then

        receive-character

    then

   ;

   \ Enable the usb console
  : usb-console
    ['] handle-ep1-to-host EP1-to-Host callback-handler !
    ['] handle-ep1-to-pico EP1-to-Pico callback-handler !
    ['] usb-key? key?-hook !
    ['] usb-key key-hook !
    ['] usb-emit? emit?-hook !
    ['] usb-emit emit-hook !
    ['] usb-emit? error-emit?-hook !
    ['] usb-emit error-emit-hook !
    ['] usb-flush-console flush-console-hook !
    ['] usb-flush-console error-flush-console-hook !
    EP1-to-Pico busy? @ not if
      [: ." Starting Receiving Data ... " cr ;] debug
      EP1-to-Pico 64 usb-receive-data-packet    \ start console reception from host
    else
      [: ." Receiving Data is Busy ... " cr ;] debug
    then
  ;

  \ Set the curent input to usb within an xt
  : with-usb-input ( xt -- )
    ['] usb-key ['] usb-key? rot with-input
  ;

  \ Set the current output to usb within an xt
  : with-usb-output ( xt -- )
    ['] usb-emit ['] usb-emit? rot ['] usb-flush-console swap with-output
  ;

  \ Set the current error output to usb within an xt
  : with-usb-error-output ( xt -- )
    ['] usb-emit ['] usb-emit? rot ['] usb-flush-console swap with-error-output
  ;
