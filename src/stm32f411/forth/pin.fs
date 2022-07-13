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
\ SOFTWARE.

compile-to-flash

begin-module pin
  
  internal import
  gpio import

  \ Pin out of range exception
  : x-pin-out-of-range ." pin out of range" cr ;

  \ GPIO out of range exception
  : x-gpio-out-of-range ." gpio out of range" cr ;

  \ Alternate function out of range exception
  : x-alternate-out-of-range ." alternate function out of range" cr ;

  begin-module pin-internal

    \ GPIO pin count
    16 constant pin-count

    \ GPIO count
    9 constant gpio-count

    \ Alternate function count
    16 constant alternate-count

    \ Create a pin
    : make-gpio ( index "name" -- )
\      <builds , does> over pin-count u< averts x-pin-out-of-range
\      @ 16 lshift or
      token dup 0<> averts x-token-expected
      start-compile lit, 16 lit, postpone lshift postpone or
      inlined visible end-compile,
    ;

    \ Validate a pin
    : validate-pin ( pin -- )
      dup $FFFF and pin-count u< averts x-pin-out-of-range
      16 rshift gpio-count u< averts x-gpio-out-of-range
    ;

    \ Validate an alternate function
    : validate-alternate alternate-count u< averts x-alternate-out-of-range ;
    
    \ Extract a GPIO
    : extract-gpio ( pin -- gpio )
      16 rshift 10 lshift ( 16 rshift $400 * ) [ gpio-internal ] :: GPIO_Base +
    ;

    \ Extract a pin
    : extract-pin ( pin -- gpio-pin )
      $FFFF and
    ;

    \ Extract both a pin and a GPIO
    : extract-both ( pin -- gpio-pin gpio )
      dup extract-pin swap extract-gpio
    ;

  end-module> import

  \ Declare the GPIO's
  0 make-gpio XA
  1 make-gpio XB
  2 make-gpio XC
  3 make-gpio XD
  4 make-gpio XE
  7 make-gpio XH

  \ Declare high and low constants
  true constant high
  false constant low

  \ Set a pin to input
  : input-pin ( pin -- )
    dup validate-pin
    dup extract-gpio gpio-clock-enable
    INPUT_MODE swap extract-both MODER!
  ;
  
  \ Set a pin to output
  : output-pin ( pin -- )
    dup validate-pin
    dup extract-gpio gpio-clock-enable
    OUTPUT_MODE swap extract-both MODER!
  ;

  \ Set an alternate function
  : alternate-pin ( function pin -- )
    dup validate-pin
    over validate-alternate
    extract-both 2dup 2>r AFR!
    ALTERNATE_MODE 2r> MODER!
  ;
  
  \ Set a pin to pull-up
  : pull-up-pin ( pin -- )
    dup validate-pin
    dup extract-gpio gpio-clock-enable
    PULL_UP swap extract-both PUPDR!
  ;

  \ Set a pin to pull-down
  : pull-down-pin ( pin -- )
    dup validate-pin
    dup extract-gpio gpio-clock-enable
    PULL_DOWN swap extract-both PUPDR!
  ;

  \ Set a pin to floating
  : floating-pin ( pin -- )
    dup validate-pin
    dup extract-gpio gpio-clock-enable
    NO_PULL_UP_PULL_DOWN swap extract-both PUPDR!
  ;

  \ Set a pin to have a slow slew rate
  : slow-pin ( pin -- )
    dup validate-pin
    dup extract-gpio gpio-clock-enable
    LOW_SPEED swap extract-both OSPEEDR!
  ;

  \ Set a pin to have a high slew rate
  : fast-pin ( pin -- )
    dup validate-pin
    dup extract-gpio gpio-clock-enable
    VERY_HIGH_SPEED swap extract-both OSPEEDR!
  ;

  \ Set the output of a pin
  : pin! ( state pin -- )
    dup validate-pin
    extract-both BSRR!
  ;

  \ Get the input of a pin
  : pin@ ( pin -- state )
    dup validate-pin
    extract-both IDR@
  ;

  \ Get the output of a pin
  : pin-out@ ( pin -- state )
    dup validate-pin
    extract-both ODR@
  ;

  \ Toggle a pin
  : toggle-pin ( pin -- )
    dup validate-pin
    extract-both 2dup ODR@ not -rot BSRR!
  ;

end-module

reboot
