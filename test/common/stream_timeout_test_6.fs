\ Copyright (c) 2021-2023 Travis Bemann
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
  stream import

  \ Our byte count
  256 constant my-count

  \ Our byte receive count
  16 constant my-recv-count
  
  \ Our byte minimum receive count
  8 constant my-min-recv-count
  
  \ Our stream
  my-count stream-size buffer: my-stream

  \ Our tasks
  variable producer-task
  variable consumer-task
  
  \ Our producer
  : producer ( -- )
    0 begin
      s" ABCDEFGHIJKLMNOPQRSTUVWXYZ" my-stream send-stream-parts dup ms 100 +
    again
  ;

  \ Our consumer
  : consumer ( -- )
    5000 timeout !
    begin
      my-recv-count [:
	dup my-recv-count my-min-recv-count my-stream peek-stream-min tuck type
	my-min-recv-count my-stream skip-stream-min drop
      ;] with-allot
    again
  ;
  
  \ Initialize our test
  : init-test ( -- )
    my-count my-stream init-stream
    0 ['] producer 420 128 512 spawn producer-task !
    0 ['] consumer 420 128 512 spawn consumer-task !
    producer-task @ run consumer-task @ run
  ;
  
end-module
