\ Copyright (c) 2022 Travis Bemann
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
  action import
  action-pool import

  \ Our schedule
  schedule-size buffer: my-schedule

  \ Our action pool size
  32 constant my-count
  
  \ Our action pool
  my-count action-pool-size buffer: my-pool

  \ Our star emitter
  defer stars
  : do-stars ( -- ) [: ." *" stars ;] 10000 delay-action ;
  ' do-stars ' stars defer!
  
  \ Our action-starter
  defer start-actions
  : do-start-actions ( -- )
    [:
      [:
	my-pool action-pool-free .
	my-schedule 0 ['] stars my-pool add-action-from-pool drop
	start-actions
      ;] yield-action
    ;] 10000 delay-action
  ;
  ' do-start-actions ' start-actions defer!

  \ Initialize our test
  : init-test ( -- )
    my-schedule init-schedule
    my-count my-pool init-action-pool
    my-schedule 0 ['] start-actions my-pool add-action-from-pool
    0 [: my-schedule run-schedule ;] 420 128 512 spawn run
  ;

end-module
