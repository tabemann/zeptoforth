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

continue-module forth

  task import
  chan import

  \ The data size
  1 constant element-size
  
  \ The intermediate stream byte count
  256 constant inter-count

  \ The endpoint stream byte count
  256 constant end-count

  \ THe endpoint counter interval
  1000 constant end-interval

  \ The tasks
  variable producer-task
  variable inter-task
  variable consumer-task

  \ The streams
  element-size inter-count chan-size buffer: inter-chan
  element-size end-count chan-size buffer: end-chan

  \ The data buffers
  element-size buffer: source-send-buf
  element-size buffer: inter-recv-buf
  element-size buffer: end-recv-buf

  \ Our producer
  : producer ( -- )
    source-send-buf element-size 0 fill
    begin source-send-buf element-size inter-chan send-chan again
  ;

  \ Our intermediate
  : inter ( -- )
    begin
      inter-recv-buf element-size inter-chan recv-chan
      inter-recv-buf swap end-chan send-chan
    again
  ;

  \ Our consumer
  : consumer ( -- )
    end-interval 0 begin
      end-recv-buf element-size end-chan recv-chan +
      2dup <= if
	dup . swap end-interval + swap
      then
    again
  ;

  \ Initialize our test
  : init-test ( -- )
    element-size inter-count inter-chan init-chan
    element-size end-count end-chan init-chan
    0 ['] consumer 480 128 512 0 spawn-on-core consumer-task !
    0 ['] inter 480 128 512 1 spawn-on-core inter-task !
    0 ['] producer 480 128 512 0 spawn-on-core producer-task !
    consumer-task @ run
    inter-task @ run
    producer-task @ run
  ;
  
end-module