\ Copyright (c) 2021-2023 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-module block

  internal import
  task import
  qspi import

  \ Block size
  1024 constant block-size

  \ Write failure exception
  : x-block-write-fail ( -- ) ." unable to write block" cr ;

  begin-module block-internal

    \ Sector size
    1024 64 * constant sector-size

    \ Sector count
    qspi-size sector-size / constant sector-count

    \ Block count within a sector
    sector-size block-size / 1- constant sector-block-count

    \ Block count
    sector-block-count sector-count * constant block-count

    \ Block ID offset
    0 constant sector-block-id-map-offset

    \ Newer block pointer offset
    sector-block-count cells constant sector-old-flag-map-offset

    \ Block erase count offset
    sector-block-count 2 * cells constant sector-erase-count-offset
    
    \ Unwritten flash
    $FFFFFFFF constant unwritten
    
    \ Allot a buffer storing the count of free blocks in sectors
    sector-count buffer: sector-free-map

    \ Allot a buffer storing the count of old blocks in sectors
    sector-count buffer: sector-old-map
    
    \ Saved sector for relocation
    variable saved-sector
    
    \ Get the sector index of an address
    : sector-index ( addr -- index ) qspi-base - sector-size / ;

    \ Get the address of a sector
    : sector-addr ( index -- addr ) sector-size * qspi-base + ;

    \ Get the blockk and sector index of a block address
    : block-index ( addr -- block-index sector-index )
      qspi-base - dup sector-size umod block-size / 1 - swap sector-size /
    ;

    \ Get the address of a sector in a block
    : block-addr ( block-index sector-index -- addr )
      sector-addr swap 1 + block-size * +
    ;

    \ Get a block's id
    : block-id@ ( block-index sector-index -- id )
      sector-addr sector-block-id-map-offset + swap cells + @
    ;

    \ Get whether a block is old
    : old-flag@ ( block-index sector-index -- flag )
      sector-addr sector-old-flag-map-offset + swap cells + @ unwritten <>
    ;

    \ Get the erase count for a sector
    : erase-count@ ( sector-index -- count )
      sector-addr sector-erase-count-offset + @
    ;

    \ Get the free count for a sector
    : free-count@ ( index -- count ) sector-free-map + c@ ;

    \ Get the old count for a sector
    : old-count@ ( index -- count ) sector-old-map + c@ ;

    \ Get the new count for a sector
    : new-count ( index -- count )
      dup free-count@ swap old-count@ + sector-block-count swap -
    ;

    \ Set the free count for a sector
    : free-count! ( count index -- ) sector-free-map + c! ;

    \ Set the old count for a sector
    : old-count! ( count index -- ) sector-old-map + c! ;

    \ Add to the free count for a sector
    : free-count+! ( change index -- ) sector-free-map + c+! ;

    \ Add to the old count for a sector
    : old-count+! ( change index -- ) sector-old-map + c+! ;

    \ Find the next sector containing new sectors, or -1 if none can be found
    : next-new-sector ( index -- index|-1 )
      begin
	dup sector-count < if
	  dup saved-sector @ <> if
	    dup new-count 0= if 1+ false else true then
	  else
	    1+ false
	  then
	else
	  drop -1 true
	then
      until
    ;

    \ Find a block in a sector, or return 0 if none is found
    : find-sector-block ( id sector-index -- addr|0 )
      sector-block-count 0 ?do
	i over block-id@ 2 pick = if
	  i over old-flag@ not if
	    i swap block-addr nip unloop exit
	  then
	then
      loop
      2drop 0
    ;

    \ Find a block by block id, or return 0 if none is found
    : find-block ( id -- addr|0 )
      0 begin
	next-new-sector dup -1 <> if
	  2dup find-sector-block ?dup if nip nip true else
	    1+ false
	  then
	else
	  drop drop 0 true
	then
      until
    ;

    \ Reuse a sector
    : reuse-sector ( erase-count index -- )
      0 over old-count!
      sector-block-count over free-count!
      sector-addr dup erase-qspi-sector
      sector-erase-count-offset + qspi!
    ;

    \ Find a completely old sector other than the saved sector, or return -1
    \ if no sectors are completel old
    : find-old-sector ( -- erase-count index|-1 )
      $FFFFFFFF -1
      sector-count 0 ?do
	i old-count@ sector-block-count = if
	  i saved-sector @ <> if
	    i erase-count@ 1+ >r over r> u> if
	      2drop i erase-count@ 1+ i
	    then
	  then
	then
      loop
    ;

    \ Find a completely old or free sector, or return -1 if no sectors are
    \ completely old or free
    : find-saved-sector ( -- index|-1 )
      $FFFFFFFF -1
      sector-count 0 ?do
	i old-count@ sector-block-count =
	i free-count@ sector-block-count = or if
	  i erase-count@ 1+ >r over r> u> if
	    2drop i erase-count@ 1+ i
	  then
	then
      loop
      nip
    ;
    
    \ Find a sector to reorganize
    : find-reorganize-sector ( -- index|-1 )
      -1 -1
      sector-count 0 ?do
	i saved-sector @ <> if
	  i new-count over u< i new-count sector-block-count u< and if
	    2drop i dup new-count
	  then
	then
      loop
      drop
    ;

    \ Find a sector with a free block, or return -1 if no sectors have free
    \ blocks
    : find-sector-with-free-block ( -- index|-1 )
      sector-count 0 ?do
	i saved-sector @ <> i free-count@ 0> and if i unloop exit then
      loop
      -1
    ;

    \ Find a free block within a sector, or return 0 if there are no free blocks
    \ within the sector
    : find-free-block-in-sector ( sector-index -- block-index|-1 )
      sector-block-count 0 ?do
	i over block-id@ unwritten = if i nip unloop exit then
      loop
      drop -1
    ;

    \ This should never happen
    : x-should-never-happen ( -- ) ." this should never happen!" cr ;

    \ Find a free block, or return -1 -1 if no free block can be allocated
    : find-free-block ( -- block-index sector-index | -1 -1 )
      find-sector-with-free-block dup -1 <> if
	dup find-free-block-in-sector dup -1 <>
	averts x-should-never-happen swap
      else
	drop find-old-sector dup -1 <> if
	  tuck reuse-sector dup find-free-block-in-sector dup -1 <> if
	    swap
	  else
	    true triggers x-should-never-happen
	  then
	else
	  nip -1
	then
      then
    ;

    \ Find a free block in the saved sector
    : find-saved-block ( -- block-index sector-index )
      saved-sector @ find-free-block-in-sector dup -1 <>
      averts x-should-never-happen
      saved-sector @
    ;
    
    \ Write a newer pointer for a block
    : old-flag! ( block-index sector-index -- )
      0 -rot sector-addr sector-old-flag-map-offset + swap cells + qspi!
    ;

    \ Write a block id for a block
    : block-id! ( id block-index sector-index -- )
      sector-addr sector-block-id-map-offset + swap cells + qspi!
    ;

    \ Write block data
    : data! ( data-addr block-addr -- )
      1024 swap mass-qspi!
    ;

    \ Write to a block (the data written must be of size block-size),
    \ Copy blocks into the saved sector
    : copy-blocks-into-saved-sector ( sector-index -- )
      sector-block-count 0 ?do
	i over old-flag@ not if
	  find-saved-block
	  -1 over free-count+!
	  2dup i 5 pick block-id@
	  -rot block-id!
	  i 3 pick block-addr ram-here block-size move
	  ram-here -rot block-addr data!
	then
      loop
      drop
    ;

    \ Mark all blocks in a sector as old
    : mark-all-old ( sector-index -- )
      sector-block-count 0 ?do
	i over old-flag@ not if
	  dup i swap old-flag!
	then
      loop
      sector-block-count over old-count!
      0 swap free-count!
    ;

    \ Reorganize a sector
    : reorganize-sector ( -- )
      saved-sector @ erase-count@ 1+
      saved-sector @ reuse-sector
      find-reorganize-sector dup -1 <> averts x-block-write-fail
      dup copy-blocks-into-saved-sector
      dup mark-all-old
      saved-sector !
    ;

    \ Check whether blocks are available
    : blocks-available? ( -- flag )
      sector-count 0 ?do
	i saved-sector @ <> if
	  i new-count sector-block-count <> if
	    true unloop exit
	  then
	then
      loop
      false
    ;

    \ Reorganize a sector and write to a block
    : reorganize-block! ( addr id -- )
      >r reorganize-sector
      find-free-block dup -1 <> averts x-block-write-fail
      -1 over free-count+!
      2dup r> -rot block-id!
      block-addr data!
    ;
    
    \ Write to a block (the data written must be of size block-size)
    : block! ( addr id -- )
      blocks-available? averts x-block-write-fail
      dup >r find-block ?dup if
	block-index 1 over old-count+! old-flag!
	find-free-block dup -1 <> if
	  -1 over free-count+!
	  2dup r> -rot block-id!
	  block-addr data!
	else
	  2drop r> reorganize-block!
	then
      else
	find-free-block dup -1 <> if
	  -1 over free-count+!
	  2dup r> -rot block-id!
	  block-addr data!
	else
	  2drop r> reorganize-block!
	then
      then
    ;

    \ Get the free block count for a sector
    : discover-sector-free-count ( index -- )
      >r 0 begin dup sector-block-count < while
	dup cells r@ sector-addr sector-block-id-map-offset + + @ unwritten = if
	  1 r@ free-count+!
	then 1+
      repeat
      drop rdrop
    ;

    \ Get the free block count for all sectors
    : discover-free-count ( -- )
      sector-free-map sector-count 0 fill
      sector-count 0 ?do i discover-sector-free-count loop
    ;

    \ Get the old block count for a sector
    : discover-sector-old-count ( index -- )
      >r 0 begin dup sector-block-count < while
	dup cells r@ sector-addr sector-old-flag-map-offset + + @ unwritten <>
	if
	  1 r@ old-count+!
	then 1+
      repeat
      drop rdrop
    ;

    \ Get the old block count for all sectors
    : discover-old-count ( -- )
      sector-old-map sector-count 0 fill
      sector-count 0 ?do i discover-sector-old-count loop
    ;

    \ Unable to find saved sector
    : x-unable-to-find-saved-sector ( -- )
      space ." unable to find saved sector" cr
    ;
    
    \ Find a saved sector
    : find-saved-sector ( -- )
      0 saved-sector !
      find-saved-sector dup -1 <> averts x-unable-to-find-saved-sector
      saved-sector !
    ;
      
    \ Erase all blocks
    : erase-all-blocks ( -- )
      erase-qspi-bulk
      sector-count 0 ?do
	0 i old-count! sector-block-count i free-count!
      loop
    ;

    \ Truncate a string to its first invalid character
    : truncate-invalid ( b-addr u -- b-addr u )
      swap tuck swap
      begin
	dup 0> if
	  over c@
	  dup $20 >= swap $7F <> and if
	    1- swap 1+ swap false
	  else
	    true
	  then
	else
	  true
	then
      until
      drop over -
    ;

  end-module> import
  
  \ Initialize blocks
  : init-block ( -- )
    discover-free-count discover-old-count find-saved-sector
  ;

  \ Block not found exception
  : x-block-not-found ( -- ) ." block not found" cr ;

  \ Invalid block id exception
  : x-invalid-block-id ( -- ) ." invalid block id" cr ;

  \ Delete a block
  : delete-block ( id -- )
    dup unwritten <> averts x-invalid-block-id
    begin-critical
    find-block ?dup if
      block-index 1 over old-count+! qspi-base block-size + -rot old-flag!
      end-critical
    else
      end-critical
      ['] x-block-not-found ?raise
    then
  ;

  \ Get whether a block exists
  : block? ( id -- flag ) find-block 0<> ;

  \ Delete blocks
  : delete-blocks ( start-id count -- )
    over + swap ?do i block? if i delete-block then loop
  ;

  \ Find a block by id, or return 0 if no block can be found
  : find-block ( id -- addr | 0 )
    dup unwritten <> averts x-invalid-block-id
    begin-critical
    find-block
    end-critical
  ;

  \ Write to a block (the data written must be of size block-size),
  \ returns whether writing was successful
  : block! ( addr id -- success )
    begin-critical
    block!
    end-critical
  ;

  \ Erase all blocks
  : erase-all-blocks ( -- )
    begin-critical
    erase-all-blocks
    end-critical
  ;

  \ Block size
  : block-size ( -- bytes ) block-size ;

  \ Block not found
  : x-block-not-found ( -- ) ." block not found" cr ;

  \ Copy a single block
  : copy-block ( src-id dest-id -- )
    over block? if
      ram-here >r block-size ram-allot
      over find-block r@ block-size move
      r> over block!
      block-size negate ram-allot
    else
      dup block? if
	dup delete-block
      then
    then
    2drop
  ;

  continue-module block-internal
  
    \ Copy blocks from the start
    : copy-blocks-from-start ( src-id dest-id count -- )
      0 ?do 2dup copy-block 1+ swap 1+ swap loop 2drop
    ;
    
    \ Copy blocks from the end
    : copy-blocks-from-end ( src-id dest-id count -- )
      >r r@ + swap r@ + swap r> 0 ?do 1- swap 1- swap 2dup copy-block loop 2drop
    ;
    
    \ Find the address after a given number of empty blocks
    : find-empty-block-seq ( start-id empty-count -- end-id )
      begin dup 0<> while
	over block? not if
	  1-
	then
	swap 1+ swap
      repeat
      drop
    ;
    
    \ Get the total number of extant blocks in a range
    : get-extant-block-count ( start-id end-id -- count )
      0 rot rot swap ?do i block? if 1+ then loop
    ;
    
    \ Copy blocks without deleting blocks, from end to start
    : copy-blocks-no-delete-from-end ( end-id count -- )
      swap 1- dup rot begin dup 0<> while
	>r
	over block? if
	  2dup copy-block 1- swap 1- swap r> 1-
	else
	  swap 1- swap r>
	then
      repeat
      drop 2drop
    ;
    
  end-module
  
  \ Copy blocks
  : copy-blocks ( src-id dest-id count -- )
    -rot 2dup < if rot copy-blocks-from-end else rot copy-blocks-from-start then
  ;

  \ Insert blocks, pushing extant blocks to higher indices
  : insert-blocks ( start-id count -- )
    dup >r 2dup find-empty-block-seq nip
    2dup get-extant-block-count
    copy-blocks-no-delete-from-end
    r> delete-blocks
  ;
  
  continue-module block-internal

    \ Get the current block being interpreted
    : current-block ( -- addr ) eval-data @ @ find-block ;

    \ Get the current line offset being interpreted
    : current-line ( -- u ) eval-data @ cell+ @ ;

    \ Advance the line being interpreted
    : advance-line ( -- ) 64 eval-data @ cell+ +! ;

    \ Refill from a block
    : block-refill ( -- )
      advance-line
      current-block current-line + 64 truncate-invalid feed-input
    ;

    \ Get whether the current line is the end of a block
    : block-eof ( -- eof? )
      current-line [ 1024 64 - ] literal >=
    ;
    
  end-module
  
  \ Load a block
  : load ( id -- )
    dup block? averts x-block-not-found
    [:
      0 >r >r rp@
      dup @ find-block 64 truncate-invalid ['] block-refill ['] block-eof
      evaluate-with-input
      rdrop rdrop
    ;] try
    ?raise
  ;

  \ List a block
  : list ( id -- )
    dup block? averts x-block-not-found
    find-block
    16 0 ?do
      cr i 9 < if space then i 1+ . dup i 64 * + 64 truncate-invalid type
    loop
    cr drop
  ;

  \ Load a range of blocks, ignoring nonexistent blocks
  : load-range ( start-id end-id -- )
    1+ swap ?do
      i find-block dup if
	dup block-size + swap ?do
	  i 64 truncate-invalid evaluate
	64 +loop
      else
	drop
      then
    loop
  ;

  \ List a range of blocks, ignoring nonexistent blocks
  : list-range ( start-id end-id -- )
    1+ swap ?do
      i find-block dup if
	dup block-size + swap ?do
	  cr i 64 truncate-invalid type
	64 +loop
      else
	drop
      then
    loop
    cr
  ;

end-module> import

\ Initialize
: init ( -- )
  init
  init-block
;

\ Load a block
: load ( id -- ) load ;

\ List a block
: list ( id -- ) list ;

\ Load a range of blocks, ignoring nonexistent blocks
: load-range ( start-id end-id -- ) load-range ;

\ List a range of blocks, ignoring nonexistent blocks
: list-range ( start-id end-id -- ) list-range ;

\ Reboot
reboot
