\ Copyright (c) 2023 Travis Bemann
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

begin-module closure-test

  closure import

  \ Prints 2 using scoped single-cell closures
  : test0 ( -- )
    1 ['] + [: 1 swap execute . ;] with-closure
  ;

  \ Prints 6,0 using scoped double-cell closures
  : test1 ( -- )
    2,0 ['] f* [: 3,0 rot execute f. ;] with-dclosure
  ;

  \ Prints 9 using scoped multi-cell closures
  : test2 ( -- )
    1 2 3 3 [: + + + ;]
    [: 3 swap execute . ;] with-nclosure
  ;

  \ Prints 9,0 using scoped multi-double-cell closures
  : test3 ( -- )
    3,0 2,0 2 [: { D: x D: y } y f* x d+ ;]
    [: 3,0 rot execute f. ;] with-ndclosure
  ;

  \ Prints 2 using scoped single-cell reference closures
  : test4 ( -- )
    1 [: @ + ;] [: 1 swap execute . ;] with-refclosure
  ;

  \ Prints 6,0 using scoped double-cell reference closures
  : test5 ( -- )
    2,0 [: 2@ f* ;] [: 3,0 rot execute f. ;] with-drefclosure
  ;

  \ Prints 9 using scoped multi-cell reference closures
  : test6 ( -- )
    1 2 3 3 [: { x y z } x @ + y @ + z @ + ;]
    [: 3 swap execute . ;] with-nrefclosure
  ;

  \ Prints 9,0 using scoped multi-double-cell reference closures
  : test7 ( -- )
    3,0 2,0 2 [: { x y } y 2@ f* x 2@ d+ ;]
    [: 3,0 rot execute f. ;] with-ndrefclosure
  ;

end-module