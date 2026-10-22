\ Copyright (c) 2025-2026 Travis Bemann
\ Copyright (c) 2026 Ken Mitton
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

  use-st7789v? not use-ili9341? not and use-st7796s? not and [if]
    st7365p-8-common import
    st7365p-8-spi import
  [then]
  use-st7789v? [if]
    st7789v-8-common import
    st7789v-8-spi import
  [then]
  use-ili9341? [if]
    ili9341-8-common import
    ili9341-8-spi import
  [then]
  use-st7796s? [if]
    st7796s-8-common import
    st7796s-8-spi import
  [then]

  use-4x8-font? [if]
    simple-font-4x8 import
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
  use-6x12-font? [if]
    simple-font-6x12 import
  [then]
  use-7x8-font? [if]
    simple-font import
  [then]

  \ PicoCalc terminal class
  <picocalc-term-common> begin-class <picocalc-term>

    begin-module picocalc-term-internal
      
      \ The display
      use-st7789v? not use-ili9341? not and use-st7796s? not and [if] <st7365p-8-spi> [then]
      use-st7789v? [if] <st7789v-8-spi> [then]
      use-ili9341? [if] <ili9341-8-spi> [then]
      use-st7796s? [if] <st7796s-8-spi> [then]
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

      [ use-st7789v? not use-ili9341? not and use-st7796s? not and ] [if]
        display-spi-tx-pin display-spi-sck-pin display-dc-pin display-spi-cs-pin
        display-rst-pin
        display-invert self display-buf display-width display-height
        display-spi-device <st7365p-8-spi> self display-intf init-object
      [then]
      [ use-st7789v? ] [if]
        display-spi-tx-pin display-spi-sck-pin display-dc-pin display-spi-cs-pin
        display-bl-pin display-rst-pin
        self display-buf false display-width display-height
        display-spi-device <st7789v-8-spi> self display-intf init-object
      [then]
      [ use-ili9341? ] [if]
        display-spi-tx-pin display-spi-sck-pin display-dc-pin display-spi-cs-pin
        display-bl-pin display-rst-pin
        self display-buf display-width display-height
        display-spi-device <ili9341-8-spi> self display-intf init-object
      [then]
      [ use-st7796s? ] [if]
        display-spi-tx-pin display-spi-sck-pin display-dc-pin display-spi-cs-pin
        display-bl-pin display-rst-pin
        self display-buf display-width display-height
        display-spi-device <st7796s-8-spi> self display-intf init-object
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
      [ use-4x8-font? ] [if] a-simple-font-4x8 [then]
      [ use-5x8-font? ] [if] a-simple-font-5x8 [then]
      [ use-5x8-v2-font? ] [if] a-simple-font-5x8-v2 [then]
      [ use-6x8-font? ] [if] a-simple-font-6x8 [then]
      [ use-6x12-font? ] [if] a-simple-font-6x12 [then]
      [ use-7x8-font? ] [if] a-simple-font [then]
      draw-char-to-pixmap8
      
      attr attr-underline and if
        char-fg-color display-x display-y char-height 1- + char-width 1
        self display-intf draw-rect-const
      then
    ; define draw-char-with-cursor

    \ Scroll up by a number of lines
    :noname { lines self -- }
      self max-margins? if
        lines term-height min to lines
        lines term-width * { chars }
        
        self chars-buf chars + self chars-buf
        [ term-width term-height * ] literal chars - move
        self chars-buf [ term-width term-height * ] literal chars - + chars
        $20 fill
        
        self attrs-buf chars + self attrs-buf
        [ term-width term-height * ] literal chars - move
        self attrs-buf [ term-width term-height * ] literal chars - + chars
        0 fill
        
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
      else
        self bottom-margin @ 1+ self top-margin @ - { total-lines }
        lines total-lines min to lines
        lines term-width * { chars }
        self top-margin @ term-width * { top-offset }

        self max-left-right-margins? if
          
          self chars-buf top-offset + chars + self chars-buf top-offset +
          term-width total-lines * chars - move
          self chars-buf top-offset + term-width total-lines * chars - + chars
          $20 fill
          
          self attrs-buf top-offset + chars + self attrs-buf top-offset +
          term-width total-lines * chars - move
          self attrs-buf top-offset + term-width total-lines * chars - + chars
          0 fill
          
          self fg-colors-buf top-offset + chars +
          self fg-colors-buf top-offset +
          term-width total-lines * chars - move
          self fg-colors-buf top-offset + term-width total-lines * chars - +
          chars self fg-color @ fill
          
          self bk-colors-buf top-offset + chars +
          self bk-colors-buf top-offset +
          term-width total-lines * chars - move
          self bk-colors-buf top-offset + term-width total-lines * chars - +
          chars self bk-color @ fill

          self bottom-margin @ 1+ char-height * { total-height }
          lines [ char-height display-width * ] literal * { pixels }
          self top-margin @ [ char-height display-width * ] literal *
          { top-pixel-offset }
          self display-buf top-pixel-offset + pixels +
          self display-buf top-pixel-offset +
          display-width total-height * top-pixel-offset - pixels - move

          total-height lines char-height * - { fill-y }
          self bk-color @ get-color
          0 fill-y display-width total-height fill-y - self display-intf
          draw-rect-const
          self display-intf set-dirty
        else
          self right-margin @ 1+ self left-margin @ - { line-chars }
          self left-margin @ self top-margin @ lines + term-width * + { src }
          self left-margin @ self top-margin @ term-width * + { dest }

          self bottom-margin @ 1+ self top-margin @ lines + ?do
            self chars-buf src + self chars-buf dest + line-chars move
            self attrs-buf src + self attrs-buf dest + line-chars move
            self fg-colors-buf src + self fg-colors-buf dest + line-chars move
            self bk-colors-buf src + self bk-colors-buf dest + line-chars move
            term-width +to src
            term-width +to dest
          loop

          self left-margin @ self bottom-margin @ 1+ lines - term-width * +
          to dest
          self fg-color @ { fg }
          self bk-color @ { bk }

          self bottom-margin @ 1+ dup lines - ?do
            self chars-buf dest + line-chars $20 fill
            self attrs-buf dest + line-chars 0 fill
            self fg-colors-buf dest + line-chars fg fill
            self bk-colors-buf dest + line-chars bk fill
            term-width +to dest
          loop

          self display-buf self left-margin @ char-width *
          self top-margin @ lines +
          [ char-height display-width * ] literal * + + to src
          self display-buf self left-margin @ char-width *
          self top-margin @
          [ char-height display-width * ] literal * + + to dest
          line-chars char-width * { line-pixels }

          self bottom-margin @ 1+ char-height *
          self top-margin @ lines + char-height * ?do
            src dest line-pixels move
            display-width +to src
            display-width +to dest
          loop

          bk get-color
          self left-margin @ char-width *
          self bottom-margin @ 1+ lines - char-height *
          line-chars char-width * lines char-height *
          self display-intf draw-rect-const
          self display-intf set-dirty
        then
      then
    ; define scroll-up

    \ Scroll down by a number of lines
    :noname { lines self -- }
      self max-margins? if
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
      else
        self bottom-margin @ 1+ self top-margin @ - { total-lines }
        lines total-lines min to lines
        lines term-width * { chars }
        self top-margin @ term-width * { top-offset }

        self max-left-right-margins? if

          self chars-buf top-offset + dup chars +
          term-width total-lines * chars - move
          self chars-buf top-offset + term-width total-lines * chars - $20 fill

          self attrs-buf self attrs-buf chars +
          term-width total-lines * chars - move
          self attrs-buf top-offset + term-width total-lines * chars - 0 fill

          self fg-colors-buf top-offset + dup chars +
          term-width total-lines * chars - move
          self fg-colors-buf top-offset + term-width total-lines * chars -
          self fg-color @ fill

          self bk-colors-buf top-offset + dup chars +
          term-width total-lines * chars - move
          self bk-colors-buf top-offset + term-width total-lines * chars -
          self bk-color @ fill

          self bottom-margin @ 1+ char-height * { total-height }
          lines [ char-height display-width * ] literal * { pixels }
          self top-margin @ [ char-height display-width * ] literal *
          { top-pixel-offset }
          self display-buf top-pixel-offset + dup pixels +
          total-height display-width * top-pixel-offset - pixels - move
          total-lines lines - char-height * { fill-y }
          self bk-color @ get-color
          0 self top-margin @ char-height * display-width fill-y
          self display-intf draw-rect-const
          self display-intf set-dirty
        else
          self right-margin @ 1+ self left-margin @ - { line-chars }
          self left-margin @ self bottom-margin @ lines - term-width * + { src }
          self left-margin @ self bottom-margin @ term-width * + { dest }
          
          self top-margin @ lines + self bottom-margin @ ?do
            self chars-buf src + self chars-buf dest + line-chars move
            self attrs-buf src + self attrs-buf dest + line-chars move
            self fg-colors-buf src + self fg-colors-buf dest + line-chars move
            self bk-colors-buf src + self bk-colors-buf dest + line-chars move
            [ term-width negate ] literal +to src
            [ term-width negate ] literal +to dest
          -1 +loop

          self left-margin @ self top-margin @ term-width * + to dest
          self fg-color @ { fg }
          self bk-color @ { bk }
          
          self top-margin @ lines + self top-margin @ ?do
            self chars-buf dest + line-chars $20 fill
            self attrs-buf dest + line-chars 0 fill
            self fg-colors-buf dest + line-chars fg fill
            self bk-colors-buf dest + line-chars bk fill
            term-width +to dest
          loop

          self display-buf self left-margin @ char-width *
          self bottom-margin @ 1+ lines -
          [ char-height display-width * ] literal * + display-width - + to src
          self display-buf self left-margin @ char-width *
          self bottom-margin @ 1+
          [ char-height display-width * ] literal * + display-width - + to dest
          line-chars char-width * { line-pixels }

          self top-margin @ lines + char-height *
          self bottom-margin @ 1+ char-height * 1- do
            src dest line-pixels move
            [ display-width negate ] literal +to src
            [ display-width negate ] literal +to dest
          -1 +loop

          bk get-color
          self left-margin @ char-width * self top-margin @ char-height *
          line-chars char-width * lines char-height *
          self display-intf draw-rect-const
          self display-intf set-dirty
        then
      then
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
      boot-delay-ms ms
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
    picocalc-term-common::term-dim@ drop term-cols !
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

  \ Refresh the terminal
  : refresh-term ( -- ) shared-term do-refresh-term ;
  
  \ Reset the terminal default foreground color
  : reset-term-default-fg-color ( -- )
    [:
      picocalc-term-common::reset-term-default-fg-color
      shared-term update-default-fg-color
    ;] with-term-lock
  ;

  \ Reset the terminal default background color
  : reset-term-default-bk-color ( -- )
    [:
      picocalc-term-common::reset-term-default-bk-color
      shared-term update-default-bk-color
    ;] with-term-lock
  ;

  \ Reset the terminal default foreground and background colors
  : reset-term-default-colors ( -- )
    [:
      picocalc-term-common::reset-term-default-colors
      shared-term update-default-fg-color
      shared-term update-default-bk-color
    ;] with-term-lock
  ;

  \ Reset the terminal colors' RGB8 mappings
  : reset-term-colors-rgb8 ( -- ) picocalc-term-common::reset-term-colors-rgb8 ;

  \ Reset the terminal colors
  : reset-term-colors ( -- )
    [:
      picocalc-term-common::reset-term-colors
      shared-term update-default-fg-color
      shared-term update-default-bk-color
    ;] with-term-lock
  ;

  \ Get the terminal default foreground color
  : term-default-fg-color@ ( -- color-const )
    picocalc-term-common::term-default-fg-color@
  ;

  \ Set the terminal default foreground color
  : term-default-fg-color! ( color-const -- )
    [:
      picocalc-term-common::term-default-fg-color!
      shared-term update-default-fg-color
    ;] with-term-lock
  ;

  \ Get the terminal default background color
  : term-default-bk-color@ ( -- color-const )
    picocalc-term-common::term-default-bk-color@
  ;

  \ Set the terminal default background color
  : term-default-bk-color! ( color-const -- )
    [:
      picocalc-term-common::term-default-bk-color!
      shared-term update-default-bk-color
    ;] with-term-lock
  ;
  
  \ Get a terminal color as RGB8
  : term-color-rgb8@ ( color-const -- color )
    picocalc-term-common::term-color-rgb8@
  ;

  \ Set a terminal color as RGB8
  : term-color-rgb8! ( color color-const -- )
    picocalc-term-common::term-color-rgb8!
  ;
  
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

  \ Set raw keys enabled
  : raw-keys-enabled! ( enabled -- ) shared-term do-raw-keys-enabled! ;

  \ Get raw keys enabled
  : raw-keys-enabled@ ( -- enabled ) shared-term do-raw-keys-enabled@ ;

  \ Get whether a raw key is available
  : raw-key>? ( -- available ) shared-term do-raw-key>? ;

  \ Get a raw key
  : raw-key> ( -- attributes key ) shared-term do-raw-key> ;
  
end-module
