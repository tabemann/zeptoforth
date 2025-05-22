\ Copyright (c) 2023-2025 Travis Bemann
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

begin-module text8

  oo import

  \ Construct a 8-bit color from three 8-bit components
  : rgb8 { r g b -- color }
    b $FF and 6 rshift 6 lshift
    g $FF and 5 rshift 3 lshift or
    r $FF and 5 rshift or
  ;

  \ Get the size of a text buffer in bytes for a given number of columns and
  \ rows
  : text8-buf-size { cols rows -- bytes }
    cols rows * 3 * cols 8 align 3 rshift rows * + cell align
  ;
  
  \ Text grid with 8-bit color class
  <object> begin-class <text8>
    
    \ Number of columns in text grid
    cell member text-cols

    \ Number of rows in text grid
    cell member text-rows

    begin-module text8-internal

      \ Character buffer
      cell member text-char-buf

      \ Foreground color buffer
      cell member text-fg-color-buf

      \ Background color buffer
      cell member text-bk-color-buf

      \ Underline buffer
      cell member text-underline-buf

      \ Get the address of a character
      method char-addr ( col row text -- addr )

      \ Get the address of a foreground color
      method fg-color-addr ( col row text -- addr )

      \ Get the address of a background color
      method bk-color-addr ( col row text -- addr )

      \ Set the entire display to be dirty
      method set-dirty ( text -- )
      
      \ Set the entire text to not be dirty
      method clear-dirty ( text -- )
      
      \ Dirty a character
      method dirty-char ( col row text -- )

      \ Dirty an area
      method dirty-area ( start-col end-col start-row end-row text -- )

    end-module> import

    \ Get text dimensions
    method dim@ ( text -- cols rows )

    \ Clear the text
    method clear-text ( text -- )

    \ Get whether the text is dirty
    method dirty? ( text -- dirty? )

    \ Quickly write a whole character at once
    method whole-char! ( c fg-color bk-color underlined? col row text -- )
    
    \ Get a character
    method char@ ( col row text -- c )

    \ Set a character
    method char! ( c col row text -- )

    \ Get a character's foreground color
    method fg-color@ ( col row text -- color )

    \ Set a character's foreground color
    method fg-color! ( color col row text -- )

    \ Get a character's background color
    method bk-color@ ( col row text -- color )

    \ Set a character's background color
    method bk-color! ( color col row text -- )

    \ Get whether a character is underlined
    method underlined@ ( col row text -- underlined? )
    
    \ Get whether a character is underlined without testing for bounds
    method raw-underlined@ ( col row text -- underlined? )

    \ Set whether a character is underlined
    method underlined! ( underlined? col row text -- )

    \ Scroll up a number of lines (note that this will *not* fill in the new
    \ lines)
    method scroll-up ( lines text -- )

    \ Scroll down a number of lines (note that this will *not* fill in the new
    \ lines)
    method scroll-down ( lines text -- )
    
  end-class

  \ Implement the <text8> class
  <text8> begin-implement

    \ Constructor
    :noname { buf cols rows self -- }
      self <object>->new
      buf self text-char-buf !
      buf cols rows * + self text-fg-color-buf !
      buf cols rows * 2 * + self text-bk-color-buf !
      buf cols rows * 3 * + self text-underline-buf !
      cols self text-cols !
      rows self text-rows !
    ; define new

    \ Get the address of a character
    :noname { col row self -- addr }
      self text-char-buf @ self text-cols @ row * col + +
    ; define char-addr

    \ Get the address of a foreground color
    :noname { col row self -- addr }
      self text-fg-color-buf @ self text-cols @ row * col + +
    ; define fg-color-addr

    \ Get the address of a background color
    :noname { col row self -- addr }
      self text-bk-color-buf @ self text-cols @ row * col + +
    ; define bk-color-addr

    \ Set the entire display to be dirty
    :noname { self -- } ; define set-dirty
    
    \ Set the entire text to not be dirty
    :noname { self -- } ; define clear-dirty
    
    \ Dirty a character
    :noname { col row self -- } ; define dirty-char

    \ Dirty an area
    :noname { start-col end-col start-row end-row self -- }
    ; define dirty-area

    \ Get text dimensions
    :noname { self -- cols rows }
      self text-cols @ self text-rows @
    ; define dim@

    \ Clear the text
    :noname { self -- }
      self text-cols @ self text-rows @ * { chars }
      self text-char-buf @ chars $20 fill
      self text-fg-color-buf @ chars 0 fill
      self text-bk-color-buf @ chars 0 fill
      self text-underline-buf @
      self text-cols @ 8 align 3 rshift self text-rows @ * 0 fill
      self set-dirty
    ; define clear-text

    \ Get whether the text is dirty
    :noname { self -- dirty? } false ; define dirty?

    \ Quickly write a whole character at once
    :noname { c fg-color bk-color underlined? col row self -- }
      col 0>= row 0>= and
      col self text-cols @ < and row self text-rows @ < and if
        self text-cols @ row * col + { offset }
        c self text-char-buf @ offset + c!
        fg-color self text-fg-color-buf @ offset + c!
        bk-color self text-bk-color-buf @ offset + c!
        col $7 and bit
        self text-underline-buf @ col 3 rshift +
        self text-cols @ 8 align 3 rshift row * +
        underlined? if cbis! else cbic! then
        col row self dirty-char
      else
        0
      then
    ; define whole-char!

    \ Get a character
    :noname { col row self -- c }
      col 0>= row 0>= and
      col self text-cols @ < and row self text-rows @ < and if
        col row self char-addr c@
      else
        0
      then
    ; define char@

    \ Set a character
    :noname { c col row self -- }
      col 0>= row 0>= and
      col self text-cols @ < and row self text-rows @ < and if
        c col row self char-addr c!
        col row self dirty-char
      then
    ; define char!

    \ Get a character's foreground color
    :noname { col row self -- color }
      col 0>= row 0>= and
      col self text-cols @ < and row self text-rows @ < and if
        col row self fg-color-addr c@
      else
        0
      then
    ; define fg-color@

    \ Set a character's foreground color
    :noname { color col row self -- }
      col 0>= row 0>= and
      col self text-cols @ < and row self text-rows @ < and if
        color col row self fg-color-addr c!
        col row self dirty-char
      then
    ; define fg-color!

    \ Get a character's background color
    :noname { col row self -- color }
      col 0>= row 0>= and
      col self text-cols @ < and row self text-rows @ < and if
        col row self bk-color-addr c@
      else
        0
      then
    ; define bk-color@

    \ Set a character's background color
    :noname { color col row self -- }
      col 0>= row 0>= and
      col self text-cols @ < and row self text-rows @ < and if
        color col row self bk-color-addr c!
        col row self dirty-char
      then
    ; define bk-color!

    \ Get whether a character is underlined
    :noname { col row self -- underlined? }
      col 0>= row 0>= and
      col self text-cols @ < and row self text-rows @ < and if
        col row self raw-underlined@
      else
        false
      then
    ; define underlined@

    \ Get whether a character is underlined without testing for bounds
    :noname { col row self -- underlined? }
      col $7 and bit
      self text-underline-buf @ col 3 rshift +
      self text-cols @ 8 align 3 rshift row * +
      cbit@
    ; define raw-underlined@
    
    \ Set whether a character is underlined
    :noname { underlined? col row self -- }
      col 0>= row 0>= and
      col self text-cols @ < and row self text-rows @ < and if
        col $7 and bit
        self text-underline-buf @ col 3 rshift +
        self text-cols @ 8 align 3 rshift row * +
        underlined? if cbis! else cbic! then
        col row self dirty-char
      then
    ; define underlined!

    \ Scroll up a number of lines (note that this will *not* fill in the new
    \ lines)
    :noname { lines self -- }
      lines 0> lines self text-rows @ < and if
        lines self text-cols @ * { erased-chars }
        self text-cols @ self text-rows @ * erased-chars - { scrolled-chars }
        self text-char-buf @ dup erased-chars + swap scrolled-chars move
        self text-fg-color-buf @ dup erased-chars + swap scrolled-chars move
        self text-bk-color-buf @ dup erased-chars + swap scrolled-chars move
        self text-cols @ 8 align 3 rshift { underline-row-size }
        self text-underline-buf @ dup lines underline-row-size * + swap
        self text-rows @ lines - underline-row-size * move
        0 self text-cols @ 0 self text-rows @ lines - self dirty-area
      then
    ; define scroll-up

    \ Scroll down a number of lines (note that this will *not* fill in the new
    \ lines)
    :noname { lines self -- }
      lines 0> lines self text-rows @ < and if
        lines self text-cols @ * { erased-chars }
        self text-cols @ self text-rows @ * erased-chars - { scrolled-chars }
        self text-char-buf @ dup erased-chars + scrolled-chars move
        self text-fg-color-buf @ dup erased-chars + scrolled-chars move
        self text-bk-color-buf @ dup erased-chars + scrolled-chars move
        self text-cols @ 8 align 3 rshift { underline-row-size }
        self text-underline-buf @ dup lines underline-row-size * +
        self text-rows @ lines - underline-row-size * move
        0 self text-cols @ lines self text-rows @ self dirty-area
      then
    ; define scroll-down

  end-implement
  
end-module
