\ Copyright (c) 2021 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-import-module-once line-internal-module

  import ansi-term-module
  import heap-module

  \ Line structure for current task
  user line

  compress-flash

  \ History block count
  128 constant history-block-count

  \ History block size
  16 constant history-block-size

  \ Line structure
  begin-structure line-size
    hfield: line-start-row
    hfield: line-start-column
    hfield: line-terminal-rows
    hfield: line-terminal-columns
    field: line-index-ptr
    field: line-count-ptr
    field: line-buffer-ptr
    field: line-buffer-size
    hfield: line-offset
    hfield: line-count
    field: line-history-first
    field: line-history-current
    history-block-size history-block-count heap-size +field line-history-heap
  end-structure
  
  \ Character constants
  $09 constant tab
  $7F constant delete
  $0A constant newline
  $0D constant return
  $01 constant ctrl-a
  $02 constant ctrl-b
  $05 constant ctrl-e
  $06 constant ctrl-f

  commit-flash
  
  \ Initialize line editing for the current task
  : init-line ( index-ptr count-ptr buffer-ptr buffer-size -- )
    4 align, here line-size allot
    swap 255 min swap tuck line-buffer-size !
    tuck line-buffer-ptr !
    tuck line-count-ptr !
    tuck line-index-ptr !
    0 over line-start-row h!
    0 over line-start-column h!
    0 over line-terminal-rows h!
    0 over line-terminal-columns h!
    0 over line-offset h!
    0 over line-count h!
    0 over line-history-first !
    0 over line-history-current !
    history-block-size history-block-count 2 pick line-history-heap init-heap
    line !
  ;

  \ Get next history item
  : history-next ( history -- next-history ) @ ;

  \ Get history string
  : history-string ( history -- c-string ) cell+ ;

  \ Get first history item
  : history-first ( -- first-history ) line @ line-history-first @ ;

  commit-flash
  
  \ Get previous history item
  : history-prev ( history -- prev-history )
    history-first 2dup <> if
      begin dup history-next 2 pick <> while history-next repeat nip
    else
      2drop 0
    then
  ;

  \ Get last history item
  : history-last ( -- last-history )
    history-first begin dup history-next while history-next repeat
  ;

  commit-flash

  \ Evict history
  : history-evict ( -- )
    history-last
    dup history-first = if
      line @ line-history-heap free 0 line @ line-history-first !
    else
      dup history-prev 0 swap ! line @ line-history-heap free
    then
  ;

  commit-flash

  \ Allocate a string in the history
  : history-allocate ( length -- )
    5 +
    begin
      [: dup line @ line-history-heap allocate ;] try
      dup ['] x-allocate-failed = if
	drop history-evict false
      else
	?raise nip true
      then
    until
  ;

  \ Move a history item to the head of the history
  : history-front ( history -- )
    dup history-prev ?dup if
      over @ swap !
      history-first over !
      line @ line-history-first !
    else
      drop
    then
  ;

  \ Get the current history
  : history-current ( -- history ) line @ line-history-current @ ;

  \ Is the current history the same as what is in the buffer
  : history-changed? ( -- changed? )
    history-current ?dup if
      cell+ count
      line @ line-buffer-ptr @ line @ line-count-ptr @ @ equal-strings? not
    else
      true
    then
  ;

  commit-flash

  \ Add a string in the history
  : history-add ( c-addr bytes -- )
    dup history-allocate
    history-first over !
    dup line @ line-history-first !
    2dup cell+ c!
    5 + swap move
  ;

  \ Initialize for refill
  : init-line-refill ( -- ) >in input# input input-size init-line ;

  \ Get whether a byte is the start of a unicode code point greater than 127
  : unicode-start? ( b -- flag ) $C0 and $C0 = ;

  \ Get whether a byte is part of a unicode code point greater than 127
  : unicode? ( b -- flag ) $80 and 0<> ;

  \ Update the start position
  : update-start-position ( -- )
    get-cursor-position line @ line-start-column h! line @ line-start-row h!
  ;

  \ Update the terminal size
  : update-terminal-size ( -- )
    get-terminal-size line @ line-terminal-columns h! line @
    line-terminal-rows h!
    line @ line-start-column h@ line @ line-terminal-columns h@ >= if
      0 line @ line-start-column h! 1 line @ line-start-row h+!
      line @ line-start-row h@ line @ line-terminal-rows h@ >= if
	line @ line-terminal-rows h@ 1- line @ line-start-row h!
      then
    then
  ;

  \ Get the start position
  : start-position ( -- row column )
    line @ line-start-row h@ line @ line-start-column h@
  ;

  \ Get the position at a character offset
  : offset-position ( offset -- row column )
    line @ line-start-column h@ +
    dup line @ line-terminal-columns h@ / line @ line-start-row h@ +
    swap line @ line-terminal-columns h@ mod
  ;

  \ Get the cursor position
  : cursor-position ( -- row column ) line @ line-offset h@ offset-position ;

  \ Get the end position
  : end-position ( -- row column ) line @ line-count h@ offset-position ;

  \ Calculate number of lines text will take up
  : total-lines ( -- lines )
    line @ line-count h@ line @ line-start-column h@ +
    line @ line-terminal-columns h@ /
  ;

  \ Adjust start row
  : adjust-start-row ( -- )
    line @ line-terminal-rows h@ 1- total-lines
    line @ line-start-row h@ + - dup 0< if
      dup line @ line-start-row h+! scroll-up
    else
      drop
    then
  ;

  commit-flash
  
  \ Type line
  : type-line ( -- )
    0 line @ line-buffer-ptr @ line @ line-count-ptr @ @ +
    line @ line-buffer-ptr @ ?do
      i c@
      dup tab <> if
	dup emit
	dup unicode-start? swap $80 u< or if
	  1+
	then
      else
	drop dup 7 bic 8 + dup rot - 0 ?do $20 emit loop
      then
    loop
    drop
  ;

  commit-flash

  \ Actually update the line editor
  : update-line ( -- )
    [:
      adjust-start-row
      start-position go-to-coord
      type-line
      end-position go-to-coord erase-down erase-end-of-line
      cursor-position go-to-coord
    ;] execute-hide-cursor
  ;
  
  \ Reset the line editor state for a new line of input
  : reset-line ( -- )
    reset-ansi-term
    0 line @ line-index-ptr @ ! 0 line @ line-count-ptr @ !
    0 line @ line-offset h! 0 line @ line-count h!
    update-start-position update-terminal-size
  ;

  \ Get the number of character spaces in the buffer up to a given index
  : get-spaces-to-index ( index -- spaces )
    line @ line-count-ptr @ @ min
    0 swap 0 ?do
      line @ line-buffer-ptr @ i + c@ dup tab = if
	drop 7 bic 8 +
      else
	dup unicode-start? if
	  drop 1+
	else
	  dup $20 >= swap $80 < and if
	    1+
	  then
	then
      then
    loop
  ;

  \ Delete a byte at an index
  : delete-byte ( -- )
    line @ line-index-ptr @ @ 0> if
      line @ line-buffer-ptr @ line @ line-index-ptr @ @ + dup 1-
      line @ line-count-ptr @ @ line @ line-index-ptr @ @ - move
      -1 line @ line-index-ptr @ +!
      -1 line @ line-count-ptr @ +!
    then
  ;

  \ Delete bytes forward
  : delete-byte-forward ( -- )
    line @ line-count-ptr @ @ line @ line-index-ptr @ @ > if
      line @ line-buffer-ptr @ line @ line-index-ptr @ @ + dup swap 1+ swap
      line @ line-count-ptr @ @ 1- line @ line-index-ptr @ @ - move
      -1 line @ line-count-ptr @ +!
    then
  ;

  \ Insert a byte at an index
  : insert-byte ( b -- success )
    line @ line-count-ptr @ @ line @ line-buffer-size @ < if
      line @ line-buffer-ptr @ line @ line-index-ptr @ @ + dup 1+
      line @ line-count-ptr @ @ line @ line-index-ptr @ @ - move
      line @ line-buffer-ptr @ line @ line-index-ptr @ @ + c!
      1 line @ line-index-ptr @ +!
      1 line @ line-count-ptr @ +!
      true
    else
      drop false
    then
  ;

  \ Append a byte
  : append-byte ( b -- success )
    line @ line-count-ptr @ @ line @ line-buffer-size @ < if
      line @ line-buffer-ptr @ line @ line-index-ptr @ @ + c!
      1 line @ line-index-ptr @ +!
      1 line @ line-count-ptr @ +!
      true
    else
      drop false
    then
  ;

  \ Get the number of bytes of the character to the left of the cursor
  : left-bytes ( -- count )
    0 line @ line-index-ptr @ @ begin
      dup 0> if
	swap 1+ swap 1- dup line @ line-buffer-ptr @ + c@
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
    line @ line-index-ptr @ @ line @ line-count-ptr @ @ < if
      line @ line-buffer-ptr @ line @ line-index-ptr @ @ + c@
      dup $80 u< if
	drop 1
      else
	1 line @ line-index-ptr @ @ 1+ begin
	  dup line @ line-count-ptr @ @ u< if
	    dup line @ line-buffer-ptr @ + c@
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

  commit-flash
  
  \ Handle delete
  : handle-delete ( -- )
    line @ line-offset h@ 0> if
      left-bytes
      line @ line-index-ptr @ @ over - get-spaces-to-index
      line @ line-index-ptr @ @ get-spaces-to-index -
      dup line @ line-offset h+! line @ line-count h+!
      0 ?do delete-byte loop
      update-line
    then
  ;

  \ Handle delete forward
  : handle-delete-forward ( -- )
    line @ line-offset h@ line @ line-count h@ < if
      right-bytes 0 ?do delete-byte-forward loop
      line @ line-count-ptr @ @ get-spaces-to-index line @ line-count h!
      update-line
    then
  ;

  \ Handle insert
  : handle-insert ( b -- )
    line @ line-index-ptr @ @ line @ line-count-ptr @ @ < if
      line @ line-index-ptr @ @ get-spaces-to-index swap
      insert-byte if
	line @ line-index-ptr @ @ get-spaces-to-index swap -
	dup line @ line-offset h+! line @ line-count h+!
	update-line
      else
	drop
      then
    else
      line @ line-index-ptr @ @ get-spaces-to-index swap
      dup tab <> if
	append-byte if
	  line @ line-index-ptr @ @ get-spaces-to-index swap -
	  dup line @ line-offset h+! line @ line-count h+!
	  line @ line-buffer-ptr @ line @ line-index-ptr @ @ + 1- c@ emit
	then
      else
	append-byte if
	  line @ line-index-ptr @ @ get-spaces-to-index swap - 0 ?do
	    1 line @ line-offset h+! 1 line @ line-count h+!
	    $20 emit
	  loop
	then
      then
    then
  ;

  \ Handle going forward
  : handle-forward ( -- )
    line @ line-index-ptr @ @ line @ line-count-ptr @ @ < if
      line @ line-index-ptr @ @ get-spaces-to-index
      right-bytes line @ line-index-ptr @ +!
      line @ line-index-ptr @ @ get-spaces-to-index swap -
      line @ line-offset h+!
      update-line
    then
  ;

  \ Handle going backward
  : handle-backward ( -- )
    line @ line-index-ptr @ @ 0> if
      line @ line-index-ptr @ @ get-spaces-to-index
      left-bytes negate line @ line-index-ptr @ +!
      line @ line-index-ptr @ @ get-spaces-to-index swap -
      line @ line-offset h+!
      update-line
    then
  ;

  \ Handle going to the start
  : handle-start ( -- )
    0 line @ line-index-ptr @ !
    0 line @ line-offset h!
    update-line
  ;

  \ Handle going to the end
  : handle-end ( -- )
    line @ line-count-ptr @ @ line @ line-index-ptr @ !
    line @ line-count h@ line @ line-offset h!
    update-line
  ;

  \ Write history to the buffer
  : set-buffer-for-history ( history -- )
    cell+ dup c@ dup line @ line-index-ptr @ ! dup line @ line-count-ptr @ !
    dup get-spaces-to-index dup line @ line-offset h! line @ line-count h!
    swap 1+ line @ line-buffer-ptr @ rot move
    update-line
  ;

  \ Clear buffer
  : clear-buffer ( -- )
    0 line @ line-offset h! 0 line @ line-count h!
    0 line @ line-index-ptr @ ! 0 line @ line-count-ptr @ !
    update-line
  ;

  \ Handle going to the next history item
  : handle-history-next ( -- )
    history-current if
      history-current history-next ?dup if
	dup line @ line-history-current ! set-buffer-for-history
      then
    else
      history-first if
	history-first dup line @ line-history-current ! set-buffer-for-history
      then
    then
  ;

  \ Handle going to the next history item
  : handle-history-prev ( -- )
    history-current if
      history-current history-first <> if
	history-current history-prev dup line @ line-history-current !
	set-buffer-for-history
      else
	0 line @ line-history-current ! clear-buffer
      then
    then
  ;

  commit-flash

  \ Handle the escape key
  : handle-escape ( -- )
    get-key case
      [char] [ of
	get-key case
	  [char] A of handle-history-next endof
	  [char] B of handle-history-prev endof
	  [char] C of handle-forward endof
	  [char] D of handle-backward endof
	  [char] 3 of
	    get-key case
	      [char] ~ of handle-delete-forward endof
	      dup set-key
	    endcase
	  endof
	  dup set-key
	endcase
      endof
      dup set-key
    endcase
  ;

  commit-flash
  
  \ The line editor
  : line-edit ( -- )
    xon ack reset-line
    begin
      get-key
      dup $20 u< if 
	case
	  return of true endof
	  newline of true endof
	  tab of tab handle-insert false endof
	  ctrl-a of handle-start false endof
	  ctrl-e of handle-end false endof
	  ctrl-f of handle-forward false endof
	  ctrl-b of handle-backward false endof
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
    history-changed? if
      line @ line-count-ptr @ @ if
	line @ line-buffer-ptr @ line @ line-count-ptr @ @ history-add
      then
    else
      history-current history-front
    then
    0 line @ line-history-current !
    end-position go-to-coord
    0 line @ line-index-ptr @ !
    xoff
  ;

end-module

import internal-module

\ Initialize
: init ( -- )
  init
  init-line-refill
;

commit-flash

\ Enable line editor
: enable-line ( -- ) ['] line-edit refill-hook ! ;

\ Disable line editor
: disable-line ( -- ) ['] do-refill refill-hook ! ;

\ Get whether the line editor is enabled
: line-enabled? ( -- ) refill-hook @ ['] line-edit = ;

unimport internal-module
unimport line-internal-module

end-compress-flash

\ Reboot
reboot
