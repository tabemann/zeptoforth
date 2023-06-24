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

begin-module cyw43-test
  
  oo import
  cyw43-control import
  armv6m import
  
  23 constant pwr-pin
  24 constant dio-pin
  25 constant cs-pin
  29 constant clk-pin
  
  0 constant pio-addr
  0 constant sm-index
  pio::PIO0 constant pio-instance
  
  <cyw43-control> class-size buffer: my-cyw43-control
  
  1500 constant mtu-size
  mtu-size cell align aligned-buffer: frame
  
  $0800 constant ETHER_TYPE_IPV4
  $0806 constant ETHER_TYPE_ARP
  $0001 constant HTYPE_ETHERNET
  $0001 constant OPER_REQUEST
  $0002 constant OPER_REPLY
  
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
    
  : rev ( x -- x' ) code[ r6 r6 rev_,_ ]code ;
  : rev16 ( h -- h' ) code[ r6 r6 rev16_,_ ]code ;
  
  \ Do an alignment-safe 32-bit load
  : unaligned@ { addr -- x }
    addr c@
    addr 1+ c@ 8 lshift or
    addr 2 + c@ 16 lshift or
    addr 3 + c@ 24 lshift or
  ;

  
  : init-test ( -- )
    dma-pool::init-dma-pool
    cyw43-clm::data cyw43-clm::size cyw43-fw::data cyw43-fw::size
    pwr-pin clk-pin dio-pin cs-pin pio-addr sm-index pio-instance
    <cyw43-control> my-cyw43-control init-object
    my-cyw43-control init-cyw43
  ;

  : mac. { addr -- }
    6 0 ?do addr i + c@ h.2 i 5 <> if ." :" then loop
  ;
  
  : ip. { addr -- }
    addr unaligned@ rev { ip } 0 24 ?do ip i rshift $FF and (.) i 0 <> if ." ." then -8 +loop
  ;
  
  : ethernet-header. { addr -- }
    cr ." destination MAC: " addr cyw43-structs::ethh-destination-mac mac.
    cr ." source MAC: " addr cyw43-structs::ethh-source-mac mac.
    cr ." ethernet type " addr cyw43-structs::ethh-ether-type h@ rev16 h.4
  ;
  
  : strip-ethernet-header { addr bytes -- addr' bytes' }
    addr cyw43-structs::ethernet-header-size +
    bytes cyw43-structs::ethernet-header-size - 0 max
  ;
  
  : parse-arp { addr bytes -- }
    cr ." ARP:"
    addr arp-htype h@ rev16 { htype }
    cr ." htype: " htype h.4
    htype HTYPE_ETHERNET = if ."  (Ethernet)" then
    addr arp-ptype h@ rev16 { ptype }
    cr ." ptype: " ptype h.4
    ptype ETHER_TYPE_IPV4 = if ."  (IPv4)" then
    addr arp-hlen c@ { hlen }
    cr ." hlen: " hlen .
    addr arp-plen c@ { plen }
    cr ." plen: " plen .
    addr arp-oper h@ rev16 { oper }
    cr ." oper: " oper .
    oper case
      OPER_REQUEST of ." (request)" endof
      OPER_REPLY of ." (reply)" endof
    endcase
    htype HTYPE_ETHERNET = ptype ETHER_TYPE_IPV4 = and if
      cr ." sha: " addr arp-sha mac.
      cr ." spa: " addr arp-spa ip.
      cr ." tha: " addr arp-tha mac.
      cr ." tpa: " addr arp-tpa ip.
    then
  ;
  
  : compute-checksum ( addr -- )
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
  
  : parse-ipv4 { addr bytes -- }
    cr ." IPV4:"
    addr ipv4-version-ihl c@ { version-ihl }
    cr ." version: " version-ihl 4 rshift .
    version-ihl $F and { ihl }
    cr ." ihl: " ihl .
    ihl cells bytes > if ." (Packet too small)" exit then
    cr ." tos: " addr ipv4-tos c@ h.2
    addr ipv4-total-len h@ rev16 { total-len }
    cr ." total length: " total-len .
    addr ipv4-identification h@ rev16 { identification }
    cr ." identification: " identification h.4
    addr ipv4-flags-fragment-offset h@ rev16 { flags-fragment-offset }
    flags-fragment-offset 13 rshift { flags }
    cr ." flags: " flags h.1
    flags-fragment-offset $1FFF and { fragment-offset }
    cr ." fragment offset: " fragment-offset .
    cr ." ttl: " addr ipv4-ttl c@ .
    cr ." protocol: " addr ipv4-protocol c@ .
    cr ." header checksum: " addr ipv4-header-checksum h@ rev16 dup h.4
    addr compute-checksum ."  (computed: " h.4 ." )"
    cr ." source IP address: " addr ipv4-src-addr ip.
    cr ." destination IP address: " addr ipv4-dest-addr ip.
  ;
  
  : receive-data
    begin key? not while
      frame cyw43-test::my-cyw43-control cyw43-control::get-cyw43-rx { bytes }
      cr ." bytes: " bytes .
      frame ethernet-header.
      frame cyw43-structs::ethh-ether-type h@ rev16 { ether-type }
      ether-type case
        ETHER_TYPE_ARP of frame bytes strip-ethernet-header parse-arp endof
        ETHER_TYPE_IPV4 of frame bytes strip-ethernet-header parse-ipv4 endof
      endcase
      frame frame bytes + dump
    repeat
  ;

  : run-test { D: ssid D: pass -- }
    init-test
    ssid pass my-cyw43-control join-cyw43-wpa2
    receive-data
  ;

end-module