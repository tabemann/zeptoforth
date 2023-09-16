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
\ SOFTWARE

begin-module pico-w-net-uart

  oo import
  cyw43-events import
  cyw43-control import
  cyw43-structs import
  net-misc import
  net-consts import
  net-config import
  net import
  endpoint-process import
  sema import
  slock import
  simple-cyw43-net import
  pico-w-cyw43-net import
  uart import
  alarm import
  
  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  <pico-w-cyw43-net> class-size buffer: my-cyw43-net
  variable my-cyw43-control
  variable my-interface

  \ UART index
  0 constant uart-index
  
  \ Port to set server at
  6668 constant server-port
  
  \ Tx lock
  slock-size buffer: tx-slock
  
  \ Server active
  variable server-active?

  \ Server tx and rx task
  variable tx-task

  \ Tx and rx delay
  100 constant server-delay
  
  \ Current tx buffer address
  variable tx-buffer-addr
  
  \ Write tx count
  variable tx-count
  
  \ Constant for number of bytes to buffer
  8192 constant tx-buffer-size
  
  \ Tx buffers
  tx-buffer-size 2 * buffer: tx-buffers
  
  \ The TCP endpoint
  variable my-endpoint

  \ The transmission notification area
  variable tx-notify-area

  \ The UART receiving notifcation area
  variable uart-rx-notify-area

  \ The transmission task
  variable tx-task

  \ The UART receiving task
  variable uart-rx-task

  \ Are we closing?
  variable closing?

  \ Closing alarm
  alarm-size buffer: my-close-alarm

  \ The received byte count
  variable rx-byte-count
  
  \ Do server transmission
  : do-tx-data ( -- )
    begin
      0 task::wait-notify-indefinite drop
      server-active? @ if
        tx-buffer-addr @ tx-count @ { buffer count }
        count 0> if
          my-endpoint @ if
            buffer count my-endpoint @ my-interface @ ['] send-tcp-endpoint try
            ?dup if
              nip nip nip nip
              [: display-red execute display-normal ;] usb::with-usb-output
            then
            \ my-cyw43-net toggle-pico-w-led
            0 tx-count !
          else
            [: cr ." NO ENDPOINT " ;] usb::with-usb-output
          then
        then
      then
      0 uart-rx-task @ task::notify
    again
  ;

  \ Do receiving data from the UART
  : do-uart-rx ( -- )
    true tx-buffers 0 systick::systick-counter { first buffer index start }
    begin
      server-active? @ my-endpoint @ 0<> and if
        index tx-buffer-size < if
          first if true else uart-index uart>? then if
            uart-index uart> buffer index + c!
            1 +to index
            systick::systick-counter to start
            false to first
            false
          else
            systick::systick-counter start - server-delay >
            tx-count @ 0= and
          then
        else
          true
        then
        if
          tx-count @ 0= if
            tx-buffer-addr @
            buffer tx-buffer-addr !
            to buffer
            index tx-count !
            0 to index
            0 tx-task @ task::notify
          else
            0 task::wait-notify-indefinite drop
          then
        then
      else
        true to first
      then
    again
  ;
  
  \ Actually handle received data
  : do-endpoint-rx { c-addr bytes -- }
    bytes rx-byte-count +!
    c-addr bytes + c-addr ?do
      i c@ uart-index >uart
    loop
  ;
  
  <endpoint-handler> begin-class <tcp-session-handler>
  end-class
  
  <tcp-session-handler> begin-implement
  
    \ Handle a endpoint packet
    :noname { endpoint self -- }
      endpoint endpoint-tcp-state@ TCP_ESTABLISHED = if
        server-active? @ not if
          0 rx-byte-count !
\          endpoint [: cr ." Got ESTABLISHED on " h.8 ;] usb::with-usb-output
        then
        true server-active? !
      else
        endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
\          endpoint [: cr ." Got CLOSE_WAIT on " h.8 ;] usb::with-usb-output
          closing? @ not if
            true closing? !
            5000 0 endpoint [: drop { endpoint }
              false server-active? !
              endpoint my-interface @ close-tcp-endpoint
              cr ." Got " rx-byte-count @ . ." bytes"
              endpoint endpoint-tcp-state@ TCP_CLOSED = if
                false closing? !
                endpoint
                [:
                  cr ." Got CLOSED after close-tcp-endpoint on " h.8
                ;] usb::with-usb-output
              then
              0 rx-byte-count !
              server-port my-interface @ allocate-tcp-listen-endpoint if
                my-endpoint !
              else
                drop
              then
            ;] my-close-alarm set-alarm-delay-default
          then
        then
      then
      endpoint endpoint-rx-data@ do-endpoint-rx
      endpoint my-interface @ endpoint-done
    ; define handle-endpoint
  
  end-implement
  
  <tcp-session-handler> class-size buffer: my-tcp-session-handler

  \ Initialize the test
  : init-test ( -- )
    false uart-special-enabled !
    tx-slock init-slock
    tx-buffers tx-buffer-size + tx-buffer-addr !
    0 tx-count !
    false server-active? !
    0 my-endpoint !
    false closing? !
    0 rx-byte-count !
    pio-addr sm-index pio-instance <pico-w-cyw43-net> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    <tcp-session-handler> my-tcp-session-handler init-object
    my-tcp-session-handler
    my-cyw43-net net-endpoint-process@ add-endpoint-handler
    0 ['] do-tx-data 512 128 1024 1 task::spawn-on-core tx-task !
    0 ['] do-uart-rx 512 128 512 1 task::spawn-on-core uart-rx-task !
    tx-notify-area 1 tx-task @ task::config-notify
    uart-rx-notify-area 1 uart-rx-task @ task::config-notify
    tx-task @ task::run
    uart-rx-task @ task::run
  ;

  \ Event message buffer
  event-message-size aligned-buffer: my-event

  \ Connect to WiFi
  : connect-wifi { D: ssid D: pass -- }
    cyw43-consts::PM_AGGRESSIVE my-cyw43-control @ cyw43-power-management!
    begin ssid pass my-cyw43-control @ join-cyw43-wpa2 nip until
    my-cyw43-control @ disable-all-cyw43-events
    my-cyw43-control @ clear-cyw43-events
    ssid pass 4 [: { D: ssid D: pass }
      begin
        my-event my-cyw43-control @ get-cyw43-event
        my-event evt-event-type @ EVENT_DISASSOC =
        my-event evt-event-type @ EVENT_DISASSOC_IND = or if
          [: cr display-red ." *** RECONNECTING TO WIFI... *** " display-normal ;] usb::with-usb-output
          begin ssid pass my-cyw43-control @ join-cyw43-wpa2 nip until
          [: cr display-red ." *** RECONNECTED TO WIFI *** " display-normal ;] usb::with-usb-output
        then
      again
    ;] 512 128 1024 task::spawn task::run
    EVENT_DISASSOC my-cyw43-control @ cyw43-control::enable-cyw43-event
    EVENT_DISASSOC_IND my-cyw43-control @ cyw43-control::enable-cyw43-event
    my-cyw43-net run-net-process
    cr ." Discovering IPv4 address..."
    my-interface @ discover-ipv4-addr
    my-interface @ intf-ipv4-addr@ cr ." IPv4 address: " ipv4.
    my-interface @ intf-ipv4-netmask@ cr ." IPv4 netmask: " ipv4.
    my-interface @ gateway-ipv4-addr@ cr ." Gateway IPv4 address: " ipv4.
    my-interface @ dns-server-ipv4-addr@ cr ." DNS server IPv4 address: " ipv4.
    my-cyw43-net toggle-pico-w-led
  ;

  \ Start the server
  : start-server ( -- )
    server-port my-interface @ allocate-tcp-listen-endpoint if
      my-endpoint !
    else
      drop
    then
  ;

end-module