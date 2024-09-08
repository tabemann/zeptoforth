\ Copyright (c) 2023-2024 Travis Bemann
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

begin-module timer1

  interrupt import

  \ Out of range alarm index
  : x-out-of-range-alarm ." out of range alarm" cr ;
  
  begin-module timer1-internal

    \ Timer count
    4 constant timer-count

    \ Timer 1 base address
    $400B8000 constant TIMER1_Base

    \ Time write high bits; always write TIMELW first
    TIMER1_Base $00 + constant TIMEHW

    \ Time write low bits
    TIMER1_Base $04 + constant TIMELW

    \ Configure an alarm to fire at a given 32-bit time
    : ALARM ( index -- addr ) cells TIMER1_Base $10 + + ;

    \ The armed state of alarms
    TIMER1_Base $20 + constant ARMED
    
    \ Time read high bits (no side effects)
    TIMER1_Base $24 + constant TIMERAWH

    \ Time read low bits (no side effects)
    TIMER1_Base $28 + constant TIMERAWL

    \ Pause the timer
    TIMER1_Base $30 + constant TIME_PAUSE

    \ Timer locked bit
    TIMER1_Base $34 + constant LOCKED

    \ TImer source bit
    TIMER1_Base $38 + constant TIME_SOURCE
    
    \ Raw timer interrupts
    TIMER1_Base $3C + constant INTR

    \ Interrupt enable
    TIMER1_Base $40 + constant INTE

    \ Interrupt status after masking and forcing
    TIMER1_Base $44 + constant INTS

    \ Validate an alarm
    : validate-alarm ( alarm -- ) timer-count u< averts x-out-of-range-alarm ;

    \ Timer IRQ
    : timer-irq ( alarm -- irq ) 4 + ;

    \ Timer vector
    : timer-vector ( alarm -- vector ) timer-irq 16 + ;

    \ Initialize the timers
    : init-timer ( -- )
      disable-int
      [: 0 bit INTR ! 0 timer-irq NVIC_ICPR_CLRPEND! ;] 0 timer-vector vector!
      [: 1 bit INTR ! 1 timer-irq NVIC_ICPR_CLRPEND! ;] 1 timer-vector vector!
      [: 2 bit INTR ! 2 timer-irq NVIC_ICPR_CLRPEND! ;] 2 timer-vector vector!
      [: 3 bit INTR ! 3 timer-irq NVIC_ICPR_CLRPEND! ;] 3 timer-vector vector!
      timer-count 0 ?do
        0 i timer-irq NVIC_IPR_IP!
        i timer-irq NVIC_ISER_SETENA!
        i bit INTE bis!
      loop
      enable-int
    ;
    
  end-module> import

  \ Get the low bits in the current time in microseconds
  : us-counter-lsb ( -- us ) TIMERAWL @ ;

  \ Get the current time in microseconds
  : us-counter ( -- us-d )
    begin
      TIMERAWL @ TIMERAWH @ TIMERAWL @
      rot over u<=
    until
    swap
  ;

  \ Set the microsecond counter
  : us-counter! { lo hi -- }
    lo TIMELW ! hi TIMEHW !
  ;

  \ Pause the microsecond counter
  : pause-us ( -- ) 0 bit TIME_PAUSE bis! ;

  \ Unpause the microsecond counter
  : unpause-us ( -- ) 0 bit TIME_PAUSE bic! ;

  \ Delay until a microsecond
  : delay-until-us ( us-d -- )
    begin
      2dup us-counter du<=
    until
    2drop
  ;

  \ Delay a given number of microseconds
  : delay-us ( us-d -- )
    us-counter d+ delay-until-us
  ;

  \ Set an alarm at a time
  : set-alarm { us xt index -- }
    index validate-alarm
    index bit ARMED !
    xt index timer-vector vector!
    us index ALARM !
  ;

  \ Clear an alarm
  : clear-alarm { index -- }
    index validate-alarm
    index bit ARMED !
    index bit INTR bic!
  ;

  \ Clear an alarm interrupt
  : clear-alarm-int { index -- }
    index validate-alarm
    index bit INTR !
    index timer-irq NVIC_ICPR_CLRPEND!
  ;
  
  \ Lock the timer
  : lock-timer ( -- )
    1 LOCKED bis!
  ;

  \ Is the timer locked?
  : timer-locked? ( -- locked? )
    LOCKED @ 1 and 0<>
  ;

  \ Set the timer to use the system clock as a source
  : sysclk-timer ( -- )
    1 TIME_SOURCE bis!
  ;

  \ Set the timer to use the 1 MHz tick as a source
  : tick-timer ( -- )
    1 TIME_SOURCE bic!
  ;

  \ Is the timer set to use the system clock as a source
  : timer-sysclk? ( -- sysclk? )
    TIME_SOURCE @ 1 and 0<>
  ;

end-module

\ Initialize
: init init timer1::timer1-internal::init-timer ;

reboot
