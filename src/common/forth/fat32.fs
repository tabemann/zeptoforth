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

begin-module fat32

  oo import
  lock import
  block-dev import
  
  \ Sector size exception
  : x-sector-size-not-supported ( -- ) ." sector sizes other than 512 are not supported" cr  ;
  
  \ Filesystem version not supported exception
  : x-fs-version-not-supported ( -- ) ." FAT32 version not supported" cr ;
  
  \ Bad info sector exception
  : x-bad-info-sector ( -- ) ." bad info sector" cr ;
  
  \ No clusters free exception
  : x-no-clusters-free ( -- ) ." no clusters free" cr ;
  
  \ Unsupported file name format exception
  : x-file-name-format ( -- ) ." unsupported filename" cr ;
  
  \ Out of range directory entry index exception
  : x-out-of-range-entry ( -- ) ." out of range directory entry" cr ;
  
  \ Out of range partition index exception
  : x-out-of-range-partition ( -- ) ." out of range partition" cr ;
  
  \ Directory entry not found exception
  : x-entry-not-found ( -- ) ." directory entry not found" cr ;
  
  \ Directory entry already exists exception
  : x-entry-already-exists ( -- ) ." directory entry already exists" cr ;
  
  \ Directory entry is not a file exception
  : x-entry-not-file ( -- ) ." directory entry not file" cr ;
  
  \ Directory entry is not a directory exception
  : x-entry-not-dir ( -- ) ." directory entry not directory" cr ;
  
  \ Directory is not empty exception
  : x-dir-is-not-empty ( -- ) ." directory is not empty" cr ;
  
  \ Directory name being changed or set is forbidden exception
  : x-forbidden-dir ( -- ) ." forbidden directory name" cr ;
  
  \ No file or directory referred to in path within directory exception
  : x-empty-path ( -- ) ." empty path" cr ;
  
  \ Invalid path exception
  : x-invalid-path ( -- ) ." invalid path" cr ;

  \ Seek from the beginning of a file
  0 constant seek-set

  \ Seek from the current position in a file
  1 constant seek-cur

  \ Seek from the end of a file
  2 constant seek-end
  
  begin-module fat32-internal

    \ The FAT32 lock
    lock-size buffer: fat32-lock
    
    \ The only supported sector size
    512 constant sector-size
    
    \ The only supported directory entry size
    32 constant entry-size
    
    \ Sector scratchpad buffer
    sector-size buffer: sector-scratchpad
    
    \ Unaligned halfword access
    : unaligned-h@ ( addr -- h ) dup c@ swap 1+ c@ 8 lshift or ;
  
  end-module> import
    
  \ Master boot record class
  <object> begin-class <mbr>
  
    continue-module fat32-internal
    
      \ The device to which this master boot record belongs
      cell member mbr-device
      
    end-module
    
    \ Is the MBR valid
    method mbr-valid? ( mbr -- valid? )
    
    \ Read a partition entry
    method partition@ ( partition index mbr -- )
    
    \ Write a partition entry
    method partition! ( partition index mbr -- )
  end-class
  
  \ Partition class
  <object> begin-class <partition>
    \ Partition active state
    cell member partition-active
    
    \ Partition type
    cell member partition-type
    
    \ Partition first sector index
    cell member partition-first-sector
    
    \ Partition sectors
    cell member partition-sectors
    
    \ Is the partition active
    method partition-active? ( partition -- active? )
  end-class
  
  \ Base FAT32 filesystem class
  <object> begin-class <base-fat32-fs>
  
    \ Get the root directory for a FAT32 filesystem
    method root-dir@ ( dir fs -- )
    
    \ Find a file or directory in a path from the root directory
    method with-root-path ( c-addr u xt fs -- ) ( xt: c-addr' u' dir -- )

    \ Find whether a file or directory in a path from the root directory exists
    method root-path-exists? ( c-addr u fs -- exists?  )
    
    \ Flush the block device for a filesystem
    method flush ( fs -- )
  
  end-class
  
  \ Implement base FAT32 filesystem class
  <base-fat32-fs> begin-implement
    ' abstract-method define root-dir@
    ' abstract-method define with-root-path
    ' abstract-method define root-path-exists?
    ' abstract-method define flush
  end-implement
  
  \ FAT32 filesystem class
  <base-fat32-fs> begin-class <fat32-fs>
  
    continue-module fat32-internal
    
      \ The device to which this filesystem belongs
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

      \ Check whether a directory entry exists by name
      method entry-exists? ( c-addr u cluster fs -- exists? )
      
      \ Allocate a directory entry
      method allocate-entry ( cluster fs -- index cluster )
      
      \ Delete a directory entry
      method delete-entry ( index cluster fs -- )
      
      \ Expand a directory by one entry
      method expand-dir ( index cluster fs -- )
    
    end-module
    
  end-class
  
  \ FAT32 file class
  <object> begin-class <fat32-file>
    
    continue-module fat32-internal
      
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
    
    end-module
      
    \ Read data from a file
    method read-file ( c-addr u file -- bytes )
    
    \ Write data to a file
    method write-file ( c-addr u file -- bytes )
    
    \ Truncate a file
    method truncate-file ( file -- )
    
    \ Seek in a file
    method seek-file ( offset whence file -- )
    
    \ Get the current offset in a file
    method tell-file ( file -- offset )
      
    \ Get the size of a file
    method file-size@ ( file -- bytes )
    
    continue-module fat32-internal
      
      \ Do the work of creating a file
      method do-create-file ( c-addr u parent-dir file -- )
      
      \ Do the work of opening a file
      method do-open-file ( c-addr u parent-dir file -- )
      
      \ Read up to a sector in a file
      method read-file-sector ( c-addr u file -- bytes )
      
      \ Write up to a sector in a file
      method write-file-sector ( c-addr u file -- bytes )
      
      \ Actually seek in a file
      method do-seek-file ( offset file -- )
      
      \ Set the size of a file
      method file-size! ( bytes file -- )
      
      \ Get the current sector in a file
      method current-file-sector@ ( file -- sector )
      
      \ Advance the current cluster if at the end of the current cluster
      method advance-cluster ( file -- )
      
      \ Expand a file
      method expand-file ( file -- )
      
    end-module
    
  end-class
  
  \ FAT32 directory class
  <object> begin-class <fat32-dir>
    
    continue-module fat32-internal
    
      \ Filesystem
      cell member dir-fs
    
      \ Parent directory index
      cell member dir-parent-index
      
      \ Parent directory cluster
      cell member dir-parent-cluster
      
      \ Starting cluster
      cell member dir-start-cluster
      
      \ Current offset in a directory in entries
      cell member dir-offset
      
      \ Current cluster in a directory
      cell member dir-current-cluster
      
      \ Current cluster index in a directory
      cell member dir-current-cluster-index
      
    end-module
    
    \ Find a file or directory in a path from a directory
    method with-path ( c-addr u xt dir -- ) ( xt: c-addr' u' dir' -- )

    \ Find whether a file or directory in a path from a directory exists
    method path-exists? ( c-addr u dir -- exists? )

    \ Read an entry from a directory, and return whether an entry was read
    method read-dir ( entry dir -- entry-read? )
    
    \ Create a file
    method create-file ( c-addr u new-file dir -- )
    
    \ Open a file
    method open-file ( c-addr u opened-file dir -- )
    
    \ Remove a file
    method remove-file ( c-addr u dir -- )
    
    \ Create a directory
    method create-dir ( c-addr u new-dir dir -- )
    
    \ Open a directory
    method open-dir ( c-addr u opened-dir dir -- )
    
    \ Remove a directory
    method remove-dir ( c-addr u dir -- )
    
    \ Rename a file or directory
    method rename ( new-c-addr new-u c-addr u dir -- )
    
    \ Get whether a directory is empty
    method dir-empty? ( dir -- empty? )

    \ Get whether a directory entry exists
    method exists? ( c-addr u dir -- exists? )

    \ Get whether a directory entry is a file
    method file? ( c-addr u dir -- file? )

    \ Get whether a directory entry is a directory
    method dir? ( c-addr u dir -- dir? )
    
    continue-module fat32-internal
    
      \ Do the work of creating a directory with . and .. entries
      method do-create-dir ( c-addr u parent-dir dir -- )
      
      \ Do the work of creating a directory without any . or .. entries
      method do-create-dir-raw ( c-addr u parent-dir dir -- )
      
      \ Create . directory entry
      method do-create-dot-entry ( dir -- )
      
      \ Create .. directory entry
      method do-create-dot-dot-entry ( parent-dir dir -- )
      
      \ Do the work of opening a directory
      method do-open-dir ( c-addr u parent-dir dir -- )
      
    end-module
    
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
    
    \ Initialize a blank directory entry
    method init-blank-entry ( entry -- )
    
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
    
    \ Set the first cluster index of a directory entry
    method first-cluster! ( cluster entry -- )
    
    \ Set the file name of a directory entry, converted from a normal string
    method file-name! ( c-addr u entry -- )
    
    \ Set the directory name of a directory entry, converted from a normal string
    method dir-name! ( c-addr u entry -- )
    
    \ Get the file name of a directory entry, converted to a normal string
    method file-name@ ( c-addr u entry -- c-addr u' )
  
  end-class
  
  continue-module fat32-internal
    
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
      s\" \"*/:<>?\\|" begin ?dup while
        over c@ 3 pick <> averts x-file-name-format 1- swap 1+ swap
      repeat
      2drop
    ;
    
    \ Validate filename characters
    : validate-file-name-chars ( c-addr u -- )
      begin ?dup while
        over c@ validate-file-name-char 1- swap 1+ swap
      repeat
      drop
    ;
    
    \ Convert a character to uppercase
    : upcase-char ( c -- c' )
      dup [char] a >= over [char] z <= and if
        [char] a - [char] A +
      then
    ;
    
    \ Convert a string to uppercase
    : upcase-string ( c-addr u xt -- )
      over [: ( c-addr u xt buffer )
        3 roll over 4 pick move ( u xt buffer )
        2 pick 0 ?do dup i + c@ upcase-char over i + c! loop ( u xt buffer )
        -rot execute ( )
      ;] with-allot ( )
    ;
    
    \ Validate a filename
    : validate-file-name ( c-addr u -- )
      2dup validate-file-name-chars
      dup 12 <= averts x-file-name-format
      2dup dot-count 1 = averts x-file-name-format
      2dup dot-index dup 0 > averts x-file-name-format
      dup 8 <= averts x-file-name-format
      2dup swap 1- < averts x-file-name-format
      swap 4 - >= averts x-file-name-format
      drop
    ;
    
    \ Validate a directory name
    : validate-dir-name ( c-addr u -- )
      2dup validate-file-name-chars
      2dup s" ." equal-strings? not if
        2dup s" .." equal-strings? not if
          2dup dot-count 0= averts x-file-name-format
          nip 8 <= averts x-file-name-format
        else
          2drop
        then
      else
        2drop
      then
    ;
    
    \ Copy to a buffer, padded with spaces
    : copy-space-pad ( c-addr u c-addr' u' -- ) 2dup $20 fill rot min move ;
    
    \ Get the used portion of a string
    : used-string ( c-addr u c-addr' u' -- c-addr'' u'' ) rot - rot drop ;
    
    \ Convert a file name into a 8.3 uppercase format without a dot
    : convert-file-name ( c-addr u xt -- )
      >r 2dup validate-file-name
      r> 11 [:
        dup 11 $20 fill
        swap 2>r
        2dup dot-index 2 pick swap r@ swap move
        2dup dot-index 1+ rot over + -rot - r@ 8 + swap move
        r@ c@ $E5 = if $05 r@ c! then
        r> 11 0 ?do dup i + c@ upcase-char over i + c! loop
        r> execute
      ;] with-allot
    ;
    
    \ Convert a directory name into an 8 uppercase format
    : convert-dir-name ( c-addr u xt )
      >r 2dup validate-dir-name
      r> 11 [:
        dup 11 $20 fill
        swap 2>r
        r@ swap move
        r@ c@ $E5 = if $05 r@ c! then
        r> 11 0 ?do dup i + c@ upcase-char over i + c! loop
        r> execute
      ;] with-allot
    ;
    
    \ Is name a directory name?
    : dir-name? ( c-addr u -- )
      s" ." 2over equal-strings? if
        2drop true
      else
        s" .." 2over equal-strings? if
          2drop true
        else
          dot-index -1 =
        then
      then
    ;
    
    \ Convert a file name or directory name
    : convert-name ( c-addr u xt )
      >r 2dup dir-name? if r> convert-dir-name else r> convert-file-name then
    ;
    
    \ Check whether a directory name is forbidden?
    : forbidden-dir? ( c-addr u -- forbidden? )
      s" ." 2over equal-strings? if
        2drop true
      else
        s" .." equal-strings?
      then
    ;
    
    \ Find a path separator
    : find-path-separator ( c-addr u -- index | -1 )
      0 begin over 0> while
        2 pick c@ [char] / = if nip nip exit else 1+ rot 1+ rot 1- rot then
      repeat
      2drop drop -1
    ;
    
    \ Strip a final path separator
    : strip-final-path-separator ( c-addr u -- c-addr' u' )
      begin dup 0> while
        2dup 1- + c@ [char] / = if 1- else exit then
      repeat
    ;
    
    \ Validate a path
    : validate-path ( c-addr u -- )
      dup 0> if
        over c@ [char] . = if
          1- swap 1+ swap
          dup 0> if
            over c@ [char] . = if
              1- swap 1+ swap
              dup 0> if
                over c@ [char] / = averts x-invalid-path
              then
            else
              over c@ [char] / = averts x-invalid-path
            then
          then
        else
          1- swap 1+ swap
        then
      then
      begin dup 0> while
        over c@ [char] / = if
          1- swap 1+ swap
          dup 0> if
            over c@ [char] . = if
              1- swap 1+ swap
              dup 0> if
                over c@ [char] . = if
                  1- swap 1+ swap
                  dup 0> if
                    over c@ [char] / = averts x-invalid-path
                  then
                else
                  over c@ [char] / = averts x-invalid-path
                then
              then
            else
              1- swap 1+ swap
            then
          then
        else
          over c@ [char] . = if
            1- swap 1+ swap
            begin dup 0> while
              over c@ [char] / <> averts x-invalid-path
              1- swap 1+ swap
            repeat
          else
            1- swap 1+ swap
          then
        then
      repeat
      2drop
    ;
    
    \ Initialize the FAT32 layer
    : init-fat32 ( -- ) fat32-lock init-lock ;

  end-module
    
  \ Implement master boot record class
  <mbr> begin-implement
    :noname ( mbr-device mbr -- )
      dup <object>->new
      mbr-device !
    ; define new
    
    :noname ( mbr -- valid? )
      [:
        >r sector-scratchpad sector-size 0 r> mbr-device @ block@
        sector-scratchpad $1FE + h@ $AA55 =
      ;] fat32-lock with-lock
    ; define mbr-valid?
    
    :noname ( partition index mbr -- )
      over 0 >= 2 pick 4 < and averts x-out-of-range-partition
      [:
        >r sector-scratchpad $10 rot $10 * $1BE + 0 r> mbr-device @ block-part@
        sector-scratchpad $00 + c@ over partition-active !
        sector-scratchpad $04 + c@ over partition-type !
        sector-scratchpad $08 + @ over partition-first-sector !
        sector-scratchpad $0C + @ swap partition-sectors !
      ;] fat32-lock with-lock
    ; define partition@
    
    :noname ( partition index mbr -- )
      over 0 >= 2 pick 4 < and averts x-out-of-range-partition
      [:
        sector-scratchpad $10 $00 fill
        2 pick partition-active @ sector-scratchpad $00 + c!
        2 pick partition-type @ sector-scratchpad $04 + c!
        2 pick partition-first-sector @ sector-scratchpad $08 + !
        rot partition-sectors @ sector-scratchpad $0C + !
        >r sector-scratchpad $10 rot $10 * $1BE + 0 r> mbr-device @ block-part!
      ;] fat32-lock with-lock
    ; define partition!
    
  end-implement
  
  \ Implement partition class
  <partition> begin-implement
    :noname ( partition -- active? ) partition-active @ $80 >= ; define partition-active? 
  end-implement
  
  \ Implement FAT32 filesystem class
  <fat32-fs> begin-implement
    :noname ( partition device fs -- )
      dup <object>->new
      tuck fat32-device !
      tuck swap partition-first-sector @ swap first-sector !
      [:
        >r sector-scratchpad sector-size r@ first-sector @ r@ fat32-device @ block@
        sector-scratchpad $00B + unaligned-h@ 512 = averts x-sector-size-not-supported
        sector-scratchpad $00D + c@ r@ cluster-sectors !
        sector-scratchpad $00E + h@ r@ reserved-sectors !
        sector-scratchpad $010 + c@ r@ fat-count !
        sector-scratchpad $020 + @ r@ sector-count !
        sector-scratchpad $024 + @ r@ fat-sectors !
        sector-scratchpad $02A + h@ 0= averts x-fs-version-not-supported
        sector-scratchpad $02C + @ r@ root-dir-cluster !
        sector-scratchpad $030 + h@ r@ info-sector ! r>
      ;] fat32-lock with-lock
      read-info-sector
    ; define new
    
    :noname ( dir fs -- )
      2dup swap <fat32-dir> swap init-object
      2dup swap dir-fs !
      root-dir-cluster @ over dir-start-cluster !
      -1 over dir-parent-cluster !
      -1 over dir-parent-index !
      0 over dir-offset !
      dup dir-start-cluster @ over dir-current-cluster !
      0 swap dir-current-cluster-index !
    ; define root-dir@
    
    :noname ( c-addr u xt fs -- ) ( xt: c-addr' u' dir -- )
      <fat32-dir> class-size
      [: tuck swap root-dir@ with-path ;] with-aligned-allot
    ; define with-root-path

    :noname ( c-addr u fs -- exists? )
      <fat32-dir> class-size
      [: tuck swap root-dir@ path-exists? ;] with-aligned-allot
    ; define root-path-exists?
    
    :noname ( fs -- ) fat32-device @ flush-blocks ; define flush
    
    :noname ( fs -- )
      [:
        >r sector-scratchpad sector-size r@ info-sector @ r@ first-sector @ + r@ fat32-device @ block@
        sector-scratchpad $000 + @ $41615252 = averts x-bad-info-sector
        sector-scratchpad $1E4 + @ $61417272 = averts x-bad-info-sector
        sector-scratchpad $1FC + @ $AA550000 = averts x-bad-info-sector
        sector-scratchpad $1E8 + @ r@ cluster-count@ min r@ free-cluster-count !
        sector-scratchpad $1EC + @ r> recent-allocated-cluster !
      ;] fat32-lock with-lock
    ; define read-info-sector
    
    :noname ( fs -- )
      [:
        >r sector-scratchpad sector-size r@ info-sector @ r@ first-sector @ + r@ fat32-device @ block@
        r@ free-cluster-count @ sector-scratchpad $1E8 + !
        r@ recent-allocated-cluster @ sector-scratchpad $1EC + !
        sector-scratchpad sector-size r@ info-sector @ r@ first-sector @ + r> fat32-device @ block!
      ;] fat32-lock with-lock
    ; define write-info-sector
    
    :noname ( cluster fat fs -- link )
      [:
        >r swap dup rot r@ cluster>fat-sector ( cluster sector )
        swap sector-size 4 / umod swap ( index sector )
        sector-scratchpad sector-size rot r> fat32-device @ block@ ( index )
        cells sector-scratchpad + @ 
      ;] fat32-lock with-lock
    ; define fat@
    
    :noname ( link cluster fat fs -- )
      [:
        >r swap dup rot r@ cluster>fat-sector ( link cluster sector )
        swap sector-size 4 / umod swap ( link index sector )
        sector-scratchpad sector-size 2 pick r@ fat32-device @ block@ ( link index sector )
        -rot dup cells sector-scratchpad + @ ( sector link index old-link )
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
        i over 0 swap fat@ free-cluster? if
          2drop i unloop exit
        then
      loop
      swap 2 ?do
        i over 0 swap fat@ free-cluster? if
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
      <fat32-entry> 4 pick init-object
      dup >r find-entry r>
      over -1 <> averts x-out-of-range-entry
      [:
        >r
        r@ cluster>sector
        over [ sector-size entry-size / ] literal u/ +
        sector-scratchpad sector-size rot r> fat32-device @ block@
        [ sector-size entry-size / ] literal umod entry-size *
        sector-scratchpad + swap buffer>entry
      ;] fat32-lock with-lock
    ; define entry@
    
    :noname ( entry index cluster fs -- )
      dup >r find-entry r> ( entry index' cluster' fs )
      over -1 <> averts x-out-of-range-entry ( entry index' cluster' fs )
      [: ( entry index' cluster' fs )
        >r ( entry index' cluster' ) 
        r@ cluster>sector ( entry index' sector )
        over [ sector-size entry-size / ] literal u/ + r> swap dup >r swap >r ( entry index' sector' )
        sector-scratchpad sector-size rot r@ fat32-device @ block@ ( entry index' )
        [ sector-size entry-size / ] literal umod entry-size * ( entry offset )
        sector-scratchpad + swap entry>buffer ( )
        sector-scratchpad sector-size r> r> swap fat32-device @ block! ( )
      ;] fat32-lock with-lock ( )
    ; define entry!
    
    :noname ( fs -- count )
      cluster-sectors @ [ sector-size entry-size / ] literal *
    ; define dir-cluster-entry-count@

    :noname ( c-addr u cluster fs -- index cluster )
      2swap ( cluster fs c-addr u ) [: ( cluster fs name )
        <fat32-entry> [: ( cluster fs name entry )
          2swap ( name entry cluster fs ) 0 -rot begin ( name entry index cluster fs )
            dup >r find-entry r> ( name entry index cluster fs )
            over -1 <> averts x-entry-not-found
            2over 2over entry@
            3 pick short-file-name c@ dup $00 <> averts x-entry-not-found
            $E5 <> if ( name entry index cluster fs )
              4 pick 8 5 pick short-file-name 8 equal-case-strings? if
                4 pick 8 + 3 5 pick short-file-ext 3 equal-case-strings? if
                  drop 2swap 2drop exit
                then
              then
            then
            rot 1+ -rot
          again
        ;] with-object
      ;] convert-name
    ; define lookup-entry

    :noname ( c-addr u cluster fs -- exists? )
      2swap ( cluster fs c-addr u ) [: ( cluster fs name )
        <fat32-entry> [: ( cluster fs name entry )
          2swap ( name entry cluster fs ) 0 -rot begin ( name entry index cluster fs )
            dup >r find-entry r> ( name entry index cluster fs )
            over -1 = if
              drop 2drop 2drop false exit
            then
            2over 2over entry@
            3 pick short-file-name c@ dup $00 = if
              2drop 2drop 2drop false exit
            then
            $E5 <> if ( name entry index cluster fs )
              4 pick 8 5 pick short-file-name 8 equal-case-strings? if
                4 pick 8 + 3 5 pick short-file-ext 3 equal-case-strings? if
                  drop 2drop 2drop true exit
                then
              then
            then
            rot 1+ -rot
          again
        ;] with-object
      ;] convert-name
    ; define entry-exists?

    :noname ( cluster fs -- index cluster )
      0 -rot
      begin
        dup >r find-entry r>
        <fat32-entry> [: ( index cluster fs entry )
          dup 4 pick 4 pick 4 pick entry@
          dup short-file-name c@ $00 = if
            drop 3dup expand-dir true
          else
            short-file-name c@ $E5 = if
              true
            else
              rot 1+ -rot false
            then
          then
        ;] with-object
      until ( index cluster fs )
      <fat32-entry> [: ( index cluster fs entry )
        dup init-blank-entry ( index cluster fs entry )
        swap 2swap rot ( entry index cluster fs )
        2 pick 2 pick 2>r ( entry index cluster fs )
        entry! 2r> ( index cluster )
      ;] with-object
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
      dup <object>->new
      tuck file-fs !
      -1 over file-parent-index !
      -1 over file-parent-cluster !
      -1 over file-start-cluster !
      0 over file-offset !
      -1 over file-current-cluster !
      0 swap file-current-cluster-index !
    ; define new
        
    :noname ( c-addr u file -- bytes 
      >r 0 ( c-addr u read-bytes )
      begin ( c-addr u read-bytes )
        over 0<> if ( c-addr u read-bytes )
          -rot 2dup r@ read-file-sector ( read-bytes c-addr u part-bytes )
          dup 0<> if ( read-bytes c-addr u part-bytes )
            >r r@ - swap r@ + swap rot r> + false ( c-addr' u' read-bytes' )
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
            r@ expand-file ( c-addr' u' write-bytes' )
            false
          else
            2drop drop true ( write-bytes flag )
          then
        else
          nip nip true ( write-bytes flag )
        then
      until ( write-bytes )
      rdrop ( write-bytes )
    ; define write-file
    
    :noname ( file -- )
      >r ( )
      r@ file-current-cluster @ r@ file-fs @ free-cluster-tail ( )
      r@ file-offset @ r@ file-size! ( )
      rdrop ( )
    ; define truncate-file
    
    :noname ( offset whence file -- )
      >r case
        seek-set of endof
        seek-cur of r@ file-offset @ + endof
        seek-end of r@ file-size@ + endof
      endcase
      r> do-seek-file
    ; define seek-file
    
    :noname ( file -- offset ) file-offset @ ; define tell-file
    
    :noname ( c-addr u parent-dir file -- )
      3 pick 3 pick 3 pick dir-start-cluster @ 3 pick file-fs @ entry-exists?
      triggers x-entry-already-exists
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
        dup >r init-file-entry r> ( file entry )
        over file-parent-index @ ( file entry parent-index )
        2 pick file-parent-cluster @ ( file entry parent-index parent-cluster )
        3 roll file-fs @ entry! ( )
      ;] with-object
    ; define do-create-file
    
    :noname ( c-addr u parent-dir file -- )
      swap 2swap rot dir-start-cluster @ 3 pick file-fs @ lookup-entry ( file entry-index entry-cluster )
      2 pick file-parent-cluster ! ( file entry-index )
      2dup swap file-parent-index ! ( file entry-index )
      <fat32-entry> [: ( file entry-index entry )
        dup >r swap 2 pick dir-parent-cluster @ 3 pick dir-fs @ ( file entry entry-index entry-cluster fs )
        entry@
        r@ entry-file? averts x-entry-not-file
        r> first-cluster@ swap 2dup file-start-cluster ! file-current-cluster ! ( )
      ;] with-object
    ; define do-open-file
    
    :noname ( c-addr u file -- bytes )
      >r r@ current-file-sector@ ( c-addr u sector )
      sector-size r@ file-offset @ sector-size umod - ( c-addr u sector limit )
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
      sector-size r@ file-offset @ sector-size umod - ( c-addr u sector limit )
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
    
    :noname ( offset file -- )
      >r r@ file-size@ min 0 max dup r@ file-offset ! ( offset )
      0 r@ file-current-cluster-index ! ( offset )
      r@ file-start-cluster @ begin ( offset cluster )
        over sector-size < if ( offset cluster )
          r> file-current-cluster ! drop true ( flag )
        else
          dup 0 r@ file-fs @ fat@ dup link-cluster? if ( offset cluster link )
            1 r@ file-current-cluster-index +! ( offset cluster link )
            nip cluster-link ( offset cluster' )
            swap sector-size - swap false ( offset' cluster' flag )
          else
            drop r> file-current-cluster ! drop true ( flag )
          then
        then
      until
    ; define do-seek-file
    
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
        r@ file-current-cluster-index @ 1+ ( offset index )
        r@ file-fs @ cluster-sectors @ sector-size * * = if ( )
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
      r@ file-current-cluster-index @ 1+ ( offset index )
      r@ file-fs @ cluster-sectors @ sector-size * * = if ( )
        r@ file-current-cluster @ 0 r@ file-fs @ fat@ ( link )
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
      dup <object>->new
      tuck dir-fs !
      -1 over dir-parent-index !
      -1 over dir-parent-cluster !
      -1 over dir-start-cluster !
      0 over dir-offset !
      -1 over dir-current-cluster !
      0 swap dir-current-cluster-index !
    ; define new
    
    :noname ( c-addr u xt dir -- ) ( xt: c-addr' u' dir' -- )
      swap 2>r ( c-addr u )
      2dup validate-path ( c-addr' u' )
      strip-final-path-separator ( c-addr' u' )
      dup 0<> averts x-empty-path ( c-addr' u' )
      2dup find-path-separator ( c-addr' u' index )
      dup 0<> if
        dup -1 = if ( c-addr' u' index )
          drop 2r> execute ( )
        else
          2 pick over 2r> <fat32-dir> class-size [: ( c-addr' u' index c-addr'' u'' dir xt dir' )
            -rot >r over >r open-dir ( c-addr' u' index )
            1+ tuck - -rot + swap 2r> swap with-path ( )
          ;] with-aligned-allot
        then
      else ( c-addr' u' index )
        drop 1- swap 1+ swap 2r> swap with-path ( )
      then ( )
    ; define with-path

    :noname ( c-addr u dir -- exists? )
      >r ( c-addr u )
      2dup validate-path ( c-addr' u' )
      strip-final-path-separator ( c-addr' u' )
      dup 0<> averts x-empty-path ( c-addr' u' )
      2dup find-path-separator ( c-addr' u' index )
      dup 0<> if
        dup -1 = if ( c-addr' u' index )
          drop r> exists? ( exists? )
        else
          2 pick over r> <fat32-dir> class-size [: ( c-addr' u' index c-addr'' u'' dir dir' )
            dup >r swap ( c-addr' u' index c-addr'' u'' dir' dir )
            2over 2 pick exists? if ( c-addr' u' index c-addr'' u'' dir' dir )
              open-dir ( c-addr' u' index )
              1+ tuck - -rot + swap r> path-exists? ( exists? )
            else
              rdrop 2drop 2drop drop false ( exists? )
            then
          ;] with-aligned-allot
        then
      else ( c-addr' u' index )
        drop 1- swap 1+ swap r> path-exists? ( exists? )
      then ( exists? )
    ; define path-exists?

    :noname ( entry dir -- entry-read? )
      >r begin
        r@ dir-offset @ r@ dir-current-cluster-index @ 1+ 32 * < if
          dup r@ dir-offset @ 32 umod r@ dir-current-cluster @ r@ dir-fs @ entry@
          dup entry-end? if
            drop false true
          else
            1 r@ dir-offset +!
            dup entry-deleted? not if
              dup entry-file? over entry-dir? or if
                drop true true
              else
                false
              then
            else
              false
            then
          then
        else
          r@ dir-current-cluster @ 0 r@ dir-fs @ fat@ dup link-cluster? if
            cluster-link r@ dir-current-cluster !
            1 r@ dir-current-cluster-index +!
            false
          else
            2drop false true
          then
        then
      until
      rdrop
    ; define read-dir 
    
    :noname ( c-addr u new-file dir -- )
      3 pick 3 pick validate-file-name
      dup dir-fs @ 2 pick <fat32-file> swap init-object
      swap do-create-file
    ; define create-file
    
    :noname ( c-addr u opened-file dir -- )
      3 pick 3 pick validate-file-name
      dup dir-fs @ 2 pick <fat32-file> swap init-object
      swap do-open-file
    ; define open-file
        
    :noname ( c-addr u dir -- )
      dup dir-start-cluster @ swap dir-fs @ dup >r lookup-entry r> -rot ( fs entry-index entry-cluster )
      <fat32-entry> [: ( fs entry-index entry-cluster entry )
        dup 3 pick 3 pick 6 pick entry@ ( fs entry-index entry-cluster entry )
        dup entry-file? averts x-entry-not-file ( fs entry-index entry-cluster entry )
        dup first-cluster@ 4 pick free-cluster-chain ( fs entry-index entry-cluster entry )
        dup mark-entry-deleted ( fs entry-index entry-cluster entry )
        rot rot 3 roll entry! ( )
      ;] with-object
    ; define remove-file
    
    :noname ( c-addr u new-dir dir -- )
      3 pick 3 pick validate-dir-name
      dup dir-fs @ 2 pick <fat32-dir> swap init-object
      swap do-create-dir
    ; define create-dir
        
    :noname ( c-addr u opened-dir dir -- )
      3 pick 3 pick validate-dir-name
      dup dir-fs @ 2 pick <fat32-dir> swap init-object
      swap do-open-dir
    ; define open-dir
        
    :noname ( c-addr u dir -- )
      3dup <fat32-dir> class-size [: ( c-addr u dir c-addr u dir dir' )
        dup >r swap open-dir r> ( c-addr u dir dir' )
        dir-empty? averts x-dir-is-not-empty
      ;] with-aligned-allot
      dup dir-start-cluster @ swap dir-fs @ dup >r lookup-entry r> -rot ( fs entry-index entry-cluster )
      <fat32-entry> [: ( fs entry-index entry-cluster entry )
        dup 3 pick 3 pick 6 pick entry@ ( fs entry-index entry-cluster entry )
        dup entry-dir? averts x-entry-not-dir ( fs entry-index entry-cluster entry )
        dup first-cluster@ 4 pick free-cluster-chain ( fs entry-index entry-cluster entry )
        dup mark-entry-deleted ( fs entry-index entry-cluster entry )
        rot rot 3 roll entry! ( )
      ;] with-object
    ; define remove-dir
    
    :noname ( c-addr' u' c-addr u dir - )
      dup dir-start-cluster @ swap dir-fs @ dup >r lookup-entry r> -rot ( c-addr' u' fs entry-index entry-cluster )
      <fat32-entry> [: ( c-addr' u' fs entry-index entry-cluster entry )
        dup 3 pick 3 pick 6 pick entry@ ( c-addr' u' fs entry-index entry-cluster entry )
        dup entry-dir? if ( c-addr' u' fs entry-index entry-cluster entry )
          5 pick 5 pick forbidden-dir? triggers x-forbidden-dir
          12 [: 12 2 pick file-name@ forbidden-dir? triggers x-forbidden-dir ;] with-allot
          5 roll 5 roll 2 pick dir-name!
        else
          5 roll 5 roll 2 pick file-name!
        then ( fs entry-index entry-cluster entry )
        -rot 3 roll entry! ( )
      ;] with-object
    ; define rename
    
    :noname ( dir -- empty? )
      <fat32-entry> [: ( dir entry )
        swap 0 swap dup dir-start-cluster @ swap dir-fs @ begin ( entry index cluster fs )
          dup >r find-entry r> ( entry index cluster fs )
          over -1 = if 2dup 2dup true exit then ( entry index cluster fs )
          2over 2over entry@ ( entry index cluster fs )
          3 pick short-file-name c@ dup $00 = if drop 2drop 2drop true exit then ( entry index cluster fs c )
          $E5 <> if ( name entry index cluster fs )
            s" .       " 5 pick short-file-name 8 equal-case-strings?
            s" ..      " 6 pick short-file-name 8 equal-case-strings? or
            s"    " 6 pick short-file-ext 3 equal-case-strings? and
            not if 2drop 2drop false exit then
          then
          rot 1+ -rot
        again
      ;] with-object
    ; define dir-empty?

    :noname ( c-addr u dir -- exists? )
      dup dir-start-cluster @ swap dir-fs @ entry-exists?
    ; define exists?

    :noname ( c-addr u dir -- file? )
      <fat32-entry> [: ( c-addr u dir entry )
        2swap 3 pick dir-start-cluster @ 4 pick dir-fs @ lookup-entry
        ( dir entry index cluster )
        2 pick >r 3 roll dir-fs @ entry@ r> ( entry )
        entry-file?
      ;] with-object
    ; define file?
    
    :noname ( c-addr u dir -- dir? )
      <fat32-entry> [: ( c-addr u dir entry )
        2swap 3 pick dir-start-cluster @ 4 pick dir-fs @ lookup-entry
        ( dir entry index cluster )
        2 pick >r 3 roll dir-fs @ entry@ r> ( entry )
        entry-dir?
      ;] with-object
    ; define dir?

    :noname ( c-addr u parent-dir dir -- )
      swap dir-start-cluster @ over dir-fs @ allocate-entry ( c-addr u dir parent-index parent-cluster )
      2 pick dir-parent-cluster ! ( c-addr u dir parent-index )
      over dir-parent-index ! ( c-addr u dir )
      dup dir-fs @ allocate-cluster ( c-addr u dir dir-cluster )
      <fat32-entry> [: ( c-addr u dir dir-cluster entry )
        dup >r init-end-entry r> ( c-addr u dir dir-cluster entry )
        0 2 pick 4 pick dir-fs @ entry! ( c-addr u dir dir-cluster )
      ;] with-object ( c-addr u dir dir-cluster )
      over dir-start-cluster ! ( c-addr u dir )
      <fat32-entry> [: ( c-addr u dir entry )
        over dir-start-cluster @ swap ( c-addr u dir dir-cluster entry )
        4 roll 4 roll rot ( dir dir-cluster c-addr u entry )
        dup >r init-dir-entry r> ( dir entry )
        over dir-parent-index @ ( dir entry parent-index )
        2 pick dir-parent-cluster @ ( dir entry parent-index parent-cluster )
        3 roll dir-fs @ entry! ( )
      ;] with-object
    ; define do-create-dir-raw
    
    :noname ( c-addr u parent-dir dir -- )
      3 pick 3 pick 3 pick dir-start-cluster @ 4 pick dir-fs @ entry-exists?
      triggers x-entry-already-exists
      2dup 2>r do-create-dir-raw 2r> ( parent-dir dir -- )
      dup do-create-dot-entry
      do-create-dot-dot-entry
    ; define do-create-dir
    
    \ Create . directory entry
    :noname ( dir -- )
      dup dir-start-cluster @ over dir-fs @ allocate-entry ( dir child-index child-cluster )
      <fat32-entry> [: ( dir child-index child-cluster entry )
        3 pick dir-start-cluster @ swap ( dir child-index child-cluster start-cluster entry )
        s" ." rot ( dir child-index child-cluster start-cluster c-addr u entry )
        dup >r init-dir-entry r> ( dir child-index child-cluster entry )
        -rot 3 roll dir-fs @ entry! ( )
      ;] with-object
    ; define do-create-dot-entry
    
    \ Create .. directory entry
    :noname ( parent-dir dir -- )
      dup dir-start-cluster @ over dir-fs @ allocate-entry ( parent-dir dir child-index child-cluster )
      <fat32-entry> [: ( parent-dir dir child-index child-cluster entry )
        4 roll dir-start-cluster @ swap ( dir child-index child-cluster start-cluster entry )
        s" .." rot ( dir child-index child-cluster start-cluster c-addr u entry )
        dup >r init-dir-entry r> ( dir child-index child-cluster entry )
        -rot 3 roll dir-fs @ entry! ( )
      ;] with-object
    ; define do-create-dot-dot-entry
    
    :noname ( c-addr u parent-dir dir -- )
      swap 2swap rot dir-start-cluster @ 3 pick dir-fs @ lookup-entry ( dir entry-index entry-cluster )
      2 pick dir-parent-cluster ! ( dir entry-index )
      2dup swap dir-parent-index ! ( dir entry-index )
      <fat32-entry> [: ( dir entry-index entry )
        dup >r swap 2 pick dir-parent-cluster @ 3 pick dir-fs @ ( dir entry entry-index entry-cluster fs )
        entry@
        r@ entry-dir? averts x-entry-not-dir
        r> first-cluster@ swap 2dup dir-start-cluster ! dir-current-cluster ! ( )
      ;] with-object
    ; define do-open-dir
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
      2dup short-file-ext swap 8 + 3 move
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
    
    :noname ( entry -- )
      >r
      $E5 r@ short-file-name  c!
      r@ short-file-name 1+ 7 $20 fill
      r@ short-file-ext 3 $20 fill
      0 r@ file-attributes c!
      0 r@ first-cluster!
      0 r@ entry-file-size !
      0 r@ nt-vfat-case c!
      0 r@ create-time-fine c!
      0 r@ create-time-coarse h!
      [ 0 9 lshift 1 5 lshift or 1 0 lshift or ] literal r@ create-date h!
      [ 0 9 lshift 1 5 lshift or 1 0 lshift or ] literal r@ access-date h!
      0 r@ modify-time-coarse h!
      [ 0 9 lshift 1 5 lshift or 1 0 lshift or ] literal r> modify-date h!
    ; define init-blank-entry
    
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
    
    :noname ( entry -- cluster )
      dup first-cluster-low h@ swap first-cluster-high h@ 16 lshift or
    ; define first-cluster@
    
    :noname ( cluster entry -- )
      2dup swap $FFFF and swap first-cluster-low h!
      swap 16 rshift swap first-cluster-high h!
    ; define first-cluster!
    
    :noname ( c-addr u entry -- )
      -rot 2dup validate-file-name ( c-addr u entry )
      [: ( entry c-addr' u' )
        rot >r ( c-addr' u' )
        r@ short-file-name 8 $20 fill ( c-addr' u' )
        r@ short-file-ext 3 $20 fill ( c-addr' u' )
        2dup dot-index ( c-addr' u' index )
        2 pick r@ short-file-name 2 pick move ( c-addr' u' index )
        1+ rot over + -rot r@ short-file-ext -rot - move ( )
        r@ short-file-name c@ $E5 = if
          $05 r@ short-file-name c!
        then
        rdrop
      ;] upcase-string
    ; define file-name!
    
    :noname ( c-addr u entry -- )
      -rot 2dup validate-dir-name ( c-addr u entry )
      [: ( entry c-addr' u' )
        rot >r ( c-addr' u' )
        r@ short-file-name 8 $20 fill ( c-addr' u' )
        r@ short-file-ext 3 $20 fill ( c-addr' u' )
        r@ short-file-name swap move ( )
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

end-module

: init ( -- ) init fat32::fat32-internal::init-fat32 ;

reboot

