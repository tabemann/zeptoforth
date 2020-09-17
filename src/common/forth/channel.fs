\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

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
  
  \ Wake task waiting to send
  : wake-send-chan ( chan -- )
    chan-send-task @ ?dup if enable-task then pause
  ;

  \ Wake task waiting to receive
  : wake-recv-chan ( chan -- )
    chan-recv-task @ ?dup if enable-task then pause
  ;

  \ Wait to send on a channel
  : wait-send-chan ( chan -- )
    begin
      dup chan-send-task @ 0<> over chan-send-task @ current-task <> and
    while
      pause
    repeat
    begin dup chan-full? while
      current-task over chan-send-task !
      current-task disable-task
      dup wake-recv-chan
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
      current-task disable-task
      dup wake-send-chan
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