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

    \ Get the state of a pixel
    method pixel@ ( col row bitmap -- state )
      
    \ Set a pixel with a constant value
    method set-pixel-const ( const dst-col dst-row dst-bitmap -- )
    
    \ Or a pixel with a constant value
    method or-pixel-const ( const dst-col dst-row dst-bitmap -- )

    \ And a pixel with a constant value
    method and-pixel-const ( const dst-col dst-row dst-bitmap -- )

    \ Bit-clear a pixel with a constant value
    method bic-pixel-const ( const dst-col dst-row dst-bitmap -- )

    \ Exclusive-or a pixel with a constant value
    method xor-pixel-const ( const dst-col dst-row dst-bitmap -- )

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

      \ Clip a destination-only rectangle
      : clip-dst-only
        { dst-col col-count dst-row row-count dst --
        new-dst-col new-col-count new-dst-row new-row-count }
        dst-col 0 < if
          dst-col col-count + 0>= if
            dst-col +to col-count
          else
            0 to col-count
          then
          0 to dst-col
        then
        dst-col dst bitmap-cols @ < if
          dst-col col-count + dst bitmap-cols @ > if
            dst bitmap-cols @ dst-col col-count + - +to col-count
          then
        else
          0 to col-count
          dst bitmap-cols @ to dst-col
        then
        dst-row 0 < if
          dst-row row-count + 0>= if
            dst-row +to row-count
          else
            0 to row-count
          then
          0 to dst-row
        then
        dst-row dst bitmap-rows @ < if
          dst-row row-count + dst bitmap-rows @ > if
            dst bitmap-rows @ dst-row row-count + - +to row-count
          then
        else
          0 to row-count
          dst bitmap-rows @ to dst-row
        then
        dst-col col-count dst-row row-count
      ;

      \ Clip a rectangle to the source dimensions
      : clip-src
        { src-col dst-col col-count src-row dst-row row-count src --
        new-src-col new-dst-col new-col-count new-src-row new-dst-row
        new-row-count }
        src-col 0 < if
          src-col negate +to dst-col
          src-col col-count + 0>= if
            src-col +to col-count
          else
            0 to col-count
          then
          0 to src-col
        then
        src-col src bitmap-cols @ < if
          src-col col-count + src bitmap-cols @ > if
            src bitmap-cols @ src-col col-count + - +to col-count
          then
        else
          0 to row-count
          src bitmap-cols @ to dst-col
        then
        src-row 0 < if
          src-row negate +to dst-row
          src-row row-count + 0>= if
            src-row +to row-count
          else
            0 to row-count
          then
          0 to src-row
        then
        src-row src bitmap-rows @ < if
          src-row row-count + src bitmap-rows @ > if
            src bitmap-rows @ src-row row-count + - +to row-count
          then
        else
          0 to row-count
          src bitmap-rows @ to dst-row
        then
        src-col dst-col col-count src-row dst-row row-count
      ;

      \ Clip a rectangle to the destination dimensions
      : clip-dst
        { src-col dst-col col-count src-row dst-row row-count dst --
        new-src-col new-dst-col new-col-count new-src-row new-dst-row
        new-row-count }
        dst-col 0 < if
          dst-col negate +to src-col
          dst-col col-count + 0>= if
            dst-col +to col-count
          else
            0 to col-count
          then
          0 to dst-col
        then
        dst-col dst bitmap-cols @ < if
          dst-col col-count + dst bitmap-cols @ > if
            dst bitmap-cols @ dst-col col-count + - +to col-count
          then
        else
          0 to row-count
          dst bitmap-cols @ to src-col
        then
        dst-row 0 < if
          dst-row negate +to src-row
          dst-row row-count + 0>= if
            dst-row +to row-count
          else
            0 to row-count
          then
          0 to dst-row
        then
        dst-row dst bitmap-rows @ < if
          dst-row row-count + dst bitmap-rows @ > if
            dst bitmap-rows @ dst-row row-count + - +to row-count
          then
        else
          0 to row-count
          dst bitmap-rows @ to src-row
        then
        src-col dst-col col-count src-row dst-row row-count
      ;

      \ Clip a rectangle
      : clip
        ( start-src-col start-dst-col col-count start-src-row start-dst-row )
        ( row-count src-bitmap dst-bitmap -- )
        { src dst } src clip-src dst clip-dst
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

    \ Set a pixel with a constant value
    :noname { const dst-col dst-row dst -- }
      0 dst-col <= 0 dst-row <= and
      dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
        dst-col dst-row dst dirty-pixel
        const dst-row 1 1 dst-col dst set-strip-const
      then
    ; define set-pixel-const
    
    \ Or a pixel with a constant value
    :noname { const dst-col dst-row dst -- }
      0 dst-col <= 0 dst-row <= and
      dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
        dst-col dst-row dst dirty-pixel
        const dst-row 1 1 dst-col dst or-strip-const
      then
    ; define or-pixel-const

    \ And a pixel with a constant value
    :noname { const dst-col dst-row dst -- }
      0 dst-col <= 0 dst-row <= and
      dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
        dst-col dst-row dst dirty-pixel
        const dst-row 1 1 dst-col dst and-strip-const
      then
    ; define and-pixel-const

    \ Bit-clear a pixel with a constant value
    :noname { const dst-col dst-row dst -- }
      0 dst-col <= 0 dst-row <= and
      dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
        dst-col dst-row dst dirty-pixel
        const dst-row 1 1 dst-col dst bic-strip-const
      then
    ; define bic-pixel-const

    \ Exclusive-or a pixel with a constant value
    :noname { const dst-col dst-row dst -- }
      0 dst-col <= 0 dst-row <= and
      dst-col dst bitmap-cols @ < dst-row dst bitmap-rows @ < and and if
        dst-col dst-row dst dirty-pixel
        const dst-row 1 1 dst-col dst xor-strip-const
      then
    ; define xor-pixel-const
    
    \ Set a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      { dst } dst clip-dst-only
      { const dst-col col-count dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      const dst-col col-count dst-row row-count dst
      ['] set-strip-const blit-const
    ; define set-rect-const

    \ Or a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      { dst } dst clip-dst-only
      { const dst-col col-count dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      const dst-col col-count dst-row row-count dst
      ['] or-strip-const blit-const
    ; define or-rect-const

    \ And a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      { dst } dst clip-dst-only
      { const dst-col col-count dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      const dst-col col-count dst-row row-count dst
      ['] and-strip-const blit-const
    ; define and-rect-const

    \ Bit-clear a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      { dst } dst clip-dst-only
      { const dst-col col-count dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      const dst-col col-count dst-row row-count dst
      ['] bic-strip-const blit-const
    ; define bic-rect-const

    \ Exclusive-or a rectangle with a constant value
    :noname
      ( const start-dst-col col-count start-dst-row row-count dst-bitmap -- )
      { dst } dst clip-dst-only
      { const dst-col col-count dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      const dst-col col-count dst-row row-count dst
      ['] xor-strip-const blit-const
    ; define xor-rect-const

    \ Set a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      { src dst } src dst clip
      { src-col dst-col col-count src-row dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      src-col dst-col col-count src-row dst-row row-count src dst
      ['] set-strip blit
    ; define set-rect
    
    \ Or a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      { src dst } src dst clip
      { src-col dst-col col-count src-row dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      src-col dst-col col-count src-row dst-row row-count src dst
      ['] or-strip blit
    ; define or-rect
    
    \ And a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      { src dst } src dst clip
      { src-col dst-col col-count src-row dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      src-col dst-col col-count src-row dst-row row-count src dst
      ['] and-strip blit
    ; define and-rect

    \ Bit-clear a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      { src dst } src dst clip
      { src-col dst-col col-count src-row dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      src-col dst-col col-count src-row dst-row row-count src dst
      ['] bic-strip blit
    ; define bic-rect

    \ Exclusive-or a rectangle
    :noname
      ( start-src-col start-dst-col col-count start-src-row start-dst-row )
      ( row-count src-bitmap dst-bitmap -- )
      { src dst } src dst clip
      { src-col dst-col col-count src-row dst-row row-count }
      dst-col dup col-count + dst-row dup row-count + dst dirty-area
      src-col dst-col col-count src-row dst-row row-count src dst
      ['] xor-strip blit
    ; define xor-rect

  end-implement
  
end-module
