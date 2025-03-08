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
\ SOFTWARE.

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
\ 2. Execute: s" <WiFi SSID>" s" <WiFi password>" pico-w-net-udp::start-server
\
\ This will establish a UDP server on port 4444 at the IPv4 address acquired via
\ DHCP that is reported on the console.
\
\ The recommended means of communicating with this REPL is:
\
\ nc -u <the reported IPv4 address> 4444
\
\ Each line transmitted by netcat will be echoed back to the user via UDP.

begin-module pico-w-net-udp
  
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
  simple-cyw43-net import
  pico-w-cyw43-net import

  <pico-w-cyw43-net> class-size buffer: my-cyw43-net
  variable my-cyw43-control
  variable my-interface
  
  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  \ Our port
  4444 constant my-port
  
  <endpoint-handler> begin-class <udp-echo-handler>
    
  end-class

  <udp-echo-handler> begin-implement
  
    \ Handle a endpoint packet
    :noname { endpoint self -- }
      endpoint endpoint-ipv4-remote@ { src-addr src-port }
      endpoint endpoint-rx-data@ { addr bytes }
      addr bytes my-port src-addr src-port bytes [: { addr bytes buf }
        addr buf bytes move true
      ;] my-interface @ send-ipv4-udp-packet drop
      my-cyw43-net toggle-pico-w-led
      endpoint my-interface @ endpoint-done
    ; define handle-endpoint
  
  end-implement
  
  <udp-echo-handler> class-size buffer: my-udp-echo-handler

  \ Initialize the test
  : init-test ( -- )
    pio-addr sm-index pio-instance <pico-w-cyw43-net> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    <udp-echo-handler> my-udp-echo-handler init-object
    my-udp-echo-handler
    my-cyw43-net net-endpoint-process@ add-endpoint-handler
    my-port my-interface @ allocate-udp-listen-endpoint if
      drop
    else
      [: ." unable to allocate UDP listen endpoint" cr ;] ?raise
    then
  ;

  \ Event message buffer
  event-message-size aligned-buffer: my-event

  \ Start the server
  : start-server { D: ssid D: pass -- }
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

end-module