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
  pixmap8 import
  pixmap8-internal import
  font import
  console import

  use-st7789v? not [if]
    st7365p-8-common import
    st7365p-8-spi import
  [else]
    st7789v-8-common import
    st7789v-8-spi import
  [then]

  use-5x8-font? [if]
    simple-font-5x8 import
  [then]
  use-5x8-v2-font? [if]
    simple-font-5x8-v2 import
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
      use-st7789v? not [if] <st7365p-8-spi> [else] <st7789v-8-spi> [then]
      class-size member display-intf
      
      \ The display buffer
      display-width display-height pixmap8-buf-size member display-buf
      
    end-module> import
    
    \ Carry out an operation against the PicoCalc terminal's display with the
    \ PicoCalc terminal locked. It is highly recommended that the user update
    \ the terminal's display or graphics drawn to it may not be displayed. Note
    \ that executing operations that print to the PicoCalc terminal should be
    \ avoided because they may block indefinitely.
    method do-with-term-display ( xt self -- ) \ xt: ( display -- )
    
  end-class

  \ Implement PicoCalc terminal class
  <picocalc-term> begin-implement

    \ Constructor
    :noname { self -- }
      self <picocalc-term-common>->new

      [ use-st7789v? not ] [if]
        display-spi-tx-pin display-spi-sck-pin display-dc-pin display-spi-cs-pin
        display-rst-pin
        display-invert self display-buf display-width display-height
        display-spi-device <st7365p-8-spi> self display-intf init-object
      [else]
        display-spi-tx-pin display-spi-sck-pin display-dc-pin display-spi-cs-pin
        display-bl-pin display-rst-pin
        self display-buf false display-width display-height
        display-spi-device <st7789v-8-spi> self display-intf init-object
      [then]
    ; define new

    \ Initialize the PicoCalc terminal
    :noname { self -- }
      self <picocalc-term-common>->init-term
      self display-intf clear-pixmap
      self draw-cursor
      self display-intf update-display
      self start-term-task
    ; define init-term

    \ Update the display
    :noname ( self -- ) display-intf update-display ; define do-update-display

    \ Carry out an operation against the PicoCalc terminal's display with the
    \ PicoCalc terminal locked. It is highly recommended that the user update
    \ the terminal's display or graphics drawn to it may not be displayed. Note
    \ that executing operations that print to the PicoCalc terminal should be
    \ avoided because they may block indefinitely.
    :noname ( xt self -- ) \ xt: ( display -- )
      [: display-intf swap execute ;] over do-with-term-lock
    ; define do-with-term-display

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
      { char-bk-color char-fg-color }
      char-width x * char-height y * { display-x display-y }

      char-bk-color display-x display-y char-width char-height
      self display-intf draw-rect-const
      
      char-fg-color c display-x display-y self display-intf
      [ use-5x8-font? ] [if] a-simple-font-5x8 [then]
      [ use-5x8-v2-font? ] [if] a-simple-font-5x8-v2 [then]
      [ use-6x8-font? ] [if] a-simple-font-6x8 [then]
      [ use-7x8-font? ] [if] a-simple-font [then]
      draw-char-to-pixmap8
      
      attr attr-underline and if
        char-fg-color display-x display-y char-height 1- + char-width 1
        self display-intf draw-rect-const
      then
    ; define draw-char-with-cursor

    \ Scroll up by a number of lines
    :noname { lines self -- }
      lines term-height min to lines
      lines term-width * { chars }

      self chars-buf chars + self chars-buf
      [ term-width term-height * ] literal chars - move
      self chars-buf [ term-width term-height * ] literal chars - + chars
      $20 fill

      self attrs-buf chars + self attrs-buf
      [ term-width term-height * ] literal chars - move
      self attrs-buf [ term-width term-height * ] literal chars - + chars 0 fill

      self fg-colors-buf chars + self fg-colors-buf
      [ term-width term-height * ] literal chars - move
      self fg-colors-buf [ term-width term-height * ] literal chars - + chars
      self fg-color @ fill

      self bk-colors-buf chars + self bk-colors-buf
      [ term-width term-height * ] literal chars - move
      self bk-colors-buf [ term-width term-height * ] literal chars - + chars
      self bk-color @ fill

      lines [ char-height display-width * ] literal * { pixels }
      self display-buf pixels + self display-buf
      [ display-width display-height * ] literal pixels - move
      [ term-height char-height * ] literal lines char-height * - { fill-y }
      self bk-color @ get-color
      0 fill-y display-width display-height fill-y - self display-intf
      draw-rect-const
      self display-intf set-dirty
    ; define scroll-up

    \ Scroll down by a number of lines
    :noname { lines self -- }
      lines term-height min to lines
      lines term-width * { chars }

      self chars-buf self chars-buf chars +
      [ term-width term-height * ] literal chars - move
      self chars-buf [ term-width term-height * ] literal chars - $20 fill

      self attrs-buf self attrs-buf chars +
      [ term-width term-height * ] literal chars - move
      self attrs-buf [ term-width term-height * ] literal chars - 0 fill

      self fg-colors-buf self fg-colors-buf chars +
      [ term-width term-height * ] literal chars - move
      self fg-colors-buf [ term-width term-height * ] literal chars -
      self fg-color @ fill

      self bk-colors-buf self bk-colors-buf chars +
      [ term-width term-height * ] literal chars - move
      self bk-colors-buf [ term-width term-height * ] literal chars -
      self bk-color @ fill

      lines [ char-height display-width * ] literal * { pixels }
      self display-buf self display-buf pixels +
      [ display-width display-height * ] literal pixels - move
      display-height term-height lines - char-height * - { fill-y }
      self bk-color @ get-color
      0 0 display-width fill-y self display-intf
      draw-rect-const
      self display-intf set-dirty
    ; define scroll-down

    \ Invert the display
    :noname { self -- }
      [ display-width display-height * ] literal self display-buf + { end-addr }
      self display-buf
      begin dup end-addr < while dup c@ $FF xor over c! 1+ repeat
      drop
      self display-intf set-dirty
    ; define invert-display
    
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
  
  \ Carry out an operation against the PicoCalc terminal's display with the
  \ PicoCalc terminal locked. It is highly recommended that the user update
  \ the terminal's display or graphics drawn to it may not be displayed. Note
  \ that executing operations that print to the PicoCalc terminal should be
  \ avoided because they may block indefinitely.
  : with-term-display ( xt -- ) \ xt: ( display -- )
    shared-term do-with-term-display
  ;
  
  \ Get the terminal pixel dimensions
  : term-pixels-dim@ ( -- width height )
    picocalc-term-common::term-pixels-dim@
  ;

  \ Get the terminal dimensions in characters
  : term-dim@ ( -- width height ) picocalc-term-common::term-dim@ ;

  \ Get the terminal character dimensions
  : term-char-dim@ ( -- width height ) picocalc-term-common::term-char-dim@ ;

  \ Get the terminal font
  : term-font@ ( -- font ) picocalc-term-common::term-font@ ;
  
  \ Read the battery
  : read-battery ( -- val ) shared-term do-read-battery ;

  \ Set the backlight
  : set-backlight ( val -- val' ) shared-term do-set-backlight ;
  
  \ Read the backlight
  : read-backlight ( -- val ) shared-term do-read-backlight ;
  
  \ Set the keyboard backlight
  : set-kbd-backlight ( val -- val' ) shared-term do-set-kbd-backlight ;
  
  \ Read the keyboard backlight
  : read-kbd-backlight ( -- val ) shared-term do-read-kbd-backlight ;
  
  \ Set visual bell enabled
  : visual-bell-enabled! ( enabled -- ) shared-term do-visual-bell-enabled! ;

  \ Get visual bell enabled
  : visual-bell-enabled@ ( -- enabled ) shared-term do-visual-bell-enabled@ ;

  \ Set audible bell enabled
  : audible-bell-enabled! ( enabled -- ) shared-term do-audible-bell-enabled! ;

  \ Get audible bell enabled
  : audible-bell-enabled@ ( -- enabled ) shared-term do-audible-bell-enabled@ ;

  \ Set the screenshot hook
  : screenshot-hook! ( xt -- ) shared-term do-screenshot-hook! ;

  \ Get the screenshot hook
  : screenshot-hook@ ( -- xt ) shared-term do-screenshot-hook@ ;
  
end-module
