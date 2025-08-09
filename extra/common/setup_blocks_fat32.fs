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

compile-to-ram

#include extra/common/init_fat32.fs

begin-module prepare-blocks-fat32

  oo import
  fat32 import
  block-dev import
  blk import
  
  \ Prepare the FAT32 filesystem
  : prepare ( -- )
    <blocks> [:
      dup <mbr> [: { dev mbr }
        mbr mbr-valid? not if
          block::erase-all-blocks
          true dev write-blk-block-zero!
          true dev write-through!
          4 dev init-fat32-tool::init-partition-and-fat32
        then
      ;] with-object
    ;] with-object
  ;
  
end-module> import

\ BE AWARE THAT THIS STEP CAN TAKE A LENGTHY AMOUNT OF TIME, ESPECIALLY WITH
\ THE rp2350_16mib PLATFORM (E.G. WITH THE PIMORONI PICO PLUS 2)
prepare

compile-to-flash

\ Compile code to set up blocks FAT32 on boot
defined? setup-blocks-fat32 not [if]

  begin-module setup-blocks-fat32

    oo import
    simple-blocks-fat32 import
    
    \ The blocks FAT32 filesystem
    <simple-blocks-fat32-fs> class-size aligned-buffer: my-fs
    
    \ Setup the blocks FAT32 filesystem
    : setup ( -- )
      <simple-blocks-fat32-fs> my-fs init-object
      true my-fs write-through!
      my-fs fat32-tools::current-fs!
    ;
    
  end-module> import

  initializer setup

[then]

reboot
