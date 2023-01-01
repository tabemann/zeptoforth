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

  task import
  chan import

  \ My channel size
  16 constant my-chan-size

  \ My channel
  1 my-chan-size chan-size buffer: my-chan

  \ Initialize my channel
  1 my-chan-size my-chan init-chan

  \ My producer
  : producer ( -- )
    begin
      [char] Z 1+ [char] A ?do
	i [: my-chan send-chan ;] provide-allot-byte pause
      loop
    again
  ;

  \ My consumer
  : consumer ( -- )
    begin
      [: my-chan recv-chan ;] extract-allot-byte emit pause
    again
  ;

  \ My producer task
  variable producer-task

  \ My consumer task
  variable consumer-task

  \ Spawn my producer task
  0 ' producer 420 128 512 spawn producer-task !

  \ Spawn my consumer task
  0 ' consumer 420 128 512 spawn consumer-task !

  \ Enable my consumer task
  consumer-task @ run

  \ Enable my producer task
  producer-task @ run

  \ Initiate execution
  pause

end-module
