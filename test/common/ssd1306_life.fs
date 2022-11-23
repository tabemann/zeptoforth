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

  oo import
  bitmap import
  ssd1306 import
  
  \ Life width
  128 constant life-width

  \ Life height
  64 constant life-height

  \ Value display initialized
  false value display-inited

  \ An I2C SSD1306 device
  <ssd1306> class-size buffer: my-ssd1306

  \ My framebuffer
  life-width life-height bitmap-buf-size buffer: my-framebuffer

  \ Life buffer 0
  life-width life-height * buffer: life-buffer-0

  \ Life buffer 1
  life-width life-height * buffer: life-buffer-1

  \ Life line buffer
  variable life-line-buffer

  \ Current buffer
  variable current-buffer

  \ New buffer
  variable new-buffer
  
  \ Wrap a life cell coordinate
  : wrap-coord ( x y -- x y )
    dup life-height >= if drop 0 then
    dup 0< if drop life-height 1- then swap
    dup life-width >= if drop 0 then
    dup 0< if drop life-width 1- then swap
  ;

  \ Get whether a life cell is alive
  : alive? ( x y -- alive? ) life-width * + current-buffer @ + c@ 16 >= ;

  \ Add alive neighbor
  : +alive ( x y -- ) life-width * + current-buffer @ + 1 swap c+! ;

  \ Remove alive neighbor
  : -alive ( x y -- ) life-width * + current-buffer @ + -1 swap c+! ;

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
    2dup $00 -rot my-ssd1306 set-pixel-const
    life-width * + current-buffer @ + 16 swap cbic!
    
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
    2dup $FF -rot my-ssd1306 set-pixel-const
    life-width * + current-buffer @ + 16 swap cbis!
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
  : new-alive? ( x y -- alive? ) life-width * + new-buffer @ + c@ 16 >= ;

  \ Add new alive neighbor
  : +new-alive ( x y -- ) life-width * + new-buffer @ + 1 swap c+! ;

  \ Remove new alive neighbor
  : -new-alive ( x y -- ) life-width * + new-buffer @ + -1 swap c+! ;

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
    2dup $00 -rot my-ssd1306 set-pixel-const
    life-width * + new-buffer @ + 16 swap cbic!
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
    2dup $FF -rot my-ssd1306 set-pixel-const
    life-width * + new-buffer @ + 16 swap cbis!
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
    2dup $00 -rot my-ssd1306 set-pixel-const
    life-width * + new-buffer @ + 16 swap cbic!
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
    2dup $FF -rot my-ssd1306 set-pixel-const
    life-width * + new-buffer @ + 16 swap cbis!
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
    2dup life-width * + current-buffer @ + c@ 2 lshift
    nonborder-state-table + @ execute
  ;

  \ Switch new buffer with old buffer
  : switch-buffers ( -- )
    current-buffer @ new-buffer @ current-buffer ! new-buffer !
  ;
  
  \ Execute a life cycle
  : cycle-life ( -- )
    current-buffer @ new-buffer @ life-width life-height * move
    0 begin dup life-height < while
      0 over false -rot new-alive!
      life-width 1- over false -rot new-alive!
      1+
    repeat
    drop
    0 begin dup life-width < while
      dup 0 false -rot new-alive!
      dup life-height 1- false -rot new-alive!
      1+
    repeat
    drop
    1 begin dup life-width 1- < while
      1 begin dup life-height 1- < while
	2dup cycle-cell 1+
      repeat
      drop 1+
    repeat
    drop
    switch-buffers
  ;

  \ Run life until a key is pressed
  : run-life ( -- )
    begin
      cycle-life my-ssd1306 update-display key?
    until
    key drop
  ;

  \ Step life one cycle
  : step-life ( -- )
    cycle-life my-ssd1306 update-display
  ;

  \ Clear life
  : clear-life ( -- )
    current-buffer @ life-width life-height * 0 fill
    $00 0 life-width 0 life-height my-ssd1306 set-rect-const
    my-ssd1306 update-display
  ;
  
  \ Initialize life
  : init-life ( -- )
    display-inited not if
      14 15 my-framebuffer life-width life-height SSD1306_I2C_ADDR 1
      <ssd1306> my-ssd1306 init-object
      true to display-inited
    then
    life-buffer-0 current-buffer !
    life-buffer-1 new-buffer !
    clear-life
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