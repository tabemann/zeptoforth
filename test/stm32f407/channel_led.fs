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

continue-module forth

  internal import
  task import
  chan import
  led import

  \ My channel size
  2 constant my-chan-size

  \ Allot the channels
  1 my-chan-size chan-size buffer: orange-chan
  1 my-chan-size chan-size buffer: green-chan
  1 my-chan-size chan-size buffer: blue-chan
  1 my-chan-size chan-size buffer: red-chan

  \ Initialize the channels
  1 my-chan-size orange-chan init-chan
  1 my-chan-size green-chan init-chan
  1 my-chan-size blue-chan init-chan
  1 my-chan-size red-chan init-chan

  \ The led structure
  begin-structure led-header-size
    \ The before channel
    field: before-chan

    \ The after channel
    field: after-chan

    \ The color
    field: color

    \ The delay
    field: delay-ms
  end-structure

  \ The inner loop of an led handler
  : do-led ( -- )
    does> begin
      [: 2 pick before-chan @ recv-chan ;] extract-allot-byte drop
      dup color @ on swap led!
      dup delay-ms @ ms
      dup color @ off swap led!
      0 [: 2 pick after-chan @ send-chan ;] provide-allot-byte
      pause
    again
  ;

  \ Create an led task
  : make-led ( before-chan after-chan color delay "name" -- )
    s" " <builds-with-name 3 roll , rot , swap , , do-led
    0 latest >body 420 128 512 spawn constant
  ;

  \ Create the led tasks
  orange-chan green-chan orange 100 make-led orange-task
  green-chan blue-chan green 100 make-led green-task
  blue-chan red-chan blue 100 make-led blue-task
  red-chan orange-chan red 100 make-led red-task

  \ Initialize the blinky
  : init-blinky ( -- )
    orange-task run
    green-task run
    blue-task run
    red-task run
    pause
    0 [: orange-chan send-chan ;] provide-allot-byte
    pause
  ;

end-module
