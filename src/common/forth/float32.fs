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

begin-module float32

  armv7m-fp import

  begin-module float32-internal

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
    
    \ Skip initial zeroes
    : skip-zeroes { addr bytes -- addr' bytes' zero? success? }
      false { zero-found? }
      begin
        bytes 0> if
          addr c@ [char] 0 = if
            true to zero-found? 1 +to addr -1 +to bytes false
          else
            true
          then
        else
          addr bytes zero-found? dup exit
        then
      until
      addr c@ dup [char] e = swap [char] E = or if
        addr bytes zero-found? dup exit
      then
      addr bytes false true
    ;

    \ Skip digits until decimal point
    : skip-decimal { addr bytes -- decimal? decimal-place addr' bytes' }
      0 { decimal-place }
      false { decimal? }
      begin
        bytes 0> if
          addr c@ dup [char] . = if
            drop 1 +to addr -1 +to bytes true to decimal? true
          else
            dup [char] 0 >= swap [char] 9 <= and if
              1 +to addr -1 +to bytes 1 +to decimal-place false
            else
              true
            then
          then
        else
          true
        then
      until
      decimal? decimal-place addr bytes
    ;

    \ Skip fraction until exponent or end
    : skip-fraction { addr bytes -- addr' bytes' places exponent? success? }
      false { non-zero-found? }
      0 { places }
      begin
        bytes 0> if
          addr c@ dup [char] e = over [char] E = or if
            drop addr 1+ bytes 1- places true true exit
          else
            dup [char] 0 = if
              non-zero-found? not if 1 +to places then
            else
              true to non-zero-found?
            then
            dup [char] 0 < swap [char] 9 > or if
              addr bytes 0 false false exit
            then
            1 +to addr -1 +to bytes false
          then
        else
          true
        then
      until
      addr bytes places false true
    ;

    \ Validate the exponent
    : validate-exponent { addr bytes -- success? }
      bytes 0<= if false exit then
      addr c@ dup [char] + = swap [char] - = or if
        1 +to addr
        -1 +to bytes
      then
      bytes 0<= if false exit then
      begin
        bytes 0> if
          addr c@ dup [char] 0 >= swap [char] 9 <= and if
            1 +to addr -1 +to bytes false
          else
            false exit
          then
        else
          true
        then
      until
      true
    ;

    \ Parse the exponent
    : parse-exponent { addr bytes -- exponent success? }
      false { exponent-sign }
      bytes 0<= if 0 false exit then
      addr c@ dup [char] + = swap [char] - = or if
        addr c@ [char] - = to exponent-sign
        1 +to addr
        -1 +to bytes
      then
      bytes 0<= if 0 false exit then
      addr bytes validate-exponent not if 2drop 0 false exit then
      base @ { saved-base }
      addr bytes [:
        10 base ! parse-integer
      ;] try saved-base base ! ?raise not if drop 0 false exit then
      exponent-sign if -1 else 1 then *
      true
    ;

    \ Normalize a significand
    : normalize-significand { D: f -- f' }
      f d0= if [: ." normalizing zero!" cr ;] ?raise then
      begin f 1,0 d>= while f 1 2rshift to f repeat
      f drop { f }
      begin f $8000_0000 and 0= while f 1 lshift to f repeat
      f
    ;

    \ Parse the significand
    : parse-significand
      { start-addr parse-addr decimal? -- places D: significand }
      0. { D: significand }
      1,0 { D:  multiplier }
      0 { places }
      decimal? not { non-zero-found? }
      begin parse-addr start-addr > while
        -1 +to parse-addr
        parse-addr c@ { digit }
        digit [char] 0 <> non-zero-found? or to non-zero-found?
        digit [char] . <> if
          non-zero-found? if
            digit [char] 0 - s>d multiplier d* +to significand
            significand 16_777_216,0 d>= if
              significand 10. d/ to significand
            else
              multiplier 10. d* to multiplier
              1 +to places
            then
          then
        then
      repeat
      places 1- significand
    ;

    \ Get the number of zero bits the value is adjusted to the right at
    : right-zero-bits { D: significand' -- bits }
      0 { zero-bits }
      begin significand' nip $8000_0000 and 0= while
        significand' 1 2lshift to significand'
        1 +to zero-bits
      repeat
      zero-bits
    ;

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
      -126 to e
      begin f $8000_0000 and 0= while
        f 1 lshift to f
        -1 +to e
        1 +to zero-bits
      repeat
      f e physical-significand-size zero-bits -
    ;

    \ Get the logarithm of 10 of a value
    : log10 ( D: x -- D: y ) ln 2,30258509299 f/ ;

    \ Convert a 2^x exponent to a 10^x exponent
    : 2x**log10 ( D: x -- D: y ) 0,30102999566 f* ;

    \ Convert a 10^x exponent to a 2^x exponent
    : 10x**log2 ( D: x -- D: y ) 3,32192809489 f* ;
    
    \ Get the number of bytes for the exponent
    : get-exponent-size { exponent -- bytes }
      exponent 0< if 1 else 0 then { sign-bytes }
      exponent abs 1 max s>f log10 round-zero 1+ sign-bytes +
    ;

    \ Determine if there is a carry, and if so, where
    : find-carry
      { extra-digit D: significance10 significand-bytes D: significand' }
      ( -- index carry? first-9? first-0 )
      -1 { index }
      0 { current }
      false { last-big }
      false { first-9? }
      -1 { first-0 }
      begin
        significance10 1,0 d>= significand-bytes 0> and significand' d0<> and
      while
        significand' nip { digit }
        extra-digit if
          digit 10 / { first-digit }
          first-digit -10 * s>f +to significand'
          significand' nip to digit
        then
        significand' d0<> if
          digit 0<> if current 1+ to first-0 then
          digit 9 <> if
            current to index
          else
            current 0= extra-digit not and if true to first-9? then
          then
          digit 4 > to last-big
        then
        false to extra-digit
        significand' drop 0 10. d* to significand'
        -1,0 +to significance10
        -1 +to significand-bytes
        1 +to current
      repeat
      significance10 1,0 d>= significand-bytes 0> and if false to last-big then
      index last-big first-9? first-0
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
    9,5367431640625 2,
    1,9073486328125 2,
    3,814697265625 2,
    7,62939453125 2,
    1,52587890625 2,
    3,0517578125 2,
    6,103515625 2,
    1,220703125 2,
    2,44140625 2,
    4,8828125 2,
    9,765625 2,
    1,953125 2,
    3,90625 2,
    7,8125 2,
    1,5625 2,
    3,125 2,
    6,25 2,
    1,25 2,
    2,5 2,
    5,0 2,
    1,0 2,
    2,0 2,
    4,0 2,
    8,0 2,
    1,6 2,
    3,2 2,
    6,4 2,
    1,28 2,
    2,56 2,
    5,12 2,
    1,024 2,
    2,048 2,
    4,096 2,
    8,192 2,
    1,6384 2,
    3,2768 2,
    6,5536 2,
    1,31072 2,
    2,62144 2,
    5,24288 2,
    1,048576 2,
    2,097152 2,
    4,194304 2,
    8,388608 2,
    1,6777216 2,
    3,3554432 2,
    6,7108864 2,
    1,34217728 2,
    2,68435456 2,
    5,36870912 2,
    1,073741824 2,
    2,147483648 2,
    4,294967296 2,
    8,589934592 2,
    1,7179869184 2,
    3,4359738368 2,
    6,8719476736 2,
    1,37438953472 2,
    2,74877906944 2,
    5,49755813888 2,
    1,099511627776 2,
    2,199023255552 2,
    4,398046511104 2,
    8,796093022208 2,
    1,7592186044416 2,
    3,5184372088832 2,
    7,0368744177664 2,
    1,40737488355328 2,
    2,81474976710656 2,
    5,62949953421312 2,
    1,125899906842624 2,
    2,251799813685248 2,
    4,503599627370496 2,
    9,007199254740993 2,
    1,8014398509481984 2,
    3,6028797018963967 2,
    7,2057594037927934 2,
    1,4411518807585588 2,
    2,8823037615171176 2,
    5,764607523034235 2,
    1,152921504606847 2,
    2,305843009213694 2,
    4,611686018427388 2,
    9,223372036854776 2,
    1,8446744073709551 2,
    3,6893488147419102 2,
    7,3786976294838205 2,
    1,475739525896764 2,
    2,951479051793528 2,
    5,902958103587056 2,
    1,1805916207174112 2,
    2,3611832414348224 2,
    4,722366482869645 2,
    9,44473296573929 2,
    1,8889465931478582 2,
    3,7778931862957164 2,
    7,555786372591433 2,
    1,5111572745182864 2,
    3,022314549036573 2,
    6,044629098073146 2,
    1,2089258196146293 2,
    2,4178516392292586 2,
    4,835703278458517 2,
    9,671406556917034 2,
    1,9342813113834065 2,
    3,868562622766813 2,
    7,737125245533626 2,
    1,5474250491067252 2,
    3,0948500982134504 2,
    6,189700196426901 2,
    1,2379400392853803 2,
    2,4758800785707606 2,
    4,951760157141521 2,
    9,903520314283043 2,
    1,9807040628566086 2,
    3,961408125713217 2,
    7,922816251426434 2,
    1,5845632502852869 2,
    3,1691265005705738 2,
    6,3382530011411475 2,
    1,2676506002282293 2,
    2,5353012004564586 2,
    5,070602400912917 2,
    1,0141204801825836 2,
    2,028240960365167 2,
    4,056481920730334 2,
    8,112963841460669 2,
    1,6225927682921335 2,
    3,245185536584267 2,
    6,490371073168534 2,
    1,298074214633707 2,
    2,596148429267414 2,
    5,192296858534828 2,
    1,0384593717069657 2,
    2,0769187434139313 2,
    4,153837486827863 2,
    8,307674973655725 2,
    1,6615349947311449 2,
    3,3230699894622897 2,
    6,646139978924579 2,
    1,3292279957849158 2,
    2,6584559915698316 2,
    5,316911983139663 2,
    1,0633823966279328 2,
    2,1267647932558655 2,
    4,253529586511731 2,
    8,507059173023462 2,
    1,7014118346046923 2,

    \ CPACR register address
    $E000ED88 constant CPACR
    
    \ Enable FPU bits
    %1111 10 2 * lshift constant CPACR_FPU_BITS

    \ FPCCR register address
    $E000EF34 constant FPCCR

    \ FPCCR LSPEN and ASPEN bits
    %11 30 lshift constant FPCCR_ASPEN_LSPEN_BITS
    
    \ Enable floating point
    : enable-float ( -- )
      CPACR_FPU_BITS CPACR bit@ not if
        FPCCR_ASPEN_LSPEN_BITS FPCCR bic!
        CPACR_FPU_BITS CPACR bis!
      then
    ;
    
    initializer enable-float
    
  end-module> import

  compress-flash
  
  \ Load floating point registers from stack
  : vload ( x y z w -- )
    [inlined]
    code[
    tos vmsrfpscr,_
    tos 1 dp ldm
    tos s0 vmov.f32.cr_,_
    tos 1 dp ldm
    tos s1 vmov.f32.cr_,_
    tos 1 dp ldm
    tos s2 vmov.f32.cr_,_
    tos 1 dp ldm
    ]code
  ;

  \ Save floating point registers on stack
  : vsave ( -- x y z w )
    [inlined]
    code[
    16 dp subs_,#_
    12 dp tos str_,[_,#_]
    s2 tos vmov.cr.f32_,_
    8 dp tos str_,[_,#_]
    s1 tos vmov.cr.f32_,_
    4 dp tos str_,[_,#_]
    s0 tos vmov.cr.f32_,_
    0 dp tos str_,[_,#_]
    tos vmrs_,fpscr
    ]code
  ;
  
  \ Get the absolute value of a single-precision floating-point value
  : vabs ( f -- f' )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s0 s0 vabs.f32_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Negate a single-precision floating-point value
  : vnegate ( f -- f' )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s0 s0 vneg.f32_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Get the square root of a single-precision floating-point value
  : vsqrt ( f -- f' )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s0 s0 vsqrt.f32_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Add two single-precision floating-point values
  : v+ ( f1 f0 -- f' )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    s0 s1 s0 vadd.f32_,_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Subtract two single-precision floating-point values
  : v- ( f1 f0 -- f' )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    s0 s1 s0 vsub.f32_,_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Multiply two single-precision floating-point values
  : v* ( f1 f0 -- f' )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    s0 s1 s0 vmul.f32_,_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Divide two single-precision floating-point values
  : v/ ( f1 f0 -- f' )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    s0 s1 s0 vdiv.f32_,_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Get the minimum of two single-precision floating-point values
  : vmin ( f1 f0 -- f' )
    [ fpv5? ] [if]
      [inlined]
      code[
      tos s0 vmov.f32.cr_,_
      s1 s1 dp vldmia!.f32_,{_..._}
      s0 s1 s0 vminnm.f32_,_,_
      s0 tos vmov.cr.f32_,_
      ]code
    [else]
      code[
      tos s0 vmov.f32.cr_,_
      s1 s1 dp vldmia!.f32_,{_..._}
      s0 s1 vcmpe.f32_,_
      apsr-nzcv vmrs_,fpscr
      ge bc>
      s1 tos vmov.cr.f32_,_
      pc 1 pop
      >mark
      s0 tos vmov.cr.f32_,_
      ]code
    [then]
  ;

  \ Get the maximum of two single-precision floating-point values
  : vmax ( f1 f0 -- f' )
    [ fpv5? ] [if]
      [inlined]
      code[
      tos s0 vmov.f32.cr_,_
      s1 s1 dp vldmia!.f32_,{_..._}
      s0 s1 s0 vmaxnm.f32_,_,_
      s0 tos vmov.cr.f32_,_
      ]code
    [else]
      code[
      tos s0 vmov.f32.cr_,_
      s1 s1 dp vldmia!.f32_,{_..._}
      s0 s1 vcmpe.f32_,_
      apsr-nzcv vmrs_,fpscr
      lt bc>
      s1 tos vmov.cr.f32_,_
      pc 1 pop
      >mark
      s0 tos vmov.cr.f32_,_
      ]code
    [then]
  ;

  \ Get whether a single-precision floating-point value is less
  : v< ( f1 f0 -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    0 tos movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    ge bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is less or equal
  : v<= ( f1 f0 -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    0 tos movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    gt bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is equal
  : v= ( f1 f0 -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    0 tos movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    ne bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is not equal
  : v<> ( f1 f0 -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    0 tos movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    eq bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is greater
  : v> ( f1 f0 -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    0 tos movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    le bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is greater or equal
  : v>= ( f1 f0 -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    s1 s1 dp vldmia!.f32_,{_..._}
    0 tos movs_,#_
    s0 s1 vcmpe.f32_,_
    apsr-nzcv vmrs_,fpscr
    lt bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is less than zero
  : v0< ( f -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    0 tos movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    ge bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is less than or equal to
  \ zero
  : v0<= ( f -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    0 tos movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    gt bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is equal to zero
  : v0= ( f -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    0 tos movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    ne bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is not equal to zero
  : v0<> ( f -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    0 tos movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    eq bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is greater than zero
  : v0> ( f -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    0 tos movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    le bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get whether a single-precision floating-point value is greater than or
  \ equal to zero
  : v0>= ( f -- flag )
    code[
    tos s0 vmov.f32.cr_,_
    0 tos movs_,#_
    s0 vcmpe.f32_,#0.0
    apsr-nzcv vmrs_,fpscr
    lt bc>
    tos tos mvns_,_
    pc 1 pop
    >mark
    ]code
  ;

  \ Get the non-fractional portion of a single-precision floating-point value
  : vnfract ( f -- f' )
    [ fpv5? ] [if]
      [inlined]
      code[
      tos s0 vmov.f32.cr_,_
      s0 s0 vrintz.f32_,_
      s0 tos vmov.cr.f32_,_
      ]code
    [else]
      dup v>exponent dup 0>= if
        23 - 0 min physical-significand-size swap +
        significand-mask swap rshift bic
      else
        2drop 0
      then
    [then]
  ;
  
  \ Get the fractional portion of a single-precision floating-point value
  : vfract ( f -- f' )
    [ fpv5? ] [if]
      [inlined]
      code[
      tos s0 vmov.f32.cr_,_
      s0 s1 vrintz.f32_,_
      s1 s0 s0 vsub.f32_,_,_
      s0 tos vmov.cr.f32_,_
      ]code
    [else]
      dup vnfract v-
    [then]
  ;

  \ Get the modulus of two single-precision floating-point values
  : vmod ( f1 f0 -- f' )
    [ fpv5? ] [if]
      [inlined]
      code[
      tos s0 vmov.f32.cr_,_
      s1 s1 dp vldmia!.f32_,{_..._}
      s0 s1 s2 vdiv.f32_,_,_
      s2 s2 vrintn.f32_,_
      s0 s2 s2 vmul.f32_,_,_
      s2 s1 s1 vsub.f32_,_,_
      s1 tos vmov.cr.f32_,_
      ]code
    [else]
      2dup v/ vnfract v* v-
    [then]
  ;
  
  \ Convert an S15.16 fixed-point value to a single-precision floating-point
  \ value
  : f32>v ( f32 -- f )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    16 s0 vcvt.f32.s32_,#<fbits>
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Convert a single-precision floating-point value to an S15.16 fixed-point
  \ value
  : v>f32 ( f -- f32 )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    16 s0 vcvt.s32.f32_,#<fbits>
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Convert a single-precision floating-point value to a signed 32-bit integer
  \ rounding towards zero
  : v>n ( f -- n )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s0 s0 vcvtr.s32.f32_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Convert a single-precision floating-point value to an unsigned 32-bit
  \ integer rounding towards zero
  : v>u ( f -- u )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s0 s0 vcvtr.u32.f32_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Convert a signed 32-bit integer to a single-precision floating-point value
  : n>v ( n -- f )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s0 s0 vcvt.f32.s32_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  \ Convert an unsigned 32-bit integer to a single-precision floating-point
  \ value
  : u>v ( u -- f )
    [inlined]
    code[
    tos s0 vmov.f32.cr_,_
    s0 s0 vcvt.f32.u32_,_
    s0 tos vmov.cr.f32_,_
    ]code
  ;

  end-compress-flash
  
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

  \ One
  1,0 f64>v constant v1

  \ Minus one
  -1,0 f64>v constant v-1
  
  \ Positive infinity
  255 physical-significand-size lshift constant +infinity

  \ Negative infinity
  255 physical-significand-size lshift sign-mask or constant -infinity

  \ NaN
  255 physical-significand-size lshift 1 or constant nan
  
  \ Convert a single-precision floating-point value to a string in the (-)x.yez
  \ format
  : format-float32-exponent { addr bytes f -- addr count }
    0 { count }
    f v>nan? if s" NaN" addr bytes string> exit then
    f v>infinite? if
      f v>sign if s" -Infinity" else s" +Infinity" then addr bytes string> exit
    then
    f v>significand f v>exponent normalize s>f 2x**log10
    { significand exponent D: significance10 }
    significand 0= if s" 0e0" addr bytes string> exit then
    significance10 1,0 d< if s" 0e0" addr bytes string> exit then
    f v>sign if [char] - addr bytes count char> to count then
    exponent s>f 2x**log10 floor { exponent' }
    exponent [ 126 23 + ] literal + 2 cells * multiplier-table + 2@
    significand 0 1 2lshift f* { D: significand' }
    false { extra-digit }
    significand' nip 10 >= if
      true to extra-digit
      1 +to exponent'
    then
    exponent' get-exponent-size { exponent-size }
    bytes count - 1- exponent-size - 0 max 7 min { significand-bytes }
    extra-digit if -1 +to significand-bytes then
    extra-digit significance10 significand-bytes significand' find-carry
    { carry-index carry? first-9? first-0 }
    carry-index -1 = carry? and first-9? and if
      [char] 1 addr bytes count char> to count
      1 +to exponent'
      0. to significand'
    then
    true { first-digit }
    0 { index }
    begin
      significance10 1,0 d>= significand-bytes 0> and significand' d0<> and
      index first-0 <> and
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
      first-0 -1 = first-0 0= or if
        0. to significand'
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
      first-digit significand' d0<> and index 1+ first-0 <> and if
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

  \ Parse a single-precision floating-point value as a string
  : parse-float32 { addr bytes -- f success? }
    addr bytes s" +infinity" equal-case-strings? if +infinity true exit then
    addr bytes s" -infinity" equal-case-strings? if -infinity true exit then
    addr bytes s" nan" equal-case-strings? if nan true exit then
    bytes 0<= if 0 false exit then
    false { sign }
    addr c@ dup [char] + = swap [char] - = or if
      addr c@ [char] - = to sign
      1 +to addr
      -1 +to bytes
    then
    addr bytes skip-zeroes not if drop 2drop 0 false exit then
    if 2drop 0 true exit then
    to bytes to addr
    addr { start-addr }
    addr c@ dup [char] . = over [char] 0 >= rot [char] 9 <= and or not if
      0 false exit
    then
    addr bytes skip-decimal to bytes to addr { decimal? decimal-place }
    addr bytes skip-fraction not if 2drop 2drop 0 false exit then
    { fraction-places exponent? } to bytes to addr
    addr { parse-addr }
    0 { exponent10 }
    exponent? if
      -1 +to parse-addr
      addr bytes validate-exponent not if 0 false exit then
      addr bytes parse-exponent not if 0 false exit then to exponent10
    then
    start-addr parse-addr decimal? parse-significand { places D: significand' }
    significand' d0= if 0 true exit then
    exponent10 s>f 10x**log2 { D: exponent2 }
    significand' 10,0 places 1+ decimal-place - fi** f/ to significand'
    exponent2 exponent2 round-zero s>f d- { D: fract }
    fract d0<> if
      significand' 2,0 fract f** f* to significand'
      fract dnegate +to exponent2
    then
    significand' right-zero-bits 31 - negate s>f +to exponent2
    exponent2 127,0 d> if
      sign if -infinity else +infinity then true exit
    then
    exponent2 -149,0 d< if 0 true exit then
    significand' normalize-significand { significand }
    exponent2 -126,0 d< if
      significand exponent2 floor 127 + abs rshift to significand
      -127,0 to exponent2
    else
      significand 1 lshift to significand
    then
    sign if sign-mask else 0 then
    exponent2 floor exponent-bias + physical-significand-size lshift or
    significand [ 32 physical-significand-size - ] literal rshift or
    true
  ;

  \ Get the ceiling of a single-precision floating-point value
  : vceil ( f -- f' )
    dup vnfract { int } vfract { fract }
    int dup v0>= if fract v0> if v1 else 0 then v+ then
  ;

  \ Get the floor of a single-precision floating-point value
  : vfloor ( f -- f' )
    dup vnfract { int } vfract { fract }
    int dup v0< if fract v0< if v-1 else 0 then v+ then
  ;

  \ Round a single-precision floating-point value to the nearest integer with
  \ half rounding up
  : vround-half-up ( f -- f' )
    dup vnfract { int } vfract { fract }
    int v0>= if
      fract [ 0,5 f64>v ] literal v>= if v1 else 0 then
    else
      fract [ -0,5 f64>v ] literal v< if v-1 else 0 then
    then
    int v+
  ;

  \ Round a double-precision floating-point value to the nearest integer with
  \ half rounding down
  : vround-half-down ( f -- f' )
    dup vnfract { int } vfract { fract }
    int v0>= if
      fract [ 0,5 f64>v ] literal v> if v1 else 0 then
    else
      fract [ -0,5 f64>v ] literal v<= if v-1 else 0 then
    then
    int v+
  ;

  \ Round a single-precision floating-point value to the nearest integer with
  \ half rounding towards zero
  : vround-half-zero ( f -- f' )
    dup vnfract { int } vfract { fract }
    int v0>= if
      fract [ 0,5 f64>v ] literal v> if v1 else 0 then
    else
      fract [ -0,5 f64>v ] literal v< if v-1 else 0 then
    then
    int v+
  ;

  \ Round a single-precision floating-point value to the nearest integer with
  \ half rounding away from zero
  : vround-half-away-zero ( f -- f' )
    dup vnfract { int } vfract { fract }
    int v0>= if
      fract [ 0,5 f64>v ] literal v>= if v1 else 0 then
    else
      fract [ -0,5 f64>v ] literal v<= if v-1 else 0 then
    then
    int v+
  ;

  \ Round a single-precision floating-point value to the nearest integer with
  \ half rounding towards even
  : vround-half-even ( f -- f' )
    dup vnfract [ 2,0 f64>v ] literal vmod v0= if
      vround-half-down
    else
      vround-half-up
    then
  ;

  \ Round a single-precision floating-point value to the nearest integer with
  \ half rounding towards odd
  : vround-half-odd ( f -- f' )
    dup vnfract [ 2,0 f64>v ] literal vmod v0= if
      vround-half-up
    else
      vround-half-down
    then
  ;
  
  \ Round a single-precision floating-point value towards zero
  : vround-zero ( f -- f' ) [inlined] vnfract ;

  \ Round a single-precision floating-point value away from zero
  : vround-away-zero ( f -- f' )
    dup vnfract { int } vfract { fract }
    int v0>= if
      fract v0<> if v1 else 0 then
    else
      fract v0<> if v-1 else 0 then
    then
    int v+
  ;

  \ Pi as a single-precision floating-point value
  pi f64>v constant vpi

  \ Exponentiate a single-precision floating-point value by an integer
  : vi** ( f exponent -- f' )
    dup 0> if
      v1 begin
        over 1 and if
          2 pick v*
        then
        swap 1 rshift dup 0= if
          drop nip true
        else
          rot dup v* swap rot false
        then
      until
    else
      0= if
        drop v1
      else
        drop 0
      then
    then
  ;

  \ Get the (e^x)-1 of a single-precision floating-point value
  : vexpm1 ( f -- f' )
    >r 0 v1 v1 begin
      swap r@ v* over v/ dup vabs 0= >r rot over v+ swap rot v1 v+ r>
    until
    rdrop 2drop
  ;

  \ Get the e^x of a single-precision floating-point value
  : vexp ( f -- f' ) vexpm1 v1 v+ ;
  
  \ Domain error exception
  : x-domain-error ( -- ) ." domain error" cr ;
  
  \ Get the ln(x+1) of a single-precision floating-point value
  : vlnp1 ( f -- f' )
    dup v-1 v<> averts x-domain-error
    dup vabs v1 v<= if
      dup dup v1 v1 { n x xy** y sign }
      begin
        v-1 sign v* to sign
        xy** x v* to xy**
        v1 y v+ to y
        xy** y v/ sign v* { w }
        n w v+ to n
        w v0=
      until
      n
    else
      v1 v+ 32 >r >r 0 begin
        dup vexp
        dup r@ swap v-
        r@ rot v+
        v/
        [ 2,0 f64>v ] literal v*
        over v+
        dup rot v-
        v>significand 0= r> r> 1- dup >r swap >r 0= or
      until
      rdrop rdrop
    then
  ;
  
  \ Get the ln(x) of a single-precision floating-point value
  : vln ( f -- f' ) v1 v- vlnp1 ;

  \ Get the sine of a single-precision floating-point value
  : vsin ( f -- f' )
    [ pi 2,0 f* f64>v ] literal vmod
    dup dup >r v1 begin
      swap r@ v* r@ v*
      over [ 2,0 f64>v ] literal v* v/
      over [ 2,0 f64>v ] literal v*
      v1 v+ v/
      dup v>significand 0= >r
      rot
      over
      3 pick [ 2,0 f64>v ] literal vmod v0= if v+ else v- then
      swap
      rot
      v1 v+
      r>
    until
    rdrop 2drop
  ;

  \ Get the cosine of a single-precision floating-point value
  : vcos ( f -- f' ) [ pi 2,0 f/ f64>v ] literal swap v- vsin ;

  \ Get the tangent of a single-precision floating-point value
  : vtan ( f -- f' ) dup vsin swap vcos v/ ;

  continue-module float32-internal

    12,595802263029547 f64>v constant RR1
    -86,186317517509520 f64>v constant RR2
    -1,2766919133361079 f64>v constant RR3
    -0,083921038065840512 f64>v constant RR4
    27,096164294378656 f64>v constant SS1
    6,5581320451487386 f64>v constant SS2
    2,1441643116703661 f64>v constant SS3
    1,2676256708212610 f64>v constant SS4
    1,7320508075688772 f64>v constant RT3
    0,52359877559829887 f64>v constant PIBY6
    0,57079632679489661 f64>v constant PIBY2M1
    0,73205080756887728 f64>v constant RT3M1
    0,26794919243112271 f64>v constant TANPIBY12

  end-module
  
  \ Get the arctangent of a single-precision floating-point value
  : vatan { x1 -- f }
    0 0 { xx1 const }
    false false { sign inv }
    x1 v0< if true to sign x1 vnegate to xx1 else x1 to xx1 then
    \ cr ." sign: " sign . ." xx1: " xx1 v.
    xx1 v1 v> if v1 xx1 v/ to xx1 true to inv then
    \ cr ." inv: " inv . ." xx1: " xx1 v.
    xx1 TANPIBY12 v> if
      RT3M1 xx1 v* v1 v- xx1 v+ xx1 RT3 v+ v/ to xx1
      PIBY6 to const
    then
    \ cr ." const: " const v. ." xx1: " xx1 v.
    xx1 xx1 v* { xsq }
    \ cr ." xsq: " xsq v.
    xsq SS4 v+ RR4 swap v/ xsq v+ SS3 v+ RR3 swap v/
    xsq v+ SS2 v+ RR2 swap v/ xsq v+ SS1 v+ RR1 swap v/ xx1 v* to xx1
    \ cr ." xx1: " xx1 v.
    xx1 const v+ to xx1
    \ cr ." xx1: " xx1 v.
    inv if v1 xx1 v- PIBY2M1 v+ to xx1 then
    \ cr ." xx1: " xx1 v.
    sign if xx1 vnegate to xx1 then
    \ cr ." xx1: " xx1 v.
    xx1
  ;

  \ Get the angle of an x and a y single-precision floating-point values
  : vatan2 ( fy fx -- fangle )
    dup v0> if
      v/ vatan
    else
      2dup v0< swap v0>= and if
        v/ vatan vpi v+
      else
        2dup v0< swap v0< and if
          v/ vatan vpi v-
        else
          2dup v0= swap v0> and if
            2drop [ pi 2,0 f/ f64>v ] literal
          else
            2dup v0= swap v0< and if
              2drop [ pi -2,0 f/ f64>v ] literal
            else
              2drop 0
            then
          then
        then
      then
    then
  ;

  \ Get the arcsine of a single-precision floating-point value
  : vasin ( f -- f' )
    dup dup v* v1 v< if
      v1 over dup v* v- vsqrt v/ vatan
    else
      v0> if
        [ pi 2,0 f/ f64>v ] literal
      else
        [ pi -2,0 f/ f64>v ] literal
      then
    then
  ;

  \ Get the arccosine of a single-precision floating-point value
  : vacos ( f -- f' )
    vasin vnegate [ pi 2,0 f/ f64>v ] literal v+
  ;

  \ Exponentiate two single-precision floating-point values
  : v** ( fb fx -- fb^fx )
    over v0= >r dup v0= r> and triggers x-domain-error
    dup vfract v0= over v>exponent 30 <= and if
      dup v>n 0>= if
        v>n vi**
      else
        v>n negate vi** v1 swap v/
      then
    else
      over v0>= averts x-domain-error swap vln v* vexp
    then
  ;

  \ Get the hyperbolic sine of an S15.16 fixed-point number
  : vsinh ( f -- f' )
    vexpm1 dup dup v1 v+ v/ v+ [ 2,0 f64>v ] literal v/
  ;
  
  \ Get the hyperbolic cosine of an S15.16 fixed-point number
  : vcosh ( f -- f' )
    vexpm1 dup dup v1 v+ v/ v- [ 2,0 f64>v ] literal v/ v1 v+
  ;

  \ Get the hyperbolic tangent of an S15.16 fixed-point number
  : vtanh ( f -- f' )
    dup vsinh swap vcosh v/
  ;

  \ Get the hyperbolic arcsine of an S15.16 fixed-point number
  : vasinh ( f -- f' )
    dup dup v* v1 v+ vsqrt v+ vln
  ;

  \ Get the hyperbolic arccosine of an S15.16 fixed-point number
  : vacosh ( f -- f' )
    dup dup v* v1 v- vsqrt v+ vln
  ;

  \ Get the hyperbolic arctangent of an S15.16 fixed-point number
  : vatanh ( f -- f' )
    dup v1 v+ swap vnegate v1 v+ v/ vln
    [ 2,0 f64>v ] literal v/
  ;

  continue-module float32-internal

    \ Saved handle number hook
    variable saved-handle-number-hook

    \ Confirm that a value contains an exponent
    : check-exponent ( addr bytes -- flag )
      over + swap ?do
        i c@ dup [char] e = swap [char] E = or if unloop true exit then
      loop
      false
    ;
    
    \ Handle numeric literals
    : do-handle-number ( addr bytes -- flag )
      2dup saved-handle-number-hook @ execute if
        state @ not if
          rot rot
        then
        2drop true
      else
        2dup check-exponent if
          parse-float32 if
            state @ if
              lit, true
            else
              true
            then
          else
            drop false
          then
        else
          2drop false
        then
      then
    ;
    
    \ Initialize
    : init-handle-number ( -- )
      handle-number-hook @ saved-handle-number-hook !
      ['] do-handle-number handle-number-hook !
    ;

    initializer init-handle-number

  end-module
  
end-module

reboot
