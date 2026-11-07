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

\ armv7m_fp64_run_test.fs: the instructions of src/common/forth/armv7m_fp64.fs run on a double-precision FPU, their results checked by their bits. Written by make_armv7m_fp64_run_test.py.
\ Load armv7m_fp.fs and armv7m_fp64.fs first. It needs a Cortex-M7 with FPv5-D16, and it ends by saying how many cases failed.

armv7m-fp import

variable tests
variable fails
0 tests !
0 fails !

: bad ( -- ) 1 fails +! ." FAIL case " tests @ . cr ;
: check2 { D: got D: want -- } 1 tests +! got want d= not if bad ." got " got swap base @ >r hex u. u. r> base ! cr then ;
: check1 { got want -- } 1 tests +! got want <> if bad ." got " got . cr then ;

: t0 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2 vadd.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0
  vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $3FF80000 $00000000 $40020000 t0 $00000000 $400E0000 check2
: t1 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2 vsub.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0
  vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $3FF00000 $00000000 $40080000 t1 $00000000 $C0000000 check2
: t2 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2 vmul.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0
  vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $3FF80000 $00000000 $40020000 t2 $00000000 $400B0000 check2
: t3 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2 vdiv.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0
  vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $3FF00000 $00000000 $40080000 t3 $55555555 $3FD55555 check2
: t4 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2 vnmul.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0
  vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $3FF80000 $00000000 $40000000 t4 $00000000 $C0080000 check2
: t5 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2 vmaxnm.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos
  r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $3FF00000 $00000000 $40140000 t5 $00000000 $40140000 check2
: t6 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2 vminnm.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos
  r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $3FF00000 $00000000 $40140000 t6 $00000000 $3FF00000 check2
: t7 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vsqrt.f64_,_ 4 dp subs_,#_ 0 dp
  tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $40000000 t7 $667F3BCD $3FF6A09E check2
: t8 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vneg.f64_,_ 4 dp subs_,#_ 0 dp
  tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $401C0000 t8 $00000000 $C01C0000 check2
: t9 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vabs.f64_,_ 4 dp subs_,#_ 0 dp
  tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $C01C0000 t9 $00000000 $401C0000 check2
: t10 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vmov.f64_,_ 4 dp subs_,#_ 0 dp
  tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $40040000 t10 $00000000 $40040000 check2
: t11 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vrinta.f64_,_ 4 dp subs_,#_ 0
  dp tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $40040000 t11 $00000000 $40080000 check2
: t12 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vrintn.f64_,_ 4 dp subs_,#_ 0
  dp tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $40040000 t12 $00000000 $40000000 check2
: t13 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vrintp.f64_,_ 4 dp subs_,#_ 0
  dp tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$CCCCCCCD $4000CCCC t13 $00000000 $40080000 check2
: t14 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vrintm.f64_,_ 4 dp subs_,#_ 0
  dp tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$CCCCCCCD $C000CCCC t14 $00000000 $C0080000 check2
: t15 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vrintz.f64_,_ 4 dp subs_,#_ 0
  dp tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$33333333 $C0073333 t15 $00000000 $C0000000 check2
: t16 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d1 vrintr.f64_,_ 4 dp subs_,#_ 0
  dp tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $400C0000 t16 $00000000 $40100000 check2
: t17 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d2 vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2
  vmla.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0
  str_,[_,#_] ]code ;
$00000000 $401C0000 $00000000 $40080000 $00000000 $40140000 t17 $00000000 $40360000 check2
: t18 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d2 vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2
  vmls.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0
  str_,[_,#_] ]code ;
$00000000 $401C0000 $00000000 $40080000 $00000000 $40140000 t18 $00000000 $C0200000 check2
: t19 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d2 vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2
  vnmla.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0
  str_,[_,#_] ]code ;
$00000000 $401C0000 $00000000 $40080000 $00000000 $40140000 t19 $00000000 $C0360000 check2
: t20 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d2 vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2
  vnmls.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0
  str_,[_,#_] ]code ;
$00000000 $401C0000 $00000000 $40080000 $00000000 $40140000 t20 $00000000 $40200000 check2
: t21 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d2 vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2
  vfma.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0
  str_,[_,#_] ]code ;
$00000000 $401C0000 $00000000 $40080000 $00000000 $40140000 t21 $00000000 $40360000 check2
: t22 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d2 vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2
  vfms.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0
  str_,[_,#_] ]code ;
$00000000 $401C0000 $00000000 $40080000 $00000000 $40140000 t22 $00000000 $C0200000 check2
: t23 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d2 vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2
  vfnma.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0
  str_,[_,#_] ]code ;
$00000000 $401C0000 $00000000 $40080000 $00000000 $40140000 t23 $00000000 $C0360000 check2
: t24 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d2 vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 d2
  vfnms.f64_,_,_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0
  str_,[_,#_] ]code ;
$00000000 $401C0000 $00000000 $40080000 $00000000 $40140000 t24 $00000000 $40200000 check2
: t25 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ d0 s0 vcvt.s32.f64_,_ s0 tos vmov.cr.f32_,_ ]code
  ;
$00000000 $C01E0000 t25 $FFFFFFF9 check1
: t26 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ d0 s0 vcvtr.s32.f64_,_ s0 tos vmov.cr.f32_,_
  ]code ;
$00000000 $40040000 t26 $00000002 check1
: t27 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ d0 s0 vcvta.s32.f64_,_ s0 tos vmov.cr.f32_,_
  ]code ;
$00000000 $40040000 t27 $00000003 check1
: t28 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ d0 s0 vcvtn.s32.f64_,_ s0 tos vmov.cr.f32_,_
  ]code ;
$00000000 $400C0000 t28 $00000004 check1
: t29 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ d0 s0 vcvtp.s32.f64_,_ s0 tos vmov.cr.f32_,_
  ]code ;
$CCCCCCCD $4000CCCC t29 $00000003 check1
: t30 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ d0 s0 vcvtm.s32.f64_,_ s0 tos vmov.cr.f32_,_
  ]code ;
$CCCCCCCD $C000CCCC t30 $FFFFFFFD check1
: t31 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ d0 s0 vcvt.u32.f64_,_ s0 tos vmov.cr.f32_,_ ]code
  ;
$C0000000 $41E65A0B t31 $B2D05E00 check1
: t32 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ d0 s0 vcvta.u32.f64_,_ s0 tos vmov.cr.f32_,_
  ]code ;
$00000000 $40040000 t32 $00000003 check1
: t33 code[ tos s0 vmov.f32.cr_,_ s0 d0 vcvt.f64.s32_,_ d0 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0
  dp r0 str_,[_,#_] ]code ;
-7 t33 $00000000 $C01C0000 check2
: t34 code[ tos s0 vmov.f32.cr_,_ s0 d0 vcvt.f64.u32_,_ d0 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0
  dp r0 str_,[_,#_] ]code ;
$FFFFFFFF t34 $FFE00000 $41EFFFFF check2
: t35 code[ tos s0 vmov.f32.cr_,_ s0 d0 vcvt.f64.f32_,_ d0 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0
  dp r0 str_,[_,#_] ]code ;
$3F000000 t35 $00000000 $3FE00000 check2
: t36 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ d0 s0 vcvt.f32.f64_,_ s0 tos vmov.cr.f32_,_ ]code
  ;
$00000000 $3FE00000 t36 $3F000000 check1
: t37 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ 8 d0 vcvt.s32.f64_,#<fbits> d0 tos r0
  vmov.cr.f64_,_,_ r0 tos movs_,_ ]code ;
$00000000 $3FF80000 t37 $00000180 check1
: t38 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ 8 d0 vcvt.s32.f64_,#<fbits> 8 d0
  vcvt.f64.s32_,#<fbits> d0 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $3FF80000 t38 $00000000 $3FF80000 check2
: t39 code[ tos 1 dp ldm 0 7 d0 vmov.f64_,#<imm4h>,#<imm4l> 4 dp subs_,#_ 0 dp tos str_,[_,#_] d0
  tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
0 t39 $00000000 $3FF00000 check2
: t40 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm d0 d0 vpush.f64{_..._} d1 d1
  vpop.f64{_..._} 4 dp subs_,#_ 0 dp tos str_,[_,#_] d1 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp
  r0 str_,[_,#_] ]code ;
$00000000 $40060000 t40 $00000000 $40060000 check2
: t41 code[ r0 1 dp ldm tos r0 d0 vmov.f64.cr_,_,_ tos 1 dp ldm 8 dp subs_,#_ 0 dp d0
  vstr.f64_,[_,#_] 0 dp d1 vldr.f64_,[_,#_] 8 dp adds_,#_ 4 dp subs_,#_ 0 dp tos str_,[_,#_] d1 tos
  r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_] ]code ;
$00000000 $40060000 t41 $00000000 $40060000 check2
: t42 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 vcmp.f64_,_ apsr-nzcv vmrs_,fpscr d1 d0 d2 vselgt.f64_,_,_ 4
  dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_]
  ]code ;
$00000000 $40000000 $00000000 $3FF00000 t42 $00000000 $40000000 check2
: t43 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 vcmp.f64_,_ apsr-nzcv vmrs_,fpscr d1 d0 d2 vselgt.f64_,_,_ 4
  dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_]
  ]code ;
$00000000 $3FF00000 $00000000 $40000000 t43 $00000000 $40000000 check2
: t44 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 vcmp.f64_,_ apsr-nzcv vmrs_,fpscr d1 d0 d2 vseleq.f64_,_,_ 4
  dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_]
  ]code ;
$00000000 $40080000 $00000000 $40080000 t44 $00000000 $40080000 check2
: t45 code[ r0 1 dp ldm tos r0 d1 vmov.f64.cr_,_,_ tos 1 dp ldm r0 1 dp ldm tos r0 d0
  vmov.f64.cr_,_,_ tos 1 dp ldm d1 d0 vcmp.f64_,_ apsr-nzcv vmrs_,fpscr d1 d0 d2 vselge.f64_,_,_ 4
  dp subs_,#_ 0 dp tos str_,[_,#_] d2 tos r0 vmov.cr.f64_,_,_ 4 dp subs_,#_ 0 dp r0 str_,[_,#_]
  ]code ;
$00000000 $3FF00000 $00000000 $40000000 t45 $00000000 $40000000 check2

: report ( -- ) ." armv7m_fp64 run: " tests @ . ." cases, " fails @ . ." failed" cr ;
report
