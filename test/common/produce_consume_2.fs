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

begin-module produce-consume

  task import
  fchan import
  lock import
  led import
  
  fchan-size buffer: my-fchan
  lock-size buffer: my-lock
  
  : consumer-0 ( -- )
    [:
      [: my-fchan recv-fchan ;] extract-allot-cell
      [: ." +" . ;] my-lock with-lock
      0 toggle-led
      500 ms
    ;] qagain
  ;
  
  : consumer-1 ( -- )
    [:
      [: my-fchan recv-fchan ;] extract-allot-cell
      [: ." *" . ;] my-lock with-lock
      0 toggle-led
      500 ms
    ;] qagain
  ;
  
  : producer ( -- )
    0 [: dup [: my-fchan send-fchan ;] provide-allot-cell 1+ ;] qagain
  ;
  
  : init-test ( -- )
    my-fchan init-fchan
    my-lock init-lock
    0 ['] consumer-0 256 128 512 spawn run
    0 ['] consumer-1 256 128 512 spawn run
    0 ['] producer 256 128 512 spawn run
  ;
  
end-module
 