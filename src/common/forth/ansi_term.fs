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

begin-module-once ansi-term-module

  begin-import-module ansi-term-internal-module

    \ Saved entered byte
    user saved-key

    \ Show cursor count
    user show-cursor-count

    \ Preserve cursor count
    user preserve-cursor-count
    
  end-module
  
  \ Character constants
  $1B constant escape
    
  \ Type a decimal integer
  : (dec.) ( n -- ) base @ 10 base ! swap (.) base ! ;

  \ Type the CSI sequence
  : csi ( -- )
    begin-critical escape emit [char] [ emit end-critical
  ;

  \ Show the cursor
  : show-cursor ( -- )
    begin-critical csi [char] ? emit 25 (dec.) [char] h emit end-critical
  ;

  \ Hide the cursor
  : hide-cursor ( -- )
    begin-critical csi [char] ? emit 25 (dec.) [char] l emit end-critical
  ;

  \ Save the cursor position
  : save-cursor ( -- ) begin-critical csi [char] s emit end-critical ;

  \ Restore the cursor position
  : restore-cursor ( -- ) begin-critical csi [char] u emit end-critical ;

  \ Scroll up screen by a number of lines
  : scroll-up ( lines -- )
    begin-critical csi (dec.) [char] S emit end-critical
  ;

  \ Move the cursor to specified row and column coordinates
  : go-to-coord ( row column -- )
    begin-critical
    swap csi 1+ (dec.) [char] ; emit 1+ (dec.) [char] f emit
    end-critical
  ;

  \ Erase from the cursor to the end of the line
  : erase-end-of-line ( -- ) begin-critical csi [char] K emit end-critical ;

  \ Erase the lines below the current line
  : erase-down ( -- ) begin-critical csi [char] J emit end-critical ;

  \ Query for the cursor position
  : query-cursor-position ( -- )
    begin-critical csi [char] 6 emit [char] n emit end-critical
  ;

  \ Show the cursor with a show/hide counter
  : show-cursor ( -- )
    1 show-cursor-count +! show-cursor-count @ 0 = if show-cursor then
  ;

  \ Hide the cursor with a show/hide counter
  : hide-cursor ( -- )
    -1 show-cursor-count +! show-cursor-count @ -1 = if hide-cursor then
  ;

  \ Execute code with a hidden cursor
  : execute-hide-cursor ( xt -- ) hide-cursor try show-cursor ?raise ;

  \ Clear a saved key
  : clear-key ( -- ) 0 saved-key ! ;

  \ Get a key
  : get-key ( -- b )
    saved-key @ ?dup if clear-key else key then
  ;
  
  \ Save a key
  : set-key ( b -- ) saved-key ! ;
  
  \ Wait for a number
  : wait-number ( -- n matches )
    ram-here >r get-key dup [char] - = if
      ram-here c! 1 ram-allot
    else
      set-key
    then
    0 begin
      dup 10 < if
	get-key dup [char] 0 >= over [char] 9 <= and if
	  ram-here c! 1 ram-allot 1+ false
	else
	  set-key true
	then
      else
	true
      then
    until
    drop base @ 10 base ! r@ ram-here r@ - parse-unsigned
    rot base ! r> ram-here!
  ;
  
  \ Wait for a character
  : wait-char ( b -- ) begin dup get-key = until drop ;

  \ Confirm that a character is what is expected
  : expect-char ( b -- flag )
    get-key dup rot = if drop true else set-key false then
  ;

  \ Get the cursor position
  : get-cursor-position ( -- row column )
    [:
      begin
	clear-key query-cursor-position escape wait-char
	[char] [ expect-char if
	  wait-number if
	    1 - [char] ; expect-char if
	      wait-number if
		[char] R expect-char if
		  1 - true
		else
		  2drop false
		then
	      else
		drop false
	      then
	    else
	      drop false
	    then
	  else
	    drop false
	  then
	else
	  false
	then
      until
    ;] execute-hide-cursor
  ;

  \ Execute code while preserving cursor position
  : execute-preserve-cursor ( xt -- )
    1 preserve-cursor-count +!
    preserve-cursor-count @ 1 = if
      save-cursor try restore-cursor
    else
      get-cursor-position >r >r try r> r> go-to-coord
    then
    -1 preserve-cursor-count +! ?raise
  ;

  \ Actually get the terminal size
  : get-terminal-size ( -- rows columns )
    [:
      get-cursor-position
      1000 1000 go-to-coord
      get-cursor-position swap 1+ swap 1+
      2swap go-to-coord
    ;] execute-hide-cursor
  ;

  \ Reset terminal state
  : reset-ansi-term ( -- ) 0 show-cursor-count ! 0 saved-key ! ;

end-module

\ Reboot
reboot
