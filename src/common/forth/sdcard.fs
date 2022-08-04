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

begin-module sd

  oo import
  block-dev import
  spi import
  systick import
  lock import

  \ SD Card timeout
  : x-sd-timeout ( -- ) ." SD card timeout" cr ;

  \ SD Card read error
  : x-sd-read-error ( -- ) ." SD card read error" cr ;

  \ SD Card write error
  : x-sd-write-error ( -- ) ." SD card write error" cr ;

  begin-module sd-internal
    
    \ SD Card init timeout
    2000 systick-divisor * constant sd-init-timeout

    \ SD Card erase timeout
    10000 systick-divisor * constant sd-erase-timeout

    \ SD Card read timeout
    300 systick-divisor * constant sd-read-timeout

    \ SD Card write timeout
    600 systick-divisor * constant sd-write-timeout

    \ init card in spi mode if CS low
    $00 constant CMD_GO_IDLE_STATE

    \ verify SD Memory Card interface operating condition.
    $08 constant CMD_SEND_IF_CMD

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

    \ set the number of write blocks to be pre-erased before writing
    $17 constant CMD_SET_WR_BLOCK_ERASE_COUNT

    \ sends the host capacity support information and activates the card's
    \ initialization process
    $29 constant CMD_SD_SEND_OP_CMD

    \ Start data token for a read or write single block
    $FE constant DATA_START_TOKEN

    \ Buffer count
    8 constant buffer-count

    \ Sector size
    512 constant sector-size

  end-module> import
    
  \ SD Card block device class
  <block-dev> begin-class <sdcard>

    continue-module sd-internal
      
      \ SPI device index
      cell member spi-device

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

    end-module

    \ Init SD card device
    method init-sd ( sd-card -- )

    continue-module sd-internal
      
      \ Send an SD card command
      method send-sd-cmd ( argument command sd-card -- response )

      \ Wait for a start block token
      method wait-sd-start-block ( sd-card -- )

      \ Read a block from the SD card
      method read-sd-block ( index block sd-card -- )

      \ Read a register from the SD card
      method read-sd-register ( addr cmd sd-card -- )

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
  <sdcard> begin-implement

    :noname ( spi-device sd-card -- )
      dup >r [ <block-dev> ] -> new r>
      dup sd-lock init-lock
      dup spi-device !
      0 begin dup buffer-count < while
	2dup cells swap sd-buffer-assign + -1 swap !
	2dup cells swap sd-buffer-age + 0 swap !
	2dup swap sd-buffer-dirty + 0 swap c!
	1+
      repeat
      2drop
    ; define new

    :noname ( sd-card -- bytes ) sector-size ; define block-size

    :noname ( sd-card -- )
      [:
	>r
	false false r@ motorola-spi
	r@ master-spi
	250000 r@ spi-baud!
	8 r@ spi-data-size!
	r> enable-spi
	1 ms \ Must supply minimum 74 clock cycles with CS high
      ;] over sd-lock with-lock
    ; define init-sd

    :noname ( c-addr u block sd-card -- )
      [:
	>r dup r@ find-sd-buffer dup -1 <> if ( c-addr u block index )
	  nip ( c-addr u index )
	else
	  drop r@ select-sd-buffer ( c-addr u block index )
	  tuck cells r@ sd-buffer-assign + ! ( c-addr u index )
	then
	$FF over r@ sd-buffer-dirty + c! ( c-addr u index )
	r> over r> r> ( c-addr u index )
	sector-size * r@ sd-buffers + swap sector-size min move ( )
	r@ age-sd-buffers ( )
	r> r> cells swap sd-buffer-age + 0 swap ! ( )
      ;] over sd-lock with-lock
    ; define block!

    :noname ( c-addr u block sd-card -- )
      [:
	>r dup r@ find-sd-buffer dup -1 <> if ( c-addr u block index )
	  r> over >r >r nip ( c-addr u index )
	else
	  drop r@ select-sd-buffer ( c-addr u block index )
	  2dup cells r@ sd-buffer-assign + ! ( c-addr u block index )
	  tuck swap r@ read-sd-block ( c-addr u index )
	  >r over r> r> ( c-addr u index )
	then
	sector-size * r@ sd-buffers + -rot sector-size min move ( )
	r@ age-sd-buffers ( )
	r> r> cells swap sd-buffer-age + 0 swap ! ( )
      ;] over sd-lock with-lock
    ; define block@

    :noname ( sd-card )
      [:
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
      ;] over sd-lock with-lock
    ; flush-blocks
    
    :noname ( argument command sd-card -- response )
      [:
	>r
	tuck $40 or r@ spi-device @ >spi
	dup 24 rshift r@ spi-device @ >spi
	dup 16 rshift $FF and r@ spi-device @ >spi
	dup 8 rshift $FF and r@ spi-device @ >spi
	$FF and r@ spi-device @ >spi
	case
	  CMD_GO_IDLE_STATE of \ CRC for CMD0 with arg 0x000
	    $95 r@ spi-device @ >spi
	  endof
	  CMD_SEND_IF_COND of \ CRC for CMD8 with arg 0x1AA
	    $87 r@ spi-device @ >spi
	  endof
	  $FF r@ spi-device @ >spi \ CRC is ignored otherwise
	endcase
	5 begin ?dup while 1- r@ spi-device @ spi> drop then
	0 begin
	  dup $100 < if
	    $FF r@ spi-device @ >spi
	    r@ spi-device @ spi> dup $80 and if
	      drop 1+ false
	    else
	      nip true
	    then
	  else
	    drop true
	  then
	until
	rdrop
      ;] critical
    ; define send-sd-cmd

    :noname ( sd-card -- )
      [:
	>r systick-counter
	begin
	  $FF r@ spi-device @ >spi
	  r@ spi-device @ spi>
	  dup $FF = if
	    systick-counter over - sd-read-timeout <= averts x-sd-timeout
	    false
	  else
	    DATA_START_TOKEN = averts x-sd-read-error true
	  then
	until
	rdrop
      ;] critical
    ; define wait-sd-start-block

    :noname ( index block sd-card -- )
      [:
	>r
	CMD_READ_BLOCK r@ send-sd-cmd triggers x-sd-read-error
	r@ wait-sd-start-block
	0 begin dup 512 < while
	  $FF r@ spi-device @ >spi r@ spi-device @ spi>
	  2 pick buffer-count * 2 pick + r@ sd-buffers + c! 1+
	repeat
	2drop
	2 begin ?dup while
	  $FF r@ spi-device @ >spi r@ spi-device @ spi> drop 1-
	repeat
	rdrop
      ;] critical
    ; define read-sd-block

    :noname ( addr cmd sd-card -- )
      [:
	>r
	0 swap r@ send-sd-cmd triggers x-sd-read-error
	r@ wait-sd-start-block
	0 begin dup 16 < while
	  $FF r@ spi-device @ >spi r@ spi-device @ spi>
	  >r 2dup + r> swap c! 1+
	repeat
	2drop
	2 begin ?dup while
	  $FF r@ spi-device @ >spi r@ spi-device @ spi> drop 1-
	repeat
	rdrop
      ;] critical
    ; define read-sd-register

    :noname ( index block sd-card -- )
      [:
	>r
	CMD_WRITE_BLOCK r@ send-sd-cmd triggers x-sd-write-error
	DATA_START_BLOCK r@ spi-device @ >spi r@ spi-device @ spi> drop
	0 begin dup sector-size < while
	  over sector-size * over + r@ sd-buffers + c@ 1+
	  r@ spi-device @ >spi r@ spi-device @ spi> drop
	repeat
	2drop
	sd-write-timeout r@ wait-not-busy
	0 CMD_SEND_STATUS r@ send-sd-cmd triggers x-sd-write-error
	$FF r@ spi-device @ >spi r@ spi-device @ spi> triggers x-sd-write-error
      ;] critical
    ; define write-sd-block

    :noname ( timeout sd-card -- )
      [:
	>r
	systick-counter
	begin systick-counter over - 2 pick < while
	  $FF r@ spi-device @ r> r@ spi-device @ spi>
	  $FF = if 2drop rdrop exit then
	repeat
	drop rdrop ['] x-sd-timeout ?raise
      ;] critical
    ; define wait-not-busy

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
	2dup = if nip rdrop exit then 1+
      repeat
      2drop rdrop -1
    ; define find-sd-buffer

    :noname ( sd-card -- )
      0 begin dup buffer-count < while
	2dup cells swap sd-buffer-age + 1 swap +! 1+
      repeat
      2drop
    ; define age-sd-buffers

    :noname ( sd-card -- index )
      >r -1 0 0 begin dup buffer-count < while
	dup cells swap r@ sd-buffer-assign + @ -1 <>  if
	  dup cells swap r@ sd-buffer-age + @ 3 pick > if
	    rot drop dup cells swap r@ sd-buffer-age + @ -rot nip dup 1+
	  else
	    1+
	  then
	else
	  1+
	then
      repeat
      drop nip rdrop
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