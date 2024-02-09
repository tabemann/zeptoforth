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
  net-consts import
  net-config import
  net-misc import
  lock import
  sema import
  chan import
  heap import

  \ Oversized frame exception
  : x-oversized-frame ( -- ) ." oversized frame" cr ;

  \ Invalid DNS name
  : x-invalid-dns-name ( -- ) ." invalid DNS name" cr ;

  \ Validate DNS name
  : validate-dns-name { addr len -- }
    len 0 > averts x-invalid-dns-name
    len max-dns-name-len <= averts x-invalid-dns-name
    addr c@ [char] . <> averts x-invalid-dns-name
    addr len 1- + c@ [char] . <> averts x-invalid-dns-name
    len 1- 0 ?do
      addr i + c@ dup $20 > averts x-invalid-dns-name
      [char] . = if addr i 1+ + c@ [char] . <> averts x-invalid-dns-name then
    loop
  ;

  \ zeptoIP internals
  begin-module net-internal

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

    \ The packet buffer offset
    cell member out-packet-offset

    \ The last sent packet buffer sequence number
    cell member last-out-packet-seq

    \ The initial sent packet buffer sequence number
    cell member init-out-packet-seq

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

    \ Clear sending outgoing packets
    method clear-out-packets ( self -- )
    
    \ Acknowledge packets
    method ack-packets ( window ack self -- )

    \ The ACKed packet buffer offset
    method acked-out-packet-offset@ ( self -- offset )

    \ Get info on packet to send
    method get-packet-to-send ( self -- addr bytes )

    \ Get the current sequence number
    method current-out-packet-seq@ ( self -- seq )

    \ Get the size of a packet to send
    method next-packet-size@ ( self -- bytes )

    \ Are there outstanding packets?
    method packets-outstanding? ( self -- outstanding? )
    
    \ Are we done sending packets?
    method packets-done? ( self -- done? )

    \ Have we sent our last packet?
    method packets-last? ( self -- last? )
    
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
      0 self out-packet-offset !
      seq self first-out-packet-seq !
      seq self init-out-packet-seq !
      seq self last-out-packet-seq !
      mss self out-packet-mss !
      window self out-packet-window !
      0 self out-packet-count !
      [ debug? ] [if]
        self [: cr ." === reset-out-packets:" out-packets. ;]
        debug-hook execute
      [then]
    ; define reset-out-packets
    
    \ Start sending outgoing packets
    :noname { addr bytes self -- }
      addr self out-packet-addr !
      bytes self out-packet-bytes !
      0 self out-packet-offset !
      0 self out-packet-count !
      self first-out-packet-seq @ self init-out-packet-seq !
      [ debug? ] [if]
        self [: cr ." === start-out-packets:" out-packets. ;]
        debug-hook execute
      [then]
    ; define start-out-packets

    \ Clear sending outgoing packets
    :noname { self -- }
      self out-packet-count @ 0> if
        self out-packet-seqs self out-packet-count @ 1- cells + @
        self first-out-packet-seq !
      then
      0 self out-packet-addr !
      0 self out-packet-bytes !
      0 self out-packet-offset !
      0 self out-packet-count !
      self first-out-packet-seq @ self init-out-packet-seq !
      [ debug? ] [if]
        self [: cr ." === clear-out-packets:" out-packets. ;]
        debug-hook execute
      [then]
    ; define clear-out-packets
    
    \ Acknowledge packets
    :noname { window ack self -- }
      window self out-packet-window !
      self first-out-packet-seq @ { first }
      ack first - 0< if exit then
      self last-out-packet-seq @ { last }
      self init-out-packet-seq @ { init }
      last ack - last first - <= ack init - last init - <= and if
        0 first { count new-first }
        self out-packet-count @ 0 ?do
          self out-packet-seqs i cells + @ { current-seq }
          last current-seq - last ack - >= if
            1 +to count
            current-seq to new-first
          else
            leave
          then
        loop
        self out-packet-sizes count 1 lshift + self out-packet-sizes
        self out-packet-count @ count - 1 lshift move
        self out-packet-seqs count cells + self out-packet-seqs
        self out-packet-count @ count - cells move
        count negate self out-packet-count +!
        count 0> if
          new-first self first-out-packet-seq !
        else
          ack self first-out-packet-seq !
        then
      then
      [ debug? ] [if]
        self [: cr ." === ack-packets:" out-packets. ;] debug-hook execute
      [then]
    ; define ack-packets

    \ The ACKed packet buffer offset
    :noname ( self -- offset )
      dup first-out-packet-seq @ swap init-out-packet-seq @ -
    ; define acked-out-packet-offset@

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
      self first-out-packet-seq @ { first }
      self last-out-packet-seq @ first - this-seq first - < if
        this-seq self last-out-packet-seq !
      then
      1 self out-packet-count +!
      self out-packet-addr @ self out-packet-offset @ + out-packet-size -
      out-packet-size
      [ debug? ] [if]
        self [: cr ." === get-packet-to-send:" out-packets. ;]
        debug-hook execute
      [then]
    ; define get-packet-to-send

    \ Get the current sequence number
    :noname { self -- seq }
      self out-packet-count @ { count }
      count 0> if
        self out-packet-seqs count 1- cells + @
        self out-packet-sizes count 1- 1 lshift + h@ -
      else
        self first-out-packet-seq @
      then
      [ debug? ] [if]
        self [: cr ." === current-out-packet-seq:" out-packets. ;]
        debug-hook execute
      [then]
    ; define current-out-packet-seq@

    \ Get the size of a packet to send
    :noname ( self -- bytes )
      dup out-packet-bytes @ over out-packet-offset @ - over
      out-packet-window @ small-send-bytes max min
      swap out-packet-mss @ min
      [ mtu-size ethernet-header-size - ipv4-header-size - tcp-header-size - ]
      literal min
      [ debug? ] [if]
        [: cr ." === next-packet-size: " dup . ;] debug-hook execute
      [then]
    ; define next-packet-size@

    \ Are there outstanding packets?
    :noname ( self -- outstanding? )
      dup acked-out-packet-offset@ swap out-packet-offset @ <>
      [ debug? ] [if]
        [: cr ." === packets-outstanding?: " dup . ;] debug-hook execute
      [then]
    ; define packets-outstanding?
    
    \ Are we done sending packets?
    :noname ( self -- done? )
      dup out-packet-count @ 0=
      swap dup acked-out-packet-offset@
      swap out-packet-bytes @ = and
      [ debug? ] [if]
        [: cr ." === packets-done?: " dup . ;] debug-hook execute
      [then]
    ; define packets-done?

    \ Have we sent our last packet
    :noname ( self -- last? )
      [ debug? ] [if]
      dup [:
        cr ." === packets-last?: bytes: " dup out-packet-bytes @ .
        ." offset: " out-packet-offset @ .
      ;] debug-hook execute
      [then]
      dup out-packet-bytes @ swap out-packet-offset @ =
    ; define packets-last?
    
    \ Can we issue a new packet?
    :noname ( self -- send? )
      dup out-packet-offset @ over acked-out-packet-offset@ -
      over out-packet-window @ <
      over out-packet-offset @ 2 pick out-packet-bytes @ < and
      swap out-packet-count @ max-out-packets < and
      [ debug? ] [if]
        [: cr ." === send-packet?: " dup . ;] debug-hook execute
      [then]
    ; define send-packet?

    \ Mark packets as to be resent
    :noname ( self -- )
      [ debug? ] [if] dup [then]
      dup acked-out-packet-offset@ over out-packet-offset !
      0 swap out-packet-count !
      [ debug? ] [if]
        [: cr ." === resend-packets: " out-packets. ;] debug-hook execute
      [then]
    ; define resend-packets

    \ Print out packets
    :noname { self -- }
      \ cr ." out-packet-count: " self out-packet-count @ .
      \ cr ." out-packet-seqs: "
      \ self out-packet-count @ 0 ?do self out-packet-seqs i cells + @ . loop
      \ cr ." out-packet-sizes: "
      \ self out-packet-count @ 0 ?do self out-packet-sizes i 1 lshift + h@ . loop
      \ cr ." out-packet-addr: " self out-packet-addr @ h.8
      \ cr ." out-packet-bytes: " self out-packet-bytes @ .
      \ cr ." acked-out-packet-offset: " self acked-out-packet-offset@ .
      \ cr ." out-packet-offset: " self out-packet-offset @ .
      \ cr ." first-out-packet-seq: " self first-out-packet-seq @ .
      \ cr ." out-packet-mss: " self out-packet-mss @ .
      \ cr ." out-packet-window: " self out-packet-window @ .
      \ cr
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

    \ The incoming UDP packets source IP address
    max-in-packets cells member in-packet-ipv4-addrs

    \ The incoming UDP packets source ports
    max-in-packets 1 lshift member in-packet-ports

    \ The pending UDP packet source IP address
    cell member in-packet-current-ipv4-addr

    \ The pending UDP packet source port
    cell member in-packet-current-port

    \ The buffer incoming packets are being written to
    cell member in-packet-addr

    \ The incoming packet buffer size
    cell member in-packet-bytes

    \ The full packet buffer offset
    cell member in-packet-offset

    \ The pending packet buffer offset
    cell member pending-in-packet-offset

    \ The pushed packet buffer offset
    cell member pushed-in-packet-offset

    \ The sequence number of the first incoming packet
    cell member first-in-packet-seq

    \ Last ACK sent
    cell member in-packet-last-ack-sent

    \ Current ACK
    cell member current-in-packet-ack

    \ Last seq
    cell member last-in-packet-seq
    
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

    \ Get the incoming packets UDP remote address and port
    method in-packets-udp-remote@ ( self -- ipv4-addr port )
    
    \ Push data
    \ method push-packets ( seq self -- )

    \ Join complete packets
    method join-complete-packets ( self -- )
    
    \ Add incoming TCP packet
    method add-in-tcp-packet ( addr bytes push? seq self -- )

    \ Insert a TCP packet entry
    method insert-tcp-packet ( addr bytes seq index self -- )

    \ Add incoming UDP packet
    method add-in-udp-packet ( addr bytes src-addr src-port self -- )
    
    \ Get whether to send an ACK packet
    method in-packet-send-ack? ( self -- send? )

    \ Mark an ACK as having been sent
    method in-packet-ack-sent ( self -- )

    \ Get the packet to ACK
    method in-packet-ack@ ( self -- ack )

    \ Get the last seq
    method in-packet-seq@ ( self -- seq )

    \ Promote incoming data to pending
    method promote-in-packets ( self -- bytes )

    \ Get the complete packet size and count
    method complete-in-packets ( self -- bytes packet-count )

    \ Get whether there are missing packets
    method missing-in-packets? ( self -- missing-packets? )
    
    \ Clear contiguous packets
    method clear-in-packets ( self -- )

    \ Get whether there is waiting data
    method in-packets-waiting? ( self -- waiting? )

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
      self in-packet-ipv4-addrs max-in-packets cells 0 fill
      self in-packet-ports max-in-packets 1 lshift 0 fill
      0 self in-packet-current-ipv4-addr !
      0 self in-packet-current-port h!
      0 self in-packet-offset !
      0 self pending-in-packet-offset !
      0 self pushed-in-packet-offset !
      seq self first-in-packet-seq !
      seq self in-packet-last-ack-sent !
      seq self current-in-packet-ack !
      seq self last-in-packet-seq !
      [ debug? ] [if]
        self [: cr ." @@@ reset-in-tcp-packets: " in-packets. ;]
        debug-hook execute
      [then]
    ; define reset-in-tcp-packets

    \ Reset the incoming UDP packet record
    :noname { self -- }
      false self in-packets-tcp !
      0 self in-packet-count !
      self in-packet-seqs max-in-packets cells 0 fill
      self in-packet-sizes max-in-packets 1 lshift 0 fill
      self in-packet-ipv4-addrs max-in-packets cells 0 fill
      self in-packet-ports max-in-packets 1 lshift 0 fill
      0 self in-packet-current-ipv4-addr !
      0 self in-packet-current-port h!
      0 self in-packet-offset !
      0 self pending-in-packet-offset !
      0 self pushed-in-packet-offset !
      0 self first-in-packet-seq !
      0 self in-packet-last-ack-sent !
      0 self current-in-packet-ack !
      0 self last-in-packet-seq !
      [ debug? ] [if]
        self [: cr ." @@@ reset-in-udp-packets: " in-packets. ;]
        debug-hook execute
      [then]
    ; define reset-in-udp-packets

    \ Get TCP window
    :noname { self -- }
      self in-packet-count @ max-in-packets < if
        self in-packet-bytes @ self in-packet-offset @ -
      else
        0
      then
      [ debug? ] [if]
        self [: cr ." @@@ in-packets-window@: " in-packets. ;]
        debug-hook execute
      [then]
    ; define in-packets-window@

    \ Get whether there are waiting in packets
    :noname ( self -- waiting? )
      pushed-in-packet-offset @ 0>
      [ debug? ] [if]
        dup [: cr ." @@@ waiting-in-packets?: " . ;]
        debug-hook execute
      [then]
    ; define waiting-in-packets?

    \ Get the amount of waiting data for an endpoint
    :noname ( self -- bytes )
      pushed-in-packet-offset @
      [ debug? ] [if]
        dup [: cr ." @@@ waiting-in-bytes@: " . ;]
        debug-hook execute
      [then]
    ; define waiting-in-bytes@

    \ Join complete packets
    :noname { self -- }
      self in-packet-count @ 0 { total-count count }
      total-count 0 ?do
        self in-packet-seqs i cells + @ { current-seq }
        self in-packet-sizes i 1 lshift + h@ { current-size }
        current-seq self first-in-packet-seq @ = if
          current-size self first-in-packet-seq +!
          current-size self pushed-in-packet-offset +!

          [ debug? ] [if]
            current-size current-seq i self first-in-packet-seq @
            [: cr ." first: " . ." i: " . ." seq: " . ." size: " . ;]
            debug-hook execute
          [then]
          
          1 +to count
        else
          leave
        then
      loop
      self in-packet-seqs count cells + self in-packet-seqs count cells move
      self in-packet-sizes count 1 lshift + self in-packet-sizes count 1 lshift
      move
      count negate self in-packet-count +!

      [ debug? ] [if]
        self in-packet-offset @ self pushed-in-packet-offset @
        self in-packet-count @
        [: cr ." count: " . ." pushed: " . ." total: " . ;]
        debug-hook execute
      [then]
      
    ; define join-complete-packets

    \ Add incoming TCP packet
    :noname { addr bytes push? seq self -- }        
      seq self last-in-packet-seq !

      seq self first-in-packet-seq @ - { diff }

      [ debug? ] [if]
        diff seq self first-in-packet-seq @
        [: cr ." first: " . ." seq: " . ." diff: " . ;] debug-hook execute
      [then]

      diff 0< if
        self first-in-packet-seq @ diff + self in-packet-last-ack-sent !
        diff negate bytes <= if
          diff negate +to addr
          diff +to bytes
          self first-in-packet-seq @ to seq
          0 to diff
        else
          exit
        then
      then

      bytes seq { init-bytes init-seq }

      bytes 0> if
        
        diff 0>=
        diff bytes + self pushed-in-packet-offset @ +
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
            diff current-diff <
            diff bytes + current-diff <= if
              addr bytes seq index self insert-tcp-packet
              0 to bytes
            else
              diff current-diff <
              diff bytes + current-diff > and if
                current-seq seq - bytes min { part-bytes }
                addr part-bytes seq index self insert-tcp-packet
                part-bytes +to seq
                part-bytes +to diff
                part-bytes +to addr
                part-bytes negate +to bytes
              else
                diff bytes + current-diff current-size + >=
                diff current-diff current-size + < and if
                  diff bytes + current-diff current-size +
                  min diff - { part-bytes }
                  part-bytes +to seq
                  part-bytes +to diff
                  part-bytes +to addr
                  part-bytes negate +to bytes
                then
                1 +to index
              then
            then
          repeat
          bytes 0> if
            addr bytes seq index self insert-tcp-packet
          then
        then        
      then

      self join-complete-packets

      self first-in-packet-seq @ self current-in-packet-ack !

      push? if
        self current-in-packet-ack @ 1- self in-packet-last-ack-sent !
      then
      
      [ debug? ] [if]
        push? init-bytes 0<> or if

          self in-packet-offset @
          self pushed-in-packet-offset @
          self pending-in-packet-offset @
          self in-packet-last-ack-sent @
          self current-in-packet-ack @
          self in-packet-count @
          self first-in-packet-seq @
          init-bytes
          [:
            cr
            ." bytes: " .
            ." first-seq: " .
            ." count: " .
            ." current-ack: " .
            ." last-ack: " .
            ." pending: " .
            ." pushed: " .
            ." offset: " .
          ;] debug-hook execute

        then
      [then]
        
      
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
        addr self in-packet-addr @ self pushed-in-packet-offset @ +
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
          addr self in-packet-addr @ self pushed-in-packet-offset @ +
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
      max-bytes self pushed-in-packet-offset @ + self in-packet-offset !
      [ debug? ] [if]
        self [: cr ." @@@ insert-tcp-packet: " in-packets. ;]
        debug-hook execute
      [then]
    ; define insert-tcp-packet
    
    \ Add incoming UDP packet
    :noname { addr bytes src-addr src-port self -- }
      self in-packet-offset @ bytes + self in-packet-bytes @ <=
      self in-packet-count @ max-in-packets < and if
        addr self in-packet-addr @ self in-packet-offset @ + bytes move
        bytes self in-packet-offset +!
        bytes self in-packet-sizes self in-packet-count @ 1 lshift + h!
        src-addr self in-packet-ipv4-addrs self in-packet-count @ cells + !
        src-port self in-packet-ports self in-packet-count @ 1 lshift + h!
        1 self in-packet-count +!
      then
      [ debug? ] [if]
        self [: cr ." @@@ add-in-udp-packet: " in-packets. ;]
        debug-hook execute
      [then]
    ; define add-in-udp-packet
    
    \ Get the incoming packets UDP remote address and port
    :noname ( self -- ipv4-addr port )
      dup in-packet-current-ipv4-addr @
      swap in-packet-current-port h@
    ; define in-packets-udp-remote@

    \ Get whether to send an ACK packet
    :noname ( self -- send? )
      dup in-packet-last-ack-sent @ swap current-in-packet-ack @ <>
      [ debug? ] [if]
        dup [: cr ." @@@ in-packet-send-ack: " . ;]
        debug-hook execute
      [then]
    ; define in-packet-send-ack?

    \ Mark an ACK as having been sent
    :noname ( self -- )
      dup current-in-packet-ack @ swap in-packet-last-ack-sent !
      [ debug? ] [if]
        [: cr ." @@@ in-packet-ack-sent" ;] debug-hook execute
      [then]
    ; define in-packet-ack-sent

    \ Get the packet to ACK
    :noname ( self -- ack )
      current-in-packet-ack @
      [ debug? ] [if]
        dup [: ." @@@ in-packet-ack@: " . ;] debug-hook execute
      [then]
    ; define in-packet-ack@

    \ Get the last packet's seq
    :noname ( self -- seq )
      last-in-packet-seq @
    ; define in-packet-seq@
    
    \ Get whether there is waiting data
    :noname ( self -- waiting? )
      dup pending-in-packet-offset @ swap in-packet-offset @ <>
    ; define in-packets-waiting?

    \ Promote incoming data to pending
    :noname { self -- bytes }
      self clear-in-packets
      self in-packets-tcp @ if
        self pushed-in-packet-offset @ self pending-in-packet-offset !
      else
        self in-packet-count @ 0> if
          self in-packet-sizes h@ self pending-in-packet-offset !
          self in-packet-ipv4-addrs @ self in-packet-current-ipv4-addr !
          self in-packet-ports h@ self in-packet-current-port !
          -1 self in-packet-count +!
          self in-packet-count @ 0> if
            self in-packet-sizes 2 + self in-packet-sizes
            self in-packet-count @ 1 lshift move
            self in-packet-ipv4-addrs cell+ self in-packet-ipv4-addrs
            self in-packet-count @ cells move
            self in-packet-ports 2 + self in-packet-ports
            self in-packet-count @ 1 lshift move
          then
        else
          0 self pending-in-packet-offset !
          0 self in-packet-current-ipv4-addr !
          0 self in-packet-current-port !
        then
      then
      self pending-in-packet-offset @

      [ debug? ] [if]
        self in-packet-offset @
        self pushed-in-packet-offset @
        self pending-in-packet-offset @
        [: cr ." promote pending: " . ." pushed: " . ." total: " . ;]
        debug-hook execute
      [then]

      [ debug? ] [if]
        self [: cr ." @@@ promote-in-packets: " in-packets. ;]
        debug-hook execute
      [then]
    ; define promote-in-packets

    \ Get the complete packet size and count
    :noname { self -- bytes packet-count }
      0 0 { current-bytes count }
      self in-packet-offset @ self pushed-in-packet-offset @ - { bytes }
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
      [ debug? ] [if]
        2dup self -rot [: cr ." @@@ complete-in-packets: " . . in-packets. ;]
        debug-hook execute
      [then]
    ; define complete-in-packets

    \ Get whether there are missing packets
    :noname { self -- missing-packets? }
      self in-packet-offset @ self pushed-in-packet-offset @ -
      self complete-in-packets drop <>
      [ debug? ] [if]
        dup [: cr ." @@@ missing-in-packets?: " . ;]
        debug-hook execute
      [then]
    ; define missing-in-packets?

    \ Clear pending packets
    :noname { self -- }

      [ debug? ] [if]
        self [: cr ." @@@ BEFORE clear-in-packets: " in-packets. ;]
        debug-hook execute
      [then]
      
      self pending-in-packet-offset @ { pending }
      self in-packets-tcp @ if
        pending negate self pushed-in-packet-offset +!
      then
      self in-packet-addr @ pending + self in-packet-addr @
      self in-packet-offset @ pending - move
      pending negate self in-packet-offset +!
      0 self pending-in-packet-offset !
      0 self in-packet-current-ipv4-addr !
      0 self in-packet-current-port h!

      [ debug? ] [if]
        self in-packet-offset @
        self pushed-in-packet-offset @
        [: cr ." clear-pending pushed: " . ." total: " . ;]
        debug-hook execute
      [then]
      
      [ debug? ] [if]
        self [: cr ." @@@ clear-in-packets: " in-packets. ;]
        debug-hook execute
      [then]
    ; define clear-in-packets

    \ Print in packets
    :noname { self -- }
      \ cr ." in-packets-tcp: " self in-packets-tcp @ .
      \ cr ." in-packet-count: " self in-packet-count @ .
      \ self in-packets-tcp @ if
      \   cr ." in-packet-seqs: "
      \   self in-packet-count @ 0 ?do self in-packet-seqs i cells + @ . loop
      \ then
      \ cr ." in-packet-sizes: "
      \ self in-packet-count @ 0 ?do self in-packet-sizes i 1 lshift + h@ . loop
      \ cr ." in-packet-addr: " self in-packet-addr @ h.8
      \ cr ." in-packet-bytes: " self in-packet-bytes @ .
      \ cr ." in-packet-offset: " self in-packet-offset @ .
      \ cr ." pending-in-packet-offset: " self pending-in-packet-offset @ .
      \ cr ." pushed-in-packet-offset: " self pushed-in-packet-offset @ .
      \ self in-packets-tcp @ if
      \   cr ." first-in-packet-seq: " self first-in-packet-seq @ .
      \   cr ." in-packet-last-ack-sent: " self in-packet-last-ack-sent @ .
      \   cr ." current-in-packet-ack: " self current-in-packet-ack @ .
      \ then
      \ cr
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

    \ Evict a DNS entry
    method evict-dns ( c-addr bytes self -- )
    
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
        c-addr bytes validate-dns-name
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

    \ Evict a DNS entry
    :noname ( c-addr bytes self -- )
      [: { c-addr bytes self }
        max-dns-cache 0 ?do
          self cached-dns-names i cells + @ ?dup if
            count c-addr bytes equal-case-strings?
            -1 self cached-dns-idents i cells + @ = and if
              self newest-dns-age @ { current-age }
              max-dns-cache 0 ?do
                self cached-dns-names i cells + @ if
                  self cached-dns-ages i cells + @ { age }
                  age self newest-dns-age @ -
                  current-age self newest-dns-age @ - < if
                    age to current-age
                  then
                then
              loop
              current-age 1- self cached-dns-ages i cells + !
              self cached-dns-names i cells + @ self dns-cache-name-heap free
              0 self cached-dns-names i cells + !
              unloop exit
            then
          then
        loop
      ;] over dns-cache-lock with-lock
    ; define evict-dns

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
        ipv4-addr 0<> if
          max-addresses 0 ?do
            self mapped-ipv4-addrs i cells + @ ipv4-addr = if
              mac-addr ipv4-addr i self save-mac-addr-at-index  unloop exit
            then
          loop
          self oldest-mac-addr-index { index }
          mac-addr ipv4-addr index self save-mac-addr-at-index
        then
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

  end-module> import
  
  \ The endpoint class
  <object> begin-class <endpoint>

    continue-module net-internal
      
      \ Is the endpoint state
      cell member endpoint-state

      \ The endpoint queue state
      cell member endpoint-queue-state

      \ The current endpoint ID
      cell member endpoint-id
      
      \ The remote IPv4 address
      cell member endpoint-remote-ipv4-addr

      \ The remote port
      cell member endpoint-remote-port
      
      \ The local port
      cell member endpoint-local-port

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
      max-endpoint-in-size cell align member endpoint-buf

      \ The initial local sequence number
      cell member endpoint-init-local-seq

      \ The timeout start
      cell member endpoint-timeout-start

      \ The timeout end
      cell member endpoint-timeout

      \ The retransmit count
      cell member endpoint-retransmits
      
      \ Endpoint event signaled?
      cell member endpoint-event

      \ Last refresh time
      cell member endpoint-last-refresh

      \ Endpoint refresh timeout
      cell member endpoint-refresh-timeout

      \ Endpoint refresh count
      cell member endpoint-refreshes

      \ Get the current endpoint ID
      method endpoint-id@ ( self -- id )
      
      \ Start timeout
      method start-endpoint-timeout ( self -- )

      \ Increase timeout
      method increase-endpoint-timeout ( self -- retransmit? )

      \ Get current timeout start
      method endpoint-timeout-start@ ( self -- start )
      
      \ Get current timeout end
      method endpoint-timeout@ ( self -- end )
      
      \ Wake an endpoint
      method wake-endpoint ( self -- )

      \ Wait for an endpoint event
      method wait-endpoint ( self -- )

      \ Is an endpoint pending
      method endpoint-pending? ( self -- pending? )

      \ Is an endpoint enqueued
      method endpoint-enqueued? ( self -- enqueued? )

      \ Mark endpoint as pending
      method mark-endpoint-pending ( self -- )

      \ Clear endpoint pending status
      method clear-endpoint-pending ( self -- )

      \ Clear endpoint enqueued status
      method clear-endpoint-enqueued ( self -- )
      
      \ Claim control of an endpoint
      method with-ctrl-endpoint ( xt self -- )

      \ Lock an endpoint
      method with-endpoint ( xt self -- )
      
      \ Set endpoint TCP state
      method endpoint-tcp-state! ( tcp-state self -- )

      \ Get waiting endpoint received data
      method endpoint-waiting-data? ( self -- waiting? )
      
      \ Get the number of bytes waiting on an endpoint
      method endpoint-waiting-bytes@ ( self -- bytes )

      \ Set endpoint received data
      method endpoint-rx-data! ( addr bytes self -- )

      \ Set endpoint source
      method endpoint-ipv4-remote! ( ipv4-addr port self -- )

      \ Get endpoint remote MAC address
      method endpoint-remote-mac-addr@ ( self -- D: mac-addr )

      \ Set local port
      method endpoint-local-port! ( port self -- )
      
      \ Reset local port
      method reset-endpoint-local-port ( self -- )
      
      \ Retire a pending received data
      method retire-rx-data ( self -- )

      \ Promote received data to pending
      method promote-rx-data ( self -- )

      \ Endpoint has pending received packet
      method pending-endpoint? ( self -- pending? )

      \ Endpoint is active but not pending
      method available-for-packet? ( self -- available? )

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
      
      \ Set an endpoint to listen on UDP
      method listen-udp ( port self -- )

      \ Set an endpoint to listen on TCP
      method listen-tcp ( port self -- )

      \ Init the TCP data stream
      method init-tcp-stream ( seq self -- )

      \ Set the initial local seq number
      method endpoint-init-local-seq! ( seq self -- )

      \ Get the initial local seq number
      method endpoint-init-local-seq@ ( self -- seq )
      
      \ Get local seq number
      method endpoint-local-seq@ ( self -- seq )
      
      \ Endpoint window size
      method endpoint-local-window@ ( self -- bytes )

      \ Add data to a TCP endpoint if possible
      method add-endpoint-tcp-data ( addr bytes push? seq self -- )

      \ Add data to a UDP endpoint if possible
      method add-endpoint-udp-data ( addr bytes src-addr src-port self -- )

      \ Handle an incoming ACK
      method endpoint-ack-in ( window ack self -- )
      
      \ Get the ACK for an endpoint
      method endpoint-ack@ ( self -- ack )

      \ Get the last remote seq
      method endpoint-remote-seq@ ( self -- seq )

      \ Get whether to send an ACK packet
      method endpoint-send-ack? ( self -- send? )

      \ Mark an ACK as having been sent
      method endpoint-ack-sent ( self -- )

      \ Start endpoint send
      method start-endpoint-send ( addr bytes self -- )

      \ Clear endpoint send
      method clear-endpoint-send ( self -- )
      
      \ Get endpoint outstanding packets
      method endpoint-send-outstanding? ( self -- outstanding? )

      \ Get endpoint sending completion
      method endpoint-send-done? ( self -- done? )

      \ Have we sent our last packet?
      method endpoint-send-last? ( self -- last? )
      
      \ Get endpoint sending readiness
      method endpoint-send-ready? ( self -- ready? )
      
      \ Get packet to send
      method get-endpoint-send-packet ( self -- addr bytes )

      \ Resend packets
      method resend-endpoint ( self -- )

      \ Is endpoint in use
      method endpoint-in-use? ( self -- in-use? )

      \ Endpoint is ready for a refresh
      method endpoint-refresh-ready? ( self -- refresh? )

      \ Reset endpoint refresh
      method reset-endpoint-refresh ( self -- )

      \ Advance last endpoint refresh
      method advance-endpoint-refresh ( self -- )
      
      \ Get whether there are missing packets
      method missing-endpoint-packets? ( self -- missing-packets? )

      \ Get next endpoint refresh timeout
      method next-endpoint-refresh-timeout@ ( self -- timeout )

      \ Escalate the next endpoint refresh
      method escalate-endpoint-refresh ( self -- )

      \ Signal an endpoint event
      method signal-endpoint-event ( self -- )

      \ Clear an endpoint event
      method clear-endpoint-event ( self -- )

      \ Does an endpoint have an event?
      method endpoint-has-event? ( self -- has-event? )
    
      \ Free an endpoint
      method free-endpoint ( self -- )

    end-module
    
    \ Get endpoint TCP state
    method endpoint-tcp-state@ ( self -- tcp-state )

    \ Get endpoint received data
    method endpoint-rx-data@ ( self -- addr bytes )

    \ Get endpoint source
    method endpoint-ipv4-remote@ ( self -- ipv4-addr port )
      
    \ Endpoint is a UDP endpoint
    method udp-endpoint? ( self -- udp? )

    \ Get local port
    method endpoint-local-port@ ( self -- port )

    \ Get whether there is waiting data
    method waiting-rx-data? ( self -- waiting? )

  end-class

  \ Implement the endpoint class
  <endpoint> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      0 self endpoint-state !
      0 self endpoint-queue-state !
      0 self endpoint-remote-ipv4-addr !
      0 self endpoint-remote-port !
      0 self endpoint-local-port !
      0 self endpoint-rx-size !
      0 self endpoint-delayed-size !
      self endpoint-buf max-endpoint-in-size
      <in-packets> self endpoint-in-packets init-object
      <out-packets> self endpoint-out-packets init-object
      self endpoint-lock init-lock
      self endpoint-ctrl-lock init-lock
      no-sema-limit 0 self endpoint-sema init-sema
      false self endpoint-event !
      0 self endpoint-id !
      systick::systick-counter self endpoint-last-refresh !
    ; define new

    \ Get the current endpoint ID
    :noname ( self -- id )
      endpoint-id @
    ; define endpoint-id@

    \ Start timeout
    :noname ( self -- )
      0 over endpoint-retransmits !
      systick::systick-counter over endpoint-timeout-start !
      init-timeout swap endpoint-timeout !
    ; define start-endpoint-timeout

    \ Increase timeout
    :noname { self -- }
      systick::systick-counter self endpoint-timeout-start !
      self endpoint-retransmits @ 1+ max-retransmits min
      self endpoint-retransmits !
      init-timeout s>f timeout-multiplier s>f
      self endpoint-retransmits @ fi** f* f>s self endpoint-timeout !
    ; define increase-endpoint-timeout

    \ Get current timeout start
    :noname ( self -- start )
      endpoint-timeout-start @
    ; define endpoint-timeout-start@
    
    \ Get current timeout end
    :noname ( self -- end )
      endpoint-timeout @
    ; define endpoint-timeout@

    \ Wake an endpoint
    :noname ( self -- )
      endpoint-sema give
    ; define wake-endpoint

    \ Wait for an endpoint event
    :noname ( self -- )
      endpoint-sema take
    ; define wait-endpoint

    \ Is an endpoint pending
    :noname ( self -- pending? )
      endpoint-pending swap endpoint-queue-state bit@
    ; define endpoint-pending?

    \ Is an endpoint enqueued
    :noname ( self -- enqueued? )
      endpoint-enqueued swap endpoint-queue-state bit@
    ; define endpoint-enqueued?
    
    \ Mark endpoint as pending
    :noname ( self -- )
      [ endpoint-pending endpoint-enqueued or ] literal
      swap endpoint-queue-state bis!
    ; define mark-endpoint-pending

    \ Clear endpoint pending status
    :noname ( self -- )
      endpoint-pending swap endpoint-queue-state bic!
    ; define clear-endpoint-pending

    \ Clear endpoint enqueued status
    :noname ( self -- )
      endpoint-enqueued swap endpoint-queue-state bic!
    ; define clear-endpoint-enqueued
    
    \ Claim control of an endpoint
    :noname ( xt self -- )
      endpoint-ctrl-lock with-lock
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
      tcp-state self [: { tcp-state self }
        self endpoint-state @ endpoint-tcp-state-mask bic
        tcp-state endpoint-tcp-state-lsb lshift or self endpoint-state !
      ;] self endpoint-lock with-lock
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
      dup udp-endpoint? not if
        dup endpoint-remote-ipv4-addr @
        swap endpoint-remote-port @
      else
        endpoint-in-packets in-packets-udp-remote@
      then
    ; define endpoint-ipv4-remote@
    
    \ Set endpoint source
    :noname { ipv4-addr port self -- }
      ipv4-addr self endpoint-remote-ipv4-addr !
      port self endpoint-remote-port !
    ; define endpoint-ipv4-remote!

    \ Get endpoint remote MAC address
    :noname ( self -- D: mac-addr )
      endpoint-remote-mac-addr 2@
    ; define endpoint-remote-mac-addr@
    
    \ Set local port
    :noname { port self -- }
      port 0> if
        port self endpoint-local-port !
      else
        get-ephemeral-port negate self endpoint-local-port !
      then
    ; define endpoint-local-port!

    \ Get local port
    :noname ( self -- port )
      endpoint-local-port @ abs
    ; define endpoint-local-port@

    \ Reset local port
    :noname ( self -- )
      [: { self }
        self endpoint-local-port @ 0< if
          get-ephemeral-port negate self endpoint-local-port !
        then
      ;] over endpoint-lock with-lock
    ; define reset-endpoint-local-port
    
    \ Retire a pending received data
    :noname ( self -- )
      [:
        dup endpoint-in-packets clear-in-packets
        0 swap endpoint-rx-size !
      ;] over endpoint-lock with-lock
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
            false self endpoint-event !
            self reset-endpoint-refresh
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
          src-port self endpoint-local-port!
          dest-ipv4-addr dest-port self endpoint-ipv4-remote!
          TCP_SYN_SENT self endpoint-tcp-state!
          mac-addr self endpoint-remote-mac-addr 2!
          false self endpoint-event !
          self reset-endpoint-refresh
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
        0 over endpoint-event !
        1 over endpoint-id +!
        endpoint-active swap endpoint-state bic!
      ;] over endpoint-lock with-lock
    ; define free-endpoint

    \ Set an endpoint to listen on UDP
    :noname ( port self -- )
      [:
        0 over endpoint-event !
        dup endpoint-in-packets reset-in-udp-packets
        endpoint-tcp over endpoint-state bic!
        endpoint-udp over endpoint-state bis! endpoint-local-port!
      ;] over endpoint-lock with-lock
    ; define listen-udp

    \ Set an endpoint to listen on TCP
    :noname ( port self -- )
      [: { port self }
        false self endpoint-event !
        endpoint-udp self endpoint-state bic!
        endpoint-tcp self endpoint-state bis!
        TCP_LISTEN self endpoint-tcp-state!
        port self endpoint-local-port!
      ;] over endpoint-lock with-lock
    ; define listen-tcp

    \ Init the TCP data stream
    :noname ( seq mss window self -- )
      [: { seq mss window self }
        false self endpoint-event !
        self endpoint-init-local-seq @
        mss window self endpoint-out-packets reset-out-packets
        seq self endpoint-in-packets reset-in-tcp-packets
      ;] over endpoint-lock with-lock
    ; define init-tcp-stream

    \ Set the inital local seq number
    :noname ( seq self -- )
      endpoint-init-local-seq !
    ; define endpoint-init-local-seq!

    \ Get the initial local seq number
    :noname ( self -- seq )
      endpoint-init-local-seq @
    ; define endpoint-init-local-seq@

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

    \ Endpoint window size
    :noname ( self -- bytes )
      [: endpoint-in-packets in-packets-window@ ;] over endpoint-lock with-lock
    ; define endpoint-local-window@

    \ Add data to a TCP endpoint if possible
    :noname ( addr bytes push? seq self -- )
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

    \ Get the last remote seq
    :noname ( self -- seq )
      [: endpoint-in-packets in-packet-seq@ ;] over endpoint-lock with-lock
    ; define endpoint-remote-seq@

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

    \ Clear endpoint send
    :noname ( self -- )
      [: endpoint-out-packets clear-out-packets ;] over endpoint-lock with-lock
    ; define clear-endpoint-send

    \ Get endpoint outstanding packets
    :noname ( self -- outstanding? )
      [: endpoint-out-packets packets-outstanding? ;]
      over endpoint-lock with-lock
    ; define endpoint-send-outstanding?
    
    \ Get endpoint sending completion
    :noname ( self -- done? )
      [: endpoint-out-packets packets-done? ;] over endpoint-lock with-lock
    ; define endpoint-send-done?

    \ Get endpoint last packet state
    :noname ( self -- last? )
      [: endpoint-out-packets packets-last? ;] over endpoint-lock with-lock
    ; define endpoint-send-last?

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
      true swap endpoint-event !
    ; define signal-endpoint-event

    \ Clear endpoint event
    :noname ( self -- )
      false swap endpoint-event !
    ; define clear-endpoint-event

    \ Does an endpoint have an event
    :noname ( self -- event? )
      endpoint-event @
    ; define endpoint-has-event?

    \ Is endpoint in use
    :noname ( self -- in-use? )
      endpoint-in-use swap endpoint-state bit@
    ; define endpoint-in-use?

    \ Endpoint is ready for a refresh
    :noname ( self -- refresh? )
      [: { self }
        self endpoint-tcp-state@ { state }
        \ state TCP_ESTABLISHED = if
        \   self endpoint-in-packets in-packet-send-ack? if
        \     true
        \   else
        \     systick::systick-counter self endpoint-last-refresh @ -
        \     self next-endpoint-refresh-timeout@ >=
        \   then
        \ else
          state case
            TCP_ESTABLISHED of true endof
            TCP_CLOSE_WAIT of true endof
            TCP_SYN_SENT of true endof
            TCP_SYN_RECEIVED of true endof
            TCP_FIN_WAIT_1 of true endof
            TCP_LAST_ACK of true endof
            false swap
          endcase if
            systick::systick-counter self endpoint-last-refresh @ -
            self next-endpoint-refresh-timeout@ >=
          else
            false
          then
        \ then
      ;] over endpoint-lock with-lock
    ; define endpoint-refresh-ready?

    \ Reset endpoint refresh
    :noname ( self -- )
      [:
        systick::systick-counter over endpoint-last-refresh !
        0 swap endpoint-refreshes !
      ;] over endpoint-lock with-lock
    ; define reset-endpoint-refresh

    \ Advance last endpoint refresh
    :noname ( self -- )
      systick::systick-counter swap endpoint-last-refresh !
    ; define advance-endpoint-refresh
    
    \ Get whether there are missing packets
    :noname ( self -- missing-packets? )
      [: endpoint-in-packets missing-in-packets? ;] over endpoint-lock with-lock
    ; define missing-endpoint-packets?

    \ Get next endpoint refresh timeout
    :noname ( self -- timeout )
      [: { self }
        self endpoint-tcp-state@ case
          TCP_ESTABLISHED of
            \ self missing-endpoint-packets? not if
            \   self reset-endpoint-refresh
            \   established-no-missing-refresh-timeout
            \ else
              established-init-refresh-timeout s>f
              established-refresh-timeout-multiplier s>f
              self endpoint-refreshes @ fi** f* f>s
            \ then
          endof
          TCP_CLOSE_WAIT of
            \ self missing-endpoint-packets? not if
            \   self reset-endpoint-refresh
            \   established-no-missing-refresh-timeout
            \ else
              established-init-refresh-timeout s>f
              established-refresh-timeout-multiplier s>f
              self endpoint-refreshes @ fi** f* f>s
            \ then
          endof
          TCP_SYN_SENT of
            syn-sent-init-refresh-timeout s>f
            syn-sent-refresh-timeout-multiplier s>f
            self endpoint-refreshes @ fi** f* f>s
          endof
          TCP_SYN_RECEIVED of
            syn-ack-sent-init-refresh-timeout s>f
            syn-ack-sent-refresh-timeout-multiplier s>f
            self endpoint-refreshes @ fi** f* f>s
          endof
          TCP_FIN_WAIT_1 of
            fin-wait-1-init-refresh-timeout s>f
            fin-wait-1-refresh-timeout-multiplier s>f
            self endpoint-refreshes @ fi** f* f>s
          endof
          TCP_LAST_ACK of
            last-ack-init-refresh-timeout s>f
            last-ack-refresh-timeout-multiplier s>f
            self endpoint-refreshes @ fi** f* f>s
          endof
          0 swap
        endcase
      ;] over endpoint-lock with-lock
    ; define next-endpoint-refresh-timeout@

    \ Escalate the next endpoint refresh
    :noname ( self -- )
      [: { self }
        self endpoint-tcp-state@ case
          TCP_ESTABLISHED of
            \ self missing-endpoint-packets? not if
            \   self reset-endpoint-refresh
            \ else
              self endpoint-refreshes @ 1+ established-max-refreshes min
              self endpoint-refreshes !
            \ then
          endof
          TCP_CLOSE_WAIT of
            \ self missing-endpoint-packets? not if
            \   self reset-endpoint-refresh
            \ else
              self endpoint-refreshes @ 1+ established-max-refreshes min
              self endpoint-refreshes !
            \ then
          endof
          TCP_SYN_SENT of
            self endpoint-refreshes @ 1+ syn-sent-max-refreshes min
            self endpoint-refreshes !
          endof
          TCP_SYN_RECEIVED of
            self endpoint-refreshes @ 1+ syn-ack-sent-max-refreshes min
            self endpoint-refreshes !
          endof
          TCP_FIN_WAIT_1 of
            self endpoint-refreshes @ 1+ fin-wait-1-max-refreshes min
            self endpoint-refreshes !
          endof
          TCP_LAST_ACK of
            self endpoint-refreshes @ 1+ last-ack-max-refreshes min
            self endpoint-refreshes !
          endof
        endcase
      ;] over endpoint-lock with-lock
    ; define escalate-endpoint-refresh

    \ Get whether there is waiting data
    :noname ( self -- waiting? )
      [: endpoint-in-packets in-packets-waiting? ;] over endpoint-lock with-lock
    ; define waiting-rx-data?

  end-implement
  
  continue-module net-internal
      
    \ TCP_WAIT class
    <object> begin-class <time-wait>

      \ TCP wait remote MAC address
      2 cells member time-wait-remote-mac-addr
      
      \ TCP wait remote IPv4 address
      cell member time-wait-remote-ipv4-addr
      
      \ Local port
      2 member time-wait-local-port
      
      \ Remote port
      2 member time-wait-remote-port

      \ Start time
      cell member time-wait-start-time

      \ Local seq
      cell member time-wait-local-seq

      \ Remote ack
      cell member time-wait-ack

      \ Local window
      cell member time-wait-local-window
      
      \ Set a TCP_WAIT record
      method time-wait! ( seq endpoint self -- )

    end-class

  end-module  

  <time-wait> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
    ; define new

    \ Set a TIME_WAIT record
    :noname ( seq endpoint self -- )
      [: { seq endpoint self }
        endpoint endpoint-ipv4-remote@
        self time-wait-remote-port h!
        self time-wait-remote-ipv4-addr !
        endpoint endpoint-local-port@ self time-wait-local-port h!
        endpoint endpoint-remote-mac-addr@ self time-wait-remote-mac-addr 2!
        endpoint endpoint-local-seq@ 1+ self time-wait-local-seq !
        seq 1+ self time-wait-ack !
        endpoint endpoint-local-window@ self time-wait-local-window !
        systick::systick-counter self time-wait-start-time !
      ;] 2 pick with-endpoint
    ; define time-wait!

  end-implement

  continue-module net-internal

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

  end-module

  \ The interface class
  <object> begin-class <interface>

    continue-module net-internal
      
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

      \ MAC address resolution semaphore
      sema-size member mac-addr-resolve-sema

      \ Curreht DHCP xid
      cell member current-dhcp-xid

      \ Current DHCP server MAC address
      2 cells member dhcp-server-mac-addr
      
      \ Current DHCP server IPv4 address
      cell member dhcp-server-ipv4-addr

      \ Current DHCP requested IPv4 address
      cell member dhcp-req-ipv4-addr

      \ DHCP discovery state
      cell member dhcp-discover-state

      \ DHCP remaining ARP attempt count
      cell member dhcp-remaining-arp-attempts
      
      \ DHCP discovery stage start
      cell member dhcp-discover-stage-start

      \ DHCP real renewal interval
      cell member dhcp-real-renew-interval
      
      \ DHCP renewal interval
      cell member dhcp-renew-interval

      \ DHCP rebinding interval
      cell member dhcp-rebind-interval

      \ DHCP real renewal start time
      cell member dhcp-real-renew-start
      
      \ DHCP renewal start time
      cell member dhcp-renew-start

      \ DHCP rebinding start time
      cell member dhcp-rebind-start

      \ Provisional DHCP IPv4 netmask
      cell member prov-ipv4-netmask

      \ Provisional DHCP IPv4 gateway
      cell member prov-gateway-ipv4-addr

      \ Provisional DHCP DNS server
      cell member prov-dns-server-ipv4-addr

      \ Provisional DHCP renewal interval
      cell member prov-dhcp-renew-interval
      
      \ DHCP semaphore
      sema-size member dhcp-sema
      
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

      \ Endpoint queue lock
      lock-size member endpoint-queue-lock

      \ Endpoint queue semaphore
      sema-size member endpoint-queue-sema

      \ Endpoint queue index
      cell member endpoint-queue-index

      \ TIME_WAIT interval start
      cell member time-wait-interval-start
      
      \ TIME_WAIT list lock
      lock-size member time-wait-list-lock

      \ TIME_WAIT list start
      cell member time-wait-list-start

      \ TIME_WAIT list end
      cell member time-wait-list-end

      \ TIME_WAIT list count
      cell member time-wait-list-count

      \ TIME_WAIT list
      <time-wait> class-size time-wait-count * member time-wait-list
      
      \ DHCP lock
      lock-size member dhcp-lock

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

      \ Send an IPv4 TCP packet with an MSS option
      method send-ipv4-tcp-with-mss
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

      \ Process an IPv4 ACK packet in TCP_SYN_SENT state
      method process-ipv4-ack-syn-sent ( addr bytes endpoint self -- )

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
\      method process-ipv4-fin-packet ( src-addr addr bytes self -- )

      \ Process an IPv4 FIN/ACK packet for a TCP_ESTABLISHED state
      method process-ipv4-ack-fin-established ( addr bytes endpoint self -- )

      \ Process an IPv4 FIN/ACK packet for a TCP_FIN_WAIT_1 state
      method process-ipv4-ack-fin-fin-wait-1 ( addr bytes endpoint self -- )
      
      \ Process an IPv4 FIN/ACK packet for a TCP_FIN_WAIT_2 state
      method process-ipv4-ack-fin-fin-wait-2 ( addr bytes endpoint self -- )

      \ Process an unexpected IPv4 FIN packet
      method process-ipv4-unexpected-fin ( addr bytes endpoint self -- )

      \ Send an ACK in response to a FIN packet
      method send-ipv4-fin-reply-ack ( addr bytes endpoint self -- )
      
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

      \ Construct and send a IPv4 packet with a specified source IPv4 address
      method construct-and-send-ipv4-packet-with-src-addr
      ( ? D: mac-addr src-addr dest-addr protocol bytes xt self -- ? sent? )
      ( xt: ? buf -- ? send? )

      \ Construct an IPv4 packet
      method construct-and-send-ipv4-packet
      ( ? D: mac-addr dest-addr protocol bytes xt self -- ? sent? )
      ( xt: ? buf -- ? send? )

      \ Attempt IPv4 DHCP ARP
      method attempt-ipv4-dhcp-arp ( dest-addr self -- found? )
      
      \ Process IPv4 DNS response packet answers
      method process-ipv4-dns-answers
      ( addr bytes all-addr all-bytes ancount ident self -- )

      \ Send an ARP request packet
      method send-ipv4-arp-request ( dest-addr self -- )

      \ Send a DHCP ARP request packet
      method send-ipv4-dhcp-arp-request ( dest-addr self -- )

      \ Send a DNS request packet
      method send-ipv4-dns-request ( c-addr bytes self -- )

      \ Send a UDP packet with a specified source IPv4 address and destination
      \ MAC address
      method send-ipv4-udp-packet-raw
      ( ? D: mac-addr src-addr src-port dest-addr dest-port bytes xt self -- )
      ( ? success? )
      ( xt: ? buf -- ? sent? )    
      
      \ Wait for a TCP endpoint to close
      method wait-endpoint-closed ( timeout start endpoint self -- )

      \ Add an endpoint time wait
      method add-time-wait ( seq endpoint self -- )

      \ Refresh endpoint time waits
      method refresh-time-wait ( self -- )
      
      \ Send a FIN packet
      method send-fin ( state endpoint self -- )

      \ Close an established conection
      method close-tcp-established ( endpoint self -- )

      \ Send a reply FIN packet
      method send-fin-reply ( endpoint self -- )
      
      \ Send a data ACK packet
      method send-data-ack ( addr bytes push? endpoint self -- )

      \ Apply DHCP state
      method apply-dhcp ( self -- )

      \ Refresh DHCP rebinding
      method refresh-dhcp-rebinding ( self -- )
      
      \ Refresh DHCP renewal
      method refresh-dhcp-renewing ( self -- )

      \ Refresh DHCP discovered
      method refresh-dhcp-discovered ( self -- )
    
      \ Refresh DHCP got NAK
      method refresh-dhcp-got-nak ( self -- )

      \ Refresh DHCP wait offer
      method refresh-dhcp-wait-offer ( self -- )

      \ Refresh DHCP wait ACK
      method refresh-dhcp-wait-ack ( self -- )

      \ Refresh DHCP wait confirm
      method refresh-dhcp-wait-confirm ( self -- )

      \ Refresh DHCP declined
      method refresh-dhcp-declined ( self -- )
    
      \ Refresh DHCP
      method refresh-dhcp ( self -- )
      
      \ Refresh an interface
      method refresh-interface ( self -- )
      
      \ Send a DHCPDISCOVER packet
      method send-dhcpdiscover ( self -- )

      \ Send a DHCPREQUEST packet
      method send-dhcprequest ( self -- )

      \ Send a renewal DHCPREQUEST packet
      method send-renew-dhcprequest ( self -- )

      \ Send a rebinding DHCPREQUEST packet
      method send-rebind-dhcprequest ( self -- )

      \ Send a DHCPDECLINE packet
      method send-dhcpdecline ( self -- )

      \ Process an IPv4 DHCP packet
      method process-ipv4-dhcp-packet ( addr bytes src-addr self -- )

      \ Process an IPv4 DHCPOFFER packet
      method process-ipv4-dhcpoffer ( addr bytes self -- )

      \ Process an IPv4 DHCPACK packet
      method process-ipv4-dhcpack ( addr bytes src-addr self -- )

      \ Process an IPv4 DHCPNAK packet
      method process-ipv4-dhcpnak ( addr bytes self -- )

      \ Enqueue a ready receiving IP endpoint
      method put-ready-endpoint ( endpoint self -- )

      \ Allocate an endpoint
      method allocate-endpoint ( self -- endpoint success? )

      \ Claim the endpoint queue lock, and if an exception occurs, restore the
      \ endpoint queue status
      method with-endpoint-queue ( xt start-ticks self -- )
      
    end-module
    
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

    \ Get the DNS server IPv4 address
    method dns-server-ipv4-addr@ ( self -- addr )

    \ Set the DNS server IPv4 address
    method dns-server-ipv4-addr! ( addr self -- )
    
    \ Get the IPv4 broadcast address
    method intf-ipv4-broadcast@ ( self -- addr )
    
    \ Get the MAC address
    method intf-mac-addr@ ( self -- D: addr )

    \ Get the TTL
    method intf-ttl@ ( self -- ttl )
    
    \ Set the TTL
    method intf-ttl! ( ttl self -- )

    \ Start DHCP discovery
    method discover-ipv4-addr ( self -- )

    \ Send data on a TCP endpoint
    method send-tcp-endpoint ( addr bytes endpoint self -- )

    \ Send a UDP packet
    method send-ipv4-udp-packet
    ( ? src-port dest-addr dest-port bytes xt self -- ? success? )
    ( xt: ? buf -- ? sent? )
    
    \ Resolve an IPv4 address's MAC address
    method resolve-ipv4-addr-mac-addr ( dest-addr self -- D: mac-addr success? )

    \ Resolve a DNS name's IPv4 address
    method resolve-dns-ipv4-addr ( c-addr bytes self -- ipv4-addr success? )

    \ Evict a DNS name's entry, forcing it to be re-resolved
    method evict-dns ( c-addr bytes self -- )
    
    \ Wait for an endpoint to become ready
    method wait-ready-endpoint ( endpoint self -- )
    
    \ Dequeue a ready receiving IP endpoint
    method get-ready-endpoint ( self -- endpoint )

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

  end-class

  \ Implement the interface class
  <interface> begin-implement

    \ Constructor
    :noname { frame-interface self -- }
      self <object>->new
      frame-interface self out-frame-interface !
      DEFAULT_IPV4_ADDR self intf-ipv4-addr !
      255 255 255 0 make-ipv4-addr self intf-ipv4-netmask !
      192 168 1 254 make-ipv4-addr self gateway-ipv4-addr !
      8 8 8 8 make-ipv4-addr self dns-server-ipv4-addr !
      64 self intf-ttl !
      1 0 self mac-addr-resolve-sema init-sema
      1 0 self dns-resolve-sema init-sema
      0 self current-dhcp-xid !
      0 self dhcp-server-ipv4-addr !
      $FFFFFFFFFF. self dhcp-server-mac-addr 2!
      DEFAULT_IPV4_ADDR self dhcp-req-ipv4-addr !
      dhcp-not-discovering self dhcp-discover-state !
      systick::systick-counter
      dhcp-arp-attempt-count self dhcp-remaining-arp-attempts !
      dup self dhcp-discover-stage-start !
      dup self dhcp-renew-start !
      dup self dhcp-rebind-start !
      self dhcp-real-renew-start !
      default-dhcp-renew-interval self dhcp-real-renew-interval !
      default-dhcp-renew-interval self dhcp-renew-interval !
      default-dhcp-renew-interval self dhcp-rebind-interval !
      no-sema-limit 0 self dhcp-sema init-sema
      self outgoing-buf-lock init-lock
      max-endpoints 0 ?do
        <endpoint> self intf-endpoints <endpoint> class-size i * + init-object
      loop
      self endpoint-queue-lock init-lock
      self dhcp-lock init-lock
      no-sema-limit 0 self endpoint-queue-sema init-sema
      0 self endpoint-queue-index !
      systick::systick-counter self time-wait-interval-start !
      self time-wait-list-lock init-lock
      0 self time-wait-list-start !
      0 self time-wait-list-end !
      0 self time-wait-list-count !
      time-wait-count 0 ?do
        <time-wait> self time-wait-list <time-wait> class-size i * + init-object
      loop
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

    \ Get the DNS server IPv4 address
    :noname ( self -- addr )
      dns-server-ipv4-addr @
    ; define dns-server-ipv4-addr@
    
    \ Set the DNS server IPv4 address
    :noname ( addr self -- )
      dns-server-ipv4-addr !
    ; define dns-server-ipv4-addr!

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
    :noname { D: mac-addr ipv4-addr self -- }
      mac-addr ipv4-addr self address-map save-mac-addr-by-ipv4
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

        [ debug? ] [if]
          addr [: cr ." @@@@@ RECEIVING TCP:" tcp. ;] debug-hook execute
        [then]
        
        src-addr addr bytes self
        addr tcp-flags c@ TCP_CONTROL and
        case
          TCP_SYN of process-ipv4-syn-packet endof
          [ TCP_SYN TCP_ACK or ] literal of process-ipv4-syn-ack-packet endof
          TCP_ACK of process-ipv4-ack-packet endof
\          TCP_FIN of process-ipv4-fin-packet endof
          [ TCP_FIN TCP_ACK or ] literal of process-ipv4-fin-ack-packet endof
          TCP_RST of process-ipv4-rst-packet endof
          nip nip nip nip
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
          endpoint try-ipv4-accept-endpoint if
            endpoint self put-ready-endpoint
            endpoint true unloop exit
          then
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
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@ 1-
      0
      max-endpoint-in-size
      TCP_SYN
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-tcp-with-mss
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
      endpoint endpoint-local-seq@ 1-
      endpoint endpoint-ack@
      endpoint endpoint-local-window@
      [ TCP_SYN TCP_ACK or ] literal
      endpoint endpoint-remote-mac-addr@

      self send-ipv4-tcp-with-mss
      TCP_SYN_RECEIVED endpoint endpoint-tcp-state!
      endpoint endpoint-ack-sent
      endpoint reset-endpoint-refresh
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
        2drop 2drop 2drop 2drop drop \ Should never happen
      then
    ; define send-ipv4-rst-for-packet

    \ Send a generic IPv4 TCP RST packet
    :noname { endpoint self -- }
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      endpoint endpoint-ack@
      0
      TCP_RST
      endpoint endpoint-remote-mac-addr @
      self send-ipv4-basic-tcp
      endpoint reset-endpoint-local-port
      TCP_CLOSED endpoint endpoint-tcp-state!
      endpoint self put-ready-endpoint
      endpoint free-endpoint
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

        [ debug? ] [if]
          buf [: cr ." @@@@@ SENDING TCP:" tcp. ;] debug-hook execute
        [then]
        
        true
      ;] 6 pick construct-and-send-ipv4-packet drop
    ; define send-ipv4-basic-tcp

    \ Send an IPv4 TCP packet with an MSS field
    :noname
      ( remote-addr remote-port local-port seq ack window flags D: mac-addr )
      ( self -- )
      -rot 9 pick PROTOCOL_TCP tcp-header-size 8 +
      ( self D: mac-addr remote-addr protocol bytes )
      [: { remote-addr remote-port local-port seq ack window flags self buf }
        local-port rev16 buf tcp-src-port hunaligned!
        remote-port rev16 buf tcp-dest-port hunaligned!
        seq rev buf tcp-seq-no unaligned!
        ack rev buf tcp-ack-no unaligned!
        [ 7 4 lshift ] literal buf tcp-data-offset c!
        flags buf tcp-flags c!
        window rev16 buf tcp-window-size hunaligned!
        0 buf tcp-urgent-ptr hunaligned!
        [ TCP_OPT_MSS 24 lshift 4 16 lshift or
        mtu-size ethernet-header-size - ipv4-header-size - tcp-header-size - or
        rev ] literal
        buf tcp-header-size + unaligned!
        [ $01010100 rev ] literal buf tcp-header-size + 4 + unaligned!
        self intf-ipv4-addr@ remote-addr buf tcp-header-size 8 + 0 tcp-checksum
        compute-tcp-checksum rev16 buf tcp-checksum hunaligned!

        [ debug? ] [if]
          buf [: cr ." @@@@@ SENDING TCP:" tcp. ;] debug-hook execute
        [then]
        
        true
      ;] 6 pick construct-and-send-ipv4-packet drop
    ; define send-ipv4-tcp-with-mss
    
    \ Process an IPv4 TCP SYN+ACK packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-connect-ipv4-endpoint not if
        drop send-ipv4-rst-for-packet exit
      then
      [: { src-addr addr bytes self endpoint }
        addr tcp-dest-port hunaligned@ rev16 endpoint endpoint-local-port@ <> if
          exit
        then
        endpoint endpoint-tcp-state@ TCP_SYN_SENT = if
          addr tcp-ack-no unaligned@ rev
          endpoint endpoint-init-local-seq@ <> if exit then
          addr tcp-seq-no unaligned@ rev 1+
          addr bytes tcp-mss@ not if
            drop [ mtu-size ethernet-header-size - ipv4-header-size -
            tcp-header-size - ] literal
          then
          addr tcp-window-size hunaligned@ rev16
          endpoint init-tcp-stream
        then
        endpoint endpoint-ipv4-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        endpoint endpoint-ack@
        endpoint endpoint-local-window@
        TCP_ACK
        endpoint endpoint-remote-mac-addr@
        self send-ipv4-basic-tcp
        TCP_ESTABLISHED endpoint endpoint-tcp-state!
        endpoint reset-endpoint-refresh
        endpoint self put-ready-endpoint
      ;] over with-endpoint
    ; define process-ipv4-syn-ack-packet

    \ Process an IPv4 ACK packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-connect-ipv4-endpoint not if
        2drop 2drop drop exit
      then
      [: { src-addr addr bytes self endpoint }
        addr tcp-dest-port hunaligned@ rev16 endpoint endpoint-local-port@ <> if
          exit
        then
        endpoint endpoint-tcp-state@ { state }
        addr bytes endpoint self
        state case
          TCP_SYN_SENT of
            process-ipv4-ack-syn-sent
          endof
          TCP_SYN_RECEIVED of
            process-ipv4-ack-syn-received
          endof
          TCP_ESTABLISHED of
            process-ipv4-ack-established
          endof
          TCP_FIN_WAIT_1 of
            process-ipv4-ack-fin-wait-1
          endof
          TCP_FIN_WAIT_2 of
            process-ipv4-ack-fin-wait-2
          endof
          TCP_CLOSE_WAIT of
            process-ipv4-ack-close-wait
          endof
          TCP_LAST_ACK of
            process-ipv4-ack-last-ack
          endof
          drop 2drop 2drop
        endcase
        endpoint endpoint-waiting-bytes@ 0>
        endpoint endpoint-tcp-state@ state <> or if
          endpoint self put-ready-endpoint
        else
          endpoint wake-endpoint
        then
        endpoint start-endpoint-timeout
      ;] over with-endpoint
    ; define process-ipv4-ack-packet

    \ Process an IPv4 FIN+ACK packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-connect-ipv4-endpoint not if
        drop send-ipv4-rst-for-packet exit
      then
      [: { src-addr addr bytes self endpoint }
        addr tcp-dest-port hunaligned@ rev16 endpoint endpoint-local-port@ <> if
          exit
        then
        endpoint endpoint-tcp-state@ { state }
        addr bytes endpoint self process-ipv4-basic-ack
        addr bytes endpoint self
        state case
          TCP_ESTABLISHED of process-ipv4-ack-fin-established endof
          TCP_FIN_WAIT_1 of process-ipv4-ack-fin-fin-wait-1 endof
          TCP_FIN_WAIT_2 of process-ipv4-ack-fin-fin-wait-2 endof
          >r 2drop 2drop r>
        endcase
        endpoint endpoint-waiting-bytes@ 0>
        endpoint endpoint-tcp-state@ state <> or if
          endpoint self put-ready-endpoint
        else
          endpoint wake-endpoint
        then
      ;] over with-endpoint
    ; define process-ipv4-fin-ack-packet

    \ Process an IPv4 ACK packet in the general case
    :noname { addr bytes endpoint self -- }
      addr full-tcp-header-size { header-size }
      addr header-size + bytes header-size -
      addr tcp-flags c@ TCP_PSH and 0<>
      addr tcp-seq-no unaligned@ rev
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
        endpoint reset-endpoint-refresh
      then
    ; define process-ipv4-basic-ack

    \ Process an IPv4 ACK packet in TCP_SYN_SENT state
    :noname ( addr bytes endpoint self -- )
      [: { addr bytes endpoint self }
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
        addr bytes endpoint self process-ipv4-basic-ack
        endpoint self put-ready-endpoint
      ;] 2 pick with-endpoint
    ; define process-ipv4-ack-syn-sent
    
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
      endpoint endpoint-local-seq@ 1+ = if
        TCP_FIN_WAIT_2 endpoint endpoint-tcp-state!
      else
        addr bytes endpoint self process-ipv4-basic-ack
      then
    ; define process-ipv4-ack-fin-wait-1
    
    \ Process an IPv4 ACK packet in TCP_FIN_WAIT_2 state
    :noname { addr bytes endpoint self -- }
      \ addr bytes endpoint self process-ipv4-basic-ack
    ; define process-ipv4-ack-fin-wait-2
    
    \ Process an IPv4 ACK packet in TCP_CLOSE_WAIT state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self process-ipv4-basic-ack
    ; define process-ipv4-ack-close-wait
    
    \ Process an IPv4 ACK packet in TCP_LAST_ACK state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self process-ipv4-basic-ack
      addr tcp-ack-no unaligned@ rev
      endpoint endpoint-local-seq@ = if
        TCP_CLOSED endpoint endpoint-tcp-state!
      then
    ; define process-ipv4-ack-last-ack

    \ Process an errant IPv4 ACK packet
    :noname { addr bytes endpoint self -- }
      endpoint endpoint-ipv4-remote@ drop addr bytes
      self send-ipv4-rst-for-packet
      TCP_CLOSED endpoint endpoint-tcp-state!
      endpoint self put-ready-endpoint
      endpoint free-endpoint
    ; define send-ipv4-rst-for-ack
    
    \ \ Process an IPv4 FIN packet
    \ :noname ( src-addr addr bytes self -- )
    \   2over 2over find-connect-ipv4-endpoint not if
    \     drop send-ipv4-rst-for-packet exit
    \   then
    \   [: { src-addr addr bytes self endpoint }
    \     addr tcp-dest-port hunaligned@ rev16 endpoint endpoint-local-port@ <> if
    \       exit
    \     then
    \     addr bytes endpoint self
    \     endpoint endpoint-tcp-state@ { state }
    \     state case
    \       TCP_ESTABLISHED of process-ipv4-fin-established endof
    \       TCP_FIN_WAIT_2 of process-ipv4-fin-fin-wait-2 endof
    \       >r process-ipv4-unexpected-fin r>
    \     endcase
    \     endpoint endpoint-tcp-state@ state <> or if
    \       endpoint self put-ready-endpoint
    \     else
    \       endpoint wake-endpoint
    \     then
    \   ;] over with-endpoint
    \ ; define process-ipv4-fin-packet

    \ Process an IPv4 FIN packet for a TCP_ESTABLISHED state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self send-ipv4-fin-reply-ack
      TCP_CLOSE_WAIT endpoint endpoint-tcp-state!
      endpoint self put-ready-endpoint
    ; define process-ipv4-ack-fin-established

    \ Process an IPv4 FIN/ACK packet for a TCP_FIN_WAIT_1 state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self send-ipv4-fin-reply-ack
      TCP_CLOSED endpoint endpoint-tcp-state! \ Formerly TCP_TIME_WAIT
      addr tcp-seq-no unaligned@ rev endpoint self add-time-wait
      endpoint self put-ready-endpoint
    ; define process-ipv4-ack-fin-fin-wait-1

    \ Process an IPv4 FIN/ACK packet for a TCP_FIN_WAIT_2 state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self send-ipv4-fin-reply-ack
      TCP_CLOSED endpoint endpoint-tcp-state! \ Formerly TCP_TIME_WAIT
      addr tcp-seq-no unaligned@ rev endpoint self add-time-wait
      endpoint self put-ready-endpoint
    ; define process-ipv4-ack-fin-fin-wait-2

    \ Process an unexpected IPv4 FIN packet
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self send-ipv4-fin-reply-ack
      TCP_CLOSED endpoint endpoint-tcp-state! \ Formerly TCP_CLOSING
      addr tcp-seq-no unaligned@ rev endpoint self add-time-wait
      endpoint wake-endpoint
    ; define process-ipv4-unexpected-fin

    \ Send an ACK in response to a FIN packet
    :noname { addr bytes endpoint self -- }
      endpoint endpoint-ipv4-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@ 1+
      addr tcp-seq-no unaligned@ rev 1+
      endpoint endpoint-local-window@
      TCP_ACK
\      [ TCP_FIN TCP_ACK or ] literal
      endpoint endpoint-remote-mac-addr@
      self send-ipv4-basic-tcp
    ; define send-ipv4-fin-reply-ack
      
    \ Process an IPv4 RST packet
    :noname ( src-addr addr bytes self -- )
      2over 2over find-connect-ipv4-endpoint if
        [: { src-addr addr bytes self endpoint }
          addr tcp-dest-port hunaligned@ rev16
          endpoint endpoint-local-port@ <> if
            exit
          then
          endpoint endpoint-tcp-state@ TCP_SYN_SENT <> if
            TCP_CLOSED endpoint endpoint-tcp-state!
          then
          endpoint reset-endpoint-local-port
          endpoint self put-ready-endpoint
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
        addr udp-src-port h@ rev16 dhcp-server-port =
        addr udp-dest-port h@ rev16 dhcp-client-port = and if
          addr udp-header-size + bytes udp-header-size -
          src-addr self process-ipv4-dhcp-packet
          exit
        then
        max-endpoints 0 ?do
          self intf-endpoints <endpoint> class-size i * + { endpoint }
          src-addr addr bytes endpoint self [:
            { src-addr addr bytes endpoint self }
            endpoint udp-endpoint?
            endpoint endpoint-local-port@ addr udp-dest-port h@ rev16 = and if
              addr udp-header-size + bytes udp-header-size -
              src-addr addr udp-src-port h@ rev16
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
      [:
        [: { bytes xt self }
          bytes mtu-size u<= averts x-oversized-frame
          self outgoing-buf xt execute if
            self outgoing-buf bytes self send-frame true
          else
            false
          then
        ;] over outgoing-buf-lock with-lock
      ;] task::no-timeout task::with-timeout
    ; define construct-and-send-frame

    \ Construct and send a IPv4 packet with a specified source IPv4 address
    :noname ( ? D: mac-addr src-addr dest-addr protocol bytes xt self -- )
      ( ? sent? ) ( xt: ? buf -- ? send? )
      2 pick [ ethernet-header-size ipv4-header-size + ] literal + over
      [: { D: mac-addr src-addr dest-addr protocol bytes xt self buf }
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
        src-addr rev ip-buf ipv4-src-addr unaligned!
        dest-addr rev ip-buf ipv4-dest-addr unaligned!
        ip-buf ipv4-header-size 0 ipv4-header-checksum compute-inet-checksum
        rev16 ip-buf ipv4-header-checksum h!
        ip-buf ipv4-header-size + xt execute

        [ debug? ] [if]
          buf bytes [ ethernet-header-size ipv4-header-size + ] literal +
          [: over + dump ;] debug-hook execute
        [then]
        
      ;] swap construct-and-send-frame
    ; define construct-and-send-ipv4-packet-with-src-addr

    \ Construct and send a IPv4 packet
    :noname ( ? D: mac-addr dest-addr protocol bytes xt self -- ? sent? )
      ( xt: ? buf -- ? send? )
      dup intf-ipv4-addr @
      swap >r swap >r swap >r swap >r swap r> r> r> r>
      construct-and-send-ipv4-packet-with-src-addr
    ; define construct-and-send-ipv4-packet

    \ Resolve an IPv4 address's MAC address
    :noname { dest-addr self -- D: mac-addr success? }
      dest-addr self intf-ipv4-netmask@ and
      192 168 1 0 make-ipv4-addr self intf-ipv4-netmask@ and <> if
        self gateway-ipv4-addr@ to dest-addr
      then
      systick::systick-counter mac-addr-resolve-interval - { tick }
      max-mac-addr-resolve-attempts { attempts }
      begin
        dest-addr self address-map lookup-mac-addr-by-ipv4 not
      while
        2drop
        systick::systick-counter tick - mac-addr-resolve-interval >= if
          attempts 0> if
            -1 +to attempts
            dest-addr self send-ipv4-arp-request
            systick::systick-counter to tick
          else
            0. false exit
          then
        else
          task::timeout @ { old-timeout }
          tick mac-addr-resolve-interval + systick::systick-counter -
          self swap [: task::timeout ! mac-addr-resolve-sema take ;] try
          dup ['] task::x-timed-out = if 2drop drop 0 then
          ?raise
          old-timeout task::timeout !
        then
      repeat
      true
    ; define resolve-ipv4-addr-mac-addr

    \ Test for DHCP ARP
    :noname { dest-addr self -- success? }
      dest-addr self intf-ipv4-netmask@ and
      192 168 1 0 make-ipv4-addr self intf-ipv4-netmask@ and <> if
        self gateway-ipv4-addr@ to dest-addr
      then
      systick::systick-counter dhcp-arp-interval - { tick }
      dhcp-arp-attempt-count { attempts }
      begin
        dest-addr self address-map lookup-mac-addr-by-ipv4 not -rot 2drop
      while
        systick::systick-counter tick - dhcp-arp-interval >= if
          attempts 0> if
            -1 +to attempts
            dest-addr self send-ipv4-dhcp-arp-request
            systick::systick-counter to tick
          else
            false exit
          then
        else
          task::timeout @ { old-timeout }
          tick dhcp-arp-interval + systick::systick-counter - task::timeout !
          self swap [: task::timeout ! mac-addr-resolve-sema take ;] try
          dup ['] task::x-timed-out = if 2drop drop 0 then
          ?raise
          old-timeout task::timeout !
        then
      repeat
      true
    ; define attempt-ipv4-dhcp-arp

    \ Resolve a DNS name's IPv4 address
    :noname { c-addr bytes self -- ipv4-addr success? }
      c-addr bytes validate-dns-name
      systick::systick-counter dns-resolve-interval - { tick }
      max-dns-resolve-attempts { attempts }
      c-addr bytes self dns-cache lookup-ipv4-addr-by-dns if
        0= if true exit then
      else
        2drop
      then
      begin
        systick::systick-counter tick - dns-resolve-interval >= if
          attempts 0> if
            -1 +to attempts
            c-addr bytes self send-ipv4-dns-request
            systick::systick-counter to tick
          else
            0 false exit
          then
        else
          task::timeout @ { old-timeout }
          tick dns-resolve-interval + systick::systick-counter -
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

    \ Evict a DNS name's cache entry, forcing it to be re-resolved
    :noname { c-addr bytes self -- }
      c-addr bytes validate-dns-name
      c-addr bytes self dns-cache net-internal::evict-dns
    ; define evict-dns

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

    \ Send a DHCP ARP request packet
    :noname ( dest-addr self -- )
      [ ethernet-header-size arp-ipv4-size + ] literal [: { dest-addr self buf }

        dhcp-log? if
          [: cr ." Sending DHCP ARP request packet" ;] debug-hook execute
        then
        
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
        [ $00000000 rev ] literal arp-buf arp-spa unaligned!
        0. arp-buf arp-tha mac!
        dest-addr rev arp-buf arp-tpa unaligned!
        true
      ;] 2 pick construct-and-send-frame drop

      dhcp-log? if
        [: cr ." Sent DHCP ARP request packet" ;] debug-hook execute
      then
      
    ; define send-ipv4-dhcp-arp-request

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

    \ Send a UDP packet with a specified source IPv4 address
    :noname
      { D: mac-addr src-addr src-port dest-addr dest-port bytes xt self -- }
      ( success? ) ( xt: ? buf -- ? sent? )
      src-port dest-port bytes xt self
      mac-addr src-addr dest-addr PROTOCOL_UDP bytes udp-header-size + [:
        { src-port dest-port bytes xt self buf }
        src-port rev16 buf udp-src-port h!
        dest-port rev16 buf udp-dest-port h!
        bytes udp-header-size + rev16 buf udp-total-len h!
        0 buf udp-checksum h!
        buf udp-header-size + xt execute
      ;] self construct-and-send-ipv4-packet-with-src-addr
    ; define send-ipv4-udp-packet-raw

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

    \ Claim the endpoint queue lock, and if an exception occurs, restore the
    \ endpoint queue status
    :noname ( xt start-ticks self -- ) { self }
      self [:
        [:
          endpoint-queue-lock with-lock
        ;] rot task::timeout @ swap task::with-timeout-from-start
      ;] try
      dup if self endpoint-queue-sema give then
      ?raise
    ; define with-endpoint-queue

    \ Enqueue a ready receiving IP endpoint
    :noname ( endpoint self -- )

      [: { self }
        
        [ debug? ] [if]
          [: cr ." +++ Begin enqueue endpoint" ;] debug-hook execute
        [then]
        
        self [: { endpoint self }
          endpoint endpoint-pending? not if
            endpoint mark-endpoint-pending
            true
          else
            endpoint signal-endpoint-event
            false
          then
          endpoint wake-endpoint
        ;] self endpoint-queue-lock with-lock
        if
          self endpoint-queue-sema give
          
          [ debug? ] [if]
            [: cr ." +++ GAVE SEMAPHORE FOR PUT-READY-ENDPOINT" ;]
            debug-hook execute
          [then]
          
          pause
        then
        
        [ debug? ] [if]
          [: cr ." +++ End enqueue endpoint" ;] debug-hook execute
        [then]

      ;] task::no-timeout task::with-timeout

    ; define put-ready-endpoint

    \ Wait for a specific endpoint to become ready
    :noname { endpoint self -- }
      systick::systick-counter { start }
      begin
        self [: endpoint-queue-sema take ;]
        task::timeout @ start task::with-timeout-from-start
        endpoint [: { endpoint }
          endpoint endpoint-enqueued? if
            endpoint clear-endpoint-enqueued
            endpoint clear-endpoint-event
            true
          else
            false
          then
        ;] start self with-endpoint-queue
        dup not if
          self endpoint-queue-sema give
          pause
        then
        endpoint [: promote-rx-data ;] task::no-timeout task::with-timeout
      until
    ; define wait-ready-endpoint

    \ Dequeue a ready receiving IP endpoint
    :noname { self -- endpoint }

      systick::systick-counter { start }

      [ debug? ] [if]
        [: cr ." +++ Begin dequeue endpoint" ;] debug-hook execute
      [then]

      self [: endpoint-queue-sema take ;]
      task::timeout @ start task::with-timeout-from-start

      [ debug? ] [if]
        [: cr ." +++ Got queue semaphore" ;] debug-hook execute
      [then]
      
      self endpoint-queue-index @ { init-index }
      
      begin

        [ debug? ] [if]
          [: cr ." +++ Trying to get endpoint" ;] debug-hook execute
        [then]
        
        self [: { self }
          self endpoint-queue-index @ { index }
          self intf-endpoints <endpoint> class-size index * + { endpoint }
          endpoint endpoint-enqueued? if
            
            [ debug? ] [if]
              index [: cr ." Got endpoint: " . ;] debug-hook execute
            [then]
            
            endpoint clear-endpoint-enqueued
            endpoint clear-endpoint-event
            endpoint true
          else
            
            [ debug? ] [if]
              index [: cr ." Skipped endpoint: " . ;] debug-hook execute
            [then]
            
            false
          then
          index 1+ max-endpoints umod self endpoint-queue-index !
          
          [ debug? ] [if]
            [: cr ." Releasing endpoint queue lock" ;] debug-hook execute
          [then]
          
        ;] start self with-endpoint-queue
      until

      [ debug? ] [if]
        [: cr ." Promoting RX data" ;] debug-hook execute
      [then]

      dup [: promote-rx-data ;] task::no-timeout task::with-timeout
      
      [ debug? ] [if]
        [: cr ." +++ End dequeue endpoint" ;] debug-hook execute
      [then]

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

      [ debug? ] [if]
        [: cr ." +++ Begin endpoint done" ;] debug-hook execute
      [then]

      endpoint endpoint-pending? if
        endpoint retire-rx-data
      then
      
      [ debug? ] [if]
        [: cr ." +++ Retired RX data" ;] debug-hook execute
      [then]

      endpoint self [: { endpoint self }

        [ debug? ] [if]
          [: cr ." +++ In endpoint done" ;] debug-hook execute
        [then]

        endpoint endpoint-pending? if
          endpoint endpoint-has-event? if
            endpoint clear-endpoint-event
            endpoint mark-endpoint-pending
            true
          else
            endpoint clear-endpoint-pending
            false
          then
        else
          false
        then

        [ debug? ] [if]
          [: cr ." +++ After endpoint done" ;] debug-hook execute
        [then]

      ;] self endpoint-queue-lock with-lock
      if
        self endpoint-queue-sema give

        [ debug? ] [if]
          [: cr ." +++ GAVE SEMAPHORE FOR ENDPOINT-DONE" ;] debug-hook execute
        [then]

        pause
      then

      [ debug? ] [if]
        [: cr ." +++ End endpoint done" ;] debug-hook execute
      [then]

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
            rng::random endpoint endpoint-init-local-seq!
            endpoint reset-endpoint-local-port
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

    \ Add an endpoint to wait
    :noname ( seq endpoint self -- )
      [: { seq endpoint self }
        self time-wait-list
        self time-wait-list-end @ <time-wait> class-size * + { time-wait }
        seq endpoint time-wait time-wait!
        self time-wait-list-end @ 1+ time-wait-count umod
        self time-wait-list-end !
        self time-wait-list-count @ 0= if
          systick::systick-counter self time-wait-interval-start !
        then
        self time-wait-list-count @ time-wait-count < if
          1 self time-wait-list-count +!
        else
          self time-wait-list-start @ 1+ time-wait-count umod
          self time-wait-list-start !
        then
      ;] over time-wait-list-lock with-lock
    ; define add-time-wait
    
    \ Refresh endpoint time waits
    :noname ( self -- )
      [: { self }
        self time-wait-list-start @
        self time-wait-list-count @ { current count }
        count 0> if
          systick::systick-counter
          self time-wait-interval-start @ time-wait-interval-timeout + < if
            exit
          else
            systick::systick-counter self time-wait-interval-start !
          then
        else
          systick::systick-counter self time-wait-interval-start !
          exit
        then
        begin count 0> while
          self time-wait-list current <time-wait> class-size * + { time-wait }
          current 1+ time-wait-count umod to current
          -1 +to count
          systick::systick-counter
          time-wait time-wait-start-time @ time-wait-timeout + < if
            time-wait time-wait-remote-ipv4-addr @
            time-wait time-wait-remote-port h@
            time-wait time-wait-local-port h@
            time-wait time-wait-local-seq @
            time-wait time-wait-ack @
            time-wait time-wait-local-window @
            TCP_ACK
            time-wait time-wait-remote-mac-addr 2@
            self send-ipv4-basic-tcp
          else
            current self time-wait-list-start !
            count self time-wait-list-count !
          then
        repeat
      ;] over time-wait-list-lock with-lock
    ; define refresh-time-wait

    \ Close a TCP endpoint
    :noname ( endpoint self -- )
      systick::systick-counter { start }
      task::timeout @ start 2swap [:
        [: { timeout start endpoint self }
          endpoint endpoint-tcp-state@ case
            TCP_SYN_SENT of endpoint self send-ipv4-rst endof
            TCP_SYN_RECEIVED of endpoint self send-ipv4-rst endof
            TCP_ESTABLISHED of
              timeout start endpoint self close-tcp-established
            endof
            TCP_FIN_WAIT_1 of
              timeout start endpoint self wait-endpoint-closed
            endof
            TCP_FIN_WAIT_2 of
              timeout start endpoint self wait-endpoint-closed
            endof
            TCP_CLOSE_WAIT of
              timeout start endpoint self send-fin-reply
            endof
            TCP_LAST_ACK of
              timeout start endpoint self wait-endpoint-closed
            endof
          endcase
        ;] 2 pick with-ctrl-endpoint
      ;] task::timeout @ start task::with-timeout-from-start
    ; define close-tcp-endpoint

    \ Wait for a TCP endpoint to close
    :noname { timeout start endpoint self -- }
      begin
        endpoint endpoint-tcp-state@ case
          TCP_FIN_WAIT_1 of true endof
          TCP_FIN_WAIT_2 of true endof
          TCP_LAST_ACK of true endof
          false swap
        endcase
      while
        timeout task::no-timeout = if close-timeout else timeout then
        start + systick::systick-counter - 0 max { current-timeout }
        task::timeout @ { old-timeout }
        endpoint current-timeout [: task::timeout ! wait-endpoint ;] try
        old-timeout task::timeout !
        dup ['] task::x-timed-out = if 2drop drop true 0 else false swap then
        ?raise
        if
          endpoint endpoint-remote-seq@ endpoint self add-time-wait
          TCP_CLOSED endpoint endpoint-tcp-state!
          endpoint free-endpoint
          exit
        then
      repeat
      TCP_CLOSED endpoint endpoint-tcp-state!
      endpoint free-endpoint
    ; define wait-endpoint-closed

    \ Send a FIN packet
    :noname ( state endpoint self -- )
      [: { state endpoint self }
        state endpoint endpoint-tcp-state!
        endpoint endpoint-ipv4-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        endpoint endpoint-ack@
        endpoint endpoint-local-window@
        [ TCP_FIN TCP_ACK or ] literal
        endpoint endpoint-remote-mac-addr@
        self send-ipv4-basic-tcp
        endpoint endpoint-ack-sent
      ;] 2 pick with-endpoint
    ; define send-fin
    
    \ Close an established conection
    :noname ( timeout start endpoint self -- )
      2dup TCP_FIN_WAIT_1 -rot send-fin wait-endpoint-closed
    ; define close-tcp-established

    \ Send a reply FIN packet
    :noname ( timeout start endpoint self -- )
      2dup TCP_LAST_ACK -rot send-fin wait-endpoint-closed
    ; define send-fin-reply

    \ Send data on a TCP endpoint
    :noname ( addr bytes endpoint self -- )
      [:
        2 pick 0= if 2drop 2drop exit then
        [: { addr bytes endpoint self }
          endpoint endpoint-id@ { id }
          id addr bytes endpoint [: { id addr bytes endpoint }
            endpoint endpoint-tcp-state@
            dup TCP_ESTABLISHED = swap TCP_CLOSE_WAIT = or
            id endpoint endpoint-id@ = and if
              addr bytes endpoint start-endpoint-send true
            else

              [ debug? ] [if]
                endpoint endpoint-tcp-state@
                [: cr ." *** TCP STATE: " . ;] debug-hook execute
              [then]
              
              false
            then
          ;] over with-endpoint if
            endpoint start-endpoint-timeout
            begin
              id endpoint self [: { id endpoint self }
                endpoint endpoint-tcp-state@
                dup TCP_ESTABLISHED = swap TCP_CLOSE_WAIT = or
                id endpoint endpoint-id@ = and if
                  endpoint endpoint-send-done? not if
                    endpoint endpoint-send-outstanding? if
                      systick::systick-counter
                      endpoint endpoint-timeout-start@ -
                      endpoint endpoint-timeout@ > if
                        endpoint increase-endpoint-timeout
                        endpoint resend-endpoint
                      then
                    else
                      endpoint start-endpoint-timeout
                    then
                    endpoint endpoint-send-ready?
                    self out-frame-interface @ tx-full? not and
                    self out-frame-interface @ rx-full? not and if
                      endpoint get-endpoint-send-packet
                      endpoint endpoint-send-last?
                      endpoint self send-data-ack
                      endpoint endpoint-send-ready? not
                      self out-frame-interface @ tx-full? or
                      self out-frame-interface @ rx-full? or true
                    else
                      true true
                    then
                  else
                    endpoint clear-endpoint-send false
                  then
                else
                  
                  [ debug? ] [if]
                    endpoint endpoint-tcp-state@
                    [: cr ." *** TCP STATE: " . ;] debug-hook execute
                  [then]

                  endpoint clear-endpoint-send false 
                then
              ;] endpoint with-endpoint
            while
              if
                task::timeout @ { old-timeout }
                endpoint endpoint-timeout@
                systick::systick-counter endpoint endpoint-timeout-start@ - -
                send-check-interval max
                endpoint swap [: task::timeout ! wait-endpoint ;] try
                dup ['] task::x-timed-out = if 2drop drop 0 then
                old-timeout task::timeout !
                ?raise
              then
            repeat
          then
        ;] 2 pick with-ctrl-endpoint
      ;] task::no-timeout task::with-timeout
    ; define send-tcp-endpoint

    \ Send a data ACK packet
    :noname ( addr bytes push? endpoint self -- )
      over endpoint-remote-mac-addr@
      3 pick endpoint-ipv4-remote@ drop
      PROTOCOL_TCP
      7 pick tcp-header-size +
      ( addr bytes push? endpoint self D: mac-addr remote-addr protocol bytes )
      [: { addr bytes push? endpoint self buf }
        endpoint endpoint-local-port@ rev16 buf tcp-src-port hunaligned!
        endpoint endpoint-ipv4-remote@ nip rev16 buf tcp-dest-port hunaligned!
        endpoint endpoint-local-seq@ rev buf tcp-seq-no unaligned!
        endpoint endpoint-ack@ rev buf tcp-ack-no unaligned!
        [ 5 4 lshift ] literal buf tcp-data-offset c!
        push? if [ TCP_ACK TCP_PSH or ] literal else TCP_ACK then
        buf tcp-flags c!
        endpoint endpoint-local-window@ rev16 buf tcp-window-size hunaligned!
        0 buf tcp-urgent-ptr hunaligned!
        addr buf tcp-header-size + bytes move
        self intf-ipv4-addr@
        endpoint endpoint-ipv4-remote@ drop
        buf tcp-header-size bytes + 0 tcp-checksum
        compute-tcp-checksum rev16 buf tcp-checksum hunaligned!
        [ debug? ] [if]
          buf bytes
          [: { buf bytes }
            cr ." @@@@@ SENDING TCP WITH DATA:" buf tcp.
            buf buf tcp-header-size + bytes + dump
          ;] debug-hook execute
        [then]
        true
      ;] 6 pick construct-and-send-ipv4-packet drop
    ; define send-data-ack

    \ Apply DHCP data
    :noname { self -- }
      self dhcp-req-ipv4-addr @ self intf-ipv4-addr!
      self prov-ipv4-netmask @ self intf-ipv4-netmask!
      self prov-gateway-ipv4-addr @ self gateway-ipv4-addr!
      self prov-dns-server-ipv4-addr @ self dns-server-ipv4-addr!
      self prov-dhcp-renew-interval @
      dup self dhcp-real-renew-interval !
      dup dhcp-renew-init-divisor / self dhcp-renew-interval !
      dhcp-rebind-multiplier * dhcp-rebind-divisor / self dhcp-rebind-interval !
      systick::systick-counter
      dup self dhcp-real-renew-start !
      dup self dhcp-renew-start !
      self dhcp-rebind-start !
      dhcp-discovered self dhcp-discover-state !
      self dhcp-sema broadcast
      self dhcp-sema give
    ; define apply-dhcp

    \ Start DHCP discovery
    :noname ( self -- )
      dup ['] send-dhcpdiscover over dhcp-lock with-lock dhcp-sema take
    ; define discover-ipv4-addr
    
    \ Send a DHCPDISCOVER packet
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending DHCPDISCOVER" ;] debug-hook execute
      then
      $FFFFFFFFFFFF. self dhcp-server-mac-addr 2!
      $00000000 self dhcp-req-ipv4-addr !
      $FFFFFFFF self dhcp-server-ipv4-addr !
      rng::random self current-dhcp-xid !
      dhcp-wait-offer self dhcp-discover-state !
      systick::systick-counter self dhcp-discover-stage-start !
      self
      $FFFFFFFFFFFF.
      $00000000 dhcp-client-port
      $FFFFFFFF dhcp-server-port
      [ dhcp-header-size 3 + 6 + 6 + 1 + ] literal [: { self buf }
        dhcp-log? if
          [: cr ." Constructing DHCPDISCOVER" ;] debug-hook execute
        then
        DHCP_OP_CLIENT buf dhcp-op c!
        DHCP_HTYPE buf dhcp-htype c!
        DHCP_HLEN buf dhcp-hlen c!
        0 buf dhcp-hops c!
        self current-dhcp-xid @ rev buf dhcp-xid unaligned!
        [ 0 rev16 ] literal buf dhcp-secs hunaligned!
        [ 0 rev16 ] literal buf dhcp-flags hunaligned!
        [ 0 rev ] literal buf dhcp-ciaddr unaligned!
        [ 0 rev ] literal buf dhcp-yiaddr unaligned!
        [ 0 rev ] literal buf dhcp-siaddr unaligned!
        [ 0 rev ] literal buf dhcp-giaddr unaligned!
        buf dhcp-chaddr 16 0 fill
        self intf-mac-addr@
        rev16 buf dhcp-chaddr hunaligned!
        rev buf dhcp-chaddr 2 + unaligned!
        buf dhcp-filler 192 0 fill
        [ DHCP_MAGIC_COOKIE rev ] literal buf dhcp-magic unaligned!
        dhcp-header-size +to buf
        DHCP_MESSAGE_TYPE buf c!
        1 buf 1 + c!
        DHCPDISCOVER buf 2 + c!
        3 +to buf
        DHCP_REQ_IPV4_ADDR buf c!
        4 buf 1 + c!
        [ DEFAULT_IPV4_ADDR rev ] literal buf 2 + unaligned!
        6 +to buf
        DHCP_PARAM_REQ_LIST buf c!
        4 buf 1 + c!
        DHCP_SERVICE_SUBNET_MASK buf 2 + c!
        DHCP_SERVICE_ROUTER buf 3 + c!
        DHCP_SERVICE_DNS_NAME buf 4 + c!
        DHCP_SERVICE_DNS_SERVER buf 5 + c!
        6 +to buf
        DHCP_END buf c!
        true
        dhcp-log? if
          [: cr ." Constructed DHCPDISCOVER packet" ;] debug-hook execute
        then
      ;] self send-ipv4-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Waiting for DHCPOFFER" ;] debug-hook execute
      then
    ; define send-dhcpdiscover

    \ Send a DHCPREQUEST packet
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending DHCPREQUEST" ;] debug-hook execute
      then
      self
      $FFFFFFFFFFFF.
      $00000000 dhcp-client-port
      $FFFFFFFF dhcp-server-port
      [ dhcp-header-size 3 + 6 + 6 + 1 + ] literal [: { self buf }
        dhcp-log? if
          [: cr ." Constructing DHCPREQUEST" ;] debug-hook execute
        then
        DHCP_OP_CLIENT buf dhcp-op c!
        DHCP_HTYPE buf dhcp-htype c!
        DHCP_HLEN buf dhcp-hlen c!
        0 buf dhcp-hops c!
        self current-dhcp-xid @ rev buf dhcp-xid unaligned!
        [ 0 rev16 ] literal buf dhcp-secs hunaligned!
        [ 0 rev16 ] literal buf dhcp-flags hunaligned!
        [ 0 rev ] literal buf dhcp-ciaddr unaligned!
        [ 0 rev ] literal buf dhcp-yiaddr unaligned!
        self dhcp-server-ipv4-addr @ rev buf dhcp-siaddr unaligned!
        [ 0 rev ] literal buf dhcp-giaddr unaligned!
        buf dhcp-chaddr 16 0 fill
        self intf-mac-addr@
        rev16 buf dhcp-chaddr hunaligned!
        rev buf dhcp-chaddr 2 + unaligned!
        buf dhcp-filler 192 0 fill
        [ DHCP_MAGIC_COOKIE rev ] literal buf dhcp-magic unaligned!
        dhcp-header-size +to buf
        DHCP_MESSAGE_TYPE buf c!
        1 buf 1 + c!
        DHCPREQUEST buf 2 + c!
        3 +to buf
        DHCP_REQ_IPV4_ADDR buf c!
        4 buf 1 + c!
        self dhcp-req-ipv4-addr @ rev buf 2 + unaligned!
        6 +to buf
        DHCP_SERVER_IPV4_ADDR buf c!
        4 buf 1 + c!
        self dhcp-server-ipv4-addr @ rev buf 2 + unaligned!
        6 +to buf
        DHCP_END buf c!
        true
        dhcp-log? if
          [: cr ." Constructed DHCPREQUEST packet" ;] debug-hook execute
        then
      ;] self send-ipv4-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Waiting for DHCPACK" ;] debug-hook execute
      then
      self dhcp-discover-state @
      dup dhcp-discovered <> swap dhcp-renewing <> and if
        dhcp-wait-ack self dhcp-discover-state !
        systick::systick-counter self dhcp-discover-stage-start !
      then
    ; define send-dhcprequest

    \ Send a renewal DHCPREQUEST packet
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending renewal DHCPREQUEST" ;] debug-hook execute
      then
      self
      self dhcp-server-mac-addr 2@
      self intf-ipv4-addr @ dhcp-client-port
      self dhcp-server-ipv4-addr @ dhcp-server-port
      [ dhcp-header-size 3 + 6 + ( 6 + ) 1 + ] literal [: { self buf }
        dhcp-log? if
          [: cr ." Constructing renewal DHCPREQUEST" ;] debug-hook execute
        then
        DHCP_OP_CLIENT buf dhcp-op c!
        DHCP_HTYPE buf dhcp-htype c!
        DHCP_HLEN buf dhcp-hlen c!
        0 buf dhcp-hops c!
        self current-dhcp-xid @ rev buf dhcp-xid unaligned!
        [ 0 rev16 ] literal buf dhcp-secs hunaligned!
        [ 0 rev16 ] literal buf dhcp-flags hunaligned!
        self dhcp-req-ipv4-addr @ rev buf dhcp-ciaddr unaligned!
        self dhcp-req-ipv4-addr @ rev buf dhcp-yiaddr unaligned!
        [ 0 rev ] literal buf dhcp-siaddr unaligned!
        self dhcp-server-ipv4-addr @ rev buf dhcp-siaddr unaligned!
        [ 0 rev ] literal buf dhcp-giaddr unaligned!
        buf dhcp-chaddr 16 0 fill
        self intf-mac-addr@
        rev16 buf dhcp-chaddr hunaligned!
        rev buf dhcp-chaddr 2 + unaligned!
        buf dhcp-filler 192 0 fill
        [ DHCP_MAGIC_COOKIE rev ] literal buf dhcp-magic unaligned!
        dhcp-header-size +to buf
        DHCP_MESSAGE_TYPE buf c!
        1 buf 1 + c!
        DHCPREQUEST buf 2 + c!
        3 +to buf
        DHCP_REQ_IPV4_ADDR buf c!
        4 buf 1 + c!
        self dhcp-req-ipv4-addr @ rev buf 2 + unaligned!
        6 +to buf
\        DHCP_SERVER_IPV4_ADDR buf c!
\        4 buf 1 + c!
\        self dhcp-server-ipv4-addr @ rev buf 2 + unaligned!
\        6 +to buf
        DHCP_END buf c!
        true
        dhcp-log? if
          [: cr ." Constructed renewal DHCPREQUEST packet" ;] debug-hook execute
        then
      ;] self send-ipv4-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Waiting for DHCPACK" ;] debug-hook execute
      then
      self dhcp-discover-state @
      dup dhcp-discovered <> swap dhcp-renewing <> and if
        dhcp-wait-ack self dhcp-discover-state !
      then
    ; define send-renew-dhcprequest

    \ Send a rebinding DHCPREQUEST packet
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending rebinding DHCPREQUEST" ;] debug-hook execute
      then
      self
      $FFFFFFFFFFFF.
      self intf-ipv4-addr @ dhcp-client-port
      $FFFFFFFF dhcp-server-port
      [ dhcp-header-size 3 + 6 + ( 6 + ) 1 + ] literal [: { self buf }
        dhcp-log? if
          [: cr ." Constructing rebinding DHCPREQUEST" ;] debug-hook execute
        then
        DHCP_OP_CLIENT buf dhcp-op c!
        DHCP_HTYPE buf dhcp-htype c!
        DHCP_HLEN buf dhcp-hlen c!
        0 buf dhcp-hops c!
        self current-dhcp-xid @ rev buf dhcp-xid unaligned!
        [ 0 rev16 ] literal buf dhcp-secs hunaligned!
        [ 0 rev16 ] literal buf dhcp-flags hunaligned!
        self dhcp-req-ipv4-addr @ rev buf dhcp-ciaddr unaligned!
        self dhcp-req-ipv4-addr @ rev buf dhcp-yiaddr unaligned!
        [ 0 rev ] literal buf dhcp-siaddr unaligned!
        [ 0 rev ] literal buf dhcp-siaddr unaligned!
        [ 0 rev ] literal buf dhcp-giaddr unaligned!
        buf dhcp-chaddr 16 0 fill
        self intf-mac-addr@
        rev16 buf dhcp-chaddr hunaligned!
        rev buf dhcp-chaddr 2 + unaligned!
        buf dhcp-filler 192 0 fill
        [ DHCP_MAGIC_COOKIE rev ] literal buf dhcp-magic unaligned!
        dhcp-header-size +to buf
        DHCP_MESSAGE_TYPE buf c!
        1 buf 1 + c!
        DHCPREQUEST buf 2 + c!
        3 +to buf
        DHCP_REQ_IPV4_ADDR buf c!
        4 buf 1 + c!
        self dhcp-req-ipv4-addr @ rev buf 2 + unaligned!
        6 +to buf
\        DHCP_SERVER_IPV4_ADDR buf c!
\        4 buf 1 + c!
\        self dhcp-server-ipv4-addr @ rev buf 2 + unaligned!
\        6 +to buf
        DHCP_END buf c!
        true
        dhcp-log? if
          [: cr ." Constructed rebinding DHCPREQUEST packet" ;] debug-hook execute
        then
      ;] self send-ipv4-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Waiting for DHCPACK" ;] debug-hook execute
      then
      self dhcp-discover-state @
      dup dhcp-discovered <> swap dhcp-renewing <> and if
        dhcp-wait-ack self dhcp-discover-state !
      then
    ; define send-rebind-dhcprequest

    \ Send a DHCPDECLINE packet
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending DHCPDECLINE" ;] debug-hook execute
      then
      self
      $FFFFFFFFFFFF.
      $00000000 dhcp-client-port
      $FFFFFFFF dhcp-server-port
      [ dhcp-header-size 3 + 6 + 6 + 1 + ] literal [: { self buf }
        dhcp-log? if
          [: cr ." Constructing DHCPDECLINE" ;] debug-hook execute
        then
        DHCP_OP_CLIENT buf dhcp-op c!
        DHCP_HTYPE buf dhcp-htype c!
        DHCP_HLEN buf dhcp-hlen c!
        0 buf dhcp-hops c!
        self current-dhcp-xid @ rev buf dhcp-xid unaligned!
        [ 0 rev16 ] literal buf dhcp-secs hunaligned!
        [ 0 rev16 ] literal buf dhcp-flags hunaligned!
        [ 0 rev ] literal buf dhcp-ciaddr unaligned!
        [ 0 rev ] literal buf dhcp-yiaddr unaligned!
        self dhcp-server-ipv4-addr @ rev buf dhcp-siaddr unaligned!
        [ 0 rev ] literal buf dhcp-giaddr unaligned!
        buf dhcp-chaddr 16 0 fill
        self intf-mac-addr@
        rev16 buf dhcp-chaddr hunaligned!
        rev buf dhcp-chaddr 2 + unaligned!
        buf dhcp-filler 192 0 fill
        [ DHCP_MAGIC_COOKIE rev ] literal buf dhcp-magic unaligned!
        dhcp-header-size +to buf
        DHCP_MESSAGE_TYPE buf c!
        1 buf 1 + c!
        DHCPDECLINE buf 2 + c!
        3 +to buf
        DHCP_REQ_IPV4_ADDR buf c!
        4 buf 1 + c!
        self dhcp-req-ipv4-addr @ rev buf 2 + unaligned!
        6 +to buf
        DHCP_SERVER_IPV4_ADDR buf c!
        4 buf 1 + c!
        self dhcp-server-ipv4-addr @ rev buf 2 + unaligned!
        6 +to buf
        DHCP_END buf c!
        true
        dhcp-log? if
          [: cr ." Constructed DHCPDECLINE packet" ;] debug-hook execute
        then
      ;] self send-ipv4-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Sent DHCPDECLINE" ;] debug-hook execute
      then
      dhcp-declined self dhcp-discover-state !
      systick::systick-counter self dhcp-discover-stage-start !
    ; define send-dhcpdecline

    \ Process an IPv4 DHCP packet
    :noname ( addr bytes src-addr self -- )
      [: { addr bytes src-addr self }
        dhcp-log? if
          [: cr ." Receiving DHCP packet" ;] debug-hook execute
        then
        bytes dhcp-header-size < if
          dhcp-log? if
            [: cr ." DHCP packet too small" ;] debug-hook execute
          then
          exit
        then
        addr dhcp-op c@ DHCP_OP_SERVER <> if
          dhcp-log? if
            [: cr ." DHCP packet is not a server packet" ;] debug-hook execute
          then
          exit
        then
        addr dhcp-xid unaligned@ rev self current-dhcp-xid @ <> if
          dhcp-log? if
            [: cr ." DHCP packet XID does not match" ;] debug-hook execute
          then
          exit
        then
        DHCP_MESSAGE_TYPE 1 addr bytes find-fixed-dhcp-opt if
          dhcp-log? if
            [: cr ." Got DHCP message type: " dup c@ . ;] debug-hook execute
          then
          c@ case
            DHCPOFFER of addr bytes self process-ipv4-dhcpoffer endof
            DHCPACK of addr bytes src-addr self process-ipv4-dhcpack endof
            DHCPNAK of addr bytes self process-ipv4-dhcpnak endof
          endcase
        else
          dhcp-log? if
            [: cr ." Did not get message type" ;] debug-hook execute
          then
          drop exit
        then
      ;] over dhcp-lock with-lock
    ; define process-ipv4-dhcp-packet

    \ Process an IPv4 DHCPOFFER packet
    :noname { addr bytes self -- }
      dhcp-log? if
        [: cr ." Sending DHCPREQUEST" ;] debug-hook execute
      then
      addr dhcp-yiaddr unaligned@ rev self dhcp-req-ipv4-addr !
      addr dhcp-siaddr unaligned@ rev self dhcp-server-ipv4-addr !
      self send-dhcprequest
    ; define process-ipv4-dhcpoffer

    \ Process an IPv4 DHCPACK packet
    :noname { addr bytes src-addr self -- }
      src-addr self address-map lookup-mac-addr-by-ipv4 if
        self dhcp-server-mac-addr 2!
      else
        2drop $FFFFFFFFFFFF. self dhcp-server-mac-addr 2!
      then
      addr dhcp-yiaddr unaligned@ rev self dhcp-req-ipv4-addr @ = if
        self dhcp-req-ipv4-addr @ { ipv4-addr }
        DHCP_IPV4_ADDR_LEASE_TIME 4 addr bytes find-fixed-dhcp-opt if
          dhcp-log? if
            [: cr ." Got DHCP lease time" ;] debug-hook execute
          then
          unaligned@ rev
          dhcp-log? if
            [: ." : " dup . ;] debug-hook execute
          then
        else
          dhcp-log? if
            [: cr ." Did not find lease time" ;] debug-hook execute
          then
          drop 86400
        then
        { renew-interval }
        DHCP_SERVICE_SUBNET_MASK 4 addr bytes find-fixed-dhcp-opt if
          dhcp-log? if
            [: cr ." Got DHCP netmask" ;] debug-hook execute
          then
          unaligned@ rev { ipv4-netmask }
          DHCP_SERVICE_ROUTER 4 addr bytes find-fixed-dhcp-opt if
            dhcp-log? if
              [: cr ." Got DHCP gateway" ;] debug-hook execute
            then
            unaligned@ rev { gateway-ipv4-addr }
            DHCP_SERVICE_DNS_SERVER addr bytes find-var-dhcp-opt if
              dhcp-log? if
                [: cr ." Got DHCP DNS server(s)" ;] debug-hook execute
              then
              4 >= if
                dhcp-log? if
                  [: cr ." Found DHCPACK fields" ;] debug-hook execute
                then
                unaligned@ rev { dns-server-ipv4-addr }
                ipv4-netmask self prov-ipv4-netmask !
                gateway-ipv4-addr self prov-gateway-ipv4-addr !
                dns-server-ipv4-addr self prov-dns-server-ipv4-addr !
                renew-interval 10000 * self prov-dhcp-renew-interval !
                self dhcp-discover-state @ dhcp-renewing =
                self dhcp-discover-state @ dhcp-rebinding = or if
                  self apply-dhcp
                else
                  dhcp-arp-attempt-count self dhcp-remaining-arp-attempts !
                  dhcp-wait-confirm self dhcp-discover-state !
                  systick::systick-counter self dhcp-discover-stage-start !
                then
                dhcp-log? if
                  [: cr ." Processed DHCPACK" ;] debug-hook execute
                then
              else
                dhcp-log? if
                  [: cr ." Insufficient DNS servers" ;] debug-hook execute
                then
                drop
              then
            else
              dhcp-log? if
                [: cr ." Did not find DNS server" ;] debug-hook execute
              then
              drop
            then
          else
            dhcp-log? if
              [: cr ." Did not find router" ;] debug-hook execute
            then
            drop
          then
        else
          dhcp-log? if
            [: cr ." Did not find netmask" ;] debug-hook execute
          then
          drop
        then
      else
        dhcp-log? if
          [: cr ." yiaddr does not match" ;] debug-hook execute
        then
      then
    ; define process-ipv4-dhcpack

    \ Process an IPv4 DHCPNAK packet
    :noname { addr bytes self -- }
      dhcp-log? if
        [: cr ." Got DHCPNAK" ;] debug-hook execute
      then
      0 self dhcp-req-ipv4-addr !
      $FFFFFFFFFFFF. self dhcp-server-mac-addr 2!
      0 self dhcp-server-ipv4-addr !
      dhcp-got-nak self dhcp-discover-state !
    ; define process-ipv4-dhcpnak

    \ Refresh DHCP rebinding
    :noname { self -- }
      systick::systick-counter self dhcp-real-renew-start @ -
      self dhcp-real-renew-interval @ > if
        dhcp-log? if
          [: cr ." Rebinding faied, issuing DHCPDISCOVER" ;]
          debug-hook execute
        then
        self send-dhcpdiscover
      then
    ; define refresh-dhcp-rebinding
    
    \ Refresh DHCP renewal
    :noname { self -- }
      systick::systick-counter self dhcp-rebind-start @ -
      self dhcp-rebind-interval @ > if
        dhcp-rebinding self dhcp-discover-state !
        dhcp-rebind-retry-interval self dhcp-rebind-interval !
        dhcp-log? if
          [: cr ." Attempting rebinding, issuing DHCPREQUEST" ;]
          debug-hook execute
        then
        systick::systick-counter self dhcp-rebind-start !
        self send-rebind-dhcprequest
      else
        systick::systick-counter self dhcp-renew-start @ -
        self dhcp-renew-interval @ > if
          dhcp-log? if
            [: cr ." Attempting renewing, issuing DHCPREQUEST" ;]
            debug-hook execute
          then
          systick::systick-counter self dhcp-renew-start !
          self send-renew-dhcprequest
        then
      then
    ; define refresh-dhcp-renewing

    \ Refresh DHCP discovered
    :noname { self -- }
      systick::systick-counter self dhcp-renew-start @ -
      self dhcp-renew-interval @ > if
        dhcp-renewing self dhcp-discover-state !
        self dhcp-renew-interval @ dhcp-renew-retry-divisor /
        self dhcp-renew-interval !
        dhcp-log? if
          [: cr ." Attempting renewing, issuing DHCPREQUEST" ;]
          debug-hook execute
        then
        systick::systick-counter self dhcp-renew-start !
        self send-renew-dhcprequest
      then
    ; define refresh-dhcp-discovered

    \ Refresh DHCP got nak
    :noname { self -- }
      dhcp-log? if
        [: cr ." Got DHCPNAK, retrying DHCPDISCOVER" ;] debug-hook execute
      then
      self send-dhcpdiscover
    ; define refresh-dhcp-got-nak

    \ Refresh DHCP wait offer
    :noname { self -- }
      systick::systick-counter self dhcp-discover-stage-start @ -
      dhcp-discover-timeout > if
        systick::systick-counter self dhcp-discover-stage-start !
        dhcp-log? if
          [: cr ." Timeout, retrying DHCPDISCOVER" ;] debug-hook execute
        then
        self send-dhcpdiscover
      then
    ; define refresh-dhcp-wait-offer

    \ Refresh DHCP wait ACK
    :noname { self -- }
      systick::systick-counter self dhcp-discover-stage-start @ -
      dhcp-discover-timeout > if
        systick::systick-counter self dhcp-discover-stage-start !
        dhcp-log? if
          [: cr ." TImeout, retrying DHCPREQUEST" ;] debug-hook execute
        then
        self send-dhcprequest
      then
    ; define refresh-dhcp-wait-ack

    \ Refresh DHCP wait confirm
    :noname { self -- }
      systick::systick-counter self dhcp-discover-stage-start @ -
      dhcp-arp-interval > if
        dhcp-log? if
          [: cr ." Trying to confirm DHCP with ARP..." ;] debug-hook execute
        then
        self dhcp-req-ipv4-addr @ self address-map
        lookup-mac-addr-by-ipv4 not -rot 2drop if
          -1 self dhcp-remaining-arp-attempts +!
          self dhcp-remaining-arp-attempts @ 0> if
            self dhcp-req-ipv4-addr @ self send-ipv4-dhcp-arp-request
            systick::systick-counter self dhcp-discover-stage-start !
          else
            dhcp-log? if
              [: cr ." DHCP is now accepted" ;] debug-hook execute
            then
            self apply-dhcp
          then
        else
          dhcp-log? if
            [: cr ." DHCP is declined" ;] debug-hook execute
          then
          self send-dhcpdecline
        then
      then      
    ; define refresh-dhcp-wait-confirm

    \ Refresh DHCP declined
    :noname { self -- }
      systick::systick-counter self dhcp-discover-stage-start @ -
      dhcpdecline-delay > if
        dhcp-log? if
          [: cr ." Retrying DHCP discovery" ;] debug-hook execute
        then
        self send-dhcpdiscover
      then
    ; define refresh-dhcp-declined

    \ Implement a jumptable for DHCP states
    create dhcp-jumptable
    ' drop ,
    ' refresh-dhcp-wait-offer ,
    ' refresh-dhcp-wait-ack ,
    ' refresh-dhcp-got-nak ,
    ' refresh-dhcp-wait-confirm ,
    ' refresh-dhcp-discovered ,
    ' refresh-dhcp-renewing ,
    ' refresh-dhcp-rebinding ,
    ' refresh-dhcp-declined ,
    
    \ Refresh DHCP
    :noname ( self -- )
      [:
        dup dhcp-discover-state @ cells dhcp-jumptable + @ execute
      ;] over dhcp-lock with-lock
    ; define refresh-dhcp
    
    \ Refresh an interface
    :noname { self -- }
      self refresh-dhcp
      self refresh-time-wait
      max-endpoints 0 ?do
        self intf-endpoints <endpoint> class-size i * +
        self [: { endpoint self }
          endpoint endpoint-refresh-ready? if
            endpoint endpoint-tcp-state@ { state }
            [ debug? ] [if] cr ." ENDPOINT " endpoint h.8 ."  STATE: " state . [then]
            state TCP_ESTABLISHED =
            state TCP_SYN_RECEIVED = or
            state TCP_CLOSE_WAIT = or if
              endpoint endpoint-ipv4-remote@
              endpoint endpoint-local-port@
              endpoint endpoint-local-seq@
              state TCP_SYN_RECEIVED = if 1- then
              endpoint endpoint-ack@
              endpoint endpoint-local-window@
              state case
                TCP_ESTABLISHED of TCP_ACK endof
                TCP_SYN_RECEIVED of [ TCP_SYN TCP_ACK or ] literal endof
                TCP_CLOSE_WAIT of TCP_ACK endof
              endcase
              endpoint endpoint-remote-mac-addr@
              self send-ipv4-basic-tcp
              endpoint endpoint-ack-sent
              endpoint advance-endpoint-refresh
            else
              state TCP_SYN_SENT = if
                endpoint reset-endpoint-local-port
                endpoint endpoint-ipv4-remote@
                endpoint endpoint-local-port@
                endpoint endpoint-init-local-seq@ 1+
                endpoint endpoint-init-local-seq!
                endpoint endpoint-local-seq@ 1-
                0
                [ mtu-size ethernet-header-size -
                ipv4-header-size - tcp-header-size - ] literal
                TCP_SYN
                endpoint endpoint-remote-mac-addr@
                self send-ipv4-basic-tcp
                endpoint advance-endpoint-refresh
              else
                state TCP_FIN_WAIT_1 = state TCP_LAST_ACK = or if
                  state endpoint self send-fin
                  endpoint advance-endpoint-refresh
                then
              then
            then
            endpoint escalate-endpoint-refresh
          then
        ;] 2 pick with-endpoint
      loop
    ; define refresh-interface

  end-implement

  \ The IP protocol handler
  <frame-handler> begin-class <ip-handler>

    continue-module net-internal
    
      \ The IP interface
      cell member ip-interface

    end-module

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
          bytes addr ipv4-total-len h@ rev16 min to bytes
          addr ipv4-version-ihl c@ $F and dup { ihl } 5 >=
          ihl 4 * bytes <= and if
            addr ipv4-dest-addr unaligned@ rev
            dup self ip-interface @ intf-ipv4-addr@ =
            over self ip-interface @ intf-ipv4-broadcast@ = or
            over $FFFFFFFF = or
            self ip-interface @ dhcp-discover-state @ dhcp-wait-ack =
            rot self ip-interface @ dhcp-req-ipv4-addr @ = and or
            self ip-interface @ dhcp-discover-state @ dhcp-wait-offer = or if
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

    continue-module net-internal
      
      \ The ARP IP interface
      cell member arp-interface
      
      \ Send an ARP response
      method send-arp-response ( addr self -- )
      
    end-module
    
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