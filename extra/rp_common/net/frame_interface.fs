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

begin-module frame-interface

  oo import

  <object> begin-class <frame-interface>

    \ Get the MTU size
    method mtu-size@ ( self -- bytes )

    \ Get the MAC address
    method mac-addr@ ( self -- D: mac-addr )

    \ Set the MAC address
    method mac-addr! ( D: mac-addr self -- )

    \ Add a MAC address to the multicast filter
    method add-multicast-filter ( D: mac-addr self -- )

    \ Remove a MAC address from the multicast filter
    method remove-multicast-filter ( D: mac-addr self -- )

    \ Attempt to put a received frame
    method poll-put-rx-frame ( addr bytes self -- success? )
    
    \ Put a received frame
    method put-rx-frame ( addr bytes self -- )

    \ Get a received frame
    method get-rx-frame ( self -- addr bytes )

    \ Poll a received frame
    method poll-rx-frame ( self -- addr bytes found? )

    \ Retire a received frame
    method retire-rx-frame ( self -- )

    \ Attempt to put a frame to transmit
    method poll-put-tx-frame ( addr bytes self -- success? )
    
    \ Put a frame to transmit
    method put-tx-frame ( addr bytes self -- )

    \ Get a frame to transmit
    method get-tx-frame ( self -- addr bytes )

    \ Poll a frame to transmit
    method poll-tx-frame ( self -- addr bytes found? )

    \ Retire a frame to transmit
    method retire-tx-frame ( self -- )

  end-class

  <frame-interface> begin-implement
  end-implement

end-module
