\ Copyright (c) 2026 Travis Bemann
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

begin-module gunzip
  
  puff import
  crc32 import

  \ gzip header identification bytes
  $1F constant id1
  $8B constant id2
  8 constant cm-deflate

  \ gzip flag bits
  1 bit constant fhcrc
  2 bit constant fextra
  3 bit constant fname
  4 bit constant fcomment
  
  \ Invalid gzip header
  : x-invalid-gzip-header ( -- ) ." invalid gzip header" cr ;

  \ Invalid gzip footer
  : x-invalid-gzip-footer ( -- ) ." invalid gzip footer" cr ;

  \ Incorrect uncompressed data length
  : x-incorrect-uncompressed-len ( -- ) ." incorrect uncompressed length" cr ;
  
  \ Invalid DEFLATE data
  : x-invalid-deflate-data ( -- ) ." invalid DEFLATE data" cr ;

  \ Insufficient room for uncompressed data
  : x-uncompressed-data-too-big ( -- ) ." uncompressed data is too big" cr ;

  \ Uncompressed data CRC32 does not match
  : x-crc32-does-not-match ( -- )
    ." uncompressed data CRC32 does not match" cr
  ;
  
  \ Validate a zero-terminated string
  : validate-gzip-zero-term { addr bytes -- addr' bytes' }
    begin
      bytes 0<> averts x-invalid-gzip-header
      addr c@ 0=
      1 +to addr -1 +to bytes
    until
    addr bytes
  ;

  \ Parse an unaligned little-endian 16-bit value
  : hunaligned@ { addr -- h }
    addr c@ addr 1+ c@ 8 lshift or
  ;

  \ Parse an unaligned little-endian 32-bit value
  : unaligned@ { addr -- x }
    addr c@
    addr 1+ c@ 8 lshift or
    addr 2 + c@ 16 lshift or
    addr 3 + c@ 24 lshift or
  ;
  
  \ Parse gzip file header(s) and get the total uncompressed file length
  : parse-gzip-headers { addr bytes -- uncompressed-len }
    0 { total-uncompressed-len }
    begin
      addr bytes { start-addr start-bytes }
      bytes 0= if total-uncompressed-len exit then
      addr c@ id1 = averts x-invalid-gzip-header
      1 +to addr -1 +to bytes
      bytes 0<> averts x-invalid-gzip-header
      addr c@ id2 = averts x-invalid-gzip-header
      1 +to addr -1 +to bytes
      bytes 0<> averts x-invalid-gzip-header
      addr c@ cm-deflate = averts x-invalid-gzip-header
      1 +to addr -1 +to bytes
      bytes 0<> averts x-invalid-gzip-header
      addr c@ { flags }
      1 +to addr -1 +to bytes
      bytes 5 u> averts x-invalid-gzip-header
      6 +to addr -6 +to bytes
      flags fextra and if
        bytes 1 u> averts x-invalid-gzip-header
        addr hunaligned@ { xlen }
        bytes xlen 1+ u> averts x-invalid-gzip-header
        xlen 2 + +to addr xlen 2 + negate +to bytes
      then
      flags fname and if
        addr bytes validate-gzip-zero-term to bytes to addr
      then
      flags fcomment and if
        addr bytes validate-gzip-zero-term to bytes to addr
      then
      flags fhcrc and if
        start-addr start-bytes bytes - generate-crc32 { hash }
        bytes 1 u> averts x-invalid-gzip-header
        addr hunaligned@ hash $FFFF and averts x-invalid-gzip-header
        2 +to addr -2 +to bytes
      then
      addr bytes 0 0 puff
      0= averts x-invalid-deflate-data { source-len dest-len }
      bytes source-len u> averts x-invalid-deflate-data
      source-len +to addr source-len negate +to bytes
      bytes 7 u> averts x-invalid-gzip-footer
      4 +to addr -4 +to bytes
      addr unaligned@ dest-len = averts x-incorrect-uncompressed-len
      4 +to addr -4 +to bytes
      dest-len +to total-uncompressed-len
    again
  ;

  \ Decompress a gzip file
  : decompress-gzip
    { src-addr src-bytes dest-addr dest-bytes -- total-dest-bytes }
    src-addr src-bytes parse-gzip-headers { total-dest-bytes }
    total-dest-bytes dest-bytes <= averts x-uncompressed-data-too-big
    begin
      src-bytes 0= if total-dest-bytes exit then
      3 +to src-addr -3 +to src-bytes
      src-addr c@ { flags }
      7 +to src-addr -7 +to src-bytes
      flags fextra and if
        src-addr hunaligned@ { xlen }
        xlen 2 + +to src-addr xlen 2 + negate +to src-bytes
      then
      flags fname and if
        src-addr src-bytes validate-gzip-zero-term to src-bytes to src-addr
      then
      flags fcomment and if
        src-addr src-bytes validate-gzip-zero-term to src-bytes to src-addr
      then
      flags fhcrc and if
        2 +to src-addr -2 +to src-bytes
      then
      src-addr src-bytes dest-addr dest-bytes puff
      0= averts x-invalid-deflate-data { source-len dest-len }
      src-bytes source-len u> averts x-invalid-deflate-data
      source-len +to src-addr source-len negate +to src-bytes
      src-addr unaligned@
      dest-addr dest-len generate-crc32 = averts x-crc32-does-not-match
      4 +to src-addr -4 +to src-bytes
      src-addr unaligned@ dest-len = averts x-incorrect-uncompressed-len
      4 +to src-addr -4 +to src-bytes
      dest-len +to dest-addr dest-len negate +to dest-bytes
    again
  ;

  \ Validate a gzip file
  : validate-gzip
    { src-addr src-bytes max-bytes -- total-dest-bytes }
    src-addr src-bytes parse-gzip-headers { total-dest-bytes }
    total-dest-bytes max-bytes <= averts x-uncompressed-data-too-big
    src-addr src-bytes max-bytes dup [:
      { src-addr src-bytes dest-bytes dest-addr }
      begin
        src-bytes 0= if exit then
        3 +to src-addr -3 +to src-bytes
        src-addr c@ { flags }
        7 +to src-addr -7 +to src-bytes
        flags fextra and if
          src-addr hunaligned@ { xlen }
          xlen 2 + +to src-addr xlen 2 + negate +to src-bytes
        then
        flags fname and if
          src-addr src-bytes validate-gzip-zero-term to src-bytes to src-addr
        then
        flags fcomment and if
          src-addr src-bytes validate-gzip-zero-term to src-bytes to src-addr
        then
        flags fhcrc and if
          2 +to src-addr -2 +to src-bytes
        then
        
        src-addr src-bytes dest-addr dest-bytes puff dup .
        0= averts x-invalid-deflate-data { source-len dest-len }
        src-bytes source-len u> averts x-invalid-deflate-data
        source-len +to src-addr source-len negate +to src-bytes
        src-addr unaligned@
        dest-addr dest-len generate-crc32 = averts x-crc32-does-not-match
        4 +to src-addr -4 +to src-bytes
        src-addr unaligned@ dest-len = averts x-incorrect-uncompressed-len
        4 +to src-addr -4 +to src-bytes
        dest-len +to dest-addr dest-len negate +to dest-bytes
      again
    ;] with-aligned-allot
    total-dest-bytes
  ;

end-module
