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
  int-io import
  stream import

  \ The intermediate stream byte count
  256 constant inter-count

  \ The endpoint stream byte count
  256 constant end-count

  \ The intermediate receive byte count
  16 constant inter-recv-count

  \ The endpoint receive byte count
  16 constant end-recv-count

  \ THe endpoint counter interval
  1000 constant end-interval

  \ The tasks
  variable producer-task
  variable inter-task
  variable consumer-task

  \ The streams
  inter-count stream-size buffer: inter-stream
  end-count stream-size buffer: end-stream

  \ The receiver buffers
  inter-recv-count buffer: inter-recv-buf
  end-recv-count buffer: end-recv-buf

  \ Our producer
  : producer ( -- )
    begin s" ABCDEFGHIJKLMNOPQRSTUVWXYZ" inter-stream send-stream again
  ;

  \ Our intermediate
  : inter ( -- )
    begin
      inter-recv-buf inter-recv-count inter-stream recv-stream
      inter-recv-buf swap end-stream send-stream
    again
  ;

  \ Our consumer
  : consumer ( -- )
    end-interval 0 begin
      end-recv-buf end-recv-count end-stream recv-stream +
      2dup <= if
	dup . swap end-interval + swap
      then
    again
  ;

  \ Initialize our test
  : init-test ( -- )
    disable-int-io
    inter-count inter-stream init-stream
    end-count end-stream init-stream
    0 ['] consumer 320 128 512 0 spawn-on-core consumer-task !
    0 ['] inter 320 128 512 1 spawn-on-core inter-task !
    0 ['] producer 320 128 512 0 spawn-on-core producer-task !
    consumer-task @ run
    inter-task @ run
    producer-task @ run
  ;
  
end-module