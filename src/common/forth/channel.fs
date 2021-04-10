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
  
  begin-import-module chan-internal-module

    \ Channel header structure
    begin-structure chan-header-size
      
      \ Channel byte count
      field: chan-count

      \ Channel receive index
      field: chan-recv-index

      \ Channel send index
      field: chan-send-index

      \ Channel receive task
      field: chan-recv-task

      \ Channel send task
      field: chan-send-task

      \ Channel is closed
      field: chan-closed

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
      begin
	dup chan-send-task @ 0<> over chan-send-task @ current-task <> and
      while
	end-critical
	pause
	begin-critical
      repeat
      dup chan-closed @ if
	end-critical ['] x-chan-closed ?raise
      then
      begin dup chan-full-unsafe? while
	current-task over chan-send-task !
	\      begin dup chan-recv-task @ 0= while pause repeat
	dup chan-recv-task @ ?dup if run then
	current-task stop
	end-critical
	pause
	begin-critical
	dup chan-closed @ if
	  end-critical ['] x-chan-closed ?raise
	then
      repeat
      0 swap chan-send-task !
    ;

    \ Wait to receive on a channel
    : wait-recv-chan ( chan -- )
      begin
	dup chan-recv-task @ 0<> over chan-recv-task @ current-task <> and
      while
	end-critical
	pause
	begin-critical
      repeat
      begin dup chan-empty-unsafe? while
	dup chan-closed @ if
	  end-critical ['] x-chan-closed ?raise
	then
	current-task over chan-recv-task !
	dup chan-send-task @ ?dup if run then
	current-task stop
	end-critical
	pause
	begin-critical
      repeat
      0 swap chan-recv-task !
    ;

    \ Get the channel send address
    : send-chan-addr ( chan -- b-addr )
      dup chan-send-index @ chan-header-size + +
    ;

    \ Get the channel receive address
    : recv-chan-addr ( chan -- b-addr )
      dup chan-recv-index @ chan-header-size + +
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
  
  \ Get channel size for a channel with a specified buffer size in bytes
  : chan-size ( bytes -- total-bytes ) 4 align chan-header-size + ;

  \ Initialize a channel for a channel with a specified buffer size in bytes
  : init-chan ( addr bytes -- )
    over chan-count !
    0 over chan-recv-index !
    0 over chan-send-index !
    0 over chan-recv-task !
    0 over chan-send-task !
    false swap chan-closed !
  ;

  \ Send a byte to a channel
  : send-chan-byte ( b chan -- )
    begin-critical
    dup chan-closed @ if
      end-critical ['] x-chan-closed ?raise
    then
    dup wait-send-chan
    tuck send-chan-addr b!
    dup advance-send-chan
    chan-recv-task @ ?dup if run then
    end-critical
  ;

  \ Receive a byte from a channel
  : recv-chan-byte ( chan -- b )
    begin-critical
    dup wait-recv-chan
    dup recv-chan-addr b@
    over advance-recv-chan
    swap chan-send-task @ ?dup if run then
    end-critical
  ;

  \ Send bytes to a channel
  : send-chan ( addr bytes chan -- )
    >r begin dup 0> while
      swap dup b@ r@ send-chan-byte 1+ swap 1-
    repeat
    2drop rdrop
  ;

  \ Receive bytes from a channel
  : recv-chan ( addr bytes chan -- )
    >r begin dup 0> while
      r@ recv-chan-byte 2 pick b! 1- swap 1+ swap
    repeat
    2drop rdrop
  ;

  \ Send a cell to a channel
  : send-chan-cell ( x chan -- )
    swap pad ! pad 4 rot send-chan
  ;

  \ Receive a cell from a channel
  : recv-chan-cell ( chan -- x )
    pad 4 rot recv-chan pad @
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
