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
\ 2. Execute: s" <WiFi SSID>" s" <WiFi password>" pico-w-net-uart::start-server
\
\ At this point one will be able to establish a TCP connection to port 6668 on
\ the IPv4 address acquired via DHCP that is reported on the console. Any data
\ received by the Raspberry Pi Pico W via this TCP connection will be then
\ transmitted via UART0. Likewise, any data received via UART0 will be
\ transmitted via this TCP connnection. Note that UART0 will be mapped to its
\ default of GPIO pins 0 and 1.

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
  16384 constant tx-buffer-size
  
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
            buffer count my-endpoint @ my-interface @ send-tcp-endpoint
            \ my-cyw43-net toggle-pico-w-led
            0 tx-count !
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
        then
        true server-active? !
      then
      endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
        closing? @ not if
          true closing? !
          0 0 endpoint [: drop { endpoint }
            false server-active? !
            endpoint my-interface @ close-tcp-endpoint
            endpoint endpoint-tcp-state@ TCP_CLOSED = if
              false closing? !
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
    c" tx" tx-task @ task::task-name!
    c" uart-rx" uart-rx-task @ task::task-name!
    tx-task @ task::run
    uart-rx-task @ task::run
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
    else
      drop
    then
  ;

end-module