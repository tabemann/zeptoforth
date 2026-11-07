\ Copyright (c) 2026 Peter Mather
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

\ float64_test.fs: double precision against IEEE doubles, by their bits. Written by make_float64_test.py, which has the cases: change them there.
\ It is for a float64 module with these words, which this tree does not have yet. The one it was written against is forth/float64.fs of https://github.com/UKTailwind/CMM2V1.5/tree/main/apps/zeptoforth
\ Each case is compiled, so literals are tried as a definition has them, and checks itself. include float64_test.fs, and the last line says how many failed.

float64 import

variable tests
variable fails
0 tests !
0 fails !
40 buffer: text

: fail ( -- ) 1 fails +! ." FAIL " tests @ . ;
: .bits ( lo hi -- ) base @ >r hex u. u. r> base ! ;

\ A value against the bits it should have, to within tol units in the last place
: check { D: got D: want tol -- }
  1 tests +!
  got want d- dup 0< if dnegate then 0= swap tol u<= and not if
    fail ." got " got .bits ." want " want .bits cr
  then
;

\ A cell against what it should be
: check-cell { got want -- }
  1 tests +!
  got want <> if fail ." got " got . ." want " want . cr then
;

\ A value as text
: check-text { D: f addr bytes -- }
  1 tests +!
  f text format-dv { count }
  text count addr bytes equal-strings? not if fail ." got " text count type ."  want " addr bytes type cr then
;

: k1 1.25de0 ;
: k2 k1 k1 dv* ;
2.5de0 2constant twofive
2variable dvar
6.5de0 dvar 2!

: test0 1de0 3de0 dv/ $55555555 $3FD55555 0 check ; test0
: test1 2de0 dvsqrt $667F3BCD $3FF6A09E 0 check ; test1
: test2 1.5de0 2.25de0 dv+ $00000000 $400E0000 0 check ; test2
: test3 1de0 1de-16 dv+ $00000000 $3FF00000 0 check ; test3
: test4 0.1de0 0.2de0 dv+ $33333334 $3FD33333 0 check ; test4
: test5 1de308 10de0 dv/ $3CAC7433 $7FAC7B1F 0 check ; test5
: test6 123456789de0 987654321de0 dv* $4FBFF538 $437B1311 0 check ; test6
: test7 5de-324 2de0 dv* $00000002 $00000000 0 check ; test7
: test8 1.7976931348623157de308 $FFFFFFFF $7FEFFFFF 0 check ; test8
: test9 2.2250738585072014de-308 $00000000 $00100000 0 check ; test9
: test10 7de0 dvnegate $00000000 $C01C0000 0 check ; test10
: test11 -7de0 dvabs $00000000 $401C0000 0 check ; test11
: test12 2147483647 n>dv $FFC00000 $41DFFFFF 0 check ; test12
: test13 -2147483648 n>dv $00000000 $C1E00000 0 check ; test13
: test14 $FFFFFFFF u>dv $FFE00000 $41EFFFFF 0 check ; test14
: test15 2.75de0 dvfloor $00000000 $40000000 0 check ; test15
: test16 -2.75de0 dvfloor $00000000 $C0080000 0 check ; test16
: test17 2.25de0 dvceil $00000000 $40080000 0 check ; test17
: test18 -2.25de0 dvceil $00000000 $C0000000 0 check ; test18
: test19 2.75de0 dvnfract $00000000 $40000000 0 check ; test19
: test20 -2.75de0 dvnfract $00000000 $C0000000 0 check ; test20
: test21 2.75de0 dvfract $00000000 $3FE80000 0 check ; test21
: test22 2.5de0 dvround-half-even $00000000 $40000000 0 check ; test22
: test23 3.5de0 dvround-half-even $00000000 $40100000 0 check ; test23
: test24 2.5de0 dvround-half-away-zero $00000000 $40080000 0 check ; test24
: test25 -2.5de0 dvround-half-away-zero $00000000 $C0080000 0 check ; test25
: test26 2.5de0 dvround-half-up $00000000 $40080000 0 check ; test26
: test27 2.5de0 dvround-half-down $00000000 $40000000 0 check ; test27
: test28 2.5de0 dvround-half-zero $00000000 $40000000 0 check ; test28
: test29 2.5de0 dvround-half-odd $00000000 $40080000 0 check ; test29
: test30 3.5de0 dvround-half-odd $00000000 $40080000 0 check ; test30
: test31 2.1de0 dvround-away-zero $00000000 $40080000 0 check ; test31
: test32 -2.1de0 dvround-away-zero $00000000 $C0080000 0 check ; test32
: test33 2.9de0 dvround-zero $00000000 $40000000 0 check ; test33
: test34 1de0 5de0 dvmin $00000000 $3FF00000 0 check ; test34
: test35 1de0 5de0 dvmax $00000000 $40140000 0 check ; test35
: test36 dv1 $00000000 $3FF00000 0 check ; test36
: test37 dv-1 $00000000 $BFF00000 0 check ; test37
: test38 dvpi $54442D18 $400921FB 0 check ; test38
: test39 7de0 4de0 dvmod $00000000 $BFF00000 0 check ; test39
: test40 1,5 f64>dv $00000000 $3FF80000 0 check ; test40
: test41 -2,25 f64>dv $00000000 $C0020000 0 check ; test41
: test42 $40490FDB v>dv $60000000 $400921FB 0 check ; test42
: test43 dvpi 6de0 dv/ dvsin $FFFFFFFF $3FDFFFFF 1 check ; test43
: test44 1de0 dvcos $0FB5068C $3FE14A28 1 check ; test44
: test45 1de0 dvtan $5CBEE3A6 $3FF8EB24 1 check ; test45
: test46 0.5de0 dvasin $382D7366 $3FE0C152 1 check ; test46
: test47 0.5de0 dvacos $382D7366 $3FF0C152 1 check ; test47
: test48 1de0 dvatan $54442D18 $3FE921FB 1 check ; test48
: test49 3de0 4de0 dvatan2 $A3269EE1 $3FE4978F 1 check ; test49
: test50 1de0 dvexp $8B145769 $4005BF0A 1 check ; test50
: test51 10de0 dvln $BBB55516 $40026BB1 1 check ; test51
: test52 1de-10 dvexpm1 $D9DDA4E3 $3DDB7CDF 1 check ; test52
: test53 1de-10 dvlnp1 $D9D1D693 $3DDB7CDF 1 check ; test53
: test54 1de0 dvsinh $C44EB982 $3FF2CD9F 1 check ; test54
: test55 1de0 dvcosh $51D9F550 $3FF8B075 1 check ; test55
: test56 1de0 dvtanh $B514F394 $3FE85EFA 1 check ; test56
: test57 1de0 dvasinh $6179D427 $3FEC3436 1 check ; test57
: test58 2de0 dvacosh $71980434 $3FF51242 1 check ; test58
: test59 0.5de0 dvatanh $7AAD030B $3FE193EA 1 check ; test59
: test60 2de0 10de0 dv** $00000000 $40900000 1 check ; test60
: test61 2de0 0.5de0 dv** $667F3BCD $3FF6A09E 1 check ; test61
: test62 2de0 10 dvi** $00000000 $40900000 1 check ; test62
: test63 2de0 -1de0 dv** $00000000 $3FE00000 1 check ; test63
: test64 1.5DE0 $00000000 $3FF80000 0 check ; test64
: test65 1.5dE0 $00000000 $3FF80000 0 check ; test65
: test66 1.5De0 $00000000 $3FF80000 0 check ; test66
: test67 -2.25de-3 $8D4FDF3B $BF626E97 0 check ; test67
: test68 .5de0 $00000000 $3FE00000 0 check ; test68
: test69 5.de0 $00000000 $40140000 0 check ; test69
: test70 1de+3 $00000000 $408F4000 0 check ; test70
: test71 2de3 $00000000 $409F4000 0 check ; test71
: test72 2.25de0 dv>f64 $40000000 $00000002 0 check ; test72
: test73 -2.25de0 dv>f64 $C0000000 $FFFFFFFD 0 check ; test73
: test74 1de0 2de0 dv< -1 check-cell ; test74
: test75 2de0 1de0 dv< 0 check-cell ; test75
: test76 1de0 1de0 dv<= -1 check-cell ; test76
: test77 1de0 1de0 dv= -1 check-cell ; test77
: test78 1de0 2de0 dv= 0 check-cell ; test78
: test79 1de0 2de0 dv<> -1 check-cell ; test79
: test80 2de0 1de0 dv> -1 check-cell ; test80
: test81 1de0 1de0 dv>= -1 check-cell ; test81
: test82 -1de0 dv0< -1 check-cell ; test82
: test83 0de0 dv0= -1 check-cell ; test83
: test84 1de0 dv0> -1 check-cell ; test84
: test85 0de0 dv0>= -1 check-cell ; test85
: test86 0de0 dv0<= -1 check-cell ; test86
: test87 1de0 dv0<> -1 check-cell ; test87
: test88 7 n>dv 2de0 dv/ dv>n 3 check-cell ; test88
: test89 -7 n>dv 2de0 dv/ dv>n -3 check-cell ; test89
: test90 3de9 dv>u -1294967296 check-cell ; test90
: test91 1de300 1de300 dv* dv>infinite? -1 check-cell ; test91
: test92 -1de0 dvsqrt dv>nan? -1 check-cell ; test92
: test93 1de0 dv>nan? 0 check-cell ; test93
: test94 1de0 dv>infinite? 0 check-cell ; test94
: test95 -1de0 dvsqrt 1de0 dv< 0 check-cell ; test95
: test96 -1de0 dvsqrt 1de0 dv> 0 check-cell ; test96
: test97 -1de0 dvsqrt -1de0 dvsqrt dv= 0 check-cell ; test97
: test98 -1de0 dvsqrt -1de0 dvsqrt dv<> -1 check-cell ; test98
: test99 [ hex ] 1de0 [ decimal ] 7648 check-cell ; test99
: test100 0.1de0 s" 0.1" check-text ; test100
: test101 1de100 s" 1e+100" check-text ; test101
: test102 -2.5de0 s" -2.5" check-text ; test102
: test103 1de0 3de0 dv/ s" 0.3333333333333333" check-text ; test103
: test104 dvpi s" 3.141592653589793" check-text ; test104
: test105 k2 s" 1.5625" check-text ; test105
: test106 twofive s" 2.5" check-text ; test106
: test107 dvar 2@ s" 6.5" check-text ; test107

: report ( -- ) ." float64: " tests @ . ." tests, " fails @ . ." failed" cr ;
report
