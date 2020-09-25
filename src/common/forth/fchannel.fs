\ Copyright (c) 2020 Travis Bemann
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

\ Set up the wordlist
forth-wordlist 1 set-order
forth-wordlist set-current

\ Check whether this is already defined
defined? fchan-wordlist not [if]

  \ Setup the wordlist
  wordlist constant fchan-wordlist
  wordlist constant fchan-internal-wordlist
  forth-wordlist task-wordlist fchan-internal-wordlist fchan-wordlist
  4 set-order
  fchan-wordlist set-current

  \ Fast channel header structure
  begin-structure fchan-size

    \ Switch the current wordlist
    fchan-internal-wordlist set-current

    \ Fast channel receive task
    field: fchan-recv-task

    \ Fast channel send task
    field: fchan-send-task

    \ Fast channel sent data address
    field: fchan-send-addr

    \ Fast channel sent data count
    field: fchan-send-count
    
  end-structure

  \ Switch the current wordlist
  fchan-wordlist set-current

  \ Initialize an fast channel
  : init-fchan ( addr -- )
    0 over fchan-recv-task !
    0 over fchan-send-task ! 
    0 over fchan-send-addr !
    0 swap fchan-send-count !
  ;

  \ Send data on an fast channel
  : send-fchan ( addr bytes fchan -- )
    0 pause-enabled ! cr ." @A" 1 pause-enabled !
    begin dup fchan-send-task @ 0<> while
      pause
      0 pause-enabled ! cr ." @B" 1 pause-enabled !
    repeat
    0 pause-enabled ! cr ." @C" 1 pause-enabled !
    current-task over fchan-send-task !
    tuck fchan-send-count !
    tuck fchan-send-addr !
    dup fchan-recv-task @ 0<> if
      0 pause-enabled ! cr ." @D" 1 pause-enabled !
      fchan-recv-task @ activate-task
    then
    0 pause-enabled ! cr ." @E" 1 pause-enabled !
    current-task disable-task
    0 pause-enabled ! cr ." @F" 1 pause-enabled !
    pause
    0 pause-enabled ! cr ." @G" 1 pause-enabled !
  ;

  \ Receive data on an fast channel
  : recv-fchan ( fchan -- addr bytes )
    0 pause-enabled ! cr dup h.8 1 pause-enabled !
    0 pause-enabled ! cr ." *A" 1 pause-enabled !
    begin dup fchan-recv-task @ 0<> while
      0 pause-enabled ! cr ." *AA" 1 pause-enabled !
      pause
      0 pause-enabled ! cr ." *B" 1 pause-enabled !
    repeat
    0 pause-enabled ! cr ." *BB" 1 pause-enabled !
    begin dup fchan-send-task @ 0= while
      0 pause-enabled ! cr ." *C" 1 pause-enabled !
      current-task over fchan-recv-task !
      current-task disable-task
      0 pause-enabled ! cr ." *D" 1 pause-enabled !
      pause
      0 pause-enabled ! cr ." *E" 1 pause-enabled !
    repeat
    0 pause-enabled ! cr ." *F" 1 pause-enabled !
    dup fchan-send-task @ enable-task
    0 over fchan-send-task !
    0 over fchan-recv-task !
    dup fchan-send-addr @
    swap fchan-send-count @
    0 pause-enabled ! cr ." *G" 1 pause-enabled !
  ;

  \ Send a cell on an fast channel
  : send-fchan-cell ( x fchan -- ) 0 swap send-fchan ;

  \ Receive a cell from an fast channel
  : recv-fchan-cell ( fchan -- x ) recv-fchan drop ;

[then]
    
