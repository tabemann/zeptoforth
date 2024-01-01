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

  \ Heap block size
  dyn-buffer-internal::segment-header-size default-segment-size +
  constant heap-block-size
  
  \ Display width
  variable display-width

  \ Display height
  variable display-height

  \ Go to a buffer coordinate
  : go-to-buffer-coord ( row col -- ) swap 1+ swap go-to-coord ;
  
  \ Buffer class
  <object> begin-class <buffer>

    \ Heap
    cell member buffer-heap
    
    \ Buffer name
    2 cells member buffer-name

    \ Buffer path
    2 cells member buffer-path

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

    \ Get whether two cursors share the same preceding newline
    method cursors-before-same-newline? ( cursor0 cursor1 buffer -- same? )

    \ Get whether two cursors are on the same line
    method cursors-on-same-line? ( cursor0 cursor1 buffer -- same? )
    
    \ Find the distance of a cursor from the previous newline
    method cursor-start-dist ( orig-cursor buffer -- distance )

    \ Find the distance of a cursor from the next newline
    method cursor-end-dist ( orig-cursor buffer -- distance )

    \ Get a cursor's distance from the left-hand side of the display
    method cursor-left-space ( cursor buffer -- col row )

    \ Get the length of a line a cursor is on
    method cursor-line-len ( cursor buffer -- length )

    \ Get the number of rows making up a line
    method cursor-line-rows ( cursor buffer -- rows )
    
    \ Get the length of the last row of a line
    method cursor-line-last-row-len ( cursor buffer -- length )

    \ Is a cursor in the last row of a line?
    method cursor-line-last? ( cursor buffer -- last? )

    \ Edit cursor left space
    method edit-cursor-left-space ( buffer -- cols rows )

    \ Edit cursor offset
    method edit-cursor-offset@ ( buffer -- offset )

    \ Buffer content length
    method buffer-len@ ( buffer -- length )

    \ Are we at the start of a line?
    method edit-cursor-at-start? ( buffer -- start? )

    \ Are we at the end of a line?
    method edit-cursor-at-end? ( buffer -- end? )

    \ Is the edit cursor in the last row of a line
    method edit-cursor-line-last? ( buffer -- last? )

    \ Is the edit cursor at the start of the last row of a line
    method edit-cursor-line-last-first? ( buffer -- last-first? )

    \ Is the edit cursor in the first row of the first line
    method edit-cursor-first-row? ( buffer -- first-row? )

    \ Is the edit cursor in the last row of the last line
    method edit-cursor-last-row? ( buffer -- last-row? )

    \ Output buffer title
    method output-title ( buffer -- )
    
    \ Refresh the current line
    method refresh-line ( buffer -- )
    
    \ Refresh the display
    method refresh-display ( buffer -- )

    \ Center the display
    method center-cursor ( center-cursor dest-cursor buffer -- )
    
    \ Get the number of rows between two cursors
    method rows-between-cursors ( cursor0 cursor1 buffer -- rows )
    
    \ Update the display
    method update-display ( buffer -- update? )

    \ Display a single character
    method output-char ( c buffer -- )

    \ Backspace a single char
    method output-backspace ( buffer -- )

    \ Move the cursor to the left
    method output-left ( buffer -- )

    \ Move the cursor to the end of the previous row
    method output-prev-row ( buffer -- )

    \ Move the cursor to the end of the previous line
    method output-prev-line ( buffer -- )

    \ Move the cursor to the right
    method output-right ( buffer -- )

    \ Move the cursor to the start of the next row
    method output-next-row ( buffer -- )

    \ Move the cursor to the start of the next line
    method output-next-line ( buffer -- )

    \ Move the cursor to the first position in a buffer
    method output-first ( buffer -- )

    \ Move the cursor to the previous line
    method output-up ( buffer -- )

    \ Move the cursor to the last position in a buffer
    method output-last ( buffer -- )
    
    \ Move the cursor to the next line
    method output-down ( buffer -- )
    
    \ Go to the first position in a buffer
    method do-first ( buffer -- )

    \ Go to the last position in a buffer
    method do-last ( buffer -- )
    
    \ Move the edit cursor left by one character
    method do-backward ( buffer -- )

    \ Move the edit cursor right by one character
    method do-forward ( buffer -- )

    \ Move the edit cursor up by one character
    method do-up ( buffer -- )

    \ Move the edit cursor down by one character
    method do-down ( buffer -- )
    
    \ Enter a character into the buffer
    method do-insert ( c buffer -- )

    \ Backspace in the buffer
    method do-delete ( buffer -- )

    \ Delete in the buffer
    method do-delete-forward ( buffer -- )

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
    
    \ Clipboard
    <clip> class-size member editor-clip

    \ Go left one character
    method handle-backward ( editor -- )

    \ Go right one character
    method handle-forward ( editor -- )

    \ Go up one character
    method handle-up ( editor -- )

    \ Go down one character
    method handle-down ( editor -- )
    
    \ Enter a character into the editor
    method handle-insert ( c editor -- )

    \ Enter a newline into the editor
    method handle-newline ( editor -- )

    \ Backspace in the editor
    method handle-delete ( editor -- )

    \ Delete in the editor
    method handle-delete-forward ( editor -- )

    \ Kill a range in the editor
    method handle-kill ( editor -- )

    \ Copy a range in the editor
    method handle-copy ( editor -- )

    \ Paste in the editor
    method handle-paste ( editor -- )

  end-class

  \ Implement buffers
  <buffer> begin-implement
    
    \ Constructor
    :noname { name-addr name-bytes heap buffer -- }
      buffer <object>->new
      heap buffer buffer-heap !
      name-bytes heap allocate { new-name-addr }
      name-addr new-name-addr name-bytes move
      name-bytes heap allocate { new-path-addr }
      name-addr new-path-addr name-bytes move
      new-name-addr name-bytes buffer buffer-name 2!
      new-path-addr name-bytes buffer buffer-path 2!
      0 buffer buffer-prev !
      0 buffer buffer-prev !
      0 buffer buffer-edit-col !
      0 buffer buffer-edit-row !
      heap <dyn-buffer> buffer buffer-dyn-buffer init-object
      buffer buffer-dyn-buffer <cursor> buffer buffer-display-cursor init-object
      buffer buffer-dyn-buffer <cursor> buffer buffer-edit-cursor init-object
      buffer buffer-dyn-buffer <cursor> buffer buffer-select-cursor init-object
    ; define new

    \ Destructor
    :noname { buffer -- }
      buffer buffer-select-cursor destroy
      buffer buffer-edit-cursor destroy
      buffer buffer-display-cursor destroy
      buffer buffer-dyn-buffer destroy
      buffer buffer-name 2@ drop buffer buffer-heap @ free
      buffer buffer-path 2@ drop buffer buffer-heap @ free
      buffer <object>->destroy
    ; define destroy
    
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

    \ Edit cursor left space
    :noname ( buffer -- cols rows )
      dup buffer-edit-cursor swap cursor-left-space
    ; define edit-cursor-left-space

    \ Edit cursor offset
    :noname ( buffer -- offset )
      buffer-edit-cursor offset@
    ; define edit-cursor-offset@

    \ Buffer content length
    :noname ( buffer -- length )
      buffer-dyn-buffer dyn-buffer-len@
    ; define buffer-len@

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

    \ Is the edit cursor at the start of the last row of a line
    :noname ( buffer -- last-first? )
      2dup edit-cursor-line-last? if
        dup buffer-edit-cursor -rot cursor-left-space drop 0=
      else
        false
      then
    ; define edit-cursor-line-last-first?

    \ Is the edit cursor in the first row of the first line
    :noname ( buffer -- first-row? )
      dup buffer-edit-cursor over cursor-start-dist swap offset@ =
    ; define edit-cursor-first-row?

    \ Is the edit cursor in the last row of the last line
    :noname { buffer -- last-row? }
      buffer buffer-edit-cursor offset@
      buffer buffer-edit-cursor buffer cursor-end-dist +
      buffer buffer-dyn-buffer dyn-buffer-len@ = if
        buffer edit-cursor-line-last?
      else
        false
      then
    ; define edit-cursor-last-row?
    
    \ Output buffer title
    :noname { buffer -- }
      0 0 go-to-coord
      buffer buffer-name 2@ dup display-width @ < if
        type erase-end-of-line
      else
        dup display-width @ = if
          type
        else
          display-width @ - { offset }
          offset - swap offset + swap type
        then
      then
    ; define output-title
    
    \ Refresh the current line
    :noname ( buffer -- )
      dup <cursor> [:
        default-segment-size [: { buffer cursor data }
          buffer buffer-edit-cursor buffer cursor-start-dist { start-dist }
          buffer buffer-edit-cursor buffer cursor-end-dist { end-dist }
          start-dist end-dist + { len }
          hide-cursor
          buffer buffer-edit-row @ 0 go-to-buffer-coord
          buffer buffer-edit-cursor cursor copy-cursor
          start-dist negate cursor adjust-offset
          begin len 0> while
            len default-segment-size min { part-size }
            data part-size cursor read-data { real-size }
            data real-size type
            part-size negate +to len
          repeat
          start-dist end-dist + display-width @ < if erase-end-of-line then
          start-dist buffer buffer-edit-col !
          buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord
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
          buffer output-title
          begin
            rows-remaining 0> if
              data default-segment-size data-cursor read-data { actual-bytes }
              data { current-data }
              begin
                actual-bytes 0> rows-remaining 0> and if
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
                  -1 +to actual-bytes
                  1 +to current-data
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
          edit-row edit-col go-to-buffer-coord
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

    \ Move the cursor to the left
    :noname { buffer -- }
      -1 buffer buffer-edit-col +!
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord
    ; define output-left

    \ Move the cursor to the end of the previous row
    :noname { buffer -- }
      -1 buffer buffer-edit-row +!
      display-width @ 1- buffer buffer-edit-col !
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord      
    ; define output-prev-row

    \ Move the cursor to the end of the previous line
    :noname { buffer -- }
      -1 buffer buffer-edit-row +!
      buffer edit-cursor-left-space drop buffer buffer-edit-col !
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord      
    ; define output-prev-line

    \ Move the cursor to the right
    :noname { buffer -- }
      1 buffer buffer-edit-col +!
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord
    ; define output-right

    \ Move the cursor to the start of the next row
    :noname { buffer -- }
      1 buffer buffer-edit-row +!
      0 buffer buffer-edit-col !
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord
    ; define output-next-row

    \ Move the cursor to the start of the next line
    :noname { buffer -- }
      1 buffer buffer-edit-row +!
      0 buffer buffer-edit-col !
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord
    ; define output-next-line

    \ Move the cursor to the first position in a buffer
    :noname { buffer -- }
      0 buffer buffer-edit-row !
      0 buffer buffer-edit-col !
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord
    ; define output-first

    \ Move the cursor to the previous line
    :noname { buffer -- }
      buffer edit-cursor-left-space { cols rows }
      -1 buffer buffer-edit-row +!
      cols buffer buffer-edit-col !
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord
    ; define output-up

    \ Move the cursor to the last position in a buffer
    :noname { buffer -- }
      buffer edit-cursor-left-space { cols rows }
      buffer buffer-edit-cursor buffer cursor-end-dist { dist }
      cols dist + buffer buffer-edit-col !
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord
    ; define output-last
    
    \ Move the cursor to the next line
    :noname { buffer -- }
      buffer edit-cursor-left-space { cols rows }
      1 buffer buffer-edit-row +!
      cols buffer buffer-edit-col !
      buffer buffer-edit-row @ buffer buffer-edit-col @ go-to-buffer-coord
    ; define output-down
    
    \ Go to the first position in a buffer
    :noname ( buffer -- )
      buffer-edit-cursor go-to-start
    ; define do-first

    \ Go to the last position in a buffer
    :noname ( buffer -- )
      dup buffer-dyn-buffer dyn-buffer-len@ swap buffer-edit-cursor go-to-offset
    ; define do-last
    
    \ Move the edit cursor left by one character
    :noname { buffer -- }
      -1 buffer buffer-edit-cursor adjust-offset
    ; define do-backward

    \ Move the edit cursor right by one character
    :noname { buffer -- }
      1 buffer buffer-edit-cursor adjust-offset
    ; define do-forward

    \ Move the edit cursor up by one character
    :noname { buffer -- }
      buffer buffer-edit-cursor buffer cursor-start-dist { dist }
      dist display-width @ <= if
        [: $0A = ;] buffer buffer-edit-cursor find-prev
        -1 buffer buffer-edit-cursor adjust-offset
        buffer buffer-edit-cursor buffer cursor-start-dist { len }
        len display-width @ umod dup { last-len } dist >= if
          last-len negate dist + buffer buffer-edit-cursor adjust-offset
        then
      else
        display-width @ negate buffer buffer-edit-cursor adjust-offset
      then
    ; define do-up

    \ Move the edit cursor down by one character
    :noname { buffer -- }
      buffer buffer-edit-cursor buffer cursor-start-dist { dist }
      buffer buffer-edit-cursor buffer cursor-left-space { col row }
      buffer edit-cursor-line-last? if
        dist display-width @ umod { last-len }
        [: $0A = ;] buffer buffer-edit-cursor find-next
        1 buffer buffer-edit-cursor adjust-offset
        buffer buffer-edit-cursor buffer cursor-line-len { next-len }
        last-len next-len min buffer buffer-edit-cursor adjust-offset
      else
        buffer buffer-edit-cursor buffer cursor-line-len { len }
        len dist - dist display-width @ umod > if
          display-width @ buffer buffer-edit-cursor adjust-offset
        else
          len dist - buffer buffer-edit-cursor adjust-offset
        then
      then
    ; define do-down
    
    \ Enter a character into the buffer
    :noname { W^ c buffer -- }
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff )
      c 1 buffer buffer-edit-cursor insert-data
      select-diff 0>= if 1 buffer buffer-select-cursor adjust-offset then
    ; define do-insert

    \ Backspace in the buffer
    :noname { buffer -- }
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff )
      1 buffer buffer-edit-cursor delete-data
      select-diff 0>= if -1 buffer buffer-select-cursor adjust-offset then
    ; define do-delete

    \ Delete in the buffer
    :noname { buffer -- }
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff )
      1 buffer buffer-edit-cursor adjust-offset
      1 buffer buffer-edit-cursor delete-data
      select-diff 0> if -1 buffer buffer-select-cursor adjust-offset then
    ; define do-delete-forward

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

    \ Constructor
    :noname { heap clip -- }
      clip <object>->new
      heap <dyn-buffer> clip clip-dyn-buffer init-object
      clip clip-dyn-buffer <cursor> clip clip-cursor init-object
    ; define new

    \ Destructor
    :noname { clip -- }
      clip clip-cursor destroy
      clip clip-dyn-buffer destroy
      clip <object>->destroy
    ; define destroy
    
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

    \ Constructor
    :noname { heap editor -- }
      editor <object>->new
      heap editor editor-heap !
      heap <clip> editor editor-clip init-object
      <buffer> class-size heap allocate { buffer }
      s" F00bar" heap <buffer> buffer init-object
      buffer editor editor-first !
      buffer editor editor-last !
      buffer editor editor-current !
    ; define new

    \ Destructor
    :noname { editor -- }
      editor editor-current @ destroy
      editor editor-current @ editor editor-heap @ free
      editor <object>->destroy
    ; define destroy
    
    \ Go left one character
    :noname { editor -- }
      editor editor-current @ { current }
      current edit-cursor-offset@ 0> if
        current edit-cursor-left-space { cols rows }
        current do-backward
        current update-display if
          current refresh-display
        else
          cols 0> if
            current output-left
          else
            rows 0> if
              current output-prev-row
            else
              current output-prev-line
            then
          then
        then
      then
    ; define handle-backward

    \ Go right one character
    :noname { editor -- }
      editor editor-current @ { current }
      current edit-cursor-offset@ current buffer-len@ < if
        current edit-cursor-left-space { cols rows }
        current edit-cursor-at-end? { at-end? }
        current do-forward
        current update-display if
          current refresh-display
        else
          at-end? if
            current output-next-line
          else
            cols display-width @ 1- < if
              current output-right
            else
              current output-next-row
            then
          then
        then
      then
    ; define handle-forward

    \ Go up one character
    :noname { editor -- }
      editor editor-current @ { current }
      current edit-cursor-first-row? if
        current do-first
        current update-display if
          current refresh-display
        else
          current output-first
        then
      else
        current do-up
        current update-display if
          current refresh-display
        else
          current output-up
        then
      then
    ; define handle-up

    \ Go down one character
    :noname { editor -- }
      editor editor-current @ { current }
      current edit-cursor-last-row? if
        current do-last
        current update-display if
          current refresh-display
        else
          current output-last
        then
      else
        current do-down
        current update-display if
          current refresh-display
        else
          current output-down
        then
      then
    ; define handle-down
    
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
      c current do-insert
      current update-display if
        current refresh-display
      else
        position case
          in-middle of current refresh-display endof
          in-last-line of current refresh-line endof
          at-end of c current output-char endof
        endcase
      then
    ; define handle-insert

    \ Enter a newline into the editor
    :noname { editor -- }
      editor editor-current @ { current }
      $0A current do-insert
      current update-display drop
      current refresh-display
    ; define handle-newline

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
      current do-delete
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
    ; define handle-delete

    \ Delete in the editor
    :noname { editor -- }
      editor editor-current @ { current }
      current editor-cursor-line-last? if
        in-last-line
      else
        in-middle
      then { position }
      current do-delete-forward
      current update-display if
        current refresh-display
      else
        position case
          in-middle of current refresh-display endof
          in-last-line of current refresh-line endof
        endcase
      then
    ; define handle-delete-forward

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

  \ Get whether a byte is the start of a unicode code point greater than 127
  : unicode-start? ( b -- flag ) $C0 and $C0 = ;

  \ Get whether a byte is part of a unicode code point greater than 127
  : unicode? ( b -- flag ) $80 and 0<> ;

  \ Configure the block editor
  : config-edit ( id -- )
    reset-ansi-term
    draw-empty
    get-cursor-position
    false edit-state @ edit-unicode-entered !
    buffer-width 4 + - 0 max edit-state @ edit-start-column !
    buffer-height - 0 max edit-state @ edit-start-row !
    get-terminal-size display-width ! 2 - display-height !
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
  : edit ( -- )
    here dup [: { heap-addr }
      heap-block-size backing-heap-size backing-heap-size / heap-addr init-heap
      config-edit
      <editor> class-size heap-addr allocate { editor }
      heap-addr <editor> editor init-object
      begin
	get-key
	dup $20 u< if
	  case
	    return of editor handle-newline false endof
	    newline of editor handle-newline false endof
\	    tab of handle-tab false endof
\	    ctrl-a of editor handle-start false endof
\	    ctrl-e of editor handle-end false endof
	    ctrl-f of editor handle-forward false endof
	    ctrl-b of editor handle-backward false endof
\	    ctrl-n of editor handle-next false endof
\	    ctrl-p of editor handle-prev false endof
	    ctrl-v of true endof
\	    ctrl-w of handle-write false endof
\	    ctrl-x of handle-revert false endof
\	    ctrl-u of handle-insert-row false endof
\	    ctrl-k of handle-delete-row false endof
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
