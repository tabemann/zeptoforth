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

begin-module font

  oo import
  bitmap import

  \ Out of range character exception
  : x-out-of-range-char ( -- ) ." out of range character" cr ;

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

    \ Set a character to a bitmap
    method set-char ( c col row bitmap font -- )

    \ Or a character to a bitmap
    method or-char ( c col row bitmap font -- )

    \ And a character to a bitmap
    method and-char ( c col row bitmap font -- )

    \ Bit-clear a character to a bitmap
    method bic-char ( c col row bitmap font -- )

    \ Exclusive-or a character to a bitmap
    method xor-char ( c col row bitmap font -- )

    \ Set a string to a bitmap
    method set-string ( c-addr u col row bitmap font -- )

    \ Or a string to a bitmap
    method or-string ( c-addr u col row bitmap font -- )

    \ And a string to a bitmap
    method and-string ( c-addr u col row bitmap font -- )

    \ Bit-clear a string to a bitmap
    method bic-string ( c-addr u col row bitmap font -- )

    \ Exclusive-or a string to a bitmap
    method xor-string ( c-addr u col row bitmap font -- )
    
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
        c self find-char-col col + row self font-bitmap set-pixel-const
        1 rshift
        self char-cols @ col - 32 umod 0= col 0<> and if drop then
        -1 +to col
      repeat
      drop
    ; define char-row!

    \ Set a character to a bitmap
    :noname { c col row bitmap self -- }
      c self min-char-index @ u< if self default-char-index @ to c then
      c self max-char-index @ u> if self default-char-index @ to c then
      c self find-char-col col self char-cols @
      0 row self char-rows @
      self font-bitmap bitmap set-rect
    ; define set-char

    \ Or a character to a bitmap
    :noname { c col row bitmap self -- }
      c self min-char-index @ u< if self default-char-index @ to c then
      c self max-char-index @ u> if self default-char-index @ to c then
      c self find-char-col col self char-cols @
      0 row self char-rows @
      self font-bitmap bitmap or-rect
    ; define or-char

    \ And a character to a bitmap
    :noname { c col row bitmap self -- }
      c self min-char-index @ u< if self default-char-index @ to c then
      c self max-char-index @ u> if self default-char-index @ to c then
      c self find-char-col col self char-cols @
      0 row self char-rows @
      self font-bitmap bitmap and-rect
    ; define and-char

    \ Bit-clear a character to a bitmap
    :noname { c col row bitmap self -- }
      c self min-char-index @ u< if self default-char-index @ to c then
      c self max-char-index @ u> if self default-char-index @ to c then
      c self find-char-col col self char-cols @
      0 row self char-rows @
      self font-bitmap bitmap bic-rect
    ; define bic-char

    \ Exclusive-or a character to a bitmap
    :noname { c col row bitmap self -- }
      c self min-char-index @ u< if self default-char-index @ to c then
      c self max-char-index @ u> if self default-char-index @ to c then
      c self find-char-col col self char-cols @
      0 row self char-rows @
      self font-bitmap bitmap xor-rect
    ; define xor-char

    \ Set a string to a bitmap
    :noname { c-addr u col row bitmap self -- }
      u 0 ?do
        c-addr i + c@ col i self char-cols @ * + row bitmap self set-char
      loop
    ; define set-string

    \ Or a string to a bitmap
    :noname { c-addr u col row bitmap self -- }
      u 0 ?do
        c-addr i + c@ col i self char-cols @ * + row bitmap self or-char
      loop
    ; define or-string

    \ And a string to a bitmap
    :noname { c-addr u col row bitmap self -- }
      u 0 ?do
        c-addr i + c@ col i self char-cols @ * + row bitmap self and-char
      loop
    ; define and-string

    \ Bit-clear a string to a bitmap
    :noname { c-addr u col row bitmap self -- }
      u 0 ?do
        c-addr i + c@ col i self char-cols @ * + row bitmap self bic-char
      loop
    ; define bic-string

    \ Exclusive-or a string to a bitmap
    :noname { c-addr u col row bitmap self -- }
      u 0 ?do
        c-addr i + c@ col i self char-cols @ * + row bitmap self xor-char
      loop
    ; define xor-string

  end-implement
    
end-module
