\ Copyright (c) 2021 Travis Bemann
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

begin-module stream

  task import
  multicore import
  tqueue import
  
  begin-module stream-internal

    \ Stream structure
    begin-structure stream-size

      \ Stream data size
      field: stream-data-size

      \ Stream receive index
      field: stream-recv-index
      
      \ Stream send index
      field: stream-send-index

      \ Stream is closed
      field: stream-closed

      \ Stream current count
      field: stream-current-count

      \ Stream send ready
      field: stream-send-ready

      \ Stream receive ready
      field: stream-recv-ready

      \ Stream send task queue
      tqueue-size +field stream-send-tqueue

      \ Stream receive task queue
      tqueue-size +field stream-recv-tqueue

    end-structure

    commit-flash
    
    \ Stream data address
    : stream-data ( stream -- addr ) [inlined] stream-size + ;

    \ Core of getting whether a stream is full
    : stream-full-unsafe? ( stream -- flag )
      dup stream-current-count @ swap stream-data-size @ =
    ;

    \ Core of getting whether a stream is empty
    : stream-empty-unsafe? ( stream -- flag )
      stream-current-count @ 0=
    ;

    \ Core of getting the number of free bytes
    : stream-free-unsafe ( stream -- bytes )
      dup stream-data-size @ swap stream-current-count @ -
    ;

  end-module> import

  \ Stream is closed exception
  : x-stream-closed ( -- ) space ." stream is closed" cr ;

  \ Attempting to send data larger than the stream exception
  : x-stream-data-too-big ( -- ) space ." data is larger than stream" cr  ;

  commit-flash

  \ Get whether a stream is full
  : stream-full? ( stream -- flag ) [: stream-full-unsafe? ;] critical ;

  \ Get whether a stream is empty
  : stream-empty? ( stream -- flag ) stream-empty-unsafe? ;
  
  \ Get the number of free bytes
  : stream-free ( stream -- bytes ) [: stream-free-unsafe ;] critical ;
  
  continue-module stream-internal

    \ Wait to send on a stream
    : wait-send-stream ( bytes stream -- )
      begin 2dup stream-free-unsafe > while
	1 over stream-send-ready +!
	dup stream-send-tqueue ['] wait-tqueue try
	-1 2 pick stream-send-ready +!
	?raise
	dup stream-closed @ triggers x-stream-closed
      repeat
      2drop
    ;

    \ Wait to send data as parts on a stream
    : wait-send-stream-parts ( stream -- )
      dup stream-full-unsafe? if
	1 over stream-send-ready +!
	dup stream-send-tqueue ['] wait-tqueue try
	-1 2 pick stream-send-ready +!
	?raise
	stream-closed @ triggers x-stream-closed
      else
	drop
      then
    ;

    \ Wait to receive on a stream
    : wait-recv-stream ( stream -- )
      dup stream-empty-unsafe? if
	dup stream-closed @ triggers x-stream-closed
	1 over stream-recv-ready +!
	dup stream-recv-tqueue ['] wait-tqueue try
	-1 rot stream-recv-ready +!
	?raise
      else
	drop
      then
    ;

    \ Wait to receive a minimum number of bytes on a stream
    : wait-recv-stream-min ( min-bytes stream -- )
      begin 2dup stream-current-count @ > while
	dup stream-closed @ triggers x-stream-closed
	1 over stream-recv-ready +!
	dup stream-recv-tqueue ['] wait-tqueue try
	-1 2 pick stream-recv-ready +!
	?raise
      repeat
      2drop
    ;
    
    \ Get the stream send address
    : send-stream-addr ( stream -- b-addr )
      dup stream-send-index @ stream-size + +
    ;

    \ Get the stream receive address
    : recv-stream-addr ( stream -- b-addr )
      dup stream-recv-index @ stream-size + +
    ;

    \ Advance the stream send index
    : advance-send-stream ( count stream -- )
      2dup stream-current-count +!
      dup stream-send-index @ rot + over stream-data-size @ umod
      swap stream-send-index !
    ;

    \ Advance the stream receive index
    : advance-recv-stream ( count stream -- )
      2dup swap negate swap stream-current-count +!
      dup stream-recv-index @ rot + over stream-data-size @ umod
      swap stream-recv-index !
    ;

    commit-flash

    \ Write bytes to a stream
    : write-stream ( addr bytes stream -- )
      dup stream-send-index @ 2 pick + over stream-data-size @ > if
	dup stream-data-size @ over stream-send-index @ - >r
	2 pick over send-stream-addr r@ move
	rot r@ + rot r> - rot stream-size + swap move
      else
	send-stream-addr swap move
      then
    ;

    \ Read bytes from a stream
    : read-stream ( addr bytes stream -- )
      dup stream-recv-index @ 2 pick + over stream-data-size @ > if
	dup stream-data-size @ over stream-recv-index @ - >r
	dup recv-stream-addr 3 pick r@ move
	stream-size + rot r@ + rot r> - move
      else
	recv-stream-addr -rot move
      then
    ;

  end-module
  
  \ Initialize a stream with the given data size
  : init-stream ( data-bytes addr -- )
    tuck stream-data-size !
    0 over stream-recv-index !
    0 over stream-send-index !
    false over stream-closed !
    0 over stream-current-count !
    0 over stream-send-ready !
    0 over stream-recv-ready !
    dup stream-send-tqueue init-tqueue
    stream-recv-tqueue init-tqueue
  ;

  commit-flash

  \ Send data to a stream
  : send-stream ( addr bytes stream -- )
    2dup stream-data-size @ > triggers x-stream-data-too-big
    [:
      s" BEGIN SEND-STREAM" trace
      dup stream-closed @ triggers x-stream-closed
      current-task prepare-block
      2dup wait-send-stream
      rot 2 pick 2 pick write-stream
      dup >r advance-send-stream r>
      dup stream-recv-ready @ 0> if
	stream-recv-tqueue wake-tqueue
      else
	drop
      then
      s" END SEND-STREAM" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Send data to a stream in parts (if the data is bigger than the available
  \ space, what data can be sent is sent, and the remaining data is sent
  \ later); note that if a timeout occurs, the data may be left partially sent
  : send-stream-parts ( addr bytes stream -- )
    [:
      s" BEGIN SEND-STREAM-PARTS" trace
      dup stream-closed @ triggers x-stream-closed
      current-task prepare-block
      begin over 0> while
	dup wait-send-stream-parts
	dup stream-free-unsafe >r
	2 pick 2 pick r@ min 2 pick write-stream
	2dup swap r@ min swap advance-send-stream
	rot r@ + rot r> - rot
	dup stream-recv-ready @ 0> if
	  dup stream-recv-tqueue wake-tqueue
	then
      repeat
      2drop drop
      s" END SEND-STREAM-PARTS" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Receive data from a stream
  : recv-stream ( addr bytes stream -- recv-bytes )
    [:
      s" BEGIN RECV-STREAM" trace
      current-task prepare-block
      dup wait-recv-stream
      dup stream-current-count @ rot min swap
      rot 2 pick 2 pick read-stream
      2dup advance-recv-stream
      dup stream-send-ready @ 0> if
	stream-send-tqueue wake-tqueue
      else
	drop
      then
      s" END RECV-STREAM" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;
  
  \ Receive data at least a minimum number of bytes from a stream
  : recv-stream-min ( addr bytes min-bytes stream -- recv-bytes )
    [:
      s" BEGIN RECV-STREAM-MIN" trace
      current-task prepare-block
      tuck wait-recv-stream-min
      dup stream-current-count @ rot min swap
      rot 2 pick 2 pick read-stream
      2dup advance-recv-stream
      dup stream-send-ready @ 0> if
	stream-send-tqueue wake-tqueue
      else
	drop
      then
      s" END RECV-STREAM-MIN" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Peek data from a stream
  : peek-stream ( addr bytes stream -- peek-bytes )
    [:
      s" BEGIN PEEK-STREAM" trace
      current-task prepare-block
      dup wait-recv-stream
      dup stream-current-count @ rot min swap
      rot 2 pick rot read-stream
      s" END PEEK-STREAM" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Peek data at least a minimum number of bytes from a stream
  : peek-stream-min ( addr bytes min-bytes stream -- peek-bytes )
    [:
      s" BEGIN PEEK-STREAM-MIN" trace
      current-task prepare-block
      tuck wait-recv-stream-min
      dup stream-current-count @ rot min swap
      rot 2 pick rot read-stream
      s" END PEEK-STREAM-MIN" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Skip data on a stream
  : skip-stream ( bytes stream -- skip-bytes )
    [:
      s" BEGIN SKIP-STREAM" trace
      current-task prepare-block
      dup wait-recv-stream
      dup stream-current-count @ rot min swap
      2dup advance-recv-stream
      dup stream-send-ready @ 0> if
	stream-send-tqueue wake-tqueue
      else
	drop
      then
      s" END SKIP-STREAM" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Skip at least a minimum number of bytes on a stream
  : skip-stream-min ( bytes min-bytes stream -- skip-bytes )
    [:
      s" BEGIN SKIP-STREAM-MIN" trace
      current-task prepare-block
      tuck wait-recv-stream-min
      dup stream-current-count @ rot min swap
      2dup advance-recv-stream
      dup stream-send-ready @ 0> if
	stream-send-tqueue wake-tqueue
      else
	drop
      then
      s" END SKIP-STREAM-MIN" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Send data to a stream without blocking (raise x-would-block if blocking
  \ would normally occur)
  : send-stream-no-block ( addr bytes stream -- )
    [:
      s" BEGIN SEND-STREAM-NO-BLOCK" trace
      dup stream-closed @ triggers x-stream-closed
      dup stream-free-unsafe 2 pick < triggers x-would-block
      rot 2 pick 2 pick write-stream
      dup >r advance-send-stream r>
      dup stream-recv-ready @ 0> if
	stream-recv-tqueue wake-tqueue
      else
	drop
      then
      s" END SEND-STREAM-NO-BLOCK" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Send data to a sream without blocking, partially sending data if blocking
  \ would occur
  : send-stream-partial-no-block ( addr bytes stream -- send-bytes )
    [:
      s" BEGIN SEND-STREAM-PARTIAL-NO-BLOCK" trace
      dup stream-closed @ triggers x-stream-closed
      dup stream-free-unsafe rot min swap
      rot 2 pick 2 pick write-stream
      2dup advance-send-stream
      dup stream-recv-ready @ 0> if
	stream-recv-tqueue wake-tqueue
      else
	drop
      then
      s" END SEND-STREAM-PARTIAL-NO-BLOCK" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Receive data from a stream without blocking (note that no exception is
  \ raised, rather a count of 0 is returned)
  : recv-stream-no-block ( addr bytes stream -- recv-bytes )
    [:
      s" BEGIN RECV-STREAM-NO-BLOCK" trace
      dup stream-current-count @ rot min swap
      rot 2 pick 2 pick read-stream
      2dup advance-recv-stream
      dup stream-send-ready @ 0> if
	stream-send-tqueue wake-tqueue
      else
	drop
      then
      s" END RECV-STREAM-NO-BLOCK" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Receive at least a minimum number of bytes from a stream without blocking
  \ (note that no exception is raised, rather a count of 0 is returned)
  : recv-stream-min-no-block ( addr bytes min-bytes stream -- recv-bytes )
    [:
      s" BEGIN RECV-STREAM-MIN-NO-BLOCK" trace
      tuck stream-current-count @ <= if
	dup stream-current-count @ rot min swap
	rot 2 pick 2 pick read-stream
	2dup advance-recv-stream
	dup stream-send-ready @ 0> if
	  stream-send-tqueue wake-tqueue
	else
	  drop
	then
      else
	2drop drop 0
      then
      s" END RECV-STREAM-MIN-NO-BLOCK" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Peek data from a stream without blocking (note that no exception is raised,
  \ rather a count of 0 is returned)
  : peek-stream-no-block ( addr bytes stream -- peek-bytes )
    [:
      s" BEGIN PEEK-STREAM-NO-BLOCK" trace
      dup stream-current-count @ rot min swap
      rot 2 pick rot read-stream
      s" END PEEK-STREAM-NO-BLOCK" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Peek at least a minimum number of bytes from a stream without blocking
  \ (note that no exception is raised, rather a count of 0 is returned)
  : peek-stream-min-no-block ( addr bytes min-bytes stream -- peek-bytes )
    [:
      s" BEGIN PEEK-STREAM-MIN-NO-BLOCK" trace
      tuck stream-current-count @ <= if
	dup stream-current-count @ rot min swap
	rot 2 pick rot read-stream
      else
	2drop drop 0
      then
      s" END PEEK-STREAM-MIN-NO-BLOCK" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;
  
  \ Skip at least a minimum number of bytes on a stream without blocking (note
  \ that no exception is raised, rather a count of 0 is returned)
  : skip-stream-min-no-block ( bytes min-bytes stream -- skip-bytes )
    [:
      s" BEGIN SKIP-STREAM-MIN-NO-BLOCK" trace
      tuck stream-current-count @ <= if
	dup stream-current-count @ rot min swap
	2dup advance-recv-stream
	dup stream-send-ready @ 0> if
	  stream-send-tqueue wake-tqueue
	else
	  drop
	then
      else
	2drop 0
      then
      s" END SKIP-STREAM-MIN-NO-BLOCK" trace
    ;] tqueue-spinlock critical-with-spinlock
  ;
  
  \ Close a stream
  : close-stream ( stream -- )
    [:
      true over stream-closed !
      dup stream-send-tqueue wake-tqueue-all
      stream-recv-tqueue wake-tqueue-all
    ;] tqueue-spinlock critical-with-spinlock
  ;

  \ Get whether a stream is closed
  : stream-closed? ( stream -- closed ) stream-closed @ ;

  \ Reopen a stream
  : reopen-stream ( stream -- ) false swap stream-closed ! ;

  \ Get the size of a stream for a given data size
  : stream-size ( data-bytes -- bytes ) [inlined] stream-size + ;

  \ Get the stream data size
  : stream-data-size ( stream -- data-bytes ) stream-data-size @ ;

end-module

end-compress-flash

\ Reboot
reboot
