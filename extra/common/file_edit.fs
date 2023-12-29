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

begin-module file-edit

  oo import
  dyn-buffer import
  systick import
  heap import
  fat32 import
  ansi-term import

  \ Buffer width
  64 constant buffer-width

  \ Buffer height
  block-size buffer-width / constant buffer-height

  \ Tab size
  2 constant tab-size \ small to save space in the blocks

  \ Heap size
  65536 constant backing-heap-size \ Temporary heap storage size

  \ Display width
  variable display-width

  \ Display height
  variable display-height
  
  \ Buffer class
  <object> begin-class <buffer>

    \ Buffer name
    cell member buffer-name

    \ Buffer path
    cell member buffer-path

    \ Previous buffer
    cell member buffer-prev

    \ Next buffer
    cell member buffer-next

    \ Edit cursor column
    cell member buffer-edit-col
    
    \ Edit cursor row
    cell member buffer-edit-row

    \ Buffer dynamic buffer
    <dyn-buffer> class-size member buffer-dyn-buffer
    
    \ Display area cursor
    <cursor> class-size member buffer-display-cursor

    \ Edit cursor
    <cursor> class-size member buffer-edit-cursor

    \ Select cursor
    <cursor> class-size member buffer-select-cursor

    \ Update the display
    method update-display ( buffer -- )

    \ Refresh the display
    method refresh-display ( buffer -- )
    
    \ Enter a character into the buffer
    method do-char ( c buffer -- )

    \ Backspace in the buffer
    method do-backspace ( buffer -- )

    \ Delete in the buffer
    method do-delete ( buffer -- )

    \ Kill a range in the buffer
    method do-kill ( clip buffer -- )

    \ Copy a range in the buffer
    method do-copy ( clip buffer -- )

    \ Paste in the buffer
    method do-paste ( clip buffer -- )
    
  end-class

  \ Clipboard class
  <object> begin-class <clip>
    
    \ Clipboard dynamic buffer
    <dyn-buffer> class-size member clip-dyn-buffer

    \ Clipboard cursor
    <cursor> class-size member clip-cursor

    \ Copy from a starting cursor to an ending cursor
    method copy-to-clip ( start-cursor end-cursor src-dyn-buffer clip -- )

    \ Insert from the clipboard at a cursor
    method insert-from-clip ( dest-cursor clip -- )
    
  end-class
  
  \ Buffer editor class
  <object> begin-class <editor>
    
    \ Heap
    cell member editor-heap
    
    \ First buffer
    cell member editor-first

    \ Last buffer
    cell member editor-last

    \ Current buffer
    cell member editor-current

    \ Display
    <display> class-size member editor-display
    
    \ Clipboard
    <clip> class-size member editor-clip

    \ Enter a character into the editor
    method handle-char ( c editor -- )

    \ Backspace in the editor
    method handle-backspace ( editor -- )

    \ Delete in the editor
    method handle-delete ( editor -- )

    \ Kill a range in the editor
    method handle-kill ( editor -- )

    \ Copy a range in the editor
    method handle-copy ( editor -- )

    \ Paste in the editor
    method handle-paste ( editor -- )
    
  end-class

  \ Implement buffers
  <buffer> begin-implement

    \ Get whether two cursors share the same preceding newline
    :noname ( cursor0 cursor1 buffer -- same? )
      buffer-dyn-buffer <cursor> [: { cursor0 cursor1 cursor }
        cursor0 cursor copy-cursor
        [: $0A = ;] cursor find-prev
        cursor offset@ { offset0 }
        cursor1 cursor copy-cursor
        [: $0A = ;] cursor find-prev
        cursor offset@ { offset1 }
        offset0 offset1 =
      ;] with-object
    ; define cursors-before-same-newline?

    \ Get whether two cursors are on the same line
    :noname { cursor0 cursor1 buffer -- same? }
      cursor0 cursor1 buffer cursors-before-same-newline? if
        cursor0 buffer cursor-start-dist display-width @ u/
        cursor1 buffer cursor-start-dist display-width @ u/ =
      else
        false
      then
    ; define cursors-on-same-line?
    
    \ Find the distance of a cursor from the previous newline
    :noname ( orig-cursor buffer -- distance )
      buffer-dyn-buffer <cursor> [: { orig-cursor cursor }
        orig-cursor cursor copy-cursor
        cursor offset@ { orig-offset }
        [: $0A = ;] cursor find-prev
        orig-offset cursor offset@ -
      ;] with-object
    ; define cursor-start-dist

    \ Find the distance of a cursor from the next newline
    :noname ( orig-cursor buffer -- distance )
      buffer-dyn-buffer <cursor> [: { orig-cursor cursor }
        orig-cursor cursor copy-cursor
        cursor offset@ { orig-offset }
        [: $0A = ;] cursor find-next
        cursor offset@ orig-offset -
      ;] with-object
    ; define cursor-end-dist

    \ Get a cursor's distance from the left-hand side of the display
    :noname { cursor buffer -- col row }
      cursor buffer cursor-start-dist dup display-width @ umod
      swap display-width @ u/
    ; define cursor-left-space

    \ Get the length of a line a cursor is on
    :noname ( cursor buffer -- length )
      2dup cursor-start-dist -rot cursor-end-dist +
    ; define cursor-line-len

    \ Get the number of rows making up a line
    :noname { cursor buffer -- rows }
      cursor buffer cursor-line-len { len }
      len display-width @ u/
      len display-width @ umod 0<> if 1+ then
    ; define cursor-line-rows
    
    \ Get the length of the last row of a line
    :noname { cursor buffer -- length }
      cursor buffer cursor-line-len display-width @ umod
    ; define cursor-line-last-row-len

    \ Is a cursor in the last row of a line?
    :noname { cursor buffer -- last? }
      cursor buffer cursor-line-last-row-len { len }
      cursor buffer cursor-end-dist { dist }
      len display-width @ < if len dist >= else dist 0= then
    ; define cursor-line-last?

    \ Are we at the start of a line?
    :noname ( buffer -- start? )
      dup buffer-edit-cursor -rot cursor-left-space drop 0=
    ; define edit-cursor-at-start?

    \ Are we at the end of a line?
    :noname ( buffer -- end? )
      dup buffer-edit-cursor swap cursor-end-dist 0=
    ; define edit-cursor-at-end?

    \ Is the edit cursor in the last row of a line
    :noname ( buffer -- last? )
      dup buffer-edit-cursor -rot cursor-line-last?
    ; define edit-cursor-line-last?

    :noname ( buffer -- last-first? )
      2dup edit-cursor-line-last? if
        dup buffer-edit-cursor -rot cursor-left-space drop 0=
      else
        false
      then
    ; define edit-cursor-line-last-first?

    \ Refresh the current line
    :noname ( buffer -- )
      dup <cursor> [:
        default-segment-size [: { buffer cursor data }
          buffer buffer-edit-cursor buffer cursor-start-dist { start-dist }
          buffer buffer-edit-cursor buffer cursor-end-dist { end-dist }
          start-dist end-dist + { len }
          hide-cursor
          buffer buffer-edit-row @ 0 go-to-coord
          buffer buffer-edit-cursor cursor copy-cursor
          start-dist negate cursor adjust-offset
          begin len 0> while
            len default-segment-size min { part-size }
            data part-size cursor read-data { real-size }
            real-size cursor adjust-offset
            data real-size type
            part-size negate +to len
          repeat
          start-dist end-dist + display-width @ < if erase-end-of-line then
          start-dist buffer buffer-edit-col !
          buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-coord
          show-cursor
        ;] with-allot
      ;] with-object
    ; define refresh-line
    
    \ Refresh the display
    :noname ( buffer -- )
      default-segment-size [:
        over buffer-dyn-buffer <cursor> [: { buffer data data-cursor }
          buffer buffer-display-cursor offset@ { display-offset }
          display-offset data-cursor go-to-offset
          display-height @ { rows-remaining }
          display-width @ { cols-remaining }
          cols-remaining 1- rows-remaining 1- { edit-col edit-row }
          buffer buffer-edit-cursor offset@  { edit-offset }
          hide-cursor
          page
          begin
            rows-remaining 0> if
              data default-segment-size data-cursor read-data { actual-cols }
              data { current-data }
              begin
                actual-cols 0> rows-remaining 0> and if
                  edit-offset display-offset = if
                    display-height @ rows-remaining - to edit-row
                    display-width @ cols-remaining - to edit-col
                  then
                  current-data c@ { byte }
                  byte $0A = if
                    cr
                    -1 +to rows-remaining
                    display-width @ to cols-remaining
                  else
                    cols-remaining 0> if
                      byte emit
                      -1 +to cols-remaining
                    else
                      cr byte emit
                      -1 +to rows-remaining
                      display-width @ to cols-remaining
                    then
                  then
                  -1 +to actual-cols
                  1 +to display-offset
                  false
                else
                  true
                then
              until
              false
            else
              true
            then
          until
          edit-row edit-col go-to-coord
          edit-row buffer buffer-edit-row !
          edit-col buffer buffer-edit-col !
          show-cursor
        ;] with-object
      ;] with-allot
    ; define refresh-display

    \ Center the display
    :noname ( center-cursor dest-cursor buffer -- )
      dup <cursor> [: { center-cursor dest-cursor buffer cursor }
        center-cursor cursor copy-cursor
        [: $0A = ;] cursor find-prev
        0 { rows }
        begin
          rows display-height @ 2 / < dest-cursor offset@ 0> and
        while
          -1 cursor adjust-offset
          cursor offset@ { old-offset }
          [: $0A = ;] cursor find-prev
          cursor offset@ { new-offset }
          old-offset new-offset - { diff }
          diff 0> if
            diff display-width @ u/
            diff display-width @ umod 0> if 1+ then
          else
            1
          then { new-rows }
          rows new-rows + display-height @ 2 / <= if
            new-rows +to rows
          else
            dest-cursor cursor copy-cursor
            diff display-width @ umod negate cursor adjust-offset
            1 +to rows
            diff display-width @ u/ 0> if
              display-height @ 2 / rows -
              display-width @ * negate cursor adjust-offset
              new-rows 1- +to rows
            then
          then
          cursor dest-cursor copy-cursor
        repeat
      ;] with-object
    ; define center-cursor
    
    \ Get the number of rows between two cursors
    :noname ( cursor0 cursor1 buffer -- rows )
      dup <cursor> [: { cursor0 cursor1 buffer cursor }
        cursor1 cursor copy-cursor
        [: $0A = ;] cursor find-prev
        0 { rows }
        begin
          cursor offset@ cursor0 offset@ >
        while
          -1 cursor adjust-offset
          cursor offset@ { old-offset }
          [: $0A = ;] cursor find-prev
          cursor offset@ { new-offset }
          old-offset new-offset - { diff }
          new-offset cursor0 offset@ >= if
            diff 0> if
              diff display-width @ u/
              diff display-width @ umod 0> if 1+ then
            else
              1
            then
            +to rows
          else
            old-offset cursor0 offset@ - to diff
            diff display-width @ umod 0> if 1 +to rows then
            diff display-width @ u/ +to rows
          then
        repeat
        rows
      ;] with-object      
    ; define rows-between-cursors
    
    \ Update the display
    :noname { buffer -- update? )
      buffer buffer-edit-cursor { edit-cursor }
      buffer buffer-display-cursor { display-cursor }
      edit-cursor offset@ display-cursor offset@ < if
        edit-cursor display-cursor buffer center-cursor true
      else
        display-cursor edit-cursor buffer rows-between-cursors
        dup display-height @ 3 / <
        swap display-height @ 2 * 3 / > or if
          edit-cursor display-cursor buffer center-cursor true
        else
          false
        then
      else
        false
      then
    ; define update-display

    \ Display a single character
    :noname { c buffer -- }
      c emit
      1 buffer buffer-edit-col +!
    ; define output-char

    \ Backspace a single char
    :noname { buffer -- }
      hide-cursor
      $08 emit
      space
      $08 emit
      show-cursor
      -1 buffer buffer-edit-col +!
    ; define output-backspace

    \ Move the cursor left by one character
    :noname { buffer -- }
      
    ; define do-left
    
    \ Enter a character into the buffer
    :noname { W^ c buffer -- }
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff )
      c 1 buffer buffer-edit-cursor insert-data
      select-diff 0>= if 1 buffer buffer-select-cursor adjust-offset then
    ; define do-char

    \ Backspace in the buffer
    :noname { buffer -- }
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff )
      1 buffer buffer-edit-cursor delete-data
      select-diff 0>= if -1 buffer buffer-select-cursor adjust-offset then
    ; define do-backspace

    \ Delete in the buffer
    :noname { buffer -- }
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff )
      1 buffer buffer-edit-cursor adjust-offset
      1 buffer buffer-edit-cursor delete-data
      select-diff 0> if -1 buffer buffer-select-cursor adjust-offset then
    ; define do-delete

    \ Kill a range in the buffer
    :noname { clip buffer -- }
      buffer buffer-select-cursor { select }
      buffer buffer-edit-cursor { edit }
      select offset@ { select-offset }
      edit offset@ { edit-offset }
      select-offset edit-offset < if
        select edit buffer buffer-dyn-buffer clip copy-to-clip
        edit-offset select-offset - edit delete-data
      else
        edit-offset select-offset < if
          edit select buffer buffer-dyn-buffer clip copy-to-clip
          select-offset edit-offset - select delete-data
        then
      then
    ; define do-kill

    \ Copy a range in the buffer
    :noname { clip buffer -- }
      buffer buffer-select-cursor { select }
      buffer buffer-edit-cursor { edit }
      select offset@ { select-offset }
      edit offset@ { edit-offset }
      select-offset edit-offset < if
        select edit buffer buffer-dyn-buffer clip copy-to-clip
      else
        edit-offset select-offset < if
          edit select buffer buffer-dyn-buffer clip copy-to-clip
        then
      then
    ; define do-copy

    \ Paste in the buffer
    :noname { clip buffer -- }
      buffer buffer-edit-cursor { edit }
      buffer buffer-select-cursor { select }
      select offset@ edit offset@ - { select-diff )
      edit clip insert-from-clip
      select-diff 0>= if edit offset@ select-diff ! select go-to-offset then
    ; define do-paste

  end-implement
  
  \ Implement the clipboard
  <clip> begin-implement
    
    \ Copy from a starting cursor to an ending cursor
    :noname ( start-cursor end-cursor src-dyn-buffer clip -- )
      default-segment-size [:
        2 pick <cursor> [:
          { start-cursor end-cursor src-dyn-buffer clip buffer src-cursor }
          clip clip-cursor offset@ clip clip-cursor delete-data
          start-cursor offset@ src-cursor go-to-offset
          begin
            end-cursor offset@ src-cursor offset@ - default-segment-size min
            { part-size }
            part-size 0> if
              buffer part-size src-cursor read-data to part-size
              buffer part-size clip clip-cursor insert-data
              part-size src-cursor adjust-offset
              false
            else
              true
            then
          until
        ;] with-object
      ;] with-allot
    ; define copy-to-clip

    \ Insert from the clipboard at a cursor
    :noname ( dest-cursor clip -- )
      default-segment-size [: { dest-cursor clip buffer }
        clip clip-cursor { src-cursor }
        src-cursor offset@ { end-offset }
        src-cursor go-to-start
        begin
          end-offset src-cursor offset@ - default-segment-size min { part-size }
          part-size 0> if
            buffer part-size src-cursor read-data to part-size
            buffer part-size dest-cursor insert-data
            part-size src-cursor adjust-offset
          else
            true
          then
        until
      ;] with-allot
    ; define insert-from-clip
    
  end-implement

  \ Editor constants
  0 constant in-middle
  1 constant in-last-line
  2 constant at-end
  3 constant at-last-first
  
  \ Implement the editor
  <editor> begin-implement
    
    \ Enter a character into the editor
    :noname { c editor -- }
      editor editor-current @ { current }
      current edit-cursor-at-end? if
        at-end
      else
        current edit-cursor-line-last? if
          in-last-line
        else
          in-middle
        then
      then { position }
      c current do-char
      current update-display if
        current refresh-display
      else
        position case
          in-middle of current refresh-display endof
          in-last-line of current refresh-line endof
          at-end of c current output-char endof
        endcase
      then
    ; define handle-char

    \ Backspace in the editor
    :noname { editor -- }
      editor editor-current @ { current }
      current edit-cursor-line-last-first? if
        at-last-first
      else
        editor edit-cursor-at-end? if
          at-end
        else
          current edit-cursor-line-last? if
            in-last-line
          else
            in-middle
          then
        then
      then { position }
      current do-backspace
      current update-display if
        current refresh-display
      else
        position case
          in-middle of current refresh-display endof
          at-last-first of current refresh-display endof
          in-last-line of current refresh-line endof
          at-end of current output-backspace endof
        endcase
      then
      current refresh-line
    ; define handle-backspace

    \ Delete in the editor
    :noname { editor -- }
      editor editor-current @ { current }
      current editor-cursor-line-last? if
        in-last-line
      else
        in-middle
      then { position }
      current do-delete
      current update-display if
        current refresh-display
      else
        position case
          in-middle of current refresh-display endof
          in-last-line of current refresh-line endof
        endcase
      then
    ; define handle-delete

    \ Kill a range in the editor
    :noname { editor -- }
      editor editor-current @ { current }
      editor editor-clip current do-kill
      current update-display drop
      current refresh-display
    ; define handle-kill

    \ Copy a range in the editor
    :noname { editor -- }
      editor editor-current @ { current }
      editor editor-clip current do-copy
    ; define handle-copy

    \ Paste in the editor
    :noname { editor -- }
      editor editor-current @ { current }
      editor editor-clip current do-paste
      current update-display drop
      current refresh-display
    ; define handle-paste

  end-implement

  \ The line editor
  variable edit-state
  
  \ Character constants
  $09 constant tab
  $7F constant delete
  $0A constant newline
  $0D constant return
  $01 constant ctrl-a
  $02 constant ctrl-b
  $05 constant ctrl-e
  $06 constant ctrl-f
  $0B constant ctrl-k
  $0E constant ctrl-n
  $10 constant ctrl-p
  $15 constant ctrl-u
  $16 constant ctrl-v
  $17 constant ctrl-w
  $18 constant ctrl-x

  \ Get a particular buffer
  : get-buffer ( u -- b-addr )
    block-size * edit-state @ edit-buffers +
  ;

  \ Get a particular row
  : get-row ( u -- b-addr )
    buffer-width * edit-state @ edit-current @ get-buffer +
  ;
  
  \ Get the current row index
  : current-row-index@ ( -- row )
    edit-state @ edit-cursor-row @ edit-state @ edit-start-row @ -
  ;

  \ Get the current column index
  : current-column-index@ ( -- column )
    edit-state @ edit-cursor-column @ edit-state @ edit-start-column @ -
  ;

  \ Set the current row index
  : current-row-index! ( row -- )
    edit-state @ edit-start-row @ + edit-state @ edit-cursor-row !
  ;

  \ Set the current column index
  : current-column-index! ( column -- )
    edit-state @ edit-start-column @ + edit-state @ edit-cursor-column !
  ;

  \ Get the current row in the buffer
  : current-row ( -- b-addr )
    current-row-index@ get-row
  ;

  \ Get whether a buffer is dirty
  : dirty? ( index -- dirty ) bit edit-state @ edit-dirty bit@ ;

  \ Set whether a buffer is dirty
  : dirty! ( dirty index -- )
    bit swap if
      edit-state @ edit-dirty bis!
    else
      edit-state @ edit-dirty bic!
    then
  ;

  \ Set a buffer's id
  : id! ( id index -- ) cells edit-state @ edit-ids + ! ;

  \ Get a buffer's id
  : id@ ( index -- id ) cells edit-state @ edit-ids + @ ;

  \ Get the current buffer's id
  : current-id ( -- id ) edit-state @ edit-current @ id@ ;

  \ Buffer not found exception
  : x-buffer-not-found ( -- ) ." buffer not found" cr ;
  
  \ Get a buffer's index by id
  : buffer-of-id ( id -- index )
    buffer-count 0 ?do i id@ over = if drop i unloop exit then loop
    drop -1
\    ['] x-buffer-not-found ?raise
  ;

  \ Set the current buffer by id
  : current-by-id! ( id -- )
    buffer-of-id dup -1 <> if edit-state @ edit-current ! else drop then
  ;
  
  \ Get the length of a row in the buffer
  : row-len ( row -- u )
    get-row 0 buffer-width 0 ?do
      over i + c@ dup $20 > swap $7F <> and if
	drop i 1+
      then
    loop
    nip
  ;

  \ Get whether a byte is the start of a unicode code point greater than 127
  : unicode-start? ( b -- flag ) $C0 and $C0 = ;

  \ Get whether a byte is part of a unicode code point greater than 127
  : unicode? ( b -- flag ) $80 and 0<> ;

  \ Initally draw the empty block editor
  : draw-empty ( -- )
    cr
    [char] + emit buffer-width 3 + 0 ?do [char] - emit loop [char] + emit
    buffer-height 0 ?do
      cr [char] | emit buffer-width 3 + 0 ?do space loop [char] | emit
    loop
    cr [char] + emit buffer-width 3 + 0 ?do [char] - emit loop [char] + emit
  ;

  \ Actually draw a row of the block editor
  : draw-row ( row -- )
    [:
      dup edit-state @ edit-start-row @ +
      edit-state @ edit-start-column @ go-to-coord
      dup 9 < if space then
      dup 1+ .
      0 swap get-row dup buffer-width + swap ?do
	i c@ dup $20 >= over $7F <> and over unicode? not and if
	  emit 1+
	else
	  dup unicode? if
	    dup emit
	    unicode-start? if 1+ then
	  else
	    drop leave
	  then
	then
      loop
      buffer-width swap - 0 ?do space loop
    ;] execute-hide-cursor
  ;

  \ Draw all rows
  : draw-all-rows ( -- )
    [: buffer-height 0 ?do i draw-row loop ;] execute-hide-cursor
  ;

  \ Draw header
  : draw-header ( -- )
    [:
      edit-state @ edit-start-row @ 1-
      edit-state @ edit-start-column @ 1- go-to-coord
      ." +-[ "
      edit-state @ edit-current @ id@ 0 <# #s #> dup >r type
      edit-state @ edit-current @ dirty? if
	space [char] * emit r> 1+ >r
      then
      space ." ]"
      buffer-width r> 2 + - 0 ?do [char] - emit loop
    ;] execute-hide-cursor
  ;

  \ Update the current row
  : update-row ( row -- )
    [: draw-row ;] execute-preserve-cursor
  ;

  \ Update all the rows
  : update-all-rows
    [: draw-all-rows ;] execute-preserve-cursor
  ;

  \ Update everything
  : update-all
    [: draw-all-rows draw-header ;] execute-preserve-cursor
  ;

  \ Update header
  : update-header
    [: draw-header ;] execute-preserve-cursor
  ;

  \ Update current row
  : update-current-row ( -- ) current-row-index@ update-row ;

  \ Initialize buffer
  : init-buffer ( index -- )
    get-buffer block-size $20 fill
  ;
  
  \ Get the lowest buffer id
  : lowest-id ( -- id )
    -1 buffer-count 0 ?do dup i id@ u> if drop i id@ then loop
  ;

  \ Get the highest buffer id
  : highest-id ( -- id )
    0 buffer-count 0 ?do dup i id@ u< if drop i id@ then loop
  ;
  
  \ Load a buffer
  : load-buffer ( id index -- )
    false over dirty!
    2dup id!
    over block? if
      swap find-block swap get-buffer block-size move
    else
      nip init-buffer
    then
  ;

  \ Get the starting block to load
  : blocks-first ( id -- id )
    dup half-buffer-count u< if
      drop 0
    else
      dup -1 half-buffer-count - u>= if
	drop -1 buffer-count -
      else
	half-buffer-count -
      then
    then
  ;

  \ Load all of the buffers
  : load-all-buffers ( id -- )
    dup blocks-first buffer-count 0 ?do
      dup i load-buffer 1+
    loop
    drop current-by-id!
  ;

  \ Save a buffer by id
  : save-buffer ( id -- )
    dup buffer-of-id dirty? if
      false over buffer-of-id dirty!
      dup buffer-of-id get-buffer swap block!
    else
      drop
    then
  ;

  \ Save all the buffers
  : save-all-buffers ( -- )
    buffer-count 0 ?do
      i cells edit-state @ edit-ids + @ save-buffer
    loop
  ;

  \ Change buffers
  : change-buffer ( id -- )
    dup highest-id u> if
      lowest-id save-buffer
      dup lowest-id buffer-of-id load-buffer
    else
      dup lowest-id u< if
	highest-id save-buffer
	dup highest-id buffer-of-id load-buffer
      else
      then
    then
    current-by-id!
    update-all
  ;

  \ Get the current column byte index
  : current-column-bytes ( -- offset )
    0 current-column-index@ begin dup 0> while
      over buffer-width < if
	swap 1+ swap over current-row + c@
	dup $20 >= over $80 < and if
	  drop 1-
	else
	  dup unicode? swap unicode-start? and if
	    1-
	    begin
	      over buffer-width < if
		over current-row + c@
		dup unicode? swap unicode-start? not and if
		  swap 1+ swap false
		else
		  true
		then
	      else
		true
	      then
	    until
	  then
	then
      else
	drop 0
      then
    repeat
    drop
  ;

  \ Get the maximum number of columns in a row
  : max-columns ( -- columns )
    current-row-index@ row-len
  ;

  \ Get the number of bytes of the character to the left of the cursor
  : left-bytes ( -- count )
    0 current-column-bytes begin
      dup 0> if
	swap 1+ swap 1- dup current-row + c@
	dup unicode-start? if
	  drop true
	else
	  unicode? not
	then
      else
	true
      then
    until
    drop
  ;

  \ Get the number of bytes of the character to the right of the cursor
  : right-bytes ( -- count )
    current-column-bytes buffer-width < if
      current-row current-column-bytes + c@
      dup $80 u< if
	drop 1
      else
	1 current-column-bytes 1+ begin
	  dup buffer-width u< if
	    dup current-row + c@
	    dup unicode-start? if
	      drop drop true
	    else
	      unicode? if
		1+ swap 1+ swap false
	      else
		drop true
	      then
	    then
	  else
	    drop true
	  then
	until
      then
    else
      0
    then
  ;

  \ Save the current column
  : save-current-column ( -- ) 
    edit-state @ edit-cursor-column @ edit-state @ edit-saved-cursor-column !
  ;

  \ Go to the current coordinate
  : go-to-current-coord ( -- )
    edit-state @ edit-cursor-row @ edit-state @ edit-cursor-column @ 3 +
    go-to-coord
  ;

  \ Dirty the current buffer
  : dirty ( -- )
    edit-state @ edit-current @ dirty?
    true edit-state @ edit-current @ dirty!
    not if update-header then
  ;

  \ Finish unicode insertion
  : finish-insert ( -- )
    save-current-column
    update-current-row
    go-to-current-coord
    dirty
    false edit-state @ edit-unicode-entered !
  ;
  
  \ Resolve unicode entered
  : resolve-unicode-entered ( -- )
    edit-state @ edit-unicode-entered @ if
      systick-counter
      begin
	systick-counter over - 100 u< if
	  key? not if
	    pause
	  else
	    get-key dup unicode? over unicode-start? not and if
	      current-row-index@ row-len buffer-width u< if
		current-row current-column-bytes +
		current-row current-column-bytes 1+ +
		buffer-width current-column-bytes - 1- move
		dup current-row current-column-bytes + c!
	      else
		drop
	      then
	      false
	    else
	      set-key finish-insert
	      true
	    then
	  then
	else
	  finish-insert
	  true
	then
      until
      drop
    then
  ;
  
  \ Handle insertion
  : handle-insert ( b -- )
    current-row-index@ row-len buffer-width u< if
      current-row current-column-bytes +
      current-row current-column-bytes 1+ +
      buffer-width current-column-bytes - 1- move
      dup current-row current-column-bytes + c!
      dup $20 >= over $80 < and if
	drop
	1 edit-state @ edit-cursor-column +!
	save-current-column
	update-current-row
	go-to-current-coord
	dirty
      else
	unicode-start? if
	  1 edit-state @ edit-cursor-column +!
	  true edit-state @ edit-unicode-entered !
	then
      then
    else
      drop
    then
  ;

  \ Handle deletion
  : handle-delete ( -- )
    current-column-index@ 0 > if
      left-bytes
      current-row current-column-bytes +
      2dup swap -
      buffer-width current-column-bytes - move
      current-row buffer-width + over - swap $20 fill
      -1 edit-state @ edit-cursor-column +!
      save-current-column
      update-current-row
      go-to-current-coord
      dirty
    then
  ;

  \ Handle delete forward
  : handle-delete-forward ( -- )
    current-column-bytes buffer-width < if
      right-bytes
      current-row current-column-bytes +
      2dup swap + swap
      buffer-width current-column-bytes - 3 pick - move
      current-row buffer-width + over - swap $20 fill
      save-current-column
      update-current-row
      dirty
    then
  ;

  \ Handle inserting a row
  : handle-insert-row ( -- )
    current-row-index@ buffer-height 1- < if
      buffer-height 1- row-len 0= if
	current-row current-row buffer-width +
	buffer-height current-row-index@ - 1- buffer-width * move
	current-row buffer-width $20 fill
	0 current-column-index!
	save-current-column
	update-all
	dirty
      then
    then
  ;

  \ Handle deleting a row
  : handle-delete-row ( -- )
    max-columns 0> if
      current-column-bytes current-row over + buffer-width rot - $20 fill
      update-all
      dirty
    else
      current-row-index@ buffer-height 1- < if
	current-row buffer-width + current-row
	buffer-height current-row-index@ - 1- buffer-width * move
	buffer-height 1- get-row buffer-width $20 fill
	0 current-column-index!
	save-current-column
	update-all
	dirty
	go-to-current-coord
      then
    then
  ;
  
  \ Handle going forward
  : handle-forward ( -- )
    current-column-bytes buffer-width < if
      1 edit-state @ edit-cursor-column +!
      save-current-column
      go-to-current-coord
    then
  ;

  \ Handle going backward
  : handle-backward ( -- )
    current-column-index@ 0> if
      -1 edit-state @ edit-cursor-column +!
      save-current-column
      go-to-current-coord
    then
  ;

  \ Handle the saved cursor column
  : use-saved-cursor-column ( -- )
    edit-state @ edit-saved-cursor-column @ edit-state @ edit-start-column @ -
    max-columns < if
      edit-state @ edit-saved-cursor-column @ edit-state @ edit-cursor-column !
    else
      max-columns edit-state @ edit-start-column @ +
      edit-state @ edit-cursor-column !
    then
  ;

  \ Handle going to the next buffer
  : handle-next ( -- )
    current-id 1+ dup $FFFFFFFF u< if change-buffer else drop then
    use-saved-cursor-column
    go-to-current-coord
  ;

  \ Handle going to the previous buffer
  : handle-prev ( -- )
    current-id dup 0<> if 1- change-buffer else drop then
    use-saved-cursor-column
    go-to-current-coord
  ;

  \ Handle going up
  : handle-up ( -- )
    current-row-index@ 0> if
      -1 edit-state @ edit-cursor-row +!
      use-saved-cursor-column
      go-to-current-coord
    \ else
    \   buffer-height 1- current-row-index!
    \   handle-prev
    then
  ;

  \ Handle going down
  : handle-down ( -- )
    current-row-index@ buffer-height 1- < if
      1 edit-state @ edit-cursor-row +!
      use-saved-cursor-column
      go-to-current-coord
    \ else
    \   0 current-row-index!
    \   handle-next
    then
  ;

  \ Handle a newline
  : handle-newline ( -- )
    0 current-column-index!
    save-current-column
    current-row-index@ buffer-height 1- < if
      1 edit-state @ edit-cursor-row +!
      go-to-current-coord
    else
      0 current-row-index!
      handle-next
    then
  ;

  \ Handle start
  : handle-start ( -- )
    0 current-column-index!
    save-current-column
    go-to-current-coord
  ;

  \ Handle end
  : handle-end ( -- )
    max-columns current-column-index!
    save-current-column
    go-to-current-coord
  ;

  \ Handle tab
  : handle-tab ( -- )
    current-column-index@ 1 and 0= if $20 handle-insert then
    $20 handle-insert
  ;

  \ Revert the current buffer
  : handle-revert ( -- )
    current-id edit-state @ edit-current @ load-buffer
    update-all
  ;

  \ Write the current buffer
  : handle-write ( -- )
    current-id save-buffer
    update-all
  ;

  \ Configure the block editor
  : config-edit ( id -- )
    reset-ansi-term
    draw-empty
    get-cursor-position
    false edit-state @ edit-unicode-entered !
    buffer-width 4 + - 0 max edit-state @ edit-start-column !
    buffer-height - 0 max edit-state @ edit-start-row !
    get-terminal-size
    edit-state @ edit-terminal-columns ! edit-state @ edit-terminal-rows !
    edit-state @ edit-start-row @ edit-state @ edit-cursor-row !
    edit-state @ edit-start-column @ edit-state @ edit-cursor-column !
    go-to-current-coord
    save-current-column
    0 edit-state @ edit-current !
    0 edit-state @ edit-dirty !
    buffer-count 0 ?do i edit-state @ edit-ids i cells + ! loop
    load-all-buffers
    current-id change-buffer
  ;

  \ Leave the editor
  : leave-edit ( -- )
    edit-state @ edit-start-row @ buffer-height +
    edit-state @ edit-start-column @ buffer-width 4 + + go-to-coord
    edit-state @ ram-here!
  ;

  \ Handle a special key
  : handle-special ( -- )
    get-key case
      [char] A of handle-up endof
      [char] B of handle-down endof
      [char] C of handle-forward endof
      [char] D of handle-backward endof
      [char] 3 of
	get-key case
	  [char] ~ of handle-delete-forward endof
	  clear-keys
	endcase
      endof
      [char] 5 of
	get-key case
	  [char] ~ of handle-prev endof
	  clear-keys
	endcase
      endof
      [char] 6 of
	get-key case
	  [char] ~ of handle-next endof
	  clear-keys
	endcase
      endof
      clear-keys
    endcase
  ;

  \ Handle the escape key
  : handle-escape ( -- )
    get-key case
      [char] [ of handle-special endof
      clear-keys
    endcase
  ;
  
  \ Edit a block
  : edit ( id -- )
    dup $FFFFFFFF <> averts x-invalid-block-id
    edit-size [:
      edit-state !
      config-edit
      begin
	resolve-unicode-entered
	get-key
	dup $20 u< if
	  case
	    return of handle-newline false endof
	    newline of handle-newline false endof
	    tab of handle-tab false endof
	    ctrl-a of handle-start false endof
	    ctrl-e of handle-end false endof
	    ctrl-f of handle-forward false endof
	    ctrl-b of handle-backward false endof
	    ctrl-n of handle-next false endof
	    ctrl-p of handle-prev false endof
	    ctrl-v of true endof
	    ctrl-w of handle-write false endof
	    ctrl-x of handle-revert false endof
	    ctrl-u of handle-insert-row false endof
	    ctrl-k of handle-delete-row false endof
	    escape of handle-escape false endof
	    swap false swap
	  endcase
	else
	  dup delete = if
	    drop handle-delete
	  else
	    handle-insert
	  then
	  false
	then
      until
      save-all-buffers
      leave-edit
    ;] with-aligned-allot
  ;

end-module
