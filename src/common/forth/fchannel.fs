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

begin-module-once fchan-module

  import task-module
  import tqueue-module
  import lock-module

  begin-import-module fchan-internal-module

    \ Fast channel header structure
    begin-structure fchan-size
      \ Fast channel data address
      field: fchan-data-addr

      \ Fast channel data size
      field: fchan-data-size
      
      \ Fast channel is closed
      field: fchan-closed

      \ Fast channel receive lock
      lock-size +field fchan-recv-lock
      
      \ Fast channel send task queue
      tqueue-size +field fchan-send-tqueue

      \ Fast channel receive task queue
      tqueue-size +field fchan-recv-tqueue

      \ Fast channel response task queue
      tqueue-size +field fchan-resp-tqueue
    end-structure

  end-module

  \ Get the fast channel size
  fchan-size constant fchan-size

  \ Fast channel is closed exception
  : x-fchan-closed ( -- ) space ." fchannel is closed" cr ;
  
  commit-flash
  
  \ Initialize a fast channel
  : init-fchan ( addr -- )
    0 over fchan-data-addr !
    0 over fchan-data-size !
    false over fchan-closed !
    dup fchan-recv-lock init-lock
    dup fchan-send-tqueue init-tqueue
    dup fchan-recv-tqueue init-tqueue
    fchan-resp-tqueue init-tqueue
  ;

  \ Send data on a fast channel
  : send-fchan ( addr bytes fchan -- )
    [:
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
    ;] critical
  ;

  \ Receive data on a fast channel
  : recv-fchan ( addr bytes fchan -- addr recv-bytes )
    dup fchan-closed @ triggers x-fchan-closed
    [:
      [:
	dup fchan-send-tqueue wake-tqueue
	[: dup fchan-recv-tqueue wait-tqueue ;] try ?dup if
	  swap fchan-send-tqueue unwake-tqueue ?raise
	then
	dup fchan-closed @ triggers x-fchan-closed
	>r
	2dup 0 fill
	r@ fchan-data-size @ min
	r@ fchan-data-addr @ ?dup if -rot
	  2dup 2>r move 2r>
	else
	  rot drop drop 0
	then
	r> fchan-resp-tqueue wake-tqueue
      ;] critical
    ;] over fchan-recv-lock with-lock
  ;

  commit-flash
  
  \ Send a double cell on a fast channel
  : send-fchan-2cell ( xd fchan -- )
    2 cells [: >r -rot r@ 2! r> 2 cells rot send-fchan ;] with-aligned-allot
  ;

  \ Receive a double cell from a fast channel
  : recv-fchan-2cell ( fchan -- xd )
    2 cells [: 2 cells rot recv-fchan 2 cells >= if 2@ else drop 0 0 then ;]
    with-aligned-allot
  ;

  \ Send a cell on a fast channel
  : send-fchan-cell ( x fchan -- )
    1 cells [: >r swap r@ ! r> 1 cells rot send-fchan ;] with-aligned-allot
  ;

  \ Receive a cell from a fast channel
  : recv-fchan-cell ( fchan -- x )
    1 cells [: 1 cells rot recv-fchan 1 cells >= if @ else drop 0 then ;]
    with-aligned-allot
  ;

  \ Close a fast channel
  : close-fchan ( fchan -- )
    [:
      true over fchan-closed !
      dup fchan-send-tqueue wake-tqueue-all
      fchan-recv-tqueue wake-tqueue-all
    ;] critical
  ;

  \ Get whether a fast channel is closed
  : fchan-closed? ( fchan -- closed ) fchan-closed @ ;

  \ Reopen a fast channel
  : reopen-fchan ( fchan -- ) false swap fchan-closed ! ;

end-module

end-compress-flash

\ Reboot
reboot
