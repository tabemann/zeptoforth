\ Copyright (c) 2023-2024 Travis Bemann
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

oo import
block-dev import
sd import
spi import
pin import
fat32 import
  
<sd> class-size buffer: my-sd
<mbr> class-size buffer: my-mbr
<partition> class-size buffer: my-partition
<fat32-fs> class-size buffer: my-fs

0 constant my-spi

: init-fs ( -- )
  my-spi 2 spi-pin
  my-spi 3 spi-pin
  my-spi 4 spi-pin
  5 output-pin
  6 2 ?do i pull-up-pin loop
  5 my-spi <sd> my-sd init-object
  my-sd init-sd
  false my-sd write-through!
  my-sd <mbr> my-mbr init-object
  <partition> my-partition init-object
  my-partition 0 my-mbr partition@
  my-partition partition-first-sector @ .
  my-partition my-sd <fat32-fs> my-fs init-object
;

init-fs
my-fs fat32-tools::current-fs!
  
#include extra/common/dyn_buffer.fs
#include extra/common/file_edit.fs