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
\ SOFTWARE.

begin-module i2c-test

  task import
  task-pool import
  i2c import
  pin import
  
  2 constant task-count
  task-count task-pool-size buffer: my-task-pool
  
  16 constant recv-buffer-size
  recv-buffer-size buffer: recv-buffer
  
  : init-test ( -- )
    0 master-i2c
    1 slave-i2c
    0 10-bit-i2c-addr
    1 10-bit-i2c-addr
    $3C 0 i2c-target-addr!
    $3C 1 i2c-slave-addr!
    0 enable-i2c
    1 enable-i2c
    1 14 i2c-pin
    1 15 i2c-pin
    0 12 i2c-pin
    0 13 i2c-pin
    320 128 512 task-count my-task-pool init-task-pool
  ;
  
  : do-test-0 ( -- )
    0 [:
      20000 timeout !
      1 wait-i2c-master-send
      begin
        recv-buffer recv-buffer-size 1 i2c>
        recv-buffer over type
      0= until
      ."  *** "
      1 wait-i2c-master-send
      begin
        recv-buffer recv-buffer-size 1 i2c>
        recv-buffer over type
      0= until
      \ 1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    pause 1000 ms
    0 [:
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" 0 >i2c-stop .
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" 0 >i2c-stop .
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" 0 >i2c-stop .
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" 0 >i2c-stop .
\      s" BAR" 0 >i2c .
\      s" BAZ" 0 >i2c-stop .
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-0a ( -- )
    0 [:
      20000 timeout !
      1 wait-i2c-master-recv
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" 1 >i2c .
      1 wait-i2c-master-recv
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" 1 >i2c .
      \ 1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    pause 1000 ms
    0 [:
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer 4 0 i2c-stop>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c-restart>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer 4 0 i2c-stop>
      recv-buffer swap type
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-1 ( -- )
    0 [:
      20000 timeout !
      1 wait-i2c-master-recv
      s" FOO" 1 >i2c .
      s" BAR" 1 >i2c .
      s" BAZ" 1 >i2c .
      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    pause 1000 ms
    0 [:
      recv-buffer 9 0 i2c-stop>
      recv-buffer swap type
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-2 ( -- )
    0 [:
      10000 timeout !
      1 wait-i2c-master-recv
    ;] my-task-pool spawn-from-task-pool run
  ;

  : do-test-3 ( -- )
    0 [:
      10000 timeout !
      1 wait-i2c-master-send
    ;] my-task-pool spawn-from-task-pool run
  ;

  : do-test-4 ( -- )
    0 [:
      10000 timeout !
      1 wait-i2c-master
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-5 ( -- )
    0 [:
      750 ms
      20000 timeout !
      1 wait-i2c-master-send
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      [:
        5000 timeout !
        s" ABCDEFGHIJKLMNOPQRSTUVWXYZ" 0 >i2c .
      ;] try ['] x-timed-out = if display-red ." timed out!" cr display-normal then
      1000 ms
      no-timeout timeout !
      s" FOO" 0 >i2c .
      s" BAR" 0 >i2c .
      s" BAZ" 0 >i2c-stop .
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-6 ( -- )
    0 [:
      750 ms
      20000 timeout !
      1 wait-i2c-master-recv
      s" FOO" 1 >i2c .
      s" BAR" 1 >i2c .
      s" BAZ" 1 >i2c .
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      [:
        5000 timeout !
        recv-buffer 9 0 i2c-stop> drop
      ;] try ['] x-timed-out = if display-red ." timed out!" cr display-normal then
      no-timeout timeout !
      recv-buffer 9 0 i2c-stop>
      recv-buffer swap type
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-7 ( -- )
    0 [:
      [:
        5000 timeout !
        1 wait-i2c-master-send
        recv-buffer recv-buffer-size 1 i2c>
        recv-buffer swap type
      ;] try ['] x-timed-out = if display-red ." timed out!" cr display-normal then
      no-timeout timeout !
      1 wait-i2c-master-send
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      1000 ms
      s" FOO" 0 >i2c .
      s" BAR" 0 >i2c .
      s" BAZ" 0 >i2c-stop .
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-8 ( -- )
    0 [:
      [:
        5000 timeout !
        1 wait-i2c-master-recv
        s" FOO" 1 >i2c .
        s" BAR" 1 >i2c .
        s" BAZ" 1 >i2c .
      ;] try ['] x-timed-out = if display-red ." timed out!" cr display-normal then
      no-timeout timeout !
      1 wait-i2c-master-recv
      s" FOO" 1 >i2c .
      s" BAR" 1 >i2c .
      s" BAZ" 1 >i2c .
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      1000 ms
      recv-buffer 9 0 i2c-stop>
      recv-buffer swap type
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-9 ( -- )
    0 [:
      1 wait-i2c-master-send
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      s" 0123456789ABCDEF" 0 >i2c .
      s" 0123456789ABCDEF" 0 >i2c .
      s" 0123456789ABCDEF" 0 >i2c .
      s" 0123456789ABCDEF" 0 >i2c-stop .
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-10 ( -- )
    0 [:
      1 wait-i2c-master-send
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 1 i2c>
      recv-buffer swap type
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZ" 0 >i2c .
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZ" 0 >i2c-stop .
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-11 ( -- )
    0 [:
      1 wait-i2c-master-recv
      s" 0123456789ABCDEF" 1 >i2c .
      s" 0123456789ABCDEF" 1 >i2c .
      s" 0123456789ABCDEF" 1 >i2c .
      s" 0123456789ABCDEF" 1 >i2c .
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c-stop>
      recv-buffer swap type
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-12 ( -- )
    0 [:
      1 wait-i2c-master-recv
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZ" 1 >i2c .
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZ" 1 >i2c .
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer 4 0 i2c-stop>
      recv-buffer swap type
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-13 ( -- )
    0 [:
      10000 timeout !
      2 0 ?do
        1 wait-i2c-master case
          accept-send of
            recv-buffer recv-buffer-size 1 i2c>
            recv-buffer swap type
          endof
          accept-recv of
            s" ABCDEFGHIJKLMNOPQRSTUVWXYZ" 1 >i2c .
            s" ABCDEFGHIJKLMNOPQRSTUVWXYZ" 1 >i2c .
          endof
        endcase
      loop
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer recv-buffer-size 0 i2c>
      recv-buffer swap type
      recv-buffer 4 0 i2c-stop>
      recv-buffer swap type
      s" 0123456789ABCDEF" 0 >i2c-stop .
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;
  
  : do-test-14 ( -- )
    0 [:
      1 wait-i2c-master-send ." *"
      recv-buffer recv-buffer-size 1 i2c> ." *"
      recv-buffer swap type ." *"
      1 i2c-nack ." *"
\      1 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
    0 [:
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" 0 >i2c .
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ" 0 >i2c .
\      0 clear-i2c
    ;] my-task-pool spawn-from-task-pool run
  ;

end-module