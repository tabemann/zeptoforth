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

\ Compile this to flash
compile-to-flash

\ Set up the wordlist order
forth-wordlist task-wordlist led-wordlist 3 set-order
forth-wordlist set-current

\ The blinker delay time
variable blinker-delay

\ The blinker
: blinker ( -- )
  led-red-on
  begin
    pause
    blinker-delay @ ms
    led-red-off
    led-green-on
    pause
    blinker-delay @ ms
    led-green-off
    led-red-on
  again
;

\ The blinker task
variable blinker-task

\ Init
: init ( -- )
  init
  500 blinker-delay !
  ['] blinker 256 256 256 spawn blinker-task !
  blinker-task @ enable-task
;

\ Reboot to initialize
reboot
