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

begin-module ntp

  oo import
  net import
  net-misc import
  net-consts import
  endpoint-process import
  alarm import

  begin-module ntp-internal

    \ Initial NTP delay
    40000 constant init-ntp-poll-multiplier

    \ The minimum NTP polling rate as a power of two
    6 constant min-ntp-poll-pow2
    
    \ The maximum NTP polling rate as a power of two
    10 constant max-ntp-poll-pow2
    
    \ The default NTP delay
    10000 min-ntp-poll-pow2 lshift constant default-ntp-delay

    \ The maximum NTP delay
    655360000 constant max-ntp-delay

    \ The NTP lookup delay
    60 60 * 10000 * constant ntp-lookup-delay

    \ Seconds between 1900 and 1970
    2208988800 constant secs-between-1900-and-1970
    
    \ The NTP packet header structure
    begin-structure ntp-header-size

      cfield: ntp-li-vn-mode
      cfield: ntp-stratum
      cfield: ntp-poll
      cfield: ntp-precision
      field: ntp-root-delay
      field: ntp-root-dispersion
      field: ntp-ref-ident
      8 +field ntp-ref-timestamp
      8 +field ntp-origin-timestamp
      8 +field ntp-rx-timestamp
      8 +field ntp-tx-timestamp

    end-structure

    \ The NTP extension header structure
    begin-structure ntp-ext-header-size

      hfield: ntp-ext-type
      hfield: ntp-ext-len

    end-structure

    \ The minimum extension size
    16 constant min-ntp-ext-size

    \ The key identifier size
    4 constant key-ident-size
    
    \ The MD5 mac size
    16 constant md5-size
    
    \ The minimum NTP packet size
    ntp-header-size ( min-ntp-ext-size 2 * + ) ( key-ident-size + md5-size + )
    constant min-ntp-size

    \ The NTP version
    4 constant ntp-version

    \ Client mode
    3 constant client-mode
    
    \ LI-VN-Mode constant
    0 6 lshift ntp-version 3 lshift or client-mode or constant li-vn-mode

    \ Load an unaligned big-endian 64-bit value
    : 2beunaligned@ { addr -- D: value }
      addr cell + unaligned@ rev addr unaligned@ rev
    ;

    \ Store an unaligned big-endian 64-bit value
    : 2beunaligned! { lo hi addr -- }
      lo rev addr cell + unaligned! hi rev addr unaligned!
    ;

    \ Get an initial NTP delay
    : init-ntp-delay ( -- delay )
      rng::random 0 init-ntp-poll-multiplier s>f f* f>s
    ;
    
  end-module> import

  \ Default NTP port
  123 constant ntp-port

  <endpoint-handler> begin-class <ntp>

    continue-module ntp-internal

      \ The hostname of the NTP server
      max-dns-name-len cell align member ntp-server-dns-name

      \ The name of the hostname of the NTP server
      cell member ntp-server-dns-name-len
      
      \ The IPv4 address of the server to use
      cell member ntp-server-ipv4-addr

      \ The port of the server to use
      cell member ntp-server-port

      \ The interface
      cell member ntp-interface
      
      \ The UDP endpoint for NTP
      cell member ntp-endpoint

      \ The NTP delay
      cell member ntp-delay

      \ Reschedule the delay
      cell member ntp-reschedule

      \ Is the time set
      cell member ntp-time-set

      \ Is the time delay set
      cell member ntp-time-alarm-set
      
      \ The starting time
      2 cells member ntp-start-time

      \ The starting microseconds
      2 cells member ntp-start-us
      
      \ The NTP alarm
      alarm-size member ntp-alarm

      \ The NTP DNS lookup alarm
      alarm-size member ntp-lookup-alarm

      \ The NTP alarm task
      alarm-task-size member ntp-alarm-task

      \ Set the time
      method current-time! ( D: time self -- )

      \ Get the reference time
      method ref-time@ ( self -- D: time )
      
      \ Handle kiss-of-death
      method kiss-of-death ( self -- )
      
      \ Handle reduce rate
      method reduce-rate ( self -- )
      
      \ Set the NTP alarm
      method set-ntp-alarm ( self -- )
      
      \ Send an NTP packet
      method send-ntp-packet ( self -- )

      \ Look up NTP server address
      method lookup-ntp-addr ( self -- )

      \ Set the NTP lookup alarm
      method set-ntp-lookup-alarm ( self -- )

    end-module

    \ Initialize the NTP client
    method init-ntp ( ipv4-addr self -- )
    
    \ Get the time
    method current-time@ ( self -- D: time )

    \ Is the time set
    method time-set? ( self -- time-set? )
      
  end-class

  <ntp> begin-implement

    \ Construct an <ntp> instance
    :noname { ip-interface self -- }
      self <endpoint-handler>->new
      ip-interface self ntp-interface !
      0 self ntp-server-ipv4-addr !
      0 self ntp-endpoint !
      init-ntp-delay self ntp-delay !
      false self ntp-reschedule !
      self ntp-server-dns-name max-dns-name-len 0 fill
      false self ntp-time-set !
      false self ntp-time-alarm-set !
      0. self ntp-start-time 2!
      0. self ntp-start-us 2!
      0 self ntp-server-dns-name-len !
      320 128 1024 0 self ntp-alarm-task init-alarm-task
    ; define new

    \ Initialize the NTP client
    :noname { dns-name dns-name-len port self -- }
      dns-name dns-name-len validate-dns-name
      dns-name self ntp-server-dns-name dns-name-len move
      dns-name-len self ntp-server-dns-name-len !
      port self ntp-server-port !
      \ self set-ntp-lookup-alarm
      self lookup-ntp-addr
    ; define init-ntp
    
    \ Handle an endpoint packet
    :noname { endpoint self -- }
      endpoint self ntp-endpoint @ = if
        endpoint endpoint-rx-data@ { data size }
        size ntp-header-size >= if
          data ntp-stratum c@ 0= if
            data ntp-ref-ident 4 case
              s" DENY" ofstr self kiss-of-death endof
              s" RSTR" ofstr self kiss-of-death endof
              s" RATE" ofstr self reduce-rate endof
            endcasestr
            endpoint self ntp-interface @ endpoint-done
          else
            self ntp-time-set @ if
              self current-time@
              data ntp-origin-timestamp 2beunaligned@ d-
              data ntp-tx-timestamp 2beunaligned@ 2dup { D: tx-timestamp }
              data ntp-rx-timestamp 2beunaligned@ d- d- 2. d/
              tx-timestamp d+ self current-time!
            else
              data ntp-tx-timestamp 2beunaligned@ self current-time!
            then
            endpoint self ntp-interface @ endpoint-done
            endpoint self ntp-interface @ close-udp-endpoint
            0 self ntp-endpoint !
          then
        else
          endpoint self ntp-interface @ endpoint-done
          endpoint self ntp-interface @ close-udp-endpoint
          0 self ntp-endpoint !
        then
      then
    ; define handle-endpoint

    \ Set the time
    :noname { D: time self -- }
      self ntp-time-set @ not if default-ntp-delay self ntp-delay ! then
      self ntp-time-set @ not self ntp-time-alarm-set @ and { reset-alarm }
      time self ntp-start-time 2!
      timer::us-counter self ntp-start-us 2!
      true self ntp-time-set !
      reset-alarm if self set-ntp-alarm then
    ; define current-time!

    \ Get the time
    :noname { self -- D: time }
      timer::us-counter self ntp-start-us 2@ d-
      1000000. ud/mod d>s -rot d>s { secs us }
      us s>f 1000000,0 f/ drop { fract }
      self ntp-start-time 2@ fract secs d+
    ; define current-time@

    \ Get the reference time
    :noname { self -- D: time }
      self ntp-time-set @ if
        self ntp-start-time 2@
      else
        0.
      then
    ; define ref-time@
    
    \ Is the time set
    :noname { self -- time-set? }
      self ntp-time-set @
    ; define time-set?

    \ Handle kiss-of-death
    :noname { self -- }
      0 self ntp-server-ipv4-addr !
      self ntp-endpoint @ self ntp-interface @ close-udp-endpoint
    ; define kiss-of-death

    \ Handle reduce rate
    :noname { self -- }
      self ntp-delay @ 1 lshift max-ntp-delay min self ntp-delay !
    ; define reduce-rate
    
    \ Carry out the NTP alarm
    :noname { self -- }
      self ntp-time-alarm-set @ if
        self ntp-alarm unset-alarm
      then
      self ntp-time-set @ if self ntp-delay @ else init-ntp-delay then
      0 self [: drop send-ntp-packet ;]
      self ntp-alarm self ntp-alarm-task set-alarm-delay
      true self ntp-time-alarm-set !
    ; define set-ntp-alarm

    \ Send an NTP packet
    :noname { self -- }
      self ntp-server-ipv4-addr @ if
        self ntp-reschedule @ not if
          self ntp-endpoint @ 0= if
            EPHEMERAL_PORT self ntp-interface @ allocate-udp-listen-endpoint if
              self ntp-endpoint !
            else
              drop
            then
          then
          self ntp-endpoint @ if
            self
            self ntp-endpoint @ endpoint-local-port@
            self ntp-server-ipv4-addr @
            self ntp-server-port @
            ntp-header-size
            [: { self buffer }
              li-vn-mode buffer ntp-li-vn-mode c!
              15 buffer ntp-stratum c!
              max-ntp-poll-pow2 buffer ntp-poll c!
              0 buffer ntp-precision c!
              [ 0 rev ] literal buffer ntp-root-delay unaligned!
              [ 0 rev ] literal buffer ntp-root-dispersion unaligned!
              [ 0 rev ] literal buffer ntp-ref-ident unaligned!
              0. buffer ntp-ref-timestamp 2beunaligned!
              0. buffer ntp-origin-timestamp 2beunaligned!
              0. buffer ntp-rx-timestamp 2beunaligned!
              self current-time@ buffer ntp-tx-timestamp 2beunaligned!
              true
            ;] self ntp-interface @ send-ipv4-udp-packet
          else
            drop
          then
        then
        self set-ntp-alarm
      then
    ; define send-ntp-packet

    \ Look up NTP server address
    :noname { self -- }
      self ntp-server-dns-name self ntp-server-dns-name-len @
      2dup self ntp-interface @ evict-dns
      self ntp-interface @ resolve-dns-ipv4-addr if
        self ntp-server-ipv4-addr !
        self set-ntp-alarm
      else
        drop
      then
      self set-ntp-lookup-alarm
    ; define lookup-ntp-addr
    
    \ Set the NTP lookup alarm
    :noname { self -- }
      ntp-lookup-delay rng::random 8 umod 10000 * +
      0 self [: drop lookup-ntp-addr ;]
      self ntp-lookup-alarm self ntp-alarm-task set-alarm-delay
    ; define set-ntp-lookup-alarm

  end-implement
  
end-module