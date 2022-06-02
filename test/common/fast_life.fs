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

begin-module life

  ansi-term import
  
  \ Life width
  variable life-width

  \ Life height
  variable life-height

  \ Life display x coordinate
  variable life-display-x

  \ Life display y coordinate
  variable life-display-y

  \ Life display width
  variable life-display-width

  \ Life display height
  variable life-display-height

  \ Life display x end coordinate
  variable life-display-x-end

  \ Life display y end coordinate
  variable life-display-y-end
  
  \ Life buffer 0
  variable life-buffer-0

  \ Life buffer 1
  variable life-buffer-1

  \ Life line buffer
  variable life-line-buffer

  \ Current buffer
  variable current-buffer

  \ New buffer
  variable new-buffer
  
  \ Wrap a life cell coordinate
  : wrap-coord ( x y -- x y )
    dup life-height @ >= if drop 0 then
    dup 0< if drop life-height @ 1- then swap
    dup life-width @ >= if drop 0 then
    dup 0< if drop life-width @ 1- then swap
  ;

  \ Get whether a life cell is alive
  : alive? ( x y -- alive? ) life-width @ * + current-buffer @ + c@ 16 >= ;

  \ Add alive neighbor
  : +alive ( x y -- ) life-width @ * + current-buffer @ + 1 swap c+! ;

  \ Remove alive neighbor
  : -alive ( x y -- ) life-width @ * + current-buffer @ + -1 swap c+! ;

  \ Set a life cell to be dead
  : set-dead ( x y -- )
    2dup 1+ wrap-coord -alive
    2dup 1+ swap 1+ swap wrap-coord -alive
    2dup 1+ swap 1- swap wrap-coord -alive
    2dup swap 1+ swap wrap-coord -alive
    2dup 1- wrap-coord -alive
    2dup 1- swap 1+ swap wrap-coord -alive
    2dup 1- swap 1- swap wrap-coord -alive
    2dup swap 1- swap wrap-coord -alive
    life-width @ * + current-buffer @ + 16 swap cbic!
  ;

  \ Set a life cell to be dead
  : set-alive ( x y -- )
    2dup 1+ wrap-coord +alive
    2dup 1+ swap 1+ swap wrap-coord +alive
    2dup 1+ swap 1- swap wrap-coord +alive
    2dup swap 1+ swap wrap-coord +alive
    2dup 1- wrap-coord +alive
    2dup 1- swap 1+ swap wrap-coord +alive
    2dup 1- swap 1- swap wrap-coord +alive
    2dup swap 1- swap wrap-coord +alive
    life-width @ * + current-buffer @ + 16 swap cbis!
  ;
  
  \ Set whether a life cell is alive
  : alive! ( alive? x y -- )
    rot if
      2dup alive? not if set-alive else 2drop then
    else
      2dup alive? if set-dead else 2drop then
    then
  ;

  \ Get whether a new life cell is alive
  : new-alive? ( x y -- alive? ) life-width @ * + new-buffer @ + c@ 16 >= ;

  \ Add new alive neighbor
  : +new-alive ( x y -- ) life-width @ * + new-buffer @ + 1 swap c+! ;

  \ Remove new alive neighbor
  : -new-alive ( x y -- ) life-width @ * + new-buffer @ + -1 swap c+! ;

  \ Set a new life cell to be dead
  : set-new-dead ( x y -- )
    2dup 1+ wrap-coord -new-alive
    2dup 1+ swap 1+ swap wrap-coord -new-alive
    2dup 1+ swap 1- swap wrap-coord -new-alive
    2dup swap 1+ swap wrap-coord -new-alive
    2dup 1- wrap-coord -new-alive
    2dup 1- swap 1+ swap wrap-coord -new-alive
    2dup 1- swap 1- swap wrap-coord -new-alive
    2dup swap 1- swap wrap-coord -new-alive
    life-width @ * + new-buffer @ + 16 swap cbic!
  ;

  \ Set a new life cell to be dead
  : set-new-alive ( x y -- )
    2dup 1+ wrap-coord +new-alive
    2dup 1+ swap 1+ swap wrap-coord +new-alive
    2dup 1+ swap 1- swap wrap-coord +new-alive
    2dup swap 1+ swap wrap-coord +new-alive
    2dup 1- wrap-coord +new-alive
    2dup 1- swap 1+ swap wrap-coord +new-alive
    2dup 1- swap 1- swap wrap-coord +new-alive
    2dup swap 1- swap wrap-coord +new-alive
    life-width @ * + new-buffer @ + 16 swap cbis!
  ;

  \ Set whether a life cell is alive
  : new-alive! ( alive? x y -- )
    rot if
      2dup alive? not if set-new-alive else 2drop then
    else
      2dup alive? if set-new-dead else 2drop then
    then
  ;

  \ Set a new non-border life cell to be dead
  : set-new-dead-nonborder ( x y -- )
    2dup 1+ -new-alive
    2dup 1+ swap 1+ swap -new-alive
    2dup 1+ swap 1- swap -new-alive
    2dup swap 1+ swap -new-alive
    2dup 1- -new-alive
    2dup 1- swap 1+ swap -new-alive
    2dup 1- swap 1- swap -new-alive
    2dup swap 1- swap -new-alive
    life-width @ * + new-buffer @ + 16 swap cbic!
  ;

  \ Set a new non-border life cell to be dead
  : set-new-alive-nonborder ( x y -- )
    2dup 1+ +new-alive
    2dup 1+ swap 1+ swap +new-alive
    2dup 1+ swap 1- swap +new-alive
    2dup swap 1+ swap +new-alive
    2dup 1- +new-alive
    2dup 1- swap 1+ swap +new-alive
    2dup 1- swap 1- swap +new-alive
    2dup swap 1- swap +new-alive
    life-width @ * + new-buffer @ + 16 swap cbis!
  ;

  \ Non-border cell sate table
  create nonborder-state-table
  ' 2drop ,
  ' 2drop ,
  ' 2drop ,
  ' set-new-alive-nonborder ,
  ' 2drop ,
  ' 2drop ,
  ' 2drop ,
  ' 2drop ,
  ' 2drop ,
  ' 2drop , ' 2drop , ' 2drop , ' 2drop , ' 2drop , ' 2drop , ' 2drop ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  ' 2drop ,
  ' 2drop ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,
  ' set-new-dead-nonborder ,

  \ Cycle a non-border life cell
  : cycle-cell ( x y -- )
    2dup life-width @ * + current-buffer @ + c@ 2 lshift
    nonborder-state-table + @ execute
  ;

  \ Switch new buffer with old buffer
  : switch-buffers ( -- )
    current-buffer @ new-buffer @ current-buffer ! new-buffer !
  ;
  
  \ Execute a life cycle
  : cycle-life ( -- )
    current-buffer @ new-buffer @ life-width @ life-height @ * move
    0 begin dup life-height @ < while
      0 over false -rot new-alive!
      life-width @ 1- over false -rot new-alive!
      1+
    repeat
    drop
    0 begin dup life-width @ < while
      dup 0 false -rot new-alive!
      dup life-height @ 1- false -rot new-alive!
      1+
    repeat
    drop
    1 begin dup life-width @ 1- < while
      1 begin dup life-height @ 1- < while
	2dup cycle-cell 1+
      repeat
      drop 1+
    repeat
    drop
    switch-buffers
  ;

  \ Display the life viewport
  : display-life ( -- )
    hide-cursor 0 0 go-to-coord
    life-display-y @ begin dup life-display-y-end @ < while
      life-display-x @ begin dup life-display-x-end @ < while
	2dup swap alive? if ." x" else space then 1+
      repeat
      drop 1+ cr
    repeat
    show-cursor
  ;

  \ Initialize sixel display
  : init-sixel ( -- )
    escape emit ." Pq#0;2;0;0;0#1;2;100;100;100"
  ;

  \ Sixel RLE count
  variable sixel-rle-count

  \ Sixel RLE value
  variable sixel-rle-value

  \ Initialize sixel RLE
  : init-sixel-rle ( -- ) 0 sixel-rle-count ! 0 sixel-rle-value ! ;
  
  \ Add sixel RLE value
  : add-sixel-rle ( b -- )
    sixel-rle-count @ 0= if
      1 sixel-rle-count !
      sixel-rle-value !
    else
      dup sixel-rle-value @ <> if
	sixel-rle-count @ 3 > if
	  ." !" sixel-rle-count @ (dec.)
	else
	  sixel-rle-count @ 2 > if
	    sixel-rle-value @ emit
	  then
	  sixel-rle-count @ 1 > if
	    sixel-rle-value @ emit
	  then
	then
	sixel-rle-value @ emit
	0 sixel-rle-count !
      then
      sixel-rle-value !
      1 sixel-rle-count +!
    then
  ;

  \ Add two sixel RLE value
  : add-sixel-rle2 ( b -- )
    sixel-rle-count @ 0= if
      2 sixel-rle-count !
      sixel-rle-value !
    else
      dup sixel-rle-value @ <> if
	sixel-rle-count @ 3 > if
	  ." !" sixel-rle-count @ (dec.)
	else
	  sixel-rle-count @ 2 > if
	    sixel-rle-value @ emit
	  then
	  sixel-rle-count @ 1 > if
	    sixel-rle-value @ emit
	  then
	then
	sixel-rle-value @ emit
	0 sixel-rle-count !
      then
      sixel-rle-value !
      2 sixel-rle-count +!
    then
  ;
  
  \ Force sixel RLE output
  : force-sixel-rle-out ( -- )
    sixel-rle-count @ 0<> if
      sixel-rle-count @ 3 > if
	." !" sixel-rle-count @ (dec.)
      else
	sixel-rle-count @ 2 > if
	  sixel-rle-value @ emit
	then
	sixel-rle-count @ 1 > if
	  sixel-rle-value @ emit
	then
      then
      sixel-rle-value @ emit
    then
  ;

  \ Part two of displaying the life viewport using sixels
  : display-life-sixel-part1 ( -- )
        ." #1" init-sixel-rle
    life-display-x @ begin dup life-display-x-end @ < while
      0 >r
      life-display-y-end @ 6 / 6 * life-height @ < if
	dup life-display-y-end @ 6 / 6 * alive? 1 and r> or >r
      then
      life-display-y-end @ 6 / 6 * 1 + life-height @ < if
	dup life-display-y-end @ 6 / 6 * 1 + alive? 2 and r> or >r
      then
      life-display-y-end @ 6 / 6 * 2 + life-height @ < if
	dup life-display-y-end @ 6 / 6 * 2 + alive? 4 and r> or >r
      then
      life-display-y-end @ 6 / 6 * 3 + life-height @ < if
	dup life-display-y-end @ 6 / 6 * 3 + alive? 8 and r> or >r
      then
      life-display-y-end @ 6 / 6 * 4 + life-height @ < if
	dup life-display-y-end @ 6 / 6 * 4 + alive? 16 and r> or >r
      then
      life-display-y-end @ 6 / 6 * 5 + life-height @ < if
	dup life-display-y-end @ 6 / 6 * 5 + alive? 32 and r> or >r
      then
      dup life-line-buffer @ + r@ swap c!
      r> [char] ? + add-sixel-rle
      1+
    repeat
    drop
    force-sixel-rle-out ." $#0" init-sixel-rle
    life-display-x @ begin dup life-display-x-end @ < while
      dup life-line-buffer @ + c@ 63 swap bic
      [char] ? + add-sixel-rle
      1+
    repeat
    drop
    force-sixel-rle-out ." -"
    escape emit $5C emit show-cursor
  ;

  \ Display the life viewport using sixels
  : display-life-sixel ( -- )
    hide-cursor 0 0 go-to-coord init-sixel
    life-display-y @ 6 / 6 * begin dup life-display-y-end @ 6 / 6 * < while
      ." #1" init-sixel-rle
      life-display-x @ begin dup life-display-x-end @ < while
	swap 0 >r
	2dup alive? 1 and r> or >r
	2dup 1 + alive? 2 and r> or >r
	2dup 2 + alive? 4 and r> or >r
	2dup 3 + alive? 8 and r> or >r
	2dup 4 + alive? 16 and r> or >r
	2dup 5 + alive? 32 and r> or >r
	swap dup life-line-buffer @ + r@ swap c!
	r> [char] ? + add-sixel-rle
	1+
      repeat
      drop force-sixel-rle-out ." $#0" init-sixel-rle
      life-display-x @ begin dup life-display-x-end @ < while
	dup life-line-buffer @ + c@ 63 swap bic
	[char] ? + add-sixel-rle
	1+
      repeat
      drop
      force-sixel-rle-out ." -"
      6 +
    repeat
    drop
    display-life-sixel-part1
  ;

  \ The second part of displaying the life viewport using 2x2 sixels
  : display-life-sixel2-part1 ( -- )
    ." #1" init-sixel-rle
    life-display-x @ begin dup life-display-x-end @ < while
      0 >r
      life-display-y-end @ 3 / 3 * life-height @ < if
	dup life-display-y-end @ 3 / 3 * alive? 3 and r> or >r
      then
      life-display-y-end @ 3 / 3 * 1 + life-height @ < if
	dup life-display-y-end @ 3 / 3 * 1 + alive? 12 and r> or >r
      then
      life-display-y-end @ 3 / 3 * 2 + life-height @ < if
	dup life-display-y-end @ 3 / 3 * 2 + alive? 48 and r> or >r
      then
      dup life-line-buffer @ + r@ swap c!
      r> [char] ? + add-sixel-rle2
      1+
    repeat
    drop
    force-sixel-rle-out ." $#0" init-sixel-rle
    life-display-x @ begin dup life-display-x-end @ < while
      dup life-line-buffer @ + c@ 63 swap bic
      [char] ? + add-sixel-rle2
      1+
    repeat
    drop
    force-sixel-rle-out ." -"
    escape emit $5C emit show-cursor
  ;

  \ Display the life viewport using 2x2 sixels
  : display-life-sixel2 ( -- )
    hide-cursor 0 0 go-to-coord init-sixel
    life-display-y @ 3 / 3 * begin dup life-display-y-end @ 3 / 3 * < while
      ." #1" init-sixel-rle
      life-display-x @ begin dup life-display-x-end @ < while
	swap 0 >r
	2dup alive? 3 and r> or >r
	2dup 1 + alive? 12 and r> or >r
	2dup 2 + alive? 48 and r> or >r
	swap dup life-line-buffer @ + r@ swap c!
	r> [char] ? + add-sixel-rle2
	1+
      repeat
      drop force-sixel-rle-out ." $#0" init-sixel-rle
      life-display-x @ begin dup life-display-x-end @ < while
	dup life-line-buffer @ + c@ 63 swap bic
	[char] ? + add-sixel-rle2
	1+
      repeat
      drop
      force-sixel-rle-out ." -"
      3 +
    repeat
    drop
    display-life-sixel2-part1
  ;

  \ Run life without displaying it until a key is pressed
  : run-life ( -- )
    cr reset-ansi-term get-cursor-position drop 0
    begin
      cycle-life over 0 go-to-coord 1+ ." Cycle: " dup . erase-end-of-line key?
    until
    key 2drop
  ;
  
  \ Run life on the terminal until a key is pressed
  : run-life-term ( -- )
    reset-ansi-term hide-cursor 0 0 go-to-coord erase-end-of-line erase-down
    begin
      cycle-life display-life key?
    until
    key show-cursor drop
  ;

  \ Run life as sixels until a key is pressed
  : run-life-sixel ( -- )
    reset-ansi-term hide-cursor 0 0 go-to-coord erase-end-of-line erase-down
    begin
      cycle-life display-life-sixel key?
    until
    key show-cursor drop
  ;

  \ Run life as 2x2 sixels until a key is pressed
  : run-life-sixel2 ( -- )
    reset-ansi-term hide-cursor 0 0 go-to-coord erase-end-of-line erase-down
    begin
      cycle-life display-life-sixel2 key?
    until
    key show-cursor drop
  ;

  \ Step life one cycle
  : step-life ( -- )
    reset-ansi-term 0 0 go-to-coord erase-end-of-line erase-down
    cycle-life display-life
  ;

  \ Step life one cycle as sixels
  : step-life-sixel ( -- )
    reset-ansi-term 0 0 go-to-coord erase-end-of-line erase-down
    cycle-life display-life-sixel
  ;

  \ Step life one cycle as 2x2 sixels
  : step-life-sixel2 ( -- )
    reset-ansi-term 0 0 go-to-coord erase-end-of-line erase-down
    cycle-life display-life-sixel2
  ;

  \ Set the life viewport
  : life-viewport! ( x y width height -- )
    dup 3 pick + life-height @ <= if
      life-display-height !
    else
      drop life-height @ 2 pick - life-display-height !
    then
    dup 3 pick + life-width @ <= if
      life-display-width !
    else
      drop life-width @ 2 pick - life-display-width !
    then
    dup 0>= if
      dup life-height @ <= if
	life-display-y !
      else
	drop life-height @ life-display-y !
      then
    else
      drop 0 life-display-y !
    then
    dup 0>= if
      dup life-width @ <= if
	life-display-x !
      else
	drop life-width @ life-display-x !
      then
    else
      drop 0 life-display-x !
    then
    life-display-x @ life-display-width @ + life-display-x-end !
    life-display-y @ life-display-height @ + life-display-y-end !
  ;

  \ Clear life
  : clear-life ( -- )
    current-buffer @ life-width @ life-height @ * 0 fill
  ;
  
  \ Initialize life
  : init-life ( width height -- )
    ram-here life-line-buffer !
    over ram-allot
    0 life-display-x ! 0 life-display-y !
    2dup life-height ! life-width !
    2dup life-display-height ! life-display-width !
    ram-here dup life-buffer-0 ! current-buffer !
    * dup ram-allot
    ram-here dup life-buffer-1 ! new-buffer !
    ram-allot
    clear-life
    life-display-x @ life-display-width @ + life-display-x-end !
    life-display-y @ life-display-height @ + life-display-y-end !
  ;

  \ Get the next non-space character from a string, or null for end of string
  : get-char ( addr1 bytes1 -- addr2 bytes2 c )
    begin
      dup 0 > if
	over c@ dup $20 <> if
	  rot 1 + rot 1 - rot true
	else
	  drop 1 - swap 1 + swap false
	then
      else
	0 true
      then
    until
  ;

  \ Set multiple cells with a string with the format "_" for an dead cell,
  \ "*" for a live cell, and a "/" for a newline
  : set-multiple ( addr bytes x y -- )
    over >r 2swap begin get-char dup 0<> while
      case
	[char] _ of 2swap 2dup false -rot alive! swap 1 + swap 2swap endof
	[char] * of 2swap 2dup true -rot alive! swap 1 + swap 2swap endof
	[char] / of 2swap 1 + nip r@ swap 2swap endof
      endcase
    repeat
    drop 2drop 2drop rdrop
  ;
  
  \ Add a block to the world
  : block ( x y -- ) s" ** / **" 2swap set-multiple ;

  \ Add a tub to the world
  : tub ( x y -- ) s" _*_ / *_* / _*_" 2swap set-multiple ;

  \ Add a boat to the world
  : boat ( x y -- )
    s" _*_ / *_* / _**" 2swap set-multiple
  ;

  \ Add a blinker to the world (2 phases)
  : blinker ( phase x y -- )
    rot case
      0 of s" _*_ / _*_ / _*_" endof
      1 of s" ___ / *** / ___" endof
    endcase
    2swap set-multiple
  ;
  
  \ Add a glider to the world (4 phases)
  : glider ( phase x y -- )
    rot case
      0 of s" _*_ / __* / ***" endof
      1 of s" *_* / _** / _*_" endof
      2 of s" __* / *_* / _**" endof
      3 of s" *__ / _** / **_" endof
    endcase
    2swap set-multiple
  ;
  
  \ Add an R-pentomino to the world
  : r-pentomino ( x y -- )
    s" _** / **_ / _*_" 2swap set-multiple
  ;

end-module