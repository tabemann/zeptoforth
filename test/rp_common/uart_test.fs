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

begin-module uart-test
  
  uart import
  
  \ These tests must be executed with the USB console and UART0 (on GPIO's 0 and
  \ 1) and UART1 (on GPIO's 8 and 9) linked together.

  : run-test ( -- )
    0 uart-enabled? if 0 disable-uart then
    1 uart-enabled? if 1 disable-uart then
    0 0 uart-pin
    0 1 uart-pin
    1 8 uart-pin
    1 9 uart-pin
    115200 0 uart-baud!
    115200 1 uart-baud!
    0 uart-enabled? not if 0 enable-uart then
    1 uart-enabled? not if 1 enable-uart then
    begin 0 uart>? while 0 uart> drop repeat
    begin 1 uart>? while 1 uart> drop repeat
    [char] Z 1+ [char] A ?do
      begin 0 >uart? until
      i 0 >uart
      begin 1 uart>? until
      1 uart> emit
    loop
    [char] Z 1+ [char] A ?do
      begin 1 >uart? until
      i 1 >uart
      begin 0 uart>? until
      0 uart> emit
    loop
    0 uart-enabled? if 0 disable-uart then
    1 uart-enabled? if 1 disable-uart then
    9600 0 uart-baud!
    9600 1 uart-baud!
    0 uart-enabled? not if 0 enable-uart then
    1 uart-enabled? not if 1 enable-uart then
    [char] Z 1+ [char] A ?do
      begin 0 >uart? until
      i 0 >uart
      begin 1 uart>? until
      1 uart> emit
    loop
    [char] Z 1+ [char] A ?do
      begin 1 >uart? until
      i 1 >uart
      begin 0 uart>? until
      0 uart> emit
    loop
    0 uart-enabled? if 0 disable-uart then
    1 uart-enabled? if 1 disable-uart then
    115200 0 uart-baud!
    115200 1 uart-baud!
    0 uart-enabled? not if 0 enable-uart then
    1 uart-enabled? not if 1 enable-uart then
  ;

end-module
