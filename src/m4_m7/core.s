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

	@ Include the kernel info, needed for welcome
	.include "../common/kernel_info.s"

	@ Include the legal info
	.include "../common/legal.s"
	
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

	@@ Remove the cell under that on the top of the stack
	define_word "nip", visible_flag | inlined_flag
_nip:	adds dp, #4
	bx lr
	end_inlined

	@@ Push the cell on top of the stack under the item beneath it
	define_word "tuck", visible_flag | inlined_flag
_tuck:	ldr r0, [dp]
	str tos, [dp]
	subs dp, #4
	str r0, [dp]
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

	@@ Bit clear
	define_word "bic", visible_flag | inlined_flag
_bic:	movs r0, tos
	pull_tos
	bics tos, r0
	bx lr
	end_inlined

	@@ Binary not
	define_word "not", visible_flag | inlined_flag
_not:	mvns tos, tos
	bx lr
	end_inlined

	@@ Negation
	define_word "negate", visible_flag | inlined_flag
_negate:
	mvns tos, tos
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

	@@ Get the minimum of two values
	define_word "min", visible_flag | inlined_flag
_min:	.if cortex_m7
        ldr r0, [dp], #4
        .else
        ldmia dp!, {r0}
        .endif
	cmp tos, r0
	blt 1f
	movs tos, r0
1:	bx lr
	end_inlined

	@@ Get the maximum of two values
	define_word "max", visible_flag | inlined_flag
_max:	.if cortex_m7
        ldr r0, [dp], #4
        .else
        ldmia dp!, {r0}
        .endif
	cmp tos, r0
	bgt 1f
	movs tos, r0
1:	bx lr
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
	end_inlined

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
	end_inlined

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
	end_inlined

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
	end_inlined

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
	end_inlined

	@@ Equals zero
	define_word "0=", visible_flag | inlined_flag
_0eq:	subs tos, #1
	sbcs tos, tos
	bx lr
	end_inlined

	@@ Not equal to zero
	define_word "0<>", visible_flag | inlined_flag
_0ne:	subs tos, #1
	sbcs tos, tos
	mvns tos, tos
	bx lr
	end_inlined

	@@ Less than zero
	define_word "0<", visible_flag | inlined_flag
_0lt:	asrs tos, #31
	bx lr
	end_inlined

	@@ Greater than zero
	define_word "0>", visible_flag | inlined_flag
_0gt:	movs r0, tos
	movs tos, #0
	cmp r0, #0
	ble 1f
	mvns tos, tos
1:	bx lr
	end_inlined

	@@ Less than or equal to zero
	define_word "0<=", visible_flag | inlined_flag
_0le:	movs r0, tos
	movs tos, #0
	cmp r0, #0
	bgt 1f
	mvns tos, tos
1:	bx lr
	end_inlined

	@@ Greater than or equal to zero
	define_word "0>=", visible_flag | inlined_flag
_0ge:	asrs tos, #31
	mvns tos, tos
	bx lr
	end_inlined
	
	@@ Unsigned less than
	define_word "u<", visible_flag
_ult:	movs r0, tos
	pull_tos
	cmp tos, r0
	bhs 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr
	end_inlined

	@@ Unsigned greater than
	define_word "u>", visible_flag
_ugt:	movs r0, tos
	pull_tos
	cmp tos, r0
	bls 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr
	end_inlined

	@@ Unsigned less than or equal
	define_word "u<=", visible_flag
_ule:	movs r0, tos
	pull_tos
	cmp tos, r0
	bhi 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr
	end_inlined

	@@ Unsigned greater than or equal
	define_word "u>=", visible_flag
_uge:	movs r0, tos
	pull_tos
	cmp tos, r0
	blo 1f
	movs tos, #-1
	bx lr
1:	movs tos, #0
	bx lr
	end_inlined
	
	@@ Get the RAM HERE pointer
	define_word "ram-here", visible_flag
_here:	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds tos, r0
	ldr tos, [tos]
	adds tos, #ram_here_offset
	ldr tos, [tos]
	pop {pc}
	end_inlined

	@@ Get the PAD pointer
	define_word "pad", visible_flag
_pad:	push {lr}
	bl _here
	ldr r1, =pad_offset
	adds tos, tos, r1
	pop {pc}
	end_inlined

	@@ Allot space in RAM
	define_word "ram-allot", visible_flag
_allot:	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	ldr r1, [r0]
	adds r1, tos
	str r1, [r0]
	pull_tos
	pop {pc}
	end_inlined

	@@ Set the RAM flash pointer
	define_word "ram-here!", visible_flag
_store_here:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	str tos, [r0]
	pull_tos
	pop {pc}
	end_inlined

	@@ Get the flash HERE pointer
	define_word "flash-here", visible_flag
_flash_here:
	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	bx lr
	end_inlined

	@@ Allot space in flash
	define_word "flash-allot", visible_flag
_flash_allot:
	ldr r0, =flash_here
	ldr r1, [r0]
	adds r1, tos
	str r1, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Set the flash HERE pointer
	define_word "flash-here!", visible_flag
_store_flash_here:
	ldr r0, =flash_here
	str tos, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Get the base address of the latest word
	define_word "latest", visible_flag
_latest:
	push_tos
	ldr r0, =latest
	ldr tos, [r0]
	bx lr
	end_inlined

	@@ Get the base address of the latest RAM word
	define_word "ram-latest", visible_flag
_ram_latest:
	push_tos
	ldr r0, =ram_latest
	ldr tos, [r0]
	bx lr
	end_inlined

	@@ Get the base address of the latest flash word
	define_word "flash-latest", visible_flag
_flash_latest:
	push_tos
	ldr r0, =flash_latest
	ldr tos, [r0]
	bx lr
	end_inlined

	@@ Set the base address of the latest word
	define_word "latest!", visible_flag
_store_latest:
	ldr r0, =latest
	str tos, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Set the base address of the latest RAM word
	define_word "ram-latest!", visible_flag
_store_ram_latest:
	ldr r0, =ram_latest
	str tos, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Set the base address of the latest flash word
	define_word "flash-latest!", visible_flag
_store_flash_latest:
	ldr r0, =flash_latest
	str tos, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Get either the HERE pointer or the flash HERE pointer, depending on
	@@ compilation mode
	define_word "here", visible_flag
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
	end_inlined

	@@ Allot space in RAM or in flash, depending on the compilation mode
	define_word "allot", visible_flag
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
	end_inlined

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
	end_inlined

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
	end_inlined

	@@ Emit a space
	define_word "space", visible_flag
_space:	push {lr}
	push_tos
	movs tos, #0x20
	bl _emit
	pop {pc}
	end_inlined

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
	end_inlined

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
	end_inlined

	@@ Type a string using the native serial driver
	define_word "serial-type", visible_flag
_serial_type:
	push {lr}
	movs r0, tos
	pull_tos
	movs r1, tos
	pull_tos
1:	cmp r0, #0
	beq 2f
	push_tos
	ldrb tos, [r1]
	push {r0, r1}
	bl _serial_emit
	pop {r0, r1}
	subs r0, #1
	adds r1, #1
	b 1b
2:	pop {pc}
	end_inlined

	@ Convert a cstring to a string
	define_word "count", visible_flag
_count:	ldrb r0, [tos]
	adds tos, #1
	push_tos
	movs tos, r0
	bx lr
	end_inlined
	
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
	end_inlined

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
	end_inlined

	@@ Enable interrupts
	define_word "enable-int", visible_flag
_enable_int:
	cpsie i
	bx lr
	end_inlined

	@@ Disable interrupts
	define_word "disable-int", visible_flag
_disable_int:
	cpsid i
@	dsb
@	isb
	bx lr
	end_inlined

	@@ Enter sleep mode
	define_word "sleep", visible_flag
_sleep:
	mrs r0, BASEPRI
	bic r0, #0xF0
	msr BASEPRI, r0
	dsb
	isb
	wfi
	bx lr
	end_inlined
	
	@@ Execute an xt
	define_word "execute", visible_flag
_execute:
	movs r0, tos
	pull_tos
	mov pc, r0
	bx lr
	end_inlined

	@@ Inline-execute an xt
	define_word "inline-execute", visible_flag | inlined_flag
_inline_execute:
	movs r0, tos
	pull_tos
	adds r0, #1
	blx r0
	bx lr
	end_inlined

	@@ Execute an xt if it is non-zero
	define_word "?execute", visible_flag
_execute_nz:
	movs r0, tos
	pull_tos
	cmp r0, #0
	beq 1f
	mov pc, r0
1:	bx lr
	end_inlined

	@@ Execute a PAUSE word, if one is set
	define_word "pause", visible_flag
_pause:	ldr r0, =pause_enabled
	ldr r0, [r0]
	cmp r0, #0
	ble 1f
	ldr r0, =pause_hook
	ldr r0, [r0]
	mov pc, r0
1:	bx lr
	end_inlined
	
	@@ Do nothing
	define_internal_word "do-nothing", visible_flag
_do_nothing:
	bx lr
	end_inlined
	
	@@ Exit a word
	define_word "exit", visible_flag
_exit:	pop {pc}
	end_inlined

	@@ Initialize the flash dictionary
	define_internal_word "init-flash-dict", visible_flag
_init_flash_dict:
	push {lr}
	bl _find_flash_end
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
	pop {pc}
	end_inlined
	
	@@ Initiatlize the dictionary
	define_internal_word "init-dict", visible_flag
_init_dict:
	push {lr}
	bl _init_flash_dict
	pull_tos
	movs r1, #0
	ldr r0, =ram_latest
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Find the last visible word
	define_internal_word "find-last-visible-word", visible_flag
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
	end_inlined

	@@ Display a welcome message
	define_word "welcome", visible_flag
_welcome:
	push {lr}
	string_ln ""
	bl _type
	string_ln "Welcome to zeptoforth"
	bl _type
	string "Built for "
	bl _type
	bl _kernel_platform
	bl _type
	string ", version "
	bl _type
	bl _kernel_version
	bl _type
	string ", on "
	bl _type
	bl _kernel_date
	bl _type
	bl _cr
	string "zeptoforth comes with ABSOLUTELY NO WARRANTY: "
	bl _type
	string_ln "for details type `license'"
	bl _type
	string_ln " ok"
	bl _type
	pop {pc}
	end_inlined

	.ltorg
	
	@@ An empty init routine, to call if no other init routines are
	@@ available, so as to enable any source file to call a preceding init
	@@ routine without having to check if one exists
	define_word "init", visible_flag
_init:	movs r0, #0
	msr BASEPRI, r0
	bx lr
	end_inlined
	
 	@@ Run the initialization routine, if there is one
	define_internal_word "do-init", visible_flag
_do_init:
	push {lr}
	string "init"
	bl _find_all
	cmp tos, #0
	beq 1f
	bl _to_xt
	bl _execute
	pop {pc}
1:	pull_tos
	pop {pc}
	end_inlined
	
	@@ Set the currently-defined word to be immediate
	define_word "[immediate]", visible_flag | immediate_flag | compiled_flag
_bracket_immediate:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #immediate_flag
	orrs r1, r2
	str r1, [r0]
	bx lr
	end_inlined

	@@ Set the currently-defined word to be compile-only
	define_word "[compile-only]", visible_flag | immediate_flag | compiled_flag
_bracket_compile_only:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #compiled_flag
	orrs r1, r2
	str r1, [r0]
	bx lr
	end_inlined

	@@ Set the currently-defined word to be inlined
	define_word "[inlined]", visible_flag | immediate_flag | compiled_flag
_bracket_inlined:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #inlined_flag
	orrs r1, r2
	str r1, [r0]
	bx lr
	end_inlined

	@@ Set the currently-defined word to be immediate
	define_word "immediate", visible_flag
_immediate:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #immediate_flag
	orrs r1, r2
	str r1, [r0]
	bx lr
	end_inlined

	.ltorg
	
	@@ Set the currently-defined word to be compile-only
	define_word "compile-only", visible_flag
_compile_only:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #compiled_flag
	orrs r1, r2
	str r1, [r0]
	bx lr
	end_inlined

	@@ Set the currently-defined word to be inlined
	define_word "inlined", visible_flag
_inlined:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #inlined_flag
	orrs r1, r2
	str r1, [r0]
	bx lr
	end_inlined

	@@ Set the currently-defined word to be visible
	define_word "visible", visible_flag
_visible:
	ldr r0, =current_flags
	ldr r1, [r0]
	movs r2, #visible_flag
	orrs r1, r2
	str r1, [r0]
	bx lr
	end_inlined

	@@ Switch to interpretation mode
	define_word "[", visible_flag | immediate_flag
_to_interpret:
	ldr r0, =state
	movs r1, #0
	str r1, [r0]
	bx lr
	end_inlined

	@@ Switch to compilation state
	define_word "]", visible_flag
_to_compile:
	ldr r0, =state
	movs r1, #-1
	str r1, [r0]
	bx lr
	end_inlined

	@@ Set compilation to RAM
	define_word "compile-to-ram", visible_flag
_compile_to_ram:
	push {lr}
	bl _asm_undefer_lit
	ldr r0, =compiling_to_flash
	movs r1, #0
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Set compilation to flash
	define_word "compile-to-flash", visible_flag
_compile_to_flash:
	push {lr}
	bl _asm_undefer_lit
	ldr r0, =compiling_to_flash
	movs r1, #-1
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Get whether compilation is to flash
	define_word "compiling-to-flash?", visible_flag
_compiling_to_flash:
	ldr r0, =compiling_to_flash
	push_tos
	ldr tos, [r0]
	bx lr
	end_inlined

	@@ Get whether to compress code compiled to flash
	define_word "compress-flash", visible_flag
_compress_flash:
	ldr r0, =compress_flash_enabled
	movs r1, #-1
	str r1, [r0]
	bx lr
	end_inlined

	@@ Get whether flash is being compressed
	define_word "compressing-flash", visible_flag
_compressing_flash:
	ldr r0, =compress_flash_enabled
	push_tos
	ldr tos, [r0]
	bx lr
	end_inlined

	@@ Compile an xt
	define_word "compile,", visible_flag
_compile:
	push {lr}
	bl _asm_call
	pop {pc}
	end_inlined

	@@ Get the word corresponding to a token
	define_word "token-word", visible_flag
_token_word:
	push {lr}
	bl _token
	cmp tos, #0
	beq 1f
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
	end_inlined

	@@ Tick
	define_word "'", visible_flag
_tick:	push {lr}
	bl _token_word
	bl _to_xt
	pop {pc}
	end_inlined

	@@ Compiled tick
	define_word "[']", visible_flag | immediate_flag | compiled_flag
_compiled_tick:
	push {lr}
	bl _tick
	bl _comma_lit
	pop {pc}
	end_inlined
	
	@@ Postpone a word
	define_word "postpone", visible_flag | immediate_flag | compiled_flag
_postpone:
	push {lr}
	bl _token_word
	ldr r0, [tos]
	tst r0, #immediate_flag
	beq 1f
	tst r0, #inlined_flag
	bne 3f
	bl _to_xt
	bl _compile
	pop {pc}
3:	bl _to_xt
	bl _asm_inline
	pop {pc}
1:	push {r0}
	bl _to_xt
	push_tos
	movs tos, #6
	bl _asm_push
	push_tos
	movs tos, #6
	bl _asm_literal
	push_tos
	pop {r0}
	tst r0, #inlined_flag
	bne 2f
	ldr tos, =_compile
	bl _compile
	pop {pc}
2:	ldr tos, =_asm_inline
	bl _compile
	pop {pc}
	end_inlined

	@@ Compile a literal
	define_word "lit,", visible_flag
_comma_lit:
	push {lr}
	ldr r1, =literal_deferred_q
	ldr r0, [r1]
	cmp r0, #0
	beq 1f
	bl _asm_undefer_lit
1:	ldr r2, =deferred_literal
	str tos, [r2]
	ldr r0, =-1
	str r0, [r1]
	pull_tos
	pop {pc}
	end_inlined

	@@ Compile a literal
	define_word "literal", visible_flag | immediate_flag | compiled_flag
_literal:	
	push {lr}
	bl _comma_lit
	pop {pc}
	end_inlined

	@@ Recursively call a word
	define_word "recurse", visible_flag | immediate_flag | compiled_flag
_recurse:
	push {lr}
	push_tos
	ldr tos, =current_compile
	ldr tos, [tos]
	bl _to_xt
	bl _asm_call
	pop {pc}
	end_inlined
	
	@@ Unknown word exception
	define_word "x-unknown-word", visible_flag
_unknown_word:
	push {lr}
	string_ln "unknown word"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Store a byte
	define_word "c!", visible_flag | inlined_flag
_store_1:
	ldr r0, [dp]
	strb r0, [tos]
	ldr tos, [dp, #4]
	adds dp, #8
	bx lr
	end_inlined

	@@ Store a halfword
	define_word "h!", visible_flag | inlined_flag
_store_2:
	ldr r0, [dp]
	strh r0, [tos]
	ldr tos, [dp, #4]
	adds dp, #8
	bx lr
	end_inlined

	@@ Store a word
	define_word "!", visible_flag | inlined_flag
_store_4:
	ldr r0, [dp]
	str r0, [tos]
	ldr tos, [dp, #4]
	adds dp, #8
	bx lr
	end_inlined

	@@ Store a doubleword
	define_word "2!", visible_flag
_store_8:
	movs r0, tos
	pull_tos
	str tos, [r0]
	pull_tos
	str tos, [r0, #4]
	pull_tos
	bx lr
	end_inlined

	@@ Read a byte from an address, add a value, and write it back
	define_word "c+!", visible_flag | inlined_flag
_add_store_1:
	ldrb r0, [tos]
	ldr r1, [dp]
	adds r0, r1
	strb r0, [tos]
	ldr tos, [dp, #4]
	adds dp, #8
	bx lr
	end_inlined

	@@ Read a halfword from an address, add a value, and write it back
	define_word "h+!", visible_flag | inlined_flag
_add_store_2:	
	ldrh r0, [tos]
	ldr r1, [dp]
	adds r0, r1
	strh r0, [tos]
	ldr tos, [dp, #4]
	adds dp, #8
	bx lr
	end_inlined

	@@ Read a word from an address, add a value, and write it back
	define_word "+!", visible_flag | inlined_flag
_add_store_4:	
	ldr r0, [tos]
	ldr r1, [dp]
	adds r0, r1
	str r0, [tos]
	ldr tos, [dp, #4]
	adds dp, #8
	bx lr
	end_inlined

	@@ Specify a bit
	define_word "bit", visible_flag | inlined_flag
_bit:	movs r0, tos
	movs tos, #1
	lsls tos, r0
	bx lr
	end_inlined

	@@ Bit set a byte
	define_word "cbis!", visible_flag | inlined_flag
_bit_set_1:
	movs r0, tos
	pull_tos
	ldrb r1, [r0]
	orrs r1, tos
	strb r1, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Bit set a halfword
	define_word "hbis!", visible_flag | inlined_flag
_bit_set_2:
	movs r0, tos
	pull_tos
	ldrh r1, [r0]
	orrs r1, tos
	strh r1, [r0]
	pull_tos
	bx lr
	end_inlined
	
	@@ Bit set a word
	define_word "bis!", visible_flag | inlined_flag
_bit_set_4:
	movs r0, tos
	pull_tos
	ldr r1, [r0]
	orrs r1, tos
	str r1, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Bit clear a byte
	define_word "cbic!", visible_flag | inlined_flag
_bit_clear_1:
	movs r0, tos
	pull_tos
	ldrb r1, [r0]
	bics r1, tos
	strb r1, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Bit clear a halfword
	define_word "hbic!", visible_flag | inlined_flag
_bit_clear_2:
	movs r0, tos
	pull_tos
	ldrh r1, [r0]
	bics r1, tos
	strh r1, [r0]
	pull_tos
	bx lr
	end_inlined
	
	@@ Bit clear a word
	define_word "bic!", visible_flag | inlined_flag
_bit_clear_4:
	movs r0, tos
	pull_tos
	ldr r1, [r0]
	bics r1, tos
	str r1, [r0]
	pull_tos
	bx lr
	end_inlined

	@ Test for bits in a byte
	define_word "cbit@", visible_flag | inlined_flag
_bit_test_1:
	movs r0, tos
	pull_tos
	ldrb r0, [r0]
	ands tos, r0
	subs tos, #1
	sbcs tos, tos
	mvns tos, tos
	bx lr
	end_inlined

	@ Test for bits in a halfword
	define_word "hbit@", visible_flag | inlined_flag
_bit_test_2:
	movs r0, tos
	pull_tos
	ldrh r0, [r0]
	ands tos, r0
	subs tos, #1
	sbcs tos, tos
	mvns tos, tos
	bx lr
	end_inlined

	@ Test for bits in a word
	define_word "bit@", visible_flag | inlined_flag
_bit_test_4:
	movs r0, tos
	pull_tos
	ldr r0, [r0]
	ands tos, r0
	subs tos, #1
	sbcs tos, tos
	mvns tos, tos
	bx lr
	end_inlined

	@@ Get a byte
	define_word "c@", visible_flag | inlined_flag
_get_1: ldrb tos, [tos]
	bx lr
	end_inlined

	@@ Get a halfword
	define_word "h@", visible_flag | inlined_flag
_get_2: ldrh tos, [tos]
	bx lr
	end_inlined

	@@ Get a word
	define_word "@", visible_flag | inlined_flag
_get_4: ldr tos, [tos]
	bx lr
	end_inlined

	@@ Get a doubleword
	define_word "2@", visible_flag
_get_8:	ldr r0, [tos]
	ldr tos, [tos, #4]
	push_tos
	movs tos, r0
	bx lr
	end_inlined

	@@ Store a byte at the RAM HERE location
	define_word "cram,", visible_flag
_comma_1:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	ldr r1, [r0]
	movs r2, #0xFF
	ands tos, r2
	strb tos, [r1], #1
	str r1, [r0]
	pull_tos
	pop {pc}
	end_inlined

	@@ Store a halfword at the RAM HERE location
	define_word "hram,", visible_flag
_comma_2:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	ldr r1, [r0]
	ldr r2, =0xFFFF
	ands tos, r2
	strh tos, [r1], #2
	str r1, [r0]
	pull_tos
	pop {pc}
	end_inlined

	@@ Store a word at the RAM HERE location
	define_word "ram,", visible_flag
_comma_4:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	ldr r1, [r0]
	str tos, [r1], #4
	str r1, [r0]
	pull_tos
	pop {pc}
	end_inlined
	
	@@ Store a doubleword at the RAM HERE location
	define_word "2ram,", visible_flag
_comma_8:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	ldr r1, [r0]
	str tos, [r1], #4
	pull_tos
	str tos, [r1], #4
	str r1, [r0]
	pull_tos
	pop {pc}
	end_inlined

	@@ Store a byte at the flash HERE location
	define_word "cflash,", visible_flag
_flash_comma_1:
	push {lr}
	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	bl _store_flash_1
	pop {r0, r1}
	adds r1, #1
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Store a halfword at the flash HERE location
	define_word "hflash,", visible_flag
_flash_comma_2:
	push {lr}
	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	bl _store_flash_2
	pop {r0, r1}
	adds r1, #2
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Store a word at the flash HERE location
	define_word "flash,", visible_flag
_flash_comma_4:
	push {lr}
	ldr r0, =flash_here
	push_tos
	ldr tos, [r0]
	push {r0, tos}
	bl _store_flash_4
	pop {r0, r1}
	adds r1, #4
	str r1, [r0]
	pop {pc}
	end_inlined

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
	end_inlined

	@@ Store a byte to RAM or to flash
	define_word "ccurrent!", visible_flag
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
	end_inlined

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
	end_inlined

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
	end_inlined

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
	end_inlined

	@@ Store a byte to the RAM or flash HERE location
	define_word "c,", visible_flag
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
	end_inlined

	@@ Store a halfword to the RAM or flash HERE location
	define_word "h,", visible_flag
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
	end_inlined

	@@ Store a word to the RAM or flash HERE location
	define_word ",", visible_flag
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
	end_inlined

	@@ Store a doubleword to the RAM or flash HERE location
	define_word "2,", visible_flag
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
	end_inlined

	@@ Reserve a byte at the RAM HERE location
	define_word "cram-reserve", visible_flag
_reserve_1:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #1
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Reserve a halfword at the RAM HERE location
	define_word "hram-reserve", visible_flag
_reserve_2:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #2
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Reserve a word at the RAM HERE location
	define_word "ram-reserve", visible_flag
_reserve_4:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #4
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Reserve a doubleword at the RAM HERE location
	define_word "2ram-reserve", visible_flag
_reserve_8:
	push {lr}
	bl _cpu_offset
	ldr r0, =dict_base
	adds r0, tos
	ldr r0, [r0]
	adds r0, #ram_here_offset
	pull_tos
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #8
	str r1, [r0]
	pop {pc}
	end_inlined

	@@ Reserve a byte at the flash HERE location
	define_word "cflash-reserve", visible_flag
_flash_reserve_1:
	ldr r0, =flash_here
	ldr r1, [r0]
	push_tos
	movs tos, r1
	adds r1, #1
	str r1, [r0]
	bx lr
	end_inlined

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
	end_inlined

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
	end_inlined

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
	end_inlined

	@@ Reserve a byte at the RAM or flash HERE location
	define_word "creserve", visible_flag
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
	end_inlined

	@@ Reserve a halfword at the RAM or flash HERE location
	define_word "hreserve", visible_flag
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
	end_inlined

	@@ Reserve a word at the RAM or flash HERE location
	define_word "reserve", visible_flag
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
	end_inlined

	@@ Reserve a doubleword at the RAM or flash HERE location
	define_word "2reserve", visible_flag
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
	end_inlined

	@@ Align to a power of two
	define_word "align,", visible_flag
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
	end_inlined

	@@ Align to a power of two
	define_word "flash-align,", visible_flag
_flash_comma_align:
	push {lr}
	subs tos, #1
	movs r0, tos
	pull_tos
1:	push {r0}
	bl _flash_here
	pop {r0}
	ands tos, r0
	beq 2f
	movs tos, #0
	push {r0}
	bl _flash_comma_1
	pop {r0}
	b 1b
2:	pull_tos
	pop {pc}
	end_inlined

	@@ Align to a power of two
	define_word "ram-align,", visible_flag
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
2:	pull_tos
	pop {pc}
	end_inlined

	@@ Compile a c-string
	define_word "cstring,", visible_flag
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
	end_inlined

	@@ Push a value onto the return stack
	define_word ">r", visible_flag | inlined_flag
_push_r:
	push {tos}
	pull_tos
	bx lr
	end_inlined

	@@ Pop a value off the return stack
	define_word "r>", visible_flag | inlined_flag
_pop_r:	push_tos
	pop {tos}
	bx lr
	end_inlined

	@@ Get a value off the return stack without popping it
	define_word "r@", visible_flag | inlined_flag
_get_r:	push_tos
	ldr tos, [sp]
	bx lr
	end_inlined

	@@ Drop a value from the return stack
	define_word "rdrop", visible_flag | inlined_flag
_rdrop:	adds sp, #4
	bx lr
	end_inlined

	@@ Push two values onto the return stack
	define_word "2>r", visible_flag | inlined_flag
_push_2r:
	.ifdef cortex_m7
	ldr r0, [dp], #4
        ldr r1, [dp], #4
	.else
	ldmia dp!, {r0, r1}
	.endif
	
	push {tos}
	push {r0}
	movs tos, r1
	bx lr
	end_inlined

	@@ Pop two values off the return stack
	define_word "2r>", visible_flag | inlined_flag
_pop_2r:
	push_tos
	pop {r0, tos}
	stmdb dp!, {r0}
	bx lr
	end_inlined

	@@ Get two values off the return stack without popping it
	define_word "2r@", visible_flag | inlined_flag
_get_2r:
	push_tos
	ldr tos, [sp, #4]
	ldr r0, [sp, #0]
	stmdb dp!, {r0}
	bx lr
	end_inlined
	
	@@ Drop two values from the return stack
	define_word "2rdrop", visible_flag | inlined_flag
_2rdrop:
	adds sp, #8
	bx lr
	end_inlined

	@@ Get the return stack pointer
	define_word "rp@", visible_flag | inlined_flag
_get_rp:
	push_tos
	mov tos, sp
	bx lr
	end_inlined

	@@ Set the return stack pointer
	define_word "rp!", visible_flag | inlined_flag
_store_rp:
	mov sp, tos
	pull_tos
	bx lr
	end_inlined

	@@ Get the data stack pointer
	define_word "sp@", visible_flag | inlined_flag
_get_sp:
	push_tos
	movs tos, dp
	bx lr
	end_inlined

	@@ Set the data stack pointer
	define_word "sp!", visible_flag | inlined_flag
_store_sp:
	movs dp, tos
	pull_tos
	bx lr
	end_inlined

	@@ Get the current compilation wordlist
	define_word "get-current", visible_flag
_get_current:
	ldr r0, =wordlist
	push_tos
	ldr tos, [r0]
	bx lr
	end_inlined

	@@ Set the current compilation wordlist
	define_word "set-current", visible_flag
_set_current:
	ldr r0, =wordlist
	str tos, [r0]
	pull_tos
	bx lr
	end_inlined

	@@ Get the current wordlist order
	define_word "get-order", visible_flag
_get_order:
  	ldr r0, =order
	ldr r1, =order_count
  	ldr r1, [r1]
	lsls r2, r1, #1
	adds r0, r2
3:	cmp r2, #0
	beq 4f
	subs r2, #2
	subs r0, #2
	push_tos
	ldrh tos, [r0]
	b 3b
4:	push_tos
	movs tos, r1
	bx lr
	end_inlined

	@@ Set the current wordlist order
	define_word "set-order", visible_flag
_set_order:
	ldr r0, =order
	ldr r1, =order_count
2:	str tos, [r1]
	movs r1, tos
	pull_tos
3:	cmp r1, #0
	beq 4f
	subs r1, #1
	strh tos, [r0]
	pull_tos
	adds r0, #2
	b 3b
4:	bx lr
	end_inlined

	@@ Context switch ( ctx -- old-ctx )
	define_internal_word "context-switch", visible_flag
_context_switch:
	movs r0, tos
	pull_tos
	push {r4, r5, r6, r7, r8, r9, r10}
	mov r1, sp
	mov sp, r0
	pop {r4, r5, r6, r7, r8, r9, r10}
	push_tos
	movs tos, r1
	bx lr
	end_inlined

	@@ Initialize a context ( ctx dp xt -- ctx )
	@@ This needs to be called by execute called as an SVCall handler.
	define_internal_word "init-context", visible_flag
_init_context:
	movs r0, tos
	@	adds r0, #1
	.ifdef cortex_m7
        ldr r1, [dp], #4
        ldr r2, [dp], #4
	.else
 	ldmia dp!, {r1, r2}
	.endif
	mov r6, sp
	push {r4, r5, r7, r8, r9}
	movs r5, #0
	ldr r3, =0x21000000
	movs r4, r2
	ands r4, #7
	beq 1f
	stmdb r2!, {r5}
	ldr r3, =0x21000200
1:	movs r7, #0
	movs r8, #0
	ldr r9, [r6, #20]
	stmdb r2!, {r0, r3}
	stmdb r2!, {r5, r7, r8, r9}
	stmdb r2!, {r5}
	stmdb r2!, {r5}
	pop {r4, r5, r7, r8, r9}
	movs r0, r7
	movs r7, r1
@	movs r3, r6
	ldr r6, =0xFEDCBA98
	stmdb r2!, {r4, r5, r6, r7, r8, r9, r10}
	movs r7, r0
@	movs r6, r3
	movs tos, r2
	bx lr
	end_inlined

	@@ Reboot (note that this does not clear RAM, but it does clear the RAM
	@@ dictionary
	define_word "reboot", visible_flag
_reboot:
	ldr r0, =0xE000ED0C @ AIRCR
	ldr r1, =0x05FA0004
	str r1, [r0]
	dsb
	isb
	bx lr
	end_inlined

	@@ Null exception handler
	define_word "handle-null", visible_flag
_handle_null:
	bx lr
	end_inlined

	@@ Initialize the variables
	define_internal_word "init-variables", visible_flag
_init_variables:
	push {lr}
	movs r1, #0
	ldr r0, =xon_xoff_enabled
	str r1, [r0]
	ldr r1, =-1
	ldr r0, =ack_nak_enabled
	str r1, [r0]
	ldr r0, =bel_enabled
	str r1, [r0]
	@@ Initialize the data stack base
	ldr r0, =ram_current + stack_base_offset
	ldr r1, =stack_top
	str r1, [r0]
	@@ Initialize the return stack base
	ldr r0, =ram_current + rstack_base_offset
	ldr r1, =rstack_top
	str r1, [r0]
	@@ Initialize the data stack end
	ldr r0, =ram_current + stack_end_offset
	ldr r1, =stack_top - stack_size
	str r1, [r0]
	@@ Initialize the return stack end
	ldr r0, =ram_current + rstack_end_offset
	ldr r1, =rstack_top - rstack_size
	str r1, [r0]
	@@ Initialize BASE
	ldr r0, =ram_current + base_offset
	movs r1, #10
	str r1, [r0]
	ldr r2, =cpu_count * 4
1:	cmp r2, #0
	beq 2f
	subs r2, #4
	ldr r1, =_do_nothing
	ldr r0, =pause_enabled
	str r1, [r0, r2]
	b 1b
2:	ldr r0, =prompt_hook
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
	ldr r1, =_do_nothing
	str r1, [r0]
	ldr r0, =validate_dict_hook
	str r1, [r0]
	ldr r0, =finalize_hook
	str r1, [r0]
	ldr r0, =find_hook
	ldr r1, =_do_find
	str r1, [r0]
	ldr r0, =compiling_to_flash
	movs r1, 0
	str r1, [r0]
	ldr r0, =current_compile
	str r1, [r0]
	ldr r0, =deferred_literal
	str r1, [r0]
	ldr r0, =literal_deferred_q
	str r1, [r0]
	ldr r0, =latest
	str r1, [r0]
	ldr r0, =ram_latest
	str r1, [r0]
	ldr r0, =flash_latest
	str r1, [r0]
	ldr r0, =wordlist
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
	ldr r0, =compress_flash_enabled
	str r1, [r0]
	ldr r0, =order
	strh r1, [r0]
	movs r1, #1
	ldr r0, =order_count
	str r1, [r0]
        bl _prepare_prompt
	pop {pc}
	end_inlined
	
	@@ Initialize the in-RAM vector table
_init_vector_table:
	ldr r0, =vectors
	ldr r1, =vectors + vector_table_size
	ldr r2, =vector_table
1:	cmp r0, r1
	beq 2f
	ldr r3, [r0]
	adds r0, #4
	str r3, [r2]
	adds r2, #4
	b 1b
2:	ldr r0, =VTOR
	ldr r1, =VTOR_value
	str r1, [r0]
	dmb
	dsb
	isb
	bx lr

	.ltorg
	
