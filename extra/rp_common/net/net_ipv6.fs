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

begin-module net-ipv6

  oo import
  frame-interface import
  frame-process import
  net-consts import
  net-config import
  net-misc import
  net import
  lock import
  sema import
  chan import
  heap import

  \ Out of range prefix length
  : x-out-of-range-prefix-len ( -- ) ." out of range prefix length" cr ;

  \ zeptoIP internals
  begin-module net-ipv6-internal

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
        [ mtu-size ipv6-header-size - tcp-header-size - ]
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
      max-in-packets ipv6-addr-size * member in-packet-ipv6-addrs

      \ The incoming UDP packets source ports
      max-in-packets 1 lshift member in-packet-ports

      \ The pending UDP packet source IP address
      ipv6-addr-size member in-packet-current-ipv6-addr

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
      method in-packets-udp-remote@ ( self -- ipv6-addr port )
      
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
        self in-packet-ipv6-addrs max-in-packets ipv6-addr-size * 0 fill
        self in-packet-ports max-in-packets 1 lshift 0 fill
        0 0 0 0 self in-packet-current-ipv6-addr ipv6-unaligned!
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
        self in-packet-ipv6-addrs max-in-packets ipv6-addr-size * 0 fill
        self in-packet-ports max-in-packets 1 lshift 0 fill
        0 0 0 0 self in-packet-current-ipv6-addr ipv6-unaligned!
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
      :noname { addr bytes src0 src1 src2 src3 src-port self -- }
        self in-packet-offset @ bytes + self in-packet-bytes @ <=
        self in-packet-count @ max-in-packets < and if
          addr self in-packet-addr @ self in-packet-offset @ + bytes move
          bytes self in-packet-offset +!
          bytes self in-packet-sizes self in-packet-count @ 1 lshift + h!
          src0 src1 src2 src3 self in-packet-ipv6-addrs
          self in-packet-count @ ipv6-addr-size * + !
          src-port self in-packet-ports self in-packet-count @ 1 lshift + h!
          1 self in-packet-count +!
        then
        [ debug? ] [if]
          self [: cr ." @@@ add-in-udp-packet: " in-packets. ;]
          debug-hook execute
        [then]
      ; define add-in-udp-packet
      
      \ Get the incoming packets UDP remote address and port
      :noname ( self -- ipv6-addr0 ipv6-addr1 ipv6-addr2 ipv6-addr3 port )
        dup in-packet-current-ipv6-addr ipv6-unaligned@
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
            self in-packet-ipv6-addrs ipv6-unaligned@
            self in-packet-current-ipv6-addr ipv6-unaligned!
            self in-packet-ports h@ self in-packet-current-port !
            -1 self in-packet-count +!
            self in-packet-count @ 0> if
              self in-packet-sizes 2 + self in-packet-sizes
              self in-packet-count @ 1 lshift move
              self in-packet-ipv6-addrs ipv6-addr-size +
              self in-packet-ipv6-addrs
              self in-packet-count @ ipv6-addr-size * move
              self in-packet-ports 2 + self in-packet-ports
              self in-packet-count @ 1 lshift move
            then
          else
            0 self pending-in-packet-offset !
            0 0 0 0 self in-packet-current-ipv6-addr ipv6-unaligned!
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
        0 0 0 0 self in-packet-current-ipv6-addr ipv6-unaligned!
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
      
      \ The cached IPv6 addresses
      max-dns-cache ipv6-addr-size * member cached-ipv6-addrs

      \ The cached DNS names
      max-dns-cache cells member cached-dns-names

      \ The IP address identification codes
      max-dns-cache cells member cached-dns-idents
      
      \ The cached IPv6 ages
      max-dns-cache cells member cached-dns-ages

      \ The IPv6 response codes
      max-dns-cache cells member cached-dns-responses

      \ The newest age
      cell member newest-dns-age

      \ The cache lock
      lock-size member dns-cache-lock

      \ The DNS cache name heap
      dns-cache-heap-block-size dns-cache-heap-block-count heap-size
      member dns-cache-name-heap

      \ Look up an IPv6 address by a DNS name
      method lookup-ipv6-addr-by-dns
      ( c-addr bytes self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 response found? )

      \ Reserve a DNS entry
      method reserve-dns ( ident c-addr bytes self -- )

      \ Evict a DNS entry
      method evict-dns ( c-addr bytes self -- )
      
      \ Save an IPv6 address by a DNS name
      method save-ipv6-addr-by-dns
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 ident c-addr bytes self -- )

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
        self cached-ipv6-addrs
        [ max-dns-cache ipv6-addr-size * ] literal $FF fill
        self cached-dns-idents [ max-dns-cache cells ] literal $FF fill
        self cached-dns-ages [ max-dns-cache cells ] literal 0 fill
        self cached-dns-responses [ max-dns-cache cells ] literal $FF fill
        0 self newest-dns-age !
        self dns-cache-lock init-lock
        dns-cache-heap-block-size dns-cache-heap-block-count
        self dns-cache-name-heap init-heap
      ; define new

      \ Look up an IPv6 address by a DNS name
      :noname ( c-addr bytes self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 response found? )
        [: { c-addr bytes self }
          c-addr bytes validate-dns-name
          max-dns-cache 0 ?do
            self cached-dns-names i cells + @ ?dup if
              count c-addr bytes equal-case-strings?
              -1 self cached-dns-idents i cells + @ = and if
                self newest-dns-age @ 1+ dup self newest-dns-age !
                self cached-dns-ages i cells + !
                self cached-ipv6-addrs i ipv6-addr-size * + ipv6-unaligned@
                self cached-dns-responses i cells + @
                true unloop exit
              then
            then
          loop
          0 -1 false
        ;] over dns-cache-lock with-lock
      ; define lookup-ipv6-addr-by-dns

      \ Save an IPv6 address by a DNS name
      :noname ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 ident c-addr bytes self -- )
        [: { ipv6-0 ipv6-1 ipv6-2 ipv6-3 ident c-addr bytes self }
          max-dns-cache 0 ?do
            self cached-dns-names i cells + @ ?dup if
              count c-addr bytes equal-case-strings?
              self cached-dns-idents i cells + @ ident = and if
                ipv6-0 ipv6-1 ipv6-2 ipv6-3
                self cached-ipv6-addrs i ipv6-addr-size * + ipv6-unaligned!
                -1 self cached-dns-idents i cells + !
                self newest-dns-age @ 1+ dup self newest-dns-age !
                self cached-dns-ages i cells + !
                0 self cached-dns-responses i cells + !
                unloop exit
              then
            then
          loop
        ;] over dns-cache-lock with-lock
      ; define save-ipv6-addr-by-dns

      \ Reserve a DNS entry
      :noname ( ident c-addr bytes self -- )
        [: { ident c-addr bytes self }
          max-dns-cache 0 ?do
            self cached-dns-names i cells + @ ?dup if
              count c-addr bytes equal-case-strings? if
                ident self cached-dns-idents i cells + !
                0 0 0 0 self cached-ipv6-addrs
                i ipv6-addr-size * + ipv6-unaligned!
                self newest-dns-age @ 1+ dup self newest-dns-age !
                self cached-dns-ages i cells + !
                -1 self cached-dns-responses i cells + !
                unloop exit
              then
            then
          loop
          c-addr bytes self save-dns-name { index }
          ident self cached-dns-idents index cells + !
          0 0 0 0 self cached-ipv6-addrs
          index ipv6-addr-size * + ipv6-unaligned!
          self newest-dns-age @ 1+ dup self newest-dns-age !
          self cached-dns-ages index cells + !
          -1 self cached-dns-responses index cells + !
        ;] over dns-cache-lock with-lock
      ; define reserve-dns

      \ Evict a DNS entry
      :noname ( c-addr bytes self -- )
        [: { c-addr bytes self }
          c-addr bytes validate-dns-name
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
              0 0 0 0 self cached-ipv6-addrs i ipv6-addr-size * + ipv6-unaligned!
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

    \ The IPv6 address map
    <object> begin-class <address-map>

      \ The mapped IPv6 addresses
      max-addresses ipv6-addr-size * member mapped-ipv6-addrs

      \ The mapped MAC addresses
      max-addresses 2 cells * member mapped-mac-addrs

      \ The mapped address ages
      max-addresses cells member mapped-addr-ages

      \ The mapped address discovery times
      max-addresses cells member mapped-addr-discovery-times

      \ The newest age
      cell member newest-addr-age

      \ The address map lock
      lock-size member address-map-lock

      \ Age out IPv6 addresses
      method age-out-mac-addrs ( reachable-ms self -- )

      \ Look up a MAC address by an IPv6 address
      method lookup-mac-addr-by-ipv6
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- D: mac-addr found? )

      \ Save a MAC address by an IPv6 address
      method save-mac-addr-by-ipv6
      ( D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 discovery-time self -- )
      
      \ Get the oldest MAC address index
      method oldest-mac-addr-index ( self -- index )

      \ Store a MAC address by an IPv6 address at an index
      method save-mac-addr-at-index
      ( D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 discovery-time index self -- )
      
    end-class

    \ Implement the IPv6 address map
    <address-map> begin-implement

      \ Constructor
      :noname { self -- }
        self <object>->new
        self mapped-ipv6-addrs [ max-addresses ipv6-addr-size * ] literal 0 fill
        self mapped-mac-addrs [ max-addresses 2 cells * ] literal $FF fill
        self mapped-addr-ages [ max-addresses cells ] literal 0 fill
        self mapped-addr-discovery-times [ max-addresses cells ] literal 0 fill
        0 self newest-addr-age !
        self address-map-lock init-lock
      ; define new

      \ Age out IPv6 addresses
      :noname { reachable-ms self -- }
        [ $FFFFFFFF 10 u/ ] literal reachable-ms min 10 * { reachable-time }
        systick::systick-counter { time }
        max-addresses 0 ?do
          self mapped-mac-addrs i [ 2 cells ] literal * + 2@ -1. <> if
            time self mapped-addr-discovery-times i cells + @ -
            reachable-time > if
              0 0 0 0 self mapped-ipv6-addrs i ipv6-addr-size * + ipv6-unaligned!
              -1. self mapped-mac-addrs i [ 2 cells ] literal * + 2!
              0 self mapped-addr-ages i cells + !
              0 self mapped-addr-discovery-times i cells + !
            then
          then
        loop
      ; define age-out-mac-addrs

      \ Look up a MAC address by an IPv6 address
      :noname ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- D: mac-addr found? )
        [: { ipv6-0 ipv6-1 ipv6-2 ipv6-3 self }
          max-addresses 0 ?do
            self mapped-ipv6-addrs i ipv6-addr-size * + ipv6-unaligned@
            ipv6-0 ipv6-1 ipv6-2 ipv6-3 ipv6= if
              self mapped-mac-addrs i 2 cells * + 2@
              self newest-addr-age @ 1+ dup self newest-addr-age !
              self mapped-addr-ages i cells + !
              true unloop exit
            then
          loop
          0. false
        ;] over address-map-lock with-lock
      ; define lookup-mac-addr-by-ipv6

      \ Save a MAC address by an IPv6 address
      :noname
        ( D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 discovery-time self -- )
        [: { D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 discovery-time self }
          ipv6-0 ipv6-1 ipv6-2 ipv6-3 0 0 0 0 ipv6= not if
            max-addresses 0 ?do
              self mapped-ipv6-addrs i ipv6-addr-size * + ipv6-unaligned@
              ipv6-0 ipv6-1 ipv6-2 ipv6-3 ipv6= if
                mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 discovery-time i
                self save-mac-addr-at-index unloop exit
              then
            loop
            self oldest-mac-addr-index { index }
            mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 discovery-time index
            self save-mac-addr-at-index
          then
        ;] over address-map-lock with-lock
      ; define save-mac-addr-by-ipv6

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

      \ Store a MAC address by an IPv6 address at an index
      :noname
        { D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 discovery-time index self -- }
        mac-addr self mapped-mac-addrs index 2 cells * + 2!
        ipv6-0 ipv6-1 ipv6-2 ipv6-3
        discovery-time self mapped-addr-discovery-times index cells + !
        self mapped-ipv6-addrs index ipv6-addr-size * + ipv6-unaligned!
        self newest-addr-age @ 1+ dup self newest-addr-age !
        self mapped-addr-ages index cells + !
      ; define save-mac-addr-at-index

    end-implement

  end-module> import
  
  \ The endpoint class
  <endpoint> begin-class <ipv6-endpoint>

    continue-module net-ipv6-internal
      
      \ Is the endpoint state
      cell member endpoint-state

      \ The endpoint queue state
      cell member endpoint-queue-state

      \ The current endpoint ID
      cell member endpoint-id
      
      \ The remote IPv6 address
      ipv6-addr-size member endpoint-remote-ipv6-addr

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
      method endpoint-ipv6-remote! ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 port self -- )

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
      method try-ipv6-accept-endpoint
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 src-port dest-port D: mac-addr self )
      ( -- accepted? )

      \ Try to open a connection on an endpoint
      method try-ipv6-connect-endpoint
      ( src-port ipv6-0 ipv6-1 ipv6-2 ipv6-3 dest-port D: mac-addr self )
      ( -- allocated? )

      \ Match an endpoint with an IPv6 TCP connection
      method match-ipv6-connect?
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 src-port dest-port self -- match? )
      
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
      method add-endpoint-udp-data
      ( addr bytes src-0 src-1 src-2 src-3 src-port self -- )

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
    
    \ Get endpoint source
    method endpoint-ipv6-remote@ ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 port )

  end-class

  \ Implement the endpoint class
  <ipv6-endpoint> begin-implement

    \ Constructor
    :noname { self -- }
      self <endpoint>->new
      0 self endpoint-state !
      0 self endpoint-queue-state !
      0 0 0 0 self endpoint-remote-ipv6-addr ipv6-unaligned!
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
    :noname ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 port )
      dup udp-endpoint? not if
        dup endpoint-remote-ipv6-addr ipv6-unaligned@
        swap endpoint-remote-port @
      else
        endpoint-in-packets in-packets-udp-remote@
      then
    ; define endpoint-ipv6-remote@
    
    \ Set endpoint source
    :noname { ipv6-0 ipv6-1 ipv6-2 ipv6-3 port self -- }
      ipv6-0 ipv6-1 ipv6-2 ipv6-3 self endpoint-remote-ipv6-addr ipv6-unaligned!
      port self endpoint-remote-port !
    ; define endpoint-ipv6-remote!

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
    :noname
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 src-port dest-port D: mac-addr self )
      ( -- accepted? )
      [: { ipv6-0 ipv6-1 ipv6-2 ipv6-3 src-port dest-port D: mac-addr self }
        self endpoint-tcp-state@ dup TCP_LISTEN = swap TCP_SYN_RECEIVED = or
        dest-port self endpoint-local-port@ = and if
          self endpoint-tcp-state@ TCP_LISTEN = if
            ipv6-0 ipv6-1 ipv6-2 ipv6-3 src-port self endpoint-ipv6-remote!
            TCP_SYN_RECEIVED self endpoint-tcp-state!
            mac-addr self endpoint-remote-mac-addr 2!
            false self endpoint-event !
            self reset-endpoint-refresh
            true
          else
            self endpoint-ipv6-remote@ src-port = { port-match? }
            ipv6-0 ipv6-1 ipv6-2 ipv6-3 ipv6= port-match? and
          then
        else
          false
        then
      ;] over endpoint-lock with-lock
    ; define try-ipv6-accept-endpoint

    \ Try to open a connection on an endpoint
    :noname
      ( src-port ipv6-0 ipv6-1 ipv6-2 ipv6-3 dest-port D: mac-addr self )
      ( -- allocated? )
      [: { src-port ipv6-0 ipv6-1 ipv6-2 ipv6-3 dest-port D: mac-addr self }
        self try-allocate-endpoint if
          src-port self endpoint-local-port!
          ipv6-0 ipv6-1 ipv6-2 ipv6-3 dest-port self endpoint-ipv6-remote!
          TCP_SYN_SENT self endpoint-tcp-state!
          mac-addr self endpoint-remote-mac-addr 2!
          false self endpoint-event !
          self reset-endpoint-refresh
          true
        else
          false
        then
      ;] over endpoint-lock with-lock
    ; define try-ipv6-connect-endpoint

    \ Match an endpoint with an IPv6 TCP connection
    :noname ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 src-port dest-port self -- match? )
      [: { ipv6-0 ipv6-1 ipv6-2 ipv6-3 src-port dest-port self }
        self endpoint-tcp-state@ dup TCP_CLOSED <> swap TCP_LISTEN <> and if
          self endpoint-ipv6-remote@ src-port = { port-match? }
          ipv6-0 ipv6-1 ipv6-2 ipv6-3 ipv6= port-match? and
          self endpoint-local-port@ dest-port = and
        else
          false
        then
      ;] over endpoint-lock with-lock
    ; define match-ipv6-connect?

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
    :noname ( addr bytes src-0 src-1 src-2 src-3 self -- )
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
  
  continue-module net-ipv6-internal
    
    \ TCP_WAIT class
    <object> begin-class <time-wait>

      \ TCP wait remote MAC address
      2 cells member time-wait-remote-mac-addr
      
      \ TCP wait remote IPv6 address
      ipv6-addr-size member time-wait-remote-ipv6-addr
      
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
        endpoint endpoint-ipv6-remote@
        self time-wait-remote-port h!
        self time-wait-remote-ipv6-addr ipv6-unaligned!
        endpoint endpoint-local-port@ self time-wait-local-port h!
        endpoint endpoint-remote-mac-addr@ self time-wait-remote-mac-addr 2!
        endpoint endpoint-local-seq@ 1+ self time-wait-local-seq !
        seq 1+ self time-wait-ack !
        endpoint endpoint-local-window@ self time-wait-local-window !
        systick::systick-counter self time-wait-start-time !
      ;] 2 pick with-endpoint
    ; define time-wait!

  end-implement

  \ The interface class
  <interface> begin-class <ipv6-interface>

    continue-module net-ipv6-internal
      
      \ The output frame interface
      cell member out-frame-interface
      
      \ The IPv6 address
      ipv6-addr-size member intf-ipv6-addr

      \ The IPv6 prefix
      ipv6-addr-size member intf-ipv6-prefix

      \ The IPv6 prefix length
      cell member intf-ipv6-prefix-len
      
      \ The IPv6 autonomous state
      cell member intf-autonomous?

      \ The discovered IPv6 address
      ipv6-addr-size member discovered-ipv6-addr

      \ The DNS server
      ipv6-addr-size member dns-server-ipv6-addr

      \ The gateway IPv6 address
      ipv6-addr-size member gateway-ipv6-addr
      
      \ Current hop limit
      cell member intf-hop-limit

      \ MAC address resolution semaphore
      sema-size member neighbor-discovery-sema

      \ Is a router discovered
      cell member router-discovered?

      \ Are we detecting duplicates
      cell member detecting-duplicate?
      
      \ Are we discovering a router
      cell member router-discovery?

      \ Are we to get an IP address with DHCPv6
      cell member use-dhcpv6?

      \ Are we to only get a DNS server address with DHCPv6
      cell member use-dhcpv6-other?

      \ The router lifetime in seconds
      cell member router-lifetime

      \ The neighbor reachable time in milliseconds
      cell member neighbor-reachable-time

      \ The neighbor retransmit time in milliseconds
      cell member neighbor-retrans-time
      
      \ Current DHCP transaction id
      cell member current-dhcp-transact-id

      \ Current DHCP request IPv6 address
      ipv6-addr-size member dhcp-req-ipv6-addr

      \ Current DHCP server DUID
      max-duid-size member dhcp-server-duid

      \ Router discovery start
      cell member router-discovery-start

      \ Router discovered start
      cell member router-discovered-start
      
      \ Current DHCP server DUID length
      cell member dhcp-server-duid-len

      \ DHCP discovery state
      cell member dhcp-discover-state

      \ DHCP discovery start
      cell member dhcp-discover-start
      
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

      \ Router semaphore
      sema-size member router-discovery-sema
      
      \ DHCP semaphore
      sema-size member dhcp-sema

      \ Found DNS semaphore
      sema-size member found-dns-sema
      
      \ DNS resolution semaphore
      sema-size member dns-resolve-sema

      \ The endpoints
      <ipv6-endpoint> class-size max-endpoints * member intf-endpoints

      \ The address map
      <address-map> class-size member address-map

      \ The DNS cache
      <dns-cache> class-size member dns-cache
      
      \ Outgoing buffer lock
      lock-size member outgoing-buf-lock

      \ This is to force outgoing-buf to be offset by two bytes
      2 member outgoing-buf-offset

      \ The outgoing frame buffer
      ethernet-frame-size cell align 2 + member outgoing-buf

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

      \ Router discovery lock
      lock-size member router-discovery-lock
      
      \ DHCP lock
      lock-size member dhcp-lock

      \ Are we listening to IPv6 address
      method ipv6-addr-listen?
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- listen? )

      \ Process a MAC address for an IPv6 address
      method process-ipv6-mac-addr
      ( D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )

      \ Process an IPv6 packet
      method process-ipv6-packet
      ( D: src-mac-addr src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 )
      ( protocol addr bytes self -- )

      \ Find a listening IPv6 TCP endpoint
      method find-listen-ipv6-endpoint
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- endpoint found? )

      \ Find a connecting/connected IPv6 TCP endpoint
      method find-connect-ipv6-endpoint
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- endpoint found? )
      
      \ Process an IPv6 TCP packet
      method process-ipv6-tcp-packet
      ( D: src-mac-addr src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 )
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 protocol addr bytes self -- )

      \ Process an IPv6 TCP SYN packet
      method process-ipv6-syn-packet
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )

      \ Send a TCP SYN packet
      method send-syn ( endpoint self -- )
      
      \ Send an IPv6 TCP SYN+ACK packet
      method send-ipv6-syn-ack
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes endpoint self -- ) 

      \ Send an IPv6 TCP RST packet in response to a packet
      method send-ipv6-rst-for-packet
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )

      \ Send a generic IPv6 TCP RST packet
      method send-ipv6-rst ( endpoint self -- )
      
      \ Send a basic IPv6 TCP packet
      method send-ipv6-basic-tcp
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 remote-port local-port seq ack window )
      ( flags D: mac-addr self -- )

      \ Send an IPv6 TCP packet with an MSS option
      method send-ipv6-tcp-with-mss
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 remote-port local-port seq ack window )
      ( flags D: mac-addr self -- )
      
      \ Process an IPv6 TCP SYN+ACK packet
      method process-ipv6-syn-ack-packet
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )

      \ Process an IPv6 ACK packet
      method process-ipv6-ack-packet
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )

      \ Process an IPv6 FIN+ACK packet
      method process-ipv6-fin-ack-packet
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )

      \ Process an IPv6 ACK packet in the general case
      method process-ipv6-basic-ack ( addr bytes endpoint self -- )

      \ Process an IPv6 ACK packet in TCP_SYN_SENT state
      method process-ipv6-ack-syn-sent ( addr bytes endpoint self -- )

      \ Process an IPv6 ACK packet in TCP_SYN_RECEIVED state
      method process-ipv6-ack-syn-received ( addr bytes endpoint self -- )
      
      \ Process an IPv6 ACK packet in TCP_ESTABLISHED state
      method process-ipv6-ack-established ( addr bytes endpoint self -- )
      
      \ Process an IPv6 ACK packet in TCP_FIN_WAIT_1 state
      method process-ipv6-ack-fin-wait-1 ( addr bytes endpoint self -- )
      
      \ Process an IPv6 ACK packet in TCP_FIN_WAIT_2 state
      method process-ipv6-ack-fin-wait-2 ( addr bytes endpoint self -- )
      
      \ Process an IPv6 ACK packet in TCP_CLOSE_WAIT state
      method process-ipv6-ack-close-wait ( addr bytes endpoint self -- )
      
      \ Process an IPv6 ACK packet in TCP_LAST_ACK state
      method process-ipv6-ack-last-ack ( addr bytes endpoint self -- )

      \ Process an errant IPv6 ACK packet
      method send-ipv6-rst-for-ack ( addr bytes endpoint self -- )

      \ Process an IPv6 FIN/ACK packet for a TCP_ESTABLISHED state
      method process-ipv6-ack-fin-established ( addr bytes endpoint self -- )

      \ Process an IPv6 FIN/ACK packet for a TCP_FIN_WAIT_1 state
      method process-ipv6-ack-fin-fin-wait-1 ( addr bytes endpoint self -- )
      
      \ Process an IPv6 FIN/ACK packet for a TCP_FIN_WAIT_2 state
      method process-ipv6-ack-fin-fin-wait-2 ( addr bytes endpoint self -- )

      \ Process an unexpected IPv6 FIN packet
      method process-ipv6-unexpected-fin ( addr bytes endpoint self -- )

      \ Send an ACK in response to a FIN packet
      method send-ipv6-fin-reply-ack ( addr bytes endpoint self -- )
      
      \ Process an IPv6 RST packet
      method process-ipv6-rst-packet
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )
      
      \ Process an IPv6 UDP packet
      method process-ipv6-udp-packet
      ( D: src-mac-addr src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 )
      ( protocol addr bytes self -- )

      \ Process an IPv6 DNS response packet
      method process-ipv6-dns-packet ( addr bytes self -- )
      
      \ Process an ICMPv6 packet
      method process-icmpv6-packet
      ( D: src-mac-addr src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 )
      ( protocol addr bytes self -- )

      \ Send an ICMPv6 neighbor solicit packet
      method send-icmpv6-neighbor-solicit
      ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )

      \ Send an ICMPv6 router solicit packet
      method send-icmpv6-router-solicit ( self -- )
      
      \ Process an ICMPv6 echo request packet
      method process-icmpv6-echo-request-packet
      ( D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )

      \ Process an ICMPv6 neighbor solicit packet
      method process-icmpv6-neighbor-solicit-packet
      ( D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )

      \ Process an ICMPv6 neighbor advertise packet
      method process-icmpv6-neighbor-advertise-packet
      ( D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )

      \ Process an ICMPv6 router advertise packet
      method process-icmpv6-router-advertise-packet
      ( D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 addr bytes self -- )

      \ Construct and send a frame
      method construct-and-send-frame
      ( ? bytes xt self -- ? sent? ) ( xt: ? buf -- ? send? )

      \ Construct and send a IPv6 packet with a specified source IPv6 address
      method construct-and-send-ipv6-packet-with-src-addr
      ( ? D: mac-addr src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 )
      ( protocol bytes xt self -- ? sent? )
      ( xt: ? buf -- ? send? )

      \ Construct an IPv6 packet
      method construct-and-send-ipv6-packet
      ( ? D: mac-addr dest-0 dest-1 dest-2 dest-3 protocol bytes xt self )
      ( -- ? sent? )
      ( xt: ? buf -- ? send? )

      \ Process IPv6 DNS response packet answers
      method process-ipv6-dns-answers
      ( addr bytes all-addr all-bytes ancount ident self -- )

      \ Send a DNS request packet
      method send-ipv6-dns-request ( c-addr bytes self -- )

      \ Send a UDP packet with a specified source IPv6 address and destination
      \ MAC address
      method send-ipv6-udp-packet-raw
      ( ? D: mac-addr src-0 src-1 src-2 src-3 src-addr src-port )
      ( dest-0 dest-1 dest-2 dest-3 dest-port bytes xt self -- ? success? )
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
      method refresh-dhcpv6-rebinding ( self -- )
      
      \ Refresh DHCP renewal
      method refresh-dhcpv6-renewing ( self -- )

      \ Refresh DHCP discovered
      method refresh-dhcpv6-discovered ( self -- )
      
      \ Refresh DHCP got NAK
      method refresh-dhcp-got-nak ( self -- )

      \ Refresh DHCP wait offer
      method refresh-dhcpv6-wait-advertise ( self -- )

      \ Refresh DHCP wait ACK
      method refresh-dhcpv6-wait-reply ( self -- )

      \ Refresh DHCP declined
      method refresh-dhcpv6-declined ( self -- )
      
      \ Refresh DHCP
      method refresh-dhcp ( self -- )

      \ Refresh router discovery
      method refresh-router-discovery ( self -- )
      
      \ Refresh an interface
      method refresh-interface ( self -- )
      
      \ Send a DHCPv6 SOLICIT packet
      method send-dhcpv6-solicit ( self -- )

      \ Send a DHCPv6 REQUEST packet
      method send-dhcpv6-request ( self -- )

      \ Send a renewal DHCPREQUEST packet
      method send-dhcpv6-renew ( self -- )

      \ Send a rebinding DHCPREQUEST packet
      method send-dhcpv6-rebind ( self -- )

      \ Send a DHCPv6 INFORMATION-REQUEST packet
      method send-dhcpv6-information-request ( self -- )

      \ Process a DHCPv6 packet
      method process-dhcpv6-packet ( addr bytes self -- )

      \ Process a DHCPv6 ADVERTISE
      method process-dhcpv6-advertise ( addr bytes self -- )

      \ Process a DHCPv6 REPLY packet
      method process-dhcpv6-reply ( addr bytes self -- )

      \ Process a DHCPv6 DECLINE packet
      method process-dhcpv6-decline ( addr bytes self -- )

      \ Enqueue a ready receiving IP endpoint
      method put-ready-endpoint ( endpoint self -- )

      \ Allocate an endpoint
      method allocate-endpoint ( self -- endpoint success? )

      \ Claim the endpoint queue lock, and if an exception occurs, restore the
      \ endpoint queue status
      method with-endpoint-queue ( xt start-ticks self -- )

      \ Attempt to set interface IPv6 address, detecting duplicates
      method detect-duplicate-and-set-intf-ipv6-addr
      ( target-0 target-1 target-2 target-3 self -- success? )
      
    end-module
    
    \ Get the IPv6 address
    method intf-ipv6-addr@ ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )
    
    \ Set the IPv6 address
    method intf-ipv6-addr! ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )

    \ Get the IPv6 prefix
    method intf-ipv6-prefix@ ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )
    
    \ Set the IPv6 prefix
    method intf-ipv6-prefix! ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )

    \ Get IPv6 autonomous state
    method intf-autonomous@ ( self -- autonomous? )

    \ Set IPv6 autonomous state
    method intf-autonomous! ( autonomous? self -- )

    \ Get the IPv6 prefix length
    method intf-ipv6-prefix-len@ ( self -- length )

    \ Set the IPv6 prefix length
    method intf-ipv6-prefix-len! ( length self -- )

    \ Get the gateway IPv6 address
    method gateway-ipv6-addr@ ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )

    \ Set the gateway IPv6 address
    method gateway-ipv6-addr! ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )

    \ Get the DNS server IPv6 address
    method dns-server-ipv6-addr@ ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )

    \ Set the DNS server IPv6 address
    method dns-server-ipv6-addr! ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )
    
    \ Get the TTL
    method intf-hop-limit@ ( self -- ttl )
    
    \ Set the TTL
    method intf-hop-limit! ( ttl self -- )

    \ Autoconfigure link-local IPv6 address
    method autoconfigure-link-local-ipv6-addr ( self -- success? )
    
    \ Start router discover
    method discover-ipv6-router ( self -- )
    
    \ Start IPv6 discovery
    method discover-ipv6-addr ( self -- success? )

    \ Start DNS discovery
    method discover-dns-ipv6-addr ( self -- )
    
    \ Send a UDP packet
    method send-ipv6-udp-packet
    ( ? src-port dest-0 dest-1 dest-2 dest-3 dest-port bytes xt self )
    ( -- ? success? )
    ( xt: ? buf -- ? sent? )
    
    \ Resolve an IPv6 address's MAC address
    method resolve-ipv6-addr-mac-addr
    ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- D: mac-addr success? )

    \ Resolve a DNS name's IPv6 address
    method resolve-dns-ipv6-addr
    ( c-addr bytes self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 success? )

    \ Get a TCP endpoint and connect to a host with it
    method allocate-tcp-connect-ipv6-endpoint
    ( src-port ipv6-0 ipv6-1 ipv6-2 ipv6-3 dest-port self -- endpoint success? )

  end-class

  \ Implement the IPv6 interface class
  <ipv6-interface> begin-implement

    \ Constructor
    :noname { frame-interface self -- }
      self <interface>->new
      frame-interface self out-frame-interface !
      0 0 0 0 self intf-ipv6-addr ipv6-unaligned!
      $fe80 $0000 $0000 $0000 $0000 $0000 $0000 $0000 make-ipv6-addr
      self intf-ipv6-prefix ipv6-unaligned!
      true intf-autonomous? !
      0 0 0 0 self discovered-ipv6-addr ipv6-unaligned!
      128 self intf-ipv6-prefix-len !
      $fe80 $0000 $0000 $0000 $0000 $0000 $0000 $0000 make-ipv6-addr
      self gateway-ipv6-addr ipv6-unaligned!
      $2001 $4860 $4860 $0000 $0000 $0000 $0000 $8888 make-ipv6-addr
      self dns-server-ipv6-addr ipv6-unaligned!
      64 self intf-hop-limit !
      false self router-discovered? !
      false self router-discovery? !
      false self use-dhcpv6? !
      false self use-dhcpv6-other? !
      false self detecting-duplicate? !
      0 self router-lifetime !
      0 self neighbor-reachable-time !
      0 self neighbor-retrans-time !
      1 0 self neighbor-discovery-sema init-sema
      1 0 self dns-resolve-sema init-sema
      0 self current-dhcp-transact-id !
      0 0 0 0 self dhcp-req-ipv6-addr ipv6-unaligned!
      dhcpv6-not-discovering self dhcp-discover-state !
      systick::systick-counter
      dup self router-discovered-start !
      dup self router-discovery-start !
      dup self dhcp-discover-start !
      dup self dhcp-discover-stage-start !
      dup self dhcp-renew-start !
      dup self dhcp-rebind-start !
      self dhcp-real-renew-start !
      default-dhcp-renew-interval self dhcp-real-renew-interval !
      default-dhcp-renew-interval self dhcp-renew-interval !
      default-dhcp-renew-interval self dhcp-rebind-interval !
      no-sema-limit 0 self router-discovery-sema init-sema
      no-sema-limit 0 self dhcp-sema init-sema
      no-sema-limit 0 self found-dns-sema init-sema
      self outgoing-buf-lock init-lock
      max-endpoints 0 ?do
        <ipv6-endpoint> self intf-endpoints <ipv6-endpoint> class-size i * +
        init-object
      loop
      self endpoint-queue-lock init-lock
      self router-discovery-lock init-lock
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
      <address-map> self address-map init-object
      <dns-cache> self dns-cache init-object
    ; define new

    \ Get the IPv6 address
    :noname ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )
      intf-ipv6-addr ipv6-unaligned@
    ; define intf-ipv6-addr@
    
    \ Set the IPv6 address
    :noname ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )
      intf-ipv6-addr ipv6-unaligned!
    ; define intf-ipv6-addr!

    \ Get the IPv6 prefix
    :noname ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )
      intf-ipv6-prefix ipv6-unaligned@
    ; define intf-ipv6-prefix@
    
    \ Set the IPv6 prefix
    :noname ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )
      intf-ipv6-prefix ipv6-unaligned!
    ; define intf-ipv6-prefix!

    \ Get IPv6 autonomous state
    :noname ( self -- autonomous? )
      intf-autonomous? @
    ; define intf-autonomous@

    \ Set IPv6 autonomous state
    :noname ( autonomous? self -- )
      intf-autonomous? !
    ; define intf-autonomous!
    
    \ Get the IPv6 prefix length
    :noname ( self -- length )
      intf-ipv6-prefix-len @
    ; define intf-ipv6-prefix-len@

    \ Set the IPv6 prefix length
    :noname ( length self -- )
      over 128 u<= averts x-out-of-range-prefix-len
      intf-ipv6-prefix-len !
    ; define intf-ipv6-prefix-len!

    \ Get the gateway IPv6 address
    :noname ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )
      gateway-ipv6-addr ipv6-unaligned@
    ; define gateway-ipv6-addr@
    
    \ Set the gateway IPv6 address
    :noname ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )
      gateway-ipv6-addr ipv6-unaligned!
    ; define gateway-ipv6-addr!

    \ Get the DNS server IPv6 address
    :noname ( self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 )
      dns-server-ipv6-addr ipv6-unaligned@
    ; define dns-server-ipv6-addr@
    
    \ Set the DNS server IPv6 address
    :noname ( ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- )
      dns-server-ipv6-addr ipv6-unaligned!
    ; define dns-server-ipv6-addr!

    \ Get the MAC address
    :noname ( self -- D: addr )
      out-frame-interface @ mac-addr@
    ; define intf-mac-addr@

    \ Get the TTL
    :noname ( self -- ttl )
      intf-hop-limit @
    ; define intf-hop-limit@
    
    \ Set the TTL
    :noname { ttl self -- }
      ttl 255 min 1 max self intf-hop-limit !
    ; define intf-hop-limit!

    \ Are we listening to IPv6 address
    :noname { dest-0 dest-1 dest-2 dest-3 self -- listen? }
      [ debug? ] [if]
        dest-0 dest-1 dest-2 dest-3
        [: cr ." ipv6-addr-listen " ipv6. ;] debug-hook execute
      [then]
      dest-0 dest-1 dest-2 dest-3 self intf-ipv6-addr@ ipv6= if
        true
      else
        dest-0 dest-1 dest-2 dest-3
        self intf-mac-addr@ make-link-local-ipv6-addr ipv6= if
          true
        else
          dest-0 dest-1 dest-2 dest-3 ipv6-addr-multicast? if
            dest-0 dest-1 dest-2 dest-3 ALL_NODES_LINK_LOCAL_MULTICAST ipv6= if
              true
            else
              dest-0 dest-1 dest-2 dest-3
              self intf-ipv6-addr@ solicit-node-link-local-multicast ipv6= if
                true
              else
                dest-0 dest-1 dest-2 dest-3
                self intf-mac-addr@ make-link-local-ipv6-addr
                solicit-node-link-local-multicast ipv6=
              then
            then
          else
            false
          then
        then
      then
      [ debug? ] [if]
        dup [: cr ."  match: " . ;] debug-hook execute
      [then]
    ; define ipv6-addr-listen?

    \ Process a MAC address for an IPv6 address
    :noname { D: mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 self -- }
      mac-addr mac-addr-multicast? if exit then
      ipv6-0 ipv6-1 ipv6-2 ipv6-3 ipv6-addr-multicast? if exit then
      mac-addr ipv6-0 ipv6-1 ipv6-2 ipv6-3 systick::systick-counter
      self address-map save-mac-addr-by-ipv6
      self neighbor-discovery-sema broadcast
      self neighbor-discovery-sema give
      [ debug? ] [if]
        [: cr ." Processed MAC address for IPv6 address" ;] debug-hook execute
      [then]
    ; define process-ipv6-mac-addr

    \ Process an IPv6 packet
    :noname
      ( D: src-mac-addr src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 )
      ( protocol addr bytes self -- )
      3 pick case
        PROTOCOL_UDP of process-ipv6-udp-packet endof
        PROTOCOL_TCP of process-ipv6-tcp-packet endof
        PROTOCOL_ICMPV6 of process-icmpv6-packet endof
        >r 2drop 2drop 2drop 2drop 2drop 2drop 2drop r>
      endcase
    ; define process-ipv6-packet

    \ Process an IPv6 TCP packet
    :noname
      { protocol addr bytes self -- }
      { D: src-mac-addr src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 }
      bytes tcp-header-size >= if
        addr full-tcp-header-size bytes > if exit then

        [ debug? ] [if]
          addr [: cr ." @@@@@ RECEIVING TCP:" tcp. ;] debug-hook execute
        [then]
        
        src-0 src-1 src-2 src-3 addr bytes self
        addr tcp-flags c@ TCP_CONTROL and
        case
          TCP_SYN of process-ipv6-syn-packet endof
          [ TCP_SYN TCP_ACK or ] literal of process-ipv6-syn-ack-packet endof
          TCP_ACK of process-ipv6-ack-packet endof
          [ TCP_FIN TCP_ACK or ] literal of process-ipv6-fin-ack-packet endof
          TCP_RST of process-ipv6-rst-packet endof
          nip nip nip nip nip nip nip
        endcase
      then
    ; define process-ipv6-tcp-packet

    \ Find a listening IPv6 TCP endpoint
    :noname { src-0 src-1 src-2 src-3 addr bytes self -- endpoint found? }
      max-endpoints 0 ?do
        self intf-endpoints <ipv6-endpoint> class-size i * + { endpoint }
        src-0 src-1 src-2 src-3
        addr tcp-src-port h@ rev16
        addr tcp-dest-port h@ rev16
        src-0 src-1 src-2 src-3 self address-map lookup-mac-addr-by-ipv6 if
          endpoint try-ipv6-accept-endpoint if
            endpoint self put-ready-endpoint
            endpoint true unloop exit
          then
        else
          2drop 2drop 2drop 2drop \ Should never happen
        then
      loop
      0 false
    ; define find-listen-ipv6-endpoint

    \ Find a connecting/connected IPv6 TCP endpoint
    :noname { src-0 src-1 src-2 src-3 addr bytes self -- endpoint found? }
      max-endpoints 0 ?do
        self intf-endpoints <ipv6-endpoint> class-size i * + { endpoint }
        src-0 src-1 src-2 src-3
        addr tcp-src-port h@ rev16
        addr tcp-dest-port h@ rev16
        endpoint match-ipv6-connect? if endpoint true unloop exit then
      loop
      0 false
    ; define find-connect-ipv6-endpoint
    
    \ Process an IPv6 TCP SYN packet
    :noname { src-0 src-1 src-2 src-3 addr bytes self -- }
      src-0 src-1 src-2 src-3 addr bytes self find-listen-ipv6-endpoint if
        { endpoint }
        src-0 src-1 src-2 src-3 addr bytes endpoint self
        [: swap send-ipv6-syn-ack ;] endpoint with-endpoint
      else
        drop src-0 src-1 src-2 src-3 addr bytes self send-ipv6-rst-for-packet
      then
    ; define process-ipv6-syn-packet

    \ Send a TCP SYN packet
    :noname { endpoint self -- }
      TCP_SYN_SENT endpoint endpoint-tcp-state!
      endpoint endpoint-ipv6-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@ 1-
      0
      max-endpoint-in-size
      TCP_SYN
      endpoint endpoint-remote-mac-addr@
      self send-ipv6-tcp-with-mss
    ; define send-syn

    \ Send an IPv6 TCP SYN+ACK packet
    :noname ( src-0 src-1 src-2 src-3 ) { addr bytes endpoint self -- }
      2drop 2drop
      rng::random endpoint endpoint-init-local-seq!
      addr tcp-seq-no @ rev 1+
      addr bytes tcp-mss@ not if
        drop [ max-ipv6-packet-size ipv6-header-size - tcp-header-size - ]
        literal
      then
      addr tcp-window-size h@ rev16
      endpoint init-tcp-stream
      endpoint endpoint-ipv6-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@ 1-
      endpoint endpoint-ack@
      endpoint endpoint-local-window@
      [ TCP_SYN TCP_ACK or ] literal
      endpoint endpoint-remote-mac-addr@

      self send-ipv6-tcp-with-mss
      TCP_SYN_RECEIVED endpoint endpoint-tcp-state!
      endpoint endpoint-ack-sent
      endpoint reset-endpoint-refresh
    ; define send-ipv6-syn-ack

    \ Send an IPv6 TCP RST packet in response to a packet
    :noname { src-0 src-1 src-2 src-3 addr bytes self -- }
      src-0 src-1 src-2 src-3
      addr tcp-src-port h@ rev16
      addr tcp-dest-port h@ rev16
      rng::random
      addr tcp-seq-no @ rev
      0
      TCP_RST
      src-0 src-1 src-2 src-3 self address-map lookup-mac-addr-by-ipv6 if
        self send-ipv6-basic-tcp
      else
        2drop 2drop 2drop 2drop 2drop 2drop \ Should never happen
      then
    ; define send-ipv6-rst-for-packet

    \ Send a generic IPv6 TCP RST packet
    :noname { endpoint self -- }
      endpoint endpoint-ipv6-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@
      endpoint endpoint-ack@
      0
      TCP_RST
      endpoint endpoint-remote-mac-addr@
      self send-ipv6-basic-tcp
      endpoint reset-endpoint-local-port
      TCP_CLOSED endpoint endpoint-tcp-state!
      endpoint self put-ready-endpoint
    ; define send-ipv6-rst

    \ Send a basic IPv6 TCP packet
    :noname
      ( remote-0 remote-1 remote-2 remote-3 remote-port local-port seq ack )
      ( window flags D: mac-addr self -- )
      -rot 12 pick 12 pick 12 pick 12 pick PROTOCOL_TCP tcp-header-size
      ( self D: mac-addr remote-addr protocol bytes )
      [: { remote-0 remote-1 remote-2 remote-3
        remote-port local-port seq ack window flags self buf }
        local-port rev16 buf tcp-src-port h!
        remote-port rev16 buf tcp-dest-port h!
        seq rev buf tcp-seq-no !
        ack rev buf tcp-ack-no !
        [ 5 4 lshift ] literal buf tcp-data-offset c!
        flags buf tcp-flags c!
        window rev16 buf tcp-window-size h!
        0 buf tcp-urgent-ptr h!
        self intf-ipv6-addr@
        remote-0 remote-1 remote-2 remote-3 PROTOCOL_TCP buf tcp-header-size
        0 tcp-checksum
        compute-ipv6-checksum rev16 buf tcp-checksum h!

        [ debug? ] [if]
          buf [: cr ." @@@@@ SENDING TCP:" tcp. ;] debug-hook execute
        [then]
        
        true
      ;] 9 pick construct-and-send-ipv6-packet drop
    ; define send-ipv6-basic-tcp

    \ Send an IPv6 TCP packet with an MSS field
    :noname
      ( remote-0 remote-1 remote-2 remote-3 remote-port local-port seq ack )
      ( window flags D: mac-addr self -- )
      -rot 12 pick 12 pick 12 pick 12 pick PROTOCOL_TCP tcp-header-size 8 +
      ( self D: mac-addr remote-addr protocol bytes )
      [: { remote-0 remote-1 remote-2 remote-3
        remote-port local-port seq ack window flags self buf }
        local-port rev16 buf tcp-src-port h!
        remote-port rev16 buf tcp-dest-port h!
        seq rev buf tcp-seq-no !
        ack rev buf tcp-ack-no !
        [ 7 4 lshift ] literal buf tcp-data-offset c!
        flags buf tcp-flags c!
        window rev16 buf tcp-window-size h!
        0 buf tcp-urgent-ptr h!
        [ TCP_OPT_MSS 24 lshift 4 16 lshift or
        max-ipv6-packet-size ipv6-header-size - tcp-header-size - or
        rev ] literal
        buf tcp-header-size + !
        [ $01010100 rev ] literal buf tcp-header-size + 4 + !
        self intf-ipv6-addr@
        remote-0 remote-1 remote-2 remote-3 PROTOCOL_TCP
        buf tcp-header-size 8 + 0 tcp-checksum
        compute-ipv6-checksum rev16 buf tcp-checksum h!

        [ debug? ] [if]
          buf [: cr ." @@@@@ SENDING TCP:" tcp. ;] debug-hook execute
        [then]
        
        true
      ;] 6 pick construct-and-send-ipv6-packet drop
    ; define send-ipv6-tcp-with-mss
    
    \ Process an IPv6 TCP SYN+ACK packet
    :noname { src-0 src-1 src-2 src-3 addr bytes self -- }
      src-0 src-1 src-2 src-3 addr bytes find-connect-ipv6-endpoint not if
        drop src-0 src-1 src-2 src-3 addr bytes send-ipv6-rst-for-packet exit
      then
      { endpoint }
      addr bytes self endpoint
      [: { addr bytes self endpoint }
        addr tcp-dest-port h@ rev16 endpoint endpoint-local-port@ <> if
          exit
        then
        endpoint endpoint-tcp-state@ TCP_SYN_SENT = if
          addr tcp-ack-no @ rev
          endpoint endpoint-init-local-seq@ <> if exit then
          addr tcp-seq-no @ rev 1+
          addr bytes tcp-mss@ not if
            drop [ max-ipv6-packet-size ipv6-header-size -
            tcp-header-size - ] literal
          then
          addr tcp-window-size h@ rev16
          endpoint init-tcp-stream
        then
        endpoint endpoint-ipv6-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        endpoint endpoint-ack@
        endpoint endpoint-local-window@
        TCP_ACK
        endpoint endpoint-remote-mac-addr@
        self send-ipv6-basic-tcp
        TCP_ESTABLISHED endpoint endpoint-tcp-state!
        endpoint reset-endpoint-refresh
        endpoint self put-ready-endpoint
      ;] endpoint with-endpoint
    ; define process-ipv6-syn-ack-packet

    \ Process an IPv6 ACK packet
    :noname ( src-0 src-1 src-2 src-3 addr bytes self -- )
      { addr bytes self }
      addr bytes self find-connect-ipv6-endpoint not if
        drop exit
      then
      { endpoint }
      addr bytes self endpoint
      [: { addr bytes self endpoint }
        addr tcp-dest-port h@ rev16 endpoint endpoint-local-port@ <> if
          exit
        then
        endpoint endpoint-tcp-state@ { state }
        addr bytes endpoint self
        state case
          TCP_SYN_SENT of
            process-ipv6-ack-syn-sent
          endof
          TCP_SYN_RECEIVED of
            process-ipv6-ack-syn-received
          endof
          TCP_ESTABLISHED of
            process-ipv6-ack-established
          endof
          TCP_FIN_WAIT_1 of
            process-ipv6-ack-fin-wait-1
          endof
          TCP_FIN_WAIT_2 of
            process-ipv6-ack-fin-wait-2
          endof
          TCP_CLOSE_WAIT of
            process-ipv6-ack-close-wait
          endof
          TCP_LAST_ACK of
            process-ipv6-ack-last-ack
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
      ;] endpoint with-endpoint
    ; define process-ipv6-ack-packet

    \ Process an IPv6 FIN+ACK packet
    :noname { src-0 src-1 src-2 src-3 addr bytes self -- }
      src-0 src-1 src-2 src-3 addr bytes self find-connect-ipv6-endpoint not if
        drop src-0 src-1 src-2 src-3 addr bytes self send-ipv6-rst-for-packet
        exit
      then
      { endpoint }
      addr bytes self endpoint
      [: { addr bytes self endpoint }
        addr tcp-dest-port h@ rev16 endpoint endpoint-local-port@ <> if
          exit
        then
        endpoint endpoint-tcp-state@ { state }
        addr bytes endpoint self process-ipv6-basic-ack
        addr bytes endpoint self
        state case
          TCP_ESTABLISHED of process-ipv6-ack-fin-established endof
          TCP_FIN_WAIT_1 of process-ipv6-ack-fin-fin-wait-1 endof
          TCP_FIN_WAIT_2 of process-ipv6-ack-fin-fin-wait-2 endof
          >r 2drop 2drop r>
        endcase
        endpoint endpoint-waiting-bytes@ 0>
        endpoint endpoint-tcp-state@ state <> or if
          endpoint self put-ready-endpoint
        else
          endpoint wake-endpoint
        then
      ;] endpoint with-endpoint
    ; define process-ipv6-fin-ack-packet

    \ Process an IPv6 ACK packet in the general case
    :noname { addr bytes endpoint self -- }
      addr full-tcp-header-size { header-size }
      addr header-size + bytes header-size -
      addr tcp-flags c@ TCP_PSH and 0<>
      addr tcp-seq-no @ rev
      endpoint add-endpoint-tcp-data
      addr tcp-window-size h@ rev16
      addr tcp-ack-no @ rev
      endpoint endpoint-ack-in
      endpoint endpoint-send-ack? if
        endpoint endpoint-ipv6-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        endpoint endpoint-ack@
        endpoint endpoint-local-window@
        TCP_ACK
        endpoint endpoint-remote-mac-addr@
        self send-ipv6-basic-tcp
        endpoint endpoint-ack-sent
        endpoint reset-endpoint-refresh
      then
    ; define process-ipv6-basic-ack

    \ Process an IPv6 ACK packet in TCP_SYN_SENT state
    :noname ( addr bytes endpoint self -- )
      [: { addr bytes endpoint self }
        addr tcp-seq-no @ rev 1+
        addr bytes tcp-mss@ not if
          drop [ max-ipv6-packet-size ipv6-header-size -
          tcp-header-size - ] literal
        then
        addr tcp-window-size h@ rev16
        endpoint init-tcp-stream
        endpoint endpoint-ipv6-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        endpoint endpoint-ack@
        endpoint endpoint-local-window@
        TCP_ACK
        endpoint endpoint-remote-mac-addr@
        self send-ipv6-basic-tcp
        TCP_ESTABLISHED endpoint endpoint-tcp-state!
        addr bytes endpoint self process-ipv6-basic-ack
        endpoint self put-ready-endpoint
      ;] 2 pick with-endpoint
    ; define process-ipv6-ack-syn-sent
    
    \ Process an IPv6 ACK packet in TCP_SYN_RECEIVED state
    :noname { addr bytes endpoint self -- }
      TCP_ESTABLISHED endpoint endpoint-tcp-state!
      addr bytes endpoint self process-ipv6-basic-ack
      endpoint self put-ready-endpoint
    ; define process-ipv6-ack-syn-received
    
    \ Process an IPv6 ACK packet in TCP_ESTABLISHED state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self process-ipv6-basic-ack
    ; define process-ipv6-ack-established
    
    \ Process an IPv6 ACK packet in TCP_FIN_WAIT_1 state
    :noname { addr bytes endpoint self -- }
      addr tcp-ack-no @ rev
      endpoint endpoint-local-seq@ 1+ = if
        TCP_FIN_WAIT_2 endpoint endpoint-tcp-state!
      else
        addr bytes endpoint self process-ipv6-basic-ack
      then
    ; define process-ipv6-ack-fin-wait-1
    
    \ Process an IPv6 ACK packet in TCP_FIN_WAIT_2 state
    :noname { addr bytes endpoint self -- }
      \ addr bytes endpoint self process-ipv6-basic-ack
    ; define process-ipv6-ack-fin-wait-2
    
    \ Process an IPv6 ACK packet in TCP_CLOSE_WAIT state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self process-ipv6-basic-ack
    ; define process-ipv6-ack-close-wait
    
    \ Process an IPv6 ACK packet in TCP_LAST_ACK state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self process-ipv6-basic-ack
      addr tcp-ack-no @ rev
      endpoint endpoint-local-seq@ = if
        TCP_CLOSED endpoint endpoint-tcp-state!
        endpoint self put-ready-endpoint
      then
    ; define process-ipv6-ack-last-ack

    \ Process an errant IPv6 ACK packet
    :noname { addr bytes endpoint self -- }
      endpoint endpoint-ipv6-remote@ drop addr bytes
      self send-ipv6-rst-for-packet
      TCP_CLOSED endpoint endpoint-tcp-state!
      endpoint self put-ready-endpoint
    ; define send-ipv6-rst-for-ack

    \ Process an IPv6 FIN packet for a TCP_ESTABLISHED state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self send-ipv6-fin-reply-ack
      TCP_CLOSE_WAIT endpoint endpoint-tcp-state!
      endpoint self put-ready-endpoint
    ; define process-ipv6-ack-fin-established

    \ Process an IPv6 FIN/ACK packet for a TCP_FIN_WAIT_1 state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self send-ipv6-fin-reply-ack
      TCP_CLOSED endpoint endpoint-tcp-state! \ Formerly TCP_TIME_WAIT
      addr tcp-seq-no @ rev endpoint self add-time-wait
      endpoint self put-ready-endpoint
    ; define process-ipv6-ack-fin-fin-wait-1

    \ Process an IPv6 FIN/ACK packet for a TCP_FIN_WAIT_2 state
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self send-ipv6-fin-reply-ack
      TCP_CLOSED endpoint endpoint-tcp-state! \ Formerly TCP_TIME_WAIT
      addr tcp-seq-no @ rev endpoint self add-time-wait
      endpoint self put-ready-endpoint
    ; define process-ipv6-ack-fin-fin-wait-2

    \ Process an unexpected IPv6 FIN packet
    :noname { addr bytes endpoint self -- }
      addr bytes endpoint self send-ipv6-fin-reply-ack
      TCP_CLOSED endpoint endpoint-tcp-state! \ Formerly TCP_CLOSING
      addr tcp-seq-no @ rev endpoint self add-time-wait
      endpoint self put-ready-endpoint
    ; define process-ipv6-unexpected-fin

    \ Send an ACK in response to a FIN packet
    :noname { addr bytes endpoint self -- }
      endpoint endpoint-ipv6-remote@
      endpoint endpoint-local-port@
      endpoint endpoint-local-seq@ 1+
      addr tcp-seq-no @ rev 1+
      endpoint endpoint-local-window@
      TCP_ACK
      \      [ TCP_FIN TCP_ACK or ] literal
      endpoint endpoint-remote-mac-addr@
      self send-ipv6-basic-tcp
    ; define send-ipv6-fin-reply-ack
    
    \ Process an IPv6 RST packet
    :noname { src-0 src-1 src-2 src-3 addr bytes self -- }
      src-0 src-1 src-2 src-3 addr bytes self find-connect-ipv6-endpoint if
        { endpoint }
        addr bytes self endpoint
        [: { addr bytes self endpoint }
          addr tcp-dest-port h@ rev16
          endpoint endpoint-local-port@ <> if
            exit
          then
          endpoint endpoint-tcp-state@ TCP_SYN_SENT <> if
            TCP_CLOSED endpoint endpoint-tcp-state!
          then
          endpoint reset-endpoint-local-port
          endpoint self put-ready-endpoint
        ;] endpoint with-endpoint
      else
        drop
      then
    ; define process-ipv6-rst-packet

    \ Process an IPv6 UDP packet
    :noname
      { protocol addr bytes self -- }
      { D: src-mac-addr src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 }
      bytes udp-header-size >= if
        addr udp-total-len h@ rev16 bytes <> if exit then
        src-0 src-1 src-2 src-3 self dns-server-ipv6-addr ipv6-unaligned@ ipv6=
        addr udp-src-port h@ rev16 dns-port = and if
          addr udp-header-size + bytes udp-header-size -
          self process-ipv6-dns-packet
          exit
        then
        addr udp-src-port h@ rev16 dhcp-server-port =
        addr udp-dest-port h@ rev16 dhcp-client-port = and if
          addr udp-header-size + bytes udp-header-size -
          src-0 src-1 src-2 src-3 self process-dhcpv6-packet
          exit
        then
        max-endpoints 0 ?do
          self intf-endpoints <ipv6-endpoint> class-size i * + { endpoint }
          src-0 src-1 src-2 src-3 addr bytes endpoint self [:
            { src-0 src-1 src-2 src-3 addr bytes endpoint self }
            endpoint udp-endpoint?
            endpoint endpoint-local-port@ addr udp-dest-port h@ rev16 = and if
              addr udp-header-size + bytes udp-header-size -
              src-0 src-1 src-2 src-3 addr udp-src-port h@ rev16
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
    ; define process-ipv6-udp-packet

    \ Process an IPv6 DNS response packet
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
      addr bytes all-addr all-bytes ancount ident self process-ipv6-dns-answers
    ; define process-ipv6-dns-packet

    \ Process IPv6 DNS response packet answers
    :noname { addr bytes all-addr all-bytes ancount ident self -- }
      begin ancount 0> bytes 0> and while
        addr bytes { saved-addr saved-bytes }
        addr bytes skip-dns-name to bytes to addr
        bytes dns-abody-size < if exit then

        addr dns-abody-type hunaligned@ [ DNS_QTYPE_AAAA rev16 ] literal =
        addr dns-abody-class hunaligned@ [ 1 rev16 ] literal = and
        addr dns-abody-rdlength hunaligned@ [ 16 rev16 ] literal = and if
          dns-abody-size +to addr
          [ dns-abody-size negate ] literal +to bytes
          bytes 16 < if exit then
          addr ipv6-unaligned@
          saved-addr saved-bytes all-addr all-bytes ident self 256 [:
            { ident self buf }
            buf parse-dns-name if
              ident buf rot self dns-cache save-ipv6-addr-by-dns
              self dns-resolve-sema broadcast
              self dns-resolve-sema give
            else
              2drop 2drop 2drop 2drop
            then
          ;] with-allot
          exit
        else
          addr dns-abody-rdlength hunaligned@ rev16 { rdlength }
          dns-abody-size rdlength + dup +to addr negate +to bytes
          -1 +to ancount
        then
      repeat
    ; define process-ipv6-dns-answers
    
    \ Process an ICMPv6 packet
    :noname
      { protocol addr bytes self -- }
      { D: src-mac-addr src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 }
      bytes icmp-header-size >= if
        src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 PROTOCOL_ICMPV6
        addr bytes 0 icmp-checksum compute-ipv6-checksum
        addr icmp-checksum hunaligned@ rev16 <> if exit then
        addr icmp-type c@ case
          ICMPV6_TYPE_ECHO_REQUEST of
            src-mac-addr src-0 src-1 src-2 src-3 addr bytes self
            process-icmpv6-echo-request-packet
          endof
          ICMPV6_TYPE_ROUTER_ADVERTISE of
            src-mac-addr src-0 src-1 src-2 src-3 addr bytes self
            process-icmpv6-router-advertise-packet
          endof
          ICMPV6_TYPE_NEIGHBOR_SOLICIT of
            src-mac-addr src-0 src-1 src-2 src-3 addr bytes self
            process-icmpv6-neighbor-solicit-packet
          endof
          ICMPV6_TYPE_NEIGHBOR_ADVERTISE of
            src-mac-addr src-0 src-1 src-2 src-3 addr bytes self
            process-icmpv6-neighbor-advertise-packet
          endof
        endcase
      then
    ; define process-icmpv6-packet

    \ Send an ICMPv6 neighbor solicit packet
    :noname { target-0 target-1 target-2 target-3 self -- }
      target-0 target-1 target-2 target-3 self
      target-0 target-1 target-2 target-3 solicit-node-link-local-multicast
      ipv6-multicast-mac-addr
      self detecting-duplicate? @ if 0 0 0 0 else self intf-ipv6-addr@ then
      target-0 target-1 target-2 target-3 solicit-node-link-local-multicast
      PROTOCOL_ICMPV6
      [ icmp-header-size ipv6-addr-size + 2 + mac-addr-size + ] literal [:
        { target-0 target-1 target-2 target-3 self buf }
        ICMPV6_TYPE_NEIGHBOR_SOLICIT buf icmp-type c!
        ICMP_CODE_UNUSED buf icmp-code c!
        target-0 target-1 target-2 target-3
        buf icmp-header-size + ipv6-unaligned!
        0 buf icmp-rest-of-header unaligned!
        [ icmp-header-size ipv6-addr-size + ] literal buf + { opt-buf }
        OPTION_SOURCE_LINK_LAYER_ADDR opt-buf c!
        1 opt-buf 1+ c!
        self intf-mac-addr@ opt-buf 2 + mac!
        self detecting-duplicate? @ if 0 0 0 0 else self intf-ipv6-addr@ then
        target-0 target-1 target-2 target-3 solicit-node-link-local-multicast
        PROTOCOL_ICMPV6
        buf [ icmp-header-size ipv6-addr-size + 2 + mac-addr-size + ] literal
        0 icmp-checksum compute-ipv6-checksum rev16
        buf icmp-checksum hunaligned!
        true
        [ debug? ] [if]
          [: cr ." Sending neighbor solicit" ;] debug-hook execute
        [then]
      ;] self construct-and-send-ipv6-packet-with-src-addr drop
    ; define send-icmpv6-neighbor-solicit

    \ Send an ICMPv6 router solicit package
    :noname { self -- }
      self intf-hop-limit @ { saved-hop-limit }
      255 self intf-hop-limit !
      self
      ALL_ROUTERS_LINK_LOCAL_MULTICAST ipv6-multicast-mac-addr
      ALL_ROUTERS_LINK_LOCAL_MULTICAST
      PROTOCOL_ICMPV6
      [ icmp-header-size 2 + mac-addr-size + ] literal
      [: { self buf }
        ICMPV6_TYPE_ROUTER_SOLICIT buf icmp-type c!
        ICMP_CODE_UNUSED buf icmp-code c!
        0 buf icmp-rest-of-header unaligned!
        icmp-header-size buf + { opt-buf }
        OPTION_SOURCE_LINK_LAYER_ADDR opt-buf c!
        1 opt-buf 1+ c!
        self intf-mac-addr@ opt-buf 2 + mac!
        self intf-ipv6-addr@ ALL_ROUTERS_LINK_LOCAL_MULTICAST PROTOCOL_ICMPV6
        buf [ icmp-header-size 2 + mac-addr-size + ] literal
        0 icmp-checksum compute-ipv6-checksum rev16
        buf icmp-checksum hunaligned!
        true
        [ debug? ] [if]
          [: cr ." Sending router solicit" ;] debug-hook execute
        [then]
      ;] self construct-and-send-ipv6-packet drop
      self intf-hop-limit @ 255 = if
        saved-hop-limit self intf-hop-limit !
      then
    ; define send-icmpv6-router-solicit
    
    \ Process an ICMPv6 echo request packet
    :noname { D: src-mac-addr src-0 src-1 src-2 src-3 addr bytes self -- }
      bytes icmp-header-size < if exit then
      addr bytes self src-mac-addr
      src-0 src-1 src-2 src-3 PROTOCOL_ICMPV6 bytes [:
        { addr bytes self buf }
        ICMPV6_TYPE_ECHO_REPLY buf icmp-type c!
        ICMP_CODE_UNUSED buf icmp-code c!
        addr 4 + buf 4 + bytes 4 - move
        buf bytes 0 icmp-checksum compute-inet-checksum rev16
        buf icmp-checksum hunaligned!
        true
      ;] self construct-and-send-ipv6-packet drop
    ; define process-icmpv6-echo-request-packet

    \ Process an ICMPv6 neighbor solicit packet
    :noname { D: src-mac-addr src-0 src-1 src-2 src-3 addr bytes self -- }
      [ debug? ] [if]
        [: cr ." Processing neighbor solicit" ;] debug-hook execute
      [then]
      bytes icmp-header-size ipv6-addr-size + < if exit then
      addr icmp-header-size + ipv6-unaligned@ self intf-ipv6-addr@ ipv6= not if
        exit
      then
      OPTION_SOURCE_LINK_LAYER_ADDR addr bytes find-icmpv6-opt if
        6 >= if mac@ to src-mac-addr else drop exit then
      then
      [ debug? ] [if]
        [: cr ." Good neighbor solicit" ;] debug-hook execute
      [then]
      src-0 src-1 src-2 src-3 ipv6-addr-multicast? not
      src-mac-addr mac-addr-multicast? not and if
        src-mac-addr src-0 src-1 src-2 src-3 systick::systick-counter
        self address-map save-mac-addr-by-ipv6
      then
      [ icmp-header-size ipv6-addr-size + ] literal { reply-bytes }
      src-mac-addr mac-addr-multicast? not if
        [ 2 mac-addr-size + ] literal +to reply-bytes
      then
      reply-bytes src-mac-addr src-0 src-1 src-2 src-3 self src-mac-addr
      src-0 src-1 src-2 src-3 PROTOCOL_ICMPV6 reply-bytes [:
        { D: bytes src-mac-addr src-0 src-1 src-2 src-3 self buf }
        ICMPV6_TYPE_NEIGHBOR_ADVERTISE buf icmp-type c!
        ICMP_CODE_UNUSED buf icmp-code c!
        self intf-ipv6-addr@ buf icmp-header-size + ipv6-unaligned!
        src-mac-addr mac-addr-multicast? not if
          [ NEIGHBOR_SOLICITED rev ] literal buf icmp-rest-of-header unaligned!
          [ icmp-header-size ipv6-addr-size + ] literal +to buf
          OPTION_TARGET_LINK_LAYER_ADDR buf c!
          1 buf 1+ c!
          src-mac-addr buf 2 + mac!
          [ debug? ] [if]
            [: cr ." Setting target link layer address" ;] debug-hook execute
          [then]
        else
          0 buf icmp-rest-of-header unaligned!
        then
        self intf-ipv6-addr@ src-0 src-1 src-2 src-3 PROTOCOL_ICMPV6
        buf bytes 0 icmp-checksum compute-ipv6-checksum rev16
        buf icmp-checksum hunaligned!
        true
        [ debug? ] [if]
          [: cr ." Sending neighbor advertise" ;] debug-hook execute
        [then]
      ;] self construct-and-send-ipv6-packet drop
    ; define process-icmpv6-neighbor-solicit-packet

    \ Process an ICMPv6 neighbor advertise packet
    :noname { D: src-mac-addr src-0 src-1 src-2 src-3 addr bytes self -- }
      [ debug? ] [if]
        [: cr ." Processing neighbor advertise" ;] debug-hook execute
      [then]
      bytes icmp-header-size ipv6-addr-size + < if exit then
      [ debug? ] [if]
        [: cr ." Good neighbor advertise" ;] debug-hook execute
      [then]
      addr icmp-header-size + ipv6-unaligned@
      { target-0 target-1 target-2 target-3 }
      src-mac-addr target-0 target-1 target-2 target-3
      self process-ipv6-mac-addr
    ; define process-icmpv6-neighbor-advertise-packet

    \ Process an ICMPv6 router advertise packet
    :noname ( D: src-mac-addr src-0 src-1 src-2 src-3 addr bytes self -- )
      [:
        { D: src-mac-addr src-0 src-1 src-2 src-3 addr bytes self }
        [ debug? ] [if]
          [: cr ." Processing router advertise" ;] debug-hook execute
        [then]
        bytes icmpv6-ra-header-size < if exit then
        src-mac-addr src-0 src-1 src-2 src-3
        self process-ipv6-mac-addr
        self router-discovery? @ if
          addr icmpv6-ra-router-lifetime hunaligned@ rev16 dup 0= if exit then
          [ debug? ] [if]
            [: cr ." Good router advertise" ;] debug-hook execute
          [then]
          self router-lifetime !
          src-0 src-1 src-2 src-3 self gateway-ipv6-addr ipv6-unaligned!
          addr icmpv6-ra-cur-hop-limit c@ self intf-hop-limit !
          addr icmpv6-ra-m-o-reserved c@
          dup icmpv6-ra-managed and 0<> self use-dhcpv6? !
          dup icmpv6-ra-managed and 0= swap icmpv6-ra-other and 0<> and
          self use-dhcpv6-other? !
          addr icmpv6-ra-reachable-time unaligned@
          rev self neighbor-reachable-time !
          addr icmpv6-ra-retrans-time unaligned@
          rev self neighbor-retrans-time !
          addr icmpv6-ra-header-size + { cur-addr }
          addr icmpv6-ra-header-size - { cur-size }
          begin
            cur-size 0> if
              OPTION_PREFIX_INFO cur-addr cur-size find-icmpv6-opt if
                { opt-addr opt-size }
                opt-addr icmpv6-prefix-info-prefix-len c@ 128 u<= if
                  opt-addr icmpv6-prefix-info-prefix-len c@
                  self intf-ipv6-prefix-len!
                  opt-addr icmpv6-prefix-info-prefix ipv6-unaligned@
                  self intf-ipv6-prefix!
                  opt-addr icmpv6-prefix-info-flags c@
                  icmpv6-prefix-info-autonomous and 0<>
                  dup self intf-autonomous!
                  [ debug? ] [if]
                    [: cr ." Got prefix info" ;] debug-hook execute
                  [then]
                else
                  false
                then
                not if
                  opt-addr icmpv6-prefix-info-len c@ 8 * opt-addr +
                  dup cur-addr - negate +to cur-size
                  to cur-addr
                  false
                else
                  true
                then
              else
                2drop true
              then
            else
              true
            then
          until
          false self router-discovery? !
          true self router-discovered? !
          systick::systick-counter self router-discovered-start !
          self router-discovery-sema give
        else
          [ debug? ] [if]
            [: cr ." Not handling router advertise" ;] debug-hook execute
          [then]
        then
      ;] over router-discovery-lock with-lock
    ; define process-icmpv6-router-advertise-packet

    \ Construct and send a frame
    :noname ( ? bytes xt self -- ? sent? ) ( xt: ? buf -- ? send? )
      [:
        dup { self }
        2 pick ethernet-frame-size u<= averts x-oversized-frame
        [: { bytes xt self }
          self outgoing-buf xt execute if
            [ debug? ] [if]
              self outgoing-buf dup bytes +
              [: cr ." SENT: " dump ;] debug-hook execute
            [then]
            self outgoing-buf bytes self out-frame-interface @ put-tx-frame
            true
          else
            false
          then
        ;] self outgoing-buf-lock with-lock
      ;] task::no-timeout task::with-timeout
    ; define construct-and-send-frame

    \ Construct and send a IPv6 packet with a specified source IPv6 address
    :noname
      ( ? D: mac-addr src-0 src-1 src-2 src-3 )
      ( dest-0 dest-1 dest-2 dest-3 protocol bytes xt self -- )
      ( ? sent? ) ( xt: ? buf -- ? send? )
      2 pick [ ethernet-header-size ipv6-header-size + ] literal + over
      [: { D: mac-addr src-0 src-1 src-2 src-3
        dest-0 dest-1 dest-2 dest-3 protocol bytes xt self buf }
        [ debug? ] [if]
          self intf-mac-addr@
          [: cr ." Source MAC:       " mac. ;] debug-hook execute
          mac-addr
          [: cr ." Destination MAC:  " mac. ;] debug-hook execute
          src-0 src-1 src-2 src-3
          [: cr ." Source IPv6:      " ipv6. ;] debug-hook execute
          dest-0 dest-1 dest-2 dest-3
          [: cr ." Destination IPv6: " ipv6. ;] debug-hook execute
        [then]
        mac-addr buf ethh-destination-mac mac!
        self intf-mac-addr@ buf ethh-source-mac mac!
        [ ETHER_TYPE_IPV6 rev16 ] literal buf ethh-ether-type h!
        buf ethernet-header-size + { ip-buf }
        [ IPV6_VERSION_TRAFFIC_FLOW_CONST rev ] literal
        ip-buf ipv6-version-traffic-flow unaligned!
        bytes rev16 ip-buf ipv6-payload-len hunaligned!
        protocol ip-buf ipv6-next-header c!
        self intf-hop-limit c@ ip-buf ipv6-hop-limit c!
        src-0 src-1 src-2 src-3 ip-buf ipv6-src-addr ipv6-unaligned!
        dest-0 dest-1 dest-2 dest-3 ip-buf ipv6-dest-addr ipv6-unaligned!
        ip-buf ipv6-header-size + xt execute
      ;] swap construct-and-send-frame
    ; define construct-and-send-ipv6-packet-with-src-addr

    \ Construct and send a IPv6 packet
    :noname
      { D: mac-addr dest-0 dest-1 dest-2 dest-3 protocol bytes xt self -- }
      ( ? sent? xt: ? buf -- ? send? )
      mac-addr self intf-ipv6-addr ipv6-unaligned@
      dest-0 dest-1 dest-2 dest-3 protocol bytes xt self
      construct-and-send-ipv6-packet-with-src-addr
    ; define construct-and-send-ipv6-packet

    \ Resolve an IPv6 address's MAC address
    :noname { dest-0 dest-1 dest-2 dest-3 self -- D: mac-addr success? }
      self neighbor-reachable-time @ 0<> if
        self neighbor-reachable-time @ self address-map age-out-mac-addrs
      then
      systick::systick-counter self neighbor-retrans-time @ 10 * - { tick }
      max-neighbor-discovery-attempts { attempts }
      begin
        dest-0 dest-1 dest-2 dest-3
        self address-map lookup-mac-addr-by-ipv6 not
      while
        2drop
        systick::systick-counter tick -
        self neighbor-retrans-time @ 10 * >= if
          attempts 0> if
            -1 to attempts
            dest-0 dest-1 dest-2 dest-3 self send-icmpv6-neighbor-solicit
            systick::systick-counter to tick
          else
            0. false exit
          then
        else
          task::timeout @ { old-timeout }
          tick self neighbor-retrans-time @ 10 * + systick::systick-counter -
          self swap [: task::timeout ! neighbor-discovery-sema take ;] try
          dup ['] task::x-timed-out = if 2drop drop 0 then
          ?raise
          old-timeout task::timeout !
        then
      repeat
      true
    ; define resolve-ipv6-addr-mac-addr

    \ Resolve a DNS name's IPv6 address
    :noname { c-addr bytes self -- ipv6-0 ipv6-1 ipv6-2 ipv6-3 success? }
      c-addr bytes validate-dns-name
      systick::systick-counter dns-resolve-interval - { tick }
      max-dns-resolve-attempts { attempts }
      c-addr bytes self dns-cache lookup-ipv6-addr-by-dns if
        0= if true exit then
      else
        2drop 2drop drop
      then
      begin
        systick::systick-counter tick - dns-resolve-interval >= if
          attempts 0> if
            -1 +to attempts
            c-addr bytes self send-ipv6-dns-request
            systick::systick-counter to tick
          else
            0 0 0 0 false exit
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
        c-addr bytes self dns-cache lookup-ipv6-addr-by-dns if
          0= true
        else
          2drop 2drop drop false
        then
      until
    ; define resolve-dns-ipv6-addr

    \ Evict a DNS name's cache entry, forcing it to be re-resolved
    :noname { c-addr bytes self -- }
      c-addr bytes validate-dns-name
      c-addr bytes self dns-cache net-ipv6-internal::evict-dns
    ; define evict-dns

    \ Send a DNS request packet
    :noname { c-addr bytes self -- }
      c-addr bytes self dns-src-port
      self dns-server-ipv6-addr ipv6-unaligned@ dns-port
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
        [ DNS_QTYPE_AAAA rev16 ] literal buf dns-qbody-qtype hunaligned!
        [ 1 rev16 ] literal buf dns-qbody-qclass hunaligned!
        true
      ;] self send-ipv6-udp-packet drop
    ; define send-ipv6-dns-request

    \ Send a UDP packet with a specified source IPv6 address
    :noname
      { D: mac-addr src-0 src-1 src-2 src-3 src-port
      dest-0 dest-1 dest-2 dest-3 dest-port bytes xt self -- }
      ( success? ) ( xt: ? buf -- ? sent? )
      src-0 src-1 src-2 src-3
      dest-0 dest-1 dest-2 dest-3
      src-port dest-port bytes xt self
      mac-addr src-0 src-1 src-2 src-3
      dest-0 dest-1 dest-2 dest-3 PROTOCOL_UDP bytes udp-header-size + [:
        { src-port dest-port bytes xt self buf }
        { src-0 src-1 src-2 src-3 dest-0 dest-1 dest-2 dest-3 }
        src-port rev16 buf udp-src-port h!
        dest-port rev16 buf udp-dest-port h!
        bytes udp-header-size + rev16 buf udp-total-len h!
        buf udp-header-size + xt execute
        src-0 src-1 src-2 src-3
        dest-0 dest-1 dest-2 dest-3
        PROTOCOL_UDP buf udp-header-size bytes + 0 udp-checksum
        compute-ipv6-checksum rev16 buf udp-checksum h!
      ;] self construct-and-send-ipv6-packet-with-src-addr
    ; define send-ipv6-udp-packet-raw

    \ Send a UDP packet
    :noname
      { src-port dest-0 dest-1 dest-2 dest-3 dest-port bytes xt self }
      ( -- success? )
      dest-0 dest-1 dest-2 dest-3
      dest-0 dest-1 dest-2 dest-3 self resolve-ipv6-addr-mac-addr if
        src-port -rot dest-port -rot bytes -rot xt -rot self -rot
        dest-0 dest-1 dest-2 dest-3 PROTOCOL_UDP bytes udp-header-size + [:
          { dest-0 dest-1 dest-2 dest-3 src-port dest-port bytes xt self buf }
          src-port rev16 buf udp-src-port h!
          dest-port rev16 buf udp-dest-port h!
          bytes udp-header-size + rev16 buf udp-total-len h!
          buf udp-header-size + xt execute
          self intf-ipv6-addr@
          dest-0 dest-1 dest-2 dest-3
          PROTOCOL_UDP buf udp-header-size bytes + 0 udp-checksum
          compute-ipv6-checksum rev16 buf udp-checksum h!
        ;] self construct-and-send-ipv6-packet
      else
        2drop false
      then
    ; define send-ipv6-udp-packet

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
          self intf-endpoints <ipv6-endpoint> class-size index * + { endpoint }
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
        self intf-endpoints i <ipv6-endpoint> class-size * + { endpoint }
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
    :noname { src-port a0 a1 a2 a3 dest-port self -- endpoint success? }
      a0 a1 a2 a3 self resolve-ipv6-addr-mac-addr not if
        2drop 0 false exit
      then { D: mac-addr }
      max-endpoints 0 ?do
        self intf-endpoints <ipv6-endpoint> class-size i * + { endpoint }
        src-port a0 a1 a2 a3 dest-port mac-addr endpoint
        try-ipv6-connect-endpoint if
          endpoint self [: { endpoint self }
            rng::random endpoint endpoint-init-local-seq!
            endpoint reset-endpoint-local-port
            endpoint self send-syn
          ;] endpoint with-endpoint
          endpoint true unloop exit
        then
      loop
      0 false
    ; define allocate-tcp-connect-ipv6-endpoint   

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
            time-wait time-wait-remote-ipv6-addr ipv6-unaligned@
            time-wait time-wait-remote-port h@
            time-wait time-wait-local-port h@
            time-wait time-wait-local-seq @
            time-wait time-wait-ack @
            time-wait time-wait-local-window @
            TCP_ACK
            time-wait time-wait-remote-mac-addr 2@
            self send-ipv6-basic-tcp
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
          endpoint [: { endpoint }
            endpoint endpoint-tcp-state@ dup case
              TCP_LISTEN of
                TCP_CLOSED endpoint endpoint-tcp-state!
                endpoint free-endpoint
              endof
            endcase
          ;] endpoint with-endpoint
          case
            TCP_SYN_SENT of
              endpoint self send-ipv6-rst
              endpoint free-endpoint
            endof
            TCP_SYN_RECEIVED of
              endpoint self send-ipv6-rst
              endpoint free-endpoint
            endof
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
            TCP_CLOSED of
              endpoint free-endpoint
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
        endpoint endpoint-ipv6-remote@
        endpoint endpoint-local-port@
        endpoint endpoint-local-seq@
        endpoint endpoint-ack@
        endpoint endpoint-local-window@
        [ TCP_FIN TCP_ACK or ] literal
        endpoint endpoint-remote-mac-addr@
        self send-ipv6-basic-tcp
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
                    endpoint endpoint-send-ready? if
                      endpoint get-endpoint-send-packet
                      endpoint endpoint-send-last?
                      endpoint self send-data-ack
                      endpoint endpoint-send-ready? not true
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
      3 pick endpoint-ipv6-remote@ drop
      PROTOCOL_TCP
      10 pick tcp-header-size +
      ( addr bytes push? endpoint self D: mac-addr )
      ( remote-0 remote-1 remote-2 remote-3 protocol bytes )
      [: { addr bytes push? endpoint self buf }
        endpoint endpoint-local-port@ rev16 buf tcp-src-port h!
        endpoint endpoint-ipv6-remote@ nip nip nip nip rev16
        buf tcp-dest-port h!
        endpoint endpoint-local-seq@ rev buf tcp-seq-no !
        endpoint endpoint-ack@ rev buf tcp-ack-no !
        [ 5 4 lshift ] literal buf tcp-data-offset c!
        push? if [ TCP_ACK TCP_PSH or ] literal else TCP_ACK then
        buf tcp-flags c!
        endpoint endpoint-local-window@ rev16 buf tcp-window-size h!
        0 buf tcp-urgent-ptr h!
        addr buf tcp-header-size + bytes move
        self intf-ipv6-addr@
        endpoint endpoint-ipv6-remote@ drop
        PROTOCOL_TCP buf tcp-header-size bytes + 0 tcp-checksum
        compute-ipv6-checksum rev16 buf tcp-checksum h!
        [ debug? ] [if]
          buf bytes
          [: { buf bytes }
            cr ." @@@@@ SENDING TCP WITH DATA:" buf tcp.
            buf buf tcp-header-size + bytes + dump
          ;] debug-hook execute
        [then]
        true
      ;] 6 pick construct-and-send-ipv6-packet drop
    ; define send-data-ack

    \ Apply DHCP data
    :noname { self -- }
      systick::systick-counter
      dup self dhcp-real-renew-start !
      dup self dhcp-renew-start !
      self dhcp-rebind-start !
      dhcpv6-discovered self dhcp-discover-state !
      self dhcp-sema broadcast
      self dhcp-sema give
    ; define apply-dhcp

    \ Autoconfigure link-local IPv6 address
    :noname { self -- success? }
      self out-frame-interface @ mac-addr@ make-link-local-ipv6-addr
      self detect-duplicate-and-set-intf-ipv6-addr
    ; define autoconfigure-link-local-ipv6-addr

    \ Attempt to set interface IPv6 address, detecting duplicates
    :noname { target-0 target-1 target-2 target-3 self -- success? }
      true self detecting-duplicate? !
      target-0 target-1 target-2 target-3 self resolve-ipv6-addr-mac-addr
      nip nip if
        false
      else
        target-0 target-1 target-2 target-3 self intf-ipv6-addr! true
      then
      false self detecting-duplicate? !
    ; define detect-duplicate-and-set-intf-ipv6-addr
    
    \ Start router discovery
    :noname { self -- }
      systick::systick-counter self router-discovery-start !
      true self router-discovery? !
      self ['] send-icmpv6-router-solicit
      self router-discovery-lock with-lock
      self router-discovery-sema take
    ; define discover-ipv6-router
    
    \ Start IPv6 discovery
    :noname { self -- success? }
      self use-dhcpv6? @ if
        systick::systick-counter self dhcp-discover-start !
        self ['] send-dhcpv6-solicit self dhcp-lock with-lock
        self dhcp-sema take
        self discovered-ipv6-addr ipv6-unaligned@
      else
        self intf-autonomous@ if
          self intf-mac-addr@ self intf-ipv6-prefix@ self intf-ipv6-prefix-len@
          make-global-unicast-ipv6-addr
          true
        else
          false exit
        then
      then
      self detect-duplicate-and-set-intf-ipv6-addr      
    ; define discover-ipv6-addr

    \ Start DNS discovery
    :noname { self -- }
      self use-dhcpv6? @ if
        self found-dns-sema take
      else
        self use-dhcpv6-other? @ if
          systick::systick-counter self dhcp-discover-start !
          self ['] send-dhcpv6-solicit self dhcp-lock with-lock
          self found-dns-sema take
        then
      then
    ; define discover-dns-ipv6-addr
    
    \ Send a DHCPV6_SOLICIT message
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending DHCPV6 SOLICIT" ;] debug-hook execute
      then
      self dhcp-server-duid max-duid-size 0 fill
      0 self dhcp-server-duid-len !
      rng::random $FFFFFF and self current-dhcp-transact-id !
      dhcpv6-wait-advertise self dhcp-discover-state !
      systick::systick-counter self dhcp-discover-stage-start !
      self
      $FFFFFFFFFFFF.
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-client-port
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-server-port
      [ dhcpv6-header-size 26 + ] literal [: { self buf }
        dhcp-log? if
          [: cr ." Constructing DHCPV6 SOLICIT" ;] debug-hook execute
        then
        self current-dhcp-transact-id @ rev buf unaligned!
        DHCPV6_SOLICIT buf dhcpv6-msg-type c!
        dhcpv6-header-size +to buf
        [ OPTION_CLIENTID rev16 ] literal buf hunaligned!
        [ 10 rev16 ] literal buf 2 + hunaligned!
        [ DUID_LL rev16 ] literal buf 4 + hunaligned!
        [ HTYPE_ETHERNET rev16 ] literal buf 6 + hunaligned!
        self intf-mac-addr@ buf 8 + mac!
        [ OPTION_ELAPSED_TIME rev16 ] literal buf 14 + hunaligned!
        [ 2 rev16 ] literal buf 16 + hunaligned!
        systick::systick-counter self dhcp-discover-start @ - 100 / $FFFF min
        buf 18 + hunaligned!
        [ OPTION_ORO rev16 ] literal buf 20 + hunaligned!
        [ 2 rev16 ] literal buf 22 + hunaligned!
        [ OPTION_SOL_MAX_RT rev16 ] literal buf 24 + hunaligned!
        dhcp-log? if
          [: cr ." Constructed DHCPv6 SOLICIT packet" ;] debug-hook execute
        then
      ;] self send-ipv6-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Waiting for DHCPV6 ADVERTISE" ;] debug-hook execute
      then
    ; define send-dhcpv6-solicit

    \ Send a DHCPV6_REQUEST packet
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending DHCPV6 REQUEST" ;] debug-hook execute
      then
      dhcpv6-wait-reply self dhcp-discover-state !
      self
      $FFFFFFFFFFFF.
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-client-port
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-server-port
      [ dhcpv6-header-size 48 + ] literal self dhcp-server-duid-len @ +
      [: { self buf }
        dhcp-log? if
          [: cr ." Constructing DHCPV6 REQUEST" ;] debug-hook execute
        then
        self current-dhcp-transact-id @ rev buf unaligned!
        DHCPV6_REQUEST buf dhcpv6-msg-type c!
        dhcpv6-header-size +to buf
        [ OPTION_CLIENTID rev16 ] literal buf hunaligned!
        [ 10 rev16 ] literal buf 2 + hunaligned!
        [ DUID_LL rev16 ] literal buf 4 + hunaligned!
        [ HTYPE_ETHERNET rev16 ] literal buf 6 + hunaligned!
        self intf-mac-addr@ buf 8 + mac!
        [ OPTION_ELAPSED_TIME rev16 ] literal buf 14 + hunaligned!
        [ 2 rev16 ] literal buf 16 + hunaligned!
        systick::systick-counter self dhcp-discover-start @ - 100 / $FFFF min
        buf 18 + hunaligned!
        [ OPTION_ORO rev16 ] literal buf 20 + hunaligned!
        [ 4 rev16 ] literal buf 22 + hunaligned!
        [ OPTION_SOL_MAX_RT rev16 ] literal buf 24 + hunaligned!
        [ OPTION_DNS_SERVERS ] literal buf 26 + hunaligned!
        [ OPTION_IA_NA rev16 ] literal buf 28 + hunaligned!
        [ 12 rev16 ] literal buf 30 + hunaligned!
        0 buf 32 + unaligned!
        0 buf 36 + unaligned!
        0 buf 40 + unaligned!
        [ OPTION_SERVERID rev16 ] literal buf 44 + hunaligned!
        self dhcp-server-duid-len @ rev16 buf 46 + hunaligned!
        self dhcp-server-duid buf 48 + self dhcp-server-duid-len @ move
        dhcp-log? if
          [: cr ." Constructed DHCPv6 REQUEST packet" ;] debug-hook execute
        then
      ;] self send-ipv6-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Waiting for DHCPv6 REPLY" ;] debug-hook execute
      then
      self dhcp-discover-state @
      dup dhcpv6-discovered <> swap dhcpv6-renewing <> and if
        dhcpv6-wait-reply self dhcp-discover-state !
        systick::systick-counter self dhcp-discover-stage-start !
      then
    ; define send-dhcpv6-request

    \ Send a DHCP RENEW packet
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending DHCPV6 RENEW" ;] debug-hook execute
      then
      dhcpv6-wait-reply self dhcp-discover-state !
      self
      $FFFFFFFFFFFF.
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-client-port
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-server-port
      [ dhcpv6-header-size 48 + ] literal self dhcp-server-duid-len @ +
      [: { self buf }
        dhcp-log? if
          [: cr ." Constructing DHCPV6 RENEW" ;] debug-hook execute
        then
        self current-dhcp-transact-id @ rev buf unaligned!
        DHCPV6_RENEW buf dhcpv6-msg-type c!
        dhcpv6-header-size +to buf
        [ OPTION_CLIENTID rev16 ] literal buf hunaligned!
        [ 10 rev16 ] literal buf 2 + hunaligned!
        [ DUID_LL rev16 ] literal buf 4 + hunaligned!
        [ HTYPE_ETHERNET rev16 ] literal buf 6 + hunaligned!
        self intf-mac-addr@ buf 8 + mac!
        [ OPTION_ELAPSED_TIME rev16 ] literal buf 14 + hunaligned!
        [ 2 rev16 ] literal buf 16 + hunaligned!
        systick::systick-counter self dhcp-discover-start @ - 100 / $FFFF min
        buf 18 + hunaligned!
        [ OPTION_ORO rev16 ] literal buf 20 + hunaligned!
        [ 4 rev16 ] literal buf 22 + hunaligned!
        [ OPTION_SOL_MAX_RT rev16 ] literal buf 24 + hunaligned!
        [ OPTION_DNS_SERVERS ] literal buf 26 + hunaligned!
        [ OPTION_IA_NA rev16 ] literal buf 28 + hunaligned!
        [ 12 rev16 ] literal buf 30 + hunaligned!
        0 buf 32 + unaligned!
        0 buf 36 + unaligned!
        0 buf 40 + unaligned!
        [ OPTION_SERVERID rev16 ] literal buf 44 + hunaligned!
        self dhcp-server-duid-len @ rev16 buf 46 + hunaligned!
        self dhcp-server-duid buf 48 + self dhcp-server-duid-len @ move
        dhcp-log? if
          [: cr ." Constructed DHCPv6 RENEW packet" ;] debug-hook execute
        then
      ;] self send-ipv6-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Waiting for DHCPv6 REPLY" ;] debug-hook execute
      then
      self dhcp-discover-state @
      dup dhcpv6-discovered <> swap dhcpv6-renewing <> and if
        dhcpv6-wait-reply self dhcp-discover-state !
        systick::systick-counter self dhcp-discover-stage-start !
      then
    ; define send-dhcpv6-renew

    \ Send a DHCP REBIND packet
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending DHCPV6 REBIND" ;] debug-hook execute
      then
      dhcpv6-wait-reply self dhcp-discover-state !
      0 self dhcp-server-duid-len !
      self
      $FFFFFFFFFFFF.
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-client-port
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-server-port
      [ dhcpv6-header-size 44 + ] literal
      [: { self buf }
        dhcp-log? if
          [: cr ." Constructing DHCPV6 REBIND" ;] debug-hook execute
        then
        self current-dhcp-transact-id @ rev buf unaligned!
        DHCPV6_REBIND buf dhcpv6-msg-type c!
        dhcpv6-header-size +to buf
        [ OPTION_CLIENTID rev16 ] literal buf hunaligned!
        [ 10 rev16 ] literal buf 2 + hunaligned!
        [ DUID_LL rev16 ] literal buf 4 + hunaligned!
        [ HTYPE_ETHERNET rev16 ] literal buf 6 + hunaligned!
        self intf-mac-addr@ buf 8 + mac!
        [ OPTION_ELAPSED_TIME rev16 ] literal buf 14 + hunaligned!
        [ 2 rev16 ] literal buf 16 + hunaligned!
        systick::systick-counter self dhcp-discover-start @ - 100 / $FFFF min
        buf 18 + hunaligned!
        [ OPTION_ORO rev16 ] literal buf 20 + hunaligned!
        [ 4 rev16 ] literal buf 22 + hunaligned!
        [ OPTION_SOL_MAX_RT rev16 ] literal buf 24 + hunaligned!
        [ OPTION_DNS_SERVERS ] literal buf 26 + hunaligned!
        [ OPTION_IA_NA rev16 ] literal buf 28 + hunaligned!
        [ 12 rev16 ] literal buf 30 + hunaligned!
        0 buf 32 + unaligned!
        0 buf 36 + unaligned!
        0 buf 40 + unaligned!
        dhcp-log? if
          [: cr ." Constructed DHCPv6 REBIND packet" ;] debug-hook execute
        then
      ;] self send-ipv6-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Waiting for DHCPv6 REPLY" ;] debug-hook execute
      then
      self dhcp-discover-state @
      dup dhcpv6-discovered <> swap dhcpv6-renewing <> and if
        dhcpv6-wait-reply self dhcp-discover-state !
        systick::systick-counter self dhcp-discover-stage-start !
      then
    ; define send-dhcpv6-rebind
    
    \ Send a DHCP INFORMATION REQUEST packet
    :noname { self -- }
      dhcp-log? if
        [: cr ." Sending DHCPV6 INFORMATION REQUEST" ;] debug-hook execute
      then
      dhcpv6-wait-reply self dhcp-discover-state !
      self
      $FFFFFFFFFFFF.
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-client-port
      DHCPV6_LINK_LOCAL_MULTICAST dhcpv6-server-port
      [ dhcpv6-header-size 30 + ] literal self dhcp-server-duid-len @ +
      [: { self buf }
        dhcp-log? if
          [: cr ." Constructing DHCPV6 INFORMATION REQUEST" ;]
          debug-hook execute
        then
        self current-dhcp-transact-id @ rev buf unaligned!
        DHCPV6_INFORMATION_REQUEST buf dhcpv6-msg-type c!
        dhcpv6-header-size +to buf
        [ OPTION_CLIENTID rev16 ] literal buf hunaligned!
        [ 10 rev16 ] literal buf 2 + hunaligned!
        [ DUID_LL rev16 ] literal buf 4 + hunaligned!
        [ HTYPE_ETHERNET rev16 ] literal buf 6 + hunaligned!
        self intf-mac-addr@ buf 8 + mac!
        [ OPTION_ELAPSED_TIME rev16 ] literal buf 14 + hunaligned!
        [ 2 rev16 ] literal buf 16 + hunaligned!
        systick::systick-counter self dhcp-discover-start @ - 100 / $FFFF min
        buf 18 + hunaligned!
        [ OPTION_ORO rev16 ] literal buf 20 + hunaligned!
        [ 2 rev16 ] literal buf 22 + hunaligned!
        [ OPTION_DNS_SERVERS ] literal buf 24 + hunaligned!
        [ OPTION_SERVERID rev16 ] literal buf 26 + hunaligned!
        self dhcp-server-duid-len @ rev16 buf 28 + hunaligned!
        self dhcp-server-duid buf 30 + self dhcp-server-duid-len @ move
        dhcp-log? if
          [: cr ." Constructed DHCPv6 INFORMATION REQUEST packet" ;]
          debug-hook execute
        then
      ;] self send-ipv6-udp-packet-raw drop
      dhcp-log? if
        [: cr ." Waiting for DHCPv6 REPLY" ;] debug-hook execute
      then
      self dhcp-discover-state @
      dup dhcpv6-discovered <> swap dhcpv6-renewing <> and if
        dhcpv6-wait-info-reply self dhcp-discover-state !
        systick::systick-counter self dhcp-discover-stage-start !
      then
    ; define send-dhcpv6-information-request

    \ Process a DHCPv6 packet
    :noname ( addr bytes src-0 src-1 src-2 src-3 self -- )
      [: { addr bytes src-0 src-1 src-2 src-3 self }
        bytes dhcpv6-header-size < if
          exit
        then
        dhcpv6-header-size +to addr
        [ dhcpv6-header-size negate ] literal +to bytes
        OPTION_CLIENTID addr bytes find-dhcpv6-opt if
          10 = if
            dup hunaligned@ rev16 DUID_LL = if
              dup 2 + hunaligned@ rev16 HTYPE_ETHERNET = if
                4 + mac@ self intf-mac-addr@ d<> if
                  exit
                then
              else
                drop exit
              then
            else
              drop exit
            then
          else
            drop exit
          then
        else
          2drop exit
        then
        addr unaligned@ rev $FFFFFF and self current-dhcp-transact-id @ = if
          addr dhcpv6-msg-type c@ case
            DHCPV6_ADVERTISE of
              addr bytes self process-dhcpv6-advertise
            endof
            DHCPV6_REPLY of
              addr bytes self process-dhcpv6-reply
            endof
            DHCPV6_DECLINE of
              addr bytes self process-dhcpv6-decline
            endof
          endcase
        then
      ;] over dhcp-lock with-lock
    ; define process-dhcpv6-packet

    \ Process a DHCPv6 ADVERTISE packet
    :noname { addr bytes self -- }
      dhcp-log? if
        [: cr ." Handling DHCPv6 ADVERTISE" ;] debug-hook execute
      then
      OPTION_SERVERID addr bytes find-dhcpv6-opt if
        dup max-duid-size <= over min-duid-size >= and if
          dup self dhcp-server-duid-len !
          self dhcp-server-duid swap move
        else
          exit
        then
      else
        2drop exit
      then
      self use-dhcpv6-other? @ if
        self send-dhcpv6-information-request
      else
        self send-dhcpv6-request
      then
    ; define process-dhcpv6-advertise

    \ Process a DHCPv6 REPLY packet
    :noname { addr bytes self -- }
      dhcp-log? if
        [: cr ." Handling DHCPv6 REPLY" ;] debug-hook execute
      then
      self use-dhcpv6? @ if
        OPTION_SERVERID addr bytes find-dhcpv6-opt if
          dup max-duid-size <= over min-duid-size >= and if
            dup self dhcp-server-duid-len !
            self dhcp-server-duid swap move
          else
            exit
          then
        else
          2drop exit
        then
        OPTION_IA_NA addr bytes find-dhcpv6-opt if
          dup 12 > if
            { addr' bytes' }
            addr' 4 + unaligned@ rev 0= if
              addr' 8 + unaligned@ rev 10000 * { renew-interval }
              addr' 12 + unaligned@ rev 10000 * { rebind-interval }
              renew-interval rebind-interval u> if exit then
              renew-interval 0= if
                default-dhcp-renew-interval to renew-interval
              then
              rebind-interval 0= if
                default-dhcp-rebind-interval to rebind-interval
              then
              rebind-interval renew-interval u< if
                rebind-interval to renew-interval
              then
              OPTION_IAADDR addr' 12 + bytes' 12 - find-dhcpv6-opt if
                24 >= if
                  { addr'' }
                  addr'' 4 + unaligned@ 10000 * { deprecated-interval }
                  addr'' 8 + unaligned@ 10000 * { valid-interval }
                  deprecated-interval valid-interval u> if exit then
                  deprecated-interval renew-interval u< if
                    deprecated-interval to renew-interval
                  then
                  valid-interval renew-interval u< if
                    valid-interval to renew-interval
                  then
                  addr'' ipv6-unaligned@
                  self discovered-ipv6-addr ipv6-unaligned!
                  rebind-interval dhcp-renew-rebind-limit-multiplier *
                  dhcp-renew-rebind-limit-divisor u/ { limit }
                  renew-interval limit u> if limit to renew-interval then
                  renew-interval self dhcp-renew-interval !
                  rebind-interval self dhcp-rebind-interval !
                  self apply-dhcp
                else
                  drop
                then
              else
                2drop
              then
            then
          else
            2drop
          then
        else
          2drop
        then
      then
      OPTION_DNS_SERVERS addr bytes find-dhcpv6-opt if { opt-addr opt-bytes }
        opt-bytes 16 umod 0= opt-bytes 0> and if
          opt-addr ipv6-unaligned@ self dns-server-ipv6-addr!
          self found-dns-sema broadcast
          self found-dns-sema give
        then
      else
        2drop
      then
    ; define process-dhcpv6-reply

    \ Refresh DHCP rebinding
    :noname { self -- }
      systick::systick-counter self dhcp-real-renew-start @ -
      self dhcp-real-renew-interval @ > if
        dhcp-log? if
          [: cr ." Rebinding failed, issuing DHCP REBIND" ;]
          debug-hook execute
        then
        self send-dhcpv6-rebind
      then
    ; define refresh-dhcpv6-rebinding
    
    \ Refresh DHCP renewal
    :noname { self -- }
      systick::systick-counter self dhcp-rebind-start @ -
      self dhcp-rebind-interval @ > if
        dhcpv6-rebinding self dhcp-discover-state !
        dhcp-rebind-retry-interval self dhcp-rebind-interval !
        dhcp-log? if
          [: cr ." Attempting rebinding, issuing DHCP REBIND" ;]
          debug-hook execute
        then
        systick::systick-counter self dhcp-rebind-start !
        self send-dhcpv6-rebind
      else
        systick::systick-counter self dhcp-renew-start @ -
        self dhcp-renew-interval @ > if
          dhcp-log? if
            [: cr ." Attempting renewing, issuing DHCP RENEW" ;]
            debug-hook execute
          then
          systick::systick-counter self dhcp-renew-start !
          self send-dhcpv6-renew
        then
      then
    ; define refresh-dhcpv6-renewing

    \ Refresh DHCP discovered
    :noname { self -- }
      systick::systick-counter self dhcp-renew-start @ -
      self dhcp-renew-interval @ > if
        dhcpv6-renewing self dhcp-discover-state !
        self dhcp-renew-interval @ dhcp-renew-retry-divisor /
        self dhcp-renew-interval !
        dhcp-log? if
          [: cr ." Attempting renewing, issuing DHCP RENEW" ;]
          debug-hook execute
        then
        systick::systick-counter self dhcp-renew-start !
        self send-dhcpv6-renew
      then
    ; define refresh-dhcpv6-discovered

    \ Refresh DHCP wait offer
    :noname { self -- }
      systick::systick-counter self dhcp-discover-stage-start @ -
      dhcp-discover-timeout > if
        systick::systick-counter self dhcp-discover-stage-start !
        dhcp-log? if
          [: cr ." Timeout, retrying DHCP SOLICIT" ;] debug-hook execute
        then
        self send-dhcpv6-solicit
      then
    ; define refresh-dhcpv6-wait-advertise

    \ Refresh DHCP wait ACK
    :noname { self -- }
      systick::systick-counter self dhcp-discover-stage-start @ -
      dhcp-discover-timeout > if
        systick::systick-counter self dhcp-discover-stage-start !
        dhcp-log? if
          [: cr ." TImeout, retrying DHCP REQUEST" ;] debug-hook execute
        then
        self send-dhcpv6-request
      then
    ; define refresh-dhcpv6-wait-reply

    \ Refresh DHCP declined
    :noname { self -- }
      systick::systick-counter self dhcp-discover-stage-start @ -
      dhcpdecline-delay > if
        dhcp-log? if
          [: cr ." Retrying DHCP discovery" ;] debug-hook execute
        then
        self send-dhcpv6-solicit
      then
    ; define refresh-dhcpv6-declined

    \ Refresh router discovery
    :noname { self -- }
      self router-discovery? @ if
        systick::systick-counter self router-discovery-start @ -
        router-discovery-interval > if
          self [: { self }
            self router-discovery? @ if
              self send-icmpv6-router-solicit
              systick::systick-counter self router-discovery-start !
            then
          ;] over router-discovery-lock with-lock
        then
      else
        self router-discovered? @ if
          systick::systick-counter self router-discovered-start @ -
          self router-lifetime @ 10000 * > if
            self [: { self }
              self router-discovered? @ if
                self send-icmpv6-router-solicit
                systick::systick-counter self router-discovery-start !
                true self router-discovery? !
              then
            ;] over router-discovery-lock with-lock
          then
        then
      then
    ; define refresh-router-discovery
    
    \ Implement a jumptable for DHCP states
    create dhcp-jumptable
    ' drop ,
    ' refresh-dhcpv6-wait-advertise ,
    ' refresh-dhcpv6-wait-reply ,
    ' refresh-dhcpv6-discovered ,
    ' refresh-dhcpv6-renewing ,
    ' refresh-dhcpv6-rebinding ,
    ' refresh-dhcpv6-declined ,
    
    \ Refresh DHCP
    :noname ( self -- )
      [:
        dup dhcp-discover-state @ cells dhcp-jumptable + @ execute
      ;] over dhcp-lock with-lock
    ; define refresh-dhcp
    
    \ Refresh an interface
    :noname { self -- }
      self refresh-router-discovery
      self refresh-dhcp
      self refresh-time-wait
      max-endpoints 0 ?do
        self intf-endpoints <ipv6-endpoint> class-size i * +
        self [: { endpoint self }
          endpoint endpoint-refresh-ready? if
            endpoint endpoint-tcp-state@ { state }
            [ debug? ] [if] cr ." ENDPOINT " endpoint h.8 ."  STATE: " state . [then]
            state TCP_ESTABLISHED =
            state TCP_SYN_RECEIVED = or
            state TCP_CLOSE_WAIT = or if
              endpoint endpoint-ipv6-remote@
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
              self send-ipv6-basic-tcp
              endpoint endpoint-ack-sent
              endpoint advance-endpoint-refresh
            else
              state TCP_SYN_SENT = if
                endpoint reset-endpoint-local-port
                endpoint endpoint-ipv6-remote@
                endpoint endpoint-local-port@
                endpoint endpoint-init-local-seq@ 1+
                endpoint endpoint-init-local-seq!
                endpoint endpoint-local-seq@ 1-
                0
                [ mtu-size ipv6-header-size - tcp-header-size - ] literal
                TCP_SYN
                endpoint endpoint-remote-mac-addr@
                self send-ipv6-basic-tcp
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

  \ The IPv6 protocol handler
  <frame-handler> begin-class <ipv6-handler>

    continue-module net-ipv6-internal
      
      \ The IP interface
      cell member ip-interface

    end-module

  end-class

  \ Implement the IPv6 protocol handler
  <ipv6-handler> begin-implement

    \ Constructor
    :noname { ip self -- }
      self <frame-handler>->new
      ip self ip-interface !
    ; define new

    \ Handle a frame
    :noname { addr bytes self -- }
      addr ethh-ether-type h@ [ ETHER_TYPE_IPV6 rev16 ] literal = if
        addr ethh-source-mac mac@ { D: src-mac-addr }
        [ debug? ] [if]
          src-mac-addr
          [: cr ." Received ETHER_TYPE_IPV6 from " mac. ;] debug-hook execute
        [then]
        ethernet-header-size +to addr
        [ ethernet-header-size negate ] literal +to bytes
        bytes ipv6-header-size >= if
          bytes addr ipv6-payload-len h@ rev16 min to bytes
          addr ipv6-dest-addr ipv6-unaligned@ self ipv6-addr-listen? if
            src-mac-addr addr ipv6-src-addr ipv6-unaligned@
            addr ipv6-dest-addr ipv6-unaligned@
            addr bytes find-ipv6-payload if 
              self ip-interface @ process-ipv6-packet
            else
              2drop 2drop 2drop 2drop 2drop 2drop drop
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
  
end-module