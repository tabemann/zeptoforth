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
  lock import
  task import
  chan import
  
  \ Life width
  128 constant life-width

  \ Life height
  64 constant life-height

  \ Life start column
  variable life-start-col

  \ Life end column
  variable life-end-col

  \ Life start row
  variable life-start-row

  \ Life end row
  variable life-end-row

  \ Redraw all of life
  variable redraw-life?

  \ Magnify life
  variable magnify-life?

  \ Lock initialized?
  false value lock-inited?
  
  \ Display initialized?
  false value display-inited?

  \ Task initialized?
  false value task-inited?

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

  \ Life lock
  lock-size buffer: life-lock
  
  \ Life command channel
  cell 1 chan-size buffer: life-cmd-chan

  \ Life response channel
  cell 1 chan-size buffer: life-resp-chan
  
  \ Life task
  variable life-task

  \ Stop life command
  0 constant cmd-stop-life

  \ Step life command
  1 constant cmd-step-life

  \ Run life command
  2 constant cmd-run-life

  \ Pause life command
  3 constant cmd-pause-life

  \ Unpause life command
  4 constant cmd-unpause-life

  \ Pause life counter
  variable pause-life-counter

  \ Draw an alive cell on the display
  : draw-alive-cell { col row -- }
    magnify-life? @ if
      $FF col life-start-col @ - 2 * 2 row life-start-row @ - 2 * 2
      my-ssd1306 set-rect-const
    else
      $FF col row my-ssd1306 set-pixel-const
    then
  ;

  \ Draw a dead cell on the display
  : draw-dead-cell { col row -- }
    magnify-life? @ if
      $00 col life-start-col @ - 2 * 2 row life-start-row @ - 2 * 2
      my-ssd1306 set-rect-const
    else
      $00 col row my-ssd1306 set-pixel-const
    then
  ;

  \ Display a cell on the display
  : draw-cell { alive col row -- }
    alive if col row draw-alive-cell else col row draw-dead-cell then
  ;
  
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
    2dup draw-dead-cell
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
    2dup draw-alive-cell
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
    2dup draw-dead-cell
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
    2dup draw-alive-cell
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
    2dup draw-dead-cell
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
    2dup draw-alive-cell
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
    [:
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
    ;] life-lock with-lock
  ;

  \ Redraw life
  : redraw-life ( -- )
    redraw-life? @ if
      life-end-row @ life-start-row @ ?do
        life-end-col @ life-start-col @ ?do
          i j alive? i j draw-cell
        loop
      loop
      false redraw-life? !
    then
  ;
  
  \ Display life
  : display-life ( -- ) redraw-life my-ssd1306 update-display ;
  
  \ Send a command
  : send-cmd ( cmd -- )
    [: life-cmd-chan send-chan ;] provide-allot-cell
    [: life-resp-chan recv-chan ;] extract-allot-cell drop
  ;

  \ Receive a command
  : recv-cmd ( -- cmd ) [: life-cmd-chan recv-chan ;] extract-allot-cell ;

  \ Acknowledge a command
  : ack-cmd ( -- )
    0 [: life-resp-chan send-chan ;] provide-allot-cell
  ;

  \ Actually step life
  : do-step-life ( -- ) cycle-life display-life ;

  \ Actually pause life
  : do-pause-life ( -- )
    begin
      recv-cmd case
        cmd-pause-life of 1 pause-life-counter +! endof
        cmd-unpause-life of -1 pause-life-counter +! endof
      endcase
      ack-cmd
      pause-life-counter @ 0<=
      display-life
    until
  ;
  
  \ Actually run life
  : do-run-life ( -- )
    begin
      life-cmd-chan chan-empty? if
        cycle-life display-life false
      else
        recv-cmd case
          cmd-stop-life of ack-cmd true endof
          cmd-pause-life of
            ack-cmd
            1 pause-life-counter +!
            pause-life-counter @ 0> if do-pause-life then
            false
          endof
          cmd-unpause-life of
            ack-cmd
            -1 pause-life-counter +!
            false
          endof
          ack-cmd false swap
        endcase
      then
    until
  ;
  
  \ Run the life task
  : run-life-task ( -- )
    begin
      recv-cmd case
        cmd-stop-life of ack-cmd endof
        cmd-step-life of do-step-life ack-cmd endof
        cmd-run-life of ack-cmd do-run-life endof
        cmd-pause-life of
          ack-cmd
          1 pause-life-counter +!
          pause-life-counter @ 0> if do-pause-life then
        endof
        cmd-unpause-life of
          ack-cmd
          -1 pause-life-counter +!
        endof
        ack-cmd
      endcase
    again
  ;
  
  \ Run life
  : run-life ( -- ) cmd-run-life send-cmd ;

  \ Step life one cycle
  : step-life ( -- ) cmd-step-life send-cmd ;

  \ Stop life
  : stop-life ( -- ) cmd-stop-life send-cmd ;

  \ Pause life
  : pause-life ( -- ) cmd-pause-life send-cmd ;
  
  \ Unpause life
  : unpause-life ( -- ) cmd-unpause-life send-cmd ;

  \ Execute code with life paused
  : with-life-paused ( xt -- )
    pause-life [: life-lock with-lock ;] try unpause-life ?raise
  ;
  
  \ Clear life
  : clear-life ( -- )
    [:
      current-buffer @ life-width life-height * 0 fill
      true redraw-life? !
    ;] with-life-paused
  ;

  \ Zoom out
  : zoom-out ( -- )
    [:
      0 life-start-col !
      life-width life-end-col !
      0 life-start-row !
      life-height life-end-row !
      true redraw-life? !
      false magnify-life? !
    ;] with-life-paused
  ;

  \ Zoom in
  : zoom-in ( col row -- )
    [:
      { col row }
      col 0 max life-width 2 / min to col
      row 0 max life-height 2 / min to row
      col life-start-col !
      col life-width 2 / + life-end-col !
      row life-start-row !
      row life-height 2 / + life-end-row !
      true redraw-life? !
      true magnify-life? !
    ;] with-life-paused
  ;

  \ Initialize life
  : init-life ( -- )
    lock-inited? not if
      life-lock init-lock
      true to lock-inited?
    then
    [:
      life-buffer-0 current-buffer !
      life-buffer-1 new-buffer !
      0 life-start-col !
      life-width life-end-col !
      0 life-start-row !
      life-height life-end-row !
      false redraw-life? !
      false magnify-life? !
    ;] life-lock with-lock
    display-inited? not if
      14 15 my-framebuffer life-width life-height SSD1306_I2C_ADDR 1
      <ssd1306> my-ssd1306 init-object
      $01 my-ssd1306 display-contrast!
      true to display-inited?
    then
    task-inited? not if
      0 pause-life-counter !
      cell 1 life-cmd-chan init-chan
      cell 1 life-resp-chan init-chan
      0 ['] run-life-task 320 128 512 spawn life-task !
      life-task @ run
      true to task-inited?
    then
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
    [:
      over >r 2swap begin get-char dup 0<> while
        case
          [char] _ of 2swap 2dup false -rot alive! swap 1 + swap 2swap endof
          [char] * of 2swap 2dup true -rot alive! swap 1 + swap 2swap endof
          [char] / of 2swap 1 + nip r@ swap 2swap endof
        endcase
      repeat
      drop 2drop 2drop rdrop
    ;] with-life-paused
  ;

  \ Flip an area vertically
  : flip-vert ( x cols y rows -- )
    [: { x cols y rows }
      rows 2 u/ { rows2/ }
      rows 2 umod 0= if
        rows2/ 0 ?do
          cols 0 ?do
            x i + y rows2/ + 1- j - alive? { alive0? }
            x i + y rows2/ + j + alive? { alive1? }
            alive0? x i + y rows2/ + j + alive!
            alive1? x i + y rows2/ + 1- j - alive!
          loop
        loop
      else
        rows2/ 0 ?do
          cols 0 ?do
            x i + y rows2/ + 1- j - alive? { alive0? }
            x i + y rows2/ + 1+ j + alive? { alive1? }
            alive0? x i + y rows2/ + 1+ j + alive!
            alive1? x i + y rows2/ + 1- j - alive!
          loop
        loop
      then
    ;] with-life-paused
  ;

  \ Mirror an area horizontally
  : flip-horiz ( x cols y rows -- )
    [: { x cols y rows }
      cols 2 u/ { cols2/ }
      cols 2 umod 0= if
        rows 0 ?do
          cols2/ 0 ?do
            x cols2/ + 1- i - y j + alive? { alive0? }
            x cols2/ + i + y j + alive? { alive1? }
            alive0? y cols2/ + i + y j + alive!
            alive1? y cols2/ + 1- i - y j + alive!
          loop
        loop
      else
        rows 0 ?do
          cols2/ 0 ?do
            x cols2/ + 1- i - y j + alive? { alive0? }
            x cols2/ + 1+ i + y j + alive? { alive1? }
            alive0? y cols2/ + 1+ i + y j + alive!
            alive1? y cols2/ + 1- i - y j + alive!
          loop
        loop
      then
    ;] with-life-paused
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

  \ Wrap alive! in a pause
  : alive! ( alive? x y -- )
    [: 3dup alive! ;] with-life-paused drop 2drop
  ;

  \ Wrap alive? in a lock
  : alive? ( x y -- alive? )
    [: 2dup alive? ;] with-life-paused >r 2drop r>
  ;

end-module