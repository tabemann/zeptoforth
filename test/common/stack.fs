\ Copyright (c) 2020-2025 Travis Bemann
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

\ Set up the wordlist
get-order get-current
forth 1 set-order
forth set-current

\ The stack fail hook
variable stack-fail-hook

\ Stack fail exception
: x-stack-fail ( -- ) space ." stack test failure" cr ;

\ Save the stack position to the return stack, will be used later
: t{ ( -- ) ( R: -- sp ) sp@ r> swap >r >r ;

\ Test the stack with a given number of cells on the stack greater than those
\ on the stack for the preceding word times two (to cover the cells put on the
\ stack for testing the cells on the stack), popping each pair of values off
\ the stack as they are being tested; note that in addition to calling the
\ stack fail hook on failure, this also raises an exception if the stack
\ size was unexpected, as the stack will be in an unknown state.
: }t ( yn ... y1 xn .. x1 count -- ) ( R: old-sp -- )
  >r sp@ r> r> r> swap >r over >r swap
  dup 0> if 2 cells else cell then * - <> if
    stack-fail-hook @ ?execute ['] x-stack-fail ?raise
  else
    r> ( ??? count )
    dup 0< if negate 0 ?do 0 loop exit then
    begin
      dup 0>
    while
      dup 1+ roll rot <> if stack-fail-hook @ ?execute then 1-
    repeat
  then
  drop
;

\ Test the stack depth after executing code and return the stack to its
\ original depth without checking the values on the stack (useful for testing
\ words for which the values put on the stack are not necessarily known); note
\ that in addition to calling the stack fail hook on failure, this also raises
\ an exception if the stack size was unexpected, as the stack will be in an
\ unknown state.
: }t-depth ( yn ... y1 count -- ) ( R: old-sp -- )
  >r sp@ r> r> r> swap >r over >r swap cells - <> if
    stack-fail-hook @ ?execute ['] x-stack-fail ?raise
  else
    r> ( ??? count )
    dup 0< if negate 0 ?do 0 loop exit then
    begin dup 0> while nip 1- repeat
  then
  drop
;

\ Test the stack with a given number of cells on the stack greater than those
\ on the stack for the preceding word times two (to cover the cells put on the
\ stack for testing the cells on the stack), popping each value being compared
\ with off the stack as they are being tested while preserving the values they
\ are being compared against; note that in addition to calling the stack fail
\ hook on failure, this also raises an exception if the stack size was
\ unexpected, as the stack will be in an unknown state.
: }t-preserve ( yn ... y1 xn .. x1 count -- yn ... y1 ) ( R: old-sp -- )
  >r sp@ r> r> r> swap >r over >r swap
  dup 0> if 2 cells else cell then * - <> if
    stack-fail-hook @ ?execute ['] x-stack-fail ?raise
  else
    r> ( ??? count )
    dup 0< if drop exit then
    dup
    begin
      dup 0>
    while
      rot >r over 1+ pick r> <> if stack-fail-hook @ ?execute then 1-
    repeat
  then
  2drop
;

\ Test the stack depth after executing code and leave the stack as is
\ afterwards if the stack depth is as expected  without checking the values on
\ the stack (useful for testing words for which the values put on the stack are
\ not necessarily known); note that in addition to calling the stack fail hook
\ on failure, this also raises an exception if the stack size was unexpected,
\ as the stack will be in an unknown state.
: }t-depth-preserve ( yn ... y1 count -- yn ... y1 ) ( R: old-sp -- )
  >r sp@ r> r> r> swap >r swap cells - <> if
    stack-fail-hook @ ?execute ['] x-stack-fail ?raise
  then
;

\ Initialize the stack fail hook
: init-stack-fail-hook ( -- )
  0 stack-fail-hook !
;
initializer init-stack-fail-hook

set-current set-order
