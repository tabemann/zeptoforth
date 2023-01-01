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

begin-module sema

  task import
  slock import
  tqueue import

  begin-module sema-internal

    \ Semaphore header structure
    begin-structure sema-size

      \ The simple lock
      slock-size +field sema-slock

      \ The task queue
      tqueue-size +field sema-tqueue

    end-structure

  end-module> import

  \ Value indicating no counter limit
  no-tqueue-limit constant no-sema-limit
  
  commit-flash

  \ Initialize a semaphore with the given counter limit and initial counter
  : init-sema ( limit counter addr -- )
    dup sema-slock init-slock
    dup sema-slock swap sema-tqueue init-tqueue-full
  ;

  \ Take a semaphore, waiting up to a timeout if one is set
  : take ( semaphore -- )
    [:
      current-task prepare-block sema-tqueue wait-tqueue
    ;] over sema-slock with-slock
  ;

  \ Give a semaphore
  : give ( semaphore -- )
    [:
      sema-tqueue wake-tqueue
    ;] over sema-slock with-slock
  ;
  
  \ Reverse give a semaphore
  : ungive ( semaphore -- )
    [:
      sema-tqueue unwake-tqueue
    ;] over sema-slock with-slock
  ;
  
  \ Broadcast a semaphore
  : broadcast ( semaphore -- )
    [:
      sema-tqueue wake-tqueue-all
    ;] over sema-slock with-slock
  ;

  \ Export the semaphore size
  ' sema-size export sema-size

end-module

end-compress-flash

\ Reboot
reboot
