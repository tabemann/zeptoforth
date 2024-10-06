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

begin-module float32

  armv7m-fp import

  begin-module float32-internal

    \ CPACR register address
    $E000ED88 constant CPACR
    
  end-module> import
  
  \ Enable floating point
  : enable-float ( -- )
    [ %1111 10 2 * lshift ] literal CPACR bis!
  ;

  initializer enable-float
  
  \ Get the absolute value of a single-precision floating-point value
  : vabs ( f -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s0 s0 vabs.f32_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Negate a single-precision floating-point value
  : vnegate ( f -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s0 s0 vneg.f32_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Get the square root of a single-precision floating-point value
  : vsqrt ( f -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s0 s0 vsqrt.f32_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Add two single-precision floating-point values
  : v+ ( f1 f0 -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    s0 s1 s0 vadd.f32_,_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Subtract two single-precision floating-point values
  : v- ( f1 f0 -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    s0 s1 s0 vsub.f32_,_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Multiply two single-precision floating-point values
  : v* ( f1 f0 -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    s0 s1 s0 vmul.f32_,_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Divide two single-precision floating-point values
  : v/ ( f1 f0 -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    s0 s1 s0 vdiv.f32_,_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Get the minimum of two single-precision floating-point values
  : vmin ( f1 f0 -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    s0 s1 s0 vminnm.f32_,_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Get the maximum of two single-precision floating-point values
  : vmax ( f1 f0 -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    s0 s1 s0 vmaxnm.f32_,_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Get whether a single-precision floating-point value is less
  : v< ( f1 f0 -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    0 r6 movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    ge bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is less or equal
  : v<= ( f1 f0 -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    0 r6 movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    gt bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is equal
  : v= ( f1 f0 -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    0 r6 movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    ne bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is not equal
  : v<> ( f1 f0 -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    0 r6 movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    eq bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is greater
  : v> ( f1 f0 -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    0 r6 movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    le bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is greater or equal
  : v>= ( f1 f0 -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    s1 s1 r7 vldmia!.f32_,{_..._}
    0 r6 movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    lt bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is less than zero
  : v0< ( f -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    0 r6 movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    ge bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is less than or equal to
  \ zero
  : v0<= ( f -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    0 r6 movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    gt bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is equal to zero
  : v0= ( f -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    0 r6 movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    ne bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is not equal to zero
  : v0<> ( f -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    0 r6 movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    eq bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is greater than zero
  : v0> ( f -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    0 r6 movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    le bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is greater than or
  \ equal to zero
  : v0>= ( f -- flag )
    code[
    r6 s0 vmov.f32.cr_,_
    0 r6 movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    lt bc>
    r6 r6 mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get the non-fractional portion of a single-precision floating-point value
  : vnfract ( f -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s0 s0 vrintn.f32_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;
  
  \ Get the fractional portion of a single-precision floating-point value
  : vfract ( f -- f' )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    s0 s1 vrintn.f32_,_
    s1 s0 s0 vsub.f32_,_,_
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Convert an S15.16 fixed-point value to a single-precision floating-point
  \ value
  : f32>v ( f32 -- f )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    16 s0 vcvt.f32.s32_,#<fbits>
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Convert a single-precision floating-point value to an S15.16 fixed-point
  \ value
  : v>f32 ( f -- f32 )
    [inlined]
    code[
    r6 s0 vmov.f32.cr_,_
    16 s0 vcvt.s32.f32_,#<fbits>
    s0 r6 vmov.cr.f32_,_
    ]code
  ;

  \ Convert an S31.32 fixed-point value to a single-precision floating-point
  \ value
  : f64>v { D: f64 -- f }
    f64 d0= if 0 exit then
    false 127 0 { sign exponent fraction }
    f64 d0< if
      f64 dnegate to f64
      true to sign
    then
    f64 nip 0= if
      f64 drop { bits }
      0 { zero-bits }
      begin bits $8000_0000 and 0= while
        bits 1 lshift to bits
        1 +to zero-bits
      repeat
      bits 1 lshift [ 32 23 - ] literal rshift to fraction
      126 zero-bits - to exponent
    else
      f64 { D: bits }
      0 { nzero-bits }
      begin bits nip $FFFF_FFFE and while
        bits 1 2rshift to bits
        1 +to nzero-bits
      repeat
      bits drop [ 32 23 - ] literal rshift to fraction
      127 nzero-bits + to exponent
    then
    sign 1 and 31 lshift exponent 23 lshift or fraction or
  ;

  \ NaN exception
  : x-nan ( -- ) ." attempted to convert NaN to S31.32 fixed point" cr ;

  \ Convert a single-precision floating-point value to an S31.32 fixed-point
  \ value
  : v>f64 { f -- f64 }
    f 31 rshift 0<> { sign }
    f 23 rshift $FF and { exponent }
    f $007F_FFFF and { fraction }
    exponent $FF = if
      fraction 0= averts x-nan
      sign if $FFFF_FFFF_FFFF_FFFF. else $7FFF_FFFF_FFFF_FFFF. then exit
    then
    \ We can handle subnormal numbers like zeroes because they have less
    \ significance than what we can represent anyways
    exponent [ 127 32 - ] literal < if 0,0 exit then
    exponent [ 127 30 + ] literal > if
      sign if $FFFF_FFFF_FFFF_FFFF. else $7FFF_FFFF_FFFF_FFFF. then exit
    then
    $0080_0000 fraction or s>d
    exponent [ 127 9 - ] literal < if
      [ 127 9 - ] literal exponent - 2rshift
    else
      exponent [ 127 9 - ] literal - 2lshift
    then
    sign if dnegate then
  ;
  
end-module
