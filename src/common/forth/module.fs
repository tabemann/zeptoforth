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
forth-module 1 set-order
forth-module set-current

\ Actually set up the wordlist
internal-module forth-module 2 set-order
internal-module set-current

\ The module stack's size in entries
5 constant module-stack-count

\ The maximum wordlist order
16 constant max-module-order

\ The current module stack index
variable module-stack-index

compress-flash

\ The module stack entry
begin-structure module-entry-size
  \ The module stack wordlist count
  hfield: module-module-count

  \ The module stack saved base
  hfield: module-base

  \ The module's wordlist
  field: module-module
end-structure

commit-flash

\ The module stack
module-entry-size module-stack-count * buffer: module-stack

\ Switch wordlists
forth-module set-current

\ Module stack overflow exception
: x-module-stack-overflow ( -- ) space ." module stack overflow" cr ;

\ Module stack underflow exception
: x-module-stack-underflow ( -- ) space ." module stack underflow" cr ;

\ Wordlist order overflow exception
: x-order-overflow ( -- ) space ." wordlist order overflow" cr ;

\ Module already defined exception
: x-module-already-defined ( -- ) space ." module already defined" cr ;

\ Module does not exist
: x-module-not-found ( -- ) space ." module not found" cr ;

\ Switch wordlists
internal-module set-current

commit-flash

\ Get the current module stack entry
: module-stack@ ( -- frame | 0 )
  module-stack-index @ 0<> if
    module-stack module-stack-index @ 1- module-entry-size * +
  else
    0
  then
;

commit-flash

\ Push a module stack entry
: push-module-stack ( wid -- )
  module-stack-index @ module-stack-count < averts x-module-stack-overflow
  1 module-stack-index +!
  module-stack@
  0 over module-module-count h!
  base @ over module-base h!
  module-module !
;

\ Pop a module stack entry
: pop-module-stack ( -- )
  module-stack-index @ 1 > averts x-module-stack-underflow
  module-stack@
  module-base h@ base !
  get-order module-stack@ module-module-count h@ begin dup 0<> while
    1- swap 1- swap rot drop
  repeat
  drop
  set-order
  -1 module-stack-index +!
  module-stack-index @ 0<> if
    module-stack@ module-module @ set-current
  else
    forth-module set-current
  then
;

\ Add a wordlist to the order
: add-module ( wid -- )
  >r get-order dup max-module-order < r> swap averts x-order-overflow
  module-stack@ module-module-count h@ 0<> if
    rot drop swap module-stack@ module-module @ swap 1+ set-order
  else
    swap 1+ set-order
  then
  module-stack-index @ 0<> averts x-module-stack-underflow
  module-stack@
  1 swap module-module-count h+!
;

\ Remove a wordlist from the order
: remove-module ( wid -- )
  module-stack-index @ 0 > averts x-module-stack-underflow
  >r get-order module-stack@ module-module-count h@ begin dup 0<> while
    dup 1+ pick r@ = if
      dup 1+ roll drop
      -1 module-stack@ module-module-count h+!
      swap 1- swap
    then
    1-
  repeat
  drop rdrop set-order
;

\ Execute or compile a particular word in a provided module
: lookup-path ( "module-name" "word-name" -- c-addr bytes word )
  get-order dup begin ?dup while 1- rot >r repeat >r
  [:
    begin
      token 2dup s" ::" equal-strings? not if
	visible-flag find ?dup if
	  >body execute 1 set-order
	else
	  ['] x-module-not-found ?raise
	then
	false
      else
	2drop token 2dup visible-flag find dup 0= triggers x-unknown-word true
      then
    until
  ;] try r> dup begin ?dup while 1- r> -rot repeat set-order ?raise
;

\ Switch wordlists
forth-module set-current

commit-flash

\ Begin a module definition
: begin-module ( "name" -- )
  token 2dup visible-flag find ?dup if
    ['] x-module-already-defined ?raise
  else
    wordlist dup >r -rot constant-with-name r>
  then
  dup push-module-stack
  dup add-module
  set-current
;

\ Continue an existing module definition
: continue-module ( "name" -- )
  token 2dup visible-flag find ?dup if
    nip nip >body execute
  else
    ['] x-module-not-found ?raise
  then
  dup push-module-stack
  dup add-module
  set-current
;

\ Begin a module definition and import it
: begin-import-module ( "name" -- )
  token 2dup visible-flag find ?dup if
    ['] x-module-already-defined ?raise
  else
    wordlist dup >r -rot constant-with-name r>
  then
  dup add-module
  dup push-module-stack
  dup add-module
  set-current
;

\ Continue an existing module definition and import it
: continue-import-module ( "name" -- )
  token 2dup visible-flag find ?dup if
    nip nip >body execute
  else
    ['] x-module-not-found ?raise
  then
  dup add-module
  dup push-module-stack
  dup add-module
  set-current
;

\ End a module definition
: end-module ( -- ) pop-module-stack ;

\ Import a module
: import ( "name" -- )
  token visible-flag find ?dup if
    >body execute add-module
  else
    ['] x-module-not-found ?raise
  then
;

\ Unimport a module
: unimport ( "name" -- )
  token visible-flag find ?dup if
    >body execute remove-module
  else
    ['] x-module-not-found ?raise
  then
;

\ Export a word from the current module
: export ( "name" -- )
  token 2dup s" ^" equal-strings? if
    2drop lookup-path
  else
    2dup visible-flag find dup 0= triggers x-unknown-word
  then
  -rot start-compile visible immediate lit, postpone apply end-compile,
;

\ Execute or compile a particular word in a provided module
: ^ ( "module-name" "word-name" -- ) [immediate] lookup-path nip nip apply ;

\ Initialize
: init ( -- )
  init
  0 module-stack-index !
  forth-module push-module-stack
  forth-module add-module
;

forth-module 1 set-order
forth-module set-current

end-compress-flash

\ Reboot
reboot
