\ Copyright (c) 2020-2025 Travis Bemann
\ Copyright (c) 2025 Paul Koning
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

begin-module simple-cyw43-net-ipv6

  oo import
  cyw43-control import
  net-misc import
  frame-process import
  net import
  endpoint-process import
  simple-net-ipv6 import
  
  \ A simple IPv6 CYW43439 networking and interface class
  <simple-net-ipv6> begin-class <simple-cyw43-net-ipv6>

    begin-module simple-cyw43-net-internal

      \ The CYW43 controller
      <cyw43-control> class-size member my-cyw43-control

    end-module> import

    \ Set country - must be set prior to executing init-cyw43-net, and if not
    \ set defaults to an abbreviation and code of XX\x00\x00 and a revision of
    \ -1
    method cyw43-net-country!
    ( abbrev-addr abbrev-bytes code-addr code-bytes country-rev self -- )

      \ Set a GPIO on the CYW43439
    method cyw43-gpio! ( state gpio self -- ) 

    \ Get the CYW43439 controller
    method cyw43-control@ ( self -- control )

    \ These just aliases, but they are sometimes invoked as
    \ <simple-cyw43-net-ipv6>->init-cyw43-net etc. so the need to be methods
    method init-cyw43-net ( self -- )
    method init-cyw43-net-no-handler ( self -- )

  end-class

  \ Implement the CYW43439 networking and interface class
  <simple-cyw43-net-ipv6> begin-implement

    \ The constructor, using a specified PWR pin, DIO pin, CS pin, CLK pin,
    \ PIO state machine index, and PIO instance
    :noname
      { pwr-pin dio-pin cs-pin clk-pin sm-index pio-instance self -- }
      self <simple-net-ipv6>->new

      default-mac-addr
      cyw43-clm::data cyw43-clm::size cyw43-fw::data cyw43-fw::size
      pwr-pin clk-pin dio-pin cs-pin sm-index pio-instance
      <cyw43-control> self my-cyw43-control init-object
    ; define new

    \ Supply the method for finding the frame interface
    :noname ( self -- frame-interface )
      my-cyw43-control cyw43-frame-interface@
    ; define device-frame-interface@

    \ Set country - must be set prior to executing init-cyw43-net, and if not
    \ set defaults to an abbreviation and code of XX\x00\x00 and a revision of
    \ -1
    :noname
      ( abbrev-addr abbrev-bytes code-addr code-bytes country-rev self -- )
      my-cyw43-control cyw43-country!
    ; define cyw43-net-country!

    \ Initialize the CYW43439 network and interface object
    \ Alias for the old name - nothing else needed
    :noname ( self -- )
      init-net
    ; define init-cyw43-net
 
    \ Initialize the CYW43439 network and interface object without starting
    \ the endpoint process
    :noname { self -- }
      self my-cyw43-control init-cyw43
      self <simple-net-ipv6>->init-net-no-handler
    ; define init-net-no-handler

    \ Alias for the old name
    :noname ( self -- )
      init-net-no-handler
    ; define init-cyw43-net-no-handler
    
    \ Set a GPIO on the CYW43439
    :noname ( state gpio self -- )
      my-cyw43-control <cyw43-control>->cyw43-gpio!
    ; define cyw43-gpio!

    \ Get the CYW43439 controller
    :noname ( self -- control )
      my-cyw43-control
    ; define cyw43-control@

  end-implement

  \ Re-exporting <simple-net-ipv6> methods for the sake of compatibility

  \ Initialize a networking and interface class instance
  : init-cyw43-net ( self -- ) init-net ;
  
  \ Initialize the network and interface object without starting
  \ the endpoint process
  : init-cyw43-net-no-handler ( self -- ) init-net-no-handler ;
  
  \ Get the zeptoIP interface
  : net-interface@ ( self -- interface ) net-interface@ ;
  
  \ Get the zeptoIP endpoint processor
  : net-endpoint-process@ ( self -- endpoint-processor ) net-endpoint-process@ ;
  
  \ Run network interface processors
  : run-net-process ( self -- ) run-net-process ;
  
end-module
