\ Copyright (c) 2023-2025 Travis Bemann
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
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp_common/pico_w_net_ipv6_all.fs
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
\ 1. Load extra/rp2040/net/net_ntp_ipv6.fs if using an RP2040 or
\    extra/rp2350/net/net_ntp_ipv6.fs if using an RP2350 (relative to the base
\    directory of zeptoforth) and then this code into RAM on a zeptoforth
\    install where zeptoIP has already been installed using a terminal which
\    supports zeptoforth, e.g. zeptocom.js or e4thcom in noforth mode.
\ 2. Execute: s" <WiFi SSID>" s" <WiFi password>" pico-w-net-ntp::start-client
\
\ This will start an NTP client pointed at pool.ntp.org and regularly report
\ the time.

begin-module pico-w-net-ntp
  
  oo import
  cyw43-events import
  cyw43-control import
  cyw43-structs import
  net-misc import
  frame-process import
  net-consts import
  net-config import
  net import
  net-ipv6 import
  endpoint-process import
  simple-cyw43-net-ipv6 import
  pico-w-cyw43-net-ipv6 import
  ntp-ipv6 import
  rtc import

  <pico-w-cyw43-net-ipv6> class-size buffer: my-cyw43-net
  variable my-cyw43-control
  variable my-interface
  
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  <ntp-ipv6> class-size buffer: my-ntp

  \ Initialize the test
  : init-test ( -- )
    sm-index pio-instance <pico-w-cyw43-net-ipv6> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    my-interface @ <ntp-ipv6> my-ntp init-object
    my-ntp my-cyw43-net net-endpoint-process@ add-endpoint-handler
  ;

  \ Event message buffer
  event-message-size aligned-buffer: my-event

  \ Start the client
  : start-client { D: ssid D: pass -- }
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
    cr ." Discovering IPv6 address..."
    cr ." Autoconfiguring link-local IPv6 address... "
    my-interface @ autoconfigure-link-local-ipv6-addr if
      ." Success"
    else
      ." Failure"
    then
    cr ." Discovering IPv6 router..."
    my-interface @ discover-ipv6-router
    cr ." Discovering IPv6 address..."
    my-interface @ discover-ipv6-addr if
      ." Success"
    else
      ." Failure"
    then
    my-interface @ intf-ipv6-addr@ cr ." Primary IPv6 address: " ipv6.
    my-interface @ intf-link-local-ipv6-addr@
    cr ." Link-local IPv6 address: " ipv6.
    my-interface @ intf-slaac-ipv6-addr@ cr ." SLAAC IPv6 address: " ipv6.
    my-interface @ intf-dhcpv6-ipv6-addr@ cr ." DHCPv6 IPv6 address: " ipv6.
    my-interface @ intf-ipv6-prefix@ cr ." IPv6 prefix: " ipv6.
    my-interface @ intf-autonomous@ cr ." Autonomous: "
    if ." yes" else ." no" then
    my-interface @ intf-ipv6-prefix-len@ cr ." IPv6 prefix length: " .
    my-interface @ gateway-ipv6-addr@ cr ." Gateway IPv6 address: " ipv6.
    cr ." Discovering IPv6 DNS server..."
    my-interface @ discover-dns-ipv6-addr
    my-interface @ dns-server-ipv6-addr@ cr ." DNS server IPv6 address: " ipv6.
    my-cyw43-net toggle-pico-w-led
    s" pool.ntp.org" ntp-port my-ntp init-ntp
    0 [:
      begin
        my-ntp time-set? if
          my-ntp current-time@ f.
          date-time-size [: { date-time }
            date-time date-time@ date-time date-time. space
          ;] with-aligned-allot
        then
        1000 ms
      again
    ;] 512 128 512 task::spawn
    c" ntp-display" over task::task-name!
    task::run
  ;

end-module