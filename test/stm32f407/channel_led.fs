\ Copyright (c) 2020-2021 Travis Bemann
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

begin-module forth-module

  import internal-module
  import task-module
  import chan-module
  import led-module

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

    \ The on xt
    field: on-xt

    \ The off xt
    field: off-xt

    \ The delay
    field: delay-ms
  end-structure

  \ The inner loop of an led handler
  : do-led ( -- )
    does> begin
      dup before-chan @ recv-chan-byte drop
      dup on-xt @ execute
      dup delay-ms @ ms
      dup off-xt @ execute
      dup after-chan @ 0 swap send-chan-byte
      pause
    again
  ;

  \ Create an led task
  : make-led ( before-chan after-chan on-xt off-xt delay "name" -- )
    s" " <builds-with-name 4 roll , 3 roll , rot , swap , , do-led
    latest >body 512 256 256 spawn constant
  ;

  \ Create the led tasks
  orange-chan green-chan ' led-orange-on ' led-orange-off 100
  make-led orange-task
  green-chan blue-chan ' led-green-on ' led-green-off 100 make-led green-task
  blue-chan red-chan ' led-blue-on ' led-blue-off 100 make-led blue-task
  red-chan orange-chan ' led-red-on ' led-red-off 100 make-led red-task

  \ Initialize the blinky
  : init-blinky ( -- )
    orange-task run
    green-task run
    blue-task run
    red-task run
    pause
    0 orange-chan send-chan-byte
    pause
  ;

end-module
