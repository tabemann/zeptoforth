\ Copyright (c) 2022-2023 Travis Bemann
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
\ SOFTWARE

continue-module forth

  task import
  systick import
  action import

  \ Our schedule
  schedule-size buffer: my-schedule

  \ Our actions
  action-size buffer: star-action
  action-size buffer: dot-action

  \ The star delay time
  variable star-time
  
  \ Our deferred star routine
  defer star

  \ The star routine
  : do-star ( -- )
    ." *"
    10000 star-time +!
    ['] star systick-counter star-time @ over - delay-action-from-time
  ;

  \ Set the deferred star routine
  ' do-star ' star defer!

  \ The dot delay time
  variable dot-time
  
  \ Our deferred dot routine
  defer dot

  \ The dot routine
  : do-dot ( -- )
    ." ."
    2500 dot-time +!
    ['] dot systick-counter dot-time @ over - delay-action-from-time
  ;

  \ Set the deferred dot routine
  ' do-dot ' dot defer!

  \ Initialize the test
  : init-test ( -- )
    systick-counter star-time !
    systick-counter dot-time !
    my-schedule init-schedule
    0 ['] star star-action init-action
    0 ['] dot dot-action init-action
    my-schedule star-action add-action
    my-schedule dot-action add-action
    0 [: my-schedule run-schedule ;] 420 128 512 spawn run
  ;
  
end-module