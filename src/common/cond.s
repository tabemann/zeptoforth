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

	@@ Start an IF block
	define_word "if", visible_flag | immediate_flag | compiled_flag
_if:	push {lr}
	push_tos
	movs tos, #0
	push_tos
	movs tos, #6
	push_tos
	movs tos, #0
	bl _asm_lsl_imm
	push_tos
	movs tos, #6
	bl _asm_pull
	push_tos
	movs tos, #0
	push_tos
	movs tos, #0
	bl _asm_cmp_imm
	bl _asm_reserve_branch
	push_tos
	movs tos, #-1
	pop {pc}

	@@ ELSE in an IF ELSE THEN block
	define_word "else", visible_flag | immediate_flag | compiled_flag
_else:	push {lr}
	cmp tos, #0
	beq 1f
	pull_tos
	movs r0, tos
	pull_tos
	push {r0}
	bl _asm_reserve_branch
	bl _current_here
	pop {r0}
	push_tos
	movs tos, r0
	bl _asm_branch_zero_back
	push_tos
	movs tos, #0
	pop {pc}
1:	ldr tos, =_not_following_if
	bl _raise
	pop {pc}
	
	@@ Not following an IF exception
	define_word "not-following-if", visible_flag
_not_following_if:
	push {lr}
	string " not following if"
	bl _type
	pop {pc}
	
	@@ End an IF block
	define_word "then", visible_flag | immediate_flag | compiled_flag
_then:	push {lr}
	movs r1, tos
	pull_tos
	movs r0, tos
	pull_tos
	push {r0}
	bl _current_here
	pop {r0}
	push_tos
	movs tos, r0
	cmp r1, #0
	beq 1f
	bl _asm_branch_zero_back
	pop {pc}
1:	bl _asm_branch_back
	pop {pc}

	@@ Start a BEGIN block
	define_word "begin", visible_flag | immediate_flag | compiled_flag
_begin:	push {lr}
	bl _current_here
	pop {pc}

	@@ Start a WHILE block
	define_word "while", visible_flag | immediate_flag | compiled_flag
_while:	push {lr}
	push_tos
	movs tos, #0
	push_tos
	movs tos, #6
	push_tos
	movs tos, #0
	bl _asm_lsl_imm
	push_tos
	movs tos, #6
	bl _asm_pull
	push_tos
	movs tos, #0
	push_tos
	movs tos, #0
	bl _asm_cmp_imm
	bl _asm_reserve_branch
	pop {pc}

	@@ End a BEGIN-WHILE-REPEAT block
	define_word "repeat", visible_flag | immediate_flag | compiled_flag
_repeat:
	push {lr}
	movs r0, tos
	pull_tos
	push {r0}
	bl _asm_branch
	bl _current_here
	pop {r0}
	push_tos
	movs tos, r0
	bl _asm_branch_zero_back
	pop {pc}

	@@ End a BEGIN-UNTIL block
	define_word "until", visible_flag | immediate_flag | compiled_flag
_until:	push {lr}
	push_tos
	movs tos, #0
	push_tos
	movs tos, #6
	push_tos
	movs tos, #0
	bl _asm_lsl_imm
	push_tos
	movs tos, #6
	bl _asm_pull
	push_tos
	movs tos, #0
	push_tos
	movs tos, #0
	bl _asm_cmp_imm
	bl _asm_branch_zero
	pop {pc}

	@@ End a BEGIN-AGAIN block
	define_word "again", visible_flag | immediate_flag | compiled_flag
_again:	push {lr}
	bl _asm_branch
	pop {pc}

	.ltorg
	
