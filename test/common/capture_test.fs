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

\ Load the capture functionality to be tested
#include test/common/capture.fs

\ Compile this to RAM
compile-to-ram

\ Display a basic message on failure
: do-fail ( -- ) [: ." *" ;] no-capture ;

\ Register fail handler
' do-fail capture-fail-hook !

\ A basic test of matching functionality
: match-test-1
  clear-capture
  [: ." match-test-1" cr ;] no-capture
  s" FOO" 0 add-match-capture
  s" BAR" 0 add-match-capture
  s" BAZ" 0 add-match-capture
  ." FOOBARBAZ"
;

\ A basic test of matching functionality
: match-test-2
  clear-capture
  [: ." match-test-2" cr ;] no-capture
  s" FOO" 0 add-match-capture
  s" BAR" 0 add-match-capture
  s" BAZ" 0 add-match-capture
  ." FOOBAZBAZ"
;

\ A basic test of skipping functionality
: skip-test-1 ( -- )
  clear-capture
  [: ." skip-test-1" cr ;] no-capture
  s" BAR" 256 0 add-skip-capture
  s" BAZ" 0 add-match-capture
  ." FOOBARBAZ"
;

\ A basic test of ignoring functionality
: ignore-test-1 ( -- )
  clear-capture
  [: ." ignore-test-1" cr ;] no-capture
  s" FOO" 256 0 add-ignore-capture
  s" BAR" 0 add-match-capture
  ." FOOFOOBAR"
;

\ A basic test of ignoring functionality
: ignore-test-2 ( -- )
  clear-capture
  [: ." ignore-test-2" cr ;] no-capture
  s" FOO" 256 0 add-ignore-capture
  s" BAR" 0 add-match-capture
  ." FOOFOBAR"
;

\ Run the tests
enable-capture
match-test-1
match-test-2
skip-test-1
ignore-test-1
ignore-test-2
disable-capture