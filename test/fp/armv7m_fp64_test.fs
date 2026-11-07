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

\ armv7m_fp64_test.fs: the double-precision instructions of src/common/forth/armv7m_fp64.fs against GNU as. Written by make_armv7m_fp64_test.py, which has the cases: change them there.
\ Load armv7m_fp.fs and armv7m_fp64.fs first. This only assembles, so it runs on any target. It ends by saying how many cases failed.

variable start
variable tests
variable fails
0 tests !
0 fails !

: begin-case ( -- ) here start ! ;

\ What was assembled against what as made, and the space given back
: check { h0 h1 -- }
  1 tests +!
  start @ h@ h0 <> start @ 2 + h@ h1 <> or here start @ - 4 <> or if
    1 fails +!
    ." FAIL case " tests @ . ." got " base @ >r hex start @ h@ u. start @ 2 + h@ u. ." want " h0 u. h1 u. r> base ! cr
  then
  start @ here - allot
;

: report ( -- ) ." armv7m_fp64: " tests @ . ." cases, " fails @ . ." failed" cr ;

armv7m-fp import
armv6m::armv6m-instr import
armv7m-fp::armv7m-fp-instr import

begin-case d5 d3 d1 vadd.f64_,_,_ $EE33 $1B05 check \ vadd.f64 d1, d3, d5
begin-case d0 d15 d7 vadd.f64_,_,_ $EE3F $7B00 check \ vadd.f64 d7, d15, d0
begin-case d15 d0 d8 vadd.f64_,_,_ $EE30 $8B0F check \ vadd.f64 d8, d0, d15
begin-case d5 d3 d1 vdiv.f64_,_,_ $EE83 $1B05 check \ vdiv.f64 d1, d3, d5
begin-case d0 d15 d7 vdiv.f64_,_,_ $EE8F $7B00 check \ vdiv.f64 d7, d15, d0
begin-case d15 d0 d8 vdiv.f64_,_,_ $EE80 $8B0F check \ vdiv.f64 d8, d0, d15
begin-case d5 d3 d1 vfma.f64_,_,_ $EEA3 $1B05 check \ vfma.f64 d1, d3, d5
begin-case d0 d15 d7 vfma.f64_,_,_ $EEAF $7B00 check \ vfma.f64 d7, d15, d0
begin-case d15 d0 d8 vfma.f64_,_,_ $EEA0 $8B0F check \ vfma.f64 d8, d0, d15
begin-case d5 d3 d1 vfms.f64_,_,_ $EEA3 $1B45 check \ vfms.f64 d1, d3, d5
begin-case d0 d15 d7 vfms.f64_,_,_ $EEAF $7B40 check \ vfms.f64 d7, d15, d0
begin-case d15 d0 d8 vfms.f64_,_,_ $EEA0 $8B4F check \ vfms.f64 d8, d0, d15
begin-case d5 d3 d1 vfnma.f64_,_,_ $EE93 $1B45 check \ vfnma.f64 d1, d3, d5
begin-case d0 d15 d7 vfnma.f64_,_,_ $EE9F $7B40 check \ vfnma.f64 d7, d15, d0
begin-case d15 d0 d8 vfnma.f64_,_,_ $EE90 $8B4F check \ vfnma.f64 d8, d0, d15
begin-case d5 d3 d1 vfnms.f64_,_,_ $EE93 $1B05 check \ vfnms.f64 d1, d3, d5
begin-case d0 d15 d7 vfnms.f64_,_,_ $EE9F $7B00 check \ vfnms.f64 d7, d15, d0
begin-case d15 d0 d8 vfnms.f64_,_,_ $EE90 $8B0F check \ vfnms.f64 d8, d0, d15
begin-case d5 d3 d1 vmaxnm.f64_,_,_ $FE83 $1B05 check \ vmaxnm.f64 d1, d3, d5
begin-case d0 d15 d7 vmaxnm.f64_,_,_ $FE8F $7B00 check \ vmaxnm.f64 d7, d15, d0
begin-case d15 d0 d8 vmaxnm.f64_,_,_ $FE80 $8B0F check \ vmaxnm.f64 d8, d0, d15
begin-case d5 d3 d1 vminnm.f64_,_,_ $FE83 $1B45 check \ vminnm.f64 d1, d3, d5
begin-case d0 d15 d7 vminnm.f64_,_,_ $FE8F $7B40 check \ vminnm.f64 d7, d15, d0
begin-case d15 d0 d8 vminnm.f64_,_,_ $FE80 $8B4F check \ vminnm.f64 d8, d0, d15
begin-case d5 d3 d1 vmla.f64_,_,_ $EE03 $1B05 check \ vmla.f64 d1, d3, d5
begin-case d0 d15 d7 vmla.f64_,_,_ $EE0F $7B00 check \ vmla.f64 d7, d15, d0
begin-case d15 d0 d8 vmla.f64_,_,_ $EE00 $8B0F check \ vmla.f64 d8, d0, d15
begin-case d5 d3 d1 vmls.f64_,_,_ $EE03 $1B45 check \ vmls.f64 d1, d3, d5
begin-case d0 d15 d7 vmls.f64_,_,_ $EE0F $7B40 check \ vmls.f64 d7, d15, d0
begin-case d15 d0 d8 vmls.f64_,_,_ $EE00 $8B4F check \ vmls.f64 d8, d0, d15
begin-case d5 d3 d1 vmul.f64_,_,_ $EE23 $1B05 check \ vmul.f64 d1, d3, d5
begin-case d0 d15 d7 vmul.f64_,_,_ $EE2F $7B00 check \ vmul.f64 d7, d15, d0
begin-case d15 d0 d8 vmul.f64_,_,_ $EE20 $8B0F check \ vmul.f64 d8, d0, d15
begin-case d5 d3 d1 vnmla.f64_,_,_ $EE13 $1B45 check \ vnmla.f64 d1, d3, d5
begin-case d0 d15 d7 vnmla.f64_,_,_ $EE1F $7B40 check \ vnmla.f64 d7, d15, d0
begin-case d15 d0 d8 vnmla.f64_,_,_ $EE10 $8B4F check \ vnmla.f64 d8, d0, d15
begin-case d5 d3 d1 vnmls.f64_,_,_ $EE13 $1B05 check \ vnmls.f64 d1, d3, d5
begin-case d0 d15 d7 vnmls.f64_,_,_ $EE1F $7B00 check \ vnmls.f64 d7, d15, d0
begin-case d15 d0 d8 vnmls.f64_,_,_ $EE10 $8B0F check \ vnmls.f64 d8, d0, d15
begin-case d5 d3 d1 vnmul.f64_,_,_ $EE23 $1B45 check \ vnmul.f64 d1, d3, d5
begin-case d0 d15 d7 vnmul.f64_,_,_ $EE2F $7B40 check \ vnmul.f64 d7, d15, d0
begin-case d15 d0 d8 vnmul.f64_,_,_ $EE20 $8B4F check \ vnmul.f64 d8, d0, d15
begin-case d5 d3 d1 vseleq.f64_,_,_ $FE03 $1B05 check \ vseleq.f64 d1, d3, d5
begin-case d0 d15 d7 vseleq.f64_,_,_ $FE0F $7B00 check \ vseleq.f64 d7, d15, d0
begin-case d15 d0 d8 vseleq.f64_,_,_ $FE00 $8B0F check \ vseleq.f64 d8, d0, d15
begin-case d5 d3 d1 vselvs.f64_,_,_ $FE13 $1B05 check \ vselvs.f64 d1, d3, d5
begin-case d0 d15 d7 vselvs.f64_,_,_ $FE1F $7B00 check \ vselvs.f64 d7, d15, d0
begin-case d15 d0 d8 vselvs.f64_,_,_ $FE10 $8B0F check \ vselvs.f64 d8, d0, d15
begin-case d5 d3 d1 vselge.f64_,_,_ $FE23 $1B05 check \ vselge.f64 d1, d3, d5
begin-case d0 d15 d7 vselge.f64_,_,_ $FE2F $7B00 check \ vselge.f64 d7, d15, d0
begin-case d15 d0 d8 vselge.f64_,_,_ $FE20 $8B0F check \ vselge.f64 d8, d0, d15
begin-case d5 d3 d1 vselgt.f64_,_,_ $FE33 $1B05 check \ vselgt.f64 d1, d3, d5
begin-case d0 d15 d7 vselgt.f64_,_,_ $FE3F $7B00 check \ vselgt.f64 d7, d15, d0
begin-case d15 d0 d8 vselgt.f64_,_,_ $FE30 $8B0F check \ vselgt.f64 d8, d0, d15
begin-case d5 d3 d1 vsub.f64_,_,_ $EE33 $1B45 check \ vsub.f64 d1, d3, d5
begin-case d0 d15 d7 vsub.f64_,_,_ $EE3F $7B40 check \ vsub.f64 d7, d15, d0
begin-case d15 d0 d8 vsub.f64_,_,_ $EE30 $8B4F check \ vsub.f64 d8, d0, d15
begin-case d5 d1 vabs.f64_,_ $EEB0 $1BC5 check \ vabs.f64 d1, d5
begin-case d15 d0 vabs.f64_,_ $EEB0 $0BCF check \ vabs.f64 d0, d15
begin-case d0 d14 vabs.f64_,_ $EEB0 $EBC0 check \ vabs.f64 d14, d0
begin-case d5 d1 vcmp.f64_,_ $EEB4 $1B45 check \ vcmp.f64 d1, d5
begin-case d15 d0 vcmp.f64_,_ $EEB4 $0B4F check \ vcmp.f64 d0, d15
begin-case d0 d14 vcmp.f64_,_ $EEB4 $EB40 check \ vcmp.f64 d14, d0
begin-case d5 d1 vcmpe.f64_,_ $EEB4 $1BC5 check \ vcmpe.f64 d1, d5
begin-case d15 d0 vcmpe.f64_,_ $EEB4 $0BCF check \ vcmpe.f64 d0, d15
begin-case d0 d14 vcmpe.f64_,_ $EEB4 $EBC0 check \ vcmpe.f64 d14, d0
begin-case d5 d1 vmov.f64_,_ $EEB0 $1B45 check \ vmov.f64 d1, d5
begin-case d15 d0 vmov.f64_,_ $EEB0 $0B4F check \ vmov.f64 d0, d15
begin-case d0 d14 vmov.f64_,_ $EEB0 $EB40 check \ vmov.f64 d14, d0
begin-case d5 d1 vneg.f64_,_ $EEB1 $1B45 check \ vneg.f64 d1, d5
begin-case d15 d0 vneg.f64_,_ $EEB1 $0B4F check \ vneg.f64 d0, d15
begin-case d0 d14 vneg.f64_,_ $EEB1 $EB40 check \ vneg.f64 d14, d0
begin-case d5 d1 vrinta.f64_,_ $FEB8 $1B45 check \ vrinta.f64 d1, d5
begin-case d15 d0 vrinta.f64_,_ $FEB8 $0B4F check \ vrinta.f64 d0, d15
begin-case d0 d14 vrinta.f64_,_ $FEB8 $EB40 check \ vrinta.f64 d14, d0
begin-case d5 d1 vrintn.f64_,_ $FEB9 $1B45 check \ vrintn.f64 d1, d5
begin-case d15 d0 vrintn.f64_,_ $FEB9 $0B4F check \ vrintn.f64 d0, d15
begin-case d0 d14 vrintn.f64_,_ $FEB9 $EB40 check \ vrintn.f64 d14, d0
begin-case d5 d1 vrintp.f64_,_ $FEBA $1B45 check \ vrintp.f64 d1, d5
begin-case d15 d0 vrintp.f64_,_ $FEBA $0B4F check \ vrintp.f64 d0, d15
begin-case d0 d14 vrintp.f64_,_ $FEBA $EB40 check \ vrintp.f64 d14, d0
begin-case d5 d1 vrintm.f64_,_ $FEBB $1B45 check \ vrintm.f64 d1, d5
begin-case d15 d0 vrintm.f64_,_ $FEBB $0B4F check \ vrintm.f64 d0, d15
begin-case d0 d14 vrintm.f64_,_ $FEBB $EB40 check \ vrintm.f64 d14, d0
begin-case d5 d1 vrintx.f64_,_ $EEB7 $1B45 check \ vrintx.f64 d1, d5
begin-case d15 d0 vrintx.f64_,_ $EEB7 $0B4F check \ vrintx.f64 d0, d15
begin-case d0 d14 vrintx.f64_,_ $EEB7 $EB40 check \ vrintx.f64 d14, d0
begin-case d5 d1 vrintz.f64_,_ $EEB6 $1BC5 check \ vrintz.f64 d1, d5
begin-case d15 d0 vrintz.f64_,_ $EEB6 $0BCF check \ vrintz.f64 d0, d15
begin-case d0 d14 vrintz.f64_,_ $EEB6 $EBC0 check \ vrintz.f64 d14, d0
begin-case d5 d1 vrintr.f64_,_ $EEB6 $1B45 check \ vrintr.f64 d1, d5
begin-case d15 d0 vrintr.f64_,_ $EEB6 $0B4F check \ vrintr.f64 d0, d15
begin-case d0 d14 vrintr.f64_,_ $EEB6 $EB40 check \ vrintr.f64 d14, d0
begin-case d5 d1 vsqrt.f64_,_ $EEB1 $1BC5 check \ vsqrt.f64 d1, d5
begin-case d15 d0 vsqrt.f64_,_ $EEB1 $0BCF check \ vsqrt.f64 d0, d15
begin-case d0 d14 vsqrt.f64_,_ $EEB1 $EBC0 check \ vsqrt.f64 d14, d0
begin-case d0 vcmp.f64_,#0.0 $EEB5 $0B40 check \ vcmp.f64 d0, #0.0
begin-case d7 vcmp.f64_,#0.0 $EEB5 $7B40 check \ vcmp.f64 d7, #0.0
begin-case d15 vcmp.f64_,#0.0 $EEB5 $FB40 check \ vcmp.f64 d15, #0.0
begin-case d0 vcmpe.f64_,#0.0 $EEB5 $0BC0 check \ vcmpe.f64 d0, #0.0
begin-case d7 vcmpe.f64_,#0.0 $EEB5 $7BC0 check \ vcmpe.f64 d7, #0.0
begin-case d15 vcmpe.f64_,#0.0 $EEB5 $FBC0 check \ vcmpe.f64 d15, #0.0
begin-case d5 s3 vcvta.u32.f64_,_ $FEFC $1B45 check \ vcvta.u32.f64 s3, d5
begin-case d15 s30 vcvta.u32.f64_,_ $FEBC $FB4F check \ vcvta.u32.f64 s30, d15
begin-case d0 s1 vcvta.u32.f64_,_ $FEFC $0B40 check \ vcvta.u32.f64 s1, d0
begin-case d8 s16 vcvta.u32.f64_,_ $FEBC $8B48 check \ vcvta.u32.f64 s16, d8
begin-case d5 s3 vcvta.s32.f64_,_ $FEFC $1BC5 check \ vcvta.s32.f64 s3, d5
begin-case d15 s30 vcvta.s32.f64_,_ $FEBC $FBCF check \ vcvta.s32.f64 s30, d15
begin-case d0 s1 vcvta.s32.f64_,_ $FEFC $0BC0 check \ vcvta.s32.f64 s1, d0
begin-case d8 s16 vcvta.s32.f64_,_ $FEBC $8BC8 check \ vcvta.s32.f64 s16, d8
begin-case d5 s3 vcvtn.u32.f64_,_ $FEFD $1B45 check \ vcvtn.u32.f64 s3, d5
begin-case d15 s30 vcvtn.u32.f64_,_ $FEBD $FB4F check \ vcvtn.u32.f64 s30, d15
begin-case d0 s1 vcvtn.u32.f64_,_ $FEFD $0B40 check \ vcvtn.u32.f64 s1, d0
begin-case d8 s16 vcvtn.u32.f64_,_ $FEBD $8B48 check \ vcvtn.u32.f64 s16, d8
begin-case d5 s3 vcvtn.s32.f64_,_ $FEFD $1BC5 check \ vcvtn.s32.f64 s3, d5
begin-case d15 s30 vcvtn.s32.f64_,_ $FEBD $FBCF check \ vcvtn.s32.f64 s30, d15
begin-case d0 s1 vcvtn.s32.f64_,_ $FEFD $0BC0 check \ vcvtn.s32.f64 s1, d0
begin-case d8 s16 vcvtn.s32.f64_,_ $FEBD $8BC8 check \ vcvtn.s32.f64 s16, d8
begin-case d5 s3 vcvtp.u32.f64_,_ $FEFE $1B45 check \ vcvtp.u32.f64 s3, d5
begin-case d15 s30 vcvtp.u32.f64_,_ $FEBE $FB4F check \ vcvtp.u32.f64 s30, d15
begin-case d0 s1 vcvtp.u32.f64_,_ $FEFE $0B40 check \ vcvtp.u32.f64 s1, d0
begin-case d8 s16 vcvtp.u32.f64_,_ $FEBE $8B48 check \ vcvtp.u32.f64 s16, d8
begin-case d5 s3 vcvtp.s32.f64_,_ $FEFE $1BC5 check \ vcvtp.s32.f64 s3, d5
begin-case d15 s30 vcvtp.s32.f64_,_ $FEBE $FBCF check \ vcvtp.s32.f64 s30, d15
begin-case d0 s1 vcvtp.s32.f64_,_ $FEFE $0BC0 check \ vcvtp.s32.f64 s1, d0
begin-case d8 s16 vcvtp.s32.f64_,_ $FEBE $8BC8 check \ vcvtp.s32.f64 s16, d8
begin-case d5 s3 vcvtm.u32.f64_,_ $FEFF $1B45 check \ vcvtm.u32.f64 s3, d5
begin-case d15 s30 vcvtm.u32.f64_,_ $FEBF $FB4F check \ vcvtm.u32.f64 s30, d15
begin-case d0 s1 vcvtm.u32.f64_,_ $FEFF $0B40 check \ vcvtm.u32.f64 s1, d0
begin-case d8 s16 vcvtm.u32.f64_,_ $FEBF $8B48 check \ vcvtm.u32.f64 s16, d8
begin-case d5 s3 vcvtm.s32.f64_,_ $FEFF $1BC5 check \ vcvtm.s32.f64 s3, d5
begin-case d15 s30 vcvtm.s32.f64_,_ $FEBF $FBCF check \ vcvtm.s32.f64 s30, d15
begin-case d0 s1 vcvtm.s32.f64_,_ $FEFF $0BC0 check \ vcvtm.s32.f64 s1, d0
begin-case d8 s16 vcvtm.s32.f64_,_ $FEBF $8BC8 check \ vcvtm.s32.f64 s16, d8
begin-case d5 s3 vcvt.u32.f64_,_ $EEFC $1BC5 check \ vcvt.u32.f64 s3, d5
begin-case d15 s30 vcvt.u32.f64_,_ $EEBC $FBCF check \ vcvt.u32.f64 s30, d15
begin-case d0 s1 vcvt.u32.f64_,_ $EEFC $0BC0 check \ vcvt.u32.f64 s1, d0
begin-case d8 s16 vcvt.u32.f64_,_ $EEBC $8BC8 check \ vcvt.u32.f64 s16, d8
begin-case d5 s3 vcvt.s32.f64_,_ $EEFD $1BC5 check \ vcvt.s32.f64 s3, d5
begin-case d15 s30 vcvt.s32.f64_,_ $EEBD $FBCF check \ vcvt.s32.f64 s30, d15
begin-case d0 s1 vcvt.s32.f64_,_ $EEFD $0BC0 check \ vcvt.s32.f64 s1, d0
begin-case d8 s16 vcvt.s32.f64_,_ $EEBD $8BC8 check \ vcvt.s32.f64 s16, d8
begin-case d5 s3 vcvtr.u32.f64_,_ $EEFC $1B45 check \ vcvtr.u32.f64 s3, d5
begin-case d15 s30 vcvtr.u32.f64_,_ $EEBC $FB4F check \ vcvtr.u32.f64 s30, d15
begin-case d0 s1 vcvtr.u32.f64_,_ $EEFC $0B40 check \ vcvtr.u32.f64 s1, d0
begin-case d8 s16 vcvtr.u32.f64_,_ $EEBC $8B48 check \ vcvtr.u32.f64 s16, d8
begin-case d5 s3 vcvtr.s32.f64_,_ $EEFD $1B45 check \ vcvtr.s32.f64 s3, d5
begin-case d15 s30 vcvtr.s32.f64_,_ $EEBD $FB4F check \ vcvtr.s32.f64 s30, d15
begin-case d0 s1 vcvtr.s32.f64_,_ $EEFD $0B40 check \ vcvtr.s32.f64 s1, d0
begin-case d8 s16 vcvtr.s32.f64_,_ $EEBD $8B48 check \ vcvtr.s32.f64 s16, d8
begin-case d5 s3 vcvt.f32.f64_,_ $EEF7 $1BC5 check \ vcvt.f32.f64 s3, d5
begin-case d15 s30 vcvt.f32.f64_,_ $EEB7 $FBCF check \ vcvt.f32.f64 s30, d15
begin-case d0 s1 vcvt.f32.f64_,_ $EEF7 $0BC0 check \ vcvt.f32.f64 s1, d0
begin-case d8 s16 vcvt.f32.f64_,_ $EEB7 $8BC8 check \ vcvt.f32.f64 s16, d8
begin-case d5 s3 vcvtb.f16.f64_,_ $EEF3 $1B45 check \ vcvtb.f16.f64 s3, d5
begin-case d15 s30 vcvtb.f16.f64_,_ $EEB3 $FB4F check \ vcvtb.f16.f64 s30, d15
begin-case d0 s1 vcvtb.f16.f64_,_ $EEF3 $0B40 check \ vcvtb.f16.f64 s1, d0
begin-case d8 s16 vcvtb.f16.f64_,_ $EEB3 $8B48 check \ vcvtb.f16.f64 s16, d8
begin-case d5 s3 vcvtt.f16.f64_,_ $EEF3 $1BC5 check \ vcvtt.f16.f64 s3, d5
begin-case d15 s30 vcvtt.f16.f64_,_ $EEB3 $FBCF check \ vcvtt.f16.f64 s30, d15
begin-case d0 s1 vcvtt.f16.f64_,_ $EEF3 $0BC0 check \ vcvtt.f16.f64 s1, d0
begin-case d8 s16 vcvtt.f16.f64_,_ $EEB3 $8BC8 check \ vcvtt.f16.f64 s16, d8
begin-case s3 d5 vcvt.f64.u32_,_ $EEB8 $5B61 check \ vcvt.f64.u32 d5, s3
begin-case s30 d15 vcvt.f64.u32_,_ $EEB8 $FB4F check \ vcvt.f64.u32 d15, s30
begin-case s1 d0 vcvt.f64.u32_,_ $EEB8 $0B60 check \ vcvt.f64.u32 d0, s1
begin-case s16 d8 vcvt.f64.u32_,_ $EEB8 $8B48 check \ vcvt.f64.u32 d8, s16
begin-case s3 d5 vcvt.f64.s32_,_ $EEB8 $5BE1 check \ vcvt.f64.s32 d5, s3
begin-case s30 d15 vcvt.f64.s32_,_ $EEB8 $FBCF check \ vcvt.f64.s32 d15, s30
begin-case s1 d0 vcvt.f64.s32_,_ $EEB8 $0BE0 check \ vcvt.f64.s32 d0, s1
begin-case s16 d8 vcvt.f64.s32_,_ $EEB8 $8BC8 check \ vcvt.f64.s32 d8, s16
begin-case s3 d5 vcvt.f64.f32_,_ $EEB7 $5AE1 check \ vcvt.f64.f32 d5, s3
begin-case s30 d15 vcvt.f64.f32_,_ $EEB7 $FACF check \ vcvt.f64.f32 d15, s30
begin-case s1 d0 vcvt.f64.f32_,_ $EEB7 $0AE0 check \ vcvt.f64.f32 d0, s1
begin-case s16 d8 vcvt.f64.f32_,_ $EEB7 $8AC8 check \ vcvt.f64.f32 d8, s16
begin-case s3 d5 vcvtb.f64.f16_,_ $EEB2 $5B61 check \ vcvtb.f64.f16 d5, s3
begin-case s30 d15 vcvtb.f64.f16_,_ $EEB2 $FB4F check \ vcvtb.f64.f16 d15, s30
begin-case s1 d0 vcvtb.f64.f16_,_ $EEB2 $0B60 check \ vcvtb.f64.f16 d0, s1
begin-case s16 d8 vcvtb.f64.f16_,_ $EEB2 $8B48 check \ vcvtb.f64.f16 d8, s16
begin-case s3 d5 vcvtt.f64.f16_,_ $EEB2 $5BE1 check \ vcvtt.f64.f16 d5, s3
begin-case s30 d15 vcvtt.f64.f16_,_ $EEB2 $FBCF check \ vcvtt.f64.f16 d15, s30
begin-case s1 d0 vcvtt.f64.f16_,_ $EEB2 $0BE0 check \ vcvtt.f64.f16 d0, s1
begin-case s16 d8 vcvtt.f64.f16_,_ $EEB2 $8BC8 check \ vcvtt.f64.f16 d8, s16
begin-case 0 d3 vcvt.u16.f64_,#<fbits> $EEBF $3B48 check \ vcvt.u16.f64 d3, d3, #0
begin-case 0 d15 vcvt.u16.f64_,#<fbits> $EEBF $FB48 check \ vcvt.u16.f64 d15, d15, #0
begin-case 1 d3 vcvt.u16.f64_,#<fbits> $EEBF $3B67 check \ vcvt.u16.f64 d3, d3, #1
begin-case 1 d15 vcvt.u16.f64_,#<fbits> $EEBF $FB67 check \ vcvt.u16.f64 d15, d15, #1
begin-case 8 d3 vcvt.u16.f64_,#<fbits> $EEBF $3B44 check \ vcvt.u16.f64 d3, d3, #8
begin-case 8 d15 vcvt.u16.f64_,#<fbits> $EEBF $FB44 check \ vcvt.u16.f64 d15, d15, #8
begin-case 16 d3 vcvt.u16.f64_,#<fbits> $EEBF $3B40 check \ vcvt.u16.f64 d3, d3, #16
begin-case 16 d15 vcvt.u16.f64_,#<fbits> $EEBF $FB40 check \ vcvt.u16.f64 d15, d15, #16
begin-case 0 d3 vcvt.f64.u16_,#<fbits> $EEBB $3B48 check \ vcvt.f64.u16 d3, d3, #0
begin-case 0 d15 vcvt.f64.u16_,#<fbits> $EEBB $FB48 check \ vcvt.f64.u16 d15, d15, #0
begin-case 1 d3 vcvt.f64.u16_,#<fbits> $EEBB $3B67 check \ vcvt.f64.u16 d3, d3, #1
begin-case 1 d15 vcvt.f64.u16_,#<fbits> $EEBB $FB67 check \ vcvt.f64.u16 d15, d15, #1
begin-case 8 d3 vcvt.f64.u16_,#<fbits> $EEBB $3B44 check \ vcvt.f64.u16 d3, d3, #8
begin-case 8 d15 vcvt.f64.u16_,#<fbits> $EEBB $FB44 check \ vcvt.f64.u16 d15, d15, #8
begin-case 16 d3 vcvt.f64.u16_,#<fbits> $EEBB $3B40 check \ vcvt.f64.u16 d3, d3, #16
begin-case 16 d15 vcvt.f64.u16_,#<fbits> $EEBB $FB40 check \ vcvt.f64.u16 d15, d15, #16
begin-case 0 d3 vcvt.s16.f64_,#<fbits> $EEBE $3B48 check \ vcvt.s16.f64 d3, d3, #0
begin-case 0 d15 vcvt.s16.f64_,#<fbits> $EEBE $FB48 check \ vcvt.s16.f64 d15, d15, #0
begin-case 1 d3 vcvt.s16.f64_,#<fbits> $EEBE $3B67 check \ vcvt.s16.f64 d3, d3, #1
begin-case 1 d15 vcvt.s16.f64_,#<fbits> $EEBE $FB67 check \ vcvt.s16.f64 d15, d15, #1
begin-case 8 d3 vcvt.s16.f64_,#<fbits> $EEBE $3B44 check \ vcvt.s16.f64 d3, d3, #8
begin-case 8 d15 vcvt.s16.f64_,#<fbits> $EEBE $FB44 check \ vcvt.s16.f64 d15, d15, #8
begin-case 16 d3 vcvt.s16.f64_,#<fbits> $EEBE $3B40 check \ vcvt.s16.f64 d3, d3, #16
begin-case 16 d15 vcvt.s16.f64_,#<fbits> $EEBE $FB40 check \ vcvt.s16.f64 d15, d15, #16
begin-case 0 d3 vcvt.f64.s16_,#<fbits> $EEBA $3B48 check \ vcvt.f64.s16 d3, d3, #0
begin-case 0 d15 vcvt.f64.s16_,#<fbits> $EEBA $FB48 check \ vcvt.f64.s16 d15, d15, #0
begin-case 1 d3 vcvt.f64.s16_,#<fbits> $EEBA $3B67 check \ vcvt.f64.s16 d3, d3, #1
begin-case 1 d15 vcvt.f64.s16_,#<fbits> $EEBA $FB67 check \ vcvt.f64.s16 d15, d15, #1
begin-case 8 d3 vcvt.f64.s16_,#<fbits> $EEBA $3B44 check \ vcvt.f64.s16 d3, d3, #8
begin-case 8 d15 vcvt.f64.s16_,#<fbits> $EEBA $FB44 check \ vcvt.f64.s16 d15, d15, #8
begin-case 16 d3 vcvt.f64.s16_,#<fbits> $EEBA $3B40 check \ vcvt.f64.s16 d3, d3, #16
begin-case 16 d15 vcvt.f64.s16_,#<fbits> $EEBA $FB40 check \ vcvt.f64.s16 d15, d15, #16
begin-case 1 d3 vcvt.u32.f64_,#<fbits> $EEBF $3BEF check \ vcvt.u32.f64 d3, d3, #1
begin-case 1 d15 vcvt.u32.f64_,#<fbits> $EEBF $FBEF check \ vcvt.u32.f64 d15, d15, #1
begin-case 8 d3 vcvt.u32.f64_,#<fbits> $EEBF $3BCC check \ vcvt.u32.f64 d3, d3, #8
begin-case 8 d15 vcvt.u32.f64_,#<fbits> $EEBF $FBCC check \ vcvt.u32.f64 d15, d15, #8
begin-case 32 d3 vcvt.u32.f64_,#<fbits> $EEBF $3BC0 check \ vcvt.u32.f64 d3, d3, #32
begin-case 32 d15 vcvt.u32.f64_,#<fbits> $EEBF $FBC0 check \ vcvt.u32.f64 d15, d15, #32
begin-case 1 d3 vcvt.f64.u32_,#<fbits> $EEBB $3BEF check \ vcvt.f64.u32 d3, d3, #1
begin-case 1 d15 vcvt.f64.u32_,#<fbits> $EEBB $FBEF check \ vcvt.f64.u32 d15, d15, #1
begin-case 8 d3 vcvt.f64.u32_,#<fbits> $EEBB $3BCC check \ vcvt.f64.u32 d3, d3, #8
begin-case 8 d15 vcvt.f64.u32_,#<fbits> $EEBB $FBCC check \ vcvt.f64.u32 d15, d15, #8
begin-case 32 d3 vcvt.f64.u32_,#<fbits> $EEBB $3BC0 check \ vcvt.f64.u32 d3, d3, #32
begin-case 32 d15 vcvt.f64.u32_,#<fbits> $EEBB $FBC0 check \ vcvt.f64.u32 d15, d15, #32
begin-case 1 d3 vcvt.s32.f64_,#<fbits> $EEBE $3BEF check \ vcvt.s32.f64 d3, d3, #1
begin-case 1 d15 vcvt.s32.f64_,#<fbits> $EEBE $FBEF check \ vcvt.s32.f64 d15, d15, #1
begin-case 8 d3 vcvt.s32.f64_,#<fbits> $EEBE $3BCC check \ vcvt.s32.f64 d3, d3, #8
begin-case 8 d15 vcvt.s32.f64_,#<fbits> $EEBE $FBCC check \ vcvt.s32.f64 d15, d15, #8
begin-case 32 d3 vcvt.s32.f64_,#<fbits> $EEBE $3BC0 check \ vcvt.s32.f64 d3, d3, #32
begin-case 32 d15 vcvt.s32.f64_,#<fbits> $EEBE $FBC0 check \ vcvt.s32.f64 d15, d15, #32
begin-case 1 d3 vcvt.f64.s32_,#<fbits> $EEBA $3BEF check \ vcvt.f64.s32 d3, d3, #1
begin-case 1 d15 vcvt.f64.s32_,#<fbits> $EEBA $FBEF check \ vcvt.f64.s32 d15, d15, #1
begin-case 8 d3 vcvt.f64.s32_,#<fbits> $EEBA $3BCC check \ vcvt.f64.s32 d3, d3, #8
begin-case 8 d15 vcvt.f64.s32_,#<fbits> $EEBA $FBCC check \ vcvt.f64.s32 d15, d15, #8
begin-case 32 d3 vcvt.f64.s32_,#<fbits> $EEBA $3BC0 check \ vcvt.f64.s32 d3, d3, #32
begin-case 32 d15 vcvt.f64.s32_,#<fbits> $EEBA $FBC0 check \ vcvt.f64.s32 d15, d15, #32
begin-case 0 r2 d3 vldr.f64_,[_,#_] $ED92 $3B00 check \ vldr d3, [r2, #0]
begin-case 8 r2 d3 vldr.f64_,[_,#_] $ED92 $3B02 check \ vldr d3, [r2, #8]
begin-case -8 r7 d15 vldr.f64_,[_,#_] $ED17 $FB02 check \ vldr d15, [r7, #-8]
begin-case 1020 r0 d0 vldr.f64_,[_,#_] $ED90 $0BFF check \ vldr d0, [r0, #1020]
begin-case -1020 r5 d1 vldr.f64_,[_,#_] $ED15 $1BFF check \ vldr d1, [r5, #-1020]
begin-case 0 r2 d3 vstr.f64_,[_,#_] $ED82 $3B00 check \ vstr d3, [r2, #0]
begin-case 8 r2 d3 vstr.f64_,[_,#_] $ED82 $3B02 check \ vstr d3, [r2, #8]
begin-case -8 r7 d15 vstr.f64_,[_,#_] $ED07 $FB02 check \ vstr d15, [r7, #-8]
begin-case 1020 r0 d0 vstr.f64_,[_,#_] $ED80 $0BFF check \ vstr d0, [r0, #1020]
begin-case -1020 r5 d1 vstr.f64_,[_,#_] $ED05 $1BFF check \ vstr d1, [r5, #-1020]
begin-case d5 d3 r2 vldmia.f64_,{_..._} $EC92 $3B06 check \ vldmia r2, {d3-d5}
begin-case d15 d0 r7 vldmia.f64_,{_..._} $EC97 $0B20 check \ vldmia r7, {d0-d15}
begin-case d15 d15 r0 vldmia.f64_,{_..._} $EC90 $FB02 check \ vldmia r0, {d15}
begin-case d9 d8 r4 vldmia.f64_,{_..._} $EC94 $8B04 check \ vldmia r4, {d8-d9}
begin-case d5 d3 r2 vldmia!.f64_,{_..._} $ECB2 $3B06 check \ vldmia r2!, {d3-d5}
begin-case d15 d0 r7 vldmia!.f64_,{_..._} $ECB7 $0B20 check \ vldmia r7!, {d0-d15}
begin-case d15 d15 r0 vldmia!.f64_,{_..._} $ECB0 $FB02 check \ vldmia r0!, {d15}
begin-case d9 d8 r4 vldmia!.f64_,{_..._} $ECB4 $8B04 check \ vldmia r4!, {d8-d9}
begin-case d5 d3 r2 vldmdb!.f64_,{_..._} $ED32 $3B06 check \ vldmdb r2!, {d3-d5}
begin-case d15 d0 r7 vldmdb!.f64_,{_..._} $ED37 $0B20 check \ vldmdb r7!, {d0-d15}
begin-case d15 d15 r0 vldmdb!.f64_,{_..._} $ED30 $FB02 check \ vldmdb r0!, {d15}
begin-case d9 d8 r4 vldmdb!.f64_,{_..._} $ED34 $8B04 check \ vldmdb r4!, {d8-d9}
begin-case d5 d3 r2 vstmia.f64_,{_..._} $EC82 $3B06 check \ vstmia r2, {d3-d5}
begin-case d15 d0 r7 vstmia.f64_,{_..._} $EC87 $0B20 check \ vstmia r7, {d0-d15}
begin-case d15 d15 r0 vstmia.f64_,{_..._} $EC80 $FB02 check \ vstmia r0, {d15}
begin-case d9 d8 r4 vstmia.f64_,{_..._} $EC84 $8B04 check \ vstmia r4, {d8-d9}
begin-case d5 d3 r2 vstmia!.f64_,{_..._} $ECA2 $3B06 check \ vstmia r2!, {d3-d5}
begin-case d15 d0 r7 vstmia!.f64_,{_..._} $ECA7 $0B20 check \ vstmia r7!, {d0-d15}
begin-case d15 d15 r0 vstmia!.f64_,{_..._} $ECA0 $FB02 check \ vstmia r0!, {d15}
begin-case d9 d8 r4 vstmia!.f64_,{_..._} $ECA4 $8B04 check \ vstmia r4!, {d8-d9}
begin-case d5 d3 r2 vstmdb!.f64_,{_..._} $ED22 $3B06 check \ vstmdb r2!, {d3-d5}
begin-case d15 d0 r7 vstmdb!.f64_,{_..._} $ED27 $0B20 check \ vstmdb r7!, {d0-d15}
begin-case d15 d15 r0 vstmdb!.f64_,{_..._} $ED20 $FB02 check \ vstmdb r0!, {d15}
begin-case d9 d8 r4 vstmdb!.f64_,{_..._} $ED24 $8B04 check \ vstmdb r4!, {d8-d9}
begin-case 0 7 d3 vmov.f64_,#<imm4h>,#<imm4l> $EEB7 $3B00 check \ vmov.f64 d3, #1.0
begin-case 0 0 d0 vmov.f64_,#<imm4h>,#<imm4l> $EEB0 $0B00 check \ vmov.f64 d0, #2.0
begin-case 0 8 d15 vmov.f64_,#<imm4h>,#<imm4l> $EEB8 $FB00 check \ vmov.f64 d15, #-2.0
begin-case 0 6 d5 vmov.f64_,#<imm4h>,#<imm4l> $EEB6 $5B00 check \ vmov.f64 d5, #0.5
begin-case 4 2 d7 vmov.f64_,#<imm4h>,#<imm4l> $EEB2 $7B04 check \ vmov.f64 d7, #10.0
begin-case d3 r2 r1 vmov.cr.f64_,_,_ $EC52 $1B13 check \ vmov r1, r2, d3
begin-case d15 r7 r0 vmov.cr.f64_,_,_ $EC57 $0B1F check \ vmov r0, r7, d15
begin-case d0 r5 r6 vmov.cr.f64_,_,_ $EC55 $6B10 check \ vmov r6, r5, d0
begin-case r2 r1 d3 vmov.f64.cr_,_,_ $EC42 $1B13 check \ vmov d3, r1, r2
begin-case r7 r0 d15 vmov.f64.cr_,_,_ $EC47 $0B1F check \ vmov d15, r0, r7
begin-case r5 r6 d0 vmov.f64.cr_,_,_ $EC45 $6B10 check \ vmov d0, r6, r5
begin-case d5 d3 vpop.f64{_..._} $ECBD $3B06 check \ vpop {d3-d5}
begin-case d15 d0 vpop.f64{_..._} $ECBD $0B20 check \ vpop {d0-d15}
begin-case d8 d8 vpop.f64{_..._} $ECBD $8B02 check \ vpop {d8}
begin-case d5 d3 vpush.f64{_..._} $ED2D $3B06 check \ vpush {d3-d5}
begin-case d15 d0 vpush.f64{_..._} $ED2D $0B20 check \ vpush {d0-d15}
begin-case d8 d8 vpush.f64{_..._} $ED2D $8B02 check \ vpush {d8}

report
