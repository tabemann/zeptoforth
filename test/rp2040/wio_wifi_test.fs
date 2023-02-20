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

begin-module wifi-test

  oo import
  esp-at import
  wio-esp-at import
  
  <esp-at> class-size buffer: device
  <wio-esp-at-spi> class-size buffer: intf
  <esp-at-status> class-size buffer: status
  
  : reset-wifi ( -- ) [: reset-esp-at ;] device with-esp-at ;
  
  : configure-power { device -- }
    disable-sleep-mode device esp-at-sleep!
    device esp-at-wifi-power@ cr ." Old WiFi power: " dup .
    
    80 < if
      10 0 ?do
        82 device esp-at-wifi-power!
        device esp-at-wifi-power@ cr ." New WiFi power: " dup .
        80 >= if leave then
      loop
    then
  ;
  
  : connect-to-ap { D: password D: ssid device -- }
    begin
      password ssid device [:
        4 pick 4 pick 4 pick 4 pick 4 pick connect-esp-at-wifi
      ;] try nip nip nip nip nip
      dup 0= if
        drop true
      else
        dup ['] x-esp-at-error = if
          drop cr ." RETRYING" 25 0 do 1000 ms ." ." loop cr false
        else
          ?raise
        then
      then
    until
  ;
  
  : station-info { device -- }
    device esp-at-station-ipv4-addr@ if
      cr ." Station IPv4 address: " type
    else
      2drop cr ." No station IPv4 address"
    then
    device esp-at-station-ipv6-gl-addr@ if
      cr ." Station IPv6 global address: " type
    else
      2drop cr ." No station IPv6 global address"
    then
  ;
  
  : resolve-google { device -- }
    resolve-ipv4-only s" google.com" device resolve-esp-at-ip if
      cr ." google.com IPv4 address: " type
    else
      2drop cr ." No google.com IPv4 address"
    then
  ;
  
  : single-google-request { device -- }
    [: cr ." >>>>> " . ." >>>>>" cr type ;] device esp-at-recv-xt!
    0 tcp 80 s\" google.com" device start-esp-at-single
    s\" GET / HTTP/1.1\r\n" device single>esp-at
    s\" Host: google.com\r\n" device single>esp-at
    s\" Accept: */*\r\n" device single>esp-at
    s\" Connection: close\r\n\r\n" device single>esp-at
    
    systick::systick-counter { start-time }
    begin
      device poll-esp-at
      systick::systick-counter start-time - 100000 >=
    until
    
    status device esp-at-status@
    status esp-at-status.
   
    device close-esp-at-single
  ;
  
  : multi-google-request { device -- }
    [: cr ." >>>>> " . ." >>>>>" cr type ;] device esp-at-recv-xt!
    true device esp-at-multi!
    
    0 tcp 80 s\" google.com" 0 device start-esp-at-multi
    s\" GET / HTTP/1.1\r\n" 0 device multi>esp-at
    s\" Host: google.com\r\n" 0 device multi>esp-at
    s\" Accept: */*\r\n\r\n" 0 device multi>esp-at
    
    systick::systick-counter { start-time }
    begin
      device poll-esp-at
      systick::systick-counter start-time - 100000 >=
    until

    status device esp-at-status@
    status esp-at-status.

    0 device close-esp-at-multi
  ;
  
  : run-test ( D: password D: ssid -- )
    <wio-esp-at-spi> intf init-object
    intf <esp-at> device init-object
    <esp-at-status> status init-object
    true intf esp-at-log!
    200000 device esp-at-timeout!
    500 device esp-at-delay!
    
    1 ms
    intf power-wio-esp-at-off
    1 ms
    intf power-wio-esp-at-on
    
    [: { D: password D: ssid device }
      station-mode device init-esp-at
      device configure-power
      password ssid device connect-to-ap
      device station-info
      device resolve-google
      device single-google-request
      device multi-google-request
    ;] device with-esp-at
  ;
  
  : get-status ( -- )
    [: { device }
      status device esp-at-status@
      status esp-at-status.
    ;] device with-esp-at
  ;
  
end-module

