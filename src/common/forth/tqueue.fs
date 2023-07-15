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
    : find-wait-queue-next ( priority queue -- wait|0 )
      tqueue-last @ ( priority current )
      begin dup while ( priority current )
	dup wait-task @ task-priority@
	( priority current current-priority )
	2 pick >= if nip ( current ) exit then
	wait-next @ ( priority next )
      repeat
      nip ( 0 )
    ;

    \ Insert a wait into a queue
    : push-wait-queue { wait queue -- }
      false wait wait-popped !
      wait wait-task @ task-priority@ queue find-wait-queue-next { next-wait }
      next-wait 0= if
        0 wait wait-next !
        queue tqueue-first @ { first-wait }
        first-wait if
          first-wait wait wait-prev !
          wait first-wait wait-next !
        else
          0 wait wait-prev !
          wait queue tqueue-last !
        then
        wait queue tqueue-first !
      else
        next-wait wait-prev @ { prev-wait }
        prev-wait wait wait-prev !
        prev-wait if
          wait prev-wait wait-next !
        then
        wait next-wait wait-prev !
        next-wait wait wait-next !
      then
    ;
      
    \ Pop a wait from a queue or return null if no queue is available
    : pop-wait-queue { queue -- wait|0 }
      queue tqueue-first @ { first-wait }
      first-wait if
        true first-wait wait-popped !
        first-wait wait-prev @ { prev-wait }
        prev-wait if
          0 prev-wait wait-next !
        else
          0 queue tqueue-last !
        then
        prev-wait queue tqueue-first !
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
          prev-wait queue tqueue-first !
        then
        prev-wait if
          next-wait prev-wait wait-next !
        else
          next-wait queue tqueue-last !
        then
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
    s" BEGIN WAIT-TQUEUE" trace
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
      swap tuck remove-wait-queue
      current-task timed-out? if
        dup tqueue-limit @ 0> if
          dup tqueue-counter @ 1+ over tqueue-limit @ min swap tqueue-counter !
        else
          1 swap tqueue-counter +!
        then
	['] x-timed-out ?raise
      else
        drop
      then
    ;] with-aligned-allot
    s" END WAIT-TQUEUE" trace
  ;

  \ Wake up a task in a task queue
  \ Note that this must be called within a critical section
  : wake-tqueue ( tqueue -- )
    s" BEGIN WAKE-TQUEUE" trace
    dup tqueue-limit @ 0> if
      dup tqueue-counter @ 1+ over tqueue-limit @ min over tqueue-counter !
    else
      1 over tqueue-counter +!
    then
    pop-wait-queue ?dup if
      wait-task @ ready
    then
    s" END WAKE-TQUEUE" trace
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
