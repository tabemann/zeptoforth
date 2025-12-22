\ Copyright (c) 2025 Travis Bemann
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

begin-module core1-led

  armv6m import
  core1 import
  gpio import
  pin import

  : do-core1-led-test ( -- )
    code[
    GPIO_OUT_XOR $000000FF and r0 movs_,#_
    GPIO_OUT_XOR $0000FF00 and 8 rshift r1 movs_,#_
    GPIO_OUT_XOR $00FF0000 and 16 rshift r2 movs_,#_
    GPIO_OUT_XOR $FF000000 and 24 rshift r3 movs_,#_
    8 r1 r1 lsls_,_,#_
    16 r2 r2 lsls_,_,#_
    24 r3 r3 lsls_,_,#_
    r1 r0 orrs_,_
    r2 r0 orrs_,_
    r3 r0 orrs_,_
    1 r1 movs_,#_
    25 r1 r1 lsls_,_,#_
    0 r0 r1 str_,[_,#_]
    mark>
    b<
    ]code
  ;

  256 constant stack-size
  stack-size buffer: stack
  
  : core1-led-test ( -- )
    25 output-pin
    ['] do-core1-led-test 2 + stack stack-size + vector-table launch-core1
  ;

end-module
