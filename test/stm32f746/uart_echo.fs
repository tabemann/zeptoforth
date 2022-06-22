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

  \ Echo characters received by USART6 (GPIO pins PC6, PC7) at a given baud
  : init-test-6 ( baud -- )
    6 6 xc uart-pin 6 7 xc uart-pin 6 uart-baud!
    begin 6 uart>? if 6 uart> dup emit 6 >uart then key? until
  ;

  \ Echo characters received by UART7 (GPIO pins PF7, PF6) at a given baud
  : init-test-7 ( baud -- )
    7 7 xf uart-pin 7 6 xf uart-pin 7 uart-baud!
    begin 7 uart>? if 7 uart> dup emit 7 >uart then key? until
  ;

end-module
