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

begin-module net

  oo import
  frame-interface import
  frame-process import
  net-consts import
  net-config import
  net-misc import
  lock import
  sema import
  chan import
  heap import

  \ Oversized frame exception
  : x-oversized-frame ( -- ) ." oversized frame" cr ;

  \ Invalid DNS name
  : x-invalid-dns-name ( -- ) ." invalid DNS name" cr ;

  \ Validate DNS name
  : validate-dns-name { addr len -- }
    len 0 > averts x-invalid-dns-name
    len max-dns-name-len <= averts x-invalid-dns-name
    addr c@ [char] . <> averts x-invalid-dns-name
    addr len 1- + c@ [char] . <> averts x-invalid-dns-name
    len 1- 0 ?do
      addr i + c@ dup $20 > averts x-invalid-dns-name
      [char] . = if addr i 1+ + c@ [char] . <> averts x-invalid-dns-name then
    loop
  ;

  \ The endpoint class
  <object> begin-class <endpoint>

    \ Get endpoint TCP state
    method endpoint-tcp-state@ ( self -- tcp-state )

    \ Get endpoint received data
    method endpoint-rx-data@ ( self -- addr bytes )
      
    \ Endpoint is a UDP endpoint
    method udp-endpoint? ( self -- udp? )

    \ Get local port
    method endpoint-local-port@ ( self -- port )

    \ Get whether there is waiting data
    method waiting-rx-data? ( self -- waiting? )

  end-class

  <endpoint> begin-implement
  end-implement
  
  \ The interface class
  <object> begin-class <interface>
    
    \ Get the MAC address
    method intf-mac-addr@ ( self -- D: addr )

    \ Set Multicast DNS enabled
    method mdns-enabled! ( enabled? self -- )

    \ Get Multicast DNS enabled
    method mdns-enabled@ ( self -- enabled? )
    
    \ Send data on a TCP endpoint
    method send-tcp-endpoint ( addr bytes endpoint self -- )

    \ Evict a DNS name's entry, forcing it to be re-resolved
    method evict-dns ( c-addr bytes self -- )
    
    \ Wait for an endpoint to become ready
    method wait-ready-endpoint ( endpoint self -- )
    
    \ Dequeue a ready receiving IP endpoint
    method get-ready-endpoint ( self -- endpoint )

    \ Mark an endpoint as done
    method endpoint-done ( endpoint self -- )

    \ Get a UDP endpoint to listen on
    method allocate-udp-listen-endpoint ( port self -- endpoint success? )

    \ Get a TCP endpoint to listen on
    method allocate-tcp-listen-endpoint ( port self -- endpoint success? )

    \ Close a UDP endpoint
    method close-udp-endpoint ( endpoint self -- )

    \ Close a TCP endpoint
    method close-tcp-endpoint ( endpoint self -- )

  end-class

  \ Implement the interface class
  <interface> begin-implement
  end-implement
  
end-module
