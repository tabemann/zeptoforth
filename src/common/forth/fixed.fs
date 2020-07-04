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

\ Compile this to RAM
compile-to-ram

\ Get the value of pi
0 314159265 0 100000000 f/ constant pi

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
