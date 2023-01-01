\ Copyright (c) 2022-2023 Travis Bemann
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

begin-module armv6m-test

  armv6m import
  
  \ Verification function
  : verify ( flag c-addr u -- ) cr type ." : " if ." PASS" else ." FAIL" then ;
  
  \ ADDS TOS, TOS, #1 test
  : test-adds-tos-tos-#1 ( -- )
    1 code[
    1 tos tos adds_,_,#_
    ]code 2 = s" ADDS TOS, TOS, #1" verify
  ;

  \ ADDS TOS, #1 test
  : test-adds-tos-#1 ( -- )
    2 code[
    1 tos adds_,#_
    ]code 3 = s" ADDS TOS, #1" verify
  ;

  \ ADDS TOS, R0, R1 test
  : test-adds-tos-r0-r1 ( -- )
    0 code[
    1 r0 movs_,#_
    2 r1 movs_,#_
    r1 r0 tos adds_,_,_
    ]code 3 = s" ADDS TOS, R0, R1" verify
  ;

  \ ADDS TOS, R12 test
  : test-adds-tos-r12 ( -- )
    1 code[
    r12 r1 mov4_,4_
    2 r0 movs_,#_
    r0 r12 mov4_,4_
    r12 tos add4_,4_
    r1 r12 mov4_,4_
    ]code 3 = s" ADDS TOS, R12" verify
  ;
  
  \ ADCS TOS, R0 test
  : test-adcs-tos-r0 ( -- )
    0 code[
    0 r0 movs_,#_
    0 r1 movs_,#_
    r1 r1 mvns_,_
    1 r1 r1 adds_,_,#_
    r0 tos adcs_,_
    ]code 1 = s" ADCS TOS, R0" verify
  ;

  \ SUBS TOS, TOS, #1 test
  : test-subs-tos-tos-#1 ( -- )
    3 code[
    1 tos tos subs_,_,#_
    ]code 2 = s" SUBS TOS, TOS, #1" verify
  ;

  \ SUBS TOS, #1 test
  : test-subs-tos-#1 ( -- )
    3 code[
    1 tos subs_,#_
    ]code 2 = s" SUBS TOS, #1" verify
  ;

  \ SUBS TOS, R0, R1 test
  : test-subs-tos-r0-r1 ( -- )
    0 code[
    3 r0 movs_,#_
    1 r1 movs_,#_
    r1 r0 tos subs_,_,_
    ]code 2 = s" SUBS TOS, R0, R1" verify
  ;

  \ SBCS TOS, R0 test
  : test-sbcs-tos-r0 ( -- )
    0 code[
    0 r0 movs_,#_
    0 r1 movs_,#_
    1 r1 r1 subs_,_,#_
    r0 tos sbcs_,_
    ]code -1 = s" SBCS TOS, R0" verify
  ;

  \ ANDS TOS, R0 test
  : test-ands-tos-r0 ( -- )
    $FC0 code[
    $FF r0 movs_,#_
    r0 tos ands_,_
    ]code $C0 = s" ANDS TOS, R0" verify
  ;

  \ ORRS TOS, R0 test
  : test-orrs-tos-r0 ( -- )
    $F0 code[
    $0F r0 movs_,#_
    r0 tos orrs_,_
    ]code $FF = s" ORRS TOS, R0" verify
  ;

  \ EORS TOS, R0 test
  : test-eors-tos-r0 ( -- )
    $F0 code[
    $3F r0 movs_,#_
    r0 tos eors_,_
    ]code $CF = s" EORS TOS, R0" verify
  ;

  \ BICS TOS, R0 test
  : test-bics-tos-r0 ( -- )
    $FF code[
    $0F r0 movs_,#_
    r0 tos bics_,_
    ]code $F0 = s" BICS TOS, R0" verify
  ;

  \ LSLS TOS, R0 test
  : test-lsls-tos-r0 ( -- )
    1 code[
    1 r0 movs_,#_
    r0 tos lsls_,_
    ]code 2 = s" LSLS TOS, R0" verify
  ;

  \ LSRS TOS, R0 test
  : test-lsrs-tos-r0 ( -- )
    $80000000 code[
    1 r0 movs_,#_
    r0 tos lsrs_,_
    ]code $40000000 = s" LSRS TOS, R0" verify
  ;

  \ ASRS TOS, R0 test
  : test-asrs-tos-r0 ( -- )
    $80000000 code[
    1 r0 movs_,#_
    r0 tos asrs_,_
    ]code $C0000000 = s" ASRS TOS, R0" verify
  ;

  \ RORS TOS, TOS, R0 test
  : test-rors-tos-r0 ( -- )
    $80000001 code[
    1 r0 movs_,#_
    r0 tos rors_,_
    ]code $C0000000 = s" RORS TOS, R0" verify
  ;

  \ LSLS TOS, TOS, #1 test
  : test-lsls-tos-tos-#1 ( -- )
    1 code[
    1 tos tos lsls_,_,#_
    ]code 2 = s" LSLS TOS, TOS, #1" verify
  ;

  \ LSRS TOS, TOS, #1 test
  : test-lsrs-tos-tos-#1 ( -- )
    $80000000 code[
    1 tos tos lsrs_,_,#_
    ]code $40000000 = s" LSRS TOS, TOS, #1" verify
  ;

  \ ASRS TOS, TOS, #1 test
  : test-asrs-tos-tos-#1 ( -- )
    $80000000 code[
    1 tos tos asrs_,_,#_
    ]code $C0000000 = s" ASRS TOS, TOS, #1" verify
  ;

  \ MULS TOS, R0 test
  : test-muls-tos-r0 ( -- )
    2 code[
    3 r0 movs_,#_
    r0 tos muls_,_
    ]code 6 = s" MULS TOS, R0" verify
  ;

  \ MVNS TOS, R0 test
  : test-mvns-tos-r0 ( -- )
    2 code[
    0 r0 movs_,#_
    r0 tos mvns_,_
    ]code -1 = s" MVNS TOS, R0" verify
  ;

  \ REV TOS, TOS test
  : test-rev-tos-tos ( -- )
    $04030201 code[
    tos tos rev_,_
    ]code $01020304 = s" REV TOS, TOS" verify
  ;

  \ REV16 TOS, TOS test
  : test-rev16-tos-tos ( -- )
    $04030201 code[
    tos tos rev16_,_
    ]code $03040102 = s" REV16 TOS, TOS" verify
  ;

  \ REVSH TOS, TOS test
  : test-revsh-tos-tos ( -- )
    $0000FFFE code[
    tos tos revsh_,_
    ]code $FFFFFEFF = s" REVSH TOS, TOS" verify
  ;

  \ RSBS TOS, R0, #0 test
  : test-rsbs-tos-r0-#0 ( -- )
    0 code[
    1 r0 movs_,#_
    r0 tos rsbs_,_,#0
    ]code -1 = s" RSBS TOS, R0, #0" verify
  ;

  \ SXTB TOS, TOS test
  : test-sxtb-tos-tos ( -- )
    $0000FCF0 code[
    tos tos sxtb_,_
    ]code $FFFFFFF0 = s" SXTB TOS, TOS" verify
  ;

  \ SXTH TOS, TOS test
  : test-sxth-tos-tos ( -- )
    $00FCF8F0 code[
    tos tos sxth_,_
    ]code $FFFFF8F0 = s" SXTH TOS, TOS" verify
  ;

  \ UXTB TOS, TOS test
  : test-uxtb-tos-tos ( -- )
    $0000FCF0 code[
    tos tos uxtb_,_
    ]code $000000F0 = s" UXTB TOS, TOS" verify
  ;

  \ UXTH TOS, TOS test
  : test-uxth-tos-tos ( -- )
    $00FCF8F0 code[
    tos tos uxth_,_
    ]code $0000F8F0 = s" UXTH TOS, TOS" verify
  ;

  \ NOP test
  : test-nop ( -- )
    1 code[
    nop
    ]code 1 = s" NOP" verify
  ;

  \ MOV TOS, R12 test
  : test-mov-tos-r12 ( -- )
    0 code[
    1 r0 movs_,#_
    r0 r12 mov4_,4_
    r12 tos mov4_,4_
    ]code 1 = s" MOV TOS, R12" verify
  ;

  2 cells buffer: test-buffer

  \ STR and LDR immediate test
  : test-str-ldr-imm ( -- )
    test-buffer code[
    1 r0 movs_,#_
    4 tos r0 str_,[_,#_]
    2 r0 movs_,#_
    4 tos r0 str_,[_,#_]
    4 tos tos ldr_,[_,#_]
    ]code 2 = s" STR AND LDR IMMEDIATE" verify
  ;

  \ STRB and LDRB immediate test
  : test-strb-ldrb-imm ( -- )
    test-buffer code[
    1 r0 movs_,#_
    4 tos r0 strb_,[_,#_]
    2 r0 movs_,#_
    4 tos r0 strb_,[_,#_]
    4 tos tos ldrb_,[_,#_]
    ]code 2 = s" STRB AND LDRB IMMEDIATE" verify
  ;

  \ STRH and LDRH immediate test
  : test-strh-ldrh-imm ( -- )
    test-buffer code[
    1 r0 movs_,#_
    4 tos r0 strh_,[_,#_]
    2 r0 movs_,#_
    4 tos r0 strh_,[_,#_]
    4 tos tos ldrh_,[_,#_]
    ]code 2 = s" STRH AND LDRH IMMEDIATE" verify
  ;

  \ STR and LDR register test
  : test-str-ldr-reg ( -- )
    test-buffer code[
    1 r0 movs_,#_
    4 r1 movs_,#_
    r1 tos r0 str_,[_,_]
    2 r0 movs_,#_
    r1 tos r0 str_,[_,_]
    r1 tos tos ldr_,[_,_]
    ]code 2 = s" STR AND LDR REGISTER" verify
  ;

  \ STRB and LDRB register test
  : test-strb-ldrb-reg ( -- )
    test-buffer code[
    1 r0 movs_,#_
    4 r1 movs_,#_
    r1 tos r0 strb_,[_,_]
    2 r0 movs_,#_
    r1 tos r0 strb_,[_,_]
    r1 tos tos ldrb_,[_,_]
    ]code 2 = s" STRB AND LDRB REGISTER" verify
  ;

  \ STRH and LDRH register test
  : test-strh-ldrh-reg ( -- )
    test-buffer code[
    1 r0 movs_,#_
    4 r1 movs_,#_
    r1 tos r0 strh_,[_,_]
    2 r0 movs_,#_
    r1 tos r0 strh_,[_,_]
    r1 tos tos ldrh_,[_,_]
    ]code 2 = s" STRH AND LDRH REGISTER" verify
  ;

  \ STRB and LDRSB register test
  : test-strb-ldrsb-reg ( -- )
    test-buffer code[
    1 r0 movs_,#_
    4 r1 movs_,#_
    r1 tos r0 strb_,[_,_]
    $F0 r0 movs_,#_
    r1 tos r0 strb_,[_,_]
    r1 tos tos ldrsb_,[_,_]
    ]code $FFFFFFF0 = s" STRB AND LDRSB REGISTER" verify
  ;

  \ STRH and LDRH register test
  : test-strh-ldrsh-reg ( -- )
    test-buffer code[
    1 r0 movs_,#_
    4 r1 movs_,#_
    r1 tos r0 strh_,[_,_]
    $F0 r0 movs_,#_
    $F1 r2 movs_,#_
    8 r2 r2 lsls_,_,#_
    r2 r0 orrs_,_
    r1 tos r0 strh_,[_,_]
    r1 tos tos ldrsh_,[_,_]
    ]code $FFFFF1F0 = s" STRH AND LDRSH REGISTER" verify
  ;

  \ Simple CMP loop test
  : test-simple-cmp-loop ( -- )
    16 code[
    0 r0 movs_,#_
    mark>
    2 r0 adds_,#_
    1 tos subs_,#_
    0 tos cmp_,#_
    ne bc<
    r0 tos movs_,_
    ]code 32 = s" SIMPLE CMP LOOP" verify
  ;

  \ Register CMP loop test
  : test-reg-cmp-loop ( -- )
    16 code[
    0 r0 movs_,#_
    mark>
    2 r0 adds_,#_
    1 tos subs_,#_
    0 r1 movs_,#_
    r1 tos cmp_,_
    ne bc<
    r0 tos movs_,_
    ]code 32 = s" REGISTER CMP LOOP" verify
  ;

  \ 4-bit register CMP loop test
  : test-reg4-cmp-loop ( -- )
    16 code[
    0 r0 movs_,#_
    r12 r2 mov4_,4_
    mark>
    2 r0 adds_,#_
    1 tos subs_,#_
    0 r1 movs_,#_
    r1 r12 mov4_,4_
    r12 tos cmp4_,4_
    ne bc<
    r2 r12 mov4_,4_
    r0 tos movs_,_
    ]code 32 = s" REGISTER4 CMP LOOP" verify
  ;

  \ Register TST loop test
  : test-reg-tst-loop ( -- )
    16 code[
    0 r0 movs_,#_
    mark>
    2 r0 adds_,#_
    1 tos subs_,#_
    $FF r1 movs_,#_
    r1 tos tst_,_
    ne bc<
    r0 tos movs_,_
    ]code 32 = s" REGISTER TST LOOP" verify
  ;
  
  \ Register CMN loop test
  : test-reg-cmn-loop ( -- )
    16 code[
    0 r0 movs_,#_
    mark>
    2 r0 adds_,#_
    1 tos subs_,#_
    0 r1 movs_,#_
    r1 tos cmn_,_
    ne bc<
    r0 tos movs_,_
    ]code 32 = s" REGISTER CMN LOOP" verify
  ;

  \ Simple backward unconditional branch test
  : test-b-backward ( -- )
    1 [:
      code[
      mark>
      0 tos cmp_,#_
      ne bc>
      pc 1 pop
      >mark
      0 tos movs_,#_
      b<
      ]code
    ;] execute 0 = s" B BACKWARD" verify
  ;

  \ Simple forward unconditional branch test
  : test-b-forward ( -- )
    1 code[
    b>
    2 tos movs_,#_
    >mark
    ]code 1 = s" B FORWARD" verify
  ;

  \ Simple forward conditional branch test
  : test-bc-forward ( -- )
    1 code[
    0 r0 movs_,#_
    eq bc>
    2 tos movs_,#_
    >mark
    ]code 1 = s" BC FOWARD" verify
  ;

  \ ADD R12, SP, R12 test
  : test-add-r12-sp-r12 ( -- )
    0 code[
    1 r0 movs_,#_
    2 r1 movs_,#_
    4 r2 movs_,#_
    r12 r3 mov4_,4_
    r0 r1 r2 r3 4 push
    r2 r12 mov4_,4_
    r12 add4_,sp
    r12 tos mov4_,4_
    0 tos tos ldr_,[_,#_]
    16 addsp,sp,#_
    r3 r12 mov4_,4_
    ]code 2 = s" ADD R12, SP, R12" verify
  ;

  \ ADD TOS, SP, #4 test
  : test-add-tos-sp-#4 ( -- )
    0 code[
    1 r0 movs_,#_
    2 r1 movs_,#_
    3 r2 movs_,#_
    4 r3 movs_,#_
    r0 r1 r2 r3 4 push
    4 tos add_,sp,#_
    0 tos tos ldr_,[_,#_]
    16 addsp,sp,#_
    ]code 2 = s" ADD TOS, SP, #4" verify
  ;

  \ LDR TOS, [SP, #4] test
  : test-ldr-tos-sp-#4 ( -- )
    0 code[
    1 r0 movs_,#_
    2 r1 movs_,#_
    3 r2 movs_,#_
    4 r3 movs_,#_
    r0 r1 r2 r3 4 push
    4 tos ldr_,[sp,#_]
    16 r0 movs_,#_
    r0 addsp,sp,4_
    ]code 2 = s" LDR TOS, [SP, #4]" verify
  ;

  \ STR R0, [SP, #4] test
  : test-str-r0-sp-#4 ( -- )
    0 code[
    8 r0 movs_,#_
    16 subsp,sp,#_
    4 r0 str_,[sp,#_]
    4 tos ldr_,[sp,#_]
    16 r0 movs_,#_
    r0 addsp,sp,4_
    ]code 8 = s" STR R0, [SP, #4]" verify
  ;

  \ LDR TOS, [PC, #_] test
  : test-ldr-tos-pc-# ( -- )
    0 [:
      code[
      tos ldr_,[pc]
      pc 1 pop
      >mark $7FFFFFFF ,
      ]code
    ;] execute $7FFFFFFF = s" LDR TOS, [PC, #_]" verify
  ;

  \ ADR test
  : test-adr-tos ( -- )
    0 [:
      code[
      tos adr_
      0 tos tos ldr_,[_,#_]
      pc 1 pop
      >mark $7FFFFFFF ,
      ]code
    ;] execute $7FFFFFFF = s" ADR TOS" verify
  ;

  \ Simple PUSH and POP test
  : test-push-pop ( -- )
    1 code[
    2 r0 movs_,#_
    3 r1 movs_,#_
    4 r2 movs_,#_
    5 r3 movs_,#_
    r3 r2 r1 r0 4 push
    ]code
    s" " type
    code[
    r3 r2 r1 r0 4 pop
    r0 tos tos adds_,_,_
    r1 tos tos adds_,_,_
    r2 tos tos adds_,_,_
    r3 tos tos adds_,_,_
    ]code 15 = s" PUSH POP" verify
  ;

  \ Simple STM and LDM test
  : test-stm-ldm ( -- )
    here code[
    tos r0 movs_,_
    2 r1 movs_,#_
    4 r2 movs_,#_
    8 r3 movs_,#_
    r1 r2 r3 3 r0 stm
    tos r0 movs_,_
    0 r1 movs_,#_
    0 r2 movs_,#_
    0 r3 movs_,#_
    r1 r2 r3 3 r0 ldm
    r2 r1 tos adds_,_,_
    r3 tos tos adds_,_,_
    ]code 14 = s" STM LDM" verify
  ;

  \ BLX TOS test
  : test-blx-tos ( -- )
    0 [: 2drop -1 ;] [:
      code[
      1 tos tos adds_,_,#_
      tos blx_
      ]code
    ;] execute -1 = s" BLX TOS" verify
  ;

  \ BX TOS test
  : test-bx-tos ( -- )
    0 [: 2drop -1 ;] [:
      code[
      4 addsp,sp,#_
      1 tos tos adds_,_,#_
      tos bx_
      ]code
    ;] execute -1 = s" BX TOS" verify
  ;

  \ Miscellaneous instructions test
  : test-misc ( -- )
    0 code[
    cpsie
    cpsid
    dmb
    dsb
    isb
    ]code 0 = s" MISC" verify
  ;

  \ Run tests
  : run-tests ( -- )
    test-adds-tos-tos-#1
    test-adds-tos-#1
    test-adds-tos-r0-r1
    test-adds-tos-r12
    test-adcs-tos-r0
    test-subs-tos-tos-#1
    test-subs-tos-#1
    test-subs-tos-r0-r1
    test-sbcs-tos-r0
    test-ands-tos-r0
    test-orrs-tos-r0
    test-eors-tos-r0
    test-bics-tos-r0
    test-lsls-tos-r0
    test-lsrs-tos-r0
    test-asrs-tos-r0
    test-rors-tos-r0
    test-lsls-tos-tos-#1
    test-lsrs-tos-tos-#1
    test-asrs-tos-tos-#1
    test-muls-tos-r0
    test-mvns-tos-r0
    test-rev-tos-tos
    test-rev16-tos-tos
    test-revsh-tos-tos
    test-rsbs-tos-r0-#0
    test-sxtb-tos-tos
    test-sxth-tos-tos
    test-uxtb-tos-tos
    test-uxth-tos-tos
    test-nop
    test-mov-tos-r12
    test-str-ldr-imm
    test-strb-ldrb-imm
    test-strh-ldrh-imm
    test-str-ldr-reg
    test-strb-ldrb-reg
    test-strh-ldrh-reg
    test-strb-ldrsb-reg
    test-strh-ldrsh-reg
    test-simple-cmp-loop
    test-reg-cmp-loop
    test-reg4-cmp-loop
    test-reg-tst-loop
    test-reg-cmn-loop
    test-b-backward
    test-b-forward
    test-bc-forward
    test-add-r12-sp-r12
    test-add-tos-sp-#4
    test-ldr-tos-sp-#4
    test-str-r0-sp-#4
    test-ldr-tos-pc-#
    test-adr-tos
    test-push-pop
    test-stm-ldm
    test-blx-tos
    test-bx-tos
    test-misc
  ;
  
end-module
