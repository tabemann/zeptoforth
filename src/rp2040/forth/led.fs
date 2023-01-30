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

begin-module led

  pin import

  \ The LED states
  low constant off
  high constant on

  \ The LED constants
  0 constant green
  1 constant red
  2 constant blue

  \ Out of range LED exception
  : x-led-out-of-range ( -- ) ." led out of range" cr ;

  begin-module led-internal

    \ Is this a Seeed XIAO RP2040 board
    variable xiao?
    
    \ Detect Seeed XIAO RP2040 boards
    : detect-xiao ( -- flag )
      s" platform-xiao" find dup if >xt execute else drop false then
    ;

    \ The LED count
    : led-count ( -- count ) xiao? @ if 3 else 1 then ;

    \ Validate an LED
    : validate-led ( led -- ) led-count u< averts x-led-out-of-range ;

    \ Get the pin of an LED
    : pin-of-led ( led -- pin )
      xiao? @ if
        case
          0 of 16 endof
          1 of 17 endof
          2 of 25 endof
        endcase
      else
        drop 25
      then
    ;
    
  end-module> import

  \ Initialize the LEDs
  : led-init ( -- )
    detect-xiao xiao? !
    xiao? @ if
      red pin-of-led output-pin
      green pin-of-led output-pin
      blue pin-of-led output-pin
      high red pin-of-led pin!
      high green pin-of-led pin!
      high blue pin-of-led pin!
    else
      green pin-of-led output-pin
      low green pin-of-led pin!
    then
  ;

  \ Set an LED
  : led! ( state led -- )
    dup validate-led
    swap xiao? @ xor swap pin-of-led pin!
  ;

  \ Get an LED
  : led@ ( led -- state )
    dup validate-led
    pin-of-led pin-out@ xiao? @ xor
  ;

  \ Toggle an LED
  : toggle-led ( led -- )
    dup validate-led
    pin-of-led toggle-pin
  ;
  
end-module> import

\ Init
: init ( -- )
  init
  led-init
;

\ Reboot
reboot
