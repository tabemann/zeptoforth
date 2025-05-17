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
  pixmap8 import
  pixmap8-internal import
  font import
  simple-font-6x8 import
  ili9488-8-common import
  ili9488-8-spi import
  picocalc-keys import
  task import
  lock import
  sema import
  stream import
  console import
  
  begin-module picocalc-term-internal

    \ Initialize the 6x8 simple font
    initializer simple-font-6x8::init-simple-font-6x8

    \ Font character with
    6 constant char-width

    \ Font character height
    8 constant char-height

    \ Display width
    320 constant display-width
    
    \ Display height
    320 constant display-height

    \ Terminal width
    display-width char-width / constant term-width

    \ Terminal height
    display-height char-height / constant term-height

    \ Display SPI device
    1 constant display-spi-device

    \ Display SCK pin
    10 constant display-spi-sck-pin

    \ Display TX pin
    11 constant display-spi-tx-pin

    \ Display RX pin
    12 constant display-spi-rx-pin

    \ Display CS pin
    13 constant display-spi-cs-pin

    \ Display DC pin
    14 constant display-dc-pin

    \ Display RST pin
    15 constant display-rst-pin

    \ Do we invert the display
    true constant display-invert
    
    \ The input stream size
    256 constant input-stream-size

    \ The output stream-size
    256 constant output-stream-size

    \ The input receive buffer size
    64 constant output-recv-buf-size

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

  \ PicoCalc terminal class
  <object> begin-class <picocalc-term>

    continue-module picocalc-term-internal
      
      \ The display
      <ili9488-8-spi> class-size member display-intf
      
      \ The display buffer
      display-width display-height pixmap8-buf-size member display-buf

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

      \ Are we attempting to destroy the terminal
      cell member try-destroy
      
      \ The destroy semaphore
      sema-size member destroy-sema

      \ The input stream
      input-stream-size stream-size member input-stream

      \ The output stream
      output-stream-size stream-size member output-stream

      \ The input receive buffer
      output-recv-buf-size cell align member output-recv-buf

      \ The console input data structure
      console-internal::console-stream-data-size cell align
      member console-input-data

      \ The console output data structure
      console-internal::console-out-stream-data-size cell align
      member console-output-data

      \ Run the terminal
      method run-term ( self -- )

      \ Draw the cursor
      method draw-cursor ( self -- )

      \ Clear the cursor
      method clear-cursor ( self -- )

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
      
      \ Flush terminal output
      method flush-term ( self -- )

  end-module

    \ Initialize the PicoCalc terminal
    method init-term ( self -- )

    \ Carry out an operation with the PicoCalc terminal locked
    method with-term ( xt self -- )
    
  end-class

  \ Implement PicoCalc terminal class
  <picocalc-term> begin-implement

    \ Constructor
    :noname { self -- }
      self <object>->new
      display-spi-tx-pin display-spi-sck-pin display-dc-pin display-spi-cs-pin
      display-rst-pin
      display-invert self display-buf display-width display-height
      display-spi-device <ili9488-8-spi> self display-intf init-object
      <picocalc-keys> self key-intf init-object
      0 self term-task !
      0 self term-task-mailbox !
      false self try-destroy !
      self term-lock init-lock
      no-sema-limit 1 self destroy-sema init-sema
      input-stream-size self input-stream init-stream
      output-stream-size self output-stream output-stream
      self input-stream self console-input-data
      console-internal::init-console-stream-input
      self output-stream self console-output-data
      console-internal::init-console-stream-output
    ; define new

    \ Destructor
    :noname { self -- }
      true self try-destroy !
      self destroy-sema take
      self key-intf destroy
      self display-intf destroy
      self <object>->destroy
    ; define destroy
    
    \ Initialize the PicoCalc terminal
    :noname { self -- }
      self destroy-sema take
      self chars-buf [ term-width term-height * ] literal 0 fill
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
      self display-intf clear-pixmap
      self draw-cursor
      self display-intf update-display
      self key-intf init-picocalc-keys
      self 1 ['] run-term 1024 256 1024 spawn self term-task !
      c" term" self term-task @ task-name!
      self term-task-mailbox 1 self term-task @ config-notify
      self term-task @ run
    ; define init-term

    \ Run the PicoCalc terminal
    :noname { self -- }
      begin self try-destroy @ not while
        self handle-input
        self handle-output
        pause
      repeat
      self destroy-sema give
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
        c { W^ c } c 1 self input-string
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
        c KEY_ESC = if s\" \x1B" self input-string 100 ms exit then
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
        c convert-control { W^ c } c 1 self input-string
      then
    ; define handle-ctrl-key
    
    \ Handle input
    :noname { self -- }
      begin self key-intf picocalc-keys>? while
        self key-intf picocalc-keys> { attrs W^ c }
        attrs 0= if
          c self handle-normal-key
        else
          attrs ATTR_CTRL = if
            c self handle-ctrl-key
          then
        then
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
      begin
        false { updated }
        0 { W^ buf }
        buf 1 self output-stream recv-stream-no-block 0<> if
          updated not if
            self term-lock claim-lock
            true to updated
          then
          buf c@ case
            escape of self handle-escape endof
            return of self handle-return endof
            linefeed of self handle-linefeed endof
            backspace of self handle-backspace endof
            tab of self handle-tab endof
            dup self handle-char
          endcase
          false
        else
          updated if
            display-intf update-display
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
          [: { self }
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
      addr bytes s" [?25h" equal-strings? if self handle-show-cursor exit then
      addr bytes s" [?25l" equal-strings? if self handle-hide-cursor exit then
      addr bytes s" [s" equal-strings? if self handle-save-cursor exit then
      addr bytes s" [u" equal-strings? if self handle-restore-cursor exit then
      addr bytes s" [6n" equal-strings? if
        self handle-query-cursor-position exit
      then
      addr bytes 1- + c@ { last-char }
      last-char [char] = if addr bytes self handle-cursor-up exit then
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
      self draw-cursor
    ; define handle-show-cursor
    
    \ Handle hiding the cursor
    :noname { self -- }
      self clear-cursor
      false self cursor-visible !
    ; define handle-hide-cursor
    
    \ Handle saving the cursor
    :noname { self -- }
      self cursor-x @ self saved-cursor-x !
      self cursor-y @ self saved-cursor-y !
    ; define handle-save-cursor
    
    \ Handle restoring the cursor
    :noname { self -- }
      self clear-cursor
      self saved-cursor-x @ self cursor-x !
      self saved-cursor-y @ self cursor-y !
      self draw-cursor
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
        [char] R self output-recv-buf size 2 + + c!
        self output-recv-buf size 3 + self input-string
      ;] try
      saved-base base !
      ?raise
    ; define handle-query-cursor-position
    
    \ Handle moving the cursor up
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self clear-cursor
        negate self cursor-y @ + 0 max self cursor-y !
        self draw-cursor
      else
        drop
      then
    ; define handle-cursor-up
    
    \ Handle moving the cursor down
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self clear-cursor
        self cursor-y @ + [ term-height 1- ] literal min self cursor-y !
        self draw-cursor
      else
        drop
      then
    ; define handle-cursor-down
    
    \ Handle moving the cursor forward
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self clear-cursor
        self cursor-x @ + [ term-width 1- ] literal min self cursor-x !
        self draw-cursor
      else
        drop
      then
    ; define handle-cursor-forward
    
    \ Handle moving the cursor back
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self clear-cursor
        negate self cursor-x @ + 0 max self cursor-x !
        self draw-cursor
      else
        drop
      then
    ; define handle-cursor-back
    
    \ Handle moving the cursor to the start of a following line
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self clear-cursor
        0 self cursor-x !
        self cursor-y @ + [ term-height 1- ] literal min self cursor-y !
        self draw-cursor
      else
        drop
      then
    ; define handle-cursor-next-line
    
    \ Handle moving the cursor to the start of a preceding line
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self clear-cursor
        0 self cursor-x !
        negate self cursor-y @ + 0 max self cursor-y !
        self draw-cursor
      else
        drop
      then
    ; define handle-cursor-prev-line
    
    \ Handle moving the cursor to a column
    :noname { addr bytes self -- }
      1 addr 1+ bytes 1- self parse-dec bytes 2 - = if
        self clear-cursor
        1- 0 max [ term-width 1- ] literal min self cursor-x !
        self draw-cursor
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
          self clear-cursor
          col 1- 0 max [ term-width 1- ] literal min self cursor-x !
          row 1- 0 max [ term-height 1- ] literal min self cursor-y !
          self draw-cursor
        then
      else
        bytes 1- row-bytes - 1 = if
          self clear-cursor
          0 self cursor-x !
          row 1- 0 max [ term-height 1- ] literal min self cursor-y !
          self draw-cursor
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

    \ Handle erasing down
    :noname { self -- }
      self fg-color @ { fg-color }
      self bk-color @ { bk-color }
      term-height self cursor-y @ 1+ ?do
        term-width 0 ?do
          term-width j * i + { offset }
          fg-color self fg-colors-buf offset + c!
          bk-color self bk-colors-buf offset + c!
          0 self attrs-buf offset + c!
          i j self draw-char
        loop
      loop
    ; define handle-erase-down

    \ Handle erasing up
    :noname { self -- }
      self fg-color @ { fg-color }
      self bk-color @ { bk-color }
      self cursor-y @ 0 ?do
        term-width 0 ?do
          term-width j * i + { offset }
          fg-color self fg-colors-buf offset + c!
          bk-color self bk-colors-buf offset + c!
          0 self attrs-buf offset + c!
          i j self draw-char
        loop
      loop
    ; define handle-erase-up

    \ Handle erasing the screen
    :noname { self -- }
      self fg-color @ { fg-color }
      self bk-color @ { bk-color }
      term-height 0 ?do
        term-width 0 ?do
          term-width j * i + { offset }
          fg-color self fg-colors-buf offset + c!
          bk-color self bk-colors-buf offset + c!
          0 self attrs-buf offset + c!
          i j self draw-char
        loop
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
      self fg-color @ { fg-color }
      self bk-color @ { bk-color }
      self cursor-y @ { y }
      self cursor-x @ 0 ?do
        term-width y * i + { offset }
        fg-color self fg-colors-buf offset + c!
        bk-color self bk-colors-buf offset + c!
        0 self attrs-buf offset + c!
        i y self draw-char
      loop
    ; define handle-erase-right

    \ Handle erasing to the left
    :noname { self -- }
      self fg-color @ { fg-color }
      self bk-color @ { bk-color }
      self cursor-y @ { y }
      term-width self cursor-x @ ?do
        term-width y * i + { offset }
        fg-color self fg-colors-buf offset + c!
        bk-color self bk-colors-buf offset + c!
        0 self attrs-buf offset + c!
        i y self draw-char
      loop
    ; define handle-erase-left

    \ Handle erasing the current line
    :noname { self -- }
      self fg-color @ { fg-color }
      self bk-color @ { bk-color }
      self cursor-y @ { y }
      term-width 0 ?do
        term-width y * i + { offset }
        fg-color self fg-colors-buf offset + c!
        bk-color self bk-colors-buf offset + c!
        0 self attrs-buf offset + c!
        i y self draw-char
      loop
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
        style-bytes valid-bytes 1- < if
          addr style-bytes + c@ [char] ; <> if exit then
          1 +to style-bytes
        then
        style-bytes +to valid-addr
        style-bytes negate +to valid-bytes
      repeat
      begin bytes 0> while
        0 addr bytes self parse-dec { style style-bytes }
        style-bytes valid-bytes 1- < if
          1 +to style-bytes
        then
        style-bytes +to addr
        style-bytes negate +to bytes
        style self handle-color-style
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
        38 of self handle-default-fg-color endof
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

    \ Handle resetting the color and stle
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
      self clear-cursor
      0 self cursor-x !
      self draw-cursor
    ; define handle-return
    
    \ Handle a linefeed
    :noname { self -- }
      self cursor-y @ term-height 1- < if
        self clear-cursor
        1 self cursor-y +!
        self draw-cursor
      else
        1 self scroll-up
      then
    ; define handle-linefeed
    
    \ Handle a backspace
    :noname { self -- }
      self clear-cursor
      -1 self cursor-x +!
      self cursor-x @ 0< if
        term-width 1- self cursor-x !
        self cursor-y @ 1- 0 max self cursor-y !
      then
      self draw-cursor
    ; define handle-backspace

    \ Handle a tab
    :noname { self -- }
      self cursor-x @ 1+ tab-size align term-width min self cursor-x @ - 0 ?do
        bl self handle-char
      loop
    ; define handle-tab
    
    \ Handle an ordinary character
    :noname { c self -- }
      self clear-cursor
      self cursor-x @ self cursor-y @ { x y }
      x term-width = if
        0 to x
        y term-height 1- >= if 1 self scroll-up else y 1+ to y then
      then
      x 1+ self cursor-x !
      term-width y * x + { offset }
      c self chars-buf offset + c!
      self attrs @ self attrs-buf offset + c!
      self fg-color @ self fg-colors-buf offset + c!
      self bk-color @ self bk-colors-buf offset + c!
      x y draw-char
      self draw-cursor
    ; define handle-char
    
    \ Carry out an operation with the PicoCalc terminal locked
    :noname ( xt self -- )
      term-lock with-lock
    ; define with-term

    \ Draw the cursor
    :noname { self -- }
      self cursor-x @ self cursor-y @ self draw-char
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
      term-width y * x + { offset }
      offset self chars-buf + c@ { c }
      offset self attrs-buf + c@ { attr }
      c 0= if bl to c then
      offset self fg-colors-buf + c@
      offset self bk-colors-buf + c@
      x self cursor-x @ = y self cursor-y @ = and
      self cursor-visible @ and if swap then
      attr attr-reverse and if swap then
      get-color swap attr modify-color get-color
      { char-bk-color char-fg-color }
      char-width x * char-height y * { display-x display-y }
      char-bk-color display-x display-y char-width char-height
      self display-intf draw-rect-const
      char-fg-color c display-x display-y
      self display-intf simple-font-6x8::a-simple-font-6x8 draw-char-to-pixmap8
      attr attr-underline and if
        char-fg-color display-x display-y char-height 1- + char-width 1
        self display-intf draw-rect-const
      then
    ; define draw-char

    \ Scroll up by a number of lines
    :noname { lines self -- }
      lines term-height min to lines
      self clear-cursor
      lines [ char-height display-width * ] literal * { pixels }
      self display-buf pixels + self display-buf
      [ display-width display-height * ] literal pixels - move
      [ term-height char-height * ] literal lines char-height * - { fill-y }
      self bk-color @ get-color
      0 fill-y display-width display-height fill-y - self display-intf
      draw-rect-const
      self draw-cursor
      self display-intf set-dirty
    ; define scroll-up

    \ Scroll down by a number of lines
    :noname { lines self -- }
      lines term-height min to lines
      self clear-cursor
      lines [ char-height display-width * ] literal * { pixels }
      self display-buf self display-buf pixels +
      [ display-width display-height * ] literal pixels - move
      [ term-height char-height * ] literal lines char-height * - { fill-y }
      self bk-color @ get-color
      0 0 display-width fill-y self display-intf draw-rect-const
      self draw-cursor
      self display-intf set-dirty
    ; define scroll-down

    \ Input a string
    :noname ( c-addr u self -- )
      [: input-stream send-stream-no-block ;] try
      ?dup if dup ['] x-would-block = if 2drop 2drop else ?raise then then
    ; define input-string

    \ Flush terminal output
    :noname { self -- }
      self console-output-data console-internal::console-io-flush
      begin self output-stream stream-empty? not while
        pause-reschedule-last
      repeat
    ; define flush-term

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
    shared-term console-input-data console-internal::console-io
    shared-term console-input-data console-internal::console-io? rot with-input
  ;

  \ Set the current output to a PicoCalc terminal within an xt
  : with-term-output ( xt -- )
    shared-term console-output-data console-internal::console-io swap
    shared-term console-output-data console-internal::console-io? swap
    shared-term ['] flush-term swap with-output
    shared-term console-output-data
    console-internal::flush-console-stream-output
  ;

  \ Set the current error output to a PicoCalc terminal within an xt
  : with-term-error-output ( xt -- )
    shared-term console-output-data console-internal::console-io swap
    shared-term console-output-data console-internal::console-io? swap
    shared-term ['] flush-term swap with-error-output
    shared-term console-output-data
    console-internal::flush-console-stream-output    
  ;

  \ Set the current console to a PicoCalc terminal
  : term-console ( -- )
    shared-term console-input-data console-internal::console-io key-hook !
    shared-term console-input-data console-internal::console-io? key?-hook !
    shared-term console-output-data console-internal::console-io emit-hook !
    shared-term console-output-data console-internal::console-io? emit?-hook !
    shared-term ['] flush-term flush-console-hook !
    shared-term console-output-data console-internal::console-io
    error-emit-hook !
    shared-term console-output-data console-internal::console-io?
    error-emit?-hook !
    shared-term ['] flush-term error-flush-console-hook !
    picocalc-welcome-displayed not if
      picocalc-welcome
      true to picocalc-welcome-displayed
    then
  ;
  
end-module
