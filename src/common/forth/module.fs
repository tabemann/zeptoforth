\ Copyright (c) 2021-2024 Travis Bemann
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
32 constant max-order

\ The temporary word stack size
128 constant temp-word-count

\ The temporary word stack index
variable temp-word-index

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

  \ The number of temporary words
  field: module-temp-word-count
end-structure

commit-flash

\ Temporary word stack
temp-word-count cells buffer: temp-word-stack

commit-flash

\ The module stack
module-entry-size module-stack-count * buffer: module-stack

\ Switch wordlists
forth set-current

\ Module stack overflow exception
: x-module-stack-overflow ( -- ) ." module stack overflow" cr ;

\ Module stack underflow exception
: x-module-stack-underflow ( -- ) ." module stack underflow" cr ;

\ Wordlist order overflow exception
: x-order-overflow ( -- ) ." wordlist order overflow" cr ;

\ Module already defined exception
: x-already-defined ( -- ) ." module already defined" cr ;

\ Module does not exist
: x-not-found ( -- ) ." module not found" cr ;

\ Temporary words overflow exception
: x-temp-words-overflow ( -- ) ." temporary words overflow" cr ;

\ Switch wordlists
internal set-current

commit-flash

\ Get the current module stack entry
: module-stack@ ( -- frame )
  module-stack-index @ 0<> averts x-module-stack-underflow
  module-stack module-stack-index @ 1- module-entry-size * +
;

commit-flash

\ Push a module stack entry
: push-stack ( wid -- )
  module-stack-index @ module-stack-count < averts x-module-stack-overflow
  1 module-stack-index +!
  module-stack@
  0 over module-temp-word-count !
  0 over module-count h!
  base @ over module-base h!
  get-current over module-old-current !
  over set-current
  module !
;

\ Drop a module stack entry
: drop-stack ( -- )
  module-stack-index @ 1 > averts x-module-stack-underflow
  module-stack@
  dup module-temp-word-count @ negate temp-word-index +!
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
: add-module ( wid -- )
  >r get-order dup max-order < r> swap averts x-order-overflow
  module-stack@ module-count h@ 0<> if
    rot drop swap module-stack@ module @ swap 1+ set-order
  else
    swap 1+ set-order
  then
  module-stack-index @ 0<> averts x-module-stack-underflow
  module-stack@
  0 over module-temp-word-count !
  1 swap module-count h+!
;

\ Remove a wordlist from the order
: remove-module ( wid -- )
  module-stack-index @ 0 > averts x-module-stack-underflow
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

\ The old find hook
variable old-find-hook

\ Find the first path separator in a name
: find-path-sep ( c-addr u -- u'|-1 )
  swap 1+ swap 1-
  1 begin over 2 > while
    2 pick c@ [char] : = if
      2 pick 1+ c@ [char] : = if
        nip nip exit
      then
    then
    rot 1+ rot 1- rot 1+
  repeat
  2drop drop -1
;

\ Execute or compile a particular word in a provided module
: do-find-with-module ( c-addr u -- word|0 )
  module-stack-index @ 0> if
    temp-word-index @ module-stack-index @ begin ?dup while
      1-
      dup module-entry-size * module-stack +
      dup module-temp-word-count @ 0> if
        2 pick swap module-temp-word-count @ - 2 pick 1- do
          2over
          i cells temp-word-stack + @ word-name count equal-case-strings? if
            2over 2>r dup >r
            get-order
            r> module-entry-size * module-stack + module @ 1 set-order
            2r> old-find-hook @ execute ?dup if
              >r set-order r> nip
            else
              set-order i cells temp-word-stack + @
            then
            -rot 2drop -rot 2drop unloop exit
          then
        -1 +loop
        tuck module-entry-size * module-stack + module-temp-word-count @ - swap
      else
        drop
      then
    repeat
    drop
  then
  2dup find-path-sep dup -1 <> if
    2 pick over old-find-hook @ execute ?dup if
      >r 2 + tuck - -rot + swap r> -rot 2>r
      >r get-order r> >xt execute 1 set-order
      2r> find >r set-order r>
    else
      2drop drop 0
    then
  else
    drop old-find-hook @ execute
  then
;

\ Add word to the temporary words
: add-temp-word ( word -- )
  temp-word-index @ cells temp-word-stack + !
  1 temp-word-index +!
  1 module-stack@ module-temp-word-count +!
;

\ Switch wordlists
forth set-current

commit-flash

\ Begin a module definition
: begin-module ( "name" -- )
  token dup 0<> averts x-token-expected
  2dup find ?dup if
    ['] x-already-defined ?raise
  else
    wordlist dup >r -rot constant-with-name r>
  then
  dup push-stack
  add-module
;

\ Continue an existing module definition
: continue-module ( "name" -- )
  token dup 0<> averts x-token-expected
  2dup find ?dup if
    nip nip >xt execute
  else
    ['] x-not-found ?raise
  then
  dup push-stack
  add-module
;

\ Start a private module definition
: private-module ( -- ) wordlist dup push-stack add-module ;

\ End a module definition
: end-module ( -- ) drop-stack ;

\ End a module definition and place the module on the stack
: end-module> ( -- module )
  module-stack-index @ 1 > averts x-module-stack-underflow
  module-stack@ module @
  drop-stack
;

\ Import a module
: import ( module -- ) add-module ;

\ Import an individual word from a module
: import-from ( module "name" -- )
  temp-word-index @ module-stack-index @ 2>r
  0 temp-word-index ! 0 module-stack-index !
  >r get-order r> 1 set-order
  token find ?dup if
    2r> module-stack-index ! temp-word-index !
    temp-word-index @ temp-word-count < if
      add-temp-word
      set-order
    else
      drop
      set-order
      ['] x-temp-words-overflow ?raise
    then
  else
    set-order
    2r> module-stack-index ! temp-word-index !
    ['] x-unknown-word ?raise
  then
;

\ Import multiple words from a module
: begin-imports-from ( module "name0" ... "namen" "end-imports-from" -- )
  >r get-order r> 1 set-order
  begin
    token
    dup 0= if
      2drop display-prompt refill false
    else
      2dup s" end-imports-from" equal-case-strings? not if
        temp-word-index @ module-stack-index @ 2>r
        0 temp-word-index ! 0 module-stack-index !
        find ?dup if
          2r> module-stack-index ! temp-word-index !
          temp-word-index @ temp-word-count < if
            add-temp-word false
          else
            drop
            set-order
            ['] x-temp-words-overflow ?raise
          then
        else
          set-order
          2r> module-stack-index ! temp-word-index !
          ['] x-unknown-word ?raise
        then
      else
        2drop
        set-order
        true
      then
    then
  until
;

\ Unimport a module import
: unimport ( module -- ) remove-module ;

\ Export a word from the current module
: export ( xt "name" -- )
  token dup 0<> averts x-token-expected
  start-compile visible
  compile, end-compile,
;

\ Initialize
: init ( -- )
  init
  0 module-stack-index !
  0 temp-word-index !
  forth push-stack
  forth add-module
  find-hook @ old-find-hook !
  ['] do-find-with-module find-hook !
;

forth 1 set-order
forth set-current

end-compress-flash

\ Reboot
reboot
