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
\ SOFTWARE

begin-module esp-at

  oo import
  pin import
  spi import
  lock import
  closure import
  task import
  timer import
  systick import

  \ ESP-AT device not owned
  : x-esp-at-not-owned ( -- ) ." ESP-AT device not owned" cr ;

  \ ESP-AT timeout exception
  : x-esp-at-timeout ( -- ) ." ESP-AT device timed out" cr ;
  
  \ General ESP-AT error
  : x-esp-at-error ( -- ) ." ESP-AT error" cr ;

  \ Out of range value
  : x-out-of-range-value ( -- ) ." out of range value" cr ;

  \ Connection types
  0 constant no-connect
  1 constant tcp
  2 constant tcpv6
  3 constant udp
  4 constant udpv6
  5 constant ssl
  6 constant sslv6

  \ Connection server/client type
  0 constant client
  1 constant server

  \ Station interface status
  0 constant station-not-inited
  1 constant station-not-connected
  2 constant station-connected
  3 constant station-active
  4 constant station-disconnected
  5 constant station-attempted-wifi

  \ Domain name resolution preference
  1 constant prefer-resolve-ipv4
  2 constant resolve-ipv4-only
  3 constant resolve-ipv6-only

  \ WiFi modes
  0 constant null-mode
  1 constant station-mode
  2 constant softap-mode
  3 constant softap-station-mode

  \ Auto-connect settings
  0 constant not-auto-connect
  1 constant auto-connect

  \ Close-all setting
  0 constant not-close-all
  1 constant close-all
  
  \ Are we logging input and output
  true constant log

  begin-module esp-at-internal

    2 constant SPI_MASTER_WRITE_DATA_TO_SLAVE_CMD
    3 constant SPI_MASTER_READ_DATA_FROM_SLAVE_CMD
    1 constant SPI_MASTER_WRITE_STATUS_TO_SLAVE_CMD
    4 constant SPI_MASTER_READ_STATUS_FROM_SLAVE_CMD

    \ Clear timeout
    1000 constant clear-timeout

    \ Emit a visible character
    : emit-visible { byte -- }
      byte $20 >= byte $0D = or byte $0A = or if byte emit then
    ;

    \ Type a visible string
    : type-visible { c-addr u -- }
      c-addr u + c-addr ?do i c@ emit-visible loop
    ;

    \ Search a string for a string and get the offset
    : find-string { in-addr in-bytes find-addr find-bytes -- offset|-1 }
      in-bytes find-addr find-bytes { total-bytes cur-addr cur-bytes }
      begin in-bytes 0> cur-bytes 0> and while
        in-addr c@ cur-addr c@ = if
          1 +to cur-addr
          -1 +to cur-bytes
        else
          find-addr to cur-addr
          find-bytes to cur-bytes
          in-addr c@ cur-addr c@ = if
            1 +to cur-addr
            -1 +to cur-bytes
          then
        then
        1 +to in-addr
        -1 +to in-bytes
      repeat
      cur-bytes 0= if
        total-bytes in-bytes -
      else
        -1
      then
    ;

    \ Find data in a response with a given header
    : find-data { resp-addr resp-bytes head-addr head-bytes -- addr' bytes' }
      resp-addr resp-bytes head-addr head-bytes find-string
      dup -1 <> averts x-esp-at-error { head-end }
      resp-addr head-end + resp-bytes head-end - s\" \r\n" find-string
      { field-end }
      field-end -1 = if
        resp-bytes head-end - to field-end
      else
        -2 +to field-end
      then
      resp-addr head-end + resp-bytes head-end - field-end -
    ;

    \ Find the nth index of data in a response with a given header
    : find-nth-data
      { resp-addr resp-bytes head-addr head-bytes index -- addr' bytes' found? }
      resp-addr resp-bytes { cur-addr cur-bytes }
      index 1+ 0 ?do
        cur-addr cur-bytes head-addr head-bytes find-string { head-end }
        head-end -1 = if 0 0 false exit then
        head-end +to cur-addr
        head-end negate +to cur-bytes
      loop
      cur-addr cur-bytes s\" \r\n" find-string { field-end }
      field-end -1 = if
        cur-bytes to field-end
      else
        -2 +to field-end
      then
      cur-addr field-end true
    ;

    \ Count the instances of data in a response
    : count-data { resp-addr resp-bytes head-addr head-bytes -- count }
      0 { head-count }
      begin
        resp-addr resp-bytes head-addr head-bytes find-string { head-end }
        head-end -1 <> if
          head-end +to resp-addr
          head-end negate +to resp-bytes
          1 +to head-count
          false
        else
          true
        then
      until
      head-count
    ;

    \ Parse a field
    : parse-field
      { c-addr bytes -- next-addr next-bytes field-addr field-bytes }
      c-addr bytes s" ," find-string dup -1 <> if
        { offset } c-addr offset + bytes offset - c-addr offset 1-
      else
        c-addr bytes + 0 c-addr bytes
      then
    ;

    \ Parse a quoted field
    : parse-quote-field
      { c-addr bytes -- next-addr next-bytes field-addr field-bytes }
      bytes 2 > if
        c-addr c@ [char] " = if
          c-addr bytes s\" \"," find-string dup -1 <> if
            { offset } c-addr offset + bytes offset -
            c-addr 1+ offset 3 -
          else
            c-addr bytes + 1- c@ [char] " = if
              c-addr bytes + 0 c-addr 1+ bytes 2 -
            else
              c-addr bytes + 0 c-addr bytes
            then
          then
        else
          c-addr bytes parse-field
        then
      else
        c-addr bytes + 0 c-addr bytes
      then
    ;

    \ Parse a connection type
    : parse-connect-type ( c-addr bytes -- type )
      case
        s" TCP" ofstr tcp endof
        s" TCPv6" ofstr tcpv6 endof
        s" UDP" ofstr udp endof
        s" UDPv6" ofstr udpv6 endof
        s" SSL" ofstr ssl endof
        s" SSLv6" ofstr sslv6 endof
        ['] x-esp-at-error ?raise
      endcasestr
    ;

    \ Format a connection type
    : format-connect-type ( type -- c-addr bytes )
      case
        tcp of s" TCP" endof
        tcpv6 of s" TCPv6" endof
        udp of s" UDP" endof
        udpv6 of s" UDPv6" endof
        ssl of s" SSL" endof
        sslv6 of s" SSLv6" endof
        ['] x-out-of-range-value ?raise
      endcase
    ;

    \ Format a decimal number
    : format-decimal ( c-addr n -- c-addr bytes )
      base @ { saved-base }
      10 base !
      ['] format-integer try
      saved-base base !
      ?raise
    ;

    \ Parse a decimal  number
    : parse-decimal ( c-addr bytes -- n integer? )
      base @ { saved-base }
      10 base !
      ['] parse-integer try
      saved-base base !
      ?raise
    ;
  
    \ Default ESP-AT timeout in ticks
    50000 constant esp-at-default-timeout
    
    \ The ESP-AT interface parent class
    <object> begin-class <esp-at-interface>

      \ Begin transaction
      method begin-transact ( self -- )

      \ End transaction
      method end-transact ( self -- )
      
      \ Get whether the ESP-AT device is ready
      method esp-at-ready? ( self -- )

      \ Send a byte
      method byte>esp-at ( byte self -- )
      
      \ Receive a byte
      method esp-at>byte ( self -- byte )
      
      \ Send a buffer
      method buffer>esp-at ( c-addr bytes self -- )
      
      \ Receive a buffer
      method esp-at>buffer ( c-addr bytes self -- )
      
      \ Send length message
      method trans-len>esp-at ( len self -- )

      \ Send data message
      method trans-data>esp-at ( data self -- )

      \ Receive length message
      method esp-at>trans-len ( self -- len )

      \ Receive data message
      method esp-at>trans-data ( data self -- )

    end-class

    \ This class is abstract so it has no method implementations
    <esp-at-interface> begin-implement
    end-implement
  
  end-module> import

  \ ESP-AT connection state info class
  <object> begin-class <esp-at-status>

    continue-module esp-at-internal

      begin-structure esp-at-status-entry-size
        cfield: esp-at-status-entry-mux
        cfield: esp-at-status-entry-type
        cfield: esp-at-status-entry-tetype
        cfield: esp-at-status-entry-remote-ip-len
        hfield: esp-at-status-entry-remote-port
        hfield: esp-at-status-entry-local-port
        40 +field esp-at-status-entry-remote-ip
      end-structure

      5 constant esp-at-status-max-count

      cell member esp-at-status-station
      cell member esp-at-status-count
      esp-at-status-max-count esp-at-status-entry-size *
      member esp-at-status-entries

      \ Validate an entry index
      method validate-esp-at-status-entry ( index self -- )

      \ Get an entry
      method esp-at-status-entry ( index self -- entry )
      
    end-module

    \ Set the station status
    method esp-at-status-station! ( station self -- )

    \ Get the station status
    method esp-at-status-station@ ( self -- station )
    
    \ Set the entry count
    method esp-at-status-count! ( count self -- )

    \ Get the entry count
    method esp-at-status-count@ ( self -- count )

    \ Set the entry link ID
    method esp-at-status-mux! ( mux index self -- )

    \ Get the entry link ID
    method esp-at-status-mux@ ( index self -- mux )

    \ Set the entry link type
    method esp-at-status-type! ( type index self -- )

    \ Get the entry link type
    method esp-at-status-type@ ( index self -- type )

    \ Set the entry link server/client type
    method esp-at-status-tetype! ( tetype index self -- )

    \ Get the entry link server/client type
    method esp-at-status-tetype@ ( index self tetype -- )

    \ Set the entry link remote IP
    method esp-at-status-remote-ip! ( ip-addr ip-len index self -- )

    \ Get the entry link remove IP
    method esp-at-status-remote-ip@ ( index self -- ip-addr ip-len )

    \ Set the entry link remote port
    method esp-at-status-remote-port! ( port index self -- )

    \ Get the entry link remote port
    method esp-at-status-remote-port@ ( index self -- port )

    \ Set the entry link local port
    method esp-at-status-local-port! ( port index self -- )

    \ Get the entry link local port
    method esp-at-status-local-port@ ( index self -- port )
    
  end-class

  \ ESP-AT connection state info class implementation
  <esp-at-status> begin-implement

    \ Initialize an ESP-AT connection state info bject
    :noname { self -- }
      self <object>->new
      station-not-inited esp-at-status-station !
      0 self esp-at-status-count !
    ; define new
    
    \ Validate an entry index
    :noname { index self -- }
      index self esp-at-status-count @ u< averts x-out-of-range-value
    ; define validate-esp-at-status-entry

    \ Get an entry
    :noname { index self -- entry }
      index self validate-esp-at-status-entry
      self esp-at-status-entries index esp-at-status-entry-size * +
    ; define esp-at-status-entry

    \ Set the station status
    :noname { station self -- }
      station station-attempted-wifi <= averts x-out-of-range-value
      station self esp-at-status-station !
    ; define esp-at-status-station!

    \ Get the station status
    :noname { self -- station }
      self esp-at-status-station @
    ; define esp-at-status-station@
    
    \ Set the entry count
    :noname { count self -- }
      count esp-at-status-max-count u< averts x-out-of-range-value
      count self esp-at-status-count !
    ; define esp-at-status-count!

    \ Get the entry count
    :noname { self -- count }
      self esp-at-status-count @
    ; define esp-at-status-count@

    \ Set the entry link ID
    :noname { mux index self -- }
      mux 5 u< averts x-out-of-range-value
      mux index self esp-at-status-entry esp-at-status-entry-mux c!
    ; define esp-at-status-mux!

    \ Get the entry link ID
    :noname { index self -- mux }
      index self esp-at-status-entry esp-at-status-entry-mux c@
    ; define esp-at-status-mux@

    \ Set the entry link type
    :noname { type index self -- }
      type sslv6 u<= averts x-out-of-range-value
      type index self esp-at-status-entry esp-at-status-entry-type c!
    ; define esp-at-status-type!

    \ Get the entry link type
    :noname { index self -- type }
      index self esp-at-status-entry esp-at-status-entry-type c@
    ; define esp-at-status-type@

    \ Set the entry link server/client type
    :noname { tetype index self -- }
      tetype server <= averts x-out-of-range-value
      tetype index self esp-at-status-entry esp-at-status-entry-type c!
    ; define esp-at-status-tetype!

    \ Get the entry link server/client type
    :noname { index self tetype -- }
      index self esp-at-status-entry esp-at-status-entry-type c@
    ; define esp-at-status-tetype@

    \ Set the entry link remote IP
    :noname { ip-addr ip-len index self -- }
      ip-len 40 < averts x-out-of-range-value
      ip-len index self esp-at-status-entry esp-at-status-entry-remote-ip-len c!
      ip-addr index self esp-at-status-entry esp-at-status-entry-remote-ip
      ip-len move
    ; define esp-at-status-remote-ip!

    \ Get the entry link remove IP
    :noname { index self -- ip-addr ip-len }
      index self esp-at-status-entry { entry }
      entry esp-at-status-entry-remote-ip
      entry esp-at-status-entry-remote-ip-len c@
    ; define esp-at-status-remote-ip@

    \ Set the entry link remote port
    :noname { port index self -- }
      port 65536 u< averts x-out-of-range-value
      port index self esp-at-status-entry esp-at-status-entry-remote-port h!
    ; define esp-at-status-remote-port!

    \ Get the entry link remote port
    :noname { index self -- port }
      index self esp-at-status-entry esp-at-status-entry-remote-port h@
    ; define esp-at-status-remote-port@

    \ Set the entry link local port
    :noname { port index self -- }
      port 65536 u< averts x-out-of-range-value
      port index self esp-at-status-entry esp-at-status-entry-local-port h!
    ; define esp-at-status-local-port!

    \ Get the entry link local port
    :noname { index self -- port }
      index self esp-at-status-entry esp-at-status-entry-local-port h@
    ; define esp-at-status-local-port@
    
  end-implement

  \ The ESP-AT SPI interface class
  <esp-at-interface> begin-class <esp-at-spi>

    continue-module esp-at-internal

      \ ESP-AT SPI device
      cell member esp-at-spi

      \ The Chip Select pin
      cell member esp-at-cs-pin
      
    end-module

  end-class

  \ The ESP-AT SPI interface class implemntation
  <esp-at-spi> begin-implement

    \ Constructor
    :noname { cs-pin spi self -- }
      spi spi-internal::validate-spi
      cs-pin pin-internal::validate-pin
      self <esp-at-interface>->new
      spi self esp-at-spi !
      cs-pin self esp-at-cs-pin !
      cs-pin output-pin
      high cs-pin pin!
    ; define new

    \ Begin transaction
    :noname { self -- }
      low self esp-at-cs-pin @ pin!
    ; define begin-transact
    
    \ End transaction
    :noname { self -- }
      high self esp-at-cs-pin @ pin!
    ; define end-transact

    \ Get whether the ESP-AT device is ready
    :noname { self -- }
      false
    ; define esp-at-ready?

    \ Send a byte
    :noname { byte self -- }
      byte self esp-at-spi @ >spi self esp-at-spi @ spi> drop
    ; define byte>esp-at
    
    \ Receive a byte
    :noname { self -- byte }
      $00 self esp-at-spi @ >spi self esp-at-spi @ spi>
    ; define esp-at>byte
    
    \ Send a buffer
    :noname { c-addr bytes self -- }
      c-addr bytes self esp-at-spi @ buffer>spi
    ; define buffer>esp-at
    
    \ Receive a buffer
    :noname { c-addr bytes self -- }
      c-addr bytes $00 self esp-at-spi @ spi>buffer
    ; define esp-at>buffer

    \ Send length message
    :noname { W^ len self -- }
      self begin-transact
      SPI_MASTER_WRITE_STATUS_TO_SLAVE_CMD self byte>esp-at
      len 4 self buffer>esp-at
      self end-transact
    ; define trans-len>esp-at

    \ Send data message
    :noname { data bytes self -- }
      self begin-transact
      SPI_MASTER_WRITE_DATA_TO_SLAVE_CMD self byte>esp-at
      0 self byte>esp-at
      bytes 0> if
        data bytes self buffer>esp-at
      then
      bytes 64 < if
        64 bytes ?do 0 self byte>esp-at loop
      then
      self end-transact
    ; define trans-data>esp-at

    \ Receive length message
    :noname { self -- len }
      self begin-transact
      SPI_MASTER_READ_STATUS_FROM_SLAVE_CMD self byte>esp-at
      0 { W^ len }
      len 4 self esp-at>buffer
      self end-transact
      len @ dup -1 = if drop 0 then
    ; define esp-at>trans-len

    \ Receive data message
    :noname { data bytes self -- }
      self begin-transact
      SPI_MASTER_READ_DATA_FROM_SLAVE_CMD self byte>esp-at
      0 self byte>esp-at
      bytes 0> if
        data bytes self esp-at>buffer
      then
      bytes 64 < if
        64 bytes ?do self esp-at>byte drop loop
      then
      self end-transact
    ; define esp-at>trans-data
      
  end-implement

  \ Filter for OK
  : filter-ok ( c-addr bytes -- index found )
    s\" \r\nOK" find-string dup -1 <> if 0 else drop 0 -1 then
  ;

  \ Filter for OK and ERROR
  : filter-ok-error { D: string -- index found }
    string s\" \r\nOK" find-string dup -1 <> if 0 exit else drop then
    string s\" \r\nERROR" find-string dup -1 <> if 1 else drop 0 -1 then
  ;

  \ Filter for >
  : filter-gt ( c-addr bytes -- index found )
    s" >" find-string dup -1 <> if 0 else drop 0 -1 then
  ;

  \ Filter for SEND OK
  : filter-send-ok ( c-addr bytes -- index found )
    s" SEND OK" find-string dup -1 <> if 0 else drop 0 -1 then
  ;

  \ The ESP-AT device class
  <object> begin-class <esp-at>

    continue-module esp-at-internal
      
      \ The ESP-AT device interface
      cell member esp-at-intf

      \ The ESP-AT lock
      lock-size member esp-at-lock

      \ ESP-AT device owner
      cell member esp-at-owner

      \ ESP-AT timeout
      cell member esp-at-timeout

      \ Are we reading a frame
      cell member esp-at-frame?
      
      \ Frame mux
      cell member esp-at-frame-mux

      \ Frame size
      cell member esp-at-frame-size

      \ Current frame size received
      cell member esp-at-frame-recv-size

      \ Frame received callback
      cell member esp-at-recv-xt

      \ Message buffer size
      4096 constant esp-at-buffer-size

      \ ESP8285 max send once size
      2048 constant esp-at-max-send-once-size

      \ Frame buffer size
      4096 constant esp-at-frame-buffer-size

      \ Message buffer
      esp-at-buffer-size member esp-at-buffer

      \ Frame buffer
      esp-at-frame-buffer-size member esp-at-frame-buffer

      \ Validate the ESP-AT device owner
      method validate-esp-at-owner ( self -- )

      \ Send a block of data for a single connection to an ESP-AT device
      method single-block>esp-at ( c-addr bytes self -- )

      \ Send a block of data for a multiple connection to an ESP-AT device
      method multi-block>esp-at ( c-addr bytes mux self -- )

      \ Process data for received frames
      method process-esp-at-frame ( c-addr bytes self -- c-addr' bytes' )

      \ Process data for received frame bodies
      method process-esp-at-frame-body ( c-addr bytes self -- c-addr' bytes' )
      
      \ Process data for received frame headers
      method process-esp-at-frame-header ( c-addr bytes self -- c-addr' bytes' )

      \ Process data for received frame header intros
      method process-esp-at-frame-intro ( c-addr bytes self -- c-addr' bytes' )
      
    end-module

    \ Execute code with an ESP-AT device
    method with-esp-at ( xt self -- )

    \ Execute code with a set timeout
    method with-esp-at-timeout ( xt timeout self -- )

    \ Set ESP-AT frame receive callback
    method esp-at-recv-xt! ( xt self -- ) ( xt: c-addr bytes mux -- )
    
    \ Set the ESP-AT device timeout in ticks
    method esp-at-timeout! ( timeout self -- )

    \ Get the ESP-AT device timeout in ticks
    method esp-at-timeout@ ( self -- timeout )
    
    \ Clear an ESP-AT device's received messages
    method clear-esp-at ( self -- )

    \ Clear an ESP-AT device's received messages without parsing data
    method clear-esp-at-no-parse ( self -- )
    
    \ Send a message to an ESP-AT device
    method msg>esp-at ( c-addr bytes self -- )

    \ End a command
    method end>esp-at ( self -- )

    \ Send an integer to an ESP-AT device
    method integer>esp-at ( n self -- )

    \ Receive a message from an ESP-AT device
    method esp-at>msg ( c-addr bytes self -- bytes' )

    \ Receive a string from an ESP-AT device
    method esp-at>string ( filter-xt self -- c-addr bytes )

    \ Match a string in the output
    method esp-at>match ( filter-xt self -- match )

    \ Wait for a string in the output
    method esp-at>wait ( filter-xt self -- )

    \ Enable/disable multiple connections
    method esp-at-multi! ( multi? self -- )

    \ Get multiple connections enabled/disabled
    method esp-at-multi@ ( self -- multi? )

    \ Enable/disable IPv6 mode
    method esp-at-ipv6! ( ipv6? self -- )

    \ Get IPv6 mode enabled/disabled
    method esp-at-ipv6@ ( self -- ipv6? )

    \ Get the connection state
    method esp-at-status@ ( status self -- )

    \ Set the WiFi mode
    method esp-at-wifi-mode! ( auto-connect-mode wifi-mode self -- )

    \ Reset an ESP-AT device
    method reset-esp-at ( self -- )
    
    \ Poll an ESP-AT device for received data
    method poll-esp-at ( self -- )
    
    \ Connect to a WiFi AP
    method connect-esp-at-wifi ( D: password D: ssid self -- )

    \ Disconnect from a WiFi AP
    method disconnect-esp-at-wifi ( self -- )

    \ Resolve a domain name
    method resolve-esp-at-ip ( prefer D: domain -- c-addr bytes found )
    
    \ Start a single ESP-AT connection
    \
    \ keep-alive is 0 for no TCP keep alive functionality, 1-7200 for
    \ detection interval in seconds.
    method start-esp-at-single
    ( keep-alive type remote-port D: remote-host self -- )
    
    \ Start a multiple ESP-AT connection
    \
    \ keep-alive is 0 for no TCP keep alive functionality, 1-7200 for
    \ detection interval in seconds.
    method start-esp-at-multi
    ( keep-alive type remote-port D: remote-host mux self -- )

    \ Start a server
    method start-esp-at-server ( port self -- )

    \ Delete a server
    method delete-esp-at-server ( close-all? self -- )

    \ Start a server
    method start-esp-at-server ( port self -- )

    \ Delete a server
    method delete-esp-at-server ( close-all? self -- )

    \ Send data for a single connection to an ESP-AT device
    method single>esp-at ( c-addr bytes self -- )

    \ Send data for a multiple connection to an ESP-AT device
    method multi>esp-at ( c-addr bytes mux self -- )
    
    \ Close a single connection on an ESP-AT device
    method close-esp-at-single ( self -- )

    \ Close a multiple connection on an ESP-AT device
    method close-esp-at-multi ( mux self -- )

  end-class

  \ Implement the ESP-AT device class
  <esp-at> begin-implement

    \ Constructor
    :noname { intf self -- }
      self <object>->new
      intf self esp-at-intf !
      0 self esp-at-owner !
      esp-at-default-timeout self esp-at-timeout !
      false self esp-at-frame? !
      -1 self esp-at-frame-mux !
      0 self esp-at-frame-size !
      0 self esp-at-frame-recv-size !
      0 self esp-at-recv-xt !
      self esp-at-lock init-lock
    ; define new

    \ Process data for received frames
    :noname { c-addr bytes self -- }
      begin
        self esp-at-frame? @ if
          c-addr bytes self process-esp-at-frame-body to bytes to c-addr
        then
        self esp-at-frame? @ not if
          c-addr bytes self process-esp-at-frame-header to bytes to c-addr
        then
        bytes 0=
      until
    ; define process-esp-at-frame

    \ Process data for received frame bodies
    :noname { c-addr bytes self -- c-addr' bytes' }
      self esp-at-frame-size @ self esp-at-frame-recv-size @ - bytes min
      { rest-bytes }
      rest-bytes esp-at-frame-buffer-size
      self esp-at-frame-recv-size @ - min { limit-rest-bytes }
      c-addr self esp-at-frame-buffer self esp-at-frame-recv-size @ +
      limit-rest-bytes move
      limit-rest-bytes self esp-at-frame-recv-size +!
      self esp-at-frame-recv-size @ self esp-at-frame-size @ = if
        self esp-at-recv-xt @ if
          self esp-at-frame-buffer self esp-at-frame-size @
          self esp-at-frame-mux @ self esp-at-recv-xt @ execute
        then
        false self esp-at-frame? !
        -1 self esp-at-frame-mux !
        0 self esp-at-frame-size !
        0 self esp-at-frame-recv-size !
      then
      rest-bytes +to c-addr
      rest-bytes negate +to bytes
      c-addr bytes
    ; define process-esp-at-frame-body
    
    \ Process data for received frame headers
    :noname { c-addr bytes self -- c-addr' bytes' }
      c-addr bytes self process-esp-at-frame-intro to bytes to c-addr
      bytes 0> self esp-at-frame-recv-size @ 4 > and if
        begin
          bytes 0> if
            c-addr c@ [char] : <> if
              c-addr c@ self esp-at-frame-buffer
              self esp-at-frame-recv-size @ + c!
              1 self esp-at-frame-recv-size +!
              1 +to c-addr
              -1 +to bytes
              false
            else
              true
            then
          else
            true
          then
        until
        bytes 0> if
          c-addr c@ [char] : = if
            self esp-at-frame-buffer 5 +
            self esp-at-frame-recv-size @ 5 - s" ," find-string { offset }
            offset -1 <> if
              self esp-at-frame-buffer 5 + offset 1- parse-decimal if
                { mux }
                self esp-at-frame-buffer offset +
                self esp-at-frame-recv-size @ offset - parse-decimal if
                  { frame-bytes }
                  true self esp-at-frame? !
                  mux self esp-at-frame-mux !
                  frame-bytes self esp-at-frame-size !
                  0 self esp-at-frame-recv-size !
                else
                  drop 0 self esp-at-frame-recv-size !
                then
              else
                drop 0 self esp-at-frame-recv-size !
              then
            else
              self esp-at-frame-buffer 5 +
              self esp-at-frame-recv-size @ 5 - parse-decimal if
                { frame-bytes }
                true self esp-at-frame? !
                -1 self esp-at-frame-mux !
                frame-bytes self esp-at-frame-size !
                0 self esp-at-frame-recv-size !
              else
                drop 0 self esp-at-frame-recv-size !
              then
            then
            1 +to c-addr
            -1 +to bytes
          then
        then
      then
      c-addr bytes
    ; define process-esp-at-frame-header

    \ Process data for receive frame header intros
    :noname { c-addr bytes self -- c-addr' bytes' }
      bytes 0> self esp-at-frame-recv-size @ 0= and if
        c-addr c@ [char] + = if ."  ==+== "
          [char] + self esp-at-frame-buffer c!
          1 self esp-at-frame-recv-size +!
        then
        1 +to c-addr
        -1 +to bytes
      then
      bytes 0> self esp-at-frame-recv-size @ 1 = and if
        c-addr c@ [char] I = if ."  ==I== "
          [char] I self esp-at-frame-buffer 1 + c!
          1 self esp-at-frame-recv-size +!
          1 +to c-addr
          -1 +to bytes
        else
          0 self esp-at-frame-recv-size !
        then
      then
      bytes 0> self esp-at-frame-recv-size @ 2 = and if
        c-addr c@ [char] P = if ."  ==P== "
          [char] P self esp-at-frame-buffer 2 + c!
          1 self esp-at-frame-recv-size +!
          1 +to c-addr
          -1 +to bytes
        else
          0 self esp-at-frame-recv-size !
        then
      then
      bytes 0> self esp-at-frame-recv-size @ 3 = and if
        c-addr c@ [char] D = if ."  ==D==  "
          [char] D self esp-at-frame-buffer 3 + c!
          1 self esp-at-frame-recv-size +!
          1 +to c-addr
          -1 +to bytes
        else .
          0 self esp-at-frame-recv-size !
        then
      then
      bytes 0> self esp-at-frame-recv-size @ 4 = and if
        c-addr c@ [char] , = if ."  ==,== "
          [char] , self esp-at-frame-buffer 4 + c!
          1 self esp-at-frame-recv-size +!
          1 +to c-addr
          -1 +to bytes
        else
          0 self esp-at-frame-recv-size !
        then
      then
      c-addr bytes
    ; define process-esp-at-frame-intro
    
    \ Execute code with an ESP-AT device
    :noname ( xt self -- ) ( xt: self -- )
      [:
        current-task over esp-at-owner ! dup >r
        [: swap execute ;] try
        0 r> esp-at-owner !
        ?raise
      ;] over esp-at-lock with-lock
    ; define with-esp-at

    \ Execute code with a set timeout
    :noname { xt timeout self -- }
      self esp-at-timeout@ { saved-timeout }
      timeout self esp-at-timeout!
      xt try
      saved-timeout self esp-at-timeout!
      ?raise
    ; define with-esp-at-timeout

    \ Set ESP-AT frame receive callback
    :noname { xt self -- } ( xt: c-addr bytes mux -- )
      xt self esp-at-recv-xt !
    ; define esp-at-recv-xt!
    
    \ Set the ESP-AT device timeout in ticks
    :noname { timeout self -- }
      timeout self esp-at-timeout !
    ; define esp-at-timeout!

    \ Get the ESP-AT device timeout in ticks
    :noname { self -- timeout }
      self esp-at-timeout @
    ; define esp-at-timeout@
    
    \ Validate the ESP-AT device owner
    :noname { self -- }
      self esp-at-owner @ current-task = averts x-esp-at-not-owned
    ; define validate-esp-at-owner

    \ Clear an ESP-AT device's received messages
    :noname { self -- }
      systick-counter { start-systick }
      begin
        begin self esp-at-intf @ esp-at-ready? while
          self esp-at-intf @ esp-at>trans-len { len }
          0 { offset }
          begin offset len < while
            200. delay-us
            len offset - 64 min { recv-bytes }
            self esp-at-buffer recv-bytes self esp-at-intf @ esp-at>trans-data
            200. delay-us
            recv-bytes +to offset
            self esp-at-buffer recv-bytes self process-esp-at-frame
          repeat
          \ log if self esp-at-buffer len type-visible then
        repeat
        systick-counter start-systick - clear-timeout > if
          false self esp-at-frame? !
          0 self esp-at-frame-size !
          0 self esp-at-frame-recv-size !
          -1 self esp-at-frame-mux !
        then
        self esp-at-frame? @ not
      until
    ; define clear-esp-at

    \ Clear an ESP-AT device's received messages without parsing data
    :noname { self -- }
      begin self esp-at-intf @ esp-at-ready? while
        self esp-at-intf @ esp-at>trans-len { len }
        0 { offset }
        begin offset len < while
          200. delay-us
          len offset - 64 min { recv-bytes }
          self esp-at-buffer 64 self esp-at-intf @ esp-at>trans-data
          200. delay-us
          recv-bytes +to offset
        repeat
\        log if self esp-at-buffer len type-visible then
      repeat
    ; define clear-esp-at-no-parse

    \ Send a message to an ESP-AT device
    :noname { c-addr bytes self -- }
      self validate-esp-at-owner
      systick-counter { start-systick }
      bytes self esp-at-intf @ trans-len>esp-at
      0 { offset }
      begin offset bytes < while
        200. delay-us
        begin
          systick-counter start-systick - self esp-at-timeout @ <
          averts x-esp-at-timeout
          self esp-at-intf @ esp-at-ready?
        until
        bytes offset - 0 max 64 min { send-bytes }
        c-addr offset + send-bytes self esp-at-intf @ trans-data>esp-at
        200. delay-us
        send-bytes +to offset
      repeat
    ; define msg>esp-at

    \ End a command
    :noname { self -- }
      systick-counter { start-systick }
      s\" \r\n" self msg>esp-at
      begin
        systick-counter start-systick - self esp-at-timeout @ <
        averts x-esp-at-timeout
        self esp-at-intf @ esp-at-ready?
      until
      0 self esp-at-intf @ trans-len>esp-at
      200. delay-us
    ; define end>esp-at
    
    \ Send an integer to an ESP-AT device
    :noname ( n self -- )
      32 [: { n self buffer }
        buffer n format-decimal self msg>esp-at
      ;] with-allot
    ; define integer>esp-at

    \ Receive a message from an ESP-AT device
    :noname { c-addr bytes self -- bytes' }
      self validate-esp-at-owner
      self esp-at-intf @ esp-at>trans-len { len }
      0 { offset }
      begin offset len < while
        200. delay-us
        len offset - 0 max 64 min { recv-bytes }
        c-addr offset + recv-bytes self esp-at-intf @ esp-at>trans-data
        200. delay-us
        recv-bytes +to offset
      repeat
      bytes len min
\      log if c-addr over type-visible then
    ; define esp-at>msg

    \ Receive a string from an ESP-AT device
    \
    \ Note that filter-xt has the signature ( c-addr bytes -- index found )
    \
    \ If found is -1 then it is treated as having not found the string
    :noname { filter-xt self -- c-addr bytes found }
      self validate-esp-at-owner
      0 { offset }
      systick-counter { start-systick }
      begin
        self esp-at-intf @ esp-at-ready? offset esp-at-buffer-size < and
        if
          systick-counter start-systick - self esp-at-timeout @ <
          averts x-esp-at-timeout
          self esp-at-buffer offset +
          esp-at-buffer-size offset - self esp-at>msg +to offset
          200. delay-us
        then
        self esp-at-buffer offset filter-xt execute dup -1 <> if
          { found-offset found-index }
          self esp-at-buffer found-offset + offset found-offset -
          self process-esp-at-frame
          self esp-at-buffer found-offset found-index true
        else
          2drop
          systick-counter start-systick - self esp-at-timeout @ <
          averts x-esp-at-timeout
          false
        then
      until
    ; define esp-at>string

    \ Match a string in the output
    :noname { filter-xt self -- match }
      filter-xt self esp-at>string nip nip
    ; define esp-at>match

    \ Wait for a string in the output
    :noname { filter-xt self -- }
      filter-xt self esp-at>string drop 2drop
    ; define esp-at>wait

    \ Enable disable multiple connections
    :noname { multi? self -- }
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPMUX=" self msg>esp-at
      multi? if 1 else 0 then self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>match 0= averts x-esp-at-error
    ; define esp-at-multi!
    
    \ Get multiple connections enabled/disabled
    :noname { self -- multi? }
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPMUX?" self msg>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>string 0= averts x-esp-at-error
      s" +CIPMUX:" find-data parse-decimal averts x-esp-at-error
      case 0 of false endof 1 of true endof ['] x-esp-at-error ?raise endcase
    ; define esp-at-multi@

    \ Enable/disable IPv6 mode
    :noname { ipv6? self -- }
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPV6=" self msg>esp-at
      ipv6? if 1 else 0 then self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>match 0= averts x-esp-at-error
    ; define esp-at-ipv6!

    \ Get IPv6 mode enabled/disabled
    :noname { self -- ipv6? }
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPMUX?" self msg>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>string 0= averts x-esp-at-error
      s" +CIPV6:" find-data parse-decimal averts x-esp-at-error
      case 0 of false endof 1 of true endof ['] x-esp-at-error ?raise endcase
    ; define esp-at-ipv6@

    \ Get the connection status
    :noname { status self -- }
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPSTATUS" self msg>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>string 0= averts x-esp-at-error
      { resp-addr resp-bytes }
      resp-addr resp-bytes s" +CIPSTATUS:" count-data
      esp-at-status-max-count min { resp-count }
      resp-count status esp-at-status-count!
      resp-count 0 ?do
        resp-addr resp-bytes s" STATUS:" find-data averts x-esp-at-error
        parse-decimal averts x-esp-at-error
        dup station-attempted-wifi u<= averts x-esp-at-error
        status esp-at-status-station!
        resp-addr resp-bytes s" +CIPSTATUS:" i find-nth-data
        averts x-esp-at-error
        parse-field parse-decimal averts x-esp-at-error
        dup 4 u<= averts x-esp-at-error
        i status esp-at-status-mux!
        parse-quote-field parse-connect-type i status esp-at-status-type!
        parse-quote-field dup 40 u<= averts x-esp-at-error
        i status esp-at-status-remote-ip!
        parse-field parse-decimal averts x-esp-at-error
        dup 65536 u< averts x-esp-at-error
        i status esp-at-status-remote-port!
        parse-field parse-decimal averts x-esp-at-error
        dup 65536 u< averts x-esp-at-error
        i status esp-at-status-local-port!
        parse-field parse-decimal averts x-esp-at-error
        dup server u<= averts x-esp-at-error
        i status esp-at-status-tetype!
      loop
    ; define esp-at-status@

    \ Set the WiFi mode
    :noname { auto-connect-mode mode self -- }
      auto-connect-mode auto-connect u<= averts x-out-of-range-value
      mode softap-station-mode u<= averts x-out-of-range-value
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CWMODE=" self msg>esp-at
      mode self integer>esp-at
      s" ," self msg>esp-at
      auto-connect-mode self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>match 0= averts x-esp-at-error
    ; define esp-at-wifi-mode!

    \ Reset an ESP-AT device
    :noname { self -- }
      self validate-esp-at-owner
      s" AT+RST" self msg>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok self esp-at>wait
    ; define reset-esp-at

    \ Poll an ESP-AT device for received data
    :noname { self -- }
      self validate-esp-at-owner
      self esp-at-intf @ esp-at-ready? if
        self esp-at-buffer esp-at-buffer-size self esp-at>msg { bytes }
        self esp-at-buffer bytes self process-esp-at-frame
      then
    ; define poll-esp-at

    \ Connect to a WiFi AP
    :noname { D: password D: ssid self -- }
      self validate-esp-at-owner
      self clear-esp-at
      s\" AT+CWJAP=\"" self msg>esp-at
      ssid self msg>esp-at
      s\" \",\"" self msg>esp-at
      password self msg>esp-at
      s\" \"\r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>match 0= averts x-esp-at-error
    ; define connect-esp-at-wifi
    
    \ Disconnect from a WiFi AP
    :noname { self -- }
      self validate-esp-at-owner
      self clear-esp-at
      s\" AT+CWQAP\r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>wait
    ; define disconnect-esp-at-wifi
    
    \ Resolve a domain name
    :noname { prefer D: domain self -- c-addr bytes found }
      prefer prefer-resolve-ipv4 u>= averts x-out-of-range-value
      prefer resolve-ipv6-only u<= averts x-out-of-range-value
      self validate-esp-at-owner
      self clear-esp-at
      s\" AT+CIPDOMAIN=\"" self msg>esp-at
      domain self msg>esp-at
      s\" \"," self msg>esp-at
      prefer self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>string 0<> if 0 0 false exit then
      { D: resp }
      resp s" +CIPDOMAIN:" self find-data parse-quote-field 2swap 2drop true
    ; define resolve-esp-at-ip

    \ Start a single ESP-AT connection
    \
    \ keep-alive is 0 for no TCP keep alive functionality, 1-7200 for
    \ detection interval in seconds.
    :noname { keep-alive type remote-port D: remote-host self -- }
      type tcp = type tcpv6 = or averts x-out-of-range-value
      remote-port 65536 u< averts x-out-of-range-value
      keep-alive 7200 u<= averts x-out-of-range-value
      self validate-esp-at-owner
      self clear-esp-at
      s\" AT+CIPSTART=\"" self msg>esp-at
      type format-connect-type self msg>esp-at
      s\" \",\"" self msg>esp-at
      remote-host self msg>esp-at
      s\" \"," self msg>esp-at
      remote-port self integer>esp-at
      s" ," self msg>esp-at
      keep-alive self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>match 0= averts x-esp-at-error
    ; define start-esp-at-single
    
    \ Start a multiple ESP-AT connection
    \
    \ keep-alive is 0 for no TCP keep alive functionality, 1-7200 for
    \ detection interval in seconds.
    :noname { keep-alive type remote-port D: remote-host mux self -- }
      type tcp = type tcpv6 = or averts x-out-of-range-value
      mux 4 u<= averts x-out-of-range-value
      remote-port 65536 u< averts x-out-of-range-value
      keep-alive 7200 u<= averts x-out-of-range-value
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPSTART=" self msg>esp-at
      mux self integer>esp-at
      s\" ,\"" self msg>esp-at
      type format-connect-type self msg>esp-at
      s\" \",\"" self msg>esp-at
      remote-host self msg>esp-at
      s\" \"," self msg>esp-at
      remote-port self integer>esp-at
      s" ," self msg>esp-at
      keep-alive self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>match 0= averts x-esp-at-error
    ; define start-esp-at-multi

    \ Start a server
    :noname { port self -- }
      port 65536 u< averts x-out-of-range-value
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPSERVER=1,"
      port self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>match 0= averts x-esp-at-error
    ; define start-esp-at-server

    \ Delete a server
    :noname { close-all? self -- }
      close-all? close-all u<= averts x-out-of-range-value
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPSERVER=0,"
      close-all? self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self esp-at>match 0= averts x-esp-at-error
    ; define delete-esp-at-server

    \ Send data for a single connection to an ESP-AT device
    :noname { c-addr bytes self -- }
      self validate-esp-at-owner
      0 { offset }
      begin offset bytes < while
        bytes offset - esp-at-max-send-once-size min { send-bytes }
        c-addr offset + send-bytes self single-block>esp-at
        send-bytes +to offset
      repeat
    ; define single>esp-at

    \ Send data for a multiple connection to an ESP-AT device
    :noname { c-addr bytes mux self -- }
      self validate-esp-at-owner
      0 { offset }
      begin offset bytes < while
        bytes offset - esp-at-max-send-once-size min { send-bytes }
        c-addr offset + send-bytes mux self multi-block>esp-at
        send-bytes +to offset
      repeat
    ; define multi>esp-at

    \ Send a block of data for a single connection to an ESP-AT device
    :noname { c-addr bytes self -- }
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPSEND=" self msg>esp-at
      bytes self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-gt self ['] esp-at>wait 50000 self with-esp-at-timeout
      self clear-esp-at-no-parse
      c-addr bytes self msg>esp-at
      self end>esp-at
      ['] filter-send-ok self ['] esp-at>wait 100000 self with-esp-at-timeout
    ; define single-block>esp-at
      
    \ Send a block of data for a multiple connection to an ESP-AT device
    :noname { c-addr bytes mux self -- }
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPSEND=" self msg>esp-at
      mux self integer>esp-at
      s" ," self msg>esp-at
      bytes self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-gt self ['] esp-at>wait 50000 self with-esp-at-timeout
      self clear-esp-at-no-parse
      c-addr bytes self msg>esp-at
      self end>esp-at
      ['] filter-send-ok self ['] esp-at>wait 100000 self with-esp-at-timeout
    ; define multi-block>esp-at

    \ Close a single connection on an ESP-AT device
    :noname { self -- }
      self validate-esp-at-owner
      self clear-esp-at
      s\" AT+CIPCLOSE" self msg>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      ['] filter-ok-error self ['] esp-at>match 50000 self with-esp-at-timeout
      0= averts x-esp-at-error
    ; define close-esp-at-single

    \ Close a multiple connection on an ESP-AT device
    :noname { mux self -- }
      self validate-esp-at-owner
      self clear-esp-at
      s" AT+CIPCLOSE=" self msg>esp-at
      mux self integer>esp-at
      s\" \r\n" self msg>esp-at
      self end>esp-at
      [: { D: string -- index found }
        string s\" \r\nOK" find-string dup -1 <> if 0 exit else drop then
        string s\" link is not" find-string dup -1 <> if 1 else drop 0 -1 then
      ;] self ['] esp-at>wait 50000 self with-esp-at-timeout
    ; define close-esp-at-multi

  end-implement
  
end-module
