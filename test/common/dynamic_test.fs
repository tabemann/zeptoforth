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

begin-module dynamic-test

  dynamic import
  
  dyn foo
  dyn bar
  dyn baz
  2dyn 2foo
  2dyn 2bar
  2dyn 2baz

  \ Validate a single-cell value
  : validate ( x1 x0 -- )
    cr 2dup = if . ." = " . ." : PASS" else . ." <> " . ." : FAIL" then
  ;
  
  \ Validate a double-cell value
  : 2validate ( x1 x0 -- )
    cr 4dup d= if f. ." = " f. ." : PASS" else f. ." <> " f. ." : FAIL" then
  ;

  \ Validate an unset variable
  : validate-unset ( xt -- )
    cr try ['] x-dyn-variable-not-set =
    if ." unset : PASS" else ." set : FAIL" then
  ;
  
  \ Run the tests
  : test ( -- )
    [: [: [: foo dyn@ ;] 3 baz dyn! ;] 2 bar dyn! ;] 1 foo dyn!
    1 validate
    [: [: [: bar dyn@ ;] 3 baz dyn! ;] 2 bar dyn! ;] 1 foo dyn!
    2 validate
    [: [: [: baz dyn@ ;] 3 baz dyn! ;] 2 bar dyn! ;] 1 foo dyn!
    3 validate
    [: [: [: foo dyn@ ;] 3 foo dyn! ;] 2 bar dyn! ;] 1 foo dyn!
    3 validate
    [: [: [: bar dyn@ ;] 3 foo dyn! ;] 2 bar dyn! ;] 1 foo dyn!
    2 validate
    [: [: [: 2foo dyn@ ;] 3,3 2baz dyn! ;] 2,4 2bar dyn! ;] 1,5 2foo dyn!
    1,5 2validate
    [: [: [: 2bar dyn@ ;] 3,3 2baz dyn! ;] 2,4 2bar dyn! ;] 1,5 2foo dyn!
    2,4 2validate
    [: [: [: 2baz dyn@ ;] 3,3 2baz dyn! ;] 2,4 2bar dyn! ;] 1,5 2foo dyn!
    3,3 2validate
    [: [: [: 2foo dyn@ ;] 3,3 2foo dyn! ;] 2,4 2bar dyn! ;] 1,5 2foo dyn!
    3,3 2validate
    [: [: [: 2bar dyn@ ;] 3,3 2foo dyn! ;] 2,4 2bar dyn! ;] 1,5 2foo dyn!
    2,4 2validate
    [: [: [: foo dyn@ ;] 3 baz dyn! ;] 2 bar dyn! ;] validate-unset
    [: [: [: 2foo dyn@ ;] 3,3 2baz dyn! ;] 2,4 2bar dyn! ;] validate-unset
  ;
  
end-module
