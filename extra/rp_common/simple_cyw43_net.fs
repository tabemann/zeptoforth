\ Copyright (c) 2020-2023 Travis Bemann
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

begin-module simple-cyw43-net

  oo import
  cyw43-control import
  net-misc import
  frame-process import
  net import
  endpoint-process import

  \ Endpoint process has not been started exception
  : x-endpoint-process-not-started ( -- ) ." endpoint process not started" cr ;
  
  \ A simple CYW43439 networking and interface class
  <object> begin-class <simple-cyw43-net>

    begin-module simple-cyw43-net-internal

      \ The CYW43 controller
      <cyw43-control> class-size member my-cyw43-control

      \ The IP stack interface
      <interface> class-size member my-interface

      \ The frame processor
      <frame-process> class-size member my-frame-process

      \ The ARP frame handler
      <arp-handler> class-size member my-arp-handler

      \ The IP frame handler
      <ip-handler> class-size member my-ip-handler

      \ The endpoint processor
      <endpoint-process> class-size member my-endpoint-process

      \ Has the endpoint process been started?
      cell member endpoint-process-started?
      
    end-module> import

    \ Initialize a CYW43439 networking and interface class instance
    method init-cyw43-net ( self -- )

    \ Initialize the CYW43439 network and interface object without starting
    \ the endpoint process
    method init-cyw43-net-no-handler ( self -- )

    \ Set country - must be set prior to executing init-cyw43-net, and if not
    \ set defaults to an abbreviation and code of XX\x00\x00 and a revision of
    \ -1
    method cyw43-net-country!
    ( abbrev-addr abbrev-bytes code-addr code-bytes country-rev self -- )

      \ Set a GPIO on the CYW43439
    method cyw43-gpio! ( state gpio self -- ) 

    \ Get the CYW43439 controller
    method cyw43-control@ ( self -- control )
    
    \ Get the zeptoIP interface
    method net-interface@ ( self -- interface )

    \ Get the zeptoIP endpoint processor
    method net-endpoint-process@ ( self -- endpoint-processor )
    
    \ Run network interface processors
    method run-net-process ( self -- )
    
  end-class

  \ Implement the CYW43439 networking and interface class
  <simple-cyw43-net> begin-implement

    \ The constructor, using a specified PWR pin, DIO pin, CS pin, CLK pin,
    \ PIO instruction base address, PIO state machine index, and PIO instance
    \ (pio::PIO0 or pio::PIO1)
    :noname
      { pwr-pin dio-pin cs-pin clk-pin pio-addr sm-index pio-instance self -- }
      self <object>->new

      default-mac-addr
      cyw43-clm::data cyw43-clm::size cyw43-fw::data cyw43-fw::size
      pwr-pin clk-pin dio-pin cs-pin
      pio-addr sm-index pio-instance
      <cyw43-control> self my-cyw43-control init-object
      false self endpoint-process-started? !
    ; define new

    \ Set country - must be set prior to executing init-cyw43-net, and if not
    \ set defaults to an abbreviation and code of XX\x00\x00 and a revision of
    \ -1
    :noname
      ( abbrev-addr abbrev-bytes code-addr code-bytes country-rev self -- )
      my-cyw43-control cyw43-country!
    ; define cyw43-net-country!

    \ Initialize the CYW43439 network and interface object
    :noname { self -- }
      self init-cyw43-net-no-handler
      self my-interface <endpoint-process> self my-endpoint-process init-object
      true self endpoint-process-started? !
    ; define init-cyw43-net

    \ Initialize the CYW43439 network and interface object without starting
    \ the endpoint process
    :noname { self -- }
      self my-cyw43-control init-cyw43
      self my-cyw43-control cyw43-frame-interface@
      <interface> self my-interface init-object
      self my-cyw43-control cyw43-frame-interface@
      <frame-process> self my-frame-process init-object
      self my-interface <ip-handler> self my-ip-handler init-object
      self my-interface <arp-handler> self my-arp-handler init-object
      self my-ip-handler self my-frame-process add-frame-handler
      self my-arp-handler self my-frame-process add-frame-handler
    ; define init-cyw43-net-no-handler

    \ Set a GPIO on the CYW43439
    :noname ( state gpio self -- )
      my-cyw43-control <cyw43-control>->cyw43-gpio!
    ; define cyw43-gpio!

    \ Get the CYW43439 controller
    :noname ( self -- control )
      my-cyw43-control
    ; define cyw43-control@
    
    \ Get the zeptoIP interface
    :noname ( self -- interface )
      my-interface
    ; define net-interface@

    \ Get the endpoint processor
    :noname ( self -- endpoint-processor )
      dup endpoint-process-started? @ averts x-endpoint-process-not-started
      my-endpoint-process
    ; define net-endpoint-process@

    \ Run network interface processors
    :noname ( self -- )
      dup endpoint-process-started? @ if
        dup my-endpoint-process run-endpoint-process
      then
      my-frame-process run-frame-process
    ; define run-net-process
    
  end-implement
  
end-module
