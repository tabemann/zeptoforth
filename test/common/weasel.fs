\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module weasel

  rng import

  \ String maximum length
  50 constant max-length

  \ Reproduction count
  100 constant reproduce-count

  \ Mutation probability numerator
  5 constant mutate-numerator

  \ Mutation probability denominator
  100 constant mutate-denominator

  \ Target length
  variable target-length
  
  \ Target buffer
  max-length buffer: target-buffer

  \ Current buffer
  max-length buffer: current-buffer
  
  \ Reproduction buffer
  max-length reproduce-count * buffer: reproduce-buffer

  \ Convert a character to be all uppercase or spaces
  : massage-char ( c -- c )
    dup [char] A >= over [char] Z <= and not if
      dup [char] a >= over [char] z <= and if
	[char] a - [char] A +
      else
	drop $20
      then
    then
  ;
  
  \ Convert the target string to be all uppercase or spaces
  : massage-target ( -- )
    target-length @ 0 ?do
      target-buffer i + c@ massage-char target-buffer i + c!
    loop
  ;

  \ Prepare the target
  : prepare-target ( c-addr u -- )
    max-length min
    dup target-length !
    target-buffer swap move
    massage-target
  ;
  
  \ Generate a random character
  : random-char ( -- c )
    random 27 umod dup 26 < if
      [char] A +
    else
      drop $20
    then
  ;

  \ Prepare the current buffer
  : prepare-current ( -- )
    target-length @ 0 ?do
      random-char current-buffer i + c!
    loop
  ;

  \ Mutate a charcter
  : mutate ( c -- c )
    random mutate-denominator umod mutate-numerator < if
      drop random-char
    then
  ;

  \ Reproduce
  : reproduce ( -- )
    reproduce-count 0 ?do
      target-length @ 0 ?do
	current-buffer i + c@ mutate reproduce-buffer max-length j * + i + c!
      loop
    loop
  ;

  \ Score the current buffer
  : score-current ( -- score )
    0 target-length @ 0 ?do
      current-buffer i + c@ target-buffer i + c@ = if
	1+
      then
    loop
  ;

  \ Score a buffer
  : score ( i -- score )
    0 target-length @ 0 ?do
      over max-length * reproduce-buffer + i + c@
      target-buffer i + c@ = if
	1+
      then
    loop
    nip
  ;

  \ Find the buffer with the highest score
  : high-score ( -- i score )
    0 0 score reproduce-count 0 ?do
      i score 2dup < if
	nip nip i swap
      else
	drop
      then
    loop
  ;

  \ Weasel
  : run-weasel ( c-addr u -- )
    prepare-target
    prepare-current
    score-current cr 0 . . current-buffer target-length @ type
    0 begin
      1+ cr dup .
      reproduce
      high-score
      dup .
      over -1 <> if
	swap max-length * reproduce-buffer + current-buffer target-length @
	move
      else
	drop
      then
      current-buffer target-length @ type
      target-length @ = key? or
    until
    drop
  ;
  
end-module