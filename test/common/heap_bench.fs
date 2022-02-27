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

begin-module heap-bench

  systick import
  tinymt32 import
  
  begin-module heap-bench-internal

    \ Space division factor
    32 constant space-div-factor

    begin-structure bench-head-size

      \ The starting seed
      field: bench-seed
      
      \ The pseudorandom number generator
      tinymt32-size +field bench-prng

      \ The heap allocate xt
      field: bench-allocate

      \ The heap free xt
      field: bench-free

      \ The heap resize xt
      field: bench-resize

      \ The heap diagnostic xt
      field: bench-diagnose

      \ The heap init xt
      field: bench-init-heap
      
      \ The heap
      field: bench-heap

      \ The heap limit
      field: bench-heap-size-limit

      \ The heap count limit
      field: bench-heap-count-limit

      \ The heap currently-allocated size
      field: bench-heap-current-size

      \ The heap current block count
      field: bench-heap-current-count
      
    end-structure
    
    begin-structure bench-block-size

      \ The block address
      field: block-addr

      \ The block size
      field: block-size
      
    end-structure

    \ Get a block's address
    : bench-block ( index bench -- addr )
      swap bench-block-size * swap + bench-head-size +
    ;

    \ Get a block
    : allocate-block ( addr size bench -- )
      >r
      r@ bench-heap-current-count @ r@ bench-block
      tuck block-size !
      block-addr !
      1 r> bench-heap-current-count +!
    ;

    \ Free a block
    : free-block ( index bench -- )
      >r
      dup 1+ r@ bench-block
      dup bench-block-size -
      rot 1+ r@ bench-heap-current-count @ swap - bench-block-size * move
      -1 r> bench-heap-current-count +!
    ;

    \ Do benchmark allocation
    : bench-do-allocate ( bench -- done )
      >r
\      r@ bench-heap-current-size @ space .
      r@ bench-heap-count-limit @ r@ bench-heap-current-count @ > if
	r@ bench-heap-size-limit @ r@ bench-heap-current-size @ -
	dup 3 cells >= if
	  space-div-factor / 3 cells max
	  r@ bench-prng tinymt32-generate-uint32 swap umod 3 cells max
	  16 align cell -
	  dup r@ bench-heap-current-size +!
	  dup r@ bench-heap @ r@ bench-allocate @ execute
	  swap r> allocate-block
	  true
	else
	  drop rdrop false
	then
      else
	rdrop false
      then
    ;

    \ Do benchmark freeing
    : bench-do-free ( bench -- done )
      >r
      r@ bench-heap-current-count @ 0> if
	r@ bench-prng tinymt32-generate-uint32
	r@ bench-heap-current-count @ umod
	dup r@ bench-block
	dup block-size @ negate r@ bench-heap-current-size +!
	block-addr @
	r@ bench-heap @ r@ bench-free @ execute
	r> free-block
	true
      else
	rdrop false
      then
    ;

    \ Do benchmark resizing
    : bench-do-resize ( bench -- done )
      >r
      r@ bench-heap-current-count @ 0> if
	r@ bench-heap-size-limit @ r@ bench-heap-current-size @ -
	dup 3 cells >= if
	  space-div-factor / 3 cells max
	  r@ bench-prng tinymt32-generate-uint32 swap umod 3 cells max 16 align
	  r@ bench-prng tinymt32-generate-uint32
	  r@ bench-heap-current-count @ umod
	  bench-block
	  2dup block-size @ + r@ bench-heap-size-limit @ <= if
	    over r@ bench-heap-current-size +!
	    tuck block-size +!
	    dup block-size @ over block-addr @ r@ bench-heap @
	    r> bench-resize @ execute
	    swap block-addr !
	    true
	  else
	    2drop rdrop false
	  then
	else
	  drop rdrop false
	then
      else
	rdrop false
      then
    ;
    
    \ Cycle the benchmark
    : bench-cycle ( bench -- )
      >r
      begin
	r@ bench-prng tinymt32-generate-uint32 4 umod
	case
	  0 of r@ bench-do-allocate ( dup if ." +" then ) endof
	  1 of r@ bench-do-allocate ( dup if ." +" then ) endof
	  2 of r@ bench-do-free ( dup if ." -" then ) endof
	  3 of r@ bench-do-free ( dup if ." -" then ) endof
\	  4 of r@ bench-do-resize dup if ." x" then endof
	endcase
      until
      rdrop
    ;
    
  end-module> import

  \ Get the size taken up by the benchmark data
  : bench-size ( count -- bytes ) bench-block-size * bench-head-size + ;

  \ Initialize the benchmark data
  : init-bench
    ( seed allocate free resize diagnose init-heap heap limit count addr -- )
    >r
    0 r@ bench-heap-current-count !
    0 r@ bench-heap-current-size !
    r@ bench-heap-count-limit !
    r@ bench-heap-size-limit !
    r@ bench-heap !
    r@ bench-init-heap !
    r@ bench-diagnose !
    r@ bench-resize !
    r@ bench-free !
    r@ bench-allocate !
    r@ bench-seed !
    r> bench-prng tinymt32-prepare-example
  ;

  \ Execute a given number of cycles of the benchmark and output a fixed point
  \ number of seconds per cycle
  : run-bench ( cycles bench -- second-per-cycle )
    dup >r
    [:
      >r
      r@ bench-heap @ r@ bench-init-heap @ execute
      r@ bench-seed @ r@ bench-prng tinymt32-init
      systick-counter
      over begin ?dup while r@ bench-cycle repeat
      systick-counter swap -
      rot 0 swap 0 swap f/ 10000,0 f/ rdrop
    ;] try if r> dup bench-heap @ swap bench-diagnose @ execute else rdrop then
  ;

end-module
