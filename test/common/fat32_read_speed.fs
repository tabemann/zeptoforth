\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module fat32-read-speed

  oo import
  fat32 import
  systick import
  
  \ File to read
  <fat32-file> class-size buffer: my-file

  \ My size to read
  512 constant my-read-size

  \ My total size to read
  65536 constant my-total-size

  \ My buffer to read to
  my-read-size buffer: my-buffer
  
  \ Test the FAT32 read speed
  : run-speed-test ( c-addr u -- )
    fat32-tools::current-fs@ averts fat32-tools::x-fs-not-set
    my-file -rot [: 3 pick swap open-file ;]
    fat32-tools::current-fs@ with-root-path drop
    0 systick-counter { bytes-read last-systick }
    begin key? not while
      my-buffer my-read-size my-file read-file { cur-bytes-read }
      cur-bytes-read 0> if
        cur-bytes-read +to bytes-read
        bytes-read my-total-size >= if
          my-total-size negate +to bytes-read
          systick-counter { current-systick }
          0 my-total-size 1024,0 f/ 
          0 current-systick last-systick - 10000,0 f/
          f/ cr ." KB per second: " f.
          current-systick to last-systick
        then
      else
        0 seek-set my-file seek-file
      then
    repeat
    key drop
  ;
  
end-module