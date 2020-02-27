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

	@@ Skip to the start of a token
	define_word "skip-to-token", visible_flag
_skip_to_token:
	push {lr}
	bl _token_start
	ldr r0, =eval_count_ptr
	ldr r0, [r0]
	str tos, [r0]
	pull_tos
	pop {pc}
	
	@@ Parse to a character in the input stream
	define_word "parse-to-char", visible_flag
_parse_to_char:
	movs r0, tos
	ldr r1, =eval_index_ptr
	ldr r1, [r1]
	ldr r1, [r1]
	ldr r2, =eval_count_ptr
	ldr r2, [r2]
	ldr r2, [r2]
	ldr r3, =eval_ptr
	ldr r3, [r3]
	adds r1, r1, r3
	adds r2, r2, r3
1:	cmp r1, r2
	beq 2f
	ldrb r3, [r1]
	cmp r3, r0
	beq 2f
	adds r1, #1
	b 1b
2:	ldr r0, =eval_index_ptr
	ldr r0, [r0]
	ldr r2, [r0]
	ldr r3, =eval_ptr
	ldr r3, [r3]
	adds tos, r0, r3
	push_tos
	movs tos, r1
	subs tos, r3
	adds r1, #1
	str r1, [r0]
	bx lr

	@@ Immediately type a string in the input stream
	define_word ".(", visible_flag
_type_to_paren:
	push {lr}
	bl _skip_to_token
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
	bl _skip_to_token
	push_tos
	movs tos, #0x22
	bl _parse_to_char
	bl _compile_cstring
	pop {pc}

	@@ Compile a counted-string
	define_word "compile-cstring", visible_flag
_compile_cstring:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	movs tos, #4
	push_tos
	movs tos, #6
	push {r0, r1}
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

	@@ Parse a character and put it on the stack
	define_word "char", visible_flag
_char:	push {lr}
	bl _token
	cmp tos, #0
	beq 1f
	pull_tos
	ldrb tos, [tos]
	pop {pc}

	@@ Parse a character and compile it
	define_word "[char]", visible_flag | immediate_flag | compiled_flag
_compile_char:
	push {lr}
	bl _char
	push_tos
	movs tos, #6
	bl _asm_push
	push_tos
	movs tos, #6
	bl _asm_literal
	pop {pc}

	@@ Type an integer without a preceding space
	define_word "(.)", visible_flag
_type_integer:
	push {lr}
	ldr r0, =here
	ldr r0, [r0]
	movs r1, tos
	movs tos, r0
	push_tos
	movs tos, r1
	bl _format_integer
	movs r0, tos
	push_tos
	push {r0}
	bl _allot
	bl _type
	pop {r0}
	push_tos
	rsbs tos, r0, #0
	bl _allot
	pop {pc}

	@@ Type an unsigned integer without a preceding space
	define_word "(u.)", visible_flag
_type_unsigned:
	push {lr}
	ldr r0, =here
	ldr r0, [r0]
	movs r1, tos
	movs tos, r0
	push_tos
	movs tos, r1
	bl _format_unsigned
	movs r0, tos
	push_tos
	push {r0}
	bl _allot
	bl _type
	pop {r0}
	push_tos
	rsbs tos, r0, #0
	bl _allot
	pop {pc}

	@@ Type an integer with a preceding space
	define_word ".", visible_flag
_type_space_integer:
	push {lr}
	bl _space
	bl _type_integer
	pop {pc}

	@@ Type an unsigned integer with a preceding space
	define_word "u.", visible_flag
_type_space_unsigned:
	push {lr}
	bl _space
	bl _type_unsigned
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

	@@ Reverse bytes in place
	define_word "reverse", visible_flag
_reverse:
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
	adds r0, r0, r1
	subs r0, #1
1:	cmp r1, r0
	bge 2f
	ldrb r2, [r1]
	ldrb r3, [r0]
	strb r2, [r0]
	strb r3, [r1]
	adds r1, #1
	subs r0, #1
	b 1b
2:	bx lr

	@@ Format an unsigned integer as a string
	define_word "format-unsigned", visible_flag
_format_unsigned:
	push {lr}
	cmp tos, #0
	beq 1f
	bl _format_integer_inner
	ldr r0, =here
	ldr r0, [r0]
	movs r1, tos
	movs tos, r0
	push_tos
	movs tos, r1
	push {r0, r1}
	bl _reverse
	pop {r0, r1}
	movs r2, tos
	movs tos, r0
	push_tos
	movs tos, r2
	push_tos
	movs tos, r1
	push {r1, r2}
	bl _move
	pop {r1, r2}
	push_tos
	movs tos, r2
	push_tos
	movs tos, r1
	pop {pc}
1:	pull_tos
	movs r0, #0x30
	strb r0, [tos]
	push_tos
	movs tos, #1
	pop {pc}
	
	@@ Format an integer as a string
	define_word "format-integer", visible_flag
_format_integer:
	push {lr}
	cmp tos, #0
	blt 1f
	beq 2f
	bl _format_integer_inner
	ldr r0, =here
	ldr r0, [r0]
	movs r1, tos
3:	movs tos, r0
	push_tos
	movs tos, r1
	push {r0, r1}
	bl _reverse
	pop {r0, r1}
	movs r2, tos
	movs tos, r0
	push_tos
	movs tos, r2
	push_tos
	movs tos, r1
	push {r1, r2}
	bl _move
	pop {r1, r2}
	push_tos
	movs tos, r2
	push_tos
	movs tos, r1
	pop {pc}
1:	rsbs tos, tos, #0
	bl _format_integer_inner
	ldr r0, =here
	ldr r0, [r0]
	movs r1, tos
	adds r2, r0, r1
	movs r3, #0x2D
	strb r3, [r2]
	adds r1, #1
	b 3b
2:	pull_tos
	movs r0, #0x30
	strb r0, [tos]
	push_tos
	movs tos, #1
	pop {pc}

	@@ The inner portion of formatting an integer as a string
	define_word "format-integer-inner", visible_flag
_format_integer_inner:
	push {lr}
	ldr r0, =here
	ldr r0, [r0]
	ldr r1, =base
	ldr r1, [r1]
	movs r2, tos
1:	cmp r2, #0
	beq 3f
	movs tos, r2
	push_tos
	movs tos, r1
	push {r0, r1, r2}
	bl _umod
	pop {r0, r1, r2}
	udiv r2, r2, r1
	cmp tos, #10
	bge 2f
	adds tos, #0x30
	strb tos, [r0]
	adds r0, #1
	b 1b
2:	adds tos, #0x37
	strb tos, [r0]
	adds r0, #1
	b 1b
3:	ldr r1, =here
	ldr r1, [r1]
	subs tos, r0, r1
	pop {pc}
	
	.ltorg
