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

\ Set up the wordlist
forth-wordlist 1 set-order
forth-wordlist set-current

\ Test to make sure this has not already been compiled
defined? systick-wordlist not [if]

  \ Compile to flash
  compile-to-flash

  wordlist constant systick-wordlist
  wordlist constant systick-internal-wordlist
  forth-wordlist internal-wordlist systick-wordlist
  systick-internal-wordlist 4 set-order
  systick-internal-wordlist set-current

  \ RW SysTick Control and Status Register
  $E000E010 constant SYST_CSR

  \ SysTick count flag mask
  1 16 lshift constant SYST_CSR_COUNTFLAG

  \ SysTick clock source flag mask
  1 2 lshift constant SYST_CSR_CLKSOURCE

  \ SysTick tick interrupt flag mask
  1 1 lshift constant SYST_CSR_TICKINT

  \ SysTick tick enable flag mask
  1 0 lshift constant SYST_CSR_ENABLE

  \ RW SysTick Reload Value Register (24 bit)
  $E000E014 constant SYST_RVR

  \ RW SysTick Current Value Register (24 bit)
  $E000E018 constant SYST_CVR

  \ RO SysTick Calibration Register
  $E000E01C constant SYST_CALIB

  \ SysTick no reference counter flag mask
  1 31 lshift constant SYST_CALIB_NOREF

  \ SysTick TENMS field is inexact flag mask
  1 30 lshift constant SYST_CALIB_SKEW

  \ SysTick value for 10 ms (100 Hz) timing, subject to skew errors mask
  $00FFFFFF constant SYST_CALIB_TENMS

  \ SysTick counter
  variable systick-counter

  \ Set non-internal
  systick-wordlist set-current

  \ SysTick handler
  : systick-handler ( -- )
    SYST_CSR @ SYST_CSR_COUNTFLAG and if
      systick-counter @ 1+ systick-counter ! wake
    then
  ;

  \ Turn on systick
  : enable-systick ( -- )
    SYST_CSR_TICKINT SYST_CSR_ENABLE or SYST_CSR bis! dsb isb
  ;  

  \ Turn off systick
  : disable-systick ( -- )
    SYST_CSR_TICKINT SYST_CSR_ENABLE or SYST_CSR bic! dsb isb
  ;

  \ Make systick-counter read-only
  : systick-counter ( -- u ) systick-counter @ ;

  \ Reset current wordlist
  forth-wordlist set-current

  \ Init
  : init ( -- )
    init
    ['] systick-handler systick-handler-hook !
    SYST_CALIB @ SYST_CALIB_TENMS and
    10 / systick-divisor / time-multiplier * time-divisor / SYST_RVR !
    0 SYST_CVR !
    0 systick-counter !
    enable-systick
  ;

  \ Wait for n milliseconds
  : ms ( u -- )
    systick-divisor * systick-counter @
    begin
      dup systick-counter @ swap - 2 pick u<
    while
      pause
    repeat
    drop drop
  ;

[then]

\ Reboot
reboot
