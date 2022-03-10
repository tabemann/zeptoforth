\ Copyright (c) 2020-2022 Travis Bemann
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

begin-module fchan

  task import
  slock import
  tqueue import
  lock import

  begin-module fchan-internal

    \ Rendezvous channel header structure
    begin-structure fchan-size

      \ Rendezvous channel simple lock
      slock-size +field fchan-slock
      
      \ Rendezvous channel data address
      field: fchan-data-addr

      \ Rendezvous channel data size
      field: fchan-data-size
      
      \ Rendezvous channel is closed
      field: fchan-closed

      \ Rendezvous channel receive lock
      lock-size +field fchan-recv-lock
      
      \ Rendezvous channel send task queue
      tqueue-size +field fchan-send-tqueue

      \ Rendezvous channel receive task queue
      tqueue-size +field fchan-recv-tqueue

      \ Rendezvous channel response task queue
      tqueue-size +field fchan-resp-tqueue
    end-structure

  end-module> import

  \ Rendezvous channel is closed exception
  : x-fchan-closed ( -- ) space ." fchannel is closed" cr ;
  
  commit-flash
  
  \ Initialize a rendezvous channel
  : init-fchan ( addr -- )
    dup fchan-slock init-slock
    0 over fchan-data-addr !
    0 over fchan-data-size !
    false over fchan-closed !
    dup fchan-recv-lock init-lock
    dup fchan-slock over fchan-send-tqueue init-tqueue
    dup fchan-slock over fchan-recv-tqueue init-tqueue
    dup fchan-slock swap fchan-resp-tqueue init-tqueue
  ;

  \ Send data on a rendezvous channel
  : send-fchan ( addr bytes fchan -- )
    [:
      s" BEGIN SEND-FCHAN" trace
      dup fchan-closed @ triggers x-fchan-closed
      current-task prepare-block
      dup fchan-send-tqueue wait-tqueue
      dup fchan-closed @ triggers x-fchan-closed
      tuck fchan-data-size !
      tuck fchan-data-addr !
      dup fchan-recv-tqueue wake-tqueue
      [: dup fchan-resp-tqueue wait-tqueue ;] try ?dup if
	0 swap fchan-data-addr ! ?raise
      then
      fchan-closed @ triggers x-fchan-closed
      s" END SEND-FCHAN" trace
    ;] over fchan-slock with-slock
  ;

  \ Receive data on a rendezvous channel
  : recv-fchan ( addr bytes fchan -- recv-bytes )
    dup fchan-closed @ triggers x-fchan-closed
    [:
      [:
	s" BEGIN RECV-FCHAN" trace
	dup fchan-send-tqueue wake-tqueue
	[: dup fchan-recv-tqueue wait-tqueue ;] try ?dup if
	  swap fchan-send-tqueue unwake-tqueue ?raise
	then
	dup fchan-closed @ triggers x-fchan-closed
	>r
	2dup 0 fill
	r@ fchan-data-size @ min
	r@ fchan-data-addr @ ?dup if -rot
	  dup >r move r>
	else
	  2drop 0
	then
	r> fchan-resp-tqueue wake-tqueue
	s" END RECV-FCHAN" trace
      ;] over fchan-slock with-slock
    ;] over fchan-recv-lock with-lock
  ;

  commit-flash

  \ Close a rendezvous channel
  : close-fchan ( fchan -- )
    [:
      true over fchan-closed !
      dup fchan-send-tqueue wake-tqueue-all
      fchan-recv-tqueue wake-tqueue-all
    ;] over fchan-slock with-slock
  ;

  \ Get whether a rendezvous channel is closed
  : fchan-closed? ( fchan -- closed ) fchan-closed @ ;

  \ Reopen a rendezvous channel
  : reopen-fchan ( fchan -- ) false swap fchan-closed ! ;

  \ Export the fchannel size
  export fchan-size

end-module

end-compress-flash

\ Reboot
reboot
