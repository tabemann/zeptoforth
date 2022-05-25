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
#define BF_0_3(instr) ((uint32_t)((instr) & 0x7))
#define BF_3_3(instr) ((uint32_t)(((instr) >> 3) & 0x7))
#define BF_6_3(instr) ((uint32_t)(((instr) >> 6) & 0x7))
#define BF_8_3(instr) ((uint32_t)(((instr) >> 8) & 0x7))
#define BF_0_7(instr) ((uint32_t)((instr) & 0x7F))
#define BF_0_8(instr) ((uint32_t)((instr) & 0xFF))
#define BF_0_11(instr) ((uint32_t)((instr) & 0x7FF))
#define BF_3_4(instr) ((uint32_t)(((instr) >> 3) & 0xF))
#define BF_6_5(instr) ((uint32_t)(((instr) >> 6) & 0x1F))
#define BF_8_4(instr) ((uint32_t)(((instr) >> 8) & 0xF))
#define BF_3_5(instr) ((uint32_t)(((instr) >> 3) & 0x1F))

/* The condition codes */
#define COND_EQ 0x0 /* Equal, Z set */
#define COND_NE 0x1 /* Not equal, Z clear */
#define COND_CS 0x2 /* Carry set, C set */
#define COND_CC 0x3 /* Carry clear, C clear */
#define COND_MI 0x4 /* Minus/negative, N set */
#define COND_PL 0x5 /* Plus/positve or zero, N clear */
#define COND_VS 0x6 /* Overflow, V set */
#define COND_VC 0x7 /* No overflow, V clear */
#define COND_HI 0x8 /* Unsigned higher, C set and Z clear */
#define COND_LS 0x9 /* Unsigned lower or same, C clear or Z set */
#define COND_GE 0xA /* Signed greater than or equal, N and V same */
#define COND_LT 0xB /* Signed less than, N and V differ */
#define COND_GT 0xC /* SIgned greater than, Z clear, N and V same */
#define COND_LE 0xD /* Signed less than or equal, Z set or N and V differ */
#define COND_AL 0xE /* Always, unconditional */
#define COND_OTHER 0xF /* Some other function */

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

/* Update the APSR for an ADD */
#define UPDATE_APSR_ADD(result, temp, src0_value, src1_value)	\
  (state.apsr =							\
   ((result) >> 31 ? APSR_N : 0) |				\
   ((result) == 0 ? APSR_Z : 0) |				\
   ((temp) >> 32 ? APSR_C : 0) |				\
   check_add_overflow((result), (src0_value), (src1_value)))

/* Update the APSR for an ADD */
#define UPDATE_APSR_SUB(result, temp, src0_value, src1_value)	\
  (state.apsr =							\
   ((result) >> 31 ? APSR_N : 0) |				\
   ((result) == 0 ? APSR_Z : 0) |				\
   ((temp) >> 32 ? APSR_C : 0) |				\
   check_sub_overflow((result), (src0_value), (src1_value)))

/* Update the APSR for bit operations */
#define UPDATE_APSR_BIT(result)		   \
  (state.apsr =				   \
   ((result) >> 31 ? APSR_N : 0) |	   \
   ((result) == 0 ? APSR_Z : 0) |	   \
   (state.apsr & (APSR_C | APSR_V)))

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
  UPDATE_APSR_ADD(result, temp, dest_value, src_value);
  ADVANCE_PC_2;
}

void instr_add_imm_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t imm = BF_6_3(instr);
  uint32_t src_value = state.registers[src];
  uint64_t temp = (uint64_t)src_value + (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[dest] = result;
  UPDATE_APSR_ADD(result, temp, src_value, imm);
  ADVANCE_PC_2;
}

void instr_add_imm_2(uint16_t instr) {
  uint32_t dest = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  uint32_t dest_value = state.registers[dest];
  uint64_t temp = (uint64_t)dest_value + (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[dest] = result;
  UPDATE_APSR_ADD(result, temp, dest_value, imm);
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
  UPDATE_APSR_ADD(result, temp, src0_value, src1_value);
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
  UPDATE_APSR_BIT(result);
  ADVANCE_PC_2;
}

void instr_asr_imm_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr);
  if(imm == 0) {
    imm == 32;
  }
  uint32_t src_value = state.registers[src];
  uint32_t result = (uint32_t)((int32_t)src_value >> imm);
  state.registers[dest] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    ((src_value >> (imm - 1)) & 0x1 ? APSR_C : 0) |
    (state.apsr & APSR_V);
  ADVANCE_PC_2;
}

void instr_asr_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint32_t result = (uint32_t)((int32_t)dest_value >> src_value);
  state.registers[dest] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (src_value ? ((dest_value >> (src_value - 1)) & 0x1 ? APSR_C : 0) : 0) |
    (state.apsr & APSR_V);
  ADVANCE_PC_2;
}

void instr_b_1(uint16_t instr) {
  uint32_t cond = BF_8_4(instr);
  uint32_t offset = (uint32_t)((int32_t)(BF_0_8(instr) << 24) >> 23);
  switch(cond) {
  case COND_EQ:
    state.registers[PC] += state.apsr & APSR_Z ? 4 + offset : 2;
    break;
  case COND_NE:
    state.registers[PC] += state.apsr & APSR_Z ? 2 : 4 + offset;
    break;
  case COND_CS:
    state.registers[PC] += state.apsr & APSR_C ? 4 + offset : 2;
    break;
  case COND_CC:
    state.registers[PC] += state.apsr & APSR_C ? 2 : 4 + offset;
    break;
  case COND_MI:
    state.registers[PC] += state.apsr & APSR_N ? 4 + offset : 2;
    break;
  case COND_PL:
    state.registers[PC] += state.apsr & APSR_N ? 2 : 4 + offset;
    break;
  case COND_VS:
    state.registers[PC] += state.apsr & APSR_V ? 4 + offset : 2;
    break;
  case COND_VC:
    state.registers[PC] += state.apsr & APSR_V ? 2 : 4 + offset;
    break;
  case COND_HI:
    state.registers[PC] += (state.apsr & APSR_C) && !(state.apsr & APSR_Z) ?
      4 + offset : 2;
    break;
  case COND_LS:
    state.registers[PC] += !(state.apsr & APSR_C) || (state.apsr & APSR_Z) ?
      4 + offset : 2;
    break;
  case COND_GE:
    state.registers[PC] +=
      ((state.apsr & APSR_N) && (state.apsr & APSR_V)) ||
      (!(state.apsr & APSR_N) && !(state.apsr & APSR_V)) ? 4 + offset : 2;
    break;
  case COND_LT:
    state.registers[PC] +=
      ((state.apsr & APSR_N) && !(state.apsr & APSR_V)) ||
      (!(state.apsr & APSR_N) && (state.apsr & APSR_V)) ? 4 + offset : 2;
    break;
  case COND_GT:
    state.registers[PC] +=
      !(state.apsr & APSR_Z) &&
      (((state.apsr & APSR_N) && (state.apsr & APSR_V)) ||
       (!(state.apsr & APSR_N) && !(state.apsr & APSR_V))) ? 4 + offset : 2;
    break;
  case COND_LE:
    state.registers[PC] +=
      (state.apsr & APSR_Z) ||
      (((state.apsr & APSR_N) && !(state.apsr & APSR_V)) ||
       (!(state.apsr & APSR_N) && (state.apsr & APSR_V))) ? 4 + offset : 2;
    break;
  case COND_OTHER:
    instr_svc(instr);
    break;
  default:
    fprintf(stderr, "PERMANENTLY UNDEFINED: %04X @ %08X\n",
	    (uint32_t)instr, state.registers[PC]);
    exit(1);
  }
}

void instr_b_2(uint16_t instr) {
  uint32_t offset = (uint32_t)((int32_t)(BF_0_11(instr) << 21) >> 20);
  state.registers[PC] += 4 + offset;
}

void instr_bic_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint32_t result = dest_value & ~src_value;
  state.registers[dest] = result;
  UPDATE_APSR_BIT(result);
  ADVANCE_PC_2;
}

void instr_bkpt(uint16_t instr) {
  fprintf(stderr, "BREAKPOINT: %04X @ %08X\n",
	  (uint32_t)instr, state.registers[PC]);
  exit(1);
}

void instr_blx_reg(uint16_t instr) {
  uint32_t src = BF_3_4(instr);
  uint32_t src_value = state.registers[src];
  if(!(src_value & 0x1)) {
    fprintf(stderr, "BAD BLX DESTINATION: %08X from R%d @ %08X\n",
	    src_value, src, state.registers[PC]);
    exit(1);
  }
  state.registers[LR] = (state.registers[PC] + 2) | 0x1;
  state.registers[PC] = src_value & ~0x1;
}

void instr_bx(uint16_t instr) {
  uint32_t src = BF_3_4(instr);
  uint32_t src_value = state.registers[src];
  if(!(src_value & 0x1)) {
    fprintf(stderr, "BAD BX DESTINATION: %08X from R%d @ %08X\n",
	    src_value, src, state.registers[PC]);
    exit(1);
  }
  state.registers[PC] = src_value & ~0x1;
}

void instr_cbnz(uint16_t instr) {
  uint32_t src = BF_0_3(instr);
  uint32_t src_value = state.registers[src];
  uint32_t offset_bits = ((instr & 0x200) >> 3) | BF_3_5(instr)
  uint32_t offset = (uint32_t)((int32_t)(offset_bits << 26) >> 25);
  state.registers[PC] += src_value ? 4 + offset : 2;
}

void instr_cbz(uint16_t instr) {
  uint32_t src = BF_0_3(instr);
  uint32_t src_value = state.registers[src];
  uint32_t offset_bits = ((instr & 0x200) >> 3) | BF_3_5(instr)
  uint32_t offset = (uint32_t)((int32_t)(offset_bits << 26) >> 25);
  state.registers[PC] += !src_value ? 4 + offset : 2;
}

void instr_cmn_reg_1(uint16_t instr) {
  uint32_t src0 = BF_0_3(instr);
  uint32_t src1 = BF_3_3(instr);
  uint32_t src0_value = state.registers[src0];
  uint32_t src1_value = state.registers[src1];
  uint64_t temp = (uint64_t)src0_value + (uint64_t)src1_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  UPDATE_APSR_ADD(result, temp, src0_value, src1_value);
  ADVANCE_PC_2;
}

void instr_cmp_imm_1(uint16_t instr) {
  uint32_t src = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  uint32_t src_value = state.registers[src];
  uint64_t temp = (uint64_t)src_value - (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  UPDATE_APSR_SUB(result, temp, src_value, imm);
  ADVANCE_PC_2;
}

void instr_cmp_reg_1(uint16_t instr) {
  uint32_t src0 = BF_0_3(instr);
  uint32_t src1 = BF_3_3(instr);
  uint32_t src0_value = state.registers[src0];
  uint32_t src1_value = state.registers[src1];
  uint64_t temp = (uint64_t)src0_value - (uint64_t)src1_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  UPDATE_APSR_SUB(result, temp, src0_value, src1_value);
  ADVANCE_PC_2;
}

void instr_cmp_reg_2(uint16_t instr) {
  uint32_t src0 = BF_0_3(instr) | ((instr & 0x80) >> 4);
  uint32_t src1 = BF_3_4(instr);
  uint32_t src0_value = state.registers[src0];
  uint32_t src1_value = state.registers[src1];
  uint64_t temp = (uint64_t)src0_value - (uint64_t)src1_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  UPDATE_APSR_SUB(result, temp, src0_value, src1_value);
  ADVANCE_PC_2;
}

void instr_cps_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_eor_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint32_t result = dest_value ^ src_value;
  state.registers[dest] = result;
  UPDATE_APSR_BIT(result);
  ADVANCE_PC_2;
}

void instr_ldmia_1(uint16_t instr) {
  uint32_t mem = BF_8_3(instr);
  uint32_t bitmap = BF_0_8(instr);
  uint32_t addr = state.registers[mem];
  if(bitmap & 0x01) {
    state.registers[0] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x02) {
    state.registers[1] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x04) {
    state.registers[2] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x08) {
    state.registers[3] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x10) {
    state.registers[4] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x20) {
    state.registers[5] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x40) {
    state.registers[6] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x80) {
    state.registers[7] = load_32(addr);
    addr += 4;
  }
  if(!((1 << mem) & bitmap)) {
    state.registers[mem] = addr;
  }
  ADVANCE_PC_2;
}

void instr_ldr_imm_1_w(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t base = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr) << 2;
  uint32_t addr = state.registers[base] + imm;
  state.registers[dest] = load_32(addr);
  ADVANCE_PC_2;
}

void instr_ldr_imm_2_w(uint16_t instr) {
  uint32_t dest = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr) << 2;
  uint32_t addr = state.registers[SP] + imm;
  state.registers[dest] = load_32(addr);
  ADVANCE_PC_2;
}

void instr_ldr_lit_1_w(uint16_t instr) {
  uint32_t dest = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr) << 2;
  uint32_t addr = (((state.registers[PC] + 1) | 0x3) + 1) + imm;
  state.registers[dest] = load_32(addr);
  ADVANCE_PC_2;
}

void instr_ldr_reg_1_w(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t base = BF_3_3(instr);
  uint32_t offset = BF_6_3(instr);
  uint32_t addr = state.registers[base] + state.registers[offset];
  state.registers[dest] = load_32(addr);
  ADVANCE_PC_2;
}

void instr_ldr_imm_1_b(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t base = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr);
  uint32_t addr = state.registers[base] + imm;
  state.registers[dest] = load_8(addr);
  ADVANCE_PC_2;
}

void instr_ldr_reg_1_b(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t base = BF_3_3(instr);
  uint32_t offset = BF_6_3(instr);
  uint32_t addr = state.registers[base] + state.registers[offset];
  state.registers[dest] = load_8(addr);
  ADVANCE_PC_2;
}

void instr_ldr_imm_1_h(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t base = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr) << 1;
  uint32_t addr = state.registers[base] + imm;
  state.registers[dest] = load_16(addr);
  ADVANCE_PC_2;
}

void instr_ldr_reg_1_h(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t base = BF_3_3(instr);
  uint32_t offset = BF_6_3(instr);
  uint32_t addr = state.registers[base] + state.registers[offset];
  state.registers[dest] = load_16(addr);
  ADVANCE_PC_2;
}

void instr_ldr_reg_1_sh(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t base = BF_3_3(instr);
  uint32_t offset = BF_6_3(instr);
  uint32_t addr = state.registers[base] + state.registers[offset];
  state.registers[dest] = (uint32_t)((int32_t)(load_16(addr) << 16) >> 16);
  ADVANCE_PC_2;
}

void instr_lsl_imm_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr);
  uint32_t src_value = state.registers[src];
  uint32_t result = src_value << imm;
  state.registers[dest] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    ((imm > 0 ? src_value << (imm - 1) & $80000000 : 0) ? APSR_C : 0) |
    (state.apsr & APSR_V);
  ADVANCE_PC_2;
}

void instr_lsl_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint32_t result = dest_value << src_value;
  state.registers[dest] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    ((src_value > 0 ? dest_value << (src_value - 1) & $80000000 : 0) ?
     APSR_C : 0) |
    (state.apsr & APSR_V);
  ADVANCE_PC_2;
}

void instr_lsr_imm_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr);
  if(imm == 0) {
    imm == 32;
  }
  uint32_t src_value = state.registers[src];
  uint32_t result = src_value >> imm;
  state.registers[dest] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    ((src_value >> (imm - 1)) & 0x1 ? APSR_C : 0) |
    (state.apsr & APSR_V);
  ADVANCE_PC_2;
}

void instr_lsr_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint32_t result = dest_value >> src_value;
  state.registers[dest] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (src_value ? ((dest_value >> (src_value - 1)) & 0x1 ? APSR_C : 0) : 0) |
    (state.apsr & APSR_V);
  ADVANCE_PC_2;
}

void instr_mov_imm_1(uint16_t instr) {
  uint32_t dest = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  state.registers[dest] = imm;
  state.apsr =
    (imm >> 31 ? APSR_N : 0) |
    (imm == 0 ? APSR_Z : 0) |
    (state.apsr & (APSR_C | APSR_V));
  ADVANCE_PC_2;
}

void instr_mov_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr) | ((instr & 0x80) >> 4);
  uint32_t src = BF_3_4(instr);
  state.registers[dest] = state.registers[src];
  ADVANCE_PC_2;
}

void instr_mul_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint32_t result = dest_value * src_value;
  state.registers[dest] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (state.apsr & (APSR_C | APSR_V));
  ADVANCE_PC_2;
}

void instr_mvn_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t src_value = state.registers[src];
  uint32_t result = ~src_value;
  state.registers[dest] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (state.apsr & (APSR_C | APSR_V));
  ADVANCE_PC_2;
}

void instr_nop_1(uint16_t instr) {
  ADVANCE_PC_2;
}

void instr_orr_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint32_t result = dest_value | src_value;
  state.registers[dest] = result;
  UPDATE_APSR_BIT(result);
  ADVANCE_PC_2;
}

void instr_pop_1(uint16_t instr) {
  uint32_t bitmap = BF_0_8(instr);
  uint32_t pop_pc = instr & 0x100;
  uint32_t addr = state.registers[SP];
  if(bitmap & 0x01) {
    state.registers[0] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x02) {
    state.registers[1] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x04) {
    state.registers[2] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x08) {
    state.registers[3] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x10) {
    state.registers[4] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x20) {
    state.registers[5] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x40) {
    state.registers[6] = load_32(addr);
    addr += 4;
  }
  if(bitmap & 0x80) {
    state.registers[7] = load_32(addr);
    addr += 4;
  }
  if(pop_pc) {
    uint32_t new_pc = load_32(addr);
    if(!(new_pc & 0x1)) {
      fprintf(stderr, "BAD POP PC DESTINATION: %08X from %08X @ %08X\n",
	      new_pc, addr, state.registers[PC]);
      exit(1);
    }
    state.registers[PC] = new_pc & ~0x1;
    addr += 4;
  } else {
    ADVANCE_PC_2;
  }
  state.registers[SP] = addr;
}

void instr_push_1(uint16_t instr) {
  uint32_t bitmap = BF_0_8(instr);
  uint32_t push_lr = instr & 0x100;
  uint32_t addr = state.registers[SP];
  if(push_lr) {
    addr -= 4;
    store_32(addr, state.registers[LR]);
  }
  if(bitmap & 0x80) {
    addr -= 4;
    store_32(addr, state.registers[7]);
  }
  if(bitmap & 0x40) {
    addr -= 4;
    store_32(addr, state.registers[6]);
  }
  if(bitmap & 0x20) {
    addr -= 4;
    store_32(addr, state.registers[5]);
  }
  if(bitmap & 0x10) {
    addr -= 4;
    store_32(addr, state.registers[4]);
  }
  if(bitmap & 0x08) {
    addr -= 4;
    store_32(addr, state.registers[3]);
  }
  if(bitmap & 0x04) {
    addr -= 4;
    store_32(addr, state.registers[2]);
  }
  if(bitmap & 0x02) {
    addr -= 4;
    store_32(addr, state.registers[1]);
  }
  if(bitmap & 0x01) {
    addr -= 4;
    store_32(addr, state.registers[0]);
  }
  state.registers[SP] = addr;
  ADVANCE_PC_2;
}

void instr_ror_reg_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t dest_value = state.registers[dest];
  uint32_t src_value = state.registers[src];
  uint32_t result =
    (dest_value >> src_value) | (dest_value << (32 - src_value));
  state.registers[dest] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (result >> 31 ? APSR_C : 0) |
    (state.apsr & APSR_V);
  ADVANCE_PC_2;
}

void instr_rsb_imm_1(uint16_t instr) {
  uint32_t dest = BF_0_3(instr);
  uint32_t src = BF_3_3(instr);
  uint32_t src_value = state.registers[src];
  uint64_t temp = 0L - (uint64_t)src_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[dest] = result;
  UPDATE_APSR_SUB(result, temp, 0, src_value);
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
  register_handler(instr_ldr_reg_1_sh, 0xFE00, 0x5E00);
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
  register_handler(instr_ror_reg_1, 0xFFC0, 0x41C0);
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
  /*  register_handler(instr_svc, 0xFF00, 0xDF00); */
  register_handler(instr_tst_reg_1, 0xFFC0, 0x4200);
  register_handler(instr_wfi_1, 0xFFFF, 0xBF30);
}
