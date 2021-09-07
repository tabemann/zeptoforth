@ Copyright (c) 2013 Matthias Koch
@ Copyright (c) 2019-2021 Travis Bemann
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

	@@ Call a ROM routine with zero parameters
	define_word "call-rom-0", visible_flag
_call_rom_0:
	push {lr}
	movs r2, #0
	movs r1, tos
	pull_tos
	ldrh r0, [r2, #0x14]
	ldrh r3, [r2, #0x18]
	blx r3
	blx r0
	pop {pc}
	end_inlined

	@@ Call a ROM routine with three parameters
	define_word "call-rom-3", visible_flag
_call_rom_3:
	push {r4, lr}
	movs r2, #0
	movs r1, tos
	pull_tos
	ldrh r0, [r2, #0x14]
	ldrh r3, [r2, #0x18]
	blx r3
	movs r3, r0
	movs r2, tos
	ldmia dp!, {r1}
	ldmia dp!, {r0}
	ldmia dp!, {r4}
	blx r3
	movs tos, r4
	pop {r4, pc}
	end_inlined

	@@ Call a ROM routine with four parameters
	define_word "call-rom-4", visible_flag
_call_rom_4:
	push {r4, r5, lr}
	movs r2, #0
	movs r1, tos
	pull_tos
	ldrh r0, [r2, #0x14]
	ldrh r3, [r2, #0x18]
	blx r3
	movs r4, r0
	movs r3, tos
	ldmia dp!, {r2}
	ldmia dp!, {r1}
	ldmia dp!, {r0}
	ldmia dp!, {r5}
	blx r4
	movs tos, r5
	pop {r4, r5, pc}
	end_inlined

	@@ Write 256 bytes starting at a 256-bit-aligned address in flash
	define_internal_word "256flash!", visible_flag
_store_flash_256:
	push {lr}
	ldr r0, =flash_min_address
	cmp tos, r0
	bge 1f
	ldr tos, =_attempted_to_write_core_flash
	bl _raise
1:	ldr r0, =flash_dict_end
	cmp tos, r0
	blt 1f
	ldr tos, =_attempted_to_write_past_flash_end
	bl _raise
1:	tst tos, #0xf
	beq 1f
	ldr tos, =_store_flash_256_unaligned
	bl _raise
	ldr r1, #256
2:	subs r1, #4
	cmp r1, #0
	blt 1f
	ldr r0, [tos, r1]
	adds r0, #1
	beq 2b
	ldr tos, =_store_flash_256_already_written
	bl _raise
1:	cpsid i
	bl _connect_qspi
	bl _exit_xip
	ldmia dp!, {r0}
	ldr r2, =flash_base
	subs tos, r2
	push_tos
	movs tos, r0
	push_tos
	ldr tos, =256
	bl _program_flash
	bl _flush_flash_cache
	bl _enter_xip
	cpsie i
	pop {pc}
	end_inlined

	@@ Wait for flash opeartions to complete
wait_for_flash:
1:	ldr r0, =FLASH_SR+2
	ldrh r1, [r0]
	movs r2, #0x01
	ands r1, r2
	bne 1b
	bx lr
	end_inlined
	
	@@ Exception handler for unaligned 256-byte flash writes
	define_internal_word "256flash!-unaligned", visible_flag
_store_flash_256_unaligned:
	push {lr}
	string_ln " unaligned 16-byte flash write"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
	define_word "256flash!-already-written", visible_flag
_store_flash_256_already_written:
	push {lr}
	string_ln " flash already written"
	bl _type
	pop {pc}
	end_inlined
	
	@@ Exception handler for flash writes where flash has already been
	@@ written
_attempted_to_write_core_flash:
	push {lr}
	string_ln " attempted to write to core flash"
	bl _type
	pop {pc}
	end_inlined

	@@ Exception handler for flash writes past the end of flash
_attempted_to_write_past_flash_end:
	push {lr}
	string_ln " attempted to write past flash end"
	bl _type
	pop {pc}
	end_inlined

	@@ Restore the state of the QSPI flash interface
	define_internal_word "connect-qspi", visible_flag
_connect_qspi:
	push {lr}
	push_tos
	ldr tos, ='I | ('F << 8)
	bl _call_rom_0
	pop {pc}
	end_inlined

	@@ Enter XIP mode - this is very slow, but we are copying into RAM so
	@@ it doesn't matter. ( -- )
	define_internal_word "enter-xip", visible_flag
_enter_xip:	
	push {lr}
	push_tos
	ldr tos, ='C | ('X << 8)
	bl _call_rom_0
	pop {pc}
	end_inlined

	@@ Enter serial-mode and then exit XIP - flushing the cache to clear
	@@ forcing the CS pin is needed before turning to XIP ( -- )
	define_internal_word "exit-xip", visible_flag
_enter_xip:
	push {lr}
	push_tos
	ldr tos, ='E | ('X << 8)
	bl _call_rom_0
	pop {pc}
	end_inlined

	@@ Flush and enable the XIP cache and clear forcing on the CS pin ( -- )
	define_internal_word "flush-xip-cache", visible_flag
_flush_xip_cache:
	push {lr}
	push_tos
	ldr tos, ='F | ('C << 8)
	bl _call_rom_0
	pop {pc}
	end_inlined

	@@ Program a range of flash ( addr count size -- )
	@@
	@@ size and count must be a multiple of 256
	define_internal_word "program-flash", visible_flag
_program_flash:
	push {lr}
	push_tos
	ldr tos, ='R | ('P << 8)
	bl _call_rom_4
	pop {pc}
	end_inlined

	@@ Erase a range of flash ( addr count size cmd -- )
	@@
	@@ Use 0xD8 for a block erase command
	@@ size and count must be a multiple of 4096
	define_internal_word "erase-flash", visible_flag
_erase_flash:
	push {lr}
	push_tos
	ldr tos, ='R | ('E << 8)
	bl _call_rom_4
	pop {pc}
	end_inlined
	
	@@ Erase after a given address (including the sector the address is in)
	define_internal_word "erase-after", visible_flag
_erase_after:
	bx lr
	end_inlined
	
	@@ Erase all flash except for the zeptoforth runtime
	define_word "erase-all", visible_flag
_erase_all:
	bx lr
	end_inlined
		
	@@ Find the end of the flash dictionary
	define_internal_word "find-flash-end", visible_flag
_find_flash_end:
	push_tos
	ldr tos, =flash_dict_end
	ldr r1, =flash_dict_start
1:	cmp tos, r1
	beq 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	bne 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	bne 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	bne 2f
	subs tos, #4
	ldr r0, [tos]
	adds r0, #1
	beq 1b
2:	bx lr
	end_inlined

	@@ Find the next flash block
	define_internal_word "next-flash-block", visible_flag
_next_flash_block:
	movs r0, #FF
	bics tos, r0
	ldr r0, =256
	adds tos, r0
	bx lr
	end_inlined

	@@ Find the start of the last flash word
	define_internal_word "find-last-flash-word", visible_flag
_find_last_flash_word:
	ldr r1, =0xDEADBEEF
	ldr r2, =flash_dict_start
1:	subs tos, #4
	cmp tos, r2
	beq 2f
	ldr r0, [tos]
	cmp r0, r1
	bne 1b
3:	subs tos, #4
	cmp tos, r2
	beq 2f
	ldr r0, [tos]
	cmp r0, r1
	beq 3b
	ldr tos, [tos]
	bx lr
2:	ldr tos, =kernel_end
	b 3b
	bx lr
	end_inlined

	@@ Write a byte to an address in a flash buffer
	define_word "cflash!", visible_flag
_store_flash_1:
	push {lr}
	push_tos
	bl _get_flash_buffer
	movs r0, tos
	pull_tos
	movs r2, #0xFF
	ands tos, r2
	ldr r1, [r0, #flash_buffer_space]
	subs r1, #1
	str r1, [r0, #flash_buffer_space]
	movs r2, r0
	adds r0, r0, tos
	pull_tos
	movs r3, #0xFF
	ands tos, r3
	strb tos, [r0]
	cmp r1, #0
	beq 1f
	pull_tos
	pop {pc}
1:	movs tos, r2
	bl _store_flash_buffer
	pop {pc}
	end_inlined
	
	@@ Write a halfword to an address in one or more flash_buffers
	define_word "hflash!", visible_flag
_store_flash_2:
	push {lr}
	movs r0, tos
	pull_tos
	push {r0, tos}
	movs r1, #0xFF
	ands tos, r1
	push_tos
	movs tos, r0
	bl _store_flash_1
	push_tos
	pop {r0, tos}
	lsrs tos, tos, #8
	movs r1, #0xFF
	ands tos, r1
	push_tos
	movs tos, r0
	adds tos, #1
	bl _store_flash_1
	pop {pc}
	end_inlined

	@@ Write a word to an address in one or more flash_buffers
	define_word "flash!", visible_flag
_store_flash_4:
	push {lr}
	movs r0, tos
	pull_tos
	push {r0, tos}
	ldr r1, =0xFFFF
	ands tos, r1
	push_tos
	movs tos, r0
	bl _store_flash_2
	push_tos
	pop {r0, tos}
	lsrs tos, tos, #16
	ldr r1, =0xFFFF
	ands tos, r1
	push_tos
	movs tos, r0
	adds tos, #2
	bl _store_flash_2
	pop {pc}
	end_inlined

	@@ Write a word to an address in one or more flash_buffers
	define_word "2flash!", visible_flag
_store_flash_8:
	push {lr}
	push {tos}
	bl _store_flash_4
	push_tos
	pop {tos}
	adds tos, #4
	bl _store_flash_4
	pop {pc}
	end_inlined

	@@ Write out the current block of flash
	define_internal_word "store-flash-buffer", visible_flag
_store_flash_buffer:
	push {lr}
	push_tos
	ldr tos, [tos, #flash_buffer_addr]
	bl _store_flash_256
	pop {pc}
	end_inlined

	@@ Flush writing the flash
	define_internal_word "flush-flash", visible_flag
_flush_flash:
	push {lr}
	bl _get_flash_buffer
	ldr r0, [tos, #flash_buffer_space]
	ldr r1, =256
	cmp r0, r1
	beq 1f
	cmp r0, #0
	beq 1f
	movs r0, #0
	str r0, [tos, #flash_buffer_space]
	bl _store_flash_buffer
	pop {pc}
1:	pull_tos
	pop {pc}
	end_inlined

	@@ Flush all the buffered flash
	define_internal_word "flush-all-flash", visible_flag
_flush_all_flash:	
	push {lr}
	ldr r0, =flash_buffers_start
	ldr r1, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
1:	cmp r0, r1
	bge 2f
	push_tos
	movs tos, r0
	push {r0, r1}
	bl _flush_flash
	pop {r0, r1}
	ldr r2, =flash_buffer_size
	adds r0, r2
	b 1b
2:	pop {pc}
	end_inlined

	@@ Fill flash until it is aligned to a block
	define_word "flash-align,", visible_flag
_flash_align:
	push {lr}
1:	bl _flash_here
	movs r0, #0xFF
	tst tos, r0
	beq 2f
	movs tos, #0
	bl _flash_comma_1
2:	pull_tos
	pop {pc}
	end_inlined

	@@ Get the flash block size in bytes
	define_word "flash-block-size", visible_flag | inlined_flag
_flash_block_size:
	push_tos
	ldr tos, =flash_block_size
	bx lr
	end_inlined

	@@ Initialize the flash buffers
	define_internal_word "init-flash-buffers", visible_flag
_init_flash_buffers:
	ldr r0, =flash_buffers_start
	movs r1, #0
	ldr r2, =flash_buffers_start + (flash_buffer_size * flash_buffer_count)
1:	str r1, [r0]
	adds r0, #4
	cmp r0, r2
	bne 1b
	bx lr
	end_inlined

	.ltorg
	
