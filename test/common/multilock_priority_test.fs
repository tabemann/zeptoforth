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

begin-module forth-module

  import task-module
  import lock-module

  \ Our locks
  lock-size buffer: lock-a
  lock-size buffer: lock-b
  lock-size buffer: lock-c

  \ Create a locking task word
  : locker ( priority ms lock "name" -- )
    <builds , , , does>
    dup @ swap cell + dup @ ms cell + @ current-task set-task-priority
    dup lock unlock begin 1000 ms again
    current-task kill
  ;

  \ Monitor priority
  : monitor-priority ( tests interval -- )
    swap 0 ?do
      dup ms
      current-task get-task-priority .
    loop
    drop
  ;

  \ Our main locking task word
  : do-lock-all ( -- )
    lock-a lock
    lock-b lock
    lock-c lock
    40 100 monitor-priority space ." A"
    lock-a unlock
    20 100 monitor-priority space ." B"
    lock-b unlock
    20 100 monitor-priority space ." C"
    lock-c unlock
    20 100 monitor-priority space ." D"
    current-task kill
  ;

  \ Create our locking task words
  1 1000 lock-a locker do-lock-a
  2 2000 lock-b locker do-lock-b
  3 3000 lock-c locker do-lock-c

  \ Our tasks
  variable task-lock-all
  variable task-lock-a
  variable task-lock-b
  variable task-lock-c

  \ Initialize our test
  : init-test ( -- )
    lock-a init-lock
    lock-b init-lock
    lock-c init-lock
    0 ['] do-lock-all 512 256 256 spawn task-lock-all !
    0 ['] do-lock-a 512 256 256 spawn task-lock-a !
    0 ['] do-lock-b 512 256 256 spawn task-lock-b !
    0 ['] do-lock-c 512 256 256 spawn task-lock-c !
    begin-critical
    task-lock-all @ run
    task-lock-a @ run
    task-lock-b @ run
    task-lock-c @ run
    end-critical
  ;

end-module
