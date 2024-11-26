\ Copyright (c) 2023-2024 Travis Bemann
\ Copyright (c) 2024 Serialcomms (GitHub)
\
\ todo - restore licence text

compile-to-flash

cornerstone remove-usb-console

begin-module usb-console

console import

    variable rx-read-index              \ RAM variable for rx buffer read-index
    variable tx-read-index              \ RAM variable for tx buffer read-index
    
    variable rx-write-index             \ RAM variable for rx buffer write-index
    variable tx-write-index             \ RAM variable for tx buffer write-index

    128 constant rx-buffer-size         \ Constant for number of bytes to buffer     
    128 constant tx-buffer-size         \ Constant for number of bytes to buffer
    
    rx-buffer-size buffer: rx-buffer    \ RX buffer to Pico
    tx-buffer-size buffer: tx-buffer    \ TX buffer to Host    
    
    : rx-full? ( -- f )                 \ Get whether the rx buffer is full
      rx-read-index @ 1- $7F and rx-write-index @ =
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
        rx-write-index @ 1+ $7F and rx-write-index !
      else
        drop
      then
    ;

    : read-rx ( -- c )                     \ Read a byte from the rx buffer
      rx-empty? not if
        rx-read-index @ rx-buffer + c@
        rx-read-index @ 1+ $7F and rx-read-index !
      else
        0
      then
    ;

    : tx-full? ( -- f )                     \ Get whether the tx buffer is full
      tx-read-index @ 1- $7F and tx-write-index @ =
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
        tx-write-index @ 1+ $7F and tx-write-index !
      else
        drop
      then
    ;

    : read-tx ( -- c )                      \ Read a byte from the tx buffer
      tx-empty? not if
        tx-read-index @ tx-buffer + c@
        tx-read-index @ 1+ $7F and tx-read-index !
      else
        0
      then
    ;

    : usb-key? ( -- key? )       \ Get whether a byte is ready to be read

      usb-device-configd? @ rx-empty? not and

    ;

    : ep1-start-queue-runner-to-host ( - - )
    
        tx-count if
        
            tx-count 64 min 0 do

            EP0-to-Host dpram-address @ I + read-tx !

            loop

            ep1-to-host I send-data-packet

        then
    ;

    : usb-emit { transmit-character }               \ Emit a byte towards the host

      begin tx-full? if while pause repeat then     \ wait (block) for queue capacity to host
      
        ep1-to-host busy? @ if

            transmit-character write-tx             \ add byte to already-running queue

            ep1-to-host busy? @ not if              \ EP may have finished by the (short) time it takes to get here

            ep1-start-queue-runner-to-host

            then
        
        else

            ep1-to-host 1 ' transmit-character usb-build-data-packet  \ skip queue as not already running

            ep1-to-host 1 send-data-packet
      
        then

    ;

   : usb-key ( -- c)

    rx-empty? if

        false                                       \ no data waiting - return false

    else

        read-rx { receive-character }               \ save character for now

        ep1-to-pico queue-long? @ if                \ is queue previously marked as long ( < 64 free bytes remaining ) ?

        rx-count 63 > if                            \ did read-rx make 64 bytes or more free ?

            false ep1-to-pico queue-long? !         \ cancel queue-long

            ep1-to-pico 64 usb-receive-data-packet  \ start another receive packet which we now have rx capacity for
   
        then

        receive-character

    then

   ;

end-Module