\ Copyright (c) 2024 Travis Bemann
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

begin-module implicit-internal

  internal import

  \ Saved RAM HERE
  0 value saved-ram-here

  \ Saved RAM latest
  0 value saved-ram-latest

  \ Saved compiling to flash
  false value saved-compiling-to-flash

  \ Test whether there is any outer syntax that is relevant
  : outer-syntax? ( -- flag )
    false { no-implicit }
    dump-syntax 0 ?do
      case
        syntax-begin-structure of false endof
        syntax-begin-class of false endof
        syntax-begin-implement of false endof
        true swap
      endcase
      no-implicit or to no-implicit
    loop
    no-implicit
  ;
  
  \ Test whether the only outer syntax is a word
  : outer-syntax-word? ( -- flag )
    dump-syntax dup 1 = if
      drop syntax-word =
    else
      0 ?do drop loop false
    then
  ;
    
  \ Start an implicit definition
  : start-implicit ( -- )
    ram-here to saved-ram-here
    ram-latest to saved-ram-latest
    compiling-to-flash? to saved-compiling-to-flash
    compile-to-ram
    s" " start-compile
    true state !
  ;
  
  \ Try to start an implicit definition
  : try-start-implicit ( -- )
    saved-ram-here 0= if outer-syntax? not if start-implicit then then
  ;

  \ End an implicit definition
  : end-implicit ( -- )
    postpone ;
    saved-ram-here >xt execute
    saved-ram-here ram-here!
    saved-ram-latest ram-latest!
    saved-compiling-to-flash if compile-to-flash then
    0 to saved-ram-here
    0 to saved-ram-latest
    false to saved-compiling-to-flash
  ;

  \ Try to end an implicit definition
  : try-end-implicit ( -- )
    saved-ram-here if outer-syntax-word? if end-implicit then then
  ;
  
  \ Saved word reset hook
  0 value saved-word-reset-hook
    
end-module> import

\ Initialize
: init ( -- )
  init
  word-reset-hook @ to saved-word-reset-hook
  [:
    saved-word-reset-hook ?execute
    saved-ram-here if
      saved-ram-here ram-here!
      saved-ram-latest ram-latest!
      saved-compiling-to-flash if compile-to-flash then
      0 to saved-ram-here
      0 to saved-ram-latest
      false to saved-compiling-to-flash
    then
  ;] word-reset-hook !
;

\ Begin a [: with implicit word creation
: [: ( -- ) [immediate] try-start-implicit postpone [: ;

\ Begin an IF with implicit word creation
: if ( -- if-block ) [immediate] try-start-implicit postpone if ;

\ Begin a BEGIN with implicit word creation
: begin ( -- begin-block ) [immediate] try-start-implicit postpone begin ;

\ Begin a DO with implicit word creation
: do ( -- do-block ) [immediate] try-start-implicit postpone do ;

\ Begin a ?DO with implicit word creation
: ?do ( -- do-block ) [immediate] try-start-implicit postpone ?do ;

\ Begin a CASE with implicit word creation
: case ( -- case-block ) [immediate] try-start-implicit postpone case ;

\ End a ;] with implicit word ending
: ;] ( -- ) [immediate] postpone ;] try-end-implicit ;

\ End a THEN with implicit word ending
: then ( if-block -- ) [immediate] postpone then try-end-implicit ;

\ End an UNTIL with implicit word ending
: until ( begin-block -- ) [immediate] postpone until try-end-implicit ;

\ End a REPEAT with implicit word ending
: repeat ( while-block -- ) [immediate] postpone repeat try-end-implicit ;

\ End an AGAIN with implicit word ending
: again ( begin-block -- ) [immediate] postpone again try-end-implicit ;

\ End a LOOP with implicit word ending
: loop ( do-block -- ) [immediate] postpone loop try-end-implicit ;

\ End a +LOOP with implicit word ending
: +loop ( do-block -- ) [immediate] postpone +loop try-end-implicit ;

\ End an ENDCASE with implicit word ending
: endcase ( case-block -- ) [immediate] postpone endcase try-end-implicit ;

\ End an ENDCASESTR with implicit word ending
: endcasestr ( case-block -- ) [immediate] postpone endcasestr try-end-implicit ;

implicit-internal unimport

reboot
