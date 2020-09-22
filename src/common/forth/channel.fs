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

\ Check whether this is already defined
defined? chan-wordlist not [if]

  \ Set up the wordlist
  forth-wordlist 1 set-order
  forth-wordlist set-current
  wordlist constant chan-wordlist
  wordlist constant chan-internal-wordlist
  forth-wordlist task-wordlist chan-internal-wordlist chan-wordlist
  4 set-order
  chan-internal-wordlist set-current

  \ Channel header structure
  begin-structure chan-header-size
    
    \ Channel byte count
    field: chan-count

    \ Channel receive index
    field: chan-recv-index

    \ Channel send index
    field: chan-send-index

    \ Channel receive task
    field: chan-recv-task

    \ Channel send task
    field: chan-send-task

  end-structure

  \ Define public words
  chan-wordlist set-current

  \ Get whether a channel is full
  : chan-full? ( chan -- flag )
    dup chan-send-index @ 1+ over chan-count @ umod
    swap chan-recv-index @ =
  ;

  \ Get whether a channel is empty
  : chan-empty? ( chan -- flag )
    dup chan-send-index @ swap chan-recv-index @ =
  ;

  \ Define internal words
  chan-internal-wordlist set-current
  
  \ Wait to send on a channel
  : wait-send-chan ( chan -- )
    begin
      dup chan-send-task @ 0<> over chan-send-task @ current-task <> and
    while
      pause
    repeat
    begin dup chan-full? while
      current-task over chan-send-task !
\      begin dup chan-recv-task @ 0= while pause repeat
      dup chan-recv-task @ ?dup if enable-task then
      current-task disable-task
      pause
    repeat
    0 swap chan-send-task !
  ;

  \ Wait to receive on a channel
  : wait-recv-chan ( chan -- )
    begin
      dup chan-recv-task @ 0<> over chan-recv-task @ current-task <> and
    while
      pause
    repeat
    begin dup chan-empty? while
      current-task over chan-recv-task !
      \      begin dup chan-send-task @ 0= while pause repeat

      \ 0 pause-enabled !
      \ flush-console
      \ 1 pause-enabled !
      
      dup chan-send-task @ ?dup if enable-task then
      current-task disable-task
      pause
    repeat
    0 swap chan-recv-task !
  ;

  \ Get the channel send address
  : send-chan-addr ( chan -- b-addr )
    dup chan-send-index @ chan-header-size + +
  ;

  \ Get the channel receive address
  : recv-chan-addr ( chan -- b-addr )
    dup chan-recv-index @ chan-header-size + +
  ;

  \ Advance the channel send index
  : advance-send-chan ( chan -- )
    dup chan-send-index @ 1+ over chan-count @ umod swap chan-send-index !
  ;

  \ Advance the channel receive index
  : advance-recv-chan ( chan -- )
    dup chan-recv-index @ 1+ over chan-count @ umod swap chan-recv-index !
  ;

  \ Define public words
  chan-wordlist set-current
  
  \ Get channel size for a channel with a specified buffer size in bytes
  : chan-size ( bytes -- ) 4 align chan-header-size + ;

  \ Initialize a channel for a channel with a specified buffer size in bytes
  : init-chan ( addr bytes -- )
    over chan-count !
    0 over chan-recv-index !
    0 over chan-send-index !
    0 over chan-recv-task !
    0 swap chan-send-task !
  ;

  \ Send a byte to a channel
  : send-chan-byte ( b chan -- )
    dup wait-send-chan
    tuck send-chan-addr b!
    advance-send-chan
  ;

  \ Receive a byte from a channel
  : recv-chan-byte ( chan -- b )
    dup wait-recv-chan
    dup recv-chan-addr b@
    swap advance-recv-chan
  ;

  \ Send bytes to a channel
  : send-chan ( addr bytes chan -- )
    >r begin dup 0> while
      swap dup b@ r@ send-chan-byte 1+ swap 1-
    repeat
    2drop rdrop
  ;

  \ Receive bytes from a channel
  : recv-chan ( addr bytes chan -- )
    >r begin dup 0> while
      r@ recv-chan-byte 2 pick b! 1- swap 1+ swap
    repeat
    2drop rdrop
  ;

  \ Send a cell to a channel
  : send-chan-cell ( x chan -- )
    swap pad ! pad 4 rot send-chan
  ;

  \ Receive a cell from a channel
  : recv-chan-cell ( chan -- x )
    pad 4 rot recv-chan pad @
  ;
  
[then]
