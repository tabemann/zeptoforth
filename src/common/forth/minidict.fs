\ Copyright (c) 2022 Travis Bemann
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

compile-to-flash

internal import

begin-module mini-dict

  armv6m import
  
  variable flash-mini-dict-free

  \ Flash mini-dictionary is out of space exception
  : x-flash-mini-dict-out-of-space ( -- )
    ." flash mini-dictionary is out of space" cr
  ;

  \ Hash a counted string
  : hash-string ( c-addr bytes -- hash )
    code[
    r0 1 dp ldm
    tos r0 r1 adds_,_,_
    mark>
    tos r2 movs_,_
    7 tos tos lsls_,_,#_
    25 r2 r2 lsrs_,_,#_
    r2 tos orrs_,_
    0 r0 r2 ldrb_,[_,#_]
    $61 r2 cmp_,#_
    lo bc>
    $7A r2 cmp_,#_
    hi bc>
    $20 r2 subs_,#_
    >mark
    >mark
    r2 tos eors_,_
    r1 r0 cmp_,_
    eq bc<
    ]code
  ;

  \ Hash an identifier by wordlist
  : hash-string-and-wid ( c-addr bytes wid -- hash )
    code[
    tos 1 push
    tos 1 dp ldm
    ]code
    hash-string
    code[
    r0 1 pop
    r0 tos eors_,_
    0 tos cmp_,#_
    ne bc>
    1 tos adds_,#_
    >mark
    ]code
  ;

  \ Hash a dictionary entry
  : hash-word ( word -- hash )
    code[
    8 tos r0 ldrb_,[_,#_]
    tos r1 movs_,_
    9 r1 adds_,#_
    2 tos tos ldrh_,[_,#_]
    8 dp subs_,#_
    0 dp r0 str_,[_,#_]
    4 dp r1 str_,[_,#_]
    ]code
    hash-string-and-wid
  ;

  \ Clear the flash mini-dictionary
  : clear-flash-mini-dict ( -- )
    flash-mini-dict-size 2 cells / 1- flash-mini-dict-free !
    flash-mini-dict-size flash-mini-dict
    code[
    r0 1 dp ldm
    0 r1 movs_,#_
    mark>
    4 r0 subs_,#_
    r0 tos r1 str_,[_,_]
    0 r0 cmp_,#_
    ne bc<
    tos 1 dp ldm
    ]code
  ;

  \ Register space being used in the flash mini-dictionary
  : register-flash-mini-dict-space ( -- )
    flash-mini-dict-free @ averts x-flash-mini-dict-out-of-space
    -1 flash-mini-dict-free +!
  ;

  \ Compare two different words
  : equal-words? ( word0 word1 -- equal? )
    code[
    r0 1 dp ldm
    2 tos r1 ldrh_,[_,#_]
    2 r0 r2 ldrh_,[_,#_]
    r1 r2 cmp_,_
    ne bc>
    12 dp subs_,#_
    tos r1 movs_,_
    9 r1 adds_,#_
    8 dp r1 str_,[_,#_]
    8 tos r1 ldrb_,[_,#_]
    4 dp r1 str_,[_,#_]
    r0 r1 movs_,_
    9 r1 adds_,#_
    0 dp r1 str_,[_,#_]
    8 r0 tos ldrb_,[_,#_]
    ]code
    equal-case-strings?
    code[
    pc 1 pop
    >mark
    0 tos movs_,#_
    pc 1 pop
    ]code
  ;

  \ Add an entry to the flash mini-dictionary when filling start to end
  : add-flash-mini-dict-end ( word -- )
    dup word-flags h@ visible-flag and 0= if drop exit then
    dup hash-word dup
    [ flash-mini-dict-size 2 cells / ] literal umod
    flash-mini-dict
    [ flash-mini-dict-size 2 cells / ] literal rot
    ( word hash flash-mini-dict flash-mini-dict-size index )
    code[
    r5 r4 2 push
    r4 r3 r2 r1 4 dp ldm
    mark>
    3 tos r0 lsls_,_,#_
    r2 r0 r0 adds_,_,_
    4 r0 r5 ldr_,[_,#_]
    0 r5 cmp_,#_
    eq bc>
    tos r4 r3 r2 r1 r0 6 push
    4 dp subs_,#_
    0 dp r4 str_,[_,#_]
    r5 tos movs_,_
    ]code
    equal-words?
    code[
    0 tos cmp_,#_
    eq bc>
    tos r4 r3 r2 r1 r0 6 pop
    0 r0 r3 str_,[_,#_]
    4 r0 r4 str_,[_,#_]
    tos 1 dp ldm
    pc r5 r4 3 pop
    >mark
    tos r4 r3 r2 r1 r0 6 pop
    1 tos adds_,#_
    r1 tos cmp_,_
    2over ne bc<
    0 tos movs_,#_
    2swap b<
    >mark
    0 r0 r3 str_,[_,#_]
    4 r0 r4 str_,[_,#_]
    tos 1 dp ldm
    r5 r4 2 pop
    ]code
    register-flash-mini-dict-space
  ;
  
  \ Add an entry to the flash mini-dictionary when filling end to start
  : add-flash-mini-dict-start ( word -- )
    dup word-flags h@ visible-flag and 0= if drop exit then
    dup hash-word dup
    [ flash-mini-dict-size 2 cells / ] literal umod
    flash-mini-dict
    [ flash-mini-dict-size 2 cells / ] literal rot
    ( word hash flash-mini-dict flash-mini-dict-size index )
    code[
    r5 r4 2 push
    r4 r3 r2 r1 4 dp ldm
    mark>
    3 tos r0 lsls_,_,#_
    r2 r0 r0 adds_,_,_
    4 r0 r5 ldr_,[_,#_]
    0 r5 cmp_,#_
    eq bc>
    tos r4 r3 r2 r1 r0 6 push
    4 dp subs_,#_
    0 dp r4 str_,[_,#_]
    r5 tos movs_,_
    ]code
    equal-words?
    code[
    0 tos cmp_,#_
    eq bc>
    tos r4 r3 r2 r1 r0 6 pop
    tos 1 dp ldm
    pc r5 r4 3 pop
    >mark
    tos r4 r3 r2 r1 r0 6 pop
    1 tos adds_,#_
    r1 tos cmp_,_
    2over ne bc<
    0 tos movs_,#_
    2swap b<
    >mark
    0 r0 r3 str_,[_,#_]
    4 r0 r4 str_,[_,#_]
    tos 1 dp ldm
    r5 r4 2 pop
    ]code
    register-flash-mini-dict-space
  ;
  
  \ Initialize the flash mini-dictionary
  : init-flash-mini-dict ( -- )
    clear-flash-mini-dict
    flash-latest
    begin ?dup while
      dup add-flash-mini-dict-start
      next-word @
    repeat
  ;

  \ Find a word in the flash mini-dictionary
  : find-flash-mini-dict ( c-addr bytes wid -- addr|0 )
    3dup hash-string-and-wid dup
    [ flash-mini-dict-size 2 cells / ] literal umod
    flash-mini-dict
    [ flash-mini-dict-size 2 cells / ] literal rot
    ( b-addr bytes wid hash flash-mini-dict flash-mini-dict-size index )
    code[
    r5 r4 2 push
    r4 r3 r2 r1 4 dp ldm
    mark>
    3 tos r0 lsls_,_,#_
    r2 r0 r0 adds_,_,_
    0 r0 r5 ldr_,[_,#_]
    r5 r3 cmp_,_
    ne bc>
    4 r0 r5 ldr_,[_,#_]
    2 r5 r5 ldrh_,[_,#_]
    r5 r4 cmp_,_
    ne bc>
    tos r4 r3 r2 r1 r0 6 push
    4 r0 r5 ldr_,[_,#_]
    8 r5 r1 ldrb_,[_,#_]
    r5 r2 movs_,_
    9 r2 adds_,#_
    r5 1 push
    0 dp r3 ldr_,[_,#_]
    4 dp r4 ldr_,[_,#_]
    12 dp subs_,#_
    0 dp r2 str_,[_,#_]
    4 dp r3 str_,[_,#_]
    8 dp r4 str_,[_,#_]
    r1 tos movs_,_
    ]code
    equal-case-strings?
    code[
    0 tos cmp_,#_
    eq bc>
    tos 1 pop
    8 dp adds_,#_
    r5 r4 r3 r2 r1 r0 6 pop
    pc r5 r4 3 pop
    >mark
    r5 1 pop
    tos r4 r3 r2 r1 r0 6 pop
    >mark
    1 tos adds_,#_
    r1 tos cmp_,_
    2over ne bc<
    0 tos movs_,#_
    2over b<
    >mark
    0 r5 cmp_,#_
    ne bc>
    0 tos movs_,#_
    8 dp adds_,#_
    pc r5 r4 3 pop
    >mark
    1 tos adds_,#_
    r1 tos cmp_,_
    2dup ne bc<
    0 tos movs_,#_
    b<
    ]code
  ;
  
  \ Add a word to the flash mini-dictionary
  : add-flash-mini-dict ( -- )
    compiling-to-flash? if
      flash-latest add-flash-mini-dict-end
    then
  ;

  \ Find a word in a particular dictionary, while making use of the flash
  \ mini-dictionary
  : find-optimized-wid ( b-addr bytes wid -- addr|0 )
    compiling-to-flash? if
      find-flash-mini-dict
    else
      3dup ram-latest swap find-dict ?dup if
	nip nip nip
      else
	find-flash-mini-dict
      then
    then
  ;

  \ Find a word using the flash mini-dictionary for optimization
  : find-optimized ( b-addr bytes -- addr|0 )
    order-count @ 1 lshift order + order
    code[
    tos r0 movs_,_
    r3 r2 r1 3 dp ldm
    mark>
    8 dp subs_,#_
    0 dp r2 str_,[_,#_]
    4 dp r3 str_,[_,#_]
    0 r0 tos ldrh_,[_,#_]
    r3 r2 r1 r0 4 push
    ]code
    find-optimized-wid
    code[
    r3 r2 r1 r0 4 pop
    0 tos cmp_,#_
    eq bc>
    pc 1 pop
    >mark
    2 r0 adds_,#_
    r1 r0 cmp_,_
    ne bc<
    ]code
  ;

end-module> import

\ Initialize the minidictionary
: init ( -- )
  init
  init-flash-mini-dict
  ['] add-flash-mini-dict finalize-hook !
  ['] find-optimized find-hook !
;

reboot