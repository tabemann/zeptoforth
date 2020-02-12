@ Copyright (c) 2020 Travis Bemann
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

	@@ Parse to a character in the input stream
	define_word "parse-to-char", visible_flag
_parse_to_char:
	movs r0, tos
	ldr r1, =input_buffer_index
	ldrh r1, [r1]
	ldr r2, =input_buffer_count
	ldrb r2, [r2]
	ldr r3, =input_buffer
	adds r1, r1, r3
	adds r2, r2, r3
1:	cmp r1, r2
	beq 2f
	ldrb r3, [r1]
	cmp r3, r0
	beq 2f
	adds r1, #1
	b 1b
2:	ldr r0, =input_buffer_index
	ldrh r2, [r0]
	ldr r3, =input_buffer
	adds tos, r0, r3
	push_tos
	movs tos, r1
	subs tos, r3
	adds r1, #1
	strh r1, [r0]
	bx lr

	@@ Immediately type a string in the input stream
	define_word ".(", visible_flag
_type_to_paren:
	push {lr}
	push_tos
	movs tos, #0x29
	bl _parse_to_char
	bl _type
	pop {pc}

	@@ Print a string immediately
	define_word ".\"", visible_flag | immediate_flag | compiled_flag
_type_compiled:
	push {lr}
	bl _compile_imm_string
	push_tos
	ldr tos, =_type
	bl _asm_call
	pop {pc}
	
	@@ Compile a non-counted string
	define_word "s\"", visible_flag | immediate_flag | compiled_flag
_compile_imm_string:
	push {lr}
	bl _compile_imm_cstring
	push_tos
	ldr tos, =_count
	bl _asm_call
	pop {pc}

	@@ Compile a counted-string
	define_word "c\"", visible_flag | immediate_flag | compiled_flag
_compile_imm_cstring:
	push {lr}
	push_tos
	movs tos, #0x22
	bl _parse_to_char
	movs r0, tos
	pull_tos
	movs r1, tos
	movs tos, #4
	push_tos
	movs tos, #6
	push {r0. r1}
	bl _asm_adr
	bl _current_here
	pop {r0, r1}
	adds tos, tos, r0
	adds tos, #2
	tst tos, #1
	bne 1f
	push {r0, r1}
	bl _asm_branch
	pop {r0, r1}
	push_tos
	movs tos, r1
	push_tos
	movs tos, r0
	bl _current_comma_cstring
	pop {pc}
1:	adds tos, #1
	push {r0, r1}
	bl _asm_branch
	pop {r0, r1}
	push_tos
	movs tos, r1
	push_tos
	movs tos, r0
	bl _current_comma_cstring
	push_tos
	movs tos, #0
	bl _current_comma_1
	pop {pc}
	
	@@ Copy bytes from one buffer to another one (which may overlap)
	define_word "move", visible_flag
_move:	push {lr}
	ldr r0, [dp, #0]
	ldr r1, [dp, #4]
	cmp r1, r0
	bgt 1f
	bl _move_from_high
	pop {pc}
1:	bl _move_from_low
	pop {pc}

	@@ Copy bytes starting at a high address
	define_word "<move", visible_flag
_move_from_high:
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
	movs r2, tos
	pull_tos
	adds r1, r1, r0
	adds r2, r2, r0
1:	cmp r0, #0
	beq 2f
	subs r0, #1
	subs r1, #1
	subs r2, #1
	ldrb r3, [r2]
	strb r3, [r1]
	b 1b
2:	bx lr

	@@ Copy bytes starting at a low address
	define_word "move>", visible_flag
_move_from_low:
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
	movs r2, tos
	pull_tos
1:	cmp r0, #0
	beq 2f
	subs r0, #1
	ldrb r3, [r2]
	strb r3, [r1]
	adds r1, #1
	adds r2, #2
	b 1b
2:	bx lr
	
	.ltorg
