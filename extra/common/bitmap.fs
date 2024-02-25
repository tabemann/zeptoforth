\ Copyright (c) 2022-2023 Travis Bemann
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

  \ Get the size of a bitmap buffer in bytes for a given number of columns and
  \ rows
  : bitmap-buf-size { cols rows --  bytes } rows 8 align 3 rshift cols * ;

  \ Pixel drawing operations

  \ Set pixels
  0 constant op-set

  \ Or pixels
  1 constant op-or

  \ And pixels
  2 constant op-and

  \ Bit-clear pixels
  3 constant op-bic

  \ Exclusive-or pixels
  4 constant op-xor

  \ Invalid operation exception
  : x-invalid-op ( -- ) ." invalid drawing operation" cr ;
  
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

    \ Get bitmap dimensions
    method dim@ ( bitmap -- cols rows )
    
    \ Clear the bitmap
    method clear-bitmap ( bitmap -- )

    \ Get whether a bitmap is dirty
    method dirty? ( bitmap -- dirty? )

    \ Get the state of a pixel
    method pixel@ ( col row bitmap -- state )
      
    \ Draw a constant pixel on a bitmap
    method draw-pixel-const ( const dst-col dst-row op dst-bitmap -- )

    \ Draw a constant rectangle on a bitmap
    method draw-rect-const
    ( const start-dst-col cols start-dst-row rows op dst-bitmap -- )

    \ Draw a rectangle on a bitmap from another bitmap
    method draw-rect
    ( start-src-col start-src-row start-dst-col start-dst-row cols rows )
    ( op src-bitmap dst-bitmap -- )

  end-class

  <bitmap> begin-implement

    \ Initialize a bitmap
    :noname { buf cols rows self -- }
      self <object>->new
      rows self bitmap-rows !
      cols self bitmap-cols !
      buf self bitmap-buf  !
      self clear-bitmap
    ; define new
    
    \ Set the entire bitmap to be dirty
    :noname ( bitmap -- ) drop ; define set-dirty

    \ Set the entire bitmap to not be dirty
    :noname ( bitmap -- ) drop ; define clear-dirty
    
    \ Get whether a bitmap is dirty
    :noname ( bitmap -- dirty? ) drop true ; define dirty?

    \ Get bitmap dimensions
    :noname ( bitmap -- cols rows )
      dup bitmap-cols @ swap bitmap-rows @
    ; define dim@
    
    \ Clear the bitmap
    :noname { self -- }
      self set-dirty
      self bitmap-buf @
      self bitmap-rows @ self bitmap-cols @ bitmap-buf-size $00 fill
    ; define clear-bitmap

    \ Get the address of a page
    :noname { page self -- addr }
      self bitmap-buf @ self bitmap-cols @ page * +
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
        { const dst-row row-count col-count dst-col self -- }
        dst-row 3 rshift self page-addr dst-col +
        dst-row 7 and to dst-row        
        code[
        8 r0 ldr_,[sp,#_]
        12 r1 ldr_,[sp,#_]
        16 r2 ldr_,[sp,#_]
        20 r3 ldr_,[sp,#_]
        r5 r4 2 push
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        8 r4 movs_,#_
        r1 r4 r4 subs_,_,_
        r4 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark<
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
        { const dst-row row-count col-count dst-col self -- }
        dst-row 3 rshift self page-addr dst-col +
        dst-row 7 and to dst-row        
        code[
        8 r0 ldr_,[sp,#_]
        12 r1 ldr_,[sp,#_]
        16 r2 ldr_,[sp,#_]
        20 r3 ldr_,[sp,#_]
        r5 r4 2 push
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        8 r4 movs_,#_
        r1 r4 r4 subs_,_,_
        r4 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark<
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
        { const dst-row row-count col-count dst-col self -- }
        dst-row 3 rshift self page-addr dst-col +
        dst-row 7 and to dst-row        
        code[
        8 r0 ldr_,[sp,#_]
        12 r1 ldr_,[sp,#_]
        16 r2 ldr_,[sp,#_]
        20 r3 ldr_,[sp,#_]
        r5 r4 2 push
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        8 r4 movs_,#_
        r1 r4 r4 subs_,_,_
        r4 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r3 r3 mvns_,_
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark<
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
        { const dst-row row-count col-count dst-col self -- }
        dst-row 3 rshift self page-addr dst-col +
        dst-row 7 and to dst-row        
        code[
        8 r0 ldr_,[sp,#_]
        12 r1 ldr_,[sp,#_]
        16 r2 ldr_,[sp,#_]
        20 r3 ldr_,[sp,#_]
        r5 r4 2 push
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        8 r4 movs_,#_
        r1 r4 r4 subs_,_,_
        r4 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark<
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
        { const dst-row row-count col-count dst-col self -- }
        dst-row 3 rshift self page-addr dst-col +
        dst-row 7 and to dst-row        
        code[
        8 r0 ldr_,[sp,#_]
        12 r1 ldr_,[sp,#_]
        16 r2 ldr_,[sp,#_]
        20 r3 ldr_,[sp,#_]
        r5 r4 2 push
        \ tos: dst-addr
        \ r0: col-count
        \ r1: row-count
        \ r2: start-dst-row
        \ r3: const
        $FF r5 movs_,#_
        8 r4 movs_,#_
        r1 r4 r4 subs_,_,_
        r4 r5 lsrs_,_
        r2 r5 lsls_,_
        \ r5: mask
        r5 r3 ands_,_
        0 r0 cmp_,#_
        eq bc>
        mark<
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
        { src-row dst-row row-count col-count src-col dst-col src dst -- }
        src-row 3 rshift src page-addr src-col +
        dst-row 3 rshift dst page-addr dst-col +
        src-row 7 and to src-row
        dst-row 7 and to dst-row
        code[
        r0 1 dp ldm
        16 r1 ldr_,[sp,#_]
        20 r2 ldr_,[sp,#_]
        24 r3 ldr_,[sp,#_]
        r4 1 push
        32 r4 ldr_,[sp,#_]
        r5 1 push
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
        mark<
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
        { src-row dst-row row-count col-count src-col dst-col src dst -- }
        src-row 3 rshift src page-addr src-col +
        dst-row 3 rshift dst page-addr dst-col +
        src-row 7 and to src-row
        dst-row 7 and to dst-row
        code[
        r0 1 dp ldm
        16 r1 ldr_,[sp,#_]
        20 r2 ldr_,[sp,#_]
        24 r3 ldr_,[sp,#_]
        r4 1 push
        32 r4 ldr_,[sp,#_]
        r5 1 push
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
        mark<
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
        { src-row dst-row row-count col-count src-col dst-col src dst -- }
        src-row 3 rshift src page-addr src-col +
        dst-row 3 rshift dst page-addr dst-col +
        src-row 7 and to src-row
        dst-row 7 and to dst-row
        code[
        r0 1 dp ldm
        16 r1 ldr_,[sp,#_]
        20 r2 ldr_,[sp,#_]
        24 r3 ldr_,[sp,#_]
        r4 1 push
        32 r4 ldr_,[sp,#_]
        r5 1 push
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
        mark<
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
        { src-row dst-row row-count col-count src-col dst-col src dst -- }
        src-row 3 rshift src page-addr src-col +
        dst-row 3 rshift dst page-addr dst-col +
        src-row 7 and to src-row
        dst-row 7 and to dst-row
        code[
        r0 1 dp ldm
        16 r1 ldr_,[sp,#_]
        20 r2 ldr_,[sp,#_]
        24 r3 ldr_,[sp,#_]
        r4 1 push
        32 r4 ldr_,[sp,#_]
        r5 1 push
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
        mark<
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
        { src-row dst-row row-count col-count src-col dst-col src dst -- }
        src-row 3 rshift src page-addr src-col +
        dst-row 3 rshift dst page-addr dst-col +
        src-row 7 and to src-row
        dst-row 7 and to dst-row
        code[
        r0 1 dp ldm
        16 r1 ldr_,[sp,#_]
        20 r2 ldr_,[sp,#_]
        24 r3 ldr_,[sp,#_]
        r4 1 push
        32 r4 ldr_,[sp,#_]
        r5 1 push
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
        mark<
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
      : next-page-row { row -- row' } row 8 align dup row = if 8 + then ;
    
      \ Get the number of rows for an iteration with no source
      : strip-rows-single { row total-row-count -- row-count }
        row total-row-count + row next-page-row min row -
      ;

      \ Get the number of rows for an iteration
      : strip-rows { src-row dst-row total-row-count -- row-count }
        src-row total-row-count strip-rows-single dst-row swap strip-rows-single
      ;
      
      \ Carry out an operation on an area with a constant value
      : blit-const { const dst-col col-count dst-row row-count dst op -- }
        begin row-count 0> while
          dst-row row-count strip-rows-single { strip-row-count }
          const dst-row strip-row-count col-count dst-col dst op execute
          strip-row-count negate +to row-count
          strip-row-count +to dst-row
        repeat
      ;

      \ Carry out an operation on an area
      : blit
        { src-col dst-col col-count src-row dst-row row-count src dst op -- }
        begin row-count 0> while
          src-row dst-row row-count strip-rows { strip-row-count }
          src-row dst-row strip-row-count col-count src-col dst-col
          src dst op execute
          strip-row-count negate +to row-count
          strip-row-count +to src-row
          strip-row-count +to dst-row
        repeat
      ;
        
      \ Set a pixel with a constant value
      : set-pixel-const { const dst-col dst-row dst -- }
        0 dst-col <= 0 dst-row <= and
        dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
          dst-col dst-row dst dirty-pixel
          const dst-row 1 1 dst-col dst set-strip-const
        then
      ;
      
      \ Or a pixel with a constant value
      : or-pixel-const { const dst-col dst-row dst -- }
        0 dst-col <= 0 dst-row <= and
        dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
          dst-col dst-row dst dirty-pixel
          const dst-row 1 1 dst-col dst or-strip-const
        then
      ;

      \ And a pixel with a constant value
      : and-pixel-const { const dst-col dst-row dst -- }
        0 dst-col <= 0 dst-row <= and
        dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
          dst-col dst-row dst dirty-pixel
          const dst-row 1 1 dst-col dst and-strip-const
        then
      ;

      \ Bit-clear a pixel with a constant value
      : bic-pixel-const { const dst-col dst-row dst -- }
        0 dst-col <= 0 dst-row <= and
        dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
          dst-col dst-row dst dirty-pixel
          const dst-row 1 1 dst-col dst bic-strip-const
        then
      ;

      \ Exclusive-or a pixel with a constant value
      : xor-pixel-const { const dst-col dst-row dst -- }
        0 dst-col <= 0 dst-row <= and
        dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
          dst-col dst-row dst dirty-pixel
          const dst-row 1 1 dst-col dst xor-strip-const
        then
      ;
      
      \ Set a rectangle with a constant value
      : set-rect-const
        ( const start-dst-col start-dst-row col-count row-count dst-bitmap -- )
        { dst } dst dim@ clip::clip-dst-only
        { const dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        const dst-col col-count dst-row row-count dst
        ['] set-strip-const blit-const
      ;

      \ Or a rectangle with a constant value
      : or-rect-const
        ( const start-dst-col start-dst-row col-count row-count dst-bitmap -- )
        { dst } dst dim@ clip::clip-dst-only
        { const dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        const dst-col col-count dst-row row-count dst
        ['] or-strip-const blit-const
      ;

      \ And a rectangle with a constant value
      : and-rect-const
        ( const start-dst-col start-dst-row col-count row-count dst-bitmap -- )
        { dst } dst dim@ clip::clip-dst-only
        { const dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        const dst-col col-count dst-row row-count dst
        ['] and-strip-const blit-const
      ;

      \ Bit-clear a rectangle with a constant value
      : bic-rect-const
        ( const start-dst-col start-dst-row col-count row-count dst-bitmap -- )
        { dst } dst dim@ clip::clip-dst-only
        { const dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        const dst-col col-count dst-row row-count dst
        ['] bic-strip-const blit-const
      ;

      \ Exclusive-or a rectangle with a constant value
      : xor-rect-const
        ( const start-dst-col start-dst-row col-count row-count dst-bitmap -- )
        { dst } dst dim@ clip::clip-dst-only
        { const dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        const dst-col col-count dst-row row-count dst
        ['] xor-strip-const blit-const
      ;

      \ Set a rectangle
      : set-rect
        ( start-src-col start-src-row start-dst-col start-dst-row col-count )
        ( row-count src-bitmap dst-bitmap -- )
        { src dst } src dim@ dst dim@ clip::clip-src-dst
        { src-col src-row dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        src-col dst-col col-count src-row dst-row row-count src dst
        ['] set-strip blit
      ;
      
      \ Or a rectangle
      : or-rect
        ( start-src-col start-src-row start-dst-col start-dst-row col-count )
        ( row-count src-bitmap dst-bitmap -- )
        { src dst } src dim@ dst dim@ clip::clip-src-dst
        { src-col src-row dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        src-col dst-col col-count src-row dst-row row-count src dst
        ['] or-strip blit
      ;
      
      \ And a rectangle
      : and-rect
        ( start-src-col start-src-row start-dst-col start-dst-row col-count )
        ( row-count src-bitmap dst-bitmap -- )
        { src dst } src dim@ dst dim@ clip::clip-src-dst
        { src-col src-row dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        src-col dst-col col-count src-row dst-row row-count src dst
        ['] and-strip blit
      ;

      \ Bit-clear a rectangle
      : bic-rect
        ( start-src-col start-src-row start-dst-col start-dst-row col-count )
        ( row-count src-bitmap dst-bitmap -- )
        { src dst } src dim@ dst dim@ clip::clip-src-dst
        { src-col src-row dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        src-col dst-col col-count src-row dst-row row-count src dst
        ['] bic-strip blit
      ;

      \ Exclusive-or a rectangle
      : xor-rect
        ( start-src-col start-src-row start-dst-col start-dst-row col-count )
        ( row-count src-bitmap dst-bitmap -- )
        { src dst } src dim@ dst dim@ clip::clip-src-dst
        { src-col src-row dst-col dst-row col-count row-count }
        dst-col dup col-count + dst-row dup row-count + dst dirty-area
        src-col dst-col col-count src-row dst-row row-count src dst
        ['] xor-strip blit
      ;

    end-module
    
    \ Get the state of a pixel
    :noname { col row self -- state }
      0 col <= 0 row <= and
      col self bitmap-cols @ < row self bitmap-rows @ < and and if
        row 3 rshift self page-addr col + c@ row 7 and rshift 1 and 0<>
      else
        false
      then
    ; define pixel@

    \ Draw a constant pixel on a bitmap
    :noname ( const dst-col dst-row op dst -- )
      swap case
        op-set of set-pixel-const endof
        op-or of or-pixel-const endof
        op-and of and-pixel-const endof
        op-bic of bic-pixel-const endof
        op-xor of xor-pixel-const endof
        ['] x-invalid-op ?raise
      endcase
    ; define draw-pixel-const

    \ Draw a constant rectangle on a bitmap
    :noname ( const start-dst-col start-dst-row cols rows op dst -- )
      swap case
        op-set of set-rect-const endof
        op-or of or-rect-const endof
        op-and of and-rect-const endof
        op-bic of bic-rect-const endof
        op-xor of xor-rect-const endof
        ['] x-invalid-op ?raise
      endcase
    ; define draw-rect-const

    \ Draw a rectangle on a bitmap from another bitmap
    :noname
      ( start-src-col start-src-row start-dst-col start-dst-row cols rows )
      ( op src dst -- )
      rot case
        op-set of set-rect endof
        op-or of or-rect endof
        op-and of and-rect endof
        op-bic of bic-rect endof
        op-xor of xor-rect endof
        ['] x-invalid-op ?raise
      endcase
    ; define draw-rect

  end-implement
  
end-module
