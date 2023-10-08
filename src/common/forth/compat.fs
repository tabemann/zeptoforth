\ Copyright (c) 2023 Travis Bemann
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

begin-module compat

  begin-module compat-internal

    \ Temporary buffer for WORD
    256 constant word-buffer-size
    word-buffer-size buffer: word-buffer
    
  end-module> import

  \ Parse a word delimited by a given character; note that this is not
  \ reentrant because the returned counted string is stored in a single global
  \ buffer; for new code TOKEN / PARSE-NAME is recommended when possible. Also,
  \ this word does not properly handle all sorts of whitespace, such as tabs
  \ and values less than $20.
  : word ( delim "<delims>word<delims>" -- c-addr ) { delim }
    source { c-addr u }
    begin
      >in @ u < if
        c-addr >in @ + c@ delim = if
          1 >in +! false
        else
          true
        then
      else
        true
      then
    until
    delim internal::parse-to-char { c-addr' u' }
    u' word-buffer c!
    c-addr' word-buffer 1+ u' move
    word-buffer
  ;

  \ Find a word's xt and whether it is immediate (signaled by 1) or
  \ non-immediate (signaled by 0)
  : find ( c-addr -- c-addr 0 | xt 1 | xt -1 )
    dup count find { counted word }
    word if
      word >xt word internal::word-flags h@ immediate-flag and if 1 else -1 then
    else
      counted 0
    then
  ;

  \ Implement the traditional Forth string copying word CMOVE - for new code
  \ using MOVE is recommended.
  : cmove ( c-addr1 c-addr2 u -- ) internal::move> ;

  \ Implement the traditional Forth string copying word CMOVE> - for new code
  \ using MOVE is recommended.
  : cmove> ( c-add1 c-addr2 u -- ) internal::<move ;

  \ Determine whether a value is between 'low', inclusive, and 'high',
  \ exclusive.
  : within ( test low high -- flag ) over - >r - r> u< ;

  \ Parse a number in a string 'c-addr u' with an accumulator initialized as a
  \ double-cell value 'acc' using the base stored in BASE
  : >number { D: acc c-addr u -- acc' c-addr' u' }
    begin u while
      c-addr c@ to-upper-char { c }
      c [char] 0 >= c [char] 9 <= and if
        c [char] 0 - base @ < if
          acc base @ s>d d* c [char] 0 - s>d d+ to acc
          1 +to c-addr
          -1 +to u
        else
          acc c-addr u exit
        then
      else
        c [char] A >= c [char] Z <= and if
          c [char] A - base @ 10 - < if
            acc base @ s>d d* c [ char A 10 - ] literal - s>d d+ to acc
            1 +to c-addr
            -1 +to u
          else
            acc c-addr u exit
          then
        else
          acc c-addr u exit
        then
      then
    repeat
    acc c-addr u
  ;

  \ Compare two strings for both content and length using the numeric values of
  \ bytes compared within and shorter common length.
  : compare { c-addr1 u1 c-addr2 u2 -- n }
    begin u1 0<> u2 0<> and while
      c-addr1 c@ c-addr2 c@ { c1 c2 }
      c1 c2 < if
        -1 exit
      else
        c1 c2 > if
          1 exit
        then
      then
      1 +to c-addr1
      -1 +to u1
      1 +to c-addr2
      -1 +to u2
    repeat
    u1 u2 u< if
      -1
    else
      u1 u2 u> if
        1
      else
        0
      then
    then
  ;

  \ Fill a buffer with zero bytes.
  : erase ( c-addr u -- ) 0 fill ;

  \ Parse a single token from the input.
  : parse-name ( "token" -- c-addr u ) token ;

  \ Output a right-justified unsigned value in a specified field width; note
  \ that if the value is wider than the specified field width the whole value
  \ will be output but no padding spaces will be added.
  : u.r { u width -- }
    u 0 { current bytes }
    begin current while
      1 +to bytes
      current base @ u/ to current
    repeat
    width bytes u> if width bytes - spaces then
    u u.
  ;

  \ Add multiple characters to <# # #> numeric formatting.
  : holds ( c-addr u -- )
    begin dup while 1- 2dup + c@ hold repeat 2drop
  ;

  \ Transfer N items and count to the return stack.
  : n>r ( xn .. x1 N -- ; R: -- x1 .. xn n )
    dup \ xn .. x1 N N -- 
    begin
      dup
    while
      rot r> swap >r >r \ xn .. N N -- ; R: .. x1 --
      1- \ xn .. N 'N -- ; R: .. x1 --
    repeat
    drop \ N -- ; R. x1 .. xn --
    r> swap >r >r
  ;

  \ Pull N items and count off the return stack.
  : nr> ( -- xn .. x1 N ; R: x1 .. xn N -- )
   r> r> swap >r dup
   begin
      dup
   while
      r> r> swap >r -rot
      1-
   repeat
   drop
 ;

 \ Raise an exception that displays a message and a following newline if the
 \ value on the stack at runtime is non-zero.
 : abort" ( "message" -- ) ( Runtime: flag -- )
   [immediate]
   postpone if
   postpone [:
   postpone ."
   postpone cr
   postpone ;]
   postpone ?raise
   postpone then
 ;
  
end-module

compile-to-ram
