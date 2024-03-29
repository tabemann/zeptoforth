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
  esp-at import
  wio-esp-at import
  task import
  slock import
  internal import

  \ Port to set server at
  6667 constant server-port

  \ ESP-AT objects
  <esp-at> class-size buffer: device
  <wio-esp-at-spi> class-size buffer: intf
  <esp-at-status> class-size buffer: status

  \ Found connection
  variable found-mux
  
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

  \ Find a client
  : find-client ( -- mux found? )
    -1 found-mux @ = if
      status ['] esp-at-status@ device with-esp-at
      status esp-at-status-count@ 0> if
        0 status esp-at-status-mux@ dup found-mux ! true
      else
        0 false
      then
    else
      found-mux @ true
    then
  ;

  \ Do server transmission and receiving
  : do-server ( -- )
    begin
      [:
        server-delay ms
        server-active? @ if
          [: poll-esp-at ;] device with-esp-at
          [: tx-empty? ;] server-slock with-slock not if
            find-client if
              { mux }
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
              actual-tx-buffer count mux ['] multi>esp-at device with-esp-at
            else
              drop
              [:
                0 tx-read-index !
                0 tx-write-index !
              ;] server-slock with-slock
            then
          then
        then 
      ;] try if
        [: ['] test-esp-at device with-esp-at ;] try ?dup if
          int-io::serial-console
          cr display-red execute display-normal
          true
        else
          false
        then
      else
        false
      then
    until
  ;
  
  \ Actually handle received data
  : do-rx-data ( c-addr bytes mux -- )
    [: { c-addr bytes mux }
      c-addr bytes + c-addr ?do
        rx-full? not if
          i c@ write-rx
        else
          leave
        then
      loop
    ;] server-slock with-slock
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
            false
          then
        ;] server-slock with-slock
        dup not if server-delay ms then
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
        dup not if server-delay ms then
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

  \ Initialize the test
  : init-test ( -- )
    server-slock init-slock
    0 tx-read-index !
    0 tx-write-index !
    0 rx-read-index !
    0 rx-write-index !
    -1 found-mux !
    <wio-esp-at-spi> intf init-object
    intf <esp-at> device init-object
    <esp-at-status> status init-object
    200000 device esp-at-timeout!
    true intf esp-at-log!
    [: { device } station-mode device init-esp-at ;] device with-esp-at
    0 ['] do-server 320 128 768 1 spawn-on-core server-task !
    server-task @ run
  ;

  \ Connect to WiFi
  : connect-wifi ( D: password D: ssid -- )
    [: { D: password D: ssid device }        
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

      device esp-at-station-ipv4-addr@ if
        cr ." Station IPv4 address: " type 
      else
        2drop cr ." No station IPv4 address"
      then
    ;] device with-esp-at
  ;

  \ Start the server
  : start-server ( -- )
    [: { device }
      true device esp-at-multi!
      ['] do-rx-data device esp-at-recv-xt!
      server-port device start-esp-at-server
      true server-active? !
    ;] device with-esp-at
  ;
  
  \ Set up telnet as a console
  : telnet-console ( -- )
    false intf esp-at-log!
    ['] telnet-key key-hook !
    ['] telnet-key? key?-hook !
    ['] telnet-emit emit-hook !
    ['] telnet-emit? emit?-hook !
  ;

end-module