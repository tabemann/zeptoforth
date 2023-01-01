\ Copyright (c) 2022-2023 Travis Bemann
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

compile-to-flash

begin-module schan

  task import
  slock import

  begin-module schan-internal

    \ Simple channel header structure
    begin-structure schan-header-size

      \ Simple channel send simple lock
      slock-size +field schan-send-slock

      \ Simple channel receive simple lock
      slock-size +field schan-recv-slock

      \ Simple channel element count
      field: schan-count

      \ Simple channel data size
      field: schan-data-size

      \ Simple channel send index
      field: schan-send-index
      
      \ Simple channel receive index
      field: schan-recv-index

      \ Simple channel is closed
      field: schan-closed
      
    end-structure

  end-module> import

  \ Simple channel is closed exception
  : x-schan-closed ( -- ) ." schannel is closed" cr ;

  \ Get whether a simple channel is full
  : schan-full? ( schan -- flag )
    dup schan-recv-index @ 1+ over schan-count @ umod
    swap schan-send-index @ =
  ;
  
  \ Get whether a simple channel is empty
  : schan-empty? ( schan -- flag )
    dup schan-recv-index @ swap schan-send-index @ =
  ;
  
  continue-module schan-internal

    \ Get the simple channel send address
    : send-schan-addr ( schan -- b-addr )
      dup schan-send-index @ over schan-data-size @ * schan-header-size + +
    ;

    \ Get the simple channel receive address
    : recv-schan-addr ( schan -- b-addr )
      dup schan-recv-index @ over schan-data-size @ * schan-header-size + +
    ;

    \ Advance the simple channel send index
    : advance-send-schan ( schan -- )
      dup schan-send-index @ 1+ over schan-count @ umod swap schan-send-index !
    ;

    \ Advance the simple channel receive index
    : advance-recv-schan ( schan -- )
      dup schan-recv-index @ 1+ over schan-count @ umod swap schan-recv-index !
    ;

    \ Wait to send on a simple channel
    : wait-send-schan ( schan -- )
      begin
	dup schan-send-slock claim-slock-timeout
	dup schan-closed @ if
	  schan-send-slock release-slock ['] x-schan-closed ?raise
	then
	dup schan-full? if
	  dup schan-send-slock release-slock pause false
	else
	  true
	then
      until
      drop
    ;

    \ Wait to receive on a simple channel
    : wait-recv-schan ( schan -- )
      begin
	dup schan-recv-slock claim-slock-timeout
	dup schan-empty? if
	  dup schan-closed @ if
	    schan-recv-slock release-slock ['] x-schan-closed ?raise
	  then
	  dup schan-recv-slock release-slock pause false
	else
	  true
	then
      until
      drop
    ;

  end-module

  \ Get simple channel size for a channel with a specified element size in bytes
  \ and element count
  : schan-size ( element-bytes element-count -- total-bytes )
    1+ * schan-header-size + 4 align
  ;

  \ Initialize a simple channel with a specified element size in bytes and
  \ element count at a specified address
  : init-schan ( element-bytes element-count addr -- )
    dup schan-send-slock init-slock
    dup schan-recv-slock init-slock
    swap 1+ over schan-count !
    tuck schan-data-size !
    0 over schan-send-index !
    0 over schan-recv-index !
    false swap schan-closed !
  ;

  \ Send data to a simple channel
  : send-schan ( addr bytes schan -- )
    dup schan-closed @ triggers x-schan-closed
    current-task prepare-block
    dup wait-send-schan
    dup send-schan-addr over schan-data-size @ 0 fill
    dup >r schan-data-size @ min r@ send-schan-addr swap move r>
    dup advance-send-schan
    schan-send-slock release-slock
  ;

  \ Receive data from a simple channel
  : recv-schan ( addr bytes schan -- recv-bytes )
    current-task prepare-block
    dup wait-recv-schan
    >r 2dup 0 fill
    r@ schan-data-size @ min r@ recv-schan-addr -rot dup >r move 2r>
    dup advance-recv-schan
    schan-recv-slock release-slock
  ;

  \ Peek data from a simple channel
  : peek-schan ( addr bytes schan -- recv-bytes )
    current-task prepare-block
    dup wait-recv-schan
    >r 2dup 0 fill
    r@ schan-data-size @ min r@ recv-schan-addr -rot dup >r move 2r>
    schan-recv-slock release-slock
  ;

  \ Skip data on a simple channel
  : skip-schan ( schan -- )
    current-task prepare-block
    dup wait-recv-schan
    dup advance-recv-schan
    schan-recv-slock release-slock
  ;

  \ Non-blocking send data to a simple channel (ISR safe)
  : send-schan-no-block ( addr bytes schan -- )
    dup schan-closed @ triggers x-schan-closed
    dup schan-send-slock try-claim-slock averts x-would-block
    dup schan-closed @ if
      schan-send-slock release-slock
      ['] x-schan-closed ?raise
    then
    dup schan-full? if
      schan-send-slock release-slock
      ['] x-would-block ?raise
    then
    dup send-schan-addr over schan-data-size @ 0 fill
    dup >r schan-data-size @ min r@ send-schan-addr swap move r>
    dup advance-send-schan
    schan-send-slock release-slock
  ;

  \ Non-blocking receive data from a simple channel (ISR safe)
  : recv-schan-no-block ( addr bytes schan -- recv-bytes )
    dup schan-recv-slock try-claim-slock averts x-would-block
    dup schan-empty? if
      dup schan-closed @ if
	schan-recv-slock release-slock
	['] x-schan-closed ?raise
      else
	schan-recv-slock release-slock
	['] x-would-block ?raise
      then
    then
    >r 2dup 0 fill
    r@ schan-data-size @ min r@ recv-schan-addr -rot dup >r move 2r>
    dup advance-recv-schan
    schan-recv-slock release-slock
  ;

  \ Non-blocking peek data from a simple channel (ISR safe)
  : peek-schan-no-block ( addr bytes schan -- recv-bytes )
    dup schan-recv-slock try-claim-slock averts x-would-block
    dup schan-empty? if
      dup schan-closed @ if
	schan-recv-slock release-slock
	['] x-schan-closed ?raise
      else
	schan-recv-slock release-slock
	['] x-would-block ?raise
      then
    then
    >r 2dup 0 fill
    r@ schan-data-size @ min r@ recv-schan-addr -rot dup >r move 2r>
    schan-recv-slock release-slock
  ;

  \ Non-blocking skip data on a simple channel (ISR safe)
  : skip-schan-no-block ( schan -- )
    dup schan-recv-slock try-claim-slock averts x-would-block
    dup schan-empty? if
      dup schan-closed @ if
	schan-recv-slock release-slock
	['] x-schan-closed ?raise
      else
	schan-recv-slock release-slock
	['] x-would-block ?raise
      then
    then
    dup wait-recv-schan
    dup advance-recv-schan
    schan-recv-slock release-slock
  ;

  \ Close a simple channel
  : close-schan ( schan -- ) true swap schan-closed ! ;

  \ Get whether a simple channel is closed
  : schan-closed? ( schan -- closed ) schan-closed @ ;

  \ Reopen a simple channel
  : reopen-schan ( schan -- ) false swap schan-closed ! ;
  
  \ Get the simple channel element count
  : schan-count ( schan -- element-count ) schan-count @ 1- ;

  \ Get the simple channel element data size
  : schan-data-size ( schan -- element-bytes ) schan-data-size @ ;
  
end-module

reboot
