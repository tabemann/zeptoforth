/* Copyright (c) 2022 Travis Bemann
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE. */

#include <stdlib.h>
#include <stdio.h>
#include <stdint.h>

#include "emulator.h"

extern state_t state;

/* Advance the PC by two */
#define ADVANCE_PC_2 (state.registers[PC] += 2)

/* Instruction decoding defines */
#define BF_0_3(instr) (instr & 0x7)
#define BF_3_3(instr) ((instr >> 3) & 0x7)
#define BF_6_3(instr) ((instr >> 6) & 0x7)
#define BF_8_3(instr) ((instr >> 8) & 0x7)
#define BF_0_7(instr) (instr & 0x7F)
#define BF_0_8(instr) (instr & 0xFF)
#define BF_3_4(instr) ((isntr >> 3) & 0xF)

/* Decode an imm12 constant */
uint32_t decode_imm12(uint32_t constant) {
  constant = constant & 0xFFF;
  if(!(constant >> 10)) {
    switch(constant >> 8) {
    case 0x0:
      return constant;
    case 0x1:
      return (constant << 16);
    case 0x2:
      return ((constant << 24) | (constant << 8));
    case 0x3:
      return ((constant << 24) | (constant << 16) | (constant << 8) | constant);
    default:
      fprintf(stderr, "This should be unreachable!\n");
      exit(1);
    }
  } else {
    uint32_t value = (constant & 0x7F) | 0x80;
    uint32_t rot = (constant >> 7) & 0x1F;
    return (value >> rot) | (value << (32 - rot));
  }
}

/* Calculate overflow on addition */
uint32_t check_add_overflow(uint32_t result, uint32_t dest, uint32_t src) {
  int32_t signed_result = result;
  int32_t signed_dest = dest;
  int32_t signed_src = src;
  if((signed_dest < 0) && (signed_src < 0) && (signed_result > 0)) {
    return APSR_V;
  } else if((signed_dest > 0) && (signed_src > 0) && (signed_result < 0)) {
    return APSR_V;
  } else {
    return 0;
  }
}

/* Calculate overflow on subtraction */
uint32_t check_sub_oveflow(uint32_t result, uint32_t dest, uint32_t src) {
  int32_t signed_result = result;
  int32_t signed_dest = dest;
  int32_t signed_src = src;
  if((signed_dest < 0) && (signed_src > 0) && (signed_result > 0)) {
    return APSR_V;
  } else if((signed_dest > 0) && (signed_src < 0) && (signed_result < 0)) {
    return APSR_V;
  } else {
    return 0;
  }
}

/* 16-bit instructions */

void instr_adc_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint64_t temp = (uint64_t)dest_value + (uint64_t)src_value +
    (state.apsr & APSR_C ? 1 : 0);
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[dest] = result;
  state.apsr =
    ((int32_t)result < 0 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (temp >> 32 ? APSR_C : 0) |
    check_add_overflow(result, dest_value, src_value);
  ADVANCE_PC_2;
}

void instr_add_imm_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t imm = BF_6_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint64_t temp = (uint64_t)dest_value + (uint64_t)src_value + (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[dest] = result;
  state.apsr =
    ((int32_t)result < 0 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (temp >> 32 ? APSR_C : 0) |
    check_add_overflow(result, dest_value, src_value);
  ADVANCE_PC_2;
}

void instr_add_imm_2(uint16_t instr) {
  uint32_t dest = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  uint32_t dest_value = state.registers[dest];
  uint64_t temp = (uint64_t)dest_value + (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[dest] = result;
  state.apsr =
    ((int32_t)result < 0 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (temp >> 32 ? APSR_C : 0) |
    check_add_overflow(result, dest_value, src_value);
  ADVANCE_PC_2;
}

void instr_add_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src0 = BF_3_3(instr);
  uint32_t src1 = BF_6_3(instr);
  uint32_t src0_value = state.registers[src0];
  uint32_t src1_value = state.registers[src1];
  uint64_t temp = (uint64_t)src0_value + (uint64_t)src1_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[dest] = result;
  state.apsr =
    ((int32_t)result < 0 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (temp >> 32 ? APSR_C : 0) |
    check_add_overflow(result, src0_value, src1_value);
  ADVANCE_PC_2;
}

void instr_add_reg_2(uint16_t instr) {
  uint32_t dest = BF_0_3(instr) | ((instr >> 7) & 0x1);
  uint32_t src = BF_3_4(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint64_t temp = (uint64_t)dest_value + (uint64_t)src_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[dest] = result;
  state.apsr =
    ((int32_t)result < 0 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (temp >> 32 ? APSR_C : 0) |
    check_add_overflow(result, dest_value, src_value);
  ADVANCE_PC_2;
}

void instr_add_sp_imm_1(uint16_t instr) {
  uint32_t dest = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  state.registers[dest] = state.registers[SP] + imm;
  ADVANCE_PC_2;
}

void instr_add_sp_imm_2(uint16_t instr) {
  uint32_t imm = BF_0_7(instr);
  state.registers[SP] += imm;
  ADVANCE_PC_2;
}

void instr_add_sp_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr) | ((instr >> 7) & 0x1);
  state.registers[dest] += state.registers[SP];
  ADVANCE_PC_2;
}

void instr_add_sp_reg_2(uint16_t instr) {
  uint32_t src = BF_3_4(instr);
  state.registers[SP] += state.registers[dest];
  ADVANCE_PC_2;
}

void instr_adr_1(uint16_t instr) {
  uint32_t dest = BF_8_3(instr);
  uint32_t pc_value = ((state.registers[PC] + 1) | 3) + 1;
  uint32_t imm = BF_0_8(instr) << 2;
  state.registers[dest] = pc_value + imm;
  ADVANCE_PC_2;
}

void instr_and_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint32_t result = dest_value & src_value;
  state.registers[dest] = result;
  state.apsr =
    ((int32_t)result < 0 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0);
  ADVANCE_PC_2;
}

void instr_asr_imm_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_asr_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_b_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_b_2(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_bic_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_bkpt(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_blx_reg(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_bx(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_cbnz(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_cbz(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_cmn_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_cmp_imm_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_cmp_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_cmp_reg_2(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_cps_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_eor_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_ldmia_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_ldr_imm_1_w(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_ldr_imm_2_w(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_ldr_lit_1_w(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_ldr_reg_1_w(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_ldr_imm_1_b(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_ldr_reg_1_b(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_ldr_imm_1_h(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_ldr_reg_1_h(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_lsl_imm_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_lsl_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_lsr_imm_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_lsr_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_mov_imm_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_mov_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_mul_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_mvn_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_nop_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_orr_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_pop_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_push_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_rsb_imm_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_sbc_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_str_imm_1_w(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_str_imm_2_w(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_str_reg_1_w(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_str_imm_1_b(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_str_reg_1_b(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_str_imm_1_h(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_str_reg_1_h(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_sub_imm_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_sub_imm_2(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_sub_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_svc(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_tst_reg_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_wfi_1(uint16_t instr) {
  ADVANCE_PC_2;
}

/* Register the instruction set */
void register_instr_set(void) {
  register_handler(instr_adc_reg_1, 0xFFC0, 0x4140);
  register_handler(instr_add_imm_1, 0xFE00, 0x1C00);
  register_handler(instr_add_imm_2, 0xF800, 0x3000);
  register_handler(instr_add_reg_1, 0xFE00, 0x1800);
  register_handler(instr_add_reg_2, 0xFF00, 0x4400);
  register_handler(instr_add_sp_imm_1, 0xF800, 0xA800);
  register_handler(instr_add_sp_imm_2, 0xFF80, 0xB000);
  register_handler(instr_add_sp_reg_1, 0xFF78, 0x4468);
  register_handler(instr_add_sp_reg_2, 0xFF87, 0x4485);  
  register_handler(instr_adr_1, 0xF000, 0xA000);
  register_handler(instr_and_reg_1, 0xFFC0, 0x4000);
  register_handler(instr_asr_imm_1, 0xF800, 0x1000);
  register_handler(instr_asr_reg_1, 0xFFC0, 0x4100);
  register_handler(instr_b_1, 0xF000, 0xD000);
  register_handler(instr_b_2, 0xF800, 0xE000);
  register_handler(instr_bic_reg_1, 0xFFC0, 0x4380);
  register_handler(instr_bkpt, 0xFF00, 0xBE00);
  register_handler(instr_blx_reg, 0xFF80, 0x4780);
  register_handler(instr_bx, 0xFF80, 0x4700);
  register_handler(instr_cbnz, 0xFD00, 0xB900);
  register_handler(instr_cbz, 0xFD00, 0xB100);
  register_handler(instr_cmn_reg_1, 0xFFC0, 0x42C0);
  register_handler(instr_cmp_imm_1, 0xF800, 0x2800);
  register_handler(instr_cmp_reg_1, 0xFFC0, 0x4280);
  register_handler(instr_cmp_reg_2, 0xFF00, 0x4500);
  register_handler(instr_cps_1, 0xFFE8, 0xB660);
  register_handler(instr_eor_reg_1, 0xFFC0, 0x4040);
  register_handler(instr_ldmia_1, 0xF800, 0xC800);
  register_handler(instr_ldr_imm_1_w, 0xF800, 0x6800);
  register_handler(instr_ldr_imm_2_w, 0xF800, 0x9800);
  register_handler(instr_ldr_lit_1_w, 0xF800, 0x4800);
  register_handler(instr_ldr_reg_1_w, 0xFE00, 0x5800);
  register_handler(instr_ldr_imm_1_b, 0xF800, 0x7800);
  register_handler(instr_ldr_reg_1_b, 0xFE00, 0x5C00);
  register_handler(instr_ldr_imm_1_h, 0xF800, 0x8800);
  register_handler(instr_ldr_reg_1_h, 0xFE00, 0x5A00);
  /*  register_handler(instr_mov_reg_2, 0xFFC0, 0x0000); */
  register_handler(instr_lsl_imm_1, 0xF800, 0x0000);
  register_handler(instr_lsl_reg_1, 0xFFC0, 0x4080);
  register_handler(instr_lsr_imm_1, 0xF800, 0x0800);
  register_handler(instr_lsr_reg_1, 0xFFC0, 0x40C0);
  register_handler(instr_mov_imm_1, 0xF800, 0x2000);
  register_handler(instr_mov_reg_1, 0xFF00, 0x4600);
  register_handler(instr_mul_1, 0xFFC0, 0x4340);
  register_handler(instr_mvn_reg_1, 0xFFC0, 0x43C0);
  register_handler(instr_nop_1, 0xFFFF, 0xBF00);
  register_handler(instr_orr_reg_1, 0xFFC0, 0x4300);
  register_handler(instr_pop_1, 0xFE00, 0xBC00);
  register_handler(instr_push_1, 0xFE00, 0xB400);
  register_handler(instr_rsb_imm_1, 0xFFC0, 0x4240);
  register_handler(instr_sbc_reg_1, 0xFFC0, 0x4180);
  register_handler(instr_str_imm_1_w, 0xF800, 0x6000);
  register_handler(instr_str_imm_2_w, 0xF800, 0x9000);
  register_handler(instr_str_reg_1_w, 0xFE00, 0x5000);
  register_handler(instr_str_imm_1_b, 0xF800, 0x7000);
  register_handler(instr_str_reg_1_b, 0xFE00, 0x5400);
  register_handler(instr_str_imm_1_h, 0xF800, 0x8000);
  register_handler(instr_str_reg_1_h, 0xFE00, 0x5200);
  register_handler(instr_sub_imm_1, 0xFE00, 0x8E00);
  register_handler(instr_sub_imm_2, 0xF800, 0x3800);
  register_handler(instr_sub_reg_1, 0xFE00, 0x8A00);
  register_handler(instr_svc, 0xFF00, 0xDF00);
  register_handler(instr_tst_reg_1, 0xFFC0, 0x4200);
  register_handler(instr_wfi_1, 0xFFFF, 0xBF30);
}
