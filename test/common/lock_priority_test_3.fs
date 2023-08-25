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

  lock-size aligned-buffer: my-lock0
  lock-size aligned-buffer: my-lock1
  
  : test ( -- )
    my-lock0 init-lock
    my-lock1 init-lock
    0 [:
      ." *A* "
      my-lock0 claim-lock
      my-lock1 claim-lock
      ." *B* "
      5000 ms
      my-lock0 release-lock
      my-lock1 release-lock
      ." *C* "
    ;] 320 128 512 spawn { task0 }
    0 [:
      1000 ms
      ." *X* "
      my-lock0 claim-lock
      ." *Y* "
      5000 ms
      my-lock0 release-lock
      ." *Z* "
    ;] 320 128 512 spawn { task1 }
    0 [:
      2000 ms
      ." *0* "
      my-lock0 claim-lock
      ." *1* "
      5000 ms
      my-lock0 release-lock
      ." *2* "
    ;] 320 128 512 spawn { task2 }
        0 [:
      500 ms
      ." *a* "
      my-lock1 claim-lock
      ." *b* "
      5000 ms
      my-lock1 release-lock
      ." *c* "
    ;] 320 128 512 spawn { task3 }
    0 [:
      1500 ms
      ." *x* "
      my-lock1 claim-lock
      ." *y* "
      5000 ms
      my-lock1 release-lock
      ." *z* "
    ;] 320 128 512 spawn { task4 }
    0 task0 task-priority!
    1 task1 task-priority!
    2 task2 task-priority!
    3 task3 task-priority!
    4 task4 task-priority!
    task0 task1 task2 task3 task4 [: { task0 task1 task2 task3 task4 }
      task0 run
      task1 run
      task2 run
      task3 run
      task4 run
    ;] critical
  ;

end-module
