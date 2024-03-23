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

begin-module font

  oo import
  bitmap import

  \ Out of range character exception
  : x-out-of-range-char ( -- ) ." out of range character" cr ;

  \ 8-bit pixmaps are not implemented
  : x-pixmap8-not-available ( -- ) ." pixmap8 not available" cr ;
  
  \ 16-bit pixmaps are not implemented
  : x-pixmap16-not-available ( -- ) ." pixmap16 not available" cr ;

  \ Get the size of a font buffer in bytes for a given number or of columns and
  \ rows per character and a given minimum character index and a given maximum
  \ character index
  : font-buf-size { char-cols char-rows min-char max-char -- bytes }
    char-cols max-char min-char - 1+ * char-rows bitmap-buf-size
  ;

  \ The <font> class
  <object> begin-class <font>

    begin-module font-internal
      
      \ The backing bitmap
      <bitmap> class-size member font-bitmap

    end-module> import

    \ Number of columns per character
    cell member char-cols
    
    \ Number of rows per character
    cell member char-rows

    \ Minimum character index
    cell member min-char-index
    
    \ Maximum character index
    cell member max-char-index
    
    \ The default character index for un-included characters
    cell member default-char-index

    \ Set a row in a character
    method char-row! ( xn ... x0 row c font -- )

    \ Draw a character onto a bitmap
    method draw-char ( c col row op bitmap font -- )

    \ Draw a string onto a bitmap
    method draw-string ( c-addr u col row op bitmap font -- )

    \ Draw a character onto a 16-bit pixmap
    method draw-char-to-pixmap16 ( color c col row pixmap16 font -- )
    
    \ Draw a string onto a 16-bit pixmap
    method draw-string-to-pixmap16 ( color c-addr u col row pixmap16 font -- )

    \ Draw a character onto a 8-bit pixmap
    method draw-char-to-pixmap8 ( color c col row pixmap8 font -- )
    
    \ Draw a string onto a 8-bit pixmap
    method draw-string-to-pixmap8 ( color c-addr u col row pixmap8 font -- )

  end-class

  continue-module font-internal
    
    \ Get the column of a character
    : find-char-col { c self -- col }
      c self min-char-index @ - self char-cols @ *
    ;

  end-module
  
  \ Implement <font> class
  <font> begin-implement

    \ Our constructor
    :noname { buf default cols rows min-char max-char self -- }
      self <object>->new
      min-char self min-char-index !
      max-char self max-char-index !
      rows self char-rows !
      cols self char-cols !
      default self default-char-index !
      buf cols max-char min-char - 1+ * rows
      <bitmap> self font-bitmap init-object
    ; define new

    \ Set a row in a character
    :noname ( xn ... x0 row c self -- )
      { row c self }
      c self min-char-index @ u>= averts x-out-of-range-char
      c self max-char-index @ u<= averts x-out-of-range-char
      self char-cols @ 1- { col }
      begin col 0>= while
        dup 1 and if $FF else $00 then
        c self find-char-col col + row op-set self font-bitmap draw-pixel-const
        1 rshift
        self char-cols @ col - 32 umod 0= col 0<> and if drop then
        -1 +to col
      repeat
      drop
    ; define char-row!

    \ Draw a character onto a bitmap
    :noname { c col row op bitmap self -- }
      c self min-char-index @ u< if self default-char-index @ to c then
      c self max-char-index @ u> if self default-char-index @ to c then
      c self find-char-col 0 col row self char-cols @ self char-rows @
      op self font-bitmap bitmap draw-rect
    ; define draw-char

    \ Draw a string onto a bitmap
    :noname { c-addr u col row op bitmap self -- }
      u 0 ?do
        c-addr i + c@ col i self char-cols @ * + row op bitmap self draw-char
      loop
    ; define draw-string

    \ Draw a character onto a 16-bit pixmap
    :noname { color c col row pixmap self -- }
      [ defined? pixmap16 ] [if]
        c self min-char-index @ u< if self default-char-index @ to c then
        c self max-char-index @ u> if self default-char-index @ to c then
        color
        c self find-char-col 0 col row self char-cols @ self char-rows @
        self font-bitmap pixmap pixmap16::draw-rect-const-mask
      [else]
        ['] x-pixmap16-not-available ?raise
      [then]
    ; define draw-char-to-pixmap16
    
    \ Draw a string onto a 16-bit pixmap
    :noname { color c-addr u col row pixmap self -- }
      [ defined? pixmap16 ] [if]
        u 0 ?do
          color c-addr i + c@ col i self char-cols @ * + row pixmap self
          draw-char-to-pixmap16
        loop
      [else]
        ['] x-pixmap16-not-available ?raise
      [then]
    ; define draw-string-to-pixmap16
      
    [then]
    
    \ Draw a character onto a 8-bit pixmap
    :noname { color c col row pixmap self -- }
      [ defined? pixmap8 ] [if]
        c self min-char-index @ u< if self default-char-index @ to c then
        c self max-char-index @ u> if self default-char-index @ to c then
        color
        c self find-char-col 0 col row self char-cols @ self char-rows @
        self font-bitmap pixmap pixmap8::draw-rect-const-mask
      [else]
        ['] x-pixmap8-not-available ?raise
      [then]
    ; define draw-char-to-pixmap8
    
    \ Draw a string onto a 8-bit pixmap
    :noname { color c-addr u col row pixmap self -- }
      [ defined? pixmap8 ] [if]
        u 0 ?do
          color c-addr i + c@ col i self char-cols @ * + row pixmap self
          draw-char-to-pixmap8
        loop
      [else]
        ['] x-pixmap8-not-available ?raise
      [then]
    ; define draw-string-to-pixmap8
      
    [then]

  end-implement
    
end-module
