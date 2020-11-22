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

\ Make sure tqueue-wordlist exists
defined? tqueue-wordlist not [if]
  :noname space ." tqueue is not installed" cr ; ?raise
[then]

\ Check whether this is already defined
defined? fchan-wordlist not [if]

  \ Setup the wordlist
  wordlist constant fchan-wordlist
  wordlist constant fchan-internal-wordlist
  forth-wordlist task-internal-wordlist task-wordlist tqueue-wordlist
  fchan-internal-wordlist fchan-wordlist
  6 set-order
  fchan-wordlist set-current

  \ Fast channel header structure
  begin-structure fchan-size

    \ Switch the current wordlist
    fchan-internal-wordlist set-current

    \ Fast channel receive ready
    field: fchan-recv-ready

    \ Fast channel send ready
    field: fchan-send-ready

    \ Fast channel sent data address
    field: fchan-send-addr

    \ Fast channel sent data count
    field: fchan-send-count

    \ Fast channel send task queue
    tqueue-size +field fchan-send-tqueue

    \ Fast channel receive task queue
    tqueue-size +field fchan-recv-tqueue

    \ Fast channel response task queue
    tqueue-size +field fchan-resp-tqueue

  end-structure
  
  \ Switch the current wordlist
  fchan-wordlist set-current

  \ Initialize an fast channel
  : init-fchan ( addr -- )
    0 over fchan-recv-ready !
    0 over fchan-send-ready !
    0 over fchan-send-addr !
    0 over fchan-send-count !
    dup fchan-send-tqueue init-tqueue
    dup fchan-recv-tqueue init-tqueue
    fchan-resp-tqueue init-tqueue
  ;

  \ Send data on an fast channel
  : send-fchan ( addr bytes fchan -- )
    begin-critical
    dup fchan-send-ready @ if dup fchan-send-tqueue wait-tqueue then
    1 over fchan-send-ready +!
    tuck fchan-send-count !
    tuck fchan-send-addr !
    dup fchan-recv-ready @ if dup fchan-recv-tqueue wake-tqueue then
    fchan-resp-tqueue wait-tqueue
    end-critical
    pause
  ;

  \ Receive data on an fast channel
  : recv-fchan ( fchan -- addr bytes )
    begin-critical
    1 over fchan-recv-ready +!
    dup fchan-send-ready @ if dup fchan-send-tqueue wake-tqueue then
    dup fchan-recv-tqueue wait-tqueue
    -1 over fchan-send-ready +!
    -1 over fchan-recv-ready +!
    dup fchan-send-addr @
    over fchan-send-count @
    rot fchan-resp-tqueue wake-tqueue
    end-critical
  ;

  \ Send a cell on an fast channel
  : send-fchan-cell ( x fchan -- ) 0 swap send-fchan ;

  \ Receive a cell from an fast channel
  : recv-fchan-cell ( fchan -- x ) recv-fchan drop ;

[then]
    
