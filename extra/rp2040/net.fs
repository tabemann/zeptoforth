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
  frame-interface import
  frame-process import
  net-misc import
  lock import
  sema import
  chan import
  heap import

  \ Oversized frame exception
  : x-oversized-frame ( -- ) cr ." oversized frame" ;

  \ Outstanding packet record
  <object> begin-class <packets>

    \ The outstanding packet count
    cell member packet-count
    
    \ The sequence numbers of the outstanding packets
    max-packets cells member packet-seqs

    \ The sizes of the outstanding packets
    max-packets cells member packet-sizes

    \ The buffer packets are being sent from
    cell member packet-addr

    \ The packet buffer size
    cell member packet-bytes

    \ The ACKed packet buffer offset
    cell member acked-packet-offset

    \ The packet buffer offset
    cell member packet-offset

    \ The sequence number of the first outstanding packet (or the first one to
    \ be sent)
    cell member first-packet-seq
    
    \ The packet MSS
    cell member packet-mss

    \ The packet window
    cell member packet-window
    
    \ Acknowledge packets
    method ack-packets ( ack self -- )

    \ Get info on packet to send
    method get-packet-to-send ( self -- addr bytes prev-seq this-seq )

    \ Get the size of a packet to send
    method next-packet-size@ ( self -- bytes )
    
    \ Are we done sending packets?
    method packets-done? ( self -- done? )

    \ Can we issue a new packet?
    method send-packet? ( self -- send? )

    \ Mark packets as to be resent
    method resend-packets ( self -- )
    
  end-class

  \ Implement the outstanding packet record
  <packets> begin-implement

    \ Constructor
    :noname { addr bytes seq mss window self -- }
      self <object>->new
      addr self packet-addr !
      bytes self packet-bytes !
      0 self acked-packet-offset !
      0 self packet-offset !
      seq self first-packet-seq !
      mss self packet-mss !
      window self packet-window !
      0 self packet-count !
    ; define new

    \ Acknowledge packets
    :noname { ack self -- }
      self first-packet-seq @ { first }
      0 0 0 { index seq bytes }
      self packet-count @ 0 ?do
        self packet-seqs i cells + @ first - ack first - <= if
          self packet-sizes i cells + @ +to bytes
          self packet-seqs i cells + @ to seq
          i to index
        else
          leave
        then
      loop
      seq self first-packet-seq !
      bytes self acked-packet-offset +!
      self packet-count @ index 1+ - cells { move-size }
      self packet-sizes index 1+ cells + self packet-sizes move-size move
      self packet-seqs index 1+ cells + self packet-seqs move-size move
    ; define ack-packets

    \ Get info on packet to send
    :noname { self -- addr bytes prev-seq this-seq }
      self packet-count @ 0> if
        self packet-seqs self packet-count @ cells + @
      else
        self first-packet-seq @
      then { prev-seq }
      self next-packet-size@ { packet-size }
      prev-seq packet-size + { this-seq }
      packet-size self packet-offset +!
      packet-size self packet-sizes self packet-count @ cells + !
      this-seq self packet-seqs self packet-count @ cells + !
      1 self packet-count +!
      self packet-buf @ self packet-offset @ +
      packet-size
      dup self packet-offset +!
      prev-seq this-seq
    ; define get-packet-to-send

    \ Get the size of a packet to send
    :noname ( self -- bytes )
      dup packet-bytes @ over packet-offset @ - over packet-window @ min
      swap packet-mss @ min
    ; define next-packet-size@
    
    \ Are we done sending packets?
    :noname ( self -- done? )
      dup packet-count @ 0=
      swap dup packet-acked-offset @
      swap packet-bytes @ = and
    ; define packets-done?

    \ Can we issue a new packet?
    :noname ( self -- send? )
      dup packet-offset @ over acked-packet-offset @ - over packet-window @ <
      over packet-offset @ 2 pick packet-bytes @ < and
      swap packet-count @ max-packets < and
    ; define send-packet?

    \ Mark packets as to be resent
    :noname ( self -- )
      dup acked-packet-offset @ over packet-offset ! 0 swap packet-count !
    ; define resend-packets

  end-class

  \ The DNS address cache
  <object> begin-class <dns-cache>
    
    \ The cached IPv4 addresses
    max-dns-cache cells member cached-ipv4-addrs

    \ The cached DNS names
    max-dns-cache cells member cached-dns-names

    \ The IP address identification codes
    max-dns-cache cells member cached-dns-idents
    
    \ The cached IPv4 ages
    max-dns-cache cells member cached-dns-ages

    \ The IPv4 response codes
    max-dns-cache cells member cached-dns-responses

    \ The newest age
    cell member newest-dns-age

    \ The cache lock
    lock-size member dns-cache-lock

    \ The DNS cache name heap
    dns-cache-heap-block-size dns-cache-heap-block-count heap-size
    member dns-cache-name-heap

    \ Look up an IPv4 address by a DNS name
    method lookup-ipv4-addr-by-dns
    ( c-addr bytes self -- ipv4-addr response found? )

    \ Reserve a DNS entry
    method reserve-dns ( ident c-addr bytes self -- )
    
    \ Save an IPv4 address by a DNS name
    method save-ipv4-addr-by-dns ( ipv4-addr ident c-addr bytes self -- )

    \ Indicate an abnormal response by DNS name
    method save-response-by-dns ( response ident self -- )
    
    \ Get the oldest DNS name index
    method oldest-dns-index ( self -- index )

    \ Get the oldest allocated DNS name index
    method next-dns-index ( self -- index )

    \ Save a DNS name
    method save-dns-name ( addr len self -- index )

    \ Try to allocate space in the DNS name heap
    method allocate-dns-name-heap ( name-len self -- addr success? )
    
  end-class
  
  \ Implement the DNS address cache
  <dns-cache> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      self cached-dns-names [ max-dns-cache cells ] literal 0 fill
      self cached-ipv4-addrs [ max-dns-cache cells ] literal $FF fill
      self cached-dns-idents [ max-dns-cache cells ] literal $FF fill
      self cached-dns-ages [ max-dns-cache cells ] literal 0 fill
      self cached-dns-responses [ max-dns-cache cells ] literal $FF fill
      0 self newest-dns-age !
      self dns-cache-lock init-lock
      dns-cache-heap-block-size dns-cache-heap-block-count
      self dns-cache-name-heap init-heap
    ; define new

    \ Look up an IPv4 address by a DNS name
    :noname ( c-addr bytes self -- ipv4-addr response found? )
      [: { c-addr bytes self }
        max-dns-cache 0 ?do
          self cached-dns-names i cells + @ ?dup if
            count c-addr bytes equal-case-strings?
            -1 self cached-dns-idents i cells + @ = and if
              self newest-dns-age @ 1+ dup self newest-dns-age !
              self cached-dns-ages i cells + !
              self cached-ipv4-addrs i cells + @
              self cached-dns-responses i cells + @
              true unloop exit
            then
          then
        loop
        0 -1 false
      ;] over dns-cache-lock with-lock
    ; define lookup-ipv4-addr-by-dns

    \ Save an IPv4 address by a DNS name
    :noname ( ipv4-addr ident c-addr bytes self -- )
      [: { ipv4-addr ident c-addr bytes self }
        max-dns-cache 0 ?do
          self cached-dns-names i cells + @ ?dup if
            count c-addr bytes equal-case-strings?
            self cached-dns-idents i cells + @ ident = and if
              ipv4-addr self cached-ipv4-addrs i cells + !
              -1 self cached-dns-idents i cells + !
              self newest-dns-age @ 1+ dup self newest-dns-age !
              self cached-dns-ages i cells + !
              0 self cached-dns-responses i cells + !
              unloop exit
            then
          then
        loop
      ;] over dns-cache-lock with-lock
    ; define save-ipv4-addr-by-dns

    \ Reserve a DNS entry
    :noname ( ident c-addr bytes self -- )
      [: { ident c-addr bytes self }
        max-dns-cache 0 ?do
          self cached-dns-names i cells + @ ?dup if
            count c-addr bytes equal-case-strings? if
              ident self cached-dns-idents i cells + !
              0 self cached-ipv4-addrs i cells + !
              self newest-dns-age @ 1+ dup self newest-dns-age !
              self cached-dns-ages i cells + !
              -1 self cached-dns-responses i cells + !
              unloop exit
            then
          then
        loop
        c-addr bytes self save-dns-name { index }
        ident self cached-dns-idents index cells + !
        0 self cached-ipv4-addrs index cells + !
        self newest-dns-age @ 1+ dup self newest-dns-age !
        self cached-dns-ages index cells + !
        -1 self cached-dns-responses index cells + !
      ;] over dns-cache-lock with-lock
    ; define reserve-dns

    \ Save an abnormal response by a DNS name
    :noname ( response ident self -- )
      [: { response ident self }
        max-dns-cache 0 ?do
          ident self cached-dns-idents i cells + @ = if
            0 self cached-ipv4-addrs i cells + !
            -1 self cached-dns-idents i cells + !
            response self cached-dns-responses i cells + !
            self newest-dns-age @ 1+ dup self newest-dns-age !
            self cached-dns-ages i cells + !
            unloop exit
          then
        loop
      ;] over dns-cache-lock with-lock
    ; define save-response-by-dns

    \ Get the oldest DNS name index
    :noname { self -- index }
      max-dns-cache 0 ?do
        self cached-dns-names i cells + @ 0= if i unloop exit then
      loop
      0 0 { index minimum }
      max-dns-cache 0 ?do
        self cached-dns-ages i cells + @ self newest-dns-age @ -
        dup minimum < if to minimum i to index else drop then
      loop
      index
    ; define oldest-dns-index

    \ Get the oldest allocated DNS name index
    :noname { self -- index }
      0 0 { index minimum }
      max-dns-cache 0 ?do
        self cached-dns-names i cells + @ if
          self cached-dns-ages i cells + @ self newest-dns-age @ -
          dup minimum < if to minimum i to index else drop then
        then
      loop
      index      
    ; define next-dns-index
    
    \ Save a DNS name
    :noname { addr len self -- index }
      self oldest-dns-index { index }
      begin
        self cached-dns-names index cells + @ ?dup if
          self dns-cache-name-heap free
        then
        0 self cached-dns-names index cells + !
        len 1+ self allocate-dns-name-heap if
          len over c!
          addr over 1+ len move
          self cached-dns-names index cells + !
          index exit
        then
        self next-dns-index +to index
      again
    ; define save-dns-name

    \ Try to allocate space in the DNS name heap
    :noname { name-len self -- addr success? }
      name-len self dns-cache-name-heap ['] allocate try
      dup 0= if
        true swap
      else
        dup ['] x-allocate-failed = if
          2drop 0 false 0
        then
      then
      ?raise
    ; define allocate-dns-name-heap

  end-implement

  \ The IPv4 address map
  <object> begin-class <address-map>

    \ The mapped IPv4 addresses
    max-addresses cells member mapped-ipv4-addrs

    \ The mapped MAC addresses
    max-addresses 2 cells * member mapped-mac-addrs

    \ The mapped address ages
    max-addresses cells member mapped-addr-ages

    \ The newest age
    cell member newest-addr-age

    \ The address map lock
    lock-size member address-map-lock

    \ Look up a MAC address by an IPv4 address
    method lookup-mac-addr-by-ipv4 ( ipv4-addr self -- D: mac-addr found? )

    \ Save a MAC address by an IPv4 address
    method save-mac-addr-by-ipv4 ( D: mac-addr ipv4-addr self -- )
    
    \ Get the oldest MAC address index
    method oldest-mac-addr-index ( self -- index )

    \ Store a MAC address by an IPv4 address at an index
    method save-mac-addr-at-index ( D: mac-addr ipv4-addr index self -- )
    
  end-class

  \ Implement the IPv4 address map
  <address-map> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      self mapped-ipv4-addrs [ max-addresses cells ] literal 0 fill
      self mapped-mac-addrs [ max-addresses 2 cells * ] literal $FF fill
      self mapped-addr-ages [ max-addresses cells ] literal 0 fill
      0 self newest-addr-age !
      self address-map-lock init-lock
    ; define new

    \ Look up a MAC address by an IPv4 address
    :noname ( ipv4-addr self -- D: mac-addr found? )
      [: { ipv4-addr self }
        max-addresses 0 ?do
          self mapped-ipv4-addrs i cells + @ ipv4-addr = if
            self mapped-mac-addrs i 2 cells * + 2@
            self newest-addr-age @ 1+ dup self newest-addr-age !
            self mapped-addr-ages i cells + !
            true unloop exit
          then
        loop
        0. false
      ;] over address-map-lock with-lock
    ; define lookup-mac-addr-by-ipv4

    \ Save a MAC address by an IPv4 address
    :noname ( D: mac-addr ipv4-addr self -- )
      [: { D: mac-addr ipv4-addr self }
        max-addresses 0 ?do
          self mapped-ipv4-addrs i cells + @ ipv4-addr = if
            mac-addr ipv4-addr i self save-mac-addr-at-index  unloop exit
          then
        loop
        self oldest-mac-addr-index { index }
        mac-addr ipv4-addr index self save-mac-addr-at-index
      ;] over address-map-lock with-lock
    ; define save-mac-addr-by-ipv4

    \ Get the oldest MAC address index
    :noname { self -- index }
      max-addresses 0 ?do
        self mapped-mac-addrs i 2 cells * + 2@ -1. d= if i unloop exit then
      loop
      0 0 { index minimum }
      max-addresses 0 ?do
        self mapped-addr-ages i cells + @ self newest-addr-age @ -
        dup minimum < if to minimum i to index else drop then
      loop
      index
    ; define oldest-mac-addr-index

    \ Store a MAC address by an IPv4 address at an index
    :noname { D: mac-addr ipv4-addr index self -- }
      mac-addr self mapped-mac-addrs index 2 cells * + 2!
      ipv4-addr self mapped-ipv4-addrs index cells + !
      self newest-addr-age @ 1+ dup self newest-addr-age !
      self mapped-addr-ages index cells + !
    ; define save-mac-addr-at-index

  end-implement

  \ The endpoint class
  <object> begin-class <endpoint>

    \ Is the endpoint state
    cell member endpoint-state
    
    \ The remote IPv4 address
    cell member endpoint-remote-ipv4-addr

    \ The remote port
    2 member endpoint-remote-port
    
    \ The local port
    2 member endpoint-local-port

    \ The remote sequence number
    cell member endpoint-remote-seq

    \ The local sequence number
    cell member endpoint-local-seq

    \ The remote maximum segment size
    cell member endpoint-remote-mss

    \ The processed data size
    cell member endpoint-rx-size

    \ The delayed data size
    cell member endpoint-delayed-size

    \ The remote MAC address
    2 cells member endpoint-remote-mac-addr
    
    \ The endpoint buffer
    max-payload-size cell align member endpoint-buf

    \ Endpoint lock
    lock-size member endpoint-lock

    \ Endpoint control lock
    lock-size member endpoint-ctrl-lock

    \ The received packet semaphore
    sema-size member endpoint-sema

    \ Broadcast an endpoint event
    method broadcast-endpoint ( self -- )

    \ Wait for an endpoint event
    method wait-endpoint ( self -- )

    \ Claim control of an endpoint
    method with-ctrl-endpoint ( xt self -- )
    
    \ Get endpoint TCP state
    method tcp-state@ ( self -- tcp-state )

    \ Set endpoint TCP state
    method tcp-state! ( tcp-state self -- )
    
    \ Get endpoint received data
    method endpoint-rx-data@ ( self -- addr bytes )

    \ Set endpoint received data
    method endpoint-rx-data! ( addr bytes self -- )

    \ Get endpoint source
    method endpoint-ipv4-remote@ ( self -- ipv4-addr port )
    
    \ Set endpoint source
    method endpoint-ipv4-remote! ( ipv4-addr port self -- )

    \ Get endpoint remote MAC address
    method endpoint-remote-mac-addr@ ( self -- D: mac-addr )

    \ Get destination port
    method endpoint-local-port@ ( self -- port )
    
    \ Retire a pending received packet
    method retire-rx-data ( self -- )

    \ Endpoint has pending received packet
    method pending-endpoint? ( self -- pending? )

    \ Endpoint has been dequeued
    method dequeued-endpoint? ( self -- dequeued? )

    \ Endpoint is active but not pending
    method available-for-packet? ( self -- available? )

    \ Endpoint is a UDP endpoint
    method udp-endpoint? ( self -- udp? )

    \ Data is available for endpoint
    method endpoint-data-available? ( self -- available? )

    \ Ready an endpoint
    method ready-endpoint ( self -- )

    \ Mark an endpoint as dequeued
    method dequeue-endpoint ( self -- )

    \ Try to allocate an endpoint
    method try-allocate-endpoint ( self -- allocated? )

    \ Try to accepting on an endpoint
    method try-ipv4-accept-endpoint
    ( src-ipv4-addr src-port dest-port D: mac-addr self -- accepted? )

    \ Try to open a connection on an endpoint
    method try-ipv4-connect-endpoint
    ( src-port dest-ipv4-addr dest-port D: mac-addr self -- allocated? )

    \ Match an endpoint with an IPv4 TCP connection
    method match-ipv4-connect?
    ( src-ipv4-addr src-port dest-port self -- match? )
    
    \ Initialize a TCP connection
    method init-ipv4-connect ( self -- )

    \ Free an endpoint
    method free-endpoint ( self -- )

    \ Set an endpoint to listen on UDP
    method listen-udp ( port self -- )

    \ Set an endpoint to listen on TCP
    method listen-tcp ( port self -- )

    \ Get remote seq number
    method endpoint-remote-seq@ ( self -- seq )

    \ Get local seq number
    method endpoint-local-seq@ ( self -- seq )
    
    \ Set the remote seq number
    method endpoint-remote-seq! ( seq self -- )
    
    \ Add to remote seq number
    method +endpoint-remote-seq! ( increment self -- )

    \ Add to local seq number
    method +endpoint-local-seq! ( increment self -- )

    \ Get the remote MSS
    method endpoint-remote-mss@ ( self -- mss )

    \ Set the remote MSS
    method endpoint-remote-mss! ( mss self -- )

    \ Endpoint window size
    method endpoint-window@ ( self -- bytes )

    \ Add data to a TCP endpoint if possible
    method add-endpoint-data ( addr bytes self -- room? )

  end-class

  \ Implement the endpoint class
  <endpoint> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      0 self endpoint-state !
      0 self endpoint-remote-ipv4-addr !
      0 self endpoint-remote-port h!
      0 self endpoint-local-port h!
      0 self endpoint-remote-seq !
      0 self endpoint-local-seq !
      0 self endpoint-rx-size !
      0 self endpoint-delayed-size !
      self endpoint-lock init-lock
      self endpoint-ctrl-lock init-lock
      1 0 self endpoint-sema init-sema
    ; define new

    \ Broadcast an endpoint event
    :noname ( self -- )
      dup endpoint-sema broadcast
      endpoint-sema give
    ; define broadcast-endpoint

    \ Wait for an endpoint event
    :noname ( self -- )
      endpoint-sema take
    ; define wait-endpoint

    \ Claim control of an endpoint
    :noname ( xt self -- )
      endpoint-ctrl-lock with-lock
    ; define with-ctrl-endpoint

    \ Get endpoint TCP state
    :noname ( self -- tcp-state )
      endpoint-state @ endpoint-tcp-state-mask and endpoint-tcp-state-lsb rshift
    ; define endpoint-tcp-state@
    
    \ Set endpoint TCP state
    :noname { tcp-state self -- }
      self endpoint-state @ endpoint-tcp-state-mask bic
      tcp-state endpoint-tcp-state-lsb lshift or self endpoint-state !
    ; define endpoint-tcp-state!

    \ Get endpoint received data
    :noname ( self -- addr bytes )
      dup endpoint-buf swap endpoint-rx-size @
    ; define endpoint-rx-data@

    \ Set endpoint received data
    :noname { addr bytes self -- }
      addr self endpoint-buf bytes move
      bytes self endpoint-rx-size !
    ; define endpoint-rx-data!

    \ Get endpoint source
    :noname ( self -- ipv4-addr port )
      dup endpoint-remote-ipv4-addr @
      swap endpoint-remote-port h@
    ; define endpoint-ipv4-remote@
    
    \ Set endpoint source
    :noname { ipv4-addr port self -- }
      ipv4-addr self endpoint-remote-ipv4-addr !
      port self endpoint-remote-port h!
    ; define endpoint-ipv4-remote!

    \ Get endpoint remote MAC address
    :noname ( self -- D: mac-addr )
      endpoint-remote-mac-addr 2@
    ; endpoint-remote-mac-addr@

    \ Get destination port
    :noname ( self -- port )
      endpoint-local-port h@
    ; define endpoint-local-port@

    \ Retire a pending received packet
    :noname ( self -- )
      [: { self }
        [ endpoint-pending endpoint-dequeued or ] literal
        self endpoint-state bic!
        endpoint-tcp self endpoint-state bit@ if
          self endpoint-buffer self endpoint-rx-size @ +
          self endpoint-buffer self endpoint-reassemble-size @ move
          self endpoint-reassemble-size @ self endpoint-rx-size !
          0 self endpoint-reassemble-size !
        else
          0 self endpoint-rx-size !
        then
      ;] over endpoint-lock with-lock
    ; define retire-rx-data

    \ Endpoint has pending received packet
    :noname ( self -- pending? )
      endpoint-state @ [ endpoint-active endpoint-pending or ] literal
      tuck and =
    ; define pending-endpoint?

    \ Endpoint has been dequeued
    :noname ( self -- dequeued? )
      endpoint-dequeued swap endpoint-state bit@
    ; define dequeued-endpoint?

    \ Endpoint is active but not pending
    :noname ( self -- available? )
      endpoint-state @ [ endpoint-active endpoint-pending or ] literal
      and endpoint-active =
    ; define available-for-packet?

    \ Ready an endpoint
    :noname ( self -- )
      [: endpoint-pending swap endpoint-state bis! ;]
      over endpoint-lock with-lock
    ; define ready-endpoint

    \ Mark an endpoint as dequeued
    :noname ( self -- )
      [: endpoint-dequeued swap endpoint-state bis! ;]
      over endpoint-lock with-lock
    ; define dequeue-endpoint
    
    \ Endpoint is a UDP endpoint
    :noname ( self -- udp? )
      endpoint-state @ [ endpoint-active endpoint-udp or ] literal
      tuck and =
    ; define udp-endpoint?

    \ Data is available for endpoint
    :noname ( self -- available? )
      endpoint-rx-size @ 0<>
    ; define endpoint-data-available?

    \ Try to allocate an endpoint
    :noname ( self -- allocated? )
      [:
        endpoint-active over endpoint-state bit@ not if
          endpoint-active swap endpoint-state bis! true
        else
          drop false
        then
      ;] over endpoint-lock with-lock
    ; define try-allocate-endpoint

    \ Try to accept a connection on an endpoint
    :noname ( src-ipv4-addr src-port dest-port D: mac-addr self -- accepted? )
      [: { src-ipv4-addr src-port dest-port D: mac-addr self }
        self endpoint-tcp-state@ TCP_LISTEN =
        dest-port self endpoint-local-port@ = if
          src-ipv4-addr src-port self endpoint-ipv4-remote!
          TCP_SYN_RECEIVED self endpoint-tcp-state!
          mac-addr self endpoint-remote-mac-addr 2!
          self init-ipv4-connect
          true
        else
          false
        then
      ;] over endpoint-lock with-lock
    ; define try-ipv4-accept-endpoint

    \ Try to open a connection on an endpoint
    :noname ( src-port dest-ipv4-addr dest-port D: mac-addr self -- allocated? )
      [: { src-port dest-ipv4-addr dest-port D: mac-addr self )
        self try-allocate-endpoint if
          src-port self endpoint-local-port h!
          dest-ipv4-addr dest-port endpoint-ipv4-remote!
          TCP_SYN_SENT self endpoint-tcp-state!
          mac-addr self endpoint-remote-mac-addr 2!
          self init-ipv4-connect
        else
          false
        then
      ;] over endpoint-lock with-lock
    ; define try-ipv4-connect-endpoint

    \ Match an endpoint with an IPv4 TCP connection
    :noname ( src-ipv4-addr src-port dest-port self -- match? )
      [: { src-ipv4-addr src-port dest-port self }
        self endpoint-tcp-state@ dup TCP_CLOSED <> swap TCP_LISTEN <> and if
          self endpoint-ipv4-remote@ src-port = swap src-ipv4-addr = and
          self endpoint-local-port@ dest-port = and
        else
          false
        then
      ;] over endpoint-lock with-lock
    ; define match-ipv4-connect?

    \ Initialize a TCP connection
    :noname { self -- }
      [ mtu-size ipv4-header-size - tcp-header-size - ] literal
      self endpoint-remote-mss !
      rng::random self endpoint-local-seq !
      0 self endpoint-rx-size !
      0 self endpoint-delayed-size !
    ; define init-ipv4-connect
    
    \ Free an endpoint
    :noname ( self -- )
      [: endpoint-active swap endpoint-state bic! ;]
      over endpoint-lock with-lock
    ; define free-endpoint

    \ Set an endpoint to listen on UDP
    :noname ( port self -- )
      [: endpoint-udp over endpoint-state bis! endpoint-local-port h! ;]
      over endpoint-lock with-lock
    ; define listen-udp

    \ Set an endpoint to listen on TCP
    :noname ( port self -- )
      [: { port self }
        endpoint-tcp self endpoint-state bis!
        TCP_LISTEN self endpoint-tcp-state!
        port self endpoint-local-port h!
      ;] over endpoint-lock with-lock
    ; define listen-tcp

    \ Get remote seq number
    :noname ( self -- seq )
      endpoint-remote-seq @
    ; define endpoint-remote-seq@

    \ Get local seq number
    :noname ( self -- seq )
      endpoint-local-seq @
    ; define endpoint-local-seq@

    \ Set the remote seq number
    :noname ( seq self -- )
      endpoint-remote-seq !
    ; define endpoint-remote-seq!
    
    \ Add to remote seq number
    :noname ( increment self -- )
      endpoint-remote-seq +!
    ; define +endpoint-remote-seq!

    \ Add to local seq number
    :noname ( increment self -- )
      endpoint-local-seq +!
    ; define +endpoint-local-seq!

    \ Get the remote MSS
    :noname ( self -- mss )
      endpoint-remote-mss @
    ; define endpoint-remote-mss@

    \ Set the remote MSS
    :noname ( mss self -- )
      endpoint-remote-mss !
    ; define endpoint-remote-mss!

    \ Endpoint window size
    :noname ( self -- bytes )
      [: dup endpoint-rx-size @ swap endpoint-delayed-size @ + ;]
      over endpoint-lock with-lock
    ; define endpoint-window@

    \ Add data to a TCP endpoint if possible
    :noname ( addr bytes self -- room? )
      [: { addr bytes self }
        self endpoint-window@ { total-used }
        bytes total-used + max-payload-size <= if
          addr endpoint endpoint-buf total-used + bytes move
          bytes endpoint
          endpoint dequeued-endpoint? not if
            endpoint-rx-size
          else
            endpoint-delayed-size
          then
          +!
        else
          false
        then
      ;] over endpoint-lock with-lock
    ; define add-endpoint-data

  end-implement
  
  \ The fragment collector class
  <object> begin-class <fragment-collect>

    \ Is the fragment collector active
    cell member fragment-active
    
    \ The fragment source IP address
    cell member fragment-src-ipv4-addr

    \ The fragment protocol
    cell member fragment-protocol

    \ The fragment identification field
    cell member fragment-ident

    \ Maximum fragment length
    cell member fragment-end-length
    
    \ The fragment bitmap
    max-fragment-units 8 align 8 / member fragment-bitmap
    
    \ The fragment buffer
    fragment-buf-size cell align member fragment-buf

    \ Apply a fragment
    method apply-fragment ( addr byte self -- complete? )

    \ Clear the fragment
    method clear-fragment ( self -- )

    \ Get the completed packet
    method get-completed-packet ( self -- src-ipv4-addr protocol addr bytes )
    
  end-class

  \ Implement the fragment collector class
  <fragment-collect> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      self clear-fragment
    ; define new
    
    \ Apply a fragment
    :noname { addr bytes self -- complete? }
      addr ipv4-fragment? not if false exit then
      addr ipv4-identification h@ rev16 { ident }
      ident self fragment-ident @ <> if self clear-fragment then
      addr ipv4-protocol c@ self fragment-protocol @ <> if
        self clear-fragment
      then
      addr ipv4-version-ihl c@ $F 5 max and 4 * { header-size }
      addr ipv4-total-len h@ rev16
      dup bytes <> if false exit then
      header-size - { len }
      addr ipv4-flags-fragment-offset h@ rev16
      dup $1FFF and { offset }
      13 rshift { flags }
      offset 8 * len + fragment-buf-size <= if
        true self fragment-active !
        ident self fragment-ident !
        addr ipv4-src-addr unaligned@ rev self fragment-src-ipv4-addr !
        addr ipv4-protocol c@ self fragment-protocol !
        flags MF and 0= if
          offset 8 * len + self fragment-end-length !
        then
        len 8 align 8 / offset + offset ?do
          i 7 and bit i 3 rshift self fragment-bitmap + 2dup cbit@ not if
            cbis!
          else
            2drop self clear-fragment false unloop exit
          then
        loop
        addr header-size + self fragment-buf offset 8 * + len move
        self fragment-end-length @ -1 <> if
          self fragment-end-length @ 8 align 8 / 0 ?do
            i 7 and bit i 3 rshift self fragment-bitmap + cbit@ not if
              false unloop exit
            then
          loop
          true
        else
          false
        then
      else
        self clear-fragment false
      then
    ; define apply-fragment

    \ Clear the fragment
    :noname { self -- }
      false self fragment-active !
      0 self fragment-src-ipv4-addr !
      0 self fragment-protocol !
      0 self fragment-ident !
      -1 self fragment-end-length !
      self fragment-bitmap max-fragment-units 8 align 8 / 0 fill
    ; define clear-fragment

    \ Get the completed packet
    :noname { self -- src-ipv4-addr protocol addr bytes }
      self fragment-src-ipv4-addr @
      self fragment-protocol @
      self fragment-buf
      self fragment-end-length @
    ; define get-completed-packet

  end-implement

  \ The interface class
  <object> begin-class <interface>

    \ The output frame interface
    cell member out-frame-interface
    
    \ The IPv4 address
    cell member intf-ipv4-addr

    \ The DNS server
    cell member dns-server-ipv4-addr

    \ The gateway IPv4 address
    cell member gateway-ipv4-addr
    
    \ The IPv4 netmask
    cell member intf-ipv4-netmask

    \ Current TTL
    cell member intf-ttl

    \ MAC address resolution interval in ticks (100 us intervals)
    cell member mac-addr-resolve-interval

    \ Maximum MAC address resolution attempts
    cell member max-mac-addr-resolve-attempts

    \ MAC address resolution semaphore
    sema-size member mac-addr-resolve-sema

    \ DNS resolution interval in ticks (100 us intervals)
    cell member dns-resolve-interval

    \ Maximum DNS resolution attempts
    cell member max-dns-resolve-attempts

    \ DNS resolution semaphore
    sema-size member dns-resolve-sema

    \ The endpoints
    <endpoint> class-size max-endpoints * member intf-endpoints

    \ The address map
    <address-map> class-size member address-map

    \ The DNS cache
    <dns-cache> class-size member dns-cache
    
    \ The IPv4 fragment collector
    <fragment-collect> class-size member fragment-collect

    \ The receive endpoint queue
    cell max-endpoints chan-size member endpoint-rx-queue

    \ Outgoing buffer lock
    lock-size member outgoing-buf-lock

    \ The outgoing frame buffer
    mtu-size cell align member outgoing-buf

    \ Get the IPv4 address
    method intf-ipv4-addr@ ( self -- addr )
    
    \ Set the IPv4 address
    method intf-ipv4-addr! ( addr self -- )

    \ Get the IPv4 netmask
    method intf-ipv4-netmask@ ( self -- netmask )

    \ Set the IPv4 netmask
    method intf-ipv4-netmask! ( netmask self -- )

    \ Get the gateway IPv4 address
    method gateway-ipv4-addr@ ( self -- addr )

    \ Set the gateway IPv4 address
    method gateway-ipv4-addr! ( addr self -- )
    
    \ Get the IPv4 broadcast address
    method intf-ipv4-broadcast@ ( self -- addr )
    
    \ Get the MAC address
    method intf-mac-addr@ ( self -- addr )

    \ Get the TTL
    method intf-ttl@ ( self -- ttl )
    
    \ Set the TTL
    method intf-ttl! ( ttl self -- )

    \ Get the MAC address resolution interval in ticks (100 us intervals)
    method mac-addr-resolve-interval@ ( self -- ticks )
    
    \ Set the MAC address resolution interval in ticks (100 us intervals)
    method mac-addr-resolve-interval! ( ticks self -- )

    \ Get the maximum MAC address resolution attempts
    method max-mac-addr-resolve-attempts@ ( self -- attempts )

    \ Set the maximum MAC address resolution attempts
    method max-mac-addr-resolve-attempts! ( attempts self -- )

    \ Get the DNS resolution interval in ticks (100 us intervals)
    method dns-resolve-interval@ ( self -- ticks )
    
    \ Set the DNS resolution interval in ticks (100 us intervals)
    method dns-resolve-interval! ( ticks self -- )

    \ Get the maximum DNS resolution attempts
    method max-dns-resolve-attempts@ ( self -- attempts )

    \ Set the maximum DNS resolution attempts
    method max-dns-resolve-attempts! ( attempts self -- )
    
    \ Send a frame
    method send-frame ( addr bytes self -- )

    \ Process a MAC address for an IPv4 address
    method process-ipv4-mac-addr ( D: mac-addr ipv4-addr self -- )
    
    \ Process a fragment
    method process-fragment ( addr bytes self -- )

    \ Process an IPv4 packet
    method process-ipv4-packet ( src-addr protocol addr bytes self -- )

    \ Find a listening IPv4 TCP endpoint
    method find-listen-ipv4-endpoint
    ( src-addr addr bytes self -- endpoint found? )

    \ Find a connecting/connected IPv4 TCP endpoint
    method find-connect-ipv4-endpoint
    ( src-addr addr bytes self -- endpoint found? )
    
    \ Process an IPv4 TCP packet
    method process-ipv4-tcp-packet ( src-addr protocol addr bytes self -- )

    \ Process an IPv4 TCP SYN packet
    method process-ipv4-syn-packet ( src-addr addr bytes self -- )

    \ Send an IPv4 TCP SYN+ACK packet
    method send-ipv4-syn-ack ( src-addr addr bytes self -- ) 

    \ Send an IPv4 TCP RST packet in response to a packet
    method send-ipv4-rst-for-packet ( src-addr addr bytes self -- )

    \ Send a basic IPv4 TCP packet
    method send-ipv4-basic-tcp
    ( remote-addr remote-port local-port seq ack window flags D: mac-addr )
    ( self -- )
    
    \ Process an IPv4 TCP SYN+ACK packet
    method process-ipv4-syn-ack-packet ( src-addr addr bytes self -- )

    \ Process an IPv4 ACK packet
    method process-ipv4-ack-packet ( src-addr addr bytes self -- )

    \ Process an IPv4 FIN packet
    method process-ipv4-fin-packet ( src-addr addr bytes self -- )

    \ Process an IPv4 RST packet
    method process-ipv4-rst-packet ( src-addr addr bytes self -- )
    
    \ Process an IPv4 UDP packet
    method process-ipv4-udp-packet ( src-addr protocol addr bytes self -- )

    \ Process an IPv4 DNS response packet
    method process-ipv4-dns-packet ( addr bytes self -- )
    
    \ Process an IPv4 ICMP packet
    method process-ipv4-icmp-packet ( src-addr protocol addr bytes self -- )

    \ Process an IPv4 ICMP echo request packet
    method process-ipv4-echo-request-packet ( src-addr addr bytes self -- )
    
    \ Construct and send a frame
    method construct-and-send-frame
    ( ? bytes xt self -- ? sent? ) ( xt: ? buf -- ? send? )

    \ Construct an IPv4 packet
    method construct-and-send-ipv4-packet
    ( ? D: mac-addr dest-addr protocol bytes xt self -- ? sent? )
    ( xt: ? buf -- ? send? )

    \ Resolve an IPv4 address's MAC address
    method resolve-ipv4-addr-mac-addr ( dest-addr self -- D: mac-addr success? )

    \ Resolve a DNS name's IPv4 address
    method resolve-dns-ipv4-addr ( c-addr bytes self -- ipv4-addr success? )
    
    \ Process IPv4 DNS response packet answers
    method process-ipv4-dns-answers
    ( addr bytes all-addr all-bytes ancount ident self -- )

    \ Send an ARP request packet
    method send-ipv4-arp-request ( dest-addr self -- )

    \ Send a DNS request packet
    method send-ipv4-dns-request ( c-addr bytes self -- )
    
    \ Send a UDP packet
    method send-ipv4-udp-packet
    ( ? src-port dest-addr dest-port bytes xt self -- ? success? )
    ( xt: ? buf -- ? sent? )
    
    \ Enqueue a ready receiving IP endpoint
    method put-ready-endpoint ( endpoint self -- )

    \ Dequeue a ready receiving IP endpoint
    method get-ready-endpoint ( self -- endpoint )

    \ Allocate an endpoint
    method allocate-endpoint ( self -- endpoint success? )

  end-class

  \ Implement the interface class
  <interface> begin-implement

    \ Constructor
    :noname { frame-interface self -- }
      self <object>->new
      frame-interface self out-frame-interface !
      0 self intf-ipv4-addr !
      255 255 255 0 make-ipv4-addr self intf-ipv4-netmask !
      192 168 1 254 make-ipv4-addr self gateway-ipv4-addr !
      8 8 8 8 make-ipv4-addr self dns-server-ipv4-addr !
      32 self intf-ttl !
      50000 self mac-addr-resolve-interval !
      5 self max-mac-addr-resolve-attempts !
      1 0 self mac-addr-resolve-sema init-sema
      50000 self dns-resolve-interval !
      5 self max-dns-resolve-attempts !
      1 0 self dns-resolve-sema init-sema
      self outgoing-buf-lock init-lock
      max-endpoints 0 ?do
        <endpoint> self intf-endpoints <endpoint> class-size i * + init-object
      loop
      <fragment-collect> self fragment-collect init-object
      <address-map> self address-map init-object
      <dns-cache> self dns-cache init-object
      cell max-endpoints self endpoint-rx-queue init-chan
    ; define new

    \ Get the IPv4 address
    :noname ( self -- addr )
      intf-ipv4-addr @
    ; define intf-ipv4-addr@
    
    \ Set the IPv4 address
    :noname ( addr self -- )
      intf-ipv4-addr !
    ; define intf-ipv4-addr!

    \ Get the IPv4 netmask
    :noname ( self -- netmask )
      intf-ipv4-netmask @
    ; define intf-ipv4-netmask@

    \ Set the IPv4 netmask
    :noname ( netmask self -- )
      intf-ipv4-netmask !
    ; define intf-ipv4-netmask!

    \ Get the gateway IPv4 address
    :noname ( self -- addr )
      gateway-ipv4-addr @
    ; define gateway-ipv4-addr@
    
    \ Set the gateway IPv4 address
    :noname ( addr self -- )
      gateway-ipv4-addr !
    ; define gateway-ipv4-addr!

    \ Get the IPv4 broadcast address
    :noname { self -- addr }
      self intf-ipv4-addr @ self intf-ipv4-netmask @ and
      $FFFFFFFF self intf-ipv4-netmask @ bic or
    ; define intf-ipv4-broadcast@

    \ Get the MAC address
    :noname ( self -- D: addr )
      out-frame-interface @ mac-addr@
    ; define intf-mac-addr@

    \ Get the TTL
    :noname ( self -- ttl )
      intf-ttl @
    ; define intf-ttl@
    
    \ Set the TTL
    :noname { ttl self -- }
      ttl 255 min 1 max self intf-ttl !
    ; define intf-ttl!

    \ Get the address resolution interval in ticks (100 us intervals)
    :noname ( self -- ticks )
      mac-addr-resolve-interval @
    ; define mac-addr-resolve-interval@
    
    \ Set the address resolution interval in ticks (100 us intervals)
    :noname ( ticks self -- )
      mac-addr-resolve-interval !
    ; define mac-addr-resolve-interval!

    \ Get the maximum MAC address resolution attempts
    :noname ( self -- attempts )
      max-mac-addr-resolve-attempts @
    ; define max-mac-addr-resolve-attempts@

    \ Set the maximum MAC address resolution attempts
    :noname ( attempts self -- )
      max-mac-addr-resolve-attempts !
    ; define max-mac-addr-resolve-attempts!
    
    \ Get the DNS resolution interval in ticks (100 us intervals)
    :noname ( self -- ticks )
      dns-resolve-interval @
    ; define dns-resolve-interval@
    
    \ Set the DNS resolution interval in ticks (100 us intervals)
    :noname ( ticks self -- )
      dns-resolve-interval !
    ; define dns-resolve-interval!

    \ Get the maximum DNS resolution attempts
    :noname ( self -- attempts )
      max-dns-resolve-attempts @
    ; define max-dns-resolve-attempts@

    \ Set the maximum DNS resolution attempts
    :noname ( attempts self -- )
      max-dns-resolve-attempts !
    ; define max-dns-resolve-attempts!

    \ Send a frame
    :noname { addr bytes self -- }
      bytes mtu-size u<= averts x-oversized-frame
      cr ." SENT: "
      addr addr bytes + dump
      addr bytes self out-frame-interface @ put-tx-frame
    ; define send-frame
    
    \ Process a MAC address for an IPv4 address
    :noname ( D: mac-addr ipv4-addr self -- )
      dup { self } address-map save-mac-addr-by-ipv4
      self mac-addr-resolve-sema broadcast
      self mac-addr-resolve-sema give
    ; define process-ipv4-mac-addr

    \ Process a fragment
    :noname ( addr bytes self -- )
      dup { self }
      fragment-collect apply-fragment if
        self fragment-collect get-completed-packet self process-ipv4-packet
        fragment-collect clear-fragment
      then
    ; define process-fragment

    \ Process an IPv4 packet
    :noname ( src-addr protocol addr bytes self -- )
      3 pick case
        PROTOCOL_UDP of process-ipv4-udp-packet endof
        PROTOCOL_TCP of process-ipv4-tcp-packet endof
        PROTOCOL_ICMP of process-ipv4-icmp-packet endof
        2drop 2drop drop
      endcase
    ; define process-ipv4-packet

    \ Process an IPv4 TCP packet
    :noname { src-addr protocol addr bytes self -- }
      bytes tcp-header-size >= if
        addr full-tcp-header-size bytes > if exit then
        src-addr addr bytes self
        addr tcp-flags c@ TCP_CONTROL and
        case
          TCP_SYN of process-ipv4-syn-packet endof
          [ TCP_SYN TCP_ACK or ] literal of process-ipv4-syn-ack-packet endof
          TCP_ACK of process-ipv4-ack-packet endof
          TCP_FIN of process-ipv4-fin-packet endof
          TCP_RST of process-ipv4-rst-packet endof
        endcase
      then
    ; define process-ipv4-tcp-packet

    \ Find a listening IPv4 TCP endpoint
    :noname { src-addr addr bytes self -- endpoint found? )
      max-endpoints 0 ?do
        self intf-endpoints <endpoint> class-size i * + { endpoint }
        src-addr
        addr tcp-src-port hunaligned@ rev16
        addr tcp-dest-port hunaligned@ rev16
        src-addr self address-map lookup-mac-addr-by-ipv4 if
          endpoint try-ipv4-accept-endpoint if endpoint true exit then
        else
          2drop 2drop drop \ Should never happen
        then
      loop
      0 false
    ; define find-listen-ipv4-endpoint

    \ Find a connecting/connected IPv4 TCP endpoint
    :noname { src-addr addr bytes self -- endpoint found? )
      max-endpoints 0 ?do
        self intf-endpoints <endpoint> class-size i * + { endpoint }
        src-addr
        addr tcp-src-port hunaligned@ rev16
        addr tcp-dest-port hunaligned@ rev16
        endpoint match-ipv4-connect? if endpoint true exit then
      loop
      0 false
    ; define find-connect-ipv4-endpoint
    
    \ Process an IPv4 TCP SYN packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-listen-ipv4-endpoint if
        swap send-ipv4-syn-ack
      else
        drop send-ipv4-rst-for-packet
      then
    ; define process-ipv4-syn-packet

    \ Send an IPv4 TCP SYN+ACK packet
    :noname { src-addr addr bytes endpoint self -- }
      addr bytes tcp-mss@ if endpoint endpoint-remote-mss! else drop then
      addr tcp-seq-no unaligned@ rev 1+ endpoint endpoint-remote-seq!
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      1 endpoint +endpoint-local-seq!
      endpoint endpoint-remote-seq@
      endpoint endpoint-window@
      [ TCP_SYN TCP_ACK or ] literal
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
      TCP_SYN_RECEIVED endpoint endpoint-tcp-state!
    ; define send-ipv4-syn-ack

    \ Send an IPv4 TCP RST packet in response to a packet
    :noname ( src-addr addr bytes self -- ) { self }
      drop
      over { src-addr }
      dup tcp-src-port hunaligned@ rev16 swap
      dup tcp-dest-port hunaligned@ rev16 swap
      random::rng swap
      tcp-seq-no unaligned@ 1+
      0
      TCP_RST
      src-addr self address-map lookup-mac-addr-by-ipv4 if
        self send-ipv4-basic-tcp
      else
        2drop 2drop 2drop 2drop \ Should never happen
      then
    ; define send-ipv4-rst-for-packet

    \ Send a basic IPv4 TCP packet
    :noname
      ( remote-addr remote-port local-port seq ack window flags D: mac-addr )
      ( self -- )
      -rot 9 pick PROTOCOL_TCP tcp-header-size
      ( self D: mac-addr remote-addr protocol bytes )
      [: { remote-addr remote-port local-port seq ack window flags self buf }
        local-port rev16 tcp-src-port hunaligned!
        remote-port rev16 tcp-dest-port hunaligned!
        seq rev buf tcp-seq-no unaligned!
        ack rev buf tcp-ack-no unaligned!
        [ 5 4 lshift ] literal buf tcp-data-offset c!
        flags buf tcp-flags c!
        window buf tcp-window-size hunaligned!
        0 buf tcp-urgent-ptr hunaligned!
        self intf-ipv4-addr@ remote-addr buf tcp-header-size 0 tcp-checksum
        rev16 buf tcp-checksum hunaligned!
        true
      ;] 6 pick construct-and-send-ipv4-packet drop
    ; define send-ipv4-basic-tcp
    
    \ Process an IPv4 TCP SYN+ACK packet
    :noname { src-addr addr bytes self -- }
      src-addr addr bytes self find-connect-ipv4-endpoint not if
        drop src-addr addr bytes self send-ipv4-rst-for-packet exit
      then
      { endpoint }
      endpoint endpoint-tcp-state@ TCP_SYN_SENT <> if
        src-addr addr bytes self send-ipv4-rst-for-packet exit
      then
      addr bytes tcp-mss@ if endpoint endpoint-remote-mss! else drop then
      addr tcp-seq-no unaligned@ rev 1+ endpoint endpoint-remote-seq!
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      1 endpoint +endpoint-local-seq!
      endpoint endpoint-remote-seq@
      endpoint endpoint-window@
      TCP_ACK
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
      TCP_ESTABLISHED endpoint endpoint-tcp-state!
      endpoint self put-ready-endpoint
    ; define process-ipv4-syn-ack-packet

    \ Process an IPv4 ACK packet
    :noname { src-addr addr bytes self -- }
      src-addr addr bytes self find-connect-ipv4-endpoint not if
        drop src-addr addr bytes self send-ipv4-rst-for-packet exit
      then
      { endpoint }
      endpoint endpoint-tcp-state@ { prev-state }
      endpoint endpoint-rx-data@ nip { prev-size }
      addr bytes endpoint self
      endpoint endpoint-tcp-state@ case
        TCP_SYN_RECEIVED of process-ipv4-ack-syn-received endof
        TCP_ESTABLISHED of process-ipv4-ack-established endof
        TCP_FIN_WAIT_1 of process-ipv4-ack-fin-wait-1 endof
        TCP_FIN_WAIT_2 of process-ipv4-ack-fin-wait-2 endof
        TCP_CLOSE_WAIT of process-ipv4-ack-close-wait endof
        TCP_LAST_ACK of process-ipv4-ack-last-ack endof
        send-ipv4-rst-for-ack
      endcase
      endpoint endpoint-rx-data@ nip prev-size <>
      endpoint endpoint-tcp-state@ prev-state <> or if
        endpoint self put-ready-endpoint
      else
        endpoint broadcast-endpoint
      then
    ; define process-ipv4-ack-packet

    \ Process an IPv4 ACK packet in the general case
    :noname { addr bytes endpoint self -- }
      addr full-tcp-header-size { header-size }
      addr tcp-seq-no unaligned@ rev endpoint endpoint-remote-seq@ = if
        addr header-size + bytes header-size - endpoint add-endpoint-data if
          bytes header-size - endpoint +endpoint-remote-seq!
          endpoint endpoint-ipv4-remote@
          endpoint endpoint-local-port@
          endpoint endpoint-local-seq@
          endpoint endpoint-remote-seq@
          endpoint endpoint-window@
          TCP_ACK
          endpoint endpoint-remote-mac-addr@
          self send-ipv4-basic-tcp
        then
      then
    ; define process-ipv4-basic-ack
    
    \ Process an IPv4 ACK packet in TCP_SYN_RECEIVED state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self process-ipv4-basic-ack
      TCP_ESTABLISHED endpoint endpoint-tcp-state!
    ; define process-ipv4-ack-syn-received
    
    \ Process an IPv4 ACK packet in TCP_ESTABLISHED state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self process-ipv4-basic-ack
    ; define process-ipv4-ack-established
    
    \ Process an IPv4 ACK packet in TCP_FIN_WAIT_1 state
    :noname { addr bytes endpoint self -- }
      addr tcp-ack-no unaligned@ rev
      endpoint endpoint-local-seq@ = if
        TCP_FIN_WAIT2 endpoint endpoint-tcp-state!
      else
        addr bytes endpoint self process-ipv4-basic-ack
      then
    ; define process-ipv4-ack-fin-wait-1
    
    \ Process an IPv4 ACK packet in TCP_FIN_WAIT_2 state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self process-ipv4-basic-ack
    ; define process-ipv4-ack-fin-wait-2
    
    \ Process an IPv4 ACK packet in TCP_CLOSE_WAIT state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self process-ipv4-basic-ack
    ; define process-ipv4-ack-close-wait
    
    \ Process an IPv4 ACK packet in TCP_LAST_ACK state
    :noname { addr bytes endpoint self -- }
      addr tcp-ack-no hunaligned rev
      endpoint endpoint-local-seq@ = if
        TCP_CLOSED endpoint endpoint-tcp-state!
      else
        addr bytes endpoint self process-ipv4-basic-ack
      then
    ; define process-ipv4-ack-last-ack

    \ Process an errant IPv4 ACK packet
    :noname { addr bytes endpoint self -- }
      endpoint endpoint-ipv4-remote@ drop addr bytes
      self send-ipv4-rst-for-packet
    ; define send-ipv4-rst-for-ack
    
    \ Process an IPv4 FIN packet
    :noname { src-addr addr bytes self -- }
      src-addr addr bytes self find-connect-ipv4-endpoint not if
        drop src-addr addr bytes self send-ipv4-rst-for-packet exit
      then
      { endpoint }
      addr bytes endpoint self
      endpoint endpoint-tcp-state@ case
        TCP_ESTABLISHED of process-ipv4-fin-established endof
        TCP_FIN_WAIT_2 of process-ipv4-fin-fin-wait-2 endof
        process-ipv4-unexpected-fin
      endcase
    ; define process-ipv4-fin-packet

    \ Process an IPv4 FIN packet for a TCP_ESTABLISHED state
    :noname { addr bytes endpoint self -- }
      addr tcp-seq-no unaligned@ rev endpoint endpoint-remote-seq!
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      endpoint endpoint-remote-seq@
      endpoint endpoint-window@
      TCP_ACK
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
      TCP_CLOSE_WAIT endpoint endpoint-tcp-state!
    ; define process-ipv4-fin-established

    \ Process an IPv4 FIN packet for a TCP_FIN_WAIT_2 state
    :noname { addr bytes endpoint self -- }
      addr tcp-seq-no unaligned@ rev endpoint endpoint-remote-seq!
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      endpoint endpoint-remote-seq@
      endpoint endpoint-window@
      TCP_ACK
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
      TCP_TIME_WAIT endpoint endpoint-tcp-state!
    ; define process-ipv4-fin-fin-wait-2

    \ Process an unexpected IPv4 FIN packet
    :noname { addr bytes endpoint self -- }
      addr tcp-seq-no unaligned@ rev endpoint endpoint-remote-seq!
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      endpoint endpoint-remote-seq@
      endpoint endpoint-window@
      TCP_ACK
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
      TCP_CLOSING endpoint endpoint-tcp-state!
    ; define process-ipv4-unexpected-fin

    \ Process an IPv4 RST packet
    :noname { src-addr addr bytes self -- }
      src-addr addr bytes self find-connect-ipv4-endpoint if
        TCP_CLOSED over endpoint-tcp-state!
        self put-ready-endpoint
      else
        drop
      then
    ; define process-ipv4-rst-packet
          
    \ Process an IPv4 UDP packet
    :noname { src-addr protocol addr bytes self -- }
      bytes udp-header-size >= if

        \ DEBUG
        cr ." Source address: " src-addr ipv4.
        cr ." Total length: " addr udp-total-len h@ rev16 .
        cr ." Actual length: " bytes .
        cr ." Source port: " addr udp-src-port h@ rev16 .
        cr ." Destination port: " addr udp-dest-port h@ rev16 .
        \ DEBUG
        
        addr udp-total-len h@ rev16 bytes <> if exit then
        src-addr self dns-server-ipv4-addr @ =
        addr udp-src-port h@ rev16 dns-port = and if
          addr udp-header-size + bytes udp-header-size -
          self process-ipv4-dns-packet
          exit
        then
        max-endpoints 0 ?do
          self intf-endpoints <endpoint> class-size i * + { endpoint }
          src-addr addr bytes endpoint self [:
            { src-addr addr bytes endpoint self }
            endpoint udp-endpoint?
            endpoint available-for-packet? and
            endpoint endpoint-local-port@ addr udp-dest-port h@ rev16 = and if
              src-addr addr udp-src-port h@ rev16 endpoint endpoint-ipv4-remote!
              addr udp-header-size + bytes udp-header-size -
              endpoint endpoint-rx-data!
              endpoint self put-ready-endpoint
              true
            else
              false
            then
          ;] endpoint endpoint-lock with-lock
          if unloop exit then
        loop
      then
    ; define process-ipv4-udp-packet

    \ Process an IPv4 DNS response packet
    :noname { addr bytes self -- }
      bytes dns-header-size < if exit then
      addr bytes { all-addr all-bytes }
      addr dns-ident hunaligned@ rev16 { ident }
      addr dns-flags hunaligned@ rev16 { flags }
      DNS_QR_RESPONSE flags and 0= if exit then
      DNS_OPCODE_MASK flags and if exit then
      DNS_TC flags and if exit then
      DNS_RA flags and 0= if exit then
      flags DNS_RCODE_MASK and ?dup if
        ident self dns-cache save-response-by-dns
        self dns-resolve-sema broadcast
        self dns-resolve-sema give
        exit
      then
      addr dns-qdcount hunaligned@ rev16 { qdcount }
      addr dns-ancount hunaligned@ rev16 { ancount }
      dns-header-size +to addr
      [ dns-header-size negate ] literal +to bytes
      begin qdcount 0> bytes 0> and while
        addr bytes skip-dns-name to bytes to addr
        bytes dns-qbody-size < if exit then
        dns-qbody-size +to addr
        [ dns-qbody-size negate ] literal +to bytes
        -1 +to qdcount
      repeat
      addr bytes all-addr all-bytes ancount ident self process-ipv4-dns-answers
    ; define process-ipv4-dns-packet

    \ Process IPv4 DNS response packet answers
    :noname { addr bytes all-addr all-bytes ancount ident self -- }
      begin ancount 0> bytes 0> and while
        addr bytes { saved-addr saved-bytes }
        addr bytes skip-dns-name to bytes to addr
        bytes dns-abody-size < if exit then

        addr dns-abody-type hunaligned@ [ 1 rev16 ] literal =
        addr dns-abody-class hunaligned@ [ 1 rev16 ] literal = and
        addr dns-abody-rdlength hunaligned@ [ 4 rev16 ] literal = and if
          dns-abody-size +to addr
          [ dns-abody-size negate ] literal +to bytes
          bytes 4 < if exit then
          addr unaligned@ rev
          saved-addr saved-bytes all-addr all-bytes ident self 256 [:
            { ident self buf }
            buf parse-dns-name if
              ident buf rot self dns-cache save-ipv4-addr-by-dns
              self dns-resolve-sema broadcast
              self dns-resolve-sema give
            else
              2drop
            then
          ;] with-allot
          exit
        else
          addr dns-abody-rdlength hunaligned@ rev16 { rdlength }
          dns-abody-size rdlength + dup +to addr negate +to bytes
          -1 +to ancount
        then
      repeat
    ; define process-ipv4-dns-answers
    
    \ Process an IPv4 ICMP packet
    :noname { src-addr protocol addr bytes self -- }
      bytes icmp-header-size >= if
        addr bytes 0 icmp-checksum compute-inet-checksum
        addr icmp-checksum h@ rev16 <> if exit then
        addr icmp-type c@ case
          ICMP_TYPE_ECHO_REQUEST of
            src-addr addr bytes self process-ipv4-echo-request-packet
          endof
        endcase
      then
    ; define process-ipv4-icmp-packet

    \ Process an IPv4 ICMP echo request packet
    :noname { src-addr addr bytes self -- }
      bytes icmp-header-size < if exit then
      src-addr self address-map lookup-mac-addr-by-ipv4 if
        { D: src-mac-addr }
        addr bytes self src-mac-addr src-addr PROTOCOL_ICMP bytes [:
          { addr bytes self buf }
          ICMP_TYPE_ECHO_REPLY buf icmp-type c!
          ICMP_CODE_UNUSED buf icmp-code c!
          addr 4 + buf 4 + bytes 4 - move
          buf bytes 0 icmp-checksum compute-inet-checksum rev16
          buf icmp-checksum h!
          true
        ;] self construct-and-send-ipv4-packet drop
      else
        2drop \ Should never happen
      then
    ; define process-ipv4-echo-request-packet
    
    \ Construct and send a frame
    :noname ( ? bytes xt self -- ? sent? ) ( xt: ? buf -- ? send? )
      [: { bytes xt self }
        bytes mtu-size u<= averts x-oversized-frame
        self outgoing-buf xt execute if
          self outgoing-buf bytes self send-frame true
        else
          false
        then
      ;] over outgoing-buf-lock with-lock
    ; define construct-and-send-frame

    \ Construct and send a IPv4 packet
    :noname ( ? D: mac-addr dest-addr protocol bytes xt self -- ? sent? )
      ( xt: ? buf -- ? send? )
      2 pick [ ethernet-header-size ipv4-header-size + ] literal + over
      [: { D: mac-addr dest-addr protocol bytes xt self buf }
        mac-addr buf ethh-destination-mac mac!
        self intf-mac-addr@ buf ethh-source-mac mac!
        [ ETHER_TYPE_IPV4 rev16 ] literal buf ethh-ether-type h!
        buf ethernet-header-size + { ip-buf }
        [ 4 4 lshift 5 or ] literal ip-buf ipv4-version-ihl c!
        0 ip-buf ipv4-tos c!
        bytes ipv4-header-size + rev16 ip-buf ipv4-total-len h!
        0 ip-buf ipv4-identification h!
        [ DF 13 lshift rev16 ] literal ip-buf ipv4-flags-fragment-offset h!
        self intf-ttl c@ ip-buf ipv4-ttl c!
        protocol ip-buf ipv4-protocol c!
        self intf-ipv4-addr@ rev ip-buf ipv4-src-addr unaligned!
        dest-addr rev ip-buf ipv4-dest-addr unaligned!
        ip-buf ipv4-header-size 0 ipv4-header-checksum compute-inet-checksum
        rev16 ip-buf ipv4-header-checksum h!
        ip-buf ipv4-header-size + xt execute
      ;] swap construct-and-send-frame
    ; define construct-and-send-ipv4-packet

    \ Resolve an IPv4 address's MAC address
    :noname { dest-addr self -- D: mac-addr success? }
      dest-addr self intf-ipv4-netmask@ and
      192 168 1 0 make-ipv4-addr self intf-ipv4-netmask@ and <> if
        self gateway-ipv4-addr@ to dest-addr
      then
      systick::systick-counter self mac-addr-resolve-interval@ - { tick }
      self max-mac-addr-resolve-attempts @ { attempts }
      begin
        dest-addr self address-map lookup-mac-addr-by-ipv4 not
      while
        2drop
        systick::systick-counter tick - self mac-addr-resolve-interval@ >= if
          attempts 0> if
            -1 +to attempts
            dest-addr self send-ipv4-arp-request
            systick::systick-counter to tick
          else
            0. false exit
          then
        else
          task::timeout @ { old-timeout }
          tick self mac-addr-resolve-interval @ + systick::systick-counter -
          task::timeout !
          self mac-addr-resolve-sema ['] take try
          dup ['] task::x-timed-out = if 2drop 0 then
          ?raise
          old-timeout task::timeout !
        then
      repeat
      true
    ; define resolve-ipv4-addr-mac-addr

    \ Resolve a DNS name's IPv4 address
    :noname { c-addr bytes self -- ipv4-addr success? }
      systick::systick-counter self dns-resolve-interval@ - { tick }
      self max-dns-resolve-attempts @ { attempts }
      c-addr bytes self dns-cache lookup-ipv4-addr-by-dns if
        0= if true exit then
      else
        2drop
      then
      begin
        systick::systick-counter tick - self dns-resolve-interval@ >= if
          attempts 0> if
            -1 +to attempts
            c-addr bytes self send-ipv4-dns-request
            systick::systick-counter to tick
          else
            0 false exit
          then
        else
          task::timeout @ { old-timeout }
          tick self dns-resolve-interval @ + systick::systick-counter -
          task::timeout !
          self dns-resolve-sema ['] take try
          dup ['] task::x-timed-out = if 2drop 0 then
          ?raise
          old-timeout task::timeout !
        then
        c-addr bytes self dns-cache lookup-ipv4-addr-by-dns if
          0= true
        else
          2drop false
        then
      until
    ; define resolve-dns-ipv4-addr

    \ Send an ARP request packet
    :noname ( dest-addr self -- )
      [ ethernet-header-size arp-ipv4-size + ] literal [: { dest-addr self buf }
        $FFFFFFFFFFFF. buf ethh-destination-mac mac!
        self intf-mac-addr@ buf ethh-source-mac mac!
        [ ETHER_TYPE_ARP rev16 ] literal buf ethh-ether-type h!
        buf ethernet-header-size + { arp-buf }
        [ HTYPE_ETHERNET rev16 ] literal arp-buf arp-htype h!
        [ ETHER_TYPE_IPV4 rev16 ] literal arp-buf arp-ptype h!
        6 arp-buf arp-hlen c!
        4 arp-buf arp-plen c!
        [ OPER_REQUEST rev16 ] literal arp-buf arp-oper h!
        self intf-mac-addr@ arp-buf arp-sha mac!
        self intf-ipv4-addr@ rev arp-buf arp-spa unaligned!
        0. arp-buf arp-tha mac!
        dest-addr rev arp-buf arp-tpa unaligned!
        true
      ;] 2 pick construct-and-send-frame drop
    ; define send-ipv4-arp-request

    \ Send a DNS request packet
    :noname { c-addr bytes self -- }
      c-addr bytes self dns-src-port self dns-server-ipv4-addr @ dns-port
      bytes dns-name-size [ dns-header-size dns-qbody-size + ] literal + [:
        { c-addr bytes self buf }
        rng::random $FFFF and
        dup c-addr bytes self dns-cache reserve-dns
        rev16 buf dns-ident hunaligned!
        [ DNS_RD rev16 ] literal buf dns-flags hunaligned!
        [ 1 rev16 ] literal buf dns-qdcount hunaligned!
        [ 0 rev16 ] literal buf dns-ancount hunaligned!
        [ 0 rev16 ] literal buf dns-nscount hunaligned!
        [ 0 rev16 ] literal buf dns-arcount hunaligned!
        dns-header-size +to buf
        c-addr bytes buf format-dns-name
        bytes dns-name-size +to buf
        [ 1 rev16 ] literal buf dns-qbody-qtype hunaligned!
        [ 1 rev16 ] literal buf dns-qbody-qclass hunaligned!
        true
      ;] self send-ipv4-udp-packet drop
    ; define send-ipv4-dns-request

    \ Send a UDP packet
    :noname { src-port dest-addr dest-port bytes xt self -- success? }
      dest-addr self resolve-ipv4-addr-mac-addr if
        src-port -rot dest-port -rot bytes -rot xt -rot self -rot
        dest-addr PROTOCOL_UDP bytes udp-header-size + [:
          { src-port dest-port bytes xt self buf }
          src-port rev16 buf udp-src-port h!
          dest-port rev16 buf udp-dest-port h!
          bytes udp-header-size + rev16 buf udp-total-len h!
          0 buf udp-checksum h!
          buf udp-header-size + xt execute
        ;] self construct-and-send-ipv4-packet
      else
        2drop false
      then
    ; define send-ipv4-udp-packet

    \ Enqueue a ready receiving IP endpoint
    :noname { W^ endpoint self -- }
      endpoint @ pending-endpoint? not if
        endpoint @ ready-endpoint
        endpoint cell self endpoint-rx-queue send-chan
        endpoint @ broadcast-endpoint
      then
    ; define put-ready-endpoint

    \ Dequeue a ready receiving IP endpoint
    :noname { self -- endpoint }
      0 { W^ endpoint }
      endpoint cell self endpoint-rx-queue recv-chan drop
      endpoint @
      dup dequeue-endpoint
    ; define get-ready-endpoint

    \ Allocate an endpoint
    :noname { self -- endpoint success? }
      max-endpoints 0 ?do
        self intf-endpoints i <endpoint> class-size * + { endpoint }
        endpoint try-allocate-endpoint if endpoint true unloop exit then
      loop
      0 false
    ; define allocate-endpoint

    \ Mark an endpoint as done
    :noname { endpoint self -- }
      endpoint retire-rx-data
      endpoint endpoint-rx-data@ nip if endpoint self put-ready-endpoint then
    ; define endpoint-done

  end-implement

  \ The IP protocol handler
  <frame-handler> begin-class <ip-handler>

    \ The IP interface
    cell member ip-interface

  end-class

  \ Implemnt the IP protocol handler
  <ip-handler> begin-implement

    \ Constructor
    :noname { ip self -- }
      self <frame-handler>->new
      ip self ip-interface !
    ; define new

    \ Handle a frame
    :noname { addr bytes self -- }
      addr ethh-ether-type h@ [ ETHER_TYPE_IPV4 rev16 ] literal = if
        addr ethh-source-mac mac@ { D: src-mac-addr }
        ethernet-header-size +to addr
        [ ethernet-header-size negate ] literal +to bytes
        bytes ipv4-header-size >= if
          src-mac-addr addr ipv4-src-addr unaligned@ rev
          self ip-interface @
          process-ipv4-mac-addr
          bytes addr ipv4-total-len h@ rev16 =
          addr ipv4-version-ihl c@ $F and dup { ihl } 5 >= and
          ihl 4 * bytes <= and if
            addr ipv4-dest-addr unaligned@ rev
            dup self ip-interface @ intf-ipv4-addr@ =
            swap self ip-interface @ intf-ipv4-broadcast@ = or if
              addr ipv4-fragment? if
                addr bytes self ip-interface @ process-fragment
              else
                addr ipv4-src-addr unaligned@ rev
                addr ipv4-protocol c@
                addr ihl cells + bytes ihl cells -
                self ip-interface @ process-ipv4-packet
              then
            then
          then
        then
      then
    ; define handle-frame

  end-implement
  
  \ The ARP packet handler
  <frame-handler> begin-class <arp-handler>

    \ The ARP IP interface
    cell member arp-interface

    \ Send an ARP response
    method send-arp-response ( addr self -- )
    
  end-class
  
  \ Implement the ARP packet handler
  <arp-handler> begin-implement
    
    \ Constructor
    :noname { ip self -- }
      self <frame-handler>->new
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
          addr arp-plen c@ 4 = and if
            addr arp-sha mac@ addr arp-spa unaligned@ rev self arp-interface @
            process-ipv4-mac-addr
            addr arp-oper h@ [ OPER_REQUEST rev16 ] literal = if
              addr arp-tpa unaligned@ rev
              self arp-interface @ intf-ipv4-addr@ = if
                addr self send-arp-response
              then
            then
          then
        then
      then
    ; define handle-frame

    \ Send an ARP response
    :noname ( addr self -- )
      [ ethernet-header-size arp-ipv4-size + ] literal
      [: { addr self buf }
        addr arp-sha mac@ buf ethh-destination-mac mac!
        self arp-interface @ intf-mac-addr@ buf ethh-source-mac mac!
        [ ETHER_TYPE_ARP rev16 ] literal buf ethh-ether-type h!
        buf ethernet-header-size + { arp-buf }
        [ HTYPE_ETHERNET rev16 ] literal arp-buf arp-htype h!
        [ ETHER_TYPE_IPV4 rev16 ] literal arp-buf arp-ptype h!
        6 arp-buf arp-hlen c!
        4 arp-buf arp-plen c!
        [ OPER_REPLY rev16 ] literal arp-buf arp-oper h!
        self arp-interface @ intf-mac-addr@ arp-buf arp-sha mac!
        self arp-interface @ intf-ipv4-addr@ rev arp-buf arp-spa unaligned!
        addr arp-sha mac@ arp-buf arp-tha mac!
        addr arp-spa unaligned@ arp-buf arp-tpa unaligned!
        true
      ;] 2 pick arp-interface @ construct-and-send-frame drop
    ; define send-arp-response
    
  end-implement
  
end-module