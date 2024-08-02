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

compile-to-flash

begin-module more-internal

  ansi-term import
  
  \ Current MORE nesting level
  0 value more-level

  \ Current screen height
  -1 value more-screen-height

  \ Current screen width
  -1 value more-screen-width
  
  \ Saved EMIT hook
  0 value saved-emit-hook

  \ Current row count
  0 value current-row

  \ Current column count
  0 value current-col

  \ Suppress newline
  false value suppress-newline
  
  \ Character modes
  0 constant normal-mode
  1 constant escape-mode
  2 constant csi-mode
  
  \ Character mode
  normal-mode value char-mode
  
  \ Characters
  $0A constant linefeed
  $0D constant carriage-return
  $08 constant backspace
  $1B constant escape

  \ Get whether a byte is the start of a unicode code point greater than 127
  : unicode-start? ( b -- flag ) $C0 and $C0 = ;

  \ Get whether a byte is part of a unicode code point greater than 127
  : unicode? ( b -- flag ) $80 and 0<> ;

  \ Pager exit message
  : x-pager-exit ( -- ) ;
  
  \ Really emit a character
  : do-emit ( c -- ) saved-emit-hook execute ;
  
  \ Handle a backspace
  : do-backspace ( c -- )
    do-emit
    current-col 1- 0 max to current-col
  ;

  \ Display pager message
  : do-pager-message ( -- )
    saved-emit-hook emit?-hook @ flush-console-hook @ [:
      more-screen-height 1- 0 go-to-coord
      ." *** Press q to exit, any other key to continue ***"
      key dup [char] q = swap [char] Q = or
      more-screen-height 1- 0 go-to-coord
      erase-end-of-line
      more-screen-height 1- current-col go-to-coord
    ;] console::with-output
    triggers x-pager-exit
    0 to current-row
  ;

  \ Handle a linefeed
  : do-linefeed ( c -- )
    do-emit
    suppress-newline if
      1 to current-row
      false to suppress-newline
    else
      1 +to current-row
    then
    current-row more-screen-height 1- >= if
      do-pager-message
    then
  ;

  \ Handle a carriage return
  : do-carriage-return ( c -- )
    do-emit
    0 to current-col
  ;

  \ Handle an escape
  : do-escape ( c -- )
    do-emit
    escape-mode to char-mode
  ;

  \ Advance one character
  : do-advance-char ( -- )
    1 +to current-col
    current-col more-screen-width >= if
      0 to current-col
      suppress-newline if
        1 to current-row
        false to suppress-newline
      else
        1 +to current-row
      then
    then
    current-row more-screen-height 1- >= suppress-newline not and if
      carriage-return do-emit
      linefeed do-emit
      do-pager-message
    then
  ;
  
  \ Handle a normal character
  : do-normal { c -- }
    c do-emit
    normal-mode char-mode = if
      do-advance-char
    else
      escape-mode char-mode = if
        c [char] [ = if
          csi-mode to char-mode
        else
          normal-mode to char-mode
        then
      else
        c [char] m = if
          normal-mode to char-mode
        then
      then
    then
  ;

  \ Special byte table
  create special-table
  ' do-emit , \ $00
  ' do-emit , \ $01
  ' do-emit , \ $02
  ' do-emit , \ $03
  ' do-emit , \ $04
  ' do-emit , \ $05
  ' do-emit , \ $06
  ' do-emit , \ $07
  ' do-backspace , \ $08
  ' do-emit , \ $09
  ' do-linefeed , \ $0A
  ' do-emit , \ $0B
  ' do-emit , \ $0C
  ' do-carriage-return , \ $0D
  ' do-emit , \ $0E
  ' do-emit , \ $0F
  ' do-emit , \ $10
  ' do-emit , \ $11
  ' do-emit , \ $12
  ' do-emit , \ $13
  ' do-emit , \ $14
  ' do-emit , \ $15
  ' do-emit , \ $16
  ' do-emit , \ $17
  ' do-emit , \ $18
  ' do-emit , \ $19
  ' do-emit , \ $1A
  ' do-escape , \ $1B
  ' do-emit , \ $1C
  ' do-emit , \ $1D
  ' do-emit , \ $1E
  ' do-emit , \ $1F
  
  \ Emit handler
  : do-emit { c -- }
    c $FF and to c
    c bl u< if
      c dup cells special-table + @ execute
    else
      c $7F u< if
        c do-normal
      else
        c unicode-start? if
          c do-normal
        else
          c unicode? if
            c do-emit
          then
        then
      then
    then
  ;
  
end-module> import

\ Implement MORE
: more ( xt -- )
  more-level { saved-more-level }
  [: { xt }
    1 +to more-level
    more-level 1 = if
      ansi-term::get-terminal-size
      to more-screen-width to more-screen-height
      emit-hook @ to saved-emit-hook
      ansi-term::get-cursor-position to current-col to current-row
      current-row more-screen-height 1- = to suppress-newline
      normal-mode to char-mode
      xt ['] do-emit emit?-hook @ flush-console-hook @ [:
        try
        dup ['] x-pager-exit = if begin depth 0> while drop repeat 0 then
        ?raise
      ;] console::with-output
    else
      xt execute
    then
  ;] try
  saved-more-level to more-level
  ?raise
;

reboot
