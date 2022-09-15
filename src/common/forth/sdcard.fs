\ Copyright (c) 2022 Travis Bemann
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

compile-to-flash

begin-module sd

  oo import
  block-dev import
  spi import
  pin import
  systick import
  lock import

  \ SD Card timeout
  : x-sd-timeout ( -- ) ." SD card timeout" cr ;

  \ SD Card init error
  : x-sd-init-error ( -- ) ." SD card init error" cr ;
  
  \ SD Card read error
  : x-sd-read-error ( -- ) ." SD card read error" cr ;

  \ SD Card write error
  : x-sd-write-error ( -- ) ." SD card write error" cr ;

  \ SD Card is not SDHC error
  : x-sd-not-sdhc ( -- ) ." SD card is not SDHC" cr ;

  \ Attempted to write to protected block zero
  : x-block-zero-protected ( - ) ." SD card block zero is protected" cr ;
  
  begin-module sd-internal
    
    \ SD Card init timeout
    $100000 constant sd-init-timeout

    \ SD Card erase timeout
    $100000 constant sd-erase-timeout

    \ SD Card read timeout
    $10000 constant sd-read-timeout

    \ SD Card write timeout
    $10000 constant sd-write-timeout
    
    \ SD Card dummy timeout
    $10000 constant sd-dummy-timeout
    
    \ Real SD card read timeout
    $100 constant sd-real-read-timeout
    
    \ Real SD card write timeout
    $100 constant sd-real-write-timeout

    \ init card in spi mode if CS low
    $00 constant CMD_GO_IDLE_STATE

    \ verify SD Memory Card interface operating condition.
    $08 constant CMD_SEND_IF_COND

    \ read the Card Specific Data (CSD register)
    $09 constant CMD_SEND_CSD

    \ read the card identification information (CID register)
    $0A constant CMD_SEND_CID

    \ read the card status register
    $0D constant CMD_SEND_STATUS

    \ read a single block from the card
    $11 constant CMD_READ_BLOCK

    \ write a single data block to the card
    $18 constant CMD_WRITE_BLOCK

    \ write blocks of data until a STOP_TRANSMISSION
    $19 constant CMD_WRITE_MULTIPLE_BLOCK

    \ sets the address of the first block to be erased
    $20 constant CMD_ERASE_WR_BLK_START

    \ sets the address of the last block to be erased
    $21 constant CMD_ERASE_WR_BLK_END

    \ erase all the previously selected blocks
    $26 constant CMD_ERASE

    \ escape for application specific command
    $37 constant CMD_APP_CMD

    \ read the OCR register of a card
    $3A constant CMD_READ_OCR
    
    \ turn CRC checking on/off
    $3B constant CMD_CRC_ON_OFF

    \ set the number of write blocks to be pre-erased before writing
    $17 constant ACMD_SET_WR_BLOCK_ERASE_COUNT

    \ sends the host capacity support information and activates the card's
    \ initialization process
    $29 constant ACMD_SD_SEND_OP_CMD

    \ Ready state
    $00 constant R1_READY_STATE
    
    \ Idle state
    $01 constant R1_IDLE_STATE

    \ Illegal command status bit
    $04 constant R1_ILLEGAL_COMMAND

    \ Start data token for a read or write single block
    $FE constant DATA_START_TOKEN

    \ Buffer count
    8 constant buffer-count

    \ Sector size
    512 constant sector-size
    
  end-module> import
    
  \ SD Card block device class
  <block-dev> begin-class <sd>

    continue-module sd-internal
      
      \ SPI device index
      cell member spi-device
      
      \ Chip select pin
      cell member cs-pin

      \ Lock protecting the SPI device
      lock-size member sd-lock

      \ SPI buffers
      sector-size buffer-count * member sd-buffers

      \ SPI buffer assignments
      buffer-count cells member sd-buffer-assign

      \ SPI buffer age
      buffer-count cells member sd-buffer-age

      \ SPI buffer dirty
      buffer-count member sd-buffer-dirty

      \ Protect block zero
      cell member sd-protect-block-zero
      
      \ Maximum block count
      cell member max-block-count
      
      \ Write through mode
      cell member write-through

    end-module

    \ Init SD card device
    method init-sd ( sd-card -- )

    \ Enable block zero writes
    method write-sd-block-zero! ( enabled sd-card -- )

    continue-module sd-internal
    
      \ Validate a block index
      method validate-block ( block sd-card -- )
      
      \ Implement flush-blocks
      method do-flush-blocks ( sd-card -- )
      
      \ Assert CS
      method assert-cs ( sd-card -- )
      
      \ Deassert CS
      method deassert-cs ( sd-card -- )
      
      \ Send a byte with a response
      method send-get-byte ( tx sd-card -- rx )
      
      \ Send a byte and discard the response
      method send-byte ( tx sd-card -- )
      
      \ Send a dummy byte ($FF) and get the response
      method get-byte ( sd-card -- rx )
      
      \ Send four dummy bytes ($FF) and get the response as a 32-bit value
      method get-word ( sd-card -- rx )
      
      \ Send a dummy byte ($FF) and discard the response
      method dummy-byte ( sd-card -- )
      
      \ Send 8 dummy bytes ($FF) and discard the responses
      method dummy-bytes ( sd-card -- )
      
      \ Send an SD card command
      method send-sd-cmd ( argument command sd-card -- response )
      
      \ End an SD card command
      method end-sd-cmd ( sd-card -- )
      
      \ Send an SD card command with no extra data sent or received
      method send-simple-sd-cmd ( argument command sd-card -- response )

      \ Initialize the SD card itself
      method init-sd-card ( sd-card -- )

      \ Wait for a start block token
      method wait-sd-start-block ( sd-card -- )

      \ Read a block from the SD card
      method read-sd-block ( index block sd-card -- )

      \ Read a register from the SD card
      method read-sd-register ( addr cmd sd-card -- )
      
      \ Write a block to the SD card
      method write-sd-block ( index block sd-card -- )

      \ Wait for the card to go not busy
      method wait-sd-not-busy ( timeout sd-card -- )

      \ Evict an SD buffer
      method evict-sd-buffer ( index sd-card -- )

      \ Find an SD buffer
      method find-sd-buffer ( block sd-card -- index | -1 )
      
      \ Age the buffers
      method age-sd-buffers ( sd-card -- )

      \ Find the oldest buffer
      method oldest-sd-buffer ( sd-card -- index )

      \ Select a free SD buffer, and if one is not free, evict the oldest
      method select-sd-buffer ( sd-card -- index )

    end-module
      
  end-class

  \ Implement SD Card block device class
  <sd> begin-implement

    :noname ( cs-pin spi-device sd-card -- )
      dup >r [ <block-dev> ] -> new r>
      dup sd-lock init-lock
      tuck spi-device !
      tuck cs-pin !
      false over write-through !
      true over sd-protect-block-zero !
      clear-blocks
    ; define new

    :noname ( sd-card -- bytes ) sector-size ; define block-size
    
    :noname ( sd-card -- blocks ) max-block-count @ ; define block-count

    :noname ( sd-card -- )
      [:
	>r
	false false r@ spi-device @ motorola-spi
	r@ spi-device @ master-spi
	125000 r@ spi-device @ spi-baud!
	8 r@ spi-device @ spi-data-size!
        r@ deassert-cs
	r@ spi-device @ enable-spi
	100 begin ?dup while
          1- $FF r@ spi-device @ >spi r@ spi-device @ spi> drop
        repeat
	r> init-sd-card
      ;] over sd-lock with-lock
    ; define init-sd

    \ Enable block zero writes
    :noname ( enabled sd-card -- )
      swap not swap sd-protect-block-zero !
    ; define write-sd-block-zero!

    :noname ( c-addr u block sd-card -- )
      2dup validate-block
      2 pick sector-size <> if
        0 -rot block-part!
      else
        [:
          >r
          dup 0= r@ sd-protect-block-zero @ and triggers x-block-zero-protected
          dup r@ find-sd-buffer dup -1 <> if ( c-addr u block index )
            nip ( c-addr u index )
          else
            drop r@ select-sd-buffer ( c-addr u block index )
            tuck cells r@ sd-buffer-assign + ! ( c-addr u index )
          then
          $FF over r@ sd-buffer-dirty + c! ( c-addr u index )
          r> over >r >r ( c-addr u index )
          sector-size * r@ sd-buffers + swap sector-size min move ( )
          r@ age-sd-buffers ( )
          r> r> cells over sd-buffer-age + 0 swap ! ( sd-card )
          dup write-through @ if do-flush-blocks else drop then ( )
        ;] over sd-lock with-lock
      then
    ; define block!
    
    :noname ( c-addr u offset block sd-card -- )
      2dup validate-block
      [:
        >r
        dup 0= r@ sd-protect-block-zero @ and triggers x-block-zero-protected
        dup r@ find-sd-buffer dup -1 <> if ( c-addr u offset block index )
          nip ( c-addr u offset index )
        else
          drop r@ select-sd-buffer ( c-addr u offset block index )
          2dup cells r@ sd-buffer-assign + ! ( c-addr u offset block index )
          tuck swap r@ read-sd-block ( c-addr u offset index )
        then
        $FF over r@ sd-buffer-dirty + c! ( c-addr u offset index )
        r> over >r >r ( c-addr u offset index )
        sector-size * r@ sd-buffers + ( c-addr u offset buffer )
        swap sector-size min dup >r + ( c-addr u buffer )
        swap sector-size r> - 0 max min move ( )
        r@ age-sd-buffers ( )
        r> r> cells over sd-buffer-age + 0 swap ! ( sd-card )
        dup write-through @ if do-flush-blocks else drop then ( )
      ;] over sd-lock with-lock
    ; define block-part!

    :noname ( c-addr u block sd-card -- )
      2dup validate-block
      [:
	>r dup r@ find-sd-buffer dup -1 <> if ( c-addr u block index )
          r> over >r >r nip ( c-addr u index )
        else
          drop r@ select-sd-buffer ( c-addr u block index )
          2dup cells r@ sd-buffer-assign + ! ( c-addr u block index )
          tuck swap r@ read-sd-block ( c-addr u index )
          r> over >r >r ( c-addr u index )
        then
        sector-size * r@ sd-buffers + -rot sector-size min move ( )
        r@ age-sd-buffers ( )
        r> r> cells swap sd-buffer-age + 0 swap ! ( )
      ;] over sd-lock with-lock
    ; define block@
    
    :noname ( c-addr u offset block sd-card -- )
      2dup validate-block
      [:
	>r dup r@ find-sd-buffer dup -1 <> if ( c-addr u offset block index )
          r> over >r >r nip ( c-addr u offset index )
        else
          drop r@ select-sd-buffer ( c-addr u offset block index )
          2dup cells r@ sd-buffer-assign + ! ( c-addr u offset block index )
          tuck swap r@ read-sd-block ( c-addr u offset index )
          r> over >r >r ( c-addr u offset index )
        then
        sector-size * r@ sd-buffers + ( c-addr u offset buffer )
        swap sector-size min dup >r + ( c-addr u buffer )
        -rot sector-size r> - 0 max min move ( )
        r@ age-sd-buffers ( )
        r> r> cells swap sd-buffer-age + 0 swap ! ( )
      ;] over sd-lock with-lock
    ; define block-part@

    :noname ( sd-card -- )
      [: do-flush-blocks ;] over sd-lock with-lock
    ; define flush-blocks
    
    :noname ( sd-card -- )
      [:
        0 begin dup buffer-count < while
          2dup cells swap sd-buffer-assign + -1 swap !
          2dup cells swap sd-buffer-age + 0 swap !
          2dup swap sd-buffer-dirty + 0 swap c!
          1+
        repeat
        2drop
      ;] over sd-lock with-lock
    ; define clear-blocks
    
    :noname ( write-through sd-card -- )
      >r dup r@ write-through ! if r@ flush-blocks then rdrop
    ; define write-through!
    
    :noname ( sd-card -- write-through )
      write-through @
    ; define write-through@
    
    :noname ( block sd-card -- )
      over 0 >= averts x-block-out-of-range
      block-count < averts x-block-out-of-range
    ; define validate-block
    
    :noname ( sd-card -- )
      >r
      0 begin dup buffer-count < while
        dup cells r@ sd-buffer-assign + @ -1 <> if
          dup r@ sd-buffer-dirty + c@ if
            0 over r@ sd-buffer-dirty + c!
            dup dup cells r@ sd-buffer-assign + @ r@ write-sd-block
          then
        then
        1+
      repeat
      drop rdrop
    ; define do-flush-blocks
    
    :noname ( sd-card -- ) high swap cs-pin @ pin! ; define deassert-cs

    :noname ( sd-card -- ) low swap cs-pin @ pin! ; define assert-cs
    
    :noname ( tx sd-card -- rx )
      ( over h.2 ." >" ) spi-device @ tuck >spi spi> ( dup h.2 space )
    ; define send-get-byte
    
    :noname ( tx sd-card -- ) send-get-byte drop ; define send-byte
    
    :noname ( sd-card -- rx ) $FF swap send-get-byte ; define get-byte
    
    :noname ( sd-card -- rx )
      >r r@ get-byte 8 lshift
      r@ get-byte or 8 lshift
      r@ get-byte or 8 lshift
      r> get-byte or
    ; define get-word
    
    :noname ( sd-card -- ) $FF swap send-get-byte drop ; define dummy-byte
    
    :noname ( sd-card -- )
      ( display-red ) >r 64 begin ?dup while r@ dummy-byte 1- repeat rdrop ( display-normal )
    ; define dummy-bytes
    
    :noname ( argument command sd-card -- response )
\      [:
	>r
	r@ deassert-cs
	r@ dummy-bytes
	r@ assert-cs
	r@ dummy-bytes
	tuck $40 or r@ send-byte
	dup 24 rshift r@ send-byte
	dup 16 rshift $FF and r@ send-byte
	dup 8 rshift $FF and r@ send-byte
	$FF and r@ send-byte
	case
	  CMD_GO_IDLE_STATE of \ CRC for CMD0 with arg 0x000
	    $95 r@ send-byte
	  endof
	  CMD_SEND_IF_COND of \ CRC for CMD8 with arg 0x1AA
	    $87 r@ send-byte
	  endof
	  CMD_READ_OCR of \ CRC for CMD58 with arg $00000000
	    $25 r@ send-byte
	  endof
	  CMD_CRC_ON_OFF of \ CRC for CMD59 with arg $00000000
            $91 r@ send-byte
          endof
	  ACMD_SD_SEND_OP_CMD of \ CRC for ACMD41 with arg $40000000
	    $77 r@ send-byte
	  endof
\	  ACMD_SD_SEND_OP_CMD of \ CRC for ACMD41 with arg $50000000
\	    $17 r@ send-byte
\	  endof
	  $FF r@ send-byte \ CRC is ignored otherwise
	endcase
	0 begin
	  dup $10000 < if
	    r@ get-byte dup $80 and if
	      drop 1+ false
	    else
	      nip true
	    then
	  else
            ['] x-sd-timeout ?raise
	  then
	until
	rdrop
\      ;] critical
    ; define send-sd-cmd
    
    :noname ( sd-card -- )
      >r r@ dummy-bytes r@ deassert-cs r> dummy-bytes
    ; define end-sd-cmd
    
    :noname ( argument cmd sd-card -- response )
      dup >r send-sd-cmd r> end-sd-cmd
    ; define send-simple-sd-cmd

    :noname ( sd-card -- )
\      [:
        >r sd-init-timeout
        begin
          dup 0> averts x-sd-init-error 1-
          0 CMD_GO_IDLE_STATE r@ send-simple-sd-cmd R1_IDLE_STATE =
        until
        drop
        $1AA CMD_SEND_IF_COND r@ send-sd-cmd
        R1_ILLEGAL_COMMAND and triggers x-sd-not-sdhc
        r@ get-word $FF and $AA = averts x-sd-init-error
        r@ end-sd-cmd
        0 CMD_CRC_ON_OFF r@ send-simple-sd-cmd R1_IDLE_STATE = averts x-sd-init-error
        sd-init-timeout
        begin
          dup 0> averts x-sd-init-error 1-
          0 CMD_APP_CMD r@ send-simple-sd-cmd drop
          $40000000 ACMD_SD_SEND_OP_CMD r@ send-simple-sd-cmd R1_READY_STATE =
        until
        drop
        25000000 r@ spi-device @ spi-baud!
        0 CMD_READ_OCR r@ send-sd-cmd R1_READY_STATE = averts x-sd-init-error
        r@ get-word r@ end-sd-cmd
        $C0000000 and averts x-sd-not-sdhc
        r> 16 [:
          2dup 2>r CMD_SEND_CSD rot read-sd-register 2r>
          dup 7 + c@ $3F and 16 lshift over 8 + c@ 8 lshift or swap 9 + c@ or 1+ 1024 *
          swap max-block-count !
        ;] with-aligned-allot
\      ;] critical
    ; define init-sd-card

    :noname ( sd-card -- success? )
\      [:
	>r sd-read-timeout
	begin
	  r@ get-byte dup $FF = if
	    drop dup 0> averts x-sd-timeout 1-
	    false
	  else
	    \ ." *" dup h.2 ." * "
	    dup $F0 and 0= over $0F and 0<> and if
              drop false true
	    else
              DATA_START_TOKEN = if true true else false then
	    then
	  then
	until
	nip rdrop
\      ;] critical
    ; define wait-sd-start-block

    :noname ( index block sd-card -- )
\      [:
	>r
	sd-real-read-timeout begin
          over CMD_READ_BLOCK r@ send-sd-cmd 0= if
            r@ wait-sd-start-block
            dup not if r@ end-sd-cmd swap 1- dup 0> averts sd-read-timeout swap then
          else
            r@ end-sd-cmd 1- dup 0> averts sd-read-timeout false
          then
	until
	2drop
	0 begin dup 512 < while
	  r@ get-byte 2 pick sector-size * 2 pick + r@ sd-buffers + c! 1+
	repeat
	2drop
	2 begin ?dup while r@ dummy-byte 1- repeat
	r> end-sd-cmd
\      ;] critical
    ; define read-sd-block

    :noname ( addr cmd sd-card -- )
\      [:
	>r
	sd-real-read-timeout begin
          over 0 swap r@ send-sd-cmd 0= if
            r@ wait-sd-start-block
            dup not if r@ end-sd-cmd swap 1- dup 0> averts sd-read-timeout swap then
          else
            r@ end-sd-cmd 1- dup 0> averts sd-read-timeout false
          then
        until
        2drop
	0 begin dup 16 < while
	  r@ get-byte >r 2dup + r> swap c! 1+
	repeat
	2drop
	2 begin ?dup while r@ dummy-byte 1- repeat
	r> end-sd-cmd
\      ;] critical
    ; define read-sd-register

    :noname ( index block sd-card -- )
\      [:
	>r
	sd-real-write-timeout begin
          over 0= r@ sd-protect-block-zero @ and triggers x-block-zero-protected
          2 pick 2 pick CMD_WRITE_BLOCK r@ send-sd-cmd 0= if
            DATA_START_TOKEN r@ spi-device @ >spi r@ spi-device @ spi> drop
            0 begin dup sector-size < while
              over sector-size * over + r@ sd-buffers + c@ r@ send-byte 1+
            repeat
            2drop
            sd-write-timeout r@ wait-sd-not-busy
            r@ end-sd-cmd
            0 CMD_SEND_STATUS r@ send-sd-cmd 0= if
              r@ get-byte 0= dup if
                r@ end-sd-cmd swap 1- dup 0> averts sd-write-timeout swap
              else
                r@ end-sd-cmd
              then
            else
              r@ end-sd-cmd 1- dup 0> averts sd-write-timeout false
            then
          else
            r@ end-sd-cmd 1- dup 0> averts sd-write-timeout false
          then
        until
        2drop drop
        rdrop
\      ;] critical
    ; define write-sd-block

    :noname ( timeout sd-card -- )
\      [:
	>r
	begin
          dup 0> averts x-sd-timeout 1-
          r@ get-byte $FF =
	until
	drop rdrop
\      ;] critical
    ; define wait-sd-not-busy

    :noname ( index sd-card -- )
      >r
      dup r@ sd-buffer-dirty + c@ if
	0 over r@ sd-buffer-dirty + c!
	dup dup cells r@ sd-buffer-assign + @ r@ write-sd-block
      then
      0 over cells r@ sd-buffer-age + !
      -1 swap cells r> sd-buffer-assign + !
    ; define evict-sd-buffer
    
    :noname ( block sd-card -- index | -1 )
      >r 0 begin dup buffer-count < while
	2dup cells r@ sd-buffer-assign + @ = if nip rdrop exit then 1+
      repeat
      2drop rdrop -1
    ; define find-sd-buffer

    :noname ( sd-card -- )
      >r 0 begin dup buffer-count < while
        dup cells r@ sd-buffer-age + 1 swap +! 1+
      repeat
      drop rdrop
    ; define age-sd-buffers

    :noname ( sd-card -- index )
      >r 0 -1 0 begin dup buffer-count < while ( oldest-index oldest-age index )
        dup cells r@ sd-buffer-assign + @ -1 <> if ( oldest-index oldest-age index )
          dup cells r@ sd-buffer-age + @ 2 pick >= if ( oldest-index oldest-age index )
            rot drop tuck dup cells r@ sd-buffer-age + @ rot drop swap 1+
          else
            1+
          then
        else
          nip nip $7FFFFFFF over 1+
        then
      repeat
      2drop rdrop
    ; define oldest-sd-buffer

    :noname ( sd-card -- index )
      >r r@ oldest-sd-buffer
      dup cells r@ sd-buffer-assign + @ -1 <> if
	dup r@ evict-sd-buffer
      then
      rdrop
    ; define select-sd-buffer
    
  end-implement
  
end-module

reboot
