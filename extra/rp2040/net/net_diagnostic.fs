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

begin-module net-diagnostic

  oo import
  frame-process import
  net-misc import

  \ The parent diagnostic handler
  <frame-handler> begin-class <diag-handler>

    \ Match a frame type
    method diag-applies? ( addr bytes self -- applies? )

    \ Display diagnostic
    method display-diag ( addr bytes self -- )

  end-class

  \ Implement the parent diagnostic handler
  <diag-handler> begin-implement

    \ Handle a frame
    :noname { addr bytes self -- }

      addr bytes self diag-applies? if

        [ debug? ] [if]
          cr ." bytes: " bytes .
          cr ." destination MAC: " addr ethh-destination-mac mac@ mac.
          cr ." source MAC: " addr ethh-source-mac mac@ mac.
          cr ." ethernet type " addr ethh-ether-type h@ rev16 h.4
        [then]

        addr bytes self display-diag

        [ debug? ] [if] addr addr bytes + dump [then]

      then
        
    ; define handle-frame

    \ Match a frame type
    :noname { addr bytes self -- applies? }
      false
    ; define diag-applies?

    \ Display diagnostic
    :noname { addr bytes self -- }
    ; define display-diag

  end-implement

  \ The ARP packet diagnostic handler
  <diag-handler> begin-class <arp-diag-handler> end-class

  \ Implement the ARP packet diagnostic handler
  <arp-diag-handler> begin-implement

    \ Match a frame type
    :noname { addr bytes self -- applies? }
      addr ethh-ether-type h@ [ ETHER_TYPE_ARP rev16 ] literal =
    ; define diag-applies?

    \ Display diagnostic
    :noname { addr bytes self -- }
      addr bytes strip-ethernet-header to bytes to addr
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
        cr ." sha: " addr arp-sha mac@ mac.
        cr ." spa: " addr arp-spa unaligned@ rev ipv4.
        cr ." tha: " addr arp-tha mac@ mac.
        cr ." tpa: " addr arp-tpa unaligned@ rev ipv4.
      then
    ; define display-diag

  end-implement

  \ The IPv4 packet diagnostic handler
  <diag-handler> begin-class <ipv4-diag-handler> end-class

  \ Implement the IPv4 packet diagnostic handler
  <ipv4-diag-handler> begin-implement

    \ Match a frame type
    :noname { addr bytes self -- applies? }
      addr ethh-ether-type h@ [ ETHER_TYPE_IPV4 rev16 ] literal =
    ; define diag-applies?

    \ Display diagnostic
    :noname { addr bytes self -- }
      addr bytes strip-ethernet-header to bytes to addr
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
      cr ." header checksum: " addr ipv4-header-checksum h@ rev16 h.4
      addr ihl 4 * 0 ipv4-header-checksum compute-inet-checksum
      ."  (computed: " h.4 ." )"
      cr ." source IP address: " addr ipv4-src-addr unaligned@ rev ipv4.
      cr ." destination IP address: " addr ipv4-dest-addr unaligned@ rev ipv4.
      addr ipv4-protocol c@ case
        PROTOCOL_TCP of
          addr ihl cells + { tcp-addr }
          bytes ihl cells - { tcp-bytes }
          tcp-addr full-tcp-header-size { header-size }
          tcp-bytes header-size >= if
            tcp-addr tcp.
            tcp-addr tcp-bytes tcp-data over + dump
          then
        endof
        addr addr bytes + dump
      endcase
    ; define display-diag

  end-implement

end-module
