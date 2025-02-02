\ Copyright (c) 2023-2025 Travis Bemann
\ Copyright (c) 2024-2025 Serialcomms (GitHub)
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
\ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY
\ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
\ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
\ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
\ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
\ SOFTWARE.

compile-to-flash

continue-module usb

  console import
  
  begin-module usb-internal

    task import
    slock import
    systick import
    usb-core import
    usb-constants import
    usb-cdc-buffers import

    \ Console key simple lock buffer
    slock-size buffer: usb-key-lock
    
    \ Console emit simple lock buffer
    slock-size buffer: usb-emit-lock

    \ Start endpoint 1 data transfer from the Pico to the host
    : start-ep1-data-transfer-to-host ( -- )
      tx-empty? not if ep1-start-ring-transfer-to-host then
    ;

    \ Start endpoint 1 data transfer from the host to the Pico
    : start-ep1-data-transfer-to-pico ( -- )
      rx-free 63 > if EP1-to-Pico 64 usb-receive-data-packet then
    ;

    \ USB Start of Frame Interrupts, every 1 ms
    : handle-sof-from-host ( -- )
      EP1-to-Pico endpoint-busy? @ not if start-ep1-data-transfer-to-pico then
      EP1-to-Host endpoint-busy? @ not if start-ep1-data-transfer-to-host then
    ;
    
    \ Wait for the USB device to be connected
    : usb-wait-for-device-connected ( -- )
      begin usb-device-connected? @ dup not if pause-wo-reschedule then until
    ;

    \ Wait for the USB device to be configured
    : usb-wait-for-device-configured ( -- )     
      begin usb-device-configured? @ dup not if pause-wo-reschedule then until
    ;

    \ Wait for host to ack Pico modem signals line state notification  
    : usb-wait-for-line-state-notification ( -- )
      begin
        line-notification-complete? @ dup not if pause-wo-reschedule then
      until
    ;

    \ Wait for device
    : usb-wait-for-device ( -- )
      begin
        usb-device-connected? @ not if usb-wait-for-device-connected then

        \ Pico connected to active USB host - not just USB powered
        usb-device-connected? @ if
      
          \ Must wait for host to set configuration - allow 2-3 mins for cold-boot host PC  
          usb-device-configured? @ not if usb-wait-for-device-configured then

          usb-device-configured? @ if
            usb-readied? @ not if
              [:
                [:
                  ['] handle-sof-from-host sof-callback-handler !
           
                  \ Enable Start of Frame interrupts for EP1 tx/rx
                  USB_INTS_START_OF_FRAME USB_INTE bis!
                  
                  true usb-readied? multicore::test-set if              
                    usb-wait-for-line-state-notification 
                  then
                ;] usb-key-lock with-slock
              ;] usb-emit-lock with-slock
            then
          then
        then
        usb-device-connected? @ usb-device-configured? @ and
      until
    ;

    \ Byte available to read from rx ring buffer ?
    : usb-key? ( -- key? )
      rx-empty? not
    ;

    \ USB host and client connected and tx ring buffer capacity to host available ?
    : usb-emit? ( -- emit? )
      usb-device-configured? @ usb-dtr? and tx-full? not and  
    ;

    \ Enqueue a byte to be transmitted via the USB CDC console
    : usb-emit ( c -- )
      begin
        usb-wait-for-device
        [: usb-emit? dup if swap write-tx then ;] usb-emit-lock with-slock
        dup not if pause-reschedule-last then
      until
    ;

    \ Dequeue a byte that has been received from the USB CDC console
    : usb-key ( -- c)
      begin
        usb-wait-for-device
        [: usb-key? dup if read-rx swap then ;] usb-key-lock with-slock
        dup not if pause-reschedule-last then
      until
    ;

    \ Flush the USB CDC console
    : usb-flush-console ( -- )
      begin tx-empty? not if pause-wo-reschedule then until
    ;

    \ Switch to USB console
    : switch-to-usb-console ( -- )

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
      
      init-usb
      init-tx-ring
      init-rx-ring

      usb-key-lock init-slock
      usb-emit-lock init-slock

      usb-insert-device

      switch-to-usb-console
    ;

    initializer init-usb-console

  end-module> import

  \ Select the USB serial console
  : usb-console ( -- ) switch-to-usb-console ;

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

compile-to-ram
