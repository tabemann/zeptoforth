\ Copyright (c) 2021-2023 Travis Bemann
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
  slock import
  tqueue import
  
  begin-module stream-internal

    \ Stream structure
    begin-structure stream-size

      \ Stream simple lock
      slock-size +field stream-slock
      
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

      \ Stream send task queue
      tqueue-size +field stream-send-tqueue

      \ Stream receive task queue
      tqueue-size +field stream-recv-tqueue

    end-structure

    commit-flash
    
    \ Stream data address
    : stream-data ( stream -- addr ) [inlined] stream-size + ;

  end-module> import

  \ Stream is closed exception
  : x-stream-closed ( -- ) ." stream is closed" cr ;

  \ Attempting to send data larger than the stream exception
  : x-stream-data-too-big ( -- ) ." data is larger than stream" cr  ;

  \ Get whether a stream is full
  : stream-full? ( stream -- flag )
    dup stream-current-count @ swap stream-data-size @ =
  ;

  \ Get whether a stream is empty
  : stream-empty? ( stream -- flag )
    stream-current-count @ 0=
  ;

  \ Get the number of free bytes
  : stream-free ( stream -- bytes )
    dup stream-data-size @ swap stream-current-count @ -
  ;

  commit-flash

  continue-module stream-internal

    \ Wait to send on a stream
    : wait-send-stream { bytes stream -- }
      begin bytes stream stream-free > while
	stream stream-send-tqueue wait-tqueue
	stream stream-closed @ triggers x-stream-closed
      repeat
    ;

    \ Wait to send data as parts on a stream
    : wait-send-stream-parts { stream -- }
      begin stream stream-full? while
	stream stream-send-tqueue wait-tqueue
	stream stream-closed @ triggers x-stream-closed
      repeat
    ;

    \ Wait to receive on a stream
    : wait-recv-stream { stream -- }
      begin stream stream-empty? while
	stream stream-closed @ triggers x-stream-closed
	stream stream-recv-tqueue wait-tqueue
      repeat
    ;

    \ Wait to receive a minimum number of bytes on a stream
    : wait-recv-stream-min { min-bytes stream -- }
      begin min-bytes stream stream-current-count @ > while
	stream stream-closed @ triggers x-stream-closed
	stream stream-recv-tqueue wait-tqueue
      repeat
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
    : write-stream { addr bytes stream -- }
      stream stream-data-size @ stream stream-send-index @ - { first-bytes }
      bytes first-bytes > if
        addr stream stream-data stream stream-send-index @ + first-bytes move
        addr first-bytes + stream stream-data bytes first-bytes - move
      else
        addr stream stream-data stream stream-send-index @ + bytes move
      then
    ;

    \ Read bytes from a stream
    : read-stream { addr bytes stream -- }
      stream stream-data-size @ stream stream-recv-index @ - { first-bytes }
      bytes first-bytes > if
        stream stream-data stream stream-recv-index @ + addr first-bytes move
        stream stream-data addr first-bytes + bytes first-bytes - move
      else
        stream stream-data stream stream-recv-index @ + addr bytes move
      then
    ;

  end-module
  
  \ Initialize a stream with the given data size
  : init-stream ( data-bytes addr -- )
    dup stream-slock init-slock
    tuck stream-data-size !
    0 over stream-recv-index !
    0 over stream-send-index !
    false over stream-closed !
    0 over stream-current-count !
    no-tqueue-limit 0 2 pick dup stream-slock
    swap stream-send-tqueue init-tqueue-full
    no-tqueue-limit 0 rot dup stream-slock
    swap stream-recv-tqueue init-tqueue-full
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
\      dup stream-recv-ready @ 0> if
	stream-recv-tqueue wake-tqueue
\      else
\	drop
\      then
      s" END SEND-STREAM" trace
    ;] over stream-slock with-slock
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
        2dup stream-free min 0 max >r
	2 pick r@ 2 pick write-stream
	r@ over advance-send-stream
	rot r@ + rot r> - rot
        dup stream-recv-tqueue wake-tqueue
      repeat
      2drop drop
      s" END SEND-STREAM-PARTS" trace
    ;] over stream-slock with-slock
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
      stream-send-tqueue wake-tqueue
      s" END RECV-STREAM" trace
    ;] over stream-slock with-slock
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
      stream-send-tqueue wake-tqueue
      s" END RECV-STREAM-MIN" trace
    ;] over stream-slock with-slock
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
    ;] over stream-slock with-slock
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
    ;] over stream-slock with-slock
  ;

  \ Skip data on a stream
  : skip-stream ( bytes stream -- skip-bytes )
    [:
      s" BEGIN SKIP-STREAM" trace
      current-task prepare-block
      dup wait-recv-stream
      dup stream-current-count @ rot min swap
      2dup advance-recv-stream
      stream-send-tqueue wake-tqueue
      s" END SKIP-STREAM" trace
    ;] over stream-slock with-slock
  ;

  \ Skip at least a minimum number of bytes on a stream
  : skip-stream-min ( bytes min-bytes stream -- skip-bytes )
    [:
      s" BEGIN SKIP-STREAM-MIN" trace
      current-task prepare-block
      tuck wait-recv-stream-min
      dup stream-current-count @ rot min swap
      2dup advance-recv-stream
      stream-send-tqueue wake-tqueue
      s" END SKIP-STREAM-MIN" trace
    ;] over stream-slock with-slock
  ;

  \ Send data to a stream without blocking (raise x-would-block if blocking
  \ would normally occur)
  : send-stream-no-block ( addr bytes stream -- )
    [:
      s" BEGIN SEND-STREAM-NO-BLOCK" trace
      dup stream-closed @ triggers x-stream-closed
      dup stream-free 2 pick < triggers x-would-block
      rot 2 pick 2 pick write-stream
      dup >r advance-send-stream r>
      stream-recv-tqueue wake-tqueue
      s" END SEND-STREAM-NO-BLOCK" trace
    ;] over stream-slock with-slock
  ;

  \ Send data to a sream without blocking, partially sending data if blocking
  \ would occur
  : send-stream-partial-no-block ( addr bytes stream -- send-bytes )
    [:
      s" BEGIN SEND-STREAM-PARTIAL-NO-BLOCK" trace
      dup stream-closed @ triggers x-stream-closed
      dup stream-free rot min swap
      rot 2 pick 2 pick write-stream
      2dup advance-send-stream
      stream-recv-tqueue wake-tqueue
      s" END SEND-STREAM-PARTIAL-NO-BLOCK" trace
    ;] over stream-slock with-slock
  ;

  \ Receive data from a stream without blocking (note that no exception is
  \ raised, rather a count of 0 is returned)
  : recv-stream-no-block ( addr bytes stream -- recv-bytes )
    [:
      s" BEGIN RECV-STREAM-NO-BLOCK" trace
      dup stream-current-count @ rot min swap
      rot 2 pick 2 pick read-stream
      2dup advance-recv-stream
      stream-send-tqueue wake-tqueue
      s" END RECV-STREAM-NO-BLOCK" trace
    ;] over stream-slock with-slock
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
        stream-send-tqueue wake-tqueue
      else
	2drop drop 0
      then
      s" END RECV-STREAM-MIN-NO-BLOCK" trace
    ;] over stream-slock with-slock
  ;

  \ Peek data from a stream without blocking (note that no exception is raised,
  \ rather a count of 0 is returned)
  : peek-stream-no-block ( addr bytes stream -- peek-bytes )
    [:
      s" BEGIN PEEK-STREAM-NO-BLOCK" trace
      dup stream-current-count @ rot min swap
      rot 2 pick rot read-stream
      s" END PEEK-STREAM-NO-BLOCK" trace
    ;] over stream-slock with-slock
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
    ;] over stream-slock with-slock
  ;
  
  \ Skip at least a minimum number of bytes on a stream without blocking (note
  \ that no exception is raised, rather a count of 0 is returned)
  : skip-stream-min-no-block ( bytes min-bytes stream -- skip-bytes )
    [:
      s" BEGIN SKIP-STREAM-MIN-NO-BLOCK" trace
      tuck stream-current-count @ <= if
	dup stream-current-count @ rot min swap
	2dup advance-recv-stream
        stream-send-tqueue wake-tqueue
      else
	2drop 0
      then
      s" END SKIP-STREAM-MIN-NO-BLOCK" trace
    ;] over stream-slock with-slock
  ;
  
  \ Close a stream
  : close-stream ( stream -- )
    [:
      true over stream-closed !
      dup stream-send-tqueue wake-tqueue-all
      stream-recv-tqueue wake-tqueue-all
    ;] over stream-slock with-slock
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
