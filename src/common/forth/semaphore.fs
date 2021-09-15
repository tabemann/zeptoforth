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

begin-module-once sema-module

  import task-module
  import tqueue-module

  begin-import-module sema-internal-module

    \ Semaphore header structure
    begin-structure sema-size

      \ The task queue
      tqueue-size +field sema-tqueue

    end-structure

  end-module

  \ Value indicating no counter limit
  no-tqueue-limit constant no-sema-limit
  
  commit-flash

  \ Export the semaphore size
  : sema-size ( -- bytes ) [inlined] sema-size ;

  \ Initialize a semaphore with the given counter limit and initial counter
  : init-sema ( limit counter addr -- ) sema-tqueue init-tqueue-full ;

  \ Take a semaphore, waiting up to a timeout if one is set
  : take ( semaphore -- )
    [: current-task prepare-block sema-tqueue wait-tqueue ;] critical
  ;

  \ Give a semaphore
  : give ( semaphore -- ) [: sema-tqueue wake-tqueue ;] critical ;
  
  \ Reverse give a semaphore
  : ungive ( semaphore -- ) [: sema-tqueue unwake-tqueue ;] critical ;
  
  \ Broadcast a semaphore
  : broadcast ( semaphore -- ) [: sema-tqueue wake-tqueue-all ;] critical ;

end-module

end-compress-flash

\ Reboot
reboot
