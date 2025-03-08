\ Copyright (c) 2022-2024 Travis Bemann
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

begin-module blk

  oo import
  block-dev import
  lock import
  block import

  \ Attempted to write to protected block zero
  : x-block-zero-protected ( - ) ." block block zero is protected" cr ;
  
  \ Blocks block device class
  <block-dev> begin-class <blocks>

    begin-module blk-internal
      
      \ Buffer count
      8 constant buffer-count
      
      \ Sector size
      512 constant sector-size

      \ Lock protecting the blocks device
      lock-size member blk-lock

      \ Block scratchpad
      block-size member blk-scratchpad
      
      \ Blocks buffers
      sector-size buffer-count * member blk-buffers

      \ Blocks buffer assignments
      buffer-count cells member blk-buffer-assign

      \ Blocks buffer age
      buffer-count cells member blk-buffer-age

      \ Blocks buffer dirty
      buffer-count member blk-buffer-dirty

      \ Protect block zero
      cell member blk-protect-block-zero
      
      \ Maximum block count
      cell member max-block-count
      
      \ Write through mode
      cell member write-through

    end-module> import

    \ Enable block zero writes
    method write-blk-block-zero! ( enabled blk -- )

    continue-module blk-internal
    
      \ Validate a block index
      method validate-block ( block blk -- )
      
      \ Implement flush-blocks
      method do-flush-blocks ( blk -- )

      \ Initialize the block itself
      method init-blk ( blk -- )

      \ Read a block from the block
      method read-blk-block ( index block blk -- )
      
      \ Write a block to the block
      method write-blk-block ( index block blk -- )

      \ Evict an BLK buffer
      method evict-blk-buffer ( index blk -- )

      \ Find an BLK buffer
      method find-blk-buffer ( block blk -- index | -1 )
      
      \ Age the buffers
      method age-blk-buffers ( blk -- )

      \ Find the oldest buffer
      method oldest-blk-buffer ( blk -- index )

      \ Select a free BLK buffer, and if one is not free, evict the oldest
      method select-blk-buffer ( blk -- index )

    end-module
      
  end-class

  \ Implement Block block device class
  <blocks> begin-implement

    :noname ( blk -- )
      dup >r <block-dev>->new r>
      dup blk-lock init-lock
      false over write-through !
      true over blk-protect-block-zero !
      block-internal::sector-count 1- block-internal::sector-block-count *
      block-size sector-size / * over max-block-count !
      clear-blocks
    ; define new

    :noname { blk -- bytes } sector-size ; define block-size
    
    :noname ( blk -- blocks ) max-block-count @ ; define block-count

    \ Enable block zero writes
    :noname ( enabled blk -- )
      swap not swap blk-protect-block-zero !
    ; define write-blk-block-zero!

    :noname ( c-addr u block blk -- )
      2dup validate-block
      2 pick sector-size <> if
        0 -rot block-part!
      else
        [:
          >r
          dup 0= r@ blk-protect-block-zero @ and triggers x-block-zero-protected
          dup r@ find-blk-buffer dup -1 <> if ( c-addr u block index )
            nip ( c-addr u index )
          else
            drop r@ select-blk-buffer ( c-addr u block index )
            tuck cells r@ blk-buffer-assign + ! ( c-addr u index )
          then
          $FF over r@ blk-buffer-dirty + c! ( c-addr u index )
          r> over >r >r ( c-addr u index )
          sector-size * r@ blk-buffers + swap sector-size min move ( )
          r@ age-blk-buffers ( )
          r> r> cells over blk-buffer-age + 0 swap ! ( blk )
          dup write-through @ if do-flush-blocks else drop then ( )
        ;] over blk-lock with-lock
      then
    ; define block!
    
    :noname ( c-addr u offset block blk -- )
      2dup validate-block
      [:
        >r
        dup 0= r@ blk-protect-block-zero @ and triggers x-block-zero-protected
        dup r@ find-blk-buffer dup -1 <> if ( c-addr u offset block index )
          nip ( c-addr u offset index )
        else
          drop r@ select-blk-buffer ( c-addr u offset block index )
          2dup cells r@ blk-buffer-assign + ! ( c-addr u offset block index )
          tuck swap r@ read-blk-block ( c-addr u offset index )
        then
        $FF over r@ blk-buffer-dirty + c! ( c-addr u offset index )
        r> over >r >r ( c-addr u offset index )
        sector-size * r@ blk-buffers + ( c-addr u offset buffer )
        swap sector-size min dup >r + ( c-addr u buffer )
        swap sector-size r> - 0 max min move ( )
        r@ age-blk-buffers ( )
        r> r> cells over blk-buffer-age + 0 swap ! ( blk )
        dup write-through @ if do-flush-blocks else drop then ( )
      ;] over blk-lock with-lock
    ; define block-part!

    :noname ( c-addr u block blk -- )
      2dup validate-block
      [:
	>r dup r@ find-blk-buffer dup -1 <> if ( c-addr u block index )
          r> over >r >r nip ( c-addr u index )
        else
          drop r@ select-blk-buffer ( c-addr u block index )
          2dup cells r@ blk-buffer-assign + ! ( c-addr u block index )
          tuck swap r@ read-blk-block ( c-addr u index )
          r> over >r >r ( c-addr u index )
        then
        sector-size * r@ blk-buffers + -rot sector-size min move ( )
        r@ age-blk-buffers ( )
        r> r> cells swap blk-buffer-age + 0 swap ! ( )
      ;] over blk-lock with-lock
    ; define block@
    
    :noname ( c-addr u offset block blk -- )
      2dup validate-block
      [:
	>r dup r@ find-blk-buffer dup -1 <> if ( c-addr u offset block index )
          r> over >r >r nip ( c-addr u offset index )
        else
          drop r@ select-blk-buffer ( c-addr u offset block index )
          2dup cells r@ blk-buffer-assign + ! ( c-addr u offset block index )
          tuck swap r@ read-blk-block ( c-addr u offset index )
          r> over >r >r ( c-addr u offset index )
        then
        sector-size * r@ blk-buffers + ( c-addr u offset buffer )
        swap sector-size min dup >r + ( c-addr u buffer )
        -rot sector-size r> - 0 max min move ( )
        r@ age-blk-buffers ( )
        r> r> cells swap blk-buffer-age + 0 swap ! ( )
      ;] over blk-lock with-lock
    ; define block-part@

    :noname ( blk -- )
      [: do-flush-blocks ;] over blk-lock with-lock
    ; define flush-blocks
    
    :noname ( blk -- )
      [:
        0 begin dup buffer-count < while
          2dup cells swap blk-buffer-assign + -1 swap !
          2dup cells swap blk-buffer-age + 0 swap !
          2dup swap blk-buffer-dirty + 0 swap c!
          1+
        repeat
        2drop
      ;] over blk-lock with-lock
    ; define clear-blocks
    
    :noname ( write-through blk -- )
      >r dup r@ write-through ! if r@ flush-blocks then rdrop
    ; define write-through!
    
    :noname ( blk -- write-through )
      write-through @
    ; define write-through@
    
    :noname ( block blk -- )
      over 0 >= averts x-block-out-of-range
      block-count < averts x-block-out-of-range
    ; define validate-block
    
    :noname ( blk -- )
      >r
      0 begin dup buffer-count < while
        dup cells r@ blk-buffer-assign + @ -1 <> if
          dup r@ blk-buffer-dirty + c@ if
            dup dup cells r@ blk-buffer-assign + @ r@ write-blk-block
            0 over r@ blk-buffer-dirty + c!
          then
        then
        1+
      repeat
      drop rdrop
    ; define do-flush-blocks

    :noname { index block blk -- }
      block 0= blk blk-protect-block-zero @ and triggers x-block-zero-protected
      block 1 and sector-size * { same-offset }
      block 1 and 1 xor sector-size * { other-offset }
      block 1 rshift find-block { addr }
      addr 0<> if
        addr other-offset + blk blk-scratchpad other-offset + sector-size move
      else
        blk blk-scratchpad other-offset + sector-size 0 fill
      then
      index sector-size * blk blk-buffers + blk blk-scratchpad same-offset +
      sector-size move
      blk blk-scratchpad block 1 rshift block::block!
    ; define write-blk-block

    :noname { index block blk -- }
      block 1 rshift find-block { addr }
      addr 0<> if
        block 1 and sector-size * { same-offset }
        addr same-offset + index sector-size * blk blk-buffers + sector-size
        move
      else
        index sector-size * blk blk-buffers + sector-size 0 fill
      then
    ; define read-blk-block

    :noname ( index blk -- )
      >r
      dup r@ blk-buffer-dirty + c@ if
	dup dup cells r@ blk-buffer-assign + @ r@ write-blk-block
	0 over r@ blk-buffer-dirty + c!
      then
      0 over cells r@ blk-buffer-age + !
      -1 swap cells r> blk-buffer-assign + !
    ; define evict-blk-buffer
    
    :noname ( block blk -- index | -1 )
      >r 0 begin dup buffer-count < while
	2dup cells r@ blk-buffer-assign + @ = if nip rdrop exit then 1+
      repeat
      2drop rdrop -1
    ; define find-blk-buffer

    :noname ( blk -- )
      >r 0 begin dup buffer-count < while
        dup cells r@ blk-buffer-age + 1 swap +! 1+
      repeat
      drop rdrop
    ; define age-blk-buffers

    :noname ( blk -- index )
      >r 0 -1 0 begin dup buffer-count < while ( oldest-index oldest-age index )
        dup cells r@ blk-buffer-assign + @ -1 <> if ( oldest-index oldest-age index )
          dup cells r@ blk-buffer-age + @ 2 pick >= if ( oldest-index oldest-age index )
            rot drop tuck dup cells r@ blk-buffer-age + @ rot drop swap 1+
          else
            1+
          then
        else
          nip nip $7FFFFFFF over 1+
        then
      repeat
      2drop rdrop
    ; define oldest-blk-buffer

    :noname ( blk -- index )
      >r r@ oldest-blk-buffer
      dup cells r@ blk-buffer-assign + @ -1 <> if
	dup r@ evict-blk-buffer
      then
      rdrop
    ; define select-blk-buffer
    
  end-implement
  
end-module

reboot
