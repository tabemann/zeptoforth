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

begin-module init-fat32-tool

  oo import
  fat32 import
  block-dev import

  begin-module init-fat32-tool-internal
    
    \ Sector size
    fat32-internal::sector-size constant sector-size
    
    \ Do an alignment-safe 32-bit store
    : unaligned! { x addr -- }
      x addr c!
      x 8 rshift addr 1+ c!
      x 16 rshift addr 2 + c!
      x 24 rshift addr 3 + c!
    ;

    \ Do an alignment-safe 16-bit store
    : unaligned-h! { h addr -- }
      h addr c!
      h 8 rshift addr 1+ c!
    ;
    
    \ Populate a partition entry
    : init-partition { media partition -- }
      active-partition partition partition-active !
      fat32-lba-partition-type partition partition-type !
      1 partition partition-first-sector !
      media block-count 1- partition partition-sectors !
    ;

    \ Get the number of sectors for data for media
    \ x + 2((x / cluster-sectors) / 128) = total
    \ x + ((x / cluster-sectors) / 64) = total
    \ 64x + (x / cluster-sectors ) = 64total
    \ (64 * cluster-sectors)x + x = (64 * cluster-sectors)total
    \ ((64 * cluster-sectors) + 1)x = (64 * cluster-sectors)total
    \ x = (64 * cluster-sectors)total / ((64 * cluster-sectors) + 1)
    : data-sectors { cluster-sectors media -- sectors }
      media block-count 3 - \ Exclude MBR, first partition, and info sectors
      64 cluster-sectors * *
      64 cluster-sectors * 1+ /
    ;

    \ Get the number of sectors for FAT for media
    : fat-sectors { cluster-sectors media -- sectors }
      cluster-sectors media data-sectors cluster-sectors / 128 align 128 /
    ;

    \ Populate a FAT32 filesystem's VBR
    : init-vbr { cluster-sectors scratchpad media -- }
      scratchpad sector-size 0 fill
      512 scratchpad $00B + unaligned-h! \ Sector size
      cluster-sectors scratchpad $00D + c! \ Cluster sector count
      2 scratchpad $00E + h! \ Reserved sector count
      2 scratchpad $010 + c! \ FAT count
      $F8 scratchpad $015 + c! \ Media descriptor
      media block-count 1- scratchpad $020 + ! \ Sector count
      cluster-sectors media fat-sectors scratchpad $024 + ! \ FAT sectors
      0 scratchpad $02A + h! \ Filesystem version
      2 scratchpad $02C + ! \ Root directory cluster
      1 scratchpad $030 + h! \ Info sector
      $28 scratchpad $042 + c! \ Extended boot signature
      scratchpad sector-size 1 media block!
    ;

    \ Populate a FAT32 filesystem's info sector
    : init-info { cluster-sectors scratchpad media -- }
      scratchpad sector-size 0 fill
      $41615252 scratchpad $000 + ! \ Magic
      $61417272 scratchpad $1E4 + ! \ magic
      $AA550000 scratchpad $1FC + ! \ Magic
      cluster-sectors media data-sectors cluster-sectors / scratchpad $1E8 + !
      \ Free clusters
      -1 scratchpad $1EC + ! \ Recent allocated cluster, initialized to -1
      scratchpad sector-size 2 media block!
    ;

    \ Initialize a FAT
    : init-fat { cluster-sectors first-sector scratchpad media -- }
      scratchpad sector-size 0 fill
      $0FFFFFF8 scratchpad $008 + ! \ First root directory cluster
      scratchpad sector-size first-sector media block!
      0 scratchpad $008 + !
      cluster-sectors media fat-sectors { fat-sectors }
      fat-sectors 1 > if
        fat-sectors first-sector + first-sector 1+ ?do
          scratchpad sector-size i media block!
        loop
      then
    ;

    \ Initialize the root directroy
    : init-root-dir { cluster-sectors scratchpad media -- }
      scratchpad sector-size 0 fill
      cluster-sectors media fat-sectors 2 * 3 + dup cluster-sectors + swap ?do
        scratchpad sector-size i media block!
      loop
    ;
    
    \ Populate a FAT32 filesystem
    : init-fat32 { cluster-sectors scratchpad media -- }
      cluster-sectors scratchpad media init-vbr
      cluster-sectors scratchpad media init-info
      cluster-sectors 3 scratchpad media init-fat
      cluster-sectors dup media fat-sectors 3 + scratchpad media init-fat
      cluster-sectors scratchpad media init-root-dir
    ;

  end-module> import
  
  \ Initialize a FAT32 filesystem in a single partition on a medium
  : init-partition-and-fat32 ( cluster-sectors media -- )
    dup <mbr> [:
      <partition> [:
        sector-size [: { cluster-sectors media mbr partition scratchpad -- }
          media partition init-partition
          mbr format-mbr
          partition 0 mbr partition!
          cluster-sectors scratchpad media init-fat32
        ;] with-aligned-allot
      ;] with-object
    ;] with-object
  ;
  
end-module
