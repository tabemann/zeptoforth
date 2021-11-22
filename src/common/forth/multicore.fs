\ Copyright (c) 2020-2021 Travis Bemann
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

begin-module-once multicore-module

  \ Spinlock count
  0 constant spinlock-count

  \ Multitasker spinlock index
  -1 constant task-spinlock

  \ Spinlock out of range exception
  : x-spinlock-out-of-range space ." spinlock out of range" cr ;

  \ Core out of range exception
  : x-core-out-of-range ( -- ) space ." core out of range" cr ;

  \ Core not addressable exception
  : x-core-not-addressable ( -- ) space ." core not addressable" cr ;

  \ Claim a spinlock - note that this will always fail
  : claim-spinlock ( index -- ) ['] x-spinlock-out-of-range ?raise ;

  \ Release a spinlock - note that this will always fail
  : release-spinlock ( index -- ) ['] x-spinlock-out-of-range ?raise ;
    
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
