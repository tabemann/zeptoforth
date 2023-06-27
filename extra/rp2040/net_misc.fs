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

  \ The maximum fragment buffer size
  1500 constant fragment-buf-size

  \ The maximum IP payload size
  1500 constant max-payload-size

  \ The maximum fragment unit count
  fragment-buf-size 8 align 8 /  constant max-fragment-units

  \ Maximum endpoint count
  4 constant max-endpoints

  \ Maximum number of addresses to map
  16 constant max-addresses

  \ Maximum resolution attempts
  10 constant max-resolve-attempts

  \ The Ethernet header structure
  begin-structure ethernet-header-size
    6 +field ethh-destination-mac
    6 +field ethh-source-mac
    hfield: ethh-ether-type
  end-structure

  \ Some constants
  $0800 constant ETHER_TYPE_IPV4
  $0806 constant ETHER_TYPE_ARP
  $0001 constant HTYPE_ETHERNET
  $0001 constant OPER_REQUEST
  $0002 constant OPER_REPLY

  \ IP flags
  1 constant DF
  2 constant MF

  \ IP protocols
  6 constant PROTOCOL_TCP
  17 constant PROTOCOL_UDP

  \ ICMP types
  0 constant ICMP_TYPE_ECHO_REPLY
  8 constant ICMP_TYPE_ECHO_REQUEST

  \ ICMP codes
  0 constant ICMP_CODE_UNUSED

  \ Endpoint bitmask
  0 bit constant endpoint-active
  1 bit constant endpoint-connected
  2 bit constant endpoint-rx-pending
  3 bit constant endpoint-udp

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

  \ UDP header structure
  begin-structure udp-header-size
    hfield: udp-src-port
    hfield: udp-dest-port
    hfield: udp-total-len
    hfield: udp-checksum
  end-structure

  \ ICMP header structure
  begin-structure icmp-header-size
    cfield: icmp-type
    cfield: icmp-code
    hfield: icmp-checksum
    field: icmp-rest-of-header
  end-structure

  \ Get ICMP size for IPv4 packet
  : icmp-error-size { addr bytes -- size }
    addr ipv4-version-ihl c@ $F and 4 * { head-copy-size }
    bytes head-copy-size > if
      bytes head-copy-size - 8 min head-copy-size +
    else
      head-copy-size
    then
    icmp-header-size +
  ;

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

  \ Compute an Internet header checksum
  : compute-inet-checksum ( addr bytes zero-offset -- h )
    over [: { addr bytes zero-offset buf }
      addr buf bytes move
      0 buf zero-offset + h!
      0 buf bytes + buf ?do
        i h@ rev16 + dup 16 rshift + $FFFF and
      2 +loop
      not $FFFF and
    ;] with-aligned-allot
  ;

  \ Make an IPv4 address
  : make-ipv4-addr ( addr0 addr1 addr2 addr3 -- addr )
    $FF and
    swap $FF and 8 lshift or
    swap $FF and 16 lshift or
    swap $FF and 24 lshift or
  ;

  \ Get whether an IPv4 packet is fragmented
  : ipv4-fragment? ( addr -- fragmented? )
    ipv4-flags-fragment-offset h@ rev16
    dup $1FFF and { offset }
    13 rshift { flags }
    flags MF and 0<> offset 0<> or
  ;

  \ Load a MAC address as a double
  : mac@ { addr -- D: mac-addr }
    addr 2 + unaligned@ rev addr h@ rev16
  ;

  \ Store a double as a MAC address
  : mac! { D: mac-addr addr -- }
    mac-addr rev16 addr h! rev addr 2 + unaligned!
  ;
  
end-module