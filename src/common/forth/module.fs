\ Copyright (c) 2021 Travis Bemann
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

\ Compile to flash
compile-to-flash

\ Set up the wordlist
forth-wordlist 1 set-order
forth-wordlist set-current

\ Actually set up the wordlist
internal-wordlist forth-wordlist 2 set-order
internal-wordlist set-current

\ The module stack's size in entries
5 constant module-stack-count

\ The maximum wordlist order
16 constant max-wordlist-order

\ The current module stack index
variable module-stack-index

\ The module stack entry
begin-structure module-entry-size
  \ The module stack wordlist count
  hfield: module-wordlist-count

  \ The module stack saved base
  hfield: module-base

  \ The module's wordlist
  field: module-wordlist
end-structure

\ The module stack
module-entry-size module-stack-count * buffer: module-stack

\ Get the current module stack entry
: module-stack@ ( -- frame | 0 )
  module-stack-index @ 0<> if
    module-stack module-stack-index @ 1- module-entry-size * +
  else
    0
  then
;

\ Switch wordlists
forth-wordlist set-current

\ Module stack overflow exception
: x-module-stack-overflow ( -- ) space ." module stack overflow" cr ;

\ Module stack underflow exception
: x-module-stack-underflow ( -- ) space ." module stack underflow" cr ;

\ Wordlist order overflow exception
: x-order-overflow ( -- ) space ." wordlist order overflow" cr ;

\ Switch wordlists
internal-wordlist set-current

\ Push a module stack entry
: push-module-stack ( wid -- )
  module-stack-index @ module-stack-count < averts x-module-stack-overflow
  1 module-stack-index +!
  module-stack@
  0 over module-wordlist-count h!
  base @ over module-base h!
  module-wordlist !
;

\ Pop a module stack entry
: pop-module-stack ( -- )
  module-stack-index @ 1 > averts x-module-stack-underflow
  module-stack@
  module-base h@ base !
  get-order module-stack@ module-wordlist-count h@ begin dup 0<> while
    1- swap 1- swap rot drop
  repeat
  drop
  set-order
  -1 module-stack-index +!
  module-stack-index @ 0<> if
    module-stack@ module-wordlist @ set-current
  else
    forth-wordlist set-current
  then
;

\ Add a wordlist to the order
: add-wordlist ( wid -- )
  >r get-order dup max-wordlist-order < r> swap averts x-order-overflow
  swap 1+ set-order
  module-stack-index @ 0<> averts x-module-stack-underflow
  module-stack@
  1 swap module-wordlist-count h+!
;

\ Remove a wordlist from the order
: remove-wordlist ( wid -- )
  module-stack-index @ 0 > averts x-module-stack-underflow
  >r get-order module-stack@ module-wordlist-count h@ begin dup 0<> while
    dup 1+ pick r@ = if
      dup 1+ roll drop
      -1 module-stack@ module-wordlist-count h+!
      swap 1- swap
    then
    1-
  repeat
  drop rdrop set-order
;

\ Switch wordlists
forth-wordlist set-current

\ Module already defined exception
: x-module-already-defined ( -- ) space ." module already defined" cr ;

\ Module does not exist
: x-module-not-found ( -- ) space ." module not found" cr ;

\ Begin a module definition
: begin-module ( "name" -- )
  token 2dup visible-flag find-all ?dup if
    nip nip >body execute
  else
    wordlist dup >r -rot constant-with-name r>
  then
  dup push-module-stack
  dup add-wordlist
  set-current
;

\ Begin a once-defined module definition
: begin-module-once ( "name" -- )
  token 2dup visible-flag find-all ?dup if
    ['] x-module-already-defined ?raise
  else
    wordlist dup >r -rot constant-with-name r>
  then
  dup push-module-stack
  dup add-wordlist
  set-current
;

\ Begin a module definition and import it
: begin-import-module ( "name" -- )
  token 2dup visible-flag find-all ?dup if
    nip nip >body execute
  else
    wordlist dup >r -rot constant-with-name r>
  then
  dup add-wordlist
  dup push-module-stack
  set-current
;

\ Begin a once-defined module definition and import it
: begin-import-module-once ( "name" -- )
  token 2dup visible-flag find-all ?dup if
    ['] x-module-already-defined ?raise
  else
    wordlist dup >r -rot constant-with-name r>
  then
  dup add-wordlist
  dup push-module-stack
  set-current
;

\ End a module definition
: end-module ( -- ) pop-module-stack ;

\ Import a module
: import ( "name" -- )
  token visible-flag find-all ?dup if
    >body execute add-wordlist
  else
    ['] x-module-not-found ?raise
  then
;

\ Unimport a module
: unimport ( "name" -- )
  token visible-flag find-all ?dup if
    >body execute remove-wordlist
  else
    ['] x-module-not-found ?raise
  then
;

\ Initialize
: init ( -- )
  init
  0 module-stack-index !
  forth-wordlist push-module-stack
;

forth-wordlist 1 set-order
forth-wordlist set-current

\ Warm reboot
warm
