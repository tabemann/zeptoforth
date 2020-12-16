\ Copyright (c) 2020 Travis Bemann
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

\ Set up the wordlist order
forth-wordlist 1 set-order
forth-wordlist set-current

\ Test to see if this has already been loaded
defined? schedule-wordlist not [if]

  \ Set up the wordlist
  wordlist constant schedule-wordlist
  wordlist constant schedule-internal-wordlist
  forth-wordlist systick-wordlist task-wordlist schedule-wordlist
  schedule-internal-wordlist 5 set-order
  schedule-internal-wordlist set-current

  \ The current action
  user current-action

  \ Set the current wordlist
  schedule-wordlist set-current

  \ The scheduler structure
  begin-structure schedule-size
    
    \ Set the current wordlist
    schedule-internal-wordlist set-current
    
    \ Current action
    field: schedule-current

    \ Last action executed
    field: schedule-last
  end-structure

  \ Set the current wordlist
  schedule-wordlist set-current

  \ The action structure
  begin-structure action-size

    \ Set the current wordlist
    schedule-internal-wordlist set-current
    
    \ The action xt
    field: action-xt

    \ Next action
    field: action-next

    \ Whether the action is active (> 0 means active)
    field: action-active

    \ Action systick start time
    field: action-systick-start
    
    \ Action systick delay time
    field: action-systick-delay
  end-structure

  \ Find the previous action
  : prev-action ( action1 -- action2 )
    dup begin dup action-next @ 2 pick <> while action-next @ repeat
    tuck = if
      drop 0
    then
  ;

  \ Set non-internal
  schedule-wordlist set-current

  \ Initalize a scheduler
  : init-schedule ( schedule -- )
    0 over schedule-current !
    0 swap schedule-last !
  ;

  \ Add an action to a scheduler
  : add-action ( xt action schedule -- )
    begin-critical
    swap
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

  \ Dispose of an action
  : dispose-action ( action schedule -- )
    begin-critical
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

  \ Start a delay from the present
  : start-action-delay ( 1/10m-delay action -- )
    begin-critical
    dup systick-counter swap action-systick-start !
    action-systick-delay !
    end-critical
  ;

  \ Set a delay for an action
  : set-action-delay ( 1/10ms-delay 1/10ms-start action -- )
    begin-critical
    tuck action-systick-start !
    action-systick-delay !
    end-critical
  ;

  \ Advance a delay for an action by a given amount of time
  : advance-action-delay ( 1/10ms-offset action -- )
    begin-critical
    systick-counter over action-systick-start @ - over
    action-systick-delay @ < if
      action-systick-delay +!
    else
      dup action-systick-delay @ over action-systick-start +!
      action-systick-delay !
    then
    end-critical
  ;

  \ Advance of start a delay from the present, depending on whether the delay
  \ length has changed
  : reset-action-delay ( 1/10ms-delay action -- )
    begin-critical
    dup action-systick-delay @ 2 pick = if
      advance-action-delay
    else
      start-action-delay
    then
    end-critical
  ;

  \ Get a delay for an action
  : get-action-delay ( action -- 1/10ms-delay 1/10ms-start )
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

  \ Make current-action read-only
  : current-action ( -- action ) current-action @ ;

[then]

\ Warm reboot
warm
