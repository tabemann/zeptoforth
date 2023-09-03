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

begin-module chan-integrity-test

  chan import
  task import
  
  2048 constant my-chan-size
  
  1 my-chan-size chan-size buffer: my-chan
  
  : init-test ( -- )
    1 my-chan-size my-chan init-chan
    0 [:
      0 0 { counter W^ my-buffer }
      begin
        my-buffer 1 my-chan recv-chan drop
        my-buffer c@ { my-byte }
        my-byte counter <> if cr ." BAD BYTE: " my-byte h.2 then
        my-byte 1+ $FF and to counter
      again
    ;] 320 128 512 spawn run
    0 [:
      0 { W^ counter }
      begin
        counter 1 my-chan send-chan
        counter c@ 1+ $FF and counter c!
      again
    ;] 320 128 512 spawn run
  ;
  
end-module
