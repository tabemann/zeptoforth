@ Copyright (c) 2019-2020 Travis Bemann
@
@ This program is free software: you can redistribute it and/or modify
@ it under the terms of the GNU General Public License as published by
@ the Free Software Foundation, either version 3 of the License, or
@ (at your option) any later version.
@
@ This program is distributed in the hope that it will be useful,
@ but WITHOUT ANY WARRANTY; without even the implied warranty of
@ MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
@ GNU General Public License for more details.
@
@ You should have received a copy of the GNU General Public License
@ along with this program.  If not, see <http://www.gnu.org/licenses/>.

	@@ Compile the start of a word
	define_word "start-compile", visible flag
_asm_start:
	push {lr}
	push_tos
	movs tos, #4
	bl _current_comma_align
	push_tos
	bl _current_here
	ldr r0, =current_compile
	str tos, [r0]
	ldr r0, =current_flags
	movs r1, #0
	str r1, [r0]
	push_tos
	movs tos, #4
	bl _current_allot
	bl _asm_link
	bl _current_comma_cstring
	bl _current_here
	movs r0, #1
	ands tos, r0
	beq 1f
	movs tos, #0
	bl _current_comma_1
	push_tos
1:	ldr tos, =0xB500	@@ push {lr}
	bl _current_comma_2
	pop {pc}

	@@ Compile a CREATEd word
	define_word "create,", visible flag
_asm_create:
	push {lr}
	push_tos
	movs tos, #4
	bl _current_comma_align
	push_tos
	bl _current_here
	ldr r0, =current_compile
	str tos, [r0]
	ldr r0, =current_flags
	movs r1, #0
	str r1, [r0]
	push_tos
	movs tos, #4
	bl _current_allot
	bl _asm_link
	bl _current_comma_cstring
	bl _current_here
	movs r0, #1
	ands tos, r0
	beq 1f
	movs tos, #0
	bl _current_comma_1
	push_tos
1:	movs tos, #6
	bl _asm_push
	bl _current_here
	movs r0, #2
	ands tos, r0
	bne 2f
	movs tos, #4
	push_tos
	movs tos, #6
	bl _asm_adr
	push_tos
	movs tos, #14
	bl _asm_bx
	pop {pc}
2:	movs tos, #0
	push_tos
	movs tos, #0
	push_tos
	movs tos, #0
	bl _asm_lsl_imm
	push_tos
	movs tos, #4
	push_tos
	movs tos, #6
	bl _asm_adr
	push_tos
	movs tos, #14
	bl _asm_bx
	pop {pc}

	@@ Compile a BUILDed word
	define_word "<build,", visible_flag
_asm_build:
	push {lr}
	ldr r0, =building
	ldr r0, [r0]
	cmp r0, #0
	bne 3f
	push_tos
	movs tos, #4
	bl _current_comma_align
	push_tos
	bl _current_here
	ldr r0, =current_compile
	str tos, [r0]
	ldr r0, =current_flags
	movs r1, #0
	str r1, [r0]
	push_tos
	movs tos, #4
	bl _current_allot
	bl _asm_link
	bl _current_comma_cstring
	bl _current_here
	movs r0, #1
	ands tos, r0
	beq 1f
	movs tos, #0
	bl _current_comma_1
	push_tos
1:	ldr tos, =0xB500	@@ push {lr}
	bl _current_comma_2
	push_tos
	movs tos, #6
	bl _asm_push
	bl _current_here
	movs r0, #2
	ands tos, r0
	beq 2f
	movs tos, #0
	push_tos
	movs tos, #1
	push_tos
	movs tos, #1
	bl _ssm_lsl_imm
	push_tos
2:	movs tos, #8
	push_tos
	movs tos, #6
	bl _asm_adr
	push_tos
	movs tos, #-4
	push_tos
	movs tos, #6
	push_tos
	movs tos, #0
	bl _asm_ldr_imm
	push_tos
	movs tos, #0
	push_tos
	movs tos, #1
	push_tos
	movs tos, #1
	bl _asm_lsl_imm
	push_tos
	movs tos, #0
	bl _asm_bx
	bl _current_reserve_4
	ldr r0, =build_target
	str tos, [r0]
	pull_tos
	ldr r0, =building
	movs r1, #-1
	str r1, [r0]
	pop {pc}
3:	push_tos
	ldr tos, =_already_building
	bl _raise
	pop {pc}

	@@ Carry out DOES>
	define_word "does>", visible_flag
_asm_does:
	push {lr}
	ldr r0, =building
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	ldr tos, [sp]
	push_tos
	ldr r0, =build_target
	ldr tos, [r0]
	bl _store_current_4
	ldr r0, =building
	movs r1, #0
	str r1, [r0]
	pop {pc}
1:	push_tos
	ldr tos, =_not_building
	bl _raise
	pop {pc}

	@@ Compile a link field
	define_word "current-link,", visible_flag
_asm_link:
	push {lr}
	push_tos
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	ldr r0, =flash_latest
	b 2f
1:	ldr r0, =ram_latest
2:	ldr tos, [r0]
	bl _current_comma_4
	pop {pc}

	@@ FInalize the compilation of a word
	define_word "finalize,", visible_flag
_asm_finalize:
	push {lr}
	push_tos
	movs tos, #4
	bl _current_comma_align
	push_tos
	ldr tos, =latest_flags
	ldr tos, [tos]
	push_tos
	ldr tos, =latest
	ldr tos, [tos]
	bl _current_store_2
1:	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	ldr r0, =current_compile
	ldr r0, [r0]
	ldr tos, =flash_here
	ldr tos, [tos]
	subs tos, tos, r0
	bl _current_comma_4
	bl _flush_all_flash
	ldr r0, =current_compile
	ldr r1, [r0]
	ldr r2, =flash_latest
	str r1, [r2]
	b 2f
1:	ldr r0, =current_compile
	ldr r1, [r0]
	ldr r2, =ram_latest
	str r1, [r2]
2:	ldr r2, =latest
	str r1, [r2]
	movs r1, #0
	str r1, [r0]
	pop {pc}
	
	@@ Compile the end of a word
	define_word "end-compile,", visible_flag
_asm_end:
	push {lr}
	push_tos
	ldr tos, =0xBD00	@@ pop {pc}
	bl _current_comma_2
	bl _asm_finalize
	pop {pc}
	
	@@ Assemble a move immediate instruction
	define_word "mov-imm,", visible_flag
_asm_mov_imm:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, #7
	ands r0, r1
	movs r1, #0xFF
	ands tos, r1
	lsls r0, r0, #8
	orrs tos, r0
	ldr r0, =0x2000
	orrs tos, r0
	bl _current_comma_2
	pop {pc}	

	@@ Assemble a logical shift left immediate instruction
	define_word "lsl-imm,", visible_flag
_asm_lsl_imm:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, #7
	ands r0, r1
	movs r1, #0xFF
	ands tos, r1
	lsls r0, r0, #8
	orrs tos, r0
	movs r1, tos
	pull_tos
	movs r0, #0x1F
	ands tos, r0
	lsls tos, tos, #6
	orrs tos, r1
	bl _current_comma_2
	pop {pc}	

	@@ Assemble an reverse subtract immediate from zero instruction
	define "neg,", visible_flag
_asm_neg:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, #7
	ands tos, r1
	ands r0, r1
	lsls tos, tos, #3
	orrs tos, r0
	ldr r0, =0x4240
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Compile a blx (register) instruction
	define_word "blx-reg,", visible_flag
_asm_blx_reg:
	push {rl}
	movs r0, #0xF
	ands tos, r0
	lsls tos, tos, #3
	ldr r0, =0x4780
	orrs tos, r0
	bl _current_comma_2
	pop {pc}
	
	@@ Compile an unconditional branch
	define_word "branch,", visible_flag
_asm_branch:
	push {rl}
	bl _current_here
	movs r0, tos
	pull_tos
	subs tos, tos, r0
	asrs tos, tos, #1
	bl _asm_b
	pop {pc}

	@@ Compile a branch on equal to zero
	define_word "0branch,", visible_flag
_asm_branch_zero:
	push {rl}
	bl _current_here
	movs r0, tos
	pull_tos
	subs tos, tos, r0
	asrs tos, tos, #1
	bl _asm_beq
	pop {pc}

	@@ Compile a back-referenced unconditional branch
	define_word "branch-back!", visible_flag
_asm_branch_back:
	push {rl}
	movs r0, tos
	pull_tos
	subs tos, tos, r0
	asrs tos, tos, #1
	push_tos
	movs tos, r0
	bl _asm_b_back
	pop {pc}

	@@ Compile a back-referenced branch on equal to zero
	define_word "0branch-back!", visible_flag
_asm_branch_zero_back:
	push {rl}
	movs r0, tos
	pull_tos
	subs tos, tos, r0
	asrs tos, tos, #1
	push_tos
	movs tos, r0
	bl _asm_beq_back
	pop {pc}

	.ifdef thumb2

	@@ Call a word at an address
	define_word "call,", visible_flag
_asm_call:	
	push {rl}
	bl _current_here
	movs r0, tos
	pull_tos
	movs r1, tos
	subs tos, tos, r0
	ldr r2, =0x00FFFFFF
	cmp tos, r2
	bgt 1f
	ldr r2, =0xFF000000
	cmp tos, r2
	blt 1f
	bl _asm_bl
	pop {pc}
1:	movs tos, r1
	adds tos, #1
	push_tos
	movs tos, #1
	bl _asm_literal
	push_tos
	movs tos, #1
	bl _asm_blx_reg
	pop {pc}
	
	@@ Compile a bl instruction
	define_word "bl,", visible_flag
_asm_bl:
	push {rl}
	movs r0, tos
	lsrs tos, tos, #12
	ldr r1, =0x3FF
	ands tos, r1
	lsls r1, r0, #24
	movs r2, #1
	ands r1, r2
	lsls r2, r1, #10
	orrs tos, r2
	ldr r2, =0xF000
	orrs tos, r2
	push {r0, r1}
	bl _current_comma_2
	pop {r0, r1}
	push_tos
	movs tos, r0
	lsrs tos, tos, #1
	ldr r2, =0x7FF
	ands tos, r2
	lsrs r2, r0, #22
	movs r3, #1
	ands r2, r3
	mvn r2, r2
	eor r2, r1
	ands r2, r3
	lsls r2, r2, #11
	orrs tos, r2
	lsrs r2, r0, #23
	ands r2, r3
	mvn r2, r2
	eor r2, r1
	ands r2, r3
	lsls r2, r2, #13
	orrs tos, r2
	ldr r2, =0xD000
	orrs tos, r2
	bl _current_comma_2
	pop {pc}

	@@ Compile a move 16-bit immediate instruction
	define_word "mov-16-imm,", visible_flag
_asm_mov_16_imm:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	lsrs tos, tos, #11
	movs r2, #1
	ands tos, r2
	lsls tos, tos, #10
	movs r3, r1
	lsrs r3, r3, #12
	orrs tos, r3
	movs r3, #0xF240
	orrs tos, r3
	push {r0, r1}
	bl _current_comma_2
	pop {r0, r1}
	push_tos
	movs tos, r1
	movs r2, #0xFF
	ands tos, r2
	lsrs r1, r1, #8
	movs r2, #7
	ands r1, r2
	lsls r1, r1, #12
	orrs tos, r1
	movs r2, #0xF
	ands r0, r2
	lsls r0, r0, #8
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Compile a move top 16-bit immediate instruction
	define_word "movt-imm,", visible_flag
_asm_movt_imm:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	lsrs tos, tos, #11
	movs r2, #1
	ands tos, r2
	lsls tos, tos, #10
	movs r3, r1
	lsrs r3, r3, #12
	orrs tos, r3
	ldr r3, =0xF2C0
	orrs tos, r3
	push {r0, r1}
	bl _current_comma_2
	pop {r0, r1}
	push_tos
	movs tos, r1
	movs r2, #0xFF
	ands tos, r2
	lsrs r1, r1, #8
	movs r2, #7
	ands r1, r2
	lsls r1, r1, #12
	orrs tos, r1
	movs r2, #0xF
	ands r0, r2
	lsls r0, r0, #8
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble a literal
	define_word "literal,", visible_flag
_asm_literal:
	push {lr}
	movs r0, tos
	pull_tos
	cmp tos, #0
	blt 1f
	movs r2, #0xFF
	cmp tos, r2
	bgt 2f
	push_tos
	movs tos, r0
	bl _asm_mov_imm
	pop {pc}
2:	push_tos
	movs tos, r0
	bl _asm_ldr_long_imm
	pop {pc}
1:	neg tos, tos
	movs r2, #0xFF
	cmp tos, r2
	bgt 3f
	push_tos
	movs tos, r0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	bl _asm_neg
	pop {pc}
3:	ldr r1, =0xFFFF
	cmp tos, r1
	bgt 4f
	push_tos
	movs tos, r0
	push {r0}
	bl _asm_ldr_long_imm
	pop {r0}
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	bl _asm_neg
	pop {pc}
4:	neg tos, tos
	push_tos
	movs tos, r0
	bl _asm_ldr_long_imm
	pop {pc}
	
	@@ Assemble a long load register immediate pseudo-opcode
	define_word "ldr-long-imm,", visible_flag
_asm_ldr_long_imm:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	ldr r2, =0xFFFF
	ands tos, r2
	mov r2, #0xFF
	cmp tos, r2
	bgt 1f
	push_tos
	mov tos, r0
	push {r0, r1}
	bl _asm_mov_imm
	pop {r0, r1}
2:	ldr r2, =0x7FFF
	cmp r1, r2
	ble 3f
	push_tos
	movs tos, r1
	lsrs tos, tos, #16
	ands tos, r2
	push_tos
	movs tos, r0
	bl _asm_movt_imm
	pop {pc}
1:	push_tos
	movs tos, r0
	push {r0, r1}
	bl _asm_mov_16_imm
	pop {r0, r1}
	b 2b
3:	pop {pc}
	
	@@ Assemble an unconditional branch
	define_word "b,", visible_flag
_asm_b:	push {lr}
	ldr r0, =1023
	cmp tos, r0
	ble 1f
	ldr r0, =8388607
	cmp tos, r0
	ble 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-1024
	cmp tos, r0
	bge 3f
	ldr r0, =-8388608
	cmp tos, r0
	bge 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
2:	bl _asm_b_32
3:	bl _asm_b_16
	pop {pc}

	@@ Assemble a branch on equal zero instruction
	define_word "beq,", visible_flag
_asm_beq:
	push {lr}
	ldr r0, =127
	cmp tos, r0
	ble 1f
	ldr r0, =524287
	cmp tos, r0
	ble 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-128
	cmp tos, r0
	bge 3f
	ldr r0, =-524288
	cmp tos, r0
	bge 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
2:	bl _asm_beq_32
3:	bl _asm_beq_16
	pop {pc}

	@@ Assemble an unconditional branch
	define_word "b-back!", visible_flag
_asm_b_back:
	push {lr}
	movs r1, tos
	pull_tos
	ldr r0, =8388607
	cmp tos, r0
	ble 1f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-8388608
	cmp tos, r0
	bge 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
2:	push_tos
	movs tos, r1
	bl _asm_b_32_back
	pop {pc}

	@@ Assemble a branch on equal zero instruction
	define_word "beq-back!", visible_flag
_asm_beq_back:
	push {lr}
	movs r1, tos
	pull_tos
	ldr r0, =524287
	cmp tos, r0
	ble 1f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-524288
	cmp tos, r0
	bge 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
2:	push_tos
	movs tos, r1
	bl _asm_beq_32_back
	pop {pc}

	@@ Assemble an unconditional branch
	define_word "b-32,", visible_flag
_asm_b_32:
	push {lr}
	movs r0, tos
	lsrs tos, tos, #11
	ldr r1, =0x3FF
	ands tos, r1
	lsrs r1, r0, #23
	movs r2, #1
	ands r1, r2
	lsls r1, r1, #10
	orr tos, r1
	ldr r1, =0xF000
	orr tos, r1
	push {r0}
	bl _current_comma_2
	pop {r0}
	push_tos
	mov tos, r0
	ldr r1, =0x7FF
	ands tos, r1
	lsrs r1, r0, #21
	movs r2, #1
	ands r1, r2
	lsls r1, r1, #11
	orr tos, r1
	lsrs r1, r0, #22
	ands r1, r2
	lsls r1, r1, #13
	orr tos, r1
	ldr r1, =0x9000
	orr tos, r1
	bl _current_comma_2
	pop {pc}

	@@ Assemble a branch on equal zero instruction
	define_word "beq-32,", visible_flag
_asm_beq_32:
	push {lr}
	push {lr}
	movs r0, tos
	lsrs tos, tos, #11
	ldr r1, =0x3FF
	ands tos, r1
	lsrs r1, r0, #19
	movs r2, #1
	ands r1, r2
	lsls r1, r1, #10
	orr tos, r1
	ldr r1, =0xF000
	orr tos, r1
	push {r0}
	bl _current_comma_2
	pop {r0}
	push_tos
	mov tos, r0
	ldr r1, =0x7FF
	ands tos, r1
	lsrs r1, r0, #17
	movs r2, #1
	ands r1, r2
	lsls r1, r1, #11
	orr tos, r1
	lsrs r1, r0, #18
	ands r1, r2
	lsls r1, r1, #13
	orr tos, r1
	ldr r1, =0x8000
	orr tos, r1
	bl _current_comma_2
	pop {pc}

	@@ Assemble an unconditional branch
	define_word "b-32-back!", visible_flag
_asm_b_32_back:
	push {lr}
	movs r3, tos
	pull_tos
	movs r0, tos
	lsrs tos, tos, #11
	ldr r1, =0x3FF
	ands tos, r1
	lsrs r1, r0, #23
	movs r2, #1
	ands r1, r2
	lsls r1, r1, #10
	orr tos, r1
	ldr r1, =0xF000
	orr tos, r1
	push_tos
	movs tos, r3
	push {r0, r3}
	bl _store_current_2
	pop {r0, r3}
	push_tos
	mov tos, r0
	ldr r1, =0x7FF
	ands tos, r1
	lsrs r1, r0, #21
	movs r2, #1
	ands r1, r2
	lsls r1, r1, #11
	orr tos, r1
	lsrs r1, r0, #22
	ands r1, r2
	lsls r1, r1, #13
	orr tos, r1
	ldr r1, =0x9000
	orr tos, r1
	push_tos
	movs tos, r3
	add tos, #2
	bls _store_current_2
	pop {pc}

	@@ Assemble a branch on equal zero instruction
	define_word "beq-32-back!", visible_flag
_asm_beq_32_back:
	push {lr}
	movs r3, tos
	pull_tos
	movs r0, tos
	lsrs tos, tos, #11
	ldr r1, =0x3FF
	ands tos, r1
	lsrs r1, r0, #19
	movs r2, #1
	ands r1, r2
	lsls r1, r1, #10
	orr tos, r1
	ldr r1, =0xF000
	orr tos, r1
	push_tos
	movs tos, r3
	push {r0, r3}
	bl _store_current_2
	pop {r0, r3}
	push_tos
	mov tos, r0
	ldr r1, =0x7FF
	ands tos, r1
	lsrs r1, r0, #17
	movs r2, #1
	ands r1, r2
	lsls r1, r1, #11
	orr tos, r1
	lsrs r1, r0, #18
	ands r1, r2
	lsls r1, r1, #13
	orr tos, r1
	ldr r1, =0x8000
	orr tos, r1
	push_tos
	movs tos, r3
	adds tos, #2
	bl _store_current_2
	pop {pc}

	@@ Reserve space for a branch
	define_word "reserve-branch", visible_flag
_reserve_branch:
	push {lr}
	bl _current_reserve_4
	pop {pc}

	.else

	@@ Call a word at an address
	define_word "call,", visible_flag
_asm_call:	
	push {rl}
	bl _current_here
	movs r0, tos
	pull_tos
	movs r1, tos
	subs tos, tos, r0
	ldr r2, =0x003FFFFF
	cmp tos, r2
	bgt 1f
	ldr r2, =0xFFC00000
	cmp tos, r2
	blt 1f
	bl _asm_bl
	pop {pc}
1:	movs tos, r1
	adds tos, #1
	push_tos
	movs tos, #1
	bl _asm_literal
	push_tos
	movs tos, #1
	bl _asm_blx_reg
	pop {pc}

	@@ Compile a bl instruction
	define_word "bl,", visible_flag
_asm_bl:
	push {rl}
	movs r0, tos
	lsrs tos, tos, #12
	ldr r1, =0x7FF
	ands tos, r1
	ldr r2, =0xF000
	orrs tos, r2
	push {r0}
	bl _current_comma_2
	pop {r0}
	push_tos
	movs tos, r0
	lsrs tos, tos, #1
	ldr r2, =0x7FF
	ands tos, r2
	ldr r2, =0xF800
	orrs tos, r2
	bl _current_comma_2
	pop {pc}

	@@ Assemble a literal
	define_word "literal,", visible_flag
_asm_literal:
	push {lr}
	movs r0, tos
	pull_tos
	cmp tos, #0
	blt 1f
	movs r1, #0xFF
	cmp tos, r1
	bgt 2f
	push_tos
	movs tos, r0
	bl _asm_mov_imm
	pop {pc}
2:	push_tos
	movs tos, r0
	bl _asm_ldr_long_imm
	pop {pc}
1:	neg tos, tos
	movs r0, #0xFF
	cmp tos, r0
	bgt 2f
	push_tos
	movs tos, r0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	bl _asm_neg
	pop {pc}
2:	push_tos
	movs tos, r0
	push {r0}
	bl _asm_ldr_long_imm
	pop {r0}
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	bl _asm_neg
	pop {pc}

	@@ Assemble a long load register immediate pseudo-opcode
	define_word "ldr-long-imm,", visible_flag
_asm_ldr_long_imm:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	lsrs r1, r1, #24
	bne 1f
	push_tos
	movs tos, r0
	bl _asm_ldr_long_imm_1st_zero
	pop {pc}
1:	movs r2, tos
	mv tos, r1
	push_tos
	mv tos, r0
	push {r0, r2}
	bl _asm_mov_imm
	pop {r0, r2}
	movs r1, r2
	lsrs r1, r1, #16
	movs r3, #0xFF
	ands r1, r3
	bne 1f
	push_tos
	movs tos, r2
	push_tos
	movs tos, r0
	bl _asm_ldr_long_imm_2nd_zero
	pop {pc}
1:	push_tos
	movs tos, #8
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r1, r2}
	bl _asm_lsl_imm
	pop {r0, r1, r2}
	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	push {r0, r2}
	bl _asm_mov_imm
	pop {r0, r2}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_orr
	pop {r0, r2}
	movs r1, r2
	ldr r1, r1, #8
	movs r3, #0xFF
	ands r1, r3
	bne 1f
	push_tos
	movs tos, r2
	push_tos
	movs tos, r0
	bl _asm_ldr_long_imm_3rd_zero
	pop {pc}
1:	push_tos
	movs tos, #8
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r1, r2}
	bl _asm_lsl_imm
	pop {r0, r1, r2}
	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	push {r0, r2}
	bl _asm_mov_imm
	pop {r0, r2}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_orr
	pop {r0, r2}
	movs r3, #0xFF
	ands r2, r3
	bne 1f
	pop {pc}
1:	push_tos
	movs tos, #8
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_lsl_imm
	pop {r0, r2}
	push_tos
	movs tos, r2
	push_tos
	movs tos, #0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	bl _asm_orr
	pop {pc}

	@@ Assemble a long load register immediate pseudo-opcode
	define_word "ldr-long-imm-1st-zero,", visible_flag
_asm_ldr_long_imm_1st_zero:
	push {lr}
	movs r0, tos
	pull_tos
	movs r2, tos
	movs r1, r2
	lsrs r1, r1, #16
	movs r3, #0xFF
	ands r1, r3
	bne 1f
	push_tos
	movs tos, r2
	push_tos
	movs tos, r0
	bl _asm_ldr_long_imm_1st_2nd_zero
	pop {pc}
1:	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	push {r0, r2}
	bl _asm_mov_imm
	pop {r0, r2}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_orr
	pop {r0, r2}
	movs r1, r2
	ldr r1, r1, #8
	movs r3, #0xFF
	ands r1, r3
	bne 1f
	push_tos
	movs tos, r2
	push_tos
	movs tos, r0
	bl _asm_ldr_long_imm_1st_3rd_zero
	pop {pc}
1:	push_tos
	movs tos, #8
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r1, r2}
	bl _asm_lsl_imm
	pop {r0, r1, r2}
	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	push {r0, r2}
	bl _asm_mov_imm
	pop {r0, r2}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_orr
	pop {r0, r2}
	push_tos
	movs tos, #8
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_lsl_imm
	pop {r0, r2}
	movs r3, #0xFF
	ands r2, r3
	bne 1f
	pop {pc}
1:	push_tos
	movs tos, r2
	push_tos
	movs tos, #0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	bl _asm_orr
	pop {pc}

	@@ Assemble a long load register immediate pseudo-opcode
	define_word "ldr-long-imm-2nd-zero,", visible_flag
_asm_ldr_long_imm_2nd_zero:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	movs r1, r2
	ldr r1, r1, #8
	movs r3, #0xFF
	ands r1, r3
	bne 1f
	push_tos
	movs tos, r2
	push_tos
	movs tos, r0
	bl _asm_ldr_long_imm_2nd_3rd_zero
	pop {pc}
1:	push_tos
	movs tos, #16
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r1, r2}
	bl _asm_lsl_imm
	pop {r0, r1, r2}
	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	push {r0, r2}
	bl _asm_mov_imm
	pop {r0, r2}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_orr
	pop {r0, r2}
	movs r3, #0xFF
	ands r2, r3
	bne 1f
	pop {pc}
1:	push_tos
	movs tos, #8
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_lsl_imm
	pop {r0, r2}
	push_tos
	movs tos, r2
	push_tos
	movs tos, #0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	bl _asm_orr
	pop {pc}

	@@ Assemble a long load register immediate pseudo-opcode
	define_word "ldr-long-imm-1st-2nd-zero,", visible_flag
_asm_ldr_long_imm_1st_2nd_zero:
	push {lr}
	movs r0, tos
	pull_tos
	movs r2, tos
	movs r1, r2
	ldr r1, r1, #8
	movs r3, #0xFF
	ands r1, r3
	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	push {r0, r2}
	bl _asm_mov_imm
	pop {r0, r2}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_orr
	pop {r0, r2}
	push_tos
	movs tos, #8
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_lsl_imm
	pop {r0, r2}
	movs r3, #0xFF
	ands r2, r3
	bne 1f
	pop {pc}
1:	push_tos
	movs tos, r2
	push_tos
	movs tos, #0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	bl _asm_orr
	pop {pc}

	@@ Assemble a long load register immediate pseudo-opcode
	define_word "ldr-long-imm-1st-3rd-zero,", visible_flag
_asm_ldr_long_imm_3rd_zero:
	push {lr}
	movs r0, tos
	pull_tos
	movs r2, tos
	movs tos, #8
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_lsl_imm
	pop {r0, r2}
	movs r3, #0xFF
	ands r2, r3
	bne 1f
	pop {pc}
1:	push_tos
	movs tos, r2
	push_tos
	movs tos, #0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	bl _asm_orr
	pop {pc}

	@@ Assemble a long load register immediate pseudo-opcode
	define_word "ldr-long-imm-1st-3rd-zero,", visible_flag
_asm_ldr_long_imm_1st_3rd_zero:
	push {lr}
	movs r0, tos
	pull_tos
	movs r2, tos
	movs tos, #16
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_lsl_imm
	pop {r0, r2}
	movs r3, #0xFF
	ands r2, r3
	bne 1f
	pop {pc}
1:	push_tos
	movs tos, r2
	push_tos
	movs tos, #0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	bl _asm_orr
	pop {pc}

	@@ Assemble a long load register immediate pseudo-opcode
	define_word "ldr-long-imm-2nd-3rd-zero,", visible_flag
_asm_ldr_long_imm_2nd_3rd_zero:
	push {lr}
	movs r0, tos
	pull_tos
	movs r2, tos
	movs tos, #24
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	push {r0, r2}
	bl _asm_lsl_imm
	pop {r0, r2}
	movs r3, #0xFF
	ands r2, r3
	bne 1f
	pop {pc}
1:	push_tos
	movs tos, r2
	push_tos
	movs tos, #0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, #0
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	bl _asm_orr
	pop {pc}

	@@ Assemble an unconditional branch
	define_word "b,", visible_flag
_asm_b:	push {lr}
	ldr r0, =1023
	cmp tos, r0
	ble 1f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-1024
	cmp tos, r0
	bge 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
2:	bl _asm_b_16
	pop {pc}

	@@ Assemble a branch on equal zero instruction
	define_word "beq,", visible_flag
_asm_beq:
	push {lr}
	ldr r0, =127
	cmp tos, r0
	ble 1f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-128
	cmp tos, r0
	bge 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
2:	bl _asm_beq_16
	pop {pc}

	@@ Assemble an unconditional branch
	define_word "b-back!", visible_flag
_asm_b_back:
	push {lr}
	movs r1, tos
	pull_tos
	ldr r0, =1023
	cmp tos, r0
	ble 1f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-1024
	cmp tos, r0
	bge 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
2:	push_tos
	movs tos, r1
	bl _asm_b_16_back
	pop {pc}

	@@ Assemble a branch on equal zero instruction
	define_word "beq-back!", visible_flag
_asm_beq_back:
	push {lr}
	movs r1, tos
	pull_tos
	ldr r0, =127
	cmp tos, r0
	ble 1f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-128
	cmp tos, r0
	bge 2f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
2:	push_tos
	movs tos, r1
	bl _asm_beq_16_back
	pop {pc}

	@@ Reserve space for a branch
	define_word "reserve-branch", visible_flag
_reserve_branch:
	push {lr}
	bl _current_reserve_2
	pop {pc}

	.endif

	@@ Out of range branch exception
	define_word "out-of-range-branch", visible_flag
_out_of_range_branch:
	string_ln " out of range branch"
	bl _type
	bl _abort

	@@ Already building exception
	define_word "already-building", visible_flag
_already_building:
	string_ln " already building"
	bl _type
	bl _abort

	@@ Not building exception
	define_word "not-building", visible_flag
_not_building:
	string_ln " not building"
	bl _type
	bl _abort
	
	@@ Assemble an unconditional branch
	define_word "b-16,", visible_flag
_asm_b_16:
	push {lr}
	ldr r0, =0xE000
	ldr r1, =0x7FF
	ands tos, r1
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble a branch on equal zero instruction
	define_word "beq-16,", visible_flag
_asm_beq_16:
	push {lr}
	ldr r0, =0xD000
	movs r1, #0xFF
	ands tos, r1
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble an unconditional branch
	define_word "b-16-back!", visible_flag
_asm_b_16_back:
	push {lr}
	movs r2, tos
	pull_tos
	ldr r0, =0xE000
	ldr r1, =0x7FF
	ands tos, r1
	orrs tos, r0
	push_tos
	movs tos, r2
	bl _store_current_2
	pop {pc}

	@@ Assemble a branch on equal zero instruction
	define_word "beq-16-back!", visible_flag
_asm_beq_16_back:
	push {lr}
	movs r2, tos
	pull_tos
	ldr r0, =0xD000
	movs r1, #0xFF
	ands tos, r1
	orrs tos, r0
	push_tos
	movs tos, r2
	bl _store_current_2
	pop {pc}

	@@ Assemble a compare to immediate instruction
	define_word "cmp-imm,", visible_flag
_asm_cmp_imm:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, #7
	ands r0, r1
	movs r1, #0xFF
	ands tos, r1
	lsls r0, r0, #8
	orrs tos, r0
	ldr r0, =0x2800
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble a logical shift left immediate instruction
	define_word "lsl-imm,", visible_flag
_asm_lsl_imm:
	push {lr}
	movs r0, tos
	movs r1, #7
	ands r0, r1
	pull_tos
	ands tos, r1
	lsls tos, tos, #3
	orrs r0, tos
	pull_tos
	movs r1, #0x1F
	ands tos, r1
	lsls tos, tos, #6
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble an or instruction
	define_word "orr,", visible_flag
_asm_orr:
	push {lr}
	movs r1, #7
	ands tos, r1
	movs r0, tos
	pull_tos
	ands tos, r1
	lsls tos, tos, #3
	orrs tos, r0
	movs r0, #0x43
	lsls r0, r0, #8
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble an str immediate instruction
	define_word "ldr-imm,", visible_flag
_asm_ldr_imm:
	push {lr}
	movs r0, #7
	ands tos, r0
	movs r1, tos
	pull_tos
	ands tos, r0
	lsls tos, tos, #3
	orrs r1, tos
	pull_tos
	asrs tos, tos, #2
	movs r0, #0x1F
	ands tos, r0
	lsls tos, tos, #6
	orrs tos, r1
	ldr r0, =0x6800
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble an str immediate instruction
	define_word "str-imm,", visible_flag
_asm_str_imm:
	push {lr}
	movs r0, #7
	ands tos, r0
	movs r1, tos
	pull_tos
	ands tos, r0
	lsls tos, tos, #3
	orrs r1, tos
	pull_tos
	asrs tos, tos, #2
	movs r0, #0x1F
	ands tos, r0
	lsls tos, tos, #6
	orrs tos, r1
	ldr r0, =0x6000
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble a subtract immediate instruction
	define_word "add-imm,", visible_flag
_asm_add_imm:
	push {lr}
	movs r0, #7
	ands tos, r0
	lsls r1, tos, #8
	pull_tos
	movs r0, #0xFF
	ands tos, r0
	orrs tos, r1
	ldr r0, =0x3000
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble a subtract immediate instruction
	define_word "sub-imm,", visible_flag
_asm_sub_imm:
	push {lr}
	movs r0, #7
	ands tos, r0
	lsls r1, tos, #8
	pull_tos
	movs r0, #0xFF
	ands tos, r0
	orrs tos, r1
	ldr r0, =0x3800
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble instructions to pull a value from the stack
	define_word "pull,", visible_flag
_asm_pull:
	push {lr}
	movs r0, tos
	movs tos, #0
	push_tos
	movs tos, #7
	push_tos
	movs tos, r0
	bl _asm_ldr_imm
	push_tos
	movs tos, #4
	push_tos
	movs tos, #7
	bl _asm_add_imm
	pop {pc}

	@@ Assemble instructions to push a value onto the stack
	define_word "push,", visible_flag
_asm_push:
	push {lr}
	movs r0, tos
	movs tos, #4
	push_tos
	movs tos, #7
	push {r0}
	bl _asm_sub_imm
	pop {r0}
	push_tos
	movs tos, #0
	push_tos
	movs tos, #7
	push_tos
	movs tos, r0
	bl _asm_str_imm
	pop {pc}
	
	@@ Assemble an instruction to generate a PC-relative address
	define_word "adr,", visible_flag
_asm_adr:
	push {lr}
	movs r1, #7
	ands r1, tos
	lsls r1, r1, #8
	pull_tos
	asrs tos, tos, #2
	movs r0, #0xFF
	ands tos, r0
	orrs tos, r1
	ldr r0, =0xC000
	orrs tos, r0
	bl _current_comma_2
	pop {pc}

	@@ Assemble a BX instruction
	define_word "bx,", visible_flag
_asm_bx:
	push {lr}
	movs r0, 0xF
	ands tos, r0
	lsls tos, tos, #3
	ldr r0, =0x4700
	orrs tos, r0
	bl _current_comma_2
	pop {pc}
