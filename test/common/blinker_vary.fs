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

  task import

  \ The blinker variation delay
  variable vary-delay

  \ The blinker variation minimum blinker delay
  variable vary-min

  \ The blinker variation maximum blinker delay
  variable vary-max

  \ The blinker variation blinker delay step
  variable vary-step

  \ The blinker variation routine
  : vary ( -- )
    begin
      vary-min @ blinker-delay !
      begin
	blinker-delay @ vary-max @ <
      while
	vary-delay @ ms
	blinker-delay @ vary-step @ + blinker-delay !
      repeat
      vary-max @ blinker-delay !
      begin
	blinker-delay @ vary-min @ >
      while
	vary-delay @ ms
	blinker-delay @ vary-step @ - blinker-delay !
      repeat
    again
  ;

  \ The blinker variation task
  variable vary-task

  \ Init
  : init-vary ( -- )
    100 vary-delay !
    50 vary-min !
    500 vary-max !
    25 vary-step !
    0 ['] vary 320 128 512 spawn vary-task !
    vary-task @ run
  ;

end-module