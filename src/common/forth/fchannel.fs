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

  begin-module fchan-internal

    \ Rendezvous channel queue structure
    begin-structure fchan-queue-size

      \ First wait in rendezvous channel queue
      field: fchan-queue-first

      \ Last wait in rendezvous channel queue
      field: fchan-queue-last
      
    end-structure

    \ Rendezvous channel header structure
    begin-structure fchan-size

      \ Rendezvous channel simple lock
      slock-size +field fchan-slock

      \ Rendezvous channel is closed
      field: fchan-closed
      
      \ Rendezvous channel send queue
      fchan-queue-size +field fchan-send-queue

      \ Rendezvous channel receive queue
      fchan-queue-size +field fchan-recv-queue

    end-structure

    \ Rendezvous channel wait structure
    begin-structure fchan-wait-size

      \ Waiting task
      field: fchan-wait-task

      \ Data buffer
      field: fchan-wait-buf

      \ Data buffer size
      field: fchan-wait-buf-size

      \ Previous entry in queue
      field: fchan-wait-prev

      \ Next entry in queue
      field: fchan-wait-next

      \ Popped flag
      field: fchan-wait-popped

    end-structure

    \ Initialize a rendezvous channel queue
    : init-fchan-queue ( addr -- )
      0 over fchan-queue-first !
      0 swap fchan-queue-last !
    ;

    \ Get last higher priority wait in queue
    : find-fchan-queue-next ( priority queue -- wait|0 )
      fchan-queue-last @
      begin dup while
	dup fchan-wait-task @ task-priority@
	2 pick >= if nip exit then
	fchan-wait-next @
      repeat
      nip
    ;

    \ Insert a wait into a queue
    : push-fchan-queue ( wait queue -- )
      over false swap fchan-wait-popped ! ( wait queue )
      2dup swap fchan-wait-task @ task-priority@ swap find-fchan-queue-next
      dup 0= if
	drop over 0 swap fchan-wait-next ! ( wait queue )
	2dup fchan-queue-first @ ( wait queue wait first )
	?dup if
	  2dup swap fchan-wait-prev ! ( wait queue wait first )
	  fchan-wait-next ! ( wait queue )
	else
	  0 swap fchan-wait-prev ! ( wait queue )
	  2dup fchan-queue-last ! ( wait queue )
	then
	fchan-queue-first ! ( )
      else ( wait queue next )
	dup fchan-wait-prev @ ( wait queue next prev )
	dup 4 pick fchan-wait-prev ! ( wait queue next prev )
	?dup if
	  3 pick swap fchan-wait-next ! ( wait queue next )
	else
	  2 pick 2 pick fchan-queue-last ! ( wait queue next )
	then
	dup 3 pick fchan-wait-next ! ( wait queue next )
	nip fchan-wait-prev ! ( )
      then
    ;

    \ Pop a wait from a queue or return null if no queue is available
    : pop-fchan-queue ( queue -- wait|0 )
      dup fchan-queue-first @ dup if ( queue first )
	true over fchan-wait-popped ! ( queue first )
	dup fchan-wait-prev @ ( queue first prev )
	dup 3 pick fchan-queue-first ! ( queue first prev )
	?dup if
	  0 swap fchan-wait-next ! ( queue first )
	  nip ( first )
	else
	  0 rot fchan-queue-last ! ( first )
	then
      else
	nip ( 0 )
      then
    ;

    \ Remove a wait from a queue if it has not already been popped
    : remove-fchan-queue ( wait queue -- )
      over fchan-wait-popped @ not if ( wait queue )
	over fchan-wait-next @ dup if ( wait queue next )
	  2 pick fchan-wait-prev @ ( wait queue next prev )
	  swap fchan-wait-prev ! ( wait queue )
	else
	  drop over fchan-wait-prev @ ( wait queue prev )
	  over fchan-queue-first ! ( wait queue )
	then
	over fchan-wait-prev @ dup if ( wait queue prev )
	  2 pick fchan-wait-next @ ( wait queue prev next )
	  swap fchan-wait-next ! ( wait queue )
	else
	  drop over fchan-wait-next @ ( wait queue next )
	  over fchan-queue-last ! ( wait queue )
	then
      then
      2drop ( )
    ;

    \ Wake all the tasks in a queue
    : wake-all-fchan-queue ( queue -- )
      >r r@ fchan-queue-first @
      begin ?dup while
	dup fchan-wait-task @ ready
	fchan-wait-prev @
      repeat
      0 r@ fchan-queue-first !
      0 r> fchan-queue-last !
    ;

  end-module> import

  \ Rendezvous channel is closed exception
  : x-fchan-closed ( -- ) space ." fchannel is closed" cr ;

  commit-flash
  
  \ Initialize a rendezvous channel
  : init-fchan ( addr -- )
    dup fchan-slock init-slock
    false over fchan-closed !
    dup fchan-send-queue init-fchan-queue
    fchan-recv-queue init-fchan-queue
  ;

  \ Send data on a rendezvous channel
  : send-fchan ( addr bytes fchan -- )
    >r
    r@ fchan-closed @ triggers x-fchan-closed
    r@ fchan-slock claim-slock
    s" BEGIN SEND-FCHAN" trace
    current-task prepare-block
    r@ fchan-recv-queue pop-fchan-queue ?dup if ( addr bytes wait )
      swap over fchan-wait-buf-size @ min ( addr wait bytes )
      >r swap over fchan-wait-buf @ r@ ( wait addr recv-addr bytes ) move
      r> over fchan-wait-buf-size ! ( wait )
      r> fchan-slock release-slock ( wait )
      fchan-wait-task @ ready ( )
    else
      r> fchan-wait-size [: swap >r ( addr bytes wait )
	current-task over fchan-wait-task ! ( addr bytes wait )
	tuck fchan-wait-buf-size ! ( addr wait )
	tuck fchan-wait-buf ! ( wait )
	dup r@ fchan-send-queue push-fchan-queue ( wait )
	r@ fchan-slock release-slock ( wait )
	[: current-task block ;] try ( wait exc )
	?dup if
	  r@ fchan-slock claim-slock ( wait exc )
	  swap r@ fchan-send-queue remove-fchan-queue ( exc )
	  r> fchan-slock release-slock ( exc )
	  ?raise
	else ( wait )
	  drop ( )
	  r> fchan-closed @ triggers x-fchan-closed ( )
	then
      ;] with-aligned-allot
    then
    s" END SEND-FCHAN" trace
  ;

  \ Receive data on a rendezvous channel
  : recv-fchan ( addr bytes fchan -- recv-bytes )
    >r
    r@ fchan-closed @ triggers x-fchan-closed
    r@ fchan-slock claim-slock
    s" BEGIN RECV-FCHAN" trace
    current-task prepare-block
    r@ fchan-send-queue pop-fchan-queue ?dup if ( addr bytes wait )
      swap over fchan-wait-buf-size @ min ( addr wait bytes )
      >r swap over fchan-wait-buf @ swap r@ ( wait send-addr addr bytes ) move
      r> swap ( bytes wait )
      r> fchan-slock release-slock ( bytes wait )
      fchan-wait-task @ ready ( bytes )
    else
      r> fchan-wait-size [: swap >r ( addr bytes wait )
	current-task over fchan-wait-task ! ( addr bytes wait )
	tuck fchan-wait-buf-size ! ( addr wait )
	tuck fchan-wait-buf ! ( wait )
	dup r@ fchan-recv-queue push-fchan-queue ( wait )
	r@ fchan-slock release-slock ( wait )
	[: current-task block ;] try ( wait exc )
	?dup if
	  r@ fchan-slock claim-slock ( wait exc )
	  swap r@ fchan-recv-queue remove-fchan-queue ( exc )
	  r> fchan-slock release-slock ( exc )
	  ?raise
	else ( wait )
	  r> fchan-closed @ triggers x-fchan-closed
	  fchan-wait-buf-size @ ( bytes )
	then
      ;] with-aligned-allot
    then
    s" END RECV-FCHAN" trace
  ;

  \ Close a rendezvous channel
  : close-fchan ( fchan -- )
    [:
      >r
      true r@ fchan-closed !
      r@ fchan-send-queue wake-all-fchan-queue
      r> fchan-recv-queue wake-all-fchan-queue
    ;] over fchan-slock with-slock
  ;

  \ Get whether a rendezvous channel is closed
  : fchan-closed? ( fchan -- closed ) fchan-closed @ ;

  \ Reopen a rendezvous channel
  : reopen-fchan ( fchan -- )
    [: true swap fchan-closed ! ;] over fchan-slock with-slock
  ;

  commit-flash

  \ Export the fchannel size
  export fchan-size

end-module

end-compress-flash

\ Reboot
reboot
