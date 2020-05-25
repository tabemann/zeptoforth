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

\ Reboot
reboot

\ Load the stack test functionlity to be tested
#include test/common/stack.fs

\ Compile this to RAM
compile-to-ram

\ Display a basic message on failure
: do-fail ( -- ) ." *" ;

\ Register fail handler
' do-fail stack-fail-hook !

\ A basic test of stack testing functionality
: stack-test-1 ( -- )
  ." stack-test-1" cr
  t{ 0 dup 0 0 2 }t
;

\ Another basic test of stack testing functionality
: stack-test-2 ( -- )
  ." stack-test-2" cr
  t{ 0 drop 0 }t
;

\ A test of stack testing stack depth failure functionality
: stack-test-3 ( -- )
  ." stack-test-3" cr
  t{ 1 dup 1 1 }t
;

\ Run the tests
stack-test-1
stack-test-2
stack-test-3