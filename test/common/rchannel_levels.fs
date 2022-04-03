\ Copyright (c) 2021-2022 Travis Bemann
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
  rchan import

  \ The sender task
  variable send-task

  \ The middle task
  variable middle-task
  
  \ The replying task
  variable reply-task

  \ The first reply channel
  rchan-size buffer: my-rchan-0

  \ The second reply channel
  rchan-size buffer: my-rchan-1
  
  \ The sender loop
  : send-loop ( -- )
    0 begin
      dup cr ." Sending: " .
      dup [: [: my-rchan-0 send-rchan ;] extract-allot-cell ;]
      provide-allot-cell
      cr ." Got reply: " . 1+
    again
  ;

  \ The middle loop
  : middle-loop ( -- )
    begin
      [: my-rchan-0 recv-rchan ;] extract-allot-cell
      dup cr ." Passing on: " .
      [: [: my-rchan-1 send-rchan ;] extract-allot-cell ;] provide-allot-cell
      dup cr ." Passing back: " .
      [: my-rchan-0 reply-rchan ;] provide-allot-cell
    again
  ;

  \ The replying loop
  : reply-loop ( -- )
    0 begin
      [: my-rchan-1 recv-rchan ;] extract-allot-cell
      cr ." Received: " .
      dup [: my-rchan-1 reply-rchan ;] provide-allot-cell 1+
    again
  ;

  \ Initialize the test
  : init-test ( -- )
    my-rchan-0 init-rchan
    my-rchan-1 init-rchan
    0 ['] send-loop 480 128 512 spawn send-task !
    0 ['] middle-loop 480 128 512 spawn middle-task !
    0 ['] reply-loop 480 128 512 spawn reply-task !
    send-task @ run
    middle-task @ run
    reply-task @ run
  ;
  
end-module
