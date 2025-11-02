\ Copyright (c) 2020-2025 Travis Bemann
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

begin-module more-words-internal

  internal import
  ansi-term import
  
  \ Actually print a string in columns of 20 characters, taking up more than one
  \ column if necessary
  : words-column ( b-addr bytes column1 cols -- column2 )
    { cols }
    over 0> if
      over words-col-width @ / + 1+
      dup term-cols @ cols min words-col-width @ / >= if
      drop 0
      then
      >r
      dup 0> if
        tuck type
        r@ 0<> if
          words-col-width @ mod words-col-width @ swap -
          begin dup 0> while 1- space repeat drop
        else
          drop
        then
        r@ 0= if cr then
      else
        2drop
      then
      r>
    else
      -rot 2drop
    then
  ;
  
  \ Implement the pager
  : pager { row rows -- continue }
    row 1+ rows 1- umod 0= if
      ." *** Press q to exit, any other key to continue ***"
      key dup [char] q <> swap [char] Q <> and
      rows 1- 0 go-to-coord
      erase-end-of-line
    else
      true
    then
  ;
  
  \ Print a string in one out of four columns, taking up more than one column
  \ if necessary, and invoking the pager prompt as needed
  : more-words-column-wrap { addr len col row rows cols -- col' row' continue }
    term-cols @ cols min words-col-width @ / col - words-col-width @ * len < if
      cr row rows pager if
        addr len 0 cols words-column row 1+ true
      else
        0 row false
      then
    else
      addr len col cols words-column dup 0= if
        row rows pager if
          row 1+ true
        else
          row false
        then
      else
        row true
      then
    then
  ;
  
  \ Display all the words in a dictionary starting at a given column, and
  \ returning the next column and the next row
  : more-words-dict { dict wid col row rows cols -- col' row' continue }
    begin dict while
      dict hidden? not dict wordlist-id h@ wid = and if
        dict word-name count col row rows cols more-words-column-wrap
        -rot to row to col not if
          col row false exit
        then
      then
      dict next-word @ to dict
    repeat
    col row true
  ;

  \ Display all the words in a dictionary starting at a given column, and
  \ returning the next column and the next row
  : more-lookup-dict
    { addr len dict wid col row rows cols -- col' row' continue }
    begin dict while
      dict hidden? not dict wordlist-id h@ wid = and if
        addr len dict word-name count prefix? if
          dict word-name count col row rows cols more-words-column-wrap
          -rot to row to col not if
            col row false exit
          then
        then
      then
      dict next-word @ to dict
    repeat
    col row true
  ;

end-module> import

\ Lookup a word by its prefix with a pager
: more-lookup ( "name" -- )
  ram-here
  [:
    ansi-term::get-terminal-size { rows cols }
    cr token dup 0<> averts x-token-expected { addr len }
    addr len ram-latest internal::find-prefix-len
    addr len flash-latest internal::find-prefix-len max to len
    0 0 { col row }
    internal::unique-order 2* over + swap ?do
      addr len ram-latest i h@ col row rows cols
      more-lookup-dict -rot to row to col not if exit then
      addr len flash-latest i h@ col row rows cols
      more-lookup-dict -rot to row to col not if exit then
    2 +loop
    cr
  ;] try
  swap ram-here! ?raise
;

\ Lookup a word by its prefix in a wordlist with a pager
: more-lookup-in ( wid "name" -- )
  { wid }
  ansi-term::get-terminal-size { rows cols }
  cr token dup 0<> averts x-token-expected { addr len }
  addr len ram-latest internal::find-prefix-len
  addr len flash-latest internal::find-prefix-len max to len
  0 0 { col row }
  addr len ram-latest wid col row rows cols
  more-lookup-dict -rot to row to col not if exit then
  addr len flash-latest wid col row rows cols more-lookup-dict
  2drop drop
  cr
;

\ Display all the words as four columns with a pager
: more-words ( -- )
  ram-here
  [:
    ansi-term::get-terminal-size { rows cols }
    cr
    0 0 { col row }
    internal::unique-order 2* over + swap ?do
      ram-latest i h@ col row rows cols
      more-words-dict -rot to row to col not if exit then
      flash-latest i h@ col row rows cols
      more-words-dict -rot to row to col not if exit then
    2 +loop
    cr
  ;] try
  swap ram-here! ?raise
;

\ Display all the words as four columns in a wordlist with a pager
: more-words-in ( wid -- )
  { wid }
  ansi-term::get-terminal-size { rows cols }
  cr
  0 0 { col row }
  ram-latest wid col row rows cols
  more-words-dict -rot to row to col not if exit then
  flash-latest wid col row rows cols more-words-dict
  2drop drop
  cr
;

reboot
