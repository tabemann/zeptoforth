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
\ SOFTWARE

compile-to-flash

begin-module action

  task import
  systick import
  slock import
  
  \ Action is already in schedule exception
  : x-already-in-schedule ( -- ) ." action already in schedule" cr ;
  
  \ Action is not in a schedule exception
  : x-not-in-schedule ( -- ) ." action not in a schedule" cr ;

  \ Schedule is already running exception
  : x-schedule-already-running ( -- ) ." schedule is already running" cr ;

  \ Operation already set exception
  : x-operation-set ( -- ) ." operation already set" cr ;

  begin-module action-internal

    user current-schedule
    user current-action
    
    begin-structure schedule-size

      slock-size +field schedule-slock
      field: schedule-next
      field: schedule-running?
      
    end-structure
    
    begin-structure action-size

      field: action-schedule
      field: action-next
      field: action-prev
      field: action-resume-xt
      field: action-systick-start
      field: action-systick-delay
      field: action-recv-xt
      field: action-send-xt
      field: action-data
      field: action-msg-src
      field: action-msg-dest
      field: action-msg-data
      field: action-msg-size
      
    end-structure

  end-module> import

  \ Print out a schedule
  : schedule. ( action -- )
    cr ." schedule-next:     " dup schedule-next @ h.8
    cr ." schedule-running?: " schedule-running? @ .
  ;

  \ Print out an action
  : action. ( action -- )
    cr ." action-schedule:      " dup action-schedule @ h.8
    cr ." action-next:          " dup action-next @ h.8
    cr ." action-prev:          " dup action-prev @ h.8
    cr ." action-resume-xt:     " dup action-resume-xt @ h.8
    cr ." action-recv-xt:       " dup action-recv-xt @ h.8
    cr ." action-send-xt:       " dup action-send-xt @ h.8
    cr ." action-systick-start: " dup action-systick-start @ .
    cr ." action-systick-delay: " dup action-systick-delay @ .
    cr ." action-data:          " dup action-data @ h.8
    cr ." action-msg-src:       " dup action-msg-src @ h.8
    cr ." action-msg-dest:      " dup action-msg-dest @ h.8
    cr ." action-msg-data:      " dup action-msg-data @ h.8
    cr ." action-msg-size:      " action-msg-size @ .
  ;

  continue-module action-internal

    \ No operation
    -2 constant no-operation

    \ No delay
    -1 constant no-delay

    \ No message
    -1 constant no-msg
    
    \ Find a message for the current action
    : find-msg ( action -- msg-action | 0 )
      current-schedule @ schedule-next @ dup >r
      r@ if
	begin
	  2dup action-msg-dest @ = if
	    nip true
	  else
	    action-next @ dup r@ = if 2drop 0 true else false then
	  then
	until
      else
	2drop 0
      then
      rdrop
    ;

    \ Copy a message from a source action to another action
    : copy-msg-src ( src-addr dest-action src-size -- bytes )
      over action-msg-size @ min >r
      action-msg-data @ r@ move r>
    ;

    \ Copy a message from one action to a destination action
    : copy-msg-dest ( src-action dest-action dest-size -- bytes )
      2 pick action-msg-size @ min >r
      swap action-msg-data @ swap action-msg-data @ r@ move r>
    ;

    \ Clear sending message
    : clear-send-msg ( src-action -- )
      no-operation over action-systick-delay !
      0 over action-msg-dest ! dup action-send-xt @ over action-resume-xt !
      0 swap action-send-xt !
    ;

    \ Fail sending message
    : fail-send-msg ( src-action -- )
      no-operation over action-systick-delay !
      0 over action-msg-dest ! 0 swap action-send-xt !
    ;
    
    \ Clear send timeout
    : clear-send-timeout ( src-action -- )
      no-operation over action-systick-delay !
      0 over action-msg-dest ! 0 over action-send-xt ! 0 swap action-resume-xt !
    ;

    \ Clear receiving message
    : clear-recv-msg ( dest-action -- )
      no-operation over action-systick-delay !
      0 over action-msg-src ! 0 swap action-resume-xt !
    ;

    \ Fail all sends to an action in a schedule
    : fail-all-send-msg ( dest-action -- )
      dup action-schedule @ schedule-next @ dup >r
      r@ if
	begin
	  2dup action-msg-dest @ = if dup fail-send-msg then
	  action-next @ dup r@ =
	until
      then
      rdrop 2drop
    ;

    \ Advance to the next action
    : advance-action ( action -- )
      [:
	dup current-schedule @ schedule-next @ = if
	  action-next @ current-schedule @ schedule-next !
	then
      ;] current-schedule @ schedule-slock with-slock
    ; 

    \ Get whether an action is the only action in a schedule
    : only-in-schedule? ( action -- flag ) dup action-next @ = ;
    
    \ Set no delay for the current action
    : no-action-delay ( -- )
      no-delay current-action @ action-systick-delay !
    ;

    \ Validate the state of the current action
    : validate-current-action ( -- )
      current-action @ action-systick-delay @ no-operation =
      averts x-operation-set
    ;

  end-module

  export schedule-size
  export action-size

  \ Initialize a schedule
  : init-schedule ( addr -- )
    dup schedule-slock init-slock
    false over schedule-running? !
    0 swap schedule-next !
  ;

  \ Initialize an action
  : init-action ( data xt addr -- )
    0 over action-schedule !
    tuck action-resume-xt !
    no-operation over action-systick-start !
    no-operation over action-systick-delay !
    0 over action-recv-xt !
    0 over action-send-xt !
    tuck action-data !
    0 over action-msg-src !
    0 over action-msg-dest !
    0 over action-msg-data !
    0 swap action-msg-size !
  ;

  \ Add an action to a schedule
  : add-action ( schedule action -- )
    [:
      dup action-schedule @ 0= averts x-already-in-schedule
      tuck action-schedule !
      dup action-schedule @ schedule-next @ if
	dup action-schedule @ schedule-next @ over action-next !
	dup action-schedule @ schedule-next @ action-prev @ over action-prev !
	dup dup action-schedule @ schedule-next @ action-prev @ action-next !
	dup dup action-schedule @ schedule-next @ action-prev !
	dup action-schedule @ schedule-next !
      else
	dup dup action-schedule @ schedule-next !
	dup dup action-next !
	dup action-prev !
      then
    ;] 2 pick schedule-slock with-slock
  ;

  \ Remove an action from a schedule
  : remove-action ( action -- )
    dup action-schedule @ 0<> averts x-not-in-schedule
    [:
      dup fail-all-send-msg
      dup only-in-schedule? if
	dup action-schedule @ 0 swap schedule-next !
      else
	dup action-schedule @ schedule-next @ over = if
	  dup action-next @ over action-schedule @ schedule-next !
	then
	dup action-next @ over action-prev @ action-next !
	dup action-prev @ over action-next @ action-prev !
      then
      0 over action-prev ! 0 over action-next ! 0 swap action-schedule !
    ;] over action-schedule @ schedule-slock with-slock
  ;

  \ Send a message to an action with an option to handle failure; send-xt has
  \ the signature ( -- ) and fail-xt has the signature ( -- )
  : send-action-fail ( send-xt fail-xt addr bytes dest-action -- )
    validate-current-action
    no-action-delay
    current-action @ >r
    r@ action-schedule @ current-schedule @ = if
      dup action-schedule @ current-schedule @ = if
	dup action-msg-src @ no-msg = if
	  dup >r swap copy-msg-src r@ action-msg-size !
	  r> r@ swap action-msg-src !
	  drop r> action-resume-xt !
	else
	  r@ action-msg-dest !
	  r@ action-msg-size !
	  r@ action-msg-data !
	  r@ action-resume-xt !
	  r> action-send-xt !
	then
      else
	drop 2drop r> action-resume-xt ! drop
      then
    else
      drop 2drop 2drop rdrop
    then
  ;

  \ Send a message to an action while silently handling failure; resume-xt has
  \ the signature ( -- )
  : send-action ( send-xt addr bytes dest-action -- )
    >r >r >r dup r> r> r> send-action-fail
  ;

  \ Send a message to an action with an option to handle failure and timeout;
  \ send-xt has the signature ( -- ) and fail-xt has the signature ( -- )
  : send-action-timeout
    ( send-xt fail-xt addr bytes dest-action timeout-ticks -- )
    validate-current-action
    current-action @ >r
    r@ action-schedule @ current-schedule @ = if
      over action-schedule @ current-schedule @ = if
	over action-msg-src @ no-msg = if
	  drop
	  dup >r swap copy-msg-src r@ action-msg-size !
	  r> r@ swap action-msg-src !
	  drop r> action-resume-xt !
	  no-action-delay
	else
	  dup 0>= if
	    r@ action-systick-delay !
	    systick-counter r@ action-systick-start !
	  else
	    0 r@ action-systick-delay !
	    systick-counter + r@ action-systick-start !
	  then
	  r@ action-msg-dest !
	  r@ action-msg-size !
	  r@ action-msg-data !
	  r@ action-resume-xt !
	  r> action-send-xt !
	then
      else
	2drop 2drop r> action-resume-xt ! drop
      then
    else
      2drop 2drop 2drop rdrop
    then
  ;

  \ Receive a message for the current action; recv-xt has the signature
  \ ( addr bytes src-action -- )
  : recv-action ( recv-xt addr bytes -- )
    validate-current-action
    no-action-delay
    current-action @ >r
    r@ action-schedule @ current-schedule @ = if
      r@ find-msg ?dup if
	dup r@ action-msg-src !
	rot r@ action-msg-data !
	r@ over >r rot copy-msg-dest r> clear-send-msg r@ action-msg-size !
      else
	no-msg r@ action-msg-src !
	r@ action-msg-size !
	r@ action-msg-data !
      then
      r> action-recv-xt !
    else
      2drop drop rdrop
    then
  ;

  \ Receive a message for the current action with a timeout; recv-xt has the
  \ signature ( addr bytes src-action -- ) and timeout-xt has the signature
  \ ( -- )
  : recv-action-timeout
    ( recv-xt timeout-xt addr bytes timeout-ticks -- )
    validate-current-action
    current-action @ >r
    r@ action-schedule @ current-schedule @ = if
      r@ find-msg ?dup if
	nip
	dup r@ action-msg-src !
	rot r@ action-msg-data !
	r@ over >r rot copy-msg-dest r> clear-send-msg r@ action-msg-size !
	drop
	r> action-recv-xt !
	no-action-delay
      else
	dup 0>= if
	  r@ action-systick-delay !
	  systick-counter r@ action-systick-start !
	else
	  0 r@ action-systick-delay !
	  systick-counter + r@ action-systick-start !
	then
	no-msg r@ action-msg-src !
	r@ action-msg-size !
	r@ action-msg-data !
	r@ action-resume-xt !
	r> action-recv-xt !
      then
    else
      2drop 2drop drop rdrop
    then
  ;

  \ Delay the current action
  : delay-action ( resume-xt ticks -- )
    validate-current-action
    current-action @ >r
    dup 0>= if
      r@ action-systick-delay !
      systick-counter r@ action-systick-start !
    else
      0 r@ action-systick-delay !
      systick-counter + r@ action-systick-start !
    then
    r> action-resume-xt !
  ;

  \ Delay the current action from a given time
  : delay-action-from-time ( resume-xt systick-start systick-delay -- )
    validate-current-action
    current-action @ >r
    dup 0>= if
      r@ action-systick-delay !
      r@ action-systick-start !
    else
      0 r@ action-systick-delay !
      + r@ action-systick-start !
    then
    r> action-resume-xt !
  ;

  \ Yield the current action
  : yield-action ( resume-xt -- )
    validate-current-action
    no-action-delay
    current-action @ action-resume-xt !
  ;
  
  continue-module action-internal
  
    \ Execute a word and handle exceptions while continuing
    : execute-handle ( xt -- )
      try ?dup if
	current-action @ remove-action display-red execute display-normal
      then
    ;

  end-module

  \ Run schedule
  : run-schedule ( schedule -- )
    dup schedule-running? @ triggers x-schedule-already-running
    current-schedule !
    true current-schedule @ schedule-running? !
    begin current-schedule @ schedule-running? @ while
      current-schedule @ schedule-next @ ?dup if
	dup action-msg-src @ ?dup if
	  no-msg = if
	    dup action-systick-delay @ 0>= swap
	    systick-counter over action-systick-start @ -
	    over action-systick-delay @ >= rot and if
	      dup current-action ! dup advance-action dup action-resume-xt @
	      swap clear-recv-msg execute-handle
	    else
	      advance-action
	    then
	  else
	    >r r@ action-msg-data @ r@ action-msg-size @
	    r@ action-msg-src @ r@ advance-action r@ action-recv-xt @
	    r@ clear-recv-msg r> current-action !
	    execute-handle
	  then
	else
	  dup action-send-xt @ 0= if
	    dup action-resume-xt @ if
	      dup action-systick-delay @ 0< swap
	      systick-counter over action-systick-start @ -
	      over action-systick-delay @ >= rot or if
		dup current-action ! dup advance-action dup action-resume-xt @
		swap no-operation over action-systick-delay !
		0 swap action-resume-xt !
		execute-handle
	      else
		advance-action
	      then
	    else
	      dup advance-action remove-action
	    then
	  else
	    dup action-resume-xt @ if
	      dup action-systick-delay @ 0>= swap
	      systick-counter over action-systick-start @ -
	      over action-systick-delay @ >= rot and if
		dup current-action ! dup advance-action dup action-resume-xt @
		swap clear-send-timeout
		execute-handle
	      else
		advance-action
	      then
	    else
	      advance-action
	    then
	  then
	then
      else
	pause
      then
    repeat
  ;

  \ Stop a running schedule cleanly
  : stop-schedule ( schedule -- ) false swap schedule-running? ! ;
  
  \ Get the current schedule
  : current-schedule ( -- schedule ) current-schedule @ ;

  \ Get the current action
  : current-action ( -- action ) current-action @ ;

  \ Get the current action's data
  : current-data ( -- data ) current-action action-data @ ;
  
end-module

reboot
