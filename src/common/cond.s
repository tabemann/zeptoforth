@ Copyright (c) 2020 Travis Bemann
@
@ Permission is hereby granted, free of charge, to any person obtaining a copy
@ of this software and associated documentation files (the "Software"), to deal
@ in the Software without restriction, including without limitation the rights
@ to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
@ copies of the Software, and to permit persons to whom the Software is
@ furnished to do so, subject to the following conditions:
@ 
@ The above copyright notice and this permission notice shall be included in
@ all copies or substantial portions of the Software.
@ 
@ THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
@ IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
@ FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
@ AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
@ LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
@ OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
@ SOFTWARE.

	@@ Start an IF block
	define_word "if", visible_flag | immediate_flag | compiled_flag
_if:	push {lr}
	bl _asm_undefer_lit
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
	end_inlined

	@@ ELSE in an IF ELSE THEN block
	define_word "else", visible_flag | immediate_flag | compiled_flag
_else:	push {lr}
	bl _asm_undefer_lit
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
	end_inlined
	
	@@ Not following an IF exception
	define_word "not-following-if", visible_flag
_not_following_if:
	push {lr}
	string " not following if"
	bl _type
	pop {pc}
	end_inlined
	
	@@ End an IF block
	define_word "then", visible_flag | immediate_flag | compiled_flag
_then:	push {lr}
	bl _asm_undefer_lit
	movs r1, tos
	pull_tos
	movs r0, tos
	pull_tos
	push {r0, r1}
	bl _current_here
	pop {r0, r1}
	push_tos
	movs tos, r0
	cmp r1, #0
	beq 1f
	bl _asm_branch_zero_back
	pop {pc}
1:	bl _asm_branch_back
	pop {pc}
	end_inlined

	@@ Start a BEGIN block
	define_word "begin", visible_flag | immediate_flag | compiled_flag
_begin:	push {lr}
	bl _asm_undefer_lit
	bl _current_here
	pop {pc}

	@@ Start a WHILE block
	define_word "while", visible_flag | immediate_flag | compiled_flag
_while:	push {lr}
	bl _asm_undefer_lit
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
	end_inlined

	@@ End a BEGIN-WHILE-REPEAT block
	define_word "repeat", visible_flag | immediate_flag | compiled_flag
_repeat:
	push {lr}
	bl _asm_undefer_lit
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
	end_inlined

	@@ End a BEGIN-UNTIL block
	define_word "until", visible_flag | immediate_flag | compiled_flag
_until:	push {lr}
	bl _asm_undefer_lit
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
	end_inlined

	@@ End a BEGIN-AGAIN block
	define_word "again", visible_flag | immediate_flag | compiled_flag
_again:	push {lr}
	bl _asm_undefer_lit
	bl _asm_branch
	pop {pc}
	end_inlined

	.ltorg
	
