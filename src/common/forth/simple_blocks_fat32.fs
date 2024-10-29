\ Copyright (c) 2022-2024 Travis Bemann
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

begin-module simple-blocks-fat32

  oo import
  block-dev import
  blk import
  fat32 import
  
  \ Simple FAT32 filesystem class definition
  <base-fat32-fs> begin-class <simple-blocks-fat32-fs>
  
    begin-module simple-blocks-fat32-internal
    
      \ The blocks device for the simple FAT32 filesystem
      <blocks> class-size member simple-blocks-fat32-dev
      
      \ The FAT32 filesystem for the simple FAT32 filesystem
      <fat32-fs> class-size member simple-blocks-fat32-fs
          
    end-module> import
  
    \ Set write-through cache mode
    method write-through! ( write-through fs -- )
    
    \ Get write-through cache mode
    method write-through@ ( fs -- write-through )
    
  end-class
  
  \ Simple blocks FAT32 filesystem class implementation
  <simple-blocks-fat32-fs> begin-implement
  
    :noname { self -- }
      self <base-fat32-fs>->new
      <blocks> self simple-blocks-fat32-dev init-object
      self self simple-blocks-fat32-dev <mbr> [:
        <partition> [: { self mbr partition }
          partition 0 mbr partition@
          partition self simple-blocks-fat32-dev
          <fat32-fs> self simple-blocks-fat32-fs init-object
        ;] with-object
      ;] with-object
    ; define new
    
    :noname ( dir fs -- ) simple-blocks-fat32-fs root-dir@ ; define root-dir@

    :noname ( dir fs -- )
      simple-blocks-fat32-fs current-dir@
    ; define current-dir@
    
    :noname ( c-addr u xt fs -- ) ( xt: c-addr' u' dir )
      simple-blocks-fat32-fs with-root-path
    ; define with-root-path

    :noname ( c-addr u fs -- exists? )
      simple-blocks-fat32-fs root-path-exists?
    ; define root-path-exists?
    
    :noname ( c-addr u xt fs -- ) ( xt: file -- )
      simple-blocks-fat32-fs with-create-file-at-root-path
    ; define with-create-file-at-root-path

    :noname ( c-addr u xt fs -- ) ( xt: file -- )
      simple-blocks-fat32-fs with-open-file-at-root-path
    ; define with-open-file-at-root-path

    :noname ( c-addr u xt fs -- ) ( xt: dir -- )
      simple-blocks-fat32-fs with-create-dir-at-root-path
    ; define with-create-dir-at-root-path

    :noname ( c-addr u xt fs -- ) ( xt: dir -- )
      simple-blocks-fat32-fs with-open-dir-at-root-path
    ; define with-open-dir-at-root-path
    
    :noname ( write-through fs -- )
      simple-blocks-fat32-dev block-dev::write-through!
    ; define write-through! 

    :noname ( fs -- write-through )
      simple-blocks-fat32-dev block-dev::write-through@
    ; define write-through@
    
    :noname ( fs -- ) simple-blocks-fat32-fs flush ; define flush

    :noname ( fs -- fs' ) simple-blocks-fat32-fs ; define real-fs@
    
  end-implement

end-module

reboot
