\ Copyright (c) 2024 Travis Bemann
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

begin-module test

  task import
  rchan import
  rng import
  
  rchan-size buffer: my-rchan
  
  : run-test ( -- )
    my-rchan init-rchan
    0 [:
      0 begin
        dup 0 { W^ s-buffer W^ r-buffer }
        s-buffer cell r-buffer cell my-rchan send-rchan drop
        r-buffer @ .
        1+
        250 ms
      again
    ;] 320 128 512 spawn run
    0 [:
      begin
        [:
          0 { W^ buffer }
          buffer cell my-rchan recv-rchan-no-block drop
          buffer cell my-rchan reply-rchan
        ;] try ['] x-would-block = if ." *" else ." x" then
        rng::random $1FF and ms
      again
    ;] 320 128 512 spawn run
    0 [:
      begin
        [:
          0 { W^ buffer }
          buffer cell my-rchan recv-rchan-no-block drop
          buffer @ negate buffer !
          buffer cell my-rchan reply-rchan
        ;] try ['] x-would-block = if ." +" else ." =" then
        rng::random $1FF and ms
      again
    ;] 320 128 512 spawn run
  ;
  
end-module