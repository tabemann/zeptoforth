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

  \ The tasks
  variable task-0
  variable task-1
  variable task-2
  variable task-3

  \ Initialize the test
  : init-test ( -- )
    0 [: begin 500 ms ." *" again ;] 420 128 512 1 spawn-on-core task-0 !
    0 [: begin 1000 ms ." +" again ;] 420 128 512 1 spawn-on-core task-1 !
    0 [: begin 1500 ms ." x" again ;] 420 128 512 0 spawn-on-core task-2 !
    0 [: begin 2000 ms ." #" again ;] 420 128 512 0 spawn-on-core task-3 !
    task-1 @ run
    task-3 @ run
    task-0 @ run
    task-2 @ run
  ;

end-module
