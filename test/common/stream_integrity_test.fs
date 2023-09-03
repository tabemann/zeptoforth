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
\ SOFTWARE

begin-module stream-integrity-test

  stream import
  task import
  
  2048 constant my-stream-size
  
  my-stream-size stream-size buffer: my-stream
  
  my-stream-size buffer: my-buffer
  
  : init-test ( -- )
    my-stream-size my-stream init-stream
    0 [:
      0 { counter }
      begin
        my-buffer my-stream-size my-stream recv-stream { bytes-read }
        bytes-read 0 ?do
          my-buffer i + c@ { my-byte }
          my-byte counter <> if cr ." BAD BYTE: " my-byte h.2 then
          my-byte 1+ $FF and to counter
        loop
      again
    ;] 320 128 512 spawn run
    0 [:
      0 { W^ counter }
      begin
        counter 1 my-stream send-stream
        counter c@ 1+ $FF and counter c!
      again
    ;] 320 128 512 spawn run
  ;
  
end-module
