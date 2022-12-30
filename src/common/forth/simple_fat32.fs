\ Copyright (c) 2022 Travis Bemann
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

begin-module simple-fat32

  oo import
  spi import
  pin import
  block-dev import
  sd import
  fat32 import
  
  \ Simple FAT32 filesystem class definition
  <base-fat32-fs> begin-class <simple-fat32-fs>
  
    begin-module simple-fat32-internal
    
      \ The SD card device for the simple FAT32 filesystem
      <sd> class-size member simple-fat32-sd
      
      \ The FAT32 filesystem for the simple FAT32 filesystem
      <fat32-fs> class-size member simple-fat32-fs
          
    end-module> import
  
    \ Set write-through cache mode
    method write-through! ( write-through fs -- )
    
    \ Get write-through cache mode
    method write-through@ ( fs -- write-through )
    
  end-class
  
  \ Simple FAT32 filesystem class implementation
  <simple-fat32-fs> begin-implement
  
    :noname { sck-pin tx-pin rx-pin cs-pin spi-device self -- }
      self <base-fat32-fs>->new
      rx-pin pull-up-pin spi-device rx-pin spi-pin
      tx-pin pull-up-pin spi-device tx-pin spi-pin
      sck-pin pull-up-pin spi-device sck-pin spi-pin
      cs-pin output-pin cs-pin pull-up-pin
      cs-pin spi-device <sd> self simple-fat32-sd init-object
      self simple-fat32-sd init-sd
      self self simple-fat32-sd <mbr> [:
        <partition> [: { self mbr partition }
          partition 0 mbr partition@
          partition self simple-fat32-sd
          <fat32-fs> self simple-fat32-fs init-object
        ;] with-object
      ;] with-object
    ; define new
    
    :noname ( dir fs -- ) simple-fat32-fs root-dir@ ; define root-dir@

    :noname ( c-addr u xt fs -- ) ( xt: c-addr' u' dir )
      simple-fat32-fs with-root-path
    ; define with-root-path

    :noname ( c-addr u fs -- exists? )
      simple-fat32-fs root-path-exists?
    ; define root-path-exists?
    
    :noname ( write-through fs -- )
      simple-fat32-sd block-dev::write-through!
    ; define write-through! 

    :noname ( fs -- write-through )
      simple-fat32-sd block-dev::write-through@
    ; define write-through@
    
    :noname ( fs -- ) simple-fat32-fs flush ; define flush
    
  end-implement

end-module

reboot
