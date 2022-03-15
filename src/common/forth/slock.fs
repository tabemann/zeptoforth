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

compile-to-flash

begin-module slock

  task import
  multicore import

  begin-module slock-internal
  
    begin-structure slock-size
      
      \ The claiming task
      field: slock-task

    end-structure
    
  end-module> import

  export slock-size

  \ Initialize a simple lock
  : init-slock ( addr -- ) 0 swap ! ;

  \ Claim a simple lock
  : claim-slock ( slock -- )
    begin-critical-wait-core begin
      dup slock-task @ ?dup if
	^ task task-internal :: task-waited-for !
	current-task block-indefinite end-critical-release-core
	begin-critical-wait-core false
      else
	current-task swap slock-task ! end-critical-release-core true
      then
    until
  ;

  \ Release a simple lock
  : release-slock ( slock -- )
    [: 0 swap ! wake-other-tasks ;] critical-with-all-core-spinlock
  ;

  \ Claim and release a simple lock while properly handling exceptions
  : with-slock ( xt slock -- )
    >r r@ claim-slock try r> release-slock ?raise
  ;

end-module

reboot