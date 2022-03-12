\ Copyright (c) 2021-2022 Travis Bemann
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

compress-flash

begin-module heap

  \ Debug heap
  false constant debug-heap?

  \ Verify that memory is allocated
  false constant verify-allocated?

  \ No blocks free exception
  : x-allocate-failed ( -- ) ." allocate failed" cr ;

  \ Internal error
  : x-internal-error ( -- ) ." heap internal error" cr ;

  \ Memory is not allocated
  : x-memory-not-allocated ( -- ) ." memory not allocated" cr ;

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

    \ The free block header structure
    begin-structure free-header-size

      \ The free group size in blocks
      field: group-size

      \ The previous free group index
      field: group-prev-free

      \ The next free block index
      field: group-next-free

    end-structure
    
    commit-flash

    \ Get the heap bitmap
    : heap-bitmap ( heap -- bitmap ) [inlined] heap-size + ;

    \ Get a heap bitmap cell
    : heap-bitmap-cell ( index heap -- addr )
      [inlined] heap-bitmap swap 5 rshift cells +
    ;
    
    \ Get the heap blocks
    : heap-blocks ( heap -- blocks )
      [inlined] dup heap-block-count @ swap heap-bitmap-cell
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

    \ Get the number of blocks for a size
    : size>blocks ( size heap -- blocks )
      2dup heap-block-size @ umod 0> if
	heap-block-size @ u/ 1+
      else
	heap-block-size @ u/
      then
    ;

    \ Get the value of the bitmap containing an index
    : bitmap-index@ ( index heap -- bitmap )
      heap-bitmap swap 5 rshift 2 lshift + @
    ;

    commit-flash

    \ Get whether a block has been allocated
    : block-allocated? ( index heap -- allocated? )
      swap tuck swap bitmap-index@ swap $1F and bit and 0<>
    ;

    commit-flash

    \ Get the group size of a block
    : group-size@ ( group heap -- blocks ) heap-block group-size @ ;

    \ Set the group size of a block
    : group-size! ( blocks group heap -- )
      [ debug-heap? ] [if]
	cr
	2 pick 900 > if ." ****** " then \ DEBUG
	." group-size! Index: " over . \ DEBUG
	." Old size: " 2dup group-size@ . \ DEBUG
	." New size: " 2 pick . \ DEBUG
      [then]
      2 pick 2 pick + over heap-block-count @ > triggers x-internal-error
      heap-block group-size !
    ;

    \ Set the next free group in the heap
    : heap-next-free! ( group heap -- )
      [ debug-heap? ] [if]
	cr ." heap-next-free! Index: " over . \ DEBUG
      [then]
      heap-next-free !
    ;

    \ Get the previous free group index
    : group-prev-free@ ( group heap -- index ) heap-block group-prev-free @ ;

    \ Set the previous free group index
    : group-prev-free! ( index group heap -- )
      [ debug-heap? ] [if]
	cr ." group-prev-free! Prev Index: " 2 pick . ." Index: " over . \ DEBUG
      [then]
      heap-block group-prev-free !
    ;

    \ Get the next free group index
    : group-next-free@ ( group heap -- index ) heap-block group-next-free @ ;

    \ Set the next free group index
    : group-next-free! ( index group heap -- )
      [ debug-heap? ] [if]
	cr ." group-next-free! Next Index: " 2 pick . ." Index: " over . \ DEBUG
      [then]
      heap-block group-next-free !
    ;
    
    commit-flash

    \ Get the end of a group
    : group-end@ ( group heap -- index ) swap tuck swap group-size@ + ;
    
    \ Get the highest zero bit in a cell lower than a given position
    : test-high-zero ( u i -- i|-1 )
      over if
	over -1 <> if
	  tuck 32 swap - lshift swap
	  begin ?dup while
	    over 31 rshift 0= if nip 1- exit then
	    1- swap 1 lshift swap
	  repeat
	  drop -1
	else
	  2drop -1
	then
      else
	nip 1-
      then
    ;

    \ Get the highest nonzero bit in a cell lower than a given position
    : test-high-nonzero ( u i -- i|-1 )
      over -1 <> if
	over if
	  tuck 32 swap - lshift swap
	  begin ?dup while
	    over 31 rshift if nip 1- exit then
	    1- swap 1 lshift swap
	  repeat
	  drop -1
	else
	  2drop -1
	then
      else
	nip 1-
      then
    ;

    \ Get the lowest zero bit in a cell higher than a given position
    : test-low-zero ( u i -- i|-1 )
      over if
	over -1 <> if
	  tuck rshift swap
	  begin dup 32 < while
	    over 1 and 0= if nip exit then
	    1+ swap 1 rshift swap
	  repeat
	  2drop -1
	else
	  2drop -1
	then
      else
	nip
      then
    ;

    commit-flash

    \ Get the end of the previous free block group
    : find-prev-free-group-end ( index heap -- index|-1 )
      over 0> if
	over $1F and if
	  over >r swap $1F bic swap 2dup bitmap-index@ r> $1F and
	else
	  swap 32 - swap 2dup bitmap-index@ 32
	then
	begin
	  test-high-zero dup -1 <> if
	    nip swap $1F bic + exit
	  then
	  drop over 0> if
	    swap 32 - swap 2dup bitmap-index@ 32 false
	  else
	    true
	  then
	until
	2drop -1
      else
	2drop -1
      then
    ;

    \ Get the start of a previous free block group
    : find-free-group-start ( index heap -- index )
      [ debug-heap? ] [if]
	cr ." find-free-group-start: End index: " over . \ DEBUG
      [then]
      over 0> if
	over $1F and if
	  over >r swap $1F bic swap 2dup bitmap-index@ r> $1F and
	else
	  swap 32 - swap 2dup bitmap-index@ 32
	then
	begin
	  [ debug-heap? ] [if]
	    ." Bitmap cell " over h.8 space ." Bitmap cell index: " dup . \ DEBUG
	  [then]
	  test-high-nonzero dup -1 <> if
	    nip swap $1F bic + 1+
	    [ debug-heap? ] [if]
	      ." Start index: " dup . \ DEBUG
	    [then]
	    exit
	  then
	  drop over 0> if
	    swap 32 - swap 2dup bitmap-index@ 32 false
	  else
	    true
	  then
	until
	2drop 0
      else
	2drop 0
      then
      [ debug-heap? ] [if]
	." Start index: " dup . \ DEBUG
      [then]
    ;

    \ Get the start of the next free block group
    : find-next-free-group-start ( index heap -- index|-1 )
      2dup heap-block-count @ < if
	over $1F bic over bitmap-index@
	begin
	  2 pick $1F and test-low-zero dup -1 <> if
	    nip swap $1F bic + exit
	  then
	  drop swap $1F bic 32 + swap 2dup heap-block-count @ < if
	    2dup bitmap-index@ false
	  else
	    true
	  then
	until
	2drop -1
      else
	2drop -1
      then
    ;

    \ Link the next free block group
    : link-group-next ( index next-index heap -- )
      >r
      dup -1 <> if
	2dup r@ group-prev-free! ( index next-index )
      then
      swap r> group-next-free! ( )
    ;

    \ Link the previous free block group
    : link-group-prev ( index prev-index heap -- )
      [ debug-heap? ] [if]
	cr ." prev-index: " over . \ DEBUG
      [then]
      >r
      dup -1 <> if
	r@ find-free-group-start ( index prev-start-index )
	2dup r@ group-next-free! ( index prev-start-index )
	swap r> group-prev-free! ( )
      else
	drop dup r@ heap-next-free! -1 swap r> group-prev-free! ( )
      then
    ;

    commit-flash

    \ Link an adjacent next free block group
    : link-group-next-adjacent ( index next-index heap -- )
      >r
      dup r@ group-next-free@ ( index next-index next-next-index )
      dup -1 <> if
	2 pick over r@ group-prev-free! ( index next-index next-next-index )
      then
      2 pick r@ group-next-free! ( index next-index )
      r@ group-size@ over r@ group-size@ + ( index size )
      swap r> group-size! ( )
    ;

    \ Link an adjacent previous free block group
    : link-group-prev-adjacent ( index prev-index heap -- )
      >r
      dup -1 <> if
	r@ find-free-group-start ( index prev-start-index )
	over r@ group-size@ ( index prev-start-index size )
	rot r@ group-next-free@ ( prev-start-index size next-index )
	-rot over r@ group-size@ ( next-index prev-start-index size prev-size )
	+ over r@ group-size! ( next-index prev-start-index )
	over -1 <> if
	  2dup swap r@ group-prev-free! ( next-index prev-start-index )
	then
	r> group-next-free! ( )
      else
	drop -1 swap r> group-prev-free! ( )
      then
    ;

    \ Mark space as allocated
    : mark-allocated ( count index heap -- )
      swap tuck swap heap-bitmap-cell >r
      begin over while
      	32 over $1F and - 2 pick min
      	$FFFFFFFF over 32 swap - rshift 2 pick $1F and lshift

	[ debug-heap? ] [if]
	  cr ." mark-allocated: " dup h.8 \ DEBUG
	[then]
	
	r@ bis! r> cell+ >r
      	tuck + -rot - swap
      repeat
      rdrop 2drop
    ;

    \ Mark space as freed
    : mark-free ( count index heap -- )
      swap tuck swap heap-bitmap-cell >r
      begin over while
      	32 over $1F and - 2 pick min
      	$FFFFFFFF over 32 swap - rshift 2 pick $1F and lshift
      	r@ bic! r> cell+ >r
      	tuck + -rot - swap
      repeat
      rdrop 2drop
    ;

    commit-flash

    \ Link a freed block group
    : link-group ( index heap -- )
      >r
      [ debug-heap? ] [if]
	cr ." heap-next-free@: " r@ heap-next-free @ . \ DEBUG
      [then]
      dup r@ find-prev-free-group-end ( index prev-end )
      [ debug-heap? ] [if]
	cr ." Link group: Index: " over . \ DEBUG
	dup ." Prev index: " . \ DEBUG
      [then]
      2dup 1+ = over -1 <> and if
	[ debug-heap? ] [if] ." Prev adjacent " [then] \ DEBUG
	over r@ group-end@ ( index prev-end end )
	r@ find-next-free-group-start ( index prev-end next-start )
	2 pick r@ group-end@ ( index prev-end next-start end )
	[ debug-heap? ] [if] over ." Next index: " . [then] \ DEBUG
	over = if ( index prev-end next-start )
	  [ debug-heap? ] [if] ." Next adjacent " [then] \ DEBUG
	  2 pick swap r@ link-group-next-adjacent ( index prev-end )
	else ( index prev-end next-start )
	  [ debug-heap? ] [if] ." Next not adjacent " [then] \ DEBUG
	  2 pick swap r@ link-group-next ( index prev-end )
	then
	r> link-group-prev-adjacent ( )
      else
	[ debug-heap? ] [if] ." Prev not adjacent " [then] \ DEBUG
	over r@ group-end@ ( index prev-end end )
	r@ find-next-free-group-start ( index prev-end next-start )
	2 pick r@ group-end@ ( index prev-end next-start end )
	[ debug-heap? ] [if] over ." Next index: " . [then] \ DEBUG
	over = if ( index prev-end next-start )
	  [ debug-heap? ] [if] ." Next adjacent " [then] \ DEBUG
	  2 pick swap r@ link-group-next-adjacent ( index prev-end )
	else
	  [ debug-heap? ] [if] ." Next not adjacent " [then] \ DEBUG
	  2 pick swap r@ link-group-next ( index prev-end )
	then
	r> link-group-prev ( )
      then
    ;

    \ Check whether a group is expandable
    : expandable-group? ( size index heap -- flag )
      >r dup r@ group-end@ ( size index end )
      dup r@ find-next-free-group-start over = if ( size index end )
	r@ group-size@ swap r> group-size@ + <= ( flag )
      else
	rdrop 2drop drop false ( flag )
      then
    ;
    
    \ Initialize a heap's bitmap
    : init-heap-bitmap ( heap -- )
      dup heap-bitmap swap heap-block-count @ 3 rshift 0 fill
    ;

    \ Find the last block in a heap
    debug-heap? [if]
      : find-last-block ( heap -- index )
	>r r@ heap-next-free @ begin
	  dup r@ group-size@ 2dup + r@ heap-block-count @ >= if
	    2dup + r@ heap-block-count @ = if
	      over r@ group-next-free@ -1 = if
		drop true
	      else
		cr ." Unexpected lack of end: Index: " swap .
		." Size: " .
		['] x-internal-error ?raise
	      then
	    else
	      cr ." Oversized block: Index: " over . ." Size: " .
	      ." Next index: " r@ group-next-free@ .
	      ['] x-internal-error ?raise
	    then
	  else
	    drop dup r@ group-next-free@ dup -1 <> if
	      nip false
	    else
	      drop cr ." Unexpected end: Index: " .
	      ['] x-internal-error ?raise
	    then
	  then
	until
	rdrop
      ;
    [then]
    
    commit-flash

    \ Find a free group
    : find-free ( size heap -- )
      [ debug-heap? ] [if]
	cr ." Finding free for size: " over . \ DEBUG
      [then]
      dup >r heap-next-free @
      begin dup -1 <> while
	[ debug-heap? ] [if]
	  cr ." Finding free: Index: " dup . \ DEBUG
	  dup r@ group-next-free@ over = if \ DEBUG
	    cr ." LOOP!" [: ;] ?raise \ DEBUG
	  then \ DEBUG
	[then]
	dup r@ group-size@ 2 pick >= if
	  [ debug-heap? ] [if]
	    cr ." Found free: Index: " dup . \ DEBUG
	    dup r@ group-size@ ." Size: " . space \ DEBUG
	  [then]
	  rdrop nip exit
	else
	  r@ group-next-free@
	then
      repeat
      rdrop nip
      [ debug-heap? ] [if]
	cr ." Not found!" \ DEBUG
      [then]
    ;

    \ Allocate from a group
    : allocate-from-group ( size index heap -- )
      [ debug-heap? ] [if]
	cr ." heap-next-free@: " dup heap-next-free @ . \ DEBUG
	cr ." Allocate from group: Index: " over . \ DEBUG
	." Size: " 2 pick . \ DEBUG
	." Group size: " 2dup group-size@ . \ DEBUG
      [then]
      >r dup r@ group-size@ 2 pick = if
	dup r@ group-prev-free@ dup -1 <> if
	  over r@ group-next-free@ swap r@ group-next-free!
	else
	  drop dup r@ group-next-free@ r@ heap-next-free!
	then
	dup r@ group-next-free@ dup -1 <> if
	  over r@ group-prev-free@ swap r@ group-prev-free!
	else
	  drop
	then
	( size index )
      else
	dup r@ group-prev-free@ dup -1 <> if
	  >r 2dup + r> r@ group-next-free!
	else
	  drop 2dup + r@ heap-next-free!
	then 
	dup r@ group-next-free@ dup -1 <> if
	  >r 2dup + r> r@ group-prev-free!
	else
	  drop
	then
	dup r@ group-prev-free@ 2 pick 2 pick + r@ group-prev-free!
	dup r@ group-next-free@ 2 pick 2 pick + r@ group-next-free!
	( size index )
	dup r@ group-size@ 2 pick - ( size index new-size )
	2 pick 2 pick + r@ group-size! ( size index )
	2dup r@ group-size! ( size index )
      then
      r> mark-allocated ( )
    ;

    \ Expand a group
    : expand-group ( size index heap -- )
      [ debug-heap? ] [if]
	cr ." Expand group: Index: " over . \ DEBUG
	." Size: " 2 pick . \ DEBUG
      [then]
      >r dup r@ group-end@ ( size index end )
      over r@ group-size@ 3 pick - ( size index end size-diff )
      over r@ group-size@ + ( size index end new-end-size )
      dup 0<> if
	r> swap >r >r -rot tuck r@ group-size! ( end index )
	dup r@ group-size@ over r@ mark-allocated swap ( index end )
	dup r@ group-prev-free@ ( index end prev-group )
	swap r@ group-next-free@ ( index prev-group next-group )
	rot r@ group-end@ ( prev-group next-group new-end )
	r> r> swap >r over r@ group-size! ( prev-group next-group new-end )
	2dup r@ group-next-free! ( prev-group next-group new-end )
	over -1 <> if
	  tuck swap r@ group-prev-free! ( prev-group new-end )
	else
	  nip ( prev-group new-end )
	then
	2dup r@ group-prev-free! ( prev-group new-end )
	over -1 <> if
	  swap r> group-next-free! ( )
	else
	  nip r> heap-next-free! ( )
	then
      else
	drop -rot tuck r@ group-size! ( end index )
	dup r@ group-size@ swap r@ mark-allocated ( end )
	dup r@ group-prev-free@ ( end prev-group )
	swap r@ group-next-free@ ( prev-group next-group )
	dup -1 <> if
	  2dup r@ group-prev-free! ( prev-group next-group )
	then
	over -1 <> if
	  swap r> group-next-free! ( )
	else
	  nip r> heap-next-free! ( )
	then
      then
    ;

    \ Verify that memory is allocated
    : is-allocated? ( index size heap -- allocated? )
      >r swap tuck + begin 2dup < while
	1- dup r@ block-allocated? not if rdrop 2drop false exit then
      repeat
      rdrop 2drop true
    ;

    \ Verify that the first free block is actually the first free block
    : verify-first-block ( heap -- )
      [ debug-heap? ] [if]
	>r r@ heap-next-free @ r@ group-prev-free@ -1 <> if
	  cr ." Supposed first free block not first: Index: "
	  r@ heap-next-free @ . ." Prev index: "
	  r@ heap-next-free @ r@ group-prev-free@ .
	  ['] x-internal-error ?raise
	then
	r@ heap-next-free @ 1- r@ find-prev-free-group-end dup -1 <> if
	  cr ." Supposed first free block not first: Index: "
	  r@ heap-next-free @ . ." Prev found index: "
	  r@ find-free-group-start .
	  ['] x-internal-error ?raise
	else
	  drop
	then
	rdrop
      [else]
	drop
      [then]
    ;

    \ Verify that no memory is allocated after the last free block
    : verify-last-block ( heap -- )
      [ debug-heap? ] [if]
	>r r@ heap-block-count @ r@ find-last-block
	dup -1 <> if
	  cr ." Last block: " dup .
	  begin 2dup > while
	    dup r@ block-allocated? if
	      cr ." Unexpected allocated block: Index: " dup .
	      ['] x-internal-error ?raise
	    else
	      1+
	    then
	  repeat
	  2drop
	else
	  drop
	then
	rdrop
      [else]
	drop
      [then]
    ;
    
  end-module> import

  \ Dump a heap's bitmap
  : diagnose-heap ( heap -- )
    cr ." DIAGNOSTIC: "
    cr ." Heap start: " 0 over heap-block h.8
\    dup heap-next-free @ begin dup -1 <> while
\      cr ." Free block: Index: " dup .
\      ." Size: " 2dup swap group-size@ .
\      2dup swap group-next-free@ over <> if \ DEBUG
\	over group-next-free@
\      else \ DEBUG
\	2drop -1 \ DEBUG
\      then \ DEBUG
\    repeat
    \    drop cr
    cr
    dup heap-block-count @ 0 ?do
      i over block-allocated? if ." *" else ." ." then
    loop
    drop
  ;

  commit-flash

  \ Initialize a heap at a given address with a given block size and block count
  : init-heap ( block-size block-count addr -- )
    tuck swap 32 align swap heap-block-count !
    tuck swap cell align 3 cells max swap heap-block-size !
    0 over heap-next-free!
    dup heap-block-count @ 0 2 pick group-size!
    -1 0 2 pick group-prev-free!
    -1 0 2 pick group-next-free!
    init-heap-bitmap
  ;

  \ Allocate from a heap
  : allocate ( bytes heap -- addr )
    [ debug-heap? ] [if]
      cr over ." Allocating bytes: " . \ DEBUG
    [then]
    >r cell+ r@ size>blocks ( size )
    dup r@ find-free dup -1 <> averts x-allocate-failed ( size index )
    tuck r@ allocate-from-group ( index size index )
    r@ heap-block cell+ ( addr )
    r@ verify-first-block
    r> verify-last-block
  ;

  \ Free a group in a heap
  : free ( addr heap -- )
    >r cell - r@ block-addr>index ( index )
    dup r@ group-size@ ( index size )
    [ verify-allocated? ] [if]
      2dup r@ is-allocated? not if
	[ debug-heap? ] [if]
	  cr ." Memory not allocated: Index: " over . ." Size: " dup .
	[then]
	['] x-memory-not-allocated ?raise
      then
    [then]
    swap dup r@ link-group ( size index )
    r@ mark-free ( )
    r@ verify-first-block
    r> verify-last-block
  ;

  commit-flash
  
  \ Resize a group in a heap
  : resize ( bytes addr heap -- new-addr )
    >r cell - r@ block-addr>index ( bytes index )
    swap cell+ r@ size>blocks swap ( size index )
    [ debug-heap? ] [if]
      cr ." Resizing block: Index: " dup . ." Size: " over . \ DEBUG
    [then]
    2dup r@ group-size@ > if
      2dup r@ expandable-group? if ( size index )
	[ debug-heap? ] [if]
	  cr ." Expanding block" \ DEBUG
	[then]
	tuck r@ expand-group r> heap-block cell+ ( new-addr )
      else
	[ debug-heap? ] [if]
	  cr ." Moving block" \ DEBUG
	[then]
	swap r@ heap-block-size @ * cell - r@ allocate ( index new-addr )
	over r@ heap-block cell+ swap ( index addr new-addr )
	2 pick r@ group-size@ r@ heap-block-size @ * cell -
	( index addr new-addr bytes )
	over >r move r> ( index new-addr )
	swap r@ heap-block cell+ r> free ( new-addr )
      then
    else ( size index )
      nip r> heap-block cell+ ( same-addr )
    then
  ;      
    
  \ Get the size of a heap with a given block size and block count
  : heap-size ( block-size block-count -- heap-bytes )
    swap cell align 3 cells max over * swap 32 align 5 rshift cells +
    heap-size +
  ;

end-module

end-compress-flash

reboot