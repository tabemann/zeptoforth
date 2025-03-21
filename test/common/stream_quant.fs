\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module stream-test

  task import
  stream import
  lambda import
  
  1024 constant my-stream-data-size
  256 constant recv-buf-size
  256 constant send-buf-size
  65536 constant interval
  
  my-stream-data-size stream-size buffer: my-stream
  recv-buf-size buffer: recv-buf
  send-buf-size buffer: send-buf
  
  : consumer ( -- )
    interval 0 [:
      recv-buf recv-buf-size my-stream recv-stream
      + 2dup <= [: dup u. swap interval + swap ;] qif
    ;] qagain
  ;
  
  : producer ( -- )
    [: send-buf send-buf-size my-stream send-stream-parts ;] qagain
  ;
  
  : init-test ( -- )
    my-stream-data-size my-stream init-stream
    send-buf-size 0 [: dup send-buf + c! ;] qcount
    0 ['] consumer 256 128 512 spawn run
    0 ['] producer 256 128 512 spawn run
  ;
  
end-module