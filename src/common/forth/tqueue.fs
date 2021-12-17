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

\ Compile to flash
compile-to-flash

compress-flash

begin-module tqueue-module

  task-module import

  begin-module tqueue-internal-module

    \ Task queue header structure
    begin-structure tqueue-size

      \ First fast channel send task queue
      field: first-wait

      \ First fast channel receive task queue
      field: last-wait

      \ Wait counter
      field: wait-counter
      
      \ Wait limit
      field: wait-limit
      
    end-structure

    \ Wait structure
    begin-structure wait-size

      \ Fast channel wait previous record
      field: wait-prev

      \ Fast channel wait task
      field: wait-task

      \ Original here position
      field: wait-orig-here

    end-structure

    commit-flash
    
    \ Set up a wait record
    : init-wait ( -- wait )
      ram-here 4 ram-align, ram-here wait-size ram-allot
      tuck wait-orig-here !
      0 over wait-prev !
      current-task over wait-task !
    ;

    \ Add a wait record
    : add-wait ( wait tqueue -- )
      dup first-wait @ if
	2dup last-wait @ wait-prev !
      else
	2dup first-wait !
      then
      last-wait !
    ;

    \ Remove a wait record
    : remove-wait ( wait tqueue -- )
      dup first-wait @ 2 pick = if
	dup last-wait @ 2 pick = if
	  0 over last-wait !
	then
	swap wait-prev @ swap first-wait !
      else
	dup first-wait @ begin
	  dup if
	    dup wait-prev @ 3 pick = if
	      over last-wait @ 3 pick = if
		0 over wait-prev ! swap last-wait ! drop true
	      else
		rot wait-prev @ swap wait-prev ! drop true
	      then
	    else
	      wait-prev @ false
	    then
	  else
	    2drop drop true
	  then
	until
      then
    ;

  end-module> import

  \ Value indicating no task queue limti
  -1 constant no-tqueue-limit
  
  \ Initialize a task queue
  : init-tqueue ( addr -- )
    0 over wait-counter !
    -1 over wait-limit !
    0 over first-wait !
    0 swap last-wait !
  ;

  \ Initialize a task queue with a limit and initial counter
  : init-tqueue-full ( limit counter addr -- )
    tuck wait-counter !
    tuck wait-limit !
    0 over first-wait !
    0 swap last-wait !
  ;

  commit-flash
  
  \ Wait on a task queue
  \ Note that this must be called within a critical section
  : wait-tqueue ( tqueue -- )
    \    begin-critical
    s" BEGIN WAIT-TQUEUE" trace
    -1 over wait-counter +!
    dup wait-counter @ 0>= if
      drop exit
    then
    init-wait
    2dup swap add-wait
    current-task block-critical
    end-critical
    [: current-task validate-timeout ;] try ?dup if
      >r [:
	1 over wait-counter +! tuck swap remove-wait wait-orig-here @ ram-here!
      ;] critical r> ?raise
    then
    wait-orig-here @ ram-here!
    drop
    s" END WAIT-TQUEUE" trace
\    end-critical
  ;

  \ Wake up a task in a task queue
  \ Note that this must be called within a critical section
  : wake-tqueue ( tqueue -- )
    \    begin-critical
    s" BEGIN WAKE-TQUEUE" trace
    dup wait-limit @ 0> if
      dup wait-counter @ 1+ over wait-limit @ min over wait-counter !
    else
      1 over wait-counter +!
    then
    dup first-wait @ ?dup if
      dup wait-prev @ 2 pick first-wait !
      over first-wait @ 0= if
	0 rot last-wait !
      else
	nip
      then
      wait-task @ ready
    else
      drop
    then
    s" END WAKE-TQUEUE" trace
\    end-critical
  ;

  \ Un-wake a task queue
  : unwake-tqueue ( tqueue -- )
    -1 swap wait-counter +!
  ;

  commit-flash

  \ Wake up all tasks in a task queue
  : wake-tqueue-all ( tqueue -- )
    \ begin-critical
    begin dup wait-counter @ 0> while
      dup wake-tqueue
    repeat
    drop
    \ end-critical
  ;
  
  \ Export tqueue-size
  export tqueue-size

end-module

end-compress-flash

\ Reboot
reboot
