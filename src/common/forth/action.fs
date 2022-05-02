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

  \ Action is already sending a message exception
  : x-already-sending-msg ( -- ) ." action is already sending a message" cr ;

  \ Action is already receiving a message exception
  : x-already-recving-msg ( -- ) ." action is already receiving a message" cr ;
  
  \ Resume xt already set for action exception
  : x-resume-xt-already-set ( -- ) ." resume xt already set for action" cr ;

  \ Delay already set for action exception
  : x-delay-already-set ( -- ) ." delay already set for action" cr ;
  
  begin-module action-internal

    user current-schedule
    user current-action
    
    begin-structure schedule-size

      slock-size +field schedule-slock
      field: schedule-next
      field: schedule-prev
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
      field: action-msg-dest
      field: action-msg-data
      field: action-msg-size
      
    end-structure

  end-module> import

  \ Print out a schedule
  : schedule. ( action -- )
    cr ." schedule-next:     " dup schedule-next @ h.8
    cr ." schedule-prev:     " dup schedule-prev @ h.8
    cr ." schedule-running?: " schedule-running? @ .
  ;

  \ Print out an action
  : action. ( action -- )
    cr ." action-schedule:      " dup action-schedule @ h.8
    cr ." action-next:          " dup action-next @ h.8
    cr ." action-prev:          " dup action-prev @ h.8
    cr ." action-resume-xt:     " dup action-resume-xt @ h.8
    cr ." action-recv-xt:       " dup action-recv-xt @ h.8
    cr ." action-systick-start: " dup action-systick-start @ .
    cr ." action-systick-delay: " dup action-systick-delay @ .
    cr ." action-data:          " dup action-data @ h.8
    cr ." action-msg-dest:      " dup action-msg-dest @ h.8
    cr ." action-msg-data:      " dup action-msg-data @ h.8
    cr ." action-msg-size:      " action-msg-size @ .
  ;

  continue-module action-internal

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

    \ Copy a message from one action to another
    : copy-msg ( src-action dest-action -- bytes )
      2dup action-msg-size @ swap action-msg-size @ min >r
      swap action-msg-data @ swap action-msg-data @ r@ move r>
    ;

    \ Clear sending message
    : clear-send-msg ( src-action -- )
      -1 over action-systick-start ! -1 over action-systick-delay !
      0 over action-msg-dest ! 0 over action-msg-data ! 0 over action-msg-size !
      dup action-send-xt @ over action-resume-xt !
      0 swap action-send-xt !
    ;

    \ Fail sending message
    : fail-send-msg ( src-action -- )
      -1 over action-systick-start ! -1 over action-systick-delay !
      0 over action-msg-dest ! 0 over action-msg-data ! 0 over action-msg-size !
      0 swap action-send-xt !
    ;
    
    \ Clear send timeout
    : clear-send-timeout ( src-action -- )
      -1 over action-systick-start ! -1 over action-systick-delay !
      0 over action-msg-dest ! 0 over action-msg-data ! 0 over action-msg-size !
      0 over action-send-xt ! 0 swap action-resume-xt !
    ;

    \ Clear receiving message
    : clear-recv-msg ( dest-action -- )
      -1 over action-systick-start ! -1 over action-systick-delay !
      0 over action-recv-xt ! 0 over action-msg-data ! 0 over action-msg-size !
      0 swap action-resume-xt !
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
	  current-schedule @ schedule-next @ action-prev @
	  current-schedule @ schedule-prev !
	then
      ;] current-schedule @ schedule-slock with-slock
    ; 

    \ Get whether an action is the only action in a schedule
    : only-in-schedule? ( action schedule -- flag )
      2dup schedule-next @ = -rot schedule-prev @ = and
    ;
    
    \ Set no delay for the current action
    : no-action-delay ( -- )
      -1 current-action @ 2dup action-systick-delay ! action-systick-start !
    ;

    \ Validate the state of the current action
    : validate-current-action ( -- )
      current-action @ action-resume-xt @ 0= averts x-resume-xt-already-set
      current-action @ action-recv-xt @ 0= averts x-already-recving-msg
      current-action @ action-send-xt @ 0= averts x-already-sending-msg
      current-action @ action-systick-start @ -1 = averts x-delay-already-set
    ;

    \ Execute a word and handle exceptions while continuing
    : execute-handle ( xt -- )
      try ?dup if display-red execute display-normal then
    ;

  end-module

  export schedule-size
  export action-size

  \ Initialize a schedule
  : init-schedule ( addr -- )
    dup schedule-slock init-slock
    false over schedule-running? !
    0 over schedule-next !
    0 swap schedule-prev !
  ;

  \ Initialize an action
  : init-action ( data xt addr -- )
    0 over action-schedule !
    tuck action-resume-xt !
    -1 over action-systick-start !
    -1 over action-systick-delay !
    0 over action-recv-xt !
    0 over action-send-xt !
    tuck action-data !
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
	dup action-schedule @ schedule-prev @ over action-prev !
	dup dup action-schedule @ schedule-next @ action-prev !
	dup dup action-schedule @ schedule-prev @ action-next !
	dup action-schedule @ schedule-next !
      else
	dup dup action-schedule @ schedule-next !
	dup dup action-schedule @ schedule-prev !
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
      dup dup action-schedule @ only-in-schedule? if
	dup action-schedule @ 0 over schedule-prev ! 0 over schedule-next !
      else
	dup action-schedule @ schedule-next @ over = if
	  dup action-next @ over action-schedule @ schedule-next !
	then
	dup action-schedule @ schedule-prev @ over = if
	  dup action-prev @ over action-schedule @ schedule-prev !
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
	r@ action-msg-dest !
	r@ action-msg-size !
	r@ action-msg-data !
	r@ action-resume-xt !
	r> action-send-xt !
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
	r@ action-systick-delay !
	systick-counter r@ action-systick-start !
	r@ action-msg-dest !
	r@ action-msg-size !
	r@ action-msg-data !
	r@ action-resume-xt !
	r> action-send-xt !
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
      r@ action-msg-size !
      r@ action-msg-data !
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
      r@ action-systick-delay !
      systick-counter r@ action-systick-start !
      r@ action-msg-size !
      r@ action-msg-data !
      r@ action-resume-xt !
      r> action-recv-xt !
    else
      2drop 2drop drop rdrop
    then
  ;

  \ Delay the current action
  : delay-action ( resume-xt ticks -- )
    validate-current-action
    current-action @ >r
    r@ action-systick-delay !
    systick-counter r@ action-systick-start !
    r> action-resume-xt !
  ;

  \ Delay the current action from a given time
  : delay-action-from-time ( resume-xt systick-start systick-delay -- )
    validate-current-action
    current-action @ >r
    r@ action-systick-delay !
    r@ action-systick-start !
    r> action-resume-xt !
  ;

  \ Yield the current action
  : yield-action ( resume-xt -- )
    validate-current-action
    no-action-delay
    current-action @ action-resume-xt !
  ;
  
  \ Run schedule
  : run-schedule ( schedule -- )
    dup schedule-running? @ triggers x-schedule-already-running
    current-schedule !
    true current-schedule @ schedule-running? !
    begin current-schedule @ schedule-running? @ while
      current-schedule @ schedule-next @ ?dup if
	dup action-recv-xt @ if
	  dup action-systick-start @ -1 = swap
	  systick-counter over action-systick-start @ -
	  over action-systick-delay @ < rot or if
	    dup find-msg ?dup if
	      2dup swap copy-msg swap >r r@ clear-send-msg
	      over action-msg-data @ swap rot dup action-recv-xt @
	      swap dup clear-recv-msg
	      dup current-action ! advance-action
	      r> swap execute-handle
	    else
	      advance-action
	    then
	  else
	    dup current-action ! dup advance-action dup action-resume-xt @
	    swap clear-recv-msg no-action-delay execute-handle
	  then
	else
	  dup action-send-xt @ 0= if
	    dup action-resume-xt @ if
	      dup action-systick-start @ -1 = swap
	      systick-counter over action-systick-start @ -
	      over action-systick-delay @ >= rot or if
		dup current-action ! dup advance-action dup action-resume-xt @
		0 rot action-resume-xt !
		no-action-delay execute-handle
	      else
		advance-action
	      then
	    else
	      dup advance-action remove-action
	    then
	  else
	    dup action-resume-xt @ if
	      dup action-systick-start @ -1 <> swap
	      systick-counter over action-systick-start @ -
	      over action-systick-delay @ >= rot and if
		dup current-action ! dup advance-action dup action-resume-xt @
		swap clear-send-timeout
		no-action-delay execute-handle
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
  : current-data ( -- data ) current-action @ action-data @ ;
  
end-module

reboot
