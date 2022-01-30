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

  systick import
  task import
  schedule import
  led import
  
  \ Our task
  variable schedule-task

  \ Our schedule
  schedule-size buffer: my-schedule

  \ Our actions
  action-size buffer: red-green-action
  action-size buffer: orange-blue-action

  \ Our delays
  variable red-delay
  variable orange-delay
  variable green-delay
  variable blue-delay

  \ Red-green systick start
  variable red-green-start

  \ Orange-blue systick start
  variable orange-blue-start

  \ Red-green action word
  : do-red-green ( -- )
    systick-counter red-green-start !
    begin
      led-red-on led-green-off
      red-green-start @ red-delay @ + red-green-start !
      begin
	systick-counter red-green-start @ - red-delay @ < if
	  50 current-action start-action-delay
	  yield
	  led-green-on
	  2 current-action start-action-delay
	  yield
	  led-green-off
	  false
	else
	  true
	then
      until
      yield
      led-green-on led-red-off
      red-green-start @ green-delay @ + red-green-start !
      begin
	systick-counter red-green-start @ - green-delay @ < if
	  50 current-action start-action-delay
	  yield
	  led-red-on
	  2 current-action start-action-delay
	  yield
	  led-red-off
	  false
	else
	  true
	then
      until
      yield
    again
  ;

  \ Orange-blue action word
  : do-orange-blue ( -- )
    systick-counter orange-blue-start !
    begin
      led-orange-on led-blue-off
      orange-blue-start @ orange-delay @ + orange-blue-start !
      begin
	systick-counter orange-blue-start @ - orange-delay @ < if
	  50 current-action start-action-delay
	  yield
	  led-blue-on
	  2 current-action start-action-delay
	  yield
	  led-blue-off
	  false
	else
	  true
	then
      until
      yield
      led-blue-on led-orange-off
      orange-blue-start @ blue-delay @ + orange-blue-start !
      begin
	systick-counter orange-blue-start @ - blue-delay @ < if
	  50 current-action start-action-delay
	  yield
	  led-orange-on
	  2 current-action start-action-delay
	  yield
	  led-orange-off
	  false
	else
	  true
	then
      until
      yield
    again
  ;

  \ Initialize the test
  : init-test ( -- )
    3333 red-delay !
    3333 blue-delay !
    3333 green-delay !
    3333 orange-delay !
    my-schedule init-schedule
    ['] do-red-green red-green-action my-schedule init-action
    ['] do-orange-blue orange-blue-action my-schedule init-action
    0 [: my-schedule run-schedule ;] 320 128 512 spawn schedule-task !
    1666 orange-blue-action start-action-delay
    red-green-action enable-action
    orange-blue-action enable-action
    schedule-task @ run
    pause
  ;

end-module
