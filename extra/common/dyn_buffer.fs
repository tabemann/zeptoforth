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

begin-module dyn-buffer

  oo import
  heap import
  
  \ Default segment size
  256 constant default-segment-size

  \ Find search distance
  128 constant find-search-distance

  begin-module dyn-buffer-internal
    
    \ Dynamic buffer segment structure
    begin-structure segment-header-size

      \ Dynamic buffer segment size
      field: segment-size

      \ Previous segment
      field: segment-prev

      \ Next segment
      field: segment-next
      
    end-structure
    
  end-module> import
  
  \ Dynamic buffer class
  <object> begin-class <dyn-buffer>
    
    continue-module dyn-buffer-internal

      \ Dynamic buffer heap
      cell member dyn-buffer-heap

      \ The first segment
      cell member dyn-buffer-first

      \ The last segment
      cell member dyn-buffer-last

      \ The dynamic buffer length
      cell member dyn-buffer-len

      \ The first cursor
      cell member dyn-buffer-first-cursor

      \ The last cursor
      cell member dyn-buffer-last-cursor

      \ Adjust the dynamic buffer length
      method adjust-dyn-buffer-len ( change dyn-buffer )
      
      \ Request a segment
      method request-segment ( bytes dyn-buffer -- segment )

      \ Request a segment after another segment
      method request-segment-after ( bytes prev-segment dyn-buffer -- segment )

      \ Request a segment before another segment
      method request-segment-before ( bytes next-segment dyn-buffer -- segment )

      \ Insert a segment
      method insert-segment ( prev-segment next-segment segment dyn-buffer -- )
      
      \ Replace a segment
      method replace-segment ( new-segment old-segment dyn-buffer -- )

      \ Delete a segment
      method delete-segment ( segment dyn-buffer -- )

      \ Execute xt for all other cursors
      method for-all-other-cursors ( xt cursor dyn-buffer -- )

      \ Resolve invalidated cursors
      method resolve-cursors ( dyn-buffer -- )
      
    end-module

    \ The dynamic buffer length
    method dyn-buffer-len@ ( dyn-buffer -- len )

  end-class
  
  \ Dynamic buffer cursor class
  <object> begin-class <cursor>

    continue-module dyn-buffer-internal
      
      \ The corresponding dynamic buffer
      cell member cursor-dyn-buffer

      \ The corresponding segment
      cell member cursor-segment

      \ The corresponding offset
      cell member cursor-offset

      \ The global offset
      cell member cursor-global-offset

      \ The previous cursor
      cell member cursor-prev

      \ The next cursor
      cell member cursor-next

      \ The cursor has been invalidated
      cell member cursor-invalid
      
      \ Initial insert
      method insert-data-initial ( addr bytes cursor -- addr' bytes' )
      
      \ Mark cursors after cursor invalid
      method mark-cursors-invalid-on-insert ( cursor -- )

      \ Insert data into segment
      method insert-data-into-segment ( addr bytes cursor -- addr' bytes' )
      
      \ Insert data after segment
      method insert-data-after-segment ( addr bytes cursor -- addr' bytes' )
      
      \ Insert data before segment
      method insert-data-before-segment ( addr bytes cursor -- addr' bytes' )
      
      \ Split segment
      method split-segment ( cursor -- )
      
      \ Invalidate sursors for deletion
      method mark-cursors-invalid-on-delete ( cursor -- )

      \ Delete data within a segment
      method delete-data-within-segment ( bytes cursor -- bytes' )

      \ Delete a whole segment
      method delete-whole-segment ( bytes cursor -- bytes' )
      
    end-module

    \ Copy a cursor
    method copy-cursor ( src-cursor dest-cursor -- )

    \ Move to the start
    method go-to-start ( cursor -- )

    \ Move to the end
    method go-to-end ( cursor -- )

    \ Go to an offset
    method go-to-offset ( offset cursor -- )
    
    \ Get the offset as a numerical value
    method offset@ ( cursor -- offset )

    \ Change the offset of the cursor
    method adjust-offset ( change cursor -- )

    \ Insert data; note that this invalidates all other cursors for the dynamic
    \ buffer
    method insert-data ( addr bytes cursor -- )

    \ Delete data; note that this invalidates all other cursors for the dynamic
    \ buffer
    method delete-data ( bytes cursor -- )

    \ Read data
    method read-data ( addr bytes cursor -- bytes' )

    \ Read data without moving the cursor
    method read-data-w/o-move ( addr bytes cursor -- bytes' )

    \ Read data before the cursor without moving the cursor
    method read-data-before-w/o-move ( addr bytes cursor -- addr' bytes' )

    \ Find previous
    method find-prev ( xt cursor -- ) ( xt: c -- match? )

    \ Find next
    method find-next ( xt cursor -- ) ( xt: c -- match? )
    
  end-class

  \ Dynamic buffer implementation
  <dyn-buffer> begin-implement

    \ Constructor
    :noname { heap dyn-buffer -- }
      dyn-buffer <object>->new
      heap dyn-buffer dyn-buffer-heap !
      0 dyn-buffer dyn-buffer-first !
      0 dyn-buffer dyn-buffer-last !
      0 dyn-buffer dyn-buffer-len !
      0 dyn-buffer dyn-buffer-first-cursor !
      0 dyn-buffer dyn-buffer-last-cursor !
    ; define new

    \ Destructor
    :noname { dyn-buffer -- }
      dyn-buffer dyn-buffer-first @ { current-segment }
      begin current-segment while
        current-segment segment-next @ { next-segment }
        current-segment dyn-buffer dyn-buffer-heap @ free
        next-segment to current-segment
      repeat
    ; define destroy
    
    \ The dynamic buffer length
    :noname ( dyn-buffer -- len )
      dyn-buffer-len @
    ; define dyn-buffer-len@

    \ Adjust the dynamic buffer length
    :noname ( change dyn-buffer )
      dyn-buffer-len +!
    ; define adjust-dyn-buffer-len

    \ Request a segment
    :noname { bytes dyn-buffer -- segment }
      bytes segment-header-size + cell align
      dyn-buffer dyn-buffer-heap @ allocate
      bytes over segment-size !
      0 over segment-prev !
      0 over segment-next !
      dyn-buffer dyn-buffer-first @ 0= if
        dup dyn-buffer dyn-buffer-first !
        dup dyn-buffer dyn-buffer-last !
      then
    ; define request-segment
    
    \ Request a segment after another segment
    :noname { bytes prev-segment dyn-buffer -- segment }
      bytes segment-header-size + cell align
      dyn-buffer dyn-buffer-heap @ allocate
      bytes over segment-size !
      prev-segment over segment-prev !
      prev-segment segment-next @ over segment-next !
      dup segment-next @ if
        dup dup segment-next @ segment-prev !
      else
        dup dyn-buffer dyn-buffer-last !
      then
      dup prev-segment segment-next !
    ; define request-segment-after

    \ Request a segment before another segment
    :noname { bytes next-segment dyn-buffer -- segment }
      bytes segment-header-size + cell align
      dyn-buffer dyn-buffer-heap @ allocate
      bytes over segment-size !
      next-segment over segment-next !
      next-segment segment-prev @ over segment-prev !
      dup segment-prev @ if
        dup dup segment-prev @ segment-next !
      else
        dup dyn-buffer dyn-buffer-first !
      then
      dup next-segment segment-prev !
    ; define request-segment-before

    \ Insert a segment
    :noname { prev-segment next-segment segment dyn-buffer -- }
      prev-segment segment segment-prev !
      prev-segment 0= if
        segment dyn-buffer dyn-buffer-first !
      else
        segment prev-segment segment-next !
      then
      next-segment segment segment-next !
      next-segment 0= if
        segment dyn-buffer dyn-buffer-last !
      else
        segment next-segment segment-prev !
      then
    ; define insert-segment
    
    \ Replace a segment
    :noname { new-segment old-segment dyn-buffer -- }
      old-segment segment-prev @ { prev-segment }
      old-segment segment-next @ { next-segment }
      prev-segment new-segment segment-prev !
      prev-segment 0= if
        new-segment dyn-buffer dyn-buffer-first !
      else
        new-segment prev-segment segment-next !
      then
      next-segment new-segment segment-next !
      next-segment 0= if
        new-segment dyn-buffer dyn-buffer-last !
      else
        new-segment next-segment segment-prev !
      then
      old-segment dyn-buffer dyn-buffer-heap @ free
    ; define replace-segment

    \ Delete a segment
    :noname { segment dyn-buffer -- }
      segment segment-prev @ { prev-segment }
      segment segment-next @ { next-segment }
      prev-segment if
        next-segment prev-segment segment-next !
      else
        next-segment dyn-buffer dyn-buffer-first !
      then
      next-segment if
        prev-segment next-segment segment-prev !
      else
        prev-segment dyn-buffer dyn-buffer-last !
      then
      segment dyn-buffer dyn-buffer-heap @ free
    ; define delete-segment
    
    \ Execute xt for all other cursors
    :noname { xt cursor dyn-buffer -- } ( xt: cursor -- )
      dyn-buffer dyn-buffer-first-cursor @ { current-cursor }
      begin current-cursor while
        current-cursor cursor <> if
          current-cursor xt execute
        then
        current-cursor cursor-next @ to current-cursor
      repeat
    ; define for-all-other-cursors

    \ Resolve invalidated cursors
    :noname { dyn-buffer -- }
      dyn-buffer dyn-buffer-first-cursor @ { current-cursor }
      begin current-cursor while
        current-cursor cursor-invalid @ if
          current-cursor offset@
          current-cursor go-to-start
          current-cursor go-to-offset
          false current-cursor cursor-invalid !
        then
        current-cursor cursor-next @ to current-cursor
      repeat
    ; define resolve-cursors
    
  end-implement
  
  \ Dynamic buffer cursor implementation
  <cursor> begin-implement
    
    \ The constructor for a cursor in a given dynamic buffer
    :noname { dyn-buffer cursor -- }
      cursor <object>->new
      dyn-buffer cursor cursor-dyn-buffer !
      dyn-buffer dyn-buffer-first @ cursor cursor-segment !
      0 cursor cursor-offset !
      0 cursor cursor-global-offset !
      dyn-buffer dyn-buffer-last-cursor @
      dup cursor cursor-prev !
      ?dup if
        cursor swap cursor-next !
      else
        cursor dyn-buffer dyn-buffer-first-cursor !
      then
      0 cursor cursor-next !
      cursor dyn-buffer dyn-buffer-last-cursor !
      false cursor cursor-invalid !
    ; define new

    \ Destroy a cursor
    :noname { cursor -- }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      cursor cursor-prev @ ?dup if
        cursor cursor-next @ swap cursor-next !
      else
        cursor cursor-next @ dyn-buffer dyn-buffer-first-cursor !
      then
      cursor cursor-next @ ?dup if
        cursor cursor-prev @ swap cursor-prev !
      else
        cursor cursor-prev @ dyn-buffer dyn-buffer-last-cursor !
      then
      cursor <object>->destroy
    ; define destroy

    \ Copy a cursor
    :noname { src-cursor dest-cursor -- }
      src-cursor cursor-dyn-buffer @ dest-cursor cursor-dyn-buffer !
      src-cursor cursor-segment @ dest-cursor cursor-segment !
      src-cursor cursor-offset @ dest-cursor cursor-offset !
      src-cursor cursor-global-offset @ dest-cursor cursor-global-offset !
    ; define copy-cursor

    \ Move to the start
    :noname { cursor -- }
      cursor cursor-dyn-buffer @ dyn-buffer-first @ cursor cursor-segment !
      0 cursor cursor-offset !
      0 cursor cursor-global-offset !
    ; define go-to-start

    \ Move to the end
    :noname { cursor -- }
      cursor cursor-dyn-buffer @ dyn-buffer-last @ cursor cursor-segment !
      cursor cursor-segment @ ?dup if segment-size @ else 0 then
      cursor cursor-offset !
      cursor cursor-dyn-buffer @ dyn-buffer-len@ cursor cursor-global-offset !
    ; define go-to-end

    \ Go to an offset
    :noname { offset cursor -- }
      offset cursor cursor-global-offset @ - cursor adjust-offset
    ; define go-to-offset
    
    \ Get the offset as a numerical value
    :noname ( cursor -- offset )
      cursor-global-offset @
    ; define offset@

    \ Change the offset of the cursor
    :noname { change cursor -- }
      cursor cursor-dyn-buffer @ dyn-buffer-first @ 0= if
        0 cursor cursor-segment !
        0 cursor cursor-offset !
        0 cursor cursor-global-offset !
        exit
      then
      cursor cursor-segment @ 0= if cursor go-to-start then
      begin
        cursor cursor-segment @ { segment }
        change 0< if
          cursor cursor-offset @ change + 0>= if
            change cursor cursor-offset +!
            change cursor cursor-global-offset +!
            true
          else
            cursor cursor-offset @ negate cursor cursor-global-offset +!
            cursor cursor-offset @ +to change
            segment segment-prev @ ?dup if
              dup cursor cursor-segment !
              segment-size @ cursor cursor-offset !
              false
            else
              0 cursor cursor-offset !
              true
            then
          then
        else
          change 0> if
            cursor cursor-offset @ change + segment segment-size @ <= if
              change cursor cursor-global-offset +!
              change cursor cursor-offset +!
              true
            else
              segment segment-size @ cursor cursor-offset @ - { diff }
              diff cursor cursor-global-offset +!
              diff negate +to change
              segment segment-next @ ?dup if
                cursor cursor-segment !
                0 cursor cursor-offset !
                false
              else
                segment segment-size @ cursor cursor-offset !
                true
              then
            then
          else
            true
          then
        then
      until
    ; define adjust-offset

    \ Initial insert
    :noname { addr bytes cursor -- addr' bytes' }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      dyn-buffer dyn-buffer-first @ 0= if
        bytes default-segment-size min { part-size }
        part-size dyn-buffer request-segment { segment }
        addr segment segment-header-size + part-size move
        segment [: { cursor }
          dup cursor cursor-segment !
          true cursor cursor-invalid !
        ;] cursor dyn-buffer for-all-other-cursors
        drop
        segment cursor cursor-segment !
        part-size cursor cursor-offset +!
        part-size cursor cursor-global-offset +!
        part-size negate +to bytes
        part-size +to addr
        part-size dyn-buffer adjust-dyn-buffer-len
      then
      addr bytes
    ; define insert-data-initial

    \ Mark cursors after cursor invalid
    :noname { cursor -- }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      cursor [: over { current-cursor orig-cursor }
        current-cursor cursor-global-offset @
        orig-cursor cursor-global-offset @ >=
        current-cursor cursor-segment @ orig-cursor cursor-segment @ = or
        current-cursor cursor-invalid @ or current-cursor cursor-invalid !
      ;] cursor dyn-buffer for-all-other-cursors
      drop
    ; define mark-cursors-invalid-on-insert

    \ Insert data into segment
    :noname { addr bytes cursor -- addr' bytes' }
      default-segment-size cursor cursor-segment @ segment-size @ bytes + min
      { total-size }
      addr bytes cursor total-size dup [:
        { addr bytes cursor total-size temp-buffer }
        cursor cursor-dyn-buffer @ { dyn-buffer }
        cursor cursor-segment @ { old-segment }
        cursor cursor-offset @ { offset }
        old-segment segment-size @ { old-size }
        total-size old-size - { part-size }
        old-segment segment-prev @ { prev-segment }
        old-segment segment-next @ { next-segment }
        old-segment segment-header-size + { old-addr }
        old-addr temp-buffer offset move
        addr temp-buffer offset + part-size move
        old-addr offset + temp-buffer offset + part-size +
        old-size offset - move
        old-segment dyn-buffer delete-segment
        old-size part-size + dyn-buffer request-segment { new-segment }
        new-segment segment-header-size + { new-addr }
        temp-buffer new-addr old-size part-size +  move
        prev-segment next-segment new-segment dyn-buffer insert-segment
        new-segment cursor cursor-segment !
        part-size cursor cursor-offset +!
        part-size cursor cursor-global-offset +!
        part-size +to addr
        part-size negate +to bytes
        part-size dyn-buffer adjust-dyn-buffer-len
        addr bytes
      ;] with-allot
    ; define insert-data-into-segment

    \ Insert data after segment
    :noname { addr bytes cursor -- addr' bytes' }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      cursor cursor-segment @ { old-segment }
      cursor cursor-offset @ { offset }
      old-segment segment-size @ { old-size }
      default-segment-size bytes min { part-size }
      part-size old-segment dyn-buffer
      request-segment-after { new-segment }
      new-segment segment-header-size + { new-addr }
      addr new-addr part-size move
      new-segment cursor cursor-segment !
      part-size cursor cursor-offset !
      part-size cursor cursor-global-offset +!
      part-size +to addr
      part-size negate +to bytes
      part-size dyn-buffer adjust-dyn-buffer-len
      addr bytes
    ; define insert-data-after-segment

    \ Insert data before segment
    :noname { addr bytes cursor -- addr' bytes' }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      cursor cursor-segment @ { old-segment }
      cursor cursor-offset @ { offset }
      old-segment segment-size @ { old-size }
      default-segment-size bytes min { part-size }
      part-size old-segment dyn-buffer
      request-segment-before { new-segment }
      new-segment segment-header-size + { new-addr }
      addr new-addr part-size move
      part-size cursor cursor-global-offset +!
      part-size +to addr
      part-size negate +to bytes
      part-size dyn-buffer adjust-dyn-buffer-len
      addr bytes
    ; define insert-data-before-segment

    \ Split segment
    :noname { cursor -- }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      cursor cursor-segment @ { old-segment }
      cursor cursor-offset @ { offset }
      old-segment segment-size @ { old-size }
      offset dyn-buffer request-segment { first-segment }
      old-segment segment-header-size + { old-addr }
      first-segment segment-header-size + { first-addr }
      old-addr first-addr offset move
      old-size offset - { remainder }
      remainder old-segment dyn-buffer request-segment-after { second-segment }
      second-segment segment-header-size + { second-addr }
      old-addr offset + second-addr remainder move
      first-segment old-segment dyn-buffer replace-segment
      first-segment cursor cursor-segment !
    ; define split-segment
    
    \ Insert data; note that this invalidates all other cursors for the dynamic
    \ buffer
    :noname { addr bytes cursor -- }
      bytes 0= if exit then
      addr bytes cursor insert-data-initial to bytes to addr
      cursor mark-cursors-invalid-on-insert
      begin bytes 0> while
        cursor cursor-segment @ { old-segment }
        cursor cursor-offset @ { offset }
        old-segment segment-size @ { old-size }
        old-size bytes + default-segment-size <= if
          addr bytes cursor insert-data-into-segment to bytes to addr
        else
          offset old-size = if
            addr bytes cursor insert-data-after-segment to bytes to addr
          else
            offset 0= if
              addr bytes cursor insert-data-before-segment to bytes to addr
            else
              cursor split-segment
            then
          then
        then
      repeat
      cursor cursor-dyn-buffer @ resolve-cursors
    ; define insert-data

    \ Invalidate cursors for deletion
    :noname { cursor -- }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      cursor [: over { current-cursor orig-cursor }
        orig-cursor cursor-segment @ { current-segment }
        current-cursor cursor-global-offset @
        orig-cursor cursor-global-offset @ >=
        current-segment orig-cursor cursor-segment @ = or
        current-segment orig-cursor cursor-segment @ segment-prev @ = or
        current-cursor cursor-invalid @ or current-cursor cursor-invalid !
      ;] cursor dyn-buffer for-all-other-cursors
      drop
    ; define mark-cursors-invalid-on-delete

    \ Delete data within a segment
    :noname { bytes cursor -- bytes' }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      cursor cursor-segment @ { old-segment }
      cursor cursor-offset @ { offset }
      old-segment segment-size @ { old-size }
      offset bytes min { part-size }
      \ cr ." DELETE-DATA-WITHIN-SEGMENT: "
      \ ." old-segment: " old-segment h.8 space
      \ ." prev-segment: " old-segment segment-prev @ h.8 space
      \ ." next-segment: " old-segment segment-next @ h.8 space
      \ ." bytes: " bytes .
      \ ." offset: " offset .
      \ ." old-size: " old-size .
      \ ." part-size: " part-size .
      \ ." global-offset: " cursor cursor-global-offset @ .
      old-size part-size - dyn-buffer request-segment { new-segment }
      old-segment segment-header-size + { old-addr }
      new-segment segment-header-size + { new-addr }
      old-addr new-addr offset part-size - move
      old-addr offset + new-addr offset + part-size - old-size offset - move
      new-segment old-segment dyn-buffer replace-segment
      part-size negate dyn-buffer adjust-dyn-buffer-len
      new-segment cursor cursor-segment !
      part-size negate cursor cursor-global-offset +!
      part-size negate cursor cursor-offset +!
      \ ." new-segment: " new-segment h.8 space
      \ ." new-prev-segment: " new-segment segment-prev @ h.8 space
      \ ." new-next-segment: " new-segment segment-next @ h.8 space
      \ ." new-global-offset: " cursor cursor-global-offset @ .
      \ ." new-offset: " cursor cursor-offset @ .
      bytes part-size -
      \ ." new-bytes: " dup .
    ; define delete-data-within-segment

    \ Delete a whole segment
    :noname { bytes cursor -- bytes' }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      cursor cursor-segment @ { old-segment }
      old-segment segment-size @ { part-size }
      \ cr ." DELETE-WHOLE-SEGMENT: "
      \ ." old-segment: " old-segment h.8 space
      \ ." prev-segment: " old-segment segment-prev @ h.8 space
      \ ." next-segment: " old-segment segment-next @ h.8 space
      \ ." bytes: " bytes .
      \ ." part-size: " part-size .
      \ ." global-offset: " cursor cursor-global-offset @ .
      old-segment segment-prev @ { prev-segment }
      old-segment segment-next @ { next-segment }
      old-segment dyn-buffer delete-segment
      prev-segment if
        prev-segment cursor cursor-segment !
        prev-segment segment-size @ cursor cursor-offset !
      else
        next-segment if
          next-segment cursor cursor-segment !
        else
          0 cursor cursor-segment !
        then
        0 cursor cursor-offset !
      then
      part-size negate dyn-buffer adjust-dyn-buffer-len
      part-size negate cursor cursor-global-offset +!
      \ ." new-global-offset: " cursor cursor-global-offset @ .
      \ ." new-offset: " cursor cursor-offset @ .
      bytes part-size -
      \ ." new-bytes: " dup .
    ; define delete-whole-segment
    
    \ Delete data; note that this invalidates all other cursors for the dynamic
    \ buffer
    :noname { bytes cursor -- }
      cursor cursor-global-offset @ bytes min to bytes
      bytes 0= if exit then
      cursor cursor-dyn-buffer @ { dyn-buffer }
      begin
        cursor cursor-segment @ { segment }
        segment 0= if
          cursor cursor-dyn-buffer @ resolve-cursors
          exit
        then
        cursor mark-cursors-invalid-on-delete
        bytes 0>
        cursor cursor-offset @ 0<> segment segment-prev @ 0<> or and if
          cursor cursor-offset @ 0= segment segment-prev @ 0<> and if
            segment segment-prev @ cursor cursor-segment !
            segment segment-prev @ segment-size @ cursor cursor-offset !
            false
          else
            cursor cursor-offset @ segment segment-size @ <>
            bytes segment segment-size @ < or if
              bytes cursor delete-data-within-segment to bytes
              false
            else
              bytes cursor delete-whole-segment to bytes
              false
            then
          then
        else
          true
        then
        cursor cursor-dyn-buffer @ resolve-cursors
      until
    ; define delete-data

    \ Read data
    :noname { addr bytes cursor -- bytes' }
      cursor cursor-segment @ { segment }
      segment 0= if 0 exit then
      0 { total-bytes }
      cursor cursor-offset @ { offset }
      begin total-bytes bytes < segment 0<> and while
        segment segment-header-size + offset + { segment-addr }
        segment segment-size @ offset - { remaining }
        bytes total-bytes - remaining >= if
          segment-addr addr remaining move
          remaining +to addr
          remaining +to total-bytes
          segment segment-next @ to segment
          segment if
            segment cursor cursor-segment !
            0 cursor cursor-offset !
          else
            remaining cursor cursor-offset +!
          then
          remaining cursor cursor-global-offset +!
          0 to offset
        else
          segment-addr addr bytes total-bytes - move
          bytes total-bytes - cursor cursor-offset +!
          bytes total-bytes - cursor cursor-global-offset +!
          bytes
          exit
        then
      repeat
      total-bytes
    ; define read-data

    \ Read data without moving the cursor
    :noname { addr bytes cursor -- bytes' }
      cursor cursor-segment @ { segment }
      segment 0= if 0 exit then
      0 { total-bytes }
      cursor cursor-offset @ { offset }
      begin total-bytes bytes < segment 0<> and while
        segment segment-header-size + offset + { segment-addr }
        segment segment-size @ offset - { remaining }
        bytes total-bytes - remaining >= if
          segment-addr addr remaining move
          remaining +to addr
          remaining +to total-bytes
          segment segment-next @ to segment
          0 to offset
        else
          segment-addr addr bytes total-bytes - move
          bytes
          exit
        then
      repeat
      total-bytes
    ; define read-data-w/o-move

    \ Read data before the cursor without moving the cursor
    :noname { addr bytes cursor -- addr' bytes' }
      bytes +to addr
      cursor cursor-segment @ { segment }
      segment 0= if addr 0 exit then
      0 { total-bytes }
      cursor cursor-offset @ { offset }
      begin total-bytes bytes < segment 0<> and while
        bytes total-bytes - offset min { part-bytes }
        part-bytes negate +to addr
        part-bytes +to total-bytes
        segment segment-header-size + offset + part-bytes -
        addr part-bytes move
        part-bytes offset <= if
          addr total-bytes exit
        else
          segment segment-prev @ to segment
          segment if
            segment segment-size @ to offset
          else
            0 to offset
          then
        then
      repeat
      addr total-bytes
    ; define read-data-before-w/o-move
    
    \ Find previous
    :noname ( xt cursor -- ) ( xt: c -- match? )
      find-search-distance [: { xt cursor buffer }
        cursor offset@ { bytes }
        begin bytes while
          buffer find-search-distance cursor read-data-before-w/o-move
          { buffer' count }
          count 0> if
            count 1+ 1 ?do
              buffer' count + i - c@ xt execute if
                i 1- negate cursor adjust-offset unloop exit
              then
            loop
            count negate cursor adjust-offset
            count negate +to bytes
          else
            exit
          then
        repeat
      ;] with-allot
    ; define find-prev
    
    \ Find next
    :noname ( xt cursor -- ) ( xt: c -- match? )
      find-search-distance [: { xt cursor buffer }
        cursor cursor-dyn-buffer @ dyn-buffer-len @ cursor offset@ - { bytes }
        begin bytes while
          buffer find-search-distance cursor read-data-w/o-move { count }
          count 0> if
            count 0 ?do
              buffer i + c@ xt execute if
                i cursor adjust-offset unloop exit
              then
            loop
            count cursor adjust-offset
            count negate +to bytes
          else
            exit
          then
        repeat
      ;] with-allot
    ; define find-next

  end-implement
  
end-module
