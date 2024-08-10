\ Copyright (c) 2022-2023 Travis Bemann
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

  gpio import
  
  \ Pin out of range exception
  : x-pin-out-of-range ." pin out of range" cr ;

  \ Alternate function out of range exception
  : x-alternate-out-of-range ." alternate function out of range" cr ;

  begin-module pin-internal

    \ Pin count
    48 constant pin-count

    \ Alternate function count
    12 constant alternate-count

    \ Validate a pin
    : validate-pin pin-count u< averts x-pin-out-of-range ;

    \ Validate an alternate function
    : validate-alternate alternate-count u< averts x-alternate-out-of-range ;
    
  end-module> import

  \ Declare high and low constants
  true constant high
  false constant low

  \ Set a pin to input
  : input-pin ( pin -- )
    dup validate-pin
    true over PADS_BANK0_IE!
    true over PADS_BANK0_OD!
    5 over GPIO_CTRL_FUNCSEL!
    bit GPIO_OE_CLR !
  ;
  
  \ Set a pin to output
  : output-pin ( pin -- )
    dup validate-pin
    false over PADS_BANK0_IE!
    false over PADS_BANK0_OD!
    5 over GPIO_CTRL_FUNCSEL!
    bit GPIO_OE_SET !
  ;

  \ Set an alternate function
  : alternate-pin ( function pin -- )
    dup validate-pin
    over validate-alternate
    true over PADS_BANK0_IE!
    false over PADS_BANK0_OD!
    GPIO_CTRL_FUNCSEL!
  ;
  
  \ Set a pin to pull-up
  : pull-up-pin ( pin -- )
    dup validate-pin
    true over PADS_BANK0_PUE!
    false swap PADS_BANK0_PDE!
  ;

  \ Set a pin to pull-down
  : pull-down-pin ( pin -- )
    dup validate-pin
    true over PADS_BANK0_PDE!
    false swap PADS_BANK0_PUE!
  ;

  \ Set a pin to floating
  : floating-pin ( pin -- )
    dup validate-pin
    false over PADS_BANK0_PUE!
    false swap PADS_BANK0_PDE!
  ;

  \ Set a pin to have a slow slew rate
  : slow-pin ( pin -- )
    dup validate-pin
    false swap PADS_BANK0_SLEWFAST!
  ;

  \ Set a pin to have a high slew rate
  : fast-pin ( pin -- )
    dup validate-pin
    true swap PADS_BANK0_SLEWFAST!
  ;

  \ Set the output of a pin
  : pin! ( state pin -- )
    dup validate-pin
    dup 32 < if
      bit swap if GPIO_OUT_SET else GPIO_OUT_CLR then !
    else
      32 - bit swap if GPIO_HI_OUT_SET else GPIO_HI_OUT_CLR then !
    then
  ;

  \ Get the input of a pin
  : pin@ ( pin -- state )
    dup validate-pin
    dup 32 < if
      bit GPIO_IN bit@
    else
      32 - bit GPIO_HI_IN bit@
    then
  ;

  \ Get the output of a pin
  : pin-out@ ( pin -- state )
    dup validate-pin
    dup 32 < if
      bit GPIO_OUT bit@
    else
      32 - bit GPIO_HI_OUT bit@
    then
  ;

  \ Toggle a pin
  : toggle-pin ( pin -- )
    dup validate-pin
    dup 32 < if
      bit GPIO_OUT_XOR !
    else
      32 - bit GPIO_HI_OUT_XOR !
    then
  ;

end-module

reboot