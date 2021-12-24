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

compress-flash

begin-module chan
  
  task import
  tqueue import
  
  begin-module chan-internal

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
    
    \ Core of getting whether a channel is full
    : chan-full-unsafe? ( chan -- flag )
      dup chan-current-count @ swap chan-count @ =
    ;

    \ Core of getting whether a channel is empty
    : chan-empty-unsafe? ( chan -- flag )
      chan-current-count @ 0=
    ;

  end-module> import
  
  \ Channel is closed exception
  : x-chan-closed ( -- ) space ." channel is closed" cr ;

  commit-flash
  
  \ Get whether a channel is full
  : chan-full? ( chan -- flag ) [: chan-full-unsafe? ;] critical ;

  \ Get whether a channel is empty
  : chan-empty? ( chan -- flag ) chan-empty-unsafe? ;

  continue-module chan-internal
    
    \ Wait to send on a channel
    : wait-send-chan ( chan -- )
      dup chan-full-unsafe? if
	1 over chan-send-ready +!
	dup chan-send-tqueue ['] wait-tqueue try
	-1 2 pick chan-send-ready +!
	?raise
      then
      chan-closed @ triggers x-chan-closed
    ;

    \ Wait to receive on a channel
    : wait-recv-chan ( chan -- )
      dup chan-empty-unsafe? if
	dup chan-closed @ triggers x-chan-closed
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
    tuck chan-count !
    tuck chan-data-size !
    0 over chan-current-count !
    0 over chan-recv-index !
    0 over chan-send-index !
    0 over chan-recv-ready !
    0 over chan-send-ready !
    dup chan-recv-tqueue init-tqueue
    dup chan-send-tqueue init-tqueue
    false swap chan-closed !
  ;

  commit-flash

  \ Send data to a channel
  : send-chan ( addr bytes chan -- )
    [:
      s" BEGIN SEND-CHAN" trace
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
      s" END SEND-CHAN" trace
    ;] critical
  ;

  \ Receive data from a channel
  : recv-chan ( addr bytes chan -- recv-bytes )
    [:
      s" BEGIN RECV-CHAN" trace
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
      s" END RECV-CHAN" trace
    ;] critical
  ;

  \ Peek data from a channel
  : peek-chan ( addr bytes chan -- addr peek-bytes )
    [:
      s" BEGIN PEEK-CHAN" trace
      current-task prepare-block
      dup wait-recv-chan
      >r 2dup 0 fill
      r@ chan-data-size @ min r> recv-chan-addr -rot dup >r move r>
      s" END PEEK-CHAN" trace
    ;] critical
  ;

  \ Skip data on a channel
  : skip-chan ( chan -- )
    [:
      s" BEGIN SKIP-CHAN" trace
      current-task prepare-block
      dup wait-recv-chan
      dup advance-recv-chan
      dup chan-send-ready @ 0> if
	chan-send-tqueue wake-tqueue
      else
	drop
      then
      s" END SKIP-CHAN" trace
    ;] critical
  ;

  \ Send data to a channel without blocking (raise x-would-block if blocking
  \ would normally occur)
  : send-chan-no-block ( addr bytes chan -- )
    [:
      s" BEGIN SEND-CHAN-NO-BLOCK" trace
      dup chan-closed @ triggers x-chan-closed
      dup chan-full-unsafe? triggers x-would-block
      dup send-chan-addr over chan-data-size @ 0 fill
      dup >r chan-data-size @ min r@ send-chan-addr swap move r>
      dup advance-send-chan
      dup chan-recv-ready @ 0> if
	chan-recv-tqueue wake-tqueue
      else
	drop
      then
      s" END SEND-CHAN-NO-BLOCK" trace
    ;] critical
  ;

  \ Receive data from a channel without blocking (raise x-would-block if
  \ blocking would normally occur)
  : recv-chan-no-block ( addr bytes chan -- addr recv-bytes )
    [:
      s" BEGIN RECV-CHAN-NO-BLOCK" trace
      dup chan-empty-unsafe? triggers x-would-block
      >r 2dup 0 fill
      r@ chan-data-size @ min r@ recv-chan-addr -rot dup >r move 2r>
      dup advance-recv-chan
      dup chan-send-ready @ 0> if
	chan-send-tqueue wake-tqueue
      else
	drop
      then
      s" END RECV-CHAN-NO-BLOCK" trace
    ;] critical
  ;

  \ Peek data from a channel without blocking (raise x-would-block if blocking
  \ would normally occur)
  : peek-chan-no-block ( addr bytes chan -- addr peek-bytes )
    [:
      s" BEGIN PEEK-CHAN-NO-BLOCK" trace
      dup chan-empty-unsafe? triggers x-would-block
      >r 2dup 0 fill
      r@ chan-data-size @ min r> recv-chan-addr -rot dup >r move r>
      s" END PEEK-CHAN-NO-BLOCK" trace
    ;] critical
  ;

  \ Skip data on a channel without blocking (raise x-would-block if blocking
  \ would normally occur)
  : skip-chan-no-block ( chan -- )
    [:
      s" BEGIN SKIP-CHAN-NO-BLOCK" trace
      dup chan-empty-unsafe? triggers x-would-block
      dup advance-recv-chan
      dup chan-send-ready @ 0> if
	chan-send-tqueue wake-tqueue
      else
	drop
      then
      s" END SKIP-CHAN-NO-BLOCK" trace
    ;] critical
  ;

  commit-flash

  \ Close a channel
  : close-chan ( chan -- )
    [:
      true over chan-closed !
      dup chan-send-tqueue wake-tqueue-all
      chan-recv-tqueue wake-tqueue-all
    ;] critical
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
