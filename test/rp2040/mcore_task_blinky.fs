\ Copyright (c) 2021-2022 Travis Bemann
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
  led import

  \ LED test loop task
  : led-loop ( ms -- ) begin led-toggle dup ms again ;

  \ Star test loop task
  : star-loop ( ms -- ) begin ." *" dup ms again ;

  \ Second core main task
  : core-1-main ( star-ms led-ms -- )
    1 ['] led-loop 420 128 512 spawn run
    1 ['] star-loop 420 128 512 spawn run
    current-task kill
  ;

  \ Initialize the test
  : init-test ( -- )
    1000 500 2 ['] core-1-main 420 128 512 1 spawn-on-core run
    250 ms 750 1 ['] led-loop 420 128 512 spawn run
  ;
  
end-module