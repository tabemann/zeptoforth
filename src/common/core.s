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

	@@ Drop the top of the data stack
	define_word "drop", visible_flag | inlined_flag
_drop:	pull_tos
	bx lr
	end_inlined

	@@ Duplicate the top of the data stack
	define_word "dup", visible_flag | inlined_flag
_dup:	push_tos
	bx lr
	end_inlined

	@@ Swap the top two places on the data stack
	define_word "swap", visible_flag | inlined_flag
_swap:	movs r0, tos
	ldr tos, [dp]
	str r0, [dp]
	bx lr
	end_inlined

	@@ Copy the second place on the data stack onto the top of the stack,
	@@ pushing the top of the data stack to the second place
	define_word "over", visible_flag | inlined_flag
_over:	push_tos
	ldr tos, [dp, #4]
	bx lr
	end_inlined

	@@ Rotate the top three places on the data stack, so the third place
	@@ moves to the first place
	define_word "rot", visible_flag | inlined_flag
_rot:	ldr r0, [dp, #4]
	ldr r1, [dp]
	str tos, [dp]
	str r1, [dp, #4]
	movs tos, r0
	bx lr
	end_inlined

	@@ Pick a value at a specified depth on the stack
	define_word "pick", visible_flag | inlined_flag
_pick:	lsls tos, tos, #2
	adds tos, tos, dp
	ldr tos, [tos]
	bx lr
	end_inlined

	@@ Rotate a value at a given deph to the top of the stackk
	define_word "roll", visible_flag | inlined_flag
_roll:	movs r0, tos
	lsls r0, r0, #2
	adds r0, r0, dp
	ldr tos, [r0]
1:	cmp r0, dp
	beq 2f
	ldr r1, [r0, #-4]
	str r1, [r0]
	subs r0, #4
	b 1b
2:	adds dp, #4
	bx lr
	end_inlined

	@@ Logical shift left
	define_word "lshift", visible_flag | inlined_flag
_lshift:
	movs r0, tos
	pull_tos
	lsls tos, tos, r0
	bx lr
	end_inlined

	@@ Logical shift right
	define_word "rshift", visible_flag | inlined_flag
_rshift:
	movs r0, tos
	pull_tos
	lsrs tos, tos, r0
	bx lr
	end_inlined

	@@ Arithmetic shift right
	define_word "arshift", visible_flag | inlined_flag
_arshift:
	movs r0, tos
	pull_tos
	asrs tos, tos, r0
	bx lr
	end_inlined

	@@ Binary and
	define_word "and", visible_flag | inlined_flag
_and:	movs r0, tos
	pull_tos
	ands tos, r0
	bx lr
	end_inlined

	@@ Binary or
	define_word "or", visible_flag | inlined_flag
_or:	movs r0, tos
	pull_tos
	orrs tos, r0
	bx lr
	end_inlined

	@@ Binary xor
	define_word "xor", visible_flag | inlined_flag
_xor:	movs r0, tos
	pull_tos
	eors tos, r0
	bx lr
	end_inlined

	@@ Binary not
	define_word "not", visible_flag | inlined_flag
_not:	mvn tos, tos
	bx lr
	end_inlined

	@@ Negation
	define_word "negate", visible_flag | inlined_flag
_negate:
	mvn tos, tos
	adds tos, #1
	bx lr
	end_inlined
	
	@@ Addition of two two's complement integers
	define_word "+", visible_flag | inlined_flag
_add:	movs r0, tos
	pull_tos
	adds tos, tos, r0
	bx lr
	end_inlined

	@@ Substraction of two two's complement integers
	define_word "-", visible_flag | inlined_flag
_sub:	movs r0, tos
	pull_tos
	subs tos, tos, r0
	bx lr
	end_inlined

	@@ Multiplication of two two's complement integers
	define_word "*", visible_flag | inlined_flag
_mul:	movs r0, tos
	pull_tos
	muls tos, tos, r0
	bx lr
	end_inlined

	@@ Signed division of two two's complement integers
	define_word "/", visible_flag | inlined_flag
_div:	movs r0, tos
	pull_tos
	sdiv tos, tos, r0
	bx lr
	end_inlined

	@@ Unsigned division of two integers
	define_word "u/", visible_flag | inlined_flag
_udiv:	movs r0, tos
	pull_tos
	udiv tos, tos, r0
	bx lr
	end_inlined

	@@ Signed modulus of two two's complement integers
	define_word "mod", visible_flag | inlined_flag
_mod:	movs r0, tos
	pull_tos
	sdiv r1, tos, r0
	mls tos, r1, r0, tos
	bx lr
	end_inlined

	@@ Unsigned modulus of two unsigned integers
	define_word "umod", visible_flag | inlined_flag
_umod:	movs r0, tos
	pull_tos
	udiv r1, tos, r0
	mls tos, r1, r0, tos
	bx lr
	end_inlined

	@@ Equals
	define_word "=", visible_flag
_eq:	movs r0, tos
	pull_tos
	cmp tos, r0
	bne 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr

	@@ Not equal
	define_word "<>", visible_flag
_ne:	movs r0, tos
	pull_tos
	cmp tos, r0
	beq 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr

	@@ Less than
	define_word "<", visible_flag
_lt:	movs r0, tos
	pull_tos
	cmp tos, r0
	bge 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr

	@@ Greater than
	define_word ">", visible_flag
_gt:	movs r0, tos
	pull_tos
	cmp tos, r0
	ble 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr

	@@ Less than or equal
	define_word "<=", visible_flag
_le:	movs r0, tos
	pull_tos
	cmp tos, r0
	bgt 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr

	@@ Greater than or equal
	define_word ">=", visible_flag
_ge:	movs r0, tos
	pull_tos
	cmp tos, r0
	blt 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr

	@@ Get the HERE pointer
	define_word "here", visible_flag
_here:	ldr r0, =here
	push_tos
	ldr tos, [r0]
	bx lr

	@@ Get the PAD pointer
	define_word "pad", visible_flag
_pad:	ldr r0, =here
	ldr r1, =pad_offset
	push_tos
	ldr tos, [r0]
	adds tos, tos, r1
	bx lr

	@@ Allot space in RAM
	define_word "allot", visible_flag
_allot:	ldr r0, =here
	ldr r1, [r0]
	adds r1, tos
	str r1, [r0]
	pull_tos
	bx lr

	@@ Set the RAM flash pointer
	define_word "here!", visible_flag
_store_here:
	ldr r0, =here
	str tos, [r0]
	pull_tos
	bx lr

	@@ Get the flash HERE pointer
	define_word "flash-here", visible_flag
_flash_here:
	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	bx lr

	@@ Allot space in flash
	define_word "flash-allot", visible_flag
_flash_allot:
	ldr r0, =flash_here
	ldr r1, [r0]
	adds r1, tos
	str r1, [r0]
	pull_tos
	bx lr

	@@ Set the flash HERE pointer
	define_word "flash-here!", visible_flag
_store_flash_here:
	ldr r0, =flash_here
	str tos, [r0]
	pull_tos
	bx lr

	@@ Get the base address of the latest word
	define_word "latest", visible_flag
_latest:
	push_tos
	ldr r0, =latest
	ldr tos, [r0]
	bx lr

	@@ Get the base address of the latest RAM word
	define_word "ram-latest", visible_flag
_ram_latest:
	push_tos
	ldr r0, =ram_latest
	ldr tos, [r0]
	bx lr

	@@ Get the base address of the latest flash word
	define_word "flash-latest", visible_flag
_flash_latest:
	push_tos
	ldr r0, =flash_latest
	ldr tos, [r0]
	bx lr

	@@ Set the base address of the latest word
	define_word "latest!", visible_flag
_store_latest:
	ldr r0, =latest
	str tos, [r0]
	pull_tos
	bx lr

	@@ Set the base address of the latest RAM word
	define_word "ram-latest!", visible_flag
_store_ram_latest:
	ldr r0, =ram_latest
	str tos, [r0]
	pull_tos
	bx lr

	@@ Set the base address of the latest flash word
	define_word "flash-latest!", visible_flag
_store_flash_latest:
	ldr r0, =flash_latest
	str tos, [r0]
	pull_tos
	bx lr

	@@ Get the address to store a literal in for the word currently being
	@@ built
	define_word "build-target", visible_flag
_build_target:
	push_tos
	ldr tos, =build_target
	bx lr

	@@ Get either the HERE pointer or the flash HERE pointer, depending on
	@@ compilation mode
	define_word "current-here", visible_flag
_current_here:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _here
	pop {pc}
1:	bl _flash_here
	pop {pc}

	@@ Allot space in RAM or in flash, depending on the compilation mode
	define_word "current-allot", visible_flag
_current_allot:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _allot
	pop {pc}
1:	bl _flash_allot
	pop {pc}

	@@ Emit a character
	define_word "emit", visible_flag
_emit:	push {lr}
	ldr r0, =emit_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, r0
	bl _execute
	pop {pc}
1:	pull_tos
	pop {pc}

	@@ Test for whether the system is ready to receive a character
	define_word "emit?", visible_flag
_emit_q:
	push {lr}
	ldr r0, =emit_q_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, r0
	bl _execute
	pop {pc}
1:	push_tos
	movs tos, #0
	pop {pc}

	@@ Emit a space
	define_word "space", visible_flag
_space:	push {lr}
	push_tos
	movs tos, #0x20
	bl _emit
	pop {pc}

	@@ Emit a newline
	define_word "cr", visible_flag
_cr:	push {lr}
	push_tos
	movs tos, #0x0D
	bl _emit
	push_tos
	movs tos, #0x0A
	bl _emit
	pop {pc}

	@@ Type a string
	define_word "type", visible_flag
_type:	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
1:	cmp r0, #0
	beq 2f
	push_tos
	ldrb tos, [r1]
	push {r0, r1}
	bl _emit
	pop {r0, r1}
	subs r0, #1
	adds r1, #1
	b 1b
2:	pop {pc}

	@ Convert a cstring to a string
	define_word "count", visible_flag
_count:	ldrb r0, [tos]
	adds tos, #1
	push_tos
	movs tos, r0
	bx lr
	
	@@ Receive a character
	define_word "key", visible_flag
_key:	push {lr}
	ldr r0, =key_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, r0
	bl _execute
	pop {pc}
1:	push_tos
	movs tos, #0x0D
	pop {pc}

	@@ Test for whether the system is ready to receive a character
	define_word "key?", visible_flag
_key_q:	push {lr}
	ldr r0, =key_q_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, r0
	bl _execute
	pop {pc}
1:	push_tos
	movs tos, #0
	pop {pc}

	@@ Execute an xt
	define_word "execute", visible_flag
_execute:
	movs r0, tos
	adds r0, #1 @ Commented out to deal with an issue with Cutter @@@
	pull_tos
	bx r0
	
	@@ Execute an xt if it is non-zero
	define_word "?execute", visible_flag
_execute_nz:
	movs r0, tos
	pull_tos
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ Execute a PAUSE word, if one is set
	define_word "pause", visible_flag
_pause:	push {lr}
	ldr r0, =pause_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	push_tos
	movs tos, r0
	bl _execute
1:	pop {pc}
	
	@@ Default implementation of PAUSE, does nothing
	define_word "do-pause", visible_flag
_do_pause:
	bx lr
	
	@@ Exit a word
	define_word "exit", visible_flag
_exit:	pop {pc}

	@@ Initiatlize the dictionary
	define_word "init-dict", visible_flag
_init_dict:
	push {lr}
	bl _find_flash_end
	push_tos
	bl _next_flash_block
	ldr r1, =flash_dict_start
	ldr r0, =flash_here
	cmp tos, r1
	blt 1f
	str tos, [r0]
	b 2f
1:	str r1, [r0]
2:	bl _find_last_flash_word
	bl _find_last_visible_word
	ldr r0, =latest
	str tos, [r0]
	ldr r0, =flash_latest
	str tos, [r0]
	pull_tos
	movs r1, #0
	ldr r0, =ram_latest
	str r1, [r0]
	pop {pc}

	@@ Find the last visible word
	define_word "find-last-visible-word", visible_flag
_find_last_visible_word:
1:	cmp tos, #0
	beq 2f
	ldr r0, [tos]
	movs r1, #visible_flag
	tst r0, r1
	bne 2f
	ldr tos, [tos, #4]
	b 1b
2:	bx lr
	
 	@@ Run the initialization routine, if there is one
	define_word "do-init", visible_flag
_do_init:
	push {lr}
	string "init"
	push_tos
	movs tos, #visible_flag
	bl _find
	cmp tos, #0
	beq 1f
	bl _to_xt
	bl _execute
	pop {pc}
1:	pull_tos
	pop {pc}
	
	@@ Set the currently-defined word to be immediate
	define_word "[immediate]", visible_flag | immediate_flag | compiled_flag
_bracket_immediate:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #immediate_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Set the currently-defined word to be compile-only
	define_word "[compile-only]", visible_flag | immediate_flag | compiled_flag
_bracket_compile_only:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #compiled_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Set the currently-defined word to be inlined
	define_word "[inlined]", visible_flag | immediate_flag | compiled_flag
_bracket_inlined:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #inlined_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Set the currently-defined word to be immediate
	define_word "immediate", visible_flag
_immediate:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #immediate_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Set the currently-defined word to be compile-only
	define_word "compile-only", visible_flag
_compile_only:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #compiled_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Set the currently-defined word to be inlined
	define_word "inlined", visible_flag
_inlined:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #inlined_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Set the currently-defined word to be visible
	define_word "visible", visible_flag
_visible:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #visible_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Switch to interpretation mode
	define_word "[", visible_flag | immediate_flag
_to_interpret:
	ldr r0, =state
	movs r1, #0
	str r1, [r0]
	bx lr

	@@ Switch to compilation state
	define_word "]", visible_flag
_to_compile:
	ldr r0, =state
	movs r1, #-1
	str r1, [r0]
	bx lr

	@@ Set compilation to RAM
	define_word "compile-to-ram", visible_flag
_compile_to_ram:
	ldr r0, =compiling_to_flash
	movs r1, #0
	str r1, [r0]
	bx lr

	@@ Set compilation to flash
	define_word "compile-to-flash", visible_flag
_compile_to_flash:
	ldr r0, =compiling_to_flash
	movs r1, #-1
	str r1, [r0]
	bx lr

	@@ Get whether compilation is to flash
	define_word "compiling-to-flash", visible_flag
_compiling_to_flash:
	ldr r0, =compiling_to_flash
	push_tos
	ldr tos, [r0]
	bx lr

	@@ Compile an xt
	define_word "compile,", visible_flag
_compile:
	push {lr}
	bl _asm_call
	pop {pc}

	@@ Get the word corresponding to a token
	define_word "token-word", visible_flag
_token_word:
	push {lr}
	bl _token
	cmp tos, #0
	beq 1f
	push_tos
	movs tos, #visible_flag
	bl _find
	cmp tos, #0
	beq 2f
	pop {pc}
1:	ldr tos, =_token_expected
	bl _raise
	pop {pc}
2:	ldr tos, =_unknown_word
	bl _raise
	pop {pc}

	@@ Tick
	define_word "'", visible_flag
_tick:	push {lr}
	bl _token_word
	bl _to_xt
	pop {pc}

	@@ Compiled tick
	define_word "[']", visible_flag | immediate_flag | compiled_flag
_compiled_tick:
	push {lr}
	bl _tick
	push_tos
	movs tos, #6
	bl _asm_push
	push_tos
	movs tos, #6
	bl _asm_literal
	pop {pc}
	
	@@ Postpone a word
	define_word "postpone", visible_flag | immediate_flag | compiled_flag
_postpone:
	push {lr}
	bl _token_word
	ldr r0, [tos]
	tst r0, #immediate_flag
	beq 1f
	bl _to_xt
	bl _compile
	pop {pc}
1:	bl _to_xt
	push_tos
	movs tos, #6
	bl _asm_push
	push_tos
	movs tos, #6
	bl _asm_literal
	push_tos
	ldr tos, =_compile
	bl _compile
	pop {pc}

	@@ Compile a literal
	define_word "lit,", visible_flag
_comma_lit:	
	push {lr}
	push_tos
	movs tos, #6
	bl _asm_push
	push_tos
	movs tos, #6
	bl _asm_literal
	pop {pc}

	@@ Compile a literal
	define_word "literal", visible_flag | immediate_flag | compiled_flag
_literal:	
	push {lr}
	bl _comma_lit
	pop {pc}

	@@ Recursively call a word
	define_word "recurse", visible_flag | immediate_flag | compiled_flag
_recurse:
	push {lr}
	push_tos
	ldr tos, =current_compile
	ldr tos, [tos]
	bl _to_xt
	bl _asm_branch
	pop {pc}
	
	@@ Unknown word exception
	define_word "unknown-word", visible_flag
_unknown_word:
	string_ln " unknown word"
	bl _type
	bl _abort
	
	@@ Store a byte
	define_word "b!", visible_flag
_store_1:
	movs r0, tos
	pull_tos
	movs r1, #0xFF
	ands tos, r1
	strb tos, [r0]
	pull_tos
	bx lr

	@@ Store a halfword
	define_word "h!", visible_flag
_store_2:
	movs r0, tos
	pull_tos
	ldr r1, =0xFFFF
	ands tos, r1
	strh tos, [r0]
	pull_tos
	bx lr

	@@ Store a word
	define_word "!", visible_flag
_store_4:
	movs r0, tos
	pull_tos
	str tos, [r0]
	pull_tos
	bx lr

	@@ Store a doubleword
	define_word "2!", visible_flag
_store_8:
	movs r0, tos
	pull_tos
	str tos, [r0]
	pull_tos
	str tos, [r0, #4]
	bx lr

	@@ Get a byte
	define_word "b@", visible_flag
_get_1: ldrb tos, [tos]
	bx lr

	@@ Get a halfword
	define_word "h@", visible_flag
_get_2: ldrh tos, [tos]
	bx lr

	@@ Get a word
	define_word "@", visible_flag
_get_4: ldr tos, [tos]
	bx lr

	@@ Get a doubleword
	define_word "2@", visible_flag
_get_8:	ldr r0, [tos]
	ldr tos, [tos, #4]
	push_tos
	movs tos, r0
	bx lr

	@@ Store a byte at the HERE location
	define_word "b,", visible_flag
_comma_1:
	ldr r0, =here
	ldr r1, [r0]
	movs r2, #0xFF
	ands tos, r2
	strb tos, [r1], #1
	str r1, [r0]
	pull_tos
	bx lr

	@@ Store a halfword at the HERE location
	define_word "h,", visible_flag
_comma_2:
	ldr r0, =here
	ldr r1, [r0]
	ldr r2, =0xFFFF
	ands tos, r2
	strh tos, [r1], #2
	str r1, [r0]
	pull_tos
	bx lr

	@@ Store a word at the HERE location
	define_word ",", visible_flag
_comma_4:
	ldr r0, =here
	ldr r1, [r0]
	str tos, [r1], #4
	str r1, [r0]
	pull_tos
	bx lr

	@@ Store a doubleword at the HERE location
	define_word "2,", visible_flag
_comma_8:
	ldr r0, =here
	ldr r1, [r0]
	str tos, [r1], #4
	pull_tos
	str tos, [r1], #4
	str r1, [r0]
	bx lr

	@@ Store a byte at the flash HERE location
	define_word "bflash,", visible_flag
_flash_comma_1:
	push {lr}

	@@ Test
	@@ string "bflash, "
	@@ bl _type
	@@ push_tos
	@@ bl _type_unsigned
	@@ bl _cr
	@@ End Test
	
	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	bl _store_flash_1
	pop {r0, r1}
	adds r1, #1
	str r1, [r0]
	pop {pc}

	@@ Store a halfword at the flash HERE location
	define_word "hflash,", visible_flag
_flash_comma_2:
	push {lr}

	@@ Test
	@@ string "hflash, "
	@@ bl _type
	@@ push_tos
	@@ bl _type_unsigned
	@@ bl _cr
	@@ End Test

	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	bl _store_flash_2
	pop {r0, r1}
	adds r1, #2
	str r1, [r0]
	pop {pc}

	@@ Store a word at the flash HERE location
	define_word "flash,", visible_flag
_flash_comma_4:
	push {lr}

	@@ Test
	@@ string "flash, "
	@@ bl _type
	@@ push_tos
	@@ bl _type_unsigned
	@@ bl _cr
	@@ End Test
	
	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	bl _store_flash_4
	pop {r0, r1}
	adds r1, #4
	str r1, [r0]
	pop {pc}

	@@ Store a doubleword at the flash HERE location
	define_word "2flash,", visible_flag
_flash_comma_8:
	push {lr}
	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	bl _store_flash_8
	pop {r0, r1}
	adds r1, #8
	str r1, [r0]
	pop {pc}

	@@ Store a byte to RAM or to flash
	define_word "bcurrent!", visible_flag
_store_current_1:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _store_1
	pop {pc}
1:	bl _store_flash_1
	pop {pc}

	@@ Store a halfword to RAM or to flash
	define_word "hcurrent!", visible_flag
_store_current_2:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _store_2
	pop {pc}
1:	bl _store_flash_2
	pop {pc}

	@@ Store a word to RAM or to flash
	define_word "current!", visible_flag
_store_current_4:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _store_4
	pop {pc}
1:	bl _store_flash_4
	pop {pc}

	@@ Store a doubleword to RAM or to flash
	define_word "2current!", visible_flag
_store_current_8:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _store_8
	pop {pc}
1:	bl _store_flash_8
	pop {pc}

	@@ Store a byte to the RAM or flash HERE location
	define_word "bcurrent,", visible_flag
_current_comma_1:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _comma_1
	pop {pc}
1:	bl _flash_comma_1
	pop {pc}

	@@ Store a halfword to the RAM or flash HERE location
	define_word "hcurrent,", visible_flag
_current_comma_2:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _comma_2
	pop {pc}
1:	bl _flash_comma_2
	pop {pc}

	@@ Store a word to the RAM or flash HERE location
	define_word "current,", visible_flag
_current_comma_4:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _comma_4
	pop {pc}
1:	bl _flash_comma_4
	pop {pc}

	@@ Store a doubleword to the RAM or flash HERE location
	define_word "2current,", visible_flag
_current_comma_8:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _comma_8
	pop {pc}
1:	bl _flash_comma_8
	pop {pc}

	@@ Reserve a byte at the RAM HERE location
	define_word "breserve", visible_flag
_reserve_1:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #1
	str r1, [r0]
	bx lr

	@@ Reserve a halfword at the RAM HERE location
	define_word "hreserve", visible_flag
_reserve_2:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #2
	str r1, [r0]
	bx lr

	@@ Reserve a word at the RAM HERE location
	define_word "reserve", visible_flag
_reserve_4:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #4
	str r1, [r0]
	bx lr

	@@ Reserve a doubleword at the RAM HERE location
	define_word "2reserve", visible_flag
_reserve_8:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #8
	str r1, [r0]
	bx lr

	@@ Reserve a byte at the flash HERE location
	define_word "bflash-reserve", visible_flag
_flash_reserve_1:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #1
	str r1, [r0]
	bx lr

	@@ Reserve a halfword at the flash HERE location
	define_word "hflash-reserve", visible_flag
_flash_reserve_2:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #2
	str r1, [r0]
	bx lr

	@@ Reserve a word at the flash HERE location
	define_word "flash-reserve", visible_flag
_flash_reserve_4:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #4
	str r1, [r0]
	bx lr

	@@ Reserve a doubleword at the flash HERE location
	define_word "2flash-reserve", visible_flag
_flash_reserve_8:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #8
	str r1, [r0]
	bx lr

	@@ Reserve a byte at the RAM or flash HERE location
	define_word "bcurrent-reserve", visible_flag
_current_reserve_1:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _reserve_1
	pop {pc}
1:	bl _flash_reserve_1
	pop {pc}

	@@ Reserve a halfword at the RAM or flash HERE location
	define_word "hcurrent-reserve", visible_flag
_current_reserve_2:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _reserve_2
	pop {pc}
1:	bl _flash_reserve_2
	pop {pc}

	@@ Reserve a word at the RAM or flash HERE location
	define_word "current-reserve", visible_flag
_current_reserve_4:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _reserve_4
	pop {pc}
1:	bl _flash_reserve_4
	pop {pc}

	@@ Reserve a doubleword at the RAM or flash HERE location
	define_word "2current-reserve", visible_flag
_current_reserve_8:
	push {lr}
	ldr r0, =compiling_to_flash
	ldr r0, [r0]
	cmp r0, #0
	bne 1f
	bl _reserve_8
	pop {pc}
1:	bl _flash_reserve_8
	pop {pc}

	@@ Align to a power of two
	define_word "current-align,", visible_flag
_current_comma_align:
	push {lr}
	subs tos, #1
	movs r0, tos
	pull_tos
1:	push {r0}
	bl _current_here
	pop {r0}
	ands tos, r0
	beq 2f
	movs tos, #0
	push {r0}
	bl _current_comma_1
	pop {r0}
	b 1b
2:	pull_tos
	pop {pc}

	@@ Align to a power of two
	define_word "align,", visible_flag
_comma_align:
	push {lr}
	subs tos, #1
	movs r0, tos
	pull_tos
1:	push {r0}
	bl _here
	pop {r0}
	ands tos, r0
	beq 2f
	movs tos, #0
	push {r0}
	bl _comma_1
	pop {r0}
	b 1b
2:	pop {pc}

	@@ Compile a c-string
	define_word "current-cstring,", visible_flag
_current_comma_cstring:
	push {lr}
	ldr r0, =255
	cmp tos, r0
	ble 1f
	movs tos, r0
1:	push_tos
	bl _current_comma_1
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
2:	cmp r0, #0
	beq 1f
	push_tos
	ldrb tos, [r1]
	push {r0, r1}
	bl _current_comma_1
	pop {r0, r1}
	subs r0, #1
	adds r1, #1
	b 2b
1:	pop {pc}	

	@@ Push a value onto the return stack
	define_word ">r", visible_flag
_push_r:
	push {tos}
	pull_tos
	bx lr

	@@ Pop a value off the return stack
	define_word "r>", visible_flag
_pop_r:	push_tos
	pop {tos}
	bx lr

	@@ Get a value off the return stack without popping it
	define_word "r@", visible_flag
_get_r:	push_tos
	ldr tos, [sp]
	bx lr

	@@ Drop a value from the return stack
	define_word "rdrop", visible_flag
_rdrop:	adds sp, #4
	bx lr

	@@ Get the return stack pointer
	define_word "rp@", visible_flag
_get_rp:
	push_tos
	mov tos, sp
	bx lr

	@@ Set the return stack pointer
	define_word "rp!", visible_flag
_store_rp:
	mov sp, tos
	pull_tos
	bx lr

	@@ Get the data stack pointer
	define_word "sp@", visible_flag
_get_sp:
	push_tos
	movs tos, dp
	bx lr

	@@ Set the data stack pointer
	define_word "sp!", visible_flag
_store_sp:
	movs dp, tos
	pull_tos
	bx lr

	@@ Initialize the variables
	define_word "init-variables", visible_flag
_init_variables:
	push {lr}
	bl _init_handlers
	ldr r0, =prompt_hook
	ldr r1, =_do_prompt
	str r1, [r0]
	ldr r0, =handle_number_hook
	ldr r1, =_do_handle_number
	str r1, [r0]
	ldr r0, =failed_parse_hook
	ldr r1, =_do_failed_parse
	str r1, [r0]
	ldr r0, =emit_hook
	ldr r1, =_serial_emit
	str r1, [r0]
	ldr r0, =emit_q_hook
	ldr r1, =_serial_emit_q
	str r1, [r0]
	ldr r0, =key_hook
	ldr r1, =_serial_key
	str r1, [r0]
	ldr r0, =key_q_hook
	ldr r1, =_serial_key_q
	str r1, [r0]
	ldr r0, =refill_hook
	ldr r1, =_do_refill
	str r1, [r0]
	ldr r0, =pause_hook
	ldr r1, =_do_pause
	str r1, [r0]
	ldr r0, =compiling_to_flash
	movs r1, 0
	str r1, [r0]
	ldr r0, =current_compile
	str r1, [r0]
	ldr r0, =latest
	str r1, [r0]
	ldr r0, =ram_latest
	str r1, [r0]
	ldr r0, =flash_latest
	str r1, [r0]
	ldr r0, =building
	str r1, [r0]
	ldr r0, =build_target
	str r1, [r0]
	ldr r0, =current_flags
	str r1, [r0]
	ldr r0, =input_buffer_index
	str r1, [r0]
	ldr r0, =input_buffer_count
	str r1, [r0]
	ldr r0, =state
	str r1, [r0]
	movs r1, #10
	ldr r0, =base
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
	pop {pc}
	
	.ltorg
	
