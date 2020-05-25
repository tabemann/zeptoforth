\ Copyright (c) 2020 Travis Bemann
\
\ This program is free software: you can redistribute it and/or modify
\ it under the terms of the GNU General Public License as published by
\ the Free Software Foundation, either version 3 of the License, or
\ (at your option) any later version.
\
\ This program is distributed in the hope that it will be useful,
\ but WITHOUT ANY WARRANTY; without even the implied warranty of
\ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
\ GNU General Public License for more details.
\
\ You should have received a copy of the GNU General Public License
\ along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
