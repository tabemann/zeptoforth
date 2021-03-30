\ Copyright (c) 2021 Travis Bemann
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

\ Compile this to flash
compile-to-flash

begin-module-once task-pool-module

  import task-module

  begin-import-module task-pool-internal-module

    \ Task pool structure
    begin-structure task-pool-size
      \ Task pool count
      field: task-pool-count

      \ Task stack size
      hfield: task-pool-stack-size

      \ Task rstack size
      hfield: task-pool-rstack-size

      \ Task dictionary size
      field: task-pool-dict-size
    end-structure

    \ Get a task in a task pool
    : task@ ( index task-pool -- task )
      task-pool-size + swap cells + @
    ;

  end-module
  
  \ No tasks are available
  : x-no-task-available ( -- ) spawn ." no task is available" cr ;
  
  \ Spawn a task from a task pool
  : spawn-from-task-pool ( xn...x0 count xt task-pool -- task )
    begin-critical
    dup task-pool-count @ 0 ?do
      i over task@ terminated? if
	i swap task@ dup >r init-task r> end-critical unloop exit
      then
    loop
    2drop end-critical ['] x-no-task-available ?raise
  ;

  \ Get task pool free count
  : task-pool-free ( task-pool -- count )
    begin-critical
    0 swap dup task-pool-count @ 0 ?do
      i over task@ terminated? if swap 1+ swap then
    loop
    drop end-critical
  ;
  
  \ Initialize a task pool
  : init-task-pool ( dict-size stack-size rstack-size count addr -- )
    tuck task-pool-count !
    tuck swap 4 align swap task-pool-rstack-size h!
    tuck swap 4 align swap task-pool-stack-size h!
    tuck swap 4 align swap task-pool-dict-size !
    dup task-pool-count @ 0 ?do
      0 [: ;]
      2 pick dup task-pool-dict-size @
      swap dup task-pool-stack-size h@
      swap task-pool-rstack-size h@ spawn
      dup kill
      over task-pool-size + i cells + !
    loop
    drop
  ;

  \ Get the size of a task pool of a given size
  : task-pool-size ( count -- bytes ) cells task-pool-size + ;

end-module

