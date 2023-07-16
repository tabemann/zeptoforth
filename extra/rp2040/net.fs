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

  \ Send timed out
  : x-send-timed-out ( -- ) cr ." send timed out" ;

  \ Outstanding packet record
  <object> begin-class <out-packets>

    \ The outstanding packet count
    cell member out-packet-count
    
    \ The sequence numbers of the outstanding packets
    max-out-packets cells member out-packet-seqs

    \ The sizes of the outstanding packets
    max-out-packets 1 lshift member out-packet-sizes

    \ The buffer packets are being sent from
    cell member out-packet-addr

    \ The packet buffer size
    cell member out-packet-bytes

    \ The ACKed packet buffer offset
    cell member acked-out-packet-offset

    \ The packet buffer offset
    cell member out-packet-offset

    \ The sequence number of the first outstanding packet (or the first one to
    \ be sent)
    cell member first-out-packet-seq
    
    \ The packet MSS
    cell member out-packet-mss

    \ The packet window
    cell member out-packet-window

    \ Reset the outgoing packets
    method reset-out-packets ( seq mss window self -- )
    
    \ Start sending outgoing packets
    method start-out-packets ( addr bytes self -- )

    \ Acknowledge packets
    method ack-packets ( window ack self -- )

    \ Get info on packet to send
    method get-packet-to-send ( self -- addr bytes )

    \ Get the current sequence number
    method current-out-packet-seq@ ( self -- seq )

    \ Increment the bare minimum current sequence number
    method +current-out-packet-seq! ( increment self -- )

    \ Get the size of a packet to send
    method next-packet-size@ ( self -- bytes )

    \ Are there outstanding packets?
    method packets-outstanding? ( self -- outstanding? )
    
    \ Are we done sending packets?
    method packets-done? ( self -- done? )

    \ Can we issue a new packet?
    method send-packet? ( self -- send? )

    \ Mark packets as to be resent
    method resend-packets ( self -- )

    \ Print out packets
    method out-packets. ( self -- )
    
  end-class

  \ Implement the outstanding packet record
  <out-packets> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      0 0 0 self reset-out-packets
    ; define new

    \ Reset the outgoing packets
    :noname { seq mss window self -- }
      0 self out-packet-addr !
      0 self out-packet-bytes !
      0 self acked-out-packet-offset !
      0 self out-packet-offset !
      seq self first-out-packet-seq !
      mss self out-packet-mss !
      window self out-packet-window !
      0 self out-packet-count !
      [ debug? ] [if] cr ." === reset-out-packets:" self out-packets. [then]
    ; define reset-out-packets
    
    \ Start sending outgoing packets
    :noname { addr bytes self -- }
      addr self out-packet-addr !
      bytes self out-packet-bytes !
      0 self acked-out-packet-offset !
      0 self out-packet-offset !
      0 self out-packet-count !
      [ debug? ] [if] cr ." === start-out-packets:" self out-packets. [then]
    ; define start-out-packets

    \ Acknowledge packets
    :noname { window ack self -- }
      window self out-packet-window !
      self first-out-packet-seq @ { first }
      0 first self acked-out-packet-offset @ { count seq bytes }
      self out-packet-count @ 0 ?do
        self out-packet-seqs i cells + @ first - ack first - <= if
          self out-packet-sizes i 1 lshift + h@ +to bytes
          self out-packet-seqs i cells + @ to seq
          i 1+ to count
        else
          leave
        then
      loop
      seq self first-out-packet-seq !
      bytes self acked-out-packet-offset !
      self out-packet-sizes count 1 lshift + self out-packet-sizes
      self out-packet-count @ count - 1 lshift move
      self out-packet-seqs count cells + self out-packet-seqs
      self out-packet-count @ count - cells move
      count negate self out-packet-count +!
      [ debug? ] [if] cr ." === ack-packets:" self out-packets. [then]
    ; define ack-packets

    \ Get info on packet to send
    :noname { self -- addr bytes }
      self out-packet-count @ 0> if
        self out-packet-seqs self out-packet-count @ 1- cells + @
      else
        self first-out-packet-seq @
      then { prev-seq }
      self next-packet-size@ { out-packet-size }
      prev-seq out-packet-size + { this-seq }
      out-packet-size self out-packet-offset +!
      out-packet-size self out-packet-sizes
      self out-packet-count @ 1 lshift + h!
      this-seq self out-packet-seqs self out-packet-count @ cells + !
      1 self out-packet-count +!
      self out-packet-addr @ self out-packet-offset @ + out-packet-size -
      out-packet-size
      [ debug? ] [if] cr ." === get-packet-to-send:" self out-packets. [then]
    ; define get-packet-to-send

    \ Get the current sequence number
    :noname ( self -- seq )
      [ debug? ] [if] dup [then]
      dup out-packet-count @ 1 > if
        dup out-packet-seqs swap out-packet-count @ 1- cells + @
      else
        first-out-packet-seq @
      then
      [ debug? ] [if] cr ." === current-out-packet-seq:" swap out-packets. [then]
    ; define current-out-packet-seq@

    \ Increment the bare minimum current sequence number
    :noname ( increment self -- )
      dup out-packet-count @ 0= if first-out-packet-seq +! else 2drop then
    ; define +current-out-packet-seq!

    \ Get the size of a packet to send
    :noname ( self -- bytes )
      dup out-packet-bytes @ over out-packet-offset @ - over
      out-packet-window @ min
      swap out-packet-mss @ min
      [ debug? ] [if] cr ." === next-packet-size: " dup . [then]
    ; define next-packet-size@

    \ Are there outstanding packets?
    :noname ( self -- outstanding? )
      dup acked-out-packet-offset @ swap out-packet-offset @ <>
      [ debug? ] [if] cr ." === packets-outstanding?: " dup . [then]
    ; define packets-outstanding?
    
    \ Are we done sending packets?
    :noname ( self -- done? )
      dup out-packet-count @ 0=
      swap dup acked-out-packet-offset @
      swap out-packet-bytes @ = and
      [ debug? ] [if] cr ." === packets-done?: " dup . [then]
    ; define packets-done?

    \ Can we issue a new packet?
    :noname ( self -- send? )
      dup out-packet-offset @ over acked-out-packet-offset @ -
      over out-packet-window @ <
      over out-packet-offset @ 2 pick out-packet-bytes @ < and
      swap out-packet-count @ max-out-packets < and
      [ debug? ] [if] cr ." === send-packet?: " dup . [then]
    ; define send-packet?

    \ Mark packets as to be resent
    :noname ( self -- )
      [ debug? ] [if] dup [then]
      dup acked-out-packet-offset @ over out-packet-offset !
      0 swap out-packet-count !
      [ debug? ] [if] cr ." === resend-packets: " out-packets. [then]
    ; define resend-packets

    \ Print out packets
    :noname { self -- }
      cr ." out-packet-count: " self out-packet-count @ .
      cr ." out-packet-seqs: "
      self out-packet-count @ 0 ?do self out-packet-seqs i cells + @ . loop
      cr ." out-packet-sizes: "
      self out-packet-count @ 0 ?do self out-packet-sizes i 1 lshift + h@ . loop
      cr ." out-packet-addr: " self out-packet-addr @ h.8
      cr ." out-packet-bytes: " self out-packet-bytes @ .
      cr ." acked-out-packet-offset: " self acked-out-packet-offset @ .
      cr ." out-packet-offset: " self out-packet-offset @ .
      cr ." first-out-packet-seq: " self first-out-packet-seq @ .
      cr ." out-packet-mss: " self out-packet-mss @ .
      cr ." out-packet-window: " self out-packet-window @ .
      cr
    ; define out-packets.
    
  end-implement

  \ The incoming packet record
  <object> begin-class <in-packets>

    \ Are the incoming packets TCP?
    cell member in-packets-tcp
    
    \ The incoming packet count
    cell member in-packet-count

    \ The sequence numbers of the incoming packets
    max-in-packets cells member in-packet-seqs

    \ The sizes of the incoming packets
    max-in-packets 1 lshift member in-packet-sizes

    \ The buffer incoming packets are being written to
    cell member in-packet-addr

    \ The incoming packet buffer size
    cell member in-packet-bytes

    \ The full packet buffer offset
    cell member in-packet-offset

    \ The pending packet buffer offset
    cell member pending-in-packet-offset

    \ The sequence number of the first incoming packet
    cell member first-in-packet-seq

    \ Last ACK sent
    cell member in-packet-last-ack-sent

    \ Current ACK
    cell member current-in-packet-ack
    
    \ Pending data size
    method pending-in-size@ ( self -- bytes )
    
    \ Reset the incoming TCP packet record
    method reset-in-tcp-packets ( seq self -- )

    \ Reset the incoming UDP packet record
    method reset-in-udp-packets ( self -- )

    \ Get TCP window
    method in-packets-window@ ( self -- )

    \ Get whether there are waiting in packets
    method waiting-in-packets? ( self -- waiting? )
    
    \ Get the amount of waiting data for an endpoint
    method waiting-in-bytes@ ( self -- bytes )

    \ Add incoming TCP packet
    method add-in-tcp-packet ( addr bytes seq self -- )

    \ Insert a TCP packet entry
    method insert-tcp-packet ( addr bytes seq index self -- )

    \ Add incoming UDP packet
    method add-in-udp-packet ( addr bytes self -- )
    
    \ Get whether to send an ACK packet
    method in-packet-send-ack? ( self -- send? )

    \ Mark an ACK as having been sent
    method in-packet-ack-sent ( self -- )

    \ Get the packet to ACK
    method in-packet-ack@ ( self -- ack )

    \ Promote incoming data to pending
    method promote-in-packets ( self -- bytes )

    \ Get the complete packet size and count
    method complete-in-packets ( self -- bytes packet-count )
    
    \ Clear contiguous packets
    method clear-in-packets ( self -- )

    \ Print in packets
    method in-packets. ( self -- )
    
  end-class

  \ Implement the incoming packet record
  <in-packets> begin-implement

    \ Constructor
    :noname { addr bytes self -- }
      self <object>->new
      addr self in-packet-addr !
      bytes self in-packet-bytes !
      self reset-in-udp-packets
    ; define new

    \ Reset the incoming TCP packet record
    :noname { seq self -- }
      true self in-packets-tcp !
      0 self in-packet-count !
      self in-packet-seqs max-in-packets cells 0 fill
      self in-packet-sizes max-in-packets 1 lshift 0 fill
      0 self in-packet-offset !
      0 self pending-in-packet-offset !
      seq self first-in-packet-seq !
      seq self in-packet-last-ack-sent !
      seq self current-in-packet-ack !
      [ debug? ] [if] cr ." @@@ reset-in-tcp-packets: " self in-packets. [then]
    ; define reset-in-tcp-packets

    \ Reset the incoming UDP packet record
    :noname { self -- }
      false self in-packets-tcp !
      0 self in-packet-count !
      self in-packet-seqs max-in-packets cells 0 fill
      self in-packet-sizes max-in-packets 1 lshift 0 fill
      0 self in-packet-offset !
      0 self pending-in-packet-offset !
      0 self first-in-packet-seq !
      0 self in-packet-last-ack-sent !
      0 self current-in-packet-ack !
      [ debug? ] [if] cr ." @@@ reset-in-udp-packets: " self in-packets. [then]
    ; define reset-in-udp-packets

    \ Get TCP window
    :noname { self -- }
      self in-packet-count @ max-in-packets < if
        self in-packet-bytes @ self in-packet-offset @ -
        self pending-in-packet-offset @ -
      else
        0
      then
      [ debug? ] [if] cr ." @@@ in-packets-window@: " self in-packets. [then]
    ; define in-packets-window@

    \ Get whether there are waiting in packets
    :noname ( self -- waiting? )
      complete-in-packets drop 0>
    ; define waiting-in-packets?

    \ Get the amount of waiting data for an endpoint
    :noname ( self -- bytes )
      complete-in-packets drop
    ; define waiting-in-bytes@
      
    \ Add incoming TCP packet
    :noname { addr bytes seq self -- }
      bytes 0> if
        seq self first-in-packet-seq @ - { diff }
        diff 0>=
        diff bytes + self pending-in-packet-offset @ +
        self in-packet-bytes @ <= and if
          0 { index }
          self in-packet-count @ 0 ?do
            self in-packet-seqs i cells + @
            self in-packet-sizes i 1 lshift + h@ +
            seq >= if
              i to index leave
            then
          loop
          begin bytes 0> index self in-packet-count @ < and while
            self in-packet-seqs index cells + @ { current-seq }
            current-seq self first-in-packet-seq @ - { current-diff }
            self in-packet-sizes index 1 lshift + h@ { current-size }
            diff current-diff < if
              current-seq seq - bytes min { part-bytes }
              addr part-bytes seq index self insert-tcp-packet
              part-bytes +to seq
              part-bytes +to diff
              part-bytes +to addr
              part-bytes negate +to bytes
            else
              diff bytes + current-diff current-size + > if
                diff bytes + current-diff current-size +
                min diff - { part-bytes }
                part-bytes +to seq
                part-bytes +to diff
                part-bytes +to addr
                part-bytes negate +to bytes
              else
                0 to bytes
              then
            then
            1 +to index
          repeat
          bytes 0> if
            addr bytes seq index self insert-tcp-packet
          then
          self complete-in-packets drop self first-in-packet-seq @ +
          self current-in-packet-ack !
        then
      then
      [ debug? ] [if] cr ." @@@ add-in-tcp-packet: " self in-packets. [then]
    ; define add-in-tcp-packet

    \ Insert a TCP packet entry
    :noname { addr bytes seq index self -- }
      self in-packet-count @ max-in-packets < if
        index self in-packet-count @ < if
          self in-packet-seqs index cells +
          self in-packet-seqs index 1+ cells +
          self in-packet-count @ index - cells move
          self in-packet-sizes index 1 lshift +
          self in-packet-sizes index 1+ 1 lshift +
          self in-packet-count @ index - 1 lshift move
        then
        seq self in-packet-seqs index cells + !
        bytes self in-packet-sizes index 1 lshift + h!
        1 self in-packet-count +!
        addr self in-packet-addr @ self pending-in-packet-offset @ +
        seq self first-in-packet-seq @ - + bytes move
      else
        index self in-packet-count @ < if
          index self in-packet-count @ 1- < if
            self in-packet-seqs index cells +
            self in-packet-seqs index 1+ cells +
            self in-packet-count @ index - 1- cells move
            self in-packet-sizes index 1 lshift +
            self in-packet-sizes index 1+ 1 lshift +
            self in-packet-count @ index - 1- 1 lshift move
          then
          seq self in-packet-seqs index cells + !
          bytes self in-packet-sizes index 1 lshift + h!
          addr self in-packet-addr @ self pending-in-packet-offset @ +
          seq self first-in-packet-seq @ - + bytes move
        then
      then
      0 { max-bytes }
      self in-packet-count @ 0 ?do
        self in-packet-seqs i cells + @ { current-seq }
        self in-packet-sizes i 1 lshift + h@ { current-bytes }
        current-seq self first-in-packet-seq @ - current-bytes +
        max-bytes max to max-bytes
      loop
      max-bytes self pending-in-packet-offset @ + self in-packet-offset !
      [ debug? ] [if] cr ." @@@ insert-tcp-packet: " self in-packets. [then]
    ; define insert-tcp-packet
    
    \ Add incoming UDP packet
    :noname { addr bytes self -- }
      self in-packet-offset @ bytes + self in-packet-bytes @ <=
      self in-packet-count @ max-in-packets < and if
        addr self in-packet-addr @ self in-packet-offset @ + bytes move
        bytes self in-packet-offset +!
        bytes self in-packet-sizes self in-packet-count @ 1 lshift + h!
        1 self in-packet-count +!
      then
      [ debug? ] [if] cr ." @@@ add-in-udp-packet: " self in-packets. [then]
    ; define add-in-udp-packet
    
    \ Get whether to send an ACK packet
    :noname ( self -- send? )
      dup in-packet-last-ack-sent @ swap current-in-packet-ack @ <>
    ; define in-packet-send-ack?

    \ Mark an ACK as having been sent
    :noname ( self -- )
      dup current-in-packet-ack @ swap in-packet-last-ack-sent !
    ; define in-packet-ack-sent

    \ Get the packet to ACK
    :noname ( self -- ack )
      dup complete-in-packets drop swap first-in-packet-seq @ +
    ; define in-packet-ack@

    \ Promote incoming data to pending
    :noname { self -- bytes }
      self complete-in-packets { bytes count }
      count 0> if
        self in-packet-count @ { total-count }
        self in-packets-tcp @ if
          self in-packet-seqs count cells +
          self in-packet-seqs total-count count - cells move
        then
        self in-packet-sizes count 1 lshift +
        self in-packet-sizes total-count count - 1 lshift move
        count negate self in-packet-count +!
        bytes self first-in-packet-seq +!
        bytes self pending-in-packet-offset !
      then
      bytes
      [ debug? ] [if] cr ." @@@ promote-in-packets: " self in-packets. [then]
    ; define promote-in-packets

    \ Get the complete packet size and count
    :noname { self -- bytes packet-count }
      0 0 { current-bytes count }
      self in-packet-offset @ self pending-in-packet-offset @ - { bytes }
      self in-packets-tcp @ if
        true { continue }
        begin
          count self in-packet-count @ <
          bytes current-bytes > and
          continue and
        while
          self in-packet-seqs count cells + @ { seq }
          self in-packet-sizes count 1 lshift + h@ { size }
          seq self first-in-packet-seq @ - current-bytes = if
            size +to current-bytes
            1 +to count
          else
            false to continue
          then
        repeat
      else
        bytes 0> if
          self in-packet-sizes h@ to current-bytes
          1 to count
        else
          0 to current-bytes
          0 to count
        then
      then
      current-bytes count
      [ debug? ] [if] cr ." @@@ complete-in-packets: " 2dup . . self in-packets. [then]
    ; define complete-in-packets
    
    \ Clear pending packets
    :noname { self -- }
      self pending-in-packet-offset @ { pending }
      self in-packet-addr @ pending + self in-packet-addr @
      self in-packet-offset @ pending - move
      pending negate self in-packet-offset +!
      0 self pending-in-packet-offset !
      [ debug? ] [if] cr ." @@@ clear-in-packets: " self in-packets. [then]
    ; define clear-in-packets
    
    \ Print in packets
    :noname { self -- }
      cr ." in-packets-tcp: " self in-packets-tcp @ .
      cr ." in-packet-count: " self in-packet-count @ .
      self in-packets-tcp @ if
        cr ." in-packet-seqs: "
        self in-packet-count @ 0 ?do self in-packet-seqs i cells + @ . loop
      then
      cr ." in-packet-sizes: "
      self in-packet-count @ 0 ?do self in-packet-sizes i 1 lshift + h@ . loop
      cr ." in-packet-addr: " self in-packet-addr @ h.8
      cr ." in-packet-bytes: " self in-packet-bytes @ .
      cr ." in-packet-offset: " self in-packet-offset @ .
      cr ." pending-in-packet-offset: " self pending-in-packet-offset @ .
      self in-packets-tcp @ if
        cr ." first-in-packet-seq: " self first-in-packet-seq @ .
        cr ." in-packet-last-ack-sent: " self in-packet-last-ack-sent @ .
        cr ." current-in-packet-ack: " self current-in-packet-ack @ .
      then
      cr
    ; define in-packets.

  end-implement

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

    \ The processed data size
    cell member endpoint-rx-size

    \ The delayed data size
    cell member endpoint-delayed-size

    \ The remote MAC address
    2 cells member endpoint-remote-mac-addr

    \ Endpoint lock
    lock-size member endpoint-lock

    \ Endpoint control lock
    lock-size member endpoint-ctrl-lock

    \ The received packet semaphore
    sema-size member endpoint-sema

    \ Incoming packet manager
    <in-packets> class-size member endpoint-in-packets

    \ Outgoing packet manager
    <out-packets> class-size member endpoint-out-packets
    
    \ The endpoint buffer
    max-payload-size cell align member endpoint-buf

    \ The initial local sequence number
    cell member endpoint-init-local-seq

    \ The timeout start
    cell member endpoint-timeout-start

    \ The timeout end
    cell member endpoint-timeout

    \ The retransmit count
    cell member endpoint-retransmits
    
    \ Endpoint event count
    cell member endpoint-event-count

    \ Last refresh time
    cell member endpoint-last-refresh
    
    \ Start timeout
    method start-endpoint-timeout ( self -- )

    \ Increase timeout
    method increase-endpoint-timeout ( self -- retransmit? )

    \ Get current timeout start
    method endpoint-timeout-start@ ( self -- start )
    
    \ Get current timeout end
    method endpoint-timeout@ ( self -- end )
    
    \ Broadcast an endpoint event
    method broadcast-endpoint ( self -- )

    \ Wait for an endpoint event
    method wait-endpoint ( self -- )

    \ Claim control of an endpoint
    method with-ctrl-endpoint ( xt self -- )

    \ Lock an endpoint
    method with-endpoint ( xt self -- )
    
    \ Get endpoint TCP state
    method endpoint-tcp-state@ ( self -- tcp-state )

    \ Set endpoint TCP state
    method endpoint-tcp-state! ( tcp-state self -- )

    \ Get waiting endpoint received data
    method endpoint-waiting-data? ( self -- waiting? )
    
    \ Get the number of bytes waiting on an endpoint
    method endpoint-waiting-bytes@ ( self -- bytes )

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
    
    \ Retire a pending received data
    method retire-rx-data ( self -- )

    \ Promote received data to pending
    method promote-rx-data ( self -- )

    \ Endpoint has pending received packet
    method pending-endpoint? ( self -- pending? )

    \ Endpoint is active but not pending
    method available-for-packet? ( self -- available? )

    \ Endpoint is a UDP endpoint
    method udp-endpoint? ( self -- udp? )

    \ Data is available for endpoint
    method endpoint-data-available? ( self -- available? )

    \ Ready an endpoint
    method ready-endpoint ( self -- )

    \ Mark an endpoint as available
    method endpoint-available ( self -- )

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
    
    \ Free an endpoint
    method free-endpoint ( self -- )

    \ Set an endpoint to listen on UDP
    method listen-udp ( port self -- )

    \ Set an endpoint to listen on TCP
    method listen-tcp ( port self -- )

    \ Init the TCP data stream
    method init-tcp-stream ( seq self -- )

    \ Set the initial local seq number
    method endpoint-init-local-seq! ( seq self -- )
    
    \ Get local seq number
    method endpoint-local-seq@ ( self -- seq )
    
    \ Add to local seq number
    method +endpoint-local-seq! ( increment self -- )

    \ Endpoint window size
    method endpoint-local-window@ ( self -- bytes )

    \ Add data to a TCP endpoint if possible
    method add-endpoint-tcp-data ( addr bytes seq self -- )

    \ Add data to a UDP endpoint if possible
    method add-endpoint-udp-data ( addr bytes self -- )

    \ Handle an incoming ACK
    method endpoint-ack-in ( window ack self -- )
    
    \ Get the ACK for an endpoint
    method endpoint-ack@ ( self -- ack )

    \ Get whether to send an ACK packet
    method endpoint-send-ack? ( self -- send? )

    \ Mark an ACK as having been sent
    method endpoint-ack-sent ( self -- )

    \ Start endpoint send
    method start-endpoint-send ( addr bytes self -- )

    \ Get endpoint outstanding packets
    method endpoint-send-outstanding? ( self -- outstanding? )

    \ Get endpoint sending completion
    method endpoint-send-done? ( self -- done? )

    \ Get endpoint sending readiness
    method endpoint-send-ready? ( self -- ready? )
    
    \ Get packet to send
    method get-endpoint-send-packet ( self -- addr bytes )

    \ Resend packets
    method resend-endpoint ( self -- )

    \ Signal endpoint event
    method signal-endpoint-event ( self -- )

    \ Decrement endpoint event
    method get-endpoint-event ( self -- )
    
    \ Does an endpoint have an event
    method endpoint-has-event? ( self -- event? )

    \ Is endpoint in use
    method endpoint-in-use? ( self -- in-use? )

    \ Endpoint is ready for a refresh
    method endpoint-refresh-ready? ( self -- refresh? )

    \ Reset endpoint refresh
    method reset-endpoint-refresh ( self -- )
    
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
      0 self endpoint-rx-size !
      0 self endpoint-delayed-size !
      self endpoint-buf max-payload-size
      <in-packets> self endpoint-in-packets init-object
      <out-packets> self endpoint-out-packets init-object
      self endpoint-lock init-lock
      self endpoint-ctrl-lock init-lock
      1 0 self endpoint-sema init-sema
      0 self endpoint-event-count !
      systick::systick-counter self endpoint-last-refresh !
    ; define new

    \ Start timeout
    :noname ( self -- )
      0 over endpoint-retransmits !
      systick::systick-counter over endpoint-timeout-start !
      init-timeout swap endpoint-timeout !
    ; define start-endpoint-timeout

    \ Increase timeout
    :noname ( self -- retransmit? )
      systick::systick-counter over endpoint-timeout-start !
      dup endpoint-retransmits @ max-retransmits >= if drop false exit then
      dup endpoint-timeout @ timeout-multiplier * over endpoint-timeout !
      1 swap endpoint-retransmits +!
      true
    ; define increase-endpoint-timeout

    \ Get current timeout start
    :noname ( self -- start )
      endpoint-timeout-start @
    ; define endpoint-timeout-start@
    
    \ Get current timeout end
    :noname ( self -- end )
      endpoint-timeout @
    ; define endpoint-timeout@

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
      [: { xt self }
        self [: endpoint-in-use swap endpoint-state bis! ;]
        self endpoint-lock with-lock
        xt try
        self [: endpoint-in-use swap endpoint-state bic! ;]
        self endpoint-lock with-lock
        ?raise
      ;] over endpoint-ctrl-lock with-lock
    ; define with-ctrl-endpoint

    \ Lock an endpoint
    :noname ( xt self -- )
      endpoint-lock with-lock
    ; define with-endpoint
    
    \ Get endpoint TCP state
    :noname ( self -- tcp-state )
      endpoint-state @ endpoint-tcp-state-mask and endpoint-tcp-state-lsb rshift
    ; define endpoint-tcp-state@
    
    \ Set endpoint TCP state
    :noname { tcp-state self -- }
      self endpoint-state @ endpoint-tcp-state-mask bic
      tcp-state endpoint-tcp-state-lsb lshift or self endpoint-state !
    ; define endpoint-tcp-state!

    \ Get waiting endpoint received data
    :noname ( self -- waiting? )
      [: endpoint-in-packets waiting-in-packets? ;] over endpoint-lock with-lock
    ; define endpoint-waiting-data?

    \ Get the number of bytes waiting on an endpoint
    :noname ( self -- bytes )
      [: endpoint-in-packets waiting-in-bytes@ ;] over endpoint-lock with-lock
    ; define endpoint-waiting-bytes@
    
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
    ; define endpoint-remote-mac-addr@

    \ Get destination port
    :noname ( self -- port )
      endpoint-local-port h@
    ; define endpoint-local-port@

    \ Retire a pending received data
    :noname ( self -- )
      [: endpoint-in-packets clear-in-packets ;] over endpoint-lock with-lock
    ; define retire-rx-data

    \ Promote received data to pending
    :noname ( self -- )
      [:
        dup endpoint-in-packets promote-in-packets
        swap endpoint-rx-size !
      ;] over endpoint-lock with-lock
    ; define promote-rx-data

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
        self endpoint-tcp-state@ dup TCP_LISTEN = swap TCP_SYN_RECEIVED = or
        dest-port self endpoint-local-port@ = and if
          self endpoint-tcp-state@ TCP_LISTEN = if
            src-ipv4-addr src-port self endpoint-ipv4-remote!
            TCP_SYN_RECEIVED self endpoint-tcp-state!
            mac-addr self endpoint-remote-mac-addr 2!
            0 self endpoint-event-count !
            true
          else
            self endpoint-ipv4-remote@ src-port = src-ipv4-addr = and
          then
        else
          false
        then
      ;] over endpoint-lock with-lock
    ; define try-ipv4-accept-endpoint

    \ Try to open a connection on an endpoint
    :noname ( src-port dest-ipv4-addr dest-port D: mac-addr self -- allocated? )
      [: { src-port dest-ipv4-addr dest-port D: mac-addr self }
        self try-allocate-endpoint if
          src-port self endpoint-local-port h!
          dest-ipv4-addr dest-port self endpoint-ipv4-remote!
          TCP_SYN_SENT self endpoint-tcp-state!
          mac-addr self endpoint-remote-mac-addr 2!
          0 self endpoint-event-count !
          true
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

    \ Free an endpoint
    :noname ( self -- )
      [:
        0 over endpoint-event-count !
        endpoint-active swap endpoint-state bic!
      ;] over endpoint-lock with-lock
    ; define free-endpoint

    \ Set an endpoint to listen on UDP
    :noname ( port self -- )
      [:
        0 over endpoint-event-count !
        dup endpoint-in-packets reset-in-udp-packets
        endpoint-udp over endpoint-state bis! endpoint-local-port h!
      ;] over endpoint-lock with-lock
    ; define listen-udp

    \ Set an endpoint to listen on TCP
    :noname ( port self -- )
      [: { port self }
        0 self endpoint-event-count !
        endpoint-tcp self endpoint-state bis!
        TCP_LISTEN self endpoint-tcp-state!
        port self endpoint-local-port h!
      ;] over endpoint-lock with-lock
    ; define listen-tcp

    \ Init the TCP data stream
    :noname ( seq mss window self -- )
      [: { seq mss window self }
        0 self endpoint-event-count !
        self endpoint-init-local-seq @
        mss window self endpoint-out-packets reset-out-packets
        seq self endpoint-in-packets reset-in-tcp-packets
      ;] over endpoint-lock with-lock
    ; define init-tcp-stream

    \ Set the inital local seq number
    :noname ( seq self -- )
      [: endpoint-init-local-seq ! ;] over endpoint-lock with-lock
    ; define endpoint-init-local-seq!
    
    \ Get local seq number
    :noname ( self -- seq )
      [:
        dup endpoint-tcp-state@ TCP_SYN_SENT <> if
          endpoint-out-packets current-out-packet-seq@
        else
          endpoint-init-local-seq @
        then
      ;] over endpoint-lock with-lock
    ; define endpoint-local-seq@

    \ Add to local seq number
    :noname ( increment self -- )
      [:
        endpoint-out-packets +current-out-packet-seq!
      ;] over endpoint-lock with-lock
    ; define +endpoint-local-seq!

    \ Endpoint window size
    :noname ( self -- bytes )
      [: endpoint-in-packets in-packets-window@ ;] over endpoint-lock with-lock
    ; define endpoint-local-window@

    \ Add data to a TCP endpoint if possible
    :noname ( addr bytes seq self -- )
      [: endpoint-in-packets add-in-tcp-packet ;] over endpoint-lock with-lock
    ; define add-endpoint-tcp-data

    \ Add data to a UDP endpoint if possible
    :noname ( addr bytes self -- )
      [: endpoint-in-packets add-in-udp-packet ;] over endpoint-lock with-lock
    ; define add-endpoint-udp-data

    \ Handle an incoming ACK
    :noname ( window ack self -- )
      [: endpoint-out-packets ack-packets ;] over endpoint-lock with-lock
    ; define endpoint-ack-in

    \ Get the ACK for an endpoint
    :noname ( self -- ack )
      [: endpoint-in-packets in-packet-ack@ ;] over endpoint-lock with-lock
    ; define endpoint-ack@

    \ Get whether to send an ACK packet
    :noname ( self -- send? )
      [: endpoint-in-packets in-packet-send-ack? ;] over endpoint-lock with-lock
    ; define endpoint-send-ack?

    \ Mark an ACK as having been sent
    :noname ( self -- )
      [: endpoint-in-packets in-packet-ack-sent ;] over endpoint-lock with-lock
    ; define endpoint-ack-sent

    \ Start endpoint send
    :noname ( addr bytes self -- )
      [: endpoint-out-packets start-out-packets ;] over endpoint-lock with-lock
    ; define start-endpoint-send

    \ Get endpoint outstanding packets
    :noname ( self -- outstanding? )
      [: endpoint-out-packets packets-outstanding? ;]
      over endpoint-lock with-lock
    ; define endpoint-send-outstanding?
    
    \ Get endpoint sending completion
    :noname ( self -- done? )
      [: endpoint-out-packets packets-done? ;] over endpoint-lock with-lock
    ; define endpoint-send-done?

    \ Get endpoint sending readiness
    :noname ( self -- ready? )
      [: endpoint-out-packets send-packet? ;] over endpoint-lock with-lock
    ; define endpoint-send-ready?
    
    \ Get packet to send
    :noname ( self -- addr bytes )
      [: endpoint-out-packets get-packet-to-send ;] over endpoint-lock with-lock
    ; define get-endpoint-send-packet

    \ Resend packets
    :noname ( self -- )
      [: endpoint-out-packets resend-packets ;] over endpoint-lock with-lock
    ; define resend-endpoint

    \ Signal endpoint event
    :noname ( self -- )
      [: 1 swap endpoint-event-count +! ;] over endpoint-lock with-lock
    ; define signal-endpoint-event

    \ Get endpoint event
    :noname ( self -- )
      [:
        dup endpoint-event-count @
        1- 0 max swap endpoint-event-count !
      ;] over endpoint-lock with-lock
    ; define get-endpoint-event

    \ Does an endpoint have an event
    :noname ( self -- event? )
      endpoint-event-count @ 0>
    ; define endpoint-has-event?

    \ Is endpoint in use
    :noname ( self -- in-use? )
      endpoint-in-use swap endpoint-state bit@
    ; define endpoint-in-use?

    \ Endpoint is ready for a refresh
    :noname ( self -- refresh? )
      systick::systick-counter swap endpoint-last-refresh @ - refresh-timeout >=
    ; define endpoint-refresh-ready?

    \ Reset endpoint refresh
    :noname ( self -- )
      systick::systick-counter swap endpoint-last-refresh !
    ; define reset-endpoint-refresh
    
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

    \ Outgoing buffer lock
    lock-size member outgoing-buf-lock

    \ The outgoing frame buffer
    mtu-size cell align member outgoing-buf

    \ Next endpoint index
    cell member next-endpoint-index

    \ Endpoint loop lock
    lock-size member endpoint-loop-lock

    \ Endpoint loop semaphore
    sema-size member endpoint-loop-sema

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

    \ Send a TCP SYN packet
    method send-syn ( endpoint self -- )
    
    \ Send an IPv4 TCP SYN+ACK packet
    method send-ipv4-syn-ack ( src-addr addr bytes endpoint self -- ) 

    \ Send an IPv4 TCP RST packet in response to a packet
    method send-ipv4-rst-for-packet ( src-addr addr bytes self -- )

    \ Send a generic IPv4 TCP RST packet
    method send-ipv4-rst ( endpoint self -- )
      
    \ Send a basic IPv4 TCP packet
    method send-ipv4-basic-tcp
    ( remote-addr remote-port local-port seq ack window flags D: mac-addr )
    ( self -- )
    
    \ Process an IPv4 TCP SYN+ACK packet
    method process-ipv4-syn-ack-packet ( src-addr addr bytes self -- )

    \ Process an IPv4 ACK packet
    method process-ipv4-ack-packet ( src-addr addr bytes self -- )

    \ Process an IPv4 FIN+ACK packet
    method process-ipv4-fin-ack-packet ( src-addr addr bytes self -- )

    \ Process an IPv4 ACK packet in the general case
    method process-ipv4-basic-ack ( addr bytes endpoint self -- )

    \ Process an IPv4 ACK packet in TCP_SYN_RECEIVED state
    method process-ipv4-ack-syn-received ( addr bytes endpoint self -- )
    
    \ Process an IPv4 ACK packet in TCP_ESTABLISHED state
    method process-ipv4-ack-established ( addr bytes endpoint self -- )
    
    \ Process an IPv4 ACK packet in TCP_FIN_WAIT_1 state
    method process-ipv4-ack-fin-wait-1 ( addr bytes endpoint self -- )
    
    \ Process an IPv4 ACK packet in TCP_FIN_WAIT_2 state
    method process-ipv4-ack-fin-wait-2 ( addr bytes endpoint self -- )
    
    \ Process an IPv4 ACK packet in TCP_CLOSE_WAIT state
    method process-ipv4-ack-close-wait ( addr bytes endpoint self -- )
    
    \ Process an IPv4 ACK packet in TCP_LAST_ACK state
    method process-ipv4-ack-last-ack ( addr bytes endpoint self -- )

    \ Process an errant IPv4 ACK packet
    method send-ipv4-rst-for-ack ( addr bytes endpoint self -- )
    
    \ Process an IPv4 FIN packet
    method process-ipv4-fin-packet ( src-addr addr bytes self -- )

    \ Process an IPv4 FIN packet for a TCP_ESTABLISHED state
    method process-ipv4-fin-established ( addr bytes endpoint self -- )

    \ Process an IPv4 FIN packet for a TCP_FIN_WAIT_2 state
    method process-ipv4-fin-fin-wait-2 ( addr bytes endpoint self -- )

    \ Process an unexpected IPv4 FIN packet
    method process-ipv4-unexpected-fin ( addr bytes endpoint self -- )

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

    \ Mark an endpoint as done
    method endpoint-done ( endpoint self -- )

    \ Get a UDP endpoint to listen on
    method allocate-udp-listen-endpoint ( port self -- endpoint success? )

    \ Get a TCP endpoint to listen on
    method allocate-tcp-listen-endpoint ( port self -- endpoint success? )

    \ Get a TCP endpoint and connect to a host with it
    method allocate-tcp-connect-ipv4-endpoint
    ( src-port dest-ipv4-addr dest-port self -- endpoint success? )

    \ Close a UDP endpoint
    method close-udp-endpoint ( endpoint self -- )

    \ Close a TCP endpoint
    method close-tcp-endpoint ( endpoint self -- )

    \ Wait for a TCP endpoint to close
    method wait-endpoint-closed ( endpoint self -- )

    \ Close an established conection
    method close-tcp-established ( endpoint self -- )

    \ Send a reply FIN packet
    method send-fin-reply ( endpoint self -- )
    
    \ Send data on a TCP endpoint
    method send-tcp-endpoint ( addr bytes endpoint self -- )

    \ Send a data ACK packet
    method send-data-ack ( addr bytes endpoint self -- )

    \ Refresh an interface
    method refresh-interface ( self -- )
    
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
      0 self next-endpoint-index !
      self endpoint-loop-lock init-lock
      max-endpoints 0 self endpoint-loop-sema init-sema
      <fragment-collect> self fragment-collect init-object
      <address-map> self address-map init-object
      <dns-cache> self dns-cache init-object
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
      [ debug? ] [if]
        cr ." SENT: "
        addr addr bytes + dump
      [then]
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
        >r 2drop 2drop drop r>
      endcase
    ; define process-ipv4-packet

    \ Process an IPv4 TCP packet
    :noname { src-addr protocol addr bytes self -- }
      bytes tcp-header-size >= if
        addr full-tcp-header-size bytes > if exit then

        [ debug? ] [if] addr tcp. [then]
        
        src-addr addr bytes self
        addr tcp-flags c@ TCP_CONTROL and
        case
          TCP_SYN of process-ipv4-syn-packet endof
          [ TCP_SYN TCP_ACK or ] literal of process-ipv4-syn-ack-packet endof
          TCP_ACK of process-ipv4-ack-packet endof
          TCP_FIN of process-ipv4-fin-packet endof
          [ TCP_FIN TCP_ACK or ] literal of process-ipv4-fin-ack-packet endof
          TCP_RST of process-ipv4-rst-packet endof
        endcase
      then
    ; define process-ipv4-tcp-packet

    \ Find a listening IPv4 TCP endpoint
    :noname { src-addr addr bytes self -- endpoint found? }
      max-endpoints 0 ?do
        self intf-endpoints <endpoint> class-size i * + { endpoint }
        src-addr
        addr tcp-src-port hunaligned@ rev16
        addr tcp-dest-port hunaligned@ rev16
        src-addr self address-map lookup-mac-addr-by-ipv4 if
          endpoint try-ipv4-accept-endpoint if endpoint true unloop exit then
        else
          2drop 2drop drop \ Should never happen
        then
      loop
      0 false
    ; define find-listen-ipv4-endpoint

    \ Find a connecting/connected IPv4 TCP endpoint
    :noname { src-addr addr bytes self -- endpoint found? }
      max-endpoints 0 ?do
        self intf-endpoints <endpoint> class-size i * + { endpoint }
        src-addr
        addr tcp-src-port hunaligned@ rev16
        addr tcp-dest-port hunaligned@ rev16
        endpoint match-ipv4-connect? if endpoint true unloop exit then
      loop
      0 false
    ; define find-connect-ipv4-endpoint
    
    \ Process an IPv4 TCP SYN packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-listen-ipv4-endpoint if
        [: swap send-ipv4-syn-ack ;] over with-endpoint
      else
        drop send-ipv4-rst-for-packet
      then
    ; define process-ipv4-syn-packet

    \ Send a TCP SYN packet
    :noname { endpoint self -- }
      TCP_SYN_SENT endpoint endpoint-tcp-state!
      rng::random endpoint endpoint-init-local-seq!
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@ 1-
      0
      [ mtu-size ethernet-header-size - ipv4-header-size - tcp-header-size - ]
      literal
      TCP_SYN
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
    ; define send-syn

    \ Send an IPv4 TCP SYN+ACK packet
    :noname { src-addr addr bytes endpoint self -- }
      rng::random endpoint endpoint-init-local-seq!
      addr tcp-seq-no unaligned@ rev 1+
      addr bytes tcp-mss@ not if
        drop [ mtu-size ethernet-header-size - ipv4-header-size -
        tcp-header-size - ] literal
      then
      addr tcp-window-size hunaligned@ rev16
      endpoint init-tcp-stream
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      1 endpoint +endpoint-local-seq!
      endpoint endpoint-ack@
      endpoint endpoint-local-window@
      [ TCP_SYN TCP_ACK or ] literal
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
      TCP_SYN_RECEIVED endpoint endpoint-tcp-state!
      endpoint endpoint-ack-sent
    ; define send-ipv4-syn-ack

    \ Send an IPv4 TCP RST packet in response to a packet
    :noname ( src-addr addr bytes self -- ) { self }
      drop
      over { src-addr }
      dup tcp-src-port hunaligned@ rev16 swap
      dup tcp-dest-port hunaligned@ rev16 swap
      rng::random swap
      tcp-seq-no unaligned@ rev
      0
      TCP_RST
      src-addr self address-map lookup-mac-addr-by-ipv4 if
        self send-ipv4-basic-tcp
      else
        2drop 2drop 2drop 2drop \ Should never happen
      then
    ; define send-ipv4-rst-for-packet

    \ Send a generic IPv4 TCP RST packet
    :noname { endpoint self -- }
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      0
      0
      0
      TCP_RST
      endpoint endpoint-remote-mac-addr @
      self send-ipv4-basic-tcp
    ; define send-ipv4-rst

    \ Send a basic IPv4 TCP packet
    :noname
      ( remote-addr remote-port local-port seq ack window flags D: mac-addr )
      ( self -- )
      -rot 9 pick PROTOCOL_TCP tcp-header-size
      ( self D: mac-addr remote-addr protocol bytes )
      [: { remote-addr remote-port local-port seq ack window flags self buf }
        local-port rev16 buf tcp-src-port hunaligned!
        remote-port rev16 buf tcp-dest-port hunaligned!
        seq rev buf tcp-seq-no unaligned!
        ack rev buf tcp-ack-no unaligned!
        [ 5 4 lshift ] literal buf tcp-data-offset c!
        flags buf tcp-flags c!
        window rev16 buf tcp-window-size hunaligned!
        0 buf tcp-urgent-ptr hunaligned!
        self intf-ipv4-addr@ remote-addr buf tcp-header-size 0 tcp-checksum
        compute-tcp-checksum rev16 buf tcp-checksum hunaligned!
        [ debug? ] [if] cr ." @@@@@ SENDING TCP:" buf tcp. [then]
        true
      ;] 6 pick construct-and-send-ipv4-packet drop
    ; define send-ipv4-basic-tcp
    
    \ Process an IPv4 TCP SYN+ACK packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-connect-ipv4-endpoint not if
        drop send-ipv4-rst-for-packet exit
      then
      [: { src-addr addr bytes self endpoint }
        endpoint endpoint-tcp-state@ TCP_SYN_SENT <> if
          src-addr addr bytes self send-ipv4-rst-for-packet exit
        then
        addr tcp-ack-no unaligned@ rev
        endpoint endpoint-local-seq@ <> if
          src-addr addr bytes self send-ipv4-rst-for-packet exit
        then
        addr tcp-seq-no unaligned@ rev 1+
        addr bytes tcp-mss@ not if
          drop [ mtu-size ethernet-header-size - ipv4-header-size -
          tcp-header-size - ] literal
        then
        addr tcp-window-size hunaligned@ rev16
        endpoint init-tcp-stream
        endpoint endpoint-ipv4-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        endpoint endpoint-ack@
        endpoint endpoint-local-window@
        TCP_ACK
        endpoint endpoint-remote-mac-addr@
        self send-ipv4-basic-tcp
        TCP_ESTABLISHED endpoint endpoint-tcp-state!
        endpoint self put-ready-endpoint
      ;] over with-endpoint
    ; define process-ipv4-syn-ack-packet

    \ Process an IPv4 ACK packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-connect-ipv4-endpoint not if
        drop send-ipv4-rst-for-packet exit
      then
      [: { src-addr addr bytes self endpoint }
        endpoint endpoint-tcp-state@ { state }
        endpoint endpoint-waiting-bytes@ { waiting-bytes }
        addr bytes endpoint self
        state case
          TCP_SYN_RECEIVED of process-ipv4-ack-syn-received endof
          TCP_ESTABLISHED of process-ipv4-ack-established endof
          TCP_FIN_WAIT_1 of process-ipv4-ack-fin-wait-1 endof
          TCP_FIN_WAIT_2 of process-ipv4-ack-fin-wait-2 endof
          TCP_CLOSE_WAIT of process-ipv4-ack-close-wait endof
          TCP_LAST_ACK of process-ipv4-ack-last-ack endof
          drop send-ipv4-rst-for-ack exit
        endcase
        endpoint endpoint-waiting-bytes@ waiting-bytes <>
        endpoint endpoint-tcp-state@ state <> or if
          endpoint self put-ready-endpoint
        else
          endpoint broadcast-endpoint
        then
      ;] over with-endpoint
    ; define process-ipv4-ack-packet

    \ Process an IPv4 FIN+ACK packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-connect-ipv4-endpoint not if
        drop send-ipv4-rst-for-packet exit
      then
      [: { src-addr addr bytes self endpoint }
        addr bytes endpoint self
        endpoint endpoint-waiting-bytes@ { waiting-bytes }
        endpoint endpoint-tcp-state@ { state }
        state case
          TCP_ESTABLISHED of process-ipv4-ack-established endof
          TCP_FIN_WAIT_1 of process-ipv4-ack-fin-wait-1 endof
          TCP_FIN_WAIT_2 of process-ipv4-ack-fin-wait-2 endof
          TCP_CLOSE_WAIT of process-ipv4-ack-close-wait endof
          TCP_LAST_ACK of process-ipv4-ack-last-ack endof
          >r send-ipv4-rst-for-ack r>
        endcase
        addr bytes endpoint self
        state case
          TCP_ESTABLISHED of process-ipv4-fin-established endof
          TCP_FIN_WAIT_2 of process-ipv4-fin-fin-wait-2 endof
        endcase
        endpoint endpoint-waiting-bytes@ waiting-bytes <>
        endpoint endpoint-tcp-state@ state <> or if
          endpoint self put-ready-endpoint
        else
          endpoint broadcast-endpoint
        then
      ;] over with-endpoint
    ; define process-ipv4-fin-ack-packet

    \ Process an IPv4 ACK packet in the general case
    :noname { addr bytes endpoint self -- }
      addr full-tcp-header-size { header-size }
      addr header-size + bytes header-size - addr tcp-seq-no unaligned@ rev
      endpoint add-endpoint-tcp-data
      addr tcp-window-size hunaligned@ rev16
      addr tcp-ack-no unaligned@ rev
      endpoint endpoint-ack-in
      endpoint endpoint-send-ack? if
        endpoint endpoint-ipv4-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        endpoint endpoint-ack@
        endpoint endpoint-local-window@
        TCP_ACK
        endpoint endpoint-remote-mac-addr@
        self send-ipv4-basic-tcp
        endpoint endpoint-ack-sent
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
        TCP_FIN_WAIT_2 endpoint endpoint-tcp-state!
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
      addr tcp-ack-no hunaligned@ rev
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
    :noname ( src-addr addr bytes self -- )
      2over 2over find-connect-ipv4-endpoint not if
        drop send-ipv4-rst-for-packet exit
      then
      [: { src-addr addr bytes self endpoint }
        addr bytes endpoint self
        endpoint endpoint-tcp-state@ { state }
        state case
          TCP_ESTABLISHED of process-ipv4-fin-established endof
          TCP_FIN_WAIT_2 of process-ipv4-fin-fin-wait-2 endof
          process-ipv4-unexpected-fin
        endcase
        endpoint endpoint-tcp-state@ state <> or if
          endpoint self put-ready-endpoint
        else
          endpoint broadcast-endpoint
        then
      ;] over with-endpoint
    ; define process-ipv4-fin-packet

    \ Process an IPv4 FIN packet for a TCP_ESTABLISHED state
    :noname { addr bytes endpoint self -- }
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      addr tcp-seq-no unaligned@ rev
      endpoint endpoint-local-window@
      TCP_ACK
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
      TCP_CLOSE_WAIT endpoint endpoint-tcp-state!
    ; define process-ipv4-fin-established

    \ Process an IPv4 FIN packet for a TCP_FIN_WAIT_2 state
    :noname { addr bytes endpoint self -- }
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      addr tcp-seq-no unaligned@ rev
      endpoint endpoint-local-window@
      TCP_ACK
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
      TCP_TIME_WAIT endpoint endpoint-tcp-state!
    ; define process-ipv4-fin-fin-wait-2

    \ Process an unexpected IPv4 FIN packet
    :noname { addr bytes endpoint self -- }
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      addr tcp-seq-no unaligned@ rev
      endpoint endpoint-local-window@
      TCP_ACK
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
      TCP_CLOSING endpoint endpoint-tcp-state!
    ; define process-ipv4-unexpected-fin

    \ Process an IPv4 RST packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-connect-ipv4-endpoint if
        [: { src-addr addr bytes self endpoint }
          TCP_CLOSED endpoint endpoint-tcp-state!
          self put-ready-endpoint
        ;] over with-endpoint
      else
        drop 2drop 2drop
      then
    ; define process-ipv4-rst-packet
          
    \ Process an IPv4 UDP packet
    :noname { src-addr protocol addr bytes self -- }
      bytes udp-header-size >= if

        [ debug? ] [if]
          cr ." Source address: " src-addr ipv4.
          cr ." Total length: " addr udp-total-len h@ rev16 .
          cr ." Actual length: " bytes .
          cr ." Source port: " addr udp-src-port h@ rev16 .
          cr ." Destination port: " addr udp-dest-port h@ rev16 .
        [then]
        
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
            endpoint endpoint-local-port@ addr udp-dest-port h@ rev16 = and if
              src-addr addr udp-src-port h@ rev16 endpoint endpoint-ipv4-remote!
              addr udp-header-size + bytes udp-header-size -
              endpoint add-endpoint-udp-data
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
        addr icmp-checksum hunaligned@ rev16 <> if exit then
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
          buf icmp-checksum hunaligned!
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
    :noname ( endpoint self -- )
      [: { endpoint self }
        endpoint endpoint-has-event? not if
          endpoint signal-endpoint-event
          self endpoint-loop-sema give
        else
        then
        endpoint broadcast-endpoint
      ;] 2 pick with-endpoint
    ; define put-ready-endpoint

    \ Dequeue a ready receiving IP endpoint
    :noname ( self -- endpoint )
      dup endpoint-loop-sema take
      [: { self }
        self next-endpoint-index @ { index }
        begin
          self intf-endpoints <endpoint> class-size index * + { endpoint }
          endpoint [: { endpoint }
            endpoint endpoint-has-event? if
              endpoint get-endpoint-event
              endpoint promote-rx-data
              endpoint true
            else
              false
            then
          ;] over with-endpoint
          dup not if index 1+ max-endpoints umod to index then
        until
        self next-endpoint-index @ 1+ max-endpoints umod
        self next-endpoint-index !
      ;] over endpoint-loop-lock with-lock
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
    :noname ( endpoint self -- )
      [:
        [: { endpoint self }
          endpoint retire-rx-data
        ;] 2 pick with-endpoint
      ;] over endpoint-loop-lock with-lock
    ; define endpoint-done
    
    \ Get a UDP endpoint to listen on
    :noname ( port self -- endpoint success? )
      allocate-endpoint if tuck listen-udp true else nip false then
    ; define allocate-udp-listen-endpoint

    \ Get a TCP endpoint to listen on
    :noname ( port self -- endpoint success? )
      allocate-endpoint if tuck listen-tcp true else nip false then
    ; define allocate-tcp-listen-endpoint

    \ Get a TCP endpoint and connect to a host with it
    :noname { src-port dest-ipv4-addr dest-port self -- endpoint success? }
      dest-ipv4-addr self resolve-ipv4-addr-mac-addr not if
        2drop 0 false exit
      then { D: mac-addr }
      max-endpoints 0 ?do
        self intf-endpoints <endpoint> class-size i * + { endpoint }
        src-port dest-ipv4-addr dest-port mac-addr endpoint
        try-ipv4-connect-endpoint if
          endpoint self [: { endpoint self }
            endpoint self send-syn
          ;] endpoint with-endpoint
          endpoint true unloop exit
        then
      loop
      0 false
    ; define allocate-tcp-connect-ipv4-endpoint   

    \ Close a UDP endpoint
    :noname ( endpoint self -- )
      drop free-endpoint
    ; define close-udp-endpoint

    \ Close a TCP endpoint
    :noname ( endpoint self -- )
      [: { endpoint self }
        endpoint endpoint-tcp-state@ case
          TCP_SYN_SENT of endpoint self send-ipv4-rst then
          TCP_SYN_RECEIVED of endpoint self close-tcp-established then
          TCP_ESTABLISHED of endpoint self close-tcp-established then
          TCP_FIN_WAIT_1 of endpoint wait-endpoint-closed then
          TCP_FIN_WAIT_2 of endpoint wait-endpoint-closed then
          TCP_CLOSE_WAIT of endpoint self send-fin-reply then
        endcase
      ;] 2 pick with-ctrl-endpoint
    ; define close-tcp-endpoint

    \ Wait for a TCP endpoint to close
    :noname { endpoint self -- }
      systick::systick-counter { start }
      begin
        endpoint endpoint-tcp-state@ case
          TCP_FIN_WAIT_1 of true endof
          TCP_FIN_WAIT_2 of true endof
          TCP_LAST_ACK of true endof
          false swap
        endcase
      while
        start close-timeout + systick::systick-counter - 0 max { timeout }
        task::timeout @ { old-timeout }
        timeout task::timeout !
        endpoint ['] wait-endpoint try
        old-timeout task::timeout !
        dup ['] task::x-timed-out = if 2drop true 0 else false swap then
        ?raise
        if
          TCP_CLOSED endpoint endpoint-tcp-state!
          endpoint free-endpoint
          exit
        then
      repeat
      TCP_CLOSED endpoint endpoint-tcp-state!
      endpoint free-endpoint
    ; define wait-endpoint-closed

    \ Close an established conection
    :noname { endpoint self -- }
      endpoint self [: { endpoint self }
        endpoint endpoint-ipv4-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        0
        endpoint endpoint-local-window@
        TCP_FIN
        endpoint endpoint-remote-mac-addr@
        self send-ipv4-basic-tcp
        TCP_FIN_WAIT_1 endpoint endpoint-tcp-state!
      ;] endpoint with-endpoint
      endpoint self wait-endpoint-closed
    ; define close-tcp-established

    \ Send a reply FIN packet
    :noname { endpoint self -- }
      endpoint self [: { endpoint self }
        endpoint endpoint-ipv4-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        0
        endpoint endpoint-local-window@
        TCP_FIN
        endpoint endpoint-remote-mac-addr@
        self send-ipv4-basic-tcp
        TCP_LAST_ACK endpoint endpoint-tcp-state!
      ;] endpoint with-endpoint
      endpoint self wait-endpoint-closed
    ; define send-fin-reply

    \ Send data on a TCP endpoint
    :noname ( addr bytes endpoint self -- )
      2 pick 0= if 2drop 2drop exit then
      [: { addr bytes endpoint self }
        addr bytes endpoint [:
          dup endpoint-tcp-state@
          dup TCP_ESTABLISHED = swap TCP_CLOSE_WAIT = or if
            start-endpoint-send true
          else
            2drop drop false
          then
        ;] over with-endpoint if
          endpoint start-endpoint-timeout
          begin
            endpoint self [: { endpoint self }
              endpoint endpoint-send-done? not if
                endpoint endpoint-send-outstanding?
                systick::systick-counter endpoint endpoint-timeout-start@ -
                endpoint endpoint-timeout@ > and if
                  endpoint increase-endpoint-timeout drop
                  endpoint resend-endpoint
                else
                  endpoint endpoint-send-outstanding? not if
                    endpoint start-endpoint-timeout
                  then
                then
                endpoint endpoint-tcp-state@
                dup TCP_ESTABLISHED = swap TCP_CLOSE_WAIT = or if
                  endpoint endpoint-send-ready? if
                    endpoint get-endpoint-send-packet
                    endpoint self send-data-ack
                    false true
                  else
                    true true
                  then
                else
                  false
                then
              else
                false
              then
            ;] endpoint with-endpoint
          while
            if
              task::timeout @ { old-timeout }
              endpoint endpoint-timeout@
              systick::systick-counter endpoint endpoint-timeout-start@ - -
              0 max task::timeout !
              endpoint ['] wait-endpoint try
              dup ['] task::x-timed-out = if 2drop 0 then
              old-timeout task::timeout !
              ?raise
            then
          repeat
        then
      ;] 2 pick with-ctrl-endpoint
    ; define send-tcp-endpoint

    \ Send a data ACK packet
    :noname ( addr bytes endpoint self -- )
      over endpoint-remote-mac-addr@
      3 pick endpoint-ipv4-remote@ drop
      PROTOCOL_TCP
      6 pick tcp-header-size +
      ( addr bytes endpoint self D: mac-addr remote-addr protocol bytes )
      [: { addr bytes endpoint self buf }
        endpoint endpoint-local-port@ rev16 buf tcp-src-port hunaligned!
        endpoint endpoint-ipv4-remote@ nip rev16 buf tcp-dest-port hunaligned!
        endpoint endpoint-local-seq@ rev buf tcp-seq-no unaligned!
        endpoint endpoint-ack@ rev buf tcp-ack-no unaligned!
        [ 5 4 lshift ] literal buf tcp-data-offset c!
        [ TCP_ACK TCP_PSH or ] literal buf tcp-flags c!
        endpoint endpoint-local-window@ rev16 buf tcp-window-size hunaligned!
        0 buf tcp-urgent-ptr hunaligned!
        addr buf tcp-header-size + bytes move
        self intf-ipv4-addr@
        endpoint endpoint-ipv4-remote@ drop
        buf tcp-header-size bytes + 0 tcp-checksum
        compute-tcp-checksum rev16 buf tcp-checksum hunaligned!
        [ debug? ] [if] cr ." @@@@@ SENDING TCP WITH DATA:" buf tcp. [then]
        true
      ;] 6 pick construct-and-send-ipv4-packet drop
    ; define send-data-ack

    \ Refresh an interface
    :noname { self -- }
      max-endpoints 0 ?do
        self intf-endpoints <endpoint> class-size i * +
        dup endpoint-refresh-ready? if
          self [: { endpoint self }
            endpoint endpoint-tcp-state@ { state }
            [ debug? ] [if] cr ." ENDPOINT " endpoint h.8 ."  STATE: " state . [then]
            state TCP_ESTABLISHED = state TCP_SYN_RECEIVED = or if
              endpoint endpoint-ipv4-remote@
              endpoint endpoint-local-port@
              endpoint endpoint-local-seq@
              endpoint endpoint-ack@
              endpoint endpoint-local-window@
              state case
                TCP_ESTABLISHED of TCP_ACK endof
                TCP_SYN_RECEIVED of [ TCP_SYN TCP_ACK or ] literal endof
              endcase
              endpoint endpoint-remote-mac-addr@
              self send-ipv4-basic-tcp
              endpoint endpoint-ack-sent
              endpoint reset-endpoint-refresh
            then
          ;] 2 pick with-endpoint
        else
          drop
        then
      loop
    ; define refresh-interface

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

    \ Handle a refresh
    :noname ( self -- )
      ip-interface @ refresh-interface
    ; define handle-refresh
    
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