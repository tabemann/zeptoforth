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

begin-module armv7m-fp

  internal import

  \ Out of range 4-bit register exception
  : x-out-of-range-4reg ( -- ) ." out of range 4-bit register" cr ;

  \ Out of range register range
  : x-out-of-range-reg-range ( -- ) ." out of range register range" cr ;
  
  \ Out of range immediate exception
  : x-out-of-range-imm ( -- ) ." out of range immediate" cr ;

  \ Unaligned immediate exception
  : x-unaligned-imm ( -- ) ." unaligned immediate" cr ;

  begin-module armv7m-fp-mask

    \ The instruction masks, for convenience's sake
    
    %1111_1111_0011_0000 constant instr-sr-load/store-imm-mask-0
    %0000_1111_0000_0000 constant instr-sr-load/store-imm-mask-1

    %1111_1111_1011_0000 constant instr-sr-load/store-multi-mask-0
    %0000_1111_0000_0000 constant instr-sr-load/store-multi-mask-1
    
    %1111_1111_1011_1111 constant instr-sr-mask-0
    %0000_1111_1101_0000 constant instr-sr-mask-1

    %1111_1111_1111_0000 constant instr-cr-sr-mask-0
    %0000_1111_0001_0000 constant instr-cr-sr-mask-1

    %1111_1111_1011_0000 constant instr-sr-imm-mask-0
    %0000_1111_0101_0000 constant instr-sr-imm-mask-1

    %1111_1111_1011_1111 constant instr-sr-fract-mask-0
    %0000_1111_1101_0000 constant instr-sr-fract-mask-1

    %1111_1111_1011_1111 constant instr-2*sr-mask-0
    %0000_1111_1101_0000 constant instr-2*sr-mask-1
    
    %1111_1111_1011_0000 constant instr-3*sr-mask-0
    %0000_1111_0101_0000 constant instr-3*sr-mask-1

    %1111_1111_1111_0000 constant instr-2*cr-2*sr-mask-0
    %0000_1111_0001_0000 constant instr-2*cr-2*sr-mask-1

    %1111_1111_1111_0000 constant instr-cr-fpscr-mask-0
    %0000_1111_0001_0000 constant instr-cr-fpscr-mask-1

  end-module
  
  begin-module armv7m-fp-internal

    \ Validate a 4-bit register
    : validate-4reg ( reg -- ) 16 u< averts x-out-of-range-4reg ;

    \ Validate two 4-bit registers
    : validate-2-4reg ( reg1 reg0 -- ) validate-4reg validate-4reg ;
    
    \ Validate three 3-bit registers
    : validate-3-4reg ( reg2 reg1 reg0 -- )
      validate-4reg validate-4reg validate-4reg
    ;

    \ Validate a register range
    : validate-reg-range ( count reg -- )
      dup validate-4reg
      over 0<> averts x-out-of-range-reg-range
      + 32 u<= averts x-out-of-range-reg-range
    ;
    
    \ Validate an immediate as being aligned to a halfword
    : validate-imm-2align ( imm -- ) $1 and 0= averts x-unaligned-imm ;

    \ Validate an immediate as being aligned to a word
    : validate-imm-4align ( imm -- ) $3 and 0= averts x-unaligned-imm ;

    \ 32-bit single-precision register load/store 4-byte-immediate instruction
    : instr-sr-load/store-imm ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { imm rn vd instr }
      vd rn validate-2-4reg
      imm validate-imm-4align
      imm abs 2 rshift 256 < averts x-out-of-range-imm
      instr $FFFF and imm 0>= 1 and 7 lshift or vd 1 and 6 lshift or rn or h,
      instr 16 rshift vd 1 rshift 12 lshift or imm abs 2 rshift or h,
    ;

    \ 32-bit single-precision register load/store multiple instruction
    : instr-sr-load/store-multi ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vd-end vd rn instr }
      vd rn validate-2-4reg
      vd-end vd - 1+ { count }
      count vd validate-reg-range
      instr $FFFF and vd 1 and 6 lshift or rn or h,
      instr 16 rshift vd 1 rshift 12 lshift or count or h,
    ;

    \ 32-bit one single-precision register instruction
    : instr-sr ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vd instr }
      vd validate-4reg
      instr $FFFF and vd 1 and 6 lshift or h,
      instr 16 rshift vd 1 rshift 12 lshift or h,
    ;

    \ 32-bit one single-precision register to core register instruction
    : instr-cr-sr ( h0 h1 reverse "name" -- )
      <builds , 16 lshift or , does> dup @ swap cell+ @ { r1 r0 rev instr }
      rev if r0 r1 else r1 r0 then { rt vn }
      rt vn validate-2-4reg
      instr $FFFF and vn 1 rshift or h,
      instr 16 rshift rt 12 lshift or vn 1 and 7 lshift or h,
    ;

    \ 32-bit one single-precision register instruction with immediate constant
    : instr-sr-imm ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { imm4l imm4h vd instr }
      vd validate-4reg
      imm4h 8 < averts x-out-of-range-imm
      imm4h -8 >= averts x-out-of-range-imm
      imm4l 16 u< averts x-out-of-range-imm
      instr $FFFF and vd 1 and 6 lshift or imm4h or h,
      instr 16 rshift vd 1 rshift 12 lshift or imm4l or h,
    ;

    \ 32-bit one single-precision register instruction with fraction
    : instr-sr-fract ( h0 h1 size "name" -- )
      <builds , 16 lshift or , does> dup @ swap cell+ @ { imm vd size instr }
      vd validate-4reg
      imm size u< averts x-out-of-range-imm
      instr $FFFF and vd 1 and 6 lshift or h,
      instr 16 rshift vd 1 rshift 12 lshift or
      imm 1 and 5 lshift or imm 1 rshift or h,
    ;

    \ 32-bit two single-precision registers instruction
    : instr-2*sr ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vm vd instr }
      vm vd validate-2-4reg
      instr $FFFF and vd 1 and 6 lshift or h,
      instr 16 rshift vd 1 rshift 12 lshift or
      vm 1 and 5 lshift or vm 1 rshift or h,
    ;

    \ 32-bit three single-precision registers instruction
    : instr-3*sr ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { vm vn vd instr }
      vm vn vd validate-3-4reg
      instr $FFFF and vd 1 and 6 lshift or vn 1 rshift or h,
      instr 16 rshift vd 1 rshift 12 lshift or
      vn 1 and 7 lshift or vm 1 and 5 lshift or vm 1 rshift or h,
    ;

    \ 32-bit transfer between two core registers and two consecutive single
    \ precision registers
    : instr-2*cr-2*sr ( h0 h1 reverse "name" -- )
      <builds , 16 lshift or , does> dup @ swap cell+ @ { r2 r1 r0 rev instr }
      rev if r1 r0 r2 else r2 r1 r0 then { rt2 rt vm }
      rt2 rt vm validate-3-4reg
      instr $FFFF and rt2 or h,
      instr 16 rshift rt 12 lshift or vm 1 and 5 lshift or vm 1 rshift or h,
    ;
    
    \ 32-bit special transfer with FPSCR to/from core register
    : instr-cr-fpscr ( h0 h1 "name" -- )
      <builds 16 lshift or , does> @ { rt instr }
      rt validate-4reg
      instr $FFFF and h,
      instr 16 rshift rt 12 lshift or h,
    ;

  end-module> import

  begin-module armv7m-fp-instr

    armv6m::armv6m-instr import
    
    \ Registers
    0 constant s0
    1 constant s1
    2 constant s2
    3 constant s3
    4 constant s4
    5 constant s5
    6 constant s6
    7 constant s7
    8 constant s8
    9 constant s9
    10 constant s10
    11 constant s11
    12 constant s12
    13 constant s13
    14 constant s14
    15 constant s15
    16 constant s16
    17 constant s17
    18 constant s18
    19 constant s19
    20 constant s20
    21 constant s21
    22 constant s22
    23 constant s23
    24 constant s24
    25 constant s25
    26 constant s26
    27 constant s27
    28 constant s28
    29 constant s29
    30 constant s30
    31 constant s31

    \ Special constant for transferring FPSCR to/from APSR flags
    %1111 constant apsr-nzcv

    \ Single-precision floating-point absolute value
    $EEB0 $0AC0 instr-2*sr vabs.f32_,_

    \ Single-precision floating-point addition
    $EE30 $0A00 instr-3*sr vadd.f32_,_,_

    \ Single-precision floating-point compare
    $EEB4 $0A40 instr-2*sr vcmp.f32_,_

    \ Single-precision floating-point compare with exception silencing
    $EEB4 $0AC0 instr-2*sr vcmpe.f32_,_

    \ Single-precision floating-point compare with zero
    $EEB5 $0A40 instr-sr vcmp.f32_,#0.0

    \ Single-precision floating-point compare with zero with exception silencing
    $EEB5 $0AC0 instr-sr vcmpe.f32_,#0.0

    \ Convert single-precision floating point to unsigned integer rounding to
    \ nearest with ties to away
    $FEBC $0A40 instr-2*sr vcvta.u32.f32_,_

    \ Convert single-precision floating point to signed integer rounding to
    \ nearest with ties to away
    $FEBC $0AC0 instr-2*sr vcvta.s32.f32_,_

    \ Convert single-precision floating point to unsigned integer rounding to
    \ nearest with ties to even
    $FEBD $0A40 instr-2*sr vcvte.u32.f32_,_

    \ Convert single-precision floating point to signed integer rounding to
    \ nearest with ties to even
    $FEBD $0AC0 instr-2*sr vcvte.s32.f32_,_    

    \ Convert single-precision floating point to unsigned integer rounding
    \ towards +Infinity
    $FEBE $0A40 instr-2*sr vcvtp.u32.f32_,_

    \ Convert single-precision floating point to signed integer rounding
    \ towards +Infinity
    $FEBE $0AC0 instr-2*sr vcvtp.s32.f32_,_    
    
    \ Convert single-precision floating point to unsigned integer rounding
    \ towards -Infinity
    $FEBF $0A40 instr-2*sr vcvtm.u32.f32_,_

    \ Convert single-precision floating point to signed integer rounding
    \ towards -Infinity
    $FEBF $0AC0 instr-2*sr vcvtm.s32.f32_,_

    \ Convert single-precision floating point to unsigned integer rounding
    \ towards zero
    $EEBC $0AC0 instr-2*sr vcvtr.u32.f32_,_

    \ Convert single-precision floating point to signed integer rounding
    \ towards zero
    $EEBD $0AC0 instr-2*sr vcvtr.s32.f32_,_

    \ Convert single-precision floating point to unsigned integer using
    \ rounding mode in FPSCR
    $EEBC $0A40 instr-2*sr vcvt.u32.f32_,_

    \ Convert single-precision floating point to signed integer using
    \ rounding mode in FPSCR
    $EEBD $0A40 instr-2*sr vcvt.s32.f32_,_

    \ Convert unsigned integer to single-precision floating point using
    \ rounding mode in FPSCR
    $EEB8 $0A40 instr-2*sr vcvt.f32.u32_,_

    \ Convert signed integer to single-precision floating point using
    \ rounding mode in FPSCR
    $EEB8 $0AC0 instr-2*sr vcvt.f32.s32_,_

    \ Convert single-precision floating point to 16-bit unsigned fixed point
    \ rounding towards zero
    $EEBF $0A40 16 instr-sr-fract vcvt.u16.f32_,#<fbits>

    \ Convert single-precision floating point to 16-bit signed fixed point
    \ rounding towards zero
    $EEBE $0A40 16 instr-sr-fract vcvt.s16.f32_,#<fbits>

    \ Convert single-precision floating point to 32-bit unsigned fixed point
    \ rounding towards zero
    $EEBF $0AC0 32 instr-sr-fract vcvt.u32.f32_,#<fbits>

    \ Convert single-precision floating point to 32-bit signed fixed point
    \ rounding towards zero
    $EEBE $0AC0 32 instr-sr-fract vcvt.s32.f32_,#<fbits>

    \ Convert 16-bit unsigned fixed point to single-precision floating point
    \ rounding towards zero
    $EEBB $0A40 16 instr-sr-fract vcvt.f32.u16_,#<fbits>

    \ Convert 16-bit signed fixed point to single-precision floating point
    \ rounding towards zero
    $EEBA $0A40 16 instr-sr-fract vcvt.f32.s16_,#<fbits>

    \ Convert 32-bit unsigned fixed point to single-precision floating point
    \ rounding towards zero
    $EEBB $0AC0 32 instr-sr-fract vcvt.f32.u32_,#<fbits>

    \ Convert 32-bit signed fixed point to single-precision floating point
    \ rounding towards zero
    $EEBA $0AC0 32 instr-sr-fract vcvt.f32.s32_,#<fbits>

    \ Convert the half-precision value in the bottom half of a single-precision
    \ register to single-precision and write the result to a single-precision
    \ register
    $EEB2 $0A40 instr-2*sr vcvtb.f32.f16_,_

    \ Convert the half-precision value in the top half of a single-precision
    \ register to single-precision and write the result to a single-precision
    \ register
    $EEB2 $0AC0 instr-2*sr vcvtt.f32.f16_,_

    \ Convert the value in a single-precision register to half-precision and
    \ write the result into the bottom half of a single-precision register,
    \ preserving the other half of the target register
    $EEB3 $0A40 instr-2*sr vcvtb.f16.f32_,_

    \ Convert the value in a single-precision register to half-precision and
    \ write the result into the top half of a single-precision register,
    \ preserving the other half of the target register
    $EEB3 $0AC0 instr-2*sr vcvtt.f16.f32_,_

    \ Single-precision floating-point divide
    $EE80 $0A00 instr-3*sr vdiv.f32_,_,_    

    \ Single-precision floating-point fused multiply accumulate
    $EEA0 $0A00 instr-3*sr vfma.f32_,_,_

    \ Single-precision floating-point fused multiply subtract
    $EEA0 $0A40 instr-3*sr vfms.f32_,_,_

    \ Single-precision floating-point fused negate multiply accumulate
    $EE90 $0A00 instr-3*sr vfnma.f32_,_,_

    \ Single-precision floating-point fused negate multiply subtract
    $EE90 $0A40 instr-3*sr vfnms.f32_,_,_

    \ Single-precision floating-point load multiple increment after
    $EC90 $0A00 instr-sr-load/store-multi vldmia.f32_,{_..._}

    \ Single-precision floating-point load multiple increment after with update
    $ECB0 $0A00 instr-sr-load/store-multi vldmia!.f32_,{_..._}

    \ Single-precision floating-point load multiple decrement before with update
    $ED30 $0A00 instr-sr-load/store-multi vldmdb!.f32_,{_..._}

    \ Single-precision floating-point load
    $ED10 $0A00 instr-sr-load/store-imm vldr.f32_,[_,#_]

    \ Single-precision floating-point maximum number
    $FE80 $0A00 instr-3*sr vmaxnm.f32_,_,_

    \ Single-precision floating-point minimum number
    $FE80 $0A40 instr-3*sr vminnm.f32_,_,_

    \ Single-precision floating-point multiply accumulate
    $EE00 $0A40 instr-3*sr vmla.f32_,_,_

    \ Single-precision floating-point multiply subtract
    $EE00 $0A00 instr-3*sr vmls.f32_,_,_

    \ Single-precision floating-point move immediate
    $EEB0 $0A00 instr-sr-imm vmov.f32_,#<imm4h>,#<imm4l>

    \ Single-precision floating-point move
    $EEB0 $0A40 instr-2*sr vmov.f32_,_

    \ Single-precision floating-point move to core register
    $EE10 $0A10 true instr-cr-sr vmov.cr.f32_,_

    \ Single-precision floating-point move from core register
    $EE00 $0A10 false instr-cr-sr vmov.f32.cr_,_

    \ Single-precision floating-point move to two core registers
    $EE50 $0A10 true instr-2*cr-2*sr vmov.cr.f32_,_,_

    \ Single-precision floating-point move from two core registers
    $EE40 $0A10 false instr-2*cr-2*sr vmov.f32.cr_,_,_

    \ Move FPSCR to a core register, or the FCSR flags to the APSR
    $EEF1 $0A10 instr-cr-fpscr vmrs_,fpscr

    \ Move a core register to the FPSCR
    $EEE1 $0A10 instr-cr-fpscr vmsrfpscr,_

    \ Single-precision floating-point multiply
    $EE20 $0A00 instr-3*sr vmul.f32_,_,_

    \ Single-precision floating-point negate
    $EEB1 $0A40 instr-2*sr vneg.f32_,_

    \ Single-precision floating-point multiply accumulate and negate
    $EE10 $0A40 instr-3*sr vnmla.f32_,_,_

    \ Single-precision floating-point multiply subtract and negate
    $EE10 $0A00 instr-3*sr vnmls.f32_,_,_

    \ Single-precision floating-point multiply and negate
    $EE20 $0A40 instr-3*sr vnmul.f32_,_,_

    \ Round a single-precision floating-point value to nearest with ties to away
    $FEB8 $0A40 instr-2*sr vrinta.f32_,_

    \ Round a single-precision floating-point value to nearest with ties to even
    $FEB9 $0A40 instr-2*sr vrintn.f32_,_

    \ Round a single-precision floating-point value towards +Infinity
    $FEBA $0A40 instr-2*sr vrintp.f32_,_

    \ Round a single-precision floating-point value towards -Infinity
    $FEBB $0A40 instr-2*sr vrintm.f32_,_

    \ Round a single-precision floating-point value using the rounding mode
    \ specified in the FPSCR, with zero giving a zero of equal sign, an
    \ infinite input giving an infinite result of equal sign, NaN being
    \ propagated, and an inexact conversion resulting in an inexact exception
    $EEB7 $0A40 instr-2*sr vrintx.f32_,_

    \ Round a single-precision floating-point towards zero, with zero giving a
    \ zero of equal sign, an infinite input giving an infinite result of equal
    \ sign, and a NaN being propagated
    $EEB6 $0AC0 instr-2*sr vrintz.f32_,_

    \ Round a single-precision floating-point value using the rounding mode
    \ specified in the FPSCR, with zero giving a zero of equal sign, an
    \ infinite input giving an infinite result of equal sign, and a NaN being
    \ propagated
    $EEB6 $0A40 instr-2*sr vrintr.f32_,_

    \ Single-precision floating-point selection based on an EQ condition code
    $FE00 $0A00 instr-3*sr vseleq.f32_,_,_

    \ Single-precision floating-point selection based on a VS condition code
    $FE10 $0A00 instr-3*sr vselvs.f32_,_,_

    \ Single-precision floating-point selection based on a GE condition code
    $FE20 $0A00 instr-3*sr vselge.f32_,_,_

    \ Single-precision floating-point selection based on a GT condition code
    $FE30 $0A00 instr-3*sr vselgt.f32_,_,_

    \ Single-precision floating-point square root
    $EEB1 $0AC0 instr-2*sr vsqrt.f32_,_
    
    \ Single-precision floating-point store multiple increment after
    $EC80 $0A00 instr-sr-load/store-multi vstmia.f32_,{_..._}

    \ Single-precision floating-point store multiple increment after with update
    $ECA0 $0A00 instr-sr-load/store-multi vstmia!.f32_,{_..._}

    \ Single-precision floating-point store multiple decrement before with
    \ update
    $ED20 $0A00 instr-sr-load/store-multi vstmdb!.f32_,{_..._}

    \ Single-precision floating-point store
    $ED00 $0A00 instr-sr-load/store-imm vstr.f32_,[_,#_]

    \ Single-precision floating-point subtraction
    $EE30 $0A40 instr-3*sr vsub.f32_,_,_
    
    \ Load multiple consecutive single-precision floating point values from the
    \ stack
    : vpop.f32{_..._} sp vldmia!.f32_,{_..._} ;
    
    \ Store multiple consecutive single-precision floating point values on the
    \ stack
    : vpush.f32{_..._} sp vstmdb!.f32_,{_..._} ;
    
  end-module

  \ Begin an assembly block
  : code[
    [compile-only] [immediate]
    armv7m-fp-instr import
    postpone armv6m::code[
  ;

  \ End an assembly block
  : ]code
    armv7m-fp-instr unimport
    armv6m::]code
  ;
  
end-module

