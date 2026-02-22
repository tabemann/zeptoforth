\ Copyright (c) 2023-2026 Travis Bemann
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
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp_common/pico_w_net_ipv4_all.fs
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
\ 1. Execute: s" <WiFi SSID>" s" <WiFi password>" s" <mDNS hostname>" pico-w-net::init-wifi

begin-module pico-w-net
  
  oo import
  cyw43-events import
  cyw43-control import
  cyw43-structs import
  net-misc import
  frame-process import
  net-consts import
  net-config import
  net import
  net-ipv4 import
  endpoint-process import
  simple-cyw43-net-ipv4 import
  pico-w-cyw43-net-ipv4 import

  <pico-w-cyw43-net-ipv4> class-size buffer: my-cyw43-net
  variable my-cyw43-control
  variable my-interface
  
  -1 constant sm-index
  -1 constant pio-instance

  \ Event message buffer
  event-message-size aligned-buffer: my-event
  
  \ WiFi SSID minimum length
  1 constant ssid-min-len
  
  \ WiFi SSID maximum length
  32 constant ssid-max-len
  
  \ WiFi password minimum length
  8 constant pass-min-len
  
  \ WiFi password maximum length
  63 constant pass-max-len
  
  \ WiFi SSID
  ssid-max-len 1+ cell align buffer: saved-ssid
  
  \ WiFi password
  pass-max-len 1+ cell align buffer: saved-pass
  
  \ Out of range SSID length
  : x-out-of-range-ssid-len ." out of range SSID length" cr ;
  
  \ Out of range password length
  : x-out-of-range-pass-len ." out of range password length" cr ;
  
  \ Save as counted string
  : cstring! { src bytes dest -- }
    bytes 255 min 0 max to bytes
    bytes dest c!
    src dest 1+ bytes move
  ;
  
  \ Validate SSID
  : validate-ssid { addr bytes -- }
    bytes ssid-min-len >= averts x-out-of-range-ssid-len
    bytes ssid-max-len <= averts x-out-of-range-ssid-len
  ;
  
  \ Validate password
  : validate-pass { addr bytes -- }
    bytes pass-min-len >= averts x-out-of-range-pass-len
    bytes pass-max-len <= averts x-out-of-range-pass-len
  ;
  
  \ Get SSID
  : ssid@ ( -- addr bytes ) saved-ssid count ;
  
  \ Get password
  : pass@ ( -- addr bytes ) saved-pass count ;
  
  \ Initialize WiFi
  : init-wifi { D: ssid D: pass D: hostname -- }
    ssid validate-ssid
    pass validate-pass
    hostname net::validate-dns-name
    ssid saved-ssid cstring!
    pass saved-pass cstring!
    sm-index pio-instance <pico-w-cyw43-net-ipv4> my-cyw43-net init-object
    my-cyw43-net cyw43-control@ my-cyw43-control !
    my-cyw43-net net-interface@ my-interface !
    my-cyw43-net init-cyw43-net
    cyw43-consts::PM_AGGRESSIVE my-cyw43-control @ cyw43-power-management!
    begin ssid@ pass@ my-cyw43-control @ join-cyw43-wpa2 nip until
    my-cyw43-control @ disable-all-cyw43-events
    my-cyw43-control @ clear-cyw43-events
    0 [:
      begin
        1000 ms
        my-event my-cyw43-control @ get-cyw43-event
        my-event evt-event-type @ EVENT_DISASSOC =
        my-event evt-event-type @ EVENT_DISASSOC_IND = or if
          begin ssid@ pass@ my-cyw43-control @ join-cyw43-wpa2 nip until
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
    true my-interface @ mdns-enabled!
    hostname my-interface @ mdns-hostname!
    my-cyw43-net toggle-pico-w-led
  ;
  
end-module