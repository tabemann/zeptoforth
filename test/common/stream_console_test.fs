\ Copyright (c) 2023 Travis Bemann
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

begin-module stream-console-test
  
  task import
  console import
  stream import
  
  1024 constant data-size
  data-size stream-size buffer: my-stream
  
  data-size my-stream init-stream
  
  1 cells buffer: out-notify-area
  1 cells buffer: in-notify-area
  
  0 :noname
    my-stream [:
      0 wait-notify drop
      begin
       [char] Z 1+ [char] A ?do i emit loop \ 100 ms
      again
    ;] with-stream-output
  ; 512 128 512 spawn constant out-task
  
  0 :noname
    my-stream [:
      0 wait-notify drop
      begin
        key emit
      again
    ;] with-stream-input
  ; 512 128 512 spawn constant in-task
  
  out-notify-area 1 out-task config-notify
  in-notify-area 1 in-task config-notify
  
  out-task run
  in-task run
  
  0 out-task notify
  0 in-task notify
  
end-module
