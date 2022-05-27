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
#define BF_0_10(instr) ((uint32_t)((instr) & 0x3FF))
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
#define UPDATE_APSR_ADD(result, temp, src0_value, src1_value)		\
  {state.apsr =								\
      ((result) >> 31 ? APSR_N : 0) |					\
      ((result) == 0 ? APSR_Z : 0) |					\
      ((temp) >> 32 ? APSR_C : 0) |					\
      check_add_overflow((result), (src0_value), (src1_value));}

/* Update the APSR for an ADD */
#define UPDATE_APSR_SUB(result, temp, src0_value, src1_value)		\
  {state.apsr =								\
      ((result) >> 31 ? APSR_N : 0) |					\
      ((result) == 0 ? APSR_Z : 0) |					\
      ((temp) >> 32 ? APSR_C : 0) |					\
      check_sub_overflow((result), (src0_value), (src1_value));}

/* Update the APSR for bit operations */
#define UPDATE_APSR_BIT(result)		   \
  {state.apsr =				   \
      ((result) >> 31 ? APSR_N : 0) |	   \
      ((result) == 0 ? APSR_Z : 0) |	   \
      (state.apsr & (APSR_C | APSR_V));}

/* Print the APSR */
void print_apsr(void) {
  if(state.apsr & APSR_N) {
    printf("N");
  } else {
    printf("n");
  }
  if(state.apsr & APSR_Z) {
    printf("Z");
  } else {
    printf("z");
  }
  if(state.apsr & APSR_C) {
    printf("C");
  } else {
    printf("c");
  }
  if(state.apsr & APSR_V) {
    printf("V\n");
  } else {
    printf("v\n");
  }
}

/* 16-bit instructions */

void instr_adc_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint64_t temp = (uint64_t)rdn_value + (uint64_t)rm_value +
    (state.apsr & APSR_C ? 1 : 0);
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rdn] = result;
  UPDATE_APSR_ADD(result, temp, rdn_value, rm_value);
#ifdef TRACE
  printf("%08X: %04X      ADCS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_add_imm_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t imm = BF_6_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint64_t temp = (uint64_t)rn_value + (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rd] = result;
  UPDATE_APSR_ADD(result, temp, rn_value, imm);
#ifdef TRACE
  printf("%08X: %04X      ADDS R%d, R%d (%d $%08X), #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, rd, rn, (int32_t)rn_value, rn_value,
	 (int32_t)imm, imm, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_add_imm_2(uint16_t instr) {
  uint32_t rdn = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint64_t temp = (uint64_t)rdn_value + (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rdn] = result;
  UPDATE_APSR_ADD(result, temp, rdn_value, imm);
#ifdef TRACE
  printf("%08X: %04X      ADDS R%d (%d $%08X), #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 (int32_t)imm, imm, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_add_reg_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t rm = BF_6_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint64_t temp = (uint64_t)rn_value + (uint64_t)rm_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rd] = result;
  UPDATE_APSR_ADD(result, temp, rn_value, rm_value);
#ifdef TRACE
  printf("%08X: %04X      ADDS R%d, R%d (%d $%08X), R%d (%d $%08X) => "
	 "%d $%08X ",
	 state.registers[PC], instr, rd, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_add_reg_2(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr) | ((instr >> 7) & 0x1);
  uint32_t rm = BF_3_4(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint64_t temp = (uint64_t)rdn_value + (uint64_t)rm_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rdn] = result;
#ifdef TRACE
  printf("%08X: %04X      ADD R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_add_sp_imm_1(uint16_t instr) {
  uint32_t rd = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  state.registers[rd] = state.registers[SP] + imm;
#ifdef TRACE
  printf("%08X: %04X      ADD R%d, SP (%d, $%08X), #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, rd, (int32_t)state.registers[SP],
	 state.registers[SP], (int32_t)imm, imm, (int32_t)state.registers[rd],
	 state.registers[rd]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_add_sp_imm_2(uint16_t instr) {
  uint32_t imm = BF_0_7(instr);
#ifdef TRACE
  printf("%08X: %04X      ADD SP, SP (%d $%08X), #%d $%08X => ",
	 state.registers[PC], instr, (int32_t)state.registers[SP],
	 state.registers[SP], (int32_t)imm, imm);
#endif
  state.registers[SP] += imm;
#ifdef TRACE
  printf("%d $%08X ", (int32_t)state.registers[SP], state.registers[SP]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_add_sp_reg_1(uint16_t instr) {
  uint32_t rdm = BF_0_3(instr) | ((instr >> 7) & 0x1);
#ifdef TRACE
  printf("%08X: %04X      ADD R%d, SP (%d $%08X), R%d (%d $%08X) => ",
	 state.registers[PC], instr, rdm,
	 (int32_t)state.registers[SP], state.registers[SP],
	 rdm, (int32_t)state.registers[rdm], state.registers[rdm]);
#endif
  state.registers[rdm] += state.registers[SP];
#ifdef TRACE
  printf("%d $%08X ", (int32_t)state.registers[rd], state.registers[rd]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_add_sp_reg_2(uint16_t instr) {
  uint32_t rm = BF_3_4(instr);
#ifdef TRACE
  printf("%08X: %04X      ADD SP (%d $%08X), R%d (%d $%08X) => ",
	 state.registers[PC], instr,
	 (int32_t)state.registers[SP], state.registers[SP],
	 rm, (int32_t)state.registers[rm], state.registers[rm]);
#endif
  state.registers[SP] += state.registers[rm];
#ifdef TRACE
  printf("%d $%08X ", (int32_t)state.registers[SP], state.registers[SP]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_adr_1(uint16_t instr) {
  uint32_t rd = BF_8_3(instr);
  uint32_t pc_value = ((state.registers[PC] + 1) | 3) + 1;
  uint32_t imm = BF_0_8(instr) << 2;
  state.registers[rd] = pc_value + imm;
#ifdef TRACE
  printf("%08X: %04X      ADR R%d, #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, (int32_t)imm, imm,
	 (int32_t)state.registers[rd], state.registers[rd]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_and_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rdn_value & rm_value;
  state.registers[rdn] = result;
  UPDATE_APSR_BIT(result);
#ifdef TRACE
  printf("%08X: %04X      ANDS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_asr_imm_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr);
  if(imm == 0) {
    imm == 32;
  }
  uint32_t rm_value = state.registers[rm];
  uint32_t result = (uint32_t)((int32_t)rm_value >> imm);
  state.registers[rd] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    ((rm_value >> (imm - 1)) & 0x1 ? APSR_C : 0) |
    (state.apsr & APSR_V);
#ifdef TRACE
  printf("%08X: %04X      ASRS R%d, R%d (%d $%08X), #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	 (int32_t)imm, imm, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_asr_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result = (uint32_t)((int32_t)rdn_value >> rm_value);
  state.registers[rd] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (rm_value ? ((rdn_value >> (rm_value - 1)) & 0x1 ? APSR_C : 0) : 0) |
    (state.apsr & APSR_V);
#ifdef TRACE
  printf("%08X: %04X      ASRS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_b_1(uint16_t instr) {
  uint32_t cond = BF_8_4(instr);
  uint32_t offset = (uint32_t)((int32_t)(BF_0_8(instr) << 24) >> 23);
#ifdef TRACE
  printf("%08X: %04X      ", state.registers[PC], instr);
#endif
  switch(cond) {
  case COND_EQ:
#ifdef TRACE
    printf("BEQ $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += state.apsr & APSR_Z ? 4 + offset : 2;
    break;
  case COND_NE:
#ifdef TRACE
    printf("BNE $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += state.apsr & APSR_Z ? 2 : 4 + offset;
    break;
  case COND_CS:
#ifdef TRACE
    printf("BCS $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += state.apsr & APSR_C ? 4 + offset : 2;
    break;
  case COND_CC:
#ifdef TRACE
    printf("BCC $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += state.apsr & APSR_C ? 2 : 4 + offset;
    break;
  case COND_MI:
#ifdef TRACE
    printf("BMI $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += state.apsr & APSR_N ? 4 + offset : 2;
    break;
  case COND_PL:
#ifdef TRACE
    printf("BPL $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += state.apsr & APSR_N ? 2 : 4 + offset;
    break;
  case COND_VS:
#ifdef TRACE
    printf("BVS $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += state.apsr & APSR_V ? 4 + offset : 2;
    break;
  case COND_VC:
#ifdef TRACE
    printf("BVC $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += state.apsr & APSR_V ? 2 : 4 + offset;
    break;
  case COND_HI:
#ifdef TRACE
    printf("BHI $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += (state.apsr & APSR_C) && !(state.apsr & APSR_Z) ?
      4 + offset : 2;
    break;
  case COND_LS:
#ifdef TRACE
    printf("BLS $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] += !(state.apsr & APSR_C) || (state.apsr & APSR_Z) ?
      4 + offset : 2;
    break;
  case COND_GE:
#ifdef TRACE
    printf("BGE $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] +=
      ((state.apsr & APSR_N) && (state.apsr & APSR_V)) ||
      (!(state.apsr & APSR_N) && !(state.apsr & APSR_V)) ? 4 + offset : 2;
    break;
  case COND_LT:
#ifdef TRACE
    printf("BLT $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] +=
      ((state.apsr & APSR_N) && !(state.apsr & APSR_V)) ||
      (!(state.apsr & APSR_N) && (state.apsr & APSR_V)) ? 4 + offset : 2;
    break;
  case COND_GT:
#ifdef TRACE
    printf("BGT $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] +=
      !(state.apsr & APSR_Z) &&
      (((state.apsr & APSR_N) && (state.apsr & APSR_V)) ||
       (!(state.apsr & APSR_N) && !(state.apsr & APSR_V))) ? 4 + offset : 2;
    break;
  case COND_LE:
#ifdef TRACE
    printf("BLE $%08X ", state.registers[PC] + 4 + offset);
#endif
    state.registers[PC] +=
      (state.apsr & APSR_Z) ||
      (((state.apsr & APSR_N) && !(state.apsr & APSR_V)) ||
       (!(state.apsr & APSR_N) && (state.apsr & APSR_V))) ? 4 + offset : 2;
    break;
  case COND_OTHER:
#ifdef TRACE
    printf("SVC ");
#endif
    instr_svc(instr);
    break;
  default:
#ifdef TRACE
    printf("<UNDEFINED>\n");
#endif
    fprintf(stderr, "PERMANENTLY UNDEFINED: %04X @ %08X\n",
	    (uint32_t)instr, state.registers[PC]);
    exit(1);
  }
#ifdef TRACE
  print_apsr();
#endif
}

void instr_b_2(uint16_t instr) {
  uint32_t offset = (uint32_t)((int32_t)(BF_0_11(instr) << 21) >> 20);
#ifdef TRACE
  printf("%08X: %04X      B $%08X ", state.registers[PC], instr,
	 state.registers[PC] + 4 + offset);
  print_apsr();
#endif
  state.registers[PC] += 4 + offset;
}

void instr_bic_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rdn_value & ~rm_value;
  state.registers[rdn] = result;
  UPDATE_APSR_BIT(result);
#ifdef TRACE
  printf("%08X: %04X      BICS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_bkpt(uint16_t instr) {
#ifdef TRACE
  printf("%08X: %04X      BKPT ", state.registers[PC], instr);
  print_apsr();
#endif
  fprintf(stderr, "BREAKPOINT: %04X @ %08X\n",
	  (uint32_t)instr, state.registers[PC]);
  exit(1);
}

void instr_blx_reg(uint16_t instr) {
  uint32_t rm = BF_3_4(instr);
  uint32_t rm_value = state.registers[rm];
  if(!(rm_value & 0x1)) {
    fprintf(stderr, "BAD BLX DESTINATION: %08X from R%d @ %08X\n",
	    rm_value, rm, state.registers[PC]);
    exit(1);
  }
  state.registers[LR] = (state.registers[PC] + 2) | 0x1;
#ifdef TRACE
  printf("%08X: %04X      BLX R%d (%d $%08X) => LR %d $%08X ",
	 state.registers[PC], (uint32_t)instr, rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[LR], state.registers[LR]);
  print_apsr();
#endif
  state.registers[PC] = rn_value & ~0x1;
}

void instr_bx(uint16_t instr) {
  uint32_t rm = BF_3_4(instr);
  uint32_t rm_value = state.registers[rm];
  if(!(rn_value & 0x1)) {
    fprintf(stderr, "BAD BX DESTINATION: %08X from R%d @ %08X\n",
	    rm_value, rm, state.registers[PC]);
    exit(1);
  }
#ifdef TRACE
  printf("%08X: %04X      BLX R%d (%d $%08X) ",
	 state.registers[PC], (uint32_t)instr, rm, (int32_t)rm_value, rm_value);
  print_apsr();
#endif
  state.registers[PC] = rn_value & ~0x1;
}

void instr_cbnz(uint16_t instr) {
  uint32_t rn = BF_0_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t offset_bits = ((instr & 0x200) >> 4) | BF_3_5(instr)
  uint32_t offset = (uint32_t)((int32_t)(offset_bits << 26) >> 25);
#ifdef TRACE
  printf("%08X: %04X      CBNZ R%d (%d %08X), $%08X ",
	 state.registers[PC], instr, rn, (int32_t)rn_value, rn_value,
	 state.registers[PC] + 4 + offset);
  print_apsr();
#endif
  state.registers[PC] += rn_value ? 4 + offset : 2;
}

void instr_cbz(uint16_t instr) {
  uint32_t rn = BF_0_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t offset_bits = ((instr & 0x200) >> 4) | BF_3_5(instr)
  uint32_t offset = (uint32_t)((int32_t)(offset_bits << 26) >> 25);
#ifdef TRACE
  printf("%08X: %04X      CBZ R%d (%d %08X), $%08X ",
	 state.registers[PC], instr, rn, (int32_t)rn_value, rn_value,
	 state.registers[PC] + 4 + offset);
  print_apsr();
#endif
  state.registers[PC] += !rn_value ? 4 + offset : 2;
}

void instr_cmn_reg_1(uint16_t instr) {
  uint32_t rn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint64_t temp = (uint64_t)rn_value + (uint64_t)rm_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  UPDATE_APSR_ADD(result, temp, rn_value, rm_value);
#ifdef TRACE
  printf("%08X: %04X      CMN R%d (%d $%08X), R%d (%d $%08X) ",
	 state.registers[PC], instr, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_cmp_imm_1(uint16_t instr) {
  uint32_t rn = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  uint32_t rn_value = state.registers[rn];
  uint64_t temp = (uint64_t)rn_value - (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  UPDATE_APSR_SUB(result, temp, rn_value, imm);
#ifdef TRACE
  printf("%08X: %04X      CMP R%d (%d $%08X), #%d $%08X ",
	 state.registers[PC], instr, rn, (int32_t)rn_value, rn_value,
	 (int32_t)imm, imm);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_cmp_reg_1(uint16_t instr) {
  uint32_t rn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint64_t temp = (uint64_t)rn_value - (uint64_t)rm_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  UPDATE_APSR_SUB(result, temp, rn_value, rm_value);
#ifdef TRACE
  printf("%08X: %04X      CMP R%d (%d $%08X), R%d (%d $%08X) ",
	 state.registers[PC], instr, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_cmp_reg_2(uint16_t instr) {
  uint32_t rn = BF_0_3(instr) | ((instr & 0x80) >> 4);
  uint32_t rm = BF_3_4(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint64_t temp = (uint64_t)rn_value - (uint64_t)rm_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  UPDATE_APSR_SUB(result, temp, rn_value, rm_value);
#ifdef TRACE
  printf("%08X: %04X      CMP R%d (%d $%08X), R%d (%d $%08X) ",
	 state.registers[PC], instr, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_cps_1(uint16_t instr) {
#ifdef TRACE
  printf("%08X: %04X      CPS ", state.registers[PC], instr);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_eor_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rdn_value ^ rm_value;
  state.registers[rdn] = result;
  UPDATE_APSR_BIT(result);
#ifdef TRACE
  printf("%08X: %04X      EORS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_ldmia_1(uint16_t instr) {
  uint32_t mem = BF_8_3(instr);
  uint32_t bitmap = BF_0_8(instr);
  uint32_t addr = state.registers[mem];
#ifdef TRACE
  printf("%08X: %04X      LDMIA R%d ($%08X)", state.registers[PC], instr, mem,
	 addr);
  if(!((1 << mem) & bitmap)) {
    printf("!");
  }
  printf(", {");
#endif
  if(bitmap & 0x01) {
#ifdef TRACE
    printf("R0");
#endif
    state.registers[0] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[0], state.registers[0]);
    if(bitmap & 0xFE) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x02) {
#ifdef TRACE
    printf("R1");
#endif
    state.registers[1] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[1], state.registers[1]);
    if(bitmap & 0xFC) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x04) {
#ifdef TRACE
    printf("R2");
#endif
    state.registers[2] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[2], state.registers[2]);
    if(bitmap & 0xF8) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x08) {
#ifdef TRACE
    printf("R3");
#endif
    state.registers[3] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[3], state.registers[3]);
    if(bitmap & 0xF0) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x10) {
#ifdef TRACE
    printf("R4");
#endif
    state.registers[4] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[4], state.registers[4]);
    if(bitmap & 0xE0) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x20) {
#ifdef TRACE
    printf("R5");
#endif
    state.registers[5] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[5], state.registers[5]);
    if(bitmap & 0xC0) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x40) {
#ifdef TRACE
    printf("R6");
#endif
    state.registers[6] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[6], state.registers[6]);
    if(bitmap & 0x80) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x80) {
#ifdef TRACE
    printf("R7");
#endif
    state.registers[7] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[7], state.registers[7]);
#endif
    addr += 4;
  }
  if(!((1 << mem) & bitmap)) {
    state.registers[mem] = addr;
#ifdef TRACE
    printf("} => $%08X ", addr);
#endif
  } else {
#ifdef TRACE
    printf("} ");
#endif
  }
#ifdef TRACE
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_ldr_imm_1_w(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr) << 2;
  uint32_t rn_value = state.registers[rn];
  uint32_t addr = rn_value + imm;
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  state.registers[rt] = load_32(addr);
#ifdef TRACE
  printf("%08X: %04X      LDR R%d, [R%d (%d $%08X), #%d $%08X] => %d $%08X ",
	 saved_pc, instr, rt, rn, (int32_t)rn_value, rn_value,
	 (int32_t)imm, imm,
	 (int32_t)state.registers[rt], state.registers[rt]);
  print_apsr();
#endif
}

void instr_ldr_imm_2_w(uint16_t instr) {
  uint32_t rt = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr) << 2;
  uint32_t addr = state.registers[SP] + imm;
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  state.registers[rt] = load_32(addr);
#ifdef TRACE
  printf("%08X: %04X      LDR R%d, [SP (%d $%08X), #%d $%08X] => %d $%08X ",
	 saved_pc, instr, rt, (int32_t)state.registers[SP],
	 state.registers[SP], (int32_t)imm, imm,
	 (int32_t)state.registers[rt], state.registers[rt]);
  print_apsr();
#endif
}

void instr_ldr_lit_1_w(uint16_t instr) {
  uint32_t rt = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr) << 2;
  uint32_t addr = (((state.registers[PC] + 1) | 0x3) + 1) + imm;
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  state.registers[rt] = load_32(addr);
#ifdef TRACE
  printf("%08X: %04X        LDR R%d, #%08X => %d $%08X ",
	 saved_pc, instr, rt, addr, (int32_t)state.registers[rt],
	 state.registers[rt]);
  print_apsr();
#endif
}

void instr_ldr_reg_1_w(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t rm = BF_6_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint32_t addr = rn_value + rm_value;
#ifdef TRACE
  uint32_t saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  state.registers[rt] = load_32(addr);
#ifdef TRACE
  printf("%08X: %04X      LDR R%d, [R%d (%d $%08X), R%d (#%d $%08X)] "
	 "=> %d $%08X ",
	 saved_pc, instr, rt, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[rt], state.registers[rt]);
  print_apsr();
#endif
}

void instr_ldr_imm_1_b(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t addr = rn_value + imm;
#ifdef TRACE
  uint32_t saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  state.registers[rt] = load_8(addr);
#ifdef TRACE
  printf("%08X: %04X      LDRB R%d, [R%d (%d $%08X), #%d $%08X] => %d $%08X ",
	 saved_pc, instr, rt, rn, (int32_t)rn_value, rn_value,
	 (int32_t)imm. imm,
	 (int32_t)state.registers[rt], state.registers[rt]);
  print_apsr();
#endif
}

void instr_ldr_reg_1_b(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t rm = BF_6_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint32_t addr = rn_value + rm_value;
#ifdef TRACE
  uint32_t saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  state.registers[rt] = load_8(addr);
#ifdef TRACE
  printf("%08X: %04X      LDRB R%d, [R%d (%d $%08X), R%d (#%d $%08X)] "
	 "=> %d $%08X ",
	 saved_pc, instr, rt, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[rt], state.registers[rt]);
  print_apsr();
#endif
}

void instr_ldr_imm_1_h(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr) << 1;
  uint32_t rn_value = state.registers[rn];
  uint32_t addr = rn_value + imm;
#ifdef TRACE
  uint32_t saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  state.registers[rt] = load_16(addr);
#ifdef TRACE
  printf("%08X: %04X      LDRH R%d, [R%d (%d $%08X), #%d $%08X] => %d $%08X ",
	 saved_pc, instr, rt, rn, (int32_t)rn_value, rn_value,
	 (int32_t)imm. imm,
	 (int32_t)state.registers[rt], state.registers[rt]);
  print_apsr();
#endif
}

void instr_ldr_reg_1_h(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t rm = BF_6_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint32_t addr = rn_value + rm_value;
#ifdef TRACE
  uint32_t saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  state.registers[rt] = load_16(addr);
#ifdef TRACE
  printf("%08X: %04X      LDRH R%d, [R%d (%d $%08X), R%d (#%d $%08X)] "
	 "=> %d $%08X ",
	 saved_pc, instr, rt, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[rt], state.registers[rt]);
  print_apsr();
#endif
}

void instr_ldr_reg_1_sh(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t rm = BF_6_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint32_t addr = rn_value + rm_value;
#ifdef TRACE
  uint32_t saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  state.registers[rt] = (uint32_t)((int32_t)(load_16(addr) << 16) >> 16);
#ifdef TRACE
  printf("%08X: %04X      LDRSH R%d, [R%d (%d $%08X), R%d (#%d $%08X)] "
	 "=> %d $%08X ",
	 saved_pc, instr, rt, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[rt], state.registers[rt]);
  print_apsr();
#endif
}

void instr_lsl_imm_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr);
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rm_value << imm;
  state.registers[rd] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    ((imm > 0 ? rm_value << (imm - 1) & $80000000 : 0) ? APSR_C : 0) |
    (state.apsr & APSR_V);
#ifdef TRACE
  if(imm) {
    printf("%08X: %04X      LSLS R%d, R%d (%d $%08X), #%d $%08X => %d $%08X ",
	   state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	   (int32_t)imm, imm, (int32_t)result, result);
  } else {
    printf("%08X: %04X      MOVS R%d, R%d (%d $%08X) => %d $%08X ",
	   state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	   (int32_t)result, result);
  }    
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_lsl_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rdn_value << rm_value;
  state.registers[rdn] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    ((rm_value > 0 ? rdn_value << (rm_value - 1) & $80000000 : 0) ?
     APSR_C : 0) |
    (state.apsr & APSR_V);
#ifdef TRACE
  printf("%08X: %04X      ASRS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_lsr_imm_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr);
  if(imm == 0) {
    imm == 32;
  }
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rm_value >> imm;
  state.registers[rd] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    ((rn_value >> (imm - 1)) & 0x1 ? APSR_C : 0) |
    (state.apsr & APSR_V);
#ifdef TRACE
  printf("%08X: %04X      LSRS R%d, R%d (%d $%08X), #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	 (int32_t)imm, imm, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_lsr_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rdn_value >> rm_value;
  state.registers[rdn] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (rn_value ? ((rd_value >> (rn_value - 1)) & 0x1 ? APSR_C : 0) : 0) |
    (state.apsr & APSR_V);
#ifdef TRACE
  printf("%08X: %04X      LSRS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_mov_imm_1(uint16_t instr) {
  uint32_t rd = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  state.registers[rd] = imm;
  state.apsr =
    (imm >> 31 ? APSR_N : 0) |
    (imm == 0 ? APSR_Z : 0) |
    (state.apsr & (APSR_C | APSR_V));
#ifdef TRACE
  printf("%08X: %04X      MOVS R%d, #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, rd, (int32_t)imm, imm,
	 (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_mov_reg_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr) | ((instr & 0x80) >> 4);
  uint32_t rm = BF_3_4(instr);
  uint32_t rm_value = state.registers[rm];
  state.registers[rd] = rm_value;
#ifdef TRACE
  printf("%08X: %04X      MOV R%d, R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[rd], state.registers[rd]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_mul_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rdn_value * rm_value;
  state.registers[rdn] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (state.apsr & (APSR_C | APSR_V));
#ifdef TRACE
  printf("%08X: %04X      MULS R%d (%d $%04X), R%d (%d $%04X) => %d $%04X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_mvn_reg_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rm_value = state.registers[rm];
  uint32_t result = ~rm_value;
  state.registers[rd] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (state.apsr & (APSR_C | APSR_V));
#ifdef TRACE
  printf("%08X: %04X      MVNS R%d, R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	 (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_nop_1(uint16_t instr) {
#ifdef TRACE
  printf("%08X: %04X      NOP ",
	 state.registers[PC], instr);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_orr_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rdn_value | rm_value;
  state.registers[rdn] = result;
  UPDATE_APSR_BIT(result);
#ifdef TRACE
  printf("%08X: %04X      ORRS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_pop_1(uint16_t instr) {
  uint32_t bitmap = BF_0_8(instr);
  uint32_t pop_pc = instr & 0x100;
  uint32_t addr = state.registers[SP];
#ifdef TRACE
  printf("%08X: %04X      POP {", state.registers[PC], instr);
#endif
  if(bitmap & 0x01) {
#ifdef TRACE
    printf("R0");
#endif
    state.registers[0] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[0], state.registers[0]);
    if((bitmap & 0xFE) || pop_pc) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x02) {
#ifdef TRACE
    printf("R1");
#endif
    state.registers[1] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[1], state.registers[1]);
    if((bitmap & 0xFC) || pop_pc) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x04) {
#ifdef TRACE
    printf("R2");
#endif
    state.registers[2] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[2], state.registers[2]);
    if((bitmap & 0xF8) || pop_pc) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x08) {
#ifdef TRACE
    printf("R3");
#endif
    state.registers[3] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[3], state.registers[3]);
    if((bitmap & 0xF0) || pop_pc) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x10) {
#ifdef TRACE
    printf("R4");
#endif
    state.registers[4] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[4], state.registers[4]);
    if((bitmap & 0xE0) || pop_pc) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x20) {
#ifdef TRACE
    printf("R5");
#endif
    state.registers[5] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[5], state.registers[5]);
    if((bitmap & 0xC0) || pop_pc) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x40) {
#ifdef TRACE
    printf("R6");
#endif
    state.registers[6] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[6], state.registers[6]);
    if((bitmap & 0x80) || pop_pc) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(bitmap & 0x80) {
#ifdef TRACE
    printf("R7");
#endif
    state.registers[7] = load_32(addr);
#ifdef TRACE
    printf(" (=> %d $%08X)", (int32_t)state.registers[7], state.registers[7]);
    if(pop_pc) {
      printf(", ");
    }
#endif
    addr += 4;
  }
  if(pop_pc) {
#ifdef TRACE
    printf("PC");
#endif
    uint32_t new_pc = load_32(addr);
    if(!(new_pc & 0x1)) {
      fprintf(stderr, "BAD POP PC DESTINATION: %08X from %08X @ %08X\n",
	      new_pc, addr, state.registers[PC]);
      exit(1);
    }
    state.registers[PC] = new_pc & ~0x1;
#ifdef TRACE
    printf(" => (%d $%08X)", (int32_t)state.registers[PC], state.registers[PC]);
#endif
    addr += 4;
  } else {
    ADVANCE_PC_2;
  }
  state.registers[SP] = addr;
#ifdef TRACE
  printf("} => $%08X ", addr);
  print_apsr();
#endif
}

void instr_push_1(uint16_t instr) {
  uint32_t bitmap = BF_0_8(instr);
  uint32_t push_lr = instr & 0x100;
  uint32_t addr = state.registers[SP];
#ifdef TRACE
  printf("%08X: %04X      PUSH {", state.registers[PC], instr)
  if(bitmap & 0x01) {
    printf("R0 (%d $%08X)", (int32_t)state.registers[0], state.registers[0]);
    if((bitmap & 0xFE) || push_lr) {
      printf(", ");
    }
  }
  if(bitmap & 0x02) {
    printf("R1 (%d $%08X)", (int32_t)state.registers[1], state.registers[1]);
    if((bitmap & 0xFC) || push_lr) {
      printf(", ");
    }
  }
  if(bitmap & 0x04) {
    printf("R2 (%d $%08X)", (int32_t)state.registers[2], state.registers[2]);
    if((bitmap & 0xF8) || push_lr) {
      printf(", ");
    }
  }
  if(bitmap & 0x08) {
    printf("R3 (%d $%08X)", (int32_t)state.registers[3], state.registers[3]);
    if((bitmap & 0xF0) || push_lr) {
      printf(", ");
    }
  }
  if(bitmap & 0x10) {
    printf("R4 (%d $%08X)", (int32_t)state.registers[4], state.registers[4]);
    if((bitmap & 0xE0) || push_lr) {
      printf(", ");
    }
  }
  if(bitmap & 0x20) {
    printf("R5 (%d $%08X)", (int32_t)state.registers[5], state.registers[5]);
    if((bitmap & 0xC0) || push_lr) {
      printf(", ");
    }
  }
  if(bitmap & 0x40) {
    printf("R6 (%d $%08X)", (int32_t)state.registers[6], state.registers[6]);
    if((bitmap & 0x80) || push_lr) {
      printf(", ");
    }
  }
  if(bitmap & 0x80) {
    printf("R7 (%d $%08X)", (int32_t)state.registers[7], state.registers[7]);
    if(push_lr) {
      printf(", ");
    }
  }
  if(push_lr) {
    printf("LR (%d $%08X)", (int32_t)state.registers[LR], state.registers[LR]);
  }
#endif
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
#ifdef TRACE
  printf("} => $%08X ", addr);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_ror_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result =
    (rdn_value >> rm_value) | (rdn_value << (32 - rm_value));
  state.registers[rd] = result;
  state.apsr =
    (result >> 31 ? APSR_N : 0) |
    (result == 0 ? APSR_Z : 0) |
    (result >> 31 ? APSR_C : 0) |
    (state.apsr & APSR_V);
#ifdef TRACE
  printf("%08X: %04X      RORS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_rsb_imm_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint64_t temp = 0L - (uint64_t)rn_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rd] = result;
  UPDATE_APSR_SUB(result, temp, 0, rn_value);
#ifdef TRACE
  printf("%08X: %04X      RSBS R%d, R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rd, rn, (int32_t)rn_value, rn_value,
	 (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_sbc_reg_1(uint16_t instr) {
  uint32_t rdn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint32_t rm_value = state.registers[rm];
  uint64_t temp =
    ((uint64_t)rdn_value - (uint64_t)rm_value) -
    (state.apsr & APSR_C ? 0L : 1L);
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rdn] = result;
  UPDATE_APSR_SUB(result, temp, rdn_value, rm_value);
#ifdef TRACE
  printf("%08X: %04X      SBCS R%d (%d $%08X), R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_stmia_1(uint16_t instr) {
  uint32_t bitmap = BF_0_8(instr);
  uint32_t mem = BF_8_3(instr);
  uint32_t addr = state.registers[mem];
#ifdef TRACE
  printf("%08X: %04X      STMIA R%d ($%08X)!, {",
	 state.registers[PC], instr, mem, addr);
#endif
  if(bitmap & 0x01) {
#ifdef TRACE
    printf("R0 (%d $%08X)", (int32_t)state.registers[0], state.registers[0]);
    if(bitmap & 0xFE) {
      printf(", ");
    }
#endif
    store_32(addr, state.registers[0]);
    addr += 4;
  }
  if(bitmap & 0x02) {
#ifdef TRACE
    printf("R1 (%d $%08X)", (int32_t)state.registers[1], state.registers[1]);
    if(bitmap & 0xFC) {
      printf(", ");
    }
#endif
    store_32(addr, state.registers[1]);
    addr += 4;
  }
  if(bitmap & 0x04) {
#ifdef TRACE
    printf("R2 (%d $%08X)", (int32_t)state.registers[2], state.registers[2]);
    if(bitmap & 0xF8) {
      printf(", ");
    }
#endif
    store_32(addr, state.registers[2]);
    addr += 4;
  }
  if(bitmap & 0x08) {
#ifdef TRACE
    printf("R3 (%d $%08X)", (int32_t)state.registers[3], state.registers[3]);
    if(bitmap & 0xF0) {
      printf(", ");
    }
#endif
    store_32(addr, state.registers[3]);
    addr += 4;
  }
  if(bitmap & 0x10) {
#ifdef TRACE
    printf("R4 (%d $%08X)", (int32_t)state.registers[4], state.registers[4]);
    if(bitmap & 0xE0) {
      printf(", ");
    }
#endif
    store_32(addr, state.registers[4]);
    addr += 4;
  }
  if(bitmap & 0x20) {
#ifdef TRACE
    printf("R5 (%d $%08X)", (int32_t)state.registers[5], state.registers[5]);
    if(bitmap & 0xC0) {
      printf(", ");
    }
#endif
    store_32(addr, state.registers[5]);
    addr += 4;
  }
  if(bitmap & 0x40) {
#ifdef TRACE
    printf("R6 (%d $%08X)", (int32_t)state.registers[6], state.registers[6]);
    if(bitmap & 0x80) {
      printf(", ");
    }
#endif
    store_32(addr, state.registers[6]);
    addr += 4;
  }
  if(bitmap & 0x80) {
#ifdef TRACE
    printf("R7 (%d $%08X)", (int32_t)state.registers[7], state.registers[7]);
#endif
    store_32(addr, state.registers[7]);
    addr += 4;
  }
  state.registers[mem] = addr;
#ifdef TRACE
  printf(" => $%08X ", addr);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_str_imm_1_w(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr) << 2;
  uint32_t rt_value = state.registers[rt];
  uint32_t rn_value = state.registers[rn];
  uint32_t addr = rn_value + imm;
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  store_32(addr, rt_value);
#ifdef TRACE
  printf("%08X: %04X      STR R%d (%d $%08X), [R%d (%d $%08X), #%d $%08X] ",
	 saved_pc, instr, rt, (int32_t)rt_value, rt_value,
	 rn, (int32_t)rn_value, rn_value, (int32_t)imm, imm);
  print_apsr();
#endif
}

void instr_str_imm_2_w(uint16_t instr) {
  uint32_t rt = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr) << 2;
  uint32_t rt_value = state.registers[rt];
  uint32_t addr = state.registers[SP] + imm;
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  store_32(addr, rt_value);
#ifdef TRACE
  printf("%08X: %04X      STR R%d (%d $%08X), [SP (%d $%08X), #%d $%08X] ",
	 saved_pc, instr, rt, (int32_t)rt_value, rt_value,
	 (int32_t)state.registers[SP],
	 state.registers[SP], (int32_t)imm, imm);
  print_apsr();
#endif
}

void instr_str_reg_1_w(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t rm = BF_6_3(instr);
  uint32_t rt_value = state.registers[rt];
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint32_t addr = rn_value + rm_value;
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  store_32(addr, rt_value);
#ifdef TRACE
  printf("%08X: %04X      STR R%d (%d $%08X), [R%d (%d $%08X), "
	 "R%d (#%d $%08X)] ",
	 saved_pc, instr, rt, (int32_t)rt_value, rt_value,
	 rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value);
  print_apsr();
#endif
}

void instr_str_imm_1_b(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr);
  uint32_t rt_value = state.registers[rt];
  uint32_t rn_value = state.registers[rn];
  uint32_t addr = rn_value + imm;
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  store_8(addr, (uint8_t)(rt_value & 0xFF));
#ifdef TRACE
  printf("%08X: %04X      STRB R%d (%d $%08X), [SP (%d $%08X), #%d $%08X] ",
	 saved_pc, instr, rt, (int32_t)rt_value, rt_value,
	 (int32_t)state.registers[SP],
	 state.registers[SP], (int32_t)imm, imm);
  print_apsr();
#endif
}

void instr_str_reg_1_b(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t offset = BF_6_3(instr);
  uint32_t rt_value = state.registers[rt];
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint32_t addr = rn_value + rm_value;
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  store_8(addr, (uint8_t)(rt_value & 0xFF));
#ifdef TRACE
  printf("%08X: %04X      STRB R%d (%d $%08X), [R%d (%d $%08X), "
	 "R%d (#%d $%08X)] ",
	 saved_pc, instr, rt, (int32_t)rt_value, rt_value,
	 rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value);
  print_apsr();
#endif
}

void instr_str_imm_1_h(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t imm = BF_6_5(instr) << 1;
  uint32_t rt_value = state.registers[rt];
  uint32_t rn_value = state.registers[rn];
  uint32_t addr = rn_value + imm;
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  store_16(addr, (uint16_t)(rt_value & 0xFFFF));
#ifdef TRACE
  printf("%08X: %04X      STRH R%d (%d $%08X), [SP (%d $%08X), #%d $%08X] ",
	 saved_pc, instr, rt, (int32_t)rt_value, rt_value,
	 (int32_t)state.registers[SP],
	 state.registers[SP], (int32_t)imm, imm);
  print_apsr();
#endif
}

void instr_str_reg_1_h(uint16_t instr) {
  uint32_t rt = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t rm = BF_6_3(instr);
  uint32_t rt_value = state.registers[rt];
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint32_t addr = rn_value + rm_value
#ifdef TRACE
  uint32 saved_pc = state.registers[PC];
#endif
  ADVANCE_PC_2;
  store_16(addr, (uint16_t)(rt_value & 0xFFFF));
#ifdef TRACE
  printf("%08X: %04X      STRH R%d (%d $%08X), [R%d (%d $%08X), "
	 "R%d (#%d $%08X)] ",
	 saved_pc, instr, rt, (int32_t)rt_value, rt_value,
	 rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value);
  print_apsr();
#endif
}

void instr_sub_imm_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t imm = BF_6_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint64_t temp = (uint64_t)rn_value - (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rd] = result;
  UPDATE_APSR_SUB(result, temp, rn_value, imm);
#ifdef TRACE
  printf("%08X: %04X      SUBS R%d, R%d (%d $%08X), #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, rd, rn, (int32_t)rn_value, rn_value,
	 (int32_t)imm, imm, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_sub_imm_2(uint16_t instr) {
  uint32_t rdn = BF_8_3(instr);
  uint32_t imm = BF_0_8(instr);
  uint32_t rdn_value = state.registers[rdn];
  uint64_t temp = (uint64_t)rdn_value - (uint64_t)imm;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rdn] = result;
  UPDATE_APSR_SUB(result, temp, rdn_value, imm);
#ifdef TRACE
  printf("%08X: %04X      SUBS R%d (%d $%08X), #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, rdn, (int32_t)rdn_value, rdn_value,
	 (int32_t)imm, imm, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_sub_reg_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rn = BF_3_3(instr);
  uint32_t rm = BF_6_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint64_t temp = (uint64_t)rn_value - (uint64_t)rm_value;
  uint32_t result = (uint32_t)(temp & 0xFFFFFFFF);
  state.registers[rd] = result;
  UPDATE_APSR_SUB(result, temp, rn_value, rm_value);
#ifdef TRACE
  printf("%08X: %04X      SUBS R%d, R%d (%d $%08X), R%d (%d $%08X) => "
	 "%d $%08X ",
	 state.registers[PC], instr, rd, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value, (int32_t)result, result);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_sub_sp_imm_1(uint16_t instr) {
  uint32_t imm = BF_0_7(instr) << 2;
#ifdef TRACE
  uint32_t saved_sp = state.registers[SP];
#endif
  state.registers[SP] -= imm;
#ifdef TRACE
  printf("%08X: %04X      SUB SP, SP (%d $%08X), #%d $%08X => %d $%08X ",
	 state.registers[PC], instr, (int32_t)saved_sp, saved_sp,
	 (int32_t)imm, imm, (int32_t)state.registers[SP], state.registers[SP]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_svc(uint16_t instr) {
#ifdef TRACE
  printf("%08X: %04X      SVC ", state.registers[PC], instr);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_sxtb_reg_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rm_value = state.registers[rm];
  state.registers[rd] = (uint32_t)((int32_t)(rm_value << 24) >> 24);
#ifdef TRACE
  printf("%08X: %04X      SXTB R%d, R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[rd], state.registers[rd]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_sxth_reg_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rm_value = state.registers[rm];
  state.registers[rd] = (uint32_t)((int32_t)(rm_value << 16) >> 16);
#ifdef TRACE
  printf("%08X: %04X      SXTH R%d, R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[rd], state.registers[rd]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_tst_reg_1(uint16_t instr) {
  uint32_t rn = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rn_value = state.registers[rn];
  uint32_t rm_value = state.registers[rm];
  uint32_t result = rn_value & rn_value;
  UPDATE_APSR_BIT(result);
#ifdef TRACE
  printf("%08X: %04X      TST R%d (%d $%08X), R%d (%d $%08X) ",
	 state.registers[PC], instr, rn, (int32_t)rn_value, rn_value,
	 rm, (int32_t)rm_value, rm_value);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_uxtb_reg_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rm_value = state.registers[rm];
  state.registers[rd] = rm_value & 0xFF;
#ifdef TRACE
  printf("%08X: %04X      UXTB R%d, R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[rd], state.registers[rd]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_uxth_reg_1(uint16_t instr) {
  uint32_t rd = BF_0_3(instr);
  uint32_t rm = BF_3_3(instr);
  uint32_t rm_value = state.registers[rm];
  state.registers[rd] = rm_value & 0xFFFF;
#ifdef TRACE
  printf("%08X: %04X      UXTH R%d, R%d (%d $%08X) => %d $%08X ",
	 state.registers[PC], instr, rd, rm, (int32_t)rm_value, rm_value,
	 (int32_t)state.registers[rd], state.registers[rd]);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_wfe_1(uint16_t instr) {
#ifdef TRACE
  printf("%08X: %04X      WFE ", state.registers[PC], instr);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_wfi_1(uint16_t instr) {
#ifdef TRACE
  printf("%08X: %04X      WFI ", state.registers[PC], instr);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

void instr_yield_1(uint16_t instr) {
#ifdef TRACE
  printf("%08X: %04X      YIELD ", state.registers[PC], instr);
  print_apsr();
#endif
  ADVANCE_PC_2;
}

/* 32-bit instructions */

void instr_bl(uint16_t instr) {
  uint32_t saved_pc = state.registers[PC];
  uint32_t instr_extra = load_16(saved_pc + 2);
  if(saved_pc == state.registers[PC]) {
    if((instr_extra & 0xD000) != 0xD000) {
      instr_bl_extra(instr, instr_extra);
    } else {
      uint32_t imm10 = BF_0_10(instr);
      uint32_t s = (uint32_t)((instr >> 10) & 0x1);
      uint32_t imm11 = BF_0_11(instr_extra);
      uint32_t j1 = (uint32_t)((instr_extra >> 13) & 0x1);
      uint32_t j2 = (uint32_t)((instr_extra >> 11) & 0x1);
      uint32_t p0 = s << 31;
      uint32_t p1 = ((~(j1 ^ s)) & 0x1) << 30;
      uint32_t p2 = ((~(j2 ^ s)) & 0x1) << 29;
      uint32_t p3 = imm10 << 19;
      uint32_t p4 = imm11 << 8;
      uint32_t offset = (uint32_t)((int32_t)(p0 | p1 | p2 | p3 | p4) >> 7);
      state.registers[LR] = (saved_pc + 4) | 0x1;
      state.registers[PC] += 4 + offset;
#ifdef TRACE
      printf("%08X: %04X %04X BL $%08X => LR %d $%08X ",
	     saved_pc, (uint32_t)instr, (uint32_t)instr_extra,
	     state.registers[PC], (int32_t)state.registers[LR],
	     state.registers[LR]);
      print_apsr();
#endif
    }
  }
}

void instr_bl_extra(uint16_t instr, uint16_t extra) {
  if(((instr & 0xFFF0) == 0xF3B0) && ((instr_extra & 0xD000) == 0x8000)) {
    /* CLREX, DMB, DSB, or ISB instruction */
#ifdef TRACE
    printf("%08X: %04X %04X ",
	   state.registers[PC], (uint32_t)instr, (uint32_t)instr_extra);
    switch(instr_extra & 0x00F0) {
    case 0x0010:
      printf("CLREX ");
      break;
    case 0x0040:
      printf("DSB ");
      break;
    case 0x0050:
      printf("DMB ");
      break;
    case 0x0060:
      printf("ISB ");
      break;
    default
      printf("<UNKNOWN> ");
    }
    print_apsr();
#endif
    ADVANCE_PC_2;
  } else {
    fprintf(stderr, "UNRECOGNIZED INSTRUCTION: %04X %04X @ %08X\n",
	    (uint32_t)instr, (uint32_t)instr_extra, state.registers[PC]);
    exit(1);
  }
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
  register_handler(instr_bl, 0xF800, 0xF000);
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
  register_handler(instr_stmia_1, 0xF800, 0xC000);
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
  register_handler(instr_sub_sp_imm_1, 0xFF80, 0xB080);
  /*  register_handler(instr_svc, 0xFF00, 0xDF00); */
  register_handler(instr_sxtb_reg_1, 0xFFC0, 0xB240);
  register_handler(instr_sxth_reg_1, 0xFFC0, 0xB200);
  register_handler(instr_tst_reg_1, 0xFFC0, 0x4200);
  register_handler(instr_uxtb_reg_1, 0xFFC0, 0xB2C0);
  register_handler(instr_uxth_reg_1, 0xFFC0, 0xB280);
  register_handler(instr_wfe_1, 0xFFFF, 0xBF20);
  register_handler(instr_wfi_1, 0xFFFF, 0xBF30);
  register_handler(instr_yield_1, 0xFFFF,  0xBF10);
}
