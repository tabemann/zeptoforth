\ Copyright (c) 2020-2024 Travis Bemann
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

\ Compile this to flash
compile-to-flash

compress-flash

internal import

\ Get the value of pi
0 314159265 0 100000000 f/ 2constant pi

\ Domain error exception
: x-domain-error ( -- ) ." domain error" cr ;

\ Duplicate four cells
: 4dup ( d1 d2 -- d1 d2 d1 d2 ) 3 pick 3 pick 3 pick 3 pick ;

\ Rotate two cells
: 2rot ( d1 d2 d3 -- d2 d3 d1 ) 5 roll 5 roll ;

\ Get the absolute value of a double-cell number
: dabs ( nd -- ud ) dup 31 arshift 0<> if dnegate then ;

\ Get the ceiling of a fixed-point number as a single-cell number
: ceil ( f -- n ) swap if 1+ then ;

\ Get the floor of a fixed-point number as a single-cell number
: floor ( f -- n ) nip ;

\ Round a fixed-point number up to the nearest integer with half rounding up
: round-half-up ( f -- n ) swap 31 rshift if 1+ then ;

\ Round a fixed-point number down to the nearest integer with half rounding down
: round-half-down ( f -- n )
  swap dup [ 31 bit ] literal <> if 31 rshift if 1+ then else drop then
;

commit-flash

\ Round a fixed-point number to the nearest integer with half rounding towards
\ zero
: round-half-zero ( f -- n )
  dup 0>= if round-half-down else round-half-up then
;

\ Round a fixed-point number to the nearest integer with half rounding away
\ from zero
: round-half-away-zero ( f -- n )
  dup 0>= if round-half-up else round-half-down then
;

\ Round a fixed-point number to the nearest integer with half rounding towards
\ even
: round-half-even ( f -- n )
  dup 1 and if round-half-up else round-half-down then
;

\ Round a fixed-point number to the nearest integer with half rounding towards
\ even
: round-half-odd ( f -- n )
  dup 1 and if round-half-down else round-half-up then
;

\ Round a fixed-point number towards zero
: round-zero ( f -- n )
  dup 0>= if nip else swap if 1+ then then
;

\ Round a fixed-point number away from zero
: round-away-zero ( f -- n )
  swap if dup 0>= if 1+ then then
;

commit-flash

\ Get the minimum of two double-cell numbers
: dmin ( nd nd -- nd ) 4dup d< if 2drop else 2swap 2drop then ;

\ Get the maximum of two double-cell numbers
: dmax ( nd nd -- nd ) 4dup d< if 2swap 2drop else 2drop then ;

\ Signed double-cell multiplication
: d* ( nd1 nd2 -- nd3 )
  2dup d0< if
    dnegate 2swap 2dup d0< if
      dnegate ud*
    else
      ud* dnegate
    then
  else
    2swap 2dup d0< if
      dnegate ud* dnegate
    else
      ud*
    then
  then
;

\ Get two cells from the return stack without changing it
: 2r@ ( -- d ) ( R: d -- d )
  [immediate] postpone r> postpone r> postpone 2dup postpone >r postpone >r
;

\ Exponentiation of a fixed point number by an unsigned integer
: fi** ( f1 u -- f2 )
  dup 0> if
    0 1 begin
      2 pick 1 and if
	4 pick 4 pick f*
      then
      rot 1 rshift dup 0= if
	drop 2nip true
      else
	4 roll 4 roll 2dup f* rot 4 roll 4 roll false
      then
    until
  else
    0= if
      2drop 0 1
    else
      2drop 0 0
    then
  then
;

\ Compute the symmetric modulus of two S31.32 fixed point numbers
: fmod { D: a D: b -- D: c }
  a a b f/ f>s dup 0< if 1+ then s>f b f* d-
;
  
continue-module internal
  
  \ Calculate whether a square root is close enough
  : sqrt-close-enough ( f1 f2 -- flag )
    4dup d- 2rot dabs 2rot dabs dmax f/ dabs 2 0 d<
  ;
  
  \ Calculate a better square root guess
  : sqrt-better-guess ( f1 f2 -- f3 ) 2dup 2rot 2rot f/ d+ 0 2 f/ ;

  commit-flash
  
end-module

\ Calculate a square root
: sqrt ( f1 -- f2 )
  64 >r
  2dup 0 2 f/
  begin
    r@ 0> if
      4dup f/ 2over sqrt-close-enough if
        rdrop 2nip true
      else
        r> 1- >r 4dup sqrt-better-guess 2nip false
      then
    else
      rdrop 2nip true
    then
  until
;

\ Calculate a factorial
: factorial ( u -- ud ) 1 0 rot 1 + 1 ?do i 0 ud* loop ;

\ Calculate (e^x)-1
: expm1 ( f1 -- f2 )
  >r >r 0 0 0 1 1 0 begin
    2swap 2r@ f* 2over d/ 2dup dabs 2 0 d< >r 2rot 2over d+
    2swap 2rot 1 0 d+ r>
  until
  rdrop rdrop 2drop 2drop
;

commit-flash

\ Execute e^x
: exp ( f1 -- f2 ) expm1 0 1 d+ ;

\ Calculate ln(x + 1)
: lnp1 ( f1 -- f2 )
  0 1 d+ >r >r 0 0 begin
    2dup exp \ y exp(y)
    2dup 2r@ 2swap d- \ y exp(y) x-exp(y)
    2r@ 2rot d+ \ y x-exp(y) x+exp(y)
    f/ \ y (x-exp(y))/(x+exp(y))
    2 0 d* \ y 2*(x-exp(y))/(x+exp(y))
    2over d+ \ y y+2*(x-exp(y))/(x+exp(y))
    2dup 2rot d- \ y+2*(x-exp(y))/(x+exp(y)) 2*(x-exp(y))/(x+exp(y))
    dabs 2 0 d<
  until
  rdrop rdrop
;

commit-flash

\ Calculate ln(x)
: ln ( f1 -- f2 ) 0 1 d- lnp1 ;

\ Calculate sin(x)
: sin ( f1 -- f2 )
  [ pi 0 2 f* swap ] literal literal fmod
  2dup 2dup >r >r 1 0 begin
    2swap 2r@ f* 2r@ f*
    2over 2 0 d* d/
    2over 2 0 d*
    1 0 d+ d/
    2dup dabs 2 0 d< >r
    2rot
    2over
    7 pick 1 and if d- else d+ then
    2swap
    2rot
    1 0 d+
    r>
  until
  rdrop rdrop 2drop 2drop
;

\ Calculate cos(x)
: cos ( f1 -- f2 ) pi 0 2 f/ 2swap d- sin ;

commit-flash

\ Calculate tan(x)
: tan ( f1 -- f2 ) 2dup sin 2swap cos f/ ;

\ Calculate atan(x)
: atan ( f1 -- f2 )
  0 1 1 40 do
    2over i 0 d* 2 fi** 2swap f/ i 2 * 1 - 0 swap d+
  -1 +loop
  f/
;

commit-flash

\ Calculate a angle for any pair of x and y coordinates
: atan2 ( fy fx -- fangle )
  2dup d0> if
    f/ atan
  else
    4dup d0< rot rot d0>= and if
      f/ atan pi d+
    else
      4dup d0< rot rot d0< and if
	f/ atan pi d-
      else
	4dup d0= rot rot d0> and if
	  2drop pi 2 0 d/
	else
	  4dup d0= rot rot d0< and if
	    2drop pi 2 0 dnegate d/
	  else
	    2drop 0 0
	  then
	then
      then
    then
  then
;

\ Calculate asin(x)
: asin ( f1 -- f2 )
  2dup 2 fi** 0 1 d< if
    0 1 2over 2 fi** d- sqrt f/ atan
  else
    2dup d0> if
      2drop pi 2 0 d/
    else
      2drop pi 2 0 negate d/
    then
  then
;

commit-flash

\ Calculate acos(x)
: acos ( f1 -- f2 ) asin dnegate pi 2 0 d/ d+ ;

\ Calculate a fixed point power b^x
: f** ( fb fx -- fb^x )
  2over d0= >r 2dup d0= r> and triggers x-domain-error
  over 0= if
    dup 0>= if
      nip fi**
    else
      nip negate fi** 0 1 2swap f/
    then
  else
    2over d0>= averts x-domain-error 2swap ln f* exp
  then
;

\ Calculate sinh(x)
: sinh ( f1 -- f2 ) expm1 2dup 2dup 0 1 d+ f/ d+ 2 0 d/ ;

\ Calculate cosh(x)
: cosh ( f1 -- f2 ) expm1 2dup 2dup 0 1 d+ f/ d- 2 0 d/ 0 1 d+ ;

commit-flash

\ Calculate tanh(x)
: tanh ( f1 -- f2 ) 2dup sinh 2swap cosh f/ ;

\ Calculate asinh(x)
: asinh ( f1 -- f2 ) 2dup 2 fi** 0 1 d+ sqrt d+ ln ;

\ Calculate acosh(x)
: acosh ( f1 -- f2 ) 2dup 2 fi** 0 1 d- sqrt d+ ln ;

\ Calculate atanh(x)
: atanh ( f1 -- f2 ) 2dup 0 1 d+ 2swap dnegate 0 1 d+ f/ ln 2 0 d/ ;

continue-module internal
  
  \ Get the number of instances of a character in a string
  : char-count ( b-addr bytes b -- count )
    >r
    0 begin over 0<> while
      rot dup c@ r@ = if
	1+ -rot 1+
      else
	1+ -rot
      then
      swap 1- swap
    repeat
    nip nip rdrop
  ;
  
  \ Handle double-cell integer numeric literals
  : handle-double ( b-addr bytes base -- flag )
    >r over c@ [char] . <> if
      0 0 begin 2 pick 0<> while
	3 pick c@ r@ parse-digit if
	  rot rot r@ 0 ud* rot 0 d+
	else
	  drop 3 pick c@ dup [char] _ <> swap [char] . <> and if
	    2drop 2drop rdrop false exit
	  then
	then
	2swap 1- swap 1+ swap 2swap
      repeat
      2swap 2drop rdrop true
    else
      rdrop 2drop false
    then
  ;

  \ Determine the maximum number of fraction characters for a base
  : get-max-fraction-chars ( base -- chars )
    >r 1 0 0 begin
      -rot r@ 0 ud* 2dup 0 1 d> if
	2drop rdrop true
      else
	rot 1+ false
      then
    until
  ;

  commit-flash

  \ Build max fraction characters table
  : build-max-fraction-chars ( -- )
    37 2 ?do
      i get-max-fraction-chars ,
    loop
  ;

  commit-flash

  \ Create a lookup table of maximum number of fraction characters
  create max-fraction-chars build-max-fraction-chars

  commit-flash
  
  \ Handle the portion of a s31.32 fixed-point double-cell numeric literal to
  \ the right of the decimal point
  : handle-fraction { c-addr bytes D: d base -- d -1 | 0 }
    bytes max-fraction-chars base 2 - cells + @ min { max-places }
    0 0 { places accum }
    begin bytes 0> places max-places < and while
      c-addr c@ dup [char] _ <> if
        base parse-digit if
          accum base * swap + to accum
          1 +to places
        else
          drop 0 exit
        then
      else
        drop
      then
      1 +to c-addr
      -1 +to bytes
    repeat
    0 accum 0 base places fi** f/ d d+ true
  ;

  commit-flash

  \ Handle s31.32 fixed-point double-cell numeric literals
  : handle-fixed ( b-addr bytes base -- flag )
    >r over c@ dup [char] . <> swap [char] , <> and if
      2dup + 1- c@ dup [char] . <> swap [char] , <> and if
	0 begin over 0<> while
	  2 pick c@ r@ parse-digit if
	    swap r@ * + rot 1+ rot 1- rot
	  else
            drop
            2 pick c@ dup [char] _ = if
              drop rot 1+ rot 1- rot
            else
              dup [char] . = swap [char] , = or if
                rot 1+ rot 1- rot 0 swap r> handle-fraction exit
              else
                drop 2drop rdrop false exit
              then
            then
	  then
	repeat
	nip nip 0 swap rdrop true
      else
	rdrop 2drop false
      then
    else
      rdrop 2drop false
    then
  ;

  commit-flash

  \ Handle an unsigned double-cell number
  : handle-unsigned-double ( b-addr bytes base -- d -1 | 0 )
    >r 2dup [char] . char-count 1 = if
      r> handle-double
    else
      2dup [char] , char-count 1 = if
	r> handle-fixed
      else
	rdrop 2drop false
      then
    then
  ;

  commit-flash

  \ Handle numeric literals
  : do-handle-number ( b-addr bytes -- flag )
    2dup do-handle-number if
      state @ not if
        rot rot
      then
      2drop true
    else
      parse-base >r
      dup 0<> if
        over c@ [char] - = if
          1- swap 1+ swap
          dup 0<> if
            r> handle-unsigned-double
          else
            rdrop 2drop false
          then
          dup if
            state @ if
              rot rot dnegate swap lit, lit,
            else
              rot rot dnegate rot
            then
          then
        else
          r> handle-unsigned-double
          dup if
            state @ if
              rot lit, swap lit,
            then
          then
        then
      else
        rdrop 2drop false
      then
    then
  ;

end-module

\ Parse an unsigned double-cell integer
: parse-double-unsigned ( c-addr bytes -- ud success? )
  2dup [char] . char-count 2 u< if
    parse-base >r
    dup 0<> if
      r> handle-double if true else 0 0 false then
    else
      rdrop 2drop 0 0 false
    then
  else
    2drop 0 0 false
  then
;

\ Parse a signed double-cell integer
: parse-double ( c-addr bytes -- nd success? )
  2dup [char] . char-count 2 u< if
    parse-base >r
    dup 0<> if
      over c@ [char] - = if
        1- swap 1+ swap
        dup 0<> if r> handle-double else rdrop 2drop false then
        if dnegate true else 0 0 false then
      else
        r> handle-double if true else 0 0 false then
      then
    else
      rdrop 2drop 0 0 false
    then
  else
    2drop 0 0 false
  then
;

\ Parse a fixed-point number
: parse-fixed ( c-addr bytes -- f success? )
  2dup [char] , char-count >r 2dup [char] . char-count r> + 2 u< if
    parse-base >r
    dup 0<> if
      over c@ [char] - = if
        1- swap 1+ swap
        dup 0<> if r> handle-fixed else rdrop 2drop false then
        if dnegate true else 0 0 false then
      else
        r> handle-fixed if true else 0 0 false then
      then
    else
      rdrop 2drop 0 0 false
    then
  else
    2drop 0 0 false
  then
;

commit-flash

\ Initialize
: init ( -- )
  init
  ['] do-handle-number handle-number-hook !
;

end-compress-flash

\ Reboot
reboot
