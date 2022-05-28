\ Copyright (c) 2020-2022 Travis Bemann
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

  \ The LED constants
  0 constant green

  \ The LED states
  low constant off
  high constant on

  \ Out of range LED exception
  : x-led-out-of-range ( -- ) ." led out of range" cr ;

  begin-module led-internal

    \ The LED count
    1 constant led-count

    \ Validate an LED
    : validate-led ( led -- ) led-count u< averts x-led-out-of-range ;

    \ Get the pin of an LED
    : pin-of-led ( led -- pin )
      drop 25
    ;
    
  end-module> import

  \ Initialize the LEDs
  : led-init ( -- )
    green pin-of-led output-pin
  ;

  \ Set an LED
  : led! ( state led -- )
    dup validate-led
    pin-of-led pin!
  ;

  \ Get an LED
  : led@ ( led -- state )
    dup validate-led
    pin-of-led pin-out@
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
