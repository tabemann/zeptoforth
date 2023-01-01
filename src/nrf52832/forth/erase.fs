\ Copyright (c) 2020-2023 Travis Bemann
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

\ Compile this to flash
compile-to-flash

\ Set up the wordlist order
forth internal 2 set-order
internal set-current

\ Begin compressing compiled code in flash
compress-flash

\ Pad flash to a 1024 byte boundary
: pad-flash-erase-block ( -- )
  begin flash-here $3FF and while
    0 cflash,
  repeat
  drop
;

\ Restore flash to a preexisting state
: restore-flash ( flash-here -- )
  erase-after rdrop
;

\ Set forth
forth set-current

\ Commit flash
commit-flash

\ Create a MARKER to erase flash/return the flash dictionary to its prior state
: marker ( "name" -- )
  compiling-to-flash?
  token
  dup 0= if ['] x-token-expected ?raise then
  compile-to-flash
  pad-flash-erase-block
  flash-here
  rot rot
  start-compile
  lit,
  ['] restore-flash compile,
  visible
  end-compile,
  not if
    compile-to-ram
  then
;

\ Set internal
internal set-current

\ Core of CORNERSTONE's DOES>
: cornerstone-does> ( -- )
  does>
  $400 align
  erase-after
;

\ Set forth
forth set-current

\ Committing code in flash
commit-flash

\ Adapted from Terry Porter's code; not sure what license it was under
: cornerstone ( "name" -- )
  compiling-to-flash?
  compile-to-flash
  <builds
  pad-flash-erase-block
  cornerstone-does>
  not if
    compile-to-ram
  then
;

\ Ending compiling code in flash
end-compress-flash

\ Reboot
reboot
