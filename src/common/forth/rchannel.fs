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

begin-module rchan-module

  task-module import
  tqueue-module import
  lock-module import

  begin-module rchan-internal-module

    \ Reply channel header structure
    begin-structure rchan-size
      \ Reply channel send data address
      field: rchan-send-addr

      \ Reply channel send data size
      field: rchan-send-size

      \ Reply channel reply data address
      field: rchan-reply-addr

      \ Reply channel reply data size address
      field: rchan-reply-size-addr

      \ Reply channel is closed
      field: rchan-closed

      \ Reply channel reply task
      field: rchan-reply-task
      
      \ Reply channel receive lock
      lock-size +field rchan-recv-lock
      
      \ Reply channel send task queue
      tqueue-size +field rchan-send-tqueue

      \ Reply channel receive task queue
      tqueue-size +field rchan-recv-tqueue

      \ Reply channel response task queue
      tqueue-size +field rchan-resp-tqueue
    end-structure

  end-module> import

  \ Commit to flash
  commit-flash
  
  \ Reply channel is closed exception
  : x-rchan-closed ( -- ) space ." rchannel is closed" cr ;
  
  \ Get whether a reply channel is closed
  : rchan-closed? ( rchan -- closed? ) rchan-closed @ ;

  \ Reply channel is not waiting for a reply from current task
  : x-rchan-not-wait-reply ( -- )
    space ." rchannel is not waiting for reply from current task" cr
  ;

  \ Attempted to receive from a reply channel awaiting a reply from the current
  \ task
  : x-rchan-wait-reply ( -- )
    space ." rchannel is waiting for reply from current task" cr
  ;
  
  \ Get whether a reply channel is waiting for a reply
  : rchan-wait-reply? ( rchan -- wait-reply? )
    rchan-reply-task @ 0<>
  ;
  
  commit-flash
  
  \ Initialize a reply channel
  : init-rchan ( addr -- )
    0 over rchan-send-addr !
    0 over rchan-send-size !
    0 over rchan-reply-addr !
    0 over rchan-reply-size-addr !
    false over rchan-closed !
    0 over rchan-reply-task !
    dup rchan-recv-lock init-lock
    dup rchan-send-tqueue init-tqueue
    dup rchan-recv-tqueue init-tqueue
    rchan-resp-tqueue init-tqueue
  ;

  \ Send data on a reply channel
  : send-rchan
    ( send-addr send-bytes reply-addr reply-bytes rchan -- addr reply-bytes' )
    [:
      s" BEGIN SEND-RCHAN" trace
      dup rchan-closed? triggers x-rchan-closed
      current-task prepare-block
      dup rchan-send-tqueue wait-tqueue
      dup rchan-closed? triggers x-rchan-closed
      cell [:
	>r r@ over rchan-reply-size-addr !
	swap r@ !
	over >r tuck rchan-reply-addr !
	tuck rchan-send-size !
	tuck rchan-send-addr !
	dup rchan-recv-tqueue wake-tqueue
	[: dup rchan-resp-tqueue wait-tqueue ;] try ?dup if
	  0 swap rchan-send-addr ! ?raise
	then
	rchan-closed? triggers x-rchan-closed
	2r> @
      ;] with-aligned-allot
      s" END SEND-RCHAN" trace
    ;] critical
  ;

  \ Receive data on a reply channel
  : recv-rchan ( addr bytes rchan -- addr recv-bytes )
    dup rchan-closed? triggers x-rchan-closed
    dup rchan-reply-task @ current-task <> averts x-rchan-wait-reply
    dup rchan-recv-lock lock
    dup >r [:
      [:
	s" BEGIN RECV-RCHAN" trace
	dup rchan-send-tqueue wake-tqueue
	[: dup rchan-recv-tqueue wait-tqueue ;] try ?dup if
	  swap rchan-send-tqueue unwake-tqueue ?raise
	then
	dup rchan-closed? triggers x-rchan-closed
	>r
	2dup 0 fill
	r@ rchan-send-size @ min
	r@ rchan-send-addr @ ?dup if -rot
	  2dup 2>r move 2r>
	else
	  rot drop drop 0
	then
	current-task r> rchan-reply-task !
	s" END RECV-RCHAN" trace
      ;] critical
    ;] try r> swap ?dup if swap rchan-recv-lock unlock ?raise else drop then
  ;

  \ Reply to a received message on a reply channel
  : reply-rchan ( addr bytes rchan -- )
    dup rchan-closed? triggers x-rchan-closed
    current-task over rchan-reply-task @ = averts x-rchan-not-wait-reply
    >r
    r@ rchan-reply-size-addr @ @ min
    dup r@ rchan-reply-size-addr @ !
    r@ rchan-reply-addr @ swap move
    r@ rchan-resp-tqueue wake-tqueue
    0 r@ rchan-reply-task !
    r> rchan-recv-lock unlock
  ;

  commit-flash

  \ Close a reply channel
  : close-rchan ( rchan -- )
    [:
      true over rchan-closed !
      dup rchan-send-tqueue wake-tqueue-all
      rchan-recv-tqueue wake-tqueue-all
    ;] critical
  ;

  \ Reopen a reply channel
  : reopen-rchan ( rchan -- ) false swap rchan-closed ! ;

  \ Export the rchannel size
  export rchan-size

end-module

end-compress-flash

\ Reboot
reboot
