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

\ Compile this to flash
compile-to-flash

\ Begin compressing compiled code in flash
compress-flash

\ Execute an xt based on whether a condition is true
: qif ( f true-xt -- ) ( true-xt: ??? -- ??? )
  [inlined] swap if inline-execute else drop then
;

\ Execute one of two different xts based on whether a condition is true or false
: qifelse ( f true-xt false-xt -- )
  ( true-xt: ??? -- ??? ) ( false-xt: ??? -- ??? )
  [inlined] 2 pick if drop nip inline-execute else nip nip inline-execute then
;

\ Execute an until loop with an xt
: quntil ( ??? xt -- ??? ) ( xt: ??? -- ??? f )
  { xt } begin xt execute until
;

\ Execute an again loop with an xt
: qagain ( ??? xt -- ??? ) ( xt: ??? -- ??? )
  { xt } begin xt execute again
;

\ Execute a while loop with a while-xt and a body-xt
: qwhile ( ??? while-xt body-xt -- ??? )
  ( while-xt: ??? -- ??? f ) ( body-xt: ??? -- ??? )
  { while-xt body-xt } begin while-xt execute while body-xt execute repeat
;

\ Execute a counted loop with an xt
: qcount ( ??? limit init xt -- ??? ) ( xt: ??? i -- ??? )
  { xt } ?do i xt execute loop
;

\ Execute a counted loop with an arbitrary increment with an xt
: qcount+ ( ??? limit init xt -- ??? ) ( xt: ??? i -- ???? increment )
  { xt } ?do i xt execute +loop
;

\ Iterate executing an xt over a byte array
: citer ( ??? addr count xt -- ??? ) ( xt: ??? b -- ??? )
  { xt } over + { addr end-addr }
  begin addr end-addr u< while addr c@ xt execute 1 +to addr repeat
;

\ Iterate executing an xt over a halfword array
: hiter ( ??? addr count xt -- ??? ) ( xt: ??? h -- ??? )
  { xt } 2* over + { addr end-addr }
  begin addr end-addr u< while addr h@ xt execute 2 +to addr repeat
;

\ Iterate executing an xt over a cell array
: iter ( ??? addr count xt -- ??? ) ( xt: ??? x -- ??? )
  { xt } cells over + { addr end-addr }
  begin addr end-addr u< while addr @ xt execute cell +to addr repeat
;

\ Iterate executing an xt over a double-word array
: 2iter ( ??? addr count xt -- ??? ) ( xt: ??? d -- ??? )
  { xt } 2* cells over + { addr end-addr }
  begin addr end-addr u< while addr 2@ xt execute 8 +to addr repeat
;

\ Iterate executing at xt over values from a getter
: iter-get ( ??? get-xt count iter-xt -- ??? )
  ( get-xt: ??? i -- ??? x ) ( iter-xt: ??? x -- ??? )
  rot { iter-xt get-xt }
  0 ?do i get-xt execute iter-xt execute loop
;

\ Iterate executing at xt over double-word values from a getter
: 2iter-get ( ??? get-xt count iter-xt -- ??? )
  ( get-xt: ??? i -- ??? d ) ( iter-xt: ??? d -- ??? )
  rot { iter-xt get-xt }
  0 ?do i get-xt execute iter-xt execute loop
;

\ Find the index of a value in a byte array with a predicate
: cfind-index ( ??? b-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )
  rot { xt addr }
  0 ?do addr i + c@ xt execute if i unloop exit then loop
  -1
;

\ Find the index of a value in a halfword array with a predicate
: hfind-index ( ??? h-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )
  rot { xt addr }
  0 ?do addr i 2* + h@ xt execute if i unloop exit then loop
  -1
;

\ Find the index of a value in a cell array with a predicate
: find-index ( ??? a-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )
  rot { xt addr }
  0 ?do addr i cells + @ xt execute if i unloop exit then loop
  -1
;

\ Find the index of a value in a double-word array with a predicate
: 2find-index ( ??? a-addr count xt -- ??? i|-1 ) ( xt: ??? d -- ??? f )
  rot { xt addr }
  0 ?do addr i 2* cells + 2@ xt execute if i unloop exit then loop
  -1
;

\ Find the index of a value from a getter with a predicate
: find-get-index ( ??? get-xt count pred-xt --- ??? i|-1 )
  ( get-xt: ??? i -- ??? x ) ( pred-xt: ??? x -- ??? f )
  rot { pred-xt get-xt }
  0 ?do i get-xt execute pred-xt execute if i unloop exit then loop
  -1
;

\ Find the index of a double-word value from a getter with a predicate
: 2find-get-index ( ??? get-xt count pred-xt --- ??? i|-1 )
  ( get-xt: ??? i -- ??? d ) ( pred-xt: ??? d -- ??? f )
  rot { pred-xt get-xt }
  0 ?do i get-xt execute pred-xt execute if i unloop exit then loop
  -1
;

\ Find a value in a byte array with a predicate
: cfind-value ( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )
  rot { xt addr }
  0 ?do addr i + c@ dup { val } xt execute if val true unloop exit then loop
  0 false
;

\ Find a value in a halfword array with a predicate
: hfind-value ( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )
  rot { xt addr }
  0 ?do
    addr i 2* + h@ dup { val } xt execute if
      val true unloop exit
    then
  loop
  0 false
;

\ Find a value in a cell array with a predicate
: find-value ( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )
  rot { xt addr }
  0 ?do
    addr i cells + @ dup { val } xt execute if val true unloop exit then
  loop
  0 false
;

\ Find a value in a double-word array with a predicate
: 2find-value ( ??? a-addr count xt -- ??? d|0 f ) ( xt: ??? d -- ??? f )
  rot { xt addr }
  0 ?do
    addr i 2* cells + 2@ 2dup { D: val } xt execute if
      val true unloop exit
    then
  loop
  0 0 false
;

\ Find a value from a getter with a predicate
: find-get-value ( ???? get-xt count pred-xt --- ??? x|0 f )
  ( get-xt: ??? i -- ??? x ) ( pred-xt: ??? x -- ??? f )
  rot { pred-xt get-xt }
  0 ?do
    i get-xt execute dup { val } pred-xt execute if
      val true unloop exit
    then
  loop
  0 false
;

\ Find a double-word value from a getter with a predicate
: 2find-get-value ( ???? get-xt count pred-xt --- ??? d|0 f )
  ( get-xt: ??? i -- ??? d ) ( pred-xt: ??? d -- ??? f )
  rot { pred-xt get-xt }
  0 ?do
    i get-xt execute 2dup { D: val } pred-xt execute if
      val true unloop exit
    then
  loop
  0 0 false
;

\ Ending compiling code in flash
end-compress-flash
compile-to-ram
