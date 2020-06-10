\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.

\ Compile to flash
compile-to-flash

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

\ SysTick handler
: systick-handler ( -- )
  SYST_CSR @ SYST_CSR_COUNTFLAG @ and if
    systick-counter @ 1+ systick-counter !
  then
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

\ Turn on systick
: enable-systick ( -- )
  ['] systick-handler systick-handler-hook !
  SYST_CSR_TICKINT SYST_CSR_ENABLE or SYST_CSR bis!
;  

\ Turn off systick
: disable-systick ( -- )
  SYST_CSR_TICKINT SYST_CSR_ENABLE or SYST_CSR bic!
  0 systick-handler-hook !
;

\ Init
: init ( -- )
  init
  SYST_CALIB @ SYST_CALIB_TENMS and
  10 / systick-divisor / time-multiplier * time-divisor / SYST_RVR !
  0 SYST_CVR !
  0 systick-counter !
  enable-systick
;

\ Make systick-counter read-only
: systick-counter ( -- u ) systick-counter @ ;

\ Warm reboot
warm