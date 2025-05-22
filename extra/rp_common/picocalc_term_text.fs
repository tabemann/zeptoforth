\ Copyright (c) 2025 Travis Bemann
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

begin-module picocalc-term

  oo import
  picocalc-term-common import
  picocalc-term-common-internal import
  text8 import
  font import
  console import

  use-st7789v? not [if]
    ili9488-text-common import
    ili9488-text-spi import
  [else]
    st7789v-text-common import
    st7789v-text-spi import
  [then]

  use-5x8-font? [if]
    simple-font-5x8 import
  [then]
  use-6x8-font? [if]
    simple-font-6x8 import
  [then]
  use-7x8-font? [if]
    simple-font import
  [then]

  \ PicoCalc terminal class
  <picocalc-term-common> begin-class <picocalc-term>

    begin-module picocalc-term-internal
      
      \ The display
      use-st7789v? not [if] <ili9488-text-spi> [else] <st7789v-text-spi> [then]
      class-size member display-intf
      
      \ The display buffer
      term-width term-height text8-buf-size member display-buf

    end-module> import
    
  end-class

  \ Implement PicoCalc terminal class
  <picocalc-term> begin-implement

    \ Constructor
    :noname { self -- }
      self <picocalc-term-common>->new

      [ use-5x8-font? ] [if] a-simple-font-5x8 [then]
      [ use-6x8-font? ] [if] a-simple-font-6x8 [then]
      [ use-7x8-font? ] [if] a-simple-font [then] { the-font }

      [ use-st7789v? not ] [if]
        display-spi-tx-pin display-spi-sck-pin display-dc-pin display-spi-cs-pin
        display-rst-pin display-invert
        the-font self display-buf term-width term-height
        display-width display-height
        display-spi-device <ili9488-text-spi> self display-intf init-object
      [else]
        display-spi-tx-pin display-spi-sck-pin display-dc-pin display-spi-cs-pin
        display-bl-pin display-rst-pin
        the-font self display-buf false term-width term-height
        display-width display-height
        display-spi-device <st7789v-text-spi> self display-intf init-object
      [then]
    ; define new
    
    \ Initialize the PicoCalc terminal
    :noname { self -- }
      self <picocalc-term-common>->init-term
      self display-intf clear-display
      self display-intf clear-text
      self draw-cursor
      self display-intf update-display
      self start-term-task
    ; define init-term

    \ Update the display
    :noname ( self -- ) display-intf update-display ; define do-update-display
    
    \ Draw a character with or without a cursor
    :noname { cursor? x y self -- }
      x term-width > y term-height > or if exit then
      term-width y * x + { offset }
      offset self chars-buf + c@ { c }
      offset self attrs-buf + c@ { attr }
      c 0= if bl to c then
      offset self fg-colors-buf + c@
      offset self bk-colors-buf + c@
      x self cursor-x @ = y self cursor-y @ = and
      self cursor-visible @ and cursor? and if swap then
      attr attr-reverse and if swap then
      get-color swap attr modify-color get-color
      swap c -rot
      attr attr-underline and 0<>
      x y self display-intf whole-char!
    ; define draw-char-with-cursor

    \ Scroll up by a number of lines
    :noname { lines self -- }
      lines self display-intf text8::scroll-up
      term-height dup lines - ?do
        term-width 0 ?do $20 0 0 false i j self display-intf whole-char! loop
      loop
    ; define scroll-up

    \ Scroll down by a number of lines
    :noname { lines self -- }
      lines self display-intf text8::scroll-down
      lines 0 ?do
        term-width 0 ?do $20 0 0 false i j self display-intf whole-char! loop
      loop
    ; define scroll-down

  end-implement
  
  \ The PicoCalc terminal
  <picocalc-term> class-size buffer: shared-term

  continue-module picocalc-term-internal
    
    \ Initialize the PicoCalc terminal
    : init-shared-term ( -- )
      <picocalc-term> shared-term init-object
      shared-term init-term
    ;
    initializer init-shared-term

  end-module

  \ Set the current input to a PicoCalc terminal within an xt
  : with-term-input ( xt -- )
    [: shared-term >term ;] [: shared-term >term? ;] rot with-input
  ;

  \ Set the current output to a PicoCalc terminal within an xt
  : with-term-output ( xt -- )
    [: shared-term term> ;] swap
    [: shared-term term>? ;] swap
    [: shared-term flush-term ;] swap with-output
    shared-term flush-term
  ;

  \ Set the current error output to a PicoCalc terminal within an xt
  : with-term-error-output ( xt -- )
    [: shared-term term> ;] swap
    [: shared-term term>? ;] swap
    [: shared-term flush-term ;] swap with-error-output
    shared-term flush-term
  ;

  \ Set the current console to a PicoCalc terminal
  : term-console ( -- )
    [: shared-term term> ;] key-hook !
    [: shared-term term>? ;] key?-hook !
    [: shared-term >term ;] emit-hook !
    [: shared-term >term? ;] emit?-hook !
    [: shared-term flush-term ;] flush-console-hook !
    [: shared-term >term ;] error-emit-hook !
    [: shared-term >term? ;] error-emit?-hook !
    [: shared-term flush-term ;] error-flush-console-hook !
    picocalc-welcome-displayed not if
      picocalc-welcome
      true to picocalc-welcome-displayed
    then
  ;
  
  \ Carry out an operation with the PicoCalc terminal locked. Note that
  \ executing operations that print to the PicoCalc terminal should be avoided
  \ because they may block indefinitely.
  : with-term-lock ( xt -- ) shared-term do-with-term-lock ;

  \ Get the terminal pixel dimensions
  : term-pixels-dim@ ( -- width height )
    picocalc-term-common::term-pixels-dim@
  ;

  \ Get the terminal dimensions in characters
  : term-dim@ ( -- width height ) picocalc-term-common::term-dim@ ;

  \ Get the terminal character dimensions
  : term-char-dim@ ( -- width height ) picocalc-term-common::term-char-dim@ ;

end-module
