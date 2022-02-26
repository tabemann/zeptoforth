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

compress-flash

begin-module new-heap

  \ Debug heap
  true constant debug-heap

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

    \ Get the number of blocks for a size
    : size>blocks ( size heap -- blocks )
      2dup heap-block-size @ umod 0> if
	heap-block-size @ u/ 1+
      else
	heap-block-size @ u/
      then
    ;

    \ Get whether a block has been allocated
    : block-allocated? ( index heap -- allocated? )
      heap-bitmap over 5 rshift + swap $1F and bit swap bit@
    ;

    commit-flash

    \ Get the group size of a block
    : group-size@ ( group heap -- blocks ) heap-block group-size @ ;

    \ Set the group size of a block
    : group-size! ( blocks group heap -- )
      [ debug-heap ] [if]
	cr ." group-size! Index: " over . \ DEBUG
	." Address: " 2dup heap-block h.8 space \ DEBUG
	." Old size: " 2dup group-size@ . \ DEBUG
	." New size: " 2 pick . \ DEBUG
      [then]
      heap-block group-size !
    ;

    \ Set the next free group in the heap
    : heap-next-free! ( group heap -- )
      [ debug-heap ] [if]
	cr ." heap-next-free! Index: " over . \ DEBUG
      [then]
      heap-next-free !
    ;

    \ Get the previous free group index
    : group-prev-free@ ( group heap -- index ) heap-block group-prev-free @ ;

    \ Set the previous free group index
    : group-prev-free! ( index group heap -- )
      [ debug-heap ] [if]
	cr ." group-prev-free! Prev Index: " 2 pick . ." Index: " over . \ DEBUG
      [then]
      heap-block group-prev-free !
    ;

    \ Get the next free group index
    : group-next-free@ ( group heap -- index ) heap-block group-next-free @ ;

    \ Set the next free group index
    : group-next-free! ( index group heap -- )
      [ debug-heap ] [if]
	cr ." group-next-free! Next Index: " 2 pick . ." Index: " over . \ DEBUG
      [then]
      heap-block group-next-free !
    ;
    
    commit-flash
    
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
	nip
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
	nip
      then
    ;

    \ Get the lowest zero bit in a cell higher than a given position
    : test-low-zero ( u i -- i|-1 )
      [ debug-heap ] [if]
	cr ." test-low-zero: u: " over h.8 space ." i: " dup . \ DEBUG
      [then]
      over if
	over -1 <> if
	  tuck rshift swap
	  begin dup 32 < while
	    over 1 and 0= if
	      nip
	      [ debug-heap ] [if]
		." f: " dup . \ DEBUG
	      [then]
	      exit
	    then
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

    \ Get the value of the bitmap containing an index
    : bitmap-index@ ( index heap -- bitmap )
      heap-bitmap swap 5 rshift 2 lshift + @
    ;

    commit-flash

    \ Get the end of the previous free block group
    : find-prev-free-group-end ( index heap -- index|-1 )
      .s
      over 0> if
	over $1F and if
	  over >r swap $1F bic swap 2dup bitmap-index@ r> $1F and
	else
	  swap 32 - swap 2dup bitmap-index@ 32
	then
	begin
	  .s test-high-zero dup -1 <> if
	    nip swap $1F bic + .s exit
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
      over 0> if
	over $1F and if
	  over >r swap $1F bic swap 2dup bitmap-index@ r> $1F and
	else
	  swap 32 - swap 2dup bitmap-index@ 32
	then
	begin
	  test-high-nonzero dup -1 <> if
	    1+ nip swap $1F bic + exit
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

    \ \ Get the end of the previous free block group
    \ : find-prev-free-group-end ( index heap -- index|-1 )
    \   over 0> if
    \ 	over $1F and if
    \ 	  2dup heap-bitmap swap 5 rshift 2 lshift + @
    \ 	  2 pick $1F and test-high-zero dup -1 <> if
    \ 	    nip swap $1F bic or exit
    \ 	  else
    \ 	    drop swap $1F bic swap
    \ 	  then
    \ 	then
    \ 	begin over while
    \ 	  2dup heap-bitmap swap 1- 5 rshift 2 lshift + @
    \ 	  32 test-high-zero dup -1 <> if
    \ 	    nip swap 1- $1F bic or exit
    \ 	  else
    \ 	    drop swap 32 - swap
    \ 	  then
    \ 	repeat
    \ 	2drop -1
    \   else
    \ 	2drop -1
    \   then
    \ ;

    \ \ Get the start of a previous free block group
    \ : find-free-group-start ( index heap -- index )
    \   over -1 <> if
    \ 	over 0> if
    \ 	  over $1F and if
    \ 	    2dup heap-bitmap swap 5 rshift 2 lshift + @
    \ 	    2 pick $1F and test-high-nonzero dup -1 <> if
    \ 	      nip swap $1F bic or 1+ exit
    \ 	    else
    \ 	      drop swap $1F bic swap
    \ 	    then
    \ 	  then
    \ 	  begin over while
    \ 	    2dup heap-bitmap swap 1- 5 rshift 2 lshift + @
    \ 	    32 test-high-nonzero dup -1 <> if
    \ 	      nip swap 1- $1F bic or 1+ exit
    \ 	    else
    \ 	      drop swap 32 - swap
    \ 	    then
    \ 	  repeat
    \ 	  2drop 0
    \ 	else
    \ 	  2drop 0
    \ 	then
    \   else
    \ 	2drop -1
    \   then
    \ ;

    \ \ Get the start of the next free block group
    \ : find-next-free-group-start ( index heap -- index|-1 )
    \   2dup heap-block-count @ < if
    \ 	over $1F and if
    \ 	  2dup heap-bitmap swap 5 rshift 2 lshift + @
    \ 	  2 pick $1F and test-low-zero dup -1 <> if
    \ 	    nip swap $1F bic or exit
    \ 	  else
    \ 	    drop swap $1F bic 32 + swap
    \ 	  then
    \ 	then
    \ 	begin 2dup heap-block-count @ < while
    \ 	  2dup heap-bitmap swap 5 rshift 2 lshift + @
    \ 	  0 test-low-zero dup -1 <> if
    \ 	    nip or exit
    \ 	  else
    \ 	    drop swap 32 + swap
    \ 	  then
    \ 	repeat
    \ 	2drop -1
    \   else
    \ 	2drop -1
    \   then
    \ ;

    \ Link the next free block group
    : link-group-next ( index next-index heap -- )
      2 pick 2 pick 2 pick group-prev-free! ( index next-index heap )
      rot swap group-next-free! ( )
    ;

    \ Link the previous free block group
    : link-group-prev ( index prev-index heap -- )
      [ debug-heap ] [if]
	cr ." prev-index: " over . \ DEBUG
      [then]
      over -1 <> if
	tuck find-free-group-start ( index heap prev-start-index )
	2 pick over 3 pick group-next-free! ( index heap prev-start-index )
	-rot group-prev-free! ( )
      else
	nip 2dup heap-next-free! -1 -rot group-prev-free! ( )
      then
    ;

    commit-flash

    \ Link an adjacent next free block group
    : link-group-next-adjacent ( index next-index heap -- )
      2dup group-next-free@ ( index next-index heap next-next-index )
      swap >r 2 pick r@ group-next-free! ( index next-index )
      r@ group-size@ over r@ group-size@ + ( index size )
      swap r> group-size! ( )
    ;

    \ Link an adjacent previous free block group
    : link-group-prev-adjacent ( index prev-index heap -- )
      over -1 <> if
	dup >r find-free-group-start ( index prev-start-index )
	over r@ group-size@ ( index prev-start-index size )
	rot r@ group-next-free@ ( prev-start-index size next-index )
	-rot over r@ group-size@ ( next-index prev-start-index size prev-size )
	+ over r@ group-size! ( next-index prev-start-index )
	over -1 <> if
	  2dup swap r@ group-prev-free! ( next-index prev-start-index )
	then
	r> group-next-free! ( )
      else
	nip -1 -rot group-prev-free! ( )
      then
    ;

    \ Mark space as allocated
    : mark-allocated ( count index heap -- )
      heap-bitmap over 5 rshift cells + >r
      begin over while
      	32 over $1F and - 2 pick min
      	$FFFFFFFF over 32 swap - rshift 2 pick $1F and lshift

	[ debug-heap ] [if]
	  cr ." mark-allocated: " dup h.8 \ DEBUG
	[then]
	
	r@ bis! r> cell+ >r
      	tuck + -rot - swap
      repeat
      rdrop 2drop
    ;

    \ \ Mark space as allocaed
    \ : mark-allocated ( count index heap -- )
    \ heap-bitmap >r
    \ begin over while
    \ 	dup $1F and 1 swap lshift over 5 rshift 2 lshift r@ + bis!
    \ 	1+ swap 1- swap
    \ repeat
    \ rdrop 2drop
    \ ;
    
    \ Mark space as freed
    : mark-free ( count index heap -- )
      heap-bitmap over 5 rshift cells + >r
      begin over while
      	32 over $1F and - 2 pick min
      	$FFFFFFFF over 32 swap - rshift 2 pick $1F and lshift
      	r@ bic! r> cell+ >r
      	tuck + -rot - swap
      repeat
      rdrop 2drop
    ;

    \ \ Mark space as freed
    \ : mark-free ( count index heap -- )
    \ heap-bitmap >r
    \ begin over while
    \ 	dup $1F and 1 swap lshift over 5 rshift 2 lshift r@ + bic!
    \ 	1+ swap 1- swap
    \ repeat
    \ rdrop 2drop
    \ ;
    
    commit-flash

    \ Link a freed block group
    : link-group ( index heap -- )
      [ debug-heap ] [if]
	cr ." heap-next-free@: " dup heap-next-free @ . \ DEBUG
      [then]
      2dup find-prev-free-group-end ( index heap prev-end )
      [ debug-heap ] [if]
	cr ." Link group: Index: " 2 pick . \ DEBUG
	2 pick 2 pick  ." Address: " heap-block h.8 space \ DEBUG
	dup ." Prev index: " . \ DEBUG
	dup -1 <> if \ DEBUG
	  2dup ." Prev address: " swap heap-block h.8 space \ DEBUG
	then \ DEBUG
      [then]
      2 pick over 1+ = over -1 <> and if
	[ debug-heap ] [if]
	  ." Prev adjacent " \ DEBUG
	[then]
	2 pick 2 pick 2dup group-size@ rot +
	swap ( index heap prev-end end heap )
	find-next-free-group-start ( index heap prev-end next-start )
	2over group-size@ 4 pick + ( index heap prev-end next-start end )
	[ debug-heap ] [if]
	  over ." Next index: " . \ DEBUG
	  over -1 <> if \ DEBUG
	    over 4 pick ." Next address: " heap-block h.8 space \ DEBUG
	  then \ DEBUG
	[then]
	over = if ( index heap prev-end next-start )
	  [ debug-heap ] [if]
	    ." Next adjacent " \ DEBUG
	  [then]
	  2over rot swap link-group-next-adjacent ( index heap prev-end )
	  swap link-group-prev-adjacent ( )
	else ( index heap prev-end next-start )
	  [ debug-heap ] [if]
	    ." Next not adjacent " \ DEBUG
	  [then]
	  2over rot swap link-group-next ( index heap prev-end )
	  swap link-group-prev-adjacent ( )
	then
      else
	[ debug-heap ] [if]
	  ." Prev not adjacent " \ DEBUG
	[then]
	2 pick 2 pick 2dup group-size@ rot +
	swap ( index heap prev-end end heap )
	find-next-free-group-start ( index heap prev-end next-start )
	2over group-size@ 4 pick + ( index heap prev-end next-start end )
	[ debug-heap ] [if]
	  over ." Next index: " . \ DEBUG
	  over -1 <> if \ DEBUG
	    over 4 pick ." Next address: " heap-block h.8 space \ DEBUG
	  then \ DEBUG
	[then]
	over = if ( index heap prev-end next-start )
	  [ debug-heap ] [if]
	    ." Next adjacent " \ DEBUG
	  [then]
	  2over rot swap link-group-next-adjacent ( index heap prev-end )
	  swap link-group-prev ( )
	else
	  [ debug-heap ] [if]
	    ." Next not adjacent " \ DEBUG
	  [then]
	  2over rot swap link-group-next ( index heap prev-end )
	  swap link-group-prev ( )
	then
      then
    ;

    \ Check whether a group is expandable
    : expandable-group? ( size index heap -- flag )
      >r dup r@ group-size@ over + ( size index end )
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

    commit-flash

    \ Find a free group
    : find-free ( size heap -- )
      [ debug-heap ] [if]
	cr ." Finding free for size: " over . \ DEBUG
      [then]
      dup >r heap-next-free @
      begin dup -1 <> while
	[ debug-heap ] [if]
	  cr ." Finding free: Index: " dup . \ DEBUG
	  dup r@ heap-block ." Address: " h.8 space \ DEBUG
	  dup r@ group-next-free@ over = if \ DEBUG
	    cr ." LOOP!" [: ;] ?raise \ DEBUG
	  then \ DEBUG
	[then]
	dup r@ group-size@ 2 pick >= if
	  [ debug-heap ] [if]
	    cr ." Found free: Index: " dup . \ DEBUG
	    dup r@ heap-block ." Address: " h.8 space \ DEBUG
	    dup r@ group-size@ ." Size: " . space \ DEBUG
	  [then]
	  rdrop nip exit
	else
	  r@ group-next-free@
	then
      repeat
      rdrop nip
      [ debug-heap ] [if]
	cr ." Not found!" \ DEBUG
      [then]
    ;

    \ Allocate from a group
    : allocate-from-group ( size index heap -- )
      [ debug-heap ] [if]
	cr ." heap-next-free@: " dup heap-next-free @ . \ DEBUG
	cr ." Allocate from group: Index: " over . \ DEBUG
	2dup heap-block ." Address: " h.8 space \ DEBUG
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
	2dup + over r@ group-next-free@ r@ group-prev-free!
	dup r@ group-prev-free@ dup -1 <> if
	  [ debug-heap ] [if] .s [then]
	  >r 2dup + r> r@ group-next-free!
	else
	  drop 2dup + r@ heap-next-free!
	then 
	dup r@ group-prev-free@ 2 pick 2 pick + r@ group-prev-free!
	dup r@ group-next-free@ 2 pick 2 pick + r@ group-next-free!
	( size index )
	dup r@ group-size@ 2 pick - ( size index new-size )
	2 pick 2 pick + r@ group-size! ( size index )
	2dup r@ group-size! ( size index )
      then
      r> .s mark-allocated ( )
    ;

    \ Expand a group
    : expand-group ( size index heap -- )
      [ debug-heap ] [if]
	cr ." Expand group: Index: " over . \ DEBUG
	2dup heap-block ." Address: " h.8 space \ DEBUG
	." Size: " 2 pick . \ DEBUG
      [then]
      >r dup r@ group-size@ over + ( size index end )
      over r@ group-size@ 3 pick - ( size index end size-diff )
      over r@ group-size@ + ( size index end new-end-size )
      r> swap >r >r -rot tuck r@ group-size! ( end index )
      dup r@ group-size@ over r@ mark-allocated swap ( index end )
      over r@ group-prev-free@ ( index end prev-group )
      swap r@ group-next-free@ ( index prev-group next-group )
      rot dup r@ group-size@ + ( prev-group next-group new-end )
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
    ;

  end-module> import

  commit-flash

  \ Initialize a heap at a given address with a given block size and block count
  : init-heap ( block-size block-count addr -- )
    tuck swap 32 align swap heap-block-count !
    tuck swap 16 align swap heap-block-size !
    0 over heap-next-free!
    dup heap-block-count @ 0 2 pick group-size!
    -1 0 2 pick group-prev-free!
    -1 0 2 pick group-next-free!
    init-heap-bitmap
  ;

  \ Allocate from a heap
  : allocate ( bytes heap -- addr )
    [ debug-heap ] [if]
      cr over ." Allocating bytes: " . \ DEBUG
    [then]
    >r cell+ r@ size>blocks ( size )
    dup r@ find-free dup -1 <> averts x-allocate-failed ( size index )
    tuck r@ allocate-from-group ( index size index )
    r> heap-block cell+ ( addr )
  ;

  \ Free a group in a heap
  : free ( addr heap -- )
    >r cell - r@ block-addr>index ( index )
    dup r@ group-size@ ( index size )
    swap dup r@ link-group ( size index )
    r> mark-free ( )
  ;

  \ Resize a group in a heap
  : resize ( bytes addr heap -- new-addr )
    >r cell - r@ block-addr>index ( bytes index )
    swap cell+ r@ size>blocks swap ( size index )
    2dup r@ group-size@ > if
      2dup r@ expandable-group? if ( size index )
	tuck r@ expand-group r> heap-block cell+ ( new-addr )
      else
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

  \ Dump a heap's bitmap
  : diagnose-heap ( heap -- )
    cr ." DIAGNOSTIC: "
    cr ." Heap start: " 0 over heap-block h.8
\    dup heap-next-free @ begin dup -1 <> while
\      cr ." Free block: Index: " dup .
\      ." Address: " 2dup swap heap-block h.8 space
\      ." Size: " 2dup swap group-size@ .
\      2dup swap group-next-free@ over <> if \ DEBUG
\	over group-next-free@
\      else \ DEBUG
\	2drop -1 \ DEBUG
\      then \ DEBUG
\    repeat
    \    drop cr
    cr
    dup heap-block-count @ swap heap-bitmap swap 0 ?do ( bitmap )
      i 5 rshift cells over + @ i $1F and rshift 1 and if ." *" else ." ." then
    loop
    drop
  ;
      
    
  commit-flash

  \ Get the size of a heap with a given block size and block count
  : heap-size ( block-size block-count -- heap-bytes )
    swap 16 align over * swap 32 align 5 rshift cells + heap-size +
  ;

end-module

end-compress-flash