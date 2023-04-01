\ Copyright (c) 2023 Travis Bemann
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

begin-module elevator

  oo import
  action import
  ansi-term import
  systick import

  \ Invalid floor count exception
  : x-out-of-range-floor-count ( -- ) ." out of range floor count" cr ;

  begin-module elevator-internal
  
    \ Maximum floor count
    9 constant max-floor-count
    
    \ Floor up flag
    0 bit constant floor-up
    
    \ Floor down flag
    1 bit constant floor-down
    
    \ Floor stop flag
    2 bit constant floor-stop
    
    \ No elevator direction
    0 constant no-elevator-dir
    
    \ Elevator up direction
    1 constant elevator-up-dir
    
    \ Elevator down direction
    2 constant elevator-down-dir
    
    \ Elevator delay in ticks
    10000 constant elevator-delay

    \ Cycle world message
    0 constant cycle-msg

    \ Floor up message
    1 constant floor-up-msg

    \ Floor down message
    2 constant floor-down-msg

    \ Floor stop message
    3 constant floor-stop-msg
    
    \ Elevator state class
    <object> begin-class <state>

      \ Floor count
      cell member floor-count

      \ Current floor
      cell member current-floor

      \ Half floor
      cell member half-floor

      \ Door is open
      cell member door-open

      \ Elevator direction
      cell member elevator-dir

      \ Floor flags
      max-floor-count cell align member floor-flags

      \ Get the floor count
      method floor-count@ ( state -- floor-count )
      
      \ Set the current floor
      method current-floor! ( floor state -- )

      \ Get the current floor
      method current-floor@ ( state -- floor )

      \ Set the half-floor status
      method half-floor! ( half-floor? state -- )

      \ Get the half-floor status
      method half-floor? ( state -- half-floor? )

      \ Set the door-open status
      method door-open! ( door-open? state -- )

      \ Get the door-open status
      method door-open? ( state -- door-open? )

      \ Set the elevator direction
      method elevator-dir! ( dir state -- )

      \ Get the elevator direction
      method elevator-dir@ ( state -- dir )

      \ Press the up button on a floor
      method floor-up! ( floor state -- )

      \ Press the down button on a floor
      method floor-down! ( floor state -- )

      \ Press the stop button on a floor
      method floor-stop! ( floor state -- )

      \ Get the up button state on a floor
      method floor-up? ( floor state -- floor-up? )

      \ Get the down button state on a floor
      method floor-down? ( floor state -- floor-down? )

      \ Get the stop button state on a floor
      method floor-stop? ( floor state -- floor-stop? )

      \ Get whether a floor is waiting
      method floor-waiting? ( floor state -- floor-waiting? )

      \ Clear the state of a floor
      method clear-floor! ( floor state -- )

      \ Get flags for a floor
      method flags@ ( floor state -- flags )

      \ Set flags for a floor
      method flags! ( flags floor state -- )

    end-class

    \ Implement the elevator state class
    <state> begin-implement

      \ Initialize an elevator state
      :noname { count state -- }
        count 0> averts x-out-of-range-floor-count
        count max-floor-count <= averts x-out-of-range-floor-count
        state <object>->new
        count state floor-count !
        1 state current-floor !
        false state half-floor !
        false state door-open !
        no-elevator-dir state elevator-dir !
        count 1+ 1 ?do i state clear-floor! loop
      ; define new

      \ Get the floor count
      :noname { state -- floor-count }
        state floor-count @
      ; define floor-count@

      \ Set the current floor
      :noname { floor state -- }
        floor state current-floor !
      ; define current-floor!

      \ Get the current floor
      :noname { state -- floor }
        state current-floor @
      ; define current-floor@

      \ Set the half-floor status
      :noname { half-floor? state -- }
        half-floor? state half-floor !
      ; define half-floor!

      \ Get the half-floor status
      :noname { state -- half-floor? }
        state half-floor @
      ; define half-floor?

      \ Set the door-open status
      :noname { door-open? state -- }
        door-open? state door-open !
      ; define door-open!

      \ Get the door-open status
      :noname { state -- door-open? }
        state door-open @
      ; define door-open?

      \ Set the elevator direction
      :noname { dir state -- }
        dir state elevator-dir !
      ; define elevator-dir!

      \ Get the elevator direction
      :noname { state -- dir }
        state elevator-dir @
      ; define elevator-dir@

      \ Press the up button on a floor
      :noname { floor state -- }
        floor-up floor state flags@ or floor state flags!
      ; define floor-up!

      \ Press the down button on a floor
      :noname { floor state -- }
        floor-down floor state flags@ or floor state flags!
      ; define floor-down!

      \ Press the stop button on a floor
      :noname { floor state -- }
        floor-stop floor state flags@ or floor state flags!
      ; define floor-stop!

      \ Get the up button state on a floor
      :noname { floor state -- floor-up? }
        floor state flags@ floor-up and 0<>
      ; define floor-up?

      \ Get the down button state on a floor
      :noname { floor state -- floor-down? }
        floor state flags@ floor-down and 0<>
      ; define floor-down?

      \ Get the stop button state on a floor
      :noname { floor state -- floor-stop? }
        floor state flags@ floor-stop and 0<>
      ; define floor-stop?
      
      \ Get whether a floor is waiting
      :noname { floor state -- floor-waiting? }
        floor state floor-up?
        floor state floor-down? or
        floor state floor-stop? or
      ; define floor-waiting?

      \ Clear the state of a floor
      :noname { floor state -- }
        0 floor state flags!
      ; define clear-floor!

      \ Get flags for a floor
      :noname { floor state -- flags }
        floor 1- state floor-flags + c@
      ; define flags@

      \ Set flags for a floor
      :noname { flags floor state -- }
        flags floor 1- state floor-flags + c!
      ; define flags!

    end-implement

    \ The elevator display class
    <object> begin-class <display>

      \ The display action
      action-size member display-action

      \ The display state
      <state> class-size member display-state

      \ Get the display's action
      method display-action@ ( display -- action )

      \ Wait for a display message
      method wait-display-msg ( display -- )
      
      \ Receive a display message
      method recv-display-msg ( display -- )

      \ Draw the world
      method draw-world ( display -- )

      \ Draw a floor
      method draw-floor ( floor display -- )

      \ Draw a half-floor
      method draw-half-floor ( floor display -- )

      \ Draw an elevator
      method draw-elevator ( display -- )
      
    end-class

    \ Implement the elevator display class
    <display> begin-implement

      \ Initialize a display
      :noname { schedule display -- }
        display <object>->new
        display [: current-data wait-display-msg ;] display display-action
        init-action
        schedule display display-action add-action
      ; define new

      \ Get the display's action
      :noname { display -- action }
        display display-action
      ; define display-action@
      
      \ Wait for a display message
      :noname { display -- }
        [: drop 2drop current-data recv-display-msg ;]
        display display-state <state> class-size recv-action
      ; define wait-display-msg
      
      \ Receive a display message
      :noname { display -- }
        display draw-world
        display wait-display-msg
      ; define recv-display-msg

      \ Draw the the simulator
      :noname ( display -- )
        [: { display }
          0 0 go-to-coord
          ."   LIFT SIMULATOR" cr
          ." #   UP DN STOPS  LIFT" cr
          ." ========================" cr
          1 display display-state floor-count @ do
            i display draw-floor
            i 1 > if
              i 1- display draw-half-floor
            then
          -1 +loop
          ." ========================" cr
        ;] execute-hide-cursor
      ; define draw-world

      \ Draw a floor
      :noname { floor display -- }
        floor display display-state flags@ { flags }
        ."  " floor (.) ."   "
        floor display display-state floor-up? if ." #  " else ."    " then
        floor display display-state floor-down? if ." #    " else ."      " then
        display display-state door-open?
        floor display display-state current-floor@ = and if
          floor display display-state floor-stop?
          if ." #    " else ."      " then
        else
          floor display display-state floor-stop?
          if ." #   |" else ."     |" then
        then
        floor display display-state current-floor@ =
        display display-state half-floor? not and if
          display draw-elevator
        else
          ."     "
        then
        ." |   " cr
      ; define draw-floor

      \ Draw a half-floor
      :noname { floor display -- }
        ." ----------------|"
        floor display display-state current-floor@ =
        display display-state half-floor? and if
          display draw-elevator
        else
          ."     "
        then
        ." |-- " cr
      ; define draw-half-floor

      \ Draw an elevator
      :noname { display -- }
        display display-state door-open? if
          display display-state elevator-dir@ case
            elevator-up-dir of ."  UP]" endof
            elevator-down-dir of ."  DN]" endof
            no-elevator-dir of ."    ]" endof
          endcase
        else
          display display-state elevator-dir@ case
            elevator-up-dir of ." [UP]" endof
            elevator-down-dir of ." [DN]" endof
            no-elevator-dir of ." [  ]" endof
          endcase
        then
      ; define draw-elevator

    end-implement

    \ Elevator controller class
    <object> begin-class <control>

      \ The control action
      action-size member control-action

      \ The display action
      cell member control-display-action
      
      \ The control state
      <state> class-size member control-state

      \ Message receive buffer
      2 cells member control-recv-buf

      \ Get the controller's action
      method control-action@ ( control -- action )
      
      \ Wait for a control message
      method wait-control-msg ( control -- )
      
      \ Receive a control message
      method recv-control-msg ( control -- )

      \ Cycle the world
      method cycle-world ( control -- )

      \ Get whether a floor is an upward stop
      method upward-stop? ( control -- stop? )

      \ Get whether a floor is a downward stop
      method downward-stop? ( control -- stop? )

      \ Visit a floor
      method visit-floor ( control -- )

      \ Update the elevator direction
      method update-dir ( control -- )

      \ Scan floors above the elevator
      method scan-floors-above ( elevator -- found? )

      \ Scan floors below the elevator
      method scan-floors-below ( elevator -- found? )

      \ Scan for the closest floor
      method scan-closest-floor ( elevator -- floor|0 )

    end-class

    \ Implement the elevator controller class
    <control> begin-implement

      \ Initialize an elevator controller
      :noname { count display-action schedule control -- }
        control <object>->new
        count <state> control control-state init-object
        display-action control control-display-action !
        control [: current-data wait-control-msg ;] control control-action
        init-action
        schedule control control-action add-action
      ; define new

      \ Get the controller's action
      :noname { control -- action }
        control control-action
      ; define control-action@
      
      \ Wait for a control message
      :noname { control -- }
        [: drop 2drop current-data recv-control-msg ;]
        control control-recv-buf 2 cells recv-action
      ; define wait-control-msg
      
      \ Receive a control message
      :noname { control -- }
        control control-recv-buf @ case
          cycle-msg of control cycle-world endof
          floor-up-msg of
            control control-recv-buf cell+ @
            control control-state floor-up!
          endof
          floor-down-msg of
            control control-recv-buf cell+ @
            control control-state floor-down!
          endof
          floor-stop-msg of
            control control-recv-buf cell+ @
            control control-state floor-stop!
          endof
        endcase
        [: current-data wait-control-msg ;]
        control control-state <state> class-size
        control control-display-action @ send-action
      ; define recv-control-msg

      \ Cycle the world
      :noname { control -- }
        false control control-state door-open!
        control control-state half-floor? not if
          control update-dir
          control control-state elevator-dir@ case
            elevator-up-dir of
              true control control-state half-floor!
            endof
            elevator-down-dir of
              control control-state current-floor@ 1-
              control control-state current-floor!
              true control control-state half-floor!
            endof
            no-elevator-dir of control visit-floor endof
          endcase
        else
          false control control-state half-floor!
          control control-state elevator-dir@ case
            elevator-up-dir of
              control control-state current-floor@ 1+
              control control-state current-floor!
            endof
          endcase
          control update-dir
          control control-state elevator-dir@ case
            elevator-up-dir of
              control upward-stop? if control visit-floor then
            endof
            elevator-down-dir of
              control downward-stop? if control visit-floor then
            endof
            no-elevator-dir of control visit-floor endof
          endcase
        then
      ; define cycle-world

      \ Get whether a floor is an upward stop
      :noname { control -- stop? }
        control control-state current-floor@ { floor }
        floor control control-state floor-stop?
        floor control control-state floor-up? or
      ; define upward-stop?

      \ Get whether a floor is a downward stop
      :noname { control -- stop? }
        control control-state current-floor@ { floor }
        floor control control-state floor-stop?
        floor control control-state floor-down? or
      ; define downward-stop?

      \ Visit a floor
      :noname { control -- }
        control control-state current-floor@ { floor }
        floor control control-state floor-waiting? if
          floor control control-state clear-floor!
          true control control-state door-open!
        then
      ; define visit-floor

      \ Update the elevator direction
      :noname { control -- }
        control control-state elevator-dir@ case
          elevator-up-dir of
            control scan-floors-above not if
              control scan-floors-below if
                control visit-floor
                elevator-down-dir
              else
                no-elevator-dir
              then
            else
              elevator-up-dir
            then
          endof
          elevator-down-dir of
            control scan-floors-below not if
              control scan-floors-above if
                control visit-floor
                elevator-up-dir
              else
                no-elevator-dir
              then
            else
              elevator-down-dir
            then
          endof
          no-elevator-dir of
            control scan-closest-floor ?dup if
              control control-state current-floor@ > if
                elevator-up-dir
              else
                elevator-down-dir
              then
            else
              no-elevator-dir
            then
          endof
        endcase
        control control-state elevator-dir!
      ; define update-dir

      \ Scan floors above the elevator
      :noname { control -- found? }
        control control-state floor-count@ 1+
        control control-state current-floor@ 1+ ?do
          i control control-state floor-waiting? if true unloop exit then
        loop
        false
      ; define scan-floors-above

      \ Scan floors below the elevator
      :noname { control -- found? }
        control control-state current-floor@ 1 ?do
          i control control-state floor-waiting? if true unloop exit then
        loop
        false
      ; define scan-floors-below

      \ Scan for the closest floor
      :noname { control -- floor|0 }
        control control-state current-floor@ 1+ { floor-above }
        control control-state current-floor@ 1- { floor-below }
        begin
          floor-below 1 >=
          floor-above control control-state floor-count@ <= and
        while
          floor-above control control-state floor-waiting? if
            floor-above exit
          then
          floor-below control control-state floor-waiting? if
            floor-below exit
          then
          1 +to floor-above
          -1 +to floor-below
        repeat
        floor-below 0= if
          begin floor-above control control-state floor-count@ <= while
            floor-above control control-state floor-waiting? if
              floor-above exit
            then
            1 +to floor-above
          repeat
        else
          begin floor-below 1 >= while
            floor-below control control-state floor-waiting? if
              floor-below exit
            then
            -1 +to floor-below
          repeat
        then
        0
      ; define scan-closest-floor

    end-implement

    \ Cycle timer class
    <object> begin-class <cycle>

      \ The cycle action
      action-size member cycle-action

      \ The control action
      cell member cycle-control-action
      
      \ Message send buffer
      cell member cycle-send-buf

      \ Get the cycle timer's action
      method cycle-action@ ( cycle -- action )
      
      \ Wait for the next cycle
      method wait-cycle ( cycle -- )
      
      \ Send a cycle message
      method send-cycle-msg ( cycle -- )

    end-class

    \ Implement the cycle timer class
    <cycle> begin-implement

      \ Initialize the cycle timer
      :noname { control-action schedule cycle -- }
        cycle <object>->new
        control-action cycle cycle-control-action !
        cycle [: current-data send-cycle-msg ;] cycle cycle-action
        init-action
        schedule cycle cycle-action add-action
      ; define new

      \ Get the cycle timer's action
      :noname { cycle -- action }
        cycle cycle-action
      ; define cycle-action@

      \ Wait for the next cycle
      :noname { cycle -- }
        [: current-data send-cycle-msg ;] elevator-delay delay-action
      ; define wait-cycle

      \ Send a cycle message
      :noname { cycle -- }
        cycle-msg cycle cycle-send-buf !
        [: current-data wait-cycle ;]
        cycle cycle-send-buf cell cycle cycle-control-action @ send-action
      ; define send-cycle-msg
      
    end-implement

    \ User input class
    <object> begin-class <input>

      \ The input action
      action-size member input-action

      \ The control action
      cell member input-control-action
      
      \ Message send buffer
      2 cells member input-send-buf

      \ Floor count
      cell member input-floor-count
      
      \ Floor input
      cell member floor-input

      \ Get the user input's action
      method input-action@ ( input -- action )
      
      \ Poll for keypresses
      method poll-key ( input -- )

      \ Handle input
      method handle-input ( msg input -- )
      
    end-class

    \ Implement the user input class
    <input> begin-implement

      \ Initialize the user input
      :noname { count control-action schedule input -- }
        count 0> averts x-out-of-range-floor-count
        count max-floor-count <= averts x-out-of-range-floor-count
        input <object>->new
        control-action input input-control-action !
        count input input-floor-count !
        0 input floor-input !
        input [: current-data poll-key ;] input input-action
        init-action
        schedule input input-action add-action
      ; define new

      \ Get the user input's action
      :noname { input -- action }
        input input-action
      ; define input-action@
      
      \ Poll for keypresses
      :noname { input -- }
        key? if
          key { input-key }
          input-key [char] 1 >=
          input-key [char] 0 input input-floor-count @ + <= and if
            input-key [char] 0 - input floor-input ! true
          else
            input-key [char] q = if
              current-schedule stop-schedule true
            else
              input floor-input @ 0> if
                input-key case
                  [char] u of floor-up-msg input handle-input false endof
                  [char] d of floor-down-msg input handle-input false endof
                  [char] s of floor-stop-msg input handle-input false endof
                  true swap
                endcase
              else
                true
              then
            then
          then
        else
          true
        then
        if [: current-data poll-key ;] yield-action then
      ; define poll-key

      \ Handle input
      :noname { msg input -- }
        msg input input-send-buf !
        input floor-input @ input input-send-buf cell+ !
        0 input floor-input !
        [: current-data poll-key ;]
        input input-send-buf 2 cells input input-control-action @ send-action
      ; define handle-input
      
    end-implement
    
  end-module> import

  \ The world class
  <object> begin-class <world>

    continue-module elevator-internal

      \ The schedule
      schedule-size member world-schedule

      \ The display
      <display> class-size member world-display

      \ The controller
      <control> class-size member world-control

      \ The cycle timer
      <cycle> class-size member world-cycle

      \ The user inut
      <input> class-size member world-input

    end-module

    \ Run the world
    method run-world ( world -- )
      
  end-class

  \ Implement the world class
  <world> begin-implement

    \ Initialize the world with a specified number of floors
    :noname { count world -- }
      count 0> averts x-out-of-range-floor-count
      count max-floor-count <= averts x-out-of-range-floor-count
      world <object>->new
      world world-schedule init-schedule
      world world-schedule <display> world world-display init-object
      count world world-display display-action@ world world-schedule <control>
      world world-control init-object
      world world-control control-action@ world world-schedule <cycle>
      world world-cycle init-object
      count world world-control control-action@ world world-schedule <input>
      world world-input init-object
    ; define new

    \ Run the world
    :noname { world -- }
      reset-ansi-term
      0 0 go-to-coord
      erase-end-of-line
      erase-down
      world world-schedule run-schedule
    ; define run-world
    
  end-implement

  \ Run the test with a specified number of floors
  : init-test { count -- }
    count <world> [: { world }
      world run-world
    ;] with-object
  ;

end-module
