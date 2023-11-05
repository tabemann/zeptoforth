\ Copyright (c) 2020-2023 Travis Bemann
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

begin-module tqueue

  task import
  slock import

  begin-module tqueue-internal

    \ Task queue header structure
    begin-structure tqueue-size

      \ A simple lock used by the task quee
      field: tqueue-slock

      \ First wait in task queue channel queue
      field: tqueue-first

      \ Last wait in task queue channel queue
      field: tqueue-last
      
      \ Wait counter
      field: tqueue-counter
      
      \ Wait limit
      field: tqueue-limit
      
    end-structure

    \ Task queue wait structure
    begin-structure wait-size

      \ Waiting task
      field: wait-task

      \ Previous entry in queue
      field: wait-prev

      \ Next entry in queue
      field: wait-next

      \ Popped flag
      field: wait-popped

    end-structure

    commit-flash

    \ Get last higher priority wait in queue
    : find-wait-queue-prev { priority queue -- wait|0 }
      queue tqueue-last @ { current }
      begin current while
        current wait-task @ task-priority@ priority < if
          current wait-prev @ to current
        else
          current exit
        then
      repeat
      0
    ;

    \ Insert a wait into a queue
    : push-wait-queue { wait queue -- }
      false wait wait-popped !
      wait wait-task @ task-priority@ queue find-wait-queue-prev { prev-wait }
      prev-wait 0= if
        0 wait wait-prev !
        queue tqueue-first @ { first-wait }
        first-wait if
          wait first-wait wait-prev !
        else
          wait queue tqueue-last !
        then
        first-wait wait wait-next !
        wait queue tqueue-first !
      else
        prev-wait wait-next @ { next-wait }
        next-wait wait wait-next !
        next-wait if
          wait next-wait wait-prev !
        else
          wait queue tqueue-last !
        then
        wait prev-wait wait-next !
        prev-wait wait wait-prev !
      then
    ;
      
    \ Pop a wait from a queue or return null if no queue is available
    : pop-wait-queue { queue -- wait|0 }
      queue tqueue-first @ { first-wait }
      first-wait if
        true first-wait wait-popped !
        first-wait wait-next @ { next-wait }
        next-wait if
          0 next-wait wait-prev !
        else
          0 queue tqueue-last !
        then
        next-wait queue tqueue-first !
        first-wait
      else
        0
      then
    ;

    \ Remove a wait from a queue if it has not already been popped
    : remove-wait-queue { wait queue -- }
      wait wait-popped @ not if
        true wait wait-popped !
        wait wait-next @ wait wait-prev @ { next-wait prev-wait }
        next-wait if
          prev-wait next-wait wait-prev !
        else
          prev-wait queue tqueue-last !
        then
        prev-wait if
          next-wait prev-wait wait-next !
        else
          next-wait queue tqueue-first !
        then
      then
    ;

    \ Increment counter
    : +tqueue-counter ( tqueue -- )
      dup tqueue-limit @ 0>= if
        dup tqueue-counter @ 1+ over tqueue-limit @ min swap tqueue-counter !
      else
        1 swap tqueue-counter +!
      then
    ;

  end-module> import

  \ Value indicating no task queue limti
  -1 constant no-tqueue-limit
  
  \ Initialize a task queue with a given simple lock
  : init-tqueue ( slock addr -- )
    tuck tqueue-slock !
    0 over tqueue-counter !
    -1 over tqueue-limit !
    0 over tqueue-first !
    0 swap tqueue-last !
  ;

  \ Initialize a task queue with a simple loc, a limit, and a initial counter
  : init-tqueue-full ( limit counter slock addr -- )
    tuck tqueue-slock !
    tuck tqueue-counter !
    tuck tqueue-limit !
    0 over tqueue-first !
    0 swap tqueue-last !
  ;

  commit-flash
  
  \ Wait on a task queue
  \ Note that this must be called within a critical section
  : wait-tqueue ( tqueue -- )
    -1 over tqueue-counter +!
    dup tqueue-counter @ 0>= if
      drop exit
    then
    wait-size [:
      current-task over wait-task !
      false over wait-popped !
      2dup swap push-wait-queue
      over tqueue-slock @ release-slock-block
      over tqueue-slock @ claim-slock
      dup wait-popped @ not if over +tqueue-counter then
      swap remove-wait-queue
      current-task validate-timeout
    ;] with-aligned-allot
  ;

  \ Wake up a task in a task queue
  \ Note that this must be called within a critical section
  : wake-tqueue ( tqueue -- )
    dup tqueue-counter @ 0< if
      dup pop-wait-queue ?dup if
        wait-task @ ready
      then
    then
    +tqueue-counter
  ;

  \ Un-wake a task queue
  : unwake-tqueue ( tqueue -- )
    dup tqueue-counter @ dup 0> if
      1- 0 max swap tqueue-counter !
    else
      2drop
    then
  ;

  commit-flash

  \ Wake up all tasks in a task queue
  : wake-tqueue-all ( tqueue -- )
    begin dup tqueue-counter @ 0< while
      dup wake-tqueue
    repeat
    drop
  ;
  
  \ Export tqueue-size
  ' tqueue-size export tqueue-size

end-module

end-compress-flash

\ Reboot
reboot
