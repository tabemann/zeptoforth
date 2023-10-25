\ Copyright (c) 2020-2023 Travis Bemann
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

begin-module systick

  internal import
  interrupt import
  
  begin-module systick-internal

    \ Interrupt Control and Status Register
    $E000ED04 constant ICSR

    \ Pending SysTick Clear bit
    25 bit constant ICSR_PENDSTCLR
    
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

    compress-flash

  end-module> import
  
  \ SysTick vector index
  15 constant systick-vector

  \ SysTick handler
  : systick-handler ( -- )
    SYST_CSR @ SYST_CSR_COUNTFLAG and if
      cpu-index 0= if
	1 systick-counter +!
      then
      wake
    then
  ;

  commit-flash

  \ Turn on systick
  : enable-systick ( -- )
    SYST_CSR_TICKINT SYST_CSR_ENABLE or SYST_CSR bis! dmb dsb isb
  ;  

  \ Turn off systick
  : disable-systick ( -- )
    SYST_CSR_TICKINT SYST_CSR_ENABLE or SYST_CSR bic! dmb dsb isb
  ;

  \ Initialize SysTick
  : init-systick ( -- )
    ['] systick-handler systick-vector vector!
    SYST_CALIB @ SYST_CALIB_TENMS and
    10 / systick-divisor / time-multiplier * time-divisor / 1- SYST_RVR !
    0 SYST_CVR !
    0 systick-counter !
    enable-systick
  ;

  \ Initialize SysTick for an auxiliary core
  : init-systick-aux-core ( -- )
    SYST_CALIB @ SYST_CALIB_TENMS and
    10 / systick-divisor / time-multiplier * time-divisor / 1- SYST_RVR !
    0 SYST_CVR !
    enable-systick
  ;

  \ Systick divisor
  : systick-divisor ( -- ) systick-divisor ;
  
  \ Make systick-counter read-only
  : systick-counter ( -- u ) systick-counter @ ;

end-module> import

internal import

commit-flash

\ Init
: init ( -- )
  init
  init-systick
;

\ Wait for n milliseconds
: ms ( u -- )
  systick-divisor * systick-counter
  begin
    dup systick-counter swap - 2 pick u<
  while
    pause
  repeat
  drop drop
;

end-compress-flash

\ Reboot
reboot

