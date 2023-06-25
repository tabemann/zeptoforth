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

begin-module net-misc

  armv6m import
  
  \ The MTU size
  1500 constant mtu-size

  \ The Ethernet header structure
  begin-structure ethernet-header-size
    6 +field ethh-destination-mac
    6 +field ethh-source-mac
    hfield: ethh-ether-type
  end-structure

  \ Some Ethernet constants
  $0800 constant ETHER_TYPE_IPV4
  $0806 constant ETHER_TYPE_ARP
  $0001 constant HTYPE_ETHERNET
  $0001 constant OPER_REQUEST
  $0002 constant OPER_REPLY

  \ IPv4 ARP header structure
  begin-structure arp-ipv4-size
    hfield: arp-htype
    hfield: arp-ptype
    cfield: arp-hlen
    cfield: arp-plen
    hfield: arp-oper
    6 +field arp-sha
    4 +field arp-spa
    6 +field arp-tha
    4 +field arp-tpa
  end-structure

  \ IPv4 header structure
  begin-structure ipv4-header-size
    cfield: ipv4-version-ihl
    cfield: ipv4-tos
    hfield: ipv4-total-len
    hfield: ipv4-identification
    hfield: ipv4-flags-fragment-offset
    cfield: ipv4-ttl
    cfield: ipv4-protocol
    hfield: ipv4-header-checksum
    field: ipv4-src-addr
    field: ipv4-dest-addr
  end-structure

  \ Reverse byte order in a 32-bit value
  : rev ( x -- x' ) code[ r6 r6 rev_,_ ]code ;

  \ Reverse byte order in a 16-bit value
  : rev16 ( h -- h' ) code[ r6 r6 rev16_,_ ]code ;

  \ Get whether two MAC addresses are the same
  : mac= { addr0 addr1 -- equal? }
    addr0 c@ addr1 c@ =
    addr0 1+ c@ addr1 1+ c@ = and
    addr0 2 + c@ addr1 2 + c@ = and
    addr0 3 + c@ addr1 3 + c@ = and
    addr0 4 + c@ addr1 4 + c@ = and
    addr0 5 + c@ addr1 5 + c@ = and
  ;

  \ Do an alignment-safe 32-bit load
  : unaligned@ { addr -- x }
    addr c@
    addr 1+ c@ 8 lshift or
    addr 2 + c@ 16 lshift or
    addr 3 + c@ 24 lshift or
  ;

  \ Do an alignment-safe 32-bit store
  : unaligned! { x addr -- }
    x addr c!
    x 8 rshift addr 1+ c!
    x 16 rshift addr 2 + c!
    x 24 rshift addr 3 + c!
  ;

  \ Print a MAC address
  : mac. { addr -- }
    6 0 ?do addr i + c@ h.2 i 5 <> if ." :" then loop
  ;

  \ Print an IPv4 address
  : ipv4. { ip -- }
    0 24 ?do ip i rshift $FF and (.) i 0 <> if ." ." then -8 +loop
  ;

  \ Strip an Ethernet header
  : strip-ethernet-header { addr bytes -- addr' bytes' }
    addr cyw43-structs::ethernet-header-size +
    bytes cyw43-structs::ethernet-header-size - 0 max
  ;

  \ Compute an IPv4 header checksum
  : compute-ipv4-header-checksum ( addr -- h )
    60 [: { addr buf }
      addr ipv4-version-ihl c@ $F and { ihl }
      addr buf ihl cells move
      00 buf ipv4-header-checksum h!
      0 buf ihl cells + buf ?do
        i h@ rev16 + dup 16 rshift + $FFFF and
      2 +loop
      not $FFFF and
    ;] with-aligned-allot
  ;
  
end-module