\ Copyright (c) 2020-2024 Travis Bemann
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

begin-module rchan

  task import
  slock import

  \ No receiving task is ready
  : x-no-recv-ready ( -- ) ." no receiving task is ready" cr ;
  
  begin-module rchan-internal

    \ Bidirectional channel queue structure
    begin-structure rchan-queue-size

      \ First wait in bidirectional channel queue
      field: rchan-queue-first

      \ Last wait in bidirectional channel queue
      field: rchan-queue-last
      
    end-structure

    \ Bidirectional channel header structure
    begin-structure rchan-size

      \ Bidirectional channel simple lock
      slock-size +field rchan-slock
      
      \ Bidirectional channel is closed
      field: rchan-closed

      \ Bidirectional channel send queue
      rchan-queue-size +field rchan-send-queue

      \ Bidirectional channel receive queue
      rchan-queue-size +field rchan-recv-queue

      \ Bidirectional channel reply task
      field: rchan-reply-task

      \ Bidirectional channel reply buffer
      field: rchan-reply-buf

      \ Bidirectional channel reply buffer size
      field: rchan-reply-buf-size

    end-structure

    \ Bidirectional channel wait structure
    begin-structure rchan-wait-size

      \ Waiting task
      field: rchan-wait-task

      \ Data buffer
      field: rchan-wait-buf

      \ Data buffer size
      field: rchan-wait-buf-size

      \ Reply buffer
      field: rchan-wait-reply-buf

      \ Reply buffer size
      field: rchan-wait-reply-buf-size

      \ Previous entry in queue
      field: rchan-wait-prev

      \ Next entry in queue
      field: rchan-wait-next

      \ Popped flag
      field: rchan-wait-popped

    end-structure

    \ Initialize a bidirectional channel queue
    : init-rchan-queue ( addr -- )
      0 over rchan-queue-first !
      0 swap rchan-queue-last !
    ;

    \ Get last higher priority wait in queue
    : find-rchan-queue-next ( priority queue -- wait|0 )
      rchan-queue-last @
      begin dup while
	dup rchan-wait-task @ task-priority@
	2 pick >= if nip exit then
	rchan-wait-next @
      repeat
      nip
    ;

    \ Insert a wait into a queue
    : push-rchan-queue ( wait queue -- )
      over false swap rchan-wait-popped ! ( wait queue )
      2dup swap rchan-wait-task @ task-priority@ swap find-rchan-queue-next
      dup 0= if
	drop over 0 swap rchan-wait-next ! ( wait queue )
	2dup rchan-queue-first @ ( wait queue wait first )
	?dup if
	  2dup swap rchan-wait-prev ! ( wait queue wait first )
	  rchan-wait-next ! ( wait queue )
	else
	  0 swap rchan-wait-prev ! ( wait queue )
	  2dup rchan-queue-last ! ( wait queue )
	then
	rchan-queue-first ! ( )
      else ( wait queue next )
	dup rchan-wait-prev @ ( wait queue next prev )
	dup 4 pick rchan-wait-prev ! ( wait queue next prev )
	?dup if
	  3 pick swap rchan-wait-next ! ( wait queue next )
	else
	  2 pick 2 pick rchan-queue-last ! ( wait queue next )
	then
	dup 3 pick rchan-wait-next ! ( wait queue next )
	nip rchan-wait-prev ! ( )
      then
    ;

    \ Pop a wait from a queue or return null if no queue is available
    : pop-rchan-queue ( queue -- wait|0 )
      dup rchan-queue-first @ dup if ( queue first )
	true over rchan-wait-popped ! ( queue first )
	dup rchan-wait-prev @ ( queue first prev )
	dup 3 pick rchan-queue-first ! ( queue first prev )
	?dup if
	  0 swap rchan-wait-next ! ( queue first )
	  nip ( first )
	else
	  0 rot rchan-queue-last ! ( first )
	then
      else
	nip ( 0 )
      then
    ;

    \ Remove a wait from a queue if it has not already been popped
    : remove-rchan-queue ( wait queue -- )
      over rchan-wait-popped @ not if ( wait queue )
	over rchan-wait-next @ ?dup if ( wait queue next )
	  2 pick rchan-wait-prev @ ( wait queue next prev )
	  swap rchan-wait-prev ! ( wait queue )
	else
	  over rchan-wait-prev @ ( wait queue prev )
	  over rchan-queue-first ! ( wait queue )
	then
	over rchan-wait-prev @ ?dup if ( wait queue prev )
	  2 pick rchan-wait-next @ ( wait queue prev next )
	  swap rchan-wait-next ! ( wait queue )
	else
	  over rchan-wait-next @ ( wait queue next )
	  over rchan-queue-last ! ( wait queue )
	then
      then
      2drop ( )
    ;

    \ Wake all the tasks in a queue
    : wake-all-rchan-queue ( queue -- )
      >r r@ rchan-queue-first @
      begin ?dup while
	dup rchan-wait-task @ ready
	rchan-wait-prev @
      repeat
      0 r@ rchan-queue-first !
      0 r> rchan-queue-last !
    ;

  end-module> import

  \ Bidirectional channel is closed exception
  : x-rchan-closed ( -- ) ." rchannel is closed" cr ;

  \ Reply pending exception
  : x-reply-pending ( -- ) ." reply pending" cr ;

  commit-flash
  
  \ Initialize a bidirectional channel
  : init-rchan ( addr -- )
    dup rchan-slock init-slock
    false over rchan-closed !
    dup rchan-send-queue init-rchan-queue
    dup rchan-recv-queue init-rchan-queue
    0 over rchan-reply-task !
    0 over rchan-reply-buf !
    0 over rchan-reply-buf-size !
  ;

  \ Send data on a bidirectional channel
  : send-rchan ( s-addr s-bytes r-addr r-bytes rchan -- r-bytes' )
    >r
    r@ rchan-closed @ triggers x-rchan-closed
    r@ rchan-slock claim-slock
    current-task prepare-block
    r@ rchan-reply-task @ 0= if
      r@ rchan-recv-queue pop-rchan-queue ?dup if
	( s-addr s-bytes r-addr r-bytes wait )
	3 pick over rchan-wait-buf-size @ min
	( s-addr s-bytes r-addr r-bytes wait s-bytes' )
	>r 4 pick over rchan-wait-buf @ r@
	( s-addr s-bytes r-addr r-bytes wait s-addr recv-addr s-bytes' )
	move ( s-addr s-bytes r-addr r-bytes wait )
	r> over rchan-wait-buf-size ! ( s-addr s-bytes r-addr r-bytes wait )
	swap r@ rchan-reply-buf-size ! ( s-addr s-bytes r-addr wait )
	swap r@ rchan-reply-buf ! ( s-addr s-bytes wait )
	current-task r@ rchan-reply-task ! ( s-addr s-bytes wait )
	-rot 2drop ( wait )
	rchan-wait-task @ ready ( )
	r@ rchan-slock release-slock-block ( )
	r@ rchan-reply-buf-size @ ( r-bytes' )
	r@ rchan-reply-task @ 1 bic ( r-bytes' task )
	current-task = if 0 r@ rchan-reply-task ! then ( r-bytes' )
	current-task check-timeout triggers x-timed-out
	r> rchan-closed @ triggers x-rchan-closed ( r-bytes' )
	false
      else
	true
      then
    else
      true
    then
    if
      r> rchan-wait-size [: swap >r ( s-addr s-bytes r-addr r-bytes wait )
	tuck rchan-wait-reply-buf-size ! ( s-addr s-bytes r-addr wait )
	tuck rchan-wait-reply-buf ! ( s-addr s-bytes wait )
	current-task over rchan-wait-task ! ( s-addr s-bytes wait )
	tuck rchan-wait-buf-size ! ( s-addr wait )
	tuck rchan-wait-buf ! ( wait )
	dup r@ rchan-send-queue push-rchan-queue ( wait )
	r@ rchan-slock release-slock-block ( wait )
	current-task check-timeout if
	  r@ rchan-slock claim-slock ( wait )
	  r@ rchan-reply-task @ 1 bic ( wait task )
	  current-task = if 0 r@ rchan-reply-task ! then ( wait )
	  r@ rchan-send-queue remove-rchan-queue ( )
	  r> rchan-slock release-slock ( )
	  ['] x-timed-out ?raise
	else ( wait )
	  drop ( )
	  r@ rchan-reply-buf-size @ ( r-bytes' )
	  0 r@ rchan-reply-task ! ( r-bytes' )
	  r> rchan-closed @ triggers x-rchan-closed ( r-bytes' )
	then
      ;] with-aligned-allot
    then
  ;

  \ Send data on a bidirectional channel, raising X-NO-RECV-READY if no
  \ receiving task is ready
  : send-rchan-recv-ready-only
    ( s-addr s-bytes r-addr r-bytes rchan -- r-bytes' )
    >r
    r@ rchan-closed @ triggers x-rchan-closed
    r@ rchan-slock claim-slock
    current-task prepare-block
    r@ rchan-reply-task @ 0= if
      r@ rchan-recv-queue pop-rchan-queue ?dup if
	( s-addr s-bytes r-addr r-bytes wait )
	3 pick over rchan-wait-buf-size @ min
	( s-addr s-bytes r-addr r-bytes wait s-bytes' )
	>r 4 pick over rchan-wait-buf @ r@
	( s-addr s-bytes r-addr r-bytes wait s-addr recv-addr s-bytes' )
	move ( s-addr s-bytes r-addr r-bytes wait )
	r> over rchan-wait-buf-size ! ( s-addr s-bytes r-addr r-bytes wait )
	swap r@ rchan-reply-buf-size ! ( s-addr s-bytes r-addr wait )
	swap r@ rchan-reply-buf ! ( s-addr s-bytes wait )
	current-task r@ rchan-reply-task ! ( s-addr s-bytes wait )
	-rot 2drop ( wait )
	rchan-wait-task @ ready ( )
	r@ rchan-slock release-slock-block ( )
	r@ rchan-reply-buf-size @ ( r-bytes' )
	r@ rchan-reply-task @ 1 bic ( r-bytes' task )
	current-task = if 0 r@ rchan-reply-task ! then ( r-bytes' )
	current-task check-timeout triggers x-timed-out
	r> rchan-closed @ triggers x-rchan-closed ( r-bytes' )
	false
      else
	true
      then
    else
      true
    then
    if
      r> rchan-slock release-slock
      ['] x-no-recv-ready ?raise
    then
  ;

  \ Receive data on a bidirectional channel
  : recv-rchan ( addr bytes rchan -- recv-bytes )
    >r
    r@ rchan-closed @ triggers x-rchan-closed
    r@ rchan-slock claim-slock
    current-task prepare-block
    r@ rchan-send-queue pop-rchan-queue ?dup if ( addr bytes wait )
      swap over rchan-wait-buf-size @ min ( addr wait bytes )
      >r swap over rchan-wait-buf @ swap r@ ( wait send-addr addr bytes ) move
      r> swap ( bytes wait )
      dup rchan-wait-reply-buf-size @ r@ rchan-reply-buf-size ! ( bytes wait )
      dup rchan-wait-reply-buf @ r@ rchan-reply-buf ! ( bytes wait )
      rchan-wait-task @ r@ rchan-reply-task ! ( bytes wait )
      r> rchan-slock release-slock ( bytes )
    else
      r> rchan-wait-size [: swap >r ( addr bytes wait )
	current-task over rchan-wait-task ! ( addr bytes wait )
	tuck rchan-wait-buf-size ! ( addr wait )
	tuck rchan-wait-buf ! ( wait )
	dup r@ rchan-recv-queue push-rchan-queue ( wait )
	r@ rchan-slock release-slock-block ( wait )
	current-task check-timeout if
	  r@ rchan-slock claim-slock ( wait )
	  r@ rchan-recv-queue remove-rchan-queue ( )
	  r> rchan-slock release-slock ( )
	  ['] x-timed-out ?raise
	else ( wait )
	  rchan-wait-buf-size @ ( bytes )
	  r> rchan-closed @ triggers x-rchan-closed ( bytes )
	then
      ;] with-aligned-allot
    then
  ;
  
  \ Receive data on a bidirectional channel, raising X-WOULD-BLOCK if blocking
  \ would occur
  : recv-rchan-no-block ( addr bytes rchan -- recv-bytes )
    >r
    r@ rchan-closed @ triggers x-rchan-closed
    r@ rchan-slock claim-slock
    r@ rchan-send-queue pop-rchan-queue ?dup if ( addr bytes wait )
      swap over rchan-wait-buf-size @ min ( addr wait bytes )
      >r swap over rchan-wait-buf @ swap r@ ( wait send-addr addr bytes ) move
      r> swap ( bytes wait )
      dup rchan-wait-reply-buf-size @ r@ rchan-reply-buf-size ! ( bytes wait )
      dup rchan-wait-reply-buf @ r@ rchan-reply-buf ! ( bytes wait )
      rchan-wait-task @ r@ rchan-reply-task ! ( bytes wait )
      r> rchan-slock release-slock ( bytes )
    else
      r> rchan-slock release-slock
      ['] x-would-block ?raise
    then
  ;

  \ Reply to a bidirectional channel
  : reply-rchan ( addr bytes rchan -- )
    >r ( addr bytes )
    r@ rchan-closed @ triggers x-rchan-closed ( addr bytes )
    r@ rchan-slock claim-slock ( addr bytes )
    r@ rchan-reply-task @ if ( addr bytes )
      r@ rchan-reply-task @ 1 and triggers x-reply-pending ( addr bytes )
      r@ rchan-reply-buf @ swap ( addr r-addr bytes )
      r@ rchan-reply-buf-size @ min ( addr r-addr r-bytes )
      dup r@ rchan-reply-buf-size !
      move ( )
      r@ rchan-reply-task @ ( r-task )
      dup 1 or r@ rchan-reply-task ! ( r-task )
      ready ( )
      r> rchan-slock release-slock ( )
    else
      2drop r> rchan-slock release-slock ( )
    then
  ;

  \ Close a bidirectional channel
  : close-rchan ( rchan -- )
    [:
      >r
      true r@ rchan-closed !
      r@ rchan-send-queue wake-all-rchan-queue
      r@ rchan-recv-queue wake-all-rchan-queue
      r@ rchan-reply-task @ 1 bic ?dup if ready then
      0 r> rchan-reply-task !
    ;] over rchan-slock with-slock
  ;

  \ Get whether a bidirectional channel is closed
  : rchan-closed? ( rchan -- closed ) rchan-closed @ ;

  \ Reopen a bidirectional channel
  : reopen-rchan ( rchan -- )
    [: true swap rchan-closed ! ;] over rchan-slock with-slock
  ;
  
  commit-flash

  \ Export the rchannel size
  ' rchan-size export rchan-size

end-module

end-compress-flash

\ Reboot
reboot
