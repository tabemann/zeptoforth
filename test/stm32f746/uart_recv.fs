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

continue-module forth

  uart import
  pin import
  task import
  
  \ Test receiving data from USART6 (GPIO pins PC6, PC7)
  : init-test-6 ( -- )
    6 6 xc uart-pin 6 7 xc uart-pin
    0 [: begin 6 uart> h.2 space again ;] 256 128 512 spawn run
  ;

  \ Test receiving data from UART7 (GPIO pins PF7, PF6)
  : init-test-7 ( -- )
    7 7 xf uart-pin 7 6 xf uart-pin
    0 [: begin 7 uart> h.2 space again ;] 256 128 512 spawn run
  ;

  \ Type a string on USART6
  : type-test-6 ( c-addr u -- )
    begin ?dup while swap dup c@ 6 >uart 1+ swap 1- repeat drop
  ;

  \ Type a string on UART7
  : type-test-7 ( c-addr u -- )
    begin ?dup while swap dup c@ 7 >uart 1+ swap 1- repeat drop
  ;

end-module
