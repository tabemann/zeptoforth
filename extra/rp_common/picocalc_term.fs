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
  simple-font-6x8
  ili9488-8-common import
  ili9488-spi-8 import
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

    \ The input stream size
    256 constant input-stream-size

    \ The output stream-size
    256 constant output-stream-size

    \ The terminal colors
    create term-colors
    0 0 0 rgb8 c, \ black
    223 0 0 rgb8 c, \ red
    0 223 0 rgb8 c, \ green
    223 223 0 rgb8 c, \ yellow
    0 0 191 rgb8 c, \ blue
    223 0 191 rgb8 c, \ magenta
    0 223 191 rgb8 c, \ cyan
    223 223 191 rgb8 c, \ white
    128 128 128 rgb8 c, \ b-black
    256 128 128 rgb8 c, \ b-red
    128 256 128 rgb8 c, \ b-green
    256 256 128 rgb8 c, \ b-yellow
    128 128 256 rgb8 c, \ b-blue
    256 128 256 rgb8 c, \ b-magenta
    128 256 256 rgb8 c, \ b-cyan
    256 256 256 rgb8 c, \ b-white

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

      \ The console input data structure
      console-internal::console-stream-data-size member console-input-data

      \ The console output data structure
      console-internal::console-out-stream-data-size member console-output-data

      \ Run the terminal
      method run-term ( self -- )

      \ Draw the cursor
      member draw-cursor ( self -- )

      \ Draw a character
      method draw-char ( x y self -- )
      
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
      display-rst-pin self display-buf display-width display-height
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
      ansi-term::white fill
      self bk-colors-buf [ term-width term-height * ] literal
      ansi-term::black fill
      ansi-term::white self fg-color !
      ansi-term::black self bk-color !
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
        
      repeat
      self destroy-sema give
    ; define run-term

    \ Carry out an operation with the PicoCalc terminal locked
    :noname ( xt self -- )
      term-lock with-lock
    ; define with-term

    \ Draw the cursor
    :noname { self -- }
      self cursor-x @ self cursor-y @ draw-char
    ; define draw-cursor

    \ Draw a character
    :noname { x y self -- }
      term-width y * x + { offset }
      offset self chars-buf + c@ { c }
      c 0= if bl to c then
      offset self fg-color + c@ get-color
      offset self bk-color + c@ get-color
      x self cursor-x @ = y self cursor-y @ = and
      self cursor-visible @ and if swap then
      { char-fg-color char-bk-color }
      char-width x * char-height y * { display-x display-y }
      char-bk-color display-x display-y char-width char-heigh
      self display-intf draw-rect-const
      char-fg-color c display-x display-y
      self display-intf simple-font-6x8::a-simple-font-6x8 draw-char-to-pixmap8
    ; define draw-char

  end-implement
  
  \ The PicoCalc terminal
  <picocalc-term> class-size buffer: shared-term

  continue-module picocalc-term-internal
    
    \ Initialize the PicoCalc terminal
    : init-shared-term ( -- )
      <picocalc-term> shared-term init-object
      shared-term init-picocalc-term
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
    shared-term console-output-data console-internal::console-io-flush swap
    with-output
    shared-term console-output-data
    console-internal::flush-console-stream-output
  ;

  \ Set the current error output to a PicoCalc terminal within an xt
  : with-term-error-output ( xt -- )
    shared-term console-output-data console-internal::console-io swap
    shared-term console-output-data console-internal::console-io? swap
    shared-term console-output-data console-internal::console-io-flush swap
    with-error-output
    shared-term console-output-data
    console-internal::flush-console-stream-output    
  ;

  \ Set the current console to a PicoCalc terminal
  : term-console { term -- }
    shared-term console-input-data console-internal::console-io key-hook !
    shared-term console-input-data console-internal::console-io? key?-hook !
    shared-term console-output-data console-internal::console-io emit-hook !
    shared-term console-output-data console-internal::console-io? emit?-hook !
    shared-term console-output-data console-internal::console-io-flush
    flush-console-hook !
    shared-term console-output-data console-internal::console-io
    error-emit-hook !
    shared-term console-output-data console-internal::console-io?
    error-emit?-hook !
    shared-term console-output-data console-internal::console-io-flush
    error-flush-console-hook !
  ;
  
end-module
