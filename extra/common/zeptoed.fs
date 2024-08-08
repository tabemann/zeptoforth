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

\ Heap size (roughly - this is not exact)
65536 value zeptoed-heap-size

\ Indent size
2 value zeptoed-indent-size

\ Tab character
8 value zeptoed-tab-size

\ Maximum search size
4096 value zeptoed-max-search-size

\ Save files with CRLF newlines
false value zeptoed-save-crlf-enabled

begin-module zeptoed-internal

  oo import
  dyn-buffer import
  systick import
  heap import
  fat32 import
  ansi-term import

  \ DEBUG
  : *echo* [immediate] char lit, postpone internal::serial-emit ;
  \ DEBUG

  \ Character constants
  $09 constant tab
  $7F constant delete
  $0A constant newline
  $0D constant return
  $00 constant ctrl-space
  $01 constant ctrl-a
  $02 constant ctrl-b
  $05 constant ctrl-e
  $06 constant ctrl-f
  $0B constant ctrl-k
  $0C constant ctrl-l
  $0E constant ctrl-n
  $0F constant ctrl-o
  $10 constant ctrl-p
  $12 constant ctrl-r
  $15 constant ctrl-u
  $16 constant ctrl-v
  $17 constant ctrl-w
  $18 constant ctrl-x
  $19 constant ctrl-y
  $1A constant ctrl-z

  \ Character entry mode
  0 constant insert-mode
  1 constant search-forward-mode
  2 constant search-backward-mode
  
  \ Normal video
  : normal-video ( -- ) csi ." 0m" ;

  \ Inverse video
  : inverse-video ( -- ) csi ." 7m" ;
  
  \ Get whether a byte is the start of a unicode code point greater than 127
  : unicode-start? ( b -- flag ) $C0 and $C0 = ;

  \ Get whether a byte is part of a unicode code point greater than 127
  : unicode? ( b -- flag ) $80 and 0<> ;

  \ Get whether a string contains any uppercase characters
  : contains-upper? ( c-addr u -- flag )
    over + swap ?do
      i c@ dup [char] A >= swap [char] Z <= and if
        true unloop exit
      then
    loop
    false
  ;

  \ Get file error string
  : file-error-string ( exception -- c-addr u )
    case
      ['] x-sector-size-not-supported of
        s" Sector sizes other than 512 are not supported"
      endof
      ['] x-fs-version-not-supported of
        s" FAT32 version not supported"
      endof
      ['] x-bad-info-sector of
        s" Bad info sector"
      endof
      ['] x-no-clusters-free of
        s" No clusters free"
      endof
      ['] x-file-name-format of
        s" Unsupported filename"
      endof
      ['] x-out-of-range-entry of
        s" Out of range directory entry"
      endof
      ['] x-out-of-range-partition of
        s" Out of range partition"
      endof
      ['] x-entry-not-found of
        s" Directory entry not found"
      endof
      ['] x-entry-already-exists of
        s" Directory entry already exists"
      endof
      ['] x-entry-not-file of
        s" Directory entry not file"
      endof
      ['] x-entry-not-dir of
        s" Directory entry not directory"
      endof
      ['] x-forbidden-dir of
        s" Forbidden directory name"
      endof
      ['] x-empty-path of
        s" Empty path"
      endof
      ['] x-invalid-path of
        s" Invalid path"
      endof
      s" "
    endcase
  ;

  \ Strip carriage returns
  : strip-crs { addr bytes -- bytes' }
    0 { offset }
    begin offset bytes < while
      addr offset + c@ $0D = if
        addr offset + 1+ addr offset + bytes offset - 1- move
        -1 +to bytes
      else
        1 +to offset
      then
    repeat
    bytes
  ;

  \ Expand newlines to CRLF's - note that the available amount of space must be
  \ 2x bytes
  : expand-newlines-to-crlfs { addr bytes -- bytes }
    0 { offset }
    begin offset bytes < while
      addr offset + c@ newline = if
        addr offset + addr offset + 1+ bytes offset - move
        $0D addr offset + c!
        1 +to bytes
        2 +to offset
      else
        1 +to offset
      then
    repeat
    bytes
  ;

  \ My base heap size
  65536 constant my-base-heap-size

  \ Maximum undo count
  64 constant max-undo-count

  \ Maximum undo byte count
  1024 constant max-undo-byte-count

  \ Delete undo accumulation
  -20 constant delete-undo-accumulation
  
  \ My block size
  16 dyn-buffer-internal::segment-header-size + constant my-block-size
  
  \ Display width
  variable display-width

  \ Display height
  variable display-height

  \ Get the length of a string
  : string-len { chars addr bytes -- chars' }
    bytes 0 ?do
      addr i + c@ { byte }
      byte tab = if
        zeptoed-tab-size chars zeptoed-tab-size umod - +to chars
      else
        byte bl >= byte delete < and byte unicode-start? or if 1 +to chars then
      then
    loop
    chars
  ;

  \ Undo header
  begin-structure undo-header-size

    \ Size
    field: undo-size

    \ Offset
    field: undo-offset

  end-structure
          
  \ Buffer class
  <object> begin-class <buffer>

    \ Editor
    cell member buffer-editor
    
    \ Heap
    cell member buffer-heap
    
    \ Buffer path
    2 cells member buffer-path

    \ Buffer name
    2 cells member buffer-name

    \ Previous buffer
    cell member buffer-prev

    \ Next buffer
    cell member buffer-next

    \ Edit cursor column
    cell member buffer-edit-col
    
    \ Edit cursor row
    cell member buffer-edit-row

    \ Select enabled
    cell member buffer-select-enabled

    \ Is the buffer dirty
    cell member buffer-dirty

    \ Character entry mode
    cell member buffer-char-entry-mode

    \ Search string
    2 cells member buffer-search-text

    \ Buffer dynamic buffer
    <dyn-buffer> class-size member buffer-dyn-buffer
    
    \ Display area cursor
    <cursor> class-size member buffer-display-cursor

    \ Edit cursor
    <cursor> class-size member buffer-edit-cursor

    \ Previous edit cursor
    <cursor> class-size member buffer-prev-cursor

    \ Select cursor
    <cursor> class-size member buffer-select-cursor

    \ Undo count
    cell member buffer-undo-count

    \ Undo byte count
    cell member buffer-undo-bytes

    \ Undo array
    max-undo-count cells member buffer-undo-array

    \ Left bound index
    cell member buffer-left-bound

    \ Update the previous cursor
    method update-prev-cursor ( buffer -- )
    
    \ Dirty buffer
    method dirty-buffer ( buffer -- )

    \ Clean buffer
    method clean-buffer ( buffer -- )

    \ Leave search
    method leave-search ( buffer -- )

    \ Get whether a buffer is being searched
    method searching? ( buffer -- searching? )

    \ Get whether a buffer is being searched forward
    method searching-forward? ( buffer -- searching-forward? )

    \ Get whether a buffer is being searched backward
    method searching-backward? ( buffer -- searching-backward? )

    \ Get the buffer search text
    method search-text@ ( buffer -- addr len )

    \ Buffer width in characters
    method buffer-width@ ( buffer -- width )

    \ Buffer height in characters
    method buffer-height@ ( buffer -- height )

    \ Set buffer coordinate
    method set-buffer-coord ( buffer -- )
    
    \ Go to a buffer coordinate
    method go-to-buffer-coord ( row col buffer -- )

    \ Clear the undos
    method clear-undos ( buffer -- )
    
    \ Get the oldest undo
    method oldest-undo@ ( buffer -- undo )

    \ Get the newest undo
    method newest-undo@ ( buffer -- undo )

    \ Push an undo
    method push-undo ( bytes buffer -- undo )

    \ Drop an undo
    method drop-undo ( buffer -- )

    \ Free an undo
    method free-undo ( undo buffer -- )
    
    \ Make room for undos
    method ensure-undo-space ( bytes buffer -- )
    
    \ Add an insert undo
    method add-insert-undo ( start-offset end-offset insert-offset buffer -- )

    \ Add a delete undo
    method add-delete-undo ( bytes offset buffer -- )

    \ Check whether any lines in a range are not indented
    method not-indented? ( cursor offset1 buffer -- not-indented? )

    \ Is there a tab in the indentation?
    method cursor-tab-in-indent? ( cursor buffer -- tab-in-indent? )

    \ Get the number of indentation bytes
    method cursor-line-indent-bytes ( cursor buffer -- indent-bytes )
    
    \ Get a line's indentation
    method cursor-line-indent ( cursor buffer -- indent )

    \ Get the edit cursor's line's indentation
    method edit-cursor-line-indent ( cursor buffer -- indent )

    \ Get the number of continguous space characters at a cursor
    method cursor-space-chars ( cursor buffer -- chars )
    
    \ Get whether two cursors share the same preceding newline
    method cursors-before-same-newline? ( cursor0 cursor1 buffer -- same? )

    \ Get whether two cursors are on the same line
    method cursors-on-same-line? ( cursor0 cursor1 buffer -- same? )

    \ Get the distance between two offsets
    method char-dist ( init-chars offset0 offset1 buffer -- distance )
    
    \ Get the byte before a cursor
    method cursor-before ( orig-cursor buffer -- c|0 )

    \ Get the byte at a cursor
    method cursor-at ( orig-cursor buffer -- c|0 )

    \ Get the byte before the edit cursor
    method edit-cursor-before ( buffer -- c|0 )

    \ Get the byte at the edit cursor
    method edit-cursor-at ( buffer -- c|0 )

    \ Length in spaces of the character before the edit cursor
    method edit-cursor-before-spaces ( buffer -- spaces )

    \ Length in spaces of the character at the edit cursor
    method edit-cursor-at-spaces ( buffer -- spaces )

    \ Find the raw distance of a cursor from the previous newline
    method cursor-raw-start-dist ( orig-cursor buffer -- distance )
    
    \ Find the distance of a cursor from the previous newline
    method cursor-start-dist ( orig-cursor buffer -- distance )

    \ Find the raw distance of a cursor from the next newline
    method cursor-raw-end-dist ( orig-cursor buffer -- distance )

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

    \ Is a cursor in the first row of a line?
    method cursor-line-first? ( cursor buffer -- first? )

    \ Is a cursor in the last row of a line?
    method cursor-line-last? ( cursor buffer -- last? )

    \ Edit cursor left space
    method edit-cursor-left-space ( buffer -- cols rows )

    \ Get the number of rows making up the current edit line
    method edit-cursor-line-rows ( buffer -- rows )

    \ Get whether the current line takes up the full width
    method edit-cursor-full-width? ( buffer -- full-width? )

    \ Edit cursor offset
    method edit-cursor-offset@ ( buffer -- offset )

    \ Buffer content length
    method buffer-len@ ( buffer -- length )

    \ Get whether a cursor is on a special line
    method cursor-special-line?
    ( cursor buffer -- special? )

    \ Get whether the edit cursor is on a special line
    method edit-cursor-special-line?
    ( buffer -- special? )

    \ Are we at the start of a line?
    method edit-cursor-at-start? ( buffer -- start? )

    \ Are we at the start of a row?
    method edit-cursor-at-row-start? ( buffer -- row-start? )

    \ Are we at the end of a line?
    method edit-cursor-at-end? ( buffer -- end? )

    \ Are we at the end of a row?
    method edit-cursor-at-row-end? ( buffer -- row-end? )

    \ Is the edit cursor in the last row of a line
    method edit-cursor-line-last? ( buffer -- last? )

    \ Is the edit cursor in a single row line?
    method edit-cursor-single-row? ( buffer -- single-row? )

    \ Is the edit cursor at the start of the last row of a line
    method edit-cursor-line-last-first? ( buffer -- last-first? )

    \ Is a cursor in the first row of the first line
    method cursor-first-row? ( cursor buffer -- first-row? )

    \ Is a cursor in the last row of the last line
    method cursor-last-row? ( cursor buffer -- last-row? )

    \ Is the edit cursor in the first row of the first line
    method edit-cursor-first-row? ( buffer -- first-row? )

    \ Is the edit cursor in the last row of the last line
    method edit-cursor-last-row? ( buffer -- last-row? )

    \ Inner workings of searching forward
    method cursor-search-forward ( addr count cursor buffer -- found? )

    \ Inner workings of searching backward
    method cursor-search-backward ( addr count cursor buffer -- found? )
    
    \ Search forward
    method edit-cursor-search-forward ( addr count buffer -- )

    \ Search backward
    method edit-cursor-search-backward ( addr count buffer -- )

    \ Continue searching forward
    method edit-cursor-continue-forward ( addr count buffer -- )

    \ Continue searching backward
    method edit-cursor-continue-backward ( addr count buffer -- )

    \ Add character to search text
    method add-search-text-char ( c buffer -- )

    \ Add text to search text
    method add-search-text ( addr count buffer -- )
    
    \ Delete character from search text
    method delete-search-text-char ( buffer -- )
    
    \ Output buffer title
    method output-title ( buffer -- )
    
    \ Refresh the current line
    method refresh-line ( buffer -- )
    
    \ Refresh the display
    method refresh-display ( buffer -- )

    \ Center the display
    method center-cursor ( center-cursor dest-cursor buffer -- )

    \ Put a cursor on the same row as a cursor
    method same-row-cursor ( src-cursor dest-cursor buffer -- )

    \ Get the number of rows between two cursors
    method rows-between-cursors ( cursor0 cursor1 buffer -- rows )
    
    \ Update the display
    method update-display ( buffer -- update? )

    \ Display a single character
    method output-char ( c buffer -- )

    \ Backspace a single char
    method output-backspace ( buffer -- )

    \ Move the cursor to the left
    method output-left ( spaces buffer -- )

    \ Move the cursor to the end of the previous row
    method output-prev-row ( buffer -- )

    \ Move the cursor to the end of the previous line
    method output-prev-line ( buffer -- )

    \ Move the cursor to the right
    method output-right ( spaces buffer -- )

    \ Move the cursor to the start of the next row
    method output-next-row ( buffer -- )

    \ Move the cursor to the start of the next line
    method output-next-line ( buffer -- )

    \ Move the cursor to the last position in a buffer
    method output-last ( buffer -- )

    \ Move the cursor to the previous line
    method output-up ( buffer -- )
    
    \ Move the cursor to the next line
    method output-down ( buffer -- )

    \ Go to the start of a Unicode character
    method go-to-unicode-start ( buffer -- )

    \ Go to the end of a Unicode character
    method go-to-unicode-end ( buffer -- )
    
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

    \ Enter a newline with indentation into the buffer
    method do-newline ( buffer -- )
    
    \ Find the begin and end offsets for a delete
    method find-delete-range ( buffer -- start-offset end-offset )

    \ Find the begin and end offsets for a delete forward
    method find-delete-forward-range ( buffer -- start-offset end-offset )

    \ Backspace in the buffer
    method do-delete ( buffer -- )

    \ Delete in the buffer
    method do-delete-forward ( buffer -- )

    \ Delete a range in the buffer
    method do-delete-range ( buffer -- )
    
    \ Kill a range in the buffer
    method do-kill ( clip buffer -- )

    \ Copy a range in the buffer
    method do-copy ( clip buffer -- )

    \ Paste in the buffer
    method do-paste ( clip buffer -- )

    \ Is there a selection in the buffer
    method selected? ( buffer -- selected? )
    
    \ Select in the buffer
    method do-select ( buffer -- )

    \ Deselect in the buffer
    method do-deselect ( buffer -- )

    \ Undo in the buffer
    method do-undo ( buffer -- )

    \ Go to the start of the line in the buffer
    method do-line-start ( buffer -- )

    \ Go to the end of the line in the buffer
    method do-line-end ( buffer -- )
    
    \ Page up in buffer
    method do-page-up ( buffer -- )

    \ Page down in buffer
    method do-page-down ( buffer -- )

    \ Home in buffer
    method do-doc-home ( buffer -- )

    \ End in buffer
    method do-doc-end ( buffer -- )

    \ Indent in buffer
    method do-indent ( buffer -- )

    \ Unindent in buffer
    method do-unindent ( buffer -- )

    \ Search forward by one character
    method do-search-forward ( c buffer -- )

    \ Search backward by one character
    method do-search-backward ( c buffer -- )

    \ Delete one character from the search
    method do-search-delete ( buffer -- )

    \ Paste into the search buffer
    method do-search-paste ( clip buffer -- )

    \ Go left one character
    method handle-backward ( buffer -- )

    \ Go right one character
    method handle-forward ( buffer -- )

    \ Go up one character
    method handle-up ( buffer -- )

    \ Go down one character
    method handle-down ( buffer -- )
    
    \ Enter a character into the buffer
    method handle-insert ( c buffer -- )

    \ Enter a newline into the buffer
    method handle-newline ( buffer -- )

    \ Backspace in the buffer
    method handle-delete ( buffer -- )

    \ Delete in the buffer
    method handle-delete-forward ( buffer -- )

    \ Kill a range in the buffer
    method handle-kill ( buffer -- )

    \ Copy a range in the buffer
    method handle-copy ( buffer -- )

    \ Paste in the buffer
    method handle-paste ( buffer -- )
    
    \ Select in the buffer
    method handle-select ( buffer -- )

    \ Undo in the buffer
    method handle-undo ( buffer -- )

    \ Go to line start in the buffer
    method handle-start ( buffer -- )

    \ Go to line end in the buffer
    method handle-end ( buffer -- )

    \ Page up in buffer
    method handle-page-up ( buffer -- )

    \ Page down in buffer
    method handle-page-down ( buffer -- )

    \ Home in buffer
    method handle-doc-home ( buffer -- )

    \ End in buffer
    method handle-doc-end ( buffer -- )
    
    \ Indent in buffer
    method handle-indent ( buffer -- )

    \ Unindent in buffer
    method handle-unindent ( buffer -- )

    \ Find a string forward
    method handle-search-forward ( buffer -- )

    \ Find a string backward
    method handle-search-backward ( buffer -- )
    
  end-class

  \ File buffer class
  <buffer> begin-class <file-buffer>

    \ The file being edited
    <fat32-file> class-size member buffer-file

    \ The file is open
    cell member buffer-file-open

    \ Exception
    cell member file-buffer-exception

    \ File has been accessed
    cell member file-buffer-accessed

    \ Buffer file is valid
    method file-buffer-valid? ( buffer -- valid? )

    \ Buffer file exception
    method file-buffer-exception@ ( buffer -- exception )
    
    \ Access a file, creating or opening it
    method access-file ( path-addr path-bytes buffer -- )

    \ Try to change the file path
    method try-change-file-path
    ( path-addr path-bytes buffer -- exception success? )
    
    \ Load from file
    method load-buffer-from-file ( buffer -- )
    
    \ Write out a file
    method write-buffer-to-file ( buffer -- )

    \ Handle change file path
    method handle-change-file-path ( buffer -- )

    \ Actal do change file path
    method do-change-file-path ( buffer -- )

    \ Handle write
    method handle-write ( buffer -- )

    \ Handle revert
    method handle-revert ( buffer -- )
    
  end-class

  \ Minibuffer class
  <buffer> begin-class <minibuffer>

    \ Are we read-only
    cell member minibuffer-read-only

    \ Minibuffer callback
    cell member minibuffer-callback

    \ Minibuffer callback object
    cell member minibuffer-callback-object

    \ Clear minibuffer
    method clear-minibuffer ( minibuffer -- )

    \ Display message
    method set-message ( addr bytes minibuffer -- )
    
    \ Set prompt
    method set-prompt ( addr bytes object xt minibuffer -- )

    \ Get the length of the input in the minibuffer
    method minibuffer-input-len@ ( minibuffer -- bytes )

    \ Read data from the minibuffer
    method read-minibuffer ( addr bytes offset minibuffer -- bytes' )
    
  end-class
  
  \ Clipboard class
  <object> begin-class <clip>
    
    \ Clipboard dynamic buffer
    <dyn-buffer> class-size member clip-dyn-buffer

    \ Clipboard cursor
    <cursor> class-size member clip-cursor

    \ Get the clip size
    method clip-size@ ( clip -- bytes )
    
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

    \ Minibuffer
    cell member editor-minibuffer

    \ Are we in the minibuffer?
    cell member editor-in-minibuffer

    \ Are we going to exit
    cell member editor-exit
    
    \ Exception
    cell member editor-exception

    \ Are we displaying a search
    cell member editor-searching

    \ Clipboard
    <clip> class-size member editor-clip

    \ Editor is valid
    method editor-valid? ( editor -- valid? )

    \ Editor exception
    method editor-exception@ ( editor -- exception )

    \ Refresh the editor
    method refresh-editor ( editor -- )

    \ Activate minibuffer
    method activate-minibuffer ( editor -- )

    \ Deactivate minibuffer
    method deactivate-minibuffer ( editor -- )

    \ Get currently active buffer
    method active-buffer@ ( editor -- buffer )

    \ Set the cursor for the current buffer
    method update-coord ( editor -- )

    \ Set the cursor if the buffer is not the current buffer
    method update-different-coord ( buffer editor -- )

    \ Display search text if applicable
    method display-search-text ( editor -- )
    
    \ Go left one character
    method handle-editor-backward ( editor -- )

    \ Go right one character
    method handle-editor-forward ( editor -- )

    \ Go up one character
    method handle-editor-up ( editor -- )

    \ Go down one character
    method handle-editor-down ( editor -- )

    \ Find a string forward
    method handle-editor-search-forward ( editor -- )

    \ Find a string backward
    method handle-editor-search-backward ( editor -- )
    
    \ Enter a character into the editor
    method handle-editor-insert ( c editor -- )

    \ Enter a newline into the editor
    method handle-editor-newline ( editor -- )

    \ Backspace in the editor
    method handle-editor-delete ( editor -- )

    \ Delete in the editor
    method handle-editor-delete-forward ( editor -- )

    \ Kill a range in the editor
    method handle-editor-kill ( editor -- )

    \ Copy a range in the editor
    method handle-editor-copy ( editor -- )

    \ Paste in the editor
    method handle-editor-paste ( editor -- )
    
    \ Select in the editor
    method handle-editor-select ( editor -- )

    \ Undo in the editor
    method handle-editor-undo ( editor -- )

    \ Go to the previous buffer
    method handle-editor-prev ( editor -- )

    \ Go to the next buffer
    method handle-editor-next ( editor -- )

    \ Create a new buffer
    method handle-editor-new ( editor -- )

    \ Actually create a new buffer
    method do-editor-new ( editor -- )

    \ Handle change file path
    method handle-editor-change-file-path ( buffer -- )

    \ Write to file
    method handle-editor-write ( editor -- )

    \ Revert from file
    method handle-editor-revert ( editor -- )
    
    \ Handle exit
    method handle-editor-exit ( editor -- )

    \ Handle a prompted exit
    method do-editor-exit ( editor -- )

    \ Handle editor close
    method handle-editor-close ( editor -- )
    
    \ Handle a prompted close
    method do-editor-close ( editor -- )

    \ Close a buffer
    method close-editor-buffer ( buffer editor -- )

    \ Handle editor start
    method handle-editor-start ( editor -- )

    \ Handle editor end
    method handle-editor-end ( editor -- )
    
    \ Handle editor page up
    method handle-editor-page-up ( editor -- )

    \ Handle editor page down
    method handle-editor-page-down ( editor -- )

    \ Handle editor home
    method handle-editor-doc-home ( editor -- )

    \ Handle editor end
    method handle-editor-doc-end ( editor -- )

    \ Handle editor indent
    method handle-editor-indent ( editor -- )

    \ Handle editor unindent
    method handle-editor-unindent ( editor -- )

    \ Handle editor refresh
    method handle-editor-refresh ( editor -- )

    \ Handle editor close
    method handle-editor-close ( editor -- )
    
    \ Handle a special key
    method handle-special ( editor -- )

    \ Handle escape
    method handle-escape ( editor -- )
    
    \ Editor main loop
    method editor-loop ( editor -- )
    
  end-class

  \ Implement buffers
  <buffer> begin-implement
    
    \ Constructor
    :noname { name-addr name-bytes heap editor buffer -- }
      buffer <object>->new
      editor buffer buffer-editor !
      heap buffer buffer-heap !
      name-bytes heap allocate { new-name-addr }
      name-addr new-name-addr name-bytes move
      name-bytes heap allocate { new-path-addr }
      name-addr new-path-addr name-bytes move
      new-name-addr name-bytes buffer buffer-name 2!
      new-path-addr name-bytes buffer buffer-path 2!
      0 buffer buffer-prev !
      0 buffer buffer-next !
      0 buffer buffer-edit-col !
      0 buffer buffer-edit-row !
      0 buffer buffer-undo-count !
      0 buffer buffer-undo-bytes !
      0 buffer buffer-left-bound !
      false buffer buffer-select-enabled !
      false buffer buffer-dirty !
      insert-mode buffer buffer-char-entry-mode !
      0 0 buffer buffer-search-text 2!
      heap <dyn-buffer> buffer buffer-dyn-buffer init-object
      buffer buffer-dyn-buffer <cursor> buffer buffer-display-cursor init-object
      buffer buffer-dyn-buffer <cursor> buffer buffer-edit-cursor init-object
      buffer buffer-dyn-buffer <cursor> buffer buffer-prev-cursor init-object
      buffer buffer-dyn-buffer <cursor> buffer buffer-select-cursor init-object
    ; define new

    \ Destructor
    :noname { buffer -- }
      buffer buffer-search-text 2@ drop ?dup if buffer buffer-heap @ free then
      buffer clear-undos
      buffer buffer-select-cursor destroy
      buffer buffer-prev-cursor destroy
      buffer buffer-edit-cursor destroy
      buffer buffer-display-cursor destroy
      buffer buffer-dyn-buffer destroy
      buffer buffer-name 2@ drop buffer buffer-heap @ free
      buffer buffer-path 2@ drop buffer buffer-heap @ free
      buffer <object>->destroy
    ; define destroy

    \ Update the previous cursor
    :noname ( buffer -- )
      dup buffer-edit-cursor swap buffer-prev-cursor copy-cursor
    ; define update-prev-cursor
    
    \ Dirty buffer
    :noname ( buffer -- )
      true swap buffer-dirty !
    ; define dirty-buffer

    \ Clean buffer
    :noname ( buffer -- )
      false swap buffer-dirty !
    ; define clean-buffer

    \ Leave search
    :noname { buffer -- }
      insert-mode buffer buffer-char-entry-mode !
      buffer buffer-search-text 2@ drop ?dup if buffer buffer-heap @ free then
      0 0 buffer buffer-search-text 2!
    ; define leave-search

    \ Get whether a buffer is being searched
    :noname { buffer -- searching? }
      buffer buffer-char-entry-mode @
      dup search-forward-mode = swap search-backward-mode = or
    ; define searching?

    \ Get whether a buffer is being searched forward
    :noname ( buffer -- searching-forward? )
      buffer-char-entry-mode @ search-forward-mode =
    ; define searching-forward?

    \ Get whether a buffer is being searched backward
    :noname ( buffer -- searching-backward? )
      buffer-char-entry-mode @ search-backward-mode =
    ; define searching-backward?
    
    \ Get the buffer search text
    :noname ( buffer -- addr len )
      buffer-search-text 2@
    ; define search-text@
    
    \ Buffer width in characters
    :noname ( buffer -- width )
      drop display-width @
    ; define buffer-width@

    \ Buffer height in characters
    :noname ( buffer --  height )
      drop display-height @ 2 -
    ; define buffer-height@

    \ Set buffer cursor
    :noname { buffer -- }
      buffer buffer-edit-row @ buffer buffer-edit-col @
      buffer go-to-buffer-coord
    ; define set-buffer-coord
    
    \ Go to buffer coordinate
    :noname ( row col buffer -- )
      drop swap 1+ swap go-to-coord
    ; define go-to-buffer-coord

    \ Clear the undos
    :noname { buffer -- }
      begin buffer newest-undo@ while buffer drop-undo repeat
    ; define clear-undos
    
    \ Get the oldest undo
    :noname { buffer -- undo }
      buffer buffer-undo-count @ 0> if
        buffer buffer-undo-array buffer buffer-undo-count @ 1- cells + @
      else
        0
      then
    ; define oldest-undo@

    \ Get the newest undo
    :noname { buffer -- undo }
      buffer buffer-undo-count @ 0> if
        buffer buffer-undo-array @
      else
        0
      then
    ; define newest-undo@

    \ Push an undo
    :noname { bytes buffer -- undo }
      bytes [ undo-header-size cell+ ] literal + { full-bytes }
      full-bytes buffer ensure-undo-space
      buffer buffer-undo-array buffer buffer-undo-array cell+
      buffer buffer-undo-count @ cells move
      full-bytes buffer buffer-undo-bytes +!
      1 buffer buffer-undo-count +!
      bytes undo-header-size + buffer buffer-heap @ allocate { undo }
      undo buffer buffer-undo-array !
      undo
    ; define push-undo

    \ Drop an undo
    :noname { buffer -- }
      buffer buffer-undo-count @ 0> if
        buffer newest-undo@ buffer free-undo
        buffer buffer-undo-array cell+ buffer buffer-undo-array
        buffer buffer-undo-count @ cells move
      then
    ; define drop-undo
    
    \ Free an undo
    :noname { undo buffer -- }
      undo undo-size @ 0 max [ undo-header-size cell+ ] literal + negate
      buffer buffer-undo-bytes +!
      -1 buffer buffer-undo-count +!
      undo buffer buffer-heap @ free
    ; define free-undo
    
    \ Make room for undos
    :noname { bytes buffer -- }
      begin
        max-undo-byte-count buffer buffer-undo-bytes @ bytes + <
        buffer buffer-undo-count @ 0> and
        buffer buffer-undo-count @ max-undo-count = or
      while
        buffer oldest-undo@ buffer free-undo
      repeat
    ; define ensure-undo-space

    \ Add an insert undo
    :noname ( start-offset end-offset insert-offset buffer -- )
      dup buffer-dyn-buffer <cursor> [:
        default-segment-size [:
          { start-offset end-offset insert-offset buffer cursor data }
          end-offset start-offset - { bytes }
          bytes buffer push-undo { undo }
          start-offset cursor go-to-offset
          0 { data-offset }
          begin data-offset bytes < while
            bytes data-offset - default-segment-size min { part-size }
            data part-size cursor read-data to part-size
            data undo undo-header-size + data-offset + part-size move
            part-size +to data-offset
          repeat
          bytes undo undo-size !
          insert-offset undo undo-offset !
        ;] with-allot
      ;] with-object
    ; define add-insert-undo

    \ Add a delete undo
    :noname { bytes offset buffer -- }
      buffer newest-undo@ { undo }
      undo if
        undo undo-size @ 0<
        undo undo-size @ bytes - delete-undo-accumulation >= and
        offset bytes - undo undo-offset @ = and
      else
        false
      then if
        bytes negate undo undo-size +!
      else
        0 buffer push-undo to undo
        bytes negate undo undo-size !
      then
      offset undo undo-offset !
    ; define add-delete-undo

    \ Check whether any lines in a range are not indented
    :noname ( cursor offset1 buffer -- not-indented? )
      dup buffer-dyn-buffer <cursor> [: { orig-cursor offset1 buffer cursor }
        orig-cursor cursor copy-cursor
        begin cursor offset@ offset1 < while
          cursor buffer cursor-at dup bl <> swap tab <> and if true exit then
          [: newline = ;] cursor find-next
          1 cursor adjust-offset
        repeat
        false
      ;] with-object
    ; define not-indented?

    \ Is there a tab in the indentation?
    :noname ( cursor buffer -- tab-in-indent? )
      dup buffer-dyn-buffer <cursor> [: { orig-cursor buffer cursor }
        orig-cursor cursor copy-cursor
        begin cursor offset@ buffer buffer-len@ < while
          cursor buffer cursor-at { byte }
          byte tab = if
            true exit
          else
            byte bl <> if
              false exit
            then
          then
          1 cursor adjust-offset
        repeat
        false
      ;] with-object
    ; define cursor-tab-in-indent?

    \ Get the number of indentation bytes
    :noname ( cursor buffer -- indent-bytes )
      buffer-dyn-buffer <cursor> [: { orig-cursor cursor }
        orig-cursor cursor copy-cursor
        cursor offset@ { offset0 }
        [: dup tab <> swap bl <> and ;] cursor find-next
        cursor offset@ offset0 -
      ;] with-object
    ; define cursor-line-indent-bytes

    \ Get a line's indentation
    :noname ( cursor buffer -- indent )
      dup buffer-dyn-buffer <cursor> [: { orig-cursor buffer cursor }
        orig-cursor cursor copy-cursor
        [: newline = ;] cursor find-prev
        0 { chars }
        begin cursor offset@ buffer buffer-len@ < while
          cursor buffer cursor-at { byte }
          byte bl = if
            1 +to chars
            1 cursor adjust-offset
          else
            byte tab = if
              zeptoed-tab-size chars zeptoed-tab-size umod - +to chars
              1 cursor adjust-offset
            else
              chars exit
            then
          then
        repeat
        chars
      ;] with-object
    ; define cursor-line-indent

    \ Get the edit cursor's line's indentation
    :noname ( cursor buffer -- indent )
      dup buffer-edit-cursor swap cursor-line-indent
    ; define edit-cursor-line-indent

    \ Get the number of continguous space characters at a cursor
    :noname ( cursor buffer -- chars )
      buffer-dyn-buffer <cursor> [: { orig-cursor cursor }
        orig-cursor cursor copy-cursor
        0 { W^ data }
        0 { count }
        begin
          data 1 cursor read-data if
            data c@ bl = if
              1 +to count false
            else
              true
            then
          else
            true
          then
        until
        count
      ;] with-object
    ; define cursor-space-chars

    \ Get whether two cursors share the same preceding newline
    :noname ( cursor0 cursor1 buffer -- same? )
      buffer-dyn-buffer <cursor> [: { cursor0 cursor1 cursor }
        cursor0 cursor copy-cursor
        [: newline = ;] cursor find-prev
        cursor offset@ { offset0 }
        cursor1 cursor copy-cursor
        [: newline = ;] cursor find-prev
        cursor offset@ { offset1 }
        offset0 offset1 =
      ;] with-object
    ; define cursors-before-same-newline?

    \ Get whether two cursors are on the same line
    :noname { cursor0 cursor1 buffer -- same? }
      cursor0 cursor1 buffer cursors-before-same-newline? if
        cursor0 buffer cursor-start-dist buffer buffer-width@ u/
        cursor1 buffer cursor-start-dist buffer buffer-width@ u/ =
      else
        false
      then
    ; define cursors-on-same-line?

    \ Get the distance between two offsets
    :noname ( init-chars offset0 offset1 buffer -- distance )
      buffer-dyn-buffer <cursor> [:
        default-segment-size [: { init-chars offset0 offset1 cursor data }
          offset0 offset1 min { start-offset }
          offset0 offset1 max { end-offset }
          start-offset cursor go-to-offset
          init-chars { chars }
          begin
            end-offset cursor offset@ - default-segment-size min 0 max
            { part-size }
            part-size 0> if
              data part-size cursor read-data to part-size
              chars data part-size string-len to chars
              false
            else
              true
            then
          until
          chars init-chars - offset1 offset0 < if negate then
        ;] with-allot
      ;] with-object
    ; define char-dist

    \ Get the byte before a cursor
    :noname { cursor buffer -- c|0 }
      0 { W^ data }
      data 1 cursor read-data-before-w/o-move if
        c@
      else
        drop 0
      then
    ; define cursor-before

    \ Get the byte at a cursor
    :noname { cursor buffer -- c|0 }
      0 { W^ data }
      data 1 cursor read-data-w/o-move if
        data c@
      else
        0
      then
    ; define cursor-at
    
    \ Get the byte before the edit cursor
    :noname ( buffer -- c|0 )
      dup buffer-edit-cursor swap cursor-before
    ; define edit-cursor-before

    \ Get the byte at the edit cursor
    :noname ( buffer -- c|0 )
      dup buffer-edit-cursor swap cursor-at
    ; define edit-cursor-at
    
    \ Length in spaces of the character before the edit cursor
    :noname ( buffer -- spaces )
      dup edit-cursor-before
      tab = if
        dup buffer-dyn-buffer <cursor> [: { buffer cursor }
          buffer buffer-edit-cursor cursor copy-cursor
          -1 cursor adjust-offset
          cursor buffer cursor-start-dist { init-chars }
          init-chars cursor offset@ buffer buffer-edit-cursor offset@
          buffer char-dist
        ;] with-object
      else
        drop 1
      then
    ; define edit-cursor-before-spaces

    \ Length in spaces of the character at the edit cursor
    :noname ( buffer -- spaces )
      dup edit-cursor-at tab = if
        dup buffer-dyn-buffer <cursor> [: { buffer cursor }
          buffer buffer-edit-cursor buffer cursor-end-dist { first-dist }
          buffer buffer-edit-cursor cursor copy-cursor
          1 cursor adjust-offset
          first-dist cursor buffer cursor-end-dist -
        ;] with-object
      else
        drop 1
      then
    ; define edit-cursor-at-spaces

    \ Find the raw distance of a cursor from the previous newline
    :noname ( orig-cursor buffer -- distance )
      buffer-dyn-buffer <cursor> [: { orig-cursor cursor }
        orig-cursor cursor copy-cursor
        cursor offset@ { orig-offset }
        [: newline = ;] cursor find-prev
        orig-offset cursor offset@ -
      ;] with-object
    ; define cursor-raw-start-dist

    \ Find the distance of a cursor from the previous newline
    :noname ( orig-cursor buffer -- distance )
      dup buffer-dyn-buffer <cursor> [: { orig-cursor buffer cursor }
        orig-cursor cursor copy-cursor
        cursor offset@ { orig-offset }
        [: newline = ;] cursor find-prev
        0 cursor offset@ orig-offset buffer char-dist
      ;] with-object
    ; define cursor-start-dist

    \ Find the raw distance of a cursor from the next newline
    :noname ( orig-cursor buffer -- distance )
      buffer-dyn-buffer <cursor> [: { orig-cursor cursor }
        orig-cursor cursor copy-cursor
        cursor offset@ { orig-offset }
        [: newline = ;] cursor find-next
        cursor offset@ orig-offset -
      ;] with-object
    ; define cursor-raw-end-dist
    
    \ Find the distance of a cursor from the next newline
    :noname ( orig-cursor buffer -- distance )
      dup buffer-dyn-buffer <cursor> [: { orig-cursor buffer cursor }
        orig-cursor buffer cursor-start-dist { init-chars }
        orig-cursor cursor copy-cursor
        cursor offset@ { orig-offset }
        [: newline = ;] cursor find-next
        init-chars orig-offset cursor offset@ buffer char-dist
      ;] with-object
    ; define cursor-end-dist

    \ Get a cursor's distance from the left-hand side of the display
    :noname { cursor buffer -- col row }
      cursor buffer cursor-start-dist dup buffer buffer-width@ umod
      swap buffer buffer-width@ u/
    ; define cursor-left-space

    \ Get the length of a line a cursor is on
    :noname ( cursor buffer -- length )
      2dup cursor-start-dist -rot cursor-end-dist +
    ; define cursor-line-len

    \ Get the number of rows making up a line
    :noname { cursor buffer -- rows }
      cursor buffer cursor-line-len { len }
      len buffer buffer-width@ u/mod swap if 1+ then
    ; define cursor-line-rows
    
    \ Get the length of the last row of a line
    :noname { cursor buffer -- length }
      cursor buffer cursor-line-len buffer buffer-width@ umod
    ; define cursor-line-last-row-len

    \ Is a cursor in the first row of a line?
    :noname { cursor buffer -- first? }
      cursor buffer cursor-start-dist buffer buffer-width@ <
    ; define cursor-line-first?
    
    \ Is a cursor in the last row of a line?
    :noname { cursor buffer -- last? }
      cursor buffer cursor-line-last-row-len { len }
      cursor buffer cursor-end-dist { dist }
      len buffer buffer-width@ < if dist len <= else dist 0= then
    ; define cursor-line-last?

    \ Edit cursor left space
    :noname ( buffer -- cols rows )
      dup buffer-edit-cursor swap cursor-left-space
    ; define edit-cursor-left-space

    \ Get the number of rows making up the current edit line
    :noname ( buffer -- rows )
      dup buffer-edit-cursor swap cursor-line-rows
    ; define edit-cursor-line-rows

    \ Get whether the current line takes up the full width
    :noname { buffer -- full-width? }
      buffer buffer-edit-cursor buffer cursor-line-len { len }
      len buffer buffer-width@ u/mod swap 0= if 0> then
    ; define edit-cursor-full-width?
    
    \ Edit cursor offset
    :noname ( buffer -- offset )
      buffer-edit-cursor offset@
    ; define edit-cursor-offset@

    \ Buffer content length
    :noname ( buffer -- length )
      buffer-dyn-buffer dyn-buffer-len@
    ; define buffer-len@

    \ Get whether a cursor is on a special line
    :noname ( cursor buffer -- special? )
      dup buffer-dyn-buffer <cursor> [: { orig-cursor buffer cursor }
        orig-cursor cursor copy-cursor
        [:
          dup tab = over unicode? or over bl < or over delete = or
          swap newline = or
        ;] cursor find-prev
        cursor offset@ { check-offset }
        orig-cursor cursor copy-cursor
        [: newline = ;] cursor find-prev
        cursor offset@ check-offset <> { checked-before }
        orig-cursor cursor copy-cursor
        [:
          dup tab = over unicode? or over bl < or over delete = or
          swap newline = or
        ;] cursor find-next
        cursor offset@ to check-offset
        orig-cursor cursor copy-cursor
        [: newline = ;] cursor find-next
        cursor offset@ check-offset <> checked-before or
      ;] with-object
    ; define cursor-special-line?

    \ Get whether the edit cursor is on a special line
    :noname ( buffer -- special? )
      dup buffer-edit-cursor swap cursor-special-line?
    ; define edit-cursor-special-line?
    
    \ Are we at the start of a line?
    :noname ( buffer -- start? )
      dup buffer-edit-cursor swap cursor-start-dist 0=
    ; define edit-cursor-at-start?

    \ Are we at the start of a row?
    :noname ( buffer -- row-start? )
      dup buffer-edit-cursor swap cursor-left-space drop 0=
    ; define edit-cursor-at-row-start?

    \ Are we at the end of a line?
    :noname ( buffer -- end? )
      dup buffer-edit-cursor swap cursor-end-dist 0=
    ; define edit-cursor-at-end?

    \ Are we at the end of a row?
    :noname { buffer -- row-end? }
      buffer buffer-edit-cursor buffer cursor-left-space drop
      buffer buffer-width@ 1- =
    ; define edit-cursor-at-row-end?

    \ Is the edit cursor in the last row of a line
    :noname ( buffer -- last? )
      dup buffer-edit-cursor swap cursor-line-last?
    ; define edit-cursor-line-last?

    \ Is the edit cursor in a single row line?
    :noname { buffer -- single-row? }
      buffer buffer-edit-cursor buffer cursor-line-len buffer buffer-width@ <
    ; define edit-cursor-single-row?

    \ Is the edit cursor at the start of the last row of a line
    :noname ( buffer -- last-first? )
      dup edit-cursor-line-last? if
        dup buffer-edit-cursor swap cursor-left-space drop 0=
      else
        drop false
      then
    ; define edit-cursor-line-last-first?

    \ Is a cursor in the first row of the first line
    :noname { cursor buffer -- first-row? }
      cursor buffer cursor-left-space nip 0=
      cursor buffer cursor-raw-start-dist cursor offset@ = and
    ; define cursor-first-row?

    \ Is a cursor in the last row of the last line
    :noname { cursor buffer -- last-row? }
      cursor offset@ cursor buffer cursor-raw-end-dist +
      buffer buffer-dyn-buffer dyn-buffer-len@ = if
        cursor buffer cursor-line-last?
      else
        false
      then
    ; define cursor-last-row?

    \ Is the edit cursor in the first row of the first line
    :noname ( buffer -- first-row? )
      dup buffer-edit-cursor swap cursor-first-row?
    ; define edit-cursor-first-row?

    \ Is the edit cursor in the last row of the last line
    :noname ( buffer -- last-row? )
      dup buffer-edit-cursor swap cursor-last-row?
    ; define edit-cursor-last-row?

    \ Inner workings of searching forward
    :noname { addr count cursor buffer -- found? }
      begin
        0 { W^ data }
        data 1 cursor read-data-w/o-move if
          addr 1 data 1
          addr count contains-upper? if
            equal-strings?
          else
            equal-case-strings?
          then
          if
            cursor addr count dup [: { cursor addr count data }
              data count cursor read-data-w/o-move count = if
                addr count data count
                addr count contains-upper? if
                  equal-strings?
                else
                  equal-case-strings?
                then
                if
                  true true
                else
                  1 cursor adjust-offset false
                then
              else
                false true
              then
            ;] with-allot
          else
            1 cursor adjust-offset false
          then
        else
          false true
        then
      until
    ; define cursor-search-forward

    \ Inner workings of searching backward
    :noname { addr count cursor buffer -- found? }
      cursor offset@ 0= if false exit then
      cursor offset@ buffer buffer-dyn-buffer dyn-buffer-len@ = if
        -1 cursor adjust-offset
      then
      begin
        0 { W^ data }
        data 1 cursor read-data-w/o-move if
          addr 1 data 1
          addr count contains-upper? if
            equal-strings?
          else
            equal-case-strings?
          then
          if
            cursor addr count dup [: { cursor addr count data }
              data count cursor read-data-w/o-move count = if
                addr count data count
                addr count contains-upper? if
                  equal-strings?
                else
                  equal-case-strings?
                then
                if
                  true true
                else
                  cursor offset@ 0= if
                    false true
                  else
                    -1 cursor adjust-offset false
                  then
                then
              else
                false true
              then
            ;] with-allot
          else
            cursor offset@ 0= if
              false true
            else
              -1 cursor adjust-offset false
            then
          then
        else
          false true
        then
      until
    ; define cursor-search-backward
    
    \ Search forward
    :noname ( addr count buffer -- )
      over 0= if 2drop drop exit then
      dup buffer-dyn-buffer <cursor> [: { addr count buffer cursor }
        buffer buffer-edit-cursor cursor copy-cursor
        addr count cursor buffer cursor-search-forward
        if cursor buffer buffer-edit-cursor copy-cursor then
      ;] with-object
    ; define edit-cursor-search-forward

    \ Search backward
    :noname ( addr count buffer -- )
      over 0= if 2drop drop exit then
      dup buffer-dyn-buffer <cursor> [: { addr count buffer cursor }
        buffer buffer-edit-cursor cursor copy-cursor
        addr count cursor buffer cursor-search-backward
        if cursor buffer buffer-edit-cursor copy-cursor then
      ;] with-object
    ; define edit-cursor-search-backward

    \ Continue searching forward
    :noname ( addr count buffer -- )
      over 0= if 2drop drop exit then
      dup buffer-dyn-buffer <cursor> [: { addr count buffer cursor }
        buffer buffer-edit-cursor cursor copy-cursor
        1 cursor adjust-offset
        addr count cursor buffer cursor-search-forward
        if cursor buffer buffer-edit-cursor copy-cursor then
      ;] with-object
    ; define edit-cursor-continue-forward

    \ Continue searching backward
    :noname ( addr count buffer -- )
      over 0= if 2drop drop exit then
      dup buffer-dyn-buffer <cursor> [: { addr count buffer cursor }
        buffer buffer-edit-cursor cursor copy-cursor
        -1 cursor adjust-offset
        addr count cursor buffer cursor-search-backward
        if cursor buffer buffer-edit-cursor copy-cursor then
      ;] with-object
    ; define edit-cursor-continue-backward

    \ Add character to search text
    :noname ( c buffer -- )
      dup buffer-search-text 2@ nip zeptoed-max-search-size = if
        2drop exit
      then
      dup buffer-search-text 2@ nip 1+ [: { c buffer data }
        buffer buffer-search-text 2@ { orig-data len }
        orig-data if orig-data data len move then
        c data len + c!
        orig-data if orig-data buffer buffer-heap @ free then
        len 1+ buffer buffer-heap @ allocate { new-data }
        data new-data len 1+ move
        new-data len 1+ buffer buffer-search-text 2!
      ;] with-allot
    ; define add-search-text-char

    \ Add string to search text
    :noname ( addr count buffer -- )
      dup buffer-search-text 2@ nip zeptoed-max-search-size swap -
      rot min swap
      over 0= if 2drop drop exit then
      dup buffer-search-text 2@ nip over + [: { addr count buffer data }
        buffer buffer-search-text 2@ { orig-data len }
        orig-data if orig-data data len move then
        addr data len + count move
        orig-data if orig-data buffer buffer-heap @ free then
        len count + buffer buffer-heap @ allocate { new-data }
        data new-data len count + move
        new-data len count + buffer buffer-search-text 2!
      ;] with-allot
    ; define add-search-text

    \ Delete character from search text
    :noname { buffer -- }
      buffer buffer-search-text 2@ nip 1 > if
        buffer dup buffer-search-text 2@ nip 1- [: { buffer data }
          buffer buffer-search-text 2@ { orig-data len }
          orig-data data len 1- move
          orig-data buffer buffer-heap @ free
          len 1- buffer buffer-heap @ allocate { new-data }
          data new-data len 1- move
          new-data len 1- buffer buffer-search-text 2!
        ;] with-allot
      else
        buffer buffer-search-text 2@ nip 1 = if
          buffer buffer-search-text 2@ drop buffer buffer-heap @ free
          0 0 buffer buffer-search-text 2!
        then
      then
    ; define delete-search-text-char

    \ Output buffer title
    :noname { buffer -- }
      0 0 go-to-coord
      buffer buffer-dirty @ if -2 else 0 then { dirty-offset }
      buffer buffer-name 2@ dup buffer buffer-width@ dirty-offset + < if
        type buffer buffer-dirty @ if ."  *" then erase-end-of-line
      else
        dup buffer buffer-width@ dirty-offset + = if
          type buffer buffer-dirty @ if ."  *" then
        else
          + buffer buffer-width@ dirty-offset + -
          buffer buffer-width@ dirty-offset +
          buffer buffer-dirty @ if ."  *" then
        then
      then
    ; define output-title
    
    \ Refresh the current line
    :noname ( buffer -- )
      dup buffer-dyn-buffer <cursor> [:
        default-segment-size [: { buffer cursor data }
          buffer buffer-edit-cursor cursor copy-cursor
          buffer buffer-prev-cursor buffer cursor-start-dist
          buffer buffer-width@ u/ { before-rows }
          [: newline = ;] cursor find-prev
          cursor buffer cursor-raw-end-dist { end-dist }
          hide-cursor
          buffer buffer-edit-row @ before-rows - 0 buffer go-to-buffer-coord
          end-dist { len }
          0 { chars }
          begin len 0> while
            len default-segment-size min { part-size }
            cursor offset@ { prev-offset }
            data part-size cursor read-data { real-size }
            real-size 0 ?do
              data i + c@ { byte }
              byte tab = if
                zeptoed-tab-size chars zeptoed-tab-size umod - { tab-spaces }
                tab-spaces spaces
                tab-spaces +to chars
              else
                byte emit
                byte bl >= byte delete < and
                byte unicode-start? or if
                  1 +to chars
                then
              then
            loop
            part-size negate +to len
          repeat
          chars buffer buffer-width@ umod if erase-end-of-line then

          buffer buffer-edit-cursor buffer cursor-start-dist
          buffer buffer-width@ u/mod { after-cols after-rows }
          buffer buffer-edit-row @ before-rows - after-rows +
          buffer buffer-edit-row !
          after-cols buffer buffer-edit-col !
          
          buffer buffer-editor @ update-coord
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
          buffer buffer-height@ { rows-remaining }
          buffer buffer-width@ { cols-remaining }
          0 0 { edit-col edit-row }
          buffer buffer-edit-cursor offset@ { edit-offset }
          buffer buffer-select-cursor offset@ { select-offset }
          hide-cursor
          normal-video
          buffer output-title
          0 0 buffer go-to-buffer-coord
          buffer buffer-select-enabled @ if
            select-offset display-offset <= if
              inverse-video
            then
          then
          begin
            rows-remaining 0> if
              data default-segment-size data-cursor read-data { actual-bytes }
              data { current-data }
              actual-bytes 0> if
                begin
                  actual-bytes 0> rows-remaining 0> and if
                    buffer buffer-select-enabled @
                    select-offset display-offset = and if
                      edit-offset display-offset <= if
                        normal-video
                      else
                        inverse-video
                      then
                    then
                    edit-offset display-offset = if
                      buffer buffer-select-enabled @ if
                        select-offset display-offset <= if
                          normal-video
                        else
                          inverse-video
                        then
                      then
                      cols-remaining 0 > if
                        buffer buffer-height@ rows-remaining - to edit-row
                        buffer buffer-width@ cols-remaining - to edit-col
                      else
                        buffer buffer-height@ rows-remaining - 1+ to edit-row
                        0 to edit-col
                      then
                    then
                    current-data c@ { byte }
                    byte newline = if
                      erase-end-of-line cr
                      -1 +to rows-remaining
                      buffer buffer-width@ to cols-remaining
                    else
                      byte tab = if
                        buffer buffer-width@ cols-remaining - { chars }
                        zeptoed-tab-size chars zeptoed-tab-size umod -
                        { tab-spaces }
                        tab-spaces spaces
                        tab-spaces negate +to cols-remaining
                        begin cols-remaining 0 <= while
                          rows-remaining 0 > if
                            cr
                            -1 +to rows-remaining
                          then
                          buffer buffer-width@ +to cols-remaining
                        repeat
                      else
                        byte emit
                        byte bl >= byte delete < and
                        byte unicode-start? or if
                          -1 +to cols-remaining
                          cols-remaining 0 <= if
                            rows-remaining 0 > if
                              cr
                              -1 +to rows-remaining
                            then
                            buffer buffer-width@ +to cols-remaining
                          then
                        then
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
            else
              true
            then
          until
          normal-video
          erase-end-of-line
          rows-remaining 1 > if
            rows-remaining 1- 0 ?do cr erase-end-of-line loop
          then
          edit-offset display-offset = if
            cols-remaining 0 > if
              buffer buffer-height@ rows-remaining - to edit-row
              buffer buffer-width@ cols-remaining - to edit-col
            else
              buffer buffer-height@ rows-remaining - 1+ to edit-row
              0 to edit-col
            then
          then
          edit-row buffer buffer-edit-row !
          edit-col buffer buffer-edit-col !
          buffer buffer-editor @ update-coord
          show-cursor
        ;] with-object
      ;] with-allot
    ; define refresh-display

    \ Center the display
    :noname ( center-cursor dest-cursor buffer -- )
      dup buffer-dyn-buffer <cursor> [:
        { center-cursor dest-cursor buffer cursor }
        center-cursor cursor copy-cursor
        [: newline = ;] cursor find-prev
        0 { rows }
        begin
          rows buffer buffer-height@ 2 / < cursor offset@ 0> and
        while
          -1 cursor adjust-offset
          cursor offset@ { old-offset }
          [: newline = ;] cursor find-prev
          cursor offset@ { new-offset }
          0 new-offset old-offset buffer char-dist { diff }
          diff 0> if
            diff buffer buffer-width@ u/
            diff buffer buffer-width@ umod 0> if 1+ then
          else
            1
          then { new-rows }
          rows new-rows + buffer buffer-height@ 2 / <= if
            new-rows +to rows
          else
            dest-cursor cursor copy-cursor
            diff buffer buffer-width@ umod negate cursor adjust-offset
            1 +to rows
            diff buffer buffer-width@ u/ 0> if
              buffer buffer-height@ 2 / rows -
              buffer buffer-width@ * negate cursor adjust-offset
              new-rows 1- +to rows
            then
          then
          cursor dest-cursor copy-cursor
        repeat
        cursor dest-cursor copy-cursor
      ;] with-object
    ; define center-cursor

    \ Put a cursor on the same row as a cursor
    :noname { src-cursor dest-cursor buffer -- }
      src-cursor dest-cursor copy-cursor
      dest-cursor buffer cursor-left-space drop { cols }
      begin cols 0> while
        dest-cursor buffer cursor-start-dist { start-dist }
        -1 dest-cursor adjust-offset
        dest-cursor buffer cursor-start-dist { new-dist }
        new-dist start-dist - +to cols
      repeat
    ; define same-row-cursor
    
    \ Get the number of rows between two cursors
    :noname ( cursor0 cursor1 buffer -- rows )
      dup buffer-dyn-buffer <cursor> [: { cursor0 cursor1 buffer cursor }
        cursor1 cursor copy-cursor
        [: newline = ;] cursor find-prev
        0 { rows }
        begin
          cursor offset@ cursor0 offset@ >
        while
          -1 cursor adjust-offset
          cursor offset@ { old-offset }
          [: newline = ;] cursor find-prev
          cursor offset@ { new-offset }
          0 new-offset old-offset buffer char-dist { diff }
          new-offset cursor0 offset@ >= if
            diff 0> if
              diff buffer buffer-width@ u/
              diff buffer buffer-width@ umod 0> if 1+ then
            else
              1
            then
            +to rows
          else
            0 cursor0 offset@ old-offset buffer char-dist to diff
            diff buffer buffer-width@ umod 0> if 1 +to rows then
            diff buffer buffer-width@ u/ +to rows
          then
        repeat
        rows
      ;] with-object
    ; define rows-between-cursors
    
    \ Update the display
    :noname { buffer -- update? }
      buffer buffer-edit-cursor { edit-cursor }
      buffer buffer-display-cursor { display-cursor }
      edit-cursor offset@ display-cursor offset@ < if
        edit-cursor display-cursor buffer center-cursor true
      else
        display-cursor edit-cursor buffer rows-between-cursors
        dup buffer buffer-height@ 3 / <
        swap buffer buffer-height@ 2 * 3 / > or if
          display-cursor offset@ { old-offset }
          edit-cursor display-cursor buffer center-cursor
          display-cursor offset@ { new-offset }
          old-offset new-offset <>
        else
          false
        then
      then
      buffer buffer-select-enabled @ or
    ; define update-display

    \ Display a single character
    :noname { c buffer -- }
      c emit
      1 buffer buffer-edit-col +!
      buffer buffer-edit-col @ buffer buffer-width@ = if
        0 buffer buffer-edit-col !
        1 buffer buffer-edit-row +!
      then
      buffer buffer-editor @ update-coord
    ; define output-char

    \ Backspace a single char
    :noname { buffer -- }
      buffer buffer-edit-col @ 0> if
        hide-cursor
        $08 emit
        space
        $08 emit
        show-cursor
        -1 buffer buffer-edit-col +!
        buffer buffer-editor @ update-coord
      else
        buffer output-prev-row
        space
        buffer buffer-editor @ update-coord
      then
    ; define output-backspace

    \ Move the cursor to the left
    :noname { spaces buffer -- }
      spaces negate buffer buffer-edit-col +!
      buffer buffer-editor @ update-coord
    ; define output-left

    \ Move the cursor to the end of the previous row
    :noname { buffer -- }
      -1 buffer buffer-edit-row +!
      buffer buffer-width@ 1- buffer buffer-edit-col !
      buffer buffer-editor @ update-coord
    ; define output-prev-row

    \ Move the cursor to the end of the previous line
    :noname { buffer -- }
      -1 buffer buffer-edit-row +!
      buffer edit-cursor-left-space drop buffer buffer-edit-col !
      buffer buffer-editor @ update-coord
    ; define output-prev-line

    \ Move the cursor to the right
    :noname { spaces buffer -- }
      spaces buffer buffer-edit-col +!
      buffer buffer-editor @ update-coord
    ; define output-right

    \ Move the cursor to the start of the next row
    :noname { buffer -- }
      1 buffer buffer-edit-row +!
      0 buffer buffer-edit-col !
      buffer buffer-editor @ update-coord
    ; define output-next-row

    \ Move the cursor to the start of the next line
    :noname { buffer -- }
      1 buffer buffer-edit-row +!
      0 buffer buffer-edit-col !
      buffer buffer-editor @ update-coord
    ; define output-next-line

    \ Move the cursor to the last position in a buffer
    :noname { buffer -- }
      buffer edit-cursor-left-space { cols rows }
      buffer buffer-edit-cursor buffer cursor-end-dist { dist }
      cols dist + buffer buffer-edit-col !
      buffer buffer-editor @ update-coord
    ; define output-last

    \ Move the cursor to the previous line
    :noname { buffer -- }
\      buffer buffer-prev-cursor buffer cursor-line-first? if
        buffer buffer-edit-cursor buffer cursor-start-dist
        buffer buffer-width@ umod { after-cols }
        buffer buffer-prev-cursor buffer cursor-first-row? not if
          -1 buffer buffer-edit-row +!
        then
        after-cols buffer buffer-edit-col !
\      else
\        buffer buffer-prev-cursor buffer cursor-start-dist
\        buffer buffer-width@ u/ { before-rows }
\        buffer buffer-edit-cursor buffer cursor-start-dist
\        buffer buffer-width@ u/mod { after-cols after-rows }
\        buffer buffer-edit-row @ before-rows - after-rows +
\        buffer buffer-edit-row !
\        after-cols buffer buffer-edit-col !
\      then
      buffer buffer-editor @ update-coord
    ; define output-up
    
    \ Move the cursor to the next line
    :noname { buffer -- }
\      buffer buffer-prev-cursor buffer cursor-line-last? if
        buffer buffer-edit-cursor buffer cursor-start-dist
        buffer buffer-width@ umod { after-cols }
        buffer buffer-prev-cursor buffer cursor-last-row? not if
          1 buffer buffer-edit-row +!
        then
        after-cols buffer buffer-edit-col !
\      else
\        buffer buffer-prev-cursor buffer cursor-start-dist
\        buffer buffer-width@ u/ { before-rows }
\        buffer buffer-edit-cursor buffer cursor-start-dist
\        buffer buffer-width@ u/mod { after-cols after-rows }
\        buffer buffer-edit-row @ before-rows - after-rows +
\        buffer buffer-edit-row !
\        after-cols buffer buffer-edit-col !
\      then
      buffer buffer-editor @ update-coord
    ; define output-down

    \ Go to the start of a Unicode character
    :noname { buffer -- }
      buffer edit-cursor-at { byte }
      byte unicode? byte unicode-start? not and if
        begin
          -1 buffer buffer-edit-cursor adjust-offset
          buffer buffer-edit-cursor offset@ 0> if
            buffer edit-cursor-at to byte
            byte unicode? not byte unicode-start? or
          else
            true
          then
        until
      then
    ; define go-to-unicode-start

    \ Go to the end of a Unicode character
    :noname { buffer -- }
      buffer edit-cursor-at { byte }
      byte unicode? byte unicode-start? not and if
        begin
          1 buffer buffer-edit-cursor adjust-offset
          buffer buffer-edit-cursor offset@ buffer buffer-len@ < if
            buffer edit-cursor-at to byte
            byte unicode? not byte unicode-start? or
          else
            true
          then
        until
      then
    ; define go-to-unicode-end
    
    \ Go to the first position in a buffer
    :noname ( buffer -- )
      dup buffer-left-bound @ ?dup if
        swap buffer-edit-cursor go-to-offset
      else
        buffer-edit-cursor go-to-start
      then
    ; define do-first

    \ Go to the last position in a buffer
    :noname ( buffer -- )
      dup buffer-dyn-buffer dyn-buffer-len@ swap buffer-edit-cursor go-to-offset
    ; define do-last
    
    \ Move the edit cursor left by one character
    :noname { buffer -- }
      buffer buffer-edit-cursor offset@ buffer buffer-left-bound @ > if
        -1 buffer buffer-edit-cursor adjust-offset
        buffer go-to-unicode-start
      then
    ; define do-backward

    \ Move the edit cursor right by one character
    :noname { buffer -- }
      1 buffer buffer-edit-cursor adjust-offset
      buffer go-to-unicode-end
    ; define do-forward

    \ Move the edit cursor up by one character
    :noname { buffer -- }
      buffer buffer-edit-cursor buffer cursor-start-dist { dist }
      dist buffer buffer-width@ u/mod { cols rows }
      rows 0= if
        [: newline = ;] buffer buffer-edit-cursor find-prev
        -1 buffer buffer-edit-cursor adjust-offset
        buffer buffer-edit-cursor buffer cursor-start-dist to dist
        dist buffer buffer-width@ umod { cols' }
        buffer edit-cursor-special-line? if
          0 { W^ data }
          begin cols' cols > while
            data 1 buffer buffer-edit-cursor read-data-before-w/o-move if
              c@ dup tab = if
                drop
                -1 buffer buffer-edit-cursor adjust-offset
                buffer buffer-edit-cursor buffer cursor-start-dist to dist
                dist buffer buffer-width@ umod to cols'
              else
                -1 buffer buffer-edit-cursor adjust-offset
                dup bl >= over delete < and swap unicode-start? or if
                  -1 +to cols'
                then
              then
            else
              drop 0 to cols'
            then
          repeat
        else
          cols' cols > if
            cols cols' - buffer buffer-edit-cursor adjust-offset
          then            
        then
      else
        buffer edit-cursor-special-line? if
          buffer buffer-width@ { cols' }
          0 { W^ data }
          begin cols' 0> while
            data 1 buffer buffer-edit-cursor read-data-before-w/o-move if
              c@ dup tab = if
                drop
                buffer buffer-edit-cursor buffer cursor-start-dist { before }
                -1 buffer buffer-edit-cursor adjust-offset
                buffer buffer-edit-cursor buffer cursor-start-dist { after }
                after before - +to cols'
              else
                -1 buffer buffer-edit-cursor adjust-offset
                dup bl >= over delete < and swap unicode-start? or if
                  -1 +to cols'
                then
              then
            else
              drop 0 to cols'
            then
          repeat
        else
          buffer buffer-width@ negate buffer buffer-edit-cursor adjust-offset
        then
      then
      buffer buffer-edit-cursor offset@ buffer buffer-left-bound @ < if
        buffer buffer-left-bound @ buffer buffer-edit-cursor go-to-offset
      then
    ; define do-up

    \ Move the edit cursor down by one character
    :noname { buffer -- }
      buffer buffer-edit-cursor buffer cursor-start-dist { dist }
      dist buffer buffer-width@ umod { cols }
      buffer edit-cursor-line-last? if
        [: newline = ;] buffer buffer-edit-cursor find-next
        1 buffer buffer-edit-cursor adjust-offset
        buffer buffer-edit-cursor buffer cursor-end-dist to dist
        dist cols >= if
          0 { cols' }
          buffer edit-cursor-special-line? if
            0 { W^ data }
            begin cols cols' > while
              data 1 buffer buffer-edit-cursor read-data if
                data c@ dup tab = if
                  drop
                  zeptoed-tab-size cols' zeptoed-tab-size umod - +to cols'
                else
                  dup bl >= over delete < and swap unicode-start? or if
                    1 +to cols'
                  then
                then
              else
                cols to cols'
              then
            repeat
          else
            dist cols > if cols else dist then
            buffer buffer-edit-cursor adjust-offset
          then
        else
          [: newline = ;] buffer buffer-edit-cursor find-next
        then
      else
        buffer buffer-edit-cursor buffer cursor-end-dist { end-dist }
        end-dist buffer buffer-width@ >= if
          buffer edit-cursor-special-line? if
            0 { cols' }
            0 { W^ data }
            begin buffer buffer-width@ cols' > while
              data 1 buffer buffer-edit-cursor read-data if
                data c@ dup tab = if
                  drop
                  zeptoed-tab-size cols' zeptoed-tab-size umod - +to cols'
                else
                  dup bl >= over delete < and swap unicode-start? or if
                    1 +to cols'
                  then
                then
              else
                buffer buffer-width@ to cols'
              then
            repeat
          else
            buffer buffer-width@ buffer buffer-edit-cursor adjust-offset
          then
        else
          [: newline = ;] buffer buffer-edit-cursor find-next
        then
      then
    ; define do-down
    
    \ Enter a character into the buffer
    :noname { W^ c buffer -- }
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff }
      c 1 buffer buffer-edit-cursor insert-data
      select-diff 0> if 1 buffer buffer-select-cursor adjust-offset then
      1 buffer buffer-edit-cursor offset@ buffer add-delete-undo
      buffer dirty-buffer
    ; define do-insert

    \ Enter a newline with indentation into the buffer
    :noname { buffer -- }
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff }
      buffer edit-cursor-line-indent { indent }
      newline { W^ data }
      data 1 buffer buffer-edit-cursor insert-data
      bl data c!
      indent 0 ?do data 1 buffer buffer-edit-cursor insert-data loop
      select-diff 0> if indent 1+ buffer buffer-select-cursor adjust-offset then
      indent 1+ buffer buffer-edit-cursor offset@ buffer add-delete-undo
      buffer dirty-buffer
    ; define do-newline
    
    \ Find the begin and end offsets for a delete
    :noname ( buffer -- start-offset end-offset )
      dup buffer-dyn-buffer <cursor> [: { buffer cursor }
        buffer buffer-edit-cursor cursor copy-cursor
        cursor offset@ { begin-offset }
        begin
          cursor offset@ 0> if
            cursor buffer cursor-before { byte }
            -1 cursor adjust-offset
            byte unicode? not byte unicode-start? or
          else
            true
          then
        until
        cursor offset@ begin-offset
      ;] with-object
    ; define find-delete-range
    
    \ Find the begin and end offsets for a delete forward
    :noname ( buffer -- start-offset end-offset )
      dup buffer-dyn-buffer <cursor> [: { buffer cursor }
        buffer buffer-edit-cursor cursor copy-cursor
        cursor buffer cursor-at { byte }
        byte unicode? byte unicode-start? not and if
          begin
            -1 cursor adjust-offset
            cursor offset@ 0> if
              cursor buffer cursor-at to byte
              byte unicode? not byte unicode-start? or
            else
              true
            then
          until
        then
        cursor offset@ { begin-offset }
        0 { count }
        begin
          cursor offset@ buffer buffer-len@ < if
            1 cursor adjust-offset
            1 +to count
            cursor buffer cursor-at to byte
            byte unicode? not byte unicode-start? or
          else
            true
          then
        until
        begin-offset dup count +
      ;] with-object
    ; define find-delete-forward-range

    \ Delete in the buffer
    :noname { buffer -- }
      buffer buffer-left-bound @ buffer buffer-edit-cursor offset@ = if
        exit
      then
      buffer find-delete-range over buffer add-insert-undo
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff }
      begin
        buffer buffer-edit-cursor offset@ 0> if
          buffer edit-cursor-before { byte }
          1 buffer buffer-edit-cursor delete-data
          buffer dirty-buffer
          select-diff 0>= if -1 buffer buffer-select-cursor adjust-offset then
          byte unicode? not byte unicode-start? or
        else
          true
        then
      until
    ; define do-delete

    \ Delete forward in the buffer
    :noname { buffer -- }
      buffer find-delete-forward-range over negate buffer add-insert-undo
      buffer go-to-unicode-start
      buffer buffer-select-cursor offset@ buffer buffer-edit-cursor offset@ -
      { select-diff }
      begin
        buffer buffer-edit-cursor offset@ buffer buffer-len@ < if
          1 buffer buffer-edit-cursor adjust-offset
          1 buffer buffer-edit-cursor delete-data
          buffer dirty-buffer
          select-diff 0> if -1 buffer buffer-select-cursor adjust-offset then
          buffer edit-cursor-at { byte }
          byte unicode? not byte unicode-start? or
        else
          true
        then
      until
    ; define do-delete-forward

    \ Delete a range in the buffer
    :noname { buffer -- }
      buffer buffer-select-cursor { select }
      buffer buffer-edit-cursor { edit }
      select offset@ { select-offset }
      edit offset@ { edit-offset }
      select-offset edit-offset < if
        select-offset edit-offset select-offset
        buffer add-insert-undo
        edit-offset select-offset - edit delete-data
        buffer dirty-buffer
      else
        edit-offset select-offset < if
          edit-offset select-offset edit-offset
          buffer add-insert-undo
          select-offset edit-offset - select delete-data
          buffer dirty-buffer
        then
      then
    ; define do-delete-range

    \ Kill a range in the buffer
    :noname { clip buffer -- }
      buffer buffer-select-cursor { select }
      buffer buffer-edit-cursor { edit }
      select offset@ { select-offset }
      edit offset@ { edit-offset }
      select-offset edit-offset < if
        select-offset edit-offset select-offset
        buffer add-insert-undo
        select edit buffer buffer-dyn-buffer clip copy-to-clip
        edit-offset select-offset - edit delete-data
        buffer dirty-buffer
      else
        edit-offset select-offset < if
          edit-offset select-offset edit-offset
          buffer add-insert-undo
          edit select buffer buffer-dyn-buffer clip copy-to-clip
          select-offset edit-offset - select delete-data
          buffer dirty-buffer
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
      edit offset@ { edit-offset }
      select offset@ edit-offset - { select-diff }
      edit clip insert-from-clip
      select-diff 0>= if
        edit offset@ edit-offset - buffer buffer-select-cursor adjust-offset
      then
      edit offset@ { new-edit-offset }
      new-edit-offset edit-offset - new-edit-offset buffer add-delete-undo
      clip clip-size@ 0> if buffer dirty-buffer then
    ; define do-paste

    \ Is there a selection in the buffer
    :noname ( buffer -- selected? )
      buffer-select-enabled @
    ; define selected?
    
    \ Select in the buffer
    :noname { buffer -- }
      true buffer buffer-select-enabled !
      buffer buffer-edit-cursor buffer buffer-select-cursor copy-cursor
    ; define do-select

    \ Deselect in the buffer
    :noname ( buffer -- )
      false swap buffer-select-enabled !
    ; define do-deselect

    \ Undo in the buffer
    :noname { buffer -- }
      buffer newest-undo@ { undo }
      undo if
        buffer buffer-select-cursor offset@ { select-offset }
        undo undo-size @ 0> if
          undo undo-offset @ 0> if
            undo undo-offset @ buffer buffer-edit-cursor go-to-offset
            undo undo-header-size + undo undo-size @
            buffer buffer-edit-cursor insert-data
            select-offset undo undo-offset @ > if
              undo undo-size @ buffer buffer-select-cursor adjust-offset
            then
          else
            undo undo-offset @ negate buffer buffer-edit-cursor go-to-offset
            undo undo-header-size + undo undo-size @
            buffer buffer-edit-cursor insert-data
            undo undo-size @ negate buffer buffer-edit-cursor adjust-offset
            select-offset undo undo-offset @ negate > if
              undo undo-size @ buffer buffer-select-cursor adjust-offset
            then
          then
        else
          undo undo-offset @ buffer buffer-edit-cursor go-to-offset
          undo undo-size @ negate buffer buffer-edit-cursor delete-data
          select-offset undo undo-offset @ > if
            undo undo-size @ buffer buffer-select-cursor adjust-offset
          then
        then
        buffer drop-undo
        buffer dirty-buffer
      then
    ; define do-undo

    \ Go to the start of the line in the buffer
    :noname { buffer -- }
      [: newline = ;] buffer buffer-edit-cursor find-prev
      buffer buffer-edit-cursor offset@ buffer buffer-left-bound @ < if
        buffer buffer-left-bound @ buffer buffer-edit-cursor go-to-offset
      then
    ; define do-line-start

    \ Go to the end of the line in the buffer
    :noname { buffer -- }
      [: newline = ;] buffer buffer-edit-cursor find-next
    ; define do-line-end

    \ Page up in buffer
    :noname { buffer -- }
      buffer buffer-height@ 0 ?do
        buffer buffer-edit-cursor offset@ buffer buffer-left-bound @ > if
          buffer do-up
        else
          unloop exit
        then
      loop
    ; define do-page-up

    \ Page down in buffer
    :noname { buffer -- }
      buffer buffer-height@ 0 ?do
        buffer buffer-edit-cursor offset@ buffer buffer-len@ < if
          buffer do-down
        else
          unloop exit
        then
      loop
    ; define do-page-down

    \ Home in buffer
    :noname { buffer -- }
      buffer buffer-left-bound @ buffer buffer-edit-cursor go-to-offset
    ; define do-doc-home

    \ End in buffer
    :noname { buffer -- }
      buffer buffer-edit-cursor go-to-end
    ; define do-doc-end
    
    \ Indent in buffer
    :noname { buffer -- }
      buffer selected? if
        buffer dup buffer-dyn-buffer <cursor> [: { buffer cursor }
          buffer buffer-select-cursor offset@ { select-offset }
          buffer buffer-edit-cursor offset@ { edit-offset }
          select-offset edit-offset min { offset0 }
          select-offset edit-offset max { offset1 }
          offset0 cursor go-to-offset
          [: newline = ;] cursor find-prev
          cursor offset@ buffer buffer-left-bound @ < if
            buffer buffer-left-bound @ cursor go-to-offset
          then
          begin cursor offset@ offset1 < while
            zeptoed-indent-size +to offset1
            [: dup tab <> swap bl <> and ;] cursor find-next
            zeptoed-indent-size 0 ?do
              bl { W^ data }
              data 1 cursor insert-data
            loop
            zeptoed-indent-size cursor offset@ buffer add-delete-undo
            [: newline = ;] cursor find-next
            1 cursor adjust-offset
          repeat
          select-offset edit-offset < if
            offset1 buffer buffer-edit-cursor go-to-offset
          else
            edit-offset select-offset < if
              offset1 buffer buffer-select-cursor go-to-offset
            then
          then
          offset0 select-offset = if
            offset0 zeptoed-indent-size +
            buffer buffer-select-cursor go-to-offset
          then
          offset0 edit-offset = if
            offset0 zeptoed-indent-size +
            buffer buffer-edit-cursor go-to-offset
          then
        ;] with-object
      else
        buffer dup buffer-dyn-buffer <cursor> [: { buffer cursor }
          buffer buffer-edit-cursor cursor copy-cursor
          [: newline = ;] cursor find-prev
          cursor offset@ buffer buffer-left-bound @ < if
            buffer buffer-left-bound @ cursor go-to-offset
          then
          [: dup tab <> swap bl <> and ;] cursor find-next
          zeptoed-indent-size 0 ?do
            bl { W^ data }
            data 1 cursor insert-data
          loop
          zeptoed-indent-size cursor offset@ buffer add-delete-undo
          zeptoed-indent-size buffer buffer-edit-cursor adjust-offset
        ;] with-object
      then
      buffer dirty-buffer
    ; define do-indent

    \ Unindent in buffer
    :noname { buffer -- }
      buffer selected? if
        buffer dup buffer-dyn-buffer <cursor> [: { buffer cursor }
          buffer buffer-select-cursor offset@ { select-offset }
          buffer buffer-edit-cursor offset@ { edit-offset }
          select-offset edit-offset min { offset0 }
          select-offset edit-offset max { offset1 }
          offset0 cursor go-to-offset
          [: newline = ;] cursor find-prev
          cursor offset@ buffer buffer-left-bound @ < if
            buffer buffer-left-bound @ cursor go-to-offset
          then
          cursor offset@ { line-offset }
          cursor offset1 buffer not-indented? if exit then
          false { first }
          begin cursor offset@ offset1 < while
            cursor buffer cursor-tab-in-indent? if
              cursor buffer cursor-line-indent-bytes
              cursor buffer cursor-line-indent zeptoed-indent-size - 0 max
            else
              cursor buffer cursor-line-indent-bytes zeptoed-indent-size min 0
            then
            { delete-count insert-count }
            [: dup tab <> swap bl <> and ;] cursor find-next
            cursor offset@ dup delete-count - tuck buffer add-insert-undo
            delete-count cursor delete-data
            insert-count 0 ?do
              bl { W^ data } data 1 cursor insert-data
            loop
            insert-count 0> if
              insert-count cursor offset@ buffer add-delete-undo
            then
            [: newline = ;] cursor find-next
            1 cursor adjust-offset
            first not if
              true to first
              offset0 select-offset = if
                offset0 insert-count + delete-count - line-offset > if
                  insert-count delete-count -
                  buffer buffer-select-cursor adjust-offset
                else
                  line-offset buffer buffer-select-cursor go-to-offset
                then
              then
              offset0 edit-offset = if
                offset0 insert-count + delete-count - line-offset > if
                  insert-count delete-count -
                  buffer buffer-edit-cursor adjust-offset
                else
                  line-offset buffer buffer-edit-cursor go-to-offset
                then
              then
            then
            insert-count delete-count - +to offset1
          repeat
          select-offset edit-offset < if
            offset1 buffer buffer-edit-cursor go-to-offset
          else
            edit-offset select-offset <  if
              offset1 buffer buffer-select-cursor go-to-offset
            then
          then
          buffer dirty-buffer
        ;] with-object
      else
        buffer dup buffer-dyn-buffer <cursor> [: { buffer cursor }
          buffer buffer-edit-cursor cursor copy-cursor
          [: newline = ;] cursor find-prev
          cursor offset@ buffer buffer-left-bound @ < if
            buffer buffer-left-bound @ cursor go-to-offset
          then
          cursor buffer cursor-tab-in-indent? if
            cursor buffer cursor-line-indent-bytes
            cursor buffer cursor-line-indent zeptoed-indent-size - 0 max
          else
            cursor buffer cursor-line-indent-bytes zeptoed-indent-size min 0
          then
          { delete-count insert-count }
          [: dup tab <> swap bl <> and ;] cursor find-next
          cursor offset@ dup delete-count - tuck buffer add-insert-undo
          delete-count cursor delete-data
          insert-count 0 ?do
            bl { W^ data } data 1 cursor insert-data
          loop
          insert-count 0> if
            insert-count cursor offset@ buffer add-delete-undo
          then
          insert-count delete-count - buffer buffer-edit-cursor adjust-offset
          buffer dirty-buffer
        ;] with-object
      then
    ; define do-unindent

    \ Search forward by one character
    :noname { c buffer -- }
      c buffer add-search-text-char
      buffer buffer-search-text 2@ buffer edit-cursor-search-forward
      buffer update-display drop
      buffer refresh-display
    ; define do-search-forward

    \ Search backward by one character
    :noname { c buffer -- }
      c buffer add-search-text-char
      buffer buffer-search-text 2@ buffer edit-cursor-search-backward
      buffer update-display drop
      buffer refresh-display
    ; define do-search-backward

    \ Delete character in search
    :noname { buffer -- }
      buffer delete-search-text-char
    ; define do-search-delete

    \ Paste in search
    :noname ( clip buffer -- )
      default-segment-size [: { clip buffer data }
        clip clip-cursor { src-cursor }
        src-cursor offset@ { end-offset }
        src-cursor go-to-start
        begin
          end-offset src-cursor offset@ - default-segment-size min { part-size }
          part-size 0> if
            data part-size src-cursor read-data to part-size
            data part-size buffer add-search-text
            false
          else
            true
          then
        until
        buffer buffer-char-entry-mode @ search-forward-mode = if
          buffer buffer-search-text 2@ buffer edit-cursor-search-forward
        else
          buffer buffer-char-entry-mode @ search-backward-mode = if
            buffer buffer-search-text 2@ buffer edit-cursor-search-backward
          then
        then
        buffer update-display drop
        buffer refresh-display
      ;] with-allot
    ; define do-search-paste
    
    \ Editor constants
    0 constant in-middle
    1 constant in-last-line
    2 constant at-end
    3 constant at-last-first
    4 constant at-last-last
    5 constant in-single-row
    6 constant at-start
    7 constant full-width
    8 constant insert/delete-tab
    
    \ Go left one character
    :noname { buffer -- }
      buffer leave-search
      buffer update-prev-cursor
      buffer edit-cursor-offset@ 0> if
        buffer edit-cursor-left-space { cols rows }
        buffer edit-cursor-before-spaces { spaces }
        buffer do-backward
        buffer update-display if
          buffer refresh-display
        else
          cols 0> if
            buffer edit-cursor-at tab <> if
              spaces buffer output-left
            else
              buffer refresh-line
            then
          else
            buffer refresh-display
          then
        then
      then
    ; define handle-backward

    \ Go right one character
    :noname { buffer -- }
      buffer leave-search
      buffer update-prev-cursor
      buffer edit-cursor-offset@ buffer buffer-len@ < if
        buffer edit-cursor-left-space { cols rows }
        buffer edit-cursor-at-spaces { spaces }
        buffer edit-cursor-at-end? { at-end? }
        buffer edit-cursor-before tab = { before-tab? }
        buffer do-forward
        buffer update-display if
          buffer refresh-display
        else
          at-end? if
            buffer output-next-line
          else
            cols buffer buffer-width@ 1- < if
              before-tab? not if
                spaces buffer output-right
              else
                buffer refresh-line
              then
            else
              buffer refresh-display
            then
          then
        then
      then
    ; define handle-forward

    \ Go up one character
    :noname { buffer -- }
      buffer leave-search
      buffer update-prev-cursor
      buffer edit-cursor-first-row? if
        buffer do-first
        buffer update-display if
          buffer refresh-display
        else
          buffer output-up
        then
      else
        buffer do-up
        buffer update-display if
          buffer refresh-display
        else
          buffer output-up
        then
      then
    ; define handle-up

    \ Go down one character
    :noname { buffer -- }
      buffer leave-search
      buffer update-prev-cursor
      buffer edit-cursor-last-row? if
        buffer do-last
        buffer update-display if
          buffer refresh-display
        else
          buffer output-last
        then
      else
        buffer do-down
        buffer update-display if
          buffer refresh-display
        else
          buffer output-down
        then
      then
    ; define handle-down
    
    \ Enter a character into the buffer
    :noname { c buffer -- }
      buffer buffer-char-entry-mode @ search-forward-mode = if
        c buffer do-search-forward exit
      then
      buffer buffer-char-entry-mode @ search-backward-mode = if
        c buffer do-search-backward exit
      then
      buffer update-prev-cursor
      buffer edit-cursor-at-end? if
        c tab <> if
          at-end
        else
          in-middle
        then
      else
        in-middle
      then { position }
\      buffer edit-cursor-line-rows { start-rows }
      c buffer do-insert
\      buffer edit-cursor-line-rows start-rows <> if
\        full-width to position
\      then
      buffer buffer-edit-cursor buffer cursor-line-last-row-len 0= if
        full-width to position
      then
      buffer update-display if
        buffer refresh-display
      else
        position case
          full-width of buffer refresh-display endof
          in-middle of buffer refresh-line endof
          at-end of c buffer output-char endof
        endcase
      then
    ; define handle-insert

    \ Enter a newline into the buffer
    :noname { buffer -- }
      buffer buffer-char-entry-mode @ search-forward-mode =
      buffer buffer-char-entry-mode @ search-backward-mode = or if
        insert-mode buffer buffer-char-entry-mode ! exit
      then
      buffer do-newline
      buffer update-display drop
      buffer refresh-display
    ; define handle-newline

    \ Backspace in the buffer
    :noname { buffer -- }
      buffer buffer-char-entry-mode @ search-forward-mode =
      buffer buffer-char-entry-mode @ search-backward-mode = or if
        buffer do-search-delete exit
      then
      buffer update-prev-cursor
      buffer selected? if
        buffer do-delete-range
        buffer do-deselect
        buffer update-display drop
        buffer refresh-display
      else
        buffer buffer-left-bound @ buffer buffer-edit-cursor offset@ = if
          exit
        then
        buffer edit-cursor-at-start? if
          at-start
        else
          buffer edit-cursor-at-end? if
            buffer edit-cursor-before tab = if
              in-middle
            else
              at-end
            then
          else
            in-middle
          then
        then { position }
        buffer buffer-edit-cursor buffer cursor-line-last-row-len 0= if
          full-width to position
        then
\        buffer edit-cursor-line-rows { start-rows }
        buffer do-delete
\        buffer edit-cursor-line-rows start-rows <> if
\          full-width to position
\        then
        buffer buffer-edit-cursor buffer cursor-line-last-row-len 0= if
          full-width to position
        then
        buffer update-display if
          buffer refresh-display
        else
          position case
            in-middle of buffer refresh-line endof
            full-width of buffer refresh-display endof
            at-start of buffer refresh-display endof
            at-end of buffer output-backspace endof
          endcase
        then
      then
    ; define handle-delete

    \ Delete in the buffer
    :noname { buffer -- }
      buffer leave-search
      buffer update-prev-cursor
      buffer selected? if
        buffer do-delete-range
        buffer do-deselect
        buffer update-display drop
        buffer refresh-display
      else
        buffer edit-cursor-offset@ buffer buffer-len@ = if exit then
        buffer edit-cursor-single-row? if
          buffer edit-cursor-at-end? if
            in-middle
          else
            in-single-row
          then
        else
          in-middle
        then { position }
        buffer do-delete-forward
        buffer update-display if
          buffer refresh-display
        else
          position case
            in-middle of buffer refresh-display endof
            in-single-row of buffer refresh-line endof
          endcase
        then
      then
    ; define handle-delete-forward

    \ Kill a range in the buffer
    :noname { buffer -- }
      buffer leave-search
      buffer selected? if
        buffer buffer-editor @ editor-clip buffer do-kill
        buffer do-deselect
        buffer update-display drop
        buffer refresh-display
      then
    ; define handle-kill

    \ Copy a range in the buffer
    :noname { buffer -- }
      buffer leave-search
      buffer selected? if
        buffer buffer-editor @ editor-clip buffer do-copy
        buffer do-deselect
        buffer refresh-display
      then
    ; define handle-copy

    \ Paste in the buffer
    :noname { buffer -- }
      buffer buffer-char-entry-mode @ search-forward-mode =
      buffer buffer-char-entry-mode @ search-backward-mode = or if
        buffer buffer-editor @ editor-clip buffer do-search-paste exit
      then
      buffer buffer-editor @ editor-clip buffer do-paste
      buffer update-display drop
      buffer refresh-display
    ; define handle-paste

    \ Select in the buffer
    :noname { buffer -- }
      buffer leave-search
      buffer selected? if
        buffer do-deselect
      else
        buffer do-select
      then
      buffer refresh-display
    ; define handle-select

    \ Undo in the buffer
    :noname { buffer -- }
      buffer leave-search
      buffer do-undo
      buffer update-display drop
      buffer refresh-display
    ; define handle-undo

    \ Go to line start in the buffer
    :noname { buffer -- }
      buffer leave-search
      buffer update-prev-cursor
      buffer edit-cursor-single-row? if
        in-single-row
      else
        in-middle
      then { position }
      buffer do-line-start
      buffer update-display if
        buffer refresh-display
      else
        position case
          in-single-row of buffer refresh-line endof
          in-middle of buffer refresh-display endof
        endcase
      then
    ; define handle-start

    \ Go to line end in the buffer
    :noname { buffer -- }
      buffer leave-search
      buffer update-prev-cursor
      buffer edit-cursor-single-row? if
        in-single-row
      else
        in-middle
      then { position }
      buffer do-line-end
      buffer update-display if
        buffer refresh-display
      else
        position case
          in-single-row of buffer refresh-line endof
          in-middle of buffer refresh-display endof
        endcase
      then
    ; define handle-end

    \ Page up in buffer
    :noname { buffer -- }
      buffer leave-search
      buffer do-page-up
      buffer update-display drop
      buffer refresh-display
    ; define handle-page-up

    \ Page down in buffer
    :noname { buffer -- }
      buffer leave-search
      buffer do-page-down
      buffer update-display drop
      buffer refresh-display
    ; define handle-page-down

    \ Home in buffer
    :noname { buffer -- }
      buffer leave-search
      buffer do-doc-home
      buffer update-display drop
      buffer refresh-display
    ; define handle-doc-home

    \ End in buffer
    :noname { buffer -- }
      buffer leave-search
      buffer do-doc-end
      buffer update-display drop
      buffer refresh-display
    ; define handle-doc-end

    \ Indent in buffer
    :noname { buffer -- }
      buffer leave-search
      buffer do-indent
      buffer update-display drop
      buffer refresh-display
    ; define handle-indent

    \ Unindent in buffer
    :noname { buffer -- }
      buffer leave-search
      buffer do-unindent
      buffer update-display drop
      buffer refresh-display
    ; define handle-unindent

    \ Start finding forward
    :noname { buffer -- }
      buffer searching? { buffer-searching? }
      search-forward-mode buffer buffer-char-entry-mode @ <> if
        search-forward-mode buffer buffer-char-entry-mode !
        buffer-searching? buffer buffer-search-text 2@ nip 0> and if
          buffer buffer-search-text 2@ buffer edit-cursor-search-forward
          buffer update-display drop
          buffer refresh-display
        then
      else
        buffer-searching? buffer buffer-search-text 2@ nip 0> and if
          buffer buffer-search-text 2@ buffer edit-cursor-continue-forward
          buffer update-display drop
          buffer refresh-display
        then
      then
    ; define handle-search-forward

    \ Start finding backward
    :noname { buffer -- }
      buffer searching? { buffer-searching? }
      search-backward-mode buffer buffer-char-entry-mode @ <> if
        search-backward-mode buffer buffer-char-entry-mode !
        buffer-searching? buffer buffer-search-text 2@ nip 0> and if
          buffer buffer-search-text 2@ buffer edit-cursor-search-backward
          buffer update-display drop
          buffer refresh-display
        then
      else
        buffer-searching? buffer buffer-search-text 2@ nip 0> and if
          buffer buffer-search-text 2@ buffer edit-cursor-continue-backward
          buffer update-display drop
          buffer refresh-display
        then
      then
    ; define handle-search-backward
    
  end-implement

  \ Implement the file buffer
  <file-buffer> begin-implement

    \ Constructor
    :noname { path-addr path-bytes heap editor buffer -- }
      path-addr path-bytes heap editor buffer <buffer>->new
      0 buffer file-buffer-exception !
      false buffer file-buffer-accessed !
      false buffer buffer-file-open !
      path-addr path-bytes buffer ['] access-file try ?dup if
        buffer file-buffer-exception ! 2drop drop exit
      then
      buffer ['] load-buffer-from-file try ?dup if
        buffer file-buffer-exception ! drop
      then
    ; define new

    \ Destructor
    :noname { buffer }
      buffer buffer-file-open @ if
        buffer buffer-file close-file
        buffer buffer-file destroy
        false buffer buffer-file-open !
      then
      buffer <buffer>->destroy
    ; define destroy

    \ Buffer file is valid
    :noname ( buffer -- valid? )
      file-buffer-exception @ 0=
    ; define file-buffer-valid?

    \ Buffer file exception
    :noname ( buffer -- exception )
      file-buffer-exception @
    ; define file-buffer-exception@

    \ Dirty buffer
    :noname { buffer -- }
      buffer buffer-dirty @ { old-dirty }
      buffer <buffer>->dirty-buffer
      old-dirty not if
        buffer output-title
        buffer buffer-editor @ update-coord
      then
    ; define dirty-buffer

    \ Clean buffer
    :noname { buffer -- }
      buffer buffer-dirty @ { old-dirty }
      buffer <buffer>->clean-buffer
      old-dirty if
        buffer output-title
        buffer buffer-editor @ update-coord
      then
    ; define clean-buffer

    \ Access a file, creating or opening it
    :noname { path-addr path-bytes buffer -- }
      buffer buffer-file-open @ if
        buffer buffer-file close-file
        buffer buffer-file destroy
        false buffer buffer-file-open !
      then
      path-addr path-bytes fat32-tools::current-fs@ root-path-exists? if
        buffer buffer-file path-addr path-bytes [:
          { file file-addr file-bytes dir }
          file-addr file-bytes file dir open-file
        ;] fat32-tools::current-fs@ with-root-path
      else
        buffer buffer-file path-addr path-bytes [:
          { file file-addr file-bytes dir }
          file-addr file-bytes file dir create-file
        ;] fat32-tools::current-fs@ with-root-path
      then
      true buffer buffer-file-open !
    ; define access-file

    \ Try to change the file path
    :noname { path-addr path-bytes buffer -- exception success? }
      path-addr path-bytes buffer ['] access-file try ?dup if
        buffer buffer-path 2@ buffer ['] access-file try ?dup if { exception }
          2drop 2drop 2drop drop
          exception buffer file-buffer-exception ! exception false
        else { exception }
          2drop drop exception false
        then
      else
        buffer buffer-name 2@ drop buffer buffer-heap @ free
        buffer buffer-path 2@ drop buffer buffer-heap @ free
        path-bytes buffer buffer-heap @ allocate { new-name-addr }
        path-addr new-name-addr path-bytes move
        path-bytes buffer buffer-heap @ allocate { new-path-addr }
        path-addr new-path-addr path-bytes move
        new-name-addr path-bytes buffer buffer-name 2!
        new-path-addr path-bytes buffer buffer-path 2!
        buffer output-title
        0 true
      then
    ; define try-change-file-path
    
    \ Load buffer from file
    :noname ( buffer -- )
      512 [: { buffer data }
        buffer buffer-edit-cursor go-to-end
        buffer buffer-edit-cursor offset@
        buffer buffer-edit-cursor delete-data
        buffer buffer-select-cursor go-to-start
        buffer buffer-display-cursor go-to-start
        0 seek-set buffer buffer-file seek-file
        buffer buffer-file file-size@ { bytes }
        0 { bytes-read }
        begin bytes-read bytes < while
          bytes bytes-read - 512 min { part-size }
          data part-size buffer buffer-file read-file to part-size
          data part-size strip-crs { stripped-size }
          data stripped-size buffer buffer-edit-cursor insert-data
          part-size +to bytes-read
        repeat
        buffer buffer-edit-cursor go-to-start
        buffer clean-buffer
      ;] with-allot
    ; define load-buffer-from-file
    
    \ Write out a file
    :noname ( buffer -- )
      zeptoed-save-crlf-enabled if 1024 else 512 then
      [:
        over buffer-dyn-buffer <cursor> [: { buffer data cursor }
          buffer buffer-len@ { bytes }
          0 { bytes-written }
          0 seek-set buffer buffer-file seek-file
          begin bytes-written bytes < while
            bytes bytes-written - 512 min { part-size }
            data part-size cursor read-data to part-size
            zeptoed-save-crlf-enabled if
              data part-size expand-newlines-to-crlfs
            else
              part-size
            then { expanded-size }
            \ This line relies on the fact that WRITE-FILE always writes the
            \ full length that is given. However, we shouldn't really trust
            \ this. TODO: Fix this.
            data expanded-size buffer buffer-file write-file drop
            part-size +to bytes-written
          repeat
          buffer buffer-file truncate-file
          fat32-tools::current-fs@ flush
          buffer clean-buffer
        ;] with-object
      ;] with-allot
    ; define write-buffer-to-file

    \ Handle change file path
    :noname { buffer -- }
      buffer buffer-editor @ activate-minibuffer
      s" Write to path: " buffer ['] do-change-file-path
      buffer buffer-editor @ editor-minibuffer @ set-prompt
    ; define handle-change-file-path

    \ Actually do change file path
    :noname { buffer -- }
      buffer buffer-editor @ editor-in-minibuffer @ if
        buffer 256 [: { buffer data }
          data 256 0 buffer buffer-editor @ editor-minibuffer @
          read-minibuffer { len }
          buffer buffer-editor @ editor-minibuffer @ clear-minibuffer
          buffer buffer-editor @ deactivate-minibuffer
          data len buffer try-change-file-path if
            drop buffer handle-write
          else
            file-error-string buffer buffer-editor @ editor-minibuffer @
            set-message
          then
        ;] with-allot
      then
    ; define do-change-file-path

    \ Handle write
    :noname { buffer -- }
      buffer write-buffer-to-file
      buffer file-buffer-valid? not if
        buffer file-buffer-exception@ file-error-string
        buffer buffer-editor @ editor-minibuffer @ set-message
      else
        buffer refresh-display
        s" Wrote file" buffer buffer-editor @ editor-minibuffer @ set-message
      then
    ; define handle-write

    \ Handle revert
    :noname { buffer -- }
      buffer load-buffer-from-file
      buffer file-buffer-valid? not if
        buffer file-buffer-exception@ file-error-string
        buffer buffer-editor @ editor-minibuffer @ set-message
      else
        buffer refresh-display
        s" Reverted file" buffer buffer-editor @ editor-minibuffer @ set-message
        buffer clear-undos
      then
    ; define handle-revert
    
  end-implement
  
  \ Implement the minibuffer
  <minibuffer> begin-implement

    \ Constructor
    :noname { heap editor minibuffer -- }
      s" " heap editor minibuffer <buffer>->new
      true minibuffer minibuffer-read-only !
      0 minibuffer minibuffer-callback !
      0 minibuffer minibuffer-callback-object !
    ; define new

    \ Clear minibuffer
    :noname ( minibuffer -- )
      s" " rot set-message
    ; define clear-minibuffer
    
    \ Display message
    :noname { addr bytes minibuffer -- }
      minibuffer clear-undos
      minibuffer buffer-edit-cursor go-to-end
      minibuffer buffer-edit-cursor offset@
      minibuffer buffer-edit-cursor delete-data
      addr bytes minibuffer buffer-edit-cursor insert-data
      minibuffer buffer-select-cursor go-to-end
      minibuffer buffer-display-cursor go-to-start
      bytes minibuffer buffer-left-bound !
      true minibuffer minibuffer-read-only !
      0 minibuffer buffer-edit-row !
      minibuffer edit-cursor-left-space drop minibuffer buffer-edit-col !
      0 minibuffer minibuffer-callback !
      0 minibuffer minibuffer-callback-object !
      minibuffer clean-buffer
      minibuffer update-display drop
      minibuffer refresh-display
    ; define set-message
    
    \ Set prompt
    :noname { addr bytes object xt minibuffer -- }
      addr bytes minibuffer set-message
      false minibuffer minibuffer-read-only !
      xt minibuffer minibuffer-callback !
      object minibuffer minibuffer-callback-object !
    ; define set-prompt

    \ Get the length of the input in the minibuffer
    :noname ( minibuffer -- bytes )
      dup buffer-dyn-buffer <cursor> [: { minibuffer cursor }
        minibuffer buffer-edit-cursor cursor copy-cursor
        cursor go-to-end
        cursor offset@ minibuffer buffer-left-bound @ -
      ;] with-object
    ; define minibuffer-input-len@

    \ Read data from the minibuffer
    :noname ( addr bytes offset minibuffer -- bytes' )
      dup buffer-dyn-buffer <cursor> [: { addr bytes offset minibuffer cursor }
        minibuffer buffer-left-bound @ offset + cursor go-to-offset
        addr bytes cursor read-data
      ;] with-object
    ; define read-minibuffer
    
    \ Buffer height in characters
    :noname ( buffer -- height )
      drop 1
    ; define buffer-height@

    \ Go to buffer coordinate
    :noname ( row col buffer -- )
      drop nip display-height @ 1- swap go-to-coord
    ; define go-to-buffer-coord

    \ Output buffer title
    :noname ( buffer -- )
      drop
    ; define output-title
    
    \ Go left one character
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-backward
      else
        drop
      then
    ; define handle-backward

    \ Go right one character
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-forward
      else
        drop
      then
    ; define handle-forward

    \ Go up one character
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-up
      else
        drop
      then
    ; define handle-up

    \ Go down one character
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-down
      else
        drop
      then
    ; define handle-down
    
    \ Enter a character into the buffer
    :noname ( c buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-insert
      else
        2drop
      then
    ; define handle-insert

    \ Enter a newline into the buffer
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        dup minibuffer-callback @ ?dup if
          swap minibuffer-callback-object @ swap execute
        then
      else
        drop
      then
    ; define handle-newline

    \ Backspace in the buffer
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-delete
      else
        drop
      then
    ; define handle-delete

    \ Delete in the buffer
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-delete-forward
      else
        drop
      then
    ; define handle-delete-forward

    \ Kill a range in the buffer
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-kill
      else
        drop
      then
    ; define handle-kill

    \ Copy a range in the buffer
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-copy
      else
        drop
      then
    ; define handle-copy

    \ Paste in the buffer
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-paste
      else
        drop
      then
    ; define handle-paste
    
    \ Select in the buffer
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-select
      else
        drop
      then
    ; define handle-select

    \ Undo in the buffer
    :noname ( buffer -- )
      dup minibuffer-read-only @ not if
        <buffer>->handle-undo
      else
        drop
      then
    ; define handle-undo

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

    \ Get the clip size
    :noname { clip -- bytes }
      clip clip-cursor offset@
    ; define clip-size@

    \ Copy from a starting cursor to an ending cursor
    :noname ( start-cursor end-cursor src-dyn-buffer clip -- )
      default-segment-size [:
        2 pick <cursor> [:
          { start-cursor end-cursor src-dyn-buffer clip buffer src-cursor }
          clip clip-cursor offset@ clip clip-cursor delete-data
          start-cursor src-cursor copy-cursor
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
            false
          else
            true
          then
        until
      ;] with-allot
    ; define insert-from-clip
    
  end-implement

  \ Implement the editor
  <editor> begin-implement

    \ Constructor
    :noname { path-addr path-bytes heap editor -- }
      editor <object>->new
      0 editor editor-exception !
      heap editor editor-heap !
      heap <clip> editor editor-clip init-object
      <file-buffer> class-size heap allocate { buffer }
      path-addr path-bytes heap editor <file-buffer> buffer init-object
      buffer file-buffer-valid? not if
        buffer file-buffer-exception@ editor editor-exception !
      then
      <minibuffer> class-size heap allocate { minibuffer }
      heap editor <minibuffer> minibuffer init-object
      buffer editor editor-first !
      buffer editor editor-last !
      buffer editor editor-current !
      minibuffer editor editor-minibuffer !
      false editor editor-in-minibuffer !
      false editor editor-exit !
      false editor editor-searching !
    ; define new

    \ Destructor
    :noname { editor -- }
      editor editor-minibuffer @ destroy
      editor editor-minibuffer @ editor editor-heap @ free
      editor editor-first @ begin ?dup while
        { current }
        current buffer-next @
        current destroy
        current editor editor-heap @ free
      repeat
      editor <object>->destroy
    ; define destroy

    \ Editor is valid
    :noname ( editor -- valid? )
      editor-exception @ 0=
    ; define editor-valid?

    \ Editor exception
    :noname ( editor -- exception )
      editor-exception @
    ; define editor-exception@

    \ Refresh the editor
    :noname { editor -- }
      editor editor-current @ refresh-display
      editor editor-minibuffer @ refresh-display
      editor display-search-text
    ; define refresh-editor

    \ Activate minibuffer
    :noname ( editor -- )
      true swap editor-in-minibuffer !
    ; define activate-minibuffer

    \ Deactivate minibuffer
    :noname ( editor -- )
      false swap editor-in-minibuffer !
    ; define deactivate-minibuffer

    \ Get currently active buffer
    :noname ( editor -- buffer )
      dup editor-in-minibuffer @ if
        editor-minibuffer @
      else
        editor-current @
      then
    ; define active-buffer@

    \ Set the cursor for the current buffer
    :noname ( editor -- )
      active-buffer@ set-buffer-coord
    ; define update-coord

    \ Set the cursor if the buffer is not the current buffer
    :noname { buffer editor -- }
      buffer editor active-buffer@ <> if
        editor active-buffer@ set-buffer-coord
      then
    ; define update-different-coord

    \ Display the search text
    :noname { editor -- }
      editor editor-in-minibuffer @ not if
        editor editor-minibuffer @ minibuffer-read-only @ if
          editor editor-current @ searching? if
            true editor editor-searching !
            editor editor-current @ search-text@ nip { search-len }
            editor
            editor editor-current @ searching-forward? if
              s" Search forward: "
            else
              s" Search backward: "
            then
            nip search-len + [: { editor data }
              editor editor-current @ searching-forward? if
                s" Search forward: "
              else
                s" Search backward: "
              then
              { prefix-addr prefix-len }
              prefix-addr data prefix-len move
              editor editor-current @ search-text@ { search-addr search-len }
              search-addr data prefix-len + search-len move
              data prefix-len search-len +
              editor editor-minibuffer @ set-message
            ;] with-allot
          else
            editor editor-searching @ if
              false editor editor-searching !
              editor editor-minibuffer @ clear-minibuffer
            then
          then
        then
      then
    ; define display-search-text
    
    \ Go left one character
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-backward
      else
        editor editor-current @ handle-backward
      then
      editor display-search-text
    ; define handle-editor-backward

    \ Go right one character
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-forward
      else
        editor editor-current @ handle-forward
      then
      editor display-search-text
    ; define handle-editor-forward

    \ Go up one character
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-up
      else
        editor editor-current @ handle-up
      then
      editor display-search-text
    ; define handle-editor-up

    \ Go down one character
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-down
      else
        editor editor-current @ handle-down
      then
      editor display-search-text
    ; define handle-editor-down
    
    \ Enter a character into the editor
    :noname { c editor -- }
      editor editor-in-minibuffer @ if
        c editor editor-minibuffer @ handle-insert
      else
        c editor editor-current @ handle-insert
      then
      editor display-search-text
    ; define handle-editor-insert

    \ Enter a newline into the editor
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-newline
      else
        editor editor-current @ handle-newline
      then
      editor display-search-text
    ; define handle-editor-newline

    \ Backspace in the editor
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-delete
      else
        editor editor-current @ handle-delete
      then
      editor display-search-text
    ; define handle-editor-delete

    \ Delete in the editor
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-delete-forward
      else
        editor editor-current @ handle-delete-forward
      then
      editor display-search-text
    ; define handle-editor-delete-forward

    \ Kill a range in the editor
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-kill
      else
        editor editor-current @ handle-kill
      then
      editor display-search-text
    ; define handle-editor-kill

    \ Copy a range in the editor
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-copy
      else
        editor editor-current @ handle-copy
      then
      editor display-search-text
    ; define handle-editor-copy

    \ Paste in the editor
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-paste
      else
        editor editor-current @ handle-paste
      then
      editor display-search-text
    ; define handle-editor-paste

    \ Select in the editor
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-select
      else
        editor editor-current @ handle-select
      then
      editor display-search-text
    ; define handle-editor-select

    \ Undo in the editor
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-undo
      else
        editor editor-current @ handle-undo
      then
      editor display-search-text
    ; define handle-editor-undo

    \ Go to the previous buffer
    :noname { editor -- }
      editor editor-in-minibuffer @ not if
        editor editor-current @ ?dup if
          buffer-prev @ ?dup if
            dup editor editor-current !
            refresh-display
            editor display-search-text
          then
        then
      then
    ; define handle-editor-prev

    \ Go to the next buffer
    :noname { editor -- }
      editor editor-in-minibuffer @ not if
        editor editor-current @ ?dup if
          buffer-next @ ?dup if
            dup editor editor-current !
            refresh-display
            editor display-search-text
          then
        then
      then
    ; define handle-editor-next

    \ Create a new buffer
    :noname { editor -- }
      editor editor-in-minibuffer @ not if
        editor activate-minibuffer
        s" New editor: " editor ['] do-editor-new
        editor editor-minibuffer @ set-prompt
      then
    ; define handle-editor-new

    \ Actually create a new buffer
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor 256 [: { editor data }
          data 256 0 editor editor-minibuffer @ read-minibuffer { len }
          <file-buffer> class-size editor editor-heap @ allocate { buffer }
          data len editor editor-heap @ editor <file-buffer> buffer init-object
          buffer file-buffer-valid? not if
            buffer file-buffer-exception@ file-error-string
            editor editor-minibuffer @ set-message
            buffer destroy
            buffer editor editor-heap @ free
          else
            editor editor-current @ buffer buffer-prev !
            editor editor-current @ buffer-next @ buffer buffer-next !
            buffer buffer-next @ if
              buffer buffer buffer-next @ buffer-prev !
            else
              buffer editor editor-last !
            then
            buffer editor editor-current @ buffer-next !
            buffer editor editor-current !
            editor editor-minibuffer @ clear-minibuffer
          then
        ;] with-allot
        editor deactivate-minibuffer
        editor editor-current @ refresh-display
        editor display-search-text
      then
    ; define do-editor-new

    \ Handle change file path
    :noname { editor -- }
      editor editor-in-minibuffer @ not if
        editor editor-current @ if
          editor editor-current @ handle-change-file-path
        then
      then
      editor display-search-text
    ; define handle-editor-change-file-path

    \ Handle write
    :noname { editor -- }
      editor editor-in-minibuffer @ not if
        editor editor-current @ if
          editor editor-current @ handle-write
        then
      then
      editor display-search-text
    ; define handle-editor-write

    \ Handle revert
    :noname { editor -- }
      editor editor-in-minibuffer @ not if
        editor editor-current @ if
          editor editor-current @ handle-revert
        then
      then
      editor display-search-text
    ; define handle-editor-revert

    \ Handle exit
    :noname { editor -- }
      false editor editor-first @ begin ?dup while
        dup buffer-dirty @ rot or swap buffer-next @
      repeat
      if
        editor activate-minibuffer
        s" A buffer is modified, exit? (yes/no): " editor ['] do-editor-exit
        editor editor-minibuffer @ set-prompt
      else
        true editor editor-exit !
      then
    ; define handle-editor-exit

    \ Handle a prompted exit
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor 256 [: { editor data }
          data 256 0 editor editor-minibuffer @ read-minibuffer { len }
          begin
            len 0> if
              data c@ { byte }
              byte bl <= byte delete = or if
                1 +to data
                -1 +to len
                false
              else
                true
              then
            else
              true
            then
          until
          data c@ { byte }
          byte [char] Y = byte [char] y = or if
            true editor editor-exit ! true
          else
            byte [char] N = byte [char] n = or
          then
          if
            editor editor-minibuffer @ clear-minibuffer
            editor deactivate-minibuffer
            editor editor-current @ refresh-display
            editor display-search-text
          then
        ;] with-allot
      then
    ; define do-editor-exit

    \ Handle editor start
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-start
      else
        editor editor-current @ handle-start
      then
      editor display-search-text
    ; define handle-editor-start

    \ Handle editor end
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-end
      else
        editor editor-current @ handle-end
      then
      editor display-search-text
    ; define handle-editor-end

    \ Handle editor page up
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-page-up
      else
        editor editor-current @ handle-page-up
      then
      editor display-search-text
    ; define handle-editor-page-up

    \ Handle editor page down
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-page-down
      else
        editor editor-current @ handle-page-down
      then
      editor display-search-text
    ; define handle-editor-page-down

    \ Handle editor home
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-doc-home
      else
        editor editor-current @ handle-doc-home
      then
      editor display-search-text
    ; define handle-editor-doc-home

    \ Handle editor end
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-doc-end
      else
        editor editor-current @ handle-doc-end
      then
      editor display-search-text
    ; define handle-editor-doc-end

    \ Handle editor indent
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-indent
      else
        editor editor-current @ handle-indent
      then
      editor display-search-text
    ; define handle-editor-indent

    \ Handle editor unindent
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-unindent
      else
        editor editor-current @ handle-unindent
      then
      editor display-search-text
    ; define handle-editor-unindent

    \ Handle editor find forward
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-search-forward
      else
        editor editor-current @ handle-search-forward
      then
      editor display-search-text
    ; define handle-editor-search-forward

    \ Handle editor find backward
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor editor-minibuffer @ handle-search-backward
      else
        editor editor-current @ handle-search-backward
      then
      editor display-search-text
    ; define handle-editor-search-backward
    
    \ Handle editor refresh
    :noname { editor -- }
      page
      reset-ansi-term
      get-terminal-size display-width ! display-height !
      editor refresh-editor
    ; define handle-editor-refresh

    \ Handle editor close
    :noname { editor -- }
      editor editor-in-minibuffer @ not if
        editor editor-current @ buffer-dirty @ if
          editor activate-minibuffer
          s" The buffer is modified, close? (yes/no): "
          editor ['] do-editor-close
          editor editor-minibuffer @ set-prompt
        else
          editor editor-current @ editor close-editor-buffer
        then
      then
    ; define handle-editor-close
    
    \ Handle a prompted close
    :noname { editor -- }
      editor editor-in-minibuffer @ if
        editor 256 [: { editor data }
          data 256 0 editor editor-minibuffer @ read-minibuffer { len }
          begin
            len 0> if
              data c@ { byte }
              byte bl <= byte delete = or if
                1 +to data
                -1 +to len
                false
              else
                true
              then
            else
              true
            then
          until
          data c@ { byte }
          byte [char] Y = byte [char] y = or if
            editor editor-current @ editor close-editor-buffer true
          else
            byte [char] N = byte [char] n = or
          then
          if
            editor editor-minibuffer @ clear-minibuffer
            editor deactivate-minibuffer
            editor editor-current @ refresh-display
            editor display-search-text
          then
        ;] with-allot
      then
    ; define do-editor-close

    \ Close a buffer
    :noname { buffer editor -- }
      buffer buffer-next @ 0<> buffer buffer-prev @ 0<> or if
        buffer buffer-next @ dup 0= if
          drop buffer buffer-prev @
        then { next-buffer }
        buffer buffer-prev @ if
          buffer buffer-next @
          buffer buffer-prev @ buffer-next !
        else
          buffer buffer-next @
          editor editor-first !
        then
        buffer buffer-next @ if
          buffer buffer-prev @
          buffer buffer-next @ buffer-prev !
        else
          buffer buffer-prev @
          editor editor-last !
        then
        buffer destroy
        buffer editor editor-heap @ free
        buffer editor editor-current @ = if
          next-buffer editor editor-current !
          next-buffer refresh-display
          editor display-search-text
        then
      else
        true editor editor-exit !
      then
    ; define close-editor-buffer

    \ Handle a special key
    :noname { editor -- }
      get-key case
        [char] A of editor handle-editor-up endof
        [char] B of editor handle-editor-down endof
        [char] C of editor handle-editor-forward endof
        [char] D of editor handle-editor-backward endof
        [char] F of editor handle-editor-doc-end endof
        [char] H of editor handle-editor-doc-home endof
        [char] Z of editor handle-editor-unindent endof
        [char] 3 of
          get-key case
            [char] ~ of editor handle-editor-delete-forward endof
            clear-keys
          endcase
        endof
        [char] 5 of
          get-key case
            [char] ~ of editor handle-editor-page-up endof
            clear-keys
          endcase
        endof
        [char] 6 of
          get-key case
            [char] ~ of editor handle-editor-page-down endof
            clear-keys
          endcase
        endof
        clear-keys
      endcase
    ; define handle-special

    \ Handle the escape key
    :noname { editor -- }
      get-key case
        tab of tab editor handle-editor-insert endof
        ctrl-k of editor handle-editor-copy endof
        ctrl-w of editor handle-editor-change-file-path endof
        ctrl-x of editor handle-editor-close endof
        ctrl-r of editor handle-editor-search-backward endof
        [char] [ of editor handle-special endof
        clear-keys
      endcase
    ; define handle-escape

    \ Editor main loop
    :noname { editor -- }
      begin editor editor-exit @ not while
        get-key
        dup bl u< if
          case
            return of editor handle-editor-newline endof
            newline of editor handle-editor-newline endof
            tab of editor handle-editor-indent endof
            ctrl-space of editor handle-editor-select endof
            ctrl-a of editor handle-editor-start endof
            ctrl-e of editor handle-editor-end endof
            ctrl-f of editor handle-editor-forward endof
            ctrl-b of editor handle-editor-backward endof
            ctrl-n of editor handle-editor-next endof
            ctrl-p of editor handle-editor-prev endof
            ctrl-o of editor handle-editor-new endof
            ctrl-r of editor handle-editor-search-forward endof
            ctrl-v of editor handle-editor-exit endof
            ctrl-w of editor handle-editor-write endof
            ctrl-x of editor handle-editor-revert endof
            \	    ctrl-u of handle-editor-insert-row endof
            ctrl-k of editor handle-editor-kill endof
            ctrl-y of editor handle-editor-paste endof
            ctrl-z of editor handle-editor-undo endof
            ctrl-l of editor handle-editor-refresh endof
            escape of editor handle-escape endof
          endcase
          depth 0< if ." *** " then \ DEBUG
        else
          dup delete = if
            drop editor handle-editor-delete
          else
            editor handle-editor-insert
          then
        then
      repeat
    ; define editor-loop
  
end-implement

  \ The line editor
  variable edit-state
  
  \ Configure the editor
  : config-edit ( -- )
    reset-ansi-term
    get-terminal-size display-width ! display-height !
  ;

  \ Calculate heap size
  : get-heap-size ( -- heap-size )
    zeptoed-heap-size my-block-size / my-block-size swap heap-size
  ;

  \ Edit one or more files in a FAT32 filesystem
  : zeptoed ( path-addr path-bytes -- )
    fat32-tools::current-fs@ averts fat32-tools::x-fs-not-set
    get-heap-size [: { path-addr path-bytes my-heap }
      my-block-size zeptoed-heap-size my-block-size / my-heap init-heap
      config-edit
      <editor> class-size my-heap allocate { editor }
      path-addr path-bytes my-heap <editor> editor init-object
      editor editor-valid? not if
        editor editor-exception@ file-error-string cr type
        editor destroy
        editor my-heap free
        exit
      then
      editor [: { editor }
        editor refresh-editor
        editor editor-loop
      ;] try
      editor destroy
      editor my-heap free
      display-height 0 go-to-coord cr
      ?raise
    ;] with-aligned-allot
  ;
  
end-module
 
\ Invoke zeptoed to edit a specified file in a FAT32 filesystem
: zeptoed ( path-addr path-bytes -- )
  zeptoed-internal::zeptoed
;


\ A shorter version of ZEPTOED just because
: zed ( path-addr path-bytes -- )
  zeptoed-internal::zeptoed
;
