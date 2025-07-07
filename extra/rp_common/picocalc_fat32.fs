\ Copyright (c) 2025 Travis Bemann
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

continue-module fat32-tools

  \ Create words for on-board blocks FAT32
  defined? setup-blocks-fat32 [if]

    \ Get the blocks FAT32 filesystem
    : blocks-fs@ ( -- fs )
      setup-blocks-fat32::my-fs
    ;
    
    \ Set the filesystem to the blocks FAT32
    : blocks-fs: ( -- )
      blocks-fs@ current-fs!
    ;

  [then]

  \ Create words for PSRAM FAT32 if installed
  defined? psram-fat32 [if]

    \ Get the PSRAM FAT32 filesystem
    : psram-fs@ ( -- fs )
      psram-fat32::psram-fs@
    ;
  
    \ Set the filesystem to the PSRAM FAT32
    : psram-fs: ( -- )
      psram-fs@ current-fs!
    ;

  [then]

  \ Set up an SD FAT32 filesystem unless NO-SD-FS is defined
  defined? no-sd-fs not [if]

    oo import
    block-dev import
    simple-fat32 import

    continue-module fat32-tools-internal
      
      \ Our SD card SPI device
      0 constant sd-spi-device
      
      \ Our SD card RX pin
      16 constant sd-rx-pin
      
      \ Our SD card Chip Select pin
      17 constant sd-cs-pin
      
      \ Our SD card SCK pin
      18 constant sd-sck-pin

      \ Our SD card TX pin
      19 constant sd-tx-pin

    end-module
      
    \ Our SD card FAT32 filesystem
    <simple-fat32-fs> class-size buffer: sd-fs@

    \ Set the filesystem to the SD card FAT32
    : sd-fs: ( -- )
      sd-fs@ current-fs!
    ;

    continue-module fat32-tools-internal
      
      \ Initialize the SD card FAT32
      : init-sd-fs ( -- )
        sd-sck-pin sd-tx-pin sd-rx-pin sd-cs-pin sd-spi-device
        <simple-fat32-fs> sd-fs@ init-object
        true sd-fs@ write-through!
      ;
      initializer init-sd-fs

    end-module
    
  [then]

end-module

reboot
