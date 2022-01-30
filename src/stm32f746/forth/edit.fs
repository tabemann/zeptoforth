\ Copyright (c) 2021-2022 Travis Bemann
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

begin-module edit-internal

  systick import
  block import
  ansi-term import

  \ Edit buffer count (must be <= 32)
  9 constant buffer-count

  \ Half buffer count
  buffer-count 1- 2 / constant half-buffer-count
  
  \ Buffer width
  64 constant buffer-width

  \ Buffer height
  block-size buffer-width / constant buffer-height

  \ Tab size
  2 constant tab-size \ small to save space in the blocks

  \ Edit structure
  begin-structure edit-size
    field: edit-start-row
    field: edit-start-column
    field: edit-terminal-rows
    field: edit-terminal-columns
    field: edit-cursor-row
    field: edit-cursor-column
    field: edit-saved-cursor-column
    field: edit-current
    field: edit-unicode-entered
    field: edit-dirty \ one bit per dirty flag, starting from bit 0
    buffer-count cells +field edit-ids
    buffer-count block-size * +field edit-buffers
  end-structure

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
  : x-buffer-not-found ( -- ) space ." buffer not found" cr ;
  
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
    [char] + emit buffer-width 0 ?do [char] - emit loop [char] + emit
    buffer-height 0 ?do
      cr [char] | emit buffer-width 0 ?do space loop [char] | emit
    loop
    cr [char] + emit buffer-width 0 ?do [char] - emit loop [char] + emit
  ;

  \ Actually draw a row of the block editor
  : draw-row ( row -- )
    [:
      dup edit-state @ edit-start-row @ +
      edit-state @ edit-start-column @ go-to-coord
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
      edit-state @ edit-current @ id@ (.)
      edit-state @ edit-current @ dirty? if
	space [char] * emit
      then
      space ." ]-"
      get-cursor-position nip
      buffer-width 1+ swap - 0 ?do [char] - emit loop
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
    dup dup highest-id u> if
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
    \ 0 buffer-width 0 ?do
    \   i current-row + c@
    \   dup $20 >= over $80 < and if
    \ 	drop 1+
    \   else
    \ 	dup unicode? swap unicode-start? and if
    \ 	  1+
    \ 	then
    \   then
    \ loop
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
    edit-state @ edit-cursor-row @ edit-state @ edit-cursor-column @ go-to-coord
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
    current-row-index@ row-len 0> if
      current-row buffer-width $20 fill
      0 current-column-index!
      save-current-column
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
    ram-here edit-size ram-allot edit-state !
    reset-ansi-term
    draw-empty
    get-cursor-position
    false edit-state @ edit-unicode-entered !
    buffer-width 1+ - 0 max edit-state @ edit-start-column !
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
    update-all
  ;

  \ Leave the editor
  : leave-edit ( -- )
    edit-state @ edit-start-row @ buffer-height +
    edit-state @ edit-start-column @ buffer-width 1+ + go-to-coord
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
	  dup set-key
	endcase
      endof
      dup set-key
    endcase
  ;

  \ Handle the escape key
  : handle-escape ( -- )
    get-key case
      [char] [ of handle-special endof
      dup set-key
    endcase
  ;
  
  \ Edit a block
  : edit ( id -- )
    dup $FFFFFFFF <> averts x-invalid-block-id
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
  ;

end-module> import

\ Edit a block
: edit ( id -- ) edit ;

\ Reboot
reboot
