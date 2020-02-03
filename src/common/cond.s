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
	define_word "if", visible_flag | immediate_flag | compile_only_flag
_if:	push {lr}
	push_tos
	movs tos, #0
	push_tos
	movs tos, #6
	bl _asm_cmp_imm
	bl _asm_reserve_branch
	push_tos
	movs tos, #-1
	pop {pc}

	@@ ADD MORE TO THIS WORD!
	@@ ELSE in an IF ELSE THEN block
	define_word "else", visible_flag | immediate_flag | compile_only_flag
_else:	push {lr}
	cmp r0, #0
	beq 1f
	movs r0, tos
	pull_tos
	push {r0}
	bl _current_here
	pop {r0}
	push_tos
	movs tos, r0
	bl _asm_branch_zero_back
	pop {pc}
1:	push_tos
	ldr tos, =_not_following_if
	bl _raise
	pop {pc}
	
	@@ Not following an IF exception
	define_word "not-following-if", visible_flag
_not_following_if:
	string " not following if"
	bl _type
	bl _abort
	
	@@ End an IF Block
	define_word "then", visible_flag | immediate_flag | compile_only_flag
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
