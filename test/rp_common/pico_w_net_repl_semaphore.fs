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
  sema import
  slock import
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
  
  \ Server Rx lock
  slock-size buffer: server-rx-slock
  
  \ Server Tx lock
  slock-size buffer: server-tx-slock
  
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
  
  \ Variables for tx buffer write-index
  2variable tx-write-index
  
  \ Tx buffer index
  variable tx-buffer-index
    
  \ Constant for number of bytes to buffer
  2048 constant tx-buffer-size
  
  \ Tx buffers
  tx-buffer-size 2 * buffer: tx-buffers
  
  \ Tx timeout
  100 value tx-timeout
  
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
    tx-write-index tx-buffer-index @ cells + @ tx-buffer-size =
  ;
  
  \ Get whether the tx buffer is empty
  : tx-empty? ( -- f )
    tx-write-index @ 0= tx-write-index cell+ @ 0= and
  ;

  \ Write a byte to the tx buffer
  : write-tx { c -- }
    tx-buffer-index @ { buffer-index }
    tx-write-index buffer-index cells + { write-var }
    write-var @ { write-index }
    write-index tx-buffer-size <> if
      c tx-buffers buffer-index tx-buffer-size * + write-index + c!
      1 write-var +!
    then
  ;

  \ Do server transmission
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
          tx-buffer-index @ { buffer-index }
          tx-buffers buffer-index tx-buffer-size * +
          tx-write-index buffer-index cells + @
          buffer-index 1+ 1 and dup { new-buffer-index } tx-buffer-index ! 
          0 tx-write-index new-buffer-index cells + !
        ;] server-tx-slock with-slock { buffer count }
        count 0> if
          my-endpoint @ if
            buffer count my-endpoint @ my-interface @ send-tcp-endpoint
            my-cyw43-net toggle-pico-w-led
          then
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
      ;] server-rx-slock with-slock
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
        ;] server-tx-slock with-slock
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
      [: tx-full? not ;] server-tx-slock with-slock
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
        ;] server-rx-slock with-slock
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
      [: rx-empty? not ;] server-rx-slock with-slock
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
    server-tx-slock init-slock
    server-rx-slock init-slock
    0 tx-buffer-index !
    0 tx-write-index !
    0 tx-write-index cell+ !
    0 rx-read-index !
    0 rx-write-index !
    systick::systick-counter tx-timeout-start !
    false server-active? !
    0 my-endpoint !
    1 0 rx-sema init-sema
    1 0 tx-sema init-sema
    1 0 tx-block-sema init-sema
    pio-addr sm-index pio-instance <pico-w-cyw43-net> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    <tcp-session-handler> my-tcp-session-handler init-object
    my-tcp-session-handler
    my-cyw43-net net-endpoint-process@ add-endpoint-handler
    0 ['] do-server 512 128 1024 1 task::spawn-on-core server-task !
    c" repl-tx" server-task @ task::task-name!
    server-task @ task::run
  ;

  \ Event message buffer
  event-message-size aligned-buffer: my-event

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
  ;

  \ Start the server
  : start-server ( D: ssid D: pass -- )
    connect-wifi
    server-port my-interface @ allocate-tcp-listen-endpoint if
      my-endpoint !
\      0 [:
\        begin
\          my-endpoint @ if
\            my-endpoint @ net-internal::endpoint-out-packets
\            net-internal::out-packet-window @ dup cr ." Window: " .
\            0< if display-red cr ." BAD WINDOW" display-normal [: ;] task::main-task task::signal exit then
\          then
\          1000 ms
\        again
\      ;] 320 128 1024 task::spawn task::run
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
        ;] server-tx-slock with-slock
        dup not if server-delay ms then
      until
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