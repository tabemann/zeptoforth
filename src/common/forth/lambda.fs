\ Copyright (c) 2020-2022 Travis Bemann
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
  >r
  begin
    r@ execute
  until
  rdrop
;

\ Execute an again loop with an xt
: qagain ( ??? xt -- ??? ) ( xt: ??? -- ??? )
  >r
  begin
    r@ execute
  again
\  rdrop -- will never be reached
;

\ Execute a while loop with a while-xt and a body-xt
: qwhile ( ??? while-xt body-xt -- ??? )
  ( while-xt: ??? -- ??? f ) ( body-xt: ??? -- ??? )
  >r >r
  begin
    r@ execute
  while
    r> r> swap >r >r r@ execute
    r> r> swap >r >r
  repeat
  rdrop rdrop
;

\ Execute a counted loop with an xt
: qcount ( ??? limit init xt -- ??? ) ( xt: ??? i -- ??? )
  -rot ?do
    i swap dup >r execute r>
  loop
  drop
;

\ Execute a counted loop with an arbitrary increment with an xt
: qcount+ ( ??? limit init xt -- ??? ) ( xt: ??? i -- ???? increment )
  -rot ?do
    i swap dup >r execute r> swap
  +loop
  drop
;

\ Iterate executing an xt over a byte array
: citer ( ??? addr count xt -- ??? ) ( xt: ??? b -- ??? )
  begin
    over 0>
  while
    dup >r swap >r swap dup >r c@ swap execute
    r> 1+ r> 1- r>
  repeat
  drop 2drop
;

\ Iterate executing an xt over a halfword array
: hiter ( ??? addr count xt -- ??? ) ( xt: ??? h -- ??? )
  begin
    over 0>
  while
    dup >r swap >r swap dup >r h@ swap execute
    r> 2+ r> 1- r>
  repeat
  drop 2drop
;

\ Iterate executing an xt over a cell array
: iter ( ??? addr count xt -- ??? ) ( xt: ??? x -- ??? )
  begin
    over 0>
  while
    dup >r swap >r swap dup >r @ swap execute
    r> cell+ r> 1- r>
  repeat
  drop 2drop
;

\ Iterate executing an xt over a double-word array
: 2iter ( ??? addr count xt -- ??? ) ( xt: ??? d -- ??? )
  begin
    over 0>
  while
    dup >r swap >r swap dup >r 2@ rot execute
    r> 2 cells + r> 1- r>
  repeat
  drop 2drop
;

\ Iterate executing at xt over values from a getter
: iter-get ( ??? get-xt count iter-xt -- ??? )
  ( get-xt: ??? i -- ??? x ) ( iter-xt: ??? x -- ??? )
  swap 0 ?do
    i swap dup >r rot dup >r rot swap execute swap execute r> r>
  loop
  drop drop
;

\ Iterate executing at xt over double-word values from a getter
: 2iter-get ( ??? get-xt count iter-xt -- ??? )
  ( get-xt: ??? i -- ??? d ) ( iter-xt: ??? d -- ??? )
  swap 0 ?do
    i swap dup >r rot dup >r rot swap execute rot execute r> r>
  loop
  drop drop
;

\ Find the index of a value in a byte array with a predicate
: cfind-index ( ??? b-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )
  swap 0 ?do
    dup >r swap dup >r c@ swap execute if
      rdrop rdrop i unloop exit
    else
      r> 1+ r>
    then
  loop
  drop drop -1
;

\ Find the index of a value in a halfword array with a predicate
: hfind-index ( ??? h-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )
  swap 0 ?do
    dup >r swap dup >r h@ swap execute if
      rdrop rdrop i unloop exit
    else
      r> 2+ r>
    then
  loop
  drop drop -1
;

\ Find the index of a value in a cell array with a predicate
: find-index ( ??? a-addr count xt -- ??? i|-1 ) ( xt: ??? x -- ??? f )
  swap 0 ?do
    dup >r swap dup >r @ swap execute if
      rdrop rdrop i unloop exit
    else
      r> cell+ r>
    then
  loop
  drop drop -1
;

\ Find the index of a value in a double-word array with a predicate
: 2find-index ( ??? a-addr count xt -- ??? i|-1 ) ( xt: ??? d -- ??? f )
  swap 0 ?do
    dup >r swap dup >r 2@ rot execute if
      rdrop rdrop i unloop exit
    else
      r> [ 2 cells ] literal + r>
    then
  loop
  drop drop -1
;

\ Find the index of a value from a getter with a predicate
: find-get-index ( ??? get-xt count pred-xt --- ??? i|-1 )
  ( get-xt: ??? i -- ??? x ) ( pred-xt: ??? x -- ??? f )
  swap 0 ?do
    i swap dup >r rot dup >r rot swap execute swap execute if
      rdrop rdrop i unloop exit
    else
      r> r>
    then
  loop
  drop drop -1
;

\ Find the index of a double-word value from a getter with a predicate
: 2find-get-index ( ??? get-xt count pred-xt --- ??? i|-1 )
  ( get-xt: ??? i -- ??? d ) ( pred-xt: ??? d -- ??? f )
  swap 0 ?do
    i swap dup >r rot dup >r rot swap execute rot execute if
      rdrop rdrop i unloop exit
    else
      r> r>
    then
  loop
  drop drop -1
;

\ Find a value in a byte array with a predicate
: cfind-value ( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )
  begin
    over 0>
  while
    dup >r swap >r swap dup >r c@ swap execute if
      r> c@ rdrop rdrop true exit
    else
      r> 1+ r> 1- r>
    then
  repeat
  drop drop drop 0 false
;

\ Find a value in a halfword array with a predicate
: hfind-value ( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )
  begin
    over 0>
  while
    dup >r swap >r swap dup >r h@ swap execute if
      r> h@ rdrop rdrop true exit
    else
      r> 2+ r> 1- r>
    then
  repeat
  drop drop drop 0 false
;

\ Find a value in a cell array with a predicate
: find-value ( ??? a-addr count xt -- ??? x|0 f ) ( xt: ??? x -- ??? f )
  begin
    over 0>
  while
    dup >r swap >r swap dup >r @ swap execute if
      r> @ rdrop rdrop true exit
    else
      r> cell+ r> 1- r>
    then
  repeat
  drop drop drop 0 false
;

\ Find a value in a double-word array with a predicate
: 2find-value ( ??? a-addr count xt -- ??? d|0 f ) ( xt: ??? d -- ??? f )
  begin
    over 0>
  while
    dup >r swap >r swap dup >r 2@ rot execute if
      r> 2@ rdrop rdrop true exit
    else
      r> [ 2 cells ] literal + r> 1- r>
    then
  repeat
  drop drop drop 0 0 false
;

\ Find a value from a getter with a predicate
: find-get-value ( ???? get-xt count pred-xt --- ??? x|0 f )
  ( get-xt: ??? i -- ??? x ) ( pred-xt: ??? x -- ??? f )
  swap 0 ?do
    i swap dup >r rot dup >r rot swap execute dup >r swap execute if
      r> rdrop rdrop unloop true exit
    else
      rdrop r> r>
    then
  loop
  drop drop 0 false
;

\ Find a double-word value from a getter with a predicate
: 2find-get-value ( ???? get-xt count pred-xt --- ??? d|0 f )
  ( get-xt: ??? i -- ??? d ) ( pred-xt: ??? d -- ??? f )
  swap 0 ?do
    i swap dup >r rot dup >r rot swap execute 2dup >r >r rot execute if
      r> r> rdrop rdrop unloop true exit
    else
      rdrop rdrop r> r>
    then
  loop
  drop drop 0 0 false
;

\ Ending compiling code in flash
end-compress-flash
compile-to-ram
