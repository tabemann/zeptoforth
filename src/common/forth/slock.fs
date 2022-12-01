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

  ' slock-size export slock-size

  \ Initialize a simple lock
  : init-slock ( addr -- ) 0 swap ! ;

  \ Try to claim a simple lock
  : try-claim-slock ( slock -- success? )
    disable-int
    slock-spinlock claim-spinlock-raw
    dup slock-task @ if
      drop false
    else
      current-task swap slock-task ! true
    then
    slock-spinlock release-spinlock-raw
    enable-int
  ;
  
  \ Claim a simple lock
  : claim-slock ( slock -- )
    disable-int
    slock-spinlock claim-spinlock-raw
    begin
      dup slock-task @ ?dup if
	task-waited-for !
	slock-spinlock task-internal::block-indefinite-self-release
	false
      else
	current-task swap slock-task !
	true
      then
    until
    slock-spinlock release-spinlock-raw
    enable-int
  ;

  \ Claim a simple lock with a timeout
  : claim-slock-timeout ( slock -- )
    disable-int
    slock-spinlock claim-spinlock-raw
    begin
      current-task compare-timeout if
	slock-spinlock release-spinlock-raw
	enable-int
	['] x-timed-out ?raise
      else
	dup slock-task @ ?dup if
	  task-waited-for !
	  slock-spinlock task-internal::block-indefinite-self-release
	  false
	else
	  current-task swap slock-task !
	  true
	then
      then
    until
    slock-spinlock release-spinlock-raw
    enable-int
  ;

  \ Release a simple lock
  : release-slock ( slock -- )
    disable-int
    claim-all-core-spinlock-raw
    0 swap ! task-internal::wake-other-tasks
    release-all-core-spinlock-raw
    enable-int
  ;

  \ Release a simple lock and block atomically
  : release-slock-block ( slock -- )
    disable-int
    claim-all-core-spinlock-raw
    0 swap ! task-internal::wake-other-tasks
    current-task task-internal::block-raw
    release-all-core-spinlock-raw
    enable-int
  ;

  \ Claim and release a simple lock while properly handling exceptions
  : with-slock ( xt slock -- )
    >r r@ claim-slock try r> release-slock ?raise
  ;

end-module

reboot