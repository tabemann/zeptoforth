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

begin-module bitmap

  oo import
  armv6m import
  
  \ The <bitmap> class
  <object> begin-class <bitmap>
    
    \ Number of columns in bitmap
    cell member bitmap-cols
    
    \ Number of rows in bitmap
    cell member bitmap-rows
      
    begin-module bitmap-internal
      
      \ Framebuffer
      cell member bitmap-buf

      \ Get the address of a page
      method page-addr ( page bitmap -- addr )
      
      \ Set the entire display to be dirty
      method set-dirty ( bitmap -- )
      
      \ Set the entire bitmap to not be dirty
      method clear-dirty ( bitmap -- )
      
      \ Dirty a pixel on a bitmap
      method dirty-pixel ( col row bitmap -- )
      
      \ Dirty an area on a bitmap
      method dirty-area ( start-col end-col start-row end-row bitmap -- )

    end-module> import
      
    \ Clear the bitmap
    method clear-bitmap ( bitmap -- )

    \ Get whether a bitmap is dirty
    method dirty? ( bitmap -- dirty? )
      
    \ Set a rectangle with a constant value
    method set-rect-const
    ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )

    \ Or a rectangle with a constant value
    method or-rect-const
    ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )

    \ And a rectangle with a constant value
    method and-rect-const
    ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )

    \ Bit-clear a rectangle with a constant value
    method bic-rect-const
    ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )

    \ Exclusive-or a rectangle with a constant value
    method xor-rect-const
    ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )

    \ Set a rectangle
    method set-rect
    ( start-src-col start-dst-col col-count start-src-row start-dst-row )
    ( row-count src-bitmap dst-bitmap -- )
    
    \ Or a rectangle
    method or-rect
    ( start-src-col start-dst-col col-count start-src-row start-dst-row )
    ( row-count src-bitmap dst-bitmap -- )
    
    \ And a rectangle
    method and-rect
    ( start-src-col start-dst-col col-count start-src-row start-dst-row )
    ( row-count src-bitmap dst-bitmap -- )

    \ Bit-clear a rectangle
    method bic-rect
    ( start-src-col start-dst-col col-count start-src-row start-dst-row )
    ( row-count src-bitmap dst-bitmap -- )

    \ Exclusive-or a rectangle
    method xor-rect
    ( start-src-col start-dst-col col-count start-src-row start-dst-row )
    ( row-count src-bitmap dst-bitmap -- )
    
  end-class

  <bitmap> begin-implement

    \ Initialize an BITMAP device
    :noname ( buffer cols rows bitmap -- )
      dup [ <object> ] -> new
      tuck bitmap-rows !
      tuck bitmap-cols !
      tuck bitmap-buf  !
    ; define new
    
    \ Set the entire bitmap to be dirty
    :noname ( bitmap -- ) drop ; define set-dirty

    \ Set the entire bitmap to not be dirty
    :noname ( bitmap -- ) drop ; define clear-dirty
    
    \ Get whether a bitmap is dirty
    :noname ( bitmap -- dirty? ) drop true ; define dirty?
    
    \ Clear the bitmap
    :noname ( bitmap -- )
      dup set-dirty
      dup bitmap-rows @ over bitmap-cols @ * 3 rshift
      swap bitmap-buf @ swap $00 fill
    ; define clear-bitmap

    \ Get the address of a page
    :noname ( page bitmap -- addr )
      dup bitmap-buf @ swap bitmap-cols @ rot * +
    ; define page-addr

    \ Dirty a pixel on a bitmap
    :noname ( col row bitmap -- ) drop 2drop ; define dirty-pixel

    \ Dirty an area on a bitmap
    :noname ( start-col end-col start-row end-row bitmap -- )
      drop 2drop 2drop
    ; define dirty-area

    continue-module bitmap-internal
      
      \ Set a strip from a constant to another bitmap
      : set-strip-const
        ( const start-dst-row row-count col-count dst-col dst-bitmap -- )
        4 pick 3 rshift swap page-addr over pick + nip
        ( c dr rc cc da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp adds_,#_
        [else]
          r3 r2 r1 r0 4 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark>
        0 tos r4 ldrb_,[_,#_]
        \ r4: dst-byte
        r5 r4 bics_,_
        r3 r4 orrs_,_
        0 tos r4 strb_,[_,#_]
        1 tos adds_,#_
        1 r0 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code        
      ;
      
      \ Or a strip from a constant to another bitmap
      : or-strip-const
        ( const start-dst-row row-count col-count dst-col dst-bitmap -- )
        4 pick 3 rshift swap page-addr over pick + nip
        ( c dr rc cc da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp adds_,#_
        [else]
          r3 r2 r1 r0 4 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark>
        0 tos r4 ldrb_,[_,#_]
        \ r4: dst-byte
        r3 r4 orrs_,_
        0 tos r4 strb_,[_,#_]
        1 tos adds_,#_
        1 r0 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code        
      ;

      \ And a strip from a constant to another bitmap
      : and-strip-const
        ( const start-dst-row row-count col-count dst-col dst-bitmap -- )
        4 pick 3 rshift swap page-addr over pick + nip
        ( c dr rc cc da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp adds_,#_
        [else]
          r3 r2 r1 r0 4 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r3 r3 mvns_,_
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark>
        0 tos r4 ldrb_,[_,#_]
        \ r4: dst-byte
        r3 r4 bics_,_
        0 tos r4 strb_,[_,#_]
        1 tos adds_,#_
        1 r0 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code        
      ;

      \ Bit-clear a strip from a constant to another bitmap
      : bic-strip-const
        ( const start-dst-row row-count col-count dst-col dst-bitmap -- )
        4 pick 3 rshift swap page-addr over pick + nip
        ( c dr rc cc da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp adds_,#_
        [else]
          r3 r2 r1 r0 4 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark>
        0 tos r4 ldrb_,[_,#_]
        \ r4: dst-byte
        r3 r4 bics_,_
        0 tos r4 strb_,[_,#_]
        1 tos adds_,#_
        1 r0 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code        
      ;
      
      \ Exclusive-or a strip from a constant to another bitmap
      : xor-strip-const
        ( const start-dst-row row-count col-count dst-col dst-bitmap -- )
        4 pick 3 rshift swap page-addr over pick + nip
        ( c dr rc cc da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp adds_,#_
        [else]
          r3 r2 r1 r0 4 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark>
        0 tos r4 ldrb_,[_,#_]
        \ r4: dst-byte
        r3 r4 eors_,_
        0 tos r4 strb_,[_,#_]
        1 tos adds_,#_
        1 r0 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code        
      ;

      \ Set a strip from one bitmap to another bitmap
      : set-strip
        ( start-src-row start-dst-row row-count col-count src-col dst-col )
        ( src-bitmap dst-bitmap -- )
        7 pick 3 rshift rot page-addr 3 pick +
        ( sr dr rc cc sc dc db sa )
        6 pick 3 rshift rot page-addr 2 pick +
        ( sr dr rc cc sc dc sa da )
        2swap 2drop
        ( sr dr rc cc sa da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp r4 ldr_,[_,#_]
          20 dp adds_,#_
        [else]
          r4 r3 r2 r1 r0 5 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: src-addr
        \ r1: col-count
        \ r2: row-count
        \ r3: start-dst-row
        \ r4: start-src-row
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        \ r5: mask
        r1 1 pop
        0 r1 cmp_,#_
        eq bc>
        mark>
        r5 1 push
        tos r0 2 push
        0 tos tos ldrb_,[_,#_]
        \ tos: dst-byte
        0 r0 r0 ldrb_,[_,#_]
        \ r0: src-byte
        r4 r0 lsrs_,_
        \ r0: src-byte start-src-row rshift
        r5 r0 ands_,_
        \ r0: src-byte start-src-row rshift mask and
        r3 r0 lsls_,_
        \ r0: src-byte start-src-row rshift mask and start-dst-row lshift
        r3 r5 lsls_,_
        \ r5: mask start-dst-row lshift
        r5 tos bics_,_
        \ tos: dst-byte mask start-dst-row lshift bic
        tos r5 movs_,_
        \ r5: dst-byte mask start-dst-row lshift bic
        r0 r5 orrs_,_
        \ r5: combined-byte
        tos r0 2 pop
        \ tos: dst-addr
        \ r0: src-addr
        0 tos r5 strb_,[_,#_]
        r5 1 pop
        \ r5: mask
        1 tos adds_,#_
        1 r0 adds_,#_
        1 r1 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code
      ;

      \ Or a strip from one bitmap to another bitmap
      : or-strip
        ( start-src-row start-dst-row row-count col-count src-col dst-col )
        ( src-bitmap dst-bitmap -- )
        7 pick 3 rshift rot page-addr 3 pick +
        ( sr dr rc cc sc dc db sa )
        6 pick 3 rshift rot page-addr 2 pick +
        ( sr dr rc cc sc dc sa da )
        2swap 2drop
        ( sr dr rc cc sa da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp r4 ldr_,[_,#_]
          20 dp adds_,#_
        [else]
          r4 r3 r2 r1 r0 5 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: src-addr
        \ r1: col-count
        \ r2: row-count
        \ r3: start-dst-row
        \ r4: start-src-row
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        \ r5: mask
        r1 1 pop
        0 r1 cmp_,#_
        eq bc>
        mark>
        r5 1 push
        tos r0 2 push
        0 tos tos ldrb_,[_,#_]
        \ tos: dst-byte
        0 r0 r0 ldrb_,[_,#_]
        \ r0: src-byte
        r4 r0 lsrs_,_
        \ r0: src-byte start-src-row rshift
        r5 r0 ands_,_
        \ r0: src-byte start-src-row rshift mask and
        r3 r0 lsls_,_
        \ r0: src-byte start-src-row rshift mask and start-dst-row lshift
        tos r5 movs_,_
        \ r5: dst-byte
        r0 r5 orrs_,_
        \ r5: combined-byte
        tos r0 2 pop
        \ tos: dst-addr
        \ r0: src-addr
        0 tos r5 strb_,[_,#_]
        r5 1 pop
        \ r5: mask
        1 tos adds_,#_
        1 r0 adds_,#_
        1 r1 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code
      ;

      \ And a strip from one bitmap to another bitmap
      : and-strip
        ( start-src-row start-dst-row row-count col-count src-col dst-col )
        ( src-bitmap dst-bitmap -- )
        7 pick 3 rshift rot page-addr 3 pick +
        ( sr dr rc cc sc dc db sa )
        6 pick 3 rshift rot page-addr 2 pick +
        ( sr dr rc cc sc dc sa da )
        2swap 2drop
        ( sr dr rc cc sa da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp r4 ldr_,[_,#_]
          20 dp adds_,#_
        [else]
          r4 r3 r2 r1 r0 5 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: src-addr
        \ r1: col-count
        \ r2: row-count
        \ r3: start-dst-row
        \ r4: start-src-row
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        \ r5: mask
        r1 1 pop
        0 r1 cmp_,#_
        eq bc>
        mark>
        r5 1 push
        tos r0 2 push
        0 tos tos ldrb_,[_,#_]
        \ tos: dst-byte
        0 r0 r0 ldrb_,[_,#_]
        \ r0: src-byte
        r0 r0 mvns_,_
        \ r0: src-byte not
        r4 r0 lsrs_,_
        \ r0: src-byte not start-src-row rshift
        r5 r0 ands_,_
        \ r0: src-byte not start-src-row rshift mask and
        r3 r0 lsls_,_
        \ r0: src-byte not start-src-row rshift mask and start-dst-row lshift
        tos r5 movs_,_
        \ r5: dst-byte
        r0 r5 bics_,_
        \ r5: combined-byte
        tos r0 2 pop
        \ tos: dst-addr
        \ r0: src-addr
        0 tos r5 strb_,[_,#_]
        r5 1 pop
        \ r5: mask
        1 tos adds_,#_
        1 r0 adds_,#_
        1 r1 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code
      ;

      \ Bit-clear a strip from one bitmap to another bitmap
      : bic-strip
        ( start-src-row start-dst-row row-count col-count src-col dst-col )
        ( src-bitmap dst-bitmap -- )
        7 pick 3 rshift rot page-addr 3 pick +
        ( sr dr rc cc sc dc db sa )
        6 pick 3 rshift rot page-addr 2 pick +
        ( sr dr rc cc sc dc sa da )
        2swap 2drop
        ( sr dr rc cc sa da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp r4 ldr_,[_,#_]
          20 dp adds_,#_
        [else]
          r4 r3 r2 r1 r0 5 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: src-addr
        \ r1: col-count
        \ r2: row-count
        \ r3: start-dst-row
        \ r4: start-src-row
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        \ r5: mask
        r1 1 pop
        0 r1 cmp_,#_
        eq bc>
        mark>
        r5 1 push
        tos r0 2 push
        0 tos tos ldrb_,[_,#_]
        \ tos: dst-byte
        0 r0 r0 ldrb_,[_,#_]
        \ r0: src-byte
        r4 r0 lsrs_,_
        \ r0: src-byte start-src-row rshift
        r5 r0 ands_,_
        \ r0: src-byte start-src-row rshift mask and
        r3 r0 lsls_,_
        \ r0: src-byte start-src-row rshift mask and start-dst-row lshift
        tos r5 movs_,_
        \ r5: dst-byte
        r0 r5 bics_,_
        \ r5: combined-byte
        tos r0 2 pop
        \ tos: dst-addr
        \ r0: src-addr
        0 tos r5 strb_,[_,#_]
        r5 1 pop
        \ r5: mask
        1 tos adds_,#_
        1 r0 adds_,#_
        1 r1 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code
      ;

      \ Exclusive-or a strip from one bitmap to another bitmap
      : xor-strip
        ( start-src-row start-dst-row row-count col-count src-col dst-col )
        ( src-bitmap dst-bitmap -- )
        7 pick 3 rshift rot page-addr 3 pick +
        ( sr dr rc cc sc dc db sa )
        6 pick 3 rshift rot page-addr 2 pick +
        ( sr dr rc cc sc dc sa da )
        2swap 2drop
        ( sr dr rc cc sa da )
        code[
        r5 r4 2 push
        cortex-m7? [if]
          0 dp r0 ldr_,[_,#_]
          4 dp r1 ldr_,[_,#_]
          8 dp r2 ldr_,[_,#_]
          12 dp r3 ldr_,[_,#_]
          16 dp r4 ldr_,[_,#_]
          20 dp adds_,#_
        [else]
          r4 r3 r2 r1 r0 5 dp ldm
        [then]
        \ tos: dst-addr
        \ r0: src-addr
        \ r1: col-count
        \ r2: row-count
        \ r3: start-dst-row
        \ r4: start-src-row
        $FF r5 movs_,#_
        r1 1 push
        8 r1 movs_,#_
        r2 r1 r1 subs_,_,_
        r1 r5 lsrs_,_
        \ r5: mask
        r1 1 pop
        0 r1 cmp_,#_
        eq bc>
        mark>
        r5 1 push
        tos r0 2 push
        0 tos tos ldrb_,[_,#_]
        \ tos: dst-byte
        0 r0 r0 ldrb_,[_,#_]
        \ r0: src-byte
        r4 r0 lsrs_,_
        \ r0: src-byte start-src-row rshift
        r5 r0 ands_,_
        \ r0: src-byte start-src-row rshift mask and
        r3 r0 lsls_,_
        \ r0: src-byte start-src-row rshift mask and start-dst-row lshift
        tos r5 movs_,_
        \ r5: dst-byte
        r0 r5 eors_,_
        \ r5: combined-byte
        tos r0 2 pop
        \ tos: dst-addr
        \ r0: src-addr
        0 tos r5 strb_,[_,#_]
        r5 1 pop
        \ r5: mask
        1 tos adds_,#_
        1 r0 adds_,#_
        1 r1 subs_,#_
        ne bc<
        >mark
        tos 1 dp ldm
        r5 r4 2 pop        
        ]code
      ;
      
      \ Get the next page-aligned row
      : next-page-row ( row -- row' )
        dup 8 align 2dup = if drop 8 + else nip then
      ;
    
      \ Get the number of rows for an iteration with no source
      : strip-rows-no-src ( start-dst-row total-row-count -- row-count )
        over + over next-page-row min swap -
      ;

      \ Get the number of rows for an iteration
      : strip-rows ( start-src-row start-dst-row total-row-count -- row-count )
        2 pick over + 3 pick next-page-row min 3 pick - ( sr dr rc src )
        2 pick 2 pick + 3 pick next-page-row min 3 pick - ( sr dr rc src drc )
        min nip nip nip ( rc' )
      ;
      
      \ Carry out an operation on an area with a constant value
      : blit-const
        ( const start-dst-col col-count start-dst-row row-count dst-bitmap )
        ( op -- )
        >r >r
        begin ?dup while
          ( c dc cc dr rc )
          2dup strip-rows-no-src
          ( c dc cc dr rc src )
          5 pick 3 pick 2 pick 6 pick 8 pick >r >r 2dup r> r> execute
          ( c dc cc dr rc src )
          r> r@ - swap r> + swap
          ( c dc cc dr rc' )
        repeat
        2drop 2drop rdrop rdrop
      ;

      \ Carry out an operation on an area
      : blit
        ( start-src-col start-dst-col col-count start-src-row start-dst-row )
        ( row-count src-bitmap dst-bitmap op -- )
        >r >r >r
        begin ?dup while
          ( sc dc cc sr dr rc )
          3dup strip-rows
          ( sc dc cc sr dr rc src )
          3 pick 3 pick 2 pick 7 pick 10 pick 10 pick r> r> r> 3dup >r >r >r
          ( sc dc cc sr dr rc src sr dr src cc sc dc sb db op )
          execute
          ( sc dc cc sr dr rc src )
          r> r@ - rot r@ + rot r> +
          ( sc dc cc sr dr rc' )
        repeat
        2drop 2drop drop rdrop rdrop rdrop
      ;

      \ Clip a destination-only rectangle
      : clip-dst-only
        ( start-dst-col col-count start-dst-row row-count dst-bitmap -- )
        ( new-start-dst-count new-col-count new-start-dst-row new-row-count )
        >r
        over 0 < if + 0 min 0 swap then
        over r@ bitmap-rows @ < if
          2dup + r@ bitmap-rows @ > if 2dup + r@ bitmap-rows @ - - 0 max then
        else
          2drop 0 0
        then
        2swap
        over 0 < if + 0 min 0 swap then
        over r@ bitmap-cols @ < if
          2dup + r@ bitmap-cols @ > if 2dup + r@ bitmap-cols @ - - 0 max then
        else
          2drop 0 0
        then
        2swap rdrop
      ;

      \ Clip a rectangle to the source dimensions
      : clip-src
        ( start-src-col start-dst-col col-count start-src-row start-dst-row )
        ( row-count src-bitmap -- )
        >r
        2 pick 0 < if 2 pick + swap 2 pick negate + swap rot drop 0 -rot then
        2 pick r@ bitmap-rows @ < if
          2 pick over + r@ bitmap-rows @ > if
            2 pick over + r@ bitmap-rows @ - - 0 max
          then
        else
          2drop drop 0 0 0
        then
        r> swap >r swap >r swap >r >r
        2 pick 0 < if 2 pick + swap 2 pick negate + swap rot drop 0 -rot then
        2 pick r@ bitmap-cols @ < if
          2 pick over + r@ bitmap-cols @ > if
            2 pick over + r@ bitmap-cols @ - - 0 max
          then
        else
          2drop drop 0 0 0
        then
        rdrop r> r> r>
      ;

      \ Clip a rectangle to the destination dimensions
      : clip-dst
        ( start-src-col start-dst-col col-count start-src-row start-dst-row )
        ( row-count src-bitmap -- )
        >r
        over 0 < if over + rot 2 pick - -rot nip 0 swap then
        over r@ bitmap-rows @ < if
          2dup + r@ bitmap-rows @ > if
            2dup + r@ bitmap-rows @ - - 0 max
          then
        else
          2drop drop 0 0 0
        then
        r> swap >r swap >r swap >r >r
        over 0 < if over + rot 2 pick - -rot nip 0 swap then
        over r@ bitmap-cols @ < if
          2dup + r@ bitmap-cols @ > if
            2dup + r@ bitmap-cols @ - - 0 max
          then
        else
          2drop drop 0 0 0
        then
        rdrop r> r> r>
      ;

      \ Clip a rectangle
      : clip
        ( start-src-col start-dst-col col-count start-src-row start-dst-row )
        ( row-count src-bitmap dst-bitmap -- )
        >r clip-src r> clip-dst
      ;
        
    end-module

    \ Set a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      >r r@ clip-dst-only r>
      4 pick dup 5 pick + 4 pick dup 5 pick + 4 pick dirty-area
      ['] set-strip-const blit-const
    ; define set-rect-const

    \ Or a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      >r r@ clip-dst-only r>
      4 pick dup 5 pick + 4 pick dup 5 pick + 4 pick dirty-area
      ['] or-strip-const blit-const
    ; define or-rect-const

    \ And a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      >r r@ clip-dst-only r>
      4 pick dup 5 pick + 4 pick dup 5 pick + 4 pick dirty-area
      ['] and-strip-const blit-const
    ; define and-rect-const

    \ Bit-clear a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      >r r@ clip-dst-only r>
      4 pick dup 5 pick + 4 pick dup 5 pick + 4 pick dirty-area
      ['] bic-strip-const blit-const
    ; define bic-rect-const

    \ Exclusive-or a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      >r r@ clip-dst-only r>
      4 pick dup 5 pick + 4 pick dup 5 pick + 4 pick dirty-area
      ['] xor-strip-const blit-const
    ; define xor-rect-const

    \ Set a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      2>r 2r@ clip 2r>
      6 pick dup 7 pick + 5 pick dup 6 pick + 4 pick dirty-area
      ['] set-strip blit
    ; define set-rect
    
    \ Or a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      2>r 2r@ clip 2r>
      6 pick dup 7 pick + 5 pick dup 6 pick + 4 pick dirty-area
      ['] or-strip blit
    ; define or-rect
    
    \ And a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      2>r 2r@ clip 2r>
      6 pick dup 7 pick + 5 pick dup 6 pick + 4 pick dirty-area
      ['] and-strip blit
    ; define and-rect

    \ Bit-clear a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      2>r 2r@ clip 2r>
      6 pick dup 7 pick + 5 pick dup 6 pick + 4 pick dirty-area
      ['] bic-strip blit
    ; define bic-rect

    \ Exclusive-or a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      2>r 2r@ clip 2r>
      6 pick dup 7 pick + 5 pick dup 6 pick + 4 pick dirty-area
      ['] xor-strip blit
    ; define xor-rect

  end-implement
  
end-module
