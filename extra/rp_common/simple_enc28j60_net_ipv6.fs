\ Copyright (c) 2020-2025 Travis Bemann, Paul Koning
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
\
\ This is the ENC28J60 driver for ZeptoIP by Paul Koning, derived from the
\ CYW43xxx driver by Travis Bemann.

begin-module simple-enc28j60-net-ipv6

  oo import
  enc28j60-runner import
  enc28j60-runner-internal import
  enc28j60-bus import
  enc28j60-bus-internal import
  simple-net-ipv6 import

  \ A simple ENC28J60 networking and interface class
  <simple-net-ipv6> begin-class <simple-enc28j60-net-ipv6>

    begin-module simple-enc28j60-net-ipv6-internal

      \ The ENC28J60 controller
      <enc28j60-runner> class-size member my-enc28j60-runner

      cell member enc28j60-duplex
      
    end-module> import
    
    \ Enable a MAC address
    method enable-mac ( D: d-mac-addr self -- )

    \ Set duplex mode
    method set-duplex ( full-duplex self -- )

  end-class

  \ Implement the ENC28J60 networking and interface class
  <simple-enc28j60-net-ipv6> begin-implement

    \ The constructor.  Arguments are the interrupt pin, the first pin (of four)
    \ of the SPI interface, the MAC address to use, and the full duplex flag.
    :noname { int-pin spi-pin spi-num D: mac-address duplex self -- }
      self <simple-net-ipv6>->new
      int-pin spi-pin spi-num mac-address <enc28j60-runner> self my-enc28j60-runner init-object
      duplex self enc28j60-duplex !
    ; define new

    \ Initialize the ENC28J60 network and interface object without starting
    \ the endpoint process
    :noname { self -- }
      self enc28j60-duplex @ self my-enc28j60-runner init-enc28j60-runner
      self <simple-net-ipv6>->init-net-no-handler
    ; define init-net-no-handler

    \ Initialize the ENC28J60 network and start it
    :noname { self -- }
      self enc28j60-duplex @ self my-enc28j60-runner init-enc28j60-runner
      self <simple-net-ipv6>->init-net
      self my-enc28j60-runner run-enc28j60
    ; define init-net
      
    \ Get the ENC28J60 controller
    :noname ( self -- frame-interface )
      my-enc28j60-runner enc28j60-frame-interface
    ; define device-frame-interface@

    \ Enable a MAC address.
    :noname ( D: d-mac-addr self -- )
      my-enc28j60-runner enc28j60-enable-mac
    ; define enable-mac

    \ Set duplex mode (true is full duplex)
    :noname ( full-duplex self -- )
      my-enc28j60-runner enc28j60-set-duplex
    ; define set-duplex
    
  end-implement
  
end-module
