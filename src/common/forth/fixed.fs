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

\ Compile this to flash
compile-to-flash

\ Get the value of pi
0 314159265 0 100000000 f/ 2constant pi

\ Duplicate four cells
: 4dup ( d1 d2 -- d1 d2 d1 d2 ) 3 pick 3 pick 3 pick 3 pick ;

\ Rotate two cells
: 2rot ( d1 d2 d3 -- d2 d3 d1 ) 5 roll 5 roll ;

\ Get the absolute value of a double-cell number
: dabs ( nd -- ud ) dup 31 arshift 0<> if dnegate then ;

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

\ Calculate whether a square root is close enough
: sqrt-close-enough ( f1 f2 -- flag )
  4dup d- 2rot dabs 2rot dabs dmax f/ dabs 2 0 d<
;

\ Calculate a better square root guess
: sqrt-better-guess ( f1 f2 -- f3 ) 2dup 2rot 2rot f/ d+ 2 0 d/ ;

\ The main loop of calculating a square root
: sqrt-test ( f1 f2 -- f3 )
  begin
    4dup f/ 2over sqrt-close-enough if
      2nip true
    else
      4dup sqrt-better-guess 2nip false
    then
  until
;

\ Calculate a square root
: sqrt ( f1 -- f2 ) 2dup 2 0 d/ sqrt-test ;

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

\ Calculate ln(x)
: ln ( f1 -- f2 ) 0 1 d- lnp1 ;

\ Calculate sin(x)
: sin ( f1 -- f2 )
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
: cos ( f1 -- f2 ) pi 2 0 d/ 2swap d- sin ;

\ Calculate tan(x)
: tan ( f1 -- f2 ) 2dup sin 2swap cos f/ ;

\ Calculate atan(x)
: atan ( f1 -- f2 )
  0 1 1 40 do
    2over i 0 d* 2 fi** 2swap f/ i 2 * 1 - 0 swap d+
  -1 +loop
  f/
;

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

\ Calculate acos(x)
: acos ( f1 -- f2 ) asin dnegate pi 2 0 d/ d+ ;

\ Domain error exception
: x-domain-error ( -- ) space ." domain error" cr ;

\ Calculate a fixed point power b^x
: f** ( fb fx -- fb^x )
  2dup d0= 2 pick 2 pick d0= and triggers x-domain-error
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

\ Calculate tanh(x)
: tanh ( f1 -- f2 ) 2dup sinh 2swap cosh f/ ;

\ Calculate asinh(x)
: asinh ( f1 -- f2 ) 2dup 2 fi** 0 1 d+ sqrt d+ ln ;

\ Calculate acosh(x)
: acosh ( f1 -- f2 ) 2dup 2 fi** 0 1 d- sqrt d+ ln ;

\ Calculate atanh(x)
: atanh ( f1 -- f2 ) 2dup 0 1 d+ 2swap dnegate 0 1 d+ f/ ln 2 0 d/ ;

\ Get the number of instances of a character in a string
: char-count ( b-addr bytes b -- count )
  >r
  0 begin over 0<> while
    rot dup b@ r@ = if
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
  >r over b@ [char] . <> if
    0 0 begin 2 pick 0<> while
      3 pick b@ r@ parse-digit if
	rot rot r@ 0 ud* rot 0 d+
      else
	drop 3 pick b@ [char] . <> if
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

\ Handle the portion of a s31.32 fixed-point double-cell numeric literal to the
\ right of the decimal point
: handle-fraction ( b-addr bytes d base -- d -1 | 0 )
  >r 0 1 begin 4 pick 0<> while
    5 pick b@ r@ parse-digit if
      0 2swap r@ 0 ud/ 2dup >r >r ud* d+
      2swap 1- swap 1+ swap 2swap r> r>
    else
      drop 2drop 2drop 2drop rdrop false exit
    then
  repeat
  2drop 2swap 2drop rdrop true
;

\ Handle s31.32 fixed-point double-cell numeric literals
: handle-fixed ( b-addr bytes base -- flag )
  >r over b@ [char] , <> if
    2dup + 1- b@ [char] , <> if
      0 begin over 0<> while
	2 pick b@ r@ parse-digit if
	  swap r@ * + rot 1+ rot 1- rot
	else
	  drop 2 pick b@ [char] , = if
	    rot 1+ rot 1- rot 0 swap r> handle-fraction exit
	  else
	    drop 2drop rdrop false exit
	  then
	then
      repeat
      nip nip rdrop true
    else
      rdrop 2drop false
    then
  else
    rdrop 2drop false
  then
;

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
      over b@ [char] - = if
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

\ Initialize
: init ( -- )
  init
  ['] do-handle-number handle-number-hook !
;

\ Warm reboot
warm
