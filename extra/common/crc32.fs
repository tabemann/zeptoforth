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

begin-module crc32

  begin-module crc32-internal

    \ Consruct a single entry for the CRC table
    : make-crc-entry ( n -- n )
      8 0 do dup 1 rshift swap 1 and if $EDB88320 xor then loop
    ;

    \ Initialize the CRC table
    : fill-crc-entries ( -- )
      256 0 do i make-crc-entry , loop
    ;

    \ The CRC table
    create crc-table fill-crc-entries

    \ One step of calculating the CRC
    : crc-step ( crc n -- crc' )
      over xor $ff and cells crc-table + @  swap 8 rshift xor
    ;
    
    \ Calculate a CRC with a buffer
    : crc-buf ( crc c-addr u -- crc )
      over swap + swap ?do i c@ crc-step loop
    ;
    
  end-module> import
    
  \ Actually calculate a CRC with a buffer
  : generate-crc32 ( c-addr u -- crc )
    $FFFFFFFF -rot crc-buf $FFFFFFFF xor
  ;
  
end-module
