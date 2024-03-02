# ARMv6-M Assembler Words

zeptoforth includes an ARMv6-M assembler, which can be used on all platforms, whether ARMv6-M or ARMv7-M. It supports all ARMv6-M instructions except for BL, as it is expected that the user will drop out of the assembler to call Forth words, which will be compiled to BL. It does not make use of labels as many assemblers do, but rather uses "marks", which are double-cells on the data stack, for forward and backward references.

### `armv6m`

The following words are in the `armv6m` module:

##### `code[`
( -- )

Begin a block of assembled code, making the assembler wordlist available within and forcing any deferred literal to be compiled in place.

##### `]code`
( -- )

End a block of assembled code, removing the assembler wordlist from the wordlist order.

##### `x-out-of-range-3reg`
( -- )

Out of range 3-bit register exception.

##### `x-out-of-range-4reg`
( -- )

Out of range 4-bit register exception.

##### `x-out-of-range-imm`
( -- )

Out of range immediate exception.

##### `x-unaligned-imm`
( -- )

Unaligned immediate exception.

##### `x-out-of-range-pc-rel`
( -- )

Out of range PC-relative address exception.

##### `x-incorrect-mark-type`
( -- )

Incorrect marker type exception.

##### `x-invalid-cond`
( -- )

Invalid condition exception.

##### `x-out-of-range-special`
( -- )

Out of range special register exception.

### Assembler words

The following words are accessible between `code[` and `]code`:


#### Registers

##### `r0`
##### `r1`
##### `r2`
##### `r3`
##### `r4`
##### `r5`
##### `r6`
##### `r7`
##### `r8`
##### `r9`
##### `r10`
##### `r11`
##### `r12`
##### `r13`
##### `r14`
##### `r15`
##### `tos`
##### `dp`
##### `sp`
##### `lr`
##### `pc`

#### Condition codes
##### `eq`
##### `ne`
##### `cs`
##### `hs`
##### `cc`
##### `lo`
##### `mi`
##### `pl`
##### `vs`
##### `vc`
##### `hi`
##### `ls`
##### `ge`
##### `lt`
##### `gt`
##### `le`
##### `al`

##### `adcs_,_`
( rm rdn -- )

Assemble an ADCS instruction.

##### `adds_,_,#_`
( imm3 rn rd -- )

Assemble a two-register ADDS immediate instruction.

##### `adds_,#_`
( imm8 rdn -- )

Assemble a one-register ADDS immediate instruction.

##### `adds_,_,_`
( rm rn rd -- )

Assemble a three-register ADDS register instruction.

##### `add4_,4_`
( rm4 rdn4 -- )

Assemble a two-4-bit register ADDS register instruction.

##### `add_,sp,#_`
( imm8 rd -- )

Assemble an ADD (SP plus immediate) instruction.

##### `addsp,sp,#_`
( imm7 -- )

Assemble an ADD (SP plus immediate to SP) instruction.

##### `add4_,sp`
( rdm4 -- )

Assemble an ADD (SP plus register) instruction.

##### `addsp,sp,4_`
( rm4 -- )

Assemble an ADD (SP plus register to SP) instruction.

##### `adr_`
( rd -- mark-addr mark )

Mark an ADR instruction.

##### `ands_,_`
( rm rdn -- )

Assemble an ANDS (register) instruction.

##### `asrs_,_,#_`
( imm5 rm rd -- )

Assemble an ASRS (immediate) instruction.

##### `asrs_,_`
( rm rdn -- )

Assemble an ASRS (register) instruction.

##### `b<`
( mark-addr mark -- )

Assemble an unconditional branch to a marker.

##### `bc<`
( mark-addr mark cond -- )

Assemble a conditional branch to a marker.

##### `b>`
( -- mark-addr mark )

Mark an unconditional branch.

##### `bc>`
( cond -- mark-addr mark )

Mark a conditional branch.

##### `bics_,_`
( rm rdn -- )

Assemble an BICS (register) instruction.

##### `bkpt#_`
( imm8 -- )

Assemble a BKPT instruction.

##### `blx_`
( rm -- )

Assemble a BLX (register) instruction.

##### `bx_`
( rm -- )

Assemble a BX instruction.

##### `cmn_,_`
( rm rn -- )

Assemble an CMN (register) instruction.

##### `cmp_,#_`
( imm8 rn -- )

Assemble a CMP (immediate) instruction.

##### `cmp_,_`
( rm rn -- )

Assemble an CMP (register) instruction.

##### `cmp4_,4_`
( rm4 rdn4 -- )

Assemble an CMP (register) instruction.

##### `cpsie`
( -- )

Assemble a CPSIE instruction.

##### `cpsid`
( -- )

Assemble a CPSID instruction.

##### `dmb`
( -- )

Assemble a DMB instruction.

##### `dsb`
( -- )

Assemble a DSB instruction.

##### `eors_,_`
( rm rdn -- )

Assemble an EORS (register) instruction.

##### `isb`
( -- )

Assemble an ISB instruction.

##### `ldm`
( rx ... r0 count rn -- )

Assemble an LDM instruction.

##### `ldr_,[_,#_]`
( imm5 rn rt -- )

Assemble an LDR (immediate) instruction.

##### `ldr_,[sp,#_]`
( imm8 rt -- )

Assemble an LDR (immediate) instruction.

##### `ldr_,[pc]`
( rd -- mark-addr mark )

Mark an LDR (literal) instruction.

##### `ldr_,[_,_]`
( rm rn rt -- )

Assemble an LDR (register) instruction.

##### `ldrb_,[_,#_]`
( imm5 rn rt -- )

Assemble an LDRB (immediate) instruction.

##### `ldrb_,[_,_]`
( rm rn rt -- )

Assemble an LDRB (register) instruction.

##### `ldrh_,[_,#_]`
( imm5 rn rt -- )

Assemble an LDRH (immediate) instruction.

##### `ldrh_,[_,_]`
( rm rn rt -- )

Assemble an LDRH (register) instruction.

##### `ldrsb_,[_,_]`
( rm rn rt -- )

Assemble an LDRSB (register) instruction.

##### `ldrsh_,[_,_]`
( rm rn rt -- )

Assemble an LDRSH (register) instruction.

##### `lsls_,_,#_`
( imm5 rm rd -- )

Assemble an LSLS (immediate) instruction.

##### `lsls_,_`
( rm rdn -- )

Assemble an LSLS (register) instruction.

##### `lsrs_,_,#_`
( imm5 rm rd -- )

Assemble an LSRS (immediate) instruction.

##### `lsrs_,_`
( rm rdn -- )

Assemble an LSRS (register) instruction.

##### `movs_,#_`
( imm8 rdn -- )

Assemble a MOVS (immediate) instruction.

##### `mov4_,4_`
( rm4 rdn4 -- )

Assemble a MOV (register) instruction.

##### `movs_,_`
( rm rd -- )

Assemble a MOVS (register) instruction.

##### `mrs_,_`
( sysm rd -- )

Assemble an MRS instruction.

##### `msr_,_`
( rn sysm -- )

Assemble an MSR (register) instruction.
    
##### `muls_,_`
( rn rdm -- )

Assemble an MULS (register) instruction.

##### `mvns_,_`
( rm rd -- )

Assemble an MVNS (register) instruction.

##### `nop`
( -- )

Assemble a NOP instruction.

##### `orrs_,_`
( rm rdn -- )

Assemble an ORRS (register) instruction.

##### `pop`
( rx ... r0 count -- )

Assemble a POP instruction.

##### `push`
( rx ... r0 count -- )

Assemble a PUSH instruction.

##### `rev_,_`
( rm rd -- )

Assemble an REV (register) instruction.

##### `rev16_,_`
( rm rd -- )

Assemble an REV16 (register) instruction.

##### `revsh_,_`
( rm rd -- )

Assemble an REVSH (register) instruction.

##### `rors_,_`
( rm rdn -- )

Assemble an RORS (register) instruction.

##### `rsbs_,_,#0`
( rn rd -- )

Assemble an RSBS (immediate) instruction.

##### `sbcs_,_`
( rm rdn -- )

Assemble an SBC (register) instruction.

##### `sev`
( -- )

Assemble an SEV instruction.

##### `stm`
( rx ... r0 count rn -- )

Assemble an STM instruction.

##### `str_,[_,#_]`
( imm5 rn rt -- )

Assemble an STR (immediate) instruction.

##### `str_,[sp,#_]`
( imm8 rt -- )

Assemble an STR (immediate) instruction.

##### `str_,[_,_]`
( rm rn rt -- )

Assemble an STR (register) instruction.
    
##### `strb_,[_,#_]`
( imm5 rn rt -- )

Assemble an STRB (immediate) instruction.

##### `strb_,[_,_]`
( rm rn rt -- )

Assemble an STRB (register) instruction.

##### `strh_,[_,#_]`
( imm5 rn rt -- )

Assemble an STRH (immediate) instruction.

##### `strh_,[_,_]`
( rm rn rt -- )

Assemble an STRH (register) instruction.

##### `subs_,_,#_`
( imm3 rn rd -- )

Assemble a two-register SUBS immediate instruction.

##### `subs_,#_`
( imm8 rdn -- )

Assemble a one-register SUBS immediate instruction.

##### `subs_,_,_`
( rm rn rd -- )

Assemble a three-register SUBS register instruction.

##### `subsp,sp,#_`
( imm7 -- )

Assemble an SUB (SP plus immediate to SP) instruction.

##### `svc#_`
( imm8 -- )

Assemble an SVC instruction.

##### `sxtb_,_`
( rm rd -- )

Assemble an SXTB instruction.

##### `sxth_,_`
( rm rd -- )

Assemble an SXTH instruction.

##### `tst_,_`
( rm rn -- )

Assemble an tST (register) instruction.

##### `udf#_`
( imm8 -- )

Assemble a UDF instruction.

##### `udf.w#_`
( imm16 -- )

Assemble a 32-bit UDF instruction.

##### `uxtb_,_`
( rm rd -- )

Assemble an UXTB instruction.

##### `uxth_,_`
( rm rd -- )

Assemble an UXTH instruction.

##### `wfe`
( -- )

Assemble a WFE instruction.

##### `wfi`
( -- )

Assemble a WFI instruction.

##### `yield`
( -- )

Assemble a YIELD instruction.

##### `mark<`
( -- mark-addr mark )

Mark a backward destination.

##### `mark>`
( -- mark-addr mark )

The old way to mark a backward destination (now deprecated).

##### `>mark`
( mark-addr mark -- )

Mark a forward destination.

