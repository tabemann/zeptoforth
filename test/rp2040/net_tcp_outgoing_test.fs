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

begin-module net-test
  
  oo import
  cyw43-control import
  net-misc import
  frame-process import
  net import
  endpoint-process import
  alarm import
  net-diagnostic import
  
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
  alarm-size buffer: my-close-alarm
  <arp-diag-handler> class-size buffer: my-arp-diag-handler
  <ipv4-diag-handler> class-size buffer: my-ipv4-diag-handler
  
  \ Have we sent the header
  false value sent-header?
  
  \ Are we closing?
  false value closing?
    
  \ Our MAC address
  default-mac-addr 2constant my-mac-addr

  <endpoint-handler> begin-class <tcp-echo-handler>
    
  end-class

  <tcp-echo-handler> begin-implement
  
    \ Handle a endpoint packet
    :noname { endpoint self -- }
      sent-header? not endpoint endpoint-tcp-state@ TCP_ESTABLISHED = and if
        cr ." SENDING HEADER" cr
        true to sent-header?
        s\" GET / HTTP/1.1\r\n" endpoint my-interface send-tcp-endpoint
        s\" Host: www.google.com\r\n" endpoint my-interface send-tcp-endpoint
        s\" Accept: */*\r\n" endpoint my-interface send-tcp-endpoint
        s\" Connection: close\r\n\r\n" endpoint my-interface send-tcp-endpoint
      then
      endpoint endpoint-rx-data@ type
      endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
        cr ." CLOSING CONNECTION" cr
        closing? not if
          true to closing?
          cr ." REGISTERING CLOSE ALARM" cr
          500 0 endpoint [:
            drop my-interface close-tcp-endpoint
            cr ." close-tcp-endpoint RETURNED" cr
          ;] my-close-alarm set-alarm-delay-default
        then
      then
      endpoint endpoint-tcp-state@ TCP_LAST_ACK = if
        cr ." WAITING FOR LAST ACK" cr
      then
      endpoint endpoint-tcp-state@ TCP_CLOSED = if
        cr ." CONNECTION CLOSED" cr
      then
      endpoint my-interface endpoint-done
    ; define handle-endpoint
  
  end-implement
  
  <tcp-echo-handler> class-size buffer: my-tcp-echo-handler
  variable my-endpoint
  
  : init-test ( -- )
    my-mac-addr cyw43-clm::data cyw43-clm::size cyw43-fw::data cyw43-fw::size
    pwr-pin clk-pin dio-pin cs-pin pio-addr sm-index pio-instance
    <cyw43-control> my-cyw43-control init-object
    my-cyw43-control init-cyw43
    cyw43-consts::PM_NONE my-cyw43-control cyw43-power-management!
    my-cyw43-control cyw43-frame-interface@ <interface> my-interface init-object
    \ 192 168 86 49 make-ipv4-addr my-interface intf-ipv4-addr!
    \ 192 168 86 1 make-ipv4-addr my-interface gateway-ipv4-addr !
    \ 255 255 255 0 make-ipv4-addr my-interface intf-ipv4-netmask!
    \ 192 168 86 1 make-ipv4-addr my-interface dns-server-ipv4-addr !
    my-cyw43-control cyw43-frame-interface@ <frame-process> my-frame-process init-object
    \ <arp-diag-handler> my-arp-diag-handler init-object
    \ <ipv4-diag-handler> my-ipv4-diag-handler init-object
    \ my-arp-diag-handler my-frame-process add-frame-handler
    \ my-ipv4-diag-handler my-frame-process add-frame-handler
    my-interface <arp-handler> my-arp-handler init-object
    my-interface <ip-handler> my-ip-handler init-object
    my-arp-handler my-frame-process add-frame-handler
    my-ip-handler my-frame-process add-frame-handler
    my-interface <endpoint-process> my-endpoint-process init-object
    <tcp-echo-handler> my-tcp-echo-handler init-object
    my-tcp-echo-handler my-endpoint-process add-endpoint-handler
  ;

  : run-test { D: ssid D: pass -- }
    init-test
    begin ssid pass my-cyw43-control join-cyw43-wpa2 .s nip until
    my-cyw43-control disable-all-cyw43-events
    my-endpoint-process run-endpoint-process
    my-frame-process run-frame-process
    cr ." Discovering IPv4 address..."
    my-interface discover-ipv4-addr
    my-interface intf-ipv4-addr@ cr ." IPv4 address: " ipv4.
    my-interface intf-ipv4-netmask@ cr ." IPv4 netmask: " ipv4.
    my-interface gateway-ipv4-addr@ cr ." Gateway IPv4 address: " ipv4.
    my-interface dns-server-ipv4-addr@ cr ." DNS server IPv4 address: " ipv4.
    cr ." DNS LOOKUP"
    s" www.google.com" my-interface resolve-dns-ipv4-addr if { addr }
      cr ." RESOLVED: " addr ipv4.
      EPHEMERAL_PORT addr 80 my-interface allocate-tcp-connect-ipv4-endpoint if
        drop cr ." CONNECTED" cr
      else
        cr ." NOT CONNECTED" drop
      then
    else
      cr ." NOT RESOLVED" drop
    then
  ;

end-module