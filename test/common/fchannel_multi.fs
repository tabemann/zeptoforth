\ Copyright (c) 2020 Travis Bemann
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

\ Set up the wordlist order
forth-wordlist internal-wordlist task-wordlist fchan-wordlist 4 set-order
forth-wordlist set-current

\ Allot the channel
fchan-size buffer: my-fchan

\ The inner loop of the consumer
: consumer ( -- )
  0 pause-enabled ! cr ." Consumer: " current-task h.8 1 pause-enabled !
  begin
    my-fchan recv-fchan
    0 pause-enabled ! cr ." Received: " type 1 pause-enabled !
\    100 ms
    pause
  again
;

\ The consumer task
' consumer 256 256 256 spawn constant consumer-task

\ The inner loop of a producer
: do-producer ( -- )
  does> @ execute
  0 pause-enabled ! cr ." Producer: " 2dup type
  ." : " current-task h.8 1 pause-enabled !
  begin
    0 pause-enabled ! cr ." Sending: " 2dup type 1 pause-enabled !
    2dup my-fchan send-fchan
    0 pause-enabled ! cr ." Done sending: " 2dup type 1 pause-enabled !
  again
;

\ Create a producer task
: make-producer ( xt "name" -- )
  s" " <builds-with-name , do-producer latest >body 256 256 256 spawn constant
;

\ Create the producers
:noname s" A" ; make-producer producer-a-task
:noname s" B" ; make-producer producer-b-task
:noname s" C" ; make-producer producer-c-task

\ Initiate the test
: init-test ( -- )
  my-fchan init-fchan
  consumer-task run
  producer-a-task run
  producer-b-task run
  producer-c-task run
  pause
;
