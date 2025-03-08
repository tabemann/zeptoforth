\ Copyright (c) 2023-2024 Travis Bemann
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

begin-module pixmap16

  oo import
  armv6m import

  \ Construct a 16-bit color from three 8-bit components
  : rgb16 { r g b -- color }
    b 3 rshift $1F and 8 lshift
    g 2 rshift $3F and dup 3 rshift $7F and swap $7 and 13 lshift or or
    r 3 rshift $1F and 3 lshift or
  ;

  \ Get the size of a pixmap buffer in bytes for a given number of columns and
  \ rows
  : pixmap16-buf-size ( cols rows -- bytes ) * 1 lshift cell align ;

  \ The <pixmap16> class
  <object> begin-class <pixmap16>

    \ Number of columns in pixmap
    cell member pixmap-cols

    \ Number of rows in pixmap
    cell member pixmap-rows

    begin-module pixmap16-internal
      
      \ Framebuffer
      cell member pixmap-buf
      
      \ Get the address of a pixel
      method pixel-addr ( col row pixmap -- addr )
      
      \ Set the entire display to be dirty
      method set-dirty ( pixmap -- )
      
      \ Set the entire pixmap to not be dirty
      method clear-dirty ( pixmap -- )
      
      \ Dirty a pixel on a pixmap
      method dirty-pixel ( col row pixmap -- )

      \ Dirty an area on a pixmap
      method dirty-area ( start-col end-col start-row end-row pixmap -- )

    end-module> import

    \ Get pixmap dimensions
    method dim@ ( pixmap -- cols rows )
    
    \ Clear the pixmap
    method clear-pixmap ( pixmap -- )

    \ Get whether a pixmap is dirty
    method dirty? ( pixmap -- dirty? )

    \ Get the color of a pixel
    method pixel@ ( col row pixmap -- color )

    \ Draw a constant pixel on a pixamp
    method draw-pixel-const ( color dst-col dst-row dst-pixemap -- )

    \ Draw a constant rectangle on a pixmap
    method draw-rect-const
    ( color start-dst-col start-dst-row cols rows dst-pixmap -- )

    \ Draw a rectangle on a pixmap from another pixmap
    method draw-rect
    ( start-src-col start-src-row start-dst-col start-dst-row cls )
    ( rows src-pixmap dst-pixmap -- )

    \ Draw a rectangle on a pixmap from a bitmap where 1 bits are given a color
    method draw-rect-const-mask
    ( color start-mask-col start-mask-row start-dst-col start-dst-row cols )
    ( rows mask-bitmap dst-pixmap -- )

    \ Draw a rectangle on a pixmap from another pixmap using a bitmap as a mask
    method draw-rect-mask
    ( start-mask-col start-mask-row start-src-col start-src-row start-dst-col )
    ( start-dst-row cols rows mask-bitmap src-pixmap dst-pixmap -- )
    
  end-class

  <pixmap16> begin-implement
    
    \ Initialize a pixmap
    :noname { buf cols rows self -- }
      self <object>->new
      rows self pixmap-rows !
      cols self pixmap-cols !
      buf self pixmap-buf  !
      self clear-pixmap
    ; define new
    
    \ Set the entire pixmap to be dirty
    :noname ( pixmap -- ) drop ; define set-dirty

    \ Set the entire pixmap to not be dirty
    :noname ( pixmap -- ) drop ; define clear-dirty
    
    \ Get whether a pixmap is dirty
    :noname ( pixmap -- dirty? ) drop true ; define dirty?

    \ Get pixmap dimensions
    :noname ( pixmap -- cols rows )
      dup pixmap-cols @ swap pixmap-rows @
    ; define dim@
    
    \ Clear the pixmap
    :noname { self -- }
      self set-dirty
      self pixmap-buf @
      self pixmap-rows @ self pixmap-cols @ pixmap16-buf-size $00 fill
    ; define clear-pixmap

    \ Get the address of a pixel
    :noname { col row self -- addr }
      self pixmap-buf @ self pixmap-cols @ row * col + 1 lshift +
    ; define pixel-addr

    \ Dirty a pixel on a pixmap
    :noname ( col row pixmap -- ) drop 2drop ; define dirty-pixel

    \ Dirty an area on a pixmap
    :noname ( start-col end-col start-row end-row pixmap -- )
      drop 2drop 2drop
    ; define dirty-area

    continue-module pixmap16-internal

      \ Set a strip to a color in a pixmap
      : set-strip-const
        { color dst-col dst-row col-count self -- }
        dst-col dst-row self pixel-addr { addr }
        code[
        0 r0 ldr_,[sp,#_] \ addr
        8 r1 ldr_,[sp,#_] \ col-count
        20 r2 ldr_,[sp,#_] \ color
        mark<
        0 r1 cmp_,#_
        le bc>
        0 r0 r2 strh_,[_,#_]
        2 r0 adds_,#_
        1 r1 subs_,#_
        2swap b<
        >mark
        ]code
      ;

      \ Copy a strip from one pixmap to another
      : set-strip
        { src-col dst-col src-row dst-row col-count src dst -- }
        src-col src-row src pixel-addr { src-addr }
        dst-col dst-row dst pixel-addr { dst-addr }
        code[
        0 r0 ldr_,[sp,#_] \ dst-addr
        4 r1 ldr_,[sp,#_] \ src-addr
        16 r2 ldr_,[sp,#_] \ col-count
        mark<
        0 r2 cmp_,#_
        le bc>
        0 r1 r3 ldrh_,[_,#_]
        0 r0 r3 strh_,[_,#_]
        2 r0 adds_,#_
        2 r1 adds_,#_
        1 r2 subs_,#_
        2swap b<
        >mark
        ]code
      ;

      \ Set a strip to a color in a pixmap for all the 1 bits in a bitmap
      : set-strip-const-mask
        { color mask-col dst-col mask-row dst-row col-count mask dst -- }
        mask-row 3 rshift mask
        bitmap::bitmap-internal::page-addr mask-col + { mask-addr }
        dst-col dst-row dst pixel-addr { dst-addr }
        code[
        r4 r5 2 push
        8 r0 ldr_,[sp,#_] \ dst-addr
        12 r1 ldr_,[sp,#_] \ mask-addr
        24 r2 ldr_,[sp,#_] \ col-count
        32 r3 ldr_,[sp,#_] \ mask-row
        7 r4 movs_,#_
        r4 r3 ands_,_
        mark<
        0 r2 cmp_,#_
        le bc>
        0 r1 r4 ldrb_,[_,#_]
        r3 r4 lsrs_,_
        1 r5 movs_,#_
        r4 r5 tst_,_
        eq bc>
        44 r4 ldr_,[sp,#_] \ color
        0 r0 r4 strh_,[_,#_]
        >mark
        2 r0 adds_,#_
        1 r1 adds_,#_
        1 r2 subs_,#_
        2swap b<
        >mark
        r4 r5 2 pop
        ]code
      ;

      \ Copy a strip from one pixmap to another for all the 1 bits in a bitmap
      : set-strip-mask
        { mask-col src-col dst-col mask-row src-row dst-row col-count mask src dst -- }
        mask-row 3 rshift mask
        bitmap::bitmap-internal::page-addr mask-col + { mask-addr }
        src-col src-row src pixel-addr { src-addr }
        dst-col dst-row dst pixel-addr { dst-addr }
        dup
        code[
        r4 r5 2 push
        8 r0 ldr_,[sp,#_] \ dst-addr
        12 r1 ldr_,[sp,#_] \ src-addr
        16 r2 ldr_,[sp,#_] \ mask-addr
        32 r3 ldr_,[sp,#_] \ col-count
        44 r4 ldr_,[sp,#_] \ mask-row
        7 r5 movs_,#_
        r5 r4 ands_,_
        mark<
        0 r3 cmp_,#_
        le bc>
        0 r2 r5 ldrb_,[_,#_]
        r4 r5 lsrs_,_
        1 r6 movs_,#_
        r5 r6 tst_,_
        eq bc>
        0 r1 r5 ldrh_,[_,#_] \ color
        0 r0 r5 strh_,[_,#_]
        >mark
        2 r0 adds_,#_
        2 r1 adds_,#_
        1 r2 adds_,#_
        1 r3 subs_,#_
        2swap b<
        >mark
        r4 r5 2 pop
        ]code
        drop
      ;          

    end-module

    \ Get the color of a pixel
    :noname { col row self -- color }
      0 col <= 0 row <= and
      col self pixmap-cols @ < row self pixmap-rows @ < and and if
        col row self pixel-addr h@
      else
        0
      then
    ; define pixel@

    \ Draw a pixel with a constant value
    :noname { color dst-col dst-row dst -- }
      0 dst-col <= 0 dst-row <= and
      dst-col dst pixmap-cols @ < dst-row dst pixmap-rows @ < and and if
        color dst-col dst-row dst pixel-addr h!
        dst-col dst-row dst dirty-pixel
      then
    ; define draw-pixel-const

    \ Draw a constant rectangle on a pixmap
    :noname
      { dst }
      dst dim@ clip::clip-dst-only
      { color dst-col dst-row cols rows }
      rows 0 ?do
        color dst-col dst-row i + cols dst set-strip-const
      loop
      dst-col dup cols + dst-row dup rows + dst dirty-area
    ; define draw-rect-const

    \ Draw a rectangle on a pixmap from another pixmap
    :noname
      { src dst }
      src dim@ dst dim@ clip::clip-src-dst
      { src-col src-row dst-col dst-row cols rows }
      rows 0 ?do
        src-col dst-col src-row i + dst-row i + cols src dst set-strip
      loop
      dst-col dup cols + dst-row dup rows + dst dirty-area
    ; define draw-rect

    \ Draw a rectangle on a pixmap from a bitmap where 1 bits are given a color
    :noname
      { mask dst }
      mask bitmap::dim@ dst dim@ clip::clip-src-dst
      { color mask-col mask-row dst-col dst-row cols rows }
      rows 0 ?do
        color mask-col dst-col mask-row i + dst-row i +
        cols mask dst set-strip-const-mask
      loop
      dst-col dup cols + dst-row dup rows + dst dirty-area
    ; define draw-rect-const-mask
    
    \ Draw a rectangle on a pixmap from another pixmap using a bitmap as a mask
    :noname
      { mask src dst }
      mask bitmap::dim@ clip::clip-mask
      src dim@ clip::clip-src-w/-mask
      dst dim@ clip::clip-dst-w/-mask
      { mask-col mask-row src-col src-row dst-col dst-row cols rows }
      rows 0 ?do
        mask-col src-col dst-col
        mask-row i + src-row i +  dst-row i +
        cols mask src dst set-strip-mask
      loop
      dst-col dup cols + dst-row dup rows + dst dirty-area
    ; define draw-rect-mask

  end-implement

end-module
