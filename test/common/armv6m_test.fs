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

begin-module armv6m-test

  armv6m import
  
  \ Verification function
  : verify ( flag c-addr u -- ) cr type ." : " if ." PASS" else ." FAIL" then ;
  
  \ ADDS TOS, TOS, #1 test
  : test-adds-tos-tos-#1 ( -- )
    1 code[ 1 tos tos adds_,_,#_ ]code 2 = s" ADDS TOS, TOS, #1" verify
  ;

  \ ADDS TOS, #1 test
  : test-adds-tos-#1 ( -- )
    2 code[ 1 tos adds_,#_ ]code 3 = s" ADDS TOS, #1" verify
  ;

  \ ADDS TOS, R0, R1 test
  : test-adds-tos-r0-r1 ( -- )
    0 code[ 1 r0 movs_,#_ 2 r1 movs_,#_ r1 r0 tos adds_,_,_ ]code
    3 = s" ADDS TOS, R0, R1" verify
  ;

  \ ADDS TOS, R12 test
  : test-adds-tos-r12 ( -- )
    1 code[
    r12 r1 mov4_,4_
    2 r0 movs_,#_
    r0 r12 mov4_,4_
    r12 tos add4_,4_
    r1 r12 mov4_,4_
    ]code
    3 = s" ADDS TOS, R12" verify
  ;

  \ ANDS TOS, R0 test
  : test-ands-tos-r0 ( -- )
    $FC0 code[
    $FF r0 movs_,#_
    r0 tos ands_,_
    ]code
    $C0 = s" ANDS TOS, R0" verify
  ;

  \ Simple loop test
  : test-simple-loop ( -- )
    16 code[
    0 r0 movs_,#_
    mark>
    2 r0 adds_,#_
    1 tos subs_,#_
\    0 tos cmp_,#_
    ne bc<
    r0 tos movs_,_
    ]code
    32 = s" SIMPLE LOOP" verify
  ;

  \ Run tests
  : run-tests ( -- )
    test-adds-tos-tos-#1
    test-adds-tos-#1
    test-adds-tos-r0-r1
    test-adds-tos-r12
    test-ands-tos-r0
    test-simple-loop
  ;
  
end-module
