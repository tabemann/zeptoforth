\ Copyright (c) 2020-2021 Travis Bemann
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

\ Compile to flash
compile-to-flash

begin-module-once chan-module
  
  import task-module
  import tqueue-module
  
  begin-import-module chan-internal-module

    \ Channel header structure
    begin-structure chan-header-size
      
      \ Channel element count
      field: chan-count

      \ Channel data size
      field: chan-data-size

      \ Channel receive index
      field: chan-recv-index

      \ Channel send index
      field: chan-send-index

      \ Channel is closed
      field: chan-closed
      
      \ Channel send ready
      field: chan-send-ready

      \ Channel receive ready
      field: chan-recv-ready

      \ Channel send task queue
      tqueue-size +field chan-send-tqueue
      
      \ Channel receive task queue
      tqueue-size +field chan-recv-tqueue

    end-structure

    \ Core of getting whether a channel is full
    : chan-full-unsafe? ( chan -- flag )
      dup chan-send-index @ 1+ over chan-count @ umod
      swap chan-recv-index @ =
    ;

    \ Core of getting whether a channel is empty
    : chan-empty-unsafe? ( chan -- flag )
      dup chan-send-index @ swap chan-recv-index @ =
    ;

  end-module
  
  \ Channel is closed exception
  : x-chan-closed ( -- ) space ." channel is closed" cr ;

  \ Get the channel element data size
  : chan-data-size ( chan -- element-bytes ) chan-data-size @ ;

  \ Get the channel element count
  : chan-count ( chan -- element-count ) chan-count ;
  
  \ Get whether a channel is full
  : chan-full? ( chan -- flag )
    begin-critical chan-full-unsafe? end-critical
  ;

  \ Get whether a channel is empty
  : chan-empty? ( chan -- flag )
    begin-critical chan-empty-unsafe? end-critical
  ;

  begin-module chan-internal-module
    
    \ Wait to send on a channel
    : wait-send-chan ( chan -- )
      dup chan-full-unsafe? if
	1 over chan-send-ready +!
	dup chan-send-tqueue ['] wait-tqueue try
	-1 2 pick chan-send-ready +!
	?raise
      then
      chan-closed @ if
	end-critical ['] x-chan-closed ?raise
      then
    ;

    \ Wait to receive on a channel
    : wait-recv-chan ( chan -- )
      dup chan-empty-unsafe? if
	dup chan-closed @ if
	  end-critical ['] x-chan-closed ?raise
	then
	1 over chan-recv-ready +!
	dup chan-recv-tqueue ['] wait-tqueue try
	-1 rot chan-recv-ready +!
	?raise
      else
	drop
      then
    ;

    \ Get the channel send address
    : send-chan-addr ( chan -- b-addr )
      dup chan-send-index @ over chan-data-size @ * chan-header-size + +
    ;

    \ Get the channel receive address
    : recv-chan-addr ( chan -- b-addr )
      dup chan-recv-index @ over chan-data-size @ * chan-header-size + +
    ;

    \ Advance the channel send index
    : advance-send-chan ( chan -- )
      dup chan-send-index @ 1+ over chan-count @ umod swap chan-send-index !
    ;

    \ Advance the channel receive index
    : advance-recv-chan ( chan -- )
      dup chan-recv-index @ 1+ over chan-count @ umod swap chan-recv-index !
    ;

  end-module
  
  \ Get channel size for a channel with a specified element size in bytes
  \ and element count
  : chan-size ( element-bytes element-count -- total-bytes )
    * chan-header-size + 4 align
  ;

  \ Initialize a channel for a channel with a specified element size in bytes
  \ and element count at a specified address
  : init-chan ( element-bytes element-count addr -- )
    tuck chan-count !
    tuck chan-data-size !
    0 over chan-recv-index !
    0 over chan-send-index !
    0 over chan-recv-ready !
    0 over chan-send-ready !
    dup chan-recv-tqueue init-tqueue
    dup chan-send-tqueue init-tqueue
    false swap chan-closed !
  ;

  \ Send data to a channel
  : send-chan ( addr bytes chan -- )
    begin-critical
    dup chan-closed @ if
      end-critical ['] x-chan-closed ?raise
    then
    current-task prepare-block
    dup wait-send-chan
    dup send-chan-addr over chan-data-size @ 0 fill
    dup >r chan-data-size @ min r@ send-chan-addr swap move
    dup advance-send-chan
    dup chan-recv-ready @ 0> if
      chan-recv-tqueue wake-tqueue
    else
      drop
    then
    end-critical
  ;

  \ Receive data from a channel
  : recv-chan ( addr bytes chan -- addr recv-bytes )
    begin-critical
    current-task prepare-block
    dup wait-recv-chan
    >r 2dup 0 fill
    r@ chan-data-size @ min r@ recv-chan-addr -rot 2dup 2>r move 2r> r>
    dup advance-recv-chan
    dup chan-send-ready @ 0> if
      chan-send-tqueue wake-tqueue
    else
      drop
    then
    end-critical
  ;

  \ Send a double cell on a channel
  : send-chan-2cell ( xd chan -- )
    2 cells [: >r -rot r@ 2! r> 2 cells rot send-chan ;] with-allot
  ;

  \ Receive a double cell from a channel
  : recv-chan-2cell ( chan -- xd )
    2 cells [: 2 cells rot recv-chan 2 cells >= if 2@ else drop 0 0 then ;]
    with-allot
  ;

  \ Send a cell on a channel
  : send-chan-cell ( x chan -- )
    1 cells [: >r swap r@ ! r> 1 cells rot send-chan ;] with-allot
  ;

  \ Receive a cell from a channel
  : recv-chan-cell ( chan -- x )
    1 cells [: 1 cells rot recv-chan 1 cells >= if @ else drop 0 then ;]
    with-allot
  ;

  \ Send a halfword on a channel
  : send-chan-half ( h chan -- )
    2 [: >r swap r@ ! r> 2 rot send-chan ;] with-allot
  ;

  \ Receive a halfword from a channel
  : recv-chan-half ( chan -- h )
    2 [: 2 rot recv-chan 2 >= if @ else drop 0 then ;]
    with-allot
  ;

  \ Send a byte on a channel
  : send-chan-byte ( h chan -- )
    1 [: >r swap r@ ! r> 1 rot send-chan ;] with-allot
  ;

  \ Receive a byte from a channel
  : recv-chan-byte ( chan -- h )
    1 [: 1 rot recv-chan 1 >= if @ else drop 0 then ;]
    with-allot
  ;

  \ Close a channel
  : close-chan ( chan -- )
    begin-critical
    true over chan-closed !
    dup chan-send-task @ ?dup if run then
    chan-recv-task @ ?dup if run then
    end-critical
  ;

  \ Get whether a channel is closed
  : chan-closed? ( chan -- closed ) chan-closed @ ;

  \ Reopen a channel
  : reopen-chan ( chan -- ) false swap chan-closed ! ;
  
end-module

\ Warm reboot
warm
