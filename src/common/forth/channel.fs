\ Copyright (c) 2020-2023 Travis Bemann
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

compress-flash

begin-module chan
  
  task import
  slock import
  tqueue import
  
  begin-module chan-internal

    \ Channel header structure
    begin-structure chan-header-size

      \ Channel simple lock
      slock-size +field chan-slock
      
      \ Channel element count
      field: chan-count

      \ Channel data size
      field: chan-data-size

      \ Channel receive index
      field: chan-recv-index

      \ Channel send index
      field: chan-send-index

      \ Channel current count
      field: chan-current-count

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

    commit-flash
    
  end-module> import
  
  \ Channel is closed exception
  : x-chan-closed ( -- ) ." channel is closed" cr ;

  \ Get whether a channel is full
  : chan-full? ( chan -- flag )
    dup chan-current-count @ swap chan-count @ =
  ;
  
  \ Get whether a channel is empty
  : chan-empty? ( chan -- flag )
    chan-current-count @ 0=
  ;

  commit-flash
  
  continue-module chan-internal
    
    \ Wait to send on a channel
    : wait-send-chan { chan -- }
      begin chan chan-full? while
	1 chan chan-send-ready +!
	chan chan-send-tqueue ['] wait-tqueue try
	-1 chan chan-send-ready +!
	?raise
      repeat
      chan chan-closed @ triggers x-chan-closed
    ;

    \ Wait to receive on a channel
    : wait-recv-chan { chan -- }
      begin chan chan-empty? while
	chan chan-closed @ triggers x-chan-closed
	1 chan chan-recv-ready +!
	chan chan-recv-tqueue ['] wait-tqueue try
	-1 chan chan-recv-ready +!
	?raise
      repeat
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
      1 over chan-current-count +!
      dup chan-send-index @ 1+ over chan-count @ umod swap chan-send-index !
    ;

    \ Advance the channel receive index
    : advance-recv-chan ( chan -- )
      -1 over chan-current-count +!
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
    dup chan-slock init-slock
    tuck chan-count !
    tuck chan-data-size !
    0 over chan-current-count !
    0 over chan-recv-index !
    0 over chan-send-index !
    0 over chan-recv-ready !
    0 over chan-send-ready !
    dup chan-slock over chan-recv-tqueue 1 -rot 0 -rot init-tqueue-full
    dup chan-slock over chan-send-tqueue 1 -rot 0 -rot init-tqueue-full
    false swap chan-closed !
  ;

  commit-flash

  \ Send data to a channel
  : send-chan ( addr bytes chan -- )
    [:
      dup chan-closed @ triggers x-chan-closed
      current-task prepare-block
      dup wait-send-chan
      dup send-chan-addr over chan-data-size @ 0 fill
      dup >r chan-data-size @ min r@ send-chan-addr swap move r>
      dup advance-send-chan
      dup chan-recv-ready @ 0> if
	chan-recv-tqueue wake-tqueue
      else
	drop
      then
    ;] over chan-slock with-slock
  ;

  \ Receive data from a channel
  : recv-chan ( addr bytes chan -- recv-bytes )
    [:
      current-task prepare-block
      dup wait-recv-chan
      >r 2dup 0 fill
      r@ chan-data-size @ min r@ recv-chan-addr -rot dup >r move 2r>
      dup advance-recv-chan
      dup chan-send-ready @ 0> if
	chan-send-tqueue wake-tqueue
      else
	drop
      then
    ;] over chan-slock with-slock
  ;

  \ Peek data from a channel
  : peek-chan ( addr bytes chan -- peek-bytes )
    [:
      current-task prepare-block
      dup wait-recv-chan
      >r 2dup 0 fill
      r@ chan-data-size @ min r> recv-chan-addr -rot dup >r move r>
    ;] over chan-slock with-slock
  ;

  \ Skip data on a channel
  : skip-chan ( chan -- )
    [:
      current-task prepare-block
      dup wait-recv-chan
      dup advance-recv-chan
      dup chan-send-ready @ 0> if
	chan-send-tqueue wake-tqueue
      else
	drop
      then
    ;] over chan-slock with-slock
  ;

  \ Send data to a channel without blocking (raise x-would-block if blocking
  \ would normally occur)
  : send-chan-no-block ( addr bytes chan -- )
    [:
      dup chan-closed @ triggers x-chan-closed
      dup chan-full? triggers x-would-block
      dup send-chan-addr over chan-data-size @ 0 fill
      dup >r chan-data-size @ min r@ send-chan-addr swap move r>
      dup advance-send-chan
      dup chan-recv-ready @ 0> if
	chan-recv-tqueue wake-tqueue
      else
	drop
      then
    ;] over chan-slock with-slock
  ;

  \ Receive data from a channel without blocking (raise x-would-block if
  \ blocking would normally occur)
  : recv-chan-no-block ( addr bytes chan -- recv-bytes )
    [:
      dup chan-empty? triggers x-would-block
      >r 2dup 0 fill
      r@ chan-data-size @ min r@ recv-chan-addr -rot dup >r move 2r>
      dup advance-recv-chan
      dup chan-send-ready @ 0> if
	chan-send-tqueue wake-tqueue
      else
	drop
      then
    ;] over chan-slock with-slock
  ;
  
  \ Peek data from a channel without blocking (raise x-would-block if blocking
  \ would normally occur)
  : peek-chan-no-block ( addr bytes chan -- peek-bytes )
    [:
      dup chan-empty? triggers x-would-block
      >r 2dup 0 fill
      r@ chan-data-size @ min r> recv-chan-addr -rot dup >r move r>
    ;] over chan-slock with-slock
  ;
  
  \ Skip data on a channel without blocking (raise x-would-block if blocking
  \ would normally occur)
  : skip-chan-no-block ( chan -- )
    [:
      dup chan-empty? triggers x-would-block
      dup advance-recv-chan
      dup chan-send-ready @ 0> if
	chan-send-tqueue wake-tqueue
      else
	drop
      then
    ;] over chan-slock with-slock
  ;
  
  commit-flash

  \ Close a channel
  : close-chan ( chan -- )
    [:
      true over chan-closed !
      dup chan-send-tqueue wake-tqueue-all
      chan-recv-tqueue wake-tqueue-all
    ;] over chan-slock with-slock
  ;

  \ Get whether a channel is closed
  : chan-closed? ( chan -- closed ) chan-closed @ ;

  \ Reopen a channel
  : reopen-chan ( chan -- ) false swap chan-closed ! ;
  
  \ Get the channel element count
  : chan-count ( chan -- element-count ) chan-count @ ;

  \ Get the channel element data size
  : chan-data-size ( chan -- element-bytes ) chan-data-size @ ;

end-module

end-compress-flash

\ Reboot
reboot
