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
forth-wordlist task-wordlist lock-wordlist 3 set-order
forth-wordlist set-current

\ Our lock
lock-size buffer: my-lock

\ Our tasks
variable my-task-0
variable my-task-1
variable my-task-2

\ Create my loop
: make-loop ( u "name" -- )
  <builds , does>
  @ begin
    50000 0 ?do loop
    my-lock lock
    ." ["
    25000 0 ?do loop
    dup . space
    25000 0 ?do loop
    ." ] "
    my-lock unlock
  again
;

\ My loops
0 make-loop loop-0
1 make-loop loop-1
2 make-loop loop-2

\ Initialize the test
: init-test ( -- )
  my-lock init-lock
  0 ['] loop-0 256 256 256 spawn my-task-0 !
  0 ['] loop-1 256 256 256 spawn my-task-1 !
  0 ['] loop-2 256 256 256 spawn my-task-2 !
  begin-critical
  my-task-0 @ run
  my-task-1 @ run
  my-task-2 @ run
  end-critical
;