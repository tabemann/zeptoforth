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

begin-module wifi-server-test

  oo import
  cyw43-events import
  cyw43-control import
  cyw43-structs import
  net-misc import
  frame-process import
  net-consts import
  net-config import
  net import
  endpoint-process import
  sema import
  slock import
  
  23 constant pwr-pin
  24 constant dio-pin
  25 constant cs-pin
  29 constant clk-pin
  
  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  <cyw43-control> class-size buffer: my-cyw43-control
  <interface> class-size buffer: my-interface
  <frame-process> class-size buffer: my-frame-process
  <arp-handler> class-size buffer: my-arp-handler
  <ip-handler> class-size buffer: my-ip-handler
  <endpoint-process> class-size buffer: my-endpoint-process

  \ Port to set server at
  6667 constant server-port
  
  \ My MAC address
  default-mac-addr 2constant my-mac-addr
  
  \ Server lock
  slock-size buffer: server-slock
  
  \ Server active
  variable server-active?

  \ Server tx and rx task
  variable server-task

  \ Tx and rx delay
  50 constant server-delay
  
  \ RAM variable for rx buffer read-index
  variable rx-read-index
  
  \ RAM variable for rx buffer write-index
  variable rx-write-index
  
  \ Constant for number of bytes to buffer
  2048 constant rx-buffer-size
  
  \ Rx buffer index mask
  $7FF constant rx-index-mask
  
  \ Rx buffer
  rx-buffer-size buffer: rx-buffer
  
  \ RAM variable for tx buffer read-index
  variable tx-read-index
  
  \ RAM variable for tx buffer write-index
  variable tx-write-index
  
  \ Constant for number of bytes to buffer
  2048 constant tx-buffer-size
  
  \ Tx buffer index mask
  $7FF constant tx-index-mask
  
  \ Tx buffer
  tx-buffer-size buffer: tx-buffer

  \ Send tx buffer
  tx-buffer-size buffer: actual-tx-buffer
  
  \ Tx timeout
  500 constant tx-timeout
  
  \ Tx timeout start
  variable tx-timeout-start
  
  \ Rx semaphore
  sema-size aligned-buffer: rx-sema
  
  \ Tx semaphore
  sema-size aligned-buffer: tx-sema
  
  \ Tx block semaphore
  sema-size aligned-buffer: tx-block-sema
    
  \ The TCP endpoint
  variable my-endpoint
  
  \ The LED state
  variable led-state

  \ Get whether the rx buffer is full
  : rx-full? ( -- f )
    rx-write-index @ rx-read-index @
    rx-buffer-size 1- + rx-index-mask and =
  ;

  \ Get whether the rx buffer is empty
  : rx-empty? ( -- f )
    rx-read-index @ rx-write-index @ =
  ;

  \ Write a byte to the rx buffer
  : write-rx ( c -- )
    rx-full? not if
      rx-write-index @ rx-buffer + c!
      rx-write-index @ 1+ rx-index-mask and rx-write-index !
    else
      drop
    then
  ;

  \ Read a byte from the rx buffer
  : read-rx ( -- c )
    rx-empty? not if
      rx-read-index @ rx-buffer + c@
      rx-read-index @ 1+ rx-index-mask and rx-read-index !
    else
      0
    then
  ;

  \ Get whether the tx buffer is full
  : tx-full? ( -- f )
    tx-write-index @ tx-read-index @
    tx-buffer-size 1- + tx-index-mask and =
  ;

  \ Get whether the tx buffer is empty
  : tx-empty? ( -- f )
    tx-read-index @ tx-write-index @ =
  ;

  \ Write a byte to the tx buffer
  : write-tx ( c -- )
    tx-full? not if
      tx-write-index @ tx-buffer + c!
      tx-write-index @ 1+ tx-index-mask and tx-write-index !
    else
      drop
    then
  ;

  \ Read a byte from the tx buffer
  : read-tx ( -- c )
    tx-empty? not if
      tx-read-index @ tx-buffer + c@
      tx-read-index @ 1+ tx-index-mask and tx-read-index !
    else
      0
    then
  ;

  \ Do server transmission and receiving
  : do-server ( -- )
    begin
      tx-timeout systick::systick-counter tx-timeout-start @ - - 0 max { my-timeout }
      my-timeout task::timeout !
      tx-sema ['] take try
      dup ['] task::x-timed-out = if 2drop 0 then
      task::no-timeout task::timeout !
      ?raise
      server-active? @ if
        [:
          0 { count }
          tx-read-index @ tx-write-index @ < if
            tx-write-index @ tx-read-index @ - to count
            tx-buffer tx-read-index @ + actual-tx-buffer count move
          else
            tx-read-index @ tx-write-index @ > if
              tx-buffer-size tx-read-index @ - { first-count }
              tx-buffer tx-read-index @ + actual-tx-buffer first-count move
              tx-buffer actual-tx-buffer first-count + tx-write-index @ move
              first-count tx-write-index @ + to count
            then
          then
          0 tx-read-index !
          0 tx-write-index !
          count
        ;] server-slock with-slock { count }
        count 0> if
          my-endpoint @ if
            actual-tx-buffer count my-endpoint @ my-interface ['] send-tcp-endpoint try
            ?dup if nip nip nip nip [: display-red execute display-normal ;] usb::with-usb-output then
          else
            [: cr ." NO ENDPOINT " ;] usb::with-usb-output
          then
          led-state @ not led-state !
          led-state @ 0 my-cyw43-control cyw43-gpio!
        then
      then
      systick::systick-counter tx-timeout-start !
      tx-block-sema broadcast
      tx-block-sema give
    again
  ;
  
  \ Actually handle received data
  : do-rx-data ( c-addr bytes -- )
    dup 0> if
      [: { c-addr bytes }
        c-addr bytes + c-addr ?do
          rx-full? not if
            i c@ write-rx
          else
            leave
          then
        loop
      ;] server-slock with-slock
      rx-sema broadcast
      rx-sema give
    then
  ;
  
  \ EMIT for telnet
  : telnet-emit ( c -- )
    server-active? @ if
      begin
        [:
          tx-full? not if
            write-tx
            true
          else
            tx-sema give
            false
          then
        ;] server-slock with-slock
        dup not if
          task::timeout @ { old-timeout }
          server-delay 10 * task::timeout !
          tx-block-sema ['] take try dup ['] task::x-timed-out = if
            2drop 0
          then
          old-timeout task::timeout !
          ?raise
        then
      until
    else
      drop
    then
  ;

  \ EMIT? for telnet
  : telnet-emit? ( -- emit? )
    server-active? @ if
      [: tx-full? not ;] server-slock with-slock
    else
      false
    then
  ;

  \ KEY for telnet
  : telnet-key ( -- c )
    begin
      server-active? @ if
        [:
          rx-empty? not if
            read-rx
            true
          else
            false
          then
        ;] server-slock with-slock
        dup not if
          task::timeout @ { old-timeout }
          server-delay 10 * task::timeout !
          rx-sema ['] take try dup ['] task::x-timed-out = if
            2drop 0
          then
          old-timeout task::timeout !
          ?raise
        then
      else
        false
      then
    until
  ;

  \ KEY? for telnet
  : telnet-key? ( -- key? )
    server-active? @ if
      [: rx-empty? not ;] server-slock with-slock
    else
      false
    then
  ;
  
  \ Type data over telnet
  : telnet-type { c-addr bytes -- }
    c-addr bytes + c-addr ?do i c@ telnet-emit loop
  ;
  
  <endpoint-handler> begin-class <tcp-session-handler>
  end-class
  
  <tcp-session-handler> begin-implement
  
    \ Handle a endpoint packet
    :noname { endpoint self -- }
      endpoint endpoint-tcp-state@ TCP_ESTABLISHED = if
        true server-active? !
        endpoint endpoint-rx-data@ do-rx-data
      then
      endpoint my-interface endpoint-done
      endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
        false server-active? !
        endpoint my-interface close-tcp-endpoint
        server-port my-interface allocate-tcp-listen-endpoint if
          my-endpoint !
        else
          drop
        then
      then
    ; define handle-endpoint
  
  end-implement
  
  <tcp-session-handler> class-size buffer: my-tcp-session-handler

  \ Initialize the test
  : init-test ( -- )
    server-slock init-slock
    0 tx-read-index !
    0 tx-write-index !
    0 rx-read-index !
    0 rx-write-index !
    systick::systick-counter tx-timeout-start !
    false server-active? !
    0 my-endpoint !
    1 0 rx-sema init-sema
    1 0 tx-sema init-sema
    1 0 tx-block-sema init-sema
    my-mac-addr cyw43-clm::data cyw43-clm::size cyw43-fw::data cyw43-fw::size
    pwr-pin clk-pin dio-pin cs-pin pio-addr sm-index pio-instance
    <cyw43-control> my-cyw43-control init-object
    my-cyw43-control init-cyw43
    cyw43-consts::PM_NONE my-cyw43-control cyw43-power-management!
    false led-state !
    led-state @ 0 my-cyw43-control cyw43-gpio!
    my-cyw43-control cyw43-frame-interface@ <interface> my-interface init-object
    my-cyw43-control cyw43-frame-interface@ <frame-process> my-frame-process init-object
    my-interface <arp-handler> my-arp-handler init-object
    my-interface <ip-handler> my-ip-handler init-object
    my-arp-handler my-frame-process add-frame-handler
    my-ip-handler my-frame-process add-frame-handler
    my-interface <endpoint-process> my-endpoint-process init-object
    <tcp-session-handler> my-tcp-session-handler init-object
    my-tcp-session-handler my-endpoint-process add-endpoint-handler
    0 ['] do-server 512 128 1024 1 task::spawn-on-core server-task !
    server-task @ task::run
  ;

  \ Event message buffer
  event-message-size aligned-buffer: my-event

  \ Connect to WiFi
  : connect-wifi { D: ssid D: pass -- }
    begin ssid pass my-cyw43-control join-cyw43-wpa2 nip until
    my-cyw43-control disable-all-cyw43-events
    my-cyw43-control clear-cyw43-events
    ssid pass 4 [: { D: ssid D: pass }
      begin
        my-event my-cyw43-control get-cyw43-event
        my-event evt-event-type @ EVENT_DISASSOC =
        my-event evt-event-type @ EVENT_DISASSOC_IND = or if
          [: cr display-red ." *** RECONNECTING TO WIFI... *** " display-normal ;] usb::with-usb-output
          begin ssid pass my-cyw43-control join-cyw43-wpa2 nip until
          [: cr display-red ." *** RECONNECTED TO WIFI *** " display-normal ;] usb::with-usb-output
        then
      again
    ;] 512 128 1024 task::spawn task::run
    EVENT_DISASSOC my-cyw43-control cyw43-control::enable-cyw43-event
    EVENT_DISASSOC_IND my-cyw43-control cyw43-control::enable-cyw43-event
    my-endpoint-process run-endpoint-process
    my-frame-process run-frame-process
    cr ." Discovering IPv4 address..."
    my-interface discover-ipv4-addr
    my-interface intf-ipv4-addr@ cr ." IPv4 address: " ipv4.
    my-interface intf-ipv4-netmask@ cr ." IPv4 netmask: " ipv4.
    my-interface gateway-ipv4-addr@ cr ." Gateway IPv4 address: " ipv4.
    my-interface dns-server-ipv4-addr@ cr ." DNS server IPv4 address: " ipv4.
  ;

  \ Start the server
  : start-server ( -- )
    server-port my-interface allocate-tcp-listen-endpoint if
      my-endpoint !
    else
      drop
    then
  ;
  
  \ Flush the telnet console
  : telnet-flush-console ( -- )
    server-active? @ if
      begin
        [:
          tx-empty? if
            true
          else
            tx-sema give
            false
          then
        ;] server-slock with-slock
        dup not if server-delay ms then
      until
    else
      drop
    then
  ;
  
  \ Set up telnet as a console
  : telnet-console ( -- )
    ['] telnet-key key-hook !
    ['] telnet-key? key?-hook !
    ['] telnet-emit emit-hook !
    ['] telnet-emit? emit?-hook !
    ['] telnet-emit error-emit-hook !
    ['] telnet-emit? error-emit?-hook !
  ;
  
  \ Set the curent input to telnet within an xt
  : with-telnet-input ( xt -- )
    ['] telnet-key
    ['] telnet-key?
    rot console::with-input
  ;
  
  \ Set the current output to telnet within an xt
  : with-telnet-output ( xt -- )
    ['] telnet-emit
    ['] telnet-emit?
    rot
    ['] telnet-flush-console
    swap console::with-output
  ;

  \ Set the current error output to telnet within an xt
  : with-telnet-error-output ( xt -- )
    ['] telnet-emit
    ['] telnet-emit?
    rot
    ['] telnet-flush-console
    swap console::with-error-output
  ;

end-module