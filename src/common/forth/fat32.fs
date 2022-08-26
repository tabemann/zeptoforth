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

begin-module fat32

  oo import
  file import
  lock import
  block-dev import
  
  \ The FAT32 lock
  lock-size buffer: fat32-lock
  
  \ The only supported sector size
  512 constant sector-size
  
  \ The only supported directory entry size
  32 constant entry-size
  
  \ Sector size exception
  : x-sector-size-not-supported ." sector sizes other than 512 are not supported" cr  ;
  
  \ Filesystem version not supported exception
  : x-fs-version-not-supported ." FAT32 version not supported" cr ;
  
  \ Bad info sector exception
  : x-bad-info-sector ." bad info sector" cr ;
  
  \ No clusters free exception
  : x-no-clusters-free ." no clusters free" cr ;
  
  \ Unsupported file name format exception
  : x-file-name-format ( -- ) ." unsupported filename" cr ;
  
  \ Out of range directory entry index
  : x-out-of-range-entry ( -- ) ." out of range directory entry" cr ;
  
  \ Directory entry not found
  : x-entry-not-found ( -- ) ." directory entry not found" cr ;
  
  \ Sector scratchpad buffer
  sector-size buffer: sector-scratchpad
  
  \ Unaligned halfword access
  : unaligned-h@ ( addr -- h ) dup c@ swap 1+ c@ 8 lshift or ;
  
  \ FAT32 filesystem class
  <fs> begin-class <fat32-fs>
    \ The FAT32 device to which this filesystem belongs
    cell member fat32-device
    
    \ The first sector of the filesystem
    cell member first-sector
    
    \ The number of sectors per cluster
    cell member cluster-sectors
    
    \ The number of reserved sectors before the FAT's
    cell member reserved-sectors
    
    \ The number of FAT's
    cell member fat-count
    
    \ The total sector count
    cell member sector-count
    
    \ The number of sectors per FAT
    cell member fat-sectors
    
    \ The cluster to which the root directory belongs (usually 2)
    cell member root-dir-cluster
    
    \ The FAT32 filesystem info sector (usually 1)
    cell member info-sector
    
    \ The free cluster count (don't trust this value)
    cell member free-cluster-count
    
    \ The most recently allocated cluster, -1 for all sectors free
    \ (Don't trust this value)
    cell member recent-allocated-cluster
    
    \ Read the FAT32 filesystem info sector
    method read-info-sector ( fs -- )
    
    \ Write the FAT32 filesystem info sector
    method write-info-sector ( fs -- )
    
    \ Read a cluster link value from a FAT
    method fat@ ( cluster fat fs -- link )
    
    \ Write a cluster link value to a FAT
    method fat! ( link cluster fat fs -- )
    
    \ Write a cluster link value to all FAT's
    method all-fat! ( link cluster fs -- )
    
    \ Get the sector of a cluster in a FAT
    method cluster>fat-sector ( cluster fat fs -- sector )
    
    \ Get the starting sector of a cluster
    method cluster>sector ( cluster fs -- sector )
    
    \ Get a sector containing an offset within a cluster
    method cluster-offset>sector ( offset cluster fs -- sector )
    
    \ Get the cluster of a sector
    method sector>cluster ( sector fs -- cluster )
    
    \ Get the number of clusters
    method cluster-count@ ( fs -- count )
    
    \ Find a free cluster
    method find-free-cluster ( fs -- cluster )
    
    \ Allocate a free cluster
    method allocate-cluster ( fs -- cluster )
    
    \ Allocate a free cluster and link it to an preceding cluster
    method allocate-link-cluster ( cluster fs -- cluster' )
    
    \ Free a cluster
    method free-cluster ( cluster fs -- )
    
    \ Free a cluster chain (for freeing a file/directory)
    method free-cluster-chain ( cluster fs -- )
    
    \ Free all the clusters in a cluster chain except the first (for truncating a file)
    method free-cluster-tail ( cluster fs -- )
    
    \ Find the entry starting from a given cluster and index within the cluster
    method find-entry ( index cluster fs -- index cluster | -1 -1 )
    
    \ Read a directory entry at a cluster and index within the cluster
    method entry@ ( entry index cluster fs -- )
    
    \ Write a directory entry at a cluster and index within the cluster
    method entry! ( entry index cluster fs -- )
    
    \ Get the entry per directory cluster size
    method dir-cluster-entry-count@ ( fs -- count )
    
    \ Look up a directory entry by name
    method lookup-entry ( c-addr u cluster fs -- index cluster )
    
    \ Allocate a directory entry
    method allocate-entry ( cluster fs -- index cluster )
    
    \ Delete a directory entry
    method delete-entry ( index cluster fs -- )
    
    \ Expand a directory by one entry
    method expand-dir ( index cluster fs -- )
  end-class
  
  \ FAT32 file class
  <object> begin-class <fat32-file>
    \ Filesystem
    cell member file-fs
    
    \ Parent directory index
    cell member file-parent-index
    
    \ Parent directory cluster
    cell member file-parent-cluster
    
    \ Starting cluster
    cell member file-start-cluster
    
    \ Current offset in a file
    cell member file-offset
    
    \ Current cluster in a file
    cell member file-current-cluster
    
    \ Current cluster index in a file
    cell member file-current-cluster-index
    
    \ Create a file
    method create-file ( c-addr u parent-dir file -- )
    
    \ Read data from a file
    method read-file ( c-addr u file -- bytes )
    
    \ Write data to a file
    method write-file ( c-addr u file -- bytes )
    
    \ Truncate a file
    method truncate-file ( file -- )
    
    \ Read up to a sector in a file
    method read-file-sector ( c-addr u file -- bytes )
    
    \ Write up to a sector in a file
    method write-file-sector ( c-addr u file -- bytes )
    
    \ Get the size of a file
    method file-size@ ( file -- bytes )
    
    \ Get the current sector in a file
    method current-file-sector@ ( file -- sector )
    
    \ Advance the current cluster if at the end of the current cluster
    method advance-cluster ( file -- )
    
    \ Expand a file
    method expand-file ( file -- )
  end-class
  
  \ FAT32 directory class
  <object> end-class <fat32-dir>
    \ Filesystem
    cell member dir-fs
  
    \ Parent directory index
    cell member dir-parent-index
    
    \ Parent directory cluster
    cell member dir-parent-cluster
    
    \ Starting cluster
    cell member dir-start-cluster
    
    \ Create a directory with . and .. entries
    method create-dir ( c-addr u parent-dir dir -- )
    
    \ Create a directory without any . or .. entries
    method create-dir-raw ( c-addr u parent-dir dir -- )
    
    \ Create . directory entry
    method create-dot-entry ( dir -- )
    
    \ Create .. directory entry
    method create-dot-dot-entry ( parent-dir dir -- )
  end-class
  
  \ FAT32 directory entry class
  <object> begin-class <fat32-entry>
    \ The short file name component, padded with spaces
    \
    \ The first byte can have the special values:
    \ $00: final entry in the directory entry table
    \ $05: the initial byte is actually $35
    \ $2E: the dot entry
    \ $E5: the directory entry has been deleted
    8 member short-file-name
    
    \ The short file extension component, padded with spaces
    3 member short-file-ext
    
    \ The file attributes
    \
    \ There are the following bits:
    \ $01: read only
    \ $02: hidden
    \ $04: system (do not move in the filesystem)
    \ $08: volume label
    \ $10: subdirectory (subdirectories have a file size of zero)
    \ $20: archive
    \ $40: device
    \ $80: reserved
    1 member file-attributes
    
    \ Windows NT VFAT case information
    1 member nt-vfat-case
    
    \ Create time file resolution, 10 ms increments, from 0 to 199
    1 member create-time-fine
    
    \ Create time with coarse resolution, 2 s increments
    \
    \ bits 15-11: hours (0-23)
    \ bits 10-5: minutes (0-59)
    \ bits 4-0: seconds / 2 (0-29)
    2 member create-time-coarse
    
    \ Create date
    \
    \ bits 15-9: year (0 = 1980)
    \ bits 8-5: month (1-12)
    \ bits 4-0: day (1-31)
    2 member create-date
    
    \ Last access date
    \
    \ bits 15-9: year (0 = 1980)
    \ bits 8-5: month (1-12)
    \ bits 4-0: day (1-31)
    2 member access-date
    
    \ High two bytes of the first cluster number
    2 member first-cluster-high
    
    \ Last modify time with coarse resolution, 2 s increments
    \
    \ bits 15-11: hours (0-23)
    \ bits 10-5: minutes (0-59)
    \ bits 4-0: seconds / 2 (0-29)
    2 member modify-time-coarse
    
    \ Last modify date
    \
    \ bits 15-9: year (0 = 1980)
    \ bits 8-5: month (1-12)
    \ bits 4-0: day (1-31)
    2 member modify-date
    
    \ Low two bytes of the first cluster number
    2 member first-cluster-low
    
    \ The file size; is always 0 for subdirectories and volume labels
    4 member entry-file-size
    
    \ Import a buffer into a directory entry
    method buffer>entry ( addr entry -- )
    
    \ Export a directory entry as a buffer
    method entry>buffer ( addr entry -- )
    
    \ Initialize a file directory entry
    method init-file-entry ( file-size first-cluster c-addr u entry -- )
    
    \ Initialize a subdirectory directory entry
    method init-dir-entry ( first-cluster c-addr u entry -- )
    
    \ Initialize an end directory entry
    method init-end-entry ( entry -- )
    
    \ Mark a directory entry as deleted
    method mark-entry-deleted ( entry -- )
    
    \ Get whether a directory entry has been deleted
    method entry-deleted? ( entry -- deleted? )
    
    \ Get whether a directory entry is the last in a directory
    method entry-end? ( entry -- end? )
    
    \ Get whether a directory entry is for a file
    method entry-file? ( entry -- file? )
    
    \ Get whether a directory entry is for a subdirectory
    method entry-dir? ( entry -- subdir? )
    
    \ Get the first cluster index of a directory entry
    method first-cluster@ ( entry -- cluster )
    
    \ Set the file name of a directory entry, converted from a normal string
    method file-name! ( c-addr u entry -- )
    
    \ Set the directory name of a directory entry, converted from a normal string
    method dir-name! ( c-addr u entry -- )
    
    \ Get the file name of a directory entry, converted to a normal string
    method file-name@ ( c-addr u entry -- c-addr u' )
  end-class
  
  \ Is a cluster free?
  : free-cluster? ( cluster-link -- free? ) $0FFFFFFF and 0= ;
  
  \ Is a cluster an end cluster?
  : end-cluster? ( cluster-link -- end? ) $0FFFFFF8 and $0FFFFFF8 = ;
  
  \ Is cluster a linked cluster?
  : link-cluster? ( cluster-link -- link? )
    $0FFFFFFF and dup $00000002 >= swap $0FFFFFEF <= and
  ;
  
  \ Get the link of a linked cluster
  : cluster-link ( cluster-link -- cluster ) $0FFFFFFF and ;
  
  \ Free cluster marker
  $00000000 constant free-cluster-mark
  
  \ End cluster marker
  $0FFFFFF8 constant end-cluster-mark
  
  \ Count the number of spaces from the end of a string
  : count-end-spaces ( c-addr u -- u' )
    0 begin over 0> while ( c-addr u u' )
      -rot 1- -rot dup c@ $20 = if ( u u' c-addr )
        1+ rot rot 1+
      else
        1+ rot rot drop 0
      then ( c-addr u u' )
    repeat
    nip nip
  ;
  
  \ Strip the spaces from the end of a string
  : strip-end-spaces ( c-addr u -- c-addr u' ) 2dup count-end-spaces - ;
  
  \ Write a string into another string, limited by its size, and return the remaining buffer
  : >string ( c-addr u c-addr' u' -- c-addr'' u'' )
    2dup 2>r rot min dup >r move r> 2r> 2 pick - swap rot + swap
  ;
  
  \ Find the index of a dot in a string
  : dot-index ( c-addr u -- u' | -1 )
    0 begin over 0> while
      2 pick c@ [char] . = if nip nip exit else 1+ rot 1+ rot 1- rot then
    repeat
    2drop drop -1
  ;
  
  \ Get the dot count in a string
  : dot-count ( c-addr u -- count )
    0 begin over 0> while
      2 pick c@ [char] . = if 1+ then rot 1+ rot 1- rot
    repeat
    nip nip
  ;
  
  \ Validate a filename character
  : validate-file-name-char ( c -- )
    case
      [char] " of true endof
      [char] * of true endof
      [char] / of true endof
      [char] : of true endof
      [char] < of true endof
      [char] > of true endof
      [char] ? of true endof
      [char] \ of true endof
      [char] | of true endof
      swap false swap
    endcase triggers x-file-name-format
  ;
  
  \ Validate filename characters
  : validate-file-name-chars ( c-addr u -- ) ['] validate-file-name-char citer ;
  
  \ Convert a character to uppercase
  : upcase-char ( c -- c' )
    dup [char] a >= over [char] z <= and if
      [char] a - [char] A +
    then
  ;
  
  \ Convert a string to uppercase
  : upcase-string ( c-addr u xt -- )
    over [:
      tuck 2>r
      dup >r 0 ?do over i + c@ upcase-char over i + c! loop
      2drop r> 2r> -rot swap rot execute
    ;] with-allot
  ;
  
  \ Validate a filename
  : validate-file-name ( c-addr u -- )
    2dup validate-file-name-chars
    dup 12 <= averts x-file-name-format
    2dup dot-count 1 = averts x-file-name-format
    2dup dot-index dup 0 > averts x-file-name-format
    dup 8 <= averts x-file-name-format
    2dup swap 1- < averts x-file-name-format
    swap 4 - >=  averts x-file-name-format
    drop
  ;
  
  \ Validate a directory name
  : validate-dir-name ( c-addr u -- )
    2dup validate-file-name-chars
    2dup s" ." equal-strings? not if
      2dup s" .." equal-strings? not if
        2dup dot-count -1 = averts x-file-name-format
        nip 8 <= averts x-file-name-format
      then
    then
  ;
  
  \ Copy to a buffer, padded with spaces
  : copy-space-pad ( c-addr u c-addr' u' -- )
    3dup >r >r >r
    rot min move
    r> r> r>
    rot ?do $20 over i + c! loop drop
  ;
  
  \ Get the used portion of a string
  : used-string ( c-addr u c-addr' u' -- c-addr'' u'' ) rot - rot drop ;
  
  \ Convert a file name into a 8.3 uppercase format without a dot
  : convert-file-name ( c-addr u xt -- )
    >r 2dup validate-file-name
    r> 11 [:
      swap >r 3dup 8 copy-space-pad
      dup c@ $E5 = if $05 over c! then
      2 pick dot-index 1+ >r rot r@ + rot r> -
      rot dup >r 8 + 3 copy-space-pad
      r> 11
      dup 0 ?do over i + c@ upcase-char 2 pick i + c! loop
      r> execute
    ;] with-allot
  ;
  
  \ Convert a directory name into an 8 uppercase format
  : convert-dir-name ( c-addr u xt )
    >r 2dup validate-dir-name
    r> 11 [:
      swap >r -rot 2 pick 8 copy-space-pad
      dup c@ $E5 = if $05 over c! then
      dup 8 + $20 3 fill
      11 dup 0 ?do over i + c@ upcase-char 2 pick i + c! loop
      r> execute
    ;] with-allot
  ;
  
  \ Is name a directory name?
  : dir-name? ( c-addr u -- )
    s" ." 2over equal-strings? if
      true
    else
      s" .." 2over equal-strings? if
        true
      else
        dot-index 0=
      then
    then
  ;
  
  \ Convert a file name or directory name
  : convert-name ( c-addr u xt )
    >r 2dup dir-name? if r> convert-dir-name else r> convert-file-name then
  ;
  
  \ Implement FAT32 filesystem class
  <fat32-fs> begin-implement
    :noname ( first-sector device fs -- )
      dup [ <fs> ] -> new
      tuck fat32-device !
      tuck first-sector !
      [:
        r> sector-scratchpad sector-size r@ first-sector @ r@ fat32-device @ block@
        sector-scratchpad $00B + unaligned-h@ 512 = averts x-sector-size-not-supported
        sector-scratchpad $00D + c@ r@ cluster-sectors !
        sector-scratchpad $00E + h@ r@ reserved-sectors !
        sector-scratchpad $010 + c@ r@ fat-count !
        sector-scratchpad $020 + @ r@ sector-count !
        sector-scratchpad $024 + @ r@ fat-sectors !
        sector-scratchpad $02A + h@ 0= averts x-fs-version-not-supported
        sector-scratchpad $02C + @ r@ root-dir-cluster !
        sector-scratchpad $030 + @ r@ info-sector ! r>
      ;] fat32-lock with-lock
      read-info-sector
    ; define new
    
    :noname ( fs -- )
      [:
        r> sector-scratchpad sector-size r@ info-sector @ r@ fat32-device @ block@
        sector-scratchpad $000 + @ $41615252 = averts x-bad-info-sector
        sector-scratchpad $1E4 + @ $61427272 = averts x-bad-info-sector
        sector-scratchpad $1FC + @ $AA550000 = averts x-bad-info-sector
        sector-scratchpad $1E8 + @ r@ cluster-count@ min r@ free-cluster-count !
        sector-scratchpad $1EC + @ r> recent-allocated-cluster !
      ;] fat32-lock with-lock
    ; define read-info-sector
    
    :noname ( fs -- )
      [:
        r> sector-scratchpad sector-size r@ info-sector @ r@ fat32-device @ block@
        r@ free-cluster-count @ sector-scratchpad $1E8 + !
        r@ recent-allocated-cluster @ sector-scratchpad $1EC + !
        sector-scratchpad sector-size r@ info-sector @ r> fat32-device @ block!
      ;] fat32-lock with-lock
    ; define write-info-sector
    
    :noname ( cluster fat fs -- link )
      [:
        >r swap dup rot r@ cluster>fat-sector ( cluster sector )
        swap sector-size 4 / mod swap ( index sector )
        sector-scratchpad sector-size rot r> fat32-device @ block@ ( index )
        cells sector-scratchpad + @
      ;] fat32-lock with-lock
    ; define fat@
    
    :noname ( link cluster fat fs -- )
      [:
        >r swap dup rot r@ cluster>fat-sector ( link cluster sector )
        swap sector-size 4 / mod swap ( link index sector )
        sector-scratchpad sector-size 2 pick r@ fat32-device @ block@ ( link index sector )
        -rot dup cells scratchpad + @ ( sector link index old-link )
        $F0000000 and rot $0FFFFFFF and or ( sector index new-link )
        swap cells sector-scratchpad + ! ( sector )
        sector-scratchpad sector-size rot r> fat32-device @ block!
      ;] fat32-lock with-lock
    ; define fat!
    
    :noname ( link cluster fs -- )
      dup fat-count @ 0 ?do 3dup i swap fat! loop 2drop drop
    ; define all-fat!
    
    :noname ( cluster fat fs -- sector )
      >r r@ first-sector @ r@ reserved-sectors @ +
      swap r> fat-sectors @ * + swap sector-size 4 / / +
    ; define cluster>fat-sector
    
    :noname ( cluster fs -- sector )
      >r
      r@ first-sector @
      r@ reserved-sectors @ +
      r@ fat-count @ r@ fat-sectors @ * +
      swap 2 - r> cluster-sectors @ * +
    ; define cluster>sector
    
    :noname ( offset cluster fs -- sector )
      cluster>sector swap sector-size / +
    ; define cluster-offset>sector
    
    :noname ( sector fs -- cluster )
      >r
      r@ first-sector @ -
      r@ reserved-sectors @ -
      r@ fat-count @ r@ fat-sectors @ * -
      r> cluster-sectors @ /
      2 +
    ; define sector>cluster
    
    :noname ( fs -- count )
      >r
      r@ sector-count @
      r@ reserved-sectors @ -
      r@ fat-count @ r@ fat-sectors @ * -
      r> fat-sectors @ sector-size * 4 / 2 - min
    ; define cluster-count@
    
    :noname ( fs -- cluster )
      >r r@ recent-allocated-cluster @
      dup -1 = if drop 2 else 1+ then
      r> dup cluster-count@ 2 + 2 pick ?do
        i over 0 swap fat@ cluster-free? if
          2drop i unloop exit
        then
      loop
      swap 2 ?do
        i over 0 swap fat@ cluster-free? if
          drop i unloop exit
        then
      loop
      drop ['] x-no-clusters-free ?raise
    ; define find-free-cluster
    
    :noname ( fs -- cluster )
      >r r@ find-free-cluster
      end-cluster-mark over r@ all-fat!
      dup r@ recent-allocated-cluster !
      -1 r@ free-cluster-count +!
      r> write-info-sector
    ; define allocate-cluster
    
    :noname ( cluster fs -- cluster' )
      dup allocate-cluster ( cluster fs cluster' )
      dup >r -rot all-fat! r>
    ; define allocate-link-cluster
    
    :noname ( cluster fs -- )
      >r free-cluster-mark swap r@ all-fat!
      1 r@ free-cluster-count +!
      r> write-info-sector
    ; define free-cluster
    
    :noname ( cluster fs -- )
      >r begin
        dup 0 r@ fat@
        swap r@ free-cluster
        dup link-cluster? if cluster-link false else drop true then
      until
      rdrop
    ; define free-cluster-chain
    
    :noname ( cluster fs -- )
      >r 0 r@ fat@
      dup link-cluster? if cluster-link r@ free-cluster-chain else drop then
      rdrop
    ; define free-cluster-tail
    
    :noname ( index cluster fs -- index cluster | -1 -1 )
      begin 2 pick over dir-cluster-entry-count@ > while
        rot over dir-cluster-entry-count@ - rot
        2dup 0 swap fat@
        dup link-cluster? if
          rot drop swap
        else
          2drop 2drop -1 -1 exit
        then
      repeat
      drop
    ; define find-entry 
    
    :noname ( entry index cluster fs -- )
      dup >r find-entry r>
      over -1 <> averts x-out-of-range-entry
      [:
        >r
        r@ cluster>sector
        over [ sector-size entry-size / ] literal u/ +
        sector-scratchpad sector-size rot r> fat32-device @ block@
        [ sector-size entry-size / ] literal umod entry-size *
        sector-scratchpad + swap entry-size move
      ;] fat32-lock with-lock
    ; define entry@
    
    :noname ( entry index cluster fs -- )
      dup >r find-entry r>
      over -1 <> averts x-out-of-range-entry
      [:
        >r
        r@ cluster>sector
        over [ sector-size entry-size / ] literal u/ + r> swap dup >r swap >r
        sector-scratchpad sector-size rot r@ fat32-device @ block@
        [ sector-size entry-size / ] literal umod entry-size *
        sector-scratchpad + entry-size move
        sector-scratchpad sector-size >r >r swap fat32-device @ block@
      ;] fat32-lock with-lock
    ; define entry!
    
    :noname ( fs -- count )
      cluster-sectors @ [ sector-size entry-size / ] literal *
    ; define dir-cluster-entry-count@

    :noname ( c-addr u cluster fs -- index cluster )
      2swap [:
        <fat32-entry> [:
          2swap 0 -rot begin ( name entry index cluster fs )
            dup >r find-entry r> ( name entry index cluster fs )
            over -1 <> averts x-entry-not-found
            2over 2over entry@
            2 pick short-file-name c@ dup $00 <> averts x-entry-not-found
            $35 <> if ( name entry index cluster fs )
              4 pick 8 5 pick short-file-name 8 equal-case-strings? if
                4 pick 8 + 3 5 pick short-file-ext 3 equal-case-strings? if
                  drop 2swap 2drop exit
                then
              then
            then
            rot 1+ -rot false
          again
        ;] with-object
      ;] convert-name
    ; define lookup-entry
    
    :noname ( cluster fs -- index cluster )
      0 -rot
      begin
        dup >r find-entry r>
        <fat32-entry> [: ( index cluster fs entry )
          dup 4 pick 4 pick 4 pick entry@
          dup short-file-name c@ $00 = if
            drop 3dup expand-dir drop true
          else
            short-file-name c@ $E5 = if
              drop true
            else
              rot 1+ -rot false
            then
          then
        ;] with-object
      until
    ; define allocate-entry
    
    :noname ( index cluster fs -- )
      <fat32-entry> [:
        dup 4 pick 4 pick 4 pick entry@
        dup mark-entry-deleted
        3 pick 3 pick 3 pick entry!
        2drop drop
      ;] with-object
    ; define delete-entry
    
    :noname ( index cluster fs -- )
      rot 1+ over cluster-sectors @ sector-size * entry-size u/ umod -rot
      2 pick 0= if
        rot drop dup allocate-link-cluster ( fs cluster )
        <fat32-entry> [: ( fs cluster entry )
          dup init-end-entry -rot 0 -rot swap ( entry index cluster fs ) entry!
        ;] with-object
      else
        <fat32-entry> [:
          swap >r swap >r swap >r dup init-end-entry
          r> r> r> entry!
        ;] with-object
      then
    ; define expand-dir
  end-implement
  
  \ Implement FAT32 file class
  <fat32-file> begin-implement
    :noname ( fs file -- )
      dup [ <object> ] -> new
      tuck file-fs !
      -1 over file-parent-index !
      -1 over file-parent-cluster !
      -1 over file-start-cluster !
      0 over file-offset !
      -1 over file-current-cluster !
      0 swap file-current-cluster-index !
    ; define new
    
    :noname ( c-addr u parent-dir file -- )
      swap dir-start-cluster @ over file-fs @ allocate-entry ( c-addr u file parent-index parent-cluster )
      2 pick file-parent-cluster ! ( c-addr u file parent-index )
      over file-parent-index ! ( c-addr u file )
      dup file-fs @ allocate-cluster ( c-addr u file dir-cluster )
      2dup swap file-start-cluster ! ( c-addr u file dir-cluster )
      over file-current-cluster ! ( c-addr u file )
      0 over file-offset ! ( c-addr u file )
      <fat32-entry> [: ( c-addr u file entry )
        0 2 pick file-start-cluster @ rot ( c-addr u file 0 file-cluster entry )
        5 roll 5 roll rot ( file 0 file-cluster c-addr u entry )
        dup >r init-dir-entry r> ( file entry )
        over file-parent-index @ ( file entry parent-index )
        2 pick file-parent-cluster @ ( file entry parent-index parent-cluster )
        3 roll file-fs @ entry! ( )
      ;] with-object
    ; define create-file
    
    :noname ( c-addr u file -- bytes )
      >r 0 ( c-addr u read-bytes )
      begin ( c-addr u read-bytes )
        over 0<> if ( c-addr u read-bytes )
          -rot 2dup r@ read-file-sector ( read-bytes c-addr u part-bytes )
          dup 0<> if ( read-bytes c-addr u part-bytes )
            >r r@ - swap r@ + swap rot r> + ( c-addr' u' read-bytes' )
          else
            2drop drop true ( read-bytes flag )
          then
        else
          nip nip true ( read-bytes flag )
        then
      until ( read-bytes )
      rdrop ( read-bytes )
    ; define read-file
    
    :noname ( c-addr u file -- bytes )
      >r 0 ( c-addr u write-bytes )
      begin ( c-addr u write-bytes )
        over 0<> if ( c-addr u write-bytes )
          -rot 2dup r@ write-file-sector ( write-bytes c-addr u part-bytes )
          dup 0<> if ( write-bytes c-addr u part-bytes )
            >r r@ - swap r@ + swap rot r> + ( c-addr' u' write-bytes' )
            over 0<> if ( c-addr' u' write-bytes' )
              r@ expand-file ( c-addr' u' write-bytes' )
            then
          else
            2drop drop true ( write-bytes flag )
          then
        else
          nip nip true ( write-bytes flag )
        then
      until ( read-bytes )
      rdrop ( read-bytes )
    ; define write-file
    
    :noname ( file -- )
      >r ( )
      r@ file-current-cluster @ r@ file-fs @ free-cluster-tail ( )
      r@ file-offset @ r@ file-size! ( )
      rdrop ( )
    ; define truncate-file
    
    :noname ( c-addr u file -- bytes )
      >r r@ current-file-sector@ ( c-addr u sector )
      sector-size r@ r@ file-offset @ sector-size umod - ( c-addr u sector limit )
      r@ file-size@ r@ file-offset @ - min ( c-addr u sector limit )
      rot min ( c-addr sector u )
      r> over >r >r swap ( c-addr u sector )
      r@ file-offset @ sector-size umod swap ( c-addr u offset sector )
      r@ file-fs @ fat32-device @ block-part@ ( )
      r@ file-offset @ r> r> swap >r + ( new-offset )
      dup r@ file-offset @ - ( new-offset bytes-read )
      swap r@ file-offset ! ( bytes-read )
      r> advance-cluster ( bytes-read )
    ; define read-file-sector
    
    :noname ( c-addr u file -- bytes )
      >r r@ current-file-sector@ ( c-addr u sector )
      sector-size r@ r@ file-offset @ sector-size umod - ( c-addr u sector limit )
      rot min ( c-addr sector u )
      r> over >r >r swap ( c-addr u sector )
      r@ file-offset @ sector-size umod swap ( c-addr u offset sector )
      r@ file-fs @ fat32-device @ block-part! ( )
      r@ file-offset @ r> r> swap >r + ( new-offset )
      r@ file-size@ ( new-offset file-size )
      over max r@ file-size! ( new-offset )
      dup r@ file-offset @ - ( new-offset bytes-written )
      swap r@ file-offset ! ( bytes-written )
      r> advance-cluster ( bytes-written )
    ; define write-file-sector
    
    :noname ( file -- bytes )
      <fat32-entry> [: ( file entry )
        dup rot dup file-parent-index @ ( entry entry file index )
        swap dup file-parent-cluster @ ( entry entry index file cluster )
        swap file-fs @ ( entry entry index cluster fs )
        entry@ ( entry )
        entry-file-size @ ( bytes )
      ;] with-object
    ; define file-size@
    
    :noname ( bytes file -- )
      <fat32-entry> [: ( bytes file entry )
        swap >r ( bytes entry )
        dup r@ file-parent-index @ ( bytes entry entry index )
        r@ file-parent-cluster @ ( bytes entry entry index cluster )
        r@ file-fs @ ( bytes entry entry index cluster fs )
        entry@ ( bytes entry )
        tuck entry-file-size ! ( entry )
        r@ file-parent-index @ ( entry index )
        r@ file-parent-cluster @ ( entry index cluster )
        r> file-fs @ ( entry index cluster fs )
        entry! ( )
      ;] with-object
    ; define file-size!
    
    :noname ( file -- sector )
      >r r@ file-offset @
      r@ file-fs @ cluster-sectors @ sector-size * umod
      r@ file-current-cluster @
      r> file-fs @ cluster-offset>sector
    ; define current-file-sector@
    
    :noname ( file -- )
      >r
      r@ file-offset @ r@ file-size@ < if
        r@ file-offset @ ( offset )
        r@ file-current-cluster-index @ ( offset index )
        r@ file-fs @ cluster-sectors @ sector-size * ( offset index cluster-size )
        dup >r * - r> = if ( )
          r@ file-current-cluster @ 0 r@ file-fs @ fat@ ( link )
          dup link-cluster? if ( link )
            cluster-link r@ file-current-cluster ! ( )
            1 r@ file-current-cluster-index +! ( )
          else ( link )
            drop ( )
          then
        then
      then
      rdrop
    ; define advance-cluster
    
    :noname ( file -- )
      >r
      r@ file-offset @ ( offset )
      r@ file-current-cluster-index @ ( offset index )
      r@ file-fs @ cluster-sectors @ sector-size * * - ( offset' )
      sector-size = if ( )
        r@ file-current-cluster @ 0 r@ file-fs @ fat @ ( link )
        end-cluster? if ( )
          r@ file-current-cluster @ r@ file-fs @ allocate-link-cluster ( cluster )
          r@ file-current-cluster ! ( )
          1 r@ file-current-cluster-index +! ( )
        then
      then
      rdrop
    ; define expand-file
    
  end-implement
  
  \ Implement FAT32 directory class
  <fat32-dir> begin-implement
    :noname ( fs dir -- )
      dup [ <object> ] -> new
      tuck dir-fs !
      -1 over dir-parent-index !
      -1 over dir-parent-cluster !
      -1 swap dir-start-cluster !
    ; define new
    
    :noname ( c-addr u parent-dir dir -- )
      swap dir-start-cluster @ over dir-fs @ allocate-entry ( c-addr u dir parent-index parent-cluster )
      2 pick dir-parent-cluster ! ( c-addr u dir parent-index )
      over dir-parent-index ! ( c-addr u dir )
      dup dir-fs @ allocate-cluster ( c-addr u dir dir-cluster )
      over dir-start-cluster ! ( c-addr u dir )
      <fat32-entry> [: ( c-addr u dir entry )
        over dir-start-cluster @ swap ( c-addr u dir dir-cluster entry )
        4 roll 4 roll rot ( dir dir-cluster c-addr u entry )
        dup >r init-dir-entry r> ( dir entry )
        over dir-parent-index @ ( dir entry parent-index )
        2 pick dir-parent-cluster @ ( dir entry parent-index parent-cluster )
        3 roll dir-fs @ entry! ( )
      ;] with-object
    ; define create-dir-raw
    
    :noname ( c-addr u parent-dir dir -- )
      2dup 2>r create-parent-raw 2r> ( parent-dir dir -- )
      dup create-dot-entry
      create-dot-dot-entry
    ; define create-dir
    
    \ Create . directory entry
    :noname ( dir -- )
      dup dir-start-cluster @ over dir-fs @ allocate-entry ( dir child-index child-cluster )
      <fat32-entry> [: ( dir child-index child-cluster entry )
        2 pick dir-start-cluster @ swap ( dir child-index child-cluster start-cluster entry )
        s" ." rot ( dir child-index child-cluster start-cluster c-addr u entry )
        dup >r init-dir-entry r> ( dir child-index child-cluster entry )
        3 roll dir-fs @ entry! ( )
      ;] with-object    ;
    ; define create-dot-entry
    
    \ Create .. directory entry
    :noname ( parent-dir dir -- )
      dup dir-start-cluster @ over dir-fs @ allocate-entry ( parent-dir dir child-index child-cluster )
      <fat32-entry> [: ( parent-dir dir child-index child-cluster entry )
        4 roll dir-start-cluster @ swap ( dir child-index child-cluster start-cluster entry )
        s" .." rot ( dir child-index child-cluster start-cluster c-addr u entry )
        dup >r init-dir-entry r> ( dir child-index child-cluster entry )
        3 roll dir-fs @ entry! ( )
      ;] with-object
    ; define create-dot-dot-entry
  end-implement
  
  \ Implement FAT32 directory entry class
  <fat32-entry> begin-implement

    :noname ( addr entry -- )
      2dup swap 0 + swap short-file-name 8 move
      2dup swap 8 + swap short-file-ext 3 move
      2dup swap 11 + c@ swap file-attributes c!
      2dup swap 12 + c@ swap nt-vfat-case c!
      2dup swap 13 + c@ swap create-time-fine c!
      2dup swap 14 + h@ swap create-time-coarse h!
      2dup swap 16 + h@ swap create-date h!
      2dup swap 18 + h@ swap access-date h!
      2dup swap 20 + h@ swap first-cluster-high h!
      2dup swap 22 + h@ swap modify-time-coarse h!
      2dup swap 24 + h@ swap modify-date h!
      2dup swap 26 + h@ swap first-cluster-low h!
      swap 28 + @ swap entry-file-size !
    ; define buffer>entry
    
    :noname ( addr entry -- )
      2dup short-file-name swap 0 + 8 move
      2dup short-file-exit swap 8 + 3 move
      2dup file-attributes c@ swap 11 + c!
      2dup nt-vfat-case c@ swap 12 + c!
      2dup create-time-fine c@ swap 13 + c!
      2dup create-time-coarse h@ swap 14 + h!
      2dup create-date h@ swap 16 + h!
      2dup access-date h@ swap 18 + h!
      2dup first-cluster-high h@ swap 20 + h!
      2dup modify-time-coarse h@ swap 22 + h!
      2dup modify-date h@ swap 24 + h!
      2dup first-cluster-low h@ swap 26 + h!
      entry-file-size @ swap 28 + !
    ; define entry>buffer
    
    :noname ( file-size first-cluster c-addr u entry -- )
      dup >r file-name!
      r@ first-cluster!
      r@ entry-file-size !
      0 r@ file-attributes c!
      0 r@ nt-vfat-case c!
      0 r@ create-time-fine c!
      0 r@ create-time-coarse h!
      [ 0 9 lshift 1 5 lshift or 1 0 lshift or ] literal r@ create-date h!
      [ 0 9 lshift 1 5 lshift or 1 0 lshift or ] literal r@ access-date h!
      0 r@ modify-time-coarse h!
      [ 0 9 lshift 1 5 lshift or 1 0 lshift or ] literal r> modify-date h!
    ; define init-file-entry

    :noname ( first-cluster c-addr u entry -- )
      dup >r dir-name!
      r@ first-cluster!
      0 r@ entry-file-size !
      $10 r@ file-attributes c!
      0 r@ nt-vfat-case c!
      0 r@ create-time-fine c!
      0 r@ create-time-coarse h!
      [ 0 9 lshift 1 5 lshift or 1 0 lshift or ] literal r@ create-date h!
      [ 0 9 lshift 1 5 lshift or 1 0 lshift or ] literal r@ access-date h!
      0 r@ modify-time-coarse h!
      [ 0 9 lshift 1 5 lshift or 1 0 lshift or ] literal r> modify-date h!
    ; define init-dir-entry
    
    :noname ( entry -- )
      dup short-file-name 8 0 fill
      dup short-file-ext 8 0 fill
      0 over file-attributes c!
      0 over nt-vfat-case c!
      0 over create-time-fine c!
      0 over create-time-coarse h!
      0 over create-date h!
      0 over access-date h!
      0 over first-cluster-high h!
      0 over modify-time-coarse h!
      0 over modify-date h!
      0 over first-cluster-low h!
      0 swap entry-file-size !
    ; define init-end-entry
    
    :noname ( entry -- ) $E5 swap short-file-name c! ; define mark-entry-deleted
    
    :noname ( entry -- deleted? ) short-file-name c@ $E5 = ; define entry-deleted?
    
    :noname ( entry -- end? ) short-file-name c@ $00 = ; define entry-end?
    
    :noname ( entry -- file? ) file-attributes c@ $58 and 0= ; define entry-file?
    
    :noname ( entry -- dir? ) file-attributes c@ $10 and 0<> ; define entry-dir?
    
    :noname ( cluster entry -- )
      2dup swap $FFFF and swap first-cluster-low h@
      swap 16 rshift swap first-cluster-high h!
    ;
    
    :noname ( entry -- cluster )
      dup first-cluster-low h@ swap first-cluster-high h@ 16 lshift or
    ; define first-cluster@
    
    :noname ( c-addr u entry -- )
      -rot 2dup validate-file-name
      [:
        rot r>
        2dup dot-index
        2 pick over r@ short-file-name 8 copy-space-pad
        1+ rot over + rot rot - r@ short-file-ext 3 copy-space-pad
        r@ short-file-name c@ $E5 = if
          $05 r@ short-file-name c!
        then
        rdrop
      ;] upcase-string
    ; define file-name!
    
    :noname ( c-addr u entry -- )
      -rot 2dup validate-dir-name
      [:
        rot r>
        r@ short-file-name 8 copy-space-pad
        s" " r@ short-file-ext 3 copy-space-pad
        r@ short-file-name c@ $E5 = if
          $05 r@ short-file-name c!
        then
        rdrop
      ;] upcase-string
    ; define dir-name!
        
    :noname ( c-addr u entry -- c-addr u' )
      over 0<> if
        2 pick >r >r
        r@ entry-dir? if
          r> short-file-name 8 strip-end-spaces
          2swap 2dup 2>r >string 2r> used-string
        else
          r@ entry-file? if
            r@ short-file-name 8 strip-end-spaces
            2swap 2dup 2>r >string
            s" ." 2swap >string
            2r> r> -rot 2>r
            short-file-ext 3 strip-end-spaces
            2swap >string 2r> used-string
          else
            rdrop drop 0
          then
        then
        r> dup c@ $05 = if $E5 swap c! else drop then
      else
        drop
      then
    ; define file-name@
  end-implement

  \ FAT32 directory class
  <dir> begin-class <fat32-dir>
    cell member fat32-fs
  end-class
  
  \ Implement FAT32 directory class
  <fat32-dir> begin-implement
    \ Get the class of an entity ( c-addr u dir -- class )
    :noname ['] x-method-not-implemented ? raise ; define entity-class@

    \ Initialize the memory for an entity ( c-addr u addr dir -- )
    :noname ['] x-method-not-implemented ? raise ; define init-entity

    \ Get the class of a directory ( dir -- class )
    :noname ['] x-method-not-implemented ? raise ; define dir-class@

    \ Create a directory ( c-addr u addr dir -- )
    :noname ['] x-method-not-implemented ? raise ; define create-dir

    \ Get the class of a file ( dir -- class )
    :noname ['] x-method-not-implemented ? raise ; define file-class@

    \ Create an ordinary file ( c-addr u addr dir -- )
    :noname ['] x-method-not-implemented ? raise ; define create-file
  end-implement

  \ FAT32 file class
  <file> begin-class <fat32-file> end-class

  \ Implement FAT32 file class
  <fat32-file> begin-implement
    \ Read from a file ( c-addr u file -- u )
    :noname ['] x-method-not-implemented ?raise ; define read-file

    \ Write to a file ( c-addr u file -- u )
    :noname ['] x-method-not-implemented ?raise ; define write-file

    \ Seek in a file ( offset whence file -- )
    :noname ['] x-method-not-implemented ?raise ; define seek-file

    \ Get the current offset in a file ( file -- offset )
    :noname ['] x-method-not-implemented ?raise ; define tell-file

    \ Flush a file ( file -- )
    :noname ['] x-method-not-implemented ?raise ; define flush-file

    \ Close a file ( file -- )
    :noname ['] x-method-not-implemented ?raise ; define close-file
  end-implement
  
end-module
