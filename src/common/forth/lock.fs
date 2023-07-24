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

begin-module lock

  task import
  slock import

  begin-module lock-internal

    \ Lock header structure
    begin-structure lock-size

      \ Lock simple lock
      slock-size +field lock-slock
      
      \ Lock holder task
      field: lock-holder-task
      
      \ First lock wait
      field: lock-first-wait
      
      \ Last lock wait
      field: lock-last-wait
      
      \ Lock nesting level
      field: lock-nest-level
      
    end-structure
    
    \ Lock wait structure
    begin-structure lock-wait-size

      \ In list
      field: lock-wait-in-list
      
      \ Lock wait previous record
      field: lock-wait-prev
      
      \ Lock wait next record
      field: lock-wait-next
      
      \ Lock wait task
      field: lock-wait-task
      
      \ Original here position
      field: lock-wait-orig-here

    end-structure

    commit-flash
    
    \ Set up a lock wait record
    : init-lock-wait ( -- wait )
      ram-here 4 ram-align, ram-here lock-wait-size ram-allot
      tuck lock-wait-orig-here !
      0 over lock-wait-prev !
      0 over lock-wait-next !
      current-task over lock-wait-task !
      false over lock-wait-in-list !
    ;

    \ Get maximum lock wait priority
    : max-lock-wait-priority ( lock -- priority )
      -32768 swap lock-first-wait @ begin dup while
	dup lock-wait-task @ task-priority@ rot max swap lock-wait-next @
      repeat
      drop
    ;

    \ Add a lock wait record
    : add-lock-wait { wait lock -- }
      0 wait lock-wait-next !
      lock lock-last-wait @ { last-wait }
      last-wait wait lock-wait-prev !
      wait lock lock-last-wait !
      last-wait 0= if wait lock lock-first-wait ! then
      true wait lock-wait-in-list !
    ;

    \ Remove a lock wait record
    : remove-lock-wait { wait lock -- }
      wait lock-wait-in-list @ if
        wait lock-wait-prev @ wait lock-wait-next @ { prev-wait next-wait }
        next-wait if
          prev-wait next-wait lock-wait-prev !
        else
          prev-wait lock lock-last-wait !
        then
        prev-wait if
          next-wait prev-wait lock-wait-next !
        else
          next-wait lock lock-first-wait !
        then
        false wait lock-wait-in-list !
      then
    ;

    \ Adjust the priority of a task
    : adjust-priority { priority task -- }
      task task-priority@ priority <= if
        task task-saved-priority@ task task-priority!
      else
        task task-priority@ task task-saved-priority!
        priority task task-priority!
      then
    ;
    
    commit-flash
    
    \ Update the priority of the tasks holding a lock
    : update-hold-priority { lock -- }
      current-task task-priority@ current-task task-saved-priority@ min
      { priority }
      lock lock-first-wait @ { current }
      begin current while
        current lock-wait-task @
        dup task-priority@ swap task-saved-priority@ min
        priority max to priority
        current lock-wait-next @ to current
      repeat
      priority current-task adjust-priority
      lock lock-first-wait @ to current
      begin current while
        priority current lock-wait-task @ adjust-priority
        current lock-wait-next @ to current
      repeat
    ;

    \ Restore a task's priority
    : restore-priority ( task -- )
      dup task-saved-priority@ swap task-priority!
    ;
    
  end-module> import

  \ Initialize a lock
  : init-lock ( addr -- )
    dup lock-slock init-slock
    0 over lock-holder-task !
    0 over lock-first-wait !
    0 over lock-last-wait !
    0 swap lock-nest-level !
  ;

  \ Attempted to unlock a lock not owned by the current task
  : x-not-currently-owned ( -- ) ." lock not owned by current task" cr ;

  commit-flash
  
  \ Lock a lock
  : claim-lock ( lock -- )
    [:
      s" BEGIN CLAIM LOCK" trace
      dup lock-holder-task @ current-task <> if
        current-task prepare-block
        dup lock-holder-task @ if
          dup current-task ['] current-lock for-task !
          init-lock-wait
          2dup swap add-lock-wait
          over update-hold-priority
          over lock-slock release-slock-block
          over lock-slock claim-slock
          current-task check-timeout if
            [:
              2dup swap remove-lock-wait
              lock-wait-orig-here @ ram-here!
              update-hold-priority
              current-task restore-priority
            ;] critical ['] x-timed-out ?raise
          then
          lock-wait-orig-here @ ram-here!
          drop
        else
          current-task over lock-holder-task !
          update-hold-priority
        then
      else
        1 swap lock-nest-level +!
      then
      s" END CLAIM LOCK" trace
    ;] over lock-slock with-slock
  ;

  \ Unlock a lock
  : release-lock ( lock -- )
    [:
      s" BEGIN RELEASE LOCK" trace
      dup lock-holder-task @ current-task = averts x-not-currently-owned
      dup lock-nest-level @ 0= if
        dup lock-first-wait @ ?dup if
          2dup swap remove-lock-wait
          lock-wait-task @
          2dup swap lock-holder-task !
          swap update-hold-priority
          dup restore-priority
        else
          0 swap lock-holder-task !
          current-task restore-priority 0     
        then
      else
        -1 swap lock-nest-level +! 0
      then
      s" END RELEASE LOCK" trace
    ;] over lock-slock with-slock
    ?dup if ready then
  ;

  \ Update the priorities of tasks holding locks
  : update-lock-priority ( lock -- )
    ['] update-hold-priority over lock-slock with-slock
  ;

  commit-flash
  
  \ Execute a block of code with a lock
  : with-lock ( xt lock -- ) dup >r claim-lock try r> release-lock ?raise ;

  \ Export lock-size
  ' lock-size export lock-size

end-module

end-compress-flash
    
\ Reboot
reboot