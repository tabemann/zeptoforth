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
	define_word "drop", visible_flag
_drop:	pull_tos
	bx lr

	@@ Duplicate the top of the data stack
	define_word "dup", visible_flag
_dup:	push_tos
	bx lr

	@@ Swap the top two places on the data stack
	define_word "swap", visible_flag
_swap:	mov r0, tos
	ldr tos, [dp]
	str r0, [dp]
	bx lr

	@@ Copy the second place on the data stack onto the top of the stack,
	@@ pushing the top of the data stack to the second place
	define_word "over", visible_flag
_over:	push_tos
	ldr tos, [dp, #4]
	bx lr

	@@ Rotate the top three places on the data stack, so the third place
	@@ moves to the first place
	define_word "rot", visible_flag
_rot:	ldr r0, [dp, #4]
	ldr r1, [dp]
	str tos, [dp]
	str r1, [dp, #4]
	mov tos, r0
	bx lr

	@@ Pick a value at a specified depth on the stack
	define_word "pick", visible_flag
_pick:	lsls tos, tos, #2
	adds tos, tos, dp
	ldr tos, [tos]
	bx lr

	@@ Rotate a value at a given deph to the top of the stackk
	define_word "roll", visible_flag
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

	@@ Logical shift left
	define_word "lshift", visible_flag
_lshift:
	movs r0, tos
	pull_tos
	lsls tos, tos, r0
	bx lr

	@@ Logical shift right
	define_word "rshift", visible_flag
_rshift:
	movs r0, tos
	pull_tos
	lsrs tos, tos, r0
	bx lr

	@@ Arithmetic shift right
	define_word "arshift", visible_flag
_arshift:
	movs r0, tos
	pull_tos
	asrs tos, tos, r0
	bx lr

	@@ Addition of two two's complement integers
	define_word "+", visible_flag
_add:	movs r0, tos
	pull_tos
	adds tos, tos, r0
	bx lr

	@@ Substraction of two two's complement integers
	define_word "-", visible_flag
_sub:	movs r0, tos
	pull_tos
	subs tos, tos, r0
	bx lr

	@@ Multiplication of two two's complement integers
	define_word "*", visible_flag
_mul:	movs r0, tos
	pull_tos
	muls tos, tos, r0
	bx lr

	@@ Signed division of two two's complement integers
	define_word "/", visible_flag
_div:	movs r0, tos
	pull_tos
	sdiv tos, tos, r0
	bx lr

	@@ Unsigned division of two integers
	define_word "u/", visible_flag
_udiv:	movs r0, tos
	pull_tos
	udiv tos, tos, r0
	bx lr

	@@ Get the HERE pointer
	define_word "here", visible_flag
_here:	ldr r0, =here
	push_tos
	ldr tos, [r0]
	bx lr

	@@ Allot space in RAM
	define_word "allot", visible_flag
_allot:	ldr r0, =here
	ldr r1, [r0]
	adds r1, tos
	str r1, [r0]
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

	@@ The emit hook
	define_word "emit-hook", visible_flag
_emit_hook:
	push_tos
	ldr tos, =emit_hook
	bx lr
	
	@@ Emit a character
	define_word "emit", visible_flag
_emit:	push {lr}
	ldr r0, =emit_hook
	ldr r0, [r0]
	cmp r0, #0
	beq 1f
	adds r0, #1
	blx r0
	pop {pc}
1:	pull_tos
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
	
	@@ Execute an xt
	define_word "execute", visible_flag
_execute:
	mov r0, tos
	adds r0, #1
	pull_tos
	bx r0
	
	@@ Execute an xt if it is non-zero
	define_word "?execute"
_execute_nz:
	mov r0, tos
	pull_tos
	cmp r0, #0
	beq 1f
	adds r0, #1
	bx r0
1:	bx lr

	@@ Exit a word
	define_word "exit", visible_flag
_exit:	adds sp, sp, #4
	pop {pc}

	@@ Initiatlize the dictionary
	define_word "init-dict", visible_flag
_init_dict:
	push {lr}
	bl _find_last_flash_word
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
	tsts r0, r1
	bne 2f
	ldr tos, [tos, #4]
	b 1b
2:	bx lr
	
 	@@ Run the initialization routine, if there is one
	define_word "do-init", visible_flag
_do_init:
	push {lr}
	string "init"
	bl _find
	cmp tos, #0
	beq 1f
	bl _to_xt
	movs r0, tos
	pull_tos
	blx r0
	pop {pc}
1:	pull_tos
	pop {pc}
	
	@@ Set the currently-defined word to be immediate
	define_word "[immediate]", visible_flag | immediate_flag | compile_only_flag
_bracket_immediate:
	ldr r0, =latest_flags
	ldr r1, [r0]
	movs r2, #immediate_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Set the currently-defined word to be compile-only
	define_word "[compile-only]", visible_flag | immediate_flag | compile_only_flag
_bracket_compile_only:
	ldr r0, =latest_flags
	ldr r1, [r0]
	movs r2, #compile_only_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Set the currently-defined word to be immediate
	define_word "immediate", visible_flag
_immediate:
	ldr r0, =latest_flags
	ldr r1, [r0]
	movs r2, #immediate_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Set the currently-defined word to be compile-only
	define_word "compile-only", visible_flag
_compile_only:
	ldr r0, =latest_flags
	ldr r1, [r0]
	movs r2, #compile_only_flag
	orr r1, r2
	str r1, [r0]
	bx lr

	@@ Store a byte
	define_word "b!", visible_flag
_store_1:
	mov r0, tos
	pull_tos
	movs r5, #0xFF
	ands tos, r5
	strb tos, [r0]
	pull_tos
	bx lr

	@@ Store a halfword
	define_word "h!", visible_flag
_store_2:
	mov r0, tos
	pull_tos
	ldr r1, =0xFFFF
	ands tos, r1
	strh tos, [r0]
	pull_tos
	bx lr

	@@ Store a word
	define_word "!", visible_flag
_store_4:
	mov r0, tos
	pull_tos
	str tos, [r0]
	pull_tos
	bx lr

	@@ Store a doubleword
	define_word "2!", visible_flag
_store_8:
	mov r0, tos
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
	mov tos, r0
	bx lr

	@@ Store a byte at the HERE location
	define_word "b,", visible_flag
_comma_1:
	ldr r0, =here
	ldr r1, [r0]
	mov r5, #0xFF
	ands tos, r5
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
	ldr r0, =here
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
	ldr r0, =here
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
	ldr r0, =here
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
	ldr r0, =here
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
	mov tos, r1
	adds r1, #1
	str r1, [r0]
	bx lr

	@@ Reserve a halfword at the RAM HERE location
	define_word "hreserve", visible_flag
_reserve_2:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	adds r1, #2
	str r1, [r0]
	bx lr

	@@ Reserve a word at the RAM HERE location
	define_word "reserve", visible_flag
_reserve_4:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	adds r1, #4
	str r1, [r0]
	bx lr

	@@ Reserve a doubleword at the RAM HERE location
	define_word "2reserve", visible_flag
_reserve_8:
	ldr r0, =here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	adds r1, #8
	str r1, [r0]
	bx lr

	@@ Reserve a byte at the flash HERE location
	define_word "bflash-reserve", visible_flag
_flash_reserve_1:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	adds r1, #1
	str r1, [r0]
	bx lr

	@@ Reserve a halfword at the flash HERE location
	define_word "hflash-reserve", visible_flag
_flash_reserve_2:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	adds r1, #2
	str r1, [r0]
	bx lr

	@@ Reserve a word at the flash HERE location
	define_word "flash-reserve", visible_flag
_flash_reserve_4:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	mov tos, r1
	adds r1, #4
	str r1, [r0]
	bx lr

	@@ Reserve a doubleword at the flash HERE location
	define_word "2flash-reserve", visible_flag
_flash_reserve_8:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	mov tos, r1
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
2:	pop {pc}
	
	@@ Compile a c-string
	define_word "current-cstring,", visible_flag
_current_comma_cstring:
	push {lr}
	movs r0, #255
	cmp tos, r0
	ble 1f
	movs tos, #255
1:	push_tos
	bl _current_comma_1
	movs r0, tos
	pull_tos
	movs r1, tos
2:	cmp r0, #0
	beq 1f
	ldrb tos, [r1]
	push {r0, r1}
	bl _current_comma_1
	pop {r0, r1}
	sub r0, #1
	add r1, #1
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

	@@ Initialize the hooks
	define_word "init-hooks", visible_flag
_init_hooks:
	ldr r0, =prompt_hook
	ldr r1, =_do_prompt
	str r1, [r0]
	ldr r0, =handle_number_hook
	ldr r1, =_do_handle_number
	str r1, [r0]
	ldr r0, =parse_integer_hook
	ldr r1, =_do_parse_integer
	str r1, [r0]
	ldr r0, =failed_parse_hook
	ldr r1, =_do_failed_parse
	str r1, [r0]
	ldr r0, =emit_hook
	ldr r1, =_do_emit
	str r1, [r0]
	ldr r0, =key_hook
	ldr r1, =_do_key
	str r1, [r0]
	ldr r0, =key_q_hook
	ldr r1, =_do_key_q
	bx lr
	
