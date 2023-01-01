\ Copyright (c) 2020-2023 Travis Bemann
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

  internal import
  task import
  fchan import

  \ Allot the channel
  fchan-size buffer: my-fchan

  \ The inner loop of the consumer
  : consumer ( -- )
    cr ." Consumer: " current-task h.8
    begin
      [: my-fchan recv-fchan ;] extract-allot-2cell
      cr ." Received: " type
      \    100 ms
      \    pause
    again
  ;

  \ The consumer task
  0 ' consumer 420 128 512 spawn constant consumer-task

  \ The inner loop of a producer
  : do-producer ( -- )
    does> @ execute dup current-task task-name!
    cr ." Producer: " dup count type
    ." : " current-task h.8
    begin
      cr ." Sending: " dup count type
      dup count [: my-fchan send-fchan ;] provide-allot-2cell
      cr ." Done sending: " dup count type
    again
  ;

  \ Create a producer task
  : make-producer ( xt "name" -- )
    s" " <builds-with-name , do-producer 0 latest >body 420 128 512 spawn
    constant
  ;

  \ Create the producers
  :noname c" A" ; make-producer producer-a-task
  :noname c" B" ; make-producer producer-b-task
  :noname c" C" ; make-producer producer-c-task

  \ Initiate the test
  : init-test ( -- )
    my-fchan init-fchan
    consumer-task run
    producer-a-task run
    producer-b-task run
    producer-c-task run
    pause
  ;

end-module