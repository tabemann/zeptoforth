@ Copyright (c) 2019-2022 Travis Bemann
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

	@@ Compile the start of a word without the push {lr}
	define_internal_word "start-compile-no-push", visible_flag
_asm_start_no_push:
	push {lr}
	bl _asm_undefer_lit
	movs r0, #0
	ldr r1, =consts_used_count
	str r0, [r1]
	ldr r1, =set_const_end
	str r0, [r1]
	ldr r1, =suppress_inline
	str r0, [r1]
	push_tos
	movs tos, #4
	bl _current_comma_align
	bl _current_here
	ldr r0, =current_compile
	str tos, [r0]
	ldr r0, =current_flags
	movs r1, #0
	str r1, [r0]
	movs tos, #2
	bl _current_allot
	bl _get_current
	bl _current_comma_2
	bl _asm_link
	bl _current_comma_cstring
	bl _current_here
	movs r0, #1
	ands tos, r0
	beq 1f
	movs tos, #0
	bl _current_comma_1
	pop {pc}
1:	pull_tos
	pop {pc}
	end_inlined
	
	@@ Compile the start of a word
	define_internal_word "start-compile", visible_flag
_asm_start:
	push {lr}
	bl _asm_start_no_push
	push_tos
	ldr tos, =0xB500	@@ push {lr}
	bl _current_comma_2
	pop {pc}
	end_inlined

	@@ Compile a link field
	define_internal_word "current-link,", visible_flag
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
	end_inlined

	@@ Finalize the compilation of a word
	define_internal_word "finalize,", visible_flag
_asm_finalize:
	push {lr}
	bl _asm_undefer_lit
	bl _asm_word_align
	bl _asm_dump_consts
	push_tos
	ldr tos, =current_flags
	ldr tos, [tos]
	ldr r0, =suppress_inline
	ldr r0, [r0]
	ldr r1, =inlined_flag
	ands r0, r1
	bics tos, r0
	ldr r0, =current_flags
	str tos, [r0]
	push_tos
	ldr tos, =current_compile
	ldr tos, [tos]
	bl _store_current_2
1:	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	ldr r0, =compress_flash_enabled
	ldr r0, [r0]
	cmp r0, #0
	bne 3f
	push_tos
	ldr r0, =current_compile
	ldr tos, [r0]
	bl _current_comma_4
	push_tos
	ldr tos, =0xDEADBEEF
	bl _current_comma_4
	bl _flash_align
3:	bl _asm_resolve_const_end
	ldr r0, =current_compile
	ldr r1, [r0]
	ldr r2, =flash_latest
	str r1, [r2]
	b 2f
1:	bl _asm_resolve_const_end
	ldr r0, =current_compile
	ldr r1, [r0]
	ldr r2, =ram_latest
	str r1, [r2]
2:	ldr r2, =latest
	str r1, [r2]
	movs r1, #0
	str r1, [r0]
	push_tos
	ldr tos, =finalize_hook
	ldr tos, [tos]
	bl _execute_nz
	pop {pc}
	end_inlined
	
	@@ Finalize the compilation of a word without aligning
	define_internal_word "finalize-no-align,", visible_flag
_asm_finalize_no_align:
	push {lr}
	bl _asm_undefer_lit
	bl _asm_dump_consts
	bl _asm_word_align
	push_tos
	ldr tos, =current_flags
	ldr tos, [tos]
	ldr r0, =suppress_inline
	ldr r0, [r0]
	ldr r1, =inlined_flag
	ands r0, r1
	bics tos, r0
	ldr r0, =current_flags
	str tos, [r0]
	push_tos
	ldr tos, =current_compile
	ldr tos, [tos]
	bl _store_current_2
1:	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	ldr r0, =compress_flash_enabled
	ldr r0, [r0]
	cmp r0, #0
	bne 3f
	push_tos
	ldr r0, =current_compile
	ldr tos, [r0]
	bl _current_comma_4
	push_tos
	ldr tos, =0xDEADBEEF
	bl _current_comma_4
3:	bl _asm_resolve_const_end
	ldr r0, =current_compile
	ldr r1, [r0]
	ldr r2, =flash_latest
	str r1, [r2]
	b 2f
1:	bl _asm_resolve_const_end
	ldr r0, =current_compile
	ldr r1, [r0]
	ldr r2, =ram_latest
	str r1, [r2]
2:	ldr r2, =latest
	str r1, [r2]
	movs r1, #0
	str r1, [r0]
	push_tos
	ldr tos, =finalize_hook
	ldr tos, [tos]
	bl _execute_nz
	pop {pc}
	end_inlined

	.ltorg
	
	@@ Compile the end of a word
	define_internal_word "end-compile,", visible_flag
_asm_end:
	push {lr}
	bl _asm_undefer_lit
	push_tos
	ldr tos, =0xBD00	@@ pop {pc}
	bl _current_comma_2
	push_tos
	ldr tos, =0x003F        @@ movs r7, r7
	bl _current_comma_2
	bl _asm_finalize
	pop {pc}
	end_inlined

	@@ End flash compression
	define_word "end-compress-flash", visible_flag
_asm_end_compress_flash:
	push {lr}
	bl _asm_undefer_lit
	ldr r0, =compress_flash_enabled
	ldr r1, [r0]
	cmp r1, #0
	beq 1f
	movs r1, #0
	str r1, [r0]
	bl _asm_word_align
	push_tos
	ldr r0, =flash_latest
	ldr tos, [r0]
	bl _flash_comma_4
	push_tos
	ldr tos, =0xDEADBEEF
	bl _flash_comma_4
	bl _flash_align
1:	pop {pc}
	end_inlined

	@@ Commit code to flash without finishing compressing it
	define_word "commit-flash", visible_flag
_asm_commit_flash:
	push {lr}
	bl _asm_undefer_lit
	ldr r0, =compress_flash_enabled
	ldr r1, [r0]
	cmp r1, #0
	beq 1f
	bl _asm_word_align
	bl _flash_align
1:	pop {pc}
	end_inlined

	@@ Assemble a move immediate instruction
	define_internal_word "mov-imm,", visible_flag
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
	end_inlined

	@@ Assemble an reverse subtract immediate from zero instruction
	define_internal_word "neg,", visible_flag
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
	end_inlined

	@@ Compile a blx (register) instruction
	define_internal_word "blx-reg,", visible_flag
_asm_blx_reg:
	push {lr}
	movs r0, #0xF
	ands tos, r0
	lsls tos, tos, #3
	ldr r0, =0x4780
	orrs tos, r0
	bl _current_comma_2
	pop {pc}
	end_inlined
	
	@@ Compile an unconditional branch
	define_internal_word "branch,", visible_flag
_asm_branch:
	push {lr}
	bl _current_here
	movs r0, tos
	pull_tos
	subs tos, tos, r0
	asrs tos, tos, #1
	subs tos, #2
	bl _asm_b
	pop {pc}
	end_inlined

	@@ Compile a branch on equal to zero
	define_internal_word "0branch,", visible_flag
_asm_branch_zero:
	push {lr}
	bl _current_here
	movs r0, tos
	pull_tos
	subs tos, tos, r0
	asrs tos, tos, #1
	subs tos, #2
	bl _asm_beq
	pop {pc}
	end_inlined

	@@ Compile a branch on not equal to zero
	define_internal_word "n0branch,", visible_flag
_asm_branch_not_zero:
	push {lr}
	bl _current_here
	movs r0, tos
	pull_tos
	subs tos, tos, r0
	asrs tos, tos, #1
	subs tos, #2
	bl _asm_bne
	pop {pc}
	end_inlined

	@@ Compile a back-referenced unconditional branch
	define_internal_word "branch-back!", visible_flag
_asm_branch_back:
	push {lr}
	movs r0, tos
	pull_tos
	subs tos, tos, r0
	asrs tos, tos, #1
	subs tos, #2
	push_tos
	movs tos, r0
	bl _asm_b_back
	pop {pc}
	end_inlined

	@@ Compile a back-referenced branch on equal to zero
	define_internal_word "0branch-back!", visible_flag
_asm_branch_zero_back:
	push {lr}
	movs r0, tos
	pull_tos
	subs tos, tos, r0
	asrs tos, tos, #1
	subs tos, #2
	push_tos
	movs tos, r0
	bl _asm_beq_back
	pop {pc}
	end_inlined

	@@ Extract the value of a constant
	define_internal_word "extract-constant", visible_flag
_asm_extract_constant:
	push {lr}
	ldrh r0, [tos]
	ldr r1, =0xB500
	cmp r0, r1
	beq 1f
3:	movs tos, #0
	push_tos
	pop {pc}
1:	adds tos, #2
	ldrh r0, [tos]
	ldr r1, =0x3F04
	cmp r0, r1
	bne 3b
	adds tos, #2
	ldrh r0, [tos]
	ldr r1, =0x603E
	cmp r0, r1
	bne 3b
	adds tos, #2
	ldrh r0, [tos]
	ldr r1, =0xFF00
	movs r2, r0
	ands r2, r1
	ldr r1, =0x2600
	cmp r2, r1
	bne 3b
2:	ldr r1, =0x00FF
	ands r0, r1
4:	adds tos, #2
	ldrh r2, [tos]
	ldr r1, =0xBD00
	cmp r2, r1
	bne 3b
	movs tos, r0
	push_tos
	ldr tos, =-1
	pop {pc}
	end_inlined

	.ltorg
	
	@@ Fold a word
	define_internal_word "fold,", visible_flag
_asm_fold:
	push {lr}
	ldr r1, =literal_deferred_q
	ldr r0, [r1]
	cmp r0, #0
	bne 1f
	bl _asm_inline
	pop {pc}
1:	ldr r0, =_add
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_add
	pop {pc}
1:	ldr r0, =_sub
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_sub
	pop {pc}
1:	ldr r0, =_mul
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_mul
	pop {pc}
1:	ldr r0, =_and
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_and
	pop {pc}
1:	ldr r0, =_or
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_or
	pop {pc}
1:	ldr r0, =_xor
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_xor
	pop {pc}
1:	ldr r0, =_lshift
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_lshift
	pop {pc}
1:	ldr r0, =_rshift
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_rshift
	pop {pc}
1:	ldr r0, =_arshift
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_arshift
	pop {pc}
1:	ldr r0, =_store_1
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_store_1
	pop {pc}
1:	ldr r0, =_store_2
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_store_2
	pop {pc}
1:	ldr r0, =_store_4
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_store_4
	pop {pc}
1:	ldr r0, =_pick
	cmp tos, r0
	bne 1f
	pull_tos
	bl _asm_fold_pick
	pop {pc}
1:	bl _asm_undefer_lit
	bl _asm_inline
	pop {pc}
	end_inlined

	@@ Constant fold +
	define_internal_word "fold+", visible_flag
_asm_fold_add:
	push {lr}
	ldr r0, =deferred_literal
	ldr r1, [r0]
	ldr r2, =255
	cmp r1, r2
	bgt 2f
	cmp r1, #0
	blt 1f
	push_tos
	movs tos, r1
	push_tos
	movs tos, #6
	bl _asm_add_imm
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
1:	mvns r3, r1
	adds r3, #1
	cmp r3, r2
	bhi 2f
	push_tos
	movs tos, r3
	push_tos
	movs tos, #6
	bl _asm_sub_imm
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
2:	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	bl _asm_literal
	push_tos
	ldr tos, =0x1836
	bl _current_comma_2
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold -
	define_internal_word "fold-", visible_flag
_asm_fold_sub:
	push {lr}
	ldr r0, =deferred_literal
	ldr r1, [r0]
	ldr r2, =255
	cmp r1, r2
	bgt 2f
	cmp r1, #0
	blt 1f
	push_tos
	movs tos, r1
	push_tos
	movs tos, #6
	bl _asm_sub_imm
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
1:	mvns r3, r1
	adds r3, #1
	cmp r3, r2
	bhi 2f
	push_tos
	movs tos, r3
	push_tos
	movs tos, #6
	bl _asm_add_imm
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
2:	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	bl _asm_literal
	push_tos
	ldr tos, =0x1A36
	bl _current_comma_2
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold *
	define_internal_word "fold*", visible_flag
_asm_fold_mul:
	push {lr}
	push_tos
	ldr r0, =deferred_literal
	ldr tos, [r0]
	push_tos
	movs tos, #0
	bl _asm_literal
	push_tos
	ldr tos, =0x4346
	bl _current_comma_2
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold AND
	define_internal_word "fold-and", visible_flag
_asm_fold_and:
	push {lr}
	ldr r0, =deferred_literal
	ldr r1, [r0]
	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	bl _asm_literal
	push_tos
	ldr tos, =0x4006
	bl _current_comma_2
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold OR
	define_internal_word "fold-or", visible_flag
_asm_fold_or:
	push {lr}
	ldr r0, =deferred_literal
	ldr r1, [r0]
	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	bl _asm_literal
	push_tos
	ldr tos, =0x4306
	bl _current_comma_2
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold XOR
	define_internal_word "fold-xor", visible_flag
_asm_fold_xor:
	push {lr}
	ldr r0, =deferred_literal
	ldr r1, [r0]
	push_tos
	movs tos, r1
	push_tos
	movs tos, #0
	bl _asm_literal
	push_tos
	ldr tos, =0x4046
	bl _current_comma_2
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold LSHIFT
	define_internal_word "fold-lshift", visible_flag
_asm_fold_lshift:
	push {lr}
	ldr r0, =deferred_literal
	ldr r1, [r0]
	ldr r2, =31
	cmp r1, r2
	bhi 1f
	push_tos
	ldr tos, =0x0036
	lsls r1, #6
	orrs tos, r1
	bl _current_comma_2
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
1:	push_tos
	movs tos, #0
	push_tos
	movs tos, #6
	bl _asm_mov_imm
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold RSHIFT
	define_internal_word "fold-rshift", visible_flag
_asm_fold_rshift:
	push {lr}
	ldr r0, =deferred_literal
	ldr r1, [r0]
	ldr r2, =31
	cmp r1, r2
	bhi 1f
	push_tos
	ldr tos, =0x0836
	lsls r1, #6
	orrs tos, r1
	bl _current_comma_2
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
1:	push_tos
	movs tos, #0
	push_tos
	movs tos, #6
	bl _asm_mov_imm
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	.ltorg
	
	@@ Constant fold ARSHIFT
	define_internal_word "fold-arshift", visible_flag
_asm_fold_arshift:
	push {lr}
	ldr r0, =deferred_literal
	ldr r1, [r0]
	ldr r2, =32
	cmp r1, r2
	blo 1f
	movs r1, #31
1:	push_tos
	ldr tos, =0x1036
	lsls r1, #6
	orrs tos, r1
	bl _current_comma_2
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold B!
	define_internal_word "fold-c!", visible_flag
_asm_fold_store_1:
	push {lr}
	push_tos
	ldr r0, =deferred_literal
	ldr tos, [r0]
	push_tos
	movs tos, #0
	bl _asm_literal
	push_tos
	ldr tos, =0x7006
	bl _current_comma_2
	push_tos
	movs tos, #6
	bl _asm_pull
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold H!
	define_internal_word "fold-h!", visible_flag
_asm_fold_store_2:
	push {lr}
	push_tos
	ldr r0, =deferred_literal
	ldr tos, [r0]
	push_tos
	movs tos, #0
	bl _asm_literal
	push_tos
	ldr tos, =0x8006
	bl _current_comma_2
	push_tos
	movs tos, #6
	bl _asm_pull
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold !
	define_internal_word "fold-!", visible_flag
_asm_fold_store_4:
	push {lr}
	push_tos
	ldr r0, =deferred_literal
	ldr tos, [r0]
	push_tos
	movs tos, #0
	bl _asm_literal
	push_tos
	ldr tos, =0x6006
	bl _current_comma_2
	push_tos
	movs tos, #6
	bl _asm_pull
	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
	end_inlined

	@@ Constant fold PICK
	define_internal_word "fold-pick", visible_flag
_asm_fold_pick:
	push {lr}
	ldr r0, =deferred_literal
	ldr r0, [r0]
	cmp r0, #0x1F
	bhi 1f
	push_tos
	movs tos, #6
	push {r0}
	bl _asm_push
	pop {r0}
	cmp r0, #0
	beq 2f
	push_tos
	lsls tos, r0, #2
	push_tos
	movs tos, #7
	push_tos
	movs tos, #6
	bl _asm_ldr_imm
2:	ldr r1, =literal_deferred_q
	movs r2, #0
	str r2, [r1]
	pop {pc}
1:	bl _asm_undefer_lit
	push_tos
	ldr tos, =_pick
	bl _asm_call
	pop {pc}
	end_inlined
	
	@@ Actually inline a word
	define_internal_word "inline,", visible_flag
_asm_inline:
	push {lr}
	push_tos
	bl _asm_extract_constant
	cmp tos, #0
	beq 1f
	pull_tos
	bl _asm_undefer_lit
	ldr r0, =-1
	ldr r1, =literal_deferred_q
	str r0, [r1]
	ldr r1, =deferred_literal
	str tos, [r1]
	adds dp, #4
	pull_tos
	pop {pc}	
1:	adds dp, #4
	pull_tos
	bl _asm_undefer_lit
	ldrh r0, [tos]
	ldr r1, =0xB500
	cmp r0, r1
	bne 1f
	adds tos, #2
1:	ldrh r0, [tos]
	ldr r1, =0xBD00
	cmp r0, r1
	beq 3f
	ldr r1, =0x4770
	cmp r0, r1
	beq 3f
2:	movs r1, tos
	push_tos
	movs tos, r0
	bl _current_comma_2
	adds tos, #2
	b 1b
3:	ldrh r2, [tos, #2]
	ldr r1, =0x003F
	cmp r2, r1
	bne 2b
	pull_tos
	pop {pc}
	end_inlined

	
	.ltorg

	@@ Call a word at an address
	define_internal_word "call,", visible_flag
_asm_call:	
	push {lr}
	bl _asm_undefer_lit
	ldr r0, =-1
	ldr r1, =suppress_inline
	str r0, [r1]
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
	end_inlined
	
	@@ Compile a bl instruction
	define_internal_word "bl,", visible_flag
_asm_bl:
	push {lr}
	subs tos, #4
	movs r0, tos
	lsrs tos, tos, #12
	ldr r1, =0x3FF
	ands tos, r1
	lsrs r1, r0, #24
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
	mvns r2, r2
	eors r2, r1
	ands r2, r3
	lsls r2, r2, #11
	orrs tos, r2
	lsrs r2, r0, #23
	ands r2, r3
	mvns r2, r2
	eors r2, r1
	ands r2, r3
	lsls r2, r2, #13
	orrs tos, r2
	ldr r2, =0xD000
	orrs tos, r2
	bl _current_comma_2
	pop {pc}
	end_inlined

	@@ Undefer a literal
	define_word "undefer-lit", visible_flag
_asm_undefer_lit:
	push {lr}
	ldr r1, =literal_deferred_q
	ldr r0, [r1]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, #6
	push {r1}
	bl _asm_push
	push_tos
	ldr r0, =deferred_literal
	ldr tos, [r0]
	push_tos
	movs tos, #6
	bl _asm_literal
	pop {r1}
	movs r0, #0
	str r0, [r1]
1:	pop {pc}
	end_inlined

	@@ Reserve space for a literal
	define_internal_word "reserve-literal", visible_flag
_asm_reserve_literal:
	push {lr}
	ldr r0, =suppress_inline
	ldr r1, =-1
	str r1, [r0]
	bl _current_reserve_2
	pop {pc}
	end_inlined

	@@ Store a literal ( x reg addr -- )
	define_internal_word "literal!", visible_flag
_asm_store_literal:	
	push {lr}
	bl _asm_register_const
	pop {pc}
	end_inlined
	
	@@ Assemble an unconditional branch
	define_internal_word "b,", visible_flag
_asm_b:	push {lr}
	ldr r0, =1023
	cmp tos, r0
	ble 1f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-1024
	cmp tos, r0
	bge 3f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
3:	bl _asm_b_16
	pop {pc}
	end_inlined

	@@ Assemble a branch on equal zero instruction
	define_internal_word "beq,", visible_flag
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
	bge 3f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
3:	bl _asm_beq_16
	pop {pc}
	end_inlined

	@@ Assemble a branch on not equal zero instruction
	define_internal_word "bne,", visible_flag
_asm_bne:
	push {lr}
	ldr r0, =127
	cmp tos, r0
	ble 1f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
1:	ldr r0, =-128
	cmp tos, r0
	bge 3f
	ldr tos, =_out_of_range_branch
	bl _raise
	pop {pc}
3:	bl _asm_bne_16
	pop {pc}
	end_inlined

	@@ Assemble an unconditional branch
	define_internal_word "b-back!", visible_flag
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
	end_inlined

	@@ Assemble a branch on equal zero instruction
	define_internal_word "beq-back!", visible_flag
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
	end_inlined

	.ltorg

	@@ Assemble a literal
	define_internal_word "literal,", visible_flag
_asm_literal:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, #255
	cmp tos, r1
	bgt 2f
	ldr r1, =-256
	cmp tos, r1
	blt 2f
	cmp tos, #0
	blt 3f
	push_tos
	movs tos, r0
	bl _asm_mov_imm
	pop {pc}
2:	ldr r1, =65535
	cmp tos, r1
	bgt 6f
	ldr r1, =-65536
	cmp tos, r1
	blt 6f
	cmp tos, #0
	blt 5f
	push_tos
	movs tos, r0
	bl _asm_literal_16
	pop {pc}
6:	push_tos
	movs tos, r0
	bl _asm_long_literal
	pop {pc}
3:	mvns tos, tos
	push_tos
	movs tos, r0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	bl _asm_mvn
	pop {pc}
5:	mvns tos, tos
	push_tos
	movs tos, r0
	push {r0}
	bl _asm_literal_16
	pop {r0}
	push_tos
	movs tos, r0
	push_tos
	movs tos, r0
	bl _asm_mvn
	pop {pc}
	end_inlined

	.ltorg
	
	@@ Assemble a 16-bit literal
	define_internal_word "literal-16,", visible_flag
_asm_literal_16:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	lsrs tos, r1, #8
	movs r3, #0xFF
	ands tos, r3
	push {r0, r1}
	push_tos
	movs tos, #3
	bl _asm_mov_imm
	push_tos
	movs tos, #8
	push_tos
	movs tos, #3
	push_tos
	bl _asm_lsl_imm
	pop {r0, r1}
	movs r3, #0xFF
	ands r1, r3
	push_tos
	movs tos, r1
	push_tos
	movs tos, r0
	push {r0}
	bl _asm_mov_imm
	pop {r0}
	push_tos
	movs tos, #3
	push_tos
	movs tos, r0
	bl _asm_orr
	pop {pc}
	end_inlined
	
	@@ Assemble a long literal
	define_internal_word "long-literal,", visible_flag
_asm_long_literal:
	push {lr}
	ldr r0, =suppress_inline
	ldr r1, =-1
	str r1, [r0]
	bl _current_reserve_2
	bl _asm_register_const
	pop {pc}
	end_inlined
	
	@@ Reserve space for a branch
	define_internal_word "reserve-branch", visible_flag
_asm_reserve_branch:
	push {lr}
	bl _current_reserve_2
	pop {pc}
	end_inlined

	@@ Out of range branch exception
	define_internal_word "x-out-of-range-branch", visible_flag
_out_of_range_branch:
	push {lr}
	string_ln "out of range branch"
	bl _type
	pop {pc}

	@@ Already building exception
	define_internal_word "x-already-building", visible_flag
_already_building:
	push {lr}
	string_ln "already building"
	bl _type
	pop {pc}
	end_inlined

	@@ Not building exception
	define_internal_word "x-not-building", visible_flag
_not_building:
	push {lr}
	string_ln "not building"
	bl _type
	pop {pc}
	end_inlined

	@@ Out of range literal
	define_internal_word "x-out-of-range-literal", visible_flag
_out_of_range_literal:
	push {lr}
	string_ln "out of range literal"
	bl _type
	pop {pc}
	
	@@ Assemble an unconditional branch
	define_internal_word "b-16,", visible_flag
_asm_b_16:
	push {lr}
	ldr r0, =0xE000
	ldr r1, =0x7FF
	ands tos, r1
	orrs tos, r0
	bl _current_comma_2
	pop {pc}
	end_inlined

	@@ Assemble a branch on equal zero instruction
	define_internal_word "beq-16,", visible_flag
_asm_beq_16:
	push {lr}
	ldr r0, =0xD000
	movs r1, #0xFF
	ands tos, r1
	orrs tos, r0
	bl _current_comma_2
	pop {pc}
	end_inlined

	@@ Assemble a branch on not equal zero instruction
	define_internal_word "bne-16,", visible_flag
_asm_bne_16:
	push {lr}
	ldr r0, =0xD100
	movs r1, #0xFF
	ands tos, r1
	orrs tos, r0
	bl _current_comma_2
	pop {pc}
	end_inlined

	@@ Assemble an unconditional branch
	define_internal_word "b-16-back!", visible_flag
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
	end_inlined

	@@ Assemble a branch on equal zero instruction
	define_internal_word "beq-16-back!", visible_flag
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
	end_inlined

	@@ Assemble a compare to immediate instruction
	define_internal_word "cmp-imm,", visible_flag
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
	end_inlined

	@@ Assemble a logical shift left immediate instruction
	define_internal_word "lsl-imm,", visible_flag
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
	end_inlined

	@@ Assemble an or instruction
	define_internal_word "orr,", visible_flag
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
	end_inlined

	.ltorg
	
	@@ Assemble an str immediate instruction
	define_internal_word "ldr-imm!", visible_flag
_asm_store_ldr_imm:
	push {lr}
	movs r2, tos
	pull_tos
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
	push_tos
	movs tos, r2
	bl _store_current_2
	pop {pc}
	end_inlined

	@@ Assemble an str immediate instruction
	define_internal_word "ldr-imm,", visible_flag
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
	end_inlined

	@@ Assemble an str immediate instruction
	define_internal_word "str-imm,", visible_flag
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
	end_inlined

	@@ Assemble an add register instruction
	define_internal_word "add,", visible_flag
_asm_add:
	push {lr}
	movs r0, #7
	ands tos, r0
	movs r1, tos
	pull_tos
	ands tos, r0
	lsls r2, tos, #3
	pull_tos
	ands tos, r0
	lsls tos, tos, #6
	orrs tos, r1
	orrs tos, r2
	ldr r0, =0x1800
	orrs tos, r0
	bl _current_comma_2
	pop {pc}
	end_inlined
	
	@@ Assemble an add immediate instruction
	define_internal_word "add-imm,", visible_flag
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
	end_inlined

	@@ Assemble a subtract immediate instruction
	define_internal_word "sub-imm,", visible_flag
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
	end_inlined

	@@ Assemble a MVNS instruction
	define_internal_word "mvn,", visible_flag
_asm_mvn:
	push {lr}
	movs r0, #7
	movs r1, tos
	ands r1, r0
	pull_tos
	ands tos, r0
	lsls tos, tos, #3
	orrs tos, r1
	ldr r1, =0x43C0
	orrs tos, r1
	bl _current_comma_2
	pop {pc}
	end_inlined

	@@ Assemble instructions to pull a value from the stack
	define_internal_word "pull,", visible_flag
_asm_pull:
@	push {lr}
@	movs r0, tos
@	movs tos, #0
@	push_tos
@	movs tos, #7
@	push_tos
@	movs tos, r0
@	bl _asm_ldr_imm
@	push_tos
@	movs tos, #4
@	push_tos
@	movs tos, #7
@	bl _asm_add_imm
@	pop {pc}
@	end_inlined
	
	push {lr}
	movs r0, #1
	lsls r0, r0, tos
	ldr r1, =7 << 8
	orrs r0, r1
	ldr r1, =0xC800
	orrs r0, r1
	movs tos, r0
	bl _current_comma_2
	pop {pc}
	end_inlined

	@@ Assemble instructions to push a value onto the stack
	define_internal_word "push,", visible_flag
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
	end_inlined

	@@ Word-align an address
	define_word "word-align,", visible_flag
_asm_word_align:
	push {lr}
	bl _current_here
	movs r0, tos
	pull_tos
	movs r1, #2
	tst r0, r1
	beq 1f
	push_tos
	movs tos, #0
	push_tos
	movs tos, #0
	push_tos
	movs tos, #0
	bl _asm_lsl_imm
1:	pop {pc}
	end_inlined

	@@ Assemble an instruction to generate a PC-relative address
	define_internal_word "adr,", visible_flag
_asm_adr:
	push {lr}
	bl _asm_word_align
	movs r1, #7
	ands r1, tos
	lsls r1, r1, #8
	pull_tos
	subs tos, #4
	lsrs tos, tos, #2
	ldr r0, =0xFF
	ands tos, r0
	orrs tos, r1
	ldr r0, =0xA000
	orrs tos, r0
	bl _current_comma_2
	pop {pc}
	end_inlined

	@@ Assemble an instruction at an address to generate a PC-relative
	@@ address
	define_internal_word "adr!", visible_flag
_asm_store_adr:
	push {lr}
	bl _asm_word_align
	movs r2, tos
	pull_tos
	movs r1, #7
	ands r1, tos
	lsls r1, r1, #8
	pull_tos
	subs tos, #4
	lsrs tos, tos, #2
	ldr r0, =0xFF
	ands tos, r0
	orrs tos, r1
	ldr r0, =0xA000
	orrs tos, r0
	push_tos
	movs tos, r2
	bl _store_current_2
	pop {pc}
	end_inlined

.ltorg

	@@ Assemble an instruction at an address to generate a PC-relative
	@@ address
	define_internal_word "adr-unaligned!", visible_flag
_asm_store_unaligned_adr:
	push {lr}
	movs r2, tos
	pull_tos
	movs r1, #7
	ands r1, tos
	lsls r1, r1, #8
	pull_tos
	subs tos, r2
	movs r3, #3
	subs tos, #1
	orrs tos, r3
	subs tos, #3
	lsrs tos, tos, #2
	ldr r0, =0xFF
	cmp tos, r0
	bhi 1f
	ands tos, r0
	orrs tos, r1
	ldr r0, =0xA000
	orrs tos, r0
	push_tos
	movs tos, r2
	bl _store_current_2
	pop {pc}
1:	push_tos
	ldr tos, =_out_of_range_literal
	bl _raise
	pop {pc}
	end_inlined

	@@ Assemble an instruction at an address to generate a PC-relative
	@@ address
	define_internal_word "ldr-pc!", visible_flag
_asm_store_ldr_pc:
	push {lr}
	movs r2, tos
	pull_tos
	movs r1, #7
	ands r1, tos
	lsls r1, r1, #8
	pull_tos
	subs tos, r2
	movs r3, #3
	subs tos, #1
	orrs tos, r3
	subs tos, #3
	lsrs tos, tos, #2
	ldr r0, =0xFF
	cmp tos, r0
	bhi 1f
	ands tos, r0
	orrs tos, r1
	ldr r0, =0x4800
	orrs tos, r0
	push_tos
	movs tos, r2
	bl _store_current_2
	pop {pc}
1:	push_tos
	ldr tos, =_out_of_range_literal
	bl _raise
	pop {pc}
	end_inlined

	@@ Write out constants
	define_internal_word "consts,", visible_flag
_asm_dump_consts:
	push {lr}
	ldr r0, =consts_used_count
	ldr r1, [r0]
	cmp r1, #0
	beq 2f
	push {r0}
	bl _asm_word_align
	pop {r0}
	movs r1, #0
1:	ldr r2, [r0]
	cmp r1, r2
	beq 2f
	push {r0, r1}
	bl _current_here
	pop {r0, r1}
	push_tos
	ldr r2, =const_buffer_reg
	ldrb tos, [r2, r1]
	push_tos
	ldr r2, =const_buffer_addr
	lsls r3, r1, #2
	ldr tos, [r2, r3]
	push {r0, r1, r3}
	bl _asm_store_ldr_pc
	pop {r0, r1, r3}
	push_tos
	ldr r2, =const_buffer_value
	ldr tos, [r2, r3]
	push {r0, r1}
	bl _current_comma_4
	pop {r0, r1}
	adds r1, #1
	b 1b
2:	movs r1, #0
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Write out constants to the code
	define_internal_word "consts-inline,", visible_flag
_asm_dump_consts_inline:
	push {lr}
	bl _current_reserve_2
	bl _asm_dump_consts
	movs r0, tos
	pull_tos
	push {r0}
	bl _current_here
	pop {r0}
	push_tos
	movs tos, r0
	bl _asm_branch_back
	pop {pc}
	end_inlined
	
	@@ Register a constant
	define_internal_word "register-const", visible_flag
_asm_register_const:
	push {lr}
	ldr r0, =consts_used_count
	ldr r1, [r0]
	lsls r2, r1, #2
	ldr r3, =const_buffer_addr
	str tos, [r3, r2]
	pull_tos
	ldr r3, =const_buffer_reg
	strb tos, [r3, r1]
	pull_tos
	ldr r3, =const_buffer_value
	str tos, [r3, r2]
	pull_tos
	adds r1, #1
	str r1, [r0]
	movs r3, #const_count
	cmp r1, r3
	bne 2f
	bl _asm_dump_consts_inline
2:	pop {pc}
	end_inlined

	@@ Set a constant to be set pointing to the end of a word ( r addr -- )
	@@ Note that only one such constant may be set for a given word.
	define_internal_word "set-const-end" visible_flag
_asm_set_const_end:
	ldr r0, =set_const_end
	str tos, [r0]
	pull_tos
	ldr r0, =set_const_end_reg
	str tos, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Set an end-of-word constant, both with regard to the ADR instruction
	@@ for the constant and the space alloted for its value.
	define_internal_word "resolve-const-end", visible_flag
_asm_resolve_const_end:
	push {lr}
	ldr r0, =set_const_end
	ldr r1, [r0]
	cmp r1, #0
	beq 1f
	push {r1}
	bl _current_here
	pop {r1}
	push_tos
	ldr r3, =set_const_end_reg
	ldr tos, [r3]
	push_tos
	movs tos, r1
	bl _asm_store_unaligned_adr
1:	pop {pc}
	end_inlined
	
	@@ Assemble a BX instruction
	define_internal_word "bx,", visible_flag
_asm_bx:
	push {lr}
	movs r0, 0xF
	ands tos, r0
	lsls tos, tos, #3
	ldr r0, =0x4700
	orrs tos, r0
	bl _current_comma_2
	pop {pc}
	end_inlined

	.ltorg
