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

begin-module schedule

  systick import
  task import

  begin-module schedule-internal

    \ The current action
    user current-action

    compress-flash

    \ The scheduler structure
    begin-structure schedule-size    
      \ Current action
      field: schedule-current

      \ Last action executed
      field: schedule-last
    end-structure

    \ The action structure
    begin-structure action-size
      \ The action xt
      field: action-xt

      \ Next action
      field: action-next

      \ Whether the action is active (> 0 means active)
      field: action-active

      \ Action scheduler
      field: action-schedule

      \ Action systick start time
      field: action-systick-start
      
      \ Action systick delay time
      field: action-systick-delay
    end-structure

    commit-flash

    \ Find the previous action
    : prev-action ( action1 -- action2 )
      dup begin dup action-next @ 2 pick <> while action-next @ repeat
      tuck = if
	drop 0
      then
    ;

  end-module> import

  \ Initalize a scheduler
  : init-schedule ( schedule -- )
    0 over schedule-current !
    0 swap schedule-last !
  ;

  \ Reset an action
  : reset-action ( xt action -- )
    tuck action-xt ! 0 over action-active !
    0 over action-systick-start ! -1 swap action-systick-delay !
  ;

  \ Set an action's xt
  : set-action-xt ( xt action -- ) action-xt ! ;

  \ Initialize an action for a scheduler
  : init-action ( xt action schedule -- )
    begin-critical
    swap
    2dup action-schedule !
    2 roll over action-xt !
    0 over action-active !
    0 over action-systick-start !
    -1 over action-systick-delay !
    over schedule-last @ 0= if
      2dup swap schedule-last !
    then
    over schedule-current @ 0<> if
      over schedule-current @ action-next @
      over action-next !
      tuck swap schedule-current @ action-next !
    else
      dup dup action-next !
      tuck swap schedule-current !
    then
    end-critical
  ;

  commit-flash

  \ Dispose of an action
  : dispose-action ( action -- )
    begin-critical
    dup action-schedule @
    over -1 swap action-schedule !
    dup schedule-current @ 2 pick = if
      over action-next @ 2 pick <> if
	swap action-next @ swap 2dup schedule-last @ action-next !
	schedule-current !
      else
	nip 0 over schedule-current ! 0 swap schedule-last !
      then
    else
      dup schedule-last @ 2 pick = if
	over prev-action 2dup swap schedule-last !
	nip swap action-next @ swap action-next !
      else
	drop dup prev-action swap action-next @ swap action-next !
      then
    then
    end-critical
  ;

  \ Get whether an action has been disposed
  : action-disposed? ( action -- disposed ) action-schedule @ -1 = ;

  \ Enable an action
  : enable-action ( action -- )
    begin-critical
    dup action-active @ 1+ swap action-active !
    end-critical
  ;

  \ Disable an action
  : disable-action ( action -- )
    begin-critical
    dup action-active @ 1- swap action-active !
    end-critical
  ;

  \ Force-enable an action
  : force-enable-action ( action -- )
    begin-critical
    dup action-active @ 1 < if
      1 swap action-active !
    else
      drop
    then
    end-critical
  ;

  \ Force-disable an action
  : force-disable-action ( action -- )
    begin-critical
    dup action-active @ 0> if
      0 swap action-active !
    else
      drop
    then
    end-critical
  ;

  \ Delay an action
  : action-delay ( ticks-delay ticks-start action -- )
    begin-critical
    tuck action-systick-start !
    action-systick-delay !
    end-critical
  ;

  \ Get the latest delay for an action
  : action-delay-latest ( action -- ticks-delay ticks-start )
    begin-critical
    dup action-systick-delay @
    over action-systick-start @
    end-critical
  ;

  \ Cancel a delay for an action
  : cancel-action-delay ( action -- )
    begin-critical
    0 over action-systick-start !
    -1 swap action-systick-delay !
    end-critical
  ;

  \ Run a schedule
  : run-schedule ( schedule -- )
    begin
      pause
      begin-critical
      dup schedule-current @ dup if
	dup action-active @ 0>
	over action-systick-delay @ -1 =
	systick-counter 3 pick action-systick-start @ -
	3 pick action-systick-delay @ u>= or and
	if
	  dup current-action !
	  dup [:
	    end-critical action-xt @ execute begin-critical
	  ;] try ?dup if
	    end-critical execute begin-critical
	  then
	  2dup swap schedule-last !
	else
	  2dup swap schedule-last @ = if
	    wait-hook @ end-critical ?execute begin-critical
	  then
	then
	action-next @ over schedule-current !
      else
	drop
      then
      end-critical
    again
  ;

  \ Yield the execution of the remainder of the current action; note that this
  \ must be executed from the outside word of the action.
  : yield ( -- )
    r> 1 bic current-action @ action-xt !
  ;

  \ Yield until a condition is true
  : yield-until ( -- )
    postpone if
    postpone true
    postpone else
    postpone yield
    postpone false
    postpone then
    postpone until
  ;

  \ Make current-action read-only
  : current-action ( -- action ) current-action @ ;

  \ Export these constants
  export schedule-size
  export action-size

end-module

end-compress-flash

\ Reboot
reboot
