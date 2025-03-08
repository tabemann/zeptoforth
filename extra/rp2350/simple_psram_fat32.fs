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

compile-to-flash

#include extra/common/init_fat32.fs

defined? simple-psram-fat32 not [if]

  begin-module simple-psram-fat32

    oo import
    block-dev import
    psram-blocks import
    fat32 import
    
    \ Simple PSRAM FAT32 filesystem class definition
    <base-fat32-fs> begin-class <simple-psram-fat32-fs>
      
      begin-module simple-psram-fat32-internal
        
        \ The PSRAM blocks device for the simple FAT32 filesystem
        <psram-blocks> class-size member simple-psram-fat32-dev
        
        \ The FAT32 filesystem for the simple FAT32 filesystem
        <fat32-fs> class-size member simple-psram-fat32-fs
        
      end-module> import
      
      \ Set write-through cache mode
      method write-through! ( write-through fs -- )
      
      \ Get write-through cache mode
      method write-through@ ( fs -- write-through )
      
    end-class
    
    \ Simple blocks FAT32 filesystem class implementation
    <simple-psram-fat32-fs> begin-implement
      
      :noname { psram-cs-pin self -- }
        self <base-fat32-fs>->new
        psram-cs-pin init-psram
        <psram-blocks> self simple-psram-fat32-dev init-object
        true self simple-psram-fat32-dev write-psram-block-zero!
        4 self simple-psram-fat32-dev init-fat32-tool::init-partition-and-fat32
        self self simple-psram-fat32-dev <mbr> [:
          <partition> [: { self mbr partition }
            partition 0 mbr partition@
            partition self simple-psram-fat32-dev
            <fat32-fs> self simple-psram-fat32-fs init-object
          ;] with-object
        ;] with-object
      ; define new
      
      :noname ( dir fs -- ) simple-psram-fat32-fs root-dir@ ; define root-dir@

      :noname ( dir fs -- )
        simple-psram-fat32-fs current-dir@
      ; define current-dir@
      
      :noname ( c-addr u xt fs -- ) ( xt: c-addr' u' dir )
        simple-psram-fat32-fs with-root-path
      ; define with-root-path

      :noname ( c-addr u fs -- exists? )
        simple-psram-fat32-fs root-path-exists?
      ; define root-path-exists?
      
      :noname ( c-addr u xt fs -- ) ( xt: file -- )
        simple-psram-fat32-fs with-create-file-at-root-path
      ; define with-create-file-at-root-path

      :noname ( c-addr u xt fs -- ) ( xt: file -- )
        simple-psram-fat32-fs with-open-file-at-root-path
      ; define with-open-file-at-root-path

      :noname ( c-addr u xt fs -- ) ( xt: dir -- )
        simple-psram-fat32-fs with-create-dir-at-root-path
      ; define with-create-dir-at-root-path

      :noname ( c-addr u xt fs -- ) ( xt: dir -- )
        simple-psram-fat32-fs with-open-dir-at-root-path
      ; define with-open-dir-at-root-path
      
      :noname ( write-through fs -- )
        simple-psram-fat32-dev block-dev::write-through!
      ; define write-through! 

      :noname ( fs -- write-through )
        simple-psram-fat32-dev block-dev::write-through@
      ; define write-through@
      
      :noname ( fs -- ) simple-psram-fat32-fs flush ; define flush

      :noname ( fs -- fs' ) simple-psram-fat32-fs ; define real-fs@
      
    end-implement

  end-module

[then]

compile-to-ram
