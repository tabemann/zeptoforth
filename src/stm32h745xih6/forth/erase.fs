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
    $8200000
    $8020000 $803FFFF check-flash-sector
    $8040000 $805FFFF check-flash-sector
    $8060000 $807FFFF check-flash-sector
    $8080000 $809FFFF check-flash-sector
    $80A0000 $80BFFFF check-flash-sector
    $80C0000 $80DFFFF check-flash-sector
    $80E0000 $80FFFFF check-flash-sector
    $8100000 $811FFFF check-flash-sector
    $8120000 $813FFFF check-flash-sector
    $8140000 $815FFFF check-flash-sector
    $8160000 $817FFFF check-flash-sector
    $8180000 $819FFFF check-flash-sector
    $81A0000 $81BFFFF check-flash-sector
    $81C0000 $81DFFFF check-flash-sector
    $81E0000 $81FFFFF check-flash-sector
  ;

  \ Test for next flash sector
  : check-flash-sector-for-addr ( flash-addr addr start end -- addr )
    dup >r 3 pick >= swap 3 roll <= and if drop r> 1+ else rdrop then
  ;

  \ Align an address for a following flash sector
  : erase-align ( addr -- addr )
    >r $8200000
    r@ swap $8020000 $803FFFF check-flash-sector-for-addr
    r@ swap $8040000 $805FFFF check-flash-sector-for-addr
    r@ swap $8060000 $807FFFF check-flash-sector-for-addr
    r@ swap $8080000 $809FFFF check-flash-sector-for-addr
    r@ swap $80A0000 $80BFFFF check-flash-sector-for-addr
    r@ swap $80C0000 $80DFFFF check-flash-sector-for-addr
    r@ swap $80E0000 $80FFFFF check-flash-sector-for-addr
    r@ swap $8100000 $811FFFF check-flash-sector-for-addr
    r@ swap $8120000 $813FFFF check-flash-sector-for-addr
    r@ swap $8140000 $815FFFF check-flash-sector-for-addr
    r@ swap $8160000 $817FFFF check-flash-sector-for-addr
    r@ swap $8180000 $819FFFF check-flash-sector-for-addr
    r@ swap $81A0000 $81BFFFF check-flash-sector-for-addr
    r@ swap $81C0000 $81DFFFF check-flash-sector-for-addr
    r> swap $81E0000 $81FFFFF check-flash-sector-for-addr
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

continue-module internal
  
  \ Core of CORNERSTONE's DOES>
  : cornerstone-does> ( -- )
    does>
    erase-align
    erase-after
  ;
  
end-module

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
