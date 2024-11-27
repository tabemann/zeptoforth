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

marker remove-usb-console

begin-module usb-console

  usb-constants import
  usb-core import
  console import
  core-lock import

  \ Are special keys enabled for USB
  variable usb-special-enabled

  \ RAM variable for rx buffer read-index
  variable rx-read-index

  \ RAM variable for tx buffer read-index
  variable tx-read-index
  
  \ RAM variable for rx buffer write-index
  variable rx-write-index
  
  \ RAM variable for tx buffer write-index
  variable tx-write-index

  \ Constant for number of bytes to buffer
  256 constant rx-buffer-size
  
  \ Constant for number of bytes to buffer
  256 constant tx-buffer-size
  
  \ RX buffer to Pico
  rx-buffer-size buffer: rx-buffer
  
  \ TX buffer to Host
  tx-buffer-size buffer: tx-buffer

  \ TX buffer that is not circular
  tx-buffer-size buffer: tx-straight-buffer

  \ The TX core lock
  core-lock-size buffer: tx-core-lock

  \ The RX core lock
  core-lock-size buffer: rx-core-lock
  
  \ Get whether the rx buffer is full
  : rx-full? ( -- f )
    rx-read-index @ 1- $FF and rx-write-index @ =
  ;

  \ Get whether the rx buffer is empty
  : rx-empty? ( -- f )
    rx-read-index @ rx-write-index @ =
  ;

  \ Get number of bytes available in the rx buffer
  : rx-count ( -- u )
    rx-read-index @ { read-index }
    rx-write-index @ { write-index }
    read-index write-index <= if
      write-index read-index -
    else
      rx-buffer-size read-index - write-index +
    then
  ;

  \ Write a byte to the rx buffer
  : write-rx ( c -- )
    rx-full? not if
      rx-write-index @ rx-buffer + c!
      rx-write-index @ 1+ $FF and rx-write-index !
    else
      drop
    then
  ;

  \ Read a byte from the rx buffer
  : read-rx ( -- c )
    rx-empty? not if
      rx-read-index @ rx-buffer + c@
      rx-read-index @ 1+ $FF and rx-read-index !
    else
      0
    then
  ;

  \ Get whether the tx buffer is full
  : tx-full? ( -- f )
    tx-read-index @ 1- $FF and tx-write-index @ =
  ;

  \ Get whether the tx buffer is empty
  : tx-empty? ( -- f )
    tx-read-index @ tx-write-index @ =
  ;

  \ Get number of bytes available in the tx buffer
  : tx-count ( -- u )
    tx-read-index @ { read-index }
    tx-write-index @ { write-index }
    read-index write-index <= if
      write-index read-index -
    else
      tx-buffer-size read-index - write-index +
    then
  ;

  \ Write a byte to the tx buffer
  : write-tx ( c -- )
    tx-full? not if
      tx-write-index @ tx-buffer + c!
      tx-write-index @ 1+ $FF and tx-write-index !
    else
      drop
    then
  ;

  \ Read a byte from the tx buffer
  : read-tx ( -- c )
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

    [ debug? ] [if]
      write-index read-index
      [: ." read-index = $" h.8 ."  write-index = $" h.8 cr ;] debug
    [then]
    
    write-index read-index <> if 
      tx-buffer read-index + { start-addr }
      write-index read-index > if
        write-index read-index - { bytes }
        bytes 64 min { real-bytes }

        [ debug? ] [if]
          real-bytes [: ." Sending Bytes = $" h.8 cr ;] debug
        [then]
        
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

          [ debug? ] [if]
            total-bytes [: ." Sending Bytes = $" h.8 cr ;] debug
          [then]
          
          EP1-to-Host total-bytes tx-straight-buffer usb-build-data-packet
          EP1-to-Host total-bytes usb-send-data-packet
          total-bytes read-index + $FF and tx-read-index !
        else

          [ debug? ] [if]
            64 [: ." Sending Bytes = $" h.8 cr ;] debug
          [then]
          
          EP1-to-Host 64 start-addr usb-build-data-packet
          EP1-to-Host 64 usb-send-data-packet
          64 read-index + $FF and tx-read-index !
        then
      then

      [ debug? ] [if]
        [: ." Done Sending Bytes " cr ;] debug
      [then]

    else
      false EP1-to-Host busy? !
    then
  ;

  \ Handle completion of receiving a packet from the host
  : handle-ep1-to-pico ( -- )

    [ debug? ] [if]
      [: ." Got EP1 Handler to Pico Callback! " cr ;] debug
    [then]

    rx-core-lock claim-core-lock
    
    EP1-to-Pico buffer-control @ @ USB_BUF_CTRL_LEN_MASK and { bytes }

    [ debug? ] [if]
      bytes [: ." Receiving Bytes = $" h.8 cr ;] debug
    [then]
    
    EP1-to-Pico dpram-address @ dup bytes + swap ?do
      i c@
      usb-special-enabled @ if
        dup ctrl-c = if
          drop reboot
        else
          attention? @ if
            rx-core-lock release-core-lock
            [: attention-hook @ execute ;] try
            rx-core-lock claim-core-lock
            ?raise
          else
            dup ctrl-t = if
              rx-core-lock release-core-lock
              drop [: attention-start-hook @ execute ;] try
              rx-core-lock claim-core-lock
              ?raise
            else
              write-rx
            then
          then
        then
      else
        write-rx
      then
    loop
    
    rx-buffer-size 1- 64 - rx-count > if

      [ debug? ] [if]
        64 [: ." Receiving More Bytes = $" h.8 cr ;] debug
      [then]
      
      EP1-to-Pico 64 usb-receive-data-packet

      [ debug? ] [if]
        [: ." Done Receiving More Bytes " cr ;] debug
      [then]
      
    else

      false EP1-to-Pico busy? !
      
      [ debug? ] [if]
        [: ." Receive Buffer Full " cr ;] debug
      [then]
      
    then

    rx-core-lock release-core-lock
  ;
  
  : init-usb-console ( -- )
    true usb-special-enabled !
    0 rx-read-index !
    0 rx-write-index !
    rx-core-lock init-core-lock
    0 tx-read-index !
    0 tx-write-index !
    tx-core-lock init-core-lock
  ;

  initializer init-usb-console
  
  \ Get whether a byte is ready to be read
  : usb-key? ( -- key? )
    usb-device-configured? @ rx-empty? not and
  ;

  \ Get whether a byte is ready to be transmitted
  : usb-emit? ( -- emit? )
    usb-device-configured? @ tx-full? not and
  ;

  \ Flush the USB console
  : usb-flush-console ( -- )
    begin tx-empty? not while pause repeat
  ;

  \ Emit a byte towards the host
  : usb-emit ( c -- )

    \ wait (block) for queue capacity to host
    begin
      tx-full? if
        pause false
      else
        dup [:
          [: { W^ byte }
            EP1-to-Host busy? @ if
              \ add byte to already-running queue
              tx-full? not if
                byte c@ write-tx true
              else
                false
              then
            else
              
              [ debug? ] [if]
                1 [: ." Sending Bytes = $" h.8 cr ;] debug
              [then]
              
              \ skip queue as not already running
              EP1-to-Host 1 byte usb-build-data-packet
              EP1-to-Host 1 usb-send-data-packet
              
              [ debug? ] [if]
                [: ." Done Sending Bytes " cr ;] debug
              [then]

              true
            then
          ;] tx-core-lock with-core-lock
        ;] critical
      then
    until
    drop
  ;

  : usb-key ( -- c )
    begin
      rx-empty? if
        pause false
      else
        [:
          [:
            rx-empty? if
              false
            else
              
              \ save character for now
              read-rx
              
              rx-buffer-size 1- 64 - rx-count > EP1-to-Pico busy? @ not and if
                
                [ debug? ] [if]
                  64 [: ." Receiving More Bytes = $" h.8 cr ;] debug
                [then]
                
                EP1-to-Pico 64 usb-receive-data-packet
                
                [ debug? ] [if]
                  [: ." Done Receiving More Bytes " cr ;] debug
                [then]
                
              else
                
                [ debug? ] [if]
                  [: ." Receive Buffer Full " cr ;] debug
                [then]
                
              then

              true
            then
          ;] rx-core-lock with-core-lock
        ;] critical
      then
    until
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

      [ debug? ] [if]
        [: ." Starting Receiving Data ... " cr ;] debug
      [then]
      
      \ start console reception from host
      EP1-to-Pico 64 usb-receive-data-packet
      
    else

      [ debug? ] [if]
        [: ." Receiving Data is Busy ... " cr ;] debug
      [then]
      
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

end-module
