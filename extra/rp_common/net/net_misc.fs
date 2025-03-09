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

begin-module net-misc

  net-consts import
  net-config import
  armv6m import

  \ Make an IPv4 address
  : make-ipv4-addr ( addr0 addr1 addr2 addr3 -- addr )
    $FF and
    swap $FF and 8 lshift or
    swap $FF and 16 lshift or
    swap $FF and 24 lshift or
  ;

  \ Make a global unicast IPv6 address
  : make-global-unicast-ipv6-addr
    { mac-addr-0 mac-addr-1 prefix-0 prefix-1 prefix-2 prefix-3 prefix-bits }
    ( -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )
    $FFFF_FFFF 128 prefix-bits - 0 min 32 max lshift { mask-0 }
    $FFFF_FFFF 96 prefix-bits - 0 min 32 max lshift { mask-1 }
    $FFFF_FFFF 64 prefix-bits - 0 min 32 max lshift { mask-2 }
    $FFFF_FFFF 32 prefix-bits - 0 min 32 max lshift { mask-3 }
    prefix-0 mask-0 and mac-addr-0 mask-0 bic or ( ipv6-0 )
    prefix-1 mask-1 and mac-addr-1 $FFFF and mask-1 bic or ( ipv6-1 )
    prefix-2 mask-2 and ( ipv6-3 )
    prefix-3 mask-3 and ( ipv6-4 )
  ;

  \ Make a link-local IPv6 address
  : make-link-local-ipv6-addr
    { mac-addr-0 mac-addr-1 -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 }
    mac-addr-0 $FFFFF and $FE000000 or \ ipv6-0
    mac-addr-0 16 rshift $FF00 and $FF or mac-addr-1 16 lshift or
    $02000000 xor \ ipv6-1
    0 \ ipv6-2
    $FE800000 \ ipv6-3
  ;

  \ IPv6 address size
  4 cells constant ipv6-addr-size

  \ Make an IPv6 address
  : make-ipv6-addr
    ( addr0 addr1 addr2 addr3 addr4 addr5 addr6 addr7 )
    ( -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )
    $FFFF and swap $FFFF and 16 lshift or { ipv6-0 }
    $FFFF and swap $FFFF and 16 lshift or { ipv6-1 }
    $FFFF and swap $FFFF and 16 lshift or { ipv6-2 }
    $FFFF and swap $FFFF and 16 lshift or { ipv6-3 }
    ipv6-0 ipv6-1 ipv6-2 ipv6-3
  ;

  \ Is an IPv6 address multicast
  : ipv6-addr-multicast? ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 -- multicast? )
    nip nip nip $FFFF0000 and $FF020000 =
  ;

  \ Is this a multicast MAC address
  : mac-addr-multicast? ( D: mac-addr -- multicast? )
    40 2rshift 1 and 0<>
  ;

  \ Get the solicit link-local multicast address
  : solicit-node-link-local-multicast
    ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 -- ipv6-0' ipv6-1' ipv6-2' ipv6-3' )
    drop drop drop $FFFFFF and $FF000000 or $1 $0 $FF020000
  ;
  
  \ The DHCPv6 link-local multicast address
  : DHCPV6_LINK_LOCAL_MULTICAST $0001002 $0 $0 $FF020000 ;

  \ The all-nodes link-local multicast address
  : ALL_NODES_LINK_LOCAL_MULTICAST $1 $0 $0 $FF020000 ;

  \ The all-routers link-local multicast address
  : ALL_ROUTERS_LINK_LOCAL_MULTICAST $2 $0 $0 $FF020000 ;

  \ IPv6 version traffic flow
  6 28 lshift constant IPV6_VERSION_TRAFFIC_FLOW_CONST
  
  \ Max domain name length
  253 constant max-dns-name-len
  
  \ DNS port
  53 constant dns-port

  \ DNS source port
  65535 constant dns-src-port

  \ The AAAA DNS QTYPE
  $001C constant DNS_QTYPE_AAAA
  
  \ DHCP client port
  68 constant dhcp-client-port

  \ DHCP server port
  67 constant dhcp-server-port

  \ IPv6 DHCP client port
  546 constant ipv6-dhcp-client-port

  \ IPv6 DHCP server port
  547 constant ipv6-dhcp-server-port

  \ DHCP discovery state
  0 constant dhcp-not-discovering
  1 constant dhcp-wait-offer
  2 constant dhcp-wait-ack
  3 constant dhcp-got-nak
  4 constant dhcp-wait-confirm
  5 constant dhcp-discovered
  6 constant dhcp-renewing
  7 constant dhcp-rebinding
  8 constant dhcp-declined

  \ DHCPv6 discovery state
  0 constant dhcpv6-not-discovering
  1 constant dhcpv6-wait-advertise
  2 constant dhcpv6-wait-reply
  3 constant dhcpv6-discovered
  4 constant dhcpv6-renewing
  5 constant dhcpv6-rebinding
  6 constant dhcpv6-declined
  7 constant dhcpv6-wait-info-reply

  \ DHCPv6 options
  1 constant OPTION_CLIENTID
  2 constant OPTION_SERVERID
  3 constant OPTION_IA_NA
  5 constant OPTION_IAADDR
  6 constant OPTION_ORO
  8 constant OPTION_ELAPSED_TIME
  23 constant OPTION_DNS_SERVERS
  82 constant OPTION_SOL_MAX_RT
  
  \ Maximumm IPv6 packet size
  1280 constant max-ipv6-packet-size

  \ The Ethernet header structure
  begin-structure ethernet-header-size
    6 +field ethh-destination-mac
    6 +field ethh-source-mac
    hfield: ethh-ether-type
  end-structure

  \ Some constants
  $0800 constant ETHER_TYPE_IPV4
  $0806 constant ETHER_TYPE_ARP
  $86DD constant ETHER_TYPE_IPV6
  $0001 constant HTYPE_ETHERNET
  $0001 constant OPER_REQUEST
  $0002 constant OPER_REPLY

  \ IP flags
  1 bit constant DF
  2 bit constant MF

  \ IP protocols
  1 constant PROTOCOL_ICMP
  6 constant PROTOCOL_TCP
  17 constant PROTOCOL_UDP
  58 constant PROTOCOL_ICMPV6

  \ ICMP types
  0 constant ICMP_TYPE_ECHO_REPLY
  8 constant ICMP_TYPE_ECHO_REQUEST

  \ ICMPv6 types
  128 constant ICMPV6_TYPE_ECHO_REQUEST
  129 constant ICMPV6_TYPE_ECHO_REPLY
  133 constant ICMPV6_TYPE_ROUTER_SOLICIT
  134 constant ICMPV6_TYPE_ROUTER_ADVERTISE
  135 constant ICMPV6_TYPE_NEIGHBOR_SOLICIT
  136 constant ICMPV6_TYPE_NEIGHBOR_ADVERTISE
  137 constant ICMPV6_TYPE_REDIRECT

  \ ICMPv6 options
  1 constant OPTION_SOURCE_LINK_LAYER_ADDR
  2 constant OPTION_TARGET_LINK_LAYER_ADDR
  3 constant OPTION_PREFIX_INFO

  \ ICMPv6 neighbor adverisement solicited flag
  30 bit constant NEIGHBOR_SOLICITED
  
  \ ICMP codes
  0 constant ICMP_CODE_UNUSED

  \ Endpoint bitmask
  0 bit constant endpoint-active
  1 bit constant endpoint-in-use
  2 bit constant endpoint-pending
  3 bit constant endpoint-enqueued
  4 bit constant endpoint-udp
  5 bit constant endpoint-tcp
  24 constant endpoint-tcp-state-lsb
  $FF endpoint-tcp-state-lsb lshift constant endpoint-tcp-state-mask

  \ TCP options
  0 constant TCP_OPT_END
  1 constant TCP_OPT_NOP
  2 constant TCP_OPT_MSS
  4 constant TCP_OPT_MSS_LEN

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

  \ IPv6 header structure
  begin-structure ipv6-header-size
    field: ipv6-version-traffic-flow
    hfield: ipv6-payload-len
    cfield: ipv6-next-header
    cfield: ipv6-hop-limit
    ipv6-addr-size +field ipv6-src-addr
    ipv6-addr-size +field ipv6-dest-addr
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

  \ ICMPv6 router advertise header structure
  begin-structure icmpv6-ra-header-size
    field: icmpv6-ra-icmp-header
    cfield: icmpv6-ra-cur-hop-limit
    cfield: icmpv6-ra-m-o-reserved
    hfield: icmpv6-ra-router-lifetime
    field: icmpv6-ra-reachable-time
    field: icmpv6-ra-retrans-timer
  end-structure

  \ ICMPv6 router advertise "managed" bit
  $80 constant icmpv6-ra-managed

  \ ICMPv6 router advertise "other stateful configuration" bit
  $40 constant icmpv6-ra-other

  \ ICMPv6 prefix information option structure
  begin-structure icmpv6-prefix-info-opt-size
    cfield: icmpv6-prefix-info-type
    cfield: icmpv6-prefix-info-len
    cfield: icmpv6-prefix-info-prefix-len
    cfield: icmpv6-prefix-info-flags
    field: icmpv6-prefix-info-valid-lifetime
    field: icmpv6-prefix-info-preferred-lifetime
    field: icmpv6-prefix-info-reserved2
    ipv6-addr-size +field icmpv6-prefix-info-prefix
  end-structure

  \ ICMPv6 prefix information "autononmous" bit
  $40 constant icmpv6-prefix-info-autonomous

  \ DNS header structure
  begin-structure dns-header-size
    hfield: dns-ident
    hfield: dns-flags
    hfield: dns-qdcount
    hfield: dns-ancount
    hfield: dns-nscount
    hfield: dns-arcount
  end-structure

  \ DNS question body structure
  begin-structure dns-qbody-size
    hfield: dns-qbody-qtype
    hfield: dns-qbody-qclass
  end-structure

  \ DNS answer body structure
  begin-structure dns-abody-size
    hfield: dns-abody-type
    hfield: dns-abody-class
    field: dns-abody-ttl
    hfield: dns-abody-rdlength
  end-structure

  \ TCP header structure
  begin-structure tcp-header-size
    hfield: tcp-src-port
    hfield: tcp-dest-port
    field: tcp-seq-no
    field: tcp-ack-no
    cfield: tcp-data-offset
    cfield: tcp-flags
    hfield: tcp-window-size
    hfield: tcp-checksum
    hfield: tcp-urgent-ptr
  end-structure

  \ DHCP header structure
  begin-structure dhcp-header-size
    cfield: dhcp-op
    cfield: dhcp-htype
    cfield: dhcp-hlen
    cfield: dhcp-hops
    field: dhcp-xid
    hfield: dhcp-secs
    hfield: dhcp-flags
    field: dhcp-ciaddr
    field: dhcp-yiaddr
    field: dhcp-siaddr
    field: dhcp-giaddr
    16 +field dhcp-chaddr
    192 +field dhcp-filler
    field: dhcp-magic
  end-structure

  \ DHCP magic cookie
  $63825363 constant DHCP_MAGIC_COOKIE

  \ DHCP HTYPE (i.e. Ethernet)
  1 constant DHCP_HTYPE

  \ DHCP HLEN (i.e. Ethernet)
  6 constant DHCP_HLEN

  \ DHCP OP
  1 constant DHCP_OP_CLIENT
  2 constant DHCP_OP_SERVER

  \ DHCP message types
  1 constant DHCPDISCOVER
  2 constant DHCPOFFER
  3 constant DHCPREQUEST
  4 constant DHCPDECLINE
  5 constant DHCPACK
  6 constant DHCPNAK
  7 constant DHCPRELEASE

  \ DHCP option types
  50 constant DHCP_REQ_IPV4_ADDR \ 4 byte payload
  51 constant DHCP_IPV4_ADDR_LEASE_TIME \ 4 byte payload, in seconds
  53 constant DHCP_MESSAGE_TYPE \ 1 byte payload
  54 constant DHCP_SERVER_IPV4_ADDR \ 4 byte payload
  55 constant DHCP_PARAM_REQ_LIST \ At least 1 byte
  255 constant DHCP_END \ 0 bytes
  0 constant DHCP_PAD \ 0 bytes

  \ DHCP services
  1 constant DHCP_SERVICE_SUBNET_MASK
  3 constant DHCP_SERVICE_ROUTER
  6 constant DHCP_SERVICE_DNS_SERVER
  15 constant DHCP_SERVICE_DNS_NAME

  \ DHCP header structure
  begin-structure dhcpv6-header-size
    cfield: dhcpv6-msg-type
    3 +field dhcp6-transact-id
  end-structure

  \ IPv6 DHCP message types
  1 constant DHCPV6_SOLICIT
  2 constant DHCPV6_ADVERTISE
  3 constant DHCPV6_REQUEST
  4 constant DHCPV6_CONFIRM
  5 constant DHCPV6_RENEW
  6 constant DHCPV6_REBIND
  7 constant DHCPV6_REPLY
  8 constant DHCPV6_RELEASE
  9 constant DHCPV6_DECLINE
  11 constant DHCPV6_INFORMATION_REQUEST

  \ DUID_LL type
  3 constant DUID_LL

  \ Minimum DUID size
  4 constant min-duid-size
  
  \ Maximum DUID size
  132 constant max-duid-size
  
  \ Default requested IP address
  0 0 0 0 make-ipv4-addr constant DEFAULT_IPV4_ADDR
\  192 168 1 100 make-ipv4-addr constant DEFAULT_IPV4_ADDR
  
  \ DNS flags
  15 bit constant DNS_QR_RESPONSE
  11 constant DNS_OPCODE_LSB
  $F DNS_OPCODE_LSB lshift constant DNS_OPCODE_MASK
  10 bit constant DNS_AA
  9 bit constant DNS_TC
  8 bit constant DNS_RD
  7 bit constant DNS_RA
  0 constant DNS_RCODE_LSB
  $F DNS_RCODE_LSB lshift constant DNS_RCODE_MASK

  \ DNS response codes
  0 constant DNS_NO_ERROR
  1 constant DNS_FORMAT_ERROR
  2 constant DNS_SERVER_FAILURE
  3 constant DNS_NAME_ERROR
  4 constant DNS_NOT_IMPLEMENTED
  5 constant DNS_REFUSED

  \ TCP flags
  7 bit constant TCP_CWR
  6 bit constant TCP_ECE
  5 bit constant TCP_URG
  4 bit constant TCP_ACK
  3 bit constant TCP_PSH
  2 bit constant TCP_RST
  1 bit constant TCP_SYN
  0 bit constant TCP_FIN
  TCP_ACK TCP_SYN or TCP_FIN or TCP_RST or constant TCP_CONTROL

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
  : rev ( x -- x' ) [inlined] code[ r6 r6 rev_,_ ]code ;

  \ Reverse byte order in a 16-bit value
  : rev16 ( h -- h' ) [inlined] code[ r6 r6 rev16_,_ ]code ;

  \ Reverse byte order in a 128-bit value
  : rev128 ( x0 x1 x2 x3 -- x0' x1' x2' x3 )
    code[
    r6 r6 rev_,_
    0 r7 r0 ldr_,[_,#_]
    r0 r0 rev_,_
    0 r7 r0 str_,[_,#_]
    4 r7 r0 ldr_,[_,#_]
    r0 r0 rev_,_
    4 r7 r0 str_,[_,#_]
    8 r7 r0 ldr_,[_,#_]
    r0 r0 rev_,_
    8 r7 r0 str_,[_,#_]
    12 r7 r0 ldr_,[_,#_]
    r0 r0 rev_,_
    12 r7 r0 str_,[_,#_]
    ]code
  ;

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
  thumb-2? not [if]
    : unaligned@ ( addr -- x )
      [inlined]
      code[
      3 tos r0 ldrb_,[_,#_]
      2 tos r1 ldrb_,[_,#_]
      8 r0 r0 lsls_,_,#_
      r1 r0 orrs_,_
      1 tos r1 ldrb_,[_,#_]
      8 r0 r0 lsls_,_,#_
      r1 r0 orrs_,_
      0 tos r1 ldrb_,[_,#_]
      8 r0 tos lsls_,_,#_
      r1 tos orrs_,_
      ]code
    ;
  [else]
    : unaligned@ ( addr -- x )
      [inlined]
      code[
      0 tos tos ldr_,[_,#_]
      ]code
    ;
  [then]

  \ Do an alignment-safe 32-bit store
  thumb-2? not [if]
    : unaligned! ( x addr -- )
      [inlined]
      code[
      r1 1 dp ldm
      0 tos r1 strb_,[_,#_]
      8 r1 r1 lsrs_,_,#_
      1 tos r1 strb_,[_,#_]
      8 r1 r1 lsrs_,_,#_
      2 tos r1 strb_,[_,#_]
      8 r1 r1 lsrs_,_,#_
      3 tos r1 strb_,[_,#_]
      tos 1 dp ldm
      ]code
    ;
  [else]
    : unaligned! ( x addr -- )
      [inlined]
      code[
      r1 1 dp ldm
      0 tos r1 str_,[_,#_]
      tos 1 dp ldm
      ]code
    ;
  [then]

  \ Do an alignment-safe 16-bit load
  thumb-2? not [if]
    : hunaligned@ ( addr -- h )
      [inlined]
      code[
      1 tos r0 ldrb_,[_,#_]
      0 tos r1 ldrb_,[_,#_]
      8 r0 tos lsls_,_,#_
      r1 tos orrs_,_
      ]code
    ;
  [else]
    : hunaligned@ ( addr -- h )
      [inlined]
      code[
      0 tos tos ldrh_,[_,#_]
      ]code
    ;
  [then]

  \ Do an alignment-safe 16-bit store
  thumb-2? not [if]
    : hunaligned! ( h addr -- )
      [inlined]
      code[
      r1 1 dp ldm
      0 tos r1 strb_,[_,#_]
      8 r1 r1 lsrs_,_,#_
      1 tos r1 strb_,[_,#_]
      tos 1 dp ldm
      ]code
    ;
  [else]
    : hunaligned! ( h addr -- )
      [inlined]
      code[
      r1 1 dp ldm
      0 tos r1 strh_,[_,#_]
      tos 1 dp ldm
      ]code
    ;
  [then]

  \ Do an alignment-safe IPv6 address load
  : ipv6-unaligned@ { addr -- x0 x1 x2 x3 }
    addr [ 3 cells ] literal + unaligned@
    addr [ 2 cells ] literal + unaligned@
    addr cell + unaligned@
    addr unaligned@
    rev128
  ;

  \ Do an alignment-safe IPv6 address store
  : ipv6-unaligned! { x0 x1 x2 x3 addr -- }
    x3 addr unaligned!
    x2 addr cell + unaligned!
    x1 addr [ 2 cells ] literal + unaligned!
    x0 addr [ 3 cells ] literal + unaligned!
    rev128
  ;

  \ Compare two IPv6 addresses
  : ipv6= { x0-0 x0-1 x0-2 x0-3 x1-0 x1-1 x1-2 x1-3 -- equal? }
    x0-0 x1-0 = x0-1 x1-1 = and x0-2 x1-2 = and x0-3 x1-3 = and
  ;
  
  \ Print a MAC address
  : mac. { D: addr -- }
    0 5 ?do
      addr i 0 ?do 256. d/ loop drop $FF and h.2 i if ." :" then
    -1 +loop
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
  
  \ Compute a checksum
  : compute-checksum ( start ) { addr bytes zero-offset -- h }
    bytes 1 bic 0 ?do
      i zero-offset <> if
        addr i + hunaligned@ rev16 + dup 16 rshift + $FFFF and
      then
    2 +loop
    bytes 1 and if
      addr bytes 1- + c@ 8 lshift + dup 16 rshift + $FFFF and
    then
    not $FFFF and
  ;
  
  \ Compute an Internet header checksum
  : compute-inet-checksum { addr bytes zero-offset -- h }
    0 addr bytes zero-offset compute-checksum
  ;

  \ Compute a TCP checksum
  : compute-tcp-checksum { src-addr dest-addr addr bytes zero-offset -- h }
    src-addr 16 rshift
    src-addr $FFFF and + dup 16 rshift + $FFFF and
    dest-addr 16 rshift + dup 16 rshift + $FFFF and
    dest-addr $FFFF and + dup 16 rshift + $FFFF and
    PROTOCOL_TCP + dup 16 rshift + $FFFF and
    bytes + dup 16 rshift + $FFFF and
    addr bytes zero-offset compute-checksum
  ;

  \ Compute an IPv6 TCP checksum
  : compute-ipv6-tcp-checksum
    { zero-offset }
    { src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 addr bytes }
    ( -- h )
    src-3 16 rshift
    src-3 $FFFF and + dup 16 rshift + $FFFF and
    src-2 16 rshift + dup 16 rshift + $FFFF and
    src-2 $FFFF and + dup 16 rshift + $FFFF and
    src-1 16 rshift + dup 16 rshift + $FFFF and
    src-1 $FFFF and + dup 16 rshift + $FFFF and
    src-0 16 rshift + dup 16 rshift + $FFFF and
    src-0 $FFFF and + dup 16 rshift + $FFFF and
    dest-3 16 rshift + dup 16 rshift + $FFFF and
    dest-3 $FFFF and + dup 16 rshift + $FFFF and
    dest-2 16 rshift + dup 16 rshift + $FFFF and
    dest-2 $FFFF and + dup 16 rshift + $FFFF and
    dest-1 16 rshift + dup 16 rshift + $FFFF and
    dest-1 $FFFF and + dup 16 rshift + $FFFF and
    dest-0 16 rshift + dup 16 rshift + $FFFF and
    dest-0 $FFFF and + dup 16 rshift + $FFFF and
    bytes + dup 16 rshift + $FFFF and
    PROTOCOL_TCP + dup 16 rshift + $FFFF and
    addr bytes zero-offset compute-checksum
  ;

  : compute-ipv6-udp-checksum
    { zero-offset }
    { src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 addr bytes }
    ( -- h )
    src-3 16 rshift
    src-3 $FFFF and + dup 16 rshift + $FFFF and
    src-2 16 rshift + dup 16 rshift + $FFFF and
    src-2 $FFFF and + dup 16 rshift + $FFFF and
    src-1 16 rshift + dup 16 rshift + $FFFF and
    src-1 $FFFF and + dup 16 rshift + $FFFF and
    src-0 16 rshift + dup 16 rshift + $FFFF and
    src-0 $FFFF and + dup 16 rshift + $FFFF and
    dest-3 16 rshift + dup 16 rshift + $FFFF and
    dest-3 $FFFF and + dup 16 rshift + $FFFF and
    dest-2 16 rshift + dup 16 rshift + $FFFF and
    dest-2 $FFFF and + dup 16 rshift + $FFFF and
    dest-1 16 rshift + dup 16 rshift + $FFFF and
    dest-1 $FFFF and + dup 16 rshift + $FFFF and
    dest-0 16 rshift + dup 16 rshift + $FFFF and
    dest-0 $FFFF and + dup 16 rshift + $FFFF and
    bytes + dup 16 rshift + $FFFF and
    PROTOCOL_UDP + dup 16 rshift + $FFFF and
    addr bytes zero-offset compute-checksum
  ;

  \ Get whether an IPv4 packet is fragmented
  : ipv4-fragment? ( addr -- fragmented? )
    ipv4-flags-fragment-offset h@ rev16
    dup $1FFF and { offset }
    13 rshift { flags }
    flags MF and 0<> offset 0<> or
  ;

  \ IPv6 extensions
  0 constant ipv6-ext-hop-by-hop-options
  43 constant ipv6-ext-routing
  44 constant ipv6-ext-fragment

  \ Hop by hop extension header
  begin-structure ipv6-ext-hop-by-hop-size
    cfield: ipv6-ext-hop-by-hop-next-header
    cfield: ipv6-ext-hop-by-hop-len
    6 +field ipv6-ext-hop-by-hop-pad
  end-structure
  
  \ Routing extension header
  begin-structure ipv6-ext-routing-size
    cfield: ipv6-ext-routing-next-header
    cfield: ipv6-ext-routing-len
    6 +field ipv6-ext-routing-pad
  end-structure

  \ Fragment extension header
  begin-structure ipv6-ext-fragment-size
    cfield: ipv6-ext-fragment-next-header
    cfield: ipv6-ext-fragment-reserved
    hfield: ipv6-ext-fragment-offset
    field: ipv6-ext-fragment-ident
  end-structure

  \ Find the final payload of an IPv6 packet
  : find-ipv6-payload { addr bytes -- header-type addr bytes valid? }
    bytes ipv6-header-size > if
      addr ipv6-next-header c@ { next-header }
      ipv6-header-size +to addr
      [ ipv6-header-size negate ] literal +to bytes
      begin
        next-header case
          ipv6-ext-hop-by-hop-options of
            bytes ipv6-ext-hop-by-hop-size >= if
              addr ipv6-ext-hop-by-hop-next-header c@ to next-header
              addr ipv6-ext-hop-by-hop-len c@ dup +to addr negate +to bytes
            else
              next-header addr bytes false exit
            then
          endof
          ipv6-ext-routing of
            bytes ipv6-ext-routing-size >= if
              addr ipv6-ext-routing-next-header c@ to next-header
              addr ipv6-ext-routing-len c@ dup +to addr negate +to bytes
            else
              next-header addr bytes false exit
            then
          endof
          ipv6-ext-fragment of
            addr bytes bytes ipv6-ext-fragment-size >= if
              addr ipv6-ext-fragment-next-header c@ to next-header
              ipv6-ext-fragment-size +to addr
              [ ipv6-ext-fragment-size negate ] literal +to bytes
            else
              next-header addr bytes false exit
            then
          endof
          addr bytes true exit
        endcase
      again
    else
      addr bytes false
    then
  ;

  \ Find an IPv6 fragment header
  : find-ipv6-fragment { addr bytes -- addr bytes fragmented? }
    bytes ipv6-header-size > if
      addr ipv6-next-header c@ { next-header }
      ipv6-header-size +to addr
      [ ipv6-header-size negate ] literal +to bytes
      begin
        next-header case
          ipv6-ext-hop-by-hop-options of
            bytes ipv6-ext-hop-by-hop-size >= if
              addr ipv6-ext-hop-by-hop-next-header c@ to next-header
              addr ipv6-ext-hop-by-hop-len c@ dup +to addr negate +to bytes
            else
              addr bytes false exit
            then
          endof
          ipv6-ext-routing of
            bytes ipv6-ext-routing-size >= if
              addr ipv6-ext-routing-next-header c@ to next-header
              addr ipv6-ext-routing-len c@ dup +to addr negate +to bytes
            else
              addr bytes false exit
            then
          endof
          ipv6-ext-fragment of
            addr bytes bytes ipv6-ext-fragment-size >= exit
          endof
          drop addr bytes false exit
        endcase
      again
    else
      addr bytes false
    then
  ;

  \ Load a MAC address as a double
  : mac@ { addr -- D: mac-addr }
    addr 2 + unaligned@ rev addr h@ rev16
  ;

  \ Store a double as a MAC address
  : mac! { D: mac-addr addr -- }
    mac-addr rev16 addr h! rev addr 2 + unaligned!
  ;

  \ Get the length of a DNS-formatted name
  : dns-name-size ( bytes -- bytes' ) ?dup if 2 + else 1 then ;

  \ Get the offset of the next dot
  : next-dot-offset { name-addr name-bytes -- offset | 0 }
    name-bytes 0 ?do name-addr i + c@ [char] . = if i unloop exit then loop 0
  ;
  
  \ Format a DNS name
  : format-dns-name { name-addr name-bytes dest-addr -- }
    begin
      name-bytes 0> if
        name-addr name-bytes next-dot-offset ?dup if
          dup dest-addr c!
          1 +to dest-addr
          name-addr dest-addr 2 pick move
          dup +to dest-addr
          1+ dup +to name-addr
          negate +to name-bytes
        else
          name-bytes dest-addr c!
          1 +to dest-addr
          name-addr dest-addr name-bytes move
          name-bytes +to dest-addr
          0 to name-bytes
        then
        false
      else
        0 dest-addr c! true
      then
    until
  ;

  \ Skip a DNS name
  : skip-dns-name { addr bytes -- addr' bytes' }
    begin
      bytes 0> if
        addr c@ $C0 and $C0 = if
          2 +to addr -2 +to bytes true
        else
          addr c@ ?dup if
            1+ dup +to addr negate +to bytes false
          else
            1 +to addr -1 +to bytes true
          then
        then
      else
        true
      then
    until
    addr bytes
  ;

  \ Parse a DNS name
  : parse-dns-name { addr bytes all-addr all-bytes buf -- len success? }
    0 { level }
    0 { offset }
    begin
      addr all-addr u>=
      addr all-addr all-bytes + u< and
      bytes all-bytes u< and
      offset max-dns-name-len < and if
        addr c@ { part-len }
        part-len $C0 and $C0 = if
          level parse-dns-name-depth = if 0 false exit then
          addr hunaligned@ rev16 $C000 bic
          dup all-bytes u>= if drop 0 false exit then
          all-bytes over - to bytes
          all-addr + to addr
          1 +to level
        else
          part-len 63 u>
          part-len offset + 1+ max-dns-name-len u> or if 0 false exit then
          part-len 0= if
            offset true exit
          then
          offset 0<> if
            [char] . buf offset + c!
            1 +to offset
          then
          1 +to addr
          -1 +to bytes
          part-len bytes u>= if 0 false exit then
          addr buf offset + part-len move
          part-len +to addr
          part-len negate +to bytes
          part-len +to offset
        then
      else
        0 false exit
      then
    again
  ;

  \ Get full TCP header size
  : full-tcp-header-size ( addr -- size ) tcp-data-offset c@ 2 rshift ;

  \ Get TCP data
  : tcp-data ( addr bytes -- addr' bytes' )
    over full-tcp-header-size dup { header-size } - swap header-size + swap
  ;

  \ Try to get MSS field from an TCP packet
  : tcp-mss@ { addr bytes -- mss found? }
    addr full-tcp-header-size dup { size } 5 cells > size bytes <= and if
      0 false { mss found? }
      addr 5 cells + { current-addr }
      begin current-addr addr bytes + < while
        current-addr c@ case
          TCP_OPT_END of mss found? exit endof
          TCP_OPT_NOP of 1 +to current-addr endof
          TCP_OPT_MSS of
            current-addr 4 + addr bytes + > if 0 false exit then
            1 +to current-addr
            current-addr c@ TCP_OPT_MSS_LEN <> if 0 false exit then
            1 +to current-addr
            current-addr hunaligned@ rev16 to mss true to found?
            2 +to current-addr
          endof
          current-addr 2 + addr bytes + > if 0 false exit then
          current-addr 1+ c@
          dup current-addr + addr bytes + > if drop 0 false exit then
          dup 2 < if drop 0 false exit then
          +to current-addr
        endcase
      repeat
      current-addr addr bytes + = if mss found? else 0 false then
    else
      0 false
    then
  ;
  
  \ Print a TCP packet
  : tcp. { addr -- }
    cr ." tcp-src-port: " addr tcp-src-port hunaligned@ rev16 .
    cr ." tcp-dest-port: " addr tcp-dest-port hunaligned@ rev16 .
    cr ." tcp-seq-no: " addr tcp-seq-no unaligned@ rev .
    cr ." tcp-ack-no: " addr tcp-ack-no unaligned@ rev .
    cr ." tcp-data-offset: " addr tcp-data-offset c@ dup .
    ." (" 2 rshift . ." bytes)"
    cr ." tcp-flags: " addr tcp-flags c@ dup .
    dup TCP_CWR and if ." CWR " then
    dup TCP_ECE and if ." ECE " then
    dup TCP_URG and if ." URG " then
    dup TCP_ACK and if ." ACK " then
    dup TCP_PSH and if ." PSH " then
    dup TCP_RST and if ." RST " then
    dup TCP_SYN and if ." SYN " then
    TCP_FIN and if ." FIN " then
    cr ." tcp-window-size: " addr tcp-window-size hunaligned@ rev16 .
    cr ." tcp-checksum: " addr tcp-checksum hunaligned@ rev16 h.4
    cr ." tcp-urgent-ptr: " addr tcp-urgent-ptr hunaligned@ rev16 .
  ;

  \ The current ephemeral port
  variable current-ephemeral-port

  \ Ephemeral port lock
  lock::lock-size aligned-buffer: ephemeral-port-lock
  
  \ Get an ephemeral port
  : get-ephemeral-port ( -- port )
    [:
      current-ephemeral-port @
      dup 1+ MIN_EPHEMERAL_PORT -
      [ MAX_EPHEMERAL_PORT MIN_EPHEMERAL_PORT - ] literal umod
      MIN_EPHEMERAL_PORT + current-ephemeral-port !
    ;] ephemeral-port-lock lock::with-lock
  ;

  \ Initialize ephemeral ports
  : init-ephemeral-ports ( -- )
    ephemeral-port-lock lock::init-lock
    rng::random [ MAX_EPHEMERAL_PORT MIN_EPHEMERAL_PORT - ] literal umod
    MIN_EPHEMERAL_PORT + current-ephemeral-port !
  ;

  \ Get a fixed size DHCP option
  : find-fixed-dhcp-opt { opt len addr bytes -- addr' found? }
    dhcp-header-size +to addr
    [ dhcp-header-size negate ] literal +to bytes
    begin
      bytes 0> if
        addr c@ DHCP_END = if
          0 false true
        else
          addr c@ DHCP_PAD = if
            1 +to addr
            -1 +to bytes
            false
          else
            bytes 1 > if
              addr 1+ c@ { len' }
              bytes len' 2 + < if 0 false true then
              addr c@ opt = if
                len len' = if addr 2 + true true else 0 false true then
              else
                len' 2 + +to addr
                len' 2 + negate +to bytes
                false
              then
            else
              0 false true
            then
          then
        then
      then
    until
  ;

  \ Get a variable size DHCP option
  : find-var-dhcp-opt { opt addr bytes -- addr' len found? }
    dhcp-header-size +to addr
    [ dhcp-header-size negate ] literal +to bytes
    begin
      bytes 0> if
        addr c@ DHCP_END = if
          0 0 false true
        else
          addr c@ DHCP_PAD = if
            1 +to addr
            -1 +to bytes
            false
          else
            bytes 1 > if
              addr 1+ c@ { len' }
              bytes len' 2 + < if 0 0 false true then
              addr c@ opt = if
                addr 2 + len' true true
              else
                len' 2 + +to addr
                len' 2 + negate +to bytes
                false
              then
            else
              0 0 false true
            then
          then
        then
      then
    until
  ;

  \ Get a variable size DHCPv6 option
  : find-dhcpv6-opt { opt addr bytes -- addr' len found? }
    begin bytes 4 >= while
      addr 2 + hunaligned@ rev16 { len }
      addr hunaligned@ rev16 opt = if
        bytes 4 - len >= if
          addr 4 + len true exit
        else
          0 0 false exit
        then
      else
        len 4 + dup +to addr negate +to bytes
      then
    repeat
    0 0 false
  ;

  \ Get a variable size ICMPv6 option
  : find-icmpv6-opt { opt addr bytes -- addr' len found? }
    begin bytes 2 >= while
      addr 1+ c@ 8 * { len }
      addr c@ opt =  if
        bytes len >= if
          addr 2 + len 2 - true exit
        else
          0 0 false exit
        then
      else
        len dup +to addr negate +to bytes
      then
    repeat
    0 0 false
  ;

end-module

\ Initialize
: init ( -- )
  init
  net-misc::init-ephemeral-ports
;