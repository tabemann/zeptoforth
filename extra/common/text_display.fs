\ Copyright (c) 2024 Travis Bemann
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

begin-module text-display

  oo import
  bitmap import
  font import
  font-internal import

  \ Get the size of a buffer for text
  : text-buf-size { cols rows char-cols char-rows -- bytes }
    cols char-cols / rows char-rows / * cell align
  ;

  \ Get the size of an inverted video buffer for text
  : invert-buf-size { cols rows char-cols char-rows -- bytes }
    cols char-cols / rows char-rows / 8 align 8 / * cell align
  ;

  \ Text display superclass
  <object> begin-class <text-display>

    begin-module text-display-internal

      \ Text display buffer
      cell member text-display-text-buf

      \ Text display inverted video bitmap buffer
      cell member text-display-invert-buf
      
      \ Text display font
      cell member text-display-font
      
      \ Text display columns
      cell member text-display-cols

      \ Text display rows
      cell member text-display-rows

      \ Text display dirty start column
      cell member text-display-dirty-start-col

      \ Text display dirty start row
      cell member text-display-dirty-start-row

      \ Text display dirty end column
      cell member text-display-dirty-end-col

      \ Text display dirty end row
      cell member text-display-dirty-end-row

    end-module> import

    \ Clear the display
    method clear-display ( self -- )

    \ Set the entire display as dirty
    method set-dirty ( self -- )
    
    \ Set a character as dirty
    method dirty-char ( col row self -- )
    
    \ Get whether a display is dirty
    method dirty? ( self -- dirty? )

    \ Get the display's dirtyr rectangle in pixels
    method dirty-rect@ ( self -- start-col start-row end-col end-row )
    
    \ Clear the entire display's dirty state
    method clear-dirty ( self -- )

    \ Get the display dimensions in characters
    method dim@ ( self -- cols rows )

    \ Set a character
    method char! ( c col row self -- )

    \ Get a character
    method char@ ( col row self -- c )

    \ Set a string
    method string! ( c-addr u col row self -- )

    \ Set inverted video
    method invert! ( invert? col row self -- )

    \ Toggle inverted video
    method toggle-invert! ( col row self -- )
    
    \ Get inverted video
    method invert@ ( col row self -- invert? )

    \ Get the state of a pixel
    method pixel@ ( pixel-col pixel-row self -- pixel-set? )

  end-class

  <text-display> begin-implement

    \ Constructor
    :noname { text-buf invert-buf font cols rows self -- }
      self <object>->new
      text-buf self text-display-text-buf !
      invert-buf self text-display-invert-buf !
      font self text-display-font !
      cols font char-cols @ / self text-display-cols !
      rows font char-rows @ / self text-display-rows !
      self clear-display
    ; define new

    \ Clear the display
    :noname { self -- }
      self text-display-rows @ 0 ?do
        self text-display-cols @ 0 ?do
          $20 i j self char!
          false i j self invert!
        loop
      loop
      self set-dirty
    ; define clear-display
    
    \ Set the entire display as dirty
    :noname { self -- }
      0 self text-display-dirty-start-col !
      0 self text-display-dirty-start-row !
      self text-display-cols @ self text-display-dirty-end-col !
      self text-display-rows @ self text-display-dirty-end-row !
    ; define set-dirty

    \ Set a character as dirty
    :noname { col row self -- }
      self dirty? if
        self text-display-dirty-start-col @ col min
        self text-display-dirty-start-col !
        self text-display-dirty-start-row @ row min
        self text-display-dirty-start-row !
        self text-display-dirty-end-col @ col 1+ max
        self text-display-dirty-end-col !
        self text-display-dirty-end-row @ row 1+ max
        self text-display-dirty-end-row !
      else
        col self text-display-dirty-start-col !
        row self text-display-dirty-start-row !
        col 1+ self text-display-dirty-end-col !
        row 1+ self text-display-dirty-end-row !
      then
    ; define dirty-char

    \ Get whether a display is dirty
    :noname { self -- dirty? }
      self text-display-dirty-start-col @ self text-display-dirty-end-col @ <>
      self text-display-dirty-start-row @ self text-display-dirty-end-row @ <>
      or
    ; define dirty?

    \ Get the display's dirty rectangle in pixels
    :noname { self -- start-col start-row end-col end-row }
      self text-display-font @ { font }
      font char-cols @ { font-cols }
      font char-rows @ { font-rows }
      self text-display-dirty-start-col @ font-cols *
      self text-display-dirty-start-row @ font-rows *
      self text-display-dirty-end-col @ font-cols *
      self text-display-dirty-end-row @ font-rows *
    ; define dirty-rect@

    \ Clear the entire display's dirty state
    :noname { self -- }
      0 self text-display-dirty-start-col !
      0 self text-display-dirty-start-row !
      0 self text-display-dirty-end-col !
      0 self text-display-dirty-end-row !
    ; define clear-dirty
    
    \ Get the display dimensions in characters
    :noname { self -- cols rows }
      self text-display-cols @ self text-display-rows @
    ; define dim@

    \ Set a character
    :noname { c col row self -- }
      col 0< col self text-display-cols @ >= or
      row 0< or row self text-display-rows @ >= or if exit then
      c row self text-display-cols @ * col + self text-display-text-buf @ + c!
      col row self dirty-char
    ; define char!

    \ Get a character
    :noname { col row self -- c }
      col 0< col self text-display-cols @ >= or
      row 0< or row self text-display-rows @ >= or if 0 exit then
      row self text-display-cols @ * col + self text-display-text-buf @ + c@
    ; define char@

    \ Set a string
    :noname { c-addr u col row self -- }
      u 0 ?do c-addr i + c@ col i + row self char! loop
    ; define string!

    \ Set inverted video
    :noname { invert? col row self -- }
      col 0< col self text-display-cols @ >= or
      row 0< or row self text-display-rows @ >= or if exit then
      row $7 and bit
      self text-display-invert-buf @
      self text-display-cols @ row 3 rshift * + col +
      invert? if cbis! else cbic! then
      col row self dirty-char
    ; define invert!

    \ Toggle inverted video
    :noname { col row self -- }
      col 0< col self text-display-cols @ >= or
      row 0< or row self text-display-rows @ >= or if exit then
      row $7 and bit
      self text-display-invert-buf @
      self text-display-cols @ row 3 rshift * + col + cxor!
      col row self dirty-char
    ; define toggle-invert!
    
    \ Get inverted video
    :noname { col row self -- invert? }
      col 0< col self text-display-cols @ >= or
      row 0< or row self text-display-rows @ >= or if false exit then
      self text-display-invert-buf @
      self text-display-cols @ row 3 rshift * + col + c@
      row $7 and bit and 0<>
    ; define invert@

    \ Get the state of a pixel - note that this does no validation
    :noname { pixel-col pixel-row self -- pixel-set? }

      self text-display-font @ { font }
      font font-bitmap { font-bitmap }
      font char-cols @ { char-cols }
      pixel-col char-cols /mod { font-pixel-col char-col }
      pixel-row font char-rows @ /mod { font-pixel-row char-row }

      char-row self text-display-cols @ * char-col +
      self text-display-text-buf @ + c@
      font min-char-index @ - char-cols *
      
      font-bitmap bitmap-internal::bitmap-buf @
      font-bitmap bitmap-cols @ font-pixel-row 3 rshift * +
      font-pixel-col + + c@
      font-pixel-row 7 and rshift 1 and 0<>
      
      self text-display-invert-buf @
      self text-display-cols @ char-row 3 rshift * + char-col + c@
      char-row 7 and rshift 1 and 0<> xor
    ; define pixel@

  end-implement
  
end-module
