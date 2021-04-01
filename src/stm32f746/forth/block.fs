\ Copyright (c) 2021 Travis Bemann
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

begin-import-module-once block-module

  import internal-module
  import task-module
  import qspi-module

  \ Block size
  1024 constant block-size

  begin-import-module block-internal-module

    \ Sector size
    1024 64 * constant sector-size

    \ Sector count
    qspi-size sector-size / constant sector-count

    \ Block count within a sector
    sector-size block-size / 1 - constant sector-block-count

    \ Block count
    sector-block-count sector-count * constant block-count

    \ Block ID offset
    0 constant sector-block-id-map-offset

    \ Newer block pointer offset
    sector-block-count cells constant sector-newer-ptr-map-offset

    \ Block erase count offset
    sector-block-count 2 * cells constant sector-erase-count-offset
    
    \ Unwritten flash
    $FFFFFFFF constant unwritten
    
    \ Maximum saved block count
    8 constant max-saved-block-count

    \ Saved block data
    variable saved-block-data

    \ Saved block count
    : saved-block-count ( -- addr ) saved-block-data ;

    \ Saved block id array
    : saved-block-ids ( -- addr ) saved-block-data cell+ ;

    \ Saved block array
    : saved-blocks ( -- addr ) saved-block-ids saved-block-count @ cells + ;
    
    \ Allot a buffer storing the count of free blocks in sectors
    sector-count buffer: sector-free-map

    \ Allot a buffer storing the count of old blocks in sectors
    sector-count buffer: sector-old-map

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

    \ Get the new count for a sector
    : new-count ( index -- count )
      dup sector-free-map + b@ swap sector-old-map + b@ +
      sector-block-count swap -
    ;

    \ Find the next sector containing new sectors, or -1 if none can be found
    : next-new-sector ( index -- index|-1 )
      begin
	dup sector-count < if
	  dup new-count 0 = if 1+ false else true then
	else
	  drop -1 true
	then
      until
    ;

    \ Find a block in a sector, or return 0 if none is found
    : find-sector-block ( id sector-index -- addr|0 )
      sector-addr
      sector-block-count 0 ?do
	i cells over sector-block-id-map-offset + + @ 2 pick = if
	  i cells over sector-newer-ptr-map-offset + + @ unwritten = if
	    i 1 + block-size * + nip unloop exit
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
      dup sector-old-map + 0 swap b!
      dup sector-free-map + sector-block-count swap b!
      sector-addr dup erase-qspi-sector
      sector-erase-count-offset + qspi!
    ;

    \ Find a completely old sector, or return -1 if no sectors are completely
    \ old
    : find-old-sector ( -- erase-count index|-1 )
      $FFFFFFFF -1
      sector-count 0 ?do
	i sector-old-map + b@ sector-block-count = if
	  i sector-addr sector-erase-count-offset + @ 1+ >r over r> u> if
	    2drop i sector-addr sector-erase-count-offset + @ 1+ i
	  then
	then
      loop
    ;

    \ Find an almost old sector, conditional on the threshold
    : find-partial-sector ( -- erase-count index|-1 )
      $FFFFFFFF -1
      sector-count 0 ?do
	i new-count max-saved-block-count <= if
	  i sector-addr sector-erase-count-offset + @ 1+ >r over r> u> if
	    2drop i sector-addr sector-erase-count-offset + @ 1+ i
	  then
	then
      loop
    ;

    \ Find a sector with a free block, or return -1 if no sectors have free
    \ blocks
    : find-sector-with-free-block ( -- index|-1 )
      sector-count 0 ?do i sector-free-map + b@ 0> if i unloop exit then loop -1
    ;

    \ Find a free block within a sector, or return 0 if there are no free blocks
    \ within the sector
    : find-free-block-in-sector ( sector-index -- block-index|-1 )
      sector-addr sector-block-id-map-offset +
      sector-block-count 0 ?do
	i cells over + @ unwritten = if i nip unloop exit then
      loop
      -1
    ;

    \ This should never happen
    : x-should-never-happen ( -- ) space ." this should never happen!" cr ;

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

    \ Set a block as old
    : set-block-old ( block-index sector-index -- )
      sector-old-map + 1 swap b+! drop
    ;

    \ Set a block as new
    : set-block-new ( block-index sector-index -- )
      sector-free-map + -1 swap b+! drop
    ;

    \ Write a newer pointer for a block
    : newer-ptr! ( addr block-index sector-index -- )
      sector-addr sector-newer-ptr-map-offset + swap cells + qspi!
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
    \ assumes it will succeed
    : basic-block! ( addr id -- )
      find-free-block dup -1 <> if
	2dup set-block-new
	2dup r> rot rot block-id!
	block-addr data!
      then
    ;

    \ Reuse a partial sector
    : reuse-partial ( sector-index -- )
      ram-here saved-block-data !
      dup >r new-count dup saved-block-count !
      dup 1+ cells swap block-size * + ram-allot
      0 sector-block-count 0 ?do
	r@ sector-addr sector-newer-ptr-map-offset + i cells + @ $FFFFFFFF =
	r@ sector-addr sector-block-id-map-offset + i cells + @ $FFFFFFFF <> and
	if
	  r@ sector-addr sector-block-id-map-offset + i cells + @
	  over cells saved-block-ids + !
	  i r@ block-addr over cells saved-blocks + block-size move
	  1+
	then
      loop
      drop
      r@ sector-addr sector-erase-count-offset + @ 1+
      r@ reuse-sector
      saved-block-count 0 ?do
	i cells saved-blocks + i basic-block!
      loop
      rdrop
      saved-block-data@ ram-here!
    ;

    \ Write to a block (the data written must be of size block-size),
    \ returns whether writing was successful
    : block! ( addr id -- success )
      dup >r find-block ?dup if
	block-index 2dup set-block-old >r >r
	find-free-block dup -1 <> if
	  2dup set-block-new
	  2dup block-addr r> r> newer-ptr!
	  2dup r> rot rot block-id!
	  block-addr data! true
	else
	  rdrop rdrop rdrop 2drop false
	then
      else
	find-free-block dup -1 <> if
	  2dup set-block-new
	  2dup r> rot rot block-id!
	  block-addr data! true
	else
	  rdrop drop 2drop false
	then
      then
    ;

    \ Get the free block count for a sector
    : discover-sector-free-count ( index -- )
      >r 0 begin dup sector-block-count < while
	dup cells r@ sector-addr sector-block-id-map-offset + + @ unwritten = if
	  1 sector-free-map r@ + b+!
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
	dup cells r@ sector-addr sector-newer-ptr-map-offset + + @ unwritten <>
	if
	  1 sector-old-map r@ + b+!
	then 1+
      repeat
      drop rdrop
    ;

    \ Get the old block count for all sectors
    : discover-old-count ( -- )
      sector-old-map sector-count 0 fill
      sector-count 0 ?do i discover-sector-old-count loop
    ;

    \ Erase all blocks
    : erase-all-blocks ( -- )
      erase-qspi-bulk
      sector-count 0 ?do
	0 i sector-old-map + b! sector-block-count i sector-free-map + b!
      loop
    ;

    \ Truncate a string to its first invalid character
    : truncate-invalid ( b-addr u -- b-addr u )
      swap tuck swap
      begin
	dup 0> if
	  over b@
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

  end-module
  
  \ Initialize blocks
  : init-block ( -- )
    discover-free-count discover-old-count
  ;

  \ Block not found exception
  : x-block-not-found ( -- ) space ." block not found" cr ;

  \ Invalid block id exception
  : x-invalid-block-id ( -- ) space ." invalid block id" cr ;

  \ Delete a block
  : delete-block ( id -- )
    dup unwritten <> averts x-invalid-block-id
    begin-critical
    find-block ?dup if
      block-index 2dup set-block-old qspi-base block-size + -rot newer-ptr!
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

  \ Write failure exception
  : x-block-write-fail ( -- ) space ." unable to write block" cr ;

  \ Write to a block (the data written must be of size block-size),
  \ returns whether writing was successful
  : block! ( addr id -- success )
    dup unwritten <> averts x-invalid-block-id
    begin-critical
    block! averts x-block-write-fail
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
  : x-block-not-found ( -- ) space ." block not found" cr ;

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

  begin-module block-internal-module
  
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
  
  \ Load a block
  : load ( id -- )
    dup block? averts x-block-not-found
    find-block dup block-size + swap ?do
      i 64 truncate-invalid evaluate
    64 +loop
  ;

  \ List a block
  : list ( id -- )
    dup block? averts x-block-not-found
    find-block dup block-size + swap ?do
      cr i 64 truncate-invalid type
    64 +loop
    cr
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

end-module

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

unimport block-module

\ Warm reboot
warm
