\ Copyright (c) 2020-2023 Travis Bemann
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

\ Reboot
reboot

\ Load the stack test functionlity to be tested
#include test/common/stack.fs

\ Compile this to RAM
compile-to-ram

\ Display a basic message on failure
: do-fail ( -- ) ." *" ;

\ Register fail handler
' do-fail stack-fail-hook !

\ A basic test of stack testing functionality
: stack-test-1 ( -- )
  ." stack-test-1" cr
  t{ 0 dup 0 0 2 }t
;

\ Another basic test of stack testing functionality
: stack-test-2 ( -- )
  ." stack-test-2" cr
  t{ 0 drop 0 }t
;

\ A test of stack testing stack depth failure functionality
: stack-test-3 ( -- )
  ." stack-test-3" cr
  t{ 1 dup 1 1 }t
;

\ Run the tests
stack-test-1
stack-test-2
stack-test-3