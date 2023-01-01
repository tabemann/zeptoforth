@ Copyright (c) 2020-2023 Travis Bemann
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

        @@ Start a block
        define_internal_word "begin-block", visible_flag
_begin_block:
        push {lr}
        ldr r0, =block_begin_hook
        ldr r0, [r0]
        cmp r0, #0
        beq 1f
        movs r1, #1
        orrs r0, r1
        blx r0
1:      pop {pc}
        end_inlined
        
        @@ End a block
        define_internal_word "end-block", visible_flag
_end_block:
        push {lr}
        ldr r0, =block_exit_hook
        ldr r0, [r0]
        cmp r0, #0
        beq 1f
        movs r1, #1
        orrs r0, r1
        blx r0
1:	ldr r0, =block_end_hook
        ldr r0, [r0]
        cmp r0, #0
        beq 2f
        movs r1, #1
        orrs r0, r1
        blx r0
2:      pop {pc}
        end_inlined
        
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
        bl _begin_block
	push_tos
	ldr tos, =-1
	pop {pc}
	end_inlined

	@@ ELSE in an IF ELSE THEN block
	define_word "else", visible_flag | immediate_flag | compiled_flag
_else:	push {lr}
	bl _asm_undefer_lit
	cmp tos, #0
	beq 1f
	pull_tos
        bl _end_block
	movs r0, tos
	pull_tos
	push {r0}
	bl _asm_reserve_branch
	bl _current_here
	pop {r0}
	push_tos
	movs tos, r0
	bl _asm_branch_zero_back
        bl _begin_block
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
	string "not following if"
	bl _type
	pop {pc}
	end_inlined
	
	@@ End an IF block
	define_word "then", visible_flag | immediate_flag | compiled_flag
_then:	push {lr}
	bl _asm_undefer_lit
        bl _end_block
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

.ltorg
        
        @@ End an IF block without ending a block
	define_internal_word "then-no-block", visible_flag | immediate_flag | compiled_flag
_then_no_block:
        push {lr}
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
        bl _begin_block
	pop {pc}

	@@ Start a WHILE block
	define_word "while", visible_flag | immediate_flag | compiled_flag
_while:	push {lr}
	bl _asm_undefer_lit
        bl _end_block
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
        bl _begin_block
	pop {pc}
	end_inlined

	@@ End a BEGIN-WHILE-REPEAT block
	define_word "repeat", visible_flag | immediate_flag | compiled_flag
_repeat:
	push {lr}
	bl _asm_undefer_lit
        bl _end_block
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
        bl _end_block
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
        bl _end_block
	bl _asm_branch
	pop {pc}
	end_inlined

	@@ Implement the core of DO
	define_internal_word "(do)", visible_flag
_xdo:
	.ifdef cortex_m7
	ldr r0, [dp], #4
	ldr r1, [dp], #4
	ldr r2, [dp], #4
	.else
	ldmia dp!, {r0, r1, r2}
	.endif
	
	push {tos}
	push {r0}
	push {r1}
	movs tos, r2
	bx lr
	end_inlined

	@@ Implement the core of ?DO
	define_internal_word "(?do)", visible_flag
_xqdo:
	.ifdef cortex_m7
	ldr r0, [dp], #4
	ldr r1, [dp], #4
	ldr r2, [dp], #4
	.else
	ldmia dp!, {r0, r1, r2}
	.endif
	
	cmp r0, r1
	beq 1f
	push {tos}
	push {r0}
	push {r1}
	movs tos, r2
	bx lr
1:	movs r0, tos
	movs tos, r2
	bx r0
	end_inlined

	@@ Implement the core of LOOP
	define_internal_word "(loop)", visible_flag
_xloop:	ldr r0, [sp, #4]
	ldr r1, [sp, #0]
	adds r0, #1
	cmp r0, r1
	beq 1f
	str r0, [sp, #4]
	movs r0, tos
	pull_tos
	bx r0
1:	pull_tos
	mov r0, sp
	adds r0, #12
	mov sp, r0
	bx lr
	end_inlined

	@@ Implement the core of +LOOP
	define_internal_word "(+loop)", visible_flag
_xploop:
	ldr r0, [sp, #4]
	ldr r1, [sp, #0]
	ldr r2, [dp, #0]
	cmp r2, #0
	blt 1f
	adds r0, r2
	cmp r0, r1
	bge 2f
	str r0, [sp, #4]
	movs r0, tos
	adds dp, #4
	pull_tos
	bx r0
1:	adds r0, r2
	cmp r0, r1
	blt 2f
	str r0, [sp, #4]
	movs r0, tos
	adds dp, #4
	pull_tos
	bx r0
2:	adds dp, #4
	pull_tos
	mov r0, sp
	adds r0, #12
	mov sp, r0
	bx lr
	end_inlined

	@@ Get the outermost loop index
	define_word "i", visible_flag | inlined_flag
_i:	push_tos
	ldr tos, [sp, #4]
	bx lr
	end_inlined

	@@ Get the first inner loop index
	define_word "j", visible_flag | inlined_flag
_j:	push_tos
	ldr tos, [sp, #16]
	bx lr
	end_inlined

	@@ Leave a do loop
	define_word "leave", visible_flag
_leave:	mov r0, sp
	adds r0, #8
	mov sp, r0
	pop {pc}
	end_inlined

	@@ Unloop a do loop
	define_word "unloop", visible_flag | inlined_flag
_unloop:
	mov r0, sp
	adds r0, #12
	mov sp, r0
	bx lr
	end_inlined
	
	.ltorg
	
