\ Copyright (c) 2023 Travis Bemann
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

begin-module lock-test

  task import
  lock import

  lock-size aligned-buffer: my-lock
  
  : test ( -- )
    my-lock init-lock
    0 [:
      ." *A* "
      my-lock claim-lock
      ." *B* "
      5000 ms
      my-lock release-lock
      ." *C* "
    ;] 320 128 512 spawn { task0 }
    0 [:
      1000 ms
      ." *X* "
      my-lock claim-lock
      ." *Y* "
      5000 ms
      my-lock release-lock
      ." *Z* "
    ;] 320 128 512 spawn { task1 }
    0 [:
      2000 ms
      ." *0* "
      my-lock claim-lock
      ." *1* "
      5000 ms
      my-lock release-lock
      ." *2* "
    ;] 320 128 512 spawn { task2 }
    0 task0 task-priority!
    1 task1 task-priority!
    2 task2 task-priority!
    task0 task1 task2 [: { task0 task1 task2 }
      task0 run
      task1 run
      task2 run
    ;] critical
  ;

end-module
