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

begin-module net

  oo import
  cyw43-control import
  net-process import
  net-misc import
  lock import

  \ The interface class
  <object> begin-class <interface>

    \ The control
    cell member intf-control
    
    \ The IPv4 address
    cell member ipv4-addr

    \ Outgoing buffer lock
    lock-size member outgoing-buf-lock

    \ The outgoing frame buffer
    mtu-size cell align member outgoing-buf

    \ Get the IPv4 address
    method ipv4-addr@ ( self -- addr )
    
    \ Set the IPv4 address
    method ipv4-addr! ( addr self -- )

    \ Get the MAC address
    method mac-addr@ ( self -- addr )

    \ Send a frame
    method send-frame ( addr bytes self -- )

    \ Use outgoing buffer
    method with-outgoing-buf ( xt self -- ) ( xt: ? buf -- ? )
    
  end-class

  \ Implement the interface class
  <interface> begin-implement

    \ Constructor
    :noname { control self -- }
      self <object>->new
      control self intf-control !
      0 self ipv4-addr !
      self outgoing-buf-lock init-lock
    ; define new

    \ Get the IPv4 address
    :noname ( self -- addr )
      ipv4-addr @
    ; define ipv4-addr@
    
    \ Set the IPv4 address
    :noname ( addr self -- )
      ipv4-addr !
    ; define ipv4-addr!

    \ Get the MAC address
    :noname ( self -- addr )
      intf-control @ cyw43-mac-addr
    ; define mac-addr@

    \ Send a frame
    :noname ( addr bytes self -- )
      intf-control @ put-cyw43-tx
    ; define send-frame

    \ Use outgoing buffer
    :noname ( xt self -- ) ( xt: ? buf -- ? )
      dup outgoing-buf -rot outgoing-buf-lock with-lock
    ; define with-outgoing-buf

  end-implement

  \ The ARP packet handler
  <net-handler> begin-class <arp-handler>

    \ The ARP IP interface
    cell member arp-interface

    \ Send an ARP response
    method send-arp-response ( addr self -- )
    
  end-class
  
  \ Implement the ARP packet handler
  <arp-handler> begin-implement
    
    \ Constructor
    :noname { ip self -- }
      self <net-handler>->new
      ip self arp-interface !
    ; define new

    \ Handle a frame
    :noname { addr bytes self -- }

      addr ethh-ether-type h@ [ ETHER_TYPE_ARP rev16 ] literal = if
        ethernet-header-size +to addr
        [ ethernet-header-size negate ] literal +to bytes
        bytes arp-ipv4-size >= if
          addr arp-htype h@ [ HTYPE_ETHERNET rev16 ] literal =
          addr arp-ptype h@ [ ETHER_TYPE_IPV4 rev16 ] literal = and
          addr arp-hlen c@ 6 = and
          addr arp-plen c@ 4 = and
          addr arp-oper h@ [ OPER_REQUEST rev16 ] literal = and if
            addr arp-tpa unaligned@ rev self arp-interface @ ipv4-addr@ = if
              addr self send-arp-response
            then
          then
        then
      then
    ; define handle-net-frame

    \ Send an ARP response
    :noname ( addr self -- )
      [: { addr self buf }
        addr arp-sha buf ethh-destination-mac 6 move
        self arp-interface @ mac-addr@ buf ethh-source-mac 6 move
        [ ETHER_TYPE_ARP rev ] literal buf ethh-ether-type !
        buf ethernet-header-size + { arp-buf }
        [ HTYPE_ETHERNET rev16 ] literal arp-buf arp-htype h!
        [ ETHER_TYPE_IPV4 rev16 ] literal arp-buf arp-ptype h!
        6 arp-buf arp-hlen c!
        4 arp-buf arp-plen c!
        [ OPER_REPLY rev16 ] literal arp-buf arp-oper h!
        self arp-interface @ mac-addr@ arp-buf arp-sha 6 move
        self arp-interface @ ipv4-addr@ rev arp-buf arp-spa !
        addr arp-sha arp-buf arp-tha 6 move
        addr arp-spa @ arp-buf arp-tpa !
        buf [ ethernet-header-size arp-ipv4-size + ] literal
        self arp-interface @ send-frame
      ;] over arp-interface @ with-outgoing-buf
    ; define send-arp-response
    
  end-implement
  
end-module