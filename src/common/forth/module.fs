\ Copyright (c) 2021-2022 Travis Bemann
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
forth 1 set-order
forth set-current

\ Actually set up the wordlist
internal forth 2 set-order
internal set-current

\ The module stack's size in entries
5 constant module-stack-count

\ The maximum wordlist order
16 constant max-order

\ The current module stack index
variable module-stack-index

compress-flash

\ The module stack entry
begin-structure module-entry-size
  \ The module stack wordlist count
  hfield: module-count

  \ The module stack saved base
  hfield: module-base

  \ The module's wordlist
  field: module

  \ The previous module's old current wordlist
  field: module-old-current
end-structure

commit-flash

\ The module stack
module-entry-size module-stack-count * buffer: module-stack

\ Switch wordlists
forth set-current

\ Module stack overflow exception
: x-stack-overflow ( -- ) space ." module stack overflow" cr ;

\ Module stack underflow exception
: x-stack-underflow ( -- ) space ." module stack underflow" cr ;

\ Wordlist order overflow exception
: x-order-overflow ( -- ) space ." wordlist order overflow" cr ;

\ Module already defined exception
: x-already-defined ( -- ) space ." module already defined" cr ;

\ Module does not exist
: x-not-found ( -- ) space ." module not found" cr ;

\ Switch wordlists
internal set-current

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
: push-stack ( wid -- )
  module-stack-index @ module-stack-count < averts x-stack-overflow
  1 module-stack-index +!
  module-stack@
  0 over module-count h!
  base @ over module-base h!
  get-current over module-old-current !
  over set-current
  module !
;

\ Drop a module stack entry
: drop-stack ( -- )
  module-stack-index @ 1 > averts x-stack-underflow
  module-stack@
  dup module-old-current @ set-current
  module-base h@ base !
  get-order module-stack@ module-count h@ begin dup 0<> while
    1- swap 1- swap rot drop
  repeat
  drop
  set-order
  -1 module-stack-index +!
;

\ Add a wordlist to the order
: add ( wid -- )
  >r get-order dup max-order < r> swap averts x-order-overflow
  module-stack@ module-count h@ 0<> if
    rot drop swap module-stack@ module @ swap 1+ set-order
  else
    swap 1+ set-order
  then
  module-stack-index @ 0<> averts x-stack-underflow
  module-stack@
  1 swap module-count h+!
;

\ Remove a wordlist from the order
: remove ( wid -- )
  module-stack-index @ 0 > averts x-stack-underflow
  >r get-order module-stack@ module-count h@ begin dup 0<> while
    dup 1+ pick r@ = if
      dup 1+ roll drop
      -1 module-stack@ module-count h+!
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
	  ['] x-not-found ?raise
	then
	false
      else
	2drop token 2dup visible-flag find dup 0= triggers x-unknown-word true
      then
    until
  ;] try r> dup begin ?dup while 1- r> -rot repeat set-order ?raise
;

\ Switch wordlists
forth set-current

commit-flash

\ Begin a module definition
: begin-module ( "name" -- )
  token 2dup visible-flag find ?dup if
    ['] x-already-defined ?raise
  else
    wordlist dup >r -rot constant-with-name r>
  then
  dup push-stack
  add
;

\ Continue an existing module definition
: continue-module ( "name" -- )
  token 2dup visible-flag find ?dup if
    nip nip >body execute
  else
    ['] x-not-found ?raise
  then
  dup push-stack
  add
;

\ Start a private module definition
: private-module ( -- ) wordlist dup push-stack add ;

\ End a module definition
: end-module ( -- ) drop-stack ;

\ End a module definition and place the module on the stack
: end-module> ( -- module )
  module-stack-index @ 1 > averts x-stack-underflow
  module-stack@ module @
  drop-stack
;

\ Import a module
: import ( module -- ) add ;

\ Una module import
: unimport ( module -- ) remove ;

\ Export a word from the current module
: export ( "name" -- )
  token 2dup s" ^" equal-strings? if
    2drop lookup-path
  else
    2dup visible-flag find dup 0= triggers x-unknown-word
  then
  -rot start-compile visible >body compile, end-compile,
;

\ Execute or compile a particular word in a provided module
: ^ ( "module-name" "word-name" -- ) [immediate] lookup-path nip nip apply ;

\ Initialize
: init ( -- )
  init
  0 module-stack-index !
  forth push-stack
  forth add
;

forth 1 set-order
forth set-current

end-compress-flash

\ Reboot
reboot
