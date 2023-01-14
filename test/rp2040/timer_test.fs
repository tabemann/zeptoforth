\ Copyright (c) 2023 Travis Bemann
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

begin-module timer-test
  
  task import
  timer import
  led import
  
  \ Blink interval
  500000 constant blink-interval
  
  \ Blink the green LED at half second intervals
  : blinky ( -- )
    0 [:
      begin
        green toggle-led
        blink-interval s>d delay-us
      again
    ;] 320 128 512 spawn run
  ;
  
  \ Time for the next alarm
  variable next-alarm
  
  \ The alarm handler
  defer handle-alarm
  :noname
    0 clear-alarm-int
    green toggle-led
    blink-interval next-alarm +!
    next-alarm @ ['] handle-alarm 0 set-alarm
  ; ' handle-alarm defer!
  
  \ Blink the green LED at half second intervals using an alarm
  : alarm-blinky ( -- )
    us-counter-lsb blink-interval + next-alarm !
    next-alarm @ ['] handle-alarm 0 set-alarm
  ;

end-module