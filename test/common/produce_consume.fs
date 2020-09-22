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
\ forth-wordlist task-wordlist chan-wordlist 3 set-order
forth-wordlist task-wordlist chan-wordlist 3 set-order
forth-wordlist set-current

\ My channel size
16 constant my-chan-size

\ My channel
my-chan-size chan-size buffer: my-chan

\ Initialize my channel
my-chan my-chan-size init-chan

\ My producer
: producer ( -- )
  begin
    [char] Z 1+ [char] A ?do
      i my-chan send-chan-byte pause
    loop
  again
;

\ My consumer
: consumer ( -- )
  begin
    my-chan recv-chan-byte emit pause
  again
;

\ My producer task
variable producer-task

\ My consumer task
variable consumer-task

\ Spawn my producer task
' producer 256 256 256 spawn producer-task !

\ Spawn my consumer task
' consumer 256 256 256 spawn consumer-task !

\ Enable my consumer task
consumer-task @ enable-task

\ Enable my producer task
producer-task @ enable-task

\ Initiate execution
pause