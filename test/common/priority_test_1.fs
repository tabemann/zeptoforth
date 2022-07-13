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

begin-module priority-test

  task import
  lock import
  
  lock-size buffer: my-lock
  
  : busy-wait ( --- ) 1000000 0 ['] drop qcount ;
  
  : high-priority ( -- )
    1 current-task task-priority!
    [: 10 0 [: [: display-red ." +" . display-normal ;] my-lock with-lock busy-wait ;] qcount 5000 ms ;] qagain
  ;
  
  : low-priority ( -- )
    0 current-task task-priority!
    [: 10 0 [: [: ." *" . ;] my-lock with-lock busy-wait ;] qcount ;] qagain
  ;
  
  : init-test ( -- )
    my-lock init-lock
    0 ['] low-priority 256 128 512 spawn run
    0 ['] high-priority 256 128 512 spawn run
  ;

end-module
