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

\ Note that prior to running this code, one must have uploaded the CYW43439
\ firmware, CYW43439 driver, and zeptoIP to one's Raspberry Pi Pico W (this
\ does not work on a Raspberry Pi Pico) as follows:
\ 
\ Execute from the base directory of zeptoforth:
\ 
\ utils/load_cyw43_fw.sh <tty device> <43439A0.bin path> <43439A0_clm.bin path>
\ 
\ 43439A0.bin and 43439A0_clm.bin can be gotten from:
\ https://github.com/tabemann/cyw43-firmware/tree/master/cyw43439-firmware
\ 
\ Then execute from the base directory of zeptoforth:
\ 
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp2040/pico_w_net_all.fs
\ 
\ Afterwards, if you had not already installed the CYW43439 firmware, driver,
\ and zeptoIP, make sure to reboot zeptoforth, either by executing:
\
\ reboot
\
\ or by entering control-C at a terminal or Reboot with zeptocom.js.
\ 
\ Use instructions
\
\ 1. Load this code into RAM on a zeptoforth install where zeptoIP has already
\    been installed using a terminal which supports zeptoforth, e.g. zeptocom.js
\    or e4thcom in noforth mode.
\ 2. Execute: s" <WiFi SSID>" s" <WiFi password>" pico-w-net-repl::start-server
\ 3: Execute: pico-w-net-repl::telnet-console
\
\ At this point the console will stop responding. Instead, the console will be
\ accessible via a TCP connection to port 6668 on the IPv4 address acquired via
\ DHCP that is reported on the console.
\
\ The recommended means of communicating with this REPL is:
\
\ stty -echo -icanon && nc <the reported IPv4 address> 6668
\
\ Note that this will set your terminal into non-echoing, non-canonical mode;
\ to return to echoing, canonical mode, once you exit out of netcat via
\ control-C, execute 'reset'; you will have to do this blindly.

begin-module pico-w-net-repl

  oo import
  cyw43-events import
  cyw43-control import
  cyw43-structs import
  net-misc import
  net-consts import
  net-config import
  net import
  endpoint-process import
  stream import
  sema import
  alarm import
  simple-cyw43-net import
  pico-w-cyw43-net import
  
  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  <pico-w-cyw43-net> class-size buffer: my-cyw43-net
  variable my-cyw43-control
  variable my-interface

  \ Port to set server at
  6668 constant server-port
  
  \ Server active
  variable server-active?

  \ Server tx and rx task
  variable server-task

  \ Tx and rx delay
  250 constant server-delay
  
  \ Constant for number of bytes to buffer in the Rx stream
  2048 constant rx-stream-size

  \ Rx stream
  rx-stream-size stream-size buffer: rx-stream
  
  \ Constant for number of bytes to buffer in the Tx stream
  2048 constant tx-stream-size

  \ Tx stream
  tx-stream-size stream-size buffer: tx-stream

  \ Constant for number of bytes to buffer for Tx
  2048 constant tx-buffer-size
  
  \ Tx buffers
  tx-buffer-size buffer: tx-buffer
  
  \ Tx send size
  variable tx-size

  \ tx alarm
  alarm-size buffer: tx-alarm

  \ Tx send semaphore
  sema-size buffer: tx-sema
  
  \ Tx complete semaphore
  sema-size buffer: tx-complete-sema
  
  \ The TCP endpoint
  variable my-endpoint
  
  \ Do server transmission
  : do-server ( -- )
    begin
      tx-sema take
      tx-buffer tx-buffer-size tx-stream recv-stream { bytes-read }
      bytes-read tx-size !
      server-active? @ if
        tx-buffer bytes-read my-endpoint @ my-interface @ send-tcp-endpoint
        my-cyw43-net toggle-pico-w-led
        0 tx-size !
        tx-complete-sema broadcast
        tx-complete-sema give
      then
    again
  ;
  
  \ Actually handle received data
  : do-rx-data ( c-addr bytes -- )
    rx-stream send-stream-partial-no-block drop
  ;
  
  \ EMIT for telnet
  : telnet-emit ( c -- )
    server-active? @ if
      tx-stream stream-full? if tx-sema give then
      { W^ buffer }
      buffer 1 tx-stream send-stream
    else
      drop
    then
  ;

  \ EMIT? for telnet
  : telnet-emit? ( -- emit? )
    server-active? @ if
      tx-stream stream-empty? not
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
    server-active? @ if
      rx-stream stream-empty? not
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
      endpoint my-interface @ endpoint-done
      endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
        false server-active? !
        endpoint my-interface @ close-tcp-endpoint
        server-port my-interface @ allocate-tcp-listen-endpoint if
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
    rx-stream-size rx-stream init-stream
    tx-stream-size tx-stream init-stream
    0 tx-size !
    1 0 tx-sema init-sema
    1 0 tx-complete-sema init-sema
    false server-active? !
    0 my-endpoint !
    pio-addr sm-index pio-instance <pico-w-cyw43-net> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    <tcp-session-handler> my-tcp-session-handler init-object
    my-tcp-session-handler
    my-cyw43-net net-endpoint-process@ add-endpoint-handler
    0 ['] do-server 512 128 1024 1 task::spawn-on-core server-task !
    c" server-tx" server-task @ task::task-name!
    server-task @ task::run
  ;

  \ Event message buffer
  event-message-size aligned-buffer: my-event

  \ Carry out the Tx alarm
  defer tx-action
  :noname ( x x -- )
    2drop
    tx-sema give
    server-delay 0 0 ['] tx-action tx-alarm set-alarm-delay-default
  ; is tx-action
  
  \ Connect to WiFi
  : connect-wifi { D: ssid D: pass -- }
    init-test
    cyw43-consts::PM_AGGRESSIVE my-cyw43-control @ cyw43-power-management!
    begin ssid pass my-cyw43-control @ join-cyw43-wpa2 nip until
    my-cyw43-control @ disable-all-cyw43-events
    my-cyw43-control @ clear-cyw43-events
    ssid pass 4 [: { D: ssid D: pass }
      begin
        my-event my-cyw43-control @ get-cyw43-event
        my-event evt-event-type @ EVENT_DISASSOC =
        my-event evt-event-type @ EVENT_DISASSOC_IND = or if
          begin ssid pass my-cyw43-control @ join-cyw43-wpa2 nip until
        then
      again
    ;] 512 128 1024 task::spawn
    c" cyw43-event-rx" over task::task-name!
    task::run
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
    server-delay 0 0 ['] tx-action tx-alarm set-alarm-delay-default
  ;

  \ Start the server
  : start-server ( D: ssid D: pass -- )
    connect-wifi
    server-port my-interface @ allocate-tcp-listen-endpoint if
      my-endpoint !
      \ 0 [:
      \   begin
      \     my-endpoint @ if
      \       my-endpoint @ net-internal::endpoint-out-packets
      \       net-internal::out-packet-window @ dup cr ." Window: " .
      \       0< if display-red cr ." BAD WINDOW" display-normal [: ;] task::main-task task::signal exit then
      \     then
      \     1000 ms
      \   again
      \ ;] 320 128 1024 task::spawn task::run
    else
      drop
    then
  ;
  
  \ Flush the telnet console
  : telnet-flush-console ( -- )
    begin server-active? @ tx-stream stream-empty? not or tx-size @ 0<> or while
      tx-complete-sema take
    repeat
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