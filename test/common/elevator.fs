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
    25000 constant elevator-delay

  end-module> import
  
  <object> begin-class <elevator>

    continue-module elevator-internal
      
      \ Floor count
      cell member floor-count

      \ Current floor
      cell member current-floor

      \ Half floor
      cell member half-floor?

      \ Elevator direction
      cell member elevator-dir

      \ Floor input
      cell member floor-input

      \ Floor flags
      max-floor-count cell align member floor-flags

      \ Collect input
      method collect-input ( elevator -- )
      
      \ Press the up button on a floor
      method floor-up! ( floor elevator -- )

      \ Press the down button on a floor
      method floor-down! ( floor elevator -- )

      \ Press the stop button on a floor
      method floor-stop! ( floor elevator -- )

      \ Draw the the simulator
      method draw-world ( elevator -- )

      \ Draw a floor
      method draw-floor ( floor elevator -- )

      \ Draw a half-floor
      method draw-half-floor ( floor elevator -- )

      \ Draw an elevator
      method draw-elevator ( elevator -- )
      
      \ Cycle the world
      method cycle-world ( elevator -- )

      \ Get flags for a floor
      method flags@ ( floor elevator -- flags )

      \ Set flags for a floor
      method flags! ( flags floor elevator -- )
      
      \ Get whether a floor is a upward stop
      method upward-stop? ( elevator -- stop? )

      \ Get whether a floor is a downward stop
      method downward-stop? ( elevator -- stop? )
      
      \ Visit a floor
      method visit-floor ( elevator -- )

      \ Update the elevator direction
      method update-dir ( elevator -- )

      \ Scan floors above the elevator
      method scan-floors-above ( elevator -- found? )

      \ Scan floors beneath the elevator
      method scan-floors-below ( elevator -- found? )

      \ Scan for the closest floor
      method scan-closest-floor ( elevator -- floor )

    end-module
    
    \ Run the world
    method run-world ( elevator -- )

  end-class

  <elevator> begin-implement

    \ Constructor
    :noname { count elevator -- }
      count max-floor-count <= averts x-out-of-range-floor-count
      elevator <object>->new
      count elevator floor-count !
      1 elevator current-floor !
      false elevator half-floor? !
      0 elevator floor-input !
      no-elevator-dir elevator elevator-dir !
      count 0 ?do 0 i elevator floor-flags + c! loop
    ; define new

    \ Run the world
    :noname { elevator -- }
      begin
        elevator draw-world
        systick-counter { start-systick }
        begin start-systick elevator-delay + systick-counter > while
          elevator collect-input
          -1 elevator floor-input @ = if
            0 elevator floor-input ! exit
          then
        repeat
        elevator cycle-world
      again
    ; define run-world

    \ Collect input
    :noname { elevator -- }
      key? if
        key { input }
        input [char] 1 >= input [char] 0 elevator floor-count @ + <= and if
          input [char] 0 - elevator floor-input !
        else
          input [char] q = if
            -1 elevator floor-input !
          else
            elevator floor-input @ 0> if
              input case
                [char] u of
                  elevator floor-input @ elevator floor-up!
                  0 elevator floor-input !
                  elevator draw-world
                endof
                [char] d of
                  elevator floor-input @ elevator floor-down!
                  0 elevator floor-input !
                  elevator draw-world
                endof
                [char] s of
                  elevator floor-input @ elevator floor-stop!
                  0 elevator floor-input !
                  elevator draw-world
                endof
              endcase
            then
          then
        then
      then
    ; define collect-input
    
    \ Press the up button on a floor
    :noname { floor elevator -- }
      floor-up floor elevator flags@ or floor elevator flags!
    ; define floor-up!

    \ Press the down button on a floor
    :noname { floor elevator -- }
      floor-down floor elevator flags@ or floor elevator flags!
    ; define floor-down!

    \ Press the stop button on a floor
    :noname { floor elevator -- }
      floor-stop floor elevator flags@ or floor elevator flags!
    ; define floor-stop!

    \ Draw the the simulator
    :noname { elevator -- }
      0 0 go-to-coord
      erase-end-of-line
      erase-down
      ."   LIFT SIMULATOR" cr
      ." #   UP DN STOPS  LIFT" cr
      ." ========================" cr
      1 elevator floor-count @ ?do
        i elevator draw-floor
        i 1 > if
          i 1- elevator draw-half-floor
        then
      -1 +loop
      ." ========================" cr      
    ; define draw-world

    \ Draw a floor
    :noname { floor elevator -- }
      floor elevator flags@ { flags }
      ."  " floor (.) ."   "
      flags floor-up and if ." #  " else ."    " then
      flags floor-down and if ." #    " else ."      " then
      flags floor-stop and if ." #   |" else ."     |" then
      floor elevator current-floor @ =
      elevator half-floor? @ not and if
        elevator draw-elevator
      else
        ."     "
      then
      ." |   " cr
    ; define draw-floor

    \ Draw a half-floor
    :noname { floor elevator -- }
      ." ----------------|"
      floor elevator current-floor @ =
      elevator half-floor? @ and if
        elevator draw-elevator
      else
        ."     "
      then
      ." |-- " cr
    ; define draw-half-floor

    \ Draw an elevator
    :noname { elevator -- }
      elevator elevator-dir @ case
        elevator-up-dir of ." [UP]" endof
        elevator-down-dir of ." [DN]" endof
        no-elevator-dir of ." [  ]" endof
      endcase
    ; define draw-elevator
    
    \ Cycle the world
    :noname { elevator -- }
      elevator half-floor? @ not if
        elevator update-dir
        elevator elevator-dir @ case
          elevator-up-dir of
            true elevator half-floor? !
          endof
          elevator-down-dir of
            -1 elevator current-floor +!
            true elevator half-floor? !
          endof
          no-elevator-dir of elevator visit-floor endof
        endcase
      else
        false elevator half-floor? !
        elevator elevator-dir @ case
          elevator-up-dir of
            1 elevator current-floor +!
          endof
        endcase
        elevator update-dir
        elevator elevator-dir @ case
          elevator-up-dir of
            elevator upward-stop? if elevator visit-floor then
          endof
          elevator-down-dir of
            elevator downward-stop? if elevator visit-floor then
          endof
          no-elevator-dir of elevator visit-floor endof
        endcase
      then
    ; define cycle-world

    \ Get flags for a floor
    :noname { floor elevator -- flags }
      floor 1- elevator floor-flags + c@
    ; define flags@

    \ Set flags for a floor
    :noname { flags floor elevator -- }
      flags floor 1- elevator floor-flags + c!
    ; define flags!
    
    \ Get whether a floor is a upward stop
    :noname { elevator -- stop? }
      elevator current-floor @ elevator flags@ { flags }
      flags floor-stop and flags floor-up and or
    ; define upward-stop?

    \ Get whether a floor is a downward stop
    :noname { elevator -- stop? }
      elevator current-floor @ elevator flags@ { flags }
      flags floor-stop and flags floor-down and or
    ; define downward-stop?
    
    \ Visit a floor
    :noname { elevator -- }
      0 elevator current-floor @ elevator flags!
    ; define visit-floor

    \ Update the elevator direction
    :noname { elevator -- }
      elevator elevator-dir @ case
        elevator-up-dir of
          elevator scan-floors-above not if
            elevator scan-floors-below if
              elevator visit-floor
              elevator-down-dir
            else
              no-elevator-dir
            then
          else
            elevator-up-dir
          then
        endof
        elevator-down-dir of
          elevator scan-floors-below not if
            elevator scan-floors-above if
              elevator visit-floor
              elevator-up-dir
            else
              no-elevator-dir
            then
          else
            elevator-down-dir
          then
        endof
        no-elevator-dir of
          elevator scan-closest-floor ?dup if
            elevator current-floor @ > if
              elevator-up-dir
            else
              elevator-down-dir
            then
          else
            no-elevator-dir
          then
        endof
      endcase
      elevator elevator-dir !
    ; define update-dir

    \ Scan floors above the elevator
    :noname { elevator -- found? }
      elevator floor-count @ 1+ elevator current-floor @ 1+ ?do
        i elevator flags@ if true unloop exit then
      loop
      false
    ; define scan-floors-above

    \ Scan floors beneath the elevator
    :noname { elevator -- found? }
      elevator current-floor @ 1 ?do
        i elevator flags@ if true unloop exit then
      loop
      false
    ; define scan-floors-below

    \ Scan for the closest floor
    :noname { elevator -- floor }
      elevator current-floor @ 1+ { floor-above }
      elevator current-floor @ 1- { floor-below }
      begin floor-below 1 >= floor-above elevator floor-count @ <= and while
        floor-above elevator flags@ if
          floor-above exit
        then
        floor-below elevator flags@ if
          floor-below exit
        then
        1 +to floor-above
        -1 +to floor-below
      repeat
      floor-below 0= if
        begin floor-above elevator floor-count @ <= while
          floor-above elevator flags@ if
            floor-above exit
          then
          1 +to floor-above
        repeat
      else
        begin floor-below 1 >= while
          floor-below elevator flags@ if
            floor-below exit
          then
          -1 +to floor-below
        repeat
      then
      0
    ; define scan-closest-floor
    
  end-implement

  \ Run the test
  : init-test { count -- }
    count <elevator> [: { elevator }
      elevator run-world
    ;] with-object
  ;
  
end-module