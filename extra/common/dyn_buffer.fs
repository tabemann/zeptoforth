\ Copyright (c) 2023 Travis Bemann
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
  40 constant default-segment-size

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

      \ The dynamic buffer length
      method dyn-buffer-len@ ( dyn-buffer -- len )

      \ Adjust the dynamic buffer length
      method adjust-dyn-buffer-len ( change dyn-buffer )
      
      \ Request a segment
      method request-segment ( bytes dyn-buffer -- segment )

      \ Request a segment after another segment
      method request-segment-after ( bytes prev-segment dyn-buffer -- segment )
      
      \ Replace a segment
      method replace-segment ( new-segment old-segment dyn-buffer -- )

      \ Delete a segment
      method delete-segment ( segment dyn-buffer -- )

      \ Execute xt for all other cursors
      method for-all-other-cursors ( xt cursor dyn-buffer -- )

      \ Resolve invalidated cursors
      method resolve-cursors ( dyn-buffer -- )
      
    end-module

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

    \ Find previous
    method find-prev ( xt cursor -- ) ( xt: c -- match? )

    \ Find next
    method find-next ( xt cursor -- ) ( xt: c -- match? )
    
  end-class

  \ Dynamic buffer implementation
  <dyn-buffer> begin-implement
    
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
      bytes segment-header-size + dyn-buffer dyn-buffer-heap @ allocate
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
      bytes segment-header-size + dyn-buffer dyn-buffer-heap @ allocate
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

    \ Replace a segment
    :noname { new-segment old-segment dyn-buffer -- }
      old-segment segment-prev @ { prev-segment }
      old-segment segment-next @ { next-segment }
      prev-segment new-segment segment-prev !
      prev-segment 0= if
        new-segment dyn-buffer dyn-buffer-first !
      then
      next-segment segment-next @ new-segment segment-next !
      next-segment 0= if
        new-segment dyn-buffer dyn-buffer-last !
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
    ; define new

    \ Destroy a cursor
    :noname { cursor -- }
      cursor cursor-prev @ ?dup if
        cursor cursor-next @ swap cursor-next !
      else
        cursor cursor-next @ cursor dyn-buffer @ dyn-buffer-first-cursor !
      then
      cursor cursor-next @ ?dup if
        cursor cursor-prev @ swap cursor-prev !
      else
        cursor cursor-prev @ cursor dyn-buffer @ dyn-buffer-last-cursor !
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
      cursor dyn-buffer @ dyn-buffer-first @ cursor cursor-segment !
      0 cursor cursor-offset !
      0 cursor cursor-global-offset !
    ; define go-to-start

    \ Move to the end
    :noname { cursor -- }
      cursor dyn-buffer @ dyn-buffer-last @ cursor cursor-segment !
      cursor cursor-segment @ segment-size @ cursor cursor-offset !
      cursor dyn-buffer @ dyn-buffer-len@ cursor cursor-global-offset !
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
      cursor cursor-segment @ 0= if exit then
      begin
        cursor cursor-segment @ { segment }
        change 0< if
          cursor cursor-offset @ change + 0>= if
            change cursor cursor-offset +!
            true
          else
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
            cursor cursor-offset @ change + segment segment-size @ < if
              change cursor cursor-offset +!
              true
            else
              cursor cursor-offset @ segment segment-size @ + negate +to change
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

    \ Insert data; note that this invalidates all other cursors for the dynamic
    \ buffer
    :noname { addr bytes cursor -- }
      cursor [: over { current-cursor orig-cursor }
        current-cursor cursor-global-offset @
        orig-cursor cursor-global-offset @ >
        current-cursor cursor-segment @ orig-cursor cursor-segment @ = or
        current-cursor cursor-invalid @ or current-cursor cursor-invalid !
      ;] cursor dup cursor-dyn-buffer @ for-all-other-cursors
      drop
      begin bytes 0> while
        cursor cursor-dyn-buffer @ { dyn-buffer }
        cursor cursor-segment @ { old-segment }
        old-segment if
          old-segment segment-size @ default-segment-size < if
            default-segment-size old-segment segment-size @ bytes +
            min { part-size }
            old-segment segment-size @ part-size +
            dyn-buffer request-segment { new-segment }
            cursor cursor-offset @ { offset }
            old-segment segment-header-size + { old-addr }
            new-segment segment-header-size + { new-addr }
            old-addr new-addr offset move
            addr new-addr offset + part-size move
            old-addr offset + new-addr offset + part-size +
            old-segment segment-size @ offset - move
            new-segment old-segment dyn-buffer replace-segment
            new-segment cursor cursor-segment !
            offset part-size + cursor cursor-offset !
            part-size +to addr
            part-size negate +to bytes
            part-size dyn-buffer adjust-dyn-buffer-len
          else
            default-segment-size bytes min { part-size }
            old-segment part-size dyn-buffer
            request-segment-after { new-segment }
            new-segment segment-header-size + { new-addr }
            addr new-addr part-size move
            part-size +to addr
            part-size negate +to bytes
            part-size dyn-buffer adjust-dyn-buffer-len
          then
        else
          bytes default-segment-size min { part-size }
          part-size dyn-buffer request-segment { new-segment }
          new-segment segment-header-size + { new-addr }
          addr new-addr part-size move
          part-size +to addr
          part-size negate +to bytes
          part-size dyn-buffer adjust-dyn-buffer-len
        then
      repeat
      cursor cursor-dyn-buffer @ resolve-cursors
    ; define insert-data

    \ Delete data; note that this invalidates all other cursors for the dynamic
    \ buffer
    :noname { bytes cursor -- }
      cursor cursor-dyn-buffer @ { dyn-buffer }
      begin
        cursor cursor-segment @ { segment }
        cursor [: over { current-cursor orig-cursor }
          current-cursor cursor-global-offset @
          orig-cursor cursor-global-offset @ >
          current-cursor cursor-segment @ orig-cursor cursor-segment @ = or
          current-cursor cursor-segment @
          orig-cursor cursor-segment @ segment-prev @ = or
          current-cursor cursor-invalid @ or current-cursor cursor-invalid !
        ;] cursor dup cursor-dyn-buffer @ for-all-other-cursors
        drop
        segment 0= if exit then
        bytes cursor cursor-offset @ >
        cursor cursor-segment @ segment-prev @ 0<> and if
          segment segment-prev @ { old-segment }
          segment segment-size @ cursor cursor-offset @ - { orig-bytes }
          old-segment segment-size @ orig-bytes +
          dyn-buffer request-segment { new-segment }
          old-segment segment-header-size + { old-addr }
          segment segment-header-size + { orig-addr }
          new-segment segment-header-size + { new-addr }
          old-segment segment-size @ { old-bytes }
          old-addr new-addr old-bytes move
          orig-addr cursor cursor-offset @ + new-addr old-bytes +
          orig-bytes move
          new-segment old-segment dyn-buffer replace-segment
          segment dyn-buffer delete-segment
          cursor cursor-offset @ negate +to bytes
          cursor cursor-offset @ negate dyn-buffer adjust-dyn-buffer-len
          new-segment cursor cursor-segment !
          old-bytes orig-bytes + cursor cursor-offset !
          false
        else
          cursor cursor-offset @ segment segment-size @ <
          bytes cursor cursor-offset @ <= xor if
            segment segment-size @ bytes - { new-bytes }
            new-bytes dyn-buffer request-segment { new-segment }
            segment segment-header-size + { old-addr }
            new-segment segment-header-size + { new-addr }
            cursor cursor-offset @ bytes - { first-bytes }
            segment segment-size @ cursor cursor-offset @ - { last-bytes }
            bytes negate dyn-buffer adjust-dyn-buffer-len
            old-addr new-addr first-bytes move
            old-addr cursor cursor-offset @ + new-addr first-bytes + last-bytes
            move
            new-segment segment dyn-buffer replace-segment
            new-segment cursor cursor-segment !
            first-bytes cursor cursor-offset !
            true
          else
            segment segment-prev @ ?dup if
              dup cursor cursor-segment !
              segment-size @ cursor cursor-offset !
              segment segment-size @ negate +to bytes
            else
              segment segment-next @ ?dup if
                cursor cursor-segment !
              else
                0 cursor cursor-segment !
              then
              segment segment-size @ negate dyn-buffer adjust-dyn-buffer-len
              segment dyn-buffer delete-segment
              0 cursor cursor-offset !
              0 to bytes
            then
            bytes 0>
          then
        then
      until
      cursor cursor-dyn-buffer @ resolve-cursors
    ; define delete-data

    \ Read data
    :noname { addr bytes cursor -- bytes' }
      cursor cursor-segment @ 0= if
        0 exit
      then
      0 { total-bytes }
      cursor cursor-segment @ { segment }
      cursor cursor-offset @ { offset }
      begin total-bytes bytes < segment 0<> and while
        segment segment-header-size + offset + { segment-addr }
        segment segment-size @ offset - { remaining }
        total-bytes bytes - remaining < if
          segment-addr addr remaining move
          remaining +to addr
          remaining +to total-bytes
          segment segment-next @ to segment
          0 to offset
        else
          segment-addr addr total-bytes bytes - move
          total-bytes
          exit
        then
      repeat
      total-bytes
    ; define read-data
    
    \ Find previous
    :noname { xt cursor -- } ( xt: c -- match? )
      0 { W^ buffer }
      begin cursor offset@ 0> while
        -1 cursor adjust-offset
        buffer 1 cursor read-data 0> if
          buffer c@ xt execute if
            1 cursor adjust-offset
            exit
          then
        else
          exit
        then
      repeat
    ; define find-prev
    
    \ Find next
    :noname { xt cursor -- } ( xt: c -- match? )
      0 { W^ buffer }
      begin cursor offset@ cursor cursor-dyn-buffer @ dyn-buffer-len @ < while
        buffer 1 cursor read-data 0> if
          buffer c@ xt execute if
            exit
          else
            1 cursor adjust-offset
          then
        else
          exit
        then
      repeat
    ; define find-next

  end-implement
  
end-module