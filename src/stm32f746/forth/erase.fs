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

continue-module internal

  \ Begin compressing compiled code in flash
  compress-flash

  \ Test for next flash sector
  : check-flash-sector ( addr start end -- addr )
    dup >r flash-here >= swap flash-here <= and if drop r> 1+ else rdrop then
  ;

  \ Get the start of the next flash sector
  : next-flash-sector ( -- addr )
    $300000
    $208000 $20FFFF check-flash-sector
    $210000 $217FFF check-flash-sector
    $218000 $21FFFF check-flash-sector
    $220000 $23FFFF check-flash-sector
    $240000 $27FFFF check-flash-sector
    $280000 $2BFFFF check-flash-sector
    $2C0000 $2FFFFF check-flash-sector
  ;

  \ Test for next flash sector
  : check-flash-sector-for-addr ( flash-addr addr start end -- addr )
    dup >r 3 pick >= swap 3 roll <= and if drop r> 1+ else rdrop then
  ;

  \ Align an address for a following flash sector
  : erase-align ( addr -- addr )
    >r $300000
    r@ swap $208000 $20FFFF check-flash-sector-for-addr
    r@ swap $210000 $217FFF check-flash-sector-for-addr
    r@ swap $218000 $21FFFF check-flash-sector-for-addr
    r@ swap $220000 $23FFFF check-flash-sector-for-addr
    r@ swap $240000 $27FFFF check-flash-sector-for-addr
    r@ swap $280000 $2BFFFF check-flash-sector-for-addr
    r> swap $2C0000 $2FFFFF check-flash-sector-for-addr
  ;

  \ Pad flash to a sector boundary
  : pad-flash-erase-block ( -- )
    next-flash-sector
    begin flash-here over < while
      0 cflash,
    repeat
    drop
  ;

  \ Restore flash to a preexisting state
  : restore-flash ( flash-here -- )
    erase-after rdrop
  ;

end-module> import

\ Commit flash
commit-flash

\ Create a MARKER to return the dictionary to its prior state
: marker ( "name" -- )
  token
  dup 0= if ['] x-token-expected ?raise then
  compiling-to-flash? if
    pad-flash-erase-block
    flash-here
    rot rot
    start-compile
    lit,
    ['] restore-flash compile,
    visible
    end-compile,
  else
    ram-latest
    ram-here
    2swap
    start-compile
    lit, postpone ram-here!
    lit, postpone ram-latest!
    visible
    end-compile,
  then
;

continue-module internal
  
  \ Core of CORNERSTONE's DOES>
  : cornerstone-does> ( -- )
    does>
    erase-align
    erase-after
  ;

  \ Core of CORNERSTONE's DOES> for RAM
  : ram-cornerstone-does> ( -- )
    does> dup @ ram-latest! cell+ @ ram-here!
    latest ram-base >= latest ram-end < and if ram-latest latest! then
  ;

end-module

\ Committing code in flash
commit-flash

\ Adapted from Terry Porter's code; not sure what license it was under
: cornerstone ( "name" -- )
  compiling-to-flash? if
    <builds
    pad-flash-erase-block
    cornerstone-does>
  else
    <builds reserve reserve ram-cornerstone-does>
    here swap current! ram-latest swap current!
  then
;

\ Forget up to and including a word in RAM
: forget ( "name" -- )
  token-word dup ram-base >= over ram-end < and if
    dup internal::next-word @ ram-latest! ram-here!
    latest ram-base >= latest ram-end < and if ram-latest latest! then
  else
    drop
  then
;

\ Ending compiling code in flash
end-compress-flash

\ Reboot
reboot
