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

  \ Are we debugging?
  false constant debug?

  \ DHCP logging
  true value dhcp-log?
  
  \ Make an IPv4 address
  : make-ipv4-addr ( addr0 addr1 addr2 addr3 -- addr )
    $FF and
    swap $FF and 8 lshift or
    swap $FF and 16 lshift or
    swap $FF and 24 lshift or
  ;

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

  \ Maximum number of DNS addresses to cache
  16 constant max-dns-cache

  \ DNS address cache name heap block size
  16 constant dns-cache-heap-block-size

  \ DNS address cache name heap block count
  64 constant dns-cache-heap-block-count

  \ Maximum number of packets to send at a time
  16 constant max-out-packets

  \ Maximum number of packets to receive at a time
  16 constant max-in-packets

  \ Minimum number of bytes to send if more bytes are available
  16 value small-send-bytes

  \ The maximum retransmit count
  2 value max-retransmits

  \ The initial timeout time
  500 value init-timeout

  \ The timeout multiplication factor
  2 value timeout-multiplier

  \ Send check interval
  500 value send-check-interval
  
  \ DNS port
  53 constant dns-port

  \ DNS source port
  65535 constant dns-src-port

  \ DHCP client port
  68 constant dhcp-client-port

  \ DHCP server port
  67 constant dhcp-server-port

  \ Parse DNS name depth
  16 constant parse-dns-name-depth

  \ Refresh interval
  500 value refresh-interval

  \ SYN_SENT initial refresh timeout
  1000 value syn-sent-init-refresh-timeout

  \ SYN_SENT refresh timeout multiplication factor
  2 value syn-sent-refresh-timeout-multiplier

  \ SYN_SENT maximum refresh count
  3 value syn-sent-max-refreshes

  \ SYN_RECEIVED initial refresh timeout
  1000 value syn-ack-sent-init-refresh-timeout

  \ SYN_RECEIVED refresh timeout multiplication factor
  2 value syn-ack-sent-refresh-timeout-multiplier

  \ SYN_RECEIVED maximum refresh count
  3 value syn-ack-sent-max-refreshes

  \ ESTABLISHED initial refresh timeout
  500 value established-init-refresh-timeout

  \ ESTABLISHED refresh timeout multplication factor
  2 value established-refresh-timeout-multiplier

  \ ESTABLISHED maximum refresh count
  3 value established-max-refreshes

  \ ESTABLISHED no missing packets refresh timeout
  5000 value established-no-missing-refresh-timeout

  \ FIN_WAIT_1 initial refresh timeout
  1000 value fin-wait-1-init-refresh-timeout

  \ FIN_WAIT_1 refresh timeout multiplication factor
  2 value fin-wait-1-refresh-timeout-multiplier

  \ FIN_WAIT_1 maximum refresh count
  3 value fin-wait-1-max-refreshes

  \ LAST_ACK initial refresh timeout
  1000 value last-ack-init-refresh-timeout

  \ LAST_ACK refresh timeout multiplication factor
  2 value last-ack-refresh-timeout-multiplier

  \ LAST_ACK maximum refresh count
  3 value last-ack-max-refreshes
  
  \ Close timeout
  100000 value close-timeout

  \ IPv4 discover attempts
  50000 value dhcp-discover-timeout

  \ Default DHCP renewal time
  86400 10000 * 2 / value default-dhcp-renew-interval
  
  \ DHCP renewal retry divisor
  60 value dhcp-renew-retry-divisor

  \ Default DHCP ARP interval
  10000 value dhcp-arp-interval

  \ DHCP ARP attempt count
  5 value dhcp-arp-attempt-count

  \ DHCPDECLINE delay
  100000 value dhcpdecline-delay
  
  \ DHCP discovery state
  0 constant dhcp-not-discovering
  1 constant dhcp-wait-offer
  2 constant dhcp-wait-ack
  3 constant dhcp-got-nak
  4 constant dhcp-wait-confirm
  5 constant dhcp-discovered
  6 constant dhcp-renewing
  
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
  1 bit constant DF
  2 bit constant MF

  \ IP protocols
  1 constant PROTOCOL_ICMP
  6 constant PROTOCOL_TCP
  17 constant PROTOCOL_UDP

  \ ICMP types
  0 constant ICMP_TYPE_ECHO_REPLY
  8 constant ICMP_TYPE_ECHO_REQUEST

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

  \ TCP states
  0 constant TCP_CLOSED
  1 constant TCP_LISTEN
  2 constant TCP_SYN_SENT
  3 constant TCP_SYN_RECEIVED
  4 constant TCP_ESTABLISHED
  5 constant TCP_FIN_WAIT_1
  6 constant TCP_FIN_WAIT_2
  7 constant TCP_CLOSING
  8 constant TCP_CLOSE_WAIT
  9 constant TCP_LAST_ACK
  10 constant TCP_TIME_WAIT

  \ TCP options
  0 constant TCP_OPT_END
  1 constant TCP_OPT_NOP
  2 constant TCP_OPT_MSS
  4 constant TCP_OPT_MSS_LEN

  \ Ephemeral ports
  49152 constant MIN_EPHEMERAL_PORT
  65534 constant MAX_EPHEMERAL_PORT
  -1 constant EPHEMERAL_PORT

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

  \ Do an alignment-safe 16-bit load
  : hunaligned@ { addr -- h }
    addr c@
    addr 1+ c@ 8 lshift or
  ;

  \ Do an alignment-safe 16-bit store
  : hunaligned! { h addr -- }
    h addr c!
    h 8 rshift addr 1+ c!
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
      offset 253 < and if
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
          part-len offset + 1+ 253 u> or if 0 false exit then
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
      dup 1+ MIN_EPHEMERAL_PORT - MAX_EPHEMERAL_PORT MIN_EPHEMERAL_PORT - umod
      MIN_EPHEMERAL_PORT + current-ephemeral-port !
    ;] ephemeral-port-lock lock::with-lock
  ;

  \ Initialize ephemeral ports
  : init-ephemeral-ports ( -- )
    ephemeral-port-lock lock::init-lock
    rng::random MAX_EPHEMERAL_PORT MIN_EPHEMERAL_PORT - umod
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

end-module

\ Initialize
: init ( -- )
  init
  net-misc::init-ephemeral-ports
;