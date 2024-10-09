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
    
    \ The sign mask
    $8000_0000 constant sign-mask

    \ The exponent mask
    $7F80_0000 constant exponent-mask
    
    \ The significand mask
    $007F_FFFF constant significand-mask

    \ The hidden bit
    $0080_0000 constant hidden-bit
    
    \ The special exponent
    128 constant special-exponent

    \ The maximum exponent
    128 constant max-exponent
    
    \ The denormal exponent
    -127 constant denormal-exponent

    \ The exponent bias
    127 constant exponent-bias

    \ The physical significand size
    23 constant physical-significand-size
    
    \ Get the sign of a single-precision floating-point value
    : v>sign ( f -- sign ) sign-mask and 0<> ;
    
    \ Get the exponent of a single-precision floating-point value
    : v>exponent ( f -- exponent )
      exponent-mask and physical-significand-size rshift exponent-bias -
    ;

    \ Is a single-precision floating-point value denormal?
    : v>denormal? ( f -- denormal? ) v>exponent denormal-exponent = ;

    \ Is a single-precision floating-point value special?
    : v>special? ( f -- special? ) v>exponent special-exponent = ;

    \ Get the significand of a single-precision floating-point value, shifted
    \ to left so the hidden bit of a normal number lies at bit 31
    : v>significand ( f -- significand )
      dup v>denormal? not if hidden-bit else 0 then swap significand-mask and +
      [ 31 physical-significand-size - ] literal lshift
    ;

    \ Is a single-precision floating-point value a NaN?
    : v>nan? ( f -- nan? ) dup v>special? swap significand-mask and 0<> and ;

    \ Is a single-precision floating-point value infinite?
    : v>infinite? ( f -- infinite? )
      dup v>special? swap significand-mask and 0= and
    ;
    
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
    false 127 0 { sign exponent significand }
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
      bits 1 lshift [ 32 23 - ] literal rshift to significand
      126 zero-bits - to exponent
    else
      f64 { D: bits }
      0 { nzero-bits }
      begin bits nip $FFFF_FFFE and while
        bits 1 2rshift to bits
        1 +to nzero-bits
      repeat
      bits drop [ 32 23 - ] literal rshift to significand
      127 nzero-bits + to exponent
    then
    sign 1 and 31 lshift exponent 23 lshift or significand or
  ;

  \ NaN exception
  : x-nan ( -- ) ." attempted to convert NaN to S31.32 fixed point" cr ;

  \ Convert a single-precision floating-point value to an S31.32 fixed-point
  \ value
  : v>f64 { f -- f64 }
    f v>sign { sign }
    f v>exponent { exponent }
    f significand-mask and { significand }
    exponent special-exponent = if
      significand 0= averts x-nan
      sign if $FFFF_FFFF_FFFF_FFFF. else $7FFF_FFFF_FFFF_FFFF. then exit
    then
    \ We can handle subnormal numbers like zeroes because they have less
    \ significance than what we can represent anyways
    exponent -32 < if 0,0 exit then
    exponent 30 > if
      sign if $FFFF_FFFF_FFFF_FFFF. else $7FFF_FFFF_FFFF_FFFF. then exit
    then
    hidden-bit significand or s>d
    exponent -9 < if
      9 exponent - 2rshift
    else
      exponent 9 + 2lshift
    then
    sign if dnegate then
  ;

  \ Positive infinity
  128 23 lshift constant +infinity

  \ Negative infinity
  128 23 lshift $8000_0000 or constant -infinity

  \ Add a character to a string
  : char> { c addr bytes count -- count' }
    count bytes < if
      c addr count + c!
      count 1+
    else
      bytes
    then
  ;

  \ Copy a string into a string
  : string> { addr0 bytes0 addr1 bytes1 -- addr1 bytes' }
    bytes0 bytes1 min { bytes }
    addr0 addr1 bytes move
    addr1 bytes
  ;

  \ Normalize a number and get the number of significant bits
  \ (physical-significand-size 1+ for normal numbers, 0 for zero)
  : normalize { f e -- f e significance }
    e -127 <> if f e [ physical-significand-size 1+ ] literal exit then
    f 0= if 0 -127 0 exit then
    0 { zero-bits }
    begin f $8000_0000 and 0= while
      f 1 lshift to f
      -1 +to e
      1 +to zero-bits
    repeat
    physical-significand-size zero-bits -
  ;

  \ Get the logarithm of 10 of a value
  : log10 ( D: x -- D: y ) ln 2,30258509299 f/ ;

  \ Convert a 2^x exponent to a 10^x exponent
  : 2x**log10 ( D: x -- D: y ) 0,30102999566 f* ;
  
  \ Get the number of bytes for the exponent
  : get-exponent-size { exponent -- bytes }
    exponent 0< if 1 else 0 then { sign-bytes }
    exponent abs 1 max s>f log10 round-zero 1+ sign-bytes +
  ;

  \ Number of sequential bits for massaging numbers
  12 constant massaging-bits

  \ Clean up a number
  : massage-number { y x -- y' x' }
    0 { count }
    0 { current-bit }
    30 { start-index }
    63 { index }
    false { massaging }
    begin
      y x index 2rshift drop 1 and if
        -1 +to index
        false
      else
        true
      then
    until
    index 31 min dup to start-index 1- to index
    begin
      y index rshift 1 and current-bit = index start-index < and if
        1 +to count
        count massaging-bits = if
          y $FFFF_FFFF 31 start-index 1- - rshift bic to y
          current-bit start-index lshift s>d y x d+ exit
        then
      else
        0 to count
        index to start-index
        y index rshift 1 and to current-bit
      then
      -1 +to index
      index -1 =
    until
    y x
  ;

  \ Determine if there is a carry, and if so, where
  : find-carry
    { extra-digit D: significance10 significand-bytes D: significand' }
    ( -- index carry? first-9? )
    -1 { index }
    0 { current }
    false { last-big }
    false { first-9? } 
    begin
      significance10 1,0 d>= significand-bytes 0> and significand' d0<> and
    while
      significand' nip { digit }
      extra-digit if
        digit 10 / { first-digit }
        first-digit -10 * s>f +to significand'
        significand' nip to digit
      then
      false to extra-digit
      significand' d0<> if
        digit 9 <> if
          current to index
        else
          index 0= if true to first-9? then
        then
        digit 4 > to last-big
      then
      significand' drop 0 10. d* to significand'
      -1,0 +to significance10
      -1 +to significand-bytes
      1 +to current
    repeat
    significance10 1,0 d>= significand-bytes 0> and if false to last-big then
    index last-big first-9?
  ;

  \ Table of multipliers created with:
  \
  \ for n in range(-126 - 23, 128):
  \     m = f(n)
  \    if n >= 0:
  \        print(10.0 ** (m - math.floor(m)))
  \    else:
  \        print(10.0 ** (1.0 + (m - math.ceil(m))))
  
  create multiplier-table
  1,401298464324835 2,
  2,8025969286496704 2,
  5,605193857299341 2,
  1,1210387714598684 2,
  2,242077542919737 2,
  4,4841550858394745 2,
  8,96831017167895 2,
  1,7936620343357903 2,
  3,587324068671581 2,
  7,174648137343163 2,
  1,434929627468633 2,
  2,869859254937266 2,
  5,739718509874533 2,
  1,1479437019749068 2,
  2,2958874039498136 2,
  4,591774807899628 2,
  9,183549615799258 2,
  1,8367099231598518 2,
  3,673419846319704 2,
  7,346839692639409 2,
  1,469367938527882 2,
  2,9387358770557643 2,
  5,87747175411153 2,
  1,1754943508223061 2,
  2,3509887016446123 2,
  4,701977403289225 2,
  9,403954806578453 2,
  1,8807909613156908 2,
  3,761581922631382 2,
  7,523163845262765 2,
  1,5046327690525532 2,
  3,0092655381051068 2,
  6,018531076210214 2,
  1,203706215242043 2,
  2,4074124304840865 2,
  4,814824860968174 2,
  9,629649721936348 2,
  1,92592994438727 2,
  3,8518598887745403 2,
  7,7037197775490815 2,
  1,5407439555098166 2,
  3,0814879110196336 2,
  6,162975822039268 2,
  1,2325951644078437 2,
  2,465190328815688 2,
  4,930380657631376 2,
  9,860761315262753 2,
  1,972152263052551 2,
  3,9443045261051024 2,
  7,888609052210206 2,
  1,5777218104420414 2,
  3,1554436208840833 2,
  6,310887241768167 2,
  1,2621774483536337 2,
  2,5243548967072673 2,
  5,0487097934145355 2,
  1,0097419586829073 2,
  2,019483917365815 2,
  4,03896783473163 2,
  8,077935669463262 2,
  1,6155871338926526 2,
  3,2311742677853053 2,
  6,462348535570611 2,
  1,2924697071141225 2,
  2,5849394142282454 2,
  5,169878828456492 2,
  1,0339757656912985 2,
  2,0679515313825974 2,
  4,135903062765195 2,
  8,271806125530324 2,
  1,6543612251060649 2,
  3,30872245021213 2,
  6,617444900424261 2,
  1,3234889800848524 2,
  2,646977960169705 2,
  5,293955920339411 2,
  1,0587911840678823 2,
  2,117582368135765 2,
  4,235164736271531 2,
  8,470329472543062 2,
  1,6940658945086127 2,
  3,388131789017226 2,
  6,7762635780344525 2,
  1,3552527156068908 2,
  2,710505431213782 2,
  5,421010862427564 2,
  1,084202172485513 2,
  2,168404344971026 2,
  4,336808689942053 2,
  8,673617379884108 2,
  1,7347234759768217 2,
  3,469446951953644 2,
  6,938893903907289 2,
  1,3877787807814579 2,
  2,775557561562916 2,
  5,551115123125833 2,
  1,1102230246251623 2,
  2,2204460492503246 2,
  4,44089209850065 2,
  8,881784197001302 2,
  1,7763568394002607 2,
  3,552713678800522 2,
  7,1054273576010445 2,
  1,421085471520209 2,
  2,8421709430404185 2,
  5,684341886080838 2,
  1,1368683772161676 2,
  2,2737367544323357 2,
  4,547473508864672 2,
  9,094947017729307 2,
  1,8189894035458618 2,
  3,637978807091724 2,
  7,275957614183449 2,
  1,45519152283669 2,
  2,9103830456733806 2,
  5,820766091346762 2,
  1,1641532182693526 2,
  2,328306436538705 2,
  4,656612873077411 2,
  9,313225746154824 2,
  1,862645149230965 2,
  3,7252902984619305 2,
  7,450580596923862 2,
  1,4901161193847696 2,
  2,9802322387695397 2,
  5,96046447753908 2,
  1,192092895507816 2,
  2,3841857910156325 2,
  4,768371582031266 2,
  9,536743164062514 2,
  1,9073486328125029 2,
  3,814697265625006 2,
  7,629394531250014 2,
  1,5258789062500029 2,
  3,051757812500006 2,
  6,103515625000013 2,
  1,2207031250000016 2,
  2,4414062500000036 2,
  4,882812500000008 2,
  9,765625000000007 2,
  1,9531250000000016 2,
  3,906250000000004 2,
  7,812500000000009 2,
  1,562500000000001 2,
  3,125000000000001 2,
  6,250000000000003 2,
  1,2500000000000004 2,
  2,5000000000000004 2,
  5,000000000000001 2,
  1,0 2,
  1,9999999999999998 2,
  3,999999999999999 2,
  7,999999999999997 2,
  1,5999999999999992 2,
  3,199999999999999 2,
  6,399999999999995 2,
  1,2799999999999987 2,
  2,5599999999999974 2,
  5,119999999999996 2,
  1,0239999999999994 2,
  2,047999999999997 2,
  4,095999999999994 2,
  8,19199999999999 2,
  1,6383999999999963 2,
  3,276799999999993 2,
  6,553599999999987 2,
  1,3107199999999977 2,
  2,6214399999999958 2,
  5,2428799999999915 2,
  1,0485759999999986 2,
  2,0971519999999932 2,
  4,1943039999999865 2,
  8,388607999999975 2,
  1,677721599999995 2,
  3,3554431999999905 2,
  6,710886399999982 2,
  1,3421772799999938 2,
  2,684354559999988 2,
  5,368709119999977 2,
  1,0737418239999956 2,
  2,1474836479999913 2,
  4,294967295999983 2,
  8,589934591999969 2,
  1,7179869183999938 2,
  3,435973836799988 2,
  6,871947673599977 2,
  1,3743895347199955 2,
  2,7487790694399914 2,
  5,497558138879984 2,
  1,0995116277759969 2,
  2,1990232555519853 2,
  4,398046511103971 2,
  8,796093022207943 2,
  1,7592186044415887 2,
  3,518437208883178 2,
  7,036874417766357 2,
  1,4073748835532716 2,
  2,8147497671065436 2,
  5,629499534213088 2,
  1,1258999068426176 2,
  2,2517998136852357 2,
  4,503599627370472 2,
  9,007199254740945 2,
  1,801439850948182 2,
  3,6028797018963643 2,
  7,2057594037927295 2,
  1,4411518807585462 2,
  2,8823037615170928 2,
  5,764607523034186 2,
  1,1529215046068375 2,
  2,305843009213675 2,
  4,611686018427351 2,
  9,223372036854704 2,
  1,844674407370941 2,
  3,6893488147418823 2,
  7,378697629483765 2,
  1,4757395258967532 2,
  2,951479051793507 2,
  5,9029581035870144 2,
  1,180591620717403 2,
  2,3611832414348064 2,
  4,722366482869614 2,
  9,44473296573923 2,
  1,888946593147846 2,
  3,7778931862956924 2,
  7,555786372591386 2,
  1,5111572745182773 2,
  3,022314549036555 2,
  6,044629098073111 2,
  1,2089258196146224 2,
  2,4178516392292253 2,
  4,8357032784584515 2,
  9,671406556916903 2,
  1,934281311383381 2,
  3,8685626227667624 2,
  7,737125245533526 2,
  1,5474250491067054 2,
  3,0948500982134113 2,
  6,1897001964268235 2,
  1,2379400392853648 2,
  2,47588007857073 2,
  4,951760157141461 2,
  9,903520314282924 2,
  1,9807040628565848 2,
  3,96140812571317 2,
  7,922816251426341 2,
  1,5845632502852685 2,
  3,1691265005705374 2,
  6,338253001141076 2,
  1,2676506002282153 2,
  2,535301200456431 2,
  5,070602400912863 2,
  1,0141204801825727 2,
  2,0282409603651455 2,
  4,056481920730292 2,
  8,112963841460585 2,
  1,6225927682921037 2,
  3,245185536584208 2,
  6,490371073168417 2,
  1,2980742146336837 2,
  2,5961484292673678 2,
  5,1922968585347355 2,
  1,0384593717069472 2,
  2,076918743413895 2,
  4,153837486827791 2,
  8,307674973655581 2,
  1,6615349947311167 2,
  3,3230699894622338 2,
  6,646139978924468 2,
  1,3292279957848938 2,
  2,658455991569788 2,
  5,316911983139577 2,
  1,0633823966279154 2,
  2,1267647932558313 2,
  4,253529586511663 2,
  8,507059173023327 2,
  1,7014118346046656 2,
  
  \ Convert a single-precision floating-point value to a string in the (-)x.yez
  \ format
  : format-float32-exponent { addr bytes f -- addr count }
    0 { count }
    f v>nan? if s" NaN" addr bytes string> exit then
    f v>infinite? if
      f v>sign if s" -Infinity" else s" +Infinity" then addr bytes string> exit
    then
    f v>significand f v>exponent normalize { significand exponent significance }
    significand 0= if s" 0e0" addr bytes string> exit then
    f v>sign if [char] - addr bytes count char> to count then
    exponent s>f 2x**log10 { D: exponent10 }
    significance s>f 2x**log10 { D: significance10 }
    exponent10 floor { exponent' }
    exponent [ 126 23 + ] literal + 2 cells * multiplier-table + 2@
    significand 0 1 2lshift f* swap $FFFF_FF00 and swap { D: significand' }
    false { extra-digit }
    significand' nip 10 >= if
      true to extra-digit
      1 +to exponent'
    then
    exponent' get-exponent-size { exponent-size }
    bytes count - 1- exponent-size - 0 max { significand-bytes }
    extra-digit significance10 significand-bytes significand' find-carry
    { carry-index carry? first-9? }
    carry-index -1 = carry? and extra-digit not and first-9? and if
      [char] 1 addr bytes count char> to count
      1 +to exponent'
      0. to significand'
    then
    true { first-digit }
    0 { index }
    begin
      significance10 d0> significand-bytes 0> and significand' d0<> and
    while
      significand' nip { digit }
      extra-digit if
        digit 10 / { first-digit }
        carry-index -1 = carry? and if
          first-digit 1+ [char] 0 + addr bytes count char> to count
          0. to significand'
          0 to digit
        else
          first-digit [char] 0 + addr bytes count char> to count
          first-digit -10 * s>f +to significand'
          significand' nip to digit
        then
      then
      extra-digit first-digit and significand' d0<> and if
        [char] . addr bytes count char> to count
        false to first-digit
      then
      false to extra-digit
      significand' d0<> if
        carry-index index = carry? and if
          1 +to digit
          0. to significand'
        else
          significand' drop 0 10. d* to significand'
        then
        digit [char] 0 + addr bytes count char> to count
      then
      first-digit significand' d0<> and if
        [char] . addr bytes count char> to count
        false to first-digit
      then
      -1,0 +to significance10
      -1 +to significand-bytes
      1 +to index
    repeat
    [char] e addr bytes count char> to count
    addr count + exponent' format-integer nip +to count
    addr count
  ;

  \ Print out a single-precision floating-point value without a space
  : (v.) ( f -- ) 15 [: 15 rot format-float32-exponent type ;] with-allot ;

  \ Print out a single-precision floating-point value with a space
  : v. ( f -- ) (v.) space ;
  
end-module
