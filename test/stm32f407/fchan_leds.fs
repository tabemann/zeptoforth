\ Copyright (c) 2021 Travis Bemann
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

continue-module forth-module

  task-module import
  fchan-module import
  led-module import

  \ Our fchannels
  fchan-size buffer: request-fchan
  fchan-size buffer: red-response-fchan
  fchan-size buffer: orange-response-fchan
  fchan-size buffer: green-response-fchan
  fchan-size buffer: blue-response-fchan

  \ Our tasks
  variable handle-led-task
  variable request-red-task
  variable request-orange-task
  variable request-green-task
  variable request-blue-task

  \ The blinker delay
  variable blinker-delay
  
  \ Select the red LED
  : select-red ( -- )
    led-red-on led-orange-off led-green-off led-blue-off
  ;

  \ Select the orange LED
  : select-orange ( -- )
    led-red-off led-orange-on led-green-off led-blue-off
  ;

  \ Select the green LED
  : select-green ( -- )
    led-red-off led-orange-off led-green-on led-blue-off
  ;

  \ Select the blue LED
  : select-blue ( -- )
    led-red-off led-orange-off led-green-off led-blue-on
  ;
  
  \ Handle LED requests
  : handle-led ( -- )
    begin
      [: request-fchan recv-fchan ;] extract-allot-cell
      dup case
	red-response-fchan of select-red endof
	orange-response-fchan of select-orange endof
	green-response-fchan of select-green endof
	blue-response-fchan of select-blue endof
      endcase
      0 [: rot send-fchan ;] provide-allot-cell
    again
  ;

  \ Create a request word
  : make-request ( fchannel "name" -- )
    <builds , does> @
    begin
      dup [: request-fchan send-fchan ;] provide-allot-cell
      blinker-delay @ ms
      [: 2 pick recv-fchan ;] extract-allot-cell drop
    again
  ;

  \ Create the request words
  red-response-fchan make-request request-red
  orange-response-fchan make-request request-orange
  green-response-fchan make-request request-green
  blue-response-fchan make-request request-blue
  
  \ Initialize the test
  : init-test ( -- )
    50 blinker-delay !
    request-fchan init-fchan
    red-response-fchan init-fchan
    orange-response-fchan init-fchan
    green-response-fchan init-fchan
    blue-response-fchan init-fchan
    0 ['] handle-led 320 128 512 spawn handle-led-task !
    0 ['] request-red 320 128 512 spawn request-red-task !
    0 ['] request-orange 320 128 512 spawn request-orange-task !
    0 ['] request-green 320 128 512 spawn request-green-task !
    0 ['] request-blue 320 128 512 spawn request-blue-task !
    handle-led-task @ run
    request-red-task @ run
    request-orange-task @ run
    request-green-task @ run
    request-blue-task @ run
  ;
  
end-module