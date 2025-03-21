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

begin-module cyw43-control

  oo import
  lock import
  cyw43-structs import
  cyw43-consts import
  cyw43-events import
  cyw43-ioctl import
  cyw43-runner import
  frame-interface import

  \ Use default MAC address
  -1. 2constant default-mac-addr
  
  \ clmload fialed
  : x-clmload-failed ( -- ) ." clmload failed" cr ;

  \ Out of range GPIO
  : x-out-of-range-gpio ( -- ) ." out of range GPIO" cr ;

  \ SSID is too short or too long
  : x-invalid-ssid-len ( -- ) ." SSID is too short or too long" cr ;
  
  \ Passphrase is too short or too long
  : x-invalid-pass-len ( -- ) ." passphrase is too short or too long" cr ;
  
  \ Link state
  0 constant cyw43-link-down
  1 constant cyw43-link-up

  begin-module cyw43-control-internal
    
    \ Download chunk size
    1024 constant cyw43-download-chunk-size
    
    \ Scratchpad size
    cyw43-download-chunk-size 8 + download-header-size + cell align
    constant cyw43-scratch-size
    
    \ Wait a moment
    : wait-a-moment ( -- ) 100 ms ;

  end-module> import
  
  \ CYW43 control class
  <object> begin-class <cyw43-control>

    continue-module cyw43-control-internal
      
      \ CYW43 runner
      <cyw43-runner> class-size member cyw43-core
      
      \ CYW43 control lock
      lock-size member cyw43-lock
      
      \ CYW43 CLM firmware address
      cell member cyw43-clm-addr

      \ CYW43 CLM firmware size
      cell member cyw43-clm-size

      \ Link state
      cell member cyw43-link-state

      \ CYW43 scratch size
      cyw43-scratch-size member cyw43-scratch-buf

      \ MAC address ( default-mac-addr )
      2 cells member cyw43-mac-addr

      \ Country abbreviation
      4 member cyw43-country-abbrev

      \ Country code
      4 member cyw43-country-code

      \ Country revision
      cell member cyw43-country-rev
    
      \ Wait for joining an AP
      method wait-for-cyw43-join ( ssid-info self -- )

      \ Start an AP
      method start-ap
      ( ssid-addr ssid-bytes pass-addr pass-bytes security channel self -- )
      
      \ Load the CLM
      method load-cyw43-clm ( self -- )

      \ Set country info
      method set-cyw43-country ( self -- )

      \ Disable spammy events
      method disable-spammy-cyw43-events ( self -- )

      \ Execute an ioctl
      method ioctl-cyw43 ( kind cmd iface buf-addr buf-size self -- resp-len )

      \ Set an ioctl to a 32-bit value
      method >ioctl-cyw43-32 ( val cmd iface self -- )

      \ Set a variable with an ioctl
      method >iovar-cyw43 ( buf-addr buf-size name-addr name-size self -- )

      \ Get a variable with an ioctl
      method iovar-cyw43>
      ( buf-addr buf-size name-addr name-size self -- resp-len )
      
      \ Set a 32-bit variable with an ioctl
      method >iovar-cyw43-32 ( val name-addr name-size self -- )

      \ Set a double 32-bit variable with an ioctl
      method >iovar-cyw43-32x2 ( val0 val1 name-addr name-size self -- )

      \ Get a 32-bit variable with an ioctl
      method iovar-cyw43-32> ( val name-addr name-size self -- resp-len )

    end-module
      
    \ Initialize the CYW43
    method init-cyw43 ( self -- )

    \ Set power management
    method cyw43-power-management! ( pm self -- )

    \ Set country - must be set prior to executing init-cyw43, and if not set
    \ defaults to an abbreviation and code of XX\x00\x00 and a revision of -1
    method cyw43-country!
    ( abbrev-addr abbrev-bytes code-addr code-bytes country-rev self -- )
    
    \ Join an open AP
    method join-cyw43-open ( ssid-addr ssid-bytes self -- status success? )

    \ Join a WPA2 AP
    method join-cyw43-wpa2
    ( ssid-addr ssid-bytes pass-addr pass-bytes self -- status success? )

    \ Start open AP
    method start-cyw43-open ( ssid-addr ssid-bytes channel self -- )

    \ Start WPA2 AP
    method start-cyw43-wpa2
    ( ssid-addr ssid-bytes pass-addr pass-bytes channel self -- )

    \ Set a GPIO
    method cyw43-gpio! ( val index self -- )

    \ Enable an event
    method enable-cyw43-event ( event self -- )

    \ Enable multiple events
    method enable-cyw43-events ( event-addr event-count self -- )
    
    \ Disable an event
    method disable-cyw43-event ( event self -- )

    \ Disable multiple events
    method disable-cyw43-events ( event-addr event-count self -- )

    \ Disable all events
    method disable-all-cyw43-events ( self -- )
    
    \ Get the CYW43 frame interface
    method cyw43-frame-interface@ ( self -- interface )
    
    \ Dequeue event message
    method get-cyw43-event ( addr self -- )

    \ Poll for event message
    method poll-cyw43-event ( addr self -- found? )

    \ Clear event queue
    method clear-cyw43-events ( self -- )
        
  end-class

  \ Implement the CYW43 control class
  <cyw43-control> begin-implement

    \ Constructor
    :noname { self }
      ( D: mac-addr clm-addr clm-bytes fw-addr fw-bytes pwr clk dio cs )
      ( sm pio )

      \ Initialize the superclass
      self <object>->new

      \ Instantiate the runner
      <cyw43-runner> self cyw43-core init-object

      \ Set the locals
      self cyw43-clm-size !
      self cyw43-clm-addr !
      cyw43-link-down self cyw43-link-state !

      \ Initialize our lock
      self cyw43-lock init-lock

      \ Set the MAC address
      self cyw43-mac-addr 2!

      \ Set the default country
      s" XX" 2dup -1 self cyw43-country!
      
    ; define new

    \ Initialize the CYW43
    :noname { self -- }

      \ Get the CYW43 runner up and, well, running
      self cyw43-core init-cyw43-runner

      self cyw43-core run-cyw43

      wait-a-moment
      
      \ Load the CLM
      self load-cyw43-clm

      [ debug? ] [if] cr ." Configuring misc stuff..." [then]

      \ Disable tx gloming which transfers multiple packets in one request.
      0 s" bus:txglom" self >iovar-cyw43-32
      1 s" apsta" self >iovar-cyw43-32

      [ debug? ] [if] cr ." Getting MAC address..." [then]

      self 2 cells [: { self buf }
        self cyw43-mac-addr 2@ default-mac-addr d= if

          buf 6 s" cur_etheraddr" self iovar-cyw43> drop
          [ debug? ] [if]
            cr ." MAC address: "
            6 0 ?do buf i + c@ h.2 i 5 <> if ." :" then loop
          [then]
          buf 2 + unaligned@ rev buf h@ rev16
          2dup self cyw43-core cyw43-runner::cyw43-frame-interface@ mac-addr!
          self cyw43-mac-addr 2!

        else

          self cyw43-mac-addr 2@
          $FFFF and rev16 buf h!
          rev buf 2 + unaligned!
          buf 6 s" cur_etheraddr" self >iovar-cyw43

          [ debug? ] [if]
            cr ." MAC address: "
            6 0 ?do buf i + c@ h.2 i 5 <> if ." :" then loop
          [then]
          self cyw43-mac-addr 2@
          self cyw43-core cyw43-runner::cyw43-frame-interface@ mac-addr!

        then
      ;] with-aligned-allot

      self set-cyw43-country

      \ set antenna to chip antenna
      [ debug? ] [if] cr ." Set antenna to chip antenna" [then]
      
      0 IOCTL_CMD_ANTDIV 0 self >ioctl-cyw43-32
      [ debug? ] [if] cr ." IOCTL_CMD_ANTDIV=0" [then]

      0 s" bus:txglom" self >iovar-cyw43-32
      [ debug? ] [if] cr ." bus:txglom=0" [then]

      wait-a-moment
      8 s" ampdu_ba_wsize" self >iovar-cyw43-32
      [ debug? ] [if] cr ." ampdu_ba_wsize=8" [then]

      wait-a-moment
      4 s" ampdu_mpdu" self >iovar-cyw43-32
      [ debug? ] [if] cr ." ampdu_mpdu=4" [then]

      wait-a-moment

      self disable-spammy-cyw43-events

      \ set wifi up
      [ debug? ] [if] cr ." Set wifi up" [then]
      0 0 0 0 ioctl-set IOCTL_CMD_UP 0 self ioctl-cyw43 drop
      wait-a-moment

      1 110 0 self >ioctl-cyw43-32 \ SET_GMODE = auto
      0 142 0 self >ioctl-cyw43-32 \ SET_BAND = any

      wait-a-moment

    ; define init-cyw43

    \ Set power management
    :noname { pm self -- }
      pm cyw43-pm-mode 2 = if
        pm cyw43-pm-sleep-rest-ms s" pm2_sleep_ret" self >iovar-cyw43-32
        pm cyw43-pm-beacon-period s" bcn_li_bcn" self >iovar-cyw43-32
        pm cyw43-pm-dtim-period s" bcn_li_dtim" self >iovar-cyw43-32
        pm cyw43-pm-assoc s" assoc_listen" self >iovar-cyw43-32
      then
      pm cyw43-pm-mode 86 0 self >ioctl-cyw43-32
    ; define cyw43-power-management!

    \ Set country - must be set prior to executing init-cyw43, and if not set
    \ defaults to an abbreviation and code of XX\x00\x00 and a revision of -1
    :noname
      { abbrev-addr abbrev-bytes code-addr code-bytes country-rev self -- }
      self cyw43-country-abbrev 4 0 fill
      self cyw43-country-code 4 0 fill
      abbrev-addr self cyw43-country-abbrev abbrev-bytes 4 min 0 max move
      code-addr self cyw43-country-code code-bytes 4 min 0 max move
      country-rev self cyw43-country-rev !
    ; define cyw43-country!
    
    \ Join an open AP
    :noname ( ssid-addr ssid-bytes self -- status success? )
      ssid-info-size [: { ssid-addr ssid-bytes self ssid-info }
        
        ssid-bytes 0 u> ssid-bytes 32 u<= and averts x-invalid-ssid-len
        
        8 s" ampdu_ba_wsize" self >iovar-cyw43-32
        
        0 134 0 self >ioctl-cyw43-32 \ wsec = open
        0 0 s" bsscfg:sup_wpa" self >iovar-cyw43-32x2
        1 20 0 self >ioctl-cyw43-32 \ set_infra = 1
        0 22 0 self >ioctl-cyw43-32 \ set_auth = open (0)

        ssid-bytes ssid-info si-len !
        ssid-addr ssid-info si-ssid ssid-bytes move

        ssid-info self wait-for-cyw43-join
        
      ;] with-aligned-allot
    ; define join-cyw43-open

    \ Join a WPA2 AP
    :noname
      ( ssid-addr ssid-bytes pass-addr pass-bytes self -- status success? )
      ssid-info-size [:
        passphrase-info-size [:
          { ssid-addr ssid-bytes pass-addr pass-bytes self ssid-info pass-info }

          ssid-bytes 0 u> ssid-bytes 32 u<= and averts x-invalid-ssid-len
          pass-bytes 64 u<= averts x-invalid-pass-len
          
          8 s" ampdu_ba_wsize" self >iovar-cyw43-32
          
          4 134 0 self >ioctl-cyw43-32 \ wsec = wpa2
          0 1 s" bsscfg:sup_wpa" self >iovar-cyw43-32x2
          0 $FFFFFFFF s" bsscfg:sup_wpa2_eapver" self >iovar-cyw43-32x2
          0 2500 s" bsscfg:sup_wpa_tmo" self >iovar-cyw43-32x2

          wait-a-moment

          pass-bytes pass-info pi-len h!
          1 pass-info pi-flags h!
          pass-info pi-passphrase 64 0 fill
          pass-addr pass-info pi-passphrase pass-bytes move

          pass-info passphrase-info-size 0 0
          ioctl-set IOCTL_CMD_SET_PASSPHRASE 0 self ioctl-cyw43 drop

          1 20 0 self >ioctl-cyw43-32 \ set_infra = 1
          0 22 0 self >ioctl-cyw43-32 \ set_auth = open (0)
          $80 165 0 self >ioctl-cyw43-32 \ set_wpa_auth

          ssid-bytes ssid-info si-len !
          ssid-info si-ssid 32 0 fill
          ssid-addr ssid-info si-ssid ssid-bytes move
          
          ssid-info self wait-for-cyw43-join

        ;] with-aligned-allot
      ;] with-aligned-allot
    ; define join-cyw43-wpa2

    \ Set a GPIO
    :noname { val index self -- }
      index 3 u< averts x-out-of-range-gpio
      1 index lshift val if 1 else 0 then s" gpioout" self >iovar-cyw43-32x2
    ; define cyw43-gpio!
    
    \ Start open AP
    :noname { ssid-addr ssid-bytes channel self -- }
      ssid-addr ssid-bytes 0 0 SECURITY_OPEN channel self start-ap
    ; define start-cyw43-open

    \ Start WPA2 AP
    :noname { ssid-addr ssid-bytes pass-addr pass-bytes channel self -- }
      ssid-addr ssid-bytes pass-addr pass-bytes WPA2_AES_PSK channel self
      start-ap
    ; define start-cyw43-wpa2

    \ Enable an event
    :noname ( event self -- )
      cyw43-core cyw43-runner::enable-cyw43-event
    ; define enable-cyw43-event

    \ Enable multiple events
    :noname ( event-addr event-count self -- )
      cyw43-core cyw43-runner::enable-cyw43-events
    ; define enable-cyw43-events
    
    \ Disable an event
    :noname ( event self -- )
      cyw43-core cyw43-runner::disable-cyw43-event
    ; define disable-cyw43-event

    \ Disable multiple events
    :noname ( event-addr event-count self -- )
      cyw43-core cyw43-runner::disable-cyw43-events
    ; define disable-cyw43-events

    \ Disable all events
    :noname ( self -- )
      cyw43-core cyw43-runner::disable-all-cyw43-events
    ; define disable-all-cyw43-events
    
    \ Get the CYW43 frame interface
    :noname ( self -- interface )
      cyw43-core cyw43-runner::cyw43-frame-interface@
    ; define cyw43-frame-interface@
    
    \ Dequeue event message
    :noname ( addr self -- )
      cyw43-core cyw43-runner::get-cyw43-event
    ; define get-cyw43-event

    \ Poll for event message
    :noname ( addr self -- found? )
      cyw43-core cyw43-runner::poll-cyw43-event
    ; define poll-cyw43-event

    \ Clear event queue
    :noname ( self -- )
      cyw43-core cyw43-runner::clear-cyw43-events
    ; define clear-cyw43-events

    \ Wait for joining an AP
    :noname ( ssid-info self -- status success? )
      
      event-message-size [: { ssid-info self event }

        self clear-cyw43-events

        \ Enable events before joining so we don't lose any
        EVENT_SET_SSID self enable-cyw43-event
        EVENT_AUTH self enable-cyw43-event
        
        \ Set ssid
        ssid-info ssid-info-size 0 0 ioctl-set IOCTL_CMD_SET_SSID 0
        self ioctl-cyw43 drop
        
        0 0 { auth-status status }

        begin
          event self get-cyw43-event

          [ debug? ] [if]
            cr ." event-type: " event evt-event-type @ h.8
            ."  status: " event evt-status @ h.8
          [then]
          
          event evt-event-type @ EVENT_AUTH =
          event evt-status @ ESTATUS_SUCCESS = and if
            event evt-status @ to auth-status
            false
          else
            event evt-event-type @ EVENT_SET_SSID = if
              event evt-status @ to status
              true
            else
              false
            then
          then
        until

        [ debug? ] [if] cr ." got response" [then]
        
        self disable-all-cyw43-events
        [ debug? ] [if] cr ." disabled all events " [then]
        self clear-cyw43-events
        [ debug? ] [if] cr ." cleared event queue" [then]

        status ESTATUS_SUCCESS = if
          cyw43-link-up self cyw43-link-state !
          [ debug? ] [if] cr ." JOINED" [then]
          status true
        else
          cyw43-link-down self cyw43-link-state !
          [ debug? ] [if]
            cr ." JOIN failed with status=" status . ."  auth=" auth-status .
          [then]
          status false
        then

      ;] with-aligned-allot
      
    ; define wait-for-cyw43-join

    \ Echo a character
    : echo [: ." *" emit ." * " ;] usb::with-usb-output ;
    
    \ Start an AP
    :noname
      ssid-info-with-index-size [: { siwi }
        { ssid-addr ssid-bytes pass-addr pass-bytes security channel self -- }

        ssid-bytes 0 u> ssid-bytes 32 u<= and averts x-invalid-ssid-len

        security SECURITY_OPEN <> if
          pass-bytes MIN_PSK_LEN >= pass-bytes MAX_PSK_LEN <= and
          averts x-invalid-pass-len
        then

        \ Temporarily set wifi down
        0 0 0 0 ioctl-set IOCTL_CMD_DOWN 0 self ioctl-cyw43 drop

        \ Turn off APSTA mode
        0 s" apsta" self >iovar-cyw43-32

        \ Set wifi up again
        0 0 0 0 ioctl-set IOCTL_CMD_UP 0 self ioctl-cyw43 drop

        \ Turn on AP mode
        1 IOCTL_CMD_SET_AP 0 self >ioctl-cyw43-32

        \ Set SSID
        0 siwi siwi-index !
        ssid-bytes siwi siwi-ssid-info si-len !
        siwi siwi-ssid-info si-ssid 32 0 fill
        ssid-addr siwi siwi-ssid-info si-ssid ssid-bytes move
        siwi ssid-info-with-index-size s" bsscfg:ssid" self >iovar-cyw43

        \ Set the channel number
        channel IOCTL_CMD_SET_CHANNEL 0 self >ioctl-cyw43-32

        \ Set security
        0 security $FF and s" bsscfg:wsec" self >iovar-cyw43-32x2

        security SECURITY_OPEN <> if

          \ wpa_auth = WPA2_AUTH_PSK | WPA_AUTH_PSK
          0 $0084 s" bsscfg:wpa_auth" self >iovar-cyw43-32x2

          wait-a-moment

          pass-addr pass-bytes self passphrase-info-size [:
            { pass-addr pass-bytes self pass-info }
            pass-bytes pass-info pi-len h!
            1 pass-info pi-flags h! \ WSEC_PASSPHRASE
            pass-info pi-passphrase 64 0 fill
            pass-addr pass-info pi-passphrase pass-bytes move
            pass-info passphrase-info-size 0 0
            ioctl-set IOCTL_CMD_SET_PASSPHRASE 0 self ioctl-cyw43 drop
          ;] with-aligned-allot
          
        then

        \ Change multicast rate from 1 Mbps to 11 Mbps
        11000000 500000 / s" 2g_mrate" self >iovar-cyw43-32

        \ Start AP
        0 1 s" bss" self >iovar-cyw43-32x2 \ bss = BSS_UP

      ;] with-aligned-allot
      
    ; define start-ap  

    \ Load the CLM
    :noname { self -- }
      
      self cyw43-clm-addr @ { addr }
      self cyw43-clm-size @ { bytes }
      
      begin bytes 0> while

        [ debug? ] [if] cr ." Downloading CLM..." [then]
        
        \ Get the size of an actual chunk being sent
        bytes cyw43-download-chunk-size min { len }

        \ Set the variable to be set
        s\" clmload\x00" self cyw43-scratch-buf swap move

        \ Populate the download header
        self cyw43-scratch-buf 8 + { dh-header }
        DOWNLOAD_FLAG_HANDLER_VER
        addr self cyw43-clm-addr @ = if DOWNLOAD_FLAG_BEGIN or then
        len bytes = if DOWNLOAD_FLAG_END or then
        dh-header dh-flag h!
        DOWNLOAD_TYPE_CLM dh-header dh-dload-type h!
        len dh-header dh-len !
        0 dh-header dh-crc !
        addr dh-header download-header-size + len move

        \ Download the chunk
        self cyw43-scratch-buf len [ 8 download-header-size + ] literal + 0 0
        ioctl-set IOCTL_CMD_SET_VAR 0 self ioctl-cyw43 drop
        
        len +to addr
        len negate +to bytes
        
      repeat

      \ Checking that clmload is okay
      s" clmload_status" self iovar-cyw43-32> 0= averts x-clmload-failed
      
    ; define load-cyw43-clm

    \ Set country info
    :noname { self -- }
      
      [ debug? ] [if] cr ." Setting country info..." [then]

      self country-info-size [: { self buf }
        self cyw43-country-abbrev buf ci-country-abbrev 4 move
        self cyw43-country-code buf ci-country-code 4 move
        self cyw43-country-rev @ buf ci-rev !
        buf country-info-size s" country" self >iovar-cyw43
      ;] with-aligned-allot

      \ set country takes some time, next ioctls fail if we don't wait
      wait-a-moment
    ; define set-cyw43-country

    \ Disable spammy events
    :noname { self -- }

      \ Disable spammy uninteresting events
      self <cyw43-event-mask> [: { self events }
        [ debug? ] [if] cr ." Disable spammy uninteresting events" [then]
        EVENT_RADIO events cyw43-events::disable-cyw43-event
        EVENT_IF events cyw43-events::disable-cyw43-event
        EVENT_PROBREQ_MSG events cyw43-events::disable-cyw43-event
        EVENT_PROBREQ_MSG_RX events cyw43-events::disable-cyw43-event
        EVENT_PROBRESP_MSG events cyw43-events::disable-cyw43-event
        EVENT_ROAM events cyw43-events::disable-cyw43-event
        events cyw43-events::cyw43-event-mask event-mask-size
        s" bsscfg:event_msgs" self >iovar-cyw43
        wait-a-moment
      ;] with-object

    ; define disable-spammy-cyw43-events

    \ Execute an ioctl
    :noname
      ( tx-addr tx-bytes rx-addr rx-bytes kind cmd iface self -- resp-len )
      cyw43-core block-cyw43-ioctl
    ; define ioctl-cyw43

    \ Set an ioctl to a 32-bit value
    :noname { W^ val cmd iface self -- }
      val cell 0 0 ioctl-set cmd iface self ioctl-cyw43 drop
    ; define >ioctl-cyw43-32
    
    \ Set a variable with an ioctl
    :noname { buf-addr buf-size name-addr name-size self -- }
      buf-addr buf-size name-addr name-size self
      name-size buf-size + 1+ 64 max [:
        { buf-addr buf-size name-addr name-size self ioctl-buf }
        name-addr ioctl-buf name-size move
        0 ioctl-buf name-size + c!
        buf-addr ioctl-buf name-size + 1+ buf-size move
        ioctl-buf name-size buf-size + 1+ 0 0 ioctl-set IOCTL_CMD_SET_VAR 0
        self ioctl-cyw43 drop
      ;] with-aligned-allot
    ; define >iovar-cyw43

    \ Get a variable with an ioctl
    :noname { buf-addr buf-size name-addr name-size self -- resp-len }
      buf-addr buf-size name-addr name-size self
      name-size 1+ buf-size max 64 max [:
        { buf-addr buf-size name-addr name-size self ioctl-buf }
        name-addr ioctl-buf name-size move
        0 ioctl-buf name-size + c!
        ioctl-buf name-size 1+ ioctl-buf buf-size 64 max
        ioctl-get IOCTL_CMD_GET_VAR 0 self ioctl-cyw43 { resp-len }
        ioctl-buf buf-addr resp-len move
        resp-len
      ;] with-aligned-allot
    ; define iovar-cyw43>

    \ Set a 32-bit variable with an ioctl
    :noname { W^ val name-addr name-size self -- }
      val cell name-addr name-size self >iovar-cyw43
    ; define >iovar-cyw43-32

    \ Set a double 32-bit variable with an ioctl
    :noname { val0 val1 name-addr name-size self -- }
      0 0 { D^ val } val0 val ! val1 val cell+ !
      val 2 cells name-addr name-size self >iovar-cyw43
    ; define >iovar-cyw43-32x2

    \ Get a 32-bit variable with an ioctl
    :noname { name-addr name-size self -- val }
      0 { W^ val }
      val cell name-addr name-size self iovar-cyw43> drop
      val @
    ; define iovar-cyw43-32>

  end-implement
  
end-module