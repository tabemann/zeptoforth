\ Copyright (c) 2024 Travis Bemann
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

begin-module task-interval-test

  task import
  systick import

  : make-normal-task ( D: string -- task )
    2 [: begin 2dup type pause again ;] 320 128 512 spawn
  ;

  : make-delay-task ( ticks D: string -- task )
    3 [:
      begin 2dup type 2 pick systick-counter current-task delay again
    ;] 320 128 512 spawn
  ;
  
  : make-test-task ( ticks D: string -- task )
    2 [: begin 2dup type pause again ;] 320 128 512 spawn
    tuck task-interval!
  ;

  : run-test ( -- )
    s"  " make-normal-task
    s" ." make-normal-task
    375 s" $" make-delay-task
    125 s" *" make-test-task
    250 s" x" make-test-task
    500 s" @" make-test-task
    [: run run run run run run ;] critical
  ;

end-module
