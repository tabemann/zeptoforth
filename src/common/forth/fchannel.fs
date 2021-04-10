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

begin-module-once fchan-module

  import task-module
  import task-internal-module
  import tqueue-module

  begin-import-module fchan-internal-module

    \ Fast channel header structure
    begin-structure fchan-size
      \ Fast channel receive ready
      field: fchan-recv-ready

      \ Fast channel send ready
      field: fchan-send-ready

      \ Fast channel sent data address
      field: fchan-send-addr

      \ Fast channel sent data count
      field: fchan-send-count

      \ Fast channel is closed
      field: fchan-closed
      
      \ Fast channel send task queue
      tqueue-size +field fchan-send-tqueue

      \ Fast channel receive task queue
      tqueue-size +field fchan-recv-tqueue

      \ Fast channel response task queue
      tqueue-size +field fchan-resp-tqueue
    end-structure

  end-module

  \ Export fchan-size
  fchan-size constant fchan-size
  
  \ Initialize an fast channel
  : init-fchan ( addr -- )
    0 over fchan-recv-ready !
    0 over fchan-send-ready !
    0 over fchan-send-addr !
    0 over fchan-send-count !
    false over fchan-closed !
    dup fchan-send-tqueue init-tqueue
    dup fchan-recv-tqueue init-tqueue
    fchan-resp-tqueue init-tqueue
  ;

  \ Fast channel is closed exception
  : x-fchan-closed ( -- ) space ." fchannel is closed" cr ;

  \ Send data on an fast channel
  : send-fchan ( addr bytes fchan -- )
    begin-critical
    dup fchan-closed @ if
      end-critical ['] x-fchan-closed ?raise
    then
    dup fchan-send-ready @ if dup fchan-send-tqueue wait-tqueue then
    dup fchan-closed @ if
      end-critical ['] x-fchan-closed ?raise
    then
    1 over fchan-send-ready +!
    tuck fchan-send-count !
    tuck fchan-send-addr !
    dup fchan-recv-tqueue wake-tqueue
    fchan-resp-tqueue wait-tqueue
    dup fchan-closed @ if
      end-critical ['] x-fchan-closed ?raise
    then
    end-critical
    pause
  ;

  \ Receive data on an fast channel
  : recv-fchan ( fchan -- addr bytes )
    begin-critical
    dup fchan-closed @ if
      end-critical ['] x-fchan-closed ?raise
    then
    1 over fchan-recv-ready +!
    dup fchan-send-ready @ if dup fchan-send-tqueue wake-tqueue then
    dup fchan-recv-tqueue wait-tqueue
    dup fchan-closed @ if
      end-critical ['] x-fchan-closed ?raise
    then
    dup fchan-send-addr @
    over fchan-send-count @
    rot fchan-resp-tqueue wake-tqueue
    -1 over fchan-send-ready +!
    -1 over fchan-recv-ready +!
    end-critical
  ;

  \ Send a cell on an fast channel
  : send-fchan-cell ( x fchan -- ) 0 swap send-fchan ;

  \ Receive a cell from an fast channel
  : recv-fchan-cell ( fchan -- x ) recv-fchan drop ;

  \ Close a fast channel
  : close-fchan ( fchan -- )
    begin-critical
    true over fchan-closed !
    dup fchan-send-tqueue wake-tqueue-all
    dup fchan-recv-tqueue wake-tqueue-all
    fchan-resp-tqueue wake-tqueue-all
    end-critical
  ;

  \ Get whether a fast channel is closed
  : fchan-closed? ( fchan -- closed ) fchan-closed @ ;

  \ Reopen a fast channel
  : reopen-fchan ( fchan -- ) false swap fchan-closed ! ;

end-module
    
\ Warm reboot
warm
