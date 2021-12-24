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

\ Compile to flash
compile-to-flash

compress-flash

begin-module heap

  \ No blocks free exception
  : x-allocate-failed ( -- ) space ." allocate failed" cr ;
  
  begin-module heap-internal
    
    \ The heap structure
    begin-structure heap-size
      
      \ The heap block count
      field: heap-block-count
      
      \ The heap block size
      field: heap-block-size

      \ Next free index
      field: heap-next-free
      
    end-structure

    commit-flash
    
    \ Get the heap bitmap
    : heap-bitmap ( heap -- bitmap ) [inlined] heap-size + ;

    \ Get the heap blocks
    : heap-blocks ( heap -- blocks )
      [inlined] dup heap-size + swap heap-block-count @ 5 rshift cells +
    ;

    commit-flash

    \ Get a block in a heap
    : heap-block ( index heap -- block )
      [inlined] dup heap-block-size @ rot * swap heap-blocks +
    ;

    \ Get a block index in a heap by address
    : block-addr>index ( addr heap -- index )
      [inlined] tuck heap-blocks - swap heap-block-size @ u/
    ;

    \ Is a sequence of blocks fully within the heap
    : in-heap? ( count index heap -- in-heap? )
      -rot + swap heap-block-count @ <=
    ;

    commit-flash

    \ Get whether a sequences of blocks is free
    : blocks-free? ( count index heap -- free? )
      2 pick 2 pick 2 pick in-heap? if
	heap-bitmap
	over 5 rshift cells + dup >r rot >r
	@ over $1F and rshift
	32 r@ 32 min - $FFFFFFFF swap rshift and if
	  rdrop rdrop drop false exit
	then
	r> swap $1F and 32 swap - - 0 max r> cell+
	begin over while
	  over 32 min 32 swap - $FFFFFFFF swap rshift over @ and if
	    2drop false exit
	  else
	    cell+ swap 32 - 0 max swap
	  then
	repeat
	2drop true
      else
	2drop drop false
      then
    ;

    \ Skip used space in the heap
    : skip-used-space ( index heap -- index )
      dup >r heap-bitmap over 5 rshift cells +
      begin
	over r@ heap-block-count @ < if
	  dup @ 2 pick $1F and rshift $FFFFFFFF 3 pick $1F and rshift = if
	    cell+ swap 32 over $1F and - rot + swap
	  else
	    dup @ 2 pick $1F and rshift >r 32 2 pick $1F and -
	    begin dup 0> while
	      r@ 1 and not if rdrop rdrop 2drop exit then
	      1- r> 1 rshift >r rot 1+ -rot
	    repeat
	    drop rdrop
	  then
	else
	  rdrop 2drop -1 exit
	then
      again
    ;

    commit-flash

    \ Find the first free space of a given size in the heap
    : find-space ( count index heap -- index )
      >r
      begin
	r@ skip-used-space dup -1 = if
	  rdrop ['] x-allocate-failed ?raise
	then
	r@ 2 pick 2 pick 2>r blocks-free? if
	  2r> nip rdrop true
	else
	  2r> over + false
	then
      until
      nip
    ;

    \ Update next free index on allocate
    : update-next-free-on-allocate ( count index heap -- )
      2dup heap-next-free @ = if
	-rot + over skip-used-space dup -1 <> if
	  swap heap-next-free !
	else
	  drop dup heap-block-count @ swap heap-next-free !
	then
      else
	2drop drop
      then
    ;

    \ Update next free index on free
    : update-next-free-on-free ( index heap -- )
      dup heap-next-free @ rot min swap heap-next-free !
    ;
    
    commit-flash

    \ Mark space as allocated
    : mark-allocated ( count index heap -- )
      2 pick 2 pick 2 pick update-next-free-on-allocate
      heap-bitmap over 5 rshift cells + >r
      begin over while
	32 over $1F and - 2 pick min
	$FFFFFFFF over 32 swap - rshift 2 pick $1F and lshift
	r@ bis! r> cell+ >r
	tuck + -rot - swap
      repeat
      rdrop 2drop
    ;

    \ Mark space as freed
    : mark-free ( count index heap -- )
      2dup update-next-free-on-free
      heap-bitmap over 5 rshift cells + >r
      begin over while
	32 over $1F and - 2 pick min
	$FFFFFFFF over 32 swap - rshift 2 pick $1F and lshift
	r@ bic! r> cell+ >r
	tuck + -rot - swap
      repeat
      rdrop 2drop
    ;

    \ Get the number of blocks for a size
    : size>blocks ( size heap -- blocks )
      2dup heap-block-size @ umod 0> if
	heap-block-size @ u/ 1+
      else
	heap-block-size @ u/
      then
    ;

    \ Initialize a heap's bitmap
    : init-heap-bitmap ( heap -- )
      dup heap-bitmap swap heap-block-count @ 3 rshift 0 fill
    ;

  end-module> import

  commit-flash
  
  \ Initialize a heap at a given address with a given block size and block count
  : init-heap ( block-size block-count addr -- )
    tuck swap 32 align swap heap-block-count !
    tuck heap-block-size !
    0 over heap-next-free !
    init-heap-bitmap
  ;

  \ Allocate space on a heap
  : allocate ( size heap -- addr )
    tuck swap cell+ swap size>blocks >r
    dup dup heap-next-free @ swap tuck r@ -rot find-space
    r@ over 3 pick mark-allocated
    swap heap-block
    dup r> swap ! cell+
  ;

  \ Free space on a heap
  : free ( addr heap -- )
    over 4 - @ over >r -rot block-addr>index r> mark-free
  ;

  commit-flash

  \ Resize space on a heap
  : resize ( size addr heap -- new-addr )
    2dup free
    2dup block-addr>index
    3 pick cell+ 2 pick size>blocks >r
    2dup swap r@ -rot blocks-free? if
      r@ 3 pick 4 - ! r> -rot swap mark-allocated nip
    else
      drop [: 2 pick over allocate ;] try
      dup ['] x-allocate-failed = if
	rdrop drop over 4 - @ rot 2 pick block-addr>index rot mark-allocated
	['] x-allocate-failed ?raise
      else
	?raise
      then
      rot 4 - rot over @ swap heap-block-size @ * rot dup >r 4 - swap move
      r> r> over 4 - ! nip
    then
  ;
  
  \ Get the size of a heap with a given block size and block count
  : heap-size ( block-size block-count -- heap-bytes )
    tuck * swap 32 align 5 rshift cells + heap-size +
  ;

end-module

end-compress-flash

\ Reboot
reboot
