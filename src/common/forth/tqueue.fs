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

\ Set up the wordlist
forth-wordlist 1 set-order
forth-wordlist set-current

\ Make sure task-wordlist exist
defined? task-wordlist not [if]
  :noname space ." task is not installed" cr ; ?raise
[then]

\ Check whether this is already defined
defined? tqueue-wordlist not [if]

  \ Compile to flash
  compile-to-flash

  \ Setup the wordlist
  wordlist constant tqueue-wordlist
  wordlist constant tqueue-internal-wordlist
  forth-wordlist task-wordlist tqueue-internal-wordlist tqueue-wordlist
  4 set-order
  tqueue-wordlist set-current

  \ Task queue header structure
  begin-structure tqueue-size

    \ Switch the current wordlist
    tqueue-internal-wordlist set-current

    \ First fast channel send task queue
    field: first-wait

    \ First fast channel receive task queue
    field: last-wait

    \ Wait counter
    field: wait-counter

  end-structure

  \ Wait structure
  begin-structure wait-size

    \ Fast channel wait next record
    field: wait-next

    \ Fast channel wait task
    field: wait-task

    \ Original here position
    field: wait-orig-here

  end-structure

  \ Set up a wait record
  : init-wait ( -- wait )
    ram-here 4 ram-align, ram-here wait-size ram-allot
    tuck wait-orig-here !
    0 over wait-next !
    current-task over wait-task !
  ;

  \ Add a wait record
  : add-wait ( wait tqueue -- )
    dup first-wait @ if
      2dup last-wait @ wait-next !
    else
      2dup first-wait !
    then
    last-wait !
  ;

  \ Switch wordlists
  tqueue-wordlist set-current

  \ Initialize a task queue
  : init-tqueue ( addr -- )
    0 over wait-counter !
    0 over first-wait !
    0 swap last-wait !
  ;
  
  \ Wait on a task queue
  \ Note that this must be called within a critical section
  : wait-tqueue ( tqueue -- )
    \    begin-critical
    1 over wait-counter +!
    dup wait-counter @ 0 <= if
      drop exit
    then
    init-wait
    dup rot add-wait
    current-task stop
    end-critical
    pause
    begin-critical
    wait-orig-here @ ram-here!
\    end-critical
  ;

  \ Wake up a task in a task quee
  \ Note that this must be called within a critical section
  : wake-tqueue ( tqueue -- )
    \    begin-critical
    -1 over wait-counter +!
    dup first-wait @ if
      dup first-wait @
      dup wait-next @ 2 pick first-wait !
      over first-wait @ 0= if
	0 rot last-wait !
      else
	nip
      then
      wait-task @ activate-task
    else
      drop
    then
\    end-critical
  ;

[then]

\ Warm reboot
warm