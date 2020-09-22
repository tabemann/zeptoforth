\ Copyright (c) 2020 Travis Bemann
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

\ Load the capture functionality to be tested
#include test/common/capture.fs

\ Compile this to RAM
compile-to-ram

\ Display a basic message on failure
: do-fail ( -- ) [: ." *" ;] no-capture ;

\ Register fail handler
' do-fail capture-fail-hook !

\ A basic test of matching functionality
: match-test-1
  clear-capture
  [: ." match-test-1" cr ;] no-capture
  s" FOO" 0 add-match-capture
  s" BAR" 0 add-match-capture
  s" BAZ" 0 add-match-capture
  ." FOOBARBAZ"
;

\ A basic test of matching functionality
: match-test-2
  clear-capture
  [: ." match-test-2" cr ;] no-capture
  s" FOO" 0 add-match-capture
  s" BAR" 0 add-match-capture
  s" BAZ" 0 add-match-capture
  ." FOOBAZBAZ"
;

\ A basic test of skipping functionality
: skip-test-1 ( -- )
  clear-capture
  [: ." skip-test-1" cr ;] no-capture
  s" BAR" 256 0 add-skip-capture
  s" BAZ" 0 add-match-capture
  ." FOOBARBAZ"
;

\ A basic test of ignoring functionality
: ignore-test-1 ( -- )
  clear-capture
  [: ." ignore-test-1" cr ;] no-capture
  s" FOO" 256 0 add-ignore-capture
  s" BAR" 0 add-match-capture
  ." FOOFOOBAR"
;

\ A basic test of ignoring functionality
: ignore-test-2 ( -- )
  clear-capture
  [: ." ignore-test-2" cr ;] no-capture
  s" FOO" 256 0 add-ignore-capture
  s" BAR" 0 add-match-capture
  ." FOOFOBAR"
;

\ Run the tests
enable-capture
match-test-1
match-test-2
skip-test-1
ignore-test-1
ignore-test-2
disable-capture