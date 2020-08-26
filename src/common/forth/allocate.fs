\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

\ Compile this to flash
compile-to-flash

\ Set up the wordlist
forth-wordlist 1 set-order
forth-wordlist set-current
wordlist constant allocate-wordlist
wordlist constant allocate-internal-wordlist
forth-wordlist allocate-wordlist allocate-internal-wordlist 3 set-order
allocate-internal-wordlist set-current

\ The block header structure
begin-structure block-header-size
  \ The block flags (current just IN-USE)
  field: block-flags
  
  \ The size of the preceding block in memory, excluding the header, in
  \ multiples of 16 bytes
  hfield: prev-block-size

  \ The size of this block in memory, excluding the header, in multiples of
  \ 16 bytes
  hfield: block-size
  
  \ The preceding free block in its sized free list
  field: prev-sized-block

  \ The succeeding free block in its sized free list
  field: next-sized-block
end-structure

\ The heap structure
begin-structure heap-size
  \ The largest size in the array of sized free lists in multiples of 16 bytes
  field: high-block-size

  \ The log2 of the largest size in the array of sized free lists in multiples
  \ of 16 bytes
  field: high-block-size-log2
  
  \ The array of sized free lists
  field: sized-blocks
end-structure

\ Block is in use flag
1 constant in-use

\ Convert a size in bytes to a size in multiples of 16 bytes
: >size ( bytes -- 16-bytes )
  dup 16 and 0> if 4 rshift 1 + else 4 rshift then ;

\ Convert a size in multiples of 16 bytes to a size in bytes
: size> ( 16-bytes -- bytes ) 4 lshift ;

\ The block header size as a multiple of 16 bytes
block-header-size >size constant block-header-size-16

\ Get the log2 of a value
: log2 ( u -- u ) 0 begin over 1 > while 1+ swap 1 rshift swap repeat nip ;

\ Get the pow2 of a value
: pow2 ( u -- u ) 1 swap lshift ;

\ Get the log2 with ceiling of a value
: log2-ceiling ( u -- u ) dup dup log2 pow2 - 0> if log2 1+ else log2 then ;

\ Get the pow2 of the log2 with ceiling of a value
: ceiling2 ( u -- u ) log2-ceiling pow2 ;

\ Get the index into the array of sized free lists for a block size
: >index ( 16-bytes heap -- index )
  dup high-block-size @ 2 pick ceiling2 < if
    nip high-block-size-log2 @
  else
    drop log2-ceiling
  then ;

\ Get the index into the array of sized free lists for a block size
: >index-fill ( 16-bytes heap -- index )
  dup high-block-size @ 2 pick < if
    nip high-block-size-log2 @
  else
    drop log2
  then ;

\ Get the header of a block
: block-header ( addr -- block ) block-header-size - ;

\ Get the data of a block
: block-data ( block -- addr ) block-header-size + ;

\ Get the previous block
: prev-block ( block -- block )
  dup prev-block-size h@ 0 > if
    dup prev-block-size h@ size> - block-header-size -
  else
    drop 0
  then
;

\ Get the next block
: next-block ( block -- block ) dup block-size h@ size> + block-header-size + ;

\ Remove a block from its sized block list
: unlink-block ( block heap -- )
  swap dup next-sized-block @ if
    dup prev-sized-block @ over next-sized-block @ prev-sized-block !
  then
  dup prev-sized-block @ if
    dup next-sized-block @ swap prev-sized-block @ next-sized-block ! drop
  else
    dup block-size h@ 2 pick >index-fill cells rot sized-blocks @ +
    swap next-sized-block @ swap !
  then
;

\ Add a block to its sized block list
: link-block ( block heap -- )
  over 0 swap prev-sized-block !
  over block-size h@ over >index-fill cells swap sized-blocks @ +
  dup @ if
    2dup @ prev-sized-block !
    2dup @ swap next-sized-block !
  else
    over 0 swap next-sized-block !
  then
  !
;

\ Update the succeeding block previous block size
: update-prev-block-size ( block -- )
  dup block-size h@ dup size> rot block-data + prev-block-size h!
;

\ Merge with a preceding block
: merge-prev-block ( block heap -- block )
  swap dup prev-block ?dup if
    block-flags @ in-use and 0= if
      dup prev-block rot unlink-block
      dup block-size h@ block-header-size-16 +
      over prev-block block-size h@ + over prev-block block-size h!
      prev-block dup update-prev-block-size
    else
      nip
    then
  else
    nip
  then
;

\ Merge with a succeeding block
: merge-next-block ( block heap -- )
  swap dup next-block block-flags @ in-use and 0 = if
    dup next-block rot unlink-block
    dup dup next-block block-size h@ block-header-size-16 +
    over block-size h@ + swap block-size h!
    update-prev-block-size
  else
    2drop
  then
;

\ Find a suitable index for a new block
: find-index ( 16-bytes heap -- index )
  tuck >index begin
    dup 2 pick high-block-size-log2 @ <= if
      dup cells 2 pick sized-blocks @ + @ if
	nip true
      else
	1 + false
      then
    else
      2drop -1 true
    then
  until
;

\ Split a block
: split-block ( 16-bytes block heap -- ) 
  2dup unlink-block
  over block-data 3 pick size> +
  3 pick over prev-block-size h!
  2 pick block-size h@ 4 pick block-header-size-16 + - over block-size h!
  dup update-prev-block-size
  0 over block-flags !
  swap link-block
  block-size h!
;

\ Search for a block that fits, or return 0 if none fit
: search-blocks ( 16-bytes block -- block )
  begin
    dup 0 <> if
      2dup block-size h@ <= if nip true else next-sized-block @ false then
    else
      nip true
    then
  until ;

\ Allocate a block
: allocate-block ( allocate-size heap -- block )
  swap >size dup 2 pick find-index dup -1 <> if
    cells 2 pick sized-blocks @ + @ over swap search-blocks ?dup if     
      dup block-size h@ ( heap allocate-size block block-size )
      2 pick block-header-size-16 2 * + >= if
	2dup 4 pick split-block
      else
	dup 3 pick unlink-block
      then
      dup block-flags @ in-use or over block-flags !
      nip nip
    else
      2drop 0
    then
  else
    2drop drop 0
  then
;

\ Free a block
: free-block ( block heap -- )
  tuck merge-prev-block
  2dup swap merge-next-block
  dup block-flags @ in-use not and over block-flags !
  swap link-block
;

\ Resize a block by allocating a new block, copying data to it, and then freeing
\ the original block
: allocate-resize-block ( 16-bytes block heap -- new-block )
  2 pick size> over allocate-block ?dup if
    2 pick block-data over block-data 4 pick block-size h@ size> move
    rot rot free-block nip
  else
    2drop drop 0
  then
;

\ Resize a block
: resize-block ( allocate-size block heap -- )
  rot >size rot dup block-size h@ block-header-size-16 2 * - 2 pick >= if
    dup >r rot split-block r>
  else
    dup block-size h@ 2 pick < if
      dup next-block block-flags @ in-use and 0 = if
	dup next-block block-size h@ block-header-size-16 +
	over block-size h@ + 2 pick >= if
	  dup 3 pick merge-next-block
	  dup block-size h@ 2 pick block-header-size-16 2 * + >= if
	    dup >r rot split-block r>
	  else
	    nip nip
	  then
	else
	  rot allocate-resize-block
	then
      else
	rot allocate-resize-block
      then
    else
      nip nip
    then
  then
;

\ Create an initial block
: init-block ( heap-size -- block )
  >size ceiling2 4 align here over size> block-header-size 2 * + allot
  tuck block-size h!
  0 over prev-block-size h!
  0 over block-flags !
  0 over prev-sized-block !
  0 over next-sized-block !
  dup dup block-size h@ size> swap block-header-size + +
  0 over block-size h!
  over block-size h@ over prev-block-size h!
  in-use over block-flags !
  0 over prev-sized-block !
  0 swap next-sized-block !
;

allocate-wordlist set-current

\ Create an heap with a specified heap size and a specified largest size
\ in the array of sized free lists
: init-heap ( high-block-size -- heap )
  4 align here heap-size allot
  swap >size ceiling2 over high-block-size !
  dup high-block-size @ log2 over high-block-size-log2 !
  4 align here over high-block-size-log2 @ 1 + cells allot
  over sized-blocks !
  dup sized-blocks @ over high-block-size-log2 @ cells 0 fill
;

\ Define a variable for the shared heap
variable shared-heap

\ Initialize the shared heap
: init-shared-heap ( high-block-size -- ) init-heap shared-heap ! ;

\ No shared heap exists
: x-no-shared-heap ( -- ) space ." no shared heap exists" cr ;

\ Get the shared heap, if it exists
: get-heap ( -- heap ) shared-heap @ dup 0<> averts x-no-shared-heap ;

\ Allocate memory on a heap; returns -1 on success and 0 on failure
: allocate-with-heap ( bytes heap -- addr -1|0 )
  swap ?dup if
    swap allocate-block ?dup if block-data true else 0 false then
  else
    drop 0 true
  then
;

\ Free memory on a heap; returns -1 on success and 0 on failure
: free-with-heap ( addr heap -- -1|0 )
  swap ?dup if block-header swap free-block true else drop true then ;

\ Resize memory on a heap; returns -1 on success and 0 on failure
: resize-with-heap ( addr new-bytes heap -- addr -1|0 )
  >r swap ?dup if
    over if
      block-header r@ resize-block ?dup if
	block-data true
      else
	0 false
      then
    else
      nip r@ free-with-heap 0 swap
    then
  else
    r@ allocate-with-heap
  then
  r> drop
;

\ Switch over to FORTH-WORDLIST for public-facing words
forth-wordlist set-current

\ Allocate memory on the current heap; returns -1 on success and 0 on failure
: allocate ( bytes -- addr -1|0 ) get-heap allocate-with-heap ;

\ Free memory on the current heap; returns -1 on success and 0 on failure
: free ( addr -- -1|0 ) get-heap free-with-heap ;

\ Resize memory on the current heap; returns -1 on success and 0 on failure
: resize ( addr new-bytes -- addr -1|0 ) get-heap resize-with-heap ;

\ Memory management failure exception
: x-memory-management-failure ( -- )
  space ." failed to allocate/free memory" cr ;

\ Allocate memory in the heap, raising an exception if allocation fails.
: allocate! ( bytes -- addr ) allocate averts x-memory-management-failure ;

\ Resize memory in the heap, raising an exception if allocation fails.
: resize! ( addr new-bytes -- new-addr )
  resize averts x-memory-management-failure ;

\ Free memory in the heap, raising an exception if freeing fails.
: free! ( addr -- ) free averts x-memory-management-failure ;

\ Init
: init ( -- ) init 0 shared-heap ! ;

\ Warm reboot
warm