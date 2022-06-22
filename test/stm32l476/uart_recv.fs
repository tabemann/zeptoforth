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

  uart import
  pin import
  task import
  
  \ Test receiving data from USART6 (GPIO pins PB6, PB7)
  : init-test-1 ( -- )
    1 6 xb uart-pin 1 7 xb uart-pin
    0 [: begin 1 uart> h.2 space again ;] 256 128 512 spawn run
  ;

  \ Test receiving data from UART7 (GPIO pins PA0, PA1)
  : init-test-4 ( -- )
    4 0 xa uart-pin 4 1 xa uart-pin
    0 [: begin 4 uart> h.2 space again ;] 256 128 512 spawn run
  ;

  \ Type a string on USART6
  : type-test-1 ( c-addr u -- )
    begin ?dup while swap dup c@ 1 >uart 1+ swap 1- repeat drop
  ;

  \ Type a string on UART7
  : type-test-4 ( c-addr u -- )
    begin ?dup while swap dup c@ 4 >uart 1+ swap 1- repeat drop
  ;

end-module
