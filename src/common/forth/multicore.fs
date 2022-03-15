\ Copyright (c) 2020-2022 Travis Bemann
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

\ Compile to flash
compile-to-flash

begin-module multicore

  \ Spinlock count
  0 constant spinlock-count

  \ Serial spinlock index
  -1 constant serial-spinlock

  \ Simple lock spinlock index
  -1 constant slock-spinlock

  \ Spinlock out of range exception
  : x-spinlock-out-of-range ( -- ) ." spinlock out of range" cr ;

  \ Core out of range exception
  : x-core-out-of-range ( -- ) ." core out of range" cr ;

  \ Core not addressable exception
  : x-core-not-addressable ( -- ) ." core not addressable" cr ;

  \ Claim the simple lock spinlock - note that this is a no-op
  : claim-slock-spinlock ( -- ) ;

  \ Release the simple lock spinlock - note that this is a no-op
  : release-slock-spinlock ( -- ) ;

  \ Claim a spinlock - note that this is a no-op
  : claim-spinlock ( index -- ) drop ;

  \ Release a spinlock - note that this is a no-op
  : release-spinlock ( index -- ) drop ;

  \ Claim a spinlock for the current core's multitasker - this is a no-op
  : claim-same-core-spinlock ( -- ) ;

  \ Release a spinlock for the current core's multitasker - this is a no-op
  : release-same-core-spinlock ( -- ) ;

  \ Claim a spinlock for a different core's multitasker - this is a no-op
  : claim-other-core-spinlock ( core -- ) drop ;

  \ Release a spinlock for the other core's multitasker - this is a no-op
  : release-other-core-spinlock ( core -- ) drop ;

  \ Claim all core's multitasker's spinlocks - this is a no-op
  : claim-all-core-spinlock ( -- ) ;

  \ Release all core's multitasker's spinlocks - this is a no-op
  : release-all-core-spinlock ( -- ) ;
  
  \ Execute an xt (and not claim a spinlock)
  : with-spinlock ( xt spinlock -- ) drop execute ;

  \ Enter a critical section (and not claim a spinlock)
  : critical-with-spinlock ( xt spinlock -- ) drop critical ;

  \ Enter a critical section (and not claim another core's multitasker's
  \ spinlock)
  : critical-with-other-core-spinlock ( xt core -- ) drop critical ;

  \ Enter a critical section (and do not claim any spinlocks)
  : critical-with-all-core-spinlock ( xt -- ) critical ;

  \ Begin a critical section (and not halt any other core)
  : begin-critical-wait-core ( -- ) begin-critical ;

  \ End a critcal section (and not release any other core)
  : end-critical-release-core ( -- ) end-critical ;
  
  \ Drain a multicore FIFO
  : fifo-drain ( core -- ) ['] x-core-out-of-range ?raise ;
  
  \ Blocking FIFO push
  : fifo-push-blocking ( x core -- ) ['] x-core-out-of-range ?raise ;
  
  \ Blocking FIFO pop
  : fifo-pop-blocking ( core -- x ) ['] x-core-out-of-range ?raise ;

  \ Attempt to send data on a FIFO and confirm that the same data is sent back.
  : fifo-push-confirm ( x core -- confirmed? ) ['] x-core-out-of-range ?raise ;
  
  \ Launch an auxiliary core
  : launch-aux-core ( xt stack-ptr rstack-ptr core -- )
    ['] x-core-out-of-range ?raise
  ;

  \ Reset an auxiliary core
  : reset-aux-core ( core -- ) ['] x-core-out-of-range ?raise ;

end-module

\ Reboot
reboot
