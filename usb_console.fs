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

begin-module usb

  console import

  begin-module usb-internal

    usb-constants import
    usb-core import
    core-lock import
    task import

    \ TX interval in microseconds
    550 constant tx-interval-us

    \ TX initial delay in microseconds
    100000 constant tx-initial-delay-us

    \ TX alarm index
    3 constant tx-alarm

    \ Alarm base IRQ
    rp2040? [if]
      0 constant alarm-base-irq
    [then]
    rp2350? [if]
      4 constant alarm-base-irq
    [then]

    \ Alarm interrupt priority
    $FF constant alarm-priority

    \ Ready to send more data
    variable next-tx-initial?

    \ Saved reboot hook
    variable saved-reboot-hook

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

    \ Handle an alarm for sending data
    defer do-tx ( -- )
    :noname ( -- )

      [ rp2040? ] [if]
        tx-alarm timer::clear-alarm-int
        timer::us-counter-lsb
      [then]
      [ rp2350? ] [if]
        tx-alarm timer1::clear-alarm-int
        timer1::us-counter-lsb
      [then]
      { start-us }
      
      EP1-to-Host busy? @ not if
          
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
        then
        
        [ debug? ] [if]
          [: ." Done Sending Bytes " cr ;] debug
        [then]
        
      then        

      [ rp2040? ] [if]
        start-us tx-interval-us + ['] do-tx tx-alarm timer::set-alarm
      [then]
      [ rp2350? ] [if]
        start-us tx-interval-us + ['] do-tx tx-alarm timer1::set-alarm
      [then]

    ; is do-tx

    \ Handle completion of sending a packet to the host
    : handle-ep1-to-host ( -- )
      false EP1-to-Host busy? !
      [ rp2040? ] [if]
        timer::us-counter-lsb
        tx-interval-us + ['] do-tx tx-alarm timer::set-alarm
      [then]
      [ rp2350? ] [if]
        timer1::us-counter-lsb
        tx-interval-us + ['] do-tx tx-alarm timer1::set-alarm
      [then]
    ;

    \ Handle completion of receiving a packet from the host
    : handle-ep1-to-pico ( -- )

      [ debug? ] [if]
        [: ." Got EP1 Handler to Pico Callback! " cr ;] debug
      [then]

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
              [: attention-hook @ execute ;] try
              ?raise
            else
              dup ctrl-t = if
                drop [: attention-start-hook @ execute ;] try
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
    ;
    
    \ Get whether a byte is ready to be read
    : usb-key? ( -- key? )
      usb-device-configured? @ rx-empty? not and
    ;

    \ Get whether a byte is ready to be transmitted
    : usb-emit? ( -- emit? )
      usb-device-configured? @ tx-full? not and usb-dtr? and
    ;

    \ Flush the USB console
    : usb-flush-console ( -- )
      begin tx-empty? not EP1-to-Host busy? @ not or while pause repeat
    ;

    \ Emit a byte towards the host
    : usb-emit ( c -- )

      \ wait (block) for queue capacity to host
      begin
        usb-emit? not if
          pause false
        else
          dup [:
            [: { W^ byte }
              next-tx-initial? @ if
                false next-tx-initial? !
                [ rp2040? ] [if]
                  timer::us-counter-lsb
                  tx-initial-delay-us + ['] do-tx tx-alarm timer::set-alarm
                [then]
                [ rp2350? ] [if]
                  timer1::us-counter-lsb
                  tx-initial-delay-us + ['] do-tx tx-alarm timer1::set-alarm
                [then]
              then
              \ add byte to already-running queue
              tx-full? not if
                byte c@ write-tx true
              else
                false
              then
            ;] tx-core-lock with-core-lock
          ;] critical
        then
      until
      drop
    ;

    : usb-key ( -- c )
      begin
        usb-key? not if
          pause false
        else
          [:
            [:
              usb-key? not if
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

    \ Handle USB device configured
    : handle-usb-device-configured ( -- )
      true next-tx-initial? !
      ['] handle-ep1-to-host EP1-to-Host callback-handler !
      ['] handle-ep1-to-pico EP1-to-Pico callback-handler !

      \ start console reception from host
      EP1-to-Pico 64 usb-receive-data-packet
    ;

    \ Enable the usb console
    : usb-console
      ['] usb-key? key?-hook !
      ['] usb-key key-hook !
      ['] usb-emit? emit?-hook !
      ['] usb-emit emit-hook !
      ['] usb-emit? error-emit?-hook !
      ['] usb-emit error-emit-hook !
      ['] usb-flush-console flush-console-hook !
      ['] usb-flush-console error-flush-console-hook !
    ;
  
    \ Initialize USB console
    : init-usb-console ( -- )
      alarm-priority alarm-base-irq tx-alarm + interrupt::NVIC_IPR_IP!
      true usb-special-enabled !
      0 rx-read-index !
      0 rx-write-index !
      rx-core-lock init-core-lock
      0 tx-read-index !
      0 tx-write-index !
      tx-core-lock init-core-lock
      true next-tx-initial? !
      init-usb
      reboot-hook @ saved-reboot-hook !
      [:
        [:
          [:
            saved-reboot-hook @ execute
            0 internal::in-critical !
            pause
            100000 0 do loop
            close-usb-endpoints
            100000 0 do loop
          ;] rx-core-lock with-core-lock
        ;] tx-core-lock with-core-lock
      ;] reboot-hook !
      ['] handle-usb-device-configured usb-device-configured-callback !
      usb-insert-device
      usb-console
    ;

    initializer init-usb-console
    
  end-module> import

  \ Select the USB console
  : usb-console ( -- ) usb-console ;
  
  \ Set the current input to usb within an xt
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

compile-to-ram
