\ Copyright (c) 2021-2023 Travis Bemann
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

begin-module line-internal

  ansi-term import
  heap import

  \ Line structure for current task
  user line

  compress-flash
  
  \ History block count
  128 constant history-block-count

  \ History block size
  16 constant history-block-size

  \ Line editor is in upload mode
  0 bit constant line-upload-mode

  \ Clipboard size
  128 constant clipboard-size

  \ No clipboard end
  0 constant no-clipboard-end

  \ Left clipboard end
  1 constant left-clipboard-end

  \ Right clipboard end
  2 constant right-clipboard-end
  
  commit-flash
  
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
    field: line-flags
    field: line-edit-deferred
    hfield: line-clipboard-count
    hfield: line-clipboard-end
    clipboard-size +field line-clipboard
    history-block-size history-block-count heap-size +field line-history-heap
  end-structure
  
  \ Character constants
  $08 constant backspace
  $09 constant tab
  $7F constant delete
  $0A constant newline
  $0D constant return
  $01 constant ctrl-a
  $02 constant ctrl-b
  $05 constant ctrl-e
  $06 constant ctrl-f
  $0B constant ctrl-k
  $15 constant ctrl-u
  $17 constant ctrl-w
  $19 constant ctrl-y
  
  commit-flash
  
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
    line @ line-terminal-columns h@ / 1+
  ;

  \ Adjust start row
  : adjust-start-row ( -- )
    line @ line-terminal-rows h@ 1- total-lines 1-
    line @ line-start-row h@ + -
    dup 0< if
      dup line @ line-start-row h+! negate scroll-up
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
    line-upload-mode line @ line-flags bic!
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
	  dup $20 >= swap $7F < and if
	    1+
	  then
	then
      then
    loop
  ;

  \ Evict a character from the left-hand side of the clipboard
  : evict-clipboard-left ( -- )
    line @ line-clipboard-count h@ 0> if
      0 { bytes }
      begin
        bytes line @ line-clipboard-count h@ < if
          line @ line-clipboard bytes + c@ { c }
          c unicode-start? if
            count 0= if 1 +to bytes false else true then
          else
            c unicode? if 1 +to bytes false else true then
          then
        else
          true
        then
      until
      line @ line-clipboard bytes + line @ line-clipboard
      line @ line-clipboard-count h@ bytes - move
      bytes negate line @ line-clipboard-count h+!
    then
  ;

  \ Evict a character from the right-hand side of the clipboard
  : evict-clipboard-right ( -- )
    line @ line-clipboard-count h@ 0> if
      1 { bytes }
      begin
        bytes line @ line-clipboard-count h@ < if
          line @ line-clipboard bytes - c@ { c }
          c unicode? c unicode-start? not and if
            1 +to bytes false
          else
            true
          then
        else
          true
        then
      until
      bytes negate line @ line-clipboard-count h+!
    then
  ;

  commit-flash

  \ Insert a byte on the left-hand side of the clipboard
  : insert-clipboard-left { c -- }
    line @ line-clipboard-end h@ left-clipboard-end = if
      line @ line-clipboard-count h@ clipboard-size = if
        evict-clipboard-right
      then
    else
      0 line @ line-clipboard-count h!
    then
    left-clipboard-end line @ line-clipboard-end h!
    line @ line-clipboard line @ line-clipboard 1+
    line @ line-clipboard-count h@ move
    c line @ line-clipboard c!
    1 line @ line-clipboard-count h+!
  ;

  \ Insert a byte on the right-hand side of the clipboard
  : insert-clipboard-right { c -- }
    line @ line-clipboard-end h@ right-clipboard-end = if
      line @ line-clipboard-count h@ clipboard-size = if
        evict-clipboard-left
      then
    else
      0 line @ line-clipboard-count h!
    then
    right-clipboard-end line @ line-clipboard-end h!
    c line @ line-clipboard line @ line-clipboard-count h@ + c!
    1 line @ line-clipboard-count h+!
  ;

  commit-flash
  
  \ Delete a byte at an index
  : delete-byte ( -- )
    line @ line-index-ptr @ @ 0> if
      line @ line-index-ptr @ @ line @ line-buffer-ptr @ + 1- c@
      insert-clipboard-left
      line @ line-buffer-ptr @ line @ line-index-ptr @ @ + dup 1-
      line @ line-count-ptr @ @ line @ line-index-ptr @ @ - move
      -1 line @ line-index-ptr @ +!
      -1 line @ line-count-ptr @ +!
    then
  ;

  \ Delete bytes forward
  : delete-byte-forward ( -- )
    line @ line-count-ptr @ @ line @ line-index-ptr @ @ > if
      line @ line-index-ptr @ @ line @ line-buffer-ptr @ + c@
      insert-clipboard-right
      line @ line-buffer-ptr @ line @ line-index-ptr @ @ + dup swap 1+ swap
      line @ line-count-ptr @ @ 1- line @ line-index-ptr @ @ - move
      -1 line @ line-count-ptr @ +!
    then
  ;

  \ Delete bytes matching a predicate
  : delete-bytes { xt -- }
    begin
      line @ line-index-ptr @ @ 0> if
        line @ line-index-ptr @ @ line @ line-buffer-ptr @ + 1- c@
        xt execute if delete-byte false else true then
      else
        true
      then
    until
  ;

  \ Delete until the start of the line
  : delete-start ( -- )
    line @ line-index-ptr @ @ 0> if
      0 line @ line-index-ptr @ @ 1- ?do
        line @ line-buffer-ptr @ i + c@ insert-clipboard-left
      -1 +loop
    then
    line @ line-index-ptr @ @ negate line @ line-count-ptr @ +!
    0 line @ line-index-ptr @ !
  ;
  
  \ Delete until the end of the line
  : delete-end ( -- )
    line @ line-count-ptr @ @ line @ line-index-ptr @ @ ?do
      line @ line-buffer-ptr @ i + c@ insert-clipboard-right
    loop
    line @ line-index-ptr @ @ line @ line-count-ptr @ !
  ;

  \ Insert a byte at an index
  : insert-byte ( b -- success )
    no-clipboard-end line @ line-clipboard-end h!
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
    no-clipboard-end line @ line-clipboard-end h!
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
    0 { bytes }
    begin line @ line-index-ptr @ @ bytes - 0> while
      line @ line-index-ptr @ @ bytes 1+ - line @ line-buffer-ptr @ + c@ { c }
      c unicode-start? if
        bytes 1+ exit
      else
        c unicode? if
          1 +to bytes
        else
          bytes 0= if 1 else bytes then exit
        then
      then
    repeat
    bytes
  ;

  \ Get the number of bytes of the character to the right of the cursor
  : right-bytes ( -- count )
    0 { bytes }
    begin line @ line-index-ptr @ @ bytes + line @ line-count-ptr @ @ < while
      line @ line-index-ptr @ @ bytes + line @ line-buffer-ptr @ + c@ { c }
      c unicode-start? if
        bytes 0= if
          1 +to bytes
        else
          bytes exit
        then
      else
        c unicode? if
          1 +to bytes
        else
          bytes 0= if 1 else bytes then exit
        then
      then
    repeat
    bytes
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

  \ Handle delete word
  : handle-delete-word ( -- )
    line @ line-offset h@ 0> if
      line @ line-index-ptr @ @ get-spaces-to-index { start-count }
      [: dup bl = swap tab = or ;] delete-bytes
      [: dup bl <> swap tab <> and ;] delete-bytes
      line @ line-index-ptr @ @ get-spaces-to-index { end-count }
      end-count start-count - dup line @ line-offset h+! line @ line-count h+!
      update-line
    then
  ;

  \ Handle deleting to the start of the line
  : handle-delete-start ( -- )
    delete-start
    line @ line-offset h@ negate line @ line-count h+!
    0 line @ line-offset h!
    update-line
  ;

  \ Handle deleting to the end of the line
  : handle-delete-end ( -- )
    delete-end
    line @ line-offset h@ line @ line-count h!
    update-line
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

  \ Determine how much clipboard can actually be pasted
  : get-paste-count ( -- count )
    line @ line-clipboard-count h@ { paste-count }
    begin
      line @ line-count-ptr @ @ paste-count + line @ line-buffer-size @ >
      paste-count 0> and
    while
      line @ line-clipboard paste-count + 1- c@ { c }
      c unicode? if
        begin
          paste-count 0> if
            line @ line-clipboard paste-count + 1- c@ { c }
            c unicode-start? if
              -1 +to paste-count true
            else
              c unicode? if
                -1 +to paste-count false
              else
                true
              then
            then
          else
            true
          then
        until
      else
        -1 +to paste-count
      then
    repeat
    paste-count
  ;
  
  \ Handle paste
  : handle-paste ( -- )
    line @ line-index-ptr @ @ get-spaces-to-index { start-count }
    get-paste-count { paste-count }
    false { pasted? }
    paste-count 0 ?do
      line @ line-clipboard i + c@ insert-byte pasted? or to pasted?
    loop
    pasted? if
      line @ line-index-ptr @ @ get-spaces-to-index { end-count }
      end-count start-count - dup line @ line-offset h+! line @ line-count h+!
      update-line
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

  \ Check whether to leave upload mode
  : continue-upload? ( -- continue )
    get-key
    dup escape = if
      drop get-key
      dup [char] O = if
	drop get-key dup [char] Q = if false else set-key true then \ F2
      else
	set-key true
      then
    else
      set-key true
    then
  ;

  \ Handle uploading a byte
  : handle-upload-byte ( c -- )
    line @ line-count-ptr @ @ line @ line-buffer-size @ < if
      dup emit
      line @ line-buffer-ptr @ line @ line-index-ptr @ @ + c!
      1 line @ line-index-ptr @ +!
      1 line @ line-count-ptr @ +!
    else
      drop
    then
  ;

  \ Handle deleting a byte in upload mode
  : handle-upload-delete-byte ( -- )
    line @ line-index-ptr @ @ 0> if
      -1 line @ line-index-ptr @ +!
      -1 line @ line-count-ptr @ +!
      backspace emit space backspace emit
    then
  ;

  \ Reset upload mode
  : reset-upload ( -- )
    0 line @ line-index-ptr @ ! 0 line @ line-count-ptr @ !
  ;

  commit-flash

  \ Handle upload mode
  : handle-upload ( -- )
    reset-upload xon ack
    continue-upload? if
      begin
	get-key
	dup $20 u< if 
	  case
	    return of true endof
	    newline of true endof
	    tab of tab handle-upload-byte false endof
	    swap false swap
	  endcase 
	else
	  dup delete = if
	    drop handle-upload-delete-byte
	  else
	    handle-upload-byte
	  then
	  false
	then
      until
      0 line @ line-index-ptr @ !
      xoff
    else
      cr ." leaving upload mode"
      line @ line-edit-deferred @ refill-hook !
      line-upload-mode line @ line-flags bic!
    then
  ;
    
  commit-flash

  \ Set upload mode
  : handle-set-upload ( -- )
    0 line @ line-index-ptr @ ! 0 line @ line-count-ptr @ !
    0 line @ line-offset h! 0 line @ line-count h!
    update-line
    s" entering upload mode " tuck type
    offset-position go-to-coord
    ['] handle-upload refill-hook !
    line-upload-mode line @ line-flags bis!
  ;
  
  commit-flash

  \ Handle the escape key
  : handle-escape ( -- exit-line-editor )
    get-key case
      [char] [ of
	get-key case
	  [char] A of handle-history-next false endof \ Up arrow
	  [char] B of handle-history-prev false endof \ Down arrow
	  [char] C of handle-forward false endof \ Forward
	  [char] D of handle-backward false endof \ Backward
	  [char] 3 of
	    get-key case
	      [char] ~ of handle-delete-forward false endof \ Delete
	      dup set-key swap false swap
	    endcase
	  endof
	  dup set-key swap false swap
	endcase
      endof
      [char] O of
	get-key case
	  [char] P of handle-set-upload true endof \ F1
	  dup set-key swap false swap
	endcase
      endof
      dup set-key swap false swap
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
          ctrl-w of handle-delete-word false endof
          ctrl-u of handle-delete-start false endof
          ctrl-k of handle-delete-end false endof
          ctrl-y of handle-paste false endof
	  escape of handle-escape endof
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
    line @ line-flags @ line-upload-mode and 0= if
      history-changed? if
	line @ line-count-ptr @ @ if
	  line @ line-buffer-ptr @ line @ line-count-ptr @ @ history-add
	then
      else
	history-current history-front
      then
      end-position go-to-coord
    then
    0 line @ line-history-current !
    0 line @ line-index-ptr @ !
    xoff
  ;

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
    0 over line-flags !
    0 over line-clipboard-count h!
    0 over line-clipboard-end h!
    ['] line-edit over line-edit-deferred !
    history-block-size history-block-count 2 pick line-history-heap init-heap
    line !
  ;
  
  \ Initialize for refill
  : init-line-refill ( -- ) >in input# input input-size init-line ;

end-module> import

internal import

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

end-compress-flash

\ Reboot
reboot
