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

#include extra/rp2350/simple_psram_fat32.fs

compile-to-flash

\ Compile code to set up PSRAM FAT32 on boot
defined? psram-fat32 not [if]

  begin-module psram-fat32

    oo import
    simple-psram-fat32 import
    
    \ The PSRAM FAT32 filesystem
    <simple-psram-fat32-fs> class-size aligned-buffer: psram-fs@
    
    \ Setup the PSRAM FAT32 filesystem
    : setup ( -- )
      47 <simple-psram-fat32-fs> psram-fs@ init-object
    ;
    
  end-module> import

  initializer setup

[then]

reboot
