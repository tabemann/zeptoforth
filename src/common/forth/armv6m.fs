\ Copyright (c) 2022 Travis Bemann
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

begin-module armv6m

  internal import

  \ Out of range 3-bit register exception
  : x-out-of-range-3reg ( -- ) ." out of range 3-bit register" cr ;

  \ Out of range 4-bit register exception
  : x-out-of-range-4reg ( -- ) ." out of range 4-bit register" cr ;

  \ Out of range immediate exception
  : x-out-of-range-imm ( -- ) ." out of range immediate" cr ;

  \ Unaligned immediate exception
  : x-unaligned-imm ( -- ) ." unaligned immediate" cr ;

  \ Out of range PC-relative address
  : x-out-of-range-pc-rel ( -- ) ." out of range PC-relative address" cr ;

  \ Incorrect marker type
  : x-incorrect-mark-type ( -- ) ." incorrect marker type" cr ;

  \ Invalid condition
  : x-invalid-cond ( -- ) ." invalid condition" cr ;

  \ Out of range special register
  : x-out-of-range-special ( -- ) ." out of range special register" ;
  
  begin-module armv6m-internal

    \ Get a marker's type
    : mark-type ( marker -- type ) $FFFF and ;

    \ Get a marker's parameter
    : mark-param ( marker -- parameter ) 16 rshift ;

    \ Destination marker type
    0 constant mark-dest

    \ ADR marker type
    1 constant mark-adr

    \ LDR marker type
    2 constant mark-ldr

    \ Conditional branch marker type
    3 constant mark-bc

    \ Unconditional branch marker type
    4 constant mark-b

    \ Validate a 3-bit register
    : validate-3reg ( reg -- ) 8 u< averts x-out-of-range-3reg ;

    \ Validate a 4-bit register
    : validate-4reg ( reg -- ) 16 u< averts x-out-of-range-4reg ;

    \ Validate two 3-bit registers
    : validate-2-3reg ( reg1 reg0 -- ) validate-3reg validate-3reg ;

    \ Validate two 4-bit registers
    : validate-2-4reg ( reg1 reg0 -- ) validate-4reg validate-4reg ;
    
    \ Validate three 3-bit registers
    : validate-3-3reg ( reg2 reg1 reg0 -- )
      validate-3reg validate-3reg validate-3reg
    ;

    \ Validate an immediate as being aligned to a halfword
    : validate-imm-2align ( imm -- ) $1 and 0= averts x-unaligned-imm ;

    \ Validate an immediate as being aligned to a word
    : validate-imm-4align ( imm -- ) $3 and 0= averts x-unaligned-imm ;

    \ Validate a condition
    : validate-cond ( cond -- ) $F u< averts x-invalid-cond ;

    \ Validate a mark
    : validate-mark ( mark mark-type -- )
      swap mark-type = averts x-incorrect-mark-type
    ;

    \ Validate a special register
    : validate-special ( special -- ) 256 u< averts x-out-of-range-special ;
    
    \ Resolve an ADR instruction
    : resolve-adr ( mark-addr mark -- )
      4 align,
      over 2 + 4 align here swap -
      dup 1024 u< averts x-out-of-range-pc-rel
      2 rshift swap mark-param 8 lshift or $B000 or swap hcurrent!
    ;

    \ Resolve an LDR instruction
    : resolve-ldr ( mark-addr mark -- )
      4 align,
      over 2 + 4 align here swap -
      dup 1024 u< averts x-out-of-range-pc-rel
      2 rshift swap mark-param 8 lshift or $4800 or swap hcurrent!
    ;

    \ Resolve an unconditional branch
    : resolve-b ( mark-addr mark -- )
      drop
      here over - 1 rshift
      dup 1024 < over -1025 > and averts x-out-of-range-pc-rel
      $7FF and $E000 or swap hcurrent!
    ;

    \ Resolve a conditional branch
    : resolve-bc ( mark-addr mark -- )
      mark-param dup validate-cond >r
      here over - 1 rshift
      dup 128 < over -129 > and averts x-out-of-range-pc-rel
      $FF and r> 8 lshift or $D000 or swap hcurrent!
    ;
    
  end-module> import
  
  begin-module armv6m-instr

    \ Registers
    0 constant r0
    1 constant r1
    2 constant r2
    3 constant r3
    4 constant r4
    5 constant r5
    6 constant r6
    7 constant r7
    8 constant r8
    9 constant r9
    10 constant r10
    11 constant r11
    12 constant r12
    13 constant r13
    14 constant r14
    15 constant r15
    r6 constant tos
    r7 constant dp
    r13 constant sp
    r14 constant lr
    r15 constant pc

    \ Condition codes
    $0 constant eq
    $1 constant ne
    $2 constant cs
    $2 constant hs
    $3 constant cc
    $3 constant lo
    $4 constant mi
    $5 constant pl
    $6 constant vs
    $7 constant vc
    $8 constant hi
    $9 constant ls
    $A constant ge
    $B constant lt
    $C constant gt
    $D constant le
    $E constant al

    \ Assemble an ADCS instruction
    : adcs_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $4140 or h,
    ;

    \ Assemble a two-register ADDS immediate instruction
    : adds_,_,#_ ( imm3 rn rd -- )
      2dup validate-2-3reg 2 pick 8 u< averts x-out-of-range-imm
      swap 3 lshift or swap 6 lshift or $1C00 or h,
    ;

    \ Assemble a one-register ADDS immediate instruction
    : adds_,#_ ( imm8 rdn -- )
      dup validate-3reg over 256 u< averts x-out-of-range-imm
      8 lshift or $3000 or h,
    ;

    \ Assemble a three-register ADDS register instruction
    : adds_,_,_ ( rm rn rd -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $1800 or h,
    ;

    \ Assemble a two-4-bit register ADDS register instruction
    : add4_,4_ ( rm4 rdn4 -- )
      2dup validate-2-4reg dup $7 and swap $8 and 4 lshift or swap 3 lshift or
      $4400 or h,
    ;

    \ Assemble an ADD (SP plus immediate) instruction
    : add_,sp,_#_ ( imm8 rd -- )
      dup validate-3reg over validate-imm-4align
      over 1024 u< averts x-out-of-range-imm
      8 lshift swap 2 rshift or $A800 or h,
    ;

    \ Assemble an ADD (SP plus immediate to SP) instruction
    : addsp,sp,#_ ( imm7 -- )
      dup validate-imm-4align dup 512 u< averts x-out-of-range-imm
      2 rshift $B000 or h,
    ;

    \ Assemble an ADD (SP plus register) instruction
    : add4_,sp,4_ ( rdm4 -- )
      dup validate-4reg dup $7 and swap $8 and 4 lshift or $4468 or h,
    ;

    \ Assemble an ADD (SP plus register to SP) instruction
    : addsp,4_ ( rm4 -- )
      dup validate-4reg 3 lshift $4485 or h,
    ;

    \ Mark an ADR instruction
    : adr_ ( rd -- mark-addr mark )
      dup validate-3reg hreserve swap 16 lshift mark-adr or
    ;

    \ Assemble an ANDS (register) instruction
    : ands_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $4000 or h,
    ;

    \ Assemble an ASRS (immediate) instruction
    : asrs_,_,#_ ( imm5 rm rd -- )
      2dup validate-2-3reg
      2 pick 1 u>= averts x-out-of-range-imm
      2 pick 33 u< averts x-out-of-range-imm
      swap 3 lshift or swap dup 32 = if drop 0 then 6 lshift or $1000 or h,
    ;

    \ Assemble an ASRS (register) instruction
    : asrs_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $4100 or h,
    ;

    \ Assemble an unconditional branch to a marker
    : b< ( mark-addr mark -- )
      mark-dest validate-mark
      here 4 + - 1 rshift
      dup 1024 < over -1025 > and averts x-out-of-range-pc-rel
      $7FF and $E000 or h,
    ;

    \ Assemble a conditional branch to a marker
    : bc< ( mark-addr mark cond -- )
      dup validate-cond
      swap mark-dest validate-mark
      swap here 4 + - 1 rshift
      dup 128 < over -129 > and averts x-out-of-range-pc-rel
      $FF and swap 8 lshift or $D000 or h,
    ;

    \ Mark an unconditional branch
    : b> ( -- mark-addr mark )
      hreserve 4 + mark-b
    ;

    \ Mark a conditional branch
    : bc> ( cond -- mark-addr mark )
      dup validate-cond hreserve 4 + swap 16 lshift mark-bc or
    ;

    \ Assemble an BICS (register) instruction
    : bics_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $4380 or h,
    ;

    \ Assemble a BKPT instruction
    : bkpt#_ ( imm8 -- )
      dup 256 u< averts x-out-of-range-imm
      $BE00 or h,
    ;

    \ We are not implementing BL; it is up to the user to use normal word calls
    \ for this

    \ Assemble a BLX (register) instruction
    : blx_ ( rm -- )
      dup validate-4reg 3 lshift $4780 or h,
    ;

    \ Assemble a BX instruction
    : bx_ ( rm -- )
      dup validate-4reg 3 lshift $4700 or h,
    ;

    \ Assemble an CMN (register) instruction
    : cmn_,_ ( rm rn -- )
      2dup validate-2-3reg swap 3 lshift or $42C0 or h,
    ;

    \ Assemble a CMP (immediate) instruction
    : cmp_,#_ ( imm8 rn -- )
      dup validate-3reg over 256 u< averts x-out-of-range-imm
      8 lshift or $2800 or h,
    ;

    \ Assemble an CMP (register) instruction
    : cmp_,_ ( rm rn -- )
      2dup validate-2-3reg swap 3 lshift or $4280 or h,
    ;

    \ Assemble an CMP (register) instruction
    : cmp4_,4_ ( rm4 rdn4 -- )
      2dup validate-2-4reg dup $7 and swap $8 and 4 lshift or swap 3 lshift or
      $4500 or h,
    ;

    \ Assemble a CPSIE instruction
    : cpsie ( -- ) $B662 h, ;

    \ Assemble a CPSID instruction
    : cpsid ( -- ) $B672 h, ;

    \ Assemble a DMB instruction
    : dmb ( -- ) $F3BF h, $8F5F h, ;

    \ Assemble a DSB instruction
    : dsb ( -- ) $F3BF h, $8F4F h, ;

    \ Assemble an EORS (register) instruction
    : eors_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $4040 or h,
    ;

    \ Assemble an ISB instruction
    : isb ( -- ) $F3BF h, $8F6F h, ;

    \ Assemble an LDM instruction
    : ldm ( rx ... r0 count rn -- )
      dup validate-3reg >r
      0 begin over while rot dup validate-3reg bit or swap 1- swap repeat nip
      r> 8 lshift or $C800 or h,
    ;

    \ Assemble an LDR (immediate) instruction
    : ldr_,[_,#_] ( imm5 rn rt -- )
      2dup validate-2-3reg 2 pick validate-imm-4align
      swap 3 lshift or swap 2 lshift dup 32 u< averts x-out-of-range-imm
      6 lshift or $6800 or h,
    ;

    \ Assemble an LDR (immediate) instruction
    : ldr_,[sp,#_] ( imm8 rt -- )
      dup validate-3reg over validate-imm-4align
      8 lshift swap 2 lshift dup 256 u< averts x-out-of-range-imm
      or $9800 or h,
    ;

    \ Mark an LDR (literal) instruction
    : ldr_,[pc] ( rd -- mark-addr mark )
      dup validate-3reg hreserve swap 16 lshift mark-ldr or
    ;

    \ Assemble an LDR (register) instruction
    : ldr_,[_,_] ( rm rn rt -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $5800 or h,
    ;

    \ Assemble an LDRB (immediate) instruction
    : ldrb_,[_,#_] ( imm5 rn rt -- )
      2dup validate-2-3reg
      swap 3 lshift or swap dup 32 u< averts x-out-of-range-imm
      6 lshift or $7800 or h,
    ;

    \ Assemble an LDRB (register) instruction
    : ldrb_,[_,_] ( rm rn rt -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $5C00 or h,
    ;

    \ Assemble an LDRH (immediate) instruction
    : ldrh_,[_,#_] ( imm5 rn rt -- )
      2dup validate-2-3reg 2 pick validate-imm-2align
      swap 3 lshift or swap 1 lshift dup 32 u< averts x-out-of-range-imm
      6 lshift or $8800 or h,
    ;

    \ Assemble an LDRH (register) instruction
    : ldrh_,[_,_] ( rm rn rt -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $5C00 or h,
    ;

    \ Assemble an LDRSB (register) instruction
    : ldrsb_,[_,_] ( rm rn rt -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $5600 or h,
    ;

    \ Assemble an LDRSH (register) instruction
    : ldrsh_,[_,_] ( rm rn rt -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $5E00 or h,
    ;

    \ Assemble an LSLS (immediate) instruction
    : lsls_,_,#_ ( imm5 rm rd -- )
      2dup validate-2-3reg
      swap 3 lshift or swap 6 lshift or $0000 or h,
    ;

    \ Assemble an LSLS (register) instruction
    : lsls_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $4080 or h,
    ;

    \ Assemble an LSRS (immediate) instruction
    : lsrs_,_,#_ ( imm5 rm rd -- )
      2dup validate-2-3reg
      2 pick 1 u>= averts x-out-of-range-imm
      2 pick 33 u< averts x-out-of-range-imm
      swap 3 lshift or swap dup 32 = if drop 0 then 6 lshift or $0800 or h,
    ;

    \ Assemble an LSRS (register) instruction
    : lsrs_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $40C0 or h,
    ;

    \ Assemble a MOVS (immediate) instruction
    : movs_,#_ ( imm8 rdn -- )
      dup validate-3reg over 256 u< averts x-out-of-range-imm
      8 lshift or $2000 or h,
    ;

    \ Assemble a MOV (register) instruction
    : mov4_,4_ ( rm4 rdn4 -- )
      2dup validate-2-4reg dup $7 and swap $8 and 4 lshift or swap 3 lshift or
      $4600 or h,
    ;

    \ Assemble a MOVS (register) instruction
    : movs_,_ ( rm rd -- )
      2dup validate-2-3reg swap 3 lshift or $0000 or h,
    ;

    \ Assemble an MRS instruction
    : mrs_,_ ( sysm rd -- )
      dup validate-4reg over validate-special
      $F3EF h, swap 8 lshift or $8000 or h,
    ;

    \ Assemble an MSR (register) instruction
    : msr_,_ ( rn sysm -- )
      dup validate-special over validate-4reg
      swap $F380 or h, $8800 or h,
    ;
    
    \ Assemble an MULS (register) instruction
    : muls_,_ ( rn rdm -- )
      2dup validate-2-3reg swap 3 lshift or $4340 or h,
    ;

    \ Assemble an MVNS (register) instruction
    : mvns_,_ ( rm rd -- )
      2dup validate-2-3reg swap 3 lshift or $43C0 or h,
    ;

    \ Assemble a NOP instruction
    : nop ( -- ) $BF00 h, ;

    \ Assemble an ORRS (register) instruction
    : orrs_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $4300 or h,
    ;

    \ Assemble a POP instruction
    : pop ( rx ... r0 count rn -- )
      0 begin over while
	rot dup pc = if
	  drop 8 bit or
	else
	  dup validate-3reg bit or
	then
	swap 1- swap
      repeat
      nip $BC00 or h,
    ;

    \ Assemble a PUSH instruction
    : push ( rx ... r0 count rn -- )
      0 begin over while
	rot dup lr = if
	  drop 8 bit or
	else
	  dup validate-3reg bit or
	then
	swap 1- swap
      repeat
      nip $BC00 or h,
    ;

    \ Assemble an REV (register) instruction
    : rev_,_ ( rm rd -- )
      2dup validate-2-3reg swap 3 lshift or $BA00 or h,
    ;

    \ Assemble an REV16 (register) instruction
    : rev16_,_ ( rm rd -- )
      2dup validate-2-3reg swap 3 lshift or $BA40 or h,
    ;

    \ Assemble an REVSH (register) instruction
    : revsh_,_ ( rm rd -- )
      2dup validate-2-3reg swap 3 lshift or $BAC0 or h,
    ;

    \ Assemble an RORS (register) instruction
    : rors_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $41C0 or h,
    ;

    \ Assemble an RSBS (immediate) instruction
    : rsbs_,_,#0 ( rn rd -- )
      2dup validate-2-3reg swap 3 lshift or $4240 or h,
    ;

    \ Assemble an SBC (register) instruction
    : sbcs_,_ ( rm rdn -- )
      2dup validate-2-3reg swap 3 lshift or $4180 or h,
    ;

    \ Assemble an SEV instruction
    : sev ( -- ) $BF40 h, ;

    \ Assemble an STM instruction
    : stm ( rx ... r0 count rn -- )
      dup validate-3reg >r
      0 begin over while rot dup validate-3reg bit or swap 1- swap repeat nip
      r> 8 lshift or $C000 or h,
    ;

    \ Assemble an STR (immediate) instruction
    : str_,[_,#_] ( imm5 rn rt -- )
      2dup validate-2-3reg 2 pick validate-imm-4align
      swap 3 lshift or swap 2 lshift dup 32 u< averts x-out-of-range-imm
      6 lshift or $6000 or h,
    ;

    \ Assemble an STR (immediate) instruction
    : str_,[sp,#_] ( imm8 rt -- )
      dup validate-3reg over validate-imm-4align
      8 lshift swap 2 lshift dup 256 u< averts x-out-of-range-imm
      or $9000 or h,
    ;

    \ Assemble an STR (register) instruction
    : str_,[_,_] ( rm rn rt -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $5000 or h,
    ;
    
    \ Assemble an STRB (immediate) instruction
    : strb_,[_,#_] ( imm5 rn rt -- )
      2dup validate-2-3reg
      swap 3 lshift or swap dup 32 u< averts x-out-of-range-imm
      6 lshift or $7000 or h,
    ;

    \ Assemble an STRB (register) instruction
    : strb_,[_,_] ( rm rn rt -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $5400 or h,
    ;

    \ Assemble an STRH (immediate) instruction
    : strh_,[_,#_] ( imm5 rn rt -- )
      2dup validate-2-3reg 2 pick validate-imm-2align
      swap 3 lshift or swap 1 lshift dup 32 u< averts x-out-of-range-imm
      6 lshift or $8000 or h,
    ;

    \ Assemble an STRH (register) instruction
    : strh_,[_,_] ( rm rn rt -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $5200 or h,
    ;

    \ Assemble a two-register SUBS immediate instruction
    : subs_,_,#_ ( imm3 rn rd -- )
      2dup validate-2-3reg 2 pick 8 u< averts x-out-of-range-imm
      swap 3 lshift or swap 6 lshift or $1E00 or h,
    ;

    \ Assemble a one-register SUBS immediate instruction
    : subs_,#_ ( imm8 rdn -- )
      dup validate-3reg over 256 u< averts x-out-of-range-imm
      8 lshift or $3800 or h,
    ;

    \ Assemble a three-register SUBS register instruction
    : subs_,_,_ ( rm rn rd -- )
      3dup validate-3-3reg swap 3 lshift or swap 6 lshift or $1A00 or h,
    ;

    \ Assemble an SUB (SP plus immediate to SP) instruction
    : subsp,sp,#_ ( imm7 -- )
      dup validate-imm-4align dup 512 u< averts x-out-of-range-imm
      2 rshift $B080 or h,
    ;

    \ Assemble an SVC instruction
    : svc#_ ( imm8 -- )
      dup 256 u< averts x-out-of-range-imm
      $BF00 or h,
    ;

    \ Assemble an SXTB instruction
    : sxtb_,_ ( rm rd -- )
      2dup validate-2-3reg swap 3 lshift or $B240 or h,
    ;

    \ Assemble an SXTH instruction
    : sxth_,_ ( rm rd -- )
      2dup validate-2-3reg swap 3 lshift or $B200 or h,
    ;

    \ Assemble an tST (register) instruction
    : tst_,_ ( rm rn -- )
      2dup validate-2-3reg swap 3 lshift or $4200 or h,
    ;

    \ Assemble a UDF instruction
    : udf#_ ( imm8 -- )
      dup 256 u< averts x-out-of-range-imm
      $DE00 or h,
    ;

    \ Assemble a 32-bit UDF instruction
    : udf.w#_ ( imm16 -- )
      dup 65536 u< averts x-out-of-range-imm
      dup 12 rshift $F and $F7F0 or h,
      $FFF and $A000 or h,
    ;

    \ Assemble an UXTB instruction
    : uxtb_,_ ( rm rd -- )
      2dup validate-2-3reg swap 3 lshift or $B2C0 or h,
    ;

    \ Assemble an UXTH instruction
    : uxth_,_ ( rm rd -- )
      2dup validate-2-3reg swap 3 lshift or $B280 or h,
    ;

    \ Assemble a WFE instruction
    : wfe ( -- ) $BF20 h, ;

    \ Assemble a WFI instruction
    : wfi ( -- ) $BF30 h, ;

    \ Assemble a YIELD instruction
    : yield ( -- ) $BF10 h, ;

    \ Mark a backward destination
    : mark> ( -- mark-addr mark ) here mark-dest ;

    \ Mark a forward destination
    : >mark ( mark-addr mark -- )
      dup mark-type case
	mark-adr of resolve-adr endof
	mark-ldr of resolve-ldr endof
	mark-b of resolve-b endof
	mark-bc of resolve-bc endof
	['] x-incorrect-mark-type ?raise
      endcase
    ;
    
  end-module

  \ Begin an assembly block
  : code[ [immediate] undefer-lit armv6m-instr import postpone [ ;

  \ End an assembly block
  : ]code [immediate] armv6m-instr unimport postpone ] ;
  
end-module