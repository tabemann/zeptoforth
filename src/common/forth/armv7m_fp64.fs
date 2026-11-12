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

\ Double-precision instructions for the assembler in armv7m_fp.fs, which must be
\ loaded first. These are for a double-precision FPU (FPv5-D16, as on a Cortex-M7
\ with one), so this file is separate, and the targets without one need not pay
\ for it. Operands come in the same order as armv7m_fp.fs's single-precision
\ instructions, and the register words d0 to d15 join s0 to s31 inside code[ ...
\ ]code. Each instruction means what GNU as means by that name, which
\ test/fp/armv7m_fp64_test.fs checks.

compile-to-flash

continue-module armv7m-fp

  internal import

  continue-module armv7m-fp-internal

    \ Validate a single-precision register
    : validate-sreg ( reg -- ) 32 u< averts x-out-of-range-4reg ;

    \ Validate a double-precision register range
    : validate-dreg-range ( count reg -- )
      dup validate-4reg
      over 0> averts x-out-of-range-reg-range
      + 16 u<= averts x-out-of-range-reg-range
    ;

    compress-flash

    \ 32-bit double-precision register load/store 4-byte-immediate instruction
    : instr-dr-load/store-imm ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { imm rn vd instr }
      vd rn validate-2-4reg
      imm validate-imm-4align
      imm abs 2 rshift 256 < averts x-out-of-range-imm
      instr $FFFF and imm 0>= 1 and 7 lshift or rn or h,
      instr 16 rshift vd 12 lshift or imm abs 2 rshift or h,
    ;

    \ 32-bit double-precision register load/store multiple instruction
    : instr-dr-load/store-multi ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vd-end vd rn instr }
      vd rn validate-2-4reg
      vd-end vd - 1+ { count }
      count vd validate-dreg-range
      instr $FFFF and rn or h,
      instr 16 rshift vd 12 lshift or count 2 * or h,
    ;

    \ 32-bit one double-precision register instruction
    : instr-dr ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vd instr }
      vd validate-4reg
      instr $FFFF and h,
      instr 16 rshift vd 12 lshift or h,
    ;

    \ 32-bit one double-precision register instruction with immediate constant
    : instr-dr-imm ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { imm4l imm4h vd instr }
      vd validate-4reg
      imm4h 16 u< averts x-out-of-range-imm
      imm4l 16 u< averts x-out-of-range-imm
      instr $FFFF and imm4h or h,
      instr 16 rshift vd 12 lshift or imm4l or h,
    ;

    \ 32-bit one double-precision register instruction with a number of fraction
    \ bits, 1 to size, or 0 to size if size is 16
    : instr-dr-fract ( h0 h1 size "name" -- )
      <builds , 16 lshift or , does> dup @ swap cell+ @ { fbits vd size instr }
      vd validate-4reg
      fbits size u<= averts x-out-of-range-imm
      size 32 = fbits 0= and triggers x-out-of-range-imm
      size fbits - { imm }
      instr $FFFF and h,
      instr 16 rshift vd 12 lshift or
      imm 1 and 5 lshift or imm 1 rshift or h,
    ;

    \ 32-bit two double-precision registers instruction
    : instr-2*dr ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vm vd instr }
      vm vd validate-2-4reg
      instr $FFFF and h,
      instr 16 rshift vd 12 lshift or vm or h,
    ;

    \ 32-bit three double-precision registers instruction
    : instr-3*dr ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vm vn vd instr }
      vm vn vd validate-3-4reg
      instr $FFFF and vn or h,
      instr 16 rshift vd 12 lshift or vm or h,
    ;

    \ 32-bit double-precision source and single-precision destination instruction
    : instr-dr>sr ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vm vd instr }
      vm validate-4reg
      vd validate-sreg
      instr $FFFF and vd 1 and 6 lshift or h,
      instr 16 rshift vd 1 rshift 12 lshift or vm or h,
    ;

    \ 32-bit single-precision source and double-precision destination instruction
    : instr-sr>dr ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vm vd instr }
      vm validate-sreg
      vd validate-4reg
      instr $FFFF and h,
      instr 16 rshift vd 12 lshift or
      vm 1 and 5 lshift or vm 1 rshift or h,
    ;

    \ 32-bit transfer between two core registers and a double-precision register
    : instr-2*cr-dr ( h0 h1 reverse "name" -- )
      <builds , 16 lshift or , does> dup @ swap cell+ @ { r2 r1 r0 rev instr }
      rev if r1 r0 r2 else r2 r1 r0 then { rt2 rt vm }
      rt2 rt vm validate-3-4reg
      instr $FFFF and rt2 or h,
      instr 16 rshift rt 12 lshift or vm or h,
    ;

    end-compress-flash

  end-module> import

  continue-module armv7m-fp-instr

    armv6m::armv6m-instr import

    compress-flash

    \ Registers
    0 constant d0
    1 constant d1
    2 constant d2
    3 constant d3
    4 constant d4
    5 constant d5
    6 constant d6
    7 constant d7
    8 constant d8
    9 constant d9
    10 constant d10
    11 constant d11
    12 constant d12
    13 constant d13
    14 constant d14
    15 constant d15

    \ Double-precision floating-point absolute value
    $EEB0 $0BC0 instr-2*dr vabs.f64_,_

    \ Double-precision floating-point addition
    $EE30 $0B00 instr-3*dr vadd.f64_,_,_

    \ Double-precision floating-point compare
    $EEB4 $0B40 instr-2*dr vcmp.f64_,_

    \ Double-precision floating-point compare with exception on quiet NaN
    $EEB4 $0BC0 instr-2*dr vcmpe.f64_,_

    \ Double-precision floating-point compare with zero
    $EEB5 $0B40 instr-dr vcmp.f64_,#0.0

    \ Double-precision floating-point compare with zero with exception on quiet
    \ NaN
    $EEB5 $0BC0 instr-dr vcmpe.f64_,#0.0

    \ Convert double-precision floating point to unsigned integer rounding to
    \ nearest with ties to away
    $FEBC $0B40 instr-dr>sr vcvta.u32.f64_,_

    \ Convert double-precision floating point to signed integer rounding to
    \ nearest with ties to away
    $FEBC $0BC0 instr-dr>sr vcvta.s32.f64_,_

    \ Convert double-precision floating point to unsigned integer rounding to
    \ nearest with ties to even
    $FEBD $0B40 instr-dr>sr vcvtn.u32.f64_,_

    \ Convert double-precision floating point to signed integer rounding to
    \ nearest with ties to even
    $FEBD $0BC0 instr-dr>sr vcvtn.s32.f64_,_

    \ Convert double-precision floating point to unsigned integer rounding
    \ towards +Infinity
    $FEBE $0B40 instr-dr>sr vcvtp.u32.f64_,_

    \ Convert double-precision floating point to signed integer rounding towards
    \ +Infinity
    $FEBE $0BC0 instr-dr>sr vcvtp.s32.f64_,_

    \ Convert double-precision floating point to unsigned integer rounding
    \ towards -Infinity
    $FEBF $0B40 instr-dr>sr vcvtm.u32.f64_,_

    \ Convert double-precision floating point to signed integer rounding towards
    \ -Infinity
    $FEBF $0BC0 instr-dr>sr vcvtm.s32.f64_,_

    \ Convert double-precision floating point to unsigned integer rounding
    \ towards zero
    $EEBC $0BC0 instr-dr>sr vcvt.u32.f64_,_

    \ Convert double-precision floating point to signed integer rounding towards
    \ zero
    $EEBD $0BC0 instr-dr>sr vcvt.s32.f64_,_

    \ Convert double-precision floating point to unsigned integer using the
    \ rounding mode in FPSCR
    $EEBC $0B40 instr-dr>sr vcvtr.u32.f64_,_

    \ Convert double-precision floating point to signed integer using the
    \ rounding mode in FPSCR
    $EEBD $0B40 instr-dr>sr vcvtr.s32.f64_,_

    \ Convert unsigned integer to double-precision floating point
    $EEB8 $0B40 instr-sr>dr vcvt.f64.u32_,_

    \ Convert signed integer to double-precision floating point
    $EEB8 $0BC0 instr-sr>dr vcvt.f64.s32_,_

    \ Convert single-precision floating point to double-precision
    $EEB7 $0AC0 instr-sr>dr vcvt.f64.f32_,_

    \ Convert double-precision floating point to single-precision
    $EEB7 $0BC0 instr-dr>sr vcvt.f32.f64_,_

    \ Convert double-precision floating point to 16-bit unsigned fixed point
    \ rounding towards zero
    $EEBF $0B40 16 instr-dr-fract vcvt.u16.f64_,#<fbits>

    \ Convert double-precision floating point to 16-bit signed fixed point
    \ rounding towards zero
    $EEBE $0B40 16 instr-dr-fract vcvt.s16.f64_,#<fbits>

    \ Convert double-precision floating point to 32-bit unsigned fixed point
    \ rounding towards zero
    $EEBF $0BC0 32 instr-dr-fract vcvt.u32.f64_,#<fbits>

    \ Convert double-precision floating point to 32-bit signed fixed point
    \ rounding towards zero
    $EEBE $0BC0 32 instr-dr-fract vcvt.s32.f64_,#<fbits>

    \ Convert 16-bit unsigned fixed point to double-precision floating point
    $EEBB $0B40 16 instr-dr-fract vcvt.f64.u16_,#<fbits>

    \ Convert 16-bit signed fixed point to double-precision floating point
    $EEBA $0B40 16 instr-dr-fract vcvt.f64.s16_,#<fbits>

    \ Convert 32-bit unsigned fixed point to double-precision floating point
    $EEBB $0BC0 32 instr-dr-fract vcvt.f64.u32_,#<fbits>

    \ Convert 32-bit signed fixed point to double-precision floating point
    $EEBA $0BC0 32 instr-dr-fract vcvt.f64.s32_,#<fbits>

    \ Convert the half-precision value in the bottom half of a single-precision
    \ register to double-precision
    $EEB2 $0B40 instr-sr>dr vcvtb.f64.f16_,_

    \ Convert the half-precision value in the top half of a single-precision
    \ register to double-precision
    $EEB2 $0BC0 instr-sr>dr vcvtt.f64.f16_,_

    \ Convert a double-precision value to half-precision in the bottom half of a
    \ single-precision register, preserving its other half
    $EEB3 $0B40 instr-dr>sr vcvtb.f16.f64_,_

    \ Convert a double-precision value to half-precision in the top half of a
    \ single-precision register, preserving its other half
    $EEB3 $0BC0 instr-dr>sr vcvtt.f16.f64_,_

    \ Double-precision floating-point divide
    $EE80 $0B00 instr-3*dr vdiv.f64_,_,_

    \ Double-precision floating-point fused multiply accumulate
    $EEA0 $0B00 instr-3*dr vfma.f64_,_,_

    \ Double-precision floating-point fused multiply subtract
    $EEA0 $0B40 instr-3*dr vfms.f64_,_,_

    \ Double-precision floating-point fused negate multiply accumulate
    $EE90 $0B40 instr-3*dr vfnma.f64_,_,_

    \ Double-precision floating-point fused negate multiply subtract
    $EE90 $0B00 instr-3*dr vfnms.f64_,_,_

    \ Double-precision floating-point load multiple increment after
    $EC90 $0B00 instr-dr-load/store-multi vldmia.f64_,{_..._}

    \ Double-precision floating-point load multiple increment after with update
    $ECB0 $0B00 instr-dr-load/store-multi vldmia!.f64_,{_..._}

    \ Double-precision floating-point load multiple decrement before with update
    $ED30 $0B00 instr-dr-load/store-multi vldmdb!.f64_,{_..._}

    \ Double-precision floating-point load
    $ED10 $0B00 instr-dr-load/store-imm vldr.f64_,[_,#_]

    \ Double-precision floating-point maximum number
    $FE80 $0B00 instr-3*dr vmaxnm.f64_,_,_

    \ Double-precision floating-point minimum number
    $FE80 $0B40 instr-3*dr vminnm.f64_,_,_

    \ Double-precision floating-point multiply accumulate
    $EE00 $0B00 instr-3*dr vmla.f64_,_,_

    \ Double-precision floating-point multiply subtract
    $EE00 $0B40 instr-3*dr vmls.f64_,_,_

    \ Double-precision floating-point move immediate
    $EEB0 $0B00 instr-dr-imm vmov.f64_,#<imm4h>,#<imm4l>

    \ Double-precision floating-point move
    $EEB0 $0B40 instr-2*dr vmov.f64_,_

    \ Double-precision floating-point move to two core registers
    $EC50 $0B10 true instr-2*cr-dr vmov.cr.f64_,_,_

    \ Double-precision floating-point move from two core registers
    $EC40 $0B10 false instr-2*cr-dr vmov.f64.cr_,_,_

    \ Double-precision floating-point multiply
    $EE20 $0B00 instr-3*dr vmul.f64_,_,_

    \ Double-precision floating-point negate
    $EEB1 $0B40 instr-2*dr vneg.f64_,_

    \ Double-precision floating-point multiply accumulate and negate
    $EE10 $0B40 instr-3*dr vnmla.f64_,_,_

    \ Double-precision floating-point multiply subtract and negate
    $EE10 $0B00 instr-3*dr vnmls.f64_,_,_

    \ Double-precision floating-point multiply and negate
    $EE20 $0B40 instr-3*dr vnmul.f64_,_,_

    \ Round a double-precision floating-point value to nearest with ties to away
    $FEB8 $0B40 instr-2*dr vrinta.f64_,_

    \ Round a double-precision floating-point value to nearest with ties to even
    $FEB9 $0B40 instr-2*dr vrintn.f64_,_

    \ Round a double-precision floating-point value towards +Infinity
    $FEBA $0B40 instr-2*dr vrintp.f64_,_

    \ Round a double-precision floating-point value towards -Infinity
    $FEBB $0B40 instr-2*dr vrintm.f64_,_

    \ Round a double-precision floating-point value using the rounding mode in
    \ FPSCR, an inexact result raising the inexact exception
    $EEB7 $0B40 instr-2*dr vrintx.f64_,_

    \ Round a double-precision floating-point value towards zero
    $EEB6 $0BC0 instr-2*dr vrintz.f64_,_

    \ Round a double-precision floating-point value using the rounding mode in
    \ FPSCR
    $EEB6 $0B40 instr-2*dr vrintr.f64_,_

    \ Double-precision floating-point selection based on an EQ condition code
    $FE00 $0B00 instr-3*dr vseleq.f64_,_,_

    \ Double-precision floating-point selection based on a VS condition code
    $FE10 $0B00 instr-3*dr vselvs.f64_,_,_

    \ Double-precision floating-point selection based on a GE condition code
    $FE20 $0B00 instr-3*dr vselge.f64_,_,_

    \ Double-precision floating-point selection based on a GT condition code
    $FE30 $0B00 instr-3*dr vselgt.f64_,_,_

    \ Double-precision floating-point square root
    $EEB1 $0BC0 instr-2*dr vsqrt.f64_,_

    \ Double-precision floating-point store multiple increment after
    $EC80 $0B00 instr-dr-load/store-multi vstmia.f64_,{_..._}

    \ Double-precision floating-point store multiple increment after with update
    $ECA0 $0B00 instr-dr-load/store-multi vstmia!.f64_,{_..._}

    \ Double-precision floating-point store multiple decrement before with update
    $ED20 $0B00 instr-dr-load/store-multi vstmdb!.f64_,{_..._}

    \ Double-precision floating-point store
    $ED00 $0B00 instr-dr-load/store-imm vstr.f64_,[_,#_]

    \ Double-precision floating-point subtraction
    $EE30 $0B40 instr-3*dr vsub.f64_,_,_

    end-compress-flash

    \ Load multiple consecutive double-precision floating point values from the
    \ stack
    : vpop.f64{_..._} sp vldmia!.f64_,{_..._} ;

    \ Store multiple consecutive double-precision floating point values on the
    \ stack
    : vpush.f64{_..._} sp vstmdb!.f64_,{_..._} ;

  end-module

end-module

reboot
