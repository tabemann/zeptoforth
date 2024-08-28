\ Copyright (c) 2024 Travis Bemann
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

compile-to-flash

begin-module watchdog

  \ Out of range watchdog delay exception
  : x-out-of-range-watchdog-delay ." out of range watchdog delay" cr ;

  begin-module watchdog-internal

    \ Resets base address
    rp2040? [if]
      $4000C000 constant RESETS_BASE
    [then]
    rp2350? [if]
      $40020000 constant RESETS_BASE
    [then]

    \ Watchdog peripheral reset enable register
    RESETS_BASE $4 + constant RESETS_WDSEL
    
    \ Reset all peripherals
    rp2040? [if]
      $01FFFFFF constant RESETS_ALL
    [then]
    rp2350? [if]
      $1FFFFFFF 3 bit bic constant RESETS_ALL
    [then]

    \ Watchdog base register
    rp2040? [if]
      $40058000 constant WATCHDOG_BASE
    [then]
    rp2350? [if]
      $400D8000 constant WATCHDOG_BASE
    [then]

    \ Watchdog control register
    WATCHDOG_BASE $0 + constant WATCHDOG_CTRL

    \ Watchdog load register
    WATCHDOG_BASE $4 + constant WATCHDOG_LOAD

    \ Watchdog reason register
    WATCHDOG_BASE $8 + constant WATCHDOG_REASON

    \ Watchdog scratch 4 register
    WATCHDOG_BASE $1C + constant WATCHDOG_SCRATCH4

    \ Watchdog trigger bit
    31 bit constant WATCHDOG_CTRL_TRIGGER
    
    \ Watchdog enable bit
    30 bit constant WATCHDOG_CTRL_ENABLE

    \ Watchdog force reason bit
    1 bit constant WATCHDOG_REASON_FORCE
    
    \ Watchdog timer reason bit
    0 bit constant WATCHDOG_REASON_TIMER

    \ Watchdog reload value
    variable watchdog-reload

    \ Watchdog is enabled
    variable watchdog-enabled

    \ Watchdog default value
    1_000_000 constant watchdog-delay-us-default

    \ Validate watchdog delay
    : validate-watchdog-delay-us ( us -- )
      dup 0 >= averts x-out-of-range-watchdog-delay
      $007FFFFF <= averts x-out-of-range-watchdog-delay
    ;
    
  end-module> import

  \ Update the watchdog timer
  : update-watchdog ( -- )
    watchdog-enabled @ if
      watchdog-reload @ WATCHDOG_LOAD !
    then
  ;

  \ Force a watchdog reboot
  : force-watchdog-reboot ( -- )
    WATCHDOG_CTRL_TRIGGER WATCHDOG_CTRL bis!
  ;
  
  \ Set the watchdog delay in microseconds
  : watchdog-delay-us! ( us -- )
    dup validate-watchdog-delay-us
    [ rp2040? ] [if] 1 lshift [then] watchdog-reload !
    watchdog-enabled @ if
      watchdog-reload @ 0= if
        force-watchdog-reboot
      else
        update-watchdog
      then
    then
  ;

  \ Enable the watchdog with the watchdog delay in microseconds
  : enable-watchdog ( -- )
    WATCHDOG_CTRL_ENABLE WATCHDOG_CTRL bic!
    true watchdog-enabled !
    watchdog-reload @ 0= if
      force-watchdog-reboot
    else
      update-watchdog
      WATCHDOG_CTRL_ENABLE WATCHDOG_CTRL bis!
    then
  ;

  \ Disable the watchdog
  : disable-watchdog ( -- )
    WATCHDOG_CTRL_ENABLE WATCHDOG_CTRL bic!
    false watchdog-enabled !
  ;

  \ Did the system reboot due to a watchdog force
  : reboot-reason-watchdog-force? ( -- force? )
    WATCHDOG_REASON_FORCE WATCHDOG_REASON bit@
  ;

  \ Did the system reboot due to a watchdog timeout
  : reboot-reason-watchdog-timer? ( -- timer? )
    WATCHDOG_REASON_TIMER WATCHDOG_REASON bit@
  ;

  \ Enable watchdog to be updated by multitasker
  : enable-multitasker-update-watchdog ( -- )
    ['] update-watchdog task::watchdog-hook !
  ;

  \ Disable watchdog from being updated by multitasker
  : disable-multitasker-update-watchdog ( -- )
    ['] update-watchdog task::watchdog-hook @ = if
      0 task::watchdog-hook !
    then
  ;

  \ Enable forcing reboot upon a hardware fault
  : enable-fault-watchdog-reboot ( -- )
    ['] force-watchdog-reboot exception::fault-hook !
  ;

  \ Disable forcing reboot upon a hardware fault
  : disable-fault-watchdog-reboot ( -- )
    ['] force-watchdog-reboot exception::fault-hook @ = if
      0 exception::fault-hook !
    then
  ;

  continue-module watchdog-internal

    \ Initialize watchdog
    : init-watchdog ( -- )
      0 WATCHDOG_SCRATCH4 ! \ Do a normal reboot on watchdog timeout
      RESETS_ALL RESETS_WDSEL !
      false watchdog-enabled !
      watchdog-delay-us-default [ rp2040? ] [if] 1 lshift [then]
      watchdog-reload !
      ['] update-watchdog task::watchdog-hook !
    ;
    
  end-module
  
end-module

initializer watchdog::watchdog-internal::init-watchdog

reboot
