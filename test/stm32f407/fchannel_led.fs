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

continue-module forth

  internal import
  task import
  fchan import
  led import

  \ Allot the channels
  fchan-size buffer: orange-fchan
  fchan-size buffer: green-fchan
  fchan-size buffer: blue-fchan
  fchan-size buffer: red-fchan

  \ Initialize the channels
  orange-fchan init-fchan
  green-fchan init-fchan
  blue-fchan init-fchan
  red-fchan init-fchan

  \ The led structure
  begin-structure led-header-size
    \ The before fast channel
    field: before-fchan

    \ The after fast channel
    field: after-fchan

    \ The colork
    field: color

    \ The delay
    field: delay-ms
  end-structure

  \ The inner loop of an led handler
  : do-led ( -- )
    does> begin
      [: 2 pick before-fchan @ recv-fchan ;] extract-allot-cell drop
      dup color @ on swap led!
      dup delay-ms @ ms
      dup color @ off swap led!
      0 [: 2 pick after-fchan @ send-fchan ;] provide-allot-cell
      pause
    again
  ;

  \ Create an led task
  : make-led ( before-fchan after-fchan color delay "name" -- )
    s" " <builds-with-name 3 roll , rot , swap , , do-led
    0 latest >body 420 128 512 spawn dup 1 swap task-priority! constant
  ;

  \ Create the led tasks
  orange-fchan green-fchan orange 100 make-led orange-task
  green-fchan blue-fchan green 100 make-led green-task
  blue-fchan red-fchan blue 100 make-led blue-task
  red-fchan orange-fchan red 100 make-led red-task

  \ Initialize the blinky
  : init-blinky ( -- )
    orange-task run
    green-task run
    blue-task run
    red-task run
    pause
    0 [: orange-fchan send-fchan ;] provide-allot-cell
    pause
  ;

end-module
