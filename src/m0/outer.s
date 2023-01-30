@ Copyright (c) 2019-2023 Travis Bemann
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

	@@ Test whether a character is whitespace.
	define_word "ws?", visible_flag
_ws_q:	cmp tos, #0x20
	bls 1f
	movs tos, #0
	bx lr
1:	ldr tos, =-1
	bx lr
	end_inlined

	@@ Test whether a character is a newline.
	define_word "newline?", visible_flag
_newline_q:
	cmp tos, #0x0A
	beq 1f
	cmp tos, #0x0D
	beq 1f
	movs tos, #0
	bx lr
1:	ldr tos, =-1
	bx lr
	end_inlined

	@@ Parse the input buffer for the start of a token
	define_word "token-start", visible_flag
_token_start:
	push {lr}
	ldr r0, =eval_index_ptr
	ldr r0, [r0]
	ldr r1, [r0]
	push_tos
1:	ldr r0, =eval_count_ptr
	ldr r0, [r0]
	ldr r2, [r0]
	cmp r1, r2
	beq 2f
	ldr r0, =eval_ptr
	ldr r0, [r0]
	adds r0, r0, r1
	ldrb tos, [r0]
	push {r1}
	bl _ws_q
	pop {r1}
	cmp tos, #0
	beq 2f
	adds r1, #1
	b 1b
2:	movs tos, r1
	pop {pc}
	end_inlined

	@@ Parse the input buffer for the end of a token
	define_word "token-end", visible_flag
_token_end:
	push {lr}
	movs r1, tos
1:	ldr r0, =eval_count_ptr
	ldr r0, [r0]
	ldr r2, [r0]
	cmp r1, r2
	beq 2f
	ldr r0, =eval_ptr
	ldr r0, [r0]
	adds r0, r0, r1
	ldrb tos, [r0]
	push {r1}
	bl _ws_q
	pop {r1}
	cmp tos, #0
	bne 2f
	adds r1, #1
	b 1b
2:	movs tos, r1
	pop {pc}
	end_inlined

	@@ Parse a token
	define_word "token", visible_flag
_token:	push {lr}
	bl _token_start
	movs r0, tos
	push {r0}
	bl _token_end
	pop {r0}
	movs r1, tos
	ldr tos, =eval_ptr
	ldr tos, [tos]
	adds tos, tos, r0
	push_tos
	subs tos, r1, r0
	ldr r0, =eval_index_ptr
	ldr r0, [r0]
	str r1, [r0]
	pop {pc}
	end_inlined

	.ltorg
	
	@@ Parse a line comment
	define_word "\\", visible_flag | immediate_flag
_line_comment:
	push {lr}
	ldr r0, =eval_index_ptr
	ldr r0, [r0]
	ldr r0, [r0]
	ldr r1, =eval_count_ptr
	ldr r1, [r1]
	ldr r1, [r1]
	ldr r2, =eval_ptr
	ldr r2, [r2]
	push_tos
1:	cmp r0, r1
	beq 2f
	adds r3, r0, r2
	ldrb tos, [r3]
	push {r0, r1, r2}
	bl _newline_q
	pop {r0, r1, r2}
	cmp tos, #0
	bne 2f
	adds r0, #1
	b 1b
2:	pull_tos
	ldr r1, =eval_index_ptr
	ldr r1, [r1]
	str r0, [r1]
	pop {pc}
	end_inlined

	@@ Parse a paren coment
	define_word "(", visible_flag | immediate_flag
_paren_comment:
	push {lr}
	ldr r0, =eval_index_ptr
	ldr r0, [r0]
	ldr r0, [r0]
	ldr r1, =eval_count_ptr
	ldr r1, [r1]
	ldr r1, [r1]
	ldr r2, =eval_ptr
	ldr r2, [r2]
1:	cmp r0, r1
	beq 3f
	adds r3, r0, r2
	ldrb r3, [r3]
	cmp r3, #0x29
	beq 2f
	adds r0, #1
	b 1b
2:	adds r0, #1
3:	ldr r1, =eval_index_ptr
	ldr r1, [r1]
	str r0, [r1]
	pop {pc}
	end_inlined
	
	@@ Convert a character to being uppercase
	define_word "to-upper-char", visible_flag
_to_upper_char:
	cmp tos, #0x61
	blo 1f
	cmp tos, #0x7A
	bhi 1f
	subs tos, #0x20
1:	bx lr
	end_inlined

	@@ Compare whether two strings are equal
	define_word "equal-case-strings?", visible_flag
_equal_case_strings:
	movs r0, tos
        ldmia dp!, {r1, r2, r3}
	cmp r0, r2
	bne 3f
1:	cmp r0, #0
	beq 2f
	ldrb r2, [r1]
	adds r1, #1
        cmp r2, #0x61
        blo 4f
        cmp r2, #0x7A
        bhi 4f
        subs r2, #0x20
4:      ldrb tos, [r3]
	adds r3, #1
        cmp tos, #0x61
        blo 5f
        cmp tos, #0x7A
        bhi 5f
        subs tos, #0x20
5:      subs r0, #1
        cmp r2, tos
	beq 1b
3:      movs tos, #0
        bx lr
2:	ldr tos, =-1
        bx lr
	end_inlined

	@@ Find a word in a specific dictionary for a specific wordlist
	@@ ( addr bytes dict wid -- addr|0 )
	define_internal_word "find-dict", visible_flag
_find_dict:
	push {r4, r5, lr}
	movs r5, tos
	pull_tos
	movs r0, tos
	pull_tos
	movs r1, #visible_flag
	movs r2, tos
	pull_tos
	movs r3, tos
1:	cmp r0, #0
	beq 3f
	ldrh r4, [r0]
	tst r4, r1
	beq 2f
	ldrh r4, [r0, #2]
	cmp r4, r5
	bne 2f
	ldrb r4, [r0, #8]
	movs tos, r3
	push_tos
	movs tos, r2
	push_tos
	movs tos, r0
	adds tos, #9
	push_tos
	movs tos, r4
	push {r0, r1, r2, r3}
	bl _equal_case_strings
	pop {r0, r1, r2, r3}
	cmp tos, #0
	bne 4f
2:	ldr r0, [r0, #4]
	b 1b
3:	movs tos, #0
	pop {r4, r5, pc}
4:	movs tos, r0
	pop {r4, r5, pc}
	end_inlined

	.ltorg
	
	@@ Duplicate three items on the stack
	define_word "3dup", visible_flag
_3dup:	push_tos
	ldr tos, [dp, #8]
	push_tos
	ldr tos, [dp, #8]
	push_tos
	ldr tos, [dp, #8]
	bx lr
	end_inlined

	@@ Find a word in a specific wordlist
	@@ ( addr bytes wid -- addr|0 )
	define_internal_word "find-in-wordlist", visible_flag
_find_in_wordlist:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
3:	movs r0, tos
	pull_tos
	push {r0}
	bl _2dup
	pop {r0}
	push_tos
	ldr r1, =ram_latest
	ldr tos, [r1]
	push_tos
	movs tos, r0
	push {r0}
	bl _find_dict
	pop {r0}
	cmp tos, #0
	bne 2f
4:	ldr r1, =flash_latest
	ldr tos, [r1]
	push_tos
	movs tos, r0
	bl _find_dict
	pop {pc}
1:	ldr r0, =state
	ldr r0, [r0]
	cmp r0, #0
	beq 3b
	movs r0, tos
	b 4b
2:	adds dp, #8
	pop {pc}
	end_inlined
	
	@@ Find a word in the dictionary according to the word order list
	@@ ( addr bytes -- addr|0 )
	define_word "do-find", visible_flag
_do_find:
	push {lr}
	ldr r0, =order_count
	ldr r0, [r0]
	ldr r1, =order
1:	cmp r0, #0
	beq 2f
	push {r0, r1}
	bl _2dup
	pop {r0, r1}
	push_tos
	ldrh tos, [r1]
	push {r0, r1}
	bl _find_in_wordlist
	pop {r0, r1}
	cmp tos, #0
	bne 3f
	subs r0, #1
	adds r1, #2
	pull_tos
	b 1b
2:	adds dp, #4
	movs tos, #0
	pop {pc}
3:	adds dp, #8
	pop {pc}
	end_inlined

	@@ Invoke the find hook
	@@ ( b-addr bytes -- addr|0 )
	define_word "find", visible_flag
_find:	ldr r0, =find_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	mov pc, r0
1:	push_tos
	ldr tos, =_hook_needed
	bl _raise
	bx lr
	end_inlined

	@@ Invoke the find raw hook
	@@ ( b-addr bytes -- addr|0 )
	define_word "find-raw", visible_flag
_find_raw:
	ldr r0, =find_raw_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	mov pc, r0
1:	push_tos
	ldr tos, =_hook_needed
	bl _raise
	bx lr
	end_inlined

	@@ Hook needed exception handler
	define_word "x-hook-needed", visible_flag
_hook_needed:
	push {lr}
	string_ln "hook needed"
	bl _type
	pop {pc}
	
	@@ Find a word in a specific dictionary in any wordlist in order of
	@@ definition
	@@ ( addr bytes dict -- addr|0 )
	define_word "find-all-dict", visible_flag
_find_all_dict:
	push {r4, lr}
	movs r0, tos
	pull_tos
	movs r1, #visible_flag
	movs r2, tos
	pull_tos
	movs r3, tos
1:	cmp r0, #0
	beq 3f
	ldrh r4, [r0]
	tst r4, r1
	beq 2f
	ldrb r4, [r0, #8]
	movs tos, r3
	push_tos
	movs tos, r2
	push_tos
	movs tos, r0
	adds tos, #9
	push_tos
	movs tos, r4
	push {r0, r1, r2, r3}
	bl _equal_case_strings
	pop {r0, r1, r2, r3}
	cmp tos, #0
	bne 4f
2:	ldr r0, [r0, #4]
	b 1b
3:	movs tos, #0
	pop {r4, pc}
4:	movs tos, r0
	pop {r4, pc}
	end_inlined

	.ltorg
	
	@@ Find a word in the dictionary in any wordlist in order of definition
	@@ ( addr bytes -- addr|0 )
	define_word "find-all", visible_flag
_find_all:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
3:	movs r1, tos
	pull_tos
	movs r2, tos
	push_tos
	movs tos, r1
	push_tos
	ldr r3, =ram_latest
	ldr tos, [r3]
	push {r1, r2}
	bl _find_all_dict
	pop {r1, r2}
	cmp tos, #0
	bne 2f
	movs tos, r2
	push_tos
	movs tos, r1
	push_tos
	ldr r3, =flash_latest
	ldr tos, [r3]
	bl _find_all_dict
	pop {pc}
1:	ldr r0, =state
	ldr r0, [r0]
	cmp r0, #0
	beq 3b
	push_tos
	ldr r0, =flash_latest
	ldr tos, [r0]
	bl _find_all_dict
2:	pop {pc}
	end_inlined

	@@ Get an xt from a word
	define_word ">xt", visible_flag
_to_xt:	push {lr}
        push_tos
        adds tos, #8
        bl _get_flash_buffer_value_1
        movs r0, tos
        pull_tos
	adds tos, #9
	adds tos, tos, r0
	movs r0, #1
	ands r0, tos
	bne 1f
	pop {pc}
1:	adds tos, #1
	pop {pc}
	end_inlined

	@@ Abort
	define_word "abort", visible_flag
_abort:	bl _stack_base
	ldr dp, [tos]
        ldr r0, =word_reset_hook
        ldr r0, [r0]
        cmp r0, #0
        beq 1f
        movs r1, #1
        orrs r0, r1
        blx r0
1:      bl _bel
	bl _nak
	b _quit
	bx lr
	end_inlined

        @@ Prepare the prompt
        define_internal_word "prepare-prompt" visible_flag
_prepare_prompt:
	movs r1, #0
        ldr r0, =prompt_disabled
        str r1, [r0]
        ldr r0, =eval_data
        str r1, [r0]
        ldr r1, =_quit_refill
        ldr r0, =eval_refill
        str r1, [r0]
        ldr r1, =_quit_eof
        ldr r0, =eval_eof
        str r1, [r0]
	ldr r0, =eval_index_ptr
	ldr r1, =input_buffer_index
	str r1, [r0]
	ldr r0, =eval_count_ptr
	ldr r1, =input_buffer_count
	str r1, [r0]
	ldr r0, =eval_ptr
	ldr r1, =input_buffer
	str r1, [r0]
        bx lr
        end_inlined
        
        @@ QUIT refill word
        define_internal_word "quit-refill", visible_flag
_quit_refill:
        push {lr}
        ldr r0, =refill_hook
        push_tos
        ldr tos, [r0]
        bl _execute_nz
        pop {pc}
        end_inlined

        @@ QUIT EOF word
        define_internal_word "quit-eof?", visible_flag
_quit_eof:
        push_tos
        movs tos, #0
        bx lr
        end_inlined

	@@ QUIT while resetting the state
	define_word "quit-reset", visible_flag
_quit_reset:    
        bl _stack_base
	ldr dp, [tos]
        ldr r0, =word_reset_hook
        ldr r0, [r0]
        cmp r0, #0
        beq 1f
        movs r1, #1
        orrs r0, r1
        blx r0
1:      b _quit
	bx lr
	end_inlined

	@@ The outer loop of Forth
	define_word "quit", visible_flag
_quit:	bl _rstack_base
	ldr tos, [tos]
	mov sp, tos
        bl _prepare_prompt
	ldr tos, =_main
	bl _try
	bl _display_red
	bl _execute_nz
	bl _display_normal
	b _abort
	bx lr
	end_inlined

	.ltorg
	
	@@ Display red text
	define_word "display-red", visible_flag
_display_red:
	push {lr}
        ldr r0, =color_enabled
        ldr r0, [r0]
        cmp r0, #0
        beq 1f
	string "\x1B[31;1m"
	bl _type
1:      pop {pc}
        end_inlined

	@@ Display normal text
	define_word "display-normal", visible_flag
_display_normal:
	push {lr}
        ldr r0, =color_enabled
        ldr r0, [r0]
        cmp r0, #0
        beq 1f
	string "\x1B[0m"
	bl _type
1:      pop {pc}
        end_inlined

	.ltorg
	
	@@ The main functionality, within the main exception handler
	define_internal_word "main", visible_flag
_main:	push {lr}
	bl _flush_all_flash
	ldr r0, =state
	movs r1, #0
	str r1, [r0]
	bl _refill
        bl _outer
        pop {pc}

        @@ The main loop of the outer interpreter
        define_word "outer", visible_flag
_outer: push {lr}
1:	bl _display_entry_space
	bl _interpret_line
        bl _display_prompt
        ldr r0, =eval_eof
        push_tos
        ldr tos, [r0]
        bl _execute
        cmp tos, #0
        bne 2f
        pull_tos
	bl _refill
	b 1b
2:      pull_tos
        pop {pc}

        @@ Display the space after enry
        define_word "display-entry-space", visible_flag
_display_entry_space:   
        push {lr}
        ldr r0, =prompt_disabled
        ldr r0, [r0]
        cmp r0, #0
        bgt 1f
        bl _space
1:      pop {pc}
        
        @@ Display the prompt
        define_word "display-prompt", visible_flag
_display_prompt:
        push {lr}
        ldr r0, =prompt_disabled
        ldr r0, [r0]
        cmp r0, #0
        bgt 1f
	ldr r0, =prompt_hook
	push_tos
	ldr tos, [r0]
	bl _execute_nz
1:      pop {pc}

	@@ Interpret a line of input
	define_internal_word "interpret-line", visible_flag
_interpret_line:
        push {lr}
1:	bl _validate
	bl _token
	cmp tos, #0
	beq 2f
	movs r0, tos
	ldr r1, [dp]
	push {r0, r1}
        ldr r2, =parse_hook
        ldr r2, [r2]
        cmp r2, #0
        beq 5f
        movs r3, #1
        orrs r2, r3
        blx r2
        cmp tos, #0
        beq 3f
        pull_tos
        pop {r0, r1}
        b 1b
3:      subs dp, #4
        ldr r1, [sp, #4]
        str r1, [dp, #0]
        ldr tos, [sp, #0]
5:	bl _find
	pop {r0, r1}
	cmp tos, #0
	beq 3f
	ldr r0, =state
	ldr r0, [r0]
	cmp r0, #0
	bne 4f
	ldr r0, [tos]
	movs r1, #compiled_flag
	tst r0, r1
	bne 5f
6:	bl _to_xt
	bl _execute
	b 1b
3:	movs tos, r1
	push_tos
	movs tos, r0
	bl _parse_literal
	b 1b
4:	ldr r0, [tos]
	movs r1, #immediate_flag
	tst r0, r1
	bne 6b
	movs r1, #inlined_flag
	tst r0, r1
	bne 7f
	movs r1, #fold_flag
	tst r0, r1
	bne 8f
	bl _to_xt
	bl _asm_call
	b 1b
5:	push_tos
	ldr tos, =_not_compiling
	bl _raise
	b 1b
7:	bl _to_xt
	bl _asm_inline
	b 1b
8:	bl _to_xt
	bl _asm_fold
	b 1b
2:	pull_tos
	pull_tos
	pop {pc}
	end_inlined
	
	@@ Validate the current state
	define_internal_word "validate", visible_flag
_validate:
	push {lr}
	bl _stack_base
	ldr r0, [tos]
	pull_tos
	cmp dp, r0
	ble 1f
	push_tos
	ldr tos, =_stack_underflow
	bl _raise
1:	bl _stack_end
	ldr r0, [tos]
	pull_tos
	cmp dp, r0
	bge 1f
	push_tos
	ldr tos, =_stack_overflow
	bl _raise
1:	mov r1, sp
	push {r1}
	bl _rstack_base
	pop {r1}
	ldr r0, [tos]
	pull_tos
	cmp r1, r0
	ble 1f
	push_tos
	ldr tos, =_rstack_underflow
	bl _raise
1:	push {r1}
	bl _rstack_end
	pop {r1}
	ldr r0, [tos]
	pull_tos
	cmp r1, r0
	bge 1f
	push_tos
	ldr tos, =_rstack_overflow
	bl _raise
1:	ldr r0, =validate_dict_hook
	push_tos
	ldr tos, [r0]
	bl _execute_nz
	pop {pc}
	end_inlined

	.ltorg
	
	@@ Stack overflow exception
	define_word "stack-overflow", visible_flag
_stack_overflow:
	push {lr}
	string_ln "stack overflow"
	bl _type
	pop {pc}
	end_inlined

	@@ Stack underflow exception
	define_word "stack-underflow", visible_flag
_stack_underflow:
	push {lr}
	string_ln "stack underflow"
	bl _type
	pop {pc}
	end_inlined

	@@ Return stack overflow exception
	define_word "rstack-overflow", visible_flag
_rstack_overflow:
	push {lr}
	string_ln "return stack overflow"
	bl _type
	pop {pc}
	end_inlined

	@@ Return stack underflow exception
	define_word "rstack-underflow", visible_flag
_rstack_underflow:
	push {lr}
	string_ln "return stack underflow"
	bl _type
	pop {pc}
	end_inlined

	@@ Display a prompt
	define_internal_word "do-prompt", visible_flag
_do_prompt:
	push {lr}
	string_ln " ok"
	bl _type
	pop {pc}
	end_inlined

	@@ Parse a literal word
	define_internal_word "parse-literal", visible_flag
_parse_literal:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
	ldr r2, =handle_number_hook
	ldr r2, [r2]
	cmp r2, #0
	beq 1f
	push {r0, r1}
	push_tos
	movs tos, r1
	push_tos
	movs tos, r0
	push_tos
	movs tos, r2
	bl _execute
	pop {r0, r1}
	cmp tos, #0
	beq 1f
	pull_tos
	pop {pc}
1:	ldr r2, =failed_parse_hook
	ldr r2, [r2]
	cmp r2, #0
	beq 2f
	push_tos
	movs tos, r1
	push_tos
	movs tos, r0
	push_tos
	movs tos, r2
	bl _execute
2:	pop {r0}
	b _abort

	@@ Refill the input buffer
	define_word "refill", visible_flag
_refill:
	push {lr}
	ldr r0, =eval_refill
        push_tos
        ldr tos, [r0]
        bl _execute_nz
	pop {pc}
	end_inlined

	.ltorg
	
	@@ Send XON
	define_word "xon", visible_flag
_xon:	push {lr}
	ldr r0, =xon_xoff_enabled
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, #0x11
	bl _emit
1:	pop {pc}
	end_inlined

	@@ Send XOFF
	define_word "xoff", visible_flag
_xoff:	push {lr}
	ldr r0, =xon_xoff_enabled
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, #0x13
	bl _emit
1:	pop {pc}
	end_inlined

	@@ Send ACK
	define_word "ack", visible_flag
_ack:	push {lr}
	ldr r0, =ack_nak_enabled
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, #0x06
	bl _emit
1:	pop {pc}
	end_inlined

	@@ Send NAK
	define_word "nak", visible_flag
_nak:	push {lr}
	ldr r0, =ack_nak_enabled
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, #0x15
	bl _emit
1:	pop {pc}
	end_inlined

	@@ Send BEL
	define_word "bel", visible_flag
_bel:	push {lr}
	ldr r0, =bel_enabled
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, #0x07
	bl _emit
1:	pop {pc}
	end_inlined

	@@ Implement the refill hook
	define_internal_word "do-refill", visible_flag
_do_refill:
	push {lr}
	bl _xon
	bl _ack
	movs r0, #0
	ldr r1, =input_buffer_size
	ldr r2, =input_buffer
	adds r0, r2
	adds r1, r2
1:	cmp r0, r1
	beq 2f
6:	push {r0, r1}
	bl _key
	pop {r0, r1}
	cmp tos, #0x0D
	beq 3f
	cmp tos, #0x0A
	beq 3f
	cmp tos, #0x7F
	beq 4f
	cmp tos, #0x09
	beq 8f
	cmp tos, #0x20
	blo 7f
8:	strb tos, [r0]
	adds r0, #1
	movs r2, tos
	push {r0, r1, r2}
	bl _emit
	pop {r0, r1, r2}
	b 1b
7:	pull_tos
	b 6b
4:	ldr r2, =input_buffer
	cmp r0, r2
	beq 1b
	push {r0, r1}
	movs tos, #0x08
	bl _emit
	push_tos
	movs tos, #0x20
	bl _emit
	push_tos
	movs tos, #0x08
	bl _emit
	pop {r0, r1}
4:	ldr r2, =input_buffer
	cmp r0, r2
	beq 1b
	subs r0, #1
	ldrb r2, [r0]
	movs r3, #0x80
	tst r2, r3
	beq 1b
	movs r3, r0
	subs r3, #1
	ldr r2, =input_buffer
	cmp r3, r2
	beq 1b
	ldrb r2, [r3]
	movs r3, #0x80
	tst r2, r3
	bne 4b
	b 1b
3:	pull_tos
2:	ldr r2, =input_buffer
	subs r0, r2
	ldr r2, =input_buffer_count
	str r0, [r2]
	movs r0, #0
	ldr r2, =input_buffer_index
	str r0, [r2]
	bl _xoff
	pop {pc}
	end_inlined
	
	@@ Implement the failed parse hook
	define_internal_word "do-failed-parse", visible_flag
_do_failed_parse:
	push {lr}
	bl _display_red
	string "unable to parse: "
	bl _type
	bl _type
	string_ln ""
	bl _type
	bl _display_normal
	push_tos
	ldr tos, =_failed_parse
	bl _raise
	pop {pc}
	end_inlined

	@@ Failed parse exception
	define_word "x-failed-parse", visible_flag
_failed_parse:
	push {lr}
	pop {pc}
	end_inlined
	
	@@ Implement the handle number hook
	define_internal_word "do-handle-number", visible_flag
_do_handle_number:
	push {lr}
	bl _parse_integer
	cmp tos, #0
	beq 2f
	pull_tos
	ldr r0, =state
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	bl _comma_lit
1:	push_tos
	ldr tos, =-1
	pop {pc}
2:	pull_tos
	movs tos, #0
	pop {pc}
	end_inlined

	@@ Parse an integer ( addr bytes -- n success )
	define_word "parse-integer", visible_flag
_parse_integer:
	push {lr}
	bl _parse_base
	bl _parse_integer_core
	pop {pc}
	end_inlined

	@@ Parse an unsigned integer ( addr bytes -- u success )
	define_word "parse-unsigned", visible_flag
_parse_unsigned:
	push {lr}
	bl _parse_base
	ldr r0, [dp]
	cmp r0, #0
	beq 1f
	bl _parse_unsigned_core
	pop {pc}
1:	pull_tos
	movs tos, #0
	str tos, [dp]
	pop {pc}
	end_inlined

	.ltorg

	@@ Actually parse an integer base ( addr bytes -- addr bytes base )
	define_word "parse-base", visible_flag
_parse_base:
	push {lr}
	cmp tos, #0
	beq 5f
	movs r0, tos
	pull_tos
	ldrb r1, [tos]
	cmp r1, #0x24
	bne 1f
	movs r1, #16
	b 6f
1:	cmp r1, #0x23
	bne 2f
	movs r1, #10
	b 6f
2:	cmp r1, #0x2F
	bne 3f
	movs r1, #8
	b 6f
3:	cmp r1, #0x25
	bne 4f
	movs r1, #2
	b 6f
4:	push_tos
	movs tos, r0
5:	bl _base
	ldr tos, [tos]
	pop {pc}
6:	adds tos, #1
	push_tos
	subs r0, #1
	movs tos, r0
	push_tos
	movs tos, r1
	pop {pc}
	end_inlined

	@@ Actually parse an integer ( addr bytes base -- n success )
	define_internal_word "parse-integer-core", visible_flag
_parse_integer_core:
	push {lr}
	movs r2, tos
	pull_tos
	cmp tos, #0
	beq 3f
	movs r0, tos
	pull_tos
	ldrb r1, [tos]
	cmp r1, #0x2D
	beq 1f
	push_tos
	movs tos, r0
	push_tos
	movs tos, r2
	bl _parse_unsigned_core
	pop {pc}
1:	adds tos, #1
	push_tos
	movs tos, r0
	subs tos, #1
	push_tos
	movs tos, r2
	bl _parse_unsigned_core
	cmp tos, #0
	beq 2f
	pull_tos
	rsbs tos, tos, #0
	push_tos
	ldr tos, =-1
	pop {pc}
3:	pull_tos
	movs tos, #0
	push_tos
	movs tos, #0
	pop {pc}
	end_inlined
	
	@@ Actually parse an unsigned integer ( addr bytes base  -- u success )
	define_internal_word "parse-unsigned-core", visible_flag
_parse_unsigned_core:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
	movs r2, tos
	pull_tos
	movs r3, #0
	cmp r0, #0
	bgt 1f
	movs tos, #0
	push_tos
	movs tos, #0
	pop {pc}
1:	cmp r0, #36
	ble 1f
	movs tos, #0
	push_tos
	movs tos, #0
	pop {pc}
1:	cmp r1, #0
	beq 3f
	push_tos
	ldrb tos, [r2]
	subs r1, #1
	adds r2, #1
	muls r3, r0, r3
	push_tos
	movs tos, r0
	push {r0, r1, r2, r3}
	bl _parse_digit
	pop {r0, r1, r2, r3}
	cmp tos, #0
	beq 2f
	pull_tos
	adds r3, r3, tos
	pull_tos
	b 1b
2:	pull_tos
	movs tos, #0
	push_tos
	movs tos, #0
	pop {pc}
3:	push_tos
	movs tos, r3
	push_tos
	ldr tos, =-1
	pop {pc}
	end_inlined

	.ltorg
	
	@@ Parse a digit ( c base -- digit success )
	define_word "parse-digit", visible_flag
_parse_digit:
	push {lr}
	movs r0, tos
	pull_tos
	cmp tos, #0x30
	blt 1f
	cmp tos, #0x39
	bgt 2f
	subs tos, #0x30
	b 3f
1:	movs tos, #0
	push_tos
	movs tos, #0
	pop {pc}
2:	push {r0}
	bl _to_upper_char
	pop {r0}
	cmp tos, #0x41
	blt 1b
	cmp tos, #0x5A
	bgt 1b
	subs tos, #0x37
3:	cmp tos, r0
	bge 1b
	push_tos
	ldr tos, =-1
	pop {pc}
	end_inlined
	
	@@ Start a colon definition
	define_word ":", visible_flag
_colon:	push {lr}
	bl _token
	cmp tos, #0
	beq 1f
	ldr r0, =state
	ldr r1, =-1
	str r1, [r0]
	bl _asm_start
	ldr r0, =current_flags
	movs r1, #visible_flag
	str r1, [r0]
	pop {pc}
1:	push_tos
	ldr tos, =_token_expected
	bl _raise
	pop {pc}
	end_inlined

	@@ Start an anonymous colon definition
	define_word ":noname", visible_flag
_colon_noname:
	push {lr}
	push_tos
	movs tos, #0
	push_tos
	movs tos, #0
	ldr r0, =state
	ldr r1, =-1
	str r1, [r0]
	bl _asm_start
	ldr r0, =current_flags
	movs r1, #0
	str r1, [r0]
	push_tos
	ldr tos, =current_compile
	ldr tos, [tos]
	bl _to_xt
	pop {pc}
	end_inlined

	@@ End a colon definition
	define_word ";", visible_flag | immediate_flag
_semi:	push {lr}
	ldr r0, =state
	ldr r1, [r0]
	cmp r1, #0
	beq 1f
	movs r1, #0
	str r1, [r0]
	bl _asm_end
	pop {pc}
1:	push_tos
	ldr tos, =_not_compiling
	bl _raise
	pop {pc}
	end_inlined

	@@ Create a constant
	define_word "constant", visible_flag
_constant_4:
	push {lr}
	bl _token
	cmp tos, #0
	beq 3f
	bl _asm_start
	push_tos
	movs tos, #6
	bl _asm_push
	ldr r0, =current_flags
	movs r1, #visible_flag | inlined_flag
	str r1, [r0]
	push_tos
	movs tos, #6
	ldr r0, =suppress_suppress_inline
	ldr r1, =-1
	str r1, [r0]
	bl _asm_literal
	ldr r0, =suppress_suppress_inline
	movs r1, #0
	str r1, [r0]
	bl _asm_end
	pop {pc}
3:	push_tos
	ldr tos, =_token_expected
	bl _raise
	pop {pc}
	end_inlined

	.ltorg
	
	@@ Create a constant with a specified name as a string
	define_internal_word "constant-with-name", visible_flag
_constant_with_name_4:
	push {lr}
	bl _asm_start
	push_tos
	movs tos, #6
	bl _asm_push
	ldr r0, =current_flags
	movs r1, #visible_flag | inlined_flag
	str r1, [r0]
	push_tos
	movs tos, #6
	ldr r0, =suppress_suppress_inline
	ldr r1, =-1
	str r1, [r0]
	bl _asm_literal
	ldr r0, =suppress_suppress_inline
	movs r1, #0
	str r1, [r0]
	bl _asm_end
	pop {pc}
	end_inlined

	@@ Create a 2-word constant
	define_word "2constant", visible_flag
_constant_8:
	push {lr}
	bl _token
	cmp tos, #0
	beq 1f
	bl _asm_start
	ldr r0, =current_flags
	movs r1, #visible_flag
	str r1, [r0]
	push_tos
	movs tos, #6
	bl _asm_push
	movs r0, tos
	movs tos, #6
	push {r0}
	bl _asm_literal
	push_tos
	movs tos, #6
	bl _asm_push
	pop {r0}
	push_tos
	movs tos, r0
	push_tos
	movs tos, #6
	bl _asm_literal
	bl _asm_end
	pop {pc}
1:	push_tos
	ldr tos, =_token_expected
	bl _raise
	pop {pc}
	end_inlined

	@@ Create a 2-word constant with a name specified as a string
	define_internal_word "2constant-with-name", visible_flag
_constant_with_name_8:
	push {lr}
	bl _token
	cmp tos, #0
	beq 1f
	bl _asm_start
	ldr r0, =current_flags
	movs r1, #visible_flag
	str r1, [r0]
	push_tos
	movs tos, #6
	bl _asm_push
	push_tos
	movs tos, #6
	bl _asm_literal
	push_tos
	movs tos, #6
	bl _asm_literal
	bl _asm_end
	pop {pc}
1:	push_tos
	ldr tos, =_token_expected
	bl _raise
	pop {pc}
	end_inlined

	@@ Token expected exception handler
	define_word "x-token-expected", visible_flag
_token_expected:
	push {lr}
	string_ln "token expected"
	bl _type
	pop {pc}
	end_inlined

	@@ We are not currently compiling
	define_word "x-not-compiling", visible_flag
_not_compiling:
	push {lr}
	string_ln "not compiling"
	bl _type
	pop {pc}
	end_inlined

	@@ We are currently compiling to flash
	define_word "x-compile-to-ram-only", visible_flag
_compile_to_ram_only:
	push {lr}
	string_ln "compile to ram only"
	bl _type
	pop {pc}
	end_inlined

	.ltorg
	
