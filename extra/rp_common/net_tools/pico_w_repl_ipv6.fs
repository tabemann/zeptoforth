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
\ utils/codeload3.sh -B 115200 -p <tty device> serial extra/rp_common/pico_w_net_ipv6_all.fs
\ 
\ Afterwards, if you had not already installed the CYW43439 firmware, driver,
\ and zeptoIP, make sure to reboot zeptoforth, either by executing:
\
\ reboot
\
\ or by entering control-C at a terminal or Reboot with zeptocom.js.
\ 
\ Then ensure that extra/rp_common/net_tools/pico_w_ipv6_base.fs is loaded into
\ RAM and configured per the directions therein.
\
\ Once that is done, execute:
\
\ pico-w-net-repl::start-server
\ pico-w-net-repl::tcp-console
\
\ At this point the console will stop responding. Instead, the console will be
\ accessible via a TCP connection to port 6668 on the IPv6 address acquired via
\ DHCP that is reported on the console.
\
\ The recommended means of communicating with this REPL is:
\
\ stty -echo -icanon && nc <the reported IPv6 address> 6668
\
\ Note that this will set your terminal into non-echoing, non-canonical mode;
\ to return to echoing, canonical mode, once you exit out of netcat via
\ control-C, execute 'reset'; you will have to do this blindly.
\
\ Also note that this connection is not password-protected or encrypted, so one
\ should not assume any security for it.

begin-module pico-w-net-repl
    
  oo import
  net-misc import
  net-consts import
  net-config import
  net import
  net-ipv6 import
  endpoint-process import
  sema import
  slock import
  simple-cyw43-net-ipv6 import
  pico-w-cyw43-net-ipv6 import

  begin-module pico-w-net-repl-internal
    
    \ Port to set server at
    6668 constant server-port
    
    \ Server Rx lock
    slock-size buffer: server-rx-slock
    
    \ Server Tx lock
    slock-size buffer: server-tx-slock

    \ Tx notify area
    variable tx-notify-area
    
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
    \ variable tx-timeout-start
    
    \ Rx semaphore
    sema-size aligned-buffer: rx-sema
    
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
        \ 0 task::wait-notify drop
        [: server-delay 10 * systick::systick-counter 0 task::wait-notify-timeout drop ;] try
        dup ['] task::x-timed-out = if drop 0 then
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
              buffer count my-endpoint @ pico-w-net::my-interface @ send-tcp-endpoint
            then
          then
        then
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
    
    \ EMIT for TCP
    : tcp-emit ( c -- )
      server-active? @ if
        begin
          [:
            tx-full? not if
              write-tx
              true
            else
              false
            then
          ;] server-tx-slock with-slock
          0 server-task @ task::notify
          dup not if
            task::timeout @ { old-timeout }
            server-delay 10 * task::timeout !
            [: tx-block-sema take ;] try
            dup ['] task::x-timed-out = if drop 0 then
            old-timeout task::timeout !
            ?raise
          then
        until
      else
        drop
      then
    ;

    \ EMIT? for TCP
    : tcp-emit? ( -- emit? )
      server-active? @ if
        [: tx-full? not ;] server-tx-slock with-slock
      else
        false
      then
    ;

    \ KEY for TCP
    : tcp-key ( -- c )
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
            [: rx-sema take ;] try
            dup ['] task::x-timed-out = if drop 0 then
            old-timeout task::timeout !
            ?raise
          then
        else
          false
        then
      until
    ;

    \ KEY? for TCP
    : tcp-key? ( -- key? )
      server-active? @ if
        [: rx-empty? not ;] server-rx-slock with-slock
      else
        false
      then
    ;
    
    \ Type data over TCP
    : tcp-type { c-addr bytes -- }
      c-addr bytes + c-addr ?do i c@ tcp-emit loop
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
        endpoint pico-w-net::my-interface @ endpoint-done
        endpoint endpoint-tcp-state@ TCP_CLOSE_WAIT = if
          false server-active? !
          endpoint pico-w-net::my-interface @ close-tcp-endpoint
          server-port pico-w-net::my-interface @ allocate-tcp-listen-endpoint if
            my-endpoint !
          else
            drop
          then
        then
      ; define handle-endpoint
      
    end-implement
    
    <tcp-session-handler> class-size buffer: my-tcp-session-handler

    \ Initialize the test
    : init-tcp-session ( -- )
      server-tx-slock init-slock
      server-rx-slock init-slock
      0 tx-buffer-index !
      0 tx-write-index !
      0 tx-write-index cell+ !
      0 rx-read-index !
      0 rx-write-index !
      false server-active? !
      0 my-endpoint !
      1 0 rx-sema init-sema
      1 0 tx-block-sema init-sema
      <tcp-session-handler> my-tcp-session-handler init-object
      my-tcp-session-handler
      pico-w-net::my-cyw43-net net-endpoint-process@ add-endpoint-handler
      0 ['] do-server 512 128 1024 1 task::spawn-on-core server-task !
      tx-notify-area 1 server-task @ task::config-notify
      c" repl-tx" server-task @ task::task-name!
      server-task @ task::run
    ;

  end-module> import
    
  \ Start the server
  : start-server ( -- )
    init-tcp-session
    server-port pico-w-net::my-interface @ allocate-tcp-listen-endpoint if
      my-endpoint !
    else
      drop
    then
  ;

  \ Flush the TCP console
  : tcp-flush-console ( -- )
    server-active? @ if
      begin
        [:
          tx-empty? if
            true
          else
            false
          then
        ;] server-tx-slock with-slock
        0 server-task @ task::notify
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
    then
  ;
  
  \ Set up TCP as a console
  : tcp-console ( -- )
    ['] tcp-key key-hook !
    ['] tcp-key? key?-hook !
    ['] tcp-emit emit-hook !
    ['] tcp-emit? emit?-hook !
    ['] tcp-emit error-emit-hook !
    ['] tcp-emit? error-emit?-hook !
  ;
  
  \ Set the current input to TCP within an xt
  : with-tcp-input ( xt -- )
    ['] tcp-key
    ['] tcp-key?
    rot console::with-input
  ;
  
  \ Set the current output to TCP within an xt
  : with-tcp-output ( xt -- )
    ['] tcp-emit
    ['] tcp-emit?
    rot
    ['] tcp-flush-console
    swap console::with-output
  ;

  \ Set the current error output to TCP within an xt
  : with-tcp-error-output ( xt -- )
    ['] tcp-emit
    ['] tcp-emit?
    rot
    ['] tcp-flush-console
    swap console::with-error-output
  ;

end-module