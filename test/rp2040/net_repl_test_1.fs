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
  net import
  endpoint-process import
  sema import
  lock import
  stream import
  
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
  
  \ Server active
  variable server-active?

  \ Server tx and rx task
  variable server-task

  \ Tx and rx delay
  50 constant server-delay
  
  \ Constant for number of bytes to buffer
  2048 constant rx-buffer-size
  
  \ Rx stream
  rx-buffer-size stream-size aligned-buffer: rx-stream
  
  \ Constant for number of bytes to buffer
  2048 constant tx-buffer-size
  
  \ Tx buffer
  tx-buffer-size stream-size aligned-buffer: tx-stream

  \ Outgoing tx buffer
  tx-buffer-size buffer: outgoing-tx-buffer
  
  \ Tx timeout
  500 constant tx-timeout
  
  \ Tx semaphore
  sema-size aligned-buffer: tx-sema
  
  \ Tx block semaphore
  sema-size aligned-buffer: tx-block-sema
    
  \ The TCP endpoint
  variable my-endpoint
  
  \ The LED state
  variable led-state

  \ Do server transmission and receiving
  : do-server ( -- )
    begin
      tx-timeout task::timeout !
      tx-sema ['] take try
      dup ['] task::x-timed-out = if 2drop 0 then
      task::no-timeout task::timeout !
      ?raise
      server-active? @ 0<> my-endpoint @ 0<> and if
        tx-stream stream-free ?dup if
          [: cr ." INIT STREAM FREE: " . ;] usb::with-usb-output
        then
        outgoing-tx-buffer tx-buffer-size tx-stream recv-stream-no-block { len }
        len 0> len 35 <= and if
          outgoing-tx-buffer len [:
            cr ." STREAM FREE: " tx-stream stream-free . ." DATA " dup (.) ." : " type
          ;] usb::with-usb-output
        then
        outgoing-tx-buffer len my-endpoint @ my-interface send-tcp-endpoint
        led-state @ not led-state !
        led-state @ 0 my-cyw43-control cyw43-gpio!
      then
      tx-block-sema broadcast
      tx-block-sema give
    again
  ;
  
  \ Actually handle received data
  : do-rx-data ( c-addr bytes -- )
    rx-stream send-stream-partial-no-block drop
  ;
  
  \ EMIT for telnet
  : telnet-emit { W^ c -- }
    server-active? @ if
      begin
        c 1 tx-stream send-stream-partial-no-block 1 = if
          true
        else
          tx-sema give
          tx-block-sema take
          false
        then
      until
    then
  ;

  \ EMIT? for telnet
  : telnet-emit? ( -- emit? )
    server-active? @ if
      tx-stream stream-full? not
    else
      false
    then
  ;

  \ KEY for telnet
  : telnet-key ( -- c )
    0 { W^ buffer }
    buffer 1 rx-stream recv-stream drop
    buffer c@
  ;

  \ KEY? for telnet
  : telnet-key? ( -- key? )
    rx-stream stream-empty? not
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
        [: cr ." *** CLOSING ***" ;] usb::with-usb-output
        false server-active? !
        endpoint my-interface close-tcp-endpoint
      then
      endpoint endpoint-tcp-state@ TCP_CLOSED = if
        [: cr ." *** CLOSED ***" ;] usb::with-usb-output
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
    false server-active? !
    0 my-endpoint !
    rx-buffer-size rx-stream init-stream
    tx-buffer-size tx-stream init-stream
    no-sema-limit 0 tx-sema init-sema
    no-sema-limit 0 tx-block-sema init-sema
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
    0 ['] do-server 512 128 1024 0 task::spawn-on-core server-task !
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
  
  \ Set up telnet as a console
  : telnet-console ( -- )
    ['] telnet-key key-hook !
    ['] telnet-key? key?-hook !
    ['] telnet-emit emit-hook !
    ['] telnet-emit? emit?-hook !
    ['] telnet-emit error-emit-hook !
    ['] telnet-emit? error-emit?-hook !
  ;
  
  \ Disconnect a session
  : disconnect-server ( -- )
    my-endpoint @ if
      my-endpoint @ my-interface close-tcp-endpoint
      [: cr ." *** CLOSED ***" ;] usb::with-usb-output
      false server-active? !
      server-port my-interface allocate-tcp-listen-endpoint if
        my-endpoint !
      else
        drop
      then
    then
  ;

end-module