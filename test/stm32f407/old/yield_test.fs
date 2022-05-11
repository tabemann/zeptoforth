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
      on red led! off green led!
      red-green-start @ red-delay @ + red-green-start !
      begin
	systick-counter red-green-start @ - red-delay @ < if
	  50 current-action start-action-delay
	  yield
	  on green led!
	  2 current-action start-action-delay
	  yield
	  off green led!
	  false
	else
	  true
	then
      until
      yield
      on green led! off red led!
      red-green-start @ green-delay @ + red-green-start !
      begin
	systick-counter red-green-start @ - green-delay @ < if
	  50 current-action start-action-delay
	  yield
	  on red led!
	  2 current-action start-action-delay
	  yield
	  off red led!
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
      on orange led! off blue led!
      orange-blue-start @ orange-delay @ + orange-blue-start !
      begin
	systick-counter orange-blue-start @ - orange-delay @ < if
	  50 current-action start-action-delay
	  yield
	  on blue led!
	  2 current-action start-action-delay
	  yield
	  off blue led!
	  false
	else
	  true
	then
      until
      yield
      on blue led! off orange led!
      orange-blue-start @ blue-delay @ + orange-blue-start !
      begin
	systick-counter orange-blue-start @ - blue-delay @ < if
	  50 current-action start-action-delay
	  yield
	  on orange led!
	  2 current-action start-action-delay
	  yield
	  off orange led!
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
    0 [: my-schedule run-schedule ;] 420 128 512 spawn schedule-task !
    1666 orange-blue-action start-action-delay
    red-green-action enable-action
    orange-blue-action enable-action
    schedule-task @ run
    pause
  ;

end-module
