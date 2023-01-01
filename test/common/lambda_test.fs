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

  \ Verification function
  : verify ( flag c-addr u -- ) cr type ." : " if ." PASS" else ." FAIL" then ;

  \ qif true test
  : test-qif-true ( -- )
    0 true [: 1+ ;] qif
    1 = s" qif true" verify
  ;

  \ qif false test
  : test-qif-false ( -- )
    0 false [: 1+ ;] qif
    0 = s" qif false" verify
  ;

  \ qifelse true test
  : test-qifelse-true ( -- )
    0 true [: 1+ ;] [: 1- ;] qifelse
    1 = s" qifelse true" verify
  ;

  \ qifelse false test
  : test-qifelse-false ( -- )
    0 false [: 1+ ;] [: 1- ;] qifelse
    -1 = s" qifelse false" verify
  ;

  \ quntil test
  : test-quntil ( -- )
    0 10 [: swap 1- swap 1- dup 0= ;] quntil drop
    -10 = s" quntil" verify
  ;
  
  \ qagain test
  : test-qagain ( -- )
    [: 0 [: 1+ dup 10 = [: [: ;] ?raise ;] qif ;] qagain ;] try
    drop true s" qagain" verify
  ;

  \ qwhile test
  : test-qwhile ( -- )
    0 10 [: ?dup ;] [: swap 1- swap 1- ;] qwhile
    -10 = s" qwhile" verify
  ;

  \ qcount test
  : test-qcount ( -- )
    0 10 0 [: + ;] qcount
    45 = s" qcount" verify
  ;

  \ qcount+ test
  : test-qcount+ ( -- )
    0 20 0 [: + 2 ;] qcount+
    90 = s" qcount+" verify
  ;

  \ Byte array
  create carray 0 c, 1 c, 2 c, 3 c, 4 c, 5 c, 6 c, 7 c, 8 c, 9 c,

  \ citer test
  : test-citer ( -- )
    0 carray 10 [: + ;] citer
    45 = s" citer" verify
  ;

  \ Halfword array
  create harray 0 h, 1 h, 2 h, 3 h, 4 h, 5 h, 6 h, 7 h, 8 h, 9 h,

  \ hiter test
  : test-hiter ( -- )
    0 harray 10 [: + ;] hiter
    45 = s" hiter" verify
  ;

  \ Word array
  create array 0 , 1 , 2 , 3 , 4 , 5 , 6 , 7 , 8 , 9 ,

  \ iter test
  : test-iter ( -- )
    0 array 10 [: + ;] iter
    45 = s" iter" verify
  ;

  \ Double word array
  create 2array 0. 2, 1. 2, 2. 2, 3. 2, 4. 2, 5. 2, 6. 2, 7. 2, 8. 2, 9. 2,

  \ 2iter test
  : test-2iter ( -- )
    0. 2array 10 [: d+ ;] 2iter
    45. d= s" 2iter" verify
  ;

  \ iter-get test
  : test-iter-get ( -- )
    0 [: ;] 10 [: + ;] iter-get
    45 = s" iter-get" verify
  ;

  \ 2iter-get test
  : test-2iter-get ( -- )
    0. [: 0 ;] 10 [: d+ ;] 2iter-get
    45. d= s" 2iter-get" verify
  ;

  \ Byte array to find a byte in
  create cfind-array 1 c, 3 c, 2 c, 5 c, 4 c, 7 c,

  \ cfind-index test
  : test-cfind-index ( -- )
    cfind-array 6 [: 5 = ;] cfind-index
    3 = s" cfind-index" verify
  ;

  \ cfind-index fail test
  : test-cfind-index-fail ( -- )
    cfind-array 6 [: 8 = ;] cfind-index
    -1 = s" cfind-index fail" verify
  ;

  \ Halfword array to find a halfword in
  create hfind-array 1 h, 3 h, 2 h, 5 h, 4 h, 7 h,
  
  \ hfind-index test
  : test-hfind-index ( -- )
    hfind-array 6 [: 5 = ;] hfind-index
    3 = s" hfind-index" verify
  ;

  \ hfind-index fail test
  : test-hfind-index-fail ( -- )
    hfind-array 6 [: 8 = ;] hfind-index
    -1 = s" hfind-index fail" verify
  ;

  \ Word array to find a word in
  create find-array 1 , 3 , 2 , 5 , 4 , 7 ,

  \ find-index test
  : test-find-index ( -- )
    find-array 6 [: 5 = ;] find-index
    3 = s" find-index" verify
  ;

  \ find-index fail test
  : test-find-index-fail ( -- )
    find-array 6 [: 8 = ;] find-index
    -1 = s" find-index fail" verify
  ;

  \ Double word array to find a double word in
  create 2find-array 1. 2, 3. 2, 2. 2, 5. 2, 4. 2, 7. 2,

  \ 2find-index test
  : test-2find-index ( -- )
    2find-array 6 [: 5. d= ;] 2find-index
    3 = s" 2find-index" verify
  ;

  \ 2find-index fail test
  : test-2find-index-fail ( -- )
    2find-array 6 [: 8. d= ;] 2find-index
    -1 = s" 2find-index fail " verify
  ;

  \ find-get-index test
  : test-find-get-index ( -- )
    [: cells find-array + @ ;] 6 [: 5 = ;] find-get-index
    3 = s" find-get-index" verify
  ;
  
  \ find-get-index fail test
  : test-find-get-index-fail ( -- )
    [: cells find-array + @ ;] 6 [: 8 = ;] find-get-index
    -1 = s" find-get-index fail" verify
  ;

  \ 2find-get-index test
  : test-2find-get-index ( -- )
    [: 2* cells 2find-array + 2@ ;] 6 [: 5. d= ;] 2find-get-index
    3 = s" 2find-get-index" verify
  ;
  
  \ 2find-get-index fail test
  : test-2find-get-index-fail ( -- )
    [: 2* cells 2find-array + 2@ ;] 6 [: 8. d= ;] 2find-get-index
    -1 = s" 2find-get-index fail" verify
  ;

  \ cfind-value test
  : test-cfind-value ( -- )
    cfind-array 6 [: 3 > ;] cfind-value
    swap 5 = and s" cfind-value" verify
  ;

  \ cfind-value fail test
  : test-cfind-value-fail ( -- )
    cfind-array 6 [: 7 > ;] cfind-value
    not swap 0 = and s" cfind-value fail" verify
  ;

  \ hfind-value test
  : test-hfind-value ( -- )
    hfind-array 6 [: 3 > ;] hfind-value
    swap 5 = and s" hfind-value" verify
  ;

  \ hfind-value fail test
  : test-hfind-value-fail ( -- )
    hfind-array 6 [: 7 > ;] hfind-value
    not swap 0 = and s" hfind-value fail" verify
  ;

  \ find-value test
  : test-find-value ( -- )
    find-array 6 [: 3 > ;] find-value
    swap 5 = and s" find-value" verify
  ;

  \ find-value fail test
  : test-find-value-fail ( -- )
    find-array 6 [: 7 > ;] find-value
    not swap 0 = and s" find-value fail" verify
  ;

  \ 2find-value test
  : test-2find-value ( -- )
    2find-array 6 [: 3. d> ;] 2find-value
    -rot 5. d= and s" 2find-value" verify
  ;

  \ 2find-value fail test
  : test-2find-value-fail ( -- )
    2find-array 6 [: 7. d> ;] 2find-value
    not -rot 0. d= and s" 2find-value fail" verify
  ;

  \ find-get-value test
  : test-find-get-value ( -- )
    [: cells find-array + @ ;] 6 [: 3 > ;] find-get-value
    swap 5 = and s" find-get-value" verify
  ;

  \ find-get-value fail test
  : test-find-get-value-fail ( -- )
    [: cells find-array + @ ;] 6 [: 7 > ;] find-get-value
    not swap 0 = and s" find-get-value fail" verify
  ;

  \ 2find-get-value test
  : test-2find-get-value ( -- )
    [: 2* cells 2find-array + 2@ ;] 6 [: 3. d> ;] 2find-get-value
    -rot 5. d= and s" 2find-get-value" verify
  ;

  \ 2find-get-value fail test
  : test-2find-get-value-fail ( -- )
    [: 2* cells 2find-array + 2@ ;] 6 [: 7. d> ;] 2find-get-value
    not -rot 0. d= and s" 2find-get-value fail" verify
  ;
  
  \ Run tests
  : run-tests ( -- )
    test-qif-true
    test-qif-false
    test-qifelse-true
    test-qifelse-false
    test-quntil
    test-qagain
    test-qwhile
    test-qcount
    test-qcount+
    test-citer
    test-hiter
    test-iter
    test-2iter
    test-iter-get
    test-2iter-get
    test-cfind-index
    test-cfind-index-fail
    test-hfind-index
    test-hfind-index-fail
    test-find-index
    test-find-index-fail
    test-2find-index
    test-2find-index-fail
    test-find-get-index
    test-find-get-index-fail
    test-2find-get-index
    test-2find-get-index-fail
    test-cfind-value
    test-cfind-value-fail
    test-hfind-value
    test-hfind-value-fail
    test-find-value
    test-find-value-fail
    test-2find-value
    test-2find-value-fail
    test-find-get-value
    test-find-get-value-fail
    test-2find-get-value
    test-2find-get-value-fail
  ;
  
end-module