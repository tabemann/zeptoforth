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

begin-module picocalc-term-common

  defined? st7789v-8-common defined? st7789v-text-common or
  constant use-st7789v?

  \ Select fonts automatically based on what fonts are loaded
  defined? simple-font-5x8 constant use-5x8-font?
  defined? simple-font-6x8 use-5x8-font? not and constant use-6x8-font?
  defined? simple-font use-5x8-font? not and use-6x8-font? not and
  constant use-7x8-font?

  \ Ensure that at least one font is available
  : font-test ( -- )
    use-5x8-font? use-6x8-font? or use-7x8-font? or not if
      [: ." no font is available" cr ;] ?raise
    then
  ;
  font-test

  oo import
  picocalc-keys import
  task import
  lock import
  stream import
  console import

  use-5x8-font? [if]
    simple-font-5x8 import
  [then]
  use-6x8-font? [if]
    simple-font-6x8 import
  [then]
  use-7x8-font? [if]
    simple-font import
  [then]

  defined? pixmap8 [if]
    pixmap8 import
  [else]
    text8 import
  [then]
  
  begin-module picocalc-term-common-internal

    \ Initialize the font
    use-5x8-font? [if]
      initializer init-simple-font-5x8
    [then]
    use-6x8-font? [if]
      initializer init-simple-font-6x8
    [then]
    use-7x8-font? [if]
      initializer init-simple-font
    [then]

    \ Font character with
    use-5x8-font? [if]
      5 constant char-width
    [then]
    use-6x8-font? [if]
      6 constant char-width
    [then]
    use-7x8-font? [if]
      7 constant char-width
    [then]

    \ Font character height
    8 constant char-height

    \ Display width
    320 constant display-width
    
    \ Display height
    use-st7789v? not [if]
      320 constant display-height
    [else]
      240 constant display-height
    [then]
    
    \ Terminal width
    display-width char-width / constant term-width

    \ Terminal height
    display-height char-height / constant term-height

    \ Display SPI device
    \ use-st7789v? not [if]
      1 constant display-spi-device
    \ [else]
    \   0 constant display-spi-device
    \ [then]

    \ Display SCK pin
\    use-st7789v? not [if]
      10 constant display-spi-sck-pin
    \ [else]
    \   18 constant display-spi-sck-pin
    \ [then]

    \ Display TX pin
    \ use-st7789v? not [if]
      11 constant display-spi-tx-pin
    \ [else]
    \   19 constant display-spi-tx-pin
    \ [then]

    \ Display CS pin
    \ use-st7789v? not [if]
      13 constant display-spi-cs-pin
    \ [else]
    \   21 constant display-spi-cs-pin
    \ [then]

    \ Display DC pin
    \ use-st7789v? not [if]
      14 constant display-dc-pin
    \ [else]
    \   13 constant display-dc-pin
    \ [then]

    \ Display RST pin
    \ use-st7789v? not [if]
      15 constant display-rst-pin
    \ [else]
    \   12 constant display-rst-pin
    \ [then]

    \ Display backlight pin
    use-st7789v? [if]
      12 constant display-bl-pin
    [then]
    
    \ Do we invert the display
    use-st7789v? not [if]
      true constant display-invert
    [then]
    
    \ The input stream size
    256 constant input-stream-size

    \ The output stream-size
    256 constant output-stream-size

    \ The input receive buffer size
    64 constant output-recv-buf-size

    \ Input limit
    8 constant input-limit
    
    \ Output limit
    term-width term-height * 2 / 3 * constant output-limit

    \ Attributes
    0 bit constant attr-bold
    1 bit constant attr-dim
    2 bit constant attr-underline
    3 bit constant attr-reverse

    \ The default foreground color
    ansi-term::b-green constant default-fg-color

    \ The default background color
    ansi-term::black constant default-bk-color
    
    \ The terminal colors
    create term-colors
    0 0 0 rgb8 c, \ black
    191 0 0 rgb8 c, \ red
    0 191 0 rgb8 c, \ green
    191 191 0 rgb8 c, \ yellow
    0 0 191 rgb8 c, \ blue
    191 0 191 rgb8 c, \ magenta
    0 191 191 rgb8 c, \ cyan
    191 191 191 rgb8 c, \ white
    63 63 63 rgb8 c, \ b-black
    255 0 0 rgb8 c, \ b-red
    0 255 0 rgb8 c, \ b-green
    255 255 0 rgb8 c, \ b-yellow
    0 0 255 rgb8 c, \ b-blue
    255 0 255 rgb8 c, \ b-magenta
    0 255 255 rgb8 c, \ b-cyan
    255 255 255 rgb8 c, \ b-white

    \ Modify the foreground color based on attributes
    : modify-color { color-const attr -- color-const' }
      attr attr-bold and if
        color-const ansi-term::black >=
        color-const ansi-term::white <= and if
          color-const ansi-term::black - ansi-term::b-black +
        else
          color-const
        then
      else
        attr attr-dim and if
          color-const ansi-term::b-black >=
          color-const ansi-term::b-white <= and if
            color-const ansi-term::b-black - ansi-term::black +
          else
            color-const
          then
        else
          color-const
        then
      then
    ;
    
    \ Get the color index
    : color-index { color-const -- index }
      color-const ansi-term::black >=
      color-const ansi-term::white <= and if
        color-const ansi-term::black -
      else
        color-const ansi-term::b-black >=
        color-const ansi-term::b-white <= and if
          color-const ansi-term::b-black - 8 +
        else
          0
        then
      then
    ;

    \ Get the color
    : get-color ( color-const -- color )
      color-index term-colors + c@
    ;

    \ Convert a control character
    : convert-control { c -- c' }
      c $20 >= c $3F <= and if c $20 - exit then
      c $40 >= c $5F <= and if c $40 - exit then
      c $60 >= c $7F <= and if c $60 - exit then
      c
    ;
    
    \ Character constants
    $1B constant escape
    $09 constant tab
    $7F constant delete
    $0A constant linefeed
    $0D constant return
    $08 constant backspace

    \ Tab size (note as implemented this must be a power of two)
    8 constant tab-size

    \ Escape character timeout in ticks
    2500 constant escape-timeout

    \ Flag for if the welcome message has been displayed on the screen yet
    false value picocalc-welcome-displayed

    \ Picocalc welcome message
    : picocalc-welcome ( -- )
      cr ." Welcome to zeptoforth on the PicoCalc"
      cr ." Built for " kernel-platform type
      cr ." Version " kernel-version type
      cr ." On " kernel-date type
      cr ." zeptoforth comes with ABSOLUTELY NO WARRANTY"
      cr ." For details type `license' "
    ;
    
  end-module> import

  : *depth* ( c -- )
    [: space emit space depth . flush-console ;] console::with-serial-output
  ;
  
  \ The common PicoCalc terminal class
  <object> begin-class <picocalc-term-common>

    continue-module picocalc-term-common-internal

      \ The keyboard
      <picocalc-keys> class-size member key-intf

      \ The character matrix
      term-width term-height * cell align member chars-buf

      \ The character attribute matrix
      term-width term-height * cell align member attrs-buf

      \ The character foreground colors matrix
      term-width term-height * cell align member fg-colors-buf

      \ The character background colors matrix
      term-width term-height * cell align member bk-colors-buf

      \ The foreground color
      cell member fg-color
      
      \ The background color
      cell member bk-color

      \ The attributes
      cell member attrs
      
      \ The cursor x coordinate
      cell member cursor-x

      \ The cursor y coordinate
      cell member cursor-y

      \ Is the cursor visible
      cell member cursor-visible

      \ The saved cursor x coordinate
      cell member saved-cursor-x

      \ The saved cursor y coordinate
      cell member saved-cursor-y
      
      \ The terminal task
      cell member term-task

      \ The terminal task mailbox
      cell member term-task-mailbox

      \ The terminal lock
      lock-size member term-lock

      \ The input stream
      input-stream-size stream-size member input-stream

      \ The output stream
      output-stream-size stream-size member output-stream

      \ The input receive buffer
      output-recv-buf-size cell align member output-recv-buf

      \ Run the terminal
      method run-term ( self -- )

      \ Start the terminal emulator task
      method start-term-task ( self -- )

      \ Update the display
      method do-update-display ( self -- )
      
      \ Draw the cursor
      method draw-cursor ( self -- )

      \ Clear the cursor
      method clear-cursor ( self -- )

      \ Draw a character with or without a cursor
      method draw-char-with-cursor ( cursor? x y self -- )
    
      \ Draw a character
      method draw-char ( x y self -- )

      \ Scroll up by a number of lines
      method scroll-up ( lines self -- )

      \ Scroll down by a number of lines
      method scroll-down ( lines self -- )

      \ Handle a normal key
      method handle-normal-key ( c self -- )

      \ Handle a control key
      method handle-ctrl-key ( c self -- )

      \ Handle an alt key
      method handle-alt-key ( c self -- )

      \ Handle a control-alt key
      method handle-ctrl-alt-key ( c self -- )
      
      \ Handle input
      method handle-input ( self -- )

      \ Handle output
      method handle-output ( self -- )

      \ Handle an escape
      method handle-escape ( self -- )
      
      \ Parse an escape
      method parse-escape ( addr bytes self -- )

      \ Parse a decimal number in a string
      method parse-dec ( default addr bytes self -- u size found? )
      
      \ Handle showing the cursor
      method handle-show-cursor ( self -- )

      \ Handle hiding the cursor
      method handle-hide-cursor ( self -- )

      \ Handle saving the cursor
      method handle-save-cursor ( self -- )

      \ Handle restoring the cursor
      method handle-restore-cursor ( self -- )

      \ Handle querying the cursor position
      method handle-query-cursor-position ( self -- )

      \ Handle moving the cursor up
      method handle-cursor-up ( addr bytes self -- )

      \ Handle moving the cursor down
      method handle-cursor-down ( addr bytes self -- )

      \ Handle moving the cursor forward
      method handle-cursor-forward ( addr bytes self -- )

      \ Handle moving the cursor back
      method handle-cursor-back ( addr bytes self -- )

      \ Handle moving the cursor to the start of a following line
      method handle-cursor-next-line ( addr bytes self -- )

      \ Handle moving the cursor to the start of a preceding line
      method handle-cursor-prev-line ( addr bytes self -- )

      \ Handle moving the cursor to a column
      method handle-horiz-abs ( addr bytes self -- )

      \ Handle setting the cursor position
      method handle-cursor-pos ( addr bytes self -- )

      \ Handle erasing in the display
      method handle-erase-in-display ( addr bytes self -- )

      \ Erase a character
      method erase-char ( x y self -- )

      \ Handle erasing down
      method handle-erase-down ( self -- )

      \ Handle erasing up
      method handle-erase-up ( self -- )

      \ Handle erasing the screen
      method handle-erase-screen ( self -- )
    
      \ Handle erasing in the line
      method handle-erase-in-line ( addr bytes self -- )

      \ Handle erasing to the right
      method handle-erase-right ( self -- )

      \ Handle erasing to the left
      method handle-erase-left ( self -- )

      \ Handle erasing the current line
      method handle-erase-line ( self -- )

      \ Handle scrolling up
      method handle-scroll-up ( addr bytes self -- )

      \ Handle scrolling down
      method handle-scroll-down ( addr bytes self -- )

      \ Parse the text color and style
      method parse-color-style ( addr bytes self -- )
      
      \ Handle setting the text color and style
      method handle-color-style ( style self -- )

      \ Handle setting the foreground color
      method handle-fg-color ( color self -- )

      \ Handle setting the background color
      method handle-bk-color ( color self -- )

      \ Handle resetting the color and stle
      method handle-reset-color-style ( self -- )

      \ Handle setting bold style
      method handle-bold ( self -- )

      \ Handle setting dim style
      method handle-dim ( self -- )

      \ Handle setting underlined style
      method handle-underline ( self -- )

      \ Handle setting reversed style
      method handle-reverse ( self -- )

      \ Handle setting normal style
      method handle-normal ( self -- )

      \ Handle setting non-underlined style
      method handle-not-underline ( self -- )

      \ Handle setting non-reversed style
      method handle-not-reverse ( self -- )

      \ Handle setting the default foreground color
      method handle-default-fg-color ( self -- )

      \ Handle setting the default background color
      method handle-default-bk-color ( self -- )
      
      \ Handle a carriage return
      method handle-return ( self -- )

      \ Handle a linefeed
      method handle-linefeed ( self -- )

      \ Handle a backspace
      method handle-backspace ( self -- )

      \ Handle a tab
      method handle-tab ( self -- )

      \ Handle an ordinary character
      method handle-char ( c self -- )

      \ Input a string
      method input-string ( c-addr u self -- )

      \ Handle break
      method handle-break ( self -- )

      \ Handle control-break
      method handle-control-break ( self -- )
      
    end-module

    \ Initialize the PicoCalc terminal
    method init-term ( self -- )
    
    \ Carry out an operation with the PicoCalc terminal locked. Note that
    \ executing operations that print to the PicoCalc terminal should be avoided
    \ because they may block indefinitely.
    method do-with-term-lock ( xt self -- )

    \ Inject a keycode
    method inject-keycode ( keycode self -- )

    \ Test the output stream for room for a byte
    method >term? ( self -- room? )

    \ Enqueue a byte to the output stream
    method >term ( c self -- )

    \ Test the input stream for availability of a byte
    method term>? ( self -- available? )

    \ Dequeue a byte from the input stream
    method term> ( self -- c )

    \ Flush the output stream
    method flush-term ( self -- )
    
  end-class

  \ Implement the common PicoCalc terminal class
  <picocalc-term-common> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
    ; define new

    \ Initialize the PicoCalc terminal
    :noname { self -- }
      <picocalc-keys> self key-intf init-object
      0 self term-task !
      0 self term-task-mailbox !
      self term-lock init-lock
      input-stream-size self input-stream init-stream
      output-stream-size self output-stream init-stream
      self chars-buf [ term-width term-height * ] literal $20 fill
      self attrs-buf [ term-width term-height * ] literal 0 fill
      self fg-colors-buf [ term-width term-height * ] literal
      default-fg-color fill
      self bk-colors-buf [ term-width term-height * ] literal
      default-bk-color fill
      default-fg-color self fg-color !
      default-bk-color self bk-color !
      0 self attrs !
      0 self cursor-x !
      0 self cursor-y !
      true self cursor-visible !
      0 self saved-cursor-x !
      0 self saved-cursor-y !
    ; define init-term

    \ Start the terminal emulator task
    :noname { self -- }
      self key-intf init-picocalc-keys
      self 1 ['] run-term 1024 256 1024 spawn self term-task !
      c" term" self term-task @ task-name!
      self term-task-mailbox 1 self term-task @ config-notify
      self term-task @ run
    ; define start-term-task

    \ Inject a keycode
    :noname ( keycode self -- )
      key-intf inject-picocalc-keycode
    ; define inject-keycode

    \ Run the PicoCalc terminal
    :noname { self -- }
      begin self handle-input self handle-output pause again
    ; define run-term

    \ Handle a normal key
    :noname { c self -- }
      attention? @ if
        c KEY_ESC = if $1B to key then
        c $80 < if c [: attention-hook @ ?execute ;] try drop then
      else
        c linefeed = if s\" \r" self input-string exit then
        c backspace = if s\" \x7F" self input-string exit then
        c KEY_ESC = if s\" \x1B" self input-string 100 ms exit then
        c KEY_F1 = if s\" \x1B\x4F\x50" self input-string exit then
        c KEY_F2 = if s\" \x1B\x4F\x51" self input-string exit then
        c KEY_F3 = if s\" \x1B\x4F\x52" self input-string exit then
        c KEY_F4 = if s\" \x1B\x4F\x53" self input-string exit then
        c KEY_F5 = if s\" \x1B\x5B\x31\x35\x7E" self input-string exit then
        c KEY_F6 = if s\" \x1B\x5B\x31\x37\x7E" self input-string exit then
        c KEY_F7 = if s\" \x1B\x5B\x31\x38\x7E" self input-string exit then
        c KEY_F8 = if s\" \x1B\x5B\x31\x39\x7E" self input-string exit then
        c KEY_F9 = if s\" \x1B\x5B\x32\x30\x7E" self input-string exit then
        c KEY_F10 = if s\" \x1B\x5B\x32\x31\x7E" self input-string exit then
        c KEY_UP = if s\" \x1B\x5B\x41" self input-string exit then
        c KEY_DOWN = if s\" \x1B\x5B\x42" self input-string exit then
        c KEY_RIGHT = if s\" \x1B\x5B\x43" self input-string exit then
        c KEY_LEFT = if s\" \x1B\x5B\x44" self input-string exit then
        c KEY_BREAK = if self handle-break exit then
        c KEY_INSERT = if s\" \x1B\x5B\x32\x7E" self input-string exit then
        c KEY_HOME = if s\" \x1B\x5B\x48" self input-string exit then
        c KEY_DEL = if s\" \x1B\x5B\x33\x7E" self input-string exit then
        c KEY_END = if s\" \x1B\x5B\x46" self input-string exit then
        c KEY_PUP = if s\" \x1B\x5B\x35\x7E" self input-string exit then
        c KEY_PDOWN = if s\" \x1B\x5B\x36\x7E" self input-string exit then
        c $7F < if c { W^ c } c 1 self input-string then
      then
    ; define handle-normal-key

    \ Handle a control key
    :noname { c self -- }
      attention? @ if
        c KEY_ESC = if $1B to key then
        c $80 < if
          c convert-control [: attention-hook @ ?execute ;] try drop
        then
      else
        c linefeed = if s\" \r" self input-string exit then
        c backspace = if s\" \x08" self input-string exit then
        c KEY_ESC = if s\" \x00" self input-string 100 ms exit then
        c KEY_F1 = if s\" \x1B\x5B\x31\x3B\x35\x50" self input-string exit then
        c KEY_F2 = if s\" \x1B\x5B\x31\x3B\x35\x51" self input-string exit then
        c KEY_F3 = if s\" \x1B\x5B\x31\x3B\x35\x52" self input-string exit then
        c KEY_F4 = if s\" \x1B\x5B\x31\x3B\x35\x53" self input-string exit then
        c KEY_F5 = if
          s\" \x1B\x5B\x31\x35\x3B\x35\x7E" self input-string exit
        then
        c KEY_F6 = if
          s\" \x1B\x5B\x31\x37\x3B\x35\x7E" self input-string exit
        then
        c KEY_F7 = if
          s\" \x1B\x5B\x31\x38\x3B\x35\x7E" self input-string exit
        then
        c KEY_F8 = if
          s\" \x1B\x5B\x31\x39\x3B\x35\x7E" self input-string exit
        then
        c KEY_F9 = if
          s\" \x1B\x5B\x32\x30\x3B\x35\x7E" self input-string exit
        then
        c KEY_F10 = if
          s\" \x1B\x5B\x32\x31\x3B\x35\x7E" self input-string exit
        then
        c KEY_UP = if
          s\" \x1B\x5B\x31\x3B\x35\x41" self input-string exit
        then
        c KEY_DOWN = if
          s\" \x1B\x5B\x31\x3B\x35\x42" self input-string exit
        then
        c KEY_RIGHT = if
          s\" \x1B\x5B\x31\x3B\x35\x43" self input-string exit
        then
        c KEY_LEFT = if
          s\" \x1B\x5B\x31\x3B\x35\x44" self input-string exit
        then
        c KEY_BREAK = if self handle-control-break exit then
        c KEY_INSERT = if
          s\" \x1B\x5B\x32\x3B\x35\x7E" self input-string exit
        then
        c KEY_HOME = if
          s\" \x1B\x5B\x31\x3B\x35\x48" self input-string exit
        then
        c KEY_DEL = if s\" \x1B\x5B\x33\x3B\x35\x7E" self input-string exit then
        c KEY_END = if s\" \x1B\x5B\x31\x3B\x35\x46" self input-string exit then
        c KEY_PUP = if s\" \x1B\x5B\x35\x3B\x35\x7E" self input-string exit then
        c KEY_PDOWN = if
          s\" \x1B\x5B\x36\x3B\x35\x7E" self input-string exit
        then
        c $7F < if
          c convert-control { W^ c } c 1 self input-string
        then
      then
    ; define handle-ctrl-key

    \ Handle an alt key
    :noname { c self -- }
      attention? @ not if
        c linefeed = if s\" \r" self input-string exit then
        c backspace = if s\" \x1B\x7F" self input-string exit then
        c KEY_ESC = if s\" \x1B\x1B" self input-string 100 ms exit then
        c KEY_F1 = if s\" \x1B\x5B\x31\x3B\x35\x50" self input-string exit then
        c KEY_F2 = if s\" \x1B\x5B\x31\x3B\x35\x51" self input-string exit then
        c KEY_F3 = if s\" \x1B\x5B\x31\x3B\x35\x52" self input-string exit then
        c KEY_F4 = if s\" \x1B\x5B\x31\x3B\x35\x53" self input-string exit then
        c KEY_F5 = if
          s\" \x1B\x5B\x31\x35\x3B\x33\x7E" self input-string exit
        then
        c KEY_F6 = if
          s\" \x1B\x5B\x31\x37\x3B\x33\x7E" self input-string exit
        then
        c KEY_F7 = if
          s\" \x1B\x5B\x31\x38\x3B\x33\x7E" self input-string exit
        then
        c KEY_F8 = if
          s\" \x1B\x5B\x31\x39\x3B\x33\x7E" self input-string exit
        then
        c KEY_F9 = if
          s\" \x1B\x5B\x32\x30\x3B\x33\x7E" self input-string exit
        then
        c KEY_F10 = if
          s\" \x1B\x5B\x32\x31\x3B\x33\x7E" self input-string exit
        then
        c KEY_UP = if
          s\" \x1B\x5B\x31\x3B\x33\x41" self input-string exit
        then
        c KEY_DOWN = if
          s\" \x1B\x5B\x31\x3B\x33\x42" self input-string exit
        then
        c KEY_RIGHT = if
          s\" \x1B\x5B\x31\x3B\x33\x43" self input-string exit
        then
        c KEY_LEFT = if
          s\" \x1B\x5B\x31\x3B\x33\x44" self input-string exit
        then
        c KEY_BREAK = if self handle-break exit then
        c KEY_INSERT = if
          s\" \x1B\x5B\x32\x3B\x33\x7E" self input-string exit
        then
        c KEY_HOME = if
          s\" \x1B\x5B\x31\x3B\x33\x48" self input-string exit
        then
        c KEY_DEL = if s\" \x1B\x5B\x33\x3B\x33\x7E" self input-string exit then
        c KEY_END = if s\" \x1B\x5B\x31\x3B\x33\x46" self input-string exit then
        c KEY_PUP = if s\" \x1B\x5B\x35\x3B\x33\x7E" self input-string exit then
        c KEY_PDOWN = if
          s\" \x1B\x5B\x36\x3B\x33\x7E" self input-string exit
        then
        c $7F < if c 8 lshift $1B or { W^ c } c 2 self input-string then
      then
    ; define handle-alt-key

    \ Handle a control-alt key
    :noname { c self -- }
      attention? @ not if
        c linefeed = if s\" \x1B\r" self input-string exit then
        c backspace = if s\" \x1B\x08" self input-string exit then
        c KEY_ESC = if s\" \x1B\x00" self input-string 100 ms exit then
        c KEY_F1 = if s\" \x1B\x5B\x31\x3B\x37\x50" self input-string exit then
        c KEY_F2 = if s\" \x1B\x5B\x31\x3B\x37\x51" self input-string exit then
        c KEY_F3 = if s\" \x1B\x5B\x31\x3B\x37\x52" self input-string exit then
        c KEY_F4 = if s\" \x1B\x5B\x31\x3B\x37\x53" self input-string exit then
        c KEY_F5 = if
          s\" \x1B\x5B\x31\x35\x3B\x37\x7E" self input-string exit
        then
        c KEY_F6 = if
          s\" \x1B\x5B\x31\x37\x3B\x37\x7E" self input-string exit
        then
        c KEY_F7 = if
          s\" \x1B\x5B\x31\x38\x3B\x37\x7E" self input-string exit
        then
        c KEY_F8 = if
          s\" \x1B\x5B\x31\x39\x3B\x37\x7E" self input-string exit
        then
        c KEY_F9 = if
          s\" \x1B\x5B\x32\x30\x3B\x37\x7E" self input-string exit
        then
        c KEY_F10 = if
          s\" \x1B\x5B\x32\x31\x3B\x37\x7E" self input-string exit
        then
        c KEY_UP = if
          s\" \x1B\x5B\x31\x3B\x37\x41" self input-string exit
        then
        c KEY_DOWN = if
          s\" \x1B\x5B\x31\x3B\x37\x42" self input-string exit
        then
        c KEY_RIGHT = if
          s\" \x1B\x5B\x31\x3B\x37\x43" self input-string exit
        then
        c KEY_LEFT = if
          s\" \x1B\x5B\x31\x3B\x37\x44" self input-string exit
        then
        c KEY_BREAK = if self handle-control-break exit then
        c KEY_INSERT = if
          s\" \x1B\x5B\x32\x3B\x37\x7E" self input-string exit
        then
        c KEY_HOME = if
          s\" \x1B\x5B\x31\x3B\x37\x48" self input-string exit
        then
        c KEY_DEL = if s\" \x1B\x5B\x33\x3B\x37\x7E" self input-string exit then
        c KEY_END = if s\" \x1B\x5B\x31\x3B\x37\x46" self input-string exit then
        c KEY_PUP = if s\" \x1B\x5B\x35\x3B\x37\x7E" self input-string exit then
        c KEY_PDOWN = if
          s\" \x1B\x5B\x36\x3B\x37\x7E" self input-string exit
        then
        c $7F < if
          c convert-control 8 lshift $1B or { W^ c } c 2 self input-string
        then
      then
    ; define handle-ctrl-alt-key

    \ Handle input
    :noname { self -- }
      input-limit { counter }
      begin self key-intf picocalc-keys>? counter 0> and while
        self key-intf picocalc-keys> { attrs c }

        \ DEBUG
        \ attrs c [:
        \   ." <" h.2 ." :" h.2 space flush-console
        \ ;] console::with-serial-output
        \ END DEBUG

        attrs 0= if
          c self handle-normal-key
        else
          attrs ATTR_CTRL = if
            c self handle-ctrl-key
          else
            attrs ATTR_ALT = if
              c self handle-alt-key
            else
              attrs [ ATTR_CTRL ATTR_ALT or ] literal = if
                c self handle-ctrl-alt-key
              then
            then
          then
        then

        -1 +to counter
      repeat
    ; define handle-input

    \ Handle break
    :noname { self -- }
      [: attention-start-hook @ ?execute ;] try drop
    ; define handle-break

    \ Handle control-break
    :noname { self -- }
      reboot
    ; define handle-control-break
    
    \ Handle output
    :noname { self -- }
      false { updated }
      output-limit { countdown }
      begin
        0 { W^ buf }
        countdown 0> if
          buf 1 self output-stream recv-stream-no-block 0<>
        else
          false
        then if
          updated not if
            self term-lock claim-lock
            self clear-cursor
            true to updated
          then

          \ DEBUG
\          buf c@ [: ." >" h.2 space flush-console ;] console::with-serial-output
          \ END DEBUG
          
          buf c@ case
            escape of self handle-escape endof
            return of self handle-return endof
            linefeed of self handle-linefeed endof
            backspace of self handle-backspace endof
            tab of self handle-tab endof
            dup self handle-char
          endcase

          -1 +to countdown
          
          false
        else
          updated if
            self draw-cursor
            self do-update-display
            self term-lock release-lock
          then
          true
        then
      until
    ; define handle-output

    \ Handle an escape
    :noname ( self -- )
      [: { self }
        systick::systick-counter { start-time }
        begin
          self [: { self }
            self output-recv-buf output-recv-buf-size
            self output-stream peek-stream { size }
            size 0> if
              self output-recv-buf c@ [char] [ = if
                false 0 { found found-size }
                size 0 ?do
                  self output-recv-buf i + c@
                  dup [char] A >= over [char] Z <= and
                  swap dup [char] a >= swap [char] z <= and or  if
                    i 1+ to found-size
                    true to found
                    leave
                  then
                loop
                found if
                  self output-recv-buf found-size self parse-escape
                  found-size self output-stream skip-stream drop
                then
                found
              else
                true
              then
            else
              false
            then
          ;]
          escape-timeout systick::systick-counter start-time - - 0 max
          task::with-timeout
        until
      ;] try dup ['] task::x-timed-out = if 2drop 0 then ?raise
    ; define handle-escape

    \ Parse an escape
    :noname { addr bytes self -- }

      \ DEBUG
      \      addr bytes [: over + dump flush-console ;] console::with-serial-output
      \ END DEBUG
      
      addr bytes s" [?25h" equal-strings? if self handle-show-cursor exit then
      addr bytes s" [?25l" equal-strings? if self handle-hide-cursor exit then
      addr bytes s" [s" equal-strings? if self handle-save-cursor exit then
      addr bytes s" [u" equal-strings? if self handle-restore-cursor exit then
      addr bytes s" [6n" equal-strings? if
        self handle-query-cursor-position exit
      then
      addr bytes 1- + c@ { last-char }

      last-char [char] A = if addr bytes self handle-cursor-up exit then
      last-char [char] B = if addr bytes self handle-cursor-down exit then
      last-char [char] C = if addr bytes self handle-cursor-forward exit then
      last-char [char] D = if addr bytes self handle-cursor-back exit then
      last-char [char] E = if addr bytes self handle-cursor-next-line exit then
      last-char [char] F = if addr bytes self handle-cursor-prev-line exit then
      last-char [char] G = if addr bytes self handle-horiz-abs exit then
      last-char [char] H = if addr bytes self handle-cursor-pos exit then
      last-char [char] J = if addr bytes self handle-erase-in-display exit then
      last-char [char] K = if addr bytes self handle-erase-in-line exit then
      last-char [char] S = if addr bytes self handle-scroll-up exit then
      last-char [char] T = if addr bytes self handle-scroll-down exit then
      last-char [char] f = if addr bytes self handle-cursor-pos exit then
      last-char [char] m = if addr bytes self parse-color-style exit then
    ; define parse-escape

    \ Parse a decimal number in a string
    :noname { default addr bytes self -- u size }
      0 true { offset searching? }
      begin offset bytes < searching? and while
        addr offset + c@ dup [char] 0 >= swap [char] 9 <= and if
          1 +to offset
        else
          false to searching?
        then
      repeat
      offset 0<> if
        addr offset parse-integer drop offset
      else
        default 0
      then
    ; define parse-dec

    \ Handle showing the cursor
    :noname { self -- }
      true self cursor-visible !
    ; define handle-show-cursor
    
    \ Handle hiding the cursor
    :noname { self -- }
      false self cursor-visible !
    ; define handle-hide-cursor
    
    \ Handle saving the cursor
    :noname { self -- }
      self cursor-x @ self saved-cursor-x !
      self cursor-y @ self saved-cursor-y !
    ; define handle-save-cursor
    
    \ Handle restoring the cursor
    :noname { self -- }
      self saved-cursor-x @ self cursor-x !
      self saved-cursor-y @ self cursor-y !
    ; define handle-restore-cursor
    
    \ Handle querying the cursor position
    :noname ( self -- )
      base @ { saved-base }
      [: { self }
        10 base !
        escape self output-recv-buf c!
        [char] [ self output-recv-buf 1+ c!
        self output-recv-buf 2 + self cursor-y @ 1+ format-integer nip { size }
        [char] ; self output-recv-buf size 2 + + c!
        self output-recv-buf size 3 + + self cursor-x @ 1+ format-integer nip
        +to size
        [char] R self output-recv-buf size 3 + + c!
        self output-recv-buf size 4 + self input-string
      ;] try
      saved-base base !
      ?raise
    ; define handle-query-cursor-position
    
    \ Handle moving the cursor up
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        negate self cursor-y @ + 0 max self cursor-y !
      else
        drop
      then
    ; define handle-cursor-up
    
    \ Handle moving the cursor down
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self cursor-y @ + [ term-height 1- ] literal min self cursor-y !
      else
        drop
      then
    ; define handle-cursor-down
    
    \ Handle moving the cursor forward
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self cursor-x @ + [ term-width 1- ] literal min self cursor-x !
      else
        drop
      then
    ; define handle-cursor-forward
    
    \ Handle moving the cursor back
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        negate self cursor-x @ + 0 max self cursor-x !
      else
        drop
      then
    ; define handle-cursor-back
    
    \ Handle moving the cursor to the start of a following line
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        0 self cursor-x !
        self cursor-y @ + [ term-height 1- ] literal min self cursor-y !
      else
        drop
      then
    ; define handle-cursor-next-line
    
    \ Handle moving the cursor to the start of a preceding line
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        0 self cursor-x !
        negate self cursor-y @ + 0 max self cursor-y !
      else
        drop
      then
    ; define handle-cursor-prev-line
    
    \ Handle moving the cursor to a column
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        1- 0 max [ term-width 1- ] literal min self cursor-x !
      else
        drop
      then
    ; define handle-horiz-abs
    
    \ Handle setting the cursor position
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec { row row-bytes }
      addr 1+ row-bytes + c@ [char] ; = if
        1 addr 2 + row-bytes + bytes 2 - row-bytes - self parse-dec
        { col col-bytes }
        bytes 2 - row-bytes - col-bytes - 1 = if
          col 1- 0 max [ term-width 1- ] literal min self cursor-x !
          row 1- 0 max [ term-height 1- ] literal min self cursor-y !
        then
      else
        bytes 1- row-bytes - 1 = if
          0 self cursor-x !
          row 1- 0 max [ term-height 1- ] literal min self cursor-y !
        then
      then
    ; define handle-cursor-pos
    
    \ Handle erasing in the display
    :noname { addr bytes self -- }
      0 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        case
          0 of self handle-erase-down endof
          1 of self handle-erase-up endof
          2 of self handle-erase-screen endof
        endcase
      else
        drop
      then
    ; define handle-erase-in-display

    \ Erase a character
    :noname { x y self -- }
      term-width y * x + { offset }
      $20 self chars-buf offset + dup { char-addr } c@ <>
      0 self attrs-buf offset + dup { attrs-addr } c@ <> or
      self fg-color @ dup { fg-color }
      self fg-colors-buf offset + dup { fg-color-addr } c@ <> or
      self bk-color @ dup { bk-color }
      self bk-colors-buf offset + dup { bk-color-addr } c@ <> or if
        $20 char-addr c!
        fg-color fg-color-addr c!
        bk-color bk-color-addr c!
        0 attrs-addr c!
        x y self draw-char
      then
    ; define erase-char
    
    \ Handle erasing down
    :noname { self -- }
      term-height self cursor-y @ 1+ ?do
        term-width 0 ?do i j self erase-char loop
      loop
    ; define handle-erase-down

    \ Handle erasing up
    :noname { self -- }
      self cursor-y @ 0 ?do
        term-width 0 ?do i j self erase-char loop
      loop
    ; define handle-erase-up

    \ Handle erasing the screen
    :noname { self -- }
      term-height 0 ?do
        term-width 0 ?do i j self erase-char loop
      loop
    ; define handle-erase-screen
    
    \ Handle erasing in the line
    :noname { addr bytes self -- }
      0 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        case
          0 of self handle-erase-right endof
          1 of self handle-erase-left endof
          2 of self handle-erase-line endof
        endcase
      else
        drop
      then
    ; define handle-erase-in-line

    \ Handle erasing to the right
    :noname { self -- }
      self cursor-y @ { y }
      term-width self cursor-x @ ?do i y self erase-char loop
    ; define handle-erase-right

    \ Handle erasing to the left
    :noname { self -- }
      self cursor-y @ { y }
      self cursor-x @ 0 ?do i y self erase-char loop
    ; define handle-erase-left

    \ Handle erasing the current line
    :noname { self -- }
      self cursor-y @ { y }
      term-width 0 ?do i y self erase-char loop
    ; define handle-erase-line
    
    \ Handle scrolling up
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self scroll-up
      else
        drop
      then
    ; define handle-scroll-up
    
    \ Handle scrolling down
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self scroll-down
      else
        drop
      then
    ; define handle-scroll-down
    
    \ Parse the text color and style
    :noname { addr bytes self -- }
      1 +to addr
      -1 +to bytes
      addr bytes { valid-addr valid-bytes }
      begin valid-bytes 0> while
        0 valid-addr valid-bytes self parse-dec { style style-bytes }
        style-bytes 0= valid-bytes 1 = and if
          1 +to style-bytes
        else
          style-bytes valid-bytes 1- < if
            valid-addr style-bytes + c@ [char] ; <> if exit then
            1 +to style-bytes
          then
        then
        style-bytes +to valid-addr
        style-bytes negate +to valid-bytes
        valid-bytes 1 = if 0 to valid-bytes then
      repeat
      begin bytes 0> while
        0 addr bytes self parse-dec { style style-bytes }
        style-bytes 0= bytes 1 = and if
          1 +to style-bytes
        else
          style-bytes bytes 1- < if
            1 +to style-bytes
          then
        then
        style-bytes +to addr
        style-bytes negate +to bytes
        style self handle-color-style
        bytes 1 = if 0 to bytes then
      repeat
    ; define parse-color-style

    \ Handle setting the text color and style
    :noname { style self -- }
      style
      dup ansi-term::black >= over ansi-term::white <= and if
        self handle-fg-color exit
      then
      dup ansi-term::b-black >= over ansi-term::b-white <= and if
        self handle-fg-color exit
      then
      dup ansi-term::black ansi-term::background >=
      over ansi-term::white ansi-term::background <= and if
        self handle-bk-color exit
      then
      dup ansi-term::b-black ansi-term::background >=
      over ansi-term::b-white ansi-term::background <= and if
        self handle-bk-color exit
      then
      case
        0 of self handle-reset-color-style endof
        1 of self handle-bold endof
        2 of self handle-dim endof
        4 of self handle-underline endof
        7 of self handle-reverse endof
        22 of self handle-normal endof
        24 of self handle-not-underline endof
        27 of self handle-not-reverse endof
        39 of self handle-default-fg-color endof
        49 of self handle-default-bk-color endof
      endcase
    ; define handle-color-style
    
    \ Handle setting the foreground color
    :noname { color self -- }
      color self fg-color !
    ; define handle-fg-color

    \ Handle setting the background color
    :noname { color self -- }
      color ansi-term::ansi-term-internal::background-offset - self bk-color !
    ; define handle-bk-color

    \ Handle resetting the color and style
    :noname { self -- }
      self handle-normal
      self handle-default-fg-color
      self handle-default-bk-color
    ; define handle-reset-color-style

    \ Handle setting bold style
    :noname { self -- }
      attr-bold self attrs bis!
    ; define handle-bold

    \ Handle setting dim style
    :noname { self -- }
      attr-dim self attrs bis!
    ; define handle-dim

    \ Handle setting underlined style
    :noname { self -- }
      attr-underline self attrs bis!
    ; define handle-underline

    \ Handle setting reversed style
    :noname { self -- }
      attr-reverse self attrs bis!
    ; define handle-reverse

    \ Handle setting normal style
    :noname { self -- }
      0 self attrs !
    ; define handle-normal

    \ Handle setting non-underlined style
    :noname { self -- }
      attr-underline self attrs bic!
    ; define handle-not-underline

    \ Handle setting non-reversed style
    :noname { self -- }
      attr-reverse self attrs bic!
    ; define handle-not-reverse

    \ Handle setting the default foreground color
    :noname { self -- }
      default-fg-color self fg-color !
    ; define handle-default-fg-color

    \ Handle setting the default background color
    :noname { self -- }
      default-bk-color self bk-color !
    ; define handle-default-bk-color

    \ Handle a carriage return
    :noname { self -- }
      0 self cursor-x !
    ; define handle-return
    
    \ Handle a linefeed
    :noname { self -- }
      self cursor-y @ term-height 1- < if
        1 self cursor-y +!
      else
        1 self scroll-up
      then
    ; define handle-linefeed
    
    \ Handle a backspace
    :noname { self -- }
      -1 self cursor-x +!
      self cursor-x @ 0< if
        term-width 1- self cursor-x !
        self cursor-y @ 1- 0 max self cursor-y !
      then
    ; define handle-backspace

    \ Handle a tab
    :noname { self -- }
      self cursor-x @ 1+ tab-size align term-width min self cursor-x @ - 0 ?do
        bl self handle-char
      loop
    ; define handle-tab
    
    \ Handle an ordinary character
    :noname { c self -- }
      c $20 < c $7E > or if exit then
      self cursor-x @ self cursor-y @ { x y }
      x term-width = if
        0 to x
        y term-height 1- >= if 1 self scroll-up else y 1+ to y then
      then
      y self cursor-y !
      x 1+ self cursor-x !
      term-width y * x + { offset }
      c self chars-buf offset + dup { char-addr } c@ <>
      self attrs @ dup { this-attrs }
      self attrs-buf offset + dup { attrs-addr } c@ <> or
      self fg-color @ dup { this-fg-color }
      self fg-colors-buf offset + dup { fg-color-addr } c@ <> or
      self bk-color @ dup { this-bk-color }
      self bk-colors-buf offset + dup { bk-color-addr } c@ <> or if
        c char-addr c!
        this-attrs attrs-addr c!
        this-fg-color fg-color-addr c!
        this-bk-color bk-color-addr c!
        x y self draw-char
      then
    ; define handle-char
    
    \ Carry out an operation with the PicoCalc terminal locked. Note that
    \ executing operations that print to the PicoCalc terminal should be avoided
    \ because they may block indefinitely.
    :noname ( xt self -- )
      term-lock with-lock
    ; define do-with-term-lock

    \ Draw the cursor
    :noname { self -- }
      true self cursor-x @ self cursor-y @ self draw-char-with-cursor
    ; define draw-cursor

    \ Clear the cursor
    :noname { self -- }
      self cursor-visible @ { saved-cursor-visible }
      false self cursor-visible !
      self draw-cursor
      saved-cursor-visible self cursor-visible !
    ; define clear-cursor

    \ Draw a character
    :noname { x y self -- }
      false x y self draw-char-with-cursor
    ; define draw-char

    \ Input a string
    :noname ( c-addr u self -- )
      [: input-stream send-stream-no-block ;] try
      ?dup if dup ['] x-would-block = if 2drop 2drop else ?raise then then
    ; define input-string

    \ Test the output stream for room for a byte
    :noname ( self -- room )
      output-stream stream-full? not
    ; define >term?

    \ Enqueue a byte to the output stream
    :noname { W^ c self -- }
      c 1 self output-stream send-stream
    ; define >term

    \ Test the input stream for availability of a byte
    :noname ( self -- available? )
      input-stream stream-empty? not
    ; define term>?

    \ Dequeue a byte from the input stream
    :noname { self -- c }
      0 { W^ buf }
      buf 1 self input-stream recv-stream drop
      buf c@
    ; define term>

    \ Flush terminal output
    :noname { self -- }
      begin self output-stream stream-empty? not while
        pause-reschedule-last
      repeat
    ; define flush-term

  end-implement

  \ Get the terminal pixel dimensions
  : term-pixels-dim@ ( -- width height ) display-width display-height ;

  \ Get the terminal dimensions in characters
  : term-dim@ ( -- width height ) term-width term-height ;

  \ Get the terminal character dimensions
  : term-char-dim@ ( -- width height ) char-width char-height ;

end-module
