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

\ Set up the wordlist
get-order get-current
forth-module 1 set-order
forth-module set-current

\ The stack fail hook
variable stack-fail-hook

compiling-to-flash? not [if]

  \ Initialize the stack fail hook
  0 stack-fail-hook !

[then]

\ Stack fail exception
: x-stack-fail ( -- ) space ." stack test failure" cr ;

\ Save the stack position to the return stack, will be used later
: t{ ( -- ) ( R: -- sp ) sp@ r> swap >r >r ;

\ Test the stack with a given number of cells on the stack greater than those
\ on the stack for the preceding word times two (to cover the cells put on the
\ stack for testing the cells on the stack); note that in addition to calling
\ the stack fail hook on failure, this also raises an exception if the stack
\ size was unexpected, as the stack will be in an unknown state.
: }t ( yn ... y1 xn .. x1 count -- ) ( R: old-sp -- )
  >r sp@ r> r> r> swap >r over >r swap 2 cells * - <> if
    stack-fail-hook @ ?execute ['] x-stack-fail ?raise
  else
    r> ( ??? count ) begin
      dup 0>
    while
      dup 1+ roll rot = if
	1 -
      else
	stack-fail-hook @ ?execute 1-
      then
    repeat
  then
;

compiling-to-flash? [if]

  \ Initialize
  : init ( -- )
    init
    0 stack-fail-hook !
  ;

[then]

set-current set-order
