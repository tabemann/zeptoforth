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
  
  \ Get whether a life cell is alive
  : alive? ( x y -- alive? )
    life-width @ 3 rshift * over 3 rshift + current-buffer @ + c@
    swap 7 and rshift 1 and 0<>
  ;

  \ Set a life cell to be dead
  : set-dead ( x y -- )
    life-width @ 3 rshift * over 3 rshift + current-buffer @ + dup >r c@
    swap 7 and 1 swap lshift bic r> c!
  ;

  \ Set a life cell to be alive
  : set-alive ( x y -- )
    life-width @ 3 rshift * over 3 rshift + current-buffer @ + dup >r c@
    swap 7 and 1 swap lshift or r> c!
  ;

  \ Set whether a life cell is alive
  : alive! ( alive? x y -- ) rot if set-alive else set-dead then ;

  \ Get whether a new life cell is alive
  : new-alive? ( x y -- alive? )
    life-width @ 3 rshift * over 3 rshift + new-buffer @ + c@
    swap 7 and rshift 1 and 0<>
  ;

  \ Set a new life cell to be dead
  : set-new-dead ( x y -- )
    life-width @ 3 rshift * over 3 rshift + new-buffer @ + dup >r c@
    swap 7 and 1 swap lshift bic r> c!
  ;

  \ Set a new life cell to be alive
  : set-new-alive ( x y -- )
    life-width @ 3 rshift * over 3 rshift + new-buffer @ + dup >r c@
    swap 7 and 1 swap lshift or r> c!
  ;

  \ Set whether a new life cell is alive
  : new-alive! ( alive? x y -- ) rot if set-new-alive else set-new-dead then ;

  \ Wrap a life cell coordinate
  : wrap-coord ( x y -- x y )
    dup life-height @ >= if drop 0 then
    dup 0< if drop life-height @ 1- then swap
    dup life-width @ >= if drop 0 then
    dup 0< if drop life-width @ 1- then swap
  ;

  \ Get the count of border cell neighbors that are alive
  : get-border-cell-alive-count ( x y -- u )
    0 >r
    2dup 1+ wrap-coord alive? if r> 1+ >r then
    2dup 1- wrap-coord alive? if r> 1+ >r then
    2dup swap 1+ swap wrap-coord alive? if r> 1+ >r then
    2dup swap 1- swap wrap-coord alive? if r> 1+ >r then
    2dup 1+ swap 1+ swap wrap-coord alive? if r> 1+ >r then
    2dup 1- swap 1+ swap wrap-coord alive? if r> 1+ >r then
    2dup 1+ swap 1- swap wrap-coord alive? if r> 1+ >r then
    1- swap 1- swap wrap-coord alive? if r> 1+ >r then
    r>
  ;

  \ Get the count of non-border cell neighbors that are alive
  : get-cell-alive-count ( x y -- u )
    0 >r
    2dup 1+ alive? if r> 1+ >r then
    2dup 1- alive? if r> 1+ >r then
    2dup swap 1+ swap alive? if r> 1+ >r then
    2dup swap 1- swap alive? if r> 1+ >r then
    2dup 1+ swap 1+ swap alive? if r> 1+ >r then
    2dup 1- swap 1+ swap alive? if r> 1+ >r then
    2dup 1+ swap 1- swap alive? if r> 1+ >r then
    1- swap 1- swap alive? if r> 1+ >r then
    r>
  ;

  \ Cycle an alive border life cell
  : cycle-alive-border-cell ( x y -- )
    2dup get-border-cell-alive-count dup 2 = swap 3 = or -rot new-alive!
  ;

  \ Cycle a dead border life cell
  : cycle-dead-border-cell ( x y -- )
    2dup get-border-cell-alive-count 3 = -rot new-alive!
  ;

  \ Cycle an alive non-border life cell
  : cycle-alive-cell ( x y -- )
    2dup get-cell-alive-count dup 2 = swap 3 = or -rot new-alive!
  ;

  \ Cycle a dead non-border life cell
  : cycle-dead-cell ( x y -- )
    2dup get-cell-alive-count 3 = -rot new-alive!
  ;
  
  \ Cycle a border life cell
  : cycle-border-cell ( x y -- )
    2dup alive? if cycle-alive-border-cell else cycle-dead-border-cell then
  ;

  \ Cycle a non-border life cell
  : cycle-cell ( x y -- )
    2dup alive? if cycle-alive-cell else cycle-dead-cell then
  ;

  \ Switch new buffer with old buffer
  : switch-buffers ( -- )
    current-buffer @ new-buffer @ current-buffer ! new-buffer !
  ;
  
  \ Execute a life cycle
  : cycle-life ( -- )
    0 begin dup life-height @ < while
      0 over cycle-border-cell
      life-width @ 1- over cycle-border-cell
      1+
    repeat
    drop
    0 begin dup life-width @ < while
      dup 0 cycle-border-cell
      dup life-height @ 1- cycle-border-cell
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

  \ Run life without displaying it until a key is pressed
  : run-life ( -- )
    cr reset-ansi-term hide-cursor get-cursor-position drop 0
    begin
      cycle-life over 0 go-to-coord 1+ ." Cycle: " dup . erase-end-of-line key?
    until
    key show-cursor 2drop
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
    current-buffer @ life-width @ life-height @ * 3 rshift 0 fill
  ;
  
  \ Initialize life
  : init-life ( width height -- )
    ram-here life-line-buffer !
    over ram-allot
    0 life-display-x ! 0 life-display-y !
    2dup life-height ! life-width !
    2dup life-display-height ! life-display-width !
    ram-here dup life-buffer-0 ! current-buffer !
    * 8 / dup ram-allot
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
    over >r 2swap begin get-char dup 0 <> while
      case
	[char] _ of 2swap 2dup set-dead swap 1 + swap 2swap endof
	[char] * of 2swap 2dup set-alive swap 1 + swap 2swap endof
	[char] / of 2swap 1 + nip r@ swap 2swap endof
      endcase
    repeat
    drop 2drop 2drop r> drop
  ;

\ Flip part of a coordinate
  : flip-coord-part ( n1 n-center -- n2 ) tuck - - ;
  
  \ Get the center of part of a coordinate
  : coord-part-center ( n1 n-span -- ) 2 / + ;
  
  \ Copy cells from the next world back to the current world
  : copy-next-to-current ( x y width height )
    3 pick begin dup 5 pick 4 pick + < while
      3 pick begin dup 5 pick 4 pick + < while
	2dup new-alive? 2 pick 2 pick alive! 1 +
      repeat
      drop 1 +
    repeat
    drop 2drop 2drop
  ;
  
  \ Actually flip a region horizontally
  : do-flip-horizontal ( x y width height -- )
    3 pick 2 pick coord-part-center
    4 pick begin dup 6 pick 5 pick + < while
      4 pick begin dup 6 pick 5 pick + < while
	2dup alive? 2 pick 4 pick flip-coord-part 2 pick new-alive! 1 +
      repeat
      drop 1 +
    repeat
    2drop 2drop 2drop
  ;
  
  \ Actually flip a region vertically
  : do-flip-vertical ( x y width height -- )
    2 pick over coord-part-center
    4 pick begin dup 6 pick 5 pick + < while
      4 pick begin dup 6 pick 5 pick + < while
	2dup alive? 2 pick 2 pick 5 pick flip-coord-part new-alive! 1 +
      repeat
      drop 1 +
    repeat
    2drop 2drop 2drop
  ;
  
  \ Flip a region horizontally
  : flip-horizontal ( x y width height -- )
    3 pick 3 pick 3 pick 3 pick do-flip-horizontal copy-next-to-current
  ;
  
  \ Flip a region vertically
  : flip-vertical ( x y width height -- )
    3 pick 3 pick 3 pick 3 pick do-flip-vertical copy-next-to-current
  ;

  \ Motion directions
  0 constant ne
  1 constant se
  2 constant sw
  3 constant nw

  \ Flip a region in two dimensions
  : flip-2d ( x y width height dir -- )
    case
      se of 2drop 2drop endof
      sw of flip-horizontal endof
      nw of 2over 2over flip-horizontal flip-vertical endof
      ne of flip-vertical endof
    endcase
  ;
  
  \ Add a block to the world
  : block ( x y -- ) s" ** / **" 2swap set-multiple ;

  \ Add a tub to the world
  : tub ( x y -- ) s" _*_ / *_* / _*_" 2swap set-multiple ;

  \ Add a boat to the world
  : boat ( orientation x y -- )
    s" _*_ / *_* / _**" 2over set-multiple rot 3 3 rot flip-2d
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
  : glider ( motion phase x y -- )
    rot case
      0 of s" _*_ / __* / ***" endof
      1 of s" *_* / _** / _*_" endof
      2 of s" __* / *_* / _**" endof
      3 of s" *__ / _** / **_" endof
    endcase
    2over set-multiple rot 3 3 rot flip-2d
  ;
  
  \ Add an R-pentomino to the world
  : r-pentomino ( dir x y -- )
    s" _** / **_ / _*_" 2over set-multiple rot 3 3 rot flip-2d
  ;

end-module