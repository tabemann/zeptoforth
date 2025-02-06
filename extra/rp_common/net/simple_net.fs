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
\
\ This is the base class for the "simple network interface" object.  It is
\ meant to be subclassed with an actual device driver class.

begin-module simple-net

  oo import
  net-misc import
  frame-process import
  net import
  endpoint-process import

  \ Endpoint process has not been started exception
  : x-endpoint-process-not-started ( -- ) ." endpoint process not started" cr ;
  
  \ A simple networking and interface base class
  <object> begin-class <simple-net>

    begin-module simple-net-internal

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

    \ Initialize a networking and interface class instance
    method init-net ( self -- )

    \ Initialize the network and interface object without starting
    \ the endpoint process
    method init-net-no-handler ( self -- )

    \ Get the controller (an instance of the actual device frame-interface,
    \ to be supplied by the subclass)
    method device-frame-interface@ ( self -- frame-interface )
    
    \ Get the zeptoIP interface
    method net-interface@ ( self -- interface )

    \ Get the interface frame processor
    method net-frame-process@ ( self -- frame-processor )
    
    \ Get the zeptoIP endpoint processor
    method net-endpoint-process@ ( self -- endpoint-processor )
    
    \ Run network interface processors
    method run-net-process ( self -- )
    
  end-class

  \ Implement the common networking and interface class
  <simple-net> begin-implement

    \ Constructor.  
    :noname
      { self -- )
      self <object>->new

      false self endpoint-process-started? !
    ; define new

    \ Initialize the network and interface object
    :noname { self -- }
      self init-net-no-handler
      self my-interface <endpoint-process> self my-endpoint-process init-object
      true self endpoint-process-started? !
    ; define init-net

    \ Initialize the network and interface object without starting
    \ the endpoint process
    :noname { self -- }
      self device-frame-interface@ <interface> self my-interface init-object
      self device-frame-interface@ <frame-process> self my-frame-process init-object
      self my-interface <arp-handler> self my-arp-handler init-object
      self my-interface <ip-handler> self my-ip-handler init-object
      self my-arp-handler self my-frame-process add-frame-handler
      self my-ip-handler self my-frame-process add-frame-handler
    ; define init-net-no-handler

    \ Get the zeptoIP interface
    :noname ( self -- interface )
      my-interface
    ; define net-interface@

    \ Get the frame process
    :noname ( self -- frame-processor )
      my-frame-process
    ; define net-frame-process@
    
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
