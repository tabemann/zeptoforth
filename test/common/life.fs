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

begin-module-once life-module

  import ansi-term-module
  
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
  
  \ Current life buffer
  variable life-current

  \ Life buffer 0
  variable life-buffer-0

  \ Life buffer 1
  variable life-buffer-1

  \ Get the current life buffer
  : current-buffer ( -- addr )
    life-current @ 0= if life-buffer-0 @ else life-buffer-1 @ then
  ;

  \ Get the new life buffer
  : new-buffer ( -- addr )
    life-current @ 0<> if life-buffer-0 @ else life-buffer-1 @ then
  ;
  
  \ Get whether a life cell is alive
  : alive? ( x y -- alive? )
    life-width @ 3 rshift * over 3 rshift + current-buffer + b@
    swap 7 and rshift 1 and 0<>
  ;

  \ Set a life cell to be dead
  : set-dead ( x y -- )
    life-width @ 3 rshift * over 3 rshift + current-buffer + dup >r b@
    swap 7 and 1 swap lshift bic r> b!
  ;

  \ Set a life cell to be alive
  : set-alive ( x y -- )
    life-width @ 3 rshift * over 3 rshift + current-buffer + dup >r b@
    swap 7 and 1 swap lshift or r> b!
  ;

  \ Set whether a life cell is alive
  : alive! ( alive? x y -- ) rot if set-alive else set-dead then ;

  \ Set a new life cell to be dead
  : set-new-dead ( x y -- )
    life-width @ 3 rshift * over 3 rshift + new-buffer + dup >r b@
    swap 7 and 1 swap lshift bic r> b!
  ;

  \ Set a new life cell to be alive
  : set-new-alive ( x y -- )
    life-width @ 3 rshift * over 3 rshift + new-buffer + dup >r b@
    swap 7 and 1 swap lshift or r> b!
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
    life-current @ 0= if 1 life-current ! else 0 life-current ! then
  ;
  
  \ Execute a life cycle
  : cycle-life ( -- )
    life-height @ 0 ?do
      0 i cycle-border-cell
      life-width @ 1- i cycle-border-cell
    loop
    life-width @ 1- 1 ?do
      i 0 cycle-border-cell
      i life-height @ 1- cycle-border-cell
    loop
    life-width @ 1- 1 ?do
      life-height @ 1- 1 ?do
	j i cycle-cell
      loop
    loop
    switch-buffers
  ;

  \ Display the life viewport
  : display-life ( -- )
    hide-cursor 0 0 go-to-coord
    life-display-height @ life-display-y @ + life-display-y @ ?do
      life-display-width @ life-display-x @ + life-display-x @ ?do
	i j alive? if ." x" else space then
      loop cr
    loop
    show-cursor
  ;
  
  \ Run life until a key is pressed
  : run-life ( -- )
    reset-ansi-term hide-cursor 0 0 go-to-coord erase-end-of-line erase-down
    begin
      cycle-life display-life key?
    until
    key show-cursor drop
  ;

  \ Step life one cycle
  : step-life ( -- )
    reset-ansi-term 0 0 go-to-coord erase-end-of-line erase-down
    cycle-life display-life
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
  ;

  \ Clear life
  : clear-life ( -- )
    current-buffer life-width @ life-height @ * 3 rshift 0 fill
  ;
  
  \ Initialize life
  : init-life ( width height -- )
    0 life-display-x ! 0 life-display-y !
    2dup life-height ! life-width !
    2dup life-display-height ! life-display-width !
    0 life-current !
    ram-here life-buffer-0 !
    * 8 / dup ram-allot
    ram-here life-buffer-1 !
    ram-allot
    clear-life
  ;

  \ Glider
  : glider ( x y -- )
    2dup true -rot swap 1+ swap wrap-coord alive!
    2dup true -rot 1+ swap 2 + swap wrap-coord alive!
    2dup true -rot 2 + wrap-coord alive!
    2dup true -rot 2 + swap 1+ swap wrap-coord alive!
    true -rot 2 + swap 2 + swap wrap-coord alive!
  ;

end-module