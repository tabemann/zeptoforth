\ Copyright (c) 2022 Travis Bemann
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

continue-module forth

  task import

  \ The two tasks
  variable my-task

  \ The mailboxes for the two tasks
  2 cells buffer: my-mailboxes
  
  \ The task being tested
  : test ( -- )
    0 wait-notify drop
    cr ." Starting"
    1 current-task clear-notify
    1 wait-notify drop
    cr ." Notified"
  ;

  \ Initialize the test
  : init-test ( -- )
    0 ['] test 420 128 512 spawn dup my-task !
    my-mailboxes 2 my-task @ config-notify
    my-task @ run
    1 my-task @ notify
    0 my-task @ notify
    20 0 ?do ." *" 100 ms loop
    1 my-task @ notify
  ;
  
end-module