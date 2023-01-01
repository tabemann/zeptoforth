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

compile-to-flash

begin-module tinymt32

  begin-module tinymt32-internal

    127 constant tinymt32-mexp
    1 constant tinymt32-sh0
    10 constant tinymt32-sh1
    8 constant tinymt32-sh8
    $7FFFFFFF constant tinymt32-mask

    begin-structure tinymt32-size

      4 cells +field tinymt32-status
      field: tinymt32-mat1
      field: tinymt32-mat2
      field: tinymt32-tmat

    end-structure

    \ This function always returns 127
    : tinymt32-get-mexp ( random -- mexp ) drop tinymt32-mexp ;

    \ This function changes internal state of tinymt32.
    \ Users should not call this function directly.
    : tinymt32-next-state ( random -- )
      >r ( )
      3 cells r@ tinymt32-status + @ ( y )
      0 cells r@ tinymt32-status + @ tinymt32-mask and ( y s0-masked )
      1 cells r@ tinymt32-status + @ xor ( y s0^s1 )
      2 cells r@ tinymt32-status + @ xor ( y x )
      dup tinymt32-sh0 lshift xor ( y x )
      swap dup tinymt32-sh0 rshift 2 pick xor xor ( x y )
      1 cells r@ tinymt32-status + @ ( x y s1 )
      0 cells r@ tinymt32-status + ! ( x y )
      2 cells r@ tinymt32-status + @ ( x y s2 )
      1 cells r@ tinymt32-status + ! ( x y )
      tuck tinymt32-sh1 lshift xor 2 cells r@ tinymt32-status + ! ( y )
      dup 3 cells r@ tinymt32-status + ! ( y )
      dup 1 and negate r@ tinymt32-mat1 @ and ( y a )
      swap 1 and negate r@ tinymt32-mat2 @ and ( a b )
      2 cells r@ tinymt32-status + @ ( a b s2 )
      xor 2 cells r@ tinymt32-status + ! ( a )
      1 cells r@ tinymt32-status + @ ( a s1 )
      xor 1 cells r> tinymt32-status + ! ( )
    ;

    \ This function outputs 32-bit unsigned integer from internal state.
    \ Users should not call tihs function directly.
    : tinymt32-temper ( random -- x )
      >r
      3 cells r@ tinymt32-status + @ ( t0 )
      0 cells r@ tinymt32-status + @ ( t0 s0 )
      2 cells r@ tinymt32-status + @ tinymt32-sh8 rshift + ( t0 t1 )
      tuck xor ( t1 t0 )
      swap 1 and if ( t0 )
	r> tinymt32-tmat @ xor ( t0 )
      else
	rdrop ( t0 )
      then
    ;

    8 constant min-loop
    8 constant pre-loop

    \ This function certificate the period of 2^127-1
    : period-certification ( random -- )
      >r ( )
      0 cells r@ tinymt32-status + @ tinymt32-mask and 0=
      1 cells r@ tinymt32-status + @ 0= and
      2 cells r@ tinymt32-status + @ 0= and
      3 cells r@ tinymt32-status + @ 0= and if
	[char] T 0 cells r@ tinymt32-status + !
	[char] I 1 cells r@ tinymt32-status + !
	[char] N 2 cells r@ tinymt32-status + !
	[char] Y 3 cells r> tinymt32-status + !
      else
	rdrop
      then
    ;

  end-module> import

  ' tinymt32-size export tinymt32-size
  
  \ This function initializes the internal state array with a 32-bit unsigned
  \ integer seed.
  : tinymt32-init ( seed random -- )
    >r ( seed )
    0 cells r@ tinymt32-status + ! ( )
    r@ tinymt32-mat1 @ 1 cells r@ tinymt32-status + ! ( )
    r@ tinymt32-mat2 @ 2 cells r@ tinymt32-status + ! ( )
    r@ tinymt32-tmat @ 3 cells r@ tinymt32-status + ! ( )
    1 begin dup min-loop < while ( i )
      dup 3 and cells r@ tinymt32-status + @ ( i x )
      1812433253 ( i x constant )
      2 pick 1- 3 and cells r@ tinymt32-status + @ ( i x constant y )
      dup 30 rshift xor * 2 pick + xor ( i z )
      over 3 and cells r@ tinymt32-status + ! ( i )
    1+ repeat drop ( )
    r@ period-certification ( )
    0 begin dup pre-loop < while r@ tinymt32-next-state 1+ repeat drop ( )
    rdrop ( )
  ;

  \ Prepare the internal state's mat1, mat2, and tmat values
  : tinymt32-prepare ( mat1 mat2 tmat random -- )
    >r ( mat1 mat2 tmat )
    r@ tinymt32-tmat ! ( mat1 mat2 )
    r@ tinymt32-mat2 ! ( mat1 )
    r> tinymt32-mat1 ! ( )
  ;

  \ Example internal state values
  : tinymt32-prepare-example ( random -- )
    >r $8F7011EE $FC78FF1F $3793FDFF r> tinymt32-prepare
  ;
    
  \ This function outputs 32-bit unsigned integer from internal state.
  : tinymt32-generate-uint32 ( random -- x )
    dup tinymt32-next-state tinymt32-temper
  ;

end-module

reboot